using Autofac;
using System.Xml.Linq;
using Warframe.Market.Helper.Abstract;

namespace Warframe.Market.Helper.AutofacModule;

public sealed class ArcanePackageModule : Module
{
	protected override void Load(ContainerBuilder builder)
	{
		builder.RegisterType<XmlToArcanePackage>()
			.AsImplementedInterfaces()
			.SingleInstance();
		builder.Register(r =>
		{
			var a = r.Resolve<IConfigCreator<XElement, ArcanePackage>>();
			var b = r.Resolve<IConfigParser<XElement, IEnumerable<ArcanePackage>>>();
			var c = b.Parse(a.Create());
			return c.ToArray();
		}).As<IEnumerable<ArcanePackage>>().As<ArcanePackage[]>().SingleInstance();
	}
}
