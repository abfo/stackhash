//---------------------------------------------------------------------
// <summary>
//      Class for holding the constants for the exception list full names.
// </summary>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;


namespace StackHash.WindowsErrorReporting.Services.Data.API
{
    /// <summary>
    /// Class for holding the constants for the exception list full names.
    /// </summary>
    class SpecialExceptionList
    {
        /// <summary>
        ///  Special Exception Name List 
        /// </summary>
        public static List<string> SpecialInnerExceptionNameList = GetSpecialInnerExceptionNameList();

        /// <summary>
        /// FeedAuthenticationException Full Class Name
        /// </summary>
        public const string FeedAuthenticationException = "Microsoft.WindowsErrorReporting.Services.Data.FeedAuthenticationException";

        /// <summary>
        /// FileUploadException Full Class Name
        /// </summary>
        public const string FileUploadException = "Microsoft.WindowsErrorReporting.Services.Data.FileUploadException";

        /// <summary>
        /// FeedException Full Class Name
        /// </summary>
        public const string FeedException = "Microsoft.WindowsErrorReporting.Services.Data.FeedException";

        /// <summary>
        /// FeedAccessException Full Class Name
        /// </summary>
        public const string FeedAccessException = "Microsoft.WindowsErrorReporting.Services.Data.FeedAccessException";

        /// <summary>
        /// ArgumentOutOfRangeException Full Class Name
        /// </summary>
        public const string ArgumentOutOfRangeException = "System.ArgumentOutOfRangeException";

        /// <summary>
        /// Method to initialize the special exception name list.
        /// </summary>
        /// <returns>List of Exception Names</returns>
        private static List<string> GetSpecialInnerExceptionNameList()
        {
            List<string> nameList = new List<string>();

            nameList.Add(SpecialExceptionList.FeedException);
            nameList.Add(SpecialExceptionList.FeedAccessException);
            nameList.Add(SpecialExceptionList.FeedAuthenticationException);
            nameList.Add(SpecialExceptionList.ArgumentOutOfRangeException);
            
            return nameList;
        }
    }
}
