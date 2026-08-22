using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x02000019 RID: 25
	[NullableContext(1)]
	[Nullable(0)]
	internal class EventLogAction : EventAction
	{
		// Token: 0x17000130 RID: 304
		// (get) Token: 0x060003B3 RID: 947 RVA: 0x0001F907 File Offset: 0x0001DB07
		// (set) Token: 0x060003B4 RID: 948 RVA: 0x0001F90F File Offset: 0x0001DB0F
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the entry. If there's already an entry with the same id, it gets overwritten.", "", false)]
		public Identifier Id { get; set; }

		// Token: 0x17000131 RID: 305
		// (get) Token: 0x060003B5 RID: 949 RVA: 0x0001F918 File Offset: 0x0001DB18
		// (set) Token: 0x060003B6 RID: 950 RVA: 0x0001F920 File Offset: 0x0001DB20
		[Serialize("", IsPropertySaveable.Yes, "Text to add to the event log. Can be the text as-is, or a tag referring to a line in a text file.", "", false)]
		public string Text { get; set; }

		// Token: 0x17000132 RID: 306
		// (get) Token: 0x060003B7 RID: 951 RVA: 0x0001F929 File Offset: 0x0001DB29
		// (set) Token: 0x060003B8 RID: 952 RVA: 0x0001F931 File Offset: 0x0001DB31
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character(s) who should see the entry. If empty, the entry is shown to everyone.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x17000133 RID: 307
		// (get) Token: 0x060003B9 RID: 953 RVA: 0x0001F93A File Offset: 0x0001DB3A
		// (set) Token: 0x060003BA RID: 954 RVA: 0x0001F942 File Offset: 0x0001DB42
		public bool ShowInServerLog { get; set; }

		// Token: 0x060003BB RID: 955 RVA: 0x0001F94C File Offset: 0x0001DB4C
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

		// Token: 0x060003BC RID: 956 RVA: 0x0001FB5C File Offset: 0x0001DD5C
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x060003BD RID: 957 RVA: 0x0001FB64 File Offset: 0x0001DD64
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x060003BE RID: 958 RVA: 0x0001FB70 File Offset: 0x0001DD70
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

		// Token: 0x060003BF RID: 959 RVA: 0x0001FBD3 File Offset: 0x0001DDD3
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

		// Token: 0x060003C0 RID: 960 RVA: 0x0001FC14 File Offset: 0x0001DE14
		private void AddEntryProjSpecific([Nullable(2)] EventLog eventLog, string displayText)
		{
			EventLogAction.<>c__DisplayClass23_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.displayText = displayText;
			if (eventLog == null)
			{
				return;
			}
			if (!this.TargetTag.IsEmpty)
			{
				List<Client> targetClients = new List<Client>();
				foreach (Entity target in this.ParentEvent.GetTargets(this.TargetTag))
				{
					Character character = target as Character;
					if (character != null)
					{
						Client ownerClient = GameMain.Server.ConnectedClients.Find((Client c) => c.Character == character);
						if (ownerClient != null)
						{
							targetClients.Add(ownerClient);
						}
					}
					else
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(79, 1);
						defaultInterpolatedStringHandler.AppendFormatted<Entity>(target);
						defaultInterpolatedStringHandler.AppendLiteral(" is not a valid target for an EventLogAction. The target should be a character.");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), this.ParentEvent.Prefab.ContentPackage);
					}
				}
				if (eventLog.TryAddEntry(this.ParentEvent.Prefab.Identifier, this.Id, CS$<>8__locals1.displayText, targetClients) && this.ShowInServerLog)
				{
					this.<AddEntryProjSpecific>g__Log|23_0(targetClients, ref CS$<>8__locals1);
					return;
				}
			}
			else if (eventLog.TryAddEntry(this.ParentEvent.Prefab.Identifier, this.Id, CS$<>8__locals1.displayText, GameMain.Server.ConnectedClients) && this.ShowInServerLog)
			{
				this.<AddEntryProjSpecific>g__Log|23_0(null, ref CS$<>8__locals1);
			}
		}

		// Token: 0x060003C1 RID: 961 RVA: 0x0001FD94 File Offset: 0x0001DF94
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

		// Token: 0x060003C2 RID: 962 RVA: 0x0001FE04 File Offset: 0x0001E004
		[CompilerGenerated]
		private void <AddEntryProjSpecific>g__Log|23_0([Nullable(new byte[]
		{
			2,
			1
		})] List<Client> targetClients, ref EventLogAction.<>c__DisplayClass23_0 A_2)
		{
			string text;
			if (targetClients != null && !targetClients.None(null))
			{
				text = " (" + string.Join(", ", from c in targetClients
				select NetworkMember.ClientLogName(c, null)) + ")";
			}
			else
			{
				text = string.Empty;
			}
			string clientStr = text;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 2);
			defaultInterpolatedStringHandler.AppendLiteral("Event \"");
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.ParentEvent.Prefab.Name);
			defaultInterpolatedStringHandler.AppendLiteral("\"");
			defaultInterpolatedStringHandler.AppendFormatted(clientStr);
			defaultInterpolatedStringHandler.AppendLiteral(": ");
			GameServer.Log(defaultInterpolatedStringHandler.ToStringAndClear() + A_2.displayText, (this.ParentEvent is TraitorEvent) ? ServerLog.MessageType.Traitors : ServerLog.MessageType.Chat);
		}

		// Token: 0x040001BB RID: 443
		[Nullable(2)]
		private readonly XElement textElement;

		// Token: 0x040001BC RID: 444
		private bool isFinished;
	}
}
