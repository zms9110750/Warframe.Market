# Warframe.Market

Warframe 市场（[warframe.market](https://warframe.market)）的 .NET 项目：

- **库**（`src/Warframe.Market`，NuGet: `zms9110750.Warframe.Market`）— Warframe.market API 的 .NET 客户端（拟合 API 文档），内置限流/重试
- **GUI**（`samples/Warframe.Market.GUI`）— PhotinoX 桌面客户端（物品搜索/用户订单/赋能包估值/快捷回复）

对应 warframe.market API **v0.25.0**（REST）/ v0.13.0（WebSocket）。

## 目录

```
src/      库（公开 NuGet 包，见其 README）
samples/  GUI 桌面客户端（见其 README）
test/     测试（166 用例 + API 数据备份）
docs/     方案文档
```

## 版本

`Directory.Build.props` 的 `<Version>` 统一版本号；发版由 CI 按 `v*` tag 自动打包。
