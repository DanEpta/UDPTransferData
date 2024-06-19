using System.Text;
using UdpProgramStatistic;

CustomUdpClient client = new CustomUdpClient("192.168.56.101", 4004); // IP-адрес вашей виртуальной машины

// Пример отправки данных
byte[] data = GenerateRandomData();
client.StartSending(data);


static byte[] GenerateRandomData()
{
    Random random = new Random();
    int dataSize = random.Next(1024, 1048576); // 1 Kb ... 1 Mb
    //int dataSize = random.Next(1048576, 104857600); // 1 Mb ... 100 Mb
    byte[] data = new byte[dataSize];
    random.NextBytes(data);
    return data;
}