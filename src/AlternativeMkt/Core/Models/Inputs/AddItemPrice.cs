
namespace AlternativeMkt.Models;

public class AddItemPrice
{
    public int price { get; set; }

    public int itemId { get; set; }
    public Guid manufacturerId { get; set; }
}