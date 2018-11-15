using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;

namespace Labels
{
    public class LabelTemplate
    {
        private string _description = "Unnamed label";
        private double _stockwidth = 6;
        private double _stockheight = 4;
        private int _rightmargin = 450;
        private int _topmargin = 10;
        private int _bottommargin = 1000;
        private int _leftmargin = 10;
        private int _heightpx = 254;
        private int _linelength = 20;
        List<LabelLineElement> _lines = new List<LabelLineElement>();
        List<LabelTextElement> _inputelements = new List<LabelTextElement>();
        HashSet<LabelElement> _elements = new HashSet<LabelElement>();
        Dictionary<string, Font> _fonts = new Dictionary<string, Font>();

        #region Properties
        public double StockWidth { get => _stockwidth; }
        public double StockHeight { get => _stockheight; }
        public string Description { get => _description; }
        public int TopMargin { get => _topmargin; }
        public int RightMargin { get => _rightmargin; }
        public int BottomMargin { get => _bottommargin; }
        public int LeftMargin { get => _leftmargin; }
        public Rectangle LabelBoundingRect { get => new Rectangle(_leftmargin, _topmargin, _rightmargin - _leftmargin, _bottommargin - _topmargin); }
        public int HeightPx { get => _heightpx; }
        public int LineLength { get => _linelength; }
        public List<LabelLineElement> Lines { get => _lines; }
        public HashSet<LabelElement> Elements { get => _elements; }
        public List<LabelTextElement> InputElements { get => _inputelements; }
        #endregion

        public Font GetFont(string alias)
        {
            return _fonts[alias];
        }

        public LabelTemplate(XElement root)
        {
            GetDescription(root);
            GetStockDimensions(root);
            GetMargins(root);
            GetFonts(root);
            GetCutLines(root);
            GetFoldLines(root);
            GetTextElements(root);
            GetBarcodes(root);
            GetPrintHeight(root);
            if (root.Element("linelength") != null)
                Int32.TryParse(root.Element("linelength").Value, out _linelength);
        }

        private void GetBarcodes(XElement root)
        {
            var barcodes = root.Elements("barcode");
            foreach (var barcode in barcodes)
                _elements.Add(new LabelBarcodeElement(barcode));
        }

        public Point GetAbsolutePosition(LabelElement element, ElementPoint ep)
        {
            Point p = new Point();
            if (!ep.X.Relative)
                p.X = ep.X.Value;
            if (!ep.Y.Relative)
                p.Y = ep.Y.Value + _topmargin;

            return p;
        }

        public Point GetAbsolutePosition(LabelElement element, ElementPoint ep, Rectangle boundingRect)
        {
            Point p = new Point();
            if (ep.X.Relative)
            {
                if (ep.X.Value > 0)
                    p.X = ep.X.Value + boundingRect.Right;
                else
                    p.X = ep.X.Value + boundingRect.Left;
            }
            else
                p.X = ep.X.Value;
            if (ep.Y.Relative)
            {
                if (ep.Y.Value > 0)
                    p.Y = ep.Y.Value + boundingRect.Bottom;
                else
                    p.Y = ep.Y.Value + boundingRect.Top;
            }
            else
                p.Y = ep.Y.Value + _topmargin;
            return p;
        }

        private void GetPrintHeight(XElement root)
        {
            if (root.Element("heightpx") != null)
                Int32.TryParse(root.Element("heightpx").Value, out _heightpx);
        }

        private void GetTextElements(XElement root)
        {
            var textElements = root.Elements("text");
            foreach (var textElement in textElements)
            {
                var element = new LabelTextElement(textElement);
                _elements.Add(element);
                if (element.Parameter == "lineinput")
                    _inputelements.Add(element);
            }
        }

        private void GetCutLines(XElement root)
        {
            var cutLines = root.Elements("cutline");
            foreach (var lineEl in cutLines)
            {
                var element = new LabelLineElement(lineEl);
                _lines.Add(element);
                _elements.Add(element);
            }
        }

        private void GetFoldLines(XElement root)
        {
            var foldLines = root.Elements("foldline");
            foreach (var lineEl in foldLines)
            {
                var element = new LabelLineElement(lineEl);
                _lines.Add(element);
                _elements.Add(element);
            }
        }

        private void GetFonts(XElement root)
        {
            var fontElements = root.Elements("font");
            foreach (var fontElement in fontElements)
            {
                Font font = new Font(fontElement.Attribute("family").Value, Convert.ToInt32(fontElement.Attribute("size").Value));
                string alias = fontElement.Attribute("alias").Value;
                _fonts.Add(alias, font);
            }
        }

        private void GetMargins(XElement root)
        {
            if (root.Element("margins") != null)
            {
                var marginsEl = root.Element("margins");
                if (marginsEl.Attribute("right") != null)
                    Int32.TryParse(marginsEl.Attribute("right").Value, out _rightmargin);
                if (marginsEl.Attribute("top") != null)
                    Int32.TryParse(marginsEl.Attribute("top").Value, out _topmargin);
                if (marginsEl.Attribute("bottom") != null)
                    Int32.TryParse(marginsEl.Attribute("bottom").Value, out _bottommargin);
                if (marginsEl.Attribute("left") != null)
                    Int32.TryParse(marginsEl.Attribute("left").Value, out _leftmargin);
            }
        }

        private void GetDescription(XElement root)
        {
            if (root.Element("description") != null)
                _description = root.Element("description").Value;
        }

        private void GetStockDimensions(XElement root)
        {
            if (root.Element("labelstock") != null)
            {
                var labelStockEl = root.Element("labelstock");
                if (labelStockEl.Attribute("width") != null)
                    Double.TryParse(labelStockEl.Attribute("width").Value, out _stockwidth);
                if (labelStockEl.Attribute("height") != null)
                    Double.TryParse(labelStockEl.Attribute("height").Value, out _stockheight);
            }
        }

        public override string ToString()
        {
            return _description;
        }
    }

    #region LabelElement classes
    public class LabelLineElement : LabelElement
    {
        private ElementPoint _endpos;
        private LineType _linetype;

        public LabelLineElement(XElement element) : base(element)
        {
            _elementType = ElementType.Line;
            if (element.Name.ToString() == "cutline")
                _linetype = LineType.CutLine;
            else
                _linetype = LineType.FoldLine;
            var points = element.Elements("point");
            if (points != null && points.Count() == 2)
            {
                _startpos = GetPoint(points.First());
                _endpos = GetPoint(points.Last());
            }
        }

        public ElementPoint EndPos { get => _endpos; set => _endpos = value; }
        public LineType LineType { get => _linetype; set => _linetype = value; }
    }

    public class LabelTextElement : LabelElement
    {
        private string _font = "base";
        private Alignment _alignment = Alignment.LEFT;
        private bool _inverted = false;
        private string _text = "";
        private bool _truncatedecimal = false;
        private int _rotation = 0;
        private int _fillwidth = 0;

        public string Font { get => _font; set => _font = value; }
        public Alignment Alignment { get => _alignment; set => _alignment = value; }
        public string Text { get => _text; set => _text = value; }
        public bool Inverted { get => _inverted; set => _inverted = value; }
        public bool TruncateDecimal { get => _truncatedecimal; }
        public int Rotation { get => _rotation; }
        public int FillWidth { get => _fillwidth; }

        public LabelTextElement(XElement element) : base(element)
        {
            _elementType = ElementType.Text;
            if (element.Attribute("font") != null)
                _font = element.Attribute("font").Value;
            if (element.Attribute("align") != null)
            {
                string align = element.Attribute("align").Value;
                if (Enum.GetNames(typeof(Alignment)).Contains(align.ToUpper()))
                    _alignment = (Alignment)Enum.Parse(typeof(Alignment), align.ToUpper());
            }
            if (element.Attribute("inverted") != null)
                bool.TryParse(element.Attribute("inverted").Value, out _inverted);
            _text = element.Value;
            if (element.Attribute("truncatedecimal") != null)
                bool.TryParse(element.Attribute("truncatedecimal").Value, out _truncatedecimal);
            if (element.Attribute("rotate") != null)
                Int32.TryParse(element.Attribute("rotate").Value, out _rotation);
            if (element.Attribute("fillwidth") != null)
                Int32.TryParse(element.Attribute("fillwidth").Value, out _fillwidth);
        }
    }

    public class LabelBarcodeElement : LabelElement
    {
        private BarcodeSymbology _symbology = BarcodeSymbology.Code39;
        private int _size = 10;

        public BarcodeSymbology Symbology { get => _symbology; set => _symbology = value; }
        public int Size { get => _size; }

        public LabelBarcodeElement(XElement element) : base(element)
        {
            _elementType = ElementType.Barcode;
            if (element.Attribute("symbology") != null)
            {
                string sym = element.Attribute("symbology").Value;
                if (Enum.GetNames(typeof(BarcodeSymbology)).Contains(sym))
                    _symbology = (BarcodeSymbology)Enum.Parse(typeof(BarcodeSymbology), sym);
            }
            if (element.Attribute("size") != null)
                Int32.TryParse(element.Attribute("size").Value, out _size);
        }
    }

    public class LabelElement
    {
        protected string _param;
        protected ElementPoint _startpos;
        protected string _id = "";
        protected string _anchor = "";
        protected ElementType _elementType;

        public ElementPoint StartPos { get => _startpos; set => _startpos = value; }
        public string Parameter { get => _param; set => _param = value; }
        public string ID { get => _id; set => _id = value; }
        public string Anchor { get => _anchor; set => _anchor = value; }
        public ElementType ElementType { get => _elementType; }

        public LabelElement(XElement element)
        {
            _startpos = GetPoint(element);
            if (element.Attribute("param") != null)
                _param = element.Attribute("param").Value;
            if (element.Attribute("id") != null)
                _id = element.Attribute("id").Value;
            if (element.Attribute("anchor") != null)
                _anchor = element.Attribute("anchor").Value;
        }

        public static ElementPoint GetPoint(XElement element)
        {
            ElementPoint p = null;
            if (element.Attribute("xpos") != null && element.Attribute("ypos") != null)
                p = new ElementPoint(element.Attribute("xpos").Value, element.Attribute("ypos").Value);

            return p;
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this == obj;
        }
    }
    #endregion

    public class ElementPoint
    {
        private Coordinate _x, _y;

        public Coordinate X { get => _x; set => _x = value; }
        public Coordinate Y { get => _y; set => _y = value; }

        public ElementPoint(string X, string Y)
        {
            this.X = new Coordinate(X);
            this.Y = new Coordinate(Y);
        }

        public class Coordinate
        {
            private int _value = -1;

            public int Value { get => _value; set => _value = value; }
            public bool Relative { get; set; } = false;

            public Coordinate(string value)
            {
                if (Int32.TryParse(value, out _value))
                    if (value.Contains("+") || value.Contains("-"))
                        Relative = true;
            }
        }
    }
    #region Enums
    public enum LineType
    {
        CutLine,
        FoldLine
    }

    public enum BarcodeSymbology
    {
        Code39,
        Code128
    }

    public enum Alignment
    {
        LEFT,
        CENTER,
        RIGHT,
        FILL
    }

    public enum ElementType
    {
        Text,
        Line,
        Barcode
    }
    #endregion
}
