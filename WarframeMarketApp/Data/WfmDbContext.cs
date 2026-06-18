using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;
using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.WarframeMarketApi.Models.Statistics;
using zms9110750.WarframeMarketApi.Models.Versions;

namespace WarframeMarketApp.Data;

public class WfmDbContext : DbContext
{
	public DbSet<ServerVersion> VersionInfos => Set<ServerVersion>();
	public DbSet<ItemShort> Items => Set<ItemShort>();
	public DbSet<Item> ItemDetails => Set<Item>();
	public DbSet<ItemSet> ItemSets => Set<ItemSet>();
	public DbSet<Entry> StatEntries => Set<Entry>();
	public DbSet<LocalizationRecord> ItemLocalizations => Set<LocalizationRecord>();

	public WfmDbContext(DbContextOptions<WfmDbContext> options) : base(options) { }

	private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		// ===== ServerVersion =====
		modelBuilder.Entity<ServerVersion>(e =>
		{
			e.HasKey(v => v.Id);
			e.Ignore(v => v.UpdatedAtLocal);
		});

		// ===== ItemShort / Item (TPH) =====
		modelBuilder.Entity<ItemShort>(e =>
		{
			e.HasKey(i => i.Id);

			// 复杂类型 → JSON TEXT
			e.Property(i => i.Tags).HasConversion(
				v => JsonSerializer.Serialize(v, JsonOpts),
				v => JsonSerializer.Deserialize<HashSet<string>>(v, JsonOpts) ?? new())
				.Metadata.SetValueComparer(new ValueComparer<HashSet<string>>(
					(c1, c2) => c1!.SetEquals(c2!),
					c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
					c => new HashSet<string>(c)));

			e.Property(i => i.Subtypes).HasConversion(
				v => JsonSerializer.Serialize(v, JsonOpts),
				v => JsonSerializer.Deserialize<ItemSubtypeSet>(v, JsonOpts) ?? new())
				.Metadata.SetValueComparer(new ValueComparer<ItemSubtypeSet>(
					(c1, c2) => c1!.SetEquals(c2!),
					c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
					c => new ItemSubtypeSet()));

			e.Property(i => i.I18n).HasConversion(
				v => JsonSerializer.Serialize(v, JsonOpts),
				v => JsonSerializer.Deserialize<Dictionary<Language, LanguagePake>>(v, JsonOpts) ?? new())
				.Metadata.SetValueComparer(new ValueComparer<Dictionary<Language, LanguagePake>>(
					(c1, c2) => c1!.Count == c2!.Count && !c1.Except(c2).Any(),
					c => c.Aggregate(0, (a, kv) => HashCode.Combine(a, kv.Key.GetHashCode(), kv.Value.GetHashCode())),
					c => new Dictionary<Language, LanguagePake>(c)));

			e.HasDiscriminator<string>("ItemType")
				.HasValue<ItemShort>(nameof(ItemShort))
				.HasValue<Item>(nameof(Item));
		});

		// ===== Item (额外字段) =====
		modelBuilder.Entity<Item>(e =>
		{
			e.Property(i => i.SetParts).HasConversion(
				v => JsonSerializer.Serialize(v, JsonOpts),
				v => JsonSerializer.Deserialize<HashSet<string>>(v, JsonOpts) ?? new())
				.Metadata.SetValueComparer(new ValueComparer<HashSet<string>>(
					(c1, c2) => c1!.SetEquals(c2!),
					c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
					c => new HashSet<string>(c)));
		});

		// ===== ItemSet =====
		modelBuilder.Entity<ItemSet>(e =>
		{
			e.HasKey(s => s.Id);
			e.Property(s => s.Items).HasConversion(
				v => JsonSerializer.Serialize(v, JsonOpts),
				v => JsonSerializer.Deserialize<Item[]>(v, JsonOpts) ?? Array.Empty<Item>());
		});

		// ===== Entry (统计) =====
		modelBuilder.Entity<Entry>(e =>
		{
			e.HasKey(x => x.Id);
			e.HasIndex(x => x.Id);
		});

		// ===== 本地化记录 =====
		modelBuilder.Entity<LocalizationRecord>(e =>
		{
			e.HasKey(l => l.Id);
			e.HasIndex(l => new { l.ItemId, l.Language }).IsUnique();
			e.Property(l => l.Id).ValueGeneratedOnAdd();
		});
	}
}

/// <summary>
/// 物品多语言本地化（单独 FK 表）
/// </summary>
public class LocalizationRecord
{
	public long Id { get; set; }
	public string ItemId { get; set; } = "";
	public string Language { get; set; } = "";
	public string Name { get; set; } = "";
	public string? Description { get; set; }
	public string? WikiLink { get; set; }
	public string Icon { get; set; } = "";
	public string Thumb { get; set; } = "";
	public string? SubIcon { get; set; }
}
