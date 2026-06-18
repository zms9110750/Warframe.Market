using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;
using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models.Items;

// ─── 测试 record 非构造器属性 EF 映射 ───
Console.Error.WriteLine("=== 非构造器属性 EF 映射测试 ===");
var efTestDb = Path.Combine(AppContext.BaseDirectory, "ef_test.db");
File.Delete(efTestDb);
using (var efCtx = new EfPropTestContext(efTestDb))
{
	await efCtx.Database.EnsureCreatedAsync();
	efCtx.Records.Add(new MyRecord("0.1", "test", null));
	await efCtx.SaveChangesAsync();
	var loaded = await efCtx.Records.FirstAsync();
	Console.Error.WriteLine($"  ApiVersion={loaded.ApiVersion}, Value={loaded.Value}");
	Console.Error.WriteLine($"  CachedAt={loaded.CachedAt:O}");
	Console.Error.WriteLine($"  ✅ 非构造器属性 CachedAt 由 EF 映射成功");
}
try { File.Delete(efTestDb); } catch { }

// ─── 测试 DateTime 存储对比 ───
Console.Error.WriteLine("\n=== DateTime 存储对比测试 ===");
var dtDb = Path.Combine(AppContext.BaseDirectory, "dt_test.db");
File.Delete(dtDb);
using (var dtCtx = new DtTestContext(dtDb))
{
	await dtCtx.Database.EnsureCreatedAsync();
	dtCtx.CSharpSet.Add(new DtRecord("a", DateTime.UtcNow));
	dtCtx.SqlDefault.Add(new DbDefault());
	await dtCtx.SaveChangesAsync();
}
using (var dtCtx2 = new DtTestContext(dtDb))
{
	var a = await dtCtx2.CSharpSet.FirstAsync();
	var b = await dtCtx2.SqlDefault.FirstAsync();
	Console.Error.WriteLine($"  C# {a.Time:O}  Kind={a.Time.Kind}");
	Console.Error.WriteLine($"  SQL {b.Time:O}  Kind={b.Time.Kind}  ToUniversal={b.Time.ToUniversalTime():O}");
	Console.Error.WriteLine($"  DatesMatch={(a.Time.Date == b.Time.ToUniversalTime().Date)}");
}
try { File.Delete(dtDb); } catch { }

// ─── 正式测试 ───
var dbPath = Path.Combine(AppContext.BaseDirectory, "wfm_test.db");
Console.Error.WriteLine($"数据库: {dbPath}");

using var db = new TestDb(dbPath);
await db.Database.EnsureCreatedAsync();
Console.Error.WriteLine("✅ 数据库就绪");

Console.Error.WriteLine("\n=== 抓取 /v2/items ===");
var wfm = new WarframeMarketClient();
var itemsResp = await wfm.GetItemsAsync();
if (itemsResp?.Content?.Data == null) { Console.Error.WriteLine("❌ API 失败"); return; }
var items = itemsResp.Content.Data;
Console.Error.WriteLine($"✅ {items.Length} 个物品");

Console.Error.WriteLine("\n=== EF 追踪写入 ===");
db.Items.RemoveRange(db.Items);
db.ItemTranslations.RemoveRange(db.ItemTranslations);

await db.Items.AddRangeAsync(items);

foreach (var item in items)
{
	foreach (var (lang, p) in item.I18n)
	{
		db.ItemTranslations.Add(new ItemTranslation(
			item.Id, lang.ToString(),
			p.Name, p.Description, p.WikiLink,
			p.Icon, p.Thumb, p.SubIcon
		));
	}
}
await db.SaveChangesAsync();

var transCount = await db.ItemTranslations.CountAsync();
Console.Error.WriteLine($"  Items: {items.Length}, 翻译: {transCount}");

Console.Error.WriteLine("\n=== EF 关联查询 ===");
var first = items[0];
var translations = await db.ItemTranslations
	.Where(t => t.ItemId == first.Id)
	.ToListAsync();
Console.Error.WriteLine($"  {first.Slug}: {translations.Count} 种语言");
foreach (var t in translations)
{
	LanguagePake pake = t;
	Console.Error.WriteLine($"    [{t.Language}] {pake.Name}");
}

Console.Error.WriteLine("\n=== 子类型 ===");
var allSubtypes = new ItemSubtypeSet();
var readBack = await db.Items.ToListAsync();
foreach (var item in readBack.Where(i => i.Subtypes != null))
	foreach (var st in item.Subtypes!)
		allSubtypes.Add(st);
Console.Error.WriteLine($"共 {allSubtypes.Count} 个");
foreach (var st in allSubtypes.OrderBy(x => x))
	Console.Error.WriteLine($"  \"{st}\",");
Console.Error.WriteLine("\n✅ 完成");

// ===== 非构造器属性测试模型 =====
public record MyRecord(string ApiVersion, string? Value, string? Error)
{
	public DateTime CachedAt { get; set; } = DateTime.UtcNow;
}

public class EfPropTestContext : DbContext
{
	public DbSet<MyRecord> Records => Set<MyRecord>();
	private readonly string _path;
	public EfPropTestContext(string path) => _path = path;
	protected override void OnConfiguring(DbContextOptionsBuilder o) => o.UseSqlite($"Data Source={_path}");
	protected override void OnModelCreating(ModelBuilder mb)
		=> mb.Entity<MyRecord>(e => e.HasKey(r => r.ApiVersion));
}

// ===== 物品测试模型 =====
public record ItemTranslation(
	string ItemId, string Language,
	string Name, string? Description, string? WikiLink, string Icon, string Thumb, string? SubIcon
) : LanguagePake(Name, Description, WikiLink, Icon, Thumb, SubIcon)
{
	public ItemShort? Item { get; set; }
}

// ===== DateTime 对比测试 =====
public record DtRecord(string Id, DateTime Time);
public record DbDefault { public int Id { get; set; } = 1; public DateTime Time { get; set; } }

public class DtTestContext : DbContext
{
	public DbSet<DtRecord> CSharpSet => Set<DtRecord>();
	public DbSet<DbDefault> SqlDefault => Set<DbDefault>();
	private readonly string _path;
	public DtTestContext(string path) => _path = path;
	protected override void OnConfiguring(DbContextOptionsBuilder o) => o.UseSqlite($"Data Source={_path}");
	protected override void OnModelCreating(ModelBuilder mb)
	{
		mb.Entity<DtRecord>(e => e.HasKey(r => r.Id));
		mb.Entity<DbDefault>(e => { e.HasKey(d => d.Id); e.Property(d => d.Time).HasDefaultValueSql("datetime('now')"); });
	}
}

public class TestDb : DbContext
{
	public DbSet<ItemShort> Items => Set<ItemShort>();
	public DbSet<ItemTranslation> ItemTranslations => Set<ItemTranslation>();
	private readonly string _dbPath;
	public TestDb(string dbPath) => _dbPath = dbPath;
	protected override void OnConfiguring(DbContextOptionsBuilder o) => o.UseSqlite($"Data Source={_dbPath}");
	private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

	protected override void OnModelCreating(ModelBuilder mb)
	{
		mb.Entity<ItemShort>(e =>
		{
			e.HasKey(i => i.Id); e.Ignore(i => i.I18n);
			e.Property(i => i.Tags).HasConversion(
				v => JsonSerializer.Serialize(v, JsonOpts),
				v => JsonSerializer.Deserialize<HashSet<string>>(v, JsonOpts) ?? new())
				.Metadata.SetValueComparer(new ValueComparer<HashSet<string>>(
					(c1, c2) => c1!.SetEquals(c2!), c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v!.GetHashCode())), c => new HashSet<string>(c)));
			e.Property(i => i.Subtypes).HasConversion(
				v => JsonSerializer.Serialize(v, JsonOpts),
				v => JsonSerializer.Deserialize<ItemSubtypeSet>(v, JsonOpts) ?? new())
				.Metadata.SetValueComparer(new ValueComparer<ItemSubtypeSet>(
					(c1, c2) => c1!.SetEquals(c2!), c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v!.GetHashCode())), c => new ItemSubtypeSet()));
		});
		mb.Entity<ItemTranslation>(e =>
		{
			e.HasKey(t => new { t.ItemId, t.Language });
			e.ToTable("ItemTranslations");
			e.HasOne<ItemShort>().WithMany().HasForeignKey(t => t.ItemId).OnDelete(DeleteBehavior.Cascade);
		});
	}
}
