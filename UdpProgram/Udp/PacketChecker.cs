using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UdpProgram.Udp
{
    public class PacketChecker
    {
        private HashSet<uint> ReceivedPacketIds;
        private uint ExpectedPacketCount;

        public PacketChecker(uint expectedPacketCount)
        { 
            ReceivedPacketIds = new HashSet<uint>();
            ExpectedPacketCount = expectedPacketCount;
        }

        public void AddReceivedPacketId(uint packetId) =>
            ReceivedPacketIds.Add(packetId);

        public bool HasLostPackets()
        {
            for (uint i = 0; i < ExpectedPacketCount; i++)
            {
                if (!ReceivedPacketIds.Contains(i))
                    return true;
            }
            return false;
        }

        public List<uint> GetMissingPacketIds() 
        {
            List<uint> missingPacketsId = new List<uint>();

            for (uint i = 0; i <= ExpectedPacketCount; i++)
            {
                if (!ReceivedPacketIds.Contains(i))
                    missingPacketsId.Add(i);
            }
            return missingPacketsId;
        }
    }
}
