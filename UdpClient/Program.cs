


await Task.Run(SendingMessagesAsync);

async Task SendingMessagesAsync()
{
    string ipAddress = "127.0.0.1";
    int port = 4004;

    UDPClient udpClient = new UDPClient(ipAddress, port);

    // запускаем получение сообщений и прослушку потерянных пактов
    await Task.WhenAll(
        udpClient.StartSendingAsync(),
        udpClient.ReceiveLostPacketNotificationAsync()
    );
}