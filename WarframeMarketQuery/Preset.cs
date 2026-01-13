// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Caching.Hybrid;
using System.Net;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using WarframeMarketQuery.API;
using WarframeMarketQuery.Arcane;
using WarframeMarketQuery.Extension;
using WarframeMarketQuery.Model.Items;
using WarframeMarketQuery.Model.Orders;
using WarframeMarketQuery.Model.Statistics;
using zms9110750.TreeCollection.Trie;

namespace WarframeMarketQuery;
/// <summary>
/// 预设功能
/// </summary> 
internal class Preset(WarframeMarketApi wmClient, ArcanePack[] pack, Trie trie, HybridCache HybridCache) : IAsyncDisposable
{
    readonly string Buffer = new string(' ', 20);
    readonly string BufferLine = new string(' ', 80);
    readonly TimeSpan BufferTime = TimeSpan.FromSeconds(0.4);
    Task Unfinished { get; set; } = Task.CompletedTask;
    public async Task Run(string[] args)
    {
        string input;
        do
        {
            switch (args)
            {
                case { Length: > 0 } when args.All(s => int.TryParse(s, out _)):
                    Console.WriteLine($"赋能包参数:{string.Join(",", args)}");
                    var set = args.Select(int.Parse).ToHashSet();
                    await ArcanePrice(set);
                    break;
                case ["/?"] or ["/help"]:
                    Console.WriteLine("""
以下命令只能单独使用：
空格分开的纯数字：一组小小黑分解后的赋能包价值。数字是每天分解数量，按照市场流通量限制低需求赋能的参考价
-c：退出
/?：帮助

以下命令可以混合使用：
-t开头的词语，查询翻译。没有联网延迟。例如：-t成长
-u开头加wm上的用户名，查询用户公开订单。例如-ulonnstyle
其他词语，查询对应物品的价格。例如：wisp");

单词包括wm支持的中文翻译，英文原名，wm网址上表示的物品id。例如：盲怒，"Blind Rage"，blind_rage
用空格隔开多个命令，如果单词本身有空格，用引号括起来。例如"Blind Rage"

空格，下划线，赋能的·，这三个符号是分隔符。物品名字会被分隔符截断。
例如，"Wisp Prime 一套"会被分割为Wisp，Prime，一套
使用分隔符代表无视一节，从下一节开始匹配。例如" Prime 一套"允许任意的第一节。
""");

                    break;
                case { Length: > 0 }:
                    foreach (var item in args)
                    {
                        if (item.StartsWith("-t"))
                        {
                            Console.WriteLine($"====查询翻译：{item[2..]}====");
                            await GetItemTranslate(item[2..]);
                        }
                        else if (item.StartsWith("-u"))
                        {
                            Console.WriteLine($"====查询用户：{item[2..]}====");
                            await GetUserOrders(item[2..]);
                        }
                        else
                        {
                            Console.WriteLine($"====查询价格：{item}====");
                            await GetItemPrice(item);
                        }
                        Console.WriteLine();
                    }
                    break;
                default:
                    Console.WriteLine("参数没有被识别");
                    break;
            }
            Console.WriteLine();
            Console.WriteLine("继续输入命令来重新执行。或者输入-c退出程序。输入/?或/help列出命令");
            input = Console.ReadLine() ?? "-c";
            Console.Clear();
            args = Preset.ParseCommandLine(input);
        } while (!input.Equals("-c", StringComparison.OrdinalIgnoreCase));
    }
    public async Task CheckUpdata(CancellationToken cancellation = default)
    {
        var local = await HybridCache.GetOrDefaultAsync<string>(nameof(Version).ToLower(), null, cancellation);
        if (local == null)
        {
            Console.WriteLine("初始化缓存，请等待");
            await wmClient.GetAndSetIndexByItemAsync(cancellation);
            Console.WriteLine("缓存更新完成");
        }
        else
        {
            Console.WriteLine("本地数据版本" + local + " , 查询服务器数据版本");
            Unfinished = Task.Run(async () =>
                {
                    var server = (await wmClient.GetVersionAsync(cancellation)).Id;
                    if (local == server)
                    {
                        Console.WriteLine("本地数据已是最新");
                    }
                    else
                    {
                        Console.WriteLine("后台更新缓存" + local + "/" + server);
                        await wmClient.GetAndSetIndexByItemAsync(cancellation);
                        Console.WriteLine("缓存更新完成");
                    }
                }, cancellation);
            Console.WriteLine();
        }
    }

    /// <summary>
    /// 查询赋能包的参考价
    /// </summary> 
    public async Task ArcanePrice(IEnumerable<int> sets)
    {
        Console.WriteLine(DateOnly.FromDateTime(DateTime.Now).ToString("O"));
        Console.Write("分解小小黑/(组*天)  ");
        foreach (var i in sets)
        {
            Console.Write($"|{i,-6}");
        }
        Console.WriteLine();
        int initialTop = Console.CursorTop;
        int initialLeft = Console.CursorLeft;
        var l = pack.ToAsyncEnumerable()
                .Scan(new Dictionary<ArcanePack, double>(), async (dic, pack) =>
                {
                    dic[pack] = await pack.GetReferencePriceAsync(wmClient);
                    return dic;
                }).Select(dic => dic.OrderByDescending(kvp => kvp.Value))
                .Buffer(BufferTime).Select(s => s.Last());
        await foreach (var item in l)
        {
            Console.SetCursorPosition(initialLeft, initialTop);
            foreach (var item2 in item)
            {
                Console.Write($"{Fill(item2.Key.Name, 20)}");
                foreach (var i in sets)
                {
                    Console.Write($"|{await item2.Key.GetReferencePriceAsync(wmClient, i),-6:f1}");
                }
                Console.WriteLine(Buffer);
            }
        }

    }

    public async Task GetUserOrders(string user)
    {
        Order[] orders;
        try
        {
            orders = await wmClient.GetOrdersFromUserAsync(user);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            Console.WriteLine("此用户不存在。");
            return;
        }
        var list = orders.ToAsyncEnumerable()
            .Scan(new List<(Order, ItemShort, Statistic)>(), async (list, s) =>
            {
                list.Add((s, await wmClient.GetItemByIndexAsync(s), await wmClient.GetStatisticByIndexAsync(s)));
                return list;
            }).Buffer(BufferTime).Select(s => s.LastOrDefault() ?? []);

        // 记录初始位置
        int initialTop = Console.CursorTop;
        int initialLeft = Console.CursorLeft;

        await foreach (var task in list)
        {
            Console.SetCursorPosition(initialLeft, initialTop);
            foreach (var group in task.GroupBy(s => s.Item1.Type))
            {
                Console.WriteLine(group.Key + BufferLine);
                foreach (var item in group.OrderBy(s => s.Item2.I18n[Language.En].Name))
                {
                    var refPrice = item.Item3.GetReferencePrice(en => en.ModRank == item.Item1.Rank && en.Subtype == item.Item1.Subtype && en.AmberStars == item.Item1.AmberStars);
                    var order = item.Item1;
                    var itemName = item.Item2.I18n[Language.ZhHans].Name;
                    var enName = item.Item2.I18n[Language.En].Name;
                    Console.WriteLine($"|{Fill(enName, 30)}|{refPrice,-6:f1}({(order.Platinum < refPrice ? "+" : "-")}{Math.Abs(order.Platinum - refPrice),4:f1}) | {itemName}{Buffer}");
                }
                Console.WriteLine(BufferLine);
            }
        }
    }
    public async Task GetItemPrice(string itemPart)
    {
        var p = trie.Search(itemPart)
                    .Select(async s => (await wmClient.GetItemByIndexAsync(s), await wmClient.GetStatisticByIndexAsync(s)));


        await foreach (var result in Task.WhenEach(trie.Search(itemPart)
            .Select(async s => (await wmClient.GetItemByIndexAsync(s), await wmClient.GetStatisticByIndexAsync(s))
        )).Select(async (t, _, _) => await t))
        {
            var (item, price0, priceMax) = (result.Item1, result.Item2.GetReferencePrice(), result.Item2.GetMaxReferencePrice());
            var itemName = item.I18n[Language.ZhHans].Name;
            var enName = item.I18n[Language.En].Name;
            Console.WriteLine($"|{Fill(enName, 30)}|{Fill(itemName, 30)}|{price0,-6:f1}|{(price0 != priceMax ? $"{priceMax,-6:f1}" : "")}");
        }
    }
    public async Task GetItemTranslate(string itemPart)
    {
        foreach (var index in trie.Search(itemPart))
        {
            var item = await wmClient.GetItemByIndexAsync(index);
            Console.WriteLine($"|{Fill(item.I18n[Language.En].Name, 30)}|{item.I18n[Language.ZhHans].Name}");
        }
    }
    string Fill(string str, int length)
    {
        return str + new string(' ', Math.Max(0, length - str.Length * 2 + str.Count(char.IsAscii)));
    }
    public static string[] ParseCommandLine(string input)
    {
        var matches = Regex.Matches(input, @"(?<match>(?<!\\)""[^""]*""|\S+)");
        return matches
            .Cast<Match>()
            .Select(m =>
            {
                var val = m.Groups["match"].Value;
                // 移除首尾引号（如果存在）并处理转义引号
                if (val.StartsWith("\"") && val.EndsWith("\""))
                {
                    val = val.Substring(1, val.Length - 2)
                             .Replace("\"\"", "\""); // 处理双引号转义
                }
                return val;
            })
            .ToArray();
    }

    public async ValueTask DisposeAsync()
    {
        await Unfinished;
        await ((IAsyncDisposable)wmClient).DisposeAsync();
    }
}