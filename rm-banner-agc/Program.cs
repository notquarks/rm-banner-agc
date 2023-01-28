// See https://aka.ms/new-console-template for more information
using Memory;
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
    
    public static async Task Main(string[] args)
    {
        if(FindWindow(null, "Genshin Impact") != IntPtr.Zero)
        {
            Console.WriteLine("Genshin Impact window found!");
            Console.WriteLine("Scanning Pattern...");
            Mem m = new Mem();
            string pattern = "01 00 00 00 80 07 00 00 38 04 00 00 00 03 00 00";
            m.OpenProcess("GenshinImpact");
            long aobScanRes = (await m.AoBScan(pattern, true, true)).FirstOrDefault();
            Console.Write("Address found", aobScanRes, null);
            Console.WriteLine("Value : " + m.ReadMemory<int>(aobScanRes.ToString("X")).ToString());
            m.WriteMemory(aobScanRes.ToString("X"), "int", "0");
            Console.WriteLine("Banner Removed");
            m.CloseProcess();
            return;
        }
        else
        {
            Console.WriteLine("Genshin Impact is not running");
        }
    }

}
