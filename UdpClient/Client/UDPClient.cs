using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


public class UDPClient
{
    private readonly IPAddress sendAddress;
    private readonly IPAddress reciveAddress;
    private readonly Socket sender;
    private readonly Socket receiver;
    private readonly int remotePort;
    private readonly Dictionary<int, byte[]> sentPacketBuffer = new Dictionary<int, byte[]>();


    private int sentPackets = 0; // id пакета
    private int lostPacketsCount = 0; // счетчик потерянных пакетов
    private readonly int packetSize = 1024; // размер пакета
    private readonly int maxLength = 4; // в байтах

    public UDPClient(string ipAddress, string ipAddressRecive, int port)
    {
        sendAddress = IPAddress.Parse(ipAddress);
        reciveAddress = IPAddress.Parse(ipAddressRecive);
        remotePort = port;
        sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        sender.Bind(new IPEndPoint(sendAddress, remotePort));
        receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        receiver.Bind(new IPEndPoint(reciveAddress, remotePort + 1));
    }

    public async Task StartSendingAsync()
    {
        Console.WriteLine("Отправка пакетов...");

        for (int  i = 0; i < 500; i++)
        //while (true)
        {
            byte[] data = GeneratePacket();
            await sender.SendToAsync(data, SocketFlags.None, new IPEndPoint(sendAddress, remotePort));
            sentPackets++;
        }
    }


    public async Task ReceiveLostPacketNotificationAsync()
    {
        byte[] data = new byte[4]; // буфер для получения номера потерянного пакета

        while (true)
        {
            var result = await receiver.ReceiveFromAsync(data, new IPEndPoint(IPAddress.Any, 0));
            int lostPacketNumber = BitConverter.ToInt32(data, 0); // извлекаем номер потерянного пакета  
            
            // отправляем потерянный пакет повторно
            if (sentPacketBuffer.TryGetValue(lostPacketNumber, out byte[] lostPacket))
                lostPacketsCount++;
                //await sender.SendToAsync(lostPacket, SocketFlags.None, new IPEndPoint(remoteAddress, remotePort));                       
        }
    }

    public async Task DisplayStatisticsAsync()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"Пакеты отправлены: {sentPackets}");
            Console.WriteLine($"Потерянные пакеты (с повторной отправкой): {lostPacketsCount}");
            await Task.Delay(5000);
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
}