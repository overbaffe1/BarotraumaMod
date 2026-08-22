using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000A1 RID: 161
	public static class GUIStyle
	{
		// Token: 0x0600146E RID: 5230 RVA: 0x000C0F80 File Offset: 0x000BF180
		static GUIStyle()
		{
			GUIStyle.<>c__DisplayClass4_0 CS$<>8__locals1;
			CS$<>8__locals1.guiClassProperties = typeof(GUIStyle).GetFields(BindingFlags.Static | BindingFlags.Public);
			GUIStyle.Fonts = GUIStyle.<.cctor>g__getPropertiesOfType|4_0<GUIFont>(ref CS$<>8__locals1);
			GUIStyle.Sprites = GUIStyle.<.cctor>g__getPropertiesOfType|4_0<GUISprite>(ref CS$<>8__locals1);
			GUIStyle.SpriteSheets = GUIStyle.<.cctor>g__getPropertiesOfType|4_0<GUISpriteSheet>(ref CS$<>8__locals1);
			GUIStyle.Colors = GUIStyle.<.cctor>g__getPropertiesOfType|4_0<GUIColor>(ref CS$<>8__locals1);
		}

		// Token: 0x17000531 RID: 1329
		// (get) Token: 0x0600146F RID: 5231 RVA: 0x000C17AC File Offset: 0x000BF9AC
		public static Point ItemFrameMargin
		{
			get
			{
				Point size = GUIStyle.defaultItemFrameMargin.Multiply(GUI.SlicedSpriteScale);
				GUIComponentStyle style = GUIStyle.GetComponentStyle("ItemUI");
				UISprite sprite = (style != null) ? style.Sprites[GUIComponent.ComponentState.None].First<UISprite>() : null;
				if (sprite != null)
				{
					size.X = Math.Min(sprite.Slices[0].Width + sprite.Slices[2].Width, size.X);
					size.Y = Math.Min(sprite.Slices[0].Height + sprite.Slices[6].Height, size.Y);
				}
				return size;
			}
		}

		// Token: 0x17000532 RID: 1330
		// (get) Token: 0x06001470 RID: 5232 RVA: 0x000C185C File Offset: 0x000BFA5C
		public static int ItemFrameTopBarHeight
		{
			get
			{
				GUIComponentStyle style = GUIStyle.GetComponentStyle("ItemUI");
				UISprite sprite = (style != null) ? style.Sprites[GUIComponent.ComponentState.None].First<UISprite>() : null;
				return (int)Math.Min((float)((sprite != null) ? sprite.Slices[0].Height : 0), (float)(GUIStyle.defaultItemFrameMargin.Y / 2) * GUI.SlicedSpriteScale);
			}
		}

		// Token: 0x17000533 RID: 1331
		// (get) Token: 0x06001471 RID: 5233 RVA: 0x000C18BD File Offset: 0x000BFABD
		public static Point ItemFrameOffset
		{
			get
			{
				return new Point(0, 3).Multiply(GUI.SlicedSpriteScale);
			}
		}

		// Token: 0x06001472 RID: 5234 RVA: 0x000C18D0 File Offset: 0x000BFAD0
		public static GUIComponentStyle GetComponentStyle(string styleName)
		{
			return GUIStyle.GetComponentStyle(styleName.ToIdentifier());
		}

		// Token: 0x06001473 RID: 5235 RVA: 0x000C18E0 File Offset: 0x000BFAE0
		public static GUIComponentStyle GetComponentStyle(Identifier identifier)
		{
			GUIComponentStyle style;
			if (!GUIStyle.ComponentStyles.TryGet(identifier, out style))
			{
				return null;
			}
			return style;
		}

		// Token: 0x06001474 RID: 5236 RVA: 0x000C18FF File Offset: 0x000BFAFF
		public static void Apply(GUIComponent targetComponent, string styleName = "", GUIComponent parent = null)
		{
			GUIStyle.Apply(targetComponent, styleName.ToIdentifier(), parent);
		}

		// Token: 0x06001475 RID: 5237 RVA: 0x000C1910 File Offset: 0x000BFB10
		public static void Apply(GUIComponent targetComponent, Identifier styleName, GUIComponent parent = null)
		{
			GUIComponentStyle componentStyle;
			if (parent != null)
			{
				GUIComponentStyle parentStyle = parent.Style;
				if (parentStyle == null)
				{
					Identifier parentStyleName = ReflectionUtils.GetTypeNameWithoutGenericArity(parent.GetType());
					if (!GUIStyle.ComponentStyles.ContainsKey(parentStyleName))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(28, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Couldn't find a GUI style \"");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(parentStyleName);
						defaultInterpolatedStringHandler.AppendLiteral("\"");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
						return;
					}
					parentStyle = GUIStyle.ComponentStyles[parentStyleName];
				}
				Identifier childStyleName = styleName.IsEmpty ? ReflectionUtils.GetTypeNameWithoutGenericArity(targetComponent.GetType()) : styleName;
				parentStyle.ChildStyles.TryGetValue(childStyleName, out componentStyle);
			}
			else
			{
				Identifier styleIdentifier = styleName.ToIdentifier<Identifier>();
				if (styleIdentifier == Identifier.Empty)
				{
					styleIdentifier = ReflectionUtils.GetTypeNameWithoutGenericArity(targetComponent.GetType());
				}
				if (!GUIStyle.ComponentStyles.ContainsKey(styleIdentifier))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(28, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("Couldn't find a GUI style \"");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(styleIdentifier);
					defaultInterpolatedStringHandler2.AppendLiteral("\"");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
					return;
				}
				componentStyle = GUIStyle.ComponentStyles[styleIdentifier];
			}
			targetComponent.ApplyStyle(componentStyle);
		}

		// Token: 0x06001476 RID: 5238 RVA: 0x000C1A37 File Offset: 0x000BFC37
		public static GUIColor GetQualityColor(int quality)
		{
			switch (quality)
			{
			case -1:
				return GUIStyle.ItemQualityColorPoor;
			case 1:
				return GUIStyle.ItemQualityColorGood;
			case 2:
				return GUIStyle.ItemQualityColorExcellent;
			case 3:
				return GUIStyle.ItemQualityColorMasterwork;
			}
			return GUIStyle.ItemQualityColorNormal;
		}

		// Token: 0x06001477 RID: 5239 RVA: 0x000C1A74 File Offset: 0x000BFC74
		public static void RecalculateFonts()
		{
			foreach (GUIFont font in GUIStyle.Fonts.Values)
			{
				font.Prefabs.ForEach(delegate(GUIFontPrefab p)
				{
					p.LoadFont();
				});
			}
		}

		// Token: 0x06001478 RID: 5240 RVA: 0x000C1AE8 File Offset: 0x000BFCE8
		public static void RecalculateSizeRestrictions()
		{
			foreach (GUIComponentStyle componentStyle in GUIStyle.ComponentStyles)
			{
				componentStyle.RefreshSize();
			}
		}

		// Token: 0x06001479 RID: 5241 RVA: 0x000C1B34 File Offset: 0x000BFD34
		[CompilerGenerated]
		internal static ImmutableDictionary<Identifier, T> <.cctor>g__getPropertiesOfType|4_0<T>(ref GUIStyle.<>c__DisplayClass4_0 A_0) where T : class
		{
			return (from p in A_0.guiClassProperties
			where p.FieldType == typeof(T)
			select new ValueTuple<Identifier, T>(p.Name.ToIdentifier(), p.GetValue(null) as T)).ToImmutableDictionary<Identifier, T>();
		}

		// Token: 0x04000A0B RID: 2571
		public static readonly ImmutableDictionary<Identifier, GUIFont> Fonts;

		// Token: 0x04000A0C RID: 2572
		public static readonly ImmutableDictionary<Identifier, GUISprite> Sprites;

		// Token: 0x04000A0D RID: 2573
		public static readonly ImmutableDictionary<Identifier, GUISpriteSheet> SpriteSheets;

		// Token: 0x04000A0E RID: 2574
		public static readonly ImmutableDictionary<Identifier, GUIColor> Colors;

		// Token: 0x04000A0F RID: 2575
		public static readonly PrefabCollection<GUIComponentStyle> ComponentStyles = new PrefabCollection<GUIComponentStyle>();

		// Token: 0x04000A10 RID: 2576
		public static readonly GUIFont Font = new GUIFont("Font");

		// Token: 0x04000A11 RID: 2577
		public static readonly GUIFont UnscaledSmallFont = new GUIFont("UnscaledSmallFont");

		// Token: 0x04000A12 RID: 2578
		public static readonly GUIFont SmallFont = new GUIFont("SmallFont");

		// Token: 0x04000A13 RID: 2579
		public static readonly GUIFont LargeFont = new GUIFont("LargeFont");

		// Token: 0x04000A14 RID: 2580
		public static readonly GUIFont SubHeadingFont = new GUIFont("SubHeadingFont");

		// Token: 0x04000A15 RID: 2581
		public static readonly GUIFont DigitalFont = new GUIFont("DigitalFont");

		// Token: 0x04000A16 RID: 2582
		public static readonly GUIFont HotkeyFont = new GUIFont("HotkeyFont");

		// Token: 0x04000A17 RID: 2583
		public static readonly GUIFont MonospacedFont = new GUIFont("MonospacedFont");

		// Token: 0x04000A18 RID: 2584
		public static readonly GUICursor CursorSprite = new GUICursor("Cursor");

		// Token: 0x04000A19 RID: 2585
		public static readonly GUISprite SubmarineLocationIcon = new GUISprite("SubmarineLocationIcon");

		// Token: 0x04000A1A RID: 2586
		public static readonly GUISprite Arrow = new GUISprite("Arrow");

		// Token: 0x04000A1B RID: 2587
		public static readonly GUISprite SpeechBubbleIcon = new GUISprite("SpeechBubbleIcon");

		// Token: 0x04000A1C RID: 2588
		public static readonly GUISprite SpeechBubbleIconSliced = new GUISprite("SpeechBubbleIconSliced");

		// Token: 0x04000A1D RID: 2589
		public static readonly GUISprite InteractionLabelBackground = new GUISprite("InteractionLabelBackground");

		// Token: 0x04000A1E RID: 2590
		public static readonly GUISprite BrokenIcon = new GUISprite("BrokenIcon");

		// Token: 0x04000A1F RID: 2591
		public static readonly GUISprite YouAreHereCircle = new GUISprite("YouAreHereCircle");

		// Token: 0x04000A20 RID: 2592
		public static readonly GUISprite SubLocationIcon = new GUISprite("SubLocationIcon");

		// Token: 0x04000A21 RID: 2593
		public static readonly GUISprite ShuttleIcon = new GUISprite("ShuttleIcon");

		// Token: 0x04000A22 RID: 2594
		public static readonly GUISprite WreckIcon = new GUISprite("WreckIcon");

		// Token: 0x04000A23 RID: 2595
		public static readonly GUISprite CaveIcon = new GUISprite("CaveIcon");

		// Token: 0x04000A24 RID: 2596
		public static readonly GUISprite OutpostIcon = new GUISprite("OutpostIcon");

		// Token: 0x04000A25 RID: 2597
		public static readonly GUISprite RuinIcon = new GUISprite("RuinIcon");

		// Token: 0x04000A26 RID: 2598
		public static readonly GUISprite EnemyIcon = new GUISprite("EnemyIcon");

		// Token: 0x04000A27 RID: 2599
		public static readonly GUISprite CorpseIcon = new GUISprite("CorpseIcon");

		// Token: 0x04000A28 RID: 2600
		public static readonly GUISprite BeaconIcon = new GUISprite("BeaconIcon");

		// Token: 0x04000A29 RID: 2601
		public static readonly GUISprite Radiation = new GUISprite("Radiation");

		// Token: 0x04000A2A RID: 2602
		public static readonly GUISpriteSheet RadiationAnimSpriteSheet = new GUISpriteSheet("RadiationAnimSpriteSheet");

		// Token: 0x04000A2B RID: 2603
		public static readonly GUISpriteSheet SavingIndicator = new GUISpriteSheet("SavingIndicator");

		// Token: 0x04000A2C RID: 2604
		public static readonly GUISpriteSheet GenericThrobber = new GUISpriteSheet("GenericThrobber");

		// Token: 0x04000A2D RID: 2605
		public static readonly GUISprite UIGlow = new GUISprite("UIGlow");

		// Token: 0x04000A2E RID: 2606
		public static readonly GUISprite TalentGlow = new GUISprite("TalentGlow");

		// Token: 0x04000A2F RID: 2607
		public static readonly GUISprite PingCircle = new GUISprite("PingCircle");

		// Token: 0x04000A30 RID: 2608
		public static readonly GUISprite UIGlowCircular = new GUISprite("UIGlowCircular");

		// Token: 0x04000A31 RID: 2609
		public static readonly GUISprite UIGlowSolidCircular = new GUISprite("UIGlowSolidCircular");

		// Token: 0x04000A32 RID: 2610
		public static readonly GUISprite UIThermalGlow = new GUISprite("UIGlowSolidCircular");

		// Token: 0x04000A33 RID: 2611
		public static readonly GUISprite ButtonPulse = new GUISprite("ButtonPulse");

		// Token: 0x04000A34 RID: 2612
		public static readonly GUISprite WalletPortraitBG = new GUISprite("WalletPortraitBG");

		// Token: 0x04000A35 RID: 2613
		public static readonly GUISprite CrewWalletIconSmall = new GUISprite("CrewWalletIconSmall");

		// Token: 0x04000A36 RID: 2614
		public static readonly GUISprite EndRoundButtonPulse = new GUISprite("EndRoundButtonPulse");

		// Token: 0x04000A37 RID: 2615
		public static readonly GUISpriteSheet FocusIndicator = new GUISpriteSheet("FocusIndicator");

		// Token: 0x04000A38 RID: 2616
		public static readonly GUISprite IconOverflowIndicator = new GUISprite("IconOverflowIndicator");

		// Token: 0x04000A39 RID: 2617
		public static readonly GUIColor Green = new GUIColor("Green", new Color(154, 213, 163, 255));

		// Token: 0x04000A3A RID: 2618
		public static readonly GUIColor Orange = new GUIColor("Orange", new Color(243, 162, 50, 255));

		// Token: 0x04000A3B RID: 2619
		public static readonly GUIColor Red = new GUIColor("Red", new Color(245, 105, 105, 255));

		// Token: 0x04000A3C RID: 2620
		public static readonly GUIColor Blue = new GUIColor("Blue", new Color(126, 211, 224, 255));

		// Token: 0x04000A3D RID: 2621
		public static readonly GUIColor Yellow = new GUIColor("Yellow", new Color(255, 255, 0, 255));

		// Token: 0x04000A3E RID: 2622
		public static readonly GUIColor ModdedServerColor = new GUIColor("ModdedServerColor", new Color(154, 185, 160, 255));

		// Token: 0x04000A3F RID: 2623
		public static readonly GUIColor ColorInventoryEmpty = new GUIColor("ColorInventoryEmpty", new Color(245, 105, 105, 255));

		// Token: 0x04000A40 RID: 2624
		public static readonly GUIColor ColorInventoryHalf = new GUIColor("ColorInventoryHalf", new Color(243, 162, 50, 255));

		// Token: 0x04000A41 RID: 2625
		public static readonly GUIColor ColorInventoryFull = new GUIColor("ColorInventoryFull", new Color(96, 222, 146, 255));

		// Token: 0x04000A42 RID: 2626
		public static readonly GUIColor ColorInventoryBackground = new GUIColor("ColorInventoryBackground", new Color(56, 56, 56, 255));

		// Token: 0x04000A43 RID: 2627
		public static readonly GUIColor ColorInventoryEmptyOverlay = new GUIColor("ColorInventoryEmptyOverlay", new Color(125, 125, 125, 255));

		// Token: 0x04000A44 RID: 2628
		public static readonly GUIColor TextColorNormal = new GUIColor("TextColorNormal", new Color(228, 217, 167, 255));

		// Token: 0x04000A45 RID: 2629
		public static readonly GUIColor TextColorBright = new GUIColor("TextColorBright", new Color(255, 255, 255, 255));

		// Token: 0x04000A46 RID: 2630
		public static readonly GUIColor TextColorDark = new GUIColor("TextColorDark", new Color(0, 0, 0, 230));

		// Token: 0x04000A47 RID: 2631
		public static readonly GUIColor TextColorDim = new GUIColor("TextColorDim", new Color(153, 153, 153, 153));

		// Token: 0x04000A48 RID: 2632
		public static readonly GUIColor ItemQualityColorPoor = new GUIColor("ItemQualityColorPoor", new Color(128, 128, 128, 255));

		// Token: 0x04000A49 RID: 2633
		public static readonly GUIColor ItemQualityColorNormal = new GUIColor("ItemQualityColorNormal", new Color(255, 255, 255, 255));

		// Token: 0x04000A4A RID: 2634
		public static readonly GUIColor ItemQualityColorGood = new GUIColor("ItemQualityColorGood", new Color(144, 238, 144, 255));

		// Token: 0x04000A4B RID: 2635
		public static readonly GUIColor ItemQualityColorExcellent = new GUIColor("ItemQualityColorExcellent", new Color(173, 216, 230, 255));

		// Token: 0x04000A4C RID: 2636
		public static readonly GUIColor ItemQualityColorMasterwork = new GUIColor("ItemQualityColorMasterwork", new Color(147, 112, 219, 255));

		// Token: 0x04000A4D RID: 2637
		public static readonly GUIColor ColorReputationVeryLow = new GUIColor("ColorReputationVeryLow", new Color(192, 60, 60, 255));

		// Token: 0x04000A4E RID: 2638
		public static readonly GUIColor ColorReputationLow = new GUIColor("ColorReputationLow", new Color(203, 145, 23, 255));

		// Token: 0x04000A4F RID: 2639
		public static readonly GUIColor ColorReputationNeutral = new GUIColor("ColorReputationNeutral", new Color(228, 217, 167, 255));

		// Token: 0x04000A50 RID: 2640
		public static readonly GUIColor ColorReputationHigh = new GUIColor("ColorReputationHigh", new Color(51, 152, 64, 255));

		// Token: 0x04000A51 RID: 2641
		public static readonly GUIColor ColorReputationVeryHigh = new GUIColor("ColorReputationVeryHigh", new Color(71, 160, 164, 255));

		// Token: 0x04000A52 RID: 2642
		public static readonly GUIColor InteractionLabelColor = new GUIColor("InteractionLabelColor", new Color(255, 255, 255, 255));

		// Token: 0x04000A53 RID: 2643
		public static readonly GUIColor InteractionLabelHoverColor = new GUIColor("InteractionLabelHoverColor", new Color(0, 255, 255, 255));

		// Token: 0x04000A54 RID: 2644
		public static readonly GUIColor EquipmentSlotIconColor = new GUIColor("EquipmentSlotIconColor", new Color(99, 70, 64, 255));

		// Token: 0x04000A55 RID: 2645
		public static readonly GUIColor BuffColorLow = new GUIColor("BuffColorLow", new Color(66, 170, 73, 255));

		// Token: 0x04000A56 RID: 2646
		public static readonly GUIColor BuffColorMedium = new GUIColor("BuffColorMedium", new Color(110, 168, 118, 255));

		// Token: 0x04000A57 RID: 2647
		public static readonly GUIColor BuffColorHigh = new GUIColor("BuffColorHigh", new Color(154, 213, 163, 255));

		// Token: 0x04000A58 RID: 2648
		public static readonly GUIColor DebuffColorLow = new GUIColor("DebuffColorLow", new Color(243, 162, 50, 255));

		// Token: 0x04000A59 RID: 2649
		public static readonly GUIColor DebuffColorMedium = new GUIColor("DebuffColorMedium", new Color(155, 55, 55, 255));

		// Token: 0x04000A5A RID: 2650
		public static readonly GUIColor DebuffColorHigh = new GUIColor("DebuffColorHigh", new Color(228, 27, 27, 255));

		// Token: 0x04000A5B RID: 2651
		public static readonly GUIColor HealthBarColorLow = new GUIColor("HealthBarColorLow", new Color(255, 0, 0, 255));

		// Token: 0x04000A5C RID: 2652
		public static readonly GUIColor HealthBarColorMedium = new GUIColor("HealthBarColorMedium", new Color(255, 165, 0, 255));

		// Token: 0x04000A5D RID: 2653
		public static readonly GUIColor HealthBarColorHigh = new GUIColor("HealthBarColorHigh", new Color(78, 114, 88));

		// Token: 0x04000A5E RID: 2654
		public static readonly GUIColor HealthBarColorPoisoned = new GUIColor("HealthBarColorPoisoned", new Color(100, 150, 0, 255));

		// Token: 0x04000A5F RID: 2655
		private static readonly Point defaultItemFrameMargin = new Point(50, 56);
	}
}
