using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.OleDb;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing.Text;
using Com.SharpZebra.Commands;
using Com.SharpZebra.Printing;
using GenCode128;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.Media;

namespace Labels
{
    public partial class Form1 : Form
    {
        private string DataSource = "";
        private string DataProvider = "";
        private string labelPrinter = "ZD";
        private int printSpeed = 2;
        private int printDarkness = 30;
        private SplashForm splashForm;
        LabelRenderer labelRenderer;
        List<XElement> labelTemplates = new List<XElement>();
        List<TextBox> textBoxes = new List<TextBox>();

        public Form1()
        {
            InitializeComponent();
            textBoxes.AddRange(new[] { line1Box, line2Box, line3Box });
            splashForm = new SplashForm();
            splashForm.Show();
            PrivateFontCollection fonts = new PrivateFontCollection();
            fonts.AddFontFile("free3of9.ttf");
            labelRenderer = new LabelRenderer(fonts, textBoxes);
            XElement configRoot = XElement.Load("config.txt");
            LoadLabelTemplates(configRoot);
            LoadConfigFields(configRoot);
            bwTestConn.RunWorkerAsync();
        }

        private void LoadConfigFields(XElement configRoot)
        {
            DataSource = configRoot.Element("dbfolder").Value;
            labelPrinter = configRoot.Element("printername").Value;
            DataProvider = configRoot.Element("dataprovider").Value;
            if (configRoot.Element("printspeed") != null)
                printSpeed = Convert.ToInt32(configRoot.Element("printspeed").Value);
            if (configRoot.Element("printdarkness") != null)
                printDarkness = Convert.ToInt32(configRoot.Element("printdarkness").Value);
        }

        private void LoadLabelTemplates(XElement configRoot)
        {
            DirectoryInfo di;
            if (configRoot.Element("labelfolder") != null)
                di = new DirectoryInfo(configRoot.Element("labelfolder").Value);
            else
                di = new DirectoryInfo(Path.GetDirectoryName(Application.ExecutablePath));
            FileInfo[] fi = di.GetFiles("*.lbl");
            foreach (FileInfo f in fi)
            {
                XElement labelTemplate = XElement.Load(f.FullName);
                labelComboBox.Items.Add(new LabelTemplate(labelTemplate));
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (labelComboBox.Items.Count > 0)
                labelComboBox.SelectedIndex = 0;            
        }

        private void DoSearch()
        {
            try
            {
                String searchText = stockTextBox.Text;
                stockGridView.Rows.Clear();
                OleDbConnection conn = new OleDbConnection { ConnectionString = String.Format("Provider={0};Data Source={1}\\pawndata;Extended Properties=Paradox 4.x;", DataProvider, DataSource) };
                conn.Open();
                OleDbCommand getStock = new OleDbCommand("SELECT [Stock #], Description, Price, Cost, Added FROM STOCK WHERE [Stock #] LIKE :stocknum", conn);
                getStock.Parameters.Add("stocknum", OleDbType.VarChar, 15);
                getStock.Prepare();
                getStock.Parameters["stocknum"].Value = searchText + "%";
                int results = 0;
                using (OleDbDataReader myreader = getStock.ExecuteReader())
                {
                    while (myreader.Read())
                    {
                        results++;
                        stockGridView.Rows.Add(myreader[0], myreader[1], myreader[2], myreader[3], myreader[4]);
                    }
                }
                if (results == 0 && searchText.Length > 1) // workaround for exact match
                {
                    getStock.Parameters[0].Value = searchText.Substring(0, searchText.Length - 1) + "%";
                    using (OleDbDataReader myreader = getStock.ExecuteReader())
                    {
                        while (myreader.Read())
                        {
                            if (myreader[0].ToString().ToUpper() == searchText.ToUpper())
                            {
                                results++;
                                stockGridView.Rows.Add(myreader[0], myreader[1], myreader[2], myreader[3], myreader[4]);
                            }
                        }
                    }
                }
                noResultsLabel.Visible = results == 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void stockTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            noResultsLabel.Visible = false;
            if (e.KeyChar == '\r')
            {
                DoSearch();
                e.Handled = true;
            }
        }

        private void allTodayLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                stockGridView.Rows.Clear();
                OleDbConnection conn = new OleDbConnection { ConnectionString = String.Format("Provider={0};Data Source={1}\\pawndata;Extended Properties=Paradox 4.x;", DataProvider, DataSource) };
                conn.Open();
                OleDbCommand getStock = new OleDbCommand("SELECT [Stock #], Description, Price, Cost, Added FROM STOCK WHERE [Added]=:value1", conn);
                getStock.Parameters.Add("value1", OleDbType.Date);
                getStock.Prepare();
                getStock.Parameters[0].Value = DateTime.Today;
                int results = 0;
                using (OleDbDataReader myreader = getStock.ExecuteReader())
                {
                    while (myreader.Read())
                    {
                        results++;
                        stockGridView.Rows.Add(myreader[0], myreader[1], myreader[2], myreader[3], myreader[4]);
                    }
                }
                noResultsLabel.Visible = results == 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private String DateCode(DateTime dt)
        {
            String code = "";
            DateTime date = dt.Date;
            code = date.Month.ToString().PadLeft(2, '0') + date.Day.ToString().PadLeft(2, '0') + date.Year.ToString().Substring(3);
            return code;
        }

        private LabelParameters GetLabelParameters()
        {
            LabelParameters parameters = new LabelParameters { Price = priceBox.Text };
            parameters.PriceText = Double.TryParse(parameters.Price, out double parsed) ? "$" + parsed.ToString(parameters.PriceFormat) : "$0";
            DataGridViewRow selectedRow = null;
            if (stockGridView.SelectedCells.Count > 0)
            {
                selectedRow = stockGridView.SelectedCells[0].OwningRow;
                parameters.StockNum = selectedRow.Cells[0].Value.ToString();
            }
            if (selectedRow != null && selectedRow.Cells[4].Value != null)
                if (DateTime.TryParse(selectedRow.Cells[4].Value.ToString(), out DateTime dt))
                    parameters.DateCode = DateCode(dt);

            return parameters;
        }

        private void UpdatePictureBox()
        {
            if (labelComboBox.SelectedIndex >= 0)
                pictureBox1.Image = labelRenderer.DrawLabel(labelComboBox.SelectedItem as LabelTemplate, pictureBox1.Image, false, GetLabelParameters());
        }

        private void labelComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            LabelTemplate labelTemplate = labelComboBox.SelectedItem as LabelTemplate;
            line1Box.Tag = labelTemplate.LineLength;
            if (labelTemplate.InputElements.Count > 1)
            {
                line2Box.Enabled = true;
                line2Box.Tag = labelTemplate.LineLength;
            }
            else
                line2Box.Enabled = false;
            if (labelTemplate.InputElements.Count > 2)
            {
                line3Box.Enabled = true;
                line3Box.Tag = labelTemplate.LineLength;
            }
            else
                line3Box.Enabled = false;

            if (line1Box.Text.Length > labelTemplate.LineLength && line2Box.Enabled)
                WrapLine(line1Box, line2Box);

            UpdatePictureBox();
        }

        private void WrapLine(TextBox box1, TextBox box2)
        {
            String toWrap = box1.Text;
            int maxLength = (int)box1.Tag;
            for (int i = toWrap.Length - 1; i >= 0; i--)
            {
                if (toWrap[i] == ' ' && i < maxLength)
                {
                    box1.Text = toWrap.Substring(0, i).Trim();
                    box2.Text = toWrap.Substring(i).Trim();
                    break;
                }
            }
        }

        private void stockGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (stockGridView.SelectedCells.Count > 0)
            {
                DataGridViewRow selectedRow = stockGridView.SelectedCells[0].OwningRow;
                line1Box.Text = selectedRow.Cells[1].Value.ToString();
                line2Box.Text = line3Box.Text = "";
                if (line1Box.Text.Length > (int)line1Box.Tag && line2Box.Enabled)
                    WrapLine(line1Box, line2Box);
                if (selectedRow.Cells["Price"].Value != null && selectedRow.Cells["Price"].Value != DBNull.Value)
                    priceBox.Text = (Convert.ToDouble(selectedRow.Cells["Price"].Value)).ToString("0.00");
                else
                    priceBox.Text = "0.00";
            }
        }

        private void line1Box_TextChanged(object sender, EventArgs e)
        {
            UpdatePictureBox();
        }

        private void printButton_Click(object sender, EventArgs e)
        {
            PrinterSettings ps = new PrinterSettings { PrinterName = labelPrinter };
            LabelTemplate labelTemplate = labelComboBox.SelectedItem as LabelTemplate;
            double labelWidth = labelTemplate.StockWidth;
            double labelHeight = labelTemplate.StockHeight;
            ps.Width = (int)(203 * labelHeight);
            ps.Length = (int)(203 * labelWidth);
            ps.Darkness = printDarkness;
            ps.PrintSpeed = printSpeed;

            List<byte> page = new List<byte>();
            page.AddRange(ZPLCommands.ClearPrinter(ps));
            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            labelRenderer.DrawLabel(labelTemplate, bmp, true, GetLabelParameters());
            page.AddRange(ZPLCommands.GraphicStore(bmp, 'R', "img"));
            page.AddRange(ZPLCommands.GraphicWrite(0, 0, "img", 'R'));
            page.AddRange(ZPLCommands.PrintBuffer((int)copiesUpDown.Value));
            new SpoolPrinter(ps).Print(page.ToArray());
        }

        private void line1Box_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '!' || e.KeyChar == '\b') return;
            TextBox tb = sender as TextBox;
            LabelTemplate labelTemplate = labelComboBox.SelectedItem as LabelTemplate;
            int rightMargin = labelTemplate.RightMargin;
            if (e.KeyChar >= 'a' && e.KeyChar <= 'z')
                e.KeyChar = (char)(e.KeyChar - 32);

            String newText = tb.Text + e.KeyChar;
            newText = newText.Replace("!", "");
            Font descFont;
            int lineIndex = textBoxes.IndexOf(sender as TextBox);
            LabelTextElement labelLine = labelTemplate.InputElements[lineIndex];
            int topMargin = labelTemplate.TopMargin;
            Point origin = labelTemplate.GetAbsolutePosition(labelLine, labelLine.StartPos);
            descFont = labelTemplate.GetFont(labelLine.Font);

            using (Graphics g = Graphics.FromImage(pictureBox1.Image))
            {
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                StringFormat format = new StringFormat(StringFormat.GenericDefault);
                format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
                int width = (int)g.MeasureString(newText, descFont, origin, format).Width;
                if (origin.X + width > rightMargin)
                {
                    e.Handled = true;
                    SystemSounds.Beep.Play();
                    return;
                }
            }            
        }

        private delegate void UpdateLabelDelegate(Label label, String text);
        private void UpdateLabel(Label label, String text)
        {
            if (InvokeRequired)
                BeginInvoke(new UpdateLabelDelegate(UpdateLabel), label, text);
            else
                label.Text = text;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {            
            UpdateLabel(coordinatesLabel, String.Format("Coordinates: ({0}, {1})", e.X, (e.Y - Convert.ToInt32(pictureBox1.Tag))));
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            UpdateLabel(coordinatesLabel, "");
        }

        private void stockGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            UpdatePictureBox();
        }

        private void priceBox_TextChanged(object sender, EventArgs e)
        {
            String text = priceBox.Text;
            if (text == "")
                text = "0.00";
            if (Double.TryParse(text, out double price))
            {
                UpdatePictureBox();
            }
            else
                SystemSounds.Beep.Play();
        }

        private void priceBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ' ')
            {
                e.Handled = true;
                return;
            }

            int caretPosition = priceBox.SelectionStart;
            if (e.KeyChar == '\b')
                return;

            String newText = priceBox.Text.Remove(caretPosition, priceBox.SelectionLength);
            newText = newText.Substring(0, caretPosition) + e.KeyChar + newText.Substring(caretPosition);
            if (!Double.TryParse(newText, out Double parsed))
            {
                e.Handled = true;
                SystemSounds.Beep.Play();
                return;
            }

            int dotIndex = newText.IndexOf(".");
            if (dotIndex >= 0 && newText.Substring(dotIndex).Length == 4)
            {
                e.Handled = true;
                SystemSounds.Beep.Play();
                return;
            }
        }

        private void bwTestConn_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                OleDbConnection conn = new OleDbConnection { ConnectionString = String.Format("Provider={0};Data Source={1}\\pawndata;Extended Properties=Paradox 4.x;", DataProvider, DataSource) };
                conn.Open();
                OleDbCommand getStock = new OleDbCommand("SELECT [Stock #], Description, Price, Cost, Added FROM STOCK WHERE [Stock #] LIKE value1", conn);
                getStock.Parameters.Add("value1", OleDbType.VarChar, 15);
                getStock.Prepare();
                getStock.Parameters[0].Value = "1010%";
                using (OleDbDataReader myreader = getStock.ExecuteReader()) { while (myreader.Read()) {  } }
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        private void bwTestConn_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            splashForm.Close();
        }
    }
}