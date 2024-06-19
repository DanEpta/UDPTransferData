using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UdpProgramStatistic
{
    public class CustomUdpServer : UdpClient
    {
        private IPEndPoint listenEndPoint;
        private readonly Dictionary<uint, UdpPacket> receivedPackets = new Dictionary<uint, UdpPacket>();

        private const string PacketCountId = "PACKET_COUNT";
        private const string ConfirmationId = "CONFIRMATION";
        private const string LostPacketsId = "LOST_PACKETS";


        public CustomUdpServer(int port) : base(port)
        {
            listenEndPoint = new IPEndPoint(IPAddress.Any, port);
        }

        public void StartReceiving()
        {
            Console.WriteLine("Начало приема пакетов...");
            byte[] buffer = new byte[65535];

            while (true)
            {
                EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                int receivedBytes = this.Client.ReceiveFrom(buffer, ref remoteEndPoint);

                byte[] receivedData = new byte[receivedBytes];
                Array.Copy(buffer, receivedData, receivedBytes);

                DataProcessing(receivedData);
            }
        }

        private void DataProcessing(byte[] receivedData)
        {
            if (IsPacketCountMessage(receivedData))
            {
                uint expectedPackets = BitConverter.ToUInt32(receivedData, PacketCountId.Length);
                Console.WriteLine($"Ожидается {expectedPackets} пакетов.");
            }
            else
            {
                UdpPacket packet = UdpPacket.FromBytes(receivedData);
                Console.WriteLine($"Получен пакет {packet.PacketId} размером {packet.ToBytes().Length} байт");
            }
        }


        private void ProcessReceivedData(byte[] data)
        {
            if (data.Length == 4)
            {
                int totalPackets = BitConverter.ToInt32(data, 0);
                Console.WriteLine($"Ожидается получение {totalPackets} пакетов");
            }
            else
            {
                UdpPacket packet = UdpPacket.FromBytes(data);
                receivedPackets[packet.PacketId] = packet;
                Console.WriteLine($"Получен пакет {packet.PacketId} размером {packet.Data.Length} байт");

                // Дополнительно можно реализовать логику обработки полученных пакетов
                // Например, отправку подтверждения обратно клиенту
            }
        }

        private bool IsPacketCountMessage(byte[] data)
        {
            string id = Encoding.UTF8.GetString(data, 0, PacketCountId.Length);
            return id == PacketCountId;
        }
    }
}