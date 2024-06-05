namespace UdpProgram.Udp
{
    public class UdpPacket
    {
        public int PacketId { get; set; }
        public byte[] SegmentsData { get; set; }

        public UdpPacket(int id, byte[] segmentsData)
        {
            PacketId = id;
            SegmentsData = segmentsData;
        }

        public byte[] ToBytes()
        {
            List<byte> result = new List<byte>();

            byte[] packetIdBytes = BitConverter.GetBytes(PacketId);
            result.AddRange(packetIdBytes);
            result.AddRange(SegmentsData);

            return result.ToArray();
        }

        public static UdpPacket FromBytes(byte[] bytes)
        {
            int packetId = BitConverter.ToInt32(bytes, 0);
            byte[] segmentsData = new byte[bytes.Length - 4];
            Buffer.BlockCopy(bytes, 4, segmentsData, 0, segmentsData.Length);
            return new UdpPacket(packetId, segmentsData);
        }
    }
}