namespace Labels
{
    partial class Form1
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle13 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle14 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle15 = new System.Windows.Forms.DataGridViewCellStyle();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.StatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.stockGridView = new System.Windows.Forms.DataGridView();
            this.StockNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Description = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Price = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Cost = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Added = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.stockTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.noResultsLabel = new System.Windows.Forms.Label();
            this.allTodayLabel = new System.Windows.Forms.LinkLabel();
            this.labelComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.line1Box = new System.Windows.Forms.TextBox();
            this.line2Box = new System.Windows.Forms.TextBox();
            this.line3Box = new System.Windows.Forms.TextBox();
            this.printButton = new System.Windows.Forms.Button();
            this.coordinatesLabel = new System.Windows.Forms.Label();
            this.copiesUpDown = new System.Windows.Forms.NumericUpDown();
            this.copiesLabel = new System.Windows.Forms.Label();
            this.priceBox = new System.Windows.Forms.TextBox();
            this.priceLabel = new System.Windows.Forms.Label();
            this.bwTestConn = new System.ComponentModel.BackgroundWorker();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.stockGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.copiesUpDown)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 564);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1370, 22);
            this.statusStrip1.TabIndex = 5;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // StatusLabel1
            // 
            this.StatusLabel1.Name = "StatusLabel1";
            this.StatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // stockGridView
            // 
            this.stockGridView.AllowUserToAddRows = false;
            this.stockGridView.AllowUserToDeleteRows = false;
            this.stockGridView.AllowUserToResizeRows = false;
            this.stockGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.stockGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.StockNum,
            this.Description,
            this.Price,
            this.Cost,
            this.Added});
            this.stockGridView.Location = new System.Drawing.Point(12, 86);
            this.stockGridView.MultiSelect = false;
            this.stockGridView.Name = "stockGridView";
            this.stockGridView.RowHeadersVisible = false;
            this.stockGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.stockGridView.Size = new System.Drawing.Size(724, 353);
            this.stockGridView.TabIndex = 2;
            this.stockGridView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.stockGridView_CellEndEdit);
            this.stockGridView.SelectionChanged += new System.EventHandler(this.stockGridView_SelectionChanged);
            // 
            // StockNum
            // 
            this.StockNum.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.StockNum.HeaderText = "Stock #";
            this.StockNum.Name = "StockNum";
            this.StockNum.Width = 70;
            // 
            // Description
            // 
            this.Description.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Description.HeaderText = "Description";
            this.Description.Name = "Description";
            this.Description.Width = 85;
            // 
            // Price
            // 
            this.Price.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle13.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle13.Format = "N2";
            this.Price.DefaultCellStyle = dataGridViewCellStyle13;
            this.Price.HeaderText = "Price";
            this.Price.Name = "Price";
            this.Price.Width = 56;
            // 
            // Cost
            // 
            this.Cost.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle14.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle14.Format = "N2";
            this.Cost.DefaultCellStyle = dataGridViewCellStyle14;
            this.Cost.HeaderText = "Cost";
            this.Cost.Name = "Cost";
            this.Cost.Width = 53;
            // 
            // Added
            // 
            this.Added.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle15.Format = "d";
            dataGridViewCellStyle15.NullValue = null;
            this.Added.DefaultCellStyle = dataGridViewCellStyle15;
            this.Added.HeaderText = "Added";
            this.Added.Name = "Added";
            this.Added.Width = 63;
            // 
            // stockTextBox
            // 
            this.stockTextBox.Location = new System.Drawing.Point(12, 60);
            this.stockTextBox.Name = "stockTextBox";
            this.stockTextBox.Size = new System.Drawing.Size(100, 20);
            this.stockTextBox.TabIndex = 1;
            this.stockTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.stockTextBox_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(99, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Search by Stock #:";
            // 
            // noResultsLabel
            // 
            this.noResultsLabel.AutoSize = true;
            this.noResultsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.noResultsLabel.Location = new System.Drawing.Point(118, 63);
            this.noResultsLabel.Name = "noResultsLabel";
            this.noResultsLabel.Size = new System.Drawing.Size(57, 13);
            this.noResultsLabel.TabIndex = 9;
            this.noResultsLabel.Text = "No results.";
            this.noResultsLabel.Visible = false;
            // 
            // allTodayLabel
            // 
            this.allTodayLabel.AutoSize = true;
            this.allTodayLabel.Location = new System.Drawing.Point(120, 44);
            this.allTodayLabel.Name = "allTodayLabel";
            this.allTodayLabel.Size = new System.Drawing.Size(100, 13);
            this.allTodayLabel.TabIndex = 10;
            this.allTodayLabel.TabStop = true;
            this.allTodayLabel.Text = "Find all pulled today";
            this.allTodayLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.allTodayLabel_LinkClicked);
            // 
            // labelComboBox
            // 
            this.labelComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.labelComboBox.FormattingEnabled = true;
            this.labelComboBox.Location = new System.Drawing.Point(748, 59);
            this.labelComboBox.Name = "labelComboBox";
            this.labelComboBox.Size = new System.Drawing.Size(143, 21);
            this.labelComboBox.TabIndex = 3;
            this.labelComboBox.SelectedIndexChanged += new System.EventHandler(this.labelComboBox_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(745, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "Label Type:";
            // 
            // line1Box
            // 
            this.line1Box.Location = new System.Drawing.Point(748, 86);
            this.line1Box.Name = "line1Box";
            this.line1Box.Size = new System.Drawing.Size(216, 20);
            this.line1Box.TabIndex = 4;
            this.line1Box.Tag = "30";
            this.line1Box.TextChanged += new System.EventHandler(this.line1Box_TextChanged);
            this.line1Box.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.line1Box_KeyPress);
            // 
            // line2Box
            // 
            this.line2Box.Location = new System.Drawing.Point(748, 112);
            this.line2Box.Name = "line2Box";
            this.line2Box.Size = new System.Drawing.Size(216, 20);
            this.line2Box.TabIndex = 5;
            this.line2Box.Tag = "30";
            this.line2Box.TextChanged += new System.EventHandler(this.line1Box_TextChanged);
            this.line2Box.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.line1Box_KeyPress);
            // 
            // line3Box
            // 
            this.line3Box.Location = new System.Drawing.Point(748, 138);
            this.line3Box.Name = "line3Box";
            this.line3Box.Size = new System.Drawing.Size(216, 20);
            this.line3Box.TabIndex = 6;
            this.line3Box.Tag = "30";
            this.line3Box.TextChanged += new System.EventHandler(this.line1Box_TextChanged);
            this.line3Box.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.line1Box_KeyPress);
            // 
            // printButton
            // 
            this.printButton.Location = new System.Drawing.Point(1130, 138);
            this.printButton.Name = "printButton";
            this.printButton.Size = new System.Drawing.Size(75, 23);
            this.printButton.TabIndex = 9;
            this.printButton.Text = "Print";
            this.printButton.UseVisualStyleBackColor = true;
            this.printButton.Click += new System.EventHandler(this.printButton_Click);
            // 
            // coordinatesLabel
            // 
            this.coordinatesLabel.AutoSize = true;
            this.coordinatesLabel.Location = new System.Drawing.Point(745, 163);
            this.coordinatesLabel.Name = "coordinatesLabel";
            this.coordinatesLabel.Size = new System.Drawing.Size(0, 13);
            this.coordinatesLabel.TabIndex = 18;
            // 
            // copiesUpDown
            // 
            this.copiesUpDown.Location = new System.Drawing.Point(1048, 139);
            this.copiesUpDown.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.copiesUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.copiesUpDown.Name = "copiesUpDown";
            this.copiesUpDown.Size = new System.Drawing.Size(76, 20);
            this.copiesUpDown.TabIndex = 8;
            this.copiesUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // copiesLabel
            // 
            this.copiesLabel.AutoSize = true;
            this.copiesLabel.Location = new System.Drawing.Point(1045, 122);
            this.copiesLabel.Name = "copiesLabel";
            this.copiesLabel.Size = new System.Drawing.Size(42, 13);
            this.copiesLabel.TabIndex = 21;
            this.copiesLabel.Text = "Copies:";
            // 
            // priceBox
            // 
            this.priceBox.Location = new System.Drawing.Point(1048, 86);
            this.priceBox.Name = "priceBox";
            this.priceBox.Size = new System.Drawing.Size(100, 20);
            this.priceBox.TabIndex = 7;
            this.priceBox.TextChanged += new System.EventHandler(this.priceBox_TextChanged);
            this.priceBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.priceBox_KeyPress);
            // 
            // priceLabel
            // 
            this.priceLabel.AutoSize = true;
            this.priceLabel.Location = new System.Drawing.Point(1045, 70);
            this.priceLabel.Name = "priceLabel";
            this.priceLabel.Size = new System.Drawing.Size(34, 13);
            this.priceLabel.TabIndex = 23;
            this.priceLabel.Text = "Price:";
            // 
            // bwTestConn
            // 
            this.bwTestConn.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bwTestConn_DoWork);
            this.bwTestConn.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bwTestConn_RunWorkerCompleted);
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Location = new System.Drawing.Point(748, 183);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(458, 256);
            this.panel1.TabIndex = 24;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(-1, -1);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(457, 254);
            this.pictureBox1.TabIndex = 14;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Tag = "0";
            this.pictureBox1.MouseLeave += new System.EventHandler(this.pictureBox1_MouseLeave);
            this.pictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseMove);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1370, 586);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.priceLabel);
            this.Controls.Add(this.priceBox);
            this.Controls.Add(this.copiesLabel);
            this.Controls.Add(this.copiesUpDown);
            this.Controls.Add(this.coordinatesLabel);
            this.Controls.Add(this.printButton);
            this.Controls.Add(this.line3Box);
            this.Controls.Add(this.line2Box);
            this.Controls.Add(this.line1Box);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.labelComboBox);
            this.Controls.Add(this.allTodayLabel);
            this.Controls.Add(this.noResultsLabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.stockTextBox);
            this.Controls.Add(this.stockGridView);
            this.Controls.Add(this.statusStrip1);
            this.Name = "Form1";
            this.Text = "Labels";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.stockGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.copiesUpDown)).EndInit();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel1;
        private System.Windows.Forms.DataGridView stockGridView;
        private System.Windows.Forms.TextBox stockTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label noResultsLabel;
        private System.Windows.Forms.LinkLabel allTodayLabel;
        private System.Windows.Forms.ComboBox labelComboBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridViewTextBoxColumn StockNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn Description;
        private System.Windows.Forms.DataGridViewTextBoxColumn Price;
        private System.Windows.Forms.DataGridViewTextBoxColumn Cost;
        private System.Windows.Forms.DataGridViewTextBoxColumn Added;
        private System.Windows.Forms.TextBox line1Box;
        private System.Windows.Forms.TextBox line2Box;
        private System.Windows.Forms.TextBox line3Box;
        private System.Windows.Forms.Button printButton;
        private System.Windows.Forms.Label coordinatesLabel;
        private System.Windows.Forms.NumericUpDown copiesUpDown;
        private System.Windows.Forms.Label copiesLabel;
        private System.Windows.Forms.TextBox priceBox;
        private System.Windows.Forms.Label priceLabel;
        private System.ComponentModel.BackgroundWorker bwTestConn;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}

