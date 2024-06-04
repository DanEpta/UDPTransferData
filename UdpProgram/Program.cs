using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class UDPServer
{
    private readonly IPAddress localAddress;
    private readonly int localPort;
    private readonly Socket receiver;
    private int receivedPackets = 0;

    public UDPServer(string ipAddress, int port)
    {
        localAddress = IPAddress.Parse(ipAddress);
        localPort = port;
        receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        receiver.Bind(new IPEndPoint(localAddress, localPort));
    }

    public async Task ReceiveMessageAsync()
    {
        byte[] data = new byte[1024];

        while (true)
        {
            var result = await receiver.ReceiveFromAsync(data, SocketFlags.None, new IPEndPoint(IPAddress.Any, 0));
            int packetNumber = BitConverter.ToInt32(data, 0); // извлекаем номер пакета
            receivedPackets++;
            Console.WriteLine($"Получен пакет №{packetNumber}, Всего получено: {receivedPackets}");
        }
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        string ipAddress = "192.168.56.1"; // IP хоста
        int port = 4004;

        UDPServer udpServer = new UDPServer(ipAddress, port);
        await udpServer.ReceiveMessageAsync();
    }
}