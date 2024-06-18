using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UdpProgramStatistic.UDPServer
{
    public class ServerStatistics
    {
        private uint totalPacketsReceived;
        private uint totalPacketsLost;
        private uint totalDataReceived;
        private DateTime startTime;

        public ServerStatistics()
        {
            Reset();
        }

        public void Start()
        {
            startTime = DateTime.Now;
        }

        public void AddReceivedPacket(uint packetSize)
        {
            totalPacketsReceived++;
            totalDataReceived += packetSize;
        }

        public void AddLostPackets(uint lostPacketsCount)
        {
            totalPacketsLost += lostPacketsCount;
        }

        public void DisplayStatistics()
        {
            TimeSpan elapsedTime = DateTime.Now - startTime;
            double throughput = totalDataReceived / elapsedTime.TotalSeconds; // bytes per second
            double lossPercentage = (double)totalPacketsLost / (totalPacketsReceived + totalPacketsLost) * 100;

            Console.WriteLine("===== Server Statistics =====");
            Console.WriteLine($"Elapsed Time: {elapsedTime.TotalSeconds:F2} seconds");
            Console.WriteLine($"Total Data Received: {totalDataReceived} bytes");
            Console.WriteLine($"Throughput: {throughput:F2} bytes/second");
            Console.WriteLine($"Total Packets Received: {totalPacketsReceived}");
            Console.WriteLine($"Total Packets Lost: {totalPacketsLost}");
            Console.WriteLine($"Packet Loss Percentage: {lossPercentage:F2}%");
            Console.WriteLine("=============================");
        }

        public void Reset()
        {
            totalPacketsReceived = 0;
            totalPacketsLost = 0;
            totalDataReceived = 0;
            startTime = DateTime.Now;
        }
    }
}
