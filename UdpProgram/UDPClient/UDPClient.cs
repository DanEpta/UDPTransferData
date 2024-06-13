using System;
using System.Net.Sockets;
using System.Net;
using UdpProgram.Udp;
using System.Text;

public class UDPClient
{
    private Socket sender;
    private Socket confirmationReceiver;
    private readonly IPAddress serverAddress;
    private readonly IPAddress confirmationAddress;
    private readonly int serverPort;
    private readonly int confirmationPort;
    private Random random;
    private bool isSending;

    private const string PacketCountId = "PACKET_COUNT";
    private const string ConfirmationId = "CONFIRMATION";
    private const string LostPacketsId = "LOST_PACKETS";


    public UDPClient(string serverIpAddress, int serverPort, string confirmationIpAddress, int confirmationPort)
    {
        serverAddress = IPAddress.Parse(serverIpAddress);
        this.serverPort = serverPort;
        sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        confirmationAddress = IPAddress.Parse(confirmationIpAddress);
        this.confirmationPort = confirmationPort;
        confirmationReceiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        confirmationReceiver.Bind(new IPEndPoint(confirmationAddress, confirmationPort));

        random = new Random();
        isSending = false;
    }


    public async Task StartSendingAsync()
    {

        while (true)
        {
            if (!isSending)
                await PacketSendingAsync();
            else
                await Task.Delay(1000);
        }
    }

    private async Task PacketSendingAsync()
    {
        byte[] data = GenerateRandomData();
        List<UdpPacket> packets = UdpSeparationData.SeparationDataToPacket(data);

        int totalPackets = packets.Count;
        await SendPacketCountAsync(totalPackets);

        Console.WriteLine($"Количество пакетов {totalPackets}");

        foreach (var packet in packets)
        {
            byte[] packetBytes = packet.ToBytes();
            await sender.SendToAsync(new ArraySegment<byte>(packetBytes), SocketFlags.None, new IPEndPoint(serverAddress, serverPort));
            Console.WriteLine($"Отправлен пакет {packet.PacketId} размером в {packet.Data.Length} байт");
            //await Task.Delay(500); // задержка между отправками
        }
        await ReceiveConfirmationOrLostPackets();
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

    private async Task ReceiveConfirmationOrLostPackets()
    {
        byte[] buffer = new byte[65535];
        EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        var result = await confirmationReceiver.ReceiveFromAsync(new ArraySegment<byte>(buffer), SocketFlags.None, remoteEndPoint);
        string message = Encoding.UTF8.GetString(buffer, 0, result.ReceivedBytes);

        if (message.StartsWith(ConfirmationId))
        {
            Console.WriteLine("Подтверждение получено от сервера: " + message);
            isSending = false;
        }
        else if (message.StartsWith(LostPacketsId))
        {
            string[] parts = message.Substring(LostPacketsId.Length + 1).Split(',');
            List<uint> lostPackets = parts.Select(uint.Parse).ToList();

            Console.WriteLine("Потерянные пакеты: " + string.Join(", ", lostPackets));
        }
    }

    private List<uint> ParseLostPackets(string lostPacketsString)
    {
        List<uint> lostPackets = new List<uint>();
        string[] lostPacketsArray = lostPacketsString.Split(',');

        foreach (string packetIdString in lostPacketsArray)
            if (uint.TryParse(packetIdString, out uint packetId))
                lostPackets.Add(packetId);

        return lostPackets;
    }

    private byte[] GenerateRandomData()
    {
        int dataSize = random.Next(512, 1024000);
        byte[] data = new byte[dataSize];
        random.NextBytes(data); // Заполнение данных случайными байтами

        return data;
    }
}