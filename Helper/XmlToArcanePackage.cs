﻿using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Warframe.Market.Model.Items;

namespace Warframe.Market.Helper;

public class XmlToArcanePackage : IConfigParser<XElement, ArcanePackage>, IConfigParser<XElement, ArcanePackage[]>
{
	public ArcanePackage Parse(XElement source)
	{
		var pack = new ArcanePackage(source.Name.ToString());
		foreach (var item in source.Elements())
		{
			pack.Add(Enum.Parse<Subtypes>(item.Name.ToString()), (double)item.Attribute("Quality")!, item.Elements().Select(s => s.Name.ToString()));
		}
		return pack;
	}
	ArcanePackage[] IConfigParser<XElement, ArcanePackage[]>.Parse(XElement source)
	{
		return [.. source.Elements().Select(Parse)];
	}
	public static string GetArcaneXml()
	{
		using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Warframe.Market.Configuration.赋能包.xml");
		using StreamReader reader = new StreamReader(stream!, Encoding.UTF8);
		return reader.ReadToEnd();
	}
}
