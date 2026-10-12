using ObjCRuntime;
using UIKit;

namespace Todo;

/// <summary>
/// Application entry point used on Mac Catalyst. Forwards to
/// <see cref="AppDelegate"/> via <see cref="UIApplication.Main(string[], Type, Type)"/>.
/// </summary>
public class Program
{
    /// <summary>
    /// The main entry point of the Mac Catalyst application.
    /// </summary>
    /// <param name="args">Command-line arguments passed by the Mac Catalyst runtime.</param>
    private static void Main(string[] args)
    {
        // If you want to use a different Application Delegate class from "AppDelegate"
        // you can specify it here.
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}
