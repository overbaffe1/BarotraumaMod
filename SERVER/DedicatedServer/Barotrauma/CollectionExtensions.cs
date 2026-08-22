using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Barotrauma
{
	// Token: 0x0200020D RID: 525
	public static class CollectionExtensions
	{
		// Token: 0x060024FF RID: 9471 RVA: 0x000F4108 File Offset: 0x000F2308
		public static Task ParallelForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> funcBody, int maxDegreeOfParallelism = 4)
		{
			return Task.WhenAll(from p in Partitioner.Create<T>(source).GetPartitions(maxDegreeOfParallelism).AsParallel<IEnumerator<T>>()
			select base.<ParallelForEachAsync>g__AwaitParallelLimit|0(p));
		}
	}
}
