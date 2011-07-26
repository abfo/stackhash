using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace StackHashBusinessObjects
{
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashNoteEntry : IComparable
    {
        private DateTime m_TimeOfEntry;
        private String m_Source;
        private String m_Note;
        private String m_User;
        private int m_NoteId;

        public StackHashNoteEntry() 
        { 
            m_TimeOfEntry = DateTime.Now.ToUniversalTime();
        }

        public StackHashNoteEntry(DateTime timeOfEntry, String source, String user, String note)
        {
            m_TimeOfEntry = timeOfEntry;
            m_Source = source;
            m_User = user;
            m_Note = note;
        }

        public StackHashNoteEntry(DateTime timeOfEntry, String source, String user, String note, int noteId)
        {
            m_TimeOfEntry = timeOfEntry;
            m_Source = source;
            m_User = user;
            m_Note = note;
            m_NoteId = noteId;
        }

        
        [DataMember]
        public DateTime TimeOfEntry
        {
            get { return m_TimeOfEntry; }
            set { m_TimeOfEntry = value; }
        }

        [DataMember]
        public String Source
        {
            get { return m_Source; }
            set { m_Source = value; }
        }

        [DataMember]
        public String Note
        {
            get { return m_Note; }
            set { m_Note = value; }
        }

        [DataMember]
        public String User
        {
            get { return m_User; }
            set { m_User = value; }
        }

        [DataMember]
        public int NoteId
        {
            get { return m_NoteId; }
            set { m_NoteId = value; }
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            StackHashNoteEntry entryToCompare = obj as StackHashNoteEntry;

            // Don't compare the note ID as this is generated.

            if ((this.Source.ToUpperInvariant() == entryToCompare.Source.ToUpperInvariant()) &&
                (this.Note == entryToCompare.Note) &&
                (this.User.ToUpperInvariant() == entryToCompare.User.ToUpperInvariant()) &&
                (this.TimeOfEntry.Date == entryToCompare.TimeOfEntry.Date) &&
                (this.TimeOfEntry.Hour == entryToCompare.TimeOfEntry.Hour) &&
                (this.TimeOfEntry.Minute == entryToCompare.TimeOfEntry.Minute) &&
                (this.TimeOfEntry.Second == entryToCompare.TimeOfEntry.Second))
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }

        #endregion
    }

    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashNotes : Collection<StackHashNoteEntry>
    {
        public StackHashNotes() { } // Needed to serialize.


        public bool ContainsNote(StackHashNoteEntry noteToCheck)
        {
            if (noteToCheck == null)
                throw new ArgumentNullException("noteToCheck");

            foreach (StackHashNoteEntry note in this)
            {
                if (noteToCheck.CompareTo(note) == 0)
                    return true;
            }

            return false;
        }

        public int MatchingCount(StackHashNoteEntry noteToCheck)
        {
            if (noteToCheck == null)
                throw new ArgumentNullException("noteToCheck");


            int matches = 0;
            foreach (StackHashNoteEntry note in this)
            {
                if (noteToCheck.CompareTo(note) == 0)
                    matches++;
            }

            return matches;
        }
    }
}
