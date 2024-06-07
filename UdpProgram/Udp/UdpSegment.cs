namespace UdpProgram.Udp
{
    public class UdpSegment
    {
        public byte SegmentId { get; set; }
        public uint PacketId { get; set; }
        public byte[] Data { get; set; }


        public UdpSegment(byte segmentId, uint packetId,  byte[] data)
        {
            SegmentId = segmentId;
            PacketId = packetId;
            Data = data;
        }


        public byte[] ToBytes()
        {
            List<byte> result = new List<byte>();

            result.Add(SegmentId);
            result.AddRange(BitConverter.GetBytes(PacketId));
            result.AddRange(Data);

            return result.ToArray();
        }

        public static UdpSegment FromBytes(byte[] bytes)
        {
            byte segmentId = bytes[0];
            uint packetId = BitConverter.ToUInt32(bytes, 1);
            byte[] data = new byte[bytes.Length - 5];
            Buffer.BlockCopy(bytes, 5, data, 0, data.Length);

            return new UdpSegment(segmentId, packetId, data);
        }
    }
}
