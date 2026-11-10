namespace Server.Models.Entities;

public class Category
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
}
