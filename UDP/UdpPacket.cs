using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDP
{
    public class UdpPacket
    {
        public int Id { get; set; }
        public byte[] Data { get; set; }


        public UdpPacket(int id, byte[] data) 
        {
            Id = id;
            Data = data;
        }


        public byte[] ToBytes()
        {
            byte[] idBytes = BitConverter.GetBytes(Id);
            byte[] packetBytes = new byte[idBytes.Length + Data.Length];

            Buffer.BlockCopy(idBytes, 0, packetBytes, 0, idBytes.Length);
            Buffer.BlockCopy(Data, 0, packetBytes, idBytes.Length, Data.Length);
            return packetBytes;
        }

        public static UdpPacket FromBytes(byte[] packetBytes)
        {
            int sizeId = 4;

            int id = BitConverter.ToInt32(packetBytes, 0);
            byte[] data = new byte[packetBytes.Length - sizeId];
            Buffer.BlockCopy(packetBytes, sizeId, data, 0, data.Length);

            return new UdpPacket(id, data);
        }

        public override string ToString() =>
            $"ID: {Id}, Data: {System.Text.Encoding.UTF8.GetString(Data)}";

    }
}
