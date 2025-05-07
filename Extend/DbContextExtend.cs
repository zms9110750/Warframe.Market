using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warframe.Market.Extend;

public static class DbContextExtend
{
	public static void HasJsonConversion<TProperty>(this PropertyBuilder<TProperty> builder)
	{
		builder.HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
			 s => JsonSerializer.Deserialize<TProperty>(s, (JsonSerializerOptions)null!)!); 
	} 
}
