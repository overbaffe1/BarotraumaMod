using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Items.Components;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200024C RID: 588
	internal abstract class MapEntity : Entity, ISpatialEntity
	{
		// Token: 0x17000C63 RID: 3171
		// (get) Token: 0x060029F5 RID: 10741 RVA: 0x0011315F File Offset: 0x0011135F
		// (set) Token: 0x060029F6 RID: 10742 RVA: 0x00113174 File Offset: 0x00111374
		[Editable]
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public string DisallowedUpgrades
		{
			get
			{
				return string.Join<Identifier>(",", this.DisallowedUpgradeSet);
			}
			set
			{
				this.DisallowedUpgradeSet.Clear();
				if (!string.IsNullOrWhiteSpace(value))
				{
					string[] splitTags = value.Split(',', StringSplitOptions.None);
					foreach (string tag in splitTags)
					{
						string[] splitTag = tag.Trim().Split(':', StringSplitOptions.None);
						this.DisallowedUpgradeSet.Add(string.Join(":", splitTag).ToIdentifier());
					}
				}
			}
		}

		// Token: 0x17000C64 RID: 3172
		// (get) Token: 0x060029F7 RID: 10743 RVA: 0x001131DF File Offset: 0x001113DF
		// (set) Token: 0x060029F8 RID: 10744 RVA: 0x001131E7 File Offset: 0x001113E7
		public bool FlippedX { get; protected set; }

		// Token: 0x17000C65 RID: 3173
		// (get) Token: 0x060029F9 RID: 10745 RVA: 0x001131F0 File Offset: 0x001113F0
		// (set) Token: 0x060029FA RID: 10746 RVA: 0x001131F8 File Offset: 0x001113F8
		public bool FlippedY { get; protected set; }

		// Token: 0x17000C66 RID: 3174
		// (get) Token: 0x060029FB RID: 10747 RVA: 0x00113201 File Offset: 0x00111401
		public static IEnumerable<MapEntity> HighlightedEntities
		{
			get
			{
				return MapEntity.highlightedEntities;
			}
		}

		// Token: 0x17000C67 RID: 3175
		// (get) Token: 0x060029FC RID: 10748 RVA: 0x00113208 File Offset: 0x00111408
		// (set) Token: 0x060029FD RID: 10749 RVA: 0x00113210 File Offset: 0x00111410
		public bool ExternalHighlight
		{
			get
			{
				return this.externalHighlight;
			}
			set
			{
				if (value != this.externalHighlight)
				{
					this.externalHighlight = value;
					this.CheckIsHighlighted();
				}
			}
		}

		// Token: 0x17000C68 RID: 3176
		// (get) Token: 0x060029FE RID: 10750 RVA: 0x00113228 File Offset: 0x00111428
		// (set) Token: 0x060029FF RID: 10751 RVA: 0x0011323A File Offset: 0x0011143A
		public bool IsHighlighted
		{
			get
			{
				return this.isHighlighted || this.ExternalHighlight;
			}
			set
			{
				if (value != this.isHighlighted)
				{
					this.isHighlighted = value;
					this.CheckIsHighlighted();
				}
			}
		}

		// Token: 0x17000C69 RID: 3177
		// (get) Token: 0x06002A00 RID: 10752 RVA: 0x00113252 File Offset: 0x00111452
		// (set) Token: 0x06002A01 RID: 10753 RVA: 0x0011325A File Offset: 0x0011145A
		public virtual float RotationRad { get; protected set; }

		// Token: 0x17000C6A RID: 3178
		// (get) Token: 0x06002A02 RID: 10754 RVA: 0x00113263 File Offset: 0x00111463
		public float RotationRadWithFlipping
		{
			get
			{
				if (!(this.FlippedX ^ this.FlippedY))
				{
					return this.RotationRad;
				}
				return -this.RotationRad;
			}
		}

		// Token: 0x17000C6B RID: 3179
		// (get) Token: 0x06002A03 RID: 10755 RVA: 0x00113282 File Offset: 0x00111482
		public float RotationWithFlipping
		{
			get
			{
				return MathHelper.ToDegrees(this.RotationRadWithFlipping);
			}
		}

		// Token: 0x17000C6C RID: 3180
		// (get) Token: 0x06002A04 RID: 10756 RVA: 0x0011328F File Offset: 0x0011148F
		// (set) Token: 0x06002A05 RID: 10757 RVA: 0x00113297 File Offset: 0x00111497
		public virtual Rectangle Rect
		{
			get
			{
				return this.rect;
			}
			set
			{
				this.rect = value;
			}
		}

		// Token: 0x17000C6D RID: 3181
		// (get) Token: 0x06002A06 RID: 10758 RVA: 0x001132A0 File Offset: 0x001114A0
		public Rectangle WorldRect
		{
			get
			{
				if (base.Submarine != null)
				{
					return new Rectangle((int)(base.Submarine.Position.X + (float)this.rect.X), (int)(base.Submarine.Position.Y + (float)this.rect.Y), this.rect.Width, this.rect.Height);
				}
				return this.rect;
			}
		}

		// Token: 0x17000C6E RID: 3182
		// (get) Token: 0x06002A07 RID: 10759 RVA: 0x00113313 File Offset: 0x00111513
		public virtual Sprite Sprite
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000C6F RID: 3183
		// (get) Token: 0x06002A08 RID: 10760 RVA: 0x00113316 File Offset: 0x00111516
		public virtual bool DrawBelowWater
		{
			get
			{
				return this.Sprite != null && this.SpriteDepth > 0.5f;
			}
		}

		// Token: 0x17000C70 RID: 3184
		// (get) Token: 0x06002A09 RID: 10761 RVA: 0x0011332F File Offset: 0x0011152F
		public virtual bool DrawOverWater
		{
			get
			{
				return !this.DrawBelowWater;
			}
		}

		// Token: 0x17000C71 RID: 3185
		// (get) Token: 0x06002A0A RID: 10762 RVA: 0x0011333A File Offset: 0x0011153A
		public virtual bool Linkable
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000C72 RID: 3186
		// (get) Token: 0x06002A0B RID: 10763 RVA: 0x00113340 File Offset: 0x00111540
		public IEnumerable<Identifier> AllowedLinks
		{
			get
			{
				if (this.Prefab != null)
				{
					return this.Prefab.AllowedLinks;
				}
				return Enumerable.Empty<Identifier>();
			}
		}

		// Token: 0x17000C73 RID: 3187
		// (get) Token: 0x06002A0C RID: 10764 RVA: 0x00113368 File Offset: 0x00111568
		public bool ResizeHorizontal
		{
			get
			{
				return this.Prefab != null && this.Prefab.ResizeHorizontal;
			}
		}

		// Token: 0x17000C74 RID: 3188
		// (get) Token: 0x06002A0D RID: 10765 RVA: 0x0011337F File Offset: 0x0011157F
		public bool ResizeVertical
		{
			get
			{
				return this.Prefab != null && this.Prefab.ResizeVertical;
			}
		}

		// Token: 0x17000C75 RID: 3189
		// (get) Token: 0x06002A0E RID: 10766 RVA: 0x00113396 File Offset: 0x00111596
		// (set) Token: 0x06002A0F RID: 10767 RVA: 0x001133A3 File Offset: 0x001115A3
		[Serialize(0, IsPropertySaveable.No, "", "", false)]
		public int RectWidth
		{
			get
			{
				return this.rect.Width;
			}
			set
			{
				if (value <= 0)
				{
					return;
				}
				this.Rect = new Rectangle(this.rect.X, this.rect.Y, value, this.rect.Height);
			}
		}

		// Token: 0x17000C76 RID: 3190
		// (get) Token: 0x06002A10 RID: 10768 RVA: 0x001133D7 File Offset: 0x001115D7
		// (set) Token: 0x06002A11 RID: 10769 RVA: 0x001133E4 File Offset: 0x001115E4
		[Serialize(0, IsPropertySaveable.No, "", "", false)]
		public int RectHeight
		{
			get
			{
				return this.rect.Height;
			}
			set
			{
				if (value <= 0)
				{
					return;
				}
				this.Rect = new Rectangle(this.rect.X, this.rect.Y, this.rect.Width, value);
			}
		}

		// Token: 0x17000C77 RID: 3191
		// (get) Token: 0x06002A12 RID: 10770 RVA: 0x00113418 File Offset: 0x00111618
		// (set) Token: 0x06002A13 RID: 10771 RVA: 0x00113420 File Offset: 0x00111620
		public bool SpriteDepthOverrideIsSet { get; private set; }

		// Token: 0x17000C78 RID: 3192
		// (get) Token: 0x06002A14 RID: 10772 RVA: 0x00113429 File Offset: 0x00111629
		public float SpriteOverrideDepth
		{
			get
			{
				return this.SpriteDepth;
			}
		}

		// Token: 0x17000C79 RID: 3193
		// (get) Token: 0x06002A15 RID: 10773 RVA: 0x00113431 File Offset: 0x00111631
		// (set) Token: 0x06002A16 RID: 10774 RVA: 0x0011345C File Offset: 0x0011165C
		[Editable(0.001f, 0.999f, 3)]
		[Serialize(float.NaN, IsPropertySaveable.Yes, "", "", false)]
		public float SpriteDepth
		{
			get
			{
				if (this.SpriteDepthOverrideIsSet)
				{
					return this._spriteOverrideDepth;
				}
				if (this.Sprite == null)
				{
					return 0f;
				}
				return this.Sprite.Depth;
			}
			set
			{
				if (!float.IsNaN(value))
				{
					this._spriteOverrideDepth = MathHelper.Clamp(value, 0.001f, 0.999999f);
					if (this is Item)
					{
						this._spriteOverrideDepth = Math.Min(this._spriteOverrideDepth, 0.9f);
					}
					this.SpriteDepthOverrideIsSet = true;
				}
			}
		}

		// Token: 0x17000C7A RID: 3194
		// (get) Token: 0x06002A17 RID: 10775 RVA: 0x001134AC File Offset: 0x001116AC
		// (set) Token: 0x06002A18 RID: 10776 RVA: 0x001134B4 File Offset: 0x001116B4
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(0.01f, 10f, 1, DecimalCount = 3, ValueStep = 0.1f)]
		public virtual float Scale { get; set; } = 1f;

		// Token: 0x17000C7B RID: 3195
		// (get) Token: 0x06002A19 RID: 10777 RVA: 0x001134BD File Offset: 0x001116BD
		// (set) Token: 0x06002A1A RID: 10778 RVA: 0x001134C5 File Offset: 0x001116C5
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool HiddenInGame { get; set; }

		// Token: 0x17000C7C RID: 3196
		// (get) Token: 0x06002A1B RID: 10779 RVA: 0x001134CE File Offset: 0x001116CE
		// (set) Token: 0x06002A1C RID: 10780 RVA: 0x001134D6 File Offset: 0x001116D6
		public bool IsLayerHidden { get; set; }

		// Token: 0x17000C7D RID: 3197
		// (get) Token: 0x06002A1D RID: 10781 RVA: 0x001134DF File Offset: 0x001116DF
		public bool IsHidden
		{
			get
			{
				return this.HiddenInGame || this.IsLayerHidden;
			}
		}

		// Token: 0x17000C7E RID: 3198
		// (get) Token: 0x06002A1E RID: 10782 RVA: 0x001134F4 File Offset: 0x001116F4
		public override Vector2 Position
		{
			get
			{
				Vector2 rectPos = new Vector2((float)this.rect.X + (float)this.rect.Width / 2f, (float)this.rect.Y - (float)this.rect.Height / 2f);
				return rectPos;
			}
		}

		// Token: 0x17000C7F RID: 3199
		// (get) Token: 0x06002A1F RID: 10783 RVA: 0x00113547 File Offset: 0x00111747
		public override Vector2 SimPosition
		{
			get
			{
				return ConvertUnits.ToSimUnits(this.Position);
			}
		}

		// Token: 0x17000C80 RID: 3200
		// (get) Token: 0x06002A20 RID: 10784 RVA: 0x00113554 File Offset: 0x00111754
		// (set) Token: 0x06002A21 RID: 10785 RVA: 0x0011356F File Offset: 0x0011176F
		public float SoundRange
		{
			get
			{
				if (this.aiTarget == null)
				{
					return 0f;
				}
				return this.aiTarget.SoundRange;
			}
			set
			{
				if (this.aiTarget == null)
				{
					return;
				}
				this.aiTarget.SoundRange = value;
			}
		}

		// Token: 0x17000C81 RID: 3201
		// (get) Token: 0x06002A22 RID: 10786 RVA: 0x00113586 File Offset: 0x00111786
		// (set) Token: 0x06002A23 RID: 10787 RVA: 0x001135A1 File Offset: 0x001117A1
		public float SightRange
		{
			get
			{
				if (this.aiTarget == null)
				{
					return 0f;
				}
				return this.aiTarget.SightRange;
			}
			set
			{
				if (this.aiTarget == null)
				{
					return;
				}
				this.aiTarget.SightRange = value;
			}
		}

		// Token: 0x17000C82 RID: 3202
		// (get) Token: 0x06002A24 RID: 10788 RVA: 0x001135B8 File Offset: 0x001117B8
		// (set) Token: 0x06002A25 RID: 10789 RVA: 0x001135C0 File Offset: 0x001117C0
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool RemoveIfLinkedOutpostDoorInUse { get; protected set; } = true;

		// Token: 0x17000C83 RID: 3203
		// (get) Token: 0x06002A26 RID: 10790 RVA: 0x001135C9 File Offset: 0x001117C9
		// (set) Token: 0x06002A27 RID: 10791 RVA: 0x001135D1 File Offset: 0x001117D1
		[Serialize("", IsPropertySaveable.Yes, "Submarine editor layer", "", false)]
		public string Layer { get; set; }

		// Token: 0x17000C84 RID: 3204
		// (get) Token: 0x06002A28 RID: 10792 RVA: 0x001135DA File Offset: 0x001117DA
		public virtual string Name
		{
			get
			{
				return "";
			}
		}

		// Token: 0x06002A29 RID: 10793 RVA: 0x001135E4 File Offset: 0x001117E4
		public MapEntity(MapEntityPrefab prefab, Submarine submarine, ushort id) : base(submarine, id)
		{
			this.Prefab = prefab;
			this.Scale = ((prefab != null) ? prefab.Scale : 1f);
		}

		// Token: 0x06002A2A RID: 10794 RVA: 0x0011366C File Offset: 0x0011186C
		protected void ParseLinks(XElement element, IdRemap idRemap)
		{
			string linkedToString = element.GetAttributeString("linked", "");
			if (!string.IsNullOrEmpty(linkedToString))
			{
				string[] linkedToIds = linkedToString.Split(',', StringSplitOptions.None);
				for (int i = 0; i < linkedToIds.Length; i++)
				{
					int srcId = int.Parse(linkedToIds[i]);
					int targetId = (int)idRemap.GetOffsetId(srcId);
					if (targetId <= 0)
					{
						if (this.unresolvedLinkedToID == null)
						{
							this.unresolvedLinkedToID = new List<ushort>();
						}
						this.unresolvedLinkedToID.Add((ushort)srcId);
					}
					else
					{
						this.linkedToID.Add((ushort)targetId);
					}
				}
			}
		}

		// Token: 0x06002A2B RID: 10795 RVA: 0x001136F4 File Offset: 0x001118F4
		public void ResolveLinks(IdRemap childRemap)
		{
			if (this.unresolvedLinkedToID == null)
			{
				return;
			}
			for (int i = 0; i < this.unresolvedLinkedToID.Count; i++)
			{
				int srcId = (int)this.unresolvedLinkedToID[i];
				int targetId = (int)childRemap.GetOffsetId(srcId);
				if (targetId > 0)
				{
					MapEntity otherEntity = Entity.FindEntityByID((ushort)targetId) as MapEntity;
					this.linkedTo.Add(otherEntity);
					if (otherEntity.Linkable && otherEntity.linkedTo != null)
					{
						otherEntity.linkedTo.Add(this);
					}
					this.unresolvedLinkedToID.RemoveAt(i);
					i--;
				}
			}
		}

		// Token: 0x06002A2C RID: 10796 RVA: 0x0011377E File Offset: 0x0011197E
		public virtual void Move(Vector2 amount, bool ignoreContacts = true)
		{
			this.rect.X = this.rect.X + (int)amount.X;
			this.rect.Y = this.rect.Y + (int)amount.Y;
		}

		// Token: 0x06002A2D RID: 10797 RVA: 0x001137AC File Offset: 0x001119AC
		public virtual bool IsMouseOn(Vector2 position)
		{
			return Submarine.RectContains(this.WorldRect, position, false);
		}

		// Token: 0x06002A2E RID: 10798 RVA: 0x001137BB File Offset: 0x001119BB
		public bool HasUpgrade(Identifier identifier)
		{
			return this.GetUpgrade(identifier) != null;
		}

		// Token: 0x06002A2F RID: 10799 RVA: 0x001137C8 File Offset: 0x001119C8
		public Upgrade GetUpgrade(Identifier identifier)
		{
			return this.Upgrades.Find(delegate(Upgrade upgrade)
			{
				Identifier identifier2 = upgrade.Identifier;
				return identifier2 == identifier;
			});
		}

		// Token: 0x06002A30 RID: 10800 RVA: 0x001137F9 File Offset: 0x001119F9
		public List<Upgrade> GetUpgrades()
		{
			return this.Upgrades;
		}

		// Token: 0x06002A31 RID: 10801 RVA: 0x00113804 File Offset: 0x00111A04
		public void SetUpgrade(Upgrade upgrade, bool createNetworkEvent = false)
		{
			Upgrade existingUpgrade = this.GetUpgrade(upgrade.Identifier);
			if (existingUpgrade != null)
			{
				existingUpgrade.Level = upgrade.Level;
				existingUpgrade.ApplyUpgrade();
				upgrade.Dispose();
			}
			else
			{
				this.AddUpgrade(upgrade, createNetworkEvent);
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(34, 4);
			defaultInterpolatedStringHandler.AppendLiteral("Set (ID: ");
			defaultInterpolatedStringHandler.AppendFormatted<ushort>(this.ID);
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.Prefab.Name);
			defaultInterpolatedStringHandler.AppendLiteral(")'s \"");
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(upgrade.Prefab.Name);
			defaultInterpolatedStringHandler.AppendLiteral("\" upgrade to level ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(upgrade.Level);
			DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x06002A32 RID: 10802 RVA: 0x001138C8 File Offset: 0x00111AC8
		public virtual bool AddUpgrade(Upgrade upgrade, bool createNetworkEvent = false)
		{
			if (!upgrade.Prefab.UpgradeCategories.Any((UpgradeCategory category) => category.CanBeApplied(this, upgrade.Prefab)))
			{
				return false;
			}
			if (this.DisallowedUpgradeSet.Contains(upgrade.Identifier))
			{
				return false;
			}
			Upgrade existingUpgrade = this.GetUpgrade(upgrade.Identifier);
			if (existingUpgrade != null)
			{
				existingUpgrade.Level += upgrade.Level;
				existingUpgrade.ApplyUpgrade();
				upgrade.Dispose();
			}
			else
			{
				upgrade.ApplyUpgrade();
				this.Upgrades.Add(upgrade);
			}
			return true;
		}

		// Token: 0x06002A33 RID: 10803 RVA: 0x00113985 File Offset: 0x00111B85
		protected virtual void CheckIsHighlighted()
		{
			if (this.IsHighlighted || this.ExternalHighlight)
			{
				MapEntity.highlightedEntities.Add(this);
				return;
			}
			MapEntity.highlightedEntities.Remove(this);
		}

		// Token: 0x06002A34 RID: 10804 RVA: 0x001139B0 File Offset: 0x00111BB0
		public static void ClearHighlightedEntities()
		{
			MapEntity.highlightedEntities.RemoveWhere((MapEntity e) => e.Removed);
			MapEntity.tempHighlightedEntities.Clear();
			MapEntity.tempHighlightedEntities.AddRange(MapEntity.highlightedEntities);
			foreach (MapEntity entity in MapEntity.tempHighlightedEntities)
			{
				entity.IsHighlighted = false;
			}
		}

		// Token: 0x06002A35 RID: 10805
		public abstract MapEntity Clone();

		// Token: 0x06002A36 RID: 10806 RVA: 0x00113A48 File Offset: 0x00111C48
		public static List<MapEntity> Clone(List<MapEntity> entitiesToClone)
		{
			List<MapEntity> clones = new List<MapEntity>();
			foreach (MapEntity e2 in entitiesToClone)
			{
				try
				{
					clones.Add(e2.Clone());
				}
				catch (Exception ex)
				{
					DebugConsole.ThrowError("Cloning entity \"" + e2.Name + "\" failed.", ex, null, false, false);
					GameAnalyticsManager.AddErrorEventOnce("MapEntity.Clone:" + e2.Name, GameAnalyticsManager.ErrorSeverity.Error, string.Concat(new string[]
					{
						"Cloning entity \"",
						e2.Name,
						"\" failed (",
						ex.Message,
						").\n",
						ex.StackTrace.CleanupStackTrace()
					}));
					return clones;
				}
			}
			for (int i = 0; i < clones.Count; i++)
			{
				if (entitiesToClone[i].linkedTo != null)
				{
					foreach (MapEntity linked in entitiesToClone[i].linkedTo)
					{
						if (entitiesToClone.Contains(linked))
						{
							clones[i].linkedTo.Add(clones[entitiesToClone.IndexOf(linked)]);
						}
					}
				}
			}
			List<Wire> orphanedWires = new List<Wire>();
			for (int j = 0; j < clones.Count; j++)
			{
				Item cloneItem = clones[j] as Item;
				if (cloneItem != null)
				{
					Door door = cloneItem.GetComponent<Door>();
					if (door != null)
					{
						door.RefreshLinkedGap();
					}
					Wire cloneWire = cloneItem.GetComponent<Wire>();
					if (cloneWire != null)
					{
						Wire originalWire = ((Item)entitiesToClone[j]).GetComponent<Wire>();
						cloneWire.SetNodes(originalWire.GetNodes());
						Predicate<MapEntity> <>9__2;
						for (int k = 0; k < 2; k++)
						{
							if (originalWire.Connections[k] == null)
							{
								Predicate<MapEntity> match;
								if ((match = <>9__2) == null)
								{
									match = (<>9__2 = delegate(MapEntity e)
									{
										Item item2 = e as Item;
										if (item2 != null)
										{
											ConnectionPanel component = item2.GetComponent<ConnectionPanel>();
											return component != null && component.DisconnectedWires.Contains(originalWire);
										}
										return false;
									});
								}
								MapEntity disconnectedFrom = entitiesToClone.Find(match);
								if (disconnectedFrom != null)
								{
									int disconnectedFromIndex = entitiesToClone.IndexOf(disconnectedFrom);
									Item item = clones[disconnectedFromIndex] as Item;
									ConnectionPanel disconnectedFromClone = (item != null) ? item.GetComponent<ConnectionPanel>() : null;
									if (disconnectedFromClone != null)
									{
										disconnectedFromClone.DisconnectedWires.Add(cloneWire);
										if (cloneWire.Item.body != null)
										{
											cloneWire.Item.body.Enabled = false;
										}
										cloneWire.IsActive = false;
									}
								}
							}
							else
							{
								Item connectedItem = originalWire.Connections[k].Item;
								if (connectedItem != null && entitiesToClone.Contains(connectedItem))
								{
									int itemIndex = entitiesToClone.IndexOf(connectedItem);
									if (itemIndex < 0)
									{
										DebugConsole.ThrowError("Error while cloning wires - item \"" + connectedItem.Name + "\" was not found in entities to clone.", null, null, false, false);
										GameAnalyticsManager.AddErrorEventOnce("MapEntity.Clone:ConnectedNotFound" + connectedItem.ID.ToString(), GameAnalyticsManager.ErrorSeverity.Error, "Error while cloning wires - item \"" + connectedItem.Name + "\" was not found in entities to clone.");
									}
									else
									{
										int connectionIndex = connectedItem.Connections.IndexOf(originalWire.Connections[k]);
										if (connectionIndex < 0)
										{
											DebugConsole.ThrowError(string.Concat(new string[]
											{
												"Error while cloning wires - connection \"",
												originalWire.Connections[k].Name,
												"\" was not found in connected item \"",
												connectedItem.Name,
												"\"."
											}), null, null, false, false);
											GameAnalyticsManager.AddErrorEventOnce("MapEntity.Clone:ConnectionNotFound" + connectedItem.ID.ToString(), GameAnalyticsManager.ErrorSeverity.Error, string.Concat(new string[]
											{
												"Error while cloning wires - connection \"",
												originalWire.Connections[k].Name,
												"\" was not found in connected item \"",
												connectedItem.Name,
												"\"."
											}));
										}
										else
										{
											(clones[itemIndex] as Item).Connections[connectionIndex].TryAddLink(cloneWire);
											cloneWire.Connect((clones[itemIndex] as Item).Connections[connectionIndex], k, false, false);
										}
									}
								}
							}
						}
						if (originalWire.Connections.Any((Connection c) => c != null) && (cloneWire.Connections[0] == null || cloneWire.Connections[1] == null) && cloneItem.GetComponent<DockingPort>() == null && !clones.Any(delegate(MapEntity c)
						{
							Item item2 = c as Item;
							bool? flag;
							if (item2 == null)
							{
								flag = null;
							}
							else
							{
								ConnectionPanel component = item2.GetComponent<ConnectionPanel>();
								flag = ((component != null) ? new bool?(component.DisconnectedWires.Contains(cloneWire)) : null);
							}
							bool? flag2 = flag;
							return flag2.GetValueOrDefault();
						}))
						{
							orphanedWires.Add(cloneWire);
						}
					}
				}
			}
			foreach (Wire orphanedWire in orphanedWires)
			{
				orphanedWire.Item.Remove();
				clones.Remove(orphanedWire.Item);
			}
			return clones;
		}

		// Token: 0x06002A37 RID: 10807 RVA: 0x00113FD0 File Offset: 0x001121D0
		protected void InsertToList()
		{
			if (this.Sprite == null)
			{
				MapEntity.MapEntityList.Add(this);
				return;
			}
			int i = 0;
			Structure structure = this as Structure;
			if (structure != null && structure.DrawDamageEffect)
			{
				float drawDepth = structure.SpriteDepth;
				while (i < MapEntity.MapEntityList.Count)
				{
					Structure structure2 = MapEntity.MapEntityList[i] as Structure;
					float otherDrawDepth = (structure2 != null) ? structure2.SpriteDepth : 1f;
					if (otherDrawDepth < drawDepth)
					{
						break;
					}
					i++;
				}
				MapEntity.MapEntityList.Insert(i, this);
				return;
			}
			i = 0;
			while (i < MapEntity.MapEntityList.Count)
			{
				i++;
				MapEntity mapEntity = MapEntity.MapEntityList[i - 1];
				if (((mapEntity != null) ? mapEntity.Prefab : null) == this.Prefab)
				{
					MapEntity.MapEntityList.Insert(i, this);
					return;
				}
			}
			MapEntity.MapEntityList.Insert(i, this);
		}

		// Token: 0x06002A38 RID: 10808 RVA: 0x001140A0 File Offset: 0x001122A0
		public virtual void ShallowRemove()
		{
			base.Remove();
			MapEntity.MapEntityList.Remove(this);
			if (this.aiTarget != null)
			{
				this.aiTarget.Remove();
			}
		}

		// Token: 0x06002A39 RID: 10809 RVA: 0x001140C8 File Offset: 0x001122C8
		public override void Remove()
		{
			base.Remove();
			MapEntity.MapEntityList.Remove(this);
			if (this.aiTarget != null)
			{
				this.aiTarget.Remove();
				this.aiTarget = null;
			}
			if (this.linkedTo != null)
			{
				for (int i = this.linkedTo.Count - 1; i >= 0; i--)
				{
					this.linkedTo[i].RemoveLinked(this);
				}
				this.linkedTo.Clear();
			}
		}

		// Token: 0x06002A3A RID: 10810 RVA: 0x00114140 File Offset: 0x00112340
		public static void UpdateAll(float deltaTime, Camera cam)
		{
			MapEntity.mapEntityUpdateTick++;
			if (MapEntity.mapEntityUpdateTick % MapEntity.MapEntityUpdateInterval == 0)
			{
				foreach (Hull hull in Hull.HullList)
				{
					hull.Update(deltaTime * (float)MapEntity.MapEntityUpdateInterval, cam);
				}
				foreach (Structure structure in Structure.WallList)
				{
					structure.Update(deltaTime * (float)MapEntity.MapEntityUpdateInterval, cam);
				}
			}
			foreach (Gap gap in Gap.GapList)
			{
				gap.ResetWaterFlowThisFrame();
			}
			foreach (Gap gap2 in from g in Gap.GapList
			orderby Rand.Int(int.MaxValue, Rand.RandSync.Unsynced)
			select g)
			{
				gap2.Update(deltaTime, cam);
			}
			if (MapEntity.mapEntityUpdateTick % MapEntity.PoweredUpdateInterval == 0)
			{
				Powered.UpdatePower(deltaTime * (float)MapEntity.PoweredUpdateInterval);
			}
			Item.UpdatePendingConditionUpdates(deltaTime);
			if (MapEntity.mapEntityUpdateTick % MapEntity.MapEntityUpdateInterval == 0)
			{
				Item lastUpdatedItem = null;
				try
				{
					foreach (Item item in Item.ItemList)
					{
						if (!LuaCsSetup.Instance.Game.UpdatePriorityItems.Contains(item))
						{
							lastUpdatedItem = item;
							item.Update(deltaTime * (float)MapEntity.MapEntityUpdateInterval, cam);
						}
					}
				}
				catch (InvalidOperationException e)
				{
					GameAnalyticsManager.AddErrorEventOnce("MapEntity.UpdateAll:ItemUpdateInvalidOperation", GameAnalyticsManager.ErrorSeverity.Critical, "Error while updating item " + (((lastUpdatedItem != null) ? lastUpdatedItem.Name : null) ?? "null") + ": " + e.Message);
					throw new InvalidOperationException("Error while updating item " + (((lastUpdatedItem != null) ? lastUpdatedItem.Name : null) ?? "null"), e);
				}
			}
			foreach (Item item2 in LuaCsSetup.Instance.Game.UpdatePriorityItems)
			{
				if (!item2.Removed)
				{
					item2.Update(deltaTime, cam);
				}
			}
			if (MapEntity.mapEntityUpdateTick % MapEntity.MapEntityUpdateInterval == 0)
			{
				EntitySpawner spawner = Entity.Spawner;
				if (spawner == null)
				{
					return;
				}
				spawner.Update(true);
			}
		}

		// Token: 0x06002A3B RID: 10811 RVA: 0x00114428 File Offset: 0x00112628
		public virtual void Update(float deltaTime, Camera cam)
		{
		}

		// Token: 0x06002A3C RID: 10812 RVA: 0x0011442C File Offset: 0x0011262C
		public virtual void FlipX(bool relativeToSub, bool force = false)
		{
			this.FlippedX = !this.FlippedX;
			if (!relativeToSub || base.Submarine == null)
			{
				return;
			}
			Vector2 relative = this.WorldPosition - base.Submarine.WorldPosition;
			relative.Y = 0f;
			this.Move(-relative * 2f, true);
		}

		// Token: 0x06002A3D RID: 10813 RVA: 0x00114490 File Offset: 0x00112690
		public virtual void FlipY(bool relativeToSub, bool force = false)
		{
			this.FlippedY = !this.FlippedY;
			if (!relativeToSub || base.Submarine == null)
			{
				return;
			}
			Vector2 relative = this.WorldPosition - base.Submarine.WorldPosition;
			relative.X = 0f;
			this.Move(-relative * 2f, true);
		}

		// Token: 0x06002A3E RID: 10814 RVA: 0x001144F2 File Offset: 0x001126F2
		public virtual Quad2D GetTransformedQuad()
		{
			return Quad2D.FromSubmarineRectangle(this.rect);
		}

		// Token: 0x06002A3F RID: 10815 RVA: 0x00114504 File Offset: 0x00112704
		public static List<MapEntity> LoadAll(Submarine submarine, XElement parentElement, string filePath, int idOffset)
		{
			IdRemap idRemap = new IdRemap(parentElement, idOffset);
			bool containsHiddenContainers = false;
			bool hiddenContainerCreated = false;
			MTRandom hiddenContainerRNG = new MTRandom(ToolBox.StringToInt(submarine.Info.Name));
			foreach (XElement element in parentElement.Elements())
			{
				Identifier identifier2 = element.NameAsIdentifier();
				if (!(identifier2 != "Item"))
				{
					Identifier[] tags = element.GetAttributeIdentifierArray("tags", Array.Empty<Identifier>(), true);
					if (tags.Contains(Tags.HiddenItemContainer))
					{
						containsHiddenContainers = true;
						break;
					}
				}
			}
			List<MapEntity> entities = new List<MapEntity>();
			foreach (XElement element2 in parentElement.Elements())
			{
				string typeName = element2.Name.ToString();
				Type t;
				try
				{
					t = Type.GetType("Barotrauma." + typeName, true, true);
					if (t == null)
					{
						DebugConsole.ThrowError(string.Concat(new string[]
						{
							"Error in ",
							filePath,
							"! Could not find a entity of the type \"",
							typeName,
							"\"."
						}), null, null, false, false);
						continue;
					}
				}
				catch (Exception e)
				{
					DebugConsole.ThrowError(string.Concat(new string[]
					{
						"Error in ",
						filePath,
						"! Could not find a entity of the type \"",
						typeName,
						"\"."
					}), e, null, false, false);
					continue;
				}
				Identifier identifier = element2.GetAttributeIdentifier("identifier", "");
				Identifier replacementIdentifier = Identifier.Empty;
				if (t == typeof(Structure))
				{
					string name = element2.Attribute("name").Value;
					if (Structure.FindPrefab(name, identifier) == null)
					{
						ItemPrefab itemPrefab = ItemPrefab.Find(name, identifier);
						if (itemPrefab != null)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(120, 1);
							defaultInterpolatedStringHandler.AppendLiteral("Could not find a structure with the identifier ");
							defaultInterpolatedStringHandler.AppendFormatted<Identifier>(identifier);
							defaultInterpolatedStringHandler.AppendLiteral(", but there's a matching item with the identifier. Converting to an item.");
							DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
							t = typeof(Item);
						}
					}
				}
				else if (t == typeof(Item) && !containsHiddenContainers && identifier == "vent" && submarine.Info.Type == SubmarineType.Player && !submarine.Info.HasTag(SubmarineTag.Shuttle))
				{
					if (!hiddenContainerCreated)
					{
						DebugConsole.AddWarning("There are no hidden containers such as loose vents or loose panels in the submarine \"" + submarine.Info.Name + "\". Certain traitor events require these to function properly. Converting one of the vents to a loose vent...", null);
					}
					if (!hiddenContainerCreated || hiddenContainerRNG.NextDouble() < 0.2)
					{
						replacementIdentifier = "loosevent".ToIdentifier();
						containsHiddenContainers = true;
						hiddenContainerCreated = true;
					}
				}
				try
				{
					MethodInfo loadMethod = t.GetMethod("Load", new Type[]
					{
						typeof(ContentXElement),
						typeof(Submarine),
						typeof(IdRemap)
					});
					if (loadMethod == null)
					{
						string str = "Could not find the method \"Load\" in ";
						Type type = t;
						DebugConsole.ThrowError(str + ((type != null) ? type.ToString() : null) + ".", null, null, false, false);
					}
					else if (!loadMethod.ReturnType.IsSubclassOf(typeof(MapEntity)))
					{
						DebugConsole.ThrowError("Error loading entity of the type \"" + t.ToString() + "\" - load method does not return a valid map entity.", null, null, false, false);
					}
					else
					{
						ContentXElement newElement = element2.FromPackage(null);
						if (!replacementIdentifier.IsEmpty)
						{
							newElement.SetAttributeValue("identifier", replacementIdentifier.ToString());
						}
						object newEntity = loadMethod.Invoke(t, new object[]
						{
							newElement,
							submarine,
							idRemap
						});
						if (newEntity != null)
						{
							entities.Add((MapEntity)newEntity);
						}
					}
				}
				catch (TargetInvocationException e2)
				{
					string str2 = "Error while loading entity of the type ";
					Type type2 = t;
					DebugConsole.ThrowError(str2 + ((type2 != null) ? type2.ToString() : null) + ".", e2.InnerException, null, false, false);
				}
				catch (Exception e3)
				{
					string str3 = "Error while loading entity of the type ";
					Type type3 = t;
					DebugConsole.ThrowError(str3 + ((type3 != null) ? type3.ToString() : null) + ".", e3, null, false, false);
				}
			}
			return entities;
		}

		// Token: 0x06002A40 RID: 10816 RVA: 0x001149A4 File Offset: 0x00112BA4
		public static void MapLoaded(List<MapEntity> entities, bool updateHulls)
		{
			MapEntity.InitializeLoadedLinks(entities);
			List<LinkedSubmarine> linkedSubs = new List<LinkedSubmarine>();
			for (int i = 0; i < entities.Count; i++)
			{
				if (!entities[i].mapLoadedCalled && !entities[i].Removed)
				{
					LinkedSubmarine sub = entities[i] as LinkedSubmarine;
					if (sub != null)
					{
						linkedSubs.Add(sub);
					}
					else
					{
						entities[i].OnMapLoaded();
					}
				}
			}
			if (updateHulls)
			{
				Item.UpdateHulls();
				Gap.UpdateHulls();
			}
			entities.ForEach(delegate(MapEntity e)
			{
				e.mapLoadedCalled = true;
			});
			foreach (LinkedSubmarine linkedSub in linkedSubs)
			{
				linkedSub.OnMapLoaded();
			}
			MapEntity.CreateDroppedStacks(entities);
		}

		// Token: 0x06002A41 RID: 10817 RVA: 0x00114A8C File Offset: 0x00112C8C
		private static void CreateDroppedStacks(List<MapEntity> entities)
		{
			List<Item> itemsInStack = new List<Item>();
			for (int i = 0; i < entities.Count; i++)
			{
				Item item = entities[i] as Item;
				if (item != null && item.Prefab.MaxStackSize > 1)
				{
					PhysicsBody body = item.body;
					if (body != null && body.Enabled)
					{
						itemsInStack.Clear();
						itemsInStack.Add(item);
						for (int j = i + 1; j < entities.Count; j++)
						{
							Item item2 = entities[j] as Item;
							if (item2 != null && item.Prefab == item2.Prefab)
							{
								body = item2.body;
								if (body != null && body.Enabled && !item2.DroppedStack.Any<Item>() && Math.Abs(item.Position.X - item2.Position.X) <= 10f && Math.Abs(item.Position.Y - item2.Position.Y) <= 10f)
								{
									itemsInStack.Add(item2);
								}
							}
						}
						if (itemsInStack.Count > 1)
						{
							item.CreateDroppedStack(itemsInStack, true);
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(34, 2);
							defaultInterpolatedStringHandler.AppendLiteral("Merged x");
							defaultInterpolatedStringHandler.AppendFormatted<int>(itemsInStack.Count);
							defaultInterpolatedStringHandler.AppendLiteral(" of ");
							defaultInterpolatedStringHandler.AppendFormatted(item.Name);
							defaultInterpolatedStringHandler.AppendLiteral(" into a dropped stack.");
							DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
						}
					}
				}
			}
		}

		// Token: 0x06002A42 RID: 10818 RVA: 0x00114C18 File Offset: 0x00112E18
		public static void InitializeLoadedLinks(IEnumerable<MapEntity> entities)
		{
			foreach (MapEntity e in entities)
			{
				if (!e.mapLoadedCalled && e.linkedToID != null && e.linkedToID.Count != 0)
				{
					e.linkedTo.Clear();
					foreach (ushort i in e.linkedToID)
					{
						MapEntity linked = Entity.FindEntityByID(i) as MapEntity;
						if (linked != null)
						{
							e.linkedTo.Add(linked);
						}
					}
					e.linkedToID.Clear();
					WayPoint wayPoint = e as WayPoint;
					if (wayPoint != null)
					{
						wayPoint.InitializeLinks();
					}
				}
			}
		}

		// Token: 0x06002A43 RID: 10819 RVA: 0x00114D04 File Offset: 0x00112F04
		public virtual void OnMapLoaded()
		{
		}

		// Token: 0x06002A44 RID: 10820 RVA: 0x00114D06 File Offset: 0x00112F06
		public virtual XElement Save(XElement parentElement)
		{
			string str = "Saving entity ";
			Type type = base.GetType();
			DebugConsole.ThrowError(str + ((type != null) ? type.ToString() : null) + " failed.", null, null, false, false);
			return null;
		}

		// Token: 0x06002A45 RID: 10821 RVA: 0x00114D33 File Offset: 0x00112F33
		public void RemoveLinked(MapEntity e)
		{
			if (this.linkedTo == null)
			{
				return;
			}
			if (this.linkedTo.Contains(e))
			{
				this.linkedTo.Remove(e);
			}
		}

		// Token: 0x06002A46 RID: 10822 RVA: 0x00114D5C File Offset: 0x00112F5C
		public HashSet<T> GetLinkedEntities<T>(HashSet<T> list = null, int? maxDepth = null, Func<T, bool> filter = null) where T : MapEntity
		{
			list = (list ?? new HashSet<T>());
			int startDepth = 0;
			MapEntity.GetLinkedEntitiesRecursive<T>(this, list, ref startDepth, maxDepth, filter);
			return list;
		}

		// Token: 0x06002A47 RID: 10823 RVA: 0x00114D84 File Offset: 0x00112F84
		private static void GetLinkedEntitiesRecursive<T>(MapEntity mapEntity, HashSet<T> linkedTargets, ref int depth, int? maxDepth = null, Func<T, bool> filter = null) where T : MapEntity
		{
			int num = depth;
			int? num2 = maxDepth;
			if (num > num2.GetValueOrDefault() & num2 != null)
			{
				return;
			}
			foreach (MapEntity linkedEntity in mapEntity.linkedTo)
			{
				T linkedTarget = linkedEntity as T;
				if (linkedTarget != null && !linkedTargets.Contains(linkedTarget) && (filter == null || filter(linkedTarget)))
				{
					linkedTargets.Add(linkedTarget);
					depth++;
					MapEntity.GetLinkedEntitiesRecursive<T>(linkedEntity, linkedTargets, ref depth, maxDepth, filter);
				}
			}
		}

		// Token: 0x040014A8 RID: 5288
		public static readonly List<MapEntity> MapEntityList = new List<MapEntity>();

		// Token: 0x040014A9 RID: 5289
		public readonly MapEntityPrefab Prefab;

		// Token: 0x040014AA RID: 5290
		protected List<ushort> linkedToID;

		// Token: 0x040014AB RID: 5291
		public List<ushort> unresolvedLinkedToID;

		// Token: 0x040014AC RID: 5292
		public static int MapEntityUpdateInterval = 1;

		// Token: 0x040014AD RID: 5293
		public static int PoweredUpdateInterval = 1;

		// Token: 0x040014AE RID: 5294
		private static int mapEntityUpdateTick;

		// Token: 0x040014AF RID: 5295
		protected readonly List<Upgrade> Upgrades = new List<Upgrade>();

		// Token: 0x040014B0 RID: 5296
		public readonly HashSet<Identifier> DisallowedUpgradeSet = new HashSet<Identifier>();

		// Token: 0x040014B1 RID: 5297
		public readonly List<MapEntity> linkedTo = new List<MapEntity>();

		// Token: 0x040014B4 RID: 5300
		public bool ShouldBeSaved = true;

		// Token: 0x040014B5 RID: 5301
		protected Rectangle rect;

		// Token: 0x040014B6 RID: 5302
		protected static readonly HashSet<MapEntity> highlightedEntities = new HashSet<MapEntity>();

		// Token: 0x040014B7 RID: 5303
		private bool externalHighlight;

		// Token: 0x040014B8 RID: 5304
		private bool isHighlighted;

		// Token: 0x040014BB RID: 5307
		private float _spriteOverrideDepth = float.NaN;

		// Token: 0x040014C1 RID: 5313
		public int OriginalModuleIndex = -1;

		// Token: 0x040014C2 RID: 5314
		public int OriginalContainerIndex = -1;

		// Token: 0x040014C3 RID: 5315
		private static readonly List<MapEntity> tempHighlightedEntities = new List<MapEntity>();

		// Token: 0x040014C4 RID: 5316
		private bool mapLoadedCalled;
	}
}
