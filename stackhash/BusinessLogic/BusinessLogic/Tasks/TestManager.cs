using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

using StackHashErrorIndex;
using StackHashBusinessObjects;
using StackHashCabs;
using StackHashUtilities;

namespace StackHashTasks
{
    public sealed class TestManager
    {
        static bool s_IsTestMode = true;

        static String[] s_FileNames = new String[] 
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

        static String[] s_ModuleNames = new String[] 
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


        public static bool IsTestMode
        {
            get { return s_IsTestMode; }
            set { s_IsTestMode = value; }
        }

        struct Language
        {
            String m_Name;
            int m_Lcid;
            String m_LocaleCode;

            public Language(String name, String localeCode, int localeId)
            {
                m_Name = name;
                m_LocaleCode = localeCode;
                m_Lcid = localeId;
            }

            public String Name
            {
                get { return m_Name; }
                set { m_Name = value; }
            }

            public int Lcid
            {
                get { return m_Lcid; }
                set { m_Lcid = value; }
            }
            public String LocaleCode
            {
                get { return m_LocaleCode; }
                set { m_LocaleCode = value; }
            }

        }

        static Language[] s_Languages = 
        {
            new Language("Afrikaans",	"af", 	0x0436),
            new Language("Albanian", "sq",	0x041C),
            new Language("Arabic - United Arab Emirates", "ar-ae",	0x3801),
            new Language("Arabic - Bahrain", "ar-bh",	0x3C01),
            new Language("Arabic - Algeria", "ar-dz",	0x1401),
            new Language("Arabic - Egypt", "ar-eg",	0x0C01),
            new Language("Arabic - Iraq", "ar-iq",	0x0801),
            new Language("Arabic - Jordan", "ar-jo",	0x2C01),
            new Language("Arabic - Kuwait", "ar-kw",	0x3401),
            new Language("Arabic - Lebanon", "ar-lb",	0x3001),
            new Language("Arabic - Libya", "ar-ly",	0x1001),
            new Language("Arabic - Morocco", "ar-ma",	0x1801),
            new Language("Arabic - Oman", "ar-om",	0x2001),
            new Language("Arabic - Qatar", "ar-qa",	0x4001),
            new Language("Arabic - Saudi Arabia", "ar-sa", 0x0401),
            new Language("Arabic - Syria", "ar-sy",	0x2801),
            new Language("Arabic - Tunisia", "ar-tn",	0x1C01),
            new Language("Arabic - Yemen", "ar-ye",	0x2401),
            new Language("Armenian", "hy",	0x042B),
            new Language("Azeri - Latin", "az-az",	0x042C),
            new Language("Azeri - Cyrillic", "az-az",	0x082C),
            new Language("Basque (Basque)", "eu",	0x042D),
            new Language("Belarusian", "be",	0x0423),
            new Language("Bulgarian", "bg",	0x0402),
            new Language("Catalan", "ca",	0x0403),
            new Language("Chinese - China", "zh-cn",	0x0804),
            new Language("Chinese - Hong Kong SAR", "zh-hk",	0x0C04),
            new Language("Chinese - Macau SAR", "zh-mo",	0x1404),
            new Language("Chinese - Singapore", "zh-sg",	0x1004),
            new Language("Chinese - Taiwan", "zh-tw",	0x0404),
            new Language("Croatian", "hr",	0x041A),
            new Language("Czech", "cs",	0x0405),
            new Language("Danish", "da",	0x0406),
            new Language("Dutch - Netherlands", "nl-nl",	0x0413),
            new Language("Dutch - Belgium", "nl-be",	0x0813),
            new Language("English - Australia", "en-au",	0x0C09),
            new Language("English - Belize", "en-bz",	0x2809),
            new Language("English - Canada", "en-ca",	0x1009),
            new Language("English - Caribbean", "en-cb",	0x2409),
            new Language("English - India", "en-in",	0x4009),
            new Language("English - Ireland", "en-ie",	0x1809),
            new Language("English - Jamaica", "en-jm",	0x2009),
            new Language("English - Malaysia", "en-my",	0x4409),
            new Language("English - New Zealand", "en-nz",	0x1409),
            new Language("English - Phillippines", "en-ph",	0x3409),
            new Language("English - Singapore", "en-sg",	0x4809),
            new Language("English - Southern Africa", "en-za",	0x1C09),
            new Language("English - Trinidad", "en-tt",	0x2C09),
            new Language("English - Great Britain", "en-gb",	0x0809),
            new Language("English - United States", "en-us",	0x0409),
            new Language("English - Zimbabwe", "en-zw",	0x3009),
            new Language("Estonian	", "et",	0x0425),
            new Language("Farsi", "fa",	0x0429),
            new Language("Finnish", "fi",	0x040B),
            new Language("Faroese", "fo",0x0438),
            new Language("French - France", "fr-fr",	0x040C),
            new Language("French - Belgium", "fr-be",	0x080C),
            new Language("French - Canada", "fr-ca",	0x0C0C),
            new Language("French - Luxembourg", "fr-lu",	0x140C),
            new Language("French - Switzerland", "fr-ch",	0x100C),
            new Language("Gaelic - Ireland", "gd-ie",	0x083C),
            new Language("Gaelic - Scotland", "gd",	0x043C),
            new Language("German - Germany", "de-de",	0x0407),
            new Language("German - Austria", "de-at",	0x0C07),
            new Language("German - Liechtenstein", "de-li",	0x1407),
            new Language("German - Luxembourg", "de-lu",	0x1007),
            new Language("German - Switzerland", "de-ch",	0x0807),
            new Language("Greek", "el",	0x0408),
            new Language("Hebrew", "he",	0x040D),
            new Language("Hindi", "hi",	0x0439),
            new Language("Hungarian", "hu",	0x040E),
            new Language("Icelandic", "is",	0x040F),
            new Language("Indonesian", "id",	0x0421),
            new Language("Italian - Italy", "it-it",	0x0410),
            new Language("Italian - Switzerland", "it-ch",	0x0810),
            new Language("Japanese", "ja",	0x0411),
            new Language("Korean", "ko",	0x0412),
            new Language("Latvian", "lv",	0x0426),
            new Language("Lithuanian", "lt",	0x0427),
            new Language("F.Y.R.O. Macedonia", "mk",	0x042F),
            new Language("Malay - Malaysia", "ms-my",	0x043E),
            new Language("Malay – Brunei", "ms-bn",	0x083E),
            new Language("Maltese", "mt",	0x043A),
            new Language("Marathi", "mr",	0x044E),
            new Language("Norwegian - Bokmål", "nb-no",	0x0414),
            new Language("Norwegian - Nynorsk", "nn-no",	0x0814),
            new Language("Polish", "pl",	0x0415),
            new Language("Portuguese - Portugal", "pt-pt",	0x0816),
            new Language("Portuguese - Brazil", "pt-br",	0x0416),
            new Language("Raeto-Romance", "rm",	0x0417),
            new Language("Romanian - Romania", "ro",	0x0418),
            new Language("Romanian - Republic of Moldova", "ro-mo",	0x0818),
            new Language("Russian", "ru",	0x0419),
            new Language("Russian - Republic of Moldova", "ru-mo",	0x0819),
            new Language("Sanskrit", "sa",	0x044F),
            new Language("Serbian - Cyrillic", "sr-sp",	0x0C1A),
            new Language("Serbian - Latin", "sr-sp",	0x081A),
            new Language("Setsuana", "tn",	0x0432),
            new Language("Slovenian", "sl",	0x0424),
            new Language("Slovak", "sk",	0x041B),
            new Language("Sorbian", "sb", 0x042E),
            new Language("Spanish – Spain (Modern)", "es-es",	0x0C0A),
            new Language("Spanish - Spain (Traditional)", "	",	0x040A),
            new Language("Spanish - Argentina", "es-ar",	0x2C0A),
            new Language("Spanish - Bolivia", "es-bo",	0x400A),
            new Language("Spanish - Chile", "es-cl",	0x340A),
            new Language("Spanish - Colombia", "es-co",	0x240A),
            new Language("Spanish - Costa Rica", "es-cr",	0x140A),
            new Language("Spanish - Dominican Republic", "es-do",	0x1C0A),
            new Language("Spanish - Ecuador", "es-ec",	0x300A),
            new Language("Spanish - Guatemala", "es-gt",	0x100A),
            new Language("Spanish - Honduras", "es-hn",	0x480A),
            new Language("Spanish - Mexico", "es-mx",	0x080A),
            new Language("Spanish - Nicaragua", "es-ni",	0x4C0A),
            new Language("Spanish - Panama", "es-pa",	0x180A),
            new Language("Spanish - Peru", "es-pe",	0x280A),
            new Language("Spanish - Puerto Rico", "es-pr",	0x500A),
            new Language("Spanish - Paraguay", "es-py",	0x3C0A),
            new Language("Spanish - El Salvador", "es-sv",	0x440A),
            new Language("Spanish - Uruguay", "es-uy",	0x380A),
            new Language("Spanish - Venezuela", "es-ve",	0x200A),
            new Language("Southern Sotho", "st",	0x0430),
            new Language("Swahili", "sw",	0x0441),
            new Language("Swedish - Sweden", "sv-se",	0x041D),
            new Language("Swedish - Finland", "sv-fi",	0x081D),
            new Language("Tamil", "ta",	0x0449),
            new Language("Tatar", "tt",	0X0444),
            new Language("Thai", "th",	0x041E),
            new Language("Turkish", "tr",	0x041F),
            new Language("Tsonga", "ts",	0x0431),
            new Language("Ukrainian", "uk",	0x0422),
            new Language("Urdu", "ur",	0x0420),
            new Language("Uzbek - Cyrillic", "uz-uz",	0x0843),
            new Language("Uzbek – Latin", "uz-uz",	0x0443),
            new Language("Vietnamese", "vi",	0x042A),
            new Language("Xhosa", "xh",	0x0434),
            new Language("Yiddish", "yi",	0x043D),
            new Language("Zulu", "zu",	0x0435)
        };

        private TestManager() { ; } // Private constructor because this is a static class.

        public static void CreateTestIndex(IErrorIndex index, StackHashTestIndexData testData)
        {
            CreateTestIndex(index, testData, false);
        }

        public static void CreateScriptFile(int scriptNumber, int cabId, String scriptFolder, int randomNumber, IFormatProvider culture, int scriptFormatVersion, int scriptResultSize)
        {
            StringBuilder resultData = new StringBuilder();
            String scriptName = "Script" + scriptNumber + "_CAB" + cabId;
            String scriptFileName = scriptName + ".log";
            String fullPathAndFileName = Path.Combine(scriptFolder, scriptFileName);

            DateTime lastModifiedTime = DateTime.Now.ToUniversalTime().AddDays(-1);
            DateTime runTime = DateTime.Now.ToUniversalTime();
            resultData.AppendLine(@"Opened log file 'K:\StackHashScratch\Dummy2\Dummy2\00\00\00\00\CAB_0000000085\Analysis\" + scriptFileName);

            if (scriptFormatVersion == 1)
            {
                resultData.AppendLine(@"Script---" + scriptName + @"---LastModified---" + lastModifiedTime.ToString(culture) + "---RunTime---" +
                    runTime.ToString(culture) + "---Version---2");
            }
            else
            {
                resultData.AppendLine(@"Script2---" + scriptName + @"---LastModified---" + lastModifiedTime.ToString(culture) + "---RunTime---" +
                    runTime.ToString(culture) + "---Version---2");            
            }
            resultData.AppendLine(@"Command---dummycommand---Perform some debugger option");
            resultData.AppendLine(@"This is the output from the command - line 1" + scriptFileName);
            resultData.AppendLine(@"This is the output from the command - line_2" + randomNumber);
            resultData.AppendLine(@"CommandEnd---dummycommand---Perform some debugger option");

            File.WriteAllText(fullPathAndFileName, resultData.ToString(), Encoding.Unicode);

            // If the result file size is specified then make sure the file is approximately the specified size.
            // This is used for unit testing large results file retrieval by a client.
            if (scriptResultSize > 0)
            {
                File.AppendAllText(fullPathAndFileName, "Command---dummycommand---second command\n", Encoding.Unicode);

                String outString = "This is a dummy line of output for a dummy command when the length of the result file is specified.\n";
                for (int lineCount = 0; lineCount < scriptResultSize / outString.Length; lineCount++)
                {
                    File.AppendAllText(fullPathAndFileName, outString, Encoding.Unicode);
                }
                File.AppendAllText(fullPathAndFileName, "CommandEnd---dummycommand---second command\n", Encoding.Unicode);
            }

            resultData = new StringBuilder();
            resultData.AppendLine(@"Script Complete:");
            resultData.AppendLine(@"Closing open log file K:\StackHashScratch\Dummy2\Dummy2\00\00\00\00\CAB_0000000085\Analysis\" + scriptFileName);

            File.AppendAllText(fullPathAndFileName, resultData.ToString(), Encoding.Unicode);
        }

        [SuppressMessage("Microsoft.Design", "CA1062")]
        public static void CreateTestScripts(IErrorIndex index, StackHashProduct product, StackHashFile file, StackHashEvent theEvent, StackHashCab cab, int numberOfScripts, int scriptFileSize)
        {
            String analysisFolder = index.GetCabFolder(product, file, theEvent, cab) + "\\analysis";


            if (!Directory.Exists(analysisFolder))
                Directory.CreateDirectory(analysisFolder);

            for (int scriptCount = 0; scriptCount < numberOfScripts; scriptCount++)
            {
                CreateScriptFile(scriptCount, cab.Id, analysisFolder, (int)DateTime.Now.Ticks, CultureInfo.InvariantCulture, 2, scriptFileSize);
            }
        }


        [SuppressMessage("Microsoft.Maintainability", "CA1502")]
        [SuppressMessage("Microsoft.Maintainability", "CA1505")]
        [SuppressMessage("Microsoft.Design", "CA1062")]
        public static void CreateTestIndex(IErrorIndex index, StackHashTestIndexData testData, bool includeDuplicates)
        {
            if (testData.EventsToAssignCabs == 0)
                testData.EventsToAssignCabs = 1;

            Random rand = new Random(1);
            Random scriptRand = new Random(1);

            if (index == null)
                throw new ArgumentNullException("index");
            if (testData == null)
                throw new ArgumentNullException("testData");

            DateTime now = DateTime.Now;


            int fileId = 1;
            int eventId = 1;
            int cabId = 1;
            int productId = 1;
            int offset = 10000;

            if (!s_IsTestMode)
            {
                productId = 26214;
                fileId = 1035620;
                eventId = 1099309964;
                cabId = 939529168;
            }

            int initialFileId = fileId;
            int initialEventId = eventId;
            int initialCabId = cabId;
            int initialOffset = offset;

            int totalEventsPerProduct = testData.NumberOfFiles * testData.NumberOfEvents;
            index.SetLastSyncTimeLocal(-2, DateTime.Now.AddDays(-14));

            for (int i = 0; i < testData.NumberOfProducts; i++)
            {
                StackHashProduct product = new StackHashProduct();
                product.DateCreatedLocal = now.ToUniversalTime().AddDays(-1 * (rand.Next() % 180)).RoundToPreviousMinute();
                product.DateModifiedLocal = product.DateCreatedLocal.AddDays(rand.Next(-10, 10)).RoundToPreviousMinute();
                product.FilesLink = "http://www.cucku.com";
                product.Id = productId;
                product.Name = "StackHash" + productId.ToString(CultureInfo.InvariantCulture);
                product.TotalEvents = totalEventsPerProduct;
                product.TotalResponses = 1;
                product.Version = "1.2.3." + productId.ToString(CultureInfo.InvariantCulture);

                index.AddProduct(product);
                productId++;

                if (includeDuplicates)
                {
                    fileId = initialFileId;
                    eventId = initialEventId;
                    cabId = initialCabId;
                    offset = initialOffset;
                    rand = new Random(1);
                }

                for (int j = 0; j < testData.NumberOfFiles; j++)
                {
                    StackHashFile file = new StackHashFile();

                    file.DateCreatedLocal = now.ToUniversalTime().AddDays(-1 * (rand.Next() % 180)).RoundToPreviousMinute();
                    file.DateModifiedLocal = file.DateCreatedLocal.AddDays(rand.Next(-10, 10)).RoundToPreviousMinute();
                    file.Id = fileId++;
                    file.LinkDateLocal = file.DateCreatedLocal.AddDays(rand.Next(-10, 10)).RoundToPreviousMinute();

                    int fileIndex = rand.Next() % s_FileNames.Length;

                    file.Name = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", s_FileNames[fileIndex], "dll");
                    file.Version = String.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}{3}.{4}",
                        fileId, rand.Next() % 99, rand.Next() % 366, rand.Next() % 5 + 2005, (j + 1) * 1237);

                    index.AddFile(product, file);

                    for (int k = 0; k < testData.NumberOfEvents; k++)
                    {
                        int totalHits = 0;

                        Random hitsRand = new Random(k);
                        for (int l = 0; l < testData.NumberOfEventInfos; l++)
                        {
                            if (s_IsTestMode)
                            {
                                totalHits += (l + k);
                            }
                            else
                            {
                                totalHits += hitsRand.Next(0, 50);
                            }
                        }

                        StackHashEvent theEvent = new StackHashEvent();

                        theEvent.DateCreatedLocal = now.ToUniversalTime().AddDays(-1 * (rand.Next() % 180)).RoundToPreviousMinute();
                        theEvent.DateModifiedLocal = theEvent.DateCreatedLocal.AddDays(rand.Next(-10, 10)).RoundToPreviousMinute();

                        theEvent.EventTypeName = s_EventTypes[rand.Next(0, s_EventTypes.Length)];
                        theEvent.FileId = file.Id;
                        theEvent.Id = eventId;
                        theEvent.BugId = "Bug" + eventId.ToString(CultureInfo.InvariantCulture);
                        theEvent.TotalHits = totalHits;

                        theEvent.EventSignature = new StackHashEventSignature();
                        theEvent.EventSignature.ApplicationName = "StackHash.exe";
                        theEvent.EventSignature.ApplicationTimeStamp = now.ToUniversalTime().AddDays(rand.Next(-180, 0));
                        theEvent.EventSignature.ApplicationVersion = String.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}{3}.{4}",
                            eventId, rand.Next() % 99, rand.Next() % 366, rand.Next() % 5 + 2005, (Math.Abs(eventId) + 1) * 1234);
                        theEvent.EventSignature.ExceptionCode = 0xc0000000 + rand.Next(0, 16);

                        theEvent.EventSignature.ModuleName = "Module" + k.ToString(CultureInfo.InvariantCulture);
                        theEvent.EventSignature.ModuleTimeStamp = now.ToUniversalTime().AddDays(-1 * ((rand.Next() % 200)));
                        theEvent.EventSignature.ModuleVersion = String.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}{3}.{4}",
                            eventId, rand.Next() % 99, rand.Next() % 366, rand.Next() % 5 + 2005, (Math.Abs(eventId) + 1) * 1234);

                        if (s_IsTestMode)
                            theEvent.EventSignature.Offset = offset--;   // Make these go backwards.
                        else
                            theEvent.EventSignature.Offset = rand.Next(0, 0xfffff);

                        theEvent.EventSignature.Parameters = new StackHashParameterCollection();
                        theEvent.EventSignature.Parameters.Add(new StackHashParameter("applicationName", theEvent.EventSignature.ApplicationName));
                        theEvent.EventSignature.Parameters.Add(new StackHashParameter("applicationTimeStamp", theEvent.EventSignature.ApplicationTimeStamp.ToString(CultureInfo.InvariantCulture)));
                        theEvent.EventSignature.Parameters.Add(new StackHashParameter("applicationVersion", theEvent.EventSignature.ApplicationVersion));
                        theEvent.EventSignature.Parameters.Add(new StackHashParameter("exceptionCode", String.Format(CultureInfo.InvariantCulture, "{0:X}", theEvent.EventSignature.ExceptionCode)));
                        theEvent.EventSignature.Parameters.Add(new StackHashParameter("moduleName", theEvent.EventSignature.ModuleName));
                        theEvent.EventSignature.Parameters.Add(new StackHashParameter("moduleTimeStamp", theEvent.EventSignature.ModuleTimeStamp.ToString(CultureInfo.InvariantCulture)));
                        theEvent.EventSignature.Parameters.Add(new StackHashParameter("moduleVersion", theEvent.EventSignature.ModuleVersion.ToString()));
                        theEvent.EventSignature.Parameters.Add(new StackHashParameter("offset", String.Format(CultureInfo.InvariantCulture, "{0:X}", theEvent.EventSignature.Offset)));
                        theEvent.EventSignature.InterpretParameters();
                        index.AddEvent(product, file, theEvent);

                        // Allow for some duplicate event ids.
                        if (!s_IsTestMode)
                        {
                            eventId++;
                            //if (rand.Next(0, 100) > 5)
                            //    eventId++;

                            //if (rand.Next(0, 100) > 50)
                            //    eventId = -1 * eventId;
                        }
                        else
                        {
                            eventId++;
                        }

                        hitsRand = new Random(k);
                        StackHashEventInfoCollection eventInfoCollection = new StackHashEventInfoCollection();
                        for (int l = 0; l < testData.NumberOfEventInfos; l++)
                        {
                            int languageIndex = rand.Next() % s_Languages.Length;

                            if (s_IsTestMode)
                                languageIndex = l % s_Languages.Length;

                            StackHashEventInfo eventInfo = new StackHashEventInfo();
                            eventInfo.DateCreatedLocal = now.ToUniversalTime().AddDays(-1 * (rand.Next() % 180)).RoundToPreviousMinute();
                            eventInfo.DateModifiedLocal = eventInfo.DateCreatedLocal.AddDays(l).RoundToPreviousMinute();

                            if (s_IsTestMode)
                                eventInfo.HitDateLocal = now.ToUniversalTime().AddDays(-1 * l).RoundToPreviousMinute();
                            else
                                eventInfo.HitDateLocal = now.ToUniversalTime().AddDays(rand.Next(-180, 0)).RoundToPreviousMinute();

                            eventInfo.Language = s_Languages[languageIndex].Name;
                            eventInfo.Lcid = s_Languages[languageIndex].Lcid;
                            eventInfo.Locale = s_Languages[languageIndex].LocaleCode;


                            int osIndex = rand.Next(0, s_OperatingSystems.Length);
                            if (s_IsTestMode)
                                osIndex = l % s_OperatingSystems.Length;
                            eventInfo.OperatingSystemName = s_OperatingSystems[osIndex];

                            if (s_IsTestMode)
                                eventInfo.OperatingSystemVersion = s_OperatingSystemVersions[osIndex] + l.ToString(CultureInfo.InvariantCulture);
                            else
                                eventInfo.OperatingSystemVersion = s_OperatingSystemVersions[osIndex];

                            if (s_IsTestMode)
                                eventInfo.TotalHits = l + k;
                            else
                                eventInfo.TotalHits = hitsRand.Next(0, 50);

                            if (eventInfoCollection.FindEventInfo(eventInfo) == null)
                                eventInfoCollection.Add(eventInfo);
                        }

                        index.MergeEventInfoCollection(product, file, theEvent, eventInfoCollection);

                        for (int m = 0; m < testData.NumberOfCabs; m++)
                        {
                            if ((k % testData.EventsToAssignCabs) != 0)
                                break;

                            StackHashCab cab = new StackHashCab();

                            if (IsTestMode)
                            {
                                cab.DateCreatedLocal = now.ToUniversalTime().AddDays(-1 * m).RoundToPreviousMinute();
                                cab.DateModifiedLocal = cab.DateCreatedLocal.AddDays(m).RoundToPreviousMinute();
                            }
                            else
                            {
                                cab.DateCreatedLocal = now.ToUniversalTime().AddDays(-1 * (rand.Next() % 180)).RoundToPreviousMinute();
                                cab.DateModifiedLocal = cab.DateCreatedLocal.AddDays(m).RoundToPreviousMinute();
                            }
                            cab.EventId = theEvent.Id;
                            cab.EventTypeName = theEvent.EventTypeName;
                            cab.FileName = String.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}.cab", cab.EventId, cab.EventTypeName, cab.Id);
                            cab.Id = cabId++;
                            cab.SizeInBytes = 64123 + rand.Next(-4000, 4000); // Some random value - corrected later if a real file exists.
                            cab.CabDownloaded = true;
                            
                            // Get the size of the cab file.
                            String sourceCabFile;
                            if (testData.CabFileName != null)
                                sourceCabFile = Path.Combine(TestSettings.TestDataFolder + @"Cabs\", testData.CabFileName);
                            else if (testData.UseLargeCab)
                                sourceCabFile = TestSettings.TestDataFolder + @"Cabs\1630796338-Crash32bit-0760025228.cab";
                            else
                                sourceCabFile = TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab";

                            if (File.Exists(sourceCabFile))
                            {
                                FileInfo sourceCabFileInfo = new FileInfo(sourceCabFile);
                                cab.SizeInBytes = sourceCabFileInfo.Length;
                            }

                            index.AddCab(product, file, theEvent, cab, false);


                            // Copy in a test cab file.

                            String cabFolder = index.GetCabFolder(product, file, theEvent, cab);
                            if (!Directory.Exists(cabFolder))
                                Directory.CreateDirectory(cabFolder);
                            String cabFileName = index.GetCabFileName(product, file, theEvent, cab);

                            if (!File.Exists(cabFileName))
                            {
                                if (testData.CabFileName != null)
                                    File.Copy(Path.Combine(TestSettings.TestDataFolder + @"Cabs\", testData.CabFileName), cabFileName);
                                else if (testData.UseLargeCab)
                                    File.Copy(TestSettings.TestDataFolder + @"Cabs\1630796338-Crash32bit-0760025228.cab", cabFileName);
                                else
                                    File.Copy(TestSettings.TestDataFolder + @"Cabs\1641909485-Crash32bit-0773522646.cab", cabFileName);
                            }


                            // Make sure the file is not read only.
                            FileAttributes attributes = File.GetAttributes(cabFileName);
                            attributes &= ~FileAttributes.ReadOnly;
                            File.SetAttributes(cabFileName, attributes);


                            // Unwrap the cab.
                            if (testData.UnwrapCabs)
                            {
                                Cabs.ExtractCab(cabFileName, cabFolder);
                            }

                            if (testData.NumberOfScriptResults > 0)
                            {
                                String analysisFolder = index.GetCabFolder(product, file, theEvent, cab) + "\\analysis";
                                
                                if (!Directory.Exists(analysisFolder))
                                    Directory.CreateDirectory(analysisFolder);

                                for (int scriptCount = 0; scriptCount < testData.NumberOfScriptResults; scriptCount++)
                                {
                                    CreateScriptFile(scriptCount, cab.Id, analysisFolder, scriptRand.Next(5), CultureInfo.InvariantCulture, 2, testData.ScriptFileSize);
                                }
                            }


                            StackHashNotes cabNotes = index.GetCabNotes(product, file, theEvent, cab);
                            for (int q = 0; q < testData.NumberOfCabNotes; q++)
                            {
                                StackHashNoteEntry note = new StackHashNoteEntry(now.ToUniversalTime().RoundToPreviousSecond(), "User", "MarkJ", "This is a cab note" + q.ToString(CultureInfo.InvariantCulture));

                                // Don't add duplicate cab notes in the SQL index. The XML index may contain duplicates.
                                if (index.IndexType == ErrorIndexType.Xml || !cabNotes.ContainsNote(note))
                                    index.AddCabNote(product, file, theEvent, cab, note);
                            }

                        }

                        StackHashNotes eventNotes = index.GetEventNotes(product, file, theEvent);
                        for (int p = 0; p < testData.NumberOfEventNotes; p++)
                        {
                            StackHashNoteEntry note = new StackHashNoteEntry(now.ToUniversalTime().RoundToPreviousSecond(), "User", "MarkJ", "This is an event note" + p.ToString(CultureInfo.InvariantCulture));
                            
                            // Don't add duplicate event notes in the SQL index. The XML index may contain duplicates.
                            if (index.IndexType == ErrorIndexType.Xml || !eventNotes.ContainsNote(note))
                                index.AddEventNote(product, file, theEvent, note);
                        }
                    }
                }
                index.UpdateProductStatistics(product);
            }

        }
    }
}
