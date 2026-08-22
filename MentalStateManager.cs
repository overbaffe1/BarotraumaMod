using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x02000168 RID: 360
	internal class MentalStateManager
	{
		// Token: 0x17000AD9 RID: 2777
		// (get) Token: 0x06002B17 RID: 11031 RVA: 0x001DBC80 File Offset: 0x001D9E80
		// (set) Token: 0x06002B18 RID: 11032 RVA: 0x001DBC88 File Offset: 0x001D9E88
		public bool Active { get; set; }

		// Token: 0x17000ADA RID: 2778
		// (get) Token: 0x06002B19 RID: 11033 RVA: 0x001DBC91 File Offset: 0x001D9E91
		// (set) Token: 0x06002B1A RID: 11034 RVA: 0x001DBC99 File Offset: 0x001D9E99
		public MentalStateManager.MentalType CurrentMentalType { get; private set; }

		// Token: 0x06002B1B RID: 11035 RVA: 0x001DBCA2 File Offset: 0x001D9EA2
		public MentalStateManager(Character character, HumanAIController humanAIController)
		{
			this.character = character;
			this.humanAIController = humanAIController;
		}

		// Token: 0x06002B1C RID: 11036 RVA: 0x001DBCB8 File Offset: 0x001D9EB8
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

		// Token: 0x06002B1D RID: 11037 RVA: 0x001DBD24 File Offset: 0x001D9F24
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

		// Token: 0x17000ADB RID: 2779
		// (get) Token: 0x06002B1E RID: 11038 RVA: 0x001DBDB0 File Offset: 0x001D9FB0
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

		// Token: 0x06002B1F RID: 11039 RVA: 0x001DBDD8 File Offset: 0x001D9FD8
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

		// Token: 0x06002B20 RID: 11040 RVA: 0x001DBE60 File Offset: 0x001DA060
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

		// Token: 0x06002B21 RID: 11041 RVA: 0x001DBFDC File Offset: 0x001DA1DC
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

		// Token: 0x04001681 RID: 5761
		private float mentalStateTimer;

		// Token: 0x04001682 RID: 5762
		private const float MentalStateInterval = 7.5f;

		// Token: 0x04001683 RID: 5763
		private float mentalBehaviorTimer;

		// Token: 0x04001684 RID: 5764
		private const float MentalBehaviorInterval = 7.5f;

		// Token: 0x04001685 RID: 5765
		private readonly Character character;

		// Token: 0x04001686 RID: 5766
		private readonly HumanAIController humanAIController;

		// Token: 0x04001689 RID: 5769
		private const string MentalTeamChange = "mental";

		// Token: 0x0400168A RID: 5770
		private int mentalTypeCount;

		// Token: 0x02000DEC RID: 3564
		public enum MentalType
		{
			// Token: 0x040050F8 RID: 20728
			Normal,
			// Token: 0x040050F9 RID: 20729
			Confused,
			// Token: 0x040050FA RID: 20730
			Afraid,
			// Token: 0x040050FB RID: 20731
			Desperate,
			// Token: 0x040050FC RID: 20732
			Berserk
		}
	}
}
