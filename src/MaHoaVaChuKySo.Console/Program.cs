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
