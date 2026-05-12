using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sklad_Rynashko
{
    public partial class Form1 : Form
    {
        // Создаем объекты управления программно
        private Warehouse warehouse = new Warehouse();
        private DataGridView dgvInventory = new DataGridView();
        private TextBox txtName = new TextBox();
        private TextBox txtUnit = new TextBox();
        private NumericUpDown numQty = new NumericUpDown();
        private Button btnAdd = new Button();
        private Button btnShip = new Button();

        public Form1()
        {
            InitializeComponent();
            SetupInterface(); // Вызываем настройку интерфейса
        }

        private void SetupInterface()
        {
            this.Text = "Автоматизированное рабочее место: Склад";
            this.Size = new Size(800, 500);

            // Настройка таблицы (Ведомость)
            dgvInventory.Location = new Point(20, 20);
            dgvInventory.Size = new Size(740, 250);
            dgvInventory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.Controls.Add(dgvInventory);

            // Поля ввода
            Label lblName = new Label { Text = "Товар:", Location = new Point(20, 280), Width = 50 };
            txtName.Location = new Point(80, 280); txtName.Width = 150;

            Label lblUnit = new Label { Text = "Ед. изм:", Location = new Point(20, 310), Width = 50 };
            txtUnit.Location = new Point(80, 310); txtUnit.Width = 150;

            Label lblQty = new Label { Text = "Кол-во:", Location = new Point(20, 340), Width = 50 };
            numQty.Location = new Point(80, 340); numQty.Width = 150; numQty.Maximum = 10000;

            this.Controls.AddRange(new Control[] { lblName, txtName, lblUnit, txtUnit, lblQty, numQty });

            // Кнопки
            btnAdd.Text = "Оформить ПРИХОД";
            btnAdd.Location = new Point(250, 280);
            btnAdd.Size = new Size(150, 40);
            btnAdd.BackColor = Color.LightGreen;
            btnAdd.Click += BtnAdd_Click;

            btnShip.Text = "Оформить РАСХОД";
            btnShip.Location = new Point(250, 330);
            btnShip.Size = new Size(150, 40);
            btnShip.BackColor = Color.LightSalmon;
            btnShip.Click += BtnShip_Click;

            this.Controls.Add(btnAdd);
            this.Controls.Add(btnShip);
        }

        // Логика кнопки ПРИХОД
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text)) return;

            var product = new Product(txtName.Text, txtUnit.Text, (double)numQty.Value, DateTime.Now);
            warehouse.RegisterArrival(product);
            RefreshGrid();
        }

        // Логика кнопки РАСХОД
        private void BtnShip_Click(object sender, EventArgs e)
        {
            if (warehouse.RegisterShipment(txtName.Text, (double)numQty.Value))
                RefreshGrid();
            else
                MessageBox.Show("Недостаточно товара на складе!");
        }

        private void RefreshGrid()
        {
            dgvInventory.DataSource = null;
            dgvInventory.DataSource = warehouse.GetInventory();
        }
    }
}