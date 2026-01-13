using WarframeMarketQuery.Model.Items;

namespace WarframeMarketQuery.Model.Users;

/// <summary>
/// 用户记录类，表示游戏平台上的用户信息
/// </summary>
/// <param name="Id">用户唯一标识符</param>
/// <param name="IngameName">用户的游戏内名称</param>
/// <param name="Slug">用户的平台名称</param>
/// <param name="Avatar">用户头像URL（可选）</param>
/// <param name="Background">用户个人背景图URL（可选）</param>
/// <param name="About">用户个人简介（HTML格式）（可选）</param>
/// <param name="Reputation">用户声誉分数</param> 
/// <param name="Platform">用户游戏平台（PC/PSN/Xbox等）</param>
/// <param name="Crossplay">是否开放跨平台交易</param>
/// <param name="Locale">用户语言/地区偏好</param> 
/// <param name="Status">用户当前状态（在线/离线等）</param>
/// <param name="LastSeen">用户最后在线时间戳</param> 
public record User(
    string Id,
    string IngameName,
    string Slug,
    string? Avatar,
    string? Background,
    string? About,
    ushort Reputation,
    Platform Platform,
    bool Crossplay,
    Language Locale,
    UserStatus Status,
    DateTime LastSeen)
{
	public static implicit operator string(User item)
	{
		return item.Slug;
	}
}
/*
{
  "apiVersion": "0.19.1",
  "data": {
    "id": "60166887151ed600bd45be48",
    "role": "user",
    "tier": "none",
    "ingameName": "zms9110750",
    "slug": "zms9110750",
    "avatar": "user/avatar/60166887151ed600bd45be48.png?460b6809fb4f8659d52b1ff612aa35b7",
    "about": "<p>UTC+8</p>",
    "reputation": 233,
    "masteryRank": 0,
    "status": "offline",
    "activity": {
      "type": "UNKNOWN",
      "details": "unknown",
      "startedAt": "2025-07-26T13:36:37Z"
    },
    "lastSeen": "2025-07-26T13:36:37Z",
    "platform": "pc",
    "crossplay": true,
    "locale": "zh-hans"
  },
  "error": null
}
{
  "id": null,
  "ingameName": null,
  "avatar": null,
  "background": null,
  "about": null,
  "reputation": 0,
  "platform": null,
  "crossplay": false,
  "locale": "node",
  "status": "invisible",
  "lastSeen": "0001-01-01T00:00:00"
}


*/