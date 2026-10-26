using System.Security.Cryptography;

namespace MaHoaVaChuKySo.Core;

public static class RsaCryptoService
{
    public static byte[] EncryptWithPublic(byte[] data, string publicPem)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(publicPem.AsSpan());
        int maxBytes = rsa.KeySize / 8 - 2 * 32 - 2;
        if (data is null || data.Length > maxBytes)
            throw new ArgumentException($"Du lieu phai tu 0 toi {maxBytes} byte voi RSA-OAEP-SHA256.");
        return rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA256);
    }

    public static byte[] DecryptWithPrivate(byte[] ciphertext, string privatePem)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(privatePem.AsSpan());
        return rsa.Decrypt(ciphertext, RSAEncryptionPadding.OaepSHA256);
    }
}
