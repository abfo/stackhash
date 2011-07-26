using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StackHashUtilities;

namespace ServiceUnitTests
{
    static class ServiceTestSettings
    {
        public static String WinQualUserName
        {
            get
            {
                return TestSettings.WinQualUserName;
            }
        }
        public static String WinQualPassword
        {
            get
            {
                return TestSettings.WinQualPassword;
            }
        }
    }
}
