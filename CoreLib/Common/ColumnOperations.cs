using System;
using System.Collections.Generic;

namespace CoreLib.Models
{
    public class ColumnOperation
    {
        public OperationType Type { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string? OldColumnName { get; set; }
        public string? NewColumnName { get; set; }
        public List<string>? NewOrder { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ColumnOperation CreateRenameOperation(string tableName, string oldName, string newName)
        {
            return new ColumnOperation
            {
                Type = OperationType.Rename,
                TableName = tableName,
                OldColumnName = oldName,
                NewColumnName = newName
            };
        }

        public static ColumnOperation CreateReorderOperation(string tableName, List<string> newOrder)
        {
            return new ColumnOperation
            {
                Type = OperationType.Reorder,
                TableName = tableName,
                NewOrder = newOrder
            };
        }
    }

    public enum OperationType
    {
        Rename,
        Reorder
    }
}