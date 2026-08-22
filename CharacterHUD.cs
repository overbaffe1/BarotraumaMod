using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000028 RID: 40
	internal class CharacterHUD
	{
		// Token: 0x170001CC RID: 460
		// (get) Token: 0x0600062C RID: 1580 RVA: 0x00034F60 File Offset: 0x00033160
		public static GUIFrame HUDFrame
		{
			get
			{
				if (CharacterHUD.hudFrame == null)
				{
					CharacterHUD.hudFrame = new GUIFrame(new RectTransform(GUI.Canvas.RelativeSize, GUI.Canvas, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
					{
						CanBeFocused = false
					};
					CharacterHUD.bossHealthContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.15f, 0.5f), CharacterHUD.hudFrame.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal)
					{
						RelativeOffset = new Vector2(0.005f, 0f)
					}, false, Anchor.TopLeft)
					{
						AbsoluteSpacing = GUI.IntScale(10f)
					};
				}
				return CharacterHUD.hudFrame;
			}
		}

		// Token: 0x170001CD RID: 461
		// (get) Token: 0x0600062D RID: 1581 RVA: 0x00035034 File Offset: 0x00033234
		// (set) Token: 0x0600062E RID: 1582 RVA: 0x0003503B File Offset: 0x0003323B
		public static bool RecreateHudTexts { get; set; } = true;

		// Token: 0x170001CE RID: 462
		// (get) Token: 0x0600062F RID: 1583 RVA: 0x00035043 File Offset: 0x00033243
		public static bool IsCampaignInterfaceOpen
		{
			get
			{
				GameSession gameSession = GameMain.GameSession;
				return ((gameSession != null) ? gameSession.Campaign : null) != null && (GameMain.GameSession.Campaign.ShowCampaignUI || GameMain.GameSession.Campaign.ForceMapUI);
			}
		}

		// Token: 0x06000630 RID: 1584 RVA: 0x0003507C File Offset: 0x0003327C
		public static bool ShouldDrawInventory(Character character)
		{
			Item selectedItem = character.SelectedItem;
			Controller controller = (selectedItem != null) ? selectedItem.GetComponent<Controller>() : null;
			return ((character != null) ? character.Inventory : null) != null && !character.Removed && !character.IsKnockedDown && (((controller != null) ? controller.User : null) != character || !controller.HideHUD || Screen.Selected.IsEditor) && !CharacterHUD.IsCampaignInterfaceOpen && !ConversationAction.FadeScreenToBlack;
		}

		// Token: 0x06000631 RID: 1585 RVA: 0x000350F0 File Offset: 0x000332F0
		public static LocalizedString GetCachedHudText(string textTag, InputType keyBind)
		{
			if (CharacterHUD.cachedHudTextLanguage != GameSettings.CurrentConfig.Language)
			{
				CharacterHUD.cachedHudTexts.Clear();
			}
			LocalizedString left = textTag + keyBind.ToString();
			GameSettings.Config.KeyMapping keyMap = GameSettings.CurrentConfig.KeyMap;
			Identifier key = (left + keyMap.KeyBindText(keyBind)).ToIdentifier<LocalizedString>();
			LocalizedString text;
			if (CharacterHUD.cachedHudTexts.TryGetValue(key, out text))
			{
				return text;
			}
			string varName = "[key]";
			keyMap = GameSettings.CurrentConfig.KeyMap;
			text = TextManager.GetWithVariable(textTag, varName, keyMap.KeyBindText(keyBind), FormatCapitals.No).Value;
			CharacterHUD.cachedHudTexts.Add(key, text);
			CharacterHUD.cachedHudTextLanguage = GameSettings.CurrentConfig.Language;
			return text;
		}

		// Token: 0x06000632 RID: 1586 RVA: 0x000351AC File Offset: 0x000333AC
		public static void AddToGUIUpdateList(Character character)
		{
			if (GUI.DisableHUD)
			{
				return;
			}
			if (!character.IsIncapacitated && character.Stun <= 0f && !CharacterHUD.IsCampaignInterfaceOpen)
			{
				if (character.Inventory != null)
				{
					for (int i = 0; i < character.Inventory.Capacity; i++)
					{
						Item item = character.Inventory.GetItemAt(i);
						if (item != null && character.Inventory.SlotTypes[i] != InvSlotType.Any)
						{
							foreach (ItemComponent ic in item.Components)
							{
								if (ic.DrawHudWhenEquipped)
								{
									ic.AddToGUIUpdateList(0);
								}
							}
						}
					}
				}
				if (character.Params.CanInteract && character.SelectedCharacter != null)
				{
					character.SelectedCharacter.CharacterHealth.AddToGUIUpdateList();
				}
			}
			CharacterHUD.HUDFrame.AddToGUIUpdateList(false, 0);
		}

		// Token: 0x06000633 RID: 1587 RVA: 0x000352A4 File Offset: 0x000334A4
		public static void Update(float deltaTime, Character character, Camera cam)
		{
			CharacterHUD.UpdateBossProgressBars(deltaTime);
			if (GUI.DisableHUD)
			{
				if (character.Inventory != null && !CharacterHUD.LockInventory(character))
				{
					character.Inventory.UpdateSlotInput();
				}
				return;
			}
			if (character.ShowInteractionLabels && character.ViewTarget == null)
			{
				InteractionLabelManager.Update(character, cam);
			}
			if (!character.IsIncapacitated && character.Stun <= 0f && !CharacterHUD.IsCampaignInterfaceOpen)
			{
				if (character.Info != null && !character.ShouldLockHud() && character.SelectedCharacter == null && Screen.Selected != GameMain.SubEditorScreen)
				{
					bool mouseOnPortrait = CharacterHUD.MouseOnCharacterPortrait() && GUI.MouseOn == null;
					bool healthWindowOpen = CharacterHealth.OpenHealthWindow != null || CharacterHUD.timeHealthWindowClosed < 0.2f;
					if (mouseOnPortrait && !healthWindowOpen && PlayerInput.PrimaryMouseButtonClicked() && Inventory.DraggingItems.None(null))
					{
						CharacterHealth.OpenHealthWindow = character.CharacterHealth;
					}
				}
				if (character.Inventory != null)
				{
					if (!CharacterHUD.LockInventory(character))
					{
						character.Inventory.Update(deltaTime, cam, false);
					}
					else
					{
						character.Inventory.ClearSubInventories();
					}
				}
				if (character.Params.CanInteract && character.SelectedCharacter != null && character.SelectedCharacter.Inventory != null)
				{
					if (character.SelectedCharacter.IsInventoryAccessibleTo(character, CharacterInventory.AccessLevel.AllowBotsAndPets))
					{
						character.SelectedCharacter.Inventory.Update(deltaTime, cam, false);
					}
					character.SelectedCharacter.CharacterHealth.UpdateHUD(deltaTime);
				}
				Inventory.UpdateDragging();
			}
			if (CharacterHUD.focusedItem != null)
			{
				if (character.FocusedItem != null)
				{
					CharacterHUD.focusedItemOverlayTimer = Math.Min(CharacterHUD.focusedItemOverlayTimer + deltaTime, 2f);
				}
				else
				{
					CharacterHUD.focusedItemOverlayTimer = Math.Max(CharacterHUD.focusedItemOverlayTimer - deltaTime, 0f);
					if (CharacterHUD.focusedItemOverlayTimer <= 0f)
					{
						CharacterHUD.focusedItem = null;
						CharacterHUD.RecreateHudTexts = true;
					}
				}
			}
			if (CharacterHUD.brokenItemsCheckTimer > 0f)
			{
				CharacterHUD.brokenItemsCheckTimer -= deltaTime;
			}
			else
			{
				CharacterHUD.brokenItems.Clear();
				CharacterHUD.brokenItemsCheckTimer = 1f;
				foreach (Item item in Item.ItemList)
				{
					if (item.Submarine != null && item.Submarine.TeamID == character.TeamID && !item.Submarine.Info.IsWreck)
					{
						if (item.Repairables.Any((Repairable r) => r.IsBelowRepairIconThreshold) && (Submarine.VisibleEntities == null || Submarine.VisibleEntities.Contains(item)))
						{
							Vector2 diff = item.WorldPosition - character.WorldPosition;
							if (Submarine.CheckVisibility(character.SimPosition, character.SimPosition + ConvertUnits.ToSimUnits(diff), false, false, true, true, true, null) == null)
							{
								CharacterHUD.brokenItems.Add(item);
							}
						}
					}
				}
			}
			if (CharacterHealth.OpenHealthWindow != null)
			{
				CharacterHUD.timeHealthWindowClosed = 0f;
				return;
			}
			CharacterHUD.timeHealthWindowClosed += deltaTime;
		}

		// Token: 0x06000634 RID: 1588 RVA: 0x000355B0 File Offset: 0x000337B0
		public static void Draw(SpriteBatch spriteBatch, Character character, Camera cam)
		{
			CharacterHUD.<>c__DisplayClass28_0 CS$<>8__locals1;
			CS$<>8__locals1.character = character;
			CS$<>8__locals1.spriteBatch = spriteBatch;
			CS$<>8__locals1.cam = cam;
			if (GUI.DisableHUD)
			{
				return;
			}
			CS$<>8__locals1.character.CharacterHealth.Alignment = Alignment.Right;
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.CrewManager : null) != null)
			{
				CharacterHUD.orderIndicatorCount.Clear();
				foreach (CrewManager.ActiveOrder activeOrder in GameMain.GameSession.CrewManager.ActiveOrders)
				{
					if (CharacterHUD.<Draw>g__DrawIcon|28_2(activeOrder.Order))
					{
						if (activeOrder.FadeOutTime != null)
						{
							CharacterHUD.DrawOrderIndicator(CS$<>8__locals1.spriteBatch, CS$<>8__locals1.cam, CS$<>8__locals1.character, activeOrder.Order, MathHelper.Clamp(activeOrder.FadeOutTime.Value / 10f, 0.2f, 1f), true, 1f, false);
						}
						else
						{
							float iconAlpha = CharacterHUD.<Draw>g__GetDistanceBasedIconAlpha|28_0(activeOrder.Order.TargetSpatialEntity, 450f, ref CS$<>8__locals1);
							if (iconAlpha > 0f)
							{
								CharacterHUD.DrawOrderIndicator(CS$<>8__locals1.spriteBatch, CS$<>8__locals1.cam, CS$<>8__locals1.character, activeOrder.Order, iconAlpha, false, 0.5f, true);
							}
						}
					}
				}
				Order currentOrder = CS$<>8__locals1.character.GetCurrentOrderWithTopPriority();
				if (currentOrder != null && CharacterHUD.<Draw>g__DrawIcon|28_2(currentOrder))
				{
					CharacterHUD.DrawOrderIndicator(CS$<>8__locals1.spriteBatch, CS$<>8__locals1.cam, CS$<>8__locals1.character, currentOrder, 1f, true, 1f, false);
				}
			}
			if (GameMain.GameSession != null)
			{
				foreach (Mission mission in GameMain.GameSession.Missions)
				{
					if (mission.DisplayTargetHudIcons)
					{
						foreach (Entity target in mission.HudIconTargets)
						{
							if (target.Submarine == CS$<>8__locals1.character.Submarine && !target.Removed)
							{
								float alpha = CharacterHUD.<Draw>g__GetDistanceBasedIconAlpha|28_0(target, mission.Prefab.HudIconMaxDistance, ref CS$<>8__locals1);
								if (alpha > 0f)
								{
									GUI.DrawIndicator(CS$<>8__locals1.spriteBatch, target.DrawPosition, CS$<>8__locals1.cam, 100f, mission.Prefab.HudIcon, mission.Prefab.HudIconColor * alpha, true, 1f, null);
								}
							}
						}
					}
				}
			}
			foreach (Character.ObjectiveEntity objectiveEntity in CS$<>8__locals1.character.ActiveObjectiveEntities)
			{
				CharacterHUD.DrawObjectiveIndicator(CS$<>8__locals1.spriteBatch, CS$<>8__locals1.cam, CS$<>8__locals1.character, objectiveEntity, 1f);
			}
			foreach (Item brokenItem in CharacterHUD.brokenItems)
			{
				if (brokenItem.IsInteractable(CS$<>8__locals1.character))
				{
					float alpha2 = CharacterHUD.<Draw>g__GetDistanceBasedIconAlpha|28_0(brokenItem, 1000f, ref CS$<>8__locals1);
					if (alpha2 > 0f)
					{
						GUI.DrawIndicator(CS$<>8__locals1.spriteBatch, brokenItem.DrawPosition, CS$<>8__locals1.cam, 100f, GUIStyle.BrokenIcon.Value.Sprite, Color.Lerp(GUIStyle.Red, GUIStyle.Orange * 0.5f, brokenItem.Condition / brokenItem.MaxCondition) * alpha2, true, 1f, null);
					}
				}
			}
			OrderPrefab deconstructOrder;
			if (OrderPrefab.Prefabs.TryGet(Tags.DeconstructThis, out deconstructOrder))
			{
				foreach (Item deconstructItem in Item.DeconstructItems)
				{
					if (deconstructItem.ParentInventory == null && !deconstructItem.OrderedToBeIgnored && deconstructItem.Submarine == CS$<>8__locals1.character.Submarine && deconstructItem.IsInteractable(CS$<>8__locals1.character))
					{
						float alpha3 = CharacterHUD.<Draw>g__GetDistanceBasedIconAlpha|28_0(deconstructItem, 450f, ref CS$<>8__locals1) * 0.7f;
						if (alpha3 > 0f)
						{
							GUI.DrawIndicator(CS$<>8__locals1.spriteBatch, deconstructItem.DrawPosition, CS$<>8__locals1.cam, 100f, deconstructOrder.SymbolSprite, GUIStyle.Red, true, 0.5f, new float?(alpha3));
						}
					}
				}
			}
			if (!CS$<>8__locals1.character.IsIncapacitated && CS$<>8__locals1.character.Stun <= 0f && !CharacterHUD.IsCampaignInterfaceOpen)
			{
				if (CS$<>8__locals1.character.IsKeyDown(InputType.Aim))
				{
					if (!CS$<>8__locals1.character.HeldItems.None((Item it) => ((it != null) ? it.GetComponent<Sprayer>() : null) != null))
					{
						goto IL_BD6;
					}
				}
				if (!CS$<>8__locals1.character.ShowInteractionLabels)
				{
					Character focusedCharacter = CS$<>8__locals1.character.FocusedCharacter;
					if (focusedCharacter != null && focusedCharacter.CanBeSelected)
					{
						CharacterHUD.DrawCharacterHoverTexts(CS$<>8__locals1.spriteBatch, CS$<>8__locals1.cam, CS$<>8__locals1.character);
					}
					if (CS$<>8__locals1.character.FocusedItem != null)
					{
						if (CharacterHUD.focusedItem != CS$<>8__locals1.character.FocusedItem)
						{
							CharacterHUD.focusedItemOverlayTimer = Math.Min(1f, CharacterHUD.focusedItemOverlayTimer);
							CharacterHUD.RecreateHudTexts = true;
						}
						CharacterHUD.focusedItem = CS$<>8__locals1.character.FocusedItem;
					}
					if (CharacterHUD.focusedItem != null && CharacterHUD.focusedItemOverlayTimer > 1f)
					{
						Vector2 circlePos = CS$<>8__locals1.cam.WorldToScreen(CharacterHUD.focusedItem.DrawPosition);
						float circleSize = (float)Math.Max(CharacterHUD.focusedItem.Rect.Width, CharacterHUD.focusedItem.Rect.Height) * 1.5f;
						circleSize = MathHelper.Clamp(circleSize, 45f, 100f) * Math.Min((CharacterHUD.focusedItemOverlayTimer - 1f) * 5f, 1f);
						if (circleSize > 0f)
						{
							Vector2 scale = new Vector2(circleSize / (float)GUIStyle.FocusIndicator.FrameSize.X);
							GUIStyle.FocusIndicator.Draw(CS$<>8__locals1.spriteBatch, (int)((CharacterHUD.focusedItemOverlayTimer - 1f) * (float)GUIStyle.FocusIndicator.FrameCount * 3f), circlePos, Color.LightBlue * 0.3f, GUIStyle.FocusIndicator.FrameSize.ToVector2() / 2f, (float)Timing.TotalTime, scale, SpriteEffects.None, null);
						}
						if (!GUI.DisableItemHighlights && !Inventory.DraggingItemToWorld)
						{
							bool hudTextsContextual = PlayerInput.KeyDown(InputType.ContextualCommand);
							if (CharacterHUD.RecreateHudTexts || CharacterHUD.lastHudTextsContextual != hudTextsContextual)
							{
								CharacterHUD.RecreateHudTexts = true;
								CharacterHUD.lastHudTextsContextual = hudTextsContextual;
							}
							List<ColoredText> hudTexts = CharacterHUD.focusedItem.GetHUDTexts(CS$<>8__locals1.character, CharacterHUD.RecreateHudTexts);
							CharacterHUD.RecreateHudTexts = false;
							int dir = Math.Sign(CharacterHUD.focusedItem.WorldPosition.X - CS$<>8__locals1.character.WorldPosition.X);
							Vector2 textSize = GUIStyle.Font.MeasureString(hudTexts.First<ColoredText>().Text, false);
							Vector2 largeTextSize = GUIStyle.SubHeadingFont.MeasureString(hudTexts.First<ColoredText>().Text, false);
							Vector2 startPos = CS$<>8__locals1.cam.WorldToScreen(CharacterHUD.focusedItem.DrawPosition);
							startPos.Y -= (float)(hudTexts.Count + 1) * textSize.Y;
							if (CharacterHUD.focusedItem.Sprite != null)
							{
								startPos.X += (float)((int)(circleSize * 0.4f * (float)dir));
								startPos.Y -= (float)((int)(circleSize * 0.4f));
							}
							Vector2 textPos = startPos;
							if (dir == -1)
							{
								textPos.X -= largeTextSize.X;
							}
							float alpha4 = MathHelper.Clamp((CharacterHUD.focusedItemOverlayTimer - 1f) * 2f, 0f, 1f);
							GUI.DrawString(CS$<>8__locals1.spriteBatch, textPos, hudTexts.First<ColoredText>().Text, hudTexts.First<ColoredText>().Color * alpha4, new Color?(Color.Black * alpha4 * 0.7f), 2, GUIStyle.SubHeadingFont, ForceUpperCase.No);
							startPos.X += (float)dir * 10f * GUI.Scale;
							textPos.X += (float)dir * 10f * GUI.Scale;
							textPos.Y += largeTextSize.Y;
							foreach (ColoredText coloredText in hudTexts.Skip(1))
							{
								if (dir == -1)
								{
									textPos.X = (float)((int)(startPos.X - GUIStyle.SmallFont.MeasureString(coloredText.Text, false).X));
								}
								GUI.DrawString(CS$<>8__locals1.spriteBatch, textPos, coloredText.Text, coloredText.Color * alpha4, new Color?(Color.Black * alpha4 * 0.7f), 2, GUIStyle.SmallFont, ForceUpperCase.Inherit);
								textPos.Y += textSize.Y;
							}
						}
					}
				}
				if (CS$<>8__locals1.character.ShowInteractionLabels && CS$<>8__locals1.character.ViewTarget == null)
				{
					InteractionLabelManager.DrawLabels(CS$<>8__locals1.spriteBatch, CS$<>8__locals1.cam, CS$<>8__locals1.character);
				}
				foreach (HUDProgressBar progressBar in CS$<>8__locals1.character.HUDProgressBars.Values)
				{
					progressBar.Draw(CS$<>8__locals1.spriteBatch, CS$<>8__locals1.cam);
				}
				foreach (Character npc in Character.CharacterList)
				{
					if (npc.CampaignInteractionType != CampaignMode.InteractionType.None)
					{
						CharacterHUD.<Draw>g__DrawInteractionIcon|28_3(npc, ("CampaignInteractionIcon." + npc.CampaignInteractionType.ToString()).ToIdentifier(), ref CS$<>8__locals1);
					}
				}
				GameSession gameSession2 = GameMain.GameSession;
				TutorialMode tutorialMode = ((gameSession2 != null) ? gameSession2.GameMode : null) as TutorialMode;
				if (tutorialMode != null && tutorialMode.Tutorial != null)
				{
					foreach (ValueTuple<Entity, Identifier> valueTuple in tutorialMode.Tutorial.Icons)
					{
						Entity entity = valueTuple.Item1;
						Identifier iconStyle = valueTuple.Item2;
						CharacterHUD.<Draw>g__DrawInteractionIcon|28_3(entity, iconStyle, ref CS$<>8__locals1);
					}
				}
				foreach (Item item in Item.ItemList)
				{
					if (item.IconStyle != null && item.Submarine == CS$<>8__locals1.character.Submarine && Vector2.DistanceSquared(CS$<>8__locals1.character.Position, item.Position) <= 250000f)
					{
						Body body = Submarine.CheckVisibility(CS$<>8__locals1.character.SimPosition, item.SimPosition, true, false, true, true, true, null);
						if (body == null || body.UserData as Item == item)
						{
							SpriteBatch spriteBatch2 = CS$<>8__locals1.spriteBatch;
							Vector2 vector = item.DrawPosition + new Vector2(0f, (float)item.RectHeight * 0.65f);
							Camera cam2 = CS$<>8__locals1.cam;
							Range<float> range = new Range<float>(-100f, 500f);
							GUI.DrawIndicator(spriteBatch2, vector, cam2, range, item.IconStyle.GetDefaultSprite(), item.IconStyle.Color, false, 1f, null, null);
						}
					}
				}
			}
			IL_BD6:
			if (CS$<>8__locals1.character.SelectedItem != null && (CS$<>8__locals1.character.CanInteractWith(CS$<>8__locals1.character.SelectedItem, true) || Screen.Selected == GameMain.SubEditorScreen))
			{
				CS$<>8__locals1.character.SelectedItem.DrawHUD(CS$<>8__locals1.spriteBatch, CS$<>8__locals1.cam, CS$<>8__locals1.character);
			}
			if (CS$<>8__locals1.character.Inventory != null)
			{
				foreach (Item item2 in CS$<>8__locals1.character.Inventory.AllItems)
				{
					if (CS$<>8__locals1.character.HasEquippedItem(item2, null, null))
					{
						item2.DrawHUD(CS$<>8__locals1.spriteBatch, CS$<>8__locals1.cam, CS$<>8__locals1.character);
					}
				}
			}
			if (CharacterHUD.IsCampaignInterfaceOpen)
			{
				return;
			}
			if (CS$<>8__locals1.character.Inventory != null)
			{
				for (int i = 0; i < CS$<>8__locals1.character.Inventory.Capacity; i++)
				{
					Item item3 = CS$<>8__locals1.character.Inventory.GetItemAt(i);
					if (item3 != null && CS$<>8__locals1.character.Inventory.SlotTypes[i] != InvSlotType.Any)
					{
						bool duplicateFound = false;
						for (int j = 0; j < i; j++)
						{
							if (CS$<>8__locals1.character.Inventory.SlotTypes[j] != InvSlotType.Any && CS$<>8__locals1.character.Inventory.GetItemAt(j) == item3)
							{
								duplicateFound = true;
								break;
							}
						}
						if (!duplicateFound)
						{
							foreach (ItemComponent ic in item3.Components)
							{
								if (ic.DrawHudWhenEquipped)
								{
									ic.DrawHUD(CS$<>8__locals1.spriteBatch, CS$<>8__locals1.character);
								}
							}
						}
					}
				}
			}
			bool mouseOnPortrait = false;
			if (CS$<>8__locals1.character.Stun <= 0.1f && !CS$<>8__locals1.character.IsDead)
			{
				bool wiringMode = Screen.Selected == GameMain.SubEditorScreen && GameMain.SubEditorScreen.WiringMode;
				if (CharacterHealth.OpenHealthWindow == null && CS$<>8__locals1.character.SelectedCharacter == null && !wiringMode)
				{
					if (CS$<>8__locals1.character.Info != null && !CS$<>8__locals1.character.ShouldLockHud())
					{
						CS$<>8__locals1.character.Info.DrawBackground(CS$<>8__locals1.spriteBatch);
						CS$<>8__locals1.character.Info.DrawJobIcon(CS$<>8__locals1.spriteBatch, new Rectangle((int)((float)HUDLayoutSettings.BottomRightInfoArea.X + (float)HUDLayoutSettings.BottomRightInfoArea.Width * 0.05f), (int)((float)HUDLayoutSettings.BottomRightInfoArea.Y + (float)HUDLayoutSettings.BottomRightInfoArea.Height * 0.1f), HUDLayoutSettings.BottomRightInfoArea.Width / 2, (int)((float)HUDLayoutSettings.BottomRightInfoArea.Height * 0.7f)), CS$<>8__locals1.character.Info.IsDisguisedAsAnother);
						GameSession gameSession3 = GameMain.GameSession;
						float yOffset = (float)((((gameSession3 != null) ? gameSession3.Campaign : null) is MultiPlayerCampaign) ? -10 : 4) * GUI.Scale;
						CharacterInfo info = CS$<>8__locals1.character.Info;
						if (info != null)
						{
							info.DrawIcon(CS$<>8__locals1.spriteBatch, new Vector2((float)HUDLayoutSettings.PortraitArea.Center.X - 12f * GUI.Scale, (float)HUDLayoutSettings.PortraitArea.Center.Y), HUDLayoutSettings.PortraitArea.Size.ToVector2(), true);
						}
						CS$<>8__locals1.character.Info.DrawForeground(CS$<>8__locals1.spriteBatch);
					}
					mouseOnPortrait = (CharacterHUD.MouseOnCharacterPortrait() && !CS$<>8__locals1.character.ShouldLockHud());
					if (mouseOnPortrait)
					{
						GUIStyle.UIGlow.Draw(CS$<>8__locals1.spriteBatch, HUDLayoutSettings.BottomRightInfoArea, GUIStyle.Green * 0.5f, SpriteEffects.None);
					}
				}
				if (CharacterHUD.ShouldDrawInventory(CS$<>8__locals1.character))
				{
					CS$<>8__locals1.character.Inventory.Locked = (CS$<>8__locals1.character == Character.Controlled && CharacterHUD.LockInventory(CS$<>8__locals1.character));
					CS$<>8__locals1.character.Inventory.DrawOwn(CS$<>8__locals1.spriteBatch);
					CS$<>8__locals1.character.Inventory.CurrentLayout = ((CharacterHealth.OpenHealthWindow == null && CS$<>8__locals1.character.SelectedCharacter == null) ? CharacterInventory.Layout.Default : CharacterInventory.Layout.Right);
				}
			}
			if (!CS$<>8__locals1.character.IsIncapacitated && CS$<>8__locals1.character.Stun <= 0f)
			{
				if (CS$<>8__locals1.character.Params.CanInteract && CS$<>8__locals1.character.SelectedCharacter != null && CS$<>8__locals1.character.SelectedCharacter.Inventory != null)
				{
					if (CS$<>8__locals1.character.SelectedCharacter.IsInventoryAccessibleTo(CS$<>8__locals1.character, CharacterInventory.AccessLevel.AllowBotsAndPets))
					{
						CS$<>8__locals1.character.SelectedCharacter.Inventory.Locked = false;
						CS$<>8__locals1.character.SelectedCharacter.Inventory.CurrentLayout = CharacterInventory.Layout.Left;
						CS$<>8__locals1.character.SelectedCharacter.Inventory.DrawOwn(CS$<>8__locals1.spriteBatch);
					}
					if (CharacterHealth.OpenHealthWindow == CS$<>8__locals1.character.SelectedCharacter.CharacterHealth)
					{
						CS$<>8__locals1.character.SelectedCharacter.CharacterHealth.Alignment = Alignment.Left;
					}
				}
				else
				{
					CharacterInventory inventory = CS$<>8__locals1.character.Inventory;
				}
			}
			if (mouseOnPortrait)
			{
				SpriteBatch spriteBatch3 = CS$<>8__locals1.spriteBatch;
				CharacterInfo info2 = CS$<>8__locals1.character.Info;
				GUIComponent.DrawToolTip(spriteBatch3, (((info2 != null) ? info2.Job : null) == null) ? CS$<>8__locals1.character.DisplayName : (CS$<>8__locals1.character.DisplayName + " (" + CS$<>8__locals1.character.Info.Job.Name + ")"), HUDLayoutSettings.PortraitArea, Anchor.BottomCenter, Pivot.TopLeft);
			}
		}

		// Token: 0x06000635 RID: 1589 RVA: 0x000368A8 File Offset: 0x00034AA8
		public static bool MouseOnCharacterPortrait()
		{
			return Character.Controlled != null && CharacterHealth.OpenHealthWindow == null && Character.Controlled.SelectedCharacter == null && HUDLayoutSettings.BottomRightInfoArea.Contains(PlayerInput.MousePosition);
		}

		// Token: 0x06000636 RID: 1590 RVA: 0x000368E8 File Offset: 0x00034AE8
		private static void DrawCharacterHoverTexts(SpriteBatch spriteBatch, Camera cam, Character character)
		{
			CharacterInventory inventory = character.Inventory;
			IEnumerable<Item> allItems = (inventory != null) ? inventory.AllItems : null;
			if (allItems != null)
			{
				foreach (Item item in allItems)
				{
					StatusHUD statusHUD = (item != null) ? item.GetComponent<StatusHUD>() : null;
					if (statusHUD != null && statusHUD.IsActive && statusHUD.VisibleCharacters.Contains(character.FocusedCharacter))
					{
						return;
					}
				}
			}
			float dist = Vector2.Distance(character.FocusedCharacter.DrawPosition, character.DrawPosition);
			Vector2 startPos = character.DrawPosition + (character.FocusedCharacter.DrawPosition - character.DrawPosition) / dist * Math.Min(dist, 200f);
			startPos = cam.WorldToScreen(startPos);
			string focusName = (character.FocusedCharacter.Info == null) ? character.FocusedCharacter.DisplayName : character.FocusedCharacter.Info.DisplayName;
			Vector2 textPos = startPos;
			Vector2 textSize = GUIStyle.Font.MeasureString("T", false);
			Vector2 largeTextSize = GUIStyle.SubHeadingFont.MeasureString("T", false);
			textPos -= new Vector2(textSize.X / 2f, textSize.Y);
			Color nameColor = character.FocusedCharacter.GetNameColor();
			GUI.DrawString(spriteBatch, textPos, focusName, nameColor, new Color?(Color.Black * 0.7f), 2, GUIStyle.SubHeadingFont, ForceUpperCase.No);
			textPos.Y += GUIStyle.SubHeadingFont.MeasureString(focusName, false).Y;
			CharacterInfo info = character.FocusedCharacter.Info;
			if (((info != null) ? info.Title : null) != null && !character.FocusedCharacter.Info.Title.IsNullOrEmpty() && character.FocusedCharacter.TeamID != CharacterTeamType.Team1)
			{
				GUI.DrawString(spriteBatch, textPos, character.FocusedCharacter.Info.Title, nameColor, new Color?(Color.Black * 0.7f), 2, GUIStyle.SubHeadingFont, ForceUpperCase.No);
				textPos.Y += GUIStyle.SubHeadingFont.MeasureString(character.FocusedCharacter.Info.Title.Value, false).Y;
			}
			textPos.X += 10f * GUI.Scale;
			if (!character.FocusedCharacter.IsIncapacitated && character.FocusedCharacter.IsPet)
			{
				EnemyAIController enemyAI = character.FocusedCharacter.AIController as EnemyAIController;
				if (enemyAI != null && enemyAI.PetBehavior.CanPlayWith(character))
				{
					GUI.DrawString(spriteBatch, textPos, CharacterHUD.GetCachedHudText("PlayHint", InputType.Use), GUIStyle.Green, new Color?(Color.Black), 2, GUIStyle.SmallFont, ForceUpperCase.Inherit);
					textPos.Y += largeTextSize.Y;
				}
			}
			if (character.FocusedCharacter.CanBeDraggedBy(character))
			{
				string text = character.CanEat ? "EatHint" : "GrabHint";
				GUI.DrawString(spriteBatch, textPos, CharacterHUD.GetCachedHudText(text, InputType.Grab), GUIStyle.Green, new Color?(Color.Black), 2, GUIStyle.SmallFont, ForceUpperCase.Inherit);
				textPos.Y += largeTextSize.Y;
			}
			if (character.FocusedCharacter.CanBeHealedBy(character, true))
			{
				GUI.DrawString(spriteBatch, textPos, CharacterHUD.GetCachedHudText("HealHint", InputType.Health), GUIStyle.Green, new Color?(Color.Black), 2, GUIStyle.SmallFont, ForceUpperCase.Inherit);
				textPos.Y += textSize.Y;
			}
			if (character.FocusedCharacter.ShouldShowCustomInteractText)
			{
				GUI.DrawString(spriteBatch, textPos, character.FocusedCharacter.CustomInteractHUDText, GUIStyle.Green, new Color?(Color.Black), 2, GUIStyle.SmallFont, ForceUpperCase.Inherit);
				textPos.Y += textSize.Y;
			}
		}

		// Token: 0x06000637 RID: 1591 RVA: 0x00036CEC File Offset: 0x00034EEC
		public static void ShowBossHealthBar(Character character, float damage)
		{
			if (character == null || character.IsDead || character.Removed)
			{
				return;
			}
			if (CharacterHUD.bossProgressBars.Any((CharacterHUD.ProgressBar b) => b.IsDuplicate(character)))
			{
				return;
			}
			CharacterHUD.AddBossProgressBar(new CharacterHUD.HealthBar(character));
		}

		// Token: 0x06000638 RID: 1592 RVA: 0x00036D54 File Offset: 0x00034F54
		public static void ShowMissionProgressBar(Mission mission)
		{
			if (mission == null || mission.Completed || mission.Failed)
			{
				return;
			}
			if (CharacterHUD.bossProgressBars.Any((CharacterHUD.ProgressBar b) => b.IsDuplicate(mission)))
			{
				return;
			}
			CharacterHUD.AddBossProgressBar(new CharacterHUD.MissionProgressBar(mission));
		}

		// Token: 0x06000639 RID: 1593 RVA: 0x00036DBC File Offset: 0x00034FBC
		public static void ClearBossProgressBars()
		{
			for (int i = CharacterHUD.bossProgressBars.Count - 1; i >= 0; i--)
			{
				CharacterHUD.RemoveBossProgressBar(CharacterHUD.bossProgressBars[i]);
			}
			CharacterHUD.bossProgressBars.Clear();
		}

		// Token: 0x0600063A RID: 1594 RVA: 0x00036DFC File Offset: 0x00034FFC
		private static void RemoveBossProgressBar(CharacterHUD.ProgressBar progressBar)
		{
			GUIComponent parent = progressBar.SideContainer.Parent;
			if (parent != null)
			{
				parent.RemoveChild(progressBar.SideContainer);
			}
			GUIComponent parent2 = progressBar.TopContainer.Parent;
			if (parent2 != null)
			{
				parent2.RemoveChild(progressBar.TopContainer);
			}
			CharacterHUD.bossProgressBars.Remove(progressBar);
		}

		// Token: 0x0600063B RID: 1595 RVA: 0x00036E50 File Offset: 0x00035050
		private static void AddBossProgressBar(CharacterHUD.ProgressBar progressBar)
		{
			NetworkMember networkMember = GameMain.NetworkMember;
			EnemyHealthBarMode healthBarMode = (networkMember != null) ? networkMember.ServerSettings.ShowEnemyHealthBars : GameSettings.CurrentConfig.ShowEnemyHealthBars;
			if (healthBarMode == EnemyHealthBarMode.HideAll && !(progressBar is CharacterHUD.MissionProgressBar))
			{
				return;
			}
			if (CharacterHUD.bossProgressBars.Count > 5)
			{
				CharacterHUD.ProgressBar oldestHealthBar = CharacterHUD.bossProgressBars.First<CharacterHUD.ProgressBar>();
				foreach (CharacterHUD.ProgressBar bar in CharacterHUD.bossProgressBars)
				{
					if (bar.TopBar.BarSize < oldestHealthBar.TopBar.BarSize)
					{
						oldestHealthBar = bar;
					}
				}
				oldestHealthBar.FadeTimer = Math.Min(oldestHealthBar.FadeTimer, 1f);
			}
			CharacterHUD.bossProgressBars.Add(progressBar);
		}

		// Token: 0x0600063C RID: 1596 RVA: 0x00036F1C File Offset: 0x0003511C
		public static void UpdateBossProgressBars(float deltaTime)
		{
			NetworkMember networkMember = GameMain.NetworkMember;
			EnemyHealthBarMode healthBarMode = (networkMember != null) ? networkMember.ServerSettings.ShowEnemyHealthBars : GameSettings.CurrentConfig.ShowEnemyHealthBars;
			for (int i = 0; i < CharacterHUD.bossProgressBars.Count; i++)
			{
				CharacterHUD.ProgressBar bossHealthBar = CharacterHUD.bossProgressBars[i];
				bool showTopBar = i == CharacterHUD.bossProgressBars.Count - 1;
				if (showTopBar && !bossHealthBar.TopContainer.Visible)
				{
					bossHealthBar.SideContainer.SetAsLastChild();
					CharacterHUD.<UpdateBossProgressBars>g__SetColor|36_0(bossHealthBar, bossHealthBar.SideContainer, 0f);
				}
				bossHealthBar.TopContainer.Visible = showTopBar;
				bossHealthBar.SideContainer.Visible = !bossHealthBar.TopContainer.Visible;
				bossHealthBar.TopBar.BarSize = (bossHealthBar.SideBar.BarSize = bossHealthBar.State);
				float alpha = Math.Min(bossHealthBar.FadeTimer, 1f);
				if (bossHealthBar.TopContainer.Visible)
				{
					CharacterHUD.<UpdateBossProgressBars>g__SetColor|36_0(bossHealthBar, bossHealthBar.TopContainer, alpha);
				}
				if (bossHealthBar.SideContainer.Visible)
				{
					CharacterHUD.<UpdateBossProgressBars>g__SetColor|36_0(bossHealthBar, bossHealthBar.SideContainer, alpha);
				}
				if (bossHealthBar.Interrupted)
				{
					bossHealthBar.FadeTimer = Math.Min(bossHealthBar.FadeTimer, 1f);
				}
				else if (bossHealthBar.Completed)
				{
					bossHealthBar.FadeTimer = Math.Min(bossHealthBar.FadeTimer, 5f);
				}
				bossHealthBar.FadeTimer -= deltaTime;
			}
			for (int j = CharacterHUD.bossProgressBars.Count - 1; j >= 0; j--)
			{
				CharacterHUD.ProgressBar bossHealthBar2 = CharacterHUD.bossProgressBars[j];
				if (bossHealthBar2.FadeTimer <= 0f || (healthBarMode == EnemyHealthBarMode.HideAll && !(bossHealthBar2 is CharacterHUD.MissionProgressBar)))
				{
					GUIComponent parent = bossHealthBar2.SideContainer.Parent;
					if (parent != null)
					{
						parent.RemoveChild(bossHealthBar2.SideContainer);
					}
					GUIComponent parent2 = bossHealthBar2.TopContainer.Parent;
					if (parent2 != null)
					{
						parent2.RemoveChild(bossHealthBar2.TopContainer);
					}
					CharacterHUD.bossProgressBars.RemoveAt(j);
					CharacterHUD.bossHealthContainer.Recalculate();
				}
			}
		}

		// Token: 0x0600063D RID: 1597 RVA: 0x00037122 File Offset: 0x00035322
		private static bool LockInventory(Character character)
		{
			return ((character != null) ? character.Inventory : null) == null || !character.AllowInput || character.LockHands || CharacterHUD.IsCampaignInterfaceOpen || character.ShouldLockHud();
		}

		// Token: 0x0600063E RID: 1598 RVA: 0x00037154 File Offset: 0x00035354
		private static void DrawOrderIndicator(SpriteBatch spriteBatch, Camera cam, Character character, Order order, float iconAlpha = 1f, bool createOffset = true, float scaleMultiplier = 1f, bool overrideAlpha = false)
		{
			if (((order != null) ? order.SymbolSprite : null) == null)
			{
				return;
			}
			if (order.IsReport && order.OrderGiver != character && !order.HasAppropriateJob(character))
			{
				return;
			}
			Controller connectedController = order.ConnectedController;
			ISpatialEntity spatialEntity = (connectedController != null) ? connectedController.Item : null;
			ISpatialEntity target = spatialEntity ?? order.TargetSpatialEntity;
			if (target == null)
			{
				return;
			}
			if (character.Submarine != target.Submarine && Vector2.DistanceSquared(character.WorldPosition, target.WorldPosition) > 1000000f)
			{
				return;
			}
			if (!CharacterHUD.orderIndicatorCount.ContainsKey(target))
			{
				CharacterHUD.orderIndicatorCount.Add(target, 0);
			}
			Entity entity = target as Entity;
			Vector2 drawPos = (entity != null) ? entity.DrawPosition : ((target.Submarine == null) ? target.Position : (target.Position + target.Submarine.DrawPosition));
			drawPos += Vector2.UnitX * order.SymbolSprite.size.X * 1.5f * (float)CharacterHUD.orderIndicatorCount[target];
			GUI.DrawIndicator(spriteBatch, drawPos, cam, 100f, order.SymbolSprite, order.Color * iconAlpha, createOffset, scaleMultiplier, overrideAlpha ? new float?(iconAlpha) : null);
			CharacterHUD.orderIndicatorCount[target] = CharacterHUD.orderIndicatorCount[target] + 1;
		}

		// Token: 0x0600063F RID: 1599 RVA: 0x000372B8 File Offset: 0x000354B8
		private static void DrawObjectiveIndicator(SpriteBatch spriteBatch, Camera cam, Character character, Character.ObjectiveEntity objectiveEntity, float iconAlpha = 1f)
		{
			if (objectiveEntity == null)
			{
				return;
			}
			Vector2 drawPos = objectiveEntity.Entity.WorldPosition;
			GUI.DrawIndicator(spriteBatch, drawPos, cam, 100f, objectiveEntity.Sprite, objectiveEntity.Color * iconAlpha, true, 1f, null);
		}

		// Token: 0x06000640 RID: 1600 RVA: 0x00037304 File Offset: 0x00035504
		private static void RecreateHudTextsIfControllingProjSpecific(Character character)
		{
			if (character == Character.Controlled)
			{
				CharacterHUD.RecreateHudTexts = true;
			}
		}

		// Token: 0x06000641 RID: 1601 RVA: 0x00037314 File Offset: 0x00035514
		private static void RecreateHudTextsIfFocusedProjSpecific(params Item[] items)
		{
			foreach (Item item in items)
			{
				Item item2 = item;
				Character controlled = Character.Controlled;
				if (item2 == ((controlled != null) ? controlled.FocusedItem : null))
				{
					CharacterHUD.RecreateHudTexts = true;
					return;
				}
			}
		}

		// Token: 0x06000642 RID: 1602 RVA: 0x00037350 File Offset: 0x00035550
		public static void RecreateHudTextsIfControlling(Character character)
		{
			CharacterHUD.RecreateHudTextsIfControllingProjSpecific(character);
		}

		// Token: 0x06000643 RID: 1603 RVA: 0x00037358 File Offset: 0x00035558
		public static void RecreateHudTextsIfFocused(params Item[] items)
		{
			CharacterHUD.RecreateHudTextsIfFocusedProjSpecific(items);
		}

		// Token: 0x06000646 RID: 1606 RVA: 0x000373A4 File Offset: 0x000355A4
		[CompilerGenerated]
		internal static bool <Draw>g__DrawIcon|28_2(Order o)
		{
			if (o != null)
			{
				Item i = o.TargetEntity as Item;
				return i == null || o.DrawIconWhenContained || i.GetRootInventoryOwner() == i;
			}
			return false;
		}

		// Token: 0x06000647 RID: 1607 RVA: 0x000373D8 File Offset: 0x000355D8
		[CompilerGenerated]
		internal static float <Draw>g__GetDistanceBasedIconAlpha|28_0(ISpatialEntity target, float maxDistance = 1000f, ref CharacterHUD.<>c__DisplayClass28_0 A_2)
		{
			float dist = Vector2.Distance(A_2.character.WorldPosition, target.WorldPosition);
			return Math.Min((maxDistance - dist) / maxDistance * 2f, 1f);
		}

		// Token: 0x06000648 RID: 1608 RVA: 0x00037414 File Offset: 0x00035614
		[CompilerGenerated]
		internal static void <Draw>g__DrawInteractionIcon|28_3(Entity entity, Identifier iconStyle, ref CharacterHUD.<>c__DisplayClass28_0 A_2)
		{
			if (entity == null || entity.Removed)
			{
				return;
			}
			Character character = entity as Character;
			Hull hull;
			if (character == null)
			{
				Item item = entity as Item;
				if (item == null)
				{
					hull = null;
				}
				else
				{
					hull = item.CurrentHull;
				}
			}
			else
			{
				hull = character.CurrentHull;
			}
			Hull currentHull = hull;
			Range<float> visibleRange = new Range<float>((currentHull == Character.Controlled.CurrentHull) ? 500f : 100f, float.PositiveInfinity);
			LocalizedString label = null;
			Character characterEntity = entity as Character;
			if (characterEntity != null)
			{
				if (characterEntity.IsDead || characterEntity.IsIncapacitated)
				{
					return;
				}
				if (characterEntity != null && characterEntity.CampaignInteractionType == CampaignMode.InteractionType.Examine)
				{
					if (Vector2.DistanceSquared(A_2.character.Position, entity.Position) > 250000f)
					{
						return;
					}
					Body body = Submarine.CheckVisibility(A_2.character.SimPosition, entity.SimPosition, true, false, true, true, true, null);
					if (body != null && body.UserData != entity)
					{
						return;
					}
					visibleRange = new Range<float>(-100f, 500f);
				}
				LocalizedString localizedString;
				if (characterEntity == null)
				{
					localizedString = null;
				}
				else
				{
					CharacterInfo info = characterEntity.Info;
					localizedString = ((info != null) ? info.Title : null);
				}
				label = localizedString;
			}
			GUIComponentStyle style = GUIStyle.GetComponentStyle(iconStyle);
			if (style == null)
			{
				return;
			}
			float dist = Vector2.Distance(A_2.character.WorldPosition, entity.WorldPosition);
			float distFactor = 1f - MathUtils.InverseLerp(1000f, 3000f, dist);
			float alpha = MathHelper.Lerp(0.3f, 1f, distFactor);
			SpriteBatch spriteBatch = A_2.spriteBatch;
			Vector2 drawPosition = entity.DrawPosition;
			Camera cam = A_2.cam;
			Sprite defaultSprite = style.GetDefaultSprite();
			Color color = style.Color * alpha;
			bool createOffset = true;
			float scaleMultiplier = 1f;
			LocalizedString label2 = label;
			GUI.DrawIndicator(spriteBatch, drawPosition, cam, visibleRange, defaultSprite, color, createOffset, scaleMultiplier, null, label2);
		}

		// Token: 0x06000649 RID: 1609 RVA: 0x000375C8 File Offset: 0x000357C8
		[CompilerGenerated]
		internal static void <UpdateBossProgressBars>g__SetColor|36_0(CharacterHUD.ProgressBar bossHealthBar, GUIComponent container, float alpha)
		{
			foreach (GUIComponent component in container.GetAllChildren())
			{
				component.Color = new Color(bossHealthBar.Color, (int)((byte)(alpha * 255f)));
				GUITextBlock textBlock = component as GUITextBlock;
				if (textBlock != null)
				{
					textBlock.TextColor = new Color(bossHealthBar.Completed ? Color.Gray : textBlock.TextColor, (int)((byte)(alpha * 255f)));
				}
			}
		}

		// Token: 0x04000334 RID: 820
		private static readonly Dictionary<ISpatialEntity, int> orderIndicatorCount = new Dictionary<ISpatialEntity, int>();

		// Token: 0x04000335 RID: 821
		private const float ItemOverlayDelay = 1f;

		// Token: 0x04000336 RID: 822
		private static Item focusedItem;

		// Token: 0x04000337 RID: 823
		private static float focusedItemOverlayTimer;

		// Token: 0x04000338 RID: 824
		private static readonly List<Item> brokenItems = new List<Item>();

		// Token: 0x04000339 RID: 825
		private static float brokenItemsCheckTimer;

		// Token: 0x0400033A RID: 826
		private static readonly List<CharacterHUD.ProgressBar> bossProgressBars = new List<CharacterHUD.ProgressBar>();

		// Token: 0x0400033B RID: 827
		private static readonly Dictionary<Identifier, LocalizedString> cachedHudTexts = new Dictionary<Identifier, LocalizedString>();

		// Token: 0x0400033C RID: 828
		private static LanguageIdentifier cachedHudTextLanguage = LanguageIdentifier.None;

		// Token: 0x0400033D RID: 829
		private static GUILayoutGroup bossHealthContainer;

		// Token: 0x0400033E RID: 830
		private static GUIFrame hudFrame;

		// Token: 0x04000340 RID: 832
		private static bool lastHudTextsContextual;

		// Token: 0x04000341 RID: 833
		private static float timeHealthWindowClosed;

		// Token: 0x020006C5 RID: 1733
		private abstract class ProgressBar
		{
			// Token: 0x170019B9 RID: 6585
			// (get) Token: 0x060066A9 RID: 26281
			public abstract bool Completed { get; }

			// Token: 0x170019BA RID: 6586
			// (get) Token: 0x060066AA RID: 26282
			public abstract bool Interrupted { get; }

			// Token: 0x170019BB RID: 6587
			// (get) Token: 0x060066AB RID: 26283
			public abstract float State { get; }

			// Token: 0x170019BC RID: 6588
			// (get) Token: 0x060066AC RID: 26284
			public abstract string NumberToDisplay { get; }

			// Token: 0x170019BD RID: 6589
			// (get) Token: 0x060066AD RID: 26285
			public abstract Color Color { get; }

			// Token: 0x060066AE RID: 26286 RVA: 0x00347900 File Offset: 0x00345B00
			public ProgressBar(LocalizedString label, float fadeTimer = 120f)
			{
				this.FadeTimer = fadeTimer;
				this.TopContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.18f, 0.03f), CharacterHUD.HUDFrame.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal)
				{
					MinSize = new Point(100, 50),
					RelativeOffset = new Vector2(0f, 0.01f)
				}, false, Anchor.TopCenter);
				new GUITextBlock(new RectTransform(new Vector2(1f, 0.4f), this.TopContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), label, new Color?(GUIStyle.Red), null, Alignment.Center, false, "", null);
				this.TopBar = new GUIProgressBar(new RectTransform(new Vector2(1f, 0.6f), this.TopContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
				{
					MinSize = new Point(100, HUDLayoutSettings.HealthBarArea.Size.Y)
				}, 0f, null, "CharacterHealthBarCentered", true)
				{
					Color = GUIStyle.Red
				};
				this.<.ctor>g__CreateNumberText|15_2(this.TopBar);
				this.SideContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.05f), CharacterHUD.bossHealthContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
				{
					MinSize = new Point(80, 60)
				}, false, Anchor.TopRight);
				new GUITextBlock(new RectTransform(new Vector2(1f, 0.3f), this.SideContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), label, new Color?(GUIStyle.Red), null, Alignment.CenterRight, false, "", null);
				this.SideBar = new GUIProgressBar(new RectTransform(new Vector2(1f, 0.7f), this.SideContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), 0f, null, "CharacterHealthBar", true)
				{
					Color = GUIStyle.Red
				};
				this.<.ctor>g__CreateNumberText|15_2(this.SideBar);
				this.TopContainer.Visible = (this.SideContainer.Visible = false);
				this.TopContainer.CanBeFocused = false;
				this.TopContainer.Children.ForEach(delegate(GUIComponent c)
				{
					c.CanBeFocused = false;
				});
				this.SideContainer.CanBeFocused = false;
				this.SideContainer.Children.ForEach(delegate(GUIComponent c)
				{
					c.CanBeFocused = false;
				});
			}

			// Token: 0x060066AF RID: 26287
			public abstract bool IsDuplicate(object targetObject);

			// Token: 0x060066B0 RID: 26288 RVA: 0x00347C50 File Offset: 0x00345E50
			[CompilerGenerated]
			private void <.ctor>g__CreateNumberText|15_2(GUIComponent parent)
			{
				new GUITextBlock(new RectTransform(Vector2.One, parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
				{
					AbsoluteOffset = new Point(2)
				}, string.Empty, new Color?(GUIStyle.TextColorDark), null, Alignment.Center, false, "", null).TextGetter = (() => this.NumberToDisplay);
				new GUITextBlock(new RectTransform(Vector2.One, parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), string.Empty, new Color?(GUIStyle.TextColorBright), null, Alignment.Center, false, "", null).TextGetter = (() => this.NumberToDisplay);
			}

			// Token: 0x040037D7 RID: 14295
			public float FadeTimer;

			// Token: 0x040037D8 RID: 14296
			public readonly GUIComponent TopContainer;

			// Token: 0x040037D9 RID: 14297
			public readonly GUIComponent SideContainer;

			// Token: 0x040037DA RID: 14298
			public readonly GUIProgressBar TopBar;

			// Token: 0x040037DB RID: 14299
			public readonly GUIProgressBar SideBar;
		}

		// Token: 0x020006C6 RID: 1734
		private class HealthBar : CharacterHUD.ProgressBar
		{
			// Token: 0x170019BE RID: 6590
			// (get) Token: 0x060066B3 RID: 26291 RVA: 0x00347D5F File Offset: 0x00345F5F
			public override float State
			{
				get
				{
					return this.Character.Vitality / this.Character.MaxVitality;
				}
			}

			// Token: 0x170019BF RID: 6591
			// (get) Token: 0x060066B4 RID: 26292 RVA: 0x00347D78 File Offset: 0x00345F78
			public override bool Completed
			{
				get
				{
					return this.Character.IsDead;
				}
			}

			// Token: 0x170019C0 RID: 6592
			// (get) Token: 0x060066B5 RID: 26293 RVA: 0x00347D85 File Offset: 0x00345F85
			public override bool Interrupted
			{
				get
				{
					return this.Character.Removed || !this.Character.Enabled;
				}
			}

			// Token: 0x170019C1 RID: 6593
			// (get) Token: 0x060066B6 RID: 26294 RVA: 0x00347DA4 File Offset: 0x00345FA4
			public override Color Color
			{
				get
				{
					return (this.Character.CharacterHealth.GetAfflictionStrengthByType(AfflictionPrefab.PoisonType, true) > 0f || this.Character.CharacterHealth.GetAfflictionStrengthByType(AfflictionPrefab.ParalysisType, true) > 0f) ? GUIStyle.HealthBarColorPoisoned : GUIStyle.Red;
				}
			}

			// Token: 0x170019C2 RID: 6594
			// (get) Token: 0x060066B7 RID: 26295 RVA: 0x00347DFC File Offset: 0x00345FFC
			public override string NumberToDisplay
			{
				get
				{
					return string.Empty;
				}
			}

			// Token: 0x060066B8 RID: 26296 RVA: 0x00347E03 File Offset: 0x00346003
			public HealthBar(Character character) : base(character.DisplayName, 120f)
			{
				this.Character = character;
			}

			// Token: 0x060066B9 RID: 26297 RVA: 0x00347E24 File Offset: 0x00346024
			public override bool IsDuplicate(object targetObject)
			{
				Character character = targetObject as Character;
				return character != null && this.Character == character;
			}

			// Token: 0x040037DC RID: 14300
			public readonly Character Character;
		}

		// Token: 0x020006C7 RID: 1735
		private class MissionProgressBar : CharacterHUD.ProgressBar
		{
			// Token: 0x170019C3 RID: 6595
			// (get) Token: 0x060066BA RID: 26298 RVA: 0x00347E46 File Offset: 0x00346046
			public override float State
			{
				get
				{
					return (float)this.Mission.State / (float)this.Mission.Prefab.MaxProgressState;
				}
			}

			// Token: 0x170019C4 RID: 6596
			// (get) Token: 0x060066BB RID: 26299 RVA: 0x00347E66 File Offset: 0x00346066
			public override bool Completed
			{
				get
				{
					return this.Mission.State >= this.Mission.Prefab.MaxProgressState;
				}
			}

			// Token: 0x170019C5 RID: 6597
			// (get) Token: 0x060066BC RID: 26300 RVA: 0x00347E88 File Offset: 0x00346088
			public override bool Interrupted
			{
				get
				{
					if (!this.Mission.Failed)
					{
						GameSession gameSession = GameMain.GameSession;
						if (((gameSession != null) ? gameSession.Missions : null) != null)
						{
							return !GameMain.GameSession.Missions.Contains(this.Mission);
						}
					}
					return true;
				}
			}

			// Token: 0x170019C6 RID: 6598
			// (get) Token: 0x060066BD RID: 26301 RVA: 0x00347EC4 File Offset: 0x003460C4
			public override Color Color
			{
				get
				{
					return this.Mission.Prefab.ProgressBarColor;
				}
			}

			// Token: 0x170019C7 RID: 6599
			// (get) Token: 0x060066BE RID: 26302 RVA: 0x00347ED8 File Offset: 0x003460D8
			public override string NumberToDisplay
			{
				get
				{
					if (!this.Mission.Prefab.ShowProgressInNumbers)
					{
						return string.Empty;
					}
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
					defaultInterpolatedStringHandler.AppendFormatted<int>(this.Mission.State);
					defaultInterpolatedStringHandler.AppendLiteral("/");
					defaultInterpolatedStringHandler.AppendFormatted<int>(this.Mission.Prefab.MaxProgressState);
					return defaultInterpolatedStringHandler.ToStringAndClear();
				}
			}

			// Token: 0x060066BF RID: 26303 RVA: 0x00347F42 File Offset: 0x00346142
			public MissionProgressBar(Mission mission) : base(mission.Prefab.ProgressBarLabel, float.PositiveInfinity)
			{
				this.Mission = mission;
			}

			// Token: 0x060066C0 RID: 26304 RVA: 0x00347F64 File Offset: 0x00346164
			public override bool IsDuplicate(object targetObject)
			{
				Mission mission = targetObject as Mission;
				return mission != null && this.Mission == mission;
			}

			// Token: 0x040037DD RID: 14301
			public readonly Mission Mission;
		}
	}
}
