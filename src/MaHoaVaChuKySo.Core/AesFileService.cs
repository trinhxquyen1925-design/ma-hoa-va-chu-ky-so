using System.Security.Cryptography;
using System.Text;

namespace MaHoaVaChuKySo.Core;

public static class AesFileService
{
    public static void EncryptFile(string sourcePath, string destPath, byte[] masterKey, bool overwrite = false)
    {
        ValidateMaster(masterKey);
        if (!File.Exists(sourcePath)) throw new FileNotFoundException("Khong tim thay file nguon.", sourcePath);
        if (Path.GetFullPath(sourcePath) == Path.GetFullPath(destPath)) throw new ArgumentException("File nguon va file dich phai khac nhau.");
        if (File.Exists(destPath) && !overwrite) throw new IOException("File dich da ton tai.");

        var encKey = Derive(masterKey, "MHCKS-ENC");
        var macKey = Derive(masterKey, "MHCKS-MAC");
        var iv = new byte[16];
        RandomNumberGenerator.Fill(iv);
        var tempPath = destPath + ".tmp";

        try
        {
            using var aes = Aes.Create();
            aes.Key = encKey;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (var input = new FileStream(sourcePath, FileMode.Open, FileAccess.Read))
            using (var temp = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
            {
                temp.Write(iv, 0, iv.Length);
                using var crypto = new CryptoStream(temp, aes.CreateEncryptor(), CryptoStreamMode.Write);
                input.CopyTo(crypto, 131072);
            }

            byte[] mac;
            using (var macHash = IncrementalHash.CreateHMAC(HashAlgorithmName.SHA256, macKey))
            using (var tempRead = new FileStream(tempPath, FileMode.Open, FileAccess.Read))
            {
                var buffer = new byte[131072];
                int read;
                while ((read = tempRead.Read(buffer, 0, buffer.Length)) > 0)
                    macHash.AppendData(new ReadOnlySpan<byte>(buffer, 0, read));
                mac = macHash.GetHashAndReset();
            }

            using (var output = new FileStream(destPath, FileMode.Create, FileAccess.Write))
            {
                output.Write(Encoding.ASCII.GetBytes("MHCKS"));
                output.WriteByte(0x01);
                output.Write(mac, 0, mac.Length);
                using var tempRead = new FileStream(tempPath, FileMode.Open, FileAccess.Read);
                tempRead.CopyTo(output, 131072);
            }
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    public static void DecryptFile(string sourcePath, string destPath, byte[] masterKey, bool overwrite = false)
    {
        ValidateMaster(masterKey);
        if (!File.Exists(sourcePath)) throw new FileNotFoundException("Khong tim thay file ma hoa.", sourcePath);
        if (Path.GetFullPath(sourcePath) == Path.GetFullPath(destPath)) throw new ArgumentException("File nguon va file dich phai khac nhau.");
        if (File.Exists(destPath) && !overwrite) throw new IOException("File dich da ton tai.");

        var encKey = Derive(masterKey, "MHCKS-ENC");
        var macKey = Derive(masterKey, "MHCKS-MAC");

        using var input = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
        var header = new byte[6];
        ReadExact(input, header, 6);
        if (Encoding.ASCII.GetString(header, 0, 5) != "MHCKS" || header[5] != 0x01)
            throw new InvalidDataException("Khong phai file MHCKS v1.");
        var macStored = new byte[32];
        ReadExact(input, macStored, 32);
        var iv = new byte[16];
        ReadExact(input, iv, 16);

        using (var macHash = IncrementalHash.CreateHMAC(HashAlgorithmName.SHA256, macKey))
        {
            macHash.AppendData(iv);
            var buffer = new byte[131072];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                macHash.AppendData(new ReadOnlySpan<byte>(buffer, 0, read));
            var macComputed = macHash.GetHashAndReset();
            if (!CryptographicOperations.FixedTimeEquals(macStored, macComputed))
                throw new CryptographicException("Xac minh HMAC that bai: file da bi sua hoac sai khoa.");
        }

        input.Seek(54, SeekOrigin.Begin);
        using var aes = Aes.Create();
        aes.Key = encKey;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        try
        {
            using var output = new FileStream(destPath, FileMode.Create, FileAccess.Write);
            using var crypto = new CryptoStream(input, aes.CreateDecryptor(), CryptoStreamMode.Read);
            crypto.CopyTo(output, 131072);
        }
        catch
        {
            if (File.Exists(destPath)) File.Delete(destPath);
            throw;
        }
    }

    private static byte[] Derive(byte[] masterKey, string label)
    {
        var labelBytes = Encoding.UTF8.GetBytes(label);
        var input = new byte[labelBytes.Length + masterKey.Length];
        Buffer.BlockCopy(labelBytes, 0, input, 0, labelBytes.Length);
        Buffer.BlockCopy(masterKey, 0, input, labelBytes.Length, masterKey.Length);
        return SHA256.HashData(input);
    }

    private static void ValidateMaster(byte[]? key)
    {
        if (key is null || key.Length != 32)
            throw new ArgumentException("Khoa AES phai dai 32 byte (AES-256).");
    }

    private static void ReadExact(Stream stream, byte[] buffer, int count)
    {
        int offset = 0;
        while (offset < count)
        {
            int read = stream.Read(buffer, offset, count - offset);
            if (read == 0) throw new InvalidDataException("File ma hoa bi cut.");
            offset += read;
        }
    }
}
