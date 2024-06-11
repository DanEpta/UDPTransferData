

await Task.Run(async () =>
{
    string serverAddress = "192.168.56.101";
    int port = 4004;

    string confirmAddresIp = "192.168.56.1";
    int portConfirm = port + 1;

    UDPClient client = new UDPClient(serverAddress, port, confirmAddresIp, portConfirm);
    await client.StartSendingAsync();
});