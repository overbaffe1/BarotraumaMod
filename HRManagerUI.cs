using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x020000A7 RID: 167
	internal class HRManagerUI
	{
		// Token: 0x17000575 RID: 1397
		// (get) Token: 0x06001530 RID: 5424 RVA: 0x000C54C0 File Offset: 0x000C36C0
		private CampaignMode campaign
		{
			get
			{
				return this.campaignUI.Campaign;
			}
		}

		// Token: 0x17000576 RID: 1398
		// (get) Token: 0x06001531 RID: 5425 RVA: 0x000C54CD File Offset: 0x000C36CD
		private List<CharacterInfo> PendingHires
		{
			get
			{
				Map map = this.campaign.Map;
				if (map == null)
				{
					return null;
				}
				Location currentLocation = map.CurrentLocation;
				if (currentLocation == null)
				{
					return null;
				}
				HireManager hireManager = currentLocation.HireManager;
				if (hireManager == null)
				{
					return null;
				}
				return hireManager.PendingHires;
			}
		}

		// Token: 0x17000577 RID: 1399
		// (get) Token: 0x06001532 RID: 5426 RVA: 0x000C54FC File Offset: 0x000C36FC
		private static bool ReplacingPermanentlyDeadCharacter
		{
			get
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				ServerSettings serverSettings = (networkMember != null) ? networkMember.ServerSettings : null;
				if (serverSettings != null && serverSettings.RespawnMode == RespawnMode.Permadeath && !serverSettings.IronmanMode)
				{
					GameClient client = GameMain.Client;
					CharacterInfo characterInfo = (client != null) ? client.CharacterInfo : null;
					return characterInfo != null && characterInfo.PermanentlyDead;
				}
				return false;
			}
		}

		// Token: 0x17000578 RID: 1400
		// (get) Token: 0x06001533 RID: 5427 RVA: 0x000C554E File Offset: 0x000C374E
		private static bool ReserveBenchEnabled
		{
			get
			{
				GameSession gameSession = GameMain.GameSession;
				return ((gameSession != null) ? gameSession.Campaign : null) is MultiPlayerCampaign;
			}
		}

		// Token: 0x17000579 RID: 1401
		// (get) Token: 0x06001534 RID: 5428 RVA: 0x000C556C File Offset: 0x000C376C
		private static bool HasPermissionToHire
		{
			get
			{
				if (!HRManagerUI.ReplacingPermanentlyDeadCharacter)
				{
					return CampaignMode.AllowedToManageCampaign(ClientPermissions.ManageHires);
				}
				NetworkMember networkMember = GameMain.NetworkMember;
				return (networkMember != null && networkMember.ServerSettings.ReplaceCostPercentage <= 0f) || CampaignMode.AllowedToManageCampaign(ClientPermissions.ManageMoney) || CampaignMode.AllowedToManageCampaign(ClientPermissions.ManageHires);
			}
		}

		// Token: 0x06001535 RID: 5429 RVA: 0x000C55C8 File Offset: 0x000C37C8
		public HRManagerUI(CampaignUI campaignUI, GUIComponent parentComponent)
		{
			this.campaignUI = campaignUI;
			this.parentComponent = parentComponent;
			this.CreateUI();
			this.UpdateLocationView(campaignUI.Campaign.Map.CurrentLocation, true, null);
			campaignUI.Campaign.Map.OnLocationChanged.RegisterOverwriteExisting("CrewManagement.UpdateLocationView".ToIdentifier(), delegate(Map.LocationChangeInfo locationChangeInfo)
			{
				this.UpdateLocationView(locationChangeInfo.NewLocation, true, locationChangeInfo.PrevLocation);
			});
			Reputation.OnAnyReputationValueChanged.RegisterOverwriteExisting("CrewManagement.UpdateLocationView".ToIdentifier(), delegate(Reputation _)
			{
				this.needsHireableRefresh = true;
			});
			this.hadPermissionToHire = HRManagerUI.HasPermissionToHire;
			this.wasReplacingPermanentlyDeadCharacter = HRManagerUI.ReplacingPermanentlyDeadCharacter;
		}

		// Token: 0x06001536 RID: 5430 RVA: 0x000C5668 File Offset: 0x000C3868
		public void RefreshUI()
		{
			this.RefreshCrewFrames(this.hireableList);
			this.RefreshCrewFrames(this.crewList);
			this.RefreshCrewFrames(this.pendingList);
			if (this.clearAllButton != null)
			{
				this.clearAllButton.Enabled = HRManagerUI.HasPermissionToHire;
			}
			this.hadPermissionToHire = HRManagerUI.HasPermissionToHire;
			this.wasReplacingPermanentlyDeadCharacter = HRManagerUI.ReplacingPermanentlyDeadCharacter;
		}

		// Token: 0x06001537 RID: 5431 RVA: 0x000C56C8 File Offset: 0x000C38C8
		private void RefreshCrewFrames(GUIListBox listBox)
		{
			if (listBox == null)
			{
				return;
			}
			listBox.CanBeFocused = HRManagerUI.HasPermissionToHire;
			foreach (GUIComponent child in listBox.Content.Children)
			{
				GUIButton buyButton = child.FindChild((GUIComponent c) => c is GUIButton && c.UserData is CharacterInfo, true) as GUIButton;
				if (buyButton != null)
				{
					CharacterInfo characterInfo = buyButton.UserData as CharacterInfo;
					buyButton.Enabled = (!HRManagerUI.ReplacingPermanentlyDeadCharacter && HRManagerUI.HasPermissionToHire && this.EnoughReputationToHire(characterInfo) && this.campaign.CanAffordNewCharacter(characterInfo));
					foreach (GUITextBlock text in child.GetAllChildren<GUITextBlock>())
					{
						text.TextColor = new Color(text.TextColor, buyButton.Enabled ? 1f : 0.6f);
					}
				}
			}
		}

		// Token: 0x06001538 RID: 5432 RVA: 0x000C57F4 File Offset: 0x000C39F4
		private void CreateUI()
		{
			GUIComponent glowChild = this.parentComponent.FindChild((GUIComponent c) => c.UserData as string == "glow", false);
			if (glowChild != null)
			{
				this.parentComponent.RemoveChild(glowChild);
			}
			GUIComponent containerChild = this.parentComponent.FindChild((GUIComponent c) => c.UserData as string == "container", false);
			if (containerChild != null)
			{
				this.parentComponent.RemoveChild(containerChild);
			}
			new GUIFrame(new RectTransform(new Vector2(1.25f, 1.25f), this.parentComponent.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "OuterGlow", new Color?(Color.Black * 0.7f)).UserData = "glow";
			GUIFrame guiframe = new GUIFrame(new RectTransform(new Vector2(0.95f), this.parentComponent.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), null, null);
			guiframe.CanBeFocused = false;
			guiframe.UserData = "container";
			int panelMaxWidth = (int)(GUI.xScale * (float)((GUI.HorizontalAspectRatio < 1.4f) ? 650 : 560));
			GUILayoutGroup availableMainGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.4f, 1f), this.campaignUI.GetTabContainer(CampaignMode.InteractionType.Crew).RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MaxSize = new Point(panelMaxWidth, this.campaignUI.GetTabContainer(CampaignMode.InteractionType.Crew).Rect.Height)
			}, false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.02f
			};
			GUILayoutGroup headerGroup = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.05357143f), availableMainGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				RelativeSpacing = 0.005f
			};
			float imageWidth = (float)headerGroup.Rect.Height / (float)headerGroup.Rect.Width;
			new GUIImage(new RectTransform(new Vector2(imageWidth, 1f), headerGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "CrewManagementHeaderIcon", GUIImage.ScalingMode.None);
			RectTransform rectT = new RectTransform(new Vector2(1f - imageWidth, 1f), headerGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = TextManager.Get("campaigncrew.header");
			GUIFont font = GUIStyle.LargeFont;
			GUITextBlock guitextBlock = new GUITextBlock(rectT, text, null, font, Alignment.Left, false, "", null);
			guitextBlock.CanBeFocused = false;
			guitextBlock.ForceUpperCase = ForceUpperCase.Yes;
			GUILayoutGroup hireablesGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.95f), new GUIFrame(new RectTransform(new Vector2(1f, 0.9464286f), availableMainGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null).RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.015f,
				Stretch = true
			};
			GUILayoutGroup sortGroup = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.04f), hireablesGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				RelativeSpacing = 0.015f,
				Stretch = true
			};
			new GUITextBlock(new RectTransform(new Vector2(0.5f, 1f), sortGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("campaignstore.sortby"), null, null, Alignment.Left, false, "", null);
			this.sortingDropDown = new GUIDropDown(new RectTransform(new Vector2(0.5f, 1f), sortGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, 5, "", false, false, Alignment.CenterLeft, 1f)
			{
				OnSelected = delegate(GUIComponent child, object userData)
				{
					this.SortCharacters(this.hireableList, (HRManagerUI.SortingMethod)userData);
					return true;
				}
			};
			string tag = "sortingmethod.";
			this.sortingDropDown.AddItem(TextManager.Get(tag + HRManagerUI.SortingMethod.JobAsc.ToString()), HRManagerUI.SortingMethod.JobAsc, null, null, null);
			this.sortingDropDown.AddItem(TextManager.Get(tag + HRManagerUI.SortingMethod.SkillAsc.ToString()), HRManagerUI.SortingMethod.SkillAsc, null, null, null);
			this.sortingDropDown.AddItem(TextManager.Get(tag + HRManagerUI.SortingMethod.SkillDesc.ToString()), HRManagerUI.SortingMethod.SkillDesc, null, null, null);
			this.sortingDropDown.AddItem(TextManager.Get(tag + HRManagerUI.SortingMethod.PriceAsc.ToString()), HRManagerUI.SortingMethod.PriceAsc, null, null, null);
			this.sortingDropDown.AddItem(TextManager.Get(tag + HRManagerUI.SortingMethod.PriceDesc.ToString()), HRManagerUI.SortingMethod.PriceDesc, null, null, null);
			this.hireableList = new GUIListBox(new RectTransform(new Vector2(1f, 0.96f), hireablesGroup.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				Spacing = 1
			};
			GUILayoutGroup pendingAndCrewMainGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.4f, 1f), this.campaignUI.GetTabContainer(CampaignMode.InteractionType.Crew).RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal)
			{
				MaxSize = new Point(panelMaxWidth, this.campaignUI.GetTabContainer(CampaignMode.InteractionType.Crew).Rect.Height)
			}, false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.02f
			};
			this.playerBalanceElement = CampaignUI.AddBalanceElement(pendingAndCrewMainGroup, new Vector2(1f, 0.05357143f));
			this.pendingAndCrewPanel = new GUIFrame(new RectTransform(new Vector2(1f, 0.9464286f), pendingAndCrewMainGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MaxSize = new Point(panelMaxWidth, this.campaignUI.GetTabContainer(CampaignMode.InteractionType.Crew).Rect.Height)
			}, "", null);
			GUILayoutGroup pendingAndCrewGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.95f), this.pendingAndCrewPanel.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			float height = 0.05f;
			RectTransform rectT2 = new RectTransform(new Vector2(1f, height), pendingAndCrewGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = TextManager.Get("campaigncrew.pending");
			font = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT2, text2, null, font, Alignment.Left, false, "", null);
			this.pendingList = new GUIListBox(new RectTransform(new Vector2(1f, 8f * height), pendingAndCrewGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				Spacing = 1
			};
			RectTransform rectT3 = new RectTransform(new Vector2(1f, height), pendingAndCrewGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text3 = TextManager.Get("campaignmenucrew");
			font = GUIStyle.SubHeadingFont;
			GUITextBlock crewHeader = new GUITextBlock(rectT3, text3, null, font, Alignment.Left, false, "", null);
			new GUITextBlock(new RectTransform(new Vector2(0.5f, 1f), crewHeader.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), string.Empty, null, null, Alignment.CenterRight, false, "", null).TextGetter = delegate()
			{
				CampaignMode campaign = this.campaign;
				int? num;
				if (campaign == null)
				{
					num = null;
				}
				else
				{
					CrewManager crewManager = campaign.CrewManager;
					if (crewManager == null)
					{
						num = null;
					}
					else
					{
						IEnumerable<CharacterInfo> characterInfos = crewManager.GetCharacterInfos(false);
						num = ((characterInfos != null) ? new int?(characterInfos.Count<CharacterInfo>()) : null);
					}
				}
				int? num2 = num;
				int crewSize = num2.GetValueOrDefault();
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
				defaultInterpolatedStringHandler.AppendFormatted<int>(crewSize);
				defaultInterpolatedStringHandler.AppendLiteral("/");
				defaultInterpolatedStringHandler.AppendFormatted<int>(16);
				return defaultInterpolatedStringHandler.ToStringAndClear();
			};
			this.crewList = new GUIListBox(new RectTransform(new Vector2(1f, 8f * height), pendingAndCrewGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				Spacing = 1
			};
			GUILayoutGroup group = new GUILayoutGroup(new RectTransform(new Vector2(1f, height), pendingAndCrewGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			new GUITextBlock(new RectTransform(new Vector2(0.5f, 1f), group.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("campaignstore.total"), null, null, Alignment.Left, false, "", null);
			RectTransform rectT4 = new RectTransform(new Vector2(0.5f, 1f), group.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text4 = "";
			font = GUIStyle.SubHeadingFont;
			this.totalBlock = new GUITextBlock(rectT4, text4, null, font, Alignment.Right, false, "", null)
			{
				TextScale = 1.1f
			};
			group = new GUILayoutGroup(new RectTransform(new Vector2(1f, height), pendingAndCrewGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopRight)
			{
				RelativeSpacing = 0.01f
			};
			this.validateHiresButton = new GUIButton(new RectTransform(new Vector2(0.4f, 1f), group.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("campaigncrew.validate"), Alignment.Center, "", null)
			{
				ClickSound = GUISoundType.ConfirmTransaction,
				ForceUpperCase = ForceUpperCase.Yes,
				OnClicked = ((GUIButton b, object o) => this.ValidateHires(this.PendingHires, true, true, true))
			};
			this.clearAllButton = new GUIButton(new RectTransform(new Vector2(0.4f, 1f), group.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("campaignstore.clearall"), Alignment.Center, "", null)
			{
				ClickSound = GUISoundType.Cart,
				ForceUpperCase = ForceUpperCase.Yes,
				Enabled = HRManagerUI.HasPermissionToHire,
				OnClicked = ((GUIButton b, object o) => this.RemoveAllPendingHires(true))
			};
			GUITextBlock.AutoScaleAndNormalize(new GUITextBlock[]
			{
				this.validateHiresButton.TextBlock,
				this.clearAllButton.TextBlock
			});
			this.resolutionWhenCreated = new Point(GameMain.GraphicsWidth, GameMain.GraphicsHeight);
		}

		// Token: 0x06001539 RID: 5433 RVA: 0x000C64D4 File Offset: 0x000C46D4
		private void UpdateLocationView(Location location, bool removePending, Location prevLocation = null)
		{
			if (prevLocation != null && prevLocation == location && GameMain.NetworkMember != null)
			{
				return;
			}
			if (this.characterPreviewFrame != null)
			{
				GUIComponent parent = this.characterPreviewFrame.Parent;
				if (parent != null)
				{
					parent.RemoveChild(this.characterPreviewFrame);
				}
				this.characterPreviewFrame = null;
			}
			this.UpdateHireables(location);
			if (this.pendingList != null)
			{
				if (removePending)
				{
					List<CharacterInfo> pendingHires = this.PendingHires;
					if (pendingHires != null)
					{
						pendingHires.Clear();
					}
					this.pendingList.Content.ClearChildren();
				}
				else
				{
					List<CharacterInfo> pendingHires2 = this.PendingHires;
					if (pendingHires2 != null)
					{
						pendingHires2.ForEach(delegate(CharacterInfo ci)
						{
							this.AddPendingHire(ci, true, false);
						});
					}
				}
				this.SetTotalHireCost();
			}
			this.UpdateCrew();
		}

		// Token: 0x0600153A RID: 5434 RVA: 0x000C657C File Offset: 0x000C477C
		public void RefreshHRView()
		{
			CampaignMode campaign = this.campaign;
			Location currentLocation = (campaign != null) ? campaign.CurrentLocation : null;
			if (currentLocation == null)
			{
				return;
			}
			if (this.characterPreviewFrame != null)
			{
				GUIComponent parent = this.characterPreviewFrame.Parent;
				if (parent != null)
				{
					parent.RemoveChild(this.characterPreviewFrame);
				}
				this.characterPreviewFrame = null;
			}
			this.UpdateHireables(currentLocation);
			if (this.pendingList != null)
			{
				this.pendingList.Content.ClearChildren();
				List<CharacterInfo> pendingHires = this.PendingHires;
				if (pendingHires != null)
				{
					pendingHires.ForEach(delegate(CharacterInfo ci)
					{
						this.AddPendingHire(ci, false, false);
					});
				}
				this.SetTotalHireCost();
			}
			this.UpdateCrew();
		}

		// Token: 0x0600153B RID: 5435 RVA: 0x000C6613 File Offset: 0x000C4813
		public void UpdateHireables()
		{
			CampaignMode campaign = this.campaign;
			this.UpdateHireables((campaign != null) ? campaign.CurrentLocation : null);
		}

		// Token: 0x0600153C RID: 5436 RVA: 0x000C6630 File Offset: 0x000C4830
		private void UpdateHireables(Location location)
		{
			if (this.hireableList == null)
			{
				return;
			}
			this.hireableList.Content.Children.ToList<GUIComponent>().ForEach(delegate(GUIComponent c)
			{
				this.hireableList.RemoveChild(c);
			});
			IEnumerable<CharacterInfo> hireableCharacters = location.GetHireableCharacters();
			if (hireableCharacters.None(null))
			{
				new GUITextBlock(new RectTransform(new Vector2(1f, 0.2f), this.hireableList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("HireUnavailable"), null, null, Alignment.Center, false, "", null).CanBeFocused = false;
			}
			else
			{
				foreach (CharacterInfo c2 in hireableCharacters)
				{
					if (c2 != null && !this.PendingHires.Contains(c2))
					{
						this.CreateCharacterFrame(c2, this.hireableList, false);
					}
				}
			}
			this.sortingDropDown.SelectItem(HRManagerUI.SortingMethod.JobAsc);
			this.hireableList.UpdateScrollBarSize();
		}

		// Token: 0x0600153D RID: 5437 RVA: 0x000C6770 File Offset: 0x000C4970
		public void SetHireables(Location location, List<CharacterInfo> availableHires)
		{
			HireManager hireManager = location.HireManager;
			if (hireManager == null)
			{
				return;
			}
			int hireVal = hireManager.AvailableCharacters.Aggregate(0, (int curr, CharacterInfo hire) => curr + (int)hire.ID);
			int newVal = availableHires.Aggregate(0, (int curr, CharacterInfo hire) => curr + (int)hire.ID);
			if (hireVal != newVal)
			{
				location.HireManager.AvailableCharacters = availableHires;
				this.UpdateHireables(location);
			}
		}

		// Token: 0x0600153E RID: 5438 RVA: 0x000C67F4 File Offset: 0x000C49F4
		public void UpdateCrew()
		{
			this.crewList.Content.Children.ToList<GUIComponent>().ForEach(delegate(GUIComponent c)
			{
				this.crewList.Content.RemoveChild(c);
			});
			foreach (CharacterInfo ci in GameMain.GameSession.CrewManager.GetCharacterInfos(true))
			{
				if (ci.Character == null || (!ci.Character.IsRemotePlayer && ci.Character.IsBot))
				{
					this.CreateCharacterFrame(ci, this.crewList, false);
				}
			}
			this.SortCharacters(this.crewList, HRManagerUI.SortingMethod.JobAsc);
			this.crewList.UpdateScrollBarSize();
		}

		// Token: 0x0600153F RID: 5439 RVA: 0x000C68B4 File Offset: 0x000C4AB4
		private void SortCharacters(GUIListBox list, HRManagerUI.SortingMethod sortingMethod)
		{
			if (sortingMethod == HRManagerUI.SortingMethod.AlphabeticalAsc)
			{
				list.Content.RectTransform.SortChildren(delegate(RectTransform x, RectTransform y)
				{
					int? num = this.<SortCharacters>g__CompareReputationRequirement|37_5(x.GUIComponent, y.GUIComponent);
					if (num == null)
					{
						return ((HRManagerUI.InfoSkill)x.GUIComponent.UserData).CharacterInfo.Name.CompareTo(((HRManagerUI.InfoSkill)y.GUIComponent.UserData).CharacterInfo.Name);
					}
					return num.GetValueOrDefault();
				});
			}
			else if (sortingMethod == HRManagerUI.SortingMethod.JobAsc)
			{
				this.SortCharacters(list, HRManagerUI.SortingMethod.AlphabeticalAsc);
				list.Content.RectTransform.SortChildren(delegate(RectTransform x, RectTransform y)
				{
					int? num = this.<SortCharacters>g__CompareReputationRequirement|37_5(x.GUIComponent, y.GUIComponent);
					if (num == null)
					{
						return string.Compare(((HRManagerUI.InfoSkill)x.GUIComponent.UserData).CharacterInfo.Job.Name.Value, ((HRManagerUI.InfoSkill)y.GUIComponent.UserData).CharacterInfo.Job.Name.Value, StringComparison.Ordinal);
					}
					return num.GetValueOrDefault();
				});
			}
			else if (sortingMethod == HRManagerUI.SortingMethod.PriceAsc || sortingMethod == HRManagerUI.SortingMethod.PriceDesc)
			{
				this.SortCharacters(list, HRManagerUI.SortingMethod.AlphabeticalAsc);
				list.Content.RectTransform.SortChildren(delegate(RectTransform x, RectTransform y)
				{
					int? num = this.<SortCharacters>g__CompareReputationRequirement|37_5(x.GUIComponent, y.GUIComponent);
					if (num == null)
					{
						return ((HRManagerUI.InfoSkill)x.GUIComponent.UserData).CharacterInfo.Salary.CompareTo(((HRManagerUI.InfoSkill)y.GUIComponent.UserData).CharacterInfo.Salary);
					}
					return num.GetValueOrDefault();
				});
				if (sortingMethod == HRManagerUI.SortingMethod.PriceDesc)
				{
					list.Content.RectTransform.ReverseChildren();
				}
			}
			else if (sortingMethod == HRManagerUI.SortingMethod.SkillAsc || sortingMethod == HRManagerUI.SortingMethod.SkillDesc)
			{
				this.SortCharacters(list, HRManagerUI.SortingMethod.AlphabeticalAsc);
				list.Content.RectTransform.SortChildren(delegate(RectTransform x, RectTransform y)
				{
					int? num = this.<SortCharacters>g__CompareReputationRequirement|37_5(x.GUIComponent, y.GUIComponent);
					if (num == null)
					{
						return ((HRManagerUI.InfoSkill)x.GUIComponent.UserData).SkillLevel.CompareTo(((HRManagerUI.InfoSkill)y.GUIComponent.UserData).SkillLevel);
					}
					return num.GetValueOrDefault();
				});
				if (sortingMethod == HRManagerUI.SortingMethod.SkillDesc)
				{
					list.Content.RectTransform.ReverseChildren();
				}
			}
			list.Content.RectTransform.SortChildren((RectTransform x, RectTransform y) => ((HRManagerUI.InfoSkill)x.GUIComponent.UserData).CharacterInfo.BotStatus.CompareTo(((HRManagerUI.InfoSkill)y.GUIComponent.UserData).CharacterInfo.BotStatus));
		}

		// Token: 0x06001540 RID: 5440 RVA: 0x000C69C4 File Offset: 0x000C4BC4
		public GUIComponent CreateCharacterFrame(CharacterInfo characterInfo, GUIListBox listBox, bool hideSalary = false)
		{
			HRManagerUI.<>c__DisplayClass39_0 CS$<>8__locals1 = new HRManagerUI.<>c__DisplayClass39_0();
			CS$<>8__locals1.characterInfo = characterInfo;
			CS$<>8__locals1.<>4__this = this;
			string characterName = (listBox == this.hireableList) ? CS$<>8__locals1.characterInfo.OriginalName : CS$<>8__locals1.characterInfo.Name;
			Skill skill = null;
			Color? jobColor = null;
			if (CS$<>8__locals1.characterInfo.Job != null)
			{
				Job job = CS$<>8__locals1.characterInfo.Job;
				Skill skill2;
				if ((skill2 = ((job != null) ? job.PrimarySkill : null)) == null)
				{
					skill2 = (from s in CS$<>8__locals1.characterInfo.Job.GetSkills()
					orderby s.Level descending
					select s).FirstOrDefault<Skill>();
				}
				skill = skill2;
				jobColor = new Color?(CS$<>8__locals1.characterInfo.Job.Prefab.UIColor);
			}
			GUIFrame frame = new GUIFrame(new RectTransform(new Point(listBox.Content.Rect.Width, (int)(GUI.yScale * 55f)), listBox.Content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), "ListBoxElement", null)
			{
				UserData = new HRManagerUI.InfoSkill(CS$<>8__locals1.characterInfo, (skill != null) ? skill.Level : 0f)
			};
			GUILayoutGroup mainGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.95f), frame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				AbsoluteSpacing = 1,
				Stretch = true
			};
			float portraitWidth = 0.8f * (float)mainGroup.Rect.Height / (float)mainGroup.Rect.Width;
			GUICustomComponent icon = new GUICustomComponent(new RectTransform(new Vector2(portraitWidth, 0.8f), mainGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), delegate(SpriteBatch sb, GUICustomComponent component)
			{
				CS$<>8__locals1.characterInfo.DrawIcon(sb, component.Rect.Center.ToVector2(), component.Rect.Size.ToVector2(), false);
			}, null)
			{
				CanBeFocused = false
			};
			GUILayoutGroup nameAndJobGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.4f - portraitWidth, 0.8f), mainGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				CanBeFocused = false
			};
			GUILayoutGroup nameGroup = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.5f), nameAndJobGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				CanBeFocused = false
			};
			GUITextBlock nameBlock = new GUITextBlock(new RectTransform(Vector2.One, nameGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), characterName, jobColor, null, Alignment.BottomLeft, false, "", null)
			{
				CanBeFocused = false
			};
			GUITextBlock jobBlock = new GUITextBlock(new RectTransform(new Vector2(1f, 0.5f), nameAndJobGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), CS$<>8__locals1.characterInfo.Title ?? CS$<>8__locals1.characterInfo.Job.Name, new Color?(Color.White), GUIStyle.SmallFont, Alignment.TopLeft, false, "", null)
			{
				CanBeFocused = false
			};
			if (!CS$<>8__locals1.characterInfo.MinReputationToHire.Item1.IsEmpty)
			{
				Faction faction = this.campaign.Factions.Find((Faction f) => f.Prefab.Identifier == CS$<>8__locals1.characterInfo.MinReputationToHire.Item1);
				if (faction != null)
				{
					jobBlock.TextColor = faction.Prefab.IconColor;
				}
			}
			RichString fullJobText = jobBlock.Text;
			if (CS$<>8__locals1.characterInfo.Job != null && skill != null)
			{
				GUILayoutGroup skillGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.14f, 0.6f), mainGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
				float iconWidth = (float)skillGroup.Rect.Height / (float)skillGroup.Rect.Width;
				GUITextBlock guitextBlock = new GUITextBlock(new RectTransform(new Vector2(1f - iconWidth, 1f), skillGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), ((int)skill.Level).ToString(), null, null, Alignment.CenterRight, false, "", null);
				guitextBlock.Padding = Vector4.Zero;
				guitextBlock.CanBeFocused = false;
				GUIImage skillIcon = new GUIImage(new RectTransform(Vector2.One, skillGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Smallest), skill.Icon, true, null)
				{
					CanBeFocused = false
				};
				if (jobColor != null)
				{
					skillIcon.Color = jobColor.Value;
				}
			}
			if (!hideSalary)
			{
				if (listBox != this.crewList)
				{
					new GUITextBlock(new RectTransform(new Vector2(0.2f, 1f), mainGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.FormatCurrency(HRManagerUI.ReplacingPermanentlyDeadCharacter ? this.campaign.NewCharacterCost(CS$<>8__locals1.characterInfo) : HireManager.GetSalaryFor(CS$<>8__locals1.characterInfo), true), null, null, Alignment.Center, false, "", null).CanBeFocused = false;
				}
				else
				{
					new GUIFrame(new RectTransform(new Vector2(0.2f, 1f), mainGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null).CanBeFocused = false;
				}
			}
			if (listBox == this.hireableList)
			{
				GUIButton hireButton = new GUIButton(new RectTransform(new Vector2(0.12f, 0.9f), mainGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.Center, "CrewManagementAddButton", null)
				{
					ToolTip = TextManager.Get(HRManagerUI.ReserveBenchEnabled ? "hirebutton.crew" : "hirebutton"),
					ClickSound = GUISoundType.Cart,
					UserData = CS$<>8__locals1.characterInfo,
					Enabled = (CS$<>8__locals1.<CreateCharacterFrame>g__CanHire|1(CS$<>8__locals1.characterInfo) && !HRManagerUI.ReplacingPermanentlyDeadCharacter),
					OnClicked = delegate(GUIButton b, object o)
					{
						CharacterInfo currentCharacterInfo = (CharacterInfo)o;
						currentCharacterInfo.BotStatus = BotStatus.PendingHireToActiveService;
						return CS$<>8__locals1.<>4__this.AddPendingHire(currentCharacterInfo, true, true);
					}
				};
				GUIButton guibutton = hireButton;
				guibutton.OnAddedToGUIUpdateList = (Action<GUIComponent>)Delegate.Combine(guibutton.OnAddedToGUIUpdateList, new Action<GUIComponent>(delegate(GUIComponent btn)
				{
					if (HRManagerUI.ReplacingPermanentlyDeadCharacter)
					{
						return;
					}
					if (CS$<>8__locals1.<>4__this.PendingHires.Count((CharacterInfo ci) => ci.BotStatus == BotStatus.PendingHireToActiveService) + CS$<>8__locals1.<>4__this.campaign.CrewManager.GetCharacterInfos(false).Count<CharacterInfo>() >= 16)
					{
						if (btn.Enabled)
						{
							btn.ToolTip = TextManager.Get("canthiremorecharacters");
							btn.Enabled = false;
							return;
						}
					}
					else if (!btn.Enabled)
					{
						btn.ToolTip = string.Empty;
						btn.Enabled = base.<CreateCharacterFrame>g__CanHire|1(CS$<>8__locals1.characterInfo);
					}
				}));
				if (HRManagerUI.ReplacingPermanentlyDeadCharacter)
				{
					bool canHire = CS$<>8__locals1.<CreateCharacterFrame>g__CanHire|1(CS$<>8__locals1.characterInfo) && this.campaign.CanAffordNewCharacter(CS$<>8__locals1.characterInfo);
					GUIButton takeoverButton = new GUIButton(new RectTransform(new Vector2(0.12f, 0.9f), mainGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.Center, "CrewManagementTakeControlButton", null)
					{
						ToolTip = (canHire ? TextManager.Get("hireandtakecontrol") : TextManager.Get("hireandtakecontroldisabled")),
						ClickSound = GUISoundType.ConfirmTransaction,
						UserData = CS$<>8__locals1.characterInfo,
						Enabled = canHire,
						OnClicked = delegate(GUIButton b, object o)
						{
							GameClient gameClient = GameMain.Client;
							if (gameClient == null)
							{
								return false;
							}
							Client client = gameClient.ConnectedClients.FirstOrDefault((Client c) => c.SessionId == gameClient.SessionId);
							if (!CS$<>8__locals1.<>4__this.campaign.TryPurchase(client, CS$<>8__locals1.<>4__this.campaign.NewCharacterCost(CS$<>8__locals1.characterInfo)))
							{
								return false;
							}
							gameClient.SendTakeOverBotRequest(CS$<>8__locals1.characterInfo);
							CS$<>8__locals1.<>4__this.needsHireableRefresh = true;
							CS$<>8__locals1.<>4__this.campaign.ShowCampaignUI = false;
							return true;
						}
					};
					GUIButton guibutton2 = takeoverButton;
					guibutton2.OnAddedToGUIUpdateList = (Action<GUIComponent>)Delegate.Combine(guibutton2.OnAddedToGUIUpdateList, new Action<GUIComponent>(delegate(GUIComponent btn)
					{
						bool canHireCurrently = HRManagerUI.ReplacingPermanentlyDeadCharacter && base.<CreateCharacterFrame>g__CanHire|1(CS$<>8__locals1.characterInfo) && CS$<>8__locals1.<>4__this.campaign.CanAffordNewCharacter(CS$<>8__locals1.characterInfo);
						btn.ToolTip = TextManager.Get(canHireCurrently ? "hireandtakecontrol" : "hireandtakecontroldisabled");
						GameSession gameSession = GameMain.GameSession;
						btn.Visible = (gameSession != null && gameSession.AllowHrManagerBotTakeover);
						btn.Enabled = canHireCurrently;
					}));
				}
				if (HRManagerUI.ReserveBenchEnabled && !HRManagerUI.ReplacingPermanentlyDeadCharacter)
				{
					GUIButton hireToReserveBenchButton = new GUIButton(new RectTransform(new Vector2(0.12f, 0.9f), mainGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.Center, "CrewManagementAddAsReserveButton", null)
					{
						ToolTip = TextManager.Get("hirebutton.reservebench"),
						ClickSound = GUISoundType.Cart,
						UserData = CS$<>8__locals1.characterInfo,
						Enabled = CS$<>8__locals1.<CreateCharacterFrame>g__CanHire|1(CS$<>8__locals1.characterInfo),
						OnClicked = delegate(GUIButton b, object o)
						{
							CharacterInfo currentCharacterInfo = (CharacterInfo)o;
							currentCharacterInfo.BotStatus = BotStatus.PendingHireToReserveBench;
							return CS$<>8__locals1.<>4__this.AddPendingHire(currentCharacterInfo, false, true);
						}
					};
					GUIButton guibutton3 = hireToReserveBenchButton;
					guibutton3.OnAddedToGUIUpdateList = (Action<GUIComponent>)Delegate.Combine(guibutton3.OnAddedToGUIUpdateList, new Action<GUIComponent>(delegate(GUIComponent btn)
					{
						btn.Visible = HRManagerUI.ReserveBenchEnabled;
						btn.Enabled = (base.<CreateCharacterFrame>g__CanHire|1(CS$<>8__locals1.characterInfo) && !HRManagerUI.ReplacingPermanentlyDeadCharacter);
					}));
				}
			}
			else if (listBox == this.pendingList)
			{
				if (HRManagerUI.ReserveBenchEnabled && !HRManagerUI.ReplacingPermanentlyDeadCharacter)
				{
					GUIButton guibutton4 = new GUIButton(new RectTransform(new Vector2(0.12f, 0.9f), mainGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.Center, (CS$<>8__locals1.characterInfo.BotStatus == BotStatus.PendingHireToActiveService) ? "CrewManagementReserveBenchButtonActive" : "CrewManagementReserveBenchButtonReserve", null);
					guibutton4.UserData = CS$<>8__locals1.characterInfo;
					guibutton4.ToolTip = TextManager.Get((CS$<>8__locals1.characterInfo.BotStatus == BotStatus.PendingHireToActiveService) ? "ReserveBenchTogglePendingHire.Active" : "ReserveBenchTogglePendingHire.Reserve");
					guibutton4.Enabled = (CS$<>8__locals1.<CreateCharacterFrame>g__CanHire|1(CS$<>8__locals1.characterInfo) && (CS$<>8__locals1.characterInfo.BotStatus == BotStatus.PendingHireToActiveService || !this.ActiveServiceFull()));
					guibutton4.OnClicked = delegate(GUIButton btn, object obj)
					{
						CS$<>8__locals1.<>4__this.SelectCharacter(null, null, null);
						CharacterInfo currentCharacterInfo = (CharacterInfo)obj;
						GameClient client = GameMain.Client;
						if (client != null)
						{
							client.ToggleReserveBench(currentCharacterInfo, true);
						}
						return true;
					};
				}
				GUIButton guibutton5 = new GUIButton(new RectTransform(new Vector2(0.12f, 0.9f), mainGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.Center, "CrewManagementRemoveButton", null);
				guibutton5.ClickSound = GUISoundType.Cart;
				guibutton5.UserData = CS$<>8__locals1.characterInfo;
				guibutton5.Enabled = CS$<>8__locals1.<CreateCharacterFrame>g__CanHire|1(CS$<>8__locals1.characterInfo);
				guibutton5.OnClicked = ((GUIButton b, object o) => CS$<>8__locals1.<>4__this.RemovePendingHire(o as CharacterInfo, true, true));
			}
			else if (listBox == this.crewList && this.campaign != null)
			{
				if (HRManagerUI.ReserveBenchEnabled && !HRManagerUI.ReplacingPermanentlyDeadCharacter)
				{
					GUIButton guibutton6 = new GUIButton(new RectTransform(new Vector2(0.12f, 0.9f), mainGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.Center, (CS$<>8__locals1.characterInfo.BotStatus == BotStatus.ActiveService) ? "CrewManagementReserveBenchButtonActive" : "CrewManagementReserveBenchButtonReserve", null);
					guibutton6.UserData = CS$<>8__locals1.characterInfo;
					guibutton6.ToolTip = TextManager.Get((CS$<>8__locals1.characterInfo.BotStatus == BotStatus.ActiveService) ? "ReserveBenchToggle.Active" : "ReserveBenchToggle.Reserve");
					guibutton6.Enabled = (CS$<>8__locals1.<CreateCharacterFrame>g__CanHire|1(CS$<>8__locals1.characterInfo) && (CS$<>8__locals1.characterInfo.BotStatus == BotStatus.ActiveService || !this.ActiveServiceFull()));
					guibutton6.OnClicked = delegate(GUIButton btn, object obj)
					{
						CS$<>8__locals1.<>4__this.SelectCharacter(null, null, null);
						CharacterInfo currentCharacterInfo = (CharacterInfo)obj;
						if (currentCharacterInfo.BotStatus == BotStatus.ActiveService && CS$<>8__locals1.characterInfo.Character != null)
						{
							GameMain.GameSession.CrewManager.RemoveCharacter(CS$<>8__locals1.characterInfo.Character, true, true);
						}
						GameClient client = GameMain.Client;
						if (client != null)
						{
							client.ToggleReserveBench(currentCharacterInfo, false);
						}
						return true;
					};
				}
				CrewManager cm = GameMain.GameSession.CrewManager;
				bool fireButtonEnabled = HRManagerUI.HasPermissionToHire && (CS$<>8__locals1.characterInfo.IsOnReserveBench || (cm.GetCharacterInfos(false).Contains(CS$<>8__locals1.characterInfo) && cm.GetCharacterInfos(false).Count<CharacterInfo>() > 1));
				GUIButton guibutton7 = new GUIButton(new RectTransform(new Vector2(0.12f, 0.9f), mainGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.Center, "CrewManagementFireButton", null);
				guibutton7.UserData = CS$<>8__locals1.characterInfo;
				guibutton7.Enabled = fireButtonEnabled;
				guibutton7.OnClicked = delegate(GUIButton btn, object obj)
				{
					GUIMessageBox confirmDialog = new GUIMessageBox(TextManager.Get("FireWarningHeader"), TextManager.GetWithVariable("FireWarningText", "[charactername]", ((CharacterInfo)obj).Name, FormatCapitals.No), new LocalizedString[]
					{
						TextManager.Get("Yes"),
						TextManager.Get("No")
					}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
					confirmDialog.Buttons[0].UserData = (CharacterInfo)obj;
					confirmDialog.Buttons[0].OnClicked = new GUIButton.OnClickedHandler(CS$<>8__locals1.<>4__this.FireCharacter);
					GUIButton guibutton9 = confirmDialog.Buttons[0];
					guibutton9.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton9.OnClicked, new GUIButton.OnClickedHandler(confirmDialog.Close));
					confirmDialog.Buttons[1].OnClicked = new GUIButton.OnClickedHandler(confirmDialog.Close);
					return true;
				};
			}
			else if (HRManagerUI.ReserveBenchEnabled && CS$<>8__locals1.characterInfo.IsOnReserveBench)
			{
				new GUIImage(new RectTransform(new Vector2(0.1f, 0.6f), mainGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "CrewManagementReserveBenchIconReserve", GUIImage.ScalingMode.None).ToolTip = TextManager.Get("ReserveBenchStatus.Reserve.WillSpawn");
			}
			else
			{
				new GUILayoutGroup(new RectTransform(new Vector2(0.1f, 0.6f), mainGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft).CanBeFocused = false;
			}
			if (listBox == this.pendingList || listBox == this.crewList)
			{
				bool canRename = (listBox == this.crewList) ? HRManagerUI.HasPermissionToHire : CS$<>8__locals1.<CreateCharacterFrame>g__CanHire|1(CS$<>8__locals1.characterInfo);
				if (canRename)
				{
					nameBlock.RectTransform.Resize(new Point(nameBlock.Rect.Width - nameBlock.Rect.Height, nameBlock.Rect.Height), true);
					nameBlock.Text = ToolBox.LimitString(characterName, nameBlock.Font, nameBlock.Rect.Width);
					nameBlock.RectTransform.Resize(new Point((int)(nameBlock.Padding.X + nameBlock.TextSize.X + nameBlock.Padding.Z), nameBlock.Rect.Height), true);
					Point iconSize = new Point((int)(0.7f * (float)nameBlock.Rect.Height));
					new GUIImage(new RectTransform(iconSize, nameGroup.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), "EditIcon", GUIImage.ScalingMode.None).CanBeFocused = false;
					Point buttonSize = new Point(3 * mainGroup.AbsoluteSpacing + icon.Rect.Width + nameAndJobGroup.Rect.Width + (int)((float)iconSize.X * 1.5f), mainGroup.Rect.Height);
					GUIButton guibutton8 = new GUIButton(new RectTransform(buttonSize, frame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false)
					{
						RelativeOffset = new Vector2(0.025f)
					}, Alignment.Center, null, null);
					guibutton8.ClampMouseRectToParent = false;
					guibutton8.ToolTip = TextManager.GetWithVariable("campaigncrew.givenicknametooltip", "[mouseprimary]", PlayerInput.PrimaryMouseLabel, FormatCapitals.No);
					guibutton8.UserData = CS$<>8__locals1.characterInfo;
					guibutton8.OnClicked = new GUIButton.OnClickedHandler(this.CreateRenamingComponent);
				}
			}
			mainGroup.Recalculate();
			nameBlock.Text = ToolBox.LimitString(characterName, nameBlock.Font, nameBlock.Rect.Width);
			jobBlock.Text = ToolBox.LimitString(fullJobText, jobBlock.Font, jobBlock.Rect.Width);
			if (jobBlock.Text != fullJobText)
			{
				jobBlock.ToolTip = fullJobText;
				jobBlock.CanBeFocused = true;
			}
			return frame;
		}

		// Token: 0x06001541 RID: 5441 RVA: 0x000C7938 File Offset: 0x000C5B38
		private bool ActiveServiceFull()
		{
			List<CharacterInfo> pendingHires = this.PendingHires;
			int num;
			if (pendingHires == null)
			{
				num = 0;
			}
			else
			{
				num = pendingHires.Count((CharacterInfo ci) => ci.BotStatus == BotStatus.PendingHireToActiveService);
			}
			int pendingHireCount = num;
			return pendingHireCount + this.campaign.CrewManager.GetCharacterInfos(false).Count<CharacterInfo>() >= 16;
		}

		// Token: 0x06001542 RID: 5442 RVA: 0x000C7998 File Offset: 0x000C5B98
		private bool EnoughReputationToHire(CharacterInfo characterInfo)
		{
			return !(characterInfo.MinReputationToHire.Item1 != Identifier.Empty) || MathF.Round(this.campaign.GetReputation(characterInfo.MinReputationToHire.Item1)) >= characterInfo.MinReputationToHire.Item2;
		}

		// Token: 0x06001543 RID: 5443 RVA: 0x000C79E8 File Offset: 0x000C5BE8
		private void CreateCharacterPreviewFrame(GUIListBox listBox, GUIFrame characterFrame, CharacterInfo characterInfo)
		{
			Pivot pivot = (listBox == this.hireableList) ? Pivot.TopLeft : Pivot.TopRight;
			Point absoluteOffset = new Point((pivot == Pivot.TopLeft) ? (listBox.Parent.Parent.Rect.Right + 5) : (listBox.Parent.Parent.Rect.Left - 5), characterFrame.Rect.Top);
			Point frameSize = new Point(GUI.IntScale(300f), GUI.IntScale(350f));
			if (GameMain.GraphicsHeight - (absoluteOffset.Y + frameSize.Y) < 0)
			{
				pivot = ((listBox == this.hireableList) ? Pivot.BottomLeft : Pivot.BottomRight);
				absoluteOffset.Y = characterFrame.Rect.Bottom;
			}
			this.characterPreviewFrame = new GUIFrame(new RectTransform(frameSize, this.campaignUI.GetTabContainer(CampaignMode.InteractionType.Crew).Parent.RectTransform, Anchor.TopLeft, new Pivot?(pivot), ScaleBasis.Normal, false)
			{
				AbsoluteOffset = absoluteOffset
			}, "InnerFrame", null)
			{
				UserData = characterInfo
			};
			GUILayoutGroup mainGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.95f), this.characterPreviewFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.01f,
				Stretch = true
			};
			GUILayoutGroup infoGroup = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.475f), mainGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			GUILayoutGroup infoLabelGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.4f, 1f), infoGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			GUILayoutGroup infoValueGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.6f, 1f), infoGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			float blockHeight = 0.25f;
			new GUITextBlock(new RectTransform(new Vector2(1f, blockHeight), infoLabelGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("name"), new Color?(GUIStyle.TextColorBright), null, Alignment.Left, false, "", null);
			GUITextBlock nameBlock = new GUITextBlock(new RectTransform(new Vector2(1f, blockHeight), infoValueGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null);
			string name = (listBox == this.hireableList) ? characterInfo.OriginalName : characterInfo.Name;
			nameBlock.Text = ToolBox.LimitString(name, nameBlock.Font, nameBlock.Rect.Width);
			if (characterInfo.HasSpecifierTags)
			{
				Identifier menuCategoryVar = characterInfo.Prefab.MenuCategoryVar;
				new GUITextBlock(new RectTransform(new Vector2(1f, blockHeight), infoLabelGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get(menuCategoryVar), new Color?(GUIStyle.TextColorBright), null, Alignment.Left, false, "", null);
				RectTransform rectT = new RectTransform(new Vector2(1f, blockHeight), infoValueGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 1);
				defaultInterpolatedStringHandler.AppendLiteral("[");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(menuCategoryVar);
				defaultInterpolatedStringHandler.AppendLiteral("]");
				new GUITextBlock(rectT, TextManager.Get(characterInfo.ReplaceVars(defaultInterpolatedStringHandler.ToStringAndClear())), null, null, Alignment.Left, false, "", null);
			}
			Job job = characterInfo.Job;
			if (job != null)
			{
				new GUITextBlock(new RectTransform(new Vector2(1f, blockHeight), infoLabelGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("tabmenu.job"), new Color?(GUIStyle.TextColorBright), null, Alignment.Left, false, "", null);
				new GUITextBlock(new RectTransform(new Vector2(1f, blockHeight), infoValueGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), job.Name, null, null, Alignment.Left, false, "", null);
			}
			NPCPersonalityTrait trait = characterInfo.PersonalityTrait;
			if (trait != null)
			{
				new GUITextBlock(new RectTransform(new Vector2(1f, blockHeight), infoLabelGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("PersonalityTrait"), new Color?(GUIStyle.TextColorBright), null, Alignment.Left, false, "", null);
				new GUITextBlock(new RectTransform(new Vector2(1f, blockHeight), infoValueGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), trait.DisplayName, null, null, Alignment.Left, false, "", null);
			}
			infoLabelGroup.Recalculate();
			infoValueGroup.Recalculate();
			new GUIImage(new RectTransform(new Vector2(1f, 0.05f), mainGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "HorizontalLine", GUIImage.ScalingMode.None);
			GUILayoutGroup skillGroup = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.475f), mainGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			GUILayoutGroup skillNameGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.8f, 1f), skillGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			GUILayoutGroup skillLevelGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.2f, 1f), skillGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			IEnumerable<Skill> characterSkills = characterInfo.Job.GetSkills();
			blockHeight = 1f / (float)characterSkills.Count<Skill>();
			foreach (Skill skill in characterSkills)
			{
				RectTransform rectT2 = new RectTransform(new Vector2(1f, blockHeight), skillNameGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text = TextManager.Get("SkillName." + skill.Identifier.ToString());
				GUIFont smallFont = GUIStyle.SmallFont;
				new GUITextBlock(rectT2, text, null, smallFont, Alignment.Left, false, "", null);
				new GUITextBlock(new RectTransform(new Vector2(1f, blockHeight), skillLevelGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), ((int)skill.Level).ToString(), null, null, Alignment.Right, false, "", null);
			}
			if (characterInfo.MinReputationToHire.Item2 > 0f)
			{
				LocalizedString repStr = TextManager.GetWithVariables("campaignstore.reputationrequired", new ValueTuple<string, string>[]
				{
					new ValueTuple<string, string>("[amount]", ((int)characterInfo.MinReputationToHire.Item2).ToString()),
					new ValueTuple<string, string>("[faction]", TextManager.Get("faction." + characterInfo.MinReputationToHire.Item1.ToString()).Value)
				});
				new GUITextBlock(new RectTransform(new Vector2(1f, 0f), mainGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), repStr, new Color?((!this.EnoughReputationToHire(characterInfo)) ? GUIStyle.Orange : GUIStyle.Green), GUIStyle.SmallFont, Alignment.Center, true, "", null);
			}
			mainGroup.Recalculate();
			this.characterPreviewFrame.RectTransform.MinSize = new Point(0, (int)(mainGroup.Children.Sum((GUIComponent c) => (float)c.Rect.Height + (float)mainGroup.Rect.Height * mainGroup.RelativeSpacing) / mainGroup.RectTransform.RelativeSize.Y));
		}

		// Token: 0x06001544 RID: 5444 RVA: 0x000C841C File Offset: 0x000C661C
		private bool SelectCharacter(GUIListBox listBox, GUIFrame characterFrame, CharacterInfo characterInfo)
		{
			if (this.characterPreviewFrame != null && this.characterPreviewFrame.UserData != characterInfo)
			{
				GUIComponent parent = this.characterPreviewFrame.Parent;
				if (parent != null)
				{
					parent.RemoveChild(this.characterPreviewFrame);
				}
				this.characterPreviewFrame = null;
			}
			if (listBox == null || characterFrame == null || characterInfo == null)
			{
				return false;
			}
			if (this.characterPreviewFrame == null)
			{
				this.CreateCharacterPreviewFrame(listBox, characterFrame, characterInfo);
			}
			return true;
		}

		// Token: 0x06001545 RID: 5445 RVA: 0x000C8480 File Offset: 0x000C6680
		private bool AddPendingHire(CharacterInfo characterInfo, bool checkCrewSizeLimit = true, bool createNetworkMessage = true)
		{
			if (checkCrewSizeLimit && characterInfo.BotStatus == BotStatus.PendingHireToActiveService && this.ActiveServiceFull())
			{
				return false;
			}
			this.hireableList.Content.RemoveChild(this.hireableList.Content.FindChild((GUIComponent c) => ((HRManagerUI.InfoSkill)c.UserData).CharacterInfo == characterInfo, false));
			this.hireableList.UpdateScrollBarSize();
			if (!this.PendingHires.Contains(characterInfo))
			{
				this.PendingHires.Add(characterInfo);
			}
			this.CreateCharacterFrame(characterInfo, this.pendingList, false);
			this.SortCharacters(this.pendingList, HRManagerUI.SortingMethod.JobAsc);
			this.pendingList.UpdateScrollBarSize();
			this.SetTotalHireCost();
			if (createNetworkMessage)
			{
				this.SendCrewState(true, default(ValueTuple<CharacterInfo, string>), null, false);
			}
			return true;
		}

		// Token: 0x06001546 RID: 5446 RVA: 0x000C8558 File Offset: 0x000C6758
		private bool RemovePendingHire(CharacterInfo characterInfo, bool setTotalHireCost = true, bool createNetworkMessage = true)
		{
			if (this.PendingHires.Contains(characterInfo))
			{
				this.PendingHires.Remove(characterInfo);
			}
			this.pendingList.Content.RemoveChild(this.pendingList.Content.FindChild((GUIComponent c) => ((HRManagerUI.InfoSkill)c.UserData).CharacterInfo == characterInfo, false));
			this.pendingList.UpdateScrollBarSize();
			if (!GameMain.IsMultiplayer)
			{
				CharacterInfo characterInfo2 = characterInfo;
				if (characterInfo2 != null)
				{
					characterInfo2.ResetName();
				}
			}
			if (this.campaign.Map.CurrentLocation.HireManager.AvailableCharacters.Any((CharacterInfo info) => info.GetIdentifierUsingOriginalName() == characterInfo.GetIdentifierUsingOriginalName()) && this.hireableList.Content.Children.None(delegate(GUIComponent c)
			{
				object userData2 = c.UserData;
				if (userData2 is HRManagerUI.InfoSkill)
				{
					HRManagerUI.InfoSkill userData = (HRManagerUI.InfoSkill)userData2;
					return userData.CharacterInfo.GetIdentifierUsingOriginalName() == characterInfo.GetIdentifierUsingOriginalName();
				}
				return false;
			}))
			{
				this.CreateCharacterFrame(characterInfo, this.hireableList, false);
				this.SortCharacters(this.hireableList, (HRManagerUI.SortingMethod)this.sortingDropDown.SelectedItemData);
				this.hireableList.UpdateScrollBarSize();
			}
			if (setTotalHireCost)
			{
				this.SetTotalHireCost();
			}
			if (createNetworkMessage)
			{
				this.SendCrewState(true, default(ValueTuple<CharacterInfo, string>), null, false);
			}
			return true;
		}

		// Token: 0x06001547 RID: 5447 RVA: 0x000C8694 File Offset: 0x000C6894
		private bool RemoveAllPendingHires(bool createNetworkMessage = true)
		{
			this.pendingList.Content.Children.ToList<GUIComponent>().ForEach(delegate(GUIComponent c)
			{
				this.RemovePendingHire(((HRManagerUI.InfoSkill)c.UserData).CharacterInfo, false, createNetworkMessage);
			});
			this.SetTotalHireCost();
			return true;
		}

		// Token: 0x06001548 RID: 5448 RVA: 0x000C86E4 File Offset: 0x000C68E4
		private void SetTotalHireCost()
		{
			if (this.pendingList == null || this.totalBlock == null || this.validateHiresButton == null)
			{
				return;
			}
			CharacterInfo[] infos = (from c in this.pendingList.Content.Children
			select ((HRManagerUI.InfoSkill)c.UserData).CharacterInfo).ToArray<CharacterInfo>();
			int total = HireManager.GetSalaryFor(infos);
			this.totalBlock.Text = TextManager.FormatCurrency(total, true);
			bool enoughMoney = this.campaign == null || this.campaign.CanAfford(total, null);
			this.totalBlock.TextColor = (enoughMoney ? Color.White : Color.Red);
			this.validateHiresButton.Enabled = (enoughMoney && HRManagerUI.HasPermissionToHire && this.pendingList.Content.RectTransform.Children.Any<RectTransform>());
		}

		// Token: 0x06001549 RID: 5449 RVA: 0x000C87C8 File Offset: 0x000C69C8
		public bool ValidateHires(List<CharacterInfo> hires, bool takeMoney = true, bool createNetworkEvent = false, bool createNotification = true)
		{
			if (hires == null || hires.None(null))
			{
				return false;
			}
			List<CharacterInfo> nonDuplicateHires = new List<CharacterInfo>();
			hires.ForEach(delegate(CharacterInfo hireInfo)
			{
				if (this.campaign.CrewManager.GetCharacterInfos(true).None((CharacterInfo crewInfo) => crewInfo.IsNewHire && crewInfo.GetIdentifierUsingOriginalName() == hireInfo.GetIdentifierUsingOriginalName()))
				{
					nonDuplicateHires.Add(hireInfo);
				}
			});
			if (nonDuplicateHires.None(null))
			{
				return false;
			}
			if (takeMoney)
			{
				int total = HireManager.GetSalaryFor(nonDuplicateHires);
				if (!this.campaign.CanAfford(total, null))
				{
					return false;
				}
			}
			bool atLeastOneHiredToActiveDuty = false;
			bool atLeastOneHiredToReserveBench = false;
			foreach (CharacterInfo ci in nonDuplicateHires)
			{
				bool toReserveBench = ci.BotStatus == BotStatus.PendingHireToReserveBench;
				if (!this.campaign.TryHireCharacter(this.campaign.Map.CurrentLocation, ci, takeMoney, null, false))
				{
					break;
				}
				if (toReserveBench)
				{
					atLeastOneHiredToReserveBench = true;
				}
				else
				{
					atLeastOneHiredToActiveDuty = true;
				}
			}
			if (atLeastOneHiredToActiveDuty || atLeastOneHiredToReserveBench)
			{
				this.UpdateLocationView(this.campaign.Map.CurrentLocation, true, null);
				this.SelectCharacter(null, null, null);
				if (createNotification)
				{
					LocalizedString msg = string.Empty;
					if (atLeastOneHiredToActiveDuty)
					{
						LocalizedString left = msg;
						string tag = "crewhiredmessage";
						string varName = "[location]";
						CampaignUI campaignUI = this.campaignUI;
						LocalizedString value;
						if (campaignUI == null)
						{
							value = null;
						}
						else
						{
							CampaignMode campaign = campaignUI.Campaign;
							if (campaign == null)
							{
								value = null;
							}
							else
							{
								Map map = campaign.Map;
								if (map == null)
								{
									value = null;
								}
								else
								{
									Location currentLocation = map.CurrentLocation;
									value = ((currentLocation != null) ? currentLocation.DisplayName : null);
								}
							}
						}
						msg = left + TextManager.GetWithVariable(tag, varName, value, FormatCapitals.No);
					}
					if (atLeastOneHiredToReserveBench)
					{
						if (!msg.IsNullOrEmpty())
						{
							msg += "\n\n";
						}
						LocalizedString left2 = msg;
						NetworkMember networkMember = GameMain.NetworkMember;
						ServerSettings serverSettings = (networkMember != null) ? networkMember.ServerSettings : null;
						msg = left2 + ((serverSettings != null && serverSettings.RespawnMode == RespawnMode.Permadeath && !serverSettings.IronmanMode) ? TextManager.Get("crewhiredmessage.reservebench.permadeath") : TextManager.Get("crewhiredmessage.reservebench"));
					}
					GUIMessageBox dialog = new GUIMessageBox(TextManager.Get("newcrewmembers"), msg, new LocalizedString[]
					{
						TextManager.Get("Ok")
					}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
					GUIButton guibutton = dialog.Buttons[0];
					guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(dialog.Close));
				}
			}
			if (createNetworkEvent)
			{
				this.SendCrewState(true, default(ValueTuple<CharacterInfo, string>), null, true);
			}
			return false;
		}

		// Token: 0x0600154A RID: 5450 RVA: 0x000C8A4C File Offset: 0x000C6C4C
		private bool CreateRenamingComponent(GUIButton button, object userData)
		{
			if (HRManagerUI.HasPermissionToHire)
			{
				CharacterInfo characterInfo = userData2 as CharacterInfo;
				if (characterInfo != null)
				{
					GUIFrame outerGlowFrame = new GUIFrame(new RectTransform(new Vector2(1.25f, 1.25f), this.parentComponent.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "OuterGlow", new Color?(Color.Black * 0.7f));
					GUIFrame frame = new GUIFrame(new RectTransform(new Vector2(0.33f, 0.4f), outerGlowFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal)
					{
						MaxSize = new Point(400, 300).Multiply(GUI.Scale)
					}, "", null);
					GUILayoutGroup layoutGroup = new GUILayoutGroup(new RectTransform((frame.Rect.Size - GUIStyle.ItemFrameMargin).Multiply(new Vector2(0.75f, 1f)), frame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), false, Anchor.TopCenter)
					{
						RelativeSpacing = 0.02f,
						Stretch = true
					};
					RectTransform rectT = new RectTransform(new Vector2(1f, 0f), layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
					RichString text2 = TextManager.Get("campaigncrew.givenickname");
					GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
					new GUITextBlock(rectT, text2, null, subHeadingFont, Alignment.Center, true, "", null);
					Vector2 groupElementSize = new Vector2(1f, 0.25f);
					GUITextBox nameBox = new GUITextBox(new RectTransform(groupElementSize, layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null, false, true)
					{
						MaxTextLength = new int?(32)
					};
					nameBox.OnTextChanged += delegate(GUITextBox textBox, string text)
					{
						if (text.Contains('\n') || text.Contains('\r'))
						{
							textBox.Text = text.Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ');
						}
						return true;
					};
					new GUIButton(new RectTransform(groupElementSize, layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("confirm"), Alignment.Center, "", null).OnClicked = delegate(GUIButton button, object userData)
					{
						HRManagerUI <>4__this = this;
						CharacterInfo characterInfo = characterInfo;
						string text3 = nameBox.Text;
						if (<>4__this.RenameCharacter(characterInfo, (text3 != null) ? text3.Trim() : null))
						{
							this.parentComponent.RemoveChild(outerGlowFrame);
							return true;
						}
						nameBox.Flash(new Color?(Color.Red), 1.5f, false, false, null);
						return false;
					};
					new GUIButton(new RectTransform(groupElementSize, layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("cancel"), Alignment.Center, "", null).OnClicked = delegate(GUIButton button, object userData)
					{
						this.parentComponent.RemoveChild(outerGlowFrame);
						return true;
					};
					layoutGroup.Recalculate();
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600154B RID: 5451 RVA: 0x000C8D90 File Offset: 0x000C6F90
		public bool RenameCharacter(CharacterInfo characterInfo, string newName)
		{
			if (characterInfo == null || string.IsNullOrEmpty(newName))
			{
				return false;
			}
			if (newName == characterInfo.Name)
			{
				return false;
			}
			if (GameMain.IsMultiplayer)
			{
				this.SendCrewState(false, new ValueTuple<CharacterInfo, string>(characterInfo, newName), null, false);
			}
			else
			{
				GUIComponent crewComponent = this.crewList.Content.FindChild((GUIComponent c) => ((HRManagerUI.InfoSkill)c.UserData).CharacterInfo == characterInfo, false);
				if (crewComponent != null)
				{
					this.crewList.Content.RemoveChild(crewComponent);
					this.campaign.CrewManager.RenameCharacter(characterInfo, newName);
					this.CreateCharacterFrame(characterInfo, this.crewList, false);
					this.SortCharacters(this.crewList, HRManagerUI.SortingMethod.JobAsc);
				}
				else
				{
					GUIComponent pendingComponent = this.pendingList.Content.FindChild((GUIComponent c) => ((HRManagerUI.InfoSkill)c.UserData).CharacterInfo == characterInfo, false);
					if (pendingComponent == null)
					{
						return false;
					}
					this.pendingList.Content.RemoveChild(pendingComponent);
					this.campaign.Map.CurrentLocation.HireManager.RenameCharacter(characterInfo, newName);
					this.CreateCharacterFrame(characterInfo, this.pendingList, false);
					this.SortCharacters(this.pendingList, HRManagerUI.SortingMethod.JobAsc);
					this.SetTotalHireCost();
				}
			}
			return true;
		}

		// Token: 0x0600154C RID: 5452 RVA: 0x000C8EE0 File Offset: 0x000C70E0
		private bool FireCharacter(GUIButton button, object selection)
		{
			CharacterInfo characterInfo = selection as CharacterInfo;
			if (characterInfo == null)
			{
				return false;
			}
			this.campaign.CrewManager.FireCharacter(characterInfo);
			this.SelectCharacter(null, null, null);
			this.UpdateCrew();
			bool updatePending = false;
			CharacterInfo firedCharacter = characterInfo;
			this.SendCrewState(updatePending, default(ValueTuple<CharacterInfo, string>), firedCharacter, false);
			return false;
		}

		// Token: 0x0600154D RID: 5453 RVA: 0x000C8F30 File Offset: 0x000C7130
		public void Update()
		{
			if (GameMain.GraphicsWidth != this.resolutionWhenCreated.X || GameMain.GraphicsHeight != this.resolutionWhenCreated.Y)
			{
				this.CreateUI();
				this.UpdateLocationView(this.campaign.Map.CurrentLocation, false, null);
			}
			else
			{
				this.playerBalanceElement = CampaignUI.UpdateBalanceElement(this.playerBalanceElement);
			}
			this.pendingAndCrewPanel.Visible = !HRManagerUI.ReplacingPermanentlyDeadCharacter;
			if (this.hadPermissionToHire != HRManagerUI.HasPermissionToHire || this.wasReplacingPermanentlyDeadCharacter != HRManagerUI.ReplacingPermanentlyDeadCharacter)
			{
				this.RefreshUI();
			}
			if (this.needsHireableRefresh)
			{
				this.RefreshCrewFrames(this.hireableList);
				GUIDropDown guidropDown = this.sortingDropDown;
				if (((guidropDown != null) ? guidropDown.SelectedItemData : null) != null)
				{
					this.SortCharacters(this.hireableList, (HRManagerUI.SortingMethod)this.sortingDropDown.SelectedItemData);
				}
				this.needsHireableRefresh = false;
			}
			ValueTuple<GUIComponent, CharacterInfo> valueTuple = HRManagerUI.<Update>g__FindHighlightedCharacter|52_0(GUI.MouseOn);
			GUIComponent highlightedFrame = valueTuple.Item1;
			CharacterInfo highlightedInfo = valueTuple.Item2;
			if (highlightedFrame != null && highlightedInfo != null)
			{
				if (this.characterPreviewFrame == null || highlightedInfo != this.characterPreviewFrame.UserData)
				{
					GUIComponent component = GUI.MouseOn;
					GUIListBox listBox = null;
					while (!(component.Parent is GUIListBox))
					{
						if (component.Parent != null)
						{
							component = component.Parent;
							if (listBox == null)
							{
								continue;
							}
						}
						IL_13B:
						if (listBox != null)
						{
							this.SelectCharacter(listBox, highlightedFrame as GUIFrame, highlightedInfo);
							return;
						}
						return;
					}
					listBox = (component.Parent as GUIListBox);
					goto IL_13B;
				}
			}
			else if (this.characterPreviewFrame != null)
			{
				GUIComponent parent = this.characterPreviewFrame.Parent;
				if (parent != null)
				{
					parent.RemoveChild(this.characterPreviewFrame);
				}
				this.characterPreviewFrame = null;
			}
		}

		// Token: 0x0600154E RID: 5454 RVA: 0x000C90B8 File Offset: 0x000C72B8
		public void SetPendingHires(List<ushort> characterInfos, bool[] characterInfoReserveBenchStatuses, Location location, bool checkCrewSizeLimit)
		{
			List<CharacterInfo> oldHires = this.PendingHires.ToList<CharacterInfo>();
			foreach (CharacterInfo pendingHire in oldHires)
			{
				this.RemovePendingHire(pendingHire, true, false);
			}
			this.PendingHires.Clear();
			int i = 0;
			using (List<ushort>.Enumerator enumerator2 = characterInfos.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					ushort identifier = enumerator2.Current;
					CharacterInfo match = location.HireManager.AvailableCharacters.Find((CharacterInfo info) => info.ID == identifier);
					if (match != null)
					{
						match.BotStatus = (characterInfoReserveBenchStatuses[i] ? BotStatus.PendingHireToReserveBench : BotStatus.PendingHireToActiveService);
						this.AddPendingHire(match, checkCrewSizeLimit, false);
						if (!this.PendingHires.Contains(match))
						{
							DebugConsole.ThrowError("Failed to add a pending hire", null, null, false, false);
						}
					}
					else
					{
						DebugConsole.ThrowError("Received a hire that doesn't exist.", null, null, false, false);
					}
					i++;
				}
			}
		}

		// Token: 0x0600154F RID: 5455 RVA: 0x000C91DC File Offset: 0x000C73DC
		public void SendCrewState(bool updatePending = false, [TupleElementNames(new string[]
		{
			"info",
			"newName"
		})] ValueTuple<CharacterInfo, string> renameCharacter = default(ValueTuple<CharacterInfo, string>), CharacterInfo firedCharacter = null, bool validateHires = false)
		{
			if (this.campaign is MultiPlayerCampaign)
			{
				IWriteMessage msg = new WriteOnlyMessage();
				msg.WriteByte(16);
				msg.WriteBoolean(updatePending);
				if (updatePending)
				{
					msg.WriteUInt16((ushort)this.PendingHires.Count);
					foreach (CharacterInfo pendingHire in this.PendingHires)
					{
						msg.WriteUInt16(pendingHire.ID);
						msg.WriteBoolean(pendingHire.BotStatus == BotStatus.PendingHireToReserveBench);
					}
				}
				msg.WriteBoolean(validateHires);
				bool validRenaming = renameCharacter.Item1 != null && !string.IsNullOrEmpty(renameCharacter.Item2);
				msg.WriteBoolean(validRenaming);
				if (validRenaming)
				{
					msg.WriteUInt16(renameCharacter.Item1.ID);
					msg.WriteString(renameCharacter.Item2);
					CrewManager crewManager = this.campaign.CrewManager;
					bool existingCrewMember = crewManager != null && crewManager.GetCharacterInfos(true).Any((CharacterInfo ci) => ci.ID == renameCharacter.Item1.ID);
					msg.WriteBoolean(existingCrewMember);
				}
				msg.WriteBoolean(firedCharacter != null);
				if (firedCharacter != null)
				{
					msg.WriteUInt16(firedCharacter.ID);
				}
				ClientPeer clientPeer = GameMain.Client.ClientPeer;
				if (clientPeer == null)
				{
					return;
				}
				clientPeer.Send(msg, DeliveryMethod.Reliable, true);
			}
		}

		// Token: 0x0600155E RID: 5470 RVA: 0x000C9628 File Offset: 0x000C7828
		[CompilerGenerated]
		private int? <SortCharacters>g__CompareReputationRequirement|37_5(GUIComponent c1, GUIComponent c2)
		{
			CharacterInfo info = ((HRManagerUI.InfoSkill)c1.UserData).CharacterInfo;
			CharacterInfo info2 = ((HRManagerUI.InfoSkill)c2.UserData).CharacterInfo;
			float requirement = this.EnoughReputationToHire(info) ? 0f : info.MinReputationToHire.Item2;
			float requirement2 = this.EnoughReputationToHire(info2) ? 0f : info2.MinReputationToHire.Item2;
			if (MathUtils.NearlyEqual(requirement, 0f, 0.0001f) && MathUtils.NearlyEqual(requirement2, 0f, 0.0001f))
			{
				return null;
			}
			return new int?(requirement.CompareTo(requirement2));
		}

		// Token: 0x0600155F RID: 5471 RVA: 0x000C96CC File Offset: 0x000C78CC
		[CompilerGenerated]
		[return: TupleElementNames(new string[]
		{
			"GuiComponent",
			"CharacterInfo"
		})]
		internal static ValueTuple<GUIComponent, CharacterInfo> <Update>g__FindHighlightedCharacter|52_0(GUIComponent c)
		{
			if (c == null)
			{
				return default(ValueTuple<GUIComponent, CharacterInfo>);
			}
			object userData = c.UserData;
			if (userData is HRManagerUI.InfoSkill)
			{
				HRManagerUI.InfoSkill highlightedData = (HRManagerUI.InfoSkill)userData;
				return new ValueTuple<GUIComponent, CharacterInfo>(c, highlightedData.CharacterInfo);
			}
			if (c.Parent == null)
			{
				return default(ValueTuple<GUIComponent, CharacterInfo>);
			}
			if (c.Parent is GUIListBox)
			{
				return default(ValueTuple<GUIComponent, CharacterInfo>);
			}
			return HRManagerUI.<Update>g__FindHighlightedCharacter|52_0(c.Parent);
		}

		// Token: 0x04000AAB RID: 2731
		private readonly CampaignUI campaignUI;

		// Token: 0x04000AAC RID: 2732
		private readonly GUIComponent parentComponent;

		// Token: 0x04000AAD RID: 2733
		private GUIComponent pendingAndCrewPanel;

		// Token: 0x04000AAE RID: 2734
		private GUIListBox hireableList;

		// Token: 0x04000AAF RID: 2735
		private GUIListBox pendingList;

		// Token: 0x04000AB0 RID: 2736
		private GUIListBox crewList;

		// Token: 0x04000AB1 RID: 2737
		private GUIFrame characterPreviewFrame;

		// Token: 0x04000AB2 RID: 2738
		private GUIDropDown sortingDropDown;

		// Token: 0x04000AB3 RID: 2739
		private GUITextBlock totalBlock;

		// Token: 0x04000AB4 RID: 2740
		private GUIButton validateHiresButton;

		// Token: 0x04000AB5 RID: 2741
		private GUIButton clearAllButton;

		// Token: 0x04000AB6 RID: 2742
		private CampaignUI.PlayerBalanceElement? playerBalanceElement;

		// Token: 0x04000AB7 RID: 2743
		private bool wasReplacingPermanentlyDeadCharacter;

		// Token: 0x04000AB8 RID: 2744
		private bool hadPermissionToHire;

		// Token: 0x04000AB9 RID: 2745
		private Point resolutionWhenCreated;

		// Token: 0x04000ABA RID: 2746
		private bool needsHireableRefresh;

		// Token: 0x02000988 RID: 2440
		private enum SortingMethod
		{
			// Token: 0x04004188 RID: 16776
			AlphabeticalAsc,
			// Token: 0x04004189 RID: 16777
			JobAsc,
			// Token: 0x0400418A RID: 16778
			PriceAsc,
			// Token: 0x0400418B RID: 16779
			PriceDesc,
			// Token: 0x0400418C RID: 16780
			SkillAsc,
			// Token: 0x0400418D RID: 16781
			SkillDesc
		}

		// Token: 0x02000989 RID: 2441
		private readonly struct InfoSkill
		{
			// Token: 0x06007228 RID: 29224 RVA: 0x0036C2C7 File Offset: 0x0036A4C7
			public InfoSkill(CharacterInfo characterInfo, float skillLevel)
			{
				this.CharacterInfo = characterInfo;
				this.SkillLevel = skillLevel;
			}

			// Token: 0x0400418E RID: 16782
			public readonly CharacterInfo CharacterInfo;

			// Token: 0x0400418F RID: 16783
			public readonly float SkillLevel;
		}
	}
}
