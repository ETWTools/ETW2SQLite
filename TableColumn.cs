namespace ETW2SQLite
{
    using ETWDeserializer;

    internal sealed class TableColumn
    {
        public TableColumn(string name, TDH_IN_TYPE type, bool isPrimaryKey, bool isAutoIncrement)
        {
            this.Name = name;
            this.Type = type;
            this.IsPrimaryKey = isPrimaryKey;
            this.IsAutoIncrement = isAutoIncrement;
        }

        public string Name { get; private set; }

        public TDH_IN_TYPE Type { get; private set; }

        public bool IsPrimaryKey { get; private set; }

        public bool IsAutoIncrement { get; private set; }

        public override string ToString()
        {
            string decl = "\"" + this.Name + "\" " + this.Type.SQLiteType() + " ";
            if (this.IsPrimaryKey)
            {
                decl += "PRIMARY KEY ";
            }

            if (this.IsAutoIncrement)
            {
                decl += "AUTOINCREMENT ";
            }

            return decl;
        }
    }
}