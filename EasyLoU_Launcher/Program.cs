using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

class Program
{
    private static Random random = new Random();

    /// <summary>
    ///     Generates a random alphanumeric string.
    /// </summary>
    /// <param name="length">Length of the string</param>
    /// <returns>A randomly generated string.</returns>
    private static string GenerateRandomString(int length)
    {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var stringChars = new char[length];

        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        return new string(stringChars);
    }

    static void Main(string[] args)
    {
        string rceditPath = Path.Combine(Path.GetTempPath(), "rcedit-x64.exe");
        string easyLoUPath = Path.Combine(Path.GetTempPath(), GenerateRandomString(8) + ".exe");

        var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        Console.WriteLine();
        Console.WriteLine($"EasyLoU_Launcher v{version} - made by Lady Binary with \u2665");
        Console.WriteLine();

        Console.WriteLine("Staging EasyLoU into temporary folder...");
        File.WriteAllBytes(rceditPath, EasyLoU_Launcher.Resources.rcedit_x64);
        Console.WriteLine($"...{easyLoUPath} ready to be patched!");

        Console.WriteLine();

        Console.WriteLine("Staging the patching tool rc-edit into temporary folder...");
        File.WriteAllBytes(easyLoUPath, EasyLoU_Launcher.Resources.EasyLoU);
        Console.WriteLine($"...{rceditPath} ready to be launched!");
        
        Console.WriteLine();

        Console.WriteLine("Patching EasyLoU via rc-edit to evade anti-cheat...");

        string rceditCommand = $"\"{easyLoUPath}\"" +
        $" --set-version-string \"Comments\" \"{GenerateRandomString(8)}\"" +
        $" --set-version-string \"CompanyName\" \"{GenerateRandomString(8)}\"" +
        $" --set-version-string \"FileDescription\" \"{GenerateRandomString(8)}\"" +
        //$" --set-version-string \"FileVersion\" \"{GenerateRandomString(8)}\"" + // this doesn't really work
        $" --set-version-string \"InternalName\" \"{GenerateRandomString(8)}\"" +
        $" --set-version-string \"LegalCopyright\" \"{GenerateRandomString(8)}\"" +
        $" --set-version-string \"LegalTrademarks\" \"{GenerateRandomString(8)}\"" +
        $" --set-version-string \"OriginalFilename\" \"{GenerateRandomString(8)}\"" +
        $" --set-version-string \"ProductName\" \"{GenerateRandomString(8)}\"" +
        $" --set-version-string \"ProductVersion\" \"{GenerateRandomString(8)}\"";
        ///Console.WriteLine($"...executing command: {rceditPath} {rceditCommand}...");
        Process.Start(rceditPath, rceditCommand);

        Console.WriteLine("...waiting 5 seconds for the patch to be applied...");
        System.Threading.Thread.Sleep(5000);

        Console.WriteLine("...EasyLoU successfully patched!");

        Console.WriteLine();

        Console.WriteLine("Starting EasyLoU ...");
        Process.Start(easyLoUPath);
        Console.WriteLine("...EasyLoU started, all good! It is safe to close this window now.");

        Console.WriteLine();

        Console.WriteLine("Please hit Enter to close this window.");
        Console.ReadLine();
    }
}
