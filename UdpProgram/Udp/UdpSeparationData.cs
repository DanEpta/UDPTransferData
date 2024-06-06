

namespace UdpProgram.Udp
{
    public class UdpSeparationData
    {
        private static readonly int MaxSizeDataPacket = 65439;
        private static readonly UInt16 SizeDataSegment = 1400;


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

        public static List<UdpSegment> SeparationPacketToSegment(UdpPacket packet)
        {
            List<UdpSegment> segments = new List<UdpSegment>();

            byte[] packetBytes = packet.ToBytes();

            for (int i = sizeof(uint); i < packetBytes.Length; i += SizeDataSegment)
            {
                byte[] segmentData = packetBytes.Skip(i).Take(SizeDataSegment).ToArray();
                UdpSegment segment = new UdpSegment((byte)(i / SizeDataSegment), packet.PacketId, segmentData);
                segments.Add(segment);
            }
            return segments;
        }
    }
}
