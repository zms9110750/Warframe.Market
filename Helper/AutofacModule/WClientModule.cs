using Autofac;
using Warframe.Market.Helper.CacheAndClient;
using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.DependencyInjection;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Warframe.Market.Helper.Abstract;

namespace Warframe.Market.Helper.AutofacModule;
public sealed class WClientModule : Module
{
	protected override void Load(ContainerBuilder builder)
	{
		var service = new ServiceCollection();
		service.AddMemoryCache(options =>
		{
			options.SizeLimit = 1024 * 1024*4;
			options.ExpirationScanFrequency = TimeSpan.FromMinutes(1);
		});
		builder.Populate(service);
		builder.RegisterType<WClient>().As<IWMClient>().SingleInstance().WithAttributeFiltering();
		builder.RegisterType<CacheDbContext>().As<ICacheLoacd>().SingleInstance();
		builder.RegisterDecorator<IWMClient>((context, parameters, instance) 
			=>new WMCacheAndClient(context.Resolve<ICacheLoacd>(), instance, context.Resolve<IMemoryCache>()));
		builder.Register(c => c.Resolve<IHttpClientFactory>().CreateClient(ResilientHttpModule.Key)).Keyed<HttpClient>(ResilientHttpModule.Key).SingleInstance();
	}

}
