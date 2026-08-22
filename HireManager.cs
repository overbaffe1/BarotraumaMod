using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020002D5 RID: 725
	internal class HireManager
	{
		// Token: 0x1700101C RID: 4124
		// (get) Token: 0x06003D09 RID: 15625 RVA: 0x0022CFC5 File Offset: 0x0022B1C5
		// (set) Token: 0x06003D0A RID: 15626 RVA: 0x0022CFCD File Offset: 0x0022B1CD
		public List<CharacterInfo> AvailableCharacters { get; set; }

		// Token: 0x06003D0B RID: 15627 RVA: 0x0022CFD6 File Offset: 0x0022B1D6
		public HireManager()
		{
			this.AvailableCharacters = new List<CharacterInfo>();
		}

		// Token: 0x06003D0C RID: 15628 RVA: 0x0022CFF4 File Offset: 0x0022B1F4
		public void RemoveCharacter(CharacterInfo character)
		{
			this.AvailableCharacters.Remove(character);
		}

		// Token: 0x06003D0D RID: 15629 RVA: 0x0022D003 File Offset: 0x0022B203
		public static int GetSalaryFor(IReadOnlyCollection<CharacterInfo> hires)
		{
			return hires.Sum((CharacterInfo hire) => HireManager.GetSalaryFor(hire));
		}

		// Token: 0x06003D0E RID: 15630 RVA: 0x0022D02C File Offset: 0x0022B22C
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

		// Token: 0x06003D0F RID: 15631 RVA: 0x0022D0E8 File Offset: 0x0022B2E8
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

		// Token: 0x06003D10 RID: 15632 RVA: 0x0022D1C4 File Offset: 0x0022B3C4
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

		// Token: 0x06003D11 RID: 15633 RVA: 0x0022D292 File Offset: 0x0022B492
		public void Remove()
		{
			this.AvailableCharacters.ForEach(delegate(CharacterInfo c)
			{
				c.Remove();
			});
			this.AvailableCharacters.Clear();
		}

		// Token: 0x06003D12 RID: 15634 RVA: 0x0022D2CC File Offset: 0x0022B4CC
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

		// Token: 0x06003D13 RID: 15635 RVA: 0x0022D33C File Offset: 0x0022B53C
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

		// Token: 0x04001FA1 RID: 8097
		public List<CharacterInfo> PendingHires = new List<CharacterInfo>();

		// Token: 0x04001FA2 RID: 8098
		public const int MaxAvailableCharacters = 6;
	}
}
