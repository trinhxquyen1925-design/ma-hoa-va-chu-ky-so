using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MaHoaVaChuKySo.Core;

namespace MaHoaVaChuKySo.Desktop;

public partial class Form1 : Form
{
    private byte[]? _aesKey;
    private string? _rsaPublicPem;
    private string? _rsaPrivatePem;

    public Form1()
    {
        InitializeComponent();
        Text = "MaHoaVaChuKySo - Bo cong cu An toan thong tin";
        Size = new Size(900, 600);
        StartPosition = FormStartPosition.CenterScreen;
        BuildTabs();
    }

    private void BuildTabs()
    {
        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(BuildAesTab());
        tabs.TabPages.Add(BuildRsaTab());
        tabs.TabPages.Add(BuildHashTab());
        tabs.TabPages.Add(BuildSignatureTab());
        tabs.TabPages.Add(BuildBenchmarkTab());
        Controls.Add(tabs);
    }

    private static void Append(TextBox output, string line)
    {
        output.AppendText(line + Environment.NewLine);
    }

    private static TextBox MakeOutput(int top)
    {
        return new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Both,
            WordWrap = false,
            Font = new Font("Consolas", 10),
            Location = new Point(10, top),
            Size = new Size(860, 530 - top),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
        };
    }

    private static Button MakeButton(string text, int x, int y, EventHandler handler)
    {
        var button = new Button { Text = text, Location = new Point(x, y), Size = new Size(220, 40) };
        button.Click += handler;
        return button;
    }

    private byte[] AesKeyOrNew(TextBox output)
    {
        if (_aesKey is null)
        {
            _aesKey = AesKeyService.GenerateKey();
            Append(output, "Chua co khoa, da tu sinh khoa AES-256 moi.");
        }
        return _aesKey;
    }

    private (string, string) RsaKeysOrNew(TextBox output)
    {
        if (_rsaPublicPem is null || _rsaPrivatePem is null)
        {
            var pair = RsaKeyService.GenerateKeyPair(2048);
            _rsaPublicPem = pair.PublicPem;
            _rsaPrivatePem = pair.PrivatePem;
            RsaKeyService.SavePem("keys/rsa-public.pem", _rsaPublicPem);
            RsaKeyService.SavePem("keys/rsa-private.pem", _rsaPrivatePem);
            Append(output, "Chua co cap khoa, da tu sinh cap khoa RSA-2048 moi.");
        }
        return (_rsaPublicPem, _rsaPrivatePem);
    }

    private TabPage BuildAesTab()
    {
        var page = new TabPage("AES");
        var output = MakeOutput(60);
        var btnKey = MakeButton("Sinh khoá AES", 10, 10, (s, e) =>
        {
            try
            {
                _aesKey = AesKeyService.GenerateKey();
                Append(output, $"Da sinh khoa AES-256, dai {_aesKey.Length} byte.");
                Append(output, "Khoa Base64: " + AesKeyService.ToBase64(_aesKey));
                AesKeyService.SaveKey(_aesKey, "keys/aes-test.key");

                Append(output, "Da luu khoa vao keys/aes-test.key (thu muc nay bi gitignore).");
            }
            catch (Exception ex) { Append(output, "Loi: " + ex.Message); }
        });
        var btnText = MakeButton("Mã hoá văn bản mẫu", 240, 10, (s, e) =>
        {
            try
            {
                var key = AesKeyOrNew(output);
                var text = "Xin chào AES-GCM, tiếng Việt có dấu!";
                var cipher = AesGcmService.EncryptText(text, key);
                var plain = AesGcmService.DecryptText(cipher, key);
                Append(output, "Van ban goc: " + text);
                Append(output, "Ban ma Base64: " + cipher);
                Append(output, plain == text ? "Giai ma van ban: PASS" : "Giai ma van ban: FAIL");
            }
            catch (Exception ex) { Append(output, "Loi: " + ex.Message); }
        });
        var btnFile = MakeButton("Mã hoá file mẫu", 470, 10, (s, e) =>
        {
            try
            {
                var key = AesKeyOrNew(output);
                Directory.CreateDirectory("test-data");
                var sample = "test-data/ui-sample.txt";
                var enc = "test-data/ui-sample.enc";
                var dec = "test-data/ui-sample-dec.txt";
                File.WriteAllText(sample, "Noi dung file mau cho WinForms.", Encoding.UTF8);
              AesFileService.EncryptFile(sample, enc, key, true);

               AesFileService.DecryptFile(enc, dec, key, true);

                var ok = File.ReadAllBytes(sample).SequenceEqual(File.ReadAllBytes(dec));
                Append(output, $"File mau: goc {new FileInfo(sample).Length} byte, ma hoa {new FileInfo(enc).Length} byte, giai ma {new FileInfo(dec).Length} byte.");
                Append(output, ok ? "Ma hoa file: PASS - giai ma khop goc" : "Ma hoa file: FAIL");
            }
            catch (Exception ex) { Append(output, "Loi: " + ex.Message); }
        });
        page.Controls.AddRange(new Control[] { btnKey, btnText, btnFile, output });
        return page;
    }

    private TabPage BuildRsaTab()
    {
        var page = new TabPage("RSA");
        var output = MakeOutput(60);
        var btnGen = MakeButton("Sinh cặp khoá RSA", 10, 10, (s, e) =>
        {
            try
            {
                var pair = RsaKeyService.GenerateKeyPair(2048);
                _rsaPublicPem = pair.PublicPem;
                _rsaPrivatePem = pair.PrivatePem;
                RsaKeyService.SavePem("keys/rsa-public.pem", _rsaPublicPem);
                RsaKeyService.SavePem("keys/rsa-private.pem", _rsaPrivatePem);
                var pub = RsaKeyService.LoadPublicParameters(_rsaPublicPem);
                var priv = RsaKeyService.LoadPrivateParameters(_rsaPrivatePem);
                Append(output, "Public PEM: -----BEGIN PUBLIC KEY----- (SPKI)");
                Append(output, "Private PEM: -----BEGIN PRIVATE KEY----- (PKCS#8)");
                Append(output, $"Public: e = {RsaKeyService.ExponentToLong(pub.Exponent)}, n dai {pub.Modulus?.Length} byte");
                Append(output, $"Private: n dai {priv.Modulus?.Length} byte, d dai {priv.D?.Length} byte, p dai {priv.P?.Length} byte, q dai {priv.Q?.Length} byte");
                Append(output, "Nap lai PEM: PASS");
            }
            catch (Exception ex) { Append(output, "Loi: " + ex.Message); }
        });
        var btnEnc = MakeButton("Mã hoá khoá AES bằng RSA", 240, 10, (s, e) =>
        {
            try
            {
                var (pubPem, privPem) = RsaKeysOrNew(output);
                var aesKey = AesKeyService.GenerateKey();
                var cipher = RsaCryptoService.EncryptWithPublic(aesKey, pubPem);
                Directory.CreateDirectory("test-data");
                File.WriteAllText("test-data/rsa-encrypted-aes-key.b64", Convert.ToBase64String(cipher));
                var loaded = Convert.FromBase64String(File.ReadAllText("test-data/rsa-encrypted-aes-key.b64").Trim());
                var plain = RsaCryptoService.DecryptWithPrivate(loaded, privPem);
                Append(output, $"RSA ma hoa khoa AES 32 byte -> ciphertext {cipher.Length} byte (dung 256 byte voi RSA-2048)");
                Append(output, plain.SequenceEqual(aesKey) ? "RSA giai ma: PASS - khoa AES giai ma khop goc" : "RSA giai ma: FAIL");
            }
            catch (Exception ex) { Append(output, "Loi: " + ex.Message); }
        });
        page.Controls.AddRange(new Control[] { btnGen, btnEnc, output });
        return page;
    }

    private TabPage BuildHashTab()
    {
        var page = new TabPage("Hash");
        var input = new TextBox { Location = new Point(10, 10), Size = new Size(860, 30), Text = "Xin chào" };
        var output = MakeOutput(100);
        var btnHash = MakeButton("Băm SHA-256", 10, 50, (s, e) =>
        {
            try
            {
                var hash = HashService.HashText(input.Text);
                Append(output, "Van ban: " + input.Text);
                Append(output, "SHA-256: " + hash);
                Append(output, $"Do dai hash: {hash.Length} ky tu hex (256 bit).");
            }
            catch (Exception ex) { Append(output, "Loi: " + ex.Message); }
        });
        var btnAvalanche = MakeButton("Avalanche mẫu", 240, 50, (s, e) =>
        {
            try
            {
                var pairs = new (string, string)[]
                {
                    ("hello", "hellp"),
                    ("Xin chao", "Xin chbo"),
                    ("An toàn", "An toan"),
                    ("", "a"),
                    ("Ma hoa", "Ma hoa"),
                };
                foreach (var (a, b) in pairs)
                {
                    var diff = HashService.CountDifferentBits(a, b);
                    Append(output, $"\"{a}\" vs \"{b}\": {diff}/256 bit ({diff / 256.0 * 100:F1}%)");
                }
            }
            catch (Exception ex) { Append(output, "Loi: " + ex.Message); }
        });
        page.Controls.AddRange(new Control[] { input, btnHash, btnAvalanche, output });
        return page;
    }

    private TabPage BuildSignatureTab()
    {
        var page = new TabPage("Chữ ký");
        var output = MakeOutput(60);
        var btnPss = MakeButton("Ký PSS 2 lần", 10, 10, (s, e) =>
        {
            try
            {
                var (pubPem, privPem) = RsaKeysOrNew(output);
                var data = Encoding.UTF8.GetBytes("Van ban can ky: Bao cao mon An toan thong tin");
                var sig1 = SignatureService.SignPss(data, privPem);
                var sig2 = SignatureService.SignPss(data, privPem);
                Append(output, $"PSS: chu ky dai {sig1.Length} byte, 2 lan ky khac nhau: {(sig1.SequenceEqual(sig2) ? "FAIL" : "PASS")}");
                Append(output, (SignatureService.VerifyPss(data, sig1, pubPem) && SignatureService.VerifyPss(data, sig2, pubPem)) ? "PSS xac minh: PASS - ca 2 chu ky deu hop le" : "PSS xac minh: FAIL");
                var tampered = Encoding.UTF8.GetBytes("Van ban can ky: Bao cao mon An toan thong tin!");
                Append(output, !SignatureService.VerifyPss(tampered, sig1, pubPem) ? "PSS xac minh du lieu bi sua: PASS - bi tu choi" : "PSS xac minh du lieu bi sua: FAIL");
            }
            catch (Exception ex) { Append(output, "Loi: " + ex.Message); }
        });
        var btnPkcs = MakeButton("Ký PKCS#1 v1.5", 240, 10, (s, e) =>
        {
            try
            {
                var (pubPem, privPem) = RsaKeysOrNew(output);
                var data = Encoding.UTF8.GetBytes("Van ban can ky: Bao cao mon An toan thong tin");
                var sig1 = SignatureService.SignPkcs1(data, privPem);
                var sig2 = SignatureService.SignPkcs1(data, privPem);
                Append(output, $"PKCS#1 v1.5: chu ky dai {sig1.Length} byte, 2 lan ky giong het nhau: {(sig1.SequenceEqual(sig2) ? "PASS - xac dinh, khong co salt" : "FAIL")}");
                Append(output, SignatureService.VerifyPkcs1(data, sig1, pubPem) ? "PKCS#1 xac minh: PASS" : "PKCS#1 xac minh: FAIL");
            }
            catch (Exception ex) { Append(output, "Loi: " + ex.Message); }
        });
        page.Controls.AddRange(new Control[] { btnPss, btnPkcs, output });
        return page;
    }

    private TabPage BuildBenchmarkTab()
    {
        var page = new TabPage("Benchmark");
        var output = MakeOutput(60);
        var btnRsa = MakeButton("Benchmark RSA quick", 10, 10, async (s, e) =>
        {
            var button = (Button)s!;
            button.Enabled = false;
            try
            {
                Append(output, "Dang chay benchmark RSA quick, cho chut...");
                await Task.Run(() => RsaBenchmarkService.Run("quick"));
                Append(output, File.ReadAllText("results/benchmark/rsa-benchmark.csv"));
            }
            catch (Exception ex) { Append(output, "Loi: " + ex.Message); }
            finally { button.Enabled = true; }
        });
        var btnAes = MakeButton("Benchmark AES quick", 240, 10, async (s, e) =>
        {
            var button = (Button)s!;
            button.Enabled = false;
            try
            {
                Append(output, "Dang chay benchmark AES quick, cho chut...");
                await Task.Run(() => AesBenchmarkService.Run("quick", AesKeyService.GenerateKey()));
                Append(output, File.ReadAllText("results/benchmark/aes-benchmark.csv"));
                Append(output, "File test nam trong test-data/, da bi gitignore.");
            }
            catch (Exception ex) { Append(output, "Loi: " + ex.Message); }
            finally { button.Enabled = true; }
        });
        page.Controls.AddRange(new Control[] { btnRsa, btnAes, output });
        return page;
    }
}
