using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Networking;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004AB RID: 1195
	internal class CustomInterface : ItemComponent, IClientSerializable, INetSerializable, IServerSerializable
	{
		// Token: 0x0600434F RID: 17231 RVA: 0x001B0528 File Offset: 0x001AE728
		public void ServerEventRead(IReadMessage msg, Client c)
		{
			bool[] elementStates = new bool[this.customInterfaceElementList.Count];
			string[] elementValues = new string[this.customInterfaceElementList.Count];
			for (int i = 0; i < this.customInterfaceElementList.Count; i++)
			{
				CustomInterface.CustomInterfaceElement element = this.customInterfaceElementList[i];
				CustomInterface.CustomInterfaceElement.InputTypeOption inputType = element.InputType;
				if (inputType > CustomInterface.CustomInterfaceElement.InputTypeOption.Text)
				{
					if (inputType - CustomInterface.CustomInterfaceElement.InputTypeOption.Button <= 1)
					{
						elementStates[i] = msg.ReadBoolean();
					}
				}
				else
				{
					elementValues[i] = msg.ReadString();
				}
			}
			CustomInterface.CustomInterfaceElement clickedButton = null;
			if (c.Character != null && base.DrawHudWhenEquipped)
			{
				Inventory parentInventory = this.item.ParentInventory;
				if (((parentInventory != null) ? parentInventory.Owner : null) == c.Character)
				{
					goto IL_B3;
				}
			}
			if (!this.item.CanClientAccess(c))
			{
				goto IL_18C;
			}
			IL_B3:
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
								if (CustomInterface.TryParseFloatInvariantCulture(elementValues[j], out value))
								{
									this.ValueChanged(element2, value);
								}
							}
						}
						else if (int.TryParse(elementValues[j], out value2))
						{
							this.ValueChanged(element2, value2);
						}
					}
					break;
				}
				case CustomInterface.CustomInterfaceElement.InputTypeOption.Text:
					this.TextChanged(element2, elementValues[j]);
					break;
				case CustomInterface.CustomInterfaceElement.InputTypeOption.Button:
					if (elementStates[j])
					{
						clickedButton = element2;
						this.ButtonClicked(element2);
					}
					break;
				case CustomInterface.CustomInterfaceElement.InputTypeOption.TickBox:
					this.TickBoxToggled(element2, elementStates[j]);
					break;
				}
			}
			IL_18C:
			this.item.CreateServerEvent<CustomInterface>(this, new CustomInterface.EventData(clickedButton));
			this.item.CreateServerEvent<CustomInterface>(this);
		}

		// Token: 0x06004350 RID: 17232 RVA: 0x001B06E4 File Offset: 0x001AE8E4
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			for (int i = 0; i < this.customInterfaceElementList.Count; i++)
			{
				CustomInterface.CustomInterfaceElement element = this.customInterfaceElementList[i];
				switch (element.InputType)
				{
				case CustomInterface.CustomInterfaceElement.InputTypeOption.Number:
				case CustomInterface.CustomInterfaceElement.InputTypeOption.Text:
					msg.WriteString(element.Signal);
					break;
				case CustomInterface.CustomInterfaceElement.InputTypeOption.Button:
				{
					if (!(extraData is Item.ComponentStateEventData))
					{
						goto IL_8C;
					}
					ItemComponent.IEventData componentData = ((Item.ComponentStateEventData)extraData).ComponentData;
					if (!(componentData is CustomInterface.EventData))
					{
						goto IL_8C;
					}
					CustomInterface.EventData eventData = (CustomInterface.EventData)componentData;
					bool val = eventData.BtnElement == this.customInterfaceElementList[i];
					IL_8D:
					msg.WriteBoolean(val);
					break;
					IL_8C:
					val = false;
					goto IL_8D;
				}
				case CustomInterface.CustomInterfaceElement.InputTypeOption.TickBox:
					msg.WriteBoolean(element.State);
					break;
				}
			}
		}

		// Token: 0x170011EC RID: 4588
		// (get) Token: 0x06004351 RID: 17233 RVA: 0x001B0798 File Offset: 0x001AE998
		// (set) Token: 0x06004352 RID: 17234 RVA: 0x001B07AC File Offset: 0x001AE9AC
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

		// Token: 0x170011ED RID: 4589
		// (get) Token: 0x06004353 RID: 17235 RVA: 0x001B07F0 File Offset: 0x001AE9F0
		// (set) Token: 0x06004354 RID: 17236 RVA: 0x001B0810 File Offset: 0x001AEA10
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

		// Token: 0x170011EE RID: 4590
		// (get) Token: 0x06004355 RID: 17237 RVA: 0x001B0854 File Offset: 0x001AEA54
		// (set) Token: 0x06004356 RID: 17238 RVA: 0x001B0874 File Offset: 0x001AEA74
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
						}
						i++;
					}
				}
			}
		}

		// Token: 0x170011EF RID: 4591
		// (get) Token: 0x06004357 RID: 17239 RVA: 0x001B08EB File Offset: 0x001AEAEB
		// (set) Token: 0x06004358 RID: 17240 RVA: 0x001B08F3 File Offset: 0x001AEAF3
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool ShowInsufficientPowerWarning { get; set; }

		// Token: 0x06004359 RID: 17241 RVA: 0x001B08FC File Offset: 0x001AEAFC
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
			this.Labels = element.GetAttributeString("labels", "");
			this.Signals = element.GetAttributeString("signals", "");
			this.ElementStates = element.GetAttributeString("elementstates", "");
		}

		// Token: 0x0600435A RID: 17242 RVA: 0x001B0AD4 File Offset: 0x001AECD4
		private void UpdateLabels(string[] newLabels)
		{
			this.labels = new string[this.customInterfaceElementList.Count];
			for (int i = 0; i < this.labels.Length; i++)
			{
				this.labels[i] = ((i < newLabels.Length) ? newLabels[i] : this.customInterfaceElementList[i].Label);
				this.customInterfaceElementList[i].Label = this.labels[i];
			}
		}

		// Token: 0x0600435B RID: 17243 RVA: 0x001B0B48 File Offset: 0x001AED48
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
		}

		// Token: 0x0600435C RID: 17244 RVA: 0x001B0BF8 File Offset: 0x001AEDF8
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

		// Token: 0x0600435D RID: 17245 RVA: 0x001B0CD8 File Offset: 0x001AEED8
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

		// Token: 0x0600435E RID: 17246 RVA: 0x001B0DD0 File Offset: 0x001AEFD0
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
			if (this.customInterfaceElementList.Any<CustomInterface.CustomInterfaceElement>())
			{
				CoroutineManager.Invoke(delegate
				{
					if (this.item.FullyInitialized && !this.item.Removed)
					{
						this.item.CreateServerEvent<CustomInterface>(this);
					}
				}, 0.1f);
			}
		}

		// Token: 0x0600435F RID: 17247 RVA: 0x001B0E7C File Offset: 0x001AF07C
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

		// Token: 0x06004360 RID: 17248 RVA: 0x001B0F40 File Offset: 0x001AF140
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

		// Token: 0x06004361 RID: 17249 RVA: 0x001B0F69 File Offset: 0x001AF169
		private void TextChanged(CustomInterface.CustomInterfaceElement textElement, string text)
		{
			if (textElement == null)
			{
				return;
			}
			textElement.Signal = text;
			this.SetPropertyValueToSignal(textElement);
		}

		// Token: 0x06004362 RID: 17250 RVA: 0x001B0F80 File Offset: 0x001AF180
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

		// Token: 0x06004363 RID: 17251 RVA: 0x001B1020 File Offset: 0x001AF220
		private void ValueChanged(CustomInterface.CustomInterfaceElement numberInputElement, float value)
		{
			if (numberInputElement == null)
			{
				return;
			}
			numberInputElement.Signal = value.ToString();
			this.SetPropertyValueToSignal(numberInputElement);
		}

		// Token: 0x06004364 RID: 17252 RVA: 0x001B103C File Offset: 0x001AF23C
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

		// Token: 0x06004365 RID: 17253 RVA: 0x001B11BC File Offset: 0x001AF3BC
		public override void UpdateBroken(float deltaTime, Camera cam)
		{
			this.Update(deltaTime, cam);
		}

		// Token: 0x06004366 RID: 17254 RVA: 0x001B11C8 File Offset: 0x001AF3C8
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

		// Token: 0x06004367 RID: 17255 RVA: 0x001B127B File Offset: 0x001AF47B
		private static bool TryParseFloatInvariantCulture(string s, out float f)
		{
			return float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out f);
		}

		// Token: 0x04002036 RID: 8246
		private string[] labels;

		// Token: 0x04002037 RID: 8247
		private string[] signals;

		// Token: 0x04002038 RID: 8248
		private bool[] elementStates;

		// Token: 0x0400203A RID: 8250
		private readonly List<CustomInterface.CustomInterfaceElement> customInterfaceElementList = new List<CustomInterface.CustomInterfaceElement>();

		// Token: 0x02000DEC RID: 3564
		private readonly struct EventData : ItemComponent.IEventData
		{
			// Token: 0x060068BC RID: 26812 RVA: 0x002235F3 File Offset: 0x002217F3
			public EventData(CustomInterface.CustomInterfaceElement btnElement)
			{
				this.BtnElement = btnElement;
			}

			// Token: 0x0400411C RID: 16668
			public readonly CustomInterface.CustomInterfaceElement BtnElement;
		}

		// Token: 0x02000DED RID: 3565
		private class CustomInterfaceElement : ISerializableEntity
		{
			// Token: 0x1700167E RID: 5758
			// (get) Token: 0x060068BD RID: 26813 RVA: 0x002235FC File Offset: 0x002217FC
			// (set) Token: 0x060068BE RID: 26814 RVA: 0x00223604 File Offset: 0x00221804
			[Serialize("", IsPropertySaveable.No, "The text displayed on this button/tickbox.", "Label.", false)]
			[Editable]
			public string Label { get; set; }

			// Token: 0x1700167F RID: 5759
			// (get) Token: 0x060068BF RID: 26815 RVA: 0x0022360D File Offset: 0x0022180D
			// (set) Token: 0x060068C0 RID: 26816 RVA: 0x00223615 File Offset: 0x00221815
			[Serialize("1", IsPropertySaveable.No, "The signal sent out when this button is pressed or this tickbox checked.", "", false)]
			[Editable]
			public string Signal { get; set; }

			// Token: 0x17001680 RID: 5760
			// (get) Token: 0x060068C1 RID: 26817 RVA: 0x0022361E File Offset: 0x0022181E
			public Identifier PropertyName { get; }

			// Token: 0x17001681 RID: 5761
			// (get) Token: 0x060068C2 RID: 26818 RVA: 0x00223626 File Offset: 0x00221826
			public Identifier TargetItemComponent { get; }

			// Token: 0x17001682 RID: 5762
			// (get) Token: 0x060068C3 RID: 26819 RVA: 0x0022362E File Offset: 0x0022182E
			public bool TargetOnlyParentProperty { get; }

			// Token: 0x17001683 RID: 5763
			// (get) Token: 0x060068C4 RID: 26820 RVA: 0x00223636 File Offset: 0x00221836
			public string NumberInputMin { get; }

			// Token: 0x17001684 RID: 5764
			// (get) Token: 0x060068C5 RID: 26821 RVA: 0x0022363E File Offset: 0x0022183E
			public string NumberInputMax { get; }

			// Token: 0x17001685 RID: 5765
			// (get) Token: 0x060068C6 RID: 26822 RVA: 0x00223646 File Offset: 0x00221846
			public string NumberInputStep { get; }

			// Token: 0x17001686 RID: 5766
			// (get) Token: 0x060068C7 RID: 26823 RVA: 0x0022364E File Offset: 0x0022184E
			public int NumberInputDecimalPlaces { get; }

			// Token: 0x17001687 RID: 5767
			// (get) Token: 0x060068C8 RID: 26824 RVA: 0x00223656 File Offset: 0x00221856
			public int MaxTextLength { get; }

			// Token: 0x17001688 RID: 5768
			// (get) Token: 0x060068C9 RID: 26825 RVA: 0x0022365E File Offset: 0x0022185E
			public CustomInterface.CustomInterfaceElement.InputTypeOption InputType { get; }

			// Token: 0x17001689 RID: 5769
			// (get) Token: 0x060068CA RID: 26826 RVA: 0x00223666 File Offset: 0x00221866
			public NumberType? NumberType { get; }

			// Token: 0x1700168A RID: 5770
			// (get) Token: 0x060068CB RID: 26827 RVA: 0x0022366E File Offset: 0x0022186E
			public bool HasPropertyName { get; }

			// Token: 0x1700168B RID: 5771
			// (get) Token: 0x060068CC RID: 26828 RVA: 0x00223676 File Offset: 0x00221876
			// (set) Token: 0x060068CD RID: 26829 RVA: 0x0022367E File Offset: 0x0022187E
			public bool ShouldSetProperty { get; set; }

			// Token: 0x1700168C RID: 5772
			// (get) Token: 0x060068CE RID: 26830 RVA: 0x00223687 File Offset: 0x00221887
			// (set) Token: 0x060068CF RID: 26831 RVA: 0x0022368F File Offset: 0x0022188F
			public float GetValueInterval { get; set; } = -1f;

			// Token: 0x1700168D RID: 5773
			// (get) Token: 0x060068D0 RID: 26832 RVA: 0x00223698 File Offset: 0x00221898
			public string Name
			{
				get
				{
					return "CustomInterfaceElement";
				}
			}

			// Token: 0x1700168E RID: 5774
			// (get) Token: 0x060068D1 RID: 26833 RVA: 0x0022369F File Offset: 0x0022189F
			// (set) Token: 0x060068D2 RID: 26834 RVA: 0x002236A7 File Offset: 0x002218A7
			public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; set; }

			// Token: 0x060068D3 RID: 26835 RVA: 0x002236B0 File Offset: 0x002218B0
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

			// Token: 0x0400411D RID: 16669
			public bool ContinuousSignal;

			// Token: 0x0400411E RID: 16670
			public bool State;

			// Token: 0x0400411F RID: 16671
			public string ConnectionName;

			// Token: 0x04004120 RID: 16672
			public Connection Connection;

			// Token: 0x0400412B RID: 16683
			public const string DefaultNumberInputMin = "0";

			// Token: 0x0400412C RID: 16684
			public const string DefaultNumberInputMax = "99";

			// Token: 0x0400412D RID: 16685
			public const string DefaultNumberInputStep = "1";

			// Token: 0x0400412E RID: 16686
			public const int DefaultNumberInputDecimalPlaces = 0;

			// Token: 0x04004134 RID: 16692
			public float GetValueTimer;

			// Token: 0x04004136 RID: 16694
			public List<StatusEffect> StatusEffects = new List<StatusEffect>();

			// Token: 0x02000EFC RID: 3836
			public enum InputTypeOption
			{
				// Token: 0x04004448 RID: 17480
				Number,
				// Token: 0x04004449 RID: 17481
				Text,
				// Token: 0x0400444A RID: 17482
				Button,
				// Token: 0x0400444B RID: 17483
				TickBox
			}
		}
	}
}
