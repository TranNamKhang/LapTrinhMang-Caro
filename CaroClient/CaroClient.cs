using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Program
{
    static void Main()
    {
        Console.Write("Nhap ten cua ban: ");
        string myName = Console.ReadLine();

        TcpClient client = new TcpClient("127.0.0.1", 8888); // Đổi IP nếu khác máy
        Console.WriteLine("Da ket noi thanh cong!");
        Console.WriteLine("Lenh: /chat noi dung");
        NetworkStream stream = client.GetStream();

        // Gửi tên client cho server
        byte[] buffer = Encoding.UTF8.GetBytes(myName);
        stream.Write(buffer, 0, buffer.Length);

        // Thread nhận tin nhắn từ server
        new Thread(() =>
        {
            while (true)
            {
                try
                {
                    byte[] buf = new byte[1024];
                    int bytesRead = stream.Read(buf, 0, buf.Length);
                    if (bytesRead > 0)
                    {
                        string msg = Encoding.UTF8.GetString(buf, 0, bytesRead);
                        Console.WriteLine(msg);
                    }
                }
                catch
                {
                    break;
                }
            }
        }).Start();

        // Gửi chat
        while (true)
        {
            string input = Console.ReadLine();
            if (input.StartsWith("/chat "))
            {
                string chatMsg = "[Chat] " + input.Substring(6);
                byte[] sendBuf = Encoding.UTF8.GetBytes(chatMsg);
                stream.Write(sendBuf, 0, sendBuf.Length);
            }
        }
    }
}