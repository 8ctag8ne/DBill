using System;

namespace CoreLib.Exceptions
{
    public class TableNotFoundException : DatabaseException
    {
        public string TableName { get; }

        public TableNotFoundException(string tableName) 
            : base($"Table '{tableName}' not found")
        {
            TableName = tableName;
        }
    }
}