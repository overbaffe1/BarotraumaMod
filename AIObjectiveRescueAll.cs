using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x0200018C RID: 396
	internal class AIObjectiveRescueAll : AIObjectiveLoop<Character>
	{
		// Token: 0x17000BF1 RID: 3057
		// (get) Token: 0x06002EB8 RID: 11960 RVA: 0x001F2224 File Offset: 0x001F0424
		// (set) Token: 0x06002EB9 RID: 11961 RVA: 0x001F222C File Offset: 0x001F042C
		public override Identifier Identifier { get; set; } = "rescue all".ToIdentifier();

		// Token: 0x17000BF2 RID: 3058
		// (get) Token: 0x06002EBA RID: 11962 RVA: 0x001F2235 File Offset: 0x001F0435
		public override bool ForceRun
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000BF3 RID: 3059
		// (get) Token: 0x06002EBB RID: 11963 RVA: 0x001F2238 File Offset: 0x001F0438
		public override bool InverseTargetPriority
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000BF4 RID: 3060
		// (get) Token: 0x06002EBC RID: 11964 RVA: 0x001F223B File Offset: 0x001F043B
		protected override bool AllowOutsideSubmarine
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000BF5 RID: 3061
		// (get) Token: 0x06002EBD RID: 11965 RVA: 0x001F223E File Offset: 0x001F043E
		protected override bool AllowInAnySub
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06002EBE RID: 11966 RVA: 0x001F2241 File Offset: 0x001F0441
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

		// Token: 0x06002EBF RID: 11967 RVA: 0x001F2264 File Offset: 0x001F0464
		public AIObjectiveRescueAll(Character character, AIObjectiveManager objectiveManager, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
		}

		// Token: 0x06002EC0 RID: 11968 RVA: 0x001F22A0 File Offset: 0x001F04A0
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

		// Token: 0x06002EC1 RID: 11969 RVA: 0x001F235F File Offset: 0x001F055F
		protected override IEnumerable<Character> GetList()
		{
			return Character.CharacterList;
		}

		// Token: 0x06002EC2 RID: 11970 RVA: 0x001F2368 File Offset: 0x001F0568
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

		// Token: 0x06002EC3 RID: 11971 RVA: 0x001F2424 File Offset: 0x001F0624
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

		// Token: 0x06002EC4 RID: 11972 RVA: 0x001F24FC File Offset: 0x001F06FC
		public static IEnumerable<Affliction> GetTreatableAfflictions(Character character, bool ignoreTreatmentThreshold)
		{
			AIObjectiveRescueAll.<GetTreatableAfflictions>d__21 <GetTreatableAfflictions>d__ = new AIObjectiveRescueAll.<GetTreatableAfflictions>d__21(-2);
			<GetTreatableAfflictions>d__.<>3__character = character;
			<GetTreatableAfflictions>d__.<>3__ignoreTreatmentThreshold = ignoreTreatmentThreshold;
			return <GetTreatableAfflictions>d__;
		}

		// Token: 0x06002EC5 RID: 11973 RVA: 0x001F2513 File Offset: 0x001F0713
		protected override AIObjective ObjectiveConstructor(Character target)
		{
			return new AIObjectiveRescue(this.character, target, this.objectiveManager, base.PriorityModifier);
		}

		// Token: 0x06002EC6 RID: 11974 RVA: 0x001F252D File Offset: 0x001F072D
		protected override void OnObjectiveCompleted(AIObjective objective, Character target)
		{
			HumanAIController.RemoveTargets<AIObjectiveRescueAll, Character>(this.character, target);
		}

		// Token: 0x06002EC7 RID: 11975 RVA: 0x001F253C File Offset: 0x001F073C
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

		// Token: 0x06002EC8 RID: 11976 RVA: 0x001F25FC File Offset: 0x001F07FC
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

		// Token: 0x04001853 RID: 6227
		private readonly HashSet<Character> charactersWithMinorInjuries = new HashSet<Character>();

		// Token: 0x04001854 RID: 6228
		private const float vitalityThreshold = 75f;

		// Token: 0x04001855 RID: 6229
		private const float vitalityThresholdForOrders = 90f;
	}
}
