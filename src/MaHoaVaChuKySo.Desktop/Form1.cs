using System.Drawing;
using System.Windows.Forms;

namespace MaHoaVaChuKySo.Desktop;

public partial class Form1 : Form
{
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
        tabs.TabPages.Add(MakeTab("AES", "Tab AES: sinh khoa AES-256, ma hoa van ban bang AES-GCM, ma hoa file streaming CBC-HMAC.\nSe noi voi Core o commit sau."));
        tabs.TabPages.Add(MakeTab("RSA", "Tab RSA: sinh cap khoa, xuat/nhap PEM, ma hoa khoa AES 32 byte bang OAEP-SHA256.\nSe noi voi Core o commit sau."));
        tabs.TabPages.Add(MakeTab("Hash", "Tab Hash: bam SHA-256 va do avalanche khi doi 1 ky tu.\nSe noi voi Core o commit sau."));
        tabs.TabPages.Add(MakeTab("Chu ky", "Tab Chu ky: ky so RSA-PSS, cung du lieu ky 2 lan khac nhau, doi chieu PKCS#1 v1.5.\nSe noi voi Core o commit sau."));
        tabs.TabPages.Add(MakeTab("Benchmark", "Tab Benchmark: do thoi gian AES theo kich thuoc file va RSA theo so lan / kich thuoc khoa.\nSe noi voi Core o commit sau."));
        Controls.Add(tabs);
    }

    private static TabPage MakeTab(string title, string text)
    {
        var page = new TabPage(title);
        var label = new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = false
        };
        page.Controls.Add(label);
        return page;
    }
}
