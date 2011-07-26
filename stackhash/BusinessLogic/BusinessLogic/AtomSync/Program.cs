using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Security;

using WinQualAtomFeed;
using StackHashBusinessObjects;
using StackHashUtilities;

namespace AtomSync
{
    public class ProgramArguments
    {
        public ProgramArguments()
        {
            ProxySettings = new StackHashProxySettings();
            Iterations = 1;
            IPAddress = "winqual.microsoft.com";
        }

        public String UserName { get; set; }
        public String Password { get; set; }
        public StackHashProxySettings ProxySettings { get; set; }

        // Options.
        public int Iterations { get; set; }
        public bool NoCabs { get; set; }
        public bool LogXml { get; set; }
        public String IPAddress { get; set; }
    }

    class Program
    {

        static void Synopsis()
        {
            DiagnosticsHelper.LogMessage(DiagSeverity.Information, "***************************************************************************************");
            DiagnosticsHelper.LogMessage(DiagSeverity.Information, "ATOMSYNC Version 1.0 - Copyright (c) Cucku, Inc 2010");
            DiagnosticsHelper.LogMessage(DiagSeverity.Information, "***************************************************************************************");
            DiagnosticsHelper.LogMessage(DiagSeverity.Information, "NOT FOR REDISTRIBUTION. TO BE USED BY RECIPIENT ONLY AS AGREED BY CUCKU, INC.");
            DiagnosticsHelper.LogMessage(DiagSeverity.Information, " atomsync [options] username password ");
            DiagnosticsHelper.LogMessage(DiagSeverity.Information, " atomsync [options] username password proxyHost proxyPort");
            DiagnosticsHelper.LogMessage(DiagSeverity.Information, " atomsync [options] username password proxyHost proxyPort proxyUsername proxyPassword");
            DiagnosticsHelper.LogMessage(DiagSeverity.Information, " atomsync [options] username password proxyHost proxyPort proxyUsername proxyPassword proxyDomain");
            DiagnosticsHelper.LogMessage(DiagSeverity.Information, " where [options] are... ");
            DiagnosticsHelper.LogMessage(DiagSeverity.Information, "  -iN          - Number of iterations e.g. -i1000");
            DiagnosticsHelper.LogMessage(DiagSeverity.Information, "  -nocabs      - Don't download any cabs");
            DiagnosticsHelper.LogMessage(DiagSeverity.Information, "  -aIPADDRESS  - Use a specific IP address for winqual.microsoft.com e.g. -a131.107.97.31");
            DiagnosticsHelper.LogMessage(DiagSeverity.Information, "  -xml         - log XML");
            DiagnosticsHelper.LogMessage(DiagSeverity.Information, "***************************************************************************************");
        }

        public static StackHashCabCollection GetCabInfoAtom(AtomFeed feed, AtomEvent theEvent)
        {
            // Get the list of events.
            AtomCabCollection atomCabs = feed.GetCabs(theEvent);

            // Convert to a StackHashCabCollection.
            StackHashCabCollection atomStackHashCabss = new StackHashCabCollection();

            foreach (AtomCab atomCab in atomCabs)
            {
                atomStackHashCabss.Add(atomCab.Cab);
            }

            return atomStackHashCabss;
        }

        public static StackHashEventInfoCollection GetEventInfoAtom(AtomFeed feed, AtomEvent theEvent, int days)
        {
            // Get the list of events.
            AtomEventInfoCollection atomEventInfos = feed.GetEventDetails(theEvent, days);

            // Convert to a StackHashEventInfoCollection.
            StackHashEventInfoCollection atomStackHashEventInfos = new StackHashEventInfoCollection();

            foreach (AtomEventInfo atomEventInfo in atomEventInfos)
            {
                atomStackHashEventInfos.Add(atomEventInfo.EventInfo);
            }

            return atomStackHashEventInfos;
        }

        static void Main(string[] args)
        {
            // Define the certificate policy for the application.
            ServicePointManager.ServerCertificateValidationCallback =
                new RemoteCertificateValidationCallback(MyCertificateValidation.ValidateServerCertificate);

            ProgramArguments programArguments = processArgs(args);

            // Didn't help with the certificate errors.
            // ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;

            for (int i = 0; i < programArguments.Iterations; i++)
            {
                Go(programArguments);
            }
        }

        static ProgramArguments processArgs(string[] args)
        {
            ProgramArguments arguments = new ProgramArguments();
            List<String> nonOptionArguments = new List<string>();

            if (args.Length == 0)
            {
                DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Error - must supply all parameters");

                Synopsis();
                return null;
            }

            
            foreach (String argument in args)
            {
                if (argument.Length < 2)
                    throw new ArgumentException("Options must be of form -abcde with no spaces between the - and first letter", "args");

                if (argument.StartsWith("-"))
                {
                    if (String.Compare(argument, "-nocabs", StringComparison.OrdinalIgnoreCase) == 0)
                        arguments.NoCabs = true;
                    if (String.Compare(argument, "-xml", StringComparison.OrdinalIgnoreCase) == 0)
                        arguments.LogXml = true;

                    if (argument[1] == 'i' || argument[1] == 'I')
                        arguments.Iterations = Int32.Parse(argument.Substring(2));
                    if (argument[1] == 'a' || argument[1] == 'A')
                        arguments.IPAddress = argument.Substring(2);
                }
                else
                {
                    nonOptionArguments.Add(argument);
                }
            }

            if (nonOptionArguments.Count < 2)
            {
                DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Error - must supply all parameters");

                Synopsis();
                return null;
            }

            arguments.UserName = nonOptionArguments[0];
            arguments.Password = nonOptionArguments[1];

            if (nonOptionArguments.Count > 2)
            {
                arguments.ProxySettings.UseProxy = true;

                if (nonOptionArguments.Count == 4)
                {
                    arguments.ProxySettings.ProxyHost = args[2];
                    arguments.ProxySettings.ProxyPort = Int32.Parse(args[3]);
                }

                else if (nonOptionArguments.Count == 6)
                {
                    arguments.ProxySettings.UseProxyAuthentication = true;
                    arguments.ProxySettings.ProxyHost = args[2];
                    arguments.ProxySettings.ProxyPort = Int32.Parse(args[3]);
                    arguments.ProxySettings.ProxyUserName = args[4];
                    arguments.ProxySettings.ProxyPassword = args[5];
                }

                else if (nonOptionArguments.Count == 7)
                {
                    arguments.ProxySettings.UseProxyAuthentication = true;
                    arguments.ProxySettings.UseProxyAuthentication = true;
                    arguments.ProxySettings.ProxyHost = args[2];
                    arguments.ProxySettings.ProxyPort = Int32.Parse(args[3]);
                    arguments.ProxySettings.ProxyUserName = args[4];
                    arguments.ProxySettings.ProxyPassword = args[5];
                    arguments.ProxySettings.ProxyDomain = args[6];
                }
            }


            return arguments;
        }

        static void Go(ProgramArguments programArguments)
        {
            LogManager logger = new LogManager();
            logger.StartLogging();
            bool noCabs = false;

            try
            {
                StackHashUtilities.SystemInformation.DisableSleep();

                AtomFeed atomFeed = new AtomFeed(programArguments.ProxySettings, 5, 300000, programArguments.LogXml, true, programArguments.IPAddress, 11);

                if (!atomFeed.Login(programArguments.UserName, programArguments.Password))
                {
                    DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Failed to logon - check your username and password");
                    return;
                }
                else
                {
                    DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Logged on successfully.");
                }

                // ATOM GetProducts.
                AtomProductCollection atomProducts = atomFeed.GetProducts();

                foreach (AtomProduct atomProduct in atomProducts)
                {
                }

                foreach (AtomProduct atomProduct in atomProducts)
                {
                    DiagnosticsHelper.LogMessage(DiagSeverity.Information, atomProduct.Product.ToString());

                    // ATOM GetFiles.
                    AtomFileCollection atomFiles = atomFeed.GetFiles(atomProduct);
                    int totalEvents = 0;

                    foreach (AtomFile atomFile in atomFiles)
                    {
 //                       String eventsLink = @"https://winqual.microsoft.com/Services/wer/user/events.aspx?fileid=" + fileId.ToString();
 //                       AtomFile atomFile = new AtomFile(new StackHashFile(), eventsLink);

                        DiagnosticsHelper.LogMessage(DiagSeverity.Information, atomFile.File.ToString());

                        // ATOM GetEvents.
                        AtomEventCollection atomEvents = atomFeed.GetEvents(atomFile);

                        foreach (AtomEvent atomEvent in atomEvents)
                        {
                            totalEvents++;
                            DiagnosticsHelper.LogMessage(DiagSeverity.Information, atomEvent.Event.ToString());

                            // ATOM events.
                            StackHashEventInfoCollection eventInfos = GetEventInfoAtom(atomFeed, atomEvent, 90);

                            StackHashEventInfoCollection normalizedEventInfos = new StackHashEventInfoCollection();

                            bool stop = false;
                            foreach (StackHashEventInfo eventInfo in eventInfos)
                            {
                                DiagnosticsHelper.LogMessage(DiagSeverity.Information, eventInfo.ToString());
                                if (eventInfos.GetEventInfoMatchCount(eventInfo) != 1)
                                {
                                    DiagnosticsHelper.LogMessage(DiagSeverity.Information, "*** DUPLICATE HIT");
                                    DiagnosticsHelper.LogMessage(DiagSeverity.Information, eventInfo.ToString());
                                    stop = true;
                                }
                                StackHashEventInfo normalizedEventInfo = eventInfo.Normalize();
                                DiagnosticsHelper.LogMessage(DiagSeverity.Information, "N: " + normalizedEventInfo.ToString());
                                normalizedEventInfos.Add(normalizedEventInfo);
                                if (normalizedEventInfos.GetEventInfoMatchCount(normalizedEventInfo) != 1)
                                {
                                    DiagnosticsHelper.LogMessage(DiagSeverity.Information, "*** NORMALIZATION ERROR");
                                    DiagnosticsHelper.LogMessage(DiagSeverity.Information, normalizedEventInfo.ToString());
                                    stop = true;
                                }
                            }

                            if (stop)
                                return;


                            if (!noCabs)
                            {
                                // ATOM GetCabs.
                                StackHashCabCollection atomCabs = GetCabInfoAtom(atomFeed, atomEvent);

                                foreach (StackHashCab cab in atomCabs)
                                {
                                    DiagnosticsHelper.LogMessage(DiagSeverity.Information, cab.ToString());
                                }
                            }
                        }
                    }

                    DiagnosticsHelper.LogMessage(DiagSeverity.Information, String.Format("TOTAL EVENTS: {0}", totalEvents));
                }
            }
            catch (System.Exception ex)
            {
                DiagnosticsHelper.LogMessage(DiagSeverity.Information, "Error: " + ex.ToString());
            }
            finally
            {
                StackHashUtilities.SystemInformation.EnableSleep();
                logger.StopLogging();
            }
        }
    }
}
