using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StackHashDBConfig
{
    /// <summary>
    /// List of candidates for a StackHash database
    /// </summary>
    class DatabaseList
    {
        /// <summary>
        /// Database candidates
        /// </summary>
        public List<DatabaseSettings> Candidates { get; private set; }

        /// <summary>
        /// Selected database candidate
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public DatabaseSettings SelectedCandidate { get; set; }

        /// <summary>
        /// List of candidates for a StackHash database
        /// </summary>
        public DatabaseList()
        {
            DatabaseSettingsSqlServer defaultDatabase = new DatabaseSettingsSqlServer();
            
            this.Candidates = new List<DatabaseSettings>();
            this.Candidates.Add(defaultDatabase);

            this.SelectedCandidate = defaultDatabase;
        }
    }
}
