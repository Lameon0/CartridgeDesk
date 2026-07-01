using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Toolkit.Uwp.Notifications;

namespace OM_CartridgeDesk
{
    public partial class OpClient : Form
    {
        private static readonly string dbPath = ConfigurationManager.AppSettings["DatabasePath"];
        private static readonly string connectionString = GetConnectionString();

        private static string GetConnectionString()
        {
            string[] providers = {
                "Microsoft.ACE.OLEDB.12.0",
                "Microsoft.ACE.OLEDB.16.0",
                "Microsoft.Jet.OLEDB.4.0"
            };

            foreach (string provider in providers)
            {
                try
                {
                    string testConn = $@"Provider={provider};Data Source={dbPath};";
                    using (OleDbConnection conn = new OleDbConnection(testConn))
                    {
                        conn.Open();
                        return testConn;
                    }
                }
                catch
                {
                }
            }

            MessageBox.Show(
                "Не найден подходящий драйвер для подключения к базе данных Access.\n\n" +
                "Установите Microsoft Access Database Engine:\n" +
                "https://www.microsoft.com/ru-ru/download/details.aspx?id=54920",
                "Ошибка подключения",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            Environment.Exit(1);
            return null;
        }

        private List<int> previousOrderIds = new List<int>();
        private readonly object orderLock = new object();

        public OpClient()
        {
            InitializeComponent();

            ToastNotificationManagerCompat.OnActivated += OnToastActivated;

            dgvCurrentOrders.CellContentClick += DgvCurrentOrders_CellContentClick;
            dgvStock.CellContentClick += dgvStock_CellContentClick;
            dgvOrdersHistory.CellContentClick += DgvOrdersHistory_CellContentClick;
            timer1.Tick += timer1_Tick_1;
            dgvDepartments.SelectionChanged += dgvDepartments_SelectionChanged;
            btnSaveRelations.Click += btnSaveRelations_Click_1;
            dgvDepartments.UserDeletingRow += dgvDepartments_UserDeletingRow;
            dgvPrinters.UserDeletingRow += dgvPrinters_UserDeletingRow;
            btnDepart.Click += btnDepart_Click;
            btnPrinter.Click += btnPrinter_Click;
            dgvPrinters.SelectionChanged += dgvPrinters_SelectionChanged;
            btnSaveCartridge.Click += btnSaveCartridge_Click;
            dgvCartridges.UserDeletingRow += dgvCartridges_UserDeletingRow;

            dgvCartridges.AllowUserToAddRows = false;
            dgvCartridges.AllowUserToDeleteRows = true;
            dgvCartridges.ReadOnly = false;

            typeof(DataGridView).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
                null, dgvCurrentOrders, new object[] { true });

            dgvDepartments.AllowUserToAddRows = false;
            dgvDepartments.AllowUserToDeleteRows = true;
            dgvPrinters.AllowUserToAddRows = false;
            dgvPrinters.AllowUserToDeleteRows = true;

            RefreshAll();
            RefreshCartridgesGrid();

            var dt = GetCurrentOrdersIds();
            previousOrderIds = dt.AsEnumerable().Select(r => r.Field<int>("ID")).ToList();
        }

        private void OnToastActivated(ToastNotificationActivatedEventArgsCompat args)
        {
            this.Invoke((MethodInvoker)delegate
            {
                this.WindowState = FormWindowState.Normal;
                this.Show();
                this.BringToFront();
                if (tabControl1.InvokeRequired)
                    tabControl1.Invoke((MethodInvoker)(() => tabControl1.SelectedIndex = 0));
                else
                    tabControl1.SelectedIndex = 0;
            });
        }

        private void ShowWindowsNotification(string title, string message)
        {
            new ToastContentBuilder()
                .AddArgument("action", "open")
                .AddText(title)
                .AddText(message)
                .Show();
        }

        private void button3_Click(object sender, EventArgs e) => btnCartridge_Click(sender, e);

        private string InputBox(string prompt, string title, string defaultValue = "")
        {
            using (var form = new Form())
            {
                form.Text = title;
                form.Size = new System.Drawing.Size(400, 150);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.MaximizeBox = false;
                form.MinimizeBox = false;

                var label = new Label() { Left = 10, Top = 10, Width = 360, Text = prompt };
                var textBox = new TextBox() { Left = 10, Top = 40, Width = 360, Text = defaultValue };
                var buttonOk = new Button() { Text = "OK", Left = 200, Top = 70, Width = 80, DialogResult = DialogResult.OK };
                var buttonCancel = new Button() { Text = "Отмена", Left = 290, Top = 70, Width = 80, DialogResult = DialogResult.Cancel };

                form.Controls.Add(label);
                form.Controls.Add(textBox);
                form.Controls.Add(buttonOk);
                form.Controls.Add(buttonCancel);
                form.AcceptButton = buttonOk;
                form.CancelButton = buttonCancel;

                return form.ShowDialog() == DialogResult.OK ? textBox.Text : "";
            }
        }

        private void RefreshAll()
        {
            RefreshDepartmentsGrid();
            RefreshCurrentOrders();
            RefreshOrdersHistory();
            RefreshStockGrid();
        }

        private DataTable GetDataTable(string query, params OleDbParameter[] parameters)
        {
            using (var conn = new OleDbConnection(connectionString))
            using (var cmd = new OleDbCommand(query, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                var da = new OleDbDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        private void AddButtonColumn(DataGridView dgv, string name, string header, string text)
        {
            var btn = new DataGridViewButtonColumn
            {
                Name = name,
                HeaderText = header,
                Text = text,
                UseColumnTextForButtonValue = true
            };
            dgv.Columns.Add(btn);
        }

        private void RefreshCurrentOrders()
        {
            dgvCurrentOrders.DataSource = null;
            dgvCurrentOrders.Columns.Clear();

            string query = @"SELECT O.ID, O.Order_date AS [Дата], O.UserName AS [Пользователь], 
                                    D.DepName AS [Отдел], P.[Model printer] AS [Принтер], 
                                    C.[Model name] AS [Картридж], O.CartridgeValue AS [Кол-во], O.CartridgeID
                             FROM (((Orders O
                             INNER JOIN Departments D ON O.Department_ID = D.id)
                             INNER JOIN Printers P ON O.Printer_ID = P.ID)
                             INNER JOIN Cartridge C ON O.CartridgeID = C.ID)
                             WHERE O.[Status] = 'В работе'
                             ORDER BY O.Order_date DESC";

            try
            {
                var dt = GetDataTable(query);
                dgvCurrentOrders.DataSource = dt;
                if (dgvCurrentOrders.Columns["ID"] != null) dgvCurrentOrders.Columns["ID"].Visible = false;
                if (dgvCurrentOrders.Columns["CartridgeID"] != null) dgvCurrentOrders.Columns["CartridgeID"].Visible = false;
                foreach (DataGridViewColumn col in dgvCurrentOrders.Columns)
                    if (col.Name != "ID" && col.Name != "CartridgeID") col.ReadOnly = true;

                AddButtonColumn(dgvCurrentOrders, "CompleteAction", "Действие", "Завершить");
                AddButtonColumn(dgvCurrentOrders, "RejectAction", "", "Отклонить");
                dgvCurrentOrders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка загрузки заявок:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void DgvCurrentOrders_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgvCurrentOrders.Rows[e.RowIndex];
            int orderId = Convert.ToInt32(row.Cells["ID"].Value);
            int cartridgeId = Convert.ToInt32(row.Cells["CartridgeID"].Value);
            int qty = Convert.ToInt32(row.Cells["Кол-во"].Value);
            string model = row.Cells["Картридж"].Value.ToString();

            if (dgvCurrentOrders.Columns[e.ColumnIndex].Name == "CompleteAction")
            {
                using (var connCheck = new OleDbConnection(connectionString))
                {
                    connCheck.Open();
                    var checkCmd = new OleDbCommand("SELECT Quantity FROM Cartridge WHERE ID = @cId", connCheck);
                    checkCmd.Parameters.AddWithValue("@cId", cartridgeId);
                    int stock = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (stock < qty)
                    {
                        MessageBox.Show($"Недостаточно! Доступно: {stock} шт.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                if (MessageBox.Show($"Завершить заявку №{orderId} ({model}, {qty} шт.)?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    using (var conn = new OleDbConnection(connectionString))
                    {
                        conn.Open();
                        var tran = conn.BeginTransaction();
                        try
                        {
                            new OleDbCommand($"UPDATE Cartridge SET Quantity = Quantity - {qty} WHERE ID = {cartridgeId}", conn, tran).ExecuteNonQuery();
                            new OleDbCommand($"UPDATE Orders SET [Status] = 'Завершена' WHERE ID = {orderId}", conn, tran).ExecuteNonQuery();
                            tran.Commit();
                            MessageBox.Show("Заявка закрыта, картриджи списаны.", "Успех");
                            RefreshCurrentOrders();
                            RefreshOrdersHistory();
                            RefreshStockGrid();
                        }
                        catch { tran.Rollback(); throw; }
                    }
                }
            }
            else if (dgvCurrentOrders.Columns[e.ColumnIndex].Name == "RejectAction")
            {
                if (MessageBox.Show($"Отклонить заявку №{orderId} ({model})?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    using (var conn = new OleDbConnection(connectionString))
                    {
                        conn.Open();
                        new OleDbCommand($"UPDATE Orders SET [Status] = 'Отклонена' WHERE ID = {orderId}", conn).ExecuteNonQuery();
                        MessageBox.Show("Заявка отклонена.", "Успех");
                        RefreshCurrentOrders();
                        RefreshOrdersHistory();
                    }
                }
            }
        }

        private void RefreshOrdersHistory()
        {
            dgvOrdersHistory.DataSource = null;
            dgvOrdersHistory.Columns.Clear();

            string query = @"SELECT O.ID AS [№ Заявки], O.Order_date AS [Дата выдачи], O.UserName AS [Пользователь], 
                                    D.DepName AS [Отдел], P.[Model printer] AS [Принтер], 
                                    C.[Model name] AS [Картридж], O.CartridgeValue AS [Выдано],
                                    O.CartridgeID, O.[Status] AS [Статус]
                             FROM (((Orders O
                             INNER JOIN Departments D ON O.Department_ID = D.id)
                             INNER JOIN Printers P ON O.Printer_ID = P.ID)
                             INNER JOIN Cartridge C ON O.CartridgeID = C.ID)
                             WHERE O.[Status] IN ('Завершена', 'Отклонена')
                             ORDER BY O.Order_date DESC";

            try
            {
                var dt = GetDataTable(query);
                dgvOrdersHistory.DataSource = dt;
                if (dgvOrdersHistory.Columns["CartridgeID"] != null) dgvOrdersHistory.Columns["CartridgeID"].Visible = false;
                AddButtonColumn(dgvOrdersHistory, "UndoAction", "Действие", "Отменить");
                dgvOrdersHistory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка загрузки истории:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void DgvOrdersHistory_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgvOrdersHistory.Columns[e.ColumnIndex].Name != "UndoAction") return;
            var row = dgvOrdersHistory.Rows[e.RowIndex];
            int orderId = Convert.ToInt32(row.Cells["№ Заявки"].Value);
            int cartridgeId = Convert.ToInt32(row.Cells["CartridgeID"].Value);
            int qty = Convert.ToInt32(row.Cells["Выдано"].Value);
            string status = row.Cells["Статус"].Value.ToString();

            if (MessageBox.Show($"Отменить заявку №{orderId}? Количество будет возвращено.", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using (var conn = new OleDbConnection(connectionString))
                {
                    conn.Open();
                    var tran = conn.BeginTransaction();
                    try
                    {
                        if (status == "Завершена")
                            new OleDbCommand($"UPDATE Cartridge SET Quantity = Quantity + {qty} WHERE ID = {cartridgeId}", conn, tran).ExecuteNonQuery();
                        new OleDbCommand($"UPDATE Orders SET [Status] = 'В работе' WHERE ID = {orderId}", conn, tran).ExecuteNonQuery();
                        tran.Commit();
                        MessageBox.Show("Заявка возвращена в работу.", "Успех");
                        RefreshCurrentOrders();
                        RefreshOrdersHistory();
                        RefreshStockGrid();
                    }
                    catch { tran.Rollback(); throw; }
                }
            }
        }

        private void RefreshStockGrid()
        {
            dgvStock.DataSource = null;
            dgvStock.Columns.Clear();

            string query = "SELECT ID, [Model name] AS [Модель картриджа], Quantity AS [Текущий остаток], has_Drum AS [Имеет фотобарабан] FROM Cartridge ORDER BY [Model name]";
            try
            {
                var dt = GetDataTable(query);
                dgvStock.DataSource = dt;
                if (dgvStock.Columns["ID"] != null) dgvStock.Columns["ID"].Visible = false;
                foreach (DataGridViewColumn col in dgvStock.Columns)
                    if (col.Name != "ID") col.ReadOnly = true;

                var qtyCol = new DataGridViewTextBoxColumn { Name = "IncomingQty", HeaderText = "Сколько завезли (шт.)", ValueType = typeof(int), ReadOnly = false };
                dgvStock.Columns.Add(qtyCol);
                foreach (DataGridViewRow r in dgvStock.Rows) r.Cells["IncomingQty"].Value = 0;

                AddButtonColumn(dgvStock, "AddAction", "Действие", "Добавить");
                dgvStock.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка загрузки склада:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void dgvStock_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgvStock.Columns[e.ColumnIndex].Name != "AddAction") return;
            var row = dgvStock.Rows[e.RowIndex];
            int id = Convert.ToInt32(row.Cells["ID"].Value);
            string model = row.Cells["Модель картриджа"].Value.ToString();
            if (!int.TryParse(row.Cells["IncomingQty"].Value?.ToString(), out int qty) || qty <= 0)
            {
                MessageBox.Show("Введите корректное число > 0.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using (var conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                new OleDbCommand($"UPDATE Cartridge SET Quantity = Quantity + {qty} WHERE ID = {id}", conn).ExecuteNonQuery();
                MessageBox.Show($"Добавлено {qty} шт. для {model}", "Успех");
                RefreshStockGrid();
            }
        }

        private void RefreshDepartmentsGrid()
        {
            try
            {
                string query = "SELECT ID, DepName FROM Departments ORDER BY DepName";
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    OleDbDataAdapter da = new OleDbDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvDepartments.DataSource = dt;
                    dgvDepartments.Columns["ID"].Visible = false;
                    dgvDepartments.Columns["DepName"].ReadOnly = true;
                    dgvDepartments.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }

                if (dgvDepartments.Rows.Count > 0)
                    dgvDepartments.CurrentCell = dgvDepartments.Rows[0].Cells["DepName"];
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отделов: {ex.Message}", "Ошибка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvDepartments_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvDepartments.CurrentRow == null) return;
            var val = dgvDepartments.CurrentRow.Cells["ID"].Value;
            if (val != null && val != DBNull.Value)
                RefreshPrintersForDepartment(Convert.ToInt32(val));
            else
                dgvPrinters.DataSource = null;
        }

        private void btnDepart_Click(object sender, EventArgs e)
        {
            string name = InputBox("Введите название отдела:", "Добавление отдела", "");
            if (string.IsNullOrWhiteSpace(name)) return;

            using (var conn = new OleDbConnection(connectionString))
            using (var cmd = new OleDbCommand("INSERT INTO Departments (DepName) VALUES (@name)", conn))
            {
                cmd.Parameters.AddWithValue("@name", name.Trim());
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            RefreshDepartmentsGrid();
            if (dgvDepartments.Rows.Count > 0)
                dgvDepartments.CurrentCell = dgvDepartments.Rows[dgvDepartments.Rows.Count - 1].Cells["DepName"];
        }

        private void dgvDepartments_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            if (e.Row.IsNewRow) return;
            if (MessageBox.Show($"Удалить отдел '{e.Row.Cells["DepName"].Value}'?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }

            int deptId = Convert.ToInt32(e.Row.Cells["ID"].Value);

            using (var conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {
                    new OleDbCommand($"DELETE FROM Department_Printers WHERE department_id = {deptId}", conn, tran).ExecuteNonQuery();
                    new OleDbCommand($"DELETE FROM Departments WHERE ID = {deptId}", conn, tran).ExecuteNonQuery();
                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }

            e.Cancel = true;
            RefreshDepartmentsGrid();
            if (dgvDepartments.Rows.Count == 0)
                dgvPrinters.DataSource = null;
        }

        private void RefreshPrintersForDepartment(int departmentId)
        {
            try
            {
                dgvPrinters.DataSource = null;
                dgvPrinters.Columns.Clear();

                var allPrinters = GetDataTable("SELECT ID, [Model printer] FROM Printers ORDER BY [Model printer]");
                var related = new List<int>();
                using (var conn = new OleDbConnection(connectionString))
                using (var cmd = new OleDbCommand("SELECT printer_id FROM Department_Printers WHERE department_id = @deptId", conn))
                {
                    cmd.Parameters.AddWithValue("@deptId", departmentId);
                    conn.Open();
                    using (var rdr = cmd.ExecuteReader())
                        while (rdr.Read()) related.Add(rdr.GetInt32(0));
                }

                var display = new DataTable();
                display.Columns.Add("ID", typeof(int));
                display.Columns.Add("Модель", typeof(string));
                display.Columns.Add("Привязан", typeof(bool));
                foreach (DataRow row in allPrinters.Rows)
                {
                    int id = Convert.ToInt32(row["ID"]);
                    display.Rows.Add(id, row["Model printer"].ToString(), related.Contains(id));
                }
                dgvPrinters.DataSource = display;

                if (dgvPrinters.Columns["ID"] != null) dgvPrinters.Columns["ID"].Visible = false;
                if (dgvPrinters.Columns["Модель"] != null) dgvPrinters.Columns["Модель"].ReadOnly = true;

                if (dgvPrinters.Columns["Привязан"] != null)
                {
                    var chk = new DataGridViewCheckBoxColumn { Name = "Привязан", HeaderText = "Привязан", DataPropertyName = "Привязан" };
                    dgvPrinters.Columns.Remove("Привязан");
                    dgvPrinters.Columns.Insert(0, chk);
                }
                dgvPrinters.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка загрузки принтеров: {ex.Message}", "Ошибка"); }
        }

        private void btnPrinter_Click(object sender, EventArgs e)
        {
            if (dgvDepartments.CurrentRow == null)
            {
                MessageBox.Show("Сначала выберите отдел, для которого добавляется принтер.");
                return;
            }

            string model = InputBox("Введите название принтера:", "Добавление принтера", "");
            if (string.IsNullOrWhiteSpace(model)) return;

            using (var conn = new OleDbConnection(connectionString))
            using (var cmd = new OleDbCommand("INSERT INTO Printers ([Model printer]) VALUES (@model)", conn))
            {
                cmd.Parameters.AddWithValue("@model", model.Trim());
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            int deptId = Convert.ToInt32(dgvDepartments.CurrentRow.Cells["ID"].Value);
            RefreshPrintersForDepartment(deptId);
        }

        private void dgvPrinters_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            if (e.Row.IsNewRow) return;
            if (MessageBox.Show($"Удалить принтер '{e.Row.Cells["Модель"].Value}'?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }

            int printerId = Convert.ToInt32(e.Row.Cells["ID"].Value);

            using (var conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {
                    new OleDbCommand($"DELETE FROM Department_Printers WHERE printer_id = {printerId}", conn, tran).ExecuteNonQuery();
                    new OleDbCommand($"DELETE FROM Printers WHERE ID = {printerId}", conn, tran).ExecuteNonQuery();
                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }

            e.Cancel = true;
            if (dgvDepartments.CurrentRow != null)
            {
                int deptId = Convert.ToInt32(dgvDepartments.CurrentRow.Cells["ID"].Value);
                RefreshPrintersForDepartment(deptId);
            }
        }

        private void btnSaveRelations_Click_1(object sender, EventArgs e)
        {
            if (dgvDepartments.CurrentRow == null) { MessageBox.Show("Выберите отдел."); return; }
            int deptId = Convert.ToInt32(dgvDepartments.CurrentRow.Cells["ID"].Value);
            string deptName = dgvDepartments.CurrentRow.Cells["DepName"].Value.ToString();

            var selected = new List<int>();
            foreach (DataGridViewRow row in dgvPrinters.Rows)
                if (row.Cells["Привязан"].Value != null && (bool)row.Cells["Привязан"].Value)
                    selected.Add(Convert.ToInt32(row.Cells["ID"].Value));

            using (var conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {
                    new OleDbCommand($"DELETE FROM Department_Printers WHERE department_id = {deptId}", conn, tran).ExecuteNonQuery();
                    foreach (int pid in selected)
                        new OleDbCommand($"INSERT INTO Department_Printers (department_id, printer_id) VALUES ({deptId}, {pid})", conn, tran).ExecuteNonQuery();
                    tran.Commit();
                    MessageBox.Show($"Связи для '{deptName}' обновлены.", "Успех");
                    RefreshPrintersForDepartment(deptId);
                }
                catch { tran.Rollback(); throw; }
            }
        }

        private void RefreshCartridgesGrid()
        {
            dgvCartridges.DataSource = null;
            dgvCartridges.Columns.Clear();

            string query = "SELECT ID, [Model name] AS [Модель] FROM Cartridge ORDER BY [Model name]";
            try
            {
                var dt = GetDataTable(query);
                dgvCartridges.DataSource = dt;
                if (dgvCartridges.Columns["ID"] != null) dgvCartridges.Columns["ID"].Visible = false;
                dgvCartridges.Columns["Модель"].ReadOnly = true;
                dgvCartridges.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки картриджей: {ex.Message}", "Ошибка");
            }
        }

        private void RefreshCartridgesForPrinter(int printerId)
        {
            dgvCartridges.DataSource = null;
            dgvCartridges.Columns.Clear();

            int? currentCartridgeId = null;
            using (var conn = new OleDbConnection(connectionString))
            using (var cmd = new OleDbCommand("SELECT [Cartridge ID] FROM Printers WHERE ID = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", printerId);
                conn.Open();
                var result = cmd.ExecuteScalar();
                if (result != DBNull.Value) currentCartridgeId = Convert.ToInt32(result);
            }

            string query = "SELECT ID, [Model name] AS [Модель] FROM Cartridge ORDER BY [Model name]";
            var dtAll = GetDataTable(query);

            DataTable display = new DataTable();
            display.Columns.Add("ID", typeof(int));
            display.Columns.Add("Модель", typeof(string));
            display.Columns.Add("Назначен", typeof(bool));

            foreach (DataRow row in dtAll.Rows)
            {
                int id = Convert.ToInt32(row["ID"]);
                string model = row["Модель"].ToString();
                bool isAssigned = (currentCartridgeId.HasValue && currentCartridgeId.Value == id);
                display.Rows.Add(id, model, isAssigned);
            }

            dgvCartridges.DataSource = display;
            if (dgvCartridges.Columns["ID"] != null) dgvCartridges.Columns["ID"].Visible = false;

            if (dgvCartridges.Columns["Назначен"] != null)
            {
                var chk = new DataGridViewCheckBoxColumn
                {
                    Name = "Назначен",
                    HeaderText = "Назначен",
                    DataPropertyName = "Назначен"
                };
                dgvCartridges.Columns.Remove("Назначен");
                dgvCartridges.Columns.Insert(0, chk);
            }

            dgvCartridges.Columns["Модель"].ReadOnly = true;
            dgvCartridges.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void btnCartridge_Click(object sender, EventArgs e)
        {
            string model = InputBox("Введите название картриджа:", "Добавление картриджа", "");
            if (string.IsNullOrWhiteSpace(model)) return;

            using (var conn = new OleDbConnection(connectionString))
            using (var cmd = new OleDbCommand("INSERT INTO Cartridge ([Model name], Quantity, has_Drum) VALUES (@model, 0, false)", conn))
            {
                cmd.Parameters.AddWithValue("@model", model.Trim());
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            RefreshCartridgesGrid();
            if (dgvPrinters.CurrentRow != null)
            {
                int printerId = Convert.ToInt32(dgvPrinters.CurrentRow.Cells["ID"].Value);
                RefreshCartridgesForPrinter(printerId);
            }
        }

        private void dgvPrinters_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPrinters.CurrentRow == null) return;
            var val = dgvPrinters.CurrentRow.Cells["ID"].Value;
            if (val != null && val != DBNull.Value)
            {
                int printerId = Convert.ToInt32(val);
                RefreshCartridgesForPrinter(printerId);
            }
            else
            {
                dgvCartridges.DataSource = null;
            }
        }

        private void btnSaveCartridge_Click(object sender, EventArgs e)
        {
            if (dgvPrinters.CurrentRow == null)
            {
                MessageBox.Show("Выберите принтер.");
                return;
            }

            int printerId = Convert.ToInt32(dgvPrinters.CurrentRow.Cells["ID"].Value);

            int? selectedCartridgeId = null;
            foreach (DataGridViewRow row in dgvCartridges.Rows)
            {
                if (row.Cells["Назначен"].Value != null && (bool)row.Cells["Назначен"].Value)
                {
                    selectedCartridgeId = Convert.ToInt32(row.Cells["ID"].Value);
                    break;
                }
            }

            using (var conn = new OleDbConnection(connectionString))
            using (var cmd = new OleDbCommand("UPDATE Printers SET [Cartridge ID] = @cartId WHERE ID = @printerId", conn))
            {
                cmd.Parameters.AddWithValue("@cartId", (object)selectedCartridgeId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@printerId", printerId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Картридж для принтера назначен.", "Успех");
            RefreshCartridgesForPrinter(printerId);
        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {
            DataTable dt = GetCurrentOrdersIds();
            List<int> currentIds = new List<int>();
            foreach (DataRow row in dt.Rows)
                currentIds.Add(Convert.ToInt32(row["ID"]));

            lock (orderLock)
            {
                var newIds = currentIds.Except(previousOrderIds).ToList();
                if (newIds.Count > 0)
                {
                    string message = $"Поступила новая заявка! ID: {string.Join(", ", newIds)}";
                    ShowWindowsNotification("Новая заявка", message);
                }
                previousOrderIds = currentIds;
            }

            RefreshCurrentOrders();
            RefreshOrdersHistory();
            Text = $"Панель оператора — Последнее фоновое обновление: {DateTime.Now.ToLongTimeString()}";
        }

        private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 2)
            {
                timer1.Stop();
                RefreshStockGrid();
                Text = "Панель оператора — Работа со складом (Автообновление приостановлено)";
            }
            else
            {
                timer1.Start();
            }
        }

        private void dgvCartridges_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            if (e.Row.IsNewRow) return;

            int cartId = Convert.ToInt32(e.Row.Cells["ID"].Value);

            using (var conn = new OleDbConnection(connectionString))
            using (var cmd = new OleDbCommand("SELECT COUNT(*) FROM Printers WHERE [Cartridge ID] = @cartId", conn))
            {
                cmd.Parameters.AddWithValue("@cartId", cartId);
                conn.Open();
                int count = (int)cmd.ExecuteScalar();
                if (count > 0)
                {
                    MessageBox.Show("Невозможно удалить картридж, он назначен на принтер(ы). Сначала отвяжите его.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Cancel = true;
                    return;
                }
            }

            if (MessageBox.Show($"Удалить картридж '{e.Row.Cells["Модель"].Value}'?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }

            using (var conn = new OleDbConnection(connectionString))
            using (var cmd = new OleDbCommand("DELETE FROM Cartridge WHERE ID = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", cartId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            e.Cancel = true;
            RefreshCartridgesGrid();
            if (dgvPrinters.CurrentRow != null)
            {
                int printerId = Convert.ToInt32(dgvPrinters.CurrentRow.Cells["ID"].Value);
                RefreshCartridgesForPrinter(printerId);
            }
        }

        private DataTable GetCurrentOrdersIds()
        {
            string query = "SELECT ID FROM Orders WHERE [Status] = 'В работе'";
            return GetDataTable(query);
        }
    }
}