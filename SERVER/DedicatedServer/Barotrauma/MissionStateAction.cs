using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020001A5 RID: 421
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class MissionStateAction : EventAction
	{
		// Token: 0x17000906 RID: 2310
		// (get) Token: 0x06001F75 RID: 8053 RVA: 0x000D874E File Offset: 0x000D694E
		// (set) Token: 0x06001F76 RID: 8054 RVA: 0x000D8756 File Offset: 0x000D6956
		[Serialize("", IsPropertySaveable.Yes, "Identifiers of the missions whose states to change. Leave blank to only set the state of the mission that triggered the parent event.", "", false)]
		public Identifier MissionIdentifier { get; set; }

		// Token: 0x17000907 RID: 2311
		// (get) Token: 0x06001F77 RID: 8055 RVA: 0x000D875F File Offset: 0x000D695F
		// (set) Token: 0x06001F78 RID: 8056 RVA: 0x000D8767 File Offset: 0x000D6967
		[Serialize(MissionStateAction.OperationType.Set, IsPropertySaveable.Yes, "The operation to perform on missions' states.", "", false)]
		public MissionStateAction.OperationType Operation { get; set; }

		// Token: 0x17000908 RID: 2312
		// (get) Token: 0x06001F79 RID: 8057 RVA: 0x000D8770 File Offset: 0x000D6970
		// (set) Token: 0x06001F7A RID: 8058 RVA: 0x000D8778 File Offset: 0x000D6978
		[Serialize(0, IsPropertySaveable.Yes, "The value to apply to missions' states.", "", false)]
		public int State { get; set; }

		// Token: 0x17000909 RID: 2313
		// (get) Token: 0x06001F7B RID: 8059 RVA: 0x000D8781 File Offset: 0x000D6981
		// (set) Token: 0x06001F7C RID: 8060 RVA: 0x000D8789 File Offset: 0x000D6989
		[Serialize(false, IsPropertySaveable.Yes, "If set to true, missions are forced to fail without a chance of retrying them.", "", false)]
		public bool ForceFailure { get; set; }

		// Token: 0x06001F7D RID: 8061 RVA: 0x000D8794 File Offset: 0x000D6994
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

		// Token: 0x06001F7E RID: 8062 RVA: 0x000D8837 File Offset: 0x000D6A37
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06001F7F RID: 8063 RVA: 0x000D883F File Offset: 0x000D6A3F
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06001F80 RID: 8064 RVA: 0x000D8848 File Offset: 0x000D6A48
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

		// Token: 0x06001F81 RID: 8065 RVA: 0x000D88F4 File Offset: 0x000D6AF4
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

		// Token: 0x06001F82 RID: 8066 RVA: 0x000D8940 File Offset: 0x000D6B40
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

		// Token: 0x04000F02 RID: 3842
		private bool isFinished;

		// Token: 0x02000917 RID: 2327
		[NullableContext(0)]
		public enum OperationType
		{
			// Token: 0x040031FB RID: 12795
			Set,
			// Token: 0x040031FC RID: 12796
			Add
		}
	}
}
