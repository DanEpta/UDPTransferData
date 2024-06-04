using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;


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

        private List<int> idLossPackets = new List<int>();

        public UDPServer(string ipAddressRemote, string ipAddressSend, int port)
        {
            remoteAddress = IPAddress.Parse(ipAddressRemote);
            sendAddress = IPAddress.Parse(ipAddressSend);
            remotePort = port;
            sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        // прием сообщений
        public async Task ReceiveMessageAsync()
        {
            byte[] data = new byte[65535]; // буфер для получаемых данных
            // запускаем получение сообщений по адресу remoteAddress:remotePort
            receiver.Bind(new IPEndPoint(remoteAddress, remotePort));

            sender.Bind(new IPEndPoint(sendAddress, remotePort + 1));

            while (true)
            {
                // получаем данные в массив data
                var result = await receiver.ReceiveFromAsync(data, new IPEndPoint(IPAddress.Any, 0));
                int packetNumber = BitConverter.ToInt32(data, 0); // извлекаем номер пакета
                receivedPackets++;

                while (packetNumber > lastPacketNumber + 1)
                {
                    int lostPacketNumber = lastPacketNumber + 1;
                    idLossPackets.Add(lostPacketNumber); // udalit potom
                    lostPackets++; // считаем количество потерянных пакетов
                    lastPacketNumber++;

                    // Отправка номера потерянного пакета клиенту
                    byte[] lostPacketData = BitConverter.GetBytes(lostPacketNumber);
                    await sender.SendToAsync(lostPacketData, new IPEndPoint(sendAddress, remotePort + 1));
                }

                lastPacketNumber = packetNumber;
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

                foreach (var id in idLossPackets)
                    Console.Write($"{id}  ");

                await Task.Delay(5000);
            }
        }
    }
}
