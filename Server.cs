using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerUDP
{
    public class UDPServer
    {
        UdpClient sender;
        IPEndPoint endPoint;
        public void Server(string address, int port)
        {
             sender = new UdpClient(); // создаем UdpClient для отправки
             sender.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
             IPAddress remoteAddress = IPAddress.Parse(address);
             endPoint = new IPEndPoint(remoteAddress, port);
        }

        public void Send(Tick tick)
        {
            byte[] data = tick.GetBytes();
            sender.Send(data, data.Length, endPoint);
        }
        
    }
}
