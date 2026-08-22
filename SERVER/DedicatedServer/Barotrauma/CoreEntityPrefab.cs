using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000228 RID: 552
	internal class CoreEntityPrefab : MapEntityPrefab
	{
		// Token: 0x060025CF RID: 9679 RVA: 0x000F6028 File Offset: 0x000F4228
		private CoreEntityPrefab(Identifier identifier, ConstructorInfo constructor, bool resizeHorizontal = false, bool resizeVertical = false, bool linkable = false, IEnumerable<Identifier> allowedLinks = null, IEnumerable<string> aliases = null) : base(identifier)
		{
			this.constructor = constructor;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
			defaultInterpolatedStringHandler.AppendLiteral("EntityName.");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(identifier);
			this.Name = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(18, 1);
			defaultInterpolatedStringHandler2.AppendLiteral("EntityDescription.");
			defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(identifier);
			base.Description = TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear());
			base.ResizeHorizontal = resizeHorizontal;
			base.ResizeVertical = resizeVertical;
			base.Linkable = linkable;
			this.AllowedLinks = (allowedLinks ?? Enumerable.Empty<Identifier>()).ToImmutableHashSet<Identifier>();
			this.Aliases = (aliases ?? Enumerable.Empty<string>()).Concat(identifier.Value.ToEnumerable<string>()).ToImmutableHashSet<string>();
			base.Scale = 1f;
		}

		// Token: 0x17000AD0 RID: 2768
		// (get) Token: 0x060025D0 RID: 9680 RVA: 0x000F6112 File Offset: 0x000F4312
		// (set) Token: 0x060025D1 RID: 9681 RVA: 0x000F6119 File Offset: 0x000F4319
		public static CoreEntityPrefab HullPrefab { get; private set; }

		// Token: 0x17000AD1 RID: 2769
		// (get) Token: 0x060025D2 RID: 9682 RVA: 0x000F6121 File Offset: 0x000F4321
		// (set) Token: 0x060025D3 RID: 9683 RVA: 0x000F6128 File Offset: 0x000F4328
		public static CoreEntityPrefab GapPrefab { get; private set; }

		// Token: 0x17000AD2 RID: 2770
		// (get) Token: 0x060025D4 RID: 9684 RVA: 0x000F6130 File Offset: 0x000F4330
		// (set) Token: 0x060025D5 RID: 9685 RVA: 0x000F6137 File Offset: 0x000F4337
		public static CoreEntityPrefab WayPointPrefab { get; private set; }

		// Token: 0x17000AD3 RID: 2771
		// (get) Token: 0x060025D6 RID: 9686 RVA: 0x000F613F File Offset: 0x000F433F
		// (set) Token: 0x060025D7 RID: 9687 RVA: 0x000F6146 File Offset: 0x000F4346
		public static CoreEntityPrefab SpawnPointPrefab { get; private set; }

		// Token: 0x060025D8 RID: 9688 RVA: 0x000F6150 File Offset: 0x000F4350
		public static void InitCorePrefabs()
		{
			CoreEntityPrefab.HullPrefab = new CoreEntityPrefab("hull".ToIdentifier(), typeof(Hull).GetConstructor(new Type[]
			{
				typeof(Rectangle)
			}), true, true, true, new Identifier[]
			{
				"hull".ToIdentifier()
			}, null);
			CoreEntityPrefab.Prefabs.Add(CoreEntityPrefab.HullPrefab, false);
			CoreEntityPrefab.GapPrefab = new CoreEntityPrefab("gap".ToIdentifier(), typeof(Gap).GetConstructor(new Type[]
			{
				typeof(Rectangle)
			}), true, true, false, null, null);
			CoreEntityPrefab.Prefabs.Add(CoreEntityPrefab.GapPrefab, false);
			CoreEntityPrefab.WayPointPrefab = new CoreEntityPrefab("waypoint".ToIdentifier(), typeof(WayPoint).GetConstructor(new Type[]
			{
				typeof(MapEntityPrefab),
				typeof(Rectangle)
			}), false, false, false, null, null);
			CoreEntityPrefab.Prefabs.Add(CoreEntityPrefab.WayPointPrefab, false);
			CoreEntityPrefab.SpawnPointPrefab = new CoreEntityPrefab("spawnpoint".ToIdentifier(), typeof(WayPoint).GetConstructor(new Type[]
			{
				typeof(MapEntityPrefab),
				typeof(Rectangle)
			}), false, false, false, null, null);
			CoreEntityPrefab.Prefabs.Add(CoreEntityPrefab.SpawnPointPrefab, false);
		}

		// Token: 0x060025D9 RID: 9689 RVA: 0x000F62BC File Offset: 0x000F44BC
		protected override void CreateInstance(Rectangle rect)
		{
			if (this == CoreEntityPrefab.WayPointPrefab || this == CoreEntityPrefab.SpawnPointPrefab)
			{
				object[] lobject = new object[]
				{
					this,
					rect
				};
				this.constructor.Invoke(lobject);
				return;
			}
			object[] lobject2 = new object[]
			{
				rect
			};
			this.constructor.Invoke(lobject2);
		}

		// Token: 0x17000AD4 RID: 2772
		// (get) Token: 0x060025DA RID: 9690 RVA: 0x000F6318 File Offset: 0x000F4518
		public override Sprite Sprite
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000AD5 RID: 2773
		// (get) Token: 0x060025DB RID: 9691 RVA: 0x000F631B File Offset: 0x000F451B
		public override string OriginalName
		{
			get
			{
				return this.Name.Value;
			}
		}

		// Token: 0x17000AD6 RID: 2774
		// (get) Token: 0x060025DC RID: 9692 RVA: 0x000F6328 File Offset: 0x000F4528
		public override LocalizedString Name { get; }

		// Token: 0x17000AD7 RID: 2775
		// (get) Token: 0x060025DD RID: 9693 RVA: 0x000F6330 File Offset: 0x000F4530
		public override ImmutableHashSet<Identifier> Tags { get; } = Enumerable.Empty<Identifier>().ToImmutableHashSet<Identifier>();

		// Token: 0x17000AD8 RID: 2776
		// (get) Token: 0x060025DE RID: 9694 RVA: 0x000F6338 File Offset: 0x000F4538
		public override ImmutableHashSet<Identifier> AllowedLinks { get; }

		// Token: 0x17000AD9 RID: 2777
		// (get) Token: 0x060025DF RID: 9695 RVA: 0x000F6340 File Offset: 0x000F4540
		public override MapEntityCategory Category
		{
			get
			{
				return MapEntityCategory.Structure;
			}
		}

		// Token: 0x17000ADA RID: 2778
		// (get) Token: 0x060025E0 RID: 9696 RVA: 0x000F6343 File Offset: 0x000F4543
		public override ImmutableHashSet<string> Aliases { get; }

		// Token: 0x060025E1 RID: 9697 RVA: 0x000F634B File Offset: 0x000F454B
		public override void Dispose()
		{
			throw new InvalidOperationException("CoreEntityPrefab.Dispose should never be called");
		}

		// Token: 0x04001262 RID: 4706
		public static readonly PrefabCollection<CoreEntityPrefab> Prefabs = new PrefabCollection<CoreEntityPrefab>();

		// Token: 0x04001263 RID: 4707
		private readonly ConstructorInfo constructor;
	}
}
