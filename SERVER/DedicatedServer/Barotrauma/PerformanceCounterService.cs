using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.LuaCs;
using FluentResults;

namespace Barotrauma
{
	// Token: 0x02000214 RID: 532
	public class PerformanceCounterService : IReusableService, IService, IDisposable
	{
		// Token: 0x17000AC9 RID: 2761
		// (get) Token: 0x06002544 RID: 9540 RVA: 0x000F512B File Offset: 0x000F332B
		// (set) Token: 0x06002545 RID: 9541 RVA: 0x000F5133 File Offset: 0x000F3333
		public bool EnablePerformanceCounter { get; set; }

		// Token: 0x06002546 RID: 9542 RVA: 0x000F513C File Offset: 0x000F333C
		public void AddElapsedTicks(IPerformanceData data)
		{
			if (!this.EnablePerformanceCounter)
			{
				return;
			}
			if (!this._data.ContainsKey(data.Identifier))
			{
				this._data.Add(data.Identifier, new List<IPerformanceData>());
			}
			this._data[data.Identifier].Add(data);
			this.Trim(data.Identifier, 100);
		}

		// Token: 0x06002547 RID: 9543 RVA: 0x000F51A0 File Offset: 0x000F33A0
		public T GetLatestSnapshot<T>(string identifier) where T : class, IPerformanceData
		{
			if (!this._data.ContainsKey(identifier))
			{
				return default(T);
			}
			return (T)((object)this._data[identifier].Last<IPerformanceData>());
		}

		// Token: 0x06002548 RID: 9544 RVA: 0x000F51DC File Offset: 0x000F33DC
		public T[] GetSnapshot<T>(string identifier, int length) where T : class, IPerformanceData, new()
		{
			if (!this._data.ContainsKey(identifier))
			{
				return new T[0];
			}
			length = Math.Min(length, this._data[identifier].Count);
			return this._data[identifier].GetRange(this._data[identifier].Count - length, length).Cast<T>().ToArray<T>();
		}

		// Token: 0x06002549 RID: 9545 RVA: 0x000F5248 File Offset: 0x000F3448
		public void Trim(string identifier, int maxSize)
		{
			if (!this._data.ContainsKey(identifier))
			{
				return;
			}
			if (this._data[identifier].Count > maxSize)
			{
				this._data[identifier].RemoveRange(0, this._data[identifier].Count - maxSize);
			}
		}

		// Token: 0x0600254A RID: 9546 RVA: 0x000F529D File Offset: 0x000F349D
		public Result Reset()
		{
			this._data = new Dictionary<string, List<IPerformanceData>>();
			return Result.Ok();
		}

		// Token: 0x0600254B RID: 9547 RVA: 0x000F52AF File Offset: 0x000F34AF
		public void Dispose()
		{
		}

		// Token: 0x17000ACA RID: 2762
		// (get) Token: 0x0600254C RID: 9548 RVA: 0x000F52B1 File Offset: 0x000F34B1
		public bool IsDisposed { get; }

		// Token: 0x0400124F RID: 4687
		private Dictionary<string, List<IPerformanceData>> _data = new Dictionary<string, List<IPerformanceData>>();
	}
}
