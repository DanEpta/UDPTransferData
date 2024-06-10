using System;
using System.Net.Sockets;
using System.Net;
using UdpProgram.Udp;
using System.Text;

public class UDPClient
{
    private Socket sender;
    private readonly IPAddress serverAddress;
    private readonly int serverPort;
    private Random random;

    private const string PacketCountId = "PACKET_COUNT";


    public UDPClient(string serverIpAddress, int serverPort)
    {
        serverAddress = IPAddress.Parse(serverIpAddress);
        this.serverPort = serverPort;
        sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        random = new Random();
    }


    public async Task StartSendingAsync()
    {

        while (true)
        {
            byte[] data = GenerateRandomData();            
            List<UdpPacket> packets = UdpSeparationData.SeparationDataToPacket(data);

            int totalPackets = packets.Count;            
            await SendPacketCountAsync(totalPackets);

            Console.WriteLine($"Количество пакетов {totalPackets}");
            //await Task.Delay(500); // задержка между отправками

            foreach (var packet in packets)
            {
                byte[] packetBytes = packet.ToBytes();
                await sender.SendToAsync(new ArraySegment<byte>(packetBytes), SocketFlags.None, new IPEndPoint(serverAddress, serverPort));
                Console.WriteLine($"Отправлен пакет {packet.PacketId} размером в {packet.Data.Length} байт");
                //await Task.Delay(500); // задержка между отправками
            }
        }
    }

    private async Task SendPacketCountAsync(int totalPackets)
    {
        byte[] idBytes = Encoding.UTF8.GetBytes(PacketCountId);
        byte[] totalPacketsBytes = BitConverter.GetBytes(totalPackets);
        byte[] message = new byte[idBytes.Length + totalPacketsBytes.Length];

        Buffer.BlockCopy(idBytes, 0, message, 0, idBytes.Length);
        Buffer.BlockCopy(totalPacketsBytes, 0, message, idBytes.Length, totalPacketsBytes.Length);

        await sender.SendToAsync(new ArraySegment<byte>(message), SocketFlags.None, new IPEndPoint(serverAddress, serverPort));
    }

    private byte[] GenerateRandomData()
    {
        int dataSize = random.Next(512, 1024000);
        byte[] data = new byte[dataSize];
        random.NextBytes(data); // Заполнение данных случайными байтами

        return data;
    }
}