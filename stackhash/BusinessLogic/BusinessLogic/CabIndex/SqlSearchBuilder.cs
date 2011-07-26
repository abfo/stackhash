using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;

using StackHashBusinessObjects;

namespace StackHashErrorIndex
{
    [SuppressMessage("Microsoft.Performance", "CA1812")]
    public class SqlSearchBuilder
    {
        StringBuilder m_SqlSearch;

        static String s_ProductTable = "P.";
        static String s_FileTable = "F.";
        static String s_EventTable = "E.";
        static String s_EventInfoTable = "EI.";
        static String s_CabTable = "C.";


        public SqlSearchBuilder(String command)
        {
            m_SqlSearch = new StringBuilder(command);
        }

        public String SqlSearchString
        {
            get { return m_SqlSearch.ToString(); }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811")]
        private static String getTableName(StackHashSearchOption searchOption)
        {
            switch (searchOption.ObjectType)
            {
                case StackHashObjectType.Product:
                    return s_ProductTable;
                case StackHashObjectType.File:
                    return s_FileTable;
                case StackHashObjectType.Event:
                case StackHashObjectType.EventSignature:
                    return s_EventTable;
                case StackHashObjectType.EventInfo:
                    return s_EventInfoTable;
                case StackHashObjectType.CabInfo:
                    return s_CabTable;
                default:
                    throw new InvalidOperationException("Unknown object type");
            }            
        }


        public void AddSearchOption(StackHashSearchOption searchOption)
        {
            if (searchOption == null)
                throw new ArgumentNullException("searchOption");

            m_SqlSearch.Append(searchOption.ToSqlString(getTableName(searchOption)));
        }


        public void AddSearchCriteria(StackHashSearchCriteria searchCriteria)
        {
            if (searchCriteria == null)
                throw new ArgumentNullException("searchCriteria");

            bool firstOption = true;

            m_SqlSearch.Append("(");

            foreach (StackHashSearchOption option in searchCriteria.SearchFieldOptions)
            {
                if (!firstOption)
                    m_SqlSearch.Append(" AND ");
                else
                    firstOption = false;

                AddSearchOption(option);
            }

            m_SqlSearch.Append(")");
        }

        public void AddSearchCriteria(StackHashSearchCriteriaCollection searchCriteria)
        {
            if (searchCriteria == null)
                throw new ArgumentNullException("searchCriteria");

            bool firstCriteria = true;

            m_SqlSearch.Append("(");
            foreach (StackHashSearchCriteria criteria in searchCriteria)
            {
                if (!firstCriteria)
                    m_SqlSearch.Append(" OR ");
                else
                    firstCriteria = false;

                // These conditions should be connected with an OR.
                AddSearchCriteria(criteria);
            }
        }
    }
}
