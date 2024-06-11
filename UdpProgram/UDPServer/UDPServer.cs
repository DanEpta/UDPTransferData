using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace UdpProgram.Udp
{
    public class UDPServer
    {
        private Socket receiver;
        PacketChecker packetChecker;
        private readonly IPAddress localAddress;
        private readonly IPAddress remoteAddress;
        private readonly int localPort;

        private const string PacketCountId = "PACKET_COUNT";
        private const string ConfirmationId = "CONFIRMATION";
        private List<byte[]> receivedPackets;
        private uint expectedPackets;


        public UDPServer(string localIpAddress, int localPort, string remoteIpAddress)
        {
            localAddress = IPAddress.Parse(localIpAddress);
            this.localPort = localPort;
            remoteAddress = IPAddress.Parse(remoteIpAddress);
            expectedPackets = 0;
            receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            receivedPackets = new List<byte[]>();
            packetChecker = new PacketChecker();
        }


        public async Task StartReceivingAsync()
        {
            Console.WriteLine("Начало приема пакетов...");

            receiver.Bind(new IPEndPoint(localAddress, localPort));
            byte[] buffer = new byte[65535];

            while (true)
            {
                // Прием данных
                var result = await receiver.ReceiveFromAsync(new ArraySegment<byte>(buffer), SocketFlags.None, new IPEndPoint(IPAddress.Any, 0));
                byte[] receivedData = buffer.Take(result.ReceivedBytes).ToArray();
                DataProcessing(receivedData);
            }
        }

        private void DataProcessing(byte[] receivedData)
        {
            if (IsPacketCountMessage(receivedData))
            {
                expectedPackets = BitConverter.ToUInt32(receivedData, PacketCountId.Length);
                packetChecker.SetExpectedPacketCount(expectedPackets);

                Console.WriteLine($"Ожидается {expectedPackets} пакетов.");
            }
            else
            {
                UdpPacket packet = UdpPacket.FromBytes(receivedData);
                packetChecker.AddReceivedPacketId(packet.PacketId);
                receivedPackets.Add(packet.Data);

                Console.WriteLine($"Получен пакет {packet.PacketId} размером {packet.ToBytes().Length} байт");

                if (packetChecker.HaveAllPackages())
                    DataProcessingReceived();
                /*else if (packetChecker.HasLostPackets())
                {
                    Console.WriteLine("Есть потерянные пакеты: \n"); // надо обработать

                    List<uint> missingIds = packetChecker.GetMissingPacketIds();

                    foreach (var missingId in missingIds)
                        Console.Write($"{missingId} \t");
                    Console.WriteLine();
                    packetChecker.Reset();
                    expectedPackets = 0;
                    receivedPackets.Clear();
                }*/
            }
        }        

        private bool IsPacketCountMessage(byte[] data)
        {
            string id = Encoding.UTF8.GetString(data, 0, PacketCountId.Length);
            return id == PacketCountId;
        }

        private void SendConfirmation()
        {
            string confirmationMessage = $"{ConfirmationId}";
            byte[] confirmationData = Encoding.UTF8.GetBytes(confirmationMessage);
            EndPoint confirmationEndPoint = new IPEndPoint(remoteAddress, localPort + 1);
            receiver.SendTo(confirmationData, confirmationEndPoint);
            Console.WriteLine("Подтверждение отправлено клиенту.");
        }

        private void DataProcessingReceived()
        {
            Console.WriteLine("Все пакеты получены");
            SendConfirmation();
            //////////////// надо что то сделать с полученными данными
            expectedPackets = 0;
            packetChecker.Reset();
            receivedPackets.Clear();
        }
    }
}