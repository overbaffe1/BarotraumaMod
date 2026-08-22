using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x0200046F RID: 1135
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class ServerInfo : ISerializableEntity
	{
		// Token: 0x17001364 RID: 4964
		// (get) Token: 0x06004C53 RID: 19539 RVA: 0x002A0E9B File Offset: 0x0029F09B
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public ImmutableArray<Endpoint> Endpoints { [return: Nullable(new byte[]
		{
			0,
			1
		})] get; }

		// Token: 0x17001365 RID: 4965
		// (get) Token: 0x06004C54 RID: 19540 RVA: 0x002A0EA3 File Offset: 0x0029F0A3
		// (set) Token: 0x06004C55 RID: 19541 RVA: 0x002A0EAB File Offset: 0x0029F0AB
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public string ServerName
		{
			get
			{
				return this.serverName;
			}
			set
			{
				this.serverName = value;
				this.cachedNormalizedName = null;
			}
		}

		// Token: 0x17001366 RID: 4966
		// (get) Token: 0x06004C56 RID: 19542 RVA: 0x002A0EBB File Offset: 0x0029F0BB
		// (set) Token: 0x06004C57 RID: 19543 RVA: 0x002A0EC3 File Offset: 0x0029F0C3
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public string ServerMessage
		{
			get
			{
				return this.serverMessage;
			}
			set
			{
				this.serverMessage = value;
				this.cachedNormalizedMessage = null;
			}
		}

		// Token: 0x17001367 RID: 4967
		// (get) Token: 0x06004C58 RID: 19544 RVA: 0x002A0ED3 File Offset: 0x0029F0D3
		public string NormalizedServerName
		{
			get
			{
				if (this.cachedNormalizedName == null)
				{
					this.cachedNormalizedName = Homoglyphs.Normalize(this.ServerName);
				}
				return this.cachedNormalizedName;
			}
		}

		// Token: 0x17001368 RID: 4968
		// (get) Token: 0x06004C59 RID: 19545 RVA: 0x002A0EF4 File Offset: 0x0029F0F4
		public string NormalizedServerMessage
		{
			get
			{
				if (this.cachedNormalizedMessage == null)
				{
					this.cachedNormalizedMessage = Homoglyphs.Normalize(this.ServerMessage);
				}
				return this.cachedNormalizedMessage;
			}
		}

		// Token: 0x17001369 RID: 4969
		// (get) Token: 0x06004C5A RID: 19546 RVA: 0x002A0F18 File Offset: 0x0029F118
		public string NormalizedGameMode
		{
			get
			{
				if (this.cachedNormalizedGameMode == null)
				{
					this.cachedNormalizedGameMode = Homoglyphs.Normalize(this.GameMode.Value);
				}
				return this.cachedNormalizedGameMode;
			}
		}

		// Token: 0x1700136A RID: 4970
		// (get) Token: 0x06004C5B RID: 19547 RVA: 0x002A0F4C File Offset: 0x0029F14C
		// (set) Token: 0x06004C5C RID: 19548 RVA: 0x002A0F54 File Offset: 0x0029F154
		public int PlayerCount { get; set; }

		// Token: 0x1700136B RID: 4971
		// (get) Token: 0x06004C5D RID: 19549 RVA: 0x002A0F5D File Offset: 0x0029F15D
		// (set) Token: 0x06004C5E RID: 19550 RVA: 0x002A0F65 File Offset: 0x0029F165
		[Serialize(0, IsPropertySaveable.Yes, "", "", false)]
		public int MaxPlayers { get; set; }

		// Token: 0x1700136C RID: 4972
		// (get) Token: 0x06004C5F RID: 19551 RVA: 0x002A0F6E File Offset: 0x0029F16E
		// (set) Token: 0x06004C60 RID: 19552 RVA: 0x002A0F76 File Offset: 0x0029F176
		public bool GameStarted { get; set; }

		// Token: 0x1700136D RID: 4973
		// (get) Token: 0x06004C61 RID: 19553 RVA: 0x002A0F7F File Offset: 0x0029F17F
		// (set) Token: 0x06004C62 RID: 19554 RVA: 0x002A0F87 File Offset: 0x0029F187
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool HasPassword { get; set; }

		// Token: 0x1700136E RID: 4974
		// (get) Token: 0x06004C63 RID: 19555 RVA: 0x002A0F90 File Offset: 0x0029F190
		// (set) Token: 0x06004C64 RID: 19556 RVA: 0x002A0F98 File Offset: 0x0029F198
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public Identifier GameMode
		{
			get
			{
				return this.gameMode;
			}
			set
			{
				this.gameMode = value;
				this.cachedNormalizedGameMode = null;
			}
		}

		// Token: 0x1700136F RID: 4975
		// (get) Token: 0x06004C65 RID: 19557 RVA: 0x002A0FA8 File Offset: 0x0029F1A8
		// (set) Token: 0x06004C66 RID: 19558 RVA: 0x002A0FB0 File Offset: 0x0029F1B0
		[Serialize(SelectionMode.Manual, IsPropertySaveable.Yes, "", "", false)]
		public SelectionMode ModeSelectionMode { get; set; }

		// Token: 0x17001370 RID: 4976
		// (get) Token: 0x06004C67 RID: 19559 RVA: 0x002A0FB9 File Offset: 0x0029F1B9
		// (set) Token: 0x06004C68 RID: 19560 RVA: 0x002A0FC1 File Offset: 0x0029F1C1
		[Serialize(SelectionMode.Manual, IsPropertySaveable.Yes, "", "", false)]
		public SelectionMode SubSelectionMode { get; set; }

		// Token: 0x17001371 RID: 4977
		// (get) Token: 0x06004C69 RID: 19561 RVA: 0x002A0FCA File Offset: 0x0029F1CA
		// (set) Token: 0x06004C6A RID: 19562 RVA: 0x002A0FD2 File Offset: 0x0029F1D2
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool AllowSpectating { get; set; }

		// Token: 0x17001372 RID: 4978
		// (get) Token: 0x06004C6B RID: 19563 RVA: 0x002A0FDB File Offset: 0x0029F1DB
		// (set) Token: 0x06004C6C RID: 19564 RVA: 0x002A0FE3 File Offset: 0x0029F1E3
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool VoipEnabled { get; set; }

		// Token: 0x17001373 RID: 4979
		// (get) Token: 0x06004C6D RID: 19565 RVA: 0x002A0FEC File Offset: 0x0029F1EC
		// (set) Token: 0x06004C6E RID: 19566 RVA: 0x002A0FF4 File Offset: 0x0029F1F4
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool KarmaEnabled { get; set; }

		// Token: 0x17001374 RID: 4980
		// (get) Token: 0x06004C6F RID: 19567 RVA: 0x002A0FFD File Offset: 0x0029F1FD
		// (set) Token: 0x06004C70 RID: 19568 RVA: 0x002A1005 File Offset: 0x0029F205
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool FriendlyFireEnabled { get; set; }

		// Token: 0x17001375 RID: 4981
		// (get) Token: 0x06004C71 RID: 19569 RVA: 0x002A100E File Offset: 0x0029F20E
		// (set) Token: 0x06004C72 RID: 19570 RVA: 0x002A1016 File Offset: 0x0029F216
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool AllowRespawn { get; set; }

		// Token: 0x17001376 RID: 4982
		// (get) Token: 0x06004C73 RID: 19571 RVA: 0x002A101F File Offset: 0x0029F21F
		// (set) Token: 0x06004C74 RID: 19572 RVA: 0x002A1027 File Offset: 0x0029F227
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float TraitorProbability { get; set; }

		// Token: 0x17001377 RID: 4983
		// (get) Token: 0x06004C75 RID: 19573 RVA: 0x002A1030 File Offset: 0x0029F230
		// (set) Token: 0x06004C76 RID: 19574 RVA: 0x002A1038 File Offset: 0x0029F238
		[Serialize(PlayStyle.Casual, IsPropertySaveable.Yes, "", "", false)]
		public PlayStyle PlayStyle { get; set; }

		// Token: 0x17001378 RID: 4984
		// (get) Token: 0x06004C77 RID: 19575 RVA: 0x002A1041 File Offset: 0x0029F241
		// (set) Token: 0x06004C78 RID: 19576 RVA: 0x002A1049 File Offset: 0x0029F249
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public LanguageIdentifier Language { get; set; }

		// Token: 0x17001379 RID: 4985
		// (get) Token: 0x06004C79 RID: 19577 RVA: 0x002A1052 File Offset: 0x0029F252
		// (set) Token: 0x06004C7A RID: 19578 RVA: 0x002A105A File Offset: 0x0029F25A
		public bool EosCrossplay { get; set; }

		// Token: 0x1700137A RID: 4986
		// (get) Token: 0x06004C7B RID: 19579 RVA: 0x002A1063 File Offset: 0x0029F263
		// (set) Token: 0x06004C7C RID: 19580 RVA: 0x002A106B File Offset: 0x0029F26B
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public string SelectedSub { get; set; }

		// Token: 0x1700137B RID: 4987
		// (get) Token: 0x06004C7D RID: 19581 RVA: 0x002A1074 File Offset: 0x0029F274
		// (set) Token: 0x06004C7E RID: 19582 RVA: 0x002A107C File Offset: 0x0029F27C
		public Version GameVersion { get; set; }

		// Token: 0x1700137C RID: 4988
		// (get) Token: 0x06004C7F RID: 19583 RVA: 0x002A1085 File Offset: 0x0029F285
		public bool IsModded
		{
			get
			{
				return this.ContentPackages.Any((ServerListContentPackageInfo p) => !GameMain.VanillaContent.NameMatches(p.Name));
			}
		}

		// Token: 0x06004C80 RID: 19584 RVA: 0x002A10B1 File Offset: 0x0029F2B1
		public ServerInfo(params Endpoint[] endpoint) : this(endpoint.ToImmutableArray<Endpoint>())
		{
		}

		// Token: 0x06004C81 RID: 19585 RVA: 0x002A10C4 File Offset: 0x0029F2C4
		public ServerInfo([Nullable(new byte[]
		{
			0,
			1
		})] ImmutableArray<Endpoint> endpoints)
		{
			Option.UnspecifiedNone none = Option.None;
			this.MetadataSource = none;
			this.serverName = "";
			this.serverMessage = "";
			this.gameMode = Identifier.Empty;
			this.SelectedSub = string.Empty;
			this.GameVersion = new Version(0, 0, 0, 0);
			this.Ping = Option<int>.None();
			base..ctor();
			this.SerializableProperties = SerializableProperty.GetProperties(this);
			this.Endpoints = endpoints;
			this.ContentPackages = ImmutableArray<ServerListContentPackageInfo>.Empty;
		}

		// Token: 0x06004C82 RID: 19586 RVA: 0x002A1150 File Offset: 0x0029F350
		public static ServerInfo FromServerEndpoints([Nullable(new byte[]
		{
			0,
			1
		})] ImmutableArray<Endpoint> endpoints, ServerSettings serverSettings)
		{
			ServerInfo serverInfo2 = new ServerInfo(endpoints);
			GameModePreset selectedMode = GameMain.NetLobbyScreen.SelectedMode;
			serverInfo2.GameMode = ((selectedMode != null) ? selectedMode.Identifier : Identifier.Empty);
			serverInfo2.GameStarted = (Screen.Selected != GameMain.NetLobbyScreen);
			serverInfo2.GameVersion = GameMain.Version;
			serverInfo2.PlayerCount = GameMain.Client.ConnectedClients.Count;
			serverInfo2.ContentPackages = (from p in ContentPackageManager.EnabledPackages.All
			select new ServerListContentPackageInfo(p)).ToImmutableArray<ServerListContentPackageInfo>();
			serverInfo2.Ping = GameMain.Client.Ping;
			serverInfo2.ServerName = serverSettings.ServerName;
			serverInfo2.ServerMessage = serverSettings.ServerMessageText;
			serverInfo2.HasPassword = serverSettings.HasPassword;
			serverInfo2.VoipEnabled = serverSettings.VoiceChatEnabled;
			serverInfo2.FriendlyFireEnabled = serverSettings.AllowFriendlyFire;
			serverInfo2.Checked = true;
			ServerInfo serverInfo = serverInfo2;
			Dictionary<Identifier, SerializableProperty> serverInfoSerializableProperties = SerializableProperty.GetProperties(serverInfo);
			Dictionary<Identifier, SerializableProperty> serverSettingsSerializableProperties = SerializableProperty.GetProperties(serverSettings);
			IEnumerable<Identifier> intersection = serverInfoSerializableProperties.Keys.Where(new Func<Identifier, bool>(serverSettingsSerializableProperties.ContainsKey));
			foreach (Identifier key in intersection)
			{
				SerializableProperty propToGet = serverSettingsSerializableProperties[key];
				SerializableProperty propToSet = serverInfoSerializableProperties[key];
				if (propToGet.PropertyInfo.CanRead && propToSet.PropertyInfo.CanWrite)
				{
					propToSet.SetValue(serverInfo, propToGet.GetValue(serverSettings));
				}
			}
			return serverInfo;
		}

		// Token: 0x06004C83 RID: 19587 RVA: 0x002A12E4 File Offset: 0x0029F4E4
		public void CreatePreviewWindow(GUIFrame frame)
		{
			ServerInfo.<>c__DisplayClass103_0 CS$<>8__locals1 = new ServerInfo.<>c__DisplayClass103_0();
			CS$<>8__locals1.<>4__this = this;
			frame.ClearChildren();
			ServerListScreen serverListScreen = GameMain.ServerListScreen;
			RectTransform rectT = new RectTransform(new Vector2(1f, 0f), frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = this.ServerName;
			GUIFont font = GUIStyle.LargeFont;
			GUITextBlock title = new GUITextBlock(rectT, text, null, font, Alignment.Left, false, "", null)
			{
				ToolTip = this.ServerName,
				CanBeFocused = false
			};
			title.Text = ToolBox.LimitString(title.Text, title.Font, (int)((float)title.Rect.Width * 0.85f));
			new GUITextBlock(new RectTransform(new Vector2(1f, 0f), frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.AddPunctuation(':', new LocalizedString[]
			{
				TextManager.Get("ServerListVersion"),
				(this.GameVersion == new Version(0, 0, 0, 0)) ? TextManager.Get("Unknown") : this.GameVersion.ToString()
			}), null, null, Alignment.Left, false, "", null).CanBeFocused = false;
			PlayStyle playStyle = this.PlayStyle;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 1);
			defaultInterpolatedStringHandler.AppendLiteral("PlayStyleBanner.");
			defaultInterpolatedStringHandler.AppendFormatted<PlayStyle>(playStyle);
			GUIComponentStyle componentStyle = GUIStyle.GetComponentStyle(defaultInterpolatedStringHandler.ToStringAndClear());
			Sprite playStyleBannerSprite = (componentStyle != null) ? componentStyle.GetSprite(GUIComponent.ComponentState.None) : null;
			GUIComponent playStyleBanner;
			Color playStyleBannerColor;
			if (playStyleBannerSprite != null)
			{
				float playStyleBannerAspectRatio = (float)playStyleBannerSprite.SourceRect.Width / (float)playStyleBannerSprite.SourceRect.Height;
				playStyleBanner = new GUIImage(new RectTransform(new Vector2(1f, 1f / playStyleBannerAspectRatio), frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothWidth), playStyleBannerSprite, true, null);
				ContentXElement sourceElement = playStyleBannerSprite.SourceElement;
				string key = "bannercolor";
				Color black = Color.Black;
				playStyleBannerColor = sourceElement.GetAttributeColor(key, black);
			}
			else
			{
				playStyleBanner = new GUIFrame(new RectTransform(new ValueTuple<float, float>(1f, 0.2f), frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
				{
					Color = Color.Black,
					DisabledColor = Color.Black,
					OutlineColor = Color.Black,
					PressedColor = Color.Black,
					SelectedColor = Color.Black,
					HoverColor = Color.Black
				};
				playStyleBannerColor = Color.Black;
			}
			RectTransform rectTransform = new RectTransform(new Vector2(0.15f, 0f), playStyleBanner.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			rectTransform.RelativeOffset = new Vector2(0f, 0.06f);
			char punctuationSymbol = ':';
			LocalizedString[] array = new LocalizedString[2];
			array[0] = TextManager.Get("serverplaystyle");
			int num = 1;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(10, 1);
			defaultInterpolatedStringHandler2.AppendLiteral("servertag.");
			defaultInterpolatedStringHandler2.AppendFormatted<PlayStyle>(playStyle);
			array[num] = TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear());
			GUITextBlock playStyleName = new GUITextBlock(rectTransform, TextManager.AddPunctuation(punctuationSymbol, array), new Color?(Color.White), GUIStyle.SmallFont, Alignment.Center, false, "GUISlopedHeader", new Color?(playStyleBannerColor));
			playStyleName.RectTransform.NonScaledSize = (playStyleName.Font.MeasureString(playStyleName.Text, false) + new Vector2(20f, 5f) * GUI.Scale).ToPoint();
			playStyleName.RectTransform.IsFixedSize = true;
			GUITextBlock serverType = new GUITextBlock(new RectTransform(new Vector2(1f, 0f), frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), this.Endpoints.First<Endpoint>().ServerTypeString, null, null, Alignment.TopLeft, false, "", null)
			{
				CanBeFocused = false
			};
			serverType.RectTransform.MinSize = new Point(0, (int)((float)serverType.Rect.Height * 1.5f));
			GUILayoutGroup content = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.6f), frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			GUILayoutGroup buttonContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 0.25f), playStyleBanner.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Normal), true, Anchor.BottomRight);
			GUIFrame guiframe = new GUIFrame(new RectTransform(new Vector2(3.15f, 1.05f), buttonContainer.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Smallest), null, null);
			guiframe.Color = Color.Black * 0.7f;
			guiframe.IgnoreLayoutGroups = true;
			bool isFavorite = serverListScreen.IsFavorite(this);
			GUITickBox guitickBox = new GUITickBox(new RectTransform(Vector2.One, buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Smallest), "", null, "GUIServerListFavoriteTickBox");
			guitickBox.UserData = this;
			guitickBox.Selected = isFavorite;
			guitickBox.ToolTip = ServerInfo.<CreatePreviewWindow>g__favoriteTickBoxToolTip|103_0(isFavorite);
			guitickBox.OnSelected = delegate(GUITickBox tickbox)
			{
				ServerInfo info = (ServerInfo)tickbox.UserData;
				if (tickbox.Selected)
				{
					GameMain.ServerListScreen.AddToFavoriteServers(info);
				}
				else
				{
					GameMain.ServerListScreen.RemoveFromFavoriteServers(info);
				}
				tickbox.ToolTip = ServerInfo.<CreatePreviewWindow>g__favoriteTickBoxToolTip|103_0(tickbox.Selected);
				return true;
			};
			GUIButton guibutton = new GUIButton(new RectTransform(Vector2.One, buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Smallest), Alignment.Center, "GUIServerListReportServer", null);
			guibutton.ToolTip = TextManager.Get("reportserver");
			guibutton.OnClicked = delegate(GUIButton _, object _)
			{
				ServerListScreen.CreateReportPrompt(CS$<>8__locals1.<>4__this);
				return true;
			};
			GUIButton guibutton2 = new GUIButton(new RectTransform(Vector2.One, buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Smallest), Alignment.Center, "GUIServerListHideServer", null);
			guibutton2.ToolTip = TextManager.Get("filterserver");
			guibutton2.OnClicked = delegate(GUIButton _, object _)
			{
				ServerListScreen.CreateFilterServerPrompt(CS$<>8__locals1.<>4__this);
				return true;
			};
			GUILayoutGroup playStyleContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.15f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.01f,
				CanBeFocused = true
			};
			IEnumerable<Identifier> playStyleTags = this.GetPlayStyleTags();
			foreach (Identifier tag in playStyleTags)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(14, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("PlayStyleIcon.");
				defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(tag);
				GUIComponentStyle componentStyle2 = GUIStyle.GetComponentStyle(defaultInterpolatedStringHandler3.ToStringAndClear());
				Sprite playStyleIcon = (componentStyle2 != null) ? componentStyle2.GetSprite(GUIComponent.ComponentState.None) : null;
				if (playStyleIcon != null)
				{
					GUIImage guiimage = new GUIImage(new RectTransform(Vector2.One, playStyleContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), playStyleIcon, true, null);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(21, 1);
					defaultInterpolatedStringHandler4.AppendLiteral("servertagdescription.");
					defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(tag);
					guiimage.ToolTip = TextManager.Get(defaultInterpolatedStringHandler4.ToStringAndClear());
					guiimage.Color = Color.White;
				}
			}
			playStyleContainer.Recalculate();
			new GUIFrame(new RectTransform(new Vector2(1f, 0.025f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			CS$<>8__locals1.serverMsg = new GUIListBox(new RectTransform(new Vector2(1f, 0.3f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				ScrollBarVisible = true
			};
			ServerInfo.<>c__DisplayClass103_0 CS$<>8__locals2 = CS$<>8__locals1;
			RectTransform rectT2 = new RectTransform(new Vector2(1f, 0f), CS$<>8__locals1.serverMsg.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = this.ServerMessage ?? string.Empty;
			font = GUIStyle.SmallFont;
			CS$<>8__locals2.msgText = new GUITextBlock(rectT2, text2, null, font, Alignment.Left, true, "", null)
			{
				CanBeFocused = false
			};
			CS$<>8__locals1.serverMsg.Content.RectTransform.SizeChanged += delegate()
			{
				CS$<>8__locals1.msgText.CalculateHeightFromText(0, false);
			};
			CS$<>8__locals1.msgText.RectTransform.SizeChanged += delegate()
			{
				CS$<>8__locals1.serverMsg.UpdateScrollBarSize();
			};
			GUITextBlock languageLabel = new GUITextBlock(new RectTransform(new Vector2(1f, 0.075f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Language"), null, null, Alignment.Left, false, "", null);
			RectTransform rectT3 = new RectTransform(Vector2.One, languageLabel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			ServerLanguageOptions.LanguageOption? languageOption;
			string text3 = (ServerLanguageOptions.Options.FirstOrNull((ServerLanguageOptions.LanguageOption o) => o.Identifier == CS$<>8__locals1.<>4__this.Language) != null) ? languageOption.GetValueOrDefault().Label : null;
			new GUITextBlock(rectT3, (text3 != null) ? text3 : TextManager.Get("Unknown"), null, null, Alignment.Right, false, "", null);
			GUITextBlock gameMode = new GUITextBlock(new RectTransform(new Vector2(1f, 0.075f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("GameMode"), null, null, Alignment.Left, false, "", null);
			new GUITextBlock(new RectTransform(Vector2.One, gameMode.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get(this.GameMode.IsEmpty ? "Unknown" : ("GameMode." + this.GameMode.ToString())).Fallback(this.GameMode.Value, true), null, null, Alignment.Right, false, "", null);
			if (!string.IsNullOrEmpty(this.SelectedSub))
			{
				GUITextBlock submarineText = new GUITextBlock(new RectTransform(new Vector2(1f, 0.075f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Submarine"), null, null, Alignment.Left, false, "", null);
				new GUITextBlock(new RectTransform(Vector2.One, submarineText.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), this.SelectedSub, null, null, Alignment.Right, false, "", null);
			}
			GUITextBlock playStyleText = new GUITextBlock(new RectTransform(new Vector2(1f, 0.075f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("serverplaystyle"), null, null, Alignment.Left, false, "", null);
			new GUITextBlock(new RectTransform(Vector2.One, playStyleText.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("servertag." + playStyle.ToString()), null, null, Alignment.Right, false, "", null);
			GUITextBlock subSelection = new GUITextBlock(new RectTransform(new Vector2(1f, 0.075f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerListSubSelection"), null, null, Alignment.Left, false, "", null);
			new GUITextBlock(new RectTransform(Vector2.One, subSelection.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get(this.SubSelectionMode.ToString()), null, null, Alignment.Right, false, "", null);
			GUITextBlock modeSelection = new GUITextBlock(new RectTransform(new Vector2(1f, 0.075f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerListModeSelection"), null, null, Alignment.Left, false, "", null);
			new GUITextBlock(new RectTransform(Vector2.One, modeSelection.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get(this.ModeSelectionMode.ToString()), null, null, Alignment.Right, false, "", null);
			if (gameMode.TextSize.X + gameMode.GetChild<GUITextBlock>().TextSize.X > (float)gameMode.Rect.Width || subSelection.TextSize.X + subSelection.GetChild<GUITextBlock>().TextSize.X > (float)subSelection.Rect.Width || modeSelection.TextSize.X + modeSelection.GetChild<GUITextBlock>().TextSize.X > (float)modeSelection.Rect.Width)
			{
				gameMode.Font = (subSelection.Font = (modeSelection.Font = GUIStyle.SmallFont));
				gameMode.GetChild<GUITextBlock>().Font = (subSelection.GetChild<GUITextBlock>().Font = (modeSelection.GetChild<GUITextBlock>().Font = GUIStyle.SmallFont));
				playStyleText.Font = (playStyleText.GetChild<GUITextBlock>().Font = GUIStyle.SmallFont);
			}
			GUITickBox allowSpectating = new GUITickBox(new RectTransform(new Vector2(1f, 0.075f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerListAllowSpectating"), null, "")
			{
				CanBeFocused = false
			};
			allowSpectating.Selected = this.AllowSpectating;
			GUITickBox allowRespawn = new GUITickBox(new RectTransform(new Vector2(1f, 0.075f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsAllowRespawning"), null, "")
			{
				CanBeFocused = false
			};
			allowRespawn.Selected = this.AllowRespawn;
			RectTransform rectT4 = new RectTransform(new Vector2(1f, 0f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text4 = TextManager.Get("ServerListContentPackages");
			font = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT4, text4, null, font, Alignment.Center, false, "", null);
			GUIListBox guilistBox = new GUIListBox(new RectTransform(new Vector2(1f, 0.3f), frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
			guilistBox.ScrollBarVisible = true;
			guilistBox.OnSelected = ((GUIComponent component, object o) => false);
			GUIListBox contentPackageList = guilistBox;
			if (this.ContentPackages.Length == 0)
			{
				new GUITextBlock(new RectTransform(Vector2.One, contentPackageList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Unknown"), null, null, Alignment.Center, false, "", null).CanBeFocused = false;
			}
			else
			{
				ImmutableArray<ServerListContentPackageInfo>.Enumerator enumerator2 = this.ContentPackages.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					ServerListContentPackageInfo package = enumerator2.Current;
					GUITickBox packageText = new GUITickBox(new RectTransform(new Vector2(1f, 0.15f), contentPackageList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
					{
						MinSize = new Point(0, 15)
					}, package.Name, null, "")
					{
						Enabled = false
					};
					packageText.Box.DisabledColor = packageText.Box.Color;
					packageText.TextBlock.DisabledTextColor = packageText.TextBlock.TextColor;
					if (!string.IsNullOrEmpty(package.Hash))
					{
						ContentPackage matchingPackage = ContentPackageManager.AllPackages.FirstOrDefault((ContentPackage contentPackage) => contentPackage.Hash.StringRepresentation == package.Hash);
						ContentPackageId ugcId;
						if (matchingPackage != null)
						{
							packageText.TextColor = GUIStyle.Green;
							packageText.Selected = true;
							matchingPackage.TryFetchUgcDescription(delegate(string description)
							{
								if (packageText.ToolTip.IsNullOrEmpty() && !string.IsNullOrEmpty(description))
								{
									packageText.ToolTip = description + "...";
								}
							});
						}
						else if (package.Id.TryUnwrap(out ugcId) && ugcId is SteamWorkshopId)
						{
							packageText.ToolTip = TextManager.GetWithVariable("ServerListIncompatibleContentPackageWorkshopAvailable", "[contentpackage]", package.Name, FormatCapitals.No);
						}
						else
						{
							packageText.TextColor = (GameMain.VanillaContent.NameMatches(package.Name) ? GUIStyle.Red : GUIStyle.Yellow);
							packageText.ToolTip = TextManager.GetWithVariables("ServerListIncompatibleContentPackage", new ValueTuple<string, string>[]
							{
								new ValueTuple<string, string>("[contentpackage]", package.Name),
								new ValueTuple<string, string>("[hash]", package.Hash)
							});
						}
					}
				}
				if (this.ContentPackageCount > this.ContentPackages.Length)
				{
					new GUITextBlock(new RectTransform(new Vector2(1f, 0.15f), contentPackageList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
					{
						MinSize = new Point(0, 15)
					}, TextManager.GetWithVariable("workshopitemdownloadprompttruncated", "[number]", (this.ContentPackageCount - this.ContentPackages.Length).ToString(), FormatCapitals.No), null, null, Alignment.Left, false, "", null).CanBeFocused = false;
				}
			}
			foreach (GUIComponent c in content.Children)
			{
				GUITextBlock textBlock = c as GUITextBlock;
				if (textBlock != null)
				{
					textBlock.Padding = Vector4.Zero;
				}
			}
		}

		// Token: 0x06004C84 RID: 19588 RVA: 0x002A293C File Offset: 0x002A0B3C
		public IEnumerable<Identifier> GetPlayStyleTags()
		{
			ServerInfo.<GetPlayStyleTags>d__104 <GetPlayStyleTags>d__ = new ServerInfo.<GetPlayStyleTags>d__104(-2);
			<GetPlayStyleTags>d__.<>4__this = this;
			return <GetPlayStyleTags>d__;
		}

		// Token: 0x06004C85 RID: 19589 RVA: 0x002A294C File Offset: 0x002A0B4C
		public void UpdateInfo([Nullable(new byte[]
		{
			1,
			1,
			2
		})] Func<string, string> valueGetter)
		{
			ServerInfo.<>c__DisplayClass105_0 CS$<>8__locals1;
			CS$<>8__locals1.valueGetter = valueGetter;
			this.ServerMessage = ServerInfo.ExtractServerMessage(CS$<>8__locals1.valueGetter);
			Version version;
			if (Version.TryParse(CS$<>8__locals1.valueGetter("version"), out version))
			{
				this.GameVersion = version;
			}
			int playerCount;
			if (int.TryParse(CS$<>8__locals1.valueGetter("playercount"), out playerCount))
			{
				this.PlayerCount = playerCount;
			}
			int maxPlayers;
			if (int.TryParse(CS$<>8__locals1.valueGetter("maxplayers"), out maxPlayers))
			{
				this.MaxPlayers = maxPlayers;
			}
			else if (int.TryParse(CS$<>8__locals1.valueGetter("maxplayernum"), out maxPlayers))
			{
				this.MaxPlayers = maxPlayers;
			}
			SelectionMode modeSelectionMode;
			if (Enum.TryParse<SelectionMode>(CS$<>8__locals1.valueGetter("modeselectionmode"), out modeSelectionMode))
			{
				this.ModeSelectionMode = modeSelectionMode;
			}
			SelectionMode subSelectionMode;
			if (Enum.TryParse<SelectionMode>(CS$<>8__locals1.valueGetter("subselectionmode"), out subSelectionMode))
			{
				this.SubSelectionMode = subSelectionMode;
			}
			this.HasPassword = ServerInfo.<UpdateInfo>g__getBool|105_0("haspassword", ref CS$<>8__locals1);
			this.GameStarted = ServerInfo.<UpdateInfo>g__getBool|105_0("gamestarted", ref CS$<>8__locals1);
			this.KarmaEnabled = ServerInfo.<UpdateInfo>g__getBool|105_0("karmaenabled", ref CS$<>8__locals1);
			this.FriendlyFireEnabled = ServerInfo.<UpdateInfo>g__getBool|105_0("friendlyfireenabled", ref CS$<>8__locals1);
			this.AllowSpectating = ServerInfo.<UpdateInfo>g__getBool|105_0("allowspectating", ref CS$<>8__locals1);
			this.AllowRespawn = ServerInfo.<UpdateInfo>g__getBool|105_0("allowrespawn", ref CS$<>8__locals1);
			this.VoipEnabled = ServerInfo.<UpdateInfo>g__getBool|105_0("voicechatenabled", ref CS$<>8__locals1);
			this.EosCrossplay = ServerInfo.<UpdateInfo>g__getBool|105_0("eoscrossplay", ref CS$<>8__locals1);
			string text = CS$<>8__locals1.valueGetter("gamemode");
			this.GameMode = ((text != null) ? text.ToIdentifier() : Identifier.Empty);
			float traitorProbability;
			if (float.TryParse(CS$<>8__locals1.valueGetter("traitors"), NumberStyles.Any, CultureInfo.InvariantCulture, out traitorProbability))
			{
				this.TraitorProbability = traitorProbability;
			}
			PlayStyle playStyle;
			if (Enum.TryParse<PlayStyle>(CS$<>8__locals1.valueGetter("playstyle"), out playStyle))
			{
				this.PlayStyle = playStyle;
			}
			string text2 = CS$<>8__locals1.valueGetter("language");
			this.Language = ((text2 != null) ? text2.ToLanguageIdentifier() : LanguageIdentifier.None);
			this.SelectedSub = (CS$<>8__locals1.valueGetter("submarine") ?? string.Empty);
			this.ContentPackages = ServerInfo.ExtractContentPackageInfo(this.ServerName, CS$<>8__locals1.valueGetter).ToImmutableArray<ServerListContentPackageInfo>();
			this.ContentPackageCount = this.ContentPackages.Length;
			int packageCount;
			if (int.TryParse(CS$<>8__locals1.valueGetter("packagecount"), out packageCount))
			{
				this.ContentPackageCount = packageCount;
			}
		}

		// Token: 0x06004C86 RID: 19590 RVA: 0x002A2BD0 File Offset: 0x002A0DD0
		private static string ExtractServerMessage([Nullable(new byte[]
		{
			1,
			1,
			2
		})] Func<string, string> valueGetter)
		{
			string msg = valueGetter("message") ?? string.Empty;
			if (!msg.IsNullOrEmpty())
			{
				return msg;
			}
			int messageIndex = 0;
			string splitMessage;
			do
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(7, 1);
				defaultInterpolatedStringHandler.AppendLiteral("message");
				defaultInterpolatedStringHandler.AppendFormatted<int>(messageIndex);
				splitMessage = (valueGetter(defaultInterpolatedStringHandler.ToStringAndClear()) ?? string.Empty);
				msg += splitMessage;
				messageIndex++;
			}
			while (!splitMessage.IsNullOrEmpty());
			return msg;
		}

		// Token: 0x06004C87 RID: 19591 RVA: 0x002A2C48 File Offset: 0x002A0E48
		private static ServerListContentPackageInfo[] ExtractContentPackageInfo(string serverName, [Nullable(new byte[]
		{
			1,
			1,
			2
		})] Func<string, string> valueGetter)
		{
			int individualPackageIndex = 0;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(14, 1);
			defaultInterpolatedStringHandler.AppendLiteral("contentpackage");
			defaultInterpolatedStringHandler.AppendFormatted<int>(individualPackageIndex);
			string individualPackage = valueGetter(defaultInterpolatedStringHandler.ToStringAndClear());
			if (!individualPackage.IsNullOrEmpty())
			{
				List<ServerListContentPackageInfo> contentPackages = new List<ServerListContentPackageInfo>();
				ServerListContentPackageInfo info;
				while (ServerListContentPackageInfo.ParseSingleEntry(individualPackage).TryUnwrap(out info))
				{
					contentPackages.Add(info);
					individualPackageIndex++;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(14, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("contentpackage");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(individualPackageIndex);
					individualPackage = valueGetter(defaultInterpolatedStringHandler2.ToStringAndClear());
					if (individualPackage.IsNullOrEmpty())
					{
						return contentPackages.ToArray();
					}
				}
				return Array.Empty<ServerListContentPackageInfo>();
			}
			string joinedNames = valueGetter("contentpackage");
			string joinedHashes = valueGetter("contentpackagehash");
			string joinedUgcIds = valueGetter("contentpackageid");
			IReadOnlyList<string> readOnlyList;
			if (!joinedNames.IsNullOrEmpty())
			{
				readOnlyList = joinedNames.SplitEscaped(',');
			}
			else
			{
				IReadOnlyList<string> readOnlyList2 = Array.Empty<string>();
				readOnlyList = readOnlyList2;
			}
			IReadOnlyList<string> contentPackageNames = readOnlyList;
			IReadOnlyList<string> readOnlyList3;
			if (!joinedHashes.IsNullOrEmpty())
			{
				readOnlyList3 = joinedHashes.SplitEscaped(',');
			}
			else
			{
				IReadOnlyList<string> readOnlyList2 = Array.Empty<string>();
				readOnlyList3 = readOnlyList2;
			}
			IReadOnlyList<string> contentPackageHashes = readOnlyList3;
			IReadOnlyList<string> readOnlyList4;
			if (!joinedUgcIds.IsNullOrEmpty())
			{
				readOnlyList4 = joinedUgcIds.SplitEscaped(',');
			}
			else
			{
				IReadOnlyList<string> readOnlyList2 = new string[]
				{
					string.Empty
				};
				readOnlyList4 = readOnlyList2;
			}
			IReadOnlyList<string> contentPackageIds = readOnlyList4;
			if (contentPackageNames.Count != contentPackageHashes.Count || contentPackageHashes.Count != contentPackageIds.Count)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(93, 5);
				defaultInterpolatedStringHandler3.AppendLiteral("The number of names, hashes and UGC IDs on server \"");
				defaultInterpolatedStringHandler3.AppendFormatted(serverName);
				defaultInterpolatedStringHandler3.AppendLiteral("\"");
				defaultInterpolatedStringHandler3.AppendLiteral(" doesn't match: ");
				defaultInterpolatedStringHandler3.AppendFormatted<int>(contentPackageNames.Count);
				defaultInterpolatedStringHandler3.AppendLiteral(" names (");
				defaultInterpolatedStringHandler3.AppendFormatted(string.Join(", ", contentPackageNames));
				defaultInterpolatedStringHandler3.AppendLiteral("), ");
				defaultInterpolatedStringHandler3.AppendFormatted<int>(contentPackageHashes.Count);
				defaultInterpolatedStringHandler3.AppendLiteral(" hashes, ");
				defaultInterpolatedStringHandler3.AppendFormatted<int>(contentPackageIds.Count);
				defaultInterpolatedStringHandler3.AppendLiteral(" ids)");
				DebugConsole.Log(defaultInterpolatedStringHandler3.ToStringAndClear());
				return Array.Empty<ServerListContentPackageInfo>();
			}
			List<ServerListContentPackageInfo> contentPackageInfos = new List<ServerListContentPackageInfo>();
			for (int i = 0; i < contentPackageNames.Count; i++)
			{
				string name = contentPackageNames[i];
				string hash = contentPackageHashes[i];
				string ugcId = contentPackageIds[i];
				if (name.Length > 129)
				{
					name = name.Substring(0, 129);
				}
				if (hash.Length > 32)
				{
					hash = hash.Substring(0, 32);
				}
				contentPackageInfos.Add(new ServerListContentPackageInfo(name, hash, ContentPackageId.Parse(ugcId)));
			}
			return contentPackageInfos.ToArray();
		}

		// Token: 0x06004C88 RID: 19592 RVA: 0x002A2EE4 File Offset: 0x002A10E4
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		public static Option<ServerInfo> FromXElement(XElement element)
		{
			List<Endpoint> endpoints = new List<Endpoint>();
			string text;
			if ((text = element.GetAttributeString("Endpoint", null)) == null && (text = element.GetAttributeString("OwnerID", null)) == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
				defaultInterpolatedStringHandler.AppendFormatted(element.GetAttributeString("IP", ""));
				defaultInterpolatedStringHandler.AppendLiteral(":");
				defaultInterpolatedStringHandler.AppendFormatted<int>(element.GetAttributeInt("Port", 0));
				text = defaultInterpolatedStringHandler.ToStringAndClear();
			}
			string endpointStr = text;
			Endpoint endpoint;
			if (Endpoint.Parse(endpointStr).TryUnwrap(out endpoint))
			{
				endpoints.Add(endpoint);
			}
			else
			{
				string[] multipleEndpointStrs = element.GetAttributeStringArray("Endpoints", Array.Empty<string>(), true, false);
				List<Endpoint> list = endpoints;
				IEnumerable<string> source = multipleEndpointStrs;
				Func<string, Option<Endpoint>> selector;
				if ((selector = ServerInfo.<>O.<0>__Parse) == null)
				{
					selector = (ServerInfo.<>O.<0>__Parse = new Func<string, Option<Endpoint>>(Endpoint.Parse));
				}
				list.AddRange(source.Select(selector).NotNone<Endpoint>());
			}
			if (endpoints.Count == 0)
			{
				Option.UnspecifiedNone none = Option.None;
				return none;
			}
			string gameVersionStr = element.GetAttributeString("GameVersion", "");
			Version gameVersion;
			if (!Version.TryParse(gameVersionStr, out gameVersion))
			{
				gameVersion = GameMain.Version;
			}
			ServerInfo info = new ServerInfo(endpoints.ToImmutableArray<Endpoint>())
			{
				GameVersion = gameVersion
			};
			SerializableProperty.DeserializeProperties(info, element);
			info.MetadataSource = ServerInfo.DataSource.Parse(element);
			return Option.Some<ServerInfo>(info);
		}

		// Token: 0x06004C89 RID: 19593 RVA: 0x002A3028 File Offset: 0x002A1228
		public XElement ToXElement()
		{
			XElement element = new XElement(base.GetType().Name);
			element.SetAttributeValue("Endpoints", string.Join(",", from e in this.Endpoints
			select e.StringRepresentation));
			element.SetAttributeValue("GameVersion", this.GameVersion.ToString());
			SerializableProperty.SerializeProperties(this, element, true, false);
			ServerInfo.DataSource dataSource;
			if (this.MetadataSource.TryUnwrap(out dataSource))
			{
				dataSource.Write(element);
			}
			return element;
		}

		// Token: 0x06004C8A RID: 19594 RVA: 0x002A30CC File Offset: 0x002A12CC
		[NullableContext(2)]
		public override bool Equals(object obj)
		{
			ServerInfo other = obj as ServerInfo;
			return other != null && this.Equals(other);
		}

		// Token: 0x06004C8B RID: 19595 RVA: 0x002A30EC File Offset: 0x002A12EC
		public bool Equals(ServerInfo other)
		{
			return other != null && other.Endpoints.Any(new Func<Endpoint, bool>(this.Endpoints.Contains));
		}

		// Token: 0x06004C8C RID: 19596 RVA: 0x002A3115 File Offset: 0x002A1315
		public override int GetHashCode()
		{
			return this.Endpoints.First<Endpoint>().GetHashCode();
		}

		// Token: 0x1700137D RID: 4989
		// (get) Token: 0x06004C8D RID: 19597 RVA: 0x002A3127 File Offset: 0x002A1327
		string ISerializableEntity.Name
		{
			get
			{
				return "ServerInfo";
			}
		}

		// Token: 0x1700137E RID: 4990
		// (get) Token: 0x06004C8E RID: 19598 RVA: 0x002A312E File Offset: 0x002A132E
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; }

		// Token: 0x06004C8F RID: 19599 RVA: 0x002A3136 File Offset: 0x002A1336
		[CompilerGenerated]
		internal static LocalizedString <CreatePreviewWindow>g__favoriteTickBoxToolTip|103_0(bool isFavorite)
		{
			return TextManager.Get(isFavorite ? "RemoveFromFavorites" : "AddToFavorites");
		}

		// Token: 0x06004C90 RID: 19600 RVA: 0x002A314C File Offset: 0x002A134C
		[CompilerGenerated]
		internal static bool <UpdateInfo>g__getBool|105_0(string key, ref ServerInfo.<>c__DisplayClass105_0 A_1)
		{
			string data = A_1.valueGetter(key);
			bool result;
			return bool.TryParse(data, out result) && result;
		}

		// Token: 0x040027DD RID: 10205
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public Option<ServerInfo.DataSource> MetadataSource;

		// Token: 0x040027DE RID: 10206
		[Nullable(2)]
		private string cachedNormalizedName;

		// Token: 0x040027DF RID: 10207
		[Nullable(2)]
		private string cachedNormalizedMessage;

		// Token: 0x040027E0 RID: 10208
		[Nullable(2)]
		private string cachedNormalizedGameMode;

		// Token: 0x040027E1 RID: 10209
		private string serverName;

		// Token: 0x040027E2 RID: 10210
		private string serverMessage;

		// Token: 0x040027E3 RID: 10211
		private Identifier gameMode;

		// Token: 0x040027F5 RID: 10229
		[Nullable(0)]
		public Option<int> Ping;

		// Token: 0x040027F6 RID: 10230
		public bool Checked;

		// Token: 0x040027F7 RID: 10231
		[Nullable(0)]
		public ImmutableArray<ServerListContentPackageInfo> ContentPackages;

		// Token: 0x040027F8 RID: 10232
		public int ContentPackageCount;

		// Token: 0x02001215 RID: 4629
		[Nullable(0)]
		public abstract class DataSource
		{
			// Token: 0x0600930D RID: 37645 RVA: 0x003CB1FE File Offset: 0x003C93FE
			[return: Nullable(new byte[]
			{
				0,
				1
			})]
			public static Option<ServerInfo.DataSource> Parse(XElement element)
			{
				return ReflectionUtils.ParseDerived<ServerInfo.DataSource, XElement>(element);
			}

			// Token: 0x0600930E RID: 37646
			public abstract void Write(XElement element);
		}

		// Token: 0x02001216 RID: 4630
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04005E23 RID: 24099
			[Nullable(0)]
			public static Func<string, Option<Endpoint>> <0>__Parse;
		}
	}
}
