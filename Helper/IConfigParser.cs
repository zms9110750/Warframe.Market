namespace Warframe.Market.Helper;
public interface IConfigParser<in TSource, out TTarget>
{
	TTarget Parse(TSource source);
}