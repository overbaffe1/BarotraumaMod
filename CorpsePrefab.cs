using System;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x020001C3 RID: 451
	internal class CorpsePrefab : HumanPrefab
	{
		// Token: 0x06003191 RID: 12689 RVA: 0x002049FC File Offset: 0x00202BFC
		public override void Dispose()
		{
		}

		// Token: 0x06003192 RID: 12690 RVA: 0x00204A00 File Offset: 0x00202C00
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

		// Token: 0x17000D00 RID: 3328
		// (get) Token: 0x06003193 RID: 12691 RVA: 0x00204A5E File Offset: 0x00202C5E
		// (set) Token: 0x06003194 RID: 12692 RVA: 0x00204A66 File Offset: 0x00202C66
		[Serialize(Level.PositionType.Wreck, IsPropertySaveable.No, "", "", false)]
		public Level.PositionType SpawnPosition { get; private set; }

		// Token: 0x17000D01 RID: 3329
		// (get) Token: 0x06003195 RID: 12693 RVA: 0x00204A6F File Offset: 0x00202C6F
		// (set) Token: 0x06003196 RID: 12694 RVA: 0x00204A77 File Offset: 0x00202C77
		[Serialize(0, IsPropertySaveable.No, "", "", false)]
		public int MinMoney { get; private set; }

		// Token: 0x17000D02 RID: 3330
		// (get) Token: 0x06003197 RID: 12695 RVA: 0x00204A80 File Offset: 0x00202C80
		// (set) Token: 0x06003198 RID: 12696 RVA: 0x00204A88 File Offset: 0x00202C88
		[Serialize(0, IsPropertySaveable.No, "", "", false)]
		public int MaxMoney { get; private set; }

		// Token: 0x06003199 RID: 12697 RVA: 0x00204A91 File Offset: 0x00202C91
		public CorpsePrefab(ContentXElement element, CorpsesFile file) : base(element, file, Identifier.Empty)
		{
		}

		// Token: 0x0600319A RID: 12698 RVA: 0x00204AA0 File Offset: 0x00202CA0
		public static CorpsePrefab Random(Rand.RandSync sync = Rand.RandSync.Unsynced)
		{
			return CorpsePrefab.Prefabs.GetRandom(sync);
		}

		// Token: 0x040019CD RID: 6605
		public static readonly PrefabCollection<CorpsePrefab> Prefabs = new PrefabCollection<CorpsePrefab>();
	}
}
