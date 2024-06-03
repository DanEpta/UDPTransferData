using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Client
{
    static void Main(string[] args)
    {
        IPAddress serverAddress = IPAddress.Parse("192.168.56.101"); // Укажите IP адрес сервера (виртуальной машины)
        int serverPort = 4004; // Укажите порт сервера

        using Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        while (true)
        {
            Console.Write("Введите сообщение для отправки: ");
            string message = Console.ReadLine();

            byte[] data = Encoding.UTF8.GetBytes(message);
            EndPoint serverEP = new IPEndPoint(serverAddress, serverPort);
            sender.SendTo(data, serverEP);
            Console.WriteLine("Сообщение отправлено!");
        }
 
    }
}