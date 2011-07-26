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
using System.Collections.ObjectModel;
using StackHash.StackHashService;
using StackHash.ValueConverters;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using StackHashUtilities;

namespace StackHash
{
    /// <summary>
    /// Event args containing a note to save
    /// </summary>
    public class SaveNoteEventArgs : EventArgs
    {
        /// <summary>
        /// The note to save
        /// </summary>
        public string Note { get; private set; }

        /// <summary>
        /// The Id of the note to save (0 = create a new note)
        /// </summary>
        public int NoteId { get; private set; }

        /// <summary>
        /// Event args containing a note to save
        /// </summary>
        /// <param name="note">The note to save</param>
        /// <param name="noteId">The note id to save (0 = create new note)</param>
        public SaveNoteEventArgs(string note, int noteId)
        {
            this.Note = note;
            this.NoteId = noteId;
        }
    }

    /// <summary>
    /// Display a collection of notes and allows a new note to be added
    /// </summary>
    public partial class NotesControl : UserControl
    {
        private DateTimeDisplayConverter _dateTimeDisplayConverter;

        /// <summary>
        /// Event fired when the user request that a new note is saved
        /// </summary>
        public event EventHandler<SaveNoteEventArgs> SaveNote;

        private Regex _uriRegex;
        private StackHashNoteEntry _currentEditNote;

        /// <summary>
        /// Display a collection of notes and allows a new note to be added
        /// </summary>
        public NotesControl()
        {
            InitializeComponent();

            _dateTimeDisplayConverter = new DateTimeDisplayConverter();

            // http://www.west-wind.com/WebLog/posts/9779.aspx
            _uriRegex = new Regex(@"([""'=]|&quot;)?(http://|ftp://|https://|www\.|ftp\.[\w]+)([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Updates the current collection of notes
        /// </summary>
        /// <param name="notes">Notes to display</param>
        public void UpdateNotes(ObservableCollection<StackHashNoteEntry> notes)
        {
            Debug.Assert(notes != null);

            ClearNotes();
            richTextBoxNotes.Document.Blocks.Add(GetNotesParagraph(notes));
        }

        /// <summary>
        /// Clears all notes from the control
        /// </summary>
        public void ClearNotes()
        {
            // clear out the rich text box
            richTextBoxNotes.Document.Blocks.Clear();

            ConfigureReadMode();
        }

        private Paragraph GetNotesParagraph(ObservableCollection<StackHashNoteEntry> notes)
        {
            Paragraph para = new Paragraph();

            for (int note = notes.Count - 1; note >= 0; note--)
            {
                AddNoteToParagraph(notes[note], ref para);
            }

            return para;
        }

        private void AddNoteToParagraph(StackHashNoteEntry note, ref Paragraph para)
        {
            // create header
            Span noteHeaderSpan = new Span();
            noteHeaderSpan.FontWeight = FontWeights.Bold;
            noteHeaderSpan.Inlines.Add(new Run(string.Format(CultureInfo.CurrentCulture,
                Properties.Resources.NotesControl_HeaderName,
                note.User)));

            Span noteInfoSpan = new Span();
            noteInfoSpan.Foreground = Brushes.Gray;
            noteInfoSpan.Inlines.Add(new Run(string.Format(CultureInfo.CurrentCulture,
                Properties.Resources.NotesControl_HeaderDetails,
               (string)_dateTimeDisplayConverter.Convert(note.TimeOfEntry, typeof(string), null, CultureInfo.CurrentCulture),
                note.Source)));

            // create note
            Run noteRun = new Run(note.Note);

            Hyperlink linkEditNote = new Hyperlink(new Run("Edit"));
            linkEditNote.Tag = note;
            linkEditNote.NavigateUri = new Uri("http://www.stackhash.com/");
            linkEditNote.RequestNavigate += new RequestNavigateEventHandler(linkEditNote_RequestNavigate);
            
            para.Inlines.Add(noteHeaderSpan);
            para.Inlines.Add(new Run(" "));
            para.Inlines.Add(noteInfoSpan);
            para.Inlines.Add(new Run(" ("));
            para.Inlines.Add(linkEditNote);
            para.Inlines.Add(new Run(")"));
            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(noteRun);
            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(new LineBreak());

            // add hyperlinks to note run
            try
            {
                while (ProcessInlines(para.Inlines.FirstInline)) { };
            }
            catch (Exception ex)
            {
                DiagnosticsHelper.LogException(DiagSeverity.Warning, "AddNoteToParagraph failed", ex);
            }
        }

        void linkEditNote_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Hyperlink sourceLink = e.OriginalSource as Hyperlink;
            if (sourceLink != null)
            {
                ConfigureEditMode(sourceLink.Tag as StackHashNoteEntry);
            }
        }

        private bool ProcessInlines(Inline inline)
        {
            bool foundLink = false;

            Inline i = inline;

            while (i != null)
            {
                Run r = i as Run;
                if (r != null)
                {
                    bool hasLink = ProcessRuns(r);
                    if (hasLink)
                    {
                        foundLink = true;
                    }
                }

                i = i.NextInline;
            }

            return foundLink;
        }

        private bool ProcessRuns(Run run)
        {
            bool hasLink = false;

            Match match = _uriRegex.Match(run.Text);
            if ((match != null) && (match.Length > 0))
            {
                TextPointer start = run.ContentStart.GetPositionAtOffset(match.Index);
                TextPointer end = start.GetPositionAtOffset(match.Length);
                string linkText = run.Text.Substring(match.Index, match.Length);

                if ((start.IsAtInsertionPosition) && (end.IsAtInsertionPosition))
                {
                    Hyperlink link = new Hyperlink(start, end);
                    link.NavigateUri = new Uri(linkText);
                    link.RequestNavigate += new RequestNavigateEventHandler(link_RequestNavigate);

                    hasLink = true;
                }
            }

            return hasLink;
        }

        void link_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            if (e.Uri != null)
            {
                DefaultBrowser.OpenUrl(e.Uri.ToString());
            }
        }

        private void RaiseSaveNote(string note, int noteId)
        {
            if (SaveNote != null)
            {
                SaveNote(this, new SaveNoteEventArgs(note, noteId));
            }
        }

        private void ConfigureEditMode(StackHashNoteEntry editNote)
        {
            _currentEditNote = editNote;
            if (_currentEditNote == null)
            {
                textBoxNote.Text = string.Empty;
            }
            else
            {
                textBoxNote.Text = _currentEditNote.Note;
            }

            textAddNote.Visibility = Visibility.Collapsed;
            textBoxNote.Visibility = Visibility.Visible;
            panelButtons.Visibility = Visibility.Visible;

            Keyboard.Focus(textBoxNote);
        }

        private void ConfigureReadMode()
        {
            textAddNote.Visibility = Visibility.Visible;
            textBoxNote.Visibility = Visibility.Collapsed;
            panelButtons.Visibility = Visibility.Collapsed;
        }

        private void linkAddNote_Click(object sender, RoutedEventArgs e)
        {
            ConfigureEditMode(null);
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            int noteId = 0;
            if (_currentEditNote != null)
            {
                noteId = _currentEditNote.NoteId;
            }

            RaiseSaveNote(textBoxNote.Text.Trim(), noteId);
            ConfigureReadMode();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            ConfigureReadMode();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ConfigureReadMode();
        }
    }
}
