# Warframe.Market GUI

Warframe 市场（[warframe.market](https://warframe.market)）的桌面客户端（PhotinoX + Masa.Blazor）。

## 页面

- **物品搜索**：中/英文名搜索（`/` 分隔多词），结果按词分组；满级价格列动态（无满级概念的物品不显示）；遗物映射"满级=光辉/参考=完整"；展开可看 Top 订单
- **用户订单**：搜用户 → 订单列表（买/卖分组），价格后台分批加载不阻塞；点击订单展开子面板（买卖/等级/子类型**初始匹配该订单**）
- **订单子面板**：购/售切换、在线筛选、等级滑块（mod/赋能）、遗物精炼度滑块（购≤档/售≥档）、塑像豆子滑块（含每 p 豆子）、子类型按钮组（多选默认全开）、批量交易列、价格区间/最少数量
- **赋能包**：包×每日购买量的期望值矩阵（流动性封顶），失败显示 `-`
- **快捷回复**：私信模板，可编辑
- **设置**：语言包下载（官方 wfm-localization）、平台/跨平台（切换需重启）、发布地址

## 技术

- PhotinoX 4.x + Masa.Blazor 1.11 + net10.0
- 库层服务（物品索引/参考价/赋能包计算）来自 `zms9110750.Warframe.Market`（见 `src/`）
- Serilog 日志（`logs/app.log`）；微软配置系统（`appsettings.json`：`Gui` 节 + `Version:Program`）
- 弹性管道：HTTP 缓存（HybridCache/FusionCache）+ 限流（3/s）+ 429/空数据重试；Polly 遥测日志
- 子类别中文本地化来自官方 wfm-localization（运行时读取）
