# Warframe.Market API

[Warframe.market](https://warframe.market) 官方 API 的 .NET 客户端库（拟合 API 文档 v0.25.0）。

## 安装

```bash
dotnet add package zms9110750.Warframe.Market
```

## 用法

```csharp
using zms9110750.WarframeMarketApi;

var client = new WarframeMarketClient();
client.Language = Language.ZhHans;
client.Platform = Platform.PC;

var item = await client.GetItemsAsync();
```

## 内容

- `WarframeMarketClient`：全部端点（物品/订单/用户/成就/统计/版本），内置 Polly 弹性管道（限流 3/s + 429/空数据重试）
- `Api/`：Refit 接口定义
- `Models/`：API 数据模型
- `Requests/`：请求体
