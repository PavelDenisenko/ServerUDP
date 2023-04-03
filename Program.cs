using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerUDP
{
    class Program
    {
        static void Main(string[] args)
        {
            Config.CreateConfigFileIfNotExist();
            Config cfg = Config.DeserializeFromConfigFile();

            Console.WriteLine("Press s or c for run SERVER or CLIENT mode.");
            ConsoleKeyInfo ki = Console.ReadKey();
            

            if (ki.Key == ConsoleKey.S) // server
            {

                UDPServer server = new UDPServer();
                server.Server(cfg.BroadcastIp, cfg.BroadcastPort);
                Random rnd = new Random();
                Tick tick;
                ulong lastTickNumber = 0;
                Stopwatch sw = new Stopwatch();
                double avRateMs = 0;
                do
                {
                    sw.Restart();
                    tick.Number = lastTickNumber++;
                    tick.TimeStamp = DateTime.Now;
                    tick.Price = (decimal)(rnd.Next(cfg.MinRndValue, cfg.MaxRndValue) * cfg.Point);
                    server.Send(tick);
                    sw.Stop();
                    avRateMs = (avRateMs * (lastTickNumber-1) + sw.Elapsed.TotalMilliseconds) / lastTickNumber;
                    Console.WriteLine($"Sending rate in ms:{Math.Round(avRateMs,6)}");

                } while (true);
            }
            else if (ki.Key == ConsoleKey.C) // client
            {
                Client client = new Client(cfg.BroadcastIp, cfg.BroadcastPort, cfg.ClientDelayms, (cfg.MaxRndValue- cfg.MinRndValue) * cfg.Point);
                client.Start();

                Console.WriteLine("CLIENT mode. press ENTER to view statistics");
                ConsoleKeyInfo kinf;
                do
                {
                    kinf = Console.ReadKey();
                    if (kinf.Key == ConsoleKey.Enter)
                    {
                        Tuple<double, double, double, double, double, double> stat =   client.GetStat();
                        Console.WriteLine($"av:{Math.Round(stat.Item1, 5)} sd:{Math.Round(stat.Item2, 5)} median:{Math.Round(stat.Item3, 5)} mode:{Math.Round(stat.Item4, 5)} countms:{Math.Round(stat.Item5, 5)} lost:{Math.Round(stat.Item6,5)}%");

                    }

                } while (kinf.Key != ConsoleKey.Escape);
            }

        }
    }
}
