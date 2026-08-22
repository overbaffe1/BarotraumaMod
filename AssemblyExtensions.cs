using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Barotrauma
{
	// Token: 0x020002F5 RID: 757
	public static class AssemblyExtensions
	{
		// Token: 0x06003DD6 RID: 15830 RVA: 0x00231514 File Offset: 0x0022F714
		public static IEnumerable<Type> GetSafeTypes(this Assembly assembly)
		{
			IEnumerable<Type> result;
			try
			{
				result = assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException re)
			{
				try
				{
					result = from x in re.Types
					where x != null
					select x;
				}
				catch (InvalidOperationException)
				{
					result = new List<Type>();
				}
			}
			catch (Exception)
			{
				result = new List<Type>();
			}
			return result;
		}
	}
}
