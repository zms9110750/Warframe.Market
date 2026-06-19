# TODO — 组件拆分 + Bug 修复

## DataTableHeader 用法

`DataTableHeader<T>` 的构造参数可以传委托表达式，不是只能传属性名。
例如：`new DataTableHeader<Order>("名称", r => r.User?.IngameName ?? "")`
这样不用猜属性名，也不用 `ItemColContent` 硬编码。

---

## P0 Bug

### 1. 搜索结果：物品名显示"加载中..."且不加载

`SearchResultTable` 的渐进刷新循环可能没正确触发。`ItemSvc.GetStatisticAsync` 返回 null 或卡住。

### 2. 英文名列为空

`SearchResultTable` 的 `ItemColContent` 中英文名列读取 `Language.En`，但 `ItemShort.I18n` 字典可能为空（EF Core 的 `Ignore(i => i.I18n)`）。需要确保 I18n 已填充。

### 3. 子面板 "没有必需参数"

`OrderTop` 收到 null `TargetItem`。原因是 `ExpandedItemContent` 的 `context` 未正确传递物品数据。

### 4. 参考价/差价不加载

`UserSearch` 的价格加载循环可能被跳过，或 `ItemSvc.GetStatisticAsync` 返回 null。

### 5. 列 "语言" 多余

删除"语言"列。有中文名和英文名就够了。

### 6. 用户订单没有折叠

每个用户的结果应该可折叠（像物品搜索的 `SearchResultTable` 那样）。

### 7. 赋能包子面板 colspan 不是 9999

`ArcaneTable` 的 `<td colspan` 可能写死了数字，应改为 9999。

### 8. 标签 Index 错误

点第二个固定标签显示第一个的内容，点第一个标签时激活标签 UI 消失。`activeTabIndex` 与标签实际索引不匹配。

---

## P1 — 组件拆分

### 9. 拆分物品搜索组件

```
Pages/ItemSearch/
  ├── ItemSearch.razor        ← 页面入口
  ├── ItemSearch.razor.cs
  ├── SearchTabs.razor        ← 标签栏（与用户搜索共用）
  ├── SearchBox.razor         ← 搜索框（与用户搜索共用）
  ├── SearchTermGroup.razor   ← 参数为搜索词：内含 / 分割逻辑、foreach 列表、折叠
  └── SearchResultTable.razor ← 搜索结果表
```

### 10. 拆分用户订单组件

```
Pages/UserSearch/
  ├── UserSearch.razor        ← 页面入口
  ├── UserSearch.razor.cs
  ├── UserResultTable.razor   ← 每个用户的订单表
  └── UserTabs.razor          ← 标签栏（与物品搜索共用 SearchTabs）
```

### 11. 抽出共用标签组件

`SearchTabs.razor` 被物品搜索和用户订单共用，放在 `Pages/Shared/`。
