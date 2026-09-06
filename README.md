# Electricity Price Alert Engine

## Electricity Price Engine Nedir?

Electricity Price Engine enerji piyasalarındaki saatlik fiyat değişimlerini izlemek ve önceden koyulmuş kurallara göre kullanıcıya alarmlar oluşturan bir "Kural Motoru"dur.

Geleneksel mimarilerde bu tarz kontroller sayısız if-else blokları ile oluşturulurken bu projede Open/Closed prensibini kullanarak şu 3 sorunu çözmeyi hedefledim:
1. **Koddan Bağımsız İş Mantığı:** Kurallar C# kodunun içine gömülmeden JSON'dan okunur. Bu da kolayca yeni kurallar eklememizi ya da var olan kurallarımızı çıkarmamızı sağlar.
2. **Derleme Gerektirmeyen Esneklik:** Sisteme yeni bir kural ekleme veya çıkarma gibi durumlarda sistemi yeniden derlemeye (recompile) gerek yoktur. Çalışmaya kaldığı yerden devam edebilir.
3. **Modülerlik:** Sistem modülerliğini Composite Pattern ile sağlarken hesaplama karmaşasını recursive (özyineli) kurguladım. Kapsayıcı bir kuralın `Evaluate` metodu, kendi içindeki alt kuralların `Evaluate` metodunu tetikliyor. Bu recursive yapı sayesinde sınırsız derinlikte bir kural ağacı, ana motoru hiç yormadan aşağıdan yukarıya doğru çözebiliyor.

## Mimari Nedir? Neden Bu Şekilde Yaptım?

Tasarımı oluştururken öncelikle kuralların dinamik ve genişletilebilir olmasına odaklandım. Bu yüzden Gang of Four kalıplarından Composite Pattern'i tercih ettim.

* Projeyi Rule-Based / Composite Architecture ile oluşturdum. Amacım sınıf başına düşen döngüsel karmaşıklığı azaltmak ve firmanın fiyat eşikleri değişse bile sistem yöneticilerinin sadece JSON dosyalarına odaklanmasını sağlamaktı.
* Polymorphic JSON Serialization ile JSON'dan gelen type'ları switch-case mantığında yazmak yerine polimorfik attribute'lar sayesinde bellekte otomatik oluşturdum.
* Composite Design Pattern ile iç içe geçebilen `And`, `Or`, `Not` gibi kuralları aynı abstract sınıftan (`RuleDefinition`) türettim. Bu sayede herhangi bir derinlik sınırı olmaksızın recursive ağaçlar (tree) oluşturdum.
* Kurallar değerlendirilirken teker teker parametreleri seçmek yerine tüm durumları `EngineContext` paketinde topladım. Bu sayede yeni bir kural gelse de `Evaluate` metodu değişmeden Context'e yeni alanlar eklenebilmesini sağladım.
* Fiyatlarda double ya da float yerine decimal kullandım. decimal bellekte 128 bitlik bir alan kaplasa da ticari ve borsa işlemlerinde yuvarlama hatalarını engellediği için çok daha güvenilirdir.
* Hafıza kuralları için `EngineContext` taşıyıcısı çok işlevlidir. Cooldown zamanını O(1) sürede okumak için Dictionary yapısı kurdum.
* SOLID prensiplerine uyularak sistemin çekirdeğine (`Evaluator` motoruna) dokunmadan sadece yeni kurallar eklemek plug-in mimarisi ile çok basit bir hale geldi. Ayrıca kural motorunun bağımsız doğası testleri de kolaylaştırdı.

## Karmaşıklık Hesaplarım ve Tercih Sebeplerim Neler?

**Time Complexity (Zaman Karmaşıklığı) Karşılaştırması:**
* **Geleneksel Yöntem:** **O(N)** olur. İşlemci, koda manuel olarak yazılan her bir if, else if, && ve || şartını sırayla kontrol etmek zorundadır. N adet şart varsa, en kötü senaryoda N adım atılır.
* **Bizim Sistemimiz:** Yine **O(N)** olur. Motor, ağaç yapısındaki kuralların en tepesinden başlayıp yapraklara inene kadar her kuralın `Evaluate` metodunu bir kez çağırır. Ağaçta toplam N adet kural düğümü varsa, tam N adet çağrı yapılır.

**Uzay Karmaşıklığı (Space Complexity) Karşılaştırması:**
* **Geleneksel Yöntem:** **O(1)** olur. Sadece temel değişkenler kullanıldığı için ekstra hafıza (RAM) tüketimi sabittir.
* **Bizim Sistemimiz:** **O(D)** olur. Recursive çağrılardan dolayı işlemcinin Call Stack (Çağrı Yığını) üzerinde kural ağacının derinliği (D) kadar ekstra hafıza tutması gerekir.

**Döngüsel Karmaşıklığın Karşılaştırılması:**
* **Geleneksel Yöntem:** **O(N)** olur. Tüm iş mantığının tek bir metodun içinde olduğu senaryolarda eklenen her yeni kural, kodun test edilmesi gereken farklı senaryolarını artırır ve kodu spagettiye çevirebilir. Ayrıca bakımı belli bir noktadan sonra aşırı zorlaşır.
* **Bizim Sistemimiz:** Sınıf başına **O(1)** olur. Sisteme 1000 yeni kural eklense dahi her sınıf kendinden sorumludur, önceden yazılan `AndRule` ya da `ChangeRule`'umuzu etkileyemez. Bu da bakımı ve testi oldukça kolay kılar.

**Tercih Sebebim:** OOP'nin doğasından gelen bir maliyet olsa da bu yöntemi seçme nedenim döngüsel karmaşıklığı iş büyümeden **O(1)**'e düşürmekti. Sistemin başlangıçta mikro düzeydeki performans kaybını ve mimari kurulum maliyetini bilerek göze aldım; ancak zaman ilerledikçe kodlarımın spagetti olmasını, bakımının ve okunabilirliğinin zorlaşmasını istemedim.

## Testte Neden xUnit Tercih Ettim?

Modern .NET projelerinde sektör standardı haline gelmesi başlıca sebeplerimden olsa da ana sebebim değildir. Microsoft bile .NET Runtime'ın kendi kaynak kodlarını ve ASP.NET Core kütüphanelerini test ederken xUnit kullanıyor. Sunulan diğer test framework'leri (NUnit ve MSTest) daha çok eski .NET Framework projelerinden gelen bir mirasa sahiptir. Sebepleri:
* **Test İzolasyonu Sağlaması:** Birim testlerde en istenmeyen durum testin arkasında bıraktığı veri veya durumların kendisinden sonra çalışan başka bir testi yanıltmasıdır. NUnit ve MSTest aynı test sınıfı örneğini birden fazla test için tekrar tekrar kullanabilir. Bu da testler arası durum sızıntılarına yol açabilir. xUnit ise her bir Fact ya da Theory metodu için test sınıfının yeni bir kopyasını oluşturarak testin işi bittiğinde çöpe atar. Bu sayede testleri birbirine karıştırma olasılığı sıfırdır.
* **OOP Doğasına Uyum:** NUnit ve MSTest testten önce çalışacak kodlar için kendi Setup ve Teardown attribute'larını oluşturuyor. xUnit ise C#'ın kendi doğasında halihazırda bulunan yapım için Constructor (Kurucu) ve temizlik için `IDisposable` arayüzünü kullanıyor.
* xUnit farklı test sınıflarını varsayılan olarak eş zamanlı çalıştırır. Test sayısı artsa bile bilgisayardaki tüm işlemci çekirdeklerini kullanarak testi saniyeler içinde tamamlar.

Kısacası xUnit her bir test metodu için "new instance per test" prensibi ile kusursuz bir izolasyon, uyumluluk ve hız sağladı. Yapay attribute'lar yerine Constructor ve IDisposable gibi OOP mantığını benimseyen C# projelerinde çok daha temiz bir test süreci sundu.

## Bölüm 4 Sorularının Cevapları

Eğer saniyede 10.000 veri işleme hedefimiz olsaydı, bu yükü Message-Driven bir mimariyle; RabbitMQ, Redis ve Elasticsearch kullanarak çözerdim. RabbitMQ anlık yük patlamalarında veriyi kuyruğa alır ve yatayda ölçeklenen consumer'lar bu yükü kolayca işleyebilirdi.

Ancak, sistemin bir borsa eşleştirme motoru olsaydı ve milisaniyelik gecikmelerin bile tolere edilemeyeceği bir senaryo yaşansaydı Event-Driven mimariye geçiş yapardım. Veri akışını Kafka ile yönetir, bellek içi kural değerlendirme aşamasında ise işletim sistemi kilitlerini aşarak donanımdan maksimum verim alan Ring Buffer teknolojisini kullanırdım. Bu mimari sayesinde işlem gecikmesi neredeyse sıfıra inerdi.

## Ufak Bir Not / Belirsiz Durumlar

Projede kuralları yazdıktan sonra derlemeye karar verdim ve sürekli ekranda *"Price is outside the comfortable trading zone."* mesajını gördüğümü fark ettim. Kurallarımı incelediğimde bir anormallik görmedim ve `NotRule` kuralını incelediğimde; içteki `RangeRule` kuralı şartnamede belirlenen bandın dışındakileri doğru (true) kabul edeceği için bu şekilde kodlamıştım. 

Daha sonra bu kural bir `NotRule` ile sarmalandığında, tersine çevirme mantığından dolayı aslında bandın "içindekileri" doğru kabul etmemiz ve mesajı ekrana yazdırmamız gerekiyordu. Bu durumu, piyasanın olağan dışı durgunlaştığı (fiyatın bant içinde sıkıştığı) senaryoları yakalamak adına sistemin beklenen ve doğru bir davranışı olarak yorumladım.

## Projenin Kurulumu ve Kullanımı

Bilgisayarınızda projede herhangi bir sorun yaşamamak adına **.NET 10** kurulu olmalıdır.
Eğer yüklü değilse [Microsoft .NET İndirme Sayfası](https://dotnet.microsoft.com/download) üzerinden işletim sisteminize uygun SDK'yı kurabilirsiniz.

Terminal veya komut satırınızı açın ve projeyi bilgisayarınıza indirin:
```bash
git clone [https://github.com/betulkaragoz/ElectricityPriceAlert.git](https://github.com/betulkaragoz/ElectricityPriceAlert.git)

Projeyi çalıştırmak için:

dotnet run --project PriceConsole -- data_prices.json rules_rules.json

Testleri çalıştırmak için:

dotnet test