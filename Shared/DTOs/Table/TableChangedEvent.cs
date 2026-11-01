using Shared.Enums;

namespace Shared.DTOs.Table;

/// <summary>Server broadcast cho mọi client khi trạng thái một bàn thay đổi.</summary>
public sealed record TableChangedEvent(int TableId, TableStatus NewStatus);
