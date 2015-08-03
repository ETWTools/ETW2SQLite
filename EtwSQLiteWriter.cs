namespace ETW2SQLite
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;
    using ETWDeserializer;
    using Newtonsoft.Json;

    public unsafe sealed class EtwSQLiteWriter : IEtwWriter, IDisposable
    {
        private readonly StringBuilder builderBuffer;

        private readonly HashSet<string> tableNames;

        private readonly sqlite3* db;

        private readonly Dictionary<EventMetadata, long> commandTable;
        
        private sqlite3_stmt* currentStatement;

        private int currentParameterIndex;

        private StringWriter buffer;

        private JsonWriter writer;

        private int structDepth;

        private int arrayDepth;

        internal EtwSQLiteWriter(sqlite3* db)
        {
            this.db = db;
            this.commandTable = new Dictionary<EventMetadata, long>();
            this.tableNames = new HashSet<string>();
            this.builderBuffer = new StringBuilder(64 * 1024); // max event size
            this.buffer = new StringWriter(this.builderBuffer);
            this.writer = new JsonTextWriter(this.buffer);
            this.structDepth = 0;
            this.arrayDepth = 0;
        }

        public void Dispose()
        {
            NativeMethods.sqlite3_finalize(this.currentStatement);

            foreach (var stmt in this.commandTable.Values)
            {
                NativeMethods.sqlite3_finalize((sqlite3_stmt*)stmt);
            }

            this.writer.Close();
            this.buffer.Close();
        }

        public void WriteEventBegin(EventMetadata metadata, RuntimeEventMetadata runtimeMetadata)
        {
            sqlite3_stmt* insertCommand;
            long ptr;

            if (this.commandTable.TryGetValue(metadata, out ptr))
            {
                insertCommand = (sqlite3_stmt*)ptr;
            }
            else
            {
                this.CreateTable(metadata, out insertCommand);
            }

            this.currentStatement = insertCommand;

            NativeMethods.sqlite3_bind_int64(insertCommand, 1, runtimeMetadata.Timestamp);
            NativeMethods.sqlite3_bind_int64(insertCommand, 2, runtimeMetadata.ProcessId);
            NativeMethods.sqlite3_bind_int64(insertCommand, 3, runtimeMetadata.ThreadId);
            NativeMethods.sqlite3_bind_text(insertCommand, 4, runtimeMetadata.ActivityId == Guid.Empty ? string.Empty : runtimeMetadata.ActivityId.ToString(), 36, NativeMethods.Transient);
            NativeMethods.sqlite3_bind_text(insertCommand, 5, runtimeMetadata.RelatedActivityId == Guid.Empty ? string.Empty : runtimeMetadata.RelatedActivityId.ToString(), 36, NativeMethods.Transient);

            this.currentParameterIndex = 6;
        }

        public void WriteEventEnd()
        {
            NativeMethods.sqlite3_step(this.currentStatement);
            NativeMethods.sqlite3_reset(this.currentStatement);
        }

        public void WriteStructBegin()
        {
            ++this.structDepth;
            this.writer.WriteStartObject();
        }

        public void WriteStructEnd()
        {
            --this.structDepth;
            this.writer.WriteEndObject();

            if (this.structDepth == 0 && this.arrayDepth == 0)
            {
                this.writer.Flush();
                this.writer.Close();
                this.BindUTF16String(this.buffer.ToString());
                this.buffer.Close();
                this.builderBuffer.Clear();
                this.buffer = new StringWriter(this.builderBuffer);
                this.writer = new JsonTextWriter(this.buffer);
            }
        }

        public void WritePropertyBegin(PropertyMetadata metadata)
        {
            if (this.structDepth > 0)
            {
                this.writer.WritePropertyName(metadata.Name);
            }
        }

        public void WritePropertyEnd()
        {
        }

        public void WriteArrayBegin()
        {
            ++this.arrayDepth;
            this.writer.WriteStartArray();
        }

        public void WriteArrayEnd()
        {
            --this.arrayDepth;
            this.writer.WriteEndArray();

            if (this.arrayDepth == 0 && this.structDepth == 0)
            {
                this.writer.Flush();
                this.writer.Close();
                this.BindUTF16String(this.buffer.ToString());
                this.buffer.Close();
                this.builderBuffer.Clear();
                this.buffer = new StringWriter(this.builderBuffer);
                this.writer = new JsonTextWriter(this.buffer);
            }
        }

        public void WriteAnsiString(string value)
        {
            this.BindUTF8String(value);
        }

        public void WriteUnicodeString(string value)
        {
            this.BindUTF16String(value);
        }

        public void WriteInt8(sbyte value)
        {
            this.BindInt(value);
        }

        public void WriteUInt8(byte value)
        {
            this.BindInt(value);
        }

        public void WriteInt16(short value)
        {
            this.BindInt(value);
        }

        public void WriteUInt16(ushort value)
        {
            this.BindInt(value);
        }

        public void WriteInt32(int value)
        {
            this.BindInt(value);
        }

        public void WriteUInt32(uint value)
        {
            this.BindInt64(value);
        }

        public void WriteInt64(long value)
        {
            this.BindInt64(value);
        }

        public void WriteUInt64(ulong value)
        {
            this.BindInt64((long)value);
        }

        public void WriteFloat(float value)
        {
            this.BindDouble(value);
        }

        public void WriteDouble(double value)
        {
            this.BindDouble(value);
        }

        public void WriteBoolean(bool value)
        {
            this.BindInt(value ? 1 : 0);
        }

        public void WriteBinary(byte[] value)
        {
            this.BindBlob(value);
        }

        public void WriteGuid(Guid value)
        {
            this.BindUTF8String(value.ToString());
        }

        public void WritePointer(ulong value)
        {
            this.BindInt64((long)value);
        }

        public void WriteFileTime(DateTime value)
        {
            this.BindUTF8String(value.ToString(@"yyyy\-MM\-dd HH\:mm\:ss.FFFFFFF"));
        }

        public void WriteSystemTime(DateTime value)
        {
            this.BindUTF8String(value.ToString(@"yyyy\-MM\-dd HH\:mm\:ss.FFFFFFF"));
        }

        public void WriteSid(string value)
        {
            this.BindUTF8String(value);
        }

        public void WriteUnicodeChar(char value)
        {
            this.BindUTF16String(new string(&value));
        }

        public void WriteAnsiChar(char value)
        {
            this.BindUTF8String(new string(&value));
        }

        public void WriteHexDump(byte[] value)
        {
            this.BindUTF8String(BitConverter.ToString(value));
        }

        public void WriteWbemSid(string value)
        {
            this.BindUTF8String(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BindInt(int value)
        {
            if (this.structDepth > 0 || this.arrayDepth > 0)
            {
                this.writer.WriteValue(value);
            }
            else
            {
                NativeMethods.sqlite3_bind_int(this.currentStatement, this.currentParameterIndex++, value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BindInt64(long value)
        {
            if (this.structDepth > 0 || this.arrayDepth > 0)
            {
                this.writer.WriteValue(value);
            }
            else
            {
                NativeMethods.sqlite3_bind_int64(this.currentStatement, this.currentParameterIndex++, value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BindDouble(double value)
        {
            if (this.structDepth > 0 || this.arrayDepth > 0)
            {
                this.writer.WriteValue(value);
            }
            else
            {
                NativeMethods.sqlite3_bind_double(this.currentStatement, this.currentParameterIndex++, value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BindUTF8String(string value)
        {
            if (this.structDepth > 0 || this.arrayDepth > 0)
            {
                this.writer.WriteValue(value);
            }
            else
            {
                NativeMethods.sqlite3_bind_text(this.currentStatement, this.currentParameterIndex++, value, value.Length, NativeMethods.Transient);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BindUTF16String(string value)
        {
            if (this.structDepth > 0 || this.arrayDepth > 0)
            {
                this.writer.WriteValue(value);
            }
            else
            {
                NativeMethods.sqlite3_bind_text16(this.currentStatement, this.currentParameterIndex++, value, value.Length * 2, NativeMethods.Transient);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BindBlob(byte[] value)
        {
            if (this.structDepth > 0 || this.arrayDepth > 0)
            {
                this.writer.WriteValue(value);
            }
            else
            {
                fixed (byte* ptr = value)
                {
                    NativeMethods.sqlite3_bind_blob(this.currentStatement, this.currentParameterIndex++, (IntPtr)ptr, value.Length, NativeMethods.Transient);
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void CreateTable(EventMetadata metadata, out sqlite3_stmt* insertCommand)
        {
            string name = metadata.Name;
            {
                int i = 0;
                while (this.tableNames.Contains(name))
                {
                    name = name + ++i;
                }

                this.tableNames.Add(name);
            }

            Table table = new Table(name);
            {
                for (int i = 0; i < metadata.Properties.Length; ++i)
                {
                    var property = metadata.Properties[i];
                    if (property.IsStruct)
                    {
                        i += property.ChildrenCount;
                    }

                    table.AddColumn(new TableColumn(property.Name, property.InType, false, false));
                }
            }

            /* create table */
            char* tail;
            var tableSql = table.ToString();
            
            sqlite3_stmt* tableCreationStatement;
            NativeMethods.sqlite3_prepare_v2(this.db, tableSql, tableSql.Length, out tableCreationStatement, out tail);
            NativeMethods.sqlite3_step(tableCreationStatement);
            NativeMethods.sqlite3_finalize(tableCreationStatement);

            /* end create table */

            var insertString = table.InsertString();
            NativeMethods.sqlite3_prepare_v2(this.db, insertString, insertString.Length, out insertCommand, out tail);
            
            this.commandTable.Add(metadata, (long)insertCommand);
        }
    }
}