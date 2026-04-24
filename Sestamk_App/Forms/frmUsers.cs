using Guna.UI2.WinForms;
using Microsoft.Data.SqlClient;
using Sestamk.Classes;
using Sestamk.Classes.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Sestamk.Forms
{
    public partial class frmUsers : BaseForm
    {
        #region ── Constants ──────────────────────────────────────────────────

        private static readonly Color TAB_ACTIVE = Color.FromArgb(94, 148, 255);
        private static readonly Color TAB_INACTIVE = Color.FromArgb(30, 41, 59);

        #endregion

        #region ── Permission Definitions ─────────────────────────────────────

        private static readonly (string Module, string Display, string Column)[] PermDefs = new[]
        {
            ("المبيعات", "عرض المبيعات", "CanViewSales"),
            ("المبيعات", "إنشاء فاتورة", "CanCreateInvoice"),
            ("المبيعات", "مرتجعات", "CanReturnSales"),
            ("المبيعات", "تطبيق خصم", "CanApplyDiscount"),

            ("المنتجات", "عرض المنتجات", "CanViewProducts"),
            ("المنتجات", "إضافة منتج", "CanAddProduct"),
            ("المنتجات", "تعديل منتج", "CanEditProduct"),
            ("المنتجات", "حذف منتج", "CanDeleteProduct"),

            ("الأقسام", "عرض الأقسام", "CanViewCategories"),
            ("الأقسام", "إضافة قسم", "CanAddCategory"),
            ("الأقسام", "تعديل قسم", "CanEditCategory"),
            ("الأقسام", "حذف قسم", "CanDeleteCategory"),

            ("الأحجام", "عرض الأحجام", "CanViewSizes"),
            ("الأحجام", "إضافة حجم", "CanAddSize"),
            ("الأحجام", "تعديل حجم", "CanEditSize"),
            ("الأحجام", "حذف حجم", "CanDeleteSize"),

            ("الإضافات", "عرض الإضافات", "CanViewAddons"),
            ("الإضافات", "إضافة", "CanAddAddon"),
            ("الإضافات", "تعديل", "CanEditAddon"),
            ("الإضافات", "حذف", "CanDeleteAddon"),

            ("العملاء", "عرض العملاء", "CanViewCustomers"),
            ("العملاء", "إضافة عميل", "CanAddCustomer"),
            ("العملاء", "تعديل عميل", "CanEditCustomer"),
            ("العملاء", "حذف عميل", "CanDeleteCustomer"),

            ("الموردين", "عرض الموردين", "CanViewSuppliers"),
            ("الموردين", "إضافة مورد", "CanAddSupplier"),
            ("الموردين", "تعديل مورد", "CanEditSupplier"),
            ("الموردين", "حذف مورد", "CanDeleteSupplier"),

            ("المشتريات", "عرض المشتريات", "CanViewPurchases"),
            ("المشتريات", "إضافة", "CanAddPurchase"),
            ("المشتريات", "تعديل", "CanEditPurchase"),
            ("المشتريات", "حذف", "CanDeletePurchase"),

            ("التقارير", "عرض التقارير", "CanViewReports"),
            ("التقارير", "تصدير التقارير", "CanExportReports"),

            ("المستخدمين", "عرض المستخدمين", "CanViewUsers"),
            ("المستخدمين", "إضافة مستخدم", "CanAddUser"),
            ("المستخدمين", "تعديل مستخدم", "CanEditUser"),
            ("المستخدمين", "حذف مستخدم", "CanDeleteUser"),
            ("المستخدمين", "إدارة الصلاحيات", "CanManagePermissions"),

            ("الطيارين", "عرض الطيارين", "CanViewDelivery"),
            ("الطيارين", "إضافة طيار", "CanAddDelivery"),
            ("الطيارين", "تعديل طيار", "CanEditDelivery"),
            ("الطيارين", "حذف طيار", "CanDeleteDelivery"),

            ("الطاولات", "عرض الطاولات", "CanViewTables"),
            ("الطاولات", "تعديل الطاولات", "CanEditTables"),

            ("الإعدادات", "عرض الإعدادات", "CanViewSettings"),
            ("الإعدادات", "تعديل الإعدادات", "CanEditSettings"),

            ("النسخ الاحتياطي", "عرض", "CanViewBackup"),
            ("النسخ الاحتياطي", "إنشاء نسخة", "CanCreateBackup"),
            ("النسخ الاحتياطي", "استعادة نسخة", "CanRestoreBackup"),
        };

        #endregion

        #region ── Fields ─────────────────────────────────────────────────────

        private BindingList<UserModel> _usersList = new BindingList<UserModel>();
        private BindingSource _usersBinding = new BindingSource();
        private List<UserModel> _allUsers = new List<UserModel>();
        private int _selectedUserId = 0;
        private string _selectedImagePath = "";

        #endregion

        #region ── Constructor ────────────────────────────────────────────────

        public frmUsers()
        {
            InitializeComponent();
            SetupEvents();
            SetupGrid();
            _ = LoadUsersAsync();
            _ = LoadRolesComboAsync(cmbRole);
            _ = LoadRolesComboAsync(cmbPermRole);
        }

        #endregion

        #region ── Events Setup ───────────────────────────────────────────────

        private void SetupEvents()
        {
            Main_Methods.Attach(pnlTitleBar, this);
            Main_Methods.Attach(lblFormTitle, this);
            Main_Methods.Attach(pbLogo, this);
            Main_Methods.Attach(lblSep2, this);

            btn_close.Click += (s, e) => this.Close();
            btnMax.Click += (s, e) =>
            {
                this.WindowState = this.WindowState == FormWindowState.Maximized
                    ? FormWindowState.Normal : FormWindowState.Maximized;
            };
            btnMin.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

            // Tab switching
            btnTabUserData.Click += (s, e) => SwitchTab(true);
            btnTabPermissions.Click += (s, e) => SwitchTab(false);

            // CRUD
            btnSave.Click += BtnSave_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnClear.Click += (s, e) => ClearForm();

            // Search & Filter
            txtSearch.TextChanged += (s, e) => ApplyFilter();
            cmbFilterStatus.SelectedIndexChanged += (s, e) => ApplyFilter();

            // Grid click
            dgvUsers.CellClick += DgvUsers_CellClick;

            // Blocked toggle → show/hide block reason
            tglBlocked.CheckedChanged += (s, e) =>
            {
                lblBlockReason.Visible = tglBlocked.Checked;
                txtBlockReason.Visible = tglBlocked.Checked;
            };

            // Password eye toggle
            btnEyeToggle.Click += (s, e) =>
            {
                txtPassword.PasswordChar = txtPassword.PasswordChar == '●' ? '\0' : '●';
            };

            // User image click
            picUserImage.Click += (s, e) => UploadImage();
            lblUploadHint.Click += (s, e) => UploadImage();

            // Permissions
            cmbPermRole.SelectedIndexChanged += async (s, e) =>
            {
                if (cmbPermRole.SelectedValue is int roleId && roleId > 0)
                    await LoadPermissionsForRoleAsync(roleId);
            };
            btnAddRole.Click += BtnAddRole_Click;
            btnSavePermissions.Click += BtnSavePermissions_Click;
            btnSelectAll.Click += (s, e) => SetAllPermissions(true);
            btnDeselectAll.Click += (s, e) => SetAllPermissions(false);
        }

        private void SwitchTab(bool showUserData)
        {
            pnlUserDataTab.Visible = showUserData;
            pnlPermissionsTab.Visible = !showUserData;
            btnTabUserData.FillColor = showUserData ? TAB_ACTIVE : TAB_INACTIVE;
            btnTabPermissions.FillColor = !showUserData ? TAB_ACTIVE : TAB_INACTIVE;
        }

        #endregion

        #region ── Grid Setup ─────────────────────────────────────────────────

        private void SetupGrid()
        {
            // ─── Users Grid Columns ───
            dgvUsers.AutoGenerateColumns = false;
            dgvUsers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvUsers.ScrollBars = ScrollBars.Both;

            dgvUsers.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "colId", HeaderText = "#", DataPropertyName = "ID", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "colCode", HeaderText = "كود المستخدم", DataPropertyName = "User_Code", Width = 140 },
                new DataGridViewTextBoxColumn { Name = "colFullName", HeaderText = "الاسم الكامل", DataPropertyName = "full_name", Width = 250 },
                new DataGridViewTextBoxColumn { Name = "colUsername", HeaderText = "اسم المستخدم", DataPropertyName = "Username", Width = 180 },
                new DataGridViewTextBoxColumn { Name = "colRole", HeaderText = "الدور الوظيفي", DataPropertyName = "RoleName", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "colPhone", HeaderText = "رقم الهاتف", DataPropertyName = "User_Phone", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "colEmail", HeaderText = "البريد الإلكتروني", DataPropertyName = "Email", Width = 220 },
                new DataGridViewTextBoxColumn { Name = "colStatus", HeaderText = "الحالة", DataPropertyName = "StatusDisplay", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "colLastLogin", HeaderText = "تاريخ آخر دخول", DataPropertyName = "Last_Login", Width = 180 },
            });

            _usersBinding.DataSource = _usersList;
            dgvUsers.DataSource = _usersBinding;

            // ─── Permissions Grid Columns ───
            dgvPermissions.AutoGenerateColumns = false;
            dgvPermissions.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvPermissions.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "colModule", HeaderText = "القسم / الموديول", Width = 180, ReadOnly = true },
                new DataGridViewTextBoxColumn { Name = "colPermName", HeaderText = "اسم الصلاحية", Width = 250, ReadOnly = true },
                new DataGridViewCheckBoxColumn { Name = "colEnabled", HeaderText = "تفعيل", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "colDbColumn", HeaderText = "DB", Visible = false },
            });

            // Populate permissions rows
            foreach (var p in PermDefs)
            {
                dgvPermissions.Rows.Add(p.Module, p.Display, false, p.Column);
            }
        }

        #endregion

        #region ── Data Loading ───────────────────────────────────────────────

        private async System.Threading.Tasks.Task LoadUsersAsync()
        {
            try
            {
                string query = @"SELECT u.*, ISNULL(r.RoleName, N'بدون دور') AS RoleName
                                 FROM Users u
                                 LEFT JOIN Roles r ON u.Role_Id = r.RoleID
                                 WHERE ISNULL(u.IsDeleted, 0) = 0
                                 ORDER BY u.ID DESC";

                DataTable dt = await DB_Server.GetTableAsync(query);

                _usersList.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    _usersList.Add(new UserModel
                    {
                        ID = Convert.ToInt32(row["ID"]),
                        Username = row["Username"]?.ToString() ?? "",
                        Role_Id = row["Role_Id"] == DBNull.Value ? 0 : Convert.ToInt32(row["Role_Id"]),
                        RoleName = row["RoleName"]?.ToString() ?? "",
                        User_Code = row["User_Code"]?.ToString() ?? "",
                        Email = row["Email"]?.ToString() ?? "",
                        full_name = row["full_name"]?.ToString() ?? "",
                        User_Image = row["User_Image"]?.ToString() ?? "",
                        User_Phone = row["User_Phone"]?.ToString() ?? "",
                        User_Address = row["User_Address"]?.ToString() ?? "",
                        Usre_Job_Title = row["Usre_Job_Title"]?.ToString() ?? "",
                        User_department = row["User_department"]?.ToString() ?? "",
                        Is_active = row["Is_active"] != DBNull.Value && Convert.ToBoolean(row["Is_active"]),
                        Is_blocked = row["Is_blocked"] != DBNull.Value && Convert.ToBoolean(row["Is_blocked"]),
                        block_reason = row["block_reason"]?.ToString() ?? "",
                        IsDeleted = row["IsDeleted"] != DBNull.Value && Convert.ToBoolean(row["IsDeleted"]),
                        bank_id = row["bank_id"] == DBNull.Value ? 0 : Convert.ToInt32(row["bank_id"]),
                        Created_At = row["Created_At"] == DBNull.Value ? null : Convert.ToDateTime(row["Created_At"]),
                        Last_Login = row["Last_Login"] == DBNull.Value ? null : Convert.ToDateTime(row["Last_Login"]),
                    });
                }

                _allUsers = _usersList.ToList();
                _usersBinding.ResetBindings(false);
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في تحميل المستخدمين: " + ex.Message);
            }
        }

        private async System.Threading.Tasks.Task LoadRolesComboAsync(Guna2ComboBox cmb)
        {
            try
            {
                string query = "SELECT RoleID, RoleName FROM Roles WHERE IsActive = 1 ORDER BY RoleID";
                DataTable dt = await DB_Server.GetTableAsync(query);

                cmb.DataSource = null;
                cmb.Items.Clear();
                cmb.DisplayMember = "RoleName";
                cmb.ValueMember = "RoleID";
                cmb.DataSource = dt;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading roles: " + ex.Message);
                cmb.Items.Clear();
                cmb.Items.Add("(لم يتم إنشاء جدول الأدوار بعد)");
                cmb.SelectedIndex = 0;
            }
        }

        private async System.Threading.Tasks.Task LoadPermissionsForRoleAsync(int roleId)
        {
            try
            {
                string query = "SELECT * FROM Permissions_TBL WHERE RoleID = @RoleID";
                DataTable dt = await DB_Server.GetTableAsync(query,
                    new[] { new SqlParameter("@RoleID", roleId) });

                foreach (DataGridViewRow row in dgvPermissions.Rows)
                {
                    string colName = row.Cells["colDbColumn"].Value?.ToString() ?? "";
                    bool isChecked = false;

                    if (dt.Rows.Count > 0 && dt.Columns.Contains(colName))
                    {
                        object val = dt.Rows[0][colName];
                        if (val != DBNull.Value)
                            isChecked = Convert.ToBoolean(val);
                    }

                    row.Cells["colEnabled"].Value = isChecked;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading permissions: " + ex.Message);
                foreach (DataGridViewRow row in dgvPermissions.Rows)
                    row.Cells["colEnabled"].Value = false;
            }
        }

        #endregion

        #region ── Grid Click ─────────────────────────────────────────────────

        private void DgvUsers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgvUsers.Rows[e.RowIndex];
            if (row.Cells["colId"].Value == null) return;

            _selectedUserId = Convert.ToInt32(row.Cells["colId"].Value);

            var user = _allUsers.FirstOrDefault(u => u.ID == _selectedUserId);
            if (user == null) return;

            txtUserCode.Text = user.User_Code;
            txtFullName.Text = user.full_name;
            txtUsername.Text = user.Username;
            txtPassword.Text = "";
            txtEmail.Text = user.Email;
            txtPhone.Text = user.User_Phone;
            txtAddress.Text = user.User_Address;
            txtJobTitle.Text = user.Usre_Job_Title;
            txtDepartment.Text = user.User_department;

            if (cmbRole.DataSource is DataTable dtRoles)
            {
                for (int i = 0; i < dtRoles.Rows.Count; i++)
                {
                    if (Convert.ToInt32(dtRoles.Rows[i]["RoleID"]) == user.Role_Id)
                    {
                        cmbRole.SelectedIndex = i;
                        break;
                    }
                }
            }

            tglActive.Checked = user.Is_active;
            tglBlocked.Checked = user.Is_blocked;
            txtBlockReason.Text = user.block_reason;
            lblBlockReason.Visible = user.Is_blocked;
            txtBlockReason.Visible = user.Is_blocked;

            try
            {
                if (!string.IsNullOrEmpty(user.User_Image) && File.Exists(user.User_Image))
                    picUserImage.Image = Image.FromFile(user.User_Image);
                else
                    picUserImage.Image = Properties.Resources.user__2_;
            }
            catch { picUserImage.Image = Properties.Resources.user__2_; }

            _selectedImagePath = user.User_Image;
        }

        #endregion

        #region ── CRUD Operations ────────────────────────────────────────────

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs(true)) return;

            try
            {
                string hashedPassword = PasswordHelper.HashPassword(txtPassword.Text.Trim());
                int roleId = GetSelectedRoleId();

                string query = @"INSERT INTO Users 
                    (Username, Password, Role_Id, User_Code, Email, full_name, 
                     User_Image, User_Phone, User_Address, Usre_Job_Title, User_department,
                     Is_active, Is_blocked, block_reason, IsDeleted, Created_At, User_Stats)
                    VALUES 
                    (@Username, @Password, @RoleId, @UserCode, @Email, @FullName,
                     @UserImage, @Phone, @Address, @JobTitle, @Department,
                     @IsActive, @IsBlocked, @BlockReason, 0, GETDATE(), 1)";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@Username", txtUsername.Text.Trim()),
                    new SqlParameter("@Password", hashedPassword),
                    new SqlParameter("@RoleId", roleId),
                    new SqlParameter("@UserCode", txtUserCode.Text.Trim()),
                    new SqlParameter("@Email", txtEmail.Text.Trim()),
                    new SqlParameter("@FullName", txtFullName.Text.Trim()),
                    new SqlParameter("@UserImage", _selectedImagePath ?? ""),
                    new SqlParameter("@Phone", txtPhone.Text.Trim()),
                    new SqlParameter("@Address", txtAddress.Text.Trim()),
                    new SqlParameter("@JobTitle", txtJobTitle.Text.Trim()),
                    new SqlParameter("@Department", txtDepartment.Text.Trim()),
                    new SqlParameter("@IsActive", tglActive.Checked),
                    new SqlParameter("@IsBlocked", tglBlocked.Checked),
                    new SqlParameter("@BlockReason", tglBlocked.Checked ? txtBlockReason.Text.Trim() : ""),
                };

                await DB_Server.ExecuteAsync(query, parameters);
                await LoadUsersAsync();
                ClearForm();
                ToastManager.ShowSuccess("نجاح", "تم إضافة المستخدم بنجاح ✅");
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في الإضافة: " + ex.Message);
            }
        }

        private async void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (_selectedUserId == 0)
            {
                ToastManager.ShowWarning("تنبيه", "اختر مستخدم أولاً");
                return;
            }
            if (!ValidateInputs(false)) return;

            try
            {
                int roleId = GetSelectedRoleId();

                string passwordPart = "";
                var paramList = new List<SqlParameter>
                {
                    new SqlParameter("@Username", txtUsername.Text.Trim()),
                    new SqlParameter("@RoleId", roleId),
                    new SqlParameter("@UserCode", txtUserCode.Text.Trim()),
                    new SqlParameter("@Email", txtEmail.Text.Trim()),
                    new SqlParameter("@FullName", txtFullName.Text.Trim()),
                    new SqlParameter("@UserImage", _selectedImagePath ?? ""),
                    new SqlParameter("@Phone", txtPhone.Text.Trim()),
                    new SqlParameter("@Address", txtAddress.Text.Trim()),
                    new SqlParameter("@JobTitle", txtJobTitle.Text.Trim()),
                    new SqlParameter("@Department", txtDepartment.Text.Trim()),
                    new SqlParameter("@IsActive", tglActive.Checked),
                    new SqlParameter("@IsBlocked", tglBlocked.Checked),
                    new SqlParameter("@BlockReason", tglBlocked.Checked ? txtBlockReason.Text.Trim() : ""),
                    new SqlParameter("@id", _selectedUserId),
                };

                if (!string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    passwordPart = ", Password = @Password";
                    paramList.Add(new SqlParameter("@Password", PasswordHelper.HashPassword(txtPassword.Text.Trim())));
                }

                string query = $@"UPDATE Users SET
                    Username = @Username, Role_Id = @RoleId, User_Code = @UserCode,
                    Email = @Email, full_name = @FullName, User_Image = @UserImage,
                    User_Phone = @Phone, User_Address = @Address, 
                    Usre_Job_Title = @JobTitle, User_department = @Department,
                    Is_active = @IsActive, Is_blocked = @IsBlocked, block_reason = @BlockReason
                    {passwordPart}
                    WHERE ID = @id";

                await DB_Server.ExecuteAsync(query, paramList.ToArray());
                await LoadUsersAsync();
                ClearForm();
                ToastManager.ShowSuccess("نجاح", "تم تعديل بيانات المستخدم بنجاح ✅");
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في التعديل: " + ex.Message);
            }
        }

        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedUserId == 0)
            {
                ToastManager.ShowWarning("تنبيه", "اختر مستخدم أولاً");
                return;
            }

            if (!frmConfirm.Show("تأكيد الحذف", "هل أنت متأكد من حذف هذا المستخدم؟\nسيتم إلغاء تفعيله (حذف ناعم)."))
                return;

            try
            {
                string query = "UPDATE Users SET IsDeleted = 1, Is_active = 0 WHERE ID = @id";
                await DB_Server.ExecuteAsync(query, new[] { new SqlParameter("@id", _selectedUserId) });
                await LoadUsersAsync();
                ClearForm();
                ToastManager.ShowSuccess("نجاح", "تم حذف المستخدم بنجاح ✅");
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في الحذف: " + ex.Message);
            }
        }

        #endregion

        #region ── Permissions Operations ──────────────────────────────────────

        private async void BtnSavePermissions_Click(object sender, EventArgs e)
        {
            if (!(cmbPermRole.SelectedValue is int roleId) || roleId <= 0)
            {
                ToastManager.ShowWarning("تنبيه", "اختر دور أولاً");
                return;
            }

            try
            {
                var setClauses = new List<string>();
                var parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@RoleID", roleId));

                foreach (DataGridViewRow row in dgvPermissions.Rows)
                {
                    string colName = row.Cells["colDbColumn"].Value?.ToString() ?? "";
                    bool isChecked = row.Cells["colEnabled"].Value != null &&
                                     Convert.ToBoolean(row.Cells["colEnabled"].Value);

                    setClauses.Add($"{colName} = @{colName}");
                    parameters.Add(new SqlParameter($"@{colName}", isChecked));
                }

                object exists = await DB_Server.ScalarAsync(
                    "SELECT COUNT(*) FROM Permissions_TBL WHERE RoleID = @RoleID",
                    new[] { new SqlParameter("@RoleID", roleId) });

                if (Convert.ToInt32(exists) > 0)
                {
                    string query = $"UPDATE Permissions_TBL SET {string.Join(", ", setClauses)} WHERE RoleID = @RoleID";
                    await DB_Server.ExecuteAsync(query, parameters.ToArray());
                }
                else
                {
                    var colNames = new List<string> { "RoleID" };
                    var paramNames = new List<string> { "@RoleID" };
                    foreach (DataGridViewRow row in dgvPermissions.Rows)
                    {
                        string colName = row.Cells["colDbColumn"].Value?.ToString() ?? "";
                        colNames.Add(colName);
                        paramNames.Add($"@{colName}");
                    }
                    string query = $"INSERT INTO Permissions_TBL ({string.Join(", ", colNames)}) VALUES ({string.Join(", ", paramNames)})";
                    await DB_Server.ExecuteAsync(query, parameters.ToArray());
                }

                ToastManager.ShowSuccess("نجاح", "تم حفظ الصلاحيات بنجاح ✅");
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في حفظ الصلاحيات: " + ex.Message);
            }
        }

        private async void BtnAddRole_Click(object sender, EventArgs e)
        {
            string roleName = txtNewRoleName.Text.Trim();
            if (string.IsNullOrWhiteSpace(roleName))
            {
                ToastManager.ShowWarning("تنبيه", "أدخل اسم الدور الجديد");
                return;
            }

            try
            {
                object existing = await DB_Server.ScalarAsync(
                    "SELECT COUNT(*) FROM Roles WHERE RoleName = @Name",
                    new[] { new SqlParameter("@Name", roleName) });

                if (Convert.ToInt32(existing) > 0)
                {
                    ToastManager.ShowWarning("تنبيه", "هذا الدور موجود بالفعل");
                    return;
                }

                int newRoleId = await DB_Server.ExecuteWithIdentityAsync(
                    "INSERT INTO Roles (RoleName, IsActive) VALUES (@Name, 1); SELECT SCOPE_IDENTITY()",
                    new[] { new SqlParameter("@Name", roleName) });

                await DB_Server.ExecuteAsync(
                    "INSERT INTO Permissions_TBL (RoleID) VALUES (@RoleID)",
                    new[] { new SqlParameter("@RoleID", newRoleId) });

                txtNewRoleName.Text = "";
                await LoadRolesComboAsync(cmbPermRole);
                await LoadRolesComboAsync(cmbRole);
                ToastManager.ShowSuccess("نجاح", $"تم إضافة الدور \"{roleName}\" بنجاح ✅");
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في إضافة الدور: " + ex.Message);
            }
        }

        private void SetAllPermissions(bool value)
        {
            foreach (DataGridViewRow row in dgvPermissions.Rows)
                row.Cells["colEnabled"].Value = value;
        }

        #endregion

        #region ── Search & Filter ────────────────────────────────────────────

        private void ApplyFilter()
        {
            string keyword = (txtSearch.Text ?? "").Trim().ToLower();
            string status = cmbFilterStatus.SelectedItem?.ToString() ?? "الكل";

            IEnumerable<UserModel> result = _allUsers;

            if (status == "نشط") result = result.Where(u => u.Is_active && !u.Is_blocked);
            else if (status == "غير نشط") result = result.Where(u => !u.Is_active);
            else if (status == "محظور") result = result.Where(u => u.Is_blocked);

            if (!string.IsNullOrEmpty(keyword))
            {
                result = result.Where(u =>
                    (u.full_name ?? "").ToLower().Contains(keyword) ||
                    (u.Username ?? "").ToLower().Contains(keyword) ||
                    (u.User_Code ?? "").ToLower().Contains(keyword) ||
                    (u.Email ?? "").ToLower().Contains(keyword) ||
                    (u.User_Phone ?? "").Contains(keyword));
            }

            _usersList.Clear();
            foreach (var u in result) _usersList.Add(u);
            _usersBinding.ResetBindings(false);
        }

        #endregion

        #region ── Helpers ────────────────────────────────────────────────────

        private void UploadImage()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "صور|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                ofd.Title = "اختر صورة المستخدم";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string destFolder = Path.Combine(Application.StartupPath, "Imges", "Users");
                        Directory.CreateDirectory(destFolder);
                        string ext = Path.GetExtension(ofd.FileName);
                        string fileName = $"user_{DateTime.Now:yyyyMMddHHmmss}{ext}";
                        string destPath = Path.Combine(destFolder, fileName);
                        File.Copy(ofd.FileName, destPath, true);

                        picUserImage.Image = Image.FromFile(destPath);
                        _selectedImagePath = destPath;
                    }
                    catch (Exception ex)
                    {
                        ToastManager.ShowError("خطأ", "خطأ في رفع الصورة: " + ex.Message);
                    }
                }
            }
        }

        private bool ValidateInputs(bool isNew)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                ToastManager.ShowWarning("تنبيه", "أدخل الاسم كامل");
                txtFullName.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                ToastManager.ShowWarning("تنبيه", "أدخل اسم المستخدم");
                txtUsername.Focus();
                return false;
            }
            if (isNew && string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                ToastManager.ShowWarning("تنبيه", "أدخل كلمة المرور");
                txtPassword.Focus();
                return false;
            }
            if (tglBlocked.Checked && string.IsNullOrWhiteSpace(txtBlockReason.Text))
            {
                ToastManager.ShowWarning("تنبيه", "أدخل سبب الحظر");
                txtBlockReason.Focus();
                return false;
            }
            return true;
        }

        private int GetSelectedRoleId()
        {
            if (cmbRole.SelectedValue is int roleId)
                return roleId;
            return 0;
        }

        private void ClearForm()
        {
            _selectedUserId = 0;
            _selectedImagePath = "";
            txtUserCode.Text = "";
            txtFullName.Text = "";
            txtUsername.Text = "";
            txtPassword.Text = "";
            txtEmail.Text = "";
            txtPhone.Text = "";
            txtAddress.Text = "";
            txtJobTitle.Text = "";
            txtDepartment.Text = "";
            txtBlockReason.Text = "";
            tglActive.Checked = true;
            tglBlocked.Checked = false;
            lblBlockReason.Visible = false;
            txtBlockReason.Visible = false;
            cmbRole.SelectedIndex = cmbRole.Items.Count > 0 ? 0 : -1;
            picUserImage.Image = Properties.Resources.user__2_;
        }

        #endregion

    }
}
