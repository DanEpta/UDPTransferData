using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UdpProgramStatistic
{
    public class CustomUdpServer : UdpClient
    {
        private IPEndPoint listenEndPoint;
        private IPEndPoint clientEndPoint;
        private PacketChecker packetChecker;
        private Timer timeoutTimer;
        private ServerPacketLossHandler serverPacketLossHandler;
        private readonly Dictionary<uint, UdpPacket> receivedPackets = new Dictionary<uint, UdpPacket>();

        private const string PacketCountId = "PACKET_COUNT";
        private const string ConfirmationId = "CONFIRMATION";
        private const string LostPacketsId = "LOST_PACKETS";
        private const string RetransmitId = "RETRANSMIT";
        private readonly int timeoutPeriod = 5000;

        public CustomUdpServer(int port, string clientIpAddress) : base(port)
        {
            listenEndPoint = new IPEndPoint(IPAddress.Any, port);
            clientEndPoint = new IPEndPoint(IPAddress.Parse(clientIpAddress), port + 1);

            packetChecker = new PacketChecker();
            serverPacketLossHandler = new ServerPacketLossHandler(packetChecker, this);
        }

        public async Task StartReceivingAsync()
        {
            Console.WriteLine("Начало приема пакетов...");
            byte[] buffer = new byte[65535];

            timeoutTimer = new Timer(OnTimeout, null, Timeout.Infinite, Timeout.Infinite);

            while (true)
            {
                UdpReceiveResult result = await ReceiveAsync();
                byte[] receivedData = result.Buffer;

                timeoutTimer.Change(timeoutPeriod, Timeout.Infinite);
                DataProcessing(receivedData);
            }
        }

        private void DataProcessing(byte[] receivedData)
        {
            if (IsPacketCountMessage(receivedData))
            {
                uint expectedPackets = BitConverter.ToUInt32(receivedData, PacketCountId.Length);
                packetChecker.SetExpectedPacketCount(expectedPackets);
                serverPacketLossHandler.Start(expectedPackets);

                Console.WriteLine($"Ожидается {expectedPackets} пакетов.");
            }
            else
            {
                UdpPacket packet = UdpPacket.FromBytes(receivedData);

                if (!packetChecker.AddReceivedPacketId(packet.PacketId))
                {
                    Console.WriteLine($"Получен неожиданный пакет с ID {packet.PacketId}. Пакет проигнорирован.");
                    return;
                }
                Console.WriteLine($"Получен пакет {packet.PacketId} размером {packet.ToBytes().Length} байт");

                // Обработка пакета
                receivedPackets[packet.PacketId] = packet;

                if (packetChecker.HaveAllPackages())
                {
                    DataProcessingReceived();
                    timeoutTimer.Change(Timeout.Infinite, Timeout.Infinite);
                }    
            }
        }        

        private async void OnTimeout(object state)
        {
            if (!packetChecker.HaveAllPackages())
            {
                Console.WriteLine("Не все пакеты получены. Тайм-аут.");
                // Здесь можно обработать ситуацию тайм-аута, например, запросить повторную отправку недостающих пакетов
                await RequestRetransmitAsync();
            }
            else
            {
                DataProcessingReceived();
            }
        }

        public async Task SendLostPacketsAsync(List<uint> lostPacketIds)
        {
            string lostPacketsMessage = $"{LostPacketsId}:{string.Join(",", lostPacketIds)}";
            byte[] lostPacketsData = Encoding.UTF8.GetBytes(lostPacketsMessage);

            await SendAsync(lostPacketsData, lostPacketsData.Length, clientEndPoint);
            Console.WriteLine("Список потерянных пакетов отправлен клиенту.");
        }

        private async Task SendConfirmationAsync()
        {
            string confirmationMessage = $"{ConfirmationId}";
            byte[] confirmationData = Encoding.UTF8.GetBytes(confirmationMessage);
            await SendAsync(confirmationData, confirmationData.Length, clientEndPoint);
            Console.WriteLine("Подтверждение отправлено клиенту.");
        }

        private async Task RequestRetransmitAsync()
        {
            byte[] retransmitData = Encoding.UTF8.GetBytes(RetransmitId);
            await SendAsync(retransmitData, retransmitData.Length, clientEndPoint);
            ///!!!!!!!!!!!!!!!!!!!!!
            Console.WriteLine("Запрос на повторную отправку файла отправлен клиенту.");
        }

        private bool IsPacketCountMessage(byte[] data)
        {
            string id = Encoding.UTF8.GetString(data, 0, PacketCountId.Length);
            return id == PacketCountId;
        }

        private async void DataProcessingReceived()
        {
            Console.WriteLine("Все пакеты получены");
            //fileAssembler.AssembleFile();
            await SendConfirmationAsync();
            packetChecker.Reset();
            receivedPackets.Clear();
            //fileAssembler.Reset();
            serverPacketLossHandler.Stop();
        }
    }
}