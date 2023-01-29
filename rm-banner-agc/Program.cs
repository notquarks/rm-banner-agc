// See https://aka.ms/new-console-template for more information
using IniParser;
using IniParser.Model;
using Memory;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

internal class Program
{
    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("User32")]
    static extern int ShowWindow(IntPtr hwnd, int nCmdShow);

    private static bool isClosing = false;

    
    public static async Task Main(string[] args)
    {
        Int32 delay = 28000;
        String path;
        var parser = new FileIniDataParser();
        IniData data = new IniData();
        Mem m = new Mem();
        if (!File.Exists(".\\banner_cfg.ini"))
        {
            Console.WriteLine("config not found, creating...");
            File.Create(".\\banner_cfg.ini").Close();
            data["Config"]["delay"] = delay.ToString();
            data["Config"]["path"] = ".\\injector.exe";
            parser.WriteFile(".\\banner_cfg.ini", data);
            Console.WriteLine("banner_cfg.ini created");
        }
        delay = 5000;
        data = parser.ReadFile(".\\banner_cfg.ini");
        path = data["Config"]["path"];
        if (!m.OpenProcess("GenshinImpact"))
        {
            Process acrepiProcess = new Process();
            acrepiProcess.StartInfo.FileName = path;
            acrepiProcess.StartInfo.UseShellExecute = true;
            acrepiProcess.StartInfo.Verb = "runas";
            acrepiProcess.Start();
            delay = Int32.Parse(data["Config"]["delay"]);
        }
        //String pattern = "01 00 ?? 00 80 07";
        //string pattern = "01 00 00 00 80 07 00 00 38 04 00 00 00 03 00 00";
        String offset = "CLibrary.dll+395F68";
        IntPtr hWnd = FindWindow(null, "Genshin Impact");

        while (hWnd == IntPtr.Zero)
        {
            Console.Write($"\rWindow not found, retrying...");
            hWnd = FindWindow(null, "Genshin Impact");
            Thread.Sleep(100);
        }
        Console.WriteLine("\nWindow Found");
        Thread.Sleep(5000);
        //HideGenshinImpactWindow(hWnd);
        //Console.WriteLine("Hide Window");
        Console.WriteLine("Waiting clibrary to load...");
        Thread.Sleep(delay);
        m.OpenProcess("GenshinImpact");
        //Console.WriteLine("Value for CLibrary.dll+395F68 is " + m.ReadMemory<int>("CLibrary.dll+395F68").ToString());
        int offsetValue = m.ReadMemory<int>("CLibrary.dll+395F68");
        int count = 0;
        while (offsetValue != 1)
        {
            offsetValue = m.ReadMemory<int>("CLibrary.dll+395F68");
            Console.Write($"\rReading offset value ... ({count})");
            count++;
            Thread.Sleep(2000);
            m.OpenProcess("GenshinImpact");
            if (count == 15)
            {
                Console.WriteLine("\nTimeout");
                Console.WriteLine("Try close and relaunch this program");
                Thread.Sleep(5000);
                return;
            }
        }
        m.WriteMemory(offset, "int", "0");
        Console.WriteLine("\nBanner Removed");
        m.CloseProcess();
        //isClosing = true;
        //ShowWindow(hWnd, 5);
        Console.WriteLine("You can close this window");
        Console.ReadLine();
    }

    static async void HideGenshinImpactWindow(IntPtr hWndGenshin)
    {
            for (int i = 0; i < 200; i++)
            {
                if (isClosing)
                {
                    break;
                }
                ShowWindow(hWndGenshin, 0);
                await Task.Delay(130);
            }
    }

}
