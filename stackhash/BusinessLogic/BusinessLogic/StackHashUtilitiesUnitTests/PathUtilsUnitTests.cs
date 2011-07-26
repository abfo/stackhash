using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

using StackHashUtilities;

namespace StackHashUtilitiesUnitTests
{
    /// <summary>
    /// Summary description for PathUtilsUnitTests
    /// </summary>
    [TestClass]
    public class PathUtilsUnitTests
    {
        public PathUtilsUnitTests()
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
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void GetNetworkDriveUncName_NotANetworkDrive()
        {
            String mappedDrive = PathUtils.GetNetworkDriveUncName("C:");
            Assert.AreEqual(true, String.IsNullOrEmpty(mappedDrive));
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void GetNetworkDriveUncName_InvalidPathSpecified()
        {
            String mappedDrive = PathUtils.GetNetworkDriveUncName("ABC");
            Assert.AreEqual(true, String.IsNullOrEmpty(mappedDrive));
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void GetNetworkDriveUncName_SubstDriveInvalid()
        {
            String mappedDrive = PathUtils.GetNetworkDriveUncName("R:\\");
            Assert.AreEqual(false, String.IsNullOrEmpty(mappedDrive));
        }

        [TestMethod]
        public void GetNetworkDriveUncName_SubstDriveUpperCase()
        {
            String mappedDrive = PathUtils.GetNetworkDriveUncName("R:");
            Assert.AreEqual(true, String.IsNullOrEmpty(mappedDrive));
        }

        [TestMethod]
        public void GetNetworkDriveUncName_SubstDriveLowerCase()
        {
            String mappedDrive = PathUtils.GetNetworkDriveUncName("r:");
            Assert.AreEqual(true, String.IsNullOrEmpty(mappedDrive));
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void GetNetworkDriveUncName_Null()
        {
            String mappedDrive = PathUtils.GetNetworkDriveUncName(null);
            Assert.AreEqual(true, String.IsNullOrEmpty(mappedDrive));
        }

        [TestMethod]
        public void GetNetworkDriveUncName_DriveX()
        {
            DriveInfo info = new DriveInfo("x");

            if (info.DriveType == DriveType.Network)
            {
                if (info.IsReady)
                {
                    // Assume it is a network drive.
                    String mappedDrive = PathUtils.GetNetworkDriveUncName("x:");
                    Assert.AreEqual(false, String.IsNullOrEmpty(mappedDrive));
                }
            }
        }

        [TestMethod]
        public void GetNetworkDriveUncName_AllValidDriveLetters()
        {
            // Enumerate all logical drives.
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            foreach (DriveInfo driveInfo in allDrives)
            {
                Console.WriteLine(driveInfo.Name + "- rdy: " + driveInfo.IsReady.ToString());
                if (driveInfo.IsReady)
                {
                    Console.WriteLine(driveInfo.Name + "-" + driveInfo.RootDirectory + "-" + driveInfo.IsReady.ToString() + "-" + driveInfo.DriveType.ToString() + "-" + driveInfo.DriveFormat.ToString());
                    String driveStr = driveInfo.RootDirectory.ToString();
                    driveStr = driveStr.Substring(0, 2);
                    // Assume it is a network drive.
                    String mappedDrive = PathUtils.GetNetworkDriveUncName(driveStr);
                    if (!String.IsNullOrEmpty(mappedDrive))
                    {
                        Console.WriteLine("Mapping: " + mappedDrive);
                    }
                    else
                    {
                        Console.WriteLine("No Mapping");
                    }

                    if (driveInfo.DriveType == DriveType.Network)
                    {
                        Assert.AreEqual(false, String.IsNullOrEmpty(mappedDrive));
                    }
                    else
                    {
                        Assert.AreEqual(true, String.IsNullOrEmpty(mappedDrive));
                    }
                }
            }
        }

        [TestMethod]
        public void GetPhysicalPath_LocalDriveShouldMapToSame()
        {
            String originalPath = "C:\\";
            String mappedPath = PathUtils.GetPhysicalPath(originalPath);
            Assert.AreEqual(originalPath, mappedPath);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void GetPhysicalPath_Null()
        {
            String originalPath = null;
            String mappedPath = PathUtils.GetPhysicalPath(originalPath);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void GetPhysicalPath_NotRooted()
        {
            String originalPath = "notrooted";
            String mappedPath = PathUtils.GetPhysicalPath(originalPath);
        }

        [TestMethod]
        public void GetPhysicalPath_LocalPathWithSubfolder()
        {
            String originalPath = "C:\\test";
            String mappedPath = PathUtils.GetPhysicalPath(originalPath);
            Assert.AreEqual(0, String.Compare(mappedPath, originalPath));
        }

        [TestMethod]
        public void GetPhysicalPath_SubstedLocalDrive()
        {
            String originalPath = "r:\\";
            String mappedPath = PathUtils.GetPhysicalPath(originalPath);
            Assert.AreNotEqual(null, mappedPath);
            Console.WriteLine(originalPath + "->" + mappedPath);
            Assert.AreNotEqual(originalPath, mappedPath);
        }

        [TestMethod]
        public void GetPhysicalPath_SubstedLocalDriveWithSubfolder()
        {
            String folder = "test";
            String originalPath = "r:\\" + folder;
            String mappedPath = PathUtils.GetPhysicalPath(originalPath);
            Assert.AreNotEqual(null, mappedPath);
            Console.WriteLine(originalPath + "->" + mappedPath);
            Assert.AreNotEqual(0, String.Compare(mappedPath, originalPath));
            Assert.AreEqual(true, mappedPath.EndsWith(folder));
        }

        [TestMethod]
        public void GetPhysicalPath_NetworkDrive()
        {
            // Enumerate all logical drives.
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            foreach (DriveInfo driveInfo in allDrives)
            {
                if (driveInfo.IsReady)
                {
                    if (driveInfo.DriveType == DriveType.Network)
                    {
                        // Try the raw root folder first.
                        String mappedPath = PathUtils.GetPhysicalPath(driveInfo.RootDirectory.ToString());
                        Assert.AreNotEqual(null, mappedPath);
                        Console.WriteLine(driveInfo.RootDirectory.ToString() + "->" + mappedPath);

                        // Try a subfolder.
                        String folder = "test";
                        String originalPath = driveInfo.RootDirectory.ToString() + folder;
                        mappedPath = PathUtils.GetPhysicalPath(originalPath);
                        Assert.AreNotEqual(null, mappedPath);

                        Assert.AreNotEqual(0, String.Compare(mappedPath, originalPath));
                        Assert.AreEqual(true, mappedPath.EndsWith(folder));
                    }
                }
            }
        }

        [TestMethod]
        public void GetPhysicalPath_DriveLetterNotAssigned()
        {
            // Enumerate all logical drives.
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            Dictionary<String, bool> driveDictionary = new Dictionary<string, bool>();

            foreach (DriveInfo driveInfo in allDrives)
            {
                driveDictionary[driveInfo.RootDirectory.ToString()] = true;
            }

            String foundDrive = null;

            for (char i = 'C'; i <= 'Z'; i++)
            {
                String rootStr = i.ToString() + ":\\";

                if (!driveDictionary.ContainsKey(rootStr))
                {
                    foundDrive = rootStr;
                    break;
                }
            }

            if (foundDrive != null)
            {
                String mappedPath = PathUtils.GetPhysicalPath(foundDrive);
                Assert.AreEqual(foundDrive, mappedPath);
            }
        }
    }
}
