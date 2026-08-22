using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005DB RID: 1499
	internal class CustomInterface : ItemComponent, IClientSerializable, INetSerializable, IServerSerializable
	{
		// Token: 0x17001870 RID: 6256
		// (get) Token: 0x060060E0 RID: 24800 RVA: 0x00326CC2 File Offset: 0x00324EC2
		private Point ElementMaxSize
		{
			get
			{
				return new Point(this.uiElementContainer.Rect.Width, (int)(65f * GUI.yScale));
			}
		}

		// Token: 0x17001871 RID: 6257
		// (get) Token: 0x060060E1 RID: 24801 RVA: 0x00326CE5 File Offset: 0x00324EE5
		public override bool RecreateGUIOnResolutionChange
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060060E2 RID: 24802 RVA: 0x00326CE8 File Offset: 0x00324EE8
		protected override void CreateGUI()
		{
			this.uiElements.Clear();
			IEnumerable<CustomInterface.CustomInterfaceElement> visibleElements = from ciElement in this.customInterfaceElementList
			where !string.IsNullOrEmpty(ciElement.Label)
			select ciElement;
			this.uiElementContainer = new GUILayoutGroup(new RectTransform(base.GuiFrame.Rect.Size - GUIStyle.ItemFrameMargin, base.GuiFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false)
			{
				AbsoluteOffset = GUIStyle.ItemFrameOffset
			}, false, (this.customInterfaceElementList.Count > 1) ? Anchor.TopCenter : Anchor.Center)
			{
				RelativeSpacing = 0.05f,
				Stretch = (visibleElements.Count<CustomInterface.CustomInterfaceElement>() > 2)
			};
			float elementSize = Math.Min(1f / (float)visibleElements.Count<CustomInterface.CustomInterfaceElement>(), 1f);
			foreach (CustomInterface.CustomInterfaceElement ciElement2 in visibleElements)
			{
				CustomInterface.CustomInterfaceElement.InputTypeOption inputType = ciElement2.InputType;
				bool flag = inputType <= CustomInterface.CustomInterfaceElement.InputTypeOption.Text;
				if (flag)
				{
					GUILayoutGroup layoutGroup = new GUILayoutGroup(new RectTransform(new Vector2(1f, elementSize), this.uiElementContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
					{
						RelativeSpacing = 0.02f,
						UserData = ciElement2
					};
					new GUITextBlock(new RectTransform(new Vector2(0.5f, 1f), layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get(ciElement2.Label).Fallback(ciElement2.Label, true), null, null, Alignment.Left, false, "", null);
					if (ciElement2.InputType == CustomInterface.CustomInterfaceElement.InputTypeOption.Text)
					{
						GUITextBox textBox = new GUITextBox(new RectTransform(new Vector2(0.5f, 1f), layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), ciElement2.Signal, null, null, Alignment.Left, false, "GUITextBoxNoIcon", null, false, true)
						{
							OverflowClip = true,
							UserData = ciElement2,
							MaxTextLength = new int?(ciElement2.MaxTextLength)
						};
						textBox.RectTransform.MinSize = (textBox.Frame.RectTransform.MinSize = new Point(0, 0));
						textBox.RectTransform.MaxSize = (textBox.Frame.RectTransform.MaxSize = new Point(int.MaxValue, int.MaxValue));
						textBox.OnDeselected += delegate(GUITextBox tb, Keys key)
						{
							if (GameMain.Client == null)
							{
								this.TextChanged(tb.UserData as CustomInterface.CustomInterfaceElement, textBox.Text);
								return;
							}
							this.<CreateGUI>g__CreateClientEventWithCorrectionDelay|8_1();
						};
						GUITextBox textBox2 = textBox;
						textBox2.OnEnterPressed = (GUITextBox.OnEnterHandler)Delegate.Combine(textBox2.OnEnterPressed, new GUITextBox.OnEnterHandler(delegate(GUITextBox tb, string text)
						{
							tb.Deselect();
							return true;
						}));
						this.uiElements.Add(textBox);
					}
					else
					{
						GUINumberInput numberInput = null;
						if (ciElement2.NumberType.GetValueOrDefault() == NumberType.Float)
						{
							float floatSignal;
							CustomInterface.TryParseFloatInvariantCulture(ciElement2.Signal, out floatSignal);
							float numberInputMin;
							CustomInterface.TryParseFloatInvariantCulture(ciElement2.NumberInputMin, out numberInputMin);
							float numberInputMax;
							CustomInterface.TryParseFloatInvariantCulture(ciElement2.NumberInputMax, out numberInputMax);
							float numberInputStep;
							CustomInterface.TryParseFloatInvariantCulture(ciElement2.NumberInputStep, out numberInputStep);
							numberInput = new GUINumberInput(new RectTransform(new Vector2(0.5f, 1f), layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), NumberType.Float, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null)
							{
								UserData = ciElement2,
								MinValueFloat = new float?(numberInputMin),
								MaxValueFloat = new float?(numberInputMax),
								FloatValue = Math.Clamp(floatSignal, numberInputMin, numberInputMax),
								DecimalsToDisplay = ciElement2.NumberInputDecimalPlaces,
								ValueStep = numberInputStep,
								OnValueChanged = delegate(GUINumberInput ni)
								{
									this.ValueChanged(ni.UserData as CustomInterface.CustomInterfaceElement, ni.FloatValue);
									if (!this.suppressNetworkEvents && GameMain.Client != null)
									{
										this.<CreateGUI>g__CreateClientEventWithCorrectionDelay|8_1();
									}
								}
							};
						}
						else
						{
							NumberType? numberType = ciElement2.NumberType;
							NumberType numberType2 = NumberType.Int;
							if (numberType.GetValueOrDefault() == numberType2 & numberType != null)
							{
								int intSignal;
								int.TryParse(ciElement2.Signal, out intSignal);
								int numberInputMin2;
								int.TryParse(ciElement2.NumberInputMin, out numberInputMin2);
								int numberInputMax2;
								int.TryParse(ciElement2.NumberInputMax, out numberInputMax2);
								float numberInputStep2;
								CustomInterface.TryParseFloatInvariantCulture(ciElement2.NumberInputStep, out numberInputStep2);
								numberInput = new GUINumberInput(new RectTransform(new Vector2(0.5f, 1f), layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null)
								{
									UserData = ciElement2,
									MinValueInt = new int?(numberInputMin2),
									MaxValueInt = new int?(numberInputMax2),
									IntValue = Math.Clamp(intSignal, numberInputMin2, numberInputMax2),
									ValueStep = numberInputStep2,
									OnValueChanged = delegate(GUINumberInput ni)
									{
										this.ValueChanged(ni.UserData as CustomInterface.CustomInterfaceElement, ni.IntValue);
										if (!this.suppressNetworkEvents && GameMain.Client != null)
										{
											this.<CreateGUI>g__CreateClientEventWithCorrectionDelay|8_1();
										}
									}
								};
							}
							else
							{
								string msg = "Error creating a CustomInterface component: unexpected NumberType \"" + ((ciElement2.NumberType != null) ? ciElement2.NumberType.Value.ToString() : "none") + "\"";
								ContentPackage contentPackage = this.item.Prefab.ContentPackage;
								DebugConsole.LogError(msg, null, contentPackage);
							}
						}
						if (numberInput != null)
						{
							numberInput.RectTransform.MinSize = (numberInput.LayoutGroup.RectTransform.MinSize = new Point(0, 0));
							numberInput.RectTransform.MaxSize = (numberInput.LayoutGroup.RectTransform.MaxSize = new Point(int.MaxValue, int.MaxValue));
							this.uiElements.Add(numberInput);
						}
					}
				}
				else if (ciElement2.InputType == CustomInterface.CustomInterfaceElement.InputTypeOption.TickBox)
				{
					GUITickBox tickBox = new GUITickBox(new RectTransform(new Vector2(1f, elementSize), this.uiElementContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
					{
						MaxSize = this.ElementMaxSize
					}, TextManager.Get(ciElement2.Label).Fallback(ciElement2.Label, true), null, "")
					{
						UserData = ciElement2
					};
					GUITickBox guitickBox = tickBox;
					guitickBox.OnSelected = (GUITickBox.OnSelectedHandler)Delegate.Combine(guitickBox.OnSelected, new GUITickBox.OnSelectedHandler(delegate(GUITickBox tBox)
					{
						this.TickBoxToggled(tBox.UserData as CustomInterface.CustomInterfaceElement, tBox.Selected);
						if (!this.suppressNetworkEvents && GameMain.Client != null)
						{
							this.<CreateGUI>g__CreateClientEventWithCorrectionDelay|8_1();
						}
						return true;
					}));
					tickBox.RectTransform.MinSize = new Point(0, 0);
					tickBox.RectTransform.MaxSize = new Point(int.MaxValue, int.MaxValue);
					this.uiElements.Add(tickBox);
				}
				else if (ciElement2.InputType == CustomInterface.CustomInterfaceElement.InputTypeOption.Button)
				{
					GUIButton btn = new GUIButton(new RectTransform(new Vector2(1f, elementSize), this.uiElementContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get(ciElement2.Label).Fallback(ciElement2.Label, true), Alignment.Center, "DeviceButton", null)
					{
						UserData = ciElement2
					};
					GUIButton guibutton = btn;
					guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton _, object userdata)
					{
						CustomInterface.CustomInterfaceElement btnElement = userdata as CustomInterface.CustomInterfaceElement;
						if (GameMain.Client == null)
						{
							this.ButtonClicked(btnElement);
						}
						else if (!this.suppressNetworkEvents && GameMain.Client != null)
						{
							this.item.CreateClientEvent<CustomInterface>(this, new CustomInterface.EventData(btnElement));
						}
						return true;
					}));
					btn.RectTransform.MinSize = (btn.Frame.RectTransform.MinSize = new Point(0, 0));
					btn.RectTransform.MaxSize = (btn.Frame.RectTransform.MaxSize = this.ElementMaxSize);
					this.uiElements.Add(btn);
				}
			}
			if (this.ShowInsufficientPowerWarning)
			{
				RectTransform rectTransform = new RectTransform(new Vector2(1f, 0.1f), base.GuiFrame.RectTransform, Anchor.BottomCenter, new Pivot?(Pivot.TopCenter), null, null, ScaleBasis.Normal);
				rectTransform.MinSize = new Point(0, GUI.IntScale(30f));
				RichString text2 = TextManager.Get("SteeringNoPowerTip");
				GUIFont font = GUIStyle.Font;
				this.insufficientPowerWarning = new GUITextBlock(rectTransform, text2, null, font, Alignment.Center, true, "GUIToolTip", null)
				{
					AutoScaleHorizontal = true,
					Visible = false
				};
			}
		}

		// Token: 0x060060E3 RID: 24803 RVA: 0x00327614 File Offset: 0x00325814
		public override void CreateEditingHUD(SerializableEntityEditor editor)
		{
			base.CreateEditingHUD(editor);
			if (this.customInterfaceElementList.Count > 0)
			{
				PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(this.customInterfaceElementList[0]);
				PropertyDescriptor labelProperty = properties.Find("Label", false);
				PropertyDescriptor signalProperty = properties.Find("Signal", false);
				for (int i = 0; i < this.customInterfaceElementList.Count; i++)
				{
					editor.CreateStringField(this.customInterfaceElementList[i], new SerializableProperty(labelProperty), this.customInterfaceElementList[i].Label, "Label #" + (i + 1).ToString(), "");
					editor.CreateStringField(this.customInterfaceElementList[i], new SerializableProperty(signalProperty), this.customInterfaceElementList[i].Signal, "Signal #" + (i + 1).ToString(), "");
				}
			}
		}

		// Token: 0x060060E4 RID: 24804 RVA: 0x00327720 File Offset: 0x00325920
		public void HighlightElement(int index, Color color, float duration, float pulsateAmount = 0f)
		{
			if (index < 0 || index >= this.uiElements.Count)
			{
				return;
			}
			this.uiElements[index].Flash(new Color?(color), duration, false, false, null);
			if (pulsateAmount > 0f)
			{
				GUIButton button = this.uiElements[index] as GUIButton;
				if (button != null)
				{
					button.Frame.Pulsate(Vector2.One, Vector2.One * (1f + pulsateAmount), duration);
					button.Frame.RectTransform.SetPosition(Anchor.Center, null);
					return;
				}
				this.uiElements[index].Pulsate(Vector2.One, Vector2.One * (1f + pulsateAmount), duration);
			}
		}

		// Token: 0x060060E5 RID: 24805 RVA: 0x003277E8 File Offset: 0x003259E8
		public override void UpdateHUDComponentSpecific(Character character, float deltaTime, Camera cam)
		{
			bool elementVisibilityChanged = false;
			int visibleElementCount = 0;
			foreach (GUIComponent uiElement in this.uiElements)
			{
				CustomInterface.CustomInterfaceElement element = uiElement.UserData as CustomInterface.CustomInterfaceElement;
				if (element != null)
				{
					bool visible = Screen.Selected == GameMain.SubEditorScreen || element.StatusEffects.Any<StatusEffect>() || element.HasPropertyName || (element.Connection != null && element.Connection.Wires.Count > 0);
					if (visible)
					{
						visibleElementCount++;
						if (element.GetValueInterval > 0f && this.correctionTimer <= 0f)
						{
							element.GetValueTimer -= deltaTime;
							if (element.GetValueTimer <= 0f)
							{
								this.SetSignalToPropertyValue(element);
								this.UpdateSignalProjSpecific(uiElement);
								element.GetValueTimer = element.GetValueInterval;
							}
						}
					}
					if (uiElement.Visible != visible)
					{
						uiElement.Visible = visible;
						uiElement.IgnoreLayoutGroups = !uiElement.Visible;
						elementVisibilityChanged = true;
					}
				}
			}
			if (elementVisibilityChanged)
			{
				this.uiElementContainer.Stretch = (visibleElementCount > 2);
				this.uiElementContainer.ChildAnchor = ((visibleElementCount > 1) ? Anchor.TopCenter : Anchor.Center);
				float elementSize = Math.Min(1f / (float)visibleElementCount, 1f);
				foreach (GUIComponent uiElement2 in this.uiElements)
				{
					uiElement2.RectTransform.RelativeSize = new Vector2(1f, elementSize);
				}
				base.GuiFrame.Visible = (visibleElementCount > 0);
				this.uiElementContainer.Recalculate();
			}
			if (this.insufficientPowerWarning != null)
			{
				this.insufficientPowerWarning.Visible = this.item.GetComponents<Powered>().Any((Powered p) => p.PowerConsumption > 0f && p.Voltage < p.MinVoltage);
			}
		}

		// Token: 0x060060E6 RID: 24806 RVA: 0x00327A08 File Offset: 0x00325C08
		private void UpdateSignalProjSpecific(GUIComponent uiElement)
		{
			CustomInterface.CustomInterfaceElement element = uiElement.UserData as CustomInterface.CustomInterfaceElement;
			if (element == null)
			{
				return;
			}
			this.suppressNetworkEvents = true;
			string signal = element.Signal;
			GUITextBox tb = uiElement as GUITextBox;
			if (tb != null)
			{
				GUITextBox guitextBox = tb;
				Screen selected = Screen.Selected;
				guitextBox.Text = ((selected != null && selected.IsEditor) ? signal : TextManager.Get(signal).Fallback(signal, true).Value);
			}
			else
			{
				GUINumberInput ni = uiElement as GUINumberInput;
				if (ni != null)
				{
					if (ni.InputType == NumberType.Int)
					{
						int value;
						float floatValue;
						if (int.TryParse(signal, out value))
						{
							ni.IntValue = value;
						}
						else if (float.TryParse(signal, out floatValue))
						{
							ni.IntValue = (int)MathF.Round(floatValue);
						}
					}
				}
				else
				{
					GUITickBox tickBox = uiElement as GUITickBox;
					if (tickBox != null)
					{
						tickBox.Selected = signal.Equals("true", StringComparison.OrdinalIgnoreCase);
					}
				}
			}
			this.suppressNetworkEvents = false;
		}

		// Token: 0x060060E7 RID: 24807 RVA: 0x00327ADC File Offset: 0x00325CDC
		public void ClientEventWrite(IWriteMessage msg, NetEntityEvent.IData extraData = null)
		{
			for (int i = 0; i < this.customInterfaceElementList.Count; i++)
			{
				CustomInterface.CustomInterfaceElement element = this.customInterfaceElementList[i];
				switch (element.InputType)
				{
				case CustomInterface.CustomInterfaceElement.InputTypeOption.Number:
				{
					NumberType? numberType = element.NumberType;
					if (numberType != null)
					{
						NumberType valueOrDefault = numberType.GetValueOrDefault();
						if (valueOrDefault != NumberType.Int && valueOrDefault == NumberType.Float)
						{
							msg.WriteString(((GUINumberInput)this.uiElements[i]).FloatValue.ToString());
							break;
						}
					}
					msg.WriteString(((GUINumberInput)this.uiElements[i]).IntValue.ToString());
					break;
				}
				case CustomInterface.CustomInterfaceElement.InputTypeOption.Text:
					msg.WriteString(((GUITextBox)this.uiElements[i]).Text);
					break;
				case CustomInterface.CustomInterfaceElement.InputTypeOption.Button:
				{
					if (!(extraData is Item.ComponentStateEventData))
					{
						goto IL_123;
					}
					ItemComponent.IEventData componentData = ((Item.ComponentStateEventData)extraData).ComponentData;
					if (!(componentData is CustomInterface.EventData))
					{
						goto IL_123;
					}
					CustomInterface.EventData eventData = (CustomInterface.EventData)componentData;
					bool val = eventData.BtnElement == this.customInterfaceElementList[i];
					IL_124:
					msg.WriteBoolean(val);
					break;
					IL_123:
					val = false;
					goto IL_124;
				}
				case CustomInterface.CustomInterfaceElement.InputTypeOption.TickBox:
					msg.WriteBoolean(((GUITickBox)this.uiElements[i]).Selected);
					break;
				}
			}
		}

		// Token: 0x060060E8 RID: 24808 RVA: 0x00327C28 File Offset: 0x00325E28
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			int msgStartPos = msg.BitPosition;
			this.suppressNetworkEvents = true;
			try
			{
				string[] stringValues = new string[this.customInterfaceElementList.Count];
				bool[] boolValues = new bool[this.customInterfaceElementList.Count];
				for (int i = 0; i < this.customInterfaceElementList.Count; i++)
				{
					CustomInterface.CustomInterfaceElement element = this.customInterfaceElementList[i];
					CustomInterface.CustomInterfaceElement.InputTypeOption inputType = element.InputType;
					if (inputType > CustomInterface.CustomInterfaceElement.InputTypeOption.Text)
					{
						if (inputType - CustomInterface.CustomInterfaceElement.InputTypeOption.Button <= 1)
						{
							boolValues[i] = msg.ReadBoolean();
						}
					}
					else
					{
						stringValues[i] = msg.ReadString();
					}
				}
				if (this.correctionTimer > 0f)
				{
					int msgLength = msg.BitPosition - msgStartPos;
					msg.BitPosition = msgStartPos;
					base.StartDelayedCorrection(msg.ExtractBits(msgLength), sendingTime, false);
				}
				else
				{
					for (int j = 0; j < this.customInterfaceElementList.Count; j++)
					{
						CustomInterface.CustomInterfaceElement element2 = this.customInterfaceElementList[j];
						switch (element2.InputType)
						{
						case CustomInterface.CustomInterfaceElement.InputTypeOption.Number:
						{
							NumberType? numberType = element2.NumberType;
							if (numberType != null)
							{
								NumberType valueOrDefault = numberType.GetValueOrDefault();
								int value2;
								if (valueOrDefault != NumberType.Int)
								{
									if (valueOrDefault == NumberType.Float)
									{
										float value;
										if (CustomInterface.TryParseFloatInvariantCulture(stringValues[j], out value))
										{
											this.ValueChanged(element2, value);
										}
									}
								}
								else if (int.TryParse(stringValues[j], out value2))
								{
									this.ValueChanged(element2, value2);
								}
							}
							break;
						}
						case CustomInterface.CustomInterfaceElement.InputTypeOption.Text:
							this.TextChanged(element2, stringValues[j]);
							break;
						case CustomInterface.CustomInterfaceElement.InputTypeOption.Button:
							if (boolValues[j])
							{
								this.ButtonClicked(element2);
							}
							break;
						case CustomInterface.CustomInterfaceElement.InputTypeOption.TickBox:
						{
							bool tickBoxState = boolValues[j];
							((GUITickBox)this.uiElements[j]).Selected = tickBoxState;
							this.TickBoxToggled(element2, tickBoxState);
							break;
						}
						}
					}
					this.UpdateSignalsProjSpecific();
				}
			}
			finally
			{
				this.suppressNetworkEvents = false;
			}
		}

		// Token: 0x17001872 RID: 6258
		// (get) Token: 0x060060E9 RID: 24809 RVA: 0x00327E0C File Offset: 0x0032600C
		// (set) Token: 0x060060EA RID: 24810 RVA: 0x00327E20 File Offset: 0x00326020
		[Serialize("", IsPropertySaveable.Yes, "The texts displayed on the buttons/tickboxes, separated by commas.", "", true)]
		public string Labels
		{
			get
			{
				return string.Join(",", this.labels);
			}
			set
			{
				if (value == null)
				{
					return;
				}
				if (this.customInterfaceElementList.Count > 0)
				{
					string[] splitValues = (value == "") ? Array.Empty<string>() : value.Split(',', StringSplitOptions.None);
					this.UpdateLabels(splitValues);
				}
			}
		}

		// Token: 0x17001873 RID: 6259
		// (get) Token: 0x060060EB RID: 24811 RVA: 0x00327E64 File Offset: 0x00326064
		// (set) Token: 0x060060EC RID: 24812 RVA: 0x00327E84 File Offset: 0x00326084
		[Serialize("", IsPropertySaveable.Yes, "The signals sent when the buttons are pressed or the tickboxes checked, separated by commas.", "", true)]
		public string Signals
		{
			get
			{
				if (this.signals != null)
				{
					return string.Join(";", this.signals);
				}
				return string.Empty;
			}
			set
			{
				if (value == null)
				{
					return;
				}
				if (this.customInterfaceElementList.Count > 0)
				{
					string[] splitValues = (value == "") ? Array.Empty<string>() : value.Split(';', StringSplitOptions.None);
					this.UpdateSignals(splitValues);
				}
			}
		}

		// Token: 0x17001874 RID: 6260
		// (get) Token: 0x060060ED RID: 24813 RVA: 0x00327EC8 File Offset: 0x003260C8
		// (set) Token: 0x060060EE RID: 24814 RVA: 0x00327EE8 File Offset: 0x003260E8
		[Serialize("", IsPropertySaveable.Yes, "", "", true)]
		public string ElementStates
		{
			get
			{
				if (this.elementStates != null)
				{
					return string.Join<bool>(",", this.elementStates);
				}
				return string.Empty;
			}
			set
			{
				if (value == null)
				{
					return;
				}
				if (this.customInterfaceElementList.Count > 0)
				{
					string[] splitValues = (value == "") ? Array.Empty<string>() : value.Split(',', StringSplitOptions.None);
					int i = 0;
					while (i < this.customInterfaceElementList.Count && i < splitValues.Length)
					{
						bool val;
						if (bool.TryParse(splitValues[i], out val))
						{
							this.customInterfaceElementList[i].State = val;
							if (this.uiElements != null && i < this.uiElements.Count)
							{
								GUITickBox tickBox = this.uiElements[i] as GUITickBox;
								if (tickBox != null)
								{
									tickBox.Selected = val;
								}
							}
						}
						i++;
					}
				}
			}
		}

		// Token: 0x17001875 RID: 6261
		// (get) Token: 0x060060EF RID: 24815 RVA: 0x00327F94 File Offset: 0x00326194
		// (set) Token: 0x060060F0 RID: 24816 RVA: 0x00327F9C File Offset: 0x0032619C
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool ShowInsufficientPowerWarning { get; set; }

		// Token: 0x060060F1 RID: 24817 RVA: 0x00327FA8 File Offset: 0x003261A8
		public CustomInterface(Item item, ContentXElement element) : base(item, element)
		{
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				CustomInterface.CustomInterfaceElement.InputTypeOption inputType;
				bool continuousSignalByDefault;
				if (!(a == "button"))
				{
					if (!(a == "textbox"))
					{
						if (!(a == "integerinput") && !(a == "numberinput"))
						{
							if (!(a == "tickbox"))
							{
								continue;
							}
							inputType = CustomInterface.CustomInterfaceElement.InputTypeOption.TickBox;
							continuousSignalByDefault = true;
						}
						else
						{
							inputType = CustomInterface.CustomInterfaceElement.InputTypeOption.Number;
							continuousSignalByDefault = false;
						}
					}
					else
					{
						inputType = CustomInterface.CustomInterfaceElement.InputTypeOption.Text;
						continuousSignalByDefault = false;
					}
				}
				else
				{
					inputType = CustomInterface.CustomInterfaceElement.InputTypeOption.Button;
					continuousSignalByDefault = false;
				}
				CustomInterface.CustomInterfaceElement ciElement = new CustomInterface.CustomInterfaceElement(item, subElement, this, inputType)
				{
					ContinuousSignal = subElement.GetAttributeBool("ContinuousSignal", continuousSignalByDefault)
				};
				if (string.IsNullOrEmpty(ciElement.Label))
				{
					ciElement.Label = "Signal out " + this.customInterfaceElementList.Count((CustomInterface.CustomInterfaceElement e) => e.ContinuousSignal == ciElement.ContinuousSignal).ToString();
				}
				this.customInterfaceElementList.Add(ciElement);
				this.IsActive |= (ciElement.ContinuousSignal || ciElement.GetValueInterval > 0f);
			}
			this.InitProjSpecific();
			this.Labels = element.GetAttributeString("labels", "");
			this.Signals = element.GetAttributeString("signals", "");
			this.ElementStates = element.GetAttributeString("elementstates", "");
		}

		// Token: 0x060060F2 RID: 24818 RVA: 0x00328194 File Offset: 0x00326394
		private void UpdateLabels(string[] newLabels)
		{
			this.labels = new string[this.customInterfaceElementList.Count];
			for (int i = 0; i < this.labels.Length; i++)
			{
				this.labels[i] = ((i < newLabels.Length) ? newLabels[i] : this.customInterfaceElementList[i].Label);
				this.customInterfaceElementList[i].Label = this.labels[i];
			}
			this.UpdateLabelsProjSpecific();
		}

		// Token: 0x060060F3 RID: 24819 RVA: 0x00328210 File Offset: 0x00326410
		private void UpdateSignals(string[] newSignals)
		{
			this.signals = new string[this.customInterfaceElementList.Count];
			for (int i = 0; i < this.customInterfaceElementList.Count; i++)
			{
				CustomInterface.CustomInterfaceElement element = this.customInterfaceElementList[i];
				if (i < newSignals.Length)
				{
					string newSignal = newSignals[i];
					this.signals[i] = newSignal;
					element.ShouldSetProperty = (element.Signal != newSignal);
					element.Signal = newSignal;
				}
				else
				{
					this.signals[i] = element.Signal;
				}
				if (element.HasPropertyName && element.ShouldSetProperty)
				{
					this.SetPropertyValueToSignal(element);
					this.customInterfaceElementList[i].ShouldSetProperty = false;
				}
			}
			this.UpdateSignalsProjSpecific();
		}

		// Token: 0x060060F4 RID: 24820 RVA: 0x003282C4 File Offset: 0x003264C4
		private void SetPropertyValueToSignal(CustomInterface.CustomInterfaceElement element)
		{
			if (element.TargetOnlyParentProperty)
			{
				if (base.SerializableProperties.ContainsKey(element.PropertyName))
				{
					base.SerializableProperties[element.PropertyName].TrySetValue(this, element.Signal);
					return;
				}
			}
			else
			{
				foreach (ISerializableEntity po in this.item.AllPropertyObjects)
				{
					if (po.SerializableProperties.ContainsKey(element.PropertyName))
					{
						Identifier targetItemComponent = element.TargetItemComponent;
						if (!targetItemComponent.IsEmpty)
						{
							string name = po.Name;
							targetItemComponent = element.TargetItemComponent;
							if (name != targetItemComponent)
							{
								continue;
							}
						}
						po.SerializableProperties[element.PropertyName].TrySetValue(po, element.Signal);
					}
				}
			}
		}

		// Token: 0x060060F5 RID: 24821 RVA: 0x003283A4 File Offset: 0x003265A4
		private void SetSignalToPropertyValue(CustomInterface.CustomInterfaceElement element)
		{
			if (element.TargetOnlyParentProperty)
			{
				if (base.SerializableProperties.ContainsKey(element.PropertyName))
				{
					object value = base.SerializableProperties[element.PropertyName].GetValue(this);
					element.Signal = ((value != null) ? value.ToString() : null);
					return;
				}
			}
			else
			{
				foreach (ISerializableEntity e in this.item.AllPropertyObjects)
				{
					if (e.SerializableProperties.ContainsKey(element.PropertyName))
					{
						Identifier targetItemComponent = element.TargetItemComponent;
						if (!targetItemComponent.IsEmpty)
						{
							string name = e.Name;
							targetItemComponent = element.TargetItemComponent;
							if (name != targetItemComponent)
							{
								continue;
							}
						}
						object value2 = e.SerializableProperties[element.PropertyName].GetValue(e);
						element.Signal = ((value2 != null) ? value2.ToString() : null);
						break;
					}
				}
			}
		}

		// Token: 0x060060F6 RID: 24822 RVA: 0x0032849C File Offset: 0x0032669C
		public override void OnItemLoaded()
		{
			using (List<CustomInterface.CustomInterfaceElement>.Enumerator enumerator = this.customInterfaceElementList.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					CustomInterface.CustomInterfaceElement ciElement = enumerator.Current;
					CustomInterface.CustomInterfaceElement ciElement2 = ciElement;
					List<Connection> connections = this.item.Connections;
					ciElement2.Connection = ((connections != null) ? connections.FirstOrDefault((Connection c) => c.Name == ciElement.ConnectionName) : null);
				}
			}
		}

		// Token: 0x060060F7 RID: 24823 RVA: 0x00328524 File Offset: 0x00326724
		private void UpdateLabelsProjSpecific()
		{
			int i = 0;
			while (i < this.labels.Length && i < this.uiElements.Count)
			{
				GUIButton button = this.uiElements[i] as GUIButton;
				if (button != null)
				{
					button.Text = this.<UpdateLabelsProjSpecific>g__CreateLabelText|40_0(i);
					button.TextBlock.Wrap = button.Text.Contains(' ', StringComparison.Ordinal);
				}
				else
				{
					GUITickBox tickBox = this.uiElements[i] as GUITickBox;
					if (tickBox != null)
					{
						tickBox.Text = this.<UpdateLabelsProjSpecific>g__CreateLabelText|40_0(i);
						tickBox.TextBlock.Wrap = tickBox.Text.Contains(' ', StringComparison.Ordinal);
					}
					else if (this.uiElements[i] is GUITextBox || this.uiElements[i] is GUINumberInput)
					{
						GUITextBlock textBlock = this.uiElements[i].Parent.GetChild<GUITextBlock>();
						textBlock.Text = this.<UpdateLabelsProjSpecific>g__CreateLabelText|40_0(i);
						textBlock.Wrap = textBlock.Text.Contains(' ', StringComparison.Ordinal);
					}
				}
				i++;
			}
			this.uiElementContainer.Recalculate();
			List<GUITextBlock> textBlocks = new List<GUITextBlock>();
			foreach (GUIComponent element in this.uiElementContainer.Children)
			{
				GUIButton btn = element as GUIButton;
				if (btn != null)
				{
					if (btn.TextBlock.TextSize.Y > (float)btn.Rect.Height - btn.TextBlock.Padding.Y - btn.TextBlock.Padding.W)
					{
						btn.RectTransform.RelativeSize = new Vector2(btn.RectTransform.RelativeSize.X, btn.RectTransform.RelativeSize.Y * 1.5f);
					}
					textBlocks.Add(btn.TextBlock);
				}
				else
				{
					GUITickBox tickBox2 = element as GUITickBox;
					if (tickBox2 != null)
					{
						textBlocks.Add(tickBox2.TextBlock);
					}
					else if (element is GUILayoutGroup)
					{
						textBlocks.Add(element.GetChild<GUITextBlock>());
					}
				}
			}
			this.uiElementContainer.Recalculate();
			GUITextBlock.AutoScaleAndNormalize(textBlocks, true, false, null);
		}

		// Token: 0x060060F8 RID: 24824 RVA: 0x00328784 File Offset: 0x00326984
		private void UpdateSignalsProjSpecific()
		{
			if (this.signals == null)
			{
				return;
			}
			int i = 0;
			while (i < this.signals.Length && i < this.uiElements.Count)
			{
				this.UpdateSignalProjSpecific(this.uiElements[i]);
				i++;
			}
		}

		// Token: 0x060060F9 RID: 24825 RVA: 0x003287CD File Offset: 0x003269CD
		private void InitProjSpecific()
		{
			this.CreateGUI();
		}

		// Token: 0x060060FA RID: 24826 RVA: 0x003287D8 File Offset: 0x003269D8
		private void ButtonClicked(CustomInterface.CustomInterfaceElement btnElement)
		{
			if (btnElement == null)
			{
				return;
			}
			if (btnElement.Connection != null)
			{
				this.item.SendSignal(new Signal(btnElement.Signal, 0, null, this.item, 0f, 1f), btnElement.Connection);
			}
			foreach (StatusEffect effect in btnElement.StatusEffects)
			{
				Item item = this.item;
				StatusEffect effect2 = effect;
				ActionType type = ActionType.OnUse;
				float deltaTime = 1f;
				Inventory parentInventory = this.item.ParentInventory;
				item.ApplyStatusEffect(effect2, type, deltaTime, ((parentInventory != null) ? parentInventory.Owner : null) as Character, null, null, false, true, null);
			}
		}

		// Token: 0x060060FB RID: 24827 RVA: 0x0032889C File Offset: 0x00326A9C
		private void TickBoxToggled(CustomInterface.CustomInterfaceElement tickBoxElement, bool state)
		{
			if (tickBoxElement == null)
			{
				return;
			}
			tickBoxElement.State = state;
			tickBoxElement.Signal = state.ToString();
			if (!tickBoxElement.ContinuousSignal)
			{
				this.SetPropertyValueToSignal(tickBoxElement);
			}
		}

		// Token: 0x060060FC RID: 24828 RVA: 0x003288C5 File Offset: 0x00326AC5
		private void TextChanged(CustomInterface.CustomInterfaceElement textElement, string text)
		{
			if (textElement == null)
			{
				return;
			}
			textElement.Signal = text;
			this.SetPropertyValueToSignal(textElement);
		}

		// Token: 0x060060FD RID: 24829 RVA: 0x003288DC File Offset: 0x00326ADC
		private void ValueChanged(CustomInterface.CustomInterfaceElement numberInputElement, int value)
		{
			if (numberInputElement == null)
			{
				return;
			}
			numberInputElement.Signal = value.ToString();
			this.SetPropertyValueToSignal(numberInputElement);
			foreach (StatusEffect effect in numberInputElement.StatusEffects)
			{
				Item item = this.item;
				StatusEffect effect2 = effect;
				ActionType type = ActionType.OnUse;
				float deltaTime = 1f;
				Inventory parentInventory = this.item.ParentInventory;
				item.ApplyStatusEffect(effect2, type, deltaTime, ((parentInventory != null) ? parentInventory.Owner : null) as Character, null, null, false, true, null);
			}
		}

		// Token: 0x060060FE RID: 24830 RVA: 0x0032897C File Offset: 0x00326B7C
		private void ValueChanged(CustomInterface.CustomInterfaceElement numberInputElement, float value)
		{
			if (numberInputElement == null)
			{
				return;
			}
			numberInputElement.Signal = value.ToString();
			this.SetPropertyValueToSignal(numberInputElement);
		}

		// Token: 0x060060FF RID: 24831 RVA: 0x00328998 File Offset: 0x00326B98
		public override void Update(float deltaTime, Camera cam)
		{
			foreach (CustomInterface.CustomInterfaceElement ciElement in this.customInterfaceElementList)
			{
				if (ciElement.GetValueInterval > 0f)
				{
					ciElement.GetValueTimer -= deltaTime;
					if (ciElement.GetValueTimer <= 0f)
					{
						this.SetSignalToPropertyValue(ciElement);
						ciElement.GetValueTimer = ciElement.GetValueInterval;
					}
				}
				if (!ciElement.ContinuousSignal)
				{
					Identifier propertyName = ciElement.PropertyName;
					if (propertyName != "Voltage")
					{
						continue;
					}
				}
				if (!string.IsNullOrEmpty(ciElement.Signal) && ciElement.Connection != null)
				{
					this.item.SendSignal(new Signal(ciElement.State ? ciElement.Signal : "0", 0, null, this.item, 0f, 1f), ciElement.Connection);
				}
				foreach (StatusEffect effect in ciElement.StatusEffects)
				{
					this.item.ApplyStatusEffect(effect, ciElement.State ? ActionType.OnUse : ActionType.OnSecondaryUse, 1f, null, null, null, true, false, null);
				}
			}
		}

		// Token: 0x06006100 RID: 24832 RVA: 0x00328B18 File Offset: 0x00326D18
		public override void UpdateBroken(float deltaTime, Camera cam)
		{
			this.Update(deltaTime, cam);
		}

		// Token: 0x06006101 RID: 24833 RVA: 0x00328B24 File Offset: 0x00326D24
		public override XElement Save(XElement parentElement)
		{
			this.labels = (from ci in this.customInterfaceElementList
			select ci.Label).ToArray<string>();
			this.signals = (from ci in this.customInterfaceElementList
			select ci.Signal).ToArray<string>();
			this.elementStates = (from ci in this.customInterfaceElementList
			select ci.State).ToArray<bool>();
			return base.Save(parentElement);
		}

		// Token: 0x06006102 RID: 24834 RVA: 0x00328BD7 File Offset: 0x00326DD7
		private static bool TryParseFloatInvariantCulture(string s, out float f)
		{
			return float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out f);
		}

		// Token: 0x06006107 RID: 24839 RVA: 0x00328CC3 File Offset: 0x00326EC3
		[CompilerGenerated]
		private void <CreateGUI>g__CreateClientEventWithCorrectionDelay|8_1()
		{
			this.item.CreateClientEvent<CustomInterface>(this);
			this.correctionTimer = 1f;
		}

		// Token: 0x06006108 RID: 24840 RVA: 0x00328CDC File Offset: 0x00326EDC
		[CompilerGenerated]
		private LocalizedString <UpdateLabelsProjSpecific>g__CreateLabelText|40_0(int elementIndex)
		{
			string label = this.customInterfaceElementList[elementIndex].Label;
			if (!string.IsNullOrWhiteSpace(label))
			{
				return TextManager.Get(label).Fallback(label, true);
			}
			return TextManager.GetWithVariable("connection.signaloutx", "[num]", (elementIndex + 1).ToString(), FormatCapitals.No);
		}

		// Token: 0x04003204 RID: 12804
		private readonly List<GUIComponent> uiElements = new List<GUIComponent>();

		// Token: 0x04003205 RID: 12805
		private GUILayoutGroup uiElementContainer;

		// Token: 0x04003206 RID: 12806
		private bool suppressNetworkEvents;

		// Token: 0x04003207 RID: 12807
		private GUIComponent insufficientPowerWarning;

		// Token: 0x04003208 RID: 12808
		private string[] labels;

		// Token: 0x04003209 RID: 12809
		private string[] signals;

		// Token: 0x0400320A RID: 12810
		private bool[] elementStates;

		// Token: 0x0400320C RID: 12812
		private readonly List<CustomInterface.CustomInterfaceElement> customInterfaceElementList = new List<CustomInterface.CustomInterfaceElement>();

		// Token: 0x0200146D RID: 5229
		private readonly struct EventData : ItemComponent.IEventData
		{
			// Token: 0x06009AD5 RID: 39637 RVA: 0x003E3B4A File Offset: 0x003E1D4A
			public EventData(CustomInterface.CustomInterfaceElement btnElement)
			{
				this.BtnElement = btnElement;
			}

			// Token: 0x040065A3 RID: 26019
			public readonly CustomInterface.CustomInterfaceElement BtnElement;
		}

		// Token: 0x0200146E RID: 5230
		private class CustomInterfaceElement : ISerializableEntity
		{
			// Token: 0x17001D53 RID: 7507
			// (get) Token: 0x06009AD6 RID: 39638 RVA: 0x003E3B53 File Offset: 0x003E1D53
			// (set) Token: 0x06009AD7 RID: 39639 RVA: 0x003E3B5B File Offset: 0x003E1D5B
			[Serialize("", IsPropertySaveable.No, "The text displayed on this button/tickbox.", "Label.", false)]
			[Editable]
			public string Label { get; set; }

			// Token: 0x17001D54 RID: 7508
			// (get) Token: 0x06009AD8 RID: 39640 RVA: 0x003E3B64 File Offset: 0x003E1D64
			// (set) Token: 0x06009AD9 RID: 39641 RVA: 0x003E3B6C File Offset: 0x003E1D6C
			[Serialize("1", IsPropertySaveable.No, "The signal sent out when this button is pressed or this tickbox checked.", "", false)]
			[Editable]
			public string Signal { get; set; }

			// Token: 0x17001D55 RID: 7509
			// (get) Token: 0x06009ADA RID: 39642 RVA: 0x003E3B75 File Offset: 0x003E1D75
			public Identifier PropertyName { get; }

			// Token: 0x17001D56 RID: 7510
			// (get) Token: 0x06009ADB RID: 39643 RVA: 0x003E3B7D File Offset: 0x003E1D7D
			public Identifier TargetItemComponent { get; }

			// Token: 0x17001D57 RID: 7511
			// (get) Token: 0x06009ADC RID: 39644 RVA: 0x003E3B85 File Offset: 0x003E1D85
			public bool TargetOnlyParentProperty { get; }

			// Token: 0x17001D58 RID: 7512
			// (get) Token: 0x06009ADD RID: 39645 RVA: 0x003E3B8D File Offset: 0x003E1D8D
			public string NumberInputMin { get; }

			// Token: 0x17001D59 RID: 7513
			// (get) Token: 0x06009ADE RID: 39646 RVA: 0x003E3B95 File Offset: 0x003E1D95
			public string NumberInputMax { get; }

			// Token: 0x17001D5A RID: 7514
			// (get) Token: 0x06009ADF RID: 39647 RVA: 0x003E3B9D File Offset: 0x003E1D9D
			public string NumberInputStep { get; }

			// Token: 0x17001D5B RID: 7515
			// (get) Token: 0x06009AE0 RID: 39648 RVA: 0x003E3BA5 File Offset: 0x003E1DA5
			public int NumberInputDecimalPlaces { get; }

			// Token: 0x17001D5C RID: 7516
			// (get) Token: 0x06009AE1 RID: 39649 RVA: 0x003E3BAD File Offset: 0x003E1DAD
			public int MaxTextLength { get; }

			// Token: 0x17001D5D RID: 7517
			// (get) Token: 0x06009AE2 RID: 39650 RVA: 0x003E3BB5 File Offset: 0x003E1DB5
			public CustomInterface.CustomInterfaceElement.InputTypeOption InputType { get; }

			// Token: 0x17001D5E RID: 7518
			// (get) Token: 0x06009AE3 RID: 39651 RVA: 0x003E3BBD File Offset: 0x003E1DBD
			public NumberType? NumberType { get; }

			// Token: 0x17001D5F RID: 7519
			// (get) Token: 0x06009AE4 RID: 39652 RVA: 0x003E3BC5 File Offset: 0x003E1DC5
			public bool HasPropertyName { get; }

			// Token: 0x17001D60 RID: 7520
			// (get) Token: 0x06009AE5 RID: 39653 RVA: 0x003E3BCD File Offset: 0x003E1DCD
			// (set) Token: 0x06009AE6 RID: 39654 RVA: 0x003E3BD5 File Offset: 0x003E1DD5
			public bool ShouldSetProperty { get; set; }

			// Token: 0x17001D61 RID: 7521
			// (get) Token: 0x06009AE7 RID: 39655 RVA: 0x003E3BDE File Offset: 0x003E1DDE
			// (set) Token: 0x06009AE8 RID: 39656 RVA: 0x003E3BE6 File Offset: 0x003E1DE6
			public float GetValueInterval { get; set; } = -1f;

			// Token: 0x17001D62 RID: 7522
			// (get) Token: 0x06009AE9 RID: 39657 RVA: 0x003E3BEF File Offset: 0x003E1DEF
			public string Name
			{
				get
				{
					return "CustomInterfaceElement";
				}
			}

			// Token: 0x17001D63 RID: 7523
			// (get) Token: 0x06009AEA RID: 39658 RVA: 0x003E3BF6 File Offset: 0x003E1DF6
			// (set) Token: 0x06009AEB RID: 39659 RVA: 0x003E3BFE File Offset: 0x003E1DFE
			public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; set; }

			// Token: 0x06009AEC RID: 39660 RVA: 0x003E3C08 File Offset: 0x003E1E08
			public CustomInterfaceElement(Item item, ContentXElement element, CustomInterface parent, CustomInterface.CustomInterfaceElement.InputTypeOption inputType)
			{
				this.Label = element.GetAttributeString("text", "");
				this.ConnectionName = element.GetAttributeString("connection", "");
				this.PropertyName = element.GetAttributeIdentifier("propertyname", Identifier.Empty);
				this.TargetItemComponent = element.GetAttributeIdentifier("targetitemcomponent", Identifier.Empty);
				this.TargetOnlyParentProperty = element.GetAttributeBool("targetonlyparentproperty", false);
				this.NumberInputMin = element.GetAttributeString("min", "0");
				this.NumberInputMax = element.GetAttributeString("max", "99");
				this.NumberInputStep = element.GetAttributeString("step", "1");
				this.NumberInputDecimalPlaces = element.GetAttributeInt("decimalplaces", 0);
				this.MaxTextLength = element.GetAttributeInt("maxtextlength", int.MaxValue);
				this.GetValueInterval = element.GetAttributeFloat("GetValueInterval", -1f);
				this.InputType = inputType;
				this.HasPropertyName = !this.PropertyName.IsEmpty;
				if (this.HasPropertyName && inputType == CustomInterface.CustomInterfaceElement.InputTypeOption.Number)
				{
					string numberType = element.GetAttributeString("numbertype", string.Empty);
					if (!(numberType == "f") && !(numberType == "float"))
					{
						if (!(numberType == "int") && !(numberType == "integer"))
						{
						}
						this.NumberType = new NumberType?(Barotrauma.NumberType.Int);
					}
					else
					{
						this.NumberType = new NumberType?(Barotrauma.NumberType.Float);
					}
				}
				XAttribute attribute = element.GetAttribute("signal");
				if (attribute != null)
				{
					this.Signal = attribute.Value;
					this.ShouldSetProperty = this.HasPropertyName;
				}
				else if (this.HasPropertyName && parent != null)
				{
					parent.SetSignalToPropertyValue(this);
				}
				else
				{
					this.Signal = "1";
				}
				foreach (ContentXElement subElement in element.Elements())
				{
					if (subElement.Name.ToString().Equals("statuseffect", StringComparison.OrdinalIgnoreCase))
					{
						this.StatusEffects.Add(StatusEffect.Load(subElement, "custom interface element (label " + this.Label + ")"));
					}
				}
			}

			// Token: 0x040065A4 RID: 26020
			public bool ContinuousSignal;

			// Token: 0x040065A5 RID: 26021
			public bool State;

			// Token: 0x040065A6 RID: 26022
			public string ConnectionName;

			// Token: 0x040065A7 RID: 26023
			public Connection Connection;

			// Token: 0x040065B2 RID: 26034
			public const string DefaultNumberInputMin = "0";

			// Token: 0x040065B3 RID: 26035
			public const string DefaultNumberInputMax = "99";

			// Token: 0x040065B4 RID: 26036
			public const string DefaultNumberInputStep = "1";

			// Token: 0x040065B5 RID: 26037
			public const int DefaultNumberInputDecimalPlaces = 0;

			// Token: 0x040065BB RID: 26043
			public float GetValueTimer;

			// Token: 0x040065BD RID: 26045
			public List<StatusEffect> StatusEffects = new List<StatusEffect>();

			// Token: 0x020015E0 RID: 5600
			public enum InputTypeOption
			{
				// Token: 0x04006A2B RID: 27179
				Number,
				// Token: 0x04006A2C RID: 27180
				Text,
				// Token: 0x04006A2D RID: 27181
				Button,
				// Token: 0x04006A2E RID: 27182
				TickBox
			}
		}
	}
}
