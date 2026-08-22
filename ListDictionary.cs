using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Barotrauma
{
	// Token: 0x0200038B RID: 907
	public class ListDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IReadOnlyCollection<KeyValuePair<TKey, TValue>>
	{
		// Token: 0x0600444C RID: 17484 RVA: 0x00263EFC File Offset: 0x002620FC
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

		// Token: 0x0600444D RID: 17485 RVA: 0x00263F42 File Offset: 0x00262142
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			ListDictionary<TKey, TValue>.<GetEnumerator>d__3 <GetEnumerator>d__ = new ListDictionary<TKey, TValue>.<GetEnumerator>d__3(0);
			<GetEnumerator>d__.<>4__this = this;
			return <GetEnumerator>d__;
		}

		// Token: 0x0600444E RID: 17486 RVA: 0x00263F51 File Offset: 0x00262151
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x170011CC RID: 4556
		// (get) Token: 0x0600444F RID: 17487 RVA: 0x00263F59 File Offset: 0x00262159
		public int Count
		{
			get
			{
				return this.keyToIndex.Count;
			}
		}

		// Token: 0x06004450 RID: 17488 RVA: 0x00263F66 File Offset: 0x00262166
		public bool ContainsKey(TKey key)
		{
			return this.keyToIndex.ContainsKey(key);
		}

		// Token: 0x06004451 RID: 17489 RVA: 0x00263F74 File Offset: 0x00262174
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

		// Token: 0x170011CD RID: 4557
		public TValue this[TKey key]
		{
			get
			{
				return this.list[this.keyToIndex[key]];
			}
		}

		// Token: 0x170011CE RID: 4558
		// (get) Token: 0x06004453 RID: 17491 RVA: 0x00263FC6 File Offset: 0x002621C6
		public IEnumerable<TKey> Keys
		{
			get
			{
				return this.keyToIndex.Keys;
			}
		}

		// Token: 0x170011CF RID: 4559
		// (get) Token: 0x06004454 RID: 17492 RVA: 0x00263FD3 File Offset: 0x002621D3
		public IEnumerable<TValue> Values
		{
			get
			{
				return from i in this.keyToIndex.Values
				select this.list[i];
			}
		}

		// Token: 0x040023CF RID: 9167
		private readonly ImmutableDictionary<TKey, int> keyToIndex;

		// Token: 0x040023D0 RID: 9168
		private readonly IReadOnlyList<TValue> list;
	}
}
