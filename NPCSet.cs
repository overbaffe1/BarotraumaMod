using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200032B RID: 811
	[NullableContext(1)]
	[Nullable(0)]
	internal class NPCSet : Prefab
	{
		// Token: 0x060040AB RID: 16555 RVA: 0x0023DF88 File Offset: 0x0023C188
		public NPCSet(ContentXElement element, NPCSetsFile file) : base(file, element.GetAttributeIdentifier("identifier", ""))
		{
			NPCSet <>4__this = this;
			this.Humans = (from npcElement in element.Elements()
			select new HumanPrefab(npcElement, file, <>4__this.Identifier)).ToImmutableArray<HumanPrefab>();
		}

		// Token: 0x060040AC RID: 16556 RVA: 0x0023DFE8 File Offset: 0x0023C1E8
		[NullableContext(2)]
		public static HumanPrefab Get(Identifier setIdentifier, Identifier npcidentifier, bool logError = true, ContentPackage contentPackageToLogInError = null)
		{
			Func<HumanPrefab, bool> <>9__2;
			HumanPrefab prefab = (from set in NPCSet.Sets
			where set.Identifier == setIdentifier
			select set).SelectMany(delegate(NPCSet npcSet)
			{
				ImmutableArray<HumanPrefab> humans = npcSet.Humans;
				Func<HumanPrefab, bool> predicate;
				if ((predicate = <>9__2) == null)
				{
					predicate = (<>9__2 = ((HumanPrefab npcSetHuman) => npcSetHuman.Identifier == npcidentifier));
				}
				return humans.Where(predicate);
			}).FirstOrDefault<HumanPrefab>();
			if (prefab == null)
			{
				if (logError)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(39, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Could not find human prefab \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(npcidentifier);
					defaultInterpolatedStringHandler.AppendLiteral("\" from \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(setIdentifier);
					defaultInterpolatedStringHandler.AppendLiteral("\".");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, contentPackageToLogInError, false, false);
				}
				return null;
			}
			return prefab;
		}

		// Token: 0x060040AD RID: 16557 RVA: 0x0023E097 File Offset: 0x0023C297
		public override void Dispose()
		{
		}

		// Token: 0x040021B7 RID: 8631
		public static readonly PrefabCollection<NPCSet> Sets = new PrefabCollection<NPCSet>();

		// Token: 0x040021B8 RID: 8632
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public readonly ImmutableArray<HumanPrefab> Humans;
	}
}
