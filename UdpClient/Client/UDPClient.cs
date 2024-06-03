using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class UDPClient
{
    private readonly IPAddress remoteAddress;
    private readonly Socket sender;
    private readonly int remotePort;
    private readonly Dictionary<int, byte[]> sentPacketBuffer = new Dictionary<int, byte[]>();


    private int sentPackets = 0; // id пакета
    private readonly int packetSize = 1024; // размер пакета
    private readonly int maxLength = 4; // в байтах

    public UDPClient(string ipAddress, int port)
    {
        remoteAddress = IPAddress.Parse(ipAddress);
        remotePort = port;
        sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    }

    public async Task StartSendingAsync()
    {
        Console.WriteLine("Отправка пакетов...");

        while (true)
        {
            byte[] data = GeneratePacket();
            await sender.SendToAsync(data, SocketFlags.None, new IPEndPoint(remoteAddress, remotePort));
            sentPackets++;
        }
    }

    private byte[] GeneratePacket()
    {
        byte[] data = new byte[packetSize];
        byte[] packetNumber = BitConverter.GetBytes(sentPackets); // порядковый номер пакета

        // !!!!!!!!возможно похожее надо сделать на Сервере!!!!!!!!
        if (packetNumber.Length > maxLength)
            sentPackets = 0;

        new Random().NextBytes(data); // генерируем случайные данные
        Buffer.BlockCopy(packetNumber, 0, data, 0, packetNumber.Length);

        return data;
    }

    public async Task ReceiveLostPacketNotificationAsync()
    {
        byte[] data = new byte[4]; // буфер для получения номера потерянного пакета
        using Socket receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        receiver.Bind(new IPEndPoint(IPAddress.Any, remotePort + 1)); // используем другой порт для получения уведомлений

        while (true)
        {
            var result = await receiver.ReceiveFromAsync(data, new IPEndPoint(IPAddress.Any, 0));
            int lostPacketNumber = BitConverter.ToInt32(data, 0); // извлекаем номер потерянного пакета

            // отправляем потерянный пакет повторно
            if (sentPacketBuffer.TryGetValue(lostPacketNumber, out byte[] lostPacket))
                await sender.SendToAsync(lostPacket, SocketFlags.None, new IPEndPoint(remoteAddress, remotePort));
        }
    }
}