using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.RuinGeneration;
using Barotrauma.Sounds;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200012F RID: 303
	internal static class SoundPlayer
	{
		// Token: 0x17000A5B RID: 2651
		// (get) Token: 0x0600285A RID: 10330 RVA: 0x001C0AB3 File Offset: 0x001BECB3
		private static IEnumerable<BackgroundMusic> musicClips
		{
			get
			{
				return BackgroundMusic.BackgroundMusicPrefabs;
			}
		}

		// Token: 0x17000A5C RID: 2652
		// (get) Token: 0x0600285B RID: 10331 RVA: 0x001C0ABA File Offset: 0x001BECBA
		private static SoundPrefab waterAmbienceIn
		{
			get
			{
				return SoundPrefab.WaterAmbienceIn.ActivePrefab;
			}
		}

		// Token: 0x17000A5D RID: 2653
		// (get) Token: 0x0600285C RID: 10332 RVA: 0x001C0AC6 File Offset: 0x001BECC6
		private static SoundPrefab waterAmbienceOut
		{
			get
			{
				return SoundPrefab.WaterAmbienceOut.ActivePrefab;
			}
		}

		// Token: 0x17000A5E RID: 2654
		// (get) Token: 0x0600285D RID: 10333 RVA: 0x001C0AD2 File Offset: 0x001BECD2
		private static SoundPrefab waterAmbienceMoving
		{
			get
			{
				return SoundPrefab.WaterAmbienceMoving.ActivePrefab;
			}
		}

		// Token: 0x17000A5F RID: 2655
		// (get) Token: 0x0600285E RID: 10334 RVA: 0x001C0ADE File Offset: 0x001BECDE
		public static IReadOnlyList<SoundPrefab> FlowSounds
		{
			get
			{
				return SoundPrefab.FlowSounds;
			}
		}

		// Token: 0x17000A60 RID: 2656
		// (get) Token: 0x0600285F RID: 10335 RVA: 0x001C0AE5 File Offset: 0x001BECE5
		public static IReadOnlyList<SoundPrefab> SplashSounds
		{
			get
			{
				return SoundPrefab.SplashSounds;
			}
		}

		// Token: 0x17000A61 RID: 2657
		// (get) Token: 0x06002860 RID: 10336 RVA: 0x001C0AEC File Offset: 0x001BECEC
		private static IEnumerable<DamageSound> damageSounds
		{
			get
			{
				return DamageSound.DamageSoundPrefabs;
			}
		}

		// Token: 0x17000A62 RID: 2658
		// (get) Token: 0x06002861 RID: 10337 RVA: 0x001C0AF3 File Offset: 0x001BECF3
		private static Sound startUpSound
		{
			get
			{
				return SoundPrefab.StartupSound.ActivePrefab.Sound;
			}
		}

		// Token: 0x17000A63 RID: 2659
		// (get) Token: 0x06002862 RID: 10338 RVA: 0x001C0B04 File Offset: 0x001BED04
		// (set) Token: 0x06002863 RID: 10339 RVA: 0x001C0B0B File Offset: 0x001BED0B
		public static Identifier OverrideMusicType { get; set; }

		// Token: 0x06002864 RID: 10340 RVA: 0x001C0B14 File Offset: 0x001BED14
		public static void Update(float deltaTime)
		{
			SoundPlayer.UpdateMusic(deltaTime);
			if (SoundPlayer.flowSoundChannels == null || SoundPlayer.flowSoundChannels.Length != SoundPlayer.FlowSounds.Count)
			{
				SoundPlayer.flowSoundChannels = new SoundChannel[SoundPlayer.FlowSounds.Count];
				SoundPlayer.flowVolumeLeft = new float[SoundPlayer.FlowSounds.Count];
				SoundPlayer.flowVolumeRight = new float[SoundPlayer.FlowSounds.Count];
				SoundPlayer.targetFlowLeft = new float[SoundPlayer.FlowSounds.Count];
				SoundPlayer.targetFlowRight = new float[SoundPlayer.FlowSounds.Count];
			}
			if (SoundPlayer.fireSoundChannels == null || SoundPlayer.fireSoundChannels.Length != 3)
			{
				SoundPlayer.fireSoundChannels = new SoundChannel[3];
				SoundPlayer.fireVolumeLeft = new float[3];
				SoundPlayer.fireVolumeRight = new float[3];
			}
			if (Submarine.MainSub == null || Screen.Selected != GameMain.GameScreen)
			{
				foreach (SoundChannel chn in SoundPlayer.waterAmbienceChannels.Concat(SoundPlayer.flowSoundChannels).Concat(SoundPlayer.fireSoundChannels))
				{
					if (chn != null)
					{
						chn.FadeOutAndDispose();
					}
				}
				SoundPlayer.fireVolumeLeft[0] = 0f;
				SoundPlayer.fireVolumeLeft[1] = 0f;
				SoundPlayer.fireVolumeRight[0] = 0f;
				SoundPlayer.fireVolumeRight[1] = 0f;
				SoundChannel soundChannel = SoundPlayer.hullSoundChannel;
				if (soundChannel != null)
				{
					soundChannel.FadeOutAndDispose();
				}
				SoundPlayer.hullSoundSource = null;
				return;
			}
			float ambienceVolume = 0.8f;
			if (Character.Controlled != null && !Character.Controlled.Removed)
			{
				AnimController animController = Character.Controlled.AnimController;
				if (animController.HeadInWater)
				{
					ambienceVolume = 1f;
					float limbSpeed = animController.Limbs[0].LinearVelocity.Length();
					if (MathUtils.IsValid(limbSpeed))
					{
						ambienceVolume += limbSpeed;
					}
				}
			}
			SoundPlayer.UpdateWaterAmbience(ambienceVolume, deltaTime);
			SoundPlayer.UpdateWaterFlowSounds(deltaTime);
			SoundPlayer.UpdateRandomAmbience(deltaTime);
			SoundPlayer.UpdateHullSounds(deltaTime);
			SoundPlayer.UpdateFireSounds(deltaTime);
		}

		// Token: 0x06002865 RID: 10341 RVA: 0x001C0D00 File Offset: 0x001BEF00
		private static void UpdateWaterAmbience(float ambienceVolume, float deltaTime)
		{
			SoundPlayer.<>c__DisplayClass54_0 CS$<>8__locals1;
			CS$<>8__locals1.deltaTime = deltaTime;
			if (!GameMain.SoundManager.Disabled)
			{
				GameScreen gameScreen = GameMain.GameScreen;
				if (((gameScreen != null) ? gameScreen.Cam : null) != null)
				{
					float movementSoundVolume = 0f;
					float insideSubFactor = 0f;
					foreach (Submarine sub in Submarine.Loaded)
					{
						if (sub != null && !sub.Removed)
						{
							float movementFactor = (sub.Velocity == Vector2.Zero) ? 0f : (sub.Velocity.Length() / 10f);
							movementFactor = MathHelper.Clamp(movementFactor, 0f, 1f);
							if (Character.Controlled == null || Character.Controlled.Submarine != sub)
							{
								float dist = Vector2.Distance(GameMain.GameScreen.Cam.WorldViewCenter, sub.WorldPosition);
								movementFactor /= Math.Max(dist / 1000f, 1f);
								insideSubFactor = Math.Max(1f / Math.Max(dist / 1000f, 1f), insideSubFactor);
							}
							else
							{
								insideSubFactor = 1f;
							}
							if (Character.Controlled != null && Character.Controlled.PressureTimer > 0f && !Character.Controlled.IsDead)
							{
								insideSubFactor -= Character.Controlled.PressureTimer / 100f;
							}
							movementSoundVolume = Math.Max(movementSoundVolume, movementFactor);
							if (!MathUtils.IsValid(movementSoundVolume))
							{
								string errorMsg = string.Concat(new string[]
								{
									"Failed to update water ambience volume - submarine's movement value invalid (",
									movementSoundVolume.ToString(),
									", sub velocity: ",
									sub.Velocity.ToString(),
									")"
								});
								DebugConsole.Log(errorMsg);
								GameAnalyticsManager.AddErrorEventOnce("SoundPlayer.UpdateWaterAmbience:InvalidVolume", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
								movementSoundVolume = 0f;
							}
							if (!MathUtils.IsValid(insideSubFactor))
							{
								string errorMsg2 = "Failed to update water ambience volume - inside sub value invalid (" + insideSubFactor.ToString() + ")";
								DebugConsole.Log(errorMsg2);
								GameAnalyticsManager.AddErrorEventOnce("SoundPlayer.UpdateWaterAmbience:InvalidVolume", GameAnalyticsManager.ErrorSeverity.Error, errorMsg2);
								insideSubFactor = 0f;
							}
						}
					}
					SoundPlayer.<UpdateWaterAmbience>g__updateWaterAmbience|54_0(SoundPlayer.waterAmbienceIn.Sound, ambienceVolume * (1f - movementSoundVolume) * insideSubFactor * SoundPlayer.waterAmbienceIn.Volume, ref CS$<>8__locals1);
					SoundPlayer.<UpdateWaterAmbience>g__updateWaterAmbience|54_0(SoundPlayer.waterAmbienceMoving.Sound, ambienceVolume * movementSoundVolume * insideSubFactor * SoundPlayer.waterAmbienceMoving.Volume, ref CS$<>8__locals1);
					SoundPlayer.<UpdateWaterAmbience>g__updateWaterAmbience|54_0(SoundPlayer.waterAmbienceOut.Sound, (1f - insideSubFactor) * SoundPlayer.waterAmbienceOut.Volume, ref CS$<>8__locals1);
					return;
				}
			}
		}

		// Token: 0x06002866 RID: 10342 RVA: 0x001C0FAC File Offset: 0x001BF1AC
		private static void UpdateWaterFlowSounds(float deltaTime)
		{
			if (SoundPlayer.FlowSounds.Count == 0)
			{
				return;
			}
			for (int i = 0; i < SoundPlayer.targetFlowLeft.Length; i++)
			{
				SoundPlayer.targetFlowLeft[i] = 0f;
				SoundPlayer.targetFlowRight[i] = 0f;
			}
			Vector2 listenerPos = new Vector2(GameMain.SoundManager.ListenerPosition.X, GameMain.SoundManager.ListenerPosition.Y);
			foreach (Gap gap in Gap.GapList)
			{
				Vector2 diff = gap.WorldPosition - listenerPos;
				if (Math.Abs(diff.X) < 1500f && Math.Abs(diff.Y) < 1500f)
				{
					if (gap.Open >= 0.01f && gap.LerpedFlowForce.LengthSquared() >= 100f)
					{
						float gapFlow = Math.Abs(gap.LerpedFlowForce.X) + Math.Abs(gap.LerpedFlowForce.Y) * 2.5f;
						if (!gap.IsRoomToRoom)
						{
							gapFlow *= 2f;
						}
						if (gapFlow >= 10f)
						{
							if (gap.linkedTo.Count == 2)
							{
								MapEntity mapEntity = gap.linkedTo[0];
								Hull hull1 = mapEntity as Hull;
								if (hull1 != null)
								{
									mapEntity = gap.linkedTo[1];
									Hull hull2 = mapEntity as Hull;
									if (hull2 != null && (hull1.linkedTo.Contains(hull2) || hull1.linkedTo.Any((MapEntity h) => h.linkedTo.Contains(hull1) && h.linkedTo.Contains(hull2)) || hull2.linkedTo.Any((MapEntity h) => h.linkedTo.Contains(hull1) && h.linkedTo.Contains(hull2))))
									{
										continue;
									}
								}
							}
							int flowSoundIndex = (int)Math.Floor((double)MathHelper.Clamp(gapFlow / 400f, 0f, (float)SoundPlayer.FlowSounds.Count));
							flowSoundIndex = Math.Min(flowSoundIndex, SoundPlayer.FlowSounds.Count - 1);
							float dist = diff.Length();
							float distFallOff = dist / 1500f;
							if (distFallOff < 0.99f)
							{
								float gain = MathHelper.Clamp(gapFlow / 100f, 0f, 1f);
								if (diff.X < 0f)
								{
									SoundPlayer.targetFlowLeft[flowSoundIndex] += gain - distFallOff;
								}
								else
								{
									SoundPlayer.targetFlowRight[flowSoundIndex] += gain - distFallOff;
								}
							}
						}
					}
				}
			}
			Character controlled = Character.Controlled;
			object obj;
			if (controlled == null)
			{
				obj = null;
			}
			else
			{
				CharacterHealth characterHealth = controlled.CharacterHealth;
				obj = ((characterHealth != null) ? characterHealth.GetAffliction("psychosis", true) : null);
			}
			AfflictionPsychosis psychosis = obj as AfflictionPsychosis;
			if (psychosis != null)
			{
				if (psychosis.CurrentFloodType == AfflictionPsychosis.FloodType.Minor)
				{
					SoundPlayer.targetFlowLeft[0] = Math.Max(SoundPlayer.targetFlowLeft[0], 1f);
					SoundPlayer.targetFlowRight[0] = Math.Max(SoundPlayer.targetFlowRight[0], 1f);
				}
				else if (psychosis.CurrentFloodType == AfflictionPsychosis.FloodType.Major)
				{
					SoundPlayer.targetFlowLeft[SoundPlayer.FlowSounds.Count - 1] = Math.Max(SoundPlayer.targetFlowLeft[SoundPlayer.FlowSounds.Count - 1], 1f);
					SoundPlayer.targetFlowRight[SoundPlayer.FlowSounds.Count - 1] = Math.Max(SoundPlayer.targetFlowRight[SoundPlayer.FlowSounds.Count - 1], 1f);
				}
			}
			for (int j = 0; j < SoundPlayer.FlowSounds.Count; j++)
			{
				SoundPlayer.flowVolumeLeft[j] = ((SoundPlayer.targetFlowLeft[j] < SoundPlayer.flowVolumeLeft[j]) ? Math.Max(SoundPlayer.targetFlowLeft[j], SoundPlayer.flowVolumeLeft[j] - deltaTime) : Math.Min(SoundPlayer.targetFlowLeft[j], SoundPlayer.flowVolumeLeft[j] + deltaTime * 10f));
				SoundPlayer.flowVolumeRight[j] = ((SoundPlayer.targetFlowRight[j] < SoundPlayer.flowVolumeRight[j]) ? Math.Max(SoundPlayer.targetFlowRight[j], SoundPlayer.flowVolumeRight[j] - deltaTime) : Math.Min(SoundPlayer.targetFlowRight[j], SoundPlayer.flowVolumeRight[j] + deltaTime * 10f));
				if (SoundPlayer.flowVolumeLeft[j] < 0.05f && SoundPlayer.flowVolumeRight[j] < 0.05f)
				{
					if (SoundPlayer.flowSoundChannels[j] != null)
					{
						SoundPlayer.flowSoundChannels[j].Dispose();
						SoundPlayer.flowSoundChannels[j] = null;
					}
				}
				else
				{
					SoundPrefab soundPrefab = SoundPlayer.FlowSounds[j];
					if (((soundPrefab != null) ? soundPrefab.Sound : null) != null)
					{
						Vector2 soundPos = new Vector2(GameMain.SoundManager.ListenerPosition.X + (SoundPlayer.flowVolumeRight[j] - SoundPlayer.flowVolumeLeft[j]) * 100f, GameMain.SoundManager.ListenerPosition.Y);
						if (SoundPlayer.flowSoundChannels[j] == null || !SoundPlayer.flowSoundChannels[j].IsPlaying)
						{
							SoundPlayer.flowSoundChannels[j] = SoundPlayer.FlowSounds[j].Sound.Play(1f, 1500f, soundPos, false);
							if (SoundPlayer.flowSoundChannels[j] == null)
							{
								goto IL_569;
							}
							SoundPlayer.flowSoundChannels[j].Looping = true;
						}
						SoundPlayer.flowSoundChannels[j].Gain = Math.Max(SoundPlayer.flowVolumeRight[j], SoundPlayer.flowVolumeLeft[j]);
						SoundPlayer.flowSoundChannels[j].Position = new Vector3?(new Vector3(soundPos, 0f));
					}
				}
				IL_569:;
			}
		}

		// Token: 0x06002867 RID: 10343 RVA: 0x001C1558 File Offset: 0x001BF758
		private static void UpdateFireSounds(float deltaTime)
		{
			for (int i = 0; i < SoundPlayer.fireVolumeLeft.Length; i++)
			{
				SoundPlayer.fireVolumeLeft[i] = 0f;
				SoundPlayer.fireVolumeRight[i] = 0f;
			}
			SoundPlayer.<>c__DisplayClass56_0 CS$<>8__locals1;
			CS$<>8__locals1.listenerPos = new Vector2(GameMain.SoundManager.ListenerPosition.X, GameMain.SoundManager.ListenerPosition.Y);
			foreach (Hull hull in Hull.HullList)
			{
				foreach (FireSource fs in hull.FireSources)
				{
					SoundPlayer.<UpdateFireSounds>g__AddFireVolume|56_0(fs, ref CS$<>8__locals1);
				}
				foreach (FireSource fs2 in hull.FakeFireSources)
				{
					SoundPlayer.<UpdateFireSounds>g__AddFireVolume|56_0(fs2, ref CS$<>8__locals1);
				}
			}
			for (int j = 0; j < SoundPlayer.fireVolumeLeft.Length; j++)
			{
				if (SoundPlayer.fireVolumeLeft[j] < 0.05f && SoundPlayer.fireVolumeRight[j] < 0.05f)
				{
					if (SoundPlayer.fireSoundChannels[j] != null)
					{
						SoundPlayer.fireSoundChannels[j].FadeOutAndDispose();
						SoundPlayer.fireSoundChannels[j] = null;
					}
				}
				else
				{
					Vector2 soundPos = new Vector2(GameMain.SoundManager.ListenerPosition.X + (SoundPlayer.fireVolumeRight[j] - SoundPlayer.fireVolumeLeft[j]) * 100f, GameMain.SoundManager.ListenerPosition.Y);
					if (SoundPlayer.fireSoundChannels[j] == null || !SoundPlayer.fireSoundChannels[j].IsPlaying)
					{
						SoundChannel[] array = SoundPlayer.fireSoundChannels;
						int num = j;
						Sound sound = SoundPlayer.GetSound(SoundPlayer.fireSoundTags[j]);
						array[num] = ((sound != null) ? sound.Play(1f, 1500f, soundPos, false) : null);
						if (SoundPlayer.fireSoundChannels[j] == null)
						{
							goto IL_223;
						}
						SoundPlayer.fireSoundChannels[j].Looping = true;
					}
					SoundPlayer.fireSoundChannels[j].Gain = Math.Max(SoundPlayer.fireVolumeRight[j], SoundPlayer.fireVolumeLeft[j]);
					SoundPlayer.fireSoundChannels[j].Position = new Vector3?(new Vector3(soundPos, 0f));
				}
				IL_223:;
			}
		}

		// Token: 0x06002868 RID: 10344 RVA: 0x001C17C4 File Offset: 0x001BF9C4
		private static void UpdateRandomAmbience(float deltaTime)
		{
			if (SoundPlayer.ambientSoundTimer > 0f)
			{
				SoundPlayer.ambientSoundTimer -= deltaTime;
				return;
			}
			SoundPlayer.PlaySound("ambient", new Vector2(GameMain.SoundManager.ListenerPosition.X, GameMain.SoundManager.ListenerPosition.Y) + Rand.Vector(100f, Rand.RandSync.Unsynced), new float?(Rand.Range(0.5f, 1f, Rand.RandSync.Unsynced)), new float?(1000f), null);
			SoundPlayer.ambientSoundTimer = Rand.Range(SoundPlayer.ambientSoundInterval.X, SoundPlayer.ambientSoundInterval.Y, Rand.RandSync.Unsynced);
		}

		// Token: 0x06002869 RID: 10345 RVA: 0x001C1868 File Offset: 0x001BFA68
		private static void UpdateHullSounds(float deltaTime)
		{
			if (SoundPlayer.hullSoundChannel != null && SoundPlayer.hullSoundChannel.IsPlaying && SoundPlayer.hullSoundSource != null)
			{
				SoundPlayer.hullSoundChannel.Position = new Vector3?(new Vector3(SoundPlayer.hullSoundSource.WorldPosition, 0f));
				SoundPlayer.hullSoundChannel.Gain = SoundPlayer.<UpdateHullSounds>g__GetHullSoundVolume|58_0(SoundPlayer.hullSoundSource.Submarine);
			}
			if (SoundPlayer.hullSoundTimer > 0f)
			{
				SoundPlayer.hullSoundTimer -= deltaTime;
				return;
			}
			if (!Level.IsLoadedFriendlyOutpost)
			{
				Character controlled = Character.Controlled;
				Submarine submarine;
				if (controlled == null)
				{
					submarine = null;
				}
				else
				{
					Hull currentHull = controlled.CurrentHull;
					submarine = ((currentHull != null) ? currentHull.Submarine : null);
				}
				Submarine sub = submarine;
				if (sub != null && sub.Info != null && !sub.Info.IsOutpost)
				{
					SoundPlayer.hullSoundSource = Character.Controlled.CurrentHull;
					SoundPlayer.hullSoundChannel = SoundPlayer.PlaySound("hull", SoundPlayer.hullSoundSource.WorldPosition, new float?(SoundPlayer.<UpdateHullSounds>g__GetHullSoundVolume|58_0(sub)), new float?(1500f), null);
					SoundPlayer.hullSoundTimer = Rand.Range(SoundPlayer.hullSoundInterval.X, SoundPlayer.hullSoundInterval.Y, Rand.RandSync.Unsynced);
					return;
				}
			}
			SoundPlayer.hullSoundTimer = 5f;
		}

		// Token: 0x0600286A RID: 10346 RVA: 0x001C198C File Offset: 0x001BFB8C
		public static Sound GetSound(string soundTag)
		{
			IEnumerable<SoundPrefab> matchingSounds = from p in SoundPrefab.Prefabs
			where p.ElementName == soundTag
			select p;
			if (!matchingSounds.Any<SoundPrefab>())
			{
				return null;
			}
			return matchingSounds.GetRandomUnsynced<SoundPrefab>().Sound;
		}

		// Token: 0x0600286B RID: 10347 RVA: 0x001C19D4 File Offset: 0x001BFBD4
		public static SoundChannel PlaySound(string soundTag, float volume = 1f)
		{
			Sound sound = SoundPlayer.GetSound(soundTag);
			if (sound == null)
			{
				return null;
			}
			return sound.Play(volume);
		}

		// Token: 0x0600286C RID: 10348 RVA: 0x001C19F4 File Offset: 0x001BFBF4
		public static SoundChannel PlaySound(string soundTag, Vector2 position, float? volume = null, float? range = null, Hull hullGuess = null)
		{
			Sound sound = SoundPlayer.GetSound(soundTag);
			if (sound == null)
			{
				return null;
			}
			return SoundPlayer.PlaySound(sound, position, new float?(volume ?? sound.BaseGain), new float?(range ?? sound.BaseFar), new float?(1f), hullGuess, false, false);
		}

		// Token: 0x0600286D RID: 10349 RVA: 0x001C1A60 File Offset: 0x001BFC60
		public static SoundChannel PlaySound(RoundSound sound, Vector2 position, float? volume = null, Hull hullGuess = null)
		{
			Sound sound2 = sound.Sound;
			float? volume2 = new float?(volume ?? sound.Volume);
			float? range = new float?(sound.Range);
			bool ignoreMuffling = sound.IgnoreMuffling;
			return SoundPlayer.PlaySound(sound2, position, volume2, range, new float?(sound.GetRandomFrequencyMultiplier()), hullGuess, ignoreMuffling, sound.MuteBackgroundMusic);
		}

		// Token: 0x0600286E RID: 10350 RVA: 0x001C1AC0 File Offset: 0x001BFCC0
		public static SoundChannel PlaySound(Sound sound, Vector2 position, float? volume = null, float? range = null, float? freqMult = null, Hull hullGuess = null, bool ignoreMuffling = false, bool muteBackgroundMusic = false)
		{
			if (sound == null)
			{
				string errorMsg = "Error in SoundPlayer.PlaySound (sound was null)\n" + Environment.StackTrace.CleanupStackTrace();
				GameAnalyticsManager.AddErrorEventOnce("SoundPlayer.PlaySound:SoundNull" + Environment.StackTrace.CleanupStackTrace(), GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return null;
			}
			float far = range ?? sound.BaseFar;
			if (Vector2.DistanceSquared(new Vector2(GameMain.SoundManager.ListenerPosition.X, GameMain.SoundManager.ListenerPosition.Y), position) > far * far)
			{
				return null;
			}
			bool muffle = !ignoreMuffling && SoundPlayer.ShouldMuffleSound(Character.Controlled, position, far, hullGuess);
			SoundChannel channel = sound.Play(volume ?? sound.BaseGain, far, freqMult.GetValueOrDefault(1f), position, muffle);
			if (channel != null)
			{
				channel.MuteBackgroundMusic = muteBackgroundMusic;
			}
			return channel;
		}

		// Token: 0x0600286F RID: 10351 RVA: 0x001C1BA4 File Offset: 0x001BFDA4
		public static void DisposeDisabledMusic()
		{
			bool musicDisposed = false;
			for (int i = 0; i < SoundPlayer.currentMusic.Length; i++)
			{
				BackgroundMusic music = SoundPlayer.currentMusic[i];
				if (music != null && !SoundPrefab.Prefabs.Contains(music))
				{
					SoundPlayer.musicChannel[i].Dispose();
					musicDisposed = true;
					SoundPlayer.currentMusic[i] = null;
				}
			}
			for (int j = 0; j < SoundPlayer.targetMusic.Length; j++)
			{
				BackgroundMusic music2 = SoundPlayer.targetMusic[j];
				if (music2 != null && !SoundPrefab.Prefabs.Contains(music2))
				{
					SoundPlayer.targetMusic[j] = null;
				}
			}
			if (musicDisposed)
			{
				Thread.Sleep(60);
			}
		}

		// Token: 0x06002870 RID: 10352 RVA: 0x001C1C32 File Offset: 0x001BFE32
		public static void ForceMusicUpdate()
		{
			SoundPlayer.updateMusicTimer = 0f;
		}

		// Token: 0x06002871 RID: 10353 RVA: 0x001C1C40 File Offset: 0x001BFE40
		private static void UpdateMusic(float deltaTime)
		{
			if (SoundPlayer.musicClips != null)
			{
				SoundManager soundManager = GameMain.SoundManager;
				if (soundManager != null && !soundManager.Disabled)
				{
					Identifier overrideMusicType = SoundPlayer.OverrideMusicType;
					if (overrideMusicType != null && SoundPlayer.OverrideMusicDuration != null)
					{
						SoundPlayer.OverrideMusicDuration -= deltaTime;
						float? overrideMusicDuration = SoundPlayer.OverrideMusicDuration;
						float num = 0f;
						if (overrideMusicDuration.GetValueOrDefault() <= num & overrideMusicDuration != null)
						{
							SoundPlayer.OverrideMusicType = Identifier.Empty;
							SoundPlayer.OverrideMusicDuration = null;
						}
					}
					int noiseLoopIndex = 1;
					SoundPlayer.updateMusicTimer -= deltaTime;
					int i2;
					int i;
					if (SoundPlayer.updateMusicTimer <= 0f)
					{
						Identifier currentMusicType = SoundPlayer.GetCurrentMusicType();
						GameSession gameSession = GameMain.GameSession;
						float currentIntensity = (((gameSession != null) ? gameSession.EventManager : null) != null) ? (GameMain.GameSession.EventManager.MusicIntensity * 100f) : 0f;
						IEnumerable<BackgroundMusic> suitableMusic = SoundPlayer.GetSuitableMusicClips(currentMusicType, currentIntensity);
						int mainTrackIndex = 0;
						if (suitableMusic.None(null))
						{
							SoundPlayer.targetMusic[mainTrackIndex] = null;
						}
						else if (SoundPlayer.targetMusic[mainTrackIndex] == null || SoundPlayer.currentMusic[mainTrackIndex] == null || !SoundPlayer.currentMusic[mainTrackIndex].IsPlaying() || !suitableMusic.Any((BackgroundMusic m) => m == SoundPlayer.currentMusic[mainTrackIndex]))
						{
							if (currentMusicType == "default")
							{
								if (SoundPlayer.previousDefaultMusic == null)
								{
									SoundPlayer.targetMusic[mainTrackIndex] = (SoundPlayer.previousDefaultMusic = suitableMusic.GetRandomUnsynced<BackgroundMusic>());
								}
								else
								{
									SoundPlayer.targetMusic[mainTrackIndex] = SoundPlayer.previousDefaultMusic;
								}
							}
							else
							{
								SoundPlayer.targetMusic[mainTrackIndex] = suitableMusic.GetRandomUnsynced<BackgroundMusic>();
							}
						}
						if (Level.Loaded != null && (Level.Loaded.Type == LevelData.LevelType.LocationConnection || Level.Loaded.GenerationParams.PlayNoiseLoopInOutpostLevel))
						{
							Identifier biome = Level.Loaded.LevelData.Biome.Identifier;
							if (Level.Loaded.IsEndBiome)
							{
								GameSession gameSession2 = GameMain.GameSession;
								CampaignMode campaign = (gameSession2 != null) ? gameSession2.Campaign : null;
								if (campaign != null && !campaign.Map.EndLocations.Contains(Level.Loaded.StartLocation))
								{
									biome = Level.Loaded.StartLocation.Biome.Identifier;
								}
							}
							IEnumerable<BackgroundMusic> suitableNoiseLoops = (Screen.Selected == GameMain.GameScreen) ? SoundPlayer.GetSuitableMusicClips(biome, currentIntensity) : Enumerable.Empty<BackgroundMusic>();
							if (suitableNoiseLoops.Count<BackgroundMusic>() == 0)
							{
								SoundPlayer.targetMusic[noiseLoopIndex] = null;
							}
							else if (SoundPlayer.targetMusic[noiseLoopIndex] == null || SoundPlayer.currentMusic[noiseLoopIndex] == null || !suitableNoiseLoops.Any((BackgroundMusic m) => m == SoundPlayer.currentMusic[noiseLoopIndex]))
							{
								SoundPlayer.targetMusic[noiseLoopIndex] = suitableNoiseLoops.GetRandomUnsynced<BackgroundMusic>();
							}
						}
						else
						{
							SoundPlayer.targetMusic[noiseLoopIndex] = null;
						}
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(8, 1);
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(currentMusicType);
						defaultInterpolatedStringHandler.AppendLiteral("ambience");
						IEnumerable<BackgroundMusic> suitableTypeAmbiences = SoundPlayer.GetSuitableMusicClips(defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier(), currentIntensity);
						int typeAmbienceTrackIndex = 2;
						if (suitableTypeAmbiences.None(null))
						{
							SoundPlayer.targetMusic[typeAmbienceTrackIndex] = null;
						}
						else if (SoundPlayer.targetMusic[typeAmbienceTrackIndex] == null || SoundPlayer.currentMusic[typeAmbienceTrackIndex] == null || !SoundPlayer.currentMusic[typeAmbienceTrackIndex].IsPlaying() || suitableTypeAmbiences.None((BackgroundMusic m) => m == SoundPlayer.currentMusic[typeAmbienceTrackIndex]))
						{
							SoundPlayer.targetMusic[typeAmbienceTrackIndex] = suitableTypeAmbiences.GetRandomUnsynced<BackgroundMusic>();
						}
						IEnumerable<BackgroundMusic> suitableIntensityMusic = Enumerable.Empty<BackgroundMusic>();
						BackgroundMusic mainTrack = SoundPlayer.targetMusic[mainTrackIndex];
						if ((mainTrack == null || !mainTrack.MuteIntensityTracks) && Screen.Selected == GameMain.GameScreen)
						{
							float intensity = currentIntensity;
							if (mainTrack != null && mainTrack.ForceIntensityTrack != null)
							{
								intensity = mainTrack.ForceIntensityTrack.Value;
							}
							suitableIntensityMusic = SoundPlayer.GetSuitableMusicClips("intensity".ToIdentifier(), intensity);
						}
						int intensityTrackStartIndex = 3;
						int i;
						for (i = intensityTrackStartIndex; i < 6; i = i2 + 1)
						{
							if (SoundPlayer.targetMusic[i] != null && !suitableIntensityMusic.Any((BackgroundMusic m) => m == SoundPlayer.targetMusic[i]))
							{
								SoundPlayer.targetMusic[i] = null;
							}
							i2 = i;
						}
						using (IEnumerator<BackgroundMusic> enumerator = suitableIntensityMusic.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								BackgroundMusic intensityMusic = enumerator.Current;
								if (!SoundPlayer.targetMusic.Any((BackgroundMusic m) => m != null && m == intensityMusic))
								{
									for (int k = intensityTrackStartIndex; k < 6; k++)
									{
										if (SoundPlayer.targetMusic[k] == null)
										{
											SoundPlayer.targetMusic[k] = intensityMusic;
											break;
										}
									}
								}
							}
						}
						SoundPlayer.LogCurrentMusic();
						SoundPlayer.updateMusicTimer = 5f;
						if (mainTrack != null)
						{
							SoundPlayer.updateMusicTimer += mainTrack.MinimumPlayDuration;
						}
					}
					bool muteBackgroundMusic = false;
					for (int j = 0; j < 32; j++)
					{
						SoundChannel playingSoundChannel = GameMain.SoundManager.GetSoundChannelFromIndex(SoundManager.SourcePoolIndex.Default, j);
						if (playingSoundChannel != null && playingSoundChannel.MuteBackgroundMusic && playingSoundChannel.IsPlaying)
						{
							muteBackgroundMusic = true;
							break;
						}
					}
					int activeTrackCount = SoundPlayer.targetMusic.Count((BackgroundMusic m) => m != null);
					for (i = 0; i < 6; i = i2 + 1)
					{
						if (SoundPlayer.targetMusic[i] == null)
						{
							if (SoundPlayer.musicChannel[i] != null && SoundPlayer.musicChannel[i].IsPlaying)
							{
								SoundPlayer.musicChannel[i].Gain = MathHelper.Lerp(SoundPlayer.musicChannel[i].Gain, 0f, 1f * deltaTime);
								if (SoundPlayer.musicChannel[i].Gain < 0.01f)
								{
									SoundPlayer.DisposeMusicChannel(i);
								}
							}
						}
						else if (!SoundPlayer.musicClips.Any((BackgroundMusic mc) => mc == SoundPlayer.targetMusic[i]))
						{
							SoundPlayer.targetMusic[i] = SoundPlayer.GetSuitableMusicClips(SoundPlayer.targetMusic[i].Type, 0f).GetRandomUnsynced<BackgroundMusic>();
						}
						else if (SoundPlayer.currentMusic[i] == null || SoundPlayer.targetMusic[i] != SoundPlayer.currentMusic[i])
						{
							if (SoundPlayer.musicChannel[i] != null && SoundPlayer.musicChannel[i].IsPlaying)
							{
								SoundPlayer.musicChannel[i].Gain = MathHelper.Lerp(SoundPlayer.musicChannel[i].Gain, 0f, 1f * deltaTime);
								if (SoundPlayer.musicChannel[i].Gain < 0.01f)
								{
									SoundPlayer.DisposeMusicChannel(i);
								}
							}
							if (SoundPlayer.currentMusic[i] == null || SoundPlayer.musicChannel[i] == null || !SoundPlayer.musicChannel[i].IsPlaying)
							{
								SoundPlayer.DisposeMusicChannel(i);
								SoundPlayer.currentMusic[i] = SoundPlayer.targetMusic[i];
								SoundPlayer.musicChannel[i] = SoundPlayer.currentMusic[i].Sound.Play(new float?(0f), (i == noiseLoopIndex) ? SoundManager.SoundCategoryDefault : SoundManager.SoundCategoryMusic);
								if (SoundPlayer.targetMusic[i].ContinueFromPreviousTime)
								{
									SoundPlayer.musicChannel[i].StreamSeekPos = SoundPlayer.targetMusic[i].PreviousTime;
								}
								else if (SoundPlayer.targetMusic[i].StartFromRandomTime)
								{
									SoundPlayer.musicChannel[i].StreamSeekPos = (int)((float)SoundPlayer.musicChannel[i].MaxStreamSeekPos * Rand.Range(0f, 1f, Rand.RandSync.Unsynced));
								}
								SoundPlayer.musicChannel[i].Looping = true;
							}
						}
						else
						{
							if (SoundPlayer.musicChannel[i] == null || !SoundPlayer.musicChannel[i].IsPlaying)
							{
								SoundChannel soundChannel = SoundPlayer.musicChannel[i];
								if (soundChannel != null)
								{
									soundChannel.Dispose();
								}
								SoundPlayer.musicChannel[i] = SoundPlayer.currentMusic[i].Sound.Play(new float?(0f), (i == noiseLoopIndex) ? SoundManager.SoundCategoryDefault : SoundManager.SoundCategoryMusic);
								SoundPlayer.musicChannel[i].Looping = true;
							}
							float targetGain = SoundPlayer.targetMusic[i].Volume;
							if (muteBackgroundMusic)
							{
								targetGain = 0f;
							}
							if (SoundPlayer.targetMusic[i].DuckVolume)
							{
								targetGain *= (float)Math.Sqrt((double)(1f / (float)activeTrackCount));
							}
							SoundPlayer.musicChannel[i].Gain = MathHelper.Lerp(SoundPlayer.musicChannel[i].Gain, targetGain, 1f * deltaTime);
						}
						i2 = i;
					}
					return;
				}
			}
		}

		// Token: 0x06002872 RID: 10354 RVA: 0x001C25F8 File Offset: 0x001C07F8
		private static void LogCurrentMusic()
		{
			if (Screen.Selected != GameMain.GameScreen)
			{
				return;
			}
			if (Timing.TotalTime < SoundPlayer.lastMusicLogTime + 60.0)
			{
				return;
			}
			for (int i = 0; i < SoundPlayer.musicChannel.Length; i++)
			{
				if (SoundPlayer.musicChannel[i] != null && SoundPlayer.musicChannel[i].IsPlaying)
				{
					Sound sound = SoundPlayer.musicChannel[i].Sound;
					if (((sound != null) ? sound.Filename : null) != null)
					{
						GameAnalyticsManager.AddDesignEvent("BackgroundMusic:" + Path.GetFileNameWithoutExtension(SoundPlayer.musicChannel[i].Sound.Filename.Replace(":", string.Empty).Replace(" ", string.Empty)));
					}
				}
			}
			SoundPlayer.lastMusicLogTime = Timing.TotalTime;
		}

		// Token: 0x06002873 RID: 10355 RVA: 0x001C26BC File Offset: 0x001C08BC
		private static void DisposeMusicChannel(int index)
		{
			BackgroundMusic clip = SoundPlayer.musicClips.FirstOrDefault(delegate(BackgroundMusic m)
			{
				Sound sound = m.Sound;
				SoundChannel soundChannel2 = SoundPlayer.musicChannel[index];
				return sound == ((soundChannel2 != null) ? soundChannel2.Sound : null);
			});
			if (clip != null && clip.ContinueFromPreviousTime)
			{
				clip.PreviousTime = SoundPlayer.musicChannel[index].StreamSeekPos;
			}
			SoundChannel soundChannel = SoundPlayer.musicChannel[index];
			if (soundChannel != null)
			{
				soundChannel.Dispose();
			}
			SoundPlayer.musicChannel[index] = null;
			SoundPlayer.currentMusic[index] = null;
		}

		// Token: 0x06002874 RID: 10356 RVA: 0x001C2740 File Offset: 0x001C0940
		private static IEnumerable<BackgroundMusic> GetSuitableMusicClips(Identifier musicType, float currentIntensity)
		{
			return from music in SoundPlayer.musicClips
			where SoundPlayer.IsSuitableMusicClip(music, musicType, currentIntensity)
			select music;
		}

		// Token: 0x06002875 RID: 10357 RVA: 0x001C2777 File Offset: 0x001C0977
		private static bool IsSuitableMusicClip(BackgroundMusic music, Identifier musicType, float currentIntensity)
		{
			return music != null && music.Type == musicType && currentIntensity >= music.IntensityRange.X && currentIntensity <= music.IntensityRange.Y;
		}

		// Token: 0x06002876 RID: 10358 RVA: 0x001C27AC File Offset: 0x001C09AC
		private static Identifier GetCurrentMusicType()
		{
			SoundPlayer.<>c__DisplayClass73_0 CS$<>8__locals1 = new SoundPlayer.<>c__DisplayClass73_0();
			Identifier overrideMusicType = SoundPlayer.OverrideMusicType;
			if (overrideMusicType != null)
			{
				return SoundPlayer.OverrideMusicType;
			}
			if (Screen.Selected == null)
			{
				return "menu".ToIdentifier();
			}
			Screen selected = Screen.Selected;
			if (selected == null || !selected.IsEditor)
			{
				GameSession gameSession = GameMain.GameSession;
				if (!(((gameSession != null) ? gameSession.GameMode : null) is TestGameMode) && Screen.Selected != GameMain.NetLobbyScreen)
				{
					if (Screen.Selected != GameMain.GameScreen)
					{
						SoundPlayer.previousDefaultMusic = null;
						return (SoundPlayer.firstTimeInMainMenu ? "menu" : "default").ToIdentifier();
					}
					SoundPlayer.firstTimeInMainMenu = false;
					if (GameMain.GameSession != null)
					{
						foreach (Mission mission in GameMain.GameSession.Missions)
						{
							Identifier missionMusic = mission.GetOverrideMusicType();
							if (!missionMusic.IsEmpty)
							{
								return missionMusic;
							}
						}
					}
					if (Character.Controlled != null)
					{
						if (Level.Loaded != null && Level.Loaded.Ruins != null)
						{
							if (Level.Loaded.Ruins.Any((Ruin r) => r.Area.Contains(Character.Controlled.WorldPosition)))
							{
								return "ruins".ToIdentifier();
							}
						}
						Submarine submarine = Character.Controlled.Submarine;
						bool? flag;
						if (submarine == null)
						{
							flag = null;
						}
						else
						{
							SubmarineInfo info = submarine.Info;
							flag = ((info != null) ? new bool?(info.IsWreck) : null);
						}
						bool? flag2 = flag;
						if (flag2.GetValueOrDefault())
						{
							return "wreck".ToIdentifier();
						}
						if (Level.IsLoadedOutpost)
						{
							Level loaded = Level.Loaded;
							Identifier? identifier;
							if (loaded == null)
							{
								identifier = null;
							}
							else
							{
								Location startLocation = loaded.StartLocation;
								if (startLocation == null)
								{
									identifier = null;
								}
								else
								{
									LocationType type = startLocation.Type;
									identifier = ((type != null) ? new Identifier?(type.Identifier) : null);
								}
							}
							Identifier? locationType = identifier;
							Level loaded2 = Level.Loaded;
							Identifier? identifier2;
							if (loaded2 == null)
							{
								identifier2 = null;
							}
							else
							{
								Location startLocation2 = loaded2.StartLocation;
								if (startLocation2 == null)
								{
									identifier2 = null;
								}
								else
								{
									LocationType type2 = startLocation2.Type;
									identifier2 = ((type2 != null) ? new Identifier?(type2.BackgroundMusicLocationType) : null);
								}
							}
							Identifier? backgroundMusicIdentifier = identifier2;
							Identifier id;
							if (SoundPlayer.<GetCurrentMusicType>g__MatchesTrack|73_1(backgroundMusicIdentifier, out id) || SoundPlayer.<GetCurrentMusicType>g__MatchesTrack|73_1(locationType, out id))
							{
								return id;
							}
						}
					}
					Level loaded3 = Level.Loaded;
					if (loaded3 != null && loaded3.IsEndBiome)
					{
						return "endlevel".ToIdentifier();
					}
					Character controlled = Character.Controlled;
					Submarine targetSubmarine = (controlled != null) ? controlled.Submarine : null;
					SoundPlayer.<>c__DisplayClass73_0 CS$<>8__locals2 = CS$<>8__locals1;
					GameSession gameSession2 = GameMain.GameSession;
					float? num;
					if (gameSession2 == null)
					{
						num = null;
					}
					else
					{
						EventManager eventManager = gameSession2.EventManager;
						num = ((eventManager != null) ? new float?(eventManager.MusicIntensity) : null);
					}
					float? num2 = num;
					CS$<>8__locals2.intensity = num2.GetValueOrDefault() * 100f;
					float enemyDistThreshold = 5000f;
					if (targetSubmarine != null)
					{
						enemyDistThreshold = Math.Max(enemyDistThreshold, (float)Math.Max(targetSubmarine.Borders.Width, targetSubmarine.Borders.Height) * 2f);
					}
					List<Character> monsterMusicCharacters = new List<Character>();
					using (List<Character>.Enumerator enumerator2 = Character.CharacterList.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							Character character = enumerator2.Current;
							if (!character.IsDead && character.Enabled)
							{
								AIController aicontroller = character.AIController;
								EnemyAIController enemyAI = aicontroller as EnemyAIController;
								if (enemyAI != null && aicontroller.Enabled && (enemyAI.AttackHumans || enemyAI.AttackRooms))
								{
									bool specificMonsterMusicAvailable = SoundPlayer.musicClips.Any((BackgroundMusic m) => SoundPlayer.IsSuitableMusicClip(m, character.Params.MusicType, CS$<>8__locals1.intensity));
									if (specificMonsterMusicAvailable)
									{
										float maxDistSqr = MathF.Pow(enemyDistThreshold * character.Params.MusicRangeMultiplier, 2f);
										if (targetSubmarine != null)
										{
											if (Vector2.DistanceSquared(character.WorldPosition, targetSubmarine.WorldPosition) < maxDistSqr)
											{
												monsterMusicCharacters.Add(character);
											}
										}
										else if (Character.Controlled != null && Vector2.DistanceSquared(character.WorldPosition, Character.Controlled.WorldPosition) < maxDistSqr)
										{
											monsterMusicCharacters.Add(character);
										}
									}
								}
							}
						}
					}
					if (monsterMusicCharacters.Any<Character>())
					{
						Character chosenCharacter = monsterMusicCharacters.GetRandomByWeight((Character c) => c.Params.MusicCommonness, Rand.RandSync.Unsynced);
						return chosenCharacter.Params.MusicType;
					}
					if (targetSubmarine != null && targetSubmarine.AtDamageDepth)
					{
						return "deep".ToIdentifier();
					}
					if (GameMain.GameScreen != null && Screen.Selected == GameMain.GameScreen && Submarine.MainSub != null && Level.Loaded != null && Level.Loaded.GetRealWorldDepth(GameMain.GameScreen.Cam.Position.Y) > Submarine.MainSub.RealWorldCrushDepth)
					{
						return "deep".ToIdentifier();
					}
					if (targetSubmarine != null)
					{
						float floodedArea = 0f;
						float totalArea = 0f;
						foreach (Hull hull in Hull.HullList)
						{
							if (hull.Submarine == targetSubmarine)
							{
								floodedArea += hull.WaterVolume;
								totalArea += hull.Volume;
							}
						}
						if (totalArea > 0f && floodedArea / totalArea > 0.25f)
						{
							return "flooded".ToIdentifier();
						}
					}
					if (GameMain.GameSession != null)
					{
						if (Submarine.Loaded != null && Level.Loaded != null && Submarine.MainSub != null && Submarine.MainSub.AtEndExit)
						{
							return "levelend".ToIdentifier();
						}
						if ((double)GameMain.GameSession.RoundDuration < 120.0)
						{
							Level loaded4 = Level.Loaded;
							if (loaded4 != null && loaded4.Type == LevelData.LevelType.LocationConnection)
							{
								return "start".ToIdentifier();
							}
						}
					}
					return "default".ToIdentifier();
				}
			}
			return "editor".ToIdentifier();
		}

		// Token: 0x06002877 RID: 10359 RVA: 0x001C2DFC File Offset: 0x001C0FFC
		public static bool ShouldMuffleSound(Character listener, Vector2 soundWorldPos, float range, Hull hullGuess)
		{
			if (listener == null)
			{
				return false;
			}
			float lowpassHFGain = 1f;
			AnimController animController = listener.AnimController;
			if (animController.HeadInWater)
			{
				lowpassHFGain = 0.2f;
			}
			lowpassHFGain *= Character.Controlled.LowPassMultiplier;
			if (lowpassHFGain < 0.5f)
			{
				return true;
			}
			Hull targetHull = Hull.FindHull(soundWorldPos, hullGuess, true, true);
			if (listener.CurrentHull == null || targetHull == null)
			{
				return listener.CurrentHull != targetHull;
			}
			Vector2 soundPos = soundWorldPos;
			if (targetHull.Submarine != null)
			{
				soundPos += -targetHull.Submarine.WorldPosition + targetHull.Submarine.HiddenSubPosition;
			}
			return listener.CurrentHull.GetApproximateDistance(listener.Position, soundPos, targetHull, range, 0f, 0.5f) > range;
		}

		// Token: 0x06002878 RID: 10360 RVA: 0x001C2EB4 File Offset: 0x001C10B4
		public static void PlaySplashSound(Vector2 worldPosition, float strength)
		{
			if (SoundPlayer.SplashSounds.Count == 0)
			{
				return;
			}
			int splashIndex = MathHelper.Clamp((int)(strength + Rand.Range(-2f, 2f, Rand.RandSync.Unsynced)), 0, SoundPlayer.SplashSounds.Count - 1);
			float range = 800f;
			Sound sound = SoundPlayer.SplashSounds[splashIndex].Sound;
			if (sound == null)
			{
				return;
			}
			sound.Play(1f, range, worldPosition, SoundPlayer.ShouldMuffleSound(Character.Controlled, worldPosition, range, null));
		}

		// Token: 0x06002879 RID: 10361 RVA: 0x001C2F2C File Offset: 0x001C112C
		public static void PlayDamageSound(string damageType, float damage, PhysicsBody body)
		{
			Vector2 bodyPosition = body.DrawPosition;
			SoundPlayer.PlayDamageSound(damageType, damage, bodyPosition, 800f, null, 1f);
		}

		// Token: 0x0600287A RID: 10362 RVA: 0x001C2F54 File Offset: 0x001C1154
		public static void PlayDamageSound(string damageType, float damage, Vector2 position, float range = 2000f, IEnumerable<Identifier> tags = null, float gain = 1f)
		{
			IEnumerable<DamageSound> suitableSounds = SoundPlayer.damageSounds.Where(delegate(DamageSound s)
			{
				if (!(s.DamageType == damageType))
				{
					return false;
				}
				if (s.RequiredTag.IsEmpty)
				{
					return true;
				}
				if (tags != null)
				{
					return tags.Contains(s.RequiredTag);
				}
				return s.RequiredTag.IsEmpty;
			});
			if (suitableSounds.All((DamageSound d) => damage < d.DamageRange.X))
			{
				return;
			}
			float randomizedDamage = MathHelper.Clamp(damage + Rand.Range(-10f, 10f, Rand.RandSync.Unsynced), 0f, 100f);
			suitableSounds = from s in suitableSounds
			where s.DamageRange == Vector2.Zero || (randomizedDamage >= s.DamageRange.X && randomizedDamage <= s.DamageRange.Y)
			select s;
			DamageSound damageSound = suitableSounds.GetRandomUnsynced<DamageSound>();
			if (damageSound != null)
			{
				Sound sound = damageSound.Sound;
				if (sound == null)
				{
					return;
				}
				sound.Play(gain, range, position, !damageSound.IgnoreMuffling && SoundPlayer.ShouldMuffleSound(Character.Controlled, position, range, null));
			}
		}

		// Token: 0x0600287B RID: 10363 RVA: 0x001C3020 File Offset: 0x001C1220
		public static void PlayUISound(GUISoundType soundType)
		{
			GUISound randomUnsynced = (from s in GUISound.GUISoundPrefabs
			where s.Type == soundType
			select s).GetRandomUnsynced<GUISound>();
			if (randomUnsynced == null)
			{
				return;
			}
			Sound sound = randomUnsynced.Sound;
			if (sound == null)
			{
				return;
			}
			sound.Play(null, SoundManager.SoundCategoryUi);
		}

		// Token: 0x0600287C RID: 10364 RVA: 0x001C3078 File Offset: 0x001C1278
		public static void PlayUISound(GUISoundType? soundType)
		{
			if (soundType != null)
			{
				SoundPlayer.PlayUISound(soundType.Value);
			}
		}

		// Token: 0x0600287E RID: 10366 RVA: 0x001C311C File Offset: 0x001C131C
		[CompilerGenerated]
		internal static void <UpdateWaterAmbience>g__updateWaterAmbience|54_0(Sound sound, float volume, ref SoundPlayer.<>c__DisplayClass54_0 A_2)
		{
			SoundChannel chn = SoundPlayer.waterAmbienceChannels.FirstOrDefault((SoundChannel c) => c.Sound == sound);
			if (Level.Loaded != null)
			{
				volume *= Level.Loaded.GenerationParams.WaterAmbienceVolume;
			}
			if (chn == null || !chn.IsPlaying)
			{
				if (volume < 0.01f)
				{
					return;
				}
				if (chn != null)
				{
					SoundPlayer.waterAmbienceChannels.Remove(chn);
				}
				chn = sound.Play(new float?(volume), SoundManager.SoundCategoryWaterAmbience);
				if (chn != null)
				{
					chn.Looping = true;
					SoundPlayer.waterAmbienceChannels.Add(chn);
					return;
				}
			}
			else
			{
				chn.Gain += A_2.deltaTime * (float)Math.Sign(volume - chn.Gain);
				if (chn.Gain < 0.01f)
				{
					chn.FadeOutAndDispose();
				}
				if (Character.Controlled != null && Character.Controlled.PressureTimer > 0f && !Character.Controlled.IsDead)
				{
					chn.FrequencyMultiplier = MathHelper.Clamp(Character.Controlled.PressureTimer / 200f, 0.75f, 1f);
					return;
				}
				chn.FrequencyMultiplier = Math.Min(chn.frequencyMultiplier + A_2.deltaTime, 1f);
			}
		}

		// Token: 0x0600287F RID: 10367 RVA: 0x001C3258 File Offset: 0x001C1458
		[CompilerGenerated]
		internal static void <UpdateFireSounds>g__AddFireVolume|56_0(FireSource fs, ref SoundPlayer.<>c__DisplayClass56_0 A_1)
		{
			Vector2 diff = fs.WorldPosition + fs.Size / 2f - A_1.listenerPos;
			if (Math.Abs(diff.X) < 1000f && Math.Abs(diff.Y) < 1000f)
			{
				Vector2 diffLeft = fs.WorldPosition + new Vector2(fs.Size.X, fs.Size.Y / 2f) - A_1.listenerPos;
				if (Math.Abs(diff.X) < fs.Size.X / 2f)
				{
					diffLeft.X = 0f;
				}
				if (diffLeft.X <= 0f)
				{
					float distFallOffLeft = diffLeft.Length() / 1000f;
					if (distFallOffLeft < 0.99f)
					{
						SoundPlayer.fireVolumeLeft[0] += 1f - distFallOffLeft;
						if (fs.Size.X > 200f)
						{
							SoundPlayer.fireVolumeLeft[2] += (1f - distFallOffLeft) * ((fs.Size.X - 200f) / 200f);
						}
						else if (fs.Size.X > 100f)
						{
							SoundPlayer.fireVolumeLeft[1] += (1f - distFallOffLeft) * ((fs.Size.X - 100f) / 100f);
						}
					}
				}
				Vector2 diffRight = fs.WorldPosition + new Vector2(0f, fs.Size.Y / 2f) - A_1.listenerPos;
				if (Math.Abs(diff.X) < fs.Size.X / 2f)
				{
					diffRight.X = 0f;
				}
				if (diffRight.X >= 0f)
				{
					float distFallOffRight = diffRight.Length() / 1000f;
					if (distFallOffRight < 0.99f)
					{
						SoundPlayer.fireVolumeRight[0] += 1f - distFallOffRight;
						if (fs.Size.X > 200f)
						{
							SoundPlayer.fireVolumeRight[2] += (1f - distFallOffRight) * ((fs.Size.X - 200f) / 200f);
							return;
						}
						if (fs.Size.X > 100f)
						{
							SoundPlayer.fireVolumeRight[1] += (1f - distFallOffRight) * ((fs.Size.X - 100f) / 100f);
						}
					}
				}
			}
		}

		// Token: 0x06002880 RID: 10368 RVA: 0x001C34FC File Offset: 0x001C16FC
		[CompilerGenerated]
		internal static float <UpdateHullSounds>g__GetHullSoundVolume|58_0(Submarine sub)
		{
			float depth = (Level.Loaded == null) ? 0f : (Math.Abs(sub.Position.Y - (float)Level.Loaded.Size.Y) * Physics.DisplayToRealWorldRatio);
			return Math.Clamp((depth - 800f) / 1500f, 0.4f, 1f);
		}

		// Token: 0x06002881 RID: 10369 RVA: 0x001C355C File Offset: 0x001C175C
		[CompilerGenerated]
		internal static bool <GetCurrentMusicType>g__MatchesTrack|73_1(Identifier? identifier, out Identifier idValue)
		{
			if (identifier != null)
			{
				Identifier value = identifier.Value;
				if (value != Identifier.Empty && SoundPlayer.musicClips.Any(delegate(BackgroundMusic clip)
				{
					Identifier value2 = identifier.Value;
					return clip.Type == value2;
				}))
				{
					idValue = identifier.Value;
					return true;
				}
			}
			idValue = Identifier.Empty;
			return false;
		}

		// Token: 0x04001478 RID: 5240
		private const float MusicLerpSpeed = 1f;

		// Token: 0x04001479 RID: 5241
		private const float UpdateMusicInterval = 5f;

		// Token: 0x0400147A RID: 5242
		public const float MuffleFilterFrequency = 600f;

		// Token: 0x0400147B RID: 5243
		private const int MaxMusicChannels = 6;

		// Token: 0x0400147C RID: 5244
		private static readonly BackgroundMusic[] currentMusic = new BackgroundMusic[6];

		// Token: 0x0400147D RID: 5245
		private static readonly SoundChannel[] musicChannel = new SoundChannel[6];

		// Token: 0x0400147E RID: 5246
		private static readonly BackgroundMusic[] targetMusic = new BackgroundMusic[6];

		// Token: 0x0400147F RID: 5247
		private static BackgroundMusic previousDefaultMusic;

		// Token: 0x04001480 RID: 5248
		private static float updateMusicTimer;

		// Token: 0x04001481 RID: 5249
		private static readonly HashSet<SoundChannel> waterAmbienceChannels = new HashSet<SoundChannel>();

		// Token: 0x04001482 RID: 5250
		private static float ambientSoundTimer;

		// Token: 0x04001483 RID: 5251
		private static Vector2 ambientSoundInterval = new Vector2(20f, 40f);

		// Token: 0x04001484 RID: 5252
		private static SoundChannel hullSoundChannel;

		// Token: 0x04001485 RID: 5253
		private static Hull hullSoundSource;

		// Token: 0x04001486 RID: 5254
		private static float hullSoundTimer;

		// Token: 0x04001487 RID: 5255
		private static Vector2 hullSoundInterval = new Vector2(45f, 90f);

		// Token: 0x04001488 RID: 5256
		private static float[] targetFlowLeft;

		// Token: 0x04001489 RID: 5257
		private static float[] targetFlowRight;

		// Token: 0x0400148A RID: 5258
		private static SoundChannel[] flowSoundChannels;

		// Token: 0x0400148B RID: 5259
		private static float[] flowVolumeLeft;

		// Token: 0x0400148C RID: 5260
		private static float[] flowVolumeRight;

		// Token: 0x0400148D RID: 5261
		private const float FlowSoundRange = 1500f;

		// Token: 0x0400148E RID: 5262
		private const float MaxFlowStrength = 400f;

		// Token: 0x0400148F RID: 5263
		private static SoundChannel[] fireSoundChannels;

		// Token: 0x04001490 RID: 5264
		private static float[] fireVolumeLeft;

		// Token: 0x04001491 RID: 5265
		private static float[] fireVolumeRight;

		// Token: 0x04001492 RID: 5266
		private const float FireSoundRange = 1000f;

		// Token: 0x04001493 RID: 5267
		private const float FireSoundMediumLimit = 100f;

		// Token: 0x04001494 RID: 5268
		private const float FireSoundLargeLimit = 200f;

		// Token: 0x04001495 RID: 5269
		private const int fireSizes = 3;

		// Token: 0x04001496 RID: 5270
		private static string[] fireSoundTags = new string[]
		{
			"fire",
			"firemedium",
			"firelarge"
		};

		// Token: 0x04001497 RID: 5271
		private static bool firstTimeInMainMenu = true;

		// Token: 0x04001499 RID: 5273
		public static float? OverrideMusicDuration;

		// Token: 0x0400149A RID: 5274
		private static double lastMusicLogTime;

		// Token: 0x0400149B RID: 5275
		private const double MusicLogInterval = 60.0;
	}
}
