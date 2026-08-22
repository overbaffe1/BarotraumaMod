using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Barotrauma.Eos;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Networking;
using Barotrauma.Steam;
using Barotrauma.Tutorials;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RestSharp;
using Steamworks.Data;
using Steamworks.Ugc;

namespace Barotrauma
{
	// Token: 0x02000115 RID: 277
	internal sealed class MainMenuScreen : Screen
	{
		// Token: 0x17000A02 RID: 2562
		// (get) Token: 0x06002561 RID: 9569 RVA: 0x00180D8D File Offset: 0x0017EF8D
		private static string RemoteContentUrl
		{
			get
			{
				return GameSettings.CurrentConfig.RemoteMainMenuContentUrl;
			}
		}

		// Token: 0x06002562 RID: 9570 RVA: 0x00180D9C File Offset: 0x0017EF9C
		public MainMenuScreen(GameMain game)
		{
			this.leftTextFooterLayout = this.<.ctor>g__createTextFooter|40_3();
			this.rightTextFooterLayout = this.<.ctor>g__createTextFooter|40_3();
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(20, 1);
			defaultInterpolatedStringHandler.AppendLiteral("GameAnalyticsStatus.");
			defaultInterpolatedStringHandler.AppendFormatted<GameAnalyticsManager.Consent>(GameAnalyticsManager.Consent.Unknown);
			this.gameAnalyticsStatusText = this.<.ctor>g__createLeftText|40_5(TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear()));
			string[] array = new string[9];
			array[0] = "Barotrauma v";
			int num = 1;
			Version version = GameMain.Version;
			array[num] = ((version != null) ? version.ToString() : null);
			array[2] = " (";
			array[3] = AssemblyInfo.BuildString;
			array[4] = ", branch ";
			array[5] = AssemblyInfo.GitBranch;
			array[6] = ", revision ";
			array[7] = AssemblyInfo.GitRevision;
			array[8] = ")";
			this.<.ctor>g__createLeftText|40_5(string.Concat(array));
			GUITextBlock privacyPolicyText = this.<.ctor>g__createRightText|40_6(TextManager.Get("privacypolicy").Fallback("Privacy policy", true));
			new GUICustomComponent(new RectTransform(Vector2.One, privacyPolicyText.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), delegate(SpriteBatch sb, GUICustomComponent component)
			{
				ValueTuple<Rectangle, bool> valueTuple = base.<.ctor>g__getPrivacyPolicyHoverRect|0();
				Rectangle rect = valueTuple.Item1;
				Microsoft.Xna.Framework.Color color = valueTuple.Item2 ? Microsoft.Xna.Framework.Color.White : (Microsoft.Xna.Framework.Color.White * 0.7f);
				privacyPolicyText.TextColor = color;
				GUI.DrawLine(sb, new Vector2((float)rect.Left, (float)rect.Bottom), new Vector2((float)rect.Right, (float)rect.Bottom), color, 0f, 1f);
			}, delegate(float dt, GUICustomComponent component)
			{
				bool mouseOn = base.<.ctor>g__getPrivacyPolicyHoverRect|0().Item2;
				if (mouseOn && PlayerInput.PrimaryMouseButtonClicked())
				{
					GameMain.ShowOpenUriPrompt("https://privacypolicy.daedalic.com", "openlinkinbrowserprompt", null);
				}
			});
			this.<.ctor>g__createRightText|40_6("© " + DateTime.Now.Year.ToString() + " Undertow Games & FakeFish. All rights reserved.");
			this.<.ctor>g__createRightText|40_6("© " + DateTime.Now.Year.ToString() + " Daedalic Entertainment GmbH. The Daedalic logo is a trademark of Daedalic Entertainment GmbH, Germany. All rights reserved.");
			GameMain.Instance.ResolutionChanged += delegate()
			{
				this.SetMenuTabPositioning();
				this.CreateHostServerFields();
				bool prevMenuOpen = GUI.SettingsMenuOpen;
				SettingsMenu.Create(this.menuTabs[MainMenuScreen.Tab.Settings].RectTransform);
				GUI.SettingsMenuOpen = prevMenuOpen;
				XDocument xdocument = this.remoteContentDoc;
				if (((xdocument != null) ? xdocument.Root : null) != null)
				{
					this.remoteContentContainer.ClearChildren();
					try
					{
						foreach (XElement subElement in this.remoteContentDoc.Root.Elements())
						{
							GUIComponent.FromXML(subElement.FromPackage(null), this.remoteContentContainer.RectTransform);
						}
					}
					catch (Exception e)
					{
						GameAnalyticsManager.AddErrorEventOnce("MainMenuScreen.RemoteContentParse:Exception", GameAnalyticsManager.ErrorSeverity.Error, "Reading received remote main menu content failed. " + e.Message);
					}
				}
			};
			this.versionMismatchWarning = new GUIFrame(new RectTransform(new Vector2(0.7f, 0.065f), this.Frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				AbsoluteOffset = new Point(GUI.IntScale(15f))
			}, "InnerFrame", new Microsoft.Xna.Framework.Color?(GUIStyle.Red))
			{
				IgnoreLayoutGroups = true,
				Visible = false
			};
			GUILayoutGroup versionMismatchContent = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.9f), this.versionMismatchWarning.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				RelativeSpacing = 0.05f
			};
			new GUIImage(new RectTransform(new Vector2(1f), versionMismatchContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Smallest), "GUINotificationButton", GUIImage.ScalingMode.None).Color = GUIStyle.Orange;
			new GUITextBlock(new RectTransform(new Vector2(0.85f, 1f), versionMismatchContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.GetWithVariables("versionmismatchwarning", new ValueTuple<string, string>[]
			{
				new ValueTuple<string, string>("[gameversion]", GameMain.Version.ToString()),
				new ValueTuple<string, string>("[contentversion]", ContentPackageManager.VanillaCorePackage.GameVersion.ToString())
			}), null, null, Alignment.Left, true, "", null).TextColor = GUIStyle.Orange;
			GUIImage guiimage = new GUIImage(new RectTransform(new Vector2(0.4f, 0.25f), this.Frame.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0.08f, 0.05f),
				AbsoluteOffset = new Point(-8, -8)
			}, "TitleText", GUIImage.ScalingMode.None);
			guiimage.Color = Microsoft.Xna.Framework.Color.Black * 0.5f;
			guiimage.CanBeFocused = false;
			this.titleText = new GUIImage(new RectTransform(new Vector2(0.4f, 0.25f), this.Frame.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0.08f, 0.05f)
			}, "TitleText", GUIImage.ScalingMode.None);
			this.buttonsParent = new GUILayoutGroup(new RectTransform(new Vector2(0.3f, 0.85f), this.Frame.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal)
			{
				AbsoluteOffset = new Point(50, 0)
			}, false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.02f
			};
			this.remoteContentContainer = new GUIFrame(new RectTransform(Vector2.One, this.Frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
			{
				CanBeFocused = false
			};
			SpamServerFilters.RequestGlobalSpamFilter();
			this.FetchRemoteContent();
			float labelHeight = 0.18f;
			GUILayoutGroup campaignHolder = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 1f), this.buttonsParent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0.1f, 0f)
			}, true, Anchor.TopLeft);
			new GUIImage(new RectTransform(new Vector2(0.2f, 0.7f), campaignHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "MainMenuCampaignIcon", GUIImage.ScalingMode.None).CanBeFocused = false;
			new GUIFrame(new RectTransform(new Vector2(0.02f, 0f), campaignHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUILayoutGroup campaignNavigation = new GUILayoutGroup(new RectTransform(new Vector2(0.75f, 0.75f), campaignHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0f, 0.25f)
			}, false, Anchor.TopLeft);
			RectTransform rectT = new RectTransform(new Vector2(1f, labelHeight), campaignNavigation.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = TextManager.Get("CampaignLabel");
			GUIFont largeFont = GUIStyle.LargeFont;
			new GUITextBlock(rectT, text, new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Black), largeFont, Alignment.Left, false, "MainMenuGUITextBlock", null).ForceUpperCase = ForceUpperCase.Yes;
			GUIFrame campaignButtons = new GUIFrame(new RectTransform(new Vector2(1f, 1f), campaignNavigation.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "MainMenuGUIFrame", null);
			GUILayoutGroup campaignList = new GUILayoutGroup(new RectTransform(new Vector2(0.8f, 0.2f), campaignButtons.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = false,
				RelativeSpacing = 0.035f
			};
			GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(1f, 1f), campaignList.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("TutorialButton"), Alignment.Left, "MainMenuGUIButton", null);
			guibutton.ForceUpperCase = ForceUpperCase.Yes;
			guibutton.UserData = MainMenuScreen.Tab.Tutorials;
			guibutton.OnClicked = delegate(GUIButton tb, object userdata)
			{
				this.SelectTab(tb, userdata);
				return true;
			};
			GUIButton guibutton2 = new GUIButton(new RectTransform(new Vector2(1f, 1f), campaignList.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("LoadGameButton"), Alignment.Left, "MainMenuGUIButton", null);
			guibutton2.ForceUpperCase = ForceUpperCase.Yes;
			guibutton2.UserData = MainMenuScreen.Tab.LoadGame;
			guibutton2.OnClicked = delegate(GUIButton tb, object userdata)
			{
				this.SelectTab(tb, userdata);
				return true;
			};
			GUIButton guibutton3 = new GUIButton(new RectTransform(new Vector2(1f, 1f), campaignList.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("NewGameButton"), Alignment.Left, "MainMenuGUIButton", null);
			guibutton3.ForceUpperCase = ForceUpperCase.Yes;
			guibutton3.UserData = MainMenuScreen.Tab.NewGame;
			guibutton3.OnClicked = delegate(GUIButton tb, object userdata)
			{
				this.SelectTab(tb, userdata);
				return true;
			};
			GUILayoutGroup multiplayerHolder = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 1f), this.buttonsParent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0.05f, 0f)
			}, true, Anchor.TopLeft);
			new GUIImage(new RectTransform(new Vector2(0.2f, 0.7f), multiplayerHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "MainMenuMultiplayerIcon", GUIImage.ScalingMode.None).CanBeFocused = false;
			new GUIFrame(new RectTransform(new Vector2(0.02f, 0f), multiplayerHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUILayoutGroup multiplayerNavigation = new GUILayoutGroup(new RectTransform(new Vector2(0.75f, 0.75f), multiplayerHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0f, 0.25f)
			}, false, Anchor.TopLeft);
			RectTransform rectT2 = new RectTransform(new Vector2(1f, labelHeight), multiplayerNavigation.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = TextManager.Get("MultiplayerLabel");
			largeFont = GUIStyle.LargeFont;
			new GUITextBlock(rectT2, text2, new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Black), largeFont, Alignment.Left, false, "MainMenuGUITextBlock", null).ForceUpperCase = ForceUpperCase.Yes;
			GUIFrame multiplayerButtons = new GUIFrame(new RectTransform(new Vector2(1f, 1f), multiplayerNavigation.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "MainMenuGUIFrame", null);
			GUILayoutGroup multiplayerList = new GUILayoutGroup(new RectTransform(new Vector2(0.8f, 0.2f), multiplayerButtons.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = false,
				RelativeSpacing = 0.035f
			};
			this.joinServerButton = new GUIButton(new RectTransform(new Vector2(1f, 1f), multiplayerList.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("JoinServerButton"), Alignment.Left, "MainMenuGUIButton", null)
			{
				ForceUpperCase = ForceUpperCase.Yes,
				UserData = MainMenuScreen.Tab.JoinServer,
				OnClicked = delegate(GUIButton tb, object userdata)
				{
					this.SelectTab(tb, userdata);
					return true;
				}
			};
			this.hostServerButton = new GUIButton(new RectTransform(new Vector2(1f, 1f), multiplayerList.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("HostServerButton"), Alignment.Left, "MainMenuGUIButton", null)
			{
				ForceUpperCase = ForceUpperCase.Yes,
				UserData = MainMenuScreen.Tab.HostServer,
				OnClicked = delegate(GUIButton tb, object userdata)
				{
					this.SelectTab(tb, userdata);
					return true;
				}
			};
			GUILayoutGroup customizeHolder = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 1f), this.buttonsParent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0.15f, 0f)
			}, true, Anchor.TopLeft);
			new GUIImage(new RectTransform(new Vector2(0.2f, 0.7f), customizeHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "MainMenuCustomizeIcon", GUIImage.ScalingMode.None).CanBeFocused = false;
			new GUIFrame(new RectTransform(new Vector2(0.02f, 0f), customizeHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUILayoutGroup customizeNavigation = new GUILayoutGroup(new RectTransform(new Vector2(0.75f, 0.75f), customizeHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0f, 0.25f)
			}, false, Anchor.TopLeft);
			RectTransform rectT3 = new RectTransform(new Vector2(1f, labelHeight), customizeNavigation.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text3 = TextManager.Get("CustomizeLabel");
			largeFont = GUIStyle.LargeFont;
			new GUITextBlock(rectT3, text3, new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Black), largeFont, Alignment.Left, false, "MainMenuGUITextBlock", null).ForceUpperCase = ForceUpperCase.Yes;
			GUIFrame customizeButtons = new GUIFrame(new RectTransform(new Vector2(1f, 1f), customizeNavigation.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "MainMenuGUIFrame", null);
			GUILayoutGroup customizeList = new GUILayoutGroup(new RectTransform(new Vector2(0.8f, 0.2f), customizeButtons.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = false,
				RelativeSpacing = 0.035f
			};
			this.modsButtonContainer = new GUIFrame(new RectTransform(Vector2.One, customizeList.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			this.modsButton = new GUIButton(new RectTransform(Vector2.One, this.modsButtonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("settingstab.mods"), Alignment.Left, "MainMenuGUIButton", null)
			{
				ForceUpperCase = ForceUpperCase.Yes,
				Enabled = true,
				UserData = MainMenuScreen.Tab.Mods,
				OnClicked = new GUIButton.OnClickedHandler(this.SelectTab)
			};
			GUIButton guibutton4 = new GUIButton(new RectTransform(Vector2.One * 0.95f, this.modsButtonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), Alignment.Center, "GUIUpdateButton", null);
			guibutton4.ToolTip = TextManager.Get("ModUpdatesAvailable");
			guibutton4.OnClicked = delegate(GUIButton _, object _)
			{
				BulkDownloader.PrepareUpdates();
				return false;
			};
			guibutton4.Visible = false;
			this.modUpdatesButton = guibutton4;
			GUIButton guibutton5 = new GUIButton(new RectTransform(new Vector2(1f, 1f), customizeList.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("SubEditorButton"), Alignment.Left, "MainMenuGUIButton", null);
			guibutton5.ForceUpperCase = ForceUpperCase.Yes;
			guibutton5.UserData = MainMenuScreen.Tab.SubmarineEditor;
			guibutton5.OnClicked = delegate(GUIButton tb, object userdata)
			{
				this.SelectTab(tb, userdata);
				return true;
			};
			GUIButton guibutton6 = new GUIButton(new RectTransform(new Vector2(1f, 1f), customizeList.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("CharacterEditorButton"), Alignment.Left, "MainMenuGUIButton", null);
			guibutton6.ForceUpperCase = ForceUpperCase.Yes;
			guibutton6.UserData = MainMenuScreen.Tab.CharacterEditor;
			guibutton6.OnClicked = delegate(GUIButton tb, object userdata)
			{
				this.SelectTab(tb, userdata);
				return true;
			};
			GUILayoutGroup optionHolder = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.8f), this.buttonsParent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			new GUIImage(new RectTransform(new Vector2(0.15f, 0.6f), optionHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "MainMenuOptionIcon", GUIImage.ScalingMode.None).CanBeFocused = false;
			new GUIFrame(new RectTransform(new Vector2(0.01f, 0f), optionHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUILayoutGroup optionButtons = new GUILayoutGroup(new RectTransform(new Vector2(0.8f, 1f), optionHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0f, 0f)
			}, false, Anchor.TopLeft);
			GUILayoutGroup optionList = new GUILayoutGroup(new RectTransform(new Vector2(0.8f, 0.25f), optionButtons.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = false,
				RelativeSpacing = 0.035f
			};
			GUIFrame settingsButtonContainer = new GUIFrame(new RectTransform(new Vector2(1f, 1f), optionList.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUIButton guibutton7 = new GUIButton(new RectTransform(Vector2.One, settingsButtonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("SettingsButton"), Alignment.Left, "MainMenuGUIButton", null);
			guibutton7.ForceUpperCase = ForceUpperCase.Yes;
			guibutton7.UserData = MainMenuScreen.Tab.Settings;
			guibutton7.OnClicked = new GUIButton.OnClickedHandler(this.SelectTab);
			GUIButton guibutton8 = new GUIButton(new RectTransform(new Vector2(1f, 1f), optionList.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("EditorDisclaimerWikiLink"), Alignment.Left, "MainMenuGUIButton", null);
			guibutton8.ForceUpperCase = ForceUpperCase.Yes;
			guibutton8.OnClicked = delegate(GUIButton button, object userData)
			{
				string url = TextManager.Get("EditorDisclaimerWikiUrl").Fallback("https://barotraumagame.com/wiki", true).Value;
				GameMain.ShowOpenUriPrompt(url, "openlinkinbrowserprompt", "wikinotice");
				return true;
			};
			GUIButton guibutton9 = new GUIButton(new RectTransform(new Vector2(1f, 1f), optionList.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("CreditsButton"), Alignment.Left, "MainMenuGUIButton", null);
			guibutton9.ForceUpperCase = ForceUpperCase.Yes;
			guibutton9.UserData = MainMenuScreen.Tab.Credits;
			guibutton9.OnClicked = new GUIButton.OnClickedHandler(this.SelectTab);
			GUIButton guibutton10 = new GUIButton(new RectTransform(new Vector2(1f, 1f), optionList.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("QuitButton"), Alignment.Left, "MainMenuGUIButton", null);
			guibutton10.ForceUpperCase = ForceUpperCase.Yes;
			guibutton10.OnClicked = new GUIButton.OnClickedHandler(this.QuitClicked);
			Point minButtonSize = new Point(120, 20);
			Point maxButtonSize = new Point(480, 80);
			Vector2 relativeSize = new Vector2(0.6f, 0.65f);
			Point minSize = new Point(600, 400);
			Point maxSize = new Point(2000, 1500);
			Anchor anchor = Anchor.Center;
			Pivot pivot = Pivot.Center;
			Vector2 relativeOffset = new Vector2(0.05f, 0f);
			Dictionary<MainMenuScreen.Tab, GUIFrame> dictionary = new Dictionary<MainMenuScreen.Tab, GUIFrame>();
			dictionary[MainMenuScreen.Tab.Settings] = new GUIFrame(new RectTransform(new Vector2(relativeSize.X, 0.8f), this.Frame.RectTransform, anchor, new Pivot?(pivot), new Point?(minSize), new Point?(maxSize), ScaleBasis.Normal)
			{
				RelativeOffset = relativeOffset
			}, null, null)
			{
				CanBeFocused = false
			};
			dictionary[MainMenuScreen.Tab.NewGame] = new GUIFrame(new RectTransform(relativeSize * new Vector2(1f, 1.15f), this.Frame.RectTransform, anchor, new Pivot?(pivot), new Point?(minSize), new Point?(maxSize), ScaleBasis.Normal)
			{
				RelativeOffset = relativeOffset
			}, "", null);
			dictionary[MainMenuScreen.Tab.LoadGame] = new GUIFrame(new RectTransform(relativeSize, this.Frame.RectTransform, anchor, new Pivot?(pivot), new Point?(minSize), new Point?(maxSize), ScaleBasis.Normal)
			{
				RelativeOffset = relativeOffset
			}, "", null);
			this.menuTabs = dictionary;
			this.CreateCampaignSetupUI();
			Vector2 hostServerScale = new Vector2(0.7f, 1.2f);
			this.menuTabs[MainMenuScreen.Tab.HostServer] = new GUIFrame(new RectTransform(Vector2.Multiply(relativeSize, hostServerScale), this.Frame.RectTransform, anchor, new Pivot?(pivot), new Point?(minSize.Multiply(hostServerScale)), new Point?(maxSize.Multiply(hostServerScale)), ScaleBasis.Normal)
			{
				RelativeOffset = relativeOffset
			}, "", null);
			this.CreateHostServerFields();
			this.menuTabs[MainMenuScreen.Tab.Tutorials] = new GUIFrame(new RectTransform(relativeSize, this.Frame.RectTransform, anchor, new Pivot?(pivot), new Point?(minSize), new Point?(maxSize), ScaleBasis.Normal)
			{
				RelativeOffset = relativeOffset
			}, "", null);
			this.CreateTutorialTab();
			this.game = game;
			this.menuTabs[MainMenuScreen.Tab.Credits] = new GUIFrame(new RectTransform(Vector2.One, this.Frame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), null, null)
			{
				CanBeFocused = false
			};
			GUIFrame blockerFrame = new GUIFrame(new RectTransform(GUI.Canvas.RelativeSize, this.menuTabs[MainMenuScreen.Tab.Credits].RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "GUIBackgroundBlocker", null)
			{
				CanBeFocused = false
			};
			blockerFrame.RectTransform.RelativeOffset = (GUI.IsUltrawide ? Vector2.Zero : new Vector2(0.05f, 0f));
			GUIFrame creditsContainer = new GUIFrame(new RectTransform(new Vector2(0.75f, 1.5f), this.menuTabs[MainMenuScreen.Tab.Credits].RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), "OuterGlow", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Black * 0.8f));
			this.creditsPlayer = new CreditsPlayer(new RectTransform(Vector2.One, creditsContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "Content/Texts/Credits.xml");
			this.creditsPlayer.CloseButton.OnClicked = delegate(GUIButton btn, object userdata)
			{
				this.SelectTab(MainMenuScreen.Tab.Empty);
				return true;
			};
			this.SetMenuTabPositioning();
			this.SelectTab(MainMenuScreen.Tab.Empty);
		}

		// Token: 0x06002563 RID: 9571 RVA: 0x0018270D File Offset: 0x0018090D
		public static void AddDismissedNotification(Identifier id)
		{
			MainMenuScreen.DismissedNotifications.Add(id);
			GameSettings.SaveCurrentConfig();
		}

		// Token: 0x06002564 RID: 9572 RVA: 0x00182720 File Offset: 0x00180920
		private void SetMenuTabPositioning()
		{
			foreach (GUIFrame menuTab in this.menuTabs.Values)
			{
				Anchor anchor = GUI.IsUltrawide ? Anchor.Center : Anchor.CenterRight;
				Pivot pivot = GUI.IsUltrawide ? Pivot.Center : Pivot.CenterRight;
				Vector2 relativeOffset = GUI.IsUltrawide ? Vector2.Zero : new Vector2(0.05f, 0f);
				menuTab.RectTransform.SetPosition(anchor, new Pivot?(pivot));
				menuTab.RectTransform.RelativeOffset = relativeOffset;
			}
		}

		// Token: 0x06002565 RID: 9573 RVA: 0x001827C8 File Offset: 0x001809C8
		private void CreateTutorialTab()
		{
			GUIFrame tutorialInnerFrame = new GUIFrame(new RectTransform(new Vector2(0.9f, 0.9f), this.menuTabs[MainMenuScreen.Tab.Tutorials].RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "InnerFrame", null);
			GUILayoutGroup tutorialContent = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.95f), tutorialInnerFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				RelativeSpacing = 0.02f,
				Stretch = true
			};
			this.tutorialList = new GUIListBox(new RectTransform(new Vector2(0.4f, 1f), tutorialContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				PlaySoundOnSelect = true,
				OnSelected = delegate(GUIComponent component, object obj)
				{
					this.SelectTutorial(obj as Tutorial);
					return true;
				}
			};
			GUILayoutGroup tutorialPreview = new GUILayoutGroup(new RectTransform(new Vector2(0.6f, 1f), tutorialContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.05f,
				Stretch = true
			};
			GUIFrame imageContainer = new GUIFrame(new RectTransform(new Vector2(1f, 0.5f), tutorialPreview.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "InnerFrame", null);
			this.tutorialBanner = new GUIImage(new RectTransform(Vector2.One, imageContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, GUIImage.ScalingMode.ScaleToFitSmallestExtent);
			GUIFrame infoContainer = new GUIFrame(new RectTransform(new Vector2(1f, 0.5f), tutorialPreview.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "GUIFrameListBox", null);
			GUILayoutGroup infoContent = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.9f), infoContainer.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				AbsoluteSpacing = GUI.IntScale(10f)
			};
			RectTransform rectT = new RectTransform(new Vector2(1f, 0f), infoContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = string.Empty;
			GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
			this.tutorialHeader = new GUITextBlock(rectT, text, null, subHeadingFont, Alignment.Left, false, "", null);
			this.tutorialDescription = new GUITextBlock(new RectTransform(new Vector2(1f, 0f), infoContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), string.Empty, null, null, Alignment.Left, true, "", null);
			GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(0.5f, 0f), infoContent.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Normal), TextManager.Get("startgamebutton"), Alignment.Center, "", null);
			guibutton.IgnoreLayoutGroups = true;
			guibutton.OnClicked = delegate(GUIButton component, object obj)
			{
				Tutorial tutorial2 = this.tutorialList.SelectedData as Tutorial;
				if (tutorial2 != null)
				{
					tutorial2.Start();
				}
				return true;
			};
			Tutorial firstTutorial = null;
			foreach (TutorialPrefab tutorialPrefab in from p in TutorialPrefab.Prefabs
			orderby p.Order
			select p)
			{
				Tutorial tutorial = new Tutorial(tutorialPrefab);
				if (firstTutorial == null)
				{
					firstTutorial = tutorial;
				}
				GUITextBlock tutorialText = new GUITextBlock(new RectTransform(new Vector2(1f, 0f), this.tutorialList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), tutorial.DisplayName, null, null, Alignment.Left, false, "", null)
				{
					Padding = new Vector4(30f * GUI.Scale, 0f, 0f, 0f),
					UserData = tutorial
				};
				tutorialText.RectTransform.MinSize = new Point(0, (int)(tutorialText.TextSize.Y * 2f));
			}
			GUITextBlock.AutoScaleAndNormalize(from c in this.tutorialList.Content.Children
			select c as GUITextBlock, true, false, null);
			this.tutorialList.Select(firstTutorial, GUIListBox.Force.No, GUIListBox.AutoScroll.Enabled);
		}

		// Token: 0x06002566 RID: 9574 RVA: 0x00182D64 File Offset: 0x00180F64
		private void SelectTutorial(Tutorial tutorial)
		{
			this.tutorialHeader.Text = tutorial.DisplayName;
			this.tutorialHeader.CalculateHeightFromText(0, false);
			this.tutorialDescription.Text = tutorial.Description;
			this.tutorialDescription.CalculateHeightFromText(0, false);
			GUILayoutGroup guilayoutGroup = this.tutorialDescription.Parent as GUILayoutGroup;
			if (guilayoutGroup != null)
			{
				guilayoutGroup.Recalculate();
			}
			Sprite banner = tutorial.TutorialPrefab.Banner;
			if (banner != null)
			{
				banner.EnsureLazyLoaded(false);
			}
			this.tutorialBanner.Sprite = tutorial.TutorialPrefab.Banner;
			this.tutorialBanner.Color = ((tutorial.TutorialPrefab.Banner == null) ? Microsoft.Xna.Framework.Color.Black : Microsoft.Xna.Framework.Color.White);
		}

		// Token: 0x06002567 RID: 9575 RVA: 0x00182E24 File Offset: 0x00181024
		public static void UpdateInstanceTutorialButtons()
		{
			MainMenuScreen menuScreen = GameMain.MainMenuScreen;
			if (menuScreen == null)
			{
				return;
			}
			menuScreen.tutorialList.ClearChildren();
			menuScreen.CreateTutorialTab();
		}

		// Token: 0x06002568 RID: 9576 RVA: 0x00182E4C File Offset: 0x0018104C
		public override void Select()
		{
			this.ResetModUpdateButton();
			if (MainMenuScreen.WorkshopItemsToUpdate.Any<ulong>())
			{
				ulong workshopId;
				while (MainMenuScreen.WorkshopItemsToUpdate.TryDequeue(out workshopId))
				{
					SteamManager.Workshop.OnItemDownloadComplete(workshopId, true);
				}
			}
			GUI.PreventPauseMenuToggle = false;
			base.Select();
			if (GameMain.Client != null)
			{
				GameMain.Client.Quit();
				GameMain.Client = null;
			}
			SubEditorScreen subEditorScreen = GameMain.SubEditorScreen;
			if (subEditorScreen != null)
			{
				subEditorScreen.ClearBackedUpSubInfo();
			}
			Submarine.Unload();
			this.versionMismatchWarning.Visible = (GameMain.Version < ContentPackageManager.VanillaCorePackage.GameVersion);
			this.ResetButtonStates(null);
			Action action;
			if ((action = MainMenuScreen.<>O.<0>__SyncBetweenPlatforms) == null)
			{
				action = (MainMenuScreen.<>O.<0>__SyncBetweenPlatforms = new Action(AchievementManager.SyncBetweenPlatforms));
			}
			EosAccount.ExecuteAfterLogin(action);
		}

		// Token: 0x06002569 RID: 9577 RVA: 0x00182F00 File Offset: 0x00181100
		public override void Deselect()
		{
			base.Deselect();
			this.SelectTab(null, 0);
		}

		// Token: 0x0600256A RID: 9578 RVA: 0x00182F18 File Offset: 0x00181118
		private bool SelectTab(GUIButton button, object obj)
		{
			this.titleText.Visible = true;
			if (obj is MainMenuScreen.Tab)
			{
				MainMenuScreen.Tab tab = (MainMenuScreen.Tab)obj;
				this.SelectTab(tab);
			}
			else
			{
				this.SelectTab(MainMenuScreen.Tab.Empty);
			}
			return true;
		}

		// Token: 0x0600256B RID: 9579 RVA: 0x00182F54 File Offset: 0x00181154
		private bool SelectTab(MainMenuScreen.Tab tab)
		{
			this.titleText.Visible = true;
			SettingsMenu instance = SettingsMenu.Instance;
			if (instance != null)
			{
				instance.Close();
			}
			switch (tab)
			{
			case MainMenuScreen.Tab.NewGame:
				if (GameSettings.CurrentConfig.TutorialSkipWarning)
				{
					this.selectedTab = MainMenuScreen.Tab.Empty;
					this.ShowTutorialSkipWarning(MainMenuScreen.Tab.NewGame);
					return true;
				}
				this.campaignSetupUI.RandomizeCrew();
				this.campaignSetupUI.SetPage(0);
				this.campaignSetupUI.CreateDefaultSaveName();
				this.campaignSetupUI.RandomizeSeed();
				this.campaignSetupUI.UpdateSubList(SubmarineInfo.SavedSubmarines);
				break;
			case MainMenuScreen.Tab.LoadGame:
				this.campaignSetupUI.CreateLoadMenu(null);
				break;
			case MainMenuScreen.Tab.HostServer:
			{
				if (GameSettings.CurrentConfig.TutorialSkipWarning)
				{
					this.selectedTab = MainMenuScreen.Tab.Empty;
					this.ShowTutorialSkipWarning(tab);
					return true;
				}
				this.serverExecutableDropdown.ListBox.Content.Children.ToArray<GUIComponent>().Where(delegate(GUIComponent c)
				{
					ServerExecutableFile f = c.UserData as ServerExecutableFile;
					return f != null && !ContentPackageManager.EnabledPackages.All.Contains(f.ContentPackage);
				}).ForEach(new Action<GUIComponent>(this.serverExecutableDropdown.ListBox.RemoveChild));
				ServerExecutableFile[] newServerExes = (from f in ContentPackageManager.EnabledPackages.All.SelectMany((ContentPackage p) => p.GetFiles<ServerExecutableFile>())
				where this.serverExecutableDropdown.ListBox.Content.Children.None((GUIComponent c) => c.UserData == f)
				select f).ToArray<ServerExecutableFile>();
				foreach (ServerExecutableFile newServerExe in newServerExes)
				{
					GUIComponent serverExeEntry = this.serverExecutableDropdown.AddItem(newServerExe.ContentPackage.Name + " - " + Path.GetFileNameWithoutExtension(newServerExe.Path.Value), newServerExe, null, null, null);
					if (newServerExe.ContentPackage.GameVersion < GameMain.VanillaContent.GameVersion)
					{
						serverExeEntry.ToolTip = TextManager.GetWithVariables("versionmismatchwarning", new ValueTuple<string, string>[]
						{
							new ValueTuple<string, string>("[gameversion]", newServerExe.ContentPackage.GameVersion.ToString()),
							new ValueTuple<string, string>("[contentversion]", GameMain.VanillaContent.GameVersion.ToString())
						});
						GUITextBlock serverExeText = serverExeEntry as GUITextBlock;
						if (serverExeText != null)
						{
							serverExeText.TextColor = GUIStyle.Red;
						}
					}
				}
				this.serverExecutableDropdown.ListBox.Content.Children.ForEach(delegate(GUIComponent c)
				{
					c.RectTransform.RelativeSize = new ValueTuple<float, float>(1f, c.RectTransform.RelativeSize.Y);
					c.ForceLayoutRecalculation();
				});
				bool serverExePickable = this.serverExecutableDropdown.ListBox.Content.CountChildren > 1;
				bool wasPickable = this.serverExecutableDropdown.Parent.Visible;
				if (wasPickable != serverExePickable)
				{
					this.serverExecutableDropdown.Parent.Visible = serverExePickable;
					this.serverExecutableDropdown.Parent.IgnoreLayoutGroups = !serverExePickable;
					GUILayoutGroup guilayoutGroup = this.serverExecutableDropdown.Parent.Parent as GUILayoutGroup;
					if (guilayoutGroup != null)
					{
						guilayoutGroup.Recalculate();
					}
					if (this.serverExecutableDropdown.SelectedComponent == null)
					{
						this.serverExecutableDropdown.Select(0);
					}
				}
				break;
			}
			case MainMenuScreen.Tab.Settings:
				SettingsMenu.Create(this.menuTabs[MainMenuScreen.Tab.Settings].RectTransform);
				break;
			case MainMenuScreen.Tab.Tutorials:
				this.UpdateTutorialList();
				break;
			case MainMenuScreen.Tab.JoinServer:
				if (GameSettings.CurrentConfig.TutorialSkipWarning)
				{
					this.selectedTab = MainMenuScreen.Tab.Empty;
					this.ShowTutorialSkipWarning(MainMenuScreen.Tab.JoinServer);
					return true;
				}
				GameMain.ServerListScreen.Select();
				break;
			case MainMenuScreen.Tab.CharacterEditor:
				Submarine.MainSub = null;
				CoroutineManager.StartCoroutine(this.SelectScreenWithWaitCursor(GameMain.CharacterEditorScreen), "");
				break;
			case MainMenuScreen.Tab.SubmarineEditor:
				CoroutineManager.StartCoroutine(this.SelectScreenWithWaitCursor(GameMain.SubEditorScreen), "");
				break;
			case MainMenuScreen.Tab.Mods:
			{
				SettingsMenu settings = SettingsMenu.Create(this.menuTabs[MainMenuScreen.Tab.Settings].RectTransform);
				settings.SelectTab(SettingsMenu.Tab.Mods);
				tab = MainMenuScreen.Tab.Settings;
				break;
			}
			case MainMenuScreen.Tab.Credits:
				this.titleText.Visible = false;
				this.creditsPlayer.Restart();
				break;
			case MainMenuScreen.Tab.Empty:
				this.titleText.Visible = true;
				this.selectedTab = MainMenuScreen.Tab.Empty;
				break;
			}
			this.selectedTab = tab;
			this.leftTextFooterLayout.Visible = (tab != MainMenuScreen.Tab.Credits);
			this.rightTextFooterLayout.Visible = (tab != MainMenuScreen.Tab.Credits);
			foreach (GUIFrame tabFrame in this.menuTabs.Values)
			{
				tabFrame.Visible = false;
			}
			GUIFrame visibleTab;
			if (this.menuTabs.TryGetValue(this.selectedTab, out visibleTab))
			{
				visibleTab.Visible = true;
			}
			return true;
		}

		// Token: 0x0600256C RID: 9580 RVA: 0x0018342C File Offset: 0x0018162C
		private IEnumerable<CoroutineStatus> SelectScreenWithWaitCursor(Screen screen)
		{
			MainMenuScreen.<SelectScreenWithWaitCursor>d__50 <SelectScreenWithWaitCursor>d__ = new MainMenuScreen.<SelectScreenWithWaitCursor>d__50(-2);
			<SelectScreenWithWaitCursor>d__.<>3__screen = screen;
			return <SelectScreenWithWaitCursor>d__;
		}

		// Token: 0x0600256D RID: 9581 RVA: 0x0018343C File Offset: 0x0018163C
		public bool ReturnToMainMenu(GUIButton button, object obj)
		{
			GUI.PreventPauseMenuToggle = false;
			if (Screen.Selected != this)
			{
				this.Select();
			}
			else
			{
				this.ResetButtonStates(button);
			}
			this.SelectTab(null, 0);
			return true;
		}

		// Token: 0x0600256E RID: 9582 RVA: 0x0018346C File Offset: 0x0018166C
		private void ResetButtonStates(GUIButton button)
		{
			foreach (GUIComponent child in this.buttonsParent.Children)
			{
				GUIButton otherButton = child as GUIButton;
				if (otherButton != null && otherButton != button)
				{
					otherButton.Selected = false;
				}
			}
		}

		// Token: 0x0600256F RID: 9583 RVA: 0x001834CC File Offset: 0x001816CC
		public void ResetModUpdateButton()
		{
			this.modUpdateStatus = new ValueTuple<DateTime, int>(DateTime.Now, 0);
			this.modUpdatesButton.Visible = false;
		}

		// Token: 0x06002570 RID: 9584 RVA: 0x001834EC File Offset: 0x001816EC
		public void QuickStart(bool fixedSeed = false, Identifier sub = default(Identifier), float difficulty = 50f, LevelGenerationParams levelGenerationParams = null)
		{
			if (fixedSeed)
			{
				Rand.SetSyncedSeed(1);
				Rand.SetLocalRandom(1);
			}
			SubmarineInfo selectedSub = null;
			Identifier subName = sub.IfEmpty(GameSettings.CurrentConfig.QuickStartSub);
			if (!subName.IsEmpty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(41, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Loading the predefined quick start sub \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(subName);
				defaultInterpolatedStringHandler.AppendLiteral("\"");
				DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.White), false);
				selectedSub = SubmarineInfo.SavedSubmarines.FirstOrDefault((SubmarineInfo s) => s.Name == subName);
				if (selectedSub == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(43, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("Cannot find a sub that matches the name \"");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(subName);
					defaultInterpolatedStringHandler2.AppendLiteral("\".");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
				}
			}
			if (selectedSub == null)
			{
				DebugConsole.NewMessage("Loading a random sub.", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.White), false);
				IEnumerable<SubmarineInfo> subs = from s in SubmarineInfo.SavedSubmarines
				where s.Type == SubmarineType.Player && !s.HasTag(SubmarineTag.Shuttle) && !s.HasTag(SubmarineTag.HideInMenus)
				select s;
				selectedSub = subs.ElementAt(Rand.Int(subs.Count<SubmarineInfo>(), Rand.RandSync.Unsynced));
			}
			SubmarineInfo submarineInfo = selectedSub;
			Option.UnspecifiedNone none = Option.None;
			GameSession gamesession = new GameSession(submarineInfo, none, GameModePreset.DevSandbox, null, null);
			gamesession.StartRound(fixedSeed ? "abcd" : ToolBox.RandomSeed(8), new float?(difficulty), levelGenerationParams, default(Identifier));
			GameMain.GameScreen.Select();
			Identifier[] jobIdentifiers = new Identifier[]
			{
				"captain".ToIdentifier(),
				"engineer".ToIdentifier(),
				"mechanic".ToIdentifier(),
				"securityofficer".ToIdentifier(),
				"medicaldoctor".ToIdentifier()
			};
			foreach (Identifier job in jobIdentifiers)
			{
				JobPrefab jobPrefab = JobPrefab.Get(job);
				int variant = Rand.Range(0, jobPrefab.Variants, Rand.RandSync.Unsynced);
				CharacterInfo characterInfo = new CharacterInfo(CharacterPrefab.HumanSpeciesName, "", "", jobPrefab, variant, Rand.RandSync.Unsynced, default(Identifier));
				if (characterInfo.Job == null)
				{
					DebugConsole.ThrowError("Failed to find the job \"" + job.ToString() + "\"!", null, null, false, false);
				}
				gamesession.CrewManager.AddCharacterInfo(characterInfo);
				characterInfo.SetNameBasedOnJob();
			}
			gamesession.CrewManager.InitSinglePlayerRound();
		}

		// Token: 0x06002571 RID: 9585 RVA: 0x00183798 File Offset: 0x00181998
		private void ShowTutorialSkipWarning(MainMenuScreen.Tab tabToContinueTo)
		{
			MainMenuScreen.<>c__DisplayClass55_0 CS$<>8__locals1 = new MainMenuScreen.<>c__DisplayClass55_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.tutorialSkipWarning = new GUIMessageBox("", TextManager.Get("tutorialskipwarning"), new LocalizedString[]
			{
				TextManager.Get("tutorialwarningskiptutorials"),
				TextManager.Get("tutorialwarningplaytutorials")
			}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			GUIButton guibutton = CS$<>8__locals1.tutorialSkipWarning.Buttons[0];
			guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, CS$<>8__locals1.<ShowTutorialSkipWarning>g__proceedToTab|0(tabToContinueTo));
			GUIButton guibutton2 = CS$<>8__locals1.tutorialSkipWarning.Buttons[1];
			guibutton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton2.OnClicked, CS$<>8__locals1.<ShowTutorialSkipWarning>g__proceedToTab|0(MainMenuScreen.Tab.Tutorials));
		}

		// Token: 0x06002572 RID: 9586 RVA: 0x00183874 File Offset: 0x00181A74
		public override void AddToGUIUpdateList()
		{
			base.AddToGUIUpdateList();
			if (this.selectedTab == MainMenuScreen.Tab.NewGame)
			{
				CharacterInfo.AppearanceCustomizationMenu[] characterMenus = this.campaignSetupUI.CharacterMenus;
				if (characterMenus == null)
				{
					return;
				}
				characterMenus.ForEach(delegate(CharacterInfo.AppearanceCustomizationMenu m)
				{
					m.AddToGUIUpdateList();
				});
			}
		}

		// Token: 0x06002573 RID: 9587 RVA: 0x001838C8 File Offset: 0x00181AC8
		private void UpdateTutorialList()
		{
			foreach (GUIComponent guicomponent in this.tutorialList.Content.Children)
			{
				GUITextBlock tutorialText = (GUITextBlock)guicomponent;
				Tutorial tutorial = (Tutorial)tutorialText.UserData;
				if (CompletedTutorials.Instance.Contains(tutorial.Identifier) && tutorialText.GetChild<GUIImage>() == null)
				{
					new GUIImage(new RectTransform(new Point((int)(tutorialText.Padding.X * 0.8f)), tutorialText.RectTransform, Anchor.CenterLeft, null, ScaleBasis.Normal, false), "ObjectiveIndicatorCompleted", GUIImage.ScalingMode.None);
				}
			}
		}

		// Token: 0x06002574 RID: 9588 RVA: 0x00183980 File Offset: 0x00181B80
		private bool ChangeMaxPlayers(GUIButton button, object obj)
		{
			int currMaxPlayers;
			int.TryParse(this.maxPlayersBox.Text, out currMaxPlayers);
			currMaxPlayers = MathHelper.Clamp(currMaxPlayers + (int)button.UserData, 1, NetConfig.MaxPlayers);
			this.maxPlayersBox.Text = currMaxPlayers.ToString();
			return true;
		}

		// Token: 0x06002575 RID: 9589 RVA: 0x001839CC File Offset: 0x00181BCC
		private void TryStartServer()
		{
			if (SubmarineInfo.SavedSubmarines.Any((SubmarineInfo s) => s.CalculatingHash))
			{
				GUIMessageBox waitBox = new GUIMessageBox(TextManager.Get("pleasewait"), TextManager.Get("waitforsubmarinehashcalculations"), new LocalizedString[]
				{
					TextManager.Get("cancel")
				}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
				CoroutineHandle waitCoroutine = CoroutineManager.StartCoroutine(this.WaitForSubmarineHashCalculations(waitBox), "WaitForSubmarineHashCalculations");
				GUIButton guibutton = waitBox.Buttons[0];
				guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object userdata)
				{
					CoroutineManager.StopCoroutines(waitCoroutine);
					return true;
				}));
				return;
			}
			this.StartServer();
		}

		// Token: 0x06002576 RID: 9590 RVA: 0x00183AB2 File Offset: 0x00181CB2
		private IEnumerable<CoroutineStatus> WaitForSubmarineHashCalculations(GUIMessageBox messageBox)
		{
			MainMenuScreen.<WaitForSubmarineHashCalculations>d__60 <WaitForSubmarineHashCalculations>d__ = new MainMenuScreen.<WaitForSubmarineHashCalculations>d__60(-2);
			<WaitForSubmarineHashCalculations>d__.<>4__this = this;
			<WaitForSubmarineHashCalculations>d__.<>3__messageBox = messageBox;
			return <WaitForSubmarineHashCalculations>d__;
		}

		// Token: 0x06002577 RID: 9591 RVA: 0x00183ACC File Offset: 0x00181CCC
		private void StartServer()
		{
			string name = this.serverNameBox.Text;
			GameMain.ResetNetLobbyScreen();
			try
			{
				GUIComponent selectedComponent = this.serverExecutableDropdown.SelectedComponent;
				ServerExecutableFile f = ((selectedComponent != null) ? selectedComponent.UserData : null) as ServerExecutableFile;
				string fileName;
				if (f != null && f.ContentPackage != GameMain.VanillaContent)
				{
					fileName = Path.Combine(new string[]
					{
						Path.GetDirectoryName(f.Path.Value),
						Path.GetFileNameWithoutExtension(f.Path.Value)
					});
					fileName += ".exe";
				}
				else
				{
					fileName = "DedicatedServer.exe";
				}
				List<string> arguments = new List<string>
				{
					"-name",
					name,
					"-public",
					this.isPublicBox.Selected.ToString(),
					"-playstyle",
					((PlayStyle)this.playstyleBanner.UserData).ToString(),
					"-banafterwrongpassword",
					this.wrongPasswordBanBox.Selected.ToString(),
					"-karmaenabled",
					this.karmaBox.Selected.ToString(),
					"-maxplayers",
					this.maxPlayersBox.Text,
					"-language",
					this.languageDropdown.SelectedData.ToString()
				};
				if (!string.IsNullOrWhiteSpace(this.passwordBox.Text))
				{
					arguments.Add("-password");
					arguments.Add(this.passwordBox.Text);
				}
				else
				{
					arguments.Add("-nopassword");
				}
				ImmutableArray<EosInterface.ProductUserId> puids = EosInterface.IdQueries.GetLoggedInPuids();
				List<Endpoint> endpoints = new List<Endpoint>();
				SteamId steamId;
				if (SteamManager.GetSteamId().TryUnwrap(out steamId))
				{
					endpoints.Add(new SteamP2PEndpoint(steamId));
				}
				if (puids.Length > 0)
				{
					endpoints.Add(new EosP2PEndpoint(puids[0]));
				}
				if (endpoints.Count == 0)
				{
					endpoints.Add(new LidgrenEndpoint(IPAddress.Loopback, 27015));
				}
				P2PEndpoint firstEndpoint = endpoints.First<Endpoint>() as P2PEndpoint;
				if (firstEndpoint != null)
				{
					arguments.Add("-endpoint");
					arguments.Add(firstEndpoint.StringRepresentation);
				}
				int ownerKey = Math.Max(CryptoRandom.Instance.Next(), 1);
				arguments.Add("-ownerkey");
				arguments.Add(ownerKey.ToString());
				if (NetConfig.UseLenientHandshake)
				{
					arguments.Add("-lenienthandshake");
				}
				ProcessStartInfo processInfo = new ProcessStartInfo
				{
					FileName = fileName,
					WorkingDirectory = Directory.GetCurrentDirectory(),
					CreateNoWindow = true,
					UseShellExecute = false,
					WindowStyle = ProcessWindowStyle.Hidden
				};
				arguments.ForEach(new Action<string>(processInfo.ArgumentList.Add));
				ChildServerRelay.Start(processInfo);
				Thread.Sleep(1000);
				GameMain.Client = new GameClient(MultiplayerPreferences.Instance.PlayerName.FallbackNullOrEmpty(SteamManager.GetUsername().FallbackNullOrEmpty(name)), endpoints.ToImmutableArray<Endpoint>(), name, Option.Some<int>(ownerKey));
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Failed to start server", e, null, false, false);
			}
		}

		// Token: 0x06002578 RID: 9592 RVA: 0x00183E1C File Offset: 0x0018201C
		private bool QuitClicked(GUIButton button, object obj)
		{
			this.game.Exit();
			return true;
		}

		// Token: 0x06002579 RID: 9593 RVA: 0x00183E2C File Offset: 0x0018202C
		private void UpdateOutOfDateWorkshopItemCount()
		{
			if (DateTime.Now < this.modUpdateStatus.Item1)
			{
				return;
			}
			if (!SteamManager.IsInitialized)
			{
				return;
			}
			ContentPackageManager.PackageSource installedPackages = ContentPackageManager.WorkshopPackages;
			IEnumerable<ulong> ids = (from id in SteamManager.Workshop.GetSubscribedItemIds()
			select id.Value).Union(from id in (from pkg in installedPackages
			select pkg.UgcId).NotNone<ContentPackageId>().OfType<SteamWorkshopId>()
			select id.Value);
			int count = (from id in ids
			select new Item(id)).Count(delegate(Item item)
			{
				ContentPackage pkg = installedPackages.FirstOrDefault(delegate(ContentPackage p)
				{
					SteamWorkshopId id;
					return p.UgcId.TryUnwrap<SteamWorkshopId>(out id) && id.Value == item.Id;
				});
				DateTime workshopInstallTime;
				SerializableDateTime localInstallTime;
				return pkg != null && (item.IsDownloading || item.IsDownloadPending || (item.InstallTime.TryGetValue(out workshopInstallTime) && pkg.InstallTime.TryUnwrap(out localInstallTime) && localInstallTime.ToUtcValue() < workshopInstallTime));
			});
			this.modUpdateStatus = new ValueTuple<DateTime, int>(DateTime.Now + MainMenuScreen.ModUpdateInterval, count);
		}

		// Token: 0x0600257A RID: 9594 RVA: 0x00183F43 File Offset: 0x00182143
		private static bool CanHostServer()
		{
			return EosInterface.IdQueries.IsLoggedIntoEosConnect || SteamManager.IsInitialized;
		}

		// Token: 0x0600257B RID: 9595 RVA: 0x00183F54 File Offset: 0x00182154
		public override void Update(double deltaTime)
		{
			this.hostServerButton.Enabled = MainMenuScreen.CanHostServer();
			GUITextBlock guitextBlock = this.gameAnalyticsStatusText;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(20, 1);
			defaultInterpolatedStringHandler.AppendLiteral("GameAnalyticsStatus.");
			defaultInterpolatedStringHandler.AppendFormatted<GameAnalyticsManager.Consent>(GameAnalyticsManager.UserConsented);
			guitextBlock.Text = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
			this.UpdateOutOfDateWorkshopItemCount();
			this.modUpdatesButton.Visible = (this.modUpdateStatus.Item2 > 0);
			if (this.modUpdatesButton.Visible)
			{
				Point modButtonLabelSize = this.modsButton.Font.MeasureString(this.modsButton.Text, false).ToPoint() + new Point(GUI.IntScale(25f));
				this.modUpdatesButton.RectTransform.AbsoluteOffset = new ValueTuple<int, int>(modButtonLabelSize.X, this.modsButton.Rect.Height / 2 - this.modUpdatesButton.Rect.Height / 2);
			}
			if (this.selectedTab == MainMenuScreen.Tab.NewGame)
			{
				this.campaignSetupUI.Update();
			}
		}

		// Token: 0x0600257C RID: 9596 RVA: 0x00184070 File Offset: 0x00182270
		public void DrawBackground(GraphicsDevice graphics, SpriteBatch spriteBatch)
		{
			graphics.Clear(Microsoft.Xna.Framework.Color.Black);
			if (this.backgroundSprite == null)
			{
				GUIComponentStyle mainMenuStyle = GUIStyle.GetComponentStyle("MainMenuBackground");
				List<UISprite> sprites;
				if (mainMenuStyle != null && mainMenuStyle.Sprites.TryGetValue(GUIComponent.ComponentState.None, out sprites))
				{
					UISprite randomUnsynced = sprites.GetRandomUnsynced<UISprite>();
					this.backgroundSprite = ((randomUnsynced != null) ? randomUnsynced.Sprite : null);
				}
				if (this.backgroundSprite == null)
				{
					LocationType randomUnsynced2 = LocationType.Prefabs.GetRandomUnsynced<LocationType>();
					this.backgroundSprite = ((randomUnsynced2 != null) ? randomUnsynced2.GetPortrait(0) : null);
				}
			}
			GUIComponentStyle componentStyle = GUIStyle.GetComponentStyle("mainmenuvignette");
			Sprite vignette = (componentStyle != null) ? componentStyle.GetDefaultSprite() : null;
			float vignetteScale = Math.Min((float)GameMain.GraphicsWidth / vignette.size.X, (float)GameMain.GraphicsHeight / vignette.size.Y);
			Rectangle drawArea = new Rectangle((int)(vignette.size.X * vignetteScale / 2f), 0, (int)((float)GameMain.GraphicsWidth - vignette.size.X * vignetteScale / 2f), GameMain.GraphicsHeight);
			Sprite sprite = this.backgroundSprite;
			if (((sprite != null) ? sprite.Texture : null) != null)
			{
				GUI.DrawBackgroundSprite(spriteBatch, this.backgroundSprite, Microsoft.Xna.Framework.Color.White, new Rectangle?(drawArea), SpriteEffects.None);
			}
			if (vignette != null)
			{
				vignette.Draw(spriteBatch, Vector2.Zero, Microsoft.Xna.Framework.Color.White, Vector2.Zero, 0f, vignetteScale, SpriteEffects.None, null);
			}
		}

		// Token: 0x0600257D RID: 9597 RVA: 0x001841C0 File Offset: 0x001823C0
		public override void Draw(double deltaTime, GraphicsDevice graphics, SpriteBatch spriteBatch)
		{
			spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, GameMain.ScissorTestEnable, null, null);
			this.DrawBackground(graphics, spriteBatch);
			GUI.Draw(this.Cam, spriteBatch);
			spriteBatch.End();
		}

		// Token: 0x0600257E RID: 9598 RVA: 0x00184204 File Offset: 0x00182404
		private void StartGame(SubmarineInfo selectedSub, string savePath, string mapSeed, CampaignSettings settings)
		{
			if (string.IsNullOrEmpty(savePath))
			{
				return;
			}
			IReadOnlyList<CampaignMode.SaveInfo> existingSaveFiles = SaveUtil.GetSaveFiles(SaveUtil.SaveType.Singleplayer, true, true);
			if (existingSaveFiles.Any((CampaignMode.SaveInfo s) => s.FilePath == savePath))
			{
				new GUIMessageBox(TextManager.Get("SaveNameInUseHeader"), TextManager.Get("SaveNameInUseText"), null, null, GUIMessageBox.Type.Default);
				return;
			}
			if (selectedSub == null)
			{
				new GUIMessageBox(TextManager.Get("SubNotSelected"), TextManager.Get("SelectSubRequest"), null, null, GUIMessageBox.Type.Default);
				return;
			}
			if (!Directory.Exists(SaveUtil.TempPath))
			{
				Directory.CreateDirectory(SaveUtil.TempPath, true);
			}
			try
			{
				File.Copy(selectedSub.FilePath, Path.Combine(new string[]
				{
					SaveUtil.TempPath,
					selectedSub.Name + ".sub"
				}), true, false);
			}
			catch (IOException e)
			{
				DebugConsole.ThrowError("Copying the file \"" + selectedSub.FilePath + "\" failed. The file may have been deleted or in use by another process. Try again or select another submarine.", e, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("MainMenuScreen.StartGame:IOException" + selectedSub.Name, GameAnalyticsManager.ErrorSeverity.Error, "Copying a submarine file failed. " + e.Message + "\n" + Environment.StackTrace.CleanupStackTrace());
				return;
			}
			selectedSub = new SubmarineInfo(Path.Combine(new string[]
			{
				SaveUtil.TempPath,
				selectedSub.Name + ".sub"
			}), "", null, true, false);
			SubmarineInfo submarineInfo = selectedSub;
			Option.UnspecifiedNone none = Option.None;
			GameMain.GameSession = new GameSession(submarineInfo, none, CampaignDataPath.CreateRegular(savePath), GameModePreset.SinglePlayerCampaign, settings, mapSeed, null);
			GameMain.GameSession.CrewManager.ClearCharacterInfos();
			foreach (CharacterInfo characterInfo in from m in this.campaignSetupUI.CharacterMenus
			select m.CharacterInfo)
			{
				GameMain.GameSession.CrewManager.AddCharacterInfo(characterInfo);
			}
			((SinglePlayerCampaign)GameMain.GameSession.GameMode).LoadNewLevel();
		}

		// Token: 0x0600257F RID: 9599 RVA: 0x0018445C File Offset: 0x0018265C
		private void LoadGame(string path, Option<uint> backupIndex)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				return;
			}
			try
			{
				uint index;
				CampaignDataPath dataPath = backupIndex.TryUnwrap(out index) ? new CampaignDataPath(SaveUtil.GetBackupPath(path, index), path) : CampaignDataPath.CreateRegular(path);
				SaveUtil.LoadGame(dataPath);
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Loading save \"" + path + "\" failed", e, null, false, false);
				GameMain.GameSession = null;
			}
		}

		// Token: 0x06002580 RID: 9600 RVA: 0x001844D0 File Offset: 0x001826D0
		private void CreateCampaignSetupUI()
		{
			this.menuTabs[MainMenuScreen.Tab.NewGame].ClearChildren();
			this.menuTabs[MainMenuScreen.Tab.LoadGame].ClearChildren();
			GUILayoutGroup innerNewGame = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.9f), this.menuTabs[MainMenuScreen.Tab.NewGame].RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.02f
			};
			GUIFrame newGameContent = new GUIFrame(new RectTransform(new Vector2(1f, 0.95f), innerNewGame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "InnerFrame", null);
			GUIFrame paddedLoadGame = new GUIFrame(new RectTransform(new Vector2(0.9f, 0.9f), this.menuTabs[MainMenuScreen.Tab.LoadGame].RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal)
			{
				AbsoluteOffset = new Point(0, 10)
			}, null, null);
			this.campaignSetupUI = new SinglePlayerCampaignSetupUI(newGameContent, paddedLoadGame)
			{
				LoadGame = new CampaignSetupUI.LoadGameDelegate(this.LoadGame),
				StartNewGame = new Action<SubmarineInfo, string, string, CampaignSettings>(this.StartGame)
			};
		}

		// Token: 0x06002581 RID: 9601 RVA: 0x00184640 File Offset: 0x00182840
		private void CreateHostServerFields()
		{
			this.menuTabs[MainMenuScreen.Tab.HostServer].ClearChildren();
			Exception ex;
			XDocument xdocument = XMLExtensions.TryLoadXml("serversettings.xml", out ex);
			XElement serverSettings = ((xdocument != null) ? xdocument.Root : null) ?? new XElement("serversettings");
			string name = serverSettings.GetAttributeString("ServerName", "");
			string password = serverSettings.GetAttributeString("password", "");
			bool isPublic = serverSettings.GetAttributeBool("IsPublic", true);
			bool banAfterWrongPassword = serverSettings.GetAttributeBool("banafterwrongpassword", false);
			int maxPlayersElement = serverSettings.GetAttributeInt("maxplayers", 8);
			if (maxPlayersElement > NetConfig.MaxPlayers)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(130, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Setting the maximum amount of players to ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(maxPlayersElement);
				defaultInterpolatedStringHandler.AppendLiteral(" failed due to exceeding the limit of ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(NetConfig.MaxPlayers);
				defaultInterpolatedStringHandler.AppendLiteral(" players per server. Using the maximum of ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(NetConfig.MaxPlayers);
				defaultInterpolatedStringHandler.AppendLiteral(" instead.");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
			}
			int maxPlayers = Math.Clamp(maxPlayersElement, 1, NetConfig.MaxPlayers);
			bool karmaEnabled = serverSettings.GetAttributeBool("karmaenabled", false);
			PlayStyle selectedPlayStyle = serverSettings.GetAttributeEnum("playstyle", PlayStyle.Casual);
			Vector2 textLabelSize = new Vector2(1f, 0.05f);
			Alignment textAlignment = Alignment.CenterLeft;
			Vector2 textFieldSize = new Vector2(0.5f, 1f);
			Vector2 tickBoxSize = new Vector2(0.4f, 0.04f);
			GUILayoutGroup content = new GUILayoutGroup(new RectTransform(new Vector2(0.7f, 0.95f), this.menuTabs[MainMenuScreen.Tab.HostServer].RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopCenter)
			{
				RelativeSpacing = 0.01f,
				Stretch = true
			};
			GUIComponent parent = content;
			RectTransform rectT = new RectTransform(textLabelSize, parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text10 = TextManager.Get("HostServerButton");
			GUIFont font = GUIStyle.LargeFont;
			GUITextBlock header = new GUITextBlock(rectT, text10, null, font, Alignment.Center, false, "", null)
			{
				ForceUpperCase = ForceUpperCase.Yes
			};
			header.RectTransform.IsFixedSize = true;
			GUIFrame playstyleContainer = new GUIFrame(new RectTransform(new Vector2(1.35f, 0.1f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Black));
			RectTransform rectT2 = new RectTransform(new Vector2(1f, 0.1f), playstyleContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(16, 1);
			defaultInterpolatedStringHandler2.AppendLiteral("PlayStyleBanner.");
			defaultInterpolatedStringHandler2.AppendFormatted<PlayStyle>(PlayStyle.Serious);
			this.playstyleBanner = new GUIImage(rectT2, GUIStyle.GetComponentStyle(defaultInterpolatedStringHandler2.ToStringAndClear()).GetSprite(GUIComponent.ComponentState.None), null, GUIImage.ScalingMode.ScaleToFitSmallestExtent)
			{
				UserData = PlayStyle.Serious
			};
			float bannerAspectRatio = (float)this.playstyleBanner.Sprite.SourceRect.Width / (float)this.playstyleBanner.Sprite.SourceRect.Height;
			this.playstyleBanner.RectTransform.NonScaledSize = new Point(this.playstyleBanner.Rect.Width, (int)((float)this.playstyleBanner.Rect.Width / bannerAspectRatio));
			this.playstyleBanner.RectTransform.IsFixedSize = true;
			new GUIFrame(new RectTransform(this.playstyleBanner.Rect.Size + new Point(1), this.playstyleBanner.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), "InnerGlow", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Black));
			RectTransform rectTransform = new RectTransform(new Vector2(0.15f, 0.05f), this.playstyleBanner.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			rectTransform.RelativeOffset = new Vector2(0.01f, 0.03f);
			RichString text2 = "playstyle name goes here";
			font = GUIStyle.SmallFont;
			new GUITextBlock(rectTransform, text2, new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.White), font, Alignment.Center, false, "GUISlopedHeader", null);
			new GUIButton(new RectTransform(new Vector2(0.05f, 1f), playstyleContainer.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0.02f, 0f),
				MaxSize = new Point(int.MaxValue, (int)(150f * GUI.Scale))
			}, Alignment.Center, "UIToggleButton", null)
			{
				OnClicked = delegate(GUIButton btn, object userdata)
				{
					int playStyleIndex = (int)this.playstyleBanner.UserData - 1;
					if (playStyleIndex < 0)
					{
						playStyleIndex = Enum.GetValues(typeof(PlayStyle)).Length - 1;
					}
					this.SetServerPlayStyle((PlayStyle)playStyleIndex);
					return true;
				}
			}.Children.ForEach(delegate(GUIComponent c)
			{
				c.SpriteEffects = SpriteEffects.FlipHorizontally;
			});
			new GUIButton(new RectTransform(new Vector2(0.05f, 1f), playstyleContainer.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0.02f, 0f),
				MaxSize = new Point(int.MaxValue, (int)(150f * GUI.Scale))
			}, Alignment.Center, "UIToggleButton", null).OnClicked = delegate(GUIButton btn, object userdata)
			{
				int playStyleIndex = (int)this.playstyleBanner.UserData + 1;
				if (playStyleIndex >= Enum.GetValues(typeof(PlayStyle)).Length)
				{
					playStyleIndex = 0;
				}
				this.SetServerPlayStyle((PlayStyle)playStyleIndex);
				return true;
			};
			LocalizedString longestPlayStyleStr = "";
			foreach (object obj in Enum.GetValues(typeof(PlayStyle)))
			{
				LocalizedString playStyleStr = TextManager.Get("servertagdescription." + ((PlayStyle)obj).ToString());
				if (playStyleStr.Length > longestPlayStyleStr.Length)
				{
					longestPlayStyleStr = playStyleStr;
				}
			}
			this.playstyleDescription = new GUITextBlock(new RectTransform(new Vector2(1f, 0.05f), playstyleContainer.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal), longestPlayStyleStr, null, null, Alignment.Left, true, null, null)
			{
				Color = Microsoft.Xna.Framework.Color.Black * 0.8f,
				TextColor = GUIStyle.GetComponentStyle("GUITextBlock").TextColor
			};
			this.playstyleDescription.Padding = Vector4.One * 10f * GUI.Scale;
			this.playstyleDescription.CalculateHeightFromText((int)(15f * GUI.Scale), false);
			this.playstyleDescription.RectTransform.NonScaledSize = new Point(this.playstyleDescription.Rect.Width, this.playstyleDescription.Rect.Height);
			this.playstyleDescription.RectTransform.IsFixedSize = true;
			playstyleContainer.RectTransform.NonScaledSize = new Point(playstyleContainer.Rect.Width, this.playstyleBanner.Rect.Height + this.playstyleDescription.Rect.Height);
			playstyleContainer.RectTransform.IsFixedSize = true;
			this.SetServerPlayStyle(selectedPlayStyle);
			new GUIFrame(new RectTransform(new Vector2(1f, 0.025f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			RectTransform rectT3 = new RectTransform(textLabelSize, parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text3 = TextManager.Get("ServerName");
			Alignment textAlignment2 = textAlignment;
			GUITextBlock label = new GUITextBlock(rectT3, text3, null, null, textAlignment2, false, "", null);
			RectTransform rectT4 = new RectTransform(textFieldSize, label.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal);
			string text4 = name;
			textAlignment2 = textAlignment;
			this.serverNameBox = new GUITextBox(rectT4, text4, null, null, textAlignment2, false, "", null, false, true)
			{
				MaxTextLength = new int?(NetConfig.ServerNameMaxLength),
				OverflowClip = true
			};
			label.RectTransform.IsFixedSize = true;
			RectTransform rectT5 = new RectTransform(textLabelSize, parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text5 = TextManager.Get("MaxPlayers");
			textAlignment2 = textAlignment;
			GUITextBlock maxPlayersLabel = new GUITextBlock(rectT5, text5, null, null, textAlignment2, false, "", null);
			GUILayoutGroup buttonContainer = new GUILayoutGroup(new RectTransform(textFieldSize, maxPlayersLabel.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.1f
			};
			GUIButton guibutton = new GUIButton(new RectTransform(Vector2.One, buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), Alignment.Center, "GUIMinusButton", null);
			guibutton.UserData = -1;
			guibutton.OnClicked = new GUIButton.OnClickedHandler(this.ChangeMaxPlayers);
			guibutton.ClickSound = GUISoundType.Decrease;
			this.maxPlayersBox = new GUITextBox(new RectTransform(new Vector2(0.6f, 1f), buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Center, false, "", null, false, true)
			{
				Text = maxPlayers.ToString()
			};
			GUITextBox guitextBox = this.maxPlayersBox;
			guitextBox.OnEnterPressed = (GUITextBox.OnEnterHandler)Delegate.Combine(guitextBox.OnEnterPressed, new GUITextBox.OnEnterHandler(delegate(GUITextBox sender, string text)
			{
				this.maxPlayersBox.Deselect();
				return true;
			}));
			this.maxPlayersBox.OnDeselected += delegate(GUITextBox sender, Keys key)
			{
				int currMaxPlayers;
				int.TryParse(this.maxPlayersBox.Text, out currMaxPlayers);
				currMaxPlayers = MathHelper.Clamp(currMaxPlayers, 1, NetConfig.MaxPlayers);
				this.maxPlayersBox.Text = currMaxPlayers.ToString();
			};
			GUIButton guibutton2 = new GUIButton(new RectTransform(Vector2.One, buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), Alignment.Center, "GUIPlusButton", null);
			guibutton2.UserData = 1;
			guibutton2.OnClicked = new GUIButton.OnClickedHandler(this.ChangeMaxPlayers);
			guibutton2.ClickSound = GUISoundType.Increase;
			maxPlayersLabel.RectTransform.IsFixedSize = true;
			RectTransform rectT6 = new RectTransform(textLabelSize, parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text6 = TextManager.Get("Password");
			textAlignment2 = textAlignment;
			label = new GUITextBlock(rectT6, text6, null, null, textAlignment2, false, "", null);
			RectTransform rectT7 = new RectTransform(textFieldSize, label.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal);
			string text7 = password;
			textAlignment2 = textAlignment;
			this.passwordBox = new GUITextBox(rectT7, text7, null, null, textAlignment2, false, "", null, false, true)
			{
				Censor = true
			};
			label.RectTransform.IsFixedSize = true;
			RectTransform rectT8 = new RectTransform(textLabelSize, parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text8 = TextManager.Get("Language");
			textAlignment2 = textAlignment;
			GUITextBlock languageLabel = new GUITextBlock(rectT8, text8, null, null, textAlignment2, false, "", null);
			this.languageDropdown = new GUIDropDown(new RectTransform(textFieldSize, languageLabel.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), null, 4, "", false, false, Alignment.CenterLeft, 1f);
			foreach (ServerLanguageOptions.LanguageOption language in ServerLanguageOptions.Options)
			{
				this.languageDropdown.AddItem(language.Label, language.Identifier, null, null, null);
			}
			LanguageIdentifier defaultLanguage = ServerLanguageOptions.PickLanguage(GameSettings.CurrentConfig.Language);
			LanguageIdentifier settingsLanguage = serverSettings.GetAttributeIdentifier("language", defaultLanguage.Value).ToLanguageIdentifier();
			if (!ServerLanguageOptions.Options.Any((ServerLanguageOptions.LanguageOption o) => o.Identifier == settingsLanguage))
			{
				settingsLanguage = defaultLanguage;
			}
			this.languageDropdown.Select(ServerLanguageOptions.Options.FindIndex((ServerLanguageOptions.LanguageOption o) => o.Identifier == settingsLanguage));
			RectTransform rectT9 = new RectTransform(textLabelSize, parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text9 = TextManager.Get("ServerExecutable");
			textAlignment2 = textAlignment;
			GUITextBlock serverExecutableLabel = new GUITextBlock(rectT9, text9, null, null, textAlignment2, false, "", null);
			this.serverExecutableDropdown = new GUIDropDown(new RectTransform(textFieldSize, serverExecutableLabel.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), "Vanilla", 4, "", false, false, Alignment.CenterLeft, 1f);
			Vector2 listBoxSize = this.serverExecutableDropdown.ListBox.RectTransform.RelativeSize;
			this.serverExecutableDropdown.ListBox.RectTransform.RelativeSize = new Vector2(listBoxSize.X * 1.5f, listBoxSize.Y);
			this.serverExecutableDropdown.AddItem("Vanilla", null, null, null, null);
			this.serverExecutableDropdown.OnSelected = delegate(GUIComponent selected, object userData)
			{
				if (userData != null)
				{
					GUIMessageBox warningBox = new GUIMessageBox(TextManager.Get("Warning"), TextManager.GetWithVariable("ModServerExesAtYourOwnRisk", "[exename]", this.serverExecutableDropdown.Text, FormatCapitals.No), new LocalizedString[]
					{
						TextManager.Get("Yes"),
						TextManager.Get("No")
					}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
					warningBox.Buttons[0].OnClicked = delegate(GUIButton _, object __)
					{
						warningBox.Close();
						return false;
					};
					warningBox.Buttons[1].OnClicked = delegate(GUIButton _, object __)
					{
						this.serverExecutableDropdown.Select(0);
						warningBox.Close();
						return false;
					};
				}
				this.serverExecutableDropdown.Text = ToolBox.LimitString(this.serverExecutableDropdown.Text, this.serverExecutableDropdown.Font, this.serverExecutableDropdown.Rect.Width * 8 / 10);
				return true;
			};
			serverExecutableLabel.RectTransform.IsFixedSize = true;
			GUILayoutGroup tickboxAreaUpper = new GUILayoutGroup(new RectTransform(new Vector2(1f, tickBoxSize.Y), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			this.isPublicBox = new GUITickBox(new RectTransform(new Vector2(0.5f, 1f), tickboxAreaUpper.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("PublicServer"), null, "")
			{
				Selected = isPublic,
				ToolTip = TextManager.Get("PublicServerToolTip")
			};
			this.wrongPasswordBanBox = new GUITickBox(new RectTransform(new Vector2(0.5f, 1f), tickboxAreaUpper.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsBanAfterWrongPassword"), null, "")
			{
				Selected = banAfterWrongPassword
			};
			tickboxAreaUpper.RectTransform.IsFixedSize = true;
			GUILayoutGroup tickboxAreaLower = new GUILayoutGroup(new RectTransform(new Vector2(1f, tickBoxSize.Y), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			this.karmaBox = new GUITickBox(new RectTransform(new Vector2(0.5f, 1f), tickboxAreaLower.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("HostServerKarmaSetting"), null, "")
			{
				Selected = karmaEnabled,
				ToolTip = TextManager.Get("hostserverkarmasettingtooltip")
			};
			tickboxAreaLower.RectTransform.IsFixedSize = true;
			new GUIFrame(new RectTransform(new Vector2(1f, 0.05f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			new GUIButton(new RectTransform(new Vector2(0.4f, 0.07f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("StartServerButton"), Alignment.Center, "GUIButtonLarge", null).OnClicked = delegate(GUIButton btn, object userdata)
			{
				base.<CreateHostServerFields>g__CheckServerName|5();
				return true;
			};
		}

		// Token: 0x06002582 RID: 9602 RVA: 0x001857CC File Offset: 0x001839CC
		private void SetServerPlayStyle(PlayStyle playStyle)
		{
			GUIImage guiimage = this.playstyleBanner;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 1);
			defaultInterpolatedStringHandler.AppendLiteral("PlayStyleBanner.");
			defaultInterpolatedStringHandler.AppendFormatted<PlayStyle>(playStyle);
			guiimage.Sprite = GUIStyle.GetComponentStyle(defaultInterpolatedStringHandler.ToStringAndClear()).GetSprite(GUIComponent.ComponentState.None);
			this.playstyleBanner.UserData = playStyle;
			GUITextBlock nameText = this.playstyleBanner.GetChild<GUITextBlock>();
			nameText.Text = TextManager.AddPunctuation(':', new LocalizedString[]
			{
				TextManager.Get("serverplaystyle"),
				TextManager.Get("servertag." + playStyle.ToString())
			});
			nameText.Color = (this.playstyleBanner.Sprite.SourceElement.GetAttributeColor("BannerColor") ?? Microsoft.Xna.Framework.Color.White);
			nameText.RectTransform.NonScaledSize = (nameText.Font.MeasureString(nameText.Text, false) + new Vector2(25f, 10f) * GUI.Scale).ToPoint();
			this.playstyleDescription.Text = TextManager.Get("servertagdescription." + playStyle.ToString());
			this.playstyleDescription.TextAlignment = (this.playstyleDescription.WrappedText.Contains('\n', StringComparison.Ordinal) ? Alignment.CenterLeft : Alignment.Center);
		}

		// Token: 0x06002583 RID: 9603 RVA: 0x00185948 File Offset: 0x00183B48
		private void FetchRemoteContent()
		{
			string remoteContentUrl = GameSettings.CurrentConfig.RemoteMainMenuContentUrl;
			if (string.IsNullOrEmpty(remoteContentUrl))
			{
				return;
			}
			try
			{
				RestClient client = RestFactory.CreateClient(remoteContentUrl);
				RestRequest request = RestFactory.CreateRequest("MenuContent.xml", Method.GET);
				TaskPool.Add("RequestMainMenuRemoteContent", client.ExecuteAsync(request, default(CancellationToken)), new Action<Task>(this.RemoteContentReceived));
			}
			catch (Exception e)
			{
				GameAnalyticsManager.AddErrorEventOnce("MainMenuScreen.FetchRemoteContent:Exception", GameAnalyticsManager.ErrorSeverity.Error, "Fetching remote content to the main menu failed. " + e.Message);
			}
		}

		// Token: 0x06002584 RID: 9604 RVA: 0x001859D8 File Offset: 0x00183BD8
		private void RemoteContentReceived(Task t)
		{
			try
			{
				IRestResponse remoteContentResponse;
				if (!t.TryGetResult(out remoteContentResponse))
				{
					throw new Exception("Task did not return a valid result");
				}
				if (remoteContentResponse.ErrorException != null)
				{
					DebugConsole.AddWarning("Connection error: Failed to fetch remote main menu content (" + remoteContentResponse.ErrorException.Message + ").", null);
				}
				else if (remoteContentResponse.StatusCode != HttpStatusCode.OK)
				{
					string str = "Failed to receive remote main menu content. ";
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(65, 1);
					defaultInterpolatedStringHandler.AppendLiteral("The master server might be temporarily unavailable (HTTP error: ");
					defaultInterpolatedStringHandler.AppendFormatted<HttpStatusCode>(remoteContentResponse.StatusCode);
					defaultInterpolatedStringHandler.AppendLiteral(")");
					DebugConsole.AddWarning(str + defaultInterpolatedStringHandler.ToStringAndClear(), null);
				}
				else
				{
					string xml = remoteContentResponse.Content;
					int index = xml.IndexOf('<');
					if (index > 0)
					{
						xml = xml.Substring(index, xml.Length - index);
					}
					if (!string.IsNullOrWhiteSpace(xml))
					{
						this.remoteContentDoc = XDocument.Parse(xml);
						XDocument xdocument = this.remoteContentDoc;
						foreach (XElement subElement in ((xdocument != null) ? xdocument.Root.Elements() : null))
						{
							GUIComponent.FromXML(subElement.FromPackage(null), this.remoteContentContainer.RectTransform);
						}
					}
				}
			}
			catch (Exception e)
			{
				GameAnalyticsManager.AddErrorEventOnce("MainMenuScreen.RemoteContentReceived:Exception", GameAnalyticsManager.ErrorSeverity.Error, "Reading received remote main menu content failed. " + e.Message);
			}
		}

		// Token: 0x06002586 RID: 9606 RVA: 0x00185B94 File Offset: 0x00183D94
		[CompilerGenerated]
		private GUILayoutGroup <.ctor>g__createTextFooter|40_3()
		{
			return new GUILayoutGroup(new RectTransform(new ValueTuple<float, float>(1f, 0.06f), this.Frame.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				ChildAnchor = Anchor.BottomLeft
			};
		}

		// Token: 0x06002587 RID: 9607 RVA: 0x00185BF0 File Offset: 0x00183DF0
		[CompilerGenerated]
		internal static GUITextBlock <.ctor>g__createTextInFooter|40_4(GUILayoutGroup footer, LocalizedString str, Alignment textAlignment)
		{
			RectTransform rectT = new RectTransform(new ValueTuple<float, float>(1f, 0.3f), footer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = str;
			GUIFont smallFont = GUIStyle.SmallFont;
			GUITextBlock textBlock = new GUITextBlock(rectT, text, new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.White * 0.7f), smallFont, textAlignment, false, "", null);
			textBlock.RectTransform.SetAsFirstChild();
			return textBlock;
		}

		// Token: 0x06002588 RID: 9608 RVA: 0x00185C81 File Offset: 0x00183E81
		[CompilerGenerated]
		private GUITextBlock <.ctor>g__createLeftText|40_5(LocalizedString str)
		{
			return MainMenuScreen.<.ctor>g__createTextInFooter|40_4(this.leftTextFooterLayout, str, Alignment.BottomLeft);
		}

		// Token: 0x06002589 RID: 9609 RVA: 0x00185C91 File Offset: 0x00183E91
		[CompilerGenerated]
		private GUITextBlock <.ctor>g__createRightText|40_6(LocalizedString str)
		{
			return MainMenuScreen.<.ctor>g__createTextInFooter|40_4(this.rightTextFooterLayout, str, Alignment.BottomRight);
		}

		// Token: 0x040012B2 RID: 4786
		public static HashSet<Identifier> DismissedNotifications = new HashSet<Identifier>();

		// Token: 0x040012B3 RID: 4787
		private readonly GUIComponent buttonsParent;

		// Token: 0x040012B4 RID: 4788
		private readonly Dictionary<MainMenuScreen.Tab, GUIFrame> menuTabs;

		// Token: 0x040012B5 RID: 4789
		private SinglePlayerCampaignSetupUI campaignSetupUI;

		// Token: 0x040012B6 RID: 4790
		private GUITextBox serverNameBox;

		// Token: 0x040012B7 RID: 4791
		private GUITextBox passwordBox;

		// Token: 0x040012B8 RID: 4792
		private GUITextBox maxPlayersBox;

		// Token: 0x040012B9 RID: 4793
		private GUITickBox isPublicBox;

		// Token: 0x040012BA RID: 4794
		private GUITickBox wrongPasswordBanBox;

		// Token: 0x040012BB RID: 4795
		private GUITickBox karmaBox;

		// Token: 0x040012BC RID: 4796
		private GUIDropDown languageDropdown;

		// Token: 0x040012BD RID: 4797
		private GUIDropDown serverExecutableDropdown;

		// Token: 0x040012BE RID: 4798
		private readonly GUIButton joinServerButton;

		// Token: 0x040012BF RID: 4799
		private readonly GUIButton hostServerButton;

		// Token: 0x040012C0 RID: 4800
		private readonly GUIFrame modsButtonContainer;

		// Token: 0x040012C1 RID: 4801
		private readonly GUIButton modsButton;

		// Token: 0x040012C2 RID: 4802
		private readonly GUIButton modUpdatesButton;

		// Token: 0x040012C3 RID: 4803
		[TupleElementNames(new string[]
		{
			"WhenToRefresh",
			"Count"
		})]
		private ValueTuple<DateTime, int> modUpdateStatus = new ValueTuple<DateTime, int>(DateTime.Now, 0);

		// Token: 0x040012C4 RID: 4804
		private static readonly TimeSpan ModUpdateInterval = TimeSpan.FromSeconds(60.0);

		// Token: 0x040012C5 RID: 4805
		private readonly GameMain game;

		// Token: 0x040012C6 RID: 4806
		private GUIImage playstyleBanner;

		// Token: 0x040012C7 RID: 4807
		private GUITextBlock playstyleDescription;

		// Token: 0x040012C8 RID: 4808
		private readonly GUIComponent remoteContentContainer;

		// Token: 0x040012C9 RID: 4809
		private XDocument remoteContentDoc;

		// Token: 0x040012CA RID: 4810
		private MainMenuScreen.Tab selectedTab = MainMenuScreen.Tab.Empty;

		// Token: 0x040012CB RID: 4811
		private Sprite backgroundSprite;

		// Token: 0x040012CC RID: 4812
		private readonly GUIComponent titleText;

		// Token: 0x040012CD RID: 4813
		private readonly CreditsPlayer creditsPlayer;

		// Token: 0x040012CE RID: 4814
		public static readonly Queue<ulong> WorkshopItemsToUpdate = new Queue<ulong>();

		// Token: 0x040012CF RID: 4815
		private GUIImage tutorialBanner;

		// Token: 0x040012D0 RID: 4816
		private GUITextBlock tutorialHeader;

		// Token: 0x040012D1 RID: 4817
		private GUITextBlock tutorialDescription;

		// Token: 0x040012D2 RID: 4818
		private GUIListBox tutorialList;

		// Token: 0x040012D3 RID: 4819
		private readonly GUITextBlock gameAnalyticsStatusText;

		// Token: 0x040012D4 RID: 4820
		private readonly GUILayoutGroup leftTextFooterLayout;

		// Token: 0x040012D5 RID: 4821
		private readonly GUILayoutGroup rightTextFooterLayout;

		// Token: 0x040012D6 RID: 4822
		private GUIComponent versionMismatchWarning;

		// Token: 0x02000C58 RID: 3160
		private enum Tab
		{
			// Token: 0x04004AC6 RID: 19142
			NewGame,
			// Token: 0x04004AC7 RID: 19143
			LoadGame,
			// Token: 0x04004AC8 RID: 19144
			HostServer,
			// Token: 0x04004AC9 RID: 19145
			Settings,
			// Token: 0x04004ACA RID: 19146
			Tutorials,
			// Token: 0x04004ACB RID: 19147
			JoinServer,
			// Token: 0x04004ACC RID: 19148
			CharacterEditor,
			// Token: 0x04004ACD RID: 19149
			SubmarineEditor,
			// Token: 0x04004ACE RID: 19150
			Mods,
			// Token: 0x04004ACF RID: 19151
			Credits,
			// Token: 0x04004AD0 RID: 19152
			Empty
		}

		// Token: 0x02000C59 RID: 3161
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04004AD1 RID: 19153
			public static Action <0>__SyncBetweenPlatforms;
		}
	}
}
