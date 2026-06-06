using Guna.UI2.WinForms;
using Sestamk.Classes;
using Sestamk.Classes.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Sestamk.Forms
{
    public partial class frmReturns : BaseForm
    {
        protected override Size DesignClientSize => new Size(1100, 750);

        // ═══ الحالة الداخلية ═══
        private int _currentOrderId;
        private string _currentOrderNumber = string.Empty;
        private List<ReturnItemModel> _returnItems = new();

        // أعمدة الجريد (ثوابت للوصول السريع)
        private const int COL_SELECT = 0;
        private const int COL_NAME   = 1;
        private const int COL_PRICE  = 2;
        private const int COL_ORIG   = 3;
        private const int COL_AVAIL  = 4;
        private const int COL_RETURN = 5;
        private const int COL_TOTAL  = 6;

        public frmReturns()
        {
            InitializeComponent();

            var borderless = new Guna2BorderlessForm();
            borderless.ContainerControl = this;
            borderless.BorderRadius = 20;
            borderless.ResizeForm = true;
            borderless.AnimateWindow = true;

            Main_Methods.Attach(pnlHeader, this);
            Main_Methods.Attach(lblTitle, this);

            BuildItemsGrid();
            WireEvents();
            SetEmptyState();
        }

        // ═══════════════════════════════════════════════════
        //  بناء الجريد وأعمدته
        // ═══════════════════════════════════════════════════

        private void BuildItemsGrid()
        {
            dgvItems.Columns.Clear();
            dgvItems.AllowUserToAddRows = false;
            dgvItems.AllowUserToDeleteRows = false;
            dgvItems.MultiSelect = false;
            dgvItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvItems.EditMode = DataGridViewEditMode.EditOnEnter;

            // عمود الاختيار (Checkbox)
            var colCheck = new DataGridViewCheckBoxColumn
            {
                Name = "colSelect",
                HeaderText = "مرتجع؟",
                Width = 80,
                FalseValue = false,
                TrueValue = true,
                ReadOnly = false
            };
            dgvItems.Columns.Add(colCheck);

            // اسم المنتج
            var colName = new DataGridViewTextBoxColumn
            {
                Name = "colName",
                HeaderText = "الصنف",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = 200
            };
            dgvItems.Columns.Add(colName);

            // السعر
            var colPrice = new DataGridViewTextBoxColumn
            {
                Name = "colPrice",
                HeaderText = "سعر الوحدة",
                Width = 120,
                ReadOnly = true,
                DefaultCellStyle = { Format = "N3", Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            dgvItems.Columns.Add(colPrice);

            // الكمية الأصلية
            var colOrig = new DataGridViewTextBoxColumn
            {
                Name = "colOrig",
                HeaderText = "الكمية الأصلية",
                Width = 120,
                ReadOnly = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            dgvItems.Columns.Add(colOrig);

            // الكمية المتاحة للإرجاع
            var colAvail = new DataGridViewTextBoxColumn
            {
                Name = "colAvail",
                HeaderText = "المتاح للإرجاع",
                Width = 120,
                ReadOnly = true,
                DefaultCellStyle = {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    ForeColor = Color.FromArgb(59, 130, 246)
                }
            };
            dgvItems.Columns.Add(colAvail);

            // كمية الإرجاع (قابلة للتعديل)
            var colReturn = new DataGridViewTextBoxColumn
            {
                Name = "colReturn",
                HeaderText = "كمية الإرجاع ✏",
                Width = 130,
                ReadOnly = false,
                DefaultCellStyle = {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    BackColor = Color.FromArgb(255, 251, 235),
                    ForeColor = Color.FromArgb(55, 48, 0)
                }
            };
            dgvItems.Columns.Add(colReturn);

            // إجمالي السطر
            var colLineTotal = new DataGridViewTextBoxColumn
            {
                Name = "colTotal",
                HeaderText = "إجمالي الإرجاع",
                Width = 140,
                ReadOnly = true,
                DefaultCellStyle = {
                    Format = "N3",
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    ForeColor = Color.FromArgb(239, 68, 68),
                    Font = new Font("Alexandria", 10F, FontStyle.Bold)
                }
            };
            dgvItems.Columns.Add(colLineTotal);
        }

        // ═══════════════════════════════════════════════════
        //  ربط الأحداث
        // ═══════════════════════════════════════════════════

        private void WireEvents()
        {
            btnSearchInvoice.Click += async (s, e) => await SearchInvoiceAsync();
            btnSaveReturn.Click += async (s, e) => await SaveReturnAsync();
            btnCancel.Click += (s, e) => SetEmptyState();

            txtInvoiceNumber.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    _ = SearchInvoiceAsync();
                }
            };

            // checkbox تغيير حالة الاختيار
            dgvItems.CellValueChanged += DgvItems_CellValueChanged;
            dgvItems.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (dgvItems.IsCurrentCellDirty &&
                    dgvItems.CurrentCell?.ColumnIndex == COL_SELECT)
                    dgvItems.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };

            // التحقق من قيمة كمية الإرجاع عند إنهاء التعديل
            dgvItems.CellEndEdit += DgvItems_CellEndEdit;
        }

        // ═══════════════════════════════════════════════════
        //  البحث عن الفاتورة
        // ═══════════════════════════════════════════════════

        private async System.Threading.Tasks.Task SearchInvoiceAsync()
        {
            string num = txtInvoiceNumber.Text.Trim();
            if (string.IsNullOrEmpty(num))
            {
                ToastManager.ShowWarning("تنبيه", "أدخل رقم الفاتورة أولاً");
                return;
            }

            try
            {
                lblStatus.Text = "جاري البحث...";
                btnSearchInvoice.Enabled = false;
                this.Cursor = Cursors.WaitCursor;
                SetEmptyState();

                var row = await ReturnService.SearchOrderAsync(num);

                if (row == null)
                {
                    ToastManager.ShowWarning("غير موجود", $"لم يتم العثور على فاتورة برقم: {num}");
                    lblStatus.Text = "لم يتم العثور على الفاتورة";
                    return;
                }

                // ملء معلومات الفاتورة
                _currentOrderId = Convert.ToInt32(row["OrderID"]);
                _currentOrderNumber = row["OrderNumber"].ToString()!;

                lblValInvoiceNum.Text = _currentOrderNumber;
                lblValDate.Text = Convert.ToDateTime(row["OrderDate"]).ToString("yyyy/MM/dd HH:mm");
                lblValCustomer.Text = row["CustomerName"]?.ToString() ?? "عميل نقدي";
                lblValTotal.Text = $"{Convert.ToDecimal(row["TotalAmount"]):N3}";

                // تحميل الأصناف
                _returnItems = await ReturnService.GetOrderItemsForReturnAsync(_currentOrderId);

                if (_returnItems.Count == 0)
                {
                    ToastManager.ShowWarning("تنبيه", "جميع أصناف هذه الفاتورة تم إرجاعها مسبقاً");
                    lblStatus.Text = "لا توجد أصناف متاحة للإرجاع";
                    return;
                }

                LoadItemsGrid();
                lblStatus.Text = $"تم تحميل {_returnItems.Count} صنف — اختر ما تريد إرجاعه";
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في البحث: " + ex.Message);
                lblStatus.Text = "خطأ في البحث";
            }
            finally
            {
                btnSearchInvoice.Enabled = true;
                this.Cursor = Cursors.Default;
            }
        }

        // ═══════════════════════════════════════════════════
        //  تحميل الأصناف في الجريد
        // ═══════════════════════════════════════════════════

        private void LoadItemsGrid()
        {
            dgvItems.Rows.Clear();

            foreach (var item in _returnItems)
            {
                int rowIdx = dgvItems.Rows.Add();
                var row = dgvItems.Rows[rowIdx];

                row.Cells[COL_SELECT].Value = false;
                row.Cells[COL_NAME].Value   = item.DisplayName;
                row.Cells[COL_PRICE].Value  = item.UnitPrice;
                row.Cells[COL_ORIG].Value   = item.OriginalQuantity;
                row.Cells[COL_AVAIL].Value  = item.AvailableToReturn;
                row.Cells[COL_RETURN].Value = item.AvailableToReturn; // افتراضي: الكمية كاملة
                row.Cells[COL_TOTAL].Value  = 0m;

                // تعطيل التعديل على كمية الإرجاع لحين الاختيار
                row.Cells[COL_RETURN].ReadOnly = true;
                row.Cells[COL_RETURN].Style.BackColor = Color.FromArgb(240, 240, 240);
            }

            RecalculateTotal();
        }

        // ═══════════════════════════════════════════════════
        //  أحداث الجريد
        // ═══════════════════════════════════════════════════

        private void DgvItems_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (e.ColumnIndex == COL_SELECT)
            {
                bool selected = Convert.ToBoolean(dgvItems.Rows[e.RowIndex].Cells[COL_SELECT].Value);
                var retCell = dgvItems.Rows[e.RowIndex].Cells[COL_RETURN];

                if (selected)
                {
                    retCell.ReadOnly = false;
                    retCell.Style.BackColor = Color.FromArgb(255, 251, 235);
                    retCell.Style.ForeColor = Color.FromArgb(55, 48, 0);
                }
                else
                {
                    retCell.ReadOnly = true;
                    retCell.Style.BackColor = Color.FromArgb(240, 240, 240);
                    retCell.Style.ForeColor = Color.Gray;
                    dgvItems.Rows[e.RowIndex].Cells[COL_TOTAL].Value = 0m;
                }

                RecalculateTotal();
            }

            if (e.ColumnIndex == COL_RETURN)
                UpdateRowTotal(e.RowIndex);
        }

        private void DgvItems_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != COL_RETURN) return;

            var row = dgvItems.Rows[e.RowIndex];
            var item = _returnItems[e.RowIndex];

            if (!int.TryParse(row.Cells[COL_RETURN].Value?.ToString(), out int qty) || qty < 1)
            {
                row.Cells[COL_RETURN].Value = 1;
                qty = 1;
            }

            if (qty > item.AvailableToReturn)
            {
                row.Cells[COL_RETURN].Value = item.AvailableToReturn;
                ToastManager.ShowWarning("تنبيه",
                    $"الكمية المتاحة للإرجاع من «{item.DisplayName}» هي {item.AvailableToReturn} فقط");
            }

            UpdateRowTotal(e.RowIndex);
        }

        private void UpdateRowTotal(int rowIdx)
        {
            var row = dgvItems.Rows[rowIdx];
            bool selected = Convert.ToBoolean(row.Cells[COL_SELECT].Value);
            if (!selected) { row.Cells[COL_TOTAL].Value = 0m; RecalculateTotal(); return; }

            if (!int.TryParse(row.Cells[COL_RETURN].Value?.ToString(), out int qty)) qty = 0;
            decimal price = _returnItems[rowIdx].UnitPrice;
            row.Cells[COL_TOTAL].Value = Math.Round(price * qty, 3);

            RecalculateTotal();
        }

        // ═══════════════════════════════════════════════════
        //  حساب الإجمالي
        // ═══════════════════════════════════════════════════

        private void RecalculateTotal()
        {
            decimal total = 0m;
            int selectedCount = 0;

            foreach (DataGridViewRow row in dgvItems.Rows)
            {
                if (row.IsNewRow) continue;
                if (Convert.ToBoolean(row.Cells[COL_SELECT].Value))
                {
                    total += Convert.ToDecimal(row.Cells[COL_TOTAL].Value ?? 0m);
                    selectedCount++;
                }
            }

            lblTotalValue.Text = $"{total:N3}";
            btnSaveReturn.Enabled = selectedCount > 0 && total > 0;
        }

        // ═══════════════════════════════════════════════════
        //  حفظ المرتجع
        // ═══════════════════════════════════════════════════

        private async System.Threading.Tasks.Task SaveReturnAsync()
        {
            // جمع الأصناف المحددة
            var selectedItems = new List<ReturnItemModel>();
            for (int i = 0; i < dgvItems.Rows.Count; i++)
            {
                var row = dgvItems.Rows[i];
                if (row.IsNewRow) continue;
                if (!Convert.ToBoolean(row.Cells[COL_SELECT].Value)) continue;

                if (!int.TryParse(row.Cells[COL_RETURN].Value?.ToString(), out int qty) || qty <= 0)
                    continue;

                var item = _returnItems[i];
                selectedItems.Add(new ReturnItemModel
                {
                    OrderItemID   = item.OrderItemID,
                    ProductID     = item.ProductID,
                    ProductSizeID = item.ProductSizeID,
                    ProductName   = item.ProductName,
                    SizeName      = item.SizeName,
                    UnitPrice     = item.UnitPrice,
                    ReturnQuantity = qty,
                    LineTotal     = item.UnitPrice * qty
                });
            }

            if (selectedItems.Count == 0)
            {
                ToastManager.ShowWarning("تنبيه", "لم تحدد أي صنف للإرجاع");
                return;
            }

            decimal totalAmount = 0m;
            foreach (var i in selectedItems) totalAmount += i.LineTotal;

            // تأكيد الحفظ
            string refundMethodText = rdoCash.Checked ? "نقدي" : rdoCard.Checked ? "بطاقة" : "رصيد آجل";
            var confirm = MessageBox.Show(
                $"هل تريد إتمام المرتجع؟\n\nإجمالي المرتجع: {totalAmount:N3}\nطريقة الاسترداد: {refundMethodText}",
                "تأكيد المرتجع",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);

            if (confirm != DialogResult.Yes) return;

            try
            {
                lblStatus.Text = "جاري حفظ المرتجع...";
                btnSaveReturn.Enabled = false;
                this.Cursor = Cursors.WaitCursor;

                int refundMethod = rdoCash.Checked ? 0 : rdoCard.Checked ? 1 : 2;

                // إنشاء نموذج المرتجع
                var ret = new ReturnModel
                {
                    OriginalOrderID     = _currentOrderId,
                    OriginalOrderNumber = _currentOrderNumber,
                    ReturnDate          = DateTime.Now,
                    UserID              = UserSession.UserId,
                    ShiftID             = ShiftService.CurrentShift?.ShiftID ?? 0,
                    ReturnReason        = txtReason.Text.Trim(),
                    TotalReturnAmount   = totalAmount,
                    RefundMethod        = refundMethod,
                    Notes               = string.Empty,
                    Items               = selectedItems
                };

                // ضبط CustomerID من بيانات الفاتورة (إذا وُجد)
                // نجيب البيانات من الـ DB مرة واحدة بدل تخزينها في الحالة
                var orderRow = await ReturnService.SearchOrderAsync(_currentOrderNumber);
                if (orderRow != null && orderRow["CustomerID"] != DBNull.Value)
                {
                    ret.CustomerID = Convert.ToInt32(orderRow["CustomerID"]);
                    ret.CustomerName = orderRow["CustomerName"]?.ToString() ?? "";
                }

                // رصيد آجل يتطلب وجود عميل
                if (refundMethod == 2 && !ret.CustomerID.HasValue)
                {
                    ToastManager.ShowWarning("تنبيه",
                        "طريقة الاسترداد «رصيد آجل» تتطلب ربط العميل بالفاتورة الأصلية");
                    return;
                }

                int returnId = await ReturnService.SaveReturnAsync(ret);

                if (returnId > 0)
                {
                    ToastManager.ShowSuccess("تم",
                        $"تم حفظ المرتجع بنجاح — رقم المرتجع: {ret.ReturnNumber}");

                    lblStatus.Text = $"✅ تم حفظ المرتجع {ret.ReturnNumber} بنجاح";
                    SetEmptyState();
                    txtInvoiceNumber.Clear();
                }
                else
                {
                    ToastManager.ShowError("خطأ", "فشل حفظ المرتجع — حاول مرة أخرى");
                    lblStatus.Text = "فشل حفظ المرتجع";
                }
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في حفظ المرتجع: " + ex.Message);
                lblStatus.Text = "خطأ أثناء الحفظ";
            }
            finally
            {
                btnSaveReturn.Enabled = dgvItems.Rows.Count > 0;
                this.Cursor = Cursors.Default;
            }
        }

        // ═══════════════════════════════════════════════════
        //  إعادة الضبط لحالة فارغة
        // ═══════════════════════════════════════════════════

        private void SetEmptyState()
        {
            _currentOrderId = 0;
            _currentOrderNumber = string.Empty;
            _returnItems.Clear();
            dgvItems.Rows.Clear();

            lblValInvoiceNum.Text = "—";
            lblValDate.Text       = "—";
            lblValCustomer.Text   = "—";
            lblValTotal.Text      = "—";
            lblTotalValue.Text    = "0.000";

            txtReason.Clear();
            rdoCash.Checked = true;
            btnSaveReturn.Enabled = false;
            lblStatus.Text = "ابحث عن الفاتورة للبدء";
        }
    }
}
