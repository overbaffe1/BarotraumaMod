using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.LuaCs;
using FluentResults;

namespace Barotrauma
{
	// Token: 0x020002FC RID: 764
	public class PerformanceCounterService : IReusableService, IService, IDisposable
	{
		// Token: 0x17001061 RID: 4193
		// (get) Token: 0x06003E2B RID: 15915 RVA: 0x00232AB8 File Offset: 0x00230CB8
		// (set) Token: 0x06003E2C RID: 15916 RVA: 0x00232AC0 File Offset: 0x00230CC0
		public bool EnablePerformanceCounter { get; set; }

		// Token: 0x06003E2D RID: 15917 RVA: 0x00232ACC File Offset: 0x00230CCC
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

		// Token: 0x06003E2E RID: 15918 RVA: 0x00232B30 File Offset: 0x00230D30
		public T GetLatestSnapshot<T>(string identifier) where T : class, IPerformanceData
		{
			if (!this._data.ContainsKey(identifier))
			{
				return default(T);
			}
			return (T)((object)this._data[identifier].Last<IPerformanceData>());
		}

		// Token: 0x06003E2F RID: 15919 RVA: 0x00232B6C File Offset: 0x00230D6C
		public T[] GetSnapshot<T>(string identifier, int length) where T : class, IPerformanceData, new()
		{
			if (!this._data.ContainsKey(identifier))
			{
				return new T[0];
			}
			length = Math.Min(length, this._data[identifier].Count);
			return this._data[identifier].GetRange(this._data[identifier].Count - length, length).Cast<T>().ToArray<T>();
		}

		// Token: 0x06003E30 RID: 15920 RVA: 0x00232BD8 File Offset: 0x00230DD8
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

		// Token: 0x06003E31 RID: 15921 RVA: 0x00232C2D File Offset: 0x00230E2D
		public Result Reset()
		{
			this._data = new Dictionary<string, List<IPerformanceData>>();
			return Result.Ok();
		}

		// Token: 0x06003E32 RID: 15922 RVA: 0x00232C3F File Offset: 0x00230E3F
		public void Dispose()
		{
		}

		// Token: 0x17001062 RID: 4194
		// (get) Token: 0x06003E33 RID: 15923 RVA: 0x00232C41 File Offset: 0x00230E41
		public bool IsDisposed { get; }

		// Token: 0x0400207B RID: 8315
		private Dictionary<string, List<IPerformanceData>> _data = new Dictionary<string, List<IPerformanceData>>();
	}
}
