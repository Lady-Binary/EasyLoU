using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

class Program
{
    private static Random random = new Random();

    /// <summary>
    ///     Generate a random alphanumeric string.
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
    ///     Ensure we are running from temp folder.
    ///     If we are not, then copy the current executable into temp, execute it from there,
    ///     and kill the current process.
    ///     WARNING: this is done only in DEBUG mode in order to facilitate debugging and troubleshooting.
    ///     WARNING: running in DEBUG mode and bypassing this could put you AT RISK of getting caught.
    /// </summary>
    private static void EnsureRunningFromTemp()
    {
#if DEBUG
        Console.WriteLine("WARNING: We are running in DEBUG mode and therefore we are not going to migrate to temp.");
        Console.WriteLine("This is only for debugging mode and could put you at risk.");
        Console.WriteLine();
        Console.WriteLine("Press 'y' if you know what you are doing and would like to continue AT YOUR OWN RISK.");
        ConsoleKeyInfo key = Console.ReadKey();
        Console.WriteLine();
        if (key.Key.ToString() != "y" && key.Key.ToString() != "Y")
        {
            Environment.Exit(-1);
        }
#else
        Console.WriteLine("Checking if we are running from temp folder...");

        string ourPath = Process.GetCurrentProcess().MainModule.FileName;
        if (!ourPath.ToLower().Contains(Path.GetTempPath().ToLower()))
        {
            Console.WriteLine($"...we are not running from temp folder but from {ourPath} instead, need to migrate!");

            Console.WriteLine();

            string tempPath = Path.Combine(Path.GetTempPath(), GenerateRandomString(16) + ".exe");
            Console.WriteLine($"Migrating to {tempPath}...");
            if (tempPath.ToLower().Contains("easylou"))
            {
                Console.WriteLine("ERROR: something is off, the temp folder path contains the string EasyLoU.");
                Console.WriteLine("We will certainly get caught by the anti-cheat and need to terminate now.");
                Console.WriteLine("Please hit Enter to close this window.");
                Console.ReadLine();
                Environment.Exit(-1);
            }
            File.Copy(ourPath, tempPath);
            Process.Start(tempPath);
            Console.WriteLine($"...migration to {tempPath} complete, killing current process!");
            Environment.Exit(0);
        }

        Console.WriteLine($"...we are running from {ourPath}, all good!");
#endif
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
            Console.WriteLine("WARNING: SoB_Launcher not found.");
            Console.WriteLine("Please start the SoB_Launcher first, then launch the game client, and then start EasyLoU_Launcher.");
            Console.WriteLine();
            Console.WriteLine("Press 'y' if you know what you are doing and would like to continue AT YOUR OWN RISK.");
            ConsoleKeyInfo key = Console.ReadKey();
            Console.WriteLine();
            if (key.Key.ToString() != "y" && key.Key.ToString() != "Y")
            {
                Environment.Exit(-1);
            }
            Console.WriteLine("...SoB_Launcher was not found, but you decided to continue AT YOUR OWN RISK.");
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
            Console.WriteLine();
            Console.WriteLine("WARNING: it looks like SoB_Launcher was updated!");
            Console.WriteLine($"New SoB_Launcher hash: {sobLauncherHash}");
            Console.WriteLine("This means it may contain new anticheat features that could put you in danger.");
            Console.WriteLine("Please ask for guidance on the EasyLoU Discord before proceeding.");
            Console.WriteLine();
            Console.WriteLine("Press 'y' if you know what you are doing and would like to continue AT YOUR OWN RISK.");
            ConsoleKeyInfo key = Console.ReadKey();
            Console.WriteLine();
            if (key.Key.ToString() != "y" && key.Key.ToString() != "Y")
            {
                Environment.Exit(-1);
            }
            Console.WriteLine("...SoB_Launcher version is not known, but you decided to continue AT YOUR OWN RISK.");
            return;
        }

        Console.WriteLine("...SoB_Launcher version is known, all good, we can continue.");
    }

    /// <summary>
    ///     Prepare the EasyLoU executable into a temp folder, waiting to be patched.
    /// </summary>
    /// <returns>The EasyLoU executable file path.</returns>
    private static string StageEasyLoU()
    {
        Console.WriteLine("Staging EasyLoU into temporary folder...");

        string easyLoUPath = Path.Combine(Path.GetTempPath(), GenerateRandomString(16) + ".exe");

        Console.WriteLine($"...preparing Windows Defender exclusion for {easyLoUPath}...");
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"powershell -inputformat none -outputformat none -NonInteractive -Command Add-MpPreference -ExclusionPath \"{easyLoUPath}\""
            }
        };
        process.Start();
        process.WaitForExit();

        Console.WriteLine($"...copying EasyLoU to {easyLoUPath}...");
        File.WriteAllBytes(easyLoUPath, EasyLoU_Launcher.Resources.EasyLoU);

        Console.WriteLine($"...{easyLoUPath} ready to be patched!");

        return easyLoUPath;
    }

    /// <summary>
    ///     Prepare the rcedit executable into a temp folder, waiting to patch EasyLoU.
    /// </summary>
    /// <returns>The rcedit executable file path.</returns>
    private static string StageRcedit()
    {
        Console.WriteLine("Staging rcedit into temporary folder...");

        string rceditPath = Path.Combine(Path.GetTempPath(), "rcedit-x64.exe");

        Console.WriteLine($"...preparing Windows Defender exclusion for {rceditPath}...");
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"powershell -inputformat none -outputformat none -NonInteractive -Command Add-MpPreference -ExclusionPath \"{rceditPath}\""
            }
        };
        process.Start();
        process.WaitForExit();

        Console.WriteLine($"...copying rcedit to {rceditPath}...");
        File.WriteAllBytes(rceditPath, EasyLoU_Launcher.Resources.rcedit_x64);

        Console.WriteLine($"...{rceditPath} ready to patch!");

        return rceditPath;
    }

    /// <summary>
    ///     Patch the EasyLoU executable via rcedit to further evade the anti-cheat.
    /// </summary>
    /// <param name="rceditPath">The rcedit executable file path.</param>
    /// <param name="easyLoUPath">The EasyLoU executable file path.</param>
    private static void PatchEasyLoU(string rceditPath, string easyLoUPath)
    {
        Console.WriteLine("Patching EasyLoU via rcedit to further evade anti-cheat...");

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
    }

    /// <summary>
    ///     Launch EasyLoU.
    /// </summary>
    private static void StartEasyLoU(string easyLoUPath)
    {
        Console.WriteLine("Starting EasyLoU ...");

        Process.Start(easyLoUPath);

        Console.WriteLine("...EasyLoU started, all good! It is safe to close this window now.");
    }

    static void Main(string[] args)
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        Console.WriteLine();
        Console.WriteLine($"EasyLoU_Launcher v{version} - made by Lady Binary with \u2665");
        Console.WriteLine();

        EnsureRunningFromTemp();

        Console.WriteLine();

        var sobLauncherFileName = GetSoBLauncherFilePath();

        Console.WriteLine();

        if (sobLauncherFileName != "")
        {
            CheckSoBLauncherVersion(sobLauncherFileName);

            Console.WriteLine();
        }

        var easyLoUPath = StageEasyLoU();

        Console.WriteLine();

        var rceditPath = StageRcedit();
        
        Console.WriteLine();

        PatchEasyLoU(rceditPath, easyLoUPath);

        Console.WriteLine();

        StartEasyLoU(easyLoUPath);

        Console.WriteLine();

        Console.WriteLine("All good, please hit Enter to close this window.");
        Console.ReadLine();
    }
}
