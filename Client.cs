using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerUDP
{
    public class Client
    {
        UdpClient receiver = null;
        
        int delay;
        ulong lostTicks = 0;
        int port;
        string address;
        double confinterval; 
        
        volatile float median, mode, disp, sd, av, summ, summ2, count, countms;
        
        ConcurrentQueue<decimal> queue = new ConcurrentQueue<decimal>();
        const int numstepfreq = 100;
        ulong[] freq = new ulong[numstepfreq];
        /// <summary>
        /// (confinterval = (maxintv-minintv)*point for freq count)
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="delay"></param>
        /// <param name="confinterval"></param>
        public Client(string address, int port, int delay, double confinterval)
        {
            this.address = address;
            this.port = port;
            this.delay = delay;
            this.confinterval = confinterval;
            receiver = new UdpClient(port);
            receiver.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            receiver.Client.ReceiveBufferSize = Marshal.SizeOf(typeof(Tick)) * 8;
            receiver.Client.ReceiveTimeout = 1000;
            receiver.JoinMulticastGroup(IPAddress.Parse(address), 20);
           
        }
        public void Start()
        {
            Thread thReceiving = new Thread(ReceiveMessage);
            thReceiving.Start();
            Thread thCalc = new Thread(CountStat);
            thCalc.Start();
        }

        void Reconnect()
        {
            receiver = new UdpClient(port);// multicast
            //receiver = new UdpClient(new IPEndPoint(IPAddress.Parse( this.address), this.port));// for unicast
            receiver.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            receiver.Client.ReceiveBufferSize = Marshal.SizeOf(typeof(Tick)) * 8; // its possible to optim 
            receiver.Client.ReceiveTimeout = 1000;
            receiver.JoinMulticastGroup(IPAddress.Parse(address), 20);
            Thread thReceiving = new Thread(ReceiveMessage);
            thReceiving.Start();
        }

        /// <summary>
        /// Returns Stat av, sd, median, mode, countms, lostticksPrc
        /// </summary>
        /// <returns></returns>
        public Tuple<double, double, double, double, double, double> GetStat()
        {
            return new Tuple<double, double, double, double, double, double>( this.av, this.sd, this.median, this.mode, this.countms, 1.0*lostTicks/(lostTicks+count)*100 );
        }

        private void ReceiveMessage()
        {
        
            IPEndPoint remoteIp = null;
            Tick t = new Tick();
            ulong lastTickNum = 0;
            
            try
            {
                while (true)
                {
                    byte[] data = receiver.Receive(ref remoteIp); // получаем данные
                    Tick.GetStruct(data, ref t);
                    if (lastTickNum != 0) lostTicks += t.Number - lastTickNum - 1; 
                    lastTickNum = t.Number;
                    queue.Enqueue(t.Price);
                    Thread.Sleep(delay);
                }
            }
            catch (SocketException soex) { Console.WriteLine(soex.Message); }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                receiver.Close();
                Reconnect();
            }
        }

        void CountStat()
        {
            decimal newvalueQ;
            Stopwatch sw = new Stopwatch();
            while (true)
            {
                try
                {
                    sw.Restart();
                    if (!queue.TryDequeue(out newvalueQ)) continue;
                    float newvalue = (float)newvalueQ;

                    // add to freq
                    freq[(int)Math.Floor(newvalue / confinterval * numstepfreq)]++;
                    //mode
                    int ind = Array.IndexOf(freq, freq.Max());
                    mode = (float)(1.0 * ind / numstepfreq * confinterval);

                    // median
                    summ += newvalue;
                    summ2 += newvalue * newvalue;
                    count++;
                    float delta = summ / count / count;
                    if (newvalue < median) median -= delta;
                    else median += delta;

                    // av
                    av = summ / count;
                    //disp sd
                    disp = summ2 / count - av * av;
                    sd =(float) Math.Sqrt(disp);
                    sw.Stop();
                    countms = (float) (countms *(count-1) + sw.Elapsed.TotalMilliseconds)/count;
                }
                catch (Exception ex) { Console.WriteLine($"ERROR: {ex.Message}"); }
            }

        }
    }
}
