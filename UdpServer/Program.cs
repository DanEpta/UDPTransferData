using UdpServer.Server;


await Task.Run(ExecuteAsync);

async Task ExecuteAsync()
{
    string ipAddress = "0.0.0.0";
    string sendAddress = "192.168.56.1";
    int port = 4004;

    UDPServer udpServer = new UDPServer(ipAddress, sendAddress, port);

    // запускаем получение сообщений
    await Task.WhenAll(
        udpServer.ReceiveMessageAsync(),
        udpServer.UpdateStatisticsAsync()
    );
}
