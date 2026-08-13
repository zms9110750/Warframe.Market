using System.Text;
using zms9110750.WarframeMarketApi;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace zms9110750.WarframeMarketApi;

/// <summary>
/// 私聊消息格式化：读取 order_clipboard_template.json（ICU MessageFormat 多语言模板），
/// 按目标语言 + 占位值生成 /w 私信文本。
/// 支持模板用到的 MessageFormat 子集：{key} 占位、{key, select, a {..} b {..} other {..}} 条件分支、嵌套花括号。
/// </summary>
public static class OrderMessageFormatter
{
    private static readonly Lazy<Dictionary<string, string>> Templates = new(LoadTemplates);

    private static Dictionary<string, string> LoadTemplates()
    {
        var asm = typeof(OrderMessageFormatter).Assembly;
        var name = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("order_clipboard_template.json"))
            ?? throw new InvalidOperationException("嵌入资源 order_clipboard_template.json 缺失");
        using var stream = asm.GetManifestResourceStream(name)!;
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return JsonSerializer.Deserialize<Dictionary<string, string>>(reader.ReadToEnd())
            ?? new Dictionary<string, string>();
    }

    /// <summary>取某语言的模板（缺失时回退 en）</summary>
    public static string GetTemplate(string language)
    {
        return Templates.Value.TryGetValue(language, out var t) ? t
        : Templates.Value.TryGetValue("en", out var en) ? en
        : "";
    }

    /// <summary>
    /// 格式化 MessageFormat 模板。支持：
    ///   {key}               → values[key]（缺失替换为空）
    ///   {key, select, a {A} b {B} other {O}} → 按 values[key] 匹配分支，回退 other
    /// </summary>
    public static string Format(string template, IReadOnlyDictionary<string, string> values)
    {
        var sb = new StringBuilder();
        ParseBlock(template, 0, template.Length, values, sb);
        return sb.ToString();
    }

    private static int ParseBlock(string s, int start, int end, IReadOnlyDictionary<string, string> values, StringBuilder sb)
    {
        var i = start;
        while (i < end)
        {
            var c = s[i];
            if (c == '{')
            {
                // 找匹配的闭合 }
                var depth = 0;
                var j = i;
                for (; j < end; j++)
                {
                    if (s[j] == '{')
                    {
                        depth++;
                    }
                    else if (s[j] == '}')
                    {
                        depth--; if (depth == 0)
                        {
                            break;
                        }
                    }
                }
                var inner = s[(i + 1)..j];
                var replaced = ResolveArg(inner, values);
                sb.Append(replaced);
                i = j + 1;
            }
            else
            {
                sb.Append(c);
                i++;
            }
        }
        return i;
    }

    private static string ResolveArg(string inner, IReadOnlyDictionary<string, string> values)
    {
        // {key, select, a {..} b {..} other {..}}
        var selectMatch = Regex.Match(inner, @"^\s*([\w-]+)\s*,\s*select\s*,(.*)$", RegexOptions.Singleline);
        if (selectMatch.Success)
        {
            var key = selectMatch.Groups[1].Value;
            var branches = selectMatch.Groups[2].Value;
            var chosen = values.TryGetValue(key, out var v) ? v : "";
            return SelectBranch(branches, chosen, values);
        }

        // {key}
        var keyMatch = Regex.Match(inner, @"^\s*([\w-]+)\s*$");
        if (keyMatch.Success)
        {
            return values.TryGetValue(keyMatch.Groups[1].Value, out var v) ? v : "";
        }

        // 其他复杂参数：简单展开为原始文本中的值替换
        var plain = Regex.Match(inner, @"^\s*([\w-]+)");
        return plain.Success && values.TryGetValue(plain.Groups[1].Value, out var pv) ? pv : "";
    }

    /// <summary>解析 select 分支体 "a {A} b {B} other {O}"，按 chosen 选分支（回退 other/undefined 分支）</summary>
    private static string SelectBranch(string branches, string chosen, IReadOnlyDictionary<string, string> values)
    {
        var sb = new StringBuilder();
        var i = 0;
        while (i < branches.Length)
        {
            // 跳过空白与逗号
            while (i < branches.Length && (char.IsWhiteSpace(branches[i]) || branches[i] == ','))
            {
                i++;
            }

            if (i >= branches.Length)
            {
                break;
            }

            // 分支名（到 { 或空白）
            var nameStart = i;
            while (i < branches.Length && branches[i] != '{' && !char.IsWhiteSpace(branches[i]))
            {
                i++;
            }

            var name = branches[nameStart..i].Trim();

            // 跳过分支名后空白
            while (i < branches.Length && char.IsWhiteSpace(branches[i]))
            {
                i++;
            }

            if (i >= branches.Length || branches[i] != '{') { i++; continue; }

            // 分支体（嵌套花括号平衡）
            var depth = 0;
            var j = i;
            for (; j < branches.Length; j++)
            {
                if (branches[j] == '{')
                {
                    depth++;
                }
                else if (branches[j] == '}')
                {
                    depth--; if (depth == 0)
                    {
                        break;
                    }
                }
            }
            var body = branches[(i + 1)..j];

            // 分支体可能含嵌套占位符（如 other { (rank {modRank}) }）→ 递归解析
            // 空值/缺失 → 匹配模板的 "undefined" 分支（作者用它表示"无值"）
            if (name == chosen || (chosen.Length == 0 && name == "undefined"))
            {
                return Format(body, values); // 精确匹配优先
            }

            sb.Append(name == "other" ? "other:" + body + ";" : ""); // 暂存 other
            i = j + 1;
        }

        // 无匹配：取 other 分支（去掉前缀标记）
        var other = sb.ToString();
        var idx = other.IndexOf("other:", StringComparison.Ordinal);
        if (idx >= 0)
        {
            var raw = other[(idx + "other:".Length)..other.LastIndexOf(';')];
            return Format(raw, values);
        }
        return "";
    }

    /// <summary>目标语言是否为中文（决定用中文模板还是英文模板）</summary>
    public static bool IsChineseLocale(string locale)
    {
        return locale is "zh" or "zh-hans" or "zh-hant" or "zh-cn" or "zh-tw";
    }

    /// <summary>
    /// 构建私信文本（OrderTop"联系"列使用）。
    /// 语言规则：目标语言为中文 → 中文模板；否则英文模板 + 末尾追加 "{对方语言物品名} 与 {价格}"（若有该语言 i18n）。
    /// </summary>
    public static string BuildMessage(
        string locale, string action, string ingameName, string itemName, string? itemNameLocalized,
        int? perTrade, string? subtype, int? modRank, int? ayatan, int price)
    {
        var isZh = IsChineseLocale(locale);
        var tplLang = isZh ? (locale == "zh-hant" ? "zh-hant" : "zh-hans") : "en";
        var tpl = GetTemplate(tplLang);

        var values = new Dictionary<string, string> {
            ["ingameName"] = ingameName,
            ["action"] = action, // buy/sell
            // 模板的 select 用 "undefined" 分支表示"无值"（如 undefined {} 空分支）。
            // perTrade=1（默认）不显示 x1：数量未必是 1（对方库存更多就买更多），x1 无信息量
            ["perTrade"] = perTrade is > 1 ? perTrade.ToString() ?? "undefined" : "undefined",
            ["subtype"] = subtype ?? "undefined",
            ["itemName"] = itemName,
            ["modRank"] = modRank?.ToString() ?? "undefined",
            ["ayatan"] = ayatan?.ToString() ?? "undefined",
            ["price"] = price.ToString(),
        };

        return Format(tpl, values).Trim();
    }
}
