using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace UdpServer.Server
{
    internal class UDPServer
    {
        private readonly IPAddress remoteAddress;
        private readonly IPAddress sendAddress;
        private readonly int remotePort;
        private readonly int packetSize = 1024; // размер пакета в байтах
        private Socket sender;
        private Socket receiver;

        // статистика
        private int receivedPackets = 0;
        private int lostPackets = 0;
        private int lastPacketNumber = -1;

        private ConcurrentQueue<int> packetQueue = new ConcurrentQueue<int>();
        private List<int> idLossPackets = new List<int>();


        public UDPServer(string ipAddressRemote, string ipAddressSend, int port)
        {
            remoteAddress = IPAddress.Parse(ipAddressRemote);
            sendAddress = IPAddress.Parse(ipAddressSend);
            remotePort = port;
            sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        public async Task ReceiveMessageAsync()
        {
            byte[] data = new byte[packetSize]; // буфер для получаемых данных
            receiver.Bind(new IPEndPoint(remoteAddress, remotePort));

            while (true)
            {
                var result = await receiver.ReceiveFromAsync(data, new IPEndPoint(IPAddress.Any, 0));
                int packetNumber = BitConverter.ToInt32(data, 0); // извлекаем номер пакета
                packetQueue.Enqueue(packetNumber);
            }
        }

        // проверка потерянных пакетов
        public async Task CheckLostPacketsAsync()
        {
            while (true)
            {
                while (packetQueue.TryDequeue(out int packetNumber))
                {
                    receivedPackets++;

                    // Проверка на потерянные пакеты
                    if (packetNumber > lastPacketNumber + 1)
                    {
                        for (int lostPacketNumber = lastPacketNumber + 1; lostPacketNumber < packetNumber; lostPacketNumber++)
                        {
                            idLossPackets.Add(lostPacketNumber); // добавляем в список потерянных пакетов
                            lostPackets++; // считаем количество потерянных пакетов

                            // Отправка номера потерянного пакета клиенту
                            byte[] lostPacketData = BitConverter.GetBytes(lostPacketNumber);
                            await sender.SendToAsync(new ArraySegment<byte>(lostPacketData), SocketFlags.None, new IPEndPoint(sendAddress, remotePort + 1));
                        }
                    }

                    lastPacketNumber = packetNumber;
                }

                await Task.Delay(100); // Задержка для предотвращения чрезмерного использования ресурсов
            }
        }

        // подсчет потерь пакетов
        public async Task UpdateStatisticsAsync()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Пакеты получены: {receivedPackets}");
                Console.WriteLine($"Пакеты потеряны: {lostPackets}");
                Console.WriteLine($"Потери пакетов: {(lostPackets / (double)(receivedPackets + lostPackets)) * 100:F2}%");

                /*
                foreach(var id in idLossPackets)
                    Console.Write($"{id}  ");
                */

                await Task.Delay(5000);
            }
        }
    }
}