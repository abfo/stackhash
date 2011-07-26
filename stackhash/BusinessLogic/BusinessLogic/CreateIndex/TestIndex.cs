using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

using StackHashBusinessObjects;
using StackHashErrorIndex;

namespace CreateIndex
{
    internal class TestIndex
    {
        static String [] s_FileNames = new String [] 
        {
            "StackHash.exe",
            "setup.exe",
            "StackHashUtilities.dll",
            "StackHashBusinessObjects.dll",
            "Crashy.exe",
            "Tasks.dll",
            "ServiceInterface.dll",
            "Service.exe"
        };

        static String [] s_ModuleNames = new String [] 
        {
            "Kernel32.dll",
            "User32.dll",
            "StackHash.exe",
            "mscoree.dll",
            "mscorwks.dll",
            "mstypes.dll",
            "gdi32.dll"
        };


        static String[] s_EventTypes = new String[] 
        {
            "CLR20 Managed Crash",
            "Crash 32bit",
            "Crash 64bit",
            "Hang",
            "Hang XProc"
        };

        static String[] s_OperatingSystems = new String[]
        {
            "Windows Vista",
            "Windows XP SP3",
            "Windows XP SP2",
            "Windows Vista SP2",
            "Windows Vista SP1",
            "Windows 7",
            "Windows Vista SP1 / Windows Server 2008",
        };

        static String[] s_OperatingSystemVersions = new String[]
        {
            "6.0.6000.2.0.0",
            "5.1.2600.2.3.0",
            "5.1.2600.3.4.0",
            "6.0.6002.2.1.0",
            "6.0.6001.2.1.0",
            "6.1.7600.2.0.0",
            "6.0.6001.2.1.0",
        };

        
        struct Language 
        {
            String m_Name;
            int m_Lcid;

            public String Name
            {
                get { return m_Name; } 
                set {m_Name = value; }
            }

            public int Lcid
            {
                get { return m_Lcid; } 
                set {m_Lcid = value; }
            }
        }
        
        static String [] s_Languages = 
        {
            "Arabic - Jordan",
            "Arabic - Kuwait",  
            "Arabic - Lebanon", 
            "Arabic - Libya", 
            "Arabic - Morocco", 
            "Arabic - Oman", 
            "Chinese - People's Republic of China",
            "Chinese - Singapore",
            "Chinese - Taiwan",
            "Croatian", 
            "Croatian (Bosnia/Herzegovina)", 
            "Dutch - Netherlands",
            "Dutch - Belgium",
            "English - United States",
            "English - United Kingdom",
            "English - Australia", 
            "English - Belize", 
            "English - Canada",
            "English - Caribbean", 
            "English - Hong Kong SAR",
            "English - India", 
            "English - Indonesia", 
            "English - Ireland", 
            "English - Jamaica", 
            "English - Malaysia",
            "English - New Zealand", 
            "English - Philippines", 
            "English - Singapore", 
            "English - South Africa",
            "English - Trinidad", 
            "English - Zimbabwe",
            "Estonian",
            "French - France",
            "French - Belgium", 
            "French - Cameroon",
            "French - Canada",
            "French - Democratic Rep. of Congo", 
            "French - Cote d'Ivoire",
            "French - Haiti", 
            "French - Luxembourg", 
            "French - Mali", 
            "French - Monaco",
            "French - Morocco", 
            "French - North Africa", 
            "French - Reunion", 
            "French - Senegal",
            "French - Switzerland",
            "French - West Indies",
            "German - Germany",
            "German - Austria",
            "German - Liechtenstein",
            "German - Luxembourg",
            "German - Switzerland",
            "Greek", 
            "Italian - Italy", 
            "Italian - Switzerland", 
            "Japanese", 
            "Norwegian (Bokmål)",
            "Norwegian (Nynorsk)", 
            "Polish", 
            "Portuguese - Brazil", 
            "Portuguese - Portugal",
            "Romanian",
            "Romanian - Moldava", 
            "Russian",
            "Russian - Moldava", 
            "Spanish - Spain (Modern Sort)",
            "Spanish - Spain (Traditional Sort)", 
            "Spanish - Argentina", 
            "Spanish - Bolivia", 
            "Spanish - Chile",
            "Spanish - Colombia", 
            "Spanish - Costa Rica",
            "Spanish - Dominican Republic",
            "Spanish - Ecuador",
            "Spanish - El Salvador",
            "Spanish - Guatemala",
            "Spanish - Honduras",
            "Spanish - Latin America",
            "Spanish - Mexico", 
            "Spanish - Nicaragua", 
            "Spanish - Panama",
            "Spanish - Paraguay",
            "Spanish - Peru",
            "Spanish - Puerto Rico", 
            "Spanish - United States",
            "Spanish - Uruguay",
            "Spanish - Venezuela",
        };

        static int [] s_Lcids = 
        {
            0x2c01, 
            0x3401, 
            0x3001, 
            0x1001,
            0x1801, 
            0x2001, 
            0x0804 ,
            0x1004 ,
            0x0404 ,
            0x041a ,
            0x101a ,
            0x0413 ,
            0x0813 ,
            0x0409 ,
            0x0809 ,
            0x0c09 ,
            0x2809 , 
            0x1009 ,
            0x2409 ,
            0x3c09 ,
            0x4009 ,
            0x3809 ,
            0x1809 , 
            0x2009 ,
            0x4409 ,
            0x1409 ,
            0x3409 ,
            0x4809 ,
            0x1c09 ,
            0x2c09 ,
            0x3009 ,
            0x0425 ,
            0x040c ,
            0x080c ,
            0x2c0c ,
            0x0c0c , 
            0x240c ,
            0x300c ,
            0x3c0c ,
            0x140c ,
            0x340c ,
            0x180c ,
            0x380c ,
            0xe40c ,
            0x200c ,
            0x280c ,
            0x100c ,
            0x1c0c ,
            0x0407 ,
            0x0c07 ,
            0x1407 ,
            0x1007 ,
            0x0807 ,
            0x0408 ,
            0x0410 ,
            0x0810 ,
            0x0411 ,
            0x0414 ,
            0x0814 ,
            0x0415 ,
            0x0416 ,
            0x0816 ,
            0x0418 ,
            0x0818 ,
            0x0419 ,
            0x0819 ,
            0x0c0a ,
            0x040a ,
            0x2c0a ,
            0x400a ,
            0x340a ,
            0x240a ,
            0x140a ,
            0x1c0a ,
            0x300a ,
            0x440a ,
            0x100a , 
            0x480a ,
            0x580a ,
            0x080a ,
            0x4c0a ,
            0x180a ,
            0x3c0a ,
            0x280a ,
            0x500a ,
            0x540a ,
            0x380a ,
            0x200a ,
        };

        public static void CreateTestIndex(bool sqlIndex, String folder, String name, int numProducts, int numFiles,
            int numEvents, int numEventInfos, int numCabs, String cabFile)
        {
            // The folder must exist.
            if (Directory.Exists(folder))
                throw new ArgumentException("Destination folder must not exist", "folder");

            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("You must specify an index name");

            if (!File.Exists(cabFile))
                throw new ArgumentException("Cab file must be specified and must exist");

            Directory.CreateDirectory(folder);
            
            // Create an index.

            IErrorIndex index;

            if (sqlIndex)
            {
                index = new SqlErrorIndex(StackHashSqlConfiguration.Default, name, folder);
            }
            else
            {
                index = new XmlErrorIndex(folder, name);
            }
           
            index.Activate();

            StackHashTestIndexData testData = new StackHashTestIndexData();

            testData.NumberOfProducts = numProducts;
            testData.NumberOfFiles = numFiles;
            testData.NumberOfEvents = numEvents;
            testData.NumberOfEventInfos = numEventInfos;
            testData.NumberOfCabs = numCabs;
            testData.CabFileName = cabFile;

            CreateTestIndex(index, testData); 
        }


        /// <summary>
        /// Create the index.
        /// </summary>
        /// <param name="index">Sql or xml index.</param>
        /// <param name="testData">Defines size and type of entries.</param>
        public static void CreateTestIndex(IErrorIndex index, StackHashTestIndexData testData)
        {
            int productId = 26214;
            int fileId = 1035620;
            int eventId = 1099309964;
            int cabId = 939529168;

            // Randomize some selections.
            Random rand = new Random(1);

            int totalEventsPerProduct = testData.NumberOfFiles * testData.NumberOfEvents;

            for (int i = 0; i < testData.NumberOfProducts; i++)
            {
                StackHashProduct product = new StackHashProduct();
                product.DateCreatedLocal = DateTime.Now.ToUniversalTime().AddDays(-1 * (rand.Next() % 180));
                product.DateModifiedLocal = product.DateCreatedLocal.AddDays(rand.Next(-10, 10));
                product.FilesLink = "http://www.cucku.com";
                product.Id = productId++;
                product.Name = "StackHash";
                product.TotalEvents = totalEventsPerProduct;
                product.TotalResponses = 1;
                product.Version = String.Format("{0}.{1}.{2}{3}.{4}", 
                    i.ToString(CultureInfo.InvariantCulture), rand.Next() % 99, rand.Next() % 366, rand.Next() % 5 + 2005, (i + 1) * 1237);
                index.AddProduct(product);

                Console.WriteLine(String.Format("Adding product {0} of {1}", i, testData.NumberOfProducts));

                for (int j = 0; j < testData.NumberOfFiles; j++)
                {
                    StackHashFile file = new StackHashFile();
                    file.DateCreatedLocal = DateTime.Now.ToUniversalTime().AddDays(-1 * (rand.Next() % 180));
                    file.DateModifiedLocal = file.DateCreatedLocal.AddDays(rand.Next(-10, 10));
                    file.Id = fileId++;
                    file.LinkDateLocal = file.DateCreatedLocal.AddDays(rand.Next(-10, 10));

                    int fileIndex = rand.Next() % s_FileNames.Length;
                    file.Name = String.Format("{0}.{1}", s_FileNames[fileIndex], "dll");
                    file.Version = String.Format("{0}.{1}.{2}{3}.{4}", 
                        fileId, rand.Next() % 99, rand.Next() % 366, rand.Next() % 5 + 2005, (i + 1) * 1237);

                    index.AddFile(product, file);
                    Console.WriteLine(String.Format("Adding File {0} of {1}", j, testData.NumberOfFiles));
                        
                    for (int k = 0; k < testData.NumberOfEvents; k++)
                    {
                        StackHashEvent theEvent = new StackHashEvent();
                        theEvent.DateCreatedLocal = DateTime.Now.ToUniversalTime().AddDays(-1 * (rand.Next() % 180));
                        theEvent.DateModifiedLocal = theEvent.DateCreatedLocal.AddDays(rand.Next(-10, 10));

                        // Select a random event type.
                        theEvent.EventTypeName = s_EventTypes[rand.Next(0, s_EventTypes.Length)];
                        theEvent.FileId = file.Id;
                        theEvent.Id = eventId++;

                        theEvent.EventSignature = new StackHashEventSignature();
                        theEvent.EventSignature.ApplicationName = "StackHash.exe";
                        theEvent.EventSignature.ApplicationTimeStamp = DateTime.Now.ToUniversalTime().AddDays(-180);
                        theEvent.EventSignature.ApplicationVersion = String.Format("{0}.{1}.{2}{3}.{4}",
                            rand.Next() % 99, rand.Next() % 99, rand.Next() % 366, rand.Next() % 5 + 2005, (eventId + 1) * 1234);
                        theEvent.EventSignature.ExceptionCode = 0xc0000000 + rand.Next(0, 16);
                        theEvent.EventSignature.ModuleName = s_ModuleNames[rand.Next() % s_ModuleNames.Length];
                        theEvent.EventSignature.ModuleTimeStamp = DateTime.Now.ToUniversalTime().AddDays(-1 * ((rand.Next() % 200)));
                        theEvent.EventSignature.ModuleVersion = String.Format("{0}.{1}.{2}{3}.{4}",
                            rand.Next() % 99, rand.Next() % 99, rand.Next() % 366, rand.Next() % 5 + 2005, (eventId + 1) * 1234);
                        theEvent.EventSignature.Offset = rand.Next(0, 0xfffff);
                        theEvent.EventSignature.Parameters = new StackHashParameterCollection();
                        theEvent.EventSignature.Parameters.Add(new StackHashParameter("applicationName", theEvent.EventSignature.ApplicationName));
                        theEvent.EventSignature.Parameters.Add(new StackHashParameter("applicationTimeStamp", theEvent.EventSignature.ApplicationTimeStamp.ToString()));
                        theEvent.EventSignature.Parameters.Add(new StackHashParameter("applicationVersion", theEvent.EventSignature.ApplicationVersion));
                        theEvent.EventSignature.Parameters.Add(new StackHashParameter("exceptionCode", String.Format("{0:X}",theEvent.EventSignature.ExceptionCode)));
                        theEvent.EventSignature.Parameters.Add(new StackHashParameter("moduleName", theEvent.EventSignature.ModuleName));
                        theEvent.EventSignature.Parameters.Add(new StackHashParameter("moduleTimeStamp", theEvent.EventSignature.ModuleTimeStamp.ToString()));
                        theEvent.EventSignature.Parameters.Add(new StackHashParameter("moduleVersion", theEvent.EventSignature.ModuleVersion.ToString()));
                        theEvent.EventSignature.Parameters.Add(new StackHashParameter("offset", String.Format("{0:X}", theEvent.EventSignature.Offset.ToString()))) ;

                        index.AddEvent(product, file, theEvent);
                        DateTime startTime = DateTime.Now;

                        Console.WriteLine(String.Format("P:{0} F:{1} Ev {2} of {3}", i, j, k, testData.NumberOfEvents));

                        StackHashEventInfoCollection eventInfoCollection = new StackHashEventInfoCollection();
                        for (int l = 0; l < testData.NumberOfEventInfos; l++)
                        {
                            int languageIndex = rand.Next() % s_Languages.Length;

                            StackHashEventInfo eventInfo = new StackHashEventInfo();
                            eventInfo.DateCreatedLocal = DateTime.Now.ToUniversalTime().AddDays(-1 * (rand.Next() % 180));
                            eventInfo.DateModifiedLocal = eventInfo.DateCreatedLocal.AddDays(l);
                            eventInfo.HitDateLocal = DateTime.Now.ToUniversalTime().AddDays(-1 * l);
                            eventInfo.Language = s_Languages[languageIndex];
                            eventInfo.Lcid = s_Lcids[languageIndex];
                            eventInfo.Locale = "locale";
                            int osIndex = rand.Next(0, s_OperatingSystems.Length);

                            eventInfo.OperatingSystemName = s_OperatingSystems[osIndex];
                            eventInfo.OperatingSystemVersion = s_OperatingSystemVersions[osIndex];
                            eventInfo.TotalHits = l + 1;
                            eventInfoCollection.Add(eventInfo);
                        }

                        index.AddEventInfoCollection(product, file, theEvent, eventInfoCollection);


                        for (int m = 0; m < testData.NumberOfCabs; m++)
                        {
                            StackHashCab cab = new StackHashCab();
                            cab.DateCreatedLocal = DateTime.Now.ToUniversalTime().AddDays(-1 * (rand.Next() % 180));
                            cab.DateModifiedLocal = cab.DateCreatedLocal.AddDays(m);
                            cab.EventId = theEvent.Id;
                            cab.EventTypeName = theEvent.EventTypeName;
                            cab.FileName = String.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}.cab", cab.EventId, cab.EventTypeName, cab.Id);
                            cab.Id = cabId++;
                            cab.SizeInBytes = 64123 + rand.Next(-4000, 4000);

                            index.AddCab(product, file, theEvent, cab, false);

                            // Copy in a test cab file.

                            String cabFolder = index.GetCabFolder(product, file, theEvent, cab);
                            if (!Directory.Exists(cabFolder))
                                Directory.CreateDirectory(cabFolder);
                            String cabFileName = index.GetCabFileName(product, file, theEvent, cab);

                            if (!File.Exists(cabFileName))
                            {
                                if (testData.CabFileName != null)
                                    File.Copy(Path.Combine(@"R:\stackhash\BusinessLogic\BusinessLogic\TestData\Cabs\", testData.CabFileName), cabFileName);
                                else if (testData.UseLargeCab)
                                    File.Copy(@"R:\stackhash\BusinessLogic\BusinessLogic\TestData\Cabs\1630796338-Crash32bit-0760025228.cab", cabFileName);
                                else
                                    File.Copy(@"R:\stackhash\BusinessLogic\BusinessLogic\TestData\Cabs\1641909485-Crash32bit-0773522646.cab", cabFileName);
                            }
                            // Make sure the file is not read only.
                            FileAttributes attributes = File.GetAttributes(cabFileName);
                            attributes &= ~FileAttributes.ReadOnly;
                            File.SetAttributes(cabFileName, attributes);
                        }
                        Console.WriteLine(String.Format("Added Event {0} of {1} - Duration: {2}", k, testData.NumberOfEvents, (DateTime.Now - startTime).TotalMilliseconds));
                    }
                }
            }
        }
    }
}
