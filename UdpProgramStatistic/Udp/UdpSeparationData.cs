namespace UdpProgramStatistic
{
    public class UdpSeparationData
    {
        private static readonly int MaxSizeDataPacket = 65503;


        public static List<UdpPacket> SeparationDataToPacket(byte[] data)
        {
            List<UdpPacket> packets = new List<UdpPacket>();
            int totalPackets = (int)Math.Ceiling((double)data.Length / MaxSizeDataPacket);

            for (int i = 0; i < totalPackets; i++)
            {
                int startIndex = i * MaxSizeDataPacket;
                int packetSize = Math.Min(MaxSizeDataPacket, data.Length - startIndex);

                if (packetSize > 0)
                {
                    byte[] packetData = new byte[packetSize];
                    Buffer.BlockCopy(data, startIndex, packetData, 0, packetSize);
                    UdpPacket packetDataUdp = new UdpPacket((uint)i, packetData);

                    packets.Add(packetDataUdp);
                }
            }
            return packets;
        }
    }
}
