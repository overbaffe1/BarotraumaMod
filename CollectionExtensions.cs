using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Barotrauma
{
	// Token: 0x020002F6 RID: 758
	public static class CollectionExtensions
	{
		// Token: 0x06003DD7 RID: 15831 RVA: 0x00231594 File Offset: 0x0022F794
		public static Task ParallelForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> funcBody, int maxDegreeOfParallelism = 4)
		{
			return Task.WhenAll(from p in Partitioner.Create<T>(source).GetPartitions(maxDegreeOfParallelism).AsParallel<IEnumerator<T>>()
			select base.<ParallelForEachAsync>g__AwaitParallelLimit|0(p));
		}
	}
}
