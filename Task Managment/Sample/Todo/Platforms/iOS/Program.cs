using ObjCRuntime;
using UIKit;

namespace Todo;

/// <summary>
/// The application entry point on iOS. Forwards to
/// <see cref="AppDelegate"/> via <see cref="UIApplication.Main(string[], Type, Type)"/>.
/// </summary>
public class Program
{
    /// <summary>
    /// The main entry point of the iOS application.
    /// </summary>
    /// <param name="args">Command-line arguments passed by the iOS runtime.</param>
    private static void Main(string[] args)
    {
        // If you want to use a different Application Delegate class from "AppDelegate"
        // you can specify it here.
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}
