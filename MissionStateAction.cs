using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000297 RID: 663
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class MissionStateAction : EventAction
	{
		// Token: 0x17000F4C RID: 3916
		// (get) Token: 0x06003A21 RID: 14881 RVA: 0x0021E37E File Offset: 0x0021C57E
		// (set) Token: 0x06003A22 RID: 14882 RVA: 0x0021E386 File Offset: 0x0021C586
		[Serialize("", IsPropertySaveable.Yes, "Identifiers of the missions whose states to change. Leave blank to only set the state of the mission that triggered the parent event.", "", false)]
		public Identifier MissionIdentifier { get; set; }

		// Token: 0x17000F4D RID: 3917
		// (get) Token: 0x06003A23 RID: 14883 RVA: 0x0021E38F File Offset: 0x0021C58F
		// (set) Token: 0x06003A24 RID: 14884 RVA: 0x0021E397 File Offset: 0x0021C597
		[Serialize(MissionStateAction.OperationType.Set, IsPropertySaveable.Yes, "The operation to perform on missions' states.", "", false)]
		public MissionStateAction.OperationType Operation { get; set; }

		// Token: 0x17000F4E RID: 3918
		// (get) Token: 0x06003A25 RID: 14885 RVA: 0x0021E3A0 File Offset: 0x0021C5A0
		// (set) Token: 0x06003A26 RID: 14886 RVA: 0x0021E3A8 File Offset: 0x0021C5A8
		[Serialize(0, IsPropertySaveable.Yes, "The value to apply to missions' states.", "", false)]
		public int State { get; set; }

		// Token: 0x17000F4F RID: 3919
		// (get) Token: 0x06003A27 RID: 14887 RVA: 0x0021E3B1 File Offset: 0x0021C5B1
		// (set) Token: 0x06003A28 RID: 14888 RVA: 0x0021E3B9 File Offset: 0x0021C5B9
		[Serialize(false, IsPropertySaveable.Yes, "If set to true, missions are forced to fail without a chance of retrying them.", "", false)]
		public bool ForceFailure { get; set; }

		// Token: 0x06003A29 RID: 14889 RVA: 0x0021E3C4 File Offset: 0x0021C5C4
		public MissionStateAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			this.State = element.GetAttributeInt("value", this.State);
			if (this.Operation == MissionStateAction.OperationType.Add && this.State == 0 && !this.ForceFailure)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(95, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Potential error in event \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(parentEvent.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\": ");
				defaultInterpolatedStringHandler.AppendFormatted("MissionStateAction");
				defaultInterpolatedStringHandler.AppendLiteral(" is set to only add 0 to the mission state, which will do nothing.");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), element.ContentPackage);
			}
		}

		// Token: 0x06003A2A RID: 14890 RVA: 0x0021E467 File Offset: 0x0021C667
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06003A2B RID: 14891 RVA: 0x0021E46F File Offset: 0x0021C66F
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06003A2C RID: 14892 RVA: 0x0021E478 File Offset: 0x0021C678
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			Identifier missionIdentifier = this.MissionIdentifier;
			if (!missionIdentifier.IsEmpty)
			{
				using (IEnumerator<Mission> enumerator = GameMain.GameSession.Missions.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Mission mission = enumerator.Current;
						Prefab prefab = mission.Prefab;
						missionIdentifier = this.MissionIdentifier;
						if (!(prefab.Identifier != missionIdentifier))
						{
							this.SetMissionState(mission);
						}
					}
					goto IL_86;
				}
			}
			if (this.ParentEvent.TriggeringMission != null)
			{
				this.SetMissionState(this.ParentEvent.TriggeringMission);
			}
			IL_86:
			this.isFinished = true;
		}

		// Token: 0x06003A2D RID: 14893 RVA: 0x0021E524 File Offset: 0x0021C724
		private void SetMissionState(Mission mission)
		{
			if (this.ForceFailure)
			{
				mission.ForceFailure = true;
			}
			MissionStateAction.OperationType operation = this.Operation;
			if (operation == MissionStateAction.OperationType.Set)
			{
				mission.State = this.State;
				return;
			}
			if (operation != MissionStateAction.OperationType.Add)
			{
				return;
			}
			mission.State += this.State;
		}

		// Token: 0x06003A2E RID: 14894 RVA: 0x0021E570 File Offset: 0x0021C770
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(7, 3);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.isFinished, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("MissionStateAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (");
			defaultInterpolatedStringHandler.AppendFormatted<int>((this.Operation == MissionStateAction.OperationType.Set) ? this.State : (43 + this.State));
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04001DEB RID: 7659
		private bool isFinished;

		// Token: 0x02000F26 RID: 3878
		[NullableContext(0)]
		public enum OperationType
		{
			// Token: 0x040054C2 RID: 21698
			Set,
			// Token: 0x040054C3 RID: 21699
			Add
		}
	}
}
