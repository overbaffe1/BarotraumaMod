using System;
using System.Collections.Immutable;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200031D RID: 797
	internal class LinkedSubmarinePrefab : MapEntityPrefab
	{
		// Token: 0x06003F4F RID: 16207 RVA: 0x00236F9F File Offset: 0x0023519F
		public override void Dispose()
		{
		}

		// Token: 0x170010A1 RID: 4257
		// (get) Token: 0x06003F50 RID: 16208 RVA: 0x00236FA1 File Offset: 0x002351A1
		public override Sprite Sprite
		{
			get
			{
				return null;
			}
		}

		// Token: 0x170010A2 RID: 4258
		// (get) Token: 0x06003F51 RID: 16209 RVA: 0x00236FA4 File Offset: 0x002351A4
		public override string OriginalName
		{
			get
			{
				return this.Name.Value;
			}
		}

		// Token: 0x170010A3 RID: 4259
		// (get) Token: 0x06003F52 RID: 16210 RVA: 0x00236FB1 File Offset: 0x002351B1
		public override LocalizedString Name
		{
			get
			{
				return this.subInfo.Name;
			}
		}

		// Token: 0x170010A4 RID: 4260
		// (get) Token: 0x06003F53 RID: 16211 RVA: 0x00236FC3 File Offset: 0x002351C3
		public override ImmutableHashSet<Identifier> Tags
		{
			get
			{
				return null;
			}
		}

		// Token: 0x170010A5 RID: 4261
		// (get) Token: 0x06003F54 RID: 16212 RVA: 0x00236FC6 File Offset: 0x002351C6
		public override ImmutableHashSet<Identifier> AllowedLinks
		{
			get
			{
				return null;
			}
		}

		// Token: 0x170010A6 RID: 4262
		// (get) Token: 0x06003F55 RID: 16213 RVA: 0x00236FC9 File Offset: 0x002351C9
		public override MapEntityCategory Category
		{
			get
			{
				return MapEntityCategory.Misc;
			}
		}

		// Token: 0x170010A7 RID: 4263
		// (get) Token: 0x06003F56 RID: 16214 RVA: 0x00236FD0 File Offset: 0x002351D0
		public override ImmutableHashSet<string> Aliases { get; }

		// Token: 0x06003F57 RID: 16215 RVA: 0x00236FD8 File Offset: 0x002351D8
		public LinkedSubmarinePrefab(SubmarineInfo subInfo) : base(subInfo.Name.ToIdentifier())
		{
			this.subInfo = subInfo;
			this.Aliases = this.Name.Value.ToEnumerable<string>().ToImmutableHashSet<string>();
		}

		// Token: 0x06003F58 RID: 16216 RVA: 0x00237010 File Offset: 0x00235210
		protected override void CreateInstance(Rectangle rect)
		{
			LinkedSubmarine.CreateDummy(Submarine.MainSub, this.subInfo.FilePath, rect.Location.ToVector2());
		}

		// Token: 0x040020F4 RID: 8436
		public readonly SubmarineInfo subInfo;
	}
}
