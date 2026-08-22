using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NVorbis;
using OpenAL;

namespace Barotrauma.Sounds
{
	// Token: 0x0200043B RID: 1083
	internal sealed class OggSound : Sound
	{
		// Token: 0x1700126E RID: 4718
		// (get) Token: 0x0600485B RID: 18523 RVA: 0x00279E6B File Offset: 0x0027806B
		public long MaxStreamSamplePos
		{
			get
			{
				if (this.streamReader != null)
				{
					return this.streamReader.TotalSamples * (long)this.streamReader.Channels * 2L;
				}
				return 0L;
			}
		}

		// Token: 0x1700126F RID: 4719
		// (get) Token: 0x0600485C RID: 18524 RVA: 0x00279E93 File Offset: 0x00278093
		public override double? DurationSeconds
		{
			get
			{
				return new double?(this.durationSeconds);
			}
		}

		// Token: 0x0600485D RID: 18525 RVA: 0x00279EA0 File Offset: 0x002780A0
		public OggSound(SoundManager owner, string filename, bool stream, ContentXElement xElement) : base(owner, filename, stream, true, xElement, true)
		{
			OggSound <>4__this = this;
			VorbisReader reader = new VorbisReader(this.Filename);
			this.durationSeconds = reader.TotalTime.TotalSeconds;
			base.ALFormat = ((reader.Channels == 1) ? 4353 : 4355);
			base.SampleRate = reader.SampleRate;
			if (stream)
			{
				this.streamReader = reader;
				return;
			}
			base.Loading = true;
			TaskPool.Add("LoadSamples " + filename, OggSound.LoadSamples(reader), delegate(Task t)
			{
				reader.Dispose();
				OggSound.TaskResult result;
				if (!t.TryGetResult(out result))
				{
					return;
				}
				<>4__this.sampleBuffer = result.SampleBuffer;
				<>4__this.muffleBuffer = result.MuffleBuffer;
				<>4__this.playbackAmplitude = result.PlaybackAmplitude;
				<>4__this.Owner.KillChannels(<>4__this);
				SoundBuffers buffers = <>4__this.buffers;
				if (buffers != null)
				{
					buffers.Dispose();
				}
				<>4__this.buffers = null;
				<>4__this.Loading = false;
			});
		}

		// Token: 0x0600485E RID: 18526 RVA: 0x00279F78 File Offset: 0x00278178
		private static Task<OggSound.TaskResult> LoadSamples(VorbisReader reader)
		{
			OggSound.<LoadSamples>d__12 <LoadSamples>d__;
			<LoadSamples>d__.<>t__builder = AsyncTaskMethodBuilder<OggSound.TaskResult>.Create();
			<LoadSamples>d__.reader = reader;
			<LoadSamples>d__.<>1__state = -1;
			<LoadSamples>d__.<>t__builder.Start<OggSound.<LoadSamples>d__12>(ref <LoadSamples>d__);
			return <LoadSamples>d__.<>t__builder.Task;
		}

		// Token: 0x0600485F RID: 18527 RVA: 0x00279FBC File Offset: 0x002781BC
		public override float GetAmplitudeAtPlaybackPos(int playbackPos)
		{
			if (this.playbackAmplitude == null || this.playbackAmplitude.Count == 0)
			{
				return 0f;
			}
			int index = playbackPos / 4410;
			if (index < 0)
			{
				return 0f;
			}
			if (index >= this.playbackAmplitude.Count)
			{
				index = this.playbackAmplitude.Count - 1;
			}
			return this.playbackAmplitude[index];
		}

		// Token: 0x06004860 RID: 18528 RVA: 0x0027A020 File Offset: 0x00278220
		public override int FillStreamBuffer(int samplePos, short[] buffer)
		{
			if (!this.Stream)
			{
				throw new Exception("Called FillStreamBuffer on a non-streamed sound!");
			}
			if (this.streamReader == null)
			{
				throw new Exception("Called FillStreamBuffer when the reader is null!");
			}
			if ((long)samplePos >= this.MaxStreamSamplePos)
			{
				return 0;
			}
			samplePos /= this.streamReader.Channels * 2;
			this.streamReader.DecodedPosition = (long)samplePos;
			if (this.streamFloatBuffer == null || this.streamFloatBuffer.Length < buffer.Length)
			{
				this.streamFloatBuffer = new float[buffer.Length];
			}
			int readSamples = this.streamReader.ReadSamples(this.streamFloatBuffer, 0, buffer.Length);
			Sound.CastBuffer(this.streamFloatBuffer, buffer, readSamples);
			return readSamples;
		}

		// Token: 0x06004861 RID: 18529 RVA: 0x0027A0C4 File Offset: 0x002782C4
		private static void MuffleBuffer(float[] buffer, int sampleRate)
		{
			LowpassFilter filter = new LowpassFilter(sampleRate, 600.0);
			filter.Process(buffer);
		}

		// Token: 0x06004862 RID: 18530 RVA: 0x0027A0E8 File Offset: 0x002782E8
		public override void InitializeAlBuffers()
		{
			if (this.buffers != null && SoundBuffers.BuffersGenerated < 32000)
			{
				this.FillAlBuffers();
			}
		}

		// Token: 0x06004863 RID: 18531 RVA: 0x0027A104 File Offset: 0x00278304
		public override void FillAlBuffers()
		{
			if (this.Stream)
			{
				return;
			}
			if (this.sampleBuffer.Length == 0 || this.muffleBuffer.Length == 0)
			{
				return;
			}
			if (this.buffers == null)
			{
				this.buffers = new SoundBuffers(this);
			}
			if (!this.buffers.RequestAlBuffers())
			{
				return;
			}
			Al.BufferData<short>(this.buffers.AlBuffer, base.ALFormat, this.sampleBuffer, this.sampleBuffer.Length * 2, base.SampleRate);
			int alError = Al.GetError();
			if (alError != 0)
			{
				throw new Exception("Failed to set regular buffer data for non-streamed audio! " + Al.GetErrorString(alError));
			}
			Al.BufferData<short>(this.buffers.AlMuffledBuffer, base.ALFormat, this.muffleBuffer, this.muffleBuffer.Length * 2, base.SampleRate);
			alError = Al.GetError();
			if (alError != 0)
			{
				throw new Exception("Failed to set muffled buffer data for non-streamed audio! " + Al.GetErrorString(alError));
			}
		}

		// Token: 0x06004864 RID: 18532 RVA: 0x0027A1E5 File Offset: 0x002783E5
		public override void Dispose()
		{
			if (this.Stream)
			{
				VorbisReader vorbisReader = this.streamReader;
				if (vorbisReader != null)
				{
					vorbisReader.Dispose();
				}
			}
			base.Dispose();
		}

		// Token: 0x04002579 RID: 9593
		private readonly VorbisReader streamReader;

		// Token: 0x0400257A RID: 9594
		private List<float> playbackAmplitude;

		// Token: 0x0400257B RID: 9595
		private const int AMPLITUDE_SAMPLE_COUNT = 4410;

		// Token: 0x0400257C RID: 9596
		private short[] sampleBuffer = Array.Empty<short>();

		// Token: 0x0400257D RID: 9597
		private short[] muffleBuffer = Array.Empty<short>();

		// Token: 0x0400257E RID: 9598
		private readonly double durationSeconds;

		// Token: 0x0400257F RID: 9599
		private float[] streamFloatBuffer;

		// Token: 0x02001146 RID: 4422
		private readonly struct TaskResult : IEquatable<OggSound.TaskResult>
		{
			// Token: 0x06008FA3 RID: 36771 RVA: 0x003BA5D9 File Offset: 0x003B87D9
			public TaskResult(short[] SampleBuffer, short[] MuffleBuffer, List<float> PlaybackAmplitude)
			{
				this.SampleBuffer = SampleBuffer;
				this.MuffleBuffer = MuffleBuffer;
				this.PlaybackAmplitude = PlaybackAmplitude;
			}

			// Token: 0x17001C9A RID: 7322
			// (get) Token: 0x06008FA4 RID: 36772 RVA: 0x003BA5F0 File Offset: 0x003B87F0
			// (set) Token: 0x06008FA5 RID: 36773 RVA: 0x003BA5F8 File Offset: 0x003B87F8
			public short[] SampleBuffer { get; set; }

			// Token: 0x17001C9B RID: 7323
			// (get) Token: 0x06008FA6 RID: 36774 RVA: 0x003BA601 File Offset: 0x003B8801
			// (set) Token: 0x06008FA7 RID: 36775 RVA: 0x003BA609 File Offset: 0x003B8809
			public short[] MuffleBuffer { get; set; }

			// Token: 0x17001C9C RID: 7324
			// (get) Token: 0x06008FA8 RID: 36776 RVA: 0x003BA612 File Offset: 0x003B8812
			// (set) Token: 0x06008FA9 RID: 36777 RVA: 0x003BA61A File Offset: 0x003B881A
			public List<float> PlaybackAmplitude { get; set; }

			// Token: 0x06008FAA RID: 36778 RVA: 0x003BA624 File Offset: 0x003B8824
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("TaskResult");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06008FAB RID: 36779 RVA: 0x003BA670 File Offset: 0x003B8870
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("SampleBuffer = ");
				builder.Append(this.SampleBuffer);
				builder.Append(", MuffleBuffer = ");
				builder.Append(this.MuffleBuffer);
				builder.Append(", PlaybackAmplitude = ");
				builder.Append(this.PlaybackAmplitude);
				return true;
			}

			// Token: 0x06008FAC RID: 36780 RVA: 0x003BA6C9 File Offset: 0x003B88C9
			[CompilerGenerated]
			public static bool operator !=(OggSound.TaskResult left, OggSound.TaskResult right)
			{
				return !(left == right);
			}

			// Token: 0x06008FAD RID: 36781 RVA: 0x003BA6D5 File Offset: 0x003B88D5
			[CompilerGenerated]
			public static bool operator ==(OggSound.TaskResult left, OggSound.TaskResult right)
			{
				return left.Equals(right);
			}

			// Token: 0x06008FAE RID: 36782 RVA: 0x003BA6DF File Offset: 0x003B88DF
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (EqualityComparer<short[]>.Default.GetHashCode(this.<SampleBuffer>k__BackingField) * -1521134295 + EqualityComparer<short[]>.Default.GetHashCode(this.<MuffleBuffer>k__BackingField)) * -1521134295 + EqualityComparer<List<float>>.Default.GetHashCode(this.<PlaybackAmplitude>k__BackingField);
			}

			// Token: 0x06008FAF RID: 36783 RVA: 0x003BA71F File Offset: 0x003B891F
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is OggSound.TaskResult && this.Equals((OggSound.TaskResult)obj);
			}

			// Token: 0x06008FB0 RID: 36784 RVA: 0x003BA738 File Offset: 0x003B8938
			[CompilerGenerated]
			public bool Equals(OggSound.TaskResult other)
			{
				return EqualityComparer<short[]>.Default.Equals(this.<SampleBuffer>k__BackingField, other.<SampleBuffer>k__BackingField) && EqualityComparer<short[]>.Default.Equals(this.<MuffleBuffer>k__BackingField, other.<MuffleBuffer>k__BackingField) && EqualityComparer<List<float>>.Default.Equals(this.<PlaybackAmplitude>k__BackingField, other.<PlaybackAmplitude>k__BackingField);
			}

			// Token: 0x06008FB1 RID: 36785 RVA: 0x003BA78D File Offset: 0x003B898D
			[CompilerGenerated]
			public void Deconstruct(out short[] SampleBuffer, out short[] MuffleBuffer, out List<float> PlaybackAmplitude)
			{
				SampleBuffer = this.SampleBuffer;
				MuffleBuffer = this.MuffleBuffer;
				PlaybackAmplitude = this.PlaybackAmplitude;
			}
		}
	}
}
