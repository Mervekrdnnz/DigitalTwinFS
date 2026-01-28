using System;
using System.IO;
using DigitalTwinFS;

Console.Clear();
Console.Title = "DIGITAL TWIN - ENTERPRISE CONTROL CENTER v4.0";

Console.ForegroundColor = ConsoleColor.Magenta;
Console.WriteLine(@"
    ╔════════════════════════════════════════════════════════════════╗
    ║   ██████╗ ██╗ ██████╗ ██╗████████╗ █████╗ ██╗     ████████╗██╗ ║
    ║   ██╔══██╗██║██╔════╝ ██║╚══██╔══╝██╔══██╗██║     ╚══██╔══╝██║ ║
    ║   ██║  ██║██║██║  ███╗██║   ██║   ███████║██║        ██║   ██║ ║
    ║   ██║  ██║██║██║   ██║██║   ██║   ██╔══██║██║        ██║   ██║ ║
    ║   ██████╔╝██║╚██████╔╝██║   ██║   ██║  ██║███████╗   ██║   ██║ ║
    ║   ╚═════╝ ╚═╝ ╚═════╝ ╚═╝   ╚═╝   ╚═╝  ╚═╝╚══════╝   ╚═╝   ╚═╝ ║
    ╠════════════════════════════════════════════════════════════════╣
    ║         INDUSTRIAL DATA MONITORING & CYBER SECURITY            ║
    ╚════════════════════════════════════════════════════════════════╝");
Console.ResetColor();

string baseDir = AppDomain.CurrentDomain.BaseDirectory;
string watchPath = Path.Combine(baseDir, "ModelData");
if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

try 
{
    TwinEngine engine = new TwinEngine(watchPath);
    engine.Start();

    bool running = true;
    while (running)
    {
        Console.WriteLine("\n" + new string('═', 70));
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  [1] SAĞLIK TARA (Senkron)   [5] AKILLI ARAMA     [0] RESMİ RAPOR AL");
        Console.WriteLine("  [2] OPERASYONEL ÖZET        [6] YAZMA TESTİ      [!] KARANTİNA İNCELE");
        Console.WriteLine("  [3] DİJİTAL ARŞİV           [7] KLASÖRÜ AÇ       [L] SİSTEM LOGLARI");
        Console.WriteLine("  [4] KAPASİTE ANALİZİ        [8] TÜM KAYITLAR     [9] SİSTEMİ KAPAT");
        Console.ResetColor();
        Console.WriteLine(new string('═', 70));
        Console.Write(" >> Komut Bekleniyor: ");
        
        string? input = Console.ReadLine()?.ToUpper();
        Console.Clear();

        if (input == "9") { running = false; break; }

        switch (input)
        {
            case "1": engine.CheckHealth(); break;
            case "2": engine.ShowSummary(); break;
            case "3": engine.ShowArchive(); break;
            case "4": engine.ShowSizeAnalysis(); break;
            case "5": 
                Console.Write("🔍 Aranacak Dosya: "); 
                engine.SearchFile(Console.ReadLine() ?? ""); 
                break;
            case "6": 
                Console.Write("📝 Dosya Adı: "); 
                string n = Console.ReadLine() ?? "test.txt";
                Console.Write("⚖️ Boyut (MB): "); 
                if (long.TryParse(Console.ReadLine(), out long s)) engine.CreateDummyFile(n, s);
                break;
            case "7": engine.OpenFolder(); break;
            case "8": case "L": engine.ShowLogs(); break;
            case "0": engine.ExportFinalReport(); break;
            case "!": engine.ShowQuarantine(); break;
            default: Console.WriteLine("⚠️ Geçersiz komut."); break;
        }
    }
}
catch (Exception ex) { Console.WriteLine($"\n[HATA]: {ex.Message}"); }