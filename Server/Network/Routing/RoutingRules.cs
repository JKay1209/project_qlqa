using Shared.Enums;

namespace Server.Network.Routing;

// Toàn bộ logic phân quyền và event-receiver cụ  thể.
public static class RoutingRules
{
    public static bool IsStaff(UserRole r)
        => r is UserRole.Admin or UserRole.Manager or UserRole.Cashier or UserRole.Kitchen or UserRole.Waiter;

    public static bool CanCreateOrder(UserRole r)
        => r is UserRole.Waiter or UserRole.Admin or UserRole.Manager;

    // ---- Người nhận ----
    // Order mới: Bếp nấu, Thu ngân tính tiền, Chủ quán/Quản lý theo dõi.
    public static bool ReceivesNewOrder(UserRole r)
        => r is UserRole.Kitchen or UserRole.Cashier or UserRole.Admin or UserRole.Manager;

    // Trạng thái món và trạng thái bàn: mọi nhân viên.
    public static bool ReceivesItemStatus(UserRole r) => IsStaff(r);
    public static bool ReceivesTableStatus(UserRole r) => IsStaff(r);

    // ---- Đổi trạng thái món ----
    public static bool CanChangeItem(UserRole role, ItemStatus from, ItemStatus to)
    {
        bool kitchen = (from, to) is
            (ItemStatus.Pending, ItemStatus.Cooking) or
            (ItemStatus.Pending, ItemStatus.Ready) or
            (ItemStatus.Cooking, ItemStatus.Ready) or
            (ItemStatus.Pending, ItemStatus.Cancelled) or
            (ItemStatus.Cooking, ItemStatus.Cancelled);

        bool waiter = (from, to) is
            (ItemStatus.Ready, ItemStatus.Served) or
            (ItemStatus.Pending, ItemStatus.Cancelled);

        return role switch
        {
            UserRole.Kitchen => kitchen,
            UserRole.Waiter => waiter,
            UserRole.Admin or UserRole.Manager => kitchen || waiter,
            _ => false
        };
    }

    // ---- Đổi trạng thái bàn ----
    // hasUnpaidItems: bàn còn order chưa thanh toán (có món chưa hủy).
    // Bàn còn nợ chỉ về Trống qua luồng thanh toán (Task 22), không đổi tay.
    public static bool CanChangeTable(UserRole role, TableStatus from, TableStatus to, bool hasUnpaidItems)
    {
        if (role is not (UserRole.Waiter or UserRole.Cashier or UserRole.Admin or UserRole.Manager))
            return false;
        if (to == TableStatus.Free && hasUnpaidItems)
            return false;

        return (from, to) switch
        {
            (TableStatus.Free, TableStatus.Reserved) => true,
            (TableStatus.Free, TableStatus.Occupied) => true,
            (TableStatus.Reserved, TableStatus.Free) => true,
            (TableStatus.Reserved, TableStatus.Occupied) => true,
            (TableStatus.Occupied, TableStatus.WaitingPayment) => true,
            (TableStatus.Occupied, TableStatus.Free) => true,
            (TableStatus.WaitingPayment, TableStatus.Occupied) => true,
            (TableStatus.WaitingPayment, TableStatus.Free) => role != UserRole.Waiter,
            _ => false
        };
    }
}