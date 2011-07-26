using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Data.SqlClient;
using System.Data.Common;

using StackHashBusinessObjects;
using StackHashErrorIndex;
using StackHashUtilities;

namespace ErrorIndexUnitTests
{
    /// <summary>
    /// Summary description for MoveIndexUnitTests
    /// </summary>
    [TestClass]
    public class MoveIndexUnitTests
    {

        private String m_TestFolder = "C:\\StackHashUnitTests\\MoveIndexTests\\";
        private String m_TestFolder2 = "C:\\StackHashUnitTests\\MoveIndexTests2\\";
        private String m_TestFolder3 = "Z:\\StackHashUnitTests\\MoveIndexTests3\\";
        private String m_TestFolderZ = "Z:\\StackHashUnitTests";
        private IErrorIndex m_ErrorIndex = null;
        private StackHashSqlConfiguration m_SqlSettings;
        private int m_MoveFileCount;
        static String s_MasterConnectionString = TestSettings.DefaultConnectionString + "Initial Catalog=MASTER;";

        
        public MoveIndexUnitTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //

        /// <summary>
        /// Use TestInitialize to run code before running each test 
        /// </summary>
        [TestInitialize()]
        public void MyTestInitialize() 
        {
            m_SqlSettings = StackHashSqlConfiguration.Default;
            tidyTest();
            m_MoveFileCount = 0;
        }
        
        /// <summary>
        /// Use TestCleanup to run code after each test has run
        /// </summary>
        [TestCleanup()]
        public void MyTestCleanup() 
        {
            tidyTest();
        }

        private void tidyTest()
        {
            SqlConnection.ClearAllPools();

            if (m_ErrorIndex != null)
            {
                m_ErrorIndex.Deactivate();
                try { m_ErrorIndex.DeleteIndex(); }
                catch { ; }
                m_ErrorIndex.Dispose();
                SqlConnection.ClearAllPools();
                m_ErrorIndex = null;
            }

            String sourceDatabaseName = "MoveIndexTestDatabase1";
            String destDatabaseName = "MoveIndexTestDatabase2";

            DbProviderFactory providerFactory = DbProviderFactories.GetFactory("System.Data.SqlClient");

            SqlCommands sqlCommands = new SqlCommands(providerFactory, s_MasterConnectionString, s_MasterConnectionString, 1);

            // Delete any remnants that may exist from a previous test.
            SqlConnection.ClearAllPools();
            try { sqlCommands.DeleteDatabase(sourceDatabaseName); }
            catch (System.Exception ex){ Console.WriteLine("Failed to delete: " + sourceDatabaseName + " " + ex.ToString());};

            SqlConnection.ClearAllPools();
            try { sqlCommands.DeleteDatabase(destDatabaseName); }
            catch (System.Exception ex) { Console.WriteLine("Failed to delete: " + destDatabaseName + " " + ex.ToString()); };

            sqlCommands.Dispose();

            SqlConnection.ClearAllPools();

            if (Directory.Exists(m_TestFolder))
                PathUtils.DeleteDirectory(m_TestFolder, true);
            if (Directory.Exists(m_TestFolder2))
                PathUtils.DeleteDirectory(m_TestFolder2, true);
            if (Directory.Exists(m_TestFolder3))
                PathUtils.DeleteDirectory(m_TestFolder3, true);

        }
        
        #endregion


        
        /// <summary>
        /// Move a database before it has been created.
        /// This should just change the name and location of the database.
        /// No SQL commands will be issued.
        /// </summary>
        [TestMethod]
        public void RenameIndexBeforeActive()
        {
            // Create an index with a particular name. Then change the name of the index.
            String sourceDatabaseName = "MoveIndexTestDatabase1";
            String destDatabaseName = "MoveIndexTestDatabase2";

            m_SqlSettings.InitialCatalog = sourceDatabaseName;
            m_ErrorIndex = new SqlErrorIndex(m_SqlSettings, sourceDatabaseName, m_TestFolder);

            m_SqlSettings.InitialCatalog = destDatabaseName;
            m_ErrorIndex.MoveIndex(m_TestFolder, destDatabaseName, m_SqlSettings, true);

            Assert.AreEqual(destDatabaseName, m_ErrorIndex.ErrorIndexName);
            Assert.AreEqual(m_TestFolder, m_ErrorIndex.ErrorIndexPath);
        }

        private void addDataToIndex(IErrorIndex index)
        {
            int numProducts = 100;

            int cabId = 12345678;
            int fileId = 2000;
            int eventId = 1000;

            for (int productCount = 0; productCount < numProducts; productCount++)
            {
                DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
                DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
                StackHashProduct product1 =
                    new StackHashProduct(creationDateTime, modifiedDateTime, null, (productCount + 10), "TestProduct1", 20, 30, "2.10.02123.1293");

                index.AddProduct(product1);

                StackHashFile file1 = new StackHashFile(creationDateTime, modifiedDateTime, fileId++, creationDateTime, "FileName", "1.2.3.4");
                index.AddFile(product1, file1);

                StackHashEventSignature eventSignature = new StackHashEventSignature();
                eventSignature.Parameters = new StackHashParameterCollection();
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
                eventSignature.InterpretParameters();

                StackHashEvent thisEvent = new StackHashEvent(creationDateTime, modifiedDateTime, "CLR20", eventId++, eventSignature, 1, file1.Id);
                index.AddEvent(product1, file1, thisEvent);


                StackHashCab cab = new StackHashCab(creationDateTime, modifiedDateTime, thisEvent.Id, thisEvent.EventTypeName, "cab12345_23232.cab", cabId++, 12000);
                cab.DumpAnalysis = new StackHashDumpAnalysis("2 days, 5 hours, 2 mins", "1 hour, 2 mins", "2.120.222.1121212", "Microsoft Windows Vista X64 6.0.212121212 (Build 2500)", "64 bit windows");

                cab.CabDownloaded = true;
                index.AddCab(product1, file1, thisEvent, cab, true);

                String cabFolder = index.GetCabFolder(product1, file1, thisEvent, cab);
                String cabFileName = index.GetCabFileName(product1, file1, thisEvent, cab);

                if (!Directory.Exists(cabFolder))
                    Directory.CreateDirectory(cabFolder);

                File.Copy(TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab", cabFileName);

                FileAttributes originalAttributes = File.GetAttributes(cabFileName);
                File.SetAttributes(cabFileName, originalAttributes & ~FileAttributes.ReadOnly);
            }
        }

        private void checkIndexData(IErrorIndex index)
        {
            int numProducts = 100;
            int cabId = 12345678;
            int fileId = 2000;
            int eventId = 1000;


            for (int productCount = 0; productCount < numProducts; productCount++)
            {
                StackHashProduct retrievedProduct = index.GetProduct(productCount + 10);

                DateTime creationDateTime = new DateTime(2010, 04, 04, 22, 9, 0, DateTimeKind.Utc);
                DateTime modifiedDateTime = new DateTime(2010, 05, 05, 23, 10, 0, DateTimeKind.Utc);
                StackHashProduct product1 =
                    new StackHashProduct(creationDateTime, modifiedDateTime, null, (productCount + 10), "TestProduct1", 20, 30, "2.10.02123.1293");

                Assert.AreEqual(0, product1.CompareTo(retrievedProduct));


                StackHashFile file1 = new StackHashFile(creationDateTime, modifiedDateTime, fileId++, creationDateTime, "FileName", "1.2.3.4");

                StackHashFile retrievedFile = index.GetFile(product1, file1.Id);
                Assert.AreEqual(0, file1.CompareTo(retrievedFile));
                
                StackHashEventSignature eventSignature = new StackHashEventSignature();
                eventSignature.Parameters = new StackHashParameterCollection();
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, "AppName"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, "1.2.3.4"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, creationDateTime.ToString()));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, "ModuleName"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, "2.3.4.5"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, creationDateTime.ToString()));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, "1234"));
                eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, "0x1234"));
                eventSignature.InterpretParameters();

                StackHashEvent thisEvent = new StackHashEvent(creationDateTime, modifiedDateTime, "CLR20", eventId++, eventSignature, 1, file1.Id);

                StackHashEvent retrievedEvent = index.GetEvent(product1, file1, thisEvent);
                Assert.AreEqual(0, thisEvent.CompareTo(retrievedEvent));

                StackHashCab cab = new StackHashCab(creationDateTime, modifiedDateTime, thisEvent.Id, thisEvent.EventTypeName, "cab12345_23232.cab", cabId++, 12000);
                cab.DumpAnalysis = new StackHashDumpAnalysis("2 days, 5 hours, 2 mins", "1 hour, 2 mins", "2.120.222.1121212", "Microsoft Windows Vista X64 6.0.212121212 (Build 2500)", "64 bit windows");

                cab.CabDownloaded = true;

                StackHashCab retrievedCab = index.GetCab(product1, file1, thisEvent, cab.Id);
                Assert.AreEqual(0, cab.CompareTo(retrievedCab));

                String cabFolder = index.GetCabFolder(product1, file1, thisEvent, cab);
                String cabFileName = index.GetCabFileName(product1, file1, thisEvent, cab);

                Assert.AreEqual(true, Directory.Exists(cabFolder));
                Assert.AreEqual(true, File.Exists(cabFileName));
            }
        }


        /// <summary>
        /// Renames an active database.
        /// The database should still be stored in the same location. However, the name should have changed.
        /// </summary>
        [TestMethod]
        public void RenameActiveDatabase()
        {
            // Create an index with a particular name. Then change the name of the index.
            String sourceDatabaseName = "MoveIndexTestDatabase1";
            String destDatabaseName = "MoveIndexTestDatabase2";

            m_SqlSettings.InitialCatalog = sourceDatabaseName;
            m_ErrorIndex = new SqlErrorIndex(m_SqlSettings, sourceDatabaseName, m_TestFolder);
            try { m_ErrorIndex.DeleteIndex(); }
            catch { ; }

            // This call will create the database.
            m_ErrorIndex.Activate();

            // Can't move an active database.
            m_ErrorIndex.Deactivate();

            m_SqlSettings.InitialCatalog = destDatabaseName;
            m_ErrorIndex.MoveIndex(m_TestFolder, destDatabaseName, m_SqlSettings, true);

            Assert.AreEqual(destDatabaseName, m_ErrorIndex.ErrorIndexName);
            Assert.AreEqual(m_TestFolder, m_ErrorIndex.ErrorIndexPath);

            m_ErrorIndex.Deactivate();
            m_ErrorIndex.Dispose();

            // Now reload and make sure it is present.
            m_ErrorIndex = new SqlErrorIndex(m_SqlSettings, destDatabaseName, m_TestFolder);
            Assert.AreEqual(ErrorIndexStatus.Created, m_ErrorIndex.Status);

            Assert.AreEqual(true, Directory.Exists(Path.Combine(m_TestFolder, destDatabaseName)));
            Assert.AreEqual(false, Directory.Exists(Path.Combine(m_TestFolder, sourceDatabaseName)));

            m_ErrorIndex.Activate();
            m_ErrorIndex.Deactivate();        
        }


        /// <summary>
        /// Renames an active database - same name - should have no effect.
        /// </summary>
        [TestMethod]
        public void RenameActiveDatabaseSameName()
        {
            // Create an index with a particular name. Then change the name of the index.
            String sourceDatabaseName = "MoveIndexTestDatabase1";
            String destDatabaseName = "MoveIndexTestDatabase1";

            m_SqlSettings.InitialCatalog = sourceDatabaseName;
            m_ErrorIndex = new SqlErrorIndex(m_SqlSettings, sourceDatabaseName, m_TestFolder);
            try { m_ErrorIndex.DeleteIndex(); }
            catch { ; }

            // This call will create the database.
            m_ErrorIndex.Activate();

            // Can't move an active database.
            m_ErrorIndex.Deactivate();

            m_SqlSettings.InitialCatalog = destDatabaseName;
            m_ErrorIndex.MoveIndex(m_TestFolder, destDatabaseName, m_SqlSettings, true);

            Assert.AreEqual(destDatabaseName, m_ErrorIndex.ErrorIndexName);
            Assert.AreEqual(m_TestFolder, m_ErrorIndex.ErrorIndexPath);

            m_ErrorIndex.Deactivate();
            m_ErrorIndex.Dispose();

            // Now reload and make sure it is present.
            m_ErrorIndex = new SqlErrorIndex(m_SqlSettings, destDatabaseName, m_TestFolder);
            Assert.AreEqual(ErrorIndexStatus.Created, m_ErrorIndex.Status);

            Assert.AreEqual(true, Directory.Exists(Path.Combine(m_TestFolder, destDatabaseName)));

            m_ErrorIndex.Activate();
            m_ErrorIndex.Deactivate();
        }


        /// <summary>
        /// Move to a new folder same drive - same name.
        /// </summary>
        [TestMethod]
        public void MoveDatabaseToNewFolder()
        {
            // Create an index with a particular name. Then change the name of the index.
            String sourceDatabaseName = "MoveIndexTestDatabase1";
            String destDatabaseName = "MoveIndexTestDatabase1";

            m_SqlSettings.InitialCatalog = sourceDatabaseName;
            m_ErrorIndex = new SqlErrorIndex(m_SqlSettings, sourceDatabaseName, m_TestFolder);
            try { m_ErrorIndex.DeleteIndex(); }
            catch { ; }

            // This call will create the database.
            m_ErrorIndex.Activate();

            // Can't move an active database.
            m_ErrorIndex.Deactivate();

            // Must create the destination folder.
            if (!Directory.Exists(m_TestFolder2))
                Directory.CreateDirectory(m_TestFolder2);

            m_SqlSettings.InitialCatalog = destDatabaseName;
            m_ErrorIndex.MoveIndex(m_TestFolder2, destDatabaseName, m_SqlSettings, true);

            Assert.AreEqual(destDatabaseName, m_ErrorIndex.ErrorIndexName);
            Assert.AreEqual(m_TestFolder2, m_ErrorIndex.ErrorIndexPath);

            m_ErrorIndex.Deactivate();
            m_ErrorIndex.Dispose();

            // Now reload and make sure it is present.
            m_ErrorIndex = new SqlErrorIndex(m_SqlSettings, destDatabaseName, m_TestFolder2);
            Assert.AreEqual(ErrorIndexStatus.Created, m_ErrorIndex.Status);

            Assert.AreEqual(true, Directory.Exists(Path.Combine(m_TestFolder2, destDatabaseName)));
            Assert.AreEqual(false, Directory.Exists(Path.Combine(m_TestFolder, sourceDatabaseName)));

            m_ErrorIndex.Activate();
            m_ErrorIndex.Deactivate();
        }

        ///// <summary>
        ///// Move to a new folder same drive - different name.
        ///// </summary>
        //[TestMethod]
        //public void MoveDatabaseToNewFolderNewNameRep()
        //{
        //    for (int i = 0; i < 100; i++)
        //    {
        //        Console.WriteLine("Rep: " + i.ToString());
        //        if (i != 0)
        //            MyTestInitialize();
        //        MoveDatabaseToNewFolderNewName();
        //        MyTestCleanup();
        //    }
        //}


        /// <summary>
        /// Move to a new folder same drive - different name.
        /// </summary>
        [TestMethod]
        public void MoveDatabaseToNewFolderNewName()
        {
            // Create an index with a particular name. Then change the name of the index.
            String sourceDatabaseName = "MoveIndexTestDatabase1";
            String destDatabaseName = "MoveIndexTestDatabase2";


            m_SqlSettings.InitialCatalog = sourceDatabaseName;
            m_ErrorIndex = new SqlErrorIndex(m_SqlSettings, sourceDatabaseName, m_TestFolder);
            try { m_ErrorIndex.DeleteIndex(); }
            catch { ; }

            // This call will create the database.
            m_ErrorIndex.Activate();

            // Can't move an active database.
            m_ErrorIndex.Deactivate();

            // Must create the destination folder.
            if (!Directory.Exists(m_TestFolder2))
                Directory.CreateDirectory(m_TestFolder2);

            m_SqlSettings.InitialCatalog = destDatabaseName;
            m_ErrorIndex.MoveIndex(m_TestFolder2, destDatabaseName, m_SqlSettings, true);

            Assert.AreEqual(destDatabaseName, m_ErrorIndex.ErrorIndexName);
            Assert.AreEqual(m_TestFolder2, m_ErrorIndex.ErrorIndexPath);

            m_ErrorIndex.Deactivate();
            m_ErrorIndex.Dispose();

            // Now reload and make sure it is present.
            m_ErrorIndex = new SqlErrorIndex(m_SqlSettings, destDatabaseName, m_TestFolder2);
            Assert.AreEqual(ErrorIndexStatus.Created, m_ErrorIndex.Status);

            Assert.AreEqual(true, Directory.Exists(Path.Combine(m_TestFolder2, destDatabaseName)));
            Assert.AreEqual(false, Directory.Exists(Path.Combine(m_TestFolder, sourceDatabaseName)));

            m_ErrorIndex.Activate();
            m_ErrorIndex.Deactivate();
        }

        private void errorIndexMoveCallback(Object sender, ErrorIndexMoveEventArgs e)
        {
            m_MoveFileCount++;
        }
        
        /// <summary>
        /// Move to a new folder different drive - same name.
        /// </summary>
        [TestMethod]
        public void MoveDatabaseToNewFolderDifferentDrive()
        {
            // Only run this test on machine with a Z drive mapped.
            if (!Directory.Exists(m_TestFolderZ))
                return;

            // Create an index with a particular name. Then change the name of the index.
            String sourceDatabaseName = "MoveIndexTestDatabase1";
            String destDatabaseName = "MoveIndexTestDatabase1";

            m_SqlSettings.InitialCatalog = sourceDatabaseName;
            m_ErrorIndex = new SqlErrorIndex(m_SqlSettings, sourceDatabaseName, m_TestFolder);
            try { m_ErrorIndex.DeleteIndex(); }
            catch { ; }

            // This call will create the database.
            m_ErrorIndex.Activate();

            // Can't move an active database.
            m_ErrorIndex.Deactivate();

            // Must create the destination folder.
            if (!Directory.Exists(m_TestFolder3))
                Directory.CreateDirectory(m_TestFolder3);

            m_SqlSettings.InitialCatalog = destDatabaseName;

            m_ErrorIndex.IndexMoveProgress += new EventHandler<ErrorIndexMoveEventArgs>(this.errorIndexMoveCallback);

            try
            {
                m_ErrorIndex.MoveIndex(m_TestFolder3, destDatabaseName, m_SqlSettings, true);

                Assert.AreEqual(destDatabaseName, m_ErrorIndex.ErrorIndexName);
                Assert.AreEqual(m_TestFolder3, m_ErrorIndex.ErrorIndexPath);

                m_ErrorIndex.Deactivate();
                m_ErrorIndex.Dispose();

                // Now reload and make sure it is present.
                m_ErrorIndex = new SqlErrorIndex(m_SqlSettings, destDatabaseName, m_TestFolder3);
                Assert.AreEqual(ErrorIndexStatus.Created, m_ErrorIndex.Status);

                Assert.AreEqual(true, Directory.Exists(Path.Combine(m_TestFolder3, destDatabaseName)));
                Assert.AreEqual(false, Directory.Exists(Path.Combine(m_TestFolder, sourceDatabaseName)));

                Assert.AreEqual(true, m_MoveFileCount > 0);

                m_ErrorIndex.Activate();
                m_ErrorIndex.Deactivate();
            }
            finally
            {
                m_ErrorIndex.IndexMoveProgress -= new EventHandler<ErrorIndexMoveEventArgs>(this.errorIndexMoveCallback);
            }
        }


        /// <summary>
        /// Move to a new folder different drive - new name.
        /// </summary>
        [TestMethod]
        public void MoveDatabaseToNewFolderDifferentDriveNewName()
        {
            // Only run this test on machine with a Z drive mapped.
            if (!Directory.Exists(m_TestFolderZ))
                return;

            // Create an index with a particular name. Then change the name of the index.
            String sourceDatabaseName = "MoveIndexTestDatabase1";
            String destDatabaseName = "MoveIndexTestDatabase2";

            m_SqlSettings.InitialCatalog = sourceDatabaseName;
            m_ErrorIndex = new SqlErrorIndex(m_SqlSettings, sourceDatabaseName, m_TestFolder);
            try { m_ErrorIndex.DeleteIndex(); }
            catch { ; }

            // This call will create the database.
            m_ErrorIndex.Activate();

            // Can't move an active database.
            m_ErrorIndex.Deactivate();

            // Must create the destination folder.
            if (!Directory.Exists(m_TestFolder3))
                Directory.CreateDirectory(m_TestFolder3);

            m_SqlSettings.InitialCatalog = destDatabaseName;
            m_ErrorIndex.MoveIndex(m_TestFolder3, destDatabaseName, m_SqlSettings, true);

            Assert.AreEqual(destDatabaseName, m_ErrorIndex.ErrorIndexName);
            Assert.AreEqual(m_TestFolder3, m_ErrorIndex.ErrorIndexPath);

            m_ErrorIndex.Deactivate();
            m_ErrorIndex.Dispose();

            // Now reload and make sure it is present.
            m_ErrorIndex = new SqlErrorIndex(m_SqlSettings, destDatabaseName, m_TestFolder3);
            Assert.AreEqual(ErrorIndexStatus.Created, m_ErrorIndex.Status);

            Assert.AreEqual(true, Directory.Exists(Path.Combine(m_TestFolder3, destDatabaseName)));
            Assert.AreEqual(false, Directory.Exists(Path.Combine(m_TestFolder, sourceDatabaseName)));

            m_ErrorIndex.Activate();
            m_ErrorIndex.Deactivate();
        }


        /// <summary>
        /// Move to a new folder different drive - new name - with data.
        /// </summary>
        [TestMethod]
        public void MoveDatabaseToNewFolderDifferentDriveNewNameWithData()
        {
            // Only run this test on machine with a Z drive mapped.
            if (!Directory.Exists(m_TestFolderZ))
                return;

            // Create an index with a particular name. Then change the name of the index.
            String sourceDatabaseName = "MoveIndexTestDatabase1";
            String destDatabaseName = "MoveIndexTestDatabase2";

            m_SqlSettings.InitialCatalog = sourceDatabaseName;
            m_ErrorIndex = new SqlErrorIndex(m_SqlSettings, sourceDatabaseName, m_TestFolder);
            try { m_ErrorIndex.DeleteIndex(); }
            catch { ; }

            // This call will create the database.
            m_ErrorIndex.Activate();

            addDataToIndex(m_ErrorIndex);

            // Can't move an active database.
            m_ErrorIndex.Deactivate();

            // Must create the destination folder.
            if (!Directory.Exists(m_TestFolder3))
                Directory.CreateDirectory(m_TestFolder3);

            m_SqlSettings.InitialCatalog = destDatabaseName;
            m_ErrorIndex.MoveIndex(m_TestFolder3, destDatabaseName, m_SqlSettings, true);

            Assert.AreEqual(destDatabaseName, m_ErrorIndex.ErrorIndexName);
            Assert.AreEqual(m_TestFolder3, m_ErrorIndex.ErrorIndexPath);

            m_ErrorIndex.Deactivate();
            m_ErrorIndex.Dispose();

            // Now reload and make sure it is present.
            m_ErrorIndex = new SqlErrorIndex(m_SqlSettings, destDatabaseName, m_TestFolder3);
            Assert.AreEqual(ErrorIndexStatus.Created, m_ErrorIndex.Status);

            checkIndexData(m_ErrorIndex);
            m_ErrorIndex.Activate();

            Assert.AreEqual(true, Directory.Exists(Path.Combine(m_TestFolder3, destDatabaseName)));
            Assert.AreEqual(false, Directory.Exists(Path.Combine(m_TestFolder, sourceDatabaseName)));

            m_ErrorIndex.Deactivate();
        }
    }
}
