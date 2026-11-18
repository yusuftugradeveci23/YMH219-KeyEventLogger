using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace YMH219_KeyEventLogger
{
    public partial class Form1 : Form
    {
        private readonly KeyLogManager _logManager;

        // Üst Bilgi ve İstatistik Bileşenleri
        private Panel _topPanel = null!;
        private Label _lblStatusBadge = null!;
        private Label _lblLastKey = null!;
        private Label _lblStats = null!;
        private TextBox _txtTestArea = null!;

        // Kontrol Butonları (Föye uygun ayrı Başlat ve Durdur butonları)
        private Button _btnStart = null!;
        private Button _btnStop = null!;
        private Button _btnClear = null!;
        private Button _btnExportTxt = null!;
        private Button _btnExportCsv = null!;

        // Kayıt Listesi (Föye uygun ListBox bileşeni)
        private ListBox _lstLogs = null!;

        public Form1()
        {
            InitializeComponent();
            _logManager = new KeyLogManager();
            SetupCustomUI();
            BindKeyboardEvents();
            UpdateRecordingStateUI();
        }

        private void SetupCustomUI()
        {
            // Form Genel Ayarları
            this.Text = "Key Event Logger - YMH 219 (Uygulama İçi Klavye Olay İzleyici)";
            this.Size = new Size(980, 640);
            this.MinimumSize = new Size(880, 540);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);

            // Form üzerindeki tüm kontrollerden önce klavye olaylarını yakalamak için kritik ayar
            this.KeyPreview = true;

            // Üst Kontrol Paneli
            _topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 155,
                BackColor = Color.White,
                Padding = new Padding(15)
            };

            // Etik Sınır & Durum Rozeti
            _lblStatusBadge = new Label
            {
                Text = "● KAYIT AKTİF (Yalnızca Bu Pencere Odaktayken İzlenir)",
                AutoSize = true,
                Location = new Point(16, 12),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(22, 163, 74)
            };

            // Son Yakalanan Tuş Bilgisi
            _lblLastKey = new Label
            {
                Text = "Son Olay: Henüz tuşa basılmadı",
                AutoSize = true,
                Location = new Point(16, 40),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59)
            };

            // Sayaç İstatistikleri
            _lblStats = new Label
            {
                Text = "Toplam: 0  |  KeyDown: 0  |  KeyPress: 0  |  KeyUp: 0",
                AutoSize = true,
                Location = new Point(16, 68),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(71, 85, 105)
            };

            // Test Yazı Alanı (Kullanıcı odaklanıp rahatça yazabilsin diye)
            _txtTestArea = new TextBox
            {
                Location = new Point(540, 14),
                Size = new Size(405, 55),
                Multiline = true,
                Font = new Font("Segoe UI", 10F),
                PlaceholderText = "Buraya tıklayıp klavyeden tuşlara basarak KeyDown, KeyPress ve KeyUp olaylarını canlı test edebilirsiniz..."
            };

            // Kontrol Butonları (Ayrı Başlat ve Durdur Butonları)
            _btnStart = CreateActionButton("Başlat", new Point(16, 102), new Size(125, 36), Color.FromArgb(22, 163, 74));
            _btnStart.Click += BtnStart_Click;

            _btnStop = CreateActionButton("Durdur", new Point(151, 102), new Size(125, 36), Color.FromArgb(220, 38, 38));
            _btnStop.Click += BtnStop_Click;

            _btnClear = CreateActionButton("Temizle", new Point(286, 102), new Size(135, 36), Color.FromArgb(100, 116, 139));
            _btnClear.Click += BtnClear_Click;

            _btnExportTxt = CreateActionButton("TXT Kaydet", new Point(431, 102), new Size(145, 36), Color.FromArgb(37, 99, 235));
            _btnExportTxt.Click += BtnExportTxt_Click;

            _btnExportCsv = CreateActionButton("CSV Kaydet", new Point(586, 102), new Size(145, 36), Color.FromArgb(13, 148, 136));
            _btnExportCsv.Click += BtnExportCsv_Click;

            _topPanel.Controls.AddRange(new Control[]
            {
                _lblStatusBadge, _lblLastKey, _lblStats, _txtTestArea,
                _btnStart, _btnStop, _btnClear, _btnExportTxt, _btnExportCsv
            });

            // Kayıt Listesi (ListBox)
            _lstLogs = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(30, 41, 59),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 10F, FontStyle.Regular),
                IntegralHeight = false,
                HorizontalScrollbar = true
            }; 

            // Kontrolleri Forma Ekle (Önce Fill olan liste, sonra Top olan panel eklenir)
            this.Controls.Add(_lstLogs);
            this.Controls.Add(_topPanel);
        }

        private Button CreateActionButton(string text, Point location, Size size, Color backColor)
        {
            return new Button
            {
                Text = text,
                Location = location,
                Size = size,
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TabStop = false // Space/Enter basıldığında butonların yanlışlıkla tetiklenmesini önler
            };
        }

        private void BindKeyboardEvents()
        {
            this.KeyDown += Form1_KeyDown;
            this.KeyPress += Form1_KeyPress;
            this.KeyUp += Form1_KeyUp;
        }

        private string GetModifiersText(Keys modifiers)
        {
            var list = new List<string>();
            if ((modifiers & Keys.Control) == Keys.Control) list.Add("Ctrl");
            if ((modifiers & Keys.Shift) == Keys.Shift) list.Add("Shift");
            if ((modifiers & Keys.Alt) == Keys.Alt) list.Add("Alt");
            return list.Count > 0 ? string.Join(" + ", list) : "Yok";
        }

        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            string modifiers = GetModifiersText(e.Modifiers);
            var entry = _logManager.AddEntry("KeyDown", e.KeyCode.ToString(), e.KeyCode.ToString(), e.KeyValue, modifiers);
            if (entry != null) AddEntryToList(entry);
        }

        private void Form1_KeyPress(object? sender, KeyPressEventArgs e)
        {
            string charDisplay = e.KeyChar switch
            {
                '\r' => "[Enter]",
                '\b' => "[Backspace]",
                '\t' => "[Tab]",
                ' ' => "[Space]",
                (char)27 => "[Esc]",
                _ when char.IsControl(e.KeyChar) => $"[Ctrl+{(int)e.KeyChar}]",
                _ => e.KeyChar.ToString()
            };

            string modifiers = GetModifiersText(Control.ModifierKeys);
            var entry = _logManager.AddEntry("KeyPress", charDisplay, "Char", (int)e.KeyChar, modifiers);
            if (entry != null) AddEntryToList(entry);
        }

        private void Form1_KeyUp(object? sender, KeyEventArgs e)
        {
            string modifiers = GetModifiersText(e.Modifiers);
            var entry = _logManager.AddEntry("KeyUp", e.KeyCode.ToString(), e.KeyCode.ToString(), e.KeyValue, modifiers);
            if (entry != null) AddEntryToList(entry);
        }

        private void AddEntryToList(KeyLogEntry entry)
        {
            string formattedLine = $"#{entry.SequenceNumber,-4} | {entry.Timestamp:HH:mm:ss.fff} | {entry.EventType,-8} | Tuş: {entry.KeyName,-12} | Kod: {entry.KeyValue,-4} | Değiştirici: {entry.Modifiers}";

            // En son olay her zaman en üstte görünecek şekilde 0. indekse ekliyoruz
            _lstLogs.Items.Insert(0, formattedLine);

            _lblLastKey.Text = $"Son Olay: {entry.EventType} → {entry.KeyName} (Değer: {entry.KeyValue}, Değiştirici: {entry.Modifiers})";
            UpdateStatsLabel();
        }

        private void UpdateStatsLabel()
        {
            _lblStats.Text = $"Toplam: {_logManager.TotalCount}  |  KeyDown: {_logManager.KeyDownCount}  |  KeyPress: {_logManager.KeyPressCount}  |  KeyUp: {_logManager.KeyUpCount}";
        }

        private void UpdateRecordingStateUI()
        {
            if (_logManager.IsRecording)
            {
                _lblStatusBadge.Text = "● KAYIT AKTİF (Yalnızca Bu Pencere Odaktayken İzlenir)";
                _lblStatusBadge.ForeColor = Color.FromArgb(22, 163, 74);
                _btnStart.Enabled = false;
                _btnStop.Enabled = true;
            }
            else
            {
                _lblStatusBadge.Text = "■ KAYIT DURDURULDU";
                _lblStatusBadge.ForeColor = Color.FromArgb(220, 38, 38);
                _btnStart.Enabled = true;
                _btnStop.Enabled = false;
            }
        }

        private void BtnStart_Click(object? sender, EventArgs e)
        {
            _logManager.IsRecording = true;
            UpdateRecordingStateUI();
        }

        private void BtnStop_Click(object? sender, EventArgs e)
        {
            _logManager.IsRecording = false;
            UpdateRecordingStateUI();
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            _logManager.Clear();
            _lstLogs.Items.Clear();
            _lblLastKey.Text = "Son Olay: Geçmiş temizlendi";
            UpdateStatsLabel();
        }

        private void BtnExportTxt_Click(object? sender, EventArgs e)
        {
            if (_logManager.TotalCount == 0)
            {
                MessageBox.Show("Dışa aktarılacak kayıt bulunmuyor.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "Metin Dosyası (*.txt)|*.txt",
                FileName = $"KeyLog_{DateTime.Now:yyyyMMdd_HHmmss}.txt",
                Title = "Kayıtları TXT Olarak Kaydet"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                _logManager.ExportToTxt(sfd.FileName);
                MessageBox.Show("Kayıtlar TXT dosyası olarak başarıyla kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnExportCsv_Click(object? sender, EventArgs e)
        {
            if (_logManager.TotalCount == 0)
            {
                MessageBox.Show("Dışa aktarılacak kayıt bulunmuyor.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "CSV Dosyası (*.csv)|*.csv",
                FileName = $"KeyLog_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                Title = "Kayıtları CSV Olarak Kaydet"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                _logManager.ExportToCsv(sfd.FileName);
                MessageBox.Show("Kayıtlar CSV dosyası olarak başarıyla kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}