using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Security;
using System.Security;
using StackHashUtilities;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics.CodeAnalysis;

namespace StackHashUtilities
{
    public static class MyCertificateValidation
    {
        [SuppressMessage("Microsoft.Usage", "CA1801")]
        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        // The following method is invoked by the RemoteCertificateValidationDelegate.
        public static bool ValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            if (certificate == null)
                return false;

            // Dump the error and certificate data.
            String errorString = String.Format(CultureInfo.InvariantCulture, "Certificate error: {0} Certificate: {1}", sslPolicyErrors, certificate);
            DiagnosticsHelper.LogMessage(DiagSeverity.Warning, errorString);

            // Microsoft seem to have certificate issues possibly related to load balancing. 
            // We are seeing certificate name errors where CN=supportnatalapj.xbox.com and DNS Name = kinectsupport.xbox.com
            if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch)
            {
                if (String.Compare(certificate.Issuer, "CN=Microsoft Secure Server Authority, DC=redmond, DC=corp, DC=microsoft, DC=com", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "ISSUER ACCEPTED: " + certificate.Issuer);

                    if (String.Compare(certificate.Subject, "CN=supportnatalapj.xbox.com", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "SUBJECT ACCEPTED: " + certificate.Subject);
                        return true;
                    }
                    else
                    {
                        DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "SUBJECT NOT ACCEPTED: " + certificate.Subject);
                        return true;
                    }
                }
                else
                {
                    DiagnosticsHelper.LogMessage(DiagSeverity.Warning, "ISSUER NOT ACCEPTED: " + certificate.Issuer);
                }
            }

            // Do not allow this client to communicate with unauthenticated servers.
            return false;
        }
    }
}
