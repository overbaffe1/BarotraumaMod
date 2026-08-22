using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Barotrauma
{
	// Token: 0x0200020C RID: 524
	public static class AssemblyExtensions
	{
		// Token: 0x060024FE RID: 9470 RVA: 0x000F4088 File Offset: 0x000F2288
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
