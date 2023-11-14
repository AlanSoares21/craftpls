
namespace AlternativeMkt;

public class DateTools : IDateTools
{
    public DateTime UtcNow()
    {
        return new DateTime(DateTime.UtcNow.Ticks);
    }
}