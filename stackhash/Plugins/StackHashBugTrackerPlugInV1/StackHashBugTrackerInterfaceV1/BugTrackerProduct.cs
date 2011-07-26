using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace StackHashBugTrackerInterfaceV1
{
    [Serializable]
    public class BugTrackerProduct
    {
        String m_ProductName;
        String m_ProductVersion;
        long m_ProductId;

        public BugTrackerProduct(String productName, String productVersion, long productId)
        {
            m_ProductName = productName;
            m_ProductVersion = productVersion;
            m_ProductId = productId;
        }

        public String ProductName
        {
            get { return m_ProductName; }
        }

        public String ProductVersion
        {
            get { return m_ProductVersion; }
        }

        public long ProductId
        {
            get { return m_ProductId; }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            String mainFields = String.Format(CultureInfo.InvariantCulture, "Product: Id={0}, Name={1}, Version={2}",
                m_ProductId, m_ProductName, m_ProductVersion);
            result.AppendLine(mainFields);

            return result.ToString();
        }
    }

}
