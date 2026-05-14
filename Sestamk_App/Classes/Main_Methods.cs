using Guna.UI2.WinForms;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Management;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Sestamk.Classes
{
    public static class Main_Methods
    {
        public static Dictionary<Type, Form> _openForms = new Dictionary<Type, Form>();


        private static Point _startCursor;
        private static Point _startLocation;
        private static bool _isDragging;


        public static bool IsInternetAvailable()
        {
			try
			{
				using (Ping myping = new Ping()) 
				{
					PingReply pingReply = myping.Send("8.8.8.8", 2000);
					return (pingReply != null && pingReply.Status == IPStatus.Success);
				}
			}
			catch (Exception)
			{

				return false;
			}
        }
        public static string GetMacAddress()
        {
            try
            {
                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (var netInterface in networkInterfaces)
                {
                    if (netInterface.OperationalStatus == OperationalStatus.Up &&
                        netInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                        netInterface.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                    {
                        var bytes = netInterface.GetPhysicalAddress().GetAddressBytes();

                        if (bytes.Length == 6)
                        {
                            return BitConverter.ToString(bytes); // AA-BB-CC-DD-EE-FF
                        }
                    }
                }
            }
            catch (Exception)
            {
                return "لا يمكن الحصول على عنوان MAC";
            }

            return "لا يمكن الحصول على عنوان MAC";
        }
        private static string _cachedExternalIp;
        private static DateTime _cachedExternalIpAt;
        private static readonly TimeSpan _externalIpTtl = TimeSpan.FromMinutes(30);
        private static readonly object _externalIpLock = new object();

        public static string GetExternalIp()
        {
            lock (_externalIpLock)
            {
                if (_cachedExternalIp != null && DateTime.UtcNow - _cachedExternalIpAt < _externalIpTtl)
                    return _cachedExternalIp;
            }

            string result;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    result = client.GetStringAsync("https://api.ipify.org").Result;
                }
            }
            catch
            {
                result = "لا يمكن الحصول على IP الخارجي";
            }

            lock (_externalIpLock)
            {
                _cachedExternalIp = result;
                _cachedExternalIpAt = DateTime.UtcNow;
            }
            return result;
        }
        public static string GetMachineGuid()
        {
            try
            {
                using (var regKey = Registry.LocalMachine
                    .OpenSubKey(@"SOFTWARE\Microsoft\Cryptography"))
                {
                    return regKey?.GetValue("MachineGuid")?.ToString() ?? "UNKNOWN_GUID";
                }
            }
            catch
            {
                return "UNKNOWN_GUID";
            }
        }
        public static string GetCpuId()
        {
            try
            {
                var searcher = new ManagementObjectSearcher(
                    "SELECT ProcessorId FROM Win32_Processor");

                foreach (ManagementObject obj in searcher.Get())
                    return obj["ProcessorId"]?.ToString() ?? "UNKNOWN_CPU";
            }
            catch { }

            return "UNKNOWN_CPU";
        }
        public static string GetMotherboardSerial()
        {
            try
            {
                var searcher = new ManagementObjectSearcher(
                    "SELECT SerialNumber FROM Win32_BaseBoard");

                foreach (ManagementObject obj in searcher.Get())
                    return obj["SerialNumber"]?.ToString() ?? "UNKNOWN_MB";
            }
            catch { }

            return "UNKNOWN_MB";
        }
        public static string GenerateHWID(string input)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder sb = new StringBuilder();

                foreach (byte b in bytes)
                    sb.Append(b.ToString("X2"));

                return sb.ToString();
            }
        }
        public static void OpenForm(Type formType)
        {
            Form openedform = Application.OpenForms
                .Cast<Form>()
                .FirstOrDefault(f => f.GetType() == formType);

            if (openedform != null)
            {
                openedform.WindowState = FormWindowState.Normal;
                openedform.BringToFront();
                openedform.Activate();

            }
            else
            {
                Form frm = (Form)Activator.CreateInstance(formType);
                frm.Show();
            }
        }


        public static void StyleDataGridView(Guna2DataGridView dgv)
        {
            // ── إعدادات عامة ──────────────────────────────────────
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeColumns = true;
            dgv.AllowUserToResizeRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgv.ScrollBars = ScrollBars.Both;
            dgv.ColumnHeadersHeight = 50;
            dgv.BackgroundColor = Color.FromArgb(15, 23, 42);
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            dgv.EditMode = DataGridViewEditMode.EditProgrammatically;
            // Stretch last column to fill remaining space after data binding
            dgv.DataBindingComplete += (s, e) =>
            {
                if (dgv.Columns.Count > 0)
                {
                    // Apply minimum width then let the last column fill remaining space
                    foreach (DataGridViewColumn col in dgv.Columns)
                        col.MinimumWidth = 60;
                    dgv.Columns[dgv.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
            };
            dgv.GridColor = Color.FromArgb(30, 41, 59);
            dgv.MultiSelect = false;
            dgv.ReadOnly = true;
            dgv.RightToLeft = RightToLeft.Yes;
            dgv.RowHeadersVisible = false;
            dgv.RowTemplate.Height = 50;

            // ── AlternatingRows Style ──────────────────────────────
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(17, 24, 39);
            dgv.AlternatingRowsDefaultCellStyle.ForeColor = Color.FromArgb(226, 232, 240);
            dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(51, 65, 85);
            dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.White;

            // ── Column Header Style ────────────────────────────────
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 23, 42);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Alexandria", 14F, FontStyle.Bold, GraphicsUnit.Point, 0);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(148, 163, 184);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(15, 23, 42);
            dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.FromArgb(148, 163, 184);
            dgv.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;

            // ── Default Cell Style ─────────────────────────────────
            dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.DefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
            dgv.DefaultCellStyle.Font = new Font("Alexandria", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            dgv.DefaultCellStyle.ForeColor = Color.FromArgb(226, 232, 240);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(51, 65, 85);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.False;

            // ── ThemeStyle ─────────────────────────────────────────
            dgv.ThemeStyle.BackColor = Color.FromArgb(15, 23, 42);
            dgv.ThemeStyle.GridColor = Color.FromArgb(30, 41, 59);
            dgv.ThemeStyle.ReadOnly = true;

            dgv.ThemeStyle.AlternatingRowsStyle.BackColor = Color.FromArgb(17, 24, 39);
            dgv.ThemeStyle.AlternatingRowsStyle.Font = null;
            dgv.ThemeStyle.AlternatingRowsStyle.ForeColor = Color.FromArgb(226, 232, 240);
            dgv.ThemeStyle.AlternatingRowsStyle.SelectionBackColor = Color.FromArgb(51, 65, 85);
            dgv.ThemeStyle.AlternatingRowsStyle.SelectionForeColor = Color.White;

            dgv.ThemeStyle.HeaderStyle.BackColor = Color.FromArgb(15, 23, 42);
            dgv.ThemeStyle.HeaderStyle.BorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.ThemeStyle.HeaderStyle.Font = new Font("Alexandria", 14F, FontStyle.Bold, GraphicsUnit.Point, 0);
            dgv.ThemeStyle.HeaderStyle.ForeColor = Color.FromArgb(148, 163, 184);
            dgv.ThemeStyle.HeaderStyle.HeaightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            dgv.ThemeStyle.HeaderStyle.Height = 50;

            dgv.ThemeStyle.RowsStyle.BackColor = Color.FromArgb(30, 41, 59);
            dgv.ThemeStyle.RowsStyle.BorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.ThemeStyle.RowsStyle.Font = new Font("Alexandria", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            dgv.ThemeStyle.RowsStyle.ForeColor = Color.FromArgb(226, 232, 240);
            dgv.ThemeStyle.RowsStyle.Height = 50;
            dgv.ThemeStyle.RowsStyle.SelectionBackColor = Color.FromArgb(51, 65, 85);
            dgv.ThemeStyle.RowsStyle.SelectionForeColor = Color.White;
        }



        public static void FillComboBoxWithGridHeaders(DataGridView dgv, ComboBox cbo)
        {
            try
            {
                cbo.Items.Clear();

                foreach (DataGridViewColumn col in dgv.Columns)
                {
                    if (col.Visible && !string.IsNullOrWhiteSpace(col.HeaderText))
                    {
                        cbo.Items.Add(col.HeaderText.Trim());
                    }
                }

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ أثناء تحميل الحقول: " + ex.Message);
            }
        }


        public static void OpenForm<T>(Action onClosed = null) where T : Form, new ()
        {
            Type formType =typeof(T);

            // لو الفورم شغاله
            if (_openForms.ContainsKey(formType) && !_openForms[formType].IsDisposed)
            {
                Form existingForm = _openForms[formType];
                if (existingForm.WindowState == FormWindowState.Minimized)
                {
                    existingForm.WindowState = FormWindowState.Normal;
                }
                existingForm.Focus();
            }
            else
            {
                T newform = new T();
                _openForms[formType] = newform;

                if (onClosed != null)
                {
                    newform.FormClosed += (s, e) => onClosed();
                }
                newform.Show();
            }
        }


        public static void Attach(Control handle, Form targetForm)
        {
            handle.MouseDown += (s, e) =>
            {
                if (e.Button != MouseButtons.Left) return;
                _isDragging = true;
                _startCursor = Cursor.Position;
                _startLocation = targetForm.Location;
            };

            handle.MouseMove += (s, e) =>
            {
                if (!_isDragging) return;

                int deltaX = Cursor.Position.X - _startCursor.X;
                int deltaY = Cursor.Position.Y - _startCursor.Y;

                targetForm.Location = new Point(
                    _startLocation.X + deltaX,
                    _startLocation.Y + deltaY
                );
            };

            handle.MouseUp += (s, e) =>
            {
                _isDragging = false;
            };
        }

        /// <summary>
        /// نفس الفكرة بس لو عايز تحرك UserControl أو Panel جوه فورم
        /// </summary>
        public static void AttachToControl(Control handle, Control target)
        {
            handle.MouseDown += (s, e) =>
            {
                if (e.Button != MouseButtons.Left) return;
                _isDragging = true;
                _startCursor = Cursor.Position;
                _startLocation = target.Location;
            };

            handle.MouseMove += (s, e) =>
            {
                if (!_isDragging) return;

                int deltaX = Cursor.Position.X - _startCursor.X;
                int deltaY = Cursor.Position.Y - _startCursor.Y;

                target.Location = new Point(
                    _startLocation.X + deltaX,
                    _startLocation.Y + deltaY
                );
            };

            handle.MouseUp += (s, e) =>
            {
                _isDragging = false;
            };
        }


        public static void SetClipboardTextSafe(string text)
        {
            Thread thread = new Thread(() =>
            {
                Clipboard.SetText(text);
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

    }
}
