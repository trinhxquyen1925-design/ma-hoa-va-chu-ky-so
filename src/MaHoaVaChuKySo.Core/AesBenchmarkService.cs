using System.Diagnostics;
using System.Security.Cryptography;

namespace MaHoaVaChuKySo.Core;

public static class AesBenchmarkService
{
    public static void Run(string mode, byte[] key)
    {
        if (mode != "quick" && mode != "full")
        {
            Console.WriteLine("Mode phai la quick hoac full.");
            return;
        }

        long[] sizes = mode == "full"
            ? new long[] { 100_000_000, 1_000_000_000 }
            : new long[] { 1_000_000, 10_000_000 };

        Directory.CreateDirectory("test-data");
        Directory.CreateDirectory("results/benchmark");
        var csvPath = "results/benchmark/aes-benchmark.csv";
        if (!File.Exists(csvPath))
            File.WriteAllText(csvPath, "SizeBytes,EncryptMs,DecryptMs,EncryptMBs,DecryptMBs,Integrity,ThoiGian\n");

        Console.WriteLine("Size | Encrypt ms | Decrypt ms | Encrypt MB/s | Decrypt MB/s | Integrity");
        foreach (var size in sizes)
        {
            var source = $"test-data/bench-{size}.bin";
            var enc = $"test-data/bench-{size}.enc";
            var dec = $"test-data/bench-{size}.dec";
            GenerateTestFile(source, size);

            var sw = Stopwatch.StartNew();
            AesFileService.EncryptFile(source, enc, key, overwrite: true);
            sw.Stop();
            var encMs = sw.Elapsed.TotalMilliseconds;

            sw.Restart();
            AesFileService.DecryptFile(enc, dec, key, overwrite: true);
            sw.Stop();
            var decMs = sw.Elapsed.TotalMilliseconds;

            var integrity = ComputeSha256(source) == ComputeSha256(dec) ? "PASS" : "FAIL";
            double mb = size / 1_000_000.0;
            double encSpeed = mb / (encMs / 1000.0);
            double decSpeed = mb / (decMs / 1000.0);
            Console.WriteLine($"{mb} MB | {encMs:F3} | {decMs:F3} | {encSpeed:F2} | {decSpeed:F2} | {integrity}");
            File.AppendAllText(csvPath, $"{size},{encMs:F3},{decMs:F3},{encSpeed:F2},{decSpeed:F2},{integrity},{DateTime.Now:yyyy-MM-dd HH:mm}\n");
        }
        Console.WriteLine($"Da ghi ket qua vao: {csvPath}");
    }

    private static void GenerateTestFile(string path, long sizeBytes)
    {
        using var output = new FileStream(path, FileMode.Create, FileAccess.Write);
        var buffer = new byte[131072];
        long remaining = sizeBytes;
        while (remaining > 0)
        {
            int chunk = (int)Math.Min(buffer.Length, remaining);
            RandomNumberGenerator.Fill(buffer.AsSpan(0, chunk));
            output.Write(buffer, 0, chunk);
            remaining -= chunk;
        }
    }

    private static string ComputeSha256(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(stream));
    }
}
