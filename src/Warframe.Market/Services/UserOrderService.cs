using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.WarframeMarketApi.Models.Orders;
using zms9110750.WarframeMarketApi.Models.Users;

namespace zms9110750.WarframeMarketApi.Services;

/// <summary>
/// 用户订单查询实现：确认用户存在 → 拉订单 → 本地索引补物品（不额外走 API）→ 价格分批加载
/// </summary>
public class UserOrderService : IUserOrderService
{
    private readonly WarframeMarketClient _wfm;
    private readonly IItemSearchService _items;

    public UserOrderService(WarframeMarketClient wfm, IItemSearchService items)
    {
        _wfm = wfm;
        _items = items;
    }

    public async Task<UserSearchResult> SearchUserAsync(string name, CancellationToken ct = default)
    {
        var result = new UserSearchResult { Loading = true };
        Log.Information("UserOrderService 查询用户: {Name}", name);
        try
        {
            User? user;
            try
            {
                var userResp = await _wfm.GetUserAsync(name, ct);
                user = userResp?.Content?.Data;
                Log.Information("UserOrderService 用户响应: {Name}, User={User}", name, user?.IngameName);
            }
            catch (Refit.ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Log.Warning("UserOrderService 用户 404: {Name}", name);
                result.NotFound = true;
                result.Loading = false;
                return result;
            }
            if (user == null)
            {
                Log.Warning("UserOrderService 用户为空: {Name}", name);
                result.NotFound = true;
                result.Loading = false;
                return result;
            }
            result.User = user;

            var orderResp = await _wfm.GetOrdersFromUserAsync(name, ct);
            if (orderResp?.Content?.Data == null || orderResp.Content.Data.Length == 0)
            {
                result.Loading = false;
                return result;
            }
            result.Orders = orderResp.Content.Data.ToList();
            result.Loading = false;

            // 补物品信息（本地索引/HTTP 缓存，不走多余 API）
            foreach (var o in result.Orders)
            {
                var itemId = o.ItemId ?? "";
                if (result.ItemCache.ContainsKey(itemId))
                {
                    continue;
                }

                result.ItemCache[itemId] = await _items.FindByKeyAsync(itemId);
            }

            // 注意：不在此同步加载价格——SearchUserAsync 只返回 用户+订单（列表立即展示），
            // 价格由 UI 层调 LoadPricesAsync 后台分批加载（完成即刷新），避免"价格加载完才展示表格"
            Log.Information("UserOrderService 用户订单列表就绪: {Name}, {Count} 个订单", name, result.Orders.Count);
        }
        catch (Exception ex)
        {
            result.Error = ex.Message;
        }
        result.Loading = false;
        return result;
    }

    /// <summary>
    /// 后台分批加载订单参考价（每批 3 个，配合限流）。UI 层调用后每批完成需刷新界面；
    /// result.LoadingPrices 在整个过程为 true，结束置 false。
    /// </summary>
    public async Task LoadPricesAsync(UserSearchResult result, CancellationToken ct = default)
    {
        result.LoadingPrices = true;
        try
        {
            var priceTasks = new List<Task>();
            foreach (var o in result.Orders)
            {
                var item = result.ItemCache.GetValueOrDefault(o.ItemId ?? "");
                if (item != null && !result.Prices.ContainsKey(item.Slug))
                {
                    var slug = item.Slug;
                    priceTasks.Add(LoadPriceAsync(result, slug, ct));
                    if (priceTasks.Count >= 3)
                    {
                        await Task.WhenAll(priceTasks);
                        priceTasks.Clear();
                    }
                }
            }

            await Task.WhenAll(priceTasks);
            Log.Information("UserOrderService 价格加载完成: {Count} 个", result.Prices.Count);
        }
        finally
        {
            result.LoadingPrices = false;
        }
    }

    private async Task LoadPriceAsync(UserSearchResult result, string slug, CancellationToken ct)
    {
        try
        {
            var stat = await _items.GetStatisticAsync(slug, ct);
            if (stat != null)
            {
                result.Prices[slug] = stat;
            }
        }
        catch { }
    }
}
