using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020002BF RID: 703
	public class LinkedPairSet<T1, T2> : IEnumerable<ValueTuple<T1, T2>>, IEnumerable
	{
		// Token: 0x06002FC2 RID: 12226 RVA: 0x0014A244 File Offset: 0x00148444
		public bool Contains(T1 t1)
		{
			return this.t1ToT2.ContainsKey(t1);
		}

		// Token: 0x06002FC3 RID: 12227 RVA: 0x0014A252 File Offset: 0x00148452
		public bool Contains(T2 t2)
		{
			return this.t2ToT1.ContainsKey(t2);
		}

		// Token: 0x17000DD5 RID: 3541
		public T2 this[T1 t1]
		{
			get
			{
				return this.t1ToT2[t1];
			}
			set
			{
				T2 prevT2 = this.t1ToT2[t1];
				this.t2ToT1.Remove(prevT2);
				this.t2ToT1.Add(value, t1);
				this.t1ToT2[t1] = value;
			}
		}

		// Token: 0x17000DD6 RID: 3542
		public T1 this[T2 t2]
		{
			get
			{
				return this.t2ToT1[t2];
			}
			set
			{
				T1 prevT = this.t2ToT1[t2];
				this.t1ToT2.Remove(prevT);
				this.t1ToT2.Add(value, t2);
				this.t2ToT1[t2] = value;
			}
		}

		// Token: 0x06002FC8 RID: 12232 RVA: 0x0014A304 File Offset: 0x00148504
		public void Add(T1 t1, T2 t2)
		{
			if (this.Contains(t1))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(18, 2);
				defaultInterpolatedStringHandler.AppendFormatted(base.GetType().Name);
				defaultInterpolatedStringHandler.AppendLiteral(" already contains ");
				defaultInterpolatedStringHandler.AppendFormatted<T1>(t1);
				throw new ArgumentException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			if (this.Contains(t2))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(18, 2);
				defaultInterpolatedStringHandler2.AppendFormatted(base.GetType().Name);
				defaultInterpolatedStringHandler2.AppendLiteral(" already contains ");
				defaultInterpolatedStringHandler2.AppendFormatted<T2>(t2);
				throw new ArgumentException(defaultInterpolatedStringHandler2.ToStringAndClear());
			}
			this.t1ToT2.Add(t1, t2);
			this.t2ToT1.Add(t2, t1);
		}

		// Token: 0x06002FC9 RID: 12233 RVA: 0x0014A3B8 File Offset: 0x001485B8
		public void Remove(T1 t1)
		{
			T2 t2 = this.t1ToT2[t1];
			this.t1ToT2.Remove(t1);
			this.t2ToT1.Remove(t2);
		}

		// Token: 0x06002FCA RID: 12234 RVA: 0x0014A3EC File Offset: 0x001485EC
		public void Remove(T2 t2)
		{
			T1 t3 = this.t2ToT1[t2];
			this.t1ToT2.Remove(t3);
			this.t2ToT1.Remove(t2);
		}

		// Token: 0x06002FCB RID: 12235 RVA: 0x0014A420 File Offset: 0x00148620
		public IEnumerator<ValueTuple<T1, T2>> GetEnumerator()
		{
			LinkedPairSet<T1, T2>.<GetEnumerator>d__13 <GetEnumerator>d__ = new LinkedPairSet<T1, T2>.<GetEnumerator>d__13(0);
			<GetEnumerator>d__.<>4__this = this;
			return <GetEnumerator>d__;
		}

		// Token: 0x06002FCC RID: 12236 RVA: 0x0014A42F File Offset: 0x0014862F
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x040017FB RID: 6139
		private readonly Dictionary<T1, T2> t1ToT2 = new Dictionary<T1, T2>();

		// Token: 0x040017FC RID: 6140
		private readonly Dictionary<T2, T1> t2ToT1 = new Dictionary<T2, T1>();
	}
}
