using AWSGateway.Helpers;
using AWSGateway.Models;
using System.Drawing.Drawing2D;

namespace AWSGateway.Forms
{
    // stupčasti graf dnevnog troška - crta se izravno (GDI+), bez vanjske biblioteke
    // .NET za Windows Forms nema ugrađenu Chart kontrolu, a za jedan graf vanjska ovisnost nije opravdana
    public class DailyCostChart : Control
    {
        private const int MarginLeft = 62;
        private const int MarginRight = 12;
        private const int MarginTop = 26;
        private const int MarginBottom = 24;

        // najmanji razmak između oznaka dana na x-osi, u pikselima
        private const int MinLabelSpacing = 34;

        private List<DailyCostTotal> _days = new List<DailyCostTotal>();
        private string _unit = "USD";
        private int _hoverIndex = -1;
        private ToolTip _toolTip = new ToolTip();

        public DailyCostChart()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.ResizeRedraw, true);
        }

        public void SetData(List<DailyCostTotal> days, string unit)
        {
            if (days == null)
            {
                _days = new List<DailyCostTotal>();
            }
            else
            {
                _days = days;
            }

            if (string.IsNullOrEmpty(unit) == false)
            {
                _unit = unit;
            }

            _hoverIndex = -1;
            _toolTip.Hide(this);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (Brush textBrush = new SolidBrush(ForeColor))
            using (Brush mutedBrush = new SolidBrush(UiTheme.Colors.Muted))
            {
                g.DrawString(LanguageHelper.Format("cost_chart_title", _unit), Font, textBrush, MarginLeft, 4);

                Rectangle plot = new Rectangle(MarginLeft, MarginTop, Width - MarginLeft - MarginRight, Height - MarginTop - MarginBottom);

                if (_days.Count == 0 || plot.Width < 20 || plot.Height < 20)
                {
                    DrawCentered(g, LanguageHelper.Get("cost_chart_no_data"), mutedBrush, plot);
                    return;
                }

                double axisMax = GetNiceAxisMax(GetMaxAmount());

                DrawGrid(g, plot, axisMax, mutedBrush);
                DrawBars(g, plot, axisMax);
                DrawDayLabels(g, plot, mutedBrush);
            }
        }

        private double GetMaxAmount()
        {
            double max = 0;

            foreach (DailyCostTotal day in _days)
            {
                if (day.Amount > max)
                {
                    max = day.Amount;
                }
            }

            return max;
        }

        // gornja granica osi zaokružena na 1, 2 ili 5 × 10^n - oznake su tada čitljive (npr. 0,5 umjesto 0,4731)
        private static double GetNiceAxisMax(double max)
        {
            if (max <= 0)
            {
                return 1;
            }

            double magnitude = Math.Pow(10, Math.Floor(Math.Log10(max)));
            double normalized = max / magnitude;
            double niceNormalized;

            if (normalized <= 1)
            {
                niceNormalized = 1;
            }
            else if (normalized <= 2)
            {
                niceNormalized = 2;
            }
            else if (normalized <= 5)
            {
                niceNormalized = 5;
            }
            else
            {
                niceNormalized = 10;
            }

            return niceNormalized * magnitude;
        }

        private void DrawGrid(Graphics g, Rectangle plot, double axisMax, Brush labelBrush)
        {
            using (Pen gridPen = new Pen(Color.FromArgb(60, UiTheme.Colors.Muted)))
            {
                // tri linije: 0, polovica i maksimum
                for (int i = 0; i <= 2; i++)
                {
                    double value = axisMax * i / 2.0;
                    int y = plot.Bottom - (int)(plot.Height * i / 2.0);

                    g.DrawLine(gridPen, plot.Left, y, plot.Right, y);

                    string label = FormatAxisValue(value);
                    SizeF size = g.MeasureString(label, Font);
                    g.DrawString(label, Font, labelBrush, plot.Left - size.Width - 6, y - size.Height / 2);
                }
            }
        }

        private void DrawBars(Graphics g, Rectangle plot, double axisMax)
        {
            float slotWidth = (float)plot.Width / _days.Count;
            float barWidth = Math.Max(1f, slotWidth * 0.75f);

            using (Brush barBrush = new SolidBrush(UiTheme.AccentColor))
            using (Brush hoverBrush = new SolidBrush(ControlPaint.Dark(UiTheme.AccentColor, 0.1f)))
            {
                for (int i = 0; i < _days.Count; i++)
                {
                    double amount = Math.Max(0, _days[i].Amount);
                    float barHeight = (float)(plot.Height * amount / axisMax);

                    if (amount > 0 && barHeight < 1)
                    {
                        // vrlo mali, ali ne nulti trošak ostaje vidljiv
                        barHeight = 1;
                    }

                    float x = plot.Left + i * slotWidth + (slotWidth - barWidth) / 2f;
                    float y = plot.Bottom - barHeight;

                    if (i == _hoverIndex)
                    {
                        g.FillRectangle(hoverBrush, x, y, barWidth, barHeight);
                    }
                    else
                    {
                        g.FillRectangle(barBrush, x, y, barWidth, barHeight);
                    }
                }
            }
        }

        // oznaka dana u mjesecu; kod mnogo dana prikazuje se svaki n-ti, da se oznake ne preklapaju
        private void DrawDayLabels(Graphics g, Rectangle plot, Brush labelBrush)
        {
            float slotWidth = (float)plot.Width / _days.Count;
            int step = (int)Math.Ceiling(MinLabelSpacing / slotWidth);

            if (step < 1)
            {
                step = 1;
            }

            for (int i = 0; i < _days.Count; i += step)
            {
                string label = _days[i].Date.ToString("dd.MM.");
                SizeF size = g.MeasureString(label, Font);
                float x = plot.Left + i * slotWidth + slotWidth / 2f - size.Width / 2f;

                g.DrawString(label, Font, labelBrush, x, plot.Bottom + 4);
            }
        }

        private void DrawCentered(Graphics g, string text, Brush brush, Rectangle area)
        {
            SizeF size = g.MeasureString(text, Font);
            g.DrawString(text, Font, brush, area.Left + (area.Width - size.Width) / 2, area.Top + (area.Height - size.Height) / 2);
        }

        private static string FormatAxisValue(double value)
        {
            if (value == 0)
            {
                return "0";
            }

            if (value < 0.1)
            {
                return value.ToString("0.###");
            }

            return value.ToString("0.##");
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            int plotWidth = Width - MarginLeft - MarginRight;
            int index = -1;

            if (_days.Count > 0 && plotWidth > 0 && e.X >= MarginLeft && e.X < MarginLeft + plotWidth)
            {
                index = (int)((e.X - MarginLeft) / ((float)plotWidth / _days.Count));

                if (index >= _days.Count)
                {
                    index = _days.Count - 1;
                }
            }

            if (index == _hoverIndex)
            {
                return;
            }

            _hoverIndex = index;
            Invalidate();

            if (index < 0)
            {
                _toolTip.Hide(this);
                return;
            }

            DailyCostTotal day = _days[index];
            string text = day.Date.ToString("dd.MM.yyyy") + ": " + day.Amount.ToString("N4") + " " + _unit;

            _toolTip.Show(text, this, e.X + 12, e.Y - 20);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            _hoverIndex = -1;
            _toolTip.Hide(this);
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _toolTip.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}