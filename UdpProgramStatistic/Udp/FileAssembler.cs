namespace UdpProgramStatistic.Udp
{
    public class FileAssembler
    {
        private Dictionary<uint, byte[]> packets;
        private readonly object lockObject = new object();


        public FileAssembler() =>
            packets = new Dictionary<uint, byte[]>();
        

        public void AddPacket(uint packetId, byte[] data)
        {
            lock (lockObject)
                if (!packets.ContainsKey(packetId))
                    packets.Add(packetId, data);

        }

        public void AssembleFile()
        {
            lock (lockObject)
            {
                var orderedPackets = packets.OrderBy(p => p.Key).Select(p => p.Value).ToList();
                // Заглушка вместо сохранения файла
                Console.WriteLine($"Файл собран из {orderedPackets.Count} пакетов.");
            }
        }

        public void Reset()
        {
            lock (lockObject)
                packets.Clear();
        }
    }
}