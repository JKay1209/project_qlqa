using Shared.Enums;

namespace Server.Models.Entities;

public class Table
{
    public int TableId { get; set; }
    public string TableName { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public TableStatus Status { get; set; } = TableStatus.Free;
}
