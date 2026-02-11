using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;

public class TrayRunner : Form
{
    private NotifyIcon trayIcon;
    private ContextMenuStrip trayMenu;
    private ToolStripMenuItem mgmtItem;
    private ToolStripMenuItem exitItem;
    private Process childProcess;
    private string targetExe;
    private string targetArgs;
    private string version = "unknown";
    private string port = "8317";
    private string logFile;
    private bool enableLogging = false;

    [STAThread]
    public static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new TrayRunner(args));
    }

    public TrayRunner(string[] args)
    {
        this.WindowState = FormWindowState.Normal;
        this.FormBorderStyle = FormBorderStyle.None;
        this.ShowInTaskbar = false;
        this.Visible = false;
        this.Opacity = 0;
        this.Size = new Size(0, 0);

        InitializeApp(args);
    }

    private void InitializeApp(string[] args)
    {
        if (args.Length > 0)
        {
            targetExe = Path.GetFullPath(args[0]);
            targetArgs = args.Length > 1 ? string.Join(" ", args, 1, args.Length - 1) : "";
        }
        else
        {
            targetExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cli-proxy-api.exe");
            targetArgs = "";
        }

        logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cli-proxy-api-tray.log");

        if (!File.Exists(targetExe))
        {
            MessageBox.Show("Target executable not found: " + targetExe, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(1);
            return;
        }

        trayIcon = new NotifyIcon();
        trayIcon.Text = "TrayRunner: " + Path.GetFileName(targetExe);
        
        string appDir = AppDomain.CurrentDomain.BaseDirectory;
        string faviconPath = Path.Combine(appDir, "favicon.ico");
        string explicitIconPath = null;

        foreach (string arg in args)
        {
            if (arg.EndsWith(".ico", StringComparison.OrdinalIgnoreCase) && File.Exists(arg))
            {
                explicitIconPath = arg;
            }
            else if (arg == "--log" || arg == "-l")
            {
                enableLogging = true;
            }
        }

        try {
            Icon appIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            if (appIcon != null)
            {
                trayIcon.Icon = appIcon;
            }
            else if (!string.IsNullOrEmpty(explicitIconPath))
            {
                trayIcon.Icon = new Icon(explicitIconPath);
            }
            else if (File.Exists(faviconPath))
            {
                trayIcon.Icon = new Icon(faviconPath);
            }
            else
            {
                trayIcon.Icon = Icon.ExtractAssociatedIcon(targetExe);
            }
        } catch {
            trayIcon.Icon = SystemIcons.Application;
        }

        trayMenu = new ContextMenuStrip();
        trayMenu.Renderer = new ModernRenderer();
        trayMenu.ImageScalingSize = new Size(16, 16);

        mgmtItem = new ToolStripMenuItem("Management Center", GetSmallIcon(trayIcon.Icon), OnOpenManagement);
        mgmtItem.Padding = new Padding(12, 0, 12, 0);
        mgmtItem.TextAlign = ContentAlignment.MiddleLeft;
        mgmtItem.ImageAlign = ContentAlignment.MiddleCenter;
        mgmtItem.Font = new Font(mgmtItem.Font, FontStyle.Bold);
        
        exitItem = new ToolStripMenuItem("Exit", GetExitIcon(), OnExit);
        exitItem.Padding = new Padding(12, 0, 12, 0);
        exitItem.TextAlign = ContentAlignment.MiddleLeft;
        exitItem.ImageAlign = ContentAlignment.MiddleCenter;

        trayMenu.Items.Add(mgmtItem);
        trayMenu.Items.Add(new ToolStripSeparator());
        trayMenu.Items.Add(exitItem);

        trayIcon.ContextMenuStrip = trayMenu;
        trayIcon.Visible = true;
        trayIcon.DoubleClick += OnOpenManagement;

        StartChildProcess();
    }

    private Image GetSmallIcon(Icon icon)
    {
        try {
            if (icon != null) {
                return new Bitmap(icon.ToBitmap(), new Size(16, 16));
            }
        } catch {}
        return null;
    }

    private Image GetExitIcon()
    {
        try {
            Bitmap bmp = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen pen = new Pen(Color.FromArgb(200, 50, 50), 2f))
                {
                    g.DrawLine(pen, 4, 4, 12, 12);
                    g.DrawLine(pen, 12, 4, 4, 12);
                }
            }
            return bmp;
        } catch {
            return null;
        }
    }

    private void StartChildProcess()
    {
        try
        {
            childProcess = new Process();
            childProcess.StartInfo.FileName = targetExe;
            childProcess.StartInfo.Arguments = targetArgs;
            childProcess.StartInfo.CreateNoWindow = true;
            childProcess.StartInfo.UseShellExecute = false;
            childProcess.StartInfo.RedirectStandardOutput = true;
            childProcess.StartInfo.RedirectStandardError = true;
            childProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(targetExe);
            childProcess.EnableRaisingEvents = true;
            childProcess.Exited += OnChildProcessExited;
            childProcess.OutputDataReceived += OnOutputDataReceived;
            childProcess.ErrorDataReceived += OnOutputDataReceived;
            childProcess.Start();
            childProcess.BeginOutputReadLine();
            childProcess.BeginErrorReadLine();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Failed to start process: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(1);
        }
    }

    private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Data)) return;

        if (enableLogging)
        {
            try
            {
                File.AppendAllText(logFile, string.Format("[{0:yyyy-MM-dd HH:mm:ss}] {1}{2}", DateTime.Now, e.Data, Environment.NewLine));
            }
            catch { }
        }

        bool updated = false;
        if (e.Data.Contains("CLIProxyAPI Version:"))
        {
            var parts = e.Data.Split(new[] { "CLIProxyAPI Version:" }, StringSplitOptions.None);
            if (parts.Length > 1)
            {
                var versionPart = parts[1].Trim();
                int commaIndex = versionPart.IndexOf(',');
                int spaceIndex = versionPart.IndexOf(' ');
                int stopIndex = -1;
                if (commaIndex >= 0 && spaceIndex >= 0) stopIndex = Math.Min(commaIndex, spaceIndex);
                else if (commaIndex >= 0) stopIndex = commaIndex;
                else if (spaceIndex >= 0) stopIndex = spaceIndex;

                if (stopIndex >= 0)
                {
                    version = versionPart.Substring(0, stopIndex).Trim();
                }
                else
                {
                    version = versionPart;
                }
                updated = true;
            }
        }
        else if (e.Data.Contains("API server started successfully on:"))
        {
            var parts = e.Data.Split(new[] { "API server started successfully on:" }, StringSplitOptions.None);
            if (parts.Length > 1)
            {
                var url = parts[1].Trim();
                try
                {
                    var uri = new Uri(url);
                    port = uri.Port.ToString();
                    updated = true;
                }
                catch
                {
                    var lastColon = url.LastIndexOf(':');
                    if (lastColon >= 0)
                    {
                        port = url.Substring(lastColon + 1).Trim('/').TrimStart(':');
                        updated = true;
                    }
                }
            }
        }

        if (updated)
        {
            UpdateTooltip();
        }
    }

    private void UpdateTooltip()
    {
        if (this.InvokeRequired)
        {
            this.BeginInvoke(new Action(UpdateTooltip));
            return;
        }

        if (trayIcon != null)
        {
            string text = port == "unknown" 
                ? string.Format("CLI Proxy API v{0}", version)
                : string.Format("CLI Proxy API v{0} (Port: {1})", version, port);
            if (text.Length > 63) text = text.Substring(0, 60) + "...";
            trayIcon.Text = text;
        }

        if (mgmtItem != null)
        {
            mgmtItem.Text = "Management Center";
        }
    }

    private void OnChildProcessExited(object sender, EventArgs e)
    {
        if (trayIcon != null)
        {
            trayIcon.ShowBalloonTip(3000, "Process Exited", "The child process has terminated.", ToolTipIcon.Info);
            System.Threading.Thread.Sleep(3000);
        }
        Environment.Exit(0);
    }

    private void OnOpenManagement(object sender, EventArgs e)
    {
        try {
            Process.Start(string.Format("http://localhost:{0}/management.html", port));
        } catch (Exception ex) {
            MessageBox.Show("Failed to open management URL: " + ex.Message);
        }
    }

    private void OnExit(object sender, EventArgs e)
    {
        Cleanup();
        Environment.Exit(0);
    }

    private void Cleanup()
    {
        if (childProcess != null && !childProcess.HasExited)
        {
            try
            {
                childProcess.Kill();
            }
            catch { }
        }
        if (trayIcon != null)
        {
            trayIcon.Visible = false;
            trayIcon.Dispose();
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        Cleanup();
        base.OnFormClosing(e);
    }
    
    private const int WS_EX_TOOLWINDOW = 0x80;

    protected override CreateParams CreateParams
    {
        get
        {
            CreateParams cp = base.CreateParams;
            cp.ExStyle |= WS_EX_TOOLWINDOW; 
            return cp;
        }
    }

    protected override void SetVisibleCore(bool value)
    {
        if (!this.IsHandleCreated)
        {
            this.CreateHandle();
            value = false;
        }
        base.SetVisibleCore(value);
    }
}

public class ModernRenderer : ToolStripProfessionalRenderer
{
    public ModernRenderer() : base(new ModernColorTable()) { }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        if (e.Item is ToolStripMenuItem)
        {
            int x = 36; 
            Color textColor = e.Item.Enabled ? Color.Black : Color.Gray;
            if (e.Item.Selected) textColor = Color.Black;

            TextRenderer.DrawText(e.Graphics, e.Text, e.TextFont, 
                new Rectangle(x, 0, e.Item.Width - x - 8, e.Item.Height), 
                textColor, Color.Transparent, 
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPrefix);
        }
        else
        {
            base.OnRenderItemText(e);
        }
    }

    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
    {
        using (Pen p = new Pen(Color.FromArgb(200, 200, 200)))
        {
            e.Graphics.DrawRectangle(p, 0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
        }
    }

    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
    {
        if (e.Item is ToolStripSeparator)
        {
            using (Pen p = new Pen(Color.FromArgb(230, 230, 230)))
            {
                int y = e.Item.ContentRectangle.Top + e.Item.ContentRectangle.Height / 2;
                e.Graphics.DrawLine(p, 36, y, e.ToolStrip.Width - 8, y);
            }
        }
        else
        {
            base.OnRenderSeparator(e);
        }
    }
}

public class ModernColorTable : ProfessionalColorTable
{
    public override Color MenuItemSelected { get { return Color.FromArgb(230, 230, 230); } }
    public override Color MenuItemSelectedGradientBegin { get { return Color.FromArgb(230, 230, 230); } }
    public override Color MenuItemSelectedGradientEnd { get { return Color.FromArgb(230, 230, 230); } }
    public override Color MenuItemBorder { get { return Color.Transparent; } }
    public override Color ToolStripDropDownBackground { get { return Color.White; } }
    public override Color ImageMarginGradientBegin { get { return Color.White; } }
    public override Color ImageMarginGradientMiddle { get { return Color.White; } }
    public override Color ImageMarginGradientEnd { get { return Color.White; } }
}
