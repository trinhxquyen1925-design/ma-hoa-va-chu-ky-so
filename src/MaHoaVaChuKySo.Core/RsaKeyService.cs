using System.Security.Cryptography;

namespace MaHoaVaChuKySo.Core;

public static class RsaKeyService
{
    public static (string PublicPem, string PrivatePem) GenerateKeyPair(int keySize = 2048)
    {
        using var rsa = RSA.Create(keySize);
        return (rsa.ExportSubjectPublicKeyInfoPem(), rsa.ExportPkcs8PrivateKeyPem());
    }

    public static RSAParameters LoadPublicParameters(string pem)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(pem.AsSpan());
        return rsa.ExportParameters(false);
    }

    public static RSAParameters LoadPrivateParameters(string pem)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(pem.AsSpan());
        return rsa.ExportParameters(true);
    }

    public static void SavePem(string path, string pem)
    {
        var folder = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(folder)) Directory.CreateDirectory(folder);
        File.WriteAllText(path, pem);
    }

    public static string LoadPem(string path)
    {
        return File.ReadAllText(path);
    }

    public static long ExponentToLong(byte[]? exponent)
    {
        long value = 0;
        if (exponent is null) return value;
        foreach (var b in exponent) value = (value << 8) | b;
        return value;
    }
}
