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
	// Token: 0x02000310 RID: 784
	internal class CoreEntityPrefab : MapEntityPrefab
	{
		// Token: 0x06003EB6 RID: 16054 RVA: 0x002339E0 File Offset: 0x00231BE0
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

		// Token: 0x17001068 RID: 4200
		// (get) Token: 0x06003EB7 RID: 16055 RVA: 0x00233ACA File Offset: 0x00231CCA
		// (set) Token: 0x06003EB8 RID: 16056 RVA: 0x00233AD1 File Offset: 0x00231CD1
		public static CoreEntityPrefab HullPrefab { get; private set; }

		// Token: 0x17001069 RID: 4201
		// (get) Token: 0x06003EB9 RID: 16057 RVA: 0x00233AD9 File Offset: 0x00231CD9
		// (set) Token: 0x06003EBA RID: 16058 RVA: 0x00233AE0 File Offset: 0x00231CE0
		public static CoreEntityPrefab GapPrefab { get; private set; }

		// Token: 0x1700106A RID: 4202
		// (get) Token: 0x06003EBB RID: 16059 RVA: 0x00233AE8 File Offset: 0x00231CE8
		// (set) Token: 0x06003EBC RID: 16060 RVA: 0x00233AEF File Offset: 0x00231CEF
		public static CoreEntityPrefab WayPointPrefab { get; private set; }

		// Token: 0x1700106B RID: 4203
		// (get) Token: 0x06003EBD RID: 16061 RVA: 0x00233AF7 File Offset: 0x00231CF7
		// (set) Token: 0x06003EBE RID: 16062 RVA: 0x00233AFE File Offset: 0x00231CFE
		public static CoreEntityPrefab SpawnPointPrefab { get; private set; }

		// Token: 0x06003EBF RID: 16063 RVA: 0x00233B08 File Offset: 0x00231D08
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

		// Token: 0x06003EC0 RID: 16064 RVA: 0x00233C74 File Offset: 0x00231E74
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

		// Token: 0x1700106C RID: 4204
		// (get) Token: 0x06003EC1 RID: 16065 RVA: 0x00233CD0 File Offset: 0x00231ED0
		public override Sprite Sprite
		{
			get
			{
				return null;
			}
		}

		// Token: 0x1700106D RID: 4205
		// (get) Token: 0x06003EC2 RID: 16066 RVA: 0x00233CD3 File Offset: 0x00231ED3
		public override string OriginalName
		{
			get
			{
				return this.Name.Value;
			}
		}

		// Token: 0x1700106E RID: 4206
		// (get) Token: 0x06003EC3 RID: 16067 RVA: 0x00233CE0 File Offset: 0x00231EE0
		public override LocalizedString Name { get; }

		// Token: 0x1700106F RID: 4207
		// (get) Token: 0x06003EC4 RID: 16068 RVA: 0x00233CE8 File Offset: 0x00231EE8
		public override ImmutableHashSet<Identifier> Tags { get; } = Enumerable.Empty<Identifier>().ToImmutableHashSet<Identifier>();

		// Token: 0x17001070 RID: 4208
		// (get) Token: 0x06003EC5 RID: 16069 RVA: 0x00233CF0 File Offset: 0x00231EF0
		public override ImmutableHashSet<Identifier> AllowedLinks { get; }

		// Token: 0x17001071 RID: 4209
		// (get) Token: 0x06003EC6 RID: 16070 RVA: 0x00233CF8 File Offset: 0x00231EF8
		public override MapEntityCategory Category
		{
			get
			{
				return MapEntityCategory.Structure;
			}
		}

		// Token: 0x17001072 RID: 4210
		// (get) Token: 0x06003EC7 RID: 16071 RVA: 0x00233CFB File Offset: 0x00231EFB
		public override ImmutableHashSet<string> Aliases { get; }

		// Token: 0x06003EC8 RID: 16072 RVA: 0x00233D03 File Offset: 0x00231F03
		public override void Dispose()
		{
			throw new InvalidOperationException("CoreEntityPrefab.Dispose should never be called");
		}

		// Token: 0x0400208E RID: 8334
		public static readonly PrefabCollection<CoreEntityPrefab> Prefabs = new PrefabCollection<CoreEntityPrefab>();

		// Token: 0x0400208F RID: 8335
		private readonly ConstructorInfo constructor;
	}
}
