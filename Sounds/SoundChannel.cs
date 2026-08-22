using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Xna.Framework;
using OpenAL;

namespace Barotrauma.Sounds
{
	// Token: 0x0200043F RID: 1087
	internal class SoundChannel : IDisposable
	{
		// Token: 0x1700127B RID: 4731
		// (get) Token: 0x0600488F RID: 18575 RVA: 0x0027AC14 File Offset: 0x00278E14
		// (set) Token: 0x06004890 RID: 18576 RVA: 0x0027AC1C File Offset: 0x00278E1C
		public Vector3? Position
		{
			get
			{
				return this.position;
			}
			set
			{
				this.position = value;
				if (this.ALSourceIndex < 0)
				{
					return;
				}
				if (this.position != null)
				{
					if (float.IsNaN(this.position.Value.X))
					{
						DebugConsole.ThrowError("Failed to set source's position: " + this.debugName + ", position.X is NaN", null, null, false, true);
						return;
					}
					if (float.IsNaN(this.position.Value.Y))
					{
						DebugConsole.ThrowError("Failed to set source's position: " + this.debugName + ", position.Y is NaN", null, null, false, true);
						return;
					}
					if (float.IsNaN(this.position.Value.Z))
					{
						DebugConsole.ThrowError("Failed to set source's position: " + this.debugName + ", position.Z is NaN", null, null, false, true);
						return;
					}
					if (float.IsInfinity(this.position.Value.X))
					{
						DebugConsole.ThrowError("Failed to set source's position: " + this.debugName + ", position.X is Infinity", null, null, false, true);
						return;
					}
					if (float.IsInfinity(this.position.Value.Y))
					{
						DebugConsole.ThrowError("Failed to set source's position: " + this.debugName + ", position.Y is Infinity", null, null, false, true);
						return;
					}
					if (float.IsInfinity(this.position.Value.Z))
					{
						DebugConsole.ThrowError("Failed to set source's position: " + this.debugName + ", position.Z is Infinity", null, null, false, true);
						return;
					}
					uint alSource = this.Sound.Owner.GetSourceFromIndex(this.Sound.SourcePoolIndex, this.ALSourceIndex);
					Al.Sourcei(alSource, 514, 0);
					int alError = Al.GetError();
					if (alError != 0)
					{
						DebugConsole.ThrowError("Failed to enable source's relative flag: " + this.debugName + ", " + Al.GetErrorString(alError), null, null, false, true);
						return;
					}
					Al.Source3f(alSource, 4100, this.position.Value.X, this.position.Value.Y, this.position.Value.Z);
					alError = Al.GetError();
					if (alError != 0)
					{
						DebugConsole.ThrowError("Failed to set source's position: " + this.debugName + ", " + Al.GetErrorString(alError), null, null, false, true);
						return;
					}
				}
				else
				{
					uint alSource2 = this.Sound.Owner.GetSourceFromIndex(this.Sound.SourcePoolIndex, this.ALSourceIndex);
					Al.Sourcei(alSource2, 514, 1);
					int alError2 = Al.GetError();
					if (alError2 != 0)
					{
						DebugConsole.ThrowError("Failed to disable source's relative flag: " + this.debugName + ", " + Al.GetErrorString(alError2), null, null, false, true);
						return;
					}
					Al.Source3f(alSource2, 4100, 0f, 0f, 0f);
					alError2 = Al.GetError();
					if (alError2 != 0)
					{
						DebugConsole.ThrowError("Failed to reset source's position: " + this.debugName + ", " + Al.GetErrorString(alError2), null, null, false, true);
						return;
					}
				}
			}
		}

		// Token: 0x1700127C RID: 4732
		// (get) Token: 0x06004891 RID: 18577 RVA: 0x0027AEFA File Offset: 0x002790FA
		// (set) Token: 0x06004892 RID: 18578 RVA: 0x0027AF04 File Offset: 0x00279104
		public float Near
		{
			get
			{
				return this.near;
			}
			set
			{
				this.near = value;
				if (this.ALSourceIndex < 0)
				{
					return;
				}
				uint alSource = this.Sound.Owner.GetSourceFromIndex(this.Sound.SourcePoolIndex, this.ALSourceIndex);
				Al.Sourcef(alSource, 4128, this.near);
				int alError = Al.GetError();
				if (alError != 0)
				{
					DebugConsole.ThrowError("Failed to set source's reference distance: " + this.debugName + ", " + Al.GetErrorString(alError), null, null, false, true);
					return;
				}
			}
		}

		// Token: 0x1700127D RID: 4733
		// (get) Token: 0x06004893 RID: 18579 RVA: 0x0027AF83 File Offset: 0x00279183
		// (set) Token: 0x06004894 RID: 18580 RVA: 0x0027AF8C File Offset: 0x0027918C
		public float Far
		{
			get
			{
				return this.far;
			}
			set
			{
				this.far = value;
				if (this.ALSourceIndex < 0)
				{
					return;
				}
				uint alSource = this.Sound.Owner.GetSourceFromIndex(this.Sound.SourcePoolIndex, this.ALSourceIndex);
				Al.Sourcef(alSource, 4131, this.far);
				int alError = Al.GetError();
				if (alError != 0)
				{
					DebugConsole.ThrowError("Failed to set source's max distance: " + this.debugName + ", " + Al.GetErrorString(alError), null, null, false, true);
					return;
				}
			}
		}

		// Token: 0x1700127E RID: 4734
		// (get) Token: 0x06004895 RID: 18581 RVA: 0x0027B00B File Offset: 0x0027920B
		// (set) Token: 0x06004896 RID: 18582 RVA: 0x0027B014 File Offset: 0x00279214
		public float Gain
		{
			get
			{
				return this.gain;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					return;
				}
				this.gain = Math.Max(value, 0f);
				if (this.ALSourceIndex < 0)
				{
					return;
				}
				uint alSource = this.Sound.Owner.GetSourceFromIndex(this.Sound.SourcePoolIndex, this.ALSourceIndex);
				float effectiveGain = this.gain;
				if (this.category != null)
				{
					effectiveGain *= this.Sound.Owner.GetCategoryGainMultiplier(this.category, -1);
				}
				Al.Sourcef(alSource, 4106, effectiveGain);
				int alError = Al.GetError();
				if (alError != 0)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(53, 4);
					defaultInterpolatedStringHandler.AppendLiteral("Failed to set source's gain to ");
					defaultInterpolatedStringHandler.AppendFormatted<float>(this.gain);
					defaultInterpolatedStringHandler.AppendLiteral(" (effective gain ");
					defaultInterpolatedStringHandler.AppendFormatted<float>(effectiveGain);
					defaultInterpolatedStringHandler.AppendLiteral("): ");
					defaultInterpolatedStringHandler.AppendFormatted(this.debugName);
					defaultInterpolatedStringHandler.AppendLiteral(", ");
					defaultInterpolatedStringHandler.AppendFormatted(Al.GetErrorString(alError));
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, true);
					return;
				}
			}
		}

		// Token: 0x1700127F RID: 4735
		// (get) Token: 0x06004897 RID: 18583 RVA: 0x0027B125 File Offset: 0x00279325
		// (set) Token: 0x06004898 RID: 18584 RVA: 0x0027B130 File Offset: 0x00279330
		public bool Looping
		{
			get
			{
				return this.looping;
			}
			set
			{
				this.looping = value;
				if (this.ALSourceIndex < 0)
				{
					return;
				}
				if (!this.IsStream)
				{
					uint alSource = this.Sound.Owner.GetSourceFromIndex(this.Sound.SourcePoolIndex, this.ALSourceIndex);
					Al.Sourcei(alSource, 4103, (this.looping > false) ? 1 : 0);
					int alError = Al.GetError();
					if (alError != 0)
					{
						DebugConsole.ThrowError("Failed to set source's looping state: " + this.debugName + ", " + Al.GetErrorString(alError), null, null, false, true);
						return;
					}
				}
			}
		}

		// Token: 0x17001280 RID: 4736
		// (get) Token: 0x06004899 RID: 18585 RVA: 0x0027B1BA File Offset: 0x002793BA
		// (set) Token: 0x0600489A RID: 18586 RVA: 0x0027B1C4 File Offset: 0x002793C4
		public float FrequencyMultiplier
		{
			get
			{
				return this.frequencyMultiplier;
			}
			set
			{
				bool flag = value < 0.25f || value > 4f;
				if (flag)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(35, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Frequency multiplier out of range: ");
					defaultInterpolatedStringHandler.AppendFormatted<float>(value);
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear() + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				}
				this.frequencyMultiplier = Math.Clamp(value, 0.25f, 4f);
				if (this.ALSourceIndex < 0)
				{
					return;
				}
				uint alSource = this.Sound.Owner.GetSourceFromIndex(this.Sound.SourcePoolIndex, this.ALSourceIndex);
				Al.Sourcef(alSource, 4099, this.frequencyMultiplier);
				int alError = Al.GetError();
				if (alError != 0)
				{
					throw new Exception("Failed to set source's frequency multiplier: " + this.debugName + ", " + Al.GetErrorString(alError));
				}
			}
		}

		// Token: 0x17001281 RID: 4737
		// (get) Token: 0x0600489B RID: 18587 RVA: 0x0027B2A4 File Offset: 0x002794A4
		// (set) Token: 0x0600489C RID: 18588 RVA: 0x0027B2AC File Offset: 0x002794AC
		public bool FilledByNetwork { get; private set; }

		// Token: 0x17001282 RID: 4738
		// (get) Token: 0x0600489D RID: 18589 RVA: 0x0027B2B5 File Offset: 0x002794B5
		// (set) Token: 0x0600489E RID: 18590 RVA: 0x0027B2C0 File Offset: 0x002794C0
		public bool Muffled
		{
			get
			{
				return this.muffled;
			}
			set
			{
				if (this.muffled == value)
				{
					return;
				}
				this.muffled = value;
				if (this.ALSourceIndex < 0)
				{
					return;
				}
				if (!this.IsPlaying)
				{
					return;
				}
				if (this.IsStream)
				{
					return;
				}
				uint alSource = this.Sound.Owner.GetSourceFromIndex(this.Sound.SourcePoolIndex, this.ALSourceIndex);
				int playbackPos;
				Al.GetSourcei(alSource, 4133, out playbackPos);
				int alError = Al.GetError();
				if (alError != 0)
				{
					DebugConsole.ThrowError("Failed to get source's playback position: " + this.debugName + ", " + Al.GetErrorString(alError), null, null, false, true);
					return;
				}
				Al.SourceStop(alSource);
				alError = Al.GetError();
				if (alError != 0)
				{
					DebugConsole.ThrowError("Failed to stop source: " + this.debugName + ", " + Al.GetErrorString(alError), null, null, false, true);
					return;
				}
				this.Sound.FillAlBuffers();
				SoundBuffers buffers = this.Sound.Buffers;
				if (buffers == null || buffers.AlBuffer == 0U || buffers.AlMuffledBuffer == 0U)
				{
					return;
				}
				Al.Sourcei(alSource, 4105, (int)(this.muffled ? this.Sound.Buffers.AlMuffledBuffer : this.Sound.Buffers.AlBuffer));
				alError = Al.GetError();
				if (alError != 0)
				{
					DebugConsole.ThrowError("Failed to bind buffer to source: " + this.debugName + ", " + Al.GetErrorString(alError), null, null, false, true);
					return;
				}
				Al.SourcePlay(alSource);
				alError = Al.GetError();
				if (alError != 0)
				{
					DebugConsole.ThrowError("Failed to replay source: " + this.debugName + ", " + Al.GetErrorString(alError), null, null, false, true);
					return;
				}
				Al.Sourcei(alSource, 4133, playbackPos);
				alError = Al.GetError();
				if (alError != 0)
				{
					DebugConsole.ThrowError("Failed to reset playback position: " + this.debugName + ", " + Al.GetErrorString(alError), null, null, false, true);
					return;
				}
			}
		}

		// Token: 0x17001283 RID: 4739
		// (get) Token: 0x0600489F RID: 18591 RVA: 0x0027B488 File Offset: 0x00279688
		public float CurrentAmplitude
		{
			get
			{
				if (!this.IsPlaying)
				{
					return 0f;
				}
				Sound sound = this.Sound;
				uint? num;
				if (sound == null)
				{
					num = null;
				}
				else
				{
					SoundManager owner = sound.Owner;
					num = ((owner != null) ? new uint?(owner.GetSourceFromIndex(this.Sound.SourcePoolIndex, this.ALSourceIndex)) : null);
				}
				uint? num2 = num;
				uint alSource = num2.GetValueOrDefault();
				if (alSource == 0U)
				{
					return 0f;
				}
				if (this.IsStream)
				{
					Monitor.Enter(this.mutex);
					float retVal = this.streamAmplitude;
					Monitor.Exit(this.mutex);
					return retVal;
				}
				int playbackPos;
				Al.GetSourcei(alSource, 4133, out playbackPos);
				int alError = Al.GetError();
				if (alError != 0)
				{
					DebugConsole.ThrowError("Failed to get source's playback position: " + this.debugName + ", " + Al.GetErrorString(alError), null, null, false, true);
					return 0f;
				}
				return this.Sound.GetAmplitudeAtPlaybackPos(playbackPos);
			}
		}

		// Token: 0x17001284 RID: 4740
		// (get) Token: 0x060048A0 RID: 18592 RVA: 0x0027B56E File Offset: 0x0027976E
		// (set) Token: 0x060048A1 RID: 18593 RVA: 0x0027B576 File Offset: 0x00279776
		public Identifier Category
		{
			get
			{
				return this.category;
			}
			set
			{
				this.category = value;
				this.Gain = this.gain;
			}
		}

		// Token: 0x17001285 RID: 4741
		// (get) Token: 0x060048A2 RID: 18594 RVA: 0x0027B58B File Offset: 0x0027978B
		// (set) Token: 0x060048A3 RID: 18595 RVA: 0x0027B593 File Offset: 0x00279793
		public Sound Sound { get; private set; }

		// Token: 0x17001286 RID: 4742
		// (get) Token: 0x060048A4 RID: 18596 RVA: 0x0027B59C File Offset: 0x0027979C
		// (set) Token: 0x060048A5 RID: 18597 RVA: 0x0027B5A4 File Offset: 0x002797A4
		public int ALSourceIndex { get; private set; } = -1;

		// Token: 0x17001287 RID: 4743
		// (get) Token: 0x060048A6 RID: 18598 RVA: 0x0027B5AD File Offset: 0x002797AD
		// (set) Token: 0x060048A7 RID: 18599 RVA: 0x0027B5B5 File Offset: 0x002797B5
		public bool IsStream { get; private set; }

		// Token: 0x17001288 RID: 4744
		// (get) Token: 0x060048A8 RID: 18600 RVA: 0x0027B5BE File Offset: 0x002797BE
		// (set) Token: 0x060048A9 RID: 18601 RVA: 0x0027B5C6 File Offset: 0x002797C6
		public int StreamSeekPos
		{
			get
			{
				return this.streamSeekPos;
			}
			set
			{
				if (!this.IsStream)
				{
					throw new InvalidOperationException("Cannot set StreamSeekPos on a non-streaming sound channel.");
				}
				this.streamSeekPos = Math.Max(value, 0);
			}
		}

		// Token: 0x17001289 RID: 4745
		// (get) Token: 0x060048AA RID: 18602 RVA: 0x0027B5E8 File Offset: 0x002797E8
		public long MaxStreamSeekPos
		{
			get
			{
				if (this.IsStream)
				{
					OggSound oggSound = this.Sound as OggSound;
					if (oggSound != null)
					{
						return oggSound.MaxStreamSamplePos;
					}
				}
				return 0L;
			}
		}

		// Token: 0x1700128A RID: 4746
		// (get) Token: 0x060048AB RID: 18603 RVA: 0x0027B618 File Offset: 0x00279818
		public bool IsPlaying
		{
			get
			{
				if (this.ALSourceIndex < 0)
				{
					return false;
				}
				if (this.IsStream && !this.reachedEndSample)
				{
					return true;
				}
				uint alSource = this.Sound.Owner.GetSourceFromIndex(this.Sound.SourcePoolIndex, this.ALSourceIndex);
				if (!Al.IsSource(alSource))
				{
					return false;
				}
				int state;
				Al.GetSourcei(alSource, 4112, out state);
				int alError = Al.GetError();
				if (alError != 0)
				{
					DebugConsole.ThrowError("Failed to determine playing state from source: " + this.debugName + ", " + Al.GetErrorString(alError), null, null, false, true);
					return false;
				}
				return state == 4114;
			}
		}

		// Token: 0x060048AC RID: 18604 RVA: 0x0027B6B4 File Offset: 0x002798B4
		public SoundChannel(Sound sound, float gain, Vector3? position, float freqMult, float near, float far, Identifier category, bool muffle = false)
		{
			SoundChannel.<>c__DisplayClass73_0 CS$<>8__locals1;
			CS$<>8__locals1.position = position;
			CS$<>8__locals1.gain = gain;
			CS$<>8__locals1.freqMult = freqMult;
			CS$<>8__locals1.near = near;
			CS$<>8__locals1.far = far;
			CS$<>8__locals1.category = category;
			base..ctor();
			CS$<>8__locals1.<>4__this = this;
			this.Sound = sound;
			this.debugName = ((sound == null) ? "SoundChannel (null)" : ("SoundChannel (" + (string.IsNullOrEmpty(sound.Filename) ? "filename empty" : sound.Filename) + ")"));
			this.IsStream = sound.Stream;
			this.FilledByNetwork = (sound is VoipSound);
			this.decayTimer = 0;
			this.streamSeekPos = 0;
			this.reachedEndSample = false;
			this.buffersToRequeue = 4;
			this.muffled = muffle;
			if (this.IsStream)
			{
				this.mutex = new object();
			}
			try
			{
				if (this.mutex != null)
				{
					Monitor.Enter(this.mutex);
				}
				if (sound.Owner.CountPlayingInstances(sound) < sound.MaxSimultaneousInstances)
				{
					this.ALSourceIndex = sound.Owner.AssignFreeSourceToChannel(this);
				}
				if (this.ALSourceIndex >= 0)
				{
					if (!this.IsStream)
					{
						Al.Sourcei(sound.Owner.GetSourceFromIndex(this.Sound.SourcePoolIndex, this.ALSourceIndex), 4105, 0);
						int alError = Al.GetError();
						if (alError != 0)
						{
							throw new Exception("Failed to reset source buffer: " + this.debugName + ", " + Al.GetErrorString(alError));
						}
						this.Sound.FillAlBuffers();
						SoundBuffers buffers = this.Sound.Buffers;
						if (buffers == null || buffers.AlBuffer == 0U || buffers.AlMuffledBuffer == 0U)
						{
							return;
						}
						uint alBuffer = (sound.Owner.GetCategoryMuffle(CS$<>8__locals1.category) || this.muffled) ? this.Sound.Buffers.AlMuffledBuffer : this.Sound.Buffers.AlBuffer;
						Al.Sourcei(sound.Owner.GetSourceFromIndex(this.Sound.SourcePoolIndex, this.ALSourceIndex), 4105, (int)alBuffer);
						alError = Al.GetError();
						if (alError != 0)
						{
							throw new Exception(string.Concat(new string[]
							{
								"Failed to bind buffer to source (",
								this.ALSourceIndex.ToString(),
								":",
								sound.Owner.GetSourceFromIndex(this.Sound.SourcePoolIndex, this.ALSourceIndex).ToString(),
								",",
								alBuffer.ToString(),
								"): ",
								this.debugName,
								", ",
								Al.GetErrorString(alError)
							}));
						}
						this.<.ctor>g__SetProperties|73_0(ref CS$<>8__locals1);
						Al.SourcePlay(sound.Owner.GetSourceFromIndex(this.Sound.SourcePoolIndex, this.ALSourceIndex));
						alError = Al.GetError();
						if (alError != 0)
						{
							throw new Exception("Failed to play source: " + this.debugName + ", " + Al.GetErrorString(alError));
						}
					}
					else
					{
						uint alBuffer2 = 0U;
						Al.Sourcei(sound.Owner.GetSourceFromIndex(this.Sound.SourcePoolIndex, this.ALSourceIndex), 4105, (int)alBuffer2);
						int alError2 = Al.GetError();
						if (alError2 != 0)
						{
							throw new Exception("Failed to reset source buffer: " + this.debugName + ", " + Al.GetErrorString(alError2));
						}
						Al.Sourcei(sound.Owner.GetSourceFromIndex(this.Sound.SourcePoolIndex, this.ALSourceIndex), 4103, 0);
						alError2 = Al.GetError();
						if (alError2 != 0)
						{
							throw new Exception("Failed to set stream looping state: " + this.debugName + ", " + Al.GetErrorString(alError2));
						}
						this.streamShortBuffer = new short[8820];
						this.streamBuffers = new uint[4];
						this.unqueuedBuffers = new uint[4];
						this.streamBufferAmplitudes = new float[4];
						for (int i = 0; i < 4; i++)
						{
							Al.GenBuffer(out this.streamBuffers[i]);
							alError2 = Al.GetError();
							if (alError2 != 0)
							{
								throw new Exception("Failed to generate stream buffers: " + this.debugName + ", " + Al.GetErrorString(alError2));
							}
							if (!Al.IsBuffer(this.streamBuffers[i]))
							{
								throw new Exception("Generated streamBuffer[" + i.ToString() + "] is invalid! " + this.debugName);
							}
						}
						this.Sound.Owner.InitUpdateChannelThread();
						this.<.ctor>g__SetProperties|73_0(ref CS$<>8__locals1);
					}
				}
			}
			catch
			{
				throw;
			}
			finally
			{
				if (this.mutex != null)
				{
					Monitor.Exit(this.mutex);
				}
			}
			this.Sound.Owner.Update();
		}

		// Token: 0x060048AD RID: 18605 RVA: 0x0027BBA8 File Offset: 0x00279DA8
		public override string ToString()
		{
			return this.debugName;
		}

		// Token: 0x1700128B RID: 4747
		// (get) Token: 0x060048AE RID: 18606 RVA: 0x0027BBB0 File Offset: 0x00279DB0
		// (set) Token: 0x060048AF RID: 18607 RVA: 0x0027BBB8 File Offset: 0x00279DB8
		public bool FadingOutAndDisposing { get; private set; }

		// Token: 0x060048B0 RID: 18608 RVA: 0x0027BBC1 File Offset: 0x00279DC1
		public void FadeOutAndDispose()
		{
			this.FadingOutAndDisposing = true;
			this.Sound.Owner.InitUpdateChannelThread();
		}

		// Token: 0x060048B1 RID: 18609 RVA: 0x0027BBDC File Offset: 0x00279DDC
		public void Dispose()
		{
			try
			{
				if (this.mutex != null)
				{
					Monitor.Enter(this.mutex);
				}
				if (this.ALSourceIndex >= 0)
				{
					Al.SourceStop(this.Sound.Owner.GetSourceFromIndex(this.Sound.SourcePoolIndex, this.ALSourceIndex));
					int alError = Al.GetError();
					if (alError != 0)
					{
						throw new Exception("Failed to stop source: " + this.debugName + ", " + Al.GetErrorString(alError));
					}
					if (this.IsStream)
					{
						uint alSource = this.Sound.Owner.GetSourceFromIndex(this.Sound.SourcePoolIndex, this.ALSourceIndex);
						Al.SourceStop(alSource);
						alError = Al.GetError();
						if (alError != 0)
						{
							throw new Exception("Failed to stop streamed source: " + this.debugName + ", " + Al.GetErrorString(alError));
						}
						int buffersToRequeue = 0;
						buffersToRequeue = 0;
						Al.GetSourcei(alSource, 4118, out buffersToRequeue);
						alError = Al.GetError();
						if (alError != 0)
						{
							throw new Exception("Failed to determine processed buffers from streamed source: " + this.debugName + ", " + Al.GetErrorString(alError));
						}
						Al.SourceUnqueueBuffers(alSource, buffersToRequeue, this.unqueuedBuffers);
						alError = Al.GetError();
						if (alError != 0)
						{
							throw new Exception("Failed to unqueue buffers from streamed source: " + this.debugName + ", " + Al.GetErrorString(alError));
						}
						Al.Sourcei(alSource, 4105, 0);
						alError = Al.GetError();
						if (alError != 0)
						{
							throw new Exception("Failed to reset buffer for streamed source: " + this.debugName + ", " + Al.GetErrorString(alError));
						}
						for (int i = 0; i < 4; i++)
						{
							Al.DeleteBuffer(this.streamBuffers[i]);
							alError = Al.GetError();
							if (alError != 0)
							{
								throw new Exception(string.Concat(new string[]
								{
									"Failed to delete streamBuffers[",
									i.ToString(),
									"] (",
									this.streamBuffers[i].ToString(),
									"): ",
									this.debugName,
									", ",
									Al.GetErrorString(alError)
								}));
							}
						}
						this.reachedEndSample = true;
					}
					else
					{
						Al.Sourcei(this.Sound.Owner.GetSourceFromIndex(this.Sound.SourcePoolIndex, this.ALSourceIndex), 4105, 0);
						alError = Al.GetError();
						if (alError != 0)
						{
							throw new Exception("Failed to unbind buffer to non-streamed source: " + this.debugName + ", " + Al.GetErrorString(alError));
						}
					}
					this.ALSourceIndex = -1;
					this.debugName += " [DISPOSED]";
				}
			}
			finally
			{
				if (this.mutex != null)
				{
					Monitor.Exit(this.mutex);
				}
			}
		}

		// Token: 0x060048B2 RID: 18610 RVA: 0x0027BE90 File Offset: 0x0027A090
		public void UpdateStream()
		{
			try
			{
				if (!this.IsStream)
				{
					throw new Exception("Called UpdateStream on a non-streamed sound channel!");
				}
				Monitor.Enter(this.mutex);
				if (!this.reachedEndSample)
				{
					uint alSource = this.Sound.Owner.GetSourceFromIndex(this.Sound.SourcePoolIndex, this.ALSourceIndex);
					int state;
					Al.GetSourcei(alSource, 4112, out state);
					bool playing = state == 4114;
					int alError = Al.GetError();
					if (alError != 0)
					{
						throw new Exception("Failed to determine playing state from streamed source: " + this.debugName + ", " + Al.GetErrorString(alError));
					}
					int unqueuedBufferCount;
					Al.GetSourcei(alSource, 4118, out unqueuedBufferCount);
					alError = Al.GetError();
					if (alError != 0)
					{
						throw new Exception("Failed to determine processed buffers from streamed source: " + this.debugName + ", " + Al.GetErrorString(alError));
					}
					Al.SourceUnqueueBuffers(alSource, unqueuedBufferCount, this.unqueuedBuffers);
					alError = Al.GetError();
					if (alError != 0)
					{
						throw new Exception("Failed to unqueue buffers from streamed source: " + this.debugName + ", " + Al.GetErrorString(alError));
					}
					this.buffersToRequeue += unqueuedBufferCount;
					int iterCount = this.buffersToRequeue;
					int i = 0;
					while (i < iterCount)
					{
						int index = this.queueStartIndex;
						short[] buffer = this.streamShortBuffer;
						int readSamples = this.Sound.FillStreamBuffer(this.streamSeekPos, buffer);
						float readAmplitude = 0f;
						for (int j = 0; j < Math.Min(readSamples, buffer.Length); j++)
						{
							float sampleF = (float)buffer[j] / 32767f;
							readAmplitude = Math.Max(readAmplitude, Math.Abs(sampleF));
						}
						if (this.FilledByNetwork)
						{
							if (readSamples <= 0)
							{
								this.streamAmplitude *= 0.5f;
								this.decayTimer++;
								if (this.decayTimer > 120)
								{
									this.reachedEndSample = true;
								}
							}
							else
							{
								VoipSound voipSound = this.Sound as VoipSound;
								if (voipSound != null)
								{
									voipSound.ApplyFilters(buffer, readSamples);
								}
								this.decayTimer = 0;
							}
						}
						else if (this.Sound.StreamsReliably)
						{
							this.streamSeekPos += readSamples * 2;
							if (readSamples * 2 < 8820)
							{
								if (this.looping)
								{
									this.streamSeekPos = 0;
								}
								else
								{
									this.reachedEndSample = true;
								}
							}
						}
						if (readSamples > 0)
						{
							this.streamBufferAmplitudes[index] = readAmplitude;
							Al.BufferData<short>(this.streamBuffers[index], this.Sound.ALFormat, buffer, readSamples * 2, this.Sound.SampleRate);
							alError = Al.GetError();
							if (alError != 0)
							{
								throw new Exception(string.Concat(new string[]
								{
									"Failed to assign data to stream buffer: ",
									Al.GetErrorString(alError),
									": ",
									this.streamBuffers[index].ToString(),
									"/",
									this.streamBuffers.Length.ToString(),
									", readSamples: ",
									readSamples.ToString(),
									", ",
									this.debugName
								}));
							}
							Al.SourceQueueBuffer(alSource, this.streamBuffers[index]);
							this.queueStartIndex = (this.queueStartIndex + 1) % 4;
							alError = Al.GetError();
							if (alError != 0)
							{
								throw new Exception(string.Concat(new string[]
								{
									"Failed to queue streamBuffer[",
									index.ToString(),
									"] to stream: ",
									this.debugName,
									", ",
									Al.GetErrorString(alError)
								}));
							}
							this.buffersToRequeue--;
							i++;
						}
						else
						{
							if (readSamples < 0)
							{
								this.reachedEndSample = true;
								break;
							}
							break;
						}
					}
					this.streamAmplitude = this.streamBufferAmplitudes[this.queueStartIndex];
					Al.GetSourcei(alSource, 4112, out state);
					alError = Al.GetError();
					if (alError != 0)
					{
						throw new Exception("Failed to retrieve stream source state: " + this.debugName + ", " + Al.GetErrorString(alError));
					}
					if (state != 4114)
					{
						Al.SourcePlay(alSource);
						alError = Al.GetError();
						if (alError != 0)
						{
							throw new Exception("Failed to start stream playback: " + this.debugName + ", " + Al.GetErrorString(alError));
						}
					}
				}
				if (this.reachedEndSample)
				{
					this.streamAmplitude = 0f;
				}
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("An exception was thrown when updating a sound stream (" + this.debugName + ")", e, null, false, false);
			}
			finally
			{
				Monitor.Exit(this.mutex);
			}
		}

		// Token: 0x060048B3 RID: 18611 RVA: 0x0027C31C File Offset: 0x0027A51C
		[CompilerGenerated]
		private void <.ctor>g__SetProperties|73_0(ref SoundChannel.<>c__DisplayClass73_0 A_1)
		{
			this.Position = A_1.position;
			this.Gain = A_1.gain;
			this.FrequencyMultiplier = A_1.freqMult;
			this.Looping = false;
			this.Near = A_1.near;
			this.Far = A_1.far;
			this.Category = A_1.category;
		}

		// Token: 0x04002597 RID: 9623
		private const int STREAM_BUFFER_SIZE = 8820;

		// Token: 0x04002598 RID: 9624
		private readonly short[] streamShortBuffer;

		// Token: 0x04002599 RID: 9625
		private string debugName = "SoundChannel";

		// Token: 0x0400259A RID: 9626
		private Vector3? position;

		// Token: 0x0400259B RID: 9627
		private float near;

		// Token: 0x0400259C RID: 9628
		private float far;

		// Token: 0x0400259D RID: 9629
		private float gain;

		// Token: 0x0400259E RID: 9630
		private bool looping;

		// Token: 0x0400259F RID: 9631
		public const float MinFrequencyMultiplier = 0.25f;

		// Token: 0x040025A0 RID: 9632
		public const float MaxFrequencyMultiplier = 4f;

		// Token: 0x040025A1 RID: 9633
		public float frequencyMultiplier;

		// Token: 0x040025A3 RID: 9635
		private int decayTimer;

		// Token: 0x040025A4 RID: 9636
		private bool muffled;

		// Token: 0x040025A5 RID: 9637
		private float streamAmplitude;

		// Token: 0x040025A6 RID: 9638
		private Identifier category;

		// Token: 0x040025AA RID: 9642
		private int streamSeekPos;

		// Token: 0x040025AB RID: 9643
		private int buffersToRequeue;

		// Token: 0x040025AC RID: 9644
		private bool reachedEndSample;

		// Token: 0x040025AD RID: 9645
		private int queueStartIndex;

		// Token: 0x040025AE RID: 9646
		private readonly uint[] streamBuffers;

		// Token: 0x040025AF RID: 9647
		private readonly uint[] unqueuedBuffers;

		// Token: 0x040025B0 RID: 9648
		private readonly float[] streamBufferAmplitudes;

		// Token: 0x040025B1 RID: 9649
		public bool MuteBackgroundMusic;

		// Token: 0x040025B2 RID: 9650
		private readonly object mutex;
	}
}
