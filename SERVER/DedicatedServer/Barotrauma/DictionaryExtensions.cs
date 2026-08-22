using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020002C5 RID: 709
	[NullableContext(1)]
	[Nullable(0)]
	public static class DictionaryExtensions
	{
		// Token: 0x06003013 RID: 12307 RVA: 0x0014AE57 File Offset: 0x00149057
		[return: Nullable(new byte[]
		{
			0,
			1,
			1
		})]
		public static NetDictionary<T, U> ToNetDictionary<T, [Nullable(2)] U>(this Dictionary<T, U> source)
		{
			return new NetDictionary<T, U>((from pair in source
			select new NetPair<T, U>(pair.Key, pair.Value)).ToImmutableArray<NetPair<T, U>>());
		}

		// Token: 0x06003014 RID: 12308 RVA: 0x0014AE88 File Offset: 0x00149088
		[return: Nullable(new byte[]
		{
			0,
			1,
			1
		})]
		public static NetDictionary<T, U> ToNetDictionary<T, [Nullable(2)] U>(this ImmutableDictionary<T, U> source)
		{
			return new NetDictionary<T, U>((from pair in source
			select new NetPair<T, U>(pair.Key, pair.Value)).ToImmutableArray<NetPair<T, U>>());
		}
	}
}
