# TODO

## P0

### 1. 商品搜索结果没有展开子面板

搜索结果表（SearchResultTable）的 `ShowExpand` 没有正确渲染 `ExpandedItemContent` 中的 `OrderTop`。

**原因**：可能是 `ItemColContent` 的 `colspan` 与 `ExpandedItemContent` 的 `colspan` 不匹配，或 `ItemKey` 设置不正确导致展开状态丢失。

**要求**：
- 点击行展开图标，能正常展开并看到 OrderTop
- 展开/收起不丢失数据

---

### 2. 打开链接开关无用

`MainLayout` 传递了 `ClickLink` 级联值，但 `SearchResultTable`、`OrderTop`、`UserSearch` 等组件虽然接收了 `[CascadingParameter]`，实际没有在所有需要的地方用上。

**要求**：
- `SearchResultTable`：中文名 → wfm 物品链接，价格 → wfm 统计链接
- `OrderTop`：卖家名 → wfm 个人主页链接（已有）
- 用户订单页面：物品名 → wfm 物品链接

---

### 3. 赋能包主表没有排序

赋能包页面的 `ArcanePacks` 表头 `_headers` 的 `ValueExpression` 返回的是 `pack.Name`（占位符），实际值在 `ItemColContent` 里渲染。MDataTable 无法按这些列排序。

**要求**：
- 列头点击可排序
- `ValueExpression` 返回正确的值（计划数值或 null）

---

### 4. 子面板应该放在所属 Page 的文件夹下

当前 `OrderTop`、`SearchResultTable`、`ArcaneTable`、`SearchBox` 都挤在 `Shared/` 文件夹下。

**要求**：
- `SearchBox` → `Pages/FindItem/`
- `SearchResultTable` → `Pages/FindItem/`
- `OrderTop` → `Pages/` 下（被多个页面引用，可放 `Pages/Shared/` 或 `Pages/OrderTop/`）
- `ArcaneTable` → `Pages/FindArcane/`

---

## P1

### 5. 子面板 colspan 不足

通过 `@oninit` 添加动态列（如 "等级"、"琥珀星"、"类型"）时，`ExpandedItemContent` 的 `colspan` 写死了数字，不匹配实际列数。

**要求**：
- 所有 `ExpandedItemContent` 的 `colspan` 改为 `9999`（溢出不会有事）

---

### 6. 版本按钮"再次点击强制刷新"没有业务逻辑

当前点击版本按钮 → 显示"再次点击强制刷新"→ 再点击 → 只调了 `GetVersionsAsync`，没有执行全量数据刷新（删表 → 拉取 → 写入）。

**要求**：
- 再次点击时，调用 `CacheService.RefreshAllAsync()`，和启动时的全量刷新逻辑一致
- 刷新期间显示进度，完成后更新日期
