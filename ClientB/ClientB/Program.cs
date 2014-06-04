using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Net;
using System.Net.Sockets;

public class SynchronousSocketClient
{
    static byte[] keyAES;
    public static void StartClient()
    {
        // Data buffer for incoming data.
        byte[] bytes;
        // соединение с удаленным устройством
        try
        {
            // устанавливаем удаленную точку для сокета
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11001);

            // Create a TCP/IP  socket.
            Socket sender = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);

            // Соединяем сокет с удаленной точкой. Catch any errors.
            try
            {
                sender.Connect(remoteEP);
                Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());
                //чтение ключа В из файла
                byte[] keyB = File.ReadAllBytes("d:\\4 курс\\8 семестр\\Защиат информации\\3Lab\\keyB.txt");
                string data = null;
                while (true)
                {
                    bytes = new byte[1024];
                    //получаем ответ от сервера
                    int bytesRec = sender.Receive(bytes);
                    data = Encoding.UTF8.GetString(bytes, 0, bytesRec);
                    Console.WriteLine("Принято сообщение от сервера: {0}", data);
                    break;
                }
                Console.Read();
                // освобождаем сокет
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
    public static int Main(String[] args)
    {
        StartClient();
        return 0;
    }
}
