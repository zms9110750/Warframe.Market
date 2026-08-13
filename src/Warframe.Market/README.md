# Warframe.Market API

[Warframe.market](https://warframe.market) 官方 API 的 .NET 客户端库（拟合 API 文档 v0.25.0）。支持 .NET 8+，依赖仅 3 个包（Refit / Polly / Polly.RateLimiting）。

## 安装

```bash
dotnet add package zms9110750.Warframe.Market
```

## 用法

```csharp
using zms9110750.WarframeMarketApi;

// 新建连接器：内置 Polly 弹性管道（限流 3/s + 429/空数据重试）
var client = new WarframeMarketClient();

// 拿物品全集（建议缓存：全集较大且变更不频繁；内置限流 3/s 会自动排队）
var items = (await client.GetItemsAsync())?.Content?.Data ?? [];

// 从全集里拿"盲怒"，查它的订单
var blindRage = items.First(i => i.Slug == "blind_rage");
var orders = (await client.GetOrdersItemAsync(blindRage.Slug))?.Content?.Data ?? [];

foreach (var o in orders.Take(10))
{
    Console.WriteLine($"{o.User?.IngameName} {o.Type} {o.Platinum}p x{o.Quantity}");
}
```

## 内容

- `WarframeMarketClient`：全部端点（物品/订单/用户/成就/统计/版本），内置 Polly 弹性管道（限流 3/s + 429/空数据重试）
- `Api/`：Refit 接口定义
- `Models/`：API 数据模型
- `Requests/`：请求体
