namespace ETW2SQLite
{
    using System;
    using System.Runtime.InteropServices;

    internal unsafe struct sqlite3
    {
    }

    internal unsafe struct sqlite3_stmt
    {
    }

    internal static unsafe class NativeMethods
    {
        internal static IntPtr Transient = new IntPtr(-1);

        [DllImport("sqlite3", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_open16([MarshalAs(UnmanagedType.LPWStr)] string filename, out sqlite3* ppDb);

        [DllImport("sqlite3", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_close(sqlite3* db);

        [DllImport("sqlite3", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_prepare(
            sqlite3* db, /* Database handle */
            string zSql, /* SQL statement, UTF-8 encoded */
            int nByte, /* Maximum length of zSql in bytes. */
            out sqlite3_stmt* ppStmt, /* OUT: Statement handle */
            out char* pzTail /* OUT: Pointer to unused portion of zSql */
            );

        [DllImport("sqlite3", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_prepare_v2(
            sqlite3* db, /* Database handle */
            string zSql, /* SQL statement, UTF-8 encoded */
            int nByte, /* Maximum length of zSql in bytes. */
            out sqlite3_stmt* ppStmt, /* OUT: Statement handle */
            out char* pzTail /* OUT: Pointer to unused portion of zSql */
            );

        [DllImport("sqlite3", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_exec(
            sqlite3* db, /* An open database */
            string sql, /* SQL to be evaluated */
            IntPtr callback, /* Callback function */
            IntPtr firstArg, /* 1st argument to callback */
            IntPtr errmsg /* Error msg written here */
            );

        [DllImport("sqlite3", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_step(sqlite3_stmt* pStmt);

        [DllImport("sqlite3", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_reset(sqlite3_stmt* pStmt);

        [DllImport("sqlite3", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_finalize(sqlite3_stmt* pStmt);

        [DllImport("sqlite3", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_bind_blob(sqlite3_stmt* pStmt, int index, IntPtr blob, int n, IntPtr @static);

        [DllImport("sqlite3", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_bind_double(sqlite3_stmt* pStmt, int index, double value);

        [DllImport("sqlite3", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_bind_int(sqlite3_stmt* pStmt, int index, int value);

        [DllImport("sqlite3", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_bind_int64(sqlite3_stmt* pStmt, int index, long value);

        [DllImport("sqlite3", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_bind_null(sqlite3_stmt* pStmt, int index);

        [DllImport("sqlite3", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_bind_text(sqlite3_stmt* pStmt, int index, string value, int n, IntPtr @static);

        [DllImport("sqlite3", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_bind_text16(sqlite3_stmt* pStmt, int index, [MarshalAs(UnmanagedType.LPWStr)] string value, int n, IntPtr @static);

        [DllImport("sqlite3", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_bind_zeroblob(sqlite3_stmt* pStmt, int index, int n);
    }
}