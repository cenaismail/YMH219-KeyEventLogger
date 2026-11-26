using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace YMH219_KeyEventLogger
{
    // NOT: Arayüz kontrolleri InitializeUi() metodunda kodla oluşturulur.
    public partial class Form1 : Form
    {
        private readonly KeyLogManager _manager = new KeyLogManager();
        private readonly HashSet<Keys> _pressedKeys = new HashSet<Keys>(); // tuş tekrarını engeller

        private ListBox lstLog;
        private Button btnStart, btnStop, btnClear, btnSave;
        private Label lblStatus, lblCount;
        private Panel pnlIndicator;

        public Form1()
        {
            InitializeUi();

            KeyPreview = true;          // Tuşlar önce forma gelsin (yalnızca bu pencere aktifken)
            KeyDown += Form1_KeyDown;
            KeyUp += Form1_KeyUp;
            Deactivate += Form1_Deactivate;
            _manager.EntryAdded += OnEntryAdded;

            UpdateState();
        }

        private void InitializeUi()
        {
            Text = "Key Event Logger (Eğitsel)";
            ClientSize = new Size(560, 440);
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(480, 360);

            pnlIndicator = new Panel { Size = new Size(16, 16), Location = new Point(12, 16) };
            lblStatus = new Label { AutoSize = true, Location = new Point(34, 15), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            lblCount = new Label { AutoSize = true, Location = new Point(400, 15), Anchor = AnchorStyles.Top | AnchorStyles.Right };

            btnStart = MakeButton("Başlat", 12);
            btnStop = MakeButton("Durdur", 112);
            btnClear = MakeButton("Temizle", 212);
            btnSave = MakeButton("Kaydet (.txt)", 312);
            btnSave.Width = 120;

            btnStart.Click += (s, e) => { _manager.Start(); UpdateState(); ReturnFocus(); };
            btnStop.Click += (s, e) => { _manager.Stop(); _pressedKeys.Clear(); UpdateState(); ReturnFocus(); };
            btnClear.Click += (s, e) => { _manager.Clear(); lstLog.Items.Clear(); UpdateState(); ReturnFocus(); };
            btnSave.Click += BtnSave_Click;

            lstLog = new ListBox
            {
                Location = new Point(12, 90),
                Size = new Size(536, 338),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font("Consolas", 9.5f),
                TabStop = false
            };

            Controls.AddRange(new Control[] { pnlIndicator, lblStatus, lblCount, btnStart, btnStop, btnClear, btnSave, lstLog });
        }

        private Button MakeButton(string text, int x) => new Button
        {
            Text = text,
            Location = new Point(x, 45),
            Size = new Size(94, 32),
            TabStop = false   // Boşluk/Enter tuşu butonları tetiklemesin
        };

        /// <summary>
        /// Butona tıklandıktan sonra odağı listeye verir. Aksi halde odak bir butonda kalır
        /// ve Space/Enter tuşu kaydedilmek yerine o butona basar (ör. kaydı durdurur).
        /// </summary>
        private void ReturnFocus()
        {
            ActiveControl = lstLog;
        }

        /// <summary>
        /// Pencere odağı kaybedince basılı tuş listesini temizler. Aksi halde başka pencereye
        /// geçerken basılı olan tuşun KeyUp olayı gelmez ve o tuşun sonraki basışı kaydedilmez.
        /// </summary>
        private void Form1_Deactivate(object? sender, EventArgs e)
        {
            _pressedKeys.Clear();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (!_manager.IsRecording) return;
            if (!_pressedKeys.Add(e.KeyCode)) return;   // basılı tutma tekrarını atla
            _manager.Add(e.KeyData.ToString(), "KeyDown");
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            _pressedKeys.Remove(e.KeyCode);
            if (!_manager.IsRecording) return;
            _manager.Add(e.KeyCode.ToString(), "KeyUp");
        }

        private void OnEntryAdded(KeyLogEntry entry)
        {
            lstLog.Items.Add(entry.ToString());
            lstLog.TopIndex = lstLog.Items.Count - 1;   // en yeni kayda kaydır
            UpdateState();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (_manager.Count == 0)
            {
                MessageBox.Show("Kaydedilecek kayıt yok.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dlg = new SaveFileDialog
            {
                Filter = "Metin dosyası (*.txt)|*.txt",
                FileName = $"keylog_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
            })
            {
                if (dlg.ShowDialog() != DialogResult.OK) { ReturnFocus(); return; }

                try
                {
                    _manager.SaveToFile(dlg.FileName);
                    MessageBox.Show("Kayıtlar kaydedildi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Dosya kaydedilemedi:\n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                ReturnFocus();
            }
        }

        private void UpdateState()
        {
            bool rec = _manager.IsRecording;
            pnlIndicator.BackColor = rec ? Color.Red : Color.Gray;
            lblStatus.Text = rec ? "KAYIT AÇIK (yalnızca bu pencere)" : "KAYIT KAPALI";
            lblCount.Text = $"Kayıt sayısı: {_manager.Count}";
            btnStart.Enabled = !rec;
            btnStop.Enabled = rec;
            btnClear.Enabled = _manager.Count > 0;
            btnSave.Enabled = _manager.Count > 0;
        }
    }
}