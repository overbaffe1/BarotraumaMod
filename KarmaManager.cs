using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Barotrauma.IO;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000F2 RID: 242
	internal class KarmaManager : ISerializableEntity
	{
		// Token: 0x060022E2 RID: 8930 RVA: 0x00162978 File Offset: 0x00160B78
		public void CreateSettingsFrame(GUIComponent parent)
		{
			if (TextManager.ContainsTag("Karma.ResetKarmaBetweenRounds"))
			{
				this.CreateLabeledTickBox(parent, "ResetKarmaBetweenRounds");
			}
			this.CreateLabeledSlider(parent, 0f, 40f, 1f, "KickBanThreshold");
			if (TextManager.ContainsTag("Karma.KicksBeforeBan"))
			{
				this.CreateLabeledNumberInput(parent, 0, 10, "KicksBeforeBan");
			}
			this.CreateLabeledSlider(parent, 0f, 50f, 1f, "HerpesThreshold");
			this.CreateLabeledSlider(parent, 0f, 0.5f, 0.01f, "KarmaDecay");
			this.CreateLabeledSlider(parent, 50f, 100f, 1f, "KarmaDecayThreshold");
			this.CreateLabeledSlider(parent, 0f, 0.5f, 0.01f, "KarmaIncrease");
			this.CreateLabeledSlider(parent, 0f, 50f, 1f, "KarmaIncreaseThreshold");
			RectTransform rectT = new RectTransform(new Vector2(1f, 0.12f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = TextManager.Get("Karma.PositiveActions");
			GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT, text, null, subHeadingFont, Alignment.Center, false, "", null).CanBeFocused = false;
			this.CreateLabeledSlider(parent, 0f, 1f, 0.01f, "StructureRepairKarmaIncrease");
			this.CreateLabeledSlider(parent, 0f, 1f, 0.01f, "HealFriendlyKarmaIncrease");
			this.CreateLabeledSlider(parent, 0f, 1f, 0.01f, "DamageEnemyKarmaIncrease");
			this.CreateLabeledSlider(parent, 0f, 1f, 0.01f, "ItemRepairKarmaIncrease");
			this.CreateLabeledSlider(parent, 0f, 10f, 0.05f, "ExtinguishFireKarmaIncrease");
			this.CreateLabeledSlider(parent, 0f, 1f, 0.01f, "BallastFloraKarmaIncrease");
			RectTransform rectT2 = new RectTransform(new Vector2(1f, 0.12f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = TextManager.Get("Karma.NegativeActions");
			subHeadingFont = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT2, text2, null, subHeadingFont, Alignment.Center, false, "", null).CanBeFocused = false;
			this.CreateLabeledSlider(parent, 0f, 1f, 0.01f, "StructureDamageKarmaDecrease");
			this.CreateLabeledSlider(parent, 0f, 1f, 0.01f, "DamageFriendlyKarmaDecrease");
			if (TextManager.ContainsTag("Karma.StunFriendlyKarmaDecrease"))
			{
				this.CreateLabeledSlider(parent, 0f, 1f, 0.01f, "StunFriendlyKarmaDecrease");
			}
			if (TextManager.ContainsTag("Karma.StunFriendlyKarmaDecreaseThreshold"))
			{
				this.CreateLabeledSlider(parent, 0f, 10f, 1f, "StunFriendlyKarmaDecreaseThreshold");
			}
			this.CreateLabeledSlider(parent, 0f, 100f, 1f, "ReactorMeltdownKarmaDecrease");
			this.CreateLabeledSlider(parent, 0f, 10f, 0.05f, "ReactorOverheatKarmaDecrease");
			this.CreateLabeledNumberInput(parent, 0, 20, "AllowedWireDisconnectionsPerMinute");
			this.CreateLabeledSlider(parent, 0f, 20f, 0.5f, "WireDisconnectionKarmaDecrease");
			this.CreateLabeledSlider(parent, 0f, 30f, 1f, "SpamFilterKarmaDecrease");
			if (TextManager.ContainsTag("Karma.DangerousItemStealKarmaDecrease"))
			{
				this.CreateLabeledSlider(parent, 0f, 30f, 1f, "DangerousItemStealKarmaDecrease");
			}
			if (TextManager.ContainsTag("Karma.DangerousItemStealBots"))
			{
				this.CreateLabeledTickBox(parent, "DangerousItemStealBots");
			}
			this.CreateLabeledSlider(parent, 0f, 30f, 0.5f, "DangerousItemContainKarmaDecrease");
			this.CreateLabeledTickBox(parent, "IsDangerousItemContainKarmaDecreaseIncremental");
			this.CreateLabeledSlider(parent, 0f, 100f, 1f, "MaxDangerousItemContainKarmaDecrease");
		}

		// Token: 0x060022E3 RID: 8931 RVA: 0x00162D60 File Offset: 0x00160F60
		private void CreateLabeledSlider(GUIComponent parent, float min, float max, float step, string propertyName)
		{
			KarmaManager.<>c__DisplayClass1_0 CS$<>8__locals1 = new KarmaManager.<>c__DisplayClass1_0();
			CS$<>8__locals1.step = step;
			GUILayoutGroup container = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.1f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.05f,
				ToolTip = TextManager.Get("Karma." + propertyName + "ToolTip")
			};
			CS$<>8__locals1.labelText = TextManager.Get("Karma." + propertyName);
			KarmaManager.<>c__DisplayClass1_0 CS$<>8__locals2 = CS$<>8__locals1;
			RectTransform rectT = new RectTransform(new Vector2(0.7f, 1f), container.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = CS$<>8__locals1.labelText;
			GUIFont smallFont = GUIStyle.SmallFont;
			CS$<>8__locals2.label = new GUITextBlock(rectT, text, null, smallFont, Alignment.CenterLeft, false, "", null)
			{
				ToolTip = TextManager.Get("Karma." + propertyName + "ToolTip")
			};
			GUIScrollBar slider = new GUIScrollBar(new RectTransform(new Vector2(0.3f, 1f), container.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), 0.1f, null, "GUISlider", null)
			{
				Step = ((CS$<>8__locals1.step <= 0f) ? 0f : (CS$<>8__locals1.step / (max - min))),
				Range = new Vector2(min, max),
				OnMoved = delegate(GUIScrollBar scrollBar, float barScroll)
				{
					string formattedValueStr = (CS$<>8__locals1.step >= 1f) ? ((int)scrollBar.BarScrollValue).ToString() : scrollBar.BarScrollValue.Format((CS$<>8__locals1.step <= 0.1f) ? 2 : 1);
					CS$<>8__locals1.label.Text = TextManager.AddPunctuation(':', new LocalizedString[]
					{
						CS$<>8__locals1.labelText,
						formattedValueStr
					});
					return true;
				}
			};
			container.RectTransform.MinSize = new Point(0, container.RectTransform.Children.Max((RectTransform c) => c.MinSize.Y));
			GameMain.NetworkMember.ServerSettings.AssignGUIComponent(propertyName, slider);
			slider.OnMoved(slider, slider.BarScroll);
		}

		// Token: 0x060022E4 RID: 8932 RVA: 0x00162FA0 File Offset: 0x001611A0
		private void CreateLabeledNumberInput(GUIComponent parent, int min, int max, string propertyName)
		{
			GUILayoutGroup container = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.1f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.05f,
				ToolTip = TextManager.Get("Karma." + propertyName + "ToolTip")
			};
			LocalizedString labelText = TextManager.Get("Karma." + propertyName);
			RectTransform rectT = new RectTransform(new Vector2(0.7f, 1f), container.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = labelText;
			GUIFont smallFont = GUIStyle.SmallFont;
			new GUITextBlock(rectT, text, null, smallFont, Alignment.CenterLeft, false, "", null).ToolTip = TextManager.Get("Karma." + propertyName + "ToolTip");
			GUINumberInput numInput = new GUINumberInput(new RectTransform(new Vector2(0.3f, 1f), container.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null)
			{
				MinValueInt = new int?(min),
				MaxValueInt = new int?(max)
			};
			container.RectTransform.MinSize = new Point(0, container.RectTransform.Children.Max((RectTransform c) => c.MinSize.Y));
			GameMain.NetworkMember.ServerSettings.AssignGUIComponent(propertyName, numInput);
		}

		// Token: 0x060022E5 RID: 8933 RVA: 0x00163184 File Offset: 0x00161384
		private void CreateLabeledTickBox(GUIComponent parent, string propertyName)
		{
			GUITickBox tickBox = new GUITickBox(new RectTransform(new Vector2(0.3f, 0.1f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Karma." + propertyName), null, "")
			{
				ToolTip = TextManager.Get("Karma." + propertyName + "ToolTip").Fallback("", true)
			};
			GameMain.NetworkMember.ServerSettings.AssignGUIComponent(propertyName, tickBox);
		}

		// Token: 0x1700097A RID: 2426
		// (get) Token: 0x060022E6 RID: 8934 RVA: 0x00163225 File Offset: 0x00161425
		public string Name
		{
			get
			{
				return "KarmaManager";
			}
		}

		// Token: 0x1700097B RID: 2427
		// (get) Token: 0x060022E7 RID: 8935 RVA: 0x0016322C File Offset: 0x0016142C
		// (set) Token: 0x060022E8 RID: 8936 RVA: 0x00163234 File Offset: 0x00161434
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

		// Token: 0x1700097C RID: 2428
		// (get) Token: 0x060022E9 RID: 8937 RVA: 0x0016323D File Offset: 0x0016143D
		// (set) Token: 0x060022EA RID: 8938 RVA: 0x00163245 File Offset: 0x00161445
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool ResetKarmaBetweenRounds { get; set; }

		// Token: 0x1700097D RID: 2429
		// (get) Token: 0x060022EB RID: 8939 RVA: 0x0016324E File Offset: 0x0016144E
		// (set) Token: 0x060022EC RID: 8940 RVA: 0x00163256 File Offset: 0x00161456
		[Serialize(0.1f, IsPropertySaveable.Yes, "", "", false)]
		public float KarmaDecay { get; set; }

		// Token: 0x1700097E RID: 2430
		// (get) Token: 0x060022ED RID: 8941 RVA: 0x0016325F File Offset: 0x0016145F
		// (set) Token: 0x060022EE RID: 8942 RVA: 0x00163267 File Offset: 0x00161467
		[Serialize(50f, IsPropertySaveable.Yes, "", "", false)]
		public float KarmaDecayThreshold { get; set; }

		// Token: 0x1700097F RID: 2431
		// (get) Token: 0x060022EF RID: 8943 RVA: 0x00163270 File Offset: 0x00161470
		// (set) Token: 0x060022F0 RID: 8944 RVA: 0x00163278 File Offset: 0x00161478
		[Serialize(0.15f, IsPropertySaveable.Yes, "", "", false)]
		public float KarmaIncrease { get; set; }

		// Token: 0x17000980 RID: 2432
		// (get) Token: 0x060022F1 RID: 8945 RVA: 0x00163281 File Offset: 0x00161481
		// (set) Token: 0x060022F2 RID: 8946 RVA: 0x00163289 File Offset: 0x00161489
		[Serialize(50f, IsPropertySaveable.Yes, "", "", false)]
		public float KarmaIncreaseThreshold { get; set; }

		// Token: 0x17000981 RID: 2433
		// (get) Token: 0x060022F3 RID: 8947 RVA: 0x00163292 File Offset: 0x00161492
		// (set) Token: 0x060022F4 RID: 8948 RVA: 0x0016329A File Offset: 0x0016149A
		[Serialize(0.05f, IsPropertySaveable.Yes, "", "", false)]
		public float StructureRepairKarmaIncrease { get; set; }

		// Token: 0x17000982 RID: 2434
		// (get) Token: 0x060022F5 RID: 8949 RVA: 0x001632A3 File Offset: 0x001614A3
		// (set) Token: 0x060022F6 RID: 8950 RVA: 0x001632AB File Offset: 0x001614AB
		[Serialize(0.1f, IsPropertySaveable.Yes, "", "", false)]
		public float StructureDamageKarmaDecrease { get; set; }

		// Token: 0x17000983 RID: 2435
		// (get) Token: 0x060022F7 RID: 8951 RVA: 0x001632B4 File Offset: 0x001614B4
		// (set) Token: 0x060022F8 RID: 8952 RVA: 0x001632BC File Offset: 0x001614BC
		[Serialize(15f, IsPropertySaveable.Yes, "", "", false)]
		public float MaxStructureDamageKarmaDecreasePerSecond { get; set; }

		// Token: 0x17000984 RID: 2436
		// (get) Token: 0x060022F9 RID: 8953 RVA: 0x001632C5 File Offset: 0x001614C5
		// (set) Token: 0x060022FA RID: 8954 RVA: 0x001632CD File Offset: 0x001614CD
		[Serialize(0.03f, IsPropertySaveable.Yes, "", "", false)]
		public float ItemRepairKarmaIncrease { get; set; }

		// Token: 0x17000985 RID: 2437
		// (get) Token: 0x060022FB RID: 8955 RVA: 0x001632D6 File Offset: 0x001614D6
		// (set) Token: 0x060022FC RID: 8956 RVA: 0x001632DE File Offset: 0x001614DE
		[Serialize(0.5f, IsPropertySaveable.Yes, "", "", false)]
		public float ReactorOverheatKarmaDecrease { get; set; }

		// Token: 0x17000986 RID: 2438
		// (get) Token: 0x060022FD RID: 8957 RVA: 0x001632E7 File Offset: 0x001614E7
		// (set) Token: 0x060022FE RID: 8958 RVA: 0x001632EF File Offset: 0x001614EF
		[Serialize(30f, IsPropertySaveable.Yes, "", "", false)]
		public float ReactorMeltdownKarmaDecrease { get; set; }

		// Token: 0x17000987 RID: 2439
		// (get) Token: 0x060022FF RID: 8959 RVA: 0x001632F8 File Offset: 0x001614F8
		// (set) Token: 0x06002300 RID: 8960 RVA: 0x00163300 File Offset: 0x00161500
		[Serialize(0.1f, IsPropertySaveable.Yes, "", "", false)]
		public float DamageEnemyKarmaIncrease { get; set; }

		// Token: 0x17000988 RID: 2440
		// (get) Token: 0x06002301 RID: 8961 RVA: 0x00163309 File Offset: 0x00161509
		// (set) Token: 0x06002302 RID: 8962 RVA: 0x00163311 File Offset: 0x00161511
		[Serialize(0.2f, IsPropertySaveable.Yes, "", "", false)]
		public float HealFriendlyKarmaIncrease { get; set; }

		// Token: 0x17000989 RID: 2441
		// (get) Token: 0x06002303 RID: 8963 RVA: 0x0016331A File Offset: 0x0016151A
		// (set) Token: 0x06002304 RID: 8964 RVA: 0x00163322 File Offset: 0x00161522
		[Serialize(0.25f, IsPropertySaveable.Yes, "", "", false)]
		public float DamageFriendlyKarmaDecrease { get; set; }

		// Token: 0x1700098A RID: 2442
		// (get) Token: 0x06002305 RID: 8965 RVA: 0x0016332B File Offset: 0x0016152B
		// (set) Token: 0x06002306 RID: 8966 RVA: 0x00163333 File Offset: 0x00161533
		[Serialize(0.25f, IsPropertySaveable.Yes, "", "", false)]
		public float StunFriendlyKarmaDecrease { get; set; }

		// Token: 0x1700098B RID: 2443
		// (get) Token: 0x06002307 RID: 8967 RVA: 0x0016333C File Offset: 0x0016153C
		// (set) Token: 0x06002308 RID: 8968 RVA: 0x00163344 File Offset: 0x00161544
		[Serialize(0.3f, IsPropertySaveable.Yes, "", "", false)]
		public float StunFriendlyKarmaDecreaseThreshold { get; set; }

		// Token: 0x1700098C RID: 2444
		// (get) Token: 0x06002309 RID: 8969 RVA: 0x0016334D File Offset: 0x0016154D
		// (set) Token: 0x0600230A RID: 8970 RVA: 0x00163355 File Offset: 0x00161555
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		public float ExtinguishFireKarmaIncrease { get; set; }

		// Token: 0x1700098D RID: 2445
		// (get) Token: 0x0600230B RID: 8971 RVA: 0x0016335E File Offset: 0x0016155E
		// (set) Token: 0x0600230C RID: 8972 RVA: 0x00163366 File Offset: 0x00161566
		[Serialize(15f, IsPropertySaveable.Yes, "", "", false)]
		public float DangerousItemStealKarmaDecrease { get; set; }

		// Token: 0x1700098E RID: 2446
		// (get) Token: 0x0600230D RID: 8973 RVA: 0x0016336F File Offset: 0x0016156F
		// (set) Token: 0x0600230E RID: 8974 RVA: 0x00163377 File Offset: 0x00161577
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool DangerousItemStealBots { get; set; }

		// Token: 0x1700098F RID: 2447
		// (get) Token: 0x0600230F RID: 8975 RVA: 0x00163380 File Offset: 0x00161580
		// (set) Token: 0x06002310 RID: 8976 RVA: 0x00163388 File Offset: 0x00161588
		[Serialize(0.05f, IsPropertySaveable.Yes, "", "", false)]
		public float BallastFloraKarmaIncrease { get; set; }

		// Token: 0x17000990 RID: 2448
		// (get) Token: 0x06002311 RID: 8977 RVA: 0x00163391 File Offset: 0x00161591
		// (set) Token: 0x06002312 RID: 8978 RVA: 0x00163399 File Offset: 0x00161599
		[Serialize(5, IsPropertySaveable.Yes, "", "", false)]
		public int AllowedWireDisconnectionsPerMinute
		{
			get
			{
				return this.allowedWireDisconnectionsPerMinute;
			}
			set
			{
				this.allowedWireDisconnectionsPerMinute = Math.Max(0, value);
			}
		}

		// Token: 0x17000991 RID: 2449
		// (get) Token: 0x06002313 RID: 8979 RVA: 0x001633A8 File Offset: 0x001615A8
		// (set) Token: 0x06002314 RID: 8980 RVA: 0x001633B0 File Offset: 0x001615B0
		[Serialize(6f, IsPropertySaveable.Yes, "", "", false)]
		public float WireDisconnectionKarmaDecrease { get; set; }

		// Token: 0x17000992 RID: 2450
		// (get) Token: 0x06002315 RID: 8981 RVA: 0x001633B9 File Offset: 0x001615B9
		// (set) Token: 0x06002316 RID: 8982 RVA: 0x001633C1 File Offset: 0x001615C1
		[Serialize(0.15f, IsPropertySaveable.Yes, "", "", false)]
		public float SteerSubKarmaIncrease { get; set; }

		// Token: 0x17000993 RID: 2451
		// (get) Token: 0x06002317 RID: 8983 RVA: 0x001633CA File Offset: 0x001615CA
		// (set) Token: 0x06002318 RID: 8984 RVA: 0x001633D2 File Offset: 0x001615D2
		[Serialize(15f, IsPropertySaveable.Yes, "", "", false)]
		public float SpamFilterKarmaDecrease { get; set; }

		// Token: 0x17000994 RID: 2452
		// (get) Token: 0x06002319 RID: 8985 RVA: 0x001633DB File Offset: 0x001615DB
		// (set) Token: 0x0600231A RID: 8986 RVA: 0x001633E3 File Offset: 0x001615E3
		[Serialize(40f, IsPropertySaveable.Yes, "", "", false)]
		public float HerpesThreshold { get; set; }

		// Token: 0x17000995 RID: 2453
		// (get) Token: 0x0600231B RID: 8987 RVA: 0x001633EC File Offset: 0x001615EC
		// (set) Token: 0x0600231C RID: 8988 RVA: 0x001633F4 File Offset: 0x001615F4
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		public float KickBanThreshold { get; set; }

		// Token: 0x17000996 RID: 2454
		// (get) Token: 0x0600231D RID: 8989 RVA: 0x001633FD File Offset: 0x001615FD
		// (set) Token: 0x0600231E RID: 8990 RVA: 0x00163405 File Offset: 0x00161605
		[Serialize(0, IsPropertySaveable.Yes, "", "", false)]
		public int KicksBeforeBan { get; set; }

		// Token: 0x17000997 RID: 2455
		// (get) Token: 0x0600231F RID: 8991 RVA: 0x0016340E File Offset: 0x0016160E
		// (set) Token: 0x06002320 RID: 8992 RVA: 0x00163416 File Offset: 0x00161616
		[Serialize(10f, IsPropertySaveable.Yes, "", "", false)]
		public float KarmaNotificationInterval { get; set; }

		// Token: 0x17000998 RID: 2456
		// (get) Token: 0x06002321 RID: 8993 RVA: 0x0016341F File Offset: 0x0016161F
		// (set) Token: 0x06002322 RID: 8994 RVA: 0x00163427 File Offset: 0x00161627
		[Serialize(120f, IsPropertySaveable.Yes, "", "", false)]
		public float AllowedRetaliationTime { get; set; }

		// Token: 0x17000999 RID: 2457
		// (get) Token: 0x06002323 RID: 8995 RVA: 0x00163430 File Offset: 0x00161630
		// (set) Token: 0x06002324 RID: 8996 RVA: 0x00163438 File Offset: 0x00161638
		[Serialize(5f, IsPropertySaveable.Yes, "", "", false)]
		public float DangerousItemContainKarmaDecrease { get; set; }

		// Token: 0x1700099A RID: 2458
		// (get) Token: 0x06002325 RID: 8997 RVA: 0x00163441 File Offset: 0x00161641
		// (set) Token: 0x06002326 RID: 8998 RVA: 0x00163449 File Offset: 0x00161649
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool IsDangerousItemContainKarmaDecreaseIncremental { get; set; }

		// Token: 0x1700099B RID: 2459
		// (get) Token: 0x06002327 RID: 8999 RVA: 0x00163452 File Offset: 0x00161652
		// (set) Token: 0x06002328 RID: 9000 RVA: 0x0016345A File Offset: 0x0016165A
		[Serialize(30f, IsPropertySaveable.Yes, "", "", false)]
		public float MaxDangerousItemContainKarmaDecrease { get; set; }

		// Token: 0x06002329 RID: 9001 RVA: 0x00163464 File Offset: 0x00161664
		public KarmaManager()
		{
			XDocument doc = null;
			int maxLoadRetries = 4;
			for (int i = 0; i <= maxLoadRetries; i++)
			{
				try
				{
					doc = XMLExtensions.TryLoadXml(KarmaManager.ConfigFile);
					break;
				}
				catch (IOException)
				{
					if (i == maxLoadRetries)
					{
						break;
					}
					DebugConsole.NewMessage("Opening karma settings file \"" + KarmaManager.ConfigFile + "\" failed, retrying in 250 ms...", null, false);
					Thread.Sleep(250);
				}
			}
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, (doc != null) ? doc.Root : null);
			if (((doc != null) ? doc.Root : null) != null)
			{
				this.Presets["custom"] = doc.Root;
				foreach (XElement subElement in doc.Root.Elements())
				{
					string presetName = subElement.GetAttributeString("name", "");
					this.Presets[presetName.ToLowerInvariant()] = subElement;
				}
				NetworkMember networkMember = GameMain.NetworkMember;
				string text;
				if (networkMember == null)
				{
					text = null;
				}
				else
				{
					ServerSettings serverSettings = networkMember.ServerSettings;
					text = ((serverSettings != null) ? serverSettings.KarmaPreset : null);
				}
				this.SelectPreset(text ?? "default");
			}
			this.herpesAffliction = AfflictionPrefab.List.FirstOrDefault((AfflictionPrefab ap) => ap.Identifier == "spaceherpes");
		}

		// Token: 0x0600232A RID: 9002 RVA: 0x001635E8 File Offset: 0x001617E8
		public void SelectPreset(string presetName)
		{
			if (string.IsNullOrEmpty(presetName))
			{
				return;
			}
			presetName = presetName.ToLowerInvariant();
			if (this.Presets.ContainsKey(presetName))
			{
				SerializableProperty.DeserializeProperties(this, this.Presets[presetName]);
				return;
			}
			if (this.Presets.ContainsKey("custom"))
			{
				SerializableProperty.DeserializeProperties(this, this.Presets["custom"]);
			}
		}

		// Token: 0x0600232B RID: 9003 RVA: 0x00163651 File Offset: 0x00161851
		public void SaveCustomPreset()
		{
			if (this.Presets.ContainsKey("custom"))
			{
				SerializableProperty.SerializeProperties(this, this.Presets["custom"], true, false);
			}
		}

		// Token: 0x0600232C RID: 9004 RVA: 0x00163680 File Offset: 0x00161880
		public void Save()
		{
			XDocument doc = new XDocument(new object[]
			{
				new XElement(this.Name)
			});
			foreach (KeyValuePair<string, XElement> preset in this.Presets)
			{
				doc.Root.Add(preset.Value);
			}
			XmlWriterSettings settings = new XmlWriterSettings
			{
				Indent = true,
				NewLineOnAttributes = true
			};
			int maxLoadRetries = 4;
			for (int i = 0; i <= maxLoadRetries; i++)
			{
				try
				{
					using (XmlWriter writer = XmlWriter.Create(KarmaManager.ConfigFile, settings))
					{
						doc.SaveSafe(writer);
					}
					break;
				}
				catch (IOException)
				{
					if (i == maxLoadRetries)
					{
						throw;
					}
					DebugConsole.NewMessage("Saving karma settings file file \"" + KarmaManager.ConfigFile + "\" failed, retrying in 250 ms...", null, false);
					Thread.Sleep(250);
				}
			}
		}

		// Token: 0x0600232D RID: 9005 RVA: 0x0016379C File Offset: 0x0016199C
		// Note: this type is marked as 'beforefieldinit'.
		static KarmaManager()
		{
			ReadOnlySpan<char> str = "Data";
			char directorySeparatorChar = Path.DirectorySeparatorChar;
			KarmaManager.ConfigFile = str + new ReadOnlySpan<char>(ref directorySeparatorChar) + "karmasettings.xml";
		}

		// Token: 0x04001189 RID: 4489
		public static readonly string ConfigFile;

		// Token: 0x0400119F RID: 4511
		private int allowedWireDisconnectionsPerMinute;

		// Token: 0x040011AB RID: 4523
		private readonly AfflictionPrefab herpesAffliction;

		// Token: 0x040011AC RID: 4524
		public Dictionary<string, XElement> Presets = new Dictionary<string, XElement>();
	}
}
