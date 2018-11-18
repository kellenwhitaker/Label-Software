using GenCode128;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Labels
{
    public class LabelRenderer
    {
        private PrivateFontCollection fonts;
        private List<TextBox> textBoxes;
        public LabelRenderer(PrivateFontCollection fonts, List<TextBox> textBoxes)
        {
            this.fonts = fonts;
            this.textBoxes = textBoxes;
        }

        public Image DrawLabel(LabelTemplate labelTemplate, Image img, bool forPrinting, LabelParameters pars, string printText = null)
        {
            Dictionary<string, Rectangle> boundingRects = new Dictionary<string, Rectangle> { { "", new Rectangle() } };
            //pictureBox1.Tag = labelTemplate.TopMargin;
            Bitmap bmp = img != null ? (Bitmap)img : new Bitmap(457, 254);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.FillRectangle(Brushes.White, new Rectangle(0, 0, bmp.Width, bmp.Height));
                if (!forPrinting)
                    DrawMargins(g, labelTemplate);

                g.TextRenderingHint = TextRenderingHint.AntiAlias;

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
            return bmp;
        }

        private static void DrawMargins(Graphics g, LabelTemplate labelTemplate)
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

        private static void DrawLines(Graphics g, LabelTemplate labelTemplate)
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

        private static Bitmap ScaleBarcode(Image barcode, double scaleFactor)
        {
            Bitmap bmp = new Bitmap(barcode, new Size((int)(barcode.Width * scaleFactor), (int)(barcode.Height * scaleFactor)));
            return bmp;
        }

        private static Font GetFillFont(Graphics g, LabelTemplate labelTemplate, LabelTextElement element, string text, Point center, string fontFamily)
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

        private static Font GetFillFont(Graphics g, LabelTemplate labelTemplate, LabelTextElement element, string text, string fontFamily, int fillWidth)
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

        private static Rectangle GetTextBoundingRect(Graphics g, Font font, string text, Point p, Alignment alignment)
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

        private static void DrawRotatedText(Graphics g, Font f, String text, int angle, Point p)
        {
            SizeF sz = g.VisibleClipBounds.Size;
            g.TranslateTransform(p.X, p.Y);
            g.RotateTransform(angle);
            g.TranslateTransform(-p.X, -p.Y);
            sz = g.MeasureString(text, f);
            g.DrawString(text, f, Brushes.Black, new PointF(p.X - sz.Width / 2.0f, p.Y + (int)(500.0 / sz.Height) - 5));
            g.ResetTransform();
        }

        private static void AddHighlights(Graphics g, Font font, Regex r, String text, int y, int leftMargin)
        {
            MatchCollection matches = r.Matches(text);
            foreach (Match match in matches)
            {
                AddSingleHighlight(g, font, match, text, y, leftMargin);
            }
        }

        private static void AddSingleHighlight(Graphics g, Font font, Match m, String text, int y, int leftMargin)
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

        private static int MeasureDisplayStringWidth(Graphics graphics, string text, Font font)
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

        private static int MeasureDisplayStringWidth(Graphics graphics, string text, Font font, StringAlignment alignment)
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

        private static string FormatPrice(string priceText, LabelTextElement element)
        {
            if (element.TruncateDecimal && priceText.Contains(".00"))
                return priceText.Remove(priceText.IndexOf(".00"), 3);
            else
                return priceText;
        }
    }
}
