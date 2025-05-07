using System.Net;
using Autofac;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using System.Threading.RateLimiting;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Polly.RateLimiting; 

namespace Warframe.Market.Helper.AutofacModule;
public sealed class ResilientHttpModule : Module
{
	public const string Key = "WarframeMarket";
	protected override void Load(ContainerBuilder builder)
	{
		var service = new ServiceCollection();
		service.AddHttpClient(Key, client =>
		{
			client.BaseAddress = new Uri("https://api.warframe.market");
			client.DefaultRequestHeaders.Add("Language", "zh-hans");
		})
	   .AddResilienceHandler("429retry", builder =>
	   {
		   var rateLimiter = new TokenBucketRateLimiter(
			   new TokenBucketRateLimiterOptions
			   {
				   TokenLimit = 6,
				   TokensPerPeriod = 2,
				   ReplenishmentPeriod = TimeSpan.FromSeconds(0.4),
				   QueueLimit = 60
			   });
		   builder.AddRetry(new HttpRetryStrategyOptions
		   {
			   MaxRetryAttempts = 20,
			   MaxDelay = TimeSpan.FromSeconds(10),
			   DelayGenerator = args => args.Outcome switch
			   {
				   { Exception: RateLimiterRejectedException } => ValueTask.FromResult<TimeSpan?>(TimeSpan.FromSeconds(rateLimiter.GetStatistics()!.CurrentQueuedCount / 6)),
				   { Result.StatusCode: HttpStatusCode.TooManyRequests } => ValueTask.FromResult<TimeSpan?>(TimeSpan.FromSeconds(args.AttemptNumber / 2 + Random.Shared.NextDouble())),
				   { Exception: HttpRequestException } => ValueTask.FromResult<TimeSpan?>(TimeSpan.FromSeconds(1)),
				   _ => ValueTask.FromResult<TimeSpan?>(TimeSpan.FromSeconds(0))
			   },
			   ShouldHandle = args => args switch
			   {
				   { Outcome.Exception: RateLimiterRejectedException, AttemptNumber: < 20 } => PredicateResult.True(),
				   { Outcome.Result.StatusCode: HttpStatusCode.TooManyRequests, AttemptNumber: < 5 } => PredicateResult.True(),
				   { Outcome.Exception: HttpRequestException, AttemptNumber: < 1 } => PredicateResult.True(),
				   _ => PredicateResult.False()
			   }
		   });
		   builder.AddRateLimiter(rateLimiter);
	   });
		builder.Populate(service);
	}
}
