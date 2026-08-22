using System;
using System.Runtime.CompilerServices;
using Barotrauma.Tutorials;

namespace Barotrauma
{
	// Token: 0x02000047 RID: 71
	internal class EventObjectiveAction : EventAction
	{
		// Token: 0x06000A69 RID: 2665 RVA: 0x00061F6C File Offset: 0x0006016C
		[NullableContext(1)]
		public static void Trigger(EventObjectiveAction.SegmentActionType Type, Identifier Identifier, Identifier ObjectiveTag, Identifier ParentObjectiveId, Identifier TextTag, bool CanBeCompleted, bool autoPlayVideo = false, string videoFile = "", int width = 450, int height = 80)
		{
			if (Type == EventObjectiveAction.SegmentActionType.AddIfNotFound && ObjectiveManager.IsSegmentActive(Identifier))
			{
				return;
			}
			ObjectiveManager.Segment segment = null;
			if (Type == EventObjectiveAction.SegmentActionType.Trigger)
			{
				segment = ObjectiveManager.Segment.CreateInfoBoxSegment(Identifier, ObjectiveTag, autoPlayVideo ? Barotrauma.Tutorials.AutoPlayVideo.Yes : Barotrauma.Tutorials.AutoPlayVideo.No, new ObjectiveManager.Segment.Text(TextTag, width, height, Anchor.Center), new ObjectiveManager.Segment.Video(videoFile, TextTag, width, height));
			}
			else if (Type == EventObjectiveAction.SegmentActionType.Add || Type == EventObjectiveAction.SegmentActionType.AddIfNotFound)
			{
				segment = ObjectiveManager.Segment.CreateObjectiveSegment(Identifier, (!ObjectiveTag.IsEmpty) ? ObjectiveTag : Identifier);
			}
			if (segment != null)
			{
				segment.CanBeCompleted = CanBeCompleted;
				segment.ParentId = ParentObjectiveId;
			}
			switch (Type)
			{
			case EventObjectiveAction.SegmentActionType.Trigger:
			case EventObjectiveAction.SegmentActionType.Add:
			case EventObjectiveAction.SegmentActionType.AddIfNotFound:
				ObjectiveManager.TriggerSegment(segment, false);
				return;
			case EventObjectiveAction.SegmentActionType.Complete:
				ObjectiveManager.CompleteSegment(Identifier);
				return;
			case EventObjectiveAction.SegmentActionType.CompleteAndRemove:
				ObjectiveManager.CompleteSegment(Identifier);
				ObjectiveManager.RemoveSegment(Identifier);
				return;
			case EventObjectiveAction.SegmentActionType.Remove:
				ObjectiveManager.RemoveSegment(Identifier);
				return;
			case EventObjectiveAction.SegmentActionType.Fail:
				ObjectiveManager.FailSegment(Identifier);
				return;
			case EventObjectiveAction.SegmentActionType.FailAndRemove:
				ObjectiveManager.FailSegment(Identifier);
				ObjectiveManager.RemoveSegment(Identifier);
				return;
			default:
				return;
			}
		}

		// Token: 0x170002E2 RID: 738
		// (get) Token: 0x06000A6A RID: 2666 RVA: 0x00062041 File Offset: 0x00060241
		// (set) Token: 0x06000A6B RID: 2667 RVA: 0x00062049 File Offset: 0x00060249
		[Serialize(EventObjectiveAction.SegmentActionType.Add, IsPropertySaveable.Yes, "Should the action add a new objective, or do something to an existing objective?", "", false)]
		public EventObjectiveAction.SegmentActionType Type { get; set; }

		// Token: 0x170002E3 RID: 739
		// (get) Token: 0x06000A6C RID: 2668 RVA: 0x00062052 File Offset: 0x00060252
		// (set) Token: 0x06000A6D RID: 2669 RVA: 0x0006205A File Offset: 0x0006025A
		[Serialize("", IsPropertySaveable.Yes, "Arbitrary identifier given to the objective. Can be used to complete/remove/fail the objective later. Also used to fetch the text from the text files.", "", false)]
		public Identifier Identifier { get; set; }

		// Token: 0x170002E4 RID: 740
		// (get) Token: 0x06000A6E RID: 2670 RVA: 0x00062063 File Offset: 0x00060263
		// (set) Token: 0x06000A6F RID: 2671 RVA: 0x0006206B File Offset: 0x0006026B
		[Obsolete]
		[Serialize("", IsPropertySaveable.Yes, "Legacy support. Tag of the text to display as an objective in info box segments.", "", false)]
		public Identifier ObjectiveTag { get; set; }

		// Token: 0x170002E5 RID: 741
		// (get) Token: 0x06000A70 RID: 2672 RVA: 0x00062074 File Offset: 0x00060274
		// (set) Token: 0x06000A71 RID: 2673 RVA: 0x0006207C File Offset: 0x0006027C
		[Obsolete]
		[Serialize(true, IsPropertySaveable.Yes, "Legacy support. Is this objective possible to complete if it's used in an info box segment.", "", false)]
		public bool CanBeCompleted { get; set; }

		// Token: 0x170002E6 RID: 742
		// (get) Token: 0x06000A72 RID: 2674 RVA: 0x00062085 File Offset: 0x00060285
		// (set) Token: 0x06000A73 RID: 2675 RVA: 0x0006208D File Offset: 0x0006028D
		[Serialize("", IsPropertySaveable.Yes, "Identifier of a parent objective. If set, this objective is displayed as a subobjective under the parent objective.", "", false)]
		public Identifier ParentObjectiveId { get; set; }

		// Token: 0x170002E7 RID: 743
		// (get) Token: 0x06000A74 RID: 2676 RVA: 0x00062096 File Offset: 0x00060296
		// (set) Token: 0x06000A75 RID: 2677 RVA: 0x0006209E File Offset: 0x0006029E
		[Obsolete]
		[Serialize(false, IsPropertySaveable.Yes, "Legacy support. Should the video defined by VideoFile play automatically, or wait for the user to play it.", "", false)]
		public bool AutoPlayVideo { get; set; }

		// Token: 0x170002E8 RID: 744
		// (get) Token: 0x06000A76 RID: 2678 RVA: 0x000620A7 File Offset: 0x000602A7
		// (set) Token: 0x06000A77 RID: 2679 RVA: 0x000620AF File Offset: 0x000602AF
		[Obsolete]
		[Serialize("", IsPropertySaveable.Yes, "Legacy support. Tag of the main text to display in info box segments.", "", false)]
		public Identifier TextTag { get; set; }

		// Token: 0x170002E9 RID: 745
		// (get) Token: 0x06000A78 RID: 2680 RVA: 0x000620B8 File Offset: 0x000602B8
		// (set) Token: 0x06000A79 RID: 2681 RVA: 0x000620C0 File Offset: 0x000602C0
		[Obsolete]
		[Serialize("", IsPropertySaveable.Yes, "Legacy support. Path of a video file to display in info box segments.", "", false)]
		public string VideoFile { get; set; }

		// Token: 0x170002EA RID: 746
		// (get) Token: 0x06000A7A RID: 2682 RVA: 0x000620C9 File Offset: 0x000602C9
		// (set) Token: 0x06000A7B RID: 2683 RVA: 0x000620D1 File Offset: 0x000602D1
		[Obsolete]
		[Serialize(450, IsPropertySaveable.Yes, "Legacy support. Width of the info box segment.", "", false)]
		public int Width { get; set; }

		// Token: 0x170002EB RID: 747
		// (get) Token: 0x06000A7C RID: 2684 RVA: 0x000620DA File Offset: 0x000602DA
		// (set) Token: 0x06000A7D RID: 2685 RVA: 0x000620E2 File Offset: 0x000602E2
		[Obsolete]
		[Serialize(80, IsPropertySaveable.Yes, "Legacy support. Height of the info box segment.", "", false)]
		public int Height { get; set; }

		// Token: 0x170002EC RID: 748
		// (get) Token: 0x06000A7E RID: 2686 RVA: 0x000620EB File Offset: 0x000602EB
		// (set) Token: 0x06000A7F RID: 2687 RVA: 0x000620F3 File Offset: 0x000602F3
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character(s) to show the objective to.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x06000A80 RID: 2688 RVA: 0x000620FC File Offset: 0x000602FC
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

		// Token: 0x06000A81 RID: 2689 RVA: 0x00062260 File Offset: 0x00060460
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			this.UpdateProjSpecific();
			this.isFinished = true;
		}

		// Token: 0x06000A82 RID: 2690 RVA: 0x00062278 File Offset: 0x00060478
		private void UpdateProjSpecific()
		{
			EventObjectiveAction.Trigger(this.Type, this.Identifier, this.ObjectiveTag, this.ParentObjectiveId, this.TextTag, this.CanBeCompleted, this.AutoPlayVideo, this.VideoFile, this.Width, this.Height);
		}

		// Token: 0x06000A83 RID: 2691 RVA: 0x000622C6 File Offset: 0x000604C6
		public override bool IsFinished(ref string goToLabel)
		{
			return this.isFinished;
		}

		// Token: 0x06000A84 RID: 2692 RVA: 0x000622CE File Offset: 0x000604CE
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x04000563 RID: 1379
		private bool isFinished;

		// Token: 0x0200079F RID: 1951
		public enum SegmentActionType
		{
			// Token: 0x04003B2A RID: 15146
			[Obsolete]
			Trigger,
			// Token: 0x04003B2B RID: 15147
			Add,
			// Token: 0x04003B2C RID: 15148
			AddIfNotFound,
			// Token: 0x04003B2D RID: 15149
			Complete,
			// Token: 0x04003B2E RID: 15150
			CompleteAndRemove,
			// Token: 0x04003B2F RID: 15151
			Remove,
			// Token: 0x04003B30 RID: 15152
			Fail,
			// Token: 0x04003B31 RID: 15153
			FailAndRemove
		}
	}
}
