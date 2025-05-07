using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Warframe.Market.Model.LocalItems;

namespace Warframe.Market.Extend;
#if DEBUG
internal static class DebugLock
{

	static Lock Lock = new();
	public static void TraceMessage(this object obj,
			string message = "",
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0,
			[CallerArgumentExpression(nameof(obj))] string callerArgumentExpression = "")
	{
		XElement element = new XElement("message");
		element.Add(new XAttribute("obj", obj switch
		{
			ItemShort s => s.I18n.ZhHans.Name,
			Exception e => e.Message,
			_ => obj.ToString() ?? "null"
		}));
		if (obj is Exception { InnerException: Exception e2 })
		{
			element.Add(new XAttribute("innerException", e2.Message));
		}
		element.Add(new XAttribute("message", message));
		element.Add(new XAttribute("memberName", memberName));
		element.Add(new XAttribute("sourceFilePath", sourceFilePath[sourceFilePath.LastIndexOf("\\")..]));
		element.Add(new XAttribute("sourceLineNumber", sourceLineNumber));
		element.Add(new XAttribute("callerArgumentExpression", callerArgumentExpression));
		Trace.WriteLine(element);
		lock (Lock)
		{
			File.AppendAllText("X:\\log.xml", element.ToString() + "\n");
		}
	}
}
#endif