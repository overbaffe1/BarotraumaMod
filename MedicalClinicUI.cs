using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x020000AC RID: 172
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class MedicalClinicUI
	{
		// Token: 0x06001597 RID: 5527 RVA: 0x000CAF06 File Offset: 0x000C9106
		public MedicalClinicUI(MedicalClinic clinic, GUIComponent parent)
		{
			this.medicalClinic = clinic;
			this.container = parent;
			clinic.OnUpdate = new Action(this.OnMedicalClinicUpdated);
			this.CreateUI();
		}

		// Token: 0x06001598 RID: 5528 RVA: 0x000CAF34 File Offset: 0x000C9134
		private void OnMedicalClinicUpdated()
		{
			this.UpdateCrewPanel();
			this.UpdatePending();
			this.UpdatePopupAfflictions();
		}

		// Token: 0x06001599 RID: 5529 RVA: 0x000CAF48 File Offset: 0x000C9148
		private void UpdatePopupAfflictions()
		{
			MedicalClinicUI.PopupAfflictionList? popupAfflictionList = this.selectedCrewAfflictionList;
			if (popupAfflictionList != null)
			{
				MedicalClinicUI.PopupAfflictionList afflictionList = popupAfflictionList.GetValueOrDefault();
				foreach (MedicalClinicUI.PopupAffliction popupAffliction in afflictionList.Afflictions)
				{
					MedicalClinicUI.ToggleElements(MedicalClinicUI.ElementState.Enabled, popupAffliction.ElementsToDisable);
					if (this.medicalClinic.IsAfflictionPending(afflictionList.Target, popupAffliction.Target))
					{
						MedicalClinicUI.ToggleElements(MedicalClinicUI.ElementState.Disabled, popupAffliction.ElementsToDisable);
					}
				}
				afflictionList.TreatAllButton.Enabled = true;
				if (afflictionList.Afflictions.All((MedicalClinicUI.PopupAffliction a) => this.medicalClinic.IsAfflictionPending(afflictionList.Target, a.Target)))
				{
					afflictionList.TreatAllButton.Enabled = false;
				}
				return;
			}
		}

		// Token: 0x0600159A RID: 5530 RVA: 0x000CB03C File Offset: 0x000C923C
		private void UpdatePending()
		{
			MedicalClinicUI.PendingHealList? pendingHealList = this.pendingHealList;
			if (pendingHealList != null)
			{
				MedicalClinicUI.PendingHealList healList = pendingHealList.GetValueOrDefault();
				ImmutableArray<MedicalClinic.NetCrewMember> pendingList = this.medicalClinic.PendingHeals.ToImmutableArray<MedicalClinic.NetCrewMember>();
				foreach (MedicalClinic.NetCrewMember crewMember in pendingList)
				{
					MedicalClinicUI.PendingHealElement? pendingHealElement = healList.FindCrewElement(crewMember);
					if (pendingHealElement != null)
					{
						MedicalClinicUI.PendingHealElement element2 = pendingHealElement.GetValueOrDefault();
						element2.Target = crewMember;
						healList.UpdateElement(element2);
					}
					else
					{
						this.CreatePendingHealElement(healList.HealList.Content, crewMember, healList, ImmutableArray<MedicalClinic.NetAffliction>.Empty);
					}
				}
				using (List<MedicalClinicUI.PendingHealElement>.Enumerator enumerator2 = healList.HealElements.ToList<MedicalClinicUI.PendingHealElement>().GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						MedicalClinicUI.PendingHealElement element = enumerator2.Current;
						if (pendingList.Any((MedicalClinic.NetCrewMember member) => member.CharacterEquals(element.Target)))
						{
							this.UpdatePendingAfflictions(element);
						}
						else
						{
							healList.HealElements.Remove(element);
							healList.HealList.Content.RemoveChild(element.UIElement);
						}
					}
				}
				int totalCost = this.medicalClinic.GetTotalCost();
				healList.PriceBlock.Text = TextManager.FormatCurrency(totalCost, true);
				healList.PriceBlock.TextColor = GUIStyle.Red;
				healList.HealButton.Enabled = false;
				if (this.medicalClinic.GetBalance() >= totalCost)
				{
					healList.PriceBlock.TextColor = GUIStyle.TextColorNormal;
					if (this.medicalClinic.PendingHeals.Any<MedicalClinic.NetCrewMember>())
					{
						healList.HealButton.Enabled = true;
					}
				}
				return;
			}
		}

		// Token: 0x0600159B RID: 5531 RVA: 0x000CB208 File Offset: 0x000C9408
		private void UpdatePendingAfflictions(MedicalClinicUI.PendingHealElement element)
		{
			MedicalClinic.NetCrewMember crewMember = element.Target;
			foreach (MedicalClinic.NetAffliction affliction2 in crewMember.Afflictions.ToList<MedicalClinic.NetAffliction>())
			{
				MedicalClinicUI.PendingAfflictionElement? pendingAfflictionElement = element.FindAfflictionElement(affliction2);
				if (pendingAfflictionElement != null)
				{
					MedicalClinicUI.PendingAfflictionElement existingAffliction = pendingAfflictionElement.GetValueOrDefault();
					existingAffliction.Price.Text = TextManager.FormatCurrency((int)affliction2.Strength, true);
				}
				else
				{
					this.CreatePendingAffliction(element.AfflictionList, crewMember, affliction2, element);
				}
			}
			using (List<MedicalClinicUI.PendingAfflictionElement>.Enumerator enumerator2 = element.Afflictions.ToList<MedicalClinicUI.PendingAfflictionElement>().GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					MedicalClinicUI.PendingAfflictionElement afflictionElement = enumerator2.Current;
					if (!crewMember.Afflictions.Any((MedicalClinic.NetAffliction affliction) => affliction.AfflictionEquals(afflictionElement.Target)))
					{
						element.Afflictions.Remove(afflictionElement);
						element.AfflictionList.Content.RemoveChild(afflictionElement.UIElement);
					}
				}
			}
		}

		// Token: 0x0600159C RID: 5532 RVA: 0x000CB344 File Offset: 0x000C9544
		public void UpdateCrewPanel()
		{
			MedicalClinicUI.CrewHealList? crewHealList = this.crewHealList;
			if (crewHealList != null)
			{
				MedicalClinicUI.CrewHealList healList = crewHealList.GetValueOrDefault();
				ImmutableArray<CharacterInfo> crew = MedicalClinic.GetCrewCharacters();
				ImmutableArray<CharacterInfo>.Enumerator enumerator = crew.GetEnumerator();
				while (enumerator.MoveNext())
				{
					CharacterInfo info = enumerator.Current;
					if (!healList.HealElements.Any((MedicalClinicUI.CrewElement element) => element.Target == info))
					{
						this.CreateCrewEntry(healList.HealList.Content, healList, info, healList.Panel);
					}
				}
				using (List<MedicalClinicUI.CrewElement>.Enumerator enumerator2 = healList.HealElements.ToList<MedicalClinicUI.CrewElement>().GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						MedicalClinicUI.CrewElement element = enumerator2.Current;
						if (crew.Any((CharacterInfo info) => element.Target == info))
						{
							MedicalClinicUI.UpdateAfflictionList(element);
						}
						else
						{
							healList.HealElements.Remove(element);
							healList.HealList.Content.RemoveChild(element.UIElement);
						}
					}
				}
				IEnumerable<MedicalClinicUI.CrewElement> orderedList = healList.HealElements.OrderBy(delegate(MedicalClinicUI.CrewElement element)
				{
					Character character = element.Target.Character;
					if (character == null)
					{
						return 100f;
					}
					return character.HealthPercentage;
				});
				foreach (MedicalClinicUI.CrewElement element3 in orderedList)
				{
					element3.UIElement.SetAsLastChild();
				}
				healList.TreatAllButton.Enabled = false;
				foreach (MedicalClinicUI.CrewElement element2 in healList.HealElements)
				{
					if (element2.Afflictions.Count != 0)
					{
						healList.TreatAllButton.Enabled = true;
						break;
					}
				}
				return;
			}
		}

		// Token: 0x0600159D RID: 5533 RVA: 0x000CB54C File Offset: 0x000C974C
		private static void UpdateAfflictionList(MedicalClinicUI.CrewElement healElement)
		{
			Character character = healElement.Target.Character;
			CharacterHealth health = (character != null) ? character.CharacterHealth : null;
			if (health == null)
			{
				return;
			}
			Dictionary<AfflictionPrefab, float> afflictionAndStrength = new Dictionary<AfflictionPrefab, float>();
			IEnumerable<Affliction> allAfflictions = health.GetAllAfflictions();
			Func<Affliction, bool> predicate;
			if ((predicate = MedicalClinicUI.<>O.<0>__IsHealable) == null)
			{
				predicate = (MedicalClinicUI.<>O.<0>__IsHealable = new Func<Affliction, bool>(MedicalClinic.IsHealable));
			}
			foreach (Affliction affliction in allAfflictions.Where(predicate))
			{
				float strength;
				if (afflictionAndStrength.TryGetValue(affliction.Prefab, out strength))
				{
					strength += affliction.Strength;
					afflictionAndStrength[affliction.Prefab] = strength;
				}
				else
				{
					afflictionAndStrength.Add(affliction.Prefab, affliction.Strength);
				}
			}
			foreach (MedicalClinicUI.AfflictionElement element2 in healElement.Afflictions)
			{
				element2.UIElement.Visible = false;
			}
			healElement.OverflowIndicator.Visible = false;
			foreach (KeyValuePair<AfflictionPrefab, float> keyValuePair in afflictionAndStrength)
			{
				AfflictionPrefab afflictionPrefab;
				float num;
				keyValuePair.Deconstruct(out afflictionPrefab, out num);
				AfflictionPrefab prefab = afflictionPrefab;
				float strength2 = num;
				bool found = false;
				foreach (MedicalClinicUI.AfflictionElement existingElement in healElement.Afflictions)
				{
					if (existingElement.Target.AfflictionEquals(prefab))
					{
						GUIImage icon = existingElement.UIImage;
						if (icon != null)
						{
							icon.Color = CharacterHealth.GetAfflictionIconColor(prefab, strength2);
						}
						found = true;
					}
				}
				if (!found)
				{
					MedicalClinicUI.CreateCrewAfflictionIcon(healElement, healElement.AfflictionList.Content, prefab, strength2);
				}
			}
			using (List<MedicalClinicUI.AfflictionElement>.Enumerator enumerator5 = healElement.Afflictions.ToList<MedicalClinicUI.AfflictionElement>().GetEnumerator())
			{
				while (enumerator5.MoveNext())
				{
					MedicalClinicUI.AfflictionElement element = enumerator5.Current;
					if (!afflictionAndStrength.Any((KeyValuePair<AfflictionPrefab, float> pair) => element.Target.AfflictionEquals(pair.Key)))
					{
						healElement.AfflictionList.Content.RemoveChild(element.UIElement);
						healElement.Afflictions.Remove(element);
					}
				}
			}
			int i = 0;
			while (i < 3 && i < healElement.Afflictions.Count)
			{
				healElement.Afflictions[i].UIElement.Visible = true;
				i++;
			}
			healElement.OverflowIndicator.Visible = (healElement.Afflictions.Count > 3);
			healElement.OverflowIndicator.SetAsLastChild();
		}

		// Token: 0x0600159E RID: 5534 RVA: 0x000CB830 File Offset: 0x000C9A30
		private static void CreateCrewAfflictionIcon(MedicalClinicUI.CrewElement healElement, GUIComponent parent, AfflictionPrefab prefab, float strength)
		{
			GUIFrame backgroundFrame = new GUIFrame(new RectTransform(new Vector2(0.25f, 1f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
			{
				CanBeFocused = false,
				Visible = false
			};
			GUIImage uiIcon = null;
			Sprite icon = prefab.Icon;
			if (icon != null)
			{
				uiIcon = new GUIImage(new RectTransform(Vector2.One, backgroundFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), icon, true, null)
				{
					Color = CharacterHealth.GetAfflictionIconColor(prefab, strength)
				};
			}
			healElement.Afflictions.Add(new MedicalClinicUI.AfflictionElement(new MedicalClinic.NetAffliction
			{
				Prefab = prefab
			}, backgroundFrame, uiIcon));
		}

		// Token: 0x0600159F RID: 5535 RVA: 0x000CB914 File Offset: 0x000C9B14
		private void CreateUI()
		{
			this.container.ClearChildren();
			this.pendingHealList = null;
			this.playerBalanceElement = null;
			int panelMaxWidth = (int)(GUI.xScale * (float)((GUI.HorizontalAspectRatio < 1.4f) ? 650 : 560));
			GUIFrame paddedParent = new GUIFrame(new RectTransform(new Vector2(0.95f), this.container.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), null, null);
			GUILayoutGroup clinicContent = new GUILayoutGroup(new RectTransform(new Vector2(0.45f, 1f), paddedParent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MaxSize = new Point(panelMaxWidth, this.container.Rect.Height)
			}, false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.01f
			};
			GUILayoutGroup clinicLabelLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.1f), clinicContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft);
			new GUIImage(new RectTransform(Vector2.One, clinicLabelLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), "CrewManagementHeaderIcon", true);
			RectTransform rectT = new RectTransform(Vector2.One, clinicLabelLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = TextManager.Get("medicalclinic.medicalclinic");
			GUIFont largeFont = GUIStyle.LargeFont;
			new GUITextBlock(rectT, text, null, largeFont, Alignment.Left, false, "", null);
			GUIFrame clinicBackground = new GUIFrame(new RectTransform(Vector2.One, clinicContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null);
			this.CreateLeftSidePanel(clinicBackground);
			GUILayoutGroup crewContent = new GUILayoutGroup(new RectTransform(new Vector2(0.45f, 1f), paddedParent.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal)
			{
				MaxSize = new Point(panelMaxWidth, this.container.Rect.Height)
			}, false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.01f
			};
			this.playerBalanceElement = CampaignUI.AddBalanceElement(crewContent, new Vector2(1f, 0.1f));
			GUIFrame crewBackground = new GUIFrame(new RectTransform(Vector2.One, crewContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null);
			this.CreateRightSidePanel(crewBackground);
			this.prevResolution = new Point(GameMain.GraphicsWidth, GameMain.GraphicsHeight);
		}

		// Token: 0x060015A0 RID: 5536 RVA: 0x000CBC48 File Offset: 0x000C9E48
		private void CreateLeftSidePanel(GUIComponent parent)
		{
			this.crewHealList = null;
			GUILayoutGroup clinicContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.95f), parent.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.015f,
				Stretch = true
			};
			GUIListBox crewList = new GUIListBox(new RectTransform(Vector2.One, clinicContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
			GUIButton treatAllButton = new GUIButton(new RectTransform(new Vector2(1f, 0.05f), clinicContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("medicalclinic.treateveryone"), Alignment.Center, "", null)
			{
				OnClicked = delegate(GUIButton button, object _)
				{
					MedicalClinicUI.<>c__DisplayClass29_0 CS$<>8__locals1 = new MedicalClinicUI.<>c__DisplayClass29_0();
					CS$<>8__locals1.<>4__this = this;
					CS$<>8__locals1.button = button;
					if (this.isWaitingForServer)
					{
						return true;
					}
					CS$<>8__locals1.button.Enabled = false;
					this.isWaitingForServer = true;
					if (!this.medicalClinic.TreatAllButtonAction(delegate(MedicalClinic.CallbackOnlyRequest _)
					{
						base.<CreateLeftSidePanel>g__ReEnableButton|2();
					}))
					{
						CS$<>8__locals1.<CreateLeftSidePanel>g__ReEnableButton|2();
					}
					return true;
				}
			};
			this.crewHealList = new MedicalClinicUI.CrewHealList?(new MedicalClinicUI.CrewHealList(crewList, parent, treatAllButton));
		}

		// Token: 0x060015A1 RID: 5537 RVA: 0x000CBD7C File Offset: 0x000C9F7C
		private void CreateCrewEntry(GUIComponent parent, MedicalClinicUI.CrewHealList healList, CharacterInfo info, GUIComponent panel)
		{
			GUIButton crewBackground = new GUIButton(new RectTransform(new Vector2(1f, 0.1f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.Center, "ListBoxElement", null);
			GUILayoutGroup crewLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.95f), crewBackground.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft);
			GUILayoutGroup characterBlockLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.45f, 0.9f), crewLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft);
			MedicalClinicUI.CreateCharacterBlock(characterBlockLayout, info);
			GUIListBox afflictionList = new GUIListBox(new RectTransform(new Vector2(0.45f, 1f), crewLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, null, null, true, false);
			GUILayoutGroup healthLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.1f, 1f), crewLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.Center);
			RectTransform rectT = new RectTransform(Vector2.One, healthLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = string.Empty;
			GUIFont font = GUIStyle.SubHeadingFont;
			GUITextBlock guitextBlock = new GUITextBlock(rectT, text, null, font, Alignment.Center, false, "", null);
			guitextBlock.TextGetter = delegate()
			{
				string tag = "percentageformat";
				string varName = "[value]";
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
				Character character = info.Character;
				defaultInterpolatedStringHandler.AppendFormatted<int>((int)MathF.Round((character != null) ? character.HealthPercentage : 100f));
				return TextManager.GetWithVariable(tag, varName, defaultInterpolatedStringHandler.ToStringAndClear(), FormatCapitals.No);
			};
			guitextBlock.TextColor = GUIStyle.Green;
			RectTransform rectT2 = new RectTransform(new Vector2(0.25f, 1f), afflictionList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight);
			RichString text2 = "+";
			font = GUIStyle.LargeFont;
			GUITextBlock overflowIndicator = new GUITextBlock(rectT2, text2, null, font, Alignment.Center, false, "", null)
			{
				Visible = false,
				CanBeFocused = false,
				TextColor = GUIStyle.Red
			};
			MedicalClinic.NetCrewMember member = new MedicalClinic.NetCrewMember(info);
			crewBackground.OnClicked = delegate(GUIButton _, object _)
			{
				this.SelectCharacter(member, new Vector2((float)panel.Rect.Right, (float)crewBackground.Rect.Top));
				return true;
			};
			healList.HealElements.Add(new MedicalClinicUI.CrewElement(info, overflowIndicator, crewBackground, afflictionList));
		}

		// Token: 0x060015A2 RID: 5538 RVA: 0x000CC07C File Offset: 0x000CA27C
		private void CreateRightSidePanel(GUIComponent parent)
		{
			GUILayoutGroup pendingHealContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.95f), parent.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.015f,
				Stretch = true
			};
			RectTransform rectT = new RectTransform(new Vector2(1f, 0.05f), pendingHealContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = TextManager.Get("medicalclinic.pendingheals");
			GUIFont font = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT, text, null, font, Alignment.Left, false, "", null);
			GUIFrame healListContainer = new GUIFrame(new RectTransform(new Vector2(1f, 0.9f), pendingHealContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUITextBlock errorBlock = null;
			if (!GameMain.IsSingleplayer)
			{
				RectTransform rectT2 = new RectTransform(Vector2.One, healListContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text2 = TextManager.Get("pleasewaitupnp");
				font = GUIStyle.LargeFont;
				errorBlock = new GUITextBlock(rectT2, text2, null, font, Alignment.Center, false, "", null);
			}
			GUIListBox healList = new GUIListBox(new RectTransform(Vector2.One, healListContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				Spacing = GUI.IntScale(8f),
				Visible = GameMain.IsSingleplayer
			};
			GUILayoutGroup footerLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.1f), pendingHealContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			GUILayoutGroup priceLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.5f), footerLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			GUITextBlock priceLabelBlock = new GUITextBlock(new RectTransform(new Vector2(0.5f, 1f), priceLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("campaignstore.total"), null, null, Alignment.Left, false, "", null);
			RectTransform rectT3 = new RectTransform(new Vector2(0.5f, 1f), priceLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text3 = TextManager.FormatCurrency(this.medicalClinic.GetTotalCost(), true);
			font = GUIStyle.SubHeadingFont;
			GUITextBlock priceBlock = new GUITextBlock(rectT3, text3, null, font, Alignment.Right, false, "", null);
			GUILayoutGroup buttonLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.5f), footerLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterRight);
			GUIButton healButton = new GUIButton(new RectTransform(new Vector2(0.33f, 1f), buttonLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("medicalclinic.heal"), Alignment.Center, "", null)
			{
				ClickSound = GUISoundType.ConfirmTransaction,
				Enabled = (this.medicalClinic.PendingHeals.Any<MedicalClinic.NetCrewMember>() && this.medicalClinic.GetBalance() >= this.medicalClinic.GetTotalCost()),
				OnClicked = delegate(GUIButton button, object _)
				{
					button.Enabled = false;
					this.isWaitingForServer = true;
					if (!this.medicalClinic.HealAllButtonAction(delegate(MedicalClinic.HealRequest request)
					{
						this.isWaitingForServer = false;
						MedicalClinic.HealRequestResult healResult = request.HealResult;
						if (healResult != MedicalClinic.HealRequestResult.InsufficientFunds)
						{
							if (healResult == MedicalClinic.HealRequestResult.Refused)
							{
								GUI.NotifyPrompt(TextManager.Get("medicalclinic.unabletoheal"), TextManager.Get("medicalclinic.healrefused"));
							}
						}
						else
						{
							GUI.NotifyPrompt(TextManager.Get("medicalclinic.unabletoheal"), TextManager.Get("medicalclinic.insufficientfunds"));
						}
						button.Enabled = true;
						this.ClosePopup();
					}))
					{
						this.isWaitingForServer = false;
						button.Enabled = true;
					}
					this.ClosePopup();
					return true;
				}
			};
			GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(0.33f, 1f), buttonLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("campaignstore.clearall"), Alignment.Center, "", null);
			guibutton.ClickSound = GUISoundType.Cart;
			guibutton.OnClicked = delegate(GUIButton button, object _)
			{
				MedicalClinicUI.<>c__DisplayClass31_1 CS$<>8__locals1 = new MedicalClinicUI.<>c__DisplayClass31_1();
				CS$<>8__locals1.<>4__this = this;
				CS$<>8__locals1.button = button;
				if (this.isWaitingForServer)
				{
					return true;
				}
				CS$<>8__locals1.button.Enabled = false;
				this.isWaitingForServer = true;
				if (!this.medicalClinic.ClearAllButtonAction(delegate(MedicalClinic.CallbackOnlyRequest _)
				{
					base.<CreateRightSidePanel>g__ReEnableButton|4();
				}))
				{
					CS$<>8__locals1.<CreateRightSidePanel>g__ReEnableButton|4();
				}
				return true;
			};
			MedicalClinicUI.PendingHealList list = new MedicalClinicUI.PendingHealList(healList, priceBlock, healButton, errorBlock);
			foreach (MedicalClinic.NetCrewMember heal in this.GetPendingCharacters())
			{
				this.CreatePendingHealElement(healList.Content, heal, list, heal.Afflictions);
			}
			this.pendingHealList = new MedicalClinicUI.PendingHealList?(list);
		}

		// Token: 0x060015A3 RID: 5539 RVA: 0x000CC5A0 File Offset: 0x000CA7A0
		[NullableContext(0)]
		private void CreatePendingHealElement([Nullable(1)] GUIComponent parent, MedicalClinic.NetCrewMember crewMember, MedicalClinicUI.PendingHealList healList, ImmutableArray<MedicalClinic.NetAffliction> afflictions)
		{
			CharacterInfo healInfo = crewMember.FindCharacterInfo(MedicalClinic.GetCrewCharacters());
			if (healInfo == null)
			{
				return;
			}
			GUIFrame pendingHealBackground = new GUIFrame(new RectTransform(new Vector2(1f, 0.25f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "ListBoxElement", null)
			{
				CanBeFocused = false
			};
			GUILayoutGroup pendingHealLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.95f), pendingHealBackground.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			GUILayoutGroup topHeaderLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.3f), pendingHealLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true
			};
			MedicalClinicUI.CreateCharacterBlock(topHeaderLayout, healInfo);
			GUILayoutGroup bottomLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.7f), pendingHealLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.Center);
			GUIListBox pendingAfflictionList = new GUIListBox(new RectTransform(Vector2.One, bottomLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				AutoHideScrollBar = false,
				ScrollBarVisible = true
			};
			MedicalClinicUI.PendingHealElement healElement = new MedicalClinicUI.PendingHealElement(crewMember, pendingHealBackground, pendingAfflictionList);
			foreach (MedicalClinic.NetAffliction affliction in afflictions)
			{
				this.CreatePendingAffliction(pendingAfflictionList, crewMember, affliction, healElement);
			}
			healList.HealElements.Add(healElement);
			MedicalClinicUI.RecalculateLayouts(new GUILayoutGroup[]
			{
				pendingHealLayout,
				topHeaderLayout,
				bottomLayout
			});
			pendingAfflictionList.ForceUpdate();
		}

		// Token: 0x060015A4 RID: 5540 RVA: 0x000CC7A0 File Offset: 0x000CA9A0
		private void CreatePendingAffliction(GUIListBox parent, MedicalClinic.NetCrewMember crewMember, MedicalClinic.NetAffliction affliction, MedicalClinicUI.PendingHealElement healElement)
		{
			GUIFrame backgroundFrame = new GUIFrame(new RectTransform(new Vector2(1f, 0.33f), parent.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "ListBoxElement", null)
			{
				CanBeFocused = false
			};
			GUILayoutGroup parentLayout = new GUILayoutGroup(new RectTransform(Vector2.One, backgroundFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			AfflictionPrefab prefab = affliction.Prefab;
			if (prefab == null)
			{
				return;
			}
			Sprite icon = prefab.Icon;
			if (icon != null)
			{
				new GUIImage(new RectTransform(Vector2.One, parentLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), icon, true, null).Color = CharacterHealth.GetAfflictionIconColor(prefab, (float)affliction.Strength);
			}
			GUILayoutGroup textLayout = new GUILayoutGroup(new RectTransform(Vector2.One, parentLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			LocalizedString name = prefab.Name;
			GUIFrame textContainer = new GUIFrame(new RectTransform(new Vector2(0.6f, 1f), textLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			RectTransform rectT = new RectTransform(Vector2.One, textContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = name;
			GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
			GUITextBlock afflictionName = new GUITextBlock(rectT, text, null, subHeadingFont, Alignment.Left, false, "", null);
			RectTransform rectT2 = new RectTransform(new Vector2(0.2f, 1f), textLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = TextManager.FormatCurrency((int)affliction.Price, true);
			subHeadingFont = GUIStyle.SubHeadingFont;
			GUITextBlock healCost = new GUITextBlock(rectT2, text2, null, subHeadingFont, Alignment.Center, false, "", null)
			{
				Padding = Vector4.Zero
			};
			GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(0.2f, 1f), textLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.Center, "CrewManagementRemoveButton", null);
			guibutton.ClickSound = GUISoundType.Cart;
			guibutton.OnClicked = delegate(GUIButton button, object _)
			{
				button.Enabled = false;
				if (!this.medicalClinic.RemovePendingButtonAction(crewMember, affliction, delegate(MedicalClinic.CallbackOnlyRequest _)
				{
					button.Enabled = true;
				}))
				{
					button.Enabled = true;
				}
				return true;
			};
			MedicalClinicUI.EnsureTextDoesntOverflow(name.Value, afflictionName, textContainer.Rect, new ImmutableArray<GUILayoutGroup>?(ImmutableArray.Create<GUILayoutGroup>(textLayout, parentLayout)));
			healElement.Afflictions.Add(new MedicalClinicUI.PendingAfflictionElement(affliction, backgroundFrame, healCost));
			MedicalClinicUI.RecalculateLayouts(new GUILayoutGroup[]
			{
				parentLayout,
				textLayout
			});
			parent.ForceUpdate();
		}

		// Token: 0x060015A5 RID: 5541 RVA: 0x000CCB14 File Offset: 0x000CAD14
		private static void CreateCharacterBlock(GUIComponent parent, CharacterInfo info)
		{
			new GUICustomComponent(new RectTransform(Vector2.One, parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), delegate(SpriteBatch spriteBatch, GUICustomComponent component)
			{
				info.DrawIcon(spriteBatch, component.Rect.Center.ToVector2(), component.Rect.Size.ToVector2(), false);
			}, null);
			GUILayoutGroup textGroup = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.8f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			string characterName = info.Name;
			LocalizedString jobName = null;
			GUITextBlock nameBlock = new GUITextBlock(new RectTransform(new Vector2(1f, 0.5f), textGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), characterName, null, null, Alignment.Left, false, "", null);
			GUITextBlock jobBlock = null;
			Job job = info.Job;
			if (job != null)
			{
				LocalizedString name = job.Name;
				if (name != null)
				{
					JobPrefab prefab = job.Prefab;
					if (prefab != null)
					{
						Color color = prefab.UIColor;
						jobName = name;
						jobBlock = new GUITextBlock(new RectTransform(new Vector2(1f, 0.5f), textGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), jobName, null, null, Alignment.Left, false, "", null);
						nameBlock.TextColor = color;
					}
				}
			}
			GUILayoutGroup layoutGroup = parent as GUILayoutGroup;
			if (layoutGroup != null)
			{
				ImmutableArray<GUILayoutGroup> layoutGroups = ImmutableArray.Create<GUILayoutGroup>(layoutGroup, textGroup);
				MedicalClinicUI.EnsureTextDoesntOverflow(characterName, nameBlock, parent.Rect, new ImmutableArray<GUILayoutGroup>?(layoutGroups));
				if (jobBlock == null)
				{
					return;
				}
				MedicalClinicUI.EnsureTextDoesntOverflow((jobName != null) ? jobName.Value : null, jobBlock, parent.Rect, new ImmutableArray<GUILayoutGroup>?(layoutGroups));
			}
		}

		// Token: 0x060015A6 RID: 5542 RVA: 0x000CCD20 File Offset: 0x000CAF20
		private void SelectCharacter(MedicalClinic.NetCrewMember crewMember, Vector2 location)
		{
			MedicalClinicUI.<>c__DisplayClass35_0 CS$<>8__locals1 = new MedicalClinicUI.<>c__DisplayClass35_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.crewMember = crewMember;
			CS$<>8__locals1.info = CS$<>8__locals1.crewMember.FindCharacterInfo(MedicalClinic.GetCrewCharacters());
			if (CS$<>8__locals1.info == null)
			{
				return;
			}
			if (this.isWaitingForServer)
			{
				return;
			}
			this.ClosePopup();
			GUIFrame mainFrame = new GUIFrame(new RectTransform(new Vector2(0.28f, 0.5f), this.container.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				ScreenSpaceOffset = location.ToPoint()
			}, "", null);
			GUILayoutGroup mainLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.9f), mainFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.01f,
				Stretch = true
			};
			if (mainFrame.Rect.Bottom > GameMain.GraphicsHeight)
			{
				mainFrame.RectTransform.ScreenSpaceOffset = new Point((int)location.X, GameMain.GraphicsHeight - mainFrame.Rect.Height);
			}
			MedicalClinicUI.<>c__DisplayClass35_0 CS$<>8__locals2 = CS$<>8__locals1;
			RectTransform rectT = new RectTransform(Vector2.One, mainFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = TextManager.Get("pleasewaitupnp");
			GUIFont largeFont = GUIStyle.LargeFont;
			CS$<>8__locals2.feedbackBlock = new GUITextBlock(rectT, text, null, largeFont, Alignment.Center, true, "", null)
			{
				Visible = true
			};
			CS$<>8__locals1.treatAllButton = new GUIButton(new RectTransform(new Vector2(1f, 0.2f), mainLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("medicalclinic.treatall"), Alignment.Center, "", null)
			{
				ClickSound = GUISoundType.Cart,
				Font = GUIStyle.SubHeadingFont,
				Visible = false
			};
			CS$<>8__locals1.afflictionList = new GUIListBox(new RectTransform(new Vector2(1f, 0.8f), mainLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				Visible = false
			};
			CS$<>8__locals1.popupAfflictionList = new MedicalClinicUI.PopupAfflictionList(CS$<>8__locals1.crewMember, CS$<>8__locals1.afflictionList, CS$<>8__locals1.treatAllButton);
			this.selectedCrewElement = mainFrame;
			this.selectedCrewAfflictionList = new MedicalClinicUI.PopupAfflictionList?(CS$<>8__locals1.popupAfflictionList);
			this.isWaitingForServer = true;
			if (!this.medicalClinic.RequestAfflictions(CS$<>8__locals1.info, new Action<MedicalClinic.AfflictionRequest>(CS$<>8__locals1.<SelectCharacter>g__OnReceived|0)))
			{
				this.isWaitingForServer = false;
				this.ClosePopup();
			}
		}

		// Token: 0x060015A7 RID: 5543 RVA: 0x000CD01C File Offset: 0x000CB21C
		private MedicalClinicUI.CreatedPopupAfflictionElement CreatePopupAffliction(GUIComponent parent, MedicalClinic.NetCrewMember crewMember, MedicalClinic.NetAffliction affliction)
		{
			ToolBox.ThrowIfNull<AfflictionPrefab>(affliction.Prefab);
			GUIFrame backgroundFrame = new GUIFrame(new RectTransform(new Vector2(1f, 0.33f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "ListBoxElement", null);
			new GUIFrame(new RectTransform(new Vector2(1f, 0.01f), backgroundFrame.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal), "HorizontalLine", null);
			GUILayoutGroup mainLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.9f), backgroundFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.05f
			};
			GUILayoutGroup topLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.33f), mainLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			Color iconColor = CharacterHealth.GetAfflictionIconColor(affliction.Prefab, (float)affliction.Strength);
			GUIImage icon = new GUIImage(new RectTransform(Vector2.One, topLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), affliction.Prefab.Icon, true, null)
			{
				Color = iconColor,
				DisabledColor = iconColor * 0.5f
			};
			GUILayoutGroup topTextLayout = new GUILayoutGroup(new RectTransform(Vector2.One, topLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			RectTransform rectT = new RectTransform(new Vector2(0.5f, 1f), topTextLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = affliction.Prefab.Name;
			GUIFont font = GUIStyle.SubHeadingFont;
			GUITextBlock prefabBlock = new GUITextBlock(rectT, text, null, font, Alignment.Left, false, "", null);
			Color textColor = Color.Lerp(GUIStyle.Orange, GUIStyle.Red, (float)affliction.Strength / affliction.Prefab.MaxStrength);
			LocalizedString vitalityText = (affliction.VitalityDecrease == 0) ? string.Empty : TextManager.GetWithVariable("medicalclinic.vitalitydifference", "[amount]", (-affliction.VitalityDecrease).ToString(), FormatCapitals.No);
			GUITextBlock vitalityBlock = new GUITextBlock(new RectTransform(new Vector2(0.25f, 1f), topTextLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), vitalityText, null, null, Alignment.Center, false, "", null)
			{
				TextColor = textColor,
				DisabledTextColor = textColor * 0.5f,
				Padding = Vector4.Zero,
				AutoScaleHorizontal = true
			};
			LocalizedString severityText = Affliction.GetStrengthText((float)affliction.Strength, affliction.Prefab.MaxStrength);
			RectTransform rectT2 = new RectTransform(new Vector2(0.25f, 1f), topTextLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = severityText;
			font = GUIStyle.SubHeadingFont;
			GUITextBlock severityBlock = new GUITextBlock(rectT2, text2, null, font, Alignment.Center, false, "", null)
			{
				TextColor = textColor,
				DisabledTextColor = textColor * 0.5f,
				Padding = Vector4.Zero,
				AutoScaleHorizontal = true
			};
			MedicalClinicUI.EnsureTextDoesntOverflow(affliction.Prefab.Name.Value, prefabBlock, prefabBlock.Rect, new ImmutableArray<GUILayoutGroup>?(ImmutableArray.Create<GUILayoutGroup>(mainLayout, topLayout, topTextLayout)));
			GUILayoutGroup bottomLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.66f), mainLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft);
			GUILayoutGroup bottomTextLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.8f, 1f), bottomLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.05f
			};
			LocalizedString description = affliction.Prefab.GetDescription((float)affliction.Strength, AfflictionPrefab.Description.TargetType.OtherCharacter);
			RectTransform rectT3 = new RectTransform(new Vector2(1f, 0.6f), bottomTextLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text3 = description;
			font = GUIStyle.SmallFont;
			GUITextBlock descriptionBlock = new GUITextBlock(rectT3, text3, null, font, Alignment.Left, true, "", null)
			{
				ToolTip = description
			};
			bool truncated = false;
			while (descriptionBlock.TextSize.Y > (float)descriptionBlock.Rect.Height && descriptionBlock.WrappedText.Contains('\n', StringComparison.Ordinal))
			{
				string[] split = descriptionBlock.WrappedText.Value.Split('\n', StringSplitOptions.None);
				descriptionBlock.Text = string.Join<string>('\n', split.Take(split.Length - 1));
				truncated = true;
			}
			if (truncated)
			{
				GUITextBlock guitextBlock = descriptionBlock;
				guitextBlock.Text += TextManager.Get("ellipsis");
			}
			RectTransform rectT4 = new RectTransform(new Vector2(1f, 0.25f), bottomTextLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text4 = TextManager.FormatCurrency((int)affliction.Price, true);
			font = GUIStyle.SubHeadingFont;
			GUITextBlock priceBlock = new GUITextBlock(rectT4, text4, null, font, Alignment.Left, false, "", null);
			GUIButton buyButton = new GUIButton(new RectTransform(new Vector2(0.2f, 0.75f), bottomLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.Center, "CrewManagementAddButton", null)
			{
				ClickSound = GUISoundType.Cart
			};
			ImmutableArray<GUIComponent> elementsToDisable = ImmutableArray.Create<GUIComponent>(new GUIComponent[]
			{
				prefabBlock,
				backgroundFrame,
				icon,
				vitalityBlock,
				severityBlock,
				buyButton,
				descriptionBlock,
				priceBlock
			});
			buyButton.OnClicked = delegate(GUIButton _, object __)
			{
				if (!buyButton.Enabled)
				{
					return false;
				}
				this.AddPending(elementsToDisable, crewMember, ImmutableArray.Create<MedicalClinic.NetAffliction>(affliction));
				return true;
			};
			return new MedicalClinicUI.CreatedPopupAfflictionElement(backgroundFrame, elementsToDisable);
		}

		// Token: 0x060015A8 RID: 5544 RVA: 0x000CD7BC File Offset: 0x000CB9BC
		[NullableContext(0)]
		private void AddPending([Nullable(new byte[]
		{
			0,
			1
		})] ImmutableArray<GUIComponent> elementsToDisable, MedicalClinic.NetCrewMember crewMember, ImmutableArray<MedicalClinic.NetAffliction> afflictions)
		{
			MedicalClinic.NetCrewMember? netCrewMember = this.medicalClinic.PendingHeals.FirstOrNull((MedicalClinic.NetCrewMember m) => m.CharacterEquals(crewMember));
			MedicalClinic.NetCrewMember existingMember;
			if (netCrewMember != null)
			{
				MedicalClinic.NetCrewMember foundHeal = netCrewMember.GetValueOrDefault();
				existingMember = foundHeal;
			}
			else
			{
				MedicalClinic.NetCrewMember crewMember2 = crewMember;
				crewMember2.Afflictions = ImmutableArray<MedicalClinic.NetAffliction>.Empty;
				MedicalClinic.NetCrewMember newMember = crewMember2;
				existingMember = newMember;
			}
			ImmutableArray<MedicalClinic.NetAffliction>.Enumerator enumerator = afflictions.GetEnumerator();
			while (enumerator.MoveNext())
			{
				MedicalClinic.NetAffliction affliction = enumerator.Current;
				if (existingMember.Afflictions.FirstOrNull((MedicalClinic.NetAffliction a) => a.AfflictionEquals(affliction)) != null)
				{
					return;
				}
			}
			existingMember.Afflictions = existingMember.Afflictions.Concat(afflictions).ToImmutableArray<MedicalClinic.NetAffliction>();
			MedicalClinicUI.ToggleElements(MedicalClinicUI.ElementState.Disabled, elementsToDisable);
			if (!this.medicalClinic.AddPendingButtonAction(existingMember, delegate(MedicalClinic.CallbackOnlyRequest request)
			{
				if (request.Result == MedicalClinic.RequestResult.Timeout)
				{
					MedicalClinicUI.ToggleElements(MedicalClinicUI.ElementState.Enabled, elementsToDisable);
				}
			}))
			{
				MedicalClinicUI.ToggleElements(MedicalClinicUI.ElementState.Enabled, elementsToDisable);
			}
		}

		// Token: 0x060015A9 RID: 5545 RVA: 0x000CD8D4 File Offset: 0x000CBAD4
		public static void EnsureTextDoesntOverflow([Nullable(2)] string text, GUITextBlock textBlock, Rectangle bounds, [Nullable(new byte[]
		{
			0,
			1
		})] ImmutableArray<GUILayoutGroup>? layoutGroups = null)
		{
			MedicalClinicUI.<>c__DisplayClass39_0 CS$<>8__locals1;
			CS$<>8__locals1.layoutGroups = layoutGroups;
			if (string.IsNullOrWhiteSpace(text))
			{
				return;
			}
			string originalText = text;
			MedicalClinicUI.<EnsureTextDoesntOverflow>g__UpdateLayoutGroups|39_0(ref CS$<>8__locals1);
			while ((float)textBlock.Rect.X + textBlock.TextSize.X + textBlock.Padding.X + textBlock.Padding.W > (float)bounds.Right && !string.IsNullOrWhiteSpace(text))
			{
				string text2 = text;
				text = text2.Substring(0, text2.Length - 1);
				textBlock.Text = text + "...";
				textBlock.ToolTip = originalText;
				MedicalClinicUI.<EnsureTextDoesntOverflow>g__UpdateLayoutGroups|39_0(ref CS$<>8__locals1);
			}
		}

		// Token: 0x060015AA RID: 5546 RVA: 0x000CD97C File Offset: 0x000CBB7C
		public void RequestLatestPending()
		{
			MedicalClinicUI.<>c__DisplayClass40_0 CS$<>8__locals1 = new MedicalClinicUI.<>c__DisplayClass40_0();
			CS$<>8__locals1.<>4__this = this;
			this.UpdateCrewPanel();
			if (!GameMain.IsSingleplayer)
			{
				MedicalClinicUI.PendingHealList? pendingHealList = this.pendingHealList;
				if (pendingHealList != null)
				{
					MedicalClinicUI.PendingHealList valueOrDefault = pendingHealList.GetValueOrDefault();
					CS$<>8__locals1.errorBlock = valueOrDefault.ErrorBlock;
					if (CS$<>8__locals1.errorBlock != null)
					{
						CS$<>8__locals1.healList = valueOrDefault.HealList;
						if (CS$<>8__locals1.healList != null)
						{
							CS$<>8__locals1.errorBlock.Visible = true;
							CS$<>8__locals1.errorBlock.TextColor = GUIStyle.TextColorNormal;
							CS$<>8__locals1.errorBlock.Text = TextManager.Get("pleasewaitupnp");
							CS$<>8__locals1.healList.Visible = false;
							this.isWaitingForServer = true;
							this.medicalClinic.RequestLatestPending(new Action<MedicalClinic.PendingRequest>(CS$<>8__locals1.<RequestLatestPending>g__OnReceived|0));
							return;
						}
					}
				}
			}
		}

		// Token: 0x060015AB RID: 5547 RVA: 0x000CDA4C File Offset: 0x000CBC4C
		public void UpdateAfflictions(MedicalClinic.NetCrewMember crewMember)
		{
			MedicalClinicUI.PopupAfflictionList? popupAfflictionList = this.selectedCrewAfflictionList;
			if (popupAfflictionList != null)
			{
				MedicalClinicUI.PopupAfflictionList afflictionList = popupAfflictionList.GetValueOrDefault();
				if (afflictionList.Target.CharacterEquals(crewMember))
				{
					List<GUIComponent> allComponents = new List<GUIComponent>();
					using (HashSet<MedicalClinicUI.PopupAffliction>.Enumerator enumerator = afflictionList.Afflictions.ToHashSet<MedicalClinicUI.PopupAffliction>().GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							MedicalClinicUI.PopupAffliction existingAffliction = enumerator.Current;
							if (crewMember.Afflictions.None((MedicalClinic.NetAffliction received) => received.AfflictionEquals(existingAffliction.Target)))
							{
								existingAffliction.TargetElement.RectTransform.Parent = null;
								afflictionList.Afflictions.Remove(existingAffliction);
							}
							else
							{
								allComponents.AddRange(existingAffliction.ElementsToDisable);
							}
						}
					}
					ImmutableArray<MedicalClinic.NetAffliction>.Enumerator enumerator2 = crewMember.Afflictions.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						MedicalClinic.NetAffliction received = enumerator2.Current;
						if (!afflictionList.Afflictions.Any((MedicalClinicUI.PopupAffliction existing) => existing.Target.AfflictionEquals(received)))
						{
							MedicalClinicUI.CreatedPopupAfflictionElement createdComponents = this.CreatePopupAffliction(afflictionList.ListElement.Content, crewMember, received);
							allComponents.AddRange(createdComponents.AllCreatedElements);
							afflictionList.Afflictions.Add(new MedicalClinicUI.PopupAffliction(createdComponents.AllCreatedElements, createdComponents.MainElement, received));
						}
					}
					allComponents.Add(afflictionList.TreatAllButton);
					Func<MedicalClinic.NetAffliction, bool> <>9__3;
					afflictionList.TreatAllButton.OnClicked = delegate(GUIButton _, object _)
					{
						ImmutableArray<MedicalClinic.NetAffliction> afflictions2 = crewMember.Afflictions;
						Func<MedicalClinic.NetAffliction, bool> predicate;
						if ((predicate = <>9__3) == null)
						{
							predicate = (<>9__3 = ((MedicalClinic.NetAffliction a) => !this.medicalClinic.IsAfflictionPending(crewMember, a)));
						}
						ImmutableArray<MedicalClinic.NetAffliction> afflictions = afflictions2.Where(predicate).ToImmutableArray<MedicalClinic.NetAffliction>();
						if (!afflictions.Any<MedicalClinic.NetAffliction>())
						{
							return true;
						}
						this.AddPending(allComponents.ToImmutableArray<GUIComponent>(), crewMember, afflictions);
						return true;
					};
					this.UpdatePopupAfflictions();
					return;
				}
			}
		}

		// Token: 0x060015AC RID: 5548 RVA: 0x000CDC48 File Offset: 0x000CBE48
		public void ClosePopup()
		{
			GUIFrame popup = this.selectedCrewElement;
			if (popup != null)
			{
				popup.RectTransform.Parent = null;
			}
			this.selectedCrewElement = null;
			this.selectedCrewAfflictionList = null;
		}

		// Token: 0x060015AD RID: 5549 RVA: 0x000CDC80 File Offset: 0x000CBE80
		private static LocalizedString GetErrorText(MedicalClinic.RequestResult result)
		{
			LocalizedString result2;
			if (result == MedicalClinic.RequestResult.Timeout)
			{
				result2 = TextManager.Get("medicalclinic.requesttimeout");
			}
			else
			{
				result2 = TextManager.Get("error");
			}
			return result2;
		}

		// Token: 0x060015AE RID: 5550 RVA: 0x000CDCAA File Offset: 0x000CBEAA
		[NullableContext(0)]
		private ImmutableArray<MedicalClinic.NetCrewMember> GetPendingCharacters()
		{
			return this.medicalClinic.PendingHeals.ToImmutableArray<MedicalClinic.NetCrewMember>();
		}

		// Token: 0x060015AF RID: 5551 RVA: 0x000CDCBC File Offset: 0x000CBEBC
		private static void ToggleElements(MedicalClinicUI.ElementState state, [Nullable(new byte[]
		{
			0,
			1
		})] ImmutableArray<GUIComponent> elements)
		{
			foreach (GUIComponent component in elements)
			{
				GUIComponent guicomponent = component;
				bool enabled;
				if (state != MedicalClinicUI.ElementState.Enabled)
				{
					if (state != MedicalClinicUI.ElementState.Disabled)
					{
						throw new ArgumentOutOfRangeException("state", state, null);
					}
					enabled = false;
				}
				else
				{
					enabled = true;
				}
				guicomponent.Enabled = enabled;
			}
		}

		// Token: 0x060015B0 RID: 5552 RVA: 0x000CDD10 File Offset: 0x000CBF10
		private static void RecalculateLayouts(params GUILayoutGroup[] layouts)
		{
			foreach (GUILayoutGroup layout in layouts)
			{
				layout.Recalculate();
			}
		}

		// Token: 0x060015B1 RID: 5553 RVA: 0x000CDD38 File Offset: 0x000CBF38
		public void Update(float deltaTime)
		{
			if (this.prevResolution.X != GameMain.GraphicsWidth || this.prevResolution.Y != GameMain.GraphicsHeight)
			{
				this.CreateUI();
			}
			else
			{
				this.playerBalanceElement = CampaignUI.UpdateBalanceElement(this.playerBalanceElement);
			}
			this.refreshTimer += deltaTime;
			if (this.refreshTimer > 3f)
			{
				this.UpdateCrewPanel();
				this.refreshTimer = 0f;
			}
		}

		// Token: 0x060015B2 RID: 5554 RVA: 0x000CDDAE File Offset: 0x000CBFAE
		public void OnDeselected()
		{
			if (GameMain.NetworkMember != null)
			{
				MedicalClinic.SendUnsubscribeRequest();
			}
			this.ClosePopup();
		}

		// Token: 0x060015B6 RID: 5558 RVA: 0x000CDEF0 File Offset: 0x000CC0F0
		[CompilerGenerated]
		internal static void <EnsureTextDoesntOverflow>g__UpdateLayoutGroups|39_0(ref MedicalClinicUI.<>c__DisplayClass39_0 A_0)
		{
			if (A_0.layoutGroups == null)
			{
				return;
			}
			foreach (GUILayoutGroup layoutGroup in A_0.layoutGroups.Value)
			{
				layoutGroup.Recalculate();
			}
		}

		// Token: 0x04000AE5 RID: 2789
		private readonly MedicalClinic medicalClinic;

		// Token: 0x04000AE6 RID: 2790
		private readonly GUIComponent container;

		// Token: 0x04000AE7 RID: 2791
		private Point prevResolution;

		// Token: 0x04000AE8 RID: 2792
		private MedicalClinicUI.PendingHealList? pendingHealList;

		// Token: 0x04000AE9 RID: 2793
		private MedicalClinicUI.CrewHealList? crewHealList;

		// Token: 0x04000AEA RID: 2794
		[Nullable(2)]
		private GUIFrame selectedCrewElement;

		// Token: 0x04000AEB RID: 2795
		private MedicalClinicUI.PopupAfflictionList? selectedCrewAfflictionList;

		// Token: 0x04000AEC RID: 2796
		private bool isWaitingForServer;

		// Token: 0x04000AED RID: 2797
		private const float refreshTimerMax = 3f;

		// Token: 0x04000AEE RID: 2798
		private float refreshTimer;

		// Token: 0x04000AEF RID: 2799
		private CampaignUI.PlayerBalanceElement? playerBalanceElement;

		// Token: 0x0200099B RID: 2459
		[NullableContext(0)]
		private enum ElementState
		{
			// Token: 0x040041BC RID: 16828
			Enabled,
			// Token: 0x040041BD RID: 16829
			Disabled
		}

		// Token: 0x0200099C RID: 2460
		[Nullable(0)]
		private struct PendingAfflictionElement
		{
			// Token: 0x0600726C RID: 29292 RVA: 0x0036CE0F File Offset: 0x0036B00F
			public PendingAfflictionElement(MedicalClinic.NetAffliction target, GUIComponent element, GUITextBlock price)
			{
				this.UIElement = element;
				this.Target = target;
				this.Price = price;
			}

			// Token: 0x040041BE RID: 16830
			public readonly GUIComponent UIElement;

			// Token: 0x040041BF RID: 16831
			public readonly MedicalClinic.NetAffliction Target;

			// Token: 0x040041C0 RID: 16832
			public readonly GUITextBlock Price;
		}

		// Token: 0x0200099D RID: 2461
		[Nullable(0)]
		private struct PendingHealElement
		{
			// Token: 0x0600726D RID: 29293 RVA: 0x0036CE26 File Offset: 0x0036B026
			public PendingHealElement(MedicalClinic.NetCrewMember target, GUIComponent element, GUIListBox afflictionList)
			{
				this.UIElement = element;
				this.Target = target;
				this.AfflictionList = afflictionList;
				this.Afflictions = new List<MedicalClinicUI.PendingAfflictionElement>();
			}

			// Token: 0x0600726E RID: 29294 RVA: 0x0036CE48 File Offset: 0x0036B048
			public MedicalClinicUI.PendingAfflictionElement? FindAfflictionElement(MedicalClinic.NetAffliction target)
			{
				return this.Afflictions.FirstOrNull((MedicalClinicUI.PendingAfflictionElement element) => element.Target.Identifier == target.Identifier);
			}

			// Token: 0x040041C1 RID: 16833
			public readonly GUIComponent UIElement;

			// Token: 0x040041C2 RID: 16834
			public MedicalClinic.NetCrewMember Target;

			// Token: 0x040041C3 RID: 16835
			public readonly GUIListBox AfflictionList;

			// Token: 0x040041C4 RID: 16836
			public readonly List<MedicalClinicUI.PendingAfflictionElement> Afflictions;
		}

		// Token: 0x0200099E RID: 2462
		[Nullable(0)]
		private readonly struct AfflictionElement
		{
			// Token: 0x0600726F RID: 29295 RVA: 0x0036CE79 File Offset: 0x0036B079
			public AfflictionElement(MedicalClinic.NetAffliction target, GUIComponent element, [Nullable(2)] GUIImage icon)
			{
				this.UIElement = element;
				this.UIImage = icon;
				this.Target = target;
			}

			// Token: 0x040041C5 RID: 16837
			[Nullable(2)]
			public readonly GUIImage UIImage;

			// Token: 0x040041C6 RID: 16838
			public readonly GUIComponent UIElement;

			// Token: 0x040041C7 RID: 16839
			public readonly MedicalClinic.NetAffliction Target;
		}

		// Token: 0x0200099F RID: 2463
		[Nullable(0)]
		private readonly struct CrewElement
		{
			// Token: 0x06007270 RID: 29296 RVA: 0x0036CE90 File Offset: 0x0036B090
			public CrewElement(CharacterInfo target, GUIComponent overflowIndicator, GUIComponent element, GUIListBox afflictionList)
			{
				this.OverflowIndicator = overflowIndicator;
				this.UIElement = element;
				this.Target = target;
				this.AfflictionList = afflictionList;
				this.Afflictions = new List<MedicalClinicUI.AfflictionElement>();
			}

			// Token: 0x040041C8 RID: 16840
			public readonly GUIComponent UIElement;

			// Token: 0x040041C9 RID: 16841
			public readonly CharacterInfo Target;

			// Token: 0x040041CA RID: 16842
			public readonly GUIListBox AfflictionList;

			// Token: 0x040041CB RID: 16843
			public readonly List<MedicalClinicUI.AfflictionElement> Afflictions;

			// Token: 0x040041CC RID: 16844
			public readonly GUIComponent OverflowIndicator;
		}

		// Token: 0x020009A0 RID: 2464
		[Nullable(0)]
		private readonly struct PendingHealList
		{
			// Token: 0x06007271 RID: 29297 RVA: 0x0036CEBA File Offset: 0x0036B0BA
			public PendingHealList(GUIListBox healList, GUITextBlock priceBlock, GUIButton healButton, [Nullable(2)] GUITextBlock errorBlock)
			{
				this.HealList = healList;
				this.ErrorBlock = errorBlock;
				this.PriceBlock = priceBlock;
				this.HealButton = healButton;
				this.HealElements = new List<MedicalClinicUI.PendingHealElement>();
			}

			// Token: 0x06007272 RID: 29298 RVA: 0x0036CEE4 File Offset: 0x0036B0E4
			public void UpdateElement(MedicalClinicUI.PendingHealElement newElement)
			{
				foreach (MedicalClinicUI.PendingHealElement element in this.HealElements.ToList<MedicalClinicUI.PendingHealElement>())
				{
					if (element.Target.CharacterEquals(newElement.Target))
					{
						this.HealElements.Remove(element);
						this.HealElements.Add(newElement);
						break;
					}
				}
			}

			// Token: 0x06007273 RID: 29299 RVA: 0x0036CF64 File Offset: 0x0036B164
			public MedicalClinicUI.PendingHealElement? FindCrewElement(MedicalClinic.NetCrewMember crewMember)
			{
				return this.HealElements.FirstOrNull((MedicalClinicUI.PendingHealElement element) => element.Target.CharacterInfoID == crewMember.CharacterInfoID);
			}

			// Token: 0x040041CD RID: 16845
			public readonly GUIListBox HealList;

			// Token: 0x040041CE RID: 16846
			[Nullable(2)]
			public readonly GUITextBlock ErrorBlock;

			// Token: 0x040041CF RID: 16847
			public readonly GUITextBlock PriceBlock;

			// Token: 0x040041D0 RID: 16848
			public readonly List<MedicalClinicUI.PendingHealElement> HealElements;

			// Token: 0x040041D1 RID: 16849
			public readonly GUIButton HealButton;
		}

		// Token: 0x020009A1 RID: 2465
		[Nullable(0)]
		private readonly struct CrewHealList
		{
			// Token: 0x06007274 RID: 29300 RVA: 0x0036CF95 File Offset: 0x0036B195
			public CrewHealList(GUIListBox healList, GUIComponent panel, GUIComponent treatAllButton)
			{
				this.Panel = panel;
				this.HealList = healList;
				this.TreatAllButton = treatAllButton;
				this.HealElements = new List<MedicalClinicUI.CrewElement>();
			}

			// Token: 0x040041D2 RID: 16850
			public readonly GUIComponent Panel;

			// Token: 0x040041D3 RID: 16851
			public readonly GUIListBox HealList;

			// Token: 0x040041D4 RID: 16852
			public readonly GUIComponent TreatAllButton;

			// Token: 0x040041D5 RID: 16853
			public readonly List<MedicalClinicUI.CrewElement> HealElements;
		}

		// Token: 0x020009A2 RID: 2466
		[Nullable(0)]
		private readonly struct PopupAffliction
		{
			// Token: 0x06007275 RID: 29301 RVA: 0x0036CFB7 File Offset: 0x0036B1B7
			public PopupAffliction([Nullable(new byte[]
			{
				0,
				1
			})] ImmutableArray<GUIComponent> elementsToDisable, GUIComponent component, MedicalClinic.NetAffliction target)
			{
				this.Target = target;
				this.ElementsToDisable = elementsToDisable;
				this.TargetElement = component;
			}

			// Token: 0x040041D6 RID: 16854
			public readonly MedicalClinic.NetAffliction Target;

			// Token: 0x040041D7 RID: 16855
			[Nullable(new byte[]
			{
				0,
				1
			})]
			public readonly ImmutableArray<GUIComponent> ElementsToDisable;

			// Token: 0x040041D8 RID: 16856
			public readonly GUIComponent TargetElement;
		}

		// Token: 0x020009A3 RID: 2467
		[Nullable(0)]
		private readonly struct PopupAfflictionList
		{
			// Token: 0x06007276 RID: 29302 RVA: 0x0036CFCE File Offset: 0x0036B1CE
			public PopupAfflictionList(MedicalClinic.NetCrewMember crewMember, GUIListBox listElement, GUIButton treatAllButton)
			{
				this.ListElement = listElement;
				this.Target = crewMember;
				this.TreatAllButton = treatAllButton;
				this.Afflictions = new HashSet<MedicalClinicUI.PopupAffliction>();
			}

			// Token: 0x040041D9 RID: 16857
			public readonly MedicalClinic.NetCrewMember Target;

			// Token: 0x040041DA RID: 16858
			public readonly GUIListBox ListElement;

			// Token: 0x040041DB RID: 16859
			public readonly GUIButton TreatAllButton;

			// Token: 0x040041DC RID: 16860
			public readonly HashSet<MedicalClinicUI.PopupAffliction> Afflictions;
		}

		// Token: 0x020009A4 RID: 2468
		[Nullable(0)]
		private readonly struct CreatedPopupAfflictionElement : IEquatable<MedicalClinicUI.CreatedPopupAfflictionElement>
		{
			// Token: 0x06007277 RID: 29303 RVA: 0x0036CFF0 File Offset: 0x0036B1F0
			public CreatedPopupAfflictionElement(GUIComponent MainElement, [Nullable(new byte[]
			{
				0,
				1
			})] ImmutableArray<GUIComponent> AllCreatedElements)
			{
				this.MainElement = MainElement;
				this.AllCreatedElements = AllCreatedElements;
			}

			// Token: 0x17001A60 RID: 6752
			// (get) Token: 0x06007278 RID: 29304 RVA: 0x0036D000 File Offset: 0x0036B200
			// (set) Token: 0x06007279 RID: 29305 RVA: 0x0036D008 File Offset: 0x0036B208
			public GUIComponent MainElement { get; set; }

			// Token: 0x17001A61 RID: 6753
			// (get) Token: 0x0600727A RID: 29306 RVA: 0x0036D011 File Offset: 0x0036B211
			// (set) Token: 0x0600727B RID: 29307 RVA: 0x0036D019 File Offset: 0x0036B219
			[Nullable(new byte[]
			{
				0,
				1
			})]
			public ImmutableArray<GUIComponent> AllCreatedElements { [return: Nullable(new byte[]
			{
				0,
				1
			})] get; [param: Nullable(new byte[]
			{
				0,
				1
			})] set; }

			// Token: 0x0600727C RID: 29308 RVA: 0x0036D024 File Offset: 0x0036B224
			[NullableContext(0)]
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("CreatedPopupAfflictionElement");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x0600727D RID: 29309 RVA: 0x0036D070 File Offset: 0x0036B270
			[NullableContext(0)]
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("MainElement = ");
				builder.Append(this.MainElement);
				builder.Append(", AllCreatedElements = ");
				builder.Append(this.AllCreatedElements.ToString());
				return true;
			}

			// Token: 0x0600727E RID: 29310 RVA: 0x0036D0BE File Offset: 0x0036B2BE
			[CompilerGenerated]
			public static bool operator !=(MedicalClinicUI.CreatedPopupAfflictionElement left, MedicalClinicUI.CreatedPopupAfflictionElement right)
			{
				return !(left == right);
			}

			// Token: 0x0600727F RID: 29311 RVA: 0x0036D0CA File Offset: 0x0036B2CA
			[CompilerGenerated]
			public static bool operator ==(MedicalClinicUI.CreatedPopupAfflictionElement left, MedicalClinicUI.CreatedPopupAfflictionElement right)
			{
				return left.Equals(right);
			}

			// Token: 0x06007280 RID: 29312 RVA: 0x0036D0D4 File Offset: 0x0036B2D4
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return EqualityComparer<GUIComponent>.Default.GetHashCode(this.<MainElement>k__BackingField) * -1521134295 + EqualityComparer<ImmutableArray<GUIComponent>>.Default.GetHashCode(this.<AllCreatedElements>k__BackingField);
			}

			// Token: 0x06007281 RID: 29313 RVA: 0x0036D0FD File Offset: 0x0036B2FD
			[NullableContext(0)]
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is MedicalClinicUI.CreatedPopupAfflictionElement && this.Equals((MedicalClinicUI.CreatedPopupAfflictionElement)obj);
			}

			// Token: 0x06007282 RID: 29314 RVA: 0x0036D115 File Offset: 0x0036B315
			[CompilerGenerated]
			public bool Equals(MedicalClinicUI.CreatedPopupAfflictionElement other)
			{
				return EqualityComparer<GUIComponent>.Default.Equals(this.<MainElement>k__BackingField, other.<MainElement>k__BackingField) && EqualityComparer<ImmutableArray<GUIComponent>>.Default.Equals(this.<AllCreatedElements>k__BackingField, other.<AllCreatedElements>k__BackingField);
			}

			// Token: 0x06007283 RID: 29315 RVA: 0x0036D147 File Offset: 0x0036B347
			[CompilerGenerated]
			public void Deconstruct(out GUIComponent MainElement, [Nullable(new byte[]
			{
				0,
				1
			})] out ImmutableArray<GUIComponent> AllCreatedElements)
			{
				MainElement = this.MainElement;
				AllCreatedElements = this.AllCreatedElements;
			}
		}

		// Token: 0x020009A5 RID: 2469
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x040041DF RID: 16863
			[Nullable(0)]
			public static Func<Affliction, bool> <0>__IsHealable;
		}
	}
}
