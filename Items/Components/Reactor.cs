using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005C6 RID: 1478
	internal class Reactor : Powered, IServerSerializable, INetSerializable, IClientSerializable
	{
		// Token: 0x1700175B RID: 5979
		// (get) Token: 0x06005CD7 RID: 23767 RVA: 0x002FE86D File Offset: 0x002FCA6D
		// (set) Token: 0x06005CD8 RID: 23768 RVA: 0x002FE875 File Offset: 0x002FCA75
		public GUIButton AutoTempSwitch { get; private set; }

		// Token: 0x1700175C RID: 5980
		// (get) Token: 0x06005CD9 RID: 23769 RVA: 0x002FE87E File Offset: 0x002FCA7E
		// (set) Token: 0x06005CDA RID: 23770 RVA: 0x002FE886 File Offset: 0x002FCA86
		public GUIButton PowerButton { get; private set; }

		// Token: 0x1700175D RID: 5981
		// (get) Token: 0x06005CDB RID: 23771 RVA: 0x002FE88F File Offset: 0x002FCA8F
		// (set) Token: 0x06005CDC RID: 23772 RVA: 0x002FE897 File Offset: 0x002FCA97
		public GUIScrollBar FissionRateScrollBar { get; private set; }

		// Token: 0x1700175E RID: 5982
		// (get) Token: 0x06005CDD RID: 23773 RVA: 0x002FE8A0 File Offset: 0x002FCAA0
		// (set) Token: 0x06005CDE RID: 23774 RVA: 0x002FE8A8 File Offset: 0x002FCAA8
		public GUIScrollBar TurbineOutputScrollBar { get; private set; }

		// Token: 0x1700175F RID: 5983
		// (get) Token: 0x06005CDF RID: 23775 RVA: 0x002FE8B1 File Offset: 0x002FCAB1
		public override bool RecreateGUIOnResolutionChange
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17001760 RID: 5984
		// (get) Token: 0x06005CE0 RID: 23776 RVA: 0x002FE8B4 File Offset: 0x002FCAB4
		// (set) Token: 0x06005CE1 RID: 23777 RVA: 0x002FE8BC File Offset: 0x002FCABC
		public bool TriggerInfographic { get; set; }

		// Token: 0x17001761 RID: 5985
		// (get) Token: 0x06005CE2 RID: 23778 RVA: 0x002FE8C5 File Offset: 0x002FCAC5
		public bool IsInfographicVisible
		{
			get
			{
				return this.infographic != null && this.infographic.Visible;
			}
		}

		// Token: 0x06005CE3 RID: 23779 RVA: 0x002FE8DC File Offset: 0x002FCADC
		protected override void CreateGUI()
		{
			this.warningButtons.Clear();
			this.paddedFrame = new GUILayoutGroup(new RectTransform(base.GuiFrame.Rect.Size - GUIStyle.ItemFrameMargin, base.GuiFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false)
			{
				AbsoluteOffset = GUIStyle.ItemFrameOffset
			}, true, Anchor.TopLeft)
			{
				CanBeFocused = true,
				HoverCursor = CursorState.Default,
				AlwaysOverrideCursor = true,
				RelativeSpacing = 0.012f,
				Stretch = true
			};
			GUILayoutGroup columnLeft = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 1f), this.paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.012f,
				Stretch = true
			};
			GUILayoutGroup columnRight = new GUILayoutGroup(new RectTransform(new Vector2(0.4f, 1f), this.paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				CanBeFocused = true,
				RelativeSpacing = 0.012f,
				Stretch = true
			};
			this.inventoryWindow = new GUIFrame(new RectTransform(new Vector2(0.1f, 0.75f), base.GuiFrame.RectTransform, Anchor.TopLeft, new Pivot?(Pivot.TopRight), null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(85, 220),
				RelativeOffset = new Vector2(-0.02f, 0f)
			}, "ItemUI", null);
			GUILayoutGroup inventoryContent = new GUILayoutGroup(new RectTransform(this.inventoryWindow.Rect.Size - GUIStyle.ItemFrameMargin, this.inventoryWindow.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false)
			{
				AbsoluteOffset = GUIStyle.ItemFrameOffset
			}, false, Anchor.TopCenter)
			{
				Stretch = true
			};
			this.inventoryContainer = new GUIFrame(new RectTransform(new Vector2(1f, 0.9f), inventoryContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUILayoutGroup topLeftArea = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.2f), columnLeft.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true
			};
			Point maxIndicatorSize = new Point(int.MaxValue, (int)(40f * GUI.Scale));
			this.criticalHeatWarning = new GUITickBox(new RectTransform(new Vector2(0.3f, 1f), topLeftArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MaxSize = maxIndicatorSize
			}, TextManager.Get("ReactorWarningCriticalTemp"), GUIStyle.SubHeadingFont, "IndicatorLightRed")
			{
				Selected = false,
				Enabled = false,
				ToolTip = TextManager.Get("ReactorHeatTip")
			};
			this.criticalOutputWarning = new GUITickBox(new RectTransform(new Vector2(0.3f, 1f), topLeftArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MaxSize = maxIndicatorSize
			}, TextManager.Get("ReactorWarningCriticalOutput"), GUIStyle.SubHeadingFont, "IndicatorLightRed")
			{
				Selected = false,
				Enabled = false,
				ToolTip = TextManager.Get("ReactorOutputTip")
			};
			this.lowTemperatureWarning = new GUITickBox(new RectTransform(new Vector2(0.4f, 1f), topLeftArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MaxSize = maxIndicatorSize
			}, TextManager.Get("ReactorWarningCriticalLowTemp"), GUIStyle.SubHeadingFont, "IndicatorLightRed")
			{
				Selected = false,
				Enabled = false,
				ToolTip = TextManager.Get("ReactorTempTip")
			};
			List<GUITickBox> indicatorLights = new List<GUITickBox>
			{
				this.criticalHeatWarning,
				this.lowTemperatureWarning,
				this.criticalOutputWarning
			};
			indicatorLights.ForEach(delegate(GUITickBox l)
			{
				l.TextBlock.OverrideTextColor(GUIStyle.TextColorNormal);
			});
			topLeftArea.Recalculate();
			new GUIFrame(new RectTransform(new Vector2(1f, 0.01f), columnLeft.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "HorizontalLine", null);
			float relativeYMargin = 0.02f;
			Vector2 relativeTextSize = new Vector2(0.9f, 0.15f);
			Vector2 sliderSize = new Vector2(1f, 0.125f);
			Vector2 meterSize = new Vector2(1f, 1f - relativeTextSize.Y - relativeYMargin - sliderSize.Y - 0.1f);
			GUIFrame meterArea = new GUIFrame(new RectTransform(new Vector2(1f, 0.6f - relativeYMargin * 2f), columnLeft.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUIFrame leftArea = new GUIFrame(new RectTransform(new Vector2(0.49f, 1f), meterArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUIFrame rightArea = new GUIFrame(new RectTransform(new Vector2(0.49f, 1f), meterArea.RectTransform, Anchor.TopCenter, new Pivot?(Pivot.TopLeft), null, null, ScaleBasis.Normal), null, null);
			GUITextBlock fissionRateTextBox = new GUITextBlock(new RectTransform(relativeTextSize, leftArea.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), TextManager.Get("ReactorFissionRate"), new Color?(GUIStyle.TextColorNormal), GUIStyle.SubHeadingFont, Alignment.Center, false, "", null)
			{
				AutoScaleHorizontal = true
			};
			GUICustomComponent fissionMeter = new GUICustomComponent(new RectTransform(meterSize, leftArea.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0f, relativeTextSize.Y + relativeYMargin)
			}, new Action<SpriteBatch, GUICustomComponent>(this.DrawFissionRateMeter), null)
			{
				ToolTip = TextManager.Get("ReactorTipFissionRate")
			};
			GUITextBlock turbineOutputTextBox = new GUITextBlock(new RectTransform(relativeTextSize, rightArea.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), TextManager.Get("ReactorTurbineOutput"), new Color?(GUIStyle.TextColorNormal), GUIStyle.SubHeadingFont, Alignment.Center, false, "", null)
			{
				AutoScaleHorizontal = true
			};
			GUITextBlock.AutoScaleAndNormalize(new GUITextBlock[]
			{
				turbineOutputTextBox,
				fissionRateTextBox
			});
			GUICustomComponent turbineMeter = new GUICustomComponent(new RectTransform(meterSize, rightArea.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0f, relativeTextSize.Y + relativeYMargin)
			}, new Action<SpriteBatch, GUICustomComponent>(this.DrawTurbineOutputMeter), null)
			{
				ToolTip = TextManager.Get("ReactorTipTurbineOutput")
			};
			RectTransform rectTransform = new RectTransform(sliderSize, leftArea.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal);
			rectTransform.RelativeOffset = new Vector2(0f, fissionMeter.RectTransform.RelativeOffset.Y + meterSize.Y + relativeYMargin);
			float barSize = 0.15f;
			Color? color = null;
			string style = "DeviceSlider";
			bool? isHorizontal = null;
			this.FissionRateScrollBar = new GUIScrollBar(rectTransform, barSize, color, style, isHorizontal)
			{
				Enabled = false,
				Step = 0.003921569f,
				OnMoved = delegate(GUIScrollBar bar, float scrollAmount)
				{
					this.LastUser = Character.Controlled;
					this.unsentChanges = true;
					this.TargetFissionRate = scrollAmount * 100f;
					return false;
				}
			};
			this.FissionRateScrollBar.Frame.UserData = UIHighlightAction.ElementId.FissionRateSlider;
			RectTransform rectTransform2 = new RectTransform(sliderSize, rightArea.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal);
			rectTransform2.RelativeOffset = new Vector2(0f, turbineMeter.RectTransform.RelativeOffset.Y + meterSize.Y + relativeYMargin);
			float barSize2 = 0.15f;
			isHorizontal = new bool?(true);
			this.TurbineOutputScrollBar = new GUIScrollBar(rectTransform2, barSize2, null, "DeviceSlider", isHorizontal)
			{
				Enabled = false,
				Step = 0.003921569f,
				OnMoved = delegate(GUIScrollBar bar, float scrollAmount)
				{
					this.LastUser = Character.Controlled;
					this.unsentChanges = true;
					this.TargetTurbineOutput = scrollAmount * 100f;
					return false;
				}
			};
			this.TurbineOutputScrollBar.Frame.UserData = UIHighlightAction.ElementId.TurbineOutputSlider;
			this.buttonArea = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.2f), columnLeft.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.02f
			};
			GUILayoutGroup upperButtons = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.5f), this.buttonArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.01f
			};
			GUILayoutGroup lowerButtons = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.5f), this.buttonArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.01f
			};
			int buttonCount = Reactor.warningTexts.Length;
			for (int i = 0; i < buttonCount; i++)
			{
				string text = Reactor.warningTexts[i];
				GUIButton b2 = new GUIButton(new RectTransform(Vector2.One, (i < 4) ? upperButtons.RectTransform : lowerButtons.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get(text), Alignment.Center, "IndicatorButton", null)
				{
					Font = GUIStyle.SubHeadingFont,
					CanBeFocused = false
				};
				this.warningButtons.Add(text, b2);
			}
			upperButtons.Recalculate();
			lowerButtons.Recalculate();
			this.warningButtons.Values.ForEach(delegate(GUIButton b)
			{
				b.TextBlock.Wrap = (b.Text.Contains(' ', StringComparison.Ordinal) && b.TextBlock.TextSize.X > (float)b.TextBlock.Rect.Width * 1.5f);
			});
			GUITextBlock.AutoScaleAndNormalize(from b in this.warningButtons.Values
			select b.TextBlock, true, false, null);
			GUILayoutGroup topRightArea = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.25f), columnRight.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.02f
			};
			topRightArea.RectTransform.MinSize = new Point(0, topLeftArea.Rect.Height);
			topRightArea.RectTransform.MaxSize = new Point(int.MaxValue, topLeftArea.Rect.Height);
			new GUIFrame(new RectTransform(new Vector2(0.01f, 1f), topRightArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "VerticalLine", null);
			this.AutoTempSwitch = new GUIButton(new RectTransform(new Vector2(0.15f, 0.9f), topRightArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.Center, "SwitchVertical", null)
			{
				UserData = UIHighlightAction.ElementId.AutoTempSwitch,
				Enabled = false,
				Selected = this.AutoTemp,
				ClickSound = GUISoundType.UISwitch,
				OnClicked = delegate(GUIButton button, object data)
				{
					this.AutoTemp = !this.AutoTemp;
					this.LastUser = Character.Controlled;
					this.unsentChanges = true;
					return true;
				}
			};
			this.AutoTempSwitch.RectTransform.MaxSize = new Point((int)((float)this.AutoTempSwitch.Rect.Height * 0.4f), int.MaxValue);
			this.autoTempLight = new GUITickBox(new RectTransform(new Vector2(0.4f, 1f), topRightArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ReactorAutoTemp"), GUIStyle.SubHeadingFont, "IndicatorLightYellow")
			{
				ToolTip = TextManager.Get("ReactorTipAutoTemp"),
				CanBeFocused = false,
				Selected = this.AutoTemp
			};
			this.autoTempLight.RectTransform.MaxSize = new Point(int.MaxValue, this.criticalHeatWarning.Rect.Height);
			this.autoTempLight.TextBlock.OverrideTextColor(GUIStyle.TextColorNormal);
			new GUIFrame(new RectTransform(new Vector2(0.01f, 1f), topRightArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "VerticalLine", null);
			GUIFrame powerArea = new GUIFrame(new RectTransform(new Vector2(0.4f, 1f), topRightArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUIFrame paddedPowerArea = new GUIFrame(new RectTransform(new Vector2(0.9f, 0.9f), powerArea.RectTransform, Anchor.Center, null, null, null, ScaleBasis.BothHeight), "PowerButtonFrame", null);
			this.powerLight = new GUITickBox(new RectTransform(new Vector2(0.87f, 0.3f), paddedPowerArea.RectTransform, Anchor.TopCenter, new Pivot?(Pivot.Center), null, null, ScaleBasis.Normal), TextManager.Get("PowerLabel"), GUIStyle.SubHeadingFont, "IndicatorLightPower")
			{
				CanBeFocused = false,
				Selected = this._powerOn
			};
			this.powerLight.TextBlock.Padding = new Vector4(5f, 0f, 0f, 0f);
			this.powerLight.TextBlock.AutoScaleHorizontal = true;
			this.powerLight.TextBlock.OverrideTextColor(GUIStyle.TextColorNormal);
			this.PowerButton = new GUIButton(new RectTransform(new Vector2(0.8f, 0.75f), paddedPowerArea.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0f, 0.1f)
			}, Alignment.Center, "PowerButton", null)
			{
				UserData = UIHighlightAction.ElementId.PowerButton,
				OnClicked = delegate(GUIButton button, object data)
				{
					this.PowerOn = !this.PowerOn;
					this.LastUser = Character.Controlled;
					this.unsentChanges = true;
					return true;
				}
			};
			topRightArea.Recalculate();
			this.autoTempLight.TextBlock.Padding = new Vector4(this.autoTempLight.TextBlock.Padding.X, 0f, 0f, 0f);
			this.autoTempLight.TextBlock.Text = this.autoTempLight.TextBlock.Text.Replace(" ", "\n", StringComparison.Ordinal);
			this.autoTempLight.TextBlock.AutoScaleHorizontal = true;
			GUITextBlock.AutoScaleAndNormalize(from l in indicatorLights
			select l.TextBlock, true, false, null);
			new GUIFrame(new RectTransform(new Vector2(0.95f, 0.01f), columnRight.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "HorizontalLine", null);
			GUILayoutGroup bottomRightArea = new GUILayoutGroup(new RectTransform(Vector2.One, columnRight.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true,
				CanBeFocused = true,
				RelativeSpacing = 0.02f
			};
			new GUIFrame(new RectTransform(new Vector2(0.01f, 1f), bottomRightArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "VerticalLine", null);
			GUILayoutGroup temperatureArea = new GUILayoutGroup(new RectTransform(new Vector2(0.1f, 1f), bottomRightArea.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.01f
			};
			this.temperatureBoostUpButton = new GUIButton(new RectTransform(Vector2.One, temperatureArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothWidth), Alignment.Center, "GUIPlusButton", null)
			{
				ToolTip = TextManager.Get("reactor.temperatureboostup"),
				OnClicked = delegate(GUIButton _, object __)
				{
					this.unsentChanges = true;
					this.sendUpdateTimer = 0f;
					this.ApplyTemperatureBoost(25f);
					return true;
				}
			};
			new GUICustomComponent(new RectTransform(Vector2.One, temperatureArea.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), new Action<SpriteBatch, GUICustomComponent>(this.DrawTempMeter), null);
			this.temperatureBoostDownButton = new GUIButton(new RectTransform(Vector2.One, temperatureArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothWidth), Alignment.Center, "GUIMinusButton", null)
			{
				ToolTip = TextManager.Get("reactor.temperatureboostdown"),
				OnClicked = delegate(GUIButton _, object __)
				{
					this.unsentChanges = true;
					this.sendUpdateTimer = 0f;
					this.ApplyTemperatureBoost(-25f);
					return true;
				}
			};
			GUILayoutGroup graphArea = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 1f), bottomRightArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.02f
			};
			relativeTextSize = new Vector2(1f, 0.15f);
			GUITextBlock loadText = new GUITextBlock(new RectTransform(relativeTextSize, graphArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "Load", new Color?(this.loadColor), GUIStyle.SubHeadingFont, Alignment.CenterLeft, false, "", null)
			{
				ToolTip = TextManager.Get("ReactorTipLoad")
			};
			LocalizedString loadStr = TextManager.Get("ReactorLoad");
			LocalizedString kW = TextManager.Get("kilowatt");
			GUITextBlock guitextBlock = loadText;
			guitextBlock.TextGetter = (GUITextBlock.TextGetterHandler)Delegate.Combine(guitextBlock.TextGetter, new GUITextBlock.TextGetterHandler(delegate()
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(loadStr.Replace("[kw]", ((int)this.Load).ToString(), StringComparison.Ordinal));
				defaultInterpolatedStringHandler.AppendLiteral(" ");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(kW);
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}));
			GUIFrame graphFrame = new GUIFrame(new RectTransform(new Vector2(1f, 0.9f), graphArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "InnerFrameRed", null);
			this.graph = new GUICustomComponent(new RectTransform(new Vector2(0.9f, 0.98f), graphFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), new Action<SpriteBatch, GUICustomComponent>(this.DrawGraph), null);
			GUITextBlock outputText = new GUITextBlock(new RectTransform(relativeTextSize, graphArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "Output", new Color?(this.outputColor), GUIStyle.SubHeadingFont, Alignment.CenterLeft, false, "", null)
			{
				ToolTip = TextManager.Get("ReactorTipPower")
			};
			LocalizedString outputStr = TextManager.Get("ReactorOutput");
			GUITextBlock guitextBlock2 = outputText;
			guitextBlock2.TextGetter = (GUITextBlock.TextGetterHandler)Delegate.Combine(guitextBlock2.TextGetter, new GUITextBlock.TextGetterHandler(delegate()
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(outputStr.Replace("[kw]", ((int)(-(int)this.currPowerConsumption)).ToString(), StringComparison.Ordinal));
				defaultInterpolatedStringHandler.AppendLiteral(" ");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(kW);
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}));
			this.InitInventoryUI();
			int buttonHeight = (int)((float)GUIStyle.ItemFrameMargin.Y * 0.4f);
			RectTransform helpButtonRt = new RectTransform(new Point(buttonHeight), base.GuiFrame.RectTransform, Anchor.TopRight, null, ScaleBasis.Normal, false)
			{
				AbsoluteOffset = new Point(buttonHeight / 4),
				MinSize = new Point(buttonHeight)
			};
			new GUIButton(helpButtonRt, "", Alignment.Center, "HelpIcon", null).OnClicked = delegate(GUIButton _, object _)
			{
				this.CreateInfrographic();
				return true;
			};
		}

		// Token: 0x06005CE4 RID: 23780 RVA: 0x002FFF80 File Offset: 0x002FE180
		private void ApplyTemperatureBoost(float amount)
		{
			if (Math.Abs(this.temperatureBoost) <= 22.5f && Math.Abs(amount) > 22.5f)
			{
				RoundSound sound = (amount > 0f) ? this.temperatureBoostSoundUp : this.temperatureBoostSoundDown;
				if (sound != null)
				{
					RoundSound sound2 = sound;
					Vector2 worldPosition = this.item.WorldPosition;
					Hull currentHull = this.item.CurrentHull;
					SoundPlayer.PlaySound(sound2, worldPosition, null, currentHull);
				}
			}
			this.temperatureBoost = amount;
		}

		// Token: 0x06005CE5 RID: 23781 RVA: 0x002FFFF8 File Offset: 0x002FE1F8
		private void InitInventoryUI()
		{
			ItemContainer itemContainer = this.item.GetComponent<ItemContainer>();
			if (itemContainer != null)
			{
				itemContainer.UILabel = "";
				itemContainer.AllowUIOverlap = true;
				itemContainer.Inventory.RectTransform = this.inventoryContainer.RectTransform;
			}
		}

		// Token: 0x06005CE6 RID: 23782 RVA: 0x0030003C File Offset: 0x002FE23C
		public override void OnItemLoaded()
		{
			base.OnItemLoaded();
			this.TurbineOutputScrollBar.BarScroll = this.TargetTurbineOutput / 100f;
			this.FissionRateScrollBar.BarScroll = this.TargetFissionRate / 100f;
			this.InitInventoryUI();
		}

		// Token: 0x06005CE7 RID: 23783 RVA: 0x00300078 File Offset: 0x002FE278
		private void DrawTempMeter(SpriteBatch spriteBatch, GUICustomComponent container)
		{
			Vector2 meterPos = new Vector2((float)container.Rect.X, (float)container.Rect.Y);
			Vector2 meterScale = new Vector2((float)container.Rect.Width / (float)this.tempMeterFrame.SourceRect.Width, (float)container.Rect.Height / (float)this.tempMeterFrame.SourceRect.Height);
			this.tempMeterFrame.Draw(spriteBatch, meterPos, Color.White, this.tempMeterFrame.Origin, 0f, meterScale, SpriteEffects.None, null);
			float tempFill = this.temperature / 100f;
			float meterBarScale = (float)container.Rect.Width / (float)this.tempMeterBar.SourceRect.Width;
			Vector2 meterBarPos = new Vector2(container.Center.X, (float)container.Rect.Bottom - this.tempMeterBar.size.Y * meterBarScale - (float)((int)(5f * GUI.yScale)));
			while (meterBarPos.Y > (float)(container.Rect.Bottom + (int)(5f * GUI.yScale)) - (float)container.Rect.Height * tempFill)
			{
				float tempRatio = 1f - (meterBarPos.Y - (float)container.Rect.Y) / (float)container.Rect.Height;
				Color color = ToolBox.GradientLerp(tempRatio, this.temperatureColors);
				this.tempMeterBar.Draw(spriteBatch, meterBarPos, color, 0f, meterBarScale, SpriteEffects.None, null);
				int spacing = 2;
				meterBarPos.Y -= this.tempMeterBar.size.Y * meterBarScale + (float)spacing;
			}
			if (this.temperature > this.optimalTemperature.Y)
			{
				GUI.DrawRectangle(spriteBatch, meterPos, new Vector2((float)container.Rect.Width, (float)container.Rect.Bottom - (float)container.Rect.Height * this.optimalTemperature.Y / 100f - (float)container.Rect.Y), this.warningColor * (float)Math.Sin(Timing.TotalTime * 5.0) * 0.7f, true, 0f, 1f);
			}
			if (this.temperature < this.optimalTemperature.X)
			{
				GUI.DrawRectangle(spriteBatch, new Vector2(meterPos.X, (float)container.Rect.Bottom - (float)container.Rect.Height * this.optimalTemperature.X / 100f), new Vector2((float)container.Rect.Width, (float)container.Rect.Bottom - ((float)container.Rect.Bottom - (float)container.Rect.Height * this.optimalTemperature.X / 100f)), this.warningColor * (float)Math.Sin(Timing.TotalTime * 5.0) * 0.7f, true, 0f, 1f);
			}
			float tempRangeIndicatorScale = (float)container.Rect.Width / (float)this.tempRangeIndicator.SourceRect.Width;
			this.tempRangeIndicator.Draw(spriteBatch, new Vector2(container.Center.X, (float)container.Rect.Bottom - (float)container.Rect.Height * this.optimalTemperature.X / 100f), Color.White, this.tempRangeIndicator.Origin, 0f, tempRangeIndicatorScale, SpriteEffects.None, null);
			this.tempRangeIndicator.Draw(spriteBatch, new Vector2(container.Center.X, (float)container.Rect.Bottom - (float)container.Rect.Height * this.optimalTemperature.Y / 100f), Color.White, this.tempRangeIndicator.Origin, 0f, tempRangeIndicatorScale, SpriteEffects.None, null);
		}

		// Token: 0x06005CE8 RID: 23784 RVA: 0x003004B0 File Offset: 0x002FE6B0
		private void DrawGraph(SpriteBatch spriteBatch, GUICustomComponent container)
		{
			if (this.item.Removed)
			{
				return;
			}
			float maxLoad = this.loadGraph.Max();
			float xOffset = this.graphTimer / (float)this.updateGraphInterval;
			Rectangle graphRect = new Rectangle(container.Rect.X, container.Rect.Y, container.Rect.Width, container.Rect.Height - (int)(5f * GUI.yScale));
			this.DrawGraph(this.outputGraph, spriteBatch, graphRect, Math.Max(10000f, maxLoad), xOffset, this.outputColor);
			this.DrawGraph(this.loadGraph, spriteBatch, graphRect, Math.Max(10000f, maxLoad), xOffset, this.loadColor);
		}

		// Token: 0x06005CE9 RID: 23785 RVA: 0x00300568 File Offset: 0x002FE768
		private void UpdateGraph(float deltaTime)
		{
			this.graphTimer += deltaTime * 1000f;
			if (this.graphTimer > (float)this.updateGraphInterval)
			{
				Reactor.UpdateGraph<float>(this.outputGraph, -this.currPowerConsumption);
				Reactor.UpdateGraph<float>(this.loadGraph, this.Load);
				this.graphTimer = 0f;
			}
			if (this.autoTemp)
			{
				this.FissionRateScrollBar.BarScroll = this.FissionRate / 100f;
				this.TurbineOutputScrollBar.BarScroll = this.TurbineOutput / 100f;
			}
		}

		// Token: 0x06005CEA RID: 23786 RVA: 0x003005FC File Offset: 0x002FE7FC
		private void DrawFissionRateMeter(SpriteBatch spriteBatch, GUICustomComponent container)
		{
			if (this.item.Removed)
			{
				return;
			}
			Rectangle prevScissorRect = spriteBatch.GraphicsDevice.ScissorRectangle;
			spriteBatch.End();
			spriteBatch.GraphicsDevice.ScissorRectangle = container.Rect;
			spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, GameMain.ScissorTestEnable, null, null);
			float jitter = 0f;
			if (this.FissionRate > this.allowedFissionRate.Y - 5f)
			{
				float jitterAmount = Math.Min(this.TargetFissionRate - this.allowedFissionRate.Y, 10f);
				float t = this.graphTimer / (float)this.updateGraphInterval;
				jitter = (PerlinNoise.GetPerlin(t * 0.5f, t * 0.1f) - 0.5f) * jitterAmount;
			}
			this.DrawMeter(spriteBatch, container.Rect, this.fissionRateMeter, this.FissionRate + jitter, new Vector2(0f, 100f), this.optimalFissionRate, this.allowedFissionRate);
			spriteBatch.End();
			spriteBatch.GraphicsDevice.ScissorRectangle = prevScissorRect;
			spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);
		}

		// Token: 0x06005CEB RID: 23787 RVA: 0x0030071C File Offset: 0x002FE91C
		private void DrawTurbineOutputMeter(SpriteBatch spriteBatch, GUICustomComponent container)
		{
			if (this.item.Removed)
			{
				return;
			}
			Vector2 clampedOptimalTurbineOutput = this.optimalTurbineOutput;
			Vector2 clampedAllowedTurbineOutput = this.allowedTurbineOutput;
			if (clampedOptimalTurbineOutput.X > 100f)
			{
				clampedOptimalTurbineOutput = new Vector2(92f, 110f);
				clampedAllowedTurbineOutput = new Vector2(85f, 110f);
			}
			this.DrawMeter(spriteBatch, container.Rect, this.turbineOutputMeter, this.TurbineOutput, new Vector2(0f, 100f), clampedOptimalTurbineOutput, clampedAllowedTurbineOutput);
		}

		// Token: 0x06005CEC RID: 23788 RVA: 0x003007A0 File Offset: 0x002FE9A0
		public override void UpdateHUDComponentSpecific(Character character, float deltaTime, Camera cam)
		{
			this.IsActive = true;
			bool lightOn = Timing.TotalTime % 0.5 < 0.25 && this.PowerOn;
			this.criticalHeatWarning.Selected = (this.temperature > this.allowedTemperature.Y && lightOn);
			this.lowTemperatureWarning.Selected = (this.temperature < this.allowedTemperature.X && lightOn);
			this.criticalOutputWarning.Selected = (-this.currPowerConsumption > this.Load * 1.5f && lightOn);
			this.warningButtons["ReactorWarningOverheating"].Selected = (this.temperature > this.optimalTemperature.Y && lightOn);
			this.warningButtons["ReactorWarningHighOutput"].Selected = (-this.currPowerConsumption > this.Load * 1.1f && lightOn);
			this.warningButtons["ReactorWarningLowTemp"].Selected = (this.temperature < this.optimalTemperature.X && lightOn);
			this.warningButtons["ReactorWarningLowOutput"].Selected = (-this.currPowerConsumption < this.Load * 0.9f && lightOn);
			this.warningButtons["ReactorWarningFuelOut"].Selected = (this.prevAvailableFuel < this.fissionRate * 0.01f && lightOn);
			this.warningButtons["ReactorWarningLowFuel"].Selected = (this.prevAvailableFuel < this.fissionRate && lightOn);
			this.warningButtons["ReactorWarningMeltdown"].Selected = (this.meltDownTimer > this.MeltdownDelay * 0.5f || (this.item.Condition == 0f && lightOn));
			this.warningButtons["ReactorWarningSCRAM"].Selected = (this.temperature > 0.1f && !this.PowerOn);
			if (this.paddedFrame.Rect.Contains(PlayerInput.MousePosition) && !PlayerInput.KeyDown(InputType.Deselect) && !PlayerInput.KeyHit(InputType.Deselect))
			{
				Character.DisableControls = true;
			}
			if (!this.PowerOn)
			{
				this.FissionRateScrollBar.BarScroll = this.FissionRate / 100f;
				this.TurbineOutputScrollBar.BarScroll = this.TurbineOutput / 100f;
			}
			else if (!this.autoTemp && Character.DisableControls && GUI.KeyboardDispatcher.Subscriber == null)
			{
				Vector2 input = Vector2.Zero;
				float rate = 50f;
				if (PlayerInput.KeyDown(InputType.Left))
				{
					input.X += -1f;
				}
				if (PlayerInput.KeyDown(InputType.Right))
				{
					input.X += 1f;
				}
				if (PlayerInput.KeyDown(InputType.Up))
				{
					input.Y += 1f;
				}
				if (PlayerInput.KeyDown(InputType.Down))
				{
					input.Y += -1f;
				}
				if (PlayerInput.KeyDown(InputType.Run))
				{
					rate = 200f;
				}
				else if (PlayerInput.KeyDown(InputType.Crouch))
				{
					rate = 20f;
				}
				rate *= deltaTime;
				input.X *= rate;
				input.Y *= rate;
				if (input.LengthSquared() > 0f)
				{
					this.LastUser = Character.Controlled;
					this.unsentChanges = true;
					if (input.X != 0f && GUIScrollBar.DraggingBar != this.FissionRateScrollBar)
					{
						this.TargetFissionRate = MathHelper.Clamp(this.TargetFissionRate + input.X, 0f, 100f);
						this.FissionRateScrollBar.BarScroll += input.X / 100f;
					}
					if (input.Y != 0f && GUIScrollBar.DraggingBar != this.TurbineOutputScrollBar)
					{
						this.TargetTurbineOutput = MathHelper.Clamp(this.TargetTurbineOutput + input.Y, 0f, 100f);
						this.TurbineOutputScrollBar.BarScroll += input.Y / 100f;
					}
				}
			}
			if (base.GuiFrame != null && base.GuiFrame.Visible && this.TriggerInfographic)
			{
				this.CreateInfrographic();
				this.TriggerInfographic = false;
			}
		}

		// Token: 0x06005CED RID: 23789 RVA: 0x00300BE0 File Offset: 0x002FEDE0
		private void DrawMeter(SpriteBatch spriteBatch, Rectangle rect, Sprite meterSprite, float value, Vector2 range, Vector2 optimalRange, Vector2 allowedRange)
		{
			float scale = Math.Min((float)rect.Width / meterSprite.size.X, (float)rect.Height / meterSprite.size.Y);
			Vector2 pos = new Vector2((float)rect.Center.X, (float)rect.Y + meterSprite.Origin.Y * scale);
			Vector2 optimalRangeNormalized = new Vector2(MathHelper.Clamp((optimalRange.X - range.X) / (range.Y - range.X), 0f, 0.95f), MathHelper.Clamp((optimalRange.Y - range.X) / (range.Y - range.X), 0f, 1f));
			Vector2 allowedRangeNormalized = new Vector2(MathHelper.Clamp((allowedRange.X - range.X) / (range.Y - range.X), 0f, 0.95f), MathHelper.Clamp((allowedRange.Y - range.X) / (range.Y - range.X), 0f, 1f));
			Vector2 sectorRad = new Vector2(-1.35f, 1.35f);
			Vector2 optimalSectorRad = new Vector2(MathHelper.Lerp(sectorRad.X, sectorRad.Y, optimalRangeNormalized.X), MathHelper.Lerp(sectorRad.X, sectorRad.Y, optimalRangeNormalized.Y));
			Vector2 allowedSectorRad = new Vector2(MathHelper.Lerp(sectorRad.X, sectorRad.Y, allowedRangeNormalized.X), MathHelper.Lerp(sectorRad.X, sectorRad.Y, allowedRangeNormalized.Y));
			Vector2 pointerPos = pos - new Vector2(0f, 30f) * scale;
			float scaleMultiplier = 0.95f;
			if (optimalRangeNormalized.X == optimalRangeNormalized.Y)
			{
				this.sectorSprite.Draw(spriteBatch, pointerPos, GUIStyle.Red, 1.5707964f, scale * scaleMultiplier, SpriteEffects.None, null);
			}
			else
			{
				spriteBatch.End();
				Rectangle prevScissorRect = spriteBatch.GraphicsDevice.ScissorRectangle;
				spriteBatch.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, GameMain.GraphicsWidth, (int)(pointerPos.Y + (meterSprite.size.Y - meterSprite.Origin.Y) * scale) - 3);
				spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, GameMain.ScissorTestEnable, null, null);
				this.sectorSprite.Draw(spriteBatch, pointerPos, this.optimalRangeColor, 1.5707964f + (allowedSectorRad.X + allowedSectorRad.Y) / 2f, scale * scaleMultiplier, SpriteEffects.None, null);
				this.sectorSprite.Draw(spriteBatch, pointerPos, this.offRangeColor, optimalSectorRad.X, scale * scaleMultiplier, SpriteEffects.None, null);
				this.sectorSprite.Draw(spriteBatch, pointerPos, this.warningColor, allowedSectorRad.X, scale * scaleMultiplier, SpriteEffects.None, null);
				this.sectorSprite.Draw(spriteBatch, pointerPos, this.offRangeColor, 3.1415927f + optimalSectorRad.Y, scale * scaleMultiplier, SpriteEffects.None, null);
				this.sectorSprite.Draw(spriteBatch, pointerPos, this.warningColor, 3.1415927f + allowedSectorRad.Y, scale * scaleMultiplier, SpriteEffects.None, null);
				spriteBatch.End();
				spriteBatch.GraphicsDevice.ScissorRectangle = prevScissorRect;
				spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);
			}
			meterSprite.Draw(spriteBatch, pos, 0f, scale, SpriteEffects.None);
			float normalizedValue = (value - range.X) / (range.Y - range.X);
			float valueRad = MathHelper.Lerp(sectorRad.X, sectorRad.Y, normalizedValue);
			this.meterPointer.Draw(spriteBatch, pointerPos, valueRad, scale, SpriteEffects.None);
		}

		// Token: 0x06005CEE RID: 23790 RVA: 0x00300FC8 File Offset: 0x002FF1C8
		private static void UpdateGraph<T>(IList<T> graph, T newValue)
		{
			for (int i = graph.Count - 1; i > 0; i--)
			{
				graph[i] = graph[i - 1];
			}
			graph[0] = newValue;
		}

		// Token: 0x06005CEF RID: 23791 RVA: 0x00301000 File Offset: 0x002FF200
		private void DrawGraph(IList<float> graph, SpriteBatch spriteBatch, Rectangle rect, float maxVal, float xOffset, Color color)
		{
			Rectangle prevScissorRect = spriteBatch.GraphicsDevice.ScissorRectangle;
			spriteBatch.End();
			spriteBatch.GraphicsDevice.ScissorRectangle = rect;
			spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, GameMain.ScissorTestEnable, null, null);
			float lineWidth = (float)rect.Width / (float)(graph.Count - 2);
			float yScale = (float)rect.Height / maxVal;
			Vector2 prevPoint = new Vector2((float)rect.Right, (float)rect.Bottom - (graph[1] + (graph[0] - graph[1]) * xOffset) * yScale);
			float currX = (float)rect.Right - (xOffset - 1f) * lineWidth;
			for (int i = 1; i < graph.Count - 1; i++)
			{
				currX -= lineWidth;
				Vector2 newPoint = new Vector2(currX, (float)rect.Bottom - graph[i] * yScale);
				Sprite sprite = this.graphLine;
				if (((sprite != null) ? sprite.Texture : null) == null)
				{
					GUI.DrawLine(spriteBatch, prevPoint, newPoint - new Vector2(1f, 0f), color, 0f, 1f);
				}
				else
				{
					Vector2 dir = Vector2.Normalize(newPoint - prevPoint);
					GUI.DrawLine(spriteBatch, this.graphLine.Texture, prevPoint - dir, newPoint + dir, color, 0f, 5);
				}
				prevPoint = newPoint;
			}
			Vector2 lastPoint = new Vector2((float)rect.X, (float)rect.Bottom - (graph[graph.Count - 1] + (graph[graph.Count - 2] - graph[graph.Count - 1]) * xOffset) * yScale);
			Sprite sprite2 = this.graphLine;
			if (((sprite2 != null) ? sprite2.Texture : null) == null)
			{
				GUI.DrawLine(spriteBatch, prevPoint, lastPoint, color, 0f, 1f);
			}
			else
			{
				GUI.DrawLine(spriteBatch, this.graphLine.Texture, prevPoint, lastPoint + (lastPoint - prevPoint), color, 0f, 5);
			}
			spriteBatch.End();
			spriteBatch.GraphicsDevice.ScissorRectangle = prevScissorRect;
			spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);
		}

		// Token: 0x06005CF0 RID: 23792 RVA: 0x0030122C File Offset: 0x002FF42C
		private void CreateInfrographic()
		{
			Reactor.<>c__DisplayClass74_0 CS$<>8__locals1 = new Reactor.<>c__DisplayClass74_0();
			CS$<>8__locals1.<>4__this = this;
			if (this.infographic != null)
			{
				return;
			}
			Color dimColor = Color.Lerp(Color.Black, Color.TransparentBlack, 0.25f);
			this.infographic = new GUIFrame(new RectTransform(Vector2.One, base.GuiFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null)
			{
				CanBeFocused = false,
				Color = dimColor
			};
			new GUIFrame(new RectTransform(this.inventoryWindow.Rect.Size, this.infographic.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false)
			{
				AbsoluteOffset = this.inventoryWindow.Rect.Location - base.GuiFrame.Rect.Location
			}, "", new Color?(dimColor)).CanBeFocused = false;
			CS$<>8__locals1.arrowSize = (int)(70f * GUI.Scale);
			CS$<>8__locals1.arrows = new Dictionary<string, GUIImage>
			{
				{
					"fuelslots",
					CS$<>8__locals1.<CreateInfrographic>g__CreateArrow|0(Reactor.InfographicArrowStyle.Curved, this.inventoryWindow, Anchor.TopLeft, Pivot.TopRight, SpriteEffects.FlipVertically, 0f, null)
				},
				{
					"temperature",
					CS$<>8__locals1.<CreateInfrographic>g__CreateArrow|0(Reactor.InfographicArrowStyle.Straight, this.temperatureBoostDownButton, Anchor.Center, Pivot.Center, SpriteEffects.None, 0f, null)
				},
				{
					"automaticcontrol",
					CS$<>8__locals1.<CreateInfrographic>g__CreateArrow|0(Reactor.InfographicArrowStyle.Curved, this.AutoTempSwitch, Anchor.TopRight, Pivot.BottomRight, SpriteEffects.None, 90f, null)
				},
				{
					"power",
					CS$<>8__locals1.<CreateInfrographic>g__CreateArrow|0(Reactor.InfographicArrowStyle.Straight, this.PowerButton, Anchor.BottomCenter, Pivot.TopCenter, SpriteEffects.None, 0f, null)
				}
			};
			CS$<>8__locals1.<CreateInfrographic>g__CreateArrow|0(Reactor.InfographicArrowStyle.Straight, this.FissionRateScrollBar, Anchor.Center, Pivot.Center, SpriteEffects.None, 0f, null);
			CS$<>8__locals1.<CreateInfrographic>g__CreateArrow|0(Reactor.InfographicArrowStyle.Straight, this.TurbineOutputScrollBar, Anchor.Center, Pivot.Center, SpriteEffects.None, 0f, null);
			CS$<>8__locals1.<CreateInfrographic>g__CreateArrow|0(Reactor.InfographicArrowStyle.Straight, this.graph, Anchor.TopLeft, Pivot.TopLeft, SpriteEffects.FlipHorizontally, 0f, new Point?(new Point(CS$<>8__locals1.arrowSize / 2, 0)));
			CS$<>8__locals1.<CreateInfrographic>g__CreateArrow|0(Reactor.InfographicArrowStyle.Straight, this.graph, Anchor.BottomLeft, Pivot.BottomLeft, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0f, new Point?(new Point(CS$<>8__locals1.arrowSize / 2, 0)));
			new GUICustomComponent(new RectTransform(Vector2.One, this.infographic.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), delegate(SpriteBatch sb, GUICustomComponent c)
			{
				Reactor.<>c__DisplayClass74_1 CS$<>8__locals2;
				CS$<>8__locals2.sb = sb;
				Reactor.<CreateInfrographic>g__DrawToolTip|74_3("fuelslots", Anchor.TopLeft, Pivot.BottomCenter, CS$<>8__locals1.arrows["fuelslots"], ref CS$<>8__locals2);
				Reactor.<CreateInfrographic>g__DrawToolTip|74_3("fissionrate", Anchor.TopCenter, Pivot.TopCenter, CS$<>8__locals1.<>4__this.buttonArea, ref CS$<>8__locals2);
				Reactor.<CreateInfrographic>g__DrawToolTip|74_3("temperature", Anchor.BottomLeft, Pivot.TopLeft, CS$<>8__locals1.arrows["temperature"], ref CS$<>8__locals2);
				Reactor.<CreateInfrographic>g__DrawToolTip|74_3("automaticcontrol", Anchor.TopLeft, Pivot.CenterRight, CS$<>8__locals1.arrows["automaticcontrol"], ref CS$<>8__locals2);
				Reactor.<CreateInfrographic>g__DrawToolTip|74_3("power", Anchor.BottomCenter, Pivot.TopCenter, CS$<>8__locals1.arrows["power"], ref CS$<>8__locals2);
				Reactor.<CreateInfrographic>g__DrawToolTip|74_3("load", Anchor.CenterLeft, Pivot.CenterLeft, CS$<>8__locals1.<>4__this.graph, ref CS$<>8__locals2);
			}, null).CanBeFocused = false;
			RectTransform closeButtonRt = new RectTransform(new Point(200, 50).Multiply(GUI.Scale), this.infographic.RectTransform, Anchor.TopRight, new Pivot?(Pivot.BottomRight), ScaleBasis.Normal, false)
			{
				AbsoluteOffset = new Point(0, -50).Multiply(GUI.Scale)
			};
			GUIButton guibutton = new GUIButton(closeButtonRt, TextManager.Get("closeinfographic"), Alignment.Center, "", null);
			guibutton.UserData = UIHighlightAction.ElementId.CloseButton;
			guibutton.OnClicked = delegate(GUIButton _, object _)
			{
				base.<CreateInfrographic>g__CloseInfographic|1(Character.Controlled);
				return true;
			};
			Item item = this.item;
			item.OnDeselect = (Action<Character>)Delegate.Combine(item.OnDeselect, new Action<Character>(CS$<>8__locals1.<CreateInfrographic>g__CloseInfographic|1));
		}

		// Token: 0x06005CF1 RID: 23793 RVA: 0x0030158C File Offset: 0x002FF78C
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			Sprite sprite = this.graphLine;
			if (sprite != null)
			{
				sprite.Remove();
			}
			Sprite sprite2 = this.fissionRateMeter;
			if (sprite2 != null)
			{
				sprite2.Remove();
			}
			Sprite sprite3 = this.turbineOutputMeter;
			if (sprite3 != null)
			{
				sprite3.Remove();
			}
			Sprite sprite4 = this.meterPointer;
			if (sprite4 != null)
			{
				sprite4.Remove();
			}
			Sprite sprite5 = this.sectorSprite;
			if (sprite5 != null)
			{
				sprite5.Remove();
			}
			Sprite sprite6 = this.tempMeterFrame;
			if (sprite6 != null)
			{
				sprite6.Remove();
			}
			Sprite sprite7 = this.tempMeterBar;
			if (sprite7 != null)
			{
				sprite7.Remove();
			}
			Sprite sprite8 = this.tempRangeIndicator;
			if (sprite8 == null)
			{
				return;
			}
			sprite8.Remove();
		}

		// Token: 0x06005CF2 RID: 23794 RVA: 0x00301628 File Offset: 0x002FF828
		public void ClientEventWrite(IWriteMessage msg, NetEntityEvent.IData extraData = null)
		{
			msg.WriteBoolean(this.autoTemp);
			msg.WriteBoolean(this.PowerOn);
			msg.WriteRangedSingle(this.TargetFissionRate, 0f, 100f, 8);
			msg.WriteRangedSingle(this.TargetTurbineOutput, 0f, 100f, 8);
			msg.WriteRangedSingle(this.temperatureBoost, -25f, 25f, 8);
			this.correctionTimer = 1f;
		}

		// Token: 0x06005CF3 RID: 23795 RVA: 0x003016A0 File Offset: 0x002FF8A0
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			if (this.correctionTimer > 0f)
			{
				base.StartDelayedCorrection(msg.ExtractBits(42), sendingTime, false);
				return;
			}
			this.AutoTemp = msg.ReadBoolean();
			this.PowerOn = msg.ReadBoolean();
			this.Temperature = msg.ReadRangedSingle(0f, 100f, 8);
			this.TargetFissionRate = msg.ReadRangedSingle(0f, 100f, 8);
			this.TargetTurbineOutput = msg.ReadRangedSingle(0f, 100f, 8);
			this.degreeOfSuccess = msg.ReadRangedSingle(0f, 1f, 8);
			this.ApplyTemperatureBoost(msg.ReadRangedSingle(-25f, 25f, 8));
			if (Math.Abs(this.FissionRateScrollBar.BarScroll - this.TargetFissionRate / 100f) > 0.01f)
			{
				this.FissionRateScrollBar.BarScroll = this.TargetFissionRate / 100f;
			}
			if (Math.Abs(this.TurbineOutputScrollBar.BarScroll - this.TargetTurbineOutput / 100f) > 0.01f)
			{
				this.TurbineOutputScrollBar.BarScroll = this.TargetTurbineOutput / 100f;
			}
			this.IsActive = true;
		}

		// Token: 0x06005CF4 RID: 23796 RVA: 0x003017D4 File Offset: 0x002FF9D4
		private void UpdateUIElementStates()
		{
			if (this.powerLight != null)
			{
				this.powerLight.Selected = this._powerOn;
			}
			if (this.AutoTempSwitch != null)
			{
				this.AutoTempSwitch.Selected = this.autoTemp;
				this.AutoTempSwitch.Enabled = this._powerOn;
			}
			if (this.autoTempLight != null)
			{
				this.autoTempLight.Selected = (this.autoTemp && this._powerOn);
			}
			if (this.FissionRateScrollBar != null)
			{
				this.FissionRateScrollBar.Enabled = (this._powerOn && !this.autoTemp);
			}
			if (this.TurbineOutputScrollBar != null)
			{
				this.TurbineOutputScrollBar.Enabled = (this._powerOn && !this.autoTemp);
			}
		}

		// Token: 0x17001762 RID: 5986
		// (get) Token: 0x06005CF5 RID: 23797 RVA: 0x00301896 File Offset: 0x002FFA96
		public bool AllowTemperatureBoost
		{
			get
			{
				return Math.Abs(this.temperatureBoost) < 22.5f;
			}
		}

		// Token: 0x17001763 RID: 5987
		// (get) Token: 0x06005CF6 RID: 23798 RVA: 0x003018AA File Offset: 0x002FFAAA
		// (set) Token: 0x06005CF7 RID: 23799 RVA: 0x003018B2 File Offset: 0x002FFAB2
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool PowerOn
		{
			get
			{
				return this._powerOn;
			}
			set
			{
				this._powerOn = value;
				this.UpdateUIElementStates();
			}
		}

		// Token: 0x17001764 RID: 5988
		// (get) Token: 0x06005CF8 RID: 23800 RVA: 0x003018C1 File Offset: 0x002FFAC1
		protected override PowerPriority Priority
		{
			get
			{
				return PowerPriority.Reactor;
			}
		}

		// Token: 0x17001765 RID: 5989
		// (get) Token: 0x06005CF9 RID: 23801 RVA: 0x003018C4 File Offset: 0x002FFAC4
		// (set) Token: 0x06005CFA RID: 23802 RVA: 0x003018CC File Offset: 0x002FFACC
		public Character LastAIUser { get; private set; }

		// Token: 0x17001766 RID: 5990
		// (get) Token: 0x06005CFB RID: 23803 RVA: 0x003018D5 File Offset: 0x002FFAD5
		// (set) Token: 0x06005CFC RID: 23804 RVA: 0x003018DD File Offset: 0x002FFADD
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool LastUserWasPlayer { get; private set; }

		// Token: 0x17001767 RID: 5991
		// (get) Token: 0x06005CFD RID: 23805 RVA: 0x003018E6 File Offset: 0x002FFAE6
		// (set) Token: 0x06005CFE RID: 23806 RVA: 0x003018F0 File Offset: 0x002FFAF0
		public Character LastUser
		{
			get
			{
				return this.lastUser;
			}
			private set
			{
				if (this.lastUser == value)
				{
					return;
				}
				if (Screen.Selected.IsEditor)
				{
					return;
				}
				this.lastUser = value;
				if (this.lastUser == null)
				{
					this.degreeOfSuccess = 0f;
					this.LastUserWasPlayer = false;
					return;
				}
				this.degreeOfSuccess = Math.Min(base.DegreeOfSuccess(this.lastUser), 1f);
				this.LastUserWasPlayer = this.lastUser.IsPlayer;
			}
		}

		// Token: 0x17001768 RID: 5992
		// (get) Token: 0x06005CFF RID: 23807 RVA: 0x00301963 File Offset: 0x002FFB63
		// (set) Token: 0x06005D00 RID: 23808 RVA: 0x0030196B File Offset: 0x002FFB6B
		[Editable(0f, 3.4028235E+38f, 1)]
		[Serialize(10000f, IsPropertySaveable.Yes, "How much power (kW) the reactor generates when operating at full capacity.", "", true)]
		public float MaxPowerOutput
		{
			get
			{
				return this.maxPowerOutput;
			}
			set
			{
				this.maxPowerOutput = Math.Max(0f, value);
			}
		}

		// Token: 0x17001769 RID: 5993
		// (get) Token: 0x06005D01 RID: 23809 RVA: 0x0030197E File Offset: 0x002FFB7E
		// (set) Token: 0x06005D02 RID: 23810 RVA: 0x00301986 File Offset: 0x002FFB86
		[Editable(0f, 3.4028235E+38f, 1)]
		[Serialize(120f, IsPropertySaveable.Yes, "How long the temperature has to stay critical until a meltdown occurs.", "", false)]
		public float MeltdownDelay
		{
			get
			{
				return this.meltDownDelay;
			}
			set
			{
				this.meltDownDelay = Math.Max(value, 0f);
			}
		}

		// Token: 0x1700176A RID: 5994
		// (get) Token: 0x06005D03 RID: 23811 RVA: 0x00301999 File Offset: 0x002FFB99
		// (set) Token: 0x06005D04 RID: 23812 RVA: 0x003019A1 File Offset: 0x002FFBA1
		[Editable(0f, 3.4028235E+38f, 1)]
		[Serialize(30f, IsPropertySaveable.Yes, "How long the temperature has to stay critical until the reactor catches fire.", "", false)]
		public float FireDelay
		{
			get
			{
				return this.fireDelay;
			}
			set
			{
				this.fireDelay = Math.Max(value, 0f);
			}
		}

		// Token: 0x1700176B RID: 5995
		// (get) Token: 0x06005D05 RID: 23813 RVA: 0x003019B4 File Offset: 0x002FFBB4
		// (set) Token: 0x06005D06 RID: 23814 RVA: 0x003019BC File Offset: 0x002FFBBC
		[Serialize(0f, IsPropertySaveable.Yes, "Current temperature of the reactor (0% - 100%). Indended to be used by StatusEffect conditionals.", "", false)]
		public float Temperature
		{
			get
			{
				return this.temperature;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					return;
				}
				this.temperature = MathHelper.Clamp(value, 0f, 100f);
			}
		}

		// Token: 0x1700176C RID: 5996
		// (get) Token: 0x06005D07 RID: 23815 RVA: 0x003019DD File Offset: 0x002FFBDD
		// (set) Token: 0x06005D08 RID: 23816 RVA: 0x003019E5 File Offset: 0x002FFBE5
		[Serialize(0f, IsPropertySaveable.Yes, "Current fission rate of the reactor (0% - 100%). Intended to be used by StatusEffect conditionals (setting the value from XML is not recommended).", "", false)]
		public float FissionRate
		{
			get
			{
				return this.fissionRate;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					return;
				}
				this.fissionRate = MathHelper.Clamp(value, 0f, 100f);
			}
		}

		// Token: 0x1700176D RID: 5997
		// (get) Token: 0x06005D09 RID: 23817 RVA: 0x00301A06 File Offset: 0x002FFC06
		// (set) Token: 0x06005D0A RID: 23818 RVA: 0x00301A0E File Offset: 0x002FFC0E
		[Serialize(0f, IsPropertySaveable.Yes, "Current turbine output of the reactor (0% - 100%). Intended to be used by StatusEffect conditionals (setting the value from XML is not recommended).", "", false)]
		public float TurbineOutput
		{
			get
			{
				return this.turbineOutput;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					return;
				}
				this.turbineOutput = MathHelper.Clamp(value, 0f, 100f);
			}
		}

		// Token: 0x1700176E RID: 5998
		// (get) Token: 0x06005D0B RID: 23819 RVA: 0x00301A2F File Offset: 0x002FFC2F
		// (set) Token: 0x06005D0C RID: 23820 RVA: 0x00301A37 File Offset: 0x002FFC37
		[Serialize(0.2f, IsPropertySaveable.Yes, "How fast the condition of the contained fuel rods deteriorates per second.", "", false)]
		[Editable(0f, 1000f, 3)]
		public float FuelConsumptionRate
		{
			get
			{
				return this.fuelConsumptionRate;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					return;
				}
				this.fuelConsumptionRate = Math.Max(value, 0f);
			}
		}

		// Token: 0x1700176F RID: 5999
		// (get) Token: 0x06005D0D RID: 23821 RVA: 0x00301A53 File Offset: 0x002FFC53
		// (set) Token: 0x06005D0E RID: 23822 RVA: 0x00301A68 File Offset: 0x002FFC68
		[Serialize(false, IsPropertySaveable.Yes, "Is the temperature currently critical. Intended to be used by StatusEffect conditionals (setting the value from XML has no effect).", "", false)]
		public bool TemperatureCritical
		{
			get
			{
				return this.temperature > this.allowedTemperature.Y;
			}
			set
			{
			}
		}

		// Token: 0x17001770 RID: 6000
		// (get) Token: 0x06005D0F RID: 23823 RVA: 0x00301A6A File Offset: 0x002FFC6A
		// (set) Token: 0x06005D10 RID: 23824 RVA: 0x00301A72 File Offset: 0x002FFC72
		[Serialize(false, IsPropertySaveable.Yes, "Is the automatic temperature control currently on. Indended to be used by StatusEffect conditionals (setting the value from XML is not recommended).", "", false)]
		public bool AutoTemp
		{
			get
			{
				return this.autoTemp;
			}
			set
			{
				this.autoTemp = value;
				this.UpdateUIElementStates();
			}
		}

		// Token: 0x17001771 RID: 6001
		// (get) Token: 0x06005D11 RID: 23825 RVA: 0x00301A81 File Offset: 0x002FFC81
		// (set) Token: 0x06005D12 RID: 23826 RVA: 0x00301A89 File Offset: 0x002FFC89
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float AvailableFuel { get; set; }

		// Token: 0x17001772 RID: 6002
		// (get) Token: 0x06005D13 RID: 23827 RVA: 0x00301A92 File Offset: 0x002FFC92
		// (set) Token: 0x06005D14 RID: 23828 RVA: 0x00301A9A File Offset: 0x002FFC9A
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public new float Load { get; private set; }

		// Token: 0x17001773 RID: 6003
		// (get) Token: 0x06005D15 RID: 23829 RVA: 0x00301AA3 File Offset: 0x002FFCA3
		// (set) Token: 0x06005D16 RID: 23830 RVA: 0x00301AAB File Offset: 0x002FFCAB
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float TargetFissionRate { get; set; }

		// Token: 0x17001774 RID: 6004
		// (get) Token: 0x06005D17 RID: 23831 RVA: 0x00301AB4 File Offset: 0x002FFCB4
		// (set) Token: 0x06005D18 RID: 23832 RVA: 0x00301ABC File Offset: 0x002FFCBC
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float TargetTurbineOutput { get; set; }

		// Token: 0x17001775 RID: 6005
		// (get) Token: 0x06005D19 RID: 23833 RVA: 0x00301AC5 File Offset: 0x002FFCC5
		// (set) Token: 0x06005D1A RID: 23834 RVA: 0x00301ACD File Offset: 0x002FFCCD
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float CorrectTurbineOutput { get; set; }

		// Token: 0x17001776 RID: 6006
		// (get) Token: 0x06005D1B RID: 23835 RVA: 0x00301AD6 File Offset: 0x002FFCD6
		// (set) Token: 0x06005D1C RID: 23836 RVA: 0x00301ADE File Offset: 0x002FFCDE
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool ExplosionDamagesOtherSubs { get; set; }

		// Token: 0x17001777 RID: 6007
		// (get) Token: 0x06005D1D RID: 23837 RVA: 0x00301AE7 File Offset: 0x002FFCE7
		// (set) Token: 0x06005D1E RID: 23838 RVA: 0x00301AEF File Offset: 0x002FFCEF
		public bool MeltedDownThisRound { get; private set; }

		// Token: 0x06005D1F RID: 23839 RVA: 0x00301AF8 File Offset: 0x002FFCF8
		public Reactor(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
			this.InitProjSpecific(element);
		}

		// Token: 0x06005D20 RID: 23840 RVA: 0x00301BCC File Offset: 0x002FFDCC
		private void InitProjSpecific(ContentXElement element)
		{
			this.CreateGUI();
			ContentXElement childElement = element.GetChildElement("fissionratemeter");
			this.fissionRateMeter = new Sprite((childElement != null) ? childElement.GetChildElement("sprite") : null, "", "", false, 1f);
			ContentXElement childElement2 = element.GetChildElement("turbineoutputmeter");
			this.turbineOutputMeter = new Sprite((childElement2 != null) ? childElement2.GetChildElement("sprite") : null, "", "", false, 1f);
			ContentXElement childElement3 = element.GetChildElement("meterpointer");
			this.meterPointer = new Sprite((childElement3 != null) ? childElement3.GetChildElement("sprite") : null, "", "", false, 1f);
			ContentXElement childElement4 = element.GetChildElement("sectorsprite");
			this.sectorSprite = new Sprite((childElement4 != null) ? childElement4.GetChildElement("sprite") : null, "", "", false, 1f);
			ContentXElement childElement5 = element.GetChildElement("tempmeterframe");
			this.tempMeterFrame = new Sprite((childElement5 != null) ? childElement5.GetChildElement("sprite") : null, "", "", false, 1f);
			ContentXElement childElement6 = element.GetChildElement("tempmeterbar");
			this.tempMeterBar = new Sprite((childElement6 != null) ? childElement6.GetChildElement("sprite") : null, "", "", false, 1f);
			ContentXElement childElement7 = element.GetChildElement("temprangeindicator");
			this.tempRangeIndicator = new Sprite((childElement7 != null) ? childElement7.GetChildElement("sprite") : null, "", "", false, 1f);
			ContentXElement childElement8 = element.GetChildElement("graphline");
			this.graphLine = new Sprite((childElement8 != null) ? childElement8.GetChildElement("sprite") : null, "", "", false, 1f);
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "temperatureboostsoundup"))
				{
					if (a == "temperatureboostsounddown")
					{
						this.temperatureBoostSoundDown = RoundSound.Load(subElement);
					}
				}
				else
				{
					this.temperatureBoostSoundUp = RoundSound.Load(subElement);
				}
			}
		}

		// Token: 0x06005D21 RID: 23841 RVA: 0x00301E18 File Offset: 0x00300018
		public override void Update(float deltaTime, Camera cam)
		{
			if (this.LastAIUser != null && this.LastAIUser.SelectedItem != this.item && this.LastAIUser.CanInteractWith(this.item, true))
			{
				this.AutoTemp = true;
				NetworkMember networkMember = GameMain.NetworkMember;
				if (networkMember != null && networkMember.IsServer)
				{
					this.unsentChanges = true;
				}
				this.LastAIUser = null;
			}
			bool fissionRateControlledBySignals = this.signalControlledTargetFissionRate != null && this.lastReceivedFissionRateSignalTime > Timing.TotalTime - 1.0;
			bool turbineOutputRateControlledBySignals = this.signalControlledTargetTurbineOutput != null && this.lastReceivedTurbineOutputSignalTime > Timing.TotalTime - 1.0;
			GameSession gameSession = GameMain.GameSession;
			if (gameSession != null && gameSession.RoundDuration < 5f)
			{
				Character character = this.lastUser;
				if ((character == null || !character.IsPlayer) && this.PowerOn && this.AutoTemp && !fissionRateControlledBySignals && !turbineOutputRateControlledBySignals)
				{
					this.UpdateAutoTemp(100f, 0.16666667f);
				}
			}
			if (this.PowerOn && this.AvailableFuel < 1f)
			{
				HintManager.OnReactorOutOfFuel(this);
			}
			float maxPowerOut = this.GetMaxOutput();
			if (fissionRateControlledBySignals)
			{
				this.TargetFissionRate = Reactor.<Update>g__adjustValueWithoutOverShooting|185_0(this.TargetFissionRate, this.signalControlledTargetFissionRate.Value, deltaTime * 5f);
				this.FissionRateScrollBar.BarScroll = this.TargetFissionRate / 100f;
			}
			else
			{
				this.signalControlledTargetFissionRate = null;
			}
			if (turbineOutputRateControlledBySignals)
			{
				this.TargetTurbineOutput = Reactor.<Update>g__adjustValueWithoutOverShooting|185_0(this.TargetTurbineOutput, this.signalControlledTargetTurbineOutput.Value, deltaTime * 5f);
				this.TurbineOutputScrollBar.BarScroll = this.TargetTurbineOutput / 100f;
			}
			else
			{
				this.signalControlledTargetTurbineOutput = null;
			}
			this.prevAvailableFuel = this.AvailableFuel;
			base.ApplyStatusEffects(ActionType.OnActive, deltaTime, null, null, null, null, null, 1f);
			if (!MathUtils.NearlyEqual(maxPowerOut, 0f, 0.0001f))
			{
				this.CorrectTurbineOutput += MathHelper.Clamp(this.Load / maxPowerOut * 100f - this.CorrectTurbineOutput, -20f, 20f) * deltaTime;
			}
			float tolerance = MathHelper.Lerp(2.5f, 10f, this.degreeOfSuccess);
			this.optimalTurbineOutput = new Vector2(this.CorrectTurbineOutput - tolerance, this.CorrectTurbineOutput + tolerance);
			tolerance = MathHelper.Lerp(5f, 20f, this.degreeOfSuccess);
			this.allowedTurbineOutput = new Vector2(this.CorrectTurbineOutput - tolerance, this.CorrectTurbineOutput + tolerance);
			this.optimalTemperature = Vector2.Lerp(new Vector2(40f, 60f), new Vector2(30f, 70f), this.degreeOfSuccess);
			this.allowedTemperature = Vector2.Lerp(new Vector2(30f, 70f), new Vector2(10f, 90f), this.degreeOfSuccess);
			this.optimalFissionRate = Vector2.Lerp(new Vector2(30f, this.AvailableFuel - 20f), new Vector2(20f, this.AvailableFuel - 10f), this.degreeOfSuccess);
			this.optimalFissionRate.X = Math.Min(this.optimalFissionRate.X, this.optimalFissionRate.Y - 10f);
			this.allowedFissionRate = Vector2.Lerp(new Vector2(20f, this.AvailableFuel), new Vector2(10f, this.AvailableFuel), this.degreeOfSuccess);
			this.allowedFissionRate.X = Math.Min(this.allowedFissionRate.X, this.allowedFissionRate.Y - 10f);
			float heatAmount = this.GetGeneratedHeat(this.fissionRate);
			float temperatureDiff = heatAmount - this.turbineOutput - this.Temperature;
			this.Temperature += MathHelper.Clamp((float)Math.Sign(temperatureDiff) * 10f * deltaTime, -Math.Abs(temperatureDiff), Math.Abs(temperatureDiff));
			this.temperatureBoost = Reactor.<Update>g__adjustValueWithoutOverShooting|185_0(this.temperatureBoost, 0f, deltaTime);
			this.temperatureBoostUpButton.Enabled = (this.temperatureBoostDownButton.Enabled = this.AllowTemperatureBoost);
			this.FissionRate = MathHelper.Lerp(this.fissionRate, Math.Min(this.TargetFissionRate, this.AvailableFuel), deltaTime);
			this.TurbineOutput = MathHelper.Lerp(this.turbineOutput, this.TargetTurbineOutput, deltaTime);
			float temperatureFactor = Math.Min(this.temperature / 50f, 1f);
			if (!this.PowerOn)
			{
				this.TargetFissionRate = 0f;
				this.TargetTurbineOutput = 0f;
			}
			else if (this.autoTemp)
			{
				this.UpdateAutoTemp(2f, deltaTime);
			}
			float fuelLeft = 0f;
			ItemInventory ownInventory = this.item.OwnInventory;
			IEnumerable<Item> containedItems = (ownInventory != null) ? ownInventory.AllItems : null;
			if (containedItems != null)
			{
				foreach (Item item in containedItems)
				{
					if (item.HasTag(Tags.ReactorFuel))
					{
						if (this.fissionRate > 0f)
						{
							if (!Level.IsLoadedOutpost)
							{
								goto IL_56D;
							}
							Submarine submarine = base.Item.Submarine;
							if (submarine == null || submarine.TeamID != CharacterTeamType.Team1)
							{
								goto IL_56D;
							}
							bool flag = base.Item.Submarine.GetConnectedSubs().Any((Submarine s) => s.Info.IsOutpost && s.TeamID == CharacterTeamType.FriendlyNPC);
							IL_56E:
							if (!flag)
							{
								item.Condition -= this.fissionRate / 100f * this.GetFuelConsumption() * deltaTime;
								goto IL_597;
							}
							goto IL_597;
							IL_56D:
							flag = false;
							goto IL_56E;
						}
						IL_597:
						fuelLeft += item.ConditionPercentage;
					}
				}
			}
			if (this.fissionRate > 0f && this.item.AiTarget != null && maxPowerOut > 0f)
			{
				AITarget aiTarget = this.item.AiTarget;
				float range = Math.Abs(this.currPowerConsumption) / maxPowerOut;
				aiTarget.SoundRange = MathHelper.Lerp(aiTarget.MinSoundRange, aiTarget.MaxSoundRange, range);
				if (this.item.CurrentHull != null)
				{
					AITarget hullAITarget = this.item.CurrentHull.AiTarget;
					if (hullAITarget != null)
					{
						hullAITarget.SoundRange = Math.Max(hullAITarget.SoundRange, aiTarget.SoundRange);
					}
				}
			}
			this.item.SendSignal(((int)(this.temperature * 100f)).ToString(), "temperature_out");
			this.item.SendSignal(((int)(-(int)base.CurrPowerConsumption)).ToString(), "power_value_out");
			this.item.SendSignal(((int)this.Load).ToString(), "load_value_out");
			this.item.SendSignal(((int)this.AvailableFuel).ToString(), "fuel_out");
			this.item.SendSignal(((int)fuelLeft).ToString(), "fuel_percentage_left");
			this.UpdateFailures(deltaTime);
			this.UpdateGraph(deltaTime);
			this.AvailableFuel = 0f;
			this.sendUpdateTimer -= deltaTime;
			if (this.unsentChanges && this.sendUpdateTimer <= 0f)
			{
				if (GameMain.Client != null)
				{
					this.item.CreateClientEvent<Reactor>(this);
				}
				this.sendUpdateTimer = 0.5f;
				this.unsentChanges = false;
			}
		}

		// Token: 0x06005D22 RID: 23842 RVA: 0x00302590 File Offset: 0x00300790
		public override float GetCurrentPowerConsumption(Connection connection = null)
		{
			return (float)((connection != null && connection.IsPower && connection.IsOutput) ? -1 : 0);
		}

		// Token: 0x06005D23 RID: 23843 RVA: 0x003025AC File Offset: 0x003007AC
		public override PowerRange MinMaxPowerOut(Connection conn, float load)
		{
			float tolerance = 1f;
			if (this.turbineOutput > this.optimalTurbineOutput.X && this.turbineOutput < this.optimalTurbineOutput.Y && this.temperature > this.optimalTemperature.X && this.temperature < this.optimalTemperature.Y)
			{
				tolerance = 3f;
			}
			float maxPowerOut = this.GetMaxOutput();
			float temperatureFactor = Math.Min(this.temperature / 50f, 1f);
			float minOutput = maxPowerOut * Math.Clamp(Math.Min((this.turbineOutput - tolerance) / 100f, temperatureFactor), 0f, 1f);
			float maxOutput = maxPowerOut * Math.Min((this.turbineOutput + tolerance) / 100f, temperatureFactor);
			this.minUpdatePowerOut = minOutput;
			this.maxUpdatePowerOut = maxOutput;
			float reactorMax = this.PowerOn ? maxPowerOut : this.maxUpdatePowerOut;
			return new PowerRange(minOutput, maxOutput, reactorMax);
		}

		// Token: 0x06005D24 RID: 23844 RVA: 0x0030269C File Offset: 0x0030089C
		public override float GetConnectionPowerOut(Connection conn, float power, PowerRange minMaxPower, float load)
		{
			float loadLeft = MathHelper.Max(load - power, 0f);
			float expectedPower = MathHelper.Clamp(loadLeft, minMaxPower.Min, minMaxPower.Max);
			float ratio = MathHelper.Max((loadLeft - minMaxPower.Min) / (minMaxPower.Max - minMaxPower.Min), 0f);
			if (float.IsInfinity(ratio))
			{
				ratio = 0f;
			}
			float output = MathHelper.Clamp(ratio * (this.maxUpdatePowerOut - this.minUpdatePowerOut) + this.minUpdatePowerOut, this.minUpdatePowerOut, this.maxUpdatePowerOut);
			float newLoad = loadLeft;
			float maxOutput = this.GetMaxOutput();
			if (maxOutput != minMaxPower.ReactorMaxOutput)
			{
				float idealLoad = maxOutput / minMaxPower.ReactorMaxOutput * loadLeft;
				float loadAdjust = MathHelper.Clamp((ratio - 0.5f) * 25f + idealLoad - this.turbineOutput / 100f * maxOutput, -maxOutput / 100f, maxOutput / 100f);
				newLoad = MathHelper.Clamp(loadLeft - (expectedPower - output) + loadAdjust, 0f, loadLeft);
			}
			if (float.IsNegative(newLoad))
			{
				newLoad = 0f;
			}
			this.Load = newLoad;
			this.currPowerConsumption = -output;
			return output;
		}

		// Token: 0x06005D25 RID: 23845 RVA: 0x003027B2 File Offset: 0x003009B2
		private float GetGeneratedHeat(float fissionRate)
		{
			return fissionRate * (this.prevAvailableFuel / 100f) * 2f + this.temperatureBoost;
		}

		// Token: 0x06005D26 RID: 23846 RVA: 0x003027D0 File Offset: 0x003009D0
		private bool NeedMoreFuel(float minimumOutputRatio, float minCondition = 0f)
		{
			float remainingFuel = this.item.ContainedItems.Sum((Item i) => i.Condition);
			if (remainingFuel <= minCondition && this.Load > 0f)
			{
				return true;
			}
			float maxFissionRate = Math.Min(this.prevAvailableFuel, 100f);
			if (maxFissionRate >= 100f)
			{
				return false;
			}
			float maxTurbineOutput = 100f;
			float theoreticalMaxHeat = this.GetGeneratedHeat(maxFissionRate);
			float temperatureFactor = Math.Min(theoreticalMaxHeat / 50f, 1f);
			float theoreticalMaxOutput = Math.Min(maxTurbineOutput / 100f, temperatureFactor) * this.GetMaxOutput();
			return theoreticalMaxOutput < this.Load * minimumOutputRatio;
		}

		// Token: 0x06005D27 RID: 23847 RVA: 0x00302880 File Offset: 0x00300A80
		private bool TooMuchFuel()
		{
			ItemInventory ownInventory = this.item.OwnInventory;
			IEnumerable<Item> containedItems = (ownInventory != null) ? ownInventory.AllItems : null;
			if (containedItems != null && containedItems.Count<Item>() <= 1)
			{
				return false;
			}
			float minimumHeat = this.GetGeneratedHeat(this.optimalFissionRate.X);
			return minimumHeat > Math.Min(this.CorrectTurbineOutput * 1.5f, 90f);
		}

		// Token: 0x06005D28 RID: 23848 RVA: 0x003028E0 File Offset: 0x00300AE0
		private void UpdateFailures(float deltaTime)
		{
			if (this.temperature > this.allowedTemperature.Y)
			{
				this.item.SendSignal("1", "meltdown_warning");
				if (!this.item.InvulnerableToDamage)
				{
					this.meltDownTimer += MathHelper.Lerp(deltaTime * 2f, deltaTime, this.item.Condition / this.item.MaxCondition);
					if (this.meltDownTimer > this.MeltdownDelay)
					{
						this.MeltDown();
						return;
					}
				}
			}
			else
			{
				this.item.SendSignal("0", "meltdown_warning");
				this.meltDownTimer = Math.Max(0f, this.meltDownTimer - deltaTime);
			}
			if (this.temperature > this.optimalTemperature.Y)
			{
				this.fireTimer += MathHelper.Lerp(deltaTime * 2f, deltaTime, this.item.Condition / this.item.MaxCondition);
				if (this.fireTimer >= this.FireDelay)
				{
					new FireSource(this.item.WorldPosition, null, null, false);
					this.fireTimer = 0f;
					return;
				}
			}
			else
			{
				this.fireTimer = Math.Max(0f, this.fireTimer - deltaTime);
			}
		}

		// Token: 0x06005D29 RID: 23849 RVA: 0x00302A20 File Offset: 0x00300C20
		public void UpdateAutoTemp(float speed, float deltaTime)
		{
			float desiredTurbineOutput = (this.optimalTurbineOutput.X + this.optimalTurbineOutput.Y) / 2f;
			this.TargetTurbineOutput += MathHelper.Clamp(desiredTurbineOutput - this.TargetTurbineOutput, -speed, speed) * deltaTime;
			this.TargetTurbineOutput = MathHelper.Clamp(this.TargetTurbineOutput, 0f, 100f);
			float desiredFissionRate = (this.optimalFissionRate.X + this.optimalFissionRate.Y) / 2f;
			this.TargetFissionRate += MathHelper.Clamp(desiredFissionRate - this.TargetFissionRate, -speed, speed) * deltaTime;
			if (this.temperature > (this.optimalTemperature.X + this.optimalTemperature.Y) / 2f)
			{
				this.TargetFissionRate = Math.Min(this.TargetFissionRate - speed * 2f * deltaTime, this.allowedFissionRate.Y);
			}
			else if (-this.currPowerConsumption < this.Load)
			{
				this.TargetFissionRate = Math.Min(this.TargetFissionRate + speed * 2f * deltaTime, 100f);
			}
			this.TargetFissionRate = MathHelper.Clamp(this.TargetFissionRate, 0f, 100f);
			this.TargetFissionRate = MathHelper.Clamp(this.TargetFissionRate, this.FissionRate - 5f, this.FissionRate + 5f);
		}

		// Token: 0x06005D2A RID: 23850 RVA: 0x00302B84 File Offset: 0x00300D84
		public void PowerUpImmediately()
		{
			this.PowerOn = true;
			this.AutoTemp = true;
			this.prevAvailableFuel = this.AvailableFuel;
			for (int i = 0; i < 100; i++)
			{
				this.Update(0.16666667f, null);
				this.UpdateAutoTemp(100f, 0.16666667f);
				this.AvailableFuel = this.prevAvailableFuel;
			}
		}

		// Token: 0x06005D2B RID: 23851 RVA: 0x00302BE0 File Offset: 0x00300DE0
		public override void UpdateBroken(float deltaTime, Camera cam)
		{
			base.UpdateBroken(deltaTime, cam);
			this.item.SendSignal(((int)(this.temperature * 100f)).ToString(), "temperature_out");
			this.currPowerConsumption = 0f;
			this.Temperature -= deltaTime * 1000f;
			this.TargetFissionRate = Math.Max(this.TargetFissionRate - deltaTime * 10f, 0f);
			this.TargetTurbineOutput = Math.Max(this.TargetTurbineOutput - deltaTime * 10f, 0f);
			this.FissionRateScrollBar.BarScroll = 1f - this.FissionRate / 100f;
			this.TurbineOutputScrollBar.BarScroll = 1f - this.TurbineOutput / 100f;
			this.UpdateGraph(deltaTime);
		}

		// Token: 0x06005D2C RID: 23852 RVA: 0x00302CB8 File Offset: 0x00300EB8
		private void MeltDown()
		{
			if (this.item.Condition <= 0f)
			{
				return;
			}
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			if (!this.ExplosionDamagesOtherSubs)
			{
				Dictionary<ActionType, List<StatusEffect>> statusEffectLists = this.statusEffectLists;
				if (statusEffectLists != null && statusEffectLists.ContainsKey(ActionType.OnBroken))
				{
					foreach (StatusEffect statusEffect in this.statusEffectLists[ActionType.OnBroken])
					{
						foreach (Explosion explosion in statusEffect.Explosions)
						{
							foreach (Submarine sub in Submarine.Loaded)
							{
								if (sub != this.item.Submarine)
								{
									explosion.IgnoredSubmarines.Add(sub);
								}
							}
						}
					}
				}
			}
			this.item.Condition = 0f;
			this.fireTimer = 0f;
			this.meltDownTimer = 0f;
			this.MeltedDownThisRound = true;
			ItemInventory ownInventory = this.item.OwnInventory;
			IEnumerable<Item> containedItems = (ownInventory != null) ? ownInventory.AllItems : null;
			if (containedItems != null)
			{
				foreach (Item containedItem in containedItems)
				{
					containedItem.Condition = 0f;
				}
			}
		}

		// Token: 0x06005D2D RID: 23853 RVA: 0x00302E78 File Offset: 0x00301078
		public override bool Pick(Character picker)
		{
			return picker != null;
		}

		// Token: 0x06005D2E RID: 23854 RVA: 0x00302E80 File Offset: 0x00301080
		public override bool CrewAIOperate(float deltaTime, Character character, AIObjectiveOperateItem objective)
		{
			Reactor.<>c__DisplayClass198_0 CS$<>8__locals1 = new Reactor.<>c__DisplayClass198_0();
			CS$<>8__locals1.character = character;
			CS$<>8__locals1.<>4__this = this;
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return false;
			}
			CS$<>8__locals1.character.AIController.SteeringManager.Reset();
			bool shutDown = objective.Option == "shutdown";
			this.IsActive = true;
			if (!shutDown)
			{
				float degreeOfSuccess = Math.Min(base.DegreeOfSuccess(CS$<>8__locals1.character), 1f);
				float refuelLimit = 0.3f;
				if (degreeOfSuccess > refuelLimit)
				{
					if (this.aiUpdateTimer > 0f)
					{
						this.aiUpdateTimer -= deltaTime;
						return false;
					}
					this.aiUpdateTimer = 0.2f;
					float minCondition = this.GetFuelConsumption() * MathUtils.Pow2((degreeOfSuccess - refuelLimit) * 2f);
					if (this.NeedMoreFuel(0.5f, minCondition))
					{
						Reactor.<>c__DisplayClass198_1 CS$<>8__locals2 = new Reactor.<>c__DisplayClass198_1();
						CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
						CS$<>8__locals2.outOfFuel = false;
						ItemContainer container = this.item.GetComponent<ItemContainer>();
						if (objective.SubObjectives.None(null))
						{
							AIObjectiveContainItem containObjective = base.AIContainItems<Reactor>(container, CS$<>8__locals2.CS$<>8__locals1.character, objective, 1, true, true, !CS$<>8__locals2.CS$<>8__locals1.character.IsOnPlayerTeam, true);
							containObjective.Completed += CS$<>8__locals2.<CrewAIOperate>g__ReportFuelRodCount|2;
							containObjective.Abandoned += CS$<>8__locals2.<CrewAIOperate>g__ReportFuelRodCount|2;
							CS$<>8__locals2.CS$<>8__locals1.character.Speak(TextManager.Get("DialogReactorFuel").Value, null, 0f, Tags.ReactorFuel, 30f);
						}
						return CS$<>8__locals2.outOfFuel;
					}
					if (base.Item.ConditionPercentage <= 0f && AIObjectiveRepairItems.IsValidTarget(base.Item, CS$<>8__locals1.character))
					{
						if (base.Item.Repairables.Average((Repairable r) => r.DegreeOfSuccess(CS$<>8__locals1.character)) > 0.4f)
						{
							objective.AddSubObjective(new AIObjectiveRepairItem(CS$<>8__locals1.character, base.Item, objective.objectiveManager, 1f, true), false);
							return false;
						}
						Character character2 = CS$<>8__locals1.character;
						string value = TextManager.Get("DialogReactorIsBroken").Value;
						Identifier identifier = "reactorisbroken".ToIdentifier();
						character2.Speak(value, null, 0f, identifier, 30f);
					}
					if (this.TooMuchFuel())
					{
						CS$<>8__locals1.<CrewAIOperate>g__DropFuel|0(0.1f, 100f);
					}
					else
					{
						CS$<>8__locals1.<CrewAIOperate>g__DropFuel|0(0f, 0f);
					}
				}
			}
			if (objective.Override)
			{
				if (this.lastUser != null && this.lastUser != CS$<>8__locals1.character && this.lastUser != this.LastAIUser && this.lastUser.SelectedItem == this.item && CS$<>8__locals1.character.IsOnPlayerTeam)
				{
					CS$<>8__locals1.character.Speak(TextManager.Get("DialogReactorTaken").Value, null, 0f, "reactortaken".ToIdentifier(), 10f);
				}
			}
			else if (this.LastUserWasPlayer && this.lastUser != null && this.lastUser.TeamID == CS$<>8__locals1.character.TeamID)
			{
				return true;
			}
			this.LastUser = (this.LastAIUser = CS$<>8__locals1.character);
			bool prevAutoTemp = this.autoTemp;
			bool prevPowerOn = this._powerOn;
			float prevFissionRate = this.TargetFissionRate;
			float prevTurbineOutput = this.TargetTurbineOutput;
			if (shutDown)
			{
				this.PowerOn = false;
				this.TargetFissionRate = 0f;
				this.TargetTurbineOutput = 0f;
				this.unsentChanges = true;
				return true;
			}
			this.PowerOn = true;
			if (objective.Override || !this.autoTemp)
			{
				if (this.degreeOfSuccess < 0.5f)
				{
					this.AutoTemp = true;
				}
				else
				{
					this.AutoTemp = false;
					this.UpdateAutoTemp(MathHelper.Lerp(0.5f, 2f, this.degreeOfSuccess), 1f);
				}
			}
			this.FissionRateScrollBar.BarScroll = this.FissionRate / 100f;
			this.TurbineOutputScrollBar.BarScroll = this.TurbineOutput / 100f;
			if (this.autoTemp != prevAutoTemp || prevPowerOn != this._powerOn || Math.Abs(prevFissionRate - this.TargetFissionRate) > 1f || Math.Abs(prevTurbineOutput - this.TargetTurbineOutput) > 1f)
			{
				this.unsentChanges = true;
			}
			this.aiUpdateTimer = 0.2f;
			return false;
		}

		// Token: 0x06005D2F RID: 23855 RVA: 0x003032F6 File Offset: 0x003014F6
		public override void OnMapLoaded()
		{
			this.prevAvailableFuel = this.AvailableFuel;
		}

		// Token: 0x06005D30 RID: 23856 RVA: 0x00303304 File Offset: 0x00301504
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			string name = connection.Name;
			if (!(name == "shutdown"))
			{
				float newFissionRate;
				if (!(name == "set_fissionrate"))
				{
					if (!(name == "set_turbineoutput"))
					{
						return;
					}
					float newTurbineOutput;
					if (this.PowerOn && float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out newTurbineOutput))
					{
						this.signalControlledTargetTurbineOutput = new float?(MathHelper.Clamp(newTurbineOutput, 0f, 100f));
						this.lastReceivedTurbineOutputSignalTime = Timing.TotalTime;
						this.<ReceiveSignal>g__registerUnsentChanges|200_0();
					}
				}
				else if (this.PowerOn && float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out newFissionRate))
				{
					this.signalControlledTargetFissionRate = new float?(MathHelper.Clamp(newFissionRate, 0f, 100f));
					this.lastReceivedFissionRateSignalTime = Timing.TotalTime;
					this.<ReceiveSignal>g__registerUnsentChanges|200_0();
					return;
				}
			}
			else if (this.TargetFissionRate > 0f || this.TargetTurbineOutput > 0f)
			{
				this.PowerOn = false;
				this.AutoTemp = false;
				this.TargetFissionRate = 0f;
				this.TargetTurbineOutput = 0f;
				this.<ReceiveSignal>g__registerUnsentChanges|200_0();
				return;
			}
		}

		// Token: 0x06005D31 RID: 23857 RVA: 0x00303429 File Offset: 0x00301629
		private float GetMaxOutput()
		{
			return this.item.StatManager.GetAdjustedValueMultiplicative(ItemTalentStats.ReactorMaxOutput, this.MaxPowerOutput);
		}

		// Token: 0x06005D32 RID: 23858 RVA: 0x00303442 File Offset: 0x00301642
		private float GetFuelConsumption()
		{
			return this.item.StatManager.GetAdjustedValueMultiplicative(ItemTalentStats.ReactorFuelConsumption, this.fuelConsumptionRate);
		}

		// Token: 0x06005D34 RID: 23860 RVA: 0x003034B4 File Offset: 0x003016B4
		[CompilerGenerated]
		internal static void <CreateInfrographic>g__DrawToolTip|74_3(string textTag, Anchor anchor, Pivot pivot, GUIComponent targetComponent, ref Reactor.<>c__DisplayClass74_1 A_4)
		{
			GUIComponent.DrawToolTip(A_4.sb, TextManager.Get("infographic.reactor." + textTag), targetComponent.Rect, anchor, pivot);
		}

		// Token: 0x06005D35 RID: 23861 RVA: 0x003034DF File Offset: 0x003016DF
		[CompilerGenerated]
		internal static float <Update>g__adjustValueWithoutOverShooting|185_0(float current, float target, float speed)
		{
			if (target >= current)
			{
				return Math.Min(target, current + speed);
			}
			return Math.Max(target, current - speed);
		}

		// Token: 0x06005D36 RID: 23862 RVA: 0x003034F8 File Offset: 0x003016F8
		[CompilerGenerated]
		private void <ReceiveSignal>g__registerUnsentChanges|200_0()
		{
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember != null && networkMember.IsServer)
			{
				this.unsentChanges = true;
			}
		}

		// Token: 0x04002F84 RID: 12164
		private GUITickBox powerLight;

		// Token: 0x04002F85 RID: 12165
		private GUITickBox autoTempLight;

		// Token: 0x04002F86 RID: 12166
		private const int GraphSize = 25;

		// Token: 0x04002F87 RID: 12167
		private float graphTimer;

		// Token: 0x04002F88 RID: 12168
		private readonly int updateGraphInterval = 500;

		// Token: 0x04002F89 RID: 12169
		private Sprite fissionRateMeter;

		// Token: 0x04002F8A RID: 12170
		private Sprite turbineOutputMeter;

		// Token: 0x04002F8B RID: 12171
		private Sprite meterPointer;

		// Token: 0x04002F8C RID: 12172
		private Sprite sectorSprite;

		// Token: 0x04002F8D RID: 12173
		private Sprite tempMeterFrame;

		// Token: 0x04002F8E RID: 12174
		private Sprite tempMeterBar;

		// Token: 0x04002F8F RID: 12175
		private Sprite tempRangeIndicator;

		// Token: 0x04002F90 RID: 12176
		private Sprite graphLine;

		// Token: 0x04002F91 RID: 12177
		private GUICustomComponent graph;

		// Token: 0x04002F92 RID: 12178
		private GUIFrame inventoryWindow;

		// Token: 0x04002F93 RID: 12179
		private GUILayoutGroup buttonArea;

		// Token: 0x04002F94 RID: 12180
		private GUIFrame infographic;

		// Token: 0x04002F95 RID: 12181
		private Color optimalRangeColor = new Color(74, 238, 104, 255);

		// Token: 0x04002F96 RID: 12182
		private Color offRangeColor = Color.Orange;

		// Token: 0x04002F97 RID: 12183
		private Color warningColor = Color.Red;

		// Token: 0x04002F98 RID: 12184
		private readonly Color[] temperatureColors = new Color[]
		{
			Color.Blue,
			Color.LightBlue,
			Color.Orange,
			Color.Red
		};

		// Token: 0x04002F99 RID: 12185
		private Color outputColor = Color.Goldenrod;

		// Token: 0x04002F9A RID: 12186
		private Color loadColor = Color.LightSteelBlue;

		// Token: 0x04002F9B RID: 12187
		private RoundSound temperatureBoostSoundUp;

		// Token: 0x04002F9C RID: 12188
		private RoundSound temperatureBoostSoundDown;

		// Token: 0x04002F9D RID: 12189
		private GUIButton temperatureBoostUpButton;

		// Token: 0x04002F9E RID: 12190
		private GUIButton temperatureBoostDownButton;

		// Token: 0x04002FA1 RID: 12193
		private readonly float[] outputGraph = new float[25];

		// Token: 0x04002FA2 RID: 12194
		private readonly float[] loadGraph = new float[25];

		// Token: 0x04002FA3 RID: 12195
		private GUITickBox criticalHeatWarning;

		// Token: 0x04002FA4 RID: 12196
		private GUITickBox lowTemperatureWarning;

		// Token: 0x04002FA5 RID: 12197
		private GUITickBox criticalOutputWarning;

		// Token: 0x04002FA6 RID: 12198
		private GUIFrame inventoryContainer;

		// Token: 0x04002FA7 RID: 12199
		private GUILayoutGroup paddedFrame;

		// Token: 0x04002FA8 RID: 12200
		private readonly Dictionary<string, GUIButton> warningButtons = new Dictionary<string, GUIButton>();

		// Token: 0x04002FA9 RID: 12201
		private static readonly string[] warningTexts = new string[]
		{
			"ReactorWarningLowTemp",
			"ReactorWarningLowOutput",
			"ReactorWarningLowFuel",
			"ReactorWarningMeltdown",
			"ReactorWarningOverheating",
			"ReactorWarningHighOutput",
			"ReactorWarningFuelOut",
			"ReactorWarningSCRAM"
		};

		// Token: 0x04002FAB RID: 12203
		private const float NetworkUpdateIntervalHigh = 0.5f;

		// Token: 0x04002FAC RID: 12204
		private const float TemperatureBoostAmount = 25f;

		// Token: 0x04002FAD RID: 12205
		private float fissionRate;

		// Token: 0x04002FAE RID: 12206
		private float turbineOutput;

		// Token: 0x04002FAF RID: 12207
		private float temperature;

		// Token: 0x04002FB0 RID: 12208
		private bool autoTemp;

		// Token: 0x04002FB1 RID: 12209
		private float fuelConsumptionRate;

		// Token: 0x04002FB2 RID: 12210
		private float meltDownTimer;

		// Token: 0x04002FB3 RID: 12211
		private float meltDownDelay;

		// Token: 0x04002FB4 RID: 12212
		private float fireTimer;

		// Token: 0x04002FB5 RID: 12213
		private float fireDelay;

		// Token: 0x04002FB6 RID: 12214
		private float maxPowerOutput;

		// Token: 0x04002FB7 RID: 12215
		private float minUpdatePowerOut;

		// Token: 0x04002FB8 RID: 12216
		private float maxUpdatePowerOut;

		// Token: 0x04002FB9 RID: 12217
		private bool unsentChanges;

		// Token: 0x04002FBA RID: 12218
		private float sendUpdateTimer;

		// Token: 0x04002FBB RID: 12219
		private float degreeOfSuccess;

		// Token: 0x04002FBC RID: 12220
		private Vector2 optimalTemperature;

		// Token: 0x04002FBD RID: 12221
		private Vector2 allowedTemperature;

		// Token: 0x04002FBE RID: 12222
		private Vector2 optimalFissionRate;

		// Token: 0x04002FBF RID: 12223
		private Vector2 allowedFissionRate;

		// Token: 0x04002FC0 RID: 12224
		private Vector2 optimalTurbineOutput;

		// Token: 0x04002FC1 RID: 12225
		private Vector2 allowedTurbineOutput;

		// Token: 0x04002FC2 RID: 12226
		private float? signalControlledTargetFissionRate;

		// Token: 0x04002FC3 RID: 12227
		private float? signalControlledTargetTurbineOutput;

		// Token: 0x04002FC4 RID: 12228
		private double lastReceivedFissionRateSignalTime;

		// Token: 0x04002FC5 RID: 12229
		private double lastReceivedTurbineOutputSignalTime;

		// Token: 0x04002FC6 RID: 12230
		private float temperatureBoost;

		// Token: 0x04002FC7 RID: 12231
		private bool _powerOn;

		// Token: 0x04002FCA RID: 12234
		private Character lastUser;

		// Token: 0x04002FCB RID: 12235
		private float prevAvailableFuel;

		// Token: 0x02001417 RID: 5143
		private enum InfographicArrowStyle
		{
			// Token: 0x0400645C RID: 25692
			Straight,
			// Token: 0x0400645D RID: 25693
			Curved
		}
	}
}
