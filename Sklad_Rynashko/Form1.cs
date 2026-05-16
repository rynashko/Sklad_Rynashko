using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Linq;

namespace Sklad_Rynashko
{
    public partial class Form1 : Form
    {
        private Warehouse warehouse = new Warehouse();

        private TabControl tabs = new TabControl();
        private TabPage pageStock = new TabPage("📦 Текущий склад");
        private TabPage pageHistory = new TabPage("📑 Архив накладных");

        private DataGridView dgvStock = new DataGridView();
        private DataGridView dgvHistory = new DataGridView();

        private TextBox txtName = new TextBox();
        private ComboBox cbUnit = new ComboBox();
        private NumericUpDown numQty = new NumericUpDown();

        public Form1()
        {
            InitializeComponent();
            this.Text = "АРМ Складской учет - Рынашко";
            this.Size = new Size(1100, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(45, 45, 48);

            SetupDesign();
            RefreshData();

            // Переключение вкладок без паники CurrencyManager
            tabs.SelectedIndexChanged += (s, e) => { RefreshData(); };
        }

        private void SetupDesign()
        {
            tabs.Dock = DockStyle.Fill;
            tabs.Padding = new Point(20, 10);
            this.Controls.Add(tabs);
            tabs.TabPages.Add(pageStock);
            tabs.TabPages.Add(pageHistory);

            pageStock.BackColor = Color.FromArgb(28, 28, 28);
            pageHistory.BackColor = Color.FromArgb(28, 28, 28);

            // --- ВКЛАДКА СКЛАД ---
            ApplyGridStyle(dgvStock);
            dgvStock.Dock = DockStyle.Top;
            dgvStock.Height = 380;
            dgvStock.CellClick += dgvStock_CellClick;
            pageStock.Controls.Add(dgvStock);

            Panel pnlActions = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 260,
                BackColor = Color.FromArgb(37, 37, 38),
                Padding = new Padding(30)
            };
            pageStock.Controls.Add(pnlActions);

            Label title = new Label
            {
                Text = "УПРАВЛЕНИЕ ЗАПАСАМИ И НАКЛАДНЫМИ",
                ForeColor = Color.DarkGray,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(30, 15),
                Width = 400
            };
            pnlActions.Controls.Add(title);

            StyleControl(pnlActions, "Наименование товара:", txtName, 55);

            // Настройка выпадающего списка
            Label lblUnit = new Label { Text = "Единица измерения:", Location = new Point(30, 113), Width = 140, ForeColor = Color.LightGray };
            cbUnit.Location = new Point(170, 110); cbUnit.Width = 200;
            cbUnit.BackColor = Color.FromArgb(30, 30, 30); cbUnit.ForeColor = Color.White;
            cbUnit.FlatStyle = FlatStyle.Flat;
            cbUnit.DropDownStyle = ComboBoxStyle.DropDown;
            cbUnit.KeyPress += (s, e) => e.Handled = true; // Блокировка ручного ввода текста

            cbUnit.Items.AddRange(new string[] { "шт.", "кг", "л", "м", "упак.", "т", "куб. м" });
            if (cbUnit.Items.Count > 0) cbUnit.SelectedIndex = 0;

            pnlActions.Controls.Add(lblUnit);
            pnlActions.Controls.Add(cbUnit);

            // Поле количества
            Label lblQty = new Label { Text = "Количество:", Location = new Point(30, 168), Width = 140, ForeColor = Color.LightGray };
            numQty.Location = new Point(170, 165); numQty.Width = 200;
            numQty.BackColor = Color.FromArgb(30, 30, 30); numQty.ForeColor = Color.White;
            numQty.BorderStyle = BorderStyle.FixedSingle; numQty.Maximum = 1000000;
            pnlActions.Controls.Add(lblQty); pnlActions.Controls.Add(numQty);

            // Кнопки действий
            Button btnIn = CreateStyledButton("📥 ПРИХОДНАЯ НАКЛАДНАЯ", new Point(420, 55), Color.FromArgb(45, 120, 45));
            btnIn.Click += (s, e) => ProcessOp(true);

            Button btnOut = CreateStyledButton("📤 РАСХОДНАЯ НАКЛАДНАЯ", new Point(420, 120), Color.FromArgb(140, 45, 45));
            btnOut.Click += (s, e) => ProcessOp(false);

            Button btnDelete = CreateStyledButton("❌ СПИСАТЬ / УДАЛИТЬ", new Point(750, 55), Color.FromArgb(70, 70, 70));
            btnDelete.Click += btnDelete_Click;

            Button btnExport = CreateStyledButton("📄 ВЫВЕСТИ ВЕДОМОСТЬ", new Point(750, 120), Color.FromArgb(30, 100, 150));
            btnExport.Click += btnExport_Click;

            pnlActions.Controls.Add(btnIn);
            pnlActions.Controls.Add(btnOut);
            pnlActions.Controls.Add(btnDelete);
            pnlActions.Controls.Add(btnExport);

            // --- ВКЛАДКА ИСТОРИЯ ---
            ApplyGridStyle(dgvHistory);
            dgvHistory.Dock = DockStyle.Fill;
            pageHistory.Controls.Add(dgvHistory);
        }

        private void ApplyGridStyle(DataGridView g)
        {
            g.BackgroundColor = Color.FromArgb(30, 30, 30);
            g.ForeColor = Color.White;
            g.BorderStyle = BorderStyle.None;
            g.GridColor = Color.FromArgb(50, 50, 50);
            g.EnableHeadersVisualStyles = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.ReadOnly = true;
            g.RowHeadersVisible = false;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(60, 60, 60);
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            g.DefaultCellStyle.BackColor = Color.FromArgb(35, 35, 35);
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(75, 75, 75);
        }

        private void StyleControl(Control parent, string labelText, Control ctrl, int y)
        {
            Label lbl = new Label { Text = labelText, Location = new Point(30, y + 3), Width = 140, ForeColor = Color.LightGray };
            ctrl.Location = new Point(170, y); ctrl.Width = 200;
            ctrl.BackColor = Color.FromArgb(30, 30, 30); ctrl.ForeColor = Color.White;
            if (ctrl is TextBox) ((TextBox)ctrl).BorderStyle = BorderStyle.FixedSingle;
            parent.Controls.Add(lbl); parent.Controls.Add(ctrl);
        }

        private Button CreateStyledButton(string text, Point loc, Color backColor)
        {
            return new Button
            {
                Text = text,
                Location = loc,
                Size = new Size(300, 50),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
        }

        // Исправленный обработчик клика по таблице с полной защитой от NullReferenceException
        private void dgvStock_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Проверяем, что кликнули именно по строке, а не по шапке
            if (e.RowIndex < 0 || e.RowIndex >= dgvStock.Rows.Count)
                return;

            var selectedRow = dgvStock.Rows[e.RowIndex];

            // Проверяем, что строка привязана к объекту данных
            if (selectedRow.DataBoundItem != null)
            {
                txtName.Text = selectedRow.Cells["Name"].Value?.ToString() ?? "";
                string unitValue = selectedRow.Cells["Unit"].Value?.ToString() ?? "";

                if (cbUnit.Items.Contains(unitValue))
                    cbUnit.SelectedItem = unitValue;
                else
                    cbUnit.Text = unitValue;
            }
        }

        private void ProcessOp(bool isArrival)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Ошибка: Укажите название товара!", "Валидация", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (numQty.Value <= 0)
            {
                MessageBox.Show("Ошибка: Количество должно быть больше нуля!", "Валидация", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedUnit = cbUnit.Text;
            if (string.IsNullOrWhiteSpace(selectedUnit)) selectedUnit = "шт.";

            if (isArrival)
            {
                warehouse.RegisterArrival(new Product(txtName.Text, selectedUnit, (double)numQty.Value, DateTime.Now));
            }
            else
            {
                if (!warehouse.RegisterShipment(txtName.Text, (double)numQty.Value))
                {
                    MessageBox.Show("Ошибка: Недостаточно товара на складе для отгрузки!", "Склад", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            RefreshData();
            ClearInputs();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Выберите или введите товар для списания!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dialogResult = MessageBox.Show($"Вы уверены, что хотите полностью списать товар '{txtName.Text}' со склада?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                if (warehouse.DeleteProduct(txtName.Text))
                {
                    RefreshData();
                    ClearInputs();
                }
                else
                {
                    MessageBox.Show("Товар с таким именем не найден на складе.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                string path = warehouse.ExportInventoryToTXT();
                MessageBox.Show($"Инвентарная ведомость успешно сгенерирована и сохранена в файл:\n\n{path}", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось сохранить файл: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshData()
        {
            dgvStock.DataSource = null; // Сброс нужен, чтобы принудительно обновить UI таблиц
            var inventory = warehouse.GetInventory();
            if (inventory != null)
            {
                dgvStock.DataSource = inventory.ToArray();
                if (dgvStock.Columns.Count > 0)
                {
                    if (dgvStock.Columns["Name"] != null) dgvStock.Columns["Name"].HeaderText = "Товар";
                    if (dgvStock.Columns["Unit"] != null) dgvStock.Columns["Unit"].HeaderText = "Ед. изм.";
                    if (dgvStock.Columns["Quantity"] != null) dgvStock.Columns["Quantity"].HeaderText = "Остаток";
                    if (dgvStock.Columns["LastDelivery"] != null) dgvStock.Columns["LastDelivery"].HeaderText = "Последний завоз";
                }
            }

            dgvHistory.DataSource = null;
            var invoices = warehouse.GetInvoices();
            if (invoices != null)
            {
                dgvHistory.DataSource = invoices.ToArray();
                if (dgvHistory.Columns.Count > 0)
                {
                    if (dgvHistory.Columns["Number"] != null) dgvHistory.Columns["Number"].HeaderText = "№ Документа";
                    if (dgvHistory.Columns["Date"] != null) dgvHistory.Columns["Date"].HeaderText = "Дата / Время";
                    if (dgvHistory.Columns["Type"] != null) dgvHistory.Columns["Type"].HeaderText = "Тип операции";
                    if (dgvHistory.Columns["Content"] != null) dgvHistory.Columns["Content"].HeaderText = "Содержание операции";
                }
            }

            if (dgvStock.Rows.Count > 0) dgvStock.ClearSelection();
            if (dgvHistory.Rows.Count > 0) dgvHistory.ClearSelection();
        }

        private void ClearInputs()
        {
            txtName.Clear();
            if (cbUnit.Items.Count > 0) cbUnit.SelectedIndex = 0;
            numQty.Value = 0;
        }
    }
}