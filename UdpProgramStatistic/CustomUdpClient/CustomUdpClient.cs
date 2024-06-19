using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace UdpProgramStatistic
{
    public class CustomUdpClient : UdpClient
    {
        private readonly IPEndPoint serverEndPoint;


        public CustomUdpClient(string serverAddress, int serverPort) : base()
        {
            IPAddress remoteIPAddress = IPAddress.Parse(serverAddress);
            serverEndPoint = new IPEndPoint(remoteIPAddress, serverPort);
        }


        public void StartSending(byte[] data)
        {
            PacketSending(data);
        }

        private void PacketSending(byte[] data)
        {
            List<UdpPacket> packets = UdpSeparationData.SeparationDataToPacket(data);

            int totalPackets = packets.Count;
            SendPacketCount(totalPackets);

            Console.WriteLine($"Количество пакетов: {totalPackets}");

            foreach (var packet in packets)
            {
                SendPacket(packet);
            }

            // Реализация ожидания подтверждения или повторной отправки, если необходимо
            // WaitForConfirmationOrResend();
        }

        private void SendPacket(UdpPacket packet)
        {
            byte[] packetBytes = packet.ToBytes();
            Send(packetBytes, packetBytes.Length, serverEndPoint);
            Console.WriteLine($"Отправлен пакет {packet.PacketId} размером в {packet.Data.Length} байт");
        }

        private void SendPacketCount(int totalPackets)
        {
            byte[] packetCountBytes = BitConverter.GetBytes(totalPackets);
            Send(packetCountBytes, packetCountBytes.Length, serverEndPoint);
            Console.WriteLine($"Отправлено количество пакетов: {totalPackets}");
        }
    }
}