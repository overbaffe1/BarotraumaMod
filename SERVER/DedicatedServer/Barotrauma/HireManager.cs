using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020001EB RID: 491
	internal class HireManager
	{
		// Token: 0x17000A28 RID: 2600
		// (get) Token: 0x06002363 RID: 9059 RVA: 0x000ED957 File Offset: 0x000EBB57
		// (set) Token: 0x06002364 RID: 9060 RVA: 0x000ED95F File Offset: 0x000EBB5F
		public List<CharacterInfo> AvailableCharacters { get; set; }

		// Token: 0x06002365 RID: 9061 RVA: 0x000ED968 File Offset: 0x000EBB68
		public HireManager()
		{
			this.AvailableCharacters = new List<CharacterInfo>();
		}

		// Token: 0x06002366 RID: 9062 RVA: 0x000ED986 File Offset: 0x000EBB86
		public void RemoveCharacter(CharacterInfo character)
		{
			this.AvailableCharacters.Remove(character);
		}

		// Token: 0x06002367 RID: 9063 RVA: 0x000ED995 File Offset: 0x000EBB95
		public static int GetSalaryFor(IReadOnlyCollection<CharacterInfo> hires)
		{
			return hires.Sum((CharacterInfo hire) => HireManager.GetSalaryFor(hire));
		}

		// Token: 0x06002368 RID: 9064 RVA: 0x000ED9BC File Offset: 0x000EBBBC
		public static int GetSalaryFor(CharacterInfo hire)
		{
			IEnumerable<Character> crew = GameSession.GetSessionCrewCharacters(CharacterType.Both);
			float multiplier = 0f;
			foreach (Character character in crew)
			{
				float num = multiplier;
				float? num2;
				if (character == null)
				{
					num2 = null;
				}
				else
				{
					CharacterInfo info = character.Info;
					num2 = ((info != null) ? new float?(info.GetSavedStatValueWithAll(StatTypes.HireCostMultiplier, hire.Job.Prefab.Identifier)) : null);
				}
				float? num3 = num2;
				multiplier = num + num3.GetValueOrDefault();
			}
			float finalMultiplier = 1f + MathF.Max(multiplier, -1f);
			return (int)((float)hire.Salary * finalMultiplier);
		}

		// Token: 0x06002369 RID: 9065 RVA: 0x000EDA78 File Offset: 0x000EBC78
		public void GenerateCharacters(Location location, int amount)
		{
			this.AvailableCharacters.ForEach(delegate(CharacterInfo c)
			{
				c.Remove();
			});
			this.AvailableCharacters.Clear();
			foreach (JobPrefab missingJob in location.Type.GetHireablesMissingFromCrew())
			{
				this.<GenerateCharacters>g__AddCharacter|10_1(missingJob);
				amount--;
			}
			for (int i = 0; i < amount; i++)
			{
				this.<GenerateCharacters>g__AddCharacter|10_1(location.Type.GetRandomHireable());
			}
			if (location.Faction != null)
			{
				this.GenerateFactionCharacters(location.Faction.Prefab);
			}
			if (location.SecondaryFaction != null)
			{
				this.GenerateFactionCharacters(location.SecondaryFaction.Prefab);
			}
		}

		// Token: 0x0600236A RID: 9066 RVA: 0x000EDB54 File Offset: 0x000EBD54
		private void GenerateFactionCharacters(FactionPrefab faction)
		{
			foreach (FactionPrefab.HireableCharacter character in faction.HireableCharacters)
			{
				HumanPrefab humanPrefab = NPCSet.Get(character.NPCSetIdentifier, character.NPCIdentifier, true, null);
				if (humanPrefab == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(93, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Couldn't create a hireable for the location: character prefab \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(character.NPCIdentifier);
					defaultInterpolatedStringHandler.AppendLiteral("\" not found in the NPC set \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(character.NPCSetIdentifier);
					defaultInterpolatedStringHandler.AppendLiteral("\".");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				}
				else
				{
					CharacterInfo characterInfo = humanPrefab.CreateCharacterInfo(Rand.RandSync.Unsynced);
					characterInfo.MinReputationToHire = new ValueTuple<Identifier, float>(faction.Identifier, character.MinReputation);
					this.AvailableCharacters.Add(characterInfo);
				}
			}
		}

		// Token: 0x0600236B RID: 9067 RVA: 0x000EDC22 File Offset: 0x000EBE22
		public void Remove()
		{
			this.AvailableCharacters.ForEach(delegate(CharacterInfo c)
			{
				c.Remove();
			});
			this.AvailableCharacters.Clear();
		}

		// Token: 0x0600236C RID: 9068 RVA: 0x000EDC5C File Offset: 0x000EBE5C
		public void RenameCharacter(CharacterInfo characterInfo, string newName)
		{
			if (characterInfo == null || string.IsNullOrEmpty(newName))
			{
				return;
			}
			CharacterInfo characterInfo2 = this.AvailableCharacters.FirstOrDefault((CharacterInfo ci) => ci == characterInfo);
			if (characterInfo2 != null)
			{
				characterInfo2.Rename(newName);
			}
			CharacterInfo characterInfo3 = this.PendingHires.FirstOrDefault((CharacterInfo ci) => ci == characterInfo);
			if (characterInfo3 == null)
			{
				return;
			}
			characterInfo3.Rename(newName);
		}

		// Token: 0x0600236D RID: 9069 RVA: 0x000EDCCC File Offset: 0x000EBECC
		[CompilerGenerated]
		private void <GenerateCharacters>g__AddCharacter|10_1(JobPrefab job)
		{
			if (job == null)
			{
				return;
			}
			int variant = Rand.Range(0, job.Variants, Rand.RandSync.Unsynced);
			this.AvailableCharacters.Add(new CharacterInfo(CharacterPrefab.HumanSpeciesName, "", "", job, variant, Rand.RandSync.Unsynced, default(Identifier)));
		}

		// Token: 0x04001118 RID: 4376
		public List<CharacterInfo> PendingHires = new List<CharacterInfo>();

		// Token: 0x04001119 RID: 4377
		public const int MaxAvailableCharacters = 6;
	}
}
