using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Barotrauma
{
	// Token: 0x020002C0 RID: 704
	public class ListDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IReadOnlyCollection<KeyValuePair<TKey, TValue>>
	{
		// Token: 0x06002FCE RID: 12238 RVA: 0x0014A458 File Offset: 0x00148658
		public ListDictionary(IReadOnlyList<TValue> list, int len, Func<int, TKey> keyFunc)
		{
			this.list = list;
			Dictionary<TKey, int> keyToIndex = new Dictionary<TKey, int>();
			for (int i = 0; i < len; i++)
			{
				keyToIndex.Add(keyFunc(i), i);
			}
			this.keyToIndex = keyToIndex.ToImmutableDictionary<TKey, int>();
		}

		// Token: 0x06002FCF RID: 12239 RVA: 0x0014A49E File Offset: 0x0014869E
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			ListDictionary<TKey, TValue>.<GetEnumerator>d__3 <GetEnumerator>d__ = new ListDictionary<TKey, TValue>.<GetEnumerator>d__3(0);
			<GetEnumerator>d__.<>4__this = this;
			return <GetEnumerator>d__;
		}

		// Token: 0x06002FD0 RID: 12240 RVA: 0x0014A4AD File Offset: 0x001486AD
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x17000DD7 RID: 3543
		// (get) Token: 0x06002FD1 RID: 12241 RVA: 0x0014A4B5 File Offset: 0x001486B5
		public int Count
		{
			get
			{
				return this.keyToIndex.Count;
			}
		}

		// Token: 0x06002FD2 RID: 12242 RVA: 0x0014A4C2 File Offset: 0x001486C2
		public bool ContainsKey(TKey key)
		{
			return this.keyToIndex.ContainsKey(key);
		}

		// Token: 0x06002FD3 RID: 12243 RVA: 0x0014A4D0 File Offset: 0x001486D0
		public bool TryGetValue(TKey key, out TValue value)
		{
			int index;
			if (this.keyToIndex.TryGetValue(key, out index))
			{
				value = this.list[index];
				return true;
			}
			value = default(TValue);
			return false;
		}

		// Token: 0x17000DD8 RID: 3544
		public TValue this[TKey key]
		{
			get
			{
				return this.list[this.keyToIndex[key]];
			}
		}

		// Token: 0x17000DD9 RID: 3545
		// (get) Token: 0x06002FD5 RID: 12245 RVA: 0x0014A522 File Offset: 0x00148722
		public IEnumerable<TKey> Keys
		{
			get
			{
				return this.keyToIndex.Keys;
			}
		}

		// Token: 0x17000DDA RID: 3546
		// (get) Token: 0x06002FD6 RID: 12246 RVA: 0x0014A52F File Offset: 0x0014872F
		public IEnumerable<TValue> Values
		{
			get
			{
				return from i in this.keyToIndex.Values
				select this.list[i];
			}
		}

		// Token: 0x040017FD RID: 6141
		private readonly ImmutableDictionary<TKey, int> keyToIndex;

		// Token: 0x040017FE RID: 6142
		private readonly IReadOnlyList<TValue> list;
	}
}
