using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services;

public interface ICacheEncryptionService
{
    byte[] Encrypt(byte[] data, string key);
    byte[] Decrypt(byte[] encryptedData, string key);
    string EncryptString(string text, string key);
    string DecryptString(string encryptedText, string key);
}

public class CacheEncryptionService(
    ILogger<CacheEncryptionService> logger
   ) : ICacheEncryptionService
{
    public byte[] Encrypt(byte[] data, string key)
    {
        if (data == null || data.Length == 0)
            return data!;

        try
        {
            using var aes = Aes.Create();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            Array.Resize(ref keyBytes, 32); // AES-256 needs a 32-byte key
            aes.Key = keyBytes;
            
            aes.GenerateIV();
            var iv = aes.IV;
            
            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                cs.Write(data, 0, data.Length);
            }
            
            var encrypted = ms.ToArray();
            
            // Prepend IV to encrypted data for decryption
            var result = new byte[iv.Length + encrypted.Length];
            Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
            Buffer.BlockCopy(encrypted, 0, result, iv.Length, encrypted.Length);
            
            return result;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to encrypt data, returning original data");
            return data;
        }
    }

    public byte[] Decrypt(byte[] encryptedData, string key)
    {
        if (encryptedData == null || encryptedData.Length <= 16) // IV is 16 bytes
            return encryptedData!;

        try
        {
            using var aes = Aes.Create();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            Array.Resize(ref keyBytes, 32);
            aes.Key = keyBytes;
            
            // Extract IV from the beginning of encrypted data
            var iv = new byte[16];
            Buffer.BlockCopy(encryptedData, 0, iv, 0, 16);
            aes.IV = iv;
            
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(encryptedData, 16, encryptedData.Length - 16);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var result = new MemoryStream();
            cs.CopyTo(result);
            return result.ToArray();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to decrypt data, returning encrypted data");
            return encryptedData;
        }
    }

    public string EncryptString(string text, string key)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        try
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            var encrypted = Encrypt(bytes, key);
            return Convert.ToBase64String(encrypted);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to encrypt string, returning original string");
            return text;
        }
    }

    public string DecryptString(string encryptedText, string key)
    {
        if (string.IsNullOrEmpty(encryptedText))
            return encryptedText;

        try
        {
            var encrypted = Convert.FromBase64String(encryptedText);
            var decrypted = Decrypt(encrypted, key);
            return Encoding.UTF8.GetString(decrypted);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to decrypt string, returning encrypted string");
            return encryptedText;
        }
    }
}
