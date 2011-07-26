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
using System.Collections.ObjectModel;
using StackHash.StackHashService;
using System.Globalization;
using System.Xml;
using System.Collections;
using System.ComponentModel;
using StackHashUtilities;
using System.Collections.Specialized;

namespace StackHash
{
    /// <summary>
    /// Events args for a country double click event
    /// </summary>
    public class CountryDoubleClickEventArgs : EventArgs
    {
        /// <summary>
        /// The locale of the double clicked country
        /// </summary>
        public string Locale { get; private set; }

        /// <summary>
        /// Events args for a country double click event
        /// </summary>
        /// <param name="locale">The locale of the double clicked country</param>
        public CountryDoubleClickEventArgs(string locale)
        {
            this.Locale = locale;
        }
    }

    /// <summary>
    /// Displays WinQual event hit density on a world map
    /// </summary>
    /// <remarks>
    /// Map canvas is generated using R:\cucku\prototypes\robe\UI Prototypes\MapTest
    /// FipsMap.xml is generated using R:\cucku\prototypes\robe\UI Prototypes\FipsConvert
    /// </remarks>
    public partial class WorldMapControl : UserControl
    {
        private class CountryInfo : INotifyPropertyChanged
        {
            private static readonly SolidColorBrush CountryGreenBrush;

            private const string FipsMapUri = "FipsMap.xml";
            private const string ElementLocaleInfo = "LocaleInfo";
            private const string AttributeFips = "shfips";
            private const string AttributeLocale = "locale";
            private const string AttributeCountryName = "name";

            public string Fips { get; private set; }
            public string LocaleSearchString { get; private set; }
            public string CountryName { get; private set; }
            public string ToolTip { get; private set; }
            public ulong EventCount { get; set; }
            public SolidColorBrush Fill { get; private set; }
            private SolidColorBrush _fillNoHighlight;

            static CountryInfo()
            {
                Color countryGreenColor = new Color();
                countryGreenColor.R = 225;
                countryGreenColor.G = 255;
                countryGreenColor.B = 225;
                countryGreenColor.A = 255;

                CountryGreenBrush = new SolidColorBrush(countryGreenColor);
            }

            public CountryInfo(string fips, string countryName, string[] localeList)
            {
                Debug.Assert(!string.IsNullOrEmpty(fips));
                Debug.Assert(!string.IsNullOrEmpty(countryName));
                Debug.Assert(localeList != null);

                this.Fips = fips;
                this.CountryName = countryName;
                this.Fill = CountryGreenBrush;
                _fillNoHighlight = CountryGreenBrush;

                if (localeList.Length == 1)
                {
                    this.LocaleSearchString = string.Format(CultureInfo.InvariantCulture, "locale:{0}", localeList[0]);
                }
                else
                {
                    StringBuilder searchBuilder = new StringBuilder();
                    for (int i = 0; i < localeList.Length; i++)
                    {
                        searchBuilder.AppendFormat(CultureInfo.InvariantCulture, "(locale:{0})", localeList[i]);
                        if (i < (localeList.Length - 1))
                        {
                            searchBuilder.Append(" OR ");
                        }
                    }
                    this.LocaleSearchString = searchBuilder.ToString();
                }
            }

            public void MouseOverHighlight(bool isMouseOver)
            {
                // only toggle the color if we have data to display
                if (this.EventCount > 0)
                {
                    if (isMouseOver)
                    {
                        this.Fill = ClientUtils.ChartHighlightBrush;
                    }
                    else
                    {
                        this.Fill = _fillNoHighlight;
                    }
                    OnPropertyChanged("Fill");
                }
            }

            public void SetMaxEventsAndUpdate(ulong maxEvents)
            {
                string newToolTip = null;
                SolidColorBrush newFill = null;

                if (this.EventCount > 0)
                {
                    Color fillColor = new Color();
                    fillColor.ScA = 1.0F;
                    fillColor.ScR = 1.0F;
                    float greenAndBlue = (1.0F - ((float)((double)this.EventCount / (double)maxEvents))) * 0.75F;
                    fillColor.ScG = greenAndBlue;
                    fillColor.ScB = greenAndBlue;
                    newFill = new SolidColorBrush(fillColor);

                    newToolTip = string.Format(CultureInfo.CurrentCulture,
                        Properties.Resources.WorldMapControl_ToolTipTemplate,
                        this.CountryName,
                        this.EventCount,
                        this.EventCount == 1 ? Properties.Resources.ErrorReport : Properties.Resources.ErrorReports);
                }
                else
                {
                    newToolTip = null;
                    newFill = CountryGreenBrush;
                }

                _fillNoHighlight = newFill;

                if (this.ToolTip != newToolTip)
                {
                    this.ToolTip = newToolTip;
                    OnPropertyChanged("ToolTip");
                }

                if (this.Fill != newFill)
                {
                    this.Fill = newFill;
                    OnPropertyChanged("Fill");
                }
            }

            public static void LoadDictionaries(out Dictionary<string, CountryInfo> fipsToCountry, 
                out Dictionary<string, CountryInfo> localeToCountry)
            {
                fipsToCountry = new Dictionary<string, CountryInfo>();
                localeToCountry = new Dictionary<string, CountryInfo>();

                char[] localeSeps = new char[] {';'};

                using (XmlReader reader = XmlReader.Create(Application.GetResourceStream(new Uri(FipsMapUri, UriKind.Relative)).Stream))
                {
                    while (reader.Read())
                    {
                        if ((reader.Name == ElementLocaleInfo) && (reader.IsStartElement()))
                        {
                            string locales = reader.GetAttribute(AttributeLocale);
                            string[] localeList = locales.Split(localeSeps, StringSplitOptions.RemoveEmptyEntries);

                            CountryInfo countryInfo = new CountryInfo(reader.GetAttribute(AttributeFips),
                                reader.GetAttribute(AttributeCountryName),
                                localeList);

                            // add to FIPS map
                            if (!fipsToCountry.ContainsKey(countryInfo.Fips))
                            {
                                fipsToCountry.Add(countryInfo.Fips, countryInfo);
                            }

                            // add each locale
                            foreach (string locale in localeList)
                            {
                                if (!localeToCountry.ContainsKey(locale))
                                {
                                    localeToCountry.Add(locale, countryInfo);
                                }
                            }
                        }
                    }
                }
            }

            #region INotifyPropertyChanged Members

            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged(string propertyName)
            {
                Debug.Assert(propertyName != null);

                if (PropertyChanged != null)
                {
                    PropertyChanged(this,
                        new PropertyChangedEventArgs(propertyName));
                }
            }

            #endregion
        }

        private Dictionary<string, CountryInfo> _fipsToCountry;
        private Dictionary<string, CountryInfo> _localeToCountry;

        /// <summary>
        /// Event fired when a country is double clicked
        /// </summary>
        public event EventHandler<CountryDoubleClickEventArgs> CountryDoubleClick;

        /// <summary>
        /// Displays WinQual event density on a world map
        /// </summary>
        public WorldMapControl()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                InitializeMap();
            }
        }

        /// <summary>
        /// Resets the map to display no data
        /// </summary>
        public void ResetMap()
        {
            Debug.Assert(_localeToCountry != null);

            foreach (CountryInfo countryInfo in _localeToCountry.Values)
            {
                countryInfo.EventCount = 0;
                countryInfo.SetMaxEventsAndUpdate(0);
            }
        }

        private void InitializeMap()
        {
            CountryInfo.LoadDictionaries(out _fipsToCountry, out _localeToCountry);
            Debug.Assert(_fipsToCountry != null);
            Debug.Assert(_localeToCountry != null);

            // set up data binding for each country that has a corresponding CountryInfo
            IEnumerable children = LogicalTreeHelper.GetChildren(canvasWorld);
            foreach (object obj in children)
            {
                Path path = obj as Path;
                if (path != null)
                {
                    if (_fipsToCountry.ContainsKey(path.Name))
                    {
                        CountryInfo countryInfo = _fipsToCountry[path.Name];

                        BindingOperations.SetBinding(path, Path.ToolTipProperty, new Binding("ToolTip"));
                        BindingOperations.SetBinding(path, Path.FillProperty, new Binding("Fill"));
                        path.DataContext = countryInfo;
                        path.MouseDown += new MouseButtonEventHandler(path_MouseDown);
                        path.MouseEnter += new MouseEventHandler(path_MouseEnter);
                        path.MouseLeave += new MouseEventHandler(path_MouseLeave);
                    }
                }
            }
        }

        void path_MouseLeave(object sender, MouseEventArgs e)
        {
            Path path = e.OriginalSource as Path;
            if (path != null)
            {
                CountryInfo countryInfo = path.DataContext as CountryInfo;
                if ((countryInfo != null) && (countryInfo.EventCount > 0))
                {
                    countryInfo.MouseOverHighlight(false);
                }
            }
        }

        void path_MouseEnter(object sender, MouseEventArgs e)
        {
            Path path = e.OriginalSource as Path;
            if (path != null)
            {
                CountryInfo countryInfo = path.DataContext as CountryInfo;
                if ((countryInfo != null) && (countryInfo.EventCount > 0))
                {
                    countryInfo.MouseOverHighlight(true);
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
                    CountryInfo countryInfo = path.DataContext as CountryInfo;
                    if (countryInfo != null)
                    {
                        if (countryInfo.EventCount > 0)
                        {
                            RaiseCountryDoubleClick(countryInfo.LocaleSearchString);
                        }
                    }
                }
            }
        }

        private void RaiseCountryDoubleClick(string locale)
        {
            if (CountryDoubleClick != null)
            {
                CountryDoubleClick(this, new CountryDoubleClickEventArgs(locale));
            }
        }

        /// <summary>
        /// Update the map from a list of event packages
        /// </summary>
        /// <param name="localeSummary">Locale summary information</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        public void UpdateMap(StackHashProductLocaleSummaryCollection localeSummary)
        {
            Debug.Assert(localeSummary != null);

            ResetMap();

            if (localeSummary != null)
            {
                ulong maxEvents = 0;
                ulong currentEvents = 0;

                StringCollection localeMisses = null;

                foreach (StackHashProductLocaleSummary summary in localeSummary)
                {
                    if (!string.IsNullOrEmpty(summary.Locale))
                    {
                        string lowerLocale = summary.Locale.ToLower(CultureInfo.InvariantCulture);
                        if (_localeToCountry.ContainsKey(lowerLocale))
                        {
                            currentEvents = _localeToCountry[lowerLocale].EventCount += (ulong)summary.TotalHits;

                            if (currentEvents > maxEvents)
                            {
                                maxEvents = currentEvents;
                            }
                        }
                        else
                        {
                            if (localeMisses == null)
                            {
                                localeMisses = new StringCollection();
                            }

                            if ((!localeMisses.Contains(lowerLocale)) &&
                                (lowerLocale != "none"))
                            {
                                localeMisses.Add(lowerLocale);
                            }
                        }
                    }
                }

                // log any locales not on the map
                if (localeMisses != null)
                {
                    foreach (string missedLocale in localeMisses)
                    {
                        DiagnosticsHelper.LogMessage(DiagSeverity.Information,
                            string.Format(CultureInfo.InvariantCulture,
                            "WorldMapControl: no country is associated with culture {0}",
                            missedLocale));
                    }
                }

                // update the map by setting the highest event count
                foreach (CountryInfo countryInfo in _localeToCountry.Values)
                {
                    countryInfo.SetMaxEventsAndUpdate(maxEvents);
                }
            }
        }
    }
}
