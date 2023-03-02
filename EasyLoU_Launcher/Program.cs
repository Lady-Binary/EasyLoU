using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

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

    /// <summary>
    ///     Search for a process named SoB_Launcher, and return its executable file path.
    /// </summary>
    /// <returns>The SoB_Launcher executable file path.</returns>
    private static string GetSoBLauncherFilePath()
    {
        Console.WriteLine("Checking if SoB_Launcher is running...");

        var sobLauncherProcess = Process.GetProcessesByName("SoB_Launcher");
        if (sobLauncherProcess == null || sobLauncherProcess.Length == 0)
        {
            Console.WriteLine();
            Console.WriteLine("ERROR: SoB_Launcher not found.");
            Console.WriteLine("Please start the SoB_Launcher first, then launch the game client, and then start EasyLoU_Launcher.");
            Console.WriteLine();
            Console.WriteLine("Press 'y' if you know what you are doing and would like to continue at your own risk.");
            ConsoleKeyInfo key = Console.ReadKey();
            Console.WriteLine();
            if (key.Key.ToString() != "y" && key.Key.ToString() != "Y")
            {
                Environment.Exit(-1);
            }

            return "";
        }

        string sobLauncherFileName = sobLauncherProcess[0].MainModule.FileName;

        Console.WriteLine($"...SoB_Launcher found {sobLauncherFileName}, all good, we can continue.");

        return sobLauncherFileName;
    }

    /// <summary>
    ///     Check if the SoB_Launcher executable is a known version or not.
    /// </summary>
    /// <param name="sobLauncherFileName">The SoB_Launcher executable file path.</param>
    private static void CheckSoBLauncherVersion(string sobLauncherFileName)
    {
        Console.WriteLine("Checking SoB_Launcher version...");

        var KNOWN_SOBLAUNCHER_HASHES = new System.Collections.ArrayList() {
                "08ba8c5c4d2bfc5c0558774d850a45f102667eb24f1f58cc41805017dcc98dae", // Released 2023-02-28
                "c049b38b7a3a28ea44e2dcc7451a6ae25de5f17ab0383b3fa26c24fb412213bf" // Released 2023-03-02
            };
        BufferedStream inputStream = new BufferedStream(File.OpenRead(sobLauncherFileName), 1200000);
        string sobLauncherHash = BitConverter.ToString(new SHA256Managed().ComputeHash(inputStream)).Replace("-", string.Empty).ToLowerInvariant();
        if (!KNOWN_SOBLAUNCHER_HASHES.Contains(sobLauncherHash))
        {
            Console.WriteLine("WARNING: it looks like SoB_Launcher was updated!");
            Console.WriteLine($"New SoB_Launcher hash: {sobLauncherHash}");
            Console.WriteLine("This means it may contain new anticheat features that could put you in danger.");
            Console.WriteLine("Please ask for guidance on the EasyLoU Discord before proceeding.");
            Console.WriteLine();
            Console.WriteLine("Press 'y' if you know what you are doing and would like to continue at your own risk.");
            ConsoleKeyInfo key = Console.ReadKey();
            Console.WriteLine();
            if (key.Key.ToString() != "y" && key.Key.ToString() != "Y")
            {
                Environment.Exit(-1);
            }
        }

        Console.WriteLine("...SoB_Launcher version is known, all good, we can continue.");
    }

    static void Main(string[] args)
    {
        string rceditPath = Path.Combine(Path.GetTempPath(), "rcedit-x64.exe");
        string easyLoUPath = Path.Combine(Path.GetTempPath(), GenerateRandomString(8) + ".exe");

        var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        Console.WriteLine();
        Console.WriteLine($"EasyLoU_Launcher v{version} - made by Lady Binary with \u2665");
        Console.WriteLine();

        var sobLauncherFileName = GetSoBLauncherFilePath();

        Console.WriteLine();

        if (sobLauncherFileName != "")
        {
            CheckSoBLauncherVersion(sobLauncherFileName);
        }

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
