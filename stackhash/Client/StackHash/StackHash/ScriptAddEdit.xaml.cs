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
using StackHash.StackHashService;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.ComponentModel;
using System.Globalization;

namespace StackHash
{
    /// <summary>
    /// Adds or edits a debugger script
    /// </summary>
    public partial class ScriptAddEdit : Window
    {
        private const string WindowKey = "ScriptAddEdit";
        private const char ScriptCommentSeparator = '*';
        private static string[] NoCommentCommands = new string[] { ".sympath", "sympath", ".srcpath", "srcpath", ".exepath", "exepath" };

        private class DumpTypeWrapper
        {
            public StackHashScriptDumpType Type { get; private set; }
            public string DisplayName { get; private set; }

            public DumpTypeWrapper(StackHashScriptDumpType type, string displayName)
            {
                this.Type = type;
                this.DisplayName = displayName;
            }

            public static ObservableCollection<DumpTypeWrapper> GetTypeList()
            {
                ObservableCollection<DumpTypeWrapper> types = new ObservableCollection<DumpTypeWrapper>();

                types.Add(new DumpTypeWrapper(StackHashScriptDumpType.UnmanagedAndManaged, Properties.Resources.DumpType_All));
                types.Add(new DumpTypeWrapper(StackHashScriptDumpType.ManagedOnly, Properties.Resources.DumpType_Managed));
                types.Add(new DumpTypeWrapper(StackHashScriptDumpType.UnmanagedOnly, Properties.Resources.DumpType_Native));

                return types;
            }
        }

        private class ContextValidation : IDataErrorInfo
        {
            public bool ValidationEnabled { get; set; }
            public ObservableCollection<StackHashScriptFileData> AllScriptData { get; set; }
            public int InitialNameIndex { get; set; }
            public bool RunAutomatically { get; set; }
            public bool CanEdit { get; private set; }
            public DumpTypeWrapper DumpType { get; set; }
            public ObservableCollection<DumpTypeWrapper> DumpTypes { get; private set; }

            public string ScriptName { get; set; }
            public string Script { get; set; }

            private char[] _invalidScriptNameChars;
            private string _invalidScriptNameCharsMessage;

            public ContextValidation(ObservableCollection<StackHashScriptFileData> allScriptData, 
                int initialNameIndex, 
                string scriptName, 
                string script, 
                bool runAutomatically,
                bool isReadOnly,
                StackHashScriptDumpType dumpType)
            {
                this.DumpTypes = DumpTypeWrapper.GetTypeList();
                foreach (DumpTypeWrapper dumpWrapper in this.DumpTypes)
                {
                    if (dumpWrapper.Type == dumpType)
                    {
                        this.DumpType = dumpWrapper;
                        break;
                    }
                }

                this.AllScriptData = allScriptData;
                this.InitialNameIndex = initialNameIndex;
                this.ScriptName = scriptName;
                this.Script = script;
                this.RunAutomatically = runAutomatically;
                this.CanEdit = !isReadOnly;

                _invalidScriptNameChars = System.IO.Path.GetInvalidFileNameChars();
            }

            #region IDataErrorInfo Members

            public string Error
            {
                get { return null; }
            }

            public string this[string columnName]
            {
                get 
                {
                    if (!ValidationEnabled)
                    {
                        return null;
                    }

                    string result = null;

                    switch (columnName)
                    {
                        case "ScriptName":
                            if (string.IsNullOrEmpty(this.ScriptName))
                            {
                                result = Properties.Resources.ScriptAddEdit_ValidationErrorEmptyScriptName;
                            }
                            else
                            {
                                // check for duplicates
                                for (int i = 0; i < this.AllScriptData.Count; i++)
                                {
                                    // don't compare to the original index of this name
                                    if (i != this.InitialNameIndex)
                                    {
                                        if (string.Compare(this.ScriptName, this.AllScriptData[i].Name, StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            result = Properties.Resources.ScriptAddEdit_ValidationErrorDuplicateScriptName;
                                            break;
                                        }
                                    }
                                }

                                // if still no error check that no illegal characters are included
                                if (result == null)
                                {
                                    if (this.ScriptName.IndexOfAny(_invalidScriptNameChars) > 0)
                                    {
                                        // build the error string the first time it's needed
                                        if (_invalidScriptNameCharsMessage == null)
                                        {
                                            StringBuilder sb = new StringBuilder();
                                            foreach (char c in _invalidScriptNameChars)
                                            {
                                                sb.AppendFormat(CultureInfo.CurrentCulture, " {0}", c);
                                            }
                                            _invalidScriptNameCharsMessage = sb.ToString();
                                        }

                                        result = string.Format(CultureInfo.CurrentCulture,
                                            Properties.Resources.ScriptAddEdit_ValidationErrorBadCharInScriptName,
                                            _invalidScriptNameCharsMessage);
                                    }
                                }
                            }
                            break;

                        case "Script":
                            if (string.IsNullOrEmpty(this.Script))
                            {
                                result = Properties.Resources.ScriptAddEdit_ValidationErrorEmptyScript;
                            }
                            break;
                    }

                    return result;
                }
            }

            #endregion
        }

        private ClientLogic _clientLogic;
        private ContextValidation _contextValidation;
        private StackHashScriptSettings _scriptSettings;
        private bool _add;

        /// <summary>
        /// Adds or edits a debugger script
        /// </summary>
        /// <param name="clientLogic">Client Logic</param>
        /// <param name="scriptSettings">Script to edit (new blank script if add)</param>
        /// <param name="add">True if this is a new script, false if an existing script is being edited</param>
        public ScriptAddEdit(ClientLogic clientLogic, StackHashScriptSettings scriptSettings, bool add)
        {
            Debug.Assert(clientLogic != null);
            Debug.Assert(scriptSettings != null);

            _clientLogic = clientLogic;
            _scriptSettings = scriptSettings;
            _add = add;

            InitializeComponent();

            if (UserSettings.Settings.HaveWindow(WindowKey))
            {
                UserSettings.Settings.RestoreWindow(WindowKey, this);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title = _add ? Properties.Resources.ScriptAddEdit_AddTitle : Properties.Resources.ScriptAddEdit_EditTitle;

            // if a system script hide cancel and change OK to close
            if (_scriptSettings.IsReadOnly)
            {
                buttonCancel.Visibility = System.Windows.Visibility.Collapsed;
                buttonOK.Content = Properties.Resources.ButtonText_Close;
            }

            LoadFromScript();
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            // enable validation on OK
            _contextValidation.ValidationEnabled = true;

            if (BindingValidator.IsValid(this))
            {
                SaveToScript();
                DialogResult = true;
            }
        }

        private void LoadFromScript()
        {
            // convert the script to a string
            StringBuilder sb = new StringBuilder();
            foreach (StackHashScriptLine line in _scriptSettings.Script)
            {
                sb.Append(line.Command);

                if (!string.IsNullOrEmpty(line.Comment))
                {
                    sb.Append(string.Format(CultureInfo.CurrentCulture,
                        " {0} {1}",
                        ScriptCommentSeparator,
                        line.Comment));
                }

                sb.Append(Environment.NewLine);
            }

            int initialNameIndex = -1;
            if (!_add)
            {
                // find the index of this script name so we can detect duplicates later
                for (int i = 0; i < _clientLogic.ScriptData.Count; i++)
                {
                    if (string.Compare(_scriptSettings.Name, _clientLogic.ScriptData[i].Name, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        initialNameIndex = i;
                        break;
                    }
                }
            }

            _contextValidation = new ContextValidation(_clientLogic.ScriptData, 
                initialNameIndex, 
                _scriptSettings.Name, 
                sb.ToString(), 
                _scriptSettings.RunAutomatically,
                _scriptSettings.IsReadOnly,
                _scriptSettings.DumpType);

            this.DataContext = _contextValidation;
        }

        private void SaveToScript()
        {
            // convert the script string back to a script
            StackHashScript script = new StackHashScript();
            bool isNoCommentCommand = false;
            string line = null;

            for (int i = 0; i < textScript.LineCount; i++)
            {
                isNoCommentCommand = false;
                line = textScript.GetLineText(i).Trim();

                // for certain commands comments are ignored (because the * is part of the command)
                foreach (string noCommentCommand in NoCommentCommands)
                {
                    if (line.IndexOf(noCommentCommand, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        isNoCommentCommand = true;
                        break;
                    }
                }

                if (isNoCommentCommand)
                {
                    StackHashScriptLine scriptLine = new StackHashScriptLine();
                    scriptLine.Command = line;
                    script.Add(scriptLine);
                }
                else
                {
                    StackHashScriptLine scriptLine = new StackHashScriptLine();
                    int lastCommentSeparator = line.LastIndexOf(ScriptCommentSeparator);
                    if (lastCommentSeparator > 0)
                    {
                        scriptLine.Command = line.Substring(0, lastCommentSeparator).Trim();
                        scriptLine.Comment = line.Substring(lastCommentSeparator + 1).Trim();
                    }
                    else
                    {
                        scriptLine.Command = line;
                    }
                    script.Add(scriptLine);
                }
            }

            _scriptSettings.Name = textScriptName.Text;
            _scriptSettings.RunAutomatically = _contextValidation.RunAutomatically;
            _scriptSettings.Script = script;
            _scriptSettings.DumpType = _contextValidation.DumpType.Type;
        }

        private void commandBindingHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            StackHashHelp.ShowTopic("script-add-edit.htm");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            UserSettings.Settings.SaveWindow(WindowKey, this);
        }
    }
}
