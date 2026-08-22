using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Barotrauma.IO;
using Microsoft.Xna.Framework;
using OpenAL;

namespace Barotrauma.Sounds
{
	// Token: 0x02000448 RID: 1096
	internal class SoundManager : IDisposable
	{
		// Token: 0x1700128C RID: 4748
		// (get) Token: 0x060048C7 RID: 18631 RVA: 0x0027CDCF File Offset: 0x0027AFCF
		// (set) Token: 0x060048C8 RID: 18632 RVA: 0x0027CDD7 File Offset: 0x0027AFD7
		public bool Disabled { get; private set; }

		// Token: 0x1700128D RID: 4749
		// (get) Token: 0x060048C9 RID: 18633 RVA: 0x0027CDE0 File Offset: 0x0027AFE0
		public IReadOnlyList<Sound> LoadedSounds
		{
			get
			{
				return this.loadedSounds;
			}
		}

		// Token: 0x1700128E RID: 4750
		// (get) Token: 0x060048CA RID: 18634 RVA: 0x0027CDE8 File Offset: 0x0027AFE8
		// (set) Token: 0x060048CB RID: 18635 RVA: 0x0027CDF0 File Offset: 0x0027AFF0
		public bool CanDetectDisconnect { get; private set; }

		// Token: 0x1700128F RID: 4751
		// (get) Token: 0x060048CC RID: 18636 RVA: 0x0027CDF9 File Offset: 0x0027AFF9
		// (set) Token: 0x060048CD RID: 18637 RVA: 0x0027CE01 File Offset: 0x0027B001
		public bool Disconnected { get; private set; }

		// Token: 0x17001290 RID: 4752
		// (get) Token: 0x060048CE RID: 18638 RVA: 0x0027CE0A File Offset: 0x0027B00A
		// (set) Token: 0x060048CF RID: 18639 RVA: 0x0027CE14 File Offset: 0x0027B014
		public Vector3 ListenerPosition
		{
			get
			{
				return this.listenerPosition;
			}
			set
			{
				if (this.Disabled)
				{
					return;
				}
				this.listenerPosition = value;
				Al.Listener3f(4100, value.X, value.Y, value.Z);
				int alError = Al.GetError();
				if (alError != 0 && !GameMain.IsExiting)
				{
					throw new Exception("Failed to set listener position: " + Al.GetErrorString(alError));
				}
			}
		}

		// Token: 0x17001291 RID: 4753
		// (get) Token: 0x060048D0 RID: 18640 RVA: 0x0027CE73 File Offset: 0x0027B073
		// (set) Token: 0x060048D1 RID: 18641 RVA: 0x0027CE94 File Offset: 0x0027B094
		public Vector3 ListenerTargetVector
		{
			get
			{
				return new Vector3(this.listenerOrientation[0], this.listenerOrientation[1], this.listenerOrientation[2]);
			}
			set
			{
				if (this.Disabled)
				{
					return;
				}
				this.listenerOrientation[0] = value.X;
				this.listenerOrientation[1] = value.Y;
				this.listenerOrientation[2] = value.Z;
				Al.Listenerfv(4111, this.listenerOrientation);
				int alError = Al.GetError();
				if (alError != 0 && !GameMain.IsExiting)
				{
					throw new Exception("Failed to set listener target vector: " + Al.GetErrorString(alError));
				}
			}
		}

		// Token: 0x17001292 RID: 4754
		// (get) Token: 0x060048D2 RID: 18642 RVA: 0x0027CF0A File Offset: 0x0027B10A
		// (set) Token: 0x060048D3 RID: 18643 RVA: 0x0027CF2C File Offset: 0x0027B12C
		public Vector3 ListenerUpVector
		{
			get
			{
				return new Vector3(this.listenerOrientation[3], this.listenerOrientation[4], this.listenerOrientation[5]);
			}
			set
			{
				if (this.Disabled)
				{
					return;
				}
				this.listenerOrientation[3] = value.X;
				this.listenerOrientation[4] = value.Y;
				this.listenerOrientation[5] = value.Z;
				Al.Listenerfv(4111, this.listenerOrientation);
				int alError = Al.GetError();
				if (alError != 0 && !GameMain.IsExiting)
				{
					throw new Exception("Failed to set listener up vector: " + Al.GetErrorString(alError));
				}
			}
		}

		// Token: 0x17001293 RID: 4755
		// (get) Token: 0x060048D4 RID: 18644 RVA: 0x0027CFA2 File Offset: 0x0027B1A2
		// (set) Token: 0x060048D5 RID: 18645 RVA: 0x0027CFAC File Offset: 0x0027B1AC
		public float ListenerGain
		{
			get
			{
				return this.listenerGain;
			}
			set
			{
				if (this.Disabled)
				{
					return;
				}
				if (Math.Abs(this.ListenerGain - value) < 0.001f)
				{
					return;
				}
				this.listenerGain = value;
				Al.Listenerf(4106, this.listenerGain);
				int alError = Al.GetError();
				if (alError != 0 && !GameMain.IsExiting)
				{
					throw new Exception("Failed to set listener gain: " + Al.GetErrorString(alError));
				}
			}
		}

		// Token: 0x17001294 RID: 4756
		// (get) Token: 0x060048D6 RID: 18646 RVA: 0x0027D014 File Offset: 0x0027B214
		public float PlaybackAmplitude
		{
			get
			{
				if (this.Disabled)
				{
					return 0f;
				}
				float aggregateAmplitude = 0f;
				for (int i = 0; i < 2; i++)
				{
					foreach (SoundChannel soundChannel in from ch in this.playingChannels[i]
					where ch != null
					select ch)
					{
						float amplitude = soundChannel.CurrentAmplitude;
						amplitude *= soundChannel.Gain;
						float dist = Vector3.Distance(this.ListenerPosition, soundChannel.Position ?? this.ListenerPosition);
						if (dist > soundChannel.Near)
						{
							amplitude *= 1f - Math.Min(1f, (dist - soundChannel.Near) / (soundChannel.Far - soundChannel.Near));
						}
						aggregateAmplitude += amplitude;
					}
				}
				return aggregateAmplitude;
			}
		}

		// Token: 0x17001295 RID: 4757
		// (get) Token: 0x060048D7 RID: 18647 RVA: 0x0027D128 File Offset: 0x0027B328
		// (set) Token: 0x060048D8 RID: 18648 RVA: 0x0027D130 File Offset: 0x0027B330
		public float CompressionDynamicRangeGain { get; private set; }

		// Token: 0x17001296 RID: 4758
		// (get) Token: 0x060048D9 RID: 18649 RVA: 0x0027D139 File Offset: 0x0027B339
		// (set) Token: 0x060048DA RID: 18650 RVA: 0x0027D141 File Offset: 0x0027B341
		public float VoipAttenuatedGain
		{
			get
			{
				return this.voipAttenuatedGain;
			}
			set
			{
				this.lastAttenuationTime = Timing.TotalTime;
				this.voipAttenuatedGain = value;
			}
		}

		// Token: 0x17001297 RID: 4759
		// (get) Token: 0x060048DB RID: 18651 RVA: 0x0027D155 File Offset: 0x0027B355
		public int LoadedSoundCount
		{
			get
			{
				return this.loadedSounds.Count;
			}
		}

		// Token: 0x17001298 RID: 4760
		// (get) Token: 0x060048DC RID: 18652 RVA: 0x0027D162 File Offset: 0x0027B362
		public int UniqueLoadedSoundCount
		{
			get
			{
				return (from s in this.loadedSounds
				select s.Filename).Distinct<string>().Count<string>();
			}
		}

		// Token: 0x060048DD RID: 18653 RVA: 0x0027D198 File Offset: 0x0027B398
		public SoundManager()
		{
			this.loadedSounds = new List<Sound>();
			this.updateChannelsThread = null;
			this.sourcePools = new SoundSourcePool[2];
			this.playingChannels[0] = new SoundChannel[32];
			this.playingChannels[1] = new SoundChannel[16];
			string deviceName = GameSettings.CurrentConfig.Audio.AudioOutputDevice;
			if (string.IsNullOrEmpty(deviceName))
			{
				deviceName = Alc.GetString((IntPtr)null, 4100);
			}
			IReadOnlyList<string> audioDeviceNames = Alc.GetStringList((IntPtr)null, 4115);
			if (audioDeviceNames.Any<string>() && !audioDeviceNames.Any((string n) => n.Equals(deviceName, StringComparison.OrdinalIgnoreCase)))
			{
				deviceName = audioDeviceNames[0];
			}
			if (GameSettings.CurrentConfig.Audio.AudioOutputDevice != deviceName)
			{
				SoundManager.SetAudioOutputDevice(deviceName);
			}
			this.InitializeAlcDevice(deviceName);
			this.ListenerPosition = Vector3.Zero;
			this.ListenerTargetVector = new Vector3(0f, 0f, 1f);
			this.ListenerUpVector = new Vector3(0f, -1f, 0f);
			this.CompressionDynamicRangeGain = 1f;
		}

		// Token: 0x060048DE RID: 18654 RVA: 0x0027D310 File Offset: 0x0027B510
		private unsafe static void SetAudioOutputDevice(string deviceName)
		{
			GameSettings.Config config = *GameSettings.CurrentConfig;
			config.Audio.AudioOutputDevice = deviceName;
			GameSettings.SetCurrentConfig(config);
		}

		// Token: 0x060048DF RID: 18655 RVA: 0x0027D33C File Offset: 0x0027B53C
		public bool InitializeAlcDevice(string deviceName)
		{
			this.ReleaseResources(true);
			DebugConsole.NewMessage("Attempting to open ALC device \"" + deviceName + "\"", null, false);
			this.alcDevice = IntPtr.Zero;
			int alcError;
			for (int i = 0; i < 3; i++)
			{
				this.alcDevice = Alc.OpenDevice(deviceName);
				if (this.alcDevice == IntPtr.Zero)
				{
					alcError = Alc.GetError(IntPtr.Zero);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(72, 2);
					defaultInterpolatedStringHandler.AppendLiteral("ALC device initialization attempt #");
					defaultInterpolatedStringHandler.AppendFormatted<int>(i + 1);
					defaultInterpolatedStringHandler.AppendLiteral(" failed: device is null (error code ");
					defaultInterpolatedStringHandler.AppendFormatted(Alc.GetErrorString(alcError));
					defaultInterpolatedStringHandler.AppendLiteral(")");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, false);
					if (!string.IsNullOrEmpty(deviceName))
					{
						deviceName = null;
						DebugConsole.NewMessage("Switching to default device...", null, false);
					}
				}
				else
				{
					alcError = Alc.GetError(this.alcDevice);
					if (alcError == 0)
					{
						break;
					}
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(55, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("ALC device initialization attempt #");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(i + 1);
					defaultInterpolatedStringHandler2.AppendLiteral(" failed: error code ");
					defaultInterpolatedStringHandler2.AppendFormatted(Alc.GetErrorString(alcError));
					DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), null, false);
					if (!Alc.CloseDevice(this.alcDevice))
					{
						DebugConsole.NewMessage("Failed to close ALC device", null, false);
					}
					this.alcDevice = IntPtr.Zero;
				}
			}
			if (this.alcDevice == IntPtr.Zero)
			{
				DebugConsole.ThrowError("ALC device creation failed too many times!", null, null, false, false);
				this.Disabled = true;
				return false;
			}
			this.CanDetectDisconnect = Alc.IsExtensionPresent(this.alcDevice, "ALC_EXT_disconnect");
			alcError = Alc.GetError(this.alcDevice);
			if (alcError != 0)
			{
				DebugConsole.ThrowError("Error determining if disconnect can be detected: " + alcError.ToString() + ". Disabling audio playback...", null, null, false, false);
				this.Disabled = true;
				return false;
			}
			this.Disconnected = false;
			int[] alcContextAttrs = new int[0];
			this.alcContext = Alc.CreateContext(this.alcDevice, alcContextAttrs);
			IntPtr intPtr = this.alcContext;
			if (!Alc.MakeContextCurrent(this.alcContext))
			{
				DebugConsole.ThrowError("Failed to assign the current ALC context! (error code: " + Alc.GetError(this.alcDevice).ToString() + "). Disabling audio playback...", null, null, false, false);
				this.Disabled = true;
				return false;
			}
			alcError = Alc.GetError(this.alcDevice);
			if (alcError != 0)
			{
				DebugConsole.ThrowError("Error after assigning ALC context: " + Alc.GetErrorString(alcError) + ". Disabling audio playback...", null, null, false, false);
				this.Disabled = true;
				return false;
			}
			Al.DistanceModel(53252);
			int alError = Al.GetError();
			if (alError != 0)
			{
				DebugConsole.ThrowError("Error setting distance model: " + Al.GetErrorString(alError) + ". Disabling audio playback...", null, null, false, false);
				this.Disabled = true;
				return false;
			}
			this.sourcePools[0] = new SoundSourcePool(32);
			this.sourcePools[1] = new SoundSourcePool(16);
			this.ReloadSounds();
			this.Disabled = false;
			return true;
		}

		// Token: 0x060048E0 RID: 18656 RVA: 0x0027D644 File Offset: 0x0027B844
		public Sound LoadSound(string filename, bool stream = false)
		{
			if (this.Disabled)
			{
				return null;
			}
			if (!File.Exists(filename))
			{
				throw new FileNotFoundException("Sound file \"" + filename + "\" doesn't exist!");
			}
			Sound newSound = new OggSound(this, filename, stream, null);
			List<Sound> obj = this.loadedSounds;
			lock (obj)
			{
				this.loadedSounds.Add(newSound);
			}
			return newSound;
		}

		// Token: 0x060048E1 RID: 18657 RVA: 0x0027D6C0 File Offset: 0x0027B8C0
		public Sound LoadSound(ContentXElement element, bool stream = false, string overrideFilePath = null)
		{
			if (this.Disabled)
			{
				return null;
			}
			string text = overrideFilePath;
			if (overrideFilePath == null)
			{
				ContentPath attributeContentPath = element.GetAttributeContentPath("file");
				text = (((attributeContentPath != null) ? attributeContentPath.Value : null) ?? "");
			}
			string filePath = text;
			if (!File.Exists(filePath))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(48, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Sound file \"");
				defaultInterpolatedStringHandler.AppendFormatted(filePath);
				defaultInterpolatedStringHandler.AppendLiteral("\" doesn't exist! Content package \"");
				ContentPackage contentPackage = element.ContentPackage;
				defaultInterpolatedStringHandler.AppendFormatted(((contentPackage != null) ? contentPackage.Name : null) ?? "Unknown");
				defaultInterpolatedStringHandler.AppendLiteral("\".");
				throw new FileNotFoundException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			OggSound newSound = new OggSound(this, filePath, stream, element)
			{
				BaseGain = element.GetAttributeFloat("volume", 1f)
			};
			float range = element.GetAttributeFloat("range", 1000f);
			newSound.BaseNear = range * 0.4f;
			newSound.BaseFar = range;
			List<Sound> obj = this.loadedSounds;
			lock (obj)
			{
				this.loadedSounds.Add(newSound);
			}
			return newSound;
		}

		// Token: 0x060048E2 RID: 18658 RVA: 0x0027D7F0 File Offset: 0x0027B9F0
		public SoundChannel GetSoundChannelFromIndex(SoundManager.SourcePoolIndex poolIndex, int ind)
		{
			if (this.Disabled || ind < 0 || ind >= this.playingChannels[(int)poolIndex].Length)
			{
				return null;
			}
			return this.playingChannels[(int)poolIndex][ind];
		}

		// Token: 0x060048E3 RID: 18659 RVA: 0x0027D818 File Offset: 0x0027BA18
		public uint GetSourceFromIndex(SoundManager.SourcePoolIndex poolIndex, int srcInd)
		{
			if (this.Disabled || srcInd < 0 || srcInd >= this.sourcePools[(int)poolIndex].ALSources.Length)
			{
				return 0U;
			}
			if (!Al.IsSource(this.sourcePools[(int)poolIndex].ALSources[srcInd]))
			{
				throw new Exception("alSources[" + srcInd.ToString() + "] is invalid!");
			}
			return this.sourcePools[(int)poolIndex].ALSources[srcInd];
		}

		// Token: 0x060048E4 RID: 18660 RVA: 0x0027D888 File Offset: 0x0027BA88
		public int AssignFreeSourceToChannel(SoundChannel newChannel)
		{
			if (this.Disabled)
			{
				return -1;
			}
			int poolIndex = (int)newChannel.Sound.SourcePoolIndex;
			SoundChannel[] obj = this.playingChannels[poolIndex];
			lock (obj)
			{
				for (int i = 0; i < this.playingChannels[poolIndex].Length; i++)
				{
					if (this.playingChannels[poolIndex][i] == null || !this.playingChannels[poolIndex][i].IsPlaying)
					{
						if (this.playingChannels[poolIndex][i] != null)
						{
							this.playingChannels[poolIndex][i].Dispose();
						}
						this.playingChannels[poolIndex][i] = newChannel;
						return i;
					}
				}
			}
			return -1;
		}

		// Token: 0x060048E5 RID: 18661 RVA: 0x0027D93C File Offset: 0x0027BB3C
		public bool IsPlaying(Sound sound)
		{
			if (this.Disabled)
			{
				return false;
			}
			SoundChannel[] obj = this.playingChannels[(int)sound.SourcePoolIndex];
			lock (obj)
			{
				for (int i = 0; i < this.playingChannels[(int)sound.SourcePoolIndex].Length; i++)
				{
					if (this.playingChannels[(int)sound.SourcePoolIndex][i] != null && this.playingChannels[(int)sound.SourcePoolIndex][i].Sound == sound && this.playingChannels[(int)sound.SourcePoolIndex][i].IsPlaying)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060048E6 RID: 18662 RVA: 0x0027D9E8 File Offset: 0x0027BBE8
		public int CountPlayingInstances(Sound sound)
		{
			if (this.Disabled)
			{
				return 0;
			}
			int count = 0;
			SoundChannel[] obj = this.playingChannels[(int)sound.SourcePoolIndex];
			lock (obj)
			{
				for (int i = 0; i < this.playingChannels[(int)sound.SourcePoolIndex].Length; i++)
				{
					if (this.playingChannels[(int)sound.SourcePoolIndex][i] != null && this.playingChannels[(int)sound.SourcePoolIndex][i].Sound.Filename == sound.Filename && this.playingChannels[(int)sound.SourcePoolIndex][i].IsPlaying)
					{
						count++;
					}
				}
			}
			return count;
		}

		// Token: 0x060048E7 RID: 18663 RVA: 0x0027DAA4 File Offset: 0x0027BCA4
		public SoundChannel GetChannelFromSound(Sound sound)
		{
			if (this.Disabled)
			{
				return null;
			}
			SoundChannel[] obj = this.playingChannels[(int)sound.SourcePoolIndex];
			lock (obj)
			{
				for (int i = 0; i < this.playingChannels[(int)sound.SourcePoolIndex].Length; i++)
				{
					if (this.playingChannels[(int)sound.SourcePoolIndex][i] != null && this.playingChannels[(int)sound.SourcePoolIndex][i].Sound == sound && this.playingChannels[(int)sound.SourcePoolIndex][i].IsPlaying)
					{
						return this.playingChannels[(int)sound.SourcePoolIndex][i];
					}
				}
			}
			return null;
		}

		// Token: 0x060048E8 RID: 18664 RVA: 0x0027DB5C File Offset: 0x0027BD5C
		public void KillChannels(Sound sound)
		{
			if (this.Disabled)
			{
				return;
			}
			SoundChannel[] obj = this.playingChannels[(int)sound.SourcePoolIndex];
			lock (obj)
			{
				for (int i = 0; i < this.playingChannels[(int)sound.SourcePoolIndex].Length; i++)
				{
					if (this.playingChannels[(int)sound.SourcePoolIndex][i] != null && this.playingChannels[(int)sound.SourcePoolIndex][i].Sound == sound)
					{
						SoundChannel soundChannel = this.playingChannels[(int)sound.SourcePoolIndex][i];
						if (soundChannel != null)
						{
							soundChannel.Dispose();
						}
						this.playingChannels[(int)sound.SourcePoolIndex][i] = null;
					}
				}
			}
		}

		// Token: 0x060048E9 RID: 18665 RVA: 0x0027DC14 File Offset: 0x0027BE14
		public void RemoveSound(Sound sound)
		{
			List<Sound> obj = this.loadedSounds;
			lock (obj)
			{
				for (int i = 0; i < this.loadedSounds.Count; i++)
				{
					if (this.loadedSounds[i] == sound)
					{
						this.loadedSounds.RemoveAt(i);
						break;
					}
				}
			}
		}

		// Token: 0x060048EA RID: 18666 RVA: 0x0027DC84 File Offset: 0x0027BE84
		public void MoveSoundToPosition(Sound sound, int pos)
		{
			List<Sound> obj = this.loadedSounds;
			lock (obj)
			{
				int index = this.loadedSounds.IndexOf(sound);
				if (index >= 0)
				{
					this.loadedSounds.SiftElement(index, pos);
				}
			}
		}

		// Token: 0x060048EB RID: 18667 RVA: 0x0027DCDC File Offset: 0x0027BEDC
		public void SetCategoryGainMultiplier(Identifier category, float gain, int index = 0)
		{
			if (this.Disabled)
			{
				return;
			}
			Dictionary<Identifier, SoundManager.CategoryModifier> obj = this.categoryModifiers;
			lock (obj)
			{
				if (!this.categoryModifiers.ContainsKey(category))
				{
					this.categoryModifiers.Add(category, new SoundManager.CategoryModifier(index, gain, false));
				}
				else
				{
					this.categoryModifiers[category].SetGainMultiplier(index, gain);
				}
			}
			for (int i = 0; i < this.playingChannels.Length; i++)
			{
				SoundChannel[] obj2 = this.playingChannels[i];
				lock (obj2)
				{
					for (int j = 0; j < this.playingChannels[i].Length; j++)
					{
						if (this.playingChannels[i][j] != null && this.playingChannels[i][j].IsPlaying)
						{
							this.playingChannels[i][j].Gain = this.playingChannels[i][j].Gain;
						}
					}
				}
			}
		}

		// Token: 0x060048EC RID: 18668 RVA: 0x0027DDF4 File Offset: 0x0027BFF4
		public float GetCategoryGainMultiplier(Identifier category, int index = -1)
		{
			if (this.Disabled)
			{
				return 0f;
			}
			Dictionary<Identifier, SoundManager.CategoryModifier> obj = this.categoryModifiers;
			float result;
			lock (obj)
			{
				SoundManager.CategoryModifier categoryModifier;
				if (this.categoryModifiers == null || !this.categoryModifiers.TryGetValue(category, out categoryModifier))
				{
					result = 1f;
				}
				else if (index < 0)
				{
					float accumulatedMultipliers = 1f;
					for (int i = 0; i < categoryModifier.GainMultipliers.Length; i++)
					{
						accumulatedMultipliers *= categoryModifier.GainMultipliers[i];
					}
					result = accumulatedMultipliers;
				}
				else
				{
					result = categoryModifier.GainMultipliers[index];
				}
			}
			return result;
		}

		// Token: 0x060048ED RID: 18669 RVA: 0x0027DE9C File Offset: 0x0027C09C
		public void SetCategoryMuffle(Identifier category, bool muffle)
		{
			if (this.Disabled)
			{
				return;
			}
			Dictionary<Identifier, SoundManager.CategoryModifier> obj = this.categoryModifiers;
			lock (obj)
			{
				if (!this.categoryModifiers.ContainsKey(category))
				{
					this.categoryModifiers.Add(category, new SoundManager.CategoryModifier(0, 1f, muffle));
				}
				else
				{
					this.categoryModifiers[category].Muffle = muffle;
				}
			}
			for (int i = 0; i < this.playingChannels.Length; i++)
			{
				SoundChannel[] obj2 = this.playingChannels[i];
				lock (obj2)
				{
					for (int j = 0; j < this.playingChannels[i].Length; j++)
					{
						if (this.playingChannels[i][j] != null && this.playingChannels[i][j].IsPlaying)
						{
							SoundChannel soundChannel = this.playingChannels[i][j];
							Identifier? identifier;
							Identifier? identifier2;
							if (soundChannel == null)
							{
								identifier = null;
								identifier2 = identifier;
							}
							else
							{
								identifier2 = new Identifier?(soundChannel.Category);
							}
							identifier = identifier2;
							Identifier? identifier3 = new Identifier?(category);
							if (identifier == identifier3)
							{
								this.playingChannels[i][j].Muffled = muffle;
							}
						}
					}
				}
			}
		}

		// Token: 0x060048EE RID: 18670 RVA: 0x0027DFE4 File Offset: 0x0027C1E4
		public bool GetCategoryMuffle(Identifier category)
		{
			if (this.Disabled)
			{
				return false;
			}
			Dictionary<Identifier, SoundManager.CategoryModifier> obj = this.categoryModifiers;
			bool result;
			lock (obj)
			{
				SoundManager.CategoryModifier categoryModifier;
				if (this.categoryModifiers == null || !this.categoryModifiers.TryGetValue(category, out categoryModifier))
				{
					result = false;
				}
				else
				{
					result = categoryModifier.Muffle;
				}
			}
			return result;
		}

		// Token: 0x060048EF RID: 18671 RVA: 0x0027E04C File Offset: 0x0027C24C
		public void Update()
		{
			if (this.Disconnected || this.Disabled)
			{
				return;
			}
			if (this.CanDetectDisconnect)
			{
				int isConnected;
				Alc.GetInteger(this.alcDevice, 787, out isConnected);
				int alcError = Alc.GetError(this.alcDevice);
				if (alcError != 0)
				{
					throw new Exception("Failed to determine if device is connected: " + alcError.ToString());
				}
				if (isConnected == 0)
				{
					if (!GameMain.Instance.HasLoaded)
					{
						return;
					}
					DebugConsole.ThrowError("Playback device has been disconnected. You can select another available device in the settings.", null, null, false, false);
					SoundManager.SetAudioOutputDevice("<disconnected>");
					this.Disconnected = true;
					SoundManager.TryRefreshDevice();
					return;
				}
			}
			if (GameMain.Client != null && GameSettings.CurrentConfig.Audio.VoipAttenuationEnabled)
			{
				if (Timing.TotalTime > this.lastAttenuationTime + 0.2)
				{
					this.voipAttenuatedGain = this.voipAttenuatedGain * 0.9f + 0.1f;
				}
			}
			else
			{
				this.voipAttenuatedGain = 1f;
			}
			this.SetCategoryGainMultiplier(SoundManager.SoundCategoryDefault, this.VoipAttenuatedGain, 1);
			this.SetCategoryGainMultiplier(SoundManager.SoundCategoryUi, this.VoipAttenuatedGain, 1);
			this.SetCategoryGainMultiplier(SoundManager.SoundCategoryWaterAmbience, this.VoipAttenuatedGain, 1);
			this.SetCategoryGainMultiplier(SoundManager.SoundCategoryMusic, this.VoipAttenuatedGain, 1);
			if (GameSettings.CurrentConfig.Audio.DynamicRangeCompressionEnabled)
			{
				float targetGain = (Math.Min(1f, 1f / this.PlaybackAmplitude) - 1f) * 0.5f + 1f;
				if (targetGain < this.CompressionDynamicRangeGain)
				{
					this.CompressionDynamicRangeGain = targetGain;
				}
				else
				{
					this.CompressionDynamicRangeGain = targetGain * 0.05f + this.CompressionDynamicRangeGain * 0.95f;
				}
			}
			else
			{
				this.CompressionDynamicRangeGain = 1f;
			}
			if (this.updateChannelsThread == null || this.updateChannelsThread.ThreadState.HasFlag(ThreadState.Stopped))
			{
				bool startedStreamThread = false;
				for (int i = 0; i < this.playingChannels.Length; i++)
				{
					SoundChannel[] obj = this.playingChannels[i];
					lock (obj)
					{
						for (int j = 0; j < this.playingChannels[i].Length; j++)
						{
							if (this.playingChannels[i][j] != null)
							{
								if (this.playingChannels[i][j].IsStream && this.playingChannels[i][j].IsPlaying)
								{
									this.InitUpdateChannelThread();
									startedStreamThread = true;
								}
								if (startedStreamThread)
								{
									break;
								}
							}
						}
					}
					if (startedStreamThread)
					{
						break;
					}
				}
			}
		}

		// Token: 0x060048F0 RID: 18672 RVA: 0x0027E2CC File Offset: 0x0027C4CC
		public void ApplySettings()
		{
			this.SetCategoryGainMultiplier(SoundManager.SoundCategoryDefault, GameSettings.CurrentConfig.Audio.SoundVolume, 0);
			this.SetCategoryGainMultiplier(SoundManager.SoundCategoryUi, GameSettings.CurrentConfig.Audio.UiVolume, 0);
			this.SetCategoryGainMultiplier(SoundManager.SoundCategoryWaterAmbience, GameSettings.CurrentConfig.Audio.SoundVolume, 0);
			this.SetCategoryGainMultiplier(SoundManager.SoundCategoryMusic, GameSettings.CurrentConfig.Audio.MusicVolume, 0);
			this.SetCategoryGainMultiplier(SoundManager.SoundCategoryVoip, Math.Min(GameSettings.CurrentConfig.Audio.VoiceChatVolume, 1f), 0);
		}

		// Token: 0x060048F1 RID: 18673 RVA: 0x0027E36C File Offset: 0x0027C56C
		public void InitUpdateChannelThread()
		{
			if (this.Disabled)
			{
				return;
			}
			object obj = this.threadDeathMutex;
			bool isUpdateChannelsThreadDying;
			lock (obj)
			{
				isUpdateChannelsThreadDying = !this.needsUpdateChannels;
			}
			if (this.updateChannelsThread == null || this.updateChannelsThread.ThreadState.HasFlag(ThreadState.Stopped) || isUpdateChannelsThreadDying)
			{
				if (this.updateChannelsThread != null && !this.updateChannelsThread.Join(1000))
				{
					DebugConsole.ThrowError("SoundManager.UpdateChannels thread join timed out!", null, null, false, false);
				}
				this.needsUpdateChannels = true;
				this.updateChannelsThread = new Thread(new ThreadStart(this.UpdateChannels))
				{
					Name = "SoundManager.UpdateChannels Thread",
					IsBackground = true
				};
				this.updateChannelsThread.Start();
			}
		}

		// Token: 0x060048F2 RID: 18674 RVA: 0x0027E448 File Offset: 0x0027C648
		private void UpdateChannels()
		{
			this.updateChannelsMre = new ManualResetEvent(false);
			bool killThread = false;
			while (!killThread)
			{
				killThread = true;
				for (int sourcePoolIndex = 0; sourcePoolIndex < this.playingChannels.Length; sourcePoolIndex++)
				{
					SoundChannel[] obj = this.playingChannels[sourcePoolIndex];
					lock (obj)
					{
						for (int channelIndex = 0; channelIndex < this.playingChannels[sourcePoolIndex].Length; channelIndex++)
						{
							SoundChannel channel = this.playingChannels[sourcePoolIndex][channelIndex];
							if (channel != null)
							{
								if (channel.FadingOutAndDisposing)
								{
									killThread = false;
									channel.Gain -= 0.1f;
									if (channel.Gain <= 0f)
									{
										channel.Dispose();
										this.playingChannels[sourcePoolIndex][channelIndex] = null;
									}
								}
								else if (channel.IsStream)
								{
									if (channel.IsPlaying)
									{
										killThread = false;
										channel.UpdateStream();
									}
									else
									{
										channel.Dispose();
										this.playingChannels[sourcePoolIndex][channelIndex] = null;
									}
								}
							}
						}
					}
				}
				this.updateChannelsMre.WaitOne(10);
				this.updateChannelsMre.Reset();
				object obj2 = this.threadDeathMutex;
				lock (obj2)
				{
					this.needsUpdateChannels = !killThread;
				}
			}
		}

		// Token: 0x060048F3 RID: 18675 RVA: 0x0027E5AC File Offset: 0x0027C7AC
		public void ForceStreamUpdate()
		{
			ManualResetEvent manualResetEvent = this.updateChannelsMre;
			if (manualResetEvent == null)
			{
				return;
			}
			manualResetEvent.Set();
		}

		// Token: 0x060048F4 RID: 18676 RVA: 0x0027E5C0 File Offset: 0x0027C7C0
		private void ReloadSounds()
		{
			for (int i = this.loadedSounds.Count - 1; i >= 0; i--)
			{
				this.loadedSounds[i].InitializeAlBuffers();
			}
		}

		// Token: 0x060048F5 RID: 18677 RVA: 0x0027E5F8 File Offset: 0x0027C7F8
		private void ReleaseResources(bool keepSounds)
		{
			for (int i = 0; i < this.playingChannels.Length; i++)
			{
				SoundChannel[] obj = this.playingChannels[i];
				lock (obj)
				{
					for (int j = 0; j < this.playingChannels[i].Length; j++)
					{
						SoundChannel soundChannel = this.playingChannels[i][j];
						if (soundChannel != null)
						{
							soundChannel.Dispose();
						}
					}
				}
			}
			Thread thread = this.updateChannelsThread;
			if (thread != null)
			{
				thread.Join();
			}
			for (int k = this.loadedSounds.Count - 1; k >= 0; k--)
			{
				if (keepSounds)
				{
					this.loadedSounds[k].DeleteAlBuffers();
				}
				else
				{
					this.loadedSounds[k].Dispose();
				}
			}
			SoundSourcePool soundSourcePool = this.sourcePools[0];
			if (soundSourcePool != null)
			{
				soundSourcePool.Dispose();
			}
			SoundSourcePool soundSourcePool2 = this.sourcePools[1];
			if (soundSourcePool2 != null)
			{
				soundSourcePool2.Dispose();
			}
			SoundBuffers.ClearPool();
		}

		// Token: 0x060048F6 RID: 18678 RVA: 0x0027E6F4 File Offset: 0x0027C8F4
		public void Dispose()
		{
			if (this.Disabled)
			{
				return;
			}
			this.ReleaseResources(false);
			if (!Alc.MakeContextCurrent(IntPtr.Zero) && !GameMain.IsExiting)
			{
				throw new Exception("Failed to detach the current ALC context! (error code: " + Alc.GetError(this.alcDevice).ToString() + ")");
			}
			Alc.DestroyContext(this.alcContext);
			if (!Alc.CloseDevice(this.alcDevice) && !GameMain.IsExiting)
			{
				throw new Exception("Failed to close ALC device!");
			}
		}

		// Token: 0x060048F7 RID: 18679 RVA: 0x0027E778 File Offset: 0x0027C978
		public unsafe static void TryRefreshDevice()
		{
			DebugConsole.NewMessage("Refreshing audio playback device", null, false);
			List<string> deviceList = Alc.GetStringList(IntPtr.Zero, 4115).ToList<string>();
			int alcError = Alc.GetError(IntPtr.Zero);
			if (alcError != 0)
			{
				DebugConsole.ThrowError("Failed to list available audio playback devices: " + alcError.ToString(), null, null, false, false);
				return;
			}
			if (deviceList.Any<string>())
			{
				string availablePreviousDevice = deviceList.Find((string n) => n.Equals(GameSettings.CurrentConfig.Audio.AudioOutputDevice, StringComparison.OrdinalIgnoreCase));
				string device;
				if (availablePreviousDevice != null)
				{
					DebugConsole.NewMessage(" Previous device choice available: " + availablePreviousDevice, null, false);
					device = availablePreviousDevice;
				}
				else
				{
					device = Alc.GetString(IntPtr.Zero, 4100);
					DebugConsole.NewMessage(" Reverting to default device: " + device, null, false);
				}
				if (string.IsNullOrEmpty(device))
				{
					device = deviceList[0];
					DebugConsole.NewMessage(" No default device found, resorting to first available device: " + device, null, false);
				}
				GameSettings.Config currentConfig = *GameSettings.CurrentConfig;
				currentConfig.Audio.AudioOutputDevice = device;
				GameSettings.SetCurrentConfig(currentConfig);
				GameMain.SoundManager.InitializeAlcDevice(device);
			}
			if (GUI.SettingsMenuOpen)
			{
				SettingsMenu instance = SettingsMenu.Instance;
				if (instance == null)
				{
					return;
				}
				instance.CreateAudioAndVCTab(true);
			}
		}

		// Token: 0x040025C1 RID: 9665
		public const int SourceCount = 32;

		// Token: 0x040025C2 RID: 9666
		public static readonly Identifier SoundCategoryDefault = "default".ToIdentifier();

		// Token: 0x040025C3 RID: 9667
		public static readonly Identifier SoundCategoryUi = "ui".ToIdentifier();

		// Token: 0x040025C4 RID: 9668
		public static readonly Identifier SoundCategoryWaterAmbience = "waterambience".ToIdentifier();

		// Token: 0x040025C5 RID: 9669
		public static readonly Identifier SoundCategoryMusic = "music".ToIdentifier();

		// Token: 0x040025C6 RID: 9670
		public static readonly Identifier SoundCategoryVoip = "voip".ToIdentifier();

		// Token: 0x040025C8 RID: 9672
		private IntPtr alcDevice;

		// Token: 0x040025C9 RID: 9673
		private IntPtr alcContext;

		// Token: 0x040025CA RID: 9674
		private readonly SoundSourcePool[] sourcePools;

		// Token: 0x040025CB RID: 9675
		private readonly List<Sound> loadedSounds;

		// Token: 0x040025CC RID: 9676
		private readonly SoundChannel[][] playingChannels = new SoundChannel[2][];

		// Token: 0x040025CD RID: 9677
		private readonly object threadDeathMutex = new object();

		// Token: 0x040025D0 RID: 9680
		private Thread updateChannelsThread;

		// Token: 0x040025D1 RID: 9681
		private Vector3 listenerPosition;

		// Token: 0x040025D2 RID: 9682
		private readonly float[] listenerOrientation = new float[6];

		// Token: 0x040025D3 RID: 9683
		private float listenerGain;

		// Token: 0x040025D5 RID: 9685
		private float voipAttenuatedGain;

		// Token: 0x040025D6 RID: 9686
		private double lastAttenuationTime;

		// Token: 0x040025D7 RID: 9687
		private readonly Dictionary<Identifier, SoundManager.CategoryModifier> categoryModifiers = new Dictionary<Identifier, SoundManager.CategoryModifier>();

		// Token: 0x040025D8 RID: 9688
		private bool needsUpdateChannels;

		// Token: 0x040025D9 RID: 9689
		private ManualResetEvent updateChannelsMre;

		// Token: 0x0200114C RID: 4428
		public enum SourcePoolIndex
		{
			// Token: 0x04005B7C RID: 23420
			Default,
			// Token: 0x04005B7D RID: 23421
			Voice
		}

		// Token: 0x0200114D RID: 4429
		private class CategoryModifier
		{
			// Token: 0x06008FBB RID: 36795 RVA: 0x003BAB3C File Offset: 0x003B8D3C
			public CategoryModifier(int gainMultiplierIndex, float gain, bool muffle)
			{
				this.Muffle = muffle;
				this.GainMultipliers = new float[gainMultiplierIndex + 1];
				for (int i = 0; i < this.GainMultipliers.Length; i++)
				{
					if (i == gainMultiplierIndex)
					{
						this.GainMultipliers[i] = gain;
					}
					else
					{
						this.GainMultipliers[i] = 1f;
					}
				}
			}

			// Token: 0x06008FBC RID: 36796 RVA: 0x003BAB94 File Offset: 0x003B8D94
			public void SetGainMultiplier(int index, float gain)
			{
				if (this.GainMultipliers.Length < index + 1)
				{
					int oldLength = this.GainMultipliers.Length;
					Array.Resize<float>(ref this.GainMultipliers, index + 1);
					for (int i = oldLength; i < this.GainMultipliers.Length; i++)
					{
						this.GainMultipliers[i] = 1f;
					}
				}
				this.GainMultipliers[index] = gain;
			}

			// Token: 0x04005B7E RID: 23422
			public float[] GainMultipliers;

			// Token: 0x04005B7F RID: 23423
			public bool Muffle;
		}
	}
}
