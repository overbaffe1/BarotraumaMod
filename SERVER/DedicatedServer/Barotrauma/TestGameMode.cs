using System;
using System.Linq;

namespace Barotrauma
{
	// Token: 0x020001E7 RID: 487
	internal class TestGameMode : GameMode
	{
		// Token: 0x06002303 RID: 8963 RVA: 0x000E997C File Offset: 0x000E7B7C
		public TestGameMode(GameModePreset preset) : base(preset)
		{
			foreach (JobPrefab jobPrefab in from p in JobPrefab.Prefabs
			orderby p.Identifier
			select p)
			{
				for (int i = 0; i < jobPrefab.InitialCount; i++)
				{
					int variant = Rand.Range(0, jobPrefab.Variants, Rand.RandSync.Unsynced);
					base.CrewManager.AddCharacterInfo(new CharacterInfo(CharacterPrefab.HumanSpeciesName, "", "", jobPrefab, variant, Rand.RandSync.Unsynced, default(Identifier)));
				}
			}
		}
	}
}
