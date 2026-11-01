namespace Shared.Enums;

/// <summary>
/// Giá trị số TRÙNG với RoleId trong bảng Roles trong Database
/// Muốn thêm vai trò: thêm ở đây, migration sẽ tự seed dòng mới.
/// </summary>
public enum UserRole
{
    Admin = 1,
    Manager = 2,
    Cashier = 3,
    Kitchen = 4,
    Waiter = 5
}
