using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace ProxyGAC {
    class Misc {
        public static string ConsoleTitle = "ProxyGAC v1.0 by bbangg";

        public enum Color {
            White,
            Red,
            Green,
            Blue,
            Yellow,
        }

        public static void ChangeColor(Color color) {
            switch (color) {
                case Color.White:
                Console.ForegroundColor = ConsoleColor.White;
                break;
                case Color.Red:
                Console.ForegroundColor = ConsoleColor.Red;
                break;
                case Color.Green:
                Console.ForegroundColor = ConsoleColor.Green;
                break;
                case Color.Blue:
                Console.ForegroundColor = ConsoleColor.Blue;
                break;
                case Color.Yellow:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            }
        }

    }

    class Program {
        public static List<string> LProxies = new List<string>();
        public static List<string> WProxies = new List<string>();
        public static List<string> DProxies = new List<string>();

        public static void CurrentDomain_ProcessExit(object sender, EventArgs e) {
            string date = DateTime.Now.ToLongDateString();

            if (!Directory.Exists("Logs")) {
                Directory.CreateDirectory("Logs");
                goto SaveProxies;
            }

            SaveProxies:
            if (!Directory.Exists($"Logs\\{date}")) {
                Directory.CreateDirectory($"Logs\\{date}");
            }
            File.WriteAllLines($"Logs\\{date}\\Working.txt", WProxies);
            File.WriteAllLines($"Logs\\{date}\\Proxies.txt", LProxies);
        }

        static void ProxyGAC() {
            Console.Title = Misc.ConsoleTitle;
            Misc.ChangeColor(Misc.Color.Blue);
            Console.WriteLine(" ");
            Console.WriteLine("              ██████╗ ██████╗  ██████╗ ██╗  ██╗██╗   ██╗ ██████╗  █████╗  ██████╗");
            Console.WriteLine("              ██╔══██╗██╔══██╗██╔═══██╗╚██╗██╔╝╚██╗ ██╔╝██╔════╝ ██╔══██╗██╔════╝");
            Console.WriteLine("              ██████╔╝██████╔╝██║   ██║ ╚███╔╝  ╚████╔╝ ██║  ███╗███████║██║");
            Console.WriteLine("              ██╔═══╝ ██╔══██╗██║   ██║ ██╔██╗   ╚██╔╝  ██║   ██║██╔══██║██║");
            Console.WriteLine("              ██║     ██║  ██║╚██████╔╝██╔╝ ██╗   ██║   ╚██████╔╝██║  ██║╚██████╗");
            Console.WriteLine("              ╚═╝     ╚═╝  ╚═╝ ╚═════╝ ╚═╝  ╚═╝   ╚═╝    ╚═════╝ ╚═╝  ╚═╝ ╚═════╝");
            Console.WriteLine(" ");
            Misc.ChangeColor(Misc.Color.White);
        }

        static string[] GrabProxies(string type) {
            Misc.ChangeColor(Misc.Color.Yellow);
            Console.WriteLine($" [!] Grabbing {type.ToUpper()} Proxies (from proxyscrape.com)");
            return new WebClient().DownloadString($"https://api.proxyscrape.com/v2/?request=getproxies&protocol={type.ToLower()}&timeout=10000&country=all&ssl=all&anonymity=all").Split('\n');
        }

        static void Prepare(string path) {
            string[] pl;
            switch (path) {
                case "HTTP":
                pl = GrabProxies("HTTP");
                break;
                case "SOCKS4":
                pl = GrabProxies("SOCKS4");
                break;
                case "SOCKS5":
                pl = GrabProxies("SOCKS5");
                break;
                default:
                pl = File.ReadAllLines(path);
                break;
            }
            foreach (string line in pl) {
                line.Replace('\r','.');
                if(line.Length > 3) {
                    var found = line.Split(':');
                    var proxy = found[0].ToString();
                    var port = found[1].ToString();
                    LProxies.Add(proxy + ":" + port);
                }
            }
            Console.Title = $"{Misc.ConsoleTitle} | Loaded: {LProxies.Count.ToString()} proxies";
            Console.Clear();
            ProxyGAC();
        }

        static void Main(string[] args) {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);

            ProxyGAC();
            if (string.IsNullOrEmpty(args.ToString()) || args.Length <= 0) {
                Misc.ChangeColor(Misc.Color.Blue);
                Console.WriteLine(" [!] Please Choose Method:");
                Misc.ChangeColor(Misc.Color.Green);
                Console.WriteLine("     [1] HTTP Proxies (from proxyscrape)");
                Console.WriteLine("     [2] SOCKS4 Proxies (from proxyscrape)");
                Console.WriteLine("     [3] SOCKS5 Proxies (from proxyscrape)");
                Console.WriteLine("     [4] Advanced Mode (from file)");
                Misc.ChangeColor(Misc.Color.White);
                
                string method = Console.ReadLine();
                
                switch (method) {
                    case "1":
                    Prepare("HTTP");
                    break;
                    case "2":
                    Prepare("SOCKS4");
                    break;
                    case "3":
                    Prepare("SOCKS5");
                    break;
                    default:
                    Misc.ChangeColor(Misc.Color.Blue);
                    Console.WriteLine(" [!] Please drag your proxy list onto the exe!");
                    string filePath = Console.ReadLine();
                    Prepare(filePath.Replace("\"",""));
                    break;
                }
            } else {
                Prepare(args[0]);
            }

            Misc.ChangeColor(Misc.Color.Red);
            Console.WriteLine(" [!] Press ENTER at any time to save progress and exit program!\n");

            Task.Run(() => CheckProxies());

            Console.Read();

        }

        static void CheckProxies() {
            for (int i = 0; i < LProxies.Count; i++) {
                var proxy = LProxies[i].Replace("\r\n", "").Replace("\r", "").Replace("\n", "");

                int amountchecked = 0;
                var ip = proxy.Split(':')[0];
                var port = proxy.Split(':')[1];
                var ping = new Ping();
                var reply = ping.Send(ip);

                if (reply.Status == IPStatus.Success) {
                    amountchecked++;
                    Misc.ChangeColor(Misc.Color.Green);
                    Console.WriteLine($" [+] {ip}:{port} | ({reply.RoundtripTime}ms)");
                    WProxies.Add(proxy);
                    Console.Title = $"{Misc.ConsoleTitle} | Total: {LProxies.Count.ToString()} proxies | Working: {WProxies.Count} | Dead: {DProxies.Count}";
                } else {
                    amountchecked++;
                    Misc.ChangeColor(Misc.Color.Red);
                    Console.WriteLine($" [-] {ip}:{port} | No response!");
                    DProxies.Add(proxy);
                    Console.Title = $"{Misc.ConsoleTitle} | Total: {LProxies.Count.ToString()} proxies | Working: {WProxies.Count} | Dead: {DProxies.Count}";
                }
            }
        }
    }
}
