

namespace UdpProgramStatistic.Udp
{
    public class UdpPacket
    {
        public uint PacketId { get; set; }
        public byte[] Data { get; set; }


        public UdpPacket(uint id, byte[] data)
        {
            PacketId = id;
            Data = data;
        }

        
        public byte[] ToBytes()
        {
            List<byte> result = new List<byte>();

            result.AddRange(BitConverter.GetBytes(PacketId));
            result.AddRange(Data);

            return result.ToArray();
        }
        
        public static UdpPacket FromBytes(byte[] bytes)
        {
            uint packetId = BitConverter.ToUInt32(bytes, 0);
            byte[] segmentsData = new byte[bytes.Length - 4];
            Buffer.BlockCopy(bytes, 4, segmentsData, 0, segmentsData.Length);

            return new UdpPacket(packetId, segmentsData);
        }
    }
}