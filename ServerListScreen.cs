using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Networking;
using Barotrauma.Steam;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Steamworks;

namespace Barotrauma
{
	// Token: 0x0200011C RID: 284
	internal sealed class ServerListScreen : Screen
	{
		// Token: 0x17000A37 RID: 2615
		// (get) Token: 0x060026A6 RID: 9894 RVA: 0x0019986F File Offset: 0x00197A6F
		// (set) Token: 0x060026A7 RID: 9895 RVA: 0x00199878 File Offset: 0x00197A78
		private ServerListScreen.TabEnum selectedTab
		{
			get
			{
				return this._selectedTabBackingField;
			}
			set
			{
				this._selectedTabBackingField = value;
				this.tabs.ForEach(delegate(KeyValuePair<ServerListScreen.TabEnum, ServerListScreen.Tab> kvp)
				{
					kvp.Value.Button.Selected = (value == kvp.Key);
				});
				if (Screen.Selected == this)
				{
					this.RefreshServers();
				}
			}
		}

		// Token: 0x17000A38 RID: 2616
		// (get) Token: 0x060026A8 RID: 9896 RVA: 0x001998C3 File Offset: 0x00197AC3
		// (set) Token: 0x060026A9 RID: 9897 RVA: 0x001998CB File Offset: 0x00197ACB
		public GUITextBox ClientNameBox { get; private set; }

		// Token: 0x060026AA RID: 9898 RVA: 0x001998D4 File Offset: 0x00197AD4
		public ServerListScreen()
		{
			this.selectedServer = Option<ServerInfo>.None();
			GameMain.Instance.ResolutionChanged += this.CreateUI;
			this.CreateUI();
		}

		// Token: 0x060026AB RID: 9899 RVA: 0x0019994C File Offset: 0x00197B4C
		private static Task<string> GetDefaultUserName()
		{
			return new CompositeFriendProvider(new FriendProvider[]
			{
				new SteamFriendProvider(),
				new EpicFriendProvider()
			}).GetSelfUserName();
		}

		// Token: 0x060026AC RID: 9900 RVA: 0x00199970 File Offset: 0x00197B70
		private void AddTernaryFilter(RectTransform parent, float elementHeight, Identifier tag, Action<ServerListScreen.TernaryOption> valueSetter)
		{
			GUILayoutGroup filterLayoutGroup = new GUILayoutGroup(new RectTransform(new Vector2(1f, elementHeight), parent, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			GUIFrame box = new GUIFrame(new RectTransform(Vector2.One, filterLayoutGroup.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.BothHeight)
			{
				IsFixedSize = true
			}, null, null)
			{
				HoverColor = Color.Gray,
				SelectedColor = Color.DarkGray,
				CanBeFocused = false
			};
			if (box.RectTransform.MinSize.Y > 0)
			{
				box.RectTransform.MinSize = new Point(box.RectTransform.MinSize.Y);
				box.RectTransform.Resize(box.RectTransform.MinSize, true);
			}
			Vector2 textBlockScale = new Vector2((float)(filterLayoutGroup.Rect.Width - filterLayoutGroup.Rect.Height) / (float)Math.Max((double)filterLayoutGroup.Rect.Width, 1.0), 1f);
			GUITextBlock guitextBlock = new GUITextBlock(new RectTransform(new Vector2(0.6f, 1f) * textBlockScale, filterLayoutGroup.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("servertag." + tag.ToString() + ".label"), null, null, Alignment.CenterLeft, false, "", null);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 1);
			defaultInterpolatedStringHandler.AppendLiteral("servertag.");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(tag);
			defaultInterpolatedStringHandler.AppendLiteral(".label");
			guitextBlock.UserData = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
			GUITextBlock filterLabel = guitextBlock;
			GUIStyle.Apply(filterLabel, "GUITextBlock", null);
			GUIDropDown dropDown = new GUIDropDown(new RectTransform(new Vector2(0.4f, 1f) * textBlockScale, filterLayoutGroup.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), null, 3, "", false, false, Alignment.CenterLeft, 1f);
			dropDown.AddItem(TextManager.Get("any"), ServerListScreen.TernaryOption.Any, null, null, null);
			GUIDropDown guidropDown = dropDown;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(15, 1);
			defaultInterpolatedStringHandler2.AppendLiteral("servertag.");
			defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(tag);
			defaultInterpolatedStringHandler2.AppendLiteral(".true");
			LocalizedString text = TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear());
			object userData = ServerListScreen.TernaryOption.Enabled;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(26, 1);
			defaultInterpolatedStringHandler3.AppendLiteral("servertagdescription.");
			defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(tag);
			defaultInterpolatedStringHandler3.AppendLiteral(".true");
			guidropDown.AddItem(text, userData, TextManager.Get(defaultInterpolatedStringHandler3.ToStringAndClear()), null, null);
			GUIDropDown guidropDown2 = dropDown;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(16, 1);
			defaultInterpolatedStringHandler4.AppendLiteral("servertag.");
			defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(tag);
			defaultInterpolatedStringHandler4.AppendLiteral(".false");
			LocalizedString text2 = TextManager.Get(defaultInterpolatedStringHandler4.ToStringAndClear());
			object userData2 = ServerListScreen.TernaryOption.Disabled;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(27, 1);
			defaultInterpolatedStringHandler5.AppendLiteral("servertagdescription.");
			defaultInterpolatedStringHandler5.AppendFormatted<Identifier>(tag);
			defaultInterpolatedStringHandler5.AppendLiteral(".false");
			guidropDown2.AddItem(text2, userData2, TextManager.Get(defaultInterpolatedStringHandler5.ToStringAndClear()), null, null);
			dropDown.SelectItem(ServerListScreen.TernaryOption.Any);
			dropDown.OnSelected = delegate(GUIComponent _, object data)
			{
				valueSetter((ServerListScreen.TernaryOption)data);
				this.FilterServers();
				this.StoreServerFilters();
				return true;
			};
			this.ternaryFilters.Add(tag, dropDown);
		}

		// Token: 0x060026AD RID: 9901 RVA: 0x00199D64 File Offset: 0x00197F64
		private void CreateUI()
		{
			ServerListScreen.<>c__DisplayClass53_0 CS$<>8__locals1 = new ServerListScreen.<>c__DisplayClass53_0();
			CS$<>8__locals1.<>4__this = this;
			this.menu = new GUIFrame(new RectTransform(new Vector2(0.95f, 0.85f), GUI.Canvas, Anchor.Center, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(GameMain.GraphicsHeight, 0)
			}, "", null);
			GUILayoutGroup paddedFrame = new GUILayoutGroup(new RectTransform(new Vector2(0.98f, 0.98f), this.menu.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.02f,
				Stretch = true
			};
			GUILayoutGroup topRow = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.15f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			GUILayoutGroup titleContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.995f, 0.33f), topRow.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			RectTransform rectT = new RectTransform(new Vector2(1f, 1f), titleContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = TextManager.Get("JoinServer");
			GUIFont font = GUIStyle.LargeFont;
			GUITextBlock guitextBlock = new GUITextBlock(rectT, text, null, font, Alignment.Left, false, "", null);
			guitextBlock.Padding = Vector4.Zero;
			guitextBlock.ForceUpperCase = ForceUpperCase.Yes;
			guitextBlock.AutoScaleHorizontal = true;
			GUIButton guibutton = new GUIButton(new RectTransform(Vector2.One * 0.9f, titleContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), Alignment.Center, "FriendsButton", null);
			guibutton.OnClicked = delegate(GUIButton _, object _)
			{
				SocialOverlay socialOverlay = SocialOverlay.Instance;
				if (socialOverlay != null)
				{
					socialOverlay.IsOpen = true;
				}
				return false;
			};
			guibutton.ToolTip = TextManager.GetWithVariable("SocialOverlayShortcutHint", "[shortcut]", SocialOverlay.ShortcutBindText, FormatCapitals.No);
			GUIButton friendsButton = guibutton;
			new GUIFrame(new RectTransform(Vector2.One, friendsButton.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "FriendsButtonIcon", null).CanBeFocused = false;
			GUILayoutGroup infoHolder = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.33f), topRow.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.BottomLeft)
			{
				RelativeSpacing = 0.01f,
				Stretch = false
			};
			GUILayoutGroup clientNameHolder = new GUILayoutGroup(new RectTransform(new Vector2(0.2f, 1f), infoHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.05f
			};
			RectTransform rectT2 = new RectTransform(new Vector2(1f, 0f), clientNameHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = TextManager.Get("YourName");
			font = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT2, text2, null, font, Alignment.Left, false, "", null);
			this.ClientNameBox = new GUITextBox(new RectTransform(new Vector2(1f, 0.5f), clientNameHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null, false, true)
			{
				Text = MultiplayerPreferences.Instance.PlayerName,
				MaxTextLength = new int?(32),
				OverflowClip = true
			};
			GUILayoutGroup tabButtonHolder = new GUILayoutGroup(new RectTransform(new Vector2(0.8f - infoHolder.RelativeSpacing, 0.5f), infoHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			this.tabs[ServerListScreen.TabEnum.All] = new ServerListScreen.Tab(ServerListScreen.TabEnum.All, this, tabButtonHolder, "");
			this.tabs[ServerListScreen.TabEnum.Favorites] = new ServerListScreen.Tab(ServerListScreen.TabEnum.Favorites, this, tabButtonHolder, "Data/favoriteservers.xml");
			this.tabs[ServerListScreen.TabEnum.Recent] = new ServerListScreen.Tab(ServerListScreen.TabEnum.Recent, this, tabButtonHolder, "Data/recentservers.xml");
			GUILayoutGroup bottomRow = new GUILayoutGroup(new RectTransform(new Vector2(1f, 1f - topRow.RectTransform.RelativeSize.Y), paddedFrame.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			GUILayoutGroup serverListHolder = new GUILayoutGroup(new RectTransform(new Vector2(1f, 1f), bottomRow.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				OutlineColor = Color.Black
			};
			GUILayoutGroup serverListContainer = null;
			GUIFrame filtersHolder = null;
			filtersHolder = new GUIFrame(new RectTransform(new Vector2(0.2f, 1f), serverListHolder.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), null, null)
			{
				Color = new Color(12, 14, 15, 255) * 0.5f,
				OutlineColor = Color.Black
			};
			CS$<>8__locals1.elementHeight = 0.05f;
			RectTransform rectT3 = new RectTransform(new Vector2(1f, CS$<>8__locals1.elementHeight), filtersHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text3 = TextManager.Get("FilterServers");
			font = GUIStyle.SubHeadingFont;
			GUITextBlock guitextBlock2 = new GUITextBlock(rectT3, text3, null, font, Alignment.Left, false, "", null);
			guitextBlock2.Padding = Vector4.Zero;
			guitextBlock2.AutoScaleHorizontal = true;
			guitextBlock2.CanBeFocused = false;
			GUILayoutGroup searchHolder = new GUILayoutGroup(new RectTransform(new Vector2(1f, CS$<>8__locals1.elementHeight), filtersHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0f, CS$<>8__locals1.elementHeight)
			}, true, Anchor.TopLeft)
			{
				Stretch = true
			};
			CS$<>8__locals1.searchTitle = new GUITextBlock(new RectTransform(new Vector2(0.001f, 1f), searchHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Search") + "...", null, null, Alignment.Left, false, "", null);
			this.searchBox = new GUITextBox(new RectTransform(new Vector2(1f, 1f), searchHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null, false, true);
			this.searchBox.OnSelected += delegate(GUITextBox sender, Keys userdata)
			{
				CS$<>8__locals1.searchTitle.Visible = false;
			};
			this.searchBox.OnDeselected += delegate(GUITextBox sender, Keys userdata)
			{
				CS$<>8__locals1.searchTitle.Visible = true;
			};
			this.searchBox.OnTextChanged += delegate(GUITextBox txtBox, string txt)
			{
				CS$<>8__locals1.<>4__this.FilterServers();
				return true;
			};
			CS$<>8__locals1.filters = new GUIListBox(new RectTransform(new Vector2(0.98f, 1f - CS$<>8__locals1.elementHeight * 2f), filtersHolder.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				ScrollBarVisible = true,
				Spacing = (int)(5f * GUI.Scale)
			};
			this.ternaryFilters = new Dictionary<Identifier, GUIDropDown>();
			this.filterTickBoxes = new Dictionary<Identifier, GUITickBox>();
			this.filterSameVersion = CS$<>8__locals1.<CreateUI>g__addTickBox|4("FilterSameVersion".ToIdentifier(), null, true, false);
			this.filterPassword = CS$<>8__locals1.<CreateUI>g__addTickBox|4("FilterPassword".ToIdentifier(), null, false, false);
			this.filterFull = CS$<>8__locals1.<CreateUI>g__addTickBox|4("FilterFullServers".ToIdentifier(), null, false, false);
			this.filterEmpty = CS$<>8__locals1.<CreateUI>g__addTickBox|4("FilterEmptyServers".ToIdentifier(), null, false, false);
			this.filterOffensive = CS$<>8__locals1.<CreateUI>g__addTickBox|4("FilterOffensiveServers".ToIdentifier(), null, false, false);
			if (ServerLanguageOptions.Options.Any<ServerLanguageOptions.LanguageOption>())
			{
				ServerListScreen.<>c__DisplayClass53_1 CS$<>8__locals2 = new ServerListScreen.<>c__DisplayClass53_1();
				CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
				CS$<>8__locals2.languageKey = "Language".ToIdentifier();
				CS$<>8__locals2.allLanguagesKey = "AllLanguages".ToIdentifier();
				RectTransform rectT4 = new RectTransform(new Vector2(1f, 0.05f), CS$<>8__locals2.CS$<>8__locals1.filters.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text4 = TextManager.Get(CS$<>8__locals2.languageKey);
				font = GUIStyle.SubHeadingFont;
				new GUITextBlock(rectT4, text4, null, font, Alignment.Left, false, "", null).CanBeFocused = false;
				this.languageDropdown = new GUIDropDown(CS$<>8__locals2.CS$<>8__locals1.<CreateUI>g__createFilterRectT|3(), null, 4, "", true, false, Alignment.CenterLeft, 1f);
				this.languageDropdown.AddItem(TextManager.Get(CS$<>8__locals2.allLanguagesKey), CS$<>8__locals2.allLanguagesKey, null, null, null);
				ServerListScreen.<>c__DisplayClass53_1 CS$<>8__locals3 = CS$<>8__locals2;
				GUIComponent guicomponent = this.languageDropdown.ListBox.Content.FindChild(CS$<>8__locals2.allLanguagesKey, false);
				CS$<>8__locals3.allTickbox = ((guicomponent != null) ? guicomponent.GetChild<GUITickBox>() : null);
				GUIFrame guiframe = new GUIFrame(new RectTransform(new Vector2(1f, 0f), this.languageDropdown.ListBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
				{
					MinSize = new Point(0, GUI.IntScaleCeiling(2f))
				}, null, null);
				guiframe.Color = Color.DarkGray;
				guiframe.CanBeFocused = false;
				CS$<>8__locals2.selectedLanguages = ServerListFilters.Instance.GetAttributeLanguageIdentifierArray(CS$<>8__locals2.languageKey, Array.Empty<LanguageIdentifier>());
				foreach (ServerLanguageOptions.LanguageOption languageOption in ServerLanguageOptions.Options)
				{
					string text5;
					LanguageIdentifier languageIdentifier;
					ImmutableArray<LanguageIdentifier> immutableArray;
					languageOption.Deconstruct(out text5, out languageIdentifier, out immutableArray);
					string label = text5;
					LanguageIdentifier identifier = languageIdentifier;
					this.languageDropdown.AddItem(label, identifier, null, null, null);
				}
				if (!CS$<>8__locals2.selectedLanguages.Any<LanguageIdentifier>())
				{
					CS$<>8__locals2.selectedLanguages = (from o in ServerLanguageOptions.Options
					select o.Identifier).ToArray<LanguageIdentifier>();
				}
				foreach (LanguageIdentifier lang in CS$<>8__locals2.selectedLanguages)
				{
					this.languageDropdown.SelectItem(lang);
				}
				if (ServerLanguageOptions.Options.All((ServerLanguageOptions.LanguageOption o) => CS$<>8__locals2.selectedLanguages.Any((LanguageIdentifier l) => o.Identifier == l)))
				{
					this.languageDropdown.SelectItem(CS$<>8__locals2.allLanguagesKey);
					this.languageDropdown.Text = TextManager.Get(CS$<>8__locals2.allLanguagesKey);
				}
				CS$<>8__locals2.langTickboxes = (from c in this.languageDropdown.ListBox.Content.Children
				where c.UserData is LanguageIdentifier
				select c.GetChild<GUITickBox>()).ToArray<GUITickBox>();
				CS$<>8__locals2.inSelectedCall = false;
				this.languageDropdown.OnSelected = delegate(GUIComponent _, object userData)
				{
					if (CS$<>8__locals2.inSelectedCall)
					{
						return true;
					}
					bool result;
					try
					{
						CS$<>8__locals2.inSelectedCall = true;
						if (object.Equals(CS$<>8__locals2.allLanguagesKey, userData))
						{
							foreach (GUITickBox tb2 in CS$<>8__locals2.langTickboxes)
							{
								tb2.Selected = CS$<>8__locals2.allTickbox.Selected;
							}
						}
						bool noneSelected = CS$<>8__locals2.langTickboxes.All((GUITickBox tb) => !tb.Selected);
						bool allSelected = CS$<>8__locals2.langTickboxes.All((GUITickBox tb) => tb.Selected);
						if (allSelected != CS$<>8__locals2.allTickbox.Selected)
						{
							CS$<>8__locals2.allTickbox.Selected = allSelected;
						}
						result = true;
					}
					finally
					{
						CS$<>8__locals2.inSelectedCall = false;
					}
					return result;
				};
				this.languageDropdown.AfterSelected = delegate(GUIComponent _, object userData)
				{
					bool noneSelected = CS$<>8__locals2.langTickboxes.All((GUITickBox tb) => !tb.Selected);
					bool allSelected = CS$<>8__locals2.langTickboxes.All((GUITickBox tb) => tb.Selected);
					if (allSelected)
					{
						CS$<>8__locals2.CS$<>8__locals1.<>4__this.languageDropdown.Text = TextManager.Get(CS$<>8__locals2.allLanguagesKey);
					}
					else if (noneSelected)
					{
						CS$<>8__locals2.CS$<>8__locals1.<>4__this.languageDropdown.Text = TextManager.Get("None");
					}
					IEnumerable<LanguageIdentifier> languages = CS$<>8__locals2.CS$<>8__locals1.<>4__this.languageDropdown.SelectedDataMultiple.OfType<LanguageIdentifier>();
					ServerListFilters.Instance.SetAttribute(CS$<>8__locals2.languageKey, string.Join<LanguageIdentifier>(", ", languages));
					GameSettings.SaveCurrentConfig();
					CS$<>8__locals2.CS$<>8__locals1.<>4__this.FilterServers();
					return true;
				};
			}
			RectTransform rectT5 = new RectTransform(new Vector2(1f, 0.05f), CS$<>8__locals1.filters.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text6 = TextManager.Get("servertags");
			font = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT5, text6, null, font, Alignment.Left, false, "", null).CanBeFocused = false;
			this.AddTernaryFilter(CS$<>8__locals1.filters.Content.RectTransform, CS$<>8__locals1.elementHeight, "karma".ToIdentifier(), delegate(ServerListScreen.TernaryOption value)
			{
				CS$<>8__locals1.<>4__this.filterKarmaValue = value;
			});
			this.AddTernaryFilter(CS$<>8__locals1.filters.Content.RectTransform, CS$<>8__locals1.elementHeight, "traitors".ToIdentifier(), delegate(ServerListScreen.TernaryOption value)
			{
				CS$<>8__locals1.<>4__this.filterTraitorValue = value;
			});
			this.AddTernaryFilter(CS$<>8__locals1.filters.Content.RectTransform, CS$<>8__locals1.elementHeight, "friendlyfire".ToIdentifier(), delegate(ServerListScreen.TernaryOption value)
			{
				CS$<>8__locals1.<>4__this.filterFriendlyFireValue = value;
			});
			this.AddTernaryFilter(CS$<>8__locals1.filters.Content.RectTransform, CS$<>8__locals1.elementHeight, "voip".ToIdentifier(), delegate(ServerListScreen.TernaryOption value)
			{
				CS$<>8__locals1.<>4__this.filterVoipValue = value;
			});
			this.AddTernaryFilter(CS$<>8__locals1.filters.Content.RectTransform, CS$<>8__locals1.elementHeight, "modded".ToIdentifier(), delegate(ServerListScreen.TernaryOption value)
			{
				CS$<>8__locals1.<>4__this.filterModdedValue = value;
			});
			RectTransform rectT6 = new RectTransform(new Vector2(1f, 0.05f), CS$<>8__locals1.filters.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text7 = TextManager.Get("ServerSettingsPlayStyle");
			font = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT6, text7, null, font, Alignment.Left, false, "", null).CanBeFocused = false;
			this.playStyleTickBoxes = new Dictionary<Identifier, GUITickBox>();
			foreach (object obj2 in Enum.GetValues(typeof(PlayStyle)))
			{
				PlayStyle playStyle = (PlayStyle)obj2;
				ServerListScreen.<>c__DisplayClass53_0 CS$<>8__locals4 = CS$<>8__locals1;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
				defaultInterpolatedStringHandler.AppendLiteral("servertag.");
				defaultInterpolatedStringHandler.AppendFormatted<PlayStyle>(playStyle);
				GUITickBox selectionTick = CS$<>8__locals4.<CreateUI>g__addTickBox|4(defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier(), null, true, true);
				selectionTick.UserData = playStyle;
				Dictionary<Identifier, GUITickBox> dictionary = this.playStyleTickBoxes;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(10, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("servertag.");
				defaultInterpolatedStringHandler2.AppendFormatted<PlayStyle>(playStyle);
				dictionary.Add(defaultInterpolatedStringHandler2.ToStringAndClear().ToIdentifier(), selectionTick);
			}
			RectTransform rectT7 = new RectTransform(new Vector2(1f, 0.05f), CS$<>8__locals1.filters.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text8 = TextManager.Get("gamemode");
			font = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT7, text8, null, font, Alignment.Left, false, "", null).CanBeFocused = false;
			this.gameModeTickBoxes = new Dictionary<Identifier, GUITickBox>();
			foreach (GameModePreset mode in GameModePreset.List)
			{
				if (!mode.IsSinglePlayer)
				{
					GUITickBox selectionTick2 = CS$<>8__locals1.<CreateUI>g__addTickBox|4(mode.Identifier, mode.Name, true, true);
					selectionTick2.UserData = mode.Identifier;
					this.gameModeTickBoxes.Add(mode.Identifier, selectionTick2);
				}
			}
			CS$<>8__locals1.filters.Content.RectTransform.SizeChanged += delegate()
			{
				CS$<>8__locals1.filters.Content.RectTransform.RecalculateChildren(true, true);
				CS$<>8__locals1.<>4__this.filterTickBoxes.ForEach(delegate(KeyValuePair<Identifier, GUITickBox> t)
				{
					GUITickBox value = t.Value;
					LocalizedString lStr = t.Value.UserData as LocalizedString;
					value.Text = ((lStr != null) ? lStr : t.Value.UserData.ToString());
				});
				CS$<>8__locals1.<>4__this.gameModeTickBoxes.ForEach(delegate(KeyValuePair<Identifier, GUITickBox> tb)
				{
					tb.Value.Text = tb.Value.ToolTip;
				});
				CS$<>8__locals1.<>4__this.playStyleTickBoxes.ForEach(delegate(KeyValuePair<Identifier, GUITickBox> tb)
				{
					tb.Value.Text = tb.Value.ToolTip;
				});
				GUITextBlock.AutoScaleAndNormalize((from tb in CS$<>8__locals1.<>4__this.filterTickBoxes.Values
				select tb.TextBlock).Concat(from dd in CS$<>8__locals1.<>4__this.ternaryFilters.Values
				select dd.Parent.GetChild<GUITextBlock>()), true, false, new float?(1f));
				if (CS$<>8__locals1.<>4__this.filterTickBoxes.Values.First<GUITickBox>().TextBlock.TextScale < 0.8f)
				{
					CS$<>8__locals1.<>4__this.filterTickBoxes.ForEach(delegate(KeyValuePair<Identifier, GUITickBox> t)
					{
						t.Value.TextBlock.TextScale = 1f;
					});
					IEnumerable<KeyValuePair<Identifier, GUITickBox>> source = CS$<>8__locals1.<>4__this.filterTickBoxes;
					Action<KeyValuePair<Identifier, GUITickBox>> action;
					if ((action = CS$<>8__locals1.<>9__32) == null)
					{
						action = (CS$<>8__locals1.<>9__32 = delegate(KeyValuePair<Identifier, GUITickBox> t)
						{
							t.Value.TextBlock.Text = ToolBox.LimitString(t.Value.TextBlock.Text, t.Value.TextBlock.Font, (int)((float)CS$<>8__locals1.filters.Content.Rect.Width * 0.8f));
						});
					}
					source.ForEach(action);
				}
			};
			serverListContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 1f), serverListHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			this.labelHolder = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.05f), serverListContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(0, 15)
			}, true, Anchor.BottomLeft)
			{
				Stretch = false
			};
			foreach (ServerListScreen.Column column in ServerListScreen.columns.Values)
			{
				ServerListScreen.ColumnLabel label3 = column.Label;
				LocalizedString label2 = TextManager.Get(label3.ToString());
				ServerListScreen.<>c__DisplayClass53_3 CS$<>8__locals5;
				CS$<>8__locals5.btn = new GUIButton(new RectTransform(new Vector2(column.RelativeWidth, 1f), this.labelHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), label2, Alignment.Center, "GUIButtonSmall", null)
				{
					ToolTip = label2,
					ForceUpperCase = ForceUpperCase.Yes,
					UserData = column.Label,
					OnClicked = new GUIButton.OnClickedHandler(this.SortList)
				};
				CS$<>8__locals5.btn.Color *= 0.5f;
				this.labelTexts.Add(CS$<>8__locals5.btn.TextBlock);
				ServerListScreen.<CreateUI>g__arrowImg|53_33("arrowup", SpriteEffects.None, ref CS$<>8__locals5);
				ServerListScreen.<CreateUI>g__arrowImg|53_33("arrowdown", SpriteEffects.FlipVertically, ref CS$<>8__locals5);
			}
			this.serverList = new GUIListBox(new RectTransform(new Vector2(1f, 1f), serverListContainer.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				PlaySoundOnSelect = true,
				ScrollBarVisible = true,
				OnSelected = delegate(GUIComponent btn, object obj)
				{
					if (GUI.MouseOn is GUIButton)
					{
						return false;
					}
					ServerInfo serverInfo = obj as ServerInfo;
					if (serverInfo == null)
					{
						return false;
					}
					CS$<>8__locals1.<>4__this.joinButton.Enabled = true;
					CS$<>8__locals1.<>4__this.selectedServer = Option<ServerInfo>.Some(serverInfo);
					if (!CS$<>8__locals1.<>4__this.serverPreviewContainer.Visible)
					{
						CS$<>8__locals1.<>4__this.serverPreviewContainer.RectTransform.RelativeSize = new Vector2(0.2f, 1f);
						CS$<>8__locals1.<>4__this.serverPreviewContainer.Visible = true;
						CS$<>8__locals1.<>4__this.serverPreviewContainer.IgnoreLayoutGroups = false;
					}
					serverInfo.CreatePreviewWindow(CS$<>8__locals1.<>4__this.serverPreview.Content);
					CS$<>8__locals1.<>4__this.serverPreview.ForceLayoutRecalculation();
					CS$<>8__locals1.<>4__this.panelAnimator.RightEnabled = true;
					CS$<>8__locals1.<>4__this.panelAnimator.RightVisible = true;
					IEnumerable<GUIComponent> children = btn.Children;
					Action<GUIComponent> action;
					if ((action = CS$<>8__locals1.<>9__35) == null)
					{
						action = (CS$<>8__locals1.<>9__35 = delegate(GUIComponent c)
						{
							c.SpriteEffects = (CS$<>8__locals1.<>4__this.serverPreviewContainer.Visible ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
						});
					}
					children.ForEach(action);
					return true;
				}
			};
			this.serverPreviewContainer = new GUIFrame(new RectTransform(new Vector2(0.2f, 1f), serverListHolder.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), null, null)
			{
				Color = new Color(12, 14, 15, 255) * 0.5f,
				OutlineColor = Color.Black,
				IgnoreLayoutGroups = true
			};
			GUIListBox guilistBox = new GUIListBox(new RectTransform(Vector2.One, this.serverPreviewContainer.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
			guilistBox.Padding = Vector4.One * 10f * GUI.Scale;
			guilistBox.HoverCursor = CursorState.Default;
			guilistBox.OnSelected = ((GUIComponent component, object o) => false);
			this.serverPreview = guilistBox;
			this.panelAnimator = new PanelAnimator(new RectTransform(Vector2.One, serverListHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), filtersHolder, serverListContainer, this.serverPreviewContainer);
			this.panelAnimator.RightEnabled = false;
			new GUIFrame(new RectTransform(new Vector2(1f, 0.02f), bottomRow.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUILayoutGroup buttonContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.075f), bottomRow.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				RelativeSpacing = 0.02f,
				Stretch = true
			};
			GUIButton button = new GUIButton(new RectTransform(new Vector2(0.25f, 0.9f), buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Back"), Alignment.Center, "", null)
			{
				OnClicked = new GUIButton.OnClickedHandler(GameMain.MainMenuScreen.ReturnToMainMenu)
			};
			this.scanServersButton = new GUIButton(new RectTransform(new Vector2(0.25f, 0.9f), buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerListRefresh"), Alignment.Center, "", null)
			{
				OnClicked = delegate(GUIButton btn, object userdata)
				{
					CS$<>8__locals1.<>4__this.RefreshServers();
					return true;
				}
			};
			new GUIButton(new RectTransform(new Vector2(0.25f, 0.9f), buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("serverlistdirectjoin"), Alignment.Center, "", null).OnClicked = delegate(GUIButton btn, object userdata)
			{
				if (string.IsNullOrWhiteSpace(CS$<>8__locals1.<>4__this.ClientNameBox.Text))
				{
					CS$<>8__locals1.<>4__this.ClientNameBox.Flash(null, 1.5f, false, false, null);
					CS$<>8__locals1.<>4__this.ClientNameBox.Select(-1, false);
					SoundPlayer.PlayUISound(GUISoundType.PickItemFail);
					return false;
				}
				CS$<>8__locals1.<>4__this.ShowDirectJoinPrompt();
				return true;
			};
			this.joinButton = new GUIButton(new RectTransform(new Vector2(0.25f, 0.9f), buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerListJoin"), Alignment.Center, "", null)
			{
				OnClicked = delegate(GUIButton btn, object userdata)
				{
					ServerInfo serverInfo;
					if (CS$<>8__locals1.<>4__this.selectedServer.TryUnwrap(out serverInfo))
					{
						CS$<>8__locals1.<>4__this.JoinServer(serverInfo.Endpoints, serverInfo.ServerName);
					}
					return true;
				},
				Enabled = false
			};
			buttonContainer.RectTransform.MinSize = new Point(0, (int)((float)buttonContainer.RectTransform.Children.Max((RectTransform c) => c.MinSize.Y) * 1.2f));
			bottomRow.Recalculate();
			serverListHolder.Recalculate();
			serverListContainer.Recalculate();
			this.labelHolder.RectTransform.MaxSize = new Point(this.serverList.Content.Rect.Width, int.MaxValue);
			this.labelHolder.RectTransform.AbsoluteOffset = new Point((int)this.serverList.Padding.X, 0);
			this.labelHolder.Recalculate();
			this.serverList.Content.RectTransform.SizeChanged += delegate()
			{
				CS$<>8__locals1.<>4__this.labelHolder.RectTransform.MaxSize = new Point(CS$<>8__locals1.<>4__this.serverList.Content.Rect.Width, int.MaxValue);
				CS$<>8__locals1.<>4__this.labelHolder.RectTransform.AbsoluteOffset = new Point((int)CS$<>8__locals1.<>4__this.serverList.Padding.X, 0);
				CS$<>8__locals1.<>4__this.labelHolder.Recalculate();
				foreach (GUITextBlock labelText in CS$<>8__locals1.<>4__this.labelTexts)
				{
					labelText.Text = ToolBox.LimitString(labelText.ToolTip, labelText.Font, labelText.Rect.Width);
				}
			};
			button.SelectedColor = button.Color;
			this.selectedTab = ServerListScreen.TabEnum.All;
		}

		// Token: 0x060026AE RID: 9902 RVA: 0x0019B61C File Offset: 0x0019981C
		public void UpdateOrAddServerInfo(ServerInfo serverInfo)
		{
			GUIComponent existingElement = this.serverList.Content.FindChild(delegate(GUIComponent d)
			{
				ServerInfo existingServerInfo = d.UserData as ServerInfo;
				return existingServerInfo != null && existingServerInfo.Endpoints.Any(new Func<Endpoint, bool>(serverInfo.Endpoints.Contains));
			}, false);
			if (existingElement == null)
			{
				this.AddToServerList(serverInfo, false);
				return;
			}
			existingElement.UserData = serverInfo;
		}

		// Token: 0x060026AF RID: 9903 RVA: 0x0019B674 File Offset: 0x00199874
		public void AddToRecentServers(ServerInfo info)
		{
			if (info.Endpoints.First<Endpoint>().Address.IsLocalHost)
			{
				return;
			}
			this.tabs[ServerListScreen.TabEnum.Recent].AddOrUpdate(info);
			this.tabs[ServerListScreen.TabEnum.Recent].Save();
		}

		// Token: 0x060026B0 RID: 9904 RVA: 0x0019B6C4 File Offset: 0x001998C4
		public bool IsFavorite(ServerInfo info)
		{
			return this.tabs[ServerListScreen.TabEnum.Favorites].Contains(info);
		}

		// Token: 0x060026B1 RID: 9905 RVA: 0x0019B6E8 File Offset: 0x001998E8
		public void AddToFavoriteServers(ServerInfo info)
		{
			this.tabs[ServerListScreen.TabEnum.Favorites].AddOrUpdate(info);
			this.tabs[ServerListScreen.TabEnum.Favorites].Save();
		}

		// Token: 0x060026B2 RID: 9906 RVA: 0x0019B720 File Offset: 0x00199920
		public void RemoveFromFavoriteServers(ServerInfo info)
		{
			this.tabs[ServerListScreen.TabEnum.Favorites].Remove(info);
			this.tabs[ServerListScreen.TabEnum.Favorites].Save();
		}

		// Token: 0x060026B3 RID: 9907 RVA: 0x0019B758 File Offset: 0x00199958
		private bool SortList(GUIButton button, object obj)
		{
			if (obj is ServerListScreen.ColumnLabel)
			{
				ServerListScreen.ColumnLabel sortBy = (ServerListScreen.ColumnLabel)obj;
				this.SortList(sortBy, true);
				return true;
			}
			return false;
		}

		// Token: 0x060026B4 RID: 9908 RVA: 0x0019B784 File Offset: 0x00199984
		private void SortList(ServerListScreen.ColumnLabel sortBy, bool toggle)
		{
			GUIButton button = this.labelHolder.GetChildByUserData(sortBy) as GUIButton;
			if (button == null)
			{
				return;
			}
			this.sortedBy = sortBy;
			GUIComponent arrowUp = button.GetChildByUserData("arrowup");
			GUIComponent arrowDown = button.GetChildByUserData("arrowdown");
			foreach (GUIComponent child in button.Parent.Children)
			{
				if (child != button)
				{
					child.GetChildByUserData("arrowup").Visible = false;
					child.GetChildByUserData("arrowdown").Visible = false;
				}
			}
			this.sortedAscending = arrowUp.Visible;
			if (toggle)
			{
				this.sortedAscending = !this.sortedAscending;
			}
			arrowUp.Visible = this.sortedAscending;
			arrowDown.Visible = !this.sortedAscending;
			this.serverList.Content.RectTransform.SortChildren(delegate(RectTransform c1, RectTransform c2)
			{
				ServerInfo s = c1.GUIComponent.UserData as ServerInfo;
				if (s == null)
				{
					return 0;
				}
				ServerInfo s2 = c2.GUIComponent.UserData as ServerInfo;
				if (s2 == null)
				{
					return 0;
				}
				return ServerListScreen.CompareServer(sortBy, s, s2, this.sortedAscending);
			});
		}

		// Token: 0x060026B5 RID: 9909 RVA: 0x0019B8B0 File Offset: 0x00199AB0
		public void HideServerPreview()
		{
			this.serverPreviewContainer.Visible = false;
			this.panelAnimator.RightEnabled = false;
			this.panelAnimator.RightVisible = false;
		}

		// Token: 0x060026B6 RID: 9910 RVA: 0x0019B8D8 File Offset: 0x00199AD8
		private void InsertServer(ServerInfo serverInfo, GUIComponent component)
		{
			List<RectTransform> children = this.serverList.Content.RectTransform.Children.Reverse<RectTransform>().ToList<RectTransform>();
			foreach (RectTransform child in children)
			{
				ServerInfo serverInfo2 = child.GUIComponent.UserData as ServerInfo;
				if (serverInfo2 != null && !serverInfo.Equals(serverInfo2) && ServerListScreen.CompareServer(this.sortedBy, serverInfo, serverInfo2, this.sortedAscending) >= 0)
				{
					int index = this.serverList.Content.RectTransform.GetChildIndex(child);
					component.RectTransform.RepositionChildInHierarchy(Math.Min(index + 1, this.serverList.Content.CountChildren - 1));
					return;
				}
			}
			component.RectTransform.SetAsFirstChild();
		}

		// Token: 0x060026B7 RID: 9911 RVA: 0x0019B9C4 File Offset: 0x00199BC4
		private static int CompareServer(ServerListScreen.ColumnLabel sortBy, ServerInfo s1, ServerInfo s2, bool ascending)
		{
			bool s1HasPing = s1.Ping.IsSome();
			bool s2HasPing = s2.Ping.IsSome();
			if (s1HasPing != s2HasPing)
			{
				if (!s1HasPing)
				{
					return 1;
				}
				return -1;
			}
			else
			{
				int comparison = ascending ? 1 : -1;
				switch (sortBy)
				{
				case ServerListScreen.ColumnLabel.ServerListCompatible:
				{
					bool s1Compatible = NetworkMember.IsCompatible(GameMain.Version, s1.GameVersion);
					bool s2Compatible = NetworkMember.IsCompatible(GameMain.Version, s2.GameVersion);
					if (s1Compatible == s2Compatible)
					{
						return 0;
					}
					return (s1Compatible ? -1 : 1) * comparison;
				}
				case ServerListScreen.ColumnLabel.ServerListHasPassword:
					if (s1.HasPassword == s2.HasPassword)
					{
						return 0;
					}
					return (s1.HasPassword ? 1 : -1) * comparison;
				case ServerListScreen.ColumnLabel.ServerListName:
					return string.Compare(s1.ServerName, s2.ServerName, StringComparison.CurrentCulture) * comparison;
				case ServerListScreen.ColumnLabel.ServerListRoundStarted:
					if (s1.GameStarted == s2.GameStarted)
					{
						return 0;
					}
					return (s1.GameStarted ? 1 : -1) * comparison;
				case ServerListScreen.ColumnLabel.ServerListPlayers:
					return s2.PlayerCount.CompareTo(s1.PlayerCount) * comparison;
				case ServerListScreen.ColumnLabel.ServerListPing:
				{
					int s1Ping;
					bool flag = s1.Ping.TryUnwrap(out s1Ping);
					int s2Ping;
					bool flag2 = s2.Ping.TryUnwrap(out s2Ping);
					int num;
					if (!flag)
					{
						if (flag2)
						{
							num = 1;
						}
						else
						{
							num = 0;
						}
					}
					else if (!flag2)
					{
						num = -1;
					}
					else
					{
						num = s2Ping.CompareTo(s1Ping);
					}
					return num * comparison;
				}
				default:
					return 0;
				}
			}
		}

		// Token: 0x060026B8 RID: 9912 RVA: 0x0019BB0C File Offset: 0x00199D0C
		public unsafe override void Select()
		{
			base.Select();
			if (string.IsNullOrEmpty(this.ClientNameBox.Text))
			{
				TaskPool.Add("GetDefaultUserName", ServerListScreen.GetDefaultUserName(), delegate(Task t)
				{
					string name;
					if (!t.TryGetResult(out name))
					{
						return;
					}
					if (this.ClientNameBox.Text.IsNullOrEmpty())
					{
						this.ClientNameBox.Text = name;
						string nameWithoutInvisibleSymbols = string.Empty;
						foreach (char c in this.ClientNameBox.Text)
						{
							Vector2 size = this.ClientNameBox.Font.MeasureChar(c);
							if (size.X > 0f && size.Y > 0f)
							{
								ReadOnlySpan<char> str = nameWithoutInvisibleSymbols;
								char c2 = c;
								nameWithoutInvisibleSymbols = str + new ReadOnlySpan<char>(ref c2);
							}
						}
						if (nameWithoutInvisibleSymbols != this.ClientNameBox.Text)
						{
							MultiplayerPreferences.Instance.PlayerName = (this.ClientNameBox.Text = nameWithoutInvisibleSymbols);
							new GUIMessageBox(TextManager.Get("Warning"), TextManager.GetWithVariable("NameContainsInvisibleSymbols", "[name]", nameWithoutInvisibleSymbols, FormatCapitals.No), null, null, GUIMessageBox.Type.Default);
						}
					}
				});
			}
			this.ClientNameBox.OnTextChanged += delegate(GUITextBox textbox, string text)
			{
				MultiplayerPreferences.Instance.PlayerName = text;
				return true;
			};
			if (EosInterface.IdQueries.IsLoggedIntoEosConnect)
			{
				if (SteamManager.IsInitialized)
				{
					this.serverProvider = new CompositeServerProvider(new ServerProvider[]
					{
						new EosServerProvider(),
						new SteamDedicatedServerProvider(),
						new SteamP2PServerProvider()
					});
				}
				else
				{
					this.serverProvider = new EosServerProvider();
				}
			}
			else if (SteamManager.IsInitialized)
			{
				this.serverProvider = new CompositeServerProvider(new ServerProvider[]
				{
					new SteamDedicatedServerProvider(),
					new SteamP2PServerProvider()
				});
			}
			else
			{
				this.serverProvider = null;
			}
			SteamMatchmaking.ResetActions();
			this.selectedTab = ServerListScreen.TabEnum.All;
			GameMain.ServerListScreen.LoadServerFilters();
			if (GameSettings.CurrentConfig.ShowOffensiveServerPrompt)
			{
				GUIMessageBox filterOffensivePrompt = new GUIMessageBox(string.Empty, TextManager.Get("FilterOffensiveServersPrompt"), new LocalizedString[]
				{
					TextManager.Get("yes"),
					TextManager.Get("no")
				}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
				filterOffensivePrompt.Buttons[0].OnClicked = delegate(GUIButton btn, object userData)
				{
					this.filterOffensive.Selected = true;
					filterOffensivePrompt.Close();
					return true;
				};
				filterOffensivePrompt.Buttons[1].OnClicked = new GUIButton.OnClickedHandler(filterOffensivePrompt.Close);
				GameSettings.Config config = *GameSettings.CurrentConfig;
				config.ShowOffensiveServerPrompt = false;
				GameSettings.SetCurrentConfig(config);
			}
			if (GameMain.Client != null)
			{
				GameMain.Client.Quit();
				GameMain.Client = null;
			}
			this.RefreshServers();
		}

		// Token: 0x060026B9 RID: 9913 RVA: 0x0019BD07 File Offset: 0x00199F07
		public override void Deselect()
		{
			base.Deselect();
			ServerProvider serverProvider = this.serverProvider;
			if (serverProvider != null)
			{
				serverProvider.Cancel();
			}
			GameSettings.SaveCurrentConfig();
		}

		// Token: 0x060026BA RID: 9914 RVA: 0x0019BD25 File Offset: 0x00199F25
		public override void Update(double deltaTime)
		{
			base.Update(deltaTime);
			PanelAnimator panelAnimator = this.panelAnimator;
			if (panelAnimator != null)
			{
				panelAnimator.Update();
			}
			this.scanServersButton.Enabled = (DateTime.Now - this.lastRefreshTime >= ServerListScreen.AllowedRefreshInterval);
		}

		// Token: 0x060026BB RID: 9915 RVA: 0x0019BD64 File Offset: 0x00199F64
		public void FilterServers()
		{
			this.RemoveMsgFromServerList(ServerListScreen.MsgUserData.NoMatchingServers);
			foreach (GUIComponent child in this.serverList.Content.Children)
			{
				ServerInfo serverInfo = child.UserData as ServerInfo;
				if (serverInfo != null)
				{
					child.Visible = this.ShouldShowServer(serverInfo);
				}
			}
			if (this.serverList.Content.Children.All((GUIComponent c) => !c.Visible))
			{
				this.PutMsgInServerList(ServerListScreen.MsgUserData.NoMatchingServers);
			}
			this.serverList.UpdateScrollBarSize();
		}

		// Token: 0x17000A39 RID: 2617
		// (get) Token: 0x060026BC RID: 9916 RVA: 0x0019BE20 File Offset: 0x0019A020
		private bool AllLanguagesVisible
		{
			get
			{
				if (this.languageDropdown == null)
				{
					return true;
				}
				int tickBoxCount = this.languageDropdown.ListBox.Content.CountChildren - 1;
				int selectedCount = this.languageDropdown.SelectedIndexMultiple.Count<int>();
				return selectedCount >= tickBoxCount;
			}
		}

		// Token: 0x060026BD RID: 9917 RVA: 0x0019BE68 File Offset: 0x0019A068
		private bool ShouldShowServer(ServerInfo serverInfo)
		{
			if (serverInfo == null)
			{
				return false;
			}
			if (ToolBox.VersionNewerIgnoreRevision(GameMain.Version, serverInfo.GameVersion))
			{
				return false;
			}
			if (!string.IsNullOrEmpty(this.searchBox.Text) && !serverInfo.ServerName.Contains(this.searchBox.Text, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			if (this.filterSameVersion.Selected && !NetworkMember.IsCompatible(serverInfo.GameVersion, GameMain.Version))
			{
				return false;
			}
			if (this.filterPassword.Selected && serverInfo.HasPassword)
			{
				return false;
			}
			if (this.filterFull.Selected && serverInfo.PlayerCount >= serverInfo.MaxPlayers)
			{
				return false;
			}
			if (this.filterEmpty.Selected && serverInfo.PlayerCount <= 0)
			{
				return false;
			}
			if (this.filterOffensive.Selected && ForbiddenWordFilter.IsForbidden(serverInfo.ServerName))
			{
				return false;
			}
			if (this.filterKarmaValue != ServerListScreen.TernaryOption.Any && serverInfo.KarmaEnabled != (this.filterKarmaValue == ServerListScreen.TernaryOption.Enabled))
			{
				return false;
			}
			if (this.filterFriendlyFireValue != ServerListScreen.TernaryOption.Any && serverInfo.FriendlyFireEnabled != (this.filterFriendlyFireValue == ServerListScreen.TernaryOption.Enabled))
			{
				return false;
			}
			if (this.filterTraitorValue != ServerListScreen.TernaryOption.Any && serverInfo.TraitorProbability > 0f != (this.filterTraitorValue == ServerListScreen.TernaryOption.Enabled))
			{
				return false;
			}
			if (this.filterVoipValue != ServerListScreen.TernaryOption.Any && serverInfo.VoipEnabled != (this.filterVoipValue == ServerListScreen.TernaryOption.Enabled))
			{
				return false;
			}
			if (this.filterModdedValue != ServerListScreen.TernaryOption.Any && serverInfo.IsModded != (this.filterModdedValue == ServerListScreen.TernaryOption.Enabled))
			{
				return false;
			}
			foreach (GUITickBox tickBox in this.playStyleTickBoxes.Values)
			{
				PlayStyle playStyle = (PlayStyle)tickBox.UserData;
				if (!tickBox.Selected && serverInfo.PlayStyle == playStyle)
				{
					return false;
				}
			}
			if (!this.AllLanguagesVisible && !this.languageDropdown.SelectedDataMultiple.OfType<LanguageIdentifier>().Contains(serverInfo.Language))
			{
				return false;
			}
			foreach (GUITickBox tickBox2 in this.gameModeTickBoxes.Values)
			{
				Identifier gameMode = (Identifier)tickBox2.UserData;
				if (!tickBox2.Selected)
				{
					Identifier gameMode2 = serverInfo.GameMode;
					if (!gameMode2.IsEmpty)
					{
						gameMode2 = serverInfo.GameMode;
						if (gameMode2 == gameMode)
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		// Token: 0x060026BE RID: 9918 RVA: 0x0019C0E8 File Offset: 0x0019A2E8
		private void ShowDirectJoinPrompt()
		{
			GUIMessageBox msgBox = new GUIMessageBox(TextManager.Get("ServerListDirectJoin"), "", new LocalizedString[]
			{
				TextManager.Get("ServerListJoin"),
				TextManager.Get("AddToFavorites"),
				TextManager.Get("Cancel")
			}, new Vector2?(new Vector2(0.25f, 0.2f)), new Point?(new Point(400, 150)), Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			msgBox.Content.ChildAnchor = Anchor.TopCenter;
			GUILayoutGroup content = new GUILayoutGroup(new RectTransform(new Vector2(0.8f, 0.5f), msgBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopCenter)
			{
				IgnoreLayoutGroups = false,
				Stretch = true,
				RelativeSpacing = 0.05f
			};
			new GUITextBlock(new RectTransform(new Vector2(1f, 0.5f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), SteamManager.IsInitialized ? TextManager.Get("ServerEndpoint") : TextManager.Get("ServerIP"), null, null, Alignment.Center, false, "", null);
			GUITextBox endpointBox = new GUITextBox(new RectTransform(new Vector2(1f, 0.5f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null, false, true);
			content.RectTransform.NonScaledSize = new Point(content.Rect.Width, content.RectTransform.Children.Sum((RectTransform c) => c.Rect.Height));
			content.RectTransform.IsFixedSize = true;
			msgBox.InnerFrame.RectTransform.MinSize = new Point(0, (int)((float)(content.RectTransform.NonScaledSize.Y + msgBox.Content.RectTransform.Children.Sum((RectTransform c) => c.NonScaledSize.Y + msgBox.Content.AbsoluteSpacing)) * 1.1f));
			GUIButton okButton = msgBox.Buttons[0];
			okButton.Enabled = false;
			okButton.OnClicked = delegate(GUIButton btn, object userdata)
			{
				Endpoint endpoint;
				LidgrenEndpoint lidgrenEndpoint;
				if (Endpoint.Parse(endpointBox.Text).TryUnwrap(out endpoint))
				{
					if (endpoint is SteamP2PEndpoint && !SteamManager.IsInitialized)
					{
						new GUIMessageBox(TextManager.Get("error"), TextManager.Get("CannotJoinSteamServer.SteamNotInitialized"), null, null, GUIMessageBox.Type.Default);
					}
					else if (endpoint is EosP2PEndpoint && !EosInterface.Core.IsInitialized)
					{
						new GUIMessageBox(TextManager.Get("error"), TextManager.Get("EosStatus.NotInitialized"), null, null, GUIMessageBox.Type.Default);
					}
					else
					{
						this.JoinServer(endpoint.ToEnumerable<Endpoint>().ToImmutableArray<Endpoint>(), "");
					}
				}
				else if (LidgrenEndpoint.ParseFromWithHostNameCheck(endpointBox.Text, true).TryUnwrap(out lidgrenEndpoint))
				{
					this.JoinServer(lidgrenEndpoint.ToEnumerable<Endpoint>().ToImmutableArray<Endpoint>(), "");
				}
				else
				{
					new GUIMessageBox(TextManager.Get("error"), TextManager.GetWithVariable("invalidipaddress", "[serverip]:[port]", endpointBox.Text, FormatCapitals.No), null, null, GUIMessageBox.Type.Default);
					endpointBox.Flash(null, 1.5f, false, false, null);
				}
				msgBox.Close();
				return false;
			};
			GUIButton favoriteButton = msgBox.Buttons[1];
			favoriteButton.Enabled = false;
			favoriteButton.OnClicked = delegate(GUIButton button, object userdata)
			{
				Endpoint endpoint;
				if (!Endpoint.Parse(endpointBox.Text).TryUnwrap(out endpoint))
				{
					return false;
				}
				ServerInfo serverInfo = new ServerInfo(new Endpoint[]
				{
					endpoint
				})
				{
					ServerName = "Server",
					GameVersion = GameMain.Version
				};
				GUIComponent serverFrame = this.serverList.Content.FindChild(delegate(GUIComponent d)
				{
					ServerInfo info = d.UserData as ServerInfo;
					return info != null && info.Equals(serverInfo);
				}, false);
				if (serverFrame != null)
				{
					serverInfo = (ServerInfo)serverFrame.UserData;
				}
				else
				{
					this.AddToServerList(serverInfo, false);
				}
				this.AddToFavoriteServers(serverInfo);
				this.selectedTab = ServerListScreen.TabEnum.Favorites;
				this.FilterServers();
				msgBox.Close();
				return false;
			};
			GUIButton cancelButton = msgBox.Buttons[2];
			cancelButton.OnClicked = new GUIButton.OnClickedHandler(msgBox.Close);
			endpointBox.OnTextChanged += delegate(GUITextBox textBox, string text)
			{
				okButton.Enabled = (favoriteButton.Enabled = !string.IsNullOrEmpty(text));
				return true;
			};
			GUITextBox endpointBox2 = endpointBox;
			endpointBox2.OnEnterPressed = (GUITextBox.OnEnterHandler)Delegate.Combine(endpointBox2.OnEnterPressed, new GUITextBox.OnEnterHandler(delegate(GUITextBox textBox, string text)
			{
				if (okButton.Enabled)
				{
					if (okButton.PlaySoundOnSelect)
					{
						SoundPlayer.PlayUISound(okButton.ClickSound);
					}
					okButton.OnClicked(okButton, okButton.UserData);
				}
				return true;
			}));
			CoroutineManager.Invoke(delegate
			{
				endpointBox.Select(-1, true);
			}, 0.1f);
		}

		// Token: 0x060026BF RID: 9919 RVA: 0x0019C488 File Offset: 0x0019A688
		private void RemoveMsgFromServerList()
		{
			(from c in this.serverList.Content.Children
			where c.UserData is ServerListScreen.MsgUserData
			select c).ForEachMod(new Action<GUIComponent>(this.serverList.Content.RemoveChild));
		}

		// Token: 0x060026C0 RID: 9920 RVA: 0x0019C4E5 File Offset: 0x0019A6E5
		private void RemoveMsgFromServerList(ServerListScreen.MsgUserData userData)
		{
			this.serverList.Content.RemoveChild(this.serverList.Content.FindChild(userData, false));
		}

		// Token: 0x060026C1 RID: 9921 RVA: 0x0019C510 File Offset: 0x0019A710
		private void PutMsgInServerList(ServerListScreen.MsgUserData userData)
		{
			this.RemoveMsgFromServerList();
			GUITextBlock guitextBlock = new GUITextBlock(new RectTransform(Vector2.One, this.serverList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get(userData.ToString()), null, null, Alignment.Center, false, "", null);
			guitextBlock.CanBeFocused = false;
			guitextBlock.UserData = userData;
		}

		// Token: 0x060026C2 RID: 9922 RVA: 0x0019C5A4 File Offset: 0x0019A7A4
		private void RefreshServers()
		{
			this.lastRefreshTime = DateTime.Now;
			ServerProvider serverProvider = this.serverProvider;
			if (serverProvider != null)
			{
				serverProvider.Cancel();
			}
			this.currentServerDataRecvCallbackObj = null;
			PingUtils.QueryPingData();
			this.spamServerCache.Clear();
			this.serverInfoStringCache.Clear();
			this.tabs[ServerListScreen.TabEnum.All].Clear();
			this.serverList.ClearChildren();
			this.serverPreview.Content.ClearChildren();
			this.panelAnimator.RightEnabled = false;
			this.joinButton.Enabled = false;
			Option.UnspecifiedNone none = Option.None;
			this.selectedServer = none;
			if (this.selectedTab == ServerListScreen.TabEnum.All)
			{
				this.PutMsgInServerList(ServerListScreen.MsgUserData.RefreshingServerList);
			}
			else
			{
				ServerInfo[] servers = this.tabs[this.selectedTab].Servers.ToArray<ServerInfo>();
				foreach (ServerInfo server in servers)
				{
					server.Ping = Option<int>.None();
					this.AddToServerList(server, true);
				}
				if (!servers.Any<ServerInfo>())
				{
					this.PutMsgInServerList(ServerListScreen.MsgUserData.NoServers);
					return;
				}
			}
			ValueTuple<Action<ServerInfo, ServerProvider>, Action> valueTuple = this.MakeServerQueryCallbacks();
			Action<ServerInfo, ServerProvider> onServerDataReceived = valueTuple.Item1;
			Action onQueryCompleted = valueTuple.Item2;
			ServerProvider serverProvider2 = this.serverProvider;
			if (serverProvider2 == null)
			{
				return;
			}
			serverProvider2.RetrieveServers(onServerDataReceived, onQueryCompleted);
		}

		// Token: 0x060026C3 RID: 9923 RVA: 0x0019C6E4 File Offset: 0x0019A8E4
		private GUIComponent FindFrameMatchingServerInfo(ServerInfo serverInfo)
		{
			ServerListScreen.<>c__DisplayClass76_0 CS$<>8__locals1 = new ServerListScreen.<>c__DisplayClass76_0();
			CS$<>8__locals1.serverInfo = serverInfo;
			return this.serverList.Content.FindChild(new Func<GUIComponent, bool>(CS$<>8__locals1.<FindFrameMatchingServerInfo>g__matches|0), false);
		}

		// Token: 0x060026C4 RID: 9924 RVA: 0x0019C71C File Offset: 0x0019A91C
		[return: TupleElementNames(new string[]
		{
			"OnServerDataReceived",
			"OnQueryCompleted"
		})]
		private ValueTuple<Action<ServerInfo, ServerProvider>, Action> MakeServerQueryCallbacks()
		{
			object uniqueObject = new object();
			this.currentServerDataRecvCallbackObj = uniqueObject;
			return new ValueTuple<Action<ServerInfo, ServerProvider>, Action>(delegate(ServerInfo serverInfo, ServerProvider serverProvider)
			{
				if (!base.<MakeServerQueryCallbacks>g__shouldRunCallback|0())
				{
					return;
				}
				if (!(serverProvider is EosServerProvider) && EosInterface.IdQueries.IsLoggedIntoEosConnect && serverInfo.EosCrossplay)
				{
					return;
				}
				if (this.selectedTab == ServerListScreen.TabEnum.All)
				{
					this.AddToServerList(serverInfo, false);
					return;
				}
				if (this.FindFrameMatchingServerInfo(serverInfo) == null)
				{
					return;
				}
				this.UpdateServerInfoUI(serverInfo);
				PingUtils.GetServerPing(serverInfo, new Action<ServerInfo>(this.UpdateServerInfoUI));
			}, delegate()
			{
				if (base.<MakeServerQueryCallbacks>g__shouldRunCallback|0())
				{
					this.ServerQueryFinished();
				}
			});
		}

		// Token: 0x060026C5 RID: 9925 RVA: 0x0019C76C File Offset: 0x0019A96C
		private void AddToServerList(ServerInfo serverInfo, bool skipPing = false)
		{
			if (string.IsNullOrWhiteSpace(serverInfo.ServerName))
			{
				return;
			}
			if (serverInfo.ServerName.Length > NetConfig.ServerNameMaxLength)
			{
				return;
			}
			if (serverInfo.ServerName.Contains('\n') || serverInfo.ServerName.Contains('\r'))
			{
				return;
			}
			if (serverInfo.ServerMessage.Length > NetConfig.ServerMessageMaxLength)
			{
				return;
			}
			if (serverInfo.PlayerCount > serverInfo.MaxPlayers + 1)
			{
				return;
			}
			if (serverInfo.PlayerCount < 0)
			{
				return;
			}
			if (serverInfo.MaxPlayers <= 0)
			{
				return;
			}
			if (!serverInfo.SelectedSub.IsNullOrEmpty() && serverInfo.SelectedSub.Length > 30)
			{
				return;
			}
			if (serverInfo.MaxPlayers > 1000)
			{
				return;
			}
			string serverCacheKey = serverInfo.Endpoints.First<Endpoint>().StringRepresentation;
			if (this.spamServerCache.Contains(serverCacheKey))
			{
				return;
			}
			if (SpamServerFilters.IsFiltered(serverInfo))
			{
				this.spamServerCache.Add(serverCacheKey);
				return;
			}
			int similarServerCount = 0;
			string serverInfoStr = ServerListScreen.<AddToServerList>g__getServerInfoStr|79_0(serverInfo);
			foreach (GUIComponent serverElement in this.serverList.Content.Children)
			{
				if (serverElement.Visible)
				{
					ServerInfo otherServer = serverElement.UserData as ServerInfo;
					if (otherServer != null && otherServer != serverInfo && (float)ToolBox.LevenshteinDistance(serverInfoStr, this.<AddToServerList>g__getCachedServerInfoStr|79_1(otherServer)) < (float)serverInfoStr.Length * 0.19999999f)
					{
						similarServerCount++;
						if (similarServerCount > 10)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(76, 2);
							defaultInterpolatedStringHandler.AppendLiteral("Server ");
							defaultInterpolatedStringHandler.AppendFormatted(serverInfo.ServerName);
							defaultInterpolatedStringHandler.AppendLiteral(" seems to be almost identical to ");
							defaultInterpolatedStringHandler.AppendFormatted(otherServer.ServerName);
							defaultInterpolatedStringHandler.AppendLiteral(". Hiding as a potential spam server.");
							DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
							break;
						}
					}
				}
			}
			if (similarServerCount > 10)
			{
				return;
			}
			this.RemoveMsgFromServerList(ServerListScreen.MsgUserData.RefreshingServerList);
			this.RemoveMsgFromServerList(ServerListScreen.MsgUserData.NoServers);
			GUIFrame serverFrame = new GUIFrame(new RectTransform(new Vector2(1f, 0.06f), this.serverList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(0, 35)
			}, "ListBoxElement", null)
			{
				UserData = serverInfo
			};
			GUIFrame guiframe = serverFrame;
			guiframe.OnSecondaryClicked = (GUIComponent.SecondaryButtonDownHandler)Delegate.Combine(guiframe.OnSecondaryClicked, new GUIComponent.SecondaryButtonDownHandler(delegate(GUIComponent _, object data)
			{
				ServerInfo info = data as ServerInfo;
				if (info == null)
				{
					return false;
				}
				this.CreateContextMenu(info);
				return true;
			}));
			new GUILayoutGroup(new RectTransform(new Vector2(1f, 1f), serverFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft).Stretch = false;
			this.UpdateServerInfoUI(serverInfo);
			if (!skipPing)
			{
				PingUtils.GetServerPing(serverInfo, new Action<ServerInfo>(this.UpdateServerInfoUI));
			}
		}

		// Token: 0x060026C6 RID: 9926 RVA: 0x0019CA58 File Offset: 0x0019AC58
		private void CreateContextMenu(ServerInfo info)
		{
			ContextMenuOption favoriteOption = new ContextMenuOption(this.IsFavorite(info) ? "removefromfavorites" : "addtofavorites", true, delegate()
			{
				if (this.IsFavorite(info))
				{
					this.RemoveFromFavoriteServers(info);
				}
				else
				{
					this.AddToFavoriteServers(info);
				}
				this.FilterServers();
			});
			ContextMenuOption reportOption = new ContextMenuOption("reportserver", true, delegate()
			{
				ServerListScreen.CreateReportPrompt(info);
			});
			ContextMenuOption filterOption = new ContextMenuOption("filterserver", true, delegate()
			{
				ServerListScreen.CreateFilterServerPrompt(info);
			})
			{
				Tooltip = TextManager.Get("filterservertooltip")
			};
			GUIContextMenu.CreateContextMenu(new ContextMenuOption[]
			{
				favoriteOption,
				filterOption,
				reportOption
			});
		}

		// Token: 0x060026C7 RID: 9927 RVA: 0x0019CB14 File Offset: 0x0019AD14
		public static void CreateFilterServerPrompt(ServerInfo info)
		{
			GUI.AskForConfirmation(TextManager.Get("filterserver"), TextManager.GetWithVariables("filterserverconfirm", new ValueTuple<string, string>[]
			{
				new ValueTuple<string, string>("[server]", info.ServerName),
				new ValueTuple<string, string>("[filepath]", SpamServerFilter.SavePath)
			}), delegate
			{
				SpamServerFilters.AddServerToLocalSpamList(info);
				ServerListScreen serverListScreen = GameMain.ServerListScreen;
				if (serverListScreen == null)
				{
					return;
				}
				ServerInfo selectedServer;
				if (serverListScreen.selectedServer.TryUnwrap(out selectedServer) && selectedServer.Equals(info))
				{
					serverListScreen.HideServerPreview();
				}
				serverListScreen.FilterServers();
			}, null, new Vector2?(ServerListScreen.confirmPopupSize), new Point?(ServerListScreen.confirmPopupMinSize));
		}

		// Token: 0x060026C8 RID: 9928 RVA: 0x0019CBA4 File Offset: 0x0019ADA4
		public static void CreateReportPrompt(ServerInfo info)
		{
			ServerListScreen.<>c__DisplayClass85_0 CS$<>8__locals1 = new ServerListScreen.<>c__DisplayClass85_0();
			CS$<>8__locals1.info = info;
			if (!GameAnalyticsManager.SendUserStatistics)
			{
				GUI.NotifyPrompt(TextManager.Get("reportserver"), TextManager.Get("reportserverdisabled"));
				return;
			}
			ServerListScreen.<>c__DisplayClass85_0 CS$<>8__locals2 = CS$<>8__locals1;
			RichString headerText = TextManager.Get("reportserver");
			RichString text = string.Empty;
			Vector2? relativeSize = new Vector2?(new Vector2(0.2f, 0.4f));
			Point? point = new Point?(new Point(380, 430));
			CS$<>8__locals2.msgBox = new GUIMessageBox(headerText, text, Array.Empty<LocalizedString>(), relativeSize, point, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			Vector2 one = Vector2.One;
			RectTransform rectTransform = CS$<>8__locals1.msgBox.Content.RectTransform;
			Anchor anchor = Anchor.Center;
			Pivot? pivot = null;
			point = null;
			Point? minSize = point;
			point = null;
			GUILayoutGroup layout = new GUILayoutGroup(new RectTransform(one, rectTransform, anchor, pivot, minSize, point, ScaleBasis.Normal), false, Anchor.TopLeft);
			Vector2 relativeSize2 = new Vector2(1f, 0.3f);
			RectTransform rectTransform2 = layout.RectTransform;
			Anchor anchor2 = Anchor.TopLeft;
			Pivot? pivot2 = null;
			point = null;
			Point? minSize2 = point;
			point = null;
			new GUITextBlock(new RectTransform(relativeSize2, rectTransform2, anchor2, pivot2, minSize2, point, ScaleBasis.Normal), TextManager.GetWithVariable("reportserverexplanation", "[server]", CS$<>8__locals1.info.ServerName, FormatCapitals.No), null, null, Alignment.Left, true, "", null).ToolTip = TextManager.Get("reportserverprompttooltip");
			ServerListScreen.<>c__DisplayClass85_0 CS$<>8__locals3 = CS$<>8__locals1;
			Vector2 relativeSize3 = new Vector2(1f, 0.3f);
			RectTransform rectTransform3 = layout.RectTransform;
			Anchor anchor3 = Anchor.TopLeft;
			Pivot? pivot3 = null;
			point = null;
			Point? minSize3 = point;
			point = null;
			CS$<>8__locals3.listBox = new GUIListBox(new RectTransform(relativeSize3, rectTransform3, anchor3, pivot3, minSize3, point, ScaleBasis.Normal), false, null, "", true, false);
			ServerListScreen.ReportReason[] enums = Enum.GetValues<ServerListScreen.ReportReason>();
			foreach (ServerListScreen.ReportReason reason in enums)
			{
				Vector2 relativeSize4 = new Vector2(1f, 1f / (float)enums.Length);
				RectTransform rectTransform4 = CS$<>8__locals1.listBox.Content.RectTransform;
				Anchor anchor4 = Anchor.TopLeft;
				Pivot? pivot4 = null;
				point = null;
				Point? minSize4 = point;
				point = null;
				RectTransform rectT = new RectTransform(relativeSize4, rectTransform4, anchor4, pivot4, minSize4, point, ScaleBasis.Normal);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(13, 1);
				defaultInterpolatedStringHandler.AppendLiteral("reportreason.");
				defaultInterpolatedStringHandler.AppendFormatted<ServerListScreen.ReportReason>(reason);
				new GUITickBox(rectT, TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear()), null, "").UserData = reason;
			}
			Vector2 relativeSize5 = new Vector2(1f, 0.05f);
			RectTransform rectTransform5 = layout.RectTransform;
			Anchor anchor5 = Anchor.TopLeft;
			Pivot? pivot5 = null;
			point = null;
			Point? minSize5 = point;
			point = null;
			new GUIFrame(new RectTransform(relativeSize5, rectTransform5, anchor5, pivot5, minSize5, point, ScaleBasis.Normal), null, null);
			Vector2 relativeSize6 = new Vector2(1f, 0.3f);
			RectTransform rectTransform6 = layout.RectTransform;
			Anchor anchor6 = Anchor.TopLeft;
			Pivot? pivot6 = null;
			point = null;
			Point? minSize6 = point;
			point = null;
			GUILayoutGroup buttonLayout = new GUILayoutGroup(new RectTransform(relativeSize6, rectTransform6, anchor6, pivot6, minSize6, point, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			ServerListScreen.<>c__DisplayClass85_0 CS$<>8__locals4 = CS$<>8__locals1;
			Vector2 relativeSize7 = new Vector2(1f, 0.333f);
			RectTransform rectTransform7 = buttonLayout.RectTransform;
			Anchor anchor7 = Anchor.TopLeft;
			Pivot? pivot7 = null;
			point = null;
			Point? minSize7 = point;
			point = null;
			CS$<>8__locals4.reportAndHideButton = new GUIButton(new RectTransform(relativeSize7, rectTransform7, anchor7, pivot7, minSize7, point, ScaleBasis.Normal), TextManager.Get("reportoption.reportandhide"), Alignment.Center, "", null)
			{
				Enabled = false,
				OnClicked = delegate(GUIButton _, object _)
				{
					ServerListScreen.CreateFilterServerPrompt(CS$<>8__locals1.info);
					CS$<>8__locals1.msgBox.Close();
					return true;
				}
			};
			ServerListScreen.<>c__DisplayClass85_0 CS$<>8__locals5 = CS$<>8__locals1;
			Vector2 relativeSize8 = new Vector2(1f, 0.333f);
			RectTransform rectTransform8 = buttonLayout.RectTransform;
			Anchor anchor8 = Anchor.TopLeft;
			Pivot? pivot8 = null;
			point = null;
			Point? minSize8 = point;
			point = null;
			CS$<>8__locals5.reportButton = new GUIButton(new RectTransform(relativeSize8, rectTransform8, anchor8, pivot8, minSize8, point, ScaleBasis.Normal), TextManager.Get("reportoption.report"), Alignment.Center, "", null)
			{
				Enabled = false,
				OnClicked = delegate(GUIButton _, object _)
				{
					ServerListScreen.ReportServer(CS$<>8__locals1.info, base.<CreateReportPrompt>g__GetUserSelectedReasons|0());
					CS$<>8__locals1.msgBox.Close();
					return true;
				}
			};
			Vector2 relativeSize9 = new Vector2(1f, 0.333f);
			RectTransform rectTransform9 = buttonLayout.RectTransform;
			Anchor anchor9 = Anchor.TopLeft;
			Pivot? pivot9 = null;
			point = null;
			Point? minSize9 = point;
			point = null;
			new GUIButton(new RectTransform(relativeSize9, rectTransform9, anchor9, pivot9, minSize9, point, ScaleBasis.Normal), TextManager.Get("cancel"), Alignment.Center, "", null).OnClicked = delegate(GUIButton _, object _)
			{
				CS$<>8__locals1.msgBox.Close();
				return true;
			};
			foreach (GUITickBox child in CS$<>8__locals1.listBox.Content.GetAllChildren<GUITickBox>())
			{
				GUITickBox guitickBox = child;
				Delegate onSelected = guitickBox.OnSelected;
				GUITickBox.OnSelectedHandler b;
				if ((b = CS$<>8__locals1.<>9__4) == null)
				{
					b = (CS$<>8__locals1.<>9__4 = delegate(GUITickBox _)
					{
						CS$<>8__locals1.reportAndHideButton.Enabled = (CS$<>8__locals1.reportButton.Enabled = base.<CreateReportPrompt>g__GetUserSelectedReasons|0().Any<ServerListScreen.ReportReason>());
						return true;
					});
				}
				guitickBox.OnSelected = (GUITickBox.OnSelectedHandler)Delegate.Combine(onSelected, b);
			}
		}

		// Token: 0x060026C9 RID: 9929 RVA: 0x0019D09C File Offset: 0x0019B29C
		private static void ReportServer(ServerInfo info, IEnumerable<ServerListScreen.ReportReason> reasons)
		{
			if (!reasons.Any<ServerListScreen.ReportReason>())
			{
				return;
			}
			GameAnalyticsManager.ErrorSeverity errorSeverity = GameAnalyticsManager.ErrorSeverity.Info;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(72, 4);
			defaultInterpolatedStringHandler.AppendLiteral("[Spam] Reported server: Name: \"");
			defaultInterpolatedStringHandler.AppendFormatted(info.ServerName);
			defaultInterpolatedStringHandler.AppendLiteral("\", Message: \"");
			defaultInterpolatedStringHandler.AppendFormatted(info.ServerMessage);
			defaultInterpolatedStringHandler.AppendLiteral("\", Endpoint: \"");
			defaultInterpolatedStringHandler.AppendFormatted(info.Endpoints.First<Endpoint>().StringRepresentation);
			defaultInterpolatedStringHandler.AppendLiteral("\". Reason: \"");
			defaultInterpolatedStringHandler.AppendFormatted(string.Join<ServerListScreen.ReportReason>(", ", reasons));
			defaultInterpolatedStringHandler.AppendLiteral("\".");
			GameAnalyticsManager.AddErrorEvent(errorSeverity, defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x060026CA RID: 9930 RVA: 0x0019D148 File Offset: 0x0019B348
		private void UpdateServerInfoUI(ServerInfo serverInfo)
		{
			ServerListScreen.<>c__DisplayClass87_0 CS$<>8__locals1 = new ServerListScreen.<>c__DisplayClass87_0();
			CS$<>8__locals1.serverInfo = serverInfo;
			CS$<>8__locals1.serverFrame = this.FindFrameMatchingServerInfo(CS$<>8__locals1.serverInfo);
			if (CS$<>8__locals1.serverFrame == null)
			{
				return;
			}
			CS$<>8__locals1.serverFrame.UserData = CS$<>8__locals1.serverInfo;
			CS$<>8__locals1.serverContent = (CS$<>8__locals1.serverFrame.Children.First<GUIComponent>() as GUILayoutGroup);
			CS$<>8__locals1.serverContent.ClearChildren();
			CS$<>8__locals1.sections = new Dictionary<ServerListScreen.ColumnLabel, GUIFrame>();
			foreach (object obj in Enum.GetValues(typeof(ServerListScreen.ColumnLabel)))
			{
				ServerListScreen.ColumnLabel label = (ServerListScreen.ColumnLabel)obj;
				CS$<>8__locals1.sections[label] = new GUIFrame(new RectTransform(new Vector2(ServerListScreen.columns[label].RelativeWidth, 1f), CS$<>8__locals1.serverContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			}
			GUITickBox compatibleBox = new GUITickBox(CS$<>8__locals1.<UpdateServerInfoUI>g__columnRT|1(ServerListScreen.ColumnLabel.ServerListCompatible, 0.95f), "", null, "")
			{
				CanBeFocused = false,
				Selected = NetworkMember.IsCompatible(GameMain.Version, CS$<>8__locals1.serverInfo.GameVersion),
				UserData = "compatible"
			};
			GUITickBox guitickBox = new GUITickBox(CS$<>8__locals1.<UpdateServerInfoUI>g__columnRT|1(ServerListScreen.ColumnLabel.ServerListHasPassword, 0.6f), "", null, "GUIServerListPasswordTickBox");
			guitickBox.Selected = CS$<>8__locals1.serverInfo.HasPassword;
			guitickBox.UserData = "password";
			guitickBox.CanBeFocused = false;
			CS$<>8__locals1.<UpdateServerInfoUI>g__sectionTooltip|2(ServerListScreen.ColumnLabel.ServerListHasPassword, TextManager.Get(CS$<>8__locals1.serverInfo.HasPassword ? "ServerListHasPassword" : "FilterPassword"));
			CS$<>8__locals1.serverName = new GUITextBlock(CS$<>8__locals1.<UpdateServerInfoUI>g__columnRT|1(ServerListScreen.ColumnLabel.ServerListName, 0.95f), CS$<>8__locals1.serverInfo.ServerName, null, null, Alignment.Left, false, "GUIServerListTextBox", null)
			{
				CanBeFocused = false
			};
			if (CS$<>8__locals1.serverInfo.IsModded)
			{
				CS$<>8__locals1.serverName.TextColor = GUIStyle.ModdedServerColor;
			}
			GUITickBox guitickBox2 = new GUITickBox(CS$<>8__locals1.<UpdateServerInfoUI>g__columnRT|1(ServerListScreen.ColumnLabel.ServerListRoundStarted, 0.95f), "", null, "");
			guitickBox2.Selected = CS$<>8__locals1.serverInfo.GameStarted;
			guitickBox2.CanBeFocused = false;
			CS$<>8__locals1.<UpdateServerInfoUI>g__sectionTooltip|2(ServerListScreen.ColumnLabel.ServerListRoundStarted, TextManager.Get(CS$<>8__locals1.serverInfo.GameStarted ? "ServerListRoundStarted" : "ServerListRoundNotStarted"));
			RectTransform rectT = CS$<>8__locals1.<UpdateServerInfoUI>g__columnRT|1(ServerListScreen.ColumnLabel.ServerListPlayers, 0.95f);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
			defaultInterpolatedStringHandler.AppendFormatted<int>(CS$<>8__locals1.serverInfo.PlayerCount);
			defaultInterpolatedStringHandler.AppendLiteral("/");
			defaultInterpolatedStringHandler.AppendFormatted<int>(CS$<>8__locals1.serverInfo.MaxPlayers);
			GUITextBlock serverPlayers = new GUITextBlock(rectT, defaultInterpolatedStringHandler.ToStringAndClear(), null, null, Alignment.Right, false, "GUIServerListTextBox", null)
			{
				ToolTip = TextManager.Get("ServerListPlayers")
			};
			GUITextBlock serverPingText = new GUITextBlock(CS$<>8__locals1.<UpdateServerInfoUI>g__columnRT|1(ServerListScreen.ColumnLabel.ServerListPing, 0.95f), "?", new Color?(Color.White * 0.5f), null, Alignment.Right, false, "GUIServerListTextBox", null)
			{
				ToolTip = TextManager.Get("ServerListPing")
			};
			int ping;
			if (CS$<>8__locals1.serverInfo.Ping.TryUnwrap(out ping))
			{
				serverPingText.Text = ping.ToString();
				serverPingText.TextColor = ServerListScreen.GetPingTextColor(ping);
			}
			else
			{
				if (CS$<>8__locals1.serverInfo.Endpoints.Length != 1 || !(CS$<>8__locals1.serverInfo.Endpoints.First<Endpoint>() is EosP2PEndpoint))
				{
					if (!SteamManager.IsInitialized)
					{
						if (CS$<>8__locals1.serverInfo.Endpoints.Any((Endpoint e) => e is P2PEndpoint))
						{
							goto IL_424;
						}
					}
					serverPingText.Text = "?";
					serverPingText.TextColor = Color.DarkRed;
					goto IL_473;
				}
				IL_424:
				serverPingText.Text = "-";
				serverPingText.ToolTip = TextManager.Get("EosPingUnavailable");
				serverPingText.TextAlignment = Alignment.Center;
			}
			IL_473:
			LocalizedString toolTip = "";
			if (!CS$<>8__locals1.serverInfo.Checked)
			{
				toolTip = TextManager.Get("ServerOffline");
				CS$<>8__locals1.serverName.TextColor *= 0.8f;
				serverPlayers.TextColor *= 0.8f;
			}
			else if (!CS$<>8__locals1.serverInfo.ContentPackages.Any<ServerListContentPackageInfo>())
			{
				compatibleBox.Selected = false;
				new GUITextBlock(new RectTransform(new Vector2(0.8f, 0.8f), compatibleBox.Box.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), " ? ", new Color?(GUIStyle.Orange * 0.85f), null, Alignment.Center, false, "", null).ToolTip = TextManager.Get("ServerListUnknownContentPackage");
			}
			else if (!compatibleBox.Selected)
			{
				if (CS$<>8__locals1.serverInfo.GameVersion != GameMain.Version)
				{
					toolTip = TextManager.GetWithVariable("ServerListIncompatibleVersion", "[version]", CS$<>8__locals1.serverInfo.GameVersion.ToString(), FormatCapitals.No);
				}
				int maxIncompatibleToList = 10;
				List<LocalizedString> incompatibleModNames = new List<LocalizedString>();
				ImmutableArray<ServerListContentPackageInfo>.Enumerator enumerator2 = CS$<>8__locals1.serverInfo.ContentPackages.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					ServerListContentPackageInfo contentPackage = enumerator2.Current;
					bool listAsIncompatible = !ContentPackageManager.EnabledPackages.All.Any((ContentPackage cp) => cp.Hash.StringRepresentation == contentPackage.Hash);
					if (listAsIncompatible)
					{
						incompatibleModNames.Add(TextManager.GetWithVariables("ModNameAndHashFormat", new ValueTuple<string, string>[]
						{
							new ValueTuple<string, string>("[name]", contentPackage.Name),
							new ValueTuple<string, string>("[hash]", Md5Hash.GetShortHash(contentPackage.Hash))
						}));
					}
				}
				if (incompatibleModNames.Any<LocalizedString>())
				{
					toolTip += '\n' + TextManager.Get("ModDownloadHeader") + "\n" + string.Join<LocalizedString>(", ", incompatibleModNames.Take(maxIncompatibleToList));
					if (incompatibleModNames.Count > maxIncompatibleToList)
					{
						toolTip += '\n' + TextManager.GetWithVariable("workshopitemdownloadprompttruncated", "[number]", (incompatibleModNames.Count - maxIncompatibleToList).ToString(), FormatCapitals.No);
					}
				}
				CS$<>8__locals1.serverName.TextColor *= 0.5f;
				serverPlayers.TextColor *= 0.5f;
			}
			else
			{
				ImmutableArray<ServerListContentPackageInfo>.Enumerator enumerator3 = CS$<>8__locals1.serverInfo.ContentPackages.GetEnumerator();
				while (enumerator3.MoveNext())
				{
					ServerListContentPackageInfo contentPackage = enumerator3.Current;
					if (ContentPackageManager.EnabledPackages.All.None((ContentPackage cp) => cp.Hash.StringRepresentation == contentPackage.Hash))
					{
						if (toolTip != "")
						{
							toolTip += "\n";
						}
						toolTip += TextManager.GetWithVariable("ServerListIncompatibleContentPackageWorkshopAvailable", "[contentpackage]", contentPackage.Name, FormatCapitals.No);
						break;
					}
				}
			}
			CS$<>8__locals1.<UpdateServerInfoUI>g__disableElementFocus|0();
			string separator = toolTip.IsNullOrWhiteSpace() ? "" : "\n\n";
			GUIComponent serverFrame = CS$<>8__locals1.serverFrame;
			LocalizedString left = toolTip + separator;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(21, 1);
			defaultInterpolatedStringHandler2.AppendLiteral("‖color:gui.blue‖");
			defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(TextManager.GetWithVariable("serverlisttooltip", "[button]", PlayerInput.SecondaryMouseLabel, FormatCapitals.No));
			defaultInterpolatedStringHandler2.AppendLiteral("‖end‖");
			serverFrame.ToolTip = RichString.Rich(left + defaultInterpolatedStringHandler2.ToStringAndClear(), null);
			foreach (GUIFrame section in CS$<>8__locals1.sections.Values)
			{
				GUIComponent child = section.Children.First<GUIComponent>();
				child.RectTransform.ScaleBasis = ((child is GUITextBlock) ? ScaleBasis.Normal : ScaleBasis.BothHeight);
			}
			CS$<>8__locals1.isDirty = true;
			CS$<>8__locals1.serverContent.GetAllChildren().ForEach(delegate(GUIComponent c)
			{
				c.RectTransform.ResetSizeChanged();
				c.RectTransform.SizeChanged += base.<UpdateServerInfoUI>g__markAsDirty|4;
			});
			new GUICustomComponent(new RectTransform(Vector2.Zero, CS$<>8__locals1.serverContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, delegate(float _, GUICustomComponent __)
			{
				if (CS$<>8__locals1.serverFrame.MouseRect.Height <= 0 || !CS$<>8__locals1.isDirty)
				{
					return;
				}
				CS$<>8__locals1.serverContent.GetAllChildren().ForEach(delegate(GUIComponent c)
				{
					GUITextBlock textBlock = c as GUITextBlock;
					if (textBlock != null)
					{
						textBlock.SetTextPos();
						return;
					}
					GUITickBox tickBox = c as GUITickBox;
					if (tickBox == null)
					{
						return;
					}
					tickBox.ResizeBox();
				});
				CS$<>8__locals1.serverName.Text = ToolBox.LimitString(CS$<>8__locals1.serverInfo.ServerName, CS$<>8__locals1.serverName.Font, CS$<>8__locals1.serverName.Rect.Width);
				CS$<>8__locals1.isDirty = false;
			});
			CS$<>8__locals1.serverContent.Recalculate();
			if (this.tabs[ServerListScreen.TabEnum.Favorites].Contains(CS$<>8__locals1.serverInfo))
			{
				this.AddToFavoriteServers(CS$<>8__locals1.serverInfo);
			}
			this.InsertServer(CS$<>8__locals1.serverInfo, CS$<>8__locals1.serverFrame);
			this.FilterServers();
		}

		// Token: 0x060026CB RID: 9931 RVA: 0x0019DB04 File Offset: 0x0019BD04
		private void ServerQueryFinished()
		{
			this.currentServerDataRecvCallbackObj = null;
			if (!this.serverList.Content.Children.Any((GUIComponent c) => c.UserData is ServerInfo))
			{
				this.PutMsgInServerList(ServerListScreen.MsgUserData.NoServers);
				return;
			}
			if (this.serverList.Content.Children.All((GUIComponent c) => !c.Visible))
			{
				this.PutMsgInServerList(ServerListScreen.MsgUserData.NoMatchingServers);
			}
		}

		// Token: 0x060026CC RID: 9932 RVA: 0x0019DB94 File Offset: 0x0019BD94
		public void JoinServer(ImmutableArray<Endpoint> endpoints, string serverName)
		{
			ServerListScreen.<>c__DisplayClass89_0 CS$<>8__locals1 = new ServerListScreen.<>c__DisplayClass89_0();
			CS$<>8__locals1.endpoints = endpoints;
			CS$<>8__locals1.serverName = serverName;
			if (string.IsNullOrWhiteSpace(this.ClientNameBox.Text))
			{
				this.ClientNameBox.Flash(null, 1.5f, false, false, null);
				this.ClientNameBox.Select(-1, false);
				SoundPlayer.PlayUISound(GUISoundType.PickItemFail);
				return;
			}
			MultiplayerPreferences.Instance.PlayerName = this.ClientNameBox.Text;
			GameSettings.SaveCurrentConfig();
			if (MultiplayerPreferences.Instance.PlayerName.IsNullOrEmpty())
			{
				TaskPool.Add("GetDefaultUserName", ServerListScreen.GetDefaultUserName(), delegate(Task t)
				{
					string name;
					if (!t.TryGetResult(out name))
					{
						return;
					}
					base.<JoinServer>g__startClient|1(name);
				});
				return;
			}
			CS$<>8__locals1.<JoinServer>g__startClient|1(MultiplayerPreferences.Instance.PlayerName);
		}

		// Token: 0x060026CD RID: 9933 RVA: 0x0019DC58 File Offset: 0x0019BE58
		private static Color GetPingTextColor(int ping)
		{
			if (ping < 0)
			{
				return Color.DarkRed;
			}
			return ToolBox.GradientLerp((float)ping / 200f, new Color[]
			{
				GUIStyle.Green,
				GUIStyle.Orange,
				GUIStyle.Red
			});
		}

		// Token: 0x060026CE RID: 9934 RVA: 0x0019DCB8 File Offset: 0x0019BEB8
		public override void Draw(double deltaTime, GraphicsDevice graphics, SpriteBatch spriteBatch)
		{
			graphics.Clear(Color.CornflowerBlue);
			spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, GameMain.ScissorTestEnable, null, null);
			GameMain.MainMenuScreen.DrawBackground(graphics, spriteBatch);
			GUI.Draw(this.Cam, spriteBatch);
			spriteBatch.End();
		}

		// Token: 0x060026CF RID: 9935 RVA: 0x0019DD0B File Offset: 0x0019BF0B
		public override void AddToGUIUpdateList()
		{
			this.menu.AddToGUIUpdateList(false, 0);
		}

		// Token: 0x060026D0 RID: 9936 RVA: 0x0019DD1C File Offset: 0x0019BF1C
		public void StoreServerFilters()
		{
			if (this.loadingServerFilters)
			{
				return;
			}
			foreach (KeyValuePair<Identifier, GUITickBox> filterBox in this.filterTickBoxes)
			{
				ServerListFilters.Instance.SetAttribute(filterBox.Key, filterBox.Value.Selected.ToString());
			}
			foreach (KeyValuePair<Identifier, GUIDropDown> ternaryFilter in this.ternaryFilters)
			{
				ServerListFilters.Instance.SetAttribute(ternaryFilter.Key, ternaryFilter.Value.SelectedData.ToString());
			}
			GameSettings.SaveCurrentConfig();
		}

		// Token: 0x060026D1 RID: 9937 RVA: 0x0019DDFC File Offset: 0x0019BFFC
		public void LoadServerFilters()
		{
			this.loadingServerFilters = true;
			XDocument currentConfigDoc = XMLExtensions.TryLoadXml("config_player.xml");
			ServerListFilters.Init(currentConfigDoc.Root.GetChildElement("serverfilters", StringComparison.OrdinalIgnoreCase));
			foreach (KeyValuePair<Identifier, GUITickBox> filterBox in this.filterTickBoxes)
			{
				filterBox.Value.Selected = ServerListFilters.Instance.GetAttributeBool(filterBox.Key, filterBox.Value.Selected);
			}
			foreach (KeyValuePair<Identifier, GUIDropDown> ternaryFilter in this.ternaryFilters)
			{
				ServerListScreen.TernaryOption ternaryOption = ServerListFilters.Instance.GetAttributeEnum<ServerListScreen.TernaryOption>(ternaryFilter.Key, (ServerListScreen.TernaryOption)ternaryFilter.Value.SelectedData);
				GUIComponent child = ternaryFilter.Value.ListBox.Content.GetChildByUserData(ternaryOption);
				ternaryFilter.Value.Select(ternaryFilter.Value.ListBox.Content.GetChildIndex(child));
			}
			this.loadingServerFilters = false;
		}

		// Token: 0x060026D3 RID: 9939 RVA: 0x0019E040 File Offset: 0x0019C240
		[CompilerGenerated]
		internal static GUIImage <CreateUI>g__arrowImg|53_33(object userData, SpriteEffects sprEffects, ref ServerListScreen.<>c__DisplayClass53_3 A_2)
		{
			return new GUIImage(new RectTransform(new Vector2(0.5f, 0.3f), A_2.btn.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.BothHeight), "GUIButtonVerticalArrow", true)
			{
				CanBeFocused = false,
				UserData = userData,
				SpriteEffects = sprEffects,
				Visible = false
			};
		}

		// Token: 0x060026D5 RID: 9941 RVA: 0x0019E1C8 File Offset: 0x0019C3C8
		[CompilerGenerated]
		internal static string <AddToServerList>g__getServerInfoStr|79_0(ServerInfo serverInfo)
		{
			string str = serverInfo.ServerName + serverInfo.ServerMessage + serverInfo.MaxPlayers.ToString();
			if (str.Length > 200)
			{
				return str.Substring(0, 200);
			}
			return str;
		}

		// Token: 0x060026D6 RID: 9942 RVA: 0x0019E210 File Offset: 0x0019C410
		[CompilerGenerated]
		private string <AddToServerList>g__getCachedServerInfoStr|79_1(ServerInfo serverInfo)
		{
			string cacheKey = serverInfo.Endpoints.First<Endpoint>().StringRepresentation;
			string cachedStr;
			if (!this.serverInfoStringCache.TryGetValue(cacheKey, out cachedStr))
			{
				cachedStr = ServerListScreen.<AddToServerList>g__getServerInfoStr|79_0(serverInfo);
				this.serverInfoStringCache[cacheKey] = cachedStr;
			}
			return cachedStr;
		}

		// Token: 0x0400137A RID: 4986
		private static readonly TimeSpan AllowedRefreshInterval = TimeSpan.FromSeconds(3.0);

		// Token: 0x0400137B RID: 4987
		private DateTime lastRefreshTime = DateTime.Now;

		// Token: 0x0400137C RID: 4988
		private readonly HashSet<string> spamServerCache = new HashSet<string>();

		// Token: 0x0400137D RID: 4989
		private readonly Dictionary<string, string> serverInfoStringCache = new Dictionary<string, string>();

		// Token: 0x0400137E RID: 4990
		private GUIFrame menu;

		// Token: 0x0400137F RID: 4991
		private GUIListBox serverList;

		// Token: 0x04001380 RID: 4992
		private PanelAnimator panelAnimator;

		// Token: 0x04001381 RID: 4993
		private GUIFrame serverPreviewContainer;

		// Token: 0x04001382 RID: 4994
		private GUIListBox serverPreview;

		// Token: 0x04001383 RID: 4995
		private GUIButton joinButton;

		// Token: 0x04001384 RID: 4996
		private Option<ServerInfo> selectedServer;

		// Token: 0x04001385 RID: 4997
		private GUIButton scanServersButton;

		// Token: 0x04001386 RID: 4998
		private readonly Dictionary<ServerListScreen.TabEnum, ServerListScreen.Tab> tabs = new Dictionary<ServerListScreen.TabEnum, ServerListScreen.Tab>();

		// Token: 0x04001387 RID: 4999
		private ServerListScreen.TabEnum _selectedTabBackingField;

		// Token: 0x04001388 RID: 5000
		private ServerProvider serverProvider;

		// Token: 0x0400138A RID: 5002
		private static readonly ImmutableDictionary<ServerListScreen.ColumnLabel, ServerListScreen.Column> columns = (from c in ServerListScreen.Column.Normalize(new ServerListScreen.Column[]
		{
			new ValueTuple<float, ServerListScreen.ColumnLabel>(0.1f, ServerListScreen.ColumnLabel.ServerListCompatible),
			new ValueTuple<float, ServerListScreen.ColumnLabel>(0.1f, ServerListScreen.ColumnLabel.ServerListHasPassword),
			new ValueTuple<float, ServerListScreen.ColumnLabel>(0.7f, ServerListScreen.ColumnLabel.ServerListName),
			new ValueTuple<float, ServerListScreen.ColumnLabel>(0.12f, ServerListScreen.ColumnLabel.ServerListRoundStarted),
			new ValueTuple<float, ServerListScreen.ColumnLabel>(0.08f, ServerListScreen.ColumnLabel.ServerListPlayers),
			new ValueTuple<float, ServerListScreen.ColumnLabel>(0.08f, ServerListScreen.ColumnLabel.ServerListPing)
		})
		select new ValueTuple<ServerListScreen.ColumnLabel, ServerListScreen.Column>(c.Label, c)).ToImmutableDictionary<ServerListScreen.ColumnLabel, ServerListScreen.Column>();

		// Token: 0x0400138B RID: 5003
		private GUILayoutGroup labelHolder;

		// Token: 0x0400138C RID: 5004
		private readonly List<GUITextBlock> labelTexts = new List<GUITextBlock>();

		// Token: 0x0400138D RID: 5005
		private GUITextBox searchBox;

		// Token: 0x0400138E RID: 5006
		private GUITickBox filterSameVersion;

		// Token: 0x0400138F RID: 5007
		private GUITickBox filterPassword;

		// Token: 0x04001390 RID: 5008
		private GUITickBox filterFull;

		// Token: 0x04001391 RID: 5009
		private GUITickBox filterEmpty;

		// Token: 0x04001392 RID: 5010
		private GUIDropDown languageDropdown;

		// Token: 0x04001393 RID: 5011
		private Dictionary<Identifier, GUIDropDown> ternaryFilters;

		// Token: 0x04001394 RID: 5012
		private Dictionary<Identifier, GUITickBox> filterTickBoxes;

		// Token: 0x04001395 RID: 5013
		private Dictionary<Identifier, GUITickBox> playStyleTickBoxes;

		// Token: 0x04001396 RID: 5014
		private Dictionary<Identifier, GUITickBox> gameModeTickBoxes;

		// Token: 0x04001397 RID: 5015
		private GUITickBox filterOffensive;

		// Token: 0x04001398 RID: 5016
		private ServerListScreen.TernaryOption filterFriendlyFireValue;

		// Token: 0x04001399 RID: 5017
		private ServerListScreen.TernaryOption filterKarmaValue;

		// Token: 0x0400139A RID: 5018
		private ServerListScreen.TernaryOption filterTraitorValue;

		// Token: 0x0400139B RID: 5019
		private ServerListScreen.TernaryOption filterVoipValue;

		// Token: 0x0400139C RID: 5020
		private ServerListScreen.TernaryOption filterModdedValue;

		// Token: 0x0400139D RID: 5021
		private ServerListScreen.ColumnLabel sortedBy;

		// Token: 0x0400139E RID: 5022
		private bool sortedAscending = true;

		// Token: 0x0400139F RID: 5023
		private const float sidebarWidth = 0.2f;

		// Token: 0x040013A0 RID: 5024
		private object currentServerDataRecvCallbackObj;

		// Token: 0x040013A1 RID: 5025
		private static readonly Vector2 confirmPopupSize = new Vector2(0.2f, 0.2625f);

		// Token: 0x040013A2 RID: 5026
		private static readonly Point confirmPopupMinSize = new Point(300, 300);

		// Token: 0x040013A3 RID: 5027
		private bool loadingServerFilters;

		// Token: 0x02000CAF RID: 3247
		private enum MsgUserData
		{
			// Token: 0x04004C2A RID: 19498
			RefreshingServerList,
			// Token: 0x04004C2B RID: 19499
			NoServers,
			// Token: 0x04004C2C RID: 19500
			NoMatchingServers
		}

		// Token: 0x02000CB0 RID: 3248
		private enum TernaryOption
		{
			// Token: 0x04004C2E RID: 19502
			Any,
			// Token: 0x04004C2F RID: 19503
			Enabled,
			// Token: 0x04004C30 RID: 19504
			Disabled
		}

		// Token: 0x02000CB1 RID: 3249
		public enum TabEnum
		{
			// Token: 0x04004C32 RID: 19506
			All,
			// Token: 0x04004C33 RID: 19507
			Favorites,
			// Token: 0x04004C34 RID: 19508
			Recent
		}

		// Token: 0x02000CB2 RID: 3250
		public readonly struct Tab
		{
			// Token: 0x17001AD1 RID: 6865
			// (get) Token: 0x06007D93 RID: 32147 RVA: 0x0038A70D File Offset: 0x0038890D
			public IReadOnlyList<ServerInfo> Servers
			{
				get
				{
					return this.servers;
				}
			}

			// Token: 0x06007D94 RID: 32148 RVA: 0x0038A718 File Offset: 0x00388918
			public Tab(ServerListScreen.TabEnum tabEnum, ServerListScreen serverListScreen, GUILayoutGroup tabber, string storage)
			{
				this.Storage = storage;
				this.servers = new List<ServerInfo>();
				RectTransform rectT = new RectTransform(new Vector2(0.33f, 1f), tabber.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(14, 1);
				defaultInterpolatedStringHandler.AppendLiteral("ServerListTab.");
				defaultInterpolatedStringHandler.AppendFormatted<ServerListScreen.TabEnum>(tabEnum);
				this.Button = new GUIButton(rectT, TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear()), Alignment.Center, "GUITabButton", null)
				{
					OnClicked = delegate(GUIButton _, object __)
					{
						serverListScreen.selectedTab = tabEnum;
						return false;
					}
				};
				this.Reload();
			}

			// Token: 0x06007D95 RID: 32149 RVA: 0x0038A7E8 File Offset: 0x003889E8
			public void Reload()
			{
				if (this.Storage.IsNullOrEmpty())
				{
					return;
				}
				this.servers.Clear();
				Exception ex;
				XDocument doc = XMLExtensions.TryLoadXml(this.Storage, out ex);
				if (((doc != null) ? doc.Root : null) == null)
				{
					return;
				}
				List<ServerInfo> list = this.servers;
				IEnumerable<XElement> source = doc.Root.Elements();
				Func<XElement, Option<ServerInfo>> selector;
				if ((selector = ServerListScreen.Tab.<>O.<0>__FromXElement) == null)
				{
					selector = (ServerListScreen.Tab.<>O.<0>__FromXElement = new Func<XElement, Option<ServerInfo>>(ServerInfo.FromXElement));
				}
				list.AddRange(source.Select(selector).NotNone<ServerInfo>().Distinct<ServerInfo>());
			}

			// Token: 0x06007D96 RID: 32150 RVA: 0x0038A86B File Offset: 0x00388A6B
			public bool Contains(ServerInfo info)
			{
				return this.servers.Contains(info);
			}

			// Token: 0x06007D97 RID: 32151 RVA: 0x0038A879 File Offset: 0x00388A79
			public bool Remove(ServerInfo info)
			{
				return this.servers.Remove(info);
			}

			// Token: 0x06007D98 RID: 32152 RVA: 0x0038A887 File Offset: 0x00388A87
			public void AddOrUpdate(ServerInfo info)
			{
				this.servers.Remove(info);
				this.servers.Add(info);
			}

			// Token: 0x06007D99 RID: 32153 RVA: 0x0038A8A2 File Offset: 0x00388AA2
			public void Clear()
			{
				this.servers.Clear();
			}

			// Token: 0x06007D9A RID: 32154 RVA: 0x0038A8B0 File Offset: 0x00388AB0
			public void Save()
			{
				XDocument doc = new XDocument();
				XElement rootElement = new XElement("servers");
				doc.Add(rootElement);
				foreach (ServerInfo info in this.servers)
				{
					rootElement.Add(info.ToXElement());
				}
				doc.SaveSafe(this.Storage, SaveOptions.None, false, 0);
			}

			// Token: 0x04004C35 RID: 19509
			public readonly string Storage;

			// Token: 0x04004C36 RID: 19510
			public readonly GUIButton Button;

			// Token: 0x04004C37 RID: 19511
			private readonly List<ServerInfo> servers;

			// Token: 0x0200153F RID: 5439
			[CompilerGenerated]
			private static class <>O
			{
				// Token: 0x040067E1 RID: 26593
				public static Func<XElement, Option<ServerInfo>> <0>__FromXElement;
			}
		}

		// Token: 0x02000CB3 RID: 3251
		private enum ColumnLabel
		{
			// Token: 0x04004C39 RID: 19513
			ServerListCompatible,
			// Token: 0x04004C3A RID: 19514
			ServerListHasPassword,
			// Token: 0x04004C3B RID: 19515
			ServerListName,
			// Token: 0x04004C3C RID: 19516
			ServerListRoundStarted,
			// Token: 0x04004C3D RID: 19517
			ServerListPlayers,
			// Token: 0x04004C3E RID: 19518
			ServerListPing
		}

		// Token: 0x02000CB4 RID: 3252
		private struct Column
		{
			// Token: 0x06007D9B RID: 32155 RVA: 0x0038A934 File Offset: 0x00388B34
			public static implicit operator ServerListScreen.Column([TupleElementNames(new string[]
			{
				"W",
				"L"
			})] ValueTuple<float, ServerListScreen.ColumnLabel> pair)
			{
				return new ServerListScreen.Column
				{
					RelativeWidth = pair.Item1,
					Label = pair.Item2
				};
			}

			// Token: 0x06007D9C RID: 32156 RVA: 0x0038A964 File Offset: 0x00388B64
			public static ServerListScreen.Column[] Normalize(params ServerListScreen.Column[] columns)
			{
				float totalWidth = (from c in columns
				select c.RelativeWidth).Aggregate((float a, float b) => a + b);
				for (int i = 0; i < columns.Length; i++)
				{
					int num = i;
					columns[num].RelativeWidth = columns[num].RelativeWidth / totalWidth;
				}
				return columns;
			}

			// Token: 0x04004C3F RID: 19519
			public float RelativeWidth;

			// Token: 0x04004C40 RID: 19520
			public ServerListScreen.ColumnLabel Label;
		}

		// Token: 0x02000CB5 RID: 3253
		private enum ReportReason
		{
			// Token: 0x04004C42 RID: 19522
			Spam,
			// Token: 0x04004C43 RID: 19523
			Advertising,
			// Token: 0x04004C44 RID: 19524
			Inappropriate
		}
	}
}
