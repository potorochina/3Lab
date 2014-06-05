using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class SynchronousSocketListener
{
    // Incoming data from the client.
    public static string data = null;
    static Socket handlerA, handlerB;
    public static void StartListening()
    {
        // Data buffer for incoming data.
        byte[] bytes = new Byte[16000];
        //устанавливаем для сокетов локальные точки А и В
        IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        IPEndPoint pointA = new IPEndPoint(ipAddress, 11000);
        IPEndPoint pointB = new IPEndPoint(ipAddress, 11001);
        // Create a TCP/IP socket.
        Socket listenerA = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
        Socket listenerB = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
        //string answer = null;
        byte[] aesKey = null;
        //answer = Console.ReadLine();
        // назначаем сокет локальной точке
        // и слушаем входящие сокеты
        try
        {
            listenerA.Bind(pointA);
            listenerA.Listen(10);
            listenerB.Bind(pointB);
            listenerB.Listen(10);
            // начинаем слушать соединения
            while (true)
            {
                Console.WriteLine("Waiting for a connection point A");
                // Программа приостанавливается, ожидая входящее соединение
                handlerA = listenerA.Accept();
                Console.WriteLine("Point A connected to server");
                Console.WriteLine("Waiting for a connection point B");
                handlerB = listenerB.Accept();
                Console.WriteLine("Point B connected to server");
                data = null;
                Console.WriteLine("Соединения с точкой А и B установлены.");
                //чтение ключей А и В из файлов
                byte[] keyA = File.ReadAllBytes("d:\\4 курс\\8 семестр\\Защиат информации\\3Lab\\keyA.txt");
                byte[] keyB = File.ReadAllBytes("d:\\4 курс\\8 семестр\\Защиат информации\\3Lab\\keyB.txt");
                byte[] IV = new byte[16];
                DateTime time;
                //ожидание клиента
                byte[] enc;
                while (true)
                {
                    bytes = new byte[4096];
                    int bytesRec = handlerA.Receive(bytes);
                    enc = new byte[bytesRec];
                    for (int i = 0; i < bytesRec; i++)
                        enc[i] = bytes[i];
                    time = DateTime.Now;//время приема сообщения для сравнения с временем в принятом сообщении
                    break;
                }
                string decrypt = DecryptStringFromBytes_Aes(enc, keyA, IV);
                Console.WriteLine("Принято сообщение от точки А: {0}", decrypt);
                string message_time = decrypt.Substring(1,19);//присвоение подстроки (время в принятом сообщении)
                string message_name = decrypt.Substring(22,3);//присвоение подстроки имени
                string message_key = decrypt.Substring(decrypt.IndexOf(';') + 1, decrypt.Length - decrypt.IndexOf(';') - 2);//присвоение подстроки ключа
                DateTime timeA = Convert.ToDateTime(message_time);//перевод строки в дату
               // DateComparisonResult comparison;//сравнение дат
                int numSeconds = (time - timeA).Seconds;
                System.TimeSpan diff = timeA.Subtract(time);
                if (numSeconds > 30)
                    Console.WriteLine("Полученное сообщение написано давно");
                else//если полученное сообщение написано недавно, формируем сообщение для В
                {
                    string nameA = "Alisa";//имя клиента А, который хочет связаться с клиентом В 
                    string message = "{" + time + ", "+nameA+"; "+ message_key + "}";//сообщение для В
                    //отправка зашифрованного сообщения 
                   handlerB.Send(EncryptStringToBytes_Aes(message, keyB, IV));
                    //handlerB.Send(Encoding.UTF8.GetBytes(data));
                }

                Thread threadA = new Thread(funcA);
                Thread threadB = new Thread(funcB);
                threadA.Start();
                threadB.Start();
                break;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
        Console.WriteLine("\nPress ENTER to continue...");
        Console.Read();
    }

    
    static void funcA()
    {
        while (true)
        {
            byte[] buff = new byte[2048];
            int countByte = handlerA.Receive(buff);
            byte[] mess = new byte[countByte];
            for (int i = 0; i < countByte; i++)
                mess[i] = buff[i];
            handlerB.Send(mess);
        }
    }

    static void funcB()
    {
        while (true)
        {
            byte[] buff = new byte[2048];
            int countByte = handlerB.Receive(buff);
            byte[] mess = new byte[countByte];
            for (int i = 0; i < countByte; i++)
                mess[i] = buff[i];
            handlerA.Send(mess);
        }
    }
    static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments. 
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold 
            // the decrypted text. 
            string plaintext = null;

            // Create an AesCryptoServiceProvider object 
            // with the specified key and IV. 
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption. 
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream 
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;

        }

//Шифрование сообщения
static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
{
    // Check arguments. 
    if (plainText == null || plainText.Length <= 0)
        throw new ArgumentNullException("plainText");
    if (Key == null || Key.Length <= 0)
        throw new ArgumentNullException("Key");
    if (IV == null || IV.Length <= 0)
        throw new ArgumentNullException("Key");
    byte[] encrypted;
    // Create an AesCryptoServiceProvider object 
    // with the specified key and IV. 
    using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
    {
        aesAlg.Key = Key;
        aesAlg.IV = IV;

        // Create a decrytor to perform the stream transform.
        ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
        // Create the streams used for encryption. 
        using (MemoryStream msEncrypt = new MemoryStream())
        {
            using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                {
                    //Write all data to the stream.
                    swEncrypt.Write(plainText);
                }
                encrypted = msEncrypt.ToArray();
            }
        }
    }
    // Return the encrypted bytes from the memory stream. 
    return encrypted;
}
    
    [STAThread]
    public static int Main(String[] args)
    {
        StartListening();
        return 0;
    }
}
