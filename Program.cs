namespace ETW2SQLite
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using ETWDeserializer;

    public class Program
    {
        private static string Usage = "Usage: ETW2SQLite filename.etl [filename2.etl] -output=filename.sqlite";

        static unsafe void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine(Usage);
                return;
            }

            var commandLineInfo = new CommandLineInfo(args);
            if (!commandLineInfo.Initialize())
            {
                return;
            }

            var fileName = commandLineInfo.Output;

            if (File.Exists(fileName))
            {
                Console.WriteLine(fileName + " already exists, please delete the file and try again, or change the output filename");
                return;
            }

            sqlite3* db;
            int error;
            if ((error = NativeMethods.sqlite3_open16(fileName, out db)) != 0)
            {
                Console.WriteLine("sqlite3_open16 failed with error code: " + error);
            }

            var sqlWriter = new EtwSQLiteWriter(db);
            var deserializer = new Deserializer<EtwSQLiteWriter>(sqlWriter);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            var inputs = commandLineInfo.Inputs;

            int count = inputs.Count;

            var fileSessions = new EVENT_TRACE_LOGFILEW[inputs.Count];
            var handles = new ulong[count];

            DateTime minStartTime = DateTime.MaxValue;
            DateTime maxEndTime = DateTime.MinValue;

            for (int i = 0; i < count; ++i)
            {
                fileSessions[i] = new EVENT_TRACE_LOGFILEW
                {
                    LogFileName = inputs[i],
                    EventRecordCallback = deserializer.Deserialize,
                    BufferCallback = deserializer.BufferCallback,
                    LogFileMode = Etw.PROCESS_TRACE_MODE_EVENT_RECORD | Etw.PROCESS_TRACE_MODE_RAW_TIMESTAMP
                };

                handles[i] = Etw.OpenTrace(ref fileSessions[i]);
                minStartTime = MinDate(minStartTime, DateTime.FromFileTime(fileSessions[i].LogfileHeader.StartTime));
                maxEndTime = MaxDate(maxEndTime, DateTime.FromFileTime(fileSessions[i].LogfileHeader.StartTime));
            }

            sqlite3_stmt* createTableStmt;
            char* tail;

            var columnNames = @"(LogFileName TEXT, LoggerName TEXT, StartTime TEXT, EndTime TEXT, BootTime Text, EventsLost INTEGER, CpuSpeedInMHz DOUBLE, PerfFreq DOUBLE, NumberOfProcessors INTEGER, PointerSize INTEGER, BufferSize INTEGER, BuffersRead INTEGER, BuffersLost INTEGER, BuffersWritten INTEGER, LogFileMode INTEGER, MaximumFileSize INTEGER, ReservedFlags INTEGER, INTEGER TimerResolution, INTEGER CurrentTime, INTEGER Filled, INTEGER IsKernelTrace)";

            var createSql = @"CREATE TABLE EventInfo " + columnNames;
            NativeMethods.sqlite3_prepare(db, createSql, createSql.Length, out createTableStmt, out tail);

            NativeMethods.sqlite3_step(createTableStmt);
            NativeMethods.sqlite3_finalize(createTableStmt);

            sqlite3_stmt* eventStmt;

            string eventInfoTableSql =
                @"INSERT INTO EventInfo " + columnNames + " VALUES (" +
                "@1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11, @12, @13, @14, @15, @16, @17, @18, @19, @20, @21) ";
            NativeMethods.sqlite3_prepare(db, eventInfoTableSql, eventInfoTableSql.Length, out eventStmt, out tail);

            NativeMethods.sqlite3_step(eventStmt);
            NativeMethods.sqlite3_finalize(eventStmt);
            
            for (int i = 0; i < handles.Length; ++i)
            {
                unchecked
                {
                    if (handles[i] == (ulong)(~0))
                    {
                        switch (Marshal.GetLastWin32Error())
                        {
                        case 0x57:
                            Console.WriteLine("Error: For file: " + inputs[i] + " Windows returned 0x57 -- The Logfile parameter is NULL.");
                            return;
                        case 0xA1:
                            Console.WriteLine("Error: For file: " + inputs[i] + " Windows returned 0xA1 -- The specified path is invalid.");
                            return;
                        case 0x5:
                            Console.WriteLine("Error: For file: " + inputs[i] + " Windows returned 0x5 -- Access is denied.");
                            return;
                        default:
                            Console.WriteLine("Error: For file: " + inputs[i] + " Windows returned an unknown error.");
                            return;
                        }
                    }
                }

                var currentSession = fileSessions[i];

                var logFileName = currentSession.LogFileName ?? string.Empty;
                NativeMethods.sqlite3_bind_text(eventStmt, 1, logFileName, logFileName.Length, NativeMethods.Transient);

                var loggerName = currentSession.LoggerName ?? string.Empty;
                NativeMethods.sqlite3_bind_text(eventStmt, 2, loggerName, loggerName.Length, NativeMethods.Transient);

                var startDateStr = DateTime.FromFileTime(currentSession.LogfileHeader.StartTime).ToString(@"yyyy\-MM\-dd HH\:mm\:ss.FFFFFFF");
                NativeMethods.sqlite3_bind_text(eventStmt, 3, startDateStr, startDateStr.Length, NativeMethods.Transient);

                var endDateStr = DateTime.FromFileTime(currentSession.LogfileHeader.EndTime).ToString(@"yyyy\-MM\-dd HH\:mm\:ss.FFFFFFF");
                NativeMethods.sqlite3_bind_text(eventStmt, 4, endDateStr, endDateStr.Length, NativeMethods.Transient);

                var bootDateStr = DateTime.FromFileTime(currentSession.LogfileHeader.BootTime).ToString(@"yyyy\-MM\-dd HH\:mm\:ss.FFFFFFF");
                NativeMethods.sqlite3_bind_text(eventStmt, 5, bootDateStr, bootDateStr.Length, NativeMethods.Transient);

                NativeMethods.sqlite3_bind_int64(eventStmt, 6, currentSession.LogfileHeader.EventsLost);

                NativeMethods.sqlite3_bind_int64(eventStmt, 7, currentSession.LogfileHeader.CpuSpeedInMHz);
                NativeMethods.sqlite3_bind_int64(eventStmt, 8, currentSession.LogfileHeader.PerfFreq);
                NativeMethods.sqlite3_bind_int64(eventStmt, 9, currentSession.LogfileHeader.NumberOfProcessors);

                NativeMethods.sqlite3_bind_int64(eventStmt, 10, currentSession.LogfileHeader.PointerSize);

                NativeMethods.sqlite3_bind_int64(eventStmt, 11, currentSession.BufferSize);
                NativeMethods.sqlite3_bind_int64(eventStmt, 12, currentSession.BuffersRead);
                NativeMethods.sqlite3_bind_int64(eventStmt, 13, currentSession.LogfileHeader.BuffersLost);
                NativeMethods.sqlite3_bind_int64(eventStmt, 14, currentSession.LogfileHeader.BuffersWritten);

                NativeMethods.sqlite3_bind_int64(eventStmt, 15, currentSession.LogfileHeader.LogFileMode);
                NativeMethods.sqlite3_bind_int64(eventStmt, 16, currentSession.LogfileHeader.MaximumFileSize);

                NativeMethods.sqlite3_bind_int64(eventStmt, 17, currentSession.LogfileHeader.ReservedFlags);
                NativeMethods.sqlite3_bind_int64(eventStmt, 18, currentSession.LogfileHeader.TimerResolution);

                NativeMethods.sqlite3_bind_int64(eventStmt, 19, currentSession.CurrentTime);
                NativeMethods.sqlite3_bind_int64(eventStmt, 20, currentSession.Filled);
                NativeMethods.sqlite3_bind_int64(eventStmt, 21, currentSession.IsKernelTrace);

                NativeMethods.sqlite3_step(eventStmt);
                NativeMethods.sqlite3_reset(eventStmt);
            }

            NativeMethods.sqlite3_finalize(eventStmt);

            NativeMethods.sqlite3_exec(db, "BEGIN TRANSACTION;", IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            Etw.ProcessTrace(handles, (uint)handles.Length, IntPtr.Zero, IntPtr.Zero);
            sqlWriter.Dispose();

            NativeMethods.sqlite3_exec(db, "END TRANSACTION;", IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            NativeMethods.sqlite3_close(db);

            watch.Stop();

            Console.WriteLine("Processing Time: " + watch.ElapsedMilliseconds + " milliseconds");

            GC.KeepAlive(fileSessions);
        }

        private static DateTime MaxDate(DateTime first, DateTime second)
        {
            if (Comparer<DateTime>.Default.Compare(first, second) > 0)
            {
                return first;
            }

            return second;
        }

        private static DateTime MinDate(DateTime first, DateTime second)
        {
            if (Comparer<DateTime>.Default.Compare(first, second) > 0)
            {
                return first;
            }

            return second;
        }

        private sealed class CommandLineInfo
        {
            private readonly string[] args;

            public string Output;

            public List<string> Inputs;

            public CommandLineInfo(string[] args)
            {
                this.args = args;
            }

            public bool Initialize()
            {
                bool outputSet = false;
                this.Inputs = new List<string>();
                foreach (var arg in this.args)
                {
                    if (arg.StartsWith("-output") || arg.StartsWith("--output") || arg.StartsWith("/output"))
                    {
                        var s = arg.Split('=');
                        if (s.Length < 2)
                        {
                            Console.WriteLine("ERROR: Encountered incorrect formatting for output filename");
                            Console.WriteLine(Usage);
                            return false;
                        }
                        else
                        {
                            if (outputSet)
                            {
                                Console.WriteLine("ERROR: Encountered incorrect formatting for output filename");
                                Console.WriteLine(Usage);
                                return false;
                            }

                            outputSet = true;
                            this.Output = s[1];
                        }
                    }
                    else
                    {
                        this.Inputs.Add(arg);
                    }
                }

                if (string.IsNullOrEmpty(this.Output))
                {
                    Console.WriteLine("ERROR: Encountered incorrect formatting for output filename");
                    Console.WriteLine(Usage);
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
    }
}