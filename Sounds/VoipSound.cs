using System;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;
using Concentus.Structs;
using Microsoft.Xna.Framework;

namespace Barotrauma.Sounds
{
	// Token: 0x0200044A RID: 1098
	internal class VoipSound : Sound
	{
		// Token: 0x1700129A RID: 4762
		// (get) Token: 0x06004904 RID: 18692 RVA: 0x0027EBF8 File Offset: 0x0027CDF8
		public override double? DurationSeconds
		{
			get
			{
				return null;
			}
		}

		// Token: 0x1700129B RID: 4763
		// (get) Token: 0x06004905 RID: 18693 RVA: 0x0027EC0E File Offset: 0x0027CE0E
		public override SoundManager.SourcePoolIndex SourcePoolIndex
		{
			get
			{
				return SoundManager.SourcePoolIndex.Voice;
			}
		}

		// Token: 0x1700129C RID: 4764
		// (get) Token: 0x06004906 RID: 18694 RVA: 0x0027EC11 File Offset: 0x0027CE11
		public new bool IsPlaying
		{
			get
			{
				return this.soundChannel != null && this.soundChannel.IsPlaying;
			}
		}

		// Token: 0x1700129D RID: 4765
		// (get) Token: 0x06004907 RID: 18695 RVA: 0x0027EC28 File Offset: 0x0027CE28
		// (set) Token: 0x06004908 RID: 18696 RVA: 0x0027EC30 File Offset: 0x0027CE30
		public float Near { get; private set; }

		// Token: 0x1700129E RID: 4766
		// (get) Token: 0x06004909 RID: 18697 RVA: 0x0027EC39 File Offset: 0x0027CE39
		// (set) Token: 0x0600490A RID: 18698 RVA: 0x0027EC41 File Offset: 0x0027CE41
		public float Far { get; private set; }

		// Token: 0x1700129F RID: 4767
		// (get) Token: 0x0600490B RID: 18699 RVA: 0x0027EC4A File Offset: 0x0027CE4A
		// (set) Token: 0x0600490C RID: 18700 RVA: 0x0027EC60 File Offset: 0x0027CE60
		public float Gain
		{
			get
			{
				if (this.soundChannel != null)
				{
					return this.gain;
				}
				return 0f;
			}
			set
			{
				if (this.soundChannel == null)
				{
					return;
				}
				this.gain = value;
				this.soundChannel.Gain = value * GameSettings.CurrentConfig.Audio.VoiceChatVolume * this.client.VoiceVolume;
			}
		}

		// Token: 0x170012A0 RID: 4768
		// (get) Token: 0x0600490D RID: 18701 RVA: 0x0027EC9A File Offset: 0x0027CE9A
		public float CurrentAmplitude
		{
			get
			{
				SoundChannel soundChannel = this.soundChannel;
				if (soundChannel == null)
				{
					return 0f;
				}
				return soundChannel.CurrentAmplitude;
			}
		}

		// Token: 0x0600490E RID: 18702 RVA: 0x0027ECB4 File Offset: 0x0027CEB4
		public VoipSound(Client targetClient, SoundManager owner, VoipQueue q) : base(owner, "VoIP (" + targetClient.Name + ")", true, true, null, false)
		{
			this.client = targetClient;
			this.decoder = VoipConfig.CreateDecoder();
			base.ALFormat = 4353;
			base.SampleRate = 48000;
			this.queue = q;
			this.bufferID = (int)this.queue.LatestBufferID;
			this.soundChannel = null;
			SoundChannel chn = new SoundChannel(this, 1f, null, 1f, 0.4f, 1f, SoundManager.SoundCategoryVoip, false);
			this.soundChannel = chn;
			this.Gain = 1f;
		}

		// Token: 0x0600490F RID: 18703 RVA: 0x0027EDA8 File Offset: 0x0027CFA8
		public override float GetAmplitudeAtPlaybackPos(int playbackPos)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06004910 RID: 18704 RVA: 0x0027EDAF File Offset: 0x0027CFAF
		public void SetPosition(Vector3? pos)
		{
			this.soundChannel.Position = pos;
		}

		// Token: 0x06004911 RID: 18705 RVA: 0x0027EDC0 File Offset: 0x0027CFC0
		public void SetRange(float near, float far)
		{
			SoundChannel soundChannel = this.soundChannel;
			this.Near = near;
			soundChannel.Near = near;
			SoundChannel soundChannel2 = this.soundChannel;
			this.Far = far;
			soundChannel2.Far = far;
		}

		// Token: 0x06004912 RID: 18706 RVA: 0x0027EDF8 File Offset: 0x0027CFF8
		public void ApplyFilters(short[] buffer, int readSamples)
		{
			float finalGain = this.gain * GameSettings.CurrentConfig.Audio.VoiceChatVolume * this.client.VoiceVolume;
			for (int i = 0; i < readSamples; i++)
			{
				float fVal = ToolBox.ShortAudioSampleToFloat(buffer[i]);
				if (finalGain > 1f)
				{
					fVal = Math.Clamp(fVal * finalGain, -1f, 1f);
				}
				if (this.UseMuffleFilter)
				{
					foreach (BiQuad filter in this.muffleFilters)
					{
						fVal = filter.Process(fVal);
					}
				}
				if (this.UseRadioFilter)
				{
					foreach (BiQuad filter2 in this.radioFilters)
					{
						fVal = Math.Clamp(filter2.Process(fVal) * 1.2f, -1f, 1f);
					}
				}
				buffer[i] = ToolBox.FloatToShortAudioSample(fVal);
			}
		}

		// Token: 0x06004913 RID: 18707 RVA: 0x0027EEDF File Offset: 0x0027D0DF
		public override SoundChannel Play(float gain, float range, Vector2 position, bool muffle = false)
		{
			throw new InvalidOperationException();
		}

		// Token: 0x06004914 RID: 18708 RVA: 0x0027EEE6 File Offset: 0x0027D0E6
		public override SoundChannel Play(Vector3? position, float gain, float freqMult = 1f, bool muffle = false)
		{
			throw new InvalidOperationException();
		}

		// Token: 0x06004915 RID: 18709 RVA: 0x0027EEED File Offset: 0x0027D0ED
		public override SoundChannel Play(float gain)
		{
			throw new InvalidOperationException();
		}

		// Token: 0x06004916 RID: 18710 RVA: 0x0027EEF4 File Offset: 0x0027D0F4
		public override SoundChannel Play()
		{
			throw new InvalidOperationException();
		}

		// Token: 0x06004917 RID: 18711 RVA: 0x0027EEFC File Offset: 0x0027D0FC
		public override int FillStreamBuffer(int samplePos, short[] buffer)
		{
			int compressedSize;
			byte[] compressedBuffer;
			this.queue.RetrieveBuffer(this.bufferID, out compressedSize, out compressedBuffer);
			try
			{
				if (compressedSize > 0)
				{
					this.decoder.Decode(compressedBuffer, 0, compressedSize, buffer, 0, 960, false);
					this.bufferID++;
					return 960;
				}
				if (this.bufferID < (int)(this.queue.LatestBufferID - 7))
				{
					this.bufferID = (int)(this.queue.LatestBufferID - 7);
				}
			}
			catch (Exception e)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(57, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to decode Opus buffer (buffer size ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(compressedBuffer.Length);
				defaultInterpolatedStringHandler.AppendLiteral(", packet size ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(compressedSize);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), e, null, false, false);
				this.bufferID = (int)(this.queue.LatestBufferID - 7);
			}
			return 0;
		}

		// Token: 0x06004918 RID: 18712 RVA: 0x0027EFF8 File Offset: 0x0027D1F8
		public override void Dispose()
		{
			if (this.soundChannel != null)
			{
				this.soundChannel.Dispose();
				this.soundChannel = null;
			}
			base.Dispose();
		}

		// Token: 0x040025DE RID: 9694
		private readonly VoipQueue queue;

		// Token: 0x040025DF RID: 9695
		private int bufferID;

		// Token: 0x040025E0 RID: 9696
		private SoundChannel soundChannel;

		// Token: 0x040025E1 RID: 9697
		private readonly OpusDecoder decoder;

		// Token: 0x040025E2 RID: 9698
		public bool UseRadioFilter;

		// Token: 0x040025E3 RID: 9699
		public bool UseMuffleFilter;

		// Token: 0x040025E4 RID: 9700
		public bool UsingRadio;

		// Token: 0x040025E7 RID: 9703
		private readonly BiQuad[] muffleFilters = new BiQuad[]
		{
			new LowpassFilter(48000, 800.0)
		};

		// Token: 0x040025E8 RID: 9704
		private readonly BiQuad[] radioFilters = new BiQuad[]
		{
			new BandpassFilter(48000, 2000.0)
		};

		// Token: 0x040025E9 RID: 9705
		private const float PostRadioFilterBoost = 1.2f;

		// Token: 0x040025EA RID: 9706
		private float gain;

		// Token: 0x040025EB RID: 9707
		private Client client;
	}
}
