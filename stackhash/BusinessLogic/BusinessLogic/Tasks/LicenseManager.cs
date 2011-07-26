using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using StackHashBusinessObjects;
using StackHashUtilities;

namespace StackHashTasks
{
    public class LicenseManager : IDisposable
    {
        private StackHashLicenseData m_LicenseData;
        private String m_LicenseFileName;
        private String m_ServiceGuid;

        /// <summary>
        /// License ID.
        /// </summary>
        public String LicenseId
        {
            get
            {
                Monitor.Enter(this);

                try
                {
                    return m_LicenseData.LicenseId;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }

        /// <summary>
        /// Name of the company to whom the license belongs.
        /// </summary>
        public String CompanyName
        {
            get
            {
                Monitor.Enter(this);

                try
                {
                    return m_LicenseData.CompanyName;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }

        /// <summary>
        /// Department within the company.
        /// </summary>
        public String DepartmentName
        {
            get
            {
                Monitor.Enter(this);

                try
                {
                    return m_LicenseData.DepartmentName;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }

        /// <summary>
        /// The maximum number of events allowed to be downloaded.
        /// </summary>
        public long MaxEvents
        {
            get
            {
                Monitor.Enter(this);

                try
                {
                    return m_LicenseData.MaxEvents;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }

        /// <summary>
        /// The maximum number of client seats permitted.
        /// </summary>
        public int MaxSeats
        {
            get
            {
                Monitor.Enter(this);

                try
                {
                    return m_LicenseData.MaxSeats;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }

        /// <summary>
        /// Expiry date for the license.
        /// </summary>
        public DateTime ExpiryUtc
        {
            get
            {
                Monitor.Enter(this);

                try
                {
                    return m_LicenseData.ExpiryUtc;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }

        /// <summary>
        /// A full copy of the license data.
        /// </summary>
        public StackHashLicenseData LicenseData
        {
            get
            {
                Monitor.Enter(this);

                try
                {
                    return new StackHashLicenseData(m_LicenseData);
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }

        /// <summary>
        /// Is a trial license?
        /// </summary>
        public bool IsTrialLicense
        {
            get
            {
                Monitor.Enter(this);

                try
                {
                    if (m_LicenseData != null)
                        return m_LicenseData.IsTrialLicense;
                    else
                        return false;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }
        
        /// <summary>
        /// Initialises the license manager from the specified file.
        /// </summary>
        /// <param name="licenseFileName"></param>
        /// <param name="serviceGuid">Unique guid for the service.</param>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public LicenseManager(String licenseFileName, String serviceGuid)
        {
            if (licenseFileName == null)
                throw new ArgumentNullException(licenseFileName);

            m_LicenseFileName = licenseFileName;
            m_ServiceGuid = serviceGuid;

            // Just set a default license.
            // To introduce license management - add code here to read the license file.
            // Expires in year 2100. 
            // Events = 0x7fffffff.
            // Seats = 0x7fffffff.           
            m_LicenseData = new StackHashLicenseData(true, "FULLLICENSE", "Company", "Department", 0x7fffffff, 0x7fffffff, 
                new DateTime(2100, 1, 1), false);
        }


        /// <summary>
        /// Saves the license data.
        /// </summary>
        public void Save()
        {
            Monitor.Enter(this);

            try
            {
                m_LicenseData.Save(m_LicenseFileName);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        /// <summary>
        /// Deletes the current license.
        /// </summary>
        private void deleteLicense()
        {

            m_LicenseData = new StackHashLicenseData(false, null, null, null, 0, 0, new DateTime(0), false);

            if (File.Exists(m_LicenseFileName))
                File.Delete(m_LicenseFileName);

            DiagnosticsHelper.LogMessage(DiagSeverity.Information, "License deleted");
        }


        /// <summary>
        /// Sets the current license data.
        /// null causes a refresh.
        /// </summary>
        /// <param name="licenseId">The ID of the new license.</param>
        public void SetLicense(String licenseId)
        {
            
            Monitor.Enter(this);

            try
            {
                // TODO: Go get the license data associated with the specified license ID from the license server.
                DiagnosticsHelper.LogMessage(DiagSeverity.Information, "License changed to: " + licenseId);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        /// Gets a new trial license.
        /// </summary>
        /// <param name="licenseId">The ID of the new license.</param>
        public void GetTrialLicense()
        {
            Monitor.Enter(this);

            try
            {
                // TODO: Get the license from the license server.

                // Save it.
                Save();
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        #region IDisposable Members

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
