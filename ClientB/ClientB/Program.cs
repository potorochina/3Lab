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
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());//
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
                byte[] keyB = File.ReadAllBytes("c:\\gitproga\\3Lab\\keyB.txt");
                byte[] IV = new Byte[16];
                string data = null;
                DateTime timeB;
                byte[] enc;
                while (true) //ожидание сообщения от сервера
                {
                    bytes = new byte[4096];
                    //получаем ответ от сервера
                    int bytesRec = sender.Receive(bytes);
                    enc = new byte[bytesRec];
                    for (int i = 0; i < bytesRec; i++)
                        enc[i] = bytes[i];
                    timeB = DateTime.Now; //время приема сообщения клиента В от сервера
                    break;
                }
                string decryptB = DecryptStringFromBytes_Aes(enc,keyB,IV);
                Console.WriteLine("Принято сообщение от сервера: {0}", decryptB);
                string message_time = decryptB.Substring(1, decryptB.IndexOf(',') - 1); //присвоение подстроки времени из сообщения от сервера
                string message_key = decryptB.Substring(decryptB.IndexOf(';') - 1, decryptB.IndexOf('}') - 1);//присвоение подстроки ключа,созданного клиентом А
                DateTime timeS = Convert.ToDateTime(message_time);//перевод строки в дату
                int numSeconds = (timeB - timeS).Seconds;
                System.TimeSpan diff = timeS.Subtract(timeB);
                if (numSeconds > 30)
                    Console.WriteLine("Полученное сообщение написано давно");
                else
                    keyAES = Encoding.UTF8.GetBytes(message_key);//перевод из строки в байты сеансового ключа
                string time = DateTime.Now.ToString("MM.dd.yyyy HH:mm:ss"); 
                string message = "{" + time + "Hello Alisa. This is Bob"+"}";
               // hend
        


                   // break;
                
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

    public static int Main(String[] args)
    {
        StartClient();
        return 0;
    }
}
