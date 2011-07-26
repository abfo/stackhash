using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WinQualAtomFeed
{
    public interface IWebCalls
    {
        String WinQualCall(String url, RequestType requestType, String postPayload, String soapAction);
    }
}
