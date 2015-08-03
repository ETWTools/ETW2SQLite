namespace ETW2SQLite
{
    using System;
    using ETWDeserializer;

    internal static class Extensions
    {
        public static string SQLiteType(this TDH_IN_TYPE tdhType)
        {
            switch (tdhType)
            {
                case TDH_IN_TYPE.TDH_INTYPE_UNICODESTRING:
                    return "TEXT";
                case TDH_IN_TYPE.TDH_INTYPE_ANSISTRING:
                    return "TEXT";
                case TDH_IN_TYPE.TDH_INTYPE_INT8:
                    return "TINYINT";
                case TDH_IN_TYPE.TDH_INTYPE_UINT8:
                    return "TINYINT";
                case TDH_IN_TYPE.TDH_INTYPE_INT16:
                    return "SMALLINT";
                case TDH_IN_TYPE.TDH_INTYPE_UINT16:
                    return "SMALLINT";
                case TDH_IN_TYPE.TDH_INTYPE_INT32:
                    return "INTEGER";
                case TDH_IN_TYPE.TDH_INTYPE_UINT32:
                    return "INTEGER";
                case TDH_IN_TYPE.TDH_INTYPE_INT64:
                    return "BIGINT";
                case TDH_IN_TYPE.TDH_INTYPE_UINT64:
                    return "UNSIGNED BIG INT";
                case TDH_IN_TYPE.TDH_INTYPE_FLOAT:
                    return "FLOAT";
                case TDH_IN_TYPE.TDH_INTYPE_DOUBLE:
                    return "DOUBLE";
                case TDH_IN_TYPE.TDH_INTYPE_BOOLEAN:
                    return "BOOLEAN";
                case TDH_IN_TYPE.TDH_INTYPE_BINARY:
                    return "BLOB";
                case TDH_IN_TYPE.TDH_INTYPE_GUID:
                    return "TEXT";
                case TDH_IN_TYPE.TDH_INTYPE_POINTER:
                    return "BIGINT";
                case TDH_IN_TYPE.TDH_INTYPE_FILETIME:
                    return "TEXT";
                case TDH_IN_TYPE.TDH_INTYPE_SYSTEMTIME:
                    return "TEXT";
                case TDH_IN_TYPE.TDH_INTYPE_SID:
                    return "TEXT";
                case TDH_IN_TYPE.TDH_INTYPE_HEXINT32:
                    return "INTEGER";
                case TDH_IN_TYPE.TDH_INTYPE_HEXINT64:
                    return "BIGINT";
                case TDH_IN_TYPE.TDH_INTYPE_COUNTEDSTRING:
                    return "TEXT";
                case TDH_IN_TYPE.TDH_INTYPE_COUNTEDANSISTRING:
                    return "TEXT";
                case TDH_IN_TYPE.TDH_INTYPE_REVERSEDCOUNTEDSTRING:
                    return "TEXT";
                case TDH_IN_TYPE.TDH_INTYPE_REVERSEDCOUNTEDANSISTRING:
                    return "TEXT";
                case TDH_IN_TYPE.TDH_INTYPE_NONNULLTERMINATEDSTRING:
                    return "TEXT";
                case TDH_IN_TYPE.TDH_INTYPE_NONNULLTERMINATEDANSISTRING:
                    return "TEXT";
                case TDH_IN_TYPE.TDH_INTYPE_UNICODECHAR:
                    return "INTEGER";
                case TDH_IN_TYPE.TDH_INTYPE_ANSICHAR:
                    return "INTEGER";
                case TDH_IN_TYPE.TDH_INTYPE_SIZET:
                    return "BIGINT";
                case TDH_IN_TYPE.TDH_INTYPE_HEXDUMP:
                    return "CLOB";
                case TDH_IN_TYPE.TDH_INTYPE_WBEMSID:
                    return "TEXT";
                default:
                    throw new Exception("Unreachable");
            }
        }
    }
}