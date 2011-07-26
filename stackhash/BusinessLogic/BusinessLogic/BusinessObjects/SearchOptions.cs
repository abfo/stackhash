using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Data.SqlTypes;

namespace StackHashBusinessObjects
{
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public enum StackHashSearchDirection
    {
        [EnumMember()]
        Forwards,
        [EnumMember()]
        Backwards
    }

    
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public enum StackHashSearchFieldType
    {
        [EnumMember()]
        Integer,
        [EnumMember()]
        Long,
        [EnumMember()]
        DateTime,
        [EnumMember()]
        String
    }

    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public enum StackHashObjectType
    {
        [EnumMember()]
        Product,
        [EnumMember()]
        File,
        [EnumMember()]
        Event,
        [EnumMember()]
        EventInfo,
        [EnumMember()]
        CabInfo,
        [EnumMember()]
        EventSignature,
        [EnumMember()]
        Script,
        [EnumMember()]
        EventWorkFlow,
        [EnumMember()]
        EventNotes,
        [EnumMember()]
        CabNotes,
    }

    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    [KnownType(typeof(IntSearchOption))]
    [KnownType(typeof(LongSearchOption))]
    [KnownType(typeof(DateTimeSearchOption))]
    [KnownType(typeof(StringSearchOption))]
    [SuppressMessage("Microsoft.Design", "CA1012")]
    public abstract class StackHashSearchOption
    {
        private StackHashSearchFieldType m_SearchType;
        private String m_FieldName;
        private StackHashObjectType m_ObjectType;
        private StackHashSearchOptionType m_SearchOptionType;

        private static String s_ProductTable = "P";
        private static String s_FileTable = "F";
        private static String s_EventTable = "E";
        private static String s_EventInfoTable = "EI";
        private static String s_CabTable = "C";
        private static String s_WorkFlowTable = "C";
        private static String s_CabCountTable = "CABCOUNTER";

        public StackHashSearchOption()
        {
        }

        public StackHashSearchOption(StackHashSearchFieldType searchType, StackHashObjectType objectType, String fieldName, 
            StackHashSearchOptionType searchOptionType)
        {
            m_SearchType = searchType;
            m_ObjectType = objectType;
            m_FieldName = fieldName;
            m_SearchOptionType = searchOptionType;
        }

        [DataMember]
        public StackHashSearchFieldType SearchType
        {
            get { return m_SearchType; }
            set { m_SearchType = value; }
        }
        [DataMember]
        public StackHashObjectType ObjectType
        {
            get { return m_ObjectType; }
            set { m_ObjectType = value; }
        }
        [DataMember]
        public String FieldName
        {
            get { return m_FieldName; }
            set { m_FieldName = value; }
        }
        [DataMember]
        public StackHashSearchOptionType SearchOptionType
        {
            get { return m_SearchOptionType; }
            set { m_SearchOptionType = value; }
        }

        public virtual String ToSqlString(String tableName)
        {
            return String.Empty;
        }


        public static String CorrectTableName(StackHashObjectType objectType, String fieldName, String originalTableName)
        {
            // Some fields - e.g. EventTypeName are actually stored in a separate table and 
            // referenced from via an EventTypeId.
            // If the user wants to search on EventTypeName, the field will not thus appear in 
            // the expected object table E in this case (Events table), it will be in the ET table.

            switch (ConvertFieldName(objectType, fieldName))
            {
                case "EventTypeName":
                    if (originalTableName == "C")  // The CabInfo also has the EventTypeName.
                        return "CET";
                    else
                        return "ET";
                case "OperatingSystemName":
                    return "O";
                case "OperatingSystemVersion":
                    return "O";
                case "LocaleCode":
                    return "l";
                case "LocaleName":
                    return "l";
                case "WorkFlowStatusName":
                    return "WF";
                case "CabCount":
                    return s_CabCountTable;
                case "User":
                case "StackHashUser":
                    return "U";
                case "Source":
                    return "S";

                default:
                    return originalTableName;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811")]
        public static String GetTableName(StackHashObjectType objectType)
        {
            switch (objectType)
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
                case StackHashObjectType.EventWorkFlow:
                    return s_WorkFlowTable;
                default:
                    throw new InvalidOperationException("Unknown object type");
            }
        }


        public static bool IsValidSqlFieldName(StackHashObjectType objectType, String fieldName)
        {
            Type stackHashObjectType;

            switch (objectType)
            {
                case StackHashObjectType.Product:
                    stackHashObjectType = typeof(StackHashProduct);
                    break;
                case StackHashObjectType.File:
                    stackHashObjectType = typeof(StackHashFile);
                    break;
                case StackHashObjectType.Event:
                    stackHashObjectType = typeof(StackHashEvent);
                    break;
                case StackHashObjectType.EventSignature:
                    stackHashObjectType = typeof(StackHashEventSignature);
                    break;
                case StackHashObjectType.EventInfo:
                    stackHashObjectType = typeof(StackHashEventInfo);
                    break;
                case StackHashObjectType.CabInfo:
                    stackHashObjectType = typeof(StackHashCab);
                    break;
                case StackHashObjectType.EventWorkFlow:
                    // This is a virtual object - doesn't really exist. Needs to be like this because the workflowstatusname is 
                    // in the work flow table and not in the event table.
                    if (String.Compare("WorkFlowStatusName", fieldName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                    else
                    {
                        return (fieldName == "*");
                    }
                case StackHashObjectType.EventNotes:
                    stackHashObjectType = typeof(StackHashNoteEntry);
                    break;
                case StackHashObjectType.CabNotes:
                    stackHashObjectType = typeof(StackHashNoteEntry);
                    break;
                default:
                    throw new InvalidOperationException("Unknown object type");
            }

            if (String.Compare("FilesLink", fieldName, StringComparison.OrdinalIgnoreCase) == 0)
                return false;

            // This field not present in the database. Search on FileId in StackHashFile instead.
            if ((objectType == StackHashObjectType.Event) && 
                 (String.Compare("FileId", fieldName, StringComparison.OrdinalIgnoreCase) == 0))
                return false;

            // The workflowstatus is found in the WorkFlow table.
            if ((objectType == StackHashObjectType.Event) &&
                 (String.Compare("WorkFlowStatusName", fieldName, StringComparison.OrdinalIgnoreCase) == 0))
                return false;

            if (fieldName == "*")
                return true;
            if (fieldName == "StructureVersion")
                return false;

            PropertyInfo property = stackHashObjectType.GetProperty(fieldName, BindingFlags.Public | BindingFlags.Instance);
            if (property != null)
                return true;
            else
                return false;
        }

        public static Collection<String> GetFieldsInObjectOfSpecifiedType(StackHashObjectType objectType, Type searchFieldType)
        {
            Collection<String> fields = new Collection<String>();

            Type stackHashObjectType;

            switch (objectType)
            {
                case StackHashObjectType.Product:
                    stackHashObjectType = typeof(StackHashProduct);
                    break;
                case StackHashObjectType.File:
                    stackHashObjectType = typeof(StackHashFile);
                    break;
                case StackHashObjectType.Event:
                    stackHashObjectType = typeof(StackHashEvent);
                    break;
                case StackHashObjectType.EventSignature:
                    stackHashObjectType = typeof(StackHashEventSignature);
                    break;
                case StackHashObjectType.EventInfo:
                    stackHashObjectType = typeof(StackHashEventInfo);
                    break;
                case StackHashObjectType.CabInfo:
                    stackHashObjectType = typeof(StackHashCab);
                    break;
                case StackHashObjectType.EventWorkFlow:
                    // Not a real object.
                    fields.Add("WorkFlowStatusName");
                    return fields;
                default:
                    throw new InvalidOperationException("Unknown object type");
            }

            PropertyInfo[] properties = stackHashObjectType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo pi in properties)
            {
                if (pi.PropertyType != searchFieldType)
                    continue;

                if (IsValidSqlFieldName(objectType, pi.Name))
                    fields.Add(pi.Name);
            }
            
            return fields;
        }

        public static String ConvertFieldName(StackHashObjectType objectType, String originalFieldName)
        {
            String returnFieldName = originalFieldName;

            // TODO: Validate all possible table names here. Call into Schema to do this if possible as
            // it should know better than us.

            switch (objectType)
            {
                case StackHashObjectType.Product:
                    if (originalFieldName == "Id")
                        returnFieldName = "ProductId";
                    else if (originalFieldName == "Name")
                        returnFieldName = "ProductName";
                    break;
                case StackHashObjectType.File:
                    if (originalFieldName == "Id")
                        returnFieldName = "FileId";
                    else if (originalFieldName == "Name")
                        returnFieldName = "FileName";
                    break;
                case StackHashObjectType.Event:
                    if (originalFieldName == "Id")
                        returnFieldName = "EventId";
                    break;
                case StackHashObjectType.EventSignature:
                    break;
                case StackHashObjectType.EventInfo:
                    if (originalFieldName == "Lcid")
                        returnFieldName = "LocaleId";
                    else if (originalFieldName == "Locale")
                        returnFieldName = "LocaleCode";
                    else if (originalFieldName == "Language")
                        returnFieldName = "LocaleName";
                    break;
                case StackHashObjectType.CabInfo:
                    if (originalFieldName == "Id")
                        returnFieldName = "CabId";
                    else if (originalFieldName == "FileName")
                        return "CabFileName";
                    break;
                case StackHashObjectType.EventWorkFlow:
                    break;
                case StackHashObjectType.EventNotes:
                    if (originalFieldName == "User")
                        return "StackHashUser";
                    break;
                case StackHashObjectType.CabNotes:
                    if (originalFieldName == "User")
                        return "StackHashUser";
                    break;
                default:
                    throw new InvalidOperationException("Unknown object type");
            }

            return returnFieldName;
        }
    }

    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public enum StackHashSearchOptionType
    {
        [EnumMember()]
        All,
        [EnumMember()]
        Equal,
        [EnumMember()]
        NotEqual,
        [EnumMember()]
        GreaterThan,
        [EnumMember()]
        GreaterThanOrEqual,
        [EnumMember()]
        LessThan,
        [EnumMember()]
        LessThanOrEqual,
        [EnumMember()]
        RangeInclusive,
        [EnumMember()]
        RangeExclusive,
        [EnumMember()]
        StringStartsWith,
        [EnumMember()]
        StringContains,
        [EnumMember()]
        StringDoesNotContain
    }

    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    [KnownType(typeof(StackHashSearchOption))]
    public class IntSearchOption : StackHashSearchOption
    {
        private int m_Start;
        private int m_End;


        public IntSearchOption()
        {
        }

        public IntSearchOption(StackHashObjectType objectType, String fieldName, StackHashSearchOptionType type, int start, int end)
            : base(StackHashSearchFieldType.Integer, objectType, fieldName, type)
        {
            m_Start = start;
            m_End = end;
        }

        [DataMember]
        public int Start
        {
            get { return m_Start; }
            set { m_Start = value; }
        }

        [DataMember]
        public int End
        {
            get { return m_End; }
            set { m_End = value; }
        }

        public bool IsMatch(int value)
        {
            switch (SearchOptionType)
            {
                case StackHashSearchOptionType.All:
                    return true;
                case StackHashSearchOptionType.GreaterThan:
                    return (value > m_Start);
                case StackHashSearchOptionType.GreaterThanOrEqual:
                    return (value >= m_Start);
                case StackHashSearchOptionType.LessThan:
                    return (value < m_Start);
                case StackHashSearchOptionType.LessThanOrEqual:
                    return (value <= m_Start);
                case StackHashSearchOptionType.NotEqual:
                    return (value != m_Start);
                case StackHashSearchOptionType.Equal:
                    return (value == m_Start);
                case StackHashSearchOptionType.RangeInclusive:
                    return ((value >= m_Start) && (value <= m_End));
                case StackHashSearchOptionType.RangeExclusive:
                    return ((value > m_Start) && (value < m_End));

                default:
                    throw new NotSupportedException("Unrecognized search option type");
            }
        }

        public override String ToSqlString(String tableName)
        {
            StringBuilder sqlString = new StringBuilder(String.Empty);

            if (SearchOptionType != StackHashSearchOptionType.All)
                sqlString.Append("(");

            String correctedTableName = CorrectTableName(this.ObjectType, this.FieldName, tableName);

            switch (SearchOptionType)
            {
                case StackHashSearchOptionType.All:
                    break;
                case StackHashSearchOptionType.GreaterThan:
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append(">");
                    sqlString.Append(m_Start.ToString(CultureInfo.InvariantCulture));
                    break;
                case StackHashSearchOptionType.GreaterThanOrEqual:
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append(">=");
                    sqlString.Append(m_Start.ToString(CultureInfo.InvariantCulture));
                    break;
                case StackHashSearchOptionType.LessThan:
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append("<");
                    sqlString.Append(m_Start.ToString(CultureInfo.InvariantCulture));
                    break;
                case StackHashSearchOptionType.LessThanOrEqual:
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append("<=");
                    sqlString.Append(m_Start.ToString(CultureInfo.InvariantCulture));
                    break;
                case StackHashSearchOptionType.NotEqual:
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append("<>");
                    sqlString.Append(m_Start.ToString(CultureInfo.InvariantCulture));
                    break;
                case StackHashSearchOptionType.Equal:
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append("=");
                    sqlString.Append(m_Start.ToString(CultureInfo.InvariantCulture));
                    break;
                case StackHashSearchOptionType.RangeInclusive:
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append(" BETWEEN ");
                    sqlString.Append(m_Start.ToString(CultureInfo.InvariantCulture));
                    sqlString.Append(" AND ");
                    sqlString.Append(m_End.ToString(CultureInfo.InvariantCulture));
                    break;
                case StackHashSearchOptionType.RangeExclusive:
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append(">");
                    sqlString.Append(m_Start.ToString(CultureInfo.InvariantCulture));
                    sqlString.Append(" AND ");
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append("<");
                    sqlString.Append(m_End.ToString(CultureInfo.InvariantCulture));
                    break;

                default:
                    throw new NotSupportedException("Unrecognized search option type");
            }

            if (SearchOptionType != StackHashSearchOptionType.All)
                sqlString.Append(")");

            return sqlString.ToString();
        }
    }

    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    [KnownType(typeof(StackHashSearchOption))]
    public class LongSearchOption : StackHashSearchOption
    {
        private long m_Start;
        private long m_End;

        public LongSearchOption()
        {
        }

        public LongSearchOption(StackHashObjectType objectType, String fieldName, StackHashSearchOptionType type, long start, long end)
            : base(StackHashSearchFieldType.Long, objectType, fieldName, type)
        {
            m_Start = start;
            m_End = end;
        }


        [DataMember]
        public long Start
        {
            get { return m_Start; }
            set { m_Start = value; }
        }

        [DataMember]
        public long End
        {
            get { return m_End; }
            set { m_End = value; }
        }


        public bool IsMatch(long value)
        {
            switch (SearchOptionType)
            {
                case StackHashSearchOptionType.All:
                    return true;
                case StackHashSearchOptionType.GreaterThan:
                    return (value > m_Start);
                case StackHashSearchOptionType.GreaterThanOrEqual:
                    return (value >= m_Start);
                case StackHashSearchOptionType.LessThan:
                    return (value < m_Start);
                case StackHashSearchOptionType.LessThanOrEqual:
                    return (value <= m_Start);
                case StackHashSearchOptionType.Equal:
                    return (value == m_Start);
                case StackHashSearchOptionType.NotEqual:
                    return (value != m_Start);
                case StackHashSearchOptionType.RangeInclusive:
                    return ((value >= m_Start) && (value <= m_End));
                case StackHashSearchOptionType.RangeExclusive:
                    return ((value > m_Start) && (value < m_End));

                default:
                    throw new NotSupportedException("Unrecognized search option type");
            }
        }


        public override String ToSqlString(String tableName)
        {
            StringBuilder sqlString = new StringBuilder(String.Empty);

            String correctedTableName = CorrectTableName(this.ObjectType, this.FieldName, tableName);

            if (SearchOptionType != StackHashSearchOptionType.All)
                sqlString.Append("(");
            switch (SearchOptionType)
            {
                case StackHashSearchOptionType.All:
                    break;
                case StackHashSearchOptionType.GreaterThan:
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append(">");
                    sqlString.Append(m_Start.ToString(CultureInfo.InvariantCulture));
                    break;
                case StackHashSearchOptionType.GreaterThanOrEqual:
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append(">=");
                    sqlString.Append(m_Start.ToString(CultureInfo.InvariantCulture));
                    break;
                case StackHashSearchOptionType.LessThan:
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append("<");
                    sqlString.Append(m_Start.ToString(CultureInfo.InvariantCulture));
                    break;
                case StackHashSearchOptionType.LessThanOrEqual:
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append("<=");
                    sqlString.Append(m_Start.ToString(CultureInfo.InvariantCulture));
                    break;
                case StackHashSearchOptionType.NotEqual:
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append("<>");
                    sqlString.Append(m_Start.ToString(CultureInfo.InvariantCulture));
                    break;
                case StackHashSearchOptionType.Equal:
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append("=");
                    sqlString.Append(m_Start.ToString(CultureInfo.InvariantCulture));
                    break;
                case StackHashSearchOptionType.RangeInclusive:
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append(" BETWEEN ");
                    sqlString.Append(m_Start.ToString(CultureInfo.InvariantCulture));
                    sqlString.Append(" AND ");
                    sqlString.Append(m_End.ToString(CultureInfo.InvariantCulture));
                    break;
                case StackHashSearchOptionType.RangeExclusive:
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append(">");
                    sqlString.Append(m_Start.ToString(CultureInfo.InvariantCulture));
                    sqlString.Append(" AND ");
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append("<");
                    sqlString.Append(m_End.ToString(CultureInfo.InvariantCulture));
                    break;

                default:
                    throw new NotSupportedException("Unrecognized search option type");
            }
            if (SearchOptionType != StackHashSearchOptionType.All)
                sqlString.Append(")");

            return sqlString.ToString();
        }
    }

    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    [KnownType(typeof(StackHashSearchOption))]
    public class DateTimeSearchOption : StackHashSearchOption
    {
        private DateTime m_Start;
        private DateTime m_End;


        public DateTimeSearchOption()
        {
        }

        public DateTimeSearchOption(StackHashObjectType objectType, String fieldName, StackHashSearchOptionType type, DateTime start, DateTime end)
            : base(StackHashSearchFieldType.DateTime, objectType, fieldName, type)
        {
            m_Start = start;
            m_End = end;
        }

        [DataMember]
        public DateTime Start
        {
            get { return m_Start; }
            set { m_Start = value; }
        }

        [DataMember]
        public DateTime End
        {
            get { return m_End; }
            set { m_End = value; }
        }


        public bool IsMatch(DateTime value)
        {
            DateTime start = m_Start.Date;
            DateTime end = m_End.Date;
            DateTime dateToTest = value.Date;

            switch (SearchOptionType)
            {
                case StackHashSearchOptionType.All:
                    return true;
                case StackHashSearchOptionType.GreaterThan:
                    return (dateToTest > start);
                case StackHashSearchOptionType.GreaterThanOrEqual:
                    return (dateToTest >= start);
                case StackHashSearchOptionType.LessThan:
                    return (dateToTest < start);
                case StackHashSearchOptionType.LessThanOrEqual:
                    return (dateToTest <= start);
                case StackHashSearchOptionType.Equal:
                    return (dateToTest == start);
                case StackHashSearchOptionType.NotEqual:
                    return (dateToTest != start);
                case StackHashSearchOptionType.RangeInclusive:
                    return ((dateToTest >= start) && (dateToTest <= end));
                case StackHashSearchOptionType.RangeExclusive:
                    return ((dateToTest > start) && (dateToTest < end));

                default:
                    throw new NotSupportedException("Unrecognized search option type");
            }
        }

        public override String ToSqlString(String tableName)
        {
            String correctedTableName = CorrectTableName(this.ObjectType, this.FieldName, tableName);

            // DateTime: SqlDateTime overflow. Must be between 1/1/1753 12:00:00 AM and 12/31/9999 11:59:59 PM
            // SmallDateTime: January 1, 1900, through June 6, 2079
            if (m_Start.Year < 1900)
                m_Start = m_Start.AddYears(1900 - m_Start.Year + 1);
            if (m_Start.Year > 2078)
                m_Start = m_Start.AddYears(-1 * (m_Start.Year - 2078 + 1));
            if (m_End.Year < 1900)
                m_End = m_End.AddYears(1900 - m_End.Year + 1);
            if (m_End.Year > 2078)
                m_End = m_End.AddYears(-1 * (m_End.Year - 2078 + 1));


            StringBuilder sqlString = new StringBuilder(String.Empty);
            String sqlStart = "'" + m_Start.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss", CultureInfo.InvariantCulture) + "'";
            String sqlEnd = "'" + m_End.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss", CultureInfo.InvariantCulture) + "'";


            if (SearchOptionType != StackHashSearchOptionType.All)
                sqlString.Append("(");
            switch (SearchOptionType)
            {
                case StackHashSearchOptionType.All:
                    break;
                case StackHashSearchOptionType.GreaterThan:
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append(">");
                    sqlString.Append(sqlStart);
                    break;
                case StackHashSearchOptionType.GreaterThanOrEqual:
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append(">=");
                    sqlString.Append(sqlStart);
                    break;
                case StackHashSearchOptionType.LessThan:
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append("<");
                    sqlString.Append(sqlStart);
                    break;
                case StackHashSearchOptionType.LessThanOrEqual:
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append("<=");
                    sqlString.Append(sqlStart);
                    break;
                case StackHashSearchOptionType.NotEqual:
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append("<>");
                    sqlString.Append(sqlStart);
                    break;
                case StackHashSearchOptionType.Equal:
                    DateTime endTime = m_Start.AddDays(1);

                    String newSqlEnd = "'" + endTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss", CultureInfo.InvariantCulture) + "'";
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append(">=");
                    sqlString.Append(sqlStart);
                    sqlString.Append(" AND ");
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append("<");
                    sqlString.Append(newSqlEnd);
                    break;
                case StackHashSearchOptionType.RangeInclusive:
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append(" BETWEEN ");
                    sqlString.Append(sqlStart);
                    sqlString.Append(" AND ");
                    sqlString.Append(sqlEnd);
                    break;
                case StackHashSearchOptionType.RangeExclusive:
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append(">");
                    sqlString.Append(sqlStart);
                    sqlString.Append(" AND ");
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append("<");
                    sqlString.Append(sqlEnd);
                    break;

                default:
                    throw new NotSupportedException("Unrecognized search option type");
            }
            if (SearchOptionType != StackHashSearchOptionType.All)
                sqlString.Append(")");

            return sqlString.ToString();
        }
    }
    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    [KnownType(typeof(StackHashSearchOption))]
    public class StringSearchOption : StackHashSearchOption
    {
        private String m_Start;
        private String m_End;
        private bool m_CaseSensitive;

        public StringSearchOption()
        {
        }

        public StringSearchOption(StackHashObjectType objectType, String fieldName, StackHashSearchOptionType type, String start, String end, bool caseSensitive)
            : base(StackHashSearchFieldType.String, objectType, fieldName, type)
        {
            m_Start = start;
            m_End = end;
            m_CaseSensitive = caseSensitive;
        }

        [DataMember]
        public String Start
        {
            get { return m_Start; }
            set { m_Start = value; }
        }

        [DataMember]
        public String End
        {
            get { return m_End; }
            set { m_End = value; }
        }

        [DataMember]
        public bool CaseSensitive
        {
            get { return m_CaseSensitive; }
            set { m_CaseSensitive = value; }
        }


        public bool IsMatch(String value)
        {
            if (value == null)
                return false;

            int result = 0;
            int result2 = 0;

            if ((SearchOptionType != StackHashSearchOptionType.All) &&
                (value == null))
                return false;

            StringComparison comparisonType = m_CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            switch (SearchOptionType)
            {
                case StackHashSearchOptionType.All:
                    return true;
                case StackHashSearchOptionType.GreaterThan:
                    result = string.Compare(value, m_Start, comparisonType);
                    return (result > 0);
                case StackHashSearchOptionType.GreaterThanOrEqual:
                    result = string.Compare(value, m_Start, comparisonType);
                    return (result >= 0);
                case StackHashSearchOptionType.LessThan:
                    result = string.Compare(value, m_Start, comparisonType);
                    return (result < 0);
                case StackHashSearchOptionType.LessThanOrEqual:
                    result = string.Compare(value, m_Start, comparisonType);
                    return (result <= 0);
                case StackHashSearchOptionType.NotEqual:
                    result = string.Compare(value, m_Start, comparisonType);
                    return (result != 0);
                case StackHashSearchOptionType.Equal:
                    result = string.Compare(value, m_Start, comparisonType);
                    return (result == 0);
                case StackHashSearchOptionType.RangeInclusive:
                    result = string.Compare(value, m_Start, comparisonType);
                    result2 = string.Compare(value, m_End, comparisonType);
                    return ((result > 0) && (result2 < 0));
                case StackHashSearchOptionType.RangeExclusive:
                    result = string.Compare(value, m_Start, comparisonType);
                    result2 = string.Compare(value, m_End, comparisonType);
                    return ((result >= 0) && (result2 <= 0));
                case StackHashSearchOptionType.StringStartsWith:
                    return (value.StartsWith(m_Start, comparisonType));
                case StackHashSearchOptionType.StringContains:
                    if (m_CaseSensitive)
                    {
                        return (value.Contains(m_Start));
                    }
                    else
                    {
                        String string1 = m_Start.ToUpperInvariant();
                        String string2 = value.ToUpperInvariant();
                        
                        // Convert both strings to upper case before 
                        return (string2.Contains(string1));
                    }

                default:
                    throw new NotSupportedException("Unrecognized search option type");
            }
        }

        public override String ToSqlString(String tableName)
        {
            StringBuilder sqlString = new StringBuilder(String.Empty);

            String correctedTableName = CorrectTableName(this.ObjectType, this.FieldName, tableName);

            if (SearchOptionType != StackHashSearchOptionType.All)
                sqlString.Append("(");
            switch (SearchOptionType)
            {
                case StackHashSearchOptionType.All:
                    break;
                case StackHashSearchOptionType.GreaterThan:
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append("> N'");
                    sqlString.Append(m_Start.ToString(CultureInfo.InvariantCulture));
                    sqlString.Append("'");
                    break;
                case StackHashSearchOptionType.GreaterThanOrEqual:
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append(">= N'");
                    sqlString.Append(m_Start.ToString(CultureInfo.InvariantCulture));
                    sqlString.Append("'");
                    break;
                case StackHashSearchOptionType.LessThan:
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append("< N'");
                    sqlString.Append(m_Start.ToString(CultureInfo.InvariantCulture));
                    sqlString.Append("'");
                    break;
                case StackHashSearchOptionType.LessThanOrEqual:
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append("<= N'");
                    sqlString.Append(m_Start.ToString(CultureInfo.InvariantCulture));
                    sqlString.Append("'");
                    break;
                case StackHashSearchOptionType.NotEqual:
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append("<> N'");
                    sqlString.Append(m_Start.ToString(CultureInfo.InvariantCulture));
                    sqlString.Append("'");
                    break;
                case StackHashSearchOptionType.Equal:
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append("= N'");
                    sqlString.Append(m_Start.ToString(CultureInfo.InvariantCulture));
                    sqlString.Append("'");
                    break;
                case StackHashSearchOptionType.RangeInclusive:
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append(" BETWEEN N'");
                    sqlString.Append(m_Start.ToString(CultureInfo.InvariantCulture));
                    sqlString.Append("' AND N'");
                    sqlString.Append(m_End.ToString(CultureInfo.InvariantCulture));
                    sqlString.Append("'");
                    break;
                case StackHashSearchOptionType.RangeExclusive:
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append("> N'");
                    sqlString.Append(m_Start.ToString(CultureInfo.InvariantCulture));
                    sqlString.Append("' AND ");
                    sqlString.Append(correctedTableName);
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append("< N'");
                    sqlString.Append(m_End.ToString(CultureInfo.InvariantCulture));
                    sqlString.Append("'");
                    break;

                case StackHashSearchOptionType.StringStartsWith:
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append(" LIKE N'");
                    sqlString.Append(m_Start.ToString(CultureInfo.InvariantCulture));
                    sqlString.Append("%'");
                    break;

                case StackHashSearchOptionType.StringContains:
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append(" LIKE N'%");
                    sqlString.Append(m_Start.ToString(CultureInfo.InvariantCulture));
                    sqlString.Append("%'");
                    break;

                case StackHashSearchOptionType.StringDoesNotContain:
                    // Must allow for NULL too.
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append(" NOT LIKE N'%");
                    sqlString.Append(m_Start.ToString(CultureInfo.InvariantCulture));
                    sqlString.Append("%' OR ");
                    sqlString.Append(correctedTableName);
                    sqlString.Append('.');
                    sqlString.Append(ConvertFieldName(ObjectType, base.FieldName));
                    sqlString.Append(" IS NULL");
                    break;

                default:
                    throw new NotSupportedException("Unrecognized search option type");
            }
            if (SearchOptionType != StackHashSearchOptionType.All)
                sqlString.Append(")");

            return sqlString.ToString();
        }
    }

    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashSearchOptionCollection : Collection<StackHashSearchOption>
    {
        public StackHashSearchOptionCollection() { } // Needed to serialize.
    }


    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashSearchCriteria
    {
        StackHashSearchOptionCollection m_SearchFieldOptions = new StackHashSearchOptionCollection();

        [DataMember]
        public StackHashSearchOptionCollection SearchFieldOptions
        {
            get { return m_SearchFieldOptions; }
            set { m_SearchFieldOptions = value; }
        }

        public StackHashSearchCriteria() { } // Needed to serialize.
        public StackHashSearchCriteria(StackHashSearchOptionCollection searchOptions)
        {
            m_SearchFieldOptions = searchOptions;
        }

        public bool IncludeAll(StackHashObjectType objectType)
        {
            bool all = true;
            foreach (StackHashSearchOption searchFieldOption in m_SearchFieldOptions)
            {
                if ((searchFieldOption.ObjectType == objectType) && 
                    (searchFieldOption.SearchOptionType != StackHashSearchOptionType.All))
                {
                    all = false;
                }
            }
            return all;
        }

        public bool ContainsObject(StackHashObjectType objectType)
        {
            foreach (StackHashSearchOption searchFieldOption in m_SearchFieldOptions)
            {
                if (searchFieldOption.ObjectType == objectType)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Works through correcting any object errors.
        /// e.g. the Event.WorkFlowStatusName should be specified as the Object.WorkFlow instead of Event.
        /// </summary>
        /// <returns>Number of changes made.</returns>
        public int CorrectObjects()
        {
            int numChanges = 0;

            foreach (StackHashSearchOption searchFieldOption in m_SearchFieldOptions)
            {
                if ((searchFieldOption.ObjectType == StackHashObjectType.Event) &&
                    (String.Compare(searchFieldOption.FieldName, "WorkFlowStatusName", StringComparison.OrdinalIgnoreCase) == 0))
                {
                    searchFieldOption.ObjectType = StackHashObjectType.EventWorkFlow;
                    numChanges++;
                }
            }

            return numChanges;
        }

        
        public int ObjectCount(StackHashObjectType objectType)
        {
            int totalObjects = 0;

            foreach (StackHashSearchOption searchFieldOption in m_SearchFieldOptions)
            {
                if (searchFieldOption.ObjectType == objectType)
                {
                    totalObjects++;
                }
            }
            return totalObjects;
        }

        
        private static Type getPropertyType(Type searchOptionType)
        {
            if (searchOptionType == typeof(IntSearchOption))
                return typeof(int);
            else if (searchOptionType == typeof(StringSearchOption))
                return typeof(string);
            else if (searchOptionType == typeof(LongSearchOption))
                return typeof(long);
            else if (searchOptionType == typeof(DateTimeSearchOption))
                return typeof(DateTime);
            else
                throw new InvalidOperationException("Unrecognized type");
        }


        /// <summary>
        /// Determines if the specified object matches any of the listed criteria.
        /// </summary>
        /// <param name="objectType">The type of the object being checked.</param>
        /// <param name="theObject">The object itself.</param>
        /// <returns>true if a match, false otherwise.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1720")]
        public bool IsMatch(StackHashObjectType objectType, Object theObject)
        {
            if (theObject == null)
                throw new ArgumentNullException("theObject");

            // Run through the different options and make sure each is a match.
            // There must be an item matching the type and ALL such items must match.
            bool matchFound = true;           
            bool atLeastOneCriteriaFound = false;

            foreach (StackHashSearchOption searchFieldOption in m_SearchFieldOptions)
            {
                if (searchFieldOption.ObjectType == objectType)
                {
                    atLeastOneCriteriaFound = true;


                    // Check for a wildcard. In this case if ANY field matches then this is counted as a match.
                    if (searchFieldOption.FieldName == "*")
                    {
                        // Get a list of all properties.
                        PropertyInfo[] properties = theObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

                        int numMatches = 0;

                        foreach (PropertyInfo pi in properties)
                        {
                            // Check that the field is the correct type.
                            if (pi.PropertyType != getPropertyType(searchFieldOption.GetType()))
                                continue;
                            if (pi.Name == "StructureVersion")
                                continue;
                            if (pi.Name == "ThisStructureVersion")
                                continue;

                            // Now read the property value.
                            object fieldValue = pi.GetValue(theObject, null);

                            // Get the IsMatch method and call it with the property value.
                            Type searchFieldType = searchFieldOption.GetType();
                            MethodInfo isMatchMethodInfo = searchFieldType.GetMethod("IsMatch");

                            bool result = (bool)isMatchMethodInfo.Invoke(searchFieldOption, new object[] { fieldValue });

                            if (result)
                                numMatches++;
                        }
                        if (numMatches == 0)
                            matchFound = false;
                    }
                    else
                    {
                        // Get the property information for this field - must exist.
                        PropertyInfo pi = theObject.GetType().GetProperty(searchFieldOption.FieldName,
                            BindingFlags.Public | BindingFlags.Instance);

                        if (pi == null)
                            throw new InvalidOperationException("Field not found in object: " +
                                searchFieldOption.FieldName.ToString(CultureInfo.CurrentCulture));

                        // Now read the property value.
                        object fieldValue = pi.GetValue(theObject, null);

                        // Get the IsMatch method and call it with the property value.
                        Type searchFieldType = searchFieldOption.GetType();
                        MethodInfo isMatchMethodInfo = searchFieldType.GetMethod("IsMatch");
                        bool result = (bool)isMatchMethodInfo.Invoke(searchFieldOption, new object[] { fieldValue });

                        if (!result)
                            matchFound = false;
                    }
                }
            }

            return (atLeastOneCriteriaFound && matchFound);
        }


        /// <summary>
        /// Converts a criteria to a string suitable for an SQL predicate.
        /// </summary>
        /// <param name="objectType">The type of the object to analyse.</param>
        /// <param name="tableName">Initial table name.</param>
        /// <returns></returns>
        public String ToSqlString(StackHashObjectType objectType, String tableName)
        {
            StringBuilder searchString = new StringBuilder();
            bool firstOption = true;

            bool addBrackets = (this.ObjectCount(objectType) > 1);

            if (addBrackets)
                searchString.Append("(");

            foreach (StackHashSearchOption option in this.SearchFieldOptions)
            {
                if (option.ObjectType != objectType)
                    continue;

                // Ignore invalid field names.
                if (!StackHashSearchOption.IsValidSqlFieldName(objectType, option.FieldName))
                    continue;

                if (!firstOption)
                    searchString.Append(" AND ");
                else
                    firstOption = false;

                // For windcards, add every field to an (A OR B OR ... structure)
                if (option.FieldName == "*")
                {
                    StringBuilder allFields = new StringBuilder();
                    Collection<String> fieldNames = StackHashSearchOption.GetFieldsInObjectOfSpecifiedType(option.ObjectType, getPropertyType(option.GetType()));

                    if (fieldNames.Count > 1)
                        allFields.Append("(");

                    try
                    {
                        bool firstWildcardField = true;
                        foreach (String fieldName in fieldNames)
                        {
                            if (!firstWildcardField)
                                allFields.Append(" OR ");
                            else
                                firstWildcardField = false;

                            option.FieldName = fieldName;
                            allFields.Append(option.ToSqlString(tableName));
                        }
                    }
                    finally
                    {
                        option.FieldName = "*";
                    }

                    if (fieldNames.Count > 1)
                        allFields.Append(")");

                    searchString.Append(allFields);
                }
                else
                {
                    searchString.Append(option.ToSqlString(tableName));
                }
            }

            if (addBrackets)
                searchString.Append(")");

            return searchString.ToString();
        }
    }


    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashSearchCriteriaCollection : Collection<StackHashSearchCriteria>
    {
        public StackHashSearchCriteriaCollection() { } // Needed to serialize.

        public int ObjectCount(StackHashObjectType objectType)
        {
            int count = 0;

            foreach (StackHashSearchCriteria criteria in this)
            {
                if (criteria.ContainsObject(objectType))
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Works through correcting any object errors.
        /// e.g. the Event.WorkFlowStatusName should be specified as the Object.WorkFlow instead of Event.
        /// </summary>
        /// <returns>Number of changes made.</returns>
        public int CorrectObjects()
        {
            int numChanges = 0;

            foreach (StackHashSearchCriteria criteria in this)
            {
                numChanges += criteria.CorrectObjects();
            }

            return numChanges;
        }

        public String ToSqlString(StackHashObjectType objectType, String tableName)
        {
            StringBuilder searchString = new StringBuilder();
            bool firstCriteria = true;
            bool addBrackets = false;


            addBrackets = (this.ObjectCount(objectType) > 1);

            if (addBrackets)
                searchString.Append("(");

            foreach (StackHashSearchCriteria criteria in this)
            {
                if (!criteria.ContainsObject(objectType))
                    continue;

                if (!firstCriteria)
                    searchString.Append(" OR ");
                else
                    firstCriteria = false;

                searchString.Append(criteria.ToSqlString(objectType, tableName));
            }

            if (addBrackets)
                searchString.Append(")");

            return searchString.ToString();
        }
    }


    [Serializable]
    [DataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17")]
    public class StackHashSortOrder
    {
        private StackHashObjectType m_ObjectType;
        private String m_FieldName;
        private bool m_Ascending;

        public StackHashSortOrder()
        {
        }

        public StackHashSortOrder(StackHashObjectType objectType, String fieldName, bool ascending)
        {
            m_ObjectType = objectType;
            m_FieldName = fieldName;
            m_Ascending = ascending;
        }

        [DataMember]
        public StackHashObjectType ObjectType
        {
            get { return m_ObjectType; }
            set { m_ObjectType = value; }
        }

        
        [DataMember]
        public String FieldName
        {
            get { return m_FieldName; }
            set { m_FieldName = value; }
        }

        [DataMember]
        public bool Ascending
        {
            get { return m_Ascending; }
            set { m_Ascending = value; }
        }
    }


    [Serializable]
    [CollectionDataContract(Namespace = "http://schemas.cucku.com/stackhash/2010/02/17", ItemName = "Item")]
    public class StackHashSortOrderCollection : Collection<StackHashSortOrder>
    {
        public StackHashSortOrderCollection() { } // Needed to serialize.

        public bool Validate()
        {
            if (this.Count == 0)
                return false;

            foreach (StackHashSortOrder sortOrder in this)
            {
                if ((sortOrder.ObjectType != StackHashObjectType.Event) &&
                    (sortOrder.ObjectType != StackHashObjectType.EventSignature) &&
                    (sortOrder.ObjectType != StackHashObjectType.EventWorkFlow))
                {
                    return false;
                }
            }

            return true;
        }

        public int Correct()
        {
            int numCorrections = 0;

            foreach (StackHashSortOrder sortOrder in this)
            {
                if ((sortOrder.ObjectType == StackHashObjectType.Event) &&
                    (String.Compare(sortOrder.FieldName, "WorkFlowStatusName", StringComparison.OrdinalIgnoreCase) == 0))
                {
                    sortOrder.ObjectType = StackHashObjectType.EventWorkFlow;
                    numCorrections++;
                }
            }

            return numCorrections;
        }

        public String ToSqlString(String tableName, bool doNotTranslateTableName)
        {
            StringBuilder sqlString = new StringBuilder();
            bool isFirstOrderField = true;

            foreach (StackHashSortOrder sortOrder in this)
            {
                if (!StackHashSearchOption.IsValidSqlFieldName(sortOrder.ObjectType, sortOrder.FieldName))
                    continue;

                if (!isFirstOrderField)
                {
                    sqlString.Append(",");
                }

                isFirstOrderField = false;

                // Add the table name.
                if (doNotTranslateTableName)
                {
                    sqlString.Append(tableName);
                }
                else
                {
                    String newTableName = StackHashSearchOption.CorrectTableName(
                        sortOrder.ObjectType, sortOrder.FieldName, StackHashSearchOption.GetTableName(sortOrder.ObjectType));
                    sqlString.Append(newTableName);
                }
                sqlString.Append(".");

                // Add the field name.
                sqlString.Append(StackHashSearchOption.ConvertFieldName(sortOrder.ObjectType, sortOrder.FieldName));
 
                // Add the operator to determine the sort order.
                if (sortOrder.Ascending)
                    sqlString.Append(" ASC ");
                else
                    sqlString.Append(" DESC ");
            }
            return sqlString.ToString();
        }
    }


}
