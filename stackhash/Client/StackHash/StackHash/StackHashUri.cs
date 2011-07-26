using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace StackHash
{
    /// <summary>
    /// Represents a uri of the form stackhash://profile/product/eventid:eventtype/cab
    /// </summary>
    public class StackHashUri
    {
        //http://msdn.microsoft.com/en-us/library/aa767914(v=vs.85).aspx

        /// <summary>
        /// Gets the raw URI string 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
        public string RawUri { get; private set; }

        /// <summary>
        /// Gets the Context Id
        /// </summary>
        public int ContextId { get; private set; }

        /// <summary>
        /// Gets the Product Id 
        /// </summary>
        public int? ProductId { get; private set; }

        /// <summary>
        /// Gets the EventId 
        /// </summary>
        public int? EventId { get; private set; }

        /// <summary>
        /// Gets the Event Type
        /// </summary>
        public string EventType { get; private set; }

        /// <summary>
        /// Gets the Cab Id
        /// </summary>
        public int? CabId { get; private set; }

        /// <summary>
        /// Create a StackHashUri string
        /// </summary>
        /// <param name="contextId">Context Id</param>
        /// <param name="productId">Product Id</param>
        /// <param name="eventId">Event Id</param>
        /// <param name="eventType">Event Type Name</param>
        /// <param name="cabId">Cab Id</param>
        /// <returns>StackHashUri String</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
        public static string CreateUriString(int contextId, int productId, int? eventId, string eventType, int? cabId)
        {
            StringBuilder uriBuilder = new StringBuilder();
            uriBuilder.AppendFormat(CultureInfo.InvariantCulture, "stackhash://{0}/{1}", contextId, productId);
            if ((eventId != null) && (eventType != null))
            {
                uriBuilder.AppendFormat(CultureInfo.InvariantCulture, "/{0}:{1}", eventId, eventType.Replace(" ", "%20"));
                if (cabId != null)
                {
                    uriBuilder.AppendFormat(CultureInfo.InvariantCulture, "/{0}", cabId);
                }
            }
            return uriBuilder.ToString();
        }

        /// <summary>
        /// Create a StackHashUri string
        /// </summary>
        /// <param name="contextId">Context Id</param>
        /// <param name="productId">Product Id</param>
        /// <param name="eventId">Event Id</param>
        /// <param name="eventType">Event Type Name</param>
        /// <returns>StackHashUri String</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
        public static string CreateUriString(int contextId, int productId, int? eventId, string eventType)
        {
            return CreateUriString(contextId, productId, eventId, eventType, null);
        }

        /// <summary>
        /// Create a StackHashUri string
        /// </summary>
        /// <param name="contextId">Context Id</param>
        /// <param name="productId">Product Id</param>
        /// <returns>StackHashUri String</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
        public static string CreateUriString(int contextId, int productId)
        {
            return CreateUriString(contextId, productId, null, null, null);
        }

        /// <summary>
        /// Trys to parse a StackHashUri from a uri string
        /// </summary>
        /// <param name="uriString">The Uri string</param>
        /// <param name="uri">Returns the StackHashUri</param>
        /// <returns>True if a Uri was parsed</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static bool TryParse(string uriString, out StackHashUri uri)
        {
            List<string> dummyList = new List<string>();
            dummyList.Add("dummy");
            dummyList.Add(uriString);

            return TryParse(dummyList, out uri);
        }

        /// <summary>
        /// Trys to parse a StackHashUri from command line arguments
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <param name="uri">Returns the StackHashUri</param>
        /// <returns>True if a Uri was parsed</returns>
        public static bool TryParse(IList<string> args, out StackHashUri uri)
        {
            uri = null;
            bool ret = false;

            StackHashUri candidateUri = new StackHashUri();

            if ((args != null) && args.Count > 1)
            {
                if (args.Count == 2)
                {
                    candidateUri.RawUri = args[1];
                }
                else
                {
                    // spaces may have broken the URI into separate command line arguments
                    StringBuilder sb = new StringBuilder();
                    for (int arg = 1; arg < args.Count; arg++)
                    {
                        sb.Append(args[arg]);
                        sb.Append(" ");
                    }
                    candidateUri.RawUri = sb.ToString().Trim();
                }

                // chrome (at least) doesn't seem to pass the protocol part so add it if needed
                if (candidateUri.RawUri.IndexOf("//", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    candidateUri.RawUri = "stackhash:" + candidateUri.RawUri;
                }

                // process any escaped space characters
                candidateUri.RawUri = candidateUri.RawUri.Replace("%20", " ");

                // check for stackHash://
                if (candidateUri.RawUri.IndexOf("stackhash://", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    string[] uribits = candidateUri.RawUri.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    if (uribits.Length >= 2)
                    {
                        int contextId;
                        if (Int32.TryParse(uribits[1], out contextId))
                        {
                            // we at least have a context Id to load
                            candidateUri.ContextId = contextId;
                            ret = true;

                            // try to parse further...
                            if (uribits.Length >= 3)
                            {
                                // Product Id
                                int productId;
                                if (Int32.TryParse(uribits[2], out productId))
                                {
                                    candidateUri.ProductId = productId;

                                    if (uribits.Length >= 4)
                                    {
                                        // Event Id
                                        int eventId;

                                        string[] eventbits = uribits[3].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                        if ((eventbits.Length == 2) && (Int32.TryParse(eventbits[0], out eventId)))
                                        {
                                            candidateUri.EventId = eventId;
                                            candidateUri.EventType = eventbits[1];

                                            if (uribits.Length >= 5)
                                            {
                                                // Cab Id
                                                int cabId;
                                                if (Int32.TryParse(uribits[4], out cabId))
                                                {
                                                    candidateUri.CabId = cabId;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (ret)
            {
                uri = candidateUri;
            }

            return ret;
        }

        private StackHashUri()
        {
            this.RawUri = null;
            this.ContextId = UserSettings.InvalidContextId;
            this.ProductId = null;
            this.EventId = null;
            this.EventType = null;
            this.CabId = null;
        }
    }
}
