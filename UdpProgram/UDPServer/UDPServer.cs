using System;
using System.Net;
using System.Net.Sockets;


namespace UdpProgram.Udp
{
    public class UDPServer
    {
        private Socket receiver;
        private readonly IPAddress localAddress;
        private readonly int localPort;


        public UDPServer(string localIpAddress, int localPort)
        {
            localAddress = IPAddress.Parse(localIpAddress);
            this.localPort = localPort;
            receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }


        public async Task StartReceivingAsync()
        {
            Console.WriteLine("Прием пакетов...");

            receiver.Bind(new IPEndPoint(localAddress, localPort));
            byte[] buffer = new byte[65535];

            while (true)
            {
                // Прием данных
                var result = await receiver.ReceiveFromAsync(new ArraySegment<byte>(buffer), SocketFlags.None, new IPEndPoint(IPAddress.Any, 0));
                UdpPacket packet = UdpPacket.FromBytes(buffer.Take(result.ReceivedBytes).ToArray());

                Console.WriteLine($"Получен пакет {packet.PacketId} размером {packet.ToBytes().Length} байт");              
            }
        }
    }
}