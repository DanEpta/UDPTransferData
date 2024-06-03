using UdpServer.Server;


await Task.Run(ExecuteAsync);

async Task ExecuteAsync()
{
    string ipAddress = "127.0.0.1";
    int port = 4004;

    UDPServer udpServer = new UDPServer(ipAddress, port);

    // запускаем получение сообщений
    await Task.WhenAll(
        udpServer.ReceiveMessageAsync(),
        udpServer.UpdateStatisticsAsync()
    );

}
