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

        private int expectedPackets;


        public UDPServer(string localIpAddress, int localPort)
        {
            localAddress = IPAddress.Parse(localIpAddress);
            this.localPort = localPort;
            expectedPackets = -1;
            receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }


        public async Task StartReceivingAsync()
        {
            Console.WriteLine("Прием пакетов...");

            receiver.Bind(new IPEndPoint(localAddress, localPort));
            byte[] buffer = new byte[65535];
            int receivedPacket = 0; //!!!!!!

            while (true)
            {
                // Прием данных
                var result = await receiver.ReceiveFromAsync(new ArraySegment<byte>(buffer), SocketFlags.None, new IPEndPoint(IPAddress.Any, 0));
                byte[] receivedData = buffer.Take(result.ReceivedBytes).ToArray();

                if (expectedPackets == -1)
                {
                    expectedPackets = BitConverter.ToInt32(receivedData);
                    Console.WriteLine($"Ожидается {expectedPackets} пакетов.");
                }
                else
                {
                    UdpPacket packet = UdpPacket.FromBytes(receivedData);
                    Console.WriteLine($"Получен пакет {packet.PacketId} размером {packet.ToBytes().Length} байт");
                    receivedPacket++;

                    if (receivedPacket == expectedPackets)
                    { 
                        expectedPackets = -1;
                        receivedPacket = 0;
                    }
                }

            }
        }
    }
}