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

        public CustomUdpServer(int port) : base(port)
        {
            listenEndPoint = new IPEndPoint(IPAddress.Any, port);
        }

        public void StartListening()
        {
            try
            {
                Console.WriteLine($"Сервер слушает на порту {listenEndPoint.Port}...");

                while (true)
                {
                    byte[] receivedData = this.Receive(ref listenEndPoint);
                    ProcessReceivedData(receivedData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при приёме данных: {ex.Message}");
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
    }
}