using Guna.UI2.WinForms;
using Microsoft.Data.SqlClient;
using Sestamk.Classes;
using Sestamk.Classes.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Sestamk.Forms
{
    public partial class frmTables : BaseForm
    {
        // ═══════════════════════════════════════════
        //  📊 Data
        // ═══════════════════════════════════════════
        private BindingList<TableModel> _tablesList = new BindingList<TableModel>();
        private BindingSource _tablesBinding = new BindingSource();
        private List<TableModel> _allTables = new List<TableModel>();
        private long _selectedTableId = 0;
        private DataTable _sectionsTable = null;

        public frmTables()
        {
            InitializeComponent();
            SetupEvents();
            SetupGrid();
            _ = InitializeDataAsync();
            Main_Methods.Attach(lblTitle, this);
        }

        private async Task InitializeDataAsync()
        {
            await LoadSectionsAsync();
            await LoadTablesAsync();
        }

        // ═══════════════════════════════════════════
        //  🔧 ربط الأحداث + إعداد الجريد
        // ═══════════════════════════════════════════
        private void SetupEvents()
        {
            Main_Methods.Attach(pnlHeader, this);
            btnClose.Click += (s, e) => this.Close();

            // أزرار الأقسام
            btnAddSection.Click += BtnAddSection_Click;
            btnDeleteSection.Click += BtnDeleteSection_Click;

            // أزرار الطاولات
            btnSave.Click += BtnSave_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnClear.Click += (s, e) => ClearTableForm();

            // البحث والفلترة
            txtSearch.TextChanged += (s, e) => ApplyFilter();
            cmbFilterStatus.SelectedIndexChanged += (s, e) => ApplyFilter();
            cmbFilterSection.SelectedIndexChanged += (s, e) => ApplyFilter();
        }

        private void SetupGrid()
        {
            Main_Methods.StyleDataGridView(dgvTables);

            // تفعيل الاسكرول الأفقي
            dgvTables.ScrollBars = ScrollBars.Both;
            dgvTables.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            dgvTables.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "colId", HeaderText = "الرقم التعريفي", DataPropertyName = "Id", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "colSection", HeaderText = "اسم القسم", DataPropertyName = "SectionName", Width = 180 },
                new DataGridViewTextBoxColumn { Name = "colNumber", HeaderText = "رقم الطاولة", DataPropertyName = "TableNumber", Width = 130 },
                new DataGridViewTextBoxColumn { Name = "colName", HeaderText = "اسم الطاولة", DataPropertyName = "TableName", Width = 200 },
                new DataGridViewTextBoxColumn { Name = "colCapacity", HeaderText = "عدد الأشخاص", DataPropertyName = "Capacity", Width = 130 },
                new DataGridViewTextBoxColumn { Name = "colStatus", HeaderText = "الحالة", DataPropertyName = "Status", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "colQRCode", HeaderText = "باركود الطاولة", DataPropertyName = "QRCode", Width = 200 },
                new DataGridViewCheckBoxColumn { Name = "colActive", HeaderText = "نشط", DataPropertyName = "IsActive", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "colNotes", HeaderText = "ملاحظات", DataPropertyName = "Notes", Width = 250 },
            });

            _tablesBinding.DataSource = _tablesList;
            dgvTables.DataSource = _tablesBinding;
            dgvTables.CellClick += DgvTables_CellClick;
        }

        // ═══════════════════════════════════════════
        //  🔄 تحميل الأقسام
        // ═══════════════════════════════════════════
        private async Task LoadSectionsAsync()
        {
            try
            {
                string query = @"SELECT Id, SectionName, Description, SortOrder 
                                 FROM Sections 
                                 WHERE IsActive = 1 AND DeletedAt IS NULL 
                                 ORDER BY SortOrder";

                _sectionsTable = await DB_Server.GetTableAsync(query);

                cmbSection.Items.Clear();
                cmbSection.Items.Add("-- اختر القسم --");
                foreach (DataRow row in _sectionsTable.Rows)
                    cmbSection.Items.Add(row["SectionName"].ToString());
                cmbSection.SelectedIndex = 0;

                cmbFilterSection.Items.Clear();
                cmbFilterSection.Items.Add("كل الأقسام");
                foreach (DataRow row in _sectionsTable.Rows)
                    cmbFilterSection.Items.Add(row["SectionName"].ToString());
                cmbFilterSection.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في تحميل الأقسام: " + ex.Message);
            }
        }

        // ═══════════════════════════════════════════
        //  🔄 تحميل الطاولات
        // ═══════════════════════════════════════════
        private async Task LoadTablesAsync()
        {
            try
            {
                string query = @"SELECT t.Id, t.SectionId, t.TableNumber, t.TableName, 
                                        t.Capacity, t.Status, t.IsActive, t.Notes, t.QRCode,
                                        s.SectionName
                                 FROM Tables t
                                 LEFT JOIN Sections s ON t.SectionId = s.Id
                                 WHERE t.DeletedAt IS NULL
                                 ORDER BY t.Id DESC";

                DataTable dt = await DB_Server.GetTableAsync(query);

                _tablesList.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    _tablesList.Add(new TableModel
                    {
                        Id = Convert.ToInt64(row["Id"]),
                        SectionId = Convert.ToInt64(row["SectionId"]),
                        TableNumber = row["TableNumber"]?.ToString() ?? "",
                        TableName = row["TableName"]?.ToString() ?? "",
                        Capacity = Convert.ToInt32(row["Capacity"]),
                        Status = row["Status"]?.ToString() ?? "available",
                        IsActive = row["IsActive"] != DBNull.Value && Convert.ToBoolean(row["IsActive"]),
                        Notes = row["Notes"] == DBNull.Value ? "" : row["Notes"].ToString(),
                        QRCode = row["QRCode"] == DBNull.Value ? "" : row["QRCode"].ToString(),
                        SectionName = row["SectionName"] == DBNull.Value ? "" : row["SectionName"].ToString(),
                    });
                }

                _allTables = _tablesList.ToList();
                _tablesBinding.ResetBindings(false);
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في تحميل الطاولات: " + ex.Message);
            }
        }

        // ═══════════════════════════════════════════
        //  🔍 الفلترة
        // ═══════════════════════════════════════════
        private void ApplyFilter()
        {
            string keyword = (txtSearch.Text ?? "").Trim().ToLower();
            string statusFilter = cmbFilterStatus.SelectedItem?.ToString() ?? "كل الحالات";
            string sectionFilter = cmbFilterSection.SelectedItem?.ToString() ?? "كل الأقسام";

            IEnumerable<TableModel> result = _allTables;

            if (statusFilter != "كل الحالات")
                result = result.Where(t => t.Status == statusFilter);

            if (sectionFilter != "كل الأقسام")
                result = result.Where(t => t.SectionName == sectionFilter);

            if (!string.IsNullOrEmpty(keyword))
            {
                result = result.Where(t =>
                    (t.TableNumber ?? "").ToLower().Contains(keyword) ||
                    (t.TableName ?? "").ToLower().Contains(keyword) ||
                    (t.SectionName ?? "").ToLower().Contains(keyword));
            }

            _tablesList.Clear();
            foreach (var t in result) _tablesList.Add(t);
            _tablesBinding.ResetBindings(false);
        }

        // ═══════════════════════════════════════════
        //  📋 ملء الفورم
        // ═══════════════════════════════════════════
        private void DgvTables_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgvTables.Rows[e.RowIndex];
            if (row.Cells["colId"].Value == null) return;

            _selectedTableId = Convert.ToInt64(row.Cells["colId"].Value);
            txtTableNumber.Text = row.Cells["colNumber"].Value?.ToString() ?? "";
            txtTableName.Text = row.Cells["colName"].Value?.ToString() ?? "";
            txtCapacity.Text = row.Cells["colCapacity"].Value?.ToString() ?? "";
            txtNotes.Text = row.Cells["colNotes"].Value?.ToString() ?? "";
            txtQRCode.Text = row.Cells["colQRCode"].Value?.ToString() ?? "";

            string sectionName = row.Cells["colSection"].Value?.ToString() ?? "";
            int secIdx = cmbSection.Items.IndexOf(sectionName);
            cmbSection.SelectedIndex = secIdx >= 0 ? secIdx : 0;

            var tableModel = _tablesList.FirstOrDefault(t => t.Id == _selectedTableId);
            if (tableModel != null)
            {
                int statusIdx = cmbStatus.Items.IndexOf(tableModel.Status);
                cmbStatus.SelectedIndex = statusIdx >= 0 ? statusIdx : 0;
            }
        }

        // ═══════════════════════════════════════════
        //  ➕ إضافة قسم
        // ═══════════════════════════════════════════
        private async void BtnAddSection_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSectionName.Text))
            {
                ToastManager.ShowWarning("تنبيه", "أدخل اسم القسم");
                return;
            }

            try
            {
                string query = @"INSERT INTO Sections (SectionName, Description, SortOrder) 
                                 VALUES (@name, @desc, (SELECT ISNULL(MAX(SortOrder),0)+1 FROM Sections))";
                await DB_Server.ExecuteAsync(query, new[]
                {
                    new SqlParameter("@name", txtSectionName.Text.Trim()),
                    new SqlParameter("@desc", txtSectionDesc.Text.Trim())
                });

                await LoadSectionsAsync();
                txtSectionName.Text = "";
                txtSectionDesc.Text = "";
                ToastManager.ShowSuccess("نجاح", "تم إضافة القسم ✅");
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في إضافة القسم: " + ex.Message);
            }
        }

        // ═══════════════════════════════════════════
        //  🗑️ حذف قسم
        // ═══════════════════════════════════════════
        private async void BtnDeleteSection_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSectionName.Text))
            {
                ToastManager.ShowWarning("تنبيه", "أدخل اسم القسم للحذف");
                return;
            }

            if (!frmConfirm.Show("تأكيد", $"حذف القسم \"{txtSectionName.Text}\"؟")) return;

            try
            {
                string query = "UPDATE Sections SET IsActive=0, DeletedAt=GETDATE() WHERE SectionName=@name";
                await DB_Server.ExecuteAsync(query, new[] { new SqlParameter("@name", txtSectionName.Text.Trim()) });
                await LoadSectionsAsync();
                txtSectionName.Text = "";
                txtSectionDesc.Text = "";
                ToastManager.ShowSuccess("نجاح", "تم حذف القسم ✅");
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في حذف القسم: " + ex.Message);
            }
        }

        // ═══════════════════════════════════════════
        //  ➕ إضافة طاولة
        // ═══════════════════════════════════════════
        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateTableInputs()) return;

            try
            {
                long sectionId = GetSelectedSectionId();
                // توليد باركود تلقائي لو المستخدم ما دخلش
                string qrCode = txtQRCode.Text.Trim();
                if (string.IsNullOrEmpty(qrCode))
                {
                    qrCode = $"TBL-{sectionId}-{txtTableNumber.Text.Trim()}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
                }

                string query = @"INSERT INTO Tables (SectionId, TableNumber, TableName, Capacity, Status, IsActive, Notes, QRCode) 
                                 VALUES (@section, @number, @name, @capacity, @status, 1, @notes, @qrcode)";

                await DB_Server.ExecuteAsync(query, new[]
                {
                    new SqlParameter("@section", sectionId),
                    new SqlParameter("@number", txtTableNumber.Text.Trim()),
                    new SqlParameter("@name", txtTableName.Text.Trim()),
                    new SqlParameter("@capacity", int.TryParse(txtCapacity.Text, out int cap) ? cap : 1),
                    new SqlParameter("@status", cmbStatus.SelectedItem?.ToString() ?? "available"),
                    new SqlParameter("@notes", txtNotes.Text.Trim()),
                    new SqlParameter("@qrcode", qrCode),
                });

                await LoadTablesAsync();
                ClearTableForm();
                ToastManager.ShowSuccess("نجاح", "تم إضافة الطاولة ✅");
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في الإضافة: " + ex.Message);
            }
        }

        // ═══════════════════════════════════════════
        //  ✏️ تعديل طاولة
        // ═══════════════════════════════════════════
        private async void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (_selectedTableId == 0)
            {
                ToastManager.ShowWarning("تنبيه", "اختر طاولة أولاً");
                return;
            }
            if (!ValidateTableInputs()) return;

            try
            {
                long sectionId = GetSelectedSectionId();
                string query = @"UPDATE Tables SET SectionId=@section, TableNumber=@number, 
                                 TableName=@name, Capacity=@capacity, Status=@status, Notes=@notes, QRCode=@qrcode
                                 WHERE Id=@id";

                await DB_Server.ExecuteAsync(query, new[]
                {
                    new SqlParameter("@id", _selectedTableId),
                    new SqlParameter("@section", sectionId),
                    new SqlParameter("@number", txtTableNumber.Text.Trim()),
                    new SqlParameter("@name", txtTableName.Text.Trim()),
                    new SqlParameter("@capacity", int.TryParse(txtCapacity.Text, out int cap) ? cap : 1),
                    new SqlParameter("@status", cmbStatus.SelectedItem?.ToString() ?? "available"),
                    new SqlParameter("@notes", txtNotes.Text.Trim()),
                    new SqlParameter("@qrcode", txtQRCode.Text.Trim()),
                });

                await LoadTablesAsync();
                ClearTableForm();
                ToastManager.ShowSuccess("نجاح", "تم التعديل ✅");
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في التعديل: " + ex.Message);
            }
        }

        // ═══════════════════════════════════════════
        //  🗑️ حذف طاولة
        // ═══════════════════════════════════════════
        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedTableId == 0)
            {
                ToastManager.ShowWarning("تنبيه", "اختر طاولة أولاً");
                return;
            }

            if (!frmConfirm.Show("تأكيد", "حذف هذه الطاولة؟")) return;

            try
            {
                string query = "UPDATE Tables SET IsActive=0, DeletedAt=GETDATE() WHERE Id=@id";
                await DB_Server.ExecuteAsync(query, new[] { new SqlParameter("@id", _selectedTableId) });
                await LoadTablesAsync();
                ClearTableForm();
                ToastManager.ShowSuccess("نجاح", "تم الحذف ✅");
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في الحذف: " + ex.Message);
            }
        }

        // ═══════════════════════════════════════════
        //  🛠️ مساعدات
        // ═══════════════════════════════════════════
        private bool ValidateTableInputs()
        {
            if (cmbSection.SelectedIndex <= 0)
            {
                ToastManager.ShowWarning("تنبيه", "اختر القسم");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtTableNumber.Text))
            {
                ToastManager.ShowWarning("تنبيه", "أدخل رقم الطاولة");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtTableName.Text))
            {
                ToastManager.ShowWarning("تنبيه", "أدخل اسم الطاولة");
                return false;
            }
            if (!int.TryParse(txtCapacity.Text, out int cap) || cap <= 0)
            {
                ToastManager.ShowWarning("تنبيه", "أدخل سعة صحيحة (أكبر من 0)");
                return false;
            }
            return true;
        }

        private long GetSelectedSectionId()
        {
            if (_sectionsTable == null || cmbSection.SelectedIndex <= 0) return 0;
            int dataIndex = cmbSection.SelectedIndex - 1;
            if (dataIndex >= 0 && dataIndex < _sectionsTable.Rows.Count)
                return Convert.ToInt64(_sectionsTable.Rows[dataIndex]["Id"]);
            return 0;
        }

        private void ClearTableForm()
        {
            _selectedTableId = 0;
            txtTableNumber.Text = "";
            txtTableName.Text = "";
            txtCapacity.Text = "";
            txtNotes.Text = "";
            txtQRCode.Text = "";
            cmbSection.SelectedIndex = 0;
            cmbStatus.SelectedIndex = 0;
        }
    }
}
