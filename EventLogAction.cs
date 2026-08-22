using System;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000046 RID: 70
	[NullableContext(1)]
	[Nullable(0)]
	internal class EventLogAction : EventAction
	{
		// Token: 0x170002DE RID: 734
		// (get) Token: 0x06000A5A RID: 2650 RVA: 0x00061BCA File Offset: 0x0005FDCA
		// (set) Token: 0x06000A5B RID: 2651 RVA: 0x00061BD2 File Offset: 0x0005FDD2
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the entry. If there's already an entry with the same id, it gets overwritten.", "", false)]
		public Identifier Id { get; set; }

		// Token: 0x170002DF RID: 735
		// (get) Token: 0x06000A5C RID: 2652 RVA: 0x00061BDB File Offset: 0x0005FDDB
		// (set) Token: 0x06000A5D RID: 2653 RVA: 0x00061BE3 File Offset: 0x0005FDE3
		[Serialize("", IsPropertySaveable.Yes, "Text to add to the event log. Can be the text as-is, or a tag referring to a line in a text file.", "", false)]
		public string Text { get; set; }

		// Token: 0x170002E0 RID: 736
		// (get) Token: 0x06000A5E RID: 2654 RVA: 0x00061BEC File Offset: 0x0005FDEC
		// (set) Token: 0x06000A5F RID: 2655 RVA: 0x00061BF4 File Offset: 0x0005FDF4
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character(s) who should see the entry. If empty, the entry is shown to everyone.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x170002E1 RID: 737
		// (get) Token: 0x06000A60 RID: 2656 RVA: 0x00061BFD File Offset: 0x0005FDFD
		// (set) Token: 0x06000A61 RID: 2657 RVA: 0x00061C05 File Offset: 0x0005FE05
		public bool ShowInServerLog { get; set; }

		// Token: 0x06000A62 RID: 2658 RVA: 0x00061C10 File Offset: 0x0005FE10
		public EventLogAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			Identifier identifier = this.Id;
			if (identifier == Identifier.Empty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Error in event \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(parentEvent.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\". ");
				defaultInterpolatedStringHandler.AppendFormatted("EventLogAction");
				defaultInterpolatedStringHandler.AppendLiteral(" with no id.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
			}
			identifier = this.Id;
			string str = identifier.ToString();
			identifier = this.TargetTag;
			this.Id = (str + identifier.ToString()).ToIdentifier();
			foreach (ContentXElement elem in element.Elements())
			{
				if (elem.Name.LocalName.Equals("text", StringComparison.OrdinalIgnoreCase))
				{
					this.textElement = elem;
					break;
				}
			}
			if (this.Text == null)
			{
				this.Text = string.Empty;
			}
			if (this.textElement == null)
			{
				if (this.Text.IsNullOrEmpty())
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(40, 3);
					defaultInterpolatedStringHandler2.AppendLiteral("Error in event \"");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(parentEvent.Prefab.Identifier);
					defaultInterpolatedStringHandler2.AppendLiteral("\". ");
					defaultInterpolatedStringHandler2.AppendFormatted("EventLogAction");
					defaultInterpolatedStringHandler2.AppendLiteral(" with no text set (");
					defaultInterpolatedStringHandler2.AppendFormatted<ContentXElement>(element);
					defaultInterpolatedStringHandler2.AppendLiteral(").");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, element.ContentPackage, false, false);
				}
				else
				{
					this.Text = TextManager.Get(this.Text).Fallback(this.Text, true).Value;
				}
			}
			this.ShowInServerLog = element.GetAttributeBool("ShowInServerLog", this.ParentEvent is TraitorEvent);
		}

		// Token: 0x06000A63 RID: 2659 RVA: 0x00061E20 File Offset: 0x00060020
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06000A64 RID: 2660 RVA: 0x00061E28 File Offset: 0x00060028
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06000A65 RID: 2661 RVA: 0x00061E34 File Offset: 0x00060034
		public LocalizedString GetDisplayText()
		{
			LocalizedString text = this.Text;
			if (this.textElement != null)
			{
				LocalizedString tempDescription = string.Empty;
				TextManager.ConstructDescription(ref tempDescription, this.textElement, new Func<string, string>(this.ParentEvent.GetTextForReplacementElement));
				text = tempDescription.Value;
			}
			return this.ParentEvent.ReplaceVariablesInEventText(text);
		}

		// Token: 0x06000A66 RID: 2662 RVA: 0x00061E97 File Offset: 0x00060097
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			GameSession gameSession = GameMain.GameSession;
			EventLog eventLog;
			if (gameSession == null)
			{
				eventLog = null;
			}
			else
			{
				EventManager eventManager = gameSession.EventManager;
				eventLog = ((eventManager != null) ? eventManager.EventLog : null);
			}
			this.AddEntryProjSpecific(eventLog, this.GetDisplayText().Value);
			this.isFinished = true;
		}

		// Token: 0x06000A67 RID: 2663 RVA: 0x00061ED7 File Offset: 0x000600D7
		private void AddEntryProjSpecific([Nullable(2)] EventLog eventLog, string displayText)
		{
			if (eventLog != null)
			{
				eventLog.AddEntry(this.ParentEvent.Prefab.Identifier, this.Id, displayText);
			}
		}

		// Token: 0x06000A68 RID: 2664 RVA: 0x00061EFC File Offset: 0x000600FC
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 3);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.isFinished, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("EventLogAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (Id: ");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Id);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04000556 RID: 1366
		[Nullable(2)]
		private readonly XElement textElement;

		// Token: 0x04000557 RID: 1367
		private bool isFinished;
	}
}
