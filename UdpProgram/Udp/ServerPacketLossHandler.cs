namespace UdpProgram.Udp
{
    public class ServerPacketLossHandler
    {
        private readonly PacketChecker packetChecker;
        private readonly UDPServer udpServer;
        private readonly Timer packetCheckTimer;
        private readonly object lockObject = new object();
        private uint expectedPackets;
        private int checkIntervalMilliseconds;

        public ServerPacketLossHandler(PacketChecker packetChecker, UDPServer udpServer, int checkIntervalMilliseconds = 5000)
        {
            this.packetChecker = packetChecker;
            this.udpServer = udpServer;
            this.checkIntervalMilliseconds = checkIntervalMilliseconds;
            packetCheckTimer = new Timer(CheckForLostPackets, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void Start(uint expectedPackets)
        {
            this.expectedPackets = expectedPackets;
            packetChecker.SetExpectedPacketCount(expectedPackets);
            packetCheckTimer.Change(checkIntervalMilliseconds, checkIntervalMilliseconds);
        }

        public void Stop() => 
            packetCheckTimer.Change(Timeout.Infinite, Timeout.Infinite);

        private void CheckForLostPackets(object state)
        {
            lock (lockObject)
            {
                List<uint> missingIds = packetChecker.FullGetMissingPacketIds();
                if (missingIds.Any())
                {
                    packetChecker.AddMissingPackets(missingIds);
                    Console.WriteLine("Обнаружены потерянные пакеты: " + string.Join(", ", missingIds));
                    udpServer.SendLostPackets(missingIds);
                }
            }
        }
    }
}
