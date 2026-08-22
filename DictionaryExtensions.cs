using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000390 RID: 912
	[NullableContext(1)]
	[Nullable(0)]
	public static class DictionaryExtensions
	{
		// Token: 0x06004491 RID: 17553 RVA: 0x002648FB File Offset: 0x00262AFB
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

		// Token: 0x06004492 RID: 17554 RVA: 0x0026492C File Offset: 0x00262B2C
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
