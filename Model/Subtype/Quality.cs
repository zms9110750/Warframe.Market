using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warframe.Market.Model.Subtype;
public enum Quality
{
	All,
	/// <summary>
	/// 常见
	/// </summary>
	Common,
	/// <summary>
	/// 罕见
	/// </summary>
	Uncommon,
	/// <summary>
	/// 稀有
	/// </summary>
	Rare,
	/// <summary>
	/// 传说
	/// </summary>
	Legendary
}