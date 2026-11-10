using Shared.Enums;

namespace Shared.DTOs.Table;

public sealed record TableDto(int Id, string Name, int Seats, TableStatus Status);
