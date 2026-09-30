using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;
using System.Windows.Forms;

// AppCrate : catalogue d'applications a cocher, installation silencieuse via winget.
// Interface moderne dessinee a la main (WinForms). Compatible C# 5 (csc.exe integre a Windows).

// =====================================================================
//  Langues : fichiers lang\xx.txt integres dans l'exe (format cle=valeur)
// =====================================================================
static class AppInfo
{
    public const string Version = "1.3";
}

static class L
{
    static readonly Dictionary<string, Dictionary<string, string>> data = new Dictionary<string, Dictionary<string, string>>();
    static readonly List<string> codes = new List<string>();
    static Dictionary<string, string> cur = new Dictionary<string, string>();
    static Dictionary<string, string> fallback = new Dictionary<string, string>();
    static string lang = "en";

    static L()
    {
        try
        {
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            foreach (string res in asm.GetManifestResourceNames())
            {
                if (!res.StartsWith("lang.") || !res.EndsWith(".txt")) continue;
                string code = res.Substring(5, res.Length - 9);
                using (Stream s = asm.GetManifestResourceStream(res))
                    data[code] = Parse(s);
            }
        }
        catch (Exception) { }
        if (!data.ContainsKey("en")) data["en"] = new Dictionary<string, string>();
        fallback = data["en"];

        string[] order = new string[] { "fr", "en", "es", "it", "de", "pt", "nl", "ru", "zh", "ja", "ko" };
        foreach (string c in order) if (data.ContainsKey(c)) codes.Add(c);
        List<string> rest = new List<string>();
        foreach (string c in data.Keys) if (!codes.Contains(c)) rest.Add(c);
        rest.Sort();
        codes.AddRange(rest);

        string sys = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        Lang = data.ContainsKey(sys) ? sys : "en";
    }

    static Dictionary<string, string> Parse(Stream s)
    {
        Dictionary<string, string> d = new Dictionary<string, string>();
        using (StreamReader r = new StreamReader(s, Encoding.UTF8))
        {
            string line;
            while ((line = r.ReadLine()) != null)
            {
                if (line.Length == 0 || line[0] == '#') continue;
                int eq = line.IndexOf('=');
                if (eq <= 0) continue;
                d[line.Substring(0, eq).Trim()] = line.Substring(eq + 1).Trim().Replace("\\n", "\n");
            }
        }
        return d;
    }

    public static List<string> Codes { get { return codes; } }

    public static bool Has(string code) { return data.ContainsKey(code); }

    public static string NameOf(string code)
    {
        string n;
        return data[code].TryGetValue("name", out n) ? n : code;
    }

    public static string Lang
    {
        get { return lang; }
        set
        {
            if (!data.ContainsKey(value)) value = "en";
            lang = value;
            cur = data[value];
        }
    }

    // Renvoie null si la cle n'existe ni dans la langue courante ni en anglais
    public static string Try(string key)
    {
        string v;
        if (cur.TryGetValue(key, out v)) return v;
        if (fallback.TryGetValue(key, out v)) return v;
        return null;
    }

    public static string Get(string key)
    {
        return Try(key) ?? key;
    }

    public static string Get(string key, params object[] args)
    {
        return string.Format(Get(key), args);
    }
}

// Parametres enregistres dans %LocalAppData%\AppCrate\settings.ini
static class Settings
{
    public static string Dir
    {
        get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AppCrate"); }
    }

    static string FilePath { get { return Path.Combine(Dir, "settings.ini"); } }

    // Renvoie true si une langue a deja ete enregistree
    public static bool Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return false;
            foreach (string line in File.ReadAllLines(FilePath))
            {
                string[] p = line.Split(new char[] { '=' }, 2);
                if (p.Length == 2 && p[0].Trim() == "language")
                {
                    string v = p[1].Trim();
                    if (L.Has(v)) { L.Lang = v; return true; }
                }
            }
        }
        catch (Exception) { }
        return false;
    }

    public static void Save()
    {
        try
        {
            Directory.CreateDirectory(Dir);
            File.WriteAllText(FilePath, "language=" + L.Lang + Environment.NewLine);
        }
        catch (Exception) { }
    }
}

static class Native
{
    [DllImport("dwmapi.dll")]
    static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int val, int size);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

    public static void DarkTitle(IntPtr h)
    {
        try
        {
            int on = 1;
            if (DwmSetWindowAttribute(h, 20, ref on, 4) != 0) DwmSetWindowAttribute(h, 19, ref on, 4);
        }
        catch (Exception) { }
    }
}

// =====================================================================
//  Theme et outils de dessin
// =====================================================================
static class Theme
{
    public static readonly Color Bg = Color.FromArgb(24, 25, 30);
    public static readonly Color Side = Color.FromArgb(18, 19, 23);
    public static readonly Color Card = Color.FromArgb(38, 40, 48);
    public static readonly Color CardHover = Color.FromArgb(47, 50, 60);
    public static readonly Color Border = Color.FromArgb(58, 61, 72);
    public static readonly Color Text = Color.FromArgb(238, 240, 245);
    public static readonly Color Muted = Color.FromArgb(140, 146, 160);
    public static readonly Color Accent = Color.FromArgb(76, 141, 255);
    public static readonly Color AccentHover = Color.FromArgb(105, 162, 255);
    public static readonly Color Green = Color.FromArgb(52, 199, 120);
    public static readonly Color Orange = Color.FromArgb(255, 171, 64);
    public static readonly Color Red = Color.FromArgb(255, 95, 95);

    public static readonly Font F9 = new Font("Segoe UI", 9f);
    public static readonly Font F85 = new Font("Segoe UI", 8.5f);
    public static readonly Font F85B = new Font("Segoe UI", 8.5f, FontStyle.Bold);
    public static readonly Font F10 = new Font("Segoe UI", 10f);
    public static readonly Font F10B = new Font("Segoe UI", 10f, FontStyle.Bold);
    public static readonly Font F11B = new Font("Segoe UI Semibold", 11f, FontStyle.Bold);
    public static readonly Font F14B = new Font("Segoe UI", 14f, FontStyle.Bold);
    public static readonly Font Mono = new Font("Consolas", 9f);

    public static readonly Color[] Palette = new Color[]
    {
        Color.FromArgb(76, 141, 255), Color.FromArgb(155, 89, 255), Color.FromArgb(255, 120, 90),
        Color.FromArgb(40, 180, 110), Color.FromArgb(240, 160, 50), Color.FromArgb(0, 170, 205),
        Color.FromArgb(226, 62, 140), Color.FromArgb(110, 134, 146)
    };
}

static class Gfx
{
    public static GraphicsPath Round(Rectangle r, int rad)
    {
        int d = rad * 2;
        GraphicsPath p = new GraphicsPath();
        p.AddArc(r.X, r.Y, d, d, 180, 90);
        p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        p.CloseFigure();
        return p;
    }

    public static void Smooth(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
    }

    public static StringFormat Fmt(StringAlignment h, StringAlignment v)
    {
        StringFormat f = new StringFormat(StringFormatFlags.NoWrap);
        f.Alignment = h;
        f.LineAlignment = v;
        f.Trimming = StringTrimming.EllipsisCharacter;
        return f;
    }

    public static void FillRound(Graphics g, Rectangle r, int rad, Color c)
    {
        using (GraphicsPath gp = Round(r, rad))
        using (Brush b = new SolidBrush(c))
            g.FillPath(b, gp);
    }
}

// =====================================================================
//  Controles personnalises
// =====================================================================
class DBControl : Control
{
    public DBControl()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        SetStyle(ControlStyles.Selectable, false);
        SetStyle(ControlStyles.StandardDoubleClick, false);
    }

    protected Color ParentBack
    {
        get { return Parent != null ? Parent.BackColor : Theme.Bg; }
    }
}

class DBFlow : FlowLayoutPanel
{
    public DBFlow()
    {
        DoubleBuffered = true;
    }
}

enum St { None, Waiting, Running, Done, Already, Failed }

// Carte d'une application
class AppCard : DBControl
{
    public string AppName, AppId, Category;
    string description = "";
    Image icon;
    bool installed;
    bool isChecked;
    St status;
    bool hover;
    public event EventHandler CheckedChanged;

    public bool IsChecked
    {
        get { return isChecked; }
        set
        {
            if (isChecked == value) return;
            isChecked = value;
            Invalidate();
            if (CheckedChanged != null) CheckedChanged(this, EventArgs.Empty);
        }
    }

    public St Status
    {
        get { return status; }
        set { status = value; Invalidate(); }
    }

    public bool Installed
    {
        get { return installed; }
        set { installed = value; Invalidate(); }
    }

    public Image Icon
    {
        get { return icon; }
        set { icon = value; Invalidate(); }
    }

    public string Description
    {
        get { return description; }
        set { description = value; Invalidate(); }
    }

    public AppCard()
    {
        Size = new Size(296, 96);
        Margin = new Padding(6);
        Cursor = Cursors.Hand;
    }

    protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        IsChecked = !IsChecked;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.Clear(ParentBack);
        Gfx.Smooth(g);

        Rectangle r = new Rectangle(1, 1, Width - 3, Height - 3);
        bool hot = hover && Enabled;
        using (GraphicsPath gp = Gfx.Round(r, 12))
        {
            using (Brush b = new SolidBrush(hot ? Theme.CardHover : Theme.Card)) g.FillPath(b, gp);
            using (Pen pen = new Pen(isChecked ? Theme.Accent : Theme.Border, isChecked ? 2f : 1f)) g.DrawPath(pen, gp);
        }

        // Logo (ou pastille avec l'initiale si le logo n'est pas disponible)
        Rectangle ic = new Rectangle(14, (Height - 44) / 2, 44, 44);
        if (icon != null)
        {
            Gfx.FillRound(g, ic, 10, Color.FromArgb(244, 245, 250));
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.DrawImage(icon, new Rectangle(ic.X + 7, ic.Y + 7, ic.Width - 14, ic.Height - 14));
        }
        else
        {
            int idx = (AppName == null ? 0 : (AppName.GetHashCode() & 0x7fffffff)) % Theme.Palette.Length;
            using (Brush b = new SolidBrush(Theme.Palette[idx])) g.FillEllipse(b, ic);
            string letter = (AppName != null && AppName.Length > 0) ? AppName.Substring(0, 1).ToUpper() : "?";
            using (StringFormat sf = Gfx.Fmt(StringAlignment.Center, StringAlignment.Center))
            using (Brush b = new SolidBrush(Color.White))
                g.DrawString(letter, Theme.F14B, b, new RectangleF(ic.X, ic.Y, ic.Width, ic.Height), sf);
        }

        // Pastille verte : deja installe sur ce PC
        if (installed)
        {
            Rectangle bd = new Rectangle(ic.Right - 12, ic.Bottom - 12, 18, 18);
            using (Brush ring = new SolidBrush(hot ? Theme.CardHover : Theme.Card))
                g.FillEllipse(ring, bd.X - 2, bd.Y - 2, bd.Width + 4, bd.Height + 4);
            using (Brush gb = new SolidBrush(Theme.Green)) g.FillEllipse(gb, bd);
            using (Pen cp = new Pen(Color.White, 1.8f))
            {
                cp.StartCap = LineCap.Round;
                cp.EndCap = LineCap.Round;
                cp.LineJoin = LineJoin.Round;
                g.DrawLines(cp, new Point[] {
                    new Point(bd.X + 4, bd.Y + 9), new Point(bd.X + 7, bd.Y + 12), new Point(bd.X + 13, bd.Y + 5) });
            }
        }

        // Nom
        using (StringFormat sf = Gfx.Fmt(StringAlignment.Near, StringAlignment.Center))
        using (Brush b = new SolidBrush(Theme.Text))
            g.DrawString(AppName, Theme.F10B, b, new RectangleF(68, 8, Width - 68 - 44, 26), sf);

        // Description, ou etat de l'installation
        string text = description;
        Font tf = Theme.F85;
        Color tc = Theme.Muted;
        switch (status)
        {
            case St.Waiting: text = L.Get("st_waiting"); tf = Theme.F85B; break;
            case St.Running: text = L.Get("st_running"); tf = Theme.F85B; tc = Theme.Accent; break;
            case St.Done: text = L.Get("st_done"); tf = Theme.F85B; tc = Theme.Green; break;
            case St.Already: text = L.Get("st_already"); tf = Theme.F85B; tc = Theme.Green; break;
            case St.Failed: text = L.Get("st_failed"); tf = Theme.F85B; tc = Theme.Red; break;
        }
        using (StringFormat wf = new StringFormat())
        using (Brush b = new SolidBrush(tc))
        {
            wf.Trimming = StringTrimming.EllipsisWord;
            wf.FormatFlags = StringFormatFlags.LineLimit;
            g.DrawString(text ?? "", tf, b, new RectangleF(68, 36, Width - 68 - 14, Height - 36 - 8), wf);
        }

        // Case a cocher
        Rectangle cb = new Rectangle(Width - 36, 11, 22, 22);
        Color cbFill = Theme.Accent;
        if (status == St.Done || status == St.Already) cbFill = Theme.Green;
        else if (status == St.Failed) cbFill = Theme.Red;
        if (isChecked)
        {
            Gfx.FillRound(g, cb, 6, cbFill);
            using (Pen p = new Pen(Color.White, 2.4f))
            {
                p.StartCap = LineCap.Round;
                p.EndCap = LineCap.Round;
                p.LineJoin = LineJoin.Round;
                g.DrawLines(p, new Point[] {
                    new Point(cb.X + 5, cb.Y + 11), new Point(cb.X + 9, cb.Y + 15), new Point(cb.X + 17, cb.Y + 7) });
            }
        }
        else
        {
            using (GraphicsPath gp = Gfx.Round(cb, 6))
            using (Pen p = new Pen(Theme.Muted, 1.5f))
                g.DrawPath(p, gp);
        }
    }
}

// Titre de categorie (clic = tout cocher / decocher)
class CatHeader : DBControl
{
    public string Key;
    string title = "";
    public int SelCount, Total;
    public event EventHandler Toggled;

    public string Title
    {
        get { return title; }
        set { title = value; Invalidate(); }
    }

    public CatHeader()
    {
        Height = 36;
        Margin = new Padding(6, 14, 6, 0);
        Cursor = Cursors.Hand;
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        if (Toggled != null) Toggled(this, EventArgs.Empty);
    }

    protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.Clear(ParentBack);
        Gfx.Smooth(g);

        using (Brush b = new SolidBrush(Theme.Text))
        using (StringFormat sf = Gfx.Fmt(StringAlignment.Near, StringAlignment.Center))
            g.DrawString(title, Theme.F11B, b, new RectangleF(4, 0, Width - 160, Height - 4), sf);

        string right = (SelCount == Total && Total > 0) ? L.Get("deselect_all") : L.Get("select_all");
        using (Brush b = new SolidBrush(Enabled ? Theme.Accent : Theme.Muted))
        using (StringFormat sf = Gfx.Fmt(StringAlignment.Far, StringAlignment.Center))
            g.DrawString(right, Theme.F9, b, new RectangleF(Width - 156, 0, 152, Height - 4), sf);

        using (Pen p = new Pen(Theme.Border, 1f))
            g.DrawLine(p, 4, Height - 3, Width - 4, Height - 3);
    }
}

// Element de la barre laterale
class NavItem : DBControl
{
    string title = "";
    public bool Selected;
    int count;
    bool hover;

    public string Title
    {
        get { return title; }
        set { title = value; Invalidate(); }
    }

    public int Count
    {
        get { return count; }
        set { count = value; Invalidate(); }
    }

    public NavItem()
    {
        Height = 40;
        Cursor = Cursors.Hand;
    }

    protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.Clear(Theme.Side);
        Gfx.Smooth(g);

        Rectangle r = new Rectangle(10, 2, Width - 20, Height - 4);
        if (Selected) Gfx.FillRound(g, r, 8, Color.FromArgb(38, 42, 56));
        else if (hover) Gfx.FillRound(g, r, 8, Color.FromArgb(28, 30, 36));

        if (Selected)
            Gfx.FillRound(g, new Rectangle(r.X, r.Y + 9, 4, r.Height - 18), 2, Theme.Accent);

        using (Brush b = new SolidBrush(Selected ? Theme.Text : Theme.Muted))
        using (StringFormat sf = Gfx.Fmt(StringAlignment.Near, StringAlignment.Center))
            g.DrawString(title, Selected ? Theme.F10B : Theme.F10, b,
                new RectangleF(r.X + 18, r.Y, r.Width - 70, r.Height), sf);

        if (count > 0)
        {
            Rectangle pill = new Rectangle(r.Right - 40, r.Y + (r.Height - 20) / 2, 32, 20);
            Gfx.FillRound(g, pill, 10, Selected ? Theme.Accent : Color.FromArgb(52, 56, 68));
            using (Brush b = new SolidBrush(Color.White))
            using (StringFormat sf = Gfx.Fmt(StringAlignment.Center, StringAlignment.Center))
                g.DrawString(count.ToString(), Theme.F85, b, new RectangleF(pill.X, pill.Y, pill.Width, pill.Height), sf);
        }
    }
}

// Bouton arrondi
class RoundButton : DBControl
{
    public Color Fill = Theme.Card;
    public Color FillHover = Theme.CardHover;
    bool hover;

    public RoundButton()
    {
        Cursor = Cursors.Hand;
        Height = 44;
    }

    protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }
    protected override void OnTextChanged(EventArgs e) { Invalidate(); base.OnTextChanged(e); }

    public void SetColors(Color fill, Color hoverFill)
    {
        Fill = fill;
        FillHover = hoverFill;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.Clear(ParentBack);
        Gfx.Smooth(g);
        Color c = !Enabled ? Color.FromArgb(45, 47, 55) : (hover ? FillHover : Fill);
        Gfx.FillRound(g, new Rectangle(0, 0, Width - 1, Height - 1), 10, c);
        using (Brush b = new SolidBrush(Enabled ? Color.White : Theme.Muted))
        using (StringFormat sf = Gfx.Fmt(StringAlignment.Center, StringAlignment.Center))
            g.DrawString(Text, Theme.F10B, b, new RectangleF(0, 0, Width, Height), sf);
    }
}

// Barre de progression fine
class SlimProgress : DBControl
{
    int value, maximum = 100;

    public int Value { get { return value; } set { this.value = value; Invalidate(); } }
    public int Maximum { get { return maximum; } set { maximum = Math.Max(1, value); Invalidate(); } }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.Clear(ParentBack);
        Gfx.Smooth(g);
        Gfx.FillRound(g, new Rectangle(0, 0, Width - 1, Height - 1), Height / 2, Color.FromArgb(45, 47, 56));
        int w = (int)((Width - 1) * (double)Math.Min(value, maximum) / maximum);
        if (w > Height)
            Gfx.FillRound(g, new Rectangle(0, 0, w, Height - 1), Height / 2, Theme.Accent);
    }
}

// Champ de recherche arrondi
class SearchBox : Panel
{
    public TextBox Input;

    public SearchBox()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Input = new TextBox();
        Input.BorderStyle = BorderStyle.None;
        Input.BackColor = Theme.Card;
        Input.ForeColor = Theme.Text;
        Input.Font = Theme.F10;
        Controls.Add(Input);
    }

    protected override void OnResize(EventArgs eventargs)
    {
        base.OnResize(eventargs);
        Input.SetBounds(38, Math.Max(0, (Height - Input.Height) / 2), Math.Max(10, Width - 38 - 14), Input.Height);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.Clear(BackColor);
        Gfx.Smooth(g);
        Gfx.FillRound(g, new Rectangle(0, 0, Width - 1, Height - 1), 10, Theme.Card);
        int cy = Height / 2;
        using (Pen p = new Pen(Theme.Muted, 1.8f))
        {
            p.EndCap = LineCap.Round;
            g.DrawEllipse(p, 14, cy - 8, 12, 12);
            g.DrawLine(p, 24, cy + 3, 28, cy + 8);
        }
    }
}

// Permet de faire defiler la liste avec la molette sans avoir a cliquer dessus
class WheelFilter : IMessageFilter
{
    [DllImport("user32.dll")]
    static extern IntPtr WindowFromPoint(Point p);
    [DllImport("user32.dll")]
    static extern IntPtr SendMessage(IntPtr h, int m, IntPtr w, IntPtr l);

    public bool PreFilterMessage(ref Message m)
    {
        if (m.Msg == 0x020A)
        {
            IntPtr h = WindowFromPoint(Cursor.Position);
            if (h != IntPtr.Zero && h != m.HWnd && Control.FromHandle(h) != null)
            {
                SendMessage(h, m.Msg, m.WParam, m.LParam);
                return true;
            }
        }
        return false;
    }
}

// =====================================================================
//  Logos : telecharges une fois, puis gardes en cache local
// =====================================================================
static class IconLoader
{
    static readonly Dictionary<string, Image> mem = new Dictionary<string, Image>();

    static string Dir { get { return Path.Combine(Settings.Dir, "icons"); } }

    // {0} = site complet (avec chemin), {1} = nom d'hote seul. Le 2e champ = largeur minimale acceptee.
    static readonly string[][] Sources = new string[][]
    {
        new string[] { "https://t3.gstatic.com/faviconV2?client=SOCIAL&type=FAVICON&fallback_opts=TYPE,SIZE,URL&url=https://{0}&size=128", "32" },
        new string[] { "https://t3.gstatic.com/faviconV2?client=SOCIAL&type=FAVICON&fallback_opts=TYPE,SIZE,URL&url=https://{1}&size=128", "32" },
        new string[] { "https://{1}/apple-touch-icon.png", "64" },
        new string[] { "https://www.{1}/apple-touch-icon.png", "64" },
        new string[] { "https://www.google.com/s2/favicons?sz=128&domain_url=https://{1}", "32" },
        new string[] { "https://icons.duckduckgo.com/ip3/{1}.ico", "24" },
        new string[] { "https://{1}/favicon.ico", "16" },
        new string[] { "https://www.{1}/favicon.ico", "16" }
    };

    static string Safe(string s)
    {
        foreach (char c in Path.GetInvalidFileNameChars()) s = s.Replace(c, '_');
        return s.Replace('.', '_');
    }

    static Image FromBytes(byte[] data)
    {
        try
        {
            using (MemoryStream ms = new MemoryStream(data))
            using (Image img = Image.FromStream(ms))
                return new Bitmap(img);
        }
        catch (Exception) { return null; }
    }

    static byte[] Download(string url)
    {
        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
        req.Timeout = 6000;
        req.ReadWriteTimeout = 6000;
        req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppCrate";
        req.AllowAutoRedirect = true;
        using (WebResponse resp = req.GetResponse())
        using (Stream s = resp.GetResponseStream())
        using (MemoryStream ms = new MemoryStream())
        {
            byte[] buf = new byte[8192];
            int n;
            while ((n = s.Read(buf, 0, buf.Length)) > 0)
            {
                ms.Write(buf, 0, n);
                if (ms.Length > 3000000) break;
            }
            return ms.ToArray();
        }
    }

    public static Image Load(string site)
    {
        lock (mem) { Image known; if (mem.TryGetValue(site, out known)) return known; }

        string host = site;
        int slash = site.IndexOf('/');
        if (slash > 0) host = site.Substring(0, slash);

        Image result = null;
        try
        {
            Directory.CreateDirectory(Dir);
            string file = Path.Combine(Dir, Safe(site) + ".png");
            if (File.Exists(file))
            {
                result = FromBytes(File.ReadAllBytes(file));
                if (result == null) { try { File.Delete(file); } catch (Exception) { } }
            }
            if (result == null)
            {
                foreach (string[] src in Sources)
                {
                    try
                    {
                        byte[] data = Download(string.Format(src[0], site, host));
                        Image img = FromBytes(data);
                        if (img == null) continue;
                        if (img.Width >= int.Parse(src[1]))
                        {
                            try { File.WriteAllBytes(file, data); } catch (Exception) { }
                            result = img;
                            break;
                        }
                        img.Dispose();
                    }
                    catch (Exception) { }
                }
            }
        }
        catch (Exception) { }
        if (result != null) lock (mem) { mem[site] = result; }
        return result;
    }

    public static void ClearCache()
    {
        lock (mem) { mem.Clear(); }
        try
        {
            if (Directory.Exists(Dir))
                foreach (string f in Directory.GetFiles(Dir, "*.png")) { try { File.Delete(f); } catch (Exception) { } }
        }
        catch (Exception) { }
    }
}

class Entry
{
    public string Name, Id, Category, Site;
    public AppCard Card;

    public string Desc { get { return L.Try("app." + Id) ?? ""; } }
}

// =====================================================================
//  Fenetres de dialogue (langue au premier lancement, parametres)
// =====================================================================
// Icone de l'application (integree a l'exe)
static class AppIcon
{
    static Icon icon;

    public static Icon Get()
    {
        if (icon != null) return icon;
        try
        {
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            using (Stream s = asm.GetManifestResourceStream("AppCrate.ico"))
            {
                if (s != null) { icon = new Icon(s); return icon; }
            }
        }
        catch (Exception) { }
        try { icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); }
        catch (Exception) { }
        return icon;
    }
}

class DarkForm : Form
{
    public DarkForm()
    {
        BackColor = Theme.Bg;
        ForeColor = Theme.Text;
        Font = Theme.F9;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        Icon = AppIcon.Get();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        Native.DarkTitle(Handle);
    }
}

class LangForm : DarkForm
{
    public LangForm()
    {
        Text = "AppCrate";
        StartPosition = FormStartPosition.CenterScreen;
        ShowInTaskbar = true;

        int cols = 3, bw = 150, bh = 46, gap = 12, margin = 40, top = 120;
        int n = L.Codes.Count;
        int rows = (n + cols - 1) / cols;
        int width = cols * bw + (cols - 1) * gap + margin * 2;
        ClientSize = new Size(width, top + rows * (bh + gap) + 70);

        Label t = new Label();
        t.Text = "AppCrate";
        t.Font = new Font("Segoe UI", 22f, FontStyle.Bold);
        t.ForeColor = Theme.Text;
        t.AutoSize = false;
        t.TextAlign = ContentAlignment.MiddleCenter;
        t.SetBounds(0, 20, width, 50);
        Controls.Add(t);

        Label q = new Label();
        q.Text = L.Get("choose_lang");
        q.Font = Theme.F10;
        q.ForeColor = Theme.Muted;
        q.AutoSize = false;
        q.TextAlign = ContentAlignment.MiddleCenter;
        q.SetBounds(0, 76, width, 24);
        Controls.Add(q);

        for (int i = 0; i < n; i++)
        {
            string code = L.Codes[i];
            RoundButton b = new RoundButton();
            b.Text = L.NameOf(code);
            b.SetBounds(margin + (i % cols) * (bw + gap), top + (i / cols) * (bh + gap), bw, bh);
            if (code == L.Lang) b.SetColors(Theme.Accent, Theme.AccentHover);
            else b.SetColors(Theme.Card, Theme.CardHover);
            b.Click += delegate { Pick(code); };
            Controls.Add(b);
        }

        Label hint = new Label();
        hint.Text = L.Get("lang_hint");
        hint.Font = Theme.F85;
        hint.ForeColor = Theme.Muted;
        hint.AutoSize = false;
        hint.TextAlign = ContentAlignment.MiddleCenter;
        hint.SetBounds(20, top + rows * (bh + gap) + 10, width - 40, 40);
        Controls.Add(hint);
    }

    void Pick(string lang)
    {
        L.Lang = lang;
        DialogResult = DialogResult.OK;
        Close();
    }
}

class SettingsForm : DarkForm
{
    Label title, lblLang, lblSel, lblLogos, lblAbout;
    List<RoundButton> langBtns = new List<RoundButton>();
    RoundButton btnExport, btnImport, btnClear, btnClose;
    Action onLang, onClear, onExport, onImport;

    Label MakeLabel(Font f, Color c, int x, int y)
    {
        Label l = new Label();
        l.Font = f;
        l.ForeColor = c;
        l.AutoSize = true;
        l.Location = new Point(x, y);
        Controls.Add(l);
        return l;
    }

    RoundButton MakeButton(int x, int y, int w)
    {
        RoundButton b = new RoundButton();
        b.SetBounds(x, y, w, 46);
        Controls.Add(b);
        return b;
    }

    public SettingsForm(Action onLanguageChanged, Action onClearLogos, Action onExportSelection, Action onImportSelection)
    {
        onLang = onLanguageChanged;
        onClear = onClearLogos;
        onExport = onExportSelection;
        onImport = onImportSelection;
        StartPosition = FormStartPosition.CenterParent;

        title = MakeLabel(Theme.F14B, Theme.Text, 24, 20);

        int y = 78;
        lblLang = MakeLabel(Theme.F10B, Theme.Muted, 26, y);
        y += 28;
        int cols = 3, gap = 10, bh = 40;
        int bw = (412 - gap * (cols - 1)) / cols;
        for (int i = 0; i < L.Codes.Count; i++)
        {
            string code = L.Codes[i];
            RoundButton b = MakeButton(24 + (i % cols) * (bw + gap), y + (i / cols) * (bh + gap), bw);
            b.Height = bh;
            b.Text = L.NameOf(code);
            b.Click += delegate { SetLang(code); };
            langBtns.Add(b);
        }
        int rows = (L.Codes.Count + cols - 1) / cols;
        y += rows * (bh + gap) + 14;

        lblSel = MakeLabel(Theme.F10B, Theme.Muted, 26, y);
        y += 28;
        btnExport = MakeButton(24, y, 200);
        btnExport.Click += delegate { onExport(); };
        btnImport = MakeButton(236, y, 200);
        btnImport.Click += delegate { onImport(); };
        y += 46 + 24;

        lblLogos = MakeLabel(Theme.F10B, Theme.Muted, 26, y);
        y += 28;
        btnClear = MakeButton(24, y, 412);
        btnClear.Click += delegate
        {
            btnClear.Text = L.Get("logos_reloading");
            onClear();
        };
        y += 46 + 22;

        lblAbout = MakeLabel(Theme.F9, Theme.Muted, 26, y);
        y += 30;

        btnClose = MakeButton(24, y, 412);
        btnClose.SetColors(Theme.Accent, Theme.AccentHover);
        btnClose.Click += delegate { Close(); };

        ClientSize = new Size(460, y + 46 + 24);
        Retext();
    }

    void Retext()
    {
        Text = L.Get("settings");
        title.Text = L.Get("settings");
        lblLang.Text = L.Get("language");
        lblSel.Text = L.Get("selection");
        lblLogos.Text = L.Get("logos");
        btnExport.Text = L.Get("export_sel");
        btnImport.Text = L.Get("import_sel");
        btnClear.Text = L.Get("clear_logos");
        btnClose.Text = L.Get("close");
        lblAbout.Text = L.Get("about", AppInfo.Version);
        for (int i = 0; i < langBtns.Count; i++)
        {
            if (L.Codes[i] == L.Lang) langBtns[i].SetColors(Theme.Accent, Theme.AccentHover);
            else langBtns[i].SetColors(Theme.Card, Theme.CardHover);
        }
    }

    void SetLang(string lang)
    {
        if (L.Lang == lang) return;
        L.Lang = lang;
        Settings.Save();
        onLang();
        Retext();
    }
}

// =====================================================================
//  Fenetre principale
// =====================================================================
public class MainForm : Form
{
    // Catalogue.
    //   "#Categorie" ouvre une categorie (le nom affiche vient de lang\xx.txt : cat.<Categorie>)
    //   "Nom|Identifiant winget|site web (pour le logo)"  (description : app.<Identifiant> dans lang\xx.txt)
    // Pour trouver l'identifiant d'une appli : "winget search nom" dans un terminal.
    static readonly string[] DATA = new string[]
    {
        "#Navigateurs",
        "Google Chrome|Google.Chrome|google.com/chrome",
        "Mozilla Firefox|Mozilla.Firefox|mozilla.org/firefox",
        "Brave|Brave.Brave|brave.com",
        "Opera|Opera.Opera|opera.com",
        "Vivaldi|Vivaldi.Vivaldi|vivaldi.com",

        "#Messagerie",
        "Discord|Discord.Discord|discord.com",
        "Zoom|Zoom.Zoom|zoom.us",
        "Microsoft Teams|Microsoft.Teams|microsoft.com/microsoft-teams/group-chat-software",
        "Telegram|Telegram.TelegramDesktop|telegram.org",
        "Signal|OpenWhisperSystems.Signal|signal.org",
        "Slack|SlackTechnologies.Slack|slack.com",

        "#Multimédia",
        "VLC|VideoLAN.VLC|videolan.org",
        "Spotify|Spotify.Spotify|spotify.com",
        "Audacity|Audacity.Audacity|audacityteam.org",
        "OBS Studio|OBSProject.OBSStudio|obsproject.com",
        "HandBrake|HandBrake.HandBrake|handbrake.fr",
        "MPC-HC|clsid2.mpc-hc|mpc-hc.org",
        "foobar2000|PeterPawlowski.foobar2000|foobar2000.org",
        "Kodi|XBMCFoundation.Kodi|kodi.tv",

        "#Images et graphisme",
        "GIMP|GIMP.GIMP|gimp.org",
        "Inkscape|Inkscape.Inkscape|inkscape.org",
        "Krita|Krita.Krita|krita.org",
        "Blender|BlenderFoundation.Blender|blender.org",
        "Paint.NET|dotPDN.PaintDotNet|getpaint.net",
        "IrfanView|IrfanSkiljan.IrfanView|irfanview.com",
        "ShareX|ShareX.ShareX|getsharex.com",
        "Greenshot|Greenshot.Greenshot|getgreenshot.org",

        "#Bureautique",
        "LibreOffice|TheDocumentFoundation.LibreOffice|libreoffice.org",
        "Adobe Acrobat Reader|Adobe.Acrobat.Reader.64-bit|adobe.com/acrobat",
        "SumatraPDF|SumatraPDF.SumatraPDF|sumatrapdfreader.org",
        "Notepad++|Notepad++.Notepad++|notepad-plus-plus.org",
        "Obsidian|Obsidian.Obsidian|obsidian.md",
        "Notion|Notion.Notion|notion.so",

        "#Utilitaires",
        "7-Zip|7zip.7zip|7-zip.org",
        "WinRAR|RARLab.WinRAR|win-rar.com",
        "Everything|voidtools.Everything|voidtools.com",
        "PowerToys|Microsoft.PowerToys|microsoft.com",
        "qBittorrent|qBittorrent.qBittorrent|qbittorrent.org",
        "Bitwarden|Bitwarden.Bitwarden|bitwarden.com",
        "KeePassXC|KeePassXCTeam.KeePassXC|keepassxc.org",
        "AnyDesk|AnyDesk.AnyDesk|anydesk.com",
        "TeamViewer|TeamViewer.TeamViewer|teamviewer.com",
        "Rufus|Rufus.Rufus|rufus.ie",
        "balenaEtcher|Balena.Etcher|etcher.balena.io",
        "WinDirStat|WinDirStat.WinDirStat|windirstat.net",
        "Malwarebytes|Malwarebytes.Malwarebytes|malwarebytes.com",

        "#Infos système",
        "CPU-Z|CPUID.CPU-Z|cpuid.com",
        "HWMonitor|CPUID.HWMonitor|cpuid.com",
        "GPU-Z|TechPowerUp.GPU-Z|techpowerup.com",
        "CrystalDiskInfo|CrystalDewWorld.CrystalDiskInfo|crystalmark.info",
        "CrystalDiskMark|CrystalDewWorld.CrystalDiskMark|crystalmark.info",

        "#Développement",
        "Visual Studio Code|Microsoft.VisualStudioCode|code.visualstudio.com",
        "Git|Git.Git|git-scm.com",
        "GitHub Desktop|GitHub.GitHubDesktop|desktop.github.com",
        "Python 3.12|Python.Python.3.12|python.org",
        "Node.js LTS|OpenJS.NodeJS.LTS|nodejs.org",
        "Windows Terminal|Microsoft.WindowsTerminal|microsoft.com",
        "Docker Desktop|Docker.DockerDesktop|docker.com",
        "IntelliJ IDEA Community|JetBrains.IntelliJIDEA.Community|jetbrains.com/idea",
        "Postman|Postman.Postman|postman.com",
        "WinSCP|WinSCP.WinSCP|winscp.net",
        "PuTTY|PuTTY.PuTTY|putty.org",
        "Java JDK 21 (Temurin)|EclipseAdoptium.Temurin.21.JDK|adoptium.net",

        "#Jeux",
        "Steam|Valve.Steam|store.steampowered.com",
        "Epic Games Launcher|EpicGames.EpicGamesLauncher|epicgames.com",
        "GOG Galaxy|GOG.Galaxy|gog.com",
        "Ubisoft Connect|Ubisoft.Connect|ubisoft.com",
        "EA app|ElectronicArts.EADesktop|ea.com",

        "#Stockage cloud",
        "Dropbox|Dropbox.Dropbox|dropbox.com",
        "Google Drive|Google.GoogleDrive|drive.google.com",
        "Nextcloud|Nextcloud.NextcloudDesktop|nextcloud.com",

        "#Runtimes",
        "Visual C++ 2015-2022 x64|Microsoft.VCRedist.2015+.x64|microsoft.com",
        "Visual C++ 2015-2022 x86|Microsoft.VCRedist.2015+.x86|microsoft.com",
        ".NET Desktop Runtime 8|Microsoft.DotNet.DesktopRuntime.8|dotnet.microsoft.com",
        "Java Runtime (JRE)|Oracle.JavaRuntimeEnvironment|java.com"
    };

    List<Entry> entries = new List<Entry>();
    List<string> categories = new List<string>();
    Dictionary<string, CatHeader> headers = new Dictionary<string, CatHeader>();
    List<NavItem> navs = new List<NavItem>();

    Panel header, footer, sidebar, logPanel;
    DBFlow flow;
    SearchBox search;
    Label lblCount, lblStatus, lblEmpty, subLabel, sideTitle;
    SlimProgress prog;
    RoundButton btnInstall, btnClear, btnLog, btnSettings;
    TextBox log;
    ToolTip tip = new ToolTip();
    WheelFilter wheel = new WheelFilter();

    string filterCat;          // null = toutes
    string query = "";
    bool bulk, installing;
    volatile bool cancel;

    public MainForm()
    {
        Text = "AppCrate";
        Icon = AppIcon.Get();
        BackColor = Theme.Bg;
        ForeColor = Theme.Text;
        Font = Theme.F9;
        DoubleBuffered = true;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1200, 760);
        MinimumSize = new Size(1000, 620);
        KeyPreview = true;
        KeyDown += OnKey;

        // ---- Zone centrale (cartes)
        flow = new DBFlow();
        flow.Dock = DockStyle.Fill;
        flow.AutoScroll = true;
        flow.WrapContents = true;
        flow.FlowDirection = FlowDirection.LeftToRight;
        flow.BackColor = Theme.Bg;
        flow.Padding = new Padding(16, 6, 6, 16);

        lblEmpty = new Label();
        lblEmpty.ForeColor = Theme.Muted;
        lblEmpty.Font = Theme.F10;
        lblEmpty.Height = 40;
        lblEmpty.Visible = false;
        lblEmpty.Margin = new Padding(10, 20, 0, 0);

        // ---- Catalogue
        string cur = null;
        foreach (string line in DATA)
        {
            if (line.Length == 0) continue;
            if (line[0] == '#')
            {
                cur = line.Substring(1);
                categories.Add(cur);
                CatHeader h = new CatHeader();
                h.Key = cur;
                h.Toggled += OnHeaderToggled;
                headers[cur] = h;
                flow.Controls.Add(h);
                flow.SetFlowBreak(h, true);
            }
            else if (cur != null)
            {
                string[] p = line.Split('|');
                Entry en = new Entry();
                en.Name = p[0];
                en.Id = p[1];
                en.Site = p.Length > 2 ? p[2] : null;
                en.Category = cur;
                AppCard c = new AppCard();
                c.AppName = en.Name;
                c.AppId = en.Id;
                c.Category = cur;
                c.CheckedChanged += delegate { if (!bulk) RefreshCounts(); };
                en.Card = c;
                entries.Add(en);
                flow.Controls.Add(c);
            }
        }
        flow.Controls.Add(lblEmpty);
        flow.Resize += delegate { LayoutHeaders(); };

        // ---- Journal (masque par defaut)
        logPanel = new Panel();
        logPanel.Dock = DockStyle.Bottom;
        logPanel.Height = 170;
        logPanel.BackColor = Color.FromArgb(14, 15, 18);
        logPanel.Padding = new Padding(16, 10, 10, 10);
        logPanel.Visible = false;
        log = new TextBox();
        log.Multiline = true;
        log.ReadOnly = true;
        log.ScrollBars = ScrollBars.Vertical;
        log.BorderStyle = BorderStyle.None;
        log.BackColor = Color.FromArgb(14, 15, 18);
        log.ForeColor = Color.FromArgb(200, 205, 215);
        log.Font = Theme.Mono;
        log.Dock = DockStyle.Fill;
        logPanel.Controls.Add(log);

        // ---- Barre laterale
        sidebar = new Panel();
        sidebar.Dock = DockStyle.Left;
        sidebar.Width = 230;
        sidebar.BackColor = Theme.Side;

        sideTitle = new Label();
        sideTitle.ForeColor = Theme.Muted;
        sideTitle.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
        sideTitle.AutoSize = true;
        sideTitle.Location = new Point(24, 16);
        sidebar.Controls.Add(sideTitle);

        AddNav(null, 0);
        for (int i = 0; i < categories.Count; i++) AddNav(categories[i], i + 1);
        navs[0].Selected = true;

        // ---- Pied de page
        footer = new Panel();
        footer.Dock = DockStyle.Bottom;
        footer.Height = 96;
        footer.BackColor = Theme.Side;

        lblCount = new Label();
        lblCount.Font = Theme.F11B;
        lblCount.ForeColor = Theme.Text;
        lblCount.AutoSize = true;
        lblCount.Location = new Point(24, 14);

        lblStatus = new Label();
        lblStatus.Font = Theme.F9;
        lblStatus.ForeColor = Theme.Muted;
        lblStatus.AutoEllipsis = true;
        lblStatus.Location = new Point(24, 40);
        lblStatus.Size = new Size(400, 20);

        prog = new SlimProgress();
        prog.Height = 6;

        btnInstall = new RoundButton();
        btnInstall.SetColors(Theme.Accent, Theme.AccentHover);
        btnInstall.Click += delegate { OnInstallClick(); };

        btnClear = new RoundButton();
        btnClear.Click += delegate { SetAll(false); };

        btnLog = new RoundButton();
        btnLog.Click += delegate { logPanel.Visible = !logPanel.Visible; };

        footer.Controls.Add(lblCount);
        footer.Controls.Add(lblStatus);
        footer.Controls.Add(prog);
        footer.Controls.Add(btnInstall);
        footer.Controls.Add(btnClear);
        footer.Controls.Add(btnLog);
        footer.Resize += delegate { LayoutFooter(); };

        // ---- En-tete
        header = new Panel();
        header.Dock = DockStyle.Top;
        header.Height = 82;
        header.BackColor = Theme.Side;

        Label title = new Label();
        title.Text = "AppCrate";
        title.Font = new Font("Segoe UI", 20f, FontStyle.Bold);
        title.ForeColor = Theme.Text;
        title.AutoSize = true;
        title.Location = new Point(20, 8);

        subLabel = new Label();
        subLabel.Font = Theme.F9;
        subLabel.ForeColor = Theme.Muted;
        subLabel.AutoSize = true;
        subLabel.Location = new Point(24, 54);

        search = new SearchBox();
        search.BackColor = Theme.Side;
        search.Size = new Size(300, 40);
        search.Input.TextChanged += delegate
        {
            query = search.Input.Text.Trim();
            ApplyFilter();
        };

        btnSettings = new RoundButton();
        btnSettings.Height = 40;
        btnSettings.Click += delegate { OpenSettings(); };

        header.Controls.Add(title);
        header.Controls.Add(subLabel);
        header.Controls.Add(search);
        header.Controls.Add(btnSettings);
        header.Resize += delegate { LayoutHeader(); };

        // Ordre d'ajout important pour le "docking" : le dernier ajoute est place en premier.
        Controls.Add(flow);
        Controls.Add(logPanel);
        Controls.Add(sidebar);
        Controls.Add(footer);
        Controls.Add(header);

        Application.AddMessageFilter(wheel);
        FormClosed += delegate { Application.RemoveMessageFilter(wheel); };
        FormClosing += OnClosing;
        Load += delegate { OnLoaded(); };
    }

    string CatName(string key)
    {
        return L.Try("cat." + key) ?? key;
    }

    void AddNav(string cat, int index)
    {
        NavItem n = new NavItem();
        n.SetBounds(0, 40 + index * 42, sidebar.Width, 40);
        n.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        n.Click += delegate
        {
            filterCat = cat;
            foreach (NavItem x in navs) x.Selected = (x == n);
            foreach (NavItem x in navs) x.Invalidate();
            ApplyFilter();
        };
        navs.Add(n);
        sidebar.Controls.Add(n);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        Native.DarkTitle(Handle);
    }

    void OnLoaded()
    {
        LayoutFooter();
        LayoutHeader();
        ApplyLanguage();
        LayoutHeaders();
        StartIconLoading();

        try
        {
            ProcessStartInfo psi = new ProcessStartInfo("winget", "--version");
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.CreateNoWindow = true;
            using (Process p = Process.Start(psi))
            {
                string v = p.StandardOutput.ReadToEnd().Trim();
                p.WaitForExit();
                Log(L.Get("winget_ok", v));
                ScanInstalled(true);
            }
        }
        catch (Exception)
        {
            btnInstall.Enabled = false;
            lblStatus.Text = L.Get("winget_missing_status");
            MessageBox.Show(L.Get("winget_missing_body"), L.Get("winget_missing_title"),
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    // Applique la langue courante a toute l'interface.
    void ApplyLanguage()
    {
        subLabel.Text = L.Get("subtitle");
        sideTitle.Text = L.Get("categories");
        lblEmpty.Text = L.Get("no_result");
        btnClear.Text = L.Get("deselect_all");
        btnLog.Text = L.Get("details");
        btnSettings.Text = L.Get("settings");

        navs[0].Title = L.Get("all_apps");
        for (int i = 0; i < categories.Count; i++)
        {
            navs[i + 1].Title = CatName(categories[i]);
            headers[categories[i]].Title = CatName(categories[i]);
        }
        foreach (Entry en in entries)
        {
            string d = en.Desc;
            en.Card.Description = d;
            UpdateTip(en);
        }
        if (search.Input.IsHandleCreated)
        {
            try { Native.SendMessage(search.Input.Handle, 0x1501, (IntPtr)1, L.Get("search_cue")); }
            catch (Exception) { }
        }
        if (!installing && btnInstall.Enabled) lblStatus.Text = L.Get("ready");
        RefreshCounts();
        foreach (CatHeader hd in headers.Values) hd.Invalidate();
        foreach (Entry en in entries) en.Card.Invalidate();
    }

    void OpenSettings()
    {
        using (SettingsForm f = new SettingsForm(ApplyLanguage, ClearLogos, ExportSelection, ImportSelection))
            f.ShowDialog(this);
    }

    void ClearLogos()
    {
        IconLoader.ClearCache();
        foreach (Entry en in entries) en.Card.Icon = null;
        StartIconLoading();
    }

    void UpdateTip(Entry en)
    {
        string t = en.Id + "\n" + en.Desc;
        if (en.Card.Installed) t += "\n" + L.Get("installed_tag");
        tip.SetToolTip(en.Card, t);
    }

    // Raccourcis : Ctrl+F = rechercher, Echap = effacer la recherche
    void OnKey(object sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.F)
        {
            search.Input.Focus();
            search.Input.SelectAll();
            e.SuppressKeyPress = true;
        }
        else if (e.KeyCode == Keys.Escape && search.Input.Text.Length > 0)
        {
            search.Input.Clear();
            e.SuppressKeyPress = true;
        }
    }

    // ---------- Detection des applications deja installees ----------
    // "winget export" produit un JSON avec tous les identifiants reconnus : fiable, sans souci de largeur de console.
    void ScanInstalled(bool announce)
    {
        Thread t = new Thread(delegate()
        {
            try
            {
                string tmp = Path.Combine(Path.GetTempPath(), "appcrate_export_" + Process.GetCurrentProcess().Id + ".json");
                ProcessStartInfo psi = new ProcessStartInfo("winget", "export -o \"" + tmp + "\" --accept-source-agreements");
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.RedirectStandardOutput = true;
                using (Process p = Process.Start(psi))
                {
                    p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                }
                if (!File.Exists(tmp)) return;
                string json = File.ReadAllText(tmp, Encoding.UTF8);
                try { File.Delete(tmp); } catch (Exception) { }

                Dictionary<string, bool> ids = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
                foreach (Match m in Regex.Matches(json, "\"PackageIdentifier\"\\s*:\\s*\"([^\"]+)\""))
                    ids[m.Groups[1].Value] = true;

                UI(delegate
                {
                    int n = 0;
                    foreach (Entry en in entries)
                    {
                        bool inst = ids.ContainsKey(en.Id);
                        en.Card.Installed = inst;
                        if (inst) n++;
                        UpdateTip(en);
                    }
                    if (announce && !installing) lblStatus.Text = L.Get("installed_found", n);
                });
            }
            catch (Exception) { }
        });
        t.IsBackground = true;
        t.Start();
    }

    // ---------- Export / import de la selection ----------
    void ExportSelection()
    {
        using (SaveFileDialog d = new SaveFileDialog())
        {
            d.Filter = "AppCrate (*.txt)|*.txt";
            d.FileName = "appcrate-selection.txt";
            if (d.ShowDialog() != DialogResult.OK) return;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# AppCrate selection - one winget id per line");
            foreach (Entry en in entries)
                if (en.Card.IsChecked) sb.AppendLine(en.Id);
            try
            {
                File.WriteAllText(d.FileName, sb.ToString(), Encoding.UTF8);
                lblStatus.Text = L.Get("sel_exported", d.FileName);
            }
            catch (Exception ex) { lblStatus.Text = ex.Message; }
        }
    }

    void ImportSelection()
    {
        using (OpenFileDialog d = new OpenFileDialog())
        {
            d.Filter = "AppCrate (*.txt)|*.txt|*.*|*.*";
            if (d.ShowDialog() != DialogResult.OK) return;
            Dictionary<string, bool> ids = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (string raw in File.ReadAllLines(d.FileName, Encoding.UTF8))
                {
                    string line = raw.Trim();
                    if (line.Length == 0 || line[0] == '#') continue;
                    ids[line.Split(new char[] { '\t', ' ', ';' })[0]] = true;
                }
            }
            catch (Exception ex) { lblStatus.Text = ex.Message; return; }

            int n = 0;
            bulk = true;
            foreach (Entry en in entries)
            {
                bool want = ids.ContainsKey(en.Id);
                en.Card.IsChecked = want;
                if (want) n++;
            }
            bulk = false;
            RefreshCounts();
            lblStatus.Text = L.Get("sel_imported", n);
        }
    }

    // ---------- Logos ----------
    void StartIconLoading()
    {
        List<Entry> list = new List<Entry>();
        foreach (Entry en in entries)
            if (!string.IsNullOrEmpty(en.Site)) list.Add(en);
        LoadIcons(list, 1);
    }

    // Telecharge les logos en arriere-plan (6 fils). Les echecs sont retentes une fois.
    void LoadIcons(List<Entry> list, int pass)
    {
        Queue<Entry> q = new Queue<Entry>(list);
        List<Entry> failed = new List<Entry>();
        int remaining = 6;
        for (int i = 0; i < 6; i++)
        {
            Thread t = new Thread(delegate()
            {
                while (true)
                {
                    Entry en;
                    lock (q)
                    {
                        if (q.Count == 0) break;
                        en = q.Dequeue();
                    }
                    Image img = IconLoader.Load(en.Site);
                    if (img != null) UI(delegate { en.Card.Icon = img; });
                    else lock (failed) { failed.Add(en); }
                }
                bool last;
                lock (failed) { remaining--; last = (remaining == 0); }
                if (last && pass == 1 && failed.Count > 0)
                {
                    Thread.Sleep(3000);
                    LoadIcons(failed, 2);
                }
            });
            t.IsBackground = true;
            t.Start();
        }
    }

    void OnClosing(object sender, FormClosingEventArgs e)
    {
        if (installing)
        {
            DialogResult r = MessageBox.Show(L.Get("quit_confirm"), "AppCrate",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes) e.Cancel = true;
        }
    }

    // ---------- Mise en page ----------
    void LayoutHeader()
    {
        int w = header.ClientSize.Width;
        btnSettings.SetBounds(w - 24 - 140, (header.Height - 40) / 2, 140, 40);
        search.Location = new Point(btnSettings.Left - 12 - search.Width, (header.Height - search.Height) / 2);
    }

    void LayoutFooter()
    {
        int w = footer.ClientSize.Width;
        btnInstall.SetBounds(w - 24 - 200, 16, 200, 46);
        btnClear.SetBounds(btnInstall.Left - 10 - 150, 16, 150, 46);
        btnLog.SetBounds(btnClear.Left - 10 - 100, 16, 100, 46);
        prog.SetBounds(24, 76, Math.Max(50, w - 48), 6);
        lblStatus.Width = Math.Max(50, btnLog.Left - 24 - 20);
    }

    void LayoutHeaders()
    {
        int w = flow.ClientSize.Width - flow.Padding.Horizontal - 12 - SystemInformation.VerticalScrollBarWidth;
        w = Math.Max(240, w);
        foreach (CatHeader h in headers.Values) h.Width = w;
        lblEmpty.Width = w;
    }

    // ---------- Filtre / compteurs ----------
    bool Matches(Entry e)
    {
        if (query.Length == 0) return true;
        return e.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
               e.Id.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
               e.Desc.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    void ApplyFilter()
    {
        flow.SuspendLayout();
        int totalVisible = 0;
        foreach (string cat in categories)
        {
            int vis = 0;
            foreach (Entry e in entries)
            {
                if (e.Category != cat) continue;
                bool show = (filterCat == null || filterCat == cat) && Matches(e);
                e.Card.Visible = show;
                if (show) vis++;
            }
            headers[cat].Visible = vis > 0;
            totalVisible += vis;
        }
        lblEmpty.Visible = totalVisible == 0;
        flow.ResumeLayout(true);
        flow.AutoScrollPosition = new Point(0, 0);
        LayoutHeaders();
    }

    void RefreshCounts()
    {
        Dictionary<string, int> sel = new Dictionary<string, int>();
        Dictionary<string, int> tot = new Dictionary<string, int>();
        foreach (string c in categories) { sel[c] = 0; tot[c] = 0; }
        int total = 0;
        foreach (Entry e in entries)
        {
            tot[e.Category]++;
            if (e.Card.IsChecked) { sel[e.Category]++; total++; }
        }
        navs[0].Count = total;
        for (int i = 0; i < categories.Count; i++) navs[i + 1].Count = sel[categories[i]];
        foreach (string c in categories)
        {
            headers[c].SelCount = sel[c];
            headers[c].Total = tot[c];
            headers[c].Invalidate();
        }
        lblCount.Text = L.Get(total > 1 ? "sel_many" : "sel_one", total);
        if (!installing) btnInstall.Text = total > 0 ? L.Get("install_n", total) : L.Get("install");
    }

    void SetAll(bool value)
    {
        bulk = true;
        foreach (Entry e in entries) e.Card.IsChecked = value;
        bulk = false;
        RefreshCounts();
    }

    void OnHeaderToggled(object sender, EventArgs args)
    {
        CatHeader h = (CatHeader)sender;
        bool anyUnchecked = false;
        foreach (Entry e in entries)
            if (e.Category == h.Key && e.Card.Visible && !e.Card.IsChecked) { anyUnchecked = true; break; }
        bulk = true;
        foreach (Entry e in entries)
            if (e.Category == h.Key && e.Card.Visible) e.Card.IsChecked = anyUnchecked;
        bulk = false;
        RefreshCounts();
    }

    // ---------- Journal ----------
    void UI(MethodInvoker a)
    {
        if (IsDisposed) return;
        if (InvokeRequired) { try { BeginInvoke(a); } catch (Exception) { } }
        else a();
    }

    void Log(string s)
    {
        UI(delegate { log.AppendText(s + Environment.NewLine); });
    }

    // ---------- Installation ----------
    void OnInstallClick()
    {
        if (installing)
        {
            cancel = true;
            lblStatus.Text = L.Get("cancelling");
            return;
        }
        StartInstall();
    }

    void SetInstalling(bool value)
    {
        installing = value;
        foreach (Entry e in entries) e.Card.Enabled = !value;
        foreach (CatHeader h in headers.Values) h.Enabled = !value;
        btnClear.Enabled = !value;
        if (value) btnInstall.SetColors(Color.FromArgb(200, 70, 70), Color.FromArgb(225, 95, 95));
        else btnInstall.SetColors(Theme.Accent, Theme.AccentHover);
        btnInstall.Text = value ? L.Get("cancel") : L.Get("install");
        if (!value) RefreshCounts();
    }

    void StartInstall()
    {
        List<Entry> todo = new List<Entry>();
        foreach (Entry e in entries) if (e.Card.IsChecked) todo.Add(e);
        if (todo.Count == 0)
        {
            lblStatus.Text = L.Get("none_checked");
            return;
        }

        cancel = false;
        foreach (Entry e in entries) e.Card.Status = St.None;
        foreach (Entry e in todo) e.Card.Status = St.Waiting;
        SetInstalling(true);
        prog.Maximum = todo.Count;
        prog.Value = 0;
        log.Clear();

        Thread t = new Thread(delegate() { InstallLoop(todo); });
        t.IsBackground = true;
        t.Start();
    }

    void InstallLoop(List<Entry> todo)
    {
        int ok = 0, already = 0, failed = 0, done = 0;
        for (int i = 0; i < todo.Count; i++)
        {
            if (cancel) break;
            Entry en = todo[i];
            int index = i;
            UI(delegate
            {
                en.Card.Status = St.Running;
                if (en.Card.Visible) flow.ScrollControlIntoView(en.Card);
                lblStatus.Text = L.Get("installing_now", index + 1, todo.Count, en.Name);
            });
            Log("[" + (i + 1) + "/" + todo.Count + "] " + en.Name + " (" + en.Id + ")");

            St result;
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("winget",
                    "install --id " + en.Id + " -e --silent --accept-package-agreements --accept-source-agreements");
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.RedirectStandardOutput = true;
                psi.StandardOutputEncoding = Encoding.UTF8;
                using (Process p = Process.Start(psi))
                {
                    p.OutputDataReceived += delegate(object s, DataReceivedEventArgs ev)
                    {
                        if (ev.Data == null) return;
                        string l = ev.Data.Trim();
                        if (l.Length <= 2 || l.IndexOf('█') >= 0 || l.IndexOf('▒') >= 0) return;
                        Log("    " + l);
                    };
                    p.BeginOutputReadLine();
                    p.WaitForExit();
                    int code = p.ExitCode;
                    if (code == 0) { ok++; result = St.Done; Log("    -> OK"); }
                    else if (code == unchecked((int)0x8A15002B) || code == unchecked((int)0x8A150061))
                    {
                        already++;
                        result = St.Already;
                        Log("    -> " + L.Get("st_already"));
                    }
                    else
                    {
                        failed++;
                        result = St.Failed;
                        Log("    -> " + L.Get("st_failed") + " (0x" + code.ToString("X8") + ")");
                    }
                }
            }
            catch (Exception ex)
            {
                failed++;
                result = St.Failed;
                Log("    -> " + ex.Message);
            }

            done++;
            int d = done;
            St res = result;
            UI(delegate { en.Card.Status = res; prog.Value = d; });
        }

        int fOk = ok, fAlready = already, fFailed = failed;
        bool wasCancelled = cancel;
        UI(delegate
        {
            foreach (Entry e in todo)
                if (e.Card.Status == St.Waiting || e.Card.Status == St.Running) e.Card.Status = St.None;
            SetInstalling(false);
            ScanInstalled(false);
            lblStatus.Text = (wasCancelled ? L.Get("cancelled") : L.Get("done")) + " " + L.Get("summary", fOk, fAlready, fFailed);
            if (fFailed > 0) logPanel.Visible = true;
        });
        Log("");
        Log(L.Get("summary", fOk, fAlready, fFailed));
    }

    [STAThread]
    public static void Main()
    {
        try { ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | ServicePointManager.SecurityProtocol; }
        catch (Exception) { }
        Application.EnableVisualStyles();

        // Premier lancement : on demande la langue
        if (!Settings.Load())
        {
            using (LangForm f = new LangForm()) f.ShowDialog();
            Settings.Save();
        }
        Application.Run(new MainForm());
    }
}
