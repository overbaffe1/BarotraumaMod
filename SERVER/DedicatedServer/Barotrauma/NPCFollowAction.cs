using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020001A9 RID: 425
	internal class NPCFollowAction : EventAction
	{
		// Token: 0x17000914 RID: 2324
		// (get) Token: 0x06001FAB RID: 8107 RVA: 0x000D946C File Offset: 0x000D766C
		// (set) Token: 0x06001FAC RID: 8108 RVA: 0x000D9474 File Offset: 0x000D7674
		[Serialize("", IsPropertySaveable.Yes, "Tag of the NPC(s) that should follow the target.", "", false)]
		public Identifier NPCTag { get; set; }

		// Token: 0x17000915 RID: 2325
		// (get) Token: 0x06001FAD RID: 8109 RVA: 0x000D947D File Offset: 0x000D767D
		// (set) Token: 0x06001FAE RID: 8110 RVA: 0x000D9485 File Offset: 0x000D7685
		[Serialize("", IsPropertySaveable.Yes, "Tag of the target. Can be any type of entity: if it's a static one like a device or a hull, the NPC will just stay at the position of that target.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x17000916 RID: 2326
		// (get) Token: 0x06001FAF RID: 8111 RVA: 0x000D948E File Offset: 0x000D768E
		// (set) Token: 0x06001FB0 RID: 8112 RVA: 0x000D9496 File Offset: 0x000D7696
		[Serialize(true, IsPropertySaveable.Yes, "Should the NPC start or stop following the target?", "", false)]
		public bool Follow { get; set; }

		// Token: 0x17000917 RID: 2327
		// (get) Token: 0x06001FB1 RID: 8113 RVA: 0x000D949F File Offset: 0x000D769F
		// (set) Token: 0x06001FB2 RID: 8114 RVA: 0x000D94A7 File Offset: 0x000D76A7
		[Serialize(false, IsPropertySaveable.Yes, "Should the NPC be forced to walk towards the target?", "", false)]
		public bool ForceWalk { get; set; }

		// Token: 0x17000918 RID: 2328
		// (get) Token: 0x06001FB3 RID: 8115 RVA: 0x000D94B0 File Offset: 0x000D76B0
		// (set) Token: 0x06001FB4 RID: 8116 RVA: 0x000D94B8 File Offset: 0x000D76B8
		[Serialize(-1, IsPropertySaveable.Yes, "Maximum number of NPCs to target (e.g. you could choose to only make a specific number of security officers follow the player.)", "", false)]
		public int MaxTargets { get; set; }

		// Token: 0x17000919 RID: 2329
		// (get) Token: 0x06001FB5 RID: 8117 RVA: 0x000D94C1 File Offset: 0x000D76C1
		// (set) Token: 0x06001FB6 RID: 8118 RVA: 0x000D94C9 File Offset: 0x000D76C9
		[Serialize(true, IsPropertySaveable.Yes, "The event actions reset when a GoTo action makes the event jump to a different point. Should the NPC stop following the target when the event resets?", "", false)]
		public bool AbandonOnReset { get; set; }

		// Token: 0x1700091A RID: 2330
		// (get) Token: 0x06001FB7 RID: 8119 RVA: 0x000D94D2 File Offset: 0x000D76D2
		// (set) Token: 0x06001FB8 RID: 8120 RVA: 0x000D94DA File Offset: 0x000D76DA
		[Serialize(100f, IsPropertySaveable.Yes, "AI priority for the action. Uses 100 by default, which is the absolute maximum for any objectives, meaning nothing can be prioritized over it, including the emergency objectives, such as find safety and combat.Setting the priority to 70 would function like a regular order, but with the highest priority.A priority of 60 would make the objective work like a lowest priority order.So, if we'll want the character to follow, but still be able to find safety, defend themselves when attacked, or flee from dangers,it's better to use e.g. 70 instead of 100.", "", false)]
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

		// Token: 0x06001FB9 RID: 8121 RVA: 0x000D94F2 File Offset: 0x000D76F2
		public NPCFollowAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06001FBA RID: 8122 RVA: 0x000D94FC File Offset: 0x000D76FC
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			this.target = this.ParentEvent.GetTargets(this.TargetTag).FirstOrDefault<Entity>();
			if (this.target == null)
			{
				return;
			}
			int targetCount = 0;
			this.affectedNpcs = this.ParentEvent.GetTargets(this.NPCTag).OfType<Character>();
			foreach (Character npc in this.affectedNpcs)
			{
				if (!npc.Removed)
				{
					HumanAIController humanAiController = npc.AIController as HumanAIController;
					if (humanAiController != null)
					{
						if (this.Follow)
						{
							AIObjectiveGoTo newObjective = new AIObjectiveGoTo(this.target, npc, humanAiController.ObjectiveManager, true, true, 1f, 0f)
							{
								OverridePriority = new float?(this.Priority),
								IsFollowOrder = true,
								ForceWalkPermanently = this.ForceWalk
							};
							humanAiController.ObjectiveManager.AddObjective<AIObjectiveGoTo>(newObjective);
							humanAiController.ObjectiveManager.WaitTimer = 0f;
						}
						else
						{
							foreach (AIObjective objective in humanAiController.ObjectiveManager.Objectives)
							{
								AIObjectiveGoTo goToObjective = objective as AIObjectiveGoTo;
								if (goToObjective != null && goToObjective.Target == this.target)
								{
									goToObjective.Abandon = true;
								}
							}
						}
						targetCount++;
						if (this.MaxTargets > -1 && targetCount >= this.MaxTargets)
						{
							break;
						}
					}
				}
			}
			this.isFinished = true;
		}

		// Token: 0x06001FBB RID: 8123 RVA: 0x000D96BC File Offset: 0x000D78BC
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06001FBC RID: 8124 RVA: 0x000D96C4 File Offset: 0x000D78C4
		public override void Reset()
		{
			if (this.affectedNpcs != null && this.target != null && this.AbandonOnReset)
			{
				foreach (Character npc in this.affectedNpcs)
				{
					if (!npc.Removed)
					{
						HumanAIController humanAiController = npc.AIController as HumanAIController;
						if (humanAiController != null)
						{
							foreach (AIObjectiveGoTo goToObjective in humanAiController.ObjectiveManager.GetActiveObjectives<AIObjectiveGoTo>())
							{
								if (goToObjective.Target == this.target)
								{
									goToObjective.Abandon = true;
								}
							}
						}
					}
				}
				this.target = null;
				this.affectedNpcs = null;
			}
			this.isFinished = false;
		}

		// Token: 0x06001FBD RID: 8125 RVA: 0x000D97AC File Offset: 0x000D79AC
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(38, 5);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.isFinished, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("NPCFollowAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (NPCTag: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.NPCTag.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", TargetTag: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.TargetTag.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", Follow: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Follow.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04000F17 RID: 3863
		private float _priority;

		// Token: 0x04000F18 RID: 3864
		private bool isFinished;

		// Token: 0x04000F19 RID: 3865
		private IEnumerable<Character> affectedNpcs;

		// Token: 0x04000F1A RID: 3866
		private Entity target;
	}
}
