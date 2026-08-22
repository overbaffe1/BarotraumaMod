using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x0200001A RID: 26
	internal class EventObjectiveAction : EventAction
	{
		// Token: 0x17000134 RID: 308
		// (get) Token: 0x060003C3 RID: 963 RVA: 0x0001FED9 File Offset: 0x0001E0D9
		// (set) Token: 0x060003C4 RID: 964 RVA: 0x0001FEE1 File Offset: 0x0001E0E1
		[Serialize(EventObjectiveAction.SegmentActionType.Add, IsPropertySaveable.Yes, "Should the action add a new objective, or do something to an existing objective?", "", false)]
		public EventObjectiveAction.SegmentActionType Type { get; set; }

		// Token: 0x17000135 RID: 309
		// (get) Token: 0x060003C5 RID: 965 RVA: 0x0001FEEA File Offset: 0x0001E0EA
		// (set) Token: 0x060003C6 RID: 966 RVA: 0x0001FEF2 File Offset: 0x0001E0F2
		[Serialize("", IsPropertySaveable.Yes, "Arbitrary identifier given to the objective. Can be used to complete/remove/fail the objective later. Also used to fetch the text from the text files.", "", false)]
		public Identifier Identifier { get; set; }

		// Token: 0x17000136 RID: 310
		// (get) Token: 0x060003C7 RID: 967 RVA: 0x0001FEFB File Offset: 0x0001E0FB
		// (set) Token: 0x060003C8 RID: 968 RVA: 0x0001FF03 File Offset: 0x0001E103
		[Obsolete]
		[Serialize("", IsPropertySaveable.Yes, "Legacy support. Tag of the text to display as an objective in info box segments.", "", false)]
		public Identifier ObjectiveTag { get; set; }

		// Token: 0x17000137 RID: 311
		// (get) Token: 0x060003C9 RID: 969 RVA: 0x0001FF0C File Offset: 0x0001E10C
		// (set) Token: 0x060003CA RID: 970 RVA: 0x0001FF14 File Offset: 0x0001E114
		[Obsolete]
		[Serialize(true, IsPropertySaveable.Yes, "Legacy support. Is this objective possible to complete if it's used in an info box segment.", "", false)]
		public bool CanBeCompleted { get; set; }

		// Token: 0x17000138 RID: 312
		// (get) Token: 0x060003CB RID: 971 RVA: 0x0001FF1D File Offset: 0x0001E11D
		// (set) Token: 0x060003CC RID: 972 RVA: 0x0001FF25 File Offset: 0x0001E125
		[Serialize("", IsPropertySaveable.Yes, "Identifier of a parent objective. If set, this objective is displayed as a subobjective under the parent objective.", "", false)]
		public Identifier ParentObjectiveId { get; set; }

		// Token: 0x17000139 RID: 313
		// (get) Token: 0x060003CD RID: 973 RVA: 0x0001FF2E File Offset: 0x0001E12E
		// (set) Token: 0x060003CE RID: 974 RVA: 0x0001FF36 File Offset: 0x0001E136
		[Obsolete]
		[Serialize(false, IsPropertySaveable.Yes, "Legacy support. Should the video defined by VideoFile play automatically, or wait for the user to play it.", "", false)]
		public bool AutoPlayVideo { get; set; }

		// Token: 0x1700013A RID: 314
		// (get) Token: 0x060003CF RID: 975 RVA: 0x0001FF3F File Offset: 0x0001E13F
		// (set) Token: 0x060003D0 RID: 976 RVA: 0x0001FF47 File Offset: 0x0001E147
		[Obsolete]
		[Serialize("", IsPropertySaveable.Yes, "Legacy support. Tag of the main text to display in info box segments.", "", false)]
		public Identifier TextTag { get; set; }

		// Token: 0x1700013B RID: 315
		// (get) Token: 0x060003D1 RID: 977 RVA: 0x0001FF50 File Offset: 0x0001E150
		// (set) Token: 0x060003D2 RID: 978 RVA: 0x0001FF58 File Offset: 0x0001E158
		[Obsolete]
		[Serialize("", IsPropertySaveable.Yes, "Legacy support. Path of a video file to display in info box segments.", "", false)]
		public string VideoFile { get; set; }

		// Token: 0x1700013C RID: 316
		// (get) Token: 0x060003D3 RID: 979 RVA: 0x0001FF61 File Offset: 0x0001E161
		// (set) Token: 0x060003D4 RID: 980 RVA: 0x0001FF69 File Offset: 0x0001E169
		[Obsolete]
		[Serialize(450, IsPropertySaveable.Yes, "Legacy support. Width of the info box segment.", "", false)]
		public int Width { get; set; }

		// Token: 0x1700013D RID: 317
		// (get) Token: 0x060003D5 RID: 981 RVA: 0x0001FF72 File Offset: 0x0001E172
		// (set) Token: 0x060003D6 RID: 982 RVA: 0x0001FF7A File Offset: 0x0001E17A
		[Obsolete]
		[Serialize(80, IsPropertySaveable.Yes, "Legacy support. Height of the info box segment.", "", false)]
		public int Height { get; set; }

		// Token: 0x1700013E RID: 318
		// (get) Token: 0x060003D7 RID: 983 RVA: 0x0001FF83 File Offset: 0x0001E183
		// (set) Token: 0x060003D8 RID: 984 RVA: 0x0001FF8B File Offset: 0x0001E18B
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character(s) to show the objective to.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x060003D9 RID: 985 RVA: 0x0001FF94 File Offset: 0x0001E194
		public EventObjectiveAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			if (this.Identifier.IsEmpty)
			{
				this.Identifier = element.GetAttributeIdentifier("id", Identifier.Empty);
			}
			if (this.Type != EventObjectiveAction.SegmentActionType.Trigger && !this.TextTag.IsEmpty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(97, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Error in ");
				defaultInterpolatedStringHandler.AppendFormatted("EventObjectiveAction");
				defaultInterpolatedStringHandler.AppendLiteral(" in the event \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(parentEvent.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\"");
				defaultInterpolatedStringHandler.AppendLiteral(" - ");
				defaultInterpolatedStringHandler.AppendFormatted("TextTag");
				defaultInterpolatedStringHandler.AppendLiteral(" will do nothing unless the action triggers a message box or a video.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
			}
			ContentXElement childElement = element.GetChildElement("Replace");
			ContentXElement contentXElement = null;
			if (childElement != contentXElement)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(65, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("Error in ");
				defaultInterpolatedStringHandler2.AppendFormatted("EventObjectiveAction");
				defaultInterpolatedStringHandler2.AppendLiteral(" in the event \"");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(parentEvent.Prefab.Identifier);
				defaultInterpolatedStringHandler2.AppendLiteral("\"");
				defaultInterpolatedStringHandler2.AppendLiteral(" - unrecognized child element \"Replace\".");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, element.ContentPackage, false, false);
			}
		}

		// Token: 0x060003DA RID: 986 RVA: 0x000200F8 File Offset: 0x0001E2F8
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			this.UpdateProjSpecific();
			this.isFinished = true;
		}

		// Token: 0x060003DB RID: 987 RVA: 0x00020110 File Offset: 0x0001E310
		private void UpdateProjSpecific()
		{
			if (GameMain.Server == null)
			{
				return;
			}
			EventManager.NetEventObjective objective = new EventManager.NetEventObjective(this.Type, this.Identifier, this.ObjectiveTag, this.TextTag, this.ParentObjectiveId, this.CanBeCompleted);
			if (this.TargetTag.IsEmpty)
			{
				using (IEnumerator<Client> enumerator = GameMain.Server.ConnectedClients.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Client client = enumerator.Current;
						if (client.Character != null)
						{
							EventManager.ServerWriteObjective(client, objective);
						}
					}
					return;
				}
			}
			foreach (Entity target in this.ParentEvent.GetTargets(this.TargetTag))
			{
				Character character = target as Character;
				if (character != null)
				{
					Client ownerClient = GameMain.Server.ConnectedClients.Find((Client c) => c.Character == character);
					if (ownerClient != null)
					{
						EventManager.ServerWriteObjective(ownerClient, objective);
					}
				}
			}
		}

		// Token: 0x060003DC RID: 988 RVA: 0x00020240 File Offset: 0x0001E440
		public override bool IsFinished(ref string goToLabel)
		{
			return this.isFinished;
		}

		// Token: 0x060003DD RID: 989 RVA: 0x00020248 File Offset: 0x0001E448
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x040001C8 RID: 456
		private bool isFinished;

		// Token: 0x020005AF RID: 1455
		public enum SegmentActionType
		{
			// Token: 0x0400273D RID: 10045
			[Obsolete]
			Trigger,
			// Token: 0x0400273E RID: 10046
			Add,
			// Token: 0x0400273F RID: 10047
			AddIfNotFound,
			// Token: 0x04002740 RID: 10048
			Complete,
			// Token: 0x04002741 RID: 10049
			CompleteAndRemove,
			// Token: 0x04002742 RID: 10050
			Remove,
			// Token: 0x04002743 RID: 10051
			Fail,
			// Token: 0x04002744 RID: 10052
			FailAndRemove
		}
	}
}
