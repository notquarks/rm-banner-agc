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
        Int32 delay = 25000;
        String path;
        var parser = new FileIniDataParser();
        IniData data = new IniData();
        if (!File.Exists(".\\banner_cfg.ini"))
        {
            Console.WriteLine("config not found, creating...");
            File.Create(".\\banner_cfg.ini").Close();
            //Thread.Sleep(250);
            data["Config"]["delay"] = delay.ToString();
            data["Config"]["path"] = ".\\injector.exe";
            //Thread.Sleep(1250);
            parser.WriteFile(".\\banner_cfg.ini", data);
            Console.WriteLine("banner_cfg.ini created");
        }

        data = parser.ReadFile(".\\banner_cfg.ini");
        delay = Int32.Parse(data["Config"]["delay"]);
        path = data["Config"]["path"];

        Process acrepiProcess = new Process();
        acrepiProcess.StartInfo.FileName = path;
        acrepiProcess.StartInfo.UseShellExecute = true;
        acrepiProcess.StartInfo.Verb = "runas";
        acrepiProcess.Start();

        //String pattern = "01 00 ?? 00 80 07";
        string pattern = "01 00 00 00 80 07 00 00 38 04 00 00 00 03 00 00";
        Mem m = new Mem();
        IntPtr hWnd = FindWindow(null, "Genshin Impact");

        while (hWnd == IntPtr.Zero)
        {
            Console.Write($"\rWindow not found, retrying...");
            hWnd = FindWindow(null, "Genshin Impact");
            Thread.Sleep(100);
        }
        Console.WriteLine("\nWindow Found");
        Thread.Sleep(5000);
        HideGenshinImpactWindow(hWnd);
        ShowWindow(hWnd, 0);
        Console.WriteLine("Hide Window");
        Thread.Sleep(delay);
        Console.WriteLine("Scanning Pattern");
        m.OpenProcess("GenshinImpact");
        //Console.WriteLine("Value for CLibrary.dll+395F68 address is " + m.ReadMemory<int>("CLibrary.dll+395F68").ToString());


        //foreach (long res in AoBScanResults)
        //{
        //    Console.Write("I found the address {0} in the AoB scan.", res, null);
        //    Console.WriteLine("Value for our address is " + m.ReadMemory<int>(res.ToString("X")).ToString());
        //}
        
        long SingleAobScanResult = (await m.AoBScan(pattern, true, true)).FirstOrDefault();
        while (SingleAobScanResult.ToString() == "0")
        {
            SingleAobScanResult = (await m.AoBScan(pattern, true, true)).FirstOrDefault();
            Thread.Sleep(250);
            m.OpenProcess("GenshinImpact");
        }
        Console.Write("Address found", SingleAobScanResult, null);
        Console.WriteLine("Value : " + m.ReadMemory<int>(SingleAobScanResult.ToString("X")).ToString());
        m.WriteMemory(SingleAobScanResult.ToString("X"), "int", "0");
        Console.WriteLine("Banner Removed");
        m.CloseProcess();
        isClosing = true;
        ShowWindow(hWnd, 5);
        return;
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
