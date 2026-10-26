using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace MaHoaVaChuKySo.Core;

public static class HashService
{
    public static string HashText(string text)
    {
        return Convert.ToHexString(HashBytes(text));
    }

    public static byte[] HashBytes(string text)
    {
        using var sha = SHA256.Create();
        return sha.ComputeHash(Encoding.UTF8.GetBytes(text ?? ""));
    }

    public static int CountDifferentBits(string textA, string textB)
    {
        var a = HashBytes(textA);
        var b = HashBytes(textB);
        int count = 0;
        for (int i = 0; i < a.Length; i++)
            count += BitOperations.PopCount((uint)(a[i] ^ b[i]));
        return count;
    }
}
