namespace Warframe.Market.Helper.Abstract;
public interface IConfigParser<in TSource, out TTarget>
{
	TTarget Parse(TSource source);
}
