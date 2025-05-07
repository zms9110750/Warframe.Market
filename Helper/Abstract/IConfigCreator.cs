namespace Warframe.Market.Helper.Abstract;

public interface IConfigCreator<TFormat, out TTarget>
{
	public  TFormat Create();
}