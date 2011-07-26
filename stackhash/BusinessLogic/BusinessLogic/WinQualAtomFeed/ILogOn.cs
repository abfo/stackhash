using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;

namespace WinQualAtomFeed
{
    public interface ILogOn : IDisposable
    {
        void Initialise(IWebCalls webCalls);
        bool LogIn(String userName, String password);
        void LogInWithException(String userName, String password);
        void ProcessRequest(HttpWebRequest webRequest);
        void ProcessRequest(WebHeaderCollection webRequestHeaders);
        void ProcessResponse(HttpWebResponse webResponse);
        bool ProcessWebException(WebException ex);
        void LogOut();
    }
}
