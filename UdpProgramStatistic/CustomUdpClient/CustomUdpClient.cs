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

        private const string PacketCountId = "PACKET_COUNT";
        private const string ConfirmationId = "CONFIRMATION";
        private const string LostPacketsId = "LOST_PACKETS";


        public CustomUdpClient(string serverAddress, int serverPort) : base()
        {
            IPAddress remoteIPAddress = IPAddress.Parse(serverAddress);
            serverEndPoint = new IPEndPoint(remoteIPAddress, serverPort);
        }


        public void StartSending(byte[] data) =>
            PacketSending(data);

        private void PacketSending(byte[] data)
        {
            List<UdpPacket> packets = UdpSeparationData.SeparationDataToPacket(data);

            int totalPackets = packets.Count;
            SendPacketCount(totalPackets);

            foreach (var packet in packets)
                SendPacket(packet);

            // Реализация ожидания подтверждения или повторной отправки, если необходимо
            // WaitForConfirmationOrResend();
        }

        private void SendPacketCount(int totalPackets)
        {
            byte[] idBytes = Encoding.UTF8.GetBytes(PacketCountId);
            byte[] totalPacketsBytes = BitConverter.GetBytes(totalPackets);
            byte[] message = new byte[idBytes.Length + totalPacketsBytes.Length];

            Buffer.BlockCopy(idBytes, 0, message, 0, idBytes.Length);
            Buffer.BlockCopy(totalPacketsBytes, 0, message, idBytes.Length, totalPacketsBytes.Length);

            this.Send(message, message.Length, serverEndPoint);
            Console.WriteLine($"Отправлено количество пакетов: {totalPackets}");
        }

        private void SendPacket(UdpPacket packet)
        {
            byte[] packetBytes = packet.ToBytes();
            this.Send(packetBytes, packetBytes.Length, serverEndPoint);
            Console.WriteLine($"Отправлен пакет {packet.PacketId} размером в {packet.Data.Length} байт");
        }
    }
}