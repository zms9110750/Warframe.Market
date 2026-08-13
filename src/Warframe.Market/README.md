# Warframe.Market API

[Warframe.market](https://warframe.market) 的 .NET 客户端库，内置限流/重试/缓存与参考价计算。

对应 warframe.market API **v0.25.0**（REST）/ v0.13.0（WebSocket）。

## 安装

```bash
dotnet add package zms9110750.Warframe.Market
```

## 快速开始

```csharp
using zms9110750.WarframeMarketApi;

// 默认构造：内置 Polly 弹性管道（限流 3/s + 429 重试 + 空数据重试）
var client = new WarframeMarketClient();
client.Language = Language.ZhHans;
client.Platform = Platform.PC;

// 物品搜索（Trie 索引 + 归一化匹配）
var service = new ItemSearchService(client);
var items = await service.SearchAsync("wisp");
```

### 领域服务

| 服务 | 接口 | 说明 |
|---|---|---|
| `ItemSearchService` | `IItemSearchService` | 物品搜索（Trie + 归一化）、统计/参考价/满级价、统计缓存与优先级 |
| `UserOrderService` | `IUserOrderService` | 用户确认 → 订单 → 补物品 → 价格分批加载 |
| `OrderService` | `IOrderService` | 订单全量拉取 + 纯函数筛选（购/售/等级/价格/数量） |
| `ArcanePackService` | `IArcanePackService` | 赋能包期望值（流动性封顶）、日均交易量 |

### 参考价与工具

- **`StatisticPrice`**：90 天成交量加权中位数、满级价/材料价、遗物精炼度映射（光辉=满级/完整=参考）
- **`AyatanEndo`**：塑像星星 → 内融核心（Wiki 公式，11 塑像目录）
- **`OrderMessageFormatter`**：私信文本模板（多语言 ICU 风格）
- **`ItemSubtypeSet`**：物品子类型判定（mod/遗物/赋能/鱼/裂罅等）

### 弹性管道

默认构造内置 Polly 管道：缓存层 → 限流重试 → 并发限流（3/s）→ 429 重试 → 空数据重试。可选接入 HTTP 响应缓存（`AddResilienceHandler` + HybridCache/FusionCache，端点级 TTL 策略见 GUI 的 `CacheConfig`）。

## 项目

- 库：本目录（`src/Warframe.Market`）
- GUI：`samples/Warframe.Market.GUI`（PhotinoX 桌面客户端）
- 测试：`test/Warframe.Market.Tests`（166 用例）
