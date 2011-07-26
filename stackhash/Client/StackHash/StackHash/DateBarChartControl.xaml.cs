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
using System.Globalization;
using System.Windows.Threading;
using System.Diagnostics;

namespace StackHash
{
    /// <summary>
    /// Chart control showing a horizontal bar chart over a range of dates
    /// </summary>
    public partial class DateBarChartControl : UserControl
    {
        private static int[] ScaleDays = new int[] { 30, 60, 90, 365 };
        private static int[] ScaleScroll = new int[] { 1, 3, 9, 30 };
        private const double YLegendMargin = 6.0;

        /// <summary>
        /// Event fired when a bar is double-clicked
        /// </summary>
        public event EventHandler<SearchForDateEventArgs> SearchForDate;

        /// <summary>
        /// Gets or sets the stroke brush
        /// </summary>
        public SolidColorBrush Stroke { get; set; }

        /// <summary>
        /// Gets or sets the fill brush
        /// </summary>
        public SolidColorBrush Fill { get; set; }

        /// <summary>
        /// Gets or sets the chart title
        /// </summary>
        public string ChartTitle { get; set; }

        /// <summary>
        /// True for the product page, false for the event page
        /// </summary>
        public bool IsProductPageChart { get; set; }

        private Dictionary<DateTime, ulong> _data;
        private DateTime _startDate;
        private DateTime _startDateDisplay;
        private DateTime _endDate;
        private DateTime _endDateDisplay;
        private ulong _maxVal;
        private int _autoScrollBy;
        private bool _scrollEnabled;
        private DispatcherTimer _timer;
        private int _scaleIndex;
        private int _maxDays;
        private int _scrollDays;

        /// <summary>
        /// Chart control showing a horizontal bar chart over a range of dates
        /// </summary>
        public DateBarChartControl()
        {
            InitializeComponent();

            _maxDays = ScaleDays[0];
            _scrollDays = ScaleScroll[0];

            this.Stroke = Brushes.Black;
            this.Fill = new SolidColorBrush(Color.FromRgb(69, 111, 167));
            this.ChartTitle = string.Empty;

            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 1);
            _timer.Tick += new EventHandler(_timer_Tick);
        }

        /// <summary>
        /// Sets data for the chart
        /// </summary>
        /// <param name="data">data</param>
        public void SetData(Dictionary<DateTime, ulong> data)
        {
            _data = data;

            if (this.IsProductPageChart)
            {
                _scaleIndex = UserSettings.Settings.HitScaleProduct;
            }
            else
            {
                _scaleIndex = UserSettings.Settings.HitScaleEvent;
            }

            if (_scaleIndex >= ScaleDays.Length)
            {
                _scaleIndex = 0;
            }

            _maxDays = ScaleDays[_scaleIndex];
            _scrollDays = ScaleScroll[_scaleIndex];

            PlotChart(true);
        }

        /// <summary>
        /// Clears any data in the chart
        /// </summary>
        public void ClearData()
        {
            _data = null;
            PlotChart(true);
        }

        private void gridChart_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            canvasChart.Width = gridChart.ActualWidth;
            canvasChart.Height = gridChart.ActualHeight;

            PlotChart(false);
        }

        private void PlotChart(bool updateDates)
        {
            canvasChart.Children.Clear();
            canvasYAxis.Children.Clear();
            textDateStart.Text = string.Empty;
            textDateEnd.Text = string.Empty;
            textBlockBack.Visibility = Visibility.Collapsed;
            textBlockNext.Visibility = Visibility.Collapsed;
            textBlockScaleContent.Text = string.Empty;

            textTitle.Text = this.ChartTitle;

            if ((_data == null) || (_data.Count == 0))
            {
                return;
            }

            if (updateDates)
            {
                DateTime startRange = DateTime.MaxValue;
                DateTime endRange = DateTime.MinValue;
                _maxVal = 0;

                foreach (DateTime date in _data.Keys)
                {
                    if (date < startRange)
                    {
                        startRange = date;
                    }

                    if (date > endRange)
                    {
                        endRange = date;
                    }

                    if (_data[date] > _maxVal)
                    {
                        _maxVal = _data[date];
                    }
                }

                if ((_maxVal == 0) ||
                    (startRange == DateTime.MaxValue) ||
                    (endRange == DateTime.MinValue))
                {
                    return;
                }

                _startDate = startRange;
                _endDate = DateTime.Today;

                _endDateDisplay = _endDate;
                _startDateDisplay = _endDate.AddDays(-_maxDays);

                TimeSpan totalRange = _endDate - _startDate;
                _scrollEnabled = (Math.Ceiling(totalRange.TotalDays) > _maxDays);
            }

            // y axis
            TextBlock textBlockMax = new TextBlock(new Run(_maxVal.ToString("n0", CultureInfo.CurrentCulture)));
            TextBlock textBlockMin = new TextBlock(new Run("0"));

            Size infinity = new Size(double.PositiveInfinity, double.PositiveInfinity);
            textBlockMax.Measure(infinity);
            textBlockMin.Measure(infinity);

            textBlockMin.TextAlignment = TextAlignment.Right;
            textBlockMax.TextAlignment = TextAlignment.Right;
            textBlockMin.Margin = new Thickness(0, 0, YLegendMargin, 0);
            textBlockMax.Margin = new Thickness(0, 0, YLegendMargin, 0);

            Double yAxisWidth = textBlockMax.DesiredSize.Width;
            if (textBlockMin.DesiredSize.Width > yAxisWidth)
            {
                yAxisWidth = textBlockMin.DesiredSize.Width;
            }

            columnYAxis.Width = new GridLength(yAxisWidth + (YLegendMargin * 2));
            textBlockMax.Width = yAxisWidth + YLegendMargin;
            textBlockMin.Width = yAxisWidth + YLegendMargin;
            canvasYAxis.Children.Add(textBlockMin);
            canvasYAxis.Children.Add(textBlockMax);
            Canvas.SetTop(textBlockMax, 0);
            Canvas.SetLeft(textBlockMax, 0);
            Canvas.SetBottom(textBlockMin, 0);
            Canvas.SetLeft(textBlockMin, 0);

            TimeSpan range = _endDateDisplay - _startDateDisplay;
            int days = (int)range.TotalDays;

            Double barWidth = (canvasChart.Width / (days + 1));
            Double heightPerHit = canvasChart.Height / _maxVal;
            Double currentBarX = 0.0;

            // style to invert fill color on mouse over
            Trigger trigger = new Trigger { Property=Rectangle.IsMouseOverProperty, Value=true };
            trigger.Setters.Add(new Setter(Rectangle.FillProperty, ClientUtils.ChartHighlightBrush));
            Style mouseOverStyle = new Style(typeof(Rectangle));
            mouseOverStyle.Setters.Add(new Setter(Rectangle.FillProperty, this.Fill));
            mouseOverStyle.Triggers.Add(trigger);

            DateTime current = DateTime.MinValue;
            for (int day = 0; day <= days; day++)
            {
                current = _startDateDisplay.AddDays(day);

                if (_data.ContainsKey(current))
                {
                    Rectangle rectangle = new Rectangle();
                    rectangle.Stroke = this.Stroke;
                    rectangle.Width = barWidth;
                    rectangle.Height = _data[current] * heightPerHit;
                    rectangle.Tag = current;
                    rectangle.Style = mouseOverStyle;

                    rectangle.ToolTip = string.Format(CultureInfo.CurrentCulture,
                        "{0} ({1})",
                        current.ToShortDateString(),
                        _data[current]);

                    rectangle.MouseDown += new MouseButtonEventHandler(rectangle_MouseDown);

                    canvasChart.Children.Add(rectangle);
                    Canvas.SetBottom(rectangle, 0);
                    Canvas.SetLeft(rectangle, currentBarX);
                }

                currentBarX += barWidth;
            }

            // axis lines
            Rectangle xAxisRect = new Rectangle();
            xAxisRect.Width = canvasChart.Width;
            xAxisRect.Height = 1;
            xAxisRect.Stroke = this.Stroke;

            Rectangle yAxisRect = new Rectangle();
            yAxisRect.Height = canvasChart.Height;
            yAxisRect.Width = 1;
            yAxisRect.Stroke = this.Stroke;

            canvasChart.Children.Add(xAxisRect);
            canvasChart.Children.Add(yAxisRect);
            Canvas.SetLeft(xAxisRect, 0);
            Canvas.SetBottom(xAxisRect, 0);
            Canvas.SetLeft(yAxisRect, 0);
            Canvas.SetTop(yAxisRect, 0);

            textDateStart.Text = _startDateDisplay.ToShortDateString();
            textDateEnd.Text = _endDateDisplay.ToShortDateString();

            UpdateLinkStates();
            UpdateScaleLinkText();
        }

        void rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                Rectangle rect = sender as Rectangle;
                if (rect != null)
                {
                    DateTime date = (DateTime)rect.Tag;
                    RaiseSearchForDate(date);
                }
            }
        }

        private void RaiseSearchForDate(DateTime date)
        {
            if (SearchForDate != null)
            {
                SearchForDate(this, new SearchForDateEventArgs(date));
            }
        }

        private void linkBack_Click(object sender, RoutedEventArgs e)
        {
            _startDateDisplay = _startDateDisplay.AddDays(-_scrollDays);
            _endDateDisplay = _endDateDisplay.AddDays(-_scrollDays);
            PlotChart(false);
        }

        private void linkBack_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                _startDateDisplay = _startDate;
                _endDateDisplay = _startDate.AddDays(_maxDays);
                PlotChart(false);
                e.Handled = true;
            }
            else
            {
                _autoScrollBy = -_scrollDays;
                _timer.Interval = new TimeSpan(0, 0, 0, 1);
                _timer.Start();
            }
        }

        private void linkBack_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _autoScrollBy = 0;
            _timer.Stop();
        }

        private void linkNext_Click(object sender, RoutedEventArgs e)
        {
            _startDateDisplay = _startDateDisplay.AddDays(_scrollDays);
            _endDateDisplay = _endDateDisplay.AddDays(_scrollDays);
            PlotChart(false);
        }

        private void linkNext_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                _startDateDisplay = _endDate.AddDays(-_maxDays);
                _endDateDisplay = _endDate;
                PlotChart(false);
                e.Handled = true;
            }
            else
            {
                _autoScrollBy = _scrollDays;
                _timer.Interval = new TimeSpan(0, 0, 0, 1);
                _timer.Start();
            }
        }

        private void linkNext_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _autoScrollBy = 0;
            _timer.Stop();
        }

        void _timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();

            _startDateDisplay = _startDateDisplay.AddDays(_autoScrollBy);
            _endDateDisplay = _endDateDisplay.AddDays(_autoScrollBy);
            PlotChart(false);

            bool startTimer = false;

            if ((_autoScrollBy > 0) && (linkNext.IsEnabled))
            {
                startTimer = true;
            }
            else if ((_autoScrollBy < 0) && (linkBack.IsEnabled))
            {
                startTimer = true;
            }

            if (startTimer)
            {
                _timer.Interval = new TimeSpan(0, 0, 0, 0, 200);
                _timer.Start();
            }
        }

        private void linkScale_Click(object sender, RoutedEventArgs e)
        {
            _scaleIndex++;
            if (_scaleIndex >= ScaleDays.Length)
            {
                _scaleIndex = 0;
            }

            _maxDays = ScaleDays[_scaleIndex];
            _scrollDays = ScaleScroll[_scaleIndex];

            if (this.IsProductPageChart)
            {
                UserSettings.Settings.HitScaleProduct = _scaleIndex;
            }
            else
            {
                UserSettings.Settings.HitScaleEvent = _scaleIndex;
            }

            PlotChart(true);
        }

        private void UpdateScaleLinkText()
        {
            textBlockScaleContent.Text = string.Format(CultureInfo.CurrentCulture,
                Properties.Resources.DateBarChartControl_ScaleText,
                _maxDays);
        }

        private void UpdateLinkStates()
        {
            if (_scrollEnabled)
            {
                if (_startDateDisplay > _startDate)
                {
                    textBlockBack.Visibility = Visibility.Visible;
                }
                else
                {
                    textBlockBack.Visibility = Visibility.Hidden;
                }

                if (_endDateDisplay < _endDate)
                {
                    textBlockNext.Visibility = Visibility.Visible;
                }
                else
                {
                    textBlockNext.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                textBlockBack.Visibility = Visibility.Hidden;
                textBlockNext.Visibility = Visibility.Hidden;
            }
        }
    }

    /// <summary>
    /// Event args to request a search for a date
    /// </summary>
    public class SearchForDateEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the date to search for
        /// </summary>
        public DateTime Date { get; private set; }

        /// <summary>
        /// Event args to request a search for a date
        /// </summary>
        /// <param name="date">Date to search for</param>
        public SearchForDateEventArgs(DateTime date)
        {
            this.Date = date;
        }
    }
}
