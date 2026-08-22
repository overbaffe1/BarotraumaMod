using System;
using System.Collections.Generic;
using Barotrauma.Media;
using Microsoft.Xna.Framework;

namespace Barotrauma.Sounds
{
	// Token: 0x02000449 RID: 1097
	internal class VideoSound : Sound
	{
		// Token: 0x17001299 RID: 4761
		// (get) Token: 0x060048F9 RID: 18681 RVA: 0x0027E91C File Offset: 0x0027CB1C
		public override double? DurationSeconds
		{
			get
			{
				return null;
			}
		}

		// Token: 0x060048FA RID: 18682 RVA: 0x0027E934 File Offset: 0x0027CB34
		public VideoSound(SoundManager owner, string filename, int sampleRate, int channelCount, Video vid) : base(owner, filename, true, false, null, true)
		{
			base.ALFormat = ((channelCount == 2) ? 4355 : 4353);
			base.SampleRate = sampleRate;
			this.sampleQueue = new Queue<short[]>();
			this.mutex = new object();
			this.soundChannel = null;
			this.video = vid;
		}

		// Token: 0x060048FB RID: 18683 RVA: 0x0027E990 File Offset: 0x0027CB90
		public override float GetAmplitudeAtPlaybackPos(int playbackPos)
		{
			throw new NotImplementedException();
		}

		// Token: 0x060048FC RID: 18684 RVA: 0x0027E998 File Offset: 0x0027CB98
		public override bool IsPlaying()
		{
			bool retVal = false;
			object obj = this.mutex;
			lock (obj)
			{
				retVal = (this.soundChannel != null && this.soundChannel.IsPlaying);
			}
			return retVal;
		}

		// Token: 0x060048FD RID: 18685 RVA: 0x0027E9EC File Offset: 0x0027CBEC
		public void Enqueue(short[] buf)
		{
			object obj = this.mutex;
			lock (obj)
			{
				this.sampleQueue.Enqueue(buf);
			}
		}

		// Token: 0x060048FE RID: 18686 RVA: 0x0027EA34 File Offset: 0x0027CC34
		public override SoundChannel Play(float gain, float range, Vector2 position, bool muffle = false)
		{
			throw new InvalidOperationException();
		}

		// Token: 0x060048FF RID: 18687 RVA: 0x0027EA3B File Offset: 0x0027CC3B
		public override SoundChannel Play(Vector3? position, float gain, float freqMult = 1f, bool muffle = false)
		{
			throw new InvalidOperationException();
		}

		// Token: 0x06004900 RID: 18688 RVA: 0x0027EA44 File Offset: 0x0027CC44
		public override SoundChannel Play(float gain)
		{
			SoundChannel chn = null;
			object obj = this.mutex;
			lock (obj)
			{
				if (this.soundChannel != null)
				{
					this.soundChannel.Dispose();
					this.soundChannel = null;
				}
			}
			chn = new SoundChannel(this, gain, null, 1f, 1f, 3f, "video".ToIdentifier(), false);
			object obj2 = this.mutex;
			lock (obj2)
			{
				this.soundChannel = chn;
			}
			return chn;
		}

		// Token: 0x06004901 RID: 18689 RVA: 0x0027EAFC File Offset: 0x0027CCFC
		public override SoundChannel Play()
		{
			return this.Play(this.BaseGain);
		}

		// Token: 0x06004902 RID: 18690 RVA: 0x0027EB0C File Offset: 0x0027CD0C
		public override int FillStreamBuffer(int samplePos, short[] buffer)
		{
			if (!this.video.IsPlaying)
			{
				return -1;
			}
			int readAmount = 0;
			object obj = this.mutex;
			lock (obj)
			{
				while (readAmount < buffer.Length)
				{
					if (this.sampleQueue.Count == 0)
					{
						break;
					}
					short[] buf = this.sampleQueue.Peek();
					if (readAmount + buf.Length >= buffer.Length)
					{
						break;
					}
					buf = this.sampleQueue.Dequeue();
					buf.CopyTo(buffer, readAmount);
					readAmount += buf.Length;
				}
			}
			return readAmount;
		}

		// Token: 0x06004903 RID: 18691 RVA: 0x0027EBA4 File Offset: 0x0027CDA4
		public override void Dispose()
		{
			object obj = this.mutex;
			lock (obj)
			{
				SoundChannel soundChannel = this.soundChannel;
				if (soundChannel != null)
				{
					soundChannel.Dispose();
				}
				base.Dispose();
			}
		}

		// Token: 0x040025DA RID: 9690
		private readonly object mutex;

		// Token: 0x040025DB RID: 9691
		private readonly Queue<short[]> sampleQueue;

		// Token: 0x040025DC RID: 9692
		private SoundChannel soundChannel;

		// Token: 0x040025DD RID: 9693
		private readonly Video video;
	}
}
