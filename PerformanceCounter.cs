using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000340 RID: 832
	public class PerformanceCounter
	{
		// Token: 0x1700116A RID: 4458
		// (get) Token: 0x060041B1 RID: 16817 RVA: 0x00246C5C File Offset: 0x00244E5C
		// (set) Token: 0x060041B2 RID: 16818 RVA: 0x00246C64 File Offset: 0x00244E64
		public double AverageFramesPerSecond { get; private set; }

		// Token: 0x1700116B RID: 4459
		// (get) Token: 0x060041B3 RID: 16819 RVA: 0x00246C6D File Offset: 0x00244E6D
		// (set) Token: 0x060041B4 RID: 16820 RVA: 0x00246C75 File Offset: 0x00244E75
		public double CurrentFramesPerSecond { get; private set; }

		// Token: 0x1700116C RID: 4460
		// (get) Token: 0x060041B5 RID: 16821 RVA: 0x00246C7E File Offset: 0x00244E7E
		// (set) Token: 0x060041B6 RID: 16822 RVA: 0x00246C86 File Offset: 0x00244E86
		public double AverageFramesPerSecondInPastMinute { get; private set; }

		// Token: 0x1700116D RID: 4461
		// (get) Token: 0x060041B7 RID: 16823 RVA: 0x00246C90 File Offset: 0x00244E90
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

		// Token: 0x060041B8 RID: 16824 RVA: 0x00246CF4 File Offset: 0x00244EF4
		public PerformanceCounter()
		{
			this.timer.Start();
		}

		// Token: 0x060041B9 RID: 16825 RVA: 0x00246D80 File Offset: 0x00244F80
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

		// Token: 0x060041BA RID: 16826 RVA: 0x00246E50 File Offset: 0x00245050
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

		// Token: 0x060041BB RID: 16827 RVA: 0x00246EAC File Offset: 0x002450AC
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

		// Token: 0x0400222C RID: 8748
		private readonly object mutex = new object();

		// Token: 0x04002230 RID: 8752
		public const int MaximumSamples = 10;

		// Token: 0x04002231 RID: 8753
		private readonly Queue<double> sampleBuffer = new Queue<double>();

		// Token: 0x04002232 RID: 8754
		private readonly Queue<double> averageFramesPerSecondBuffer = new Queue<double>();

		// Token: 0x04002233 RID: 8755
		private readonly Stopwatch timer = new Stopwatch();

		// Token: 0x04002234 RID: 8756
		private long lastSecondMark;

		// Token: 0x04002235 RID: 8757
		private long lastMinuteMark;

		// Token: 0x04002236 RID: 8758
		private readonly Dictionary<string, Queue<long>> elapsedTicks = new Dictionary<string, Queue<long>>();

		// Token: 0x04002237 RID: 8759
		private readonly Dictionary<string, long> avgTicksPerFrame = new Dictionary<string, long>();

		// Token: 0x04002238 RID: 8760
		internal Graph UpdateTimeGraph = new Graph(500);

		// Token: 0x04002239 RID: 8761
		internal Graph DrawTimeGraph = new Graph(500);

		// Token: 0x0400223A RID: 8762
		private readonly List<string> tempSavedIdentifiers = new List<string>();

		// Token: 0x02001059 RID: 4185
		public class TickInfo
		{
			// Token: 0x17001C65 RID: 7269
			// (get) Token: 0x06008C3C RID: 35900 RVA: 0x003AF15A File Offset: 0x003AD35A
			// (set) Token: 0x06008C3D RID: 35901 RVA: 0x003AF162 File Offset: 0x003AD362
			public Queue<long> ElapsedTicks { get; set; } = new Queue<long>();

			// Token: 0x17001C66 RID: 7270
			// (get) Token: 0x06008C3E RID: 35902 RVA: 0x003AF16B File Offset: 0x003AD36B
			// (set) Token: 0x06008C3F RID: 35903 RVA: 0x003AF173 File Offset: 0x003AD373
			public long AvgTicksPerFrame { get; set; }
		}
	}
}
