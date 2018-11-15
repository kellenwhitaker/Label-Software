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
        PrivateFontCollection fonts = new PrivateFontCollection();
        List<XElement> labelTemplates = new List<XElement>();
        List<TextBox> textBoxes = new List<TextBox>();

        public Form1()
        {
            InitializeComponent();
            textBoxes.AddRange(new[] { line1Box, line2Box, line3Box });
            splashForm = new SplashForm();
            splashForm.Show();
            fonts.AddFontFile("free3of9.ttf");
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

        static public int MeasureDisplayStringWidth(Graphics graphics, string text, Font font)
        {
            if (text == null || text == "") return 0;
            StringFormat format = new StringFormat();
            RectangleF rect = new RectangleF(0, 0, 1000, 1000);
            CharacterRange[] ranges = { new CharacterRange(0, text.Length) };
            Region[] regions = new Region[1];

            format.SetMeasurableCharacterRanges(ranges);

            regions = graphics.MeasureCharacterRanges(text, font, rect, format);
            rect = regions[0].GetBounds(graphics);

            return (int)(rect.Right + 1.0f);
        }

        static public int MeasureDisplayStringWidth(Graphics graphics, string text, Font font, StringAlignment alignment)
        {
            if (text == null || text == "") return 0;
            StringFormat format = new StringFormat { Alignment = alignment };
            RectangleF rect = new RectangleF(0, 0, 1000, 1000);
            CharacterRange[] ranges = { new CharacterRange(0, text.Length) };
            Region[] regions = new Region[1];

            format.SetMeasurableCharacterRanges(ranges);

            regions = graphics.MeasureCharacterRanges(text, font, rect, format);
            rect = regions[0].GetBounds(graphics);

            return (int)(rect.Right + 1.0f);
        }

        private void AddHighlights(Graphics g, Font font, Regex r, String text, int y, int leftMargin)
        {
            MatchCollection matches = r.Matches(text);            
            foreach (Match match in matches)
            {
                AddSingleHighlight(g, font, match, text, y, leftMargin);
            }
        }

        private void AddSingleHighlight(Graphics g, Font font, Match m, String text, int y, int leftMargin)
        {
            StringFormat format = new StringFormat(StringFormat.GenericTypographic) { Trimming = StringTrimming.None, Alignment = StringAlignment.Center };
            format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;

            if (m.Success)
            {
                int startIndex = m.Index;
                SizeF textSize = new SizeF(0, 0);
                Rectangle rect = new Rectangle();
                String highlightedText = m.Value.Replace("!", "");
                if (startIndex > 0)
                {
                    String priorText = text.Substring(0, startIndex);
                    String cleanPriorText = priorText.Replace("!", "");
                    textSize = g.MeasureString("DUMMY", font, new Point(), format);
                    float doubleWidth = g.MeasureString(cleanPriorText + cleanPriorText, font, new Point(), format).Width;
                    textSize.Width = doubleWidth - textSize.Width;
                    textSize.Width = MeasureDisplayStringWidth(g, cleanPriorText, font);
                    doubleWidth = MeasureDisplayStringWidth(g, cleanPriorText + cleanPriorText, font);
                    textSize.Width = doubleWidth - textSize.Width;
                    rect.X = (int)(leftMargin + (int)textSize.Width - .1 * textSize.Height);
                    rect.Width = MeasureDisplayStringWidth(g, highlightedText + highlightedText, font) - MeasureDisplayStringWidth(g, highlightedText, font);
                    rect.Width += (int)(.25 * textSize.Height);
                    rect.Height = (int)textSize.Height;
                    rect.Y = y - 1;
                }
                else
                {
                    textSize.Height = g.MeasureString("DUMMY", font, new Point(), format).Height;
                    rect.X = (int)(leftMargin + (int)textSize.Width - .1 * textSize.Height);
                    rect.Y = y - 1;
                    rect.Width = MeasureDisplayStringWidth(g, highlightedText + highlightedText, font) - MeasureDisplayStringWidth(g, highlightedText, font);
                    rect.Width += (int)(.25 * textSize.Height);
                    rect.Height = (int)textSize.Height;
                }
                g.FillRectangle(Brushes.Black, rect);
                g.DrawString(m.Value.Replace("!", ""), font, Brushes.White, new PointF(rect.X, y));
            }

        }

        private void DrawRotatedText(Graphics g, Font f, String text, int angle, Point p)
        {
            SizeF sz = g.VisibleClipBounds.Size;
            g.TranslateTransform(p.X, p.Y);
            g.RotateTransform(angle);
            g.TranslateTransform(-p.X, -p.Y);
            sz = g.MeasureString(text, f);
            //g.DrawString(text, f, Brushes.Black, new PointF(p.X - sz.Width / 2.0f, p.Y - sz.Height / 2.0f));
            g.DrawString(text, f, Brushes.Black, new PointF(p.X - sz.Width / 2.0f, p.Y + (int)(500.0 / sz.Height) - 5));
            //g.DrawString(text, f, Brushes.Black, new PointF(p.X - sz.Width / 2.0f, p.Y));
            g.ResetTransform();
        }

        private Rectangle GetTextBoundingRect(Graphics g, Font font, string text, Point p, Alignment alignment)
        {
            Rectangle rect = new Rectangle();
            if (alignment == Alignment.LEFT)
            {
                rect.X = p.X;
                rect.Y = p.Y;
                rect.Width = MeasureDisplayStringWidth(g, text, font);
                rect.Height = (int)g.MeasureString(text, font).Height;
            }
            else if (alignment == Alignment.RIGHT)
            {
                rect.Width = MeasureDisplayStringWidth(g, text, font);
                rect.Height = (int)g.MeasureString(text, font).Height;
                rect.X = p.X - rect.Width;
                rect.Y = p.Y;
            }
            else
            {
                rect.Width = MeasureDisplayStringWidth(g, text, font);
                rect.X = (int)(p.X - rect.Width / 2.0f);
                rect.Height = (int)g.MeasureString(text, font).Height;
                rect.Y = (int)(p.Y - rect.Height / 2.0f);
            }

            return rect;
        }

        private Font GetFillFont(Graphics g, LabelTemplate labelTemplate, LabelTextElement element, string text, Point center, string fontFamily)
        {            
            Rectangle labelBounds = labelTemplate.LabelBoundingRect;
            Rectangle rect = labelBounds;
            float fontSize = 10.0f;
            Font font = null;
            while (Rectangle.Union(rect, labelBounds).Equals(labelBounds))
            {
                font = new Font(fontFamily, fontSize);
                rect = GetTextBoundingRect(g, font, text, center, Alignment.CENTER);
                fontSize += 1;
            }

            return new Font(fontFamily, fontSize - 1);
        }

        private Font GetFillFont(Graphics g, LabelTemplate labelTemplate, LabelTextElement element, string text, string fontFamily, int fillWidth)
        {
            Rectangle boundingRect = new Rectangle();
            float fontSize = 10.0f;
            Font font = null;
            while (boundingRect.Width < fillWidth)
            {
                font = new Font(fontFamily, fontSize);
                boundingRect = GetTextBoundingRect(g, font, text, new Point(0, 0), Alignment.CENTER);
                fontSize += 1;
            }
            return new Font(fontFamily, fontSize - 1);
        }

        private Bitmap ScaleBarcode(Image barcode, double scaleFactor)
        {
            Bitmap bmp = new Bitmap(barcode, new Size((int)(barcode.Width * scaleFactor), (int)(barcode.Height * scaleFactor)));
            return bmp;
        }

        private void DrawLines(Graphics g, LabelTemplate labelTemplate)
        {
            foreach (var line in labelTemplate.Lines)
            {
                Point p1 = labelTemplate.GetAbsolutePosition(line, line.StartPos);
                Point p2 = labelTemplate.GetAbsolutePosition(line, line.EndPos);
                Pen pen = new Pen(Brushes.Black);
                if (line.LineType == LineType.CutLine)
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    pen.Width = 2;
                }
                g.DrawLine(pen, p1, p2);
            }
        }

        private string FormatPrice(string priceText, LabelTextElement element)
        {
            if (element.TruncateDecimal && priceText.Contains(".00"))
                return priceText.Remove(priceText.IndexOf(".00"), 3);
            else
                return priceText;
        }

        private void DrawMargins(Graphics g, LabelTemplate labelTemplate)
        {
            Brush marginColor = new SolidBrush(Color.FromArgb(0, 0, 0, 255));
            Rectangle topMarginRect = new Rectangle(0, 0, (int)g.ClipBounds.Width, labelTemplate.TopMargin);
            g.FillRectangle(marginColor, topMarginRect);
            if (labelTemplate.RightMargin != 1000)
            {
                Rectangle rightMarginRect = new Rectangle(labelTemplate.RightMargin, 0, 10, labelTemplate.TopMargin + labelTemplate.HeightPx);
                g.FillRectangle(marginColor, rightMarginRect);
            }
            if (labelTemplate.BottomMargin != 1000)
            {
                Rectangle bottomMarginRect = new Rectangle(0, labelTemplate.BottomMargin, topMarginRect.Width, 10);
                g.FillRectangle(marginColor, bottomMarginRect);
            }
        }

        private class LabelParameters
        {
            public string StockNum = "00000";
            public string DateCode = "00000";
            public string Price = "0";
            public string PriceFormat = "0.00";
            public string PriceText = "0";
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

        private void DrawLabel(LabelTemplate labelTemplate, Image img, bool forPrinting, string printText = null)
        {
            Dictionary<string, Rectangle> boundingRects = new Dictionary<string, Rectangle> { { "", new Rectangle() } };
            pictureBox1.Tag = labelTemplate.TopMargin;
            Bitmap bmp = img != null ? (Bitmap)img : new Bitmap(457, 254);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.FillRectangle(Brushes.White, new Rectangle(0, 0, bmp.Width, bmp.Height));
                if (!forPrinting)
                    DrawMargins(g, labelTemplate);

                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                LabelParameters pars = GetLabelParameters();

                DrawLines(g, labelTemplate);
                int lineInputCount = 0;
                foreach (var element in labelTemplate.Elements)
                {
                    Point p = labelTemplate.GetAbsolutePosition(element, element.StartPos, boundingRects[element.Anchor]);
                    string param = element.Parameter;
                    string paramValue = printText == null ? "" : printText;
                    switch (param)
                    {
                        case "stock": paramValue = pars.StockNum; break;
                        case "date": paramValue = pars.DateCode; break;
                        case "store": paramValue = "Dave's Pawn LLC"; break;
                        case "price": paramValue = FormatPrice(pars.PriceText, (LabelTextElement)element); break;
                        default: continue;
                    }
                    if (element.ElementType == ElementType.Text)
                    {
                        Font font = labelTemplate.GetFont((element as LabelTextElement).Font);
                        Brush textBrush = Brushes.Black;
                        LabelTextElement textElement = element as LabelTextElement;
                        if (textElement.Rotation == 0)
                        {
                            Rectangle rect = GetTextBoundingRect(g, font, paramValue, p, textElement.Alignment);
                            if (textElement.Inverted)
                            {
                                Rectangle fillRect = new Rectangle(rect.X + (int)(rect.Height * 0.1), rect.Y, rect.Width - (int)(rect.Height * 0.1), (int)(rect.Height * 0.85));
                                g.FillRectangle(Brushes.Black, fillRect);
                                textBrush = Brushes.White;
                            }

                            if (textElement.Alignment == Alignment.LEFT)
                                g.DrawString(paramValue, font, textBrush, p);
                            else
                            {
                                rect.Width = 500;
                                g.DrawString(paramValue, font, textBrush, rect);
                            }
                            if (element.ID != "")
                                boundingRects.Add(element.ID, rect);
                        }
                        else
                        {
                            if (textElement.Alignment == Alignment.FILL)
                                font = GetFillFont(g, labelTemplate, textElement, paramValue, font.FontFamily.Name, textElement.FillWidth);
                            DrawRotatedText(g, font, paramValue, textElement.Rotation, p);
                        }
                    }
                    else if (element.ElementType == ElementType.Barcode)
                    {
                        Rectangle boundingRect = new Rectangle();
                        if ((element as LabelBarcodeElement).Symbology == BarcodeSymbology.Code39)
                        {
                            using (Font C39 = new Font(fonts.Families[0], (element as LabelBarcodeElement).Size))
                            {
                                boundingRect = GetTextBoundingRect(g, C39, "*" + paramValue + "*", p, Alignment.LEFT);
                                g.DrawString("*" + paramValue + "*", C39, Brushes.Black, p);
                            }
                        }
                        else if ((element as LabelBarcodeElement).Symbology == BarcodeSymbology.Code128)
                        {
                            int barWeight = 1;
                            Bitmap barcode = (Bitmap)Code128Rendering.MakeBarcodeImage(paramValue, barWeight, false);
                            g.DrawImage(barcode, p);
                            boundingRect = new Rectangle(p.X, p.Y, barcode.Width, barcode.Height);
                        }
                        if (element.ID != "")
                            boundingRects.Add(element.ID, boundingRect);
                    }
                }

                foreach (var element in labelTemplate.InputElements)
                {
                    string paramValue = printText == null ? textBoxes[lineInputCount].Text : printText;
                    if (paramValue != "")
                    {
                        LabelTextElement textElement = element as LabelTextElement;
                        Rectangle rect = new Rectangle();
                        Font font = null;
                        Point p = labelTemplate.GetAbsolutePosition(element, element.StartPos, boundingRects[element.Anchor]);
                        if (textElement.Alignment != Alignment.FILL)
                        {
                            font = labelTemplate.GetFont(element.Font);
                            rect = GetTextBoundingRect(g, font, paramValue.Replace("!", ""), p, (element as LabelTextElement).Alignment);
                            g.DrawString(paramValue.Replace("!", ""), font, Brushes.Black, p);
                            AddHighlights(g, font, new Regex("![^!]+!"), paramValue, p.Y, p.X + 3);
                        }
                        else
                        {
                            font = labelTemplate.GetFont(element.Font);
                            font = GetFillFont(g, labelTemplate, textElement, paramValue, p, font.FontFamily.Name);
                            rect = GetTextBoundingRect(g, font, paramValue, p, Alignment.CENTER);
                            rect.Width = rect.Width + (int)(rect.Height * .05);
                            Brush textBrush = Brushes.Black;
                            if (textElement.Inverted)
                            {
                                textBrush = Brushes.White;
                                g.FillRectangle(Brushes.Black, rect);
                            }
                            rect.Width = 500;
                            g.DrawString(paramValue, font, textBrush, rect);
                        }

                        if (element.ID != "")
                            boundingRects.Add(element.ID, rect);

                    }
                    lineInputCount++;
                }
            }
            if (!forPrinting)
                pictureBox1.Image = bmp;
        }

        private void UpdatePictureBox()
        {
            if (labelComboBox.SelectedIndex >= 0)
                DrawLabel(labelComboBox.SelectedItem as LabelTemplate, pictureBox1.Image, false);
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
            String text = line1Box.Text;
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
            DrawLabel(labelTemplate, bmp, true);
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