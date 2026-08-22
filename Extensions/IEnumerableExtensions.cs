using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma.Extensions
{
	// Token: 0x020003AB RID: 939
	[NullableContext(1)]
	[Nullable(0)]
	public static class IEnumerableExtensions
	{
		// Token: 0x060045A4 RID: 17828 RVA: 0x00269454 File Offset: 0x00267654
		public static T[] Randomize<[Nullable(2)] T>(this IList<T> source, Rand.RandSync randSync)
		{
			return (from i in source
			orderby Rand.Value(randSync)
			select i).ToArray<T>();
		}

		// Token: 0x060045A5 RID: 17829 RVA: 0x00269485 File Offset: 0x00267685
		public static void Shuffle<[Nullable(2)] T>(this IList<T> list, Rand.RandSync randSync)
		{
			list.Shuffle(Rand.GetRNG(randSync));
		}

		// Token: 0x060045A6 RID: 17830 RVA: 0x00269494 File Offset: 0x00267694
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

		// Token: 0x060045A7 RID: 17831 RVA: 0x002694DA File Offset: 0x002676DA
		[return: Nullable(2)]
		public static T GetRandom<[Nullable(2)] T>(this IReadOnlyList<T> source, Func<T, bool> predicate, Rand.RandSync randSync)
		{
			if (predicate == null)
			{
				return source.GetRandom(randSync);
			}
			return source.Where(predicate).ToArray<T>().GetRandom(randSync);
		}

		// Token: 0x060045A8 RID: 17832 RVA: 0x002694FC File Offset: 0x002676FC
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

		// Token: 0x060045A9 RID: 17833 RVA: 0x0026952C File Offset: 0x0026772C
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

		// Token: 0x060045AA RID: 17834 RVA: 0x0026955B File Offset: 0x0026775B
		[return: Nullable(2)]
		public static T GetRandomUnsynced<[Nullable(2)] T>(this IEnumerable<T> source, Func<T, bool> predicate)
		{
			if (predicate == null)
			{
				return source.GetRandomUnsynced<T>();
			}
			return source.Where(predicate).GetRandomUnsynced<T>();
		}

		// Token: 0x060045AB RID: 17835 RVA: 0x00269574 File Offset: 0x00267774
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

		// Token: 0x060045AC RID: 17836 RVA: 0x002695B5 File Offset: 0x002677B5
		[return: Nullable(2)]
		public static T GetRandom<[Nullable(0)] T>(this IEnumerable<T> source, Random rand) where T : PrefabWithUintIdentifier
		{
			return (from p in source
			orderby p.UintIdentifier
			select p).ToArray<T>().GetRandom(rand);
		}

		// Token: 0x060045AD RID: 17837 RVA: 0x002695E7 File Offset: 0x002677E7
		[return: Nullable(2)]
		public static T GetRandom<[Nullable(0)] T>(this IEnumerable<T> source, Rand.RandSync randSync) where T : PrefabWithUintIdentifier
		{
			return (from p in source
			orderby p.UintIdentifier
			select p).ToArray<T>().GetRandom(randSync);
		}

		// Token: 0x060045AE RID: 17838 RVA: 0x00269619 File Offset: 0x00267819
		[return: Nullable(2)]
		public static T GetRandom<[Nullable(0)] T>(this IEnumerable<T> source, Func<T, bool> predicate, Rand.RandSync randSync) where T : PrefabWithUintIdentifier
		{
			return (from p in source.Where(predicate)
			orderby p.UintIdentifier
			select p).ToArray<T>().GetRandom(randSync);
		}

		// Token: 0x060045AF RID: 17839 RVA: 0x00269651 File Offset: 0x00267851
		public static T GetRandomByWeight<[Nullable(2)] T>(this IEnumerable<T> source, Func<T, float> weightSelector, Rand.RandSync randSync)
		{
			return ToolBox.SelectWeightedRandom<T>(source, weightSelector, randSync);
		}

		// Token: 0x060045B0 RID: 17840 RVA: 0x0026965C File Offset: 0x0026785C
		public static void ForEachMod<[Nullable(2)] T>(this IEnumerable<T> source, Action<T> action)
		{
			if (source.None(null))
			{
				return;
			}
			List<T> temp = new List<T>(source);
			temp.ForEach(action);
		}

		// Token: 0x060045B1 RID: 17841 RVA: 0x00269684 File Offset: 0x00267884
		public static void ForEach<[Nullable(2)] T>(this IEnumerable<T> source, Action<T> action)
		{
			foreach (T item in source)
			{
				action(item);
			}
		}

		// Token: 0x060045B2 RID: 17842 RVA: 0x002696CC File Offset: 0x002678CC
		public static void Consume<[Nullable(2)] T>(this IEnumerable<T> enumerable)
		{
			foreach (T _ in enumerable)
			{
			}
		}

		// Token: 0x060045B3 RID: 17843 RVA: 0x00269710 File Offset: 0x00267910
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

		// Token: 0x060045B4 RID: 17844 RVA: 0x00269729 File Offset: 0x00267929
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

		// Token: 0x060045B5 RID: 17845 RVA: 0x00269742 File Offset: 0x00267942
		public static IEnumerable<T> ToEnumerable<[Nullable(2)] T>(this T item)
		{
			IEnumerableExtensions.<ToEnumerable>d__17<T> <ToEnumerable>d__ = new IEnumerableExtensions.<ToEnumerable>d__17<T>(-2);
			<ToEnumerable>d__.<>3__item = item;
			return <ToEnumerable>d__;
		}

		// Token: 0x060045B6 RID: 17846 RVA: 0x00269752 File Offset: 0x00267952
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

		// Token: 0x060045B7 RID: 17847 RVA: 0x00269764 File Offset: 0x00267964
		public static IEnumerable<T> SelectManyRecursive<[Nullable(2)] T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> selector)
		{
			IEnumerable<T> result = source.SelectMany(selector);
			if (!result.Any<T>())
			{
				return result;
			}
			return result.Concat(result.SelectManyRecursive(selector));
		}

		// Token: 0x060045B8 RID: 17848 RVA: 0x00269790 File Offset: 0x00267990
		public static void AddIfNotNull<[Nullable(2)] T>(this IList<T> source, T value)
		{
			if (value != null)
			{
				source.Add(value);
			}
		}

		// Token: 0x060045B9 RID: 17849 RVA: 0x002697A1 File Offset: 0x002679A1
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		public static NetCollection<T> ToNetCollection<[Nullable(2)] T>(this IEnumerable<T> enumerable)
		{
			return new NetCollection<T>(enumerable.ToImmutableArray<T>());
		}

		// Token: 0x060045BA RID: 17850 RVA: 0x002697B0 File Offset: 0x002679B0
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

		// Token: 0x060045BB RID: 17851 RVA: 0x0026980C File Offset: 0x00267A0C
		public static ICollection<T> CollectionConcat<[Nullable(2)] T>(this IEnumerable<T> self, IEnumerable<T> other)
		{
			return new CollectionConcat<T>(self, other);
		}

		// Token: 0x060045BC RID: 17852 RVA: 0x00269815 File Offset: 0x00267A15
		public static IReadOnlyList<T> ListConcat<[Nullable(2)] T>(this IEnumerable<T> self, IEnumerable<T> other)
		{
			return new ListConcat<T>(self, other);
		}
	}
}
