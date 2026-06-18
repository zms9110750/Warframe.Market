using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;
using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.WarframeMarketApi.Models.Versions;

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

Console.Error.WriteLine("\n=== 写入 Items + ItemTranslations ===");
db.Items.RemoveRange(db.Items);
await db.Database.ExecuteSqlRawAsync("DELETE FROM ItemTranslations");
await db.Items.AddRangeAsync(items);
await db.SaveChangesAsync();

// 原生 SQL 批量写入翻译
var insertSql = @"INSERT INTO ItemTranslations (ItemId, Language, Name, Description, WikiLink, Icon, Thumb, SubIcon)
	VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})";
int transCount = 0;
foreach (var item in items)
{
	foreach (var (lang, p) in item.I18n)
	{
		await db.Database.ExecuteSqlRawAsync(insertSql,
			item.Id, lang.ToString(),
			p.Name, p.Description, p.WikiLink,
			p.Icon, p.Thumb, p.SubIcon);
		transCount++;
	}
}
Console.Error.WriteLine($"  Items: {items.Length}, 翻译: {transCount}");

Console.Error.WriteLine("\n=== 查询验证 ===");
var first = items[0];

// 查为 LanguagePake
var pake = await db.Database.SqlQueryRaw<LanguagePake>(
	"SELECT Name, Description, WikiLink, Icon, Thumb, SubIcon FROM ItemTranslations WHERE ItemId = {0} AND Language = {1}",
	first.Id, "ZhHans").FirstOrDefaultAsync();
Console.Error.WriteLine($"  LanguagePake: {first.Slug} zh-hans = {pake?.Name}");

// 查为 ItemTranslation（也是 LanguagePake）
var row = await db.Database.SqlQueryRaw<ItemTranslation>(
	"SELECT ItemId, Language, Name, Description, WikiLink, Icon, Thumb, SubIcon FROM ItemTranslations WHERE ItemId = {0} AND Language = {1}",
	first.Id, "En").FirstOrDefaultAsync();
if (row != null)
{
	LanguagePake asPake = row; // 隐式转换验证
	Console.Error.WriteLine($"  ItemTranslation: {row.ItemId} [{row.Language}] = {asPake.Name}");
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

// ===== 本地类型继承 LanguagePake =====
public record ItemTranslation(
	string ItemId, string Language,
	string Name, string? Description, string? WikiLink, string Icon, string Thumb, string? SubIcon
) : LanguagePake(Name, Description, WikiLink, Icon, Thumb, SubIcon);

public class TestDb : DbContext
{
	public DbSet<ItemShort> Items => Set<ItemShort>();
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
		mb.Entity<ItemTranslation>(e => { e.HasNoKey(); e.ToTable("ItemTranslations"); });
	}
}
