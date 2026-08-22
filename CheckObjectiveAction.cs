using System;

namespace Barotrauma
{
	// Token: 0x02000044 RID: 68
	internal class CheckObjectiveAction : BinaryOptionAction
	{
		// Token: 0x170002C7 RID: 711
		// (get) Token: 0x06000A10 RID: 2576 RVA: 0x0005FF67 File Offset: 0x0005E167
		// (set) Token: 0x06000A11 RID: 2577 RVA: 0x0005FF6F File Offset: 0x0005E16F
		[Serialize(CheckObjectiveAction.CheckType.Completed, IsPropertySaveable.Yes, "The objective must be in this state for the check to succeed.", "", false)]
		public CheckObjectiveAction.CheckType Type { get; set; }

		// Token: 0x170002C8 RID: 712
		// (get) Token: 0x06000A12 RID: 2578 RVA: 0x0005FF78 File Offset: 0x0005E178
		// (set) Token: 0x06000A13 RID: 2579 RVA: 0x0005FF80 File Offset: 0x0005E180
		[Serialize("", IsPropertySaveable.Yes, "The identifier of the objective to check.", "", false)]
		public Identifier Identifier { get; set; }

		// Token: 0x06000A14 RID: 2580 RVA: 0x0005FF89 File Offset: 0x0005E189
		public CheckObjectiveAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06000A15 RID: 2581 RVA: 0x0005FF94 File Offset: 0x0005E194
		protected override bool? DetermineSuccess()
		{
			bool success = false;
			this.DetermineSuccessProjSpecific(ref success);
			return new bool?(success);
		}

		// Token: 0x06000A16 RID: 2582 RVA: 0x0005FFB4 File Offset: 0x0005E1B4
		private void DetermineSuccessProjSpecific(ref bool success)
		{
			success = false;
			if (this.Identifier.IsEmpty)
			{
				success = ObjectiveManager.AllActiveObjectivesCompleted();
				return;
			}
			ObjectiveManager.Segment segment = ObjectiveManager.GetObjective(this.Identifier);
			if (segment != null)
			{
				bool flag;
				switch (this.Type)
				{
				case CheckObjectiveAction.CheckType.Added:
					flag = true;
					break;
				case CheckObjectiveAction.CheckType.Completed:
					flag = segment.IsCompleted;
					break;
				case CheckObjectiveAction.CheckType.Incomplete:
					flag = !segment.IsCompleted;
					break;
				default:
					flag = false;
					break;
				}
				success = flag;
				return;
			}
			if (this.Type == CheckObjectiveAction.CheckType.Incomplete)
			{
				success = true;
			}
		}

		// Token: 0x02000793 RID: 1939
		public enum CheckType
		{
			// Token: 0x04003AF9 RID: 15097
			Added,
			// Token: 0x04003AFA RID: 15098
			Completed,
			// Token: 0x04003AFB RID: 15099
			Incomplete
		}
	}
}
