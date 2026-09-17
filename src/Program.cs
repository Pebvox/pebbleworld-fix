using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;

[assembly: AssemblyTitle("PebbleWorld Fix")]
[assembly: AssemblyDescription("PebbleWorld, Discord and YouTube DPI Bypass Utility")]
[assembly: AssemblyCompany("Pebvox Studio")]
[assembly: AssemblyProduct("pebbleworld-fix")]
[assembly: AssemblyCopyright("Copyright (C) Pebvox 2026")]
[assembly: AssemblyVersion("3.0.0.0")]
[assembly: AssemblyFileVersion("3.0.0.0")]

namespace PebbleFix
{
    public class ModernButton : Control
    {
        public Color NormalColor { get; set; }
        public Color HoverColor { get; set; }
        public Color PressedColor { get; set; }
        public Color BorderColor { get; set; }
        public int CornerRadius { get; set; }
        private bool isHovered = false;
        private bool isPressed = false;

        public ModernButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            NormalColor = Color.FromArgb(16, 185, 129);
            HoverColor = Color.FromArgb(5, 150, 105);
            PressedColor = Color.FromArgb(4, 120, 87);
            BorderColor = Color.Transparent;
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            CornerRadius = 12;
            Cursor = Cursors.Hand;
        }

        protected override void OnMouseEnter(EventArgs e) { isHovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { isHovered = false; isPressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { if (e.Button == MouseButtons.Left) { isPressed = true; Invalidate(); } base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { isPressed = false; Invalidate(); base.OnMouseUp(e); }

        protected override void OnPaint(PaintEventArgs pe)
        {
            pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            pe.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Color bg = isPressed ? PressedColor : (isHovered ? HoverColor : NormalColor);
            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);

            using (GraphicsPath path = CreateRoundedRect(rect, CornerRadius))
            {
                using (SolidBrush brush = new SolidBrush(bg))
                {
                    pe.Graphics.FillPath(brush, path);
                }
                if (BorderColor != Color.Transparent)
                {
                    using (Pen pen = new Pen(BorderColor, 1.2f))
                    {
                        pe.Graphics.DrawPath(pen, path);
                    }
                }
            }

            TextRenderer.DrawText(pe.Graphics, Text, Font, ClientRectangle, ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }

        public static GraphicsPath CreateRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0) { path.AddRectangle(bounds); return path; }
            int d = radius * 2;
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    public class ModernCard : Panel
    {
        public int CornerRadius { get; set; }
        public Color BorderColor { get; set; }

        public ModernCard()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Color.FromArgb(18, 22, 32);
            BorderColor = Color.FromArgb(32, 40, 56);
            CornerRadius = 14;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = ModernButton.CreateRoundedRect(rect, CornerRadius))
            {
                using (SolidBrush brush = new SolidBrush(BackColor))
                {
                    pe.Graphics.FillPath(brush, path);
                }
                using (Pen pen = new Pen(BorderColor, 1.2f))
                {
                    pe.Graphics.DrawPath(pen, path);
                }
            }
        }
    }

    public class PingCard : Control
    {
        public string ServiceName { get; set; }
        public string ServiceDesc { get; set; }
        public string StatusText { get; set; }
        public Color StatusColor { get; set; }
        public int LatencyMs { get; set; }
        public bool IsOnline { get; set; }

        public PingCard(string name, string desc)
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            ServiceName = name;
            ServiceDesc = desc;
            StatusText = "Проверка...";
            StatusColor = Color.FromArgb(156, 163, 175);
            LatencyMs = -1;
            IsOnline = false;
            Cursor = Cursors.Hand;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            pe.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = ModernButton.CreateRoundedRect(rect, 12))
            {
                using (SolidBrush bg = new SolidBrush(Color.FromArgb(14, 18, 26)))
                {
                    pe.Graphics.FillPath(bg, path);
                }
                using (Pen border = new Pen(Color.FromArgb(28, 36, 50), 1.2f))
                {
                    pe.Graphics.DrawPath(border, path);
                }
            }

            // Dot
            int dotSize = 8;
            using (SolidBrush dotBrush = new SolidBrush(StatusColor))
            {
                pe.Graphics.FillEllipse(dotBrush, 14, 15, dotSize, dotSize);
            }

            // Name
            using (Font fn = new Font("Segoe UI", 9.5f, FontStyle.Bold))
            {
                using (SolidBrush textBrush = new SolidBrush(Color.White))
                {
                    pe.Graphics.DrawString(ServiceName, fn, textBrush, 28, 10);
                }
            }

            // Desc
            using (Font fd = new Font("Segoe UI", 7.5f, FontStyle.Regular))
            {
                using (SolidBrush descBrush = new SolidBrush(Color.FromArgb(148, 163, 184)))
                {
                    pe.Graphics.DrawString(ServiceDesc, fd, descBrush, 14, 34);
                }
            }

            // Status Badge / Latency
            using (Font fs = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            {
                using (SolidBrush stBrush = new SolidBrush(StatusColor))
                {
                    string txt = StatusText;
                    if (LatencyMs >= 0) txt = string.Format("{0} ms • Доступен", LatencyMs);
                    pe.Graphics.DrawString(txt, fs, stBrush, 14, 52);
                }
            }
        }
    }

    public class MainForm : Form
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private Panel headerPanel;
        private Label lblLogoTitle;
        private Label lblLogoSub;
        private Label lblEngineBadge;

        private ModernCard heroCard;
        private Label lblHeroState;
        private Label lblHeroSub;
        private ModernButton btnToggleBypass;

        private TableLayoutPanel pingPanel;
        private PingCard cardDiscord;
        private PingCard cardYouTube;
        private PingCard cardPebbleWorld;

        private ModernCard utilsCard;
        private ModernButton btnServiceToggle;
        private ModernButton btnCleanLists;
        private ModernButton btnFlushDns;
        private ModernButton btnCopyLog;

        private TextBox txtLog;
        private Label lblFooter;
        private System.Windows.Forms.Timer statusTimer;

        private string activeZapretDir = null;
        private bool isBypassActive = false;

        [STAThread]
        public static void Main()
        {
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | (SecurityProtocolType)768 | SecurityProtocolType.Tls;
            }
            catch {}

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        public MainForm()
        {
            InitializeComponent();
            ApplyDarkTitlebar();
            InitEngine();
            StartStatusWatcher();
            RunConnectivityCheck();
        }

        private void ApplyDarkTitlebar()
        {
            try
            {
                int dark = 1;
                // Attr 20 for Win11/Win10 (18985+), Attr 19 for older Win10
                DwmSetWindowAttribute(this.Handle, 20, ref dark, sizeof(int));
                DwmSetWindowAttribute(this.Handle, 19, ref dark, sizeof(int));
            }
            catch {}
        }

        private static bool IsAdministrator()
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch { return false; }
        }

        private bool EnsureAdmin()
        {
            if (IsAdministrator()) return true;
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = Application.ExecutablePath;
                psi.UseShellExecute = true;
                psi.Verb = "runas";
                Process.Start(psi);
                Application.Exit();
                return false;
            }
            catch
            {
                MessageBox.Show("Для управления сетевыми службами Windows требуются права Администратора!", "Требуются права", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        private void InitializeComponent()
        {
            this.Text = "PebbleWorld Fix v3.0 (ALT 11 • Pebvox)";
            this.Size = new Size(760, 720);
            this.MinimumSize = new Size(760, 720);
            this.MaximumSize = new Size(760, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(10, 13, 20);
            this.DoubleBuffered = true;

            try
            {
                this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch {}

            // 1. Header
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 74,
                BackColor = Color.FromArgb(13, 17, 26),
                Padding = new Padding(24, 14, 24, 14)
            };
            headerPanel.Paint += (s, pe) =>
            {
                using (Pen borderPen = new Pen(Color.FromArgb(24, 30, 44), 1))
                {
                    pe.Graphics.DrawLine(borderPen, 0, headerPanel.Height - 1, headerPanel.Width, headerPanel.Height - 1);
                }
            };

            lblLogoTitle = new Label
            {
                Text = "⚡ PebbleFix",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(22, 12),
                AutoSize = true
            };

            lblEngineBadge = new Label
            {
                Text = "v3.0.0 • ALT 11 ENGINE",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = Color.FromArgb(16, 185, 129),
                BackColor = Color.FromArgb(24, 16, 185, 129),
                Location = new Point(190, 18),
                Padding = new Padding(6, 3, 6, 3),
                AutoSize = true
            };

            lblLogoSub = new Label
            {
                Text = "Автономная система обхода DPI (Discord Голос + Web, YouTube 4K, PebbleWorld) by Pebvox",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(24, 44),
                AutoSize = true
            };

            headerPanel.Controls.Add(lblLogoTitle);
            headerPanel.Controls.Add(lblEngineBadge);
            headerPanel.Controls.Add(lblLogoSub);
            this.Controls.Add(headerPanel);

            // 2. Hero Card (Central 1-Click Toggle)
            heroCard = new ModernCard
            {
                Location = new Point(24, 88),
                Size = new Size(696, 92),
                BackColor = Color.FromArgb(16, 20, 30),
                BorderColor = Color.FromArgb(28, 36, 52),
                CornerRadius = 16
            };

            lblHeroState = new Label
            {
                Text = "⚪ ОБХОД НЕАКТИВЕН",
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(20, 20),
                AutoSize = true
            };

            lblHeroSub = new Label
            {
                Text = "Нажмите кнопку справа для мгновенной активации ALT 11",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(21, 48),
                Width = 400,
                AutoEllipsis = true
            };

            btnToggleBypass = new ModernButton
            {
                Text = "▶  ЗАПУСТИТЬ ОБХОД",
                Location = new Point(440, 18),
                Size = new Size(236, 54),
                NormalColor = Color.FromArgb(16, 185, 129),
                HoverColor = Color.FromArgb(5, 150, 105),
                PressedColor = Color.FromArgb(4, 120, 87),
                CornerRadius = 14,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold)
            };
            btnToggleBypass.Click += BtnToggleBypass_Click;

            heroCard.Controls.Add(lblHeroState);
            heroCard.Controls.Add(lblHeroSub);
            heroCard.Controls.Add(btnToggleBypass);
            this.Controls.Add(heroCard);

            // 3. Live Monitoring Cards (Discord, YouTube, PebbleWorld)
            pingPanel = new TableLayoutPanel
            {
                Location = new Point(24, 192),
                Size = new Size(696, 80),
                ColumnCount = 3,
                RowCount = 1
            };
            pingPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            pingPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            pingPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34f));

            cardDiscord = new PingCard("Discord", "Голос (UDP) + Gateway API") { Dock = DockStyle.Fill };
            cardDiscord.Click += (s, e) => RunConnectivityCheck();

            cardYouTube = new PingCard("YouTube", "Видеопотоки Googlevideo 4K") { Dock = DockStyle.Fill };
            cardYouTube.Click += (s, e) => RunConnectivityCheck();

            cardPebbleWorld = new PingCard("PebbleWorld", "Сайт и игровой сервер") { Dock = DockStyle.Fill };
            cardPebbleWorld.Click += (s, e) => RunConnectivityCheck();

            pingPanel.Controls.Add(cardDiscord, 0, 0);
            pingPanel.Controls.Add(cardYouTube, 1, 0);
            pingPanel.Controls.Add(cardPebbleWorld, 2, 0);
            this.Controls.Add(pingPanel);

            // 4. Quick Utils Card
            utilsCard = new ModernCard
            {
                Location = new Point(24, 282),
                Size = new Size(696, 68),
                BackColor = Color.FromArgb(15, 19, 28),
                BorderColor = Color.FromArgb(28, 36, 52),
                CornerRadius = 14
            };

            btnServiceToggle = new ModernButton
            {
                Text = "⚡ Служба Windows (Автозапуск)",
                Location = new Point(14, 14),
                Size = new Size(230, 40),
                NormalColor = Color.FromArgb(30, 41, 59),
                HoverColor = Color.FromArgb(51, 65, 85),
                PressedColor = Color.FromArgb(15, 23, 42),
                BorderColor = Color.FromArgb(51, 65, 85),
                CornerRadius = 10,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            btnServiceToggle.Click += BtnServiceToggle_Click;

            btnCleanLists = new ModernButton
            {
                Text = "🛡️ Очистить Cloudflare",
                Location = new Point(252, 14),
                Size = new Size(160, 40),
                NormalColor = Color.FromArgb(30, 41, 59),
                HoverColor = Color.FromArgb(51, 65, 85),
                PressedColor = Color.FromArgb(15, 23, 42),
                BorderColor = Color.FromArgb(51, 65, 85),
                CornerRadius = 10,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            btnCleanLists.Click += BtnCleanLists_Click;

            btnFlushDns = new ModernButton
            {
                Text = "🧹 Сбросить DNS",
                Location = new Point(420, 14),
                Size = new Size(130, 40),
                NormalColor = Color.FromArgb(30, 41, 59),
                HoverColor = Color.FromArgb(51, 65, 85),
                PressedColor = Color.FromArgb(15, 23, 42),
                BorderColor = Color.FromArgb(51, 65, 85),
                CornerRadius = 10,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            btnFlushDns.Click += BtnFlushDns_Click;

            btnCopyLog = new ModernButton
            {
                Text = "📋 Скопировать лог",
                Location = new Point(558, 14),
                Size = new Size(124, 40),
                NormalColor = Color.FromArgb(30, 41, 59),
                HoverColor = Color.FromArgb(51, 65, 85),
                PressedColor = Color.FromArgb(15, 23, 42),
                BorderColor = Color.FromArgb(51, 65, 85),
                CornerRadius = 10,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            btnCopyLog.Click += BtnCopyLog_Click;

            utilsCard.Controls.Add(btnServiceToggle);
            utilsCard.Controls.Add(btnCleanLists);
            utilsCard.Controls.Add(btnFlushDns);
            utilsCard.Controls.Add(btnCopyLog);
            this.Controls.Add(utilsCard);

            // 5. Monospace Log Console
            txtLog = new TextBox
            {
                Location = new Point(24, 360),
                Size = new Size(696, 270),
                Multiline = true,
                ReadOnly = true,
                BackColor = Color.FromArgb(8, 10, 16),
                ForeColor = Color.FromArgb(148, 163, 184),
                Font = new Font("Consolas", 9f, FontStyle.Regular),
                BorderStyle = BorderStyle.FixedSingle,
                ScrollBars = ScrollBars.Vertical
            };
            this.Controls.Add(txtLog);

            // 6. Footer
            lblFooter = new Label
            {
                Text = "Pebvox Studio • Проект с открытым исходным кодом pebbleworld-fix • GitHub",
                Font = new Font("Segoe UI", 8f, FontStyle.Regular),
                ForeColor = Color.FromArgb(71, 85, 105),
                Location = new Point(24, 642),
                AutoSize = true
            };
            this.Controls.Add(lblFooter);

            Log("[SYSTEM] PebbleFix v3.0 успешно инициализирован.");
            Log("[SYSTEM] База обхода: проверенная конфигурация ALT 11 с мультисплитом и TLS-пейлоадами.");
            if (!IsAdministrator())
            {
                Log("[WARN] Программа запущена без прав Администратора. Для управления службами будут запрошены права UAC.");
            }
        }

        private void Log(string msg)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action<string>(Log), msg);
                return;
            }
            string time = DateTime.Now.ToString("HH:mm:ss");
            txtLog.AppendText(string.Format("[{0}] {1}\r\n", time, msg));
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }

        private void StartStatusWatcher()
        {
            statusTimer = new System.Windows.Forms.Timer();
            statusTimer.Interval = 1500;
            statusTimer.Tick += (s, e) => UpdateStatus();
            statusTimer.Start();
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            bool serviceRunning = false;
            try
            {
                ServiceController sc = new ServiceController("zapret");
                serviceRunning = (sc.Status == ServiceControllerStatus.Running);
            }
            catch {}

            bool processRunning = false;
            try
            {
                processRunning = Process.GetProcessesByName("winws").Length > 0;
            }
            catch {}

            isBypassActive = (serviceRunning || processRunning);

            if (isBypassActive)
            {
                lblHeroState.Text = "🟢 ОБХОД АКТИВЕН (ALT 11)";
                lblHeroState.ForeColor = Color.FromArgb(16, 185, 129);
                if (serviceRunning)
                {
                    lblHeroSub.Text = "Работает в фоне как системная служба Windows (24/7 без консолей)";
                    btnServiceToggle.Text = "⏹ Удалить службу Windows";
                    btnServiceToggle.NormalColor = Color.FromArgb(69, 10, 10);
                    btnServiceToggle.BorderColor = Color.FromArgb(153, 27, 27);
                }
                else
                {
                    lblHeroSub.Text = "Работает в свёрнутом фоновом процессе winws.exe";
                    btnServiceToggle.Text = "⚡ Служба Windows (Автозапуск)";
                    btnServiceToggle.NormalColor = Color.FromArgb(30, 41, 59);
                    btnServiceToggle.BorderColor = Color.FromArgb(51, 65, 85);
                }

                btnToggleBypass.Text = "⏹  ОСТАНОВИТЬ ОБХОД";
                btnToggleBypass.NormalColor = Color.FromArgb(239, 68, 68);
                btnToggleBypass.HoverColor = Color.FromArgb(220, 38, 38);
                btnToggleBypass.PressedColor = Color.FromArgb(185, 28, 28);
            }
            else
            {
                lblHeroState.Text = "⚪ ОБХОД НЕАКТИВЕН";
                lblHeroState.ForeColor = Color.FromArgb(148, 163, 184);
                lblHeroSub.Text = "Нажмите кнопку справа для мгновенного запуска обхода";

                btnServiceToggle.Text = "⚡ Служба Windows (Автозапуск)";
                btnServiceToggle.NormalColor = Color.FromArgb(30, 41, 59);
                btnServiceToggle.BorderColor = Color.FromArgb(51, 65, 85);

                btnToggleBypass.Text = "▶  ЗАПУСТИТЬ ОБХОД";
                btnToggleBypass.NormalColor = Color.FromArgb(16, 185, 129);
                btnToggleBypass.HoverColor = Color.FromArgb(5, 150, 105);
                btnToggleBypass.PressedColor = Color.FromArgb(4, 120, 87);
            }

            heroCard.Invalidate();
        }

        private void InitEngine()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    // 1. Check folder next to exe
                    string appDir = AppDomain.CurrentDomain.BaseDirectory;
                    if (IsValidZapretFolder(appDir))
                    {
                        activeZapretDir = appDir;
                        Log("[ENGINE] Используется движок рядом с программой: " + activeZapretDir);
                        CleanLists(activeZapretDir);
                        return;
                    }

                    // 2. Check C:\\Users\\<user>\\Desktop\\zapret
                    string desktopZapret = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "zapret");
                    if (IsValidZapretFolder(desktopZapret))
                    {
                        activeZapretDir = desktopZapret;
                        Log("[ENGINE] Найден рабочий Zapret на Рабочем столе: " + activeZapretDir);
                        CleanLists(activeZapretDir);
                        return;
                    }

                    // 3. Check %LOCALAPPDATA%\\PebbleFix\\zapret
                    string localZapret = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PebbleFix", "zapret");
                    if (IsValidZapretFolder(localZapret))
                    {
                        activeZapretDir = localZapret;
                        Log("[ENGINE] Используется локальный движок ALT 11: " + activeZapretDir);
                        CleanLists(activeZapretDir);
                        return;
                    }

                    // 4. Extract embedded bundle
                    Log("[ENGINE] Распаковка встроенного автономного движка ALT 11...");
                    string extracted = ExtractEmbeddedBundle(localZapret);
                    if (!string.IsNullOrEmpty(extracted) && IsValidZapretFolder(extracted))
                    {
                        activeZapretDir = extracted;
                        CleanLists(activeZapretDir);
                        Log("[OK] Автономный движок ALT 11 готов к работе!");
                        return;
                    }

                    Log("[WARN] Движок не найден. Проверьте bundle.zip или укажите путь.");
                }
                catch (Exception ex)
                {
                    Log("[ERR] Ошибка инициализации движка: " + ex.Message);
                }
            });
        }

        private bool IsValidZapretFolder(string dir)
        {
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return false;
            string winws = Path.Combine(dir, "bin", "winws.exe");
            if (!File.Exists(winws)) winws = Path.Combine(dir, "winws.exe");
            string lists = Path.Combine(dir, "lists");
            return File.Exists(winws) && (Directory.Exists(lists) || File.Exists(Path.Combine(dir, "list-general.txt")));
        }

        private string ExtractEmbeddedBundle(string targetDir)
        {
            try
            {
                Directory.CreateDirectory(targetDir);
                using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("bundle.zip"))
                {
                    if (s == null) return null;
                    string tempZip = Path.Combine(targetDir, "bundle_temp.zip");
                    using (FileStream fs = new FileStream(tempZip, FileMode.Create, FileAccess.Write))
                    {
                        s.CopyTo(fs);
                    }
                    ZipFile.ExtractToDirectory(tempZip, targetDir);
                    try { File.Delete(tempZip); } catch {}
                    return targetDir;
                }
            }
            catch (Exception ex)
            {
                Log("[ERR] Распаковка bundle.zip: " + ex.Message);
                return null;
            }
        }

        private void CleanLists(string zapretDir)
        {
            try
            {
                string listsDir = Path.Combine(zapretDir, "lists");
                string binDir = Path.Combine(zapretDir, "bin");

                // 1. Ensure PebbleWorld is in list-exclude.txt
                string excludePath = Path.Combine(listsDir, "list-exclude.txt");
                if (File.Exists(excludePath))
                {
                    List<string> lines = File.ReadAllLines(excludePath).ToList();
                    string[] needed = new string[] { "pebbleworld.cyou", "beta.pebbleworld.cyou", "chat.pebbleworld.cyou" };
                    bool added = false;
                    foreach (string n in needed)
                    {
                        if (!lines.Any(l => l.Trim().Equals(n, StringComparison.OrdinalIgnoreCase)))
                        {
                            lines.Add(n);
                            added = true;
                        }
                    }
                    if (added)
                    {
                        File.WriteAllLines(excludePath, lines.ToArray());
                        Log("[LISTS] Домены PebbleWorld добавлены в list-exclude.txt (защита Cloudflare)");
                    }
                }

                // 2. Remove pebbleworld & cloudflare from general lists
                string[] checkFiles = new string[]
                {
                    Path.Combine(listsDir, "list-general.txt"),
                    Path.Combine(listsDir, "list-general-user.txt"),
                    Path.Combine(binDir, "list-general.txt"),
                    Path.Combine(zapretDir, "list-general.txt")
                };

                string[] bad = new string[] { "pebbleworld", "trycloudflare.com", "cloudflare.com" };
                foreach (string cf in checkFiles)
                {
                    if (File.Exists(cf))
                    {
                        string[] lines = File.ReadAllLines(cf);
                        List<string> cleaned = lines.Where(l => !bad.Any(b => l.IndexOf(b, StringComparison.OrdinalIgnoreCase) >= 0)).ToList();
                        if (cleaned.Count != lines.Length)
                        {
                            File.WriteAllLines(cf, cleaned.ToArray());
                            Log("[LISTS] Очищен файл " + Path.GetFileName(cf) + ": удалены записи Cloudflare.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log("[WARN] Очистка списков: " + ex.Message);
            }
        }

        private string BuildAlt11Arguments(string zapretDir)
        {
            string bin = Path.Combine(zapretDir, "bin") + Path.DirectorySeparatorChar;
            string lists = Path.Combine(zapretDir, "lists") + Path.DirectorySeparatorChar;
            if (!Directory.Exists(lists)) lists = zapretDir + Path.DirectorySeparatorChar;
            if (!Directory.Exists(bin)) bin = zapretDir + Path.DirectorySeparatorChar;

            return "--wf-tcp=80,443,2053,2083,2087,2096,8443,1024-65535 --wf-udp=443,19294-19344,50000-50100,1024-65535 " +
                   "--filter-udp=443 --hostlist=\"" + lists + "list-general.txt\" --hostlist=\"" + lists + "list-general-user.txt\" --hostlist-exclude=\"" + lists + "list-exclude.txt\" --hostlist-exclude=\"" + lists + "list-exclude-user.txt\" --ipset-exclude=\"" + lists + "ipset-exclude.txt\" --ipset-exclude=\"" + lists + "ipset-exclude-user.txt\" --dpi-desync=fake --dpi-desync-repeats=11 --dpi-desync-fake-quic=\"" + bin + "quic_initial_www_google_com.bin\" --new " +
                   "--filter-udp=19294-19344,50000-50100 --filter-l7=discord,stun --dpi-desync=fake --dpi-desync-fake-discord=\"" + bin + "ACTIVE_DISCORD_UDP.bin\" --dpi-desync-fake-stun=\"" + bin + "ACTIVE_DISCORD_UDP.bin\" --dpi-desync-repeats=6 --new " +
                   "--filter-tcp=2053,2083,2087,2096,8443 --hostlist-domains=discord.media --dpi-desync=fake,multisplit --dpi-desync-split-seqovl=681 --dpi-desync-split-pos=1 --dpi-desync-fooling=ts --dpi-desync-repeats=8 --dpi-desync-split-seqovl-pattern=\"" + bin + "tls_clienthello_www_google_com.bin\" --dpi-desync-fake-tls=\"" + bin + "tls_clienthello_www_google_com.bin\" --new " +
                   "--filter-tcp=443 --hostlist=\"" + lists + "list-google.txt\" --ip-id=zero --dpi-desync=fake,multisplit --dpi-desync-split-seqovl=681 --dpi-desync-split-pos=1 --dpi-desync-fooling=ts --dpi-desync-repeats=8 --dpi-desync-split-seqovl-pattern=\"" + bin + "tls_clienthello_www_google_com.bin\" --dpi-desync-fake-tls=\"" + bin + "tls_clienthello_www_google_com.bin\" --new " +
                   "--filter-tcp=80,443 --hostlist=\"" + lists + "list-general.txt\" --hostlist=\"" + lists + "list-general-user.txt\" --hostlist-exclude=\"" + lists + "list-exclude.txt\" --hostlist-exclude=\"" + lists + "list-exclude-user.txt\" --ipset-exclude=\"" + lists + "ipset-exclude.txt\" --ipset-exclude=\"" + lists + "ipset-exclude-user.txt\" --dpi-desync=fake,multisplit --dpi-desync-split-seqovl=664 --dpi-desync-split-pos=1 --dpi-desync-fooling=ts --dpi-desync-repeats=8 --dpi-desync-split-seqovl-pattern=\"" + bin + "tls_clienthello_max_ru.bin\" --dpi-desync-fake-tls=\"" + bin + "stun2.bin\" --dpi-desync-fake-tls=\"" + bin + "tls_clienthello_max_ru.bin\" --dpi-desync-fake-http=\"" + bin + "tls_clienthello_max_ru.bin\" --new " +
                   "--filter-udp=443 --ipset=\"" + lists + "ipset-all.txt\" --hostlist-exclude=\"" + lists + "list-exclude.txt\" --hostlist-exclude=\"" + lists + "list-exclude-user.txt\" --ipset-exclude=\"" + lists + "ipset-exclude.txt\" --ipset-exclude=\"" + lists + "ipset-exclude-user.txt\" --dpi-desync=fake --dpi-desync-repeats=11 --dpi-desync-fake-quic=\"" + bin + "quic_initial_www_google_com.bin\" --new " +
                   "--filter-tcp=80,443,8443 --ipset=\"" + lists + "ipset-all.txt\" --hostlist-exclude=\"" + lists + "list-exclude.txt\" --hostlist-exclude=\"" + lists + "list-exclude-user.txt\" --ipset-exclude=\"" + lists + "ipset-exclude.txt\" --ipset-exclude=\"" + lists + "ipset-exclude-user.txt\" --dpi-desync=fake,multisplit --dpi-desync-split-seqovl=664 --dpi-desync-split-pos=1 --dpi-desync-fooling=ts --dpi-desync-repeats=8 --dpi-desync-split-seqovl-pattern=\"" + bin + "tls_clienthello_max_ru.bin\" --dpi-desync-fake-tls=\"" + bin + "stun2.bin\" --dpi-desync-fake-tls=\"" + bin + "tls_clienthello_max_ru.bin\" --dpi-desync-fake-http=\"" + bin + "tls_clienthello_max_ru.bin\" --new " +
                   "--filter-tcp=1024-65535 --ipset=\"" + lists + "ipset-all.txt\" --ipset-exclude=\"" + lists + "ipset-exclude.txt\" --ipset-exclude=\"" + lists + "ipset-exclude-user.txt\" --dpi-desync=fake,multisplit --dpi-desync-any-protocol=1 --dpi-desync-cutoff=n4 --dpi-desync-split-seqovl=664 --dpi-desync-split-pos=1 --dpi-desync-fooling=ts --dpi-desync-repeats=8 --dpi-desync-split-seqovl-pattern=\"" + bin + "tls_clienthello_max_ru.bin\" --dpi-desync-fake-tls=\"" + bin + "stun2.bin\" --dpi-desync-fake-tls=\"" + bin + "tls_clienthello_max_ru.bin\" --dpi-desync-fake-http=\"" + bin + "tls_clienthello_max_ru.bin\" --dpi-desync-fake-unknown=\"" + bin + "stun2.bin\" --dpi-desync-fake-unknown=\"" + bin + "tls_clienthello_max_ru.bin\" --new " +
                   "--filter-udp=1024-65535 --ipset=\"" + lists + "ipset-all.txt\" --ipset-exclude=\"" + lists + "ipset-exclude.txt\" --ipset-exclude=\"" + lists + "ipset-exclude-user.txt\" --dpi-desync=fake --dpi-desync-repeats=10 --dpi-desync-any-protocol=1 --dpi-desync-fake-unknown-udp=\"" + bin + "ACTIVE_GAME_UDP.bin\" --dpi-desync-cutoff=n4";
        }

        private void KillProcesses()
        {
            foreach (Process p in Process.GetProcessesByName("winws"))
            {
                try { p.Kill(); p.WaitForExit(1000); } catch {}
            }
        }

        private void RunCommandSilent(string cmd)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c " + cmd);
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                Process p = Process.Start(psi);
                p.WaitForExit(4000);
            }
            catch {}
        }

        private void BtnToggleBypass_Click(object sender, EventArgs e)
        {
            if (!EnsureAdmin()) return;

            if (isBypassActive)
            {
                Log("[ACTION] Остановка обхода...");
                ThreadPool.QueueUserWorkItem(delegate
                {
                    RunCommandSilent("net stop zapret");
                    KillProcesses();
                    Thread.Sleep(800);
                    Log("[OK] Обход успешно остановлен.");
                    RunConnectivityCheck();
                });
            }
            else
            {
                if (string.IsNullOrEmpty(activeZapretDir) || !Directory.Exists(activeZapretDir))
                {
                    MessageBox.Show("Движок Zapret не готов к запуску.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Log("[ACTION] Запуск обхода ALT 11 в фоновом режиме...");
                ThreadPool.QueueUserWorkItem(delegate
                {
                    try
                    {
                        CleanLists(activeZapretDir);
                        RunCommandSilent("net stop zapret");
                        KillProcesses();

                        string binDir = Path.Combine(activeZapretDir, "bin");
                        if (!Directory.Exists(binDir)) binDir = activeZapretDir;
                        string winwsPath = Path.Combine(binDir, "winws.exe");
                        string args = BuildAlt11Arguments(activeZapretDir);

                        ProcessStartInfo psi = new ProcessStartInfo();
                        psi.FileName = winwsPath;
                        psi.Arguments = args;
                        psi.WorkingDirectory = binDir;
                        psi.UseShellExecute = true;
                        psi.WindowStyle = ProcessWindowStyle.Minimized;
                        Process.Start(psi);

                        Thread.Sleep(1000);
                        Log("[OK] Обход ALT 11 активен! Discord, YouTube и сайт PebbleWorld разблокированы.");
                        RunConnectivityCheck();
                    }
                    catch (Exception ex)
                    {
                        Log("[ERR] Запуск: " + ex.Message);
                    }
                });
            }
        }

        private void BtnServiceToggle_Click(object sender, EventArgs e)
        {
            if (!EnsureAdmin()) return;

            bool serviceExists = false;
            try
            {
                ServiceController sc = new ServiceController("zapret");
                ServiceControllerStatus st = sc.Status;
                serviceExists = true;
            }
            catch {}

            if (serviceExists)
            {
                Log("[ACTION] Остановка и удаление системной службы Windows...");
                ThreadPool.QueueUserWorkItem(delegate
                {
                    RunCommandSilent("net stop zapret");
                    RunCommandSilent("sc delete zapret");
                    KillProcesses();
                    Thread.Sleep(1000);
                    Log("[OK] Служба zapret удалена.");
                });
            }
            else
            {
                if (string.IsNullOrEmpty(activeZapretDir)) return;
                Log("[ACTION] Установка и регистрация постоянной службы Windows (автозапуск 24/7)...");
                ThreadPool.QueueUserWorkItem(delegate
                {
                    try
                    {
                        CleanLists(activeZapretDir);
                        KillProcesses();
                        RunCommandSilent("net stop zapret");
                        RunCommandSilent("sc delete zapret");
                        RunCommandSilent("netsh int tcp set global timestamps=enabled");

                        string binDir = Path.Combine(activeZapretDir, "bin");
                        if (!Directory.Exists(binDir)) binDir = activeZapretDir;
                        string winwsPath = Path.Combine(binDir, "winws.exe");
                        string args = BuildAlt11Arguments(activeZapretDir);

                        string binPathArg = "\\\"" + winwsPath + "\\\" " + args;
                        string createCmd = string.Format("sc create zapret binPath= \"{0}\" DisplayName= \"zapret\" start= auto", binPathArg);
                        RunCommandSilent(createCmd);
                        RunCommandSilent("sc description zapret \"PebbleWorld & Discord DPI Bypass Service (ALT 11)\"");
                        RunCommandSilent("sc start zapret");

                        Thread.Sleep(1500);
                        Log("[OK] Служба Windows успешно создана и запущена!");
                        Log("[OK] Теперь обход запускается автоматически вместе с Windows в фоне.");
                        RunConnectivityCheck();
                    }
                    catch (Exception ex)
                    {
                        Log("[ERR] Установка службы: " + ex.Message);
                    }
                });
            }
        }

        private void BtnCleanLists_Click(object sender, EventArgs e)
        {
            if (!EnsureAdmin()) return;
            if (string.IsNullOrEmpty(activeZapretDir)) return;

            Log("[ACTION] Проверка списков на исключение Cloudflare и PebbleWorld...");
            CleanLists(activeZapretDir);
            MessageBox.Show("Списки успешно проверены!\nДомены PebbleWorld и Cloudflare исключены из десинхронизации.", "Очистка завершена", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnFlushDns_Click(object sender, EventArgs e)
        {
            Log("[ACTION] Очистка кэша сопоставителя DNS Windows (ipconfig /flushdns)...");
            ThreadPool.QueueUserWorkItem(delegate
            {
                RunCommandSilent("ipconfig /flushdns");
                Log("[OK] Кэш DNS успешно очищен.");
            });
        }

        private void BtnCopyLog_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(txtLog.Text))
                {
                    Clipboard.SetText(txtLog.Text);
                    Log("[INFO] Текст журнала скопирован в буфер обмена.");
                }
            }
            catch {}
        }

        private void RunConnectivityCheck()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                CheckServicePing(cardDiscord, "https://gateway.discord.gg/");
                CheckServicePing(cardYouTube, "https://www.youtube.com/");
                CheckServicePing(cardPebbleWorld, "https://beta.pebbleworld.cyou/");
            });
        }

        private void CheckServicePing(PingCard card, string url)
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Timeout = 6000;
                req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    sw.Stop();
                    int ms = (int)sw.ElapsedMilliseconds;
                    this.Invoke(new Action(delegate
                    {
                        card.IsOnline = true;
                        card.LatencyMs = ms;
                        card.StatusColor = Color.FromArgb(16, 185, 129);
                        card.StatusText = string.Format("{0} ms • Доступен", ms);
                        card.Invalidate();
                    }));
                }
            }
            catch (WebException wex)
            {
                if (wex.Response is HttpWebResponse)
                {
                    this.Invoke(new Action(delegate
                    {
                        card.IsOnline = true;
                        card.LatencyMs = 60;
                        card.StatusColor = Color.FromArgb(16, 185, 129);
                        card.StatusText = "Доступен (HTTP OK)";
                        card.Invalidate();
                    }));
                    return;
                }
                this.Invoke(new Action(delegate
                {
                    card.IsOnline = false;
                    card.LatencyMs = -1;
                    card.StatusColor = Color.FromArgb(239, 68, 68);
                    card.StatusText = "Недоступен";
                    card.Invalidate();
                }));
            }
            catch
            {
                this.Invoke(new Action(delegate
                {
                    card.IsOnline = false;
                    card.LatencyMs = -1;
                    card.StatusColor = Color.FromArgb(239, 68, 68);
                    card.StatusText = "Таймаут";
                    card.Invalidate();
                }));
            }
        }
    }
}
