using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace Api
{
public class Crypto
{
    public static string Decode(string base64CipherText)
    {
        byte[] cipherText = System.Convert.FromBase64String(base64CipherText);

        // Check arguments.
        if (cipherText == null || cipherText.Length <= 0)
        {
            throw new ArgumentNullException("cipherText");
        }

        string dir = "Crypto/";
        byte[] key = (UnityEngine.Resources.Load(dir + "crypto_key") as UnityEngine.TextAsset).bytes;
        byte[] iv = (UnityEngine.Resources.Load(dir + "crypto_iv") as UnityEngine.TextAsset).bytes;
        Console.WriteLine("key length : {0}", key.Length);
        Console.WriteLine("iv length : {0}", iv.Length);

        if (key == null || key.Length <= 0)
        {
            throw new ArgumentNullException("Key");
        }

        if (iv == null || iv.Length <= 0)
        {
            throw new ArgumentNullException("IV");
        }

        // AES暗号化サービスプロバイダ
        RijndaelManaged aes = new RijndaelManaged();
        aes.BlockSize = 128; // 16 bytes
        aes.KeySize = 128;   // 16 bytes
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        // Declare the string used to hold
        // the decrypted text.
        string plaintext = null;

        // 複号化する
        using (ICryptoTransform decryptor = aes.CreateDecryptor())
        {
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

        //Debug.Log("Decode finish:" + plaintext);
        return plaintext;
    }
}
}

