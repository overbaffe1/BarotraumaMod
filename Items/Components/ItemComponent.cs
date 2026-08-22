using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Barotrauma.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005B3 RID: 1459
	internal class ItemComponent : ISerializableEntity
	{
		// Token: 0x1700167A RID: 5754
		// (get) Token: 0x0600599D RID: 22941 RVA: 0x002E1F93 File Offset: 0x002E0193
		public bool HasSounds
		{
			get
			{
				return this.sounds.Count > 0;
			}
		}

		// Token: 0x1700167B RID: 5755
		// (get) Token: 0x0600599E RID: 22942 RVA: 0x002E1FA3 File Offset: 0x002E01A3
		public bool[] HasSoundsOfType
		{
			get
			{
				return this.hasSoundsOfType;
			}
		}

		// Token: 0x1700167C RID: 5756
		// (get) Token: 0x0600599F RID: 22943 RVA: 0x002E1FAB File Offset: 0x002E01AB
		public virtual bool RecreateGUIOnResolutionChange
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700167D RID: 5757
		// (get) Token: 0x060059A0 RID: 22944 RVA: 0x002E1FAE File Offset: 0x002E01AE
		// (set) Token: 0x060059A1 RID: 22945 RVA: 0x002E1FB6 File Offset: 0x002E01B6
		public ItemComponent.GUILayoutSettings DefaultLayout { get; protected set; }

		// Token: 0x1700167E RID: 5758
		// (get) Token: 0x060059A2 RID: 22946 RVA: 0x002E1FBF File Offset: 0x002E01BF
		// (set) Token: 0x060059A3 RID: 22947 RVA: 0x002E1FC7 File Offset: 0x002E01C7
		public ItemComponent.GUILayoutSettings AlternativeLayout { get; protected set; }

		// Token: 0x1700167F RID: 5759
		// (get) Token: 0x060059A4 RID: 22948 RVA: 0x002E1FD0 File Offset: 0x002E01D0
		// (set) Token: 0x060059A5 RID: 22949 RVA: 0x002E1FD8 File Offset: 0x002E01D8
		public GUIFrame GuiFrame { get; set; }

		// Token: 0x17001680 RID: 5760
		// (get) Token: 0x060059A6 RID: 22950 RVA: 0x002E1FE1 File Offset: 0x002E01E1
		// (set) Token: 0x060059A7 RID: 22951 RVA: 0x002E1FE9 File Offset: 0x002E01E9
		public Sprite HUDOverlay { get; set; }

		// Token: 0x17001681 RID: 5761
		// (get) Token: 0x060059A8 RID: 22952 RVA: 0x002E1FF2 File Offset: 0x002E01F2
		// (set) Token: 0x060059A9 RID: 22953 RVA: 0x002E1FFA File Offset: 0x002E01FA
		public float HUDOverlayAnimSpeed { get; set; }

		// Token: 0x17001682 RID: 5762
		// (get) Token: 0x060059AA RID: 22954 RVA: 0x002E2003 File Offset: 0x002E0203
		// (set) Token: 0x060059AB RID: 22955 RVA: 0x002E200B File Offset: 0x002E020B
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool AllowUIOverlap { get; set; }

		// Token: 0x17001683 RID: 5763
		// (get) Token: 0x060059AC RID: 22956 RVA: 0x002E2014 File Offset: 0x002E0214
		// (set) Token: 0x060059AD RID: 22957 RVA: 0x002E201C File Offset: 0x002E021C
		[Serialize(true, IsPropertySaveable.No, "", "", false)]
		public bool CloseByClickingOutsideGUIFrame { get; set; }

		// Token: 0x17001684 RID: 5764
		// (get) Token: 0x060059AE RID: 22958 RVA: 0x002E2025 File Offset: 0x002E0225
		// (set) Token: 0x060059AF RID: 22959 RVA: 0x002E202D File Offset: 0x002E022D
		[Serialize("", IsPropertySaveable.No, "", "", false)]
		public string LinkUIToComponent { get; set; }

		// Token: 0x17001685 RID: 5765
		// (get) Token: 0x060059B0 RID: 22960 RVA: 0x002E2036 File Offset: 0x002E0236
		// (set) Token: 0x060059B1 RID: 22961 RVA: 0x002E203E File Offset: 0x002E023E
		[Serialize(0, IsPropertySaveable.No, "", "", false)]
		public int HudPriority { get; private set; }

		// Token: 0x17001686 RID: 5766
		// (get) Token: 0x060059B2 RID: 22962 RVA: 0x002E2047 File Offset: 0x002E0247
		// (set) Token: 0x060059B3 RID: 22963 RVA: 0x002E204F File Offset: 0x002E024F
		[Serialize(0, IsPropertySaveable.No, "", "", false)]
		public int HudLayer { get; private set; }

		// Token: 0x17001687 RID: 5767
		// (get) Token: 0x060059B4 RID: 22964 RVA: 0x002E2058 File Offset: 0x002E0258
		// (set) Token: 0x060059B5 RID: 22965 RVA: 0x002E2060 File Offset: 0x002E0260
		public bool UseAlternativeLayout
		{
			get
			{
				return this.useAlternativeLayout;
			}
			set
			{
				if (this.AlternativeLayout != null)
				{
					if (value == this.useAlternativeLayout)
					{
						return;
					}
					this.useAlternativeLayout = value;
					if (this.useAlternativeLayout)
					{
						ItemComponent.GUILayoutSettings alternativeLayout = this.AlternativeLayout;
						if (alternativeLayout == null)
						{
							return;
						}
						alternativeLayout.ApplyTo(this.GuiFrame.RectTransform);
						return;
					}
					else
					{
						ItemComponent.GUILayoutSettings defaultLayout = this.DefaultLayout;
						if (defaultLayout == null)
						{
							return;
						}
						defaultLayout.ApplyTo(this.GuiFrame.RectTransform);
					}
				}
			}
		}

		// Token: 0x060059B6 RID: 22966 RVA: 0x002E20C5 File Offset: 0x002E02C5
		public ItemComponent GetReplacementOrThis()
		{
			if (this.ReplacedBy != null && this.ReplacedBy != this)
			{
				return this.ReplacedBy.GetReplacementOrThis();
			}
			return this;
		}

		// Token: 0x060059B7 RID: 22967 RVA: 0x002E20E5 File Offset: 0x002E02E5
		public bool NeedsSoundUpdate()
		{
			return this.hasSoundsOfType[0] || (this.loopingSoundChannel != null && this.loopingSoundChannel.IsPlaying) || this.playingOneshotSoundChannels.Count > 0;
		}

		// Token: 0x060059B8 RID: 22968 RVA: 0x002E211C File Offset: 0x002E031C
		public void UpdateSounds()
		{
			if (this.loopingSound != null && this.loopingSoundChannel != null && this.loopingSoundChannel.IsPlaying)
			{
				if (Timing.TotalTime > (double)(this.lastMuffleCheckTime + 0.2f))
				{
					Character controlled = Character.Controlled;
					Vector2 worldPosition = this.item.WorldPosition;
					float range = this.loopingSound.Range;
					Character controlled2 = Character.Controlled;
					this.shouldMuffleLooping = SoundPlayer.ShouldMuffleSound(controlled, worldPosition, range, (controlled2 != null) ? controlled2.CurrentHull : null);
					this.lastMuffleCheckTime = (float)Timing.TotalTime;
				}
				this.loopingSoundChannel.Muffled = this.shouldMuffleLooping;
				float targetGain = this.GetSoundVolume(this.loopingSound);
				float gainDiff = targetGain - this.loopingSoundChannel.Gain;
				this.loopingSoundChannel.Gain += ((Math.Abs(gainDiff) < 0.1f) ? gainDiff : ((float)Math.Sign(gainDiff) * 0.1f));
				this.loopingSoundChannel.Position = new Vector3?(new Vector3(this.item.WorldPosition, 0f));
				this.loopingSound.RoundSound.LastStreamSeekPos = this.loopingSoundChannel.StreamSeekPos;
			}
			for (int i = 0; i < this.playingOneshotSoundChannels.Count; i++)
			{
				if (!this.playingOneshotSoundChannels[i].IsPlaying)
				{
					this.playingOneshotSoundChannels[i].Dispose();
					this.playingOneshotSoundChannels[i] = null;
				}
			}
			this.playingOneshotSoundChannels.RemoveAll((SoundChannel ch) => ch == null);
			foreach (SoundChannel channel in this.playingOneshotSoundChannels)
			{
				channel.Position = new Vector3?(new Vector3(this.item.WorldPosition, 0f));
			}
		}

		// Token: 0x060059B9 RID: 22969 RVA: 0x002E2314 File Offset: 0x002E0514
		public void PlaySound(ActionType type, Character user = null)
		{
			if (!this.hasSoundsOfType[(int)type])
			{
				return;
			}
			GameClient client = GameMain.Client;
			if (client != null && client.MidRoundSyncing)
			{
				return;
			}
			if (this.item.Submarine != null && this.item.Submarine.IsAboveLevel)
			{
				return;
			}
			if (this.loopingSound == null)
			{
				List<ItemSound> matchingSounds = this.sounds[type];
				if (this.loopingSoundChannel == null || !this.loopingSoundChannel.IsPlaying)
				{
					SoundSelectionMode soundSelectionMode = this.soundSelectionModes[type];
					int index;
					if (soundSelectionMode == SoundSelectionMode.CharacterSpecific && user != null)
					{
						index = (int)user.ID % matchingSounds.Count;
					}
					else if (soundSelectionMode == SoundSelectionMode.ItemSpecific)
					{
						index = (int)this.item.ID % matchingSounds.Count;
					}
					else
					{
						if (soundSelectionMode == SoundSelectionMode.All)
						{
							foreach (ItemSound sound in matchingSounds)
							{
								this.PlaySound(sound, this.item.WorldPosition);
							}
							return;
						}
						if (soundSelectionMode == SoundSelectionMode.Manual)
						{
							index = Math.Clamp(this.ManuallySelectedSound, 0, matchingSounds.Count - 1);
						}
						else
						{
							index = Rand.Int(matchingSounds.Count, Rand.RandSync.Unsynced);
						}
					}
					this.PlaySound(matchingSounds[index], this.item.WorldPosition);
					this.item.CheckNeedsSoundUpdate(this);
				}
				return;
			}
			if (Vector3.DistanceSquared(GameMain.SoundManager.ListenerPosition, new Vector3(this.item.WorldPosition, 0f)) > this.loopingSound.Range * this.loopingSound.Range || this.GetSoundVolume(this.loopingSound) <= 0.0001f)
			{
				if (this.loopingSoundChannel != null)
				{
					this.loopingSoundChannel.FadeOutAndDispose();
					this.loopingSoundChannel = null;
					this.loopingSound = null;
				}
				return;
			}
			if (this.loopingSoundChannel != null && this.loopingSoundChannel.Sound != this.loopingSound.RoundSound.Sound)
			{
				this.loopingSoundChannel.FadeOutAndDispose();
				this.loopingSoundChannel = null;
				this.loopingSound = null;
			}
			if (this.loopingSoundChannel == null || !this.loopingSoundChannel.IsPlaying)
			{
				Sound sound2 = this.loopingSound.RoundSound.Sound;
				Vector3? position = new Vector3?(new Vector3(this.item.WorldPosition, 0f));
				float gain = 0.01f;
				float randomFrequencyMultiplier = this.loopingSound.RoundSound.GetRandomFrequencyMultiplier();
				Character controlled = Character.Controlled;
				Vector2 worldPosition = this.item.WorldPosition;
				float range = this.loopingSound.Range;
				Character controlled2 = Character.Controlled;
				this.loopingSoundChannel = sound2.Play(position, gain, randomFrequencyMultiplier, SoundPlayer.ShouldMuffleSound(controlled, worldPosition, range, (controlled2 != null) ? controlled2.CurrentHull : null));
				if (this.loopingSoundChannel != null)
				{
					this.loopingSoundChannel.Looping = true;
					this.item.CheckNeedsSoundUpdate(this);
					this.loopingSoundChannel.Near = this.loopingSound.Range * 0.4f;
					this.loopingSoundChannel.Far = this.loopingSound.Range;
				}
			}
			if (this.loopingSoundChannel != null && this.loopingSoundChannel.IsPlaying && this.soundSelectionModes[type] == SoundSelectionMode.Manual)
			{
				int playingIndex = this.sounds[type].IndexOf(this.loopingSound);
				int shouldBePlayingIndex = Math.Clamp(this.ManuallySelectedSound, 0, this.sounds[type].Count);
				if (playingIndex != shouldBePlayingIndex)
				{
					this.loopingSoundChannel.FadeOutAndDispose();
					this.loopingSoundChannel = null;
					this.loopingSound = null;
				}
			}
		}

		// Token: 0x060059BA RID: 22970 RVA: 0x002E2684 File Offset: 0x002E0884
		private void PlaySound(ItemSound itemSound, Vector2 position)
		{
			if (Vector2.DistanceSquared(new Vector2(GameMain.SoundManager.ListenerPosition.X, GameMain.SoundManager.ListenerPosition.Y), position) > itemSound.Range * itemSound.Range)
			{
				return;
			}
			if (itemSound.OnlyPlayInSameSub && this.item.Submarine != null && Character.Controlled != null && (Character.Controlled.Submarine == null || !Character.Controlled.Submarine.IsEntityFoundOnThisSub(this.item, true, false, false)))
			{
				return;
			}
			if (itemSound.Loop)
			{
				if (this.loopingSoundChannel != null && this.loopingSoundChannel.Sound != itemSound.RoundSound.Sound)
				{
					this.loopingSoundChannel.FadeOutAndDispose();
					this.loopingSoundChannel = null;
				}
				if (this.loopingSoundChannel == null || !this.loopingSoundChannel.IsPlaying)
				{
					float volume = this.GetSoundVolume(itemSound);
					if (volume <= 0.0001f)
					{
						return;
					}
					this.loopingSound = itemSound;
					this.loopingSoundChannel = SoundPlayer.PlaySound(this.loopingSound.RoundSound, position, new float?(0.01f), this.item.CurrentHull);
					if (this.loopingSoundChannel != null)
					{
						this.loopingSoundChannel.Looping = true;
						this.loopingSoundChannel.Near = this.loopingSound.Range * 0.4f;
						this.loopingSoundChannel.Far = this.loopingSound.Range;
						if (this.loopingSound.RoundSound.Stream)
						{
							this.loopingSoundChannel.StreamSeekPos = this.loopingSound.RoundSound.LastStreamSeekPos;
							return;
						}
					}
				}
			}
			else
			{
				float volume2 = this.GetSoundVolume(itemSound);
				if (volume2 <= 0.0001f)
				{
					return;
				}
				SoundChannel channel = SoundPlayer.PlaySound(itemSound.RoundSound, position, new float?(volume2), this.item.CurrentHull);
				if (channel != null)
				{
					this.playingOneshotSoundChannels.Add(channel);
				}
			}
		}

		// Token: 0x060059BB RID: 22971 RVA: 0x002E285B File Offset: 0x002E0A5B
		public void StopLoopingSound()
		{
			if (this.loopingSound == null)
			{
				return;
			}
			if (this.loopingSoundChannel != null)
			{
				this.loopingSoundChannel.FadeOutAndDispose();
				this.loopingSoundChannel = null;
				this.loopingSound = null;
			}
		}

		// Token: 0x060059BC RID: 22972 RVA: 0x002E2887 File Offset: 0x002E0A87
		public void StopSounds(ActionType type)
		{
			if (this.loopingSound == null || this.loopingSound.Type != type)
			{
				return;
			}
			this.StopLoopingSound();
		}

		// Token: 0x060059BD RID: 22973 RVA: 0x002E28A8 File Offset: 0x002E0AA8
		private float GetSoundVolume(ItemSound sound)
		{
			if (sound == null)
			{
				return 0f;
			}
			if (sound.VolumeProperty == "")
			{
				return sound.VolumeMultiplier;
			}
			SerializableProperty property = null;
			ISerializableEntity targetEntity = null;
			if (this.SerializableProperties.TryGetValue(sound.VolumeProperty, out property))
			{
				targetEntity = this;
			}
			else if (this.Item.SerializableProperties.TryGetValue(sound.VolumeProperty, out property))
			{
				targetEntity = this.Item;
			}
			if (property != null)
			{
				float newVolume;
				try
				{
					newVolume = property.GetFloatValue(targetEntity);
				}
				catch
				{
					return 0f;
				}
				newVolume = Math.Min(newVolume * sound.VolumeMultiplier, 1f);
				if (!MathUtils.IsValid(newVolume))
				{
					DebugConsole.Log(string.Concat(new string[]
					{
						"Invalid sound volume (item ",
						this.item.Name,
						", ",
						base.GetType().ToString(),
						"): ",
						newVolume.ToString()
					}));
					GameAnalyticsManager.AddErrorEventOnce("ItemComponent.PlaySound:" + this.item.Name + base.GetType().ToString(), GameAnalyticsManager.ErrorSeverity.Error, string.Concat(new string[]
					{
						"Invalid sound volume (item ",
						this.item.Name,
						", ",
						base.GetType().ToString(),
						"): ",
						newVolume.ToString()
					}));
					return 0f;
				}
				return MathHelper.Clamp(newVolume, 0f, 1f);
			}
			return 0f;
		}

		// Token: 0x060059BE RID: 22974 RVA: 0x002E2A3C File Offset: 0x002E0C3C
		public bool ShouldDrawHUD(Character character)
		{
			Character controlled = Character.Controlled;
			if (((controlled != null) ? controlled.SelectedItem : null) != null)
			{
				Controller controller = this.item.GetComponent<Controller>();
				if (controller != null && controller.User == Character.Controlled && controller.HideAllItemComponentHUDs)
				{
					return false;
				}
			}
			return this.ShouldDrawHUDComponentSpecific(character);
		}

		// Token: 0x060059BF RID: 22975 RVA: 0x002E2A89 File Offset: 0x002E0C89
		protected virtual bool ShouldDrawHUDComponentSpecific(Character character)
		{
			return true;
		}

		// Token: 0x060059C0 RID: 22976 RVA: 0x002E2A8C File Offset: 0x002E0C8C
		public ItemComponent GetLinkUIToComponent()
		{
			if (string.IsNullOrEmpty(this.LinkUIToComponent))
			{
				return null;
			}
			foreach (ItemComponent component in this.item.Components)
			{
				if (component.name.Equals(this.LinkUIToComponent, StringComparison.OrdinalIgnoreCase))
				{
					this.linkToUIComponent = component;
				}
			}
			if (this.linkToUIComponent == null)
			{
				DebugConsole.ThrowError(string.Concat(new string[]
				{
					"Failed to link the component \"",
					this.Name,
					"\" to \"",
					this.LinkUIToComponent,
					"\" in the item \"",
					this.item.Name,
					"\" - component with a matching name not found."
				}), null, null, false, false);
			}
			return this.linkToUIComponent;
		}

		// Token: 0x060059C1 RID: 22977 RVA: 0x002E2B6C File Offset: 0x002E0D6C
		public virtual void DrawHUD(SpriteBatch spriteBatch, Character character)
		{
			if (this.HUDOverlay != null)
			{
				Vector2 screenSize = new Vector2((float)GameMain.GraphicsWidth, (float)GameMain.GraphicsHeight);
				SpriteSheet spriteSheet = this.HUDOverlay as SpriteSheet;
				if (spriteSheet != null)
				{
					spriteSheet.Draw(spriteBatch, spriteSheet.GetAnimatedSpriteIndex(this.HUDOverlayAnimSpeed, false), screenSize / 2f, Color.White, this.HUDOverlay.Origin, 0f, screenSize / spriteSheet.FrameSize.ToVector2(), SpriteEffects.None, null);
					return;
				}
				this.HUDOverlay.Draw(spriteBatch, screenSize / 2f, Color.White, this.HUDOverlay.Origin, 0f, screenSize / this.HUDOverlay.size, SpriteEffects.None, null);
			}
		}

		// Token: 0x060059C2 RID: 22978 RVA: 0x002E2C40 File Offset: 0x002E0E40
		public virtual void AddToGUIUpdateList(int order = 0)
		{
			GUIFrame guiFrame = this.GuiFrame;
			if (guiFrame == null)
			{
				return;
			}
			guiFrame.AddToGUIUpdateList(false, order);
		}

		// Token: 0x060059C3 RID: 22979 RVA: 0x002E2C54 File Offset: 0x002E0E54
		public void UpdateHUD(Character character, float deltaTime, Camera cam)
		{
			this.UpdateHUDComponentSpecific(character, deltaTime, cam);
			if (this.guiFrameUpdatePending && !PlayerInput.PrimaryMouseButtonHeld())
			{
				this.guiFrameUpdatePending = false;
				SerializableProperty property;
				if (this.SerializableProperties.TryGetValue("GuiFrameOffset".ToIdentifier(), out property))
				{
					GameClient client = GameMain.Client;
					if (client == null)
					{
						return;
					}
					client.CreateEntityEvent(this.Item, new Item.ChangePropertyEventData(property, this));
				}
			}
		}

		// Token: 0x060059C4 RID: 22980 RVA: 0x002E2CBA File Offset: 0x002E0EBA
		public virtual void UpdateHUDComponentSpecific(Character character, float deltaTime, Camera cam)
		{
		}

		// Token: 0x060059C5 RID: 22981 RVA: 0x002E2CBC File Offset: 0x002E0EBC
		public virtual void UpdateEditing(float deltaTime)
		{
		}

		// Token: 0x060059C6 RID: 22982 RVA: 0x002E2CBE File Offset: 0x002E0EBE
		public virtual void CreateEditingHUD(SerializableEntityEditor editor)
		{
		}

		// Token: 0x060059C7 RID: 22983 RVA: 0x002E2CC0 File Offset: 0x002E0EC0
		private bool LoadElemProjSpecific(ContentXElement subElement)
		{
			string a = subElement.Name.ToString().ToLowerInvariant();
			if (!(a == "guiframe"))
			{
				if (!(a == "hudoverlayanimated"))
				{
					if (!(a == "hudoverlay"))
					{
						if (!(a == "alternativelayout"))
						{
							if (!(a == "itemsound") && !(a == "sound"))
							{
								return false;
							}
							string filePath = subElement.GetAttributeStringUnrestricted("file", "");
							if (filePath.IsNullOrEmpty())
							{
								filePath = subElement.GetAttributeStringUnrestricted("sound", "");
							}
							if (filePath.IsNullOrEmpty())
							{
								DebugConsole.ThrowError("Error when instantiating item \"" + this.item.Name + "\" - sound with no file path set", null, subElement.ContentPackage, false, false);
							}
							else
							{
								string typeStr = subElement.GetAttributeString("type", "");
								ActionType type;
								try
								{
									type = (ActionType)Enum.Parse(typeof(ActionType), typeStr, true);
								}
								catch (Exception e)
								{
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(33, 2);
									defaultInterpolatedStringHandler.AppendLiteral("Invalid sound type \"");
									defaultInterpolatedStringHandler.AppendFormatted(typeStr);
									defaultInterpolatedStringHandler.AppendLiteral("\" in item \"");
									defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.item.Prefab.Identifier);
									defaultInterpolatedStringHandler.AppendLiteral("\"!");
									DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), e, subElement.ContentPackage, false, false);
									return true;
								}
								RoundSound sound = RoundSound.Load(subElement);
								if (sound != null)
								{
									ItemSound itemSound = new ItemSound(sound, type, subElement.GetAttributeBool("loop", false), subElement.GetAttributeBool("onlyinsamesub", false))
									{
										VolumeProperty = subElement.GetAttributeIdentifier("volumeproperty", "")
									};
									if (this.soundSelectionModes == null)
									{
										this.soundSelectionModes = new Dictionary<ActionType, SoundSelectionMode>();
									}
									if (!this.soundSelectionModes.ContainsKey(type) || this.soundSelectionModes[type] == SoundSelectionMode.Random)
									{
										Dictionary<ActionType, SoundSelectionMode> dictionary = this.soundSelectionModes;
										ActionType key = type;
										string key2 = "selectionmode";
										SoundSelectionMode soundSelectionMode = SoundSelectionMode.Random;
										dictionary[key] = subElement.GetAttributeEnum<SoundSelectionMode>(key2, soundSelectionMode);
									}
									List<ItemSound> soundList;
									if (!this.sounds.TryGetValue(itemSound.Type, out soundList))
									{
										soundList = new List<ItemSound>();
										this.sounds.Add(itemSound.Type, soundList);
										this.hasSoundsOfType[(int)itemSound.Type] = true;
									}
									soundList.Add(itemSound);
								}
							}
						}
						else
						{
							this.AlternativeLayout = ItemComponent.GUILayoutSettings.Load(subElement);
						}
					}
					else
					{
						this.HUDOverlay = new Sprite(subElement, "", "", false, 1f);
					}
				}
				else
				{
					this.HUDOverlay = new SpriteSheet(subElement, "", "");
					this.HUDOverlayAnimSpeed = subElement.GetAttributeFloat("animspeed", 1f);
				}
			}
			else if (subElement.GetAttribute("rect") != null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(78, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("Error in item config \"");
				defaultInterpolatedStringHandler2.AppendFormatted<ContentPath>(this.item.ConfigFilePath);
				defaultInterpolatedStringHandler2.AppendLiteral("\" - GUIFrame defined as rect, use RectTransform instead.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, subElement.ContentPackage, false, false);
			}
			else
			{
				this.GuiFrameSource = subElement;
				this.ReloadGuiFrame();
			}
			return true;
		}

		// Token: 0x060059C8 RID: 22984 RVA: 0x002E3004 File Offset: 0x002E1204
		protected void ReleaseGuiFrame()
		{
			if (this.GuiFrame != null)
			{
				this.GuiFrame.RectTransform.Parent = null;
			}
		}

		// Token: 0x060059C9 RID: 22985 RVA: 0x002E3020 File Offset: 0x002E1220
		protected void ReloadGuiFrame()
		{
			if (this.GuiFrame != null)
			{
				this.ReleaseGuiFrame();
			}
			Color? color = null;
			if (this.GuiFrameSource.Attribute("color") != null)
			{
				color = new Color?(this.GuiFrameSource.GetAttributeColor("color", Color.White));
			}
			string style = (this.GuiFrameSource.GetAttribute("style", StringComparison.OrdinalIgnoreCase) == null) ? null : this.GuiFrameSource.GetAttributeString("style", "");
			this.GuiFrame = new GUIFrame(RectTransform.Load(this.GuiFrameSource, GUI.Canvas, Anchor.Center), style, color);
			this.GuiFrame.RectTransform.ScreenSpaceOffset = this.GuiFrameOffset;
			this.TryCreateDragHandle();
			this.DefaultLayout = ItemComponent.GUILayoutSettings.Load(this.GuiFrameSource);
			if (this.GuiFrame != null)
			{
				this.GuiFrame.RectTransform.ParentChanged += this.OnGUIParentChanged;
			}
			GameMain.Instance.ResolutionChanged += this.OnResolutionChangedPrivate;
		}

		// Token: 0x060059CA RID: 22986 RVA: 0x002E3128 File Offset: 0x002E1328
		protected void TryCreateDragHandle()
		{
			if (this.GuiFrame != null && this.GuiFrameSource.GetAttributeBool("draggable", true))
			{
				bool hideDragIcons = this.GuiFrameSource.GetAttributeBool("hidedragicons", false);
				this.guiFrameDragHandle = new GUIDragHandle(new RectTransform(Vector2.One, this.GuiFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), this.GuiFrame.RectTransform, null)
				{
					Enabled = !this.LockGuiFramePosition,
					DragArea = HUDLayoutSettings.ItemHUDArea
				};
				int iconHeight = GUIStyle.ItemFrameMargin.Y / 4;
				GUIImage dragIcon = new GUIImage(new RectTransform(new Point(this.GuiFrame.Rect.Width, iconHeight), this.guiFrameDragHandle.RectTransform, Anchor.TopCenter, null, ScaleBasis.Normal, false)
				{
					AbsoluteOffset = new Point(0, iconHeight / 2)
				}, "GUIDragIndicatorHorizontal", GUIImage.ScalingMode.None);
				dragIcon.RectTransform.MinSize = new Point(0, iconHeight);
				this.guiFrameDragHandle.ValidatePosition = delegate(RectTransform rectT)
				{
					Character controlled = Character.Controlled;
					IEnumerable<ItemComponent> enumerable;
					if (controlled == null)
					{
						enumerable = null;
					}
					else
					{
						Item selectedItem = controlled.SelectedItem;
						enumerable = ((selectedItem != null) ? selectedItem.ActiveHUDs : null);
					}
					IEnumerable<ItemComponent> activeHuds = enumerable ?? this.item.ActiveHUDs;
					foreach (ItemComponent ic in activeHuds)
					{
						if (ic != this && ic.GuiFrame != null && ic.CanBeSelected && ((float)ic.GuiFrame.Rect.Width <= (float)GameMain.GraphicsWidth * 0.9f || (float)ic.GuiFrame.Rect.Height <= (float)GameMain.GraphicsHeight * 0.9f) && dragIcon.Rect.Intersects(ic.GuiFrame.Rect))
						{
							this.GuiFrame.ImmediateFlash(null);
							return false;
						}
					}
					foreach (ItemComponent ic2 in activeHuds)
					{
						ItemContainer itemContainer = ic2 as ItemContainer;
						if (itemContainer != null)
						{
							itemContainer.Inventory.CreateSlots();
						}
					}
					this.GuiFrameOffset = this.GuiFrame.RectTransform.ScreenSpaceOffset;
					this.guiFrameUpdatePending = true;
					return true;
				};
				int buttonHeight = (int)((float)GUIStyle.ItemFrameMargin.Y * 0.4f);
				GUIButton settingsIcon = new GUIButton(new RectTransform(new Point(buttonHeight), this.guiFrameDragHandle.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false)
				{
					AbsoluteOffset = new Point(buttonHeight / 4),
					MinSize = new Point(buttonHeight)
				}, Alignment.Center, "GUIButtonSettings", null)
				{
					OnClicked = delegate(GUIButton btn, object userdata)
					{
						GUIContextMenu.CreateContextMenu(new ContextMenuOption[]
						{
							new ContextMenuOption("item.resetuiposition", true, delegate()
							{
								foreach (ItemComponent ic in this.item.Components)
								{
									if (ic.GuiFrame != null && ic.GuiFrameOffset != Point.Zero)
									{
										ic.GuiFrameOffset = Point.Zero;
										ic.guiFrameUpdatePending = true;
									}
								}
								Character controlled = Character.Controlled;
								if (((controlled != null) ? controlled.SelectedItem : null) != null && this.item != Character.Controlled.SelectedItem)
								{
									Character.Controlled.SelectedItem.ForceHUDLayoutUpdate(true);
									return;
								}
								this.item.ForceHUDLayoutUpdate(true);
							}),
							new ContextMenuOption(TextManager.Get(this.LockGuiFramePosition ? "item.unlockuiposition" : "item.lockuiposition"), true, delegate()
							{
								this.GuiFrameOffset = this.GuiFrame.RectTransform.ScreenSpaceOffset;
								this.LockGuiFramePosition = !this.LockGuiFramePosition;
								this.guiFrameDragHandle.Enabled = !this.LockGuiFramePosition;
								SerializableProperty property;
								if (this.SerializableProperties.TryGetValue("LockGuiFramePosition".ToIdentifier(), out property))
								{
									GameClient client = GameMain.Client;
									if (client == null)
									{
										return;
									}
									client.CreateEntityEvent(this.Item, new Item.ChangePropertyEventData(property, this));
								}
							})
						});
						return true;
					}
				};
				if (hideDragIcons)
				{
					dragIcon.Visible = false;
					settingsIcon.Visible = false;
				}
			}
		}

		// Token: 0x060059CB RID: 22987 RVA: 0x002E32FA File Offset: 0x002E14FA
		protected virtual void CreateGUI()
		{
		}

		// Token: 0x060059CC RID: 22988 RVA: 0x002E32FC File Offset: 0x002E14FC
		protected void StartDelayedCorrection(IReadMessage buffer, float sendingTime, bool waitForMidRoundSync = false)
		{
			if (this.delayedCorrectionCoroutine != null)
			{
				CoroutineManager.StopCoroutines(this.delayedCorrectionCoroutine);
			}
			this.delayedCorrectionCoroutine = CoroutineManager.StartCoroutine(this.DoDelayedCorrection(buffer, sendingTime, waitForMidRoundSync), "");
		}

		// Token: 0x060059CD RID: 22989 RVA: 0x002E332A File Offset: 0x002E152A
		private IEnumerable<CoroutineStatus> DoDelayedCorrection(IReadMessage buffer, float sendingTime, bool waitForMidRoundSync)
		{
			ItemComponent.<DoDelayedCorrection>d__89 <DoDelayedCorrection>d__ = new ItemComponent.<DoDelayedCorrection>d__89(-2);
			<DoDelayedCorrection>d__.<>4__this = this;
			<DoDelayedCorrection>d__.<>3__buffer = buffer;
			<DoDelayedCorrection>d__.<>3__sendingTime = sendingTime;
			<DoDelayedCorrection>d__.<>3__waitForMidRoundSync = waitForMidRoundSync;
			return <DoDelayedCorrection>d__;
		}

		// Token: 0x060059CE RID: 22990 RVA: 0x002E334F File Offset: 0x002E154F
		protected void OnGUIParentChanged(RectTransform newParent)
		{
			if (newParent == null)
			{
				GameMain.Instance.ResolutionChanged -= this.OnResolutionChangedPrivate;
			}
		}

		// Token: 0x060059CF RID: 22991 RVA: 0x002E336A File Offset: 0x002E156A
		protected virtual void OnResolutionChanged()
		{
		}

		// Token: 0x060059D0 RID: 22992 RVA: 0x002E336C File Offset: 0x002E156C
		private void OnResolutionChangedPrivate()
		{
			if (this.RecreateGUIOnResolutionChange)
			{
				this.ReloadGuiFrame();
				this.CreateGUI();
			}
			this.OnResolutionChanged();
			this.item.ForceHUDLayoutUpdate(true);
			if (this.GuiFrame != null)
			{
				GUIDragHandle dragHandle = this.GuiFrame.GetChild<GUIDragHandle>();
				if (dragHandle != null)
				{
					dragHandle.DragArea = HUDLayoutSettings.ItemHUDArea;
				}
			}
		}

		// Token: 0x060059D1 RID: 22993 RVA: 0x002E33C1 File Offset: 0x002E15C1
		public virtual void OnPlayerSkillsChanged()
		{
		}

		// Token: 0x060059D2 RID: 22994 RVA: 0x002E33C3 File Offset: 0x002E15C3
		public virtual void AddTooltipInfo(ref LocalizedString name, ref LocalizedString description)
		{
		}

		// Token: 0x17001688 RID: 5768
		// (get) Token: 0x060059D3 RID: 22995 RVA: 0x002E33C5 File Offset: 0x002E15C5
		// (set) Token: 0x060059D4 RID: 22996 RVA: 0x002E33D0 File Offset: 0x002E15D0
		public ItemComponent Parent
		{
			get
			{
				return this.parent;
			}
			set
			{
				if (this.parent == value)
				{
					return;
				}
				if (this.InheritParentIsActive)
				{
					if (this.parent != null)
					{
						ItemComponent itemComponent = this.parent;
						itemComponent.OnActiveStateChanged = (Action<bool>)Delegate.Remove(itemComponent.OnActiveStateChanged, new Action<bool>(this.SetActiveState));
					}
					if (value != null)
					{
						value.OnActiveStateChanged = (Action<bool>)Delegate.Combine(value.OnActiveStateChanged, new Action<bool>(this.SetActiveState));
					}
				}
				this.parent = value;
			}
		}

		// Token: 0x17001689 RID: 5769
		// (get) Token: 0x060059D5 RID: 22997 RVA: 0x002E344A File Offset: 0x002E164A
		// (set) Token: 0x060059D6 RID: 22998 RVA: 0x002E3452 File Offset: 0x002E1652
		[Serialize(true, IsPropertySaveable.No, "If this is a child component of another component, should this component inherit the IsActive state of the parent?", "", false)]
		public bool InheritParentIsActive { get; set; }

		// Token: 0x1700168A RID: 5770
		// (get) Token: 0x060059D7 RID: 22999 RVA: 0x002E345B File Offset: 0x002E165B
		public virtual bool DontTransferInventoryBetweenSubs
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700168B RID: 5771
		// (get) Token: 0x060059D8 RID: 23000 RVA: 0x002E345E File Offset: 0x002E165E
		public virtual bool DisallowSellingItemsFromContainer
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700168C RID: 5772
		// (get) Token: 0x060059D9 RID: 23001 RVA: 0x002E3461 File Offset: 0x002E1661
		// (set) Token: 0x060059DA RID: 23002 RVA: 0x002E3469 File Offset: 0x002E1669
		[Editable]
		[Serialize(0f, IsPropertySaveable.No, "How long it takes to pick up the item (in seconds).", "", false)]
		public float PickingTime { get; set; }

		// Token: 0x1700168D RID: 5773
		// (get) Token: 0x060059DB RID: 23003 RVA: 0x002E3472 File Offset: 0x002E1672
		// (set) Token: 0x060059DC RID: 23004 RVA: 0x002E347A File Offset: 0x002E167A
		[Serialize("", IsPropertySaveable.No, "What to display on the progress bar when this item is being picked.", "", false)]
		public string PickingMsg { get; set; }

		// Token: 0x1700168E RID: 5774
		// (get) Token: 0x060059DD RID: 23005 RVA: 0x002E3483 File Offset: 0x002E1683
		// (set) Token: 0x060059DE RID: 23006 RVA: 0x002E348B File Offset: 0x002E168B
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; protected set; }

		// Token: 0x1700168F RID: 5775
		// (get) Token: 0x060059DF RID: 23007 RVA: 0x002E3494 File Offset: 0x002E1694
		// (set) Token: 0x060059E0 RID: 23008 RVA: 0x002E349C File Offset: 0x002E169C
		public virtual bool IsActive
		{
			get
			{
				return this.isActive;
			}
			set
			{
				if (!value)
				{
					this.IsActiveTimer = 0f;
					if (this.isActive)
					{
						this.StopSounds(ActionType.OnActive);
					}
				}
				if (value != this.IsActive)
				{
					Action<bool> onActiveStateChanged = this.OnActiveStateChanged;
					if (onActiveStateChanged != null)
					{
						onActiveStateChanged(value);
					}
				}
				this.isActive = value;
			}
		}

		// Token: 0x17001690 RID: 5776
		// (get) Token: 0x060059E1 RID: 23009 RVA: 0x002E34E8 File Offset: 0x002E16E8
		// (set) Token: 0x060059E2 RID: 23010 RVA: 0x002E34F0 File Offset: 0x002E16F0
		[Serialize(PropertyConditional.LogicalOperatorType.And, IsPropertySaveable.No, "", "", false)]
		public PropertyConditional.LogicalOperatorType IsActiveConditionalComparison { get; set; }

		// Token: 0x17001691 RID: 5777
		// (get) Token: 0x060059E3 RID: 23011 RVA: 0x002E34F9 File Offset: 0x002E16F9
		// (set) Token: 0x060059E4 RID: 23012 RVA: 0x002E3504 File Offset: 0x002E1704
		public bool Drawable
		{
			get
			{
				return this.drawable;
			}
			set
			{
				if (value == this.drawable)
				{
					return;
				}
				if (!(this is IDrawableComponent))
				{
					DebugConsole.ThrowError("Couldn't make \"" + ((this != null) ? this.ToString() : null) + "\" drawable (the component doesn't implement the IDrawableComponent interface)", null, null, false, false);
					return;
				}
				this.drawable = value;
				if (this.drawable)
				{
					this.item.EnableDrawableComponent((IDrawableComponent)this);
					return;
				}
				this.item.DisableDrawableComponent((IDrawableComponent)this);
			}
		}

		// Token: 0x17001692 RID: 5778
		// (get) Token: 0x060059E5 RID: 23013 RVA: 0x002E357B File Offset: 0x002E177B
		// (set) Token: 0x060059E6 RID: 23014 RVA: 0x002E3583 File Offset: 0x002E1783
		[Editable]
		[Serialize(false, IsPropertySaveable.No, "Can the item be picked up (or interacted with, if the pick action does something else than picking up the item).", "", false)]
		public bool CanBePicked
		{
			get
			{
				return this.canBePicked;
			}
			set
			{
				this.canBePicked = value;
			}
		}

		// Token: 0x17001693 RID: 5779
		// (get) Token: 0x060059E7 RID: 23015 RVA: 0x002E358C File Offset: 0x002E178C
		// (set) Token: 0x060059E8 RID: 23016 RVA: 0x002E3594 File Offset: 0x002E1794
		[Serialize(false, IsPropertySaveable.No, "Should the interface of the item (if it has one) be drawn when the item is equipped.", "", false)]
		public bool DrawHudWhenEquipped { get; protected set; }

		// Token: 0x17001694 RID: 5780
		// (get) Token: 0x060059E9 RID: 23017 RVA: 0x002E359D File Offset: 0x002E179D
		// (set) Token: 0x060059EA RID: 23018 RVA: 0x002E35A5 File Offset: 0x002E17A5
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		[ConditionallyEditable(ConditionallyEditable.ConditionType.OnlyByStatusEffectsAndNetwork, false)]
		public bool LockGuiFramePosition { get; set; }

		// Token: 0x17001695 RID: 5781
		// (get) Token: 0x060059EB RID: 23019 RVA: 0x002E35AE File Offset: 0x002E17AE
		// (set) Token: 0x060059EC RID: 23020 RVA: 0x002E35B6 File Offset: 0x002E17B6
		[Serialize("0,0", IsPropertySaveable.Yes, "", "", false)]
		[ConditionallyEditable(ConditionallyEditable.ConditionType.OnlyByStatusEffectsAndNetwork, false)]
		public Point GuiFrameOffset { get; set; }

		// Token: 0x17001696 RID: 5782
		// (get) Token: 0x060059ED RID: 23021 RVA: 0x002E35BF File Offset: 0x002E17BF
		// (set) Token: 0x060059EE RID: 23022 RVA: 0x002E35C7 File Offset: 0x002E17C7
		[Serialize(false, IsPropertySaveable.No, "Can the item be selected by interacting with it.", "", false)]
		public bool CanBeSelected
		{
			get
			{
				return this.canBeSelected;
			}
			set
			{
				this.canBeSelected = value;
			}
		}

		// Token: 0x17001697 RID: 5783
		// (get) Token: 0x060059EF RID: 23023 RVA: 0x002E35D0 File Offset: 0x002E17D0
		// (set) Token: 0x060059F0 RID: 23024 RVA: 0x002E35D8 File Offset: 0x002E17D8
		[Serialize(false, IsPropertySaveable.No, "Can the item be combined with other items of the same type.", "", false)]
		public bool CanBeCombined
		{
			get
			{
				return this.canBeCombined;
			}
			set
			{
				this.canBeCombined = value;
			}
		}

		// Token: 0x17001698 RID: 5784
		// (get) Token: 0x060059F1 RID: 23025 RVA: 0x002E35E1 File Offset: 0x002E17E1
		// (set) Token: 0x060059F2 RID: 23026 RVA: 0x002E35E9 File Offset: 0x002E17E9
		[Serialize(false, IsPropertySaveable.No, "Should the item be removed if combining it with an other item causes the condition of this item to drop to 0.", "", false)]
		public bool RemoveOnCombined
		{
			get
			{
				return this.removeOnCombined;
			}
			set
			{
				this.removeOnCombined = value;
			}
		}

		// Token: 0x17001699 RID: 5785
		// (get) Token: 0x060059F3 RID: 23027 RVA: 0x002E35F2 File Offset: 0x002E17F2
		// (set) Token: 0x060059F4 RID: 23028 RVA: 0x002E35FA File Offset: 0x002E17FA
		[Serialize(false, IsPropertySaveable.No, "Can the \"Use\" action of the item be triggered by characters or just other items/StatusEffects.", "", false)]
		public bool CharacterUsable
		{
			get
			{
				return this.characterUsable;
			}
			set
			{
				this.characterUsable = value;
			}
		}

		// Token: 0x1700169A RID: 5786
		// (get) Token: 0x060059F5 RID: 23029 RVA: 0x002E3603 File Offset: 0x002E1803
		// (set) Token: 0x060059F6 RID: 23030 RVA: 0x002E360B File Offset: 0x002E180B
		[Serialize(true, IsPropertySaveable.No, "Can the properties of the component be edited in-game (only applicable if the component has in-game editable properties).", "", false)]
		[Editable]
		public bool AllowInGameEditing { get; set; }

		// Token: 0x1700169B RID: 5787
		// (get) Token: 0x060059F7 RID: 23031 RVA: 0x002E3614 File Offset: 0x002E1814
		// (set) Token: 0x060059F8 RID: 23032 RVA: 0x002E361C File Offset: 0x002E181C
		public InputType PickKey { get; protected set; }

		// Token: 0x1700169C RID: 5788
		// (get) Token: 0x060059F9 RID: 23033 RVA: 0x002E3625 File Offset: 0x002E1825
		// (set) Token: 0x060059FA RID: 23034 RVA: 0x002E362D File Offset: 0x002E182D
		public InputType SelectKey { get; protected set; }

		// Token: 0x1700169D RID: 5789
		// (get) Token: 0x060059FB RID: 23035 RVA: 0x002E3636 File Offset: 0x002E1836
		// (set) Token: 0x060059FC RID: 23036 RVA: 0x002E363E File Offset: 0x002E183E
		[Serialize(false, IsPropertySaveable.No, "Should the item be deleted when it's used.", "", false)]
		public bool DeleteOnUse { get; set; }

		// Token: 0x1700169E RID: 5790
		// (get) Token: 0x060059FD RID: 23037 RVA: 0x002E3647 File Offset: 0x002E1847
		public Item Item
		{
			get
			{
				return this.item;
			}
		}

		// Token: 0x1700169F RID: 5791
		// (get) Token: 0x060059FE RID: 23038 RVA: 0x002E364F File Offset: 0x002E184F
		public string Name
		{
			get
			{
				return this.name;
			}
		}

		// Token: 0x170016A0 RID: 5792
		// (get) Token: 0x060059FF RID: 23039 RVA: 0x002E3657 File Offset: 0x002E1857
		// (set) Token: 0x06005A00 RID: 23040 RVA: 0x002E365F File Offset: 0x002E185F
		[Editable]
		[Serialize("", IsPropertySaveable.Yes, "A text displayed next to the item when it's highlighted (generally instructs how to interact with the item, e.g. \"[Mouse1] Pick up\").", "ItemMsg", false)]
		public string Msg { get; set; }

		// Token: 0x170016A1 RID: 5793
		// (get) Token: 0x06005A01 RID: 23041 RVA: 0x002E3668 File Offset: 0x002E1868
		// (set) Token: 0x06005A02 RID: 23042 RVA: 0x002E3670 File Offset: 0x002E1870
		public LocalizedString DisplayMsg { get; set; }

		// Token: 0x170016A2 RID: 5794
		// (get) Token: 0x06005A03 RID: 23043 RVA: 0x002E3679 File Offset: 0x002E1879
		// (set) Token: 0x06005A04 RID: 23044 RVA: 0x002E3681 File Offset: 0x002E1881
		[Serialize(0f, IsPropertySaveable.No, "How useful the item is in combat? Used by AI to decide which item it should use as a weapon. For the sake of clarity, use a value between 0 and 100 (not forced). Note that there's also a generic BotPriority for all item prefabs.", "", false)]
		public float CombatPriority { get; private set; }

		// Token: 0x170016A3 RID: 5795
		// (get) Token: 0x06005A05 RID: 23045 RVA: 0x002E368A File Offset: 0x002E188A
		// (set) Token: 0x06005A06 RID: 23046 RVA: 0x002E3692 File Offset: 0x002E1892
		[Serialize(0, IsPropertySaveable.Yes, "", "", true)]
		public int ManuallySelectedSound { get; private set; }

		// Token: 0x170016A4 RID: 5796
		// (get) Token: 0x06005A07 RID: 23047 RVA: 0x002E369B File Offset: 0x002E189B
		public float Speed
		{
			get
			{
				return this.item.Speed;
			}
		}

		// Token: 0x06005A08 RID: 23048 RVA: 0x002E36A8 File Offset: 0x002E18A8
		public ItemComponent(Item item, ContentXElement element)
		{
			ItemComponent.<>c__DisplayClass217_0 CS$<>8__locals1;
			CS$<>8__locals1.item = item;
			base..ctor();
			CS$<>8__locals1.<>4__this = this;
			this.item = CS$<>8__locals1.item;
			this.originalElement = element;
			this.name = element.Name.ToString();
			this.SerializableProperties = SerializableProperty.GetProperties(this);
			this.RequiredItems = new Dictionary<RelatedItem.RelationType, List<RelatedItem>>();
			this.hasSoundsOfType = new bool[Enum.GetValues(typeof(ActionType)).Length];
			this.sounds = new Dictionary<ActionType, List<ItemSound>>();
			this.SelectKey = InputType.Select;
			try
			{
				string selectKeyStr = element.GetAttributeString("selectkey", "Select");
				selectKeyStr = ToolBox.ConvertInputType(selectKeyStr);
				this.SelectKey = (InputType)Enum.Parse(typeof(InputType), selectKeyStr, true);
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Invalid select key in " + ((element != null) ? element.ToString() : null) + "!", e, element.ContentPackage, false, false);
			}
			this.PickKey = InputType.Select;
			try
			{
				string pickKeyStr = element.GetAttributeString("pickkey", "Select");
				pickKeyStr = ToolBox.ConvertInputType(pickKeyStr);
				this.PickKey = (InputType)Enum.Parse(typeof(InputType), pickKeyStr, true);
			}
			catch (Exception e2)
			{
				DebugConsole.ThrowError("Invalid pick key in " + ((element != null) ? element.ToString() : null) + "!", e2, element.ContentPackage, false, false);
			}
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			this.ParseMsg();
			string inheritRequiredSkillsFrom = element.GetAttributeString("inheritrequiredskillsfrom", "");
			if (!string.IsNullOrEmpty(inheritRequiredSkillsFrom))
			{
				ItemComponent component = CS$<>8__locals1.item.Components.Find((ItemComponent ic) => ic.Name.Equals(inheritRequiredSkillsFrom, StringComparison.OrdinalIgnoreCase));
				if (component == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(126, 3);
					defaultInterpolatedStringHandler.AppendLiteral("Error in item \"");
					defaultInterpolatedStringHandler.AppendFormatted(CS$<>8__locals1.item.Name);
					defaultInterpolatedStringHandler.AppendLiteral("\" - component \"");
					defaultInterpolatedStringHandler.AppendFormatted(this.name);
					defaultInterpolatedStringHandler.AppendLiteral("\" is set to inherit its required skills from \"");
					defaultInterpolatedStringHandler.AppendFormatted(inheritRequiredSkillsFrom);
					defaultInterpolatedStringHandler.AppendLiteral("\", but a component of that type couldn't be found.");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
				}
				else
				{
					this.RequiredSkills = component.RequiredSkills;
				}
			}
			string inheritStatusEffectsFrom = element.GetAttributeString("inheritstatuseffectsfrom", "");
			if (!string.IsNullOrEmpty(inheritStatusEffectsFrom))
			{
				this.InheritStatusEffects = true;
				ItemComponent component2 = CS$<>8__locals1.item.Components.Find((ItemComponent ic) => ic.Name.Equals(inheritStatusEffectsFrom, StringComparison.OrdinalIgnoreCase));
				if (component2 == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(124, 3);
					defaultInterpolatedStringHandler2.AppendLiteral("Error in item \"");
					defaultInterpolatedStringHandler2.AppendFormatted(CS$<>8__locals1.item.Name);
					defaultInterpolatedStringHandler2.AppendLiteral("\" - component \"");
					defaultInterpolatedStringHandler2.AppendFormatted(this.name);
					defaultInterpolatedStringHandler2.AppendLiteral("\" is set to inherit its StatusEffects from \"");
					defaultInterpolatedStringHandler2.AppendFormatted(inheritStatusEffectsFrom);
					defaultInterpolatedStringHandler2.AppendLiteral("\", but a component of that type couldn't be found.");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, element.ContentPackage, false, false);
				}
				else if (component2.statusEffectLists != null)
				{
					if (this.statusEffectLists == null)
					{
						this.statusEffectLists = new Dictionary<ActionType, List<StatusEffect>>();
					}
					foreach (KeyValuePair<ActionType, List<StatusEffect>> kvp in component2.statusEffectLists)
					{
						List<StatusEffect> effectList;
						if (!this.statusEffectLists.TryGetValue(kvp.Key, out effectList))
						{
							effectList = new List<StatusEffect>();
							this.statusEffectLists.Add(kvp.Key, effectList);
						}
						effectList.AddRange(kvp.Value);
					}
				}
			}
			foreach (ContentXElement subElement in element.Elements())
			{
				string text = subElement.Name.ToString().ToLowerInvariant();
				if (text != null)
				{
					int length = text.Length;
					switch (length)
					{
					case 8:
						if (!(text == "isactive"))
						{
							goto IL_61E;
						}
						break;
					case 9:
					case 10:
					case 11:
						goto IL_61E;
					case 12:
					{
						char c = text[0];
						if (c != 'r')
						{
							if (c != 's')
							{
								goto IL_61E;
							}
							if (!(text == "statuseffect"))
							{
								goto IL_61E;
							}
							if (this.statusEffectLists == null)
							{
								this.statusEffectLists = new Dictionary<ActionType, List<StatusEffect>>();
							}
							this.<.ctor>g__LoadStatusEffect|217_0(subElement, ref CS$<>8__locals1);
							continue;
						}
						else
						{
							if (!(text == "requireditem"))
							{
								goto IL_61E;
							}
							goto IL_54D;
						}
						break;
					}
					case 13:
					{
						char c = text[8];
						if (c != 'i')
						{
							if (c != 's')
							{
								goto IL_61E;
							}
							if (!(text == "requiredskill"))
							{
								goto IL_61E;
							}
							goto IL_55B;
						}
						else
						{
							if (!(text == "requireditems"))
							{
								goto IL_61E;
							}
							goto IL_54D;
						}
						break;
					}
					case 14:
						if (!(text == "requiredskills"))
						{
							goto IL_61E;
						}
						goto IL_55B;
					default:
						if (length != 17)
						{
							if (length != 19)
							{
								goto IL_61E;
							}
							if (!(text == "isactiveconditional"))
							{
								goto IL_61E;
							}
						}
						else if (!(text == "activeconditional"))
						{
							goto IL_61E;
						}
						break;
					}
					if (this.IsActiveConditionals == null)
					{
						this.IsActiveConditionals = new List<PropertyConditional>();
					}
					this.IsActiveConditionals.AddRange(PropertyConditional.FromXElement(subElement, null));
					continue;
					IL_54D:
					this.SetRequiredItems(subElement, true);
					continue;
					IL_55B:
					if (subElement.GetAttribute("name") != null)
					{
						string[] array = new string[5];
						array[0] = "Error in item config \"";
						int num = 1;
						ContentPath configFilePath = CS$<>8__locals1.item.ConfigFilePath;
						array[num] = ((configFilePath != null) ? configFilePath.ToString() : null);
						array[2] = "\" - skill requirement in component ";
						array[3] = base.GetType().ToString();
						array[4] = " should use a skill identifier instead of the name of the skill.";
						DebugConsole.ThrowError(string.Concat(array), null, element.ContentPackage, false, false);
						continue;
					}
					Identifier skillIdentifier = subElement.GetAttributeIdentifier("identifier", "");
					this.RequiredSkills.Add(new Skill(skillIdentifier, (float)subElement.GetAttributeInt("level", 0)));
					continue;
				}
				IL_61E:
				if (!this.LoadElemProjSpecific(subElement))
				{
					ItemComponent ic2 = ItemComponent.Load(subElement, CS$<>8__locals1.item, false);
					if (ic2 != null)
					{
						ic2.Parent = this;
						if (ic2.InheritParentIsActive)
						{
							ic2.IsActive = this.isActive;
							this.OnActiveStateChanged = (Action<bool>)Delegate.Combine(this.OnActiveStateChanged, new Action<bool>(ic2.SetActiveState));
						}
						CS$<>8__locals1.item.AddComponent(ic2);
					}
				}
			}
		}

		// Token: 0x06005A09 RID: 23049 RVA: 0x002E3DC0 File Offset: 0x002E1FC0
		private void SetActiveState(bool isActive)
		{
			this.IsActive = isActive;
		}

		// Token: 0x06005A0A RID: 23050 RVA: 0x002E3DCC File Offset: 0x002E1FCC
		public void SetRequiredItems(ContentXElement element, bool allowEmpty = false)
		{
			bool returnEmpty = Screen.Selected == GameMain.SubEditorScreen;
			RelatedItem ri = RelatedItem.Load(element, returnEmpty, this.item.Name);
			if (ri == null)
			{
				if (!allowEmpty)
				{
					string[] array = new string[5];
					array[0] = "Error in item config \"";
					int num = 1;
					ContentPath configFilePath = this.item.ConfigFilePath;
					array[num] = ((configFilePath != null) ? configFilePath.ToString() : null);
					array[2] = "\" - component ";
					array[3] = base.GetType().ToString();
					array[4] = " requires an item with no identifiers.";
					DebugConsole.ThrowError(string.Concat(array), null, element.ContentPackage, false, false);
				}
				return;
			}
			if (ri.Identifiers.Count == 0)
			{
				this.DisabledRequiredItems.Add(ri);
				return;
			}
			if (!this.RequiredItems.ContainsKey(ri.Type))
			{
				this.RequiredItems.Add(ri.Type, new List<RelatedItem>());
			}
			this.RequiredItems[ri.Type].Add(ri);
		}

		// Token: 0x06005A0B RID: 23051 RVA: 0x002E3EB5 File Offset: 0x002E20B5
		public virtual void Move(Vector2 amount, bool ignoreContacts = false)
		{
		}

		// Token: 0x06005A0C RID: 23052 RVA: 0x002E3EB7 File Offset: 0x002E20B7
		public virtual bool Pick(Character picker)
		{
			return false;
		}

		// Token: 0x06005A0D RID: 23053 RVA: 0x002E3EBA File Offset: 0x002E20BA
		public virtual bool Select(Character character)
		{
			return this.CanBeSelected;
		}

		// Token: 0x06005A0E RID: 23054 RVA: 0x002E3EC2 File Offset: 0x002E20C2
		public virtual void Drop(Character dropper, bool setTransform = true)
		{
		}

		// Token: 0x06005A0F RID: 23055 RVA: 0x002E3EC4 File Offset: 0x002E20C4
		public virtual bool CrewAIOperate(float deltaTime, Character character, AIObjectiveOperateItem objective)
		{
			return false;
		}

		// Token: 0x170016A5 RID: 5797
		// (get) Token: 0x06005A10 RID: 23056 RVA: 0x002E3EC7 File Offset: 0x002E20C7
		public virtual bool UpdateWhenInactive
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170016A6 RID: 5798
		// (get) Token: 0x06005A11 RID: 23057 RVA: 0x002E3ECA File Offset: 0x002E20CA
		// (set) Token: 0x06005A12 RID: 23058 RVA: 0x002E3ED2 File Offset: 0x002E20D2
		[Serialize(false, IsPropertySaveable.No, "If true, the component will retain its normal functionality when the item reaches 0 condition.", "", false)]
		public bool UpdateWhenBroken { get; set; }

		// Token: 0x06005A13 RID: 23059 RVA: 0x002E3EDC File Offset: 0x002E20DC
		public virtual void Update(float deltaTime, Camera cam)
		{
			this.ApplyStatusEffects(ActionType.OnActive, deltaTime, null, null, null, null, null, 1f);
		}

		// Token: 0x06005A14 RID: 23060 RVA: 0x002E3F03 File Offset: 0x002E2103
		public virtual void UpdateBroken(float deltaTime, Camera cam)
		{
			this.StopSounds(ActionType.OnActive);
		}

		// Token: 0x06005A15 RID: 23061 RVA: 0x002E3F0C File Offset: 0x002E210C
		public virtual bool Use(float deltaTime, Character character = null)
		{
			return this.characterUsable || character == null;
		}

		// Token: 0x06005A16 RID: 23062 RVA: 0x002E3F1C File Offset: 0x002E211C
		public virtual bool SecondaryUse(float deltaTime, Character character = null)
		{
			return false;
		}

		// Token: 0x06005A17 RID: 23063 RVA: 0x002E3F1F File Offset: 0x002E211F
		public virtual void Equip(Character character)
		{
		}

		// Token: 0x06005A18 RID: 23064 RVA: 0x002E3F21 File Offset: 0x002E2121
		public virtual void Unequip(Character character)
		{
		}

		// Token: 0x06005A19 RID: 23065 RVA: 0x002E3F24 File Offset: 0x002E2124
		public virtual void ReceiveSignal(Signal signal, Connection connection)
		{
			string a = connection.Name;
			if (!(a == "activate") && !(a == "use") && !(a == "trigger_in"))
			{
				if (!(a == "toggle"))
				{
					if (!(a == "set_active") && !(a == "set_state"))
					{
						return;
					}
					this.IsActive = (signal.value != "0");
				}
				else if (signal.value != "0")
				{
					this.IsActive = !this.isActive;
					return;
				}
			}
			else if (signal.value != "0")
			{
				this.item.Use(1f, signal.sender, null, null, null);
				return;
			}
		}

		// Token: 0x06005A1A RID: 23066 RVA: 0x002E3FEC File Offset: 0x002E21EC
		public virtual bool Combine(Item item, Character user)
		{
			if (!this.canBeCombined || this.item.Prefab != item.Prefab || item.Condition <= 0f || this.item.Condition <= 0f || item.IsFullCondition || this.item.IsFullCondition)
			{
				return false;
			}
			float transferAmount = Math.Min(item.Condition, this.item.MaxCondition - this.item.Condition);
			if (MathUtils.NearlyEqual(transferAmount, 0f, 0.0001f))
			{
				return false;
			}
			if (this.removeOnCombined)
			{
				if (item.Condition - transferAmount <= 0f)
				{
					if (item.ParentInventory != null)
					{
						Character owner = item.ParentInventory.Owner as Character;
						if (owner != null && owner.HeldItems.Contains(item))
						{
							item.Unequip(owner);
						}
						item.ParentInventory.RemoveItem(item);
					}
					ItemComponent.<Combine>g__RemoveItem|238_0(item);
				}
				else
				{
					item.Condition -= transferAmount;
				}
				if (this.Item.Condition + transferAmount <= 0f)
				{
					if (this.Item.ParentInventory != null)
					{
						Character owner2 = this.Item.ParentInventory.Owner as Character;
						if (owner2 != null && owner2.HeldItems.Contains(this.Item))
						{
							this.Item.Unequip(owner2);
						}
						this.Item.ParentInventory.RemoveItem(this.Item);
					}
					ItemComponent.<Combine>g__RemoveItem|238_0(this.Item);
				}
				else
				{
					this.Item.Condition += transferAmount;
				}
			}
			else
			{
				this.Item.Condition += transferAmount;
				item.Condition -= transferAmount;
			}
			return true;
		}

		// Token: 0x06005A1B RID: 23067 RVA: 0x002E41B4 File Offset: 0x002E23B4
		public void Remove()
		{
			if (this.loopingSoundChannel != null)
			{
				this.loopingSoundChannel.Dispose();
				this.loopingSoundChannel = null;
			}
			if (this.GuiFrame != null)
			{
				GUI.RemoveFromUpdateList(this.GuiFrame, true);
				this.GuiFrame.RectTransform.Parent = null;
				this.GuiFrame = null;
			}
			if (this.delayedCorrectionCoroutine != null)
			{
				CoroutineManager.StopCoroutines(this.delayedCorrectionCoroutine);
				this.delayedCorrectionCoroutine = null;
			}
			this.RemoveComponentSpecific();
		}

		// Token: 0x06005A1C RID: 23068 RVA: 0x002E4227 File Offset: 0x002E2427
		public void ShallowRemove()
		{
			if (this.loopingSoundChannel != null)
			{
				this.loopingSoundChannel.Dispose();
				this.loopingSoundChannel = null;
			}
			this.ShallowRemoveComponentSpecific();
		}

		// Token: 0x06005A1D RID: 23069 RVA: 0x002E4249 File Offset: 0x002E2449
		protected virtual void ShallowRemoveComponentSpecific()
		{
			this.RemoveComponentSpecific();
		}

		// Token: 0x06005A1E RID: 23070 RVA: 0x002E4251 File Offset: 0x002E2451
		protected virtual void RemoveComponentSpecific()
		{
			Sprite hudoverlay = this.HUDOverlay;
			if (hudoverlay != null)
			{
				hudoverlay.Remove();
			}
			this.HUDOverlay = null;
		}

		// Token: 0x06005A1F RID: 23071 RVA: 0x002E426B File Offset: 0x002E246B
		protected string GetTextureDirectory(ContentXElement subElement)
		{
			return this.item.Prefab.GetTexturePath(subElement, this.item.Prefab.ParentPrefab);
		}

		// Token: 0x06005A20 RID: 23072 RVA: 0x002E4290 File Offset: 0x002E2490
		public bool HasRequiredSkills(Character character)
		{
			Skill temp;
			return this.HasRequiredSkills(character, out temp);
		}

		// Token: 0x06005A21 RID: 23073 RVA: 0x002E42A8 File Offset: 0x002E24A8
		public bool HasRequiredSkills(Character character, out Skill insufficientSkill)
		{
			foreach (Skill skill in this.RequiredSkills)
			{
				float characterLevel = character.GetSkillLevel(skill.Identifier);
				if (characterLevel < skill.Level * this.GetSkillMultiplier())
				{
					insufficientSkill = skill;
					return false;
				}
			}
			insufficientSkill = null;
			return true;
		}

		// Token: 0x06005A22 RID: 23074 RVA: 0x002E4320 File Offset: 0x002E2520
		public virtual float GetSkillMultiplier()
		{
			return 1f;
		}

		// Token: 0x06005A23 RID: 23075 RVA: 0x002E4327 File Offset: 0x002E2527
		public float DegreeOfSuccess(Character character)
		{
			return this.DegreeOfSuccess(character, this.RequiredSkills);
		}

		// Token: 0x06005A24 RID: 23076 RVA: 0x002E4338 File Offset: 0x002E2538
		public float DegreeOfSuccess(Character character, List<Skill> requiredSkills)
		{
			if (requiredSkills.Count == 0)
			{
				return 1f;
			}
			if (character == null)
			{
				string errorMsg = "ItemComponent.DegreeOfSuccess failed (character was null).\n" + Environment.StackTrace.CleanupStackTrace();
				DebugConsole.ThrowError(errorMsg, null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("ItemComponent.DegreeOfSuccess:CharacterNull", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return 0f;
			}
			float skillSuccessSum = 0f;
			for (int i = 0; i < requiredSkills.Count; i++)
			{
				float characterLevel = character.GetSkillLevel(requiredSkills[i].Identifier);
				skillSuccessSum += characterLevel - requiredSkills[i].Level;
			}
			float average = skillSuccessSum / (float)requiredSkills.Count;
			return (average + 100f) / 2f / 100f;
		}

		// Token: 0x06005A25 RID: 23077 RVA: 0x002E43E1 File Offset: 0x002E25E1
		public virtual void FlipX(bool relativeToSub)
		{
		}

		// Token: 0x06005A26 RID: 23078 RVA: 0x002E43E3 File Offset: 0x002E25E3
		public virtual void FlipY(bool relativeToSub)
		{
		}

		// Token: 0x06005A27 RID: 23079 RVA: 0x002E43E8 File Offset: 0x002E25E8
		public bool IsEmpty(Character user)
		{
			if (!this.HasRequiredContainedItems(user, false, null))
			{
				return true;
			}
			if (this.Item.OwnInventory != null)
			{
				return !this.Item.OwnInventory.AllItems.Any((Item i) => i.Condition > 0f);
			}
			return false;
		}

		// Token: 0x06005A28 RID: 23080 RVA: 0x002E4448 File Offset: 0x002E2648
		public bool HasRequiredContainedItems(Character user, bool addMessage, LocalizedString msg = null)
		{
			if (!this.RequiredItems.ContainsKey(RelatedItem.RelationType.Contained))
			{
				return true;
			}
			if (this.item.OwnInventory == null)
			{
				return false;
			}
			foreach (RelatedItem ri in this.RequiredItems[RelatedItem.RelationType.Contained])
			{
				if (!ri.CheckRequirements(user, this.item))
				{
					if (msg == null)
					{
						msg = ri.Msg;
					}
					if (addMessage && !msg.IsNullOrEmpty())
					{
						GUI.AddMessage(msg, Color.Red, null, true, null);
					}
					return false;
				}
			}
			return true;
		}

		// Token: 0x06005A29 RID: 23081 RVA: 0x002E44FC File Offset: 0x002E26FC
		public virtual bool HasAccess(Character character)
		{
			if (character.IsBot && this.item.IgnoreByAI(character))
			{
				return false;
			}
			if (!this.item.IsInteractable(character))
			{
				return false;
			}
			if (this.RequiredItems.Count == 0)
			{
				return true;
			}
			List<RelatedItem> relatedItems;
			if (character.Inventory != null && this.RequiredItems.TryGetValue(RelatedItem.RelationType.Picked, out relatedItems))
			{
				foreach (RelatedItem relatedItem in relatedItems)
				{
					foreach (Item otherItem in character.Inventory.AllItems)
					{
						if (relatedItem.MatchesItem(otherItem))
						{
							IdCard idCard = otherItem.GetComponent<IdCard>();
							if (idCard == null || this.CheckIdCardAccess(relatedItem, idCard))
							{
								return true;
							}
						}
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x06005A2A RID: 23082 RVA: 0x002E4600 File Offset: 0x002E2800
		private bool CheckIdCardAccess(RelatedItem relatedItem, IdCard idCard)
		{
			Submarine submarine = this.item.Submarine;
			if (submarine != null && !submarine.IsRespawnShuttle)
			{
				if (idCard.TeamID != CharacterTeamType.None && idCard.TeamID != this.item.Submarine.TeamID)
				{
					if (relatedItem.Identifiers.Any((Identifier id) => id != "idcard"))
					{
						GameSession gameSession = GameMain.GameSession;
						if (!(((gameSession != null) ? gameSession.GameMode : null) is PvPMode))
						{
							return false;
						}
						if (this.item.Submarine.TeamID != CharacterTeamType.FriendlyNPC && this.item.Submarine.TeamID != CharacterTeamType.None)
						{
							return false;
						}
						return true;
					}
				}
				if (idCard.SubmarineSpecificID != 0 && this.item.Submarine.SubmarineSpecificIDTag != idCard.SubmarineSpecificID)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06005A2B RID: 23083 RVA: 0x002E46E0 File Offset: 0x002E28E0
		public virtual bool HasRequiredItems(Character character, bool addMessage, LocalizedString msg = null)
		{
			ItemComponent.<>c__DisplayClass255_0 CS$<>8__locals1 = new ItemComponent.<>c__DisplayClass255_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.msg = msg;
			if (this.RequiredItems.None(null))
			{
				return true;
			}
			if (character.Inventory == null)
			{
				return false;
			}
			CS$<>8__locals1.hasRequiredItems = false;
			bool canContinue = true;
			if (this.RequiredItems.ContainsKey(RelatedItem.RelationType.Equipped))
			{
				foreach (RelatedItem ri in this.RequiredItems[RelatedItem.RelationType.Equipped])
				{
					canContinue = CS$<>8__locals1.<HasRequiredItems>g__CheckItems|0(ri, character.HeldItems);
					if (!canContinue)
					{
						break;
					}
				}
			}
			if (canContinue && this.RequiredItems.ContainsKey(RelatedItem.RelationType.Picked))
			{
				foreach (RelatedItem ri2 in this.RequiredItems[RelatedItem.RelationType.Picked])
				{
					if (!CS$<>8__locals1.<HasRequiredItems>g__CheckItems|0(ri2, character.Inventory.AllItems))
					{
						break;
					}
				}
			}
			if (!CS$<>8__locals1.hasRequiredItems && addMessage && !CS$<>8__locals1.msg.IsNullOrEmpty())
			{
				GUI.AddMessage(CS$<>8__locals1.msg, Color.Red, null, true, null);
			}
			return CS$<>8__locals1.hasRequiredItems;
		}

		// Token: 0x06005A2C RID: 23084 RVA: 0x002E4834 File Offset: 0x002E2A34
		public void ApplyStatusEffects(ActionType type, float deltaTime, Character character = null, Limb targetLimb = null, Entity useTarget = null, Character user = null, Vector2? worldPosition = null, float attackMultiplier = 1f)
		{
			if (this.statusEffectLists == null)
			{
				return;
			}
			List<StatusEffect> statusEffects;
			if (!this.statusEffectLists.TryGetValue(type, out statusEffects))
			{
				return;
			}
			bool broken = this.item.Condition <= 0f;
			bool reducesCondition = false;
			foreach (StatusEffect effect in statusEffects)
			{
				if (!broken || effect.AllowWhenBroken || effect.type == ActionType.OnBroken)
				{
					if (user != null)
					{
						effect.SetUser(user);
					}
					effect.AttackMultiplier = attackMultiplier;
					Character c = character;
					if (user != null && effect.HasTargetType(StatusEffect.TargetType.Character) && !effect.HasTargetType(StatusEffect.TargetType.UseTarget))
					{
						c = user;
					}
					this.item.ApplyStatusEffect(effect, type, deltaTime, c, targetLimb, useTarget, false, false, worldPosition);
					effect.AttackMultiplier = 1f;
					reducesCondition |= effect.ReducesItemCondition();
				}
			}
			if (reducesCondition && user != null && type != ActionType.OnBroken)
			{
				foreach (ItemComponent ic in this.item.Components)
				{
					List<StatusEffect> brokenEffects;
					if (ic.statusEffectLists != null && ic.statusEffectLists.TryGetValue(ActionType.OnBroken, out brokenEffects))
					{
						foreach (StatusEffect brokenEffect in brokenEffects)
						{
							brokenEffect.SetUser(user);
						}
					}
				}
			}
			HintManager.OnStatusEffectApplied(this, type, character);
		}

		// Token: 0x06005A2D RID: 23085 RVA: 0x002E49EC File Offset: 0x002E2BEC
		public virtual void Load(ContentXElement componentElement, bool usePrefabValues, IdRemap idRemap, bool isItemSwap)
		{
			ContentXElement contentXElement = null;
			if (componentElement != contentXElement)
			{
				foreach (XAttribute attribute in componentElement.Attributes())
				{
					SerializableProperty property;
					if (this.SerializableProperties.TryGetValue(attribute.NameAsIdentifier(), out property))
					{
						if (!property.OverridePrefabValues && usePrefabValues)
						{
							if (!isItemSwap)
							{
								continue;
							}
							Editable attribute2 = property.GetAttribute<Editable>();
							if (attribute2 == null || !attribute2.TransferToSwappedItem)
							{
								continue;
							}
						}
						property.TrySetValue(this, attribute.Value);
					}
				}
				this.ParseMsg();
				this.OverrideRequiredItems(componentElement);
			}
			if (this.GuiFrame != null)
			{
				this.GuiFrame.RectTransform.ScreenSpaceOffset = this.GuiFrameOffset;
				if (this.guiFrameDragHandle != null)
				{
					this.guiFrameDragHandle.Enabled = !this.LockGuiFramePosition;
				}
			}
			if (this.item.Submarine != null)
			{
				SerializableProperty.UpgradeGameVersion(this, this.originalElement, this.item.Submarine.Info.GameVersion);
			}
		}

		// Token: 0x06005A2E RID: 23086 RVA: 0x002E4AFC File Offset: 0x002E2CFC
		public virtual void OnMapLoaded()
		{
		}

		// Token: 0x06005A2F RID: 23087 RVA: 0x002E4AFE File Offset: 0x002E2CFE
		public virtual void OnItemLoaded()
		{
		}

		// Token: 0x06005A30 RID: 23088 RVA: 0x002E4B00 File Offset: 0x002E2D00
		public virtual void Clone(ItemComponent original)
		{
		}

		// Token: 0x06005A31 RID: 23089 RVA: 0x002E4B02 File Offset: 0x002E2D02
		public virtual void OnScaleChanged()
		{
		}

		// Token: 0x06005A32 RID: 23090 RVA: 0x002E4B04 File Offset: 0x002E2D04
		public virtual void OnInventoryChanged()
		{
		}

		// Token: 0x06005A33 RID: 23091 RVA: 0x002E4B08 File Offset: 0x002E2D08
		public static ItemComponent Load(ContentXElement element, Item item, bool errorMessages = true)
		{
			Identifier typeName = element.NameAsIdentifier();
			Type type;
			try
			{
				type = ReflectionUtils.GetDerivedNonAbstract<ItemComponent>().Append(typeof(ItemComponent)).FirstOrDefault((Type t) => t.Name == typeName);
				if (type == null)
				{
					if (errorMessages)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(34, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Could not find the component \"");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(typeName);
						defaultInterpolatedStringHandler.AppendLiteral("\" (");
						defaultInterpolatedStringHandler.AppendFormatted<ContentPath>(item.Prefab.ContentFile.Path);
						defaultInterpolatedStringHandler.AppendLiteral(")");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
					}
					return null;
				}
			}
			catch (Exception e)
			{
				if (errorMessages)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(34, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("Could not find the component \"");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(typeName);
					defaultInterpolatedStringHandler2.AppendLiteral("\" (");
					defaultInterpolatedStringHandler2.AppendFormatted<ContentPath>(item.Prefab.ContentFile.Path);
					defaultInterpolatedStringHandler2.AppendLiteral(")");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), e, element.ContentPackage, false, false);
				}
				return null;
			}
			ConstructorInfo constructor;
			try
			{
				if (type != typeof(ItemComponent) && !type.IsSubclassOf(typeof(ItemComponent)))
				{
					return null;
				}
				constructor = type.GetConstructor(new Type[]
				{
					typeof(Item),
					typeof(ContentXElement)
				});
				if (constructor == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(53, 2);
					defaultInterpolatedStringHandler3.AppendLiteral("Could not find the constructor of the component \"");
					defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(typeName);
					defaultInterpolatedStringHandler3.AppendLiteral("\" (");
					defaultInterpolatedStringHandler3.AppendFormatted<ContentPath>(item.Prefab.ContentFile.Path);
					defaultInterpolatedStringHandler3.AppendLiteral(")");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), null, element.ContentPackage, false, false);
					return null;
				}
			}
			catch (Exception e2)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(53, 2);
				defaultInterpolatedStringHandler4.AppendLiteral("Could not find the constructor of the component \"");
				defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(typeName);
				defaultInterpolatedStringHandler4.AppendLiteral("\" (");
				defaultInterpolatedStringHandler4.AppendFormatted<ContentPath>(item.Prefab.ContentFile.Path);
				defaultInterpolatedStringHandler4.AppendLiteral(")");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler4.ToStringAndClear(), e2, element.ContentPackage, false, false);
				return null;
			}
			ItemComponent ic = null;
			try
			{
				object[] lobject = new object[]
				{
					item,
					element
				};
				object component = constructor.Invoke(lobject);
				ic = (ItemComponent)component;
				ic.name = element.Name.ToString();
			}
			catch (TargetInvocationException e3)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(43, 1);
				defaultInterpolatedStringHandler5.AppendLiteral("Error while loading component of the type ");
				defaultInterpolatedStringHandler5.AppendFormatted<Type>(type);
				defaultInterpolatedStringHandler5.AppendLiteral(".");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler5.ToStringAndClear(), e3.InnerException, element.ContentPackage, false, false);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(44, 2);
				defaultInterpolatedStringHandler6.AppendLiteral("ItemComponent.Load:TargetInvocationException");
				defaultInterpolatedStringHandler6.AppendFormatted(item.Name);
				defaultInterpolatedStringHandler6.AppendFormatted<XName>(element.Name);
				string identifier = defaultInterpolatedStringHandler6.ToStringAndClear();
				GameAnalyticsManager.ErrorSeverity errorSeverity = GameAnalyticsManager.ErrorSeverity.Error;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler7 = new DefaultInterpolatedStringHandler(43, 3);
				defaultInterpolatedStringHandler7.AppendLiteral("Error while loading entity of the type ");
				defaultInterpolatedStringHandler7.AppendFormatted<Type>(type);
				defaultInterpolatedStringHandler7.AppendLiteral(" (");
				defaultInterpolatedStringHandler7.AppendFormatted<Exception>(e3.InnerException);
				defaultInterpolatedStringHandler7.AppendLiteral(")\n");
				defaultInterpolatedStringHandler7.AppendFormatted(Environment.StackTrace.CleanupStackTrace());
				GameAnalyticsManager.AddErrorEventOnce(identifier, errorSeverity, defaultInterpolatedStringHandler7.ToStringAndClear());
			}
			return ic;
		}

		// Token: 0x06005A34 RID: 23092 RVA: 0x002E4EC8 File Offset: 0x002E30C8
		public virtual XElement Save(XElement parentElement)
		{
			XElement componentElement = new XElement(this.name);
			foreach (KeyValuePair<RelatedItem.RelationType, List<RelatedItem>> kvp in this.RequiredItems)
			{
				foreach (RelatedItem ri in kvp.Value)
				{
					XElement newElement = new XElement("requireditem");
					ri.Save(newElement);
					componentElement.Add(newElement);
				}
			}
			foreach (RelatedItem ri2 in this.DisabledRequiredItems)
			{
				XElement newElement2 = new XElement("requireditem");
				if (!ri2.Identifiers.IsEmpty || !this.RequiredItems.Any<KeyValuePair<RelatedItem.RelationType, List<RelatedItem>>>())
				{
					ri2.Save(newElement2);
					componentElement.Add(newElement2);
				}
			}
			SerializableProperty.SerializeProperties(this, componentElement, false, false);
			parentElement.Add(componentElement);
			return componentElement;
		}

		// Token: 0x06005A35 RID: 23093 RVA: 0x002E5010 File Offset: 0x002E3210
		public virtual void Reset()
		{
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, this.originalElement);
			if (this is Pickable)
			{
				this.canBePicked = true;
			}
			this.ParseMsg();
			this.OverrideRequiredItems(this.originalElement);
		}

		// Token: 0x06005A36 RID: 23094 RVA: 0x002E504C File Offset: 0x002E324C
		private void OverrideRequiredItems(ContentXElement element)
		{
			Dictionary<RelatedItem.RelationType, List<RelatedItem>> prevRequiredItems = new Dictionary<RelatedItem.RelationType, List<RelatedItem>>(this.RequiredItems);
			this.RequiredItems.Clear();
			bool returnEmptyRequirements = Screen.Selected == GameMain.SubEditorScreen;
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (a == "requireditem" || a == "requireditems")
				{
					RelatedItem newRequiredItem = RelatedItem.Load(subElement, returnEmptyRequirements, this.item.Name);
					if (newRequiredItem != null)
					{
						RelatedItem prevRequiredItem = prevRequiredItems.ContainsKey(newRequiredItem.Type) ? prevRequiredItems[newRequiredItem.Type].Find((RelatedItem ri) => ri.JoinedIdentifiers == newRequiredItem.JoinedIdentifiers) : null;
						if (prevRequiredItem != null)
						{
							newRequiredItem.StatusEffects = prevRequiredItem.StatusEffects;
							newRequiredItem.Msg = prevRequiredItem.Msg;
							newRequiredItem.IsOptional = prevRequiredItem.IsOptional;
							newRequiredItem.IgnoreInEditor = prevRequiredItem.IgnoreInEditor;
						}
						if (!this.RequiredItems.ContainsKey(newRequiredItem.Type))
						{
							this.RequiredItems[newRequiredItem.Type] = new List<RelatedItem>();
						}
						this.RequiredItems[newRequiredItem.Type].Add(newRequiredItem);
					}
				}
			}
		}

		// Token: 0x06005A37 RID: 23095 RVA: 0x002E520C File Offset: 0x002E340C
		public virtual void ParseMsg()
		{
			LocalizedString msg = TextManager.Get(this.Msg);
			if (msg.Loaded)
			{
				msg = TextManager.ParseInputTypes(msg, false);
				this.DisplayMsg = msg;
				return;
			}
			this.DisplayMsg = this.Msg;
		}

		// Token: 0x06005A38 RID: 23096 RVA: 0x002E524E File Offset: 0x002E344E
		public virtual bool ValidateEventData(NetEntityEvent.IData data)
		{
			return true;
		}

		// Token: 0x06005A39 RID: 23097 RVA: 0x002E5254 File Offset: 0x002E3454
		protected T ExtractEventData<T>(NetEntityEvent.IData data) where T : ItemComponent.IEventData
		{
			T componentData;
			if (!this.TryExtractEventData<T>(data, out componentData))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(111, 4);
				defaultInterpolatedStringHandler.AppendLiteral("Malformed item component state event for ");
				defaultInterpolatedStringHandler.AppendFormatted(this.item.Name);
				defaultInterpolatedStringHandler.AppendLiteral(" ");
				defaultInterpolatedStringHandler.AppendLiteral("(item ID ");
				defaultInterpolatedStringHandler.AppendFormatted<ushort>(this.item.ID);
				defaultInterpolatedStringHandler.AppendLiteral(", component type ");
				defaultInterpolatedStringHandler.AppendFormatted(base.GetType().Name);
				defaultInterpolatedStringHandler.AppendLiteral("): ");
				defaultInterpolatedStringHandler.AppendLiteral("could not extract ComponentData of type ");
				defaultInterpolatedStringHandler.AppendFormatted(typeof(T).Name);
				throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			return componentData;
		}

		// Token: 0x06005A3A RID: 23098 RVA: 0x002E531C File Offset: 0x002E351C
		protected bool TryExtractEventData<T>(NetEntityEvent.IData data, out T componentData)
		{
			componentData = default(T);
			if (data is Item.ComponentStateEventData)
			{
				ItemComponent.IEventData componentData2 = ((Item.ComponentStateEventData)data).ComponentData;
				if (componentData2 is T)
				{
					T nestedData = (T)((object)componentData2);
					componentData = nestedData;
					return true;
				}
			}
			return false;
		}

		// Token: 0x06005A3B RID: 23099 RVA: 0x002E5360 File Offset: 0x002E3560
		protected AIObjectiveContainItem AIContainItems<T>(ItemContainer container, Character character, AIObjective currentObjective, int itemCount, bool equip, bool removeEmpty, bool spawnItemIfNotFound = false, bool dropItemOnDeselected = false) where T : ItemComponent
		{
			AIObjectiveContainItem containObjective = null;
			AIController aicontroller = character.AIController;
			HumanAIController aiController = aicontroller as HumanAIController;
			if (aiController != null)
			{
				containObjective = new AIObjectiveContainItem(character, container.ContainableItemIdentifiers, container, currentObjective.objectiveManager, 1f, spawnItemIfNotFound)
				{
					ItemCount = itemCount,
					Equip = equip,
					RemoveEmpty = removeEmpty,
					GetItemPriority = delegate(Item i)
					{
						Inventory parentInventory = i.ParentInventory;
						if (((parentInventory != null) ? parentInventory.Owner : null) is Item && ((Item)i.ParentInventory.Owner).GetComponent<T>() != null)
						{
							return 0f;
						}
						if (!container.ContainsItemsWithSameIdentifier(i))
						{
							return 0.5f;
						}
						return 1f;
					}
				};
				containObjective.Abandoned += delegate()
				{
					aiController.IgnoredItems.Add(container.Item);
				};
				if (dropItemOnDeselected)
				{
					currentObjective.Deselected += delegate()
					{
						if (containObjective == null)
						{
							return;
						}
						if (containObjective.IsCompleted)
						{
							return;
						}
						Item item = containObjective.ItemToContain;
						if (item != null && character.CanInteractWith(item, false))
						{
							item.Drop(character, true, true);
						}
					};
				}
				currentObjective.AddSubObjective(containObjective, false);
			}
			return containObjective;
		}

		// Token: 0x06005A3F RID: 23103 RVA: 0x002E55F0 File Offset: 0x002E37F0
		[CompilerGenerated]
		private void <.ctor>g__LoadStatusEffect|217_0(ContentXElement subElement, ref ItemComponent.<>c__DisplayClass217_0 A_2)
		{
			StatusEffect statusEffect = StatusEffect.Load(subElement, A_2.item.Name + ", " + base.GetType().Name);
			List<StatusEffect> effectList;
			if (!this.statusEffectLists.TryGetValue(statusEffect.type, out effectList))
			{
				effectList = new List<StatusEffect>();
				this.statusEffectLists.Add(statusEffect.type, effectList);
			}
			effectList.Add(statusEffect);
		}

		// Token: 0x06005A40 RID: 23104 RVA: 0x002E5658 File Offset: 0x002E3858
		[CompilerGenerated]
		internal static void <Combine>g__RemoveItem|238_0(Item item)
		{
			Screen selected = Screen.Selected;
			if (selected != null && selected.IsEditor)
			{
				if (item != null)
				{
					item.Remove();
					return;
				}
			}
			else
			{
				EntitySpawner spawner = Entity.Spawner;
				if (spawner == null)
				{
					return;
				}
				spawner.AddItemToRemoveQueue(item);
			}
		}

		// Token: 0x04002DC3 RID: 11715
		private readonly bool[] hasSoundsOfType;

		// Token: 0x04002DC4 RID: 11716
		private readonly Dictionary<ActionType, List<ItemSound>> sounds;

		// Token: 0x04002DC5 RID: 11717
		private Dictionary<ActionType, SoundSelectionMode> soundSelectionModes;

		// Token: 0x04002DC6 RID: 11718
		protected float correctionTimer;

		// Token: 0x04002DC7 RID: 11719
		public float IsActiveTimer;

		// Token: 0x04002DCD RID: 11725
		private GUIDragHandle guiFrameDragHandle;

		// Token: 0x04002DCE RID: 11726
		private bool guiFrameUpdatePending;

		// Token: 0x04002DD1 RID: 11729
		private ItemComponent linkToUIComponent;

		// Token: 0x04002DD5 RID: 11733
		private bool useAlternativeLayout;

		// Token: 0x04002DD6 RID: 11734
		private bool shouldMuffleLooping;

		// Token: 0x04002DD7 RID: 11735
		private float lastMuffleCheckTime;

		// Token: 0x04002DD8 RID: 11736
		private ItemSound loopingSound;

		// Token: 0x04002DD9 RID: 11737
		private SoundChannel loopingSoundChannel;

		// Token: 0x04002DDA RID: 11738
		private readonly List<SoundChannel> playingOneshotSoundChannels = new List<SoundChannel>();

		// Token: 0x04002DDB RID: 11739
		public ItemComponent ReplacedBy;

		// Token: 0x04002DDC RID: 11740
		private XElement GuiFrameSource;

		// Token: 0x04002DDD RID: 11741
		protected Item item;

		// Token: 0x04002DDE RID: 11742
		protected string name;

		// Token: 0x04002DDF RID: 11743
		private bool isActive;

		// Token: 0x04002DE0 RID: 11744
		protected bool characterUsable;

		// Token: 0x04002DE1 RID: 11745
		protected bool canBePicked;

		// Token: 0x04002DE2 RID: 11746
		protected bool canBeSelected;

		// Token: 0x04002DE3 RID: 11747
		protected bool canBeCombined;

		// Token: 0x04002DE4 RID: 11748
		protected bool removeOnCombined;

		// Token: 0x04002DE5 RID: 11749
		public bool WasUsed;

		// Token: 0x04002DE6 RID: 11750
		public bool WasSecondaryUsed;

		// Token: 0x04002DE7 RID: 11751
		public readonly Dictionary<ActionType, List<StatusEffect>> statusEffectLists;

		// Token: 0x04002DE8 RID: 11752
		public Dictionary<RelatedItem.RelationType, List<RelatedItem>> RequiredItems;

		// Token: 0x04002DE9 RID: 11753
		public readonly List<RelatedItem> DisabledRequiredItems = new List<RelatedItem>();

		// Token: 0x04002DEA RID: 11754
		public readonly List<Skill> RequiredSkills = new List<Skill>();

		// Token: 0x04002DEB RID: 11755
		private ItemComponent parent;

		// Token: 0x04002DED RID: 11757
		public readonly ContentXElement originalElement;

		// Token: 0x04002DEE RID: 11758
		protected const float CorrectionDelay = 1f;

		// Token: 0x04002DEF RID: 11759
		protected CoroutineHandle delayedCorrectionCoroutine;

		// Token: 0x04002DF3 RID: 11763
		public Action<bool> OnActiveStateChanged;

		// Token: 0x04002DF4 RID: 11764
		private bool drawable = true;

		// Token: 0x04002DF6 RID: 11766
		public List<PropertyConditional> IsActiveConditionals;

		// Token: 0x04002E02 RID: 11778
		public readonly NamedEvent<ItemComponent.ItemUseInfo> OnUsed = new NamedEvent<ItemComponent.ItemUseInfo>();

		// Token: 0x04002E03 RID: 11779
		public readonly bool InheritStatusEffects;

		// Token: 0x04002E05 RID: 11781
		protected const float AIUpdateInterval = 0.2f;

		// Token: 0x04002E06 RID: 11782
		protected float aiUpdateTimer;

		// Token: 0x020013B5 RID: 5045
		public class GUILayoutSettings
		{
			// Token: 0x17001D1D RID: 7453
			// (get) Token: 0x06009822 RID: 38946 RVA: 0x003DCF0F File Offset: 0x003DB10F
			// (set) Token: 0x06009823 RID: 38947 RVA: 0x003DCF17 File Offset: 0x003DB117
			public Vector2? RelativeSize { get; private set; }

			// Token: 0x17001D1E RID: 7454
			// (get) Token: 0x06009824 RID: 38948 RVA: 0x003DCF20 File Offset: 0x003DB120
			// (set) Token: 0x06009825 RID: 38949 RVA: 0x003DCF28 File Offset: 0x003DB128
			public Point? AbsoluteSize { get; private set; }

			// Token: 0x17001D1F RID: 7455
			// (get) Token: 0x06009826 RID: 38950 RVA: 0x003DCF31 File Offset: 0x003DB131
			// (set) Token: 0x06009827 RID: 38951 RVA: 0x003DCF39 File Offset: 0x003DB139
			public Vector2? RelativeOffset { get; private set; }

			// Token: 0x17001D20 RID: 7456
			// (get) Token: 0x06009828 RID: 38952 RVA: 0x003DCF42 File Offset: 0x003DB142
			// (set) Token: 0x06009829 RID: 38953 RVA: 0x003DCF4A File Offset: 0x003DB14A
			public Point? AbsoluteOffset { get; private set; }

			// Token: 0x17001D21 RID: 7457
			// (get) Token: 0x0600982A RID: 38954 RVA: 0x003DCF53 File Offset: 0x003DB153
			// (set) Token: 0x0600982B RID: 38955 RVA: 0x003DCF5B File Offset: 0x003DB15B
			public Anchor? Anchor { get; private set; }

			// Token: 0x17001D22 RID: 7458
			// (get) Token: 0x0600982C RID: 38956 RVA: 0x003DCF64 File Offset: 0x003DB164
			// (set) Token: 0x0600982D RID: 38957 RVA: 0x003DCF6C File Offset: 0x003DB16C
			public Pivot? Pivot { get; private set; }

			// Token: 0x0600982E RID: 38958 RVA: 0x003DCF78 File Offset: 0x003DB178
			public static ItemComponent.GUILayoutSettings Load(XElement element)
			{
				ItemComponent.GUILayoutSettings layout = new ItemComponent.GUILayoutSettings();
				Vector2 relativeSize = element.GetAttributeVector2("relativesize", Vector2.Zero);
				Point absoluteSize = element.GetAttributePoint("absolutesize", new Point(-1000, -1000));
				Vector2 relativeOffset = element.GetAttributeVector2("relativeoffset", Vector2.Zero);
				Point absoluteOffset = element.GetAttributePoint("absoluteoffset", new Point(-1000, -1000));
				if (relativeSize.Length() > 0f)
				{
					layout.RelativeSize = new Vector2?(relativeSize);
				}
				if (absoluteSize.X > 0 && absoluteSize.Y > 0)
				{
					layout.AbsoluteSize = new Point?(absoluteSize);
				}
				if (relativeOffset.Length() > 0f)
				{
					layout.RelativeOffset = new Vector2?(relativeOffset);
				}
				if (absoluteOffset.X > -1000 && absoluteOffset.Y > -1000)
				{
					layout.AbsoluteOffset = new Point?(absoluteOffset);
				}
				Anchor a;
				if (Enum.TryParse<Anchor>(element.GetAttributeString("anchor", ""), out a))
				{
					layout.Anchor = new Anchor?(a);
				}
				Pivot p;
				if (Enum.TryParse<Pivot>(element.GetAttributeString("pivot", ""), out p))
				{
					layout.Pivot = new Pivot?(p);
				}
				return layout;
			}

			// Token: 0x0600982F RID: 38959 RVA: 0x003DD0AC File Offset: 0x003DB2AC
			public void ApplyTo(RectTransform target)
			{
				if (this.RelativeOffset != null)
				{
					target.RelativeOffset = this.RelativeOffset.Value;
				}
				else if (this.AbsoluteOffset != null)
				{
					target.AbsoluteOffset = this.AbsoluteOffset.Value;
				}
				if (this.RelativeSize != null)
				{
					target.RelativeSize = this.RelativeSize.Value;
				}
				else if (this.AbsoluteSize != null)
				{
					target.NonScaledSize = this.AbsoluteSize.Value;
				}
				if (this.Anchor != null)
				{
					target.Anchor = this.Anchor.Value;
				}
				if (this.Pivot != null)
				{
					target.Pivot = this.Pivot.Value;
				}
				else
				{
					target.Pivot = RectTransform.MatchPivotToAnchor(target.Anchor);
				}
				target.RecalculateChildren(true, true);
			}
		}

		// Token: 0x020013B6 RID: 5046
		public readonly struct ItemUseInfo : IEquatable<ItemComponent.ItemUseInfo>
		{
			// Token: 0x06009831 RID: 38961 RVA: 0x003DD1B8 File Offset: 0x003DB3B8
			public ItemUseInfo(Item Item, Character User)
			{
				this.Item = Item;
				this.User = User;
			}

			// Token: 0x17001D23 RID: 7459
			// (get) Token: 0x06009832 RID: 38962 RVA: 0x003DD1C8 File Offset: 0x003DB3C8
			// (set) Token: 0x06009833 RID: 38963 RVA: 0x003DD1D0 File Offset: 0x003DB3D0
			public Item Item { get; set; }

			// Token: 0x17001D24 RID: 7460
			// (get) Token: 0x06009834 RID: 38964 RVA: 0x003DD1D9 File Offset: 0x003DB3D9
			// (set) Token: 0x06009835 RID: 38965 RVA: 0x003DD1E1 File Offset: 0x003DB3E1
			public Character User { get; set; }

			// Token: 0x06009836 RID: 38966 RVA: 0x003DD1EC File Offset: 0x003DB3EC
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("ItemUseInfo");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06009837 RID: 38967 RVA: 0x003DD238 File Offset: 0x003DB438
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Item = ");
				builder.Append(this.Item);
				builder.Append(", User = ");
				builder.Append(this.User);
				return true;
			}

			// Token: 0x06009838 RID: 38968 RVA: 0x003DD26D File Offset: 0x003DB46D
			[CompilerGenerated]
			public static bool operator !=(ItemComponent.ItemUseInfo left, ItemComponent.ItemUseInfo right)
			{
				return !(left == right);
			}

			// Token: 0x06009839 RID: 38969 RVA: 0x003DD279 File Offset: 0x003DB479
			[CompilerGenerated]
			public static bool operator ==(ItemComponent.ItemUseInfo left, ItemComponent.ItemUseInfo right)
			{
				return left.Equals(right);
			}

			// Token: 0x0600983A RID: 38970 RVA: 0x003DD283 File Offset: 0x003DB483
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return EqualityComparer<Item>.Default.GetHashCode(this.<Item>k__BackingField) * -1521134295 + EqualityComparer<Character>.Default.GetHashCode(this.<User>k__BackingField);
			}

			// Token: 0x0600983B RID: 38971 RVA: 0x003DD2AC File Offset: 0x003DB4AC
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is ItemComponent.ItemUseInfo && this.Equals((ItemComponent.ItemUseInfo)obj);
			}

			// Token: 0x0600983C RID: 38972 RVA: 0x003DD2C4 File Offset: 0x003DB4C4
			[CompilerGenerated]
			public bool Equals(ItemComponent.ItemUseInfo other)
			{
				return EqualityComparer<Item>.Default.Equals(this.<Item>k__BackingField, other.<Item>k__BackingField) && EqualityComparer<Character>.Default.Equals(this.<User>k__BackingField, other.<User>k__BackingField);
			}

			// Token: 0x0600983D RID: 38973 RVA: 0x003DD2F6 File Offset: 0x003DB4F6
			[CompilerGenerated]
			public void Deconstruct(out Item Item, out Character User)
			{
				Item = this.Item;
				User = this.User;
			}
		}

		// Token: 0x020013B7 RID: 5047
		public interface IEventData
		{
		}
	}
}
