using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Eos;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Barotrauma.Sounds;
using Barotrauma.Steam;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenAL;

namespace Barotrauma
{
	// Token: 0x02000127 RID: 295
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class SettingsMenu
	{
		// Token: 0x17000A54 RID: 2644
		// (get) Token: 0x060027E9 RID: 10217 RVA: 0x001BC267 File Offset: 0x001BA467
		// (set) Token: 0x060027EA RID: 10218 RVA: 0x001BC26E File Offset: 0x001BA46E
		[Nullable(2)]
		public static SettingsMenu Instance { [NullableContext(2)] get; [NullableContext(2)] private set; }

		// Token: 0x17000A55 RID: 2645
		// (get) Token: 0x060027EB RID: 10219 RVA: 0x001BC276 File Offset: 0x001BA476
		// (set) Token: 0x060027EC RID: 10220 RVA: 0x001BC27E File Offset: 0x001BA47E
		public SettingsMenu.Tab CurrentTab { get; private set; }

		// Token: 0x060027ED RID: 10221 RVA: 0x001BC288 File Offset: 0x001BA488
		public static SettingsMenu Create(RectTransform mainParent)
		{
			SettingsMenu instance = SettingsMenu.Instance;
			if (instance != null)
			{
				instance.Close();
			}
			SettingsMenu.Instance = new SettingsMenu(mainParent, default(GameSettings.Config));
			return SettingsMenu.Instance;
		}

		// Token: 0x060027EE RID: 10222 RVA: 0x001BC2C0 File Offset: 0x001BA4C0
		private unsafe SettingsMenu(RectTransform mainParent, GameSettings.Config setConfig = default(GameSettings.Config))
		{
			this.unsavedConfig = *GameSettings.CurrentConfig;
			this.mainFrame = new GUIFrame(new RectTransform(Vector2.One, mainParent, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null);
			GUILayoutGroup mainLayout = new GUILayoutGroup(new RectTransform(Vector2.One * 0.95f, this.mainFrame.RectTransform, Anchor.Center, new Pivot?(Pivot.Center), null, null, ScaleBasis.Normal), false, Anchor.TopRight);
			RectTransform rectT = new RectTransform(new ValueTuple<float, float>(1f, 0.07f), mainLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = TextManager.Get("Settings");
			GUIFont largeFont = GUIStyle.LargeFont;
			new GUITextBlock(rectT, text, null, largeFont, Alignment.Left, false, "", null);
			SettingsMenu.<>c__DisplayClass17_0 CS$<>8__locals1;
			CS$<>8__locals1.tabberAndContentLayout = new GUILayoutGroup(new RectTransform(new ValueTuple<float, float>(1f, 0.86f), mainLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			SettingsMenu.<.ctor>g__tabberPadding|17_0(ref CS$<>8__locals1);
			this.tabber = new GUILayoutGroup(new RectTransform(new ValueTuple<float, float>(0.06f, 1f), CS$<>8__locals1.tabberAndContentLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				AbsoluteSpacing = GUI.IntScale(5f)
			};
			SettingsMenu.<.ctor>g__tabberPadding|17_0(ref CS$<>8__locals1);
			this.tabContents = new Dictionary<SettingsMenu.Tab, ValueTuple<GUIButton, GUIFrame>>();
			this.contentFrame = new GUIFrame(new RectTransform(new ValueTuple<float, float>(0.92f, 1f), CS$<>8__locals1.tabberAndContentLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "InnerFrame", null);
			this.bottom = new GUILayoutGroup(new RectTransform(new ValueTuple<float, float>(this.contentFrame.RectTransform.RelativeSize.X, 0.04f), mainLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.01f
			};
			this.CreateGraphicsTab();
			this.CreateAudioAndVCTab(false);
			this.CreateControlsTab();
			this.CreateGameplayTab();
			this.CreateModsTab(out this.WorkshopMenu);
			this.CreateBottomButtons();
			this.SelectTab(SettingsMenu.Tab.Graphics);
			this.tabber.Recalculate();
		}

		// Token: 0x060027EF RID: 10223 RVA: 0x001BC5AA File Offset: 0x001BA7AA
		private void SwitchContent(GUIFrame newContent)
		{
			this.contentFrame.Children.ForEach(delegate(GUIComponent c)
			{
				c.Visible = false;
			});
			newContent.Visible = true;
		}

		// Token: 0x060027F0 RID: 10224 RVA: 0x001BC5E4 File Offset: 0x001BA7E4
		public void SelectTab(SettingsMenu.Tab tab)
		{
			if (tab == SettingsMenu.Tab.AudioAndVC && SettingsMenu.CurrentDeviceMismatchesDisplayed())
			{
				this.CreateAudioAndVCTab(true);
			}
			this.CurrentTab = tab;
			this.SwitchContent(this.tabContents[tab].Item2);
			this.tabber.Children.ForEach(delegate(GUIComponent c)
			{
				GUIButton btn = c as GUIButton;
				if (btn != null)
				{
					btn.Selected = (btn == this.tabContents[tab].Item1);
				}
			});
		}

		// Token: 0x060027F1 RID: 10225 RVA: 0x001BC660 File Offset: 0x001BA860
		private void AddButtonToTabber(SettingsMenu.Tab tab, GUIFrame content)
		{
			RectTransform rectT = new RectTransform(Vector2.One, this.tabber.RectTransform, Anchor.TopLeft, new Pivot?(Pivot.TopLeft), null, null, ScaleBasis.Smallest);
			LocalizedString text = "";
			Alignment textAlignment = Alignment.Center;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 1);
			defaultInterpolatedStringHandler.AppendLiteral("SettingsMenuTab.");
			defaultInterpolatedStringHandler.AppendFormatted<SettingsMenu.Tab>(tab);
			GUIButton guibutton = new GUIButton(rectT, text, textAlignment, defaultInterpolatedStringHandler.ToStringAndClear(), null);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(12, 1);
			defaultInterpolatedStringHandler2.AppendLiteral("SettingsTab.");
			defaultInterpolatedStringHandler2.AppendFormatted<SettingsMenu.Tab>(tab);
			guibutton.ToolTip = TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear());
			guibutton.OnClicked = delegate(GUIButton b, object _)
			{
				this.SelectTab(tab);
				return false;
			};
			GUIButton button = guibutton;
			button.RectTransform.MaxSize = RectTransform.MaxPoint;
			button.Children.ForEach(delegate(GUIComponent c)
			{
				c.RectTransform.MaxSize = RectTransform.MaxPoint;
			});
			this.tabContents.Add(tab, new ValueTuple<GUIButton, GUIFrame>(button, content));
		}

		// Token: 0x060027F2 RID: 10226 RVA: 0x001BC794 File Offset: 0x001BA994
		private GUIFrame CreateNewContentFrame(SettingsMenu.Tab tab)
		{
			ValueTuple<GUIButton, GUIFrame> tabContent;
			if (this.tabContents.TryGetValue(tab, out tabContent))
			{
				return tabContent.Item2;
			}
			GUIFrame content = new GUIFrame(new RectTransform(Vector2.One * 0.95f, this.contentFrame.RectTransform, Anchor.Center, new Pivot?(Pivot.Center), null, null, ScaleBasis.Normal), null, null);
			this.AddButtonToTabber(tab, content);
			return content;
		}

		// Token: 0x060027F3 RID: 10227 RVA: 0x001BC80C File Offset: 0x001BAA0C
		[return: TupleElementNames(new string[]
		{
			"Left",
			"Right"
		})]
		[return: Nullable(new byte[]
		{
			0,
			1,
			1
		})]
		public static ValueTuple<GUILayoutGroup, GUILayoutGroup> CreateSidebars(GUIFrame parent, bool split = false)
		{
			GUILayoutGroup layout = new GUILayoutGroup(new RectTransform(Vector2.One, parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			GUILayoutGroup left = new GUILayoutGroup(new RectTransform(new ValueTuple<float, float>(0.4875f, 1f), layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			GUIFrame centerFrame = new GUIFrame(new RectTransform(new ValueTuple<float, float>(0.025f, 1f), layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			if (split)
			{
				new GUICustomComponent(new RectTransform(Vector2.One, centerFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), delegate(SpriteBatch sb, GUICustomComponent c)
				{
					sb.DrawLine(new ValueTuple<float, float>((float)c.Rect.Center.X, (float)c.Rect.Top), new ValueTuple<float, float>((float)c.Rect.Center.X, (float)c.Rect.Bottom), GUIStyle.TextColorDim, 2f);
				}, null);
			}
			GUILayoutGroup right = new GUILayoutGroup(new RectTransform(new ValueTuple<float, float>(0.4875f, 1f), layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			return new ValueTuple<GUILayoutGroup, GUILayoutGroup>(left, right);
		}

		// Token: 0x060027F4 RID: 10228 RVA: 0x001BC990 File Offset: 0x001BAB90
		public static GUILayoutGroup CreateCenterLayout(GUIFrame parent)
		{
			return new GUILayoutGroup(new RectTransform(new ValueTuple<float, float>(0.5f, 1f), parent.RectTransform, Anchor.TopCenter, new Pivot?(Pivot.TopCenter), null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				ChildAnchor = Anchor.TopCenter
			};
		}

		// Token: 0x060027F5 RID: 10229 RVA: 0x001BC9E4 File Offset: 0x001BABE4
		public static RectTransform NewItemRectT(GUILayoutGroup parent)
		{
			return new RectTransform(new ValueTuple<float, float>(1f, 0.06f), parent.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal);
		}

		// Token: 0x060027F6 RID: 10230 RVA: 0x001BCA30 File Offset: 0x001BAC30
		public static void Spacer(GUILayoutGroup parent, float height = 0.03f)
		{
			new GUIFrame(new RectTransform(new ValueTuple<float, float>(1f, height), parent.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), null, null);
		}

		// Token: 0x060027F7 RID: 10231 RVA: 0x001BCA88 File Offset: 0x001BAC88
		public static GUITextBlock Label(GUILayoutGroup parent, LocalizedString str, GUIFont font)
		{
			return new GUITextBlock(SettingsMenu.NewItemRectT(parent), str, null, font, Alignment.Left, false, "", null);
		}

		// Token: 0x060027F8 RID: 10232 RVA: 0x001BCAC2 File Offset: 0x001BACC2
		public static void DropdownEnum<[Nullable(0)] T>(GUILayoutGroup parent, Func<T, LocalizedString> textFunc, [Nullable(new byte[]
		{
			2,
			1,
			1
		})] Func<T, LocalizedString> tooltipFunc, T currentValue, Action<T> setter) where T : Enum
		{
			SettingsMenu.Dropdown<T>(parent, textFunc, tooltipFunc, (T[])Enum.GetValues(typeof(T)), currentValue, setter);
		}

		// Token: 0x060027F9 RID: 10233 RVA: 0x001BCAE4 File Offset: 0x001BACE4
		public static GUIDropDown Dropdown<[Nullable(2)] T>(GUILayoutGroup parent, Func<T, LocalizedString> textFunc, [Nullable(new byte[]
		{
			2,
			1,
			1
		})] Func<T, LocalizedString> tooltipFunc, IReadOnlyList<T> values, T currentValue, Action<T> setter)
		{
			GUIDropDown dropdown = new GUIDropDown(SettingsMenu.NewItemRectT(parent), null, values.Count, "", false, false, Alignment.CenterLeft, 1f);
			values.ForEach(delegate(T v)
			{
				GUIDropDown dropdown = dropdown;
				LocalizedString text = textFunc(v);
				object userData = v;
				Func<T, LocalizedString> tooltipFunc2 = tooltipFunc;
				dropdown.AddItem(text, userData, ((tooltipFunc2 != null) ? tooltipFunc2(v) : null) ?? null, null, null);
			});
			int childIndex = values.IndexOf(currentValue);
			dropdown.Select(childIndex);
			dropdown.ListBox.ForceLayoutRecalculation();
			dropdown.ListBox.ScrollToElement(dropdown.ListBox.Content.GetChild(childIndex), GUIListBox.PlaySelectSound.No);
			dropdown.OnSelected = delegate(GUIComponent dd, object obj)
			{
				setter((T)((object)obj));
				return true;
			};
			return dropdown;
		}

		// Token: 0x060027FA RID: 10234 RVA: 0x001BCBB0 File Offset: 0x001BADB0
		[return: TupleElementNames(new string[]
		{
			"slider",
			"label"
		})]
		[return: Nullable(new byte[]
		{
			0,
			1,
			1
		})]
		public static ValueTuple<GUIScrollBar, GUITextBlock> Slider(GUILayoutGroup parent, Vector2 range, int steps, Func<float, string> labelFunc, float currentValue, Action<float> setter, [Nullable(2)] LocalizedString tooltip = null)
		{
			GUILayoutGroup layout = new GUILayoutGroup(SettingsMenu.NewItemRectT(parent), true, Anchor.TopLeft);
			GUIScrollBar slider = new GUIScrollBar(new RectTransform(new ValueTuple<float, float>(0.72f, 1f), layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), 1f, null, "GUISlider", null)
			{
				Range = range,
				BarScrollValue = currentValue,
				Step = 1f / (float)(steps - 1),
				BarSize = 1f / (float)steps
			};
			if (tooltip != null)
			{
				slider.ToolTip = tooltip;
			}
			GUITextBlock label = new GUITextBlock(new RectTransform(new ValueTuple<float, float>(0.28f, 1f), layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), labelFunc(currentValue), null, null, Alignment.Center, false, "", null);
			slider.OnMoved = delegate(GUIScrollBar sb, float val)
			{
				label.Text = labelFunc(sb.BarScrollValue);
				setter(sb.BarScrollValue);
				return true;
			};
			return new ValueTuple<GUIScrollBar, GUITextBlock>(slider, label);
		}

		// Token: 0x060027FB RID: 10235 RVA: 0x001BCD1C File Offset: 0x001BAF1C
		public static GUITickBox Tickbox(GUILayoutGroup parent, LocalizedString label, LocalizedString tooltip, bool currentValue, Action<bool> setter)
		{
			return new GUITickBox(SettingsMenu.NewItemRectT(parent), label, null, "")
			{
				Selected = currentValue,
				ToolTip = tooltip,
				OnSelected = delegate(GUITickBox tb)
				{
					setter(tb.Selected);
					return true;
				}
			};
		}

		// Token: 0x060027FC RID: 10236 RVA: 0x001BCD6E File Offset: 0x001BAF6E
		public string Percentage(float v)
		{
			return ToolBox.GetFormattedPercentage(v);
		}

		// Token: 0x060027FD RID: 10237 RVA: 0x001BCD76 File Offset: 0x001BAF76
		public static int Round(float v)
		{
			return MathUtils.RoundToInt(v);
		}

		// Token: 0x060027FE RID: 10238 RVA: 0x001BCD80 File Offset: 0x001BAF80
		private void CreateGraphicsTab()
		{
			GUIFrame content = this.CreateNewContentFrame(SettingsMenu.Tab.Graphics);
			ValueTuple<GUILayoutGroup, GUILayoutGroup> valueTuple = SettingsMenu.CreateSidebars(content, false);
			GUILayoutGroup left = valueTuple.Item1;
			GUILayoutGroup right = valueTuple.Item2;
			List<ValueTuple<int, int>> supportedResolutions = (from m in GameMain.GraphicsDeviceManager.GraphicsDevice.Adapter.SupportedDisplayModes
			where m.Format == SurfaceFormat.Color
			select new ValueTuple<int, int>(m.Width, m.Height) into m
			where m.Item1 >= GameSettings.Config.GraphicsSettings.MinSupportedResolution.X && m.Item2 >= GameSettings.Config.GraphicsSettings.MinSupportedResolution.Y
			select m).ToList<ValueTuple<int, int>>();
			ValueTuple<int, int> currentResolution = new ValueTuple<int, int>(this.unsavedConfig.Graphics.Width, this.unsavedConfig.Graphics.Height);
			if (!supportedResolutions.Contains(currentResolution))
			{
				supportedResolutions.Add(currentResolution);
			}
			SettingsMenu.Label(left, TextManager.Get("Resolution"), GUIStyle.SubHeadingFont);
			SettingsMenu.Dropdown<ValueTuple<int, int>>(left, delegate([TupleElementNames(new string[]
			{
				"Width",
				"Height"
			})] ValueTuple<int, int> m)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
				defaultInterpolatedStringHandler.AppendFormatted<int>(m.Item1);
				defaultInterpolatedStringHandler.AppendLiteral("x");
				defaultInterpolatedStringHandler.AppendFormatted<int>(m.Item2);
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}, null, supportedResolutions, currentResolution, delegate([TupleElementNames(new string[]
			{
				"Width",
				"Height"
			})] ValueTuple<int, int> res)
			{
				this.unsavedConfig.Graphics.Width = res.Item1;
				this.unsavedConfig.Graphics.Height = res.Item2;
			});
			SettingsMenu.Spacer(left, 0.03f);
			SettingsMenu.Label(left, TextManager.Get("DisplayMode"), GUIStyle.SubHeadingFont);
			SettingsMenu.DropdownEnum<WindowMode>(left, delegate(WindowMode m)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
				defaultInterpolatedStringHandler.AppendFormatted<WindowMode>(m);
				return TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
			}, null, this.unsavedConfig.Graphics.DisplayMode, delegate(WindowMode v)
			{
				this.unsavedConfig.Graphics.DisplayMode = v;
			});
			SettingsMenu.Spacer(left, 0.03f);
			GUITextBlock displayLabel = SettingsMenu.Label(left, TextManager.Get("TargetDisplay"), GUIStyle.SubHeadingFont);
			displayLabel.ToolTip = TextManager.Get("TargetDisplay.Tooltip");
			SettingsMenu.Dropdown<int>(left, (int m) => TextManager.GetWithVariables((m == 0) ? "PrimaryDisplayFormat" : "SecondaryDisplayFormat", new ValueTuple<string, string>[]
			{
				new ValueTuple<string, string>("[num]", m.ToString()),
				new ValueTuple<string, string>("[name]", Display.GetDisplayName(m))
			}), null, Enumerable.Range(0, Display.GetNumberOfDisplays()).ToArray<int>(), this.unsavedConfig.Graphics.Display, delegate(int v)
			{
				this.unsavedConfig.Graphics.Display = v;
			});
			SettingsMenu.Spacer(left, 0.03f);
			SettingsMenu.Tickbox(left, TextManager.Get("EnableVSync"), TextManager.Get("EnableVSyncTooltip"), this.unsavedConfig.Graphics.VSync, delegate(bool v)
			{
				this.unsavedConfig.Graphics.VSync = v;
			});
			SettingsMenu.Tickbox(left, TextManager.Get("EnableTextureCompression"), TextManager.Get("EnableTextureCompressionTooltip"), this.unsavedConfig.Graphics.CompressTextures, delegate(bool v)
			{
				this.unsavedConfig.Graphics.CompressTextures = v;
			});
			SettingsMenu.Spacer(right, 0.03f);
			SettingsMenu.Label(right, TextManager.Get("LOSEffect"), GUIStyle.SubHeadingFont);
			SettingsMenu.DropdownEnum<LosMode>(right, delegate(LosMode m)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(7, 1);
				defaultInterpolatedStringHandler.AppendLiteral("LosMode");
				defaultInterpolatedStringHandler.AppendFormatted<LosMode>(m);
				return TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
			}, null, this.unsavedConfig.Graphics.LosMode, delegate(LosMode v)
			{
				this.unsavedConfig.Graphics.LosMode = v;
			});
			SettingsMenu.Spacer(right, 0.03f);
			SettingsMenu.Label(right, TextManager.Get("LightMapScale"), GUIStyle.SubHeadingFont);
			SettingsMenu.Slider(right, new ValueTuple<float, float>(0.5f, 1f), 11, (float v) => TextManager.GetWithVariable("percentageformat", "[value]", SettingsMenu.Round(v * 100f).ToString(), FormatCapitals.No).Value, this.unsavedConfig.Graphics.LightMapScale, delegate(float v)
			{
				this.unsavedConfig.Graphics.LightMapScale = v;
			}, TextManager.Get("LightMapScaleTooltip"));
			SettingsMenu.Spacer(right, 0.03f);
			SettingsMenu.Label(right, TextManager.Get("VisibleLightLimit"), GUIStyle.SubHeadingFont);
			SettingsMenu.Slider(right, new ValueTuple<float, float>(10f, 510f), 21, delegate(float v)
			{
				if (v <= 500f)
				{
					return SettingsMenu.Round(v).ToString();
				}
				return TextManager.Get("unlimited").Value;
			}, (float)this.unsavedConfig.Graphics.VisibleLightLimit, delegate(float v)
			{
				this.unsavedConfig.Graphics.VisibleLightLimit = ((v > 500f) ? int.MaxValue : SettingsMenu.Round(v));
			}, TextManager.Get("VisibleLightLimitTooltip"));
			SettingsMenu.Spacer(right, 0.03f);
			SettingsMenu.Tickbox(right, TextManager.Get("RadialDistortion"), TextManager.Get("RadialDistortionTooltip"), this.unsavedConfig.Graphics.RadialDistortion, delegate(bool v)
			{
				this.unsavedConfig.Graphics.RadialDistortion = v;
			});
			SettingsMenu.Tickbox(right, TextManager.Get("ChromaticAberration"), TextManager.Get("ChromaticAberrationTooltip"), this.unsavedConfig.Graphics.ChromaticAberration, delegate(bool v)
			{
				this.unsavedConfig.Graphics.ChromaticAberration = v;
			});
			SettingsMenu.Label(right, TextManager.Get("ParticleLimit"), GUIStyle.SubHeadingFont);
			SettingsMenu.Slider(right, new ValueTuple<float, float>(100f, 1500f), 15, (float v) => SettingsMenu.Round(v).ToString(), (float)this.unsavedConfig.Graphics.ParticleLimit, delegate(float v)
			{
				this.unsavedConfig.Graphics.ParticleLimit = SettingsMenu.Round(v);
			}, null);
			SettingsMenu.Spacer(right, 0.03f);
		}

		// Token: 0x060027FF RID: 10239 RVA: 0x001BD27C File Offset: 0x001BB47C
		private static string TrimAudioDeviceName(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return string.Empty;
			}
			string[] prefixes = new string[]
			{
				"OpenAL Soft on "
			};
			foreach (string prefix in prefixes)
			{
				if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
				{
					return name.Remove(0, prefix.Length);
				}
			}
			return name;
		}

		// Token: 0x06002800 RID: 10240 RVA: 0x001BD2D4 File Offset: 0x001BB4D4
		private static int HandleAlErrors(string message)
		{
			int alcError = Alc.GetError(IntPtr.Zero);
			if (alcError != 0)
			{
				DebugConsole.ThrowError(message + ": ALC error " + Alc.GetErrorString(alcError), null, null, false, false);
				return alcError;
			}
			int alError = Al.GetError();
			if (alError != 0)
			{
				DebugConsole.ThrowError(message + ": AL error " + Al.GetErrorString(alError), null, null, false, false);
				return alError;
			}
			return 0;
		}

		// Token: 0x06002801 RID: 10241 RVA: 0x001BD334 File Offset: 0x001BB534
		private static void GetAudioDevices(int listSpecifier, int defaultSpecifier, out IReadOnlyList<string> list, ref string current)
		{
			list = Array.Empty<string>();
			List<string> retVal = Alc.GetStringList(IntPtr.Zero, listSpecifier).ToList<string>();
			if (SettingsMenu.HandleAlErrors("Alc.GetStringList failed") != 0)
			{
				return;
			}
			list = retVal;
			if (string.IsNullOrEmpty(current))
			{
				current = Alc.GetString(IntPtr.Zero, defaultSpecifier);
				if (SettingsMenu.HandleAlErrors("Alc.GetString failed") != 0)
				{
					return;
				}
			}
			string currentVal = current;
			if (list.Any<string>() && !list.Any((string n) => n.Equals(currentVal, StringComparison.OrdinalIgnoreCase)))
			{
				current = list[0];
			}
		}

		// Token: 0x06002802 RID: 10242 RVA: 0x001BD3C4 File Offset: 0x001BB5C4
		private static bool IsCurrentDevice(string savedDeviceName, int deviceType)
		{
			bool result;
			try
			{
				string currentDevice = Alc.GetString(IntPtr.Zero, deviceType);
				if (string.IsNullOrEmpty(savedDeviceName) || string.IsNullOrEmpty(currentDevice))
				{
					result = false;
				}
				else
				{
					result = currentDevice.Equals(savedDeviceName, StringComparison.OrdinalIgnoreCase);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error checking output device name: " + ex.Message);
				result = false;
			}
			return result;
		}

		// Token: 0x06002803 RID: 10243 RVA: 0x001BD428 File Offset: 0x001BB628
		private static bool CurrentDeviceMismatchesDisplayed()
		{
			return !SettingsMenu.IsCurrentDevice(GameSettings.CurrentConfig.Audio.VoiceCaptureDevice, 785) || !SettingsMenu.IsCurrentDevice(GameSettings.CurrentConfig.Audio.AudioOutputDevice, 4100);
		}

		// Token: 0x06002804 RID: 10244 RVA: 0x001BD464 File Offset: 0x001BB664
		public void CreateAudioAndVCTab(bool refresh = false)
		{
			if (GameMain.Client == null && (refresh || VoipCapture.Instance == null))
			{
				string currDevice = this.unsavedConfig.Audio.VoiceCaptureDevice;
				IReadOnlyList<string> deviceList;
				SettingsMenu.GetAudioDevices(784, 785, out deviceList, ref currDevice);
				if (deviceList.Any<string>())
				{
					VoipCapture currentCaptureInstance = VoipCapture.Instance;
					if (currentCaptureInstance != null)
					{
						currentCaptureInstance.Dispose();
					}
					VoipCapture.Create(this.unsavedConfig.Audio.VoiceCaptureDevice, null);
				}
				if (VoipCapture.Instance == null)
				{
					this.unsavedConfig.Audio.VoiceSetting = VoiceMode.Disabled;
				}
			}
			GUIFrame content = this.CreateNewContentFrame(SettingsMenu.Tab.AudioAndVC);
			if (refresh)
			{
				content.ClearChildren();
			}
			ValueTuple<GUILayoutGroup, GUILayoutGroup> valueTuple = SettingsMenu.CreateSidebars(content, true);
			GUILayoutGroup audio = valueTuple.Item1;
			GUILayoutGroup voiceChat = valueTuple.Item2;
			SettingsMenu.Label(audio, TextManager.Get("AudioOutputDevice"), GUIStyle.SubHeadingFont);
			string currentOutputDevice = this.unsavedConfig.Audio.AudioOutputDevice;
			SettingsMenu.<CreateAudioAndVCTab>g__audioDeviceElement|40_0(audio, delegate(string v)
			{
				this.unsavedConfig.Audio.AudioOutputDevice = v;
			}, 4115, 4100, ref currentOutputDevice);
			GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(1f, 1f), audio.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("RefreshAudioDevices"), Alignment.Center, "GUIButtonSmall", null);
			guibutton.ToolTip = TextManager.Get("RefreshAudioDevicesToolTip");
			guibutton.OnClicked = delegate(GUIButton btn, object obj)
			{
				this.CreateAudioAndVCTab(true);
				return true;
			};
			SettingsMenu.Spacer(audio, 0.03f);
			SettingsMenu.Label(audio, TextManager.Get("SoundVolume"), GUIStyle.SubHeadingFont);
			SettingsMenu.Slider(audio, new ValueTuple<float, float>(0f, 1f), 101, new Func<float, string>(this.Percentage), this.unsavedConfig.Audio.SoundVolume, delegate(float v)
			{
				this.unsavedConfig.Audio.SoundVolume = v;
				GameMain.SoundManager.SetCategoryGainMultiplier(SoundManager.SoundCategoryDefault, v, 0);
				GameMain.SoundManager.SetCategoryGainMultiplier(SoundManager.SoundCategoryWaterAmbience, v, 0);
			}, null);
			SettingsMenu.Label(audio, TextManager.Get("MusicVolume"), GUIStyle.SubHeadingFont);
			SettingsMenu.Slider(audio, new ValueTuple<float, float>(0f, 1f), 101, new Func<float, string>(this.Percentage), this.unsavedConfig.Audio.MusicVolume, delegate(float v)
			{
				this.unsavedConfig.Audio.MusicVolume = v;
				GameMain.SoundManager.SetCategoryGainMultiplier(SoundManager.SoundCategoryMusic, v, 0);
			}, null);
			SettingsMenu.Label(audio, TextManager.Get("UiSoundVolume"), GUIStyle.SubHeadingFont);
			SettingsMenu.Slider(audio, new ValueTuple<float, float>(0f, 1f), 101, new Func<float, string>(this.Percentage), this.unsavedConfig.Audio.UiVolume, delegate(float v)
			{
				this.unsavedConfig.Audio.UiVolume = v;
				GameMain.SoundManager.SetCategoryGainMultiplier(SoundManager.SoundCategoryUi, v, 0);
			}, null);
			SettingsMenu.Tickbox(audio, TextManager.Get("MuteOnFocusLost"), TextManager.Get("MuteOnFocusLostTooltip"), this.unsavedConfig.Audio.MuteOnFocusLost, delegate(bool v)
			{
				this.unsavedConfig.Audio.MuteOnFocusLost = v;
			});
			SettingsMenu.Tickbox(audio, TextManager.Get("DynamicRangeCompression"), TextManager.Get("DynamicRangeCompressionTooltip"), this.unsavedConfig.Audio.DynamicRangeCompressionEnabled, delegate(bool v)
			{
				this.unsavedConfig.Audio.DynamicRangeCompressionEnabled = v;
			});
			SettingsMenu.Spacer(audio, 0.03f);
			SettingsMenu.Label(audio, TextManager.Get("VoiceChatVolume"), GUIStyle.SubHeadingFont);
			SettingsMenu.Slider(audio, new ValueTuple<float, float>(0f, 2f), 201, new Func<float, string>(this.Percentage), this.unsavedConfig.Audio.VoiceChatVolume, delegate(float v)
			{
				this.unsavedConfig.Audio.VoiceChatVolume = v;
				GameMain.SoundManager.SetCategoryGainMultiplier(SoundManager.SoundCategoryVoip, v, 0);
			}, null);
			SettingsMenu.Tickbox(audio, TextManager.Get("DirectionalVoiceChat"), TextManager.Get("DirectionalVoiceChatTooltip"), this.unsavedConfig.Audio.UseDirectionalVoiceChat, delegate(bool v)
			{
				this.unsavedConfig.Audio.UseDirectionalVoiceChat = v;
			});
			SettingsMenu.Tickbox(audio, TextManager.Get("VoipAttenuation"), TextManager.Get("VoipAttenuationTooltip"), this.unsavedConfig.Audio.VoipAttenuationEnabled, delegate(bool v)
			{
				this.unsavedConfig.Audio.VoipAttenuationEnabled = v;
			});
			SettingsMenu.Label(voiceChat, TextManager.Get("AudioInputDevice"), GUIStyle.SubHeadingFont);
			string currentInputDevice = this.unsavedConfig.Audio.VoiceCaptureDevice;
			SettingsMenu.<CreateAudioAndVCTab>g__audioDeviceElement|40_0(voiceChat, delegate(string v)
			{
				this.unsavedConfig.Audio.VoiceCaptureDevice = v;
			}, 784, 785, ref currentInputDevice);
			GUIButton guibutton2 = new GUIButton(new RectTransform(new Vector2(1f, 1f), voiceChat.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("RefreshAudioDevices"), Alignment.Center, "GUIButtonSmall", null);
			guibutton2.ToolTip = TextManager.Get("RefreshAudioDevicesToolTip");
			guibutton2.OnClicked = delegate(GUIButton btn, object obj)
			{
				this.CreateAudioAndVCTab(true);
				return true;
			};
			SettingsMenu.Spacer(voiceChat, 0.03f);
			SettingsMenu.Label(voiceChat, TextManager.Get("VCInputMode"), GUIStyle.SubHeadingFont);
			SettingsMenu.DropdownEnum<VoiceMode>(voiceChat, delegate(VoiceMode v)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
				defaultInterpolatedStringHandler.AppendLiteral("VoiceMode.");
				defaultInterpolatedStringHandler.AppendFormatted<VoiceMode>(v);
				return TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
			}, delegate(VoiceMode v)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
				defaultInterpolatedStringHandler.AppendLiteral("VoiceMode.");
				defaultInterpolatedStringHandler.AppendFormatted<VoiceMode>(v);
				defaultInterpolatedStringHandler.AppendLiteral("Tooltip");
				return TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
			}, this.unsavedConfig.Audio.VoiceSetting, delegate(VoiceMode v)
			{
				this.unsavedConfig.Audio.VoiceSetting = v;
			});
			SettingsMenu.Spacer(voiceChat, 0.03f);
			GUITextBlock noiseGateThresholdLabel = SettingsMenu.Label(voiceChat, TextManager.Get("NoiseGateThreshold"), GUIStyle.SubHeadingFont);
			GUIProgressBar dbMeter = new GUIProgressBar(SettingsMenu.NewItemRectT(voiceChat), 0f, new Color?(Color.Lime), "", true);
			dbMeter.ProgressGetter = delegate()
			{
				if (VoipCapture.Instance == null)
				{
					return 0f;
				}
				GUIProgressBar dbMeter = dbMeter;
				VoiceMode voiceSetting = this.unsavedConfig.Audio.VoiceSetting;
				Color color;
				switch (voiceSetting)
				{
				case VoiceMode.Disabled:
					color = Color.LightGray;
					break;
				case VoiceMode.PushToTalk:
					color = GUIStyle.Green;
					break;
				case VoiceMode.Activity:
					color = ((VoipCapture.Instance.LastdB > (double)this.unsavedConfig.Audio.NoiseGateThreshold) ? GUIStyle.Green : GUIStyle.Orange);
					break;
				default:
					<PrivateImplementationDetails>.ThrowSwitchExpressionException(voiceSetting);
					break;
				}
				dbMeter.Color = color;
				float scrollVal = double.IsNegativeInfinity(VoipCapture.Instance.LastdB) ? 0f : (((float)VoipCapture.Instance.LastdB + 100f) / 100f);
				return scrollVal * scrollVal;
			};
			GUIScrollBar noiseGateSlider = new GUIScrollBar(new RectTransform(Vector2.One, dbMeter.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), 0.03f, new Color?(Color.White), "GUISlider", null);
			noiseGateSlider.Frame.Visible = false;
			noiseGateSlider.Step = 0.01f;
			noiseGateSlider.Range = new Vector2(-100f, 0f);
			noiseGateSlider.BarScroll = MathUtils.InverseLerp(-100f, 0f, this.unsavedConfig.Audio.NoiseGateThreshold);
			noiseGateSlider.BarScroll *= noiseGateSlider.BarScroll;
			noiseGateSlider.OnMoved = delegate(GUIScrollBar scrollBar, float barScroll)
			{
				this.unsavedConfig.Audio.NoiseGateThreshold = MathHelper.Lerp(-100f, 0f, (float)Math.Sqrt((double)scrollBar.BarScroll));
				return true;
			};
			new GUICustomComponent(new RectTransform(Vector2.Zero, voiceChat.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, delegate(float deltaTime, GUICustomComponent component)
			{
				noiseGateThresholdLabel.Visible = (this.unsavedConfig.Audio.VoiceSetting == VoiceMode.Activity);
				noiseGateSlider.Visible = (this.unsavedConfig.Audio.VoiceSetting == VoiceMode.Activity);
			});
			SettingsMenu.Spacer(voiceChat, 0.03f);
			SettingsMenu.Label(voiceChat, TextManager.Get("MicrophoneVolume"), GUIStyle.SubHeadingFont);
			SettingsMenu.Slider(voiceChat, new ValueTuple<float, float>(0f, 10f), 101, new Func<float, string>(this.Percentage), this.unsavedConfig.Audio.MicrophoneVolume, delegate(float v)
			{
				this.unsavedConfig.Audio.MicrophoneVolume = v;
			}, null);
			SettingsMenu.Spacer(voiceChat, 0.03f);
			SettingsMenu.Label(voiceChat, TextManager.Get("CutoffPrevention"), GUIStyle.SubHeadingFont);
			SettingsMenu.Slider(voiceChat, new ValueTuple<float, float>(0f, 500f), 26, delegate(float v)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 1);
				defaultInterpolatedStringHandler.AppendFormatted<int>(SettingsMenu.Round(v));
				defaultInterpolatedStringHandler.AppendLiteral(" ms");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}, (float)this.unsavedConfig.Audio.VoiceChatCutoffPrevention, delegate(float v)
			{
				this.unsavedConfig.Audio.VoiceChatCutoffPrevention = SettingsMenu.Round(v);
			}, TextManager.Get("CutoffPreventionTooltip"));
		}

		// Token: 0x06002805 RID: 10245 RVA: 0x001BDC28 File Offset: 0x001BBE28
		private void CreateControlsTab()
		{
			SettingsMenu.<>c__DisplayClass43_0 CS$<>8__locals1 = new SettingsMenu.<>c__DisplayClass43_0();
			CS$<>8__locals1.<>4__this = this;
			GUIFrame content = this.CreateNewContentFrame(SettingsMenu.Tab.Controls);
			GUILayoutGroup layout = SettingsMenu.CreateCenterLayout(content);
			SettingsMenu.Label(layout, TextManager.Get("AimAssist"), GUIStyle.SubHeadingFont);
			CS$<>8__locals1.aimAssistSlider = SettingsMenu.Slider(layout, new ValueTuple<float, float>(0f, 1f), 101, new Func<float, string>(this.Percentage), this.unsavedConfig.AimAssistAmount, delegate(float v)
			{
				CS$<>8__locals1.<>4__this.unsavedConfig.AimAssistAmount = v;
			}, TextManager.Get("AimAssistTooltip"));
			SettingsMenu.Tickbox(layout, TextManager.Get("EnableMouseLook"), TextManager.Get("EnableMouseLookTooltip"), this.unsavedConfig.EnableMouseLook, delegate(bool v)
			{
				CS$<>8__locals1.<>4__this.unsavedConfig.EnableMouseLook = v;
			});
			SettingsMenu.Spacer(layout, 0.03f);
			SettingsMenu.<>c__DisplayClass43_0 CS$<>8__locals2 = CS$<>8__locals1;
			GUIListBox guilistBox = new GUIListBox(new RectTransform(new ValueTuple<float, float>(2f, 0.7f), layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
			guilistBox.CanBeFocused = false;
			guilistBox.OnSelected = ((GUIComponent _, object __) => false);
			CS$<>8__locals2.keyMapList = guilistBox;
			SettingsMenu.Spacer(layout, 0.03f);
			this.inputButtonValueNameGetters.Clear();
			CS$<>8__locals1.currentSetter = null;
			GUICustomComponent inputListener = new GUICustomComponent(new RectTransform(Vector2.Zero, layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, delegate(float deltaTime, GUICustomComponent component)
			{
				if (CS$<>8__locals1.currentSetter == null)
				{
					return;
				}
				if (PlayerInput.PrimaryMouseButtonClicked() && CS$<>8__locals1.<>4__this.inputBoxSelectedThisFrame)
				{
					CS$<>8__locals1.<>4__this.inputBoxSelectedThisFrame = false;
					return;
				}
				Keys[] pressedKeys = PlayerInput.GetKeyboardState.GetPressedKeys();
				if (pressedKeys != null && pressedKeys.Any<Keys>())
				{
					if (pressedKeys.Contains(Keys.Escape))
					{
						base.<CreateControlsTab>g__clearSetter|9();
						return;
					}
					base.<CreateControlsTab>g__callSetter|10(pressedKeys.First<Keys>());
					return;
				}
				else
				{
					if (PlayerInput.PrimaryMouseButtonClicked() && (GUI.MouseOn == null || !(GUI.MouseOn is GUIButton) || GUI.MouseOn.IsChildOf(CS$<>8__locals1.keyMapList.Content, true)))
					{
						base.<CreateControlsTab>g__callSetter|10(MouseButton.PrimaryMouse);
						return;
					}
					if (PlayerInput.SecondaryMouseButtonClicked())
					{
						base.<CreateControlsTab>g__callSetter|10(MouseButton.SecondaryMouse);
						return;
					}
					if (PlayerInput.MidButtonClicked())
					{
						base.<CreateControlsTab>g__callSetter|10(MouseButton.MiddleMouse);
						return;
					}
					if (PlayerInput.Mouse4ButtonClicked())
					{
						base.<CreateControlsTab>g__callSetter|10(MouseButton.MouseButton4);
						return;
					}
					if (PlayerInput.Mouse5ButtonClicked())
					{
						base.<CreateControlsTab>g__callSetter|10(MouseButton.MouseButton5);
						return;
					}
					if (PlayerInput.MouseWheelUpClicked())
					{
						base.<CreateControlsTab>g__callSetter|10(MouseButton.MouseWheelUp);
						return;
					}
					if (PlayerInput.MouseWheelDownClicked())
					{
						base.<CreateControlsTab>g__callSetter|10(MouseButton.MouseWheelDown);
					}
					return;
				}
			});
			InputType[] inputTypes = (InputType[])Enum.GetValues(typeof(InputType));
			InputType[][] inputTypeColumns = new InputType[][]
			{
				inputTypes.Take(inputTypes.Length - inputTypes.Length / 2).ToArray<InputType>(),
				inputTypes.TakeLast(inputTypes.Length / 2).ToArray<InputType>()
			};
			for (int i = 0; i < inputTypes.Length; i += 2)
			{
				GUILayoutGroup currRow = CS$<>8__locals1.<CreateControlsTab>g__createInputRowLayout|2();
				for (int j = 0; j < 2; j++)
				{
					InputType[] column = inputTypeColumns[j];
					if (i / 2 >= column.Length)
					{
						break;
					}
					InputType input = column[i / 2];
					SettingsMenu.<>c__DisplayClass43_0 CS$<>8__locals4 = CS$<>8__locals1;
					GUILayoutGroup currRow3 = currRow;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
					defaultInterpolatedStringHandler.AppendLiteral("InputType.");
					defaultInterpolatedStringHandler.AppendFormatted<InputType>(input);
					CS$<>8__locals4.<CreateControlsTab>g__addInputToRow|3(currRow3, TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear()), () => CS$<>8__locals1.<>4__this.unsavedConfig.KeyMap.Bindings[input].Name, delegate(KeyOrMouse v)
					{
						CS$<>8__locals1.<>4__this.unsavedConfig.KeyMap = CS$<>8__locals1.<>4__this.unsavedConfig.KeyMap.WithBinding(input, v);
					}, SettingsMenu.LegacyInputTypes.Contains(input));
				}
			}
			for (int k = 0; k < this.unsavedConfig.InventoryKeyMap.Bindings.Length; k += 2)
			{
				GUILayoutGroup currRow2 = CS$<>8__locals1.<CreateControlsTab>g__createInputRowLayout|2();
				for (int l = 0; l < 2; l++)
				{
					int currIndex = k + l;
					if (currIndex >= this.unsavedConfig.InventoryKeyMap.Bindings.Length)
					{
						break;
					}
					KeyOrMouse input2 = this.unsavedConfig.InventoryKeyMap.Bindings[currIndex];
					CS$<>8__locals1.<CreateControlsTab>g__addInputToRow|3(currRow2, TextManager.GetWithVariable("inventoryslotkeybind", "[slotnumber]", (currIndex + 1).ToString(CultureInfo.InvariantCulture), FormatCapitals.No), () => CS$<>8__locals1.<>4__this.unsavedConfig.InventoryKeyMap.Bindings[currIndex].Name, delegate(KeyOrMouse v)
					{
						CS$<>8__locals1.<>4__this.unsavedConfig.InventoryKeyMap = CS$<>8__locals1.<>4__this.unsavedConfig.InventoryKeyMap.WithBinding(currIndex, v);
					}, false);
				}
			}
			GUILayoutGroup resetControlsHolder = new GUILayoutGroup(new RectTransform(new ValueTuple<float, float>(1.75f, 0.1f), layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.Center)
			{
				RelativeSpacing = 0.1f
			};
			GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(0.45f, 1f), resetControlsHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Reset"), Alignment.Center, "GUIButtonSmall", null);
			guibutton.ToolTip = TextManager.Get("SetDefaultBindingsTooltip");
			guibutton.OnClicked = delegate(GUIButton _, object userdata)
			{
				CS$<>8__locals1.<>4__this.unsavedConfig.InventoryKeyMap = GameSettings.Config.InventoryKeyMapping.GetDefault();
				CS$<>8__locals1.<>4__this.unsavedConfig.KeyMap = GameSettings.Config.KeyMapping.GetDefault();
				CS$<>8__locals1.aimAssistSlider.Item1.BarScrollValue = 0.05f;
				CS$<>8__locals1.aimAssistSlider.Item2.Text = CS$<>8__locals1.<>4__this.Percentage(0.05f);
				foreach (GUIButton btn in CS$<>8__locals1.<>4__this.inputButtonValueNameGetters.Keys)
				{
					btn.Text = CS$<>8__locals1.<>4__this.inputButtonValueNameGetters[btn]();
				}
				SettingsMenu instance = SettingsMenu.Instance;
				if (instance != null)
				{
					instance.SelectTab(SettingsMenu.Tab.Controls);
				}
				return true;
			};
		}

		// Token: 0x06002806 RID: 10246 RVA: 0x001BE0C8 File Offset: 0x001BC2C8
		private void CreateGameplayTab()
		{
			SettingsMenu.<>c__DisplayClass44_0 CS$<>8__locals1 = new SettingsMenu.<>c__DisplayClass44_0();
			CS$<>8__locals1.<>4__this = this;
			GUIFrame content = this.CreateNewContentFrame(SettingsMenu.Tab.Gameplay);
			ValueTuple<GUILayoutGroup, GUILayoutGroup> valueTuple = SettingsMenu.CreateSidebars(content, true);
			GUILayoutGroup leftColumn = valueTuple.Item1;
			GUILayoutGroup rightColumn = valueTuple.Item2;
			LanguageIdentifier[] languages = (from l in TextManager.AvailableLanguages
			orderby TextManager.GetTranslatedLanguageName(l).ToIdentifier()
			select l).ToArray<LanguageIdentifier>();
			SettingsMenu.Label(leftColumn, TextManager.Get("Language"), GUIStyle.SubHeadingFont);
			SettingsMenu.Dropdown<LanguageIdentifier>(leftColumn, (LanguageIdentifier v) => TextManager.GetTranslatedLanguageName(v), null, languages, this.unsavedConfig.Language, delegate(LanguageIdentifier v)
			{
				CS$<>8__locals1.<>4__this.unsavedConfig.Language = v;
			});
			SettingsMenu.Spacer(leftColumn, 0.03f);
			SettingsMenu.Tickbox(leftColumn, TextManager.Get("PauseOnFocusLost"), TextManager.Get("PauseOnFocusLostTooltip"), this.unsavedConfig.PauseOnFocusLost, delegate(bool v)
			{
				CS$<>8__locals1.<>4__this.unsavedConfig.PauseOnFocusLost = v;
			});
			SettingsMenu.Spacer(leftColumn, 0.03f);
			SettingsMenu.Tickbox(leftColumn, TextManager.Get("DisableInGameHints"), TextManager.Get("DisableInGameHintsTooltip"), this.unsavedConfig.DisableInGameHints, delegate(bool v)
			{
				CS$<>8__locals1.<>4__this.unsavedConfig.DisableInGameHints = v;
			});
			new GUIButton(new RectTransform(new Vector2(1f, 1f), leftColumn.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ResetInGameHints"), Alignment.Center, "GUIButtonSmall", null).OnClicked = delegate(GUIButton button, object o)
			{
				GUIMessageBox msgBox = new GUIMessageBox(TextManager.Get("ResetInGameHints"), TextManager.Get("ResetInGameHintsTooltip"), new LocalizedString[]
				{
					TextManager.Get("Yes"),
					TextManager.Get("No")
				}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
				msgBox.Buttons[0].OnClicked = delegate(GUIButton guiButton, object o1)
				{
					IgnoredHints.Instance.Clear();
					msgBox.Close();
					return false;
				};
				msgBox.Buttons[1].OnClicked = new GUIButton.OnClickedHandler(msgBox.Close);
				return false;
			};
			SettingsMenu.Spacer(leftColumn, 0.03f);
			SettingsMenu.Tickbox(leftColumn, TextManager.Get("ChatSpeechBubbles"), TextManager.Get("ChatSpeechBubbles.Tooltip"), this.unsavedConfig.ChatSpeechBubbles, delegate(bool v)
			{
				CS$<>8__locals1.<>4__this.unsavedConfig.ChatSpeechBubbles = v;
			});
			SettingsMenu.Label(leftColumn, TextManager.Get("ShowEnemyHealthBars"), GUIStyle.SubHeadingFont);
			SettingsMenu.DropdownEnum<EnemyHealthBarMode>(leftColumn, delegate(EnemyHealthBarMode v)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(20, 1);
				defaultInterpolatedStringHandler.AppendLiteral("ShowEnemyHealthBars.");
				defaultInterpolatedStringHandler.AppendFormatted<EnemyHealthBarMode>(v);
				return TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
			}, null, this.unsavedConfig.ShowEnemyHealthBars, delegate(EnemyHealthBarMode v)
			{
				CS$<>8__locals1.<>4__this.unsavedConfig.ShowEnemyHealthBars = v;
			});
			SettingsMenu.Spacer(leftColumn, 0.03f);
			SettingsMenu.Label(leftColumn, TextManager.Get("InteractionLabels"), GUIStyle.SubHeadingFont);
			SettingsMenu.DropdownEnum<InteractionLabelDisplayMode>(leftColumn, delegate(InteractionLabelDisplayMode v)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(18, 1);
				defaultInterpolatedStringHandler.AppendLiteral("InteractionLabels.");
				defaultInterpolatedStringHandler.AppendFormatted<InteractionLabelDisplayMode>(v);
				return TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
			}, null, this.unsavedConfig.InteractionLabelDisplayMode, delegate(InteractionLabelDisplayMode v)
			{
				CS$<>8__locals1.<>4__this.unsavedConfig.InteractionLabelDisplayMode = v;
			});
			SettingsMenu.Label(rightColumn, TextManager.Get("HUDScale"), GUIStyle.SubHeadingFont);
			SettingsMenu.Slider(rightColumn, new ValueTuple<float, float>(0.75f, 1.25f), 51, new Func<float, string>(this.Percentage), this.unsavedConfig.Graphics.HUDScale, delegate(float v)
			{
				CS$<>8__locals1.<>4__this.unsavedConfig.Graphics.HUDScale = v;
			}, null);
			SettingsMenu.Label(rightColumn, TextManager.Get("InventoryScale"), GUIStyle.SubHeadingFont);
			SettingsMenu.Slider(rightColumn, new ValueTuple<float, float>(0.75f, 1.25f), 51, new Func<float, string>(this.Percentage), this.unsavedConfig.Graphics.InventoryScale, delegate(float v)
			{
				CS$<>8__locals1.<>4__this.unsavedConfig.Graphics.InventoryScale = v;
			}, null);
			SettingsMenu.Label(rightColumn, TextManager.Get("TextScale"), GUIStyle.SubHeadingFont);
			SettingsMenu.Slider(rightColumn, new ValueTuple<float, float>(0.75f, 1.25f), 51, new Func<float, string>(this.Percentage), this.unsavedConfig.Graphics.TextScale, delegate(float v)
			{
				CS$<>8__locals1.<>4__this.unsavedConfig.Graphics.TextScale = v;
			}, null);
			SettingsMenu.Spacer(rightColumn, 0.03f);
			new GUIButton(new RectTransform(new Vector2(1f, 1f), rightColumn.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("clearserverlistfilters"), Alignment.Center, "GUIButtonSmall", null).OnClicked = delegate(GUIButton _, object _)
			{
				LocalizedString header = TextManager.Get("clearserverlistfilters");
				LocalizedString body = TextManager.Get("clearserverlistfiltersconfirmation");
				Action onConfirm;
				if ((onConfirm = SettingsMenu.<>O.<0>__ClearLocalSpamFilter) == null)
				{
					onConfirm = (SettingsMenu.<>O.<0>__ClearLocalSpamFilter = new Action(SpamServerFilters.ClearLocalSpamFilter));
				}
				GUI.AskForConfirmation(header, body, onConfirm, null, null, null);
				return true;
			};
			SettingsMenu.Spacer(rightColumn, 0.03f);
			SettingsMenu.Spacer(rightColumn, 0.03f);
			SettingsMenu.<>c__DisplayClass44_0 CS$<>8__locals2 = CS$<>8__locals1;
			GUITickBox guitickBox = new GUITickBox(SettingsMenu.NewItemRectT(rightColumn), TextManager.Get("statisticsconsenttickbox"), null, "");
			guitickBox.OnSelected = delegate(GUITickBox tickBox)
			{
				GUIMessageBox loadingBox = null;
				if (!tickBox.Selected)
				{
					loadingBox = GUIMessageBox.CreateLoadingBox(TextManager.Get("PleaseWait"), null, null);
				}
				GameAnalyticsManager.SetConsent(tickBox.Selected ? GameAnalyticsManager.Consent.Ask : GameAnalyticsManager.Consent.No, delegate
				{
					GUIMessageBox loadingBox = loadingBox;
					if (loadingBox == null)
					{
						return;
					}
					loadingBox.Close();
				});
				return false;
			};
			CS$<>8__locals2.statisticsTickBox = guitickBox;
			CS$<>8__locals1.<CreateGameplayTab>g__updateGATickBoxToolTip|13();
			CS$<>8__locals1.cachedConsent = GameAnalyticsManager.Consent.Unknown;
			GUICustomComponent statisticsTickBoxUpdater = new GUICustomComponent(new RectTransform(Vector2.Zero, CS$<>8__locals1.statisticsTickBox.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, delegate(float deltaTime, GUICustomComponent component)
			{
				bool shouldTickBoxBeSelected = GameAnalyticsManager.UserConsented == GameAnalyticsManager.Consent.Yes;
				if (CS$<>8__locals1.cachedConsent == GameAnalyticsManager.UserConsented && CS$<>8__locals1.statisticsTickBox.Selected == shouldTickBoxBeSelected)
				{
					return;
				}
				base.<CreateGameplayTab>g__updateGATickBoxToolTip|13();
				CS$<>8__locals1.cachedConsent = GameAnalyticsManager.UserConsented;
				GUITickBox.OnSelectedHandler prevHandler = CS$<>8__locals1.statisticsTickBox.OnSelected;
				CS$<>8__locals1.statisticsTickBox.OnSelected = null;
				CS$<>8__locals1.statisticsTickBox.Selected = shouldTickBoxBeSelected;
				CS$<>8__locals1.statisticsTickBox.OnSelected = prevHandler;
				CS$<>8__locals1.statisticsTickBox.Enabled &= (GameAnalyticsManager.UserConsented != GameAnalyticsManager.Consent.Error);
			});
			if (SteamManager.IsInitialized)
			{
				bool shouldCrossplayBeEnabled = this.unsavedConfig.CrossplayChoice == EosSteamPrimaryLogin.CrossplayChoice.Enabled;
				GUITickBox crossplayTickBox = SettingsMenu.Tickbox(rightColumn, TextManager.Get("EosAllowCrossplay"), TextManager.Get("EosAllowCrossplayTooltip"), shouldCrossplayBeEnabled, delegate(bool v)
				{
					CS$<>8__locals1.<>4__this.unsavedConfig.CrossplayChoice = (v ? EosSteamPrimaryLogin.CrossplayChoice.Enabled : EosSteamPrimaryLogin.CrossplayChoice.Disabled);
				});
				if (GameMain.NetworkMember != null)
				{
					crossplayTickBox.Enabled = false;
					crossplayTickBox.ToolTip = TextManager.Get("CantAccessEOSSettingsInMP");
				}
			}
		}

		// Token: 0x06002807 RID: 10247 RVA: 0x001BE628 File Offset: 0x001BC828
		private void CreateModsTab(out WorkshopMenu workshopMenu)
		{
			GUIFrame content = this.CreateNewContentFrame(SettingsMenu.Tab.Mods);
			content.RectTransform.RelativeSize = Vector2.One;
			workshopMenu = ((Screen.Selected is MainMenuScreen) ? new MutableWorkshopMenu(content) : new ImmutableWorkshopMenu(content));
			GameMain.MainMenuScreen.ResetModUpdateButton();
		}

		// Token: 0x06002808 RID: 10248 RVA: 0x001BE674 File Offset: 0x001BC874
		private void CreateBottomButtons()
		{
			new GUIButton(new RectTransform(new Vector2(1f, 1f), this.bottom.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Cancel"), Alignment.Center, "", null).OnClicked = delegate(GUIButton btn, object obj)
			{
				SoundManager soundManager = GameMain.SoundManager;
				if (soundManager != null)
				{
					soundManager.ApplySettings();
				}
				this.Close();
				return false;
			};
			GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(1f, 1f), this.bottom.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("applysettingsbutton"), Alignment.Center, "", null);
			guibutton.OnClicked = delegate(GUIButton btn, object obj)
			{
				this.ApplyInstalledModChanges();
				this.mainFrame.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
				return false;
			};
			guibutton.OnAddedToGUIUpdateList = delegate(GUIComponent component)
			{
				bool enabled;
				if (this.CurrentTab == SettingsMenu.Tab.Mods)
				{
					MutableWorkshopMenu mutableWorkshopMenu = this.WorkshopMenu as MutableWorkshopMenu;
					enabled = (mutableWorkshopMenu != null && mutableWorkshopMenu.CurrentTab == MutableWorkshopMenu.Tab.InstalledMods && !mutableWorkshopMenu.ViewingItemDetails);
				}
				else
				{
					enabled = true;
				}
				component.Enabled = enabled;
			};
		}

		// Token: 0x06002809 RID: 10249 RVA: 0x001BE76C File Offset: 0x001BC96C
		public void ApplyInstalledModChanges()
		{
			EosSteamPrimaryLogin.HandleCrossplayChoiceChange(this.unsavedConfig.CrossplayChoice);
			GameSettings.SetCurrentConfig(this.unsavedConfig);
			MutableWorkshopMenu mutableWorkshopMenu = this.WorkshopMenu as MutableWorkshopMenu;
			if (mutableWorkshopMenu != null && mutableWorkshopMenu.CurrentTab == MutableWorkshopMenu.Tab.InstalledMods)
			{
				mutableWorkshopMenu.Apply();
			}
			GameSettings.SaveCurrentConfig();
		}

		// Token: 0x0600280A RID: 10250 RVA: 0x001BE7B8 File Offset: 0x001BC9B8
		public void Close()
		{
			if (GameMain.Client == null || GameSettings.CurrentConfig.Audio.VoiceSetting == VoiceMode.Disabled)
			{
				VoipCapture instance = VoipCapture.Instance;
				if (instance != null)
				{
					instance.Dispose();
				}
			}
			this.mainFrame.Parent.RemoveChild(this.mainFrame);
			if (SettingsMenu.Instance == this)
			{
				SettingsMenu.Instance = null;
			}
			GUI.SettingsMenuOpen = false;
		}

		// Token: 0x0600280C RID: 10252 RVA: 0x001BE848 File Offset: 0x001BCA48
		[CompilerGenerated]
		internal static void <.ctor>g__tabberPadding|17_0(ref SettingsMenu.<>c__DisplayClass17_0 A_0)
		{
			new GUIFrame(new RectTransform(new ValueTuple<float, float>(0.01f, 1f), A_0.tabberAndContentLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
		}

		// Token: 0x06002818 RID: 10264 RVA: 0x001BE9AC File Offset: 0x001BCBAC
		[CompilerGenerated]
		internal static void <CreateAudioAndVCTab>g__audioDeviceElement|40_0(GUILayoutGroup parent, Action<string> setter, int listSpecifier, int defaultSpecifier, ref string currentDevice)
		{
			IReadOnlyList<string> devices;
			SettingsMenu.GetAudioDevices(listSpecifier, defaultSpecifier, out devices, ref currentDevice);
			SettingsMenu.Dropdown<string>(parent, (string v) => SettingsMenu.TrimAudioDeviceName(v), null, devices, currentDevice, setter);
		}

		// Token: 0x04001452 RID: 5202
		private GameSettings.Config unsavedConfig;

		// Token: 0x04001453 RID: 5203
		public readonly GUIFrame mainFrame;

		// Token: 0x04001454 RID: 5204
		public readonly GUILayoutGroup tabber;

		// Token: 0x04001455 RID: 5205
		public readonly GUIFrame contentFrame;

		// Token: 0x04001456 RID: 5206
		private readonly GUILayoutGroup bottom;

		// Token: 0x04001457 RID: 5207
		public readonly WorkshopMenu WorkshopMenu;

		// Token: 0x04001458 RID: 5208
		private static readonly ImmutableHashSet<InputType> LegacyInputTypes = new List<InputType>
		{
			InputType.Chat,
			InputType.RadioChat,
			InputType.LocalVoice,
			InputType.RadioVoice
		}.ToImmutableHashSet<InputType>();

		// Token: 0x04001459 RID: 5209
		[TupleElementNames(new string[]
		{
			"Button",
			"Content"
		})]
		[Nullable(new byte[]
		{
			1,
			0,
			1,
			1
		})]
		public readonly Dictionary<SettingsMenu.Tab, ValueTuple<GUIButton, GUIFrame>> tabContents;

		// Token: 0x0400145A RID: 5210
		private readonly Dictionary<GUIButton, Func<LocalizedString>> inputButtonValueNameGetters = new Dictionary<GUIButton, Func<LocalizedString>>();

		// Token: 0x0400145B RID: 5211
		private bool inputBoxSelectedThisFrame;

		// Token: 0x02000D36 RID: 3382
		[NullableContext(0)]
		public enum Tab
		{
			// Token: 0x04004EA8 RID: 20136
			Graphics,
			// Token: 0x04004EA9 RID: 20137
			AudioAndVC,
			// Token: 0x04004EAA RID: 20138
			Controls,
			// Token: 0x04004EAB RID: 20139
			Gameplay,
			// Token: 0x04004EAC RID: 20140
			Mods
		}

		// Token: 0x02000D37 RID: 3383
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04004EAD RID: 20141
			[Nullable(0)]
			public static Action <0>__ClearLocalSpamFilter;
		}
	}
}
