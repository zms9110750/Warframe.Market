using Microsoft.EntityFrameworkCore;
using Warframe.Market.Model.LocalItems;
using Warframe.Market.Extend;
using Warframe.Market.Model.Items;
namespace Warframe.Market.Helper.CacheAndClient;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using System.Threading.Tasks;
using Warframe.Market.Helper.Abstract;
using Warframe.Market.Model.ItemsSet;
using Warframe.Market.Model.Statistics;
using Warframe.Market.Model.Versions;

public class CacheDbContext : DbContext, ICacheLoacd
{
	const string DirectoryPath = "Cache(CanDeleted)";
	const string Day90 = nameof(Period.Day90);
	const string Hour48 = nameof(Period.Hour48);
	public DbSet<ItemShort> ItemSet { get; set; }
	public DbSet<Language> LanguageSet { get; set; }
	public DbSet<Entry> StatisticEntrySet { get; set; }
	public DbSet<Version> VersionSet { get; set; }
	AsyncLock AsyncLock { get; } = new AsyncLock();

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		Directory.CreateDirectory(DirectoryPath);
		if (!optionsBuilder.IsConfigured)
		{
			optionsBuilder.UseSqlite($"Data Source={DirectoryPath}/cache.sqlite");
		}
	}
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<ItemShort>(ItemShort =>
		{
			ItemShort
				.Property(s => s.Tags)
				.HasJsonConversion();

			ItemShort
				.Property(s => s.Subtypes)
				.HasJsonConversion();

			ItemShort
				.HasDiscriminator<string>(nameof(GetType))
				.HasValue<ItemShort>(nameof(ItemShort))
				.HasValue<Item>(nameof(Item));

			ItemShort
				.Property<DateTime?>("Statistic");

			ItemShort
				.Ignore(s => s.I18n);
		});
		modelBuilder.Entity<Language>(languageEntity =>
		{
			languageEntity.Property<string>("ItemShortId");
			languageEntity.Property<string>("Key");

			// 设置联合主键
			languageEntity.HasKey("ItemShortId", "Key");

			// 设置外键约束
			languageEntity.HasOne<ItemShort>()
			   .WithMany()
			   .HasForeignKey("ItemShortId");
		});
		modelBuilder.Entity<Entry>(entry =>
		{
			entry.Property<string>("Pertain");
			entry.HasKey("Id");

			// 配置 B 到 A 的外键关系
			entry.HasOne<ItemShort>()
				.WithMany()
				.HasForeignKey("ItemShortId") // 定义影子外键属性名
				.OnDelete(DeleteBehavior.Restrict);
		});

		modelBuilder.Entity<Version>(entry =>
		{
			entry.HasKey(s => s.ApiVersion);
			entry.Property(s => s.Data)
				.HasJsonConversion();
			entry.Property(s => s.Error)
				.IsRequired(false);
		});
	}
	public async Task SetItemCacheAsync(ItemCache cache, Version version, CancellationToken cancellation = default)
	{
		using var scope = await AsyncLock.LockAsync(cancellation);
		await Database.EnsureDeletedAsync(cancellation);
		await Database.EnsureCreatedAsync(cancellation);
		await ItemSet.AddRangeAsync(cache.Data, cancellation);
		foreach (var item in cache.Data)
		{
			await ItemSet.AddAsync(item, cancellation);
			foreach (var lang in item.I18n)
			{
				LanguageSet.Entry(lang.Value).Property<string>("ItemShortId").CurrentValue = item.Id;
				LanguageSet.Entry(lang.Value).Property<string>("Key").CurrentValue = lang.Key;
				await LanguageSet.AddAsync(lang.Value, cancellation);
			}
		}
		await VersionSet.ExecuteDeleteAsync(cancellation);
		await VersionSet.AddAsync(version, cancellation);
	}
	public async Task<ItemCache?> GetItemsCacheAsync(CancellationToken cancellation = default)
	{
		Dictionary<string, ItemShort>? itemArr;
		using (var scope = await AsyncLock.LockAsync(cancellation))
		{
			await Database.EnsureCreatedAsync(cancellation);
			if (!await Database.CanConnectAsync(cancellation) || !await VersionSet.AnyAsync(cancellation))
			{
				return null;
			}
			itemArr = await ItemSet.ToDictionaryAsync(a => a.Id, cancellation);
		}
		if (itemArr.Count < 3000)
		{
			return null;
		}
		else
		{
			ILookup<string, (string id, string key, Language s)>? b;
			using (var scope = await AsyncLock.LockAsync(cancellation))
			{
				var c = await LanguageSet
					.Select(s => new { id = EF.Property<string>(s, "ItemShortId"), key = EF.Property<string>(s, "Key"), s })
					.ToArrayAsync(cancellation);
				b = c.Select(s => (s.id, s.key, s.s)).ToLookup(s => s.id);
			}
			Parallel.ForEach(itemArr, item =>
				item.Value.I18n = new ItemI18n(b[item.Key].ToDictionary(s => s.key, s => s.s)));
			return new ItemCache(VersionSet.First().ApiVersion, itemArr.Values.ToArray(), null!);
		}
	}
	public async Task SetFullItemAsync(Item item, CancellationToken cancellation = default)
	{
		using var scope = await AsyncLock.LockAsync(cancellation);
		ItemSet.Update(item);
		await ItemSet.Where(s => s.Id == item.Id)
			.ExecuteUpdateAsync(s => s.SetProperty(s => EF.Property<string>(s, nameof(GetType)), nameof(Item)), cancellationToken: cancellation);
		await SaveChangesAsync(cancellation);
	}
	public async Task SetFullItemAsync(IEnumerable<Item> item, CancellationToken cancellation = default)
	{
		using var scope = await AsyncLock.LockAsync(cancellation);
		ItemSet.UpdateRange(item);
		var hash = item.Select(s => s.Id).ToHashSet();
		await ItemSet.Where(s => hash.Contains(s.Id))
			.ExecuteUpdateAsync(s => s.SetProperty(s => EF.Property<string>(s, nameof(GetType)), nameof(Item)), cancellationToken: cancellation);
		await SaveChangesAsync(cancellation);
	}
	public async Task<Item?> GetItemAsync(ItemShort itemShort, CancellationToken cancellation = default)
	{
		if (itemShort is Item item)
		{
			return item;
		}
		using var scope = await AsyncLock.LockAsync(cancellation);
		return await ItemSet.FirstAsync(s => s.Id == itemShort.Id, cancellation) as Item;
	}
	public async Task SetStatisticAsync(ItemShort itemShort, Statistic statistic, CancellationToken cancellation = default)
	{
		using var scope = await AsyncLock.LockAsync(cancellation); 
		await StatisticEntrySet.Where(s => EF.Property<string>(s, "ItemShortId") == itemShort.Id).ExecuteDeleteAsync(cancellation);
		await CreatDiscriminator(statistic.Payload.StatisticsLive.Day90, Day90);
		await CreatDiscriminator(statistic.Payload.StatisticsLive.Hour48, Hour48);
		await CreatDiscriminator(statistic.Payload.StatisticsClosed.Day90, Day90);
		await CreatDiscriminator(statistic.Payload.StatisticsClosed.Hour48, Hour48);
		await SaveChangesAsync(cancellation);
		await ItemSet.Where(s => s.Id == itemShort.Id)
				.ExecuteUpdateAsync(s => s.SetProperty(e => EF.Property<DateTime?>(e, "Statistic"), DateTime.Now), cancellationToken: cancellation);
		async Task CreatDiscriminator(IEnumerable<Entry> items, string Pertain)
		{ 
				await StatisticEntrySet.AddRangeAsync(items, cancellation); 
			Parallel.ForEach(items, item =>
		   {
			   Entry(item).Property<string>("ItemShortId").CurrentValue = itemShort.Id;
			   Entry(item).Property<string>("Pertain").CurrentValue = Pertain;
		   });
		}
	}
	public async Task<Statistic?> GetStatisticsAsync(ItemShort itemShort, CancellationToken cancellation = default)
	{
		ILookup<(string Pertain, bool), Entry>? group;
		using (var scope = await AsyncLock.LockAsync(cancellation))
		{ 
			await Entry(itemShort).ReloadAsync(cancellation);
			if (!(DateTime.Now - Entry(itemShort).Property<DateTime?>("Statistic").CurrentValue < TimeSpan.FromHours(2)))
			{
				return null;
			}
			var arr = await StatisticEntrySet.Where(s => EF.Property<string>(s, "ItemShortId") == itemShort.Id)
				.Select(entry => new { entry, Pertain = EF.Property<string>(entry, "Pertain") })
				.ToArrayAsync(cancellationToken: cancellation);
			group = arr
				.ToLookup(s => (s.Pertain, s.entry.OrderType == null)
				, s => s.entry);
		}
		var hour48close = group[(Hour48, true)].ToArray();
		var day90close = group[(Day90, true)].ToArray();
		var hour48live = group[(Hour48, false)].ToArray();
		var day90live = group[(Day90, false)].ToArray(); 
		return new Statistic(new Payload(new Period(hour48close, day90close), new Period(hour48live, day90live)));
	}
	public async Task<Version> GetVersionAsync(CancellationToken cancellation = default)
	{
		using var scope = await AsyncLock.LockAsync(cancellation);
		return await VersionSet.FirstOrDefaultAsync(cancellation);
	}

	public Task<ItemSet?> GetItemSetAsync(ItemShort item, CancellationToken cancellation = default)
	{
		return Task.FromResult<ItemSet?>(null);
	}
}