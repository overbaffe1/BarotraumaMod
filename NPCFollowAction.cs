using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200029B RID: 667
	internal class NPCFollowAction : EventAction
	{
		// Token: 0x17000F5A RID: 3930
		// (get) Token: 0x06003A57 RID: 14935 RVA: 0x0021EF88 File Offset: 0x0021D188
		// (set) Token: 0x06003A58 RID: 14936 RVA: 0x0021EF90 File Offset: 0x0021D190
		[Serialize("", IsPropertySaveable.Yes, "Tag of the NPC(s) that should follow the target.", "", false)]
		public Identifier NPCTag { get; set; }

		// Token: 0x17000F5B RID: 3931
		// (get) Token: 0x06003A59 RID: 14937 RVA: 0x0021EF99 File Offset: 0x0021D199
		// (set) Token: 0x06003A5A RID: 14938 RVA: 0x0021EFA1 File Offset: 0x0021D1A1
		[Serialize("", IsPropertySaveable.Yes, "Tag of the target. Can be any type of entity: if it's a static one like a device or a hull, the NPC will just stay at the position of that target.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x17000F5C RID: 3932
		// (get) Token: 0x06003A5B RID: 14939 RVA: 0x0021EFAA File Offset: 0x0021D1AA
		// (set) Token: 0x06003A5C RID: 14940 RVA: 0x0021EFB2 File Offset: 0x0021D1B2
		[Serialize(true, IsPropertySaveable.Yes, "Should the NPC start or stop following the target?", "", false)]
		public bool Follow { get; set; }

		// Token: 0x17000F5D RID: 3933
		// (get) Token: 0x06003A5D RID: 14941 RVA: 0x0021EFBB File Offset: 0x0021D1BB
		// (set) Token: 0x06003A5E RID: 14942 RVA: 0x0021EFC3 File Offset: 0x0021D1C3
		[Serialize(false, IsPropertySaveable.Yes, "Should the NPC be forced to walk towards the target?", "", false)]
		public bool ForceWalk { get; set; }

		// Token: 0x17000F5E RID: 3934
		// (get) Token: 0x06003A5F RID: 14943 RVA: 0x0021EFCC File Offset: 0x0021D1CC
		// (set) Token: 0x06003A60 RID: 14944 RVA: 0x0021EFD4 File Offset: 0x0021D1D4
		[Serialize(-1, IsPropertySaveable.Yes, "Maximum number of NPCs to target (e.g. you could choose to only make a specific number of security officers follow the player.)", "", false)]
		public int MaxTargets { get; set; }

		// Token: 0x17000F5F RID: 3935
		// (get) Token: 0x06003A61 RID: 14945 RVA: 0x0021EFDD File Offset: 0x0021D1DD
		// (set) Token: 0x06003A62 RID: 14946 RVA: 0x0021EFE5 File Offset: 0x0021D1E5
		[Serialize(true, IsPropertySaveable.Yes, "The event actions reset when a GoTo action makes the event jump to a different point. Should the NPC stop following the target when the event resets?", "", false)]
		public bool AbandonOnReset { get; set; }

		// Token: 0x17000F60 RID: 3936
		// (get) Token: 0x06003A63 RID: 14947 RVA: 0x0021EFEE File Offset: 0x0021D1EE
		// (set) Token: 0x06003A64 RID: 14948 RVA: 0x0021EFF6 File Offset: 0x0021D1F6
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

		// Token: 0x06003A65 RID: 14949 RVA: 0x0021F00E File Offset: 0x0021D20E
		public NPCFollowAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06003A66 RID: 14950 RVA: 0x0021F018 File Offset: 0x0021D218
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

		// Token: 0x06003A67 RID: 14951 RVA: 0x0021F1D8 File Offset: 0x0021D3D8
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06003A68 RID: 14952 RVA: 0x0021F1E0 File Offset: 0x0021D3E0
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

		// Token: 0x06003A69 RID: 14953 RVA: 0x0021F2C8 File Offset: 0x0021D4C8
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

		// Token: 0x04001E00 RID: 7680
		private float _priority;

		// Token: 0x04001E01 RID: 7681
		private bool isFinished;

		// Token: 0x04001E02 RID: 7682
		private IEnumerable<Character> affectedNpcs;

		// Token: 0x04001E03 RID: 7683
		private Entity target;
	}
}
