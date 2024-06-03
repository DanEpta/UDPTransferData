using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace UdpServer.Server
{
    internal class UDPServer
    {
        private readonly IPAddress remoteAddress;
        private readonly Socket sender;
        private readonly int remotePort;
        private readonly int packetSize = 1024; // размер пакета в байтах

        // статистика
        private int sentPackets = 0;
        private int receivedPackets = 0;
        private int lostPackets = 0;
        private int lastPacketNumber = -1;

        public UDPServer(string ipAddress, int port)
        {
            remoteAddress = IPAddress.Parse(ipAddress);
            remotePort = port;
            sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        // прием сообщений
        public async Task ReceiveMessageAsync()
        {
            byte[] data = new byte[65535]; // буфер для получаемых данных
            // сокет для прослушки сообщений
            using Socket receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            // запускаем получение сообщений по адресу remoteAddress:remotePort
            receiver.Bind(new IPEndPoint(remoteAddress, remotePort));

            while (true)
            {
                // получаем данные в массив data
                var result = await receiver.ReceiveFromAsync(data, new IPEndPoint(IPAddress.Any, 0));
                int packetNumber = BitConverter.ToInt32(data, 0); // извлекаем номер пакета
                receivedPackets++;

                if (packetNumber > lastPacketNumber + 1)
                {
                    int lostPacketNumber = lastPacketNumber + 1;
                    lostPackets += packetNumber - lastPacketNumber - 1; // считаем количество потерянных пакетов

                    // Отправка номера потерянного пакета клиенту
                    byte[] lostPacketData = BitConverter.GetBytes(lostPacketNumber);
                    await sender.SendToAsync(lostPacketData, SocketFlags.None, result.RemoteEndPoint);
                }

                lastPacketNumber = packetNumber;
            }
        }

        // подсчет потерь пакетов и пропускной способности
        public async Task UpdateStatisticsAsync()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Пакеты получены: {receivedPackets}");
                Console.WriteLine($"Пакеты потеряны: {lostPackets}");
                Console.WriteLine($"Потери пакетов: {(lostPackets / (double)(receivedPackets + lostPackets)) * 100:F2}%");

                await Task.Delay(5000);
            }
        }
    }
}
