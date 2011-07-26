using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Collections.ObjectModel;


namespace StackHashBusinessObjects
{
    /// <summary>
    /// Information relating to the user's license on the machine.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashLicenseData
    {
        private String m_LicenseId;
        private String m_CompanyName;
        private String m_DepartmentName;
        private long m_MaxEvents;
        private int m_MaxSeats;
        private DateTime m_ExpiryUTC;
        private bool m_LicenseDefined;
        private bool m_IsTrialLicense;


        /// <summary>
        /// A GUID representing the license ID.
        /// </summary>
        [DataMember]
        public String LicenseId
        {
            get
            {
                return m_LicenseId;
            }
            set
            {
                m_LicenseId = value;
            }
        }

        /// <summary>
        /// Name of the company to whom the license belongs.
        /// </summary>
        [DataMember]
        public String CompanyName
        {
            get
            {
                return m_CompanyName;
            }
            set
            {
                m_CompanyName = value;
            }
        }

        /// <summary>
        /// Department within the company.
        /// </summary>
        [DataMember]
        public String DepartmentName
        {
            get
            {
                return m_DepartmentName;
            }
            set
            {
                m_DepartmentName = value;
            }
        }

        /// <summary>
        /// The maximum number of events allowed to be downloaded.
        /// </summary>
        [DataMember]
        public long MaxEvents
        {
            get
            {
                return m_MaxEvents;
            }
            set
            {
                m_MaxEvents = value;
            }
        }

        /// <summary>
        /// The maximum number of client seats permitted.
        /// </summary>
        [DataMember]
        public int MaxSeats
        {
            get
            {
                return m_MaxSeats;
            }
            set
            {
                m_MaxSeats = value;
            }
        }

        /// <summary>
        /// Expiry date for the license.
        /// </summary>
        [DataMember]
        public DateTime ExpiryUtc
        {
            get
            {
                return m_ExpiryUTC;
            }
            set
            {
                m_ExpiryUTC = value;
            }
        }

        /// <summary>
        /// Expiry date for the license.
        /// </summary>
        [DataMember]
        public bool LicenseDefined
        {
            get
            {
                return m_LicenseDefined;
            }
            set
            {
                m_LicenseDefined = value;
            }
        }

        /// <summary>
        /// Determines if the license is a trial license or not.
        /// </summary>
        [DataMember]
        public bool IsTrialLicense
        {
            get
            {
                return m_IsTrialLicense;
            }
            set
            {
                m_IsTrialLicense = value;
            }
        }


        public StackHashLicenseData() { ; } // Required for serialization.
        public StackHashLicenseData(bool licenseDefined, String licenseId, String companyName, String departmentName, long maxEvents, int maxSeats, DateTime expiryUtc, bool isTrialLicense)
        {
            m_LicenseDefined = licenseDefined;
            m_LicenseId = licenseId;
            m_CompanyName = companyName;
            m_DepartmentName = departmentName;
            m_MaxEvents = maxEvents;
            m_MaxSeats = maxSeats;
            m_ExpiryUTC = expiryUtc;
            m_IsTrialLicense = isTrialLicense;
        }


        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="licenseData">License data to copy.</param>
        public StackHashLicenseData(StackHashLicenseData licenseData)
        {
            if (licenseData == null)
                throw new ArgumentNullException("licenseData");

            m_LicenseDefined = licenseData.LicenseDefined;
            m_LicenseId = licenseData.LicenseId;
            m_CompanyName = licenseData.CompanyName;
            m_DepartmentName = licenseData.DepartmentName;
            m_MaxEvents = licenseData.MaxEvents;
            m_MaxSeats = licenseData.MaxSeats;
            m_ExpiryUTC = licenseData.ExpiryUtc;
            m_IsTrialLicense = licenseData.IsTrialLicense;
        }


        /// <summary>
        /// Gets the object data in a serialised, encrypted byte stream.
        /// The byte stream is encrypted using the user's logon credentials.
        /// </summary>
        /// <returns>Byte array containing the serialised encrypted data</returns>
        private byte [] getData()
        {
	        // Create a new object clone of this object for serialization.
	        StackHashLicenseData tempLicenseData = 
		        new StackHashLicenseData(this);

	        // Create an output memory stream - this will grow as data is written.
	        MemoryStream ms = new MemoryStream();
            MD5 md5 = null;

            try
            {
	            // Construct a BinaryFormatter and use it to serialize the data to the stream.
	            BinaryFormatter formatter = new BinaryFormatter();
	            formatter.Serialize(ms, tempLicenseData);
	
	            // Encrypt the data in memory using the user's logon credentials.
	            byte [] data = EncryptInMemoryData(ms.GetBuffer(), DataProtectionScope.LocalMachine);

	            // Add a checksum to the start of the data. MD5 returns a 16 byte hash.
	            md5 = new MD5CryptoServiceProvider();

                byte[] hash = md5.ComputeHash(data);

                int totalSize = 16 + data.Length;
                byte[] outBuffer = new byte[totalSize];

                int nextByte = 0;
                for (int i = 0; i < 16; i++)
                    outBuffer[nextByte++] = hash[i];

                for (int i = 0; i < data.Length; i++)
                    outBuffer[nextByte++] = data[i];

                // Return the buffer.
                return outBuffer;
            }
            finally
            {
                if (md5 != null) 
                    md5.Dispose();
                if (ms != null)
                    ms.Dispose();
            }
        }


        /// <summary>
        /// Calculates the MD5 hash of the specified input buffer.
        /// </summary>
        /// <param name="buffer">Buffer hash.</param>
        /// <param name="scope">See MemoryProtectionScope.</param>
        /// <exception cref="ArgumentException">Invalid key or iv lengths</exception>
        /// <returns>Encrypted data.</returns>
        public static byte [] EncryptInMemoryData(byte [] buffer, DataProtectionScope scope)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (buffer.Length <= 0)
                throw new ArgumentException("buffer cannot be 0 bytes in length", "scope");

	        // Encrypt the data in memory. The result is stored in the same array as the original data.
	        byte [] encryptedData = ProtectedData.Protect(buffer, null, scope);
	        return encryptedData;
        }


        /// <summary>
        /// Saves the license data to the specified location.
        /// </summary>
        /// <param name="fileName"></param>
        public void Save(String fileName)
        {
            FileStream fileStream = File.Open(fileName, FileMode.Create, FileAccess.Write);

            try
            {
                byte[] encyrptedData = getData();

                fileStream.Write(encyrptedData, 0, encyrptedData.Length);
            }
            finally
            {
                fileStream.Close();
            }
        }

        /// <summary>
        /// Decrypt a block of memory using the user's logon credentials.
        /// This uses a wrapper around the Win32 DPAPI - Data Protection API.
        /// This is an IN PLACE decrypt.
        /// </summary>
        /// <param name="buffer">Buffer to decrypt (multiple of 16 bytes).</param>
        /// <param name="scope">Machine wide or user wide.</param>
        /// <exception cref="ArgumentException">Invalid key or iv lengths</exception>
        /// <returns>Decrypted data.</returns>
        public static byte [] DecryptInMemoryData(byte [] buffer, DataProtectionScope scope)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (buffer.Length <= 0)
                throw new ArgumentException("Bugger cannot be empty", "buffer");

            // Decrypt the data in memory. The result is stored in the same array as the original data.
	        byte []  decryptedData = ProtectedData.Unprotect(buffer, null, scope);
	        return decryptedData;
        }

        /// <summary>
        /// Constructor for the CuckuSecrets object. 
        /// Initialises the instance using the data from a file which must have previously
        /// been previously created using GetData.
        /// </summary>
        /// <param name="fileData">Byte array - must be multiple of 16 bytes </param>
        /// <param name="saveRequired">True - caller should save the license file.</param>
        /// <exception cref="ArgumentException">Invalid data array</exception>
        public StackHashLicenseData(byte [] fileData)
        {	
	        if (fileData == null)
		        throw new ArgumentNullException("fileData");
	        if (fileData.Length < 20) // TODO
		        throw new ArgumentException("fileData too small", "fileData");

            StackHashLicenseData tempLicenseData = null;

	        // Check the checksum at the start of the data (first 16 bytes).
	        MD5 md5 = new MD5CryptoServiceProvider();

            try
            {
                byte[] hash = md5.ComputeHash(fileData, 16, fileData.Length - 16);

                for (int i = 0; i < hash.Length; i++)
                {
                    if (hash[i] != fileData[i])
                        throw new ArgumentException("fileData points to invalid data", "fileData");
                }

                // Create array with stripped checksum.
                byte[] newFileData = new byte[fileData.Length - hash.Length];
                System.Array.Copy(fileData, hash.Length, newFileData, 0, fileData.Length - hash.Length);

                // Decrypt the data specified using the User's login credentials.
                // Note that the encryptor ensures the block will be a multiple of
                // 16 bytes as expected by DecryptInMemoryData.

                bool loadedLicenseData = false;
                try
                {
                    newFileData = DecryptInMemoryData(newFileData, DataProtectionScope.LocalMachine);
                    loadedLicenseData = true;
                }
                catch (CryptographicException) { }


                // License used to be encrypted with CurrentUser key.
                if (!loadedLicenseData)
                {
                    newFileData = DecryptInMemoryData(newFileData, DataProtectionScope.CurrentUser);
                }


                // Create a memory stream based on the supplied buffer.
                MemoryStream ms = new MemoryStream(newFileData);

                try
                {
                    // Construct a BinaryFormatter and use it to deserialize the data to a new object.
                    BinaryFormatter formatter = new BinaryFormatter();
                    tempLicenseData = (StackHashLicenseData)formatter.Deserialize(ms);

                    // Now copy the deserialised object.
                    m_LicenseId = tempLicenseData.LicenseId;
                    m_CompanyName = tempLicenseData.CompanyName;
                    m_DepartmentName = tempLicenseData.DepartmentName;
                    m_MaxEvents = tempLicenseData.MaxEvents;
                    m_MaxSeats = tempLicenseData.MaxSeats;
                    m_ExpiryUTC = tempLicenseData.ExpiryUtc;
                    m_IsTrialLicense = tempLicenseData.IsTrialLicense;
                }
                finally
                {
                    if (ms != null)
                        ms.Dispose();
                }
            }
            finally
            {
                md5.Dispose();
            }
        }
    
        /// <summary>
        /// Loads the license data from the specified location.
        /// </summary>
        /// <param name="fileName">File to load the license data from.</param>
        /// <returns></returns>
        public static StackHashLicenseData Load(String fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            if (!File.Exists(fileName))
                throw new ArgumentException("File does not exist: " + fileName, "fileName");

            FileStream licenseFile = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            StackHashLicenseData licenseData = null;
            try
            {
                byte[] bytes = new byte[licenseFile.Length];

                licenseFile.Read(bytes, 0, (int)licenseFile.Length);
                
                licenseData = new StackHashLicenseData(bytes);

                if (!String.IsNullOrEmpty(licenseData.LicenseId))
                    licenseData.LicenseDefined = true;
            }
            finally
            {
                if (licenseFile != null)
                    licenseFile.Close();
            }
            return licenseData;
        }
    }

    
    /// <summary>
    /// Information relating to the current license usage for a context.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashContextLicenseUsage
    {
        private int m_ContextId;
        private long m_NumberOfStoredEvents;

        /// <summary>
        /// The number of stored events for this context.
        /// </summary>
        [DataMember]
        public int ContextId
        {
            get
            {
                return m_ContextId;
            }
            set
            {
                m_ContextId = value;
            }
        }

        /// <summary>
        /// The number of stored events for this context.
        /// </summary>
        [DataMember]
        public long NumberOfStoredEvents
        {
            get
            {
                return m_NumberOfStoredEvents;
            }
            set
            {
                m_NumberOfStoredEvents = value;
            }
        }
    }
    

    /// <summary>
    /// Collection of context license usage data.
    /// </summary>
    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashContextLicenseUsageCollection : Collection<StackHashContextLicenseUsage>
    {
        public StackHashContextLicenseUsageCollection() { ; } // Required for serialization.
    }


    /// <summary>
    /// Information relating to the current license usage for a client.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashClientLicenseUsage
    {
        private StackHashClientData m_ClientData;
        private DateTime m_LastAccessTime;
        private DateTime m_ClientConnectTime;

        /// <summary>
        /// Identifies the client.
        /// </summary>
        [DataMember]
        public StackHashClientData ClientData
        {
            get
            {
                return m_ClientData;
            }
            set
            {
                m_ClientData = value;
            }
        }

        /// <summary>
        /// Last time the client made a call.
        /// </summary>
        [DataMember]
        public DateTime LastAccessTime
        {
            get
            {
                return m_LastAccessTime;
            }
            set
            {
                m_LastAccessTime = value;
            }
        }

        /// <summary>
        /// Created time.
        /// </summary>
        [DataMember]
        public DateTime ClientConnectTime
        {
            get
            {
                return m_ClientConnectTime;
            }
            set
            {
                m_ClientConnectTime = value;
            }
        }
    }


    
    /// <summary>
    /// Collection of client license usage data.
    /// </summary>
    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashClientLicenseUsageCollection : Collection<StackHashClientLicenseUsage>
    {
        public StackHashClientLicenseUsageCollection() { ; } // Required for serialization.
    }

    
    /// <summary>
    /// Information relating to the current license usage for a context.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashLicenseUsage
    {
        private StackHashContextLicenseUsageCollection m_ContextLicenseUsageCollection;
        private StackHashClientLicenseUsageCollection m_ClientLicenseUsageCollection;

        /// <summary>
        /// The number of stored events for this context.
        /// </summary>
        [DataMember]
        public StackHashContextLicenseUsageCollection ContextLicenseUsageCollection
        {
            get
            {
                return m_ContextLicenseUsageCollection;
            }
            set
            {
                m_ContextLicenseUsageCollection = value;
            }
        }

        /// <summary>
        /// Client related license data.
        /// </summary>
        [DataMember]
        public StackHashClientLicenseUsageCollection ClientLicenseUsageCollection
        {
            get
            {
                return m_ClientLicenseUsageCollection;
            }
            set
            {
                m_ClientLicenseUsageCollection = value;
            }
        }
    }

}
