using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Lights;
using Barotrauma.LuaCs.Events;
using Barotrauma.Networking;
using Barotrauma.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x0200002C RID: 44
	internal class CharacterHealth
	{
		// Token: 0x170001F8 RID: 504
		// (get) Token: 0x06000708 RID: 1800 RVA: 0x0003DDC4 File Offset: 0x0003BFC4
		public static Sprite DamageOverlay
		{
			get
			{
				return CharacterHealth.DamageOverlayPrefab.Prefabs.ActivePrefab.DamageOverlay;
			}
		}

		// Token: 0x170001F9 RID: 505
		// (get) Token: 0x06000709 RID: 1801 RVA: 0x0003DDD5 File Offset: 0x0003BFD5
		// (set) Token: 0x0600070A RID: 1802 RVA: 0x0003DDDD File Offset: 0x0003BFDD
		public Alignment Alignment
		{
			get
			{
				return this.alignment;
			}
			set
			{
				if (this.alignment == value)
				{
					return;
				}
				this.alignment = value;
				this.UpdateAlignment();
			}
		}

		// Token: 0x170001FA RID: 506
		// (get) Token: 0x0600070B RID: 1803 RVA: 0x0003DDF6 File Offset: 0x0003BFF6
		// (set) Token: 0x0600070C RID: 1804 RVA: 0x0003DDFE File Offset: 0x0003BFFE
		public GUIButton SuicideButton { get; private set; }

		// Token: 0x170001FB RID: 507
		// (get) Token: 0x0600070D RID: 1805 RVA: 0x0003DE07 File Offset: 0x0003C007
		// (set) Token: 0x0600070E RID: 1806 RVA: 0x0003DE0F File Offset: 0x0003C00F
		public float DamageOverlayTimer { get; private set; }

		// Token: 0x170001FC RID: 508
		// (get) Token: 0x0600070F RID: 1807 RVA: 0x0003DE18 File Offset: 0x0003C018
		public bool MouseOnElement
		{
			get
			{
				return this.highlightedLimbIndex > -1;
			}
		}

		// Token: 0x170001FD RID: 509
		// (get) Token: 0x06000710 RID: 1808 RVA: 0x0003DE23 File Offset: 0x0003C023
		// (set) Token: 0x06000711 RID: 1809 RVA: 0x0003DE2C File Offset: 0x0003C02C
		public static CharacterHealth OpenHealthWindow
		{
			get
			{
				return CharacterHealth.openHealthWindow;
			}
			set
			{
				if (CharacterHealth.openHealthWindow == value)
				{
					return;
				}
				if (value != null && (!value.UseHealthWindow || value.Character.DisableHealthWindow))
				{
					return;
				}
				CharacterHealth prevOpenHealthWindow = CharacterHealth.openHealthWindow;
				if (prevOpenHealthWindow != null)
				{
					prevOpenHealthWindow.highlightedLimbIndex = -1;
				}
				CharacterHealth.openHealthWindow = value;
				CharacterHealth.toggledThisFrame = true;
				if (Character.Controlled == null)
				{
					return;
				}
				if (value == null)
				{
					Character controlled = Character.Controlled;
					bool flag;
					if (controlled == null)
					{
						flag = (null != null);
					}
					else
					{
						Character selectedCharacter = controlled.SelectedCharacter;
						flag = (((selectedCharacter != null) ? selectedCharacter.CharacterHealth : null) != null);
					}
					if (flag && Character.Controlled.SelectedCharacter.CharacterHealth == prevOpenHealthWindow)
					{
						Character.Controlled.DeselectCharacter();
					}
				}
				Character.Controlled.DisableInteract = true;
				if (CharacterHealth.openHealthWindow != null)
				{
					if (value.Character.Info == null || value.Character == Character.Controlled || Character.Controlled.HasEquippedItem("healthscanner".ToIdentifier(), true, null))
					{
						CharacterHealth.openHealthWindow.characterName.Text = value.Character.Name;
					}
					else
					{
						CharacterHealth.openHealthWindow.characterName.Text = value.Character.Info.DisplayName;
						value.Character.Info.CheckDisguiseStatus(false, null);
					}
					Character.Controlled.SelectedItem = null;
				}
				HintManager.OnShowHealthInterface();
			}
		}

		// Token: 0x170001FE RID: 510
		// (get) Token: 0x06000712 RID: 1810 RVA: 0x0003DF73 File Offset: 0x0003C173
		public GUIButton CPRButton
		{
			get
			{
				return this.cprButton;
			}
		}

		// Token: 0x170001FF RID: 511
		// (get) Token: 0x06000713 RID: 1811 RVA: 0x0003DF7B File Offset: 0x0003C17B
		// (set) Token: 0x06000714 RID: 1812 RVA: 0x0003DF83 File Offset: 0x0003C183
		public GUIComponent InventorySlotContainer { get; private set; }

		// Token: 0x17000200 RID: 512
		// (get) Token: 0x06000715 RID: 1813 RVA: 0x0003DF8C File Offset: 0x0003C18C
		// (set) Token: 0x06000716 RID: 1814 RVA: 0x0003DF94 File Offset: 0x0003C194
		public float HealthBarPulsateTimer
		{
			get
			{
				return this.healthBarPulsateTimer;
			}
			set
			{
				this.healthBarPulsateTimer = MathHelper.Clamp(value, 0f, 10f);
			}
		}

		// Token: 0x06000717 RID: 1815 RVA: 0x0003DFAC File Offset: 0x0003C1AC
		private void OnAttacked(Character attacker, AttackResult attackResult)
		{
			if (Math.Abs(attackResult.Damage) < 0.01f)
			{
				return;
			}
			if (this.ShowDamageOverlay)
			{
				this.DamageOverlayTimer = MathHelper.Clamp(attackResult.Damage / this.MaxVitality, this.DamageOverlayTimer, 1f);
				float additionalIntensity = MathHelper.Lerp(0f, 1f, MathUtils.InverseLerp(0f, 0.1f, attackResult.Damage / this.MaxVitality));
				this.damageIntensity = MathHelper.Clamp(this.damageIntensity + additionalIntensity, 0f, 1f);
			}
			if (this.healthShadowDelay <= 0f)
			{
				this.healthShadowDelay = 1f;
			}
			if (this.healthBarPulsateTimer <= 0f)
			{
				this.healthBarPulsatePhase = 0f;
			}
			this.healthBarPulsateTimer = 1f;
			this.DisplayVitalityDelay = 0.5f;
		}

		// Token: 0x06000718 RID: 1816 RVA: 0x0003E08C File Offset: 0x0003C28C
		private void UpdateAlignment()
		{
			this.screenResolution = new Point(GameMain.GraphicsWidth, GameMain.GraphicsHeight);
			this.inventoryScale = Inventory.UIScale;
			this.uiScale = GUI.Scale;
			this.showHiddenAfflictionsButton.RectTransform.NonScaledSize = new Point(this.afflictionIconContainer.Rect.Height);
			for (int i = this.afflictionIconContainer.CountChildren - 1; i >= 0; i--)
			{
				GUIComponent child = this.afflictionIconContainer.GetChild(i);
				if (child.UserData is AfflictionPrefab)
				{
					this.afflictionIconContainer.RemoveChild(child);
				}
			}
			this.healthBarHolder.RectTransform.AbsoluteOffset = HUDLayoutSettings.HealthBarArea.Location;
			this.healthBarHolder.RectTransform.NonScaledSize = HUDLayoutSettings.HealthBarArea.Size;
			this.healthBarHolder.RectTransform.RelativeOffset = Vector2.Zero;
			Alignment alignment = this.alignment;
			if (alignment != Alignment.Left)
			{
				if (alignment == Alignment.Right)
				{
					this.healthWindow.RectTransform.SetPosition(Anchor.BottomRight, null);
					this.healthWindow.RectTransform.AbsoluteOffset = new Point(HUDLayoutSettings.Padding, this.screenResolution.Y - HUDLayoutSettings.ChatBoxArea.Y + HUDLayoutSettings.Padding);
				}
			}
			else
			{
				this.healthWindow.RectTransform.SetPosition(Anchor.BottomLeft, null);
				this.healthWindow.RectTransform.AbsoluteOffset = new Point(HUDLayoutSettings.InventoryAreaLower.X, this.screenResolution.Y - HUDLayoutSettings.ChatBoxArea.Y + HUDLayoutSettings.Padding);
			}
			this.healthWindow.RectTransform.RecalculateChildren(false, true);
			CharacterInventory inventory = this.Character.Inventory;
			if (inventory == null)
			{
				return;
			}
			inventory.RefreshSlotPositions();
		}

		// Token: 0x06000719 RID: 1817 RVA: 0x0003E25C File Offset: 0x0003C45C
		public void UpdateClientSpecific(float deltaTime)
		{
			if (GameMain.NetworkMember == null)
			{
				this.DisplayedVitality = this.Vitality;
			}
			else
			{
				this.DisplayVitalityDelay -= deltaTime;
				if (this.DisplayVitalityDelay <= 0f)
				{
					this.DisplayedVitality = this.Vitality;
				}
			}
			if (this.damageIntensity > 0f)
			{
				this.damageIntensity -= deltaTime * this.damageIntensityDropdownRate;
				if (this.damageIntensity < 0f)
				{
					this.damageIntensity = 0f;
				}
			}
			if (this.DamageOverlayTimer > 0f)
			{
				this.DamageOverlayTimer -= deltaTime;
			}
		}

		// Token: 0x0600071A RID: 1818 RVA: 0x0003E2FA File Offset: 0x0003C4FA
		public static bool IsMouseOnHealthBar()
		{
			Character controlled = Character.Controlled;
			return ((controlled != null) ? controlled.CharacterHealth : null) != null && Character.Controlled.CharacterHealth.healthBar.State == GUIComponent.ComponentState.Hover;
		}

		// Token: 0x0600071B RID: 1819 RVA: 0x0003E328 File Offset: 0x0003C528
		public void UpdateHUD(float deltaTime)
		{
			if (GUI.DisableHUD)
			{
				return;
			}
			if (CharacterHealth.openHealthWindow != null)
			{
				CharacterHealth characterHealth = CharacterHealth.openHealthWindow;
				Character controlled = Character.Controlled;
				if (characterHealth != ((controlled != null) ? controlled.CharacterHealth : null))
				{
					CharacterHealth characterHealth2 = CharacterHealth.openHealthWindow;
					Character controlled2 = Character.Controlled;
					object obj;
					if (controlled2 == null)
					{
						obj = null;
					}
					else
					{
						Character selectedCharacter = controlled2.SelectedCharacter;
						obj = ((selectedCharacter != null) ? selectedCharacter.CharacterHealth : null);
					}
					if (characterHealth2 != obj)
					{
						CharacterHealth.openHealthWindow = null;
						return;
					}
				}
			}
			bool forceAfflictionContainerUpdate = false;
			if (this.updateDisplayedAfflictionsTimer > 0f)
			{
				this.updateDisplayedAfflictionsTimer -= deltaTime;
			}
			else
			{
				forceAfflictionContainerUpdate = true;
				this.currentDisplayedAfflictions.Clear();
				this.currentDisplayedAfflictions.AddRange(this.GetAllAfflictions(true, (Affliction a) => a.ShouldShowIcon(this.Character) && a.Prefab.Icon != null));
				this.currentDisplayedAfflictions.Sort(delegate(Affliction a1, Affliction a2)
				{
					int dmgPerSecond = Math.Sign(a1.DamagePerSecond - a2.DamagePerSecond);
					if (dmgPerSecond != 0)
					{
						return dmgPerSecond;
					}
					return Math.Sign(CharacterHealth.<UpdateHUD>g__GetStr|84_2(a1) - CharacterHealth.<UpdateHUD>g__GetStr|84_2(a2));
				});
				HintManager.OnAfflictionDisplayed(this.Character, this.currentDisplayedAfflictions);
				this.updateDisplayedAfflictionsTimer = 0.5f;
			}
			if (this.healthShadowDelay > 0f)
			{
				this.healthShadowDelay -= deltaTime;
			}
			else
			{
				this.healthShadowSize = ((this.healthBar.BarSize > this.healthShadowSize) ? Math.Min(this.healthShadowSize + deltaTime, this.healthBar.BarSize) : Math.Max(this.healthShadowSize - deltaTime, this.healthBar.BarSize));
			}
			float blurStrength = 0f;
			float distortStrength = 0f;
			float distortSpeed = 0f;
			float radialDistortStrength = 0f;
			float chromaticAberrationStrength = 0f;
			float grainStrength = 0f;
			Color grainColor = Color.Transparent;
			float oxygenLowStrength = 0f;
			if (this.Character.IsUnconscious)
			{
				blurStrength = 1f;
				distortSpeed = 1f;
			}
			else if (this.OxygenAmount < 100f)
			{
				oxygenLowStrength = Math.Min(1f - (this.OxygenAmount - 50f) / 50f, 1f);
				blurStrength = MathHelper.Lerp(0.5f, 1f, 1f - this.Vitality / this.MaxVitality) * oxygenLowStrength;
				distortStrength = blurStrength * oxygenLowStrength;
				distortSpeed = blurStrength + 1f;
				distortSpeed *= distortSpeed * distortSpeed * distortSpeed;
				grainStrength = MathHelper.Lerp(0.5f, 10f, oxygenLowStrength);
				grainColor = CharacterHealth.oxygenLowGrainColor;
			}
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp in this.afflictions)
			{
				Affliction affliction = kvp.Key;
				distortStrength = Math.Max(distortStrength, affliction.GetScreenDistortStrength());
				blurStrength = Math.Max(blurStrength, affliction.GetScreenBlurStrength());
				radialDistortStrength = Math.Max(radialDistortStrength, affliction.GetRadialDistortStrength());
				chromaticAberrationStrength = Math.Max(chromaticAberrationStrength, affliction.GetChromaticAberrationStrength());
				float afflictionGrainStrength = affliction.GetScreenGrainStrength();
				if (afflictionGrainStrength > 0f)
				{
					grainStrength = Math.Max(grainStrength, afflictionGrainStrength);
					AfflictionPrefab.Effect activeEffect = affliction.GetActiveEffect();
					Color afflictionGrainColor = (activeEffect != null) ? activeEffect.GrainColor : Color.White;
					grainColor = Color.Lerp(grainColor, afflictionGrainColor, (float)Math.Pow((double)(1f - oxygenLowStrength), 2.0));
				}
			}
			this.Character.RadialDistortStrength = radialDistortStrength;
			this.Character.ChromaticAberrationStrength = chromaticAberrationStrength;
			this.Character.GrainStrength = grainStrength;
			this.Character.GrainColor = grainColor;
			if (blurStrength > 0f)
			{
				this.distortTimer = (this.distortTimer + deltaTime * distortSpeed) % 6.2831855f;
				this.Character.BlurStrength = (float)(Math.Sin((double)this.distortTimer) + 1.5) * 0.25f * blurStrength;
				this.Character.DistortStrength = (float)(Math.Sin((double)this.distortTimer) + 1.0) * 0.05f * distortStrength;
			}
			else
			{
				this.Character.BlurStrength = 0f;
				this.Character.DistortStrength = 0f;
				this.distortTimer = 0f;
			}
			this.UpdateStatusHUD(deltaTime);
			if (PlayerInput.KeyHit(InputType.Health) && GUI.KeyboardDispatcher.Subscriber == null && Character.Controlled.AllowInput && !CharacterHealth.toggledThisFrame)
			{
				if (CharacterHealth.openHealthWindow != null)
				{
					CharacterHealth.OpenHealthWindow = null;
				}
				else if (Character.Controlled == this.Character)
				{
					Character focusedCharacter = Character.Controlled.FocusedCharacter;
					if (((focusedCharacter != null) ? focusedCharacter.CharacterHealth : null) == null || !Character.Controlled.FocusedCharacter.CharacterHealth.UseHealthWindow || Character.Controlled.FocusedCharacter.DisableHealthWindow)
					{
						CharacterHealth.OpenHealthWindow = this;
						forceAfflictionContainerUpdate = true;
					}
				}
			}
			else if (CharacterHealth.openHealthWindow == this)
			{
				if (HUD.CloseHUD(this.healthWindow.Rect))
				{
					if (GameMain.Client != null)
					{
						Character.Controlled.EmulateInput(InputType.Health);
					}
					CharacterHealth.OpenHealthWindow = null;
				}
				foreach (GUIComponent afflictionIcon in this.afflictionIconList.Content.Children)
				{
					Affliction affliction2 = afflictionIcon.UserData as Affliction;
					if (affliction2 != null)
					{
						if (affliction2.AppliedAsFailedTreatmentTime > Timing.TotalTime - 1.0 && afflictionIcon.FlashTimer <= 0f)
						{
							afflictionIcon.Flash(new Color?(GUIStyle.Red), 1.5f, false, false, null);
						}
						else if (affliction2.AppliedAsSuccessfulTreatmentTime > Timing.TotalTime - 1.0 && afflictionIcon.FlashTimer <= 0f)
						{
							afflictionIcon.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
						}
					}
				}
				GUIComponent mouseOn = GUI.MouseOn;
				if (((mouseOn != null) ? mouseOn.UserData : null) is Affliction)
				{
					GUIComponent mouseOn2 = GUI.MouseOn;
					Affliction affliction3 = ((mouseOn2 != null) ? mouseOn2.UserData : null) as Affliction;
					if (this.afflictionTooltip == null || this.afflictionTooltip.UserData != affliction3)
					{
						this.afflictionTooltip = new GUIListBox(new RectTransform(new Vector2(0.4f, 0.2f), GUI.Canvas, Anchor.TopLeft, null, null, null, ScaleBasis.Smallest), false, null, "", true, false)
						{
							UserData = affliction3,
							CanBeFocused = false
						};
						this.CreateAfflictionInfoElements(this.afflictionTooltip.Content, affliction3);
						int height = this.afflictionTooltip.Content.Children.Sum((GUIComponent c) => c.Rect.Height) + 10;
						this.afflictionTooltip.RectTransform.Resize(new Point(this.afflictionTooltip.Rect.Width, height), true);
						if (this.Alignment == Alignment.Right)
						{
							this.afflictionTooltip.RectTransform.AbsoluteOffset = new Point(GUI.MouseOn.Rect.X, GUI.MouseOn.Rect.Y);
							this.afflictionTooltip.RectTransform.Pivot = Pivot.TopRight;
						}
						else
						{
							this.afflictionTooltip.RectTransform.AbsoluteOffset = new Point(GUI.MouseOn.Rect.Right, GUI.MouseOn.Rect.Y);
							this.afflictionTooltip.RectTransform.Anchor = Anchor.TopLeft;
						}
						this.afflictionTooltip.ScrollBarVisible = false;
						GUIComponent labelContainer = this.afflictionTooltip.Content.GetChildByUserData("label");
						labelContainer.RectTransform.Resize(new Point(labelContainer.Rect.Width, (int)(GUIStyle.LargeFont.Size * 1.5f)), true);
					}
				}
				else
				{
					this.afflictionTooltip = null;
				}
			}
			CharacterHealth.toggledThisFrame = false;
			if (CharacterHealth.OpenHealthWindow == this)
			{
				CharacterHealth.LimbHealth highlightedLimb = (this.highlightedLimbIndex < 0) ? null : this.limbHealths[this.highlightedLimbIndex];
				if (this.highlightedLimbIndex < 0 && this.selectedLimbIndex < 0)
				{
					Affliction affliction4 = CharacterHealth.SortAfflictionsBySeverity(this.GetAllAfflictions((Affliction a) => a.Prefab.IndicatorLimb > LimbType.None), true).FirstOrDefault<Affliction>();
					if (affliction4 != null && (affliction4.DamagePerSecond > 0f || affliction4.Strength > 0f))
					{
						CharacterHealth.LimbHealth limbHealth = this.GetMatchingLimbHealth(affliction4);
						if (limbHealth != null)
						{
							this.selectedLimbIndex = this.limbHealths.IndexOf(limbHealth);
						}
					}
					else
					{
						CharacterHealth.LimbHealth limbHealth2 = (from l in this.limbHealths
						orderby this.GetTotalDamage(l) descending
						select l).FirstOrDefault<CharacterHealth.LimbHealth>();
						this.selectedLimbIndex = this.limbHealths.IndexOf(limbHealth2);
					}
				}
				CharacterHealth.LimbHealth selectedLimb = (this.selectedLimbIndex < 0) ? highlightedLimb : this.limbHealths[this.selectedLimbIndex];
				if (selectedLimb != this.currentDisplayedLimb || forceAfflictionContainerUpdate)
				{
					this.UpdateAfflictionContainer(selectedLimb);
					this.currentDisplayedLimb = selectedLimb;
				}
				this.UpdateAfflictionInfos(from d in this.displayedAfflictions
				select d.Item1);
				foreach (GUIComponent component in this.recommendedTreatmentContainer.Content.Children)
				{
					GUIButton treatmentButton = component.GetChild<GUIButton>();
					ItemPrefab itemPrefab = ((treatmentButton != null) ? treatmentButton.UserData : null) as ItemPrefab;
					if (itemPrefab != null)
					{
						Item matchingItem = AIObjectiveRescue.FindMedicalItem(Character.Controlled.Inventory, itemPrefab.Identifier);
						treatmentButton.Enabled = (matchingItem != null);
						if (treatmentButton.Enabled && treatmentButton.State == GUIComponent.ComponentState.Hover)
						{
							Item rootContainer = matchingItem.RootContainer ?? matchingItem;
							int index = Character.Controlled.Inventory.FindIndex(rootContainer);
							if (Character.Controlled.Inventory.visualSlots != null && index > -1 && index < Character.Controlled.Inventory.visualSlots.Length && Character.Controlled.Inventory.visualSlots[index].HighlightTimer <= 0f)
							{
								Character.Controlled.Inventory.visualSlots[index].ShowBorderHighlight(GUIStyle.Green, 0.5f, 0.5f, 0.5f);
							}
						}
						if (matchingItem == null || treatmentButton.ToolTip.IsNullOrEmpty())
						{
							GUIComponent guicomponent = treatmentButton;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(34, 1);
							defaultInterpolatedStringHandler.AppendLiteral("‖color:255,255,255,255‖");
							defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(itemPrefab.Name);
							defaultInterpolatedStringHandler.AppendLiteral("‖color:end‖");
							guicomponent.ToolTip = RichString.Rich(defaultInterpolatedStringHandler.ToStringAndClear() + "\n" + itemPrefab.Description, null);
							if (treatmentButton.Enabled)
							{
								GUIComponent guicomponent2 = treatmentButton;
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(31, 2);
								defaultInterpolatedStringHandler2.AppendLiteral("‖color:gui.green‖[");
								defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(PlayerInput.PrimaryMouseLabel);
								defaultInterpolatedStringHandler2.AppendLiteral("] ");
								defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(TextManager.Get("quickuseaction.usetreatment"));
								defaultInterpolatedStringHandler2.AppendLiteral("‖color:end‖");
								guicomponent2.ToolTip = RichString.Rich(defaultInterpolatedStringHandler2.ToStringAndClear() + "\n" + treatmentButton.ToolTip.NestedStr, null);
							}
							foreach (GUIComponent child in treatmentButton.Children)
							{
								child.Enabled = treatmentButton.Enabled;
							}
						}
					}
				}
			}
			if (this.Character.IsDead)
			{
				this.healthBar.Color = (this.healthWindowHealthBar.Color = Color.Black);
				this.healthBar.BarSize = (this.healthWindowHealthBar.BarSize = 1f);
			}
			else
			{
				this.healthBar.Color = (this.healthWindowHealthBar.Color = ToolBox.GradientLerp(this.DisplayedVitality / this.MaxVitality, new Color[]
				{
					GUIStyle.HealthBarColorLow,
					GUIStyle.HealthBarColorMedium,
					GUIStyle.HealthBarColorHigh
				}));
				this.healthBar.HoverColor = (this.healthWindowHealthBar.HoverColor = this.healthBar.Color * 2f);
				this.healthBar.BarSize = (this.healthWindowHealthBar.BarSize = ((this.DisplayedVitality > 0f) ? ((this.MaxVitality > 0f) ? (this.DisplayedVitality / this.MaxVitality) : 0f) : ((Math.Abs(this.MinVitality) > 0f) ? (1f - this.DisplayedVitality / this.MinVitality) : 0f)));
				if (this.healthBarPulsateTimer > 0f)
				{
					float pulsateAmount = (float)(Math.Sin((double)this.healthBarPulsatePhase) + 1.0) / 2f;
					RectTransform rectTransform = this.healthBar.RectTransform;
					RectTransform rectTransform2 = this.healthBarShadow.RectTransform;
					Vector2 localScale = new Vector2(1f, 1f + pulsateAmount * this.healthBarPulsateTimer * 0.5f);
					rectTransform2.LocalScale = localScale;
					rectTransform.LocalScale = localScale;
					this.healthBarPulsatePhase += deltaTime * 5f;
					this.healthBarPulsateTimer -= deltaTime;
				}
				else
				{
					this.healthBar.RectTransform.LocalScale = Vector2.One;
				}
			}
			if (CharacterHealth.OpenHealthWindow == this)
			{
				if (this.Character == Character.Controlled && !this.Character.AllowInput)
				{
					CharacterHealth.openHealthWindow = null;
				}
				if (Inventory.DraggingItems.Any<Item>() && this.highlightedLimbIndex > -1)
				{
					this.selectedLimbIndex = this.highlightedLimbIndex;
				}
			}
			else
			{
				if (CharacterHealth.openHealthWindow != null && this.Character != Character.Controlled)
				{
					Character character = this.Character;
					Character controlled3 = Character.Controlled;
					if (character != ((controlled3 != null) ? controlled3.SelectedCharacter : null))
					{
						CharacterHealth.openHealthWindow = null;
					}
				}
				this.highlightedLimbIndex = -1;
			}
			this.healthBarHolder.CanBeFocused = (this.healthBar.CanBeFocused = (this.healthBarShadow.CanBeFocused = !this.Character.ShouldLockHud()));
			if (this.Character.AllowInput && this.UseHealthWindow && !this.Character.DisableHealthWindow && this.healthBar.Enabled && this.healthBar.CanBeFocused)
			{
				if (!GUI.IsMouseOn(this.healthBar))
				{
					GUIComponent mouseOn3 = GUI.MouseOn;
					if (!(((mouseOn3 != null) ? mouseOn3.UserData : null) is AfflictionPrefab))
					{
						goto IL_EE1;
					}
				}
				if (Inventory.SelectedSlot == null)
				{
					this.healthBar.State = GUIComponent.ComponentState.Hover;
					if (PlayerInput.PrimaryMouseButtonClicked())
					{
						CharacterHealth.OpenHealthWindow = ((CharacterHealth.openHealthWindow == this) ? null : this);
						goto IL_EED;
					}
					goto IL_EED;
				}
			}
			IL_EE1:
			this.healthBar.State = GUIComponent.ComponentState.None;
			IL_EED:
			this.SuicideButton.Visible = (this.Character == Character.Controlled && !this.Character.IsDead && this.Character.IsIncapacitated);
			GameSession gameSession = GameMain.GameSession;
			CampaignMode campaign = (gameSession != null) ? gameSession.Campaign : null;
			if (campaign != null)
			{
				RectTransform rectTransform3;
				if (campaign == null)
				{
					rectTransform3 = null;
				}
				else
				{
					GUIButton endRoundButton2 = campaign.EndRoundButton;
					rectTransform3 = ((endRoundButton2 != null) ? endRoundButton2.RectTransform : null);
				}
				RectTransform endRoundButton = rectTransform3;
				RectTransform rectTransform4;
				if (campaign == null)
				{
					rectTransform4 = null;
				}
				else
				{
					GUIButton readyCheckButton2 = campaign.ReadyCheckButton;
					rectTransform4 = ((readyCheckButton2 != null) ? readyCheckButton2.RectTransform : null);
				}
				RectTransform readyCheckButton = rectTransform4;
				if (endRoundButton != null)
				{
					if (this.SuicideButton.Visible)
					{
						Point offset = new Point(0, this.SuicideButton.Rect.Height);
						endRoundButton.ScreenSpaceOffset = offset;
					}
					else if (endRoundButton.ScreenSpaceOffset != Point.Zero)
					{
						endRoundButton.ScreenSpaceOffset = Point.Zero;
					}
					if (readyCheckButton != null)
					{
						readyCheckButton.ScreenSpaceOffset = endRoundButton.ScreenSpaceOffset;
					}
				}
			}
			GUIComponent guicomponent3 = this.cprButton;
			Character character2 = this.Character;
			Character controlled4 = Character.Controlled;
			guicomponent3.Visible = (character2 == ((controlled4 != null) ? controlled4.SelectedCharacter : null) && !this.Character.IsDead && this.Character.IsKnockedDown && CharacterHealth.openHealthWindow == this);
			this.cprButton.Selected = (Character.Controlled != null && this.Character == Character.Controlled.SelectedCharacter && Character.Controlled.AnimController.Anim == AnimController.Animation.CPR);
			this.deadIndicator.Visible = this.Character.IsDead;
		}

		// Token: 0x0600071C RID: 1820 RVA: 0x0003F408 File Offset: 0x0003D608
		public void AddToGUIUpdateList()
		{
			if (GUI.DisableHUD)
			{
				return;
			}
			if (CharacterHealth.OpenHealthWindow == this)
			{
				this.healthWindow.AddToGUIUpdateList(false, 0);
				GUIListBox guilistBox = this.afflictionTooltip;
				if (guilistBox != null)
				{
					guilistBox.AddToGUIUpdateList(false, 0);
				}
			}
			else if (Character.Controlled == this.Character && !CharacterHUD.IsCampaignInterfaceOpen)
			{
				this.healthBarHolder.AddToGUIUpdateList(false, 0);
				this.afflictionIconContainer.AddToGUIUpdateList(false, 0);
				if (this.hiddenAfflictionIconContainer.Visible)
				{
					this.hiddenAfflictionIconContainer.AddToGUIUpdateList(false, 0);
				}
			}
			if (this.SuicideButton.Visible && this.Character == Character.Controlled)
			{
				this.SuicideButton.AddToGUIUpdateList(false, 0);
			}
			if (this.cprButton != null && this.cprButton.Visible)
			{
				this.cprButton.AddToGUIUpdateList(false, 0);
			}
		}

		// Token: 0x0600071D RID: 1821 RVA: 0x0003F4D8 File Offset: 0x0003D6D8
		public void DrawHUD(SpriteBatch spriteBatch)
		{
			if (GUI.DisableHUD || this.Character.Removed)
			{
				return;
			}
			if (GameMain.GraphicsWidth != this.screenResolution.X || GameMain.GraphicsHeight != this.screenResolution.Y || Math.Abs(this.inventoryScale - Inventory.UIScale) > 0.01f || Math.Abs(this.uiScale - GUI.Scale) > 0.01f)
			{
				this.UpdateAlignment();
			}
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp in this.afflictions)
			{
				Affliction affliction = kvp.Key;
				AfflictionPrefab afflictionPrefab = affliction.Prefab;
				if (afflictionPrefab != null && afflictionPrefab.AfflictionOverlay != null)
				{
					Vector2 screenSize = new Vector2((float)GameMain.GraphicsWidth, (float)GameMain.GraphicsHeight);
					SpriteSheet spriteSheet = afflictionPrefab.AfflictionOverlay as SpriteSheet;
					if (spriteSheet != null)
					{
						spriteSheet.Draw(spriteBatch, spriteSheet.GetAnimatedSpriteIndex(afflictionPrefab.AfflictionOverlayAnimSpeed, false), Vector2.Zero, Color.White * affliction.GetAfflictionOverlayMultiplier(), Vector2.Zero, 0f, screenSize / spriteSheet.FrameSize.ToVector2(), SpriteEffects.None, null);
					}
					else
					{
						Sprite sprite = afflictionPrefab.AfflictionOverlay;
						if (sprite != null)
						{
							sprite.Draw(spriteBatch, Vector2.Zero, Color.White * affliction.GetAfflictionOverlayMultiplier(), Vector2.Zero, 0f, screenSize / sprite.size, SpriteEffects.None, null);
						}
					}
				}
				AfflictionPrefab.Effect activeEffect = affliction.GetActiveEffect();
				if (activeEffect != null && activeEffect.ThermalOverlayRange > 0f)
				{
					StatusHUD.DrawThermalOverlay(spriteBatch, this.Character, this.Character, activeEffect.ThermalOverlayColor, activeEffect.ThermalOverlayRange, (float)Timing.TotalTimeUnpaused, false);
				}
			}
			float damageOverlayAlpha = this.DamageOverlayTimer;
			if (this.Vitality < this.MaxVitality * 0.1f)
			{
				damageOverlayAlpha = Math.Max(1f - this.Vitality / this.UnmodifiedMaxVitality * 10f, damageOverlayAlpha);
			}
			else
			{
				float pulsateAmount = (float)(Math.Sin((double)this.healthBarPulsatePhase) + 1.0) / 2f;
				damageOverlayAlpha = pulsateAmount * this.healthBarPulsateTimer * this.damageIntensity;
			}
			if (damageOverlayAlpha > 0f)
			{
				Sprite damageOverlay = CharacterHealth.DamageOverlay;
				if (damageOverlay != null)
				{
					damageOverlay.Draw(spriteBatch, Vector2.Zero, Color.White * damageOverlayAlpha, Vector2.Zero, 0f, new Vector2((float)GameMain.GraphicsWidth / CharacterHealth.DamageOverlay.size.X, (float)GameMain.GraphicsHeight / CharacterHealth.DamageOverlay.size.Y), SpriteEffects.None, null);
				}
			}
			if (this.Character.Inventory != null)
			{
				this.healthBar.RectTransform.ScreenSpaceOffset = (this.healthBarShadow.RectTransform.ScreenSpaceOffset = Point.Zero);
			}
			if (this.healthBarHolder != null)
			{
				this.healthBarHolder.RectTransform.ScreenSpaceOffset = (this.Character.ShouldLockHud() ? new Point(0, HUDLayoutSettings.PortraitArea.Height) : Point.Zero);
			}
		}

		// Token: 0x0600071E RID: 1822 RVA: 0x0003F824 File Offset: 0x0003DA24
		public void UpdateStatusHUD(float deltaTime)
		{
			Character controlled = Character.Controlled;
			if (((controlled != null) ? controlled.SelectedCharacter : null) == null && CharacterHealth.openHealthWindow == null)
			{
				this.statusIcons.Clear();
				if (this.Character.InPressure)
				{
					this.statusIcons.Add(this.pressureAffliction);
				}
				if (this.Character.CurrentHull != null && this.Character.OxygenAvailable < 50f && this.oxygenLowAffliction.Strength < this.oxygenLowAffliction.Prefab.ShowIconThreshold)
				{
					this.statusIcons.Add(this.oxygenLowAffliction);
				}
				foreach (Affliction affliction in this.currentDisplayedAfflictions)
				{
					this.statusIcons.Add(affliction);
				}
				int spacing = GUI.IntScale(10f);
				if (this.Character.ShouldLockHud())
				{
					this.afflictionIconContainer.RectTransform.ScreenSpaceOffset = new Point(0, HUDLayoutSettings.PortraitArea.Height);
					this.hiddenAfflictionIconContainer.RectTransform.ScreenSpaceOffset = new Point(0, -this.hiddenAfflictionIconContainer.Rect.Height - spacing + HUDLayoutSettings.PortraitArea.Height);
				}
				else
				{
					this.afflictionIconContainer.RectTransform.ScreenSpaceOffset = new Point(0, 0);
					this.hiddenAfflictionIconContainer.RectTransform.ScreenSpaceOffset = new Point(0, -this.hiddenAfflictionIconContainer.Rect.Height - spacing);
				}
				this.<UpdateStatusHUD>g__RemoveNonExistentIcons|90_0(this.afflictionIconContainer);
				this.<UpdateStatusHUD>g__RemoveNonExistentIcons|90_0(this.hiddenAfflictionIconContainer);
				foreach (Affliction statusIcon in this.statusIcons)
				{
					Affliction affliction2 = statusIcon;
					AfflictionPrefab afflictionPrefab = affliction2.Prefab;
					if (!this.statusIconVisibleTime.ContainsKey(afflictionPrefab))
					{
						this.statusIconVisibleTime.Add(afflictionPrefab, 0f);
					}
					Dictionary<AfflictionPrefab, float> dictionary = this.statusIconVisibleTime;
					AfflictionPrefab key = afflictionPrefab;
					dictionary[key] += deltaTime;
					Color color = CharacterHealth.GetAfflictionIconColor(afflictionPrefab, affliction2);
					GUIComponent matchingIcon = this.afflictionIconContainer.GetChildByUserData(afflictionPrefab) ?? this.hiddenAfflictionIconContainer.GetChildByUserData(afflictionPrefab);
					if (matchingIcon == null)
					{
						GUIButton guibutton = new GUIButton(new RectTransform(new Point(this.afflictionIconContainer.Rect.Height), this.afflictionIconContainer.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), Alignment.Center, null, null);
						guibutton.UserData = afflictionPrefab;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(19, 2);
						defaultInterpolatedStringHandler.AppendLiteral("‖color:");
						defaultInterpolatedStringHandler.AppendFormatted(color.ToStringHex());
						defaultInterpolatedStringHandler.AppendLiteral("‖");
						defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(affliction2.Prefab.Name);
						defaultInterpolatedStringHandler.AppendLiteral("‖color:end‖");
						guibutton.ToolTip = defaultInterpolatedStringHandler.ToStringAndClear();
						guibutton.CanBeSelected = false;
						matchingIcon = guibutton;
						new GUIImage(new RectTransform(Vector2.One, matchingIcon.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal), afflictionPrefab.Icon, true, null).CanBeFocused = false;
					}
					if (afflictionPrefab.HideIconAfterDelay && this.statusIconVisibleTime[afflictionPrefab] > 5f)
					{
						matchingIcon.RectTransform.Parent = this.hiddenAfflictionIconContainer.RectTransform;
					}
					else
					{
						if (affliction2.Prefab.ShowDescriptionInTooltip)
						{
							GUIComponent guicomponent = matchingIcon;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(19, 2);
							defaultInterpolatedStringHandler2.AppendLiteral("‖color:");
							defaultInterpolatedStringHandler2.AppendFormatted(color.ToStringHex());
							defaultInterpolatedStringHandler2.AppendLiteral("‖");
							defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(affliction2.Prefab.Name);
							defaultInterpolatedStringHandler2.AppendLiteral("‖color:end‖");
							guicomponent.ToolTip = defaultInterpolatedStringHandler2.ToStringAndClear() + "\n" + affliction2.Prefab.GetDescription(affliction2.Strength, AfflictionPrefab.Description.TargetType.Self);
						}
						if (affliction2 == this.pressureAffliction)
						{
							matchingIcon.ToolTip = TextManager.Get("PressureHUDWarning");
						}
						else if (affliction2 == this.pressureAffliction)
						{
							matchingIcon.ToolTip = TextManager.Get("OxygenHUDWarning");
						}
						matchingIcon.ToolTip = RichString.Rich(matchingIcon.ToolTip, null);
					}
					GUIImage image = matchingIcon.GetChild<GUIImage>();
					image.Color = color;
					image.HoverColor = Color.Lerp(image.Color, Color.White, 0.5f);
					if (affliction2.DamagePerSecond > 1f && matchingIcon.FlashTimer <= 0f)
					{
						GUIComponent guicomponent2 = matchingIcon;
						Vector2? flashRectInflate = new Vector2?(Vector2.One * 15f * GUI.Scale);
						guicomponent2.Flash(null, 1.5f, false, true, flashRectInflate);
						image.Pulsate(Vector2.One, Vector2.One * 1.2f, 1f);
					}
				}
				this.afflictionIconRefreshTimer -= deltaTime;
				if (this.afflictionIconRefreshTimer <= 0f)
				{
					this.afflictionIconContainer.RectTransform.SortChildren(delegate(RectTransform r1, RectTransform r2)
					{
						object userData = r1.GUIComponent.UserData;
						AfflictionPrefab prefab1 = userData as AfflictionPrefab;
						if (prefab1 == null)
						{
							return -1;
						}
						userData = r2.GUIComponent.UserData;
						AfflictionPrefab prefab2 = userData as AfflictionPrefab;
						if (prefab2 == null)
						{
							return 1;
						}
						int index = this.statusIcons.IndexOf((Affliction s) => s.Prefab == prefab1);
						int index2 = this.statusIcons.IndexOf((Affliction s) => s.Prefab == prefab2);
						return index.CompareTo(index2);
					});
					(this.afflictionIconContainer as GUILayoutGroup).NeedsToRecalculate = true;
					this.afflictionIconRefreshTimer = 1f;
				}
				Rectangle hiddenAfflictionHoverArea = this.showHiddenAfflictionsButton.Rect;
				foreach (GUIComponent child in this.hiddenAfflictionIconContainer.Children)
				{
					hiddenAfflictionHoverArea = Rectangle.Union(hiddenAfflictionHoverArea, child.Rect);
				}
				this.afflictionIconContainer.Visible = true;
				this.hiddenAfflictionIconContainer.Visible = (this.showHiddenAfflictionsButton.Rect.Contains(PlayerInput.MousePosition) || (this.hiddenAfflictionIconContainer.Visible && hiddenAfflictionHoverArea.Contains(PlayerInput.MousePosition)));
				this.showHiddenAfflictionsButton.Visible = (this.hiddenAfflictionIconContainer.CountChildren > 0);
				this.showHiddenAfflictionsButton.IgnoreLayoutGroups = !this.showHiddenAfflictionsButton.Visible;
				GUIButton guibutton2 = this.showHiddenAfflictionsButton;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(1, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("+");
				defaultInterpolatedStringHandler3.AppendFormatted<int>(this.hiddenAfflictionIconContainer.CountChildren);
				guibutton2.Text = defaultInterpolatedStringHandler3.ToStringAndClear();
				if (this.Vitality > 0f)
				{
					float currHealth = this.healthBar.BarSize;
					Color prevColor = this.healthBar.Color;
					this.healthBarShadow.BarSize = this.healthShadowSize;
					this.healthBarShadow.Color = Color.Lerp(GUIStyle.Red, Color.Black, 0.5f);
					this.healthBarShadow.Visible = true;
					this.healthBar.BarSize = currHealth;
					this.healthBar.Color = prevColor;
					return;
				}
				this.healthBarShadow.Visible = false;
				return;
			}
			else
			{
				this.afflictionIconContainer.Visible = (this.hiddenAfflictionIconContainer.Visible = false);
				if (this.Vitality > 0f)
				{
					float currHealth2 = this.healthWindowHealthBar.BarSize;
					Color prevColor2 = this.healthWindowHealthBar.Color;
					this.healthWindowHealthBarShadow.BarSize = this.healthShadowSize;
					this.healthWindowHealthBarShadow.Color = GUIStyle.Red;
					this.healthWindowHealthBarShadow.Visible = true;
					this.healthWindowHealthBar.BarSize = currHealth2;
					this.healthWindowHealthBar.Color = prevColor2;
					return;
				}
				this.healthWindowHealthBarShadow.Visible = false;
				return;
			}
		}

		// Token: 0x0600071F RID: 1823 RVA: 0x00040034 File Offset: 0x0003E234
		public static Color GetAfflictionIconColor(AfflictionPrefab prefab, Affliction affliction)
		{
			return CharacterHealth.GetAfflictionIconColor(prefab, affliction.Strength);
		}

		// Token: 0x06000720 RID: 1824 RVA: 0x00040044 File Offset: 0x0003E244
		public static Color GetAfflictionIconColor(AfflictionPrefab prefab, float afflictionStrength)
		{
			float colorT = MathF.Sqrt(afflictionStrength / prefab.MaxStrength);
			if (prefab.IconColors != null)
			{
				return ToolBox.GradientLerp(colorT, prefab.IconColors);
			}
			if (prefab.IsBuff)
			{
				return ToolBox.GradientLerp(colorT, new Color[]
				{
					GUIStyle.BuffColorLow,
					GUIStyle.BuffColorMedium,
					GUIStyle.BuffColorHigh
				});
			}
			return ToolBox.GradientLerp(colorT, new Color[]
			{
				GUIStyle.DebuffColorLow,
				GUIStyle.DebuffColorMedium,
				GUIStyle.DebuffColorHigh
			});
		}

		// Token: 0x06000721 RID: 1825 RVA: 0x000400FE File Offset: 0x0003E2FE
		public static Color GetAfflictionIconColor(Affliction affliction)
		{
			return CharacterHealth.GetAfflictionIconColor(affliction.Prefab, affliction);
		}

		// Token: 0x06000722 RID: 1826 RVA: 0x0004010C File Offset: 0x0003E30C
		private void UpdateAfflictionContainer(CharacterHealth.LimbHealth selectedLimb)
		{
			CharacterHealth.<>c__DisplayClass95_0 CS$<>8__locals1 = new CharacterHealth.<>c__DisplayClass95_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.selectedLimb = selectedLimb;
			if (CS$<>8__locals1.selectedLimb == null)
			{
				this.afflictionIconList.Content.ClearChildren();
				return;
			}
			if (CS$<>8__locals1.<UpdateAfflictionContainer>g__afflictionsDirty|1() || CS$<>8__locals1.selectedLimb != this.currentDisplayedLimb)
			{
				IEnumerable<Affliction> currentAfflictions = from a in this.afflictions
				where CS$<>8__locals1.<>4__this.ShouldDisplayAfflictionOnLimb(a, CS$<>8__locals1.selectedLimb)
				select a.Key;
				this.CreateAfflictionInfos(currentAfflictions);
				this.CreateRecommendedTreatments();
				return;
			}
			if (this.displayedAfflictions.Any(([TupleElementNames(new string[]
			{
				"affliction",
				"strength"
			})] ValueTuple<Affliction, float> d) => Math.Abs(d.Item2 - d.Item1.Strength) > 1f))
			{
				this.CreateRecommendedTreatments();
			}
		}

		// Token: 0x06000723 RID: 1827 RVA: 0x000401D8 File Offset: 0x0003E3D8
		private void CreateAfflictionInfos(IEnumerable<Affliction> afflictions)
		{
			this.afflictionIconList.ClearChildren();
			this.displayedAfflictions.Clear();
			Affliction mostSevereAffliction = CharacterHealth.SortAfflictionsBySeverity(afflictions, false).FirstOrDefault<Affliction>();
			GUIButton buttonToSelect = null;
			foreach (Affliction affliction in afflictions)
			{
				CharacterHealth.<>c__DisplayClass96_0 CS$<>8__locals1 = new CharacterHealth.<>c__DisplayClass96_0();
				this.displayedAfflictions.Add(new ValueTuple<Affliction, float>(affliction, affliction.Strength));
				GUIButton frame = new GUIButton(new RectTransform(new Vector2(1f, 0.25f), this.afflictionIconList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.Center, "ListBoxElement", null)
				{
					UserData = affliction,
					OnClicked = new GUIButton.OnClickedHandler(this.SelectAffliction)
				};
				new GUIFrame(new RectTransform(Vector2.One, frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "GUIFrameListBox", null).CanBeFocused = false;
				GUILayoutGroup content = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.85f), frame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopCenter)
				{
					Stretch = true,
					CanBeFocused = false
				};
				GUIProgressBar progressbarBg = new GUIProgressBar(new RectTransform(new Vector2(1f, 0.18f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), 0f, new Color?(GUIStyle.Green), "GUIAfflictionBar", true)
				{
					UserData = "afflictionstrengthprediction",
					CanBeFocused = false
				};
				GUIProgressBar guiprogressBar = new GUIProgressBar(new RectTransform(Vector2.One, progressbarBg.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), 0f, new Color?(Color.Transparent), "GUIAfflictionBar", false);
				guiprogressBar.UserData = "afflictionstrength";
				guiprogressBar.CanBeFocused = false;
				new GUIFrame(new RectTransform(new Vector2(1f, 0.15f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null).CanBeFocused = false;
				if (affliction == mostSevereAffliction)
				{
					buttonToSelect = frame;
				}
				GUIImage afflictionIcon = new GUIImage(new RectTransform(Vector2.One * 0.8f, content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), affliction.Prefab.Icon, true, null)
				{
					Color = CharacterHealth.GetAfflictionIconColor(affliction),
					CanBeFocused = false
				};
				afflictionIcon.PressedColor = afflictionIcon.Color;
				afflictionIcon.HoverColor = Color.Lerp(afflictionIcon.Color, Color.White, 0.6f);
				afflictionIcon.SelectedColor = Color.Lerp(afflictionIcon.Color, Color.White, 0.5f);
				CharacterHealth.<>c__DisplayClass96_0 CS$<>8__locals2 = CS$<>8__locals1;
				RectTransform rectT = new RectTransform(new Vector2(1.1f, 0f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text = affliction.Prefab.Name;
				GUIFont smallFont = GUIStyle.SmallFont;
				CS$<>8__locals2.nameText = new GUITextBlock(rectT, text, null, smallFont, Alignment.BottomCenter, false, "", null)
				{
					CanBeFocused = false
				};
				CS$<>8__locals1.nameText.Text = ToolBox.LimitString(CS$<>8__locals1.nameText.Text, CS$<>8__locals1.nameText.Font, CS$<>8__locals1.nameText.Rect.Width);
				CS$<>8__locals1.nameText.RectTransform.MinSize = new Point(0, (int)CS$<>8__locals1.nameText.TextSize.Y);
				CS$<>8__locals1.nameText.RectTransform.SizeChanged += delegate()
				{
					CS$<>8__locals1.nameText.Text = ToolBox.LimitString(CS$<>8__locals1.nameText.Text, CS$<>8__locals1.nameText.Font, CS$<>8__locals1.nameText.Rect.Width);
				};
				content.Recalculate();
			}
			if (buttonToSelect != null)
			{
				buttonToSelect.OnClicked(buttonToSelect, buttonToSelect.UserData);
			}
			this.afflictionIconList.RecalculateChildren();
		}

		// Token: 0x06000724 RID: 1828 RVA: 0x0004068C File Offset: 0x0003E88C
		private void CreateRecommendedTreatments()
		{
			ItemPrefab prevHighlightedItem = null;
			GUIComponent mouseOn = GUI.MouseOn;
			if (((mouseOn != null) ? mouseOn.UserData : null) is ItemPrefab && this.recommendedTreatmentContainer.Content.IsParentOf(GUI.MouseOn, true))
			{
				prevHighlightedItem = (ItemPrefab)GUI.MouseOn.UserData;
			}
			this.recommendedTreatmentContainer.Content.ClearChildren();
			float num = (Character.Controlled == null) ? 0f : Character.Controlled.GetSkillLevel(Tags.MedicalSkill);
			Dictionary<Identifier, float> treatmentSuitability = new Dictionary<Identifier, float>();
			this.GetSuitableTreatments(treatmentSuitability, Character.Controlled, (this.selectedLimbIndex == -1) ? null : this.Character.AnimController.Limbs.Find((Limb l) => l.HealthIndex == this.selectedLimbIndex), true, false, true, 0f);
			foreach (Identifier treatment in treatmentSuitability.Keys.ToList<Identifier>())
			{
				if (Character.Controlled.Inventory.FindItemByIdentifier(treatment, true) != null)
				{
					Dictionary<Identifier, float> dictionary = treatmentSuitability;
					Identifier key = treatment;
					dictionary[key] *= 10f;
				}
			}
			if (!treatmentSuitability.Any<KeyValuePair<Identifier, float>>())
			{
				new GUITextBlock(new RectTransform(Vector2.One, this.recommendedTreatmentContainer.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("none"), null, null, Alignment.Center, false, "", null).CanBeFocused = false;
				this.recommendedTreatmentContainer.ScrollBarVisible = false;
				this.recommendedTreatmentContainer.AutoHideScrollBar = false;
			}
			else
			{
				this.recommendedTreatmentContainer.ScrollBarVisible = true;
				this.recommendedTreatmentContainer.AutoHideScrollBar = true;
			}
			List<KeyValuePair<Identifier, float>> treatmentSuitabilities = (from t in treatmentSuitability
			orderby t.Value descending
			select t).ToList<KeyValuePair<Identifier, float>>();
			int count = 0;
			foreach (KeyValuePair<Identifier, float> treatment2 in treatmentSuitabilities)
			{
				if (treatment2.Value >= 0f)
				{
					count++;
					if (count > 5)
					{
						break;
					}
					ItemPrefab item = MapEntityPrefab.FindByIdentifier(treatment2.Key) as ItemPrefab;
					if (item != null)
					{
						GUIFrame itemSlot = new GUIFrame(new RectTransform(new Vector2(0.16666667f, 1f), this.recommendedTreatmentContainer.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
						{
							UserData = item
						};
						GUIButton innerFrame = new GUIButton(new RectTransform(Vector2.One, itemSlot.RectTransform, Anchor.Center, new Pivot?(Pivot.Center), null, null, ScaleBasis.Smallest), Alignment.Center, "SubtreeHeader", null)
						{
							UserData = item,
							DisabledColor = Color.White * 0.1f,
							PlaySoundOnSelect = false,
							OnClicked = delegate(GUIButton btn, object userdata)
							{
								ItemPrefab itemPrefab = userdata as ItemPrefab;
								if (itemPrefab == null)
								{
									return false;
								}
								Item item2 = AIObjectiveRescue.FindMedicalItem(Character.Controlled.Inventory, (Item it) => it.Prefab == itemPrefab);
								if (item2 == null)
								{
									return false;
								}
								Limb targetLimb = this.Character.AnimController.Limbs.FirstOrDefault((Limb l) => l.HealthIndex == this.selectedLimbIndex);
								item2.ApplyTreatment(Character.Controlled, this.Character, targetLimb);
								SoundPlayer.PlayUISound(GUISoundType.Select);
								return true;
							}
						};
						GUIImage guiimage = new GUIImage(new RectTransform(Vector2.One, innerFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "TalentBackgroundGlow", GUIImage.ScalingMode.None);
						guiimage.CanBeFocused = false;
						guiimage.Color = GUIStyle.Green;
						guiimage.HoverColor = Color.White;
						guiimage.PressedColor = Color.DarkGray;
						guiimage.SelectedColor = Color.Transparent;
						guiimage.DisabledColor = Color.Transparent;
						Sprite itemSprite = item.InventoryIcon ?? item.Sprite;
						Color itemColor = (itemSprite == item.Sprite) ? item.SpriteColor : item.InventoryIconColor;
						GUIImage guiimage2 = new GUIImage(new RectTransform(new Vector2(0.8f, 0.8f), innerFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), itemSprite, true, null);
						guiimage2.CanBeFocused = false;
						guiimage2.Color = itemColor * 0.9f;
						guiimage2.HoverColor = itemColor;
						guiimage2.SelectedColor = itemColor;
						guiimage2.DisabledColor = itemColor * 0.8f;
						if (item == prevHighlightedItem)
						{
							innerFrame.State = GUIComponent.ComponentState.Hover;
							innerFrame.Children.ForEach(delegate(GUIComponent c)
							{
								c.State = GUIComponent.ComponentState.Hover;
							});
						}
					}
				}
			}
			this.recommendedTreatmentContainer.RecalculateChildren();
			this.afflictionIconList.Content.RectTransform.SortChildren(delegate(RectTransform r1, RectTransform r2)
			{
				Affliction first = r1.GUIComponent.UserData as Affliction;
				Affliction second = r2.GUIComponent.UserData as Affliction;
				int dmgPerSecond = Math.Sign(second.DamagePerSecond - first.DamagePerSecond);
				if (dmgPerSecond == 0)
				{
					return Math.Sign(second.Strength - first.Strength);
				}
				return dmgPerSecond;
			});
			if (count > 0)
			{
				int treatmentIconSize = this.recommendedTreatmentContainer.Content.Children.Sum((GUIComponent c) => c.Rect.Width + this.recommendedTreatmentContainer.Spacing);
				if (treatmentIconSize < this.recommendedTreatmentContainer.Content.Rect.Width)
				{
					GUIFrame spacing = new GUIFrame(new RectTransform(new Point((this.recommendedTreatmentContainer.Content.Rect.Width - treatmentIconSize) / 2, 0), this.recommendedTreatmentContainer.Content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), null, null)
					{
						CanBeFocused = false
					};
					spacing.RectTransform.SetAsFirstChild();
				}
			}
		}

		// Token: 0x06000725 RID: 1829 RVA: 0x00040C6C File Offset: 0x0003EE6C
		private void CreateAfflictionInfoElements(GUIComponent parent, Affliction affliction)
		{
			GUILayoutGroup labelContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.2f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true,
				AbsoluteSpacing = 10,
				UserData = "label",
				CanBeFocused = false
			};
			RectTransform rectT = new RectTransform(new Vector2(0.65f, 1f), labelContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = affliction.Prefab.Name;
			GUIFont font = GUIStyle.LargeFont;
			GUITextBlock afflictionName = new GUITextBlock(rectT, text, null, font, Alignment.CenterLeft, false, "", null)
			{
				CanBeFocused = false,
				AutoScaleHorizontal = true
			};
			RectTransform rectT2 = new RectTransform(new Vector2(0.35f, 0.6f), labelContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = "";
			font = GUIStyle.SubHeadingFont;
			GUITextBlock afflictionStrength = new GUITextBlock(rectT2, text2, null, font, Alignment.TopRight, false, "", null)
			{
				UserData = "strength",
				CanBeFocused = false
			};
			GUITextBlock vitality = new GUITextBlock(new RectTransform(new Vector2(1f, 0.4f), labelContainer.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.BottomRight, false, "", null)
			{
				Padding = afflictionStrength.Padding,
				IgnoreLayoutGroups = true,
				UserData = "vitality",
				CanBeFocused = false
			};
			this.prevHighlightedAfflictionDescription = affliction.Prefab.GetDescription(affliction.Strength, (this.Character == Character.Controlled) ? AfflictionPrefab.Description.TargetType.Self : AfflictionPrefab.Description.TargetType.OtherCharacter);
			GUITextBlock description = new GUITextBlock(new RectTransform(new Vector2(1f, 0.3f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), RichString.Rich(this.prevHighlightedAfflictionDescription, null), null, null, Alignment.TopLeft, true, "", null)
			{
				UserData = "description",
				CanBeFocused = false
			};
			if (description.Font.MeasureString(description.WrappedText, false).Y > (float)description.Rect.Height)
			{
				description.Font = GUIStyle.SmallFont;
			}
			Point nameDims = new Point(afflictionName.Rect.Width, (int)(GUIStyle.LargeFont.Size * 1.5f));
			afflictionStrength.Text = affliction.GetStrengthText();
			Vector2 strengthDims = GUIStyle.SubHeadingFont.MeasureString(afflictionStrength.Text, false);
			labelContainer.RectTransform.Resize(new Point(labelContainer.Rect.Width, nameDims.Y), true);
			afflictionName.RectTransform.Resize(new Point((int)((float)labelContainer.Rect.Width - strengthDims.X * 0.99f), nameDims.Y), true);
			afflictionStrength.RectTransform.Resize(new Point(labelContainer.Rect.Width - afflictionName.Rect.Width, nameDims.Y), true);
			afflictionStrength.TextColor = Color.Lerp(GUIStyle.Orange, GUIStyle.Red, affliction.Strength / affliction.Prefab.MaxStrength);
			description.RectTransform.Resize(new Point(description.Rect.Width, (int)(description.TextSize.Y + 10f)), true);
			int vitalityDecrease = (int)this.GetVitalityDecreaseWithVitalityMultipliers(affliction);
			if (vitalityDecrease == 0)
			{
				vitality.Visible = false;
			}
			else
			{
				vitality.Visible = true;
				vitality.Text = TextManager.Get("Vitality") + " -" + vitalityDecrease;
				vitality.TextColor = ((vitalityDecrease <= 0) ? GUIStyle.Green : Color.Lerp(GUIStyle.Orange, GUIStyle.Red, affliction.Strength / affliction.Prefab.MaxStrength));
			}
			vitality.AutoDraw = true;
		}

		// Token: 0x06000726 RID: 1830 RVA: 0x00041110 File Offset: 0x0003F310
		private bool SelectAffliction(GUIButton button, object userData)
		{
			bool selected = button.Selected;
			foreach (GUIComponent child in this.afflictionIconList.Content.Children)
			{
				GUIButton btn = child as GUIButton;
				if (btn != null)
				{
					btn.Selected = (btn == button && !selected);
				}
			}
			return false;
		}

		// Token: 0x06000727 RID: 1831 RVA: 0x00041184 File Offset: 0x0003F384
		private void UpdateAfflictionInfos(IEnumerable<Affliction> afflictions)
		{
			Item potentialTreatment = Inventory.DraggingItems.FirstOrDefault<Item>();
			if (potentialTreatment == null)
			{
				GUIComponent mouseOn = GUI.MouseOn;
				object obj = (mouseOn != null) ? mouseOn.UserData : null;
				ItemPrefab itemPrefab = obj as ItemPrefab;
				if (itemPrefab != null)
				{
					potentialTreatment = Character.Controlled.Inventory.FindItem((Item it) => it.Prefab == itemPrefab, true);
				}
			}
			if (potentialTreatment == null)
			{
				Inventory.SlotReference selectedSlot = Inventory.SelectedSlot;
				potentialTreatment = ((selectedSlot != null) ? selectedSlot.Item : null);
			}
			foreach (Affliction affliction in afflictions)
			{
				float afflictionVitalityDecrease = this.GetVitalityDecreaseWithVitalityMultipliers(affliction);
				Color afflictionEffectColor = Color.White;
				if (afflictionVitalityDecrease > 0f)
				{
					afflictionEffectColor = GUIStyle.Red;
				}
				else if (afflictionVitalityDecrease < 0f)
				{
					afflictionEffectColor = GUIStyle.Green;
				}
				GUIComponent child = this.afflictionIconList.Content.FindChild(affliction, false);
				GUIProgressBar afflictionStrengthPredictionBar = child.GetChild<GUILayoutGroup>().GetChildByUserData("afflictionstrengthprediction") as GUIProgressBar;
				afflictionStrengthPredictionBar.BarSize = 0f;
				GUIProgressBar afflictionStrengthBar = afflictionStrengthPredictionBar.GetChildByUserData("afflictionstrength") as GUIProgressBar;
				afflictionStrengthBar.BarSize = affliction.Strength / affliction.Prefab.MaxStrength;
				afflictionStrengthBar.Color = afflictionEffectColor;
				float afflictionStrengthPrediction = this.GetAfflictionStrengthPrediction(potentialTreatment, affliction);
				if (!MathUtils.NearlyEqual(afflictionStrengthPrediction, affliction.Strength, 0.0001f))
				{
					float t = (float)Math.Max(0.5, (Math.Sin(Timing.TotalTime * 5.0) + 1.0) / 2.0);
					if (afflictionStrengthPrediction < affliction.Strength)
					{
						afflictionStrengthBar.Color = afflictionEffectColor;
						afflictionStrengthPredictionBar.Color = GUIStyle.Blue * t;
						afflictionStrengthPredictionBar.BarSize = afflictionStrengthBar.BarSize;
						afflictionStrengthBar.BarSize = afflictionStrengthPrediction / affliction.Prefab.MaxStrength;
					}
					else
					{
						afflictionStrengthPredictionBar.Color = Color.Red * t;
						afflictionStrengthPredictionBar.BarSize = afflictionStrengthPrediction / affliction.Prefab.MaxStrength;
					}
				}
				if (!affliction.Prefab.ShowBarInHealthMenu)
				{
					afflictionStrengthBar.BarSize = 1f;
				}
				if (this.afflictionTooltip != null && this.afflictionTooltip.UserData == affliction)
				{
					this.UpdateAfflictionInfo(this.afflictionTooltip.Content, affliction);
				}
			}
		}

		// Token: 0x06000728 RID: 1832 RVA: 0x00041408 File Offset: 0x0003F608
		private float GetAfflictionStrengthPrediction(Item item, Affliction affliction)
		{
			float strength = affliction.Strength;
			if (item == null)
			{
				return strength;
			}
			foreach (ItemComponent ic in item.Components)
			{
				List<StatusEffect> statusEffects;
				if (ic.statusEffectLists != null && ic.statusEffectLists.TryGetValue(ActionType.OnUse, out statusEffects))
				{
					foreach (StatusEffect effect in statusEffects)
					{
						foreach (ValueTuple<Identifier, float> reduceAffliction in effect.ReduceAffliction)
						{
							Identifier identifier = affliction.Identifier;
							if (!(reduceAffliction.Item1 != identifier) || !(reduceAffliction.Item1 != affliction.Prefab.AfflictionType))
							{
								strength -= reduceAffliction.Item2 * ((effect.Duration > 0f) ? effect.Duration : 1f);
							}
						}
						foreach (Affliction addAffliction in effect.Afflictions)
						{
							if (addAffliction.Prefab == affliction.Prefab)
							{
								strength += addAffliction.Strength * ((effect.Duration > 0f) ? effect.Duration : 1f);
							}
						}
					}
				}
			}
			return strength;
		}

		// Token: 0x06000729 RID: 1833 RVA: 0x00041600 File Offset: 0x0003F800
		private void UpdateAfflictionInfo(GUIComponent parent, Affliction affliction)
		{
			GUIComponent labelContainer = parent.GetChildByUserData("label");
			GUITextBlock strengthText = labelContainer.GetChildByUserData("strength") as GUITextBlock;
			strengthText.Text = affliction.GetStrengthText();
			strengthText.TextColor = Color.Lerp(GUIStyle.Orange, GUIStyle.Red, affliction.Strength / affliction.Prefab.MaxStrength);
			GUITextBlock vitalityText = labelContainer.GetChildByUserData("vitality") as GUITextBlock;
			int vitalityDecrease = (int)this.GetVitalityDecreaseWithVitalityMultipliers(affliction);
			if (vitalityDecrease == 0)
			{
				vitalityText.Visible = false;
			}
			else
			{
				vitalityText.Visible = true;
				vitalityText.Text = TextManager.Get("Vitality") + " -" + vitalityDecrease;
				vitalityText.TextColor = ((vitalityDecrease <= 0) ? GUIStyle.Green : Color.Lerp(GUIStyle.Orange, GUIStyle.Red, affliction.Strength / affliction.Prefab.MaxStrength));
			}
			LocalizedString newDescription = affliction.Prefab.GetDescription(affliction.Strength, (this.Character == Character.Controlled) ? AfflictionPrefab.Description.TargetType.Self : AfflictionPrefab.Description.TargetType.OtherCharacter);
			if (newDescription != this.prevHighlightedAfflictionDescription)
			{
				GUITextBlock descriptionText = parent.GetChildByUserData("description") as GUITextBlock;
				if (descriptionText != null)
				{
					descriptionText.Text = RichString.Rich(newDescription, null);
				}
				this.prevHighlightedAfflictionDescription = newDescription;
			}
		}

		// Token: 0x0600072A RID: 1834 RVA: 0x0004176C File Offset: 0x0003F96C
		public bool OnItemDropped(Item item, bool ignoreMousePos)
		{
			if (!ignoreMousePos && !this.healthWindow.Rect.Contains(PlayerInput.MousePosition))
			{
				return false;
			}
			if (this.Character.IsDead)
			{
				return true;
			}
			if (item == null || !item.UseInHealthInterface)
			{
				return true;
			}
			if (!ignoreMousePos && this.highlightedLimbIndex > -1)
			{
				this.selectedLimbIndex = this.highlightedLimbIndex;
			}
			Limb targetLimb = this.Character.AnimController.Limbs.FirstOrDefault((Limb l) => l.HealthIndex == this.selectedLimbIndex) ?? this.Character.AnimController.MainLimb;
			item.ApplyTreatment(Character.Controlled, this.Character, targetLimb);
			return true;
		}

		// Token: 0x0600072B RID: 1835 RVA: 0x00041818 File Offset: 0x0003FA18
		private void UpdateLimbIndicators(float deltaTime, Rectangle drawArea)
		{
			if (!GameMain.Instance.Paused)
			{
				this.limbIndicatorOverlayAnimState += deltaTime * 8f;
			}
			this.highlightedLimbIndex = -1;
			int i = 0;
			foreach (CharacterHealth.LimbHealth limbHealth in this.limbHealths)
			{
				if (limbHealth.IndicatorSprite != null)
				{
					float scale = Math.Min((float)drawArea.Width / (float)limbHealth.IndicatorSprite.SourceRect.Width, (float)drawArea.Height / (float)limbHealth.IndicatorSprite.SourceRect.Height);
					if (this.GetLimbHighlightArea(limbHealth, drawArea).Contains(PlayerInput.MousePosition))
					{
						this.highlightedLimbIndex = i;
					}
					i++;
				}
			}
			if (PlayerInput.PrimaryMouseButtonClicked() && this.highlightedLimbIndex > -1)
			{
				this.selectedLimbIndex = this.highlightedLimbIndex;
			}
		}

		// Token: 0x0600072C RID: 1836 RVA: 0x0004190C File Offset: 0x0003FB0C
		private void DrawHealthWindow(SpriteBatch spriteBatch, Rectangle drawArea, bool allowHighlight)
		{
			if (this.Character.Removed)
			{
				return;
			}
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, GameMain.ScissorTestEnable, GameMain.GameScreen.GradientEffect, null);
			int i = 0;
			foreach (CharacterHealth.LimbHealth limbHealth in this.limbHealths)
			{
				if (limbHealth.IndicatorSprite != null)
				{
					Rectangle limbEffectiveArea = new Rectangle(limbHealth.IndicatorSprite.SourceRect.X + limbHealth.HighlightArea.X, limbHealth.IndicatorSprite.SourceRect.Y + limbHealth.HighlightArea.Y, limbHealth.HighlightArea.Width, limbHealth.HighlightArea.Height);
					float totalDamage = this.GetTotalDamage(limbHealth);
					float num = (totalDamage > 0f) ? MathHelper.Lerp(0.2f, 1f, totalDamage / 100f) : 0f;
					float negativeEffect = 0f;
					float positiveEffect = 0f;
					foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp in this.afflictions)
					{
						if (kvp.Value == limbHealth)
						{
							Affliction affliction = kvp.Key;
							if (affliction.ShouldShowIcon(this.Character))
							{
								if (!affliction.Prefab.IsBuff)
								{
									negativeEffect += affliction.Strength * CharacterHealth.GetVitalityMultiplier(affliction, limbHealth);
								}
								else
								{
									positiveEffect += affliction.Strength * 0.2f;
								}
							}
						}
					}
					float midPoint = (float)limbEffectiveArea.Center.Y / (float)limbHealth.IndicatorSprite.Texture.Height;
					float fadeDist = 0.6f * (float)limbEffectiveArea.Height / (float)limbHealth.IndicatorSprite.Texture.Height;
					if (negativeEffect > 0f && negativeEffect < 5f)
					{
						negativeEffect = 10f;
					}
					if (positiveEffect > 0f && positiveEffect < 5f)
					{
						positiveEffect = 10f;
					}
					Color positiveColor = Color.Lerp(Color.Orange, Color.Lime, Math.Min(positiveEffect / 25f, 1f));
					Color negativeColor = Color.Lerp(Color.Orange, Color.Red, Math.Min(negativeEffect / 25f, 1f));
					Color color = Color.Orange;
					Color color2 = Color.Orange;
					if (negativeEffect + positiveEffect > 0f)
					{
						if (negativeEffect >= positiveEffect)
						{
							color = Color.Lerp(positiveColor, negativeColor, (negativeEffect - positiveEffect) / negativeEffect);
							color2 = negativeColor;
						}
						else
						{
							color = positiveColor;
							color2 = Color.Lerp(negativeColor, positiveColor, (positiveEffect - negativeEffect) / positiveEffect);
						}
					}
					if (this.Character.IsDead)
					{
						color = Color.Lerp(color, Color.Black, 0.75f);
						color2 = Color.Lerp(color2, Color.Black, 0.75f);
					}
					GameMain.GameScreen.GradientEffect.Parameters["color1"].SetValue(color.ToVector4());
					GameMain.GameScreen.GradientEffect.Parameters["color2"].SetValue(color2.ToVector4());
					GameMain.GameScreen.GradientEffect.Parameters["midPoint"].SetValue(midPoint);
					GameMain.GameScreen.GradientEffect.Parameters["fadeDist"].SetValue(fadeDist);
					float scale = Math.Min((float)drawArea.Width / (float)limbHealth.IndicatorSprite.SourceRect.Width, (float)drawArea.Height / (float)limbHealth.IndicatorSprite.SourceRect.Height);
					limbHealth.IndicatorSprite.Draw(spriteBatch, drawArea.Center.ToVector2(), Color.White, limbHealth.IndicatorSprite.Origin, 0f, scale, SpriteEffects.None, null);
					if (GameMain.DebugDraw)
					{
						Rectangle highlightArea = this.GetLimbHighlightArea(limbHealth, drawArea);
						GUI.DrawRectangle(spriteBatch, highlightArea, Color.Red, false, 0f, 1f);
						GUI.DrawRectangle(spriteBatch, drawArea, Color.Red, false, 0f, 1f);
					}
					i++;
				}
			}
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, CustomBlendStates.Multiplicative, null, null, null, null, null);
			if (this.limbIndicatorOverlay != null)
			{
				float overlayScale = Math.Min((float)drawArea.Width / (float)this.limbIndicatorOverlay.FrameSize.X, (float)drawArea.Height / (float)this.limbIndicatorOverlay.FrameSize.Y);
				int frameCount = 17;
				if (this.limbIndicatorOverlayAnimState >= (float)(frameCount * 2))
				{
					this.limbIndicatorOverlayAnimState = 0f;
				}
				int frame;
				if (this.limbIndicatorOverlayAnimState < (float)frameCount)
				{
					frame = (int)this.limbIndicatorOverlayAnimState;
				}
				else
				{
					frame = frameCount - (int)(this.limbIndicatorOverlayAnimState - (float)(frameCount - 1));
				}
				this.limbIndicatorOverlay.Draw(spriteBatch, frame, drawArea.Center.ToVector2(), Color.Gray, this.limbIndicatorOverlay.FrameSize.ToVector2() / 2f, 0f, Vector2.One * overlayScale, SpriteEffects.None, null);
			}
			if (allowHighlight)
			{
				i = 0;
				foreach (CharacterHealth.LimbHealth limbHealth2 in this.limbHealths)
				{
					if (limbHealth2.HighlightSprite != null)
					{
						float scale2 = Math.Min((float)drawArea.Width / (float)limbHealth2.HighlightSprite.SourceRect.Width, (float)drawArea.Height / (float)limbHealth2.HighlightSprite.SourceRect.Height);
						int drawCount = 0;
						if (i == this.highlightedLimbIndex)
						{
							drawCount++;
						}
						if (i == this.selectedLimbIndex)
						{
							drawCount++;
						}
						for (int j = 0; j < drawCount; j++)
						{
							limbHealth2.HighlightSprite.Draw(spriteBatch, drawArea.Center.ToVector2(), Color.White, limbHealth2.HighlightSprite.Origin, 0f, scale2, SpriteEffects.None, null);
						}
						i++;
					}
				}
			}
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, null, GameMain.ScissorTestEnable, null, null);
			i = 0;
			foreach (CharacterHealth.LimbHealth limbHealth3 in this.limbHealths)
			{
				CharacterHealth.afflictionsDisplayedOnLimb.Clear();
				foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> affliction2 in this.afflictions)
				{
					if (this.ShouldDisplayAfflictionOnLimb(affliction2, limbHealth3))
					{
						CharacterHealth.afflictionsDisplayedOnLimb.Add(affliction2.Key);
					}
				}
				if (!CharacterHealth.afflictionsDisplayedOnLimb.Any<Affliction>())
				{
					i++;
				}
				else if (limbHealth3.IndicatorSprite != null)
				{
					float scale3 = Math.Min((float)drawArea.Width / (float)limbHealth3.IndicatorSprite.SourceRect.Width, (float)drawArea.Height / (float)limbHealth3.IndicatorSprite.SourceRect.Height);
					Rectangle highlightArea2 = this.GetLimbHighlightArea(limbHealth3, drawArea);
					float iconScale = 0.25f * scale3;
					Vector2 iconPos = highlightArea2.Center.ToVector2();
					Affliction mostSevereAffliction = CharacterHealth.SortAfflictionsBySeverity(CharacterHealth.afflictionsDisplayedOnLimb, false).FirstOrDefault<Affliction>();
					if (mostSevereAffliction != null)
					{
						this.DrawLimbAfflictionIcon(spriteBatch, mostSevereAffliction, iconScale, ref iconPos);
					}
					if (CharacterHealth.afflictionsDisplayedOnLimb.Count<Affliction>() > 1)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 1);
						defaultInterpolatedStringHandler.AppendLiteral("+");
						defaultInterpolatedStringHandler.AppendFormatted<int>(CharacterHealth.afflictionsDisplayedOnLimb.Count<Affliction>() - 1);
						string additionalAfflictionCount = defaultInterpolatedStringHandler.ToStringAndClear();
						Vector2 displace = GUIStyle.SubHeadingFont.MeasureString(additionalAfflictionCount, false);
						GUIStyle.SubHeadingFont.DrawString(spriteBatch, additionalAfflictionCount, iconPos + new Vector2(displace.X * 1.1f, -displace.Y * 0.45f), Color.Black * 0.75f, ForceUpperCase.Inherit, false);
						GUIStyle.SubHeadingFont.DrawString(spriteBatch, additionalAfflictionCount, iconPos + new Vector2(displace.X, -displace.Y * 0.5f), Color.White, ForceUpperCase.Inherit, false);
					}
					i++;
				}
			}
			if (this.selectedLimbIndex > -1 && this.afflictionIconList.Content.CountChildren > 0)
			{
				CharacterHealth.LimbHealth limbHealth4 = this.limbHealths[this.selectedLimbIndex];
				if (((limbHealth4 != null) ? limbHealth4.IndicatorSprite : null) != null)
				{
					Rectangle selectedLimbArea = this.GetLimbHighlightArea(limbHealth4, drawArea);
					GUI.DrawLine(spriteBatch, new Vector2((float)this.afflictionIconList.Rect.X, (float)this.afflictionIconList.Rect.Y), selectedLimbArea.Center.ToVector2(), Color.LightGray * 0.5f, 0f, 4f);
				}
			}
		}

		// Token: 0x0600072D RID: 1837 RVA: 0x00042288 File Offset: 0x00040488
		private bool ShouldDisplayAfflictionOnLimb(KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp, CharacterHealth.LimbHealth limbHealth)
		{
			if (!kvp.Key.ShouldShowIcon(this.Character))
			{
				return false;
			}
			if (kvp.Value == limbHealth)
			{
				return true;
			}
			if (kvp.Value == null)
			{
				Limb indicatorLimb = this.Character.AnimController.GetLimb(kvp.Key.Prefab.IndicatorLimb, true, false, false);
				return indicatorLimb != null && indicatorLimb.HealthIndex == this.limbHealths.IndexOf(limbHealth);
			}
			return false;
		}

		// Token: 0x0600072E RID: 1838 RVA: 0x00042300 File Offset: 0x00040500
		private void DrawLimbAfflictionIcon(SpriteBatch spriteBatch, Affliction affliction, float iconScale, ref Vector2 iconPos)
		{
			if (!affliction.ShouldShowIcon(this.Character) || affliction.Prefab.Icon == null)
			{
				return;
			}
			Vector2 iconSize = affliction.Prefab.Icon.size * iconScale;
			Character controlled = Character.Controlled;
			float showIconThreshold = (((controlled != null) ? controlled.CharacterHealth : null) == this) ? affliction.Prefab.ShowIconThreshold : affliction.Prefab.ShowIconToOthersThreshold;
			float alpha = MathHelper.Lerp(0.3f, 1f, (affliction.Strength - showIconThreshold) / Math.Min(affliction.Prefab.MaxStrength - showIconThreshold, 10f));
			affliction.Prefab.Icon.Draw(spriteBatch, iconPos - iconSize / 2f, CharacterHealth.GetAfflictionIconColor(affliction) * alpha, 0f, iconScale, SpriteEffects.None, null);
			iconPos += new Vector2(10f, 20f) * iconScale;
		}

		// Token: 0x0600072F RID: 1839 RVA: 0x0004240C File Offset: 0x0004060C
		private Rectangle GetLimbHighlightArea(CharacterHealth.LimbHealth limbHealth, Rectangle drawArea)
		{
			float scale = Math.Min((float)drawArea.Width / (float)limbHealth.IndicatorSprite.SourceRect.Width, (float)drawArea.Height / (float)limbHealth.IndicatorSprite.SourceRect.Height);
			return new Rectangle((int)((float)drawArea.Center.X - (float)(limbHealth.IndicatorSprite.SourceRect.Width / 2 - limbHealth.HighlightArea.X) * scale), (int)((float)drawArea.Center.Y - (float)(limbHealth.IndicatorSprite.SourceRect.Height / 2 - limbHealth.HighlightArea.Y) * scale), (int)((float)limbHealth.HighlightArea.Width * scale), (int)((float)limbHealth.HighlightArea.Height * scale));
		}

		// Token: 0x06000730 RID: 1840 RVA: 0x000424D4 File Offset: 0x000406D4
		public void SetHealthBarVisibility(bool value)
		{
			this.healthBarHolder.Visible = value;
		}

		// Token: 0x06000731 RID: 1841 RVA: 0x000424E4 File Offset: 0x000406E4
		public void ClientRead(IReadMessage inc)
		{
			this.newAfflictions.Clear();
			this.newPeriodicEffects.Clear();
			bool newAdded = false;
			byte afflictionCount = inc.ReadByte();
			for (int i = 0; i < (int)afflictionCount; i++)
			{
				uint afflictionID = inc.ReadUInt32();
				AfflictionPrefab afflictionPrefab = AfflictionPrefab.Prefabs.Find((AfflictionPrefab p) => p.UintIdentifier == afflictionID);
				if (afflictionPrefab == null)
				{
					DebugConsole.ThrowError("Error while reading character health data: affliction with the uint ID " + afflictionID.ToString() + " not found.", null, null, false, false);
					inc.ReadRangedSingle(0f, 100f, 8);
					int _periodicAfflictionCount = (int)inc.ReadByte();
					for (int j = 0; j < _periodicAfflictionCount; j++)
					{
						inc.ReadByte();
					}
				}
				else
				{
					float afflictionStrength = inc.ReadRangedSingle(0f, afflictionPrefab.MaxStrength, 8);
					int periodicAfflictionCount = (int)inc.ReadByte();
					for (int k = 0; k < periodicAfflictionCount; k++)
					{
						float periodicAfflictionTimer = inc.ReadRangedSingle(0f, afflictionPrefab.PeriodicEffects[k].MaxInterval, 8);
						this.newPeriodicEffects.Add(new ValueTuple<AfflictionPrefab.PeriodicEffect, float>(afflictionPrefab.PeriodicEffects[k], periodicAfflictionTimer));
					}
					this.newAfflictions.Add(new ValueTuple<CharacterHealth.LimbHealth, AfflictionPrefab, float>(null, afflictionPrefab, afflictionStrength));
				}
			}
			byte limbAfflictionCount = inc.ReadByte();
			for (int l2 = 0; l2 < (int)limbAfflictionCount; l2++)
			{
				int limbIndex = inc.ReadRangedInteger(0, this.limbHealths.Count - 1);
				uint afflictionID = inc.ReadUInt32();
				AfflictionPrefab afflictionPrefab2 = AfflictionPrefab.Prefabs.Find((AfflictionPrefab p) => p.UintIdentifier == afflictionID);
				if (afflictionPrefab2 == null)
				{
					DebugConsole.ThrowError("Error while reading character health data: affliction with the uint ID " + afflictionID.ToString() + " not found.", null, null, false, false);
					inc.ReadRangedSingle(0f, 100f, 8);
					int _periodicAfflictionCount2 = (int)inc.ReadByte();
					for (int m = 0; m < _periodicAfflictionCount2; m++)
					{
						inc.ReadByte();
					}
				}
				else
				{
					float afflictionStrength2 = inc.ReadRangedSingle(0f, afflictionPrefab2.MaxStrength, 8);
					int periodicAfflictionCount2 = (int)inc.ReadByte();
					for (int n = 0; n < periodicAfflictionCount2; n++)
					{
						float periodicAfflictionTimer2 = inc.ReadRangedSingle(afflictionPrefab2.PeriodicEffects[n].MinInterval, afflictionPrefab2.PeriodicEffects[n].MaxInterval, 8);
						this.newPeriodicEffects.Add(new ValueTuple<AfflictionPrefab.PeriodicEffect, float>(afflictionPrefab2.PeriodicEffects[n], periodicAfflictionTimer2));
					}
					this.newAfflictions.Add(new ValueTuple<CharacterHealth.LimbHealth, AfflictionPrefab, float>(this.limbHealths[limbIndex], afflictionPrefab2, afflictionStrength2));
				}
			}
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> keyValuePair in this.afflictions)
			{
				Affliction affliction2;
				CharacterHealth.LimbHealth limbHealth2;
				keyValuePair.Deconstruct(out affliction2, out limbHealth2);
				Affliction affliction = affliction2;
				CharacterHealth.LimbHealth limbHealth = limbHealth2;
				if (this.newAfflictions.None(([TupleElementNames(new string[]
				{
					"limb",
					"afflictionPrefab",
					"strength"
				})] ValueTuple<CharacterHealth.LimbHealth, AfflictionPrefab, float> a) => affliction.Prefab == a.Item2 && limbHealth == a.Item1))
				{
					affliction.Strength = 0f;
				}
			}
			foreach (ValueTuple<CharacterHealth.LimbHealth, AfflictionPrefab, float> valueTuple in this.newAfflictions)
			{
				CharacterHealth.LimbHealth limb = valueTuple.Item1;
				AfflictionPrefab afflictionPrefab3 = valueTuple.Item2;
				float strength = valueTuple.Item3;
				Affliction existingAffliction = null;
				foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> keyValuePair in this.afflictions)
				{
					Affliction affliction2;
					CharacterHealth.LimbHealth limbHealth2;
					keyValuePair.Deconstruct(out affliction2, out limbHealth2);
					Affliction affliction3 = affliction2;
					CharacterHealth.LimbHealth limbHealth3 = limbHealth2;
					if (affliction3.Prefab == afflictionPrefab3 && limbHealth3 == limb)
					{
						existingAffliction = affliction3;
						break;
					}
				}
				if (existingAffliction == null)
				{
					existingAffliction = afflictionPrefab3.Instantiate(strength, null);
					this.afflictions.Add(existingAffliction, limb);
					newAdded = true;
				}
				existingAffliction.SetStrength(strength);
				if (existingAffliction == this.stunAffliction)
				{
					this.Character.SetStun(existingAffliction.Strength, true, true);
				}
				Func<Limb, bool> <>9__3;
				foreach (ValueTuple<AfflictionPrefab.PeriodicEffect, float> periodicEffect in this.newPeriodicEffects)
				{
					if (existingAffliction.Prefab.PeriodicEffects.Contains(periodicEffect.Item1) && existingAffliction.Strength >= periodicEffect.Item1.MinStrength && (periodicEffect.Item1.MaxStrength <= 0f || existingAffliction.Strength <= periodicEffect.Item1.MaxStrength))
					{
						if (periodicEffect.Item2 - existingAffliction.PeriodicEffectTimers[periodicEffect.Item1] > periodicEffect.Item1.MinInterval / 2f)
						{
							foreach (StatusEffect effect in periodicEffect.Item1.StatusEffects)
							{
								IEnumerable<Limb> limbs = this.Character.AnimController.Limbs;
								Func<Limb, bool> predicate;
								if ((predicate = <>9__3) == null)
								{
									predicate = (<>9__3 = ((Limb l) => l.HealthIndex == this.limbHealths.IndexOf(limb)));
								}
								Limb targetLimb = limbs.FirstOrDefault(predicate);
								existingAffliction.ApplyStatusEffect(ActionType.OnActive, effect, 1f, this, targetLimb);
							}
						}
						existingAffliction.PeriodicEffectTimers[periodicEffect.Item1] = periodicEffect.Item2;
					}
				}
			}
			this.CalculateVitality();
			this.DisplayedVitality = this.Vitality;
			if (newAdded)
			{
				MedicalClinic.OnAfflictionCountChanged(this.Character);
			}
		}

		// Token: 0x17000201 RID: 513
		// (get) Token: 0x06000732 RID: 1842 RVA: 0x00042B28 File Offset: 0x00040D28
		// (set) Token: 0x06000733 RID: 1843 RVA: 0x00042B3F File Offset: 0x00040D3F
		protected float UnmodifiedMaxVitality
		{
			get
			{
				return this.Character.Params.Health.Vitality;
			}
			set
			{
				this.Character.Params.Health.Vitality = value;
			}
		}

		// Token: 0x17000202 RID: 514
		// (get) Token: 0x06000734 RID: 1844 RVA: 0x00042B57 File Offset: 0x00040D57
		// (set) Token: 0x06000735 RID: 1845 RVA: 0x00042B85 File Offset: 0x00040D85
		public bool DoesBleed
		{
			get
			{
				return this.Character.Params.Health.DoesBleed && !this.Character.Params.IsMachine;
			}
			private set
			{
				this.Character.Params.Health.DoesBleed = value;
			}
		}

		// Token: 0x17000203 RID: 515
		// (get) Token: 0x06000736 RID: 1846 RVA: 0x00042B9D File Offset: 0x00040D9D
		// (set) Token: 0x06000737 RID: 1847 RVA: 0x00042BB4 File Offset: 0x00040DB4
		public bool UseHealthWindow
		{
			get
			{
				return this.Character.Params.Health.UseHealthWindow;
			}
			set
			{
				this.Character.Params.Health.UseHealthWindow = value;
			}
		}

		// Token: 0x17000204 RID: 516
		// (get) Token: 0x06000738 RID: 1848 RVA: 0x00042BCC File Offset: 0x00040DCC
		// (set) Token: 0x06000739 RID: 1849 RVA: 0x00042BE3 File Offset: 0x00040DE3
		public float CrushDepth
		{
			get
			{
				return this.Character.Params.Health.CrushDepth;
			}
			private set
			{
				this.Character.Params.Health.CrushDepth = value;
			}
		}

		// Token: 0x17000205 RID: 517
		// (get) Token: 0x0600073A RID: 1850 RVA: 0x00042BFB File Offset: 0x00040DFB
		public Affliction BloodlossAffliction
		{
			get
			{
				return this.bloodlossAffliction;
			}
		}

		// Token: 0x17000206 RID: 518
		// (get) Token: 0x0600073B RID: 1851 RVA: 0x00042C03 File Offset: 0x00040E03
		public bool IsUnconscious
		{
			get
			{
				return this.Character.IsDead || (this.Vitality <= 0f && !this.Character.HasAbilityFlag(AbilityFlags.AlwaysStayConscious));
			}
		}

		// Token: 0x17000207 RID: 519
		// (get) Token: 0x0600073C RID: 1852 RVA: 0x00042C36 File Offset: 0x00040E36
		// (set) Token: 0x0600073D RID: 1853 RVA: 0x00042C3E File Offset: 0x00040E3E
		public float PressureKillDelay { get; private set; } = 5f;

		// Token: 0x17000208 RID: 520
		// (get) Token: 0x0600073E RID: 1854 RVA: 0x00042C47 File Offset: 0x00040E47
		public float Vitality
		{
			get
			{
				if (this.Character.IsDead)
				{
					return this.minVitality;
				}
				return this.vitality;
			}
		}

		// Token: 0x17000209 RID: 521
		// (get) Token: 0x0600073F RID: 1855 RVA: 0x00042C63 File Offset: 0x00040E63
		public float VitalityDisregardingDeath
		{
			get
			{
				return this.vitality;
			}
		}

		// Token: 0x1700020A RID: 522
		// (get) Token: 0x06000740 RID: 1856 RVA: 0x00042C6B File Offset: 0x00040E6B
		public float HealthPercentage
		{
			get
			{
				return MathUtils.Percentage(this.Vitality, this.MaxVitality);
			}
		}

		// Token: 0x1700020B RID: 523
		// (get) Token: 0x06000741 RID: 1857 RVA: 0x00042C80 File Offset: 0x00040E80
		public float MaxVitality
		{
			get
			{
				float max = this.UnmodifiedMaxVitality;
				Character character = this.Character;
				bool flag;
				if (character == null)
				{
					flag = (null != null);
				}
				else
				{
					CharacterInfo info = character.Info;
					if (info == null)
					{
						flag = (null != null);
					}
					else
					{
						Job job = info.Job;
						flag = (((job != null) ? job.Prefab : null) != null);
					}
				}
				if (flag)
				{
					max += this.Character.Info.Job.Prefab.VitalityModifier;
				}
				max *= this.Character.HumanPrefabHealthMultiplier;
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaign = (gameSession != null) ? gameSession.Campaign : null;
				if (campaign != null)
				{
					max *= (this.Character.IsOnPlayerTeam ? campaign.Settings.CrewVitalityMultiplier : campaign.Settings.NonCrewVitalityMultiplier);
				}
				max *= 1f + this.Character.GetStatValue(StatTypes.MaximumHealthMultiplier, true);
				return max * this.Character.HealthMultiplier;
			}
		}

		// Token: 0x1700020C RID: 524
		// (get) Token: 0x06000742 RID: 1858 RVA: 0x00042D4C File Offset: 0x00040F4C
		public float MinVitality
		{
			get
			{
				Character character = this.Character;
				bool flag;
				if (character == null)
				{
					flag = (null != null);
				}
				else
				{
					CharacterInfo info = character.Info;
					if (info == null)
					{
						flag = (null != null);
					}
					else
					{
						Job job = info.Job;
						flag = (((job != null) ? job.Prefab : null) != null);
					}
				}
				if (flag)
				{
					return -this.MaxVitality;
				}
				return this.minVitality;
			}
		}

		// Token: 0x1700020D RID: 525
		// (get) Token: 0x06000743 RID: 1859 RVA: 0x00042D88 File Offset: 0x00040F88
		// (set) Token: 0x06000744 RID: 1860 RVA: 0x00042D90 File Offset: 0x00040F90
		public Color FaceTint { get; private set; }

		// Token: 0x1700020E RID: 526
		// (get) Token: 0x06000745 RID: 1861 RVA: 0x00042D99 File Offset: 0x00040F99
		// (set) Token: 0x06000746 RID: 1862 RVA: 0x00042DA1 File Offset: 0x00040FA1
		public Color BodyTint { get; private set; }

		// Token: 0x1700020F RID: 527
		// (get) Token: 0x06000747 RID: 1863 RVA: 0x00042DAA File Offset: 0x00040FAA
		// (set) Token: 0x06000748 RID: 1864 RVA: 0x00042DE8 File Offset: 0x00040FE8
		public float OxygenAmount
		{
			get
			{
				if (!this.Character.NeedsOxygen || this.Unkillable || this.Character.GodMode)
				{
					return 100f;
				}
				return -this.oxygenLowAffliction.Strength + 100f;
			}
			set
			{
				if (!this.Character.NeedsOxygen || this.Unkillable || this.Character.GodMode)
				{
					return;
				}
				this.oxygenLowAffliction.Strength = MathHelper.Clamp(-value + 100f, 0f, 200f);
			}
		}

		// Token: 0x17000210 RID: 528
		// (get) Token: 0x06000749 RID: 1865 RVA: 0x00042E3A File Offset: 0x0004103A
		// (set) Token: 0x0600074A RID: 1866 RVA: 0x00042E47 File Offset: 0x00041047
		public float BloodlossAmount
		{
			get
			{
				return this.bloodlossAffliction.Strength;
			}
			set
			{
				this.bloodlossAffliction.Strength = MathHelper.Clamp(value, 0f, this.bloodlossAffliction.Prefab.MaxStrength);
			}
		}

		// Token: 0x17000211 RID: 529
		// (get) Token: 0x0600074B RID: 1867 RVA: 0x00042E6F File Offset: 0x0004106F
		// (set) Token: 0x0600074C RID: 1868 RVA: 0x00042E7C File Offset: 0x0004107C
		public float Stun
		{
			get
			{
				return this.stunAffliction.Strength;
			}
			set
			{
				if (this.Character.GodMode)
				{
					return;
				}
				this.stunAffliction.Strength = MathHelper.Clamp(value, 0f, this.stunAffliction.Prefab.MaxStrength);
			}
		}

		// Token: 0x17000212 RID: 530
		// (get) Token: 0x0600074D RID: 1869 RVA: 0x00042EB2 File Offset: 0x000410B2
		// (set) Token: 0x0600074E RID: 1870 RVA: 0x00042EBA File Offset: 0x000410BA
		public bool IsParalyzed { get; private set; }

		// Token: 0x17000213 RID: 531
		// (get) Token: 0x0600074F RID: 1871 RVA: 0x00042EC3 File Offset: 0x000410C3
		// (set) Token: 0x06000750 RID: 1872 RVA: 0x00042ECB File Offset: 0x000410CB
		public float StunTimer { get; private set; }

		// Token: 0x17000214 RID: 532
		// (get) Token: 0x06000751 RID: 1873 RVA: 0x00042ED4 File Offset: 0x000410D4
		// (set) Token: 0x06000752 RID: 1874 RVA: 0x00042EDC File Offset: 0x000410DC
		public bool WasInFullHealth { get; private set; }

		// Token: 0x17000215 RID: 533
		// (get) Token: 0x06000753 RID: 1875 RVA: 0x00042EE5 File Offset: 0x000410E5
		public Affliction PressureAffliction
		{
			get
			{
				return this.pressureAffliction;
			}
		}

		// Token: 0x06000754 RID: 1876 RVA: 0x00042EF0 File Offset: 0x000410F0
		public CharacterHealth(Character character)
		{
			this.Character = character;
			this.vitality = 100f;
			this.DoesBleed = true;
			this.UseHealthWindow = false;
			this.InitIrremovableAfflictions();
			this.limbHealths.Add(new CharacterHealth.LimbHealth());
			this.InitProjSpecific(null, character);
		}

		// Token: 0x06000755 RID: 1877 RVA: 0x00043038 File Offset: 0x00041238
		public CharacterHealth(ContentXElement element, Character character, ContentXElement limbHealthElement = null)
		{
			this.Character = character;
			this.InitIrremovableAfflictions();
			this.vitality = this.UnmodifiedMaxVitality;
			this.minVitality = element.GetAttributeFloat("MinVitality", character.IsHuman ? -100f : 0f);
			this.limbHealths.Clear();
			if (limbHealthElement == null)
			{
				limbHealthElement = element;
			}
			foreach (ContentXElement subElement in limbHealthElement.Elements())
			{
				if (subElement.Name.ToString().Equals("limb", StringComparison.OrdinalIgnoreCase))
				{
					this.limbHealths.Add(new CharacterHealth.LimbHealth(subElement, this));
				}
			}
			if (this.limbHealths.Count == 0)
			{
				this.limbHealths.Add(new CharacterHealth.LimbHealth());
			}
			this.InitProjSpecific(element, character);
		}

		// Token: 0x06000756 RID: 1878 RVA: 0x00043218 File Offset: 0x00041418
		public void CheckForErrors()
		{
			int i;
			int j;
			for (i = 0; i < this.limbHealths.Count; i = j + 1)
			{
				if (this.Character.AnimController.Limbs.None((Limb l) => l.HealthIndex == i))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(114, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Potential error in character \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Character.Prefab.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral("\": none of the limbs have been set to use the LimbHealth #");
					defaultInterpolatedStringHandler.AppendFormatted<int>(i);
					defaultInterpolatedStringHandler.AppendLiteral(", and it will do nothing. ");
					DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear() + "Did you forget to set the HealthIndex values of the limbs?", this.Character.ContentPackage);
				}
				j = i;
			}
		}

		// Token: 0x06000757 RID: 1879 RVA: 0x000432F4 File Offset: 0x000414F4
		private void InitIrremovableAfflictions()
		{
			this.irremovableAfflictions.Add(this.bloodlossAffliction = new Affliction(AfflictionPrefab.Bloodloss, 0f));
			this.irremovableAfflictions.Add(this.stunAffliction = new Affliction(AfflictionPrefab.Stun, 0f));
			this.irremovableAfflictions.Add(this.pressureAffliction = new Affliction(AfflictionPrefab.Pressure, 0f));
			this.irremovableAfflictions.Add(this.oxygenLowAffliction = new Affliction(AfflictionPrefab.OxygenLow, 0f));
			foreach (Affliction affliction in this.irremovableAfflictions)
			{
				this.afflictions.Add(affliction, null);
			}
		}

		// Token: 0x06000758 RID: 1880 RVA: 0x000433E0 File Offset: 0x000415E0
		private void InitProjSpecific(ContentXElement element, Character character)
		{
			this.DisplayedVitality = this.MaxVitality;
			Character character2 = character;
			character2.OnAttacked = (Character.OnAttackedHandler)Delegate.Combine(character2.OnAttacked, new Character.OnAttackedHandler(this.OnAttacked));
			this.healthWindow = new GUIFrame(new RectTransform(new Vector2(0.35f, 0.6f), GUI.Canvas, Anchor.Center, null, null, null, ScaleBasis.Smallest), "GUIFrameListBox", null);
			GUILayoutGroup healthWindowVerticalLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.95f), this.healthWindow.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			GUILayoutGroup nameContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.2f), healthWindowVerticalLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(0, 20)
			}, true, Anchor.TopLeft)
			{
				Stretch = true
			};
			new GUICustomComponent(new RectTransform(new Vector2(0.2f, 1f), nameContainer.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), delegate(SpriteBatch spriteBatch, GUICustomComponent component)
			{
				CharacterInfo info = character.Info;
				if (info == null)
				{
					return;
				}
				info.DrawIcon(spriteBatch, component.Rect.Center.ToVector2(), component.Rect.Size.ToVector2(), false);
			}, null);
			RectTransform rectT = new RectTransform(new Vector2(0.6f, 1f), nameContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = "";
			GUIFont font = GUIStyle.SubHeadingFont;
			this.characterName = new GUITextBlock(rectT, text, null, font, Alignment.CenterLeft, false, "", null)
			{
				AutoScaleHorizontal = true
			};
			new GUICustomComponent(new RectTransform(new Vector2(0.2f, 1f), nameContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), delegate(SpriteBatch spriteBatch, GUICustomComponent component)
			{
				CharacterInfo info = character.Info;
				if (info == null)
				{
					return;
				}
				info.DrawJobIcon(spriteBatch, component.Rect, character != Character.Controlled);
			}, null);
			GUIFrame healthBarContainer = new GUIFrame(new RectTransform(new Vector2(1f, 0.07f), healthWindowVerticalLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUIFrame healthBarIcon = new GUIFrame(new RectTransform(new Vector2(0.095f, 1f), healthBarContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "GUIHealthBarIcon", null);
			this.healthWindowHealthBarShadow = new GUIProgressBar(new RectTransform(new Vector2(0.91f, 1f), healthBarContainer.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), 1f, new Color?(GUIStyle.Green), "GUIHealthBar", true)
			{
				IsHorizontal = true
			};
			this.healthWindowHealthBar = new GUIProgressBar(new RectTransform(new Vector2(0.91f, 1f), healthBarContainer.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), 1f, new Color?(GUIStyle.Green), "GUIHealthBar", true)
			{
				IsHorizontal = true
			};
			new GUIFrame(new RectTransform(new Vector2(1f, 0.05f), healthWindowVerticalLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUILayoutGroup characterIndicatorArea = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.95f), healthWindowVerticalLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			GUIFrame left = new GUIFrame(new RectTransform(new Vector2(0.25f, 1f), characterIndicatorArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			this.InventorySlotContainer = new GUICustomComponent(new RectTransform(Vector2.One, left.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), delegate(SpriteBatch spriteBatch, GUICustomComponent component)
			{
				for (int i = 0; i < character.Inventory.Capacity; i++)
				{
					if (character.Inventory.SlotTypes[i] == InvSlotType.HealthInterface && !character.Inventory.HideSlot(i))
					{
						int width = this.Character.Inventory.visualSlots[i].Rect.Width;
						left.RectTransform.MinSize = new Point(width, left.RectTransform.MinSize.Y);
						if (this.afflictionIconList != null)
						{
							this.afflictionIconList.RectTransform.MinSize = new Point(width, this.afflictionIconList.RectTransform.MinSize.Y);
						}
						if (!Inventory.DraggingItems.Any<Item>())
						{
							goto IL_129;
						}
						if (!this.Character.Inventory.GetItemsAt(i).All((Item it) => Inventory.DraggingItems.Contains(it)))
						{
							goto IL_129;
						}
						bool flag = character.Inventory.visualSlots[i].MouseOn();
						IL_12A:
						bool drawItem = flag;
						Inventory.DrawSlot(spriteBatch, this.Character.Inventory, this.Character.Inventory.visualSlots[i], this.Character.Inventory.GetItemAt(i), i, drawItem, this.Character.Inventory.SlotTypes[i]);
						if (this.medUIExtra != null)
						{
							float overlayScale = Math.Min((float)this.Character.Inventory.visualSlots[i].Rect.Width / (float)this.medUIExtra.FrameSize.X, (float)this.Character.Inventory.visualSlots[i].Rect.Height / (float)this.medUIExtra.FrameSize.Y);
							int frame = (int)this.medUIExtraAnimState;
							this.medUIExtra.Draw(spriteBatch, frame, this.Character.Inventory.visualSlots[i].Rect.Center.ToVector2(), Color.Gray, this.medUIExtra.FrameSize.ToVector2() / 2f, 0f, Vector2.One * overlayScale, SpriteEffects.None, null);
							goto IL_29D;
						}
						goto IL_29D;
						IL_129:
						flag = true;
						goto IL_12A;
					}
					IL_29D:;
				}
			}, delegate(float dt, GUICustomComponent component)
			{
				if (!GameMain.Instance.Paused)
				{
					this.medUIExtraAnimState = (this.medUIExtraAnimState + dt * 10f) % 16f;
				}
			});
			GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(0.75f), left.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Smallest), "", Alignment.Center, "CPRButton", null);
			guibutton.UserData = UIHighlightAction.ElementId.CPRButton;
			guibutton.OnClicked = delegate(GUIButton button, object userData)
			{
				Character controlled = Character.Controlled;
				Character selectedCharacter = (controlled != null) ? controlled.SelectedCharacter : null;
				if (selectedCharacter == null || (!selectedCharacter.IsUnconscious && selectedCharacter.Stun <= 0f))
				{
					return false;
				}
				Character.Controlled.AnimController.Anim = ((Character.Controlled.AnimController.Anim == AnimController.Animation.CPR) ? AnimController.Animation.None : AnimController.Animation.CPR);
				selectedCharacter.AnimController.ResetPullJoints(null);
				if (GameMain.Client != null)
				{
					GameMain.Client.CreateEntityEvent(Character.Controlled, default(Character.TreatmentEventData));
				}
				return true;
			};
			guibutton.ToolTip = TextManager.Get("tutorial.roles.medic.objective.cpr");
			guibutton.Visible = false;
			this.cprButton = guibutton;
			GUICustomComponent limbSelection = new GUICustomComponent(new RectTransform(new Vector2(0.5f, 1f), characterIndicatorArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), delegate(SpriteBatch spriteBatch, GUICustomComponent component)
			{
				this.DrawHealthWindow(spriteBatch, component.RectTransform.Rect, true);
			}, delegate(float deltaTime, GUICustomComponent component)
			{
				this.UpdateLimbIndicators(deltaTime, component.RectTransform.Rect);
			});
			RectTransform rectT2 = new RectTransform(new Vector2(0.9f, 0.1f), limbSelection.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal);
			RichString text2 = TextManager.Get("Deceased");
			font = GUIStyle.LargeFont;
			this.deadIndicator = new GUITextBlock(rectT2, text2, null, font, Alignment.Center, false, "GUIToolTip", null)
			{
				Visible = false,
				CanBeFocused = false
			};
			if (this.deadIndicator.Text.Contains(' ', StringComparison.Ordinal))
			{
				this.deadIndicator.Wrap = true;
			}
			else
			{
				this.deadIndicator.AutoScaleHorizontal = true;
			}
			this.afflictionIconList = new GUIListBox(new RectTransform(new Vector2(0.25f, 1f), characterIndicatorArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, null, true, false);
			RectTransform rectT3 = new RectTransform(new Vector2(1f, 0.1f), healthWindowVerticalLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text3 = TextManager.Get("SuitableTreatments");
			font = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT3, text3, null, font, Alignment.BottomCenter, false, "", null);
			this.treatmentLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.15f), healthWindowVerticalLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = false
			};
			this.recommendedTreatmentContainer = new GUIListBox(new RectTransform(new Vector2(1f, 1f), this.treatmentLayout.RectTransform, Anchor.Center, new Pivot?(Pivot.Center), null, null, ScaleBasis.Normal), true, null, null, true, false)
			{
				Spacing = GUI.IntScale(4f),
				KeepSpaceForScrollBar = false,
				ScrollBarVisible = false,
				AutoHideScrollBar = false
			};
			new GUITextBlock(new RectTransform(Vector2.One, this.recommendedTreatmentContainer.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("none"), null, null, Alignment.Center, false, "", null).CanBeFocused = false;
			this.healthBarHolder = new GUIFrame(new RectTransform(Point.Zero, GUI.Canvas, Anchor.TopLeft, null, ScaleBasis.Normal, false), null, null)
			{
				HoverCursor = CursorState.Hand
			};
			this.healthBarHolder.RectTransform.AbsoluteOffset = HUDLayoutSettings.HealthBarArea.Location;
			this.healthBarHolder.RectTransform.NonScaledSize = HUDLayoutSettings.HealthBarArea.Size;
			this.healthBarHolder.RectTransform.RelativeOffset = Vector2.Zero;
			this.healthBarShadow = new GUIProgressBar(new RectTransform(Vector2.One, this.healthBarHolder.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Normal), 1f, new Color?(Color.Green), "CharacterHealthBar", false)
			{
				Visible = false
			};
			this.healthShadowSize = 1f;
			GUIProgressBar guiprogressBar = new GUIProgressBar(new RectTransform(Vector2.One, this.healthBarHolder.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Normal), 1f, new Color?(GUIStyle.HealthBarColorHigh), "CharacterHealthBar", true);
			guiprogressBar.HoverCursor = CursorState.Hand;
			string tag = "hudbutton.healthinterface";
			string varName = "[key]";
			GameSettings.Config.KeyMapping keyMap = GameSettings.CurrentConfig.KeyMap;
			guiprogressBar.ToolTip = TextManager.GetWithVariable(tag, varName, keyMap.KeyBindText(InputType.Health), FormatCapitals.No);
			guiprogressBar.Enabled = true;
			this.healthBar = guiprogressBar;
			this.afflictionIconContainer = new GUILayoutGroup(HUDLayoutSettings.ToRectTransform(HUDLayoutSettings.HealthBarAfflictionArea, GUI.Canvas), true, Anchor.CenterRight)
			{
				AbsoluteSpacing = GUI.IntScale(5f)
			};
			this.showHiddenAfflictionsButton = new GUIButton(new RectTransform(new Point(this.afflictionIconContainer.Rect.Height), this.afflictionIconContainer.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), Alignment.Center, "GUIButtonCircular", null)
			{
				Visible = false,
				CanBeFocused = false
			};
			this.hiddenAfflictionIconContainer = new GUILayoutGroup(HUDLayoutSettings.ToRectTransform(HUDLayoutSettings.HealthBarAfflictionArea, GUI.Canvas), true, Anchor.CenterRight)
			{
				AbsoluteSpacing = GUI.IntScale(5f)
			};
			this.UpdateAlignment();
			this.SuicideButton = new GUIButton(new RectTransform(new Vector2(0.1f, 0.02f), GUI.Canvas, Anchor.TopCenter, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(150, 20),
				RelativeOffset = new Vector2(0f, 0.01f)
			}, TextManager.Get("GiveInButton"), Alignment.Center, "GUIButtonLarge", null)
			{
				Visible = false,
				ToolTip = TextManager.Get((GameMain.NetworkMember == null) ? "GiveInHelpSingleplayer" : "GiveInHelpMultiplayer"),
				OnClicked = delegate(GUIButton button, object userData)
				{
					GUI.ForceMouseOn(null);
					if (Character.Controlled != null)
					{
						if (GameMain.Client != null)
						{
							GameMain.Client.CreateEntityEvent(Character.Controlled, default(Character.CharacterStatusEventData));
						}
						else
						{
							ValueTuple<CauseOfDeathType, Affliction> causeOfDeath = this.GetCauseOfDeath();
							CauseOfDeathType type = causeOfDeath.Item1;
							Affliction affliction = causeOfDeath.Item2;
							Character.Controlled.Kill(type, affliction, false, true);
							Character.Controlled = null;
						}
					}
					return true;
				}
			};
			this.SuicideButton.TextBlock.AutoScaleHorizontal = true;
			ContentXElement contentXElement = null;
			if (element != contentXElement)
			{
				foreach (ContentXElement subElement in element.Elements())
				{
					string a = subElement.Name.ToString().ToLowerInvariant();
					if (!(a == "sprite") && !(a == "meduisilhouette"))
					{
						if (a == "meduiextra")
						{
							this.medUIExtra = new SpriteSheet(subElement, "", "");
						}
					}
					else
					{
						this.limbIndicatorOverlay = new SpriteSheet(subElement, "", "");
					}
				}
			}
			healthWindowVerticalLayout.Recalculate();
		}

		// Token: 0x06000759 RID: 1881 RVA: 0x00044084 File Offset: 0x00042284
		public IReadOnlyCollection<Affliction> GetAllAfflictions()
		{
			return this.afflictions.Keys;
		}

		// Token: 0x0600075A RID: 1882 RVA: 0x00044091 File Offset: 0x00042291
		public IEnumerable<Affliction> GetAllAfflictions(Func<Affliction, bool> limbHealthFilter)
		{
			return this.afflictions.Keys.Where(limbHealthFilter);
		}

		// Token: 0x0600075B RID: 1883 RVA: 0x000440A4 File Offset: 0x000422A4
		private float GetTotalDamage(CharacterHealth.LimbHealth limbHealth)
		{
			float totalDamage = 0f;
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp in this.afflictions)
			{
				if (kvp.Value == limbHealth)
				{
					Affliction affliction = kvp.Key;
					totalDamage += affliction.GetVitalityDecrease(this);
				}
			}
			return totalDamage;
		}

		// Token: 0x0600075C RID: 1884 RVA: 0x00044114 File Offset: 0x00042314
		private CharacterHealth.LimbHealth GetMatchingLimbHealth(Limb limb)
		{
			if (limb != null)
			{
				return this.limbHealths[limb.HealthIndex];
			}
			return null;
		}

		// Token: 0x0600075D RID: 1885 RVA: 0x0004412C File Offset: 0x0004232C
		private CharacterHealth.LimbHealth GetMatchingLimbHealth(Affliction affliction)
		{
			return this.GetMatchingLimbHealth(this.Character.AnimController.GetLimb(affliction.Prefab.IndicatorLimb, false, false, false));
		}

		// Token: 0x0600075E RID: 1886 RVA: 0x00044152 File Offset: 0x00042352
		public Affliction GetAffliction(string identifier, bool allowLimbAfflictions = true)
		{
			return this.GetAffliction(identifier.ToIdentifier(), allowLimbAfflictions);
		}

		// Token: 0x0600075F RID: 1887 RVA: 0x00044164 File Offset: 0x00042364
		public Affliction GetAffliction(Identifier identifier, bool allowLimbAfflictions = true)
		{
			return this.GetAffliction((Affliction a) => a.Prefab.Identifier == identifier, allowLimbAfflictions);
		}

		// Token: 0x06000760 RID: 1888 RVA: 0x00044194 File Offset: 0x00042394
		public Affliction GetAfflictionOfType(Identifier afflictionType, bool allowLimbAfflictions = true)
		{
			return this.GetAffliction((Affliction a) => a.Prefab.AfflictionType == afflictionType, allowLimbAfflictions);
		}

		// Token: 0x06000761 RID: 1889 RVA: 0x000441C4 File Offset: 0x000423C4
		private Affliction GetAffliction(Func<Affliction, bool> predicate, bool allowLimbAfflictions = true)
		{
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp in this.afflictions)
			{
				if ((allowLimbAfflictions || kvp.Value == null) && predicate(kvp.Key))
				{
					return kvp.Key;
				}
			}
			return null;
		}

		// Token: 0x06000762 RID: 1890 RVA: 0x00044238 File Offset: 0x00042438
		public T GetAffliction<T>(Identifier identifier, bool allowLimbAfflictions = true) where T : Affliction
		{
			return this.GetAffliction(identifier, allowLimbAfflictions) as T;
		}

		// Token: 0x06000763 RID: 1891 RVA: 0x0004424C File Offset: 0x0004244C
		public Affliction GetAffliction(Identifier identifier, Limb limb)
		{
			if (limb.HealthIndex < 0 || limb.HealthIndex >= this.limbHealths.Count)
			{
				DebugConsole.ThrowError(string.Concat(new string[]
				{
					"Limb health index out of bounds. Character\"",
					this.Character.Name,
					"\" only has health configured for",
					this.limbHealths.Count.ToString(),
					" limbs but the limb ",
					limb.type.ToString(),
					" is targeting index ",
					limb.HealthIndex.ToString()
				}), null, null, false, false);
				return null;
			}
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp in this.afflictions)
			{
				if (this.limbHealths[limb.HealthIndex] == kvp.Value && kvp.Key.Prefab.Identifier == identifier)
				{
					return kvp.Key;
				}
			}
			return null;
		}

		// Token: 0x06000764 RID: 1892 RVA: 0x0004437C File Offset: 0x0004257C
		public Limb GetAfflictionLimb(Affliction affliction)
		{
			CharacterHealth.LimbHealth limbHealth;
			if (affliction != null && this.afflictions.TryGetValue(affliction, out limbHealth))
			{
				if (limbHealth == null)
				{
					return null;
				}
				int limbHealthIndex = this.limbHealths.IndexOf(limbHealth);
				foreach (Limb limb in this.Character.AnimController.Limbs)
				{
					if (limb.HealthIndex == limbHealthIndex)
					{
						return limb;
					}
				}
			}
			return null;
		}

		// Token: 0x06000765 RID: 1893 RVA: 0x000443E0 File Offset: 0x000425E0
		public float GetAfflictionStrength(Identifier afflictionType, Limb limb, bool requireLimbSpecific)
		{
			if (requireLimbSpecific && this.limbHealths.Count == 1)
			{
				return 0f;
			}
			float strength = 0f;
			CharacterHealth.LimbHealth limbHealth = this.limbHealths[limb.HealthIndex];
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp in this.afflictions)
			{
				if (kvp.Value == limbHealth)
				{
					Affliction affliction = kvp.Key;
					if (affliction.Strength >= affliction.Prefab.ActivationThreshold && affliction.Prefab.AfflictionType == afflictionType)
					{
						strength += affliction.Strength;
					}
				}
			}
			return strength;
		}

		// Token: 0x06000766 RID: 1894 RVA: 0x000444A4 File Offset: 0x000426A4
		public float GetAfflictionStrengthByType(Identifier afflictionType, bool allowLimbAfflictions = true)
		{
			return this.GetAfflictionStrength(afflictionType, Identifier.Empty, allowLimbAfflictions);
		}

		// Token: 0x06000767 RID: 1895 RVA: 0x000444B3 File Offset: 0x000426B3
		public float GetAfflictionStrengthByIdentifier(Identifier afflictionIdentifier, bool allowLimbAfflictions = true)
		{
			return this.GetAfflictionStrength(Identifier.Empty, afflictionIdentifier, allowLimbAfflictions);
		}

		// Token: 0x06000768 RID: 1896 RVA: 0x000444C4 File Offset: 0x000426C4
		public float GetAfflictionStrength(Identifier afflictionType, Identifier afflictionidentifier, bool allowLimbAfflictions = true)
		{
			float strength = 0f;
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp in this.afflictions)
			{
				if (allowLimbAfflictions || kvp.Value == null)
				{
					Affliction affliction = kvp.Key;
					if (affliction.Strength >= affliction.Prefab.ActivationThreshold && (affliction.Prefab.AfflictionType == afflictionType || afflictionType.IsEmpty) && (affliction.Prefab.Identifier == afflictionidentifier || afflictionidentifier.IsEmpty))
					{
						strength += affliction.Strength;
					}
				}
			}
			return strength;
		}

		// Token: 0x06000769 RID: 1897 RVA: 0x00044584 File Offset: 0x00042784
		public void ApplyAffliction(Limb targetLimb, Affliction affliction, bool allowStacking = true, bool ignoreUnkillability = false, bool recalculateVitality = true)
		{
			if (this.Character.GodMode)
			{
				return;
			}
			if (!ignoreUnkillability && !affliction.Prefab.IsBuff && this.Unkillable)
			{
				return;
			}
			if (affliction.Prefab.LimbSpecific)
			{
				if (targetLimb == null)
				{
					using (List<CharacterHealth.LimbHealth>.Enumerator enumerator = this.limbHealths.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							CharacterHealth.LimbHealth limbHealth = enumerator.Current;
							this.AddLimbAffliction(limbHealth, null, affliction, allowStacking, recalculateVitality);
						}
						return;
					}
				}
				this.AddLimbAffliction(targetLimb, affliction, allowStacking, recalculateVitality);
				return;
			}
			this.AddAffliction(affliction, allowStacking);
		}

		// Token: 0x0600076A RID: 1898 RVA: 0x00044628 File Offset: 0x00042828
		public float GetResistance(AfflictionPrefab afflictionPrefab, LimbType limbType)
		{
			float resistance = 0f;
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp in this.afflictions)
			{
				Affliction affliction = kvp.Key;
				resistance += affliction.GetResistance(afflictionPrefab.Identifier, limbType);
			}
			float abilityResistanceMultiplier = this.Character.GetAbilityResistance(afflictionPrefab);
			return 1f - (1f - resistance) * abilityResistanceMultiplier;
		}

		// Token: 0x0600076B RID: 1899 RVA: 0x000446B0 File Offset: 0x000428B0
		public float GetStatValue(StatTypes statType)
		{
			float value = 0f;
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp in this.afflictions)
			{
				Affliction affliction = kvp.Key;
				value += affliction.GetStatValue(statType);
			}
			return value;
		}

		// Token: 0x0600076C RID: 1900 RVA: 0x00044718 File Offset: 0x00042918
		public bool HasFlag(AbilityFlags flagType)
		{
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp in this.afflictions)
			{
				Affliction affliction = kvp.Key;
				if (affliction.HasFlag(flagType))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600076D RID: 1901 RVA: 0x0004477C File Offset: 0x0004297C
		public void ReduceAllAfflictionsOnAllLimbs(float amount, ActionType? treatmentAction = null)
		{
			this.matchingAfflictions.Clear();
			this.matchingAfflictions.AddRange(this.afflictions.Keys);
			this.ReduceMatchingAfflictions(amount, treatmentAction, null);
		}

		// Token: 0x0600076E RID: 1902 RVA: 0x000447A8 File Offset: 0x000429A8
		public void ReduceAfflictionOnAllLimbs(Identifier afflictionIdOrType, float amount, ActionType? treatmentAction = null, Character attacker = null)
		{
			if (afflictionIdOrType.IsEmpty)
			{
				throw new ArgumentException("afflictionIdOrType is empty");
			}
			this.matchingAfflictions.Clear();
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> affliction in this.afflictions)
			{
				if (affliction.Key.Prefab.Identifier == afflictionIdOrType || affliction.Key.Prefab.AfflictionType == afflictionIdOrType)
				{
					this.matchingAfflictions.Add(affliction.Key);
				}
			}
			this.ReduceMatchingAfflictions(amount, treatmentAction, attacker);
		}

		// Token: 0x0600076F RID: 1903 RVA: 0x00044864 File Offset: 0x00042A64
		private IEnumerable<Affliction> GetAfflictionsForLimb(Limb targetLimb)
		{
			return from k in this.afflictions.Keys
			where this.afflictions[k] == this.limbHealths[targetLimb.HealthIndex]
			select k;
		}

		// Token: 0x06000770 RID: 1904 RVA: 0x000448A1 File Offset: 0x00042AA1
		public void ReduceAllAfflictionsOnLimb(Limb targetLimb, float amount, ActionType? treatmentAction = null)
		{
			if (targetLimb == null)
			{
				throw new ArgumentNullException("targetLimb");
			}
			this.matchingAfflictions.Clear();
			this.matchingAfflictions.AddRange(this.GetAfflictionsForLimb(targetLimb));
			this.ReduceMatchingAfflictions(amount, treatmentAction, null);
		}

		// Token: 0x06000771 RID: 1905 RVA: 0x000448D8 File Offset: 0x00042AD8
		public void ReduceAfflictionOnLimb(Limb targetLimb, Identifier afflictionIdOrType, float amount, ActionType? treatmentAction = null, Character attacker = null)
		{
			if (afflictionIdOrType.IsEmpty)
			{
				throw new ArgumentException("afflictionIdOrType is empty");
			}
			if (targetLimb == null)
			{
				throw new ArgumentNullException("targetLimb");
			}
			this.matchingAfflictions.Clear();
			CharacterHealth.LimbHealth targetLimbHealth = this.limbHealths[targetLimb.HealthIndex];
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> affliction in this.afflictions)
			{
				if ((affliction.Key.Prefab.Identifier == afflictionIdOrType || affliction.Key.Prefab.AfflictionType == afflictionIdOrType) && affliction.Value == targetLimbHealth)
				{
					this.matchingAfflictions.Add(affliction.Key);
				}
			}
			this.ReduceMatchingAfflictions(amount, treatmentAction, attacker);
		}

		// Token: 0x06000772 RID: 1906 RVA: 0x000449C0 File Offset: 0x00042BC0
		private void ReduceMatchingAfflictions(float amount, ActionType? treatmentAction, Character attacker = null)
		{
			if (this.matchingAfflictions.Count == 0)
			{
				return;
			}
			float reduceAmount = amount / (float)this.matchingAfflictions.Count;
			if (reduceAmount > 0f)
			{
				AbilityReduceAffliction abilityReduceAffliction = new AbilityReduceAffliction(this.Character, reduceAmount);
				if (attacker != null)
				{
					attacker.CheckTalents(AbilityEffectType.OnReduceAffliction, abilityReduceAffliction);
				}
				reduceAmount = abilityReduceAffliction.Value;
			}
			for (int i = this.matchingAfflictions.Count - 1; i >= 0; i--)
			{
				Affliction matchingAffliction = this.matchingAfflictions[i];
				if (matchingAffliction.Strength < reduceAmount)
				{
					float surplus = reduceAmount - matchingAffliction.Strength;
					amount -= matchingAffliction.Strength;
					matchingAffliction.Strength = 0f;
					this.matchingAfflictions.RemoveAt(i);
					if (i == 0)
					{
						i = this.matchingAfflictions.Count;
					}
					if (i > 0)
					{
						reduceAmount += surplus / (float)i;
					}
					AchievementManager.OnAfflictionRemoved(matchingAffliction, this.Character);
				}
				else
				{
					matchingAffliction.Strength -= reduceAmount;
					amount -= reduceAmount;
					if (treatmentAction != null)
					{
						if (treatmentAction.Value == ActionType.OnUse || treatmentAction.Value == ActionType.OnSuccess)
						{
							matchingAffliction.AppliedAsSuccessfulTreatmentTime = Timing.TotalTime;
						}
						else if (treatmentAction.Value == ActionType.OnFailure)
						{
							matchingAffliction.AppliedAsFailedTreatmentTime = Timing.TotalTime;
						}
					}
				}
			}
			this.CalculateVitality();
		}

		// Token: 0x06000773 RID: 1907 RVA: 0x00044AF4 File Offset: 0x00042CF4
		public void ApplyDamage(Limb hitLimb, AttackResult attackResult, bool allowStacking = true, bool recalculateVitality = true)
		{
			if (this.Unkillable || this.Character.GodMode)
			{
				return;
			}
			if (hitLimb.HealthIndex < 0 || hitLimb.HealthIndex >= this.limbHealths.Count)
			{
				DebugConsole.ThrowError(string.Concat(new string[]
				{
					"Limb health index out of bounds. Character\"",
					this.Character.Name,
					"\" only has health configured for",
					this.limbHealths.Count.ToString(),
					" limbs but the limb ",
					hitLimb.type.ToString(),
					" is targeting index ",
					hitLimb.HealthIndex.ToString()
				}), null, null, false, false);
				return;
			}
			bool? should = null;
			LuaCsSetup.Instance.EventService.PublishEvent<IEventCharacterApplyDamage>(delegate(IEventCharacterApplyDamage x)
			{
				bool? flag = x.OnCharacterApplyDamage(this, attackResult, hitLimb, allowStacking);
				should = ((flag != null) ? flag : should);
			});
			if (should != null && should.Value)
			{
				return;
			}
			foreach (Affliction newAffliction in attackResult.Afflictions)
			{
				if (newAffliction.Prefab.LimbSpecific)
				{
					this.AddLimbAffliction(hitLimb, newAffliction, allowStacking, recalculateVitality);
				}
				else
				{
					this.AddAffliction(newAffliction, allowStacking);
				}
			}
		}

		// Token: 0x06000774 RID: 1908 RVA: 0x00044CAC File Offset: 0x00042EAC
		private void KillIfOutOfVitality()
		{
			if (this.Vitality <= this.MinVitality && !this.Character.HasAbilityFlag(AbilityFlags.CanNotDieToAfflictions))
			{
				this.Kill();
			}
		}

		// Token: 0x06000775 RID: 1909 RVA: 0x00044CD4 File Offset: 0x00042ED4
		public void SetAllDamage(float damageAmount, float bleedingDamageAmount, float burnDamageAmount)
		{
			if (this.Unkillable || this.Character.GodMode)
			{
				return;
			}
			CharacterHealth.afflictionsToRemove.Clear();
			CharacterHealth.afflictionsToRemove.AddRange(from a in this.afflictions.Keys
			where a.Prefab.AfflictionType == AfflictionPrefab.InternalDamage.AfflictionType || a.Prefab.AfflictionType == AfflictionPrefab.Burn.AfflictionType || a.Prefab.AfflictionType == AfflictionPrefab.Bleeding.AfflictionType
			select a);
			foreach (Affliction affliction in CharacterHealth.afflictionsToRemove)
			{
				this.afflictions.Remove(affliction);
			}
			foreach (CharacterHealth.LimbHealth limbHealth in this.limbHealths)
			{
				if (damageAmount > 0f)
				{
					this.afflictions.Add(AfflictionPrefab.InternalDamage.Instantiate(damageAmount, null), limbHealth);
				}
				if (bleedingDamageAmount > 0f && this.DoesBleed)
				{
					this.afflictions.Add(AfflictionPrefab.Bleeding.Instantiate(bleedingDamageAmount, null), limbHealth);
				}
				if (burnDamageAmount > 0f)
				{
					this.afflictions.Add(AfflictionPrefab.Burn.Instantiate(burnDamageAmount, null), limbHealth);
				}
			}
			this.RecalculateVitality();
		}

		// Token: 0x06000776 RID: 1910 RVA: 0x00044E30 File Offset: 0x00043030
		public float GetLimbDamage(Limb limb, Identifier afflictionType)
		{
			if (limb.IsSevered)
			{
				return 1f;
			}
			float max = this.MaxVitality / 2f;
			float damageStrength;
			if (afflictionType.IsEmpty)
			{
				float damage = this.GetAfflictionStrength(AfflictionPrefab.DamageType, limb, true);
				float bleeding = this.GetAfflictionStrength(AfflictionPrefab.BleedingType, limb, true);
				float burn = this.GetAfflictionStrength(AfflictionPrefab.BurnType, limb, true);
				damageStrength = Math.Min(damage + bleeding + burn, max);
			}
			else
			{
				damageStrength = Math.Min(this.GetAfflictionStrength(afflictionType, limb, true), max);
			}
			return damageStrength / max;
		}

		// Token: 0x06000777 RID: 1911 RVA: 0x00044EB0 File Offset: 0x000430B0
		public void RemoveAfflictions(Func<Affliction, bool> predicate)
		{
			CharacterHealth.afflictionsToRemove.Clear();
			CharacterHealth.afflictionsToRemove.AddRange(from affliction in this.afflictions.Keys
			where predicate(affliction)
			select affliction);
			foreach (Affliction affliction2 in CharacterHealth.afflictionsToRemove)
			{
				this.afflictions.Remove(affliction2);
			}
			this.CalculateVitality();
		}

		// Token: 0x06000778 RID: 1912 RVA: 0x00044F4C File Offset: 0x0004314C
		public void RemoveAllAfflictions()
		{
			CharacterHealth.afflictionsToRemove.Clear();
			CharacterHealth.afflictionsToRemove.AddRange(from a in this.afflictions.Keys
			where !this.irremovableAfflictions.Contains(a)
			select a);
			foreach (Affliction affliction in CharacterHealth.afflictionsToRemove)
			{
				affliction.Strength = 0f;
				this.afflictions.Remove(affliction);
			}
			foreach (Affliction affliction2 in this.irremovableAfflictions)
			{
				affliction2.Strength = 0f;
			}
			this.CalculateVitality();
		}

		// Token: 0x06000779 RID: 1913 RVA: 0x0004502C File Offset: 0x0004322C
		public void RemoveNegativeAfflictions()
		{
			CharacterHealth.afflictionsToRemove.Clear();
			CharacterHealth.afflictionsToRemove.AddRange(from a in this.afflictions.Keys
			where !this.irremovableAfflictions.Contains(a) && !a.Prefab.IsBuff && a.Prefab.AfflictionType != "geneticmaterialbuff" && a.Prefab.AfflictionType != "geneticmaterialdebuff"
			select a);
			foreach (Affliction affliction in CharacterHealth.afflictionsToRemove)
			{
				this.afflictions.Remove(affliction);
			}
			foreach (Affliction affliction2 in this.irremovableAfflictions)
			{
				affliction2.Strength = 0f;
			}
			this.CalculateVitality();
		}

		// Token: 0x0600077A RID: 1914 RVA: 0x00045100 File Offset: 0x00043300
		private void AddLimbAffliction(Limb limb, Affliction newAffliction, bool allowStacking = true, bool recalculateVitality = true)
		{
			if (!newAffliction.Prefab.LimbSpecific || limb == null)
			{
				return;
			}
			if (limb.HealthIndex < 0 || limb.HealthIndex >= this.limbHealths.Count)
			{
				DebugConsole.ThrowError(string.Concat(new string[]
				{
					"Limb health index out of bounds. Character\"",
					this.Character.Name,
					"\" only has health configured for",
					this.limbHealths.Count.ToString(),
					" limbs but the limb ",
					limb.type.ToString(),
					" is targeting index ",
					limb.HealthIndex.ToString()
				}), null, null, false, false);
				return;
			}
			this.AddLimbAffliction(this.limbHealths[limb.HealthIndex], limb, newAffliction, allowStacking, recalculateVitality);
		}

		// Token: 0x0600077B RID: 1915 RVA: 0x000451DC File Offset: 0x000433DC
		private void AddLimbAffliction(CharacterHealth.LimbHealth limbHealth, Limb limb, Affliction newAffliction, bool allowStacking = true, bool recalculateVitality = true)
		{
			LimbType limbType = (limb != null) ? limb.type : LimbType.None;
			if (this.Character.Params.IsMachine && !newAffliction.Prefab.AffectMachines)
			{
				return;
			}
			if (!this.DoesBleed && newAffliction is AfflictionBleeding)
			{
				return;
			}
			if (!this.Character.NeedsOxygen && newAffliction.Prefab == AfflictionPrefab.OxygenLow)
			{
				return;
			}
			if (this.Character.Params.Health.StunImmunity && newAffliction.Prefab.AfflictionType == AfflictionPrefab.StunType && (this.Character.EmpVulnerability <= 0f || this.GetAfflictionStrengthByType(AfflictionPrefab.EMPType, false) <= 0f))
			{
				return;
			}
			if (this.Character.Params.Health.PoisonImmunity && (newAffliction.Prefab.AfflictionType == AfflictionPrefab.PoisonType || newAffliction.Prefab.AfflictionType == AfflictionPrefab.ParalysisType))
			{
				return;
			}
			if (this.Character.EmpVulnerability <= 0f && newAffliction.Prefab.AfflictionType == AfflictionPrefab.EMPType)
			{
				return;
			}
			if (newAffliction.Prefab.TargetSpecies.Any<Identifier>() && newAffliction.Prefab.TargetSpecies.None(delegate(Identifier s)
			{
				Identifier speciesName = this.Character.SpeciesName;
				return s == speciesName;
			}))
			{
				return;
			}
			if (this.Character.Params.Health.ImmunityIdentifiers.Contains(newAffliction.Identifier))
			{
				return;
			}
			bool? should = null;
			LuaCsSetup.Instance.EventService.PublishEvent<IEventCharacterApplyAffliction>(delegate(IEventCharacterApplyAffliction x)
			{
				bool? flag = x.OnCharacterApplyAffliction(this, limbHealth, newAffliction, allowStacking);
				should = ((flag != null) ? flag : should);
			});
			if (should != null && should.Value)
			{
				return;
			}
			Affliction existingAffliction = null;
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> keyValuePair in this.afflictions)
			{
				Affliction affliction2;
				CharacterHealth.LimbHealth limbHealth2;
				keyValuePair.Deconstruct(out affliction2, out limbHealth2);
				Affliction affliction = affliction2;
				CharacterHealth.LimbHealth value = limbHealth2;
				if (value == limbHealth && affliction.Prefab == newAffliction.Prefab)
				{
					existingAffliction = affliction;
					break;
				}
			}
			float modifiedStrength = newAffliction.Strength * (100f / this.MaxVitality) * (1f - this.GetResistance(newAffliction.Prefab, limbType));
			if (newAffliction.Prefab.AfflictionType == AfflictionPrefab.StunType && (double)modifiedStrength < 0.016666666666666666 && this.Stun <= 0f)
			{
				return;
			}
			if (existingAffliction != null)
			{
				float newStrength = modifiedStrength;
				if (allowStacking)
				{
					newStrength += existingAffliction.Strength;
				}
				newStrength = Math.Min(existingAffliction.Prefab.MaxStrength, newStrength);
				existingAffliction.Strength = newStrength;
				if (existingAffliction == this.stunAffliction)
				{
					this.Character.SetStun(newStrength, true, true);
				}
				existingAffliction.Duration = existingAffliction.Prefab.Duration;
				if (newAffliction.Source != null)
				{
					existingAffliction.Source = newAffliction.Source;
				}
				if (recalculateVitality)
				{
					this.RecalculateVitality();
				}
				return;
			}
			Affliction copyAffliction = newAffliction.Prefab.Instantiate(Math.Min(newAffliction.Prefab.MaxStrength, modifiedStrength), newAffliction.Source);
			this.afflictions.Add(copyAffliction, limbHealth);
			AchievementManager.OnAfflictionReceived(copyAffliction, this.Character);
			MedicalClinic.OnAfflictionCountChanged(this.Character);
			this.Character.HealthUpdateInterval = 0f;
			if (recalculateVitality)
			{
				this.RecalculateVitality();
			}
			if (CharacterHealth.OpenHealthWindow != this && limbHealth != null)
			{
				this.selectedLimbIndex = -1;
			}
		}

		// Token: 0x0600077C RID: 1916 RVA: 0x000455F0 File Offset: 0x000437F0
		private void AddAffliction(Affliction newAffliction, bool allowStacking = true)
		{
			this.AddLimbAffliction(null, null, newAffliction, allowStacking, true);
		}

		// Token: 0x0600077D RID: 1917 RVA: 0x00045600 File Offset: 0x00043800
		private void UpdateSkinTint()
		{
			this.FaceTint = CharacterHealth.DefaultFaceTint;
			this.BodyTint = Color.TransparentBlack;
			if (!this.Character.Params.Health.ApplyAfflictionColors)
			{
				return;
			}
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> keyValuePair in this.afflictions)
			{
				Affliction affliction2;
				CharacterHealth.LimbHealth limbHealth;
				keyValuePair.Deconstruct(out affliction2, out limbHealth);
				Affliction affliction = affliction2;
				Color faceTint = affliction.GetFaceTint();
				if (faceTint.A > this.FaceTint.A)
				{
					this.FaceTint = faceTint;
				}
				Color bodyTint = affliction.GetBodyTint();
				if (bodyTint.A > this.BodyTint.A)
				{
					this.BodyTint = bodyTint;
				}
			}
		}

		// Token: 0x0600077E RID: 1918 RVA: 0x000456D8 File Offset: 0x000438D8
		private void UpdateLimbAfflictionOverlays()
		{
			foreach (Limb limb in this.Character.AnimController.Limbs)
			{
				limb.BurnOverlayStrength = 0f;
				limb.DamageOverlayStrength = 0f;
			}
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> keyValuePair in this.afflictions)
			{
				Affliction affliction2;
				CharacterHealth.LimbHealth limbHealth2;
				keyValuePair.Deconstruct(out affliction2, out limbHealth2);
				Affliction affliction = affliction2;
				CharacterHealth.LimbHealth limbHealth = limbHealth2;
				if (affliction.Prefab.BurnOverlayAlpha > 0f || affliction.Prefab.DamageOverlayAlpha > 0f)
				{
					float burnStrength = affliction.Strength / Math.Min(affliction.Prefab.MaxStrength, 100f) * affliction.Prefab.BurnOverlayAlpha;
					float damageOverlayStrength = affliction.Strength / Math.Min(affliction.Prefab.MaxStrength, 100f) * affliction.Prefab.DamageOverlayAlpha;
					foreach (Limb limb2 in this.Character.AnimController.Limbs)
					{
						if (limb2.HealthIndex >= 0 && limb2.HealthIndex < this.limbHealths.Count)
						{
							if (limbHealth == this.limbHealths[limb2.HealthIndex] || !affliction.Prefab.LimbSpecific)
							{
								limb2.BurnOverlayStrength += burnStrength;
								limb2.DamageOverlayStrength += damageOverlayStrength;
							}
							else
							{
								limb2.BurnOverlayStrength += burnStrength / 2f;
							}
						}
					}
				}
			}
		}

		// Token: 0x0600077F RID: 1919 RVA: 0x000458B4 File Offset: 0x00043AB4
		public void Update(float deltaTime)
		{
			this.WasInFullHealth = (this.vitality >= this.MaxVitality);
			this.UpdateOxygen(deltaTime);
			this.StunTimer = ((this.Stun > 0f) ? (this.StunTimer + deltaTime) : 0f);
			if (!this.Character.GodMode)
			{
				CharacterHealth.afflictionsToRemove.Clear();
				CharacterHealth.afflictionsToUpdate.Clear();
				foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp in this.afflictions)
				{
					Affliction affliction = kvp.Key;
					if (affliction.Strength <= 0f)
					{
						AchievementManager.OnAfflictionRemoved(affliction, this.Character);
						if (!this.irremovableAfflictions.Contains(affliction))
						{
							CharacterHealth.afflictionsToRemove.Add(affliction);
						}
					}
					else
					{
						if (affliction.Prefab.Duration > 0f)
						{
							affliction.Duration -= deltaTime;
							if (affliction.Duration <= 0f)
							{
								affliction.Strength = 0f;
								CharacterHealth.afflictionsToRemove.Add(affliction);
								continue;
							}
						}
						CharacterHealth.afflictionsToUpdate.Add(kvp);
					}
				}
				foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp2 in CharacterHealth.afflictionsToUpdate)
				{
					Affliction affliction2 = kvp2.Key;
					Limb targetLimb = null;
					if (kvp2.Value != null)
					{
						int healthIndex = this.limbHealths.IndexOf(kvp2.Value);
						targetLimb = (this.Character.AnimController.Limbs.LastOrDefault((Limb l) => !l.IsSevered && !l.Hidden && l.HealthIndex == healthIndex) ?? this.Character.AnimController.MainLimb);
					}
					affliction2.Update(this, targetLimb, deltaTime);
					affliction2.DamagePerSecondTimer += deltaTime;
					AfflictionBleeding bleeding = affliction2 as AfflictionBleeding;
					if (bleeding != null)
					{
						this.UpdateBleedingProjSpecific(bleeding, targetLimb, deltaTime);
					}
					this.Character.StackSpeedMultiplier(affliction2.GetSpeedMultiplier());
				}
				foreach (Affliction affliction3 in CharacterHealth.afflictionsToRemove)
				{
					this.afflictions.Remove(affliction3);
				}
				if (CharacterHealth.afflictionsToRemove.Count != 0)
				{
					MedicalClinic.OnAfflictionCountChanged(this.Character);
				}
			}
			this.Character.StackSpeedMultiplier(1f + this.Character.GetStatValue(StatTypes.MovementSpeed, true));
			if (this.Character.InWater)
			{
				this.Character.StackSpeedMultiplier(1f + this.Character.GetStatValue(StatTypes.SwimmingSpeed, true));
			}
			else
			{
				this.Character.StackSpeedMultiplier(1f + this.Character.GetStatValue(StatTypes.WalkingSpeed, true));
			}
			this.UpdateDamageReductions(deltaTime);
			if (!this.Character.GodMode)
			{
				this.updateVisualsTimer -= deltaTime;
				if (this.Character.IsVisible && this.updateVisualsTimer <= 0f)
				{
					this.UpdateLimbAfflictionOverlays();
					this.UpdateSkinTint();
					this.updateVisualsTimer = 0.5f;
				}
				this.RecalculateVitality();
			}
		}

		// Token: 0x06000780 RID: 1920 RVA: 0x00045C10 File Offset: 0x00043E10
		public void ForceUpdateVisuals()
		{
			this.UpdateLimbAfflictionOverlays();
			this.UpdateSkinTint();
		}

		// Token: 0x06000781 RID: 1921 RVA: 0x00045C20 File Offset: 0x00043E20
		private void UpdateDamageReductions(float deltaTime)
		{
			float healthRegen = this.Character.Params.Health.ConstantHealthRegeneration;
			if (healthRegen > 0f)
			{
				this.ReduceAfflictionOnAllLimbs("damage".ToIdentifier(), healthRegen * deltaTime, null, null);
			}
			float burnReduction = this.Character.Params.Health.BurnReduction;
			if (burnReduction > 0f)
			{
				this.ReduceAfflictionOnAllLimbs("burn".ToIdentifier(), burnReduction * deltaTime, null, null);
			}
			float bleedingReduction = this.Character.Params.Health.BleedingReduction;
			if (bleedingReduction > 0f)
			{
				this.ReduceAfflictionOnAllLimbs("bleeding".ToIdentifier(), bleedingReduction * deltaTime, null, null);
			}
		}

		// Token: 0x17000216 RID: 534
		// (get) Token: 0x06000782 RID: 1922 RVA: 0x00045CDE File Offset: 0x00043EDE
		public float OxygenLowResistance
		{
			get
			{
				if (this.Character.NeedsOxygen)
				{
					return this.GetResistance(this.oxygenLowAffliction.Prefab, LimbType.None);
				}
				return 1f;
			}
		}

		// Token: 0x06000783 RID: 1923 RVA: 0x00045D08 File Offset: 0x00043F08
		private void UpdateOxygen(float deltaTime)
		{
			if (!this.Character.NeedsOxygen)
			{
				this.oxygenLowAffliction.Strength = 0f;
				return;
			}
			float oxygenlowResistance = this.GetResistance(this.oxygenLowAffliction.Prefab, LimbType.None);
			float prevOxygen = this.OxygenAmount;
			if (this.IsUnconscious)
			{
				float decreaseSpeed = Math.Max(0.1f, 1f - oxygenlowResistance);
				this.OxygenAmount = MathHelper.Clamp(this.OxygenAmount - decreaseSpeed * deltaTime, -100f, 100f);
			}
			else
			{
				float decreaseSpeed2 = -5f;
				float increaseSpeed = 10f;
				decreaseSpeed2 *= 1f - oxygenlowResistance;
				increaseSpeed *= 1f + oxygenlowResistance;
				float holdBreathMultiplier = this.Character.GetStatValue(StatTypes.HoldBreathMultiplier, true);
				if (holdBreathMultiplier <= -1f)
				{
					this.OxygenAmount = -100f;
				}
				else
				{
					decreaseSpeed2 /= 1f + this.Character.GetStatValue(StatTypes.HoldBreathMultiplier, true);
					this.OxygenAmount = MathHelper.Clamp(this.OxygenAmount + deltaTime * ((this.Character.OxygenAvailable < 30f) ? decreaseSpeed2 : increaseSpeed), -100f, 100f);
				}
			}
			this.UpdateOxygenProjSpecific(prevOxygen, deltaTime);
		}

		// Token: 0x06000784 RID: 1924 RVA: 0x00045E28 File Offset: 0x00044028
		private void UpdateOxygenProjSpecific(float prevOxygen, float deltaTime)
		{
			if (prevOxygen > 0f && this.OxygenAmount <= 0f && Character.Controlled == this.Character)
			{
				string soundName;
				if (this.Character.Info != null)
				{
					CharacterInfo info = this.Character.Info;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(7, 1);
					defaultInterpolatedStringHandler.AppendLiteral("drown[");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Character.Info.Prefab.MenuCategoryVar);
					defaultInterpolatedStringHandler.AppendLiteral("]");
					soundName = info.ReplaceVars(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				else
				{
					CharacterInfoPrefab charInfoPrefab = CharacterPrefab.HumanPrefab.CharacterInfoPrefab;
					CharacterInfoPrefab characterInfoPrefab = charInfoPrefab;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(7, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("drown[");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(charInfoPrefab.MenuCategoryVar);
					defaultInterpolatedStringHandler2.AppendLiteral("]");
					soundName = characterInfoPrefab.ReplaceVars(defaultInterpolatedStringHandler2.ToStringAndClear(), charInfoPrefab.Heads.First<CharacterInfo.HeadPreset>());
				}
				SoundPlayer.PlaySound(soundName, 1f);
			}
			if (this.Character == Character.Controlled && !this.IsUnconscious && !this.Character.IsDead && this.OxygenAmount < 50f)
			{
				this.timeUntilNextHeartbeatSound -= deltaTime;
				if (this.timeUntilNextHeartbeatSound < 0f)
				{
					if (this.nextHeartbeatSoundIsSystole)
					{
						SoundPlayer.PlaySound("heartbeatsystole", 1f - this.OxygenAmount / 50f);
						this.timeUntilNextHeartbeatSound = MathHelper.Lerp(0.18f, 0.3f, Math.Clamp(this.OxygenAmount / 30f, 0f, 1f));
					}
					else
					{
						SoundPlayer.PlaySound("heartbeatdiastole", 1f - this.OxygenAmount / 50f);
						this.timeUntilNextHeartbeatSound = MathHelper.Lerp(0.3f, 0.5f, Math.Clamp(this.OxygenAmount / 30f, 0f, 1f));
					}
					this.nextHeartbeatSoundIsSystole = !this.nextHeartbeatSoundIsSystole;
				}
			}
		}

		// Token: 0x06000785 RID: 1925 RVA: 0x00046030 File Offset: 0x00044230
		private void UpdateBleedingProjSpecific(AfflictionBleeding affliction, Limb targetLimb, float deltaTime)
		{
			if (this.Character.InvisibleTimer > 0f)
			{
				return;
			}
			this.bloodParticleTimer -= deltaTime * (affliction.Strength / 10f);
			if (this.bloodParticleTimer <= 0f)
			{
				Limb limb = targetLimb ?? this.Character.AnimController.MainLimb;
				bool inWater = this.Character.AnimController.InWater;
				ParticlePrefab.DrawTargetType drawTarget = inWater ? ParticlePrefab.DrawTargetType.Water : ParticlePrefab.DrawTargetType.Air;
				ParticleEmitter emitter = this.Character.BloodEmitters.FirstOrDefault(delegate(ParticleEmitter e)
				{
					ParticlePrefab particlePrefab = e.Prefab.ParticlePrefab;
					if (particlePrefab == null || particlePrefab.DrawTarget != drawTarget)
					{
						ParticlePrefab particlePrefab2 = e.Prefab.ParticlePrefab;
						return particlePrefab2 != null && particlePrefab2.DrawTarget == ParticlePrefab.DrawTargetType.Both;
					}
					return true;
				});
				float particleMinScale = (emitter != null) ? emitter.Prefab.Properties.ScaleMin : 0.5f;
				float particleMaxScale = (emitter != null) ? emitter.Prefab.Properties.ScaleMax : 1f;
				float severity = Math.Min(affliction.Strength / affliction.Prefab.MaxStrength * this.Character.Params.BleedParticleMultiplier, 1f);
				float bloodParticleSize = MathHelper.Lerp(particleMinScale, particleMaxScale, severity);
				Vector2 velocity = Rand.Vector(affliction.Strength * 0.1f, Rand.RandSync.Unsynced);
				if (!inWater)
				{
					bloodParticleSize *= 2f;
					velocity = limb.LinearVelocity * 100f;
				}
				Particle blood = GameMain.ParticleManager.CreateParticle(inWater ? this.Character.Params.BleedParticleWater : this.Character.Params.BleedParticleAir, limb.WorldPosition, velocity, 0f, this.Character.AnimController.CurrentHull, 0f, null);
				if (blood != null)
				{
					blood.Size *= bloodParticleSize;
					if (!inWater && !string.IsNullOrEmpty(this.Character.BloodDecalName) && Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < 0.05f)
					{
						Particle particle = blood;
						particle.OnCollision = (Particle.OnChangeHullHandler)Delegate.Combine(particle.OnCollision, new Particle.OnChangeHullHandler(delegate(Vector2 pos, Hull hull)
						{
							Decal decal = (hull != null) ? hull.AddDecal(this.Character.BloodDecalName, pos, Rand.Range(1f, 2f, Rand.RandSync.Unsynced), true, null) : null;
							if (decal != null)
							{
								decal.FadeTimer = decal.LifeTime - decal.FadeOutTime * 2f;
							}
						}));
					}
				}
				this.bloodParticleTimer = MathHelper.Lerp(2f, 0.5f, severity);
			}
		}

		// Token: 0x06000786 RID: 1926 RVA: 0x0004624B File Offset: 0x0004444B
		public void SetVitality(float newVitality)
		{
			this.UnmodifiedMaxVitality = newVitality;
			this.CalculateVitality();
		}

		// Token: 0x06000787 RID: 1927 RVA: 0x0004625C File Offset: 0x0004445C
		private void CalculateVitality()
		{
			this.vitality = this.MaxVitality;
			this.IsParalyzed = false;
			if (this.Unkillable || this.Character.GodMode)
			{
				return;
			}
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> keyValuePair in this.afflictions)
			{
				Affliction affliction2;
				CharacterHealth.LimbHealth limbHealth2;
				keyValuePair.Deconstruct(out affliction2, out limbHealth2);
				Affliction affliction = affliction2;
				CharacterHealth.LimbHealth limbHealth = limbHealth2;
				float vitalityDecrease = affliction.GetVitalityDecrease(this);
				if (limbHealth != null)
				{
					vitalityDecrease *= CharacterHealth.GetVitalityMultiplier(affliction, limbHealth);
				}
				this.vitality -= vitalityDecrease;
				affliction.CalculateDamagePerSecond(vitalityDecrease);
				if (affliction.Strength >= affliction.Prefab.MaxStrength && affliction.Prefab.AfflictionType == AfflictionPrefab.ParalysisType)
				{
					this.IsParalyzed = true;
				}
			}
			if (this.IsUnconscious)
			{
				HintManager.OnCharacterUnconscious(this.Character);
			}
		}

		// Token: 0x06000788 RID: 1928 RVA: 0x00046358 File Offset: 0x00044558
		public void RecalculateVitality()
		{
			this.CalculateVitality();
			this.KillIfOutOfVitality();
		}

		// Token: 0x06000789 RID: 1929 RVA: 0x00046368 File Offset: 0x00044568
		private static float GetVitalityMultiplier(Affliction affliction, CharacterHealth.LimbHealth limbHealth)
		{
			float multiplier = 1f;
			float vitalityMultiplier;
			if (limbHealth.VitalityMultipliers.TryGetValue(affliction.Prefab.Identifier, out vitalityMultiplier))
			{
				multiplier *= vitalityMultiplier;
			}
			float vitalityTypeMultiplier;
			if (limbHealth.VitalityTypeMultipliers.TryGetValue(affliction.Prefab.AfflictionType, out vitalityTypeMultiplier))
			{
				multiplier *= vitalityTypeMultiplier;
			}
			return multiplier;
		}

		// Token: 0x0600078A RID: 1930 RVA: 0x000463B8 File Offset: 0x000445B8
		private float GetVitalityDecreaseWithVitalityMultipliers(Affliction affliction)
		{
			float vitalityDecrease = affliction.GetVitalityDecrease(this);
			CharacterHealth.LimbHealth limbHealth;
			if (this.afflictions.TryGetValue(affliction, out limbHealth) && limbHealth != null)
			{
				vitalityDecrease *= CharacterHealth.GetVitalityMultiplier(affliction, limbHealth);
			}
			return vitalityDecrease;
		}

		// Token: 0x0600078B RID: 1931 RVA: 0x000463EC File Offset: 0x000445EC
		private void Kill()
		{
			if (this.Unkillable || this.Character.GodMode)
			{
				return;
			}
			ValueTuple<CauseOfDeathType, Affliction> causeOfDeath = this.GetCauseOfDeath();
			CauseOfDeathType type = causeOfDeath.Item1;
			Affliction affliction = causeOfDeath.Item2;
			this.UpdateLimbAfflictionOverlays();
			this.UpdateSkinTint();
			this.Character.Kill(type, affliction, false, true);
			this.WasInFullHealth = false;
			this.DisplayVitalityDelay = 0f;
			this.DisplayedVitality = this.Vitality;
		}

		// Token: 0x0600078C RID: 1932 RVA: 0x0004645C File Offset: 0x0004465C
		public void ApplyAfflictionStatusEffects(ActionType type)
		{
			if (this.isApplyingAfflictionStatusEffects)
			{
				using (List<Affliction>.Enumerator enumerator = this.afflictions.Keys.ToList<Affliction>().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Affliction affliction = enumerator.Current;
						affliction.ApplyStatusEffects(type, 1f, this, this.GetAfflictionLimb(affliction));
					}
					return;
				}
			}
			this.isApplyingAfflictionStatusEffects = true;
			this.afflictionsCopy.Clear();
			this.afflictionsCopy.AddRange(this.afflictions.Keys);
			this.isApplyingAfflictionStatusEffects = true;
			foreach (Affliction affliction2 in this.afflictionsCopy)
			{
				affliction2.ApplyStatusEffects(type, 1f, this, this.GetAfflictionLimb(affliction2));
			}
			this.isApplyingAfflictionStatusEffects = false;
		}

		// Token: 0x0600078D RID: 1933 RVA: 0x00046558 File Offset: 0x00044758
		[return: TupleElementNames(new string[]
		{
			"type",
			"affliction"
		})]
		public ValueTuple<CauseOfDeathType, Affliction> GetCauseOfDeath()
		{
			IEnumerable<Affliction> currentAfflictions = this.GetAllAfflictions(true, null);
			Affliction strongestAffliction = null;
			float largestStrength = 0f;
			foreach (Affliction affliction in currentAfflictions)
			{
				if (strongestAffliction == null || affliction.GetVitalityDecrease(this) > largestStrength)
				{
					strongestAffliction = affliction;
					largestStrength = affliction.GetVitalityDecrease(this);
				}
			}
			CauseOfDeathType causeOfDeath = (strongestAffliction == null) ? CauseOfDeathType.Unknown : CauseOfDeathType.Affliction;
			if (strongestAffliction == this.oxygenLowAffliction)
			{
				causeOfDeath = (this.Character.AnimController.InWater ? CauseOfDeathType.Drowning : CauseOfDeathType.Suffocation);
			}
			return new ValueTuple<CauseOfDeathType, Affliction>(causeOfDeath, strongestAffliction);
		}

		// Token: 0x0600078E RID: 1934 RVA: 0x000465FC File Offset: 0x000447FC
		private IEnumerable<Affliction> GetAllAfflictions(bool mergeSameAfflictions, Func<Affliction, bool> predicate = null)
		{
			this.allAfflictions.Clear();
			if (!mergeSameAfflictions)
			{
				List<Affliction> list = this.allAfflictions;
				IEnumerable<Affliction> collection;
				if (predicate != null)
				{
					collection = this.afflictions.Keys.Where(predicate);
				}
				else
				{
					IEnumerable<Affliction> keys = this.afflictions.Keys;
					collection = keys;
				}
				list.AddRange(collection);
			}
			else
			{
				using (Dictionary<Affliction, CharacterHealth.LimbHealth>.KeyCollection.Enumerator enumerator = this.afflictions.Keys.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Affliction affliction = enumerator.Current;
						if (predicate == null || predicate(affliction))
						{
							Affliction existingAffliction = this.allAfflictions.Find((Affliction a) => a.Prefab == affliction.Prefab);
							if (existingAffliction == null)
							{
								Affliction newAffliction = affliction.Prefab.Instantiate(affliction.Strength, null);
								if (affliction.Source != null)
								{
									newAffliction.Source = affliction.Source;
								}
								newAffliction.DamagePerSecond = affliction.DamagePerSecond;
								newAffliction.DamagePerSecondTimer = affliction.DamagePerSecondTimer;
								this.allAfflictions.Add(newAffliction);
							}
							else
							{
								existingAffliction.DamagePerSecond += affliction.DamagePerSecond;
								existingAffliction.Strength += affliction.Strength;
							}
						}
					}
				}
			}
			return this.allAfflictions;
		}

		// Token: 0x0600078F RID: 1935 RVA: 0x00046774 File Offset: 0x00044974
		public void GetSuitableTreatments(Dictionary<Identifier, float> treatmentSuitability, Character user, Limb limb = null, bool ignoreHiddenAfflictions = false, bool checkTreatmentThreshold = true, bool checkTreatmentSuggestionThreshold = true, float predictFutureDuration = 0f)
		{
			treatmentSuitability.Clear();
			float minSuitability = -10f;
			float maxSuitability = 10f;
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp in this.afflictions)
			{
				Affliction affliction = kvp.Key;
				CharacterHealth.LimbHealth limbHealth = kvp.Value;
				if (limb != null && affliction.Prefab.LimbSpecific && this.GetMatchingLimbHealth(affliction) != this.GetMatchingLimbHealth(limb))
				{
					if (limbHealth == null)
					{
						continue;
					}
					int healthIndex = this.limbHealths.IndexOf(limbHealth);
					if (limb.HealthIndex != healthIndex)
					{
						continue;
					}
				}
				float strength = affliction.Strength;
				if (predictFutureDuration > 0f)
				{
					strength = this.GetPredictedStrength(affliction, predictFutureDuration, limb);
				}
				float totalAfflictionStrength = strength + this.GetTotalAdjustedAfflictionStrength(affliction, 0.3f, false);
				if (!this.afflictions.Any((KeyValuePair<Affliction, CharacterHealth.LimbHealth> otherAffliction) => affliction.Prefab.IgnoreTreatmentIfAfflictedBy.Contains(otherAffliction.Key.Identifier)))
				{
					if (ignoreHiddenAfflictions)
					{
						if (user == this.Character)
						{
							if (strength < affliction.Prefab.ShowIconThreshold)
							{
								continue;
							}
						}
						else if (strength < affliction.Prefab.ShowIconToOthersThreshold)
						{
							continue;
						}
					}
					foreach (KeyValuePair<Identifier, float> treatment in affliction.Prefab.TreatmentSuitabilities)
					{
						float suitability = treatment.Value * strength;
						if (suitability <= 0f || ((!checkTreatmentThreshold || totalAfflictionStrength >= affliction.Prefab.TreatmentThreshold) && (!checkTreatmentSuggestionThreshold || totalAfflictionStrength >= affliction.Prefab.TreatmentSuggestionThreshold)))
						{
							if (treatment.Value > strength)
							{
								float overtreatmentFactor = MathHelper.Clamp(treatment.Value / strength, 1f, 10f);
								suitability /= overtreatmentFactor;
							}
							if (!treatmentSuitability.ContainsKey(treatment.Key))
							{
								treatmentSuitability[treatment.Key] = suitability;
							}
							else
							{
								Identifier key = treatment.Key;
								treatmentSuitability[key] += suitability;
							}
							minSuitability = Math.Min(treatmentSuitability[treatment.Key], minSuitability);
							maxSuitability = Math.Max(treatmentSuitability[treatment.Key], maxSuitability);
						}
					}
				}
			}
		}

		// Token: 0x06000790 RID: 1936 RVA: 0x00046A2C File Offset: 0x00044C2C
		public float GetTotalAdjustedAfflictionStrength(Affliction affliction, float otherAfflictionMultiplier = 0.3f, bool includeSameAffliction = true)
		{
			float totalAfflictionStrength = includeSameAffliction ? affliction.Strength : 0f;
			if (affliction.Prefab.LimbSpecific)
			{
				foreach (Affliction otherAffliction in this.afflictions.Keys)
				{
					if (affliction.Prefab == otherAffliction.Prefab && affliction != otherAffliction)
					{
						totalAfflictionStrength += otherAffliction.Strength * otherAfflictionMultiplier;
					}
				}
			}
			return totalAfflictionStrength;
		}

		// Token: 0x06000791 RID: 1937 RVA: 0x00046ABC File Offset: 0x00044CBC
		public IEnumerable<Identifier> GetActiveAfflictionTags()
		{
			this.afflictionTags.Clear();
			foreach (Affliction affliction in this.afflictions.Keys)
			{
				AfflictionPrefab.Effect currentEffect = affliction.GetActiveEffect();
				if (currentEffect != null && !currentEffect.Tag.IsEmpty)
				{
					this.afflictionTags.Add(currentEffect.Tag);
				}
			}
			return this.afflictionTags;
		}

		// Token: 0x06000792 RID: 1938 RVA: 0x00046B4C File Offset: 0x00044D4C
		public float GetPredictedStrength(Affliction affliction, float predictFutureDuration, Limb limb = null)
		{
			float strength = affliction.Strength;
			Func<ISerializableEntity, bool> <>9__0;
			foreach (DurationListElement statusEffect in StatusEffect.DurationList)
			{
				IEnumerable<ISerializableEntity> targets = statusEffect.Targets;
				Func<ISerializableEntity, bool> predicate;
				if ((predicate = <>9__0) == null)
				{
					predicate = (<>9__0 = ((ISerializableEntity t) => t == this.Character || (limb != null && this.Character.AnimController.Limbs.Contains(t))));
				}
				if (targets.Any(predicate))
				{
					float statusEffectDuration = Math.Min(statusEffect.Timer, predictFutureDuration);
					foreach (Affliction statusEffectAffliction in statusEffect.Parent.Afflictions)
					{
						if (statusEffectAffliction.Prefab == affliction.Prefab)
						{
							strength += statusEffectAffliction.Strength * statusEffectDuration;
						}
					}
					foreach (ValueTuple<Identifier, float> statusEffectAffliction2 in statusEffect.Parent.ReduceAffliction)
					{
						Identifier identifier = affliction.Identifier;
						if (statusEffectAffliction2.Item1 == identifier || statusEffectAffliction2.Item1 == affliction.Prefab.AfflictionType)
						{
							strength -= statusEffectAffliction2.Item2 * statusEffectDuration;
						}
					}
				}
			}
			return MathHelper.Clamp(strength, 0f, affliction.Prefab.MaxStrength);
		}

		// Token: 0x06000793 RID: 1939 RVA: 0x00046D10 File Offset: 0x00044F10
		public void ServerWrite(IWriteMessage msg)
		{
			this.activeAfflictions.Clear();
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp in this.afflictions)
			{
				Affliction affliction = kvp.Key;
				if (kvp.Value == null && affliction.Strength > 0f && affliction.Strength >= affliction.Prefab.ActivationThreshold)
				{
					this.activeAfflictions.Add(affliction);
				}
			}
			msg.WriteByte((byte)this.activeAfflictions.Count);
			foreach (Affliction affliction2 in this.activeAfflictions)
			{
				msg.WriteUInt32(affliction2.Prefab.UintIdentifier);
				msg.WriteRangedSingle(MathHelper.Clamp(affliction2.Strength, 0f, affliction2.Prefab.MaxStrength), 0f, affliction2.Prefab.MaxStrength, 8);
				msg.WriteByte((byte)affliction2.Prefab.PeriodicEffects.Count);
				foreach (AfflictionPrefab.PeriodicEffect periodicEffect in affliction2.Prefab.PeriodicEffects)
				{
					msg.WriteRangedSingle(affliction2.PeriodicEffectTimers[periodicEffect], 0f, periodicEffect.MaxInterval, 8);
				}
			}
			this.limbAfflictions.Clear();
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp2 in this.afflictions)
			{
				Affliction limbAffliction = kvp2.Key;
				CharacterHealth.LimbHealth limbHealth = kvp2.Value;
				if (limbHealth != null && limbAffliction.Strength > 0f && limbAffliction.Strength >= limbAffliction.Prefab.ActivationThreshold)
				{
					this.limbAfflictions.Add(new ValueTuple<CharacterHealth.LimbHealth, Affliction>(limbHealth, limbAffliction));
				}
			}
			msg.WriteByte((byte)this.limbAfflictions.Count);
			foreach (ValueTuple<CharacterHealth.LimbHealth, Affliction> valueTuple in this.limbAfflictions)
			{
				CharacterHealth.LimbHealth limbHealth2 = valueTuple.Item1;
				Affliction affliction3 = valueTuple.Item2;
				msg.WriteRangedInteger(this.limbHealths.IndexOf(limbHealth2), 0, this.limbHealths.Count - 1);
				msg.WriteUInt32(affliction3.Prefab.UintIdentifier);
				msg.WriteRangedSingle(MathHelper.Clamp(affliction3.Strength, 0f, affliction3.Prefab.MaxStrength), 0f, affliction3.Prefab.MaxStrength, 8);
				msg.WriteByte((byte)affliction3.Prefab.PeriodicEffects.Count);
				foreach (AfflictionPrefab.PeriodicEffect periodicEffect2 in affliction3.Prefab.PeriodicEffects)
				{
					msg.WriteRangedSingle(affliction3.PeriodicEffectTimers[periodicEffect2], periodicEffect2.MinInterval, periodicEffect2.MaxInterval, 8);
				}
			}
		}

		// Token: 0x06000794 RID: 1940 RVA: 0x000470E0 File Offset: 0x000452E0
		public void Remove()
		{
			this.RemoveProjSpecific();
			CharacterHealth.afflictionsToRemove.Clear();
			CharacterHealth.afflictionsToUpdate.Clear();
		}

		// Token: 0x06000795 RID: 1941 RVA: 0x000470FC File Offset: 0x000452FC
		private void RemoveProjSpecific()
		{
			foreach (CharacterHealth.LimbHealth limbHealth in this.limbHealths)
			{
				if (limbHealth.IndicatorSprite != null)
				{
					limbHealth.IndicatorSprite.Remove();
					limbHealth.IndicatorSprite = null;
				}
			}
			SpriteSheet spriteSheet = this.medUIExtra;
			if (spriteSheet != null)
			{
				spriteSheet.Remove();
			}
			this.medUIExtra = null;
			Character character = this.Character;
			character.OnAttacked = (Character.OnAttackedHandler)Delegate.Remove(character.OnAttacked, new Character.OnAttackedHandler(this.OnAttacked));
			SpriteSheet spriteSheet2 = this.limbIndicatorOverlay;
			if (spriteSheet2 != null)
			{
				spriteSheet2.Remove();
			}
			this.limbIndicatorOverlay = null;
			if (this.healthWindow != null)
			{
				this.healthWindow.RectTransform.Parent = null;
				this.healthWindow = null;
			}
			if (this.healthBarHolder != null)
			{
				this.healthBarHolder.RectTransform.Parent = null;
				this.healthBarHolder = null;
			}
			if (this.SuicideButton != null)
			{
				this.SuicideButton.RectTransform.Parent = null;
				this.SuicideButton = null;
			}
			if (this.afflictionTooltip != null)
			{
				this.afflictionTooltip.RectTransform.Parent = null;
				this.afflictionTooltip = null;
			}
		}

		// Token: 0x06000796 RID: 1942 RVA: 0x0004723C File Offset: 0x0004543C
		public static IEnumerable<Affliction> SortAfflictionsBySeverity(IEnumerable<Affliction> afflictions, bool excludeBuffs = true)
		{
			return from a in afflictions
			where !excludeBuffs || !a.Prefab.IsBuff
			orderby a.DamagePerSecond descending, a.Strength / a.Prefab.MaxStrength descending
			select a;
		}

		// Token: 0x06000797 RID: 1943 RVA: 0x000472B0 File Offset: 0x000454B0
		public void Save(XElement healthElement)
		{
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp in this.afflictions)
			{
				Affliction affliction = kvp.Key;
				CharacterHealth.LimbHealth limbHealth = kvp.Value;
				if (affliction.Strength > 0f && limbHealth == null && !kvp.Key.Prefab.ResetBetweenRounds)
				{
					healthElement.Add(new XElement("Affliction", new object[]
					{
						new XAttribute("identifier", affliction.Identifier),
						new XAttribute("strength", affliction.Strength.ToString("G", CultureInfo.InvariantCulture))
					}));
				}
			}
			int i;
			Func<KeyValuePair<Affliction, CharacterHealth.LimbHealth>, bool> <>9__0;
			int j;
			for (i = 0; i < this.limbHealths.Count; i = j + 1)
			{
				XElement limbHealthElement = new XElement("LimbHealth", new XAttribute("i", i));
				healthElement.Add(limbHealthElement);
				IEnumerable<KeyValuePair<Affliction, CharacterHealth.LimbHealth>> source = this.afflictions;
				Func<KeyValuePair<Affliction, CharacterHealth.LimbHealth>, bool> predicate;
				if ((predicate = <>9__0) == null)
				{
					predicate = (<>9__0 = ((KeyValuePair<Affliction, CharacterHealth.LimbHealth> a) => a.Value == this.limbHealths[i]));
				}
				foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp2 in source.Where(predicate))
				{
					Affliction affliction2 = kvp2.Key;
					CharacterHealth.LimbHealth limbHealth2 = kvp2.Value;
					if (affliction2.Strength > 0f)
					{
						limbHealthElement.Add(new XElement("Affliction", new object[]
						{
							new XAttribute("identifier", affliction2.Identifier),
							new XAttribute("strength", affliction2.Strength.ToString("G", CultureInfo.InvariantCulture))
						}));
					}
				}
				j = i;
			}
		}

		// Token: 0x06000798 RID: 1944 RVA: 0x000474FC File Offset: 0x000456FC
		public void Load(XElement element, Func<AfflictionPrefab, bool> afflictionPredicate = null)
		{
			CharacterHealth.<>c__DisplayClass269_0 CS$<>8__locals1;
			CS$<>8__locals1.afflictionPredicate = afflictionPredicate;
			CS$<>8__locals1.<>4__this = this;
			foreach (XElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "affliction"))
				{
					if (a == "limbhealth")
					{
						int limbHealthIndex = subElement.GetAttributeInt("i", -1);
						if (limbHealthIndex < 0 || limbHealthIndex >= this.limbHealths.Count)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(65, 1);
							defaultInterpolatedStringHandler.AppendLiteral("Error while loading character health: limb index \"");
							defaultInterpolatedStringHandler.AppendFormatted<int>(limbHealthIndex);
							defaultInterpolatedStringHandler.AppendLiteral("\" out of range.");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
						}
						else
						{
							foreach (XElement afflictionElement in subElement.Elements())
							{
								this.<Load>g__LoadAffliction|269_0(afflictionElement, this.limbHealths[limbHealthIndex], ref CS$<>8__locals1);
							}
						}
					}
				}
				else
				{
					this.<Load>g__LoadAffliction|269_0(subElement, null, ref CS$<>8__locals1);
				}
			}
		}

		// Token: 0x0600079B RID: 1947 RVA: 0x000476BB File Offset: 0x000458BB
		[CompilerGenerated]
		internal static float <UpdateHUD>g__GetStr|84_2(Affliction affliction)
		{
			return affliction.Strength / affliction.Prefab.MaxStrength * (affliction.Prefab.IsBuff ? 1f : 10f);
		}

		// Token: 0x0600079D RID: 1949 RVA: 0x000476F4 File Offset: 0x000458F4
		[CompilerGenerated]
		private void <UpdateStatusHUD>g__RemoveNonExistentIcons|90_0(GUIComponent container)
		{
			for (int i = container.CountChildren - 1; i >= 0; i--)
			{
				GUIComponent child = container.GetChild(i);
				object userData = child.UserData;
				AfflictionPrefab afflictionPrefab = userData as AfflictionPrefab;
				if (afflictionPrefab != null && !this.statusIcons.Any((Affliction s) => s.Prefab == afflictionPrefab))
				{
					container.RemoveChild(child);
					this.statusIconVisibleTime.Remove(afflictionPrefab);
				}
			}
		}

		// Token: 0x060007A7 RID: 1959 RVA: 0x00047994 File Offset: 0x00045B94
		[CompilerGenerated]
		private void <Load>g__LoadAffliction|269_0(XElement afflictionElement, CharacterHealth.LimbHealth limbHealth = null, ref CharacterHealth.<>c__DisplayClass269_0 A_3)
		{
			string id = afflictionElement.GetAttributeString("identifier", "");
			AfflictionPrefab afflictionPrefab = AfflictionPrefab.Prefabs.Find((AfflictionPrefab a) => a.Identifier == id);
			if (afflictionPrefab == null)
			{
				DebugConsole.ThrowError("Error while loading character health: affliction \"" + id + "\" not found.", null, null, false, false);
				return;
			}
			if (A_3.afflictionPredicate != null && !A_3.afflictionPredicate(afflictionPrefab))
			{
				return;
			}
			float strength = afflictionElement.GetAttributeFloat("strength", 0f);
			Affliction irremovableAffliction = this.irremovableAfflictions.FirstOrDefault((Affliction a) => a.Prefab == afflictionPrefab);
			if (irremovableAffliction != null)
			{
				irremovableAffliction.Strength = strength;
				return;
			}
			this.afflictions.Add(afflictionPrefab.Instantiate(strength, null), limbHealth);
		}

		// Token: 0x040003A7 RID: 935
		private static bool toggledThisFrame;

		// Token: 0x040003A8 RID: 936
		private Point screenResolution;

		// Token: 0x040003A9 RID: 937
		private float uiScale;

		// Token: 0x040003AA RID: 938
		private float inventoryScale;

		// Token: 0x040003AB RID: 939
		private Alignment alignment = Alignment.Right;

		// Token: 0x040003AD RID: 941
		private GUIProgressBar healthBar;

		// Token: 0x040003AE RID: 942
		private GUIProgressBar healthBarShadow;

		// Token: 0x040003AF RID: 943
		private float healthShadowSize;

		// Token: 0x040003B0 RID: 944
		private float healthShadowDelay;

		// Token: 0x040003B1 RID: 945
		private float healthBarPulsateTimer;

		// Token: 0x040003B2 RID: 946
		private float healthBarPulsatePhase;

		// Token: 0x040003B3 RID: 947
		private float bloodParticleTimer;

		// Token: 0x040003B4 RID: 948
		private GUIFrame healthWindow;

		// Token: 0x040003B5 RID: 949
		private GUITextBlock deadIndicator;

		// Token: 0x040003B6 RID: 950
		private GUIButton cprButton;

		// Token: 0x040003B7 RID: 951
		private GUIListBox afflictionTooltip;

		// Token: 0x040003B8 RID: 952
		private static readonly Color oxygenLowGrainColor = new Color(0.1f, 0.1f, 0.1f, 1f);

		// Token: 0x040003B9 RID: 953
		private SpriteSheet limbIndicatorOverlay;

		// Token: 0x040003BA RID: 954
		private float limbIndicatorOverlayAnimState;

		// Token: 0x040003BB RID: 955
		private SpriteSheet medUIExtra;

		// Token: 0x040003BC RID: 956
		private float medUIExtraAnimState;

		// Token: 0x040003BD RID: 957
		private int highlightedLimbIndex = -1;

		// Token: 0x040003BE RID: 958
		private int selectedLimbIndex = -1;

		// Token: 0x040003BF RID: 959
		private CharacterHealth.LimbHealth currentDisplayedLimb;

		// Token: 0x040003C0 RID: 960
		private GUIComponent afflictionIconContainer;

		// Token: 0x040003C1 RID: 961
		private float afflictionIconRefreshTimer;

		// Token: 0x040003C2 RID: 962
		private const float AfflictionIconRefreshInterval = 1f;

		// Token: 0x040003C3 RID: 963
		private GUIButton showHiddenAfflictionsButton;

		// Token: 0x040003C4 RID: 964
		private GUIComponent hiddenAfflictionIconContainer;

		// Token: 0x040003C5 RID: 965
		private GUIProgressBar healthWindowHealthBar;

		// Token: 0x040003C6 RID: 966
		private GUIProgressBar healthWindowHealthBarShadow;

		// Token: 0x040003C7 RID: 967
		private GUITextBlock characterName;

		// Token: 0x040003C8 RID: 968
		private GUIListBox afflictionIconList;

		// Token: 0x040003C9 RID: 969
		private GUILayoutGroup treatmentLayout;

		// Token: 0x040003CA RID: 970
		private GUIListBox recommendedTreatmentContainer;

		// Token: 0x040003CB RID: 971
		private LocalizedString prevHighlightedAfflictionDescription;

		// Token: 0x040003CC RID: 972
		private float updateVisualsTimer = Rand.Range(0f, 0.5f, Rand.RandSync.Unsynced);

		// Token: 0x040003CD RID: 973
		private const float UpdateVisualsInterval = 0.5f;

		// Token: 0x040003CE RID: 974
		private float distortTimer;

		// Token: 0x040003CF RID: 975
		private float damageIntensity;

		// Token: 0x040003D0 RID: 976
		private readonly float damageIntensityDropdownRate = 0.1f;

		// Token: 0x040003D2 RID: 978
		private float updateDisplayedAfflictionsTimer;

		// Token: 0x040003D3 RID: 979
		private const float UpdateDisplayedAfflictionsInterval = 0.5f;

		// Token: 0x040003D4 RID: 980
		private readonly List<Affliction> currentDisplayedAfflictions = new List<Affliction>();

		// Token: 0x040003D5 RID: 981
		public float DisplayedVitality;

		// Token: 0x040003D6 RID: 982
		public float DisplayVitalityDelay;

		// Token: 0x040003D7 RID: 983
		private static CharacterHealth openHealthWindow;

		// Token: 0x040003D9 RID: 985
		private GUIFrame healthBarHolder;

		// Token: 0x040003DA RID: 986
		private float timeUntilNextHeartbeatSound;

		// Token: 0x040003DB RID: 987
		private bool nextHeartbeatSoundIsSystole = true;

		// Token: 0x040003DC RID: 988
		private const string diastoleSoundTag = "heartbeatdiastole";

		// Token: 0x040003DD RID: 989
		private const string systoleSoundTag = "heartbeatsystole";

		// Token: 0x040003DE RID: 990
		private readonly List<Affliction> statusIcons = new List<Affliction>();

		// Token: 0x040003DF RID: 991
		private readonly Dictionary<AfflictionPrefab, float> statusIconVisibleTime = new Dictionary<AfflictionPrefab, float>();

		// Token: 0x040003E0 RID: 992
		private const float HideStatusIconDelay = 5f;

		// Token: 0x040003E1 RID: 993
		[TupleElementNames(new string[]
		{
			"affliction",
			"strength"
		})]
		private readonly List<ValueTuple<Affliction, float>> displayedAfflictions = new List<ValueTuple<Affliction, float>>();

		// Token: 0x040003E2 RID: 994
		private static readonly List<Affliction> afflictionsDisplayedOnLimb = new List<Affliction>();

		// Token: 0x040003E3 RID: 995
		[TupleElementNames(new string[]
		{
			"limb",
			"afflictionPrefab",
			"strength"
		})]
		private readonly List<ValueTuple<CharacterHealth.LimbHealth, AfflictionPrefab, float>> newAfflictions = new List<ValueTuple<CharacterHealth.LimbHealth, AfflictionPrefab, float>>();

		// Token: 0x040003E4 RID: 996
		[TupleElementNames(new string[]
		{
			"effect",
			"timer"
		})]
		private readonly List<ValueTuple<AfflictionPrefab.PeriodicEffect, float>> newPeriodicEffects = new List<ValueTuple<AfflictionPrefab.PeriodicEffect, float>>();

		// Token: 0x040003E5 RID: 997
		public const float InsufficientOxygenThreshold = 30f;

		// Token: 0x040003E6 RID: 998
		public const float LowOxygenThreshold = 50f;

		// Token: 0x040003E7 RID: 999
		protected float minVitality;

		// Token: 0x040003E8 RID: 1000
		public bool Unkillable;

		// Token: 0x040003E9 RID: 1001
		private readonly List<CharacterHealth.LimbHealth> limbHealths = new List<CharacterHealth.LimbHealth>();

		// Token: 0x040003EA RID: 1002
		private readonly Dictionary<Affliction, CharacterHealth.LimbHealth> afflictions = new Dictionary<Affliction, CharacterHealth.LimbHealth>();

		// Token: 0x040003EB RID: 1003
		private readonly HashSet<Affliction> irremovableAfflictions = new HashSet<Affliction>();

		// Token: 0x040003EC RID: 1004
		private Affliction bloodlossAffliction;

		// Token: 0x040003ED RID: 1005
		private Affliction oxygenLowAffliction;

		// Token: 0x040003EE RID: 1006
		private Affliction pressureAffliction;

		// Token: 0x040003EF RID: 1007
		private Affliction stunAffliction;

		// Token: 0x040003F1 RID: 1009
		private float vitality;

		// Token: 0x040003F2 RID: 1010
		public static readonly Color DefaultFaceTint = Color.TransparentBlack;

		// Token: 0x040003F8 RID: 1016
		public bool ShowDamageOverlay = true;

		// Token: 0x040003F9 RID: 1017
		public readonly Character Character;

		// Token: 0x040003FA RID: 1018
		private readonly List<Affliction> matchingAfflictions = new List<Affliction>();

		// Token: 0x040003FB RID: 1019
		private static readonly List<Affliction> afflictionsToRemove = new List<Affliction>();

		// Token: 0x040003FC RID: 1020
		private static readonly List<KeyValuePair<Affliction, CharacterHealth.LimbHealth>> afflictionsToUpdate = new List<KeyValuePair<Affliction, CharacterHealth.LimbHealth>>();

		// Token: 0x040003FD RID: 1021
		private readonly List<Affliction> afflictionsCopy = new List<Affliction>();

		// Token: 0x040003FE RID: 1022
		private bool isApplyingAfflictionStatusEffects;

		// Token: 0x040003FF RID: 1023
		private readonly List<Affliction> allAfflictions = new List<Affliction>();

		// Token: 0x04000400 RID: 1024
		private readonly HashSet<Identifier> afflictionTags = new HashSet<Identifier>();

		// Token: 0x04000401 RID: 1025
		private readonly List<Affliction> activeAfflictions = new List<Affliction>();

		// Token: 0x04000402 RID: 1026
		[TupleElementNames(new string[]
		{
			"limbHealth",
			"affliction"
		})]
		private readonly List<ValueTuple<CharacterHealth.LimbHealth, Affliction>> limbAfflictions = new List<ValueTuple<CharacterHealth.LimbHealth, Affliction>>();

		// Token: 0x020006E2 RID: 1762
		public class DamageOverlayPrefab : Prefab
		{
			// Token: 0x0600671A RID: 26394 RVA: 0x003494DA File Offset: 0x003476DA
			public DamageOverlayPrefab(ContentXElement element, AfflictionsFile file) : base(file, file.Path.Value.ToIdentifier())
			{
				this.DamageOverlay = new Sprite(element, "", "", false, 1f);
			}

			// Token: 0x0600671B RID: 26395 RVA: 0x0034950F File Offset: 0x0034770F
			public override void Dispose()
			{
				this.DamageOverlay.Remove();
			}

			// Token: 0x04003832 RID: 14386
			public static readonly PrefabSelector<CharacterHealth.DamageOverlayPrefab> Prefabs = new PrefabSelector<CharacterHealth.DamageOverlayPrefab>();

			// Token: 0x04003833 RID: 14387
			public readonly Sprite DamageOverlay;
		}

		// Token: 0x020006E3 RID: 1763
		public class LimbHealth
		{
			// Token: 0x0600671D RID: 26397 RVA: 0x00349528 File Offset: 0x00347728
			public LimbHealth()
			{
			}

			// Token: 0x0600671E RID: 26398 RVA: 0x00349548 File Offset: 0x00347748
			public LimbHealth(ContentXElement element, CharacterHealth characterHealth)
			{
				string limbName = element.GetAttributeString("name", null) ?? "generic";
				if (limbName != "generic")
				{
					this.Name = TextManager.Get("HealthLimbName." + limbName);
				}
				foreach (ContentXElement subElement in element.Elements())
				{
					string a = subElement.Name.ToString().ToLowerInvariant();
					if (!(a == "sprite"))
					{
						if (!(a == "highlightsprite"))
						{
							if (a == "vitalitymultiplier")
							{
								if (subElement.GetAttribute("name") != null)
								{
									DebugConsole.ThrowError("Error in character health config (" + characterHealth.Character.Name + ") - define vitality multipliers using affliction identifiers or types instead of names.", null, element.ContentPackage, false, false);
								}
								else
								{
									Identifier[] vitalityMultipliers = subElement.GetAttributeIdentifierArray("identifier", null, true) ?? subElement.GetAttributeIdentifierArray("identifiers", null, true);
									if (vitalityMultipliers != null)
									{
										float multiplier = subElement.GetAttributeFloat("multiplier", 1f);
										Identifier[] array = vitalityMultipliers;
										for (int i = 0; i < array.Length; i++)
										{
											Identifier vitalityMultiplier = array[i];
											this.VitalityMultipliers.Add(vitalityMultiplier, multiplier);
											if (AfflictionPrefab.Prefabs.None((AfflictionPrefab p) => p.Identifier == vitalityMultiplier))
											{
												DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(169, 2);
												defaultInterpolatedStringHandler.AppendLiteral("Potentially incorrectly defined vitality multiplier in \"");
												defaultInterpolatedStringHandler.AppendFormatted(characterHealth.Character.Name);
												defaultInterpolatedStringHandler.AppendLiteral("\". Could not find any afflictions with the identifier \"");
												defaultInterpolatedStringHandler.AppendFormatted<Identifier>(vitalityMultiplier);
												defaultInterpolatedStringHandler.AppendLiteral("\". Did you mean to define the afflictions by type instead?");
												DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), element.ContentPackage);
											}
										}
									}
									Identifier[] vitalityTypeMultipliers = subElement.GetAttributeIdentifierArray("type", null, true) ?? subElement.GetAttributeIdentifierArray("types", null, true);
									if (vitalityTypeMultipliers != null)
									{
										float multiplier2 = subElement.GetAttributeFloat("multiplier", 1f);
										Identifier[] array2 = vitalityTypeMultipliers;
										for (int j = 0; j < array2.Length; j++)
										{
											Identifier vitalityTypeMultiplier = array2[j];
											this.VitalityTypeMultipliers.Add(vitalityTypeMultiplier, multiplier2);
											if (AfflictionPrefab.Prefabs.None((AfflictionPrefab p) => p.AfflictionType == vitalityTypeMultiplier))
											{
												DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(167, 2);
												defaultInterpolatedStringHandler2.AppendLiteral("Potentially incorrectly defined vitality multiplier in \"");
												defaultInterpolatedStringHandler2.AppendFormatted(characterHealth.Character.Name);
												defaultInterpolatedStringHandler2.AppendLiteral("\". Could not find any afflictions of the type \"");
												defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(vitalityTypeMultiplier);
												defaultInterpolatedStringHandler2.AppendLiteral("\". Did you mean to define the afflictions by identifier instead?");
												DebugConsole.AddWarning(defaultInterpolatedStringHandler2.ToStringAndClear(), element.ContentPackage);
											}
										}
									}
									if (vitalityMultipliers == null && this.VitalityTypeMultipliers == null)
									{
										DebugConsole.ThrowError("Error in character health config " + characterHealth.Character.Name + ": affliction identifier(s) or type(s) not defined in the \"VitalityMultiplier\" elements!", null, element.ContentPackage, false, false);
									}
								}
							}
						}
						else
						{
							this.HighlightSprite = new Sprite(subElement, "", "", false, 1f);
						}
					}
					else
					{
						this.IndicatorSprite = new Sprite(subElement, "", "", false, 1f);
						ContentXElement contentXElement = subElement;
						string key = "highlightarea";
						Rectangle rectangle = new Rectangle(0, 0, (int)this.IndicatorSprite.size.X, (int)this.IndicatorSprite.size.Y);
						this.HighlightArea = contentXElement.GetAttributeRect(key, rectangle);
					}
				}
			}

			// Token: 0x04003834 RID: 14388
			public Sprite IndicatorSprite;

			// Token: 0x04003835 RID: 14389
			public Sprite HighlightSprite;

			// Token: 0x04003836 RID: 14390
			public Rectangle HighlightArea;

			// Token: 0x04003837 RID: 14391
			public readonly LocalizedString Name;

			// Token: 0x04003838 RID: 14392
			public readonly Dictionary<Identifier, float> VitalityMultipliers = new Dictionary<Identifier, float>();

			// Token: 0x04003839 RID: 14393
			public readonly Dictionary<Identifier, float> VitalityTypeMultipliers = new Dictionary<Identifier, float>();
		}
	}
}
