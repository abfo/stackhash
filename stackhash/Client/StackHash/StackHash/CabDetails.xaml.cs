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
using StackHash.StackHashService;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;

namespace StackHash
{
    /// <summary>
    /// Shows details for a StackHashCab
    /// </summary>
    public partial class CabDetails : UserControl
    {
        private const string ListViewResultFilesKey = "CabDetails.listViewResultFiles";
        private const string ListViewCabContentsKey = "CabDetails.listViewCabContents";
        private const string Column1Key = "CabDetails.Column1";
        private const string Column3Key = "CabDetails.Column3";
        private const string InnerColumn1Key = "CabDetails.InnerColumn1";
        private const string InnerColumn3Key = "CabDetails.InnerColumn3";
        private const string Row3Key = "CabDetails.Row3";
        private const string Row5Key = "CabDetails.Row5";

        private ListViewSorter _listViewSorter;
        private ListViewSorter _listViewCabContentsSorter;
        private ClientLogic _clientLogic;
        private bool _scriptRunFromPage;

        /// <summary>
        /// Shows details for a StackHashCab
        /// </summary>
        public CabDetails()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                UserSettings.Settings.RestoreGridView(ListViewResultFilesKey, listViewResultFiles.View as GridView);
                UserSettings.Settings.RestoreGridView(ListViewCabContentsKey, listViewCabContents.View as GridView);

                if (UserSettings.Settings.HaveGridLength(Column1Key))
                {
                    Column1.Width = UserSettings.Settings.RestoreGridLength(Column1Key);
                }

                if (UserSettings.Settings.HaveGridLength(Column3Key))
                {
                    Column3.Width = UserSettings.Settings.RestoreGridLength(Column3Key);
                }

                if (UserSettings.Settings.HaveGridLength(InnerColumn1Key))
                {
                    InnerColumn1.Width = UserSettings.Settings.RestoreGridLength(InnerColumn1Key);
                }

                if (UserSettings.Settings.HaveGridLength(InnerColumn3Key))
                {
                    InnerColumn3.Width = UserSettings.Settings.RestoreGridLength(InnerColumn3Key);
                }

                if (UserSettings.Settings.HaveGridLength(Row3Key))
                {
                    Row3.Height = UserSettings.Settings.RestoreGridLength(Row3Key);
                }

                if (UserSettings.Settings.HaveGridLength(Row5Key))
                {
                    Row5.Height = UserSettings.Settings.RestoreGridLength(Row5Key);
                }

                _listViewSorter = new ListViewSorter(listViewResultFiles);

                _listViewSorter.AddDefaultSort("ScriptName", ListSortDirection.Ascending);
                _listViewSorter.AddDefaultSort("RunDate", ListSortDirection.Descending);
                _listViewSorter.AddDefaultSort("UserName", ListSortDirection.Ascending);

                _listViewCabContentsSorter = new ListViewSorter(listViewCabContents);

                _listViewCabContentsSorter.AddDefaultSort("FileName", ListSortDirection.Ascending);
                _listViewCabContentsSorter.AddDefaultSort("Length", ListSortDirection.Ascending);
            }
        }

        /// <summary>
        /// Called when the main StackHash window is closed
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void StackHashMainWindowClosed()
        {
            if (_clientLogic != null)
            {
                _clientLogic.ClientLogicUI -= _clientLogic_ClientLogicUI;
                _clientLogic.PropertyChanged -= _clientLogic_PropertyChanged;
            }

            notesControl.SaveNote -= notesControl_SaveNote;

            UserSettings.Settings.SaveGridView(ListViewResultFilesKey, listViewResultFiles.View as GridView);
            UserSettings.Settings.SaveGridView(ListViewCabContentsKey, listViewCabContents.View as GridView);
            UserSettings.Settings.SaveGridLength(Column1Key, Column1.Width);
            UserSettings.Settings.SaveGridLength(Column3Key, Column3.Width);
            UserSettings.Settings.SaveGridLength(InnerColumn1Key, InnerColumn1.Width);
            UserSettings.Settings.SaveGridLength(InnerColumn3Key, InnerColumn3.Width);
            UserSettings.Settings.SaveGridLength(Row3Key, Row3.Height);
            UserSettings.Settings.SaveGridLength(Row5Key, Row5.Height);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // note - won't have a DataContext in the designer
           _clientLogic = this.DataContext as ClientLogic;

           if (_clientLogic != null)
           {
               _clientLogic.ClientLogicUI += new EventHandler<ClientLogicUIEventArgs>(_clientLogic_ClientLogicUI);
               _clientLogic.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_clientLogic_PropertyChanged);
           }

           notesControl.SaveNote += new EventHandler<SaveNoteEventArgs>(notesControl_SaveNote);
        }

        void notesControl_SaveNote(object sender, SaveNoteEventArgs e)
        {
            _clientLogic.AddCabNote(e.Note, e.NoteId);
        }

        void _clientLogic_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                if (e.PropertyName == "CurrentCab")
                {
                    // clear notes when the cab changes
                    notesControl.ClearNotes();
                    richTextBoxScriptResult.Document.Blocks.Clear();
                    listViewCabContents.SelectedItem = null;
                    listViewResultFiles.SelectedItem = null;
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action<object,System.ComponentModel.PropertyChangedEventArgs>(_clientLogic_PropertyChanged), sender, e);
            }
        }

        private void DoGetCabNotes()
        {
            _clientLogic.GetCabNotes();
        }

        void _clientLogic_ClientLogicUI(object sender, ClientLogicUIEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                // only display result if this page initiated a request
                if ((e.UIRequest == ClientLogicUIRequest.ScriptResultsReady) && ((_scriptRunFromPage) || (_clientLogic.CurrentView == ClientLogicView.CabDetail)))
                {
                    _scriptRunFromPage = false;

                    DisplayScriptResult resultFile = listViewResultFiles.SelectedItem as DisplayScriptResult;
                    if ((resultFile != null) && (_clientLogic.CurrentScriptResult != null))
                    {
                        if (_clientLogic.CurrentScriptResult.Name == resultFile.ScriptName)
                        {
                            richTextBoxScriptResult.Document.Blocks.Clear();
                            richTextBoxScriptResult.Document.Blocks.Add(ScriptResultViewer.GetScriptResultParagraph(_clientLogic.CurrentScriptResult));
                        }
                    }

                    // begin invoke to give ClientLogic a chance to free up
                    this.Dispatcher.BeginInvoke(new Action(DoGetCabNotes));
                }
                else if (e.UIRequest == ClientLogicUIRequest.CabNotesReady)
                {
                    // if we have some notes update them
                    if (_clientLogic.CurrentCabNotes != null)
                    {
                        notesControl.UpdateNotes(_clientLogic.CurrentCabNotes);
                    }
                    else
                    {
                        notesControl.ClearNotes();
                    }
                }
                else if (e.UIRequest == ClientLogicUIRequest.CabFileReady)
                {
                    richTextBoxScriptResult.Document.Blocks.Clear();

                    if (_clientLogic.CurrentCabFileContents != null)
                    {
                        richTextBoxScriptResult.Document.Blocks.Add(new Paragraph(new Run(_clientLogic.CurrentCabFileContents)));
                    }
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action<object, ClientLogicUIEventArgs>(_clientLogic_ClientLogicUI), sender, e);
            }
        }

        private void linkMappedProducts_Click(object sender, RoutedEventArgs e)
        {
            _clientLogic.CurrentView = ClientLogicView.ProductList;
        }

        private void linkCurrentProduct_Click(object sender, RoutedEventArgs e)
        {
            _clientLogic.CurrentView = ClientLogicView.EventList;
        }

        private void linkCurrentEvent_Click(object sender, RoutedEventArgs e)
        {
            _clientLogic.CurrentView = ClientLogicView.EventDetail;
        }

        private void listViewResultFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DisplayScriptResult resultFile = listViewResultFiles.SelectedItem as DisplayScriptResult;
            if (resultFile != null)
            {
                listViewCabContents.SelectedItem = null;

                bool currentlySelected = false;

                DateTime localRunDate = resultFile.RunDate.ToLocalTime();
                textBlockPreviewHeader.Text = string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.CabDetails_ScriptHeader,
                    resultFile.ScriptName,
                    localRunDate.ToShortDateString(),
                    localRunDate);

                // if we ald
                if (_clientLogic.CurrentScriptResult != null)
                {
                    if (_clientLogic.CurrentScriptResult.Name == resultFile.ScriptName)
                    {
                        currentlySelected = true;
                    }
                }

                if (currentlySelected)
                {
                    // refresh the results
                    if (_clientLogic.CurrentScriptResult != null)
                    {
                        richTextBoxScriptResult.Document.Blocks.Clear();
                        richTextBoxScriptResult.Document.Blocks.Add(ScriptResultViewer.GetScriptResultParagraph(_clientLogic.CurrentScriptResult));
                    }
                }
                else
                {
                    // clear any existing result
                    richTextBoxScriptResult.Document.Blocks.Clear();

                    _scriptRunFromPage = true;
                    _clientLogic.AdminGetResult(resultFile.ScriptName);
                }
            }
            else
            {
                // clear result on null selection
                richTextBoxScriptResult.Document.Blocks.Clear();

                if (listViewCabContents.SelectedItem == null)
                {
                    textBlockPreviewHeader.Text = Properties.Resources.CabDetails_NoSelectionHeader;
                }
            }
        }

        private void listViewCabContents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DisplayCabFile cabFile = listViewCabContents.SelectedItem as DisplayCabFile;
            if (cabFile != null)
            {
                listViewResultFiles.SelectedItem = null;

                // clear any existing result
                richTextBoxScriptResult.Document.Blocks.Clear();

                textBlockPreviewHeader.Text = cabFile.FileName + ":";

                if (System.IO.Path.GetExtension(cabFile.FileName).IndexOf("dmp", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    richTextBoxScriptResult.Document.Blocks.Add(new Paragraph(new Run(Properties.Resources.CabDetails_PreviewUnavailable)));
                }
                else
                {
                    _clientLogic.GetCabFile(cabFile.FileName, cabFile.Length);
                }
            }
            else
            {
                // clear result on null selection
                richTextBoxScriptResult.Document.Blocks.Clear();

                if (listViewResultFiles.SelectedItem == null)
                {
                    textBlockPreviewHeader.Text = Properties.Resources.CabDetails_NoSelectionHeader;
                }
            }
        }

        private void listViewResultFiles_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader header = e.OriginalSource as GridViewColumnHeader;
            if (header != null)
            {
                _listViewSorter.SortColumn(header);
            }
        }

        private void menuDeleteScriptRun_Click(object sender, RoutedEventArgs e)
        {
            DisplayScriptResult resultFile = listViewResultFiles.SelectedItem as DisplayScriptResult;
            if (resultFile != null)
            {
                bool deleteScriptRun = true;

                if (!UserSettings.Settings.IsMessageSuppressed(UserSettings.SuppressCabDetailsDeleteScriptRun))
                {
                    bool suppress = false;

                    if (StackHashMessageBox.Show(Window.GetWindow(this),
                        string.Format(CultureInfo.CurrentCulture,
                        Properties.Resources.CabDetails_DeleteScriptRunMBMessage,
                        resultFile.ScriptName,
                        _clientLogic.CurrentCab.Id),
                        Properties.Resources.CabDetails_DeleteScriptRunMBTitle,
                        StackHashMessageBoxType.YesNo,
                        StackHashMessageBoxIcon.Question,
                        true,
                        out suppress) != StackHashDialogResult.Yes)
                    {
                        deleteScriptRun = false;
                    }

                    if (suppress)
                    {
                        UserSettings.Settings.SuppressMessage(UserSettings.SuppressCabDetailsDeleteScriptRun);
                    }
                }

                if (deleteScriptRun)
                {
                    _clientLogic.AdminRemoveResult(resultFile.ScriptName);
                }
            }
        }

        private void menuRunScriptAgain_Click(object sender, RoutedEventArgs e)
        {
            DisplayScriptResult resultFile = listViewResultFiles.SelectedItem as DisplayScriptResult;
            if (resultFile != null)
            {
                _clientLogic.AdminTestScript(resultFile.ScriptName);
            }
        }

        private void listViewResultFiles_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            menuDeleteScriptRun.IsEnabled = listViewResultFiles.SelectedItem is DisplayScriptResult;
            menuRunScriptAgain.IsEnabled = listViewResultFiles.SelectedItem is DisplayScriptResult;
        }

        private void MenuItem_IsEnabledChangedDimIcon(object sender, DependencyPropertyChangedEventArgs e)
        {
            // dim icon (if present) if disabled... can't seem to do this through a style in App.xaml like
            // with the buttons
            MenuItem menu = sender as MenuItem;
            if (menu != null)
            {
                Image menuImage = menu.Icon as Image;
                if (menuImage != null)
                {
                    if (menu.IsEnabled)
                    {
                        menuImage.Opacity = 1.0;
                    }
                    else
                    {
                        menuImage.Opacity = 0.5;
                    }
                }
            }
        }

        private void menuItemSendCabToPlugin_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = e.OriginalSource as MenuItem;
            if (menuItem != null)
            {
                StackHashBugTrackerPlugIn plugin = menuItem.Tag as StackHashBugTrackerPlugIn;
                if (plugin != null)
                {
                    if ((_clientLogic.CurrentCab != null) && (_clientLogic.CurrentEventPackage != null))
                    {
                        _clientLogic.SendCabToPlugin(_clientLogic.CurrentCab, _clientLogic.CurrentEventPackage, plugin.Name);
                    }
                }
            }
        }

        private void menuItemCopyCabUrl_Click(object sender, RoutedEventArgs e)
        {
            if ((_clientLogic.CurrentCab != null) && (_clientLogic.CurrentEventPackage != null))
            {
                Clipboard.SetText(StackHashUri.CreateUriString(UserSettings.Settings.CurrentContextId,
                    _clientLogic.CurrentEventPackage.ProductId,
                    _clientLogic.CurrentEventPackage.Id,
                    _clientLogic.CurrentEventPackage.EventTypeName,
                    _clientLogic.CurrentCab.Id));
            }
        }

        private void listViewCabContents_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader header = e.OriginalSource as GridViewColumnHeader;
            if (header != null)
            {
                _listViewCabContentsSorter.SortColumn(header);
            }
        }
    }
}
