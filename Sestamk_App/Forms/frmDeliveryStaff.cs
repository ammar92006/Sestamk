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
    public partial class frmDeliveryStaff : BaseForm
    {
        protected override Size DesignClientSize => new Size(1400, 850);

        // ═══════════════════════════════════════════
        //  📊 Data
        // ═══════════════════════════════════════════
        private BindingList<DeliveryStaffModel> _driversList = new BindingList<DeliveryStaffModel>();
        private BindingSource _driversBinding = new BindingSource();
        private List<DeliveryStaffModel> _allDrivers = new List<DeliveryStaffModel>();
        private int _selectedDriverId = 0;

        public frmDeliveryStaff()
        {
            InitializeComponent();
            SetupEvents();
            SetupGrid();
            _ = LoadDriversAsync();
        }

        // ═══════════════════════════════════════════
        //  🔧 ربط الأحداث + إعداد الجريد
        // ═══════════════════════════════════════════
        private void SetupEvents()
        {
            Main_Methods.Attach(lblTitle, this);
            btnClose.Click += (s, e) => this.Close();
            btnSave.Click += BtnSave_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnClear.Click += (s, e) => ClearForm();
            txtSearch.TextChanged += (s, e) => ApplyFilter();
            cmbFilterStatus.SelectedIndexChanged += (s, e) => ApplyFilter();
        }

        private void SetupGrid()
        {
            Main_Methods.StyleDataGridView(dgvDrivers);
            dgvDrivers.AutoGenerateColumns = false;
            dgvDrivers.ScrollBars = ScrollBars.Both;

            dgvDrivers.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "colId", HeaderText = "الرقم التعريفي", DataPropertyName = "DeliveryId", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "colName", HeaderText = "اسم الطيار", DataPropertyName = "FullName", Width = 200 },
                new DataGridViewTextBoxColumn { Name = "colPhone", HeaderText = "رقم التلفون", DataPropertyName = "Phone", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "colVehicle", HeaderText = "نوع المركبة", DataPropertyName = "VehicleType", Width = 130 },
                new DataGridViewTextBoxColumn { Name = "colFee", HeaderText = "سعر التوصيلة", DataPropertyName = "DeliveryFee", Width = 130 },
                new DataGridViewTextBoxColumn { Name = "colLicense", HeaderText = "رقم الرخصة", DataPropertyName = "LicenseNumber", Width = 140 },
                new DataGridViewTextBoxColumn { Name = "colSalaryType", HeaderText = "نوع المرتب", DataPropertyName = "SalaryType", Width = 130 },
                new DataGridViewCheckBoxColumn { Name = "colActive", HeaderText = "نشط", DataPropertyName = "IsActive", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "colNotes", HeaderText = "ملاحظات", DataPropertyName = "Notes", Width = 250 },
            });

            _driversBinding.DataSource = _driversList;
            dgvDrivers.DataSource = _driversBinding;
            dgvDrivers.CellClick += DgvDrivers_CellClick;
        }

        // ═══════════════════════════════════════════
        //  🔄 تحميل البيانات
        // ═══════════════════════════════════════════
        private async Task LoadDriversAsync()
        {
            try
            {
                string query = @"SELECT delivery_id, full_name, phone, vehicle_type, 
                                        license_number, delivery_fee, salary_type,
                                        shift_start, shift_end, is_active, hire_date, notes
                                 FROM delivery_staff
                                 ORDER BY delivery_id DESC";

                DataTable dt = await DB_Server.GetTableAsync(query);

                _driversList.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    _driversList.Add(new DeliveryStaffModel
                    {
                        DeliveryId = Convert.ToInt32(row["delivery_id"]),
                        FullName = row["full_name"]?.ToString() ?? "",
                        Phone = row["phone"]?.ToString() ?? "",
                        VehicleType = row["vehicle_type"]?.ToString() ?? "",
                        LicenseNumber = row["license_number"] == DBNull.Value ? "" : row["license_number"].ToString(),
                        DeliveryFee = row["delivery_fee"] == DBNull.Value ? 0m : Convert.ToDecimal(row["delivery_fee"]),
                        SalaryType = row["salary_type"]?.ToString() ?? "",
                        IsActive = row["is_active"] != DBNull.Value && Convert.ToBoolean(row["is_active"]),
                        Notes = row["notes"] == DBNull.Value ? "" : row["notes"].ToString(),
                    });
                }

                _allDrivers = _driversList.ToList();
                _driversBinding.ResetBindings(false);
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في التحميل: " + ex.Message);
            }
        }

        // ═══════════════════════════════════════════
        //  🔍 الفلترة
        // ═══════════════════════════════════════════
        private void ApplyFilter()
        {
            string keyword = (txtSearch.Text ?? "").Trim().ToLower();
            string status = cmbFilterStatus.SelectedItem?.ToString() ?? "الكل";

            IEnumerable<DeliveryStaffModel> result = _allDrivers;

            if (status == "نشط") result = result.Where(d => d.IsActive);
            else if (status == "غير نشط") result = result.Where(d => !d.IsActive);

            if (!string.IsNullOrEmpty(keyword))
            {
                result = result.Where(d =>
                    (d.FullName ?? "").ToLower().Contains(keyword) ||
                    (d.Phone ?? "").Contains(keyword) ||
                    (d.VehicleType ?? "").ToLower().Contains(keyword));
            }

            _driversList.Clear();
            foreach (var d in result) _driversList.Add(d);
            _driversBinding.ResetBindings(false);
        }

        // ═══════════════════════════════════════════
        //  📋 ملء الفورم عند الضغط على صف
        // ═══════════════════════════════════════════
        private void DgvDrivers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgvDrivers.Rows[e.RowIndex];
            if (row.Cells["colId"].Value == null) return;

            _selectedDriverId = Convert.ToInt32(row.Cells["colId"].Value);
            txtFullName.Text = row.Cells["colName"].Value?.ToString() ?? "";
            txtPhone.Text = row.Cells["colPhone"].Value?.ToString() ?? "";
            txtLicenseNumber.Text = row.Cells["colLicense"].Value?.ToString() ?? "";
            txtDeliveryFee.Text = row.Cells["colFee"].Value?.ToString() ?? "";
            txtNotes.Text = row.Cells["colNotes"].Value?.ToString() ?? "";

            string vt = row.Cells["colVehicle"].Value?.ToString() ?? "";
            int vtIdx = cmbVehicleType.Items.IndexOf(vt);
            cmbVehicleType.SelectedIndex = vtIdx >= 0 ? vtIdx : -1;

            string st = row.Cells["colSalaryType"].Value?.ToString() ?? "";
            int stIdx = cmbSalaryType.Items.IndexOf(st);
            cmbSalaryType.SelectedIndex = stIdx >= 0 ? stIdx : -1;

            var activeVal = row.Cells["colActive"].Value;
            tglIsActive.Checked = activeVal != null && activeVal != DBNull.Value && Convert.ToBoolean(activeVal);
        }

        // ═══════════════════════════════════════════
        //  ➕ إضافة
        // ═══════════════════════════════════════════
        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            try
            {
                string query = @"INSERT INTO delivery_staff 
                                 (full_name, phone, vehicle_type, license_number, delivery_fee, salary_type, is_active, notes)
                                 VALUES (@name, @phone, @vehicle, @license, @fee, @salary, @active, @notes)";

                var parameters = GetParameters();
                await DB_Server.ExecuteAsync(query, parameters);
                await LoadDriversAsync();
                ClearForm();
                ToastManager.ShowSuccess("نجاح", "تم إضافة الطيار بنجاح ✅");
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في الإضافة: " + ex.Message);
            }
        }

        // ═══════════════════════════════════════════
        //  ✏️ تعديل
        // ═══════════════════════════════════════════
        private async void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (_selectedDriverId == 0)
            {
                ToastManager.ShowWarning("تنبيه", "اختر طيار أولاً");
                return;
            }
            if (!ValidateInputs()) return;

            try
            {
                string query = @"UPDATE delivery_staff SET
                                 full_name=@name, phone=@phone, vehicle_type=@vehicle, 
                                 license_number=@license, delivery_fee=@fee, salary_type=@salary, 
                                 is_active=@active, notes=@notes
                                 WHERE delivery_id=@id";

                var parameters = GetParameters();
                var paramList = parameters.ToList();
                paramList.Add(new SqlParameter("@id", _selectedDriverId));
                await DB_Server.ExecuteAsync(query, paramList.ToArray());
                await LoadDriversAsync();
                ClearForm();
                ToastManager.ShowSuccess("نجاح", "تم التعديل بنجاح ✅");
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في التعديل: " + ex.Message);
            }
        }

        // ═══════════════════════════════════════════
        //  🗑️ حذف (Soft)
        // ═══════════════════════════════════════════
        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedDriverId == 0)
            {
                ToastManager.ShowWarning("تنبيه", "اختر طيار أولاً");
                return;
            }

            if (!frmConfirm.Show("تأكيد الحذف", "هل أنت متأكد من حذف هذا الطيار؟")) return;

            try
            {
                string query = "UPDATE delivery_staff SET is_active = 0 WHERE delivery_id = @id";
                await DB_Server.ExecuteAsync(query, new[] { new SqlParameter("@id", _selectedDriverId) });
                await LoadDriversAsync();
                ClearForm();
                ToastManager.ShowSuccess("نجاح", "تم الحذف بنجاح ✅");
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في الحذف: " + ex.Message);
            }
        }

        // ═══════════════════════════════════════════
        //  🛠️ مساعدات
        // ═══════════════════════════════════════════
        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                ToastManager.ShowWarning("تنبيه", "أدخل اسم الطيار");
                txtFullName.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                ToastManager.ShowWarning("تنبيه", "أدخل رقم التلفون");
                txtPhone.Focus();
                return false;
            }
            if (cmbVehicleType.SelectedIndex < 0)
            {
                ToastManager.ShowWarning("تنبيه", "اختر نوع المركبة");
                return false;
            }
            if (cmbSalaryType.SelectedIndex < 0)
            {
                ToastManager.ShowWarning("تنبيه", "اختر نوع المرتب");
                return false;
            }
            return true;
        }

        private SqlParameter[] GetParameters()
        {
            return new SqlParameter[]
            {
                new SqlParameter("@name", txtFullName.Text.Trim()),
                new SqlParameter("@phone", txtPhone.Text.Trim()),
                new SqlParameter("@vehicle", cmbVehicleType.SelectedItem?.ToString() ?? ""),
                new SqlParameter("@license", txtLicenseNumber.Text.Trim()),
                new SqlParameter("@fee", decimal.TryParse(txtDeliveryFee.Text, out decimal fee) ? fee : 0m),
                new SqlParameter("@salary", cmbSalaryType.SelectedItem?.ToString() ?? ""),
                new SqlParameter("@active", tglIsActive.Checked),
                new SqlParameter("@notes", txtNotes.Text.Trim()),
            };
        }

        private void ClearForm()
        {
            _selectedDriverId = 0;
            txtFullName.Text = "";
            txtPhone.Text = "";
            txtLicenseNumber.Text = "";
            txtDeliveryFee.Text = "";
            txtNotes.Text = "";
            cmbVehicleType.SelectedIndex = -1;
            cmbSalaryType.SelectedIndex = -1;
            tglIsActive.Checked = true;
        }
    }
}
