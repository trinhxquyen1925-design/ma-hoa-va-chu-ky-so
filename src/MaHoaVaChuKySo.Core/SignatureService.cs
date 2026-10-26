using System.Security.Cryptography;

namespace MaHoaVaChuKySo.Core;

public static class SignatureService
{
    public static byte[] SignPss(byte[] data, string privatePem)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(privatePem.AsSpan());
        return rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
    }

    public static bool VerifyPss(byte[] data, byte[] signature, string publicPem)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(publicPem.AsSpan());
        return rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
    }

    public static byte[] SignPkcs1(byte[] data, string privatePem)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(privatePem.AsSpan());
        return rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }

    public static bool VerifyPkcs1(byte[] data, byte[] signature, string publicPem)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(publicPem.AsSpan());
        return rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }
}
