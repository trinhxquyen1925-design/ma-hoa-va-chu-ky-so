using System.Security.Cryptography;

namespace MaHoaVaChuKySo.Core;

public static class AesKeyService
{
    public static byte[] GenerateKey()
    {
        var key = new byte[32];
        RandomNumberGenerator.Fill(key);
        return key;
    }

    public static string ToBase64(byte[] key)
    {
        Validate(key);
        return Convert.ToBase64String(key);
    }

    public static byte[] FromBase64(string base64)
    {
        if (string.IsNullOrWhiteSpace(base64))
            throw new ArgumentException("Chuoi Base64 khong duoc rong.");
        byte[] key;
        try { key = Convert.FromBase64String(base64.Trim()); }
        catch (FormatException) { throw new ArgumentException("Chuoi Base64 khong hop le."); }
        Validate(key);
        return key;
    }

    public static void SaveKey(byte[] key, string path)
    {
        var fullPath = Path.GetFullPath(path);
        var folder = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(folder)) Directory.CreateDirectory(folder);
        File.WriteAllText(fullPath, ToBase64(key));
    }

    public static byte[] LoadKey(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("Khong tim thay file khoa.", path);
        return FromBase64(File.ReadAllText(path));
    }

    private static void Validate(byte[]? key)
    {
        if (key is null || key.Length != 32)
            throw new ArgumentException("Khoa AES phai dai 32 byte (AES-256).");
    }
}
