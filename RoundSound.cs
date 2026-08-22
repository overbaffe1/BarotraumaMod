using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using Barotrauma.Sounds;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000E9 RID: 233
	[NullableContext(1)]
	[Nullable(0)]
	internal class RoundSound
	{
		// Token: 0x06002099 RID: 8345 RVA: 0x00146284 File Offset: 0x00144484
		private RoundSound(ContentXElement element, Sound sound)
		{
			this.Filename = sound.Filename;
			this.Sound = sound;
			this.Stream = sound.Stream;
			this.Range = element.GetAttributeFloat("range", 1000f);
			this.Volume = element.GetAttributeFloat("volume", 1f);
			this.IgnoreMuffling = element.GetAttributeBool("dontmuffle", false);
			this.MuteBackgroundMusic = element.GetAttributeBool("MuteBackgroundMusic", false);
			if (!this.Stream)
			{
				double? durationSeconds = this.Sound.DurationSeconds;
				double num = (double)60f;
				if (durationSeconds.GetValueOrDefault() > num & durationSeconds != null)
				{
					DebugConsole.AddWarning("Potential issue in content package: a large audio clip \"" + Path.GetFileName(this.Filename) + "\" is set to be loaded into memory instead of streaming it from the disk. This can lead to excessive memory usage. Large clips should generally be streamed, while small and frequently played sounds should be loaded to memory to avoid the IO overhead of streaming. Consider adding stream=\"true\" to the sound's XML element.", element.ContentPackage);
				}
			}
			this.FrequencyMultiplierRange = new Vector2(1f);
			string freqMultAttr = element.GetAttributeString("frequencymultiplier", element.GetAttributeString("frequency", "1.0"));
			if (!freqMultAttr.Contains(','))
			{
				float freqMult;
				if (float.TryParse(freqMultAttr, NumberStyles.Any, CultureInfo.InvariantCulture, out freqMult))
				{
					this.FrequencyMultiplierRange = new Vector2(freqMult);
				}
			}
			else
			{
				Vector2 freqMult2 = XMLExtensions.ParseVector2(freqMultAttr, false);
				if (freqMult2.Y >= 0.25f)
				{
					this.FrequencyMultiplierRange = freqMult2;
				}
			}
			if (this.FrequencyMultiplierRange.Y > 4f)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(67, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Loaded frequency range exceeds max value: ");
				defaultInterpolatedStringHandler.AppendFormatted<Vector2>(this.FrequencyMultiplierRange);
				defaultInterpolatedStringHandler.AppendLiteral(" (original string was \"");
				defaultInterpolatedStringHandler.AppendFormatted(freqMultAttr);
				defaultInterpolatedStringHandler.AppendLiteral("\")");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
			}
		}

		// Token: 0x0600209A RID: 8346 RVA: 0x0014643B File Offset: 0x0014463B
		public float GetRandomFrequencyMultiplier()
		{
			return Rand.Range(this.FrequencyMultiplierRange.X, this.FrequencyMultiplierRange.Y, Rand.RandSync.Unsynced);
		}

		// Token: 0x0600209B RID: 8347 RVA: 0x0014645C File Offset: 0x0014465C
		[return: Nullable(2)]
		public static RoundSound Load(ContentXElement element)
		{
			SoundManager soundManager = GameMain.SoundManager;
			if (soundManager == null || soundManager.Disabled)
			{
				return null;
			}
			bool stream = element.GetAttributeBool("Stream", false);
			ContentPath filename = element.GetAttributeContentPath("file") ?? element.GetAttributeContentPath("sound");
			if (filename == null)
			{
				string errorMsg = "Error when loading round sound (" + ((element != null) ? element.ToString() : null) + ") - file path not set";
				DebugConsole.ThrowError(errorMsg, null, element.ContentPackage, false, false);
				GameAnalyticsManager.AddErrorEventOnce("RoundSound.LoadRoundSound:FilePathEmpty" + element.ToString(), GameAnalyticsManager.ErrorSeverity.Error, errorMsg + "\n" + Environment.StackTrace.CleanupStackTrace());
				return null;
			}
			Sound existingSound = null;
			RoundSound rs;
			if (RoundSound.roundSoundByPath.TryGetValue(filename.FullPath, out rs))
			{
				Sound sound = rs.Sound;
				if (sound != null && !sound.Disposed)
				{
					existingSound = rs.Sound;
				}
				else
				{
					RoundSound.roundSoundByPath.Remove(filename.FullPath);
				}
			}
			if (existingSound == null)
			{
				try
				{
					existingSound = GameMain.SoundManager.LoadSound((filename != null) ? filename.FullPath : null, stream);
					if (existingSound == null)
					{
						return null;
					}
				}
				catch (FileNotFoundException e)
				{
					string str = "Failed to load sound file \"";
					ContentPath contentPath = filename;
					string errorMsg2 = str + ((contentPath != null) ? contentPath.ToString() : null) + "\" (file not found).";
					DebugConsole.ThrowError(errorMsg2, e, element.ContentPackage, false, false);
					if (!ContentPackageManager.ModsEnabled)
					{
						string str2 = "RoundSound.LoadRoundSound:FileNotFound";
						ContentPath contentPath2 = filename;
						GameAnalyticsManager.AddErrorEventOnce(str2 + ((contentPath2 != null) ? contentPath2.ToString() : null), GameAnalyticsManager.ErrorSeverity.Error, errorMsg2 + "\n" + Environment.StackTrace.CleanupStackTrace());
					}
					return null;
				}
				catch (InvalidDataException e2)
				{
					string str3 = "Failed to load sound file \"";
					ContentPath contentPath3 = filename;
					string errorMsg3 = str3 + ((contentPath3 != null) ? contentPath3.ToString() : null) + "\" (invalid data).";
					DebugConsole.ThrowError(errorMsg3, e2, element.ContentPackage, false, false);
					string str4 = "RoundSound.LoadRoundSound:InvalidData";
					ContentPath contentPath4 = filename;
					GameAnalyticsManager.AddErrorEventOnce(str4 + ((contentPath4 != null) ? contentPath4.ToString() : null), GameAnalyticsManager.ErrorSeverity.Error, errorMsg3 + "\n" + Environment.StackTrace.CleanupStackTrace());
					return null;
				}
			}
			RoundSound newSound = new RoundSound(element, existingSound);
			if (filename != null && !newSound.Stream)
			{
				RoundSound.roundSoundByPath.TryAdd(filename.FullPath, newSound);
			}
			RoundSound.roundSounds.Add(newSound);
			return newSound;
		}

		// Token: 0x0600209C RID: 8348 RVA: 0x001466AC File Offset: 0x001448AC
		public static void Reload(RoundSound roundSound)
		{
			RoundSound roundSound2 = RoundSound.roundSounds.Find(delegate(RoundSound s)
			{
				if (s.Filename == roundSound.Filename && s.Stream == roundSound.Stream)
				{
					Sound sound2 = s.Sound;
					return sound2 != null && !sound2.Disposed;
				}
				return false;
			});
			Sound existingSound = (roundSound2 != null) ? roundSound2.Sound : null;
			if (existingSound == null)
			{
				try
				{
					existingSound = GameMain.SoundManager.LoadSound(roundSound.Filename, roundSound.Stream);
				}
				catch (FileNotFoundException e)
				{
					string errorMsg = "Failed to load sound file \"" + roundSound.Filename + "\".";
					string error = errorMsg;
					Exception e2 = e;
					Sound sound = roundSound.Sound;
					ContentPackage contentPackage;
					if (sound == null)
					{
						contentPackage = null;
					}
					else
					{
						ContentXElement xelement = sound.XElement;
						contentPackage = ((xelement != null) ? xelement.ContentPackage : null);
					}
					DebugConsole.ThrowError(error, e2, contentPackage, false, false);
					GameAnalyticsManager.AddErrorEventOnce("RoundSound.LoadRoundSound:FileNotFound" + roundSound.Filename, GameAnalyticsManager.ErrorSeverity.Error, errorMsg + "\n" + Environment.StackTrace.CleanupStackTrace());
					return;
				}
			}
			roundSound.Sound = existingSound;
		}

		// Token: 0x0600209D RID: 8349 RVA: 0x001467A8 File Offset: 0x001449A8
		public static void RemoveAllRoundSounds()
		{
			foreach (RoundSound roundSound in RoundSound.roundSounds)
			{
				Sound sound = roundSound.Sound;
				if (sound != null)
				{
					sound.Dispose();
				}
			}
			RoundSound.roundSounds.Clear();
			RoundSound.roundSoundByPath.Clear();
		}

		// Token: 0x04001099 RID: 4249
		[Nullable(2)]
		public Sound Sound;

		// Token: 0x0400109A RID: 4250
		public readonly float Volume;

		// Token: 0x0400109B RID: 4251
		public readonly float Range;

		// Token: 0x0400109C RID: 4252
		public readonly Vector2 FrequencyMultiplierRange;

		// Token: 0x0400109D RID: 4253
		public readonly bool Stream;

		// Token: 0x0400109E RID: 4254
		public readonly bool IgnoreMuffling;

		// Token: 0x0400109F RID: 4255
		public int LastStreamSeekPos;

		// Token: 0x040010A0 RID: 4256
		public readonly bool MuteBackgroundMusic;

		// Token: 0x040010A1 RID: 4257
		[Nullable(2)]
		public readonly string Filename;

		// Token: 0x040010A2 RID: 4258
		private static readonly List<RoundSound> roundSounds = new List<RoundSound>();

		// Token: 0x040010A3 RID: 4259
		private static readonly Dictionary<string, RoundSound> roundSoundByPath = new Dictionary<string, RoundSound>();
	}
}
