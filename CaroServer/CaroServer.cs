using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

class Program
{
    static List<TcpClient> clients = new List<TcpClient>();
    static List<string> clientNames = new List<string>();
    static object locker = new object();

    static void Main()
    {
        TcpListener server = new TcpListener(IPAddress.Any, 8888);
        server.Start();
        Console.WriteLine("Server started, waiting for connection...");

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            Thread t = new Thread(() => HandleClient(client));
            t.Start();
        }
    }

    static void HandleClient(TcpClient client)
    {
        string clientName = "";
        try
        {
            NetworkStream stream = client.GetStream();

            // Nhận tên client
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            clientName = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            lock (locker)
            {
                clients.Add(client);
                clientNames.Add(clientName);
            }

            // Broadcast cho tất cả client (gồm cả client vừa vào để mỗi người đều thấy ai vào)
            Broadcast($"[System] {clientName} da tham gia phong!");

            Console.WriteLine($"[System] {clientName} da tham gia phong!");

            while (true)
            {
                buffer = new byte[1024];
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    string msg = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Broadcast($"{clientName}: {msg}");
                    Console.WriteLine($"{clientName}: {msg}");
                }
                else
                {
                    break;
                }
            }
        }
        catch { }

        // Nếu client disconnect
        lock (locker)
        {
            int idx = clients.IndexOf(client);
            if (idx >= 0)
            {
                clients.RemoveAt(idx);
                string leftName = clientNames[idx];
                clientNames.RemoveAt(idx);
                Broadcast($"[System] {leftName} da roi phong!");
                Console.WriteLine($"[Hệ thống] {leftName} da roi phong!");
            }
        }
    }

    static void Broadcast(string msg)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(msg);
        lock (locker)
        {
            // Gửi cho tất cả client đang kết nối
            foreach (var client in clients)
            {
                try
                {
                    NetworkStream stream = client.GetStream();
                    stream.Write(buffer, 0, buffer.Length);
                }
                catch { }
            }
        }
    }
}