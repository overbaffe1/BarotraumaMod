using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200029D RID: 669
	internal class NPCWaitAction : EventAction
	{
		// Token: 0x17000F6A RID: 3946
		// (get) Token: 0x06003A82 RID: 14978 RVA: 0x0021F911 File Offset: 0x0021DB11
		// (set) Token: 0x06003A83 RID: 14979 RVA: 0x0021F919 File Offset: 0x0021DB19
		[Serialize("", IsPropertySaveable.Yes, "Tag of the NPC(s) that should wait.", "", false)]
		public Identifier NPCTag { get; set; }

		// Token: 0x17000F6B RID: 3947
		// (get) Token: 0x06003A84 RID: 14980 RVA: 0x0021F922 File Offset: 0x0021DB22
		// (set) Token: 0x06003A85 RID: 14981 RVA: 0x0021F92A File Offset: 0x0021DB2A
		[Serialize(true, IsPropertySaveable.Yes, "Should the NPC start or stop waiting?", "", false)]
		public bool Wait { get; set; }

		// Token: 0x17000F6C RID: 3948
		// (get) Token: 0x06003A86 RID: 14982 RVA: 0x0021F933 File Offset: 0x0021DB33
		// (set) Token: 0x06003A87 RID: 14983 RVA: 0x0021F93B File Offset: 0x0021DB3B
		[Serialize(true, IsPropertySaveable.Yes, "The event actions reset when a GoTo action makes the event jump to a different point. Should the NPC stop waiting when the event resets?", "", false)]
		public bool AbandonOnReset { get; set; }

		// Token: 0x17000F6D RID: 3949
		// (get) Token: 0x06003A88 RID: 14984 RVA: 0x0021F944 File Offset: 0x0021DB44
		// (set) Token: 0x06003A89 RID: 14985 RVA: 0x0021F94C File Offset: 0x0021DB4C
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

		// Token: 0x06003A8A RID: 14986 RVA: 0x0021F964 File Offset: 0x0021DB64
		public NPCWaitAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06003A8B RID: 14987 RVA: 0x0021F970 File Offset: 0x0021DB70
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

		// Token: 0x06003A8C RID: 14988 RVA: 0x0021FA8C File Offset: 0x0021DC8C
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06003A8D RID: 14989 RVA: 0x0021FA94 File Offset: 0x0021DC94
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

		// Token: 0x06003A8E RID: 14990 RVA: 0x0021FB18 File Offset: 0x0021DD18
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

		// Token: 0x06003A8F RID: 14991 RVA: 0x0021FB94 File Offset: 0x0021DD94
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

		// Token: 0x04001E13 RID: 7699
		private float _priority;

		// Token: 0x04001E14 RID: 7700
		private bool isFinished;

		// Token: 0x04001E15 RID: 7701
		private IEnumerable<Character> affectedNpcs;
	}
}
