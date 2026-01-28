🧬 DigitalTwinFS

Real-Time File System Digital Twin & Security Monitoring System

📌 Proje Özeti

DigitalTwinFS, gerçek bir dosya sisteminin dijital ikizini oluşturarak dosya hareketlerini gerçek zamanlı izleyen, analiz eden ve temel siber güvenlik kontrolleri uygulayan bir C# / .NET console uygulamasıdır.

Proje, Industry 4.0 – Digital Twin yaklaşımını dosya sistemleri üzerine uyarlayarak; izleme, analiz, güvenlik ve raporlama yeteneklerini tek bir mimaride birleştirir.

🎯 Projenin Amacı

Gerçek dosya sistemi ile senkron çalışan dijital bir model oluşturmak

Dosya değişikliklerini anlık olarak tespit etmek

Potansiyel riskli dosyaları güvenlik perspektifiyle analiz etmek

Sistem bütünlüğünü health check mekanizmasıyla doğrulamak

🚀 Temel Özellikler
🔍 Gerçek Zamanlı Dosya İzleme

FileSystemWatcher kullanılarak:

Dosya oluşturma

Dosya silme

Dosya güncelleme
olayları anlık olarak takip edilir.

🧬 Dijital İkiz Modeli

Dosya sistemi, FileNode yapısı ile hiyerarşik bir dijital model olarak temsil edilir.

Fiziksel sistem ile dijital ikiz karşılaştırılabilir durumdadır.

🩺 Health Check (Sistem Tutarlılığı)

Gerçek dosya sistemi ile dijital ikiz arasında:

Eksik dosya

Fazladan dosya

Boyut farkları
tespit edilerek raporlanır.

🔒 Siber Güvenlik & Karantina Mekanizması

Potansiyel riskli uzantılar (.exe, .bat, .cmd) otomatik olarak tespit edilir.

Bu dosyalar:

Karantinaya alınır

Dijital ikizde işaretlenir

Güvenlik loglarına eklenir

📊 Analiz & İstatistik

Dosya türü dağılımları

Depolama ve boyut analizleri

Dosya değişim (delta) raporları

🗃️ Silinme Geçmişi (Dijital Arşiv)

Silinen dosyalar dijital ikizde geçmiş kayıt olarak saklanır.

Sistem davranışları sonradan analiz edilebilir.

🧩 Menü Tabanlı Mimari

Kullanıcı dostu console menüsü sayesinde:

Modüler

Genişletilebilir

Test edilebilir
bir yapı sunar.

🛠️ Kullanılan Teknolojiler

C# / .NET

FileSystemWatcher

JSON Serialization (System.Text.Json)

Nesne Yönelimli Programlama (OOP)

Katmanlı ve modüler mimari

▶️ Kurulum ve Çalıştırma
git clone https://github.com/Mervekrdnnz/DigitalTwinFS.git
cd DigitalTwinFS
dotnet run


.NET SDK kurulu olmalıdır.

📈 Proje Seviyesi

Zorluk: Orta – Orta/Zor

Hedef Profil:

Yönetim Bilişim Sistemleri

Junior Software Developer

Junior IT / System & Security

💼 CV’de Nasıl Yazılır? (Örnek)

Developed a real-time file system digital twin using C#/.NET, including security monitoring, quarantine mechanisms, and system health checks based on Industry 4.0 principles.

🔮 Geliştirilebilir Özellikler

Loglama (Serilog / NLog)

Risk puanlama sistemi (Threat Score)

GUI veya Web Dashboard

Yetkilendirme & rol bazlı erişim

Veritabanı entegrasyonu

📌 Not:
Bu proje eğitim amaçlı geliştirilmiştir ve gerçek sistemlerde kullanılmadan önce ek güvenlik önlemleri gerektirir.
