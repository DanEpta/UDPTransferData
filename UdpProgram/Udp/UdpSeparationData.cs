

namespace UdpProgram.Udp
{
    public class UdpSeparationData
    {
        private static readonly int MaxSizeDataPacket = 65503;


        public static List<UdpPacket> SeparationDataToPacket(byte[] data)
        {
            List<UdpPacket> packets = new List<UdpPacket>();

            for (int i = 0; i < data.Length; i += MaxSizeDataPacket) 
            {
                int packetSize = Math.Min(MaxSizeDataPacket, data.Length - i);
                byte[] packetData = new byte[packetSize];
                Buffer.BlockCopy(data, i, packetData, 0, packetSize);
                UdpPacket packetDataUdp = new UdpPacket((uint)(i / MaxSizeDataPacket), packetData);

                packets.Add(packetDataUdp);
            }
            return packets;
        }
    }
}
