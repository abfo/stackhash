using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Globalization;

namespace StackHash
{
    /// <summary>
    /// Event args for a double-clicked pie chart segment
    /// </summary>
    public class SegmentDoubleClickEventArgs : EventArgs
    {
        /// <summary>
        /// The legend of the double-clicked segment
        /// </summary>
        public string SegmentLegend { get; private set; }

        /// <summary>
        /// Event args for a double-clicked pie chart segment
        /// </summary>
        /// <param name="segmentLegend">The legend of the double-clicked segment</param>
        public SegmentDoubleClickEventArgs(string segmentLegend)
        {
            this.SegmentLegend = segmentLegend;
        }
    }

    /// <summary>
    /// Displays a pie chart
    /// </summary>
    public partial class PieChartControl : UserControl
    {
        /// <summary>
        /// Event fired when a segment of the pie chart is double clicked
        /// </summary>
        public event EventHandler<SegmentDoubleClickEventArgs> SegmentDoubleClick;

        /// <summary>
        /// Gets or sets the stroke brush
        /// </summary>
        public Brush Stroke { get; set; }

        /// <summary>
        /// Number of decimal places to display for data values
        /// </summary>
        public int DecimalPlaces { get; set; }

        /// <summary>
        /// Maximum length of a label
        /// </summary>
        public int MaxLabelLength { get; set; }

        /// <summary>
        /// Gets or sets the chart title
        /// </summary>
        public string ChartTitle { get; set; }

        /// <summary>
        /// Gets the list of fill brushes
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<SolidColorBrush> FillList { get; private set; }

        private const double LegendPadding = 5.0;
        private const int DefaultMaxLabelLength = 20;
        private const double RadiusFactor = 0.95;

        private List<PieChartDataPoint> _data;

        /// <summary>
        /// Displays a pie chart
        /// </summary>
        public PieChartControl()
        {
            InitializeComponent();

            _data = new List<PieChartDataPoint>();
            this.Stroke = Brushes.Black;
            this.DecimalPlaces = 0;
            this.MaxLabelLength = DefaultMaxLabelLength;
            this.FillList = new List<SolidColorBrush>();

            // 7 colors...
            this.FillList.Add(new SolidColorBrush(Color.FromRgb(69, 111, 167)));
            this.FillList.Add(new SolidColorBrush(Color.FromRgb(167, 69, 69)));
            this.FillList.Add(new SolidColorBrush(Color.FromRgb(69, 167, 69)));
            this.FillList.Add(new SolidColorBrush(Color.FromRgb(167, 69, 167)));
            this.FillList.Add(new SolidColorBrush(Color.FromRgb(69, 167, 167)));
            this.FillList.Add(new SolidColorBrush(Color.FromRgb(167, 167, 69)));
            this.FillList.Add(new SolidColorBrush(Color.FromRgb(69, 69, 167)));

            // same 7, a bit lighter
            this.FillList.Add(new SolidColorBrush(Color.FromRgb(144, 172, 210)));
            this.FillList.Add(new SolidColorBrush(Color.FromRgb(210, 144, 144)));
            this.FillList.Add(new SolidColorBrush(Color.FromRgb(144, 210, 144)));
            this.FillList.Add(new SolidColorBrush(Color.FromRgb(210, 144, 210)));
            this.FillList.Add(new SolidColorBrush(Color.FromRgb(144, 210, 210)));
            this.FillList.Add(new SolidColorBrush(Color.FromRgb(210, 210, 144)));
            this.FillList.Add(new SolidColorBrush(Color.FromRgb(144, 144, 210)));
        

            this.ChartTitle = string.Empty;

            ClearData();
        }

        /// <summary>
        /// Sets the data for the pie chart
        /// </summary>
        /// <param name="data">Data</param>
        public void SetData(IEnumerable<PieChartDataPoint> data)
        {
            _data.Clear();
            _data.AddRange(data);
            _data.Sort();

            PlotPieChart();
        }

        /// <summary>
        /// Clears any current data in the chart
        /// </summary>
        public void ClearData()
        {
            _data.Clear();

            PlotPieChart();
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // resize the canvas
            Double dimension;
            if (pieGrid.ActualWidth > pieGrid.ActualHeight)
            {
                dimension = pieGrid.ActualHeight;
            }
            else
            {
                dimension = pieGrid.ActualWidth;
            }

            pieCanvas.Width = dimension;
            pieCanvas.Height = dimension;

            PlotPieChart();
        }

        private void PlotPieChart()
        {
            pieCanvas.Children.Clear();
            legendCanvas.Children.Clear();

            textTitle.Text = this.ChartTitle;

            if (_data.Count == 0)
            {
                return;
            }

            if (this.FillList.Count == 0)
            {
                return;
            }

            double totalData = 0.0;
            for (int i = 0; i < _data.Count; i++)
            {
                totalData += _data[i].Data;
            }

            double startAngle = -(Math.PI / 2.0);
            double endAngle = 0.0;
            int fillIndex = 0;

            double legendWidth = 0.0;
            double legendHeight = 0.0;
            double legendNextY = 0.0;

            for (int segment = 0; segment < _data.Count; segment++)
            {
                startAngle += endAngle;
                endAngle = (2.0 * Math.PI) * (_data[segment].Data / totalData);

                Shape legendFillSource;

                if (_data.Count == 1)
                {
                    legendFillSource = AddSingleDataPoint(this.FillList[fillIndex], _data[segment].Label, _data[segment].Data);
                }
                else
                {
                    legendFillSource = AddSegment(this.FillList[fillIndex], startAngle, startAngle + endAngle, _data[segment].Label, _data[segment].Data);
                }
                AddLegend(legendFillSource, _data[segment].Label, ref legendWidth, ref legendHeight, ref legendNextY);

                // get the next fill, or wrap round if too many
                fillIndex++;
                if (fillIndex >= this.FillList.Count)
                {
                    fillIndex = 0;
                }
            }

            legendCanvas.Width = legendWidth;
            legendCanvas.Height = legendHeight;
        }

        private Shape AddSegment(SolidColorBrush fill, double startAngle, double endAngle, string label, double data)
        {
            // note, asumes canvas is always square
            double center = pieCanvas.Width / 2;
            double radius = center * RadiusFactor;

            Point centerPoint = new Point(center, center);

            Point startPoint = new Point(center + (radius * Math.Cos(startAngle)),
                center + (radius * Math.Sin(startAngle)));

            Point endPoint = new Point(center + (radius * Math.Cos(endAngle)),
                center + (radius * Math.Sin(endAngle)));

            ArcSegment arc = new ArcSegment();
            arc.Point = endPoint;
            arc.SweepDirection = SweepDirection.Clockwise;
            arc.Size = new Size(radius, radius);
            if ((endAngle - startAngle) > Math.PI)
            {
                arc.IsLargeArc = true;
            }

            PathFigure pathFigure = new PathFigure();
            pathFigure.IsClosed = true;
            pathFigure.StartPoint = centerPoint;
            pathFigure.Segments.Add(new LineSegment(startPoint, false));
            pathFigure.Segments.Add(arc);

            PathGeometry pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(pathFigure);

            Path path = new Path();
            path.Stroke = this.Stroke;
            path.Tag = label;
            path.Data = pathGeometry;

            Trigger trigger = new Trigger { Property = Path.IsMouseOverProperty, Value = true };
            trigger.Setters.Add(new Setter(Path.FillProperty, ClientUtils.ChartHighlightBrush));
            Style mouseOverStyle = new Style(typeof(Path));
            mouseOverStyle.Setters.Add(new Setter(Path.FillProperty, fill));
            mouseOverStyle.Triggers.Add(trigger);
            path.Style = mouseOverStyle;

            path.MouseDown += new MouseButtonEventHandler(path_MouseDown);

            path.ToolTip = string.Format(CultureInfo.CurrentCulture,
                "{0} ({1})",
                label,
                Math.Round(data, this.DecimalPlaces));

            pieCanvas.Children.Add(path);

            return path;
        }

        private Shape AddSingleDataPoint(SolidColorBrush fill, string label, double data)
        {
            // note, asumes canvas is always square
            double center = pieCanvas.Width / 2;
            double radius = center * RadiusFactor;

            Ellipse elipse = new Ellipse();
            elipse.Width = radius * 2;
            elipse.Height = radius * 2;
            elipse.Stroke = this.Stroke;
            elipse.Tag = label;
            elipse.ToolTip = string.Format(CultureInfo.CurrentCulture,
                "{0} ({1})",
                label,
                Math.Round(data, this.DecimalPlaces));

            // style to invert fill color on mouse over
            Trigger trigger = new Trigger { Property = Ellipse.IsMouseOverProperty, Value = true };
            trigger.Setters.Add(new Setter(Rectangle.FillProperty, ClientUtils.ChartHighlightBrush));
            Style mouseOverStyle = new Style(typeof(Ellipse));
            mouseOverStyle.Setters.Add(new Setter(Ellipse.FillProperty, fill));
            mouseOverStyle.Triggers.Add(trigger);
            elipse.Style = mouseOverStyle;

            elipse.MouseDown += new MouseButtonEventHandler(elipse_MouseDown);

            pieCanvas.Children.Add(elipse);

            Canvas.SetTop(elipse, center - radius);
            Canvas.SetLeft(elipse, center - radius);

            return elipse;
        }

        private void AddLegend(Shape fillSource, string text, ref double width, ref double height, ref double nextY)
        {
            string truncatedLabel = text;
            if (truncatedLabel.Length > this.MaxLabelLength)
            {
                truncatedLabel = truncatedLabel.Substring(0, this.MaxLabelLength - 3) + "...";
            }

            TextBlock textBlock = new TextBlock(new Run(truncatedLabel));
            textBlock.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));

            Rectangle rectangle = new Rectangle();
            rectangle.Width = textBlock.DesiredSize.Height;
            rectangle.Height = textBlock.DesiredSize.Height;
            rectangle.Stroke = this.Stroke;

            // bind the legeng color to the shape it refers to
            Binding fillBinding = new Binding("Fill");
            fillBinding.Source = fillSource;
            rectangle.SetBinding(Rectangle.FillProperty, fillBinding);

            legendCanvas.Children.Add(textBlock);
            legendCanvas.Children.Add(rectangle);

            Canvas.SetTop(rectangle, nextY);
            Canvas.SetLeft(rectangle, LegendPadding);

            Canvas.SetTop(textBlock, nextY);
            Canvas.SetLeft(textBlock, textBlock.DesiredSize.Height + (2 * LegendPadding));

            nextY += (textBlock.DesiredSize.Height + LegendPadding);
            height += (textBlock.DesiredSize.Height + LegendPadding);

            double currentWidth = textBlock.DesiredSize.Height + textBlock.DesiredSize.Width + (2 * LegendPadding);
            if (currentWidth > width)
            {
                width = currentWidth;
            }
        }

        void elipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                Ellipse ellipse = sender as Ellipse;
                if (ellipse != null)
                {
                    RaiseSegmentDoubleClick(ellipse.Tag as string);
                }
            }
        }

        void path_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                Path path = sender as Path;
                if (path != null)
                {
                    RaiseSegmentDoubleClick(path.Tag as string);
                }
            }
        }

        private void RaiseSegmentDoubleClick(string segmentLegend)
        {
            if (SegmentDoubleClick != null)
            {
                SegmentDoubleClick(this, new SegmentDoubleClickEventArgs(segmentLegend));
            }
        }
    }
}
