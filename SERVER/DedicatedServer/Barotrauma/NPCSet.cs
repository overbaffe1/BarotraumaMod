using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000254 RID: 596
	[NullableContext(1)]
	[Nullable(0)]
	internal class NPCSet : Prefab
	{
		// Token: 0x06002AA9 RID: 10921 RVA: 0x00115C30 File Offset: 0x00113E30
		public NPCSet(ContentXElement element, NPCSetsFile file) : base(file, element.GetAttributeIdentifier("identifier", ""))
		{
			NPCSet <>4__this = this;
			this.Humans = (from npcElement in element.Elements()
			select new HumanPrefab(npcElement, file, <>4__this.Identifier)).ToImmutableArray<HumanPrefab>();
		}

		// Token: 0x06002AAA RID: 10922 RVA: 0x00115C90 File Offset: 0x00113E90
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

		// Token: 0x06002AAB RID: 10923 RVA: 0x00115D3F File Offset: 0x00113F3F
		public override void Dispose()
		{
		}

		// Token: 0x040014F4 RID: 5364
		public static readonly PrefabCollection<NPCSet> Sets = new PrefabCollection<NPCSet>();

		// Token: 0x040014F5 RID: 5365
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public readonly ImmutableArray<HumanPrefab> Humans;
	}
}
