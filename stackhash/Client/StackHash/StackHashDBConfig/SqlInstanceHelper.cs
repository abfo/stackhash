using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;

namespace StackHashDBConfig
{
    /// <summary>
    /// Helper class to list installed SQL instances, adapted from sample code
    /// at http://msdn.microsoft.com/en-us/library/dd981032.aspx
    /// </summary>
    public static class SqlInstanceHelper
    {
        private const int MajorVersionIsAtLeast = 9; // 2005 or later

        /// <summary>
        /// Returns a list of SQL Server 2005/2008 instances on this machine
        /// </summary>
        /// <returns>List of SQL Server instances</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public static List<DatabaseInstance> FindInstances()
        {
            List<DatabaseInstance> instances = new List<DatabaseInstance>();

            string correctNamespace = GetCorrectWmiNameSpace();
            if (string.IsNullOrEmpty(correctNamespace))
            {
                return instances;
            }
            string query = string.Format("select * from SqlServiceAdvancedProperty where SQLServiceType = 1 and PropertyName = 'instanceID'");
            using (ManagementObjectSearcher getSqlEngine = new ManagementObjectSearcher(correctNamespace, query))
            {
                if (getSqlEngine.Get().Count == 0)
                {
                    return instances;
                }
                
                string instanceName = string.Empty;
                string serviceName = string.Empty;
                string version = string.Empty;
                string edition = string.Empty;
                
                foreach (ManagementObject sqlEngine in getSqlEngine.Get())
                {
                    serviceName = sqlEngine["ServiceName"].ToString();
                    instanceName = GetInstanceNameFromServiceName(serviceName);
                    version = GetWmiPropertyValueForEngineService(serviceName, correctNamespace, "Version");
                    edition = GetWmiPropertyValueForEngineService(serviceName, correctNamespace, "SKUNAME");

                    DatabaseInstance candidateInstance = new DatabaseInstance(instanceName, serviceName, edition, version);
                    if (candidateInstance.MajorVersion >= MajorVersionIsAtLeast)
                    {
                        instances.Add(candidateInstance);
                    }
                }
            }

            return instances;
        }

        /// <summary>
        /// Method returns the correct SQL namespace to use to detect SQL Server instances.
        /// </summary>
        /// <returns>namespace to use to detect SQL Server instances</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "NameSpace"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        private static string GetCorrectWmiNameSpace()
        {
            String wmiNamespaceToUse = "root\\Microsoft\\sqlserver";
            List<string> namespaces = new List<string>();
            try
            {
                // Enumerate all WMI instances of
                // __namespace WMI class.
                using (ManagementClass nsClass =
                    new ManagementClass(
                    new ManagementScope(wmiNamespaceToUse),
                    new ManagementPath("__namespace"),
                    null))
                {
                    foreach (ManagementObject ns in
                        nsClass.GetInstances())
                    {
                        namespaces.Add(ns["Name"].ToString());
                    }
                }

            }
            catch (ManagementException e)
            {
                Console.WriteLine("Exception = " + e.Message);
            }
            if (namespaces.Count > 0)
            {
                if (namespaces.Contains("ComputerManagement10"))
                {
                    //use katmai+ namespace
                    wmiNamespaceToUse = wmiNamespaceToUse + "\\ComputerManagement10";
                }
                else if (namespaces.Contains("ComputerManagement"))
                {
                    //use yukon namespace
                    wmiNamespaceToUse = wmiNamespaceToUse + "\\ComputerManagement";
                }
                else
                {
                    wmiNamespaceToUse = string.Empty;
                }
            }
            else
            {
                wmiNamespaceToUse = string.Empty;
            }
            return wmiNamespaceToUse;
        }

        /// <summary>
        /// method extracts the instance name from the service name
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        private static string GetInstanceNameFromServiceName(string serviceName)
        {
            if (!string.IsNullOrEmpty(serviceName))
            {
                if (string.Equals(serviceName, "MSSQLSERVER", StringComparison.OrdinalIgnoreCase))
                {
                    return serviceName;
                }
                else
                {
                    return serviceName.Substring(serviceName.IndexOf('$') + 1, serviceName.Length - serviceName.IndexOf('$') - 1);
                }
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Returns the WMI property value for a given property name for a particular SQL Server service Name
        /// </summary>
        /// <param name="serviceName">The service name for the SQL Server engine serivce to query for</param>
        /// <param name="wmiNamespace">The wmi namespace to connect to </param>
        /// <param name="propertyName">The property name whose value is required</param>
        /// <returns></returns>
        private static string GetWmiPropertyValueForEngineService(string serviceName, string wmiNamespace, string propertyName)
        {
            string propertyValue = string.Empty;
            string query = String.Format("select * from SqlServiceAdvancedProperty where SQLServiceType = 1 and PropertyName = '{0}' and ServiceName = '{1}'", propertyName, serviceName);
            using (ManagementObjectSearcher propertySearcher = new ManagementObjectSearcher(wmiNamespace, query))
            {
                foreach (ManagementObject sqlEdition in propertySearcher.Get())
                {
                    propertyValue = sqlEdition["PropertyStrValue"].ToString();
                }
            }
            return propertyValue;
        }
    }
}
