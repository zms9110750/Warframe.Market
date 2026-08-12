# Warframe.Market

Warframe 市场（[warframe.market](https://warframe.market)）的 .NET 客户端库 + 桌面 GUI。

- **库**（`src/Warframe.Market`，NuGet: `zms9110750.Warframe.Market`）：完整 API 客户端，内置限流/重试/缓存与参考价计算
- **GUI**（`samples/Warframe.Market.GUI`）：PhotinoX 桌面客户端，物品搜索/用户订单/赋能包估值/快捷回复

对应 warframe.market API **v0.25.0**（REST）/ v0.13.0（WebSocket）。

---

## 库（NuGet）

```bash
dotnet add package zms9110750.Warframe.Market
```

```csharp
using zms9110750.WarframeMarketApi;

// 默认构造：内置 Polly 弹性管道（限流 3/s + 429 重试 + 取消/空数据重试）
var client = new WarframeMarketClient();
client.Language = Language.ZhHans;
client.Platform = Platform.PC;

// 物品搜索（Trie 索引 + 归一化匹配）
var service = new ItemSearchService(client);
var items = await service.SearchAsync("wisp");
```

**特性**：
- 全部 V2 端点（物品/订单/用户/成就/统计）+ V1 统计
- Polly 弹性管道：限流（3/s）、429 重试、空数据重试（`/v2/user/` 偶发 200+空 data 自动重试）
- HTTP 响应缓存（可选，`AddResilienceHandler` + HybridCache/FusionCache 桥接，端点级 TTL 策略）
- 参考价算法：90 天成交量加权中位数（`StatisticPrice`）、满级价/材料价、遗物精炼度映射、赋能包期望值
- 塑像豆子换算（`AyatanEndo`：Wiki 公式验证 11 塑像，星星→内融核心）
- 私信文本模板（`OrderMessageFormatter`，多语言 ICU 风格模板）
- 子类别中文本地化（官方 wfm-localization）

## GUI（桌面客户端）

```bash
dotnet run --project samples/Warframe.Market.GUI -c Release
```

**页面**：
- **物品搜索**：中/英文名搜索（`/` 分隔多词），结果表按词分组；满级价格列动态（无满级概念的物品不显示）；遗物映射"满级=光辉/参考=完整"；展开可看 Top 订单
- **用户订单**：搜用户 → 订单列表（买/卖分组），价格后台分批加载不阻塞；点击订单展开子面板（买卖/等级/子类型**初始匹配该订单**）
- **订单子面板**：购/售切换、在线筛选、等级滑块（mod/赋能）、**遗物精炼度滑块**（购≤档/售≥档）、**塑像豆子滑块**（50/75/100 步进 + 每 p 豆子）、**子类型按钮组**（多选默认全开）、批量交易列、价格区间/最少数量
- **赋能包**：包×每日购买量的期望值矩阵（流动性封顶），失败显示 `-`
- **快捷回复**：私信模板，可编辑
- **设置**：语言包下载（官方 wfm-localization）、平台/跨平台（切换需重启）、发布地址

**技术**：PhotinoX 4.x + Masa.Blazor 1.11 + net10.0；Serilog 日志（`logs/app.log`）；微软配置系统（`appsettings.json`：`Gui` 节 + `Version:Program`）；Polly 遥测（重试/限流/缓存事件带 URL）。

## 解决方案结构

```
src/Warframe.Market/    库（公开 NuGet 包）
samples/Warframe.Market.GUI/  桌面 GUI
test/Warframe.Market.Tests/   测试（166 用例：服务/算法/缓存/bUnit 页面交互）
test/Resources/         API 数据备份（items/orders/statistics JSON）
docs/                   方案文档
```

## 构建与测试

```bash
dotnet build Warframe.Market.slnx
dotnet test test/Warframe.Market.Tests
```

## 版本

`Directory.Build.props` 的 `<Version>` 统一版本号。当前 **0.2.0-a.0**（构建元数据 `+v0.25.0` 表示对应的 wfm API 版本）。
