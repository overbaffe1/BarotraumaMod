using System;
using System.Runtime.CompilerServices;
using Barotrauma.IO;
using Microsoft.Xna.Framework;

namespace Barotrauma.Sounds
{
	// Token: 0x0200043C RID: 1084
	internal abstract class Sound : IDisposable
	{
		// Token: 0x17001270 RID: 4720
		// (get) Token: 0x06004865 RID: 18533 RVA: 0x0027A206 File Offset: 0x00278406
		public bool Disposed
		{
			get
			{
				return this.disposed;
			}
		}

		// Token: 0x17001271 RID: 4721
		// (get) Token: 0x06004866 RID: 18534
		public abstract double? DurationSeconds { get; }

		// Token: 0x17001272 RID: 4722
		// (get) Token: 0x06004867 RID: 18535 RVA: 0x0027A20E File Offset: 0x0027840E
		// (set) Token: 0x06004868 RID: 18536 RVA: 0x0027A216 File Offset: 0x00278416
		public bool Loading { get; protected set; }

		// Token: 0x17001273 RID: 4723
		// (get) Token: 0x06004869 RID: 18537 RVA: 0x0027A21F File Offset: 0x0027841F
		public virtual SoundManager.SourcePoolIndex SourcePoolIndex
		{
			get
			{
				return this.sourcePoolIndex;
			}
		}

		// Token: 0x17001274 RID: 4724
		// (get) Token: 0x0600486A RID: 18538 RVA: 0x0027A227 File Offset: 0x00278427
		public SoundBuffers Buffers
		{
			get
			{
				if (this.Stream)
				{
					return null;
				}
				return this.buffers;
			}
		}

		// Token: 0x17001275 RID: 4725
		// (get) Token: 0x0600486B RID: 18539 RVA: 0x0027A239 File Offset: 0x00278439
		// (set) Token: 0x0600486C RID: 18540 RVA: 0x0027A241 File Offset: 0x00278441
		public int ALFormat { get; protected set; }

		// Token: 0x17001276 RID: 4726
		// (get) Token: 0x0600486D RID: 18541 RVA: 0x0027A24A File Offset: 0x0027844A
		// (set) Token: 0x0600486E RID: 18542 RVA: 0x0027A252 File Offset: 0x00278452
		public int SampleRate { get; protected set; }

		// Token: 0x0600486F RID: 18543 RVA: 0x0027A25C File Offset: 0x0027845C
		public Sound(SoundManager owner, string filename, bool stream, bool streamsReliably, ContentXElement xElement = null, bool getFullPath = true)
		{
			this.Owner = owner;
			this.Filename = (getFullPath ? Path.GetFullPath(filename.CleanUpPath()).CleanUpPath() : filename);
			this.Stream = stream;
			this.StreamsReliably = streamsReliably;
			this.XElement = xElement;
			ContentXElement xelement = this.XElement;
			SoundManager.SourcePoolIndex sourcePoolIndex;
			if (xelement == null)
			{
				sourcePoolIndex = SoundManager.SourcePoolIndex.Default;
			}
			else
			{
				string key = "sourcepool";
				SoundManager.SourcePoolIndex sourcePoolIndex2 = SoundManager.SourcePoolIndex.Default;
				sourcePoolIndex = xelement.GetAttributeEnum<SoundManager.SourcePoolIndex>(key, sourcePoolIndex2);
			}
			this.sourcePoolIndex = sourcePoolIndex;
			this.BaseGain = 1f;
			this.BaseNear = 100f;
			this.BaseFar = 200f;
		}

		// Token: 0x06004870 RID: 18544 RVA: 0x0027A2F3 File Offset: 0x002784F3
		public override string ToString()
		{
			return base.GetType().ToString() + " (" + this.Filename + ")";
		}

		// Token: 0x06004871 RID: 18545 RVA: 0x0027A315 File Offset: 0x00278515
		public virtual bool IsPlaying()
		{
			return this.Owner.IsPlaying(this);
		}

		// Token: 0x06004872 RID: 18546 RVA: 0x0027A324 File Offset: 0x00278524
		public bool LogWarningIfStillLoading()
		{
			if (this.Loading)
			{
				Level loaded = Level.Loaded;
				if (loaded == null || !loaded.Generating)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(56, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Attempted to play the sound ");
					defaultInterpolatedStringHandler.AppendFormatted<Sound>(this);
					defaultInterpolatedStringHandler.AppendLiteral(" while it was still loading.");
					DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				}
				return true;
			}
			return false;
		}

		// Token: 0x06004873 RID: 18547 RVA: 0x0027A384 File Offset: 0x00278584
		public virtual SoundChannel Play(float gain, float range, Vector2 position, bool muffle = false)
		{
			this.LogWarningIfStillLoading();
			if (this.Owner.CountPlayingInstances(this) >= this.MaxSimultaneousInstances)
			{
				return null;
			}
			return new SoundChannel(this, gain, new Vector3?(new Vector3(position.X, position.Y, 0f)), 1f, range * 0.4f, range, SoundManager.SoundCategoryDefault, muffle);
		}

		// Token: 0x06004874 RID: 18548 RVA: 0x0027A3E4 File Offset: 0x002785E4
		public virtual SoundChannel Play(float gain, float range, float freqMult, Vector2 position, bool muffle = false)
		{
			this.LogWarningIfStillLoading();
			if (this.Owner.CountPlayingInstances(this) >= this.MaxSimultaneousInstances)
			{
				return null;
			}
			return new SoundChannel(this, gain, new Vector3?(new Vector3(position.X, position.Y, 0f)), freqMult, range * 0.4f, range, SoundManager.SoundCategoryDefault, muffle);
		}

		// Token: 0x06004875 RID: 18549 RVA: 0x0027A442 File Offset: 0x00278642
		public virtual SoundChannel Play(Vector3? position, float gain, float freqMult = 1f, bool muffle = false)
		{
			this.LogWarningIfStillLoading();
			if (this.Owner.CountPlayingInstances(this) >= this.MaxSimultaneousInstances)
			{
				return null;
			}
			return new SoundChannel(this, gain, position, freqMult, this.BaseNear, this.BaseFar, SoundManager.SoundCategoryDefault, muffle);
		}

		// Token: 0x06004876 RID: 18550 RVA: 0x0027A480 File Offset: 0x00278680
		public virtual SoundChannel Play(float gain)
		{
			return this.Play(null, gain, 1f, false);
		}

		// Token: 0x06004877 RID: 18551 RVA: 0x0027A4A3 File Offset: 0x002786A3
		public virtual SoundChannel Play()
		{
			return this.Play(this.BaseGain);
		}

		// Token: 0x06004878 RID: 18552 RVA: 0x0027A4B4 File Offset: 0x002786B4
		public virtual SoundChannel Play(float? gain, Identifier category)
		{
			if (this.Owner.CountPlayingInstances(this) >= this.MaxSimultaneousInstances)
			{
				return null;
			}
			return new SoundChannel(this, gain ?? this.BaseGain, null, 1f, this.BaseNear, this.BaseFar, category, false);
		}

		// Token: 0x06004879 RID: 18553 RVA: 0x0027A514 File Offset: 0x00278714
		protected static void CastBuffer(float[] inBuffer, short[] outBuffer, int length)
		{
			for (int i = 0; i < length; i++)
			{
				outBuffer[i] = ToolBox.FloatToShortAudioSample(inBuffer[i]);
			}
		}

		// Token: 0x0600487A RID: 18554
		public abstract int FillStreamBuffer(int samplePos, short[] buffer);

		// Token: 0x0600487B RID: 18555
		public abstract float GetAmplitudeAtPlaybackPos(int playbackPos);

		// Token: 0x0600487C RID: 18556 RVA: 0x0027A538 File Offset: 0x00278738
		public virtual void InitializeAlBuffers()
		{
		}

		// Token: 0x0600487D RID: 18557 RVA: 0x0027A53A File Offset: 0x0027873A
		public virtual void FillAlBuffers()
		{
		}

		// Token: 0x0600487E RID: 18558 RVA: 0x0027A53C File Offset: 0x0027873C
		public virtual void DeleteAlBuffers()
		{
			this.Owner.KillChannels(this);
			SoundBuffers soundBuffers = this.buffers;
			if (soundBuffers == null)
			{
				return;
			}
			soundBuffers.Dispose();
		}

		// Token: 0x0600487F RID: 18559 RVA: 0x0027A55A File Offset: 0x0027875A
		public virtual void Dispose()
		{
			if (this.disposed)
			{
				return;
			}
			this.DeleteAlBuffers();
			this.Owner.RemoveSound(this);
			this.disposed = true;
		}

		// Token: 0x04002580 RID: 9600
		protected bool disposed;

		// Token: 0x04002581 RID: 9601
		public readonly SoundManager Owner;

		// Token: 0x04002582 RID: 9602
		public readonly string Filename;

		// Token: 0x04002583 RID: 9603
		public readonly ContentXElement XElement;

		// Token: 0x04002584 RID: 9604
		public readonly bool Stream;

		// Token: 0x04002585 RID: 9605
		public readonly bool StreamsReliably;

		// Token: 0x04002587 RID: 9607
		private readonly SoundManager.SourcePoolIndex sourcePoolIndex;

		// Token: 0x04002588 RID: 9608
		protected SoundBuffers buffers;

		// Token: 0x0400258B RID: 9611
		public int MaxSimultaneousInstances = 5;

		// Token: 0x0400258C RID: 9612
		public float BaseGain;

		// Token: 0x0400258D RID: 9613
		public float BaseNear;

		// Token: 0x0400258E RID: 9614
		public float BaseFar;

		// Token: 0x0400258F RID: 9615
		public bool MuteBackgroundMusic;
	}
}
