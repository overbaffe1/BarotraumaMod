using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma.Extensions
{
	// Token: 0x020002E4 RID: 740
	[NullableContext(1)]
	[Nullable(0)]
	public static class IEnumerableExtensions
	{
		// Token: 0x06003167 RID: 12647 RVA: 0x00151394 File Offset: 0x0014F594
		public static T[] Randomize<[Nullable(2)] T>(this IList<T> source, Rand.RandSync randSync)
		{
			return (from i in source
			orderby Rand.Value(randSync)
			select i).ToArray<T>();
		}

		// Token: 0x06003168 RID: 12648 RVA: 0x001513C5 File Offset: 0x0014F5C5
		public static void Shuffle<[Nullable(2)] T>(this IList<T> list, Rand.RandSync randSync)
		{
			list.Shuffle(Rand.GetRNG(randSync));
		}

		// Token: 0x06003169 RID: 12649 RVA: 0x001513D4 File Offset: 0x0014F5D4
		public static void Shuffle<[Nullable(2)] T>(this IList<T> list, Random rng)
		{
			int i = list.Count;
			while (i > 1)
			{
				i--;
				int j = rng.Next(i + 1);
				T value = list[j];
				list[j] = list[i];
				list[i] = value;
			}
		}

		// Token: 0x0600316A RID: 12650 RVA: 0x0015141A File Offset: 0x0014F61A
		[return: Nullable(2)]
		public static T GetRandom<[Nullable(2)] T>(this IReadOnlyList<T> source, Func<T, bool> predicate, Rand.RandSync randSync)
		{
			if (predicate == null)
			{
				return source.GetRandom(randSync);
			}
			return source.Where(predicate).ToArray<T>().GetRandom(randSync);
		}

		// Token: 0x0600316B RID: 12651 RVA: 0x0015143C File Offset: 0x0014F63C
		[NullableContext(2)]
		public static T GetRandom<T>([Nullable(1)] this IReadOnlyList<T> source, Rand.RandSync randSync)
		{
			int count = source.Count;
			if (count != 0)
			{
				return source[Rand.Range(0, count, randSync)];
			}
			return default(T);
		}

		// Token: 0x0600316C RID: 12652 RVA: 0x0015146C File Offset: 0x0014F66C
		[return: Nullable(2)]
		public static T GetRandom<[Nullable(2)] T>(this IReadOnlyList<T> source, Random random)
		{
			int count = source.Count;
			if (count != 0)
			{
				return source[random.Next(0, count)];
			}
			return default(T);
		}

		// Token: 0x0600316D RID: 12653 RVA: 0x0015149B File Offset: 0x0014F69B
		[return: Nullable(2)]
		public static T GetRandomUnsynced<[Nullable(2)] T>(this IEnumerable<T> source, Func<T, bool> predicate)
		{
			if (predicate == null)
			{
				return source.GetRandomUnsynced<T>();
			}
			return source.Where(predicate).GetRandomUnsynced<T>();
		}

		// Token: 0x0600316E RID: 12654 RVA: 0x001514B4 File Offset: 0x0014F6B4
		[NullableContext(2)]
		public static T GetRandomUnsynced<T>([Nullable(1)] this IEnumerable<T> source)
		{
			IReadOnlyList<T> list = source as IReadOnlyList<T>;
			if (list != null)
			{
				return list.GetRandom(Rand.RandSync.Unsynced);
			}
			int count = source.Count<T>();
			if (count != 0)
			{
				return source.ElementAt(Rand.Range(0, count, Rand.RandSync.Unsynced));
			}
			return default(T);
		}

		// Token: 0x0600316F RID: 12655 RVA: 0x001514F5 File Offset: 0x0014F6F5
		[return: Nullable(2)]
		public static T GetRandom<[Nullable(0)] T>(this IEnumerable<T> source, Random rand) where T : PrefabWithUintIdentifier
		{
			return (from p in source
			orderby p.UintIdentifier
			select p).ToArray<T>().GetRandom(rand);
		}

		// Token: 0x06003170 RID: 12656 RVA: 0x00151527 File Offset: 0x0014F727
		[return: Nullable(2)]
		public static T GetRandom<[Nullable(0)] T>(this IEnumerable<T> source, Rand.RandSync randSync) where T : PrefabWithUintIdentifier
		{
			return (from p in source
			orderby p.UintIdentifier
			select p).ToArray<T>().GetRandom(randSync);
		}

		// Token: 0x06003171 RID: 12657 RVA: 0x00151559 File Offset: 0x0014F759
		[return: Nullable(2)]
		public static T GetRandom<[Nullable(0)] T>(this IEnumerable<T> source, Func<T, bool> predicate, Rand.RandSync randSync) where T : PrefabWithUintIdentifier
		{
			return (from p in source.Where(predicate)
			orderby p.UintIdentifier
			select p).ToArray<T>().GetRandom(randSync);
		}

		// Token: 0x06003172 RID: 12658 RVA: 0x00151591 File Offset: 0x0014F791
		public static T GetRandomByWeight<[Nullable(2)] T>(this IEnumerable<T> source, Func<T, float> weightSelector, Rand.RandSync randSync)
		{
			return ToolBox.SelectWeightedRandom<T>(source, weightSelector, randSync);
		}

		// Token: 0x06003173 RID: 12659 RVA: 0x0015159C File Offset: 0x0014F79C
		public static void ForEachMod<[Nullable(2)] T>(this IEnumerable<T> source, Action<T> action)
		{
			if (source.None(null))
			{
				return;
			}
			List<T> temp = new List<T>(source);
			temp.ForEach(action);
		}

		// Token: 0x06003174 RID: 12660 RVA: 0x001515C4 File Offset: 0x0014F7C4
		public static void ForEach<[Nullable(2)] T>(this IEnumerable<T> source, Action<T> action)
		{
			foreach (T item in source)
			{
				action(item);
			}
		}

		// Token: 0x06003175 RID: 12661 RVA: 0x0015160C File Offset: 0x0014F80C
		public static void Consume<[Nullable(2)] T>(this IEnumerable<T> enumerable)
		{
			foreach (T _ in enumerable)
			{
			}
		}

		// Token: 0x06003176 RID: 12662 RVA: 0x00151650 File Offset: 0x0014F850
		public static bool None<[Nullable(2)] T>(this IEnumerable<T> source, [Nullable(new byte[]
		{
			2,
			1
		})] Func<T, bool> predicate = null)
		{
			if (predicate == null)
			{
				return !source.Any<T>();
			}
			return !source.Any(predicate);
		}

		// Token: 0x06003177 RID: 12663 RVA: 0x00151669 File Offset: 0x0014F869
		public static bool Multiple<[Nullable(2)] T>(this IEnumerable<T> source, [Nullable(new byte[]
		{
			2,
			1
		})] Func<T, bool> predicate = null)
		{
			if (predicate == null)
			{
				return source.Count<T>() > 1;
			}
			return source.Count(predicate) > 1;
		}

		// Token: 0x06003178 RID: 12664 RVA: 0x00151682 File Offset: 0x0014F882
		public static IEnumerable<T> ToEnumerable<[Nullable(2)] T>(this T item)
		{
			IEnumerableExtensions.<ToEnumerable>d__17<T> <ToEnumerable>d__ = new IEnumerableExtensions.<ToEnumerable>d__17<T>(-2);
			<ToEnumerable>d__.<>3__item = item;
			return <ToEnumerable>d__;
		}

		// Token: 0x06003179 RID: 12665 RVA: 0x00151692 File Offset: 0x0014F892
		public static IEnumerable<T> NotNull<T>([Nullable(new byte[]
		{
			1,
			2
		})] this IEnumerable<T> enumerable) where T : class
		{
			IEnumerableExtensions.<NotNull>d__18<T> <NotNull>d__ = new IEnumerableExtensions.<NotNull>d__18<T>(-2);
			<NotNull>d__.<>3__enumerable = enumerable;
			return <NotNull>d__;
		}

		// Token: 0x0600317A RID: 12666 RVA: 0x001516A4 File Offset: 0x0014F8A4
		public static IEnumerable<T> SelectManyRecursive<[Nullable(2)] T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> selector)
		{
			IEnumerable<T> result = source.SelectMany(selector);
			if (!result.Any<T>())
			{
				return result;
			}
			return result.Concat(result.SelectManyRecursive(selector));
		}

		// Token: 0x0600317B RID: 12667 RVA: 0x001516D0 File Offset: 0x0014F8D0
		public static void AddIfNotNull<[Nullable(2)] T>(this IList<T> source, T value)
		{
			if (value != null)
			{
				source.Add(value);
			}
		}

		// Token: 0x0600317C RID: 12668 RVA: 0x001516E1 File Offset: 0x0014F8E1
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		public static NetCollection<T> ToNetCollection<[Nullable(2)] T>(this IEnumerable<T> enumerable)
		{
			return new NetCollection<T>(enumerable.ToImmutableArray<T>());
		}

		// Token: 0x0600317D RID: 12669 RVA: 0x001516F0 File Offset: 0x0014F8F0
		public static bool AtLeast<[Nullable(2)] T>(this IEnumerable<T> source, int amount, Predicate<T> predicate)
		{
			foreach (T elem in source)
			{
				if (predicate(elem))
				{
					amount--;
				}
				if (amount <= 0)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600317E RID: 12670 RVA: 0x0015174C File Offset: 0x0014F94C
		public static ICollection<T> CollectionConcat<[Nullable(2)] T>(this IEnumerable<T> self, IEnumerable<T> other)
		{
			return new CollectionConcat<T>(self, other);
		}

		// Token: 0x0600317F RID: 12671 RVA: 0x00151755 File Offset: 0x0014F955
		public static IReadOnlyList<T> ListConcat<[Nullable(2)] T>(this IEnumerable<T> self, IEnumerable<T> other)
		{
			return new ListConcat<T>(self, other);
		}
	}
}
