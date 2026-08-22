using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Networking;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004A7 RID: 1191
	internal class ButtonTerminal : ItemComponent, IClientSerializable, INetSerializable, IServerSerializable
	{
		// Token: 0x060042B7 RID: 17079 RVA: 0x001ABCB0 File Offset: 0x001A9EB0
		public void ServerEventRead(IReadMessage msg, Client c)
		{
			int signalIndex = msg.ReadRangedInteger(0, this.Signals.Length - 1);
			if (!this.item.CanClientAccess(c))
			{
				return;
			}
			if (!this.SendSignal(signalIndex, c.Character, false, null))
			{
				return;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(23, 3);
			defaultInterpolatedStringHandler.AppendFormatted(GameServer.CharacterLogName(c.Character));
			defaultInterpolatedStringHandler.AppendLiteral(" sent a signal \"");
			defaultInterpolatedStringHandler.AppendFormatted(this.Signals[signalIndex]);
			defaultInterpolatedStringHandler.AppendLiteral("\" from ");
			defaultInterpolatedStringHandler.AppendFormatted(this.item.Name);
			GameServer.Log(defaultInterpolatedStringHandler.ToStringAndClear(), ServerLog.MessageType.ItemInteraction);
			this.item.CreateServerEvent<ButtonTerminal>(this, new ButtonTerminal.EventData(signalIndex));
		}

		// Token: 0x060042B8 RID: 17080 RVA: 0x001ABD69 File Offset: 0x001A9F69
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			this.Write(msg, extraData);
		}

		// Token: 0x170011D6 RID: 4566
		// (get) Token: 0x060042B9 RID: 17081 RVA: 0x001ABD73 File Offset: 0x001A9F73
		// (set) Token: 0x060042BA RID: 17082 RVA: 0x001ABD7B File Offset: 0x001A9F7B
		[Editable]
		[Serialize(new string[]
		{

		}, IsPropertySaveable.Yes, "Signals sent when the corresponding buttons are pressed.", "", true)]
		public string[] Signals { get; set; }

		// Token: 0x170011D7 RID: 4567
		// (get) Token: 0x060042BB RID: 17083 RVA: 0x001ABD84 File Offset: 0x001A9F84
		// (set) Token: 0x060042BC RID: 17084 RVA: 0x001ABD8C File Offset: 0x001A9F8C
		[Editable]
		[Serialize("", IsPropertySaveable.Yes, "Identifiers or tags of items that, when contained, allow the terminal buttons to be used. Multiple ones should be separated by commas.", "", true)]
		public string ActivatingItems { get; set; }

		// Token: 0x170011D8 RID: 4568
		// (get) Token: 0x060042BD RID: 17085 RVA: 0x001ABD95 File Offset: 0x001A9F95
		// (set) Token: 0x060042BE RID: 17086 RVA: 0x001ABD9D File Offset: 0x001A9F9D
		private ItemContainer Container { get; set; }

		// Token: 0x170011D9 RID: 4569
		// (get) Token: 0x060042BF RID: 17087 RVA: 0x001ABDA6 File Offset: 0x001A9FA6
		// (set) Token: 0x060042C0 RID: 17088 RVA: 0x001ABDAE File Offset: 0x001A9FAE
		private HashSet<ItemPrefab> ActivatingItemPrefabs { get; set; } = new HashSet<ItemPrefab>();

		// Token: 0x170011DA RID: 4570
		// (get) Token: 0x060042C1 RID: 17089 RVA: 0x001ABDB7 File Offset: 0x001A9FB7
		private bool IsActivated
		{
			get
			{
				return this.ActivatingItemPrefabs.None(null) || (this.Container != null && this.Container.Inventory.AllItems.Any((Item i) => i != null && this.ActivatingItemPrefabs.Any((ItemPrefab p) => p == i.Prefab)));
			}
		}

		// Token: 0x060042C2 RID: 17090 RVA: 0x001ABDF4 File Offset: 0x001A9FF4
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
		}

		// Token: 0x060042C3 RID: 17091 RVA: 0x001ABED4 File Offset: 0x001AA0D4
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
			this.IsActive = true;
		}

		// Token: 0x060042C4 RID: 17092 RVA: 0x001ABF3F File Offset: 0x001AA13F
		public override void Update(float deltaTime, Camera cam)
		{
			base.Update(deltaTime, cam);
			this.item.SendSignal(this.IsActivated ? "1" : "0", "state_out");
		}

		// Token: 0x060042C5 RID: 17093 RVA: 0x001ABF70 File Offset: 0x001AA170
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

		// Token: 0x060042C6 RID: 17094 RVA: 0x001AC07C File Offset: 0x001AA27C
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

		// Token: 0x060042C7 RID: 17095 RVA: 0x001AC18F File Offset: 0x001AA38F
		public override void Reset()
		{
			base.Reset();
			this.Signals = null;
			this.LoadSignals();
			this.LoadActivatingItems();
		}

		// Token: 0x060042C8 RID: 17096 RVA: 0x001AC1AC File Offset: 0x001AA3AC
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

		// Token: 0x060042C9 RID: 17097 RVA: 0x001AC22C File Offset: 0x001AA42C
		public override bool ValidateEventData(NetEntityEvent.IData data)
		{
			ButtonTerminal.EventData eventData;
			return base.TryExtractEventData<ButtonTerminal.EventData>(data, out eventData);
		}

		// Token: 0x060042CA RID: 17098 RVA: 0x001AC244 File Offset: 0x001AA444
		private void Write(IWriteMessage msg, NetEntityEvent.IData extraData)
		{
			ButtonTerminal.EventData eventData = base.ExtractEventData<ButtonTerminal.EventData>(extraData);
			msg.WriteRangedInteger(eventData.SignalIndex, 0, this.Signals.Length - 1);
		}

		// Token: 0x04002014 RID: 8212
		private readonly int requiredSignalCount;

		// Token: 0x04002017 RID: 8215
		private readonly IReadOnlyList<string> buttonSignalDefinitions;

		// Token: 0x02000DD0 RID: 3536
		private readonly struct EventData : ItemComponent.IEventData
		{
			// Token: 0x06006866 RID: 26726 RVA: 0x00222BEC File Offset: 0x00220DEC
			public EventData(int signalIndex)
			{
				this.SignalIndex = signalIndex;
			}

			// Token: 0x040040CD RID: 16589
			public readonly int SignalIndex;
		}
	}
}
