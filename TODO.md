# TODO — 当前状态

## P0 — 已验证修复

| # | 问题 | 状态 |
|---|------|------|
| 1 | 搜索结果没有展开子面板 | ⚠️ 代码有 `ShowExpand` + `ExpandedItemContent`，需验证 |
| 2 | "打开链接"开关无用 | ❌ 待验证 |
| 3 | 赋能包子表无法排序（价格列异步） | ✅ 加了 `ValueExpression`，null 做默认值 |
| 4 | 子面板文件放错文件夹 | ❌ 未动 |
| 5 | 子面板 colspan 不足 | ✅ 全改为 `9999` |
| 6 | 版本按钮强制刷新没逻辑 | ✅ 已调 `RefreshAllAsync()` |
| 7 | 用户订单查询重复 key 崩溃 | ✅ `@key` 加 `group.Key` 唯一化 |
| 8 | 用户订单/OrderTop 分组 | ✅ 已有 GroupBy + GroupHeaderContent，无冗余列 |
