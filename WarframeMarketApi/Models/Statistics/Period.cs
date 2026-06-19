using System.Text.Json;
using System.Text.Json.Serialization;

namespace zms9110750.WarframeMarketApi.Models.Statistics;

/// <summary>
/// 统计时段数据，包含 48 小时和 90 天两个粒度
/// </summary>
/// <param name="Hour48">48 小时内数据，每 2h 跨度</param>
/// <param name="Day90">90 天内数据，每天跨度</param>
[JsonConverter(typeof(PeriodConverter))]
public record Period(
	Entry[] Hour48,
	Entry[] Day90
);

internal class PeriodConverter : JsonConverter<Period>
{
	public override Period? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartObject) return null;
		Entry[]? h48 = null, d90 = null;
		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.EndObject) break;
			if (reader.TokenType == JsonTokenType.PropertyName)
			{
				var name = reader.GetString();
				reader.Read();
				if (name == "48hours") h48 = JsonSerializer.Deserialize<Entry[]>(ref reader, options);
				else if (name == "90days") d90 = JsonSerializer.Deserialize<Entry[]>(ref reader, options);
				else reader.Skip();
			}
		}
		return new Period(h48 ?? Array.Empty<Entry>(), d90 ?? Array.Empty<Entry>());
	}

	public override void Write(Utf8JsonWriter writer, Period value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("48hours");
		JsonSerializer.Serialize(writer, value.Hour48, options);
		writer.WritePropertyName("90days");
		JsonSerializer.Serialize(writer, value.Day90, options);
		writer.WriteEndObject();
	}
}
