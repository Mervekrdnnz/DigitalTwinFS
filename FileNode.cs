namespace DigitalTwinFS;

public class FileNode
{
    public string Name { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime LastModified { get; set; }
    public bool IsDeleted { get; set; } = false;
    
    // YENİ EKLEDİĞİMİZ KISIM:
    // Dosyanın türünü (Resim, Kod, Doküman vb.) burada tutacağız.
    public string Category { get; set; } = "Genel"; 
}