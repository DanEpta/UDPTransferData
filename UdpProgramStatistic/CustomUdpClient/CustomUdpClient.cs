using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace UdpProgramStatistic
{
    public class CustomUdpClient : UdpClient
    {
        private readonly IPEndPoint serverEndPoint;
        private readonly UdpClient confirmationReceiver;

        private List<UdpPacket> SentPackets { get; set; }
        byte[] Data;

        private const string PacketCountId = "PACKET_COUNT";
        private const string ConfirmationId = "CONFIRMATION";
        private const string LostPacketsId = "LOST_PACKETS";
        private const string RetransmitId = "RETRANSMIT";


        public CustomUdpClient(string serverAddress, int serverPort) : base()
        {
            IPAddress remoteIPAddress = IPAddress.Parse(serverAddress);
            serverEndPoint = new IPEndPoint(remoteIPAddress, serverPort);
            confirmationReceiver = new UdpClient(serverPort + 1);

            SentPackets = new List<UdpPacket>();
        }


        public async Task StartSendingAsync(byte[] data)
        {
            await PacketSendingAsync(data);
            await WaitForConfirmationOrResendAsync();
        }

        private async Task PacketSendingAsync(byte[] data)
        {
            Data = data;
            List<UdpPacket> packets = UdpSeparationData.SeparationDataToPacket(data);

            int totalPackets = packets.Count;
            await SendPacketCountAsync(totalPackets);

            foreach (var packet in packets)
            {
                SentPackets.Add(packet);
                await SendPacketAsync(packet);
            }
        }

        private async Task SendPacketCountAsync(int totalPackets)
        {
            byte[] idBytes = Encoding.UTF8.GetBytes(PacketCountId);
            byte[] totalPacketsBytes = BitConverter.GetBytes(totalPackets);
            byte[] message = new byte[idBytes.Length + totalPacketsBytes.Length];

            Buffer.BlockCopy(idBytes, 0, message, 0, idBytes.Length);
            Buffer.BlockCopy(totalPacketsBytes, 0, message, idBytes.Length, totalPacketsBytes.Length);

            await SendAsync(message, message.Length, serverEndPoint);
            Console.WriteLine($"Отправлено количество пакетов: {totalPackets}");
        }

        private async Task SendPacketAsync(UdpPacket packet)
        {
            byte[] packetBytes = packet.ToBytes();
            await SendAsync(packetBytes, packetBytes.Length, serverEndPoint);
            Console.WriteLine($"Отправлен пакет {packet.PacketId} размером в {packet.Data.Length} байт");
        }

        private async Task WaitForConfirmationOrResendAsync()
        {
            bool allPacketsConfirmed = false;

            while (!allPacketsConfirmed)
            {
                UdpReceiveResult result = await confirmationReceiver.ReceiveAsync();
                string message = Encoding.UTF8.GetString(result.Buffer, 0, result.Buffer.Length);

                if (message.StartsWith(ConfirmationId))
                {
                    Console.WriteLine("Подтверждение получено от сервера: " + message);
                    SentPackets.Clear();
                    allPacketsConfirmed = true;
                }
                else if (message.StartsWith(LostPacketsId))
                {
                    string[] parts = message.Substring(LostPacketsId.Length + 1).Split(',');
                    List<uint> lostPackets = parts.Select(uint.Parse).ToList();

                    Console.WriteLine("Потерянные пакеты: " + string.Join(", ", lostPackets));
                    await ResendLostPacketsAsync(lostPackets);
                }
                else if (message.StartsWith(RetransmitId))
                {
                    Console.WriteLine("Сервер запросил повторную отправку всего файла.");
                    await ResendAllDataAsync();
                }
            }
            if (!allPacketsConfirmed)
                Console.WriteLine("Не удалось получить подтверждение от сервера после нескольких попыток.");
        }

        private async Task ResendAllDataAsync()
        {
            await PacketSendingAsync(Data);
        }

        private async Task ResendLostPacketsAsync(List<uint> lostPacketIds)
        {
            foreach (var packetId in lostPacketIds)
            {
                var packet = SentPackets.FirstOrDefault(p => p.PacketId == packetId);
                if (packet != null)
                    await SendPacketAsync(packet);
            }
        }
    }
}