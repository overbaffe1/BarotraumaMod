using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200038A RID: 906
	public class LinkedPairSet<T1, T2> : IEnumerable<ValueTuple<T1, T2>>, IEnumerable
	{
		// Token: 0x06004440 RID: 17472 RVA: 0x00263CE8 File Offset: 0x00261EE8
		public bool Contains(T1 t1)
		{
			return this.t1ToT2.ContainsKey(t1);
		}

		// Token: 0x06004441 RID: 17473 RVA: 0x00263CF6 File Offset: 0x00261EF6
		public bool Contains(T2 t2)
		{
			return this.t2ToT1.ContainsKey(t2);
		}

		// Token: 0x170011CA RID: 4554
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

		// Token: 0x170011CB RID: 4555
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

		// Token: 0x06004446 RID: 17478 RVA: 0x00263DA8 File Offset: 0x00261FA8
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

		// Token: 0x06004447 RID: 17479 RVA: 0x00263E5C File Offset: 0x0026205C
		public void Remove(T1 t1)
		{
			T2 t2 = this.t1ToT2[t1];
			this.t1ToT2.Remove(t1);
			this.t2ToT1.Remove(t2);
		}

		// Token: 0x06004448 RID: 17480 RVA: 0x00263E90 File Offset: 0x00262090
		public void Remove(T2 t2)
		{
			T1 t3 = this.t2ToT1[t2];
			this.t1ToT2.Remove(t3);
			this.t2ToT1.Remove(t2);
		}

		// Token: 0x06004449 RID: 17481 RVA: 0x00263EC4 File Offset: 0x002620C4
		public IEnumerator<ValueTuple<T1, T2>> GetEnumerator()
		{
			LinkedPairSet<T1, T2>.<GetEnumerator>d__13 <GetEnumerator>d__ = new LinkedPairSet<T1, T2>.<GetEnumerator>d__13(0);
			<GetEnumerator>d__.<>4__this = this;
			return <GetEnumerator>d__;
		}

		// Token: 0x0600444A RID: 17482 RVA: 0x00263ED3 File Offset: 0x002620D3
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x040023CD RID: 9165
		private readonly Dictionary<T1, T2> t1ToT2 = new Dictionary<T1, T2>();

		// Token: 0x040023CE RID: 9166
		private readonly Dictionary<T2, T1> t2ToT1 = new Dictionary<T2, T1>();
	}
}
