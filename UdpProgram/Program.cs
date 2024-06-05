using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UdpProgram.Udp;


await Task.Run(async () =>
{
    string serverAddress = "192.168.56.101";
    int port = 4004;
    UDPClient client = new UDPClient(serverAddress, port);
    await client.StartSendingAsync();
});