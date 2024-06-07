﻿using System;
using System.Net;
using System.Net.Sockets;


namespace UdpProgram.Udp
{
    public class UDPServer
    {
        private Socket receiver;
        private readonly IPAddress localAddress;
        private readonly int localPort;
        private readonly UInt16 SizeSegment = 1400;
        private int receivedPackets = 0;

        public UDPServer(string localIpAddress, int localPort)
        {
            localAddress = IPAddress.Parse(localIpAddress);
            this.localPort = localPort;
            receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        public async Task StartReceivingAsync()
        {
            Console.WriteLine("Прием сегментов...");

            receiver.Bind(new IPEndPoint(localAddress, localPort));
            byte[] buffer = new byte[1500];

            while (true)
            {
                try
                {
                    // Прием данных
                    var result = await receiver.ReceiveFromAsync(new ArraySegment<byte>(buffer), SocketFlags.None, new IPEndPoint(IPAddress.Any, 0));
                    UdpSegment segment = UdpSegment.FromBytes(buffer.Take(result.ReceivedBytes).ToArray());

                    Console.WriteLine($"Получен сегмент {segment.SegmentId} пакета {segment.PacketId} размером {segment.ToBytes().Length} байт");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при приеме сегмента: {ex.Message}");
                }
            }
        }
    }
}