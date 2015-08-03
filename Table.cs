namespace ETW2SQLite
{
    using System.Collections.Generic;
    using System.Text;

    internal sealed class Table
    {
        public Table(string name)
        {
            this.Name = name;
            this.Columns = new List<TableColumn>();
        }

        public string Name { get; private set; }

        public void AddColumn(TableColumn column)
        {
            this.Columns.Add(column);
        }

        public List<TableColumn> Columns { get; private set; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("CREATE TABLE `");
            builder.Append(this.Name.Replace('`', '_'));
            builder.Append("` (`Timestamp` INTEGER, `ProcessId` INTEGER, `ThreadId` INTEGER, `ActivityId` VARCHAR (36) , `RelatedActivityId` VARCHAR (36)");

            if (this.Columns.Count == 0)
            {
                return builder.Append(")").ToString();
            }
            else
            {
                builder.Append(",");
            }

            var count = this.Columns.Count;
            for (int index = 0; index < count; index++)
            {
                var column = this.Columns[index];
                builder.Append(" `");
                builder.Append(index);
                builder.Append("_");
                builder.Append(column.Name.Replace('`', '_'));
                builder.Append("` ");
                builder.Append(column.Type.SQLiteType());

                if (count - index != 1)
                {
                    builder.Append(",");
                }
            }

            return builder.Append(")").ToString();
        }

        public string InsertString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("INSERT INTO `");
            builder.Append(this.Name.Replace('`', '_'));
            builder.Append("` (`Timestamp`, `ProcessId`, `ThreadId`, `ActivityId`, `RelatedActivityId`");

            var count = this.Columns.Count;
            if (count == 0)
            {
            }
            else
            {
                builder.Append(",");

                for (int index = 0; index < count; index++)
                {
                    var column = this.Columns[index];
                    builder.Append(" `");
                    builder.Append(index);
                    builder.Append("_");
                    builder.Append(column.Name);
                    builder.Append("`");

                    if (count - index != 1)
                    {
                        builder.Append(",");
                    }
                }
            }

            builder.Append(") VALUES (");

            count = this.Columns.Count + 5; // 5 for Timestamp, ProcessId, ThreadId, ActivityId, RelatedActivityId
            for (int index = 0; index < count; index++)
            {
                builder.Append("@");
                builder.Append(index);

                if (count - index != 1)
                {
                    builder.Append(", ");
                }
            }

            return builder.Append(")").ToString();
        }
    }
}