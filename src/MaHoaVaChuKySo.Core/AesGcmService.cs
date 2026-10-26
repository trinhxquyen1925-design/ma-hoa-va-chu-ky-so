using System.Security.Cryptography;
using System.Text;

namespace MaHoaVaChuKySo.Core;

public static class AesGcmService
{
    public static string EncryptText(string plainText, byte[] key)
    {
        ValidateKey(key);
        var plainBytes = Encoding.UTF8.GetBytes(plainText ?? "");
        var nonce = new byte[12];
        RandomNumberGenerator.Fill(nonce);
        var cipher = new byte[plainBytes.Length];
        var tag = new byte[16];
        using var aes = new AesGcm(key, 16);
        aes.Encrypt(nonce, plainBytes, cipher, tag);
        var output = new byte[12 + 16 + cipher.Length];
        Buffer.BlockCopy(nonce, 0, output, 0, 12);
        Buffer.BlockCopy(tag, 0, output, 12, 16);
        Buffer.BlockCopy(cipher, 0, output, 28, cipher.Length);
        return Convert.ToBase64String(output);
    }

    public static string DecryptText(string base64, byte[] key)
    {
        ValidateKey(key);
        var data = Convert.FromBase64String(base64);
        if (data.Length < 28) throw new ArgumentException("Du lieu ma hoa qua ngan.");
        var nonce = data[0..12];
        var tag = data[12..28];
        var cipher = data[28..];
        var plain = new byte[cipher.Length];
        using var aes = new AesGcm(key, 16);
        try { aes.Decrypt(nonce, cipher, tag, plain); }
        catch (CryptographicException ex) { throw new ArgumentException("Giai ma that bai: sai khoa hoac du lieu da bi sua.", ex); }
        return Encoding.UTF8.GetString(plain);
    }

    private static void ValidateKey(byte[]? key)
    {
        if (key is null || key.Length != 32)
            throw new ArgumentException("Khoa AES phai dai 32 byte (AES-256).");
    }
}
