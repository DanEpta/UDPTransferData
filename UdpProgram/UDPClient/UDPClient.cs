using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UdpProgram.Udp;

public class UDPClient
{
    private Socket sender;
    private readonly IPAddress serverAddress;
    private readonly int serverPort;
    private int sequenceNumber = 0;
    private Random random;


    public UDPClient(string serverIpAddress, int serverPort)
    {
        serverAddress = IPAddress.Parse(serverIpAddress);
        this.serverPort = serverPort;
        sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        random = new Random();
    }


    public async Task StartSendingAsync()
    {
        Console.WriteLine("Отправка пакетов...");

        while (true)
        {
            byte[] data = GenerateRandomData();
            List<byte> segmentsData = new List<byte>();

            for (int i = 0; i < data.Length; i += 1024)
            {
                byte[] segmentData = data.Skip(i).Take(1024).ToArray();
                UdpSegment segment = new UdpSegment(i / 1024, segmentData);
                segmentsData.AddRange(segment.ToBytes());
            }

            UdpPacket packet = new UdpPacket(sequenceNumber++, segmentsData.ToArray());
            byte[] packetBytes = packet.ToBytes();
            await sender.SendToAsync(new ArraySegment<byte>(packetBytes), SocketFlags.None, new IPEndPoint(serverAddress, serverPort));

            await Task.Delay(1000); // задержка между отправками
        }
    }

    private byte[] GenerateRandomData()
    {
        int dataSize = random.Next(256, 16384); // Генерация случайного размера данных от 50 до 500 байт
        byte[] data = new byte[dataSize];
        random.NextBytes(data); // Заполнение данных случайными байтами
        return data;
    }
}