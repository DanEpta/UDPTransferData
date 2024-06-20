namespace UdpProgramStatistic
{
    public class PacketChecker
    {
        private readonly object lockObject = new object();
        private HashSet<uint> ReceivedPacketIds;
        private List<uint> MissingPacketIds;
        private uint? ExpectedPacketCount;
        private uint MaxReceivedPacketId;

        public PacketChecker(uint? expectedPacketCount = null)
        {
            ReceivedPacketIds = new HashSet<uint>();
            MissingPacketIds = new List<uint>();
            ExpectedPacketCount = expectedPacketCount;
            MaxReceivedPacketId = 0;
        }

        public void SetExpectedPacketCount(uint expectedPacketCount)
        {
            lock (lockObject)
                ExpectedPacketCount = expectedPacketCount;
        }

        public void ResetExpectedPacketCount()
        {
            lock (lockObject)
                ExpectedPacketCount = null;
        }

        public bool HasLostPackets()
        {
            lock (lockObject)
                return MissingPacketIds.Count > 0;
        }

        public List<uint> GetMissingPacketIds()
        {
            lock (lockObject)
                return new List<uint>(MissingPacketIds);
        }

        public void AddMissingPackets(IEnumerable<uint> missingPackets)
        {
            lock (lockObject)
            {
                var newMissingPackets = missingPackets.Except(MissingPacketIds).ToList();
                MissingPacketIds.AddRange(newMissingPackets);
            }
        }

        public bool AddReceivedPacketId(uint packetId)
        {
            if (!ExpectedPacketCount.HasValue)
                return false;

            lock (lockObject)
            {
                ReceivedPacketIds.Add(packetId);
                CheckMissingPacketId(packetId);
            }
            return true;
        }

        public bool HaveAllPackages()
        {
            if (!ExpectedPacketCount.HasValue)
                throw new InvalidOperationException("ExpectedPacketCount is not set. place HaveAllPackages");

            lock (lockObject)
                return (ReceivedPacketIds.Count == ExpectedPacketCount) && !HasLostPackets();
        }

        public List<uint> FullGetMissingPacketIds()
        {
            if (!ExpectedPacketCount.HasValue)
                throw new InvalidOperationException("ExpectedPacketCount is not set. place FullGetMissingPacketIds");

            List<uint> missingPacketsId = new List<uint>();

            lock (lockObject)
            {
                for (uint i = 0; i < ExpectedPacketCount; i++)
                    if (!ReceivedPacketIds.Contains(i))
                        missingPacketsId.Add(i);
            }

            return missingPacketsId;
        }

        public void Reset()
        {
            lock (lockObject)
            {
                ReceivedPacketIds.Clear();
                MissingPacketIds.Clear();
                ExpectedPacketCount = null;
                MaxReceivedPacketId = 0;
            }
        }

        private void CheckMissingPacketId(uint packetId)
        {
            lock (lockObject)
            {
                for (uint i = MaxReceivedPacketId; i < packetId; i++)
                    if (!ReceivedPacketIds.Contains(i) && !MissingPacketIds.Contains(i))
                        MissingPacketIds.Add(i);

                if (packetId < MaxReceivedPacketId && !ReceivedPacketIds.Contains(packetId))
                {
                    ReceivedPacketIds.Add(packetId);
                    MissingPacketIds.Remove(packetId);
                }

                MaxReceivedPacketId = packetId > MaxReceivedPacketId ? packetId : MaxReceivedPacketId;
                MissingPacketIds.Remove(packetId);
            }
        }
    }
}