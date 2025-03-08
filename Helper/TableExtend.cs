using Markdig.Extensions.Tables;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Warframe.Market.Helper;

public static class TableExtend
{

	public static IEnumerable<string> GetTableHeader(this Table table)
	{
		return table.Descendants<TableRow>().First().Descendants<TableCell>().Select(s => s.ExtractText());
	}
	public static string ExtractText(this TableCell cell)
	{
		return string.Concat(cell.ExtractTexts());
	}
	public static IEnumerable<string> ExtractTexts(this TableCell cell)
	{
		return cell.Descendants<LiteralInline>()
				.Select(l => l.Content.ToString().Trim())
				.Where(t => !string.IsNullOrEmpty(t));
	}

}
