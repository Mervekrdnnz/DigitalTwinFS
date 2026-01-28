using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace DigitalTwinFS;

public class TwinEngine
{
    private readonly string _watchPath;
    private readonly string _quarantinePath;
    private Dictionary<string, FileNode> _virtualModel = new();
    
    private readonly string _snapshotFile = "twin_snapshot.json";
    private readonly string _logFile = "system_events.log";
    private readonly object _syncLock = new object();
    
    private int _processedEvents = 0;
    private DateTime _startTime;

    public TwinEngine(string path)
    {
        _watchPath = path;
        _startTime = DateTime.Now;
        
        string? root = Directory.GetParent(_watchPath)?.FullName;
        _quarantinePath = Path.Combine(root ?? _watchPath, "Security_Quarantine");
        
        if (!Directory.Exists(_quarantinePath)) 
            Directory.CreateDirectory(_quarantinePath);

        LoadSnapshot();
        LogEvent("SYSTEM_INIT", "Dijital İkiz Motoru v4.0 Başlatıldı.");
    }

    public void Start()
    {
        FileSystemWatcher watcher = new FileSystemWatcher(_watchPath)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size | 
                           NotifyFilters.LastWrite | NotifyFilters.Attributes,
            EnableRaisingEvents = true,
            IncludeSubdirectories = true
        };

        watcher.Created += (s, e) => HandleFileSystemEvent(e, "CREATED");
        watcher.Changed += (s, e) => HandleFileSystemEvent(e, "MODIFIED");
        watcher.Deleted += (s, e) => HandleFileSystemEvent(e, "DELETED");
        watcher.Renamed += (s, e) => HandleFileSystemEvent(e, "RENAMED");
        watcher.Error += (s, e) => LogEvent("ERROR", $"Watcher Hatası: {e.GetException().Message}");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[CORE] İzleme Katmanı Aktif: {_watchPath}");
        Console.ResetColor();
    }

    private void HandleFileSystemEvent(FileSystemEventArgs e, string eventType)
    {
        Interlocked.Increment(ref _processedEvents);
        
        if (eventType == "MODIFIED") Thread.Sleep(100); 

        lock (_syncLock)
        {
            try
            {
                if (eventType == "DELETED")
                {
                    if (_virtualModel.ContainsKey(e.FullPath))
                    {
                        _virtualModel[e.FullPath].IsDeleted = true;
                        _virtualModel[e.FullPath].LastModified = DateTime.Now;
                    }
                }
                else
                {
                    var info = new FileInfo(e.FullPath);
                    if (!info.Exists) return;

                    if (IsThreat(info.Extension))
                    {
                        ExecuteQuarantine(info);
                        return;
                    }

                    AnalyzeDelta(e.FullPath, info.Length);
                    UpdateModel(e.FullPath, info);
                }

                LogEvent(eventType, $"{e.Name} başarıyla işlendi.");
                SaveSnapshot();
            }
            catch (Exception ex)
            {
                LogEvent("CRITICAL_FAIL", $"{e.Name} işlenirken hata: {ex.Message}");
            }
        }
    }

    private bool IsThreat(string extension)
    {
        string[] blackList = { ".exe", ".bat", ".cmd", ".sh", ".vbs", ".ps1" };
        return blackList.Contains(extension.ToLower());
    }

    private void ExecuteQuarantine(FileInfo file)
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string targetPath = Path.Combine(_quarantinePath, $"{timestamp}_{file.Name}.restricted");
        
        File.Move(file.FullName, targetPath);
        
        LogEvent("SECURITY_ALERT", $"Tehdit Karantinaya Alındı: {file.Name}");
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Console.Beep(1000, 500);

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n[🛡️ GÜVENLİK] İhlal Engellendi: {file.Name} -> Karantina dizinine taşındı.");
        Console.ResetColor();
    }

    private void AnalyzeDelta(string path, long newSize)
    {
        if (_virtualModel.ContainsKey(path))
        {
            long oldSize = _virtualModel[path].Size;
            if (oldSize > 0 && oldSize != newSize)
            {
                double ratio = ((double)(newSize - oldSize) / oldSize) * 100;
                string trend = ratio > 0 ? "Büyüme" : "Küçülme";
                LogEvent("DELTA_ANALYSIS", $"{Path.GetFileName(path)} %{Math.Abs(ratio):F2} {trend} gösterdi.");
            }
        }
    }

    private void UpdateModel(string path, FileInfo info)
    {
        _virtualModel[path] = new FileNode
        {
            Name = info.Name,
            FullPath = info.FullName,
            Size = info.Length,
            LastModified = DateTime.Now,
            Category = Categorize(info.Extension),
            IsDeleted = false
        };
    }

    private string Categorize(string ext) => ext.ToLower() switch
    {
        ".txt" or ".pdf" or ".docx" or ".xlsx" => "DOCUMENTATION",
        ".cs" or ".py" or ".cpp" or ".js" or ".html" => "SOURCE_CODE",
        ".jpg" or ".png" or ".mp4" or ".avi" => "MEDIA_ASSET",
        ".zip" or ".rar" or ".7z" => "ARCHIVE",
        _ => "RAW_DATA"
    };

    private void LogEvent(string tag, string message)
    {
        lock (_syncLock)
        {
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{tag}] [ID:{Guid.NewGuid().ToString().Substring(0,8)}] {message}";
            try { File.AppendAllLines(_logFile, new[] { logEntry }); } catch { }
        }
    }

    // --- PROFESYONEL RAPORLAMA MODÜLLERİ ---

    public void ShowSummary()
    {
        var activeItems = _virtualModel.Values.Where(v => !v.IsDeleted).ToList();
        
        Console.WriteLine("\n" + new string('─', 50));
        Console.WriteLine($"   📊 DİJİTAL İKİZ OPERASYONEL ÖZETİ   ");
        Console.WriteLine(new string('─', 50));
        Console.WriteLine($"| {"Kategori",-15} | {"Adet",-6} | {"Hacim (MB)",-10} |");
        Console.WriteLine(new string('─', 50));

        foreach (var group in activeItems.GroupBy(i => i.Category))
        {
            double mb = group.Sum(x => x.Size) / (1024.0 * 1024.0);
            Console.WriteLine($"| {group.Key,-15} | {group.Count(),-6} | {mb,-10:F2} |");
        }
        Console.WriteLine(new string('─', 50));
    }

    // YENİ: Program.cs'deki hatayı çözen metod
    public void ShowArchive()
    {
        Console.WriteLine("\n" + new string('═', 55));
        Console.WriteLine("   🗑️  DİJİTAL ARŞİV GEÇMİŞİ (SİLİNEN DOSYALAR)");
        Console.WriteLine(new string('═', 55));

        var deletedItems = _virtualModel.Values.Where(v => v.IsDeleted).ToList();

        if (!deletedItems.Any())
        {
            Console.WriteLine(" > Arşiv temiz: Silinmiş bir veri kaydı bulunamadı.");
        }
        else
        {
            Console.WriteLine($"| {"Dosya Adı",-25} | {"Silinme Saati",-15} |");
            Console.WriteLine(new string('-', 55));
            foreach (var item in deletedItems)
            {
                Console.WriteLine($"| {item.Name,-25} | {item.LastModified:HH:mm:ss} |");
            }
        }
        Console.WriteLine(new string('═', 55));
    }

    public void ShowSizeAnalysis()
    {
        long totalBytes = _virtualModel.Values.Where(v => !v.IsDeleted).Sum(v => v.Size);
        double totalMb = totalBytes / (1024.0 * 1024.0);
        
        Console.WriteLine($"\n📦 Toplam Sistem Yükü: {totalMb:F2} MB");
        
        int segments = (int)Math.Clamp(totalMb / 1, 1, 20); // 1MB başı bir segment
        Console.Write("Hacim Grafiği: [");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write(new string('█', segments) + new string('░', 20 - segments));
        Console.ResetColor();
        Console.WriteLine("]");
    }

    public void CheckHealth()
    {
        Console.WriteLine("\n[🩺] Derin Sağlık Taraması Yapılıyor...");
        var missing = _virtualModel.Values.Where(v => !v.IsDeleted && !File.Exists(v.FullPath)).ToList();
        
        if (!missing.Any())
            Console.WriteLine("✅ MÜKEMMEL: Dijital ve fiziksel katmanlar %100 senkronize.");
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⚠️ DİKKAT: {missing.Count} dosya ikiz modelde var ama diskte yok!");
            foreach(var m in missing) Console.WriteLine($"   - Kayıp: {m.Name}");
            Console.ResetColor();
        }
    }

    public void SearchFile(string query)
    {
        var results = _virtualModel.Values
            .Where(v => v.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!results.Any()) Console.WriteLine("❌ Eşleşen kayıt bulunamadı.");
        foreach (var r in results)
        {
            string status = r.IsDeleted ? "[SİLİNMİŞ]" : "[AKTİF]";
            Console.WriteLine($"{status,-10} {r.Name,-20} | {r.Category} | {r.LastModified}");
        }
    }

    public void ShowLogs()
    {
        Console.WriteLine("\n📜 SON SİSTEM OLAYLARI (TOP 15)");
        if (File.Exists(_logFile))
        {
            var lines = File.ReadAllLines(_logFile).TakeLast(15);
            foreach (var line in lines) Console.WriteLine(line);
        }
        else Console.WriteLine("Henüz bir log kaydı bulunmuyor.");
    }

    public void ShowQuarantine()
    {
        Console.WriteLine("\n🛡️ KARANTİNA ALTINDAKİ DOSYALAR");
        if (Directory.Exists(_quarantinePath))
        {
            var files = Directory.GetFiles(_quarantinePath);
            if (files.Length == 0) Console.WriteLine("Güvenli: Karantina boş.");
            foreach (var f in files) Console.WriteLine($"🚩 {Path.GetFileName(f)}");
        }
    }

    public void ExportFinalReport()
    {
        string fileName = $"DigitalTwin_FinalReport_{DateTime.Now:yyyyMMdd_HHmm}.txt";
        using (StreamWriter sw = new StreamWriter(fileName))
        {
            sw.WriteLine("==================================================");
            sw.WriteLine("     DİJİTAL İKİZ SİSTEMİ - RESMİ ANALİZ RAPORU    ");
            sw.WriteLine("==================================================");
            sw.WriteLine($"Rapor Tarihi  : {DateTime.Now}");
            sw.WriteLine($"Sistem Çalışma: {(DateTime.Now - _startTime).TotalMinutes:F1} Dakika");
            sw.WriteLine($"İşlenen Olay  : {_processedEvents}");
            sw.WriteLine("--------------------------------------------------");
            sw.WriteLine("\nAKTİF ENVANTER LİSTESİ:");
            foreach (var node in _virtualModel.Values.Where(v => !v.IsDeleted))
                sw.WriteLine($"- {node.Name} | {node.Size} Byte | {node.Category}");
        }
        Console.WriteLine($"\n✅ Rapor Başarıyla Kaydedildi: {fileName}");
    }

    public void OpenFolder()
    {
        try {
            Process.Start(new ProcessStartInfo { FileName = _watchPath, UseShellExecute = true });
        } catch (Exception ex) { Console.WriteLine($"❌ Klasör açılamadı: {ex.Message}"); }
    }
    
    public void CreateDummyFile(string name, long sizeInMb)
    {
        try {
            string path = Path.Combine(_watchPath, name);
            using (var fs = new FileStream(path, FileMode.Create)) fs.SetLength(sizeInMb * 1024 * 1024);
            Console.WriteLine($"✅ Dosya Oluşturuldu: {name}");
        } catch (Exception ex) { Console.WriteLine($"❌ Hata: {ex.Message}"); }
    }

    private void SaveSnapshot() => File.WriteAllText(_snapshotFile, JsonSerializer.Serialize(_virtualModel));
    
    private void LoadSnapshot()
    {
        if (File.Exists(_snapshotFile))
        {
            try { 
                _virtualModel = JsonSerializer.Deserialize<Dictionary<string, FileNode>>(File.ReadAllText(_snapshotFile)) ?? new(); 
            }
            catch { _virtualModel = new(); }
        }
    }
}