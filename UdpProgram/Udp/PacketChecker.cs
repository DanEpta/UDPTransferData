﻿namespace UdpProgram.Udp
{
    public class PacketChecker
    {
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


        public void SetExpectedPacketCount(uint expectedPacketCount) =>
            ExpectedPacketCount = expectedPacketCount;

        public void ResetExpectedPacketCount() =>
            ExpectedPacketCount = null;

        public bool HasLostPackets() =>
            MissingPacketIds.Count > 0;

        public List<uint> GetMissingPacketIds() =>
            new List<uint>(MissingPacketIds);        

        public void AddReceivedPacketId(uint packetId)
        {
            ReceivedPacketIds.Add(packetId);
            CheckMissingPacketId(packetId);
        }

        public bool HaveAllPackages()
        {
            if (!ExpectedPacketCount.HasValue)
                throw new InvalidOperationException("ExpectedPacketCount is not set.");

            return ReceivedPacketIds.Count == ExpectedPacketCount && !HasLostPackets();
        }

        public List<uint> FullGetMissingPacketIds() 
        {
            if (!ExpectedPacketCount.HasValue)
                throw new InvalidOperationException("ExpectedPacketCount is not set.");

            List<uint> missingPacketsId = new List<uint>();

            for (uint i = 0; i <= ExpectedPacketCount; i++)
                if (!ReceivedPacketIds.Contains(i))
                    missingPacketsId.Add(i);

            return missingPacketsId;
        }

        public void Reset()
        {
            ReceivedPacketIds.Clear();
            MissingPacketIds.Clear();
            ExpectedPacketCount = null;
            MaxReceivedPacketId = 0;
        }

        private void CheckMissingPacketId(uint packetId)
        {
            for (uint i = MaxReceivedPacketId; i < packetId; i++)
                if (!ReceivedPacketIds.Contains(i) && !MissingPacketIds.Contains(i))
                    MissingPacketIds.Add(i);

            MaxReceivedPacketId = packetId > MaxReceivedPacketId ? packetId : MaxReceivedPacketId; // Возможно, при false надо 0
            MissingPacketIds.Remove(packetId);
        }
    }
}
