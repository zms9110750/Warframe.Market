using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;
using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models.Items;

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

// ===== 模型 =====
public record ItemTranslation(
	string ItemId, string Language,
	string Name, string? Description, string? WikiLink, string Icon, string Thumb, string? SubIcon
) : LanguagePake(Name, Description, WikiLink, Icon, Thumb, SubIcon)
{
	public ItemShort? Item { get; set; }
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
