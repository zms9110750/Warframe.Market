# WarframeMarketApi

[![]()]()
**作者:** zms9110750  
**命名空间:** `zms9110750.WarframeMarketApi`  
**目标框架:** .NET 10

Warframe.Market 的 API 封装 NuGet 库，基于 [Refit](https://github.com/reactiveui/refit) 声明式 HTTP 客户端。

---

## 功能状态

| 类别 | 状态 | 说明 |
|------|------|------|
| ✅ 公共 API | 可用 | 所有公开端点可正常调用 |
| 🔒 认证 API | 内部 | 因 Firebase App Check 限制，暂无法从外部使用 |

### 公共 API — `IWarframeMarketApiV2`

```csharp
// 注册
services.AddRefitClient<IWarframeMarketApiV2>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.warframe.market"));
```

| 端点 | 说明 |
|------|------|
| `GET /v2/versions` | 服务器版本号 |
| `GET /v2/items` | 所有可交易物品 |
| `GET /v2/item/{slug}` | 单一物品信息 |
| `GET /v2/item/{slug}/set` | 物品套装 |
| `GET /v2/riven/weapons` | 裂罅武器列表 |
| `GET /v2/riven/weapon/{slug}` | 单一裂罅武器 |
| `GET /v2/riven/attributes` | 裂罅属性列表 |
| `GET /v2/lich/weapons` | 玄骸武器列表 |
| `GET /v2/lich/weapon/{slug}` | 单一玄骸武器 |
| `GET /v2/lich/ephemeras` | 玄骸幻纹列表 |
| `GET /v2/lich/quirks` | 玄骸 Quirk 列表 |
| `GET /v2/sister/weapons` | 姐妹武器列表 |
| `GET /v2/sister/weapon/{slug}` | 单一姐妹武器 |
| `GET /v2/sister/ephemeras` | 姐妹幻纹列表 |
| `GET /v2/sister/quirks` | 姐妹 Quirk 列表 |
| `GET /v2/locations` | 位置节点列表 |
| `GET /v2/npcs` | NPC 列表 |
| `GET /v2/missions` | 任务列表 |
| `GET /v2/orders/recent` | 最新订单 |
| `GET /v2/orders/item/{slug}` | 物品订单 |
| `GET /v2/orders/item/{slug}/top` | Top 买卖单 |
| `GET /v2/orders/user/{slug}` | 用户订单 |
| `GET /v2/user/{slug}` | 用户信息 |
| `GET /v2/achievements` | 成就列表 |
| `GET /v2/achievements/user/{slug}` | 用户成就 |
| `GET /v2/dashboard/showcase` | 展示面板 |

### 公共 API — `IWarframeMarketApiV1`

| 端点 | 说明 |
|------|------|
| `GET /v1/items/{slug}/statistics` | 物品统计数据（snake_case 序列化） |

---

## 认证 API 🔒（当前不可用）

以下接口标记为 `internal`，因为 API 要求 **Firebase App Check** 验证，无法从独立的 HTTP 客户端直接调用。

| 接口 | 说明 |
|------|------|
| `IWarframeMarketApiV2Auth` | 需要 JWT 认证的端点（创建/修改/删除订单、用户资料管理） |
| `IWarframeMarketAuthApi` | 登录/注册/刷新/登出（需 Firebase App Check） |

预计在 OAuth 2.0 文档完善后可恢复使用。

---

## 速率限制

- **每秒最多 3 个请求**
- 超出返回 `429 Too Many Requests`
- 建议客户端实现重试策略（指数退避）

## 请求头

所有请求默认需要设置：

```http
Language: zh-hans          # 返回语言（14 种语言支持）
Platform: pc               # 平台（pc/ps4/xbox/switch/mobile）
Crossplay: false           # 跨平台交易
```

## 模型结构

```
Models/
├── Response<T>           # V2 统一响应包装
├── RichStatus            # 用户详细状态
├── Items/                # 物品 (Item, ItemShort, ItemSet, 枚举...)
├── Orders/               # 订单 (Order, OrderTop, Transaction...)
├── Users/                # 用户 (User, UserShort, UserPrivate...)
├── Rivens/               # 裂罅 (Riven, RivenAttribute)
├── Liches/               # 玄骸 (LichWeapon, LichEphemera, LichQuirk)
├── Sisters/              # 姐妹 (SisterWeapon, SisterEphemera, SisterQuirk)
├── Locations/            # 位置
├── Npcs/                 # NPC
├── Missions/             # 任务
├── Achievements/         # 成就
├── Dashboard/            # 展示面板
├── Clients/              # OAuth 客户端
├── Groups/               # 分组
├── Statistics/           # 统计数据
└── Versions/             # 版本号
```

---

## 参考文档

本地 HTML 导出（Notion 私有页面）：

- `html/WFM Api v2 Documentation.htm`
- `html/Data Models.html`
- `html/OAuth 2.0.html`
- `html/Websockets.html`
