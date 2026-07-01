namespace OM_CartridgeDesk
{
    partial class OpClient
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.dgvCurrentOrders = new System.Windows.Forms.DataGridView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dgvOrdersHistory = new System.Windows.Forms.DataGridView();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.dgvStock = new System.Windows.Forms.DataGridView();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.btnSaveCartridge = new System.Windows.Forms.Button();
            this.btnCartridge = new System.Windows.Forms.Button();
            this.btnPrinter = new System.Windows.Forms.Button();
            this.btnDepart = new System.Windows.Forms.Button();
            this.dgvCartridges = new System.Windows.Forms.DataGridView();
            this.btnSaveRelations = new System.Windows.Forms.Button();
            this.dgvPrinters = new System.Windows.Forms.DataGridView();
            this.dgvDepartments = new System.Windows.Forms.DataGridView();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCurrentOrders)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOrdersHistory)).BeginInit();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStock)).BeginInit();
            this.tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCartridges)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPrinters)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDepartments)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1438, 587);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.TabControl1_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.dgvCurrentOrders);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1430, 561);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Текущие заявки";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // dgvCurrentOrders
            // 
            this.dgvCurrentOrders.AllowUserToAddRows = false;
            this.dgvCurrentOrders.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCurrentOrders.Location = new System.Drawing.Point(3, 6);
            this.dgvCurrentOrders.Name = "dgvCurrentOrders";
            this.dgvCurrentOrders.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvCurrentOrders.Size = new System.Drawing.Size(1421, 549);
            this.dgvCurrentOrders.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.dgvOrdersHistory);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1430, 561);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "История заявок";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // dgvOrdersHistory
            // 
            this.dgvOrdersHistory.AllowUserToAddRows = false;
            this.dgvOrdersHistory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvOrdersHistory.Location = new System.Drawing.Point(2, 6);
            this.dgvOrdersHistory.Name = "dgvOrdersHistory";
            this.dgvOrdersHistory.ReadOnly = true;
            this.dgvOrdersHistory.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvOrdersHistory.Size = new System.Drawing.Size(1422, 549);
            this.dgvOrdersHistory.TabIndex = 1;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.dgvStock);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(1430, 561);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Склад";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // dgvStock
            // 
            this.dgvStock.AllowUserToAddRows = false;
            this.dgvStock.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvStock.Location = new System.Drawing.Point(2, 6);
            this.dgvStock.Name = "dgvStock";
            this.dgvStock.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvStock.Size = new System.Drawing.Size(1425, 549);
            this.dgvStock.TabIndex = 2;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.btnSaveCartridge);
            this.tabPage4.Controls.Add(this.btnCartridge);
            this.tabPage4.Controls.Add(this.btnPrinter);
            this.tabPage4.Controls.Add(this.btnDepart);
            this.tabPage4.Controls.Add(this.dgvCartridges);
            this.tabPage4.Controls.Add(this.btnSaveRelations);
            this.tabPage4.Controls.Add(this.dgvPrinters);
            this.tabPage4.Controls.Add(this.dgvDepartments);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(1430, 561);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Отделы";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // btnSaveCartridge
            // 
            this.btnSaveCartridge.Location = new System.Drawing.Point(1221, 300);
            this.btnSaveCartridge.Name = "btnSaveCartridge";
            this.btnSaveCartridge.Size = new System.Drawing.Size(165, 23);
            this.btnSaveCartridge.TabIndex = 7;
            this.btnSaveCartridge.Text = "Назначить картридж";
            this.btnSaveCartridge.UseVisualStyleBackColor = true;
            // 
            // btnCartridge
            // 
            this.btnCartridge.Location = new System.Drawing.Point(785, 510);
            this.btnCartridge.Name = "btnCartridge";
            this.btnCartridge.Size = new System.Drawing.Size(385, 36);
            this.btnCartridge.TabIndex = 6;
            this.btnCartridge.Text = "Добавить картридж";
            this.btnCartridge.UseVisualStyleBackColor = true;
            this.btnCartridge.Click += new System.EventHandler(this.button3_Click);
            // 
            // btnPrinter
            // 
            this.btnPrinter.Location = new System.Drawing.Point(394, 510);
            this.btnPrinter.Name = "btnPrinter";
            this.btnPrinter.Size = new System.Drawing.Size(385, 36);
            this.btnPrinter.TabIndex = 5;
            this.btnPrinter.Text = "Добавить принтер";
            this.btnPrinter.UseVisualStyleBackColor = true;
            // 
            // btnDepart
            // 
            this.btnDepart.Location = new System.Drawing.Point(3, 510);
            this.btnDepart.Name = "btnDepart";
            this.btnDepart.Size = new System.Drawing.Size(385, 36);
            this.btnDepart.TabIndex = 4;
            this.btnDepart.Text = "Добавить отдел";
            this.btnDepart.UseVisualStyleBackColor = true;
            // 
            // dgvCartridges
            // 
            this.dgvCartridges.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCartridges.Location = new System.Drawing.Point(785, 3);
            this.dgvCartridges.Name = "dgvCartridges";
            this.dgvCartridges.Size = new System.Drawing.Size(385, 504);
            this.dgvCartridges.TabIndex = 3;
            // 
            // btnSaveRelations
            // 
            this.btnSaveRelations.Location = new System.Drawing.Point(1245, 271);
            this.btnSaveRelations.Name = "btnSaveRelations";
            this.btnSaveRelations.Size = new System.Drawing.Size(118, 23);
            this.btnSaveRelations.TabIndex = 2;
            this.btnSaveRelations.Text = "Сохранить связи";
            this.btnSaveRelations.UseVisualStyleBackColor = true;
            this.btnSaveRelations.Click += new System.EventHandler(this.btnSaveRelations_Click_1);
            // 
            // dgvPrinters
            // 
            this.dgvPrinters.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPrinters.Location = new System.Drawing.Point(394, 0);
            this.dgvPrinters.Name = "dgvPrinters";
            this.dgvPrinters.Size = new System.Drawing.Size(385, 504);
            this.dgvPrinters.TabIndex = 1;
            // 
            // dgvDepartments
            // 
            this.dgvDepartments.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDepartments.Location = new System.Drawing.Point(3, 0);
            this.dgvDepartments.Name = "dgvDepartments";
            this.dgvDepartments.Size = new System.Drawing.Size(385, 504);
            this.dgvDepartments.TabIndex = 0;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 5000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick_1);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notifyIcon1.Text = "notifyIcon1";
            this.notifyIcon1.Visible = true;
            // 
            // OpClient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1450, 599);
            this.Controls.Add(this.tabControl1);
            this.Name = "OpClient";
            this.Text = "Form2";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCurrentOrders)).EndInit();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvOrdersHistory)).EndInit();
            this.tabPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvStock)).EndInit();
            this.tabPage4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCartridges)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPrinters)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDepartments)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.DataGridView dgvCurrentOrders;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.DataGridView dgvOrdersHistory;
        private System.Windows.Forms.DataGridView dgvStock;
        private System.Windows.Forms.DataGridView dgvDepartments;
        private System.Windows.Forms.DataGridView dgvPrinters;
        private System.Windows.Forms.Button btnSaveRelations;
        private System.Windows.Forms.DataGridView dgvCartridges;
        private System.Windows.Forms.Button btnCartridge;
        private System.Windows.Forms.Button btnPrinter;
        private System.Windows.Forms.Button btnDepart;
        private System.Windows.Forms.Button btnSaveCartridge;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
    }
}