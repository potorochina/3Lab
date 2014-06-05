using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class SynchronousSocketClient
{
    static byte[] keyAES;
    static byte[] IV;
    static Socket sender;
    public static void StartClient()
    {
        // Data buffer for incoming data.
        byte[] bytes;
        IV = new byte[16];
        // соединение с удаленным устройством
        try
        {
            // устанавливаем удаленную точку для сокета
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11001);

            // Create a TCP/IP  socket.
            sender = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);

            // Соединяем сокет с удаленной точкой. Catch any errors.
            try
            {
                sender.Connect(remoteEP);
                Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());
                //чтение ключа В из файла
                byte[] keyB = File.ReadAllBytes("d:\\4 курс\\8 семестр\\Защиат информации\\3Lab\\keyB.txt");
                string data = null;
                byte[] enc;
                DateTime time;
                string timeB = "";
                while (true)
                {
                    bytes = new byte[4096];
                    //получаем ответ от сервера
                    int bytesRec = sender.Receive(bytes);
                    enc = new byte[bytesRec];
                   // data = Encoding.UTF8.GetString(bytes, 0, bytesRec);
                   // Console.WriteLine("Принято сообщение от сервера: {0}", data);
                    for (int i = 0; i < bytesRec; i++)
                        enc[i] = bytes[i];
                    time = DateTime.Now;//время приема сообщения для сравнения с временем в принятом сообщении
                    break;
                }
                string decryptB = DecryptStringFromBytes_Aes(enc, keyB, IV);//расшифровка сообщения от сервера
                Console.WriteLine("Принято сообщение от сервера: {0}", decryptB);
                string message_time = decryptB.Substring(1, 19);//присвоение подстроки (время в принятом сообщении)
                string message_name = decryptB.Substring(22, 5);//присвоение подстроки имени
                string message_key = decryptB.Substring(decryptB.IndexOf(';') + 1, decryptB.Length - decryptB.IndexOf(';') - 2);//присвоение подстроки ключа
                keyAES = Convert.FromBase64String(message_key);

                DateTime timeS = Convert.ToDateTime(message_time);//перевод строки в дату
                // DateComparisonResult comparison;//сравнение дат
                int numSeconds = (time - timeS).Seconds;
                System.TimeSpan diff = timeS.Subtract(time);
                if (numSeconds > 30)
                    Console.WriteLine("Полученное сообщение написано давно");
                else//отправляем приветствие Алисе
                    timeB = DateTime.Now.ToString("MM.dd.yyyy HH:mm:ss");
                    string message = "{Привет, Алиса. Буду рад с тобой пообщаться. Боб.}";//сообщение для A
                    byte[] encMessage = EncryptStringToBytes_Aes(message, Convert.FromBase64String(message_key), IV);
                    //отправка зашифрованного сообщения 
                   sender.Send(encMessage);

                   Thread threadB = new Thread(funcB);
                   threadB.Start();
                   while (true)
                   {
                       Console.WriteLine("Введите сообщение для отправки:");
                       string messageToA = Console.ReadLine();
                       byte[] encMessageToA = EncryptStringToBytes_Aes(messageToA, Convert.FromBase64String(message_key), IV);
                       //отправка зашифрованного сообщения 
                       sender.Send(encMessageToA);

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

    static void funcB()
    {
        while (true)
        {
            byte[] buff = new byte[2048];
            int countByte = sender.Receive(buff);
            byte[] mess = new byte[countByte];
            for (int i = 0; i < countByte; i++)
                mess[i] = buff[i];
            string decMessage = DecryptStringFromBytes_Aes(mess, keyAES, IV);
            Console.WriteLine("Принято сообщение: {0}", decMessage);
            Console.WriteLine("Введите сообщение для отправки:");
        }
    }

    public static int Main(String[] args)
    {
        StartClient();
        return 0;
    }
}
