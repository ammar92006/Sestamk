using Guna.UI2.WinForms;
using Microsoft.Data.SqlClient;
using Sestamk.Classes;
using Sestamk.Classes.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows.Forms;

namespace Sestamk.Forms
{
    public partial class frmCustomers : BaseForm
    {
        protected override Size DesignClientSize => new Size(1650, 900);

        void UpdateLanguageLabel()
        {
            var culture = InputLanguage.CurrentInputLanguage.Culture;
            lblLang.Text = culture.TwoLetterISOLanguageName.ToUpper();
        }

        private BindingList<Customer> customerList = new BindingList<Customer>();
        private BindingSource customerBinding = new BindingSource();
        private List<Customer> allCustomers = new List<Customer>();
        private bool _disableAutoUpdate = false;

        private System.Timers.Timer searchTimer;
        Color p_color = ColorTranslator.FromHtml("#0D8AFA");
        Color act_color = Color.FromArgb(34, 197, 94);
        Color inact_color = Color.DarkRed;
        Color btn_actv_ori_fill = Color.FromArgb(32, 41, 60);
        Color btn_actv_ori_fore = Color.FromArgb(77, 89, 111);
        Color btn_inactv_ori_fill = Color.FromArgb(32, 41, 60);
        Color btn_inactv_ori_fore = Color.FromArgb(77, 89, 111);

        bool cust_status;

        public frmCustomers()
        {
            InitializeComponent();
            System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
            t.Interval = 200;
            t.Tick += (s, ev) => UpdateAllIndicators();
            t.Start();
            UpdateLanguageLabel();
            setupformUI();
            LoadCustomersAsync();
            searchTimer = new System.Timers.Timer(300);
            searchTimer.AutoReset = false;
            searchTimer.Elapsed += SearchTimer_Elapsed;
            Main_Methods.FillComboBoxWithGridHeaders(dgvCustomers, cmbSearchField);
            InitializeCustomerGrid();

            // ── تحديث بيانات المستخدم من الجلسة ──
            UserSession.UpdateUserDisplay(guna2Button4, guna2Button5);


            Main_Methods.Attach(panel1, this);
            Main_Methods.Attach(label1, this);
            Main_Methods.Attach(pbLogo, this);
            Main_Methods.Attach(guna2CirclePictureBox1, this);
            Main_Methods.Attach(guna2Button4, this);
            Main_Methods.Attach(guna2Button5, this);
            Main_Methods.Attach(label3, this);
            Main_Methods.Attach(label2, this);
        }

        private void InitializeCustomerGrid()
        {
            dgvCustomers.AutoGenerateColumns = false;

            customerBinding.DataSource = customerList;
            dgvCustomers.DataSource = customerBinding;

            // ✅ تم حذف customerList.ListChanged من هنا
            // لأننا انتقلنا للـ Manual Update في btnUpdate_Click
        }

        // ✅ الدالة دي اتعدلت - بقت بس للـ UI بدون أي DB call
        // السبب: الـ ListChanged كانت بتتفعل تلقائياً مع كل تغيير خاصية
        // وده كان بيخلي الـ btnUpdate_Click يعمل DB call تاني زيادة = Double Call
        private void CustomerList_ListChanged(object sender, ListChangedEventArgs e)
        {
            // ✅ فاضي عن قصد - التحديث في قاعدة البيانات بيحصل فقط في btnUpdate_Click
            // الـ BindingList لسه شغال وبيحدث الـ DataGridView تلقائياً في الـ UI
        }

        // ✅ الدالة دي اتحدثت لتشمل كل الحقول بدون استثناء
        private async Task UpdateCustomerInDbAsync(Customer c)
        {
            try
            {
                using (SqlConnection conn = DB_Server.GetConnection())
                using (SqlCommand cmd = new SqlCommand(@"
                    UPDATE dbo.Customers 
                    SET 
                        CustomerCode     = @code,
                        CustomerName     = @name,
                        Phone1           = @phone1,
                        Phone2           = @phone2,
                        Email            = @email,
                        Address          = @address,
                        CurrentBalance   = @balance,
                        Notes            = @notes,
                        CreditLimit      = @creditLimit,
                        DiscountPercent  = @discount,
                        IsActive         = @isActive,
                        StopReason       = @stopReason,
                        Rating           = @rating,
                        AllowCredit      = @allowCredit
                    WHERE CustomerID = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", c.CustomerID);
                    cmd.Parameters.AddWithValue("@code", c.CustomerCode ?? "");
                    cmd.Parameters.AddWithValue("@name", c.CustomerName ?? "");
                    cmd.Parameters.AddWithValue("@phone1", c.Phone1 ?? "");
                    cmd.Parameters.AddWithValue("@phone2", c.Phone2 ?? "");
                    cmd.Parameters.AddWithValue("@email", c.Email ?? "");
                    cmd.Parameters.AddWithValue("@address", c.Address ?? "");
                    cmd.Parameters.AddWithValue("@balance", c.CurrentBalance);
                    cmd.Parameters.AddWithValue("@notes", c.Notes ?? "");
                    cmd.Parameters.AddWithValue("@creditLimit", c.CreditLimit);
                    cmd.Parameters.AddWithValue("@discount", c.DiscountPercent);
                    cmd.Parameters.AddWithValue("@isActive", c.IsActive);
                    cmd.Parameters.AddWithValue("@stopReason", c.StopReason ?? "");
                    cmd.Parameters.AddWithValue("@rating", c.Rating);
                    cmd.Parameters.AddWithValue("@allowCredit", c.AllowCredit);

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في تحديث البيانات: " + ex.Message);
            }
        }

        

        private void UpdateAllIndicators()
        {
            lblNum.ForeColor = Control.IsKeyLocked(Keys.NumLock) ? Color.FromArgb(13, 101, 190) : Color.FromArgb(169, 169, 169);
            lblCaps.ForeColor = Control.IsKeyLocked(Keys.CapsLock) ? Color.FromArgb(13, 101, 190) : Color.FromArgb(169, 169, 169);
            lblIns.ForeColor = Control.IsKeyLocked(Keys.Insert) ? Color.FromArgb(13, 101, 190) : Color.FromArgb(169, 169, 169);
        }

        private void ClearCustomerForm()
        {
            txtCustomerCode.Clear();
            txtCustomerName.Clear();
            txtPhone1.Clear();
            txtPhone2.Clear();
            txtEmail.Clear();
            txtAddress.Clear();
            txtBalance.Clear();
            txtNotes.Clear();
            txtCreditLimit.Clear();
            txtDiscount.Clear();
            txtStopReason.Clear();
            lblCreatedAt.Text = "";
            lblLastActivity.Text = "";
            lblAddedBy.Text = "";
            dgvCustomers.ClearSelection();
            cmbRating.SelectedIndex = -1;

            chkAllowCredit.Checked = false;
            cust_status = false;

            btnActive.FillColor = btn_actv_ori_fill;
            btnActive.ForeColor = btn_actv_ori_fore;
            btnInactive.FillColor = btn_actv_ori_fill;
            btnInactive.ForeColor = btn_actv_ori_fore;
        }

        private async Task LoadCustomersAsync(string filter = "", string field = "")
        {
            try
            {
                ClearCustomerForm();
                string query = @"SELECT CustomerID, CustomerCode, CustomerName, Phone1, Phone2, 
                        Email, Address, CurrentBalance, CreditLimit, 
                        DiscountPercent, IsActive, StopReason, CreatedDate, 
                        LastTransactionDate, CreatedByUserID, AllowCredit, Rating, Notes
                        FROM dbo.Customers 
                        WHERE IsDeleted = 0";

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    string columnName = GetDatabaseColumnName(field);
                    if (!string.IsNullOrWhiteSpace(columnName))
                        query += $" AND {columnName} LIKE @filter";
                }

                DataTable dt = new DataTable();

                using (SqlConnection conn = DB_Server.GetConnection())
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (query.Contains("@filter"))
                        cmd.Parameters.Add("@filter", SqlDbType.NVarChar).Value = "%" + filter + "%";

                    await conn.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        dt.Load(reader);
                    }
                }

                int? selectedId = null;
                if (dgvCustomers.CurrentRow != null && dgvCustomers.CurrentRow.Cells["CustomerID"].Value != null)
                {
                    selectedId = Convert.ToInt32(dgvCustomers.CurrentRow.Cells["CustomerID"].Value);
                }

                foreach (DataRow row in dt.Rows)
                {
                    int id = Convert.ToInt32(row["CustomerID"]);

                    var existing = customerList.FirstOrDefault(x => x.CustomerID == id);

                    string GetString(string col) => row[col] == DBNull.Value ? "" : row[col].ToString();
                    int GetInt(string col) => row[col] == DBNull.Value ? 0 : Convert.ToInt32(row[col]);
                    bool GetBool(string col) => row[col] != DBNull.Value && Convert.ToBoolean(row[col]);
                    decimal GetDecimal(string col) => row[col] == DBNull.Value ? 0m : Convert.ToDecimal(row[col]);
                    float GetFloat(string col) => row[col] == DBNull.Value ? 0f : Convert.ToSingle(row[col]);

                    if (existing != null)
                    {
                        existing.CustomerCode = GetString("CustomerCode");
                        existing.CustomerName = GetString("CustomerName");
                        existing.Phone1 = GetString("Phone1");
                        existing.Phone2 = GetString("Phone2");
                        existing.Email = GetString("Email");
                        existing.Address = GetString("Address");
                        existing.CurrentBalance = GetDecimal("CurrentBalance");
                        existing.CreditLimit = GetDecimal("CreditLimit");
                        existing.DiscountPercent = GetFloat("DiscountPercent");
                        existing.IsActive = GetBool("IsActive");
                        existing.StopReason = GetString("StopReason");
                        existing.AllowCredit = GetBool("AllowCredit");
                        existing.Rating = GetInt("Rating");
                        existing.Notes = GetString("Notes");
                        existing.CreatedDate = GetString("CreatedDate");
                        existing.CreatedByUserID = GetInt("CreatedByUserID");
                        existing.LastTransactionDate = GetString("LastTransactionDate");
                    }
                    else
                    {
                        customerList.Add(new Customer
                        {
                            CustomerID = id,
                            CustomerCode = GetString("CustomerCode"),
                            CustomerName = GetString("CustomerName"),
                            Phone1 = GetString("Phone1"),
                            Phone2 = GetString("Phone2"),
                            Email = GetString("Email"),
                            Address = GetString("Address"),
                            CurrentBalance = GetDecimal("CurrentBalance"),
                            CreditLimit = GetDecimal("CreditLimit"),
                            DiscountPercent = GetFloat("DiscountPercent"),
                            IsActive = GetBool("IsActive"),
                            StopReason = GetString("StopReason"),
                            AllowCredit = GetBool("AllowCredit"),
                            Rating = GetInt("Rating"),
                            Notes = GetString("Notes"),
                            CreatedDate = GetString("CreatedDate"),
                            CreatedByUserID = GetInt("CreatedByUserID"),
                            LastTransactionDate = GetString("LastTransactionDate")
                        });
                    }
                }

                var idsFromDb = dt.AsEnumerable()
                                  .Select(r => Convert.ToInt32(r["CustomerID"]))
                                  .ToHashSet();

                for (int i = customerList.Count - 1; i >= 0; i--)
                {
                    if (!idsFromDb.Contains(customerList[i].CustomerID))
                        customerList.RemoveAt(i);
                }

                allCustomers = customerList.ToList();

                if (selectedId.HasValue)
                {
                    foreach (DataGridViewRow row in dgvCustomers.Rows)
                    {
                        if (row.Cells["CustomerID"].Value != null &&
                            Convert.ToInt32(row.Cells["CustomerID"].Value) == selectedId.Value)
                        {
                            row.Selected = true;
                            dgvCustomers.CurrentCell = row.Cells[0];
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في التحميل: " + ex.Message);
            }
        }

        private void BindGridColumns()
        {
            var mappings = new Dictionary<string, string>
            {
                { "colCustomerID", "CustomerID" },
                { "colCustomerCode", "CustomerCode" },
                { "colCustomerName", "CustomerName" },
                { "colPhonePrimary", "Phone1" },
                { "colPhoneSecondary", "Phone2" },
                { "colEmail", "Email" },
                { "colAddress", "Address" },
                { "colCurrentBalance", "CurrentBalance" },
                { "colNotes", "Notes" },
                { "colCreditLimit", "CreditLimit" },
                { "colDefaultDiscountPercent", "DiscountPercent" },
                { "colCustomerStatus", "IsActive" },
                { "colStopReason", "StopReason" },
                { "colCreatedAt", "CreatedDate" },
                { "colLastActivityAt", "LastTransactionDate" },
                { "colAddedByUser", "CreatedByUserID" },
                { "colAllowCreditSales", "AllowCredit" },
                { "colCustomerRating", "Rating" }
            };

            foreach (var map in mappings)
            {
                dgvCustomers.Columns[map.Key].DataPropertyName = map.Value;
            }
        }

        private async Task<bool> IsCustomerCodeExists(string code, int currentId = 0)
        {
            string query = "SELECT COUNT(*) FROM Customers WHERE CustomerCode = @Code AND CustomerID <> @ID AND IsDeleted = 0";
            using (SqlConnection conn = DB_Server.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Code", code);
                cmd.Parameters.AddWithValue("@ID", currentId);
                await conn.OpenAsync();
                int count = (int)await cmd.ExecuteScalarAsync();
                return count > 0;
            }
        }

        private void SetParameters(SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@Name", txtCustomerName.Text);
            cmd.Parameters.AddWithValue("@P1", txtPhone1.Text);
            cmd.Parameters.AddWithValue("@P2", txtPhone2.Text);
            cmd.Parameters.AddWithValue("@Email", txtEmail.Text);
            cmd.Parameters.AddWithValue("@Addr", txtAddress.Text);
            cmd.Parameters.AddWithValue("@Balance", decimal.TryParse(txtBalance.Text, out decimal b) ? b : 0);
            cmd.Parameters.AddWithValue("@Notes", txtNotes.Text);
            cmd.Parameters.AddWithValue("@Limit", decimal.TryParse(txtCreditLimit.Text, out decimal l) ? l : 0);
            cmd.Parameters.AddWithValue("@Disc", double.TryParse(txtDiscount.Text, out double d) ? d : 0);
            cmd.Parameters.AddWithValue("@Active", cust_status);
            cmd.Parameters.AddWithValue("@Stop", txtStopReason.Text);
            cmd.Parameters.AddWithValue("@Rating", cmbRating.SelectedIndex >= 0 ? cmbRating.SelectedIndex + 1 : 0);
            cmd.Parameters.AddWithValue("@Allow", chkAllowCredit.Checked);
        }

        private string GetDatabaseColumnName(string field)
        {
            if (string.IsNullOrWhiteSpace(field)) return "";

            field = field.Trim();

            switch (field)
            {
                case "كود العميل": return "CustomerCode";
                case "اسم العميل": return "CustomerName";
                case "رقم الهاتف الأساسي": return "Phone1";
                case "رقم هاتف إضافي": return "Phone2";
                case "البريد الإلكتروني": return "Email";
                case "العنوان": return "Address";
                case "الرصيد الحالي": return "CurrentBalance";
                case "ملاحظات": return "Notes";
                case "حد الائتمان": return "CreditLimit";
                case "نسبة الخصم": return "DiscountPercent";
                case "حالة العميل": return "CustomerStatus";
                case "سبب الإيقاف": return "StopReason";
                case "تاريخ الإنشاء": return "CreatedAt";
                case "آخر تعامل": return "LastActivityAt";
                case "أضيف بواسطة": return "AddedByUser";
                case "إمكانية البيع الآجل": return "AllowCreditSales";
                case "تقييم العميل": return "CustomerRating";
                default: return "";
            }
        }

        private void setupformUI()
        {
            cmbRating.Items.Clear();
            cmbRating.Items.Add("★ - ضعيف");
            cmbRating.Items.Add("★★ - متوسط");
            cmbRating.Items.Add("★★★ - كويس");
            cmbRating.Items.Add("★★★★ - جيد جدًا");
            cmbRating.Items.Add("★★★★★ - ممتاز");
            BindGridColumns();
            cmbRating.SelectedIndex = 0;

            pnlContent.HorizontalScroll.Enabled = false;
            pnlContent.HorizontalScroll.Visible = false;
            pnlContent.VerticalScroll.Enabled = false;
            pnlContent.VerticalScroll.Visible = false;

            pnlContent.MouseWheel += (s, e) => HideDefaultScrollBars();
            pnlContent.Scroll += (s, e) => HideDefaultScrollBars();
        }

        private void HideDefaultScrollBars()
        {
            pnlContent.HorizontalScroll.Enabled = false;
            pnlContent.HorizontalScroll.Visible = false;
            pnlContent.VerticalScroll.Enabled = false;
            pnlContent.VerticalScroll.Visible = false;
        }

        private void txtEmail_TextChanged(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            if (string.IsNullOrEmpty(email))
                txtEmail.IconLeft = null;
            else if (IsValidEmail(email))
                txtEmail.IconLeft = Properties.Resources.tick_mark_2;
            else
                txtEmail.IconLeft = Properties.Resources.cross_2;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase);
            }
            catch { return false; }
        }

        private void btn_close_Click(object sender, EventArgs e) => this.Close();

        private void btnMax_Click(object sender, EventArgs e)
        {
            this.WindowState = this.WindowState == FormWindowState.Maximized
                ? FormWindowState.Normal
                : FormWindowState.Maximized;
        }

        private void btnMin_Click(object sender, EventArgs e) => this.WindowState = FormWindowState.Minimized;

        private void SetButtonState(bool isActive)
        {
            if (isActive)
            {
                btnActive.FillColor = act_color;
                btnInactive.FillColor = Color.FromArgb(77, 89, 111);
                btnActive.ForeColor = Color.WhiteSmoke;
                btnInactive.ForeColor = Color.WhiteSmoke;
                lbl_Cust_Reason.Visible = false;
                txtStopReason.Visible = false;
            }
            else
            {
                btnActive.FillColor = Color.FromArgb(77, 89, 111);
                btnInactive.FillColor = inact_color;
                btnActive.ForeColor = Color.WhiteSmoke;
                btnInactive.ForeColor = Color.WhiteSmoke;
                lbl_Cust_Reason.Visible = true;
                txtStopReason.Visible = true;
                lbl_Cust_Reason.Height = 30;
                txtStopReason.Height = 50;
            }
        }

        private void btnInactive_Click(object sender, EventArgs e) { SetButtonState(false); cust_status = false; }
        private void btnActive_Click(object sender, EventArgs e) { SetButtonState(true); cust_status = true; }

        private void dgvCustomers_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            string[] phoneColumns = { "colPhonePrimary", "colPhoneSecondary" };

            if (phoneColumns.Contains(dgvCustomers.Columns[e.ColumnIndex].Name) && e.Value != null)
            {
                string raw = new string(e.Value.ToString().Where(char.IsDigit).ToArray());

                if (!string.IsNullOrEmpty(raw))
                {
                    string formatted = raw.Length == 11
                        ? $"{raw.Substring(7, 4)} {raw.Substring(4, 3)} {raw.Substring(0, 4)}"
                        : raw;
                    e.Value = formatted;
                }
                else
                {
                    e.Value = "غير محدد";
                }
                e.FormattingApplied = true;
            }

            if (dgvCustomers.Columns[e.ColumnIndex].Name == "colCurrentBalance" && e.Value != null)
            {
                if (decimal.TryParse(e.Value.ToString(), out decimal balance))
                {
                    e.Value = balance.ToString("C", CultureInfo.CreateSpecificCulture("ar-EG"));
                    e.FormattingApplied = true;
                }
            }

            if (dgvCustomers.Columns[e.ColumnIndex].Name == "colCreditLimit" && e.Value != null)
            {
                if (decimal.TryParse(e.Value.ToString(), out decimal limit))
                {
                    e.Value = limit.ToString("C", CultureInfo.CreateSpecificCulture("ar-EG"));
                    e.FormattingApplied = true;
                }
            }

            if (dgvCustomers.Columns[e.ColumnIndex].Name == "colAllowCreditSales" && e.Value != null)
            {
                bool allowCredit = e.Value is bool b ? b : bool.TryParse(e.Value.ToString(), out bool r) && r;
                e.Value = allowCredit ? "مسموح" : "غير مسموح";
                e.FormattingApplied = true;
            }

            if (dgvCustomers.Columns[e.ColumnIndex].Name == "colCustomerStatus" && e.Value != null)
            {
                bool isActive = e.Value is bool b ? b : bool.TryParse(e.Value.ToString(), out bool r) && r;
                e.Value = isActive ? "نشط" : "موقف";
                e.FormattingApplied = true;
            }
        }

        // ✅ التعديل الرئيسي هنا - بقى يعمل DB call واحدة فقط بكل الحقول
        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvCustomers.CurrentRow == null) return;
            if (dgvCustomers.CurrentRow.Cells["colCustomerID"].Value == null) return;

            // ===== 1. التحقق من المدخلات =====
            if (!ValidateCustomerInputs(out string msg))
            {
                ToastManager.ShowWarning("تنبيه", msg);
                return;
            }

            int customerId = Convert.ToInt32(dgvCustomers.CurrentRow.Cells["colCustomerID"].Value);

            // ===== 2. التحقق من تكرار الكود =====
            if (await IsCustomerCodeExists(txtCustomerCode.Text.Trim(), customerId))
            {
                ToastManager.ShowWarning("تنبيه", "كود العميل مكرر!");
                return;
            }

            // ===== 3. جلب الكائن من القائمة =====
            var customer = customerList.FirstOrDefault(c => c.CustomerID == customerId);
            if (customer == null) return;

            // ===== 4. تحديث الكائن في الميموري =====
            customer.CustomerCode = txtCustomerCode.Text.Trim();
            customer.CustomerName = txtCustomerName.Text.Trim();
            customer.Phone1 = txtPhone1.Text.Trim();
            customer.Phone2 = txtPhone2.Text.Trim();
            customer.Email = txtEmail.Text.Trim();
            customer.Address = txtAddress.Text.Trim();
            customer.Notes = txtNotes.Text.Trim();
            customer.StopReason = txtStopReason.Text.Trim();
            customer.AllowCredit = chkAllowCredit.Checked;
            customer.IsActive = cust_status;
            customer.CurrentBalance = decimal.TryParse(txtBalance.Text, out decimal bal) ? bal : 0m;
            customer.CreditLimit = decimal.TryParse(txtCreditLimit.Text, out decimal lim) ? lim : 0m;
            customer.DiscountPercent = float.TryParse(txtDiscount.Text, out float disc) ? disc : 0f;
            customer.Rating = cmbRating.SelectedIndex >= 0 ? cmbRating.SelectedIndex + 1 : 0;

            // ===== 5. DB call واحدة فقط =====
            await UpdateCustomerInDbAsync(customer);

            // ===== 6. تحديث allCustomers بعد التعديل =====
            allCustomers = customerList.ToList();

            ToastManager.ShowSuccess("تم بنجاح", $"تم تعديل بيانات ({customer.CustomerName}) ✅");
            ClearCustomerForm();
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateCustomerInputs(out string msg))
            {
                ToastManager.ShowWarning("تنبيه", msg);
                return;
            }

            string customerCode = txtCustomerCode.Text.Trim();

            if (await IsCustomerCodeExists(customerCode))
            {
                ToastManager.ShowWarning("تنبيه", "كود العميل هذا مسجل مسبقاً، يرجى اختيار كود آخر.");
                return;
            }

            try
            {
                int newCustomerId = await InsertCustomerAsync();

                Customer newCustomer = new Customer
                {
                    CustomerID = newCustomerId,
                    CustomerCode = customerCode,
                    CustomerName = txtCustomerName.Text.Trim(),
                    Phone1 = txtPhone1.Text.Trim(),
                    Phone2 = txtPhone2.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Address = txtAddress.Text.Trim(),
                    CurrentBalance = decimal.TryParse(txtBalance.Text, out decimal b) ? b : 0,
                    Notes = txtNotes.Text.Trim(),
                    CreditLimit = decimal.TryParse(txtCreditLimit.Text, out decimal l) ? l : 0,
                    AllowCredit = chkAllowCredit.Checked,
                    DiscountPercent = float.TryParse(txtDiscount.Text, out float d) ? d : 0f,
                    IsActive = cust_status,
                    StopReason = txtStopReason.Text.Trim(),
                    Rating = cmbRating.SelectedIndex >= 0 ? cmbRating.SelectedIndex + 1 : 0,
                    CreatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    CreatedByUserID = UserSession.UserId,
                    LastTransactionDate = ""
                };

                _disableAutoUpdate = true;
                customerList.Add(newCustomer);
                allCustomers = customerList.ToList();
                _disableAutoUpdate = false;

                customerBinding.Position = customerBinding.IndexOf(newCustomer);
                ToastManager.ShowSuccess("نجاح", "تمت الإضافة بنجاح ✅");
                ClearCustomerForm();
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في الإضافة: " + ex.Message);
            }
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvCustomers.CurrentRow == null) return;

            try
            {
                int customerId = Convert.ToInt32(dgvCustomers.CurrentRow.Cells["colCustomerID"].Value);
                string customerName = Convert.ToString(dgvCustomers.CurrentRow.Cells["colCustomerName"].Value);

                if (!frmConfirm.Show("تأكيد الحذف", $"هل تريد حذف العميل ({customerName})؟")) return;

                await SoftDeleteCustomerAsync(customerId);

                var customer = customerList.FirstOrDefault(c => c.CustomerID == customerId);
                if (customer != null)
                {
                    _disableAutoUpdate = true;
                    customerList.Remove(customer);
                    allCustomers = customerList.ToList();
                    _disableAutoUpdate = false;
                }

                ClearCustomerForm();
                ToastManager.ShowSuccess("نجاح", "تم الحذف ✅");
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في الحذف: " + ex.Message);
            }
        }

        private async Task SoftDeleteCustomerAsync(int customerId)
        {
            using (SqlConnection conn = DB_Server.GetConnection())
            using (SqlCommand cmd = new SqlCommand(@"
                UPDATE dbo.Customers 
                SET IsDeleted = 1 
                WHERE CustomerID = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", customerId);
                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private void dgvCustomers_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e) { }

        private void frmCustomers_InputLanguageChanged(object sender, InputLanguageChangedEventArgs e)
        {
            lblLang.Text = e.InputLanguage.Culture.TwoLetterISOLanguageName.ToUpper();
        }

        private void dgvCustomers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dgvCustomers.Rows[e.RowIndex];

            txtCustomerCode.Text = row.Cells["colCustomerCode"].Value?.ToString();
            txtCustomerName.Text = row.Cells["colCustomerName"].Value?.ToString();
            txtPhone1.Text = row.Cells["colPhonePrimary"].Value?.ToString();
            txtPhone2.Text = row.Cells["colPhoneSecondary"].Value?.ToString();
            txtEmail.Text = row.Cells["colEmail"].Value?.ToString();
            txtAddress.Text = row.Cells["colAddress"].Value?.ToString();
            txtBalance.Text = row.Cells["colCurrentBalance"].Value?.ToString();
            txtNotes.Text = row.Cells["colNotes"].Value?.ToString();
            txtCreditLimit.Text = row.Cells["colCreditLimit"].Value?.ToString();
            txtDiscount.Text = row.Cells["colDefaultDiscountPercent"].Value?.ToString();
            txtStopReason.Text = row.Cells["colStopReason"].Value?.ToString();
            chkAllowCredit.Checked = Convert.ToBoolean(row.Cells["colAllowCreditSales"].Value);

            int ratingIndex = Convert.ToInt32(row.Cells["colCustomerRating"].Value) - 1;
            cmbRating.SelectedIndex = ratingIndex >= 0 ? ratingIndex : 0;

            cust_status = Convert.ToBoolean(row.Cells["colCustomerStatus"].Value);
            SetButtonState(cust_status);

            lblCreatedAt.Text = row.Cells["colCreatedAt"].Value?.ToString();
            lblLastActivity.Text = row.Cells["colLastActivityAt"].Value?.ToString();
            lblAddedBy.Text = row.Cells["colAddedByUser"].Value?.ToString();
        }

        private async void SearchTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Invoke(new Action(() =>
            {
                string keyword = txtSearch.Text;
                string field = cmbSearchField.SelectedItem?.ToString() ?? "اسم العميل";
                ApplySearchFilter(keyword, field);
            }));
        }

        private void ApplySearchFilter(string keyword, string field)
        {
            keyword = keyword.Trim();

            IEnumerable<Customer> result = allCustomers;

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                switch (field)
                {
                    case "كود العميل": result = result.Where(c => (c.CustomerCode ?? "").Contains(keyword)); break;
                    case "اسم العميل": result = result.Where(c => (c.CustomerName ?? "").Contains(keyword)); break;
                    case "رقم الهاتف الأساسي": result = result.Where(c => (c.Phone1 ?? "").Contains(keyword)); break;
                    case "رقم هاتف إضافي": result = result.Where(c => (c.Phone2 ?? "").Contains(keyword)); break;
                    case "البريد الإلكتروني": result = result.Where(c => (c.Email ?? "").Contains(keyword)); break;
                    case "العنوان": result = result.Where(c => (c.Address ?? "").Contains(keyword)); break;
                    case "سبب الإيقاف": result = result.Where(c => (c.StopReason ?? "").Contains(keyword)); break;
                    case "تاريخ الإنشاء": result = result.Where(c => (c.CreatedDate ?? "").Contains(keyword)); break;
                    case "آخر تعامل": result = result.Where(c => (c.LastTransactionDate ?? "").Contains(keyword)); break;
                    case "الرصيد الحالي":
                        if (decimal.TryParse(keyword, out decimal val))
                            result = result.Where(c => c.CurrentBalance == val);
                        else
                            result = result.Where(c => c.CurrentBalance.ToString().Contains(keyword));
                        break;
                    case "تقييم العميل":
                        if (int.TryParse(keyword, out int val2))
                            result = result.Where(c => c.Rating == val2);
                        else
                            result = result.Where(c => c.Rating.ToString().Contains(keyword));
                        break;
                    default:
                        result = result.Where(c => (c.CustomerName ?? "").Contains(keyword));
                        break;
                }
            }

            _disableAutoUpdate = true;
            customerList.Clear();
            foreach (var c in result)
                customerList.Add(c);
            _disableAutoUpdate = false;
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            searchTimer.Stop();
            searchTimer.Start();
        }

        private async Task<int> InsertCustomerAsync()
        {
            string customerCode = txtCustomerCode.Text.Trim();

            using (SqlConnection conn = DB_Server.GetConnection())
            // ✅ تم إضافة SELECT SCOPE_IDENTITY() لإرجاع الـ ID الجديد
            using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO Customers (
                    CustomerCode, CustomerName, Phone1, Phone2, Email, Address, 
                    CurrentBalance, Notes, CreditLimit, DiscountPercent, IsActive, 
                    StopReason, Rating, AllowCredit, CreatedByUserID) 
                VALUES (
                    @Code, @Name, @P1, @P2, @Email, @Addr, @Balance, @Notes, @Limit, 
                    @Disc, @Active, @Stop, @Rating, @Allow, @User);
                SELECT SCOPE_IDENTITY();", conn))
            {
                cmd.Parameters.AddWithValue("@Code", customerCode);
                cmd.Parameters.AddWithValue("@Name", txtCustomerName.Text.Trim());
                cmd.Parameters.AddWithValue("@P1", txtPhone1.Text.Trim());
                cmd.Parameters.AddWithValue("@P2", txtPhone2.Text.Trim());
                cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                cmd.Parameters.AddWithValue("@Addr", txtAddress.Text.Trim());
                cmd.Parameters.AddWithValue("@Balance", decimal.TryParse(txtBalance.Text, out decimal b) ? b : 0);
                cmd.Parameters.AddWithValue("@Notes", txtNotes.Text);
                cmd.Parameters.AddWithValue("@Limit", decimal.TryParse(txtCreditLimit.Text, out decimal l) ? l : 0);
                cmd.Parameters.AddWithValue("@Disc", double.TryParse(txtDiscount.Text, out double d) ? d : 0);
                cmd.Parameters.AddWithValue("@Active", cust_status);
                cmd.Parameters.AddWithValue("@Stop", txtStopReason.Text);
                cmd.Parameters.AddWithValue("@Rating", cmbRating.SelectedIndex >= 0 ? cmbRating.SelectedIndex + 1 : 0);
                cmd.Parameters.AddWithValue("@Allow", chkAllowCredit.Checked);
                cmd.Parameters.AddWithValue("@User", UserSession.UserId);

                await conn.OpenAsync();
                int newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                return newId;
            }
        }

        private void btn_clear_Click(object sender, EventArgs e) => ClearCustomerForm();

        private void dgvCustomers_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvCustomers.Columns[e.ColumnIndex].Name != "colCustomerRating") return;

            e.PaintBackground(e.ClipBounds, true);

            int stars = 0;
            if (e.Value != null && int.TryParse(e.Value.ToString(), out int rating))
                stars = rating;

            int starSize = 16;
            int spacing = 2;

            for (int i = 0; i < 5; i++)
            {
                Rectangle rect = new Rectangle(
                    e.CellBounds.Left + i * (starSize + spacing) + 2,
                    e.CellBounds.Top + (e.CellBounds.Height - starSize) / 2,
                    starSize,
                    starSize
                );

                Brush brush = i < stars ? Brushes.Gold : Brushes.LightGray;
                e.Graphics.FillPolygon(brush, GetStarPoints(rect));
            }

            e.Handled = true;
        }

        private PointF[] GetStarPoints(Rectangle r)
        {
            PointF[] pts = new PointF[10];
            float cx = r.X + r.Width / 2f;
            float cy = r.Y + r.Height / 2f;
            float radius = r.Width / 2f;
            float innerRadius = radius * 0.5f;

            for (int i = 0; i < 10; i++)
            {
                double angle = i * Math.PI / 5 - Math.PI / 2;
                float rCurrent = (i % 2 == 0) ? radius : innerRadius;
                pts[i] = new PointF(
                    cx + rCurrent * (float)Math.Cos(angle),
                    cy + rCurrent * (float)Math.Sin(angle)
                );
            }
            return pts;
        }

        private bool ValidateCustomerInputs(out string errorMessage)
        {
            errorMessage = "";

            if (string.IsNullOrWhiteSpace(txtCustomerCode.Text))
            { errorMessage = "من فضلك أدخل كود العميل"; txtCustomerCode.Focus(); return false; }

            if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
            { errorMessage = "من فضلك أدخل اسم العميل"; txtCustomerName.Focus(); return false; }

            if (string.IsNullOrWhiteSpace(txtPhone1.Text))
            { errorMessage = "من فضلك أدخل رقم هاتف أساسي"; txtPhone1.Focus(); return false; }

            if (!Regex.IsMatch(txtPhone1.Text.Trim(), @"^\d{10,15}$"))
            { errorMessage = "رقم الهاتف غير صحيح"; txtPhone1.Focus(); return false; }

            if (!string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                try { var mail = new System.Net.Mail.MailAddress(txtEmail.Text); }
                catch { errorMessage = "البريد الإلكتروني غير صحيح"; txtEmail.Focus(); return false; }
            }

            if (!decimal.TryParse(txtBalance.Text, out decimal balance))
            { errorMessage = "الرصيد الحالي يجب أن يكون رقمًا صحيحًا"; txtBalance.Focus(); return false; }

            if (!decimal.TryParse(txtCreditLimit.Text, out decimal creditLimit))
            { errorMessage = "حد الائتمان يجب أن يكون رقمًا"; txtCreditLimit.Focus(); return false; }

            if (!double.TryParse(txtDiscount.Text, out double discount))
            { errorMessage = "نسبة الخصم يجب أن تكون رقمًا"; txtDiscount.Focus(); return false; }

            if (discount < 0 || discount > 100)
            { errorMessage = "نسبة الخصم يجب أن تكون بين 0 و 100"; txtDiscount.Focus(); return false; }

            if (chkAllowCredit.Checked && creditLimit <= 0)
            { errorMessage = "يجب تحديد حد ائتمان أكبر من صفر عند تفعيل البيع الآجل"; txtCreditLimit.Focus(); return false; }

            if (!cust_status && string.IsNullOrWhiteSpace(txtStopReason.Text))
            { errorMessage = "من فضلك أدخل سبب إيقاف العميل"; txtStopReason.Focus(); return false; }

            return true;
        }
    }
}