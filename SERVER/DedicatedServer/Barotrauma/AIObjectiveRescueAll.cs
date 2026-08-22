using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x02000086 RID: 134
	internal class AIObjectiveRescueAll : AIObjectiveLoop<Character>
	{
		// Token: 0x170004D7 RID: 1239
		// (get) Token: 0x060011B0 RID: 4528 RVA: 0x0009F38C File Offset: 0x0009D58C
		// (set) Token: 0x060011B1 RID: 4529 RVA: 0x0009F394 File Offset: 0x0009D594
		public override Identifier Identifier { get; set; } = "rescue all".ToIdentifier();

		// Token: 0x170004D8 RID: 1240
		// (get) Token: 0x060011B2 RID: 4530 RVA: 0x0009F39D File Offset: 0x0009D59D
		public override bool ForceRun
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170004D9 RID: 1241
		// (get) Token: 0x060011B3 RID: 4531 RVA: 0x0009F3A0 File Offset: 0x0009D5A0
		public override bool InverseTargetPriority
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170004DA RID: 1242
		// (get) Token: 0x060011B4 RID: 4532 RVA: 0x0009F3A3 File Offset: 0x0009D5A3
		protected override bool AllowOutsideSubmarine
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170004DB RID: 1243
		// (get) Token: 0x060011B5 RID: 4533 RVA: 0x0009F3A6 File Offset: 0x0009D5A6
		protected override bool AllowInAnySub
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060011B6 RID: 4534 RVA: 0x0009F3A9 File Offset: 0x0009D5A9
		public static float GetVitalityThreshold(AIObjectiveManager manager, Character character, Character target)
		{
			if (manager == null)
			{
				return 75f;
			}
			if (character != target && !manager.HasOrder<AIObjectiveRescueAll>(null))
			{
				return 75f;
			}
			return 90f;
		}

		// Token: 0x060011B7 RID: 4535 RVA: 0x0009F3CC File Offset: 0x0009D5CC
		public AIObjectiveRescueAll(Character character, AIObjectiveManager objectiveManager, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
		}

		// Token: 0x060011B8 RID: 4536 RVA: 0x0009F408 File Offset: 0x0009D608
		protected override bool IsValidTarget(Character target)
		{
			bool ignoredasMinorWounds;
			if (!AIObjectiveRescueAll.IsValidTarget(target, this.character, out ignoredasMinorWounds))
			{
				if (ignoredasMinorWounds && this.character.IsOnPlayerTeam && target != this.character && !this.charactersWithMinorInjuries.Contains(target) && this.objectiveManager.GetFirstActiveObjective<AIObjectiveRescue>() == null)
				{
					this.charactersWithMinorInjuries.Add(target);
					Character character = this.character;
					string value = TextManager.GetWithVariable("dialogignoreminorinjuries", "[targetname]", target.DisplayName, FormatCapitals.No).Value;
					Identifier identifier = ("notreatableafflictions" + target.DisplayName).ToIdentifier();
					character.Speak(value, null, 1f, identifier, 10f);
				}
				return false;
			}
			return true;
		}

		// Token: 0x060011B9 RID: 4537 RVA: 0x0009F4C7 File Offset: 0x0009D6C7
		protected override IEnumerable<Character> GetList()
		{
			return Character.CharacterList;
		}

		// Token: 0x060011BA RID: 4538 RVA: 0x0009F4D0 File Offset: 0x0009D6D0
		protected override float GetTargetPriority()
		{
			if (base.Targets.None(null))
			{
				return 100f;
			}
			if (!this.objectiveManager.IsOrder(this) && !this.character.IsMedic && base.HumanAIController.IsTrueForAnyCrewMember((Character c) => c != this.character && c.IsMedic, true, true))
			{
				return 100f;
			}
			float worstCondition = base.Targets.Min((Character t) => AIObjectiveRescueAll.GetVitalityFactor(t));
			if (base.Targets.Contains(this.character))
			{
				if (this.character.Bleeding > 10f)
				{
					worstCondition = 0f;
				}
				worstCondition /= 2f;
			}
			return worstCondition;
		}

		// Token: 0x060011BB RID: 4539 RVA: 0x0009F58C File Offset: 0x0009D78C
		public static float GetVitalityFactor(Character character)
		{
			float vitality = 100f;
			vitality -= character.Bleeding * 2f;
			vitality += Math.Min(character.Oxygen, 0f);
			foreach (Affliction affliction in AIObjectiveRescueAll.GetTreatableAfflictions(character, true))
			{
				float strength = character.CharacterHealth.GetPredictedStrength(affliction, 10f, null);
				vitality -= affliction.GetVitalityDecrease(character.CharacterHealth, strength) / character.MaxVitality * 100f;
				if (affliction.Strength > affliction.Prefab.TreatmentThreshold && !affliction.Prefab.VitalityLossRequiredForTreatment)
				{
					vitality -= affliction.Strength;
				}
			}
			return Math.Clamp(vitality, 0f, 100f);
		}

		// Token: 0x060011BC RID: 4540 RVA: 0x0009F664 File Offset: 0x0009D864
		public static IEnumerable<Affliction> GetTreatableAfflictions(Character character, bool ignoreTreatmentThreshold)
		{
			AIObjectiveRescueAll.<GetTreatableAfflictions>d__21 <GetTreatableAfflictions>d__ = new AIObjectiveRescueAll.<GetTreatableAfflictions>d__21(-2);
			<GetTreatableAfflictions>d__.<>3__character = character;
			<GetTreatableAfflictions>d__.<>3__ignoreTreatmentThreshold = ignoreTreatmentThreshold;
			return <GetTreatableAfflictions>d__;
		}

		// Token: 0x060011BD RID: 4541 RVA: 0x0009F67B File Offset: 0x0009D87B
		protected override AIObjective ObjectiveConstructor(Character target)
		{
			return new AIObjectiveRescue(this.character, target, this.objectiveManager, base.PriorityModifier);
		}

		// Token: 0x060011BE RID: 4542 RVA: 0x0009F695 File Offset: 0x0009D895
		protected override void OnObjectiveCompleted(AIObjective objective, Character target)
		{
			HumanAIController.RemoveTargets<AIObjectiveRescueAll, Character>(this.character, target);
		}

		// Token: 0x060011BF RID: 4543 RVA: 0x0009F6A4 File Offset: 0x0009D8A4
		public static bool IsValidTarget(Character target, Character character, out bool ignoredAsMinorWounds)
		{
			ignoredAsMinorWounds = false;
			if (target == null || target.IsDead || target.Removed || target.InvisibleTimer > 0f)
			{
				return false;
			}
			if (target.IsInstigator)
			{
				return false;
			}
			if (target.IsPet)
			{
				return false;
			}
			if (!HumanAIController.IsFriendly(character, target, true, false))
			{
				return false;
			}
			HumanAIController humanAI = character.AIController as HumanAIController;
			float vitalityFactor;
			bool isBelowTreatmentThreshold;
			if (humanAI != null)
			{
				if (!AIObjectiveRescueAll.IsValidTargetForAI(target, humanAI))
				{
					return false;
				}
				vitalityFactor = AIObjectiveRescueAll.GetVitalityFactor(target);
				isBelowTreatmentThreshold = (vitalityFactor < AIObjectiveRescueAll.GetVitalityThreshold(humanAI.ObjectiveManager, character, target));
			}
			else
			{
				vitalityFactor = AIObjectiveRescueAll.GetVitalityFactor(target);
				isBelowTreatmentThreshold = (vitalityFactor < 75f);
			}
			bool hasTreatableAfflictions = AIObjectiveRescueAll.GetTreatableAfflictions(target, false).Any<Affliction>();
			bool isValidTarget = isBelowTreatmentThreshold && hasTreatableAfflictions;
			if (!isValidTarget)
			{
				ignoredAsMinorWounds = (hasTreatableAfflictions || vitalityFactor < 100f);
			}
			return isValidTarget;
		}

		// Token: 0x060011C0 RID: 4544 RVA: 0x0009F764 File Offset: 0x0009D964
		private static bool IsValidTargetForAI(Character target, HumanAIController humanAI)
		{
			Character character = humanAI.Character;
			if (!humanAI.ObjectiveManager.HasOrder<AIObjectiveRescueAll>(null))
			{
				if (!character.IsMedic && target != character)
				{
					return false;
				}
				if (humanAI.UnsafeHulls.Contains(target.CurrentHull))
				{
					return false;
				}
			}
			if (character.Submarine != null)
			{
				if (!character.Submarine.IsEntityFoundOnThisSub(target.CurrentHull, true, false, false))
				{
					return false;
				}
			}
			else if (target.Submarine != null)
			{
				return false;
			}
			if (target != character && target.IsBot && HumanAIController.IsActive(target))
			{
				HumanAIController targetAI = target.AIController as HumanAIController;
				if (targetAI != null && (targetAI.ObjectiveManager.HasActiveObjective<AIObjectiveCombat>() || targetAI.ObjectiveManager.HasActiveObjective<AIObjectiveFindSafety>() || targetAI.ObjectiveManager.HasActiveObjective<AIObjectiveRescue>()))
				{
					return false;
				}
			}
			return (target.CurrentHull == null || !Character.CharacterList.Any((Character c) => c.CurrentHull == target.CurrentHull && !HumanAIController.IsFriendly(character, c, false, false) && HumanAIController.IsActive(c))) && character.GetDamageDoneByAttacker(target) <= 0f;
		}

		// Token: 0x04000859 RID: 2137
		private readonly HashSet<Character> charactersWithMinorInjuries = new HashSet<Character>();

		// Token: 0x0400085A RID: 2138
		private const float vitalityThreshold = 75f;

		// Token: 0x0400085B RID: 2139
		private const float vitalityThresholdForOrders = 90f;
	}
}
