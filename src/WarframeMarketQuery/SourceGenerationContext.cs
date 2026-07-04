using System.Text.Json.Serialization;
using WarframeMarketQuery.Model.Items;
using WarframeMarketQuery.Model.Users;

namespace WarframeMarketQuery.Model;

[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(Response<ItemSet>))]
[JsonSerializable(typeof(Version))]
[JsonSerializable(typeof(Response<User>))]
[JsonSerializable(typeof(Response<Orders.OrderTop>))]
[JsonSerializable(typeof(Response<Statistics.Statistic>))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}
