using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using OpenAL;

namespace Barotrauma.Sounds
{
	// Token: 0x0200043D RID: 1085
	internal sealed class SoundBuffers : IDisposable
	{
		// Token: 0x17001277 RID: 4727
		// (get) Token: 0x06004880 RID: 18560 RVA: 0x0027A57E File Offset: 0x0027877E
		// (set) Token: 0x06004881 RID: 18561 RVA: 0x0027A585 File Offset: 0x00278785
		public static int BuffersGenerated { get; private set; } = 0;

		// Token: 0x17001278 RID: 4728
		// (get) Token: 0x06004882 RID: 18562 RVA: 0x0027A58D File Offset: 0x0027878D
		// (set) Token: 0x06004883 RID: 18563 RVA: 0x0027A595 File Offset: 0x00278795
		public uint AlBuffer { get; private set; }

		// Token: 0x17001279 RID: 4729
		// (get) Token: 0x06004884 RID: 18564 RVA: 0x0027A59E File Offset: 0x0027879E
		// (set) Token: 0x06004885 RID: 18565 RVA: 0x0027A5A6 File Offset: 0x002787A6
		public uint AlMuffledBuffer { get; private set; }

		// Token: 0x06004886 RID: 18566 RVA: 0x0027A5AF File Offset: 0x002787AF
		public SoundBuffers(Sound sound)
		{
			this.sound = sound;
		}

		// Token: 0x06004887 RID: 18567 RVA: 0x0027A5C0 File Offset: 0x002787C0
		public void Dispose()
		{
			if (this.AlBuffer != 0U)
			{
				HashSet<uint> obj = SoundBuffers.bufferPool;
				lock (obj)
				{
					SoundBuffers.bufferPool.Add(this.AlBuffer);
				}
			}
			if (this.AlMuffledBuffer != 0U)
			{
				HashSet<uint> obj2 = SoundBuffers.bufferPool;
				lock (obj2)
				{
					SoundBuffers.bufferPool.Add(this.AlMuffledBuffer);
				}
			}
			this.AlBuffer = 0U;
			this.AlMuffledBuffer = 0U;
		}

		// Token: 0x06004888 RID: 18568 RVA: 0x0027A664 File Offset: 0x00278864
		public static void ClearPool()
		{
			HashSet<uint> obj = SoundBuffers.bufferPool;
			lock (obj)
			{
				SoundBuffers.bufferPool.ForEach(delegate(uint b)
				{
					Al.DeleteBuffer(b);
				});
				SoundBuffers.bufferPool.Clear();
			}
			SoundBuffers.BuffersGenerated = 0;
		}

		// Token: 0x06004889 RID: 18569 RVA: 0x0027A6D8 File Offset: 0x002788D8
		public bool RequestAlBuffers()
		{
			if (this.AlBuffer != 0U)
			{
				return false;
			}
			HashSet<uint> obj = SoundBuffers.bufferPool;
			lock (obj)
			{
				while (SoundBuffers.bufferPool.Count < 2 && SoundBuffers.BuffersGenerated < 32000)
				{
					uint newBuffer;
					Al.GenBuffer(out newBuffer);
					int alError = Al.GetError();
					if (alError != 0)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(105, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Error when generating sound buffer: ");
						defaultInterpolatedStringHandler.AppendFormatted(Al.GetErrorString(alError));
						defaultInterpolatedStringHandler.AppendLiteral(". ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(SoundBuffers.BuffersGenerated);
						defaultInterpolatedStringHandler.AppendLiteral(" buffer(s) were generated. No more sound buffers will be generated.");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
						SoundBuffers.BuffersGenerated = 32000;
					}
					else if (!Al.IsBuffer(newBuffer))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(133, 1);
						defaultInterpolatedStringHandler2.AppendLiteral("Error when generating sound buffer: result is not a valid buffer. ");
						defaultInterpolatedStringHandler2.AppendFormatted<int>(SoundBuffers.BuffersGenerated);
						defaultInterpolatedStringHandler2.AppendLiteral(" buffer(s) were generated. No more sound buffers will be generated.");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler2.ToStringAndClear(), null);
						SoundBuffers.BuffersGenerated = 32000;
					}
					else
					{
						SoundBuffers.bufferPool.Add(newBuffer);
						SoundBuffers.BuffersGenerated++;
						if (SoundBuffers.BuffersGenerated >= 32000)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(67, 1);
							defaultInterpolatedStringHandler3.AppendFormatted<int>(SoundBuffers.BuffersGenerated);
							defaultInterpolatedStringHandler3.AppendLiteral(" buffer(s) were generated. No more sound buffers will be generated.");
							DebugConsole.AddWarning(defaultInterpolatedStringHandler3.ToStringAndClear(), null);
						}
					}
				}
				if (SoundBuffers.bufferPool.Count >= 2)
				{
					this.AlBuffer = SoundBuffers.bufferPool.First<uint>();
					SoundBuffers.bufferPool.Remove(this.AlBuffer);
					this.AlMuffledBuffer = SoundBuffers.bufferPool.First<uint>();
					SoundBuffers.bufferPool.Remove(this.AlMuffledBuffer);
					return true;
				}
			}
			foreach (Sound otherSound in this.sound.Owner.LoadedSounds)
			{
				if (otherSound != this.sound && !otherSound.IsPlaying() && otherSound.Buffers != null && otherSound.Buffers.AlBuffer != 0U)
				{
					otherSound.Owner.KillChannels(otherSound);
					this.AlBuffer = otherSound.Buffers.AlBuffer;
					this.AlMuffledBuffer = otherSound.Buffers.AlMuffledBuffer;
					otherSound.Buffers.AlBuffer = 0U;
					otherSound.Buffers.AlMuffledBuffer = 0U;
					this.sound.Owner.MoveSoundToPosition(this.sound, this.sound.Owner.LoadedSoundCount - 1);
					if (!Al.IsBuffer(this.AlBuffer))
					{
						throw new Exception(this.sound.Filename + " has an invalid buffer!");
					}
					if (!Al.IsBuffer(this.AlMuffledBuffer))
					{
						throw new Exception(this.sound.Filename + " has an invalid muffled buffer!");
					}
					return true;
				}
			}
			return false;
		}

		// Token: 0x04002590 RID: 9616
		private static readonly HashSet<uint> bufferPool = new HashSet<uint>();

		// Token: 0x04002591 RID: 9617
		public const int MaxBuffers = 32000;

		// Token: 0x04002593 RID: 9619
		private readonly Sound sound;
	}
}
