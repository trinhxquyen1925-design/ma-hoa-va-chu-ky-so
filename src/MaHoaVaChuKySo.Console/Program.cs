using MaHoaVaChuKySo.Core;
if (args.Length > 0 && args[0] == "benchmark")
{
    var mode = args.Length > 1 ? args[1] : "quick";
    AesBenchmarkService.Run(mode, AesKeyService.GenerateKey());
    return;
}


var key = AesKeyService.GenerateKey();
Console.WriteLine($"Da sinh khoa, do dai: {key.Length} byte");

var path = "keys/aes-test.key";
AesKeyService.SaveKey(key, path);
Console.WriteLine($"Da luu khoa vao: {path}");

var loaded = AesKeyService.LoadKey(path);
Console.WriteLine(loaded.SequenceEqual(key)
    ? "Kiem tra: PASS - khoa nap lai khop"
    : "Kiem tra: FAIL - khoa nap lai khong khop");

Console.WriteLine($"Khoa Base64 (chi de kiem tra, khoa test): {AesKeyService.ToBase64(key)}");
var text = "Xin chao AES-GCM, tieng Viet co dau!";
var encryptedText = AesGcmService.EncryptText(text, loaded);
Console.WriteLine($"Van ban goc: {text}");
Console.WriteLine($"Ban ma Base64: {encryptedText}");
var decryptedText = AesGcmService.DecryptText(encryptedText, loaded);
Console.WriteLine(decryptedText == text ? "Giai ma van ban: PASS" : "Giai ma van ban: FAIL");

var encryptedEmpty = AesGcmService.EncryptText("", loaded);
Console.WriteLine(AesGcmService.DecryptText(encryptedEmpty, loaded) == "" ? "Van ban rong: PASS" : "Van ban rong: FAIL");
Directory.CreateDirectory("test-data");
var samplePath = "test-data/sample.txt";
if (!File.Exists(samplePath)) File.WriteAllText(samplePath, "Day la file mau de thu ma hoa AES - tieng Viet co dau! ");
var encPath = "test-data/sample.enc";
var decPath = "test-data/sample.dec.txt";
AesFileService.EncryptFile(samplePath, encPath, loaded, overwrite: true);
AesFileService.DecryptFile(encPath, decPath, loaded, overwrite: true);
var originalBytes = File.ReadAllBytes(samplePath);
var decryptedBytes = File.ReadAllBytes(decPath);
Console.WriteLine(decryptedBytes.SequenceEqual(originalBytes) ? "Ma hoa file: PASS - giai ma khop goc" : "Ma hoa file: FAIL");
Console.WriteLine($"Kich thuoc: goc {originalBytes.Length} byte, ma hoa {new FileInfo(encPath).Length} byte, giai ma {decryptedBytes.Length} byte");
Console.WriteLine("--- SHA-256 avalanche ---");
Directory.CreateDirectory("results/experiments");
var pairs = new (string A, string B)[]
{
    ("hello", "hellp"),
    ("Xin chao", "Xin chbo"),
    ("An toàn", "An toan"),
    ("", "a"),
    ("Ma hoa", "Ma hoa"),
};
using (var csv = new StreamWriter("results/experiments/sha256-avalanche.csv", false, System.Text.Encoding.UTF8))
{
    csv.WriteLine("InputA,InputB,DifferentBits,Percent");
    foreach (var (a, b) in pairs)
    {
        var hashA = HashService.HashText(a);
        var hashB = HashService.HashText(b);
        var diff = HashService.CountDifferentBits(a, b);
        Console.WriteLine($"\"{a}\" vs \"{b}\": {diff}/256 bit ({diff / 256.0 * 100:F1}%)");
        Console.WriteLine($"  {hashA}");
        Console.WriteLine($"  {hashB}");
        csv.WriteLine($"\"{a}\",\"{b}\",{diff},{diff / 256.0 * 100:F1}");
    }
}
Console.WriteLine("Da ghi ket qua vao: results/experiments/sha256-avalanche.csv");
