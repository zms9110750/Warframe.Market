# TODO - 未修复问题清单（按优先级排序）

---

## P0 — 核心功能不可用

### 1. 用户搜索页面不刷新

**问题**：`/user` 点查询后界面不刷新，结果不显示。

**原因**：`UserSearch.SearchAsync` 完成后没有触发 UI 刷新。`LoadItemInfoAsync` fire-and-forget 异常被吞。

**要求**：
- 搜索完成后立即显示已有数据（用户信息、订单列表）
- 价格异步加载，逐条刷新

---

### 2. OrderTop 显示"没有必需参数" 或 死加载

**问题**：在用户搜索结果中展开行，OrderTop 要么显示"没有必需参数"，要么无限 loading。

**原因**：`OrderTop.Item` 为 null 时直接 return，`loading` 没设为 false，UI 保持 loading 状态。

**修复**：已加 `loading = false;`，但 `GetItemShort(context.Item)` 仍然可能返回 null（物品信息异步加载中）。

**要求**：
- Item 必须有值才渲染 OrderTop
- 物品信息没加载完时提前阻止渲染，不展开

---

### 3. 用户订单表格排序报错

**问题**：点击列头排序时报错。

**原因**："参考价"、"差价"列用方法渲染，没有 `ValueExpression`，MDataTable 找不到属性。

**要求**：
- 所有可排序列加 `ValueExpression`
- 或 `Sortable=false`

---

### 4. 搜索结果 + 赋能包子面板（展开明细）不工作

**问题**：
- 搜索结果表展开 OrderTop：类型分支没做全
- 赋能包展开 ArcaneTable：出货率% 应该静态，价格异步，要渐进刷新

**要求**：
- 展开/收起不丢数据
- 渐进刷新 `do { StateHasChanged(); await 200ms; } while (!allTasks.IsCompleted)`
- 出货率% 从 YAML 直接读，不依赖 API

### 10. 统计数据并发请求去重（同 key 同时请求多次）

**问题**：同一个物品的统计数据在同一时刻被多次请求（5 个购买量并发）。第一次请求还没写入缓存，后续请求又去调 API。

**日志证据**：
```
14:14:40.332 API magus_destruct
14:14:40.341 API magus_destruct
14:14:40.345 API magus_destruct  ← 21ms 内 5 次请求
```

**要求**：
- 同一时间对同一个 key 的 `GetStatisticsAsync` 只发一次 API 请求
- 后续并发请求等待第一个完成后的结果
- 用 `ConcurrentDictionary<string, Task<Statistic?>>` 或 `AsyncLazy` 去重

---

## P1 — 显示/逻辑错乱

### 5. 类型列语义错误

**问题**：OrderTop 的"买/卖"列是冗余的（已经 GroupBy 分组了）。

**要求**：
- 去掉"买/卖"列（靠分组区分）
- 动态子类型列（蓝图/成品/完整/光辉）仅在需要时显示

---

### 6. 价格不进 SQLite 缓存

**问题**：统计数据已经走 SQLite 了，但赋能包期望值等其他价格没有。

**要求**：
- 所有价格计算结果写入 `CacheEntry` 表
- 赋能包名称+购买量 → 期望值 要缓存（过期 2 天）

---

## P2 — 健壮性/可调试

### 7. 全局错误捕获 + 日志

**要求**：
- Blazor 组件所有 `OnInitializedAsync` 和事件处理器 try/catch
- 异常写入 Serilog
- UI 显示友好错误提示

---

### 8. 每个组件初始化 + 事件打日志

**要求**：
- 每个组件的 `OnInitializedAsync` 开头写 `Log.Information`
- 每个事件处理器开头写日志

---

### 9. 快捷回复：失焦保存

**要求**：
- `@onblur` 触发保存，不是点击按钮
- 空文本不保存（并从列表移除）
- 编辑模式可修改，只读模式显示复制按钮
- 关闭"可编辑"开关时触发的失焦也要保存
