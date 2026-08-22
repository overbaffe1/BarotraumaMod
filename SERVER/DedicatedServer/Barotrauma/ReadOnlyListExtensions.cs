using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma
{
	// Token: 0x020002C9 RID: 713
	public static class ReadOnlyListExtensions
	{
		// Token: 0x06003032 RID: 12338 RVA: 0x0014B278 File Offset: 0x00149478
		public static int IndexOf<T>(this IReadOnlyList<T> list, T elem)
		{
			return list.IndexOf((T input) => input.Equals(elem));
		}

		// Token: 0x06003033 RID: 12339 RVA: 0x0014B2A4 File Offset: 0x001494A4
		public static int IndexOf<T>(this IReadOnlyList<T> list, Func<T, bool> predicate)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (predicate(list[i]))
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x06003034 RID: 12340 RVA: 0x0014B2D4 File Offset: 0x001494D4
		public static T Find<T>(this IReadOnlyList<T> list, Func<T, bool> predicate)
		{
			return list.FirstOrDefault(predicate);
		}
	}
}
