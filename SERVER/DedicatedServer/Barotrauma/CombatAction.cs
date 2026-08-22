using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000197 RID: 407
	internal class CombatAction : EventAction
	{
		// Token: 0x170008C8 RID: 2248
		// (get) Token: 0x06001EAF RID: 7855 RVA: 0x000D6B10 File Offset: 0x000D4D10
		// (set) Token: 0x06001EB0 RID: 7856 RVA: 0x000D6B18 File Offset: 0x000D4D18
		[Serialize(AIObjectiveCombat.CombatMode.Offensive, IsPropertySaveable.Yes, "What kind of combat mode should the NPC switch to (Defensive, Offensive, Arrest, Retreat, None)?", "", false)]
		public AIObjectiveCombat.CombatMode CombatMode { get; set; }

		// Token: 0x170008C9 RID: 2249
		// (get) Token: 0x06001EB1 RID: 7857 RVA: 0x000D6B21 File Offset: 0x000D4D21
		// (set) Token: 0x06001EB2 RID: 7858 RVA: 0x000D6B29 File Offset: 0x000D4D29
		[Serialize(false, IsPropertySaveable.Yes, "Did this NPC start the fight (as an aggressor)? Attacking instigators doesn't reduce reputation or trigger outpost security.", "", false)]
		public bool IsInstigator { get; set; }

		// Token: 0x170008CA RID: 2250
		// (get) Token: 0x06001EB3 RID: 7859 RVA: 0x000D6B32 File Offset: 0x000D4D32
		// (set) Token: 0x06001EB4 RID: 7860 RVA: 0x000D6B3A File Offset: 0x000D4D3A
		[Serialize(AIObjectiveCombat.CombatMode.None, IsPropertySaveable.Yes, "How do guards react to this character attacking others?", "", false)]
		public AIObjectiveCombat.CombatMode GuardReaction { get; set; }

		// Token: 0x170008CB RID: 2251
		// (get) Token: 0x06001EB5 RID: 7861 RVA: 0x000D6B43 File Offset: 0x000D4D43
		// (set) Token: 0x06001EB6 RID: 7862 RVA: 0x000D6B4B File Offset: 0x000D4D4B
		[Serialize(AIObjectiveCombat.CombatMode.None, IsPropertySaveable.Yes, "How do other NPCs react to this character attacking others?", "", false)]
		public AIObjectiveCombat.CombatMode WitnessReaction { get; set; }

		// Token: 0x170008CC RID: 2252
		// (get) Token: 0x06001EB7 RID: 7863 RVA: 0x000D6B54 File Offset: 0x000D4D54
		// (set) Token: 0x06001EB8 RID: 7864 RVA: 0x000D6B5C File Offset: 0x000D4D5C
		[Serialize("", IsPropertySaveable.Yes, "The tag of the NPC to switch to combat mode.", "", false)]
		public Identifier NPCTag { get; set; }

		// Token: 0x170008CD RID: 2253
		// (get) Token: 0x06001EB9 RID: 7865 RVA: 0x000D6B65 File Offset: 0x000D4D65
		// (set) Token: 0x06001EBA RID: 7866 RVA: 0x000D6B6D File Offset: 0x000D4D6D
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character the NPC should attack.", "", false)]
		public Identifier EnemyTag { get; set; }

		// Token: 0x170008CE RID: 2254
		// (get) Token: 0x06001EBB RID: 7867 RVA: 0x000D6B76 File Offset: 0x000D4D76
		// (set) Token: 0x06001EBC RID: 7868 RVA: 0x000D6B7E File Offset: 0x000D4D7E
		[Serialize(120f, IsPropertySaveable.Yes, "How long it takes for the NPC to \"cool down\" (stop attacking).", "", false)]
		public float CoolDown { get; set; }

		// Token: 0x170008CF RID: 2255
		// (get) Token: 0x06001EBD RID: 7869 RVA: 0x000D6B87 File Offset: 0x000D4D87
		// (set) Token: 0x06001EBE RID: 7870 RVA: 0x000D6B8F File Offset: 0x000D4D8F
		[Serialize(true, IsPropertySaveable.Yes, "The event actions reset when a GoTo action makes the event jump to a different point. Should the NPC revert back to a normal state when the event resets?", "", false)]
		public bool AbandonOnReset { get; set; }

		// Token: 0x06001EBF RID: 7871 RVA: 0x000D6B98 File Offset: 0x000D4D98
		public CombatAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06001EC0 RID: 7872 RVA: 0x000D6BA4 File Offset: 0x000D4DA4
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

		// Token: 0x06001EC1 RID: 7873 RVA: 0x000D6D94 File Offset: 0x000D4F94
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06001EC2 RID: 7874 RVA: 0x000D6D9C File Offset: 0x000D4F9C
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

		// Token: 0x06001EC3 RID: 7875 RVA: 0x000D6E60 File Offset: 0x000D5060
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

		// Token: 0x04000EBA RID: 3770
		private bool isFinished;

		// Token: 0x04000EBB RID: 3771
		private IEnumerable<Character> affectedNpcs;
	}
}
