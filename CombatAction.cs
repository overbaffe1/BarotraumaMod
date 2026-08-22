using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200028A RID: 650
	internal class CombatAction : EventAction
	{
		// Token: 0x17000F1C RID: 3868
		// (get) Token: 0x06003979 RID: 14713 RVA: 0x0021BFC4 File Offset: 0x0021A1C4
		// (set) Token: 0x0600397A RID: 14714 RVA: 0x0021BFCC File Offset: 0x0021A1CC
		[Serialize(AIObjectiveCombat.CombatMode.Offensive, IsPropertySaveable.Yes, "What kind of combat mode should the NPC switch to (Defensive, Offensive, Arrest, Retreat, None)?", "", false)]
		public AIObjectiveCombat.CombatMode CombatMode { get; set; }

		// Token: 0x17000F1D RID: 3869
		// (get) Token: 0x0600397B RID: 14715 RVA: 0x0021BFD5 File Offset: 0x0021A1D5
		// (set) Token: 0x0600397C RID: 14716 RVA: 0x0021BFDD File Offset: 0x0021A1DD
		[Serialize(false, IsPropertySaveable.Yes, "Did this NPC start the fight (as an aggressor)? Attacking instigators doesn't reduce reputation or trigger outpost security.", "", false)]
		public bool IsInstigator { get; set; }

		// Token: 0x17000F1E RID: 3870
		// (get) Token: 0x0600397D RID: 14717 RVA: 0x0021BFE6 File Offset: 0x0021A1E6
		// (set) Token: 0x0600397E RID: 14718 RVA: 0x0021BFEE File Offset: 0x0021A1EE
		[Serialize(AIObjectiveCombat.CombatMode.None, IsPropertySaveable.Yes, "How do guards react to this character attacking others?", "", false)]
		public AIObjectiveCombat.CombatMode GuardReaction { get; set; }

		// Token: 0x17000F1F RID: 3871
		// (get) Token: 0x0600397F RID: 14719 RVA: 0x0021BFF7 File Offset: 0x0021A1F7
		// (set) Token: 0x06003980 RID: 14720 RVA: 0x0021BFFF File Offset: 0x0021A1FF
		[Serialize(AIObjectiveCombat.CombatMode.None, IsPropertySaveable.Yes, "How do other NPCs react to this character attacking others?", "", false)]
		public AIObjectiveCombat.CombatMode WitnessReaction { get; set; }

		// Token: 0x17000F20 RID: 3872
		// (get) Token: 0x06003981 RID: 14721 RVA: 0x0021C008 File Offset: 0x0021A208
		// (set) Token: 0x06003982 RID: 14722 RVA: 0x0021C010 File Offset: 0x0021A210
		[Serialize("", IsPropertySaveable.Yes, "The tag of the NPC to switch to combat mode.", "", false)]
		public Identifier NPCTag { get; set; }

		// Token: 0x17000F21 RID: 3873
		// (get) Token: 0x06003983 RID: 14723 RVA: 0x0021C019 File Offset: 0x0021A219
		// (set) Token: 0x06003984 RID: 14724 RVA: 0x0021C021 File Offset: 0x0021A221
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character the NPC should attack.", "", false)]
		public Identifier EnemyTag { get; set; }

		// Token: 0x17000F22 RID: 3874
		// (get) Token: 0x06003985 RID: 14725 RVA: 0x0021C02A File Offset: 0x0021A22A
		// (set) Token: 0x06003986 RID: 14726 RVA: 0x0021C032 File Offset: 0x0021A232
		[Serialize(120f, IsPropertySaveable.Yes, "How long it takes for the NPC to \"cool down\" (stop attacking).", "", false)]
		public float CoolDown { get; set; }

		// Token: 0x17000F23 RID: 3875
		// (get) Token: 0x06003987 RID: 14727 RVA: 0x0021C03B File Offset: 0x0021A23B
		// (set) Token: 0x06003988 RID: 14728 RVA: 0x0021C043 File Offset: 0x0021A243
		[Serialize(true, IsPropertySaveable.Yes, "The event actions reset when a GoTo action makes the event jump to a different point. Should the NPC revert back to a normal state when the event resets?", "", false)]
		public bool AbandonOnReset { get; set; }

		// Token: 0x06003989 RID: 14729 RVA: 0x0021C04C File Offset: 0x0021A24C
		public CombatAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x0600398A RID: 14730 RVA: 0x0021C058 File Offset: 0x0021A258
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			this.affectedNpcs = from e in this.ParentEvent.GetTargets(this.NPCTag)
			where e is Character
			select e as Character;
			foreach (Character npc in this.affectedNpcs)
			{
				if (!npc.Removed)
				{
					HumanAIController humanAiController = npc.AIController as HumanAIController;
					if (humanAiController != null)
					{
						Character enemy = null;
						float closestDist = float.MaxValue;
						foreach (Entity target in this.ParentEvent.GetTargets(this.EnemyTag))
						{
							Character character = target as Character;
							if (character != null)
							{
								float dist = Vector2.DistanceSquared(npc.WorldPosition, target.WorldPosition);
								if (dist < closestDist)
								{
									enemy = character;
									closestDist = dist;
								}
							}
						}
						if (enemy != null)
						{
							npc.CombatAction = this;
							AIObjectiveManager objectiveManager = humanAiController.ObjectiveManager;
							foreach (AIObjectiveGoTo goToObjective in objectiveManager.GetActiveObjectives<AIObjectiveGoTo>())
							{
								goToObjective.Abandon = true;
							}
							objectiveManager.AddObjective<AIObjectiveCombat>(new AIObjectiveCombat(npc, enemy, this.CombatMode, objectiveManager, 1f, this.CoolDown));
						}
					}
				}
			}
			this.isFinished = true;
		}

		// Token: 0x0600398B RID: 14731 RVA: 0x0021C248 File Offset: 0x0021A448
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x0600398C RID: 14732 RVA: 0x0021C250 File Offset: 0x0021A450
		public override void Reset()
		{
			if (this.affectedNpcs != null && this.AbandonOnReset)
			{
				foreach (Character npc in this.affectedNpcs)
				{
					if (!npc.Removed)
					{
						HumanAIController humanAiController = npc.AIController as HumanAIController;
						if (humanAiController != null)
						{
							foreach (AIObjectiveCombat combatObjective in humanAiController.ObjectiveManager.GetActiveObjectives<AIObjectiveCombat>())
							{
								combatObjective.Abandon = true;
							}
						}
					}
				}
				this.affectedNpcs = null;
			}
			this.isFinished = false;
		}

		// Token: 0x0600398D RID: 14733 RVA: 0x0021C314 File Offset: 0x0021A514
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(53, 6);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.isFinished, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("CombatAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (Cooldown: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.CoolDown.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", CombatMode: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.CombatMode.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", NPCTag: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.NPCTag.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", EnemyTag: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.EnemyTag.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04001DB1 RID: 7601
		private bool isFinished;

		// Token: 0x04001DB2 RID: 7602
		private IEnumerable<Character> affectedNpcs;
	}
}
