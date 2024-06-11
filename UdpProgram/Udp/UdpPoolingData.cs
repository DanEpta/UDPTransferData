namespace UdpProgram.Udp
{
    public static class UdpPoolingData
    {
        public static byte[] PoolingPacketToData(List<UdpPacket> packets)
        {
            List<byte> data = new List<byte>();

            foreach (var packet in packets)
                data.AddRange(packet.Data);

            return data.ToArray();
        }
    }
}
