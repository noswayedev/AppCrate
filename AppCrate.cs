using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

// AppCrate : catalogue d'applications a cocher, installation silencieuse via winget.
// Interface moderne dessinee a la main (WinForms). Compatible C# 5 (csc.exe integre a Windows).

// =====================================================================
//  Langues (francais / anglais)
// =====================================================================
static class L
{
    public static string Lang =
        System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "fr" ? "fr" : "en";

    static readonly Dictionary<string, string[]> T = new Dictionary<string, string[]>();

    static void Add(string key, string fr, string en) { T[key] = new string[] { fr, en }; }

    static L()
    {
        Add("subtitle", "Coche tes applications, clique sur Installer : tout se fait en silence.",
                        "Tick your apps, click Install: everything runs silently.");
        Add("categories", "CATÉGORIES", "CATEGORIES");
        Add("all_apps", "Toutes les applications", "All applications");
        Add("select_all", "Tout cocher", "Select all");
        Add("deselect_all", "Tout décocher", "Deselect all");
        Add("details", "Détails", "Details");
        Add("settings", "Paramètres", "Settings");
        Add("install", "Installer", "Install");
        Add("install_n", "Installer ({0})", "Install ({0})");
        Add("cancel", "Annuler", "Cancel");
        Add("sel_one", "{0} application sélectionnée", "{0} app selected");
        Add("sel_many", "{0} applications sélectionnées", "{0} apps selected");
        Add("ready", "Prêt.", "Ready.");
        Add("none_checked", "Aucune application cochée.", "No app selected.");
        Add("cancelling", "Annulation après l'application en cours...", "Cancelling after the current app...");
        Add("installing_now", "({0}/{1}) Installation de {2}...", "({0}/{1}) Installing {2}...");
        Add("done", "Terminé.", "Done.");
        Add("cancelled", "Annulé.", "Cancelled.");
        Add("summary", "{0} installée(s), {1} déjà présente(s), {2} échec(s).",
                       "{0} installed, {1} already installed, {2} failed.");
        Add("no_result", "Aucune application ne correspond à la recherche.", "No app matches your search.");
        Add("search_cue", "Rechercher une application...", "Search for an app...");
        Add("winget_ok", "winget détecté ({0}).", "winget detected ({0}).");
        Add("winget_missing_status", "winget est introuvable sur ce PC.", "winget was not found on this PC.");
        Add("winget_missing_title", "winget manquant", "winget missing");
        Add("winget_missing_body",
            "winget est introuvable sur ce PC.\n\nInstalle \"Programme d'installation d'application\" depuis le Microsoft Store, puis relance le programme.",
            "winget was not found on this PC.\n\nInstall \"App Installer\" from the Microsoft Store, then restart the program.");
        Add("quit_confirm", "Une installation est en cours. Quitter quand même ?", "An installation is in progress. Quit anyway?");
        Add("st_waiting", "En attente...", "Waiting...");
        Add("st_running", "Installation en cours...", "Installing...");
        Add("st_done", "Installé", "Installed");
        Add("st_already", "Déjà installé", "Already installed");
        Add("st_failed", "Échec", "Failed");
        Add("language", "Langue", "Language");
        Add("logos", "Logos", "Logos");
        Add("clear_logos", "Vider le cache des logos et recharger", "Clear logo cache and reload");
        Add("logos_reloading", "Rechargement des logos...", "Reloading logos...");
        Add("close", "Fermer", "Close");
        Add("about", "AppCrate 1.1 - licence MIT", "AppCrate 1.1 - MIT license");
        Add("choose_lang", "Choisissez votre langue  /  Choose your language", "Choisissez votre langue  /  Choose your language");
        Add("lang_hint", "Tu pourras la changer plus tard dans les paramètres.", "You can change it later in the settings.");
    }

    public static int Index { get { return Lang == "en" ? 1 : 0; } }

    public static string Get(string key)
    {
        string[] v;
        return T.TryGetValue(key, out v) ? v[Index] : key;
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
                    if (v == "fr" || v == "en") { L.Lang = v; return true; }
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
    public string Name, Id, Category, Site, DescFr, DescEn;
    public AppCard Card;

    public string Desc { get { return L.Index == 1 ? DescEn : DescFr; } }
}

// =====================================================================
//  Fenetres de dialogue (langue au premier lancement, parametres)
// =====================================================================
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
        ClientSize = new Size(440, 270);
        StartPosition = FormStartPosition.CenterScreen;
        ShowInTaskbar = true;

        Label t = new Label();
        t.Text = "AppCrate";
        t.Font = new Font("Segoe UI", 22f, FontStyle.Bold);
        t.ForeColor = Theme.Text;
        t.AutoSize = false;
        t.TextAlign = ContentAlignment.MiddleCenter;
        t.SetBounds(0, 24, 440, 50);

        Label q = new Label();
        q.Text = L.Get("choose_lang");
        q.Font = Theme.F10;
        q.ForeColor = Theme.Muted;
        q.AutoSize = false;
        q.TextAlign = ContentAlignment.MiddleCenter;
        q.SetBounds(0, 84, 440, 24);

        RoundButton fr = new RoundButton();
        fr.Text = "Français";
        fr.SetBounds(40, 130, 170, 60);
        fr.SetColors(Theme.Accent, Theme.AccentHover);
        fr.Click += delegate { Pick("fr"); };

        RoundButton en = new RoundButton();
        en.Text = "English";
        en.SetBounds(230, 130, 170, 60);
        en.SetColors(Theme.Accent, Theme.AccentHover);
        en.Click += delegate { Pick("en"); };

        Label hint = new Label();
        hint.Text = "Tu pourras la changer plus tard dans les paramètres.  /  You can change it later in the settings.";
        hint.Font = Theme.F85;
        hint.ForeColor = Theme.Muted;
        hint.AutoSize = false;
        hint.TextAlign = ContentAlignment.MiddleCenter;
        hint.SetBounds(20, 210, 400, 40);

        Controls.Add(t);
        Controls.Add(q);
        Controls.Add(fr);
        Controls.Add(en);
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
    Label title, lblLang, lblLogos, lblAbout;
    RoundButton btnFr, btnEn, btnClear, btnClose;
    Action onLang, onClear;

    public SettingsForm(Action onLanguageChanged, Action onClearLogos)
    {
        onLang = onLanguageChanged;
        onClear = onClearLogos;
        ClientSize = new Size(460, 390);
        StartPosition = FormStartPosition.CenterParent;

        title = new Label();
        title.Font = Theme.F14B;
        title.ForeColor = Theme.Text;
        title.AutoSize = true;
        title.Location = new Point(24, 20);

        lblLang = new Label();
        lblLang.Font = Theme.F10B;
        lblLang.ForeColor = Theme.Muted;
        lblLang.AutoSize = true;
        lblLang.Location = new Point(26, 78);

        btnFr = new RoundButton();
        btnFr.Text = "Français";
        btnFr.SetBounds(24, 106, 200, 46);
        btnFr.Click += delegate { SetLang("fr"); };

        btnEn = new RoundButton();
        btnEn.Text = "English";
        btnEn.SetBounds(236, 106, 200, 46);
        btnEn.Click += delegate { SetLang("en"); };

        lblLogos = new Label();
        lblLogos.Font = Theme.F10B;
        lblLogos.ForeColor = Theme.Muted;
        lblLogos.AutoSize = true;
        lblLogos.Location = new Point(26, 178);

        btnClear = new RoundButton();
        btnClear.SetBounds(24, 206, 412, 46);
        btnClear.Click += delegate
        {
            btnClear.Text = L.Get("logos_reloading");
            onClear();
        };

        lblAbout = new Label();
        lblAbout.Font = Theme.F9;
        lblAbout.ForeColor = Theme.Muted;
        lblAbout.AutoSize = true;
        lblAbout.Location = new Point(26, 282);

        btnClose = new RoundButton();
        btnClose.SetBounds(24, 322, 412, 46);
        btnClose.SetColors(Theme.Accent, Theme.AccentHover);
        btnClose.Click += delegate { Close(); };

        Controls.Add(title);
        Controls.Add(lblLang);
        Controls.Add(btnFr);
        Controls.Add(btnEn);
        Controls.Add(lblLogos);
        Controls.Add(btnClear);
        Controls.Add(lblAbout);
        Controls.Add(btnClose);
        Retext();
    }

    void Retext()
    {
        Text = L.Get("settings");
        title.Text = L.Get("settings");
        lblLang.Text = L.Get("language");
        lblLogos.Text = L.Get("logos");
        btnClear.Text = L.Get("clear_logos");
        btnClose.Text = L.Get("close");
        lblAbout.Text = L.Get("about");
        if (L.Lang == "fr") btnFr.SetColors(Theme.Accent, Theme.AccentHover); else btnFr.SetColors(Theme.Card, Theme.CardHover);
        if (L.Lang == "en") btnEn.SetColors(Theme.Accent, Theme.AccentHover); else btnEn.SetColors(Theme.Card, Theme.CardHover);
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
    //   "#Categorie|Category" ouvre une categorie (nom francais|nom anglais)
    //   "Nom|Identifiant winget|site web (pour le logo)|Description FR|Description EN"
    // Pour trouver l'identifiant d'une appli : "winget search nom" dans un terminal.
    static readonly string[] DATA = new string[]
    {
        "#Navigateurs|Browsers",
        "Google Chrome|Google.Chrome|google.com/chrome|Navigateur rapide de Google, synchronisé avec ton compte.|Fast web browser from Google, synced with your account.",
        "Mozilla Firefox|Mozilla.Firefox|mozilla.org/firefox|Navigateur libre de Mozilla, axé sur la vie privée.|Free, privacy-focused browser from Mozilla.",
        "Brave|Brave.Brave|brave.com|Navigateur qui bloque pubs et traqueurs par défaut.|Browser that blocks ads and trackers by default.",
        "Opera|Opera.Opera|opera.com|Navigateur avec VPN, bloqueur de pubs et messageries intégrés.|Browser with built-in VPN, ad blocker and messengers.",
        "Vivaldi|Vivaldi.Vivaldi|vivaldi.com|Navigateur très personnalisable, pensé pour les utilisateurs avancés.|Highly customizable browser built for power users.",

        "#Messagerie|Messaging",
        "Discord|Discord.Discord|discord.com|Chat vocal, vidéo et texte pour les communautés et les joueurs.|Voice, video and text chat for communities and gamers.",
        "Zoom|Zoom.Zoom|zoom.us|Visioconférences et réunions en ligne.|Video meetings and online conferencing.",
        "Microsoft Teams|Microsoft.Teams|microsoft.com/microsoft-teams/group-chat-software|Messagerie et visio de Microsoft pour le travail en équipe.|Microsoft chat and video calls for teamwork.",
        "Telegram|Telegram.TelegramDesktop|telegram.org|Messagerie rapide avec groupes, canaux et gros fichiers.|Fast messenger with groups, channels and large file sharing.",
        "Signal|OpenWhisperSystems.Signal|signal.org|Messagerie chiffrée de bout en bout, centrée sur la confidentialité.|End-to-end encrypted messenger focused on privacy.",
        "Slack|SlackTechnologies.Slack|slack.com|Messagerie d'équipe organisée par canaux.|Team messaging organized in channels.",

        "#Multimédia|Multimedia",
        "VLC|VideoLAN.VLC|videolan.org|Lecteur multimédia qui lit presque tous les formats audio et vidéo.|Media player that plays almost any audio or video format.",
        "Spotify|Spotify.Spotify|spotify.com|Streaming musical et podcasts.|Music and podcast streaming.",
        "Audacity|Audacity.Audacity|audacityteam.org|Éditeur audio gratuit pour enregistrer et retoucher des sons.|Free audio editor to record and edit sounds.",
        "OBS Studio|OBSProject.OBSStudio|obsproject.com|Enregistrement d'écran et streaming en direct.|Screen recording and live streaming.",
        "HandBrake|HandBrake.HandBrake|handbrake.fr|Convertisseur vidéo libre pour changer de format ou compresser.|Free video converter to change format or compress files.",
        "MPC-HC|clsid2.mpc-hc|mpc-hc.org|Lecteur vidéo léger et rapide, dans l'esprit du Lecteur Windows Media classique.|Lightweight, fast video player in the classic Windows Media Player spirit.",
        "foobar2000|PeterPawlowski.foobar2000|foobar2000.org|Lecteur audio léger et très configurable.|Lightweight and highly configurable audio player.",
        "Kodi|XBMCFoundation.Kodi|kodi.tv|Centre multimédia pour organiser films, séries et musique.|Media center to organize movies, shows and music.",

        "#Images et graphisme|Images & graphics",
        "GIMP|GIMP.GIMP|gimp.org|Retouche d'images avancée, alternative libre à Photoshop.|Advanced image editor, a free alternative to Photoshop.",
        "Inkscape|Inkscape.Inkscape|inkscape.org|Dessin vectoriel libre, alternative à Illustrator.|Free vector graphics editor, an alternative to Illustrator.",
        "Krita|Krita.Krita|krita.org|Logiciel de peinture numérique et d'illustration.|Digital painting and illustration software.",
        "Blender|BlenderFoundation.Blender|blender.org|Création 3D : modélisation, animation et rendu.|3D creation suite: modeling, animation and rendering.",
        "Paint.NET|dotPDN.PaintDotNet|getpaint.net|Éditeur d'images simple et rapide, un Paint en bien plus complet.|Simple, fast image editor, like Paint but far more capable.",
        "IrfanView|IrfanSkiljan.IrfanView|irfanview.com|Visionneuse d'images très légère avec conversion en lot.|Very light image viewer with batch conversion.",
        "ShareX|ShareX.ShareX|getsharex.com|Captures d'écran, GIF et partage rapide en ligne.|Screenshots, GIF capture and quick online sharing.",
        "Greenshot|Greenshot.Greenshot|getgreenshot.org|Captures d'écran avec annotations en un raccourci.|Screenshots with annotations from a single shortcut.",

        "#Bureautique|Office",
        "LibreOffice|TheDocumentFoundation.LibreOffice|libreoffice.org|Suite bureautique libre : texte, tableur, présentations.|Free office suite: documents, spreadsheets, presentations.",
        "Adobe Acrobat Reader|Adobe.Acrobat.Reader.64-bit|adobe.com/acrobat|Lecture, annotation et signature de fichiers PDF.|Read, annotate and sign PDF files.",
        "SumatraPDF|SumatraPDF.SumatraPDF|sumatrapdfreader.org|Lecteur de PDF et d'ebooks ultra léger et rapide.|Ultra-light and fast PDF and ebook reader.",
        "Notepad++|Notepad++.Notepad++|notepad-plus-plus.org|Éditeur de texte et de code léger avec coloration syntaxique.|Lightweight text and code editor with syntax highlighting.",
        "Obsidian|Obsidian.Obsidian|obsidian.md|Prise de notes en Markdown, avec des notes reliées entre elles.|Markdown note-taking with linked notes.",
        "Notion|Notion.Notion|notion.so|Notes, tâches et bases de données tout-en-un.|All-in-one notes, tasks and databases.",

        "#Utilitaires|Utilities",
        "7-Zip|7zip.7zip|7-zip.org|Compression et extraction d'archives (zip, 7z, rar...).|Archive compression and extraction (zip, 7z, rar...).",
        "WinRAR|RARLab.WinRAR|win-rar.com|Gestionnaire d'archives RAR et ZIP.|RAR and ZIP archive manager.",
        "Everything|voidtools.Everything|voidtools.com|Recherche instantanée de fichiers sur tout le PC.|Instant file search across your whole PC.",
        "PowerToys|Microsoft.PowerToys|microsoft.com|Outils Microsoft pour les utilisateurs avancés de Windows.|Microsoft utilities for Windows power users.",
        "qBittorrent|qBittorrent.qBittorrent|qbittorrent.org|Client BitTorrent libre et sans publicité.|Free, ad-free BitTorrent client.",
        "Bitwarden|Bitwarden.Bitwarden|bitwarden.com|Gestionnaire de mots de passe libre et sécurisé.|Free and secure password manager.",
        "KeePassXC|KeePassXCTeam.KeePassXC|keepassxc.org|Gestionnaire de mots de passe hors ligne.|Offline password manager.",
        "AnyDesk|AnyDesk.AnyDesk|anydesk.com|Prise de contrôle à distance d'un ordinateur.|Remote desktop access and control.",
        "TeamViewer|TeamViewer.TeamViewer|teamviewer.com|Assistance et accès à distance.|Remote support and access.",
        "Rufus|Rufus.Rufus|rufus.ie|Crée des clés USB bootables (Windows, Linux...).|Creates bootable USB drives (Windows, Linux...).",
        "balenaEtcher|Balena.Etcher|etcher.balena.io|Grave une image ISO sur une clé USB ou une carte SD.|Flashes ISO images to USB drives or SD cards.",
        "WinDirStat|WinDirStat.WinDirStat|windirstat.net|Montre ce qui prend de la place sur tes disques.|Shows what is using space on your drives.",
        "Malwarebytes|Malwarebytes.Malwarebytes|malwarebytes.com|Détection et suppression de logiciels malveillants.|Malware detection and removal.",

        "#Infos système|System info",
        "CPU-Z|CPUID.CPU-Z|cpuid.com|Détails sur le processeur, la carte mère et la mémoire.|Details about your CPU, motherboard and memory.",
        "HWMonitor|CPUID.HWMonitor|cpuid.com|Suivi des températures, tensions et ventilateurs.|Monitors temperatures, voltages and fan speeds.",
        "GPU-Z|TechPowerUp.GPU-Z|techpowerup.com|Informations et suivi de la carte graphique.|Graphics card information and monitoring.",
        "CrystalDiskInfo|CrystalDewWorld.CrystalDiskInfo|crystalmark.info|État de santé de tes disques (SMART).|Drive health status (SMART).",
        "CrystalDiskMark|CrystalDewWorld.CrystalDiskMark|crystalmark.info|Teste la vitesse de tes disques et SSD.|Benchmarks the speed of your drives and SSDs.",

        "#Développement|Development",
        "Visual Studio Code|Microsoft.VisualStudioCode|code.visualstudio.com|Éditeur de code de Microsoft, extensible, pour tous les langages.|Microsoft's extensible code editor for every language.",
        "Git|Git.Git|git-scm.com|Gestion de versions de code, indispensable aux développeurs.|Version control, essential for developers.",
        "GitHub Desktop|GitHub.GitHubDesktop|desktop.github.com|Interface graphique simple pour Git et GitHub.|Simple graphical interface for Git and GitHub.",
        "Python 3.12|Python.Python.3.12|python.org|Langage de programmation polyvalent et facile à apprendre.|Versatile, easy-to-learn programming language.",
        "Node.js LTS|OpenJS.NodeJS.LTS|nodejs.org|Environnement JavaScript côté serveur (version LTS).|Server-side JavaScript runtime (LTS version).",
        "Windows Terminal|Microsoft.WindowsTerminal|microsoft.com|Terminal moderne à onglets : PowerShell, CMD, WSL.|Modern tabbed terminal: PowerShell, CMD, WSL.",
        "Docker Desktop|Docker.DockerDesktop|docker.com|Conteneurs pour développer et déployer des applications.|Containers to build and ship applications.",
        "IntelliJ IDEA Community|JetBrains.IntelliJIDEA.Community|jetbrains.com/idea|IDE Java et Kotlin de JetBrains (édition gratuite).|JetBrains IDE for Java and Kotlin (free edition).",
        "Postman|Postman.Postman|postman.com|Teste et documente des API.|Test and document APIs.",
        "WinSCP|WinSCP.WinSCP|winscp.net|Transfert de fichiers SFTP, FTP et SCP.|SFTP, FTP and SCP file transfer.",
        "PuTTY|PuTTY.PuTTY|putty.org|Client SSH et Telnet pour se connecter à des serveurs.|SSH and Telnet client to connect to servers.",
        "Java JDK 21 (Temurin)|EclipseAdoptium.Temurin.21.JDK|adoptium.net|Kit de développement Java 21 (Eclipse Temurin).|Java 21 development kit (Eclipse Temurin).",

        "#Jeux|Games",
        "Steam|Valve.Steam|store.steampowered.com|La plus grande boutique et bibliothèque de jeux PC.|The largest PC game store and library.",
        "Epic Games Launcher|EpicGames.EpicGamesLauncher|epicgames.com|Boutique d'Epic, avec des jeux gratuits chaque semaine.|Epic's game store, with free games every week.",
        "GOG Galaxy|GOG.Galaxy|gog.com|Bibliothèque de jeux sans DRM qui réunit tes lanceurs.|DRM-free game library that unifies your launchers.",
        "Ubisoft Connect|Ubisoft.Connect|ubisoft.com|Lanceur pour les jeux Ubisoft.|Launcher for Ubisoft games.",
        "EA app|ElectronicArts.EADesktop|ea.com|Lanceur pour les jeux Electronic Arts.|Launcher for Electronic Arts games.",

        "#Stockage cloud|Cloud storage",
        "Dropbox|Dropbox.Dropbox|dropbox.com|Stockage cloud et synchronisation de fichiers.|Cloud storage and file sync.",
        "Google Drive|Google.GoogleDrive|drive.google.com|Stockage cloud de Google synchronisé avec ton PC.|Google cloud storage synced with your PC.",
        "Nextcloud|Nextcloud.NextcloudDesktop|nextcloud.com|Cloud personnel auto-hébergé : fichiers, agenda, contacts.|Self-hosted personal cloud: files, calendar, contacts.",

        "#Runtimes|Runtimes",
        "Visual C++ 2015-2022 x64|Microsoft.VCRedist.2015+.x64|microsoft.com|Bibliothèques requises par de nombreux jeux et logiciels (64 bits).|Libraries required by many games and apps (64-bit).",
        "Visual C++ 2015-2022 x86|Microsoft.VCRedist.2015+.x86|microsoft.com|Bibliothèques requises par de nombreux jeux et logiciels (32 bits).|Libraries required by many games and apps (32-bit).",
        ".NET Desktop Runtime 8|Microsoft.DotNet.DesktopRuntime.8|dotnet.microsoft.com|Nécessaire pour lancer des applications Windows .NET 8.|Required to run .NET 8 Windows applications.",
        "Java Runtime (JRE)|Oracle.JavaRuntimeEnvironment|java.com|Permet d'exécuter les applications et jeux Java (Minecraft...).|Runs Java applications and games (Minecraft...)."
    };

    List<Entry> entries = new List<Entry>();
    List<string> categories = new List<string>();
    Dictionary<string, string> catEn = new Dictionary<string, string>();
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
        BackColor = Theme.Bg;
        ForeColor = Theme.Text;
        Font = Theme.F9;
        DoubleBuffered = true;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1200, 760);
        MinimumSize = new Size(1000, 620);

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
                string[] cp = line.Substring(1).Split('|');
                cur = cp[0];
                catEn[cur] = cp.Length > 1 ? cp[1] : cp[0];
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
                en.DescFr = p.Length > 3 ? p[3] : "";
                en.DescEn = p.Length > 4 ? p[4] : en.DescFr;
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
        return L.Index == 1 ? catEn[key] : key;
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
            tip.SetToolTip(en.Card, en.Id + "\n" + d);
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
        using (SettingsForm f = new SettingsForm(ApplyLanguage, ClearLogos))
            f.ShowDialog(this);
    }

    void ClearLogos()
    {
        IconLoader.ClearCache();
        foreach (Entry en in entries) en.Card.Icon = null;
        StartIconLoading();
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
