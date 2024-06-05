namespace UdpProgram.Udp
{
    public class UdpSegment
    {
        public int SegmentId { get; set; }
        public byte[] Data { get; set; }

        public UdpSegment(int id, byte[] data)
        {
            SegmentId = id;
            Data = data;
        }

        public byte[] ToBytes()
        {
            List<byte> result = new List<byte>();
            byte[] segmentIdBytes = BitConverter.GetBytes(SegmentId);
            result.AddRange(segmentIdBytes);
            result.AddRange(Data);
            return result.ToArray();
        }

        public static UdpSegment FromBytes(byte[] bytes)
        {
            int segmentId = BitConverter.ToInt32(bytes, 0);
            byte[] data = new byte[bytes.Length - 4];
            Buffer.BlockCopy(bytes, 4, data, 0, data.Length);
            return new UdpSegment(segmentId, data);
        }
    }
}
