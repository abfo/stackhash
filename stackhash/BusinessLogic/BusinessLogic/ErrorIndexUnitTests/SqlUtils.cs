using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using StackHashErrorIndex;
using System.Data.SqlClient;

using StackHashUtilities;


namespace ErrorIndexUnitTests
{
    internal class SqlUtils
    {
        static String s_ConnectionString = TestSettings.DefaultConnectionString + "Initial Catalog=MASTER;";
        static String s_MasterConnectionString = TestSettings.DefaultConnectionString + "Initial Catalog=MASTER;";
        static String s_UnitTestDatabase = "StackHashUnitTests";


        public static String ConnectionString
        {
            get
            {
                return s_ConnectionString;
            }
        }

        public static String UnitTestDatabase
        {
            get
            {
                return s_UnitTestDatabase;
            }
        }


        /// <summary>
        /// Deletes the unit test database.
        /// </summary>
        /// <param name="providerFactory"></param>
        public static void DeleteTestDatabase(DbProviderFactory providerFactory)
        {
            SqlCommands commands = new SqlCommands(providerFactory, s_ConnectionString, s_MasterConnectionString, 1);

            commands.DeleteDatabase(s_UnitTestDatabase);
        }

    }
}
