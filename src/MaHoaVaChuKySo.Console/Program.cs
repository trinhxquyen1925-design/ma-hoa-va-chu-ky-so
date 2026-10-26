using MaHoaVaChuKySo.Core;

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
