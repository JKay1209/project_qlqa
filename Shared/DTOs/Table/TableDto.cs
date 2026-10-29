using QLQA.Enums;

namespace QLQA.DTOs.Table;

public sealed record TableDto(int Id, string Name, int Seats, TableStatus Status);
