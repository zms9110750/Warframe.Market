using Microsoft.EntityFrameworkCore;

namespace WarframeMarketApp.Data;

public class WfmDbContext : DbContext
{
	public DbSet<CachedVersion> VersionInfos => Set<CachedVersion>();
	public DbSet<CachedItemBase> Items => Set<CachedItemBase>();
	public DbSet<CachedItemDetail> ItemDetails => Set<CachedItemDetail>();
	public DbSet<CachedSet> ItemSets => Set<CachedSet>();
	public DbSet<CachedStatEntry> StatEntries => Set<CachedStatEntry>();
	public DbSet<CachedItemLocalization> ItemLocalizations => Set<CachedItemLocalization>();

	public WfmDbContext(DbContextOptions<WfmDbContext> options) : base(options) { }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		// ===== VersionInfo =====
		modelBuilder.Entity<CachedVersion>(e =>
		{
			e.HasKey(v => v.Id);
			e.Property(v => v.CollectionsJson).HasColumnName("Collections");
		});

		// ===== Items (TPH) =====
		modelBuilder.Entity<CachedItemBase>(e =>
		{
			e.HasKey(i => i.Id);
			e.Property(i => i.TagsJson).HasColumnName("Tags");
			e.Property(i => i.SubtypesJson).HasColumnName("Subtypes");
			e.Property(i => i.SetPartsJson).HasColumnName("SetParts");

			e.HasDiscriminator<string>("ItemType")
				.HasValue<CachedItemBase>(nameof(CachedItemBase))
				.HasValue<CachedItemDetail>(nameof(CachedItemDetail));
		});

		// ===== ItemSets =====
		modelBuilder.Entity<CachedSet>(e =>
		{
			e.HasKey(s => s.Id);
			e.Property(s => s.ItemIdsJson).HasColumnName("ItemIds");
		});

		// ===== StatEntries =====
		modelBuilder.Entity<CachedStatEntry>(e =>
		{
			e.HasKey(s => s.Id);
			e.HasIndex(s => s.ItemId);
			e.HasIndex(s => s.Datetime);
		});

		// ===== ItemLocalizations (FK → Items) =====
		modelBuilder.Entity<CachedItemLocalization>(e =>
		{
			e.HasKey(l => l.Id);
			e.HasIndex(l => new { l.ItemId, l.Language }).IsUnique();
			e.Property(l => l.Id).ValueGeneratedOnAdd();
		});
	}
}
