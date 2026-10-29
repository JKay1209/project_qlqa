namespace QLQA.DTOs.Menu;

public sealed record MenuItemDto(int Id, int CategoryId, string Name, decimal Price, bool Available);
