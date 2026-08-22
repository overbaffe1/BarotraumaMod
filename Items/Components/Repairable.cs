using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Networking;
using Barotrauma.Particles;
using Barotrauma.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005D2 RID: 1490
	internal class Repairable : ItemComponent, IDrawableComponent, IServerSerializable, INetSerializable, IClientSerializable
	{
		// Token: 0x170017F6 RID: 6134
		// (get) Token: 0x06005F08 RID: 24328 RVA: 0x003180E0 File Offset: 0x003162E0
		// (set) Token: 0x06005F09 RID: 24329 RVA: 0x003180E8 File Offset: 0x003162E8
		public GUIButton RepairButton { get; private set; }

		// Token: 0x170017F7 RID: 6135
		// (get) Token: 0x06005F0A RID: 24330 RVA: 0x003180F1 File Offset: 0x003162F1
		// (set) Token: 0x06005F0B RID: 24331 RVA: 0x003180F9 File Offset: 0x003162F9
		public GUIButton SabotageButton { get; private set; }

		// Token: 0x170017F8 RID: 6136
		// (get) Token: 0x06005F0C RID: 24332 RVA: 0x00318102 File Offset: 0x00316302
		// (set) Token: 0x06005F0D RID: 24333 RVA: 0x0031810A File Offset: 0x0031630A
		public GUIButton TinkerButton { get; private set; }

		// Token: 0x170017F9 RID: 6137
		// (get) Token: 0x06005F0E RID: 24334 RVA: 0x00318113 File Offset: 0x00316313
		// (set) Token: 0x06005F0F RID: 24335 RVA: 0x0031811B File Offset: 0x0031631B
		[Serialize("", IsPropertySaveable.No, "An optional description of the needed repairs displayed in the repair interface.", "", false)]
		public string Description { get; set; }

		// Token: 0x170017FA RID: 6138
		// (get) Token: 0x06005F10 RID: 24336 RVA: 0x00318124 File Offset: 0x00316324
		public Vector2 DrawSize
		{
			get
			{
				return Vector2.Zero;
			}
		}

		// Token: 0x06005F11 RID: 24337 RVA: 0x0031812C File Offset: 0x0031632C
		protected override bool ShouldDrawHUDComponentSpecific(Character character)
		{
			return !this.item.IsHidden && this.HasRequiredItems(character, false, null) && character.SelectedItem == this.item && ((character.IsTraitor && this.item.ConditionPercentage > this.MinSabotageCondition) || this.item.ConditionPercentageRelativeToDefaultMaxCondition < this.RepairThreshold || (this.CurrentFixer == character && this.item.Condition < this.item.MaxCondition) || this.IsTinkerable(character));
		}

		// Token: 0x06005F12 RID: 24338 RVA: 0x003181C4 File Offset: 0x003163C4
		private void RecreateGUI()
		{
			if (base.GuiFrame != null)
			{
				base.GuiFrame.ClearChildren();
				base.TryCreateDragHandle();
				this.CreateGUI();
			}
		}

		// Token: 0x06005F13 RID: 24339 RVA: 0x003181E8 File Offset: 0x003163E8
		protected override void CreateGUI()
		{
			GUILayoutGroup paddedFrame = new GUILayoutGroup(new RectTransform(new Vector2(0.8f, 0.75f), base.GuiFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopCenter)
			{
				Stretch = true,
				RelativeSpacing = 0.05f,
				CanBeFocused = true
			};
			RectTransform rectT = new RectTransform(new Vector2(1f, 0.15f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = this.header;
			GUIFont font = GUIStyle.LargeFont;
			new GUITextBlock(rectT, text, null, font, Alignment.TopCenter, false, "", null);
			RectTransform rectT2 = new RectTransform(new Vector2(1f, 0f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = this.Description;
			font = GUIStyle.SmallFont;
			new GUITextBlock(rectT2, text2, null, font, Alignment.Left, true, "", null);
			RectTransform rectT3 = new RectTransform(new Vector2(1f, 0f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text3 = TextManager.Get("RequiredRepairSkills");
			font = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT3, text3, null, font, Alignment.Left, false, "", null);
			this.skillTextContainer = paddedFrame;
			for (int i = 0; i < this.RequiredSkills.Count; i++)
			{
				RectTransform rectT4 = new RectTransform(new Vector2(1f, 0f), this.skillTextContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text4 = "   - " + TextManager.AddPunctuation(':', new LocalizedString[]
				{
					TextManager.Get("SkillName." + this.RequiredSkills[i].Identifier.ToString()),
					((int)Math.Round((double)(this.RequiredSkills[i].Level * this.SkillRequirementMultiplier))).ToString()
				});
				font = GUIStyle.SmallFont;
				new GUITextBlock(rectT4, text4, null, font, Alignment.Left, false, "", null).UserData = this.RequiredSkills[i];
			}
			GUILayoutGroup progressBarHolder = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.2f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.02f
			};
			this.progressBar = new GUIProgressBar(new RectTransform(new Vector2(0.6f, 1f), progressBarHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), 0f, new Color?(GUIStyle.Green), "DeviceProgressBar", true);
			RectTransform rectT5 = new RectTransform(Vector2.One, this.progressBar.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text5 = string.Empty;
			font = GUIStyle.SubHeadingFont;
			this.progressBarOverlayText = new GUITextBlock(rectT5, text5, null, font, Alignment.Center, false, "", null)
			{
				IgnoreLayoutGroups = true
			};
			this.qteTimer = 0.5f;
			this.repairButtonText = TextManager.Get("RepairButton");
			this.repairingText = TextManager.Get("Repairing");
			this.RepairButton = new GUIButton(new RectTransform(new Vector2(0.4f, 1f), progressBarHolder.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), this.repairButtonText, Alignment.Center, "", null)
			{
				UserData = UIHighlightAction.ElementId.RepairButton,
				OnClicked = delegate(GUIButton btn, object obj)
				{
					this.requestStartFixAction = Repairable.FixActions.Repair;
					this.item.CreateClientEvent<Repairable>(this);
					return true;
				},
				OnButtonDown = delegate()
				{
					this.QTEAction();
					return true;
				}
			};
			this.RepairButton.TextBlock.AutoScaleHorizontal = true;
			progressBarHolder.RectTransform.MinSize = this.RepairButton.RectTransform.MinSize;
			this.RepairButton.RectTransform.MinSize = new Point((int)(this.RepairButton.TextBlock.TextSize.X * 1.2f), this.RepairButton.RectTransform.MinSize.Y);
			this.extraButtonContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.15f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				IgnoreLayoutGroups = true,
				Stretch = true,
				AbsoluteSpacing = GUI.IntScale(5f)
			};
			this.sabotageButtonText = TextManager.Get("SabotageButton");
			this.sabotagingText = TextManager.Get("Sabotaging");
			this.SabotageButton = new GUIButton(new RectTransform(Vector2.One, this.extraButtonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), this.sabotageButtonText, Alignment.Center, "GUIButtonSmall", null)
			{
				IgnoreLayoutGroups = true,
				Visible = false,
				OnClicked = delegate(GUIButton btn, object obj)
				{
					this.requestStartFixAction = Repairable.FixActions.Sabotage;
					this.item.CreateClientEvent<Repairable>(this);
					return true;
				},
				OnButtonDown = delegate()
				{
					this.QTEAction();
					return true;
				}
			};
			this.tinkerButtonText = TextManager.Get("TinkerButton").Fallback("Tinker", true);
			this.tinkeringText = TextManager.Get("Tinkering").Fallback("Tinkering", true);
			this.TinkerButton = new GUIButton(new RectTransform(Vector2.One, this.extraButtonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), this.tinkerButtonText, Alignment.Center, "GUIButtonSmall", null)
			{
				IgnoreLayoutGroups = true,
				Visible = false,
				OnClicked = delegate(GUIButton btn, object obj)
				{
					this.requestStartFixAction = Repairable.FixActions.Tinker;
					this.item.CreateClientEvent<Repairable>(this);
					return true;
				}
			};
			this.extraButtonContainer.RectTransform.MinSize = new Point(0, this.SabotageButton.RectTransform.MinSize.Y);
		}

		// Token: 0x06005F14 RID: 24340 RVA: 0x003188EC File Offset: 0x00316AEC
		public override void DrawHUD(SpriteBatch spriteBatch, Character character)
		{
			base.DrawHUD(spriteBatch, character);
			this.IsActive = true;
			float defaultMaxCondition = this.item.MaxCondition / this.item.MaxRepairConditionMultiplier;
			this.progressBar.BarSize = this.item.Condition / defaultMaxCondition;
			this.progressBar.Color = ToolBox.GradientLerp(this.progressBar.BarSize, new Color[]
			{
				GUIStyle.Red,
				GUIStyle.Orange,
				GUIStyle.Green
			});
			Rectangle sliderRect = this.progressBar.GetSliderRect(1f);
			Color qteSliderColor = Color.White;
			if (this.qteCooldown > 0f)
			{
				qteSliderColor = (this.qteSuccess ? GUIStyle.Green : (GUIStyle.Red * 0.5f));
				this.progressBar.Color = ToolBox.GradientLerp(this.qteCooldown / 0.5f, new Color[]
				{
					this.progressBar.Color,
					qteSliderColor,
					Color.White
				});
			}
			else if (this.qteTimer / 0.5f <= this.item.Condition / this.item.MaxCondition)
			{
				qteSliderColor = Color.Lerp(qteSliderColor, GUIStyle.Green, 0.5f);
			}
			this.progressBar.Parent.Parent.Parent.DrawManually(spriteBatch, true, true);
			GUI.DrawRectangle(spriteBatch, new Rectangle(sliderRect.X + (int)(this.qteTimer / 0.5f * (float)sliderRect.Width), sliderRect.Y - 5, 2, sliderRect.Height + 10), qteSliderColor, true, 0f, 1f);
			if (this.item.Condition > defaultMaxCondition)
			{
				float extraCondition = this.item.MaxCondition * (this.item.MaxRepairConditionMultiplier - 1f);
				this.progressBar.Color = ToolBox.GradientLerp((this.item.Condition - defaultMaxCondition) / extraCondition, new Color[]
				{
					GUIStyle.ColorReputationHigh,
					GUIStyle.ColorReputationVeryHigh
				});
				this.progressBarOverlayText.Visible = true;
				GUITextBlock guitextBlock = this.progressBarOverlayText;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 1);
				defaultInterpolatedStringHandler.AppendFormatted<int>((int)Math.Round((double)(this.item.Condition / defaultMaxCondition * 100f)));
				defaultInterpolatedStringHandler.AppendLiteral("%");
				guitextBlock.Text = defaultInterpolatedStringHandler.ToStringAndClear();
			}
			else
			{
				this.progressBarOverlayText.Visible = false;
			}
			this.RepairButton.Enabled = ((this.currentFixerAction == Repairable.FixActions.None || this.CurrentFixer == character) && !this.item.IsFullCondition);
			this.RepairButton.Text = ((this.currentFixerAction == Repairable.FixActions.None || this.CurrentFixer != character || this.currentFixerAction != Repairable.FixActions.Repair) ? this.repairButtonText : (this.repairingText + new string('.', (int)(Timing.TotalTime * 2.0) % 3 + 1)));
			this.SabotageButton.Visible = character.IsTraitor;
			this.SabotageButton.IgnoreLayoutGroups = !this.SabotageButton.Visible;
			this.SabotageButton.Enabled = ((this.currentFixerAction == Repairable.FixActions.None || (this.CurrentFixer == character && this.currentFixerAction != Repairable.FixActions.Sabotage)) && character.IsTraitor && this.item.ConditionPercentage > this.MinSabotageCondition);
			this.SabotageButton.Text = ((this.currentFixerAction == Repairable.FixActions.None || this.CurrentFixer != character || this.currentFixerAction != Repairable.FixActions.Sabotage || !character.IsTraitor) ? this.sabotageButtonText : (this.sabotagingText + new string('.', (int)(Timing.TotalTime * 2.0) % 3 + 1)));
			this.TinkerButton.Visible = this.IsTinkerable(character);
			this.TinkerButton.IgnoreLayoutGroups = !this.TinkerButton.Visible;
			this.TinkerButton.Enabled = ((this.currentFixerAction == Repairable.FixActions.None || (this.CurrentFixer == character && this.currentFixerAction != Repairable.FixActions.Tinker)) && this.CanTinker(character));
			this.TinkerButton.Text = ((this.currentFixerAction == Repairable.FixActions.None || this.CurrentFixer != character || this.currentFixerAction != Repairable.FixActions.Tinker) ? this.tinkerButtonText : (this.tinkeringText + new string('.', (int)(Timing.TotalTime * 2.0) % 3 + 1)));
			this.extraButtonContainer.Visible = (this.SabotageButton.Visible || this.TinkerButton.Visible);
			this.extraButtonContainer.IgnoreLayoutGroups = !this.extraButtonContainer.Visible;
			foreach (GUIComponent c in this.skillTextContainer.Children)
			{
				Skill skill = c.UserData as Skill;
				if (skill != null)
				{
					GUITextBlock textBlock = (GUITextBlock)c;
					textBlock.TextColor = ((character.GetSkillLevel(skill.Identifier) < skill.Level * this.SkillRequirementMultiplier) ? GUIStyle.Red : GUIStyle.TextColorNormal);
				}
			}
		}

		// Token: 0x06005F15 RID: 24341 RVA: 0x00318E70 File Offset: 0x00317070
		public void Draw(SpriteBatch spriteBatch, bool editing, float itemDepth = -1f, Color? overrideColor = null)
		{
			if (GameMain.DebugDraw)
			{
				Character controlled = Character.Controlled;
				if (((controlled != null) ? controlled.FocusedItem : null) == this.item)
				{
					bool paused = !this.ShouldDeteriorate() && this.ForceDeteriorationTimer <= 0f;
					if (this.ForceDeteriorationTimer > 0f)
					{
						GUI.DrawString(spriteBatch, new Vector2(this.item.DrawPosition.X, -this.item.DrawPosition.Y), "Forced deterioration for " + ((int)this.ForceDeteriorationTimer).ToString() + " s", Color.Red, new Color?(Color.Black * 0.5f), 0, null, ForceUpperCase.Inherit);
					}
					else if (this.deteriorationTimer > 0f)
					{
						GUI.DrawString(spriteBatch, new Vector2(this.item.DrawPosition.X, -this.item.DrawPosition.Y), "Deterioration delay " + ((int)this.deteriorationTimer).ToString() + (paused ? " [PAUSED]" : ""), paused ? Color.Cyan : Color.Lime, new Color?(Color.Black * 0.5f), 0, null, ForceUpperCase.Inherit);
					}
					else
					{
						GUI.DrawString(spriteBatch, new Vector2(this.item.DrawPosition.X, -this.item.DrawPosition.Y), "Deteriorating at " + ((int)(this.DeteriorationSpeed * 60f)).ToString() + " units/min" + (paused ? " [PAUSED]" : ""), paused ? Color.Cyan : GUIStyle.Red, new Color?(Color.Black * 0.5f), 0, null, ForceUpperCase.Inherit);
					}
					GUI.DrawString(spriteBatch, new Vector2(this.item.DrawPosition.X, -this.item.DrawPosition.Y + 20f), "Condition: " + ((int)this.item.Condition).ToString() + "/" + ((int)this.item.MaxCondition).ToString(), GUIStyle.Orange, null, 0, null, ForceUpperCase.Inherit);
					if (this.MaxStressDeteriorationMultiplier > 1f)
					{
						GUI.DrawString(spriteBatch, new Vector2(this.item.DrawPosition.X, -this.item.DrawPosition.Y + 40f), "Stress multiplier: " + this.StressDeteriorationMultiplier.ToString("0.00"), GUIStyle.Red, null, 0, null, ForceUpperCase.Inherit);
					}
				}
			}
		}

		// Token: 0x06005F16 RID: 24342 RVA: 0x0031913E File Offset: 0x0031733E
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			SoundChannel soundChannel = this.repairSoundChannel;
			if (soundChannel != null)
			{
				soundChannel.FadeOutAndDispose();
			}
			this.repairSoundChannel = null;
		}

		// Token: 0x06005F17 RID: 24343 RVA: 0x00319160 File Offset: 0x00317360
		private void QTEAction()
		{
			if (this.currentFixerAction == Repairable.FixActions.Repair)
			{
				float defaultMaxCondition = this.item.MaxCondition / this.item.MaxRepairConditionMultiplier;
				this.qteSuccess = (this.qteCooldown <= 0f && this.qteTimer / 0.5f <= this.item.Condition / defaultMaxCondition);
				if (!GameMain.IsMultiplayer)
				{
					this.RepairBoost(this.qteSuccess);
				}
				SoundPlayer.PlayUISound(this.qteSuccess ? GUISoundType.Increase : GUISoundType.Decrease);
				if (!this.qteSuccess && this.qteCooldown > 0f)
				{
					this.qteTimer = 0.5f;
				}
				this.qteCooldown = 0.5f;
				this.requestStartFixAction = Repairable.FixActions.None;
				this.item.CreateClientEvent<Repairable>(this);
				return;
			}
		}

		// Token: 0x06005F18 RID: 24344 RVA: 0x0031922C File Offset: 0x0031742C
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			this.deteriorationTimer = msg.ReadSingle();
			this.ForceDeteriorationTimer = msg.ReadSingle();
			this.tinkeringDuration = msg.ReadSingle();
			this.tinkeringStrength = msg.ReadSingle();
			this.tinkeringPowersDevices = msg.ReadBoolean();
			ushort currentFixerID = msg.ReadUInt16();
			this.currentFixerAction = (Repairable.FixActions)msg.ReadRangedInteger(0, 2);
			this.CurrentFixer = ((currentFixerID != 0) ? (Entity.FindEntityByID(currentFixerID) as Character) : null);
			if (this.CurrentFixer == null)
			{
				this.qteTimer = 0.5f;
				this.qteCooldown = 0f;
				return;
			}
			this.item.MaxRepairConditionMultiplier = this.GetMaxRepairConditionMultiplier(this.CurrentFixer);
		}

		// Token: 0x06005F19 RID: 24345 RVA: 0x003192D7 File Offset: 0x003174D7
		public void ClientEventWrite(IWriteMessage msg, NetEntityEvent.IData extraData = null)
		{
			msg.WriteRangedInteger((int)this.requestStartFixAction, 0, 2);
			msg.WriteBoolean(this.qteSuccess);
		}

		// Token: 0x170017FB RID: 6139
		// (get) Token: 0x06005F1A RID: 24346 RVA: 0x003192F3 File Offset: 0x003174F3
		// (set) Token: 0x06005F1B RID: 24347 RVA: 0x003192FB File Offset: 0x003174FB
		public float ForceDeteriorationTimer { get; private set; }

		// Token: 0x170017FC RID: 6140
		// (get) Token: 0x06005F1C RID: 24348 RVA: 0x00319304 File Offset: 0x00317504
		// (set) Token: 0x06005F1D RID: 24349 RVA: 0x0031930C File Offset: 0x0031750C
		[Serialize(0f, IsPropertySaveable.Yes, "How fast the condition of the item deteriorates per second.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f, DecimalCount = 2)]
		public float DeteriorationSpeed { get; set; }

		// Token: 0x170017FD RID: 6141
		// (get) Token: 0x06005F1E RID: 24350 RVA: 0x00319315 File Offset: 0x00317515
		// (set) Token: 0x06005F1F RID: 24351 RVA: 0x0031931D File Offset: 0x0031751D
		[Serialize(0f, IsPropertySaveable.Yes, "Minimum initial delay before the item starts to deteriorate.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f, DecimalCount = 2)]
		public float MinDeteriorationDelay { get; set; }

		// Token: 0x170017FE RID: 6142
		// (get) Token: 0x06005F20 RID: 24352 RVA: 0x00319326 File Offset: 0x00317526
		// (set) Token: 0x06005F21 RID: 24353 RVA: 0x0031932E File Offset: 0x0031752E
		[Serialize(0f, IsPropertySaveable.Yes, "Maximum initial delay before the item starts to deteriorate.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f, DecimalCount = 2)]
		public float MaxDeteriorationDelay { get; set; }

		// Token: 0x170017FF RID: 6143
		// (get) Token: 0x06005F22 RID: 24354 RVA: 0x00319337 File Offset: 0x00317537
		// (set) Token: 0x06005F23 RID: 24355 RVA: 0x0031933F File Offset: 0x0031753F
		[Serialize(50f, IsPropertySaveable.Yes, "The item won't deteriorate spontaneously if the condition is below this value. For example, if set to 10, the condition will spontaneously drop to 10 and then stop dropping (unless the item is damaged further by external factors). Percentages of max condition.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float MinDeteriorationCondition { get; set; }

		// Token: 0x17001800 RID: 6144
		// (get) Token: 0x06005F24 RID: 24356 RVA: 0x00319348 File Offset: 0x00317548
		// (set) Token: 0x06005F25 RID: 24357 RVA: 0x00319350 File Offset: 0x00317550
		[Serialize(0f, IsPropertySaveable.Yes, "How low a traitor must get the item's condition for it to start breaking down.", "", false)]
		public float MinSabotageCondition { get; set; }

		// Token: 0x17001801 RID: 6145
		// (get) Token: 0x06005F26 RID: 24358 RVA: 0x00319359 File Offset: 0x00317559
		// (set) Token: 0x06005F27 RID: 24359 RVA: 0x00319361 File Offset: 0x00317561
		[Serialize(60f, IsPropertySaveable.Yes, "How long will the item spontaneously deteriorate after being sabotaged.", "", false)]
		public float SabotageDeteriorationDuration { get; set; }

		// Token: 0x17001802 RID: 6146
		// (get) Token: 0x06005F28 RID: 24360 RVA: 0x0031936A File Offset: 0x0031756A
		// (set) Token: 0x06005F29 RID: 24361 RVA: 0x00319372 File Offset: 0x00317572
		[Serialize(80f, IsPropertySaveable.Yes, "The condition of the item has to be below this for it to become repairable. Percentages of max condition.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float RepairThreshold { get; set; }

		// Token: 0x17001803 RID: 6147
		// (get) Token: 0x06005F2A RID: 24362 RVA: 0x0031937B File Offset: 0x0031757B
		// (set) Token: 0x06005F2B RID: 24363 RVA: 0x00319383 File Offset: 0x00317583
		[Serialize(1f, IsPropertySaveable.Yes, "How much faster the device can deteriorate when under stress (e.g. when operating at full speed/power).", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f, DecimalCount = 2)]
		public float MaxStressDeteriorationMultiplier { get; set; }

		// Token: 0x17001804 RID: 6148
		// (get) Token: 0x06005F2C RID: 24364 RVA: 0x0031938C File Offset: 0x0031758C
		// (set) Token: 0x06005F2D RID: 24365 RVA: 0x00319394 File Offset: 0x00317594
		[Serialize(0.5f, IsPropertySaveable.Yes, "At what speed/power must the device be operating at to be considered \"under stress\".", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f, DecimalCount = 2)]
		public float StressDeteriorationThreshold { get; set; }

		// Token: 0x17001805 RID: 6149
		// (get) Token: 0x06005F2E RID: 24366 RVA: 0x0031939D File Offset: 0x0031759D
		// (set) Token: 0x06005F2F RID: 24367 RVA: 0x003193A5 File Offset: 0x003175A5
		[Serialize(0.1f, IsPropertySaveable.Yes, "How fast the deterioration speed increases when under stress.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f, DecimalCount = 2)]
		public float StressDeteriorationIncreaseSpeed { get; set; }

		// Token: 0x17001806 RID: 6150
		// (get) Token: 0x06005F30 RID: 24368 RVA: 0x003193AE File Offset: 0x003175AE
		// (set) Token: 0x06005F31 RID: 24369 RVA: 0x003193B6 File Offset: 0x003175B6
		[Serialize(0.1f, IsPropertySaveable.Yes, "How fast the deterioration speed decreases when not under stress.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f, DecimalCount = 2)]
		public float StressDeteriorationDecreaseSpeed { get; set; }

		// Token: 0x17001807 RID: 6151
		// (get) Token: 0x06005F32 RID: 24370 RVA: 0x003193BF File Offset: 0x003175BF
		// (set) Token: 0x06005F33 RID: 24371 RVA: 0x003193C7 File Offset: 0x003175C7
		[Serialize(100f, IsPropertySaveable.Yes, "The amount of time it takes to fix the item with insufficient skill levels.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float FixDurationLowSkill { get; set; }

		// Token: 0x17001808 RID: 6152
		// (get) Token: 0x06005F34 RID: 24372 RVA: 0x003193D0 File Offset: 0x003175D0
		// (set) Token: 0x06005F35 RID: 24373 RVA: 0x003193D8 File Offset: 0x003175D8
		[Serialize(10f, IsPropertySaveable.Yes, "The amount of time it takes to fix the item with sufficient skill levels.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float FixDurationHighSkill { get; set; }

		// Token: 0x17001809 RID: 6153
		// (get) Token: 0x06005F36 RID: 24374 RVA: 0x003193E1 File Offset: 0x003175E1
		// (set) Token: 0x06005F37 RID: 24375 RVA: 0x003193EC File Offset: 0x003175EC
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		public float SkillRequirementMultiplier
		{
			get
			{
				return this.skillRequirementMultiplier;
			}
			set
			{
				float oldValue = this.skillRequirementMultiplier;
				this.skillRequirementMultiplier = value;
				if (!MathUtils.NearlyEqual(oldValue, this.skillRequirementMultiplier, 0.0001f))
				{
					this.RecreateGUI();
				}
			}
		}

		// Token: 0x1700180A RID: 6154
		// (get) Token: 0x06005F38 RID: 24376 RVA: 0x00319420 File Offset: 0x00317620
		// (set) Token: 0x06005F39 RID: 24377 RVA: 0x00319428 File Offset: 0x00317628
		public bool IsTinkering
		{
			get
			{
				return this.isTinkering;
			}
			private set
			{
				if (this.isTinkering == value)
				{
					return;
				}
				this.isTinkering = value;
				if (this.tinkeringPowersDevices)
				{
					foreach (Powered powered in this.item.GetComponents<Powered>())
					{
						if (!(powered is PowerContainer))
						{
							powered.PoweredByTinkering = this.isTinkering;
						}
					}
				}
			}
		}

		// Token: 0x1700180B RID: 6155
		// (get) Token: 0x06005F3A RID: 24378 RVA: 0x003194A0 File Offset: 0x003176A0
		// (set) Token: 0x06005F3B RID: 24379 RVA: 0x003194A8 File Offset: 0x003176A8
		public Character CurrentFixer { get; private set; }

		// Token: 0x1700180C RID: 6156
		// (get) Token: 0x06005F3C RID: 24380 RVA: 0x003194B1 File Offset: 0x003176B1
		// (set) Token: 0x06005F3D RID: 24381 RVA: 0x003194B9 File Offset: 0x003176B9
		public float StressDeteriorationMultiplier { get; private set; } = 1f;

		// Token: 0x1700180D RID: 6157
		// (get) Token: 0x06005F3E RID: 24382 RVA: 0x003194C2 File Offset: 0x003176C2
		public float TinkeringStrength
		{
			get
			{
				return this.tinkeringStrength;
			}
		}

		// Token: 0x1700180E RID: 6158
		// (get) Token: 0x06005F3F RID: 24383 RVA: 0x003194CA File Offset: 0x003176CA
		public bool TinkeringPowersDevices
		{
			get
			{
				return this.tinkeringPowersDevices;
			}
		}

		// Token: 0x1700180F RID: 6159
		// (get) Token: 0x06005F40 RID: 24384 RVA: 0x003194D2 File Offset: 0x003176D2
		public bool IsBelowRepairThreshold
		{
			get
			{
				return this.item.ConditionPercentageRelativeToDefaultMaxCondition < this.RepairThreshold;
			}
		}

		// Token: 0x17001810 RID: 6160
		// (get) Token: 0x06005F41 RID: 24385 RVA: 0x003194E7 File Offset: 0x003176E7
		public bool IsBelowRepairIconThreshold
		{
			get
			{
				return this.item.ConditionPercentageRelativeToDefaultMaxCondition < this.RepairThreshold / 2f;
			}
		}

		// Token: 0x17001811 RID: 6161
		// (get) Token: 0x06005F42 RID: 24386 RVA: 0x00319502 File Offset: 0x00317702
		// (set) Token: 0x06005F43 RID: 24387 RVA: 0x0031950A File Offset: 0x0031770A
		public Repairable.FixActions CurrentFixerAction
		{
			get
			{
				return this.currentFixerAction;
			}
			private set
			{
				this.currentFixerAction = value;
			}
		}

		// Token: 0x06005F44 RID: 24388 RVA: 0x00319514 File Offset: 0x00317714
		public Repairable(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
			this.canBeSelected = true;
			this.item = item;
			this.header = TextManager.Get(element.GetAttributeString("header", "")).Fallback(TextManager.Get(item.Prefab.ConfigElement.GetAttributeString("header", "")), true).Fallback(element.GetAttributeString("name", ""), true);
			XAttribute xattribute;
			if ((xattribute = element.Attributes().FirstOrDefault((XAttribute a) => a.Name.ToString().Equals("showrepairuithreshold", StringComparison.OrdinalIgnoreCase))) == null)
			{
				xattribute = element.Attributes().FirstOrDefault((XAttribute a) => a.Name.ToString().Equals("airepairthreshold", StringComparison.OrdinalIgnoreCase));
			}
			XAttribute repairThresholdAttribute = xattribute;
			float repairThreshold;
			if (repairThresholdAttribute != null && float.TryParse(repairThresholdAttribute.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out repairThreshold))
			{
				this.RepairThreshold = repairThreshold;
			}
			GameSession gameSession = GameMain.GameSession;
			CampaignMode campaign = (gameSession != null) ? gameSession.Campaign : null;
			List<StatusEffect> onFailureEffects;
			if (campaign != null && this.statusEffectLists != null && this.statusEffectLists.TryGetValue(ActionType.OnFailure, out onFailureEffects))
			{
				foreach (StatusEffect effect in onFailureEffects)
				{
					foreach (Affliction affliction in effect.Afflictions)
					{
						if (!(affliction.Prefab.AfflictionType == Tags.Stun))
						{
							affliction.Strength *= campaign.Settings.RepairFailMultiplier;
						}
					}
				}
			}
			this.InitProjSpecific(element);
		}

		// Token: 0x06005F45 RID: 24389 RVA: 0x00319720 File Offset: 0x00317920
		public override void OnItemLoaded()
		{
			this.deteriorationTimer = Rand.Range(this.MinDeteriorationDelay, this.MaxDeteriorationDelay, Rand.RandSync.Unsynced);
		}

		// Token: 0x06005F46 RID: 24390 RVA: 0x0031973C File Offset: 0x0031793C
		private void InitProjSpecific(ContentXElement element)
		{
			this.CreateGUI();
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (a == "emitter" || a == "particleemitter")
				{
					this.particleEmitters.Add(new ParticleEmitter(subElement));
					float minCondition = subElement.GetAttributeFloat("mincondition", 0f);
					float maxCondition = subElement.GetAttributeFloat("maxcondition", 100f);
					if (maxCondition < minCondition)
					{
						DebugConsole.ThrowError("Invalid damage particle configuration in the Repairable component of " + this.item.Name + ". MaxCondition needs to be larger than MinCondition.", null, null, false, false);
						float temp = maxCondition;
						maxCondition = minCondition;
						minCondition = temp;
					}
					this.particleEmitterConditionRanges.Add(new Vector2(minCondition, maxCondition));
				}
			}
		}

		// Token: 0x06005F47 RID: 24391 RVA: 0x00319830 File Offset: 0x00317A30
		public bool CheckCharacterSuccess(Character character, Item bestRepairItem)
		{
			if (character == null)
			{
				return false;
			}
			if (this.statusEffectLists == null)
			{
				return true;
			}
			if (bestRepairItem != null && bestRepairItem.Prefab.CannotRepairFail)
			{
				return true;
			}
			if (this.RequiredSkills.Any((Skill s) => s != null && s.Identifier == "electrical"))
			{
				Reactor reactor = this.item.GetComponent<Reactor>();
				if (reactor != null)
				{
					if (MathUtils.NearlyEqual(reactor.CurrPowerConsumption, 0f, 0.1f))
					{
						return true;
					}
				}
				else
				{
					Powered powered = this.item.GetComponent<Powered>();
					if (powered != null && powered.Voltage < 0.1f)
					{
						return true;
					}
				}
			}
			bool success = Rand.Range(0f, 0.5f, Rand.RandSync.Unsynced) < this.RepairDegreeOfSuccess(character, this.RequiredSkills);
			ActionType actionType = success ? ActionType.OnSuccess : ActionType.OnFailure;
			Repairable.<CheckCharacterSuccess>g__ApplyStatusEffectsAndCreateEntityEvent|148_1(this, actionType, character);
			Repairable.<CheckCharacterSuccess>g__ApplyStatusEffectsAndCreateEntityEvent|148_1(this, ActionType.OnUse, character);
			if (bestRepairItem != null)
			{
				Holdable holdable = bestRepairItem.GetComponent<Holdable>();
				if (holdable != null)
				{
					Repairable.<CheckCharacterSuccess>g__ApplyStatusEffectsAndCreateEntityEvent|148_1(holdable, actionType, character);
					Repairable.<CheckCharacterSuccess>g__ApplyStatusEffectsAndCreateEntityEvent|148_1(holdable, ActionType.OnUse, character);
				}
			}
			return success;
		}

		// Token: 0x06005F48 RID: 24392 RVA: 0x0031992B File Offset: 0x00317B2B
		public override float GetSkillMultiplier()
		{
			return this.SkillRequirementMultiplier;
		}

		// Token: 0x06005F49 RID: 24393 RVA: 0x00319934 File Offset: 0x00317B34
		public float RepairDegreeOfSuccess(Character character, List<Skill> skills)
		{
			if (skills.Count == 0)
			{
				return 1f;
			}
			if (character == null)
			{
				return 0f;
			}
			float skillSum = (from t in skills
			let characterLevel = character.GetSkillLevel(t.Identifier)
			select characterLevel - t.Level * this.SkillRequirementMultiplier).Sum();
			float average = skillSum / (float)skills.Count;
			return (average + 100f) / 2f / 100f;
		}

		// Token: 0x06005F4A RID: 24394 RVA: 0x003199B8 File Offset: 0x00317BB8
		public void RepairBoost(bool qteSuccess)
		{
			if (this.CurrentFixer == null)
			{
				return;
			}
			if (qteSuccess)
			{
				this.item.Condition += this.RepairDegreeOfSuccess(this.CurrentFixer, this.RequiredSkills) * 3f * ((this.currentFixerAction == Repairable.FixActions.Repair) ? 1f : -1f);
				return;
			}
			if (Rand.Range(0f, 2f, Rand.RandSync.Unsynced) > this.RepairDegreeOfSuccess(this.CurrentFixer, this.RequiredSkills))
			{
				base.ApplyStatusEffects(ActionType.OnFailure, 1f, this.CurrentFixer, null, null, null, null, 1f);
			}
		}

		// Token: 0x06005F4B RID: 24395 RVA: 0x00319A5C File Offset: 0x00317C5C
		public bool StartRepairing(Character character, Repairable.FixActions action)
		{
			if (character == null || character.IsDead || action == Repairable.FixActions.None)
			{
				DebugConsole.ThrowError("Invalid repair command!", null, null, false, false);
				return false;
			}
			if (this.CurrentFixerAction == Repairable.FixActions.Tinker && action != Repairable.FixActions.Tinker)
			{
				Character currentFixer = this.CurrentFixer;
				if (currentFixer != null)
				{
					currentFixer.CheckTalents(AbilityEffectType.OnStopTinkering);
				}
			}
			Item bestRepairItem = Repairable.<StartRepairing>g__GetBestRepairItem|152_0(character);
			if (GameMain.Client == null && (this.CurrentFixer != character || this.currentFixerAction != action) && !this.CheckCharacterSuccess(character, bestRepairItem))
			{
				return false;
			}
			this.CurrentFixer = character;
			this.currentRepairItem = bestRepairItem;
			this.CurrentFixerAction = action;
			if (action == Repairable.FixActions.Tinker)
			{
				this.tinkeringStrength = 1f + this.CurrentFixer.GetStatValue(StatTypes.TinkeringStrength, true);
				this.tinkeringPowersDevices = this.CurrentFixer.HasAbilityFlag(AbilityFlags.TinkeringPowersDevices);
				if ((character.HasAbilityFlag(AbilityFlags.CanTinkerFabricatorsAndDeconstructors) && this.item.GetComponent<Deconstructor>() != null) || this.item.GetComponent<Fabricator>() != null)
				{
					this.tinkeringDuration = float.MaxValue;
				}
				else
				{
					this.tinkeringDuration = this.CurrentFixer.GetStatValue(StatTypes.TinkeringDuration, true);
				}
			}
			return true;
		}

		// Token: 0x06005F4C RID: 24396 RVA: 0x00319B60 File Offset: 0x00317D60
		public bool StopRepairing(Character character)
		{
			if (this.CurrentFixer == character)
			{
				if (this.currentRepairItem != null)
				{
					foreach (ItemComponent ic in this.currentRepairItem.GetComponents<ItemComponent>())
					{
						ic.ApplyStatusEffects(ActionType.OnSuccess, 1f, character, null, null, null, null, 1f);
					}
				}
				if (this.CurrentFixerAction == Repairable.FixActions.Tinker)
				{
					this.CurrentFixer.CheckTalents(AbilityEffectType.OnStopTinkering);
				}
				this.CurrentFixer.AnimController.StopUsingItem();
				this.CurrentFixer = null;
				this.currentRepairItem = null;
				this.currentFixerAction = Repairable.FixActions.None;
				this.qteTimer = 0.5f;
				this.qteCooldown = 0f;
				SoundChannel soundChannel = this.repairSoundChannel;
				if (soundChannel != null)
				{
					soundChannel.FadeOutAndDispose();
				}
				this.repairSoundChannel = null;
				return true;
			}
			return false;
		}

		// Token: 0x06005F4D RID: 24397 RVA: 0x00319C4C File Offset: 0x00317E4C
		public override void UpdateBroken(float deltaTime, Camera cam)
		{
			this.Update(deltaTime, cam);
		}

		// Token: 0x06005F4E RID: 24398 RVA: 0x00319C56 File Offset: 0x00317E56
		public void ResetDeterioration()
		{
			this.deteriorationTimer = Rand.Range(this.MinDeteriorationDelay, this.MaxDeteriorationDelay, Rand.RandSync.Unsynced);
			this.item.Condition = this.item.MaxCondition;
		}

		// Token: 0x06005F4F RID: 24399 RVA: 0x00319C88 File Offset: 0x00317E88
		public override void Update(float deltaTime, Camera cam)
		{
			this.UpdateProjSpecific(deltaTime);
			this.IsTinkering = false;
			int condition = (int)(this.item.Condition / (this.item.MaxCondition / this.item.MaxRepairConditionMultiplier) * 100f);
			if (this.prevSentConditionValue != condition || this.conditionSignal == null)
			{
				this.prevSentConditionValue = condition;
				this.conditionSignal = this.prevSentConditionValue.ToString();
			}
			this.item.SendSignal(this.conditionSignal, "condition_out");
			foreach (ItemComponent component in this.item.Components)
			{
				IDeteriorateUnderStress deteriorateUnderStress = component as IDeteriorateUnderStress;
				if (deteriorateUnderStress != null)
				{
					if (deteriorateUnderStress.CurrentStress >= this.StressDeteriorationThreshold)
					{
						this.StressDeteriorationMultiplier = Math.Min(this.StressDeteriorationMultiplier + deltaTime * this.StressDeteriorationIncreaseSpeed, this.MaxStressDeteriorationMultiplier);
					}
					else
					{
						this.StressDeteriorationMultiplier = Math.Max(this.StressDeteriorationMultiplier - deltaTime * this.StressDeteriorationDecreaseSpeed, 1f);
					}
				}
			}
			if (this.ForceDeteriorationTimer > 0f)
			{
				this.ForceDeteriorationTimer -= deltaTime;
				float forceDeteriorationTimer = this.ForceDeteriorationTimer;
			}
			if (this.CurrentFixer == null)
			{
				this.updateDeteriorationCounter++;
				if (this.updateDeteriorationCounter >= 10)
				{
					this.UpdateDeterioration(deltaTime * 10f);
					this.updateDeteriorationCounter = 0;
				}
				return;
			}
			this.UpdateFixAnimation(this.CurrentFixer);
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			if (this.CurrentFixer != null && (this.CurrentFixer.SelectedItem != this.item || !this.CurrentFixer.CanInteractWith(this.item, true) || this.CurrentFixer.IsDead))
			{
				this.StopRepairing(this.CurrentFixer);
				return;
			}
			if (this.currentFixerAction != Repairable.FixActions.Tinker)
			{
				float successFactor = (this.RequiredSkills.Count == 0) ? 1f : this.RepairDegreeOfSuccess(this.CurrentFixer, this.RequiredSkills);
				if (this.IsBelowRepairThreshold)
				{
					this.wasBroken = true;
				}
				if (this.item.ConditionPercentage > this.MinSabotageCondition)
				{
					this.wasGoodCondition = true;
				}
				float talentMultiplier = this.CurrentFixer.GetStatValue(StatTypes.RepairSpeed, true);
				foreach (Skill skill in this.RequiredSkills)
				{
					if (skill.Identifier == "mechanical")
					{
						talentMultiplier += this.CurrentFixer.GetStatValue(StatTypes.MechanicalRepairSpeed, true);
					}
					else if (skill.Identifier == "electrical")
					{
						talentMultiplier += this.CurrentFixer.GetStatValue(StatTypes.ElectricalRepairSpeed, true);
					}
				}
				float fixDuration = MathHelper.Lerp(this.FixDurationLowSkill, this.FixDurationHighSkill, successFactor);
				float num = fixDuration;
				float num2 = 1f + talentMultiplier;
				Item item = this.currentRepairItem;
				fixDuration = num / (num2 + ((item != null) ? new float?(item.Prefab.AddedRepairSpeedMultiplier) : null)).GetValueOrDefault();
				fixDuration /= 1f + this.item.GetQualityModifier(Quality.StatType.RepairSpeed);
				this.item.MaxRepairConditionMultiplier = this.GetMaxRepairConditionMultiplier(this.CurrentFixer);
				if (this.currentFixerAction == Repairable.FixActions.Repair)
				{
					if (fixDuration <= 0f)
					{
						this.item.Condition = this.item.MaxCondition;
					}
					else
					{
						float conditionIncrease = deltaTime / (fixDuration / this.item.Prefab.Health);
						this.item.Condition += conditionIncrease;
					}
					if (this.item.IsFullCondition)
					{
						if (this.wasBroken)
						{
							foreach (Skill skill2 in this.RequiredSkills)
							{
								CharacterInfo info = this.CurrentFixer.Info;
								if (info != null)
								{
									info.ApplySkillGain(skill2.Identifier, SkillSettings.Current.SkillIncreasePerRepair, false, 2f, false);
								}
							}
							AchievementManager.OnItemRepaired(this.item, this.CurrentFixer);
							this.CurrentFixer.CheckTalents(AbilityEffectType.OnRepairComplete, new AbilityRepairable(this.item));
						}
						Character currentFixer = this.CurrentFixer;
						if (((currentFixer != null) ? currentFixer.SelectedItem : null) == this.item)
						{
							this.CurrentFixer.SelectedItem = null;
						}
						this.deteriorationTimer = Rand.Range(this.MinDeteriorationDelay, this.MaxDeteriorationDelay, Rand.RandSync.Unsynced);
						this.wasBroken = false;
						this.StopRepairing(this.CurrentFixer);
						return;
					}
				}
				else
				{
					if (this.currentFixerAction != Repairable.FixActions.Sabotage)
					{
						throw new NotImplementedException(this.currentFixerAction.ToString());
					}
					if (fixDuration <= 0f)
					{
						this.item.Condition = this.item.MaxCondition * (this.MinSabotageCondition / 100f);
					}
					else
					{
						float conditionDecrease = deltaTime / (fixDuration / this.item.Prefab.Health);
						this.item.Condition -= conditionDecrease;
					}
					if (this.item.ConditionPercentage <= this.MinSabotageCondition)
					{
						if (this.wasGoodCondition)
						{
							foreach (Skill skill3 in this.RequiredSkills)
							{
								float characterSkillLevel = this.CurrentFixer.GetSkillLevel(skill3.Identifier);
								CharacterInfo info2 = this.CurrentFixer.Info;
								if (info2 != null)
								{
									info2.IncreaseSkillLevel(skill3.Identifier, SkillSettings.Current.SkillIncreasePerSabotage / Math.Max(characterSkillLevel, 1f), false, false);
								}
							}
							this.deteriorationTimer = 0f;
							this.ForceDeteriorationTimer = this.SabotageDeteriorationDuration;
							this.item.Condition = this.item.MaxCondition * (this.MinSabotageCondition / 100f);
							this.wasGoodCondition = false;
						}
						this.StopRepairing(this.CurrentFixer);
						return;
					}
				}
				return;
			}
			this.tinkeringDuration -= deltaTime;
			float conditionDecrease2 = deltaTime * (this.CurrentFixer.GetStatValue(StatTypes.TinkeringDamage, true) / this.item.Prefab.Health) * 100f;
			this.item.Condition -= conditionDecrease2;
			if (!this.CanTinker(this.CurrentFixer) || this.tinkeringDuration <= 0f)
			{
				this.StopRepairing(this.CurrentFixer);
				return;
			}
			this.IsTinkering = true;
		}

		// Token: 0x06005F50 RID: 24400 RVA: 0x0031A34C File Offset: 0x0031854C
		private void UpdateDeterioration(float deltaTime)
		{
			if (this.item.Condition <= 0f)
			{
				return;
			}
			if (!this.ShouldDeteriorate())
			{
				return;
			}
			if (this.deteriorationTimer > 0f && this.ForceDeteriorationTimer <= 0f)
			{
				if (GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient)
				{
					this.deteriorationTimer -= deltaTime * this.GetDeteriorationDelayMultiplier();
				}
				return;
			}
			if (this.item.ConditionPercentage > this.MinDeteriorationCondition)
			{
				float deteriorationSpeed = this.item.StatManager.GetAdjustedValueMultiplicative(ItemTalentStats.DetoriationSpeed, this.DeteriorationSpeed);
				if (this.ForceDeteriorationTimer > 0f)
				{
					deteriorationSpeed = Math.Max(deteriorationSpeed, 1f);
				}
				this.item.Condition -= deteriorationSpeed * this.StressDeteriorationMultiplier * deltaTime;
			}
		}

		// Token: 0x06005F51 RID: 24401 RVA: 0x0031A418 File Offset: 0x00318618
		private float GetMaxRepairConditionMultiplier(Character character)
		{
			if (character == null)
			{
				return 1f;
			}
			if (this.RequiredSkills.Any((Skill s) => s != null && s.Identifier == "mechanical"))
			{
				return 1f + character.GetStatValue(StatTypes.MaxRepairConditionMultiplierMechanical, true);
			}
			if (this.RequiredSkills.Any((Skill s) => s != null && s.Identifier == "electrical"))
			{
				return 1f + character.GetStatValue(StatTypes.MaxRepairConditionMultiplierElectrical, true);
			}
			return 1f;
		}

		// Token: 0x06005F52 RID: 24402 RVA: 0x0031A4AC File Offset: 0x003186AC
		private bool IsTinkerable(Character character)
		{
			return character.HasAbilityFlag(AbilityFlags.CanTinker) && (this.item.GetComponent<Engine>() != null || this.item.GetComponent<Pump>() != null || this.item.HasTag(Tags.TurretAmmoSource) || (character.HasAbilityFlag(AbilityFlags.CanTinkerFabricatorsAndDeconstructors) && (this.item.GetComponent<Fabricator>() != null || this.item.GetComponent<Deconstructor>() != null)));
		}

		// Token: 0x06005F53 RID: 24403 RVA: 0x0031A522 File Offset: 0x00318722
		private Affliction GetTinkerExhaustion(Character character)
		{
			return character.CharacterHealth.GetAffliction("tinkerexhaustion", true);
		}

		// Token: 0x06005F54 RID: 24404 RVA: 0x0031A538 File Offset: 0x00318738
		private bool CanTinker(Character character)
		{
			if (!this.IsTinkerable(character))
			{
				return false;
			}
			Affliction tinkerExhaustion = this.GetTinkerExhaustion(character);
			return tinkerExhaustion == null || tinkerExhaustion.Strength > tinkerExhaustion.Prefab.MaxStrength;
		}

		// Token: 0x06005F55 RID: 24405 RVA: 0x0031A574 File Offset: 0x00318774
		private void UpdateProjSpecific(float deltaTime)
		{
			if (this.item.IsHidden)
			{
				return;
			}
			if (this.FakeBrokenTimer > 0f)
			{
				this.item.FakeBroken = true;
				if (Character.Controlled != null)
				{
					Affliction affliction = Character.Controlled.CharacterHealth.GetAffliction("psychosis", true);
					if (((affliction != null) ? affliction.Strength : 0f) > 0f)
					{
						this.FakeBrokenTimer -= deltaTime;
						goto IL_83;
					}
				}
				this.FakeBrokenTimer = 0f;
			}
			else
			{
				this.item.FakeBroken = false;
			}
			IL_83:
			if (!GameMain.IsMultiplayer)
			{
				Repairable.FixActions fixActions = this.requestStartFixAction;
				if (fixActions - Repairable.FixActions.Repair <= 2)
				{
					this.StartRepairing(Character.Controlled, this.requestStartFixAction);
					this.requestStartFixAction = Repairable.FixActions.None;
				}
				else
				{
					this.requestStartFixAction = Repairable.FixActions.None;
				}
			}
			float conditionPercentage = this.item.ConditionPercentageRelativeToDefaultMaxCondition;
			for (int i = 0; i < this.particleEmitters.Count; i++)
			{
				if ((conditionPercentage >= this.particleEmitterConditionRanges[i].X && conditionPercentage <= this.particleEmitterConditionRanges[i].Y) || this.FakeBrokenTimer > 0f)
				{
					this.particleEmitters[i].Emit(deltaTime, this.item.WorldPosition, this.item.CurrentHull, 0f, 0f, 1f, 1f, 1f, null, null, false, null);
				}
			}
			if (this.CurrentFixer != null && this.CurrentFixer.SelectedItem == this.item)
			{
				if (this.repairSoundChannel == null || !this.repairSoundChannel.IsPlaying)
				{
					string soundTag = "repair";
					Vector2 worldPosition = this.item.WorldPosition;
					Hull currentHull = this.item.CurrentHull;
					this.repairSoundChannel = SoundPlayer.PlaySound(soundTag, worldPosition, null, null, currentHull);
				}
				if (this.qteCooldown > 0f)
				{
					this.qteCooldown -= deltaTime;
					if (this.qteCooldown <= 0f)
					{
						this.qteTimer = 0.5f;
						return;
					}
				}
				else
				{
					this.qteTimer -= deltaTime * (this.qteTimer / 0.5f);
					if (this.qteTimer < 0f)
					{
						this.qteTimer = 0.5f;
						return;
					}
				}
			}
			else
			{
				SoundChannel soundChannel = this.repairSoundChannel;
				if (soundChannel != null)
				{
					soundChannel.FadeOutAndDispose();
				}
				this.repairSoundChannel = null;
			}
		}

		// Token: 0x06005F56 RID: 24406 RVA: 0x0031A7D8 File Offset: 0x003189D8
		public void AdjustPowerConsumption(ref float powerConsumption)
		{
			if (this.IsBelowRepairThreshold)
			{
				powerConsumption *= MathHelper.Lerp(1.5f, 1f, this.item.Condition / this.item.MaxCondition);
			}
		}

		// Token: 0x06005F57 RID: 24407 RVA: 0x0031A810 File Offset: 0x00318A10
		private bool ShouldDeteriorate()
		{
			if (this.ForceDeteriorationTimer > 0f)
			{
				return true;
			}
			if (Level.IsLoadedFriendlyOutpost)
			{
				return false;
			}
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.GameMode : null) is TutorialMode)
			{
				return false;
			}
			if ((double)this.LastActiveTime > Timing.TotalTime)
			{
				return true;
			}
			foreach (ItemComponent ic in this.item.Components)
			{
				if (ic is Fabricator || ic is Deconstructor)
				{
					return false;
				}
				PowerTransfer pt = ic as PowerTransfer;
				if (pt != null)
				{
					if (pt.Voltage > 0.1f)
					{
						return true;
					}
				}
				else
				{
					PowerContainer pc = ic as PowerContainer;
					if (pc != null)
					{
						if (Math.Abs(pc.CurrPowerConsumption) > 0.1f || Math.Abs(pc.CurrPowerOutput) > 0.1f)
						{
							return true;
						}
					}
					else
					{
						Engine engine = ic as Engine;
						if (engine != null)
						{
							if (Math.Abs(engine.Force) > 1f)
							{
								return true;
							}
						}
						else
						{
							Pump pump = ic as Pump;
							if (pump != null)
							{
								if (Math.Abs(pump.FlowPercentage) > 1f && pump.IsActive && pump.HasPower)
								{
									return true;
								}
							}
							else
							{
								Reactor reactor = ic as Reactor;
								if (reactor != null)
								{
									if (reactor.Temperature > 0.1f)
									{
										return true;
									}
								}
								else
								{
									OxygenGenerator oxyGenerator = ic as OxygenGenerator;
									if (oxyGenerator != null)
									{
										if (oxyGenerator.CurrFlow > 0.1f)
										{
											return true;
										}
									}
									else
									{
										Powered powered = ic as Powered;
										if (powered != null && !(powered is LightComponent) && powered.HasPower)
										{
											return true;
										}
									}
								}
							}
						}
					}
				}
			}
			return false;
		}

		// Token: 0x06005F58 RID: 24408 RVA: 0x0031A9E8 File Offset: 0x00318BE8
		private float GetDeteriorationDelayMultiplier()
		{
			foreach (ItemComponent ic in this.item.Components)
			{
				Engine engine = ic as Engine;
				if (engine != null)
				{
					return Math.Abs(engine.Force) / 100f;
				}
				Pump pump = ic as Pump;
				if (pump != null)
				{
					return Math.Abs(pump.FlowPercentage) / 100f;
				}
				Reactor reactor = ic as Reactor;
				if (reactor != null)
				{
					return (reactor.FissionRate + reactor.TurbineOutput) / 200f;
				}
			}
			return 1f;
		}

		// Token: 0x06005F59 RID: 24409 RVA: 0x0031AAA4 File Offset: 0x00318CA4
		private void UpdateFixAnimation(Character character)
		{
			if (character == null || character.IsDead || character.IsIncapacitated)
			{
				return;
			}
			character.AnimController.UpdateUseItem(false, this.item.WorldPosition + new Vector2(0f, 100f) * (this.item.Condition / this.item.MaxCondition % 0.1f));
		}

		// Token: 0x06005F5A RID: 24410 RVA: 0x0031AB12 File Offset: 0x00318D12
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
		}

		// Token: 0x06005F60 RID: 24416 RVA: 0x0031AB68 File Offset: 0x00318D68
		[CompilerGenerated]
		internal static void <CheckCharacterSuccess>g__ApplyStatusEffectsAndCreateEntityEvent|148_1(ItemComponent ic, ActionType actionType, Character character)
		{
			ic.ApplyStatusEffects(actionType, 1f, character, null, null, null, null, 1f);
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember != null && networkMember.IsServer && ic.statusEffectLists != null && ic.statusEffectLists.ContainsKey(actionType))
			{
				GameMain.NetworkMember.CreateEntityEvent(ic.Item, new Item.ApplyStatusEffectEventData(actionType, ic, character, null, null, null));
			}
		}

		// Token: 0x06005F61 RID: 24417 RVA: 0x0031ABE2 File Offset: 0x00318DE2
		[CompilerGenerated]
		internal static Item <StartRepairing>g__GetBestRepairItem|152_0(Character character)
		{
			return (from i in character.HeldItems
			orderby i.Prefab.AddedRepairSpeedMultiplier descending
			select i).FirstOrDefault<Item>();
		}

		// Token: 0x0400311A RID: 12570
		private GUIProgressBar progressBar;

		// Token: 0x0400311B RID: 12571
		private GUITextBlock progressBarOverlayText;

		// Token: 0x0400311C RID: 12572
		private GUILayoutGroup extraButtonContainer;

		// Token: 0x0400311D RID: 12573
		private GUIComponent skillTextContainer;

		// Token: 0x0400311E RID: 12574
		private readonly List<ParticleEmitter> particleEmitters = new List<ParticleEmitter>();

		// Token: 0x0400311F RID: 12575
		private readonly List<Vector2> particleEmitterConditionRanges = new List<Vector2>();

		// Token: 0x04003120 RID: 12576
		private SoundChannel repairSoundChannel;

		// Token: 0x04003121 RID: 12577
		private LocalizedString repairButtonText;

		// Token: 0x04003122 RID: 12578
		private LocalizedString repairingText;

		// Token: 0x04003123 RID: 12579
		private LocalizedString sabotageButtonText;

		// Token: 0x04003124 RID: 12580
		private LocalizedString sabotagingText;

		// Token: 0x04003125 RID: 12581
		private LocalizedString tinkerButtonText;

		// Token: 0x04003126 RID: 12582
		private LocalizedString tinkeringText;

		// Token: 0x04003127 RID: 12583
		private Repairable.FixActions requestStartFixAction;

		// Token: 0x04003128 RID: 12584
		private bool qteSuccess;

		// Token: 0x04003129 RID: 12585
		private float qteTimer;

		// Token: 0x0400312A RID: 12586
		private const float QteDuration = 0.5f;

		// Token: 0x0400312B RID: 12587
		private float qteCooldown;

		// Token: 0x0400312C RID: 12588
		private const float QteCooldownDuration = 0.5f;

		// Token: 0x0400312D RID: 12589
		public float FakeBrokenTimer;

		// Token: 0x0400312F RID: 12591
		private readonly LocalizedString header;

		// Token: 0x04003130 RID: 12592
		private float deteriorationTimer;

		// Token: 0x04003132 RID: 12594
		private int updateDeteriorationCounter;

		// Token: 0x04003133 RID: 12595
		private const int UpdateDeteriorationInterval = 10;

		// Token: 0x04003134 RID: 12596
		private int prevSentConditionValue;

		// Token: 0x04003135 RID: 12597
		private string conditionSignal;

		// Token: 0x04003136 RID: 12598
		private bool wasBroken;

		// Token: 0x04003137 RID: 12599
		private bool wasGoodCondition;

		// Token: 0x04003138 RID: 12600
		public float LastActiveTime;

		// Token: 0x04003146 RID: 12614
		private float skillRequirementMultiplier;

		// Token: 0x04003147 RID: 12615
		private bool isTinkering;

		// Token: 0x04003149 RID: 12617
		private Item currentRepairItem;

		// Token: 0x0400314A RID: 12618
		private float tinkeringDuration;

		// Token: 0x0400314B RID: 12619
		private float tinkeringStrength;

		// Token: 0x0400314D RID: 12621
		private bool tinkeringPowersDevices;

		// Token: 0x0400314E RID: 12622
		private Repairable.FixActions currentFixerAction;

		// Token: 0x02001448 RID: 5192
		public enum FixActions
		{
			// Token: 0x04006530 RID: 25904
			None,
			// Token: 0x04006531 RID: 25905
			Repair,
			// Token: 0x04006532 RID: 25906
			Sabotage,
			// Token: 0x04006533 RID: 25907
			Tinker
		}
	}
}
