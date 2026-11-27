using Shared.Enums;

namespace Client.LayoutLogics.Kitchen;

public enum WaitLevel { Normal, Warning, Late }

// Một dòng món trên phiếu bếp (bất biến).
public sealed record KitchenLine(
    int DetailId, int MenuItemId, string Name, int Quantity,
    string? Note, ItemStatus Status, DateTime SentAtUtc)
{
    // Đang chờ bếp xử lý (đồng hồ còn chạy)
    public bool IsActive => Status is ItemStatus.Pending or ItemStatus.Cooking;

    public TimeSpan WaitingAt(DateTime nowUtc)
        => nowUtc > SentAtUtc ? nowUtc - SentAtUtc : TimeSpan.Zero;
}

// Một phiếu = một order còn món cần bếp quan tâm (Pending/Cooking/Ready).
public sealed record KitchenTicket(
    int OrderId, int TableId, string TableName,
    DateTime CreatedAtUtc, IReadOnlyList<KitchenLine> Lines)
{
    public DateTime? OldestActiveAtUtc
        => Lines.Where(l => l.IsActive).Select(l => (DateTime?)l.SentAtUtc).Min();
}