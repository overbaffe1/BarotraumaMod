using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x02000061 RID: 97
	internal class MentalStateManager
	{
		// Token: 0x1700039A RID: 922
		// (get) Token: 0x06000DB9 RID: 3513 RVA: 0x0008801C File Offset: 0x0008621C
		// (set) Token: 0x06000DBA RID: 3514 RVA: 0x00088024 File Offset: 0x00086224
		public bool Active { get; set; }

		// Token: 0x1700039B RID: 923
		// (get) Token: 0x06000DBB RID: 3515 RVA: 0x0008802D File Offset: 0x0008622D
		// (set) Token: 0x06000DBC RID: 3516 RVA: 0x00088035 File Offset: 0x00086235
		public MentalStateManager.MentalType CurrentMentalType { get; private set; }

		// Token: 0x06000DBD RID: 3517 RVA: 0x0008803E File Offset: 0x0008623E
		public MentalStateManager(Character character, HumanAIController humanAIController)
		{
			this.character = character;
			this.humanAIController = humanAIController;
		}

		// Token: 0x06000DBE RID: 3518 RVA: 0x00088054 File Offset: 0x00086254
		public void Update(float deltaTime)
		{
			if (!this.Active)
			{
				return;
			}
			this.mentalStateTimer -= deltaTime;
			if (this.mentalStateTimer <= 0f)
			{
				this.UpdateMentalState();
				this.mentalStateTimer = 7.5f * Rand.Range(0.75f, 1.25f, Rand.RandSync.Unsynced);
			}
			this.mentalBehaviorTimer = Math.Max(0f, this.mentalBehaviorTimer - deltaTime);
		}

		// Token: 0x06000DBF RID: 3519 RVA: 0x000880C0 File Offset: 0x000862C0
		private void UpdateMentalState()
		{
			MentalStateManager.MentalType newMentalType = this.GetMentalType(this.character.CharacterHealth.GetAffliction("psychosis", true));
			bool createdCombat = false;
			if (newMentalType > MentalStateManager.MentalType.Confused)
			{
				if (newMentalType - MentalStateManager.MentalType.Afraid <= 2)
				{
					if (this.CurrentMentalType == MentalStateManager.MentalType.Berserk)
					{
						newMentalType = MentalStateManager.MentalType.Berserk;
					}
					if (newMentalType == this.CurrentMentalType)
					{
						createdCombat = this.CreateCombatBehavior(this.CurrentMentalType);
					}
				}
			}
			else
			{
				this.mentalBehaviorTimer = 0f;
			}
			if (!createdCombat)
			{
				this.CreateDialogueBehavior(newMentalType);
			}
			if (newMentalType != MentalStateManager.MentalType.Berserk)
			{
				this.character.TryRemoveTeamChange("mental");
			}
			this.CurrentMentalType = newMentalType;
		}

		// Token: 0x1700039C RID: 924
		// (get) Token: 0x06000DC0 RID: 3520 RVA: 0x0008814C File Offset: 0x0008634C
		private int MentalTypeCount
		{
			get
			{
				if (this.mentalTypeCount == 0)
				{
					this.mentalTypeCount = Enum.GetNames(typeof(MentalStateManager.MentalType)).Length;
				}
				return this.mentalTypeCount;
			}
		}

		// Token: 0x06000DC1 RID: 3521 RVA: 0x00088174 File Offset: 0x00086374
		private MentalStateManager.MentalType GetMentalType(Affliction affliction)
		{
			if (affliction == null)
			{
				return MentalStateManager.MentalType.Normal;
			}
			int psychosisIndex = (int)(affliction.Strength / (affliction.Prefab.MaxStrength / (float)this.MentalTypeCount) * Rand.Range(1f, 1.2f, Rand.RandSync.Unsynced));
			psychosisIndex = Math.Clamp(psychosisIndex, 0, 4);
			MentalStateManager.MentalType result;
			switch (psychosisIndex)
			{
			case 0:
				result = MentalStateManager.MentalType.Normal;
				break;
			case 1:
				result = MentalStateManager.MentalType.Confused;
				break;
			case 2:
				result = MentalStateManager.MentalType.Afraid;
				break;
			case 3:
				result = MentalStateManager.MentalType.Desperate;
				break;
			case 4:
				result = MentalStateManager.MentalType.Berserk;
				break;
			default:
				throw new ArgumentOutOfRangeException(psychosisIndex.ToString());
			}
			return result;
		}

		// Token: 0x06000DC2 RID: 3522 RVA: 0x000881FC File Offset: 0x000863FC
		public bool CreateCombatBehavior(MentalStateManager.MentalType mentalType)
		{
			Character mentalAttackTarget = (from possibleTarget in Character.CharacterList
			where HumanAIController.IsActive(possibleTarget) && (possibleTarget.TeamID != this.character.TeamID || mentalType == MentalStateManager.MentalType.Berserk) && this.humanAIController.VisibleHulls.Contains(possibleTarget.CurrentHull) && possibleTarget != this.character
			select possibleTarget).GetRandomUnsynced<Character>();
			if (mentalAttackTarget == null)
			{
				return false;
			}
			AIObjectiveCombat.CombatMode combatMode = AIObjectiveCombat.CombatMode.None;
			bool holdFire = mentalType == MentalStateManager.MentalType.Afraid && this.character.IsSecurity;
			switch (mentalType)
			{
			case MentalStateManager.MentalType.Afraid:
				combatMode = (this.character.IsSecurity ? AIObjectiveCombat.CombatMode.Arrest : AIObjectiveCombat.CombatMode.Retreat);
				break;
			case MentalStateManager.MentalType.Desperate:
				combatMode = ((this.character.IsSecurity && mentalAttackTarget.IsHuman) ? AIObjectiveCombat.CombatMode.Arrest : AIObjectiveCombat.CombatMode.Defensive);
				break;
			case MentalStateManager.MentalType.Berserk:
				combatMode = AIObjectiveCombat.CombatMode.Offensive;
				break;
			}
			this.mentalBehaviorTimer = 7.5f;
			this.humanAIController.AddCombatObjective(combatMode, mentalAttackTarget, 0f, (AIObjective obj) => this.mentalBehaviorTimer <= 0f, null, null, holdFire, false);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(25, 1);
			defaultInterpolatedStringHandler.AppendLiteral("dialogmentalstatereaction");
			defaultInterpolatedStringHandler.AppendFormatted<AIObjectiveCombat.CombatMode>(combatMode);
			Identifier textIdentifier = defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier();
			Character character = this.character;
			string value = TextManager.Get(textIdentifier).Value;
			float delay = Rand.Range(0.5f, 1f, Rand.RandSync.Unsynced);
			Identifier identifier = textIdentifier;
			character.Speak(value, null, delay, identifier, 25f);
			if (mentalType == MentalStateManager.MentalType.Berserk && !this.character.HasTeamChange("mental"))
			{
				this.character.TryAddNewTeamChange("mental", new ActiveTeamChange(CharacterTeamType.None, ActiveTeamChange.TeamChangePriorities.Absolute, true));
			}
			return true;
		}

		// Token: 0x06000DC3 RID: 3523 RVA: 0x00088378 File Offset: 0x00086578
		public void CreateDialogueBehavior(MentalStateManager.MentalType mentalType)
		{
			if (mentalType == MentalStateManager.MentalType.Normal)
			{
				return;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
			defaultInterpolatedStringHandler.AppendLiteral("dialogmentalstate");
			defaultInterpolatedStringHandler.AppendFormatted<MentalStateManager.MentalType>(mentalType);
			Identifier textIdentifier = defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier();
			Character character = this.character;
			string value = TextManager.Get(textIdentifier).Value;
			float delay = Rand.Range(0.5f, 1f, Rand.RandSync.Unsynced);
			Identifier identifier = textIdentifier;
			character.Speak(value, null, delay, identifier, 35f);
		}

		// Token: 0x0400066E RID: 1646
		private float mentalStateTimer;

		// Token: 0x0400066F RID: 1647
		private const float MentalStateInterval = 7.5f;

		// Token: 0x04000670 RID: 1648
		private float mentalBehaviorTimer;

		// Token: 0x04000671 RID: 1649
		private const float MentalBehaviorInterval = 7.5f;

		// Token: 0x04000672 RID: 1650
		private readonly Character character;

		// Token: 0x04000673 RID: 1651
		private readonly HumanAIController humanAIController;

		// Token: 0x04000676 RID: 1654
		private const string MentalTeamChange = "mental";

		// Token: 0x04000677 RID: 1655
		private int mentalTypeCount;

		// Token: 0x020007A1 RID: 1953
		public enum MentalType
		{
			// Token: 0x04002D92 RID: 11666
			Normal,
			// Token: 0x04002D93 RID: 11667
			Confused,
			// Token: 0x04002D94 RID: 11668
			Afraid,
			// Token: 0x04002D95 RID: 11669
			Desperate,
			// Token: 0x04002D96 RID: 11670
			Berserk
		}
	}
}
