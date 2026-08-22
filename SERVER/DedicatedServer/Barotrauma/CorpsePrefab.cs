using System;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x020000C2 RID: 194
	internal class CorpsePrefab : HumanPrefab
	{
		// Token: 0x060015D1 RID: 5585 RVA: 0x000B9BF4 File Offset: 0x000B7DF4
		public override void Dispose()
		{
		}

		// Token: 0x060015D2 RID: 5586 RVA: 0x000B9BF8 File Offset: 0x000B7DF8
		public static CorpsePrefab Get(Identifier identifier)
		{
			if (CorpsePrefab.Prefabs == null)
			{
				DebugConsole.ThrowError("Issue in the code execution order: job prefabs not loaded.", null, null, false, false);
				return null;
			}
			if (CorpsePrefab.Prefabs.ContainsKey(identifier))
			{
				return CorpsePrefab.Prefabs[identifier];
			}
			DebugConsole.ThrowError("Couldn't find a job prefab with the given identifier: " + identifier.ToString(), null, null, false, false);
			return null;
		}

		// Token: 0x1700065A RID: 1626
		// (get) Token: 0x060015D3 RID: 5587 RVA: 0x000B9C56 File Offset: 0x000B7E56
		// (set) Token: 0x060015D4 RID: 5588 RVA: 0x000B9C5E File Offset: 0x000B7E5E
		[Serialize(Level.PositionType.Wreck, IsPropertySaveable.No, "", "", false)]
		public Level.PositionType SpawnPosition { get; private set; }

		// Token: 0x1700065B RID: 1627
		// (get) Token: 0x060015D5 RID: 5589 RVA: 0x000B9C67 File Offset: 0x000B7E67
		// (set) Token: 0x060015D6 RID: 5590 RVA: 0x000B9C6F File Offset: 0x000B7E6F
		[Serialize(0, IsPropertySaveable.No, "", "", false)]
		public int MinMoney { get; private set; }

		// Token: 0x1700065C RID: 1628
		// (get) Token: 0x060015D7 RID: 5591 RVA: 0x000B9C78 File Offset: 0x000B7E78
		// (set) Token: 0x060015D8 RID: 5592 RVA: 0x000B9C80 File Offset: 0x000B7E80
		[Serialize(0, IsPropertySaveable.No, "", "", false)]
		public int MaxMoney { get; private set; }

		// Token: 0x060015D9 RID: 5593 RVA: 0x000B9C89 File Offset: 0x000B7E89
		public CorpsePrefab(ContentXElement element, CorpsesFile file) : base(element, file, Identifier.Empty)
		{
		}

		// Token: 0x060015DA RID: 5594 RVA: 0x000B9C98 File Offset: 0x000B7E98
		public static CorpsePrefab Random(Rand.RandSync sync = Rand.RandSync.Unsynced)
		{
			return CorpsePrefab.Prefabs.GetRandom(sync);
		}

		// Token: 0x04000A62 RID: 2658
		public static readonly PrefabCollection<CorpsePrefab> Prefabs = new PrefabCollection<CorpsePrefab>();
	}
}
