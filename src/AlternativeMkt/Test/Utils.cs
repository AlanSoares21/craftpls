namespace AlternativeMkt.Tests;

public static class Utils
{
    public static IDateTools GetMockedDateTools() {
        return new Mock<IDateTools>().Object;
    }
}