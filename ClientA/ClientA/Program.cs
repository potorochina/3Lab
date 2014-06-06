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
    static Thread threadA, threadSend;
    public static void StartClient()
    {
        // Буфер для входящих данных
        byte[] bytes;
        // Соединяемся с удаленным устройством
        try
        {
            // Устанавливаем удаленную точку для сокета
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

            // Create a TCP/IP  socket.
            sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Соединяем сокет с удаленной точкой
            try
            {
                sender.Connect(remoteEP);//comment
                Console.WriteLine("Socket connected to {0}",sender.RemoteEndPoint.ToString());
                //Чтение ключа А из файла
                byte[] keyA = File.ReadAllBytes("d:\\4 курс\\8 семестр\\Защиат информации\\3Lab\\keyA.txt");
                // Create a new instance of the AesCryptoServiceProvider 
                // class.  This generates a new key and initialization  
                // vector (IV).
                AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
                Console.WriteLine("Введите имя человека, с которым хотите общаться: ");
                string name = Console.ReadLine();
                //получение текущего системного времени и даты
                String time = DateTime.Now.ToString("MM.dd.yyyy HH:mm:ss");
                string message = "{" + time + ", " + name + ";" + Convert.ToBase64String(aes.Key) + "}";
                keyAES = aes.Key;
                IV = new byte[16];
                //отправка зашифрованного сообщения 
                sender.Send(EncryptStringToBytes_Aes(message, keyA, IV));
                //string data = null;

                threadA = new Thread(funcA);
                threadA.Start();
                while (true)
                {
                    Console.WriteLine("Введите сообщение для отправки: ");
                    string messageToB = Console.ReadLine();
                    byte[] encMessageToB = EncryptStringToBytes_Aes(messageToB, keyAES, IV);
                    threadA.Suspend();//приостановка потока
                    //отправка зашифрованного сообщения 
                    sender.Send(encMessageToB);
                    threadA.Resume();//возобновление работы потока
                }
               // threadSend = new Thread(send);
               // threadSend.Start();

               

                Console.Read();
                // Освобождаем сокет
              //  sender.Shutdown(SocketShutdown.Both);
               // sender.Close();
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

  /*  static void send()
    {
        
    }*/


    static void funcA()
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

    public static int Main(String[] args)
    {
        StartClient();
        return 0;
    }
}

