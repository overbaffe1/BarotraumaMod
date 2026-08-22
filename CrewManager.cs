using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Barotrauma.Tutorials;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x02000064 RID: 100
	internal class CrewManager
	{
		// Token: 0x170003CE RID: 974
		// (get) Token: 0x06000DD2 RID: 3538 RVA: 0x0007F4D3 File Offset: 0x0007D6D3
		// (set) Token: 0x06000DD3 RID: 3539 RVA: 0x0007F4DB File Offset: 0x0007D6DB
		public GUIComponent ReportButtonFrame { get; set; }

		// Token: 0x170003CF RID: 975
		// (get) Token: 0x06000DD4 RID: 3540 RVA: 0x0007F4E4 File Offset: 0x0007D6E4
		// (set) Token: 0x06000DD5 RID: 3541 RVA: 0x0007F4EC File Offset: 0x0007D6EC
		public ChatBox ChatBox { get; private set; }

		// Token: 0x170003D0 RID: 976
		// (get) Token: 0x06000DD6 RID: 3542 RVA: 0x0007F4F5 File Offset: 0x0007D6F5
		// (set) Token: 0x06000DD7 RID: 3543 RVA: 0x0007F4FD File Offset: 0x0007D6FD
		public bool IsCrewMenuOpen
		{
			get
			{
				return this._isCrewMenuOpen;
			}
			set
			{
				if (this._isCrewMenuOpen == value)
				{
					return;
				}
				this._isCrewMenuOpen = value;
				CrewManager.PreferCrewMenuOpen = value;
			}
		}

		// Token: 0x06000DD8 RID: 3544 RVA: 0x0007F516 File Offset: 0x0007D716
		public void AutoHideCrewList()
		{
			this._isCrewMenuOpen = false;
		}

		// Token: 0x06000DD9 RID: 3545 RVA: 0x0007F51F File Offset: 0x0007D71F
		public void ResetCrewListOpenState()
		{
			this._isCrewMenuOpen = CrewManager.PreferCrewMenuOpen;
		}

		// Token: 0x06000DDA RID: 3546 RVA: 0x0007F52C File Offset: 0x0007D72C
		public CrewManager(XElement element, bool isSinglePlayer) : this(isSinglePlayer)
		{
			this.AddCharacterElements(element);
		}

		// Token: 0x06000DDB RID: 3547 RVA: 0x0007F53C File Offset: 0x0007D73C
		public static void CreateReportButtons(CrewManager crewManager, GUIComponent parent, IReadOnlyList<OrderPrefab> reports, bool isHorizontal)
		{
			CrewManager.<>c__DisplayClass36_0 CS$<>8__locals1 = new CrewManager.<>c__DisplayClass36_0();
			CS$<>8__locals1.crewManager = crewManager;
			using (IEnumerator<OrderPrefab> enumerator = reports.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					CrewManager.<>c__DisplayClass36_1 CS$<>8__locals2 = new CrewManager.<>c__DisplayClass36_1();
					CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
					CS$<>8__locals2.orderPrefab = enumerator.Current;
					CrewManager.<>c__DisplayClass36_2 CS$<>8__locals3 = new CrewManager.<>c__DisplayClass36_2();
					CS$<>8__locals3.CS$<>8__locals2 = CS$<>8__locals2;
					if (CS$<>8__locals3.CS$<>8__locals2.orderPrefab.IsVisibleAsReportButton)
					{
						CrewManager.<>c__DisplayClass36_2 CS$<>8__locals4 = CS$<>8__locals3;
						Vector2 one = Vector2.One;
						RectTransform rectTransform = parent.RectTransform;
						Anchor anchor = Anchor.TopLeft;
						ScaleBasis scaleBasis = isHorizontal ? ScaleBasis.BothHeight : ScaleBasis.BothWidth;
						CS$<>8__locals4.btn = new GUIButton(new RectTransform(one, rectTransform, anchor, null, null, null, scaleBasis), Alignment.Center, null, null)
						{
							OnClicked = delegate(GUIButton button, object userData)
							{
								if (CrewManager.CanIssueOrders)
								{
									CrewManager crewManager2 = CS$<>8__locals3.CS$<>8__locals2.CS$<>8__locals1.crewManager;
									if (((crewManager2 != null) ? crewManager2.DraggedOrderPrefab : null) == null)
									{
										Submarine sub = Character.Controlled.Submarine;
										if (sub == null || sub.TeamID != Character.Controlled.TeamID || sub.Info.IsWreck)
										{
											return false;
										}
										if (CS$<>8__locals3.CS$<>8__locals2.CS$<>8__locals1.crewManager != null)
										{
											Order order = CS$<>8__locals3.CS$<>8__locals2.orderPrefab.CreateInstance(OrderPrefab.OrderTargetType.Entity, Character.Controlled, false);
											CS$<>8__locals3.CS$<>8__locals2.CS$<>8__locals1.crewManager.SetCharacterOrder(null, order, true);
											if (CS$<>8__locals3.CS$<>8__locals2.CS$<>8__locals1.crewManager.IsSinglePlayer)
											{
												HumanAIController.ReportProblem(Character.Controlled, order, null);
											}
										}
										return true;
									}
								}
								return false;
							},
							UserData = CS$<>8__locals3.CS$<>8__locals2.orderPrefab,
							ClampMouseRectToParent = false
						};
						GUIComponent btn = CS$<>8__locals3.btn;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(20, 3);
						defaultInterpolatedStringHandler.AppendLiteral("‖color:");
						defaultInterpolatedStringHandler.AppendFormatted(XMLExtensions.ColorToString(CS$<>8__locals3.CS$<>8__locals2.orderPrefab.Color));
						defaultInterpolatedStringHandler.AppendLiteral("‖");
						defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(CS$<>8__locals3.CS$<>8__locals2.orderPrefab.Name);
						defaultInterpolatedStringHandler.AppendLiteral("‖color:end‖\n");
						defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(TextManager.Get("draganddropreports"));
						btn.ToolTip = RichString.Rich(defaultInterpolatedStringHandler.ToStringAndClear(), null);
						if (CS$<>8__locals3.CS$<>8__locals2.CS$<>8__locals1.crewManager != null)
						{
							CS$<>8__locals3.btn.OnButtonDown = delegate()
							{
								CS$<>8__locals3.CS$<>8__locals2.CS$<>8__locals1.crewManager.dragOrderTreshold = (float)Math.Max(CS$<>8__locals3.btn.Rect.Width, CS$<>8__locals3.btn.Rect.Height) / 2f;
								CS$<>8__locals3.CS$<>8__locals2.CS$<>8__locals1.crewManager.DraggedOrderPrefab = CS$<>8__locals3.CS$<>8__locals2.orderPrefab;
								CS$<>8__locals3.CS$<>8__locals2.CS$<>8__locals1.crewManager.dropOrder = false;
								CS$<>8__locals3.CS$<>8__locals2.CS$<>8__locals1.crewManager.framesToSkip = 2;
								CS$<>8__locals3.CS$<>8__locals2.CS$<>8__locals1.crewManager.dragPoint = CS$<>8__locals3.btn.Rect.Center.ToVector2();
								return true;
							};
						}
						GUIFrame guiframe = new GUIFrame(new RectTransform(new Vector2(1.5f), CS$<>8__locals3.btn.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "OuterGlowCircular", null);
						guiframe.Color = GUIStyle.Red * 0.8f;
						guiframe.HoverColor = GUIStyle.Red * 1f;
						guiframe.PressedColor = GUIStyle.Red * 0.6f;
						guiframe.UserData = "highlighted";
						guiframe.CanBeFocused = false;
						guiframe.Visible = false;
						GUIImage guiimage = new GUIImage(new RectTransform(Vector2.One, CS$<>8__locals3.btn.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), CS$<>8__locals3.CS$<>8__locals2.orderPrefab.SymbolSprite, true, null);
						guiimage.Color = CS$<>8__locals3.CS$<>8__locals2.orderPrefab.Color;
						guiimage.HoverColor = Color.Lerp(CS$<>8__locals3.CS$<>8__locals2.orderPrefab.Color, Color.White, 0.5f);
						guiimage.ToolTip = CS$<>8__locals3.btn.ToolTip;
						guiimage.SpriteEffects = SpriteEffects.FlipHorizontally;
						guiimage.UserData = CS$<>8__locals3.CS$<>8__locals2.orderPrefab;
					}
				}
			}
		}

		// Token: 0x06000DDC RID: 3548 RVA: 0x0007F86C File Offset: 0x0007DA6C
		public IEnumerable<Character> GetCharacters()
		{
			return this.characters;
		}

		// Token: 0x06000DDD RID: 3549 RVA: 0x0007F874 File Offset: 0x0007DA74
		public Rectangle GetActiveCrewArea()
		{
			return this.crewArea.Rect;
		}

		// Token: 0x06000DDE RID: 3550 RVA: 0x0007F884 File Offset: 0x0007DA84
		public GUIComponent AddCharacterToCrewList(Character character)
		{
			if (character == null)
			{
				return null;
			}
			if (this.crewList.Content.Children.Any((GUIComponent c) => c.UserData as Character == character))
			{
				return null;
			}
			GUIFrame guiframe = new GUIFrame(new RectTransform(this.crewListEntrySize, this.crewList.Content.RectTransform, Anchor.TopRight, null, ScaleBasis.Normal, false), "CrewListBackground", null);
			guiframe.UserData = character;
			guiframe.OnSecondaryClicked = delegate(GUIComponent comp, object data)
			{
				if (data == null)
				{
					return false;
				}
				NetworkMember networkMember = GameMain.NetworkMember;
				Client client2;
				if (networkMember == null)
				{
					client2 = null;
				}
				else
				{
					IReadOnlyList<Client> connectedClients = networkMember.ConnectedClients;
					client2 = ((connectedClients != null) ? connectedClients.Find((Client c) => c.Character == data) : null);
				}
				Client client = client2;
				if (client != null)
				{
					NetLobbyScreen.CreateModerationContextMenu(client);
					return true;
				}
				return false;
			};
			GUIFrame background = guiframe;
			CrewManager.SetCharacterComponentTooltip(background);
			float iconRelativeWidth = (float)this.crewListEntrySize.Y / (float)background.Rect.Width;
			GUILayoutGroup layoutGroup = new GUILayoutGroup(new RectTransform(Vector2.One, background.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				CanBeFocused = false,
				RelativeSpacing = 0.1f * iconRelativeWidth,
				UserData = character
			};
			float commandButtonAbsoluteHeight = Math.Min(40f, 0.67f * (float)background.Rect.Height);
			float paddingRelativeWidth = 0.35f * commandButtonAbsoluteHeight / (float)background.Rect.Width;
			new GUIFrame(new RectTransform(new Vector2(paddingRelativeWidth, 1f), layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null).CanBeFocused = false;
			bool isJobIconVisible = this.crewListEntrySize.X >= 220;
			if (isJobIconVisible)
			{
				GUIImage jobIconBackground = new GUIImage(new RectTransform(new Vector2(0.8f * iconRelativeWidth, 0.8f), layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), this.jobIndicatorBackground, true, null)
				{
					CanBeFocused = false,
					UserData = "job"
				};
				Character character2 = character;
				bool flag;
				if (character2 == null)
				{
					flag = (null != null);
				}
				else
				{
					CharacterInfo info = character2.Info;
					if (info == null)
					{
						flag = (null != null);
					}
					else
					{
						JobPrefab prefab = info.Job.Prefab;
						flag = (((prefab != null) ? prefab.Icon : null) != null);
					}
				}
				if (flag)
				{
					GUIImage guiimage = new GUIImage(new RectTransform(Vector2.One, jobIconBackground.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), character.Info.Job.Prefab.Icon, true, null);
					guiimage.CanBeFocused = false;
					guiimage.Color = character.Info.Job.Prefab.UIColor;
					guiimage.HoverColor = character.Info.Job.Prefab.UIColor;
					guiimage.PressedColor = character.Info.Job.Prefab.UIColor;
					guiimage.SelectedColor = character.Info.Job.Prefab.UIColor;
				}
			}
			int iconsVisible = isJobIconVisible ? 6 : 5;
			float nameRelativeWidth = 1f - paddingRelativeWidth - (float)iconsVisible * 0.8f * iconRelativeWidth - 0.1f * iconRelativeWidth - 7f * layoutGroup.RelativeSpacing;
			nameRelativeWidth = Math.Max(nameRelativeWidth, 0.25f);
			GUIFont font = (layoutGroup.Rect.Width < 150) ? GUIStyle.SmallFont : GUIStyle.Font;
			RectTransform rectTransform = new RectTransform(new Vector2(nameRelativeWidth, 1f), layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			rectTransform.MaxSize = new Point(150, background.Rect.Height);
			RichString text = "";
			GUIFont font2 = font;
			CharacterInfo info2 = character.Info;
			Color? textColor;
			if (info2 == null)
			{
				textColor = null;
			}
			else
			{
				Job job = info2.Job;
				if (job == null)
				{
					textColor = null;
				}
				else
				{
					JobPrefab prefab2 = job.Prefab;
					textColor = ((prefab2 != null) ? new Color?(prefab2.UIColor) : null);
				}
			}
			GUITextBlock nameBlock = new GUITextBlock(rectTransform, text, textColor, font2, Alignment.Left, false, "", null)
			{
				CanBeFocused = false,
				UserData = "name"
			};
			nameBlock.Text = ToolBox.LimitString(character.Name, font, nameBlock.Rect.Width);
			new GUIImage(new RectTransform(new Vector2(0.1f * iconRelativeWidth, 0.5f), layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "VerticalLine", GUIImage.ScalingMode.None).CanBeFocused = false;
			GUILayoutGroup orderGroup = new GUILayoutGroup(new RectTransform(new Vector2(2.4f * iconRelativeWidth, 0.8f), layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				CanBeFocused = false,
				Stretch = true
			};
			GUIListBox currentOrderList = new GUIListBox(new RectTransform(new Vector2(0f, 1f), orderGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, null, null, true, false)
			{
				AllowMouseWheelScroll = false,
				CurrentDragMode = GUIListBox.DragMode.DragWithinBox,
				KeepSpaceForScrollBar = false,
				OnRearranged = new GUIListBox.OnRearrangedHandler(this.OnOrdersRearranged),
				ScrollBarVisible = false,
				Spacing = 2,
				UserData = character
			};
			currentOrderList.RectTransform.IsFixedSize = true;
			GUIListBox guilistBox = currentOrderList;
			guilistBox.OnAddedToGUIUpdateList = (Action<GUIComponent>)Delegate.Combine(guilistBox.OnAddedToGUIUpdateList, new Action<GUIComponent>(delegate(GUIComponent component)
			{
				GUIListBox list = component as GUIListBox;
				if (list != null)
				{
					list.CanBeFocused = CrewManager.CanIssueOrders;
					list.CurrentDragMode = ((CrewManager.CanIssueOrders && list.Content.CountChildren > 1) ? GUIListBox.DragMode.DragWithinBox : GUIListBox.DragMode.NoDragging);
				}
			}));
			GUILayoutGroup guilayoutGroup = new GUILayoutGroup(new RectTransform(Vector2.One, orderGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft);
			guilayoutGroup.CanBeFocused = false;
			guilayoutGroup.Stretch = false;
			GUIFrame extraIconFrame = new GUIFrame(new RectTransform(new Vector2(0.8f * iconRelativeWidth * 2f, 0.8f), layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
			{
				CanBeFocused = false,
				UserData = "extraicons"
			};
			GUIFrame soundIconParent = new GUIFrame(new RectTransform(new Vector2(0.8f), extraIconFrame.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Smallest), null, null)
			{
				CanBeFocused = false,
				UserData = "soundicons",
				Visible = character.IsPlayer
			};
			GUIImage guiimage2 = new GUIImage(new RectTransform(Vector2.One, soundIconParent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), GUIStyle.GetComponentStyle("GUISoundIcon").GetDefaultSprite(), true, null);
			guiimage2.CanBeFocused = false;
			guiimage2.UserData = new Pair<string, float>("soundicon", 0f);
			guiimage2.Visible = true;
			GUIImage guiimage3 = new GUIImage(new RectTransform(Vector2.One, soundIconParent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "GUISoundIconDisabled", true);
			guiimage3.CanBeFocused = true;
			guiimage3.UserData = "soundicondisabled";
			guiimage3.Visible = false;
			GUIButton guibutton = new GUIButton(new RectTransform(new Point((int)commandButtonAbsoluteHeight), background.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), Alignment.Center, "CrewListCommandButton", null);
			guibutton.ToolTip = TextManager.Get("inputtype.command");
			guibutton.OnClicked = delegate(GUIButton component, object userData)
			{
				if (!CrewManager.CanIssueOrders)
				{
					return false;
				}
				this.CreateCommandUI(character, false);
				return true;
			};
			if (character.IsBot)
			{
				GUIFrame guiframe2 = new GUIFrame(new RectTransform(Vector2.One, extraIconFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Smallest), null, null);
				guiframe2.CanBeFocused = false;
				guiframe2.UserData = "objectiveicon";
				guiframe2.Visible = false;
			}
			else
			{
				GameSession gameSession = GameMain.GameSession;
				if (gameSession != null && gameSession.TraitorsEnabled && GameMain.Client != null && character != Character.Controlled)
				{
					Client targetClient = GameMain.Client.ConnectedClients.FirstOrDefault((Client c) => c.Character == character);
					OrderPrefab order;
					if (targetClient != null && OrderPrefab.Prefabs.TryGet("reporttraitor", out order))
					{
						GUITickBox guitickBox = new GUITickBox(new RectTransform(Vector2.One, extraIconFrame.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Smallest), string.Empty, null, "TraitorVoteButton");
						guitickBox.UserData = character;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(20, 2);
						defaultInterpolatedStringHandler.AppendLiteral("‖color:");
						defaultInterpolatedStringHandler.AppendFormatted(GUIStyle.TextColorBright.ToStringHex());
						defaultInterpolatedStringHandler.AppendLiteral("‖");
						defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(TextManager.Get("traitor.blamebutton"));
						defaultInterpolatedStringHandler.AppendLiteral("‖color:end‖\n");
						guitickBox.ToolTip = RichString.Rich(defaultInterpolatedStringHandler.ToStringAndClear() + TextManager.Get("traitor.blamebutton.tooltip"), null);
						guitickBox.OnSelected = delegate(GUITickBox obj)
						{
							foreach (GUITickBox traitorBtn in this.traitorButtons)
							{
								if (traitorBtn != obj)
								{
									traitorBtn.SetSelected(false, false);
								}
							}
							GameClient client = GameMain.Client;
							if (client != null)
							{
								client.Vote(VoteType.Traitor, obj.Selected ? targetClient : null);
							}
							return true;
						};
						GUITickBox voteTraitorBtn = guitickBox;
						this.traitorButtons.Add(voteTraitorBtn);
					}
				}
			}
			GameSession gameSession2 = GameMain.GameSession;
			HRManagerUI hrmanagerUI;
			if (gameSession2 == null)
			{
				hrmanagerUI = null;
			}
			else
			{
				CampaignMode campaign = gameSession2.Campaign;
				if (campaign == null)
				{
					hrmanagerUI = null;
				}
				else
				{
					CampaignUI campaignUI = campaign.CampaignUI;
					hrmanagerUI = ((campaignUI != null) ? campaignUI.HRManagerUI : null);
				}
			}
			HRManagerUI crewManagement = hrmanagerUI;
			if (crewManagement != null)
			{
				crewManagement.RefreshUI();
			}
			return background;
		}

		// Token: 0x06000DDF RID: 3551 RVA: 0x00080310 File Offset: 0x0007E510
		public void RemoveCharacterFromCrewList(Character character)
		{
			CrewManager.<>c__DisplayClass40_0 CS$<>8__locals1 = new CrewManager.<>c__DisplayClass40_0();
			CrewManager.<>c__DisplayClass40_0 CS$<>8__locals2 = CS$<>8__locals1;
			GUIListBox guilistBox = this.crewList;
			CS$<>8__locals2.component = ((guilistBox != null) ? guilistBox.Content.GetChildByUserData(character) : null);
			if (CS$<>8__locals1.component != null)
			{
				this.crewList.RemoveChild(CS$<>8__locals1.component);
				this.traitorButtons.RemoveAll((GUITickBox t) => t.IsChildOf(CS$<>8__locals1.component, true));
			}
			GameSession gameSession = GameMain.GameSession;
			HRManagerUI hrmanagerUI;
			if (gameSession == null)
			{
				hrmanagerUI = null;
			}
			else
			{
				CampaignMode campaign = gameSession.Campaign;
				if (campaign == null)
				{
					hrmanagerUI = null;
				}
				else
				{
					CampaignUI campaignUI = campaign.CampaignUI;
					hrmanagerUI = ((campaignUI != null) ? campaignUI.HRManagerUI : null);
				}
			}
			HRManagerUI crewManagement = hrmanagerUI;
			if (crewManagement != null)
			{
				crewManagement.RefreshUI();
			}
		}

		// Token: 0x06000DE0 RID: 3552 RVA: 0x000803A8 File Offset: 0x0007E5A8
		private static void SetCharacterComponentTooltip(GUIComponent characterComponent)
		{
			Character character = ((characterComponent != null) ? characterComponent.UserData : null) as Character;
			if (character == null)
			{
				return;
			}
			CharacterInfo info = character.Info;
			bool flag;
			if (info == null)
			{
				flag = (null != null);
			}
			else
			{
				Job job = info.Job;
				flag = (((job != null) ? job.Prefab : null) != null);
			}
			if (!flag)
			{
				return;
			}
			LocalizedString tooltip = TextManager.GetWithVariables("crewlistelementtooltip", new ValueTuple<string, LocalizedString>[]
			{
				new ValueTuple<string, LocalizedString>("[name]", character.Name),
				new ValueTuple<string, LocalizedString>("[job]", character.Info.Job.Name)
			});
			string color = XMLExtensions.ColorToString(character.Info.Job.Prefab.UIColor);
			RichString richToolTip = RichString.Rich("‖color:" + color + "‖" + tooltip + "‖color:end‖", null);
			characterComponent.ToolTip = richToolTip;
		}

		// Token: 0x06000DE1 RID: 3553 RVA: 0x00080490 File Offset: 0x0007E690
		public bool CharacterClicked(GUIComponent component, object selection)
		{
			if (!this.AllowCharacterSwitch)
			{
				return false;
			}
			Character character = selection as Character;
			if (character == null || !character.IsOnPlayerTeam)
			{
				return false;
			}
			if (GameMain.IsMultiplayer)
			{
				if (Character.Controlled == null)
				{
					Camera cam = Screen.Selected.Cam;
					cam.Position = character.DrawPosition;
				}
				return true;
			}
			if (character.IsDead || character.IsUnconscious)
			{
				return false;
			}
			this.SelectCharacter(character);
			if (GUI.KeyboardDispatcher.Subscriber == this.crewList)
			{
				GUI.KeyboardDispatcher.Subscriber = null;
			}
			return true;
		}

		// Token: 0x06000DE2 RID: 3554 RVA: 0x0008051C File Offset: 0x0007E71C
		public void ReviveCharacter(Character revivedCharacter)
		{
			GUIComponent characterComponent = this.crewList.Content.GetChildByUserData(revivedCharacter);
			if (characterComponent != null)
			{
				this.crewList.Content.RemoveChild(characterComponent);
			}
			if (this.characterInfos.Contains(revivedCharacter.Info))
			{
				this.AddCharacter(revivedCharacter);
			}
		}

		// Token: 0x06000DE3 RID: 3555 RVA: 0x0008056C File Offset: 0x0007E76C
		public void KillCharacter(Character killedCharacter, bool resetCrewListIndex = true)
		{
			GUIComponent characterComponent = this.crewList.Content.GetChildByUserData(killedCharacter);
			if (characterComponent != null)
			{
				CoroutineManager.StartCoroutine(this.KillCharacterAnim(characterComponent), "");
			}
			this.RemoveCharacter(killedCharacter, false, resetCrewListIndex);
		}

		// Token: 0x06000DE4 RID: 3556 RVA: 0x000805A9 File Offset: 0x0007E7A9
		private IEnumerable<CoroutineStatus> KillCharacterAnim(GUIComponent component)
		{
			CrewManager.<KillCharacterAnim>d__45 <KillCharacterAnim>d__ = new CrewManager.<KillCharacterAnim>d__45(-2);
			<KillCharacterAnim>d__.<>4__this = this;
			<KillCharacterAnim>d__.<>3__component = component;
			return <KillCharacterAnim>d__;
		}

		// Token: 0x06000DE5 RID: 3557 RVA: 0x000805C0 File Offset: 0x0007E7C0
		private void OnCrewListRearranged(GUIListBox crewList, object draggedElementData)
		{
			if (crewList != this.crewList)
			{
				return;
			}
			if (!(draggedElementData is Character))
			{
				return;
			}
			if (crewList.HasDraggedElementIndexChanged)
			{
				if (this.IsSinglePlayer)
				{
					this.UpdateCrewListIndices();
					return;
				}
			}
			else
			{
				this.CharacterClicked(crewList.DraggedElement, draggedElementData);
			}
		}

		// Token: 0x06000DE6 RID: 3558 RVA: 0x000805FA File Offset: 0x0007E7FA
		private void ResetCrewListIndex(Character c)
		{
			if (((c != null) ? c.Info : null) == null)
			{
				return;
			}
			c.Info.CrewListIndex = int.MaxValue;
		}

		// Token: 0x06000DE7 RID: 3559 RVA: 0x0008061C File Offset: 0x0007E81C
		private void UpdateCrewListIndices()
		{
			if (this.crewList == null)
			{
				return;
			}
			for (int i = 0; i < this.crewList.Content.CountChildren; i++)
			{
				GUIComponent characterComponent = this.crewList.Content.GetChild(i);
				Character c = ((characterComponent != null) ? characterComponent.UserData : null) as Character;
				if (c != null && c.Info != null)
				{
					c.Info.CrewListIndex = i;
				}
			}
		}

		// Token: 0x06000DE8 RID: 3560 RVA: 0x00080688 File Offset: 0x0007E888
		private void SortCrewList()
		{
			if (this.crewList == null)
			{
				return;
			}
			this.crewList.Content.RectTransform.SortChildren(delegate(RectTransform x, RectTransform y)
			{
				Character character = x.GUIComponent.UserData as Character;
				int? num;
				if (character == null)
				{
					num = null;
				}
				else
				{
					CharacterInfo info = character.Info;
					num = ((info != null) ? new int?(info.CrewListIndex) : null);
				}
				int? index = num;
				Character character2 = y.GUIComponent.UserData as Character;
				int? num2;
				if (character2 == null)
				{
					num2 = null;
				}
				else
				{
					CharacterInfo info2 = character2.Info;
					num2 = ((info2 != null) ? new int?(info2.CrewListIndex) : null);
				}
				int? index2 = num2;
				if (index == null)
				{
					return ((index2 != null) > false) ? 1 : 0;
				}
				if (index2 == null)
				{
					return -1;
				}
				return index.Value.CompareTo(index2.Value);
			});
			this.UpdateCrewListIndices();
		}

		// Token: 0x06000DE9 RID: 3561 RVA: 0x000806D8 File Offset: 0x0007E8D8
		public void AddSinglePlayerChatMessage(LocalizedString senderName, LocalizedString text, ChatMessageType messageType, Entity sender)
		{
			this.AddSinglePlayerChatMessage(senderName.Value, text.Value, messageType, sender);
		}

		// Token: 0x06000DEA RID: 3562 RVA: 0x000806F0 File Offset: 0x0007E8F0
		public void AddSinglePlayerChatMessage(string senderName, string text, ChatMessageType messageType, Entity sender)
		{
			if (!this.IsSinglePlayer)
			{
				DebugConsole.ThrowError("Cannot add messages to single player chat box in multiplayer mode!\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			Character character = sender as Character;
			if (character != null)
			{
				CrewManager crewManager = GameMain.GameSession.CrewManager;
				if (crewManager != null)
				{
					crewManager.SetCharacterSpeaking(character);
				}
				if (!character.IsBot)
				{
					character.TextChatVolume = 1f;
				}
			}
			this.ChatBox.AddMessage(ChatMessage.Create(senderName, text, messageType, sender, null, PlayerConnectionChangeType.None, null));
		}

		// Token: 0x06000DEB RID: 3563 RVA: 0x00080780 File Offset: 0x0007E980
		public void AddSinglePlayerChatMessage(ChatMessage message)
		{
			if (!this.IsSinglePlayer)
			{
				DebugConsole.ThrowError("Cannot add messages to single player chat box in multiplayer mode!\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			if (string.IsNullOrEmpty(message.Text))
			{
				return;
			}
			if (message.SenderCharacter != null)
			{
				CrewManager crewManager = GameMain.GameSession.CrewManager;
				if (crewManager != null)
				{
					crewManager.SetCharacterSpeaking(message.SenderCharacter);
				}
			}
			this.ChatBox.AddMessage(message);
		}

		// Token: 0x06000DEC RID: 3564 RVA: 0x000807F0 File Offset: 0x0007E9F0
		public void SetPlayerVoiceIconState(Client client, bool muted, bool mutedLocally)
		{
			if (((client != null) ? client.Character : null) == null)
			{
				return;
			}
			GUIComponent soundIcons = this.GetSoundIconParent(client.Character);
			if (soundIcons != null)
			{
				GUIComponent soundIcon = soundIcons.FindChild(delegate(GUIComponent c)
				{
					Pair<string, float> pair = c.UserData as Pair<string, float>;
					return pair != null && pair.First == "soundicon";
				}, false);
				GUIComponent soundIconDisabled = soundIcons.FindChild("soundicondisabled", false);
				soundIcon.Visible = (!muted && !mutedLocally);
				soundIconDisabled.Visible = (muted || mutedLocally);
				soundIconDisabled.ToolTip = TextManager.Get(mutedLocally ? "MutedLocally" : "MutedGlobally");
			}
		}

		// Token: 0x06000DED RID: 3565 RVA: 0x00080888 File Offset: 0x0007EA88
		public void SetClientSpeaking(Client client)
		{
			if (((client != null) ? client.Character : null) != null)
			{
				this.SetCharacterSpeaking(client.Character);
			}
		}

		// Token: 0x06000DEE RID: 3566 RVA: 0x000808A4 File Offset: 0x0007EAA4
		public void SetCharacterSpeaking(Character character)
		{
			if (character == null || character.IsBot)
			{
				return;
			}
			GUIComponent soundIconParent = this.GetSoundIconParent(character);
			GUIComponent guicomponent;
			if (soundIconParent == null)
			{
				guicomponent = null;
			}
			else
			{
				guicomponent = soundIconParent.FindChild(delegate(GUIComponent c)
				{
					Pair<string, float> pair = c.UserData as Pair<string, float>;
					return pair != null && pair.First == "soundicon";
				}, false);
			}
			GUIComponent soundIcon = guicomponent;
			if (soundIcon != null)
			{
				soundIcon.Color = Color.White;
				Pair<string, float> userdata = soundIcon.UserData as Pair<string, float>;
				userdata.Second = 1f;
			}
		}

		// Token: 0x06000DEF RID: 3567 RVA: 0x00080918 File Offset: 0x0007EB18
		private GUIComponent GetSoundIconParent(GUIComponent characterComponent)
		{
			if (characterComponent == null)
			{
				return null;
			}
			GUIComponent guicomponent = characterComponent.FindChild((GUIComponent c) => c is GUILayoutGroup, false);
			if (guicomponent == null)
			{
				return null;
			}
			GUIComponent childByUserData = guicomponent.GetChildByUserData("extraicons");
			if (childByUserData == null)
			{
				return null;
			}
			return childByUserData.GetChildByUserData("soundicons");
		}

		// Token: 0x06000DF0 RID: 3568 RVA: 0x00080970 File Offset: 0x0007EB70
		private GUIComponent GetSoundIconParent(Character character)
		{
			GUIListBox guilistBox = this.crewList;
			return this.GetSoundIconParent((guilistBox != null) ? guilistBox.Content.GetChildByUserData(character) : null);
		}

		// Token: 0x06000DF1 RID: 3569 RVA: 0x00080990 File Offset: 0x0007EB90
		public void SetCharacterOrder(Character character, Order order, bool isNewOrder = true)
		{
			if (order != null && order.TargetAllCharacters)
			{
				Hull hull = order.TargetHull;
				if (order.IsReport)
				{
					Character orderGiver2 = order.OrderGiver;
					if (((orderGiver2 != null) ? orderGiver2.CurrentHull : null) == null && hull == null)
					{
						return;
					}
					if (hull == null)
					{
						hull = order.OrderGiver.CurrentHull;
					}
					this.AddOrder(order.WithTargetEntity(hull), new float?(order.FadeOutTime));
				}
				if (order.IsDeconstructOrder)
				{
					Item item = order.TargetEntity as Item;
					if (item == null)
					{
						goto IL_2D7;
					}
					Identifier identifier = order.Identifier;
					if (identifier == Tags.DeconstructThis)
					{
						foreach (Item stackedItem in item.GetStackedItems())
						{
							Item.DeconstructItems.Add(stackedItem);
						}
						HintManager.OnItemMarkedForDeconstruction(order.OrderGiver);
						goto IL_2D7;
					}
					using (IEnumerator<Item> enumerator2 = item.GetStackedItems().GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							Item stackedItem2 = enumerator2.Current;
							Item.DeconstructItems.Remove(stackedItem2);
						}
						goto IL_2D7;
					}
				}
				if (order.IsIgnoreOrder)
				{
					WallSection ws = null;
					if (order.TargetType == Order.OrderTargetType.Entity)
					{
						IIgnorable ignorable = order.TargetEntity as IIgnorable;
						if (ignorable != null)
						{
							Item item2 = ignorable as Item;
							Identifier identifier;
							if (item2 != null)
							{
								using (IEnumerator<Item> enumerator3 = item2.GetStackedItems().GetEnumerator())
								{
									while (enumerator3.MoveNext())
									{
										Item stackedItem3 = enumerator3.Current;
										Item item4 = stackedItem3;
										identifier = order.Identifier;
										item4.OrderedToBeIgnored = (identifier == Tags.IgnoreThis);
										this.AddOrder(order.Clone().WithTargetEntity(stackedItem3), null);
									}
									goto IL_28A;
								}
							}
							IIgnorable ignorable2 = ignorable;
							identifier = order.Identifier;
							ignorable2.OrderedToBeIgnored = (identifier == Tags.IgnoreThis);
							this.AddOrder(order.Clone(), null);
							goto IL_28A;
						}
					}
					if (order.TargetType == Order.OrderTargetType.WallSection)
					{
						Structure s = order.TargetEntity as Structure;
						if (s != null)
						{
							int wallSectionIndex = order.WallSectionIndex ?? s.Sections.IndexOf(this.wallContext);
							ws = s.GetSection(wallSectionIndex);
							if (ws != null)
							{
								WallSection wallSection = ws;
								Identifier identifier = order.Identifier;
								wallSection.OrderedToBeIgnored = (identifier == Tags.IgnoreThis);
								this.AddOrder(order.WithWallSection(s, new int?(wallSectionIndex)), null);
								goto IL_28A;
							}
							goto IL_28A;
						}
					}
					return;
					IL_28A:
					if (ws != null)
					{
						hull = Hull.FindHull(ws.WorldPosition, null, true, true);
					}
					else
					{
						Item item3 = order.TargetEntity as Item;
						if (item3 != null)
						{
							hull = item3.CurrentHull;
						}
						else
						{
							ISpatialEntity se = order.TargetEntity;
							if (se != null)
							{
								hull = Hull.FindHull(se.WorldPosition, null, true, true);
							}
						}
					}
				}
				IL_2D7:
				if (this.IsSinglePlayer)
				{
					Character orderGiver3 = order.OrderGiver;
					if (orderGiver3 == null)
					{
						return;
					}
					string targetCharacterName = "";
					string targetRoomName;
					if (hull == null)
					{
						targetRoomName = null;
					}
					else
					{
						LocalizedString displayName = hull.DisplayName;
						targetRoomName = ((displayName != null) ? displayName.Value : null);
					}
					bool givingOrderToSelf = character == order.OrderGiver;
					Identifier identifier = default(Identifier);
					string chatMessage = order.GetChatMessage(targetCharacterName, targetRoomName, givingOrderToSelf, identifier, isNewOrder);
					ChatMessageType? messageType = new ChatMessageType?(ChatMessageType.Order);
					float delay = 0f;
					identifier = default(Identifier);
					orderGiver3.Speak(chatMessage, messageType, delay, identifier, 0f);
					return;
				}
				else
				{
					OrderChatMessage msg = new OrderChatMessage(order.WithTargetEntity(order.IsReport ? hull : order.TargetEntity), null, order.OrderGiver, isNewOrder);
					GameClient client = GameMain.Client;
					if (client == null)
					{
						return;
					}
					client.SendChatMessage(msg);
					return;
				}
			}
			else
			{
				if (character == null)
				{
					return;
				}
				Character orderGiver = (order != null) ? order.OrderGiver : null;
				if (this.IsSinglePlayer)
				{
					bool isGivingOrderToSelf = orderGiver == character;
					character.SetOrder(order, isNewOrder, !isGivingOrderToSelf, false);
					string text;
					if (order == null)
					{
						text = null;
					}
					else
					{
						string name = character.Name;
						string targetRoomName2;
						if (orderGiver == null)
						{
							targetRoomName2 = null;
						}
						else
						{
							Hull currentHull = orderGiver.CurrentHull;
							if (currentHull == null)
							{
								targetRoomName2 = null;
							}
							else
							{
								LocalizedString displayName2 = currentHull.DisplayName;
								targetRoomName2 = ((displayName2 != null) ? displayName2.Value : null);
							}
						}
						text = order.GetChatMessage(name, targetRoomName2, isGivingOrderToSelf, (order != null) ? order.Option : Identifier.Empty, isNewOrder);
					}
					string message = text;
					if (orderGiver != null)
					{
						Character character2 = orderGiver;
						string message2 = message;
						ChatMessageType? messageType2 = null;
						float delay2 = 0f;
						Identifier identifier = default(Identifier);
						character2.Speak(message2, messageType2, delay2, identifier, 0f);
						return;
					}
				}
				else if (orderGiver != null)
				{
					OrderChatMessage msg2 = new OrderChatMessage(order, character, orderGiver, isNewOrder);
					GameClient client2 = GameMain.Client;
					if (client2 == null)
					{
						return;
					}
					client2.SendChatMessage(msg2);
				}
				return;
			}
		}

		// Token: 0x06000DF2 RID: 3570 RVA: 0x00080E0C File Offset: 0x0007F00C
		public void AddCurrentOrderIcon(Character character, Order order)
		{
			CrewManager.<>c__DisplayClass59_0 CS$<>8__locals1 = new CrewManager.<>c__DisplayClass59_0();
			CS$<>8__locals1.character = character;
			CS$<>8__locals1.<>4__this = this;
			if (CS$<>8__locals1.character == null)
			{
				return;
			}
			GUIComponent characterComponent = this.crewList.Content.GetChildByUserData(CS$<>8__locals1.character);
			if (characterComponent == null)
			{
				return;
			}
			CS$<>8__locals1.currentOrderIconList = this.GetCurrentOrderIconList(characterComponent);
			IEnumerable<GUIComponent> currentOrderIcons = CS$<>8__locals1.currentOrderIconList.Content.Children;
			List<GUIComponent> iconsToRemove = new List<GUIComponent>();
			List<Order> newPreviousOrders = new List<Order>();
			bool updatedExistingIcon = false;
			foreach (GUIComponent icon in currentOrderIcons)
			{
				Order orderInfo = (Order)icon.UserData;
				if (CS$<>8__locals1.character.GetCurrentOrder(orderInfo) == null)
				{
					iconsToRemove.Add(icon);
					newPreviousOrders.Add(orderInfo);
				}
				else if (orderInfo.MatchesOrder(order))
				{
					icon.UserData = order.Clone();
					GUIImage image2 = icon as GUIImage;
					if (image2 != null)
					{
						image2.Sprite = this.GetOrderIconSprite(order);
						image2.ToolTip = this.CreateOrderTooltip(order);
					}
					updatedExistingIcon = true;
				}
			}
			iconsToRemove.ForEach(delegate(GUIComponent c)
			{
				CS$<>8__locals1.currentOrderIconList.RemoveChild(c);
			});
			CS$<>8__locals1.previousOrderIconGroup = this.GetPreviousOrderIconGroup(characterComponent);
			IEnumerable<GUIComponent> previousOrderIcons = CS$<>8__locals1.previousOrderIconGroup.Children;
			foreach (GUIComponent icon2 in previousOrderIcons)
			{
				Order orderInfo2 = (Order)icon2.UserData;
				if (orderInfo2.MatchesOrder(order))
				{
					CS$<>8__locals1.previousOrderIconGroup.RemoveChild(icon2);
					break;
				}
			}
			if (updatedExistingIcon)
			{
				CS$<>8__locals1.<AddCurrentOrderIcon>g__RearrangeIcons|2();
			}
			for (int i = newPreviousOrders.Count - 1; i >= 0; i--)
			{
				this.AddPreviousOrderIcon(CS$<>8__locals1.character, characterComponent, newPreviousOrders[i]);
			}
			bool flag;
			if (order != null)
			{
				Identifier identifier = order.Identifier;
				flag = (identifier == this.dismissedOrderPrefab.Identifier);
			}
			else
			{
				flag = true;
			}
			if (flag || updatedExistingIcon)
			{
				CS$<>8__locals1.<AddCurrentOrderIcon>g__RearrangeIcons|2();
				return;
			}
			int orderIconCount = CS$<>8__locals1.currentOrderIconList.Content.CountChildren + CS$<>8__locals1.previousOrderIconGroup.CountChildren;
			if (orderIconCount >= 3)
			{
				this.RemoveLastOrderIcon(characterComponent);
			}
			float nodeWidth = 0.33333334f * (float)CS$<>8__locals1.currentOrderIconList.Parent.Rect.Width - (float)(2 * CS$<>8__locals1.currentOrderIconList.Spacing);
			Point size = new Point((int)nodeWidth, CS$<>8__locals1.currentOrderIconList.RectTransform.NonScaledSize.Y);
			GUIImage nodeIcon = this.CreateNodeIcon(size, CS$<>8__locals1.currentOrderIconList.Content.RectTransform, this.GetOrderIconSprite(order), order.Color, this.CreateOrderTooltip(order));
			nodeIcon.UserData = order.Clone();
			nodeIcon.OnSecondaryClicked = delegate(GUIComponent image, object userData)
			{
				if (!CrewManager.CanIssueOrders)
				{
					return false;
				}
				Order orderInfo3 = (Order)userData;
				Order dismissal = orderInfo3.GetDismissal();
				Order currentOrder = CS$<>8__locals1.character.GetCurrentOrder(orderInfo3);
				Order order2 = dismissal.WithManualPriority((currentOrder != null) ? currentOrder.ManualPriority : 0).WithOrderGiver(Character.Controlled);
				CS$<>8__locals1.<>4__this.SetCharacterOrder(CS$<>8__locals1.character, order2, true);
				return true;
			};
			GUIFrame guiframe = new GUIFrame(new RectTransform(new Point((int)(1.5f * nodeWidth)), nodeIcon.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), "OuterGlowCircular", null);
			guiframe.CanBeFocused = false;
			guiframe.Color = order.Color;
			guiframe.UserData = "glow";
			guiframe.Visible = false;
			int hierarchyIndex = Math.Clamp(CharacterInfo.HighestManualOrderPriority - order.ManualPriority, 0, Math.Max(CS$<>8__locals1.currentOrderIconList.Content.CountChildren - 1, 0));
			if (hierarchyIndex != CS$<>8__locals1.currentOrderIconList.Content.GetChildIndex(nodeIcon))
			{
				nodeIcon.RectTransform.RepositionChildInHierarchy(hierarchyIndex);
			}
			CS$<>8__locals1.<AddCurrentOrderIcon>g__RearrangeIcons|2();
		}

		// Token: 0x06000DF3 RID: 3571 RVA: 0x000811A4 File Offset: 0x0007F3A4
		private void AddPreviousOrderIcon(Character character, GUIComponent characterComponent, Order orderInfo)
		{
			if (orderInfo != null)
			{
				Identifier identifier = orderInfo.Identifier;
				if (!(identifier == this.dismissedOrderPrefab.Identifier))
				{
					GUIListBox currentOrderIconList = this.GetCurrentOrderIconList(characterComponent);
					int maxPreviousOrderIcons = 3 - currentOrderIconList.Content.CountChildren;
					if (maxPreviousOrderIcons < 1)
					{
						return;
					}
					GUILayoutGroup previousOrderIconGroup = this.GetPreviousOrderIconGroup(characterComponent);
					if (previousOrderIconGroup.CountChildren >= maxPreviousOrderIcons)
					{
						this.RemoveLastPreviousOrderIcon(previousOrderIconGroup);
					}
					float nodeWidth = 0.33333334f * (float)previousOrderIconGroup.Parent.Rect.Width - (float)(2 * currentOrderIconList.Spacing);
					Point size = new Point((int)nodeWidth, previousOrderIconGroup.Rect.Height);
					Order previousOrderInfo = orderInfo.WithType(Order.OrderType.Previous);
					GUIButton prevOrderFrame = new GUIButton(new RectTransform(size, previousOrderIconGroup.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), Alignment.Center, null, null)
					{
						UserData = previousOrderInfo,
						OnClicked = delegate(GUIButton button, object userData)
						{
							if (!CrewManager.CanIssueOrders)
							{
								return false;
							}
							Order orderInfo2 = (Order)userData;
							int priority = this.GetManualOrderPriority(character, orderInfo2);
							this.SetCharacterOrder(character, orderInfo2.WithManualPriority(priority).WithOrderGiver(Character.Controlled), true);
							return true;
						},
						OnSecondaryClicked = delegate(GUIComponent button, object userData)
						{
							if (previousOrderIconGroup == null)
							{
								return false;
							}
							previousOrderIconGroup.RemoveChild(button);
							previousOrderIconGroup.Recalculate();
							return true;
						}
					};
					prevOrderFrame.RectTransform.IsFixedSize = true;
					GUIFrame prevOrderIconFrame = new GUIFrame(new RectTransform(new Vector2(0.8f), prevOrderFrame.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal), null, null);
					this.CreateNodeIcon(Vector2.One, prevOrderIconFrame.RectTransform, this.GetOrderIconSprite(previousOrderInfo), previousOrderInfo.Color, this.CreateOrderTooltip(previousOrderInfo));
					foreach (GUIComponent c in prevOrderIconFrame.Children)
					{
						c.HoverColor = c.Color;
						c.PressedColor = c.Color;
						c.SelectedColor = c.Color;
					}
					new GUIImage(new RectTransform(new Vector2(0.8f), prevOrderFrame.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), this.previousOrderArrow, true, null).CanBeFocused = false;
					prevOrderFrame.SetAsFirstChild();
					return;
				}
			}
		}

		// Token: 0x06000DF4 RID: 3572 RVA: 0x00081414 File Offset: 0x0007F614
		private void AddOldPreviousOrderIcons(Character character, GUIComponent oldCharacterComponent)
		{
			IEnumerable<GUIComponent> oldPrevOrderIcons = this.GetPreviousOrderIconGroup(oldCharacterComponent).Children;
			if (oldPrevOrderIcons.None(null))
			{
				return;
			}
			if (oldPrevOrderIcons.Count<GUIComponent>() > 1)
			{
				oldPrevOrderIcons = oldPrevOrderIcons.Reverse<GUIComponent>();
			}
			GUIComponent newCharacterComponent = this.crewList.Content.Children.FirstOrDefault((GUIComponent c) => c.UserData == character);
			if (newCharacterComponent != null)
			{
				foreach (GUIComponent icon in oldPrevOrderIcons)
				{
					Order orderInfo = icon.UserData as Order;
					if (orderInfo != null)
					{
						this.AddPreviousOrderIcon(character, newCharacterComponent, orderInfo);
					}
				}
			}
		}

		// Token: 0x06000DF5 RID: 3573 RVA: 0x000814D0 File Offset: 0x0007F6D0
		private void RemoveLastOrderIcon(GUIComponent characterComponent)
		{
			GUILayoutGroup previousOrderIconGroup = this.GetPreviousOrderIconGroup(characterComponent);
			if (this.RemoveLastPreviousOrderIcon(previousOrderIconGroup))
			{
				return;
			}
			GUIListBox currentOrderIconList = this.GetCurrentOrderIconList(characterComponent);
			if (currentOrderIconList.Content.CountChildren > 0)
			{
				GUIComponent iconToRemove = currentOrderIconList.Content.Children.Last<GUIComponent>();
				currentOrderIconList.RemoveChild(iconToRemove);
				return;
			}
		}

		// Token: 0x06000DF6 RID: 3574 RVA: 0x00081520 File Offset: 0x0007F720
		private bool RemoveLastPreviousOrderIcon(GUILayoutGroup iconGroup)
		{
			if (iconGroup.CountChildren > 0)
			{
				GUIComponent iconToRemove = iconGroup.Children.Last<GUIComponent>();
				iconGroup.RemoveChild(iconToRemove);
				return true;
			}
			return false;
		}

		// Token: 0x06000DF7 RID: 3575 RVA: 0x0008154C File Offset: 0x0007F74C
		private GUIListBox GetCurrentOrderIconList(GUIComponent characterComponent)
		{
			if (characterComponent == null)
			{
				return null;
			}
			return characterComponent.GetChild<GUILayoutGroup>().GetChild<GUILayoutGroup>().GetChild<GUIListBox>();
		}

		// Token: 0x06000DF8 RID: 3576 RVA: 0x00081563 File Offset: 0x0007F763
		private GUILayoutGroup GetPreviousOrderIconGroup(GUIComponent characterComponent)
		{
			if (characterComponent == null)
			{
				return null;
			}
			return characterComponent.GetChild<GUILayoutGroup>().GetChild<GUILayoutGroup>().GetChild<GUILayoutGroup>();
		}

		// Token: 0x06000DF9 RID: 3577 RVA: 0x0008157C File Offset: 0x0007F77C
		private void OnOrdersRearranged(GUIListBox orderList, object userData)
		{
			GUIComponent orderComponent = orderList.Content.GetChildByUserData(userData);
			if (orderComponent == null)
			{
				return;
			}
			Order orderInfo = (Order)userData;
			int priority = Math.Max(CharacterInfo.HighestManualOrderPriority - orderList.Content.GetChildIndex(orderComponent), 1);
			if (orderInfo.ManualPriority == priority)
			{
				return;
			}
			Character character = (Character)orderList.UserData;
			this.SetCharacterOrder(character, orderInfo.WithManualPriority(priority), false);
		}

		// Token: 0x06000DFA RID: 3578 RVA: 0x000815E0 File Offset: 0x0007F7E0
		private LocalizedString CreateOrderTooltip(OrderPrefab orderPrefab, Identifier option, Entity targetEntity)
		{
			if (orderPrefab == null)
			{
				return "";
			}
			if (option != Identifier.Empty)
			{
				return TextManager.GetWithVariables("crewlistordericontooltip".ToIdentifier(), new ValueTuple<Identifier, LocalizedString>[]
				{
					new ValueTuple<Identifier, LocalizedString>("[ordername]".ToIdentifier(), orderPrefab.Name),
					new ValueTuple<Identifier, LocalizedString>("[orderoption]".ToIdentifier(), orderPrefab.GetOptionName(option))
				});
			}
			Item targetItem = targetEntity as Item;
			if (targetItem != null && targetItem.Prefab.MinimapIcon != null)
			{
				return TextManager.GetWithVariables("crewlistordericontooltip".ToIdentifier(), new ValueTuple<Identifier, LocalizedString>[]
				{
					new ValueTuple<Identifier, LocalizedString>("[ordername]".ToIdentifier(), orderPrefab.Name),
					new ValueTuple<Identifier, LocalizedString>("[orderoption]".ToIdentifier(), targetItem.Name)
				});
			}
			return orderPrefab.Name;
		}

		// Token: 0x06000DFB RID: 3579 RVA: 0x000816C8 File Offset: 0x0007F8C8
		private LocalizedString CreateOrderTooltip(Order order)
		{
			if (order.DisplayGiverInTooltip && order.OrderGiver != null)
			{
				return TextManager.GetWithVariables("crewlistordericontooltip", new ValueTuple<string, LocalizedString>[]
				{
					new ValueTuple<string, LocalizedString>("[ordername]", order.Name),
					new ValueTuple<string, LocalizedString>("[orderoption]", order.OrderGiver.DisplayName)
				});
			}
			return this.CreateOrderTooltip(order.Prefab, order.Option, (order != null) ? order.TargetEntity : null);
		}

		// Token: 0x06000DFC RID: 3580 RVA: 0x0008174C File Offset: 0x0007F94C
		private Sprite GetOrderIconSprite(Order order)
		{
			if (order == null)
			{
				return null;
			}
			Sprite sprite = null;
			if (order.Option != Identifier.Empty && order.Prefab.OptionSprites.Any<KeyValuePair<Identifier, Sprite>>())
			{
				order.Prefab.OptionSprites.TryGetValue(order.Option, out sprite);
			}
			if (sprite == null)
			{
				Item targetItem = order.TargetEntity as Item;
				if (targetItem != null && targetItem.Prefab.MinimapIcon != null)
				{
					sprite = targetItem.Prefab.MinimapIcon;
				}
			}
			return sprite ?? order.SymbolSprite;
		}

		// Token: 0x06000DFD RID: 3581 RVA: 0x000817D4 File Offset: 0x0007F9D4
		private void DrawMiniMapOverlay(SpriteBatch spriteBatch, GUICustomComponent container)
		{
			Submarine sub = container.UserData as Submarine;
			if (((sub != null) ? sub.HullVertices : null) == null)
			{
				return;
			}
			Rectangle dockedBorders = sub.GetDockedBorders(true);
			dockedBorders.Location += sub.WorldPosition.ToPoint();
			float scale = Math.Min((float)container.Rect.Width / (float)dockedBorders.Width, (float)container.Rect.Height / (float)dockedBorders.Height) * 0.9f;
			float displayScale = ConvertUnits.ToDisplayUnits(scale);
			Vector2 offset = (sub.WorldPosition - new Vector2((float)dockedBorders.Center.X, (float)(dockedBorders.Y - dockedBorders.Height / 2))) * scale;
			Vector2 center = container.Rect.Center.ToVector2();
			for (int i = 0; i < sub.HullVertices.Count; i++)
			{
				Vector2 start = sub.HullVertices[i] * displayScale + offset;
				start.Y = -start.Y;
				Vector2 end = sub.HullVertices[(i + 1) % sub.HullVertices.Count] * displayScale + offset;
				end.Y = -end.Y;
				GUI.DrawLine(spriteBatch, center + start, center + end, Color.DarkCyan * Rand.Range(0.3f, 0.35f, Rand.RandSync.Unsynced), 0f, 10f);
			}
		}

		// Token: 0x06000DFE RID: 3582 RVA: 0x00081970 File Offset: 0x0007FB70
		public void AddToGUIUpdateList()
		{
			if (GUI.DisableHUD)
			{
				return;
			}
			if (CoroutineManager.IsCoroutineRunning("LevelTransition") || CoroutineManager.IsCoroutineRunning("SubmarineTransition"))
			{
				return;
			}
			GUIFrame guiframe = this.commandFrame;
			if (guiframe != null)
			{
				guiframe.AddToGUIUpdateList(false, 1);
			}
			if (GUI.DisableUpperHUD)
			{
				return;
			}
			if (GameMain.GraphicsWidth != this.screenResolution.X || GameMain.GraphicsHeight != this.screenResolution.Y || this.prevUIScale != GUI.Scale)
			{
				GUIListBox oldCrewList = this.crewList;
				this.InitProjectSpecific();
				foreach (GUIComponent oldCharacterComponent in oldCrewList.Content.Children)
				{
					Character character = oldCharacterComponent.UserData as Character;
					if (character != null && !character.IsDead && !character.Removed)
					{
						this.AddCharacter(character);
						this.AddOldPreviousOrderIcons(character, oldCharacterComponent);
					}
				}
			}
			GUIComponent guicomponent = this.crewArea;
			GameSession gameSession = GameMain.GameSession;
			CampaignMode campaign = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
			guicomponent.Visible = (campaign == null || (!campaign.ForceMapUI && !campaign.ShowCampaignUI));
			this.guiFrame.AddToGUIUpdateList(false, 0);
		}

		// Token: 0x06000DFF RID: 3583 RVA: 0x00081AB4 File Offset: 0x0007FCB4
		public void SelectNextCharacter()
		{
			if (!this.AllowCharacterSwitch || GameMain.IsMultiplayer || this.characters.None(null))
			{
				return;
			}
			GUIComponent child = this.crewList.Content.GetChild(this.TryAdjustIndex(1));
			Character character = ((child != null) ? child.UserData : null) as Character;
			if (character != null)
			{
				this.SelectCharacter(character);
			}
		}

		// Token: 0x06000E00 RID: 3584 RVA: 0x00081B14 File Offset: 0x0007FD14
		public void SelectPreviousCharacter()
		{
			if (!this.AllowCharacterSwitch || GameMain.IsMultiplayer || this.characters.None(null))
			{
				return;
			}
			GUIComponent child = this.crewList.Content.GetChild(this.TryAdjustIndex(-1));
			Character character = ((child != null) ? child.UserData : null) as Character;
			if (character != null)
			{
				this.SelectCharacter(character);
			}
		}

		// Token: 0x06000E01 RID: 3585 RVA: 0x00081B74 File Offset: 0x0007FD74
		private void SelectCharacter(Character character)
		{
			if (ConversationAction.IsDialogOpen)
			{
				return;
			}
			if (!this.AllowCharacterSwitch)
			{
				return;
			}
			if (character == null || character.Removed)
			{
				return;
			}
			Character controlled = Character.Controlled;
			AIController aiController = (controlled != null) ? controlled.AIController : null;
			if (aiController != null)
			{
				aiController.Reset();
			}
			this.DisableCommandUI();
			Character.Controlled = character;
			HintManager.OnChangeCharacter();
			if (GameSession.TabMenuInstance != null && TabMenu.SelectedTab == TabMenu.InfoFrameTab.Talents)
			{
				GameSession.TabMenuInstance.SelectInfoFrameTab(TabMenu.SelectedTab);
			}
			Item selectedItem = character.SelectedItem;
			if (((selectedItem != null) ? selectedItem.GetComponent<Controller>() : null) == null && character.SelectedCharacter == null)
			{
				this.ResetCrewListOpenState();
				ChatBox.ResetChatBoxOpenState();
				return;
			}
			this.AutoHideCrewList();
			ChatBox.AutoHideChatBox();
		}

		// Token: 0x06000E02 RID: 3586 RVA: 0x00081C1C File Offset: 0x0007FE1C
		private int TryAdjustIndex(int amount)
		{
			if (Character.Controlled == null)
			{
				return 0;
			}
			int currentIndex = this.crewList.Content.GetChildIndex(this.crewList.Content.GetChildByUserData(Character.Controlled));
			if (currentIndex == -1)
			{
				return 0;
			}
			int lastIndex = this.crewList.Content.CountChildren - 1;
			int index = currentIndex + amount;
			for (int i = 0; i < this.crewList.Content.CountChildren; i++)
			{
				if (index > lastIndex)
				{
					index = 0;
				}
				if (index < 0)
				{
					index = lastIndex;
				}
				GUIComponent child = this.crewList.Content.GetChild(index);
				Character character = ((child != null) ? child.UserData : null) as Character;
				if (character != null && character.IsOnPlayerTeam && !character.Removed)
				{
					return index;
				}
				index += amount;
			}
			return 0;
		}

		// Token: 0x06000E03 RID: 3587 RVA: 0x00081CE0 File Offset: 0x0007FEE0
		private bool CreateOrder(OrderPrefab orderPrefab, Hull targetHull = null)
		{
			Character controlled = Character.Controlled;
			Submarine sub = (controlled != null) ? controlled.Submarine : null;
			if (sub == null || sub.TeamID != Character.Controlled.TeamID || sub.Info.IsWreck)
			{
				return false;
			}
			Order order = new Order(orderPrefab, targetHull, null, Character.Controlled, false).WithManualPriority(CharacterInfo.HighestManualOrderPriority);
			this.SetCharacterOrder(null, order, true);
			if (this.IsSinglePlayer)
			{
				HumanAIController.ReportProblem(Character.Controlled, order, null);
			}
			return true;
		}

		// Token: 0x06000E04 RID: 3588 RVA: 0x00081D5C File Offset: 0x0007FF5C
		private void UpdateOrderDrag()
		{
			OrderPrefab orderPrefab = this.DraggedOrderPrefab;
			if (orderPrefab != null)
			{
				if (this.dropOrder)
				{
					if (this.framesToSkip > 0)
					{
						this.framesToSkip--;
						return;
					}
					Hull hull = null;
					GUIFrame frame = GUI.MouseOn as GUIFrame;
					if (frame != null)
					{
						Hull data = frame.UserData as Hull;
						if (data != null)
						{
							hull = data;
						}
						else
						{
							GUIComponent parent = frame.Parent;
							Hull parentData = ((parent != null) ? parent.UserData : null) as Hull;
							if (parentData != null)
							{
								hull = parentData;
							}
						}
					}
					this.framesToSkip = 2;
					this.dropOrder = false;
					this.DraggedOrderPrefab = null;
					if (hull == null)
					{
						GUIComponent mouseOn = GUI.MouseOn;
						if (mouseOn != null && mouseOn.Visible && mouseOn.CanBeFocused)
						{
							return;
						}
					}
					if (hull == null)
					{
						hull = Hull.HullList.FirstOrDefault((Hull h) => h.WorldRect.ContainsWorld(Screen.Selected.Cam.ScreenToWorld(PlayerInput.MousePosition)));
					}
					this.CreateOrder(orderPrefab, hull);
					return;
				}
				else
				{
					this.DragOrder = (this.DragOrder || Vector2.DistanceSquared(this.dragPoint, PlayerInput.MousePosition) > this.dragOrderTreshold * this.dragOrderTreshold);
					if (!PlayerInput.PrimaryMouseButtonHeld())
					{
						if (this.DragOrder)
						{
							this.dropOrder = true;
						}
						else
						{
							this.DraggedOrderPrefab = null;
						}
						this.dragPoint = Vector2.Zero;
						this.DragOrder = false;
					}
				}
			}
		}

		// Token: 0x06000E05 RID: 3589 RVA: 0x00081EAC File Offset: 0x000800AC
		private void SetOrderHighlight(GUIComponent characterComponent, Identifier orderIdentifier, Identifier orderOption)
		{
			if (characterComponent == null)
			{
				return;
			}
			this.RemoveObjectiveIcon(characterComponent);
			GUIListBox currentOrderIconList = this.GetCurrentOrderIconList(characterComponent);
			if (currentOrderIconList != null)
			{
				bool foundMatch = false;
				foreach (GUIComponent orderIcon in currentOrderIconList.Content.Children)
				{
					GUIComponent glowComponent = orderIcon.GetChildByUserData("glow");
					if (glowComponent != null)
					{
						glowComponent.Color = orderIcon.Color;
						if (foundMatch)
						{
							glowComponent.Visible = false;
						}
						else
						{
							Order orderInfo = (Order)orderIcon.UserData;
							foundMatch = orderInfo.MatchesOrder(orderIdentifier, orderOption);
							glowComponent.Visible = foundMatch;
						}
					}
				}
			}
		}

		// Token: 0x06000E06 RID: 3590 RVA: 0x00081F5C File Offset: 0x0008015C
		public void SetOrderHighlight(Character character, Identifier orderIdentifier, Identifier orderOption)
		{
			if (this.crewList == null)
			{
				return;
			}
			GUIComponent characterComponent = this.crewList.Content.GetChildByUserData(character);
			this.SetOrderHighlight(characterComponent, orderIdentifier, orderOption);
		}

		// Token: 0x06000E07 RID: 3591 RVA: 0x00081F90 File Offset: 0x00080190
		private void DisableOrderHighlight(GUIComponent characterComponent)
		{
			GUIListBox currentOrderIconList = this.GetCurrentOrderIconList(characterComponent);
			if (currentOrderIconList != null)
			{
				foreach (GUIComponent orderIcon in currentOrderIconList.Content.Children)
				{
					GUIComponent glowComponent = orderIcon.GetChildByUserData("glow");
					if (glowComponent != null)
					{
						glowComponent.Visible = false;
					}
				}
			}
		}

		// Token: 0x06000E08 RID: 3592 RVA: 0x00081FFC File Offset: 0x000801FC
		private void CreateObjectiveIcon(GUIComponent characterComponent, Sprite sprite, LocalizedString tooltip)
		{
			if (characterComponent != null)
			{
				Character character = characterComponent.UserData as Character;
				if (character != null && !character.IsPlayer)
				{
					this.DisableOrderHighlight(characterComponent);
					GUIFrame objectiveIconFrame = this.GetObjectiveIconParent(characterComponent) as GUIFrame;
					if (objectiveIconFrame != null)
					{
						GUIImage existingObjectiveIcon = objectiveIconFrame.GetChild<GUIImage>();
						if (existingObjectiveIcon == null || existingObjectiveIcon.Sprite != sprite || existingObjectiveIcon.ToolTip != tooltip)
						{
							objectiveIconFrame.ClearChildren();
							if (sprite != null)
							{
								GUIImage objectiveIcon = this.CreateNodeIcon(Vector2.One, objectiveIconFrame.RectTransform, sprite, AIObjective.ObjectiveIconColor, tooltip);
								GUIFrame guiframe = new GUIFrame(new RectTransform(new Vector2(1.5f), objectiveIcon.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "OuterGlowCircular", null);
								guiframe.CanBeFocused = false;
								guiframe.Color = AIObjective.ObjectiveIconColor;
								objectiveIconFrame.Visible = true;
								return;
							}
							objectiveIconFrame.Visible = false;
						}
					}
					return;
				}
			}
		}

		// Token: 0x06000E09 RID: 3593 RVA: 0x000820ED File Offset: 0x000802ED
		public void CreateObjectiveIcon(Character character, Identifier identifier, Identifier option, Entity targetEntity)
		{
			GUIListBox guilistBox = this.crewList;
			this.CreateObjectiveIcon((guilistBox != null) ? guilistBox.Content.GetChildByUserData(character) : null, AIObjective.GetSprite(identifier, option, targetEntity), this.GetObjectiveIconTooltip(identifier, option, targetEntity));
		}

		// Token: 0x06000E0A RID: 3594 RVA: 0x00082120 File Offset: 0x00080320
		private void CreateObjectiveIcon(GUIComponent characterComponent, AIObjective objective)
		{
			this.CreateObjectiveIcon(characterComponent, (objective != null) ? objective.GetSprite() : null, this.GetObjectiveIconTooltip(objective));
		}

		// Token: 0x06000E0B RID: 3595 RVA: 0x0008213C File Offset: 0x0008033C
		private LocalizedString GetObjectiveIconTooltip(Identifier identifier, Identifier option, Entity targetEntity)
		{
			LocalizedString variableValue;
			if (OrderPrefab.Prefabs.ContainsKey(identifier))
			{
				OrderPrefab orderPrefab = OrderPrefab.Prefabs[identifier];
				variableValue = this.CreateOrderTooltip(orderPrefab, option, targetEntity);
			}
			else
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
				defaultInterpolatedStringHandler.AppendLiteral("objective.");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(identifier);
				variableValue = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			if (!variableValue.IsNullOrEmpty())
			{
				return TextManager.GetWithVariable("crewlistobjectivetooltip", "[objective]", variableValue, FormatCapitals.No);
			}
			return variableValue;
		}

		// Token: 0x06000E0C RID: 3596 RVA: 0x000821B4 File Offset: 0x000803B4
		private LocalizedString GetObjectiveIconTooltip(AIObjective objective)
		{
			if (objective != null)
			{
				Identifier identifier = objective.Identifier;
				Identifier option = objective.Option;
				AIObjectiveOperateItem aiobjectiveOperateItem = objective as AIObjectiveOperateItem;
				return this.GetObjectiveIconTooltip(identifier, option, (aiobjectiveOperateItem != null) ? aiobjectiveOperateItem.OperateTarget : null);
			}
			return "";
		}

		// Token: 0x06000E0D RID: 3597 RVA: 0x000821E8 File Offset: 0x000803E8
		private GUIComponent GetObjectiveIconParent(GUIComponent characterComponent)
		{
			if (characterComponent == null)
			{
				return null;
			}
			GUILayoutGroup child = characterComponent.GetChild<GUILayoutGroup>();
			if (child == null)
			{
				return null;
			}
			GUIComponent childByUserData = child.GetChildByUserData("extraicons");
			if (childByUserData == null)
			{
				return null;
			}
			return childByUserData.GetChildByUserData("objectiveicon");
		}

		// Token: 0x06000E0E RID: 3598 RVA: 0x00082218 File Offset: 0x00080418
		private void RemoveObjectiveIcon(GUIComponent characterComponent)
		{
			GUIFrame objectiveIconFrame = this.GetObjectiveIconParent(characterComponent) as GUIFrame;
			if (objectiveIconFrame != null)
			{
				objectiveIconFrame.ClearChildren();
				objectiveIconFrame.Visible = false;
			}
		}

		// Token: 0x170003D1 RID: 977
		// (get) Token: 0x06000E0F RID: 3599 RVA: 0x00082244 File Offset: 0x00080444
		public static bool IsCommandInterfaceOpen
		{
			get
			{
				GameSession gameSession = GameMain.GameSession;
				if (((gameSession != null) ? gameSession.CrewManager : null) != null)
				{
					Screen selected = Screen.Selected;
					if (selected == null || !selected.IsEditor)
					{
						return GameMain.GameSession.CrewManager.commandFrame != null || GameMain.GameSession.CrewManager.WasCommandInterfaceDisabledThisUpdate;
					}
				}
				return false;
			}
		}

		// Token: 0x170003D2 RID: 978
		// (get) Token: 0x06000E10 RID: 3600 RVA: 0x00082299 File Offset: 0x00080499
		private OrderPrefab dismissedOrderPrefab
		{
			get
			{
				return OrderPrefab.Dismissal;
			}
		}

		// Token: 0x170003D3 RID: 979
		// (get) Token: 0x06000E11 RID: 3601 RVA: 0x000822A0 File Offset: 0x000804A0
		// (set) Token: 0x06000E12 RID: 3602 RVA: 0x000822A8 File Offset: 0x000804A8
		private bool WasCommandInterfaceDisabledThisUpdate { get; set; }

		// Token: 0x170003D4 RID: 980
		// (get) Token: 0x06000E13 RID: 3603 RVA: 0x000822B1 File Offset: 0x000804B1
		public static bool CanIssueOrders
		{
			get
			{
				Character controlled = Character.Controlled;
				return ((controlled != null) ? controlled.Info : null) != null && Character.Controlled.SpeechImpediment < 100f;
			}
		}

		// Token: 0x06000E14 RID: 3604 RVA: 0x000822DC File Offset: 0x000804DC
		private bool CanCharacterBeHeard()
		{
			if (Character.Controlled == null)
			{
				return false;
			}
			if (this.characterContext != null)
			{
				return this.characterContext.CanHearCharacter(Character.Controlled);
			}
			if (!this.characters.Any((Character c) => c != Character.Controlled && c.CanHearCharacter(Character.Controlled)))
			{
				return this.GetOrderableFriendlyNPCs().Any((Character c) => c != Character.Controlled && c.CanHearCharacter(Character.Controlled));
			}
			return true;
		}

		// Token: 0x06000E15 RID: 3605 RVA: 0x00082364 File Offset: 0x00080564
		private Entity FindEntityContext()
		{
			Character controlled = Character.Controlled;
			Character focusedCharacter = (controlled != null) ? controlled.FocusedCharacter : null;
			if (focusedCharacter != null && !focusedCharacter.IsDead && HumanAIController.IsFriendly(Character.Controlled, focusedCharacter, false, false) && Character.Controlled.TeamID == focusedCharacter.TeamID)
			{
				Character controlled2 = Character.Controlled;
				if (((controlled2 != null) ? controlled2.FocusedItem : null) == null)
				{
					return focusedCharacter;
				}
				Vector2 mousePos = GameMain.GameScreen.Cam.ScreenToWorld(PlayerInput.MousePosition);
				if (Vector2.Distance(mousePos, focusedCharacter.WorldPosition) < Vector2.Distance(mousePos, Character.Controlled.FocusedItem.WorldPosition))
				{
					return focusedCharacter;
				}
				return Character.Controlled.FocusedItem;
			}
			else
			{
				Hull breachedHull;
				if (this.TryGetBreachedHullAtHoveredWall(out breachedHull, out this.wallContext))
				{
					return breachedHull;
				}
				Character controlled3 = Character.Controlled;
				if (controlled3 == null)
				{
					return null;
				}
				return controlled3.FocusedItem;
			}
		}

		// Token: 0x06000E16 RID: 3606 RVA: 0x00082430 File Offset: 0x00080630
		public void OpenCommandUI(Entity entityContext = null, bool forceContextual = false)
		{
			this.CreateCommandUI(entityContext, forceContextual);
			SoundPlayer.PlayUISound(GUISoundType.PopupMenu);
			this.clicklessSelectionActive = (this.isOpeningClick = true);
		}

		// Token: 0x06000E17 RID: 3607 RVA: 0x0008245C File Offset: 0x0008065C
		private void CreateCommandUI(Entity entityContext = null, bool forceContextual = false)
		{
			if (this.commandFrame != null)
			{
				this.DisableCommandUI();
			}
			this.isContextual = forceContextual;
			Character character = entityContext as Character;
			if (character != null && character.Info != null)
			{
				this.characterContext = character;
				this.itemContext = null;
				this.hullContext = null;
				this.wallContext = null;
				this.isContextual = false;
			}
			else
			{
				Item item = entityContext as Item;
				if (item != null)
				{
					this.itemContext = item;
					this.characterContext = null;
					this.hullContext = null;
					this.wallContext = null;
					this.isContextual = true;
				}
				else
				{
					Hull hull = entityContext as Hull;
					if (hull != null)
					{
						this.hullContext = hull;
						this.characterContext = null;
						this.itemContext = null;
						this.isContextual = true;
					}
				}
			}
			this.ScaleCommandUI();
			this.commandFrame = new GUIFrame(new RectTransform(Vector2.One, GUI.Canvas, Anchor.Center, null, null, null, ScaleBasis.Normal), null, new Color?(Color.Transparent));
			this.background = new GUIImage(new RectTransform(Vector2.One, this.commandFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "CommandBackground", GUIImage.ScalingMode.None);
			this.background.Color = this.background.Color * 0.8f;
			GUIButton startNode = null;
			if (this.characterContext == null)
			{
				startNode = new GUIButton(new RectTransform(this.centerNodeSize, this.commandFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), Alignment.Center, null, null);
				this.CreateNodeIcon(startNode.RectTransform, "CommandStartNode", null, null);
			}
			else
			{
				startNode = new GUIButton(new RectTransform(this.centerNodeSize, this.commandFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), Alignment.Center, null, null);
				GUIImage guiimage = new GUIImage(new RectTransform(Vector2.One, startNode.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "CommandNodeContainer", true);
				CharacterInfo info = this.characterContext.Info;
				bool flag;
				if (info == null)
				{
					flag = (null != null);
				}
				else
				{
					Job job = info.Job;
					flag = (((job != null) ? job.Prefab : null) != null);
				}
				guiimage.Color = (flag ? (this.characterContext.Info.Job.Prefab.UIColor * 0.75f) : Color.White);
				CharacterInfo info2 = this.characterContext.Info;
				bool flag2;
				if (info2 == null)
				{
					flag2 = (null != null);
				}
				else
				{
					Job job2 = info2.Job;
					flag2 = (((job2 != null) ? job2.Prefab : null) != null);
				}
				guiimage.HoverColor = (flag2 ? this.characterContext.Info.Job.Prefab.UIColor : Color.White);
				guiimage.UserData = "colorsource";
				GUICustomComponent characterIcon = new GUICustomComponent(new RectTransform(Vector2.One, startNode.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), delegate(SpriteBatch spriteBatch, GUICustomComponent _)
				{
					Character character2 = entityContext as Character;
					if (character2 == null || ((character2 != null) ? character2.Info : null) == null)
					{
						return;
					}
					GUIButton node = startNode;
					character2.Info.DrawJobIcon(spriteBatch, new Rectangle((int)((float)node.Rect.X + (float)node.Rect.Width * 0.5f), (int)((float)node.Rect.Y + (float)node.Rect.Height * 0.1f), (int)((float)node.Rect.Width * 0.6f), (int)((float)node.Rect.Height * 0.8f)), false);
					character2.Info.DrawIcon(spriteBatch, new Vector2((float)node.Rect.X + (float)node.Rect.Width * 0.35f, node.Center.Y), node.Rect.Size.ToVector2() * 0.7f, false);
				}, null);
				this.SetCharacterTooltip(characterIcon, entityContext as Character);
			}
			this.SetCenterNode(startNode, false);
			if (this.availableCategories == null)
			{
				this.availableCategories = this.GetAvailableCategories();
			}
			if (this.isContextual)
			{
				this.CreateContextualOrderNodes();
			}
			else
			{
				this.CreateShortcutNodes();
				this.CreateOrderCategoryNodes();
			}
			this.CreateNodeConnectors();
			if (Character.Controlled != null)
			{
				Character.Controlled.FollowCursor = false;
			}
			HintManager.OnShowCommandInterface();
		}

		// Token: 0x06000E18 RID: 3608 RVA: 0x00082824 File Offset: 0x00080A24
		public void ToggleCommandUI()
		{
			if (this.commandFrame == null)
			{
				if (CrewManager.CanIssueOrders)
				{
					this.CreateCommandUI(null, false);
					return;
				}
			}
			else
			{
				this.DisableCommandUI();
			}
		}

		// Token: 0x06000E19 RID: 3609 RVA: 0x00082844 File Offset: 0x00080A44
		private void ScaleCommandUI()
		{
			this.nodeSize = new Point((int)(100f * GUI.Scale));
			this.centerNodeSize = this.nodeSize;
			this.returnNodeSize = new Point((int)(48f * GUI.Scale));
			this.assignmentNodeSize = new Point((int)(64f * GUI.Scale));
			this.shortcutCenterNodeSize = this.returnNodeSize;
			this.shortcutNodeSize = this.assignmentNodeSize;
			this.centerNodeMargin = (float)this.centerNodeSize.X * 0.5f;
			this.optionNodeMargin = (float)this.nodeSize.X * 0.5f;
			this.shortcutCenterNodeMargin = (float)this.shortcutCenterNodeSize.X * 0.45f;
			this.shortcutNodeMargin = (float)this.shortcutNodeSize.X * 0.5f;
			this.returnNodeMargin = (float)this.returnNodeSize.X * 0.5f;
			this.nodeDistance = (int)(150f * GUI.Scale);
			this.shorcutCenterNodeOffset = new Point(0, (int)(1.35f * (float)this.nodeDistance));
		}

		// Token: 0x06000E1A RID: 3610 RVA: 0x00082960 File Offset: 0x00080B60
		private List<OrderCategory> GetAvailableCategories()
		{
			this.availableCategories = new List<OrderCategory>();
			using (IEnumerator enumerator = Enum.GetValues(typeof(OrderCategory)).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					OrderCategory category = (OrderCategory)enumerator.Current;
					if (OrderPrefab.Prefabs.Any(delegate(OrderPrefab o)
					{
						OrderCategory? category = o.Category;
						OrderCategory category2 = category;
						return (category.GetValueOrDefault() == category2 & category != null) && !o.IsReport;
					}))
					{
						this.availableCategories.Add(category);
					}
				}
			}
			return this.availableCategories;
		}

		// Token: 0x06000E1B RID: 3611 RVA: 0x00082A00 File Offset: 0x00080C00
		private void CreateNodeConnectors()
		{
			this.nodeConnectors = new GUICustomComponent(new RectTransform(Vector2.One, this.commandFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), new Action<SpriteBatch, GUICustomComponent>(this.DrawNodeConnectors), null)
			{
				CanBeFocused = false
			};
			this.nodeConnectors.SetAsFirstChild();
			this.background.SetAsFirstChild();
		}

		// Token: 0x06000E1C RID: 3612 RVA: 0x00082A74 File Offset: 0x00080C74
		private void DrawNodeConnectors(SpriteBatch spriteBatch, GUIComponent container)
		{
			if (this.centerNode == null || this.optionNodes == null)
			{
				return;
			}
			Vector2 startNodePos = this.centerNode.Rect.Center.ToVector2();
			CrewManager.OptionNode optionNode = this.optionNodes.FirstOrDefault<CrewManager.OptionNode>();
			if (!(((optionNode != null) ? optionNode.Button.UserData : null) is Character))
			{
				if (this.targetFrame == null || !this.targetFrame.Visible)
				{
					this.optionNodes.ForEach(delegate(CrewManager.OptionNode n)
					{
						this.DrawNodeConnector(startNodePos, this.centerNodeMargin, n.Button, this.optionNodeMargin, spriteBatch, 1f);
					});
				}
				else
				{
					foreach (CrewManager.OptionNode node in this.optionNodes)
					{
						float iconRadius = 0.5f * this.optionNodeMargin;
						Vector2 itemPosition = node.Button.Parent.Rect.Center.ToVector2();
						if (Vector2.Distance(node.Button.Center, itemPosition) > iconRadius)
						{
							this.DrawNodeConnector(itemPosition, 0f, node.Button, iconRadius, spriteBatch, 0.5f);
							SpriteBatch spriteBatch2 = spriteBatch;
							Vector2 start = itemPosition - Vector2.One;
							Vector2 size = new Vector2(3f);
							GUIComponent childByUserData = node.Button.GetChildByUserData("colorsource");
							GUI.DrawFilledRectangle(spriteBatch2, start, size, (childByUserData != null) ? childByUserData.Color : Color.White, 0f);
						}
					}
				}
			}
			this.DrawNodeConnector(startNodePos, this.centerNodeMargin, this.returnNode, this.returnNodeMargin, spriteBatch, 1f);
			if (this.shortcutCenterNode == null || !this.shortcutCenterNode.Visible)
			{
				return;
			}
			this.DrawNodeConnector(startNodePos, this.centerNodeMargin, this.shortcutCenterNode, this.shortcutCenterNodeMargin, spriteBatch, 1f);
			startNodePos = this.shortcutCenterNode.Rect.Center.ToVector2();
			this.shortcutNodes.ForEach(delegate(GUIComponent n)
			{
				this.DrawNodeConnector(startNodePos, this.shortcutCenterNodeMargin, n, this.shortcutNodeMargin, spriteBatch, 1f);
			});
		}

		// Token: 0x06000E1D RID: 3613 RVA: 0x00082CB4 File Offset: 0x00080EB4
		private void DrawNodeConnector(Vector2 startNodePos, float startNodeMargin, GUIComponent endNode, float endNodeMargin, SpriteBatch spriteBatch, float widthMultiplier = 1f)
		{
			if (endNode == null || !endNode.Visible)
			{
				return;
			}
			Vector2 endNodePos = endNode.Rect.Center.ToVector2();
			Vector2 direction = (endNodePos - startNodePos) / Vector2.Distance(startNodePos, endNodePos);
			Vector2 start = startNodePos + direction * startNodeMargin;
			Vector2 end = endNodePos - direction * endNodeMargin;
			GUIComponent colorSource = endNode.GetChildByUserData("colorsource");
			if (this.selectedNode != null || endNode == this.shortcutCenterNode || !GUI.IsMouseOn(endNode))
			{
				if (this.isSelectionHighlighted)
				{
					if (endNode == this.selectedNode)
					{
						goto IL_C2;
					}
					if (endNode == this.shortcutCenterNode)
					{
						if (this.shortcutNodes.Any((GUIComponent n) => GUI.IsMouseOn(n)))
						{
							goto IL_C2;
						}
					}
				}
				GUI.DrawLine(spriteBatch, start, end, (colorSource != null) ? colorSource.Color : (Color.White * 0.75f), 0f, Math.Max(widthMultiplier * 2f, 1f));
				return;
			}
			IL_C2:
			GUI.DrawLine(spriteBatch, start, end, (colorSource != null) ? colorSource.HoverColor : Color.White, 0f, Math.Max(widthMultiplier * 4f, 1f));
		}

		// Token: 0x06000E1E RID: 3614 RVA: 0x00082DF4 File Offset: 0x00080FF4
		public void DisableCommandUI()
		{
			if (this.commandFrame == null)
			{
				return;
			}
			this.WasCommandInterfaceDisabledThisUpdate = true;
			this.RemoveOptionNodes();
			this.historyNodes.Clear();
			this.nodeConnectors = null;
			this.centerNode = null;
			this.returnNode = null;
			this.expandNode = null;
			this.shortcutCenterNode = null;
			this.targetFrame = null;
			this.selectedNode = null;
			this.timeSelected = 0f;
			this.background = null;
			this.commandFrame = null;
			this.extraOptionCharacters.Clear();
			this.isOpeningClick = (this.isSelectionHighlighted = false);
			this.characterContext = null;
			this.itemContext = null;
			this.isContextual = false;
			this.contextualOrders.Clear();
			this.returnNodeHotkey = (this.expandNodeHotkey = Keys.None);
			if (Character.Controlled != null)
			{
				Character.Controlled.FollowCursor = true;
			}
		}

		// Token: 0x06000E1F RID: 3615 RVA: 0x00082ECC File Offset: 0x000810CC
		private bool NavigateForward(GUIButton node, object userData)
		{
			if (this.commandFrame == null)
			{
				return false;
			}
			CrewManager.OptionNode optionNode = this.optionNodes.Find((CrewManager.OptionNode n) => n.Button == node);
			if (optionNode == null || !this.optionNodes.Remove(optionNode))
			{
				this.shortcutNodes.Remove(node);
			}
			this.RemoveOptionNodes();
			bool wasMinimapVisible = this.targetFrame != null && this.targetFrame.Visible;
			this.HideMinimap();
			if (this.returnNode != null)
			{
				this.returnNode.RemoveChild(this.returnNode.GetChildByUserData("hotkey"));
				this.returnNode.Children.ForEach(delegate(GUIComponent child)
				{
					child.Visible = false;
				});
				this.returnNode.Visible = false;
				this.historyNodes.Push(this.returnNode);
			}
			bool flag;
			if (!wasMinimapVisible)
			{
				GUIButton node2 = node;
				Order order = ((node2 != null) ? node2.UserData : null) as Order;
				flag = (order != null && order.GetMatchingItems(true, this.characterContext ?? Character.Controlled).Count > 1);
			}
			else
			{
				flag = true;
			}
			Point offset = flag ? new Point(0, (int)(0.65f * (float)this.nodeDistance)) : node.RectTransform.AbsoluteOffset.Multiply(-0.65f);
			this.SetReturnNode(this.centerNode, offset);
			this.SetCenterNode(node, false);
			if (this.shortcutCenterNode != null)
			{
				this.commandFrame.RemoveChild(this.shortcutCenterNode);
				this.shortcutCenterNode = null;
			}
			this.CreateNodes(userData);
			this.CreateReturnNodeHotkey();
			return true;
		}

		// Token: 0x06000E20 RID: 3616 RVA: 0x00083084 File Offset: 0x00081284
		private bool NavigateBackward(GUIButton node, object userData)
		{
			if (this.commandFrame == null)
			{
				return false;
			}
			this.RemoveOptionNodes();
			this.HideMinimap();
			this.commandFrame.RemoveChild(this.centerNode);
			this.SetCenterNode(node, false);
			if (this.historyNodes.Count > 0)
			{
				GUIButton historyNode = this.historyNodes.Pop();
				this.SetReturnNode(historyNode, historyNode.RectTransform.AbsoluteOffset);
				historyNode.Visible = true;
				historyNode.RemoveChild(historyNode.GetChildByUserData("hotkey"));
				historyNode.Children.ForEach(delegate(GUIComponent child)
				{
					child.Visible = true;
				});
			}
			else
			{
				this.returnNode = null;
			}
			this.CreateNodes(userData);
			this.CreateReturnNodeHotkey();
			return true;
		}

		// Token: 0x06000E21 RID: 3617 RVA: 0x00083148 File Offset: 0x00081348
		private void HideMinimap()
		{
			if (this.targetFrame == null || !this.targetFrame.Visible)
			{
				return;
			}
			this.targetFrame.Visible = false;
			this.nodeConnectors.RectTransform.Parent = this.commandFrame.RectTransform;
			this.nodeConnectors.RectTransform.RepositionChildInHierarchy(1);
		}

		// Token: 0x06000E22 RID: 3618 RVA: 0x000831A4 File Offset: 0x000813A4
		private void CreateReturnNodeHotkey()
		{
			if (this.returnNode != null && this.returnNode.Visible)
			{
				int hotkey = 1;
				if (this.targetFrame == null || !this.targetFrame.Visible)
				{
					hotkey = this.optionNodes.Count + 1;
					if (this.expandNode != null && this.expandNode.Visible)
					{
						hotkey++;
					}
				}
				this.CreateHotkeyIcon(this.returnNode.RectTransform, hotkey % 10, true);
				this.returnNodeHotkey = Keys.D0 + hotkey % 10;
				return;
			}
			this.returnNodeHotkey = Keys.None;
		}

		// Token: 0x06000E23 RID: 3619 RVA: 0x00083230 File Offset: 0x00081430
		private void SetCenterNode(GUIButton node, bool resetAnchor = false)
		{
			node.RectTransform.Parent = this.commandFrame.RectTransform;
			if (resetAnchor)
			{
				node.RectTransform.SetPosition(Anchor.Center, null);
			}
			node.RectTransform.SetPosition(Anchor.Center, null);
			node.RectTransform.MoveOverTime(Point.Zero, 0.2f, null);
			node.RectTransform.ScaleOverTime(this.centerNodeSize, 0.2f);
			node.RemoveChild(node.GetChildByUserData("hotkey"));
			foreach (GUIComponent c in node.Children)
			{
				c.Color = c.HoverColor * 0.75f;
				c.HoverColor = c.Color;
				c.PressedColor = c.Color;
				c.SelectedColor = c.Color;
				this.SetCharacterTooltip(c, this.characterContext);
			}
			node.OnClicked = null;
			node.OnSecondaryClicked = null;
			node.CanBeFocused = false;
			this.centerNode = node;
		}

		// Token: 0x06000E24 RID: 3620 RVA: 0x0008335C File Offset: 0x0008155C
		private void SetReturnNode(GUIButton node, Point offset)
		{
			node.RectTransform.MoveOverTime(offset, 0.2f, null);
			node.RectTransform.ScaleOverTime(this.returnNodeSize, 0.2f);
			foreach (GUIComponent c in node.Children)
			{
				c.HoverColor = c.Color * 1.3333334f;
				c.PressedColor = c.HoverColor;
				c.SelectedColor = c.HoverColor;
				c.ToolTip = TextManager.Get("commandui.return");
			}
			node.OnClicked = new GUIButton.OnClickedHandler(this.NavigateBackward);
			node.OnSecondaryClicked = null;
			node.CanBeFocused = true;
			this.returnNode = node;
		}

		// Token: 0x06000E25 RID: 3621 RVA: 0x00083434 File Offset: 0x00081634
		private bool CreateNodes(object userData)
		{
			if (userData == null)
			{
				if (this.isContextual)
				{
					this.CreateContextualOrderNodes();
				}
				else
				{
					this.CreateShortcutNodes();
					this.CreateOrderCategoryNodes();
				}
			}
			else if (userData is OrderCategory)
			{
				OrderCategory category = (OrderCategory)userData;
				this.CreateOrderNodes(category);
			}
			else
			{
				Order nodeOrder = userData as Order;
				if (nodeOrder == null)
				{
					if (userData is CrewManager.MinimapNodeData)
					{
						Order minimapOrder = ((CrewManager.MinimapNodeData)userData).Order;
						if (minimapOrder != null && minimapOrder.Prefab.HasOptions)
						{
							this.CreateOrderOptionNodes(minimapOrder, minimapOrder.TargetEntity as Item);
							return true;
						}
					}
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(72, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Unexpected node user data of type ");
					defaultInterpolatedStringHandler.AppendFormatted<Type>(userData.GetType());
					defaultInterpolatedStringHandler.AppendLiteral(" when creating command interface nodes");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
					return false;
				}
				Submarine submarine = this.GetTargetSubmarine();
				List<Item> matchingItems = null;
				if (this.itemContext == null && nodeOrder.MustSetTarget)
				{
					Order order = nodeOrder;
					Submarine submarine2 = submarine;
					bool mustBelongToPlayerSub = true;
					Character interactableFor = this.characterContext ?? Character.Controlled;
					matchingItems = order.GetMatchingItems(submarine2, mustBelongToPlayerSub, null, interactableFor);
				}
				if (this.itemContext == null && !(nodeOrder.TargetEntity is Item) && matchingItems != null && matchingItems.Count > 1)
				{
					this.CreateMinimapNodes(nodeOrder, submarine, matchingItems);
				}
				else
				{
					Order order2 = nodeOrder;
					Item targetItem;
					if ((targetItem = this.itemContext) == null)
					{
						targetItem = ((nodeOrder.TargetEntity as Item) ?? ((matchingItems != null) ? matchingItems.FirstOrDefault<Item>() : null));
					}
					this.CreateOrderOptionNodes(order2, targetItem);
				}
			}
			return true;
		}

		// Token: 0x06000E26 RID: 3622 RVA: 0x000835AC File Offset: 0x000817AC
		private void RemoveOptionNodes()
		{
			if (this.commandFrame != null)
			{
				this.optionNodes.ForEach(delegate(CrewManager.OptionNode node)
				{
					this.commandFrame.RemoveChild(node.Button);
				});
				this.shortcutNodes.ForEach(delegate(GUIComponent node)
				{
					this.commandFrame.RemoveChild(node);
				});
				this.commandFrame.RemoveChild(this.expandNode);
			}
			this.optionNodes.Clear();
			this.shortcutNodes.Clear();
			this.expandNode = null;
			this.expandNodeHotkey = Keys.None;
			this.RemoveExtraOptionNodes();
		}

		// Token: 0x06000E27 RID: 3623 RVA: 0x0008362A File Offset: 0x0008182A
		private void RemoveExtraOptionNodes()
		{
			if (this.commandFrame != null)
			{
				this.extraOptionNodes.ForEach(delegate(GUIComponent node)
				{
					this.commandFrame.RemoveChild(node);
				});
			}
			this.extraOptionNodes.Clear();
		}

		// Token: 0x06000E28 RID: 3624 RVA: 0x00083658 File Offset: 0x00081858
		private void CreateOrderCategoryNodes()
		{
			Vector2[] offsets = MathUtils.GetPointsOnCircumference(Vector2.Zero, (float)this.nodeDistance, this.availableCategories.Count, MathHelper.ToRadians(225f));
			int offsetIndex = 0;
			this.availableCategories.ForEach(delegate(OrderCategory oc)
			{
				CrewManager <>4__this = this;
				Vector2[] offsets = offsets;
				int offsetIndex = offsetIndex;
				offsetIndex++;
				<>4__this.CreateOrderCategoryNode(oc, offsets[offsetIndex].ToPoint(), offsetIndex);
			});
		}

		// Token: 0x06000E29 RID: 3625 RVA: 0x000836BC File Offset: 0x000818BC
		private void CreateOrderCategoryNode(OrderCategory category, Point offset, int hotkey)
		{
			GUIButton node = new GUIButton(new RectTransform(this.nodeSize, this.commandFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), Alignment.Center, null, null)
			{
				UserData = category,
				OnClicked = new GUIButton.OnClickedHandler(this.NavigateForward)
			};
			node.RectTransform.MoveOverTime(offset, 0.2f, null);
			OrderCategoryIcon icon = OrderCategoryIcon.OrderCategoryIcons.FirstOrDefault((OrderCategoryIcon ic) => ic.Category == category);
			if (icon != null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(19, 1);
				defaultInterpolatedStringHandler.AppendLiteral("ordercategorytitle.");
				defaultInterpolatedStringHandler.AppendFormatted<OrderCategory>(category);
				LocalizedString tooltip = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(25, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("ordercategorydescription.");
				defaultInterpolatedStringHandler2.AppendFormatted<OrderCategory>(category);
				LocalizedString categoryDescription = TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear());
				if (!categoryDescription.IsNullOrWhiteSpace())
				{
					tooltip += "\n" + categoryDescription;
				}
				this.CreateNodeIcon(Vector2.One, node.RectTransform, icon.Sprite, icon.Color, tooltip);
			}
			this.CreateHotkeyIcon(node.RectTransform, hotkey % 10, false);
			this.optionNodes.Add(new CrewManager.OptionNode(node, Keys.D0 + hotkey % 10));
		}

		// Token: 0x06000E2A RID: 3626 RVA: 0x0008382C File Offset: 0x00081A2C
		private void CreateShortcutNodes()
		{
			CrewManager.<>c__DisplayClass165_0 CS$<>8__locals1 = new CrewManager.<>c__DisplayClass165_0();
			CS$<>8__locals1.<>4__this = this;
			Submarine sub = this.GetTargetSubmarine();
			if (sub == null)
			{
				return;
			}
			this.shortcutNodes.Clear();
			List<Item> subItems = sub.GetItems(false);
			if (CS$<>8__locals1.<CreateShortcutNodes>g__CanFitMoreNodes|8())
			{
				CrewManager.<>c__DisplayClass165_0 CS$<>8__locals2 = CS$<>8__locals1;
				Item item = subItems.Find((Item i) => i.HasTag(Tags.Reactor) && i.IsPlayerTeamInteractable);
				CS$<>8__locals2.reactor = ((item != null) ? item.GetComponent<Reactor>() : null);
				if (CS$<>8__locals1.reactor != null)
				{
					float reactorOutput = -CS$<>8__locals1.reactor.CurrPowerConsumption;
					if (CrewManager.<CreateShortcutNodes>g__ShouldDelegateOrder|165_9("operatereactor") && reactorOutput < 1E-45f && this.characters.None((Character c) => c.SelectedItem == CS$<>8__locals1.reactor.Item))
					{
						OrderPrefab orderPrefab = OrderPrefab.Prefabs["operatereactor"];
						Order order = new Order(orderPrefab, orderPrefab.Options[0], CS$<>8__locals1.reactor.Item, CS$<>8__locals1.reactor, null, false);
						if (CS$<>8__locals1.<CreateShortcutNodes>g__IsNonDuplicateOrder|11(order))
						{
							CS$<>8__locals1.<CreateShortcutNodes>g__AddOrderNode|14(order);
						}
					}
				}
			}
			if (CS$<>8__locals1.<CreateShortcutNodes>g__CanFitMoreNodes|8() && CrewManager.<CreateShortcutNodes>g__ShouldDelegateOrder|165_9("steer") && CS$<>8__locals1.<CreateShortcutNodes>g__IsNonDuplicateOrderPrefab|12(OrderPrefab.Prefabs["steer"], default(Identifier)))
			{
				CS$<>8__locals1.nav = subItems.Find((Item i) => i.HasTag(Tags.NavTerminal) && i.IsPlayerTeamInteractable);
				if (CS$<>8__locals1.nav != null && this.characters.None((Character c) => c.SelectedItem == CS$<>8__locals1.nav))
				{
					Steering steering = CS$<>8__locals1.nav.GetComponent<Steering>();
					if (steering != null && steering.HasPower)
					{
						Order order2 = new Order(OrderPrefab.Prefabs["steer"], steering.Item, steering, null, false);
						CS$<>8__locals1.<CreateShortcutNodes>g__AddOrderNode|14(order2);
					}
				}
			}
			if (CS$<>8__locals1.<CreateShortcutNodes>g__CanFitMoreNodes|8() && CrewManager.<CreateShortcutNodes>g__ShouldDelegateOrder|165_9("fightintruders"))
			{
				if (this.ActiveOrders.Any(delegate(CrewManager.ActiveOrder o)
				{
					Identifier identifier = o.Order.Identifier;
					return identifier == "reportintruders";
				}) && CS$<>8__locals1.<CreateShortcutNodes>g__IsNonDuplicateOrderPrefab|12(OrderPrefab.Prefabs["fightintruders"], default(Identifier)))
				{
					CS$<>8__locals1.<CreateShortcutNodes>g__AddOrderNodeWithIdentifier|13("fightintruders");
				}
			}
			if (CS$<>8__locals1.<CreateShortcutNodes>g__CanFitMoreNodes|8() && CrewManager.<CreateShortcutNodes>g__ShouldDelegateOrder|165_9("fixleaks") && CS$<>8__locals1.<CreateShortcutNodes>g__IsNonDuplicateOrderPrefab|12(OrderPrefab.Prefabs["fixleaks"], default(Identifier)))
			{
				if (this.ActiveOrders.Any(delegate(CrewManager.ActiveOrder o)
				{
					Identifier identifier = o.Order.Identifier;
					return identifier == "reportbreach";
				}))
				{
					CS$<>8__locals1.<CreateShortcutNodes>g__AddOrderNodeWithIdentifier|13("fixleaks");
				}
			}
			if (CS$<>8__locals1.<CreateShortcutNodes>g__CanFitMoreNodes|8())
			{
				if (this.ActiveOrders.Any(delegate(CrewManager.ActiveOrder o)
				{
					Identifier identifier = o.Order.Identifier;
					return identifier == "reportbrokendevices";
				}))
				{
					OrderPrefab reportBrokenDevices = OrderPrefab.Prefabs["reportbrokendevices"];
					bool useSpecificRepairOrder = false;
					if (CS$<>8__locals1.<CreateShortcutNodes>g__CanFitMoreNodes|8() && CrewManager.<CreateShortcutNodes>g__ShouldDelegateOrder|165_9("repairelectrical") && this.ActiveOrders.Any(delegate(CrewManager.ActiveOrder o)
					{
						if (o.Order.Prefab == reportBrokenDevices)
						{
							Repairable r = o.Order.TargetItemComponent as Repairable;
							if (r != null)
							{
								return r.RequiredSkills.Any((Skill s) => s.Identifier == "electrical");
							}
						}
						return false;
					}))
					{
						if (CS$<>8__locals1.<CreateShortcutNodes>g__IsNonDuplicateOrderPrefab|12(OrderPrefab.Prefabs["repairelectrical"], default(Identifier)))
						{
							CS$<>8__locals1.<CreateShortcutNodes>g__AddOrderNodeWithIdentifier|13("repairelectrical");
						}
						useSpecificRepairOrder = true;
					}
					if (CS$<>8__locals1.<CreateShortcutNodes>g__CanFitMoreNodes|8() && CrewManager.<CreateShortcutNodes>g__ShouldDelegateOrder|165_9("repairmechanical") && this.ActiveOrders.Any(delegate(CrewManager.ActiveOrder o)
					{
						if (o.Order.Prefab == reportBrokenDevices)
						{
							Repairable r = o.Order.TargetItemComponent as Repairable;
							if (r != null)
							{
								return r.RequiredSkills.Any((Skill s) => s.Identifier == "mechanical");
							}
						}
						return false;
					}))
					{
						if (CS$<>8__locals1.<CreateShortcutNodes>g__IsNonDuplicateOrderPrefab|12(OrderPrefab.Prefabs["repairmechanical"], default(Identifier)))
						{
							CS$<>8__locals1.<CreateShortcutNodes>g__AddOrderNodeWithIdentifier|13("repairmechanical");
						}
						useSpecificRepairOrder = true;
					}
					if (!useSpecificRepairOrder && CS$<>8__locals1.<CreateShortcutNodes>g__CanFitMoreNodes|8() && CrewManager.<CreateShortcutNodes>g__ShouldDelegateOrder|165_9("repairsystems"))
					{
						OrderPrefab repairOrder = OrderPrefab.Prefabs["repairsystems"];
						if (repairOrder != null && CS$<>8__locals1.<CreateShortcutNodes>g__IsNonDuplicateOrderPrefab|12(repairOrder, default(Identifier)))
						{
							CS$<>8__locals1.<CreateShortcutNodes>g__AddOrderNodeWithIdentifier|13("repairsystems");
						}
					}
				}
			}
			if (CS$<>8__locals1.<CreateShortcutNodes>g__CanFitMoreNodes|8() && CS$<>8__locals1.<CreateShortcutNodes>g__IsNonDuplicateOrderPrefab|12(OrderPrefab.Prefabs["extinguishfires"], default(Identifier)))
			{
				if (this.ActiveOrders.Any(delegate(CrewManager.ActiveOrder o)
				{
					Identifier identifier = o.Order.Identifier;
					return identifier == "reportfire";
				}))
				{
					CS$<>8__locals1.<CreateShortcutNodes>g__AddOrderNodeWithIdentifier|13("extinguishfires");
				}
			}
			if (CS$<>8__locals1.<CreateShortcutNodes>g__CanFitMoreNodes|8())
			{
				Character character = this.characterContext;
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
						if (job == null)
						{
							flag = (null != null);
						}
						else
						{
							JobPrefab prefab = job.Prefab;
							flag = (((prefab != null) ? prefab.AppropriateOrders : null) != null);
						}
					}
				}
				if (flag)
				{
					using (List<Identifier>.Enumerator enumerator = this.characterContext.Info.Job.Prefab.AppropriateOrders.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Identifier orderIdentifier = enumerator.Current;
							OrderPrefab orderPrefab2 = OrderPrefab.Prefabs[orderIdentifier];
							if (orderPrefab2 != null && CS$<>8__locals1.<CreateShortcutNodes>g__IsNonDuplicateOrderPrefab|12(orderPrefab2, default(Identifier)) && this.shortcutNodes.None(delegate(GUIComponent n)
							{
								Order order5 = n.UserData as Order;
								if (order5 != null)
								{
									Identifier identifier = order5.Identifier;
									return identifier == orderIdentifier;
								}
								return false;
							}) && !orderPrefab2.IsReport && orderPrefab2.Category != null)
							{
								if (!orderPrefab2.MustSetTarget)
								{
									goto IL_583;
								}
								OrderPrefab orderPrefab3 = orderPrefab2;
								Submarine submarine = sub;
								bool mustBelongToPlayerSub = true;
								Character interactableFor = this.characterContext ?? Character.Controlled;
								if (orderPrefab3.GetMatchingItems(submarine, mustBelongToPlayerSub, null, interactableFor, default(Identifier)).Any<Item>())
								{
									goto IL_583;
								}
								IL_597:
								if (!CS$<>8__locals1.<CreateShortcutNodes>g__CanFitMoreNodes|8())
								{
									break;
								}
								continue;
								IL_583:
								Order order3 = orderPrefab2.CreateInstance(OrderPrefab.OrderTargetType.Entity, null, false);
								CS$<>8__locals1.<CreateShortcutNodes>g__AddOrderNode|14(order3);
								goto IL_597;
							}
						}
					}
				}
			}
			if (CS$<>8__locals1.<CreateShortcutNodes>g__CanFitMoreNodes|8() && this.characterContext != null && !this.characterContext.IsDismissed)
			{
				Order order4 = OrderPrefab.Dismissal.CreateInstance(OrderPrefab.OrderTargetType.Entity, null, false);
				CS$<>8__locals1.<CreateShortcutNodes>g__AddOrderNode|14(order4);
			}
			this.shortcutNodes.RemoveAll(delegate(GUIComponent n)
			{
				Order o = n.UserData as Order;
				return o != null && !CS$<>8__locals1.<>4__this.IsOrderAvailable(o);
			});
			if (this.shortcutNodes.Count < 1)
			{
				return;
			}
			this.shortcutCenterNode = new GUIFrame(new RectTransform(this.shortcutCenterNodeSize, this.commandFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), null, null)
			{
				CanBeFocused = false
			};
			this.CreateNodeIcon(this.shortcutCenterNode.RectTransform, "CommandShortcutNode", null, null);
			foreach (GUIComponent c2 in this.shortcutCenterNode.Children)
			{
				c2.HoverColor = c2.Color;
				c2.PressedColor = c2.Color;
				c2.SelectedColor = c2.Color;
			}
			this.shortcutCenterNode.RectTransform.MoveOverTime(this.shorcutCenterNodeOffset, 0.2f, null);
			int nodeCountForCalculations = this.shortcutNodes.Count * 2 + 2;
			Vector2[] offsets = MathUtils.GetPointsOnCircumference(Vector2.Zero, 0.75f * (float)this.nodeDistance, nodeCountForCalculations, 0f);
			int firstOffsetIndex = nodeCountForCalculations / 2 - 1;
			for (int j = 0; j < this.shortcutNodes.Count; j++)
			{
				this.shortcutNodes[j].RectTransform.Parent = this.commandFrame.RectTransform;
				this.shortcutNodes[j].RectTransform.MoveOverTime(this.shorcutCenterNodeOffset + offsets[firstOffsetIndex - j].ToPoint(), 0.2f, null);
			}
		}

		// Token: 0x06000E2B RID: 3627 RVA: 0x00083FF8 File Offset: 0x000821F8
		private unsafe void CreateOrderNodes(OrderCategory orderCategory)
		{
			OrderPrefab[] orderPrefabs = (from o in OrderPrefab.Prefabs.Where(delegate(OrderPrefab o)
			{
				OrderCategory? category = o.Category;
				OrderCategory orderCategory2 = orderCategory;
				return (category.GetValueOrDefault() == orderCategory2 & category != null) && !o.IsReport && this.IsOrderAvailable(o);
			})
			orderby o.Identifier
			select o).ToArray<OrderPrefab>();
			Vector2[] offsets = MathUtils.GetPointsOnCircumference(Vector2.Zero, (float)this.nodeDistance, this.GetCircumferencePointCount(orderPrefabs.Length), this.GetFirstNodeAngle(orderPrefabs.Length));
			for (int i = 0; i < orderPrefabs.Length; i++)
			{
				Order order = orderPrefabs[i].CreateInstance(OrderPrefab.OrderTargetType.Entity, null, false);
				bool disableNode = !this.CanCharacterBeHeard() || (order.MustSetTarget && (order.ItemComponentType != null || order.GetTargetItems(default(Identifier)).Any<Identifier>() || (*order.RequireItems).Any<Identifier>()) && order.GetMatchingItems(true, this.characterContext ?? Character.Controlled).None(null));
				this.optionNodes.Add(new CrewManager.OptionNode(this.CreateOrderNode(this.nodeSize, this.commandFrame.RectTransform, offsets[i].ToPoint(), order, (i + 1) % 10, disableNode, false), (!disableNode) ? (Keys.D0 + (i + 1) % 10) : Keys.None));
			}
		}

		// Token: 0x06000E2C RID: 3628 RVA: 0x0008415C File Offset: 0x0008235C
		private void CreateContextualOrderNodes()
		{
			if (this.contextualOrders.None(null))
			{
				if (this.itemContext != null && this.itemContext.IsPlayerTeamInteractable)
				{
					foreach (OrderPrefab p in OrderPrefab.Prefabs)
					{
						ItemComponent targetComponent = null;
						if (p.UseController)
						{
							if (this.itemContext.Components.None((ItemComponent c) => c is Controller))
							{
								continue;
							}
						}
						if (p.HasOptionSpecificTargetItems)
						{
							foreach (Identifier option in p.Options)
							{
								if (p.TargetItemsMatchItem(this.itemContext, option))
								{
									this.contextualOrders.Add(new Order(p, option, this.itemContext, targetComponent, null, false));
								}
							}
						}
						else if (p.TargetItemsMatchItem(this.itemContext, default(Identifier)) || p.TryGetTargetItemComponent(this.itemContext, out targetComponent))
						{
							this.contextualOrders.Add(p.HasOptions ? p.CreateInstance(OrderPrefab.OrderTargetType.Entity, null, false) : new Order(p, this.itemContext, targetComponent, null, false));
						}
					}
					OrderPrefab operateWeaponsPrefab = OrderPrefab.Prefabs["operateweapons"];
					if (this.contextualOrders.None(delegate(Order o)
					{
						Identifier identifier = o.Identifier;
						return identifier == "operateweapons";
					}))
					{
						if (this.itemContext.Components.Any((ItemComponent c) => c is Controller))
						{
							Turret turret = this.itemContext.GetConnectedComponents<Turret>(false, true, null).FirstOrDefault((Turret c) => operateWeaponsPrefab.TargetItemsMatchItem(c.Item, default(Identifier))) ?? this.itemContext.GetConnectedComponents<Turret>(true, true, null).FirstOrDefault((Turret c) => operateWeaponsPrefab.TargetItemsMatchItem(c.Item, default(Identifier)));
							if (turret != null)
							{
								this.contextualOrders.Add(new Order(operateWeaponsPrefab, turret.Item, turret, null, false));
							}
						}
					}
					if (this.contextualOrders.None(delegate(Order order)
					{
						Identifier identifier = order.Identifier;
						return identifier == "repairsystems";
					}))
					{
						if (this.itemContext.Repairables.Any((Repairable r) => r.IsBelowRepairThreshold))
						{
							if (this.itemContext.Repairables.Any(delegate(Repairable r)
							{
								if (r != null)
								{
									return r.RequiredSkills.Any((Skill s) => s != null && s.Identifier.Equals("electrical"));
								}
								return false;
							}))
							{
								this.contextualOrders.Add(new Order(OrderPrefab.Prefabs["repairelectrical"], this.itemContext, null, null, false));
							}
							else if (this.itemContext.Repairables.Any(delegate(Repairable r)
							{
								if (r != null)
								{
									return r.RequiredSkills.Any((Skill s) => s != null && s.Identifier.Equals("mechanical"));
								}
								return false;
							}))
							{
								this.contextualOrders.Add(new Order(OrderPrefab.Prefabs["repairmechanical"], this.itemContext, null, null, false));
							}
							else
							{
								this.contextualOrders.Add(new Order(OrderPrefab.Prefabs["repairsystems"], this.itemContext, null, null, false));
							}
						}
					}
					Order pumpOrder = this.contextualOrders.FirstOrDefault((Order order) => order.Identifier.Equals("pumpwater"));
					if (pumpOrder != null)
					{
						Pump pump = this.itemContext.Components.FirstOrDefault((ItemComponent c) => c.GetType() == pumpOrder.ItemComponentType) as Pump;
						if (pump != null && pump.IsAutoControlled)
						{
							this.contextualOrders.Remove(pumpOrder);
						}
					}
					if (this.contextualOrders.None((Order info) => info.Identifier.Equals("cleanupitems")) && (AIObjectiveCleanupItems.IsValidTarget(this.itemContext, Character.Controlled, false, true, true, true) || AIObjectiveCleanupItems.IsValidContainer(this.itemContext, Character.Controlled)))
					{
						this.contextualOrders.Add(new Order(OrderPrefab.Prefabs["cleanupitems"], this.itemContext, null, null, false));
					}
					this.<CreateContextualOrderNodes>g__AddIgnoreOrder|167_0(this.itemContext);
				}
				else if (this.hullContext != null)
				{
					this.contextualOrders.Add(new Order(OrderPrefab.Prefabs["fixleaks"], this.hullContext, null, null, false));
					if (this.wallContext != null)
					{
						this.<CreateContextualOrderNodes>g__AddIgnoreOrder|167_0(this.wallContext);
					}
				}
				if (this.contextualOrders.None((Order order) => order.Identifier.Equals("wait")))
				{
					Vector2 position = GameMain.GameScreen.Cam.ScreenToWorld(PlayerInput.MousePosition);
					Vector2 position2 = position;
					Character controlled = Character.Controlled;
					Hull hull = Hull.FindHull(position2, (controlled != null) ? controlled.CurrentHull : null, true, true);
					this.contextualOrders.Add(new Order(OrderPrefab.Prefabs["wait"], new OrderTarget(position, hull, false), null));
				}
				if (this.contextualOrders.None((Order order) => order.Category.GetValueOrDefault() != OrderCategory.Movement))
				{
					if (this.characters.Any((Character c) => c != Character.Controlled))
					{
						if (this.contextualOrders.None((Order order) => order.Identifier.Equals("follow")))
						{
							this.contextualOrders.Add(OrderPrefab.Prefabs["follow"].CreateInstance(OrderPrefab.OrderTargetType.Entity, null, false));
						}
					}
				}
				if (this.contextualOrders.None((Order order) => order.IsDismissal))
				{
					if (this.characters.Any((Character c) => !c.IsDismissed))
					{
						this.contextualOrders.Add(OrderPrefab.Dismissal.CreateInstance(OrderPrefab.OrderTargetType.Entity, null, false));
					}
				}
			}
			this.contextualOrders.RemoveAll((Order o) => !this.IsOrderAvailable(o));
			Vector2[] offsets = MathUtils.GetPointsOnCircumference(Vector2.Zero, (float)this.nodeDistance, this.contextualOrders.Count, MathHelper.ToRadians(90f + 180f / (float)this.contextualOrders.Count));
			bool canCharacterBeHeard = this.CanCharacterBeHeard();
			for (int i = 0; i < this.contextualOrders.Count; i++)
			{
				Order order2 = this.contextualOrders[i];
				bool disableNode = !canCharacterBeHeard && !order2.TargetAllCharacters;
				int hotkey = (i + 1) % 10;
				GUIButton component = order2.Option.IsEmpty ? this.CreateOrderNode(this.nodeSize, this.commandFrame.RectTransform, offsets[i].ToPoint(), order2, hotkey, disableNode, false) : this.CreateOrderOptionNode(this.nodeSize, this.commandFrame.RectTransform, offsets[i].ToPoint(), order2, hotkey);
				this.optionNodes.Add(new CrewManager.OptionNode(component, (!disableNode) ? (Keys.D0 + (i + 1) % 10) : Keys.None));
			}
		}

		// Token: 0x06000E2D RID: 3629 RVA: 0x00084914 File Offset: 0x00082B14
		private GUIButton CreateOrderNode(Point size, RectTransform parent, Point offset, Order order, int hotkey, bool disableNode = false, bool checkIfOrderCanBeHeard = true)
		{
			GUIButton node = new GUIButton(new RectTransform(size, parent, Anchor.Center, null, ScaleBasis.Normal, false), Alignment.Center, null, null)
			{
				UserData = order
			};
			node.RectTransform.MoveOverTime(offset, 0.2f, null);
			if (checkIfOrderCanBeHeard && !disableNode)
			{
				disableNode = !this.CanCharacterBeHeard();
			}
			bool mustSetOptionOrTarget = order.Prefab.HasOptions;
			Item orderTargetEntity = null;
			if (!mustSetOptionOrTarget && order.MustSetTarget && this.itemContext == null)
			{
				Order order2 = order;
				Submarine targetSubmarine = this.GetTargetSubmarine();
				bool mustBelongToPlayerSub = true;
				Character interactableFor = this.characterContext ?? Character.Controlled;
				List<Item> matchingItems = order2.GetMatchingItems(targetSubmarine, mustBelongToPlayerSub, null, interactableFor);
				if (matchingItems.Count > 1)
				{
					mustSetOptionOrTarget = true;
				}
				else
				{
					orderTargetEntity = matchingItems.FirstOrDefault<Item>();
				}
			}
			Func<ItemComponent, bool> <>9__2;
			node.OnClicked = delegate(GUIButton button, object userData)
			{
				if (disableNode || !CrewManager.CanIssueOrders)
				{
					return false;
				}
				Order o = userData as Order;
				if (mustSetOptionOrTarget)
				{
					this.NavigateForward(button, userData);
				}
				else if (o.MustManuallyAssign && this.characterContext == null)
				{
					this.CreateAssignmentNodes(node);
				}
				else
				{
					Entity orderTargetEntity;
					if (orderTargetEntity != null)
					{
						OrderPrefab prefab = o.Prefab;
						orderTargetEntity = orderTargetEntity;
						IEnumerable<ItemComponent> components = orderTargetEntity.Components;
						Func<ItemComponent, bool> predicate;
						if ((predicate = <>9__2) == null)
						{
							predicate = (<>9__2 = ((ItemComponent ic) => ic.GetType() == order.ItemComponentType));
						}
						o = new Order(prefab, orderTargetEntity, components.FirstOrDefault(predicate), order.OrderGiver, false);
					}
					Character character = (!o.TargetAllCharacters) ? (this.characterContext ?? this.GetCharacterForQuickAssignment(o)) : null;
					int priority = this.GetManualOrderPriority(character, o);
					this.SetCharacterOrder(character, o.WithManualPriority(priority).WithOrderGiver(Character.Controlled), true);
					this.DisableCommandUI();
				}
				return true;
			};
			if (this.CanOpenManualAssignment(node))
			{
				node.OnSecondaryClicked = ((GUIComponent button, object _) => this.CreateAssignmentNodes(button));
			}
			bool showAssignmentTooltip = !mustSetOptionOrTarget && this.characterContext == null && !order.MustManuallyAssign && !order.TargetAllCharacters;
			LocalizedString orderName = this.GetOrderNameBasedOnContextuality(order);
			GUIImage icon = this.CreateNodeIcon(Vector2.One, node.RectTransform, order.SymbolSprite, order.Color, (!showAssignmentTooltip) ? orderName : (orderName + "\n" + PlayerInput.PrimaryMouseLabel + ": " + TextManager.Get("commandui.quickassigntooltip") + "\n" + PlayerInput.SecondaryMouseLabel + ": " + TextManager.Get("commandui.manualassigntooltip")));
			if (disableNode)
			{
				node.CanBeFocused = (icon.CanBeFocused = false);
				this.CreateBlockIcon(node.RectTransform, TextManager.Get((this.characterContext == null) ? "nocharactercanhear" : "thischaractercanthear"));
			}
			else if (hotkey >= 0)
			{
				this.CreateHotkeyIcon(node.RectTransform, hotkey, false);
			}
			return node;
		}

		// Token: 0x06000E2E RID: 3630 RVA: 0x00084BB8 File Offset: 0x00082DB8
		private void CreateMinimapNodes(Order order, Submarine submarine, List<Item> matchingItems)
		{
			Rectangle subBorders = submarine.GetDockedBorders(true);
			Point frameSize;
			if (subBorders.Width > subBorders.Height)
			{
				frameSize.X = Math.Min(GameMain.GraphicsWidth / 2, GameMain.GraphicsWidth - 50) / 2;
				frameSize.Y = (int)((float)frameSize.X * ((float)subBorders.Height / (float)subBorders.Width));
			}
			else
			{
				frameSize.Y = Math.Min((int)((float)GameMain.GraphicsHeight * 0.6f), GameMain.GraphicsHeight - 50) / 2;
				frameSize.X = (int)((float)frameSize.Y * ((float)subBorders.Width / (float)subBorders.Height));
			}
			this.targetFrame = new GUIFrame(new RectTransform(frameSize, this.commandFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false)
			{
				AbsoluteOffset = new Point(0, -150),
				Pivot = Pivot.BottomCenter
			}, "InnerFrame", null);
			submarine.CreateMiniMap(this.targetFrame, matchingItems, false);
			GUICustomComponent guicustomComponent = new GUICustomComponent(new RectTransform(Vector2.One, this.targetFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), new Action<SpriteBatch, GUICustomComponent>(this.DrawMiniMapOverlay), null);
			guicustomComponent.CanBeFocused = false;
			guicustomComponent.UserData = submarine;
			List<GUIComponent> optionElements = new List<GUIComponent>();
			foreach (Item item in matchingItems)
			{
				GUIComponent itemTargetFrame = this.targetFrame.Children.First<GUIComponent>().FindChild(item, false);
				if (itemTargetFrame != null)
				{
					Anchor anchor = Anchor.TopLeft;
					if (itemTargetFrame.RectTransform.RelativeOffset.X < 0.5f)
					{
						if (itemTargetFrame.RectTransform.RelativeOffset.Y < 0.5f)
						{
							anchor = Anchor.BottomRight;
						}
						else
						{
							anchor = Anchor.TopRight;
						}
					}
					else if (itemTargetFrame.RectTransform.RelativeOffset.Y < 0.5f)
					{
						anchor = Anchor.BottomLeft;
					}
					CrewManager.MinimapNodeData userData = new CrewManager.MinimapNodeData
					{
						Order = ((item == null) ? order : order.WithItemComponent(item, order.GetTargetItemComponent(item)))
					};
					GUIButton optionElement = new GUIButton(new RectTransform(new Point((int)(50f * GUI.Scale)), itemTargetFrame.RectTransform, anchor, null, ScaleBasis.Normal, false), Alignment.Center, null, null)
					{
						UserData = userData,
						Font = GUIStyle.SmallFont,
						OnClicked = delegate(GUIButton button, object obj)
						{
							if (!CrewManager.CanIssueOrders)
							{
								return false;
							}
							CrewManager.MinimapNodeData o = (CrewManager.MinimapNodeData)obj;
							if (o.Order.Prefab.HasOptions)
							{
								this.NavigateForward(button, o);
							}
							else if (o.Order.MustManuallyAssign && this.characterContext == null)
							{
								this.CreateAssignmentNodes(button);
							}
							else
							{
								Character character = this.characterContext ?? this.GetCharacterForQuickAssignment(o.Order);
								int priority = this.GetManualOrderPriority(character, o.Order);
								this.SetCharacterOrder(character, o.Order.WithManualPriority(priority).WithOrderGiver(Character.Controlled), true);
								this.DisableCommandUI();
							}
							return true;
						}
					};
					if (this.CanOpenManualAssignmentMinimapOrder(optionElement))
					{
						optionElement.OnSecondaryClicked = ((GUIComponent button, object _) => this.CreateAssignmentNodes(button));
					}
					Func<Order, bool> <>9__3;
					float colorMultiplier = this.characters.Any(delegate(Character c)
					{
						if (c.CurrentOrders != null)
						{
							IEnumerable<Order> currentOrders = c.CurrentOrders;
							Func<Order, bool> predicate;
							if ((predicate = <>9__3) == null)
							{
								predicate = (<>9__3 = delegate(Order o)
								{
									if (o != null)
									{
										Identifier identifier = o.Identifier;
										Identifier identifier2 = userData.Order.Identifier;
										if (identifier == identifier2)
										{
											return o.TargetEntity == userData.Order.TargetEntity;
										}
									}
									return false;
								});
							}
							return currentOrders.Any(predicate);
						}
						return false;
					}) ? 0.5f : 1f;
					this.CreateNodeIcon(Vector2.One, optionElement.RectTransform, item.Prefab.MinimapIcon ?? order.SymbolSprite, order.Color * colorMultiplier, item.Name);
					this.optionNodes.Add(new CrewManager.OptionNode(optionElement, Keys.None));
					optionElements.Add(optionElement);
				}
			}
			Rectangle clampArea = new Rectangle(10, 10, GameMain.GraphicsWidth - 20, GameMain.GraphicsHeight - 20);
			Rectangle disallowedArea = this.targetFrame.GetChild<GUIFrame>().Rect;
			Point originalSize = disallowedArea.Size;
			disallowedArea.Size = disallowedArea.MultiplySize(0.9f);
			disallowedArea.X += (originalSize.X - disallowedArea.Size.X) / 2;
			disallowedArea.Y += (originalSize.Y - disallowedArea.Size.Y) / 2;
			GUI.PreventElementOverlap(optionElements, new List<Rectangle>
			{
				disallowedArea
			}, new Rectangle?(clampArea));
			this.nodeConnectors.RectTransform.Parent = this.targetFrame.RectTransform;
			this.nodeConnectors.RectTransform.SetAsFirstChild();
			GUIFrame shadow = new GUIFrame(new RectTransform(this.targetFrame.Rect.Size + new Point((int)(200f * GUI.Scale)), this.targetFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), "OuterGlow", new Color?((matchingItems.Count > 1) ? (Color.Black * 0.9f) : (Color.Black * 0.7f)));
			shadow.SetAsFirstChild();
		}

		// Token: 0x06000E2F RID: 3631 RVA: 0x00085078 File Offset: 0x00083278
		private void CreateOrderOptionNodes(Order order, Item targetItem)
		{
			if (this.itemContext != null)
			{
				Item item;
				if (order.UseController)
				{
					Turret turret = this.itemContext.GetConnectedComponents<Turret>(false, true, null).FirstOrDefault<Turret>();
					if ((item = ((turret != null) ? turret.Item : null)) == null)
					{
						Turret turret2 = this.itemContext.GetConnectedComponents<Turret>(true, true, null).FirstOrDefault<Turret>();
						item = ((turret2 != null) ? turret2.Item : null);
					}
				}
				else
				{
					item = this.itemContext;
				}
				targetItem = item;
			}
			Order o = (targetItem == null) ? order : order.WithItemComponent(targetItem, order.GetTargetItemComponent(targetItem));
			Vector2[] offsets = MathUtils.GetPointsOnCircumference(Vector2.Zero, (float)this.nodeDistance, this.GetCircumferencePointCount(order.Options.Length), this.GetFirstNodeAngle(order.Options.Length));
			int offsetIndex = 0;
			for (int i = 0; i < order.Options.Length; i++)
			{
				this.optionNodes.Add(new CrewManager.OptionNode(this.CreateOrderOptionNode(this.nodeSize, this.commandFrame.RectTransform, offsets[offsetIndex++].ToPoint(), o.WithOption(order.Options[i]), (i + 1) % 10), Keys.D0 + (i + 1) % 10));
			}
		}

		// Token: 0x06000E30 RID: 3632 RVA: 0x0008519C File Offset: 0x0008339C
		private GUIButton CreateOrderOptionNode(Point size, RectTransform parent, Point offset, Order order, int hotkey)
		{
			GUIButton node = new GUIButton(new RectTransform(size, parent, Anchor.Center, null, ScaleBasis.Normal, false), Alignment.Center, null, null)
			{
				UserData = order,
				OnClicked = delegate(GUIButton button, object userData)
				{
					if (!CrewManager.CanIssueOrders)
					{
						return false;
					}
					Order o = userData as Order;
					if (o.MustManuallyAssign && this.characterContext == null)
					{
						this.CreateAssignmentNodes(button);
					}
					else
					{
						Character character = this.characterContext ?? this.GetCharacterForQuickAssignment(o);
						int priority = this.GetManualOrderPriority(character, o);
						this.SetCharacterOrder(character, o.WithManualPriority(priority).WithOrderGiver(Character.Controlled), true);
						this.DisableCommandUI();
					}
					return true;
				}
			};
			if (this.CanOpenManualAssignment(node))
			{
				node.OnSecondaryClicked = ((GUIComponent button, object _) => this.CreateAssignmentNodes(button));
			}
			node.RectTransform.MoveOverTime(offset, 0.2f, null);
			GUIImage icon = null;
			Sprite sprite;
			if (order.Prefab.OptionSprites.TryGetValue(order.Option, out sprite))
			{
				LocalizedString optionName = order.Prefab.GetOptionName(order.Option);
				bool flag = this.characterContext == null && !order.MustManuallyAssign && !order.TargetAllCharacters;
				icon = this.CreateNodeIcon(Vector2.One, node.RectTransform, sprite, order.Color, (this.characterContext != null) ? optionName : (optionName + "\n" + PlayerInput.PrimaryMouseLabel + ": " + TextManager.Get("commandui.quickassigntooltip") + "\n" + PlayerInput.SecondaryMouseLabel + ": " + TextManager.Get("commandui.manualassigntooltip")));
			}
			if (!this.CanCharacterBeHeard())
			{
				node.CanBeFocused = false;
				if (icon != null)
				{
					icon.CanBeFocused = false;
				}
				this.CreateBlockIcon(node.RectTransform, TextManager.Get((this.characterContext == null) ? "nocharactercanhear" : "thischaractercanthear"));
			}
			else if (hotkey >= 0)
			{
				this.CreateHotkeyIcon(node.RectTransform, hotkey, false);
			}
			return node;
		}

		// Token: 0x06000E31 RID: 3633 RVA: 0x00085358 File Offset: 0x00083558
		private bool CreateAssignmentNodes(GUIComponent node)
		{
			if (this.centerNode == null)
			{
				this.DisableCommandUI();
				return false;
			}
			object userData = node.UserData;
			Order order2;
			if (userData is CrewManager.MinimapNodeData)
			{
				CrewManager.MinimapNodeData minimapNodeData = (CrewManager.MinimapNodeData)userData;
				order2 = minimapNodeData.Order;
			}
			else
			{
				order2 = (node.UserData as Order);
			}
			Order order = order2;
			List<Character> characters = this.GetCharactersForManualAssignment(order);
			if (characters.None(null))
			{
				return false;
			}
			CrewManager.OptionNode optionNode = this.optionNodes.Find((CrewManager.OptionNode n) => n.Button == node);
			if (optionNode == null || !this.optionNodes.Remove(optionNode))
			{
				this.shortcutNodes.Remove(node);
			}
			this.RemoveOptionNodes();
			if (this.returnNode != null)
			{
				this.returnNode.Children.ForEach(delegate(GUIComponent child)
				{
					child.Visible = false;
				});
				this.returnNode.Visible = false;
				this.historyNodes.Push(this.returnNode);
			}
			this.SetReturnNode(this.centerNode, new Point(0, (int)(0.65f * (float)this.nodeDistance)));
			if (this.targetFrame == null || !this.targetFrame.Visible)
			{
				this.SetCenterNode(node as GUIButton, false);
			}
			else
			{
				if (order.Option.IsEmpty)
				{
					this.SetCenterNode(node as GUIButton, true);
				}
				else
				{
					GUIButton clickedOptionNode = new GUIButton(new RectTransform(this.centerNodeSize, this.commandFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), Alignment.Center, null, null)
					{
						UserData = node.UserData
					};
					Sprite sprite;
					if (order.Prefab.OptionSprites.TryGetValue(order.Option, out sprite))
					{
						this.CreateNodeIcon(Vector2.One, clickedOptionNode.RectTransform, sprite, order.Color, order.GetOptionName(order.Option));
					}
					this.SetCenterNode(clickedOptionNode, false);
					node = null;
				}
				this.HideMinimap();
			}
			if (this.shortcutCenterNode != null)
			{
				this.commandFrame.RemoveChild(this.shortcutCenterNode);
				this.shortcutCenterNode = null;
			}
			int characterCount = characters.Count;
			int hotkey = 1;
			bool needToExpand = characterCount > 10;
			Vector2[] offsets;
			if (characterCount > 5)
			{
				int charactersOnFirstRing = needToExpand ? 5 : ((int)Math.Floor((double)((float)characterCount / 2f)));
				offsets = this.GetAssignmentNodeOffsets(charactersOnFirstRing, true);
				for (int i = 0; i < charactersOnFirstRing; i++)
				{
					this.CreateAssignmentNode(order, characters[i], offsets[i].ToPoint(), hotkey++ % 10, 1f);
				}
				int charactersOnSecondRing = needToExpand ? 4 : (characterCount - charactersOnFirstRing);
				offsets = this.GetAssignmentNodeOffsets(needToExpand ? 5 : charactersOnSecondRing, false);
				for (int j = 0; j < charactersOnSecondRing; j++)
				{
					this.CreateAssignmentNode(order, characters[charactersOnFirstRing + j], offsets[j].ToPoint(), hotkey++ % 10, 1f);
				}
			}
			else
			{
				offsets = this.GetAssignmentNodeOffsets(characterCount, true);
				for (int k = 0; k < characterCount; k++)
				{
					this.CreateAssignmentNode(order, characters[k], offsets[k].ToPoint(), hotkey++ % 10, 1f);
				}
			}
			if (!needToExpand)
			{
				hotkey = this.optionNodes.Count + 1;
				this.CreateHotkeyIcon(this.returnNode.RectTransform, hotkey % 10, true);
				this.returnNodeHotkey = Keys.D0 + hotkey % 10;
				this.expandNodeHotkey = Keys.None;
				return true;
			}
			this.extraOptionCharacters.Clear();
			this.extraOptionCharacters.AddRange(characters.GetRange(hotkey - 1, characterCount - (hotkey - 1)).OrderBy(delegate(Character c)
			{
				if (c == null)
				{
					return null;
				}
				CharacterInfo info = c.Info;
				if (info == null)
				{
					return null;
				}
				Job job = info.Job;
				if (job == null)
				{
					return null;
				}
				return job.Name;
			}).ThenBy(delegate(Character c)
			{
				if (c == null)
				{
					return null;
				}
				CharacterInfo info = c.Info;
				if (info == null)
				{
					return null;
				}
				return info.DisplayName;
			}));
			this.expandNode = new GUIButton(new RectTransform(this.assignmentNodeSize, this.commandFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false)
			{
				AbsoluteOffset = offsets.Last<Vector2>().ToPoint()
			}, Alignment.Center, null, null)
			{
				UserData = order,
				OnClicked = new GUIButton.OnClickedHandler(this.ExpandAssignmentNodes)
			};
			this.CreateNodeIcon(this.expandNode.RectTransform, "CommandExpandNode", new Color?(order.Color), TextManager.Get("commandui.expand"));
			hotkey = this.optionNodes.Count + 1;
			this.CreateHotkeyIcon(this.expandNode.RectTransform, hotkey % 10, false);
			this.expandNodeHotkey = Keys.D0 + hotkey % 10;
			this.CreateHotkeyIcon(this.returnNode.RectTransform, ++hotkey % 10, true);
			this.returnNodeHotkey = Keys.D0 + hotkey % 10;
			return true;
		}

		// Token: 0x06000E32 RID: 3634 RVA: 0x00085860 File Offset: 0x00083A60
		private Vector2[] GetAssignmentNodeOffsets(int characters, bool firstRing = true)
		{
			float nodeDistance = 1.8f * (float)this.nodeDistance;
			int nodePositionsOnEachSide = (characters % 2 > 0) ? 7 : 6;
			int nodeCountForCalculation = 2 * nodePositionsOnEachSide + 2;
			Vector2[] offsets = MathUtils.GetPointsOnCircumference(firstRing ? new Vector2(0f, 0.5f * nodeDistance) : Vector2.Zero, nodeDistance, nodeCountForCalculation, MathHelper.ToRadians(180f + 360f / (float)nodeCountForCalculation));
			int emptySpacesPerSide = (nodePositionsOnEachSide - characters) / 2;
			Vector2[] offsetsInUse = new Vector2[nodePositionsOnEachSide - 2 * emptySpacesPerSide];
			for (int i = 0; i < offsetsInUse.Length; i++)
			{
				offsetsInUse[i] = offsets[i + emptySpacesPerSide];
			}
			return offsetsInUse;
		}

		// Token: 0x06000E33 RID: 3635 RVA: 0x00085900 File Offset: 0x00083B00
		private bool ExpandAssignmentNodes(GUIButton node, object userData)
		{
			node.OnClicked = delegate(GUIButton button, object _)
			{
				this.RemoveExtraOptionNodes();
				button.OnClicked = new GUIButton.OnClickedHandler(this.ExpandAssignmentNodes);
				return true;
			};
			int availableNodePositions = 20;
			Vector2[] offsets = MathUtils.GetPointsOnCircumference(Vector2.Zero, 2.7f * (float)this.nodeDistance, availableNodePositions, MathHelper.ToRadians(-90f - (float)(this.extraOptionCharacters.Count - 1) * 0.5f * (360f / (float)availableNodePositions)));
			int i = 0;
			while (i < this.extraOptionCharacters.Count && i < availableNodePositions)
			{
				this.CreateAssignmentNode(userData as Order, this.extraOptionCharacters[i], offsets[i].ToPoint(), -1, 1.15f);
				i++;
			}
			return true;
		}

		// Token: 0x06000E34 RID: 3636 RVA: 0x000859A8 File Offset: 0x00083BA8
		private void CreateAssignmentNode(Order order, Character character, Point offset, int hotkey, float nameLabelScale = 1f)
		{
			GUIButton node = new GUIButton(new RectTransform(this.assignmentNodeSize, this.commandFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), Alignment.Center, null, null)
			{
				UserData = character,
				OnClicked = delegate(GUIButton _, object userData)
				{
					if (!CrewManager.CanIssueOrders)
					{
						return false;
					}
					Character character2 = userData as Character;
					int priority = this.GetManualOrderPriority(character2, order);
					Item targetEntity = null;
					if (order.MustSetTarget && order.TargetEntity == null)
					{
						Order order2 = order;
						Submarine targetSubmarine = this.GetTargetSubmarine();
						bool mustBelongToPlayerSub = true;
						Character interactableFor = this.characterContext ?? Character.Controlled;
						List<Item> matchingItems = order2.GetMatchingItems(targetSubmarine, mustBelongToPlayerSub, null, interactableFor);
						targetEntity = matchingItems.FirstOrDefault<Item>();
					}
					this.SetCharacterOrder(character2, order.WithItemComponent(targetEntity, null).WithManualPriority(priority).WithOrderGiver(Character.Controlled), true);
					this.DisableCommandUI();
					return true;
				}
			};
			node.RectTransform.MoveOverTime(offset, 0.2f, null);
			CharacterInfo info = character.Info;
			Color? color;
			if (info == null)
			{
				color = null;
			}
			else
			{
				Job job = info.Job;
				if (job == null)
				{
					color = null;
				}
				else
				{
					JobPrefab prefab = job.Prefab;
					color = ((prefab != null) ? new Color?(prefab.UIColor) : null);
				}
			}
			Color jobColor = color ?? Color.White;
			Order topOrderInfo = character.GetCurrentOrderWithTopPriority();
			GUIImage orderIcon;
			if (topOrderInfo != null)
			{
				orderIcon = new GUIImage(new RectTransform(new Vector2(1.2f), node.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), topOrderInfo.SymbolSprite, true, null);
				LocalizedString tooltip = topOrderInfo.Name;
				if (topOrderInfo.Option != Identifier.Empty)
				{
					tooltip += " (" + topOrderInfo.GetOptionName(topOrderInfo.Option) + ")";
				}
				orderIcon.ToolTip = tooltip;
			}
			else
			{
				orderIcon = new GUIImage(new RectTransform(new Vector2(1.2f), node.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "CommandIdleNode", true);
			}
			orderIcon.Color = jobColor * 0.75f;
			orderIcon.HoverColor = jobColor;
			orderIcon.PressedColor = jobColor;
			orderIcon.SelectedColor = jobColor;
			orderIcon.UserData = "colorsource";
			int width = (int)(nameLabelScale * (float)this.nodeSize.X);
			GUIFont font = GUIStyle.SmallFont;
			RectTransform rectTransform = new RectTransform(new Point(width, 0), node.RectTransform, Anchor.TopCenter, new Pivot?(Pivot.BottomCenter), ScaleBasis.Normal, false);
			rectTransform.RelativeOffset = new Vector2(0f, -0.25f);
			CharacterInfo info2 = character.Info;
			GUITextBlock guitextBlock = new GUITextBlock(rectTransform, ToolBox.LimitString((info2 != null) ? info2.DisplayName : null, font, width), new Color?(jobColor * 0.75f), font, Alignment.Center, false, null, null);
			guitextBlock.CanBeFocused = false;
			guitextBlock.ForceUpperCase = ForceUpperCase.Yes;
			guitextBlock.HoverTextColor = jobColor;
			CharacterInfo info3 = character.Info;
			Sprite sprite;
			if (info3 == null)
			{
				sprite = null;
			}
			else
			{
				Job job2 = info3.Job;
				if (job2 == null)
				{
					sprite = null;
				}
				else
				{
					JobPrefab prefab2 = job2.Prefab;
					sprite = ((prefab2 != null) ? prefab2.IconSmall : null);
				}
			}
			Sprite smallJobIcon = sprite;
			if (smallJobIcon != null)
			{
				GUIImage guiimage = new GUIImage(new RectTransform(new Vector2(0.4f), node.RectTransform, Anchor.TopCenter, new Pivot?(Pivot.Center), null, null, ScaleBasis.Normal)
				{
					RelativeOffset = new Vector2(0f, -((orderIcon.RectTransform.RelativeSize.Y - 1f) / 2f))
				}, smallJobIcon, true, null);
				guiimage.CanBeFocused = false;
				guiimage.Color = jobColor;
				guiimage.HoverColor = jobColor;
			}
			bool canHear = character.CanHearCharacter(Character.Controlled);
			if (!canHear)
			{
				node.CanBeFocused = (orderIcon.CanBeFocused = false);
				this.CreateBlockIcon(node.RectTransform, TextManager.Get("thischaractercanthear"));
			}
			if (hotkey >= 0)
			{
				if (canHear)
				{
					this.CreateHotkeyIcon(node.RectTransform, hotkey, false);
				}
				this.optionNodes.Add(new CrewManager.OptionNode(node, canHear ? (Keys.D0 + hotkey) : Keys.None));
				return;
			}
			this.extraOptionNodes.Add(node);
		}

		// Token: 0x06000E35 RID: 3637 RVA: 0x00085D98 File Offset: 0x00083F98
		private GUIImage CreateNodeIcon(Vector2 relativeSize, RectTransform parent, Sprite sprite, Color color, LocalizedString tooltip = null)
		{
			return new GUIImage(new RectTransform(relativeSize, parent, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), sprite, true, null)
			{
				Color = color * 0.75f,
				HoverColor = color,
				PressedColor = color,
				SelectedColor = color,
				ToolTip = tooltip,
				UserData = "colorsource"
			};
		}

		// Token: 0x06000E36 RID: 3638 RVA: 0x00085E1C File Offset: 0x0008401C
		private GUIImage CreateNodeIcon(Point absoluteSize, RectTransform parent, Sprite sprite, Color color, LocalizedString tooltip = null)
		{
			return new GUIImage(new RectTransform(absoluteSize, parent, Anchor.TopLeft, null, ScaleBasis.Normal, false)
			{
				IsFixedSize = true
			}, sprite, true, null)
			{
				Color = color * 0.75f,
				HoverColor = color,
				PressedColor = color,
				SelectedColor = color,
				ToolTip = tooltip,
				UserData = "colorsource"
			};
		}

		// Token: 0x06000E37 RID: 3639 RVA: 0x00085E98 File Offset: 0x00084098
		private void CreateNodeIcon(RectTransform parent, string style, Color? color = null, LocalizedString tooltip = null)
		{
			GUIImage icon = new GUIImage(new RectTransform(Vector2.One, parent, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), style, true)
			{
				ToolTip = tooltip,
				UserData = "colorsource"
			};
			if (color != null)
			{
				icon.Color = color.Value * 0.75f;
				icon.HoverColor = color.Value;
				return;
			}
			icon.Color = icon.HoverColor * 0.75f;
		}

		// Token: 0x06000E38 RID: 3640 RVA: 0x00085F34 File Offset: 0x00084134
		private void CreateHotkeyIcon(RectTransform parent, int hotkey, bool enlargeIcon = false)
		{
			GUIImage bg = new GUIImage(new RectTransform(new Vector2(enlargeIcon ? 0.4f : 0.25f), parent, Anchor.BottomCenter, new Pivot?(Pivot.Center), null, null, ScaleBasis.Normal), "CommandHotkeyContainer", true)
			{
				CanBeFocused = false,
				UserData = "hotkey"
			};
			new GUITextBlock(new RectTransform(Vector2.One, bg.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), hotkey.ToString(), new Color?(Color.Black), null, Alignment.Center, false, "", null).CanBeFocused = false;
		}

		// Token: 0x06000E39 RID: 3641 RVA: 0x00085FF4 File Offset: 0x000841F4
		private void CreateBlockIcon(RectTransform parent, LocalizedString tooltip = null)
		{
			GUIImage icon = new GUIImage(new RectTransform(new Vector2(0.9f), parent, Anchor.Center, null, null, null, ScaleBasis.Normal), this.cancelIcon, true, null)
			{
				CanBeFocused = false,
				Color = GUIStyle.Red * 0.75f,
				HoverColor = GUIStyle.Red
			};
			if (!tooltip.IsNullOrEmpty())
			{
				string color = XMLExtensions.ColorToString(GUIStyle.Red);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(19, 2);
				defaultInterpolatedStringHandler.AppendLiteral("‖color:");
				defaultInterpolatedStringHandler.AppendFormatted(color);
				defaultInterpolatedStringHandler.AppendLiteral("‖");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(tooltip);
				defaultInterpolatedStringHandler.AppendLiteral("‖color:end‖");
				tooltip = defaultInterpolatedStringHandler.ToStringAndClear();
				icon.ToolTip = RichString.Rich(tooltip, null);
				icon.CanBeFocused = true;
			}
		}

		// Token: 0x06000E3A RID: 3642 RVA: 0x000860EA File Offset: 0x000842EA
		private int GetCircumferencePointCount(int nodes)
		{
			if (nodes % 2 <= 0)
			{
				return nodes + 1;
			}
			return nodes;
		}

		// Token: 0x06000E3B RID: 3643 RVA: 0x000860F8 File Offset: 0x000842F8
		private float GetFirstNodeAngle(int nodeCount)
		{
			float bearing = 90f;
			if (this.returnNode != null)
			{
				bearing = this.GetBearing(this.centerNode.RectTransform.AnimTargetPos.ToVector2(), this.returnNode.RectTransform.AnimTargetPos.ToVector2(), false, false);
			}
			else if (this.shortcutCenterNode != null)
			{
				bearing = this.GetBearing(this.centerNode.RectTransform.AnimTargetPos.ToVector2(), this.shorcutCenterNodeOffset.ToVector2(), false, false);
			}
			if (nodeCount % 2 <= 0)
			{
				return MathHelper.ToRadians(bearing + 360f / (float)(nodeCount + 1));
			}
			return MathHelper.ToRadians(bearing + 360f / (float)nodeCount / 2f);
		}

		// Token: 0x06000E3C RID: 3644 RVA: 0x000861B0 File Offset: 0x000843B0
		private float GetBearing(Vector2 startPoint, Vector2 endPoint, bool flipY = false, bool flipX = false)
		{
			double radians = Math.Atan2((double)((!flipY) ? (endPoint.Y - startPoint.Y) : (startPoint.Y - endPoint.Y)), (double)((!flipX) ? (endPoint.X - startPoint.X) : (startPoint.X - endPoint.X)));
			float degrees = MathHelper.ToDegrees((float)radians);
			if (degrees >= 0f)
			{
				return degrees;
			}
			return degrees + 360f;
		}

		// Token: 0x06000E3D RID: 3645 RVA: 0x00086220 File Offset: 0x00084420
		private bool TryGetBreachedHullAtHoveredWall(out Hull breachedHull, out WallSection hoveredWall)
		{
			breachedHull = null;
			hoveredWall = null;
			List<Gap> leaks = Gap.GapList.FindAll(delegate(Gap g)
			{
				if (g != null && g.ConnectedWall != null && g.ConnectedDoor == null && g.Open > 0f)
				{
					if (g.linkedTo.Any((MapEntity l) => l != null) && g.Submarine != null)
					{
						return Character.Controlled != null && g.Submarine.TeamID == Character.Controlled.TeamID && g.Submarine.Info.IsPlayer;
					}
				}
				return false;
			});
			if (leaks.None(null))
			{
				return false;
			}
			Vector2 mouseWorldPosition = GameMain.GameScreen.Cam.ScreenToWorld(PlayerInput.MousePosition);
			foreach (Gap leak in leaks)
			{
				if (Submarine.RectContains(leak.ConnectedWall.WorldRect, mouseWorldPosition, false))
				{
					breachedHull = leak.FlowTargetHull;
					foreach (WallSection section in leak.ConnectedWall.Sections)
					{
						if (Submarine.RectContains(section.WorldRect, mouseWorldPosition, false))
						{
							hoveredWall = section;
							break;
						}
					}
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000E3E RID: 3646 RVA: 0x0008631C File Offset: 0x0008451C
		private Submarine GetTargetSubmarine()
		{
			Submarine sub = Submarine.MainSub;
			if (Character.Controlled != null)
			{
				if (Character.Controlled.TeamID == CharacterTeamType.Team2 && Submarine.MainSubs.Length > 1)
				{
					sub = Submarine.MainSubs[1];
				}
				Submarine currentSub = Character.Controlled.Submarine;
				if (currentSub != null && currentSub != sub && currentSub.TeamID == Character.Controlled.TeamID && !currentSub.IsConnectedTo(sub))
				{
					sub = currentSub;
				}
			}
			return sub;
		}

		// Token: 0x06000E3F RID: 3647 RVA: 0x00086388 File Offset: 0x00084588
		private void SetCharacterTooltip(GUIComponent component, Character character)
		{
			if (component == null)
			{
				return;
			}
			LocalizedString tooltip = (((character != null) ? character.Info : null) != null) ? this.characterContext.Info.DisplayName : null;
			if (tooltip.IsNullOrWhiteSpace())
			{
				component.ToolTip = tooltip;
				return;
			}
			CharacterInfo info = character.Info;
			if (((info != null) ? info.Job : null) != null && !this.characterContext.Info.Job.Name.IsNullOrWhiteSpace())
			{
				tooltip += " (" + this.characterContext.Info.Job.Name + ")";
			}
			component.ToolTip = tooltip;
		}

		// Token: 0x06000E40 RID: 3648 RVA: 0x0008644B File Offset: 0x0008464B
		private LocalizedString GetOrderNameBasedOnContextuality(Order order)
		{
			if (order == null)
			{
				return "";
			}
			if (this.isContextual)
			{
				return order.ContextualName;
			}
			return order.Name;
		}

		// Token: 0x06000E41 RID: 3649 RVA: 0x00086470 File Offset: 0x00084670
		private int GetManualOrderPriority(Character character, Order order)
		{
			int? num;
			if (character == null)
			{
				num = null;
			}
			else
			{
				CharacterInfo info = character.Info;
				num = ((info != null) ? new int?(info.GetManualOrderPriority(order)) : null);
			}
			int? num2 = num;
			if (num2 == null)
			{
				return CharacterInfo.HighestManualOrderPriority;
			}
			return num2.GetValueOrDefault();
		}

		// Token: 0x06000E42 RID: 3650 RVA: 0x000864C2 File Offset: 0x000846C2
		private bool IsOrderAvailable(Order order)
		{
			return this.IsOrderAvailable(order.Prefab);
		}

		// Token: 0x06000E43 RID: 3651 RVA: 0x000864D0 File Offset: 0x000846D0
		private bool IsOrderAvailable(OrderPrefab order)
		{
			if (order == null)
			{
				return false;
			}
			string a = order.Identifier.Value.ToLowerInvariant();
			if (a == "assaultenemy")
			{
				Character character = this.characterContext ?? Character.Controlled;
				Character character2 = character;
				return ((character2 != null) ? character2.Submarine : null) != null && character.Submarine.GetConnectedSubs().Any((Submarine s) => s.TeamID != character.TeamID);
			}
			return true;
		}

		// Token: 0x06000E44 RID: 3652 RVA: 0x00086554 File Offset: 0x00084754
		private bool CanOpenManualAssignmentMinimapOrder(GUIComponent node)
		{
			if (node == null || this.characterContext != null)
			{
				return false;
			}
			object userData = node.UserData;
			if (userData is CrewManager.MinimapNodeData)
			{
				Order minimapOrder = ((CrewManager.MinimapNodeData)userData).Order;
				if (minimapOrder != null)
				{
					return !minimapOrder.TargetAllCharacters && (!minimapOrder.Prefab.HasOptions || !minimapOrder.Option.IsEmpty);
				}
			}
			Order nodeOrder = node.UserData as Order;
			if (nodeOrder == null)
			{
				return false;
			}
			if (nodeOrder.TargetAllCharacters || nodeOrder.Prefab.HasOptions)
			{
				return false;
			}
			if (nodeOrder.MustSetTarget && this.itemContext == null)
			{
				Order order = nodeOrder;
				Submarine targetSubmarine = this.GetTargetSubmarine();
				bool mustBelongToPlayerSub = true;
				Character controlled = Character.Controlled;
				return order.GetMatchingItems(targetSubmarine, mustBelongToPlayerSub, null, controlled).Count < 2;
			}
			return true;
		}

		// Token: 0x06000E45 RID: 3653 RVA: 0x00086614 File Offset: 0x00084814
		private bool CanOpenManualAssignment(GUIComponent node)
		{
			if (node == null || this.characterContext != null)
			{
				return false;
			}
			Order nodeOrder = node.UserData as Order;
			if (nodeOrder == null)
			{
				return false;
			}
			if (nodeOrder.TargetAllCharacters || (nodeOrder.Prefab.HasOptions && nodeOrder.Option.IsEmpty))
			{
				return false;
			}
			if (nodeOrder.MustSetTarget && this.itemContext == null)
			{
				Order order = nodeOrder;
				Submarine targetSubmarine = this.GetTargetSubmarine();
				bool mustBelongToPlayerSub = true;
				Character controlled = Character.Controlled;
				return order.GetMatchingItems(targetSubmarine, mustBelongToPlayerSub, null, controlled).Count < 2;
			}
			return true;
		}

		// Token: 0x06000E46 RID: 3654 RVA: 0x0008669A File Offset: 0x0008489A
		private Character GetCharacterForQuickAssignment(Order order)
		{
			return CrewManager.GetCharacterForQuickAssignment(order, Character.Controlled, this.characters, false);
		}

		// Token: 0x06000E47 RID: 3655 RVA: 0x000866B0 File Offset: 0x000848B0
		private List<Character> GetCharactersForManualAssignment(Order order)
		{
			if (Character.Controlled == null)
			{
				return new List<Character>();
			}
			Identifier identifier = order.Identifier;
			if (identifier == this.dismissedOrderPrefab.Identifier)
			{
				return (from c in this.characters.Union(this.GetOrderableFriendlyNPCs())
				where !c.IsDismissed
				orderby c.Info.DisplayName
				select c).ToList<Character>();
			}
			IEnumerable<Character> enumerable = this.characters;
			Character controlled = Character.Controlled;
			identifier = order.Identifier;
			return CrewManager.GetCharactersSortedForOrder(order, enumerable, controlled, identifier != "follow", this.GetOrderableFriendlyNPCs()).ToList<Character>();
		}

		// Token: 0x06000E48 RID: 3656 RVA: 0x00086774 File Offset: 0x00084974
		private IEnumerable<Character> GetOrderableFriendlyNPCs()
		{
			return from c in this.crewList.Content.Children.Where(delegate(GUIComponent c)
			{
				Character character = c.UserData as Character;
				return character != null && character.TeamID == CharacterTeamType.FriendlyNPC;
			})
			select (Character)c.UserData;
		}

		// Token: 0x06000E49 RID: 3657 RVA: 0x000867DC File Offset: 0x000849DC
		public void UpdateReports()
		{
			bool canIssueOrders = false;
			Character controlled = Character.Controlled;
			bool flag;
			if (controlled == null)
			{
				flag = (null != null);
			}
			else
			{
				Hull currentHull = controlled.CurrentHull;
				flag = (((currentHull != null) ? currentHull.Submarine : null) != null);
			}
			if (flag && Character.Controlled.SpeechImpediment < 100f)
			{
				bool flag2;
				if (ChatMessage.CanUseRadio(Character.Controlled, false))
				{
					Character controlled2 = Character.Controlled;
					CharacterTeamType? characterTeamType;
					if (controlled2 == null)
					{
						characterTeamType = null;
					}
					else
					{
						Hull currentHull2 = controlled2.CurrentHull;
						if (currentHull2 == null)
						{
							characterTeamType = null;
						}
						else
						{
							Submarine submarine = currentHull2.Submarine;
							characterTeamType = ((submarine != null) ? new CharacterTeamType?(submarine.TeamID) : null);
						}
					}
					CharacterTeamType? characterTeamType2 = characterTeamType;
					CharacterTeamType teamID = Character.Controlled.TeamID;
					if ((characterTeamType2.GetValueOrDefault() == teamID & characterTeamType2 != null) && !Character.Controlled.CurrentHull.Submarine.Info.IsWreck)
					{
						GameClient client = GameMain.Client;
						flag2 = (client == null || !client.IsBlockedBySpamFilter);
						goto IL_E4;
					}
				}
				flag2 = false;
				IL_E4:
				canIssueOrders = flag2;
			}
			if (canIssueOrders)
			{
				this.ReportButtonFrame.Visible = !Character.Controlled.ShouldLockHud();
				if (!this.ReportButtonFrame.Visible)
				{
					return;
				}
				ChatBox chatBox;
				if ((chatBox = this.ChatBox) == null)
				{
					GameClient client2 = GameMain.Client;
					chatBox = ((client2 != null) ? client2.ChatBox : null);
				}
				ChatBox reportButtonParent = chatBox;
				if (reportButtonParent == null)
				{
					return;
				}
				this.ReportButtonFrame.RectTransform.AbsoluteOffset = new Point(reportButtonParent.GUIFrame.Rect.Right + (int)(10f * GUI.Scale), reportButtonParent.GUIFrame.Rect.Y);
				bool hasFires = Character.Controlled.CurrentHull.FireSources.Count > 0;
				this.ToggleReportButton("reportfire", hasFires);
				bool hasLeaks = Character.Controlled.CurrentHull.ConnectedGaps.Any((Gap g) => !g.IsRoomToRoom && g.Open > 0f);
				this.ToggleReportButton("reportbreach", hasLeaks);
				bool hasIntruders = Character.CharacterList.Any((Character c) => c.CurrentHull == Character.Controlled.CurrentHull && AIObjectiveFightIntruders.IsValidTarget(c, Character.Controlled, false));
				this.ToggleReportButton("reportintruders", hasIntruders);
				using (IEnumerator<GUIComponent> enumerator = this.ReportButtonFrame.Children.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						GUIComponent reportButton = enumerator.Current;
						GUIComponent highlight = reportButton.GetChildByUserData("highlighted");
						if (highlight.Visible)
						{
							highlight.RectTransform.LocalScale = new Vector2(1.25f + (float)Math.Sin(Timing.TotalTime * 5.0) * 0.25f);
						}
					}
					return;
				}
			}
			this.ReportButtonFrame.Visible = false;
		}

		// Token: 0x06000E4A RID: 3658 RVA: 0x00086AA0 File Offset: 0x00084CA0
		private void ToggleReportButton(string orderIdentifier, bool enabled)
		{
			this.ToggleReportButton(orderIdentifier.ToIdentifier(), enabled);
		}

		// Token: 0x06000E4B RID: 3659 RVA: 0x00086AB0 File Offset: 0x00084CB0
		private void ToggleReportButton(Identifier orderIdentifier, bool enabled)
		{
			GUIComponent reportButton = this.ReportButtonFrame.FindChild(delegate(GUIComponent c)
			{
				OrderPrefab orderPrefab = c.UserData as OrderPrefab;
				return orderPrefab != null && orderPrefab.Identifier == orderIdentifier;
			}, false);
			if (reportButton != null)
			{
				reportButton.GetChildByUserData("highlighted").Visible = enabled;
			}
		}

		// Token: 0x06000E4C RID: 3660 RVA: 0x00086AF7 File Offset: 0x00084CF7
		public void InitSinglePlayerRound()
		{
			this.crewList.ClearChildren();
			this.InitRound();
		}

		// Token: 0x06000E4D RID: 3661 RVA: 0x00086B0A File Offset: 0x00084D0A
		public void Reset()
		{
			this.characters.Clear();
			this.characterInfos.Clear();
			this.crewList.ClearChildren();
		}

		// Token: 0x06000E4E RID: 3662 RVA: 0x00086B30 File Offset: 0x00084D30
		public XElement Save(XElement parentElement)
		{
			XElement element = new XElement("crew");
			for (int i = 0; i < this.characterInfos.Count; i++)
			{
				CharacterInfo ci = this.characterInfos[i];
				XElement infoElement = ci.Save(element);
				if (ci.InventoryData != null)
				{
					infoElement.Add(ci.InventoryData);
				}
				if (ci.HealthData != null)
				{
					infoElement.Add(ci.HealthData);
				}
				if (ci.OrderData != null)
				{
					infoElement.Add(ci.OrderData);
				}
				infoElement.Add(new XAttribute("crewlistindex", ci.CrewListIndex));
				if (ci.LastControlled)
				{
					infoElement.Add(new XAttribute("lastcontrolled", true));
				}
			}
			if (parentElement != null)
			{
				parentElement.Add(element);
			}
			return element;
		}

		// Token: 0x06000E4F RID: 3663 RVA: 0x00086C08 File Offset: 0x00084E08
		public static void ClientReadActiveOrders(IReadMessage inc)
		{
			ushort count = inc.ReadUInt16();
			if (count < 1)
			{
				return;
			}
			List<ValueTuple<Order, float?>> activeOrders = new List<ValueTuple<Order, float?>>();
			for (ushort i = 0; i < count; i += 1)
			{
				OrderChatMessage.OrderMessageInfo orderMessageInfo = OrderChatMessage.ReadOrder(inc);
				Character orderGiver = null;
				if (inc.ReadBoolean())
				{
					ushort orderGiverId = inc.ReadUInt16();
					orderGiver = ((orderGiverId != 0) ? (Entity.FindEntityByID(orderGiverId) as Character) : null);
				}
				Identifier identifier = orderMessageInfo.OrderIdentifier;
				if (identifier == Identifier.Empty)
				{
					DebugConsole.ThrowError("Invalid active order - order identifier empty.", null, null, false, false);
				}
				else
				{
					OrderPrefab orderPrefab = orderMessageInfo.OrderPrefab ?? OrderPrefab.Prefabs[orderMessageInfo.OrderIdentifier];
					Order order3;
					switch (orderMessageInfo.TargetType)
					{
					case Order.OrderTargetType.Entity:
						order3 = new Order(orderPrefab, orderMessageInfo.TargetEntity, orderPrefab.GetTargetItemComponent(orderMessageInfo.TargetEntity as Item), orderGiver, false);
						break;
					case Order.OrderTargetType.Position:
						order3 = new Order(orderPrefab, orderMessageInfo.TargetPosition, orderGiver);
						break;
					case Order.OrderTargetType.WallSection:
						order3 = new Order(orderPrefab, orderMessageInfo.TargetEntity as Structure, orderMessageInfo.WallSectionIndex, orderGiver);
						break;
					default:
						throw new NotImplementedException();
					}
					Order order = order3;
					if (order != null && order.TargetAllCharacters)
					{
						float? fadeOutTime = (!orderPrefab.IsIgnoreOrder) ? new float?(orderPrefab.FadeOutTime) : null;
						activeOrders.Add(new ValueTuple<Order, float?>(order, fadeOutTime));
					}
				}
			}
			foreach (ValueTuple<Order, float?> valueTuple in activeOrders)
			{
				Order order2 = valueTuple.Item1;
				float? fadeOutTime2 = valueTuple.Item2;
				if (order2.IsIgnoreOrder)
				{
					switch (order2.TargetType)
					{
					case Order.OrderTargetType.Entity:
					{
						IIgnorable ignorableEntity = order2.TargetEntity as IIgnorable;
						if (ignorableEntity != null)
						{
							IIgnorable ignorable = ignorableEntity;
							Identifier identifier = order2.Identifier;
							ignorable.OrderedToBeIgnored = (identifier == Tags.IgnoreThis);
						}
						break;
					}
					case Order.OrderTargetType.Position:
						throw new NotImplementedException();
					case Order.OrderTargetType.WallSection:
						if (order2.WallSectionIndex != null)
						{
							Structure s = order2.TargetEntity as Structure;
							if (s != null)
							{
								IIgnorable ignorableWall = s.GetSection(order2.WallSectionIndex.Value);
								if (ignorableWall != null)
								{
									IIgnorable ignorable2 = ignorableWall;
									Identifier identifier = order2.Identifier;
									ignorable2.OrderedToBeIgnored = (identifier == Tags.IgnoreThis);
								}
							}
						}
						break;
					}
				}
				GameSession gameSession = GameMain.GameSession;
				if (gameSession != null)
				{
					CrewManager crewManager = gameSession.CrewManager;
					if (crewManager != null)
					{
						crewManager.AddOrder(order2, fadeOutTime2);
					}
				}
			}
		}

		// Token: 0x06000E50 RID: 3664 RVA: 0x00086EA8 File Offset: 0x000850A8
		public bool UpdateReserveBenchIfNeeded(IEnumerable<CharacterInfo> updatedReserveBench)
		{
			HashSet<CharacterInfo> newBench = updatedReserveBench.ToHashSet(new CrewManager.CharacterInfoComparer());
			HashSet<CharacterInfo> currentBench = this.reserveBench.ToHashSet(new CrewManager.CharacterInfoComparer());
			bool updateNeeded = !newBench.SetEquals(currentBench);
			if (updateNeeded)
			{
				this.reserveBench.Clear();
				this.reserveBench.AddRange(updatedReserveBench);
			}
			return updateNeeded;
		}

		// Token: 0x06000E51 RID: 3665 RVA: 0x00086EF8 File Offset: 0x000850F8
		public bool UpdateCrewManagerIfNecessary(List<CharacterInfo> updatedCrewManager)
		{
			List<CharacterInfo> toRemove = (from original in this.characterInfos
			where updatedCrewManager.None((CharacterInfo updated) => updated.ID == original.ID)
			select original).ToList<CharacterInfo>();
			List<CharacterInfo> toAdd = (from updated in updatedCrewManager
			where this.characterInfos.None((CharacterInfo original) => original.ID == updated.ID)
			select updated).ToList<CharacterInfo>();
			foreach (CharacterInfo characterInfo in toRemove)
			{
				Character existingCharacter = characterInfo.Character;
				if (existingCharacter != null)
				{
					if (existingCharacter.IsBot)
					{
						this.RemoveCharacter(characterInfo.Character, true, true);
					}
				}
				else
				{
					this.characterInfos.Remove(characterInfo);
				}
			}
			this.characterInfos.AddRange(toAdd);
			return toRemove.Count > 0 || toAdd.Count > 0;
		}

		// Token: 0x06000E52 RID: 3666 RVA: 0x00086FE4 File Offset: 0x000851E4
		public IEnumerable<CharacterInfo> GetCharacterInfos(bool includeReserveBench = false)
		{
			if (includeReserveBench)
			{
				return this.characterInfos.Concat(this.reserveBench);
			}
			return this.characterInfos;
		}

		// Token: 0x06000E53 RID: 3667 RVA: 0x00087001 File Offset: 0x00085201
		public IEnumerable<CharacterInfo> GetReserveBenchInfos()
		{
			return this.reserveBench;
		}

		// Token: 0x170003D5 RID: 981
		// (get) Token: 0x06000E54 RID: 3668 RVA: 0x00087009 File Offset: 0x00085209
		// (set) Token: 0x06000E55 RID: 3669 RVA: 0x00087011 File Offset: 0x00085211
		public bool HasBots { get; set; }

		// Token: 0x170003D6 RID: 982
		// (get) Token: 0x06000E56 RID: 3670 RVA: 0x0008701A File Offset: 0x0008521A
		public List<CrewManager.ActiveOrder> ActiveOrders { get; } = new List<CrewManager.ActiveOrder>();

		// Token: 0x170003D7 RID: 983
		// (get) Token: 0x06000E57 RID: 3671 RVA: 0x00087022 File Offset: 0x00085222
		// (set) Token: 0x06000E58 RID: 3672 RVA: 0x0008702A File Offset: 0x0008522A
		public bool IsSinglePlayer { get; private set; }

		// Token: 0x06000E59 RID: 3673 RVA: 0x00087034 File Offset: 0x00085234
		public CrewManager(bool isSinglePlayer)
		{
			this.IsSinglePlayer = isSinglePlayer;
			this.conversationTimer = 5f;
			this.InitProjectSpecific();
		}

		// Token: 0x06000E5A RID: 3674 RVA: 0x0008712C File Offset: 0x0008532C
		private void InitProjectSpecific()
		{
			CrewManager.<>c__DisplayClass234_0 CS$<>8__locals1 = new CrewManager.<>c__DisplayClass234_0();
			CS$<>8__locals1.<>4__this = this;
			this.guiFrame = new GUIFrame(new RectTransform(Vector2.One, GUI.Canvas, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, new Color?(Color.Transparent))
			{
				CanBeFocused = false
			};
			this.crewArea = new GUILayoutGroup(HUDLayoutSettings.ToRectTransform(HUDLayoutSettings.CrewArea, this.guiFrame.RectTransform), false, Anchor.TopCenter)
			{
				Stretch = true
			};
			this.crewArea.RectTransform.NonScaledSize = HUDLayoutSettings.CrewArea.Size;
			for (int i = 0; i < 2; i++)
			{
				CharacterTeamType teamId = (i == 0) ? CharacterTeamType.Team1 : CharacterTeamType.Team2;
				GUITextBlock nameText = new GUITextBlock(new RectTransform(new Point(this.crewArea.Rect.Width - GUI.IntScale(10f), GUI.IntScale(30f)), this.crewArea.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), CombatMission.GetTeamName(teamId), new Color?(CombatMission.GetTeamColor(teamId)), null, Alignment.Left, false, "", null)
				{
					ForceUpperCase = ForceUpperCase.Yes,
					TextGetter = (() => CombatMission.GetTeamName(teamId)),
					Visible = false,
					IgnoreLayoutGroups = true,
					UserData = teamId
				};
				GUIImage teamIcon = new GUIImage(new RectTransform(Vector2.One, nameText.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.BothHeight), (i == 0) ? "CoalitionIcon" : "SeparatistIcon", GUIImage.ScalingMode.None)
				{
					Color = nameText.TextColor
				};
				nameText.Padding = new Vector4((float)teamIcon.Rect.Width + nameText.Padding.X, nameText.Padding.Y, nameText.Padding.Z, nameText.Padding.W);
			}
			GUIListBox guilistBox = new GUIListBox(new RectTransform(Vector2.One, this.crewArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, null, false, false);
			guilistBox.AutoHideScrollBar = false;
			guilistBox.CanBeFocused = false;
			guilistBox.CurrentDragMode = GUIListBox.DragMode.DragWithinBox;
			guilistBox.CanInteractWhenUnfocusable = true;
			guilistBox.OnSelected = ((GUIComponent component, object userData) => false);
			guilistBox.SelectMultiple = false;
			guilistBox.Spacing = (int)(GUI.Scale * 10f);
			guilistBox.OnRearranged = new GUIListBox.OnRearrangedHandler(this.OnCrewListRearranged);
			this.crewList = guilistBox;
			this.jobIndicatorBackground = new Sprite("Content/UI/CommandUIAtlas.png", new Rectangle?(new Rectangle(0, 512, 128, 128)), null, 0f);
			this.previousOrderArrow = new Sprite("Content/UI/CommandUIAtlas.png", new Rectangle?(new Rectangle(128, 512, 128, 128)), null, 0f);
			this.cancelIcon = new Sprite("Content/UI/CommandUIAtlas.png", new Rectangle?(new Rectangle(512, 384, 128, 128)), null, 0f);
			this.crewListEntrySize = new Point(this.crewList.Content.Rect.Width - HUDLayoutSettings.Padding, 0);
			int crewListEntryMinHeight = 32;
			this.crewListEntrySize.Y = Math.Max(crewListEntryMinHeight, (int)((float)this.crewListEntrySize.X / 8f));
			float charactersPerView = (float)this.crewList.Content.Rect.Height / (float)(this.crewListEntrySize.Y + this.crewList.Spacing);
			int adjustedHeight = (int)Math.Ceiling((double)this.crewList.Content.Rect.Height / Math.Round((double)charactersPerView)) - this.crewList.Spacing;
			if (adjustedHeight < crewListEntryMinHeight)
			{
				adjustedHeight = (int)Math.Ceiling((double)this.crewList.Content.Rect.Height / Math.Floor((double)charactersPerView)) - this.crewList.Spacing;
			}
			this.crewListEntrySize.Y = adjustedHeight;
			if (this.IsSinglePlayer)
			{
				this.ChatBox = new ChatBox(this.guiFrame, true)
				{
					OnEnterMessage = delegate(GUITextBox textbox, string text)
					{
						Character controlled = Character.Controlled;
						if (((controlled != null) ? controlled.Info : null) == null)
						{
							textbox.Deselect();
							textbox.Text = "";
							return true;
						}
						textbox.TextColor = ChatMessage.MessageColor[0];
						if (!string.IsNullOrWhiteSpace(text))
						{
							string msg;
							string msgCommand = ChatMessage.GetChatMessageCommand(text, out msg);
							CS$<>8__locals1.<>4__this.ChatBox.ChatManager.Store(text);
							bool isUsingRadioMode = GameMain.ActiveChatMode == ChatMode.Radio;
							bool containsRadioCommand = msgCommand == "r" || msgCommand == "radio";
							WifiComponent headset;
							bool canUseRadio = ChatMessage.CanUseRadio(Character.Controlled, out headset, false);
							ChatMessageType messageType = (((isUsingRadioMode && msgCommand == "") || containsRadioCommand) && canUseRadio) ? ChatMessageType.Radio : ChatMessageType.Default;
							CS$<>8__locals1.<>4__this.AddSinglePlayerChatMessage(Character.Controlled.Info.Name, msg, messageType, Character.Controlled);
							Character.Controlled.ShowSpeechBubble(ChatMessage.MessageColor[(int)messageType], text);
							if (messageType == ChatMessageType.Radio && headset != null)
							{
								Signal s = new Signal(msg, 0, Character.Controlled, headset.Item, 0f, 1f);
								headset.TransmitSignal(s, true);
							}
						}
						textbox.Deselect();
						textbox.Text = "";
						if (CS$<>8__locals1.<>4__this.ChatBox.CloseAfterMessageSent)
						{
							CS$<>8__locals1.<>4__this.ChatBox.ToggleOpen = false;
							CS$<>8__locals1.<>4__this.ChatBox.CloseAfterMessageSent = false;
						}
						return true;
					}
				};
				this.ChatBox.InputBox.OnTextChanged += this.ChatBox.TypingChatMessage;
			}
			else if (GameMain.Client == null)
			{
				throw new InvalidOperationException("Attempted to initialize CrewManager for multiplayer, but no multiplayer client is active. Are you trying to load a multiplayer save in singleplayer?");
			}
			CrewManager.<>c__DisplayClass234_0 CS$<>8__locals3 = CS$<>8__locals1;
			ChatBox chatBox;
			if ((chatBox = this.ChatBox) == null)
			{
				GameClient client = GameMain.Client;
				chatBox = ((client != null) ? client.ChatBox : null);
			}
			CS$<>8__locals3.chatBox = chatBox;
			if (CS$<>8__locals1.chatBox != null)
			{
				ChatBox chatBox2 = CS$<>8__locals1.chatBox;
				GUIButton guibutton = new GUIButton(new RectTransform(new Point((int)(182f * GUI.Scale * 0.4f), (int)(99f * GUI.Scale * 0.4f)), CS$<>8__locals1.chatBox.GUIFrame.Parent.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), Alignment.Center, "ChatToggleButton", null);
				string tag = "hudbutton.chatbox";
				string varName = "[key]";
				GameSettings.Config.KeyMapping keyMap = GameSettings.CurrentConfig.KeyMap;
				guibutton.ToolTip = TextManager.GetWithVariable(tag, varName, keyMap.KeyBindText(InputType.ChatBox), FormatCapitals.No);
				guibutton.ClampMouseRectToParent = false;
				chatBox2.ToggleButton = guibutton;
				CS$<>8__locals1.chatBox.ToggleButton.RectTransform.AbsoluteOffset = new Point(0, HUDLayoutSettings.ChatBoxArea.Height - CS$<>8__locals1.chatBox.ToggleButton.Rect.Height);
				GUIButton toggleButton = CS$<>8__locals1.chatBox.ToggleButton;
				toggleButton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(toggleButton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object userdata)
				{
					CS$<>8__locals1.chatBox.Toggle();
					return true;
				}));
			}
			OrderPrefab[] reports = (from o in OrderPrefab.Prefabs
			where o.IsVisibleAsReportButton
			orderby o.Identifier
			select o).ToArray<OrderPrefab>();
			if (reports.None(null))
			{
				DebugConsole.ThrowError("No valid orders for report buttons found! Cannot create report buttons. The orders for the report buttons must have 'targetallcharacters' attribute enabled and a valid 'symbolsprite' defined.", null, null, false, false);
				return;
			}
			this.ReportButtonFrame = new GUILayoutGroup(new RectTransform(new Point((HUDLayoutSettings.ChatBoxArea.Height - CS$<>8__locals1.chatBox.ToggleButton.Rect.Height - (int)((float)((reports.Length - 1) * 5) * GUI.Scale)) / reports.Length, HUDLayoutSettings.ChatBoxArea.Height - CS$<>8__locals1.chatBox.ToggleButton.Rect.Height), this.guiFrame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), false, Anchor.TopLeft)
			{
				AbsoluteSpacing = (int)(5f * GUI.Scale),
				UserData = "reportbuttons",
				CanBeFocused = false,
				Visible = false
			};
			this.ReportButtonFrame.RectTransform.AbsoluteOffset = new Point(0, -CS$<>8__locals1.chatBox.ToggleButton.Rect.Height);
			CrewManager.CreateReportButtons(this, this.ReportButtonFrame, reports, false);
			this.screenResolution = new Point(GameMain.GraphicsWidth, GameMain.GraphicsHeight);
			this.prevUIScale = GUI.Scale;
			this._isCrewMenuOpen = (CrewManager.PreferCrewMenuOpen = GameSettings.CurrentConfig.CrewMenuOpen);
		}

		// Token: 0x06000E5B RID: 3675 RVA: 0x000878E8 File Offset: 0x00085AE8
		public bool AddOrder(Order order, float? fadeOutTime)
		{
			if (order.TargetEntity == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(66, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Attempted to add a \"");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(order.Name);
				defaultInterpolatedStringHandler.AppendLiteral("\" order with no target entity to CrewManager!\n");
				defaultInterpolatedStringHandler.AppendFormatted(Environment.StackTrace.CleanupStackTrace());
				string message = defaultInterpolatedStringHandler.ToStringAndClear();
				DebugConsole.AddWarning(message, null);
				GameAnalyticsManager.AddErrorEventOnce("CrewManager.AddOrder:OrderTargetEntityNull", GameAnalyticsManager.ErrorSeverity.Error, message);
				return false;
			}
			Identifier identifier = order.Identifier;
			bool isUnignoreOrder = identifier == Tags.UnignoreThis;
			identifier = order.Identifier;
			bool isIgnoreOrder = identifier == Tags.IgnoreThis;
			OrderPrefab orderPrefab = (!isUnignoreOrder) ? order.Prefab : OrderPrefab.Prefabs[Tags.IgnoreThis];
			CrewManager.ActiveOrder existingOrder = this.ActiveOrders.Find(delegate(CrewManager.ActiveOrder o)
			{
				if (o.Order.Prefab != orderPrefab || !CrewManager.<AddOrder>g__MatchesTarget|235_1(o.Order.TargetEntity, order.TargetEntity))
				{
					return false;
				}
				if (o.Order.TargetType == Order.OrderTargetType.WallSection)
				{
					int? wallSectionIndex = o.Order.WallSectionIndex;
					int? wallSectionIndex2 = order.WallSectionIndex;
					return wallSectionIndex.GetValueOrDefault() == wallSectionIndex2.GetValueOrDefault() & wallSectionIndex != null == (wallSectionIndex2 != null);
				}
				return true;
			});
			if (existingOrder != null)
			{
				if (!isUnignoreOrder)
				{
					existingOrder.FadeOutTime = fadeOutTime;
					return false;
				}
				this.ActiveOrders.Remove(existingOrder);
				if (isIgnoreOrder)
				{
					Item targetItem = order.TargetEntity as Item;
					if (targetItem != null)
					{
						using (IEnumerator<Item> enumerator = targetItem.GetStackedItems().GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								Item stackedItem = enumerator.Current;
								this.ActiveOrders.RemoveAll((CrewManager.ActiveOrder o) => o.Order.Prefab == orderPrefab && o.Order.TargetEntity == stackedItem);
								stackedItem.OrderedToBeIgnored = false;
							}
						}
					}
				}
				return true;
			}
			else
			{
				if (!isUnignoreOrder)
				{
					if (order.IsDeconstructOrder)
					{
						Item item = order.TargetEntity as Item;
						if (item != null)
						{
							identifier = order.Identifier;
							if (identifier == Tags.DeconstructThis)
							{
								foreach (Item stackedItem4 in item.GetStackedItems())
								{
									Item.DeconstructItems.Add(stackedItem4);
								}
								HintManager.OnItemMarkedForDeconstruction(order.OrderGiver);
							}
							else
							{
								foreach (Item stackedItem2 in item.GetStackedItems())
								{
									Item.DeconstructItems.Remove(stackedItem2);
								}
							}
						}
					}
					if (isIgnoreOrder)
					{
						Item targetItem2 = order.TargetEntity as Item;
						if (targetItem2 != null)
						{
							using (IEnumerator<Item> enumerator4 = targetItem2.GetStackedItems().GetEnumerator())
							{
								while (enumerator4.MoveNext())
								{
									Item stackedItem3 = enumerator4.Current;
									this.ActiveOrders.Add(new CrewManager.ActiveOrder(order.WithTargetEntity(stackedItem3), fadeOutTime));
									stackedItem3.OrderedToBeIgnored = true;
								}
								goto IL_2F3;
							}
						}
					}
					this.ActiveOrders.Add(new CrewManager.ActiveOrder(order, fadeOutTime));
					IL_2F3:
					HintManager.OnActiveOrderAdded(order);
					return true;
				}
				return false;
			}
		}

		// Token: 0x06000E5C RID: 3676 RVA: 0x00087C2C File Offset: 0x00085E2C
		public void AddCharacterElements(XElement element)
		{
			foreach (XElement characterElement in element.Elements())
			{
				if (characterElement.Name.ToString().Equals("character", StringComparison.OrdinalIgnoreCase))
				{
					CharacterInfo characterInfo = new CharacterInfo(new ContentXElement(null, characterElement), default(Identifier));
					if (characterElement.GetAttributeBool("lastcontrolled", false))
					{
						characterInfo.LastControlled = true;
					}
					characterInfo.CrewListIndex = characterElement.GetAttributeInt("crewlistindex", -1);
					if (characterElement.GetAttributeBool("IsOnReserveBench", false))
					{
						this.reserveBench.Add(characterInfo);
					}
					else
					{
						this.characterInfos.Add(characterInfo);
					}
					foreach (XElement subElement in characterElement.Elements())
					{
						string a = subElement.Name.ToString().ToLowerInvariant();
						if (!(a == "inventory"))
						{
							if (!(a == "health"))
							{
								if (a == "orders")
								{
									characterInfo.OrderData = subElement;
								}
							}
							else
							{
								characterInfo.HealthData = subElement;
							}
						}
						else
						{
							characterInfo.InventoryData = subElement;
						}
					}
				}
			}
		}

		// Token: 0x06000E5D RID: 3677 RVA: 0x00087DA8 File Offset: 0x00085FA8
		public void RemoveCharacterInfo(CharacterInfo characterInfo)
		{
			if (characterInfo != null && characterInfo.IsOnReserveBench)
			{
				this.reserveBench.Remove(characterInfo);
			}
			this.characterInfos.Remove(characterInfo);
			GameSession gameSession = GameMain.GameSession;
			if (gameSession == null)
			{
				return;
			}
			DeathPrompt deathPrompt = gameSession.DeathPrompt;
			if (deathPrompt == null)
			{
				return;
			}
			deathPrompt.UpdateBotList();
		}

		// Token: 0x06000E5E RID: 3678 RVA: 0x00087DE8 File Offset: 0x00085FE8
		public void AddCharacter(Character character)
		{
			if (character.Removed)
			{
				DebugConsole.ThrowError("Tried to add a removed character to CrewManager!\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			if (character.IsDead)
			{
				DebugConsole.ThrowError("Tried to add a dead character to CrewManager!\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			if (character.Info == null)
			{
				if (character.Prefab.ContentPackage == GameMain.VanillaContent)
				{
					DebugConsole.ThrowError("Added a character with no CharacterInfo to the crew." + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				}
				else
				{
					DebugConsole.ThrowError("Added add a character with no CharacterInfo to the crew. This may lead to issues: consider adding HasCharacterInfo=\"True\" to the character config.", null, null, false, false);
				}
			}
			if (!this.characters.Contains(character))
			{
				this.characters.Add(character);
			}
			if (!this.characterInfos.Contains(character.Info))
			{
				this.characterInfos.Add(character.Info);
			}
			GUIComponent characterComponent = this.AddCharacterToCrewList(character);
			if (character.CurrentOrders != null)
			{
				foreach (Order order in character.CurrentOrders)
				{
					this.AddCurrentOrderIcon(character, order);
				}
			}
			HumanAIController humanAI = character.AIController as HumanAIController;
			if (humanAI != null)
			{
				AIObjectiveIdle idleObjective = humanAI.ObjectiveManager.GetObjective<AIObjectiveIdle>();
				if (idleObjective != null)
				{
					idleObjective.Behavior = character.Info.Job.Prefab.IdleBehavior;
				}
			}
		}

		// Token: 0x06000E5F RID: 3679 RVA: 0x00087F5C File Offset: 0x0008615C
		public bool IsFired(Character character)
		{
			return !this.GetCharacterInfos(false).Contains(character.Info);
		}

		// Token: 0x06000E60 RID: 3680 RVA: 0x00087F74 File Offset: 0x00086174
		public void RemoveCharacter(Character character, bool removeInfo = false, bool resetCrewListIndex = true)
		{
			if (character == null)
			{
				DebugConsole.ThrowError("Tried to remove a null character from CrewManager.\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			this.characters.Remove(character);
			if (removeInfo)
			{
				this.characterInfos.Remove(character.Info);
				this.RemoveCharacterFromCrewList(character);
			}
			if (resetCrewListIndex)
			{
				this.ResetCrewListIndex(character);
			}
		}

		// Token: 0x06000E61 RID: 3681 RVA: 0x00087FD8 File Offset: 0x000861D8
		public void AddCharacterInfo(CharacterInfo characterInfo)
		{
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.Campaign : null) is MultiPlayerCampaign && characterInfo.BotStatus != BotStatus.ActiveService)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(72, 2);
				defaultInterpolatedStringHandler.AppendLiteral("CrewManager.AddCharacterInfo called on a bot (");
				defaultInterpolatedStringHandler.AppendFormatted(characterInfo.DisplayName);
				defaultInterpolatedStringHandler.AppendLiteral(") with the wrong status (");
				defaultInterpolatedStringHandler.AppendFormatted<BotStatus>(characterInfo.BotStatus);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
			}
			if (this.characterInfos.Contains(characterInfo))
			{
				DebugConsole.ThrowError("Tried to add the same character info to CrewManager twice.\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			this.characterInfos.Add(characterInfo);
			GameSession gameSession2 = GameMain.GameSession;
			if (gameSession2 == null)
			{
				return;
			}
			DeathPrompt deathPrompt = gameSession2.DeathPrompt;
			if (deathPrompt == null)
			{
				return;
			}
			deathPrompt.UpdateBotList();
		}

		// Token: 0x06000E62 RID: 3682 RVA: 0x000880AF File Offset: 0x000862AF
		public void ClearCharacterInfos()
		{
			this.characterInfos.Clear();
			this.reserveBench.Clear();
		}

		// Token: 0x06000E63 RID: 3683 RVA: 0x000880C8 File Offset: 0x000862C8
		public void InitRound()
		{
			GUIContextMenu.CurrentContextMenu = null;
			this.characters.Clear();
			List<WayPoint> spawnWaypoints = null;
			List<WayPoint> mainSubWaypoints = WayPoint.SelectCrewSpawnPoints(this.characterInfos, Submarine.MainSub).ToList<WayPoint>();
			if (Level.Loaded != null && Level.Loaded.ShouldSpawnCrewInsideOutpost())
			{
				spawnWaypoints = this.GetOutpostSpawnpoints();
				while (spawnWaypoints.Count > this.characterInfos.Count)
				{
					spawnWaypoints.RemoveAt(Rand.Int(spawnWaypoints.Count, Rand.RandSync.Unsynced));
				}
				while (spawnWaypoints.Any<WayPoint>() && spawnWaypoints.Count < this.characterInfos.Count)
				{
					spawnWaypoints.Add(spawnWaypoints[Rand.Int(spawnWaypoints.Count, Rand.RandSync.Unsynced)]);
				}
			}
			if (spawnWaypoints == null || !spawnWaypoints.Any<WayPoint>())
			{
				spawnWaypoints = mainSubWaypoints;
			}
			for (int i = 0; i < spawnWaypoints.Count; i++)
			{
				CharacterInfo info = this.characterInfos[i];
				info.TeamID = CharacterTeamType.Team1;
				Character character = Character.Create(info, spawnWaypoints[i].WorldPosition, info.Name, 0, false, true, null, true);
				this.InitializeCharacter(character, mainSubWaypoints[i], spawnWaypoints[i]);
				this.AddCharacter(character);
				if (this.IsSinglePlayer && (Character.Controlled == null || character.Info.LastControlled))
				{
					Character.Controlled = character;
				}
			}
			if (this.IsSinglePlayer)
			{
				this.SortCrewList();
			}
			this.conversationTimer = (this.IsSinglePlayer ? Rand.Range(5f, 10f, Rand.RandSync.Unsynced) : Rand.Range(45f, 60f, Rand.RandSync.Unsynced));
		}

		// Token: 0x06000E64 RID: 3684 RVA: 0x00088248 File Offset: 0x00086448
		public List<WayPoint> GetOutpostSpawnpoints()
		{
			return WayPoint.WayPointList.FindAll((WayPoint wp) => wp.SpawnType == SpawnType.Human && wp.Submarine == Level.Loaded.StartOutpost && wp.CurrentHull != null && wp.CurrentHull.OutpostModuleTags.Contains("airlock".ToIdentifier()));
		}

		// Token: 0x06000E65 RID: 3685 RVA: 0x00088274 File Offset: 0x00086474
		public void InitializeCharacter(Character character, WayPoint mainSubWaypoint, WayPoint spawnWaypoint)
		{
			if (character.Info != null)
			{
				if (!character.Info.StartItemsGiven && character.Info.InventoryData != null)
				{
					DebugConsole.AddWarning("Error when initializing a round: character \"" + character.Name + "\" has not been given their initial items but has saved inventory data. Using the saved inventory data instead of giving the character new items.", null);
				}
				if (character.Info.InventoryData != null)
				{
					character.SpawnInventoryItems(character.Inventory, character.Info.InventoryData.FromPackage(null));
				}
				else if (!character.Info.StartItemsGiven)
				{
					GameSession gameSession = GameMain.GameSession;
					character.GiveJobItems(((gameSession != null) ? gameSession.GameMode : null) is PvPMode, mainSubWaypoint);
					foreach (Item item in character.Inventory.AllItems)
					{
						IdCard idCard = item.GetComponent<IdCard>();
						if (idCard != null)
						{
							idCard.SubmarineSpecificID = 0;
						}
					}
				}
				if (character.Info.HealthData != null)
				{
					CharacterInfo.ApplyHealthData(character, character.Info.HealthData, null);
				}
				character.LoadTalents();
				character.GiveIdCardTags(mainSubWaypoint, false);
				character.GiveIdCardTags(spawnWaypoint, false);
				character.Info.StartItemsGiven = true;
				if (character.Info.OrderData != null)
				{
					character.Info.ApplyOrderData();
				}
			}
		}

		// Token: 0x06000E66 RID: 3686 RVA: 0x000883C4 File Offset: 0x000865C4
		public void RenameCharacter(CharacterInfo characterInfo, string newName)
		{
			characterInfo.Rename(newName);
			this.RenameCharacterProjSpecific(characterInfo);
		}

		// Token: 0x06000E67 RID: 3687 RVA: 0x000883D4 File Offset: 0x000865D4
		private void RenameCharacterProjSpecific(CharacterInfo characterInfo)
		{
			GUIComponent characterComponent = this.crewList.Content.GetChildByUserData((characterInfo != null) ? characterInfo.Character : null);
			if (characterComponent == null)
			{
				return;
			}
			CrewManager.SetCharacterComponentTooltip(characterComponent);
			GUITextBlock nameBlock = characterComponent.FindChild("name", true) as GUITextBlock;
			if (nameBlock == null)
			{
				return;
			}
			nameBlock.Text = ToolBox.LimitString(characterInfo.Name, nameBlock.Font, nameBlock.Rect.Width);
		}

		// Token: 0x06000E68 RID: 3688 RVA: 0x00088445 File Offset: 0x00086645
		public void FireCharacter(CharacterInfo characterInfo)
		{
			this.RemoveCharacterInfo(characterInfo);
		}

		// Token: 0x06000E69 RID: 3689 RVA: 0x00088450 File Offset: 0x00086650
		public void ClearCurrentOrders()
		{
			foreach (CharacterInfo characterInfo in this.characterInfos)
			{
				if (characterInfo != null)
				{
					characterInfo.ClearCurrentOrders();
				}
			}
		}

		// Token: 0x06000E6A RID: 3690 RVA: 0x000884A8 File Offset: 0x000866A8
		public void Update(float deltaTime)
		{
			foreach (CrewManager.ActiveOrder order in this.ActiveOrders)
			{
				if (order.FadeOutTime != null)
				{
					order.FadeOutTime -= deltaTime;
				}
			}
			this.ActiveOrders.RemoveAll(delegate(CrewManager.ActiveOrder o)
			{
				if (o.FadeOutTime != null)
				{
					float? fadeOutTime = o.FadeOutTime;
					float num = 0f;
					if (fadeOutTime.GetValueOrDefault() <= num & fadeOutTime != null)
					{
						return true;
					}
				}
				return o.Order.TargetEntity != null && o.Order.TargetEntity.Removed;
			});
			this.UpdateConversations(deltaTime);
			this.UpdateProjectSpecific(deltaTime);
			ReadyCheck activeReadyCheck = this.ActiveReadyCheck;
			if (activeReadyCheck != null)
			{
				activeReadyCheck.Update(deltaTime);
			}
			if (this.ActiveReadyCheck != null && this.ActiveReadyCheck.IsFinished)
			{
				this.ActiveReadyCheck = null;
			}
		}

		// Token: 0x06000E6B RID: 3691 RVA: 0x0008859C File Offset: 0x0008679C
		public void AddConversation([TupleElementNames(new string[]
		{
			"speaker",
			"line"
		})] List<ValueTuple<Character, string>> conversationLines)
		{
			if (conversationLines == null || conversationLines.Count == 0)
			{
				return;
			}
			this.pendingConversationLines.AddRange(conversationLines);
		}

		// Token: 0x06000E6C RID: 3692 RVA: 0x000885B8 File Offset: 0x000867B8
		private void CreateRandomConversation()
		{
			if (GameMain.Client != null)
			{
				return;
			}
			List<Character> availableSpeakers = Character.CharacterList.FindAll(delegate(Character c)
			{
				if (c.AIController is HumanAIController && !c.IsDead && c.SpeechImpediment <= 100f)
				{
					return c.CharacterHealth.GetAllAfflictions(delegate(Affliction a)
					{
						AfflictionHusk huskInfection = a as AfflictionHusk;
						if (huskInfection != null)
						{
							AfflictionPrefabHusk afflictionPrefabHusk = huskInfection.Prefab as AfflictionPrefabHusk;
							return afflictionPrefabHusk != null && afflictionPrefabHusk.CauseSpeechImpediment;
						}
						return false;
					}).None(null);
				}
				return false;
			});
			this.pendingConversationLines.AddRange(NPCConversation.CreateRandom(availableSpeakers));
		}

		// Token: 0x06000E6D RID: 3693 RVA: 0x00088608 File Offset: 0x00086808
		private void UpdateConversations(float deltaTime)
		{
			GameSession gameSession = GameMain.GameSession;
			GameModePreset gameModePreset;
			if (gameSession == null)
			{
				gameModePreset = null;
			}
			else
			{
				GameMode gameMode = gameSession.GameMode;
				gameModePreset = ((gameMode != null) ? gameMode.Preset : null);
			}
			if (gameModePreset == GameModePreset.TestMode)
			{
				return;
			}
			GameSession gameSession2 = GameMain.GameSession;
			TutorialMode tutorialMode = ((gameSession2 != null) ? gameSession2.GameMode : null) as TutorialMode;
			if (tutorialMode != null)
			{
				Tutorial tutorial = tutorialMode.Tutorial;
				if (tutorial != null && tutorial.TutorialPrefab.DisableBotConversations)
				{
					return;
				}
			}
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.ServerSettings.DisableBotConversations)
			{
				return;
			}
			this.conversationTimer -= deltaTime;
			if (this.conversationTimer <= 0f)
			{
				this.CreateRandomConversation();
				this.conversationTimer = Rand.Range(100f, 180f, Rand.RandSync.Unsynced);
				if (GameMain.NetworkMember != null)
				{
					this.conversationTimer *= 5f;
				}
			}
			if (this.welcomeMessageNPC == null)
			{
				using (List<Character>.Enumerator enumerator = Character.CharacterList.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Character npc = enumerator.Current;
						if ((npc.TeamID == CharacterTeamType.FriendlyNPC || npc.TeamID == CharacterTeamType.None) && npc.CurrentHull != null && !npc.IsIncapacitated)
						{
							HumanAIController humanAI = npc.AIController as HumanAIController;
							if (humanAI == null || (!humanAI.ObjectiveManager.IsCurrentObjective<AIObjectiveFindSafety>() && !humanAI.ObjectiveManager.IsCurrentObjective<AIObjectiveCombat>()))
							{
								foreach (Character player in Character.CharacterList)
								{
									if (player.TeamID != npc.TeamID && !player.IsIncapacitated && player.CurrentHull == npc.CurrentHull)
									{
										List<Character> availableSpeakers = new List<Character>
										{
											npc,
											player
										};
										List<Identifier> dialogFlags = new List<Identifier>
										{
											"OutpostNPC".ToIdentifier(),
											"EnterOutpost".ToIdentifier()
										};
										if (npc.HumanPrefab != null)
										{
											foreach (Identifier tag in npc.HumanPrefab.GetTags())
											{
												dialogFlags.Add(tag);
											}
										}
										GameSession gameSession3 = GameMain.GameSession;
										CampaignMode campaignMode = ((gameSession3 != null) ? gameSession3.GameMode : null) as CampaignMode;
										if (campaignMode != null)
										{
											Map map = campaignMode.Map;
											Identifier? identifier;
											Identifier? identifier2;
											if (map == null)
											{
												identifier = null;
												identifier2 = identifier;
											}
											else
											{
												Location currentLocation = map.CurrentLocation;
												if (currentLocation == null)
												{
													identifier = null;
													identifier2 = identifier;
												}
												else
												{
													LocationType type = currentLocation.Type;
													if (type == null)
													{
														identifier = null;
														identifier2 = identifier;
													}
													else
													{
														identifier2 = new Identifier?(type.Identifier);
													}
												}
											}
											identifier = identifier2;
											if (identifier == "abandoned")
											{
												dialogFlags.Remove("OutpostNPC".ToIdentifier());
											}
											else
											{
												Map map2 = campaignMode.Map;
												bool flag;
												if (map2 == null)
												{
													flag = (null != null);
												}
												else
												{
													Location currentLocation2 = map2.CurrentLocation;
													flag = (((currentLocation2 != null) ? currentLocation2.Reputation : null) != null);
												}
												if (flag)
												{
													float normalizedReputation = MathUtils.InverseLerp((float)campaignMode.Map.CurrentLocation.Reputation.MinReputation, (float)campaignMode.Map.CurrentLocation.Reputation.MaxReputation, campaignMode.Map.CurrentLocation.Reputation.Value);
													if (normalizedReputation < 0.2f)
													{
														dialogFlags.Add("LowReputation".ToIdentifier());
													}
													else if (normalizedReputation > 0.8f)
													{
														dialogFlags.Add("HighReputation".ToIdentifier());
													}
												}
											}
										}
										this.pendingConversationLines.AddRange(NPCConversation.CreateRandom(availableSpeakers, dialogFlags));
										this.welcomeMessageNPC = npc;
										break;
									}
								}
								if (this.welcomeMessageNPC != null)
								{
									break;
								}
							}
						}
					}
					goto IL_3B5;
				}
			}
			if (this.welcomeMessageNPC.Removed)
			{
				this.welcomeMessageNPC = null;
			}
			IL_3B5:
			if (this.pendingConversationLines.Count > 0)
			{
				this.conversationLineTimer -= deltaTime;
				if (this.conversationLineTimer <= 0f)
				{
					if (this.pendingConversationLines[0].Item1.SpeechImpediment >= 100f)
					{
						this.pendingConversationLines.Clear();
						return;
					}
					this.pendingConversationLines[0].Item1.Speak(this.pendingConversationLines[0].Item2, null, 0f, default(Identifier), 0f);
					if (this.pendingConversationLines.Count > 1)
					{
						this.conversationLineTimer = MathHelper.Clamp((float)this.pendingConversationLines[0].Item2.Length * 0.1f, 1f, 5f);
					}
					this.pendingConversationLines.RemoveAt(0);
				}
			}
		}

		// Token: 0x06000E6E RID: 3694 RVA: 0x00088B00 File Offset: 0x00086D00
		public static Character GetCharacterForQuickAssignment(Order order, Character controlledCharacter, IEnumerable<Character> characters, bool includeSelf = false)
		{
			bool isControlledCharacterNull = controlledCharacter == null;
			if (isControlledCharacterNull)
			{
				return null;
			}
			Character operatingCharacter;
			if (order.Category.GetValueOrDefault() == OrderCategory.Operate && HumanAIController.IsItemTargetedBySomeone(order.TargetItemComponent, (controlledCharacter != null) ? controlledCharacter.TeamID : CharacterTeamType.Team1, out operatingCharacter) && (isControlledCharacterNull || operatingCharacter.CanHearCharacter(controlledCharacter)))
			{
				return operatingCharacter;
			}
			return CrewManager.GetCharactersSortedForOrder(order, characters, controlledCharacter, includeSelf, null).FirstOrDefault((Character c) => isControlledCharacterNull || c.CanHearCharacter(controlledCharacter)) ?? controlledCharacter;
		}

		// Token: 0x06000E6F RID: 3695 RVA: 0x00088BAC File Offset: 0x00086DAC
		public static IEnumerable<Character> GetCharactersSortedForOrder(Order order, IEnumerable<Character> characters, Character controlledCharacter, bool includeSelf, IEnumerable<Character> extraCharacters = null)
		{
			IEnumerable<Character> filteredCharacters = from c in characters
			where c.Info != null && (controlledCharacter == null || ((includeSelf || c != controlledCharacter) && c.TeamID == controlledCharacter.TeamID))
			select c;
			if (extraCharacters != null)
			{
				filteredCharacters = filteredCharacters.Union(extraCharacters);
			}
			Func<Order, bool> <>9__7;
			Func<Order, bool> <>9__8;
			return (from c in filteredCharacters
			orderby Character.Controlled == null || c.Submarine == Character.Controlled.Submarine descending
			select c).ThenByDescending(delegate(Character c)
			{
				if (order.Category.GetValueOrDefault() == OrderCategory.Operate)
				{
					IEnumerable<Order> currentOrders = c.CurrentOrders;
					Func<Order, bool> predicate;
					if ((predicate = <>9__7) == null)
					{
						predicate = (<>9__7 = delegate(Order o)
						{
							if (o != null)
							{
								Identifier identifier = o.Identifier;
								Identifier identifier2 = order.Identifier;
								if (identifier == identifier2)
								{
									return o.TargetEntity == order.TargetEntity;
								}
							}
							return false;
						});
					}
					return currentOrders.Any(predicate);
				}
				return false;
			}).ThenByDescending(new Func<Character, bool>(order.HasAppropriateJob)).ThenByDescending(delegate(Character c)
			{
				IEnumerable<Order> currentOrders = c.CurrentOrders;
				Func<Order, bool> predicate;
				if ((predicate = <>9__8) == null)
				{
					predicate = (<>9__8 = delegate(Order o)
					{
						if (o != null)
						{
							Identifier identifier = o.Identifier;
							Identifier identifier2 = order.Identifier;
							return identifier == identifier2;
						}
						return false;
					});
				}
				return currentOrders.None(predicate);
			}).ThenByDescending(new Func<Character, bool>(order.HasPreferredJob)).ThenByDescending((Character c) => c.IsBot).ThenBy(delegate(Character c)
			{
				HumanAIController humanAI = c.AIController as HumanAIController;
				if (humanAI == null)
				{
					return new float?(0f);
				}
				AIObjective currentObjective = humanAI.ObjectiveManager.CurrentObjective;
				if (currentObjective == null)
				{
					return null;
				}
				return new float?(currentObjective.Priority);
			}).ThenByDescending((Character c) => c.GetSkillLevel(order.AppropriateSkill));
		}

		// Token: 0x06000E70 RID: 3696 RVA: 0x00088CC0 File Offset: 0x00086EC0
		private void UpdateProjectSpecific(float deltaTime)
		{
			if (GameMain.IsSingleplayer && GUI.KeyboardDispatcher.Subscriber == null)
			{
				if (PlayerInput.KeyHit(InputType.SelectNextCharacter))
				{
					this.SelectNextCharacter();
				}
				if (PlayerInput.KeyHit(InputType.SelectPreviousCharacter))
				{
					this.SelectPreviousCharacter();
				}
			}
			if (GUI.DisableHUD)
			{
				return;
			}
			this.UpdateOrderDrag();
			this.WasCommandInterfaceDisabledThisUpdate = false;
			if (PlayerInput.KeyDown(InputType.Command))
			{
				if (GUI.KeyboardDispatcher.Subscriber != null)
				{
					GUIComponent component = GUI.KeyboardDispatcher.Subscriber as GUIComponent;
					if (component == null || (component != this.crewList && !component.IsChildOf(this.crewList, true)))
					{
						goto IL_177;
					}
				}
				if (this.commandFrame == null && !this.clicklessSelectionActive && CrewManager.CanIssueOrders)
				{
					GameSession gameSession = GameMain.GameSession;
					bool? flag;
					if (gameSession == null)
					{
						flag = null;
					}
					else
					{
						CampaignMode campaign = gameSession.Campaign;
						flag = ((campaign != null) ? new bool?(campaign.ShowCampaignUI) : null);
					}
					bool? flag2 = flag;
					if (!flag2.GetValueOrDefault())
					{
						Character controlled = Character.Controlled;
						ItemPrefab itemPrefab;
						if (controlled == null)
						{
							itemPrefab = null;
						}
						else
						{
							Item selectedItem = controlled.SelectedItem;
							itemPrefab = ((selectedItem != null) ? selectedItem.Prefab : null);
						}
						ItemPrefab itemPrefab2 = itemPrefab;
						if ((itemPrefab2 == null || !itemPrefab2.DisableCommandMenuWhenSelected) && !Inventory.IsMouseOnInventory)
						{
							if (PlayerInput.KeyDown(InputType.ContextualCommand))
							{
								this.CreateCommandUI(this.FindEntityContext(), true);
							}
							else
							{
								Entity entityContext;
								if (!CharacterHUD.MouseOnCharacterPortrait())
								{
									GUIComponent mouseOn = GUI.MouseOn;
									entityContext = (((mouseOn != null) ? mouseOn.UserData : null) as Character);
								}
								else
								{
									entityContext = Character.Controlled;
								}
								this.CreateCommandUI(entityContext, false);
							}
							SoundPlayer.PlayUISound(GUISoundType.PopupMenu);
							this.clicklessSelectionActive = (this.isOpeningClick = true);
						}
					}
				}
			}
			IL_177:
			if (this.commandFrame != null)
			{
				bool isMouseOnOptionNode = this.optionNodes.Any((CrewManager.OptionNode n) => GUI.IsMouseOn(n.Button));
				bool flag3;
				if (!isMouseOnOptionNode)
				{
					flag3 = this.shortcutNodes.Any((GUIComponent n) => GUI.IsMouseOn(n));
				}
				else
				{
					flag3 = false;
				}
				bool isMouseOnShortcutNode = flag3;
				bool hitDeselect = PlayerInput.KeyHit(InputType.Deselect) && (!PlayerInput.SecondaryMouseButtonClicked() || (!isMouseOnOptionNode && !isMouseOnShortcutNode));
				bool isBoundToPrimaryMouse = GameSettings.CurrentConfig.KeyMap.Bindings[InputType.Command].MouseButton == MouseButton.PrimaryMouse;
				bool flag4;
				if (isBoundToPrimaryMouse)
				{
					if (!isMouseOnOptionNode && !isMouseOnShortcutNode)
					{
						if (this.extraOptionNodes.None((GUIComponent n) => GUI.IsMouseOn(n)))
						{
							flag4 = !GUI.IsMouseOn(this.returnNode);
							goto IL_271;
						}
					}
					flag4 = false;
				}
				else
				{
					flag4 = true;
				}
				IL_271:
				bool canToggleInterface = flag4;
				if (hitDeselect || PlayerInput.KeyHit(Keys.Escape) || !CrewManager.CanIssueOrders || (canToggleInterface && PlayerInput.KeyHit(InputType.Command) && this.selectedNode == null && !this.clicklessSelectionActive))
				{
					this.DisableCommandUI();
				}
				else if (PlayerInput.KeyUp(InputType.Command))
				{
					if (canToggleInterface && !this.isOpeningClick && this.clicklessSelectionActive && this.timeSelected < 0.15f)
					{
						this.DisableCommandUI();
					}
					else
					{
						this.clicklessSelectionActive = (this.isOpeningClick = false);
						if (this.selectedNode != null)
						{
							this.<UpdateProjectSpecific>g__ResetNodeSelection|256_0(null);
						}
					}
				}
				else if (PlayerInput.KeyDown(InputType.Command) && (this.targetFrame == null || !this.targetFrame.Visible))
				{
					if (!GUI.IsMouseOn(this.centerNode))
					{
						CrewManager.<>c__DisplayClass256_0 CS$<>8__locals1 = new CrewManager.<>c__DisplayClass256_0();
						CS$<>8__locals1.<>4__this = this;
						this.clicklessSelectionActive = true;
						CS$<>8__locals1.mouseBearing = this.GetBearing(this.centerNode.Center, PlayerInput.MousePosition, true, false);
						CS$<>8__locals1.closestNode = null;
						CS$<>8__locals1.closestBearing = 0f;
						this.optionNodes.ForEach(delegate(CrewManager.OptionNode n)
						{
							base.<UpdateProjectSpecific>g__CheckIfClosest|6(n.Button);
						});
						CS$<>8__locals1.<UpdateProjectSpecific>g__CheckIfClosest|6(this.returnNode);
						if (CS$<>8__locals1.closestNode != null && CS$<>8__locals1.closestNode.CanBeFocused && CS$<>8__locals1.closestNode == this.selectedNode)
						{
							this.timeSelected += deltaTime;
							if (this.timeSelected >= this.selectionTime)
							{
								if (PlayerInput.IsShiftDown() && this.selectedNode.OnSecondaryClicked != null)
								{
									this.selectedNode.OnSecondaryClicked(this.selectedNode, this.selectedNode.UserData);
								}
								else
								{
									GUIButton.OnClickedHandler onClicked = this.selectedNode.OnClicked;
									if (onClicked != null)
									{
										onClicked(this.selectedNode, this.selectedNode.UserData);
									}
								}
								this.<UpdateProjectSpecific>g__ResetNodeSelection|256_0(null);
							}
							else if (this.timeSelected >= 0.15f && !this.isSelectionHighlighted)
							{
								this.selectedNode.Children.ForEach(delegate(GUIComponent c)
								{
									c.Color = c.HoverColor;
								});
								this.isSelectionHighlighted = true;
							}
						}
						else
						{
							this.<UpdateProjectSpecific>g__ResetNodeSelection|256_0(CS$<>8__locals1.closestNode as GUIButton);
						}
					}
					else if (this.selectedNode != null)
					{
						this.<UpdateProjectSpecific>g__ResetNodeSelection|256_0(null);
					}
				}
				bool hotkeyHit = false;
				foreach (CrewManager.OptionNode node in this.optionNodes)
				{
					if (node.Keys != Keys.None && PlayerInput.KeyHit(node.Keys))
					{
						GUIButton button = node.Button;
						if (PlayerInput.IsShiftDown() && button != null && button.OnSecondaryClicked != null)
						{
							button.OnSecondaryClicked(button, button.UserData);
						}
						else if (button != null)
						{
							GUIButton.OnClickedHandler onClicked2 = button.OnClicked;
							if (onClicked2 != null)
							{
								onClicked2(button, button.UserData);
							}
						}
						this.<UpdateProjectSpecific>g__ResetNodeSelection|256_0(null);
						hotkeyHit = true;
						break;
					}
				}
				if (!hotkeyHit)
				{
					if (this.returnNodeHotkey != Keys.None && PlayerInput.KeyHit(this.returnNodeHotkey))
					{
						GUIButton guibutton = this.returnNode;
						if (guibutton != null)
						{
							GUIButton.OnClickedHandler onClicked3 = guibutton.OnClicked;
							if (onClicked3 != null)
							{
								onClicked3(this.returnNode, this.returnNode.UserData);
							}
						}
						this.<UpdateProjectSpecific>g__ResetNodeSelection|256_0(null);
					}
					else if (this.expandNodeHotkey != Keys.None && PlayerInput.KeyHit(this.expandNodeHotkey))
					{
						GUIButton guibutton2 = this.expandNode;
						if (guibutton2 != null)
						{
							GUIButton.OnClickedHandler onClicked4 = guibutton2.OnClicked;
							if (onClicked4 != null)
							{
								onClicked4(this.expandNode, this.expandNode.UserData);
							}
						}
						this.<UpdateProjectSpecific>g__ResetNodeSelection|256_0(null);
					}
				}
			}
			else if (!PlayerInput.KeyDown(InputType.Command))
			{
				this.clicklessSelectionActive = false;
			}
			if (this.ChatBox != null)
			{
				this.ChatBox.Update(deltaTime);
				this.ChatBox.InputBox.Visible = (Character.Controlled != null);
				if (!DebugConsole.IsOpen && this.ChatBox.InputBox.Visible && GUI.KeyboardDispatcher.Subscriber == null && !this.ChatBox.InputBox.Selected)
				{
					this.ChatBox.ApplySelectionInputs();
				}
			}
			if (!GUI.DisableUpperHUD)
			{
				this.crewArea.Visible = (this.characters.Count > 0 && CharacterHealth.OpenHealthWindow == null);
				Character controlled2 = Character.Controlled;
				CharacterTeamType characterTeamType3;
				if (controlled2 == null)
				{
					GameClient client = GameMain.Client;
					CharacterTeamType? characterTeamType;
					if (client == null)
					{
						characterTeamType = null;
					}
					else
					{
						Client myClient = client.MyClient;
						characterTeamType = ((myClient != null) ? new CharacterTeamType?(myClient.TeamID) : null);
					}
					CharacterTeamType? characterTeamType2 = characterTeamType;
					characterTeamType3 = characterTeamType2.GetValueOrDefault(CharacterTeamType.Team1);
				}
				else
				{
					characterTeamType3 = controlled2.TeamID;
				}
				CharacterTeamType myTeam = characterTeamType3;
				GameSession gameSession2 = GameMain.GameSession;
				if (((gameSession2 != null) ? gameSession2.GameMode : null) is PvPMode)
				{
					GUIComponent team1Text = this.crewArea.GetChildByUserData(CharacterTeamType.Team1);
					team1Text.Visible = (myTeam == CharacterTeamType.Team1);
					team1Text.IgnoreLayoutGroups = !team1Text.Visible;
					GUIComponent team2Text = this.crewArea.GetChildByUserData(CharacterTeamType.Team2);
					team2Text.Visible = (myTeam == CharacterTeamType.Team2);
					team2Text.IgnoreLayoutGroups = !team2Text.Visible;
				}
				foreach (GUIComponent characterComponent in this.crewList.Content.Children)
				{
					Character character = characterComponent.UserData as Character;
					if (character != null)
					{
						if (character.Removed)
						{
							characterComponent.Visible = false;
						}
						else
						{
							characterComponent.Visible = (myTeam == character.TeamID);
							if (character.TeamID == CharacterTeamType.FriendlyNPC && Character.Controlled != null && (character.CurrentHull == Character.Controlled.CurrentHull || Vector2.DistanceSquared(Character.Controlled.WorldPosition, character.WorldPosition) < 250000f))
							{
								characterComponent.Visible = true;
							}
							if (characterComponent.Visible)
							{
								if (character == Character.Controlled && this.crewList.SelectedComponent != characterComponent)
								{
									this.crewList.Select(character, GUIListBox.Force.Yes, GUIListBox.AutoScroll.Enabled);
								}
								GUIListBox currentOrderIconList = this.GetCurrentOrderIconList(characterComponent);
								if (currentOrderIconList != null)
								{
									foreach (GUIComponent orderIcon in currentOrderIconList.Content.Children)
									{
										Order order = orderIcon.UserData as Order;
										if (order != null)
										{
											if (order.ColoredWhenControllingGiver && order.OrderGiver != Character.Controlled)
											{
												orderIcon.Color = AIObjective.ObjectiveIconColor;
											}
											else
											{
												orderIcon.Color = order.Color;
											}
										}
									}
								}
								if (GameMain.IsSingleplayer && character.IsBot)
								{
									HumanAIController controller = character.AIController as HumanAIController;
									if (controller != null)
									{
										AIObjectiveManager objectiveManager = controller.ObjectiveManager;
										if (objectiveManager != null)
										{
											AIObjective currentObjective = objectiveManager.CurrentObjective;
											if (currentObjective != null)
											{
												if (objectiveManager.IsOrder(currentObjective))
												{
													Order orderInfo = objectiveManager.CurrentOrders.FirstOrDefault((Order o) => o.Objective == currentObjective);
													if (orderInfo != null)
													{
														this.SetOrderHighlight(characterComponent, orderInfo.Identifier, orderInfo.Option);
													}
												}
												else
												{
													this.CreateObjectiveIcon(characterComponent, currentObjective);
												}
											}
										}
									}
								}
								if (character.IsPlayer)
								{
									this.DisableOrderHighlight(characterComponent);
									this.RemoveObjectiveIcon(characterComponent);
								}
								GUIComponent soundIconParent = this.GetSoundIconParent(characterComponent);
								if (soundIconParent != null)
								{
									GUIImage soundIcon = soundIconParent.FindChild(delegate(GUIComponent c)
									{
										Pair<string, float> pair = c.UserData as Pair<string, float>;
										return pair != null && pair.First == "soundicon";
									}, false) as GUIImage;
									if (soundIcon != null)
									{
										if (character.IsPlayer)
										{
											soundIconParent.Visible = true;
											VoipClient.UpdateVoiceIndicator(soundIcon, 0f, deltaTime);
										}
										else if (soundIcon.Visible)
										{
											Pair<string, float> userdata = soundIcon.UserData as Pair<string, float>;
											userdata.Second = 0f;
											soundIconParent.Visible = (soundIcon.Visible = false);
										}
									}
								}
							}
						}
					}
				}
				this.traitorButtons.ForEach(delegate(GUITickBox btn)
				{
					Character controlled3 = Character.Controlled;
					btn.Visible = (controlled3 != null && !controlled3.IsDead && btn.UserData as Character != Character.Controlled);
				});
				this.crewArea.RectTransform.AbsoluteOffset = Vector2.SmoothStep(new Vector2((float)(-(float)this.crewArea.Rect.Width - HUDLayoutSettings.Padding), 0f), Vector2.Zero, this.crewListOpenState).ToPoint();
				this.crewListOpenState = (this.IsCrewMenuOpen ? Math.Min(this.crewListOpenState + deltaTime * 2f, 1f) : Math.Max(this.crewListOpenState - deltaTime * 2f, 0f));
				if (GUI.KeyboardDispatcher.Subscriber == null && PlayerInput.KeyHit(InputType.CrewOrders))
				{
					SoundPlayer.PlayUISound(GUISoundType.PopupMenu);
					this.IsCrewMenuOpen = !this.IsCrewMenuOpen;
				}
			}
			this.UpdateReports();
		}

		// Token: 0x06000E71 RID: 3697 RVA: 0x00089874 File Offset: 0x00087A74
		public void SaveActiveOrders(XElement element)
		{
			List<Order> ordersToSave = new List<Order>();
			foreach (CrewManager.ActiveOrder activeOrder in this.ActiveOrders)
			{
				Order order = (activeOrder != null) ? activeOrder.Order : null;
				if (order != null && activeOrder.FadeOutTime == null)
				{
					ordersToSave.Add(order.WithManualPriority(CharacterInfo.HighestManualOrderPriority));
				}
			}
			CharacterInfo.SaveOrders(element, ordersToSave.ToArray());
		}

		// Token: 0x06000E72 RID: 3698 RVA: 0x00089900 File Offset: 0x00087B00
		public void LoadActiveOrders(XElement element)
		{
			if (element == null)
			{
				return;
			}
			foreach (Order orderInfo in CharacterInfo.LoadOrders(element))
			{
				IIgnorable ignoreTarget = null;
				if (orderInfo.IsIgnoreOrder)
				{
					Order.OrderTargetType targetType = orderInfo.TargetType;
					if (targetType != Order.OrderTargetType.Entity)
					{
						if (targetType == Order.OrderTargetType.WallSection)
						{
							Structure s = orderInfo.TargetEntity as Structure;
							if (s != null && orderInfo.WallSectionIndex != null)
							{
								ignoreTarget = s.GetSection(orderInfo.WallSectionIndex.Value);
								goto IL_88;
							}
						}
						DebugConsole.ThrowError("Error loading an ignore order - can't find a proper ignore target", null, null, false, false);
						continue;
					}
					ignoreTarget = (orderInfo.TargetEntity as IIgnorable);
				}
				IL_88:
				if (orderInfo.TargetEntity != null && (!orderInfo.IsIgnoreOrder || ignoreTarget != null))
				{
					if (ignoreTarget != null)
					{
						ignoreTarget.OrderedToBeIgnored = true;
					}
					this.AddOrder(orderInfo, null);
				}
			}
		}

		// Token: 0x06000E77 RID: 3703 RVA: 0x00089A27 File Offset: 0x00087C27
		[CompilerGenerated]
		internal static bool <CreateShortcutNodes>g__ShouldDelegateOrder|165_9(string orderIdentifier)
		{
			return CrewManager.<CreateShortcutNodes>g__ShouldDelegateOrderId|165_10(orderIdentifier.ToIdentifier());
		}

		// Token: 0x06000E78 RID: 3704 RVA: 0x00089A34 File Offset: 0x00087C34
		[CompilerGenerated]
		internal static bool <CreateShortcutNodes>g__ShouldDelegateOrderId|165_10(Identifier orderIdentifier)
		{
			Character c = Character.Controlled;
			if (c != null)
			{
				bool flag;
				if (c == null)
				{
					flag = (null != null);
				}
				else
				{
					CharacterInfo info = c.Info;
					flag = (((info != null) ? info.Job : null) != null);
				}
				return !flag || !c.Info.Job.Prefab.AppropriateOrders.Contains(orderIdentifier);
			}
			return true;
		}

		// Token: 0x06000E79 RID: 3705 RVA: 0x00089A88 File Offset: 0x00087C88
		[CompilerGenerated]
		private void <CreateContextualOrderNodes>g__AddIgnoreOrder|167_0(IIgnorable target)
		{
			CrewManager.<>c__DisplayClass167_1 CS$<>8__locals1 = new CrewManager.<>c__DisplayClass167_1();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.target = target;
			CS$<>8__locals1.orderIdentifier = Tags.IgnoreThis;
			if (!CS$<>8__locals1.target.OrderedToBeIgnored && this.contextualOrders.None(delegate(Order order)
			{
				Identifier identifier = order.Identifier;
				return identifier == CS$<>8__locals1.orderIdentifier;
			}))
			{
				CS$<>8__locals1.<CreateContextualOrderNodes>g__AddOrder|24();
				return;
			}
			CS$<>8__locals1.orderIdentifier = Tags.UnignoreThis;
			if (CS$<>8__locals1.target.OrderedToBeIgnored && this.contextualOrders.None(delegate(Order order)
			{
				Identifier identifier = order.Identifier;
				return identifier == CS$<>8__locals1.orderIdentifier;
			}))
			{
				CS$<>8__locals1.<CreateContextualOrderNodes>g__AddOrder|24();
			}
		}

		// Token: 0x06000E80 RID: 3712 RVA: 0x00089C70 File Offset: 0x00087E70
		[CompilerGenerated]
		internal static bool <AddOrder>g__MatchesTarget|235_1(Entity existingTarget, Entity newTarget)
		{
			if (existingTarget == newTarget)
			{
				return true;
			}
			Hull existingHullTarget = existingTarget as Hull;
			if (existingHullTarget != null)
			{
				Hull newHullTarget = newTarget as Hull;
				if (newHullTarget != null)
				{
					return existingHullTarget.linkedTo.Contains(newHullTarget);
				}
			}
			return false;
		}

		// Token: 0x06000E81 RID: 3713 RVA: 0x00089CA8 File Offset: 0x00087EA8
		[CompilerGenerated]
		private void <UpdateProjectSpecific>g__ResetNodeSelection|256_0(GUIButton newSelectedNode = null)
		{
			if (this.commandFrame == null)
			{
				return;
			}
			GUIButton guibutton = this.selectedNode;
			if (guibutton != null)
			{
				guibutton.Children.ForEach(delegate(GUIComponent c)
				{
					c.Color = c.HoverColor * 0.75f;
				});
			}
			this.selectedNode = newSelectedNode;
			this.timeSelected = 0f;
			this.isSelectionHighlighted = false;
		}

		// Token: 0x0400071C RID: 1820
		private Point screenResolution;

		// Token: 0x0400071D RID: 1821
		public OrderPrefab DraggedOrderPrefab;

		// Token: 0x0400071E RID: 1822
		public bool DragOrder;

		// Token: 0x0400071F RID: 1823
		private bool dropOrder;

		// Token: 0x04000720 RID: 1824
		private int framesToSkip = 2;

		// Token: 0x04000721 RID: 1825
		private float dragOrderTreshold;

		// Token: 0x04000722 RID: 1826
		private Vector2 dragPoint = Vector2.Zero;

		// Token: 0x04000724 RID: 1828
		private GUIFrame guiFrame;

		// Token: 0x04000725 RID: 1829
		private GUILayoutGroup crewArea;

		// Token: 0x04000726 RID: 1830
		private GUIListBox crewList;

		// Token: 0x04000727 RID: 1831
		private float crewListOpenState;

		// Token: 0x04000728 RID: 1832
		private bool _isCrewMenuOpen = true;

		// Token: 0x04000729 RID: 1833
		private Point crewListEntrySize;

		// Token: 0x0400072A RID: 1834
		private readonly List<GUITickBox> traitorButtons = new List<GUITickBox>();

		// Token: 0x0400072C RID: 1836
		private float prevUIScale;

		// Token: 0x0400072D RID: 1837
		public bool AllowCharacterSwitch = true;

		// Token: 0x0400072E RID: 1838
		public static bool PreferCrewMenuOpen = true;

		// Token: 0x0400072F RID: 1839
		private const float CommandNodeAnimDuration = 0.2f;

		// Token: 0x04000730 RID: 1840
		public List<GUIButton> OrderOptionButtons = new List<GUIButton>();

		// Token: 0x04000731 RID: 1841
		private Sprite jobIndicatorBackground;

		// Token: 0x04000732 RID: 1842
		private Sprite previousOrderArrow;

		// Token: 0x04000733 RID: 1843
		private Sprite cancelIcon;

		// Token: 0x04000734 RID: 1844
		private GUIFrame commandFrame;

		// Token: 0x04000735 RID: 1845
		private GUIFrame targetFrame;

		// Token: 0x04000736 RID: 1846
		private GUIButton centerNode;

		// Token: 0x04000737 RID: 1847
		private GUIButton returnNode;

		// Token: 0x04000738 RID: 1848
		private GUIButton expandNode;

		// Token: 0x04000739 RID: 1849
		private GUIFrame shortcutCenterNode;

		// Token: 0x0400073A RID: 1850
		private readonly List<CrewManager.OptionNode> optionNodes = new List<CrewManager.OptionNode>();

		// Token: 0x0400073B RID: 1851
		private Keys returnNodeHotkey;

		// Token: 0x0400073C RID: 1852
		private Keys expandNodeHotkey;

		// Token: 0x0400073D RID: 1853
		private readonly List<GUIComponent> shortcutNodes = new List<GUIComponent>();

		// Token: 0x0400073E RID: 1854
		private readonly List<GUIComponent> extraOptionNodes = new List<GUIComponent>();

		// Token: 0x0400073F RID: 1855
		private GUICustomComponent nodeConnectors;

		// Token: 0x04000740 RID: 1856
		private GUIImage background;

		// Token: 0x04000741 RID: 1857
		private GUIButton selectedNode;

		// Token: 0x04000742 RID: 1858
		private readonly float selectionTime = 0.75f;

		// Token: 0x04000743 RID: 1859
		private float timeSelected;

		// Token: 0x04000744 RID: 1860
		private bool clicklessSelectionActive;

		// Token: 0x04000745 RID: 1861
		private bool isOpeningClick;

		// Token: 0x04000746 RID: 1862
		private bool isSelectionHighlighted;

		// Token: 0x04000747 RID: 1863
		private Point centerNodeSize;

		// Token: 0x04000748 RID: 1864
		private Point nodeSize;

		// Token: 0x04000749 RID: 1865
		private Point shortcutCenterNodeSize;

		// Token: 0x0400074A RID: 1866
		private Point shortcutNodeSize;

		// Token: 0x0400074B RID: 1867
		private Point returnNodeSize;

		// Token: 0x0400074C RID: 1868
		private Point assignmentNodeSize;

		// Token: 0x0400074D RID: 1869
		private float centerNodeMargin;

		// Token: 0x0400074E RID: 1870
		private float optionNodeMargin;

		// Token: 0x0400074F RID: 1871
		private float shortcutCenterNodeMargin;

		// Token: 0x04000750 RID: 1872
		private float shortcutNodeMargin;

		// Token: 0x04000751 RID: 1873
		private float returnNodeMargin;

		// Token: 0x04000752 RID: 1874
		private List<OrderCategory> availableCategories;

		// Token: 0x04000753 RID: 1875
		private Stack<GUIButton> historyNodes = new Stack<GUIButton>();

		// Token: 0x04000754 RID: 1876
		private readonly List<Character> extraOptionCharacters = new List<Character>();

		// Token: 0x04000755 RID: 1877
		private const float nodeColorMultiplier = 0.75f;

		// Token: 0x04000756 RID: 1878
		private int nodeDistance = (int)(GUI.Scale * 250f);

		// Token: 0x04000757 RID: 1879
		private const float returnNodeDistanceModifier = 0.65f;

		// Token: 0x04000758 RID: 1880
		private Character characterContext;

		// Token: 0x04000759 RID: 1881
		private Item itemContext;

		// Token: 0x0400075A RID: 1882
		private Hull hullContext;

		// Token: 0x0400075B RID: 1883
		private WallSection wallContext;

		// Token: 0x0400075C RID: 1884
		private bool isContextual;

		// Token: 0x0400075D RID: 1885
		private readonly List<Order> contextualOrders = new List<Order>();

		// Token: 0x0400075E RID: 1886
		private Point shorcutCenterNodeOffset;

		// Token: 0x0400075F RID: 1887
		private const int maxShortcutNodeCount = 4;

		// Token: 0x04000761 RID: 1889
		private const float ConversationIntervalMin = 100f;

		// Token: 0x04000762 RID: 1890
		private const float ConversationIntervalMax = 180f;

		// Token: 0x04000763 RID: 1891
		private const float ConversationIntervalMultiplierMultiplayer = 5f;

		// Token: 0x04000764 RID: 1892
		private float conversationTimer;

		// Token: 0x04000765 RID: 1893
		private float conversationLineTimer;

		// Token: 0x04000766 RID: 1894
		[TupleElementNames(new string[]
		{
			"speaker",
			"line"
		})]
		private readonly List<ValueTuple<Character, string>> pendingConversationLines = new List<ValueTuple<Character, string>>();

		// Token: 0x04000767 RID: 1895
		public const int MaxCrewSize = 16;

		// Token: 0x04000768 RID: 1896
		private readonly List<CharacterInfo> characterInfos = new List<CharacterInfo>();

		// Token: 0x04000769 RID: 1897
		private readonly List<Character> characters = new List<Character>();

		// Token: 0x0400076A RID: 1898
		private readonly List<CharacterInfo> reserveBench = new List<CharacterInfo>();

		// Token: 0x0400076B RID: 1899
		private Character welcomeMessageNPC;

		// Token: 0x0400076F RID: 1903
		public ReadyCheck ActiveReadyCheck;

		// Token: 0x0200084E RID: 2126
		private class OptionNode
		{
			// Token: 0x06006D79 RID: 28025 RVA: 0x00362448 File Offset: 0x00360648
			public OptionNode(GUIButton guiComponent, Keys keys)
			{
				this.Button = guiComponent;
				this.Keys = keys;
			}

			// Token: 0x04003D92 RID: 15762
			public readonly GUIButton Button;

			// Token: 0x04003D93 RID: 15763
			public readonly Keys Keys;
		}

		// Token: 0x0200084F RID: 2127
		public struct MinimapNodeData
		{
			// Token: 0x04003D94 RID: 15764
			public Order Order;
		}

		// Token: 0x02000850 RID: 2128
		private class CharacterInfoComparer : IEqualityComparer<CharacterInfo>
		{
			// Token: 0x06006D7A RID: 28026 RVA: 0x0036245E File Offset: 0x0036065E
			public bool Equals(CharacterInfo x, CharacterInfo y)
			{
				return x == y || (x != null && y != null && x.ID == y.ID);
			}

			// Token: 0x06006D7B RID: 28027 RVA: 0x0036247C File Offset: 0x0036067C
			public int GetHashCode(CharacterInfo obj)
			{
				return (int)obj.ID;
			}
		}

		// Token: 0x02000851 RID: 2129
		public class ActiveOrder
		{
			// Token: 0x06006D7D RID: 28029 RVA: 0x0036248C File Offset: 0x0036068C
			public ActiveOrder(Order order, float? fadeOutTime)
			{
				this.Order = order;
				this.FadeOutTime = fadeOutTime;
			}

			// Token: 0x04003D95 RID: 15765
			public readonly Order Order;

			// Token: 0x04003D96 RID: 15766
			public float? FadeOutTime;
		}
	}
}
