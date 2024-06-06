using System;
using System.Net.Sockets;
using System.Net;
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

    // !!Нужно этот метод править
    public async Task StartSendingAsync()
    {
        Console.WriteLine("Отправка пакетов...");

        while (true)
        {
            byte[] data = GenerateRandomData();            
            List<UdpPacket> packets = UdpSeparationData.SeparationDataToPacket(data);
        
            foreach(var packet in packets)
            {                
                byte[] packetBytes = packet.ToBytes();
                await sender.SendToAsync(new ArraySegment<byte>(packetBytes), SocketFlags.None, new IPEndPoint(serverAddress, serverPort));

                await Task.Delay(100); // задержка между отправками
            }
        }
    }

    private byte[] GenerateRandomData()
    {
        int dataSize = random.Next(512, 1024000);
        byte[] data = new byte[dataSize];
        random.NextBytes(data); // Заполнение данных случайными байтами

        return data;
    }
}