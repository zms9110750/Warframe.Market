using FusionCacheReference;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Reactive.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;
using WarframeMarketQuery;
using WarframeMarketQuery.Arcane;
using WarframeMarketQuery.Extension;
using WarframeMarketQuery.Model;
using WarframeMarketQuery.Model.Items;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using zms9110750.TreeCollection.Trie;

#if false
Console.OutputEncoding = Encoding.Unicode;
Console.InputEncoding = Encoding.Unicode;
ServiceCollection? services = new ServiceCollection();
services
    .AddFusionCacheAndSqliteCache(jsonOptions: new JsonSerializerOptions
    {
        // 使用源生成器的类型信息解析器
        TypeInfoResolver = JsonTypeInfoResolver.Combine(
                SourceGenerationContext.Default.Options.TypeInfoResolver,  // 优先使用源生成
                new DefaultJsonTypeInfoResolver()// 回退到反射 	
            )
    });
services.AddWarframeMarketClient()
    .AddWarframeMarketProgramServices()
    .AddSingleton<Preset>();



ServiceProvider? buidler = services.BuildServiceProvider();
await using Preset? preset = buidler.GetRequiredService<Preset>();
await preset.CheckUpdata();
await preset.Run(args);




#endif



