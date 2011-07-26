using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace StackHash
{
    /// <summary>
    /// Utility methods for detecting and chaging folder permissions for
    /// StackHash
    /// </summary>
    public static class FolderPermissionHelper
    {
        /// <summary>
        /// Determines if NETWORK SERVICE has full access to the specified folder
        /// </summary>
        /// <param name="folder">Folder to check</param>
        /// <returns>True if NETWORK SERVICE has full access</returns>
        public static bool NSHasAccess(string folder)
        {
            if (folder == null)
            {
                throw new ArgumentNullException("folder");
            }

            if (!Directory.Exists(folder))
            {
                throw new DirectoryNotFoundException(folder);
            }

            // NETWORK SERVICE
            SecurityIdentifier nssid = new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null);

            bool hasAccess = false;

            DirectorySecurity directorySecurity = Directory.GetAccessControl(folder);
            AuthorizationRuleCollection authorizationRules = directorySecurity.GetAccessRules(true, true, typeof(SecurityIdentifier));
            foreach (AuthorizationRule rule in authorizationRules)
            {
                FileSystemAccessRule fileSystemAccessRule = rule as FileSystemAccessRule;
                if (fileSystemAccessRule != null)
                {
                    // check for NETWORK SERVICE
                    if (fileSystemAccessRule.IdentityReference == nssid)
                    {
                        // now check for full control
                        if (((fileSystemAccessRule.FileSystemRights & FileSystemRights.FullControl) == FileSystemRights.FullControl) &&
                            (fileSystemAccessRule.AccessControlType == AccessControlType.Allow))
                        {
                            hasAccess = true;
                            break;
                        }
                    }
                }
            }

            return hasAccess;
        }

        /// <summary>
        /// Provides NETWORK SERVICE with full access to the specified folder
        /// </summary>
        /// <param name="folder">Folder to add access to</param>
        public static void NSAddAccess(string folder)
        {
            if (folder == null)
            {
                throw new ArgumentNullException("folder");
            }

            if (!Directory.Exists(folder))
            {
                throw new DirectoryNotFoundException(folder);
            }

            if (!NSHasAccess(folder))
            {
                // NETWORK SERVICE
                SecurityIdentifier nssid = new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null);
                DirectorySecurity directorySecurity = Directory.GetAccessControl(folder);
                directorySecurity.AddAccessRule(new FileSystemAccessRule(nssid, FileSystemRights.FullControl, AccessControlType.Allow));
                Directory.SetAccessControl(folder, directorySecurity);
            }
        }
    }
}
