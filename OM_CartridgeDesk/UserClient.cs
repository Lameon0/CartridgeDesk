using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Data.OleDb;
using System.Security.Principal;
using System.DirectoryServices.AccountManagement;

namespace OM_CartridgeDesk
{
    public partial class UserClient : Form
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

        private readonly string[] adminGroupNames = {
            "Администраторы",
            "ВрИО"
        };

        private int currentCartridgeId = 0;
        private int currentDrumId = 0;

        public UserClient()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // ==========================================
            // ЧАСТЬ 1. ПРОВЕРКА ПРАВ ЧЕРЕЗ ACTIVE DIRECTORY
            // ==========================================

            string currentWindowsUser = WindowsIdentity.GetCurrent().Name;
            this.Text = $"Заказ картриджей — Вы вошли как: {currentWindowsUser}";

            bool isAdmin = IsUserInAnyAdminGroup();
            btnAdminPanel.Visible = isAdmin;

            // ==========================================
            // ЧАСТЬ 2. ЗАГРУЗКА СПИСКА ОТДЕЛОВ ИЗ ACCESS
            // ==========================================

            string query = "SELECT id, DepName FROM Departments ORDER BY DepName ASC";

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                try
                {
                    OleDbDataAdapter da = new OleDbDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    comboBoxDepartments.DataSource = dt;
                    comboBoxDepartments.DisplayMember = "DepName";
                    comboBoxDepartments.ValueMember = "id";
                    comboBoxDepartments.SelectedIndex = -1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки базы данных:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private bool IsUserInAnyAdminGroup()
        {
            try
            {
                string domain = Environment.UserDomainName;
                string userName = Environment.UserName;

                using (PrincipalContext context = new PrincipalContext(ContextType.Domain, domain))
                using (UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, userName))
                {
                    if (user == null) return false;

                    PrincipalSearchResult<Principal> groups = user.GetGroups();
                    foreach (Principal group in groups)
                    {
                        foreach (string adminGroup in adminGroupNames)
                        {
                            if (group.Name.Equals(adminGroup, StringComparison.OrdinalIgnoreCase))
                                return true;
                        }
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private void Label1_Click(object sender, EventArgs e) { }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxDepartments.SelectedValue == null || comboBoxDepartments.SelectedIndex == -1)
            {
                comboBoxPrinters.Enabled = false;
                comboBoxPrinters.DataSource = null;
                return;
            }

            if (comboBoxDepartments.SelectedValue is DataRowView) return;

            int selectedDepartmentId = Convert.ToInt32(comboBoxDepartments.SelectedValue);

            string query = @"SELECT P.ID, P.[Model printer] 
                     FROM Printers P 
                     INNER JOIN Department_Printers DP ON P.ID = DP.printer_id 
                     WHERE DP.department_id = @deptId 
                     ORDER BY P.[Model printer] ASC";

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                try
                {
                    OleDbCommand cmd = new OleDbCommand(query, conn);
                    cmd.Parameters.AddWithValue("@deptId", selectedDepartmentId);

                    OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        comboBoxPrinters.DataSource = dt;
                        comboBoxPrinters.DisplayMember = "Model printer";
                        comboBoxPrinters.ValueMember = "ID";

                        comboBoxPrinters.SelectedIndex = -1;
                        comboBoxPrinters.Enabled = true;
                    }
                    else
                    {
                        comboBoxPrinters.Enabled = false;
                        comboBoxPrinters.DataSource = null;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки принтеров:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void Panel1_Paint(object sender, PaintEventArgs e) { }

        private void Label7_Click(object sender, EventArgs e) { }

        private void Button1_Click(object sender, EventArgs e)
        {
            OpClient opForm = new OpClient();
            this.Hide();
            opForm.ShowDialog();
            this.Show();
        }

        private void ComboBoxPrinters_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxPrinters.SelectedValue == null || comboBoxPrinters.SelectedIndex == -1)
            {
                lblCartridgeModel.Text = "—";
                lblStockCount.Text = "0 шт.";
                chkNeedDrum.Enabled = false;
                chkNeedDrum.Checked = false;
                currentCartridgeId = 0;
                currentDrumId = 0;
                return;
            }

            if (comboBoxPrinters.SelectedValue is DataRowView) return;

            int selectedPrinterId = Convert.ToInt32(comboBoxPrinters.SelectedValue);

            string query = @"SELECT C.[Model name], C.Quantity, C.has_Drum, P.[Cartridge ID], P.Drum_ID 
                     FROM Cartridge C 
                     INNER JOIN Printers P ON C.ID = P.[Cartridge ID] 
                     WHERE P.ID = @printerId";

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                try
                {
                    OleDbCommand cmd = new OleDbCommand(query, conn);
                    cmd.Parameters.AddWithValue("@printerId", selectedPrinterId);

                    conn.Open();
                    using (OleDbDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string cartridgeModel = reader["Model name"].ToString();
                            int stockCount = Convert.ToInt32(reader["Quantity"]);

                            currentCartridgeId = Convert.ToInt32(reader["Cartridge ID"]);

                            if (reader["Drum_ID"] != DBNull.Value)
                                currentDrumId = Convert.ToInt32(reader["Drum_ID"]);
                            else
                                currentDrumId = 0;

                            bool hasDrumInCartridge = Convert.ToBoolean(reader["has_Drum"]);

                            chkNeedDrum.Enabled = !hasDrumInCartridge;
                            if (hasDrumInCartridge)
                                chkNeedDrum.Checked = false;

                            lblCartridgeModel.Text = cartridgeModel;
                            lblStockCount.Text = $"{stockCount} шт.";
                            lblStockCount.ForeColor = stockCount == 0 ? System.Drawing.Color.Red : System.Drawing.Color.Black;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при получении данных о картридже:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void Button1_Click_1(object sender, EventArgs e)
        {
            if (comboBoxDepartments.SelectedIndex == -1 || comboBoxPrinters.SelectedIndex == -1)
            {
                MessageBox.Show("Пожалуйста, выберите ваш отдел и модель принтера!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int deptId = Convert.ToInt32(comboBoxDepartments.SelectedValue);
            int printerId = Convert.ToInt32(comboBoxPrinters.SelectedValue);
            int requestedQty = Convert.ToInt32(numericUpDownQty.Value);
            string currentUser = WindowsIdentity.GetCurrent().Name;

            int currentStock = 0;
            string stockText = lblStockCount.Text.Replace(" шт.", "");
            int.TryParse(stockText, out currentStock);

            if (currentStock < requestedQty)
            {
                MessageBox.Show($"Недостаточно картриджей на складе! Доступно: {currentStock} шт.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string insertOrderQuery = @"INSERT INTO Orders (Order_date, Printer_ID, Department_ID, CartridgeValue, UserName, [Status], CartridgeID) 
                                       VALUES (Now(), @printerId, @deptId, @qty, @user, 'В работе', @cartId)";

                    OleDbCommand cmd1 = new OleDbCommand(insertOrderQuery, conn);
                    cmd1.Parameters.AddWithValue("@printerId", printerId);
                    cmd1.Parameters.AddWithValue("@deptId", deptId);
                    cmd1.Parameters.AddWithValue("@qty", requestedQty);
                    cmd1.Parameters.AddWithValue("@user", currentUser);
                    cmd1.Parameters.AddWithValue("@cartId", currentCartridgeId);
                    cmd1.ExecuteNonQuery();

                    if (chkNeedDrum.Checked && currentDrumId > 0)
                    {
                        OleDbCommand cmd2 = new OleDbCommand(insertOrderQuery, conn);
                        cmd2.Parameters.AddWithValue("@printerId", printerId);
                        cmd2.Parameters.AddWithValue("@deptId", deptId);
                        cmd2.Parameters.AddWithValue("@qty", 1);
                        cmd2.Parameters.AddWithValue("@user", currentUser);
                        cmd2.Parameters.AddWithValue("@cartId", currentDrumId);
                        cmd2.ExecuteNonQuery();
                    }

                    MessageBox.Show("Заявка успешно отправлена оператору!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    comboBoxPrinters.SelectedIndex = -1;
                    comboBoxDepartments.SelectedIndex = -1;
                    chkNeedDrum.Checked = false;
                    numericUpDownQty.Value = 1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении заявки в базу данных:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}