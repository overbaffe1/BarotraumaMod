using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x02000068 RID: 104
	internal abstract class CampaignMode : GameMode
	{
		// Token: 0x170003E1 RID: 993
		// (get) Token: 0x06000EB5 RID: 3765 RVA: 0x0008AADE File Offset: 0x00088CDE
		// (set) Token: 0x06000EB6 RID: 3766 RVA: 0x0008AAE6 File Offset: 0x00088CE6
		public bool CrewDead { get; protected set; }

		// Token: 0x170003E2 RID: 994
		// (get) Token: 0x06000EB7 RID: 3767 RVA: 0x0008AAEF File Offset: 0x00088CEF
		public GUIButton EndRoundButton
		{
			get
			{
				return this.endRoundButton;
			}
		}

		// Token: 0x170003E3 RID: 995
		// (get) Token: 0x06000EB8 RID: 3768 RVA: 0x0008AAF7 File Offset: 0x00088CF7
		// (set) Token: 0x06000EB9 RID: 3769 RVA: 0x0008AAFF File Offset: 0x00088CFF
		public SlideshowPlayer SlideshowPlayer { get; protected set; }

		// Token: 0x170003E4 RID: 996
		// (get) Token: 0x06000EBA RID: 3770 RVA: 0x0008AB08 File Offset: 0x00088D08
		// (set) Token: 0x06000EBB RID: 3771 RVA: 0x0008AB10 File Offset: 0x00088D10
		public bool ForceMapUI { get; protected set; }

		// Token: 0x170003E5 RID: 997
		// (get) Token: 0x06000EBC RID: 3772 RVA: 0x0008AB19 File Offset: 0x00088D19
		// (set) Token: 0x06000EBD RID: 3773 RVA: 0x0008AB24 File Offset: 0x00088D24
		public bool ShowCampaignUI
		{
			get
			{
				return this.showCampaignUI;
			}
			set
			{
				if (value == this.showCampaignUI)
				{
					return;
				}
				CrewManager crewManager = base.CrewManager;
				ChatBox chatBox2;
				if ((chatBox2 = ((crewManager != null) ? crewManager.ChatBox : null)) == null)
				{
					GameClient client = GameMain.Client;
					chatBox2 = ((client != null) ? client.ChatBox : null);
				}
				ChatBox chatBox = chatBox2;
				if (value)
				{
					if (chatBox != null)
					{
						this.wasChatBoxOpen = chatBox.ToggleOpen;
						chatBox.ToggleOpen = false;
					}
				}
				else if (chatBox != null)
				{
					chatBox.ToggleOpen = this.wasChatBoxOpen;
				}
				if (!value)
				{
					CampaignUI campaignUI = this.CampaignUI;
					CampaignMode.InteractionType? interactionType = (campaignUI != null) ? new CampaignMode.InteractionType?(campaignUI.SelectedTab) : null;
					if (interactionType != null)
					{
						switch (interactionType.GetValueOrDefault())
						{
						case CampaignMode.InteractionType.Store:
						{
							Store store = this.CampaignUI.Store;
							if (store != null)
							{
								store.OnDeselected();
							}
							break;
						}
						case CampaignMode.InteractionType.PurchaseSub:
							SubmarinePreview.Close();
							break;
						case CampaignMode.InteractionType.MedicalClinic:
						{
							MedicalClinicUI medicalClinic = this.CampaignUI.MedicalClinic;
							if (medicalClinic != null)
							{
								medicalClinic.OnDeselected();
							}
							break;
						}
						}
					}
				}
				this.showCampaignUI = value;
			}
		}

		// Token: 0x170003E6 RID: 998
		// (get) Token: 0x06000EBE RID: 3774 RVA: 0x0008AC1A File Offset: 0x00088E1A
		public virtual Wallet Wallet
		{
			get
			{
				return this.GetWallet(null);
			}
		}

		// Token: 0x06000EBF RID: 3775 RVA: 0x0008AC24 File Offset: 0x00088E24
		public override void ShowStartMessage()
		{
			foreach (Mission mission in (from m in this.Missions
			orderby m.Prefab.IsSideObjective
			select m).ToList<Mission>())
			{
				if (mission.Prefab.ShowStartMessage)
				{
					RichString headerText = RichString.Rich(mission.Prefab.IsSideObjective ? TextManager.AddPunctuation(':', new LocalizedString[]
					{
						TextManager.Get("sideobjective"),
						mission.Name
					}) : mission.Name, null);
					RichString text = RichString.Rich(mission.Description, null);
					LocalizedString[] buttons = Array.Empty<LocalizedString>();
					Sprite icon = mission.Prefab.Icon;
					GUIMessageBox guimessageBox = new GUIMessageBox(headerText, text, buttons, null, null, Alignment.TopLeft, GUIMessageBox.Type.InGame, "", icon, "", null, null, false);
					guimessageBox.IconColor = mission.Prefab.IconColor;
					guimessageBox.UserData = "missionstartmessage";
				}
			}
		}

		// Token: 0x06000EC0 RID: 3776 RVA: 0x0008AD50 File Offset: 0x00088F50
		private static bool IsOwner(Client client)
		{
			return client != null && client.IsOwner;
		}

		// Token: 0x06000EC1 RID: 3777 RVA: 0x0008AD5D File Offset: 0x00088F5D
		public static bool AllowedToManageCampaign(ClientPermissions permissions)
		{
			return GameMain.Client == null || GameMain.Client.HasPermission(permissions) || GameMain.Client.HasPermission(ClientPermissions.ManageCampaign) || GameMain.Client.IsServerOwner || CampaignMode.AnyOneAllowedToManageCampaign(permissions);
		}

		// Token: 0x06000EC2 RID: 3778 RVA: 0x0008AD97 File Offset: 0x00088F97
		public static bool AllowedToManageWallets()
		{
			return CampaignMode.AllowedToManageCampaign(ClientPermissions.ManageMoney);
		}

		// Token: 0x06000EC3 RID: 3779 RVA: 0x0008ADA3 File Offset: 0x00088FA3
		public static bool AllowImmediateItemDelivery()
		{
			return GameMain.Client == null || GameMain.Client.ServerSettings.AllowImmediateItemDelivery || GameMain.Client.HasPermission(ClientPermissions.ManageCampaign) || GameMain.Client.IsServerOwner;
		}

		// Token: 0x06000EC4 RID: 3780 RVA: 0x0008ADDC File Offset: 0x00088FDC
		protected GUIButton CreateEndRoundButton()
		{
			int buttonWidth = (int)(450f * GUI.xScale * (GUI.IsUltrawide ? 3f : 1f));
			int buttonHeight = (int)(40f * GUI.yScale);
			RectTransform rectT = HUDLayoutSettings.ToRectTransform(new Rectangle(GameMain.GraphicsWidth / 2, HUDLayoutSettings.ButtonAreaTop.Center.Y, buttonWidth, buttonHeight), GUI.Canvas);
			rectT.Pivot = Pivot.Center;
			return new GUIButton(rectT, TextManager.Get("EndRound"), Alignment.Center, "EndRoundButton", null)
			{
				Pulse = true,
				TextBlock = 
				{
					Shadow = true,
					AutoScaleHorizontal = true
				}
			};
		}

		// Token: 0x06000EC5 RID: 3781 RVA: 0x0008AE8C File Offset: 0x0008908C
		public override void Draw(SpriteBatch spriteBatch)
		{
			if (this.overlayColor.A > 0)
			{
				if (this.overlaySprite != null)
				{
					GUI.DrawRectangle(spriteBatch, new Rectangle(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight), Color.Black * ((float)this.overlayColor.A / 255f), true, 0f, 1f);
					float scale = Math.Max((float)GameMain.GraphicsWidth / this.overlaySprite.size.X, (float)GameMain.GraphicsHeight / this.overlaySprite.size.Y);
					this.overlaySprite.Draw(spriteBatch, new Vector2((float)GameMain.GraphicsWidth, (float)GameMain.GraphicsHeight) / 2f, this.overlayColor, this.overlaySprite.size / 2f, 0f, scale, SpriteEffects.None, null);
				}
				else
				{
					GUI.DrawRectangle(spriteBatch, new Rectangle(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight), this.overlayColor, true, 0f, 1f);
				}
			}
			SlideshowPlayer slideshowPlayer = this.SlideshowPlayer;
			if (slideshowPlayer != null)
			{
				slideshowPlayer.DrawManually(spriteBatch, false, true);
			}
			if (GUI.DisableHUD || GUI.DisableUpperHUD || this.ForceMapUI || CoroutineManager.IsCoroutineRunning("LevelTransition"))
			{
				this.endRoundButton.Visible = false;
				if (this.ReadyCheckButton != null)
				{
					this.ReadyCheckButton.Visible = false;
				}
				return;
			}
			if (Submarine.MainSub == null || Level.Loaded == null)
			{
				return;
			}
			bool allowEndingRound = false;
			this.endRoundButton.Color = this.endRoundButton.Style.Color;
			this.endRoundButton.HoverColor = this.endRoundButton.Style.HoverColor;
			RichString overrideEndRoundButtonToolTip = string.Empty;
			LevelData levelData;
			Submarine leavingSub;
			CampaignMode.TransitionType availableTransition = this.GetAvailableTransition(out levelData, out leavingSub);
			LocalizedString buttonText = "";
			switch (availableTransition)
			{
			case CampaignMode.TransitionType.LeaveLocation:
			{
				string tag = "LeaveLocation";
				string varName = "[locationname]";
				Location startLocation = Level.Loaded.StartLocation;
				buttonText = TextManager.GetWithVariable(tag, varName, ((startLocation != null) ? startLocation.DisplayName : null) ?? "[ERROR]", FormatCapitals.No);
				allowEndingRound = (!this.ForceMapUI && !this.ShowCampaignUI);
				goto IL_48F;
			}
			case CampaignMode.TransitionType.ProgressToNextLocation:
			case CampaignMode.TransitionType.ProgressToNextEmptyLocation:
				if (Level.Loaded.EndOutpost == null || !Level.Loaded.EndOutpost.DockedTo.Contains(leavingSub))
				{
					string textTag = (availableTransition == CampaignMode.TransitionType.ProgressToNextLocation) ? "EnterLocation" : "EnterEmptyLocation";
					string tag2 = textTag;
					string varName2 = "[locationname]";
					Location endLocation = Level.Loaded.EndLocation;
					buttonText = TextManager.GetWithVariable(tag2, varName2, ((endLocation != null) ? endLocation.DisplayName : null) ?? "[ERROR]", FormatCapitals.No);
					allowEndingRound = (!this.ForceMapUI && !this.ShowCampaignUI);
					goto IL_48F;
				}
				goto IL_48F;
			case CampaignMode.TransitionType.ReturnToPreviousLocation:
			case CampaignMode.TransitionType.ReturnToPreviousEmptyLocation:
				if (Level.Loaded.StartOutpost == null || !Level.Loaded.StartOutpost.DockedTo.Contains(leavingSub))
				{
					string textTag2 = (availableTransition == CampaignMode.TransitionType.ReturnToPreviousLocation) ? "EnterLocation" : "EnterEmptyLocation";
					string tag3 = textTag2;
					string varName3 = "[locationname]";
					Location startLocation2 = Level.Loaded.StartLocation;
					buttonText = TextManager.GetWithVariable(tag3, varName3, ((startLocation2 != null) ? startLocation2.DisplayName : null) ?? "[ERROR]", FormatCapitals.No);
					allowEndingRound = (!this.ForceMapUI && !this.ShowCampaignUI);
					goto IL_48F;
				}
				goto IL_48F;
			}
			Character controlled = Character.Controlled;
			bool inFriendlySub = controlled != null && controlled.IsInFriendlySub;
			if (Level.Loaded.Type == LevelData.LevelType.Outpost && !Level.Loaded.IsEndBiome)
			{
				if (!inFriendlySub)
				{
					Character controlled2 = Character.Controlled;
					bool? flag;
					if (controlled2 == null)
					{
						flag = null;
					}
					else
					{
						Hull currentHull = controlled2.CurrentHull;
						flag = ((currentHull != null) ? new bool?(currentHull.OutpostModuleTags.Contains("airlock".ToIdentifier())) : null);
					}
					bool? flag2 = flag;
					if (!flag2.GetValueOrDefault())
					{
						goto IL_48D;
					}
				}
				if (this.Missions.Any(delegate(Mission m)
				{
					SalvageMission salvageMission = m as SalvageMission;
					return salvageMission != null && salvageMission.AnyTargetNeedsToBeRetrievedToSub;
				}))
				{
					overrideEndRoundButtonToolTip = TextManager.Get("SalvageTargetNotInSub");
					this.endRoundButton.Color = GUIStyle.Red * 0.7f;
					this.endRoundButton.HoverColor = GUIStyle.Red;
				}
				string tag4 = "LeaveLocation";
				string varName4 = "[locationname]";
				Location startLocation3 = Level.Loaded.StartLocation;
				buttonText = TextManager.GetWithVariable(tag4, varName4, ((startLocation3 != null) ? startLocation3.DisplayName : null) ?? "[ERROR]", FormatCapitals.No);
				allowEndingRound = (!this.ForceMapUI && !this.ShowCampaignUI);
				goto IL_48F;
			}
			IL_48D:
			allowEndingRound = false;
			IL_48F:
			if (Level.IsLoadedOutpost && !ObjectiveManager.AllActiveObjectivesCompleted() && !(this is MultiPlayerCampaign))
			{
				allowEndingRound = false;
			}
			if (this.ReadyCheckButton != null)
			{
				this.ReadyCheckButton.Visible = (allowEndingRound && GameMain.GameSession != null && GameMain.GameSession.RoundDuration > 10f);
			}
			GUIComponent guicomponent = this.endRoundButton;
			bool visible;
			if (allowEndingRound)
			{
				controlled = Character.Controlled;
				visible = (controlled != null && !controlled.IsIncapacitated);
			}
			else
			{
				visible = false;
			}
			guicomponent.Visible = visible;
			if (this.endRoundButton.Visible)
			{
				if (!CampaignMode.AllowedToManageCampaign(ClientPermissions.ManageMap))
				{
					buttonText = TextManager.Get("map");
				}
				else if (this.prevCampaignUIAutoOpenType != availableTransition && availableTransition == CampaignMode.TransitionType.ProgressToNextEmptyLocation)
				{
					HintManager.OnAvailableTransition(availableTransition);
					HintManager.Update();
					this.Map.SelectLocation(-1);
					this.endRoundButton.OnClicked(this.EndRoundButton, null);
					this.prevCampaignUIAutoOpenType = availableTransition;
				}
				this.endRoundButton.Text = ToolBox.LimitString(buttonText.Value, this.endRoundButton.Font, this.endRoundButton.Rect.Width - 5);
				if (overrideEndRoundButtonToolTip != string.Empty)
				{
					this.endRoundButton.ToolTip = overrideEndRoundButtonToolTip;
				}
				else if (this.endRoundButton.Text != buttonText)
				{
					this.endRoundButton.ToolTip = buttonText;
				}
				Character controlled3 = Character.Controlled;
				bool? flag3;
				if (controlled3 == null)
				{
					flag3 = null;
				}
				else
				{
					CharacterHealth characterHealth = controlled3.CharacterHealth;
					if (characterHealth == null)
					{
						flag3 = null;
					}
					else
					{
						GUIButton suicideButton = characterHealth.SuicideButton;
						flag3 = ((suicideButton != null) ? new bool?(suicideButton.Visible) : null);
					}
				}
				bool? flag2 = flag3;
				if (flag2.GetValueOrDefault())
				{
					this.endRoundButton.RectTransform.ScreenSpaceOffset = new Point(0, Character.Controlled.CharacterHealth.SuicideButton.Rect.Height);
				}
				else if (GameMain.Client != null && GameMain.Client.IsFollowSubTickBoxVisible)
				{
					this.endRoundButton.RectTransform.ScreenSpaceOffset = new Point(0, HUDLayoutSettings.Padding + GameMain.Client.FollowSubTickBox.Rect.Height);
				}
				else
				{
					this.endRoundButton.RectTransform.ScreenSpaceOffset = Point.Zero;
				}
			}
			this.endRoundButton.DrawManually(spriteBatch, false, true);
			if (this is MultiPlayerCampaign && this.ReadyCheckButton != null)
			{
				this.ReadyCheckButton.RectTransform.ScreenSpaceOffset = this.endRoundButton.RectTransform.ScreenSpaceOffset;
				this.ReadyCheckButton.DrawManually(spriteBatch, false, true);
				if (ReadyCheck.ReadyCheckCooldown > DateTime.Now)
				{
					float progress = (float)(ReadyCheck.ReadyCheckCooldown - DateTime.Now).Seconds / 60f;
					this.ReadyCheckButton.Color = ToolBox.GradientLerp(progress, new Color[]
					{
						Color.White,
						GUIStyle.Red
					});
				}
			}
		}

		// Token: 0x06000EC6 RID: 3782 RVA: 0x0008B614 File Offset: 0x00089814
		public Task SelectSummaryScreen(RoundSummary roundSummary, LevelData newLevel, bool mirror, Action action)
		{
			CampaignMode.<>c__DisplayClass36_0 CS$<>8__locals1 = new CampaignMode.<>c__DisplayClass36_0();
			CS$<>8__locals1.newLevel = newLevel;
			CS$<>8__locals1.mirror = mirror;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.action = action;
			CS$<>8__locals1.roundSummaryScreen = RoundSummaryScreen.Select(this.overlaySprite, roundSummary);
			GUI.ClearCursorWait();
			this.startRoundCancellationToken = new CancellationTokenSource();
			Task loadTask = Task.Run(delegate()
			{
				CampaignMode.<>c__DisplayClass36_0.<<SelectSummaryScreen>b__0>d <<SelectSummaryScreen>b__0>d;
				<<SelectSummaryScreen>b__0>d.<>t__builder = AsyncTaskMethodBuilder.Create();
				<<SelectSummaryScreen>b__0>d.<>4__this = CS$<>8__locals1;
				<<SelectSummaryScreen>b__0>d.<>1__state = -1;
				<<SelectSummaryScreen>b__0>d.<>t__builder.Start<CampaignMode.<>c__DisplayClass36_0.<<SelectSummaryScreen>b__0>d>(ref <<SelectSummaryScreen>b__0>d);
				return <<SelectSummaryScreen>b__0>d.<>t__builder.Task;
			}, this.startRoundCancellationToken.Token);
			TaskPool.Add("AsyncCampaignStartRound", loadTask, delegate(Task t)
			{
				CS$<>8__locals1.<>4__this.overlayColor = Color.Transparent;
				Action action2 = CS$<>8__locals1.action;
				if (action2 == null)
				{
					return;
				}
				action2();
			});
			return loadTask;
		}

		// Token: 0x06000EC7 RID: 3783 RVA: 0x0008B69C File Offset: 0x0008989C
		public void CancelStartRound()
		{
			CancellationTokenSource cancellationTokenSource = this.startRoundCancellationToken;
			if (cancellationTokenSource == null)
			{
				return;
			}
			cancellationTokenSource.Cancel();
		}

		// Token: 0x06000EC8 RID: 3784 RVA: 0x0008B6B0 File Offset: 0x000898B0
		public void ThrowIfStartRoundCancellationRequested()
		{
			if (this.startRoundCancellationToken != null && this.startRoundCancellationToken.Token.IsCancellationRequested)
			{
				this.startRoundCancellationToken.Token.ThrowIfCancellationRequested();
				this.startRoundCancellationToken = null;
			}
		}

		// Token: 0x06000EC9 RID: 3785 RVA: 0x0008B6F4 File Offset: 0x000898F4
		public override void AddToGUIUpdateList()
		{
			if (this.ShowCampaignUI || this.ForceMapUI)
			{
				GUIFrame guiframe = this.campaignUIContainer;
				if (guiframe != null)
				{
					guiframe.AddToGUIUpdateList(false, 0);
				}
				CampaignUI campaignUI = this.CampaignUI;
				bool flag;
				if (campaignUI == null)
				{
					flag = (null != null);
				}
				else
				{
					UpgradeStore upgradeStore = campaignUI.UpgradeStore;
					flag = (((upgradeStore != null) ? upgradeStore.HoveredEntity : null) != null);
				}
				if (flag)
				{
					if (this.CampaignUI.SelectedTab != CampaignMode.InteractionType.Upgrade)
					{
						return;
					}
					CampaignUI campaignUI2 = this.CampaignUI;
					if (campaignUI2 != null)
					{
						UpgradeStore upgradeStore2 = campaignUI2.UpgradeStore;
						if (upgradeStore2 != null)
						{
							upgradeStore2.ItemInfoFrame.AddToGUIUpdateList(false, 1);
						}
					}
				}
			}
			base.AddToGUIUpdateList();
			base.CrewManager.AddToGUIUpdateList();
			this.endRoundButton.AddToGUIUpdateList(false, 0);
			GUIButton readyCheckButton = this.ReadyCheckButton;
			if (readyCheckButton == null)
			{
				return;
			}
			readyCheckButton.AddToGUIUpdateList(false, 0);
		}

		// Token: 0x06000ECA RID: 3786 RVA: 0x0008B7A8 File Offset: 0x000899A8
		protected void TryEndRoundWithFuelCheck(Action onConfirm, Action onReturnToMapScreen)
		{
			CampaignMode.<>c__DisplayClass40_0 CS$<>8__locals1 = new CampaignMode.<>c__DisplayClass40_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.onConfirm = onConfirm;
			CS$<>8__locals1.onReturnToMapScreen = onReturnToMapScreen;
			if (Submarine.MainSub == null)
			{
				return;
			}
			Submarine.MainSub.CheckFuel();
			bool lowFuel = Submarine.MainSub.Info.LowFuel;
			if (this.PendingSubmarineSwitch != null)
			{
				lowFuel = (this.TransferItemsOnSubSwitch ? (lowFuel && this.PendingSubmarineSwitch.LowFuel) : this.PendingSubmarineSwitch.LowFuel);
			}
			if (Level.IsLoadedFriendlyOutpost && lowFuel)
			{
				if (this.CargoManager.PurchasedItems.None((KeyValuePair<Identifier, List<PurchasedItem>> i) => i.Value.Any((PurchasedItem pi) => pi.ItemPrefab.Tags.Contains(Tags.ReactorFuel))))
				{
					GUIMessageBox extraConfirmationBox = new GUIMessageBox(TextManager.Get("lowfuelheader"), TextManager.Get("lowfuelwarning"), new LocalizedString[]
					{
						TextManager.Get("ok"),
						TextManager.Get("cancel")
					}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
					extraConfirmationBox.Buttons[0].OnClicked = delegate(GUIButton b, object o)
					{
						base.<TryEndRoundWithFuelCheck>g__Confirm|1();
						return true;
					};
					GUIButton guibutton = extraConfirmationBox.Buttons[0];
					guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(extraConfirmationBox.Close));
					extraConfirmationBox.Buttons[1].OnClicked = new GUIButton.OnClickedHandler(extraConfirmationBox.Close);
					return;
				}
			}
			CS$<>8__locals1.<TryEndRoundWithFuelCheck>g__Confirm|1();
		}

		// Token: 0x06000ECB RID: 3787 RVA: 0x0008B938 File Offset: 0x00089B38
		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);
			MedicalClinic medicalClinic = this.MedicalClinic;
			if (medicalClinic != null)
			{
				medicalClinic.Update(deltaTime);
			}
			if (PlayerInput.KeyHit(Keys.Escape))
			{
				GUIMessageBox.MessageBoxes.RemoveAll((GUIComponent mb) => mb.UserData is RoundSummary);
			}
			if (this.ShowCampaignUI || this.ForceMapUI)
			{
				CampaignUI campaignUI = this.CampaignUI;
				if (campaignUI == null)
				{
					return;
				}
				campaignUI.Update(deltaTime);
			}
		}

		// Token: 0x06000ECC RID: 3788 RVA: 0x0008B9B2 File Offset: 0x00089BB2
		public static bool HostileFactionDisablesInteraction(CampaignMode.InteractionType interactionType)
		{
			return interactionType != CampaignMode.InteractionType.None && interactionType != CampaignMode.InteractionType.Store && interactionType != CampaignMode.InteractionType.Examine;
		}

		// Token: 0x06000ECD RID: 3789 RVA: 0x0008B9C4 File Offset: 0x00089BC4
		public static bool BlocksInteraction(CampaignMode.InteractionType interactionType)
		{
			return interactionType != CampaignMode.InteractionType.None && interactionType != CampaignMode.InteractionType.Cargo;
		}

		// Token: 0x170003E7 RID: 999
		// (get) Token: 0x06000ECE RID: 3790 RVA: 0x0008B9D3 File Offset: 0x00089BD3
		public IReadOnlyList<Faction> Factions
		{
			get
			{
				return this.factions;
			}
		}

		// Token: 0x170003E8 RID: 1000
		// (get) Token: 0x06000ECF RID: 3791 RVA: 0x0008B9DB File Offset: 0x00089BDB
		// (set) Token: 0x06000ED0 RID: 3792 RVA: 0x0008B9E3 File Offset: 0x00089BE3
		protected XElement ActiveOrdersElement { get; set; }

		// Token: 0x170003E9 RID: 1001
		// (get) Token: 0x06000ED1 RID: 3793 RVA: 0x0008B9EC File Offset: 0x00089BEC
		// (set) Token: 0x06000ED2 RID: 3794 RVA: 0x0008B9F4 File Offset: 0x00089BF4
		public bool IsFirstRound { get; protected set; } = true;

		// Token: 0x170003EA RID: 1002
		// (get) Token: 0x06000ED3 RID: 3795 RVA: 0x0008B9FD File Offset: 0x00089BFD
		public bool DisableEvents
		{
			get
			{
				return this.IsFirstRound && GameMain.GameSession.RoundDuration < 0f;
			}
		}

		// Token: 0x170003EB RID: 1003
		// (get) Token: 0x06000ED4 RID: 3796 RVA: 0x0008BA1A File Offset: 0x00089C1A
		// (set) Token: 0x06000ED5 RID: 3797 RVA: 0x0008BA22 File Offset: 0x00089C22
		public bool TransferItemsOnSubSwitch { get; set; }

		// Token: 0x170003EC RID: 1004
		// (get) Token: 0x06000ED6 RID: 3798 RVA: 0x0008BA2B File Offset: 0x00089C2B
		// (set) Token: 0x06000ED7 RID: 3799 RVA: 0x0008BA33 File Offset: 0x00089C33
		public bool SwitchedSubsThisRound { get; private set; }

		// Token: 0x170003ED RID: 1005
		// (get) Token: 0x06000ED8 RID: 3800 RVA: 0x0008BA3C File Offset: 0x00089C3C
		public Map Map
		{
			get
			{
				return this.map;
			}
		}

		// Token: 0x170003EE RID: 1006
		// (get) Token: 0x06000ED9 RID: 3801 RVA: 0x0008BA44 File Offset: 0x00089C44
		public override IEnumerable<Mission> Missions
		{
			get
			{
				CampaignMode.<get_Missions>d__95 <get_Missions>d__ = new CampaignMode.<get_Missions>d__95(-2);
				<get_Missions>d__.<>4__this = this;
				return <get_Missions>d__;
			}
		}

		// Token: 0x170003EF RID: 1007
		// (get) Token: 0x06000EDA RID: 3802 RVA: 0x0008BA61 File Offset: 0x00089C61
		public Location CurrentLocation
		{
			get
			{
				Map map = this.Map;
				if (map == null)
				{
					return null;
				}
				return map.CurrentLocation;
			}
		}

		// Token: 0x170003F0 RID: 1008
		// (get) Token: 0x06000EDB RID: 3803 RVA: 0x0008BA74 File Offset: 0x00089C74
		// (set) Token: 0x06000EDC RID: 3804 RVA: 0x0008BA7C File Offset: 0x00089C7C
		public LevelData NextLevel { get; protected set; }

		// Token: 0x170003F1 RID: 1009
		// (get) Token: 0x06000EDD RID: 3805 RVA: 0x0008BA85 File Offset: 0x00089C85
		// (set) Token: 0x06000EDE RID: 3806 RVA: 0x0008BA8D File Offset: 0x00089C8D
		public virtual bool PurchasedHullRepairs { get; set; }

		// Token: 0x170003F2 RID: 1010
		// (get) Token: 0x06000EDF RID: 3807 RVA: 0x0008BA96 File Offset: 0x00089C96
		// (set) Token: 0x06000EE0 RID: 3808 RVA: 0x0008BA9E File Offset: 0x00089C9E
		public virtual bool PurchasedLostShuttles { get; set; }

		// Token: 0x170003F3 RID: 1011
		// (get) Token: 0x06000EE1 RID: 3809 RVA: 0x0008BAA7 File Offset: 0x00089CA7
		// (set) Token: 0x06000EE2 RID: 3810 RVA: 0x0008BAAF File Offset: 0x00089CAF
		public virtual bool PurchasedItemRepairs { get; set; }

		// Token: 0x06000EE3 RID: 3811 RVA: 0x0008BAB8 File Offset: 0x00089CB8
		private static bool AnyOneAllowedToManageCampaign(ClientPermissions permissions)
		{
			if (GameMain.NetworkMember == null)
			{
				return true;
			}
			if (GameMain.NetworkMember.ConnectedClients.Count == 1)
			{
				return true;
			}
			if (GameMain.NetworkMember.GameStarted)
			{
				bool someOneHasPermissions = GameMain.NetworkMember.ConnectedClients.Any((Client c) => CampaignMode.IsOwner(c) || c.HasPermission(permissions));
				return !someOneHasPermissions || ((GameMain.GameSession == null || GameMain.GameSession.RoundDuration >= 60f) && GameMain.NetworkMember.ConnectedClients.None(delegate(Client c)
				{
					if (c.InGame)
					{
						Character character = c.Character;
						if (character != null && !character.IsIncapacitated && !character.IsDead)
						{
							return CampaignMode.IsOwner(c) || c.HasPermission(permissions);
						}
					}
					return false;
				}));
			}
			return GameMain.NetworkMember.ConnectedClients.None((Client c) => CampaignMode.IsOwner(c) || c.HasPermission(permissions));
		}

		// Token: 0x06000EE4 RID: 3812 RVA: 0x0008BB70 File Offset: 0x00089D70
		protected CampaignMode(GameModePreset preset, CampaignSettings settings) : base(preset)
		{
			CampaignMode <>4__this = this;
			this.Settings = settings;
			this.Bank = new Wallet(Option<Character>.None())
			{
				Balance = settings.InitialMoney
			};
			this.CargoManager = new CargoManager(this);
			this.MedicalClinic = new MedicalClinic(this);
			this.CampaignMetadata = new CampaignMetadata();
			Identifier messageIdentifier = new Identifier("money");
			this.OnMoneyChanged.RegisterOverwriteExisting(new Identifier("CampaignMoneyChangeNotification"), delegate(WalletChangedEvent e)
			{
				CampaignMode.<>c__DisplayClass121_1 CS$<>8__locals2;
				if (!e.ChangedData.BalanceChanged.TryUnwrap(out CS$<>8__locals2.changed))
				{
					return;
				}
				if (CS$<>8__locals2.changed == 0)
				{
					return;
				}
				CS$<>8__locals2.isGain = (CS$<>8__locals2.changed > 0);
				Color clr = CS$<>8__locals2.isGain ? GUIStyle.Yellow : GUIStyle.Red;
				Character owner;
				if (e.Owner.TryUnwrap(out owner))
				{
					owner.AddMessage(CampaignMode.<.ctor>g__FormatMessage|121_1(ref CS$<>8__locals2), clr, Character.Controlled == owner, messageIdentifier, new int?(CS$<>8__locals2.changed), 3f);
					return;
				}
				if (<>4__this.IsSinglePlayer)
				{
					Character controlled = Character.Controlled;
					if (controlled == null)
					{
						return;
					}
					controlled.AddMessage(CampaignMode.<.ctor>g__FormatMessage|121_1(ref CS$<>8__locals2), clr, true, messageIdentifier, new int?(CS$<>8__locals2.changed), 3f);
				}
			});
		}

		// Token: 0x06000EE5 RID: 3813 RVA: 0x0008BC3B File Offset: 0x00089E3B
		public virtual Wallet GetWallet(Client client = null)
		{
			return this.Bank;
		}

		// Token: 0x06000EE6 RID: 3814 RVA: 0x0008BC43 File Offset: 0x00089E43
		public virtual bool TryPurchase(Client client, int price)
		{
			return price == 0 || this.GetWallet(client).TryDeduct(price);
		}

		// Token: 0x06000EE7 RID: 3815 RVA: 0x0008BC57 File Offset: 0x00089E57
		public virtual int GetBalance(Client client = null)
		{
			return this.GetWallet(client).Balance;
		}

		// Token: 0x06000EE8 RID: 3816 RVA: 0x0008BC65 File Offset: 0x00089E65
		public bool CanAfford(int cost, Client client = null)
		{
			return this.GetBalance(client) >= cost;
		}

		// Token: 0x06000EE9 RID: 3817 RVA: 0x0008BC74 File Offset: 0x00089E74
		public Location GetCurrentDisplayLocation()
		{
			Level loaded = Level.Loaded;
			LevelData levelData;
			Submarine submarine;
			if (((loaded != null) ? loaded.EndLocation : null) != null && !Level.Loaded.Generating && Level.Loaded.Type == LevelData.LevelType.LocationConnection && this.GetAvailableTransition(out levelData, out submarine) == CampaignMode.TransitionType.ProgressToNextEmptyLocation)
			{
				return Level.Loaded.EndLocation;
			}
			Level loaded2 = Level.Loaded;
			return ((loaded2 != null) ? loaded2.StartLocation : null) ?? this.Map.CurrentLocation;
		}

		// Token: 0x06000EEA RID: 3818 RVA: 0x0008BCE8 File Offset: 0x00089EE8
		public static List<Submarine> GetSubsToLeaveBehind(Submarine leavingSub)
		{
			return Submarine.Loaded.FindAll((Submarine sub) => sub != leavingSub && !leavingSub.DockedTo.Contains(sub) && sub.Info.Type == SubmarineType.Player && sub.TeamID == CharacterTeamType.Team1 && !sub.IsRespawnShuttle && (sub.AtEndExit != leavingSub.AtEndExit || sub.AtStartExit != leavingSub.AtStartExit));
		}

		// Token: 0x06000EEB RID: 3819 RVA: 0x0008BD18 File Offset: 0x00089F18
		public SubmarineInfo GetPredefinedStartOutpost()
		{
			Map map = this.Map;
			OutpostGenerationParams outpostGenerationParams;
			if (map == null)
			{
				outpostGenerationParams = null;
			}
			else
			{
				Location currentLocation = map.CurrentLocation;
				if (currentLocation == null)
				{
					outpostGenerationParams = null;
				}
				else
				{
					LocationType type = currentLocation.Type;
					outpostGenerationParams = ((type != null) ? type.GetForcedOutpostGenerationParams() : null);
				}
			}
			OutpostGenerationParams parameters = outpostGenerationParams;
			if (parameters != null && !parameters.OutpostFilePath.IsNullOrEmpty())
			{
				return new SubmarineInfo(parameters.OutpostFilePath.Value, "", null, true, false)
				{
					OutpostGenerationParams = parameters
				};
			}
			return null;
		}

		// Token: 0x06000EEC RID: 3820 RVA: 0x0008BD84 File Offset: 0x00089F84
		public override void Start()
		{
			base.Start();
			this.dialogLastSpoken.Clear();
			this.characterOutOfBoundsTimer.Clear();
			this.prevCampaignUIAutoOpenType = CampaignMode.TransitionType.None;
			foreach (Faction faction in this.factions)
			{
				faction.Reputation.ReputationAtRoundStart = faction.Reputation.Value;
			}
			if (this.PurchasedHullRepairsInLatestSave)
			{
				foreach (Structure wall in Structure.WallList)
				{
					if (wall.Submarine != null && wall.Submarine.Info.Type == SubmarineType.Player && (wall.Submarine == Submarine.MainSub || Submarine.MainSub.DockedTo.Contains(wall.Submarine)))
					{
						for (int i = 0; i < wall.SectionCount; i++)
						{
							wall.SetDamage(i, 0f, null, false, true, false, false);
						}
					}
				}
				this.PurchasedHullRepairsInLatestSave = (this.PurchasedHullRepairs = false);
			}
			if (this.PurchasedItemRepairsInLatestSave)
			{
				foreach (Item item in Item.ItemList)
				{
					if (item.Submarine != null && item.Submarine.Info.Type == SubmarineType.Player && (item.Submarine == Submarine.MainSub || Submarine.MainSub.DockedTo.Contains(item.Submarine)) && item.GetComponent<Repairable>() != null)
					{
						item.Condition = item.MaxCondition;
					}
				}
				this.PurchasedItemRepairsInLatestSave = (this.PurchasedItemRepairs = false);
			}
			this.PurchasedLostShuttlesInLatestSave = (this.PurchasedLostShuttles = false);
			IEnumerable<Submarine> connectedSubs = Submarine.MainSub.GetConnectedSubs();
			this.wasDocked = (Level.Loaded.StartOutpost != null && connectedSubs.Contains(Level.Loaded.StartOutpost));
			this.SwitchedSubsThisRound = false;
		}

		// Token: 0x06000EED RID: 3821 RVA: 0x0008BFC8 File Offset: 0x0008A1C8
		public static int GetHullRepairCost()
		{
			float totalDamage = 0f;
			foreach (Structure wall in Structure.WallList)
			{
				if (wall.Submarine != null && wall.Submarine.Info.Type == SubmarineType.Player && (wall.Submarine == Submarine.MainSub || Submarine.MainSub.DockedTo.Contains(wall.Submarine)))
				{
					for (int i = 0; i < wall.SectionCount; i++)
					{
						totalDamage += wall.SectionDamage(i);
					}
				}
			}
			return (int)Math.Min(totalDamage * 0.1f, 600f);
		}

		// Token: 0x06000EEE RID: 3822 RVA: 0x0008C084 File Offset: 0x0008A284
		public static int GetItemRepairCost()
		{
			float totalRepairDuration = 0f;
			foreach (Item item in Item.ItemList)
			{
				if (item.Submarine != null && item.Submarine.Info.Type == SubmarineType.Player && (item.Submarine == Submarine.MainSub || Submarine.MainSub.DockedTo.Contains(item.Submarine)))
				{
					Repairable repairable = item.GetComponent<Repairable>();
					if (repairable != null)
					{
						totalRepairDuration += repairable.FixDurationHighSkill * (1f - item.Condition / item.MaxCondition);
					}
				}
			}
			return (int)Math.Min(totalRepairDuration * 1f, 2000f);
		}

		// Token: 0x06000EEF RID: 3823 RVA: 0x0008C14C File Offset: 0x0008A34C
		public void InitFactions()
		{
			this.factions = new List<Faction>();
			foreach (FactionPrefab factionPrefab in FactionPrefab.Prefabs)
			{
				this.factions.Add(new Faction(this.CampaignMetadata, factionPrefab));
			}
		}

		// Token: 0x1400000A RID: 10
		// (add) Token: 0x06000EF0 RID: 3824 RVA: 0x0008C1B4 File Offset: 0x0008A3B4
		// (remove) Token: 0x06000EF1 RID: 3825 RVA: 0x0008C1EC File Offset: 0x0008A3EC
		public event Action BeforeLevelLoading;

		// Token: 0x1400000B RID: 11
		// (add) Token: 0x06000EF2 RID: 3826 RVA: 0x0008C224 File Offset: 0x0008A424
		// (remove) Token: 0x06000EF3 RID: 3827 RVA: 0x0008C25C File Offset: 0x0008A45C
		public event Action OnSaveAndQuit;

		// Token: 0x06000EF4 RID: 3828 RVA: 0x0008C294 File Offset: 0x0008A494
		public override void AddExtraMissions(LevelData levelData)
		{
			if (levelData == null)
			{
				throw new ArgumentException("Level data was null.");
			}
			this.extraMissions.Clear();
			Location currentLocation = this.Map.CurrentLocation;
			if (currentLocation == null)
			{
				throw new InvalidOperationException("Current location was null.");
			}
			if (levelData.Type == LevelData.LevelType.Outpost)
			{
				using (IEnumerator<Mission> enumerator = currentLocation.AvailableMissions.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Mission availableMission = enumerator.Current;
						if (availableMission.Locations[0] == currentLocation && availableMission.Locations[1] == currentLocation)
						{
							currentLocation.SelectMission(availableMission);
						}
					}
					goto IL_7CB;
				}
			}
			foreach (Mission mission in currentLocation.SelectedMissions.ToList<Mission>())
			{
				if (mission.Locations[0] == currentLocation && mission.Locations[1] == currentLocation)
				{
					currentLocation.DeselectMission(mission);
				}
			}
			foreach (Mission mission2 in currentLocation.AvailableMissions)
			{
				if (!mission2.Prefab.ShowInMenus || mission2.Prefab.IsSideObjective)
				{
					currentLocation.SelectMission(mission2);
				}
			}
			if (levelData.HasBeaconStation && !levelData.IsBeaconActive)
			{
				if (this.Missions.None(delegate(Mission m)
				{
					Identifier type2 = m.Prefab.Type;
					return type2 == Tags.MissionTypeBeacon;
				}))
				{
					IEnumerable<MissionPrefab> beaconMissionPrefabs = MissionPrefab.Prefabs.Where(delegate(MissionPrefab m)
					{
						if (m.IsSideObjective)
						{
							Identifier type2 = m.Type;
							return type2 == Tags.MissionTypeBeacon;
						}
						return false;
					});
					if (beaconMissionPrefabs.Any<MissionPrefab>())
					{
						IEnumerable<MissionPrefab> filteredMissions = from m in beaconMissionPrefabs
						where levelData.Difficulty >= (float)m.MinLevelDifficulty && levelData.Difficulty <= (float)m.MaxLevelDifficulty
						select m;
						if (filteredMissions.None(null))
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(90, 1);
							defaultInterpolatedStringHandler.AppendLiteral("No suitable beacon mission found matching the level difficulty ");
							defaultInterpolatedStringHandler.AppendFormatted<float>(levelData.Difficulty);
							defaultInterpolatedStringHandler.AppendLiteral(". Ignoring the restriction.");
							DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
						}
						else
						{
							beaconMissionPrefabs = filteredMissions;
						}
						Random rand = new MTRandom(ToolBox.StringToInt(levelData.Seed));
						MissionPrefab beaconMissionPrefab = ToolBox.SelectWeightedRandom<MissionPrefab>(beaconMissionPrefabs, (MissionPrefab p) => (float)p.Commonness, rand);
						this.extraMissions.Add(beaconMissionPrefab.Instantiate(this.Map.SelectedConnection.Locations, Submarine.MainSub));
					}
				}
			}
			if (levelData.HasHuntingGrounds)
			{
				IOrderedEnumerable<MissionPrefab> huntingGroundsMissionPrefabs = from m in MissionPrefab.Prefabs
				where m.IsSideObjective && m.Tags.Contains("huntinggrounds")
				orderby m.UintIdentifier
				select m;
				if (!huntingGroundsMissionPrefabs.Any<MissionPrefab>())
				{
					DebugConsole.AddWarning("Could not find a hunting grounds mission for the level. No mission with the tag \"huntinggrounds\" found.", null);
				}
				else
				{
					Random rand2 = new MTRandom(ToolBox.StringToInt(levelData.Seed));
					List<MissionPrefab> prefabs = huntingGroundsMissionPrefabs.ToList<MissionPrefab>();
					List<float> weights = (from p in prefabs
					select (float)Math.Max(p.Commonness, 1)).ToList<float>();
					for (int i = 0; i < prefabs.Count; i++)
					{
						MissionPrefab prefab = prefabs[i];
						float weight = weights[i];
						if (prefab.Tags.Contains("easy"))
						{
							weight *= MathHelper.Lerp(0.2f, 2f, MathUtils.InverseLerp(80f, 25f, levelData.Difficulty));
						}
						else if (prefab.Tags.Contains("hard"))
						{
							weight *= MathHelper.Lerp(0.5f, 1.5f, MathUtils.InverseLerp(35f, 80f, levelData.Difficulty));
						}
						weights[i] = weight;
					}
					MissionPrefab huntingGroundsMissionPrefab = ToolBox.SelectWeightedRandom<MissionPrefab>(prefabs, weights, rand2);
					if (!this.Missions.Any((Mission m) => m.Prefab.Tags.Contains("huntinggrounds")))
					{
						this.extraMissions.Add(huntingGroundsMissionPrefab.Instantiate(this.Map.SelectedConnection.Locations, Submarine.MainSub));
					}
				}
			}
			using (IEnumerator<Faction> enumerator4 = (from f in this.factions
			orderby f.Prefab.MenuOrder
			select f).GetEnumerator())
			{
				while (enumerator4.MoveNext())
				{
					Faction faction = enumerator4.Current;
					ImmutableArray<FactionPrefab.AutomaticMission>.Enumerator enumerator5 = faction.Prefab.AutomaticMissions.GetEnumerator();
					Func<Location, bool> <>9__9;
					Func<Location, bool> <>9__10;
					while (enumerator5.MoveNext())
					{
						FactionPrefab.AutomaticMission automaticMission = enumerator5.Current;
						if (faction.Reputation.Value >= automaticMission.MinReputation && faction.Reputation.Value <= automaticMission.MaxReputation)
						{
							if (automaticMission.DisallowBetweenOtherFactionOutposts && levelData.Type == LevelData.LevelType.LocationConnection)
							{
								IEnumerable<Location> locations = this.Map.SelectedConnection.Locations;
								Func<Location, bool> predicate;
								if ((predicate = <>9__9) == null)
								{
									predicate = (<>9__9 = ((Location l) => l.Faction != null && l.Faction != faction));
								}
								if (locations.All(predicate))
								{
									continue;
								}
							}
							if (automaticMission.MaxDistanceFromFactionOutpost < 2147483647)
							{
								Location startLocation = currentLocation;
								int maxDistanceFromFactionOutpost = automaticMission.MaxDistanceFromFactionOutpost;
								Func<Location, bool> criteria;
								if ((criteria = <>9__10) == null)
								{
									criteria = (<>9__10 = ((Location loc) => loc.Faction == faction));
								}
								if (!Map.LocationOrConnectionWithinDistance(startLocation, maxDistanceFromFactionOutpost, criteria, null))
								{
									continue;
								}
							}
							Random rand3 = new MTRandom(ToolBox.StringToInt(levelData.Seed + this.TotalPassedLevels.ToString()));
							if (levelData.Type == automaticMission.LevelType)
							{
								float probability = MathHelper.Lerp(automaticMission.MinProbability, automaticMission.MaxProbability, MathUtils.InverseLerp(automaticMission.MinReputation, automaticMission.MaxReputation, faction.Reputation.Value));
								if (rand3.NextDouble() < (double)probability)
								{
									Func<Identifier, bool> <>9__13;
									IOrderedEnumerable<MissionPrefab> missionPrefabs = from m in MissionPrefab.Prefabs.Where(delegate(MissionPrefab m)
									{
										IEnumerable<Identifier> tags = m.Tags;
										Func<Identifier, bool> predicate2;
										if ((predicate2 = <>9__13) == null)
										{
											predicate2 = (<>9__13 = ((Identifier t) => t == automaticMission.MissionTag));
										}
										return tags.Any(predicate2);
									})
									orderby m.UintIdentifier
									select m;
									if (missionPrefabs.Any<MissionPrefab>())
									{
										MissionPrefab missionPrefab = ToolBox.SelectWeightedRandom<MissionPrefab>(missionPrefabs, (MissionPrefab p) => (float)p.Commonness, rand3);
										Identifier type = missionPrefab.Type;
										if (type == Tags.MissionTypePirate)
										{
											if (this.Missions.Any(delegate(Mission m)
											{
												Identifier type2 = m.Prefab.Type;
												return type2 == Tags.MissionTypePirate;
											}))
											{
												continue;
											}
										}
										if (automaticMission.LevelType == LevelData.LevelType.Outpost)
										{
											this.extraMissions.Add(missionPrefab.Instantiate(new Location[]
											{
												currentLocation,
												currentLocation
											}, Submarine.MainSub));
										}
										else
										{
											this.extraMissions.Add(missionPrefab.Instantiate(this.Map.SelectedConnection.Locations, Submarine.MainSub));
										}
									}
								}
							}
						}
					}
				}
			}
			IL_7CB:
			if (levelData.Biome.IsEndBiome)
			{
				Identifier endMissionTag = Identifier.Empty;
				if (levelData.Type == LevelData.LevelType.LocationConnection)
				{
					int locationIndex = this.map.EndLocations.IndexOf(this.map.SelectedLocation);
					if (locationIndex > -1)
					{
						endMissionTag = ("endlevel_locationconnection_" + locationIndex.ToString()).ToIdentifier();
					}
				}
				else
				{
					int locationIndex2 = this.map.EndLocations.IndexOf(this.map.CurrentLocation);
					if (locationIndex2 > -1)
					{
						endMissionTag = ("endlevel_location_" + locationIndex2.ToString()).ToIdentifier();
					}
				}
				if (!endMissionTag.IsEmpty)
				{
					IOrderedEnumerable<MissionPrefab> endLevelMissionPrefabs = from m in MissionPrefab.Prefabs
					where m.Tags.Contains(endMissionTag)
					orderby m.UintIdentifier
					select m;
					if (endLevelMissionPrefabs.Any<MissionPrefab>())
					{
						Random rand4 = new MTRandom(ToolBox.StringToInt(levelData.Seed));
						MissionPrefab endLevelMissionPrefab = ToolBox.SelectWeightedRandom<MissionPrefab>(endLevelMissionPrefabs, (MissionPrefab p) => (float)p.Commonness, rand4);
						if (this.Missions.All(delegate(Mission m)
						{
							Identifier type2 = m.Prefab.Type;
							Identifier type3 = endLevelMissionPrefab.Type;
							return type2 != type3;
						}))
						{
							if (levelData.Type == LevelData.LevelType.LocationConnection)
							{
								this.extraMissions.Add(endLevelMissionPrefab.Instantiate(this.map.SelectedConnection.Locations, Submarine.MainSub));
								return;
							}
							this.extraMissions.Add(endLevelMissionPrefab.Instantiate(new Location[]
							{
								this.map.CurrentLocation,
								this.map.CurrentLocation
							}, Submarine.MainSub));
						}
					}
				}
			}
		}

		// Token: 0x06000EF5 RID: 3829 RVA: 0x0008CCC0 File Offset: 0x0008AEC0
		public void LoadNewLevel()
		{
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			if (CoroutineManager.IsCoroutineRunning("LevelTransition"))
			{
				DebugConsole.ThrowError("Level transition already running.\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			Action beforeLevelLoading = this.BeforeLevelLoading;
			if (beforeLevelLoading != null)
			{
				beforeLevelLoading();
			}
			this.BeforeLevelLoading = null;
			if (Level.Loaded == null || Submarine.MainSub == null)
			{
				this.LoadInitialLevel();
				return;
			}
			LevelData nextLevel;
			Submarine leavingSub;
			CampaignMode.TransitionType availableTransition = this.GetAvailableTransition(out nextLevel, out leavingSub);
			if (availableTransition == CampaignMode.TransitionType.None)
			{
				LocalizedString left = "Failed to load a new campaign level. No available level transitions (current location: ";
				Location currentLocation = this.map.CurrentLocation;
				LocalizedString left2 = left + (((currentLocation != null) ? currentLocation.DisplayName : null) ?? "null") + ", " + "selected location: ";
				Location selectedLocation = this.map.SelectedLocation;
				LocalizedString left3 = left2 + (((selectedLocation != null) ? selectedLocation.DisplayName : null) ?? "null") + ", " + "leaving sub: ";
				string text;
				if (leavingSub == null)
				{
					text = null;
				}
				else
				{
					SubmarineInfo info = leavingSub.Info;
					text = ((info != null) ? info.Name : null);
				}
				DebugConsole.ThrowErrorLocalized(left3 + (text ?? "null") + ", " + "at start: " + (((leavingSub != null) ? leavingSub.AtStartExit.ToString() : null) ?? "null") + ", " + "at end: " + (((leavingSub != null) ? leavingSub.AtEndExit.ToString() : null) ?? "null") + ")\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			if (nextLevel == null)
			{
				LocalizedString left4 = "Failed to load a new campaign level. No available level transitions (transition type: " + availableTransition.ToString() + ", current location: ";
				Location currentLocation2 = this.map.CurrentLocation;
				LocalizedString left5 = left4 + (((currentLocation2 != null) ? currentLocation2.DisplayName : null) ?? "null") + ", " + "selected location: ";
				Location selectedLocation2 = this.map.SelectedLocation;
				LocalizedString left6 = left5 + (((selectedLocation2 != null) ? selectedLocation2.DisplayName : null) ?? "null") + ", " + "leaving sub: ";
				string text2;
				if (leavingSub == null)
				{
					text2 = null;
				}
				else
				{
					SubmarineInfo info2 = leavingSub.Info;
					text2 = ((info2 != null) ? info2.Name : null);
				}
				DebugConsole.ThrowErrorLocalized(left6 + (text2 ?? "null") + ", " + "at start: " + (((leavingSub != null) ? leavingSub.AtStartExit.ToString() : null) ?? "null") + ", " + "at end: " + (((leavingSub != null) ? leavingSub.AtEndExit.ToString() : null) ?? "null") + ")\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			this.ShowCampaignUI = (this.ForceMapUI = false);
			LocalizedString left7 = "Transitioning to " + (((nextLevel != null) ? nextLevel.Seed : null) ?? "null") + " (current location: ";
			Location currentLocation3 = this.map.CurrentLocation;
			LocalizedString left8 = left7 + (((currentLocation3 != null) ? currentLocation3.DisplayName : null) ?? "null") + ", " + "selected location: ";
			Location selectedLocation3 = this.map.SelectedLocation;
			LocalizedString left9 = left8 + (((selectedLocation3 != null) ? selectedLocation3.DisplayName : null) ?? "null") + ", " + "leaving sub: ";
			string text3;
			if (leavingSub == null)
			{
				text3 = null;
			}
			else
			{
				SubmarineInfo info3 = leavingSub.Info;
				text3 = ((info3 != null) ? info3.Name : null);
			}
			DebugConsole.NewMessage(left9 + (text3 ?? "null") + ", " + "at start: " + (((leavingSub != null) ? leavingSub.AtStartExit.ToString() : null) ?? "null") + ", " + "at end: " + (((leavingSub != null) ? leavingSub.AtEndExit.ToString() : null) ?? "null") + ", " + "transition type: " + availableTransition + ")", null, false);
			this.IsFirstRound = false;
			bool mirror = this.map.SelectedConnection != null && this.map.CurrentLocation != this.map.SelectedConnection.Locations[0];
			CoroutineManager.StartCoroutine(this.DoLevelTransition(availableTransition, nextLevel, leavingSub, mirror), "LevelTransition");
		}

		// Token: 0x06000EF6 RID: 3830
		protected abstract void LoadInitialLevel();

		// Token: 0x06000EF7 RID: 3831
		protected abstract IEnumerable<CoroutineStatus> DoLevelTransition(CampaignMode.TransitionType transitionType, LevelData newLevel, Submarine leavingSub, bool mirror);

		// Token: 0x06000EF8 RID: 3832 RVA: 0x0008D27C File Offset: 0x0008B47C
		public CampaignMode.TransitionType GetAvailableTransition(out LevelData nextLevel, out Submarine leavingSub)
		{
			if (Level.Loaded == null || Submarine.MainSub == null)
			{
				nextLevel = null;
				leavingSub = null;
				return CampaignMode.TransitionType.None;
			}
			leavingSub = CampaignMode.GetLeavingSub();
			if (leavingSub == null)
			{
				nextLevel = null;
				return CampaignMode.TransitionType.None;
			}
			if (Level.Loaded.Type == LevelData.LevelType.LocationConnection)
			{
				if (leavingSub.AtEndExit)
				{
					if (Level.Loaded.EndLocation != null && Level.Loaded.EndLocation.Type.HasOutpost && Level.Loaded.EndOutpost != null)
					{
						nextLevel = Level.Loaded.EndLocation.LevelData;
						return CampaignMode.TransitionType.ProgressToNextLocation;
					}
					if (this.map.SelectedConnection != null)
					{
						nextLevel = this.map.SelectedConnection.LevelData;
						return CampaignMode.TransitionType.ProgressToNextEmptyLocation;
					}
					nextLevel = null;
					return CampaignMode.TransitionType.ProgressToNextEmptyLocation;
				}
				else
				{
					if (!leavingSub.AtStartExit)
					{
						nextLevel = null;
						return CampaignMode.TransitionType.None;
					}
					if (this.map.CurrentLocation.Type.HasOutpost && Level.Loaded.StartOutpost != null)
					{
						nextLevel = this.map.CurrentLocation.LevelData;
						return CampaignMode.TransitionType.ReturnToPreviousLocation;
					}
					if (this.map.SelectedLocation != null && this.map.SelectedLocation != this.map.CurrentLocation && !this.map.CurrentLocation.Type.HasOutpost && this.map.SelectedConnection != null && Level.Loaded.LevelData != this.map.SelectedConnection.LevelData)
					{
						nextLevel = this.map.SelectedConnection.LevelData;
						return CampaignMode.TransitionType.LeaveLocation;
					}
					LocationConnection selectedConnection = this.map.SelectedConnection;
					nextLevel = ((selectedConnection != null) ? selectedConnection.LevelData : null);
					return CampaignMode.TransitionType.ReturnToPreviousEmptyLocation;
				}
			}
			else
			{
				if (Level.Loaded.Type != LevelData.LevelType.Outpost)
				{
					throw new NotImplementedException();
				}
				int currentEndLocationIndex = this.map.EndLocations.IndexOf(this.map.CurrentLocation);
				if (currentEndLocationIndex > -1)
				{
					if (currentEndLocationIndex == this.map.EndLocations.Count - 1)
					{
						Location startLocation = this.map.StartLocation;
						nextLevel = ((startLocation != null) ? startLocation.LevelData : null);
						return CampaignMode.TransitionType.End;
					}
					if (leavingSub.AtEndExit && currentEndLocationIndex < this.map.EndLocations.Count - 1)
					{
						Location location = this.map.EndLocations[currentEndLocationIndex + 1];
						nextLevel = ((location != null) ? location.LevelData : null);
						return CampaignMode.TransitionType.ProgressToNextLocation;
					}
					nextLevel = null;
					return CampaignMode.TransitionType.None;
				}
				else
				{
					LevelData levelData;
					if (this.map.SelectedLocation != null)
					{
						LocationConnection selectedConnection2 = this.map.SelectedConnection;
						levelData = ((selectedConnection2 != null) ? selectedConnection2.LevelData : null);
					}
					else
					{
						levelData = null;
					}
					nextLevel = levelData;
					if (nextLevel != null)
					{
						return CampaignMode.TransitionType.LeaveLocation;
					}
					return CampaignMode.TransitionType.None;
				}
			}
		}

		// Token: 0x06000EF9 RID: 3833 RVA: 0x0008D4E8 File Offset: 0x0008B6E8
		public CampaignMode.TransitionType GetAvailableTransition()
		{
			LevelData levelData;
			Submarine submarine;
			return this.GetAvailableTransition(out levelData, out submarine);
		}

		// Token: 0x06000EFA RID: 3834 RVA: 0x0008D500 File Offset: 0x0008B700
		private static Submarine GetLeavingSub()
		{
			if (Level.IsLoadedOutpost)
			{
				return Submarine.MainSub;
			}
			IEnumerable<Character> leavingPlayers = from c in Character.CharacterList
			where !c.IsDead && (c == Character.Controlled || c.IsRemotePlayer)
			select c;
			Character character = leavingPlayers.FirstOrDefault<Character>();
			CharacterTeamType submarineTeam = (character != null) ? character.TeamID : CharacterTeamType.Team1;
			Submarine leavingSubAtStart = CampaignMode.<GetLeavingSub>g__GetLeavingSubAtStart|145_3(leavingPlayers, submarineTeam);
			Submarine leavingSubAtEnd = CampaignMode.<GetLeavingSub>g__GetLeavingSubAtEnd|145_4(leavingPlayers, submarineTeam);
			int playersInSubAtStart = (leavingSubAtStart == null || !leavingSubAtStart.AtStartExit) ? 0 : leavingPlayers.Count((Character c) => c.Submarine == leavingSubAtStart || leavingSubAtStart.DockedTo.Contains(c.Submarine) || (Level.Loaded.StartOutpost != null && c.Submarine == Level.Loaded.StartOutpost));
			int playersInSubAtEnd = (leavingSubAtEnd == null || !leavingSubAtEnd.AtEndExit) ? 0 : leavingPlayers.Count((Character c) => c.Submarine == leavingSubAtEnd || leavingSubAtEnd.DockedTo.Contains(c.Submarine) || (Level.Loaded.EndOutpost != null && c.Submarine == Level.Loaded.EndOutpost));
			if (playersInSubAtStart == 0 && playersInSubAtEnd == 0)
			{
				return null;
			}
			if (playersInSubAtStart <= playersInSubAtEnd)
			{
				return leavingSubAtEnd;
			}
			return leavingSubAtStart;
		}

		// Token: 0x06000EFB RID: 3835 RVA: 0x0008D5EC File Offset: 0x0008B7EC
		public override void End(CampaignMode.TransitionType transitionType = CampaignMode.TransitionType.None)
		{
			List<Item> takenItems = new List<Item>();
			Level loaded = Level.Loaded;
			if (loaded != null && loaded.Type == LevelData.LevelType.Outpost)
			{
				foreach (Item item in Item.ItemList)
				{
					if (item.SpawnedInCurrentOutpost && item.OriginalModuleIndex >= 0)
					{
						Entity owner = item.GetRootInventoryOwner();
						bool? flag;
						if (owner == null)
						{
							flag = null;
						}
						else
						{
							Submarine submarine = owner.Submarine;
							if (submarine == null)
							{
								flag = null;
							}
							else
							{
								SubmarineInfo info = submarine.Info;
								flag = ((info != null) ? new bool?(info.IsOutpost) : null);
							}
						}
						bool? flag2 = flag;
						if (flag2.GetValueOrDefault())
						{
							Character character = owner as Character;
							if ((character == null || character.TeamID != CharacterTeamType.Team1) && item.Submarine != null && item.Submarine.Info.IsOutpost)
							{
								continue;
							}
						}
						takenItems.Add(item);
					}
				}
			}
			if (this.map != null && this.CargoManager != null)
			{
				this.map.CurrentLocation.RegisterTakenItems(takenItems);
				if (transitionType != CampaignMode.TransitionType.None)
				{
					this.UpdateStoreStock();
				}
			}
			if (GameMain.NetworkMember == null)
			{
				CargoManager cargoManager = this.CargoManager;
				if (cargoManager != null)
				{
					cargoManager.ClearItemsInBuyCrate();
				}
				CargoManager cargoManager2 = this.CargoManager;
				if (cargoManager2 != null)
				{
					cargoManager2.ClearItemsInSellCrate();
				}
				CargoManager cargoManager3 = this.CargoManager;
				if (cargoManager3 != null)
				{
					cargoManager3.ClearItemsInSellFromSubCrate();
				}
			}
			else if (GameMain.NetworkMember.IsServer)
			{
				CargoManager cargoManager4 = this.CargoManager;
				if (cargoManager4 != null)
				{
					cargoManager4.ClearItemsInBuyCrate();
				}
				CargoManager cargoManager5 = this.CargoManager;
				if (cargoManager5 != null)
				{
					cargoManager5.ClearItemsInSellFromSubCrate();
				}
			}
			else if (GameMain.NetworkMember.IsClient)
			{
				CargoManager cargoManager6 = this.CargoManager;
				if (cargoManager6 != null)
				{
					cargoManager6.ClearItemsInSellCrate();
				}
			}
			Level loaded2 = Level.Loaded;
			if (((loaded2 != null) ? loaded2.StartOutpost : null) != null)
			{
				List<Character> killedCharacters = new List<Character>();
				foreach (Character c3 in Level.Loaded.StartOutpost.Info.OutpostNPCs.SelectMany((KeyValuePair<Identifier, List<Character>> kpv) => kpv.Value))
				{
					if (c3.IsDead || c3.Removed)
					{
						killedCharacters.Add(c3);
					}
				}
				this.map.CurrentLocation.RegisterKilledCharacters(killedCharacters);
				Level.Loaded.StartOutpost.Info.OutpostNPCs.Clear();
			}
			List<Character> deadCharacters = Character.CharacterList.FindAll((Character c) => c.IsDead);
			foreach (Character c2 in deadCharacters)
			{
				if (c2.IsDead)
				{
					base.CrewManager.RemoveCharacterInfo(c2.Info);
					c2.DespawnNow(false);
				}
			}
			foreach (Item item2 in Item.ItemList.ToList<Item>())
			{
				if (item2.HasTag(Tags.IdCardTag))
				{
					Item container = item2.Container;
					if (container != null && container.HasTag(Tags.DespawnContainer))
					{
						item2.Remove();
					}
				}
			}
			foreach (CharacterInfo ci in base.CrewManager.GetCharacterInfos(false).ToList<CharacterInfo>())
			{
				if (ci.CauseOfDeath != null)
				{
					base.CrewManager.RemoveCharacterInfo(ci);
				}
			}
			foreach (DockingPort port in DockingPort.List)
			{
				if (port.Door != null & port.Item.Submarine.Info.Type == SubmarineType.Player)
				{
					DockingPort dockingTarget = port.DockingTarget;
					bool flag3;
					if (dockingTarget == null)
					{
						flag3 = (null != null);
					}
					else
					{
						Item item4 = dockingTarget.Item;
						flag3 = (((item4 != null) ? item4.Submarine : null) != null);
					}
					if (flag3 && port.DockingTarget.Item.Submarine.Info.IsOutpost)
					{
						port.Door.IsOpen = false;
					}
				}
			}
			foreach (Item item3 in Item.ItemList)
			{
				Submarine sub = item3.Submarine;
				if (sub != null && sub.Info.IsPlayer && (sub.TeamID == CharacterTeamType.Team1 || sub.TeamID == CharacterTeamType.Team2))
				{
					Reactor reactor = item3.GetComponent<Reactor>();
					if (reactor != null && reactor.LastAIUser != null && reactor.LastUser == reactor.LastAIUser)
					{
						reactor.AutoTemp = true;
					}
				}
			}
		}

		// Token: 0x06000EFC RID: 3836 RVA: 0x0008DB24 File Offset: 0x0008BD24
		public void HandleSaveAndQuit()
		{
			Action onSaveAndQuit = this.OnSaveAndQuit;
			if (onSaveAndQuit != null)
			{
				onSaveAndQuit();
			}
			this.OnSaveAndQuit = null;
			if (Level.IsLoadedFriendlyOutpost)
			{
				this.UpdateStoreStock();
			}
			GameMain.GameSession.EndMissions(CampaignMode.TransitionType.None);
			EventManager eventManager = GameMain.GameSession.EventManager;
			if (eventManager == null)
			{
				return;
			}
			eventManager.StoreEventDataAtRoundEnd(true);
		}

		// Token: 0x06000EFD RID: 3837 RVA: 0x0008DB78 File Offset: 0x0008BD78
		public void UpdateStoreStock()
		{
			Map map = this.Map;
			if (map != null)
			{
				Location currentLocation = map.CurrentLocation;
				if (currentLocation != null)
				{
					currentLocation.AddStock(this.CargoManager.SoldItems);
				}
			}
			CargoManager cargoManager = this.CargoManager;
			if (cargoManager != null)
			{
				cargoManager.ClearSoldItemsProjSpecific();
			}
			Map map2 = this.Map;
			if (map2 == null)
			{
				return;
			}
			Location currentLocation2 = map2.CurrentLocation;
			if (currentLocation2 == null)
			{
				return;
			}
			currentLocation2.RemoveStock(this.CargoManager.PurchasedItems);
		}

		// Token: 0x06000EFE RID: 3838 RVA: 0x0008DBE4 File Offset: 0x0008BDE4
		public void EndCampaign()
		{
			foreach (Character c in Character.CharacterList)
			{
				if (c.IsOnPlayerTeam)
				{
					c.CharacterHealth.RemoveNegativeAfflictions();
				}
			}
			foreach (LocationConnection connection in this.Map.Connections)
			{
				connection.Difficulty = connection.Biome.AdjustedMaxDifficulty;
				connection.LevelData = new LevelData(connection)
				{
					IsBeaconActive = false,
					ForceOutpostGenerationParams = connection.LevelData.ForceOutpostGenerationParams
				};
				connection.LevelData.HasHuntingGrounds = connection.LevelData.OriginallyHadHuntingGrounds;
			}
			foreach (Location location in this.Map.Locations)
			{
				location.LevelData = new LevelData(location, this.Map, location.Biome.AdjustedMaxDifficulty)
				{
					ForceOutpostGenerationParams = location.LevelData.ForceOutpostGenerationParams
				};
				location.Reset(this);
			}
			this.Map.ClearLocationHistory();
			this.Map.SetLocation(this.Map.Locations.IndexOf(this.Map.StartLocation));
			this.Map.SelectLocation(-1);
			if (this.Map.Radiation != null)
			{
				this.Map.Radiation.Amount = this.Map.Radiation.Params.StartingRadiation;
			}
			foreach (Location location2 in this.Map.Locations)
			{
				location2.TurnsInRadiation = 0;
			}
			foreach (Faction faction in this.Factions)
			{
				faction.Reputation.SetReputation((float)faction.Prefab.InitialReputation);
			}
			this.EndCampaignProjSpecific();
			if (this.CampaignMetadata != null)
			{
				int loops = this.CampaignMetadata.GetInt("campaign.endings".ToIdentifier(), new int?(0));
				this.CampaignMetadata.SetValue("campaign.endings".ToIdentifier(), loops + 1);
			}
			this.Settings.TutorialEnabled = false;
			GameAnalyticsManager.ProgressionStatus progressionStatus = GameAnalyticsManager.ProgressionStatus.Complete;
			GameModePreset preset = base.Preset;
			GameAnalyticsManager.AddProgressionEvent(progressionStatus, ((preset != null) ? preset.Identifier.Value : null) ?? "none");
			string eventId = "FinishCampaign:";
			string str = eventId;
			string str2 = "Submarine:";
			Submarine mainSub = Submarine.MainSub;
			string text;
			if (mainSub == null)
			{
				text = null;
			}
			else
			{
				SubmarineInfo info = mainSub.Info;
				text = ((info != null) ? info.Name : null);
			}
			GameAnalyticsManager.AddDesignEvent(str + str2 + (text ?? "none"));
			string str3 = eventId;
			string str4 = "CrewSize:";
			CrewManager crewManager = base.CrewManager;
			int? num;
			if (crewManager == null)
			{
				num = null;
			}
			else
			{
				IEnumerable<CharacterInfo> characterInfos = crewManager.GetCharacterInfos(false);
				num = ((characterInfos != null) ? new int?(characterInfos.Count<CharacterInfo>()) : null);
			}
			int? num2 = num;
			GameAnalyticsManager.AddDesignEvent(str3 + str4 + num2.GetValueOrDefault().ToString());
			GameAnalyticsManager.AddDesignEvent(eventId + "Money", (double)this.Bank.Balance);
			GameAnalyticsManager.AddDesignEvent(eventId + "Playtime", this.TotalPlayTime);
			GameAnalyticsManager.AddDesignEvent(eventId + "PassedLevels", (double)this.TotalPassedLevels);
		}

		// Token: 0x06000EFF RID: 3839 RVA: 0x0008DFB8 File Offset: 0x0008C1B8
		protected virtual void EndCampaignProjSpecific()
		{
		}

		// Token: 0x06000F00 RID: 3840 RVA: 0x0008DFBA File Offset: 0x0008C1BA
		public Faction GetRandomFaction(Rand.RandSync randSync, bool allowEmpty = true)
		{
			return CampaignMode.GetRandomFaction(this.Factions, randSync, false, allowEmpty);
		}

		// Token: 0x06000F01 RID: 3841 RVA: 0x0008DFCA File Offset: 0x0008C1CA
		public Faction GetRandomSecondaryFaction(Rand.RandSync randSync, bool allowEmpty = true)
		{
			return CampaignMode.GetRandomFaction(this.Factions, randSync, true, allowEmpty);
		}

		// Token: 0x06000F02 RID: 3842 RVA: 0x0008DFDA File Offset: 0x0008C1DA
		public static Faction GetRandomFaction(IEnumerable<Faction> factions, Rand.RandSync randSync, bool secondary = false, bool allowEmpty = true)
		{
			return CampaignMode.GetRandomFaction(factions, Rand.GetRNG(randSync), secondary, allowEmpty);
		}

		// Token: 0x06000F03 RID: 3843 RVA: 0x0008DFEC File Offset: 0x0008C1EC
		public static Faction GetRandomFaction(IEnumerable<Faction> factions, Random random, bool secondary = false, bool allowEmpty = true)
		{
			List<Faction> factionsList = (from f in factions
			orderby f.Prefab.Identifier
			select f).ToList<Faction>();
			List<float> weights = factionsList.Select(delegate(Faction f)
			{
				if (!secondary)
				{
					return f.Prefab.ControlledOutpostPercentage;
				}
				return f.Prefab.SecondaryControlledOutpostPercentage;
			}).ToList<float>();
			float percentageSum = weights.Sum();
			if (percentageSum < 100f && allowEmpty)
			{
				factionsList.Add(null);
				weights.Add(100f - percentageSum);
			}
			return ToolBox.SelectWeightedRandom<Faction>(factionsList, weights, random);
		}

		// Token: 0x06000F04 RID: 3844 RVA: 0x0008E078 File Offset: 0x0008C278
		public bool TryHireCharacter(Location location, CharacterInfo characterInfo, bool takeMoney = true, Client client = null, bool buyingNewCharacter = false)
		{
			if (characterInfo == null)
			{
				return false;
			}
			if (characterInfo.MinReputationToHire.Item1 != Identifier.Empty && MathF.Round(this.GetReputation(characterInfo.MinReputationToHire.Item1)) < characterInfo.MinReputationToHire.Item2)
			{
				return false;
			}
			int price = buyingNewCharacter ? this.NewCharacterCost(characterInfo) : HireManager.GetSalaryFor(characterInfo);
			if (takeMoney && !this.TryPurchase(client, price))
			{
				return false;
			}
			characterInfo.IsNewHire = true;
			characterInfo.Title = null;
			location.RemoveHireableCharacter(characterInfo);
			GameSession gameSession = GameMain.GameSession;
			if (!(((gameSession != null) ? gameSession.Campaign : null) is MultiPlayerCampaign))
			{
				base.CrewManager.AddCharacterInfo(characterInfo);
			}
			int salary = characterInfo.Salary;
			GameAnalyticsManager.MoneySink moneySink = GameAnalyticsManager.MoneySink.Crew;
			Job job = characterInfo.Job;
			GameAnalyticsManager.AddMoneySpentEvent(salary, moneySink, ((job != null) ? job.Prefab.Identifier.Value : null) ?? "unknown");
			return true;
		}

		// Token: 0x06000F05 RID: 3845 RVA: 0x0008E158 File Offset: 0x0008C358
		public int NewCharacterCost(CharacterInfo characterInfo)
		{
			NetworkMember networkMember = GameMain.NetworkMember;
			float characterCostPercentage = (networkMember != null) ? networkMember.ServerSettings.ReplaceCostPercentage : 100f;
			return (int)MathF.Round((float)HireManager.GetSalaryFor(characterInfo) * (characterCostPercentage / 100f));
		}

		// Token: 0x06000F06 RID: 3846 RVA: 0x0008E195 File Offset: 0x0008C395
		public bool CanAffordNewCharacter(CharacterInfo characterInfo)
		{
			return this.CanAfford(this.NewCharacterCost(characterInfo), null);
		}

		// Token: 0x06000F07 RID: 3847 RVA: 0x0008E1A8 File Offset: 0x0008C3A8
		private void NPCInteract(Character npc, Character interactor)
		{
			if (!npc.AllowCustomInteract)
			{
				return;
			}
			HumanAIController humanAi = npc.AIController as HumanAIController;
			if (humanAi != null && !humanAi.AllowCampaignInteraction())
			{
				return;
			}
			this.NPCInteractProjSpecific(npc, interactor);
			string coroutineName = "DoCharacterWait." + ((npc != null) ? npc.ID : 0).ToString();
			if (!CoroutineManager.IsCoroutineRunning(coroutineName))
			{
				CoroutineManager.StartCoroutine(this.DoCharacterWait(npc, interactor), coroutineName);
			}
		}

		// Token: 0x06000F08 RID: 3848 RVA: 0x0008E214 File Offset: 0x0008C414
		private IEnumerable<CoroutineStatus> DoCharacterWait(Character npc, Character interactor)
		{
			CampaignMode.<DoCharacterWait>d__159 <DoCharacterWait>d__ = new CampaignMode.<DoCharacterWait>d__159(-2);
			<DoCharacterWait>d__.<>4__this = this;
			<DoCharacterWait>d__.<>3__npc = npc;
			<DoCharacterWait>d__.<>3__interactor = interactor;
			return <DoCharacterWait>d__;
		}

		// Token: 0x06000F09 RID: 3849 RVA: 0x0008E234 File Offset: 0x0008C434
		private void NPCInteractProjSpecific(Character npc, Character interactor)
		{
			if (npc == null || interactor == null)
			{
				return;
			}
			switch (npc.CampaignInteractionType)
			{
			case CampaignMode.InteractionType.None:
			case CampaignMode.InteractionType.Talk:
			case CampaignMode.InteractionType.Examine:
				return;
			case CampaignMode.InteractionType.Crew:
				if (GameMain.NetworkMember != null)
				{
					this.CampaignUI.HRManagerUI.SendCrewState(false, default(ValueTuple<CharacterInfo, string>), null, false);
				}
				break;
			case CampaignMode.InteractionType.Upgrade:
				if (!this.UpgradeManager.CanUpgradeSub())
				{
					this.UpgradeManager.CreateUpgradeErrorMessage(TextManager.Get("Dialog.CantUpgrade").Value, base.IsSinglePlayer, npc);
					return;
				}
				break;
			case CampaignMode.InteractionType.MedicalClinic:
				this.CampaignUI.MedicalClinic.RequestLatestPending();
				break;
			}
			this.ShowCampaignUI = true;
			this.CampaignUI.SelectTab(npc.CampaignInteractionType, npc);
			UpgradeStore upgradeStore = this.CampaignUI.UpgradeStore;
			if (upgradeStore != null)
			{
				upgradeStore.RequestRefresh(true);
			}
			HumanAIController humanAi = npc.AIController as HumanAIController;
			if (humanAi != null && humanAi.IsInHostileFaction())
			{
				string value = TextManager.Get("dialoglowrepcampaigninteraction").Value;
				Identifier identifier = "dialoglowrepcampaigninteraction".ToIdentifier();
				npc.Speak(value, null, 0f, identifier, 60f);
			}
		}

		// Token: 0x06000F0A RID: 3850 RVA: 0x0008E35C File Offset: 0x0008C55C
		public void AssignNPCMenuInteraction(Character character, CampaignMode.InteractionType interactionType)
		{
			character.CampaignInteractionType = interactionType;
			if (character.CampaignInteractionType == CampaignMode.InteractionType.Store)
			{
				HumanPrefab humanPrefab = character.HumanPrefab;
				if (humanPrefab != null)
				{
					Identifier merchantId = humanPrefab.Identifier;
					character.MerchantIdentifier = merchantId;
					Location currentLocation = this.map.CurrentLocation;
					if (currentLocation != null)
					{
						Location.StoreInfo store = currentLocation.GetStore(merchantId);
						if (store != null)
						{
							store.SetMerchantFaction(character.Faction);
						}
					}
				}
			}
			character.DisableHealthWindow = (interactionType != CampaignMode.InteractionType.None && interactionType != CampaignMode.InteractionType.Examine && interactionType != CampaignMode.InteractionType.Talk);
			if (interactionType == CampaignMode.InteractionType.None)
			{
				character.SetCustomInteract(null, null);
				return;
			}
			Action<Character, Character> onCustomInteract = new Action<Character, Character>(this.NPCInteract);
			string tag = "CampaignInteraction." + interactionType.ToString();
			string varName = "[key]";
			GameSettings.Config.KeyMapping keyMap = GameSettings.CurrentConfig.KeyMap;
			character.SetCustomInteract(onCustomInteract, TextManager.GetWithVariable(tag, varName, keyMap.KeyBindText(InputType.Use), FormatCapitals.No));
		}

		// Token: 0x06000F0B RID: 3851 RVA: 0x0008E424 File Offset: 0x0008C624
		protected void KeepCharactersCloseToOutpost(float deltaTime)
		{
			if (!Level.IsLoadedFriendlyOutpost)
			{
				return;
			}
			Rectangle worldBorders = Submarine.MainSub.GetDockedBorders(true);
			worldBorders.Location += Submarine.MainSub.WorldPosition.ToPoint();
			foreach (Character c in Character.CharacterList)
			{
				if ((c != Character.Controlled && !c.IsRemotePlayer) || c.Removed || c.IsDead || c.IsIncapacitated || c.Submarine != null)
				{
					if (this.characterOutOfBoundsTimer.ContainsKey(c))
					{
						c.OverrideMovement = null;
						this.characterOutOfBoundsTimer.Remove(c);
					}
				}
				else if (c.WorldPosition.Y < (float)(worldBorders.Y - worldBorders.Height) - 3000f)
				{
					if (!this.characterOutOfBoundsTimer.ContainsKey(c))
					{
						this.characterOutOfBoundsTimer.Add(c, 0f);
					}
					else
					{
						Dictionary<Character, float> dictionary = this.characterOutOfBoundsTimer;
						Character key = c;
						dictionary[key] += deltaTime;
					}
				}
				else if (c.WorldPosition.Y > (float)(worldBorders.Y - worldBorders.Height) - 2500f && this.characterOutOfBoundsTimer.ContainsKey(c))
				{
					c.OverrideMovement = null;
					this.characterOutOfBoundsTimer.Remove(c);
				}
			}
			foreach (KeyValuePair<Character, float> character in this.characterOutOfBoundsTimer)
			{
				if (character.Value <= 0f && base.IsSinglePlayer)
				{
					GameMain.GameSession.CrewManager.AddSinglePlayerChatMessage(TextManager.Get("RadioAnnouncerName"), TextManager.Get("TooFarFromOutpostWarning"), ChatMessageType.Default, null);
				}
				character.Key.OverrideMovement = new Vector2?(Vector2.UnitY * 10f);
				Character.DisableControls = true;
				if (character.Value > 10f)
				{
					Vector2 teleportPos = character.Key.WorldPosition;
					teleportPos += Vector2.Normalize(Submarine.MainSub.WorldPosition - character.Key.WorldPosition) * 100f;
					character.Key.AnimController.SetPosition(ConvertUnits.ToSimUnits(teleportPos), false, true, false, true);
				}
			}
		}

		// Token: 0x06000F0C RID: 3852 RVA: 0x0008E6E8 File Offset: 0x0008C8E8
		public void OutpostNPCAttacked(Character npc, Character attacker, AttackResult attackResult)
		{
			if (npc == null || attacker == null || npc.IsDead || npc.IsInstigator)
			{
				return;
			}
			if (npc.TeamID != CharacterTeamType.FriendlyNPC)
			{
				return;
			}
			if (!attacker.IsRemotePlayer && attacker != Character.Controlled)
			{
				return;
			}
			Identifier faction2 = npc.Faction;
			if (faction2 != null)
			{
				Faction faction = this.Factions.FirstOrDefault(delegate(Faction f)
				{
					Prefab prefab = f.Prefab;
					Identifier faction3 = npc.Faction;
					return prefab.Identifier == faction3;
				});
				if (faction != null)
				{
					Reputation reputation = faction.Reputation;
					if (reputation == null)
					{
						return;
					}
					reputation.AddReputation(-attackResult.Damage * 0.025f, 20f);
					return;
				}
			}
			Map map = this.Map;
			Location location = (map != null) ? map.CurrentLocation : null;
			if (location != null)
			{
				Reputation reputation2 = location.Reputation;
				if (reputation2 == null)
				{
					return;
				}
				reputation2.AddReputation(-attackResult.Damage * 0.025f, 20f);
			}
		}

		// Token: 0x06000F0D RID: 3853 RVA: 0x0008E7D8 File Offset: 0x0008C9D8
		public Faction GetFaction(Identifier identifier)
		{
			return this.factions.Find((Faction f) => f.Prefab.Identifier == identifier);
		}

		// Token: 0x06000F0E RID: 3854 RVA: 0x0008E80C File Offset: 0x0008CA0C
		public float GetReputation(Identifier factionIdentifier)
		{
			CampaignMode.<>c__DisplayClass166_0 CS$<>8__locals1 = new CampaignMode.<>c__DisplayClass166_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.factionIdentifier = factionIdentifier;
			CampaignMode.<>c__DisplayClass166_0 CS$<>8__locals2 = CS$<>8__locals1;
			Identifier identifier = "location".ToIdentifier();
			Faction faction = (CS$<>8__locals2.factionIdentifier == identifier) ? this.factions.Find(delegate(Faction f)
			{
				Map map = CS$<>8__locals1.<>4__this.Map;
				Faction faction2;
				if (map == null)
				{
					faction2 = null;
				}
				else
				{
					Location currentLocation = map.CurrentLocation;
					faction2 = ((currentLocation != null) ? currentLocation.Faction : null);
				}
				return f == faction2;
			}) : this.factions.Find((Faction f) => f.Prefab.Identifier == CS$<>8__locals1.factionIdentifier);
			float? num;
			if (faction == null)
			{
				num = null;
			}
			else
			{
				Reputation reputation = faction.Reputation;
				num = ((reputation != null) ? new float?(reputation.Value) : null);
			}
			float? num2 = num;
			return num2.GetValueOrDefault();
		}

		// Token: 0x06000F0F RID: 3855 RVA: 0x0008E8B0 File Offset: 0x0008CAB0
		public FactionAffiliation GetFactionAffiliation(Identifier factionIdentifier)
		{
			Faction faction = this.GetFaction(factionIdentifier);
			return Faction.GetPlayerAffiliationStatus(faction);
		}

		// Token: 0x06000F10 RID: 3856
		public abstract void Save(XElement element, bool isSavingOnLoading);

		// Token: 0x06000F11 RID: 3857 RVA: 0x0008E8CC File Offset: 0x0008CACC
		protected void LoadStats(XElement element)
		{
			this.TotalPlayTime = element.GetAttributeDouble("TotalPlayTime".ToLowerInvariant(), 0.0);
			this.TotalPassedLevels = element.GetAttributeInt("TotalPassedLevels".ToLowerInvariant(), 0);
			this.DivingSuitWarningShown = element.GetAttributeBool("DivingSuitWarningShown".ToLowerInvariant(), false);
		}

		// Token: 0x06000F12 RID: 3858 RVA: 0x0008E928 File Offset: 0x0008CB28
		protected XElement SaveStats()
		{
			return new XElement("stats", new object[]
			{
				new XAttribute("TotalPlayTime".ToLowerInvariant(), this.TotalPlayTime),
				new XAttribute("TotalPassedLevels".ToLowerInvariant(), this.TotalPassedLevels),
				new XAttribute("DivingSuitWarningShown".ToLowerInvariant(), this.DivingSuitWarningShown)
			});
		}

		// Token: 0x06000F13 RID: 3859 RVA: 0x0008E9B0 File Offset: 0x0008CBB0
		public void LogState()
		{
			DebugConsole.NewMessage("********* CAMPAIGN STATUS *********", new Color?(Color.White), false);
			DebugConsole.NewMessage("   Money: " + this.Bank.Balance.ToString(), new Color?(Color.White), false);
			DebugConsole.NewMessage("   Current location: " + this.map.CurrentLocation.DisplayName, new Color?(Color.White), false);
			DebugConsole.NewMessage("   Available destinations: ", new Color?(Color.White), false);
			for (int i = 0; i < this.map.CurrentLocation.Connections.Count; i++)
			{
				Location destination = this.map.CurrentLocation.Connections[i].OtherLocation(this.map.CurrentLocation);
				if (destination == this.map.SelectedLocation)
				{
					DebugConsole.NewMessage("     " + i.ToString() + ". " + destination.DisplayName + " [SELECTED]", new Color?(Color.White), false);
				}
				else
				{
					DebugConsole.NewMessage("     " + i.ToString() + ". " + destination.DisplayName, new Color?(Color.White), false);
				}
			}
			if (this.map.CurrentLocation != null)
			{
				foreach (Mission mission in this.map.CurrentLocation.SelectedMissions)
				{
					DebugConsole.NewMessage("   Selected mission: " + mission.Name, new Color?(Color.White), false);
					DebugConsole.NewMessage("\n" + mission.Description, new Color?(Color.White), false);
				}
			}
		}

		// Token: 0x06000F14 RID: 3860 RVA: 0x0008EBBC File Offset: 0x0008CDBC
		public override void Remove()
		{
			base.Remove();
			Map map = this.map;
			if (map != null)
			{
				map.Remove();
			}
			this.map = null;
		}

		// Token: 0x06000F15 RID: 3861 RVA: 0x0008EBDC File Offset: 0x0008CDDC
		public int NumberOfSelectableMissionsAtLocation(Location location)
		{
			Map map = this.Map;
			int? num;
			if (map == null)
			{
				num = null;
			}
			else
			{
				Location currentLocation = map.CurrentLocation;
				if (currentLocation == null)
				{
					num = null;
				}
				else
				{
					IEnumerable<Mission> selectedMissions = currentLocation.SelectedMissions;
					num = ((selectedMissions != null) ? new int?(selectedMissions.Count((Mission m) => m.Locations.Contains(location) && !m.Prefab.IsSideObjective)) : null);
				}
			}
			int? num2 = num;
			return num2.GetValueOrDefault();
		}

		// Token: 0x06000F16 RID: 3862 RVA: 0x0008EC54 File Offset: 0x0008CE54
		public void CheckTooManyMissions(Location currentLocation, Client sender)
		{
			IEnumerable<LocationConnection> connections = currentLocation.Connections;
			Func<LocationConnection, Location> <>9__0;
			Func<LocationConnection, Location> selector;
			if ((selector = <>9__0) == null)
			{
				selector = (<>9__0 = ((LocationConnection c) => c.OtherLocation(currentLocation)));
			}
			using (IEnumerator<Location> enumerator = connections.Select(selector).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Location location = enumerator.Current;
					if (this.NumberOfSelectableMissionsAtLocation(location) > this.Settings.TotalMaxMissionCount)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(93, 3);
						defaultInterpolatedStringHandler.AppendLiteral("Client ");
						defaultInterpolatedStringHandler.AppendFormatted(sender.Name);
						defaultInterpolatedStringHandler.AppendLiteral(" had too many missions selected for location ");
						defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(location.DisplayName);
						defaultInterpolatedStringHandler.AppendLiteral("! Count was ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(this.NumberOfSelectableMissionsAtLocation(location));
						defaultInterpolatedStringHandler.AppendLiteral(". Deselecting extra missions.");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
						IEnumerable<Mission> selectedMissions = currentLocation.SelectedMissions;
						Func<Mission, bool> predicate;
						Func<Mission, bool> <>9__1;
						if ((predicate = <>9__1) == null)
						{
							predicate = (<>9__1 = ((Mission m) => m.Locations[1] == location));
						}
						foreach (Mission mission in selectedMissions.Where(predicate).Skip(this.Settings.TotalMaxMissionCount).ToList<Mission>())
						{
							currentLocation.DeselectMission(mission);
						}
					}
				}
			}
		}

		// Token: 0x06000F17 RID: 3863 RVA: 0x0008EE18 File Offset: 0x0008D018
		protected static void LeaveUnconnectedSubs(Submarine leavingSub)
		{
			if (leavingSub != Submarine.MainSub && !leavingSub.DockedTo.Contains(Submarine.MainSub))
			{
				Submarine.MainSub = leavingSub;
				GameMain.GameSession.Submarine = leavingSub;
				GameMain.GameSession.SubmarineInfo = leavingSub.Info;
				leavingSub.Info.FilePath = Path.Combine(SaveUtil.TempPath, leavingSub.Info.Name + ".sub");
				List<Submarine> subsToLeaveBehind = CampaignMode.GetSubsToLeaveBehind(leavingSub);
				GameMain.GameSession.OwnedSubmarines.Add(leavingSub.Info);
				using (List<Submarine>.Enumerator enumerator = subsToLeaveBehind.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Submarine sub = enumerator.Current;
						GameMain.GameSession.OwnedSubmarines.RemoveAll((SubmarineInfo s) => s != leavingSub.Info && s.Name == sub.Info.Name);
						MapEntity.MapEntityList.RemoveAll((MapEntity e) => e.Submarine == sub && e is LinkedSubmarine);
						LinkedSubmarine.CreateDummy(leavingSub, sub);
					}
				}
			}
		}

		// Token: 0x06000F18 RID: 3864 RVA: 0x0008EF7C File Offset: 0x0008D17C
		public void SwitchSubs()
		{
			if (this.TransferItemsOnSubSwitch)
			{
				this.TransferItemsBetweenSubs();
			}
			this.RefreshOwnedSubmarines();
			this.SwitchedSubsThisRound = true;
			this.PendingSubmarineSwitch = null;
		}

		// Token: 0x06000F19 RID: 3865 RVA: 0x0008EFA0 File Offset: 0x0008D1A0
		protected void TransferItemsBetweenSubs()
		{
			Submarine currentSub = GameMain.GameSession.Submarine;
			if (currentSub == null || currentSub.Removed)
			{
				DebugConsole.ThrowError("Cannot transfer items between subs, because the current sub is null or removed!", null, null, false, false);
				return;
			}
			List<ValueTuple<Item, Item>> itemsToTransfer = new List<ValueTuple<Item, Item>>();
			if (this.PendingSubmarineSwitch != null)
			{
				HashSet<Submarine> connectedSubs2 = (from s in currentSub.GetConnectedSubs()
				where s.Info.Type == SubmarineType.Player
				select s).ToHashSet<Submarine>();
				foreach (Item item in Item.ItemList)
				{
					if (!item.Removed && !item.NonInteractable && !item.NonPlayerTeamInteractable && !item.IsHidden && connectedSubs2.Contains(item.Submarine) && !item.Prefab.DontTransferBetweenSubs && !CampaignMode.<TransferItemsBetweenSubs>g__AnyParentInventoryDisableTransfer|177_4(item))
					{
						Entity rootOwner = item.GetRootInventoryOwner();
						if (!(rootOwner is Character))
						{
							Item ownerItem = rootOwner as Item;
							if ((ownerItem == null || (!ownerItem.NonInteractable && !item.NonPlayerTeamInteractable && !ownerItem.IsHidden)) && item.GetComponent<Door>() == null)
							{
								if (!item.Components.None((ItemComponent c) => c is Pickable))
								{
									if (!item.Components.Any(delegate(ItemComponent c)
									{
										Pickable p = c as Pickable;
										return p != null && p.IsAttached;
									}))
									{
										if (!item.Components.Any(delegate(ItemComponent c)
										{
											Wire w = c as Wire;
											if (w != null)
											{
												return w.Connections.Any((Connection c) => c != null);
											}
											return false;
										}))
										{
											itemsToTransfer.Add(new ValueTuple<Item, Item>(item, item.Container));
											item.Submarine = null;
										}
									}
								}
							}
						}
					}
				}
				foreach (ValueTuple<Item, Item> valueTuple in itemsToTransfer)
				{
					Item item2 = valueTuple.Item1;
					Item container = valueTuple.Item2;
					if (((container != null) ? container.Submarine : null) != null)
					{
						item2.Drop(null, false, false);
						item2.Submarine = null;
						foreach (ItemContainer itemContainer in item2.GetComponents<ItemContainer>())
						{
							itemContainer.Inventory.FindAllItems((Item _) => true, true, null).ForEach(delegate(Item it)
							{
								it.Submarine = null;
							});
						}
					}
				}
				currentSub.Info.NoItems = true;
			}
			GameMain.GameSession.SubmarineInfo = new SubmarineInfo(currentSub);
			if (this.PendingSubmarineSwitch != null && itemsToTransfer.Any<ValueTuple<Item, Item>>())
			{
				Submarine newSub = new Submarine(this.PendingSubmarineSwitch, true, null, null);
				IEnumerable<Submarine> connectedSubs = from s in newSub.GetConnectedSubs()
				where s.Info.Type == SubmarineType.Player
				select s;
				WayPoint wp2 = WayPoint.WayPointList.FirstOrDefault((WayPoint wp) => wp.SpawnType == SpawnType.Cargo && connectedSubs.Contains(wp.Submarine));
				Hull spawnHull = ((wp2 != null) ? wp2.CurrentHull : null) ?? Hull.HullList.FirstOrDefault((Hull h) => connectedSubs.Contains(h.Submarine) && !h.IsWetRoom);
				if (spawnHull == null)
				{
					DebugConsole.AddWarning("Failed to transfer items between subs. No cargo waypoint or dry hulls found in the new sub.", null);
					return;
				}
				HashSet<ValueTuple<Item, Item>> cargoContainers = (from it in itemsToTransfer
				where it.Item1.HasTag(Tags.Crate)
				select it).ToHashSet<ValueTuple<Item, Item>>();
				foreach (ValueTuple<Item, Item> valueTuple2 in cargoContainers)
				{
					Item item3 = valueTuple2.Item1;
					Vector2 simPos = ConvertUnits.ToSimUnits(CargoManager.GetCargoPos(spawnHull, item3.Prefab));
					item3.SetTransform(simPos, 0f, false, false, null);
					item3.CurrentHull = spawnHull;
					item3.Submarine = spawnHull.Submarine;
				}
				List<ItemContainer> availableContainers = CargoManager.FindReusableCargoContainers(connectedSubs, null).ToList<ItemContainer>();
				foreach (ValueTuple<Item, Item> valueTuple3 in itemsToTransfer)
				{
					Item item4 = valueTuple3.Item1;
					Item oldContainer = valueTuple3.Item2;
					if (!cargoContainers.Contains(new ValueTuple<Item, Item>(item4, oldContainer)))
					{
						Item newContainer = null;
						item4.Submarine = newSub;
						if (item4.Container == null)
						{
							newContainer = newSub.FindContainerFor(item4, true, true, true);
						}
						string text;
						if (newContainer != null)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
							defaultInterpolatedStringHandler.AppendFormatted<Identifier>(newContainer.Prefab.Identifier);
							defaultInterpolatedStringHandler.AppendLiteral(" (");
							defaultInterpolatedStringHandler.AppendFormatted(newContainer.Tags);
							defaultInterpolatedStringHandler.AppendLiteral(")");
							text = defaultInterpolatedStringHandler.ToStringAndClear();
						}
						else
						{
							text = "(null)";
						}
						string newContainerName = text;
						if (item4.Container == null && (newContainer == null || !newContainer.OwnInventory.TryPutItem(item4, null, null, false, false, true)))
						{
							ItemContainer cargoContainer = CargoManager.GetOrCreateCargoContainerFor(item4.Prefab, spawnHull, ref availableContainers);
							if (cargoContainer == null || !cargoContainer.Inventory.TryPutItem(item4, null, null, false, false, true))
							{
								Vector2 simPos2 = ConvertUnits.ToSimUnits(CargoManager.GetCargoPos(spawnHull, item4.Prefab));
								item4.SetTransform(simPos2, 0f, false, false, null);
							}
							else
							{
								Submarine containerSub = cargoContainer.Item.Submarine;
								if (containerSub != null)
								{
									item4.Submarine = containerSub;
								}
								newContainerName = cargoContainer.Item.Prefab.Identifier.ToString();
							}
						}
						string msg;
						if (oldContainer != null)
						{
							if (newContainer == null && oldContainer == item4.Container)
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(36, 4);
								defaultInterpolatedStringHandler2.AppendLiteral("Transferred ");
								defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(item4.Prefab.Identifier);
								defaultInterpolatedStringHandler2.AppendLiteral(" (");
								defaultInterpolatedStringHandler2.AppendFormatted<ushort>(item4.ID);
								defaultInterpolatedStringHandler2.AppendLiteral(") contained inside ");
								defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(oldContainer.Prefab.Identifier);
								defaultInterpolatedStringHandler2.AppendLiteral(" (");
								defaultInterpolatedStringHandler2.AppendFormatted<ushort>(oldContainer.ID);
								defaultInterpolatedStringHandler2.AppendLiteral(")");
								msg = defaultInterpolatedStringHandler2.ToStringAndClear();
							}
							else
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(28, 5);
								defaultInterpolatedStringHandler3.AppendLiteral("Transferred ");
								defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(item4.Prefab.Identifier);
								defaultInterpolatedStringHandler3.AppendLiteral(" (");
								defaultInterpolatedStringHandler3.AppendFormatted<ushort>(item4.ID);
								defaultInterpolatedStringHandler3.AppendLiteral(") from ");
								defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(oldContainer.Prefab.Identifier);
								defaultInterpolatedStringHandler3.AppendLiteral(" (");
								defaultInterpolatedStringHandler3.AppendFormatted(oldContainer.Tags);
								defaultInterpolatedStringHandler3.AppendLiteral(") to ");
								defaultInterpolatedStringHandler3.AppendFormatted(newContainerName);
								msg = defaultInterpolatedStringHandler3.ToStringAndClear();
							}
						}
						else
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(19, 3);
							defaultInterpolatedStringHandler4.AppendLiteral("Transferred ");
							defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(item4.Prefab.Identifier);
							defaultInterpolatedStringHandler4.AppendLiteral(" (");
							defaultInterpolatedStringHandler4.AppendFormatted<ushort>(item4.ID);
							defaultInterpolatedStringHandler4.AppendLiteral(") to ");
							defaultInterpolatedStringHandler4.AppendFormatted(newContainerName);
							msg = defaultInterpolatedStringHandler4.ToStringAndClear();
						}
						DebugConsole.Log(msg);
					}
				}
				foreach (ValueTuple<Item, Item> valueTuple4 in itemsToTransfer)
				{
					Item item5 = valueTuple4.Item1;
					CampaignMode.<TransferItemsBetweenSubs>g__PropagateSubmarineProperty|177_14(item5);
				}
				newSub.Info.NoItems = false;
				this.PendingSubmarineSwitch = new SubmarineInfo(newSub);
			}
		}

		// Token: 0x06000F1A RID: 3866 RVA: 0x0008F81C File Offset: 0x0008DA1C
		protected void RefreshOwnedSubmarines()
		{
			if (this.PendingSubmarineSwitch != null)
			{
				SubmarineInfo previousSub = GameMain.GameSession.SubmarineInfo;
				GameMain.GameSession.SubmarineInfo = this.PendingSubmarineSwitch;
				for (int i = 0; i < GameMain.GameSession.OwnedSubmarines.Count; i++)
				{
					if (GameMain.GameSession.OwnedSubmarines[i].Name == previousSub.Name)
					{
						GameMain.GameSession.OwnedSubmarines[i] = previousSub;
						return;
					}
				}
			}
		}

		// Token: 0x06000F1B RID: 3867 RVA: 0x0008F89A File Offset: 0x0008DA9A
		public void SavePets(XElement parentElement = null)
		{
			this.petsElement = new XElement("pets");
			PetBehavior.SavePets(this.petsElement);
			if (parentElement != null)
			{
				parentElement.Add(this.petsElement);
			}
		}

		// Token: 0x06000F1C RID: 3868 RVA: 0x0008F8CC File Offset: 0x0008DACC
		public void LoadSaveSharedSingleAndMultiplayer(XElement element)
		{
			this.PurchasedLostShuttlesInLatestSave = element.GetAttributeBool("purchasedlostshuttles", false);
			this.PurchasedHullRepairsInLatestSave = element.GetAttributeBool("purchasedhullrepairs", false);
			this.PurchasedItemRepairsInLatestSave = element.GetAttributeBool("purchaseditemrepairs", false);
			this.CheatsEnabled = element.GetAttributeBool("cheatsenabled", false);
			if (this.CheatsEnabled)
			{
				DebugConsole.CheatsEnabled = true;
				if (!AchievementManager.CheatsEnabled)
				{
					AchievementManager.CheatsEnabled = true;
					new GUIMessageBox("Cheats enabled", "Cheat commands have been enabled on the server. You will not receive achievements until you restart the game.", null, null, GUIMessageBox.Type.Default);
				}
			}
			int oldMoney = element.GetAttributeInt("money", 0);
			if (oldMoney > 0)
			{
				this.Bank = new Wallet(Option<Character>.None())
				{
					Balance = oldMoney
				};
			}
			foreach (XElement subElement in element.Elements())
			{
				string text = subElement.Name.ToString().ToLowerInvariant();
				if (text != null)
				{
					int length = text.Length;
					switch (length)
					{
					case 4:
						if (text == "pets")
						{
							this.petsElement = subElement;
						}
						break;
					case 5:
					{
						char c = text[0];
						if (c != 'c')
						{
							if (c == 's')
							{
								if (text == "stats")
								{
									this.LoadStats(subElement);
								}
							}
						}
						else if (text == "cargo")
						{
							this.CargoManager.LoadPurchasedItems(subElement);
						}
						break;
					}
					case 6:
						if (text == "wallet")
						{
							this.Bank = new Wallet(Option<Character>.None(), subElement);
						}
						break;
					default:
						switch (length)
						{
						case 12:
							if (!(text == "eventmanager"))
							{
								continue;
							}
							GameMain.GameSession.EventManager.Load(subElement);
							continue;
						case 13:
							continue;
						case 14:
						{
							char c = text[1];
							if (c != 'n')
							{
								if (c != 'p')
								{
									continue;
								}
								if (!(text == "upgrademanager"))
								{
									continue;
								}
							}
							else
							{
								if (!(text == "unlockedrecipe"))
								{
									continue;
								}
								GameMain.GameSession.UnlockRecipe(subElement.GetAttributeEnum("team", CharacterTeamType.Team1), subElement.GetAttributeIdentifier("identifier", Identifier.Empty), false);
								continue;
							}
							break;
						}
						case 15:
							if (!(text == "pendingupgrades"))
							{
								continue;
							}
							break;
						default:
							continue;
						}
						this.UpgradeManager = new UpgradeManager(this, subElement, base.IsSinglePlayer);
						break;
					}
				}
			}
		}

		// Token: 0x06000F1D RID: 3869 RVA: 0x0008FB94 File Offset: 0x0008DD94
		public void LoadPets()
		{
			if (this.petsElement != null)
			{
				PetBehavior.LoadPets(this.petsElement);
			}
		}

		// Token: 0x06000F1E RID: 3870 RVA: 0x0008FBA9 File Offset: 0x0008DDA9
		public void SaveActiveOrders(XElement parentElement = null)
		{
			this.ActiveOrdersElement = new XElement("activeorders");
			CrewManager crewManager = base.CrewManager;
			if (crewManager != null)
			{
				crewManager.SaveActiveOrders(this.ActiveOrdersElement);
			}
			if (parentElement != null)
			{
				parentElement.Add(this.ActiveOrdersElement);
			}
		}

		// Token: 0x06000F1F RID: 3871 RVA: 0x0008FBE6 File Offset: 0x0008DDE6
		public void LoadActiveOrders()
		{
			CrewManager crewManager = base.CrewManager;
			if (crewManager == null)
			{
				return;
			}
			crewManager.LoadActiveOrders(this.ActiveOrdersElement);
		}

		// Token: 0x06000F20 RID: 3872 RVA: 0x0008FBFE File Offset: 0x0008DDFE
		[CompilerGenerated]
		internal static string <.ctor>g__FormatMessage|121_1(ref CampaignMode.<>c__DisplayClass121_1 A_0)
		{
			return TextManager.GetWithVariable(A_0.isGain ? "moneygainformat" : "moneyloseformat", "[money]", TextManager.FormatCurrency(Math.Abs(A_0.changed), true), FormatCapitals.No).ToString();
		}

		// Token: 0x06000F21 RID: 3873 RVA: 0x0008FC38 File Offset: 0x0008DE38
		[CompilerGenerated]
		internal static Submarine <GetLeavingSub>g__GetLeavingSubAtStart|145_3(IEnumerable<Character> leavingPlayers, CharacterTeamType submarineTeam)
		{
			if (Level.Loaded.StartOutpost == null)
			{
				Submarine closestSub = Submarine.FindClosest(Level.Loaded.StartExitPosition, true, true, true, new CharacterTeamType?(submarineTeam));
				if (closestSub == null)
				{
					return null;
				}
				if (!closestSub.DockedTo.Contains(Submarine.MainSub))
				{
					return closestSub;
				}
				return Submarine.MainSub;
			}
			else
			{
				if (Level.Loaded.StartOutpost.DockedTo.Any<Submarine>())
				{
					foreach (Submarine dockedSub in Level.Loaded.StartOutpost.DockedTo)
					{
						if (!dockedSub.IsRespawnShuttle && dockedSub.TeamID == submarineTeam)
						{
							return dockedSub.DockedTo.Contains(Submarine.MainSub) ? Submarine.MainSub : dockedSub;
						}
					}
				}
				if (Level.Loaded.Type == LevelData.LevelType.LocationConnection)
				{
					if (!leavingPlayers.Any((Character s) => s.Submarine == Level.Loaded.StartOutpost))
					{
						return null;
					}
				}
				Submarine closestSub2 = Submarine.FindClosest(Level.Loaded.StartOutpost.WorldPosition, true, true, true, new CharacterTeamType?(submarineTeam));
				if (closestSub2 == null || !closestSub2.AtStartExit)
				{
					return null;
				}
				if (!closestSub2.DockedTo.Contains(Submarine.MainSub))
				{
					return closestSub2;
				}
				return Submarine.MainSub;
			}
		}

		// Token: 0x06000F22 RID: 3874 RVA: 0x0008FD90 File Offset: 0x0008DF90
		[CompilerGenerated]
		internal static Submarine <GetLeavingSub>g__GetLeavingSubAtEnd|145_4(IEnumerable<Character> leavingPlayers, CharacterTeamType submarineTeam)
		{
			if (Level.Loaded.EndOutpost != null && Level.Loaded.EndOutpost.ExitPoints.Any<WayPoint>())
			{
				Submarine closestSub = Submarine.FindClosest(Level.Loaded.EndOutpost.WorldPosition, true, true, true, new CharacterTeamType?(submarineTeam));
				if (closestSub == null || !closestSub.AtEndExit)
				{
					return null;
				}
				if (!closestSub.DockedTo.Contains(Submarine.MainSub))
				{
					return closestSub;
				}
				return Submarine.MainSub;
			}
			else
			{
				if (Level.Loaded.Type == LevelData.LevelType.Outpost)
				{
					return null;
				}
				if (Level.Loaded.EndOutpost == null)
				{
					Submarine closestSub2 = Submarine.FindClosest(Level.Loaded.EndExitPosition, true, true, true, new CharacterTeamType?(submarineTeam));
					if (closestSub2 == null)
					{
						return null;
					}
					if (!closestSub2.DockedTo.Contains(Submarine.MainSub))
					{
						return closestSub2;
					}
					return Submarine.MainSub;
				}
				else
				{
					if (Level.Loaded.EndOutpost.DockedTo.Any<Submarine>())
					{
						foreach (Submarine dockedSub in Level.Loaded.EndOutpost.DockedTo)
						{
							if (!dockedSub.IsRespawnShuttle && dockedSub.TeamID == submarineTeam)
							{
								return dockedSub.DockedTo.Contains(Submarine.MainSub) ? Submarine.MainSub : dockedSub;
							}
						}
					}
					if (Level.Loaded.Type == LevelData.LevelType.LocationConnection)
					{
						if (!leavingPlayers.Any((Character s) => s.Submarine == Level.Loaded.EndOutpost))
						{
							return null;
						}
					}
					Submarine closestSub3 = Submarine.FindClosest(Level.Loaded.EndOutpost.WorldPosition, true, true, true, new CharacterTeamType?(submarineTeam));
					if (closestSub3 == null || !closestSub3.AtEndExit)
					{
						return null;
					}
					if (!closestSub3.DockedTo.Contains(Submarine.MainSub))
					{
						return closestSub3;
					}
					return Submarine.MainSub;
				}
			}
		}

		// Token: 0x06000F23 RID: 3875 RVA: 0x0008FF64 File Offset: 0x0008E164
		[CompilerGenerated]
		internal static bool <TransferItemsBetweenSubs>g__AnyParentInventoryDisableTransfer|177_4(Item item)
		{
			Inventory parentInventory = item.ParentInventory;
			Item parentOwner = ((parentInventory != null) ? parentInventory.Owner : null) as Item;
			return parentOwner != null && (CampaignMode.<TransferItemsBetweenSubs>g__HasProblematicComponent|177_6(parentOwner) || CampaignMode.<TransferItemsBetweenSubs>g__AnyParentInventoryDisableTransfer|177_4(parentOwner));
		}

		// Token: 0x06000F24 RID: 3876 RVA: 0x0008FF9E File Offset: 0x0008E19E
		[CompilerGenerated]
		internal static bool <TransferItemsBetweenSubs>g__HasProblematicComponent|177_6(Item it)
		{
			return it.Components.Any((ItemComponent c) => c.DontTransferInventoryBetweenSubs);
		}

		// Token: 0x06000F25 RID: 3877 RVA: 0x0008FFCC File Offset: 0x0008E1CC
		[CompilerGenerated]
		internal static void <TransferItemsBetweenSubs>g__PropagateSubmarineProperty|177_14(Item item)
		{
			foreach (ItemContainer ownedContainer in item.GetComponents<ItemContainer>())
			{
				foreach (Item containedItem in ownedContainer.Inventory.AllItems)
				{
					containedItem.Submarine = item.Submarine;
					CampaignMode.<TransferItemsBetweenSubs>g__PropagateSubmarineProperty|177_14(containedItem);
				}
			}
		}

		// Token: 0x0400077E RID: 1918
		protected Color overlayColor;

		// Token: 0x0400077F RID: 1919
		protected Sprite overlaySprite;

		// Token: 0x04000780 RID: 1920
		private CampaignMode.TransitionType prevCampaignUIAutoOpenType;

		// Token: 0x04000781 RID: 1921
		protected GUIButton endRoundButton;

		// Token: 0x04000782 RID: 1922
		public GUIButton ReadyCheckButton;

		// Token: 0x04000783 RID: 1923
		protected GUIFrame campaignUIContainer;

		// Token: 0x04000784 RID: 1924
		public CampaignUI CampaignUI;

		// Token: 0x04000786 RID: 1926
		private CancellationTokenSource startRoundCancellationToken;

		// Token: 0x04000788 RID: 1928
		private bool showCampaignUI;

		// Token: 0x04000789 RID: 1929
		private bool wasChatBoxOpen;

		// Token: 0x0400078A RID: 1930
		public const int MaxMoney = 1073741823;

		// Token: 0x0400078B RID: 1931
		public const int InitialMoney = 8500;

		// Token: 0x0400078C RID: 1932
		protected const float EndTransitionDuration = 5f;

		// Token: 0x0400078D RID: 1933
		private const float FirstRoundEventDelay = 0f;

		// Token: 0x0400078E RID: 1934
		public double TotalPlayTime;

		// Token: 0x0400078F RID: 1935
		public int TotalPassedLevels;

		// Token: 0x04000790 RID: 1936
		public readonly CargoManager CargoManager;

		// Token: 0x04000791 RID: 1937
		public UpgradeManager UpgradeManager;

		// Token: 0x04000792 RID: 1938
		public MedicalClinic MedicalClinic;

		// Token: 0x04000793 RID: 1939
		private List<Faction> factions;

		// Token: 0x04000794 RID: 1940
		public readonly CampaignMetadata CampaignMetadata;

		// Token: 0x04000795 RID: 1941
		protected XElement petsElement;

		// Token: 0x04000797 RID: 1943
		public CampaignSettings Settings;

		// Token: 0x04000798 RID: 1944
		private readonly List<Mission> extraMissions = new List<Mission>();

		// Token: 0x04000799 RID: 1945
		public readonly NamedEvent<WalletChangedEvent> OnMoneyChanged = new NamedEvent<WalletChangedEvent>();

		// Token: 0x0400079B RID: 1947
		public bool CheatsEnabled;

		// Token: 0x0400079C RID: 1948
		public const float HullRepairCostPerDamage = 0.1f;

		// Token: 0x0400079D RID: 1949
		public const float ItemRepairCostPerRepairDuration = 1f;

		// Token: 0x0400079E RID: 1950
		public const int ShuttleReplaceCost = 1000;

		// Token: 0x0400079F RID: 1951
		public const int MaxHullRepairCost = 600;

		// Token: 0x040007A0 RID: 1952
		public const int MaxItemRepairCost = 2000;

		// Token: 0x040007A1 RID: 1953
		protected bool wasDocked;

		// Token: 0x040007A2 RID: 1954
		private readonly Dictionary<string, double> dialogLastSpoken = new Dictionary<string, double>();

		// Token: 0x040007A3 RID: 1955
		public SubmarineInfo PendingSubmarineSwitch;

		// Token: 0x040007A6 RID: 1958
		protected Map map;

		// Token: 0x040007A7 RID: 1959
		public Wallet Bank;

		// Token: 0x040007A9 RID: 1961
		public bool PurchasedLostShuttlesInLatestSave;

		// Token: 0x040007AA RID: 1962
		public bool PurchasedHullRepairsInLatestSave;

		// Token: 0x040007AB RID: 1963
		public bool PurchasedItemRepairsInLatestSave;

		// Token: 0x040007AF RID: 1967
		public bool DivingSuitWarningShown;

		// Token: 0x040007B0 RID: 1968
		public bool ItemsRelocatedToMainSub;

		// Token: 0x040007B3 RID: 1971
		private readonly Dictionary<Character, float> characterOutOfBoundsTimer = new Dictionary<Character, float>();

		// Token: 0x02000880 RID: 2176
		[NetworkSerialize(16)]
		public readonly struct SaveInfo : INetSerializableStruct, IEquatable<CampaignMode.SaveInfo>
		{
			// Token: 0x06006E42 RID: 28226 RVA: 0x00364685 File Offset: 0x00362885
			public SaveInfo(string FilePath, Option<SerializableDateTime> SaveTime, string SubmarineName, RespawnMode RespawnMode, ImmutableArray<string> EnabledContentPackageNames)
			{
				this.FilePath = FilePath;
				this.SaveTime = SaveTime;
				this.SubmarineName = SubmarineName;
				this.RespawnMode = RespawnMode;
				this.EnabledContentPackageNames = EnabledContentPackageNames;
			}

			// Token: 0x17001A1F RID: 6687
			// (get) Token: 0x06006E43 RID: 28227 RVA: 0x003646AC File Offset: 0x003628AC
			// (set) Token: 0x06006E44 RID: 28228 RVA: 0x003646B4 File Offset: 0x003628B4
			public string FilePath { get; set; }

			// Token: 0x17001A20 RID: 6688
			// (get) Token: 0x06006E45 RID: 28229 RVA: 0x003646BD File Offset: 0x003628BD
			// (set) Token: 0x06006E46 RID: 28230 RVA: 0x003646C5 File Offset: 0x003628C5
			public Option<SerializableDateTime> SaveTime { get; set; }

			// Token: 0x17001A21 RID: 6689
			// (get) Token: 0x06006E47 RID: 28231 RVA: 0x003646CE File Offset: 0x003628CE
			// (set) Token: 0x06006E48 RID: 28232 RVA: 0x003646D6 File Offset: 0x003628D6
			public string SubmarineName { get; set; }

			// Token: 0x17001A22 RID: 6690
			// (get) Token: 0x06006E49 RID: 28233 RVA: 0x003646DF File Offset: 0x003628DF
			// (set) Token: 0x06006E4A RID: 28234 RVA: 0x003646E7 File Offset: 0x003628E7
			public RespawnMode RespawnMode { get; set; }

			// Token: 0x17001A23 RID: 6691
			// (get) Token: 0x06006E4B RID: 28235 RVA: 0x003646F0 File Offset: 0x003628F0
			// (set) Token: 0x06006E4C RID: 28236 RVA: 0x003646F8 File Offset: 0x003628F8
			public ImmutableArray<string> EnabledContentPackageNames { get; set; }

			// Token: 0x06006E4D RID: 28237 RVA: 0x00364704 File Offset: 0x00362904
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("SaveInfo");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06006E4E RID: 28238 RVA: 0x00364750 File Offset: 0x00362950
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("FilePath = ");
				builder.Append(this.FilePath);
				builder.Append(", SaveTime = ");
				builder.Append(this.SaveTime.ToString());
				builder.Append(", SubmarineName = ");
				builder.Append(this.SubmarineName);
				builder.Append(", RespawnMode = ");
				builder.Append(this.RespawnMode.ToString());
				builder.Append(", EnabledContentPackageNames = ");
				builder.Append(this.EnabledContentPackageNames.ToString());
				return true;
			}

			// Token: 0x06006E4F RID: 28239 RVA: 0x00364805 File Offset: 0x00362A05
			[CompilerGenerated]
			public static bool operator !=(CampaignMode.SaveInfo left, CampaignMode.SaveInfo right)
			{
				return !(left == right);
			}

			// Token: 0x06006E50 RID: 28240 RVA: 0x00364811 File Offset: 0x00362A11
			[CompilerGenerated]
			public static bool operator ==(CampaignMode.SaveInfo left, CampaignMode.SaveInfo right)
			{
				return left.Equals(right);
			}

			// Token: 0x06006E51 RID: 28241 RVA: 0x0036481C File Offset: 0x00362A1C
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (((EqualityComparer<string>.Default.GetHashCode(this.<FilePath>k__BackingField) * -1521134295 + EqualityComparer<Option<SerializableDateTime>>.Default.GetHashCode(this.<SaveTime>k__BackingField)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<SubmarineName>k__BackingField)) * -1521134295 + EqualityComparer<RespawnMode>.Default.GetHashCode(this.<RespawnMode>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<string>>.Default.GetHashCode(this.<EnabledContentPackageNames>k__BackingField);
			}

			// Token: 0x06006E52 RID: 28242 RVA: 0x00364895 File Offset: 0x00362A95
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is CampaignMode.SaveInfo && this.Equals((CampaignMode.SaveInfo)obj);
			}

			// Token: 0x06006E53 RID: 28243 RVA: 0x003648B0 File Offset: 0x00362AB0
			[CompilerGenerated]
			public bool Equals(CampaignMode.SaveInfo other)
			{
				return EqualityComparer<string>.Default.Equals(this.<FilePath>k__BackingField, other.<FilePath>k__BackingField) && EqualityComparer<Option<SerializableDateTime>>.Default.Equals(this.<SaveTime>k__BackingField, other.<SaveTime>k__BackingField) && EqualityComparer<string>.Default.Equals(this.<SubmarineName>k__BackingField, other.<SubmarineName>k__BackingField) && EqualityComparer<RespawnMode>.Default.Equals(this.<RespawnMode>k__BackingField, other.<RespawnMode>k__BackingField) && EqualityComparer<ImmutableArray<string>>.Default.Equals(this.<EnabledContentPackageNames>k__BackingField, other.<EnabledContentPackageNames>k__BackingField);
			}

			// Token: 0x06006E54 RID: 28244 RVA: 0x00364935 File Offset: 0x00362B35
			[CompilerGenerated]
			public void Deconstruct(out string FilePath, out Option<SerializableDateTime> SaveTime, out string SubmarineName, out RespawnMode RespawnMode, out ImmutableArray<string> EnabledContentPackageNames)
			{
				FilePath = this.FilePath;
				SaveTime = this.SaveTime;
				SubmarineName = this.SubmarineName;
				RespawnMode = this.RespawnMode;
				EnabledContentPackageNames = this.EnabledContentPackageNames;
			}
		}

		// Token: 0x02000881 RID: 2177
		public enum InteractionType
		{
			// Token: 0x04003E43 RID: 15939
			None,
			// Token: 0x04003E44 RID: 15940
			Talk,
			// Token: 0x04003E45 RID: 15941
			Examine,
			// Token: 0x04003E46 RID: 15942
			Map,
			// Token: 0x04003E47 RID: 15943
			Crew,
			// Token: 0x04003E48 RID: 15944
			Store,
			// Token: 0x04003E49 RID: 15945
			Upgrade,
			// Token: 0x04003E4A RID: 15946
			PurchaseSub,
			// Token: 0x04003E4B RID: 15947
			MedicalClinic,
			// Token: 0x04003E4C RID: 15948
			Cargo
		}

		// Token: 0x02000882 RID: 2178
		public enum TransitionType
		{
			// Token: 0x04003E4E RID: 15950
			None,
			// Token: 0x04003E4F RID: 15951
			LeaveLocation,
			// Token: 0x04003E50 RID: 15952
			ProgressToNextLocation,
			// Token: 0x04003E51 RID: 15953
			ReturnToPreviousLocation,
			// Token: 0x04003E52 RID: 15954
			ReturnToPreviousEmptyLocation,
			// Token: 0x04003E53 RID: 15955
			ProgressToNextEmptyLocation,
			// Token: 0x04003E54 RID: 15956
			End
		}
	}
}
