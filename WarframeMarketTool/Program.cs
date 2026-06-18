using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;
using System.Text.Json.Serialization;
using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.WarframeMarketApi.Models.Versions;

// ===== 数据库路径 =====
var dbPath = Path.Combine(AppContext.BaseDirectory, "wfm_test.db");
Console.Error.WriteLine($"数据库: {dbPath}");

// ===== 建库 =====
using var db = new TestDb(dbPath);
await db.Database.EnsureCreatedAsync();
Console.Error.WriteLine("✅ 数据库就绪");

// ===== 1. 抓 API 物品列表 =====
Console.Error.WriteLine("\n=== 抓取 /v2/items ===");
var wfm = new WarframeMarketClient();
var itemsResp = await wfm.GetItemsAsync();
if (itemsResp?.Content?.Data == null)
{
    Console.Error.WriteLine($"❌ API 失败: HTTP {itemsResp?.StatusCode} / Content null");
    return;
}

var items = itemsResp.Content.Data;
Console.Error.WriteLine($"✅ 获取 {items.Length} 个物品");
Console.Error.WriteLine($"   API 版本: {itemsResp.Content.ApiVersion}");
Console.Error.WriteLine($"   第一个: {items[0].Slug}");

// ===== 2. 写入数据库 =====
Console.Error.WriteLine("\n=== 写入 Items 表 ===");
db.Items.RemoveRange(db.Items);
await db.Items.AddRangeAsync(items);
await db.SaveChangesAsync();
Console.Error.WriteLine($"✅ 写入 {items.Length} 行");

// ===== 3. 读回并验证 =====
Console.Error.WriteLine("\n=== 读回验证 ===");
var readBack = await db.Items.OrderBy(i => i.Slug).ToListAsync();
Console.Error.WriteLine($"✅ 读回 {readBack.Count} 条");

// 验证前 3 条
for (int i = 0; i < Math.Min(3, readBack.Count); i++)
{
    var original = items.First(x => x.Id == readBack[i].Id);
    Console.Error.WriteLine($"\n  [{i}] {readBack[i].Slug}");
    Console.Error.WriteLine($"     Tags: {string.Join(", ", readBack[i].Tags)}");
    Console.Error.WriteLine($"     Tags 匹配: {original.Tags.SetEquals(readBack[i].Tags)}");
    Console.Error.WriteLine($"     I18n 语言数: {readBack[i].I18n.Count}");
    Console.Error.WriteLine($"     I18n 匹配: {original.I18n.Count == readBack[i].I18n.Count}");
    Console.Error.WriteLine($"     Subtypes: {(readBack[i].Subtypes?.Count > 0 ? string.Join(", ", readBack[i].Subtypes!) : "(空)")}");
}

// ===== 4. 统计子类型 =====
var allSubtypes = new ItemSubtypeSet();
foreach (var item in readBack.Where(i => i.Subtypes != null))
    foreach (var st in item.Subtypes!)
        allSubtypes.Add(st);
Console.Error.WriteLine($"\n=== 子类型汇总 ({allSubtypes.Count} 个不重复值) ===");
foreach (var st in allSubtypes.OrderBy(x => x))
    Console.Error.WriteLine($"   \"{st}\",");

// OfType 需要 TPH 配置，跳过
Console.Error.WriteLine("\n   TPH 继承检查跳过（列表 API 只返回 ItemShort）");

// ===== 6. 清理 =====
Console.Error.WriteLine("\n✅ 完成");

// ===== EF Core DbContext =====
public class TestDb : DbContext
{
	public DbSet<ItemShort> Items => Set<ItemShort>();

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

			e.Property(i => i.I18n).HasConversion(
				v => JsonSerializer.Serialize(v, JsonOpts),
				v => JsonSerializer.Deserialize<Dictionary<Language, LanguagePake>>(v, JsonOpts) ?? new())
				.Metadata.SetValueComparer(new ValueComparer<Dictionary<Language, LanguagePake>>(
					(c1, c2) => c1!.Count == c2!.Count && !c1.Except(c2).Any(),
					c => c.Aggregate(0, (a, kv) => HashCode.Combine(a, kv.Key.GetHashCode(), kv.Value.GetHashCode())),
					c => new Dictionary<Language, LanguagePake>(c)));
		});
	}
}
