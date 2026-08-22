using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200026D RID: 621
	public class PerformanceCounter
	{
		// Token: 0x17000D45 RID: 3397
		// (get) Token: 0x06002C92 RID: 11410 RVA: 0x00126918 File Offset: 0x00124B18
		// (set) Token: 0x06002C93 RID: 11411 RVA: 0x00126920 File Offset: 0x00124B20
		public double AverageFramesPerSecond { get; private set; }

		// Token: 0x17000D46 RID: 3398
		// (get) Token: 0x06002C94 RID: 11412 RVA: 0x00126929 File Offset: 0x00124B29
		// (set) Token: 0x06002C95 RID: 11413 RVA: 0x00126931 File Offset: 0x00124B31
		public double CurrentFramesPerSecond { get; private set; }

		// Token: 0x17000D47 RID: 3399
		// (get) Token: 0x06002C96 RID: 11414 RVA: 0x0012693A File Offset: 0x00124B3A
		// (set) Token: 0x06002C97 RID: 11415 RVA: 0x00126942 File Offset: 0x00124B42
		public double AverageFramesPerSecondInPastMinute { get; private set; }

		// Token: 0x17000D48 RID: 3400
		// (get) Token: 0x06002C98 RID: 11416 RVA: 0x0012694C File Offset: 0x00124B4C
		public IReadOnlyList<string> GetSavedIdentifiers
		{
			get
			{
				object obj = this.mutex;
				lock (obj)
				{
					this.tempSavedIdentifiers.Clear();
					this.tempSavedIdentifiers.AddRange(this.avgTicksPerFrame.Keys);
				}
				return this.tempSavedIdentifiers;
			}
		}

		// Token: 0x06002C99 RID: 11417 RVA: 0x001269B0 File Offset: 0x00124BB0
		public PerformanceCounter()
		{
			this.timer.Start();
		}

		// Token: 0x06002C9A RID: 11418 RVA: 0x00126A1C File Offset: 0x00124C1C
		public void AddElapsedTicks(string identifier, long ticks)
		{
			object obj = this.mutex;
			lock (obj)
			{
				if (!this.elapsedTicks.ContainsKey(identifier))
				{
					this.elapsedTicks.Add(identifier, new Queue<long>());
				}
				this.elapsedTicks[identifier].Enqueue(ticks);
				if (this.elapsedTicks[identifier].Count > 10)
				{
					this.elapsedTicks[identifier].Dequeue();
					this.avgTicksPerFrame[identifier] = (long)this.elapsedTicks[identifier].Average((long i) => i);
				}
			}
		}

		// Token: 0x06002C9B RID: 11419 RVA: 0x00126AEC File Offset: 0x00124CEC
		public float GetAverageElapsedMillisecs(string identifier)
		{
			long ticksPerFrame = 0L;
			object obj = this.mutex;
			lock (obj)
			{
				this.avgTicksPerFrame.TryGetValue(identifier, out ticksPerFrame);
			}
			return (float)ticksPerFrame * 1000f / (float)Stopwatch.Frequency;
		}

		// Token: 0x06002C9C RID: 11420 RVA: 0x00126B48 File Offset: 0x00124D48
		public bool Update(double deltaTime)
		{
			if (deltaTime == 0.0)
			{
				return false;
			}
			this.CurrentFramesPerSecond = 1.0 / deltaTime;
			this.sampleBuffer.Enqueue(this.CurrentFramesPerSecond);
			if (this.sampleBuffer.Count > 10)
			{
				this.sampleBuffer.Dequeue();
				this.AverageFramesPerSecond = this.sampleBuffer.Average();
			}
			else
			{
				this.AverageFramesPerSecond = this.CurrentFramesPerSecond;
			}
			long currentTime = this.timer.ElapsedMilliseconds;
			long currentSecond = currentTime / 1000L;
			if (currentSecond > this.lastSecondMark)
			{
				this.averageFramesPerSecondBuffer.Enqueue(this.AverageFramesPerSecond);
				this.lastSecondMark = currentSecond;
			}
			if (currentTime - this.lastMinuteMark >= 60000L && GameAnalyticsManager.ShouldLogRandomSample(GameAnalyticsManager.DataSampleSize.Small))
			{
				this.AverageFramesPerSecondInPastMinute = this.averageFramesPerSecondBuffer.Average();
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(4, 1);
				defaultInterpolatedStringHandler.AppendLiteral("FPS:");
				defaultInterpolatedStringHandler.AppendFormatted<int>(MathHelper.Clamp((int)this.AverageFramesPerSecondInPastMinute, 0, 144));
				GameAnalyticsManager.AddDesignEvent(defaultInterpolatedStringHandler.ToStringAndClear());
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(10, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("FPSLowest:");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(MathHelper.Clamp((int)this.averageFramesPerSecondBuffer.Min(), 0, 144));
				GameAnalyticsManager.AddDesignEvent(defaultInterpolatedStringHandler2.ToStringAndClear());
				this.averageFramesPerSecondBuffer.Clear();
				this.lastMinuteMark = currentTime;
			}
			return true;
		}

		// Token: 0x040015E6 RID: 5606
		private readonly object mutex = new object();

		// Token: 0x040015EA RID: 5610
		public const int MaximumSamples = 10;

		// Token: 0x040015EB RID: 5611
		private readonly Queue<double> sampleBuffer = new Queue<double>();

		// Token: 0x040015EC RID: 5612
		private readonly Queue<double> averageFramesPerSecondBuffer = new Queue<double>();

		// Token: 0x040015ED RID: 5613
		private readonly Stopwatch timer = new Stopwatch();

		// Token: 0x040015EE RID: 5614
		private long lastSecondMark;

		// Token: 0x040015EF RID: 5615
		private long lastMinuteMark;

		// Token: 0x040015F0 RID: 5616
		private readonly Dictionary<string, Queue<long>> elapsedTicks = new Dictionary<string, Queue<long>>();

		// Token: 0x040015F1 RID: 5617
		private readonly Dictionary<string, long> avgTicksPerFrame = new Dictionary<string, long>();

		// Token: 0x040015F2 RID: 5618
		private readonly List<string> tempSavedIdentifiers = new List<string>();

		// Token: 0x02000AE1 RID: 2785
		public class TickInfo
		{
			// Token: 0x170015A8 RID: 5544
			// (get) Token: 0x06005EAE RID: 24238 RVA: 0x00205942 File Offset: 0x00203B42
			// (set) Token: 0x06005EAF RID: 24239 RVA: 0x0020594A File Offset: 0x00203B4A
			public Queue<long> ElapsedTicks { get; set; } = new Queue<long>();

			// Token: 0x170015A9 RID: 5545
			// (get) Token: 0x06005EB0 RID: 24240 RVA: 0x00205953 File Offset: 0x00203B53
			// (set) Token: 0x06005EB1 RID: 24241 RVA: 0x0020595B File Offset: 0x00203B5B
			public long AvgTicksPerFrame { get; set; }
		}
	}
}
