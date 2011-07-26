using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StackHashUtilities
{
    public static class ExceptionExtensions
    {
        public static bool ContainsExceptionType(this System.Exception thisException, Type exceptionTypeToSearchFor)
        {
            System.Exception ex = thisException;
            while (ex != null)
            {
                if (ex.GetType() == exceptionTypeToSearchFor)
                    return true;
                ex = ex.InnerException;
            }
            return false;
        }

        public static String BuildDescription(this System.Exception thisException)
        {
            Exception ex = thisException;

            String message = "";
            while (ex != null)
            {
                message += ex.GetType().ToString() + " : ";
                if (!String.IsNullOrEmpty(ex.Message))
                    message = message + " : " + ex.Message + ", ";
                ex = ex.InnerException;
            }
            return message;
        }
    }
}
