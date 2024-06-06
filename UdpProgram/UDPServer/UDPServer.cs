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
        private readonly UInt16 SizeSegment = 1400;
        private int receivedPackets = 0;

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
                var result = await receiver.ReceiveFromAsync(new ArraySegment<byte>(buffer), SocketFlags.None, new IPEndPoint(IPAddress.Any, 0));
                UdpPacket packet = UdpPacket.FromBytes(buffer.Take(result.ReceivedBytes).ToArray());

                List<UdpSegment> segments = new List<UdpSegment>();
                int offset = 0;

                while (offset < packet.Data.Length)
                {
                    int segmentLength = Math.Min(SizeSegment + 1, packet.Data.Length - offset);
                    byte[] segmentBytes = new byte[segmentLength];
                    Buffer.BlockCopy(packet.Data, offset, segmentBytes, 0, segmentLength);
                    UdpSegment segment = UdpSegment.FromBytes(segmentBytes);
                    segments.Add(segment);
                    offset += segmentLength;
                }

                Console.WriteLine($"Получен пакет {packet.PacketId} размером {packet.ToBytes().Length} байт, с {segments.Count} сегментами");
                receivedPackets++;
            }
        }
    }
}