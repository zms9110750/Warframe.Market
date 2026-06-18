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

	public WfmDbContext(DbContextOptions<WfmDbContext> options) : base(options) { }

	private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<ServerVersion>(e =>
		{
			e.HasKey(v => v.Id);
			e.Ignore(v => v.UpdatedAtLocal);
		});

		modelBuilder.Entity<ItemShort>(e =>
		{
			e.HasKey(i => i.Id);
			e.Ignore(i => i.I18n); // 用 SQL 查 ItemTranslations 表填充

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

			e.HasDiscriminator<string>("ItemType")
				.HasValue<ItemShort>(nameof(ItemShort))
				.HasValue<Item>(nameof(Item));
		});

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

		modelBuilder.Entity<ItemSet>(e =>
		{
			e.HasKey(s => s.Id);
			e.Property(s => s.Items).HasConversion(
				v => JsonSerializer.Serialize(v, JsonOpts),
				v => JsonSerializer.Deserialize<Item[]>(v, JsonOpts) ?? Array.Empty<Item>());
		});

		modelBuilder.Entity<Entry>(e =>
		{
			e.HasKey(x => x.Id);
		});

		// ===== 翻译表：keyless，原始 SQL 查 → LanguagePake =====
		modelBuilder.Entity<TranslationRow>(e =>
		{
			e.HasNoKey();
			e.ToTable("ItemTranslations");
			e.Property(t => t.ItemId).HasMaxLength(64);
			e.Property(t => t.Language).HasMaxLength(16);
			e.HasIndex(t => new { t.ItemId, t.Language }).IsUnique();
		});
	}
}

/// <summary>
/// 翻译表行（keyless entity，用于 SqlQuery&lt;LanguagePake&gt;）
/// </summary>
public class TranslationRow
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
