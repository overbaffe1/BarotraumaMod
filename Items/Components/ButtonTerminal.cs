using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005D6 RID: 1494
	internal class ButtonTerminal : ItemComponent, IClientSerializable, INetSerializable, IServerSerializable
	{
		// Token: 0x06005FED RID: 24557 RVA: 0x0031F1A0 File Offset: 0x0031D3A0
		protected override void CreateGUI()
		{
			GUILayoutGroup paddedFrame = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.8f), base.GuiFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.08f
			};
			GUILayoutGroup guilayoutGroup = paddedFrame;
			guilayoutGroup.OnAddedToGUIUpdateList = (Action<GUIComponent>)Delegate.Combine(guilayoutGroup.OnAddedToGUIUpdateList, new Action<GUIComponent>(delegate(GUIComponent component)
			{
				bool buttonsEnabled = this.IsActivated;
				Action<GUIComponent> <>9__1;
				foreach (GUIComponent child in component.Children)
				{
					if (child is GUIButton && child.UserData is int)
					{
						child.Enabled = buttonsEnabled;
						IEnumerable<GUIComponent> children = child.Children;
						Action<GUIComponent> action;
						if ((action = <>9__1) == null)
						{
							action = (<>9__1 = delegate(GUIComponent c)
							{
								c.Enabled = buttonsEnabled;
							});
						}
						children.ForEach(action);
					}
				}
				if (this.Container == null)
				{
					return;
				}
				bool itemsContained = this.Container.Inventory.AllItems.Any<Item>();
				if (itemsContained)
				{
					GUIComponentStyle indicatorStyle = buttonsEnabled ? this.indicatorStyleGreen : this.indicatorStyleRed;
					if (this.containerIndicator.Style != indicatorStyle)
					{
						this.containerIndicator.ApplyStyle(indicatorStyle);
					}
				}
				this.containerIndicator.OverrideState = new GUIComponent.ComponentState?(itemsContained ? GUIComponent.ComponentState.Selected : GUIComponent.ComponentState.None);
			}));
			float x = 1f / (float)(1 + this.requiredSignalCount);
			float y = Math.Min(x * (float)paddedFrame.Rect.Width / (float)paddedFrame.Rect.Height, 0.5f);
			Vector2 relativeSize = new Vector2(x, y);
			GUIFrame containerSection = new GUIFrame(new RectTransform(new Vector2(x, 1f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUIFrame containerSlot = new GUIFrame(new RectTransform(new Vector2(1f, y), containerSection.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), null, null);
			this.containerHolder = new GUIFrame(new RectTransform(new Vector2(1f, 1.2f), containerSlot.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal), null, null);
			this.containerIndicator = new GUIImage(new RectTransform(new Vector2(0.5f, 0.5f * (1f - y)), containerSection.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal), "IndicatorLightRed", true);
			for (int i = 0; i < this.requiredSignalCount; i++)
			{
				GUIButton button2 = new GUIButton(new RectTransform(relativeSize, paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.Center, null, null)
				{
					UserData = i,
					OnClicked = delegate(GUIButton button, object userData)
					{
						int signalIndex = (int)userData;
						if (GameMain.IsSingleplayer)
						{
							this.SendSignal(signalIndex, Character.Controlled, false, null);
						}
						else
						{
							this.item.CreateClientEvent<ButtonTerminal>(this, new ButtonTerminal.EventData(signalIndex));
						}
						return true;
					}
				};
				GUIImage image = new GUIImage(new RectTransform(Vector2.One, button2.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), this.terminalButtonStyles[i], true);
			}
		}

		// Token: 0x06005FEE RID: 24558 RVA: 0x0031F459 File Offset: 0x0031D659
		protected override void OnResolutionChanged()
		{
			this.OnItemLoadedProjSpecific();
		}

		// Token: 0x06005FEF RID: 24559 RVA: 0x0031F461 File Offset: 0x0031D661
		public void ClientEventWrite(IWriteMessage msg, NetEntityEvent.IData extraData = null)
		{
			this.Write(msg, extraData);
		}

		// Token: 0x06005FF0 RID: 24560 RVA: 0x0031F46B File Offset: 0x0031D66B
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			this.SendSignal(msg.ReadRangedInteger(0, this.Signals.Length - 1), null, true, null);
		}

		// Token: 0x17001846 RID: 6214
		// (get) Token: 0x06005FF1 RID: 24561 RVA: 0x0031F488 File Offset: 0x0031D688
		// (set) Token: 0x06005FF2 RID: 24562 RVA: 0x0031F490 File Offset: 0x0031D690
		[Editable]
		[Serialize(new string[]
		{

		}, IsPropertySaveable.Yes, "Signals sent when the corresponding buttons are pressed.", "", true)]
		public string[] Signals { get; set; }

		// Token: 0x17001847 RID: 6215
		// (get) Token: 0x06005FF3 RID: 24563 RVA: 0x0031F499 File Offset: 0x0031D699
		// (set) Token: 0x06005FF4 RID: 24564 RVA: 0x0031F4A1 File Offset: 0x0031D6A1
		[Editable]
		[Serialize("", IsPropertySaveable.Yes, "Identifiers or tags of items that, when contained, allow the terminal buttons to be used. Multiple ones should be separated by commas.", "", true)]
		public string ActivatingItems { get; set; }

		// Token: 0x17001848 RID: 6216
		// (get) Token: 0x06005FF5 RID: 24565 RVA: 0x0031F4AA File Offset: 0x0031D6AA
		// (set) Token: 0x06005FF6 RID: 24566 RVA: 0x0031F4B2 File Offset: 0x0031D6B2
		private ItemContainer Container { get; set; }

		// Token: 0x17001849 RID: 6217
		// (get) Token: 0x06005FF7 RID: 24567 RVA: 0x0031F4BB File Offset: 0x0031D6BB
		// (set) Token: 0x06005FF8 RID: 24568 RVA: 0x0031F4C3 File Offset: 0x0031D6C3
		private HashSet<ItemPrefab> ActivatingItemPrefabs { get; set; } = new HashSet<ItemPrefab>();

		// Token: 0x1700184A RID: 6218
		// (get) Token: 0x06005FF9 RID: 24569 RVA: 0x0031F4CC File Offset: 0x0031D6CC
		private bool IsActivated
		{
			get
			{
				return this.ActivatingItemPrefabs.None(null) || (this.Container != null && this.Container.Inventory.AllItems.Any((Item i) => i != null && this.ActivatingItemPrefabs.Any((ItemPrefab p) => p == i.Prefab)));
			}
		}

		// Token: 0x06005FFA RID: 24570 RVA: 0x0031F50C File Offset: 0x0031D70C
		public ButtonTerminal(Item item, ContentXElement element) : base(item, element)
		{
			IEnumerable<ContentXElement> buttons = from c in element.GetChildElements("TerminalButton")
			where c.GetAttribute("style") != null
			select c;
			if (buttons.None(null))
			{
				DebugConsole.ThrowError("Error in item \"" + item.Name + "\": no TerminalButton elements with a style defined for the ButtonTerminal component!", null, element.ContentPackage, false, false);
			}
			this.requiredSignalCount = buttons.Count<ContentXElement>();
			List<string> buttonSignals = new List<string>();
			foreach (ContentXElement button in buttons)
			{
				buttonSignals.Add(button.GetAttributeString("signal", null));
			}
			this.buttonSignalDefinitions = buttonSignals.ToImmutableList<string>();
			this.InitProjSpecific(element);
		}

		// Token: 0x06005FFB RID: 24571 RVA: 0x0031F5F4 File Offset: 0x0031D7F4
		private void InitProjSpecific(ContentXElement element)
		{
			this.terminalButtonStyles = new string[this.requiredSignalCount];
			int i = 0;
			foreach (ContentXElement childElement in element.GetChildElements("TerminalButton"))
			{
				string style = childElement.GetAttributeString("style", null);
				if (style != null)
				{
					this.terminalButtonStyles[i++] = style;
				}
			}
			this.indicatorStyleRed = GUIStyle.GetComponentStyle("IndicatorLightRed");
			this.indicatorStyleGreen = GUIStyle.GetComponentStyle("IndicatorLightGreen");
			this.CreateGUI();
		}

		// Token: 0x06005FFC RID: 24572 RVA: 0x0031F698 File Offset: 0x0031D898
		public override void OnItemLoaded()
		{
			base.OnItemLoaded();
			this.LoadSignals();
			this.LoadActivatingItems();
			IEnumerable<ItemContainer> containers = this.item.GetComponents<ItemContainer>();
			if (containers.Count<ItemContainer>() != 1)
			{
				DebugConsole.ThrowError("Error in item \"" + this.item.Name + "\": the ButtonTerminal component requires exactly one ItemContainer component!", null, null, false, false);
				return;
			}
			this.Container = containers.FirstOrDefault<ItemContainer>();
			this.OnItemLoadedProjSpecific();
			this.IsActive = true;
		}

		// Token: 0x06005FFD RID: 24573 RVA: 0x0031F709 File Offset: 0x0031D909
		private void OnItemLoadedProjSpecific()
		{
			if (this.Container == null)
			{
				return;
			}
			this.Container.AllowUIOverlap = true;
			this.Container.Inventory.RectTransform = this.containerHolder.RectTransform;
		}

		// Token: 0x06005FFE RID: 24574 RVA: 0x0031F73B File Offset: 0x0031D93B
		public override void Update(float deltaTime, Camera cam)
		{
			base.Update(deltaTime, cam);
			this.item.SendSignal(this.IsActivated ? "1" : "0", "state_out");
		}

		// Token: 0x06005FFF RID: 24575 RVA: 0x0031F76C File Offset: 0x0031D96C
		private void LoadSignals()
		{
			if (this.Signals == null || this.Signals.None(null))
			{
				this.Signals = new string[this.requiredSignalCount];
				for (int i = 0; i < this.requiredSignalCount; i++)
				{
					this.Signals[i] = string.Empty;
				}
				for (int j = 0; j < this.buttonSignalDefinitions.Count; j++)
				{
					string overrideDefinition = this.buttonSignalDefinitions[j];
					if (overrideDefinition != null)
					{
						this.Signals[j] = overrideDefinition;
					}
				}
				return;
			}
			if (this.Signals.Length != this.requiredSignalCount)
			{
				string[] newSignals = new string[this.requiredSignalCount];
				if (this.Signals.Length < this.requiredSignalCount)
				{
					this.Signals.CopyTo(newSignals, 0);
					for (int k = this.Signals.Length; k < this.requiredSignalCount; k++)
					{
						newSignals[k] = string.Empty;
					}
				}
				else
				{
					for (int l = 0; l < this.requiredSignalCount; l++)
					{
						newSignals[l] = this.Signals[l];
					}
				}
				this.Signals = newSignals;
			}
		}

		// Token: 0x06006000 RID: 24576 RVA: 0x0031F878 File Offset: 0x0031DA78
		private void LoadActivatingItems()
		{
			this.ActivatingItemPrefabs.Clear();
			if (!string.IsNullOrEmpty(this.ActivatingItems))
			{
				string[] array = this.ActivatingItems.Split(',', StringSplitOptions.None);
				for (int i = 0; i < array.Length; i++)
				{
					string activatingItem = array[i];
					Identifier itemIdentifier = activatingItem.ToIdentifier();
					ItemPrefab prefab = MapEntityPrefab.FindByIdentifier(itemIdentifier) as ItemPrefab;
					if (prefab != null)
					{
						this.ActivatingItemPrefabs.Add(prefab);
					}
					else
					{
						Func<Identifier, bool> <>9__2;
						ItemPrefab.Prefabs.Where(delegate(ItemPrefab p)
						{
							IEnumerable<Identifier> tags = p.Tags;
							Func<Identifier, bool> predicate;
							if ((predicate = <>9__2) == null)
							{
								predicate = (<>9__2 = ((Identifier t) => t == itemIdentifier));
							}
							return tags.Any(predicate);
						}).ForEach(delegate(ItemPrefab p)
						{
							this.ActivatingItemPrefabs.Add(p);
						});
					}
				}
				if (this.ActivatingItemPrefabs.None(null))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(78, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Error in item \"");
					defaultInterpolatedStringHandler.AppendFormatted(this.item.Name);
					defaultInterpolatedStringHandler.AppendLiteral("\": no activating item prefabs found with identifiers or tags \"");
					defaultInterpolatedStringHandler.AppendFormatted(this.ActivatingItems);
					defaultInterpolatedStringHandler.AppendLiteral("\"");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				}
			}
		}

		// Token: 0x06006001 RID: 24577 RVA: 0x0031F98B File Offset: 0x0031DB8B
		public override void Reset()
		{
			base.Reset();
			this.Signals = null;
			this.LoadSignals();
			this.LoadActivatingItems();
		}

		// Token: 0x06006002 RID: 24578 RVA: 0x0031F9A8 File Offset: 0x0031DBA8
		private bool SendSignal(int signalIndex, Character sender, bool ignoreState = false, string overrideSignal = null)
		{
			if (!ignoreState && !this.IsActivated)
			{
				return false;
			}
			string signal = overrideSignal ?? this.Signals[signalIndex];
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
			defaultInterpolatedStringHandler.AppendLiteral("signal_out");
			defaultInterpolatedStringHandler.AppendFormatted<int>(signalIndex + 1);
			string connectionName = defaultInterpolatedStringHandler.ToStringAndClear();
			this.item.SendSignal(new Signal(signal, 0, sender, null, 0f, 1f), connectionName);
			AchievementManager.OnButtonTerminalSignal(this.item, sender);
			return true;
		}

		// Token: 0x06006003 RID: 24579 RVA: 0x0031FA28 File Offset: 0x0031DC28
		public override bool ValidateEventData(NetEntityEvent.IData data)
		{
			ButtonTerminal.EventData eventData;
			return base.TryExtractEventData<ButtonTerminal.EventData>(data, out eventData);
		}

		// Token: 0x06006004 RID: 24580 RVA: 0x0031FA40 File Offset: 0x0031DC40
		private void Write(IWriteMessage msg, NetEntityEvent.IData extraData)
		{
			ButtonTerminal.EventData eventData = base.ExtractEventData<ButtonTerminal.EventData>(extraData);
			msg.WriteRangedInteger(eventData.SignalIndex, 0, this.Signals.Length - 1);
		}

		// Token: 0x040031A5 RID: 12709
		private string[] terminalButtonStyles;

		// Token: 0x040031A6 RID: 12710
		private GUIFrame containerHolder;

		// Token: 0x040031A7 RID: 12711
		private GUIImage containerIndicator;

		// Token: 0x040031A8 RID: 12712
		private GUIComponentStyle indicatorStyleRed;

		// Token: 0x040031A9 RID: 12713
		private GUIComponentStyle indicatorStyleGreen;

		// Token: 0x040031AC RID: 12716
		private readonly int requiredSignalCount;

		// Token: 0x040031AF RID: 12719
		private readonly IReadOnlyList<string> buttonSignalDefinitions;

		// Token: 0x02001452 RID: 5202
		private readonly struct EventData : ItemComponent.IEventData
		{
			// Token: 0x06009A78 RID: 39544 RVA: 0x003E3424 File Offset: 0x003E1624
			public EventData(int signalIndex)
			{
				this.SignalIndex = signalIndex;
			}

			// Token: 0x0400654E RID: 25934
			public readonly int SignalIndex;
		}
	}
}
