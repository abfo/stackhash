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
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using StackHash.StackHashService;
using System.Collections.Specialized;
using System.Globalization;
using System.Diagnostics;

namespace StackHash
{
    /// <summary>
    /// Window for graphically building a search, class also contains static methods for manipulating search strings
    /// </summary>
    public partial class SearchBuilder : Window
    {
        private static string WindowKey = "SearchBuilder";

        #region private classes / static helper arrays

        private static readonly char[] InvalidSearchChars = new char[] { '"', '(', ')', '=', '>', '<', ':', '[', ']', '!' };
        private static readonly char[] SearchSeps = new char[] { '=', '>', '<', ':', '!' };
        private static readonly char[] SearchTrimChars = new char[] { ' ', '"' };
        private static readonly char[] SpaceChars = new char[] { ' ' };

        private class SearchFieldInfo
        {
            public string DisplayName { get; private set; }
            public string SearchName { get; private set; }
            public string FieldName {get; private set;}
            public StackHashSearchFieldType FieldType { get; private set; }
            public StackHashObjectType ObjectType { get; private set; }

            public SearchFieldInfo(string displayName, string searchName, string fieldName, StackHashSearchFieldType fieldType, StackHashObjectType objectType)
            {
                this.DisplayName = displayName;
                this.SearchName = searchName;
                this.FieldName = fieldName;
                this.FieldType = fieldType;
                this.ObjectType = objectType;
            }
        }

        private static readonly SearchFieldInfo[] SearchFields = new SearchFieldInfo[] {
            new SearchFieldInfo(Properties.Resources.FieldNameAll,          "*",            "*",                    StackHashSearchFieldType.String,    StackHashObjectType.Event),
            new SearchFieldInfo(Properties.Resources.FieldNameEvendId,      "eid",          "Id",                   StackHashSearchFieldType.Long,      StackHashObjectType.Event),
            new SearchFieldInfo(Properties.Resources.FieldNameEventReference, "eref",       "BugId",                StackHashSearchFieldType.String,    StackHashObjectType.Event),
            new SearchFieldInfo(Properties.Resources.FieldNameEventPluginReference, "epref", "PlugInBugId",         StackHashSearchFieldType.String,    StackHashObjectType.Event),
            new SearchFieldInfo(Properties.Resources.FieldNameEventStatus,  "estatus",      "WorkFlowStatusName",   StackHashSearchFieldType.String,    StackHashObjectType.Event),
            new SearchFieldInfo(Properties.Resources.FieldNameEventType,    "etype",        "EventTypeName",        StackHashSearchFieldType.String,    StackHashObjectType.Event),
            new SearchFieldInfo(Properties.Resources.FieldNameEventCabs,    "ecabs",        "CabCount",             StackHashSearchFieldType.Integer,   StackHashObjectType.Event),
            new SearchFieldInfo(Properties.Resources.FieldNameEventHits,    "ehits",        "TotalHits",            StackHashSearchFieldType.Integer,   StackHashObjectType.Event),
            new SearchFieldInfo(Properties.Resources.FieldNameEventCreated, "ecreated",     "DateCreatedLocal",     StackHashSearchFieldType.DateTime,  StackHashObjectType.Event),
            new SearchFieldInfo(Properties.Resources.FieldNameEventModified,"emodified",    "DateModifiedLocal",    StackHashSearchFieldType.DateTime,  StackHashObjectType.Event),
            new SearchFieldInfo(Properties.Resources.FieldNameEventNote,    "enote",        "Note",                 StackHashSearchFieldType.String,    StackHashObjectType.EventNotes),
            new SearchFieldInfo(Properties.Resources.FieldNameEventNoteUser,"enoteu",       "User",                 StackHashSearchFieldType.String,    StackHashObjectType.EventNotes),
            new SearchFieldInfo(Properties.Resources.FieldNameAppName,      "app",          "ApplicationName",      StackHashSearchFieldType.String,    StackHashObjectType.EventSignature), 
            new SearchFieldInfo(Properties.Resources.FieldNameAppLink,      "applink",      "ApplicationTimeStamp", StackHashSearchFieldType.DateTime,  StackHashObjectType.EventSignature), 
            new SearchFieldInfo(Properties.Resources.FieldNameAppVer,       "appver",       "ApplicationVersion",   StackHashSearchFieldType.String,    StackHashObjectType.EventSignature),
            new SearchFieldInfo(Properties.Resources.FieldNameModName,      "mod",          "ModuleName",           StackHashSearchFieldType.String,    StackHashObjectType.EventSignature), 
            new SearchFieldInfo(Properties.Resources.FieldNameModLink,      "modlink",      "ModuleTimeStamp",      StackHashSearchFieldType.DateTime,  StackHashObjectType.EventSignature),
            new SearchFieldInfo(Properties.Resources.FieldNameModVer,       "modver",       "ModuleVersion",        StackHashSearchFieldType.String,    StackHashObjectType.EventSignature), 
            new SearchFieldInfo(Properties.Resources.FieldNameException,    "exception",    "ExceptionCode",        StackHashSearchFieldType.Long,      StackHashObjectType.EventSignature), 
            new SearchFieldInfo(Properties.Resources.FieldNameOffset,       "offset",       "Offset",               StackHashSearchFieldType.Long,      StackHashObjectType.EventSignature), 
            new SearchFieldInfo(Properties.Resources.FieldNameCabId,        "cid",          "Id",                   StackHashSearchFieldType.Long,      StackHashObjectType.CabInfo),
            new SearchFieldInfo(Properties.Resources.FieldNameCabSize,      "csize",        "SizeInBytes",          StackHashSearchFieldType.Long,      StackHashObjectType.CabInfo),
            new SearchFieldInfo(Properties.Resources.FieldNameCabName,      "cname",        "FileName",             StackHashSearchFieldType.String,    StackHashObjectType.CabInfo),
            new SearchFieldInfo(Properties.Resources.FieldNameCabCreated,   "ccreated",     "DateCreatedLocal",     StackHashSearchFieldType.DateTime,  StackHashObjectType.CabInfo),
            new SearchFieldInfo(Properties.Resources.FieldNameCabModified,  "cmodified",    "DateModifiedLocal",    StackHashSearchFieldType.DateTime,  StackHashObjectType.CabInfo),
            new SearchFieldInfo(Properties.Resources.FieldNameCabNote,      "cnote",        "Note",                 StackHashSearchFieldType.String,    StackHashObjectType.CabNotes),
            new SearchFieldInfo(Properties.Resources.FieldNameCabNoteUser,  "cnoteu",       "User",                 StackHashSearchFieldType.String,    StackHashObjectType.CabNotes),
            new SearchFieldInfo(Properties.Resources.FieldNameOS,           "os",           "OperatingSystemName",  StackHashSearchFieldType.String,    StackHashObjectType.EventInfo),
            new SearchFieldInfo(Properties.Resources.FieldNameOSVer,        "osver",        "OperatingSystemVersion", StackHashSearchFieldType.String,  StackHashObjectType.EventInfo),
            new SearchFieldInfo(Properties.Resources.FieldNameHitLang,      "language",     "Language",             StackHashSearchFieldType.String,    StackHashObjectType.EventInfo),
            new SearchFieldInfo(Properties.Resources.FieldNameHitLcid,      "lcid",         "Lcid",                 StackHashSearchFieldType.Integer,    StackHashObjectType.EventInfo),
            new SearchFieldInfo(Properties.Resources.FieldNameHitLocale,    "locale",       "Locale",               StackHashSearchFieldType.String,    StackHashObjectType.EventInfo),
            new SearchFieldInfo(Properties.Resources.FieldNameHitDate,      "hdate",        "HitDateLocal",         StackHashSearchFieldType.DateTime,  StackHashObjectType.EventInfo),
            new SearchFieldInfo(Properties.Resources.FieldNameHitCount,     "hhits",        "TotalHits",            StackHashSearchFieldType.Integer,   StackHashObjectType.EventInfo),
            new SearchFieldInfo(Properties.Resources.FieldNameHitCreated,   "hcreated",     "DateCreatedLocal",     StackHashSearchFieldType.DateTime,  StackHashObjectType.EventInfo),
            new SearchFieldInfo(Properties.Resources.FieldNameHitModified,  "hmodified",    "DateModifiedLocal",    StackHashSearchFieldType.DateTime,  StackHashObjectType.EventInfo),
            new SearchFieldInfo(Properties.Resources.FieldNameScriptResults,"script",       "Content",              StackHashSearchFieldType.String,    StackHashObjectType.Script),
        };

        private static Dictionary<string, SearchFieldInfo> SearchNameDictionary;

        private class SearchOptionInfo
        {
            public string DisplayName { get; private set; }
            public StackHashSearchOptionType OptionType { get; private set; }

            public SearchOptionInfo(string displayName, StackHashSearchOptionType optionType)
            {
                this.DisplayName = displayName;
                this.OptionType = optionType;
            }
        }

        private static readonly SearchOptionInfo[] StringOptions = new SearchOptionInfo[] {
            new SearchOptionInfo(Properties.Resources.OptionNameContains, StackHashSearchOptionType.StringContains),
            new SearchOptionInfo(Properties.Resources.OptionNameDoesNotContain, StackHashSearchOptionType.StringDoesNotContain)
        };

        private static readonly SearchOptionInfo[] NumericOptions = new SearchOptionInfo[] {
            new SearchOptionInfo(Properties.Resources.OptionNameEquals, StackHashSearchOptionType.Equal),
            new SearchOptionInfo(Properties.Resources.OptionNameGreaterThan, StackHashSearchOptionType.GreaterThan),
            new SearchOptionInfo(Properties.Resources.OptionNameLessThan, StackHashSearchOptionType.LessThan)
        };

        private class SearchName : INotifyPropertyChanged, IDataErrorInfo
        {
            private string _name;

            public SearchName(string name)
            {
                this.Name = name;
            }

            public string Name
            {
                get { return _name; }
                set 
                {
                    if (_name != value)
                    {
                        _name = value;
                        RaisePropertyChanged("Name");
                    }
                }
            }

            #region INotifyPropertyChanged Members

            public event PropertyChangedEventHandler PropertyChanged;

            private void RaisePropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this,
                        new PropertyChangedEventArgs(propertyName));
                }
            }

            #endregion

            #region IDataErrorInfo Members

            public string Error
            {
                get { return null; }
            }

            public string this[string columnName]
            {
                get 
                { 
                    string result = null;

                    if (columnName == "Name")
                    {
                        if (!string.IsNullOrEmpty(this.Name))
                        {
                            if (this.Name.IndexOfAny(InvalidSearchChars) >= 0)
                            {
                                StringBuilder invalidChars = new StringBuilder();
                                foreach (char c in InvalidSearchChars)
                                {
                                    invalidChars.Append(' ');
                                    invalidChars.Append(c);
                                }

                                result = string.Format(CultureInfo.InvariantCulture,
                                    Properties.Resources.SearchBuilder_ValidationErrorSearchNameInvalidChars,
                                    invalidChars);
                            }
                        }
                    }

                    return result;
                }
            }

            #endregion
        }

        private class GenericSearchOption : INotifyPropertyChanged, IDataErrorInfo
        {
            private string _search;
            private SearchFieldInfo _fieldInfo;
            private SearchOptionInfo _optionInfo;
            private SearchFieldInfo[] _availableFields;
            private SearchOptionInfo[] _availableOptions;
            private SearchOptionInfo[] _stringOptions;
            private SearchOptionInfo[] _numericOptions;

            public GenericSearchOption(SearchFieldInfo[] availableFields, SearchOptionInfo[] stringOptions, SearchOptionInfo[] numericOptions)
            {
                _availableFields = availableFields;
                _stringOptions = stringOptions;
                _numericOptions = numericOptions;

                this.FieldInfo = _availableFields[0];
                this.Search = string.Empty;
            }

            public SearchFieldInfo[] AvailableFields
            {
                get { return _availableFields; }
            }

            public SearchOptionInfo[] AvailableOptions
            {
                get { return _availableOptions; }
                private set
                {
                    if (_availableOptions != value)
                    {
                        _availableOptions = value;
                        RaisePropertyChanged("AvailableOptions");
                    }
                }
            }

            public SearchFieldInfo FieldInfo
            {
                get { return _fieldInfo; }
                set 
                {
                    if (_fieldInfo != value)
                    {
                        _fieldInfo = value;

                        switch (_fieldInfo.FieldType)
                        {
                            case StackHashSearchFieldType.String:
                                this.AvailableOptions = _stringOptions;
                                this.OptionInfo = this.AvailableOptions[0];
                                break;

                            default:
                                this.AvailableOptions = _numericOptions;
                                this.OptionInfo = this.AvailableOptions[0];
                                break;
                        }

                        RaisePropertyChanged("FieldInfo");
                    }
                }
            }

            public SearchOptionInfo OptionInfo
            {
                get { return _optionInfo; }
                set 
                {
                    if (_optionInfo != value)
                    {
                        _optionInfo = value;
                        RaisePropertyChanged("OptionInfo");
                    }
                }
            }

            public string Search
            {
                get { return _search; }
                set 
                {
                    if (_search != value)
                    {
                        _search = value;
                        RaisePropertyChanged("Search");
                    }
                }
            }

            #region INotifyPropertyChanged Members

            public event PropertyChangedEventHandler PropertyChanged;

            private void RaisePropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this,
                        new PropertyChangedEventArgs(propertyName));
                }
            }

            #endregion

            #region IDataErrorInfo Members

            public string Error
            {
                get { return null; }
            }

            public string this[string columnName]
            {
                get 
                {
                    string result = null;

                    if (columnName == "Search")
                    {
                        if (string.IsNullOrEmpty(this.Search))
                        {
                            // we'll ignore empty fields
                            result = null;
                        }
                        else if (this.Search.IndexOfAny(InvalidSearchChars) >= 0)
                        {
                            StringBuilder invalidChars = new StringBuilder();
                            foreach (char c in InvalidSearchChars)
                            {
                                invalidChars.Append(' ');
                                invalidChars.Append(c);
                            }

                            result = string.Format(CultureInfo.InvariantCulture,
                                Properties.Resources.SearchBuilder_ValidationErrorSearchInvalidChars,
                                invalidChars);
                        }
                        else
                        {
                            switch (this.FieldInfo.FieldType)
                            {
                                case StackHashSearchFieldType.DateTime:
                                    DateTime converted;
                                    if (!TryParseDateTime(this.Search, out converted))
                                    {
                                        result = Properties.Resources.SearchBuilder_ValidationErrorSearchDateTime;
                                    }
                                    break;

                                case StackHashSearchFieldType.Integer:
                                    int int32;
                                    if (!TryParseInt32(this.Search, out int32))
                                    {
                                        result = Properties.Resources.SearchBuilder_ValidationErrorSearchNumber;
                                    }
                                    break;

                                case StackHashSearchFieldType.Long:
                                    long int64;
                                    if (!TryParseInt64(this.Search, out int64))
                                    {
                                        result = Properties.Resources.SearchBuilder_ValidationErrorSearchNumber;
                                    }
                                    break;
                            }
                        }
                    }

                    return result;
                }
            }

            #endregion
        }

        private class GenericSearch
        {
            public ObservableCollection<GenericSearchOption> Options { get; private set; }

            public GenericSearch(SearchFieldInfo[] availableFields, SearchOptionInfo[] stringOptions, SearchOptionInfo[] numericOptions)
            {
                this.Options = new ObservableCollection<GenericSearchOption>();
                this.Options.Add(new GenericSearchOption(availableFields, stringOptions, numericOptions));
            }
        }

        private class AllFieldSearchBuilder
        {
            public List<StringSearchOption> EventSearches { get; private set; }
            public List<StringSearchOption> EventInfoSearches { get; private set; }
            public List<StringSearchOption> EventSignatureSearches { get; private set; }
            public List<StringSearchOption> CabSearches { get; private set; }
            public List<StringSearchOption> EventWorkFlowSearches { get; private set; }

            public AllFieldSearchBuilder()
            {
                this.EventSearches = new List<StringSearchOption>();
                this.EventInfoSearches = new List<StringSearchOption>();
                this.EventSignatureSearches = new List<StringSearchOption>();
                this.CabSearches = new List<StringSearchOption>();
                this.EventWorkFlowSearches = new List<StringSearchOption>();
            }

            public void AddSearch(string search)
            {
                this.EventSearches.Add(CreateOption(search, StackHashObjectType.Event));
                this.EventInfoSearches.Add(CreateOption(search, StackHashObjectType.EventInfo));
                this.EventSignatureSearches.Add(CreateOption(search, StackHashObjectType.EventSignature));
                this.CabSearches.Add(CreateOption(search, StackHashObjectType.CabInfo));
                this.EventWorkFlowSearches.Add(CreateOption(search, StackHashObjectType.EventWorkFlow));
            }

            public StackHashSearchCriteria[] CreateCriteriaArray(StackHashSearchCriteria additionalCriteria)
            {
                List<StackHashSearchCriteria> returnList = new List<StackHashSearchCriteria>();

                returnList.Add(CreateCriteria(this.EventSearches, additionalCriteria));
                returnList.Add(CreateCriteria(this.EventInfoSearches, additionalCriteria));
                returnList.Add(CreateCriteria(this.EventSignatureSearches, additionalCriteria));
                returnList.Add(CreateCriteria(this.CabSearches, additionalCriteria));
                returnList.Add(CreateCriteria(this.EventWorkFlowSearches, additionalCriteria));

                return returnList.ToArray();
            }

            private static StackHashSearchCriteria CreateCriteria(List<StringSearchOption> allFieldOptions, StackHashSearchCriteria additionalCriteria)
            {
                StackHashSearchCriteria criteria = new StackHashSearchCriteria();
                criteria.SearchFieldOptions = new StackHashSearchOptionCollection();

                foreach(StringSearchOption allFieldOption in allFieldOptions)
                {
                    criteria.SearchFieldOptions.Add(allFieldOption);
                }

                foreach (StackHashSearchOption additionalOption in additionalCriteria.SearchFieldOptions)
                {
                    criteria.SearchFieldOptions.Add(additionalOption);
                }

                return criteria;
            }

            private static StringSearchOption CreateOption(string search, StackHashObjectType objectType)
            {
                StringSearchOption option = new StringSearchOption();
                option.CaseSensitive = false;
                option.FieldName = "*";
                option.ObjectType = objectType;
                option.SearchOptionType = StackHashSearchOptionType.StringContains;
                option.SearchType = StackHashSearchFieldType.String;
                option.Start = search;
                return option;
            }
        }

        #endregion

        private ObservableCollection<GenericSearch> _criteria;
        private string _searchString;
        private SearchName _searchName;

        /// <summary>
        /// Window for graphically building a search
        /// </summary>
        public SearchBuilder(string existingSearch)
        {
            InitializeComponent();

            if (UserSettings.Settings.HaveWindow(WindowKey))
            {
                UserSettings.Settings.RestoreWindow(WindowKey, this);
            }

            InitializeFromExistingSearch(existingSearch); // ok for existingSearch to be null
        }

        /// <summary>
        /// Gets the search string
        /// </summary>
        public string SearchString
        {
            get { return _searchString; }
            private set 
            {
                if (_searchString != value)
                {
                    _searchString = value;
                    textBlockSearchString.Text = _searchString;
                }
            }
        }

        #region Detect / Manipulate a saved search

        /// <summary>
        /// returns true if a string represents a saved search
        /// </summary>
        /// <param name="search">The search string</param>
        /// <returns>True if the search string represents as saved search</returns>
        public static bool IsSavedSearch(string search)
        {
            return search.Contains('[');
        }

        /// <summary>
        /// Combines a search name and search into a single search string 
        /// </summary>
        /// <param name="name">The name of the search</param>
        /// <param name="search">The search</param>
        /// <returns>Saved search string</returns>
        public static string CombineSavedSearch(string name, string search)
        {
            return string.Format(CultureInfo.CurrentCulture,
                "{0} [{1}]",
                name,
                search);
        }

        /// <summary>
        /// Cracks a saved search into a search name and a search string
        /// </summary>
        /// <param name="search">The saved search to crack</param>
        /// <param name="searchName">Returns the search name</param>
        /// <param name="searchSearch">Returns the search string</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        public static void CrackSavedSearch(string search, out string searchName, out string searchSearch)
        {
            Debug.Assert(!string.IsNullOrEmpty(search));

            searchName = null;
            searchSearch = null;

            char[] searchSplitChars = new char[] { '[', ']' };
            string[] searchSplitStrings = search.Split(searchSplitChars, StringSplitOptions.RemoveEmptyEntries);
            if (searchSplitStrings.Length == 2)
            {
                searchName = searchSplitStrings[0].Trim();
                searchSearch = searchSplitStrings[1].Trim();
            }
        }

        #endregion

        #region Convert a search string to a StackHashSearchCriteriaCollection

        /// <summary>
        /// Compiles a search string into a StackHashSearchCriteriaCollection
        /// </summary>
        /// <param name="searchString">The search string</param>
        /// <param name="product">Product to search (null to search all)</param>
        /// <param name="searchName">Returns the name of the search (null if no name)</param>
        /// <returns>StackHashSearchCriteriaCollection</returns>
        /// <exception cref="SearchParseException">Thrown if searchString could not be parsed</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public static StackHashSearchCriteriaCollection CompileSearch(string searchString, StackHashProduct product, out string searchName)
        {
            StackHashSearchCriteriaCollection criteriaCollection = new StackHashSearchCriteriaCollection();
            searchName = null;

            string search = null;

            if (IsSavedSearch(searchString))
            {
                CrackSavedSearch(searchString, out searchName, out search);
            }

            if (search == null)
            {
                search = searchString;
            }

            if (string.IsNullOrEmpty(search))
            {
                throw new SearchParseException(Properties.Resources.SearchParseException_EmptySearch);
            }

            StringCollection criteriaStrings = ExtractCriteriaStrings(search);
            foreach (string criteriaString in criteriaStrings)
            {
                StackHashSearchCriteria[] criteriaArray = CompileCriteria(criteriaString);
                if ((criteriaArray != null) && (criteriaArray.Length > 0))
                {
                    foreach (StackHashSearchCriteria criteria in criteriaArray)
                    {
                        if (criteria.SearchFieldOptions.Count > 0)
                        {
                            criteriaCollection.Add(criteria);
                        }
                    }
                }
            }

            // if a product is specified add this to each criteria
            if (product != null)
            {
                AddProductToSearch(product, ref criteriaCollection);
            }

            return criteriaCollection;
        }

        private static void AddProductToSearch(StackHashProduct product, ref StackHashSearchCriteriaCollection criteriaCollection)
        {
            IntSearchOption productSearch = new IntSearchOption();
            productSearch.FieldName = "Id";
            productSearch.ObjectType = StackHashObjectType.Product;
            productSearch.SearchOptionType = StackHashSearchOptionType.Equal;
            productSearch.SearchType = StackHashSearchFieldType.Integer;
            productSearch.Start = product.Id;

            foreach (StackHashSearchCriteria critera in criteriaCollection)
            {
                critera.SearchFieldOptions.Add(productSearch);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        private static StackHashSearchCriteria[] CompileCriteria(string criteriaString)
        {
            StackHashSearchCriteria fieldCriteria = new StackHashSearchCriteria();
            fieldCriteria.SearchFieldOptions = new StackHashSearchOptionCollection();
            AllFieldSearchBuilder allFieldSearchBuilder = new AllFieldSearchBuilder();

            // break the string in to space separated search options, text searches may be quoted to include spaces
            StringCollection optionStrings = ExtractOptionStrings(criteriaString);

            // process each option string
            foreach (string optionString in optionStrings)
            {
                string searchName;
                string search;
                StackHashSearchOptionType optionType;
                ExtractOptionFields(optionString, out searchName, out search, out optionType);

                // must have a search
                if (search != null)
                {
                    // if no search name then it's an all fields search
                    if (searchName == null)
                    {
                        allFieldSearchBuilder.AddSearch(search);
                    }
                    else
                    {
                        // compile the option
                        StackHashSearchOption searchOption = CompileSearchOption(searchName, search, optionType);
                        if (searchOption != null)
                        {
                            fieldCriteria.SearchFieldOptions.Add(searchOption);
                        }
                    }
                }
                else
                {
                    // no search
                    throw new SearchParseException(Properties.Resources.SearchParseException_MissingSearchParameter);
                }
            }

            if (allFieldSearchBuilder.CabSearches.Count == 0)
            {
                // there are no all field searches so we just return the field criteria
                return new StackHashSearchCriteria[] { fieldCriteria };
            }
            else
            {
                // all field searches need to be in separate criteria to get the desired OR behavior, so we
                // need a criteria for each of the fields containing all of the AND searches in this criteria
                return allFieldSearchBuilder.CreateCriteriaArray(fieldCriteria);
            }
        }

        private static StackHashSearchOption CompileSearchOption(string searchName, string search, StackHashSearchOptionType optionType)
        {
            StackHashSearchOption searchOption = null;

            // build the search name dictionary the first time we need it
            if (SearchNameDictionary == null)
            {
                SearchNameDictionary = new Dictionary<string, SearchFieldInfo>();
                foreach (SearchFieldInfo fieldInfo in SearchFields)
                {
                    SearchNameDictionary.Add(fieldInfo.SearchName, fieldInfo);
                }
            }

            if (SearchNameDictionary.ContainsKey(searchName))
            {
                SearchFieldInfo fieldInfo = SearchNameDictionary[searchName];
                switch (fieldInfo.FieldType)
                {
                    case StackHashSearchFieldType.String:
                        StringSearchOption stringSearch = new StringSearchOption();
                        stringSearch.CaseSensitive = false;
                        stringSearch.Start = search;

                        searchOption = stringSearch;
                        break;

                    case StackHashSearchFieldType.DateTime:
                        DateTime date;
                        if (TryParseDateTime(search, out date))
                        {
                            // date must fit a smalldatetime for the service
                            if ((date.Year < 1900) || (date.Year > 2078))
                            {
                                throw new SearchParseException(Properties.Resources.SearchParseException_DateRangeError);
                            }

                            DateTimeSearchOption dateTimeSearch = new DateTimeSearchOption();
                            dateTimeSearch.Start = date;

                            searchOption = dateTimeSearch;
                        }
                        else
                        {
                            throw new SearchParseException(string.Format(CultureInfo.CurrentCulture,
                                Properties.Resources.SearchParseException_DateConversionFailed,
                                search));
                        }
                        break;

                    case StackHashSearchFieldType.Integer:
                        int int32;
                        if (TryParseInt32(search, out int32))
                        {
                            IntSearchOption intSearch = new IntSearchOption();
                            intSearch.Start = int32;

                            searchOption = intSearch;
                        }
                        else
                        {
                            throw new SearchParseException(string.Format(CultureInfo.CurrentCulture,
                                Properties.Resources.SearchParseException_NumericConversionFailed,
                                search));
                        }
                        break;

                    case StackHashSearchFieldType.Long:
                        long int64;
                        if (TryParseInt64(search, out int64))
                        {
                            LongSearchOption longSearch = new LongSearchOption();
                            longSearch.Start = int64;

                            searchOption = longSearch;
                        }
                        else
                        {
                            throw new SearchParseException(string.Format(CultureInfo.CurrentCulture,
                                Properties.Resources.SearchParseException_NumericConversionFailed,
                                search));
                        }
                        break;
                }

                if (searchOption != null)
                {
                    searchOption.FieldName = fieldInfo.FieldName;
                    searchOption.ObjectType = fieldInfo.ObjectType;
                    searchOption.SearchOptionType = optionType;
                    searchOption.SearchType = fieldInfo.FieldType;
                }
            }
            else
            {
                // unknown search name
                throw new SearchParseException(string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.SearchParseException_UnknownField,
                    searchName));
            }

            return searchOption;
        }

        #endregion

        #region Convert a search string to a generic search

        private void InitializeFromExistingSearch(string existingSearch)
        {
            this.SearchString = string.Empty;
            _searchName = new SearchName(string.Empty);
            _criteria = new ObservableCollection<GenericSearch>();

            if (string.IsNullOrEmpty(existingSearch))
            {
                // no existing search, add a default empty search
                _criteria.Add(new GenericSearch(SearchFields, StringOptions, NumericOptions));
            }
            else
            {
                string existingSearchName = null;
                string search = null;

                if (IsSavedSearch(existingSearch))
                {
                    CrackSavedSearch(existingSearch, out existingSearchName, out search);
                }

                if (search == null)
                {
                    search = existingSearch;
                }

                // set the name if one exists
                if (existingSearchName != null)
                {
                    _searchName.Name = existingSearchName;
                }

                StringCollection criteriaStrings = ExtractCriteriaStrings(search);
                foreach (string criteriaString in criteriaStrings)
                {
                    GenericSearch genericSearch = GenericSearchFromCriteriaString(criteriaString);
                    if (genericSearch.Options.Count > 0)
                    {
                        _criteria.Add(genericSearch);
                    }
                }

                // if we failed to parse any criteria add a default
                if (_criteria.Count == 0)
                {
                    _criteria.Add(new GenericSearch(SearchFields, StringOptions, NumericOptions));
                }
            }

            textBoxSearchName.DataContext = _searchName;
            listCriteria.ItemsSource = _criteria;
            BuildSearchString();
        }

        private static GenericSearch GenericSearchFromCriteriaString(string criteriaString)
        {
            GenericSearch genericSearch = new GenericSearch(SearchFields, StringOptions, NumericOptions);
            genericSearch.Options.Clear();

            // build the search name dictionary the first time we need it
            if (SearchNameDictionary == null)
            {
                SearchNameDictionary = new Dictionary<string, SearchFieldInfo>();
                foreach (SearchFieldInfo fieldInfo in SearchFields)
                {
                    SearchNameDictionary.Add(fieldInfo.SearchName, fieldInfo);
                }
            }

            StringCollection optionStrings = ExtractOptionStrings(criteriaString);

            foreach (string optionString in optionStrings)
            {
                string searchName;
                string search;
                StackHashSearchOptionType optionType;
                ExtractOptionFields(optionString, out searchName, out search, out optionType);

                if (search != null)
                {
                    SearchFieldInfo fieldInfo = null;
                    if (searchName == null)
                    {
                        fieldInfo = SearchNameDictionary["*"];
                    }
                    else
                    {
                        if (SearchNameDictionary.ContainsKey(searchName))
                        {
                            fieldInfo = SearchNameDictionary[searchName];
                        }
                    }

                    if (fieldInfo != null)
                    {
                        GenericSearchOption genericOption = new GenericSearchOption(SearchFields, StringOptions, NumericOptions);
                        genericOption.Search = search;
                        genericOption.FieldInfo = fieldInfo;
                        foreach (SearchOptionInfo optionInfo in genericOption.AvailableOptions)
                        {
                            if (optionInfo.OptionType == optionType)
                            {
                                genericOption.OptionInfo = optionInfo;
                                break;
                            }
                        }
                        
                        genericSearch.Options.Add(genericOption);
                    }
                }
            }

            return genericSearch;
        }

        #endregion

        #region Search string parsing utilities

        private static StringCollection ExtractCriteriaStrings(string search)
        {
            Debug.Assert(search != null);
            StringCollection ret = new StringCollection();

            // each criteria is enclosed in brackets () - special case is a single criteria in which 
            // case no brackets will be present
            if (search.IndexOf('(') >= 0)
            {
                // multiple criteria
                int startCriteria = -1;
                int endCriteria = -1;

                for (int c = 0; c < search.Length; c++)
                {
                    if (search[c] == '(')
                    {
                        startCriteria = c + 1;
                    }

                    if (search[c] == ')')
                    {
                        endCriteria = c;

                        // add the criteria if the range is valid
                        if ((startCriteria >= 0) &&
                            (startCriteria < search.Length) &&
                            (endCriteria >= 0) &&
                            (endCriteria < search.Length))
                        {
                            ret.Add(search.Substring(startCriteria, endCriteria - startCriteria));
                        }

                        // reset to guard values
                        startCriteria = -1;
                        endCriteria = -1;
                    }
                }
            }
            else
            {
                // single criteria
                ret.Add(search);
            }

            return ret;
        }

        private static StringCollection ExtractOptionStrings(string criteria)
        {
            Debug.Assert(criteria != null);
            StringCollection ret = new StringCollection();

            bool inQuote = false;
            int startOption = 0;
            for (int c = 0; c < criteria.Length; c++)
            {
                if (criteria[c] == '"')
                {
                    inQuote = !inQuote;
                    continue;
                }

                if (!inQuote)
                {
                    if (criteria[c] == ' ')
                    {
                        ret.Add(criteria.Substring(startOption, c - startOption));
                        startOption = c + 1;
                    }
                }
            }

            // add the final string
            if (startOption < criteria.Length)
            {
                ret.Add(criteria.Substring(startOption, criteria.Length - startOption));
            }

            return ret;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        private static void ExtractOptionFields(string option, out string searchName, out string search, out StackHashSearchOptionType optionType)
        {
            Debug.Assert(option != null);

            searchName = null;
            search = null;
            optionType = StackHashSearchOptionType.All;

            int sepIndex = option.IndexOfAny(SearchSeps);
            if (sepIndex > 0)
            {
                // separator found, this is a search of a specific field
                char sep = option[sepIndex];
                optionType = GetOptionType(sep);

                string[] optionComponents = option.Split(SearchSeps, StringSplitOptions.RemoveEmptyEntries);
                if (optionComponents.Length == 2)
                {
                    // first component is the field type, the second is the actual search
                    searchName = optionComponents[0].Trim(SearchTrimChars).ToLower(CultureInfo.InvariantCulture);
                    search = optionComponents[1].Trim(SearchTrimChars);
                }
            }
            else
            {
                search = option.Trim(SearchTrimChars);
            }
        }

        #endregion

        #region Convert generic search to a search string

        private void BuildSearchString()
        {
            StringCollection optionStrings = new StringCollection();
            bool anyErrors = false;
            bool anyCriteriaErrors = false;

            foreach (GenericSearch search in _criteria)
            {
                anyCriteriaErrors = false;

                optionStrings.Add(BuildSearchOptionString(search, out anyCriteriaErrors));

                if (anyCriteriaErrors)
                {
                    anyErrors = true;
                }
            }

            string searchString;
            if (optionStrings.Count == 1)
            {
                // if exactly one option we use the option string
                searchString = optionStrings[0];
            }
            else
            {
                // if more than one option build with OR separators
                StringBuilder stringBuilderCriteria = new StringBuilder();

                for (int criteria = 0; criteria < optionStrings.Count; criteria++)
                {
                    // brackets around AND parameters
                    stringBuilderCriteria.AppendFormat(CultureInfo.InvariantCulture,
                        "({0})",
                        optionStrings[criteria]);

                    if (criteria < (optionStrings.Count - 1))
                    {
                        stringBuilderCriteria.Append(" OR ");
                    }
                }

                searchString = stringBuilderCriteria.ToString();
            }

            bool haveSearch = searchString.Length > 0;

            if (textBoxSearchName.Text.Length > 0)
            {
                if (_searchName["Name"] == null)
                {
                    searchString = CombineSavedSearch(_searchName.Name, searchString);
                }
                else
                {
                    // errors in name
                    anyErrors = true;
                }
            }

            // can click OK if there's a search and there are no errors
            buttonOK.IsEnabled = (haveSearch && (!anyErrors));

            this.SearchString = searchString;
        }

        private static string BuildSearchOptionString(GenericSearch search, out bool anyErrors)
        {
            StringBuilder stringBuilderSearch = new StringBuilder();
            anyErrors = false;

            for (int option = 0; option < search.Options.Count; option++)
            {
                // check that the search is valid
                if (search.Options[option]["Search"] != null)
                {
                    anyErrors = true;
                }
                else
                {
                    // skip any empty search items
                    if (!string.IsNullOrEmpty(search.Options[option].Search))
                    {
                        // quote the search experession if it contains a space
                        string quotedSearch;
                        if (search.Options[option].Search.Contains(' '))
                        {
                            quotedSearch = string.Format(CultureInfo.InvariantCulture,
                                "\"{0}\"",
                                search.Options[option].Search);
                        }
                        else
                        {
                            quotedSearch = search.Options[option].Search;
                        }

                        if (search.Options[option].FieldInfo.FieldName == "*")
                        {
                            // search is across all fields so just include the search string
                            stringBuilderSearch.Append(quotedSearch);
                        }
                        else
                        {
                            // search is for a specific field
                            if (search.Options[option].OptionInfo != null)
                            {
                                // only if OptionInfo is not null - this will happen during update
                                stringBuilderSearch.AppendFormat(CultureInfo.InvariantCulture,
                                    "{0}{1}{2}",
                                    search.Options[option].FieldInfo.SearchName,
                                    GetSearchFieldSeparator(search.Options[option].OptionInfo.OptionType),
                                    quotedSearch);
                            }
                        }

                        // append a space unless it's the last option
                        if (option < (search.Options.Count - 1))
                        {
                            stringBuilderSearch.Append(" ");
                        }
                    }
                }
            }

            return stringBuilderSearch.ToString();
        }

        #endregion

        #region Conversion Utilities

        private static string GetSearchFieldSeparator(StackHashSearchOptionType optionType)
        {
            switch (optionType)
            {
                case StackHashSearchOptionType.Equal:
                    return "=";

                case StackHashSearchOptionType.GreaterThan:
                    return ">";

                case StackHashSearchOptionType.LessThan:
                    return "<";

                case StackHashSearchOptionType.StringContains:
                    return ":";

                case StackHashSearchOptionType.StringDoesNotContain:
                    return "!";

                default:
                    Debug.Assert(false, "Unsupported StackHashSearchOptionType");
                    return ":";
            }
        }

        private static StackHashSearchOptionType GetOptionType(char c)
        {
            switch (c)
            {
                case '=':
                    return StackHashSearchOptionType.Equal;

                case '>':
                    return StackHashSearchOptionType.GreaterThan;

                case '<':
                    return StackHashSearchOptionType.LessThan;

                case ':':
                    return StackHashSearchOptionType.StringContains;

                case '!':
                    return StackHashSearchOptionType.StringDoesNotContain;

                default:
                    Debug.Assert(false, "Unsupported option character");
                    return StackHashSearchOptionType.StringContains;
            }
        }

        private static bool TryParseDateTime(string source, out DateTime dateTime)
        {
            bool parsed = false;
            dateTime = DateTime.MinValue;

            // first try a DateTime.TryParse
            parsed = DateTime.TryParse(source, out dateTime);

            // if this failed, look for a relative string
            if (!parsed)
            {
                string[] relativeBits = source.Split(SpaceChars, StringSplitOptions.RemoveEmptyEntries);
                if (relativeBits.Length == 2)
                {
                    // should be a number folloed by a unit (days, weeks, months, years)
                    int relativeNumber;
                    if (Int32.TryParse(relativeBits[0], out relativeNumber))
                    {
                        switch (Char.ToLower(relativeBits[1][0], CultureInfo.CurrentCulture))
                        {
                            case 'd': // days
                                dateTime = DateTime.Now.AddDays(relativeNumber);
                                parsed = true;
                                break;

                            case 'w': // weeks
                                dateTime = DateTime.Now.AddDays(relativeNumber * 7);
                                parsed = true;
                                break;

                            case 'm': // months
                                dateTime = DateTime.Now.AddMonths(relativeNumber);
                                parsed = true;
                                break;

                            case 'y': // years
                                dateTime = DateTime.Now.AddYears(relativeNumber);
                                parsed = true;
                                break;
                        }
                    }
                }
            }

            return parsed;
        }

        private static bool TryParseInt32(string source, out int int32)
        {
            bool parsed = false;
            int32 = 0;

            if ((source.Length > 2) && (source.IndexOf("0x", StringComparison.OrdinalIgnoreCase) == 0))
            {
                // parse as hex
                parsed = Int32.TryParse(source.Substring(2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out int32);
            }
            else
            {
                // parse normally
                parsed = Int32.TryParse(source, out int32);
            }

            return parsed;
        }

        private static bool TryParseInt64(string source, out long int64)
        {
            bool parsed = false;
            int64 = 0;

            if ((source.Length > 2) && (source.IndexOf("0x", StringComparison.OrdinalIgnoreCase) == 0))
            {
                // parse as hex
                parsed = Int64.TryParse(source.Substring(2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out int64);
            }
            else
            {
                // parse normally
                parsed = Int64.TryParse(source, out int64);
            }

            return parsed;
        }

        #endregion

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void buttonAddField_Click(object sender, RoutedEventArgs e)
        {
            Button button = e.OriginalSource as Button;
            GenericSearchOption genericSearchOption = button.DataContext as GenericSearchOption;
            ObservableCollection<GenericSearchOption> options = button.Tag as ObservableCollection<GenericSearchOption>;

            int index = options.IndexOf(genericSearchOption);
            options.Insert(index + 1, new GenericSearchOption(SearchFields, StringOptions, NumericOptions));

            BuildSearchString();
        }

        private void buttonDeleteField_Click(object sender, RoutedEventArgs e)
        {
            Button button = e.OriginalSource as Button;
            GenericSearchOption genericSearchOption = button.DataContext as GenericSearchOption;
            ObservableCollection<GenericSearchOption> options = button.Tag as ObservableCollection<GenericSearchOption>;

            options.Remove(genericSearchOption);

            BuildSearchString();
        }

        private void buttonDeleteCriteria_Click(object sender, RoutedEventArgs e)
        {
            Button button = e.OriginalSource as Button;
            GenericSearch genericSearch = button.DataContext as GenericSearch;

            _criteria.Remove(genericSearch);

            BuildSearchString();
        }

        private void buttonAddCriteria_Click(object sender, RoutedEventArgs e)
        {
            Button button = e.OriginalSource as Button;
            GenericSearch genericSearch = button.DataContext as GenericSearch;

            int index = _criteria.IndexOf(genericSearch);
            _criteria.Insert(index + 1, new GenericSearch(SearchFields, StringOptions, NumericOptions));

            BuildSearchString();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BuildSearchString();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            BuildSearchString();
        }

        private void textBoxSearchName_TextChanged(object sender, TextChangedEventArgs e)
        {
            BuildSearchString();
        }

        private void commandBindingHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            StackHashHelp.ShowTopic("search-builder.htm");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            UserSettings.Settings.SaveWindow(WindowKey, this);
        }
    }
}
