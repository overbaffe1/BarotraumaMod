using System;
using System.Collections.Immutable;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000241 RID: 577
	internal class LinkedSubmarinePrefab : MapEntityPrefab
	{
		// Token: 0x06002884 RID: 10372 RVA: 0x00106703 File Offset: 0x00104903
		public override void Dispose()
		{
		}

		// Token: 0x17000BE2 RID: 3042
		// (get) Token: 0x06002885 RID: 10373 RVA: 0x00106705 File Offset: 0x00104905
		public override Sprite Sprite
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000BE3 RID: 3043
		// (get) Token: 0x06002886 RID: 10374 RVA: 0x00106708 File Offset: 0x00104908
		public override string OriginalName
		{
			get
			{
				return this.Name.Value;
			}
		}

		// Token: 0x17000BE4 RID: 3044
		// (get) Token: 0x06002887 RID: 10375 RVA: 0x00106715 File Offset: 0x00104915
		public override LocalizedString Name
		{
			get
			{
				return this.subInfo.Name;
			}
		}

		// Token: 0x17000BE5 RID: 3045
		// (get) Token: 0x06002888 RID: 10376 RVA: 0x00106727 File Offset: 0x00104927
		public override ImmutableHashSet<Identifier> Tags
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000BE6 RID: 3046
		// (get) Token: 0x06002889 RID: 10377 RVA: 0x0010672A File Offset: 0x0010492A
		public override ImmutableHashSet<Identifier> AllowedLinks
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000BE7 RID: 3047
		// (get) Token: 0x0600288A RID: 10378 RVA: 0x0010672D File Offset: 0x0010492D
		public override MapEntityCategory Category
		{
			get
			{
				return MapEntityCategory.Misc;
			}
		}

		// Token: 0x17000BE8 RID: 3048
		// (get) Token: 0x0600288B RID: 10379 RVA: 0x00106734 File Offset: 0x00104934
		public override ImmutableHashSet<string> Aliases { get; }

		// Token: 0x0600288C RID: 10380 RVA: 0x0010673C File Offset: 0x0010493C
		public LinkedSubmarinePrefab(SubmarineInfo subInfo) : base(subInfo.Name.ToIdentifier())
		{
			this.subInfo = subInfo;
			this.Aliases = this.Name.Value.ToEnumerable<string>().ToImmutableHashSet<string>();
		}

		// Token: 0x0600288D RID: 10381 RVA: 0x00106774 File Offset: 0x00104974
		protected override void CreateInstance(Rectangle rect)
		{
			LinkedSubmarine.CreateDummy(Submarine.MainSub, this.subInfo.FilePath, rect.Location.ToVector2());
		}

		// Token: 0x040013EE RID: 5102
		public readonly SubmarineInfo subInfo;
	}
}
