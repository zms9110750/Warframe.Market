using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;
using System.Text.Json.Serialization;
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
Console.Error.WriteLine($"✅ 获取 {items.Length} 个物品");

Console.Error.WriteLine("\n=== 写入 Items + ItemLocalizations ===");
db.Items.RemoveRange(db.Items);
db.ItemLocalizations.RemoveRange(db.ItemLocalizations);

await db.Items.AddRangeAsync(items);

// I18n 字典展平写入 ItemLocalizations 表
var localizations = new List<ItemLocalization>();
foreach (var item in items)
{
	foreach (var (lang, pake) in item.I18n)
	{
		localizations.Add(new ItemLocalization
		{
			ItemId = item.Id,
			Language = lang.ToString(),
			Name = pake.Name,
			Description = pake.Description,
			WikiLink = pake.WikiLink,
			Icon = pake.Icon,
			Thumb = pake.Thumb,
			SubIcon = pake.SubIcon,
		});
	}
}
await db.ItemLocalizations.AddRangeAsync(localizations);
await db.SaveChangesAsync();
Console.Error.WriteLine($"✅ Items: {items.Length} 行");
Console.Error.WriteLine($"✅ ItemLocalizations: {localizations.Count} 行");

Console.Error.WriteLine("\n=== 读回验证 ===");
var readBack = await db.Items.OrderBy(i => i.Slug).ToListAsync();
Console.Error.WriteLine($"✅ Items: {readBack.Count}");

var locs = await db.ItemLocalizations.Take(3).ToListAsync();
foreach (var loc in locs)
	Console.Error.WriteLine($"   {loc.ItemId} [{loc.Language}] = {loc.Name}");

Console.Error.WriteLine("\n=== 子类型汇总 ===");
var allSubtypes = new ItemSubtypeSet();
foreach (var item in readBack.Where(i => i.Subtypes != null))
	foreach (var st in item.Subtypes!)
		allSubtypes.Add(st);
Console.Error.WriteLine($"共 {allSubtypes.Count} 个");
foreach (var st in allSubtypes.OrderBy(x => x))
	Console.Error.WriteLine($"   \"{st}\",");

Console.Error.WriteLine("\n✅ 完成");

public class ItemLocalization
{
	public string ItemId { get; set; } = "";
	public string Language { get; set; } = "";
	public string Name { get; set; } = "";
	public string? Description { get; set; }
	public string? WikiLink { get; set; }
	public string Icon { get; set; } = "";
	public string Thumb { get; set; } = "";
	public string? SubIcon { get; set; }
}

public class TestDb : DbContext
{
	public DbSet<ItemShort> Items => Set<ItemShort>();
	public DbSet<ItemLocalization> ItemLocalizations => Set<ItemLocalization>();

	private readonly string _dbPath;
	public TestDb(string dbPath) => _dbPath = dbPath;
	protected override void OnConfiguring(DbContextOptionsBuilder options)
		=> options.UseSqlite($"Data Source={_dbPath}");

	private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<ItemShort>(e =>
		{
			e.HasKey(i => i.Id);

			e.Property(i => i.I18n).HasConversion(
				v => JsonSerializer.Serialize(v, JsonOpts),
				v => JsonSerializer.Deserialize<Dictionary<Language, LanguagePake>>(v, JsonOpts) ?? new())
				.Metadata.SetValueComparer(new ValueComparer<Dictionary<Language, LanguagePake>>(
					(c1, c2) => c1!.Count == c2!.Count && !c1.Except(c2).Any(),
					c => c.Aggregate(0, (a, kv) => HashCode.Combine(a, kv.Key.GetHashCode(), kv.Value.GetHashCode())),
					c => new Dictionary<Language, LanguagePake>(c)));

			e.Property(i => i.Tags).HasConversion(
				v => JsonSerializer.Serialize(v, JsonOpts),
				v => JsonSerializer.Deserialize<HashSet<string>>(v, JsonOpts) ?? new())
				.Metadata.SetValueComparer(new ValueComparer<HashSet<string>>(
					(c1, c2) => c1!.SetEquals(c2!),
					c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v!.GetHashCode())),
					c => new HashSet<string>(c)));

			e.Property(i => i.Subtypes).HasConversion(
				v => JsonSerializer.Serialize(v, JsonOpts),
				v => JsonSerializer.Deserialize<ItemSubtypeSet>(v, JsonOpts) ?? new())
				.Metadata.SetValueComparer(new ValueComparer<ItemSubtypeSet>(
					(c1, c2) => c1!.SetEquals(c2!),
					c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v!.GetHashCode())),
					c => new ItemSubtypeSet()));
		});

		modelBuilder.Entity<ItemLocalization>(e =>
		{
			e.HasKey(l => new { l.ItemId, l.Language });
			e.Property(l => l.Language).HasMaxLength(16);
		});
	}
}
