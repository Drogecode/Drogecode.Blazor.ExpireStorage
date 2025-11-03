namespace Drogecode.Blazor.ExpireStorage.Helpers;

internal static class ConsoleHelper
{
    public static bool LogToConsole { get; set; }
    
    /// <summary>
    /// Write to the console when LogToConsole is true;
    /// </summary>
    public static void WriteLine(string message)
    {
        if (!LogToConsole) return;
        Console.WriteLine(message);
    }
    /// <summary>
    /// Write exception to console when LogToConsole is true;
    /// </summary>
    public static void WriteLine(Exception exception)
    {
        if (!LogToConsole) return;
        Console.WriteLine(exception);
    }
    /// <summary>
    /// Write the message and exception to the console when LogToConsole is true;
    /// </summary>
    public static void WriteLine(string message, Exception exception)
    {
        if (!LogToConsole) return;
        Console.WriteLine(message, exception);
    }
}
