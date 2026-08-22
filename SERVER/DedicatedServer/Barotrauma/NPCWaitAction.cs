using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020001AB RID: 427
	internal class NPCWaitAction : EventAction
	{
		// Token: 0x17000924 RID: 2340
		// (get) Token: 0x06001FD6 RID: 8150 RVA: 0x000D9DF5 File Offset: 0x000D7FF5
		// (set) Token: 0x06001FD7 RID: 8151 RVA: 0x000D9DFD File Offset: 0x000D7FFD
		[Serialize("", IsPropertySaveable.Yes, "Tag of the NPC(s) that should wait.", "", false)]
		public Identifier NPCTag { get; set; }

		// Token: 0x17000925 RID: 2341
		// (get) Token: 0x06001FD8 RID: 8152 RVA: 0x000D9E06 File Offset: 0x000D8006
		// (set) Token: 0x06001FD9 RID: 8153 RVA: 0x000D9E0E File Offset: 0x000D800E
		[Serialize(true, IsPropertySaveable.Yes, "Should the NPC start or stop waiting?", "", false)]
		public bool Wait { get; set; }

		// Token: 0x17000926 RID: 2342
		// (get) Token: 0x06001FDA RID: 8154 RVA: 0x000D9E17 File Offset: 0x000D8017
		// (set) Token: 0x06001FDB RID: 8155 RVA: 0x000D9E1F File Offset: 0x000D801F
		[Serialize(true, IsPropertySaveable.Yes, "The event actions reset when a GoTo action makes the event jump to a different point. Should the NPC stop waiting when the event resets?", "", false)]
		public bool AbandonOnReset { get; set; }

		// Token: 0x17000927 RID: 2343
		// (get) Token: 0x06001FDC RID: 8156 RVA: 0x000D9E28 File Offset: 0x000D8028
		// (set) Token: 0x06001FDD RID: 8157 RVA: 0x000D9E30 File Offset: 0x000D8030
		[Serialize(100f, IsPropertySaveable.Yes, "AI priority for the action. Uses 100 by default, which is the absolute maximum for any objectives, meaning nothing can be prioritized over it, including the emergency objectives, such as find safety and combat.Setting the priority to 70 would function like a regular order, but with the highest priority.A priority of 60 would make the objective work like a lowest priority order.So, if we'll want the character to wait, but still be able to find safety, defend themselves when attacked, or flee from dangers,it's better to use e.g. 70 instead of 100.", "", false)]
		public float Priority
		{
			get
			{
				return this._priority;
			}
			set
			{
				this._priority = Math.Clamp(value, 60f, 100f);
			}
		}

		// Token: 0x06001FDE RID: 8158 RVA: 0x000D9E48 File Offset: 0x000D8048
		public NPCWaitAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06001FDF RID: 8159 RVA: 0x000D9E54 File Offset: 0x000D8054
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			this.affectedNpcs = this.ParentEvent.GetTargets(this.NPCTag).OfType<Character>();
			foreach (Character npc in this.affectedNpcs)
			{
				if (!npc.Removed)
				{
					HumanAIController humanAiController = npc.AIController as HumanAIController;
					if (humanAiController != null)
					{
						if (this.Wait)
						{
							ISpatialEntity targetHull = AIObjectiveGoTo.GetTargetHull(npc);
							AIObjectiveGoTo gotoObjective = new AIObjectiveGoTo(targetHull ?? npc, npc, humanAiController.ObjectiveManager, true, true, 1f, 0f)
							{
								FaceTargetOnCompleted = false,
								OverridePriority = new float?(this.Priority),
								SourceEventAction = this,
								IsWaitOrder = true,
								CloseEnough = 100f
							};
							humanAiController.ObjectiveManager.AddObjective<AIObjectiveGoTo>(gotoObjective);
							humanAiController.ObjectiveManager.WaitTimer = 0f;
						}
						else
						{
							this.AbandonGoToObjectives(humanAiController);
						}
					}
				}
			}
			this.isFinished = true;
		}

		// Token: 0x06001FE0 RID: 8160 RVA: 0x000D9F70 File Offset: 0x000D8170
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06001FE1 RID: 8161 RVA: 0x000D9F78 File Offset: 0x000D8178
		public override void Reset()
		{
			if (this.affectedNpcs != null && this.AbandonOnReset)
			{
				foreach (Character npc in this.affectedNpcs)
				{
					if (!npc.Removed)
					{
						HumanAIController aiController = npc.AIController as HumanAIController;
						if (aiController != null)
						{
							this.AbandonGoToObjectives(aiController);
						}
					}
				}
				this.affectedNpcs = null;
			}
			this.isFinished = false;
		}

		// Token: 0x06001FE2 RID: 8162 RVA: 0x000D9FFC File Offset: 0x000D81FC
		private void AbandonGoToObjectives(HumanAIController aiController)
		{
			foreach (AIObjective objective in aiController.ObjectiveManager.Objectives)
			{
				AIObjectiveGoTo gotoObjective = objective as AIObjectiveGoTo;
				if (gotoObjective != null)
				{
					EventAction sourceEventAction = gotoObjective.SourceEventAction;
					if (((sourceEventAction != null) ? sourceEventAction.ParentEvent : null) == this.ParentEvent)
					{
						gotoObjective.Abandon = true;
					}
				}
			}
		}

		// Token: 0x06001FE3 RID: 8163 RVA: 0x000DA078 File Offset: 0x000D8278
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(23, 4);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.isFinished, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("NPCWaitAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (NPCTag: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.NPCTag.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", Wait: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Wait.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04000F2A RID: 3882
		private float _priority;

		// Token: 0x04000F2B RID: 3883
		private bool isFinished;

		// Token: 0x04000F2C RID: 3884
		private IEnumerable<Character> affectedNpcs;
	}
}
