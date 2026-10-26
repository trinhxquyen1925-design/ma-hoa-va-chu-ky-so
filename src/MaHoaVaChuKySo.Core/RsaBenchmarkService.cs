using System.Diagnostics;

namespace MaHoaVaChuKySo.Core;

public static class RsaBenchmarkService
{
    public static void Run(string mode)
    {
        bool full = mode == "full";
        int[] keySizes = full ? new[] { 2048, 3072 } : new[] { 2048 };
        int[] iterationsList = full ? new[] { 500, 5000 } : new[] { 50 };
        Console.WriteLine($"--- RSA benchmark ({mode}) ---");
        Directory.CreateDirectory("results/benchmark");
        var csvPath = "results/benchmark/rsa-benchmark.csv";
        bool fileExists = File.Exists(csvPath);
        using var writer = new StreamWriter(csvPath, append: true, System.Text.Encoding.UTF8);
        if (!fileExists) writer.WriteLine("KeySize,Iterations,EncryptMs,DecryptMs,EncryptOpsPerSec,DecryptOpsPerSec,Integrity");
        foreach (var keySize in keySizes)
        {
            var (publicPem, privatePem) = RsaKeyService.GenerateKeyPair(keySize);
            var data = AesKeyService.GenerateKey();
            var testCipher = RsaCryptoService.EncryptWithPublic(data, publicPem);
            var testPlain = RsaCryptoService.DecryptWithPrivate(testCipher, privatePem);
            var integrity = testPlain.SequenceEqual(data) ? "PASS" : "FAIL";
            var cipher = RsaCryptoService.EncryptWithPublic(data, publicPem);
            foreach (var iterations in iterationsList)
            {
                var sw = Stopwatch.StartNew();
                for (int i = 0; i < iterations; i++) RsaCryptoService.EncryptWithPublic(data, publicPem);
                sw.Stop();
                double encryptMs = sw.Elapsed.TotalMilliseconds;
                sw.Restart();
                for (int i = 0; i < iterations; i++) RsaCryptoService.DecryptWithPrivate(cipher, privatePem);
                sw.Stop();
                double decryptMs = sw.Elapsed.TotalMilliseconds;
                double encryptOps = iterations / (encryptMs / 1000.0);
                double decryptOps = iterations / (decryptMs / 1000.0);
                Console.WriteLine($"{keySize} bit, {iterations} lan: ma hoa {encryptMs:F3} ms ({encryptOps:F1} op/s), giai ma {decryptMs:F3} ms ({decryptOps:F1} op/s), {integrity}");
                writer.WriteLine($"{keySize},{iterations},{encryptMs:F3},{decryptMs:F3},{encryptOps:F1},{decryptOps:F1},{integrity}");
            }
        }
        Console.WriteLine("Da ghi ket qua vao: results/benchmark/rsa-benchmark.csv");
    }
}
