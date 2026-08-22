using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000040 RID: 64
	internal class Structure : MapEntity, IDamageable, IServerSerializable, INetSerializable, ISerializableEntity
	{
		// Token: 0x060009C9 RID: 2505 RVA: 0x00060B78 File Offset: 0x0005ED78
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			msg.WriteByte((byte)this.Sections.Length);
			for (int i = 0; i < this.Sections.Length; i++)
			{
				msg.WriteRangedSingle(this.Sections[i].damage / this.MaxHealth, 0f, 1f, 8);
			}
		}

		// Token: 0x17000295 RID: 661
		// (get) Token: 0x060009CA RID: 2506 RVA: 0x00060BCC File Offset: 0x0005EDCC
		public override ContentPackage ContentPackage
		{
			get
			{
				StructurePrefab prefab = this.Prefab;
				if (prefab == null)
				{
					return null;
				}
				return prefab.ContentPackage;
			}
		}

		// Token: 0x17000296 RID: 662
		// (get) Token: 0x060009CB RID: 2507 RVA: 0x00060BDF File Offset: 0x0005EDDF
		// (set) Token: 0x060009CC RID: 2508 RVA: 0x00060BE7 File Offset: 0x0005EDE7
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		[ConditionallyEditable(ConditionallyEditable.ConditionType.HasBody, true)]
		public bool Indestructible { get; set; }

		// Token: 0x17000297 RID: 663
		// (get) Token: 0x060009CD RID: 2509 RVA: 0x00060BF0 File Offset: 0x0005EDF0
		// (set) Token: 0x060009CE RID: 2510 RVA: 0x00060BF8 File Offset: 0x0005EDF8
		public WallSection[] Sections { get; private set; }

		// Token: 0x17000298 RID: 664
		// (get) Token: 0x060009CF RID: 2511 RVA: 0x00060C01 File Offset: 0x0005EE01
		public override Sprite Sprite
		{
			get
			{
				return this.Prefab.Sprite;
			}
		}

		// Token: 0x17000299 RID: 665
		// (get) Token: 0x060009D0 RID: 2512 RVA: 0x00060C0E File Offset: 0x0005EE0E
		public bool IsPlatform
		{
			get
			{
				return this.Prefab.Platform;
			}
		}

		// Token: 0x1700029A RID: 666
		// (get) Token: 0x060009D1 RID: 2513 RVA: 0x00060C1B File Offset: 0x0005EE1B
		// (set) Token: 0x060009D2 RID: 2514 RVA: 0x00060C23 File Offset: 0x0005EE23
		public Direction StairDirection { get; private set; }

		// Token: 0x1700029B RID: 667
		// (get) Token: 0x060009D3 RID: 2515 RVA: 0x00060C2C File Offset: 0x0005EE2C
		public override string Name
		{
			get
			{
				return this.Prefab.Name.Value;
			}
		}

		// Token: 0x1700029C RID: 668
		// (get) Token: 0x060009D4 RID: 2516 RVA: 0x00060C3E File Offset: 0x0005EE3E
		public bool HasBody
		{
			get
			{
				return this.Prefab.Body && !this.DisableCollision;
			}
		}

		// Token: 0x1700029D RID: 669
		// (get) Token: 0x060009D5 RID: 2517 RVA: 0x00060C58 File Offset: 0x0005EE58
		// (set) Token: 0x060009D6 RID: 2518 RVA: 0x00060C60 File Offset: 0x0005EE60
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		[ConditionallyEditable(ConditionallyEditable.ConditionType.HasBodyByDefault, true)]
		public bool DisableCollision { get; set; }

		// Token: 0x1700029E RID: 670
		// (get) Token: 0x060009D7 RID: 2519 RVA: 0x00060C69 File Offset: 0x0005EE69
		// (set) Token: 0x060009D8 RID: 2520 RVA: 0x00060C71 File Offset: 0x0005EE71
		public List<Body> Bodies { get; private set; }

		// Token: 0x1700029F RID: 671
		// (get) Token: 0x060009D9 RID: 2521 RVA: 0x00060C7A File Offset: 0x0005EE7A
		// (set) Token: 0x060009DA RID: 2522 RVA: 0x00060C82 File Offset: 0x0005EE82
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		[ConditionallyEditable(ConditionallyEditable.ConditionType.HasBody, true)]
		public bool CastShadow { get; set; }

		// Token: 0x170002A0 RID: 672
		// (get) Token: 0x060009DB RID: 2523 RVA: 0x00060C8B File Offset: 0x0005EE8B
		public bool IsHorizontal { get; }

		// Token: 0x170002A1 RID: 673
		// (get) Token: 0x060009DC RID: 2524 RVA: 0x00060C93 File Offset: 0x0005EE93
		public int SectionCount
		{
			get
			{
				return this.Sections.Length;
			}
		}

		// Token: 0x170002A2 RID: 674
		// (get) Token: 0x060009DD RID: 2525 RVA: 0x00060CA0 File Offset: 0x0005EEA0
		// (set) Token: 0x060009DE RID: 2526 RVA: 0x00060CD0 File Offset: 0x0005EED0
		[Serialize(100f, IsPropertySaveable.Yes, "", "", false)]
		[ConditionallyEditable(ConditionallyEditable.ConditionType.HasBody, true, MinValueFloat = 0f)]
		public float MaxHealth
		{
			get
			{
				float? num = this.maxHealth;
				if (num == null)
				{
					return this.Prefab.Health;
				}
				return num.GetValueOrDefault();
			}
			set
			{
				this.maxHealth = new float?(value);
			}
		}

		// Token: 0x170002A3 RID: 675
		// (get) Token: 0x060009DF RID: 2527 RVA: 0x00060CDE File Offset: 0x0005EEDE
		// (set) Token: 0x060009E0 RID: 2528 RVA: 0x00060CE6 File Offset: 0x0005EEE6
		[Serialize(3500f, IsPropertySaveable.Yes, "", "", false)]
		public float CrushDepth
		{
			get
			{
				return this.crushDepth;
			}
			set
			{
				this.crushDepth = Math.Max(value, 3500f);
			}
		}

		// Token: 0x170002A4 RID: 676
		// (get) Token: 0x060009E1 RID: 2529 RVA: 0x00060CF9 File Offset: 0x0005EEF9
		public float Health
		{
			get
			{
				return this.MaxHealth;
			}
		}

		// Token: 0x170002A5 RID: 677
		// (get) Token: 0x060009E2 RID: 2530 RVA: 0x00060D01 File Offset: 0x0005EF01
		public override bool DrawBelowWater
		{
			get
			{
				return base.DrawBelowWater || this.Prefab.BackgroundSprite != null;
			}
		}

		// Token: 0x170002A6 RID: 678
		// (get) Token: 0x060009E3 RID: 2531 RVA: 0x00060D1B File Offset: 0x0005EF1B
		public override bool DrawOverWater
		{
			get
			{
				return (this.Sprite == null || base.SpriteDepth <= 0.5f) && !this.DrawDamageEffect;
			}
		}

		// Token: 0x170002A7 RID: 679
		// (get) Token: 0x060009E4 RID: 2532 RVA: 0x00060D3D File Offset: 0x0005EF3D
		public bool DrawDamageEffect
		{
			get
			{
				return this.Prefab.Body && !this.IsPlatform;
			}
		}

		// Token: 0x170002A8 RID: 680
		// (get) Token: 0x060009E5 RID: 2533 RVA: 0x00060D57 File Offset: 0x0005EF57
		// (set) Token: 0x060009E6 RID: 2534 RVA: 0x00060D5F File Offset: 0x0005EF5F
		public bool HasDamage { get; private set; }

		// Token: 0x170002A9 RID: 681
		// (get) Token: 0x060009E7 RID: 2535 RVA: 0x00060D68 File Offset: 0x0005EF68
		public new StructurePrefab Prefab
		{
			get
			{
				return this.Prefab as StructurePrefab;
			}
		}

		// Token: 0x170002AA RID: 682
		// (get) Token: 0x060009E8 RID: 2536 RVA: 0x00060D75 File Offset: 0x0005EF75
		public ImmutableHashSet<Identifier> Tags
		{
			get
			{
				return this.Prefab.Tags;
			}
		}

		// Token: 0x170002AB RID: 683
		// (get) Token: 0x060009E9 RID: 2537 RVA: 0x00060D82 File Offset: 0x0005EF82
		// (set) Token: 0x060009EA RID: 2538 RVA: 0x00060D8A File Offset: 0x0005EF8A
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public string SpecialTag { get; set; }

		// Token: 0x170002AC RID: 684
		// (get) Token: 0x060009EB RID: 2539 RVA: 0x00060D93 File Offset: 0x0005EF93
		// (set) Token: 0x060009EC RID: 2540 RVA: 0x00060D9B File Offset: 0x0005EF9B
		[Editable]
		[Serialize("1.0,1.0,1.0,1.0", IsPropertySaveable.Yes, "", "", false)]
		public Color SpriteColor
		{
			get
			{
				return this.spriteColor;
			}
			set
			{
				this.spriteColor = value;
			}
		}

		// Token: 0x170002AD RID: 685
		// (get) Token: 0x060009ED RID: 2541 RVA: 0x00060DA4 File Offset: 0x0005EFA4
		// (set) Token: 0x060009EE RID: 2542 RVA: 0x00060DAC File Offset: 0x0005EFAC
		[ConditionallyEditable(ConditionallyEditable.ConditionType.HasBody, true)]
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool UseDropShadow { get; private set; }

		// Token: 0x170002AE RID: 686
		// (get) Token: 0x060009EF RID: 2543 RVA: 0x00060DB5 File Offset: 0x0005EFB5
		// (set) Token: 0x060009F0 RID: 2544 RVA: 0x00060DBD File Offset: 0x0005EFBD
		[ConditionallyEditable(ConditionallyEditable.ConditionType.HasBody, true)]
		[Serialize("0,0", IsPropertySaveable.Yes, "The position of the drop shadow relative to the structure. If set to zero, the shadow is positioned automatically so that it points towards the sub's center of mass.", "", false)]
		public Vector2 DropShadowOffset { get; private set; }

		// Token: 0x170002AF RID: 687
		// (get) Token: 0x060009F1 RID: 2545 RVA: 0x00060DC6 File Offset: 0x0005EFC6
		// (set) Token: 0x060009F2 RID: 2546 RVA: 0x00060DD0 File Offset: 0x0005EFD0
		public override float Scale
		{
			get
			{
				return this.scale;
			}
			set
			{
				if (this.scale == value)
				{
					return;
				}
				this.scale = MathHelper.Clamp(value, 0.1f, 10f);
				float relativeScale = this.scale / this.Prefab.Scale;
				if (!base.ResizeHorizontal || !base.ResizeVertical)
				{
					int newWidth = Math.Max(base.ResizeHorizontal ? this.rect.Width : ((int)((float)this.defaultRect.Width * relativeScale)), 1);
					int newHeight = Math.Max(base.ResizeVertical ? this.rect.Height : ((int)((float)this.defaultRect.Height * relativeScale)), 1);
					this.Rect = new Rectangle(this.rect.X, this.rect.Y, newWidth, newHeight);
					if (this.StairDirection != Direction.None)
					{
						this.CreateStairBodies();
						return;
					}
					if (this.Sections != null)
					{
						this.UpdateSections();
					}
				}
			}
		}

		// Token: 0x170002B0 RID: 688
		// (get) Token: 0x060009F3 RID: 2547 RVA: 0x00060EB9 File Offset: 0x0005F0B9
		// (set) Token: 0x060009F4 RID: 2548 RVA: 0x00060EC6 File Offset: 0x0005F0C6
		[ConditionallyEditable(ConditionallyEditable.ConditionType.AllowRotating, true, DecimalCount = 3, ForceShowPlusMinusButtons = true, ValueStep = 0.1f)]
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float Rotation
		{
			get
			{
				return MathHelper.ToDegrees(this.RotationRad);
			}
			set
			{
				this.RotationRad = MathHelper.WrapAngle(MathHelper.ToRadians(value));
				if (this.StairDirection != Direction.None)
				{
					this.CreateStairBodies();
					return;
				}
				if (this.Prefab.Body)
				{
					this.CreateSections();
					this.UpdateSections();
				}
			}
		}

		// Token: 0x170002B1 RID: 689
		// (get) Token: 0x060009F5 RID: 2549 RVA: 0x00060F01 File Offset: 0x0005F101
		// (set) Token: 0x060009F6 RID: 2550 RVA: 0x00060F09 File Offset: 0x0005F109
		[Editable(DecimalCount = 3, MinValueFloat = 0.01f, MaxValueFloat = 10f, ValueStep = 0.1f)]
		[Serialize("1.0, 1.0", IsPropertySaveable.No, "", "", false)]
		public Vector2 TextureScale
		{
			get
			{
				return this.textureScale;
			}
			set
			{
				this.textureScale = new Vector2(MathHelper.Clamp(value.X, 0.01f, 10f), MathHelper.Clamp(value.Y, 0.01f, 10f));
			}
		}

		// Token: 0x170002B2 RID: 690
		// (get) Token: 0x060009F7 RID: 2551 RVA: 0x00060F40 File Offset: 0x0005F140
		// (set) Token: 0x060009F8 RID: 2552 RVA: 0x00060F48 File Offset: 0x0005F148
		public float ScaleWhenTextureOffsetSet { get; private set; } = 1f;

		// Token: 0x170002B3 RID: 691
		// (get) Token: 0x060009F9 RID: 2553 RVA: 0x00060F51 File Offset: 0x0005F151
		// (set) Token: 0x060009FA RID: 2554 RVA: 0x00060F5C File Offset: 0x0005F15C
		[Editable(ForceShowPlusMinusButtons = true, ValueStep = 1f)]
		[Serialize("0.0, 0.0", IsPropertySaveable.Yes, "", "", false)]
		public Vector2 TextureOffset
		{
			get
			{
				return this.textureOffset;
			}
			set
			{
				this.textureOffset = value;
				this.textureOffset.X = MathUtils.PositiveModulo(this.textureOffset.X, (float)this.Sprite.SourceRect.Width * this.TextureScale.X * this.Scale);
				this.textureOffset.Y = MathUtils.PositiveModulo(this.textureOffset.Y, (float)this.Sprite.SourceRect.Height * this.TextureScale.Y * this.Scale);
				this.ScaleWhenTextureOffsetSet = this.Scale;
			}
		}

		// Token: 0x170002B4 RID: 692
		// (get) Token: 0x060009FB RID: 2555 RVA: 0x00060FFA File Offset: 0x0005F1FA
		// (set) Token: 0x060009FC RID: 2556 RVA: 0x00061002 File Offset: 0x0005F202
		public Rectangle DefaultRect
		{
			get
			{
				return this.defaultRect;
			}
			set
			{
				this.defaultRect = value;
			}
		}

		// Token: 0x170002B5 RID: 693
		// (get) Token: 0x060009FD RID: 2557 RVA: 0x0006100B File Offset: 0x0005F20B
		// (set) Token: 0x060009FE RID: 2558 RVA: 0x00061014 File Offset: 0x0005F214
		public override Rectangle Rect
		{
			get
			{
				return base.Rect;
			}
			set
			{
				Rectangle oldRect = this.Rect;
				base.Rect = value;
				if (this.Prefab.Body)
				{
					this.CreateSections();
					this.UpdateSections();
					return;
				}
				if (this.Sections == null)
				{
					return;
				}
				foreach (WallSection sec in this.Sections)
				{
					Rectangle secRect = sec.rect;
					secRect.X -= oldRect.X;
					secRect.Y -= oldRect.Y;
					secRect.X *= value.Width;
					secRect.X /= oldRect.Width;
					secRect.Y *= value.Height;
					secRect.Y /= oldRect.Height;
					secRect.Width *= value.Width;
					secRect.Width /= oldRect.Width;
					secRect.Height *= value.Height;
					secRect.Height /= oldRect.Height;
					secRect.X += value.X;
					secRect.Y += value.Y;
					sec.rect = secRect;
				}
			}
		}

		// Token: 0x170002B6 RID: 694
		// (get) Token: 0x060009FF RID: 2559 RVA: 0x0006114D File Offset: 0x0005F34D
		public float BodyWidth
		{
			get
			{
				if (this.Prefab.BodyWidth <= 0f)
				{
					return (float)this.rect.Width;
				}
				return this.Prefab.BodyWidth * this.scale;
			}
		}

		// Token: 0x170002B7 RID: 695
		// (get) Token: 0x06000A00 RID: 2560 RVA: 0x00061180 File Offset: 0x0005F380
		public float BodyHeight
		{
			get
			{
				if (this.Prefab.BodyHeight <= 0f)
				{
					return (float)this.rect.Height;
				}
				return this.Prefab.BodyHeight * this.scale;
			}
		}

		// Token: 0x170002B8 RID: 696
		// (get) Token: 0x06000A01 RID: 2561 RVA: 0x000611B4 File Offset: 0x0005F3B4
		public float BodyRotation
		{
			get
			{
				float rotation = MathHelper.ToRadians(this.Prefab.BodyRotation) + this.RotationRad;
				if (this.IsHorizontal)
				{
					if (base.FlippedX)
					{
						rotation = -3.1415927f - rotation;
					}
					if (base.FlippedY)
					{
						rotation = -rotation;
					}
				}
				else
				{
					if (base.FlippedX)
					{
						rotation = -rotation;
					}
					if (base.FlippedY)
					{
						rotation = -3.1415927f - rotation;
					}
				}
				return MathHelper.WrapAngle(rotation);
			}
		}

		// Token: 0x170002B9 RID: 697
		// (get) Token: 0x06000A02 RID: 2562 RVA: 0x00061224 File Offset: 0x0005F424
		public Vector2 BodyOffset
		{
			get
			{
				Vector2 bodyOffset = this.Prefab.BodyOffset;
				if (this.RotationRad != 0f)
				{
					bodyOffset = MathUtils.RotatePoint(bodyOffset, -this.RotationRad);
				}
				if (base.FlippedX)
				{
					bodyOffset.X = -bodyOffset.X;
				}
				if (base.FlippedY)
				{
					bodyOffset.Y = -bodyOffset.Y;
				}
				return bodyOffset;
			}
		}

		// Token: 0x170002BA RID: 698
		// (get) Token: 0x06000A03 RID: 2563 RVA: 0x00061285 File Offset: 0x0005F485
		// (set) Token: 0x06000A04 RID: 2564 RVA: 0x0006128D File Offset: 0x0005F48D
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public bool NoAITarget { get; private set; }

		// Token: 0x170002BB RID: 699
		// (get) Token: 0x06000A05 RID: 2565 RVA: 0x00061296 File Offset: 0x0005F496
		// (set) Token: 0x06000A06 RID: 2566 RVA: 0x0006129E File Offset: 0x0005F49E
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

		// Token: 0x06000A07 RID: 2567 RVA: 0x000612A8 File Offset: 0x0005F4A8
		public override void Move(Vector2 amount, bool ignoreContacts = true)
		{
			if (!MathUtils.IsValid(amount))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(54, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Attempted to move a structure by an invalid amount (");
				defaultInterpolatedStringHandler.AppendFormatted<Vector2>(amount);
				defaultInterpolatedStringHandler.AppendLiteral(")\n");
				defaultInterpolatedStringHandler.AppendFormatted(Environment.StackTrace.CleanupStackTrace());
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return;
			}
			base.Move(amount, ignoreContacts);
			for (int i = 0; i < this.Sections.Length; i++)
			{
				Rectangle r = this.Sections[i].rect;
				r.X += (int)amount.X;
				r.Y += (int)amount.Y;
				this.Sections[i].rect = r;
			}
			if (this.Bodies != null)
			{
				Vector2 simAmount = ConvertUnits.ToSimUnits(amount);
				foreach (Body b in this.Bodies)
				{
					Vector2 pos = b.Position + simAmount;
					if (ignoreContacts)
					{
						b.SetTransformIgnoreContacts(ref pos, b.Rotation);
					}
					else
					{
						b.SetTransform(pos, b.Rotation);
					}
				}
			}
		}

		// Token: 0x06000A08 RID: 2568 RVA: 0x000613E8 File Offset: 0x0005F5E8
		public Structure(Rectangle rectangle, StructurePrefab sp, Submarine submarine, ushort id = 0, XElement element = null) : base(sp, submarine, id)
		{
			if (rectangle.Width == 0 || rectangle.Height == 0)
			{
				return;
			}
			this.defaultRect = rectangle;
			this.maxHealth = new float?(sp.Health);
			this.rect = rectangle;
			this.TextureScale = sp.TextureScale;
			this.spriteColor = this.Prefab.SpriteColor;
			if (sp.IsHorizontal != null)
			{
				this.IsHorizontal = sp.IsHorizontal.Value;
			}
			else if (base.ResizeHorizontal && !base.ResizeVertical)
			{
				this.IsHorizontal = 1;
			}
			else if (base.ResizeVertical && !base.ResizeHorizontal)
			{
				this.IsHorizontal = 0;
			}
			else
			{
				float width = (this.BodyWidth > 0f) ? this.BodyWidth : ((float)this.rect.Width);
				float height = (this.BodyHeight > 0f) ? this.BodyHeight : ((float)this.rect.Height);
				if (this.BodyWidth > 0f && this.BodyHeight > 0f)
				{
					this.IsHorizontal = (width > height);
				}
			}
			this.StairDirection = this.Prefab.StairDirection;
			this.NoAITarget = this.Prefab.NoAITarget;
			this.SerializableProperties = ((element != null) ? SerializableProperty.DeserializeProperties(this, element) : SerializableProperty.GetProperties(this));
			if (((element != null) ? element.GetAttribute("CastShadow", StringComparison.OrdinalIgnoreCase) : null) == null)
			{
				this.CastShadow = this.Prefab.CastShadow;
			}
			if (((element != null) ? element.GetAttribute("Indestructible", StringComparison.OrdinalIgnoreCase) : null) == null)
			{
				this.Indestructible = this.Prefab.ConfigElement.GetAttributeBool("Indestructible", false);
			}
			if (this.Prefab.Body)
			{
				Structure.WallList.Add(this);
			}
			if (this.HasBody)
			{
				this.Bodies = new List<Body>();
				this.CreateSections();
				this.UpdateSections();
			}
			else if (this.StairDirection != Direction.None)
			{
				this.CreateStairBodies();
			}
			if (this.Sections == null)
			{
				this.Sections = new WallSection[1];
				this.Sections[0] = new WallSection(this.rect, this, 0f);
			}
			if (this.aiTarget == null && this.HasBody && this.Tags.Contains("wall") && submarine != null && !submarine.Info.IsWreck && !this.NoAITarget)
			{
				this.aiTarget = new AITarget(this)
				{
					MinSightRange = 1000f,
					MaxSightRange = 4000f,
					MaxSoundRange = 0f
				};
			}
			base.InsertToList();
			DebugConsole.Log(string.Concat(new string[]
			{
				"Created ",
				this.Name,
				" (",
				this.ID.ToString(),
				")"
			}));
		}

		// Token: 0x06000A09 RID: 2569 RVA: 0x000616FD File Offset: 0x0005F8FD
		public override string ToString()
		{
			return this.Name;
		}

		// Token: 0x06000A0A RID: 2570 RVA: 0x00061708 File Offset: 0x0005F908
		public override MapEntity Clone()
		{
			Structure clone = new Structure(this.rect, this.Prefab, base.Submarine, 0, null)
			{
				defaultRect = this.defaultRect
			};
			foreach (KeyValuePair<Identifier, SerializableProperty> property in this.SerializableProperties)
			{
				if (property.Value.Attributes.OfType<Serialize>().Any<Serialize>())
				{
					clone.SerializableProperties[property.Key].TrySetValue(clone, property.Value.GetValue(this));
				}
			}
			if (base.FlippedX)
			{
				clone.FlipX(false, false);
			}
			if (base.FlippedY)
			{
				clone.FlipY(false, false);
			}
			return clone;
		}

		// Token: 0x06000A0B RID: 2571 RVA: 0x000617DC File Offset: 0x0005F9DC
		private void CreateStairBodies()
		{
			this.Bodies = new List<Body>();
			this.bodyDimensions.Clear();
			float stairAngle = MathHelper.ToRadians(Math.Min(this.Prefab.StairAngle, 75f));
			float bodyWidth = ConvertUnits.ToSimUnits((double)this.rect.Width / Math.Cos((double)stairAngle));
			float bodyHeight = ConvertUnits.ToSimUnits(10);
			float stairHeight = (float)this.rect.Width * (float)Math.Tan((double)stairAngle);
			Body newBody = GameMain.World.CreateRectangle(bodyWidth, bodyHeight, 1.5f, default(Vector2), 0f, BodyType.Static, Category.Cat1, Category.All, true);
			float rotationWithFlip = base.RotationRadWithFlipping;
			newBody.BodyType = BodyType.Static;
			Vector2 stairRectHeightDiff = new Vector2(0f, stairHeight / 2f - (float)this.rect.Height / 2f);
			stairRectHeightDiff = MathUtils.RotatePoint(stairRectHeightDiff, -rotationWithFlip);
			if (base.FlippedY)
			{
				stairRectHeightDiff = -stairRectHeightDiff;
			}
			Vector2 stairPos = new Vector2(this.Position.X, (float)this.rect.Y - (float)this.rect.Height / 2f) + stairRectHeightDiff;
			newBody.Rotation = ((this.StairDirection == Direction.Right) ? stairAngle : (-stairAngle)) - rotationWithFlip;
			newBody.CollisionCategories = Category.Cat4;
			newBody.Friction = 0.8f;
			newBody.UserData = this;
			newBody.Position = ConvertUnits.ToSimUnits(stairPos) + ConvertUnits.ToSimUnits(this.BodyOffset) * this.Scale;
			this.bodyDimensions.Add(newBody, new Vector2(bodyWidth, bodyHeight));
			this.Bodies.Add(newBody);
		}

		// Token: 0x06000A0C RID: 2572 RVA: 0x00061988 File Offset: 0x0005FB88
		private void CreateSections()
		{
			int xsections = 1;
			int ysections = 1;
			int width = this.rect.Width;
			int height = this.rect.Height;
			WallSection[] prevSections = null;
			if (this.Sections != null)
			{
				prevSections = this.Sections.ToArray<WallSection>();
			}
			if (!this.Prefab.Body)
			{
				if (base.FlippedX && this.IsHorizontal)
				{
					xsections = (int)Math.Ceiling((double)((float)this.rect.Width / (float)this.Prefab.Sprite.SourceRect.Width));
					width = this.Prefab.Sprite.SourceRect.Width;
				}
				else if (base.FlippedY && !this.IsHorizontal)
				{
					ysections = (int)Math.Ceiling((double)((float)this.rect.Height / (float)this.Prefab.Sprite.SourceRect.Height));
					width = this.Prefab.Sprite.SourceRect.Height;
				}
				else
				{
					xsections = 1;
					ysections = 1;
				}
				this.Sections = new WallSection[Math.Max(xsections, ysections)];
			}
			else if (this.IsHorizontal)
			{
				xsections = (this.rect.Width + 96 - 1) / 96;
				this.Sections = new WallSection[xsections];
				width = 96;
			}
			else
			{
				ysections = (this.rect.Height + 96 - 1) / 96;
				this.Sections = new WallSection[ysections];
				height = 96;
			}
			for (int x = 0; x < xsections; x++)
			{
				for (int y = 0; y < ysections; y++)
				{
					if (base.FlippedX || base.FlippedY)
					{
						Rectangle sectionRect = new Rectangle(base.FlippedX ? (this.rect.Right - (x + 1) * width) : (this.rect.X + x * width), base.FlippedY ? (this.rect.Y - this.rect.Height + (y + 1) * height) : (this.rect.Y - y * height), width, height);
						if (base.FlippedX)
						{
							int over = Math.Max(this.rect.X - sectionRect.X, 0);
							sectionRect.X += over;
							sectionRect.Width -= over;
						}
						else
						{
							sectionRect.Width -= (int)Math.Max((float)(sectionRect.Right - this.rect.Right), 0f);
						}
						if (base.FlippedY)
						{
							int over2 = Math.Max(sectionRect.Y - this.rect.Y, 0);
							sectionRect.Y -= over2;
							sectionRect.Height -= over2;
						}
						else
						{
							sectionRect.Height -= (int)Math.Max((float)(this.rect.Y - this.rect.Height - (sectionRect.Y - sectionRect.Height)), 0f);
						}
						int xIndex = (base.FlippedX && this.IsHorizontal) ? (xsections - 1 - x) : x;
						int yIndex = (base.FlippedY && !this.IsHorizontal) ? (ysections - 1 - y) : y;
						this.Sections[xIndex + yIndex] = new WallSection(sectionRect, this, 0f);
					}
					else
					{
						Rectangle sectionRect2 = new Rectangle(this.rect.X + x * width, this.rect.Y - y * height, width, height);
						sectionRect2.Width -= (int)Math.Max((float)(sectionRect2.Right - this.rect.Right), 0f);
						sectionRect2.Height -= (int)Math.Max((float)(this.rect.Y - this.rect.Height - (sectionRect2.Y - sectionRect2.Height)), 0f);
						this.Sections[x + y] = new WallSection(sectionRect2, this, 0f);
					}
				}
			}
			if (prevSections != null && this.Sections.Length == prevSections.Length)
			{
				for (int i = 0; i < this.Sections.Length; i++)
				{
					this.Sections[i].damage = prevSections[i].damage;
				}
			}
		}

		// Token: 0x06000A0D RID: 2573 RVA: 0x00061DBC File Offset: 0x0005FFBC
		private Rectangle GenerateMergedRect(List<WallSection> mergedSections)
		{
			if (this.IsHorizontal)
			{
				return new Rectangle(mergedSections.Min((WallSection x) => x.rect.Left), mergedSections.Max((WallSection x) => x.rect.Top), mergedSections.Sum((WallSection x) => x.rect.Width), mergedSections.First<WallSection>().rect.Height);
			}
			return new Rectangle(mergedSections.Min((WallSection x) => x.rect.Left), mergedSections.Max((WallSection x) => x.rect.Top), mergedSections.First<WallSection>().rect.Width, mergedSections.Sum((WallSection x) => x.rect.Height));
		}

		// Token: 0x06000A0E RID: 2574 RVA: 0x00061EE0 File Offset: 0x000600E0
		public override Quad2D GetTransformedQuad()
		{
			return Quad2D.FromSubmarineRectangle(this.rect).Rotated((base.FlippedX != base.FlippedY) ? this.RotationRad : (-this.RotationRad));
		}

		// Token: 0x06000A0F RID: 2575 RVA: 0x00061F24 File Offset: 0x00060124
		public static Structure GetAttachTarget(Vector2 worldPosition)
		{
			foreach (MapEntity mapEntity in MapEntity.MapEntityList)
			{
				Structure structure = mapEntity as Structure;
				if (structure != null && structure.Prefab.AllowAttachItems && (structure.Bodies == null || structure.Bodies.Count <= 0))
				{
					Rectangle worldRect = mapEntity.WorldRect;
					if (worldPosition.X >= (float)worldRect.X && worldPosition.X <= (float)worldRect.Right && worldPosition.Y <= (float)worldRect.Y && worldPosition.Y >= (float)(worldRect.Y - worldRect.Height))
					{
						return structure;
					}
				}
			}
			return null;
		}

		// Token: 0x06000A10 RID: 2576 RVA: 0x00061FF8 File Offset: 0x000601F8
		public override bool IsMouseOn(Vector2 position)
		{
			if (this.StairDirection == Direction.None)
			{
				Vector2 rectSize = this.rect.Size.ToVector2();
				if (this.BodyWidth > 0f)
				{
					rectSize.X = this.BodyWidth;
				}
				if (this.BodyHeight > 0f)
				{
					rectSize.Y = this.BodyHeight;
				}
				Vector2 bodyPos = this.WorldPosition + this.BodyOffset * this.Scale;
				Vector2 transformedMousePos = MathUtils.RotatePointAroundTarget(position, bodyPos, this.BodyRotation, true);
				return Math.Abs(transformedMousePos.X - bodyPos.X) < rectSize.X / 2f && Math.Abs(transformedMousePos.Y - bodyPos.Y) < rectSize.Y / 2f;
			}
			Vector2 transformedMousePos2 = MathUtils.RotatePointAroundTarget(position, base.WorldRect.Location.ToVector2() + base.WorldRect.Size.ToVector2().FlipY() * 0.5f, this.BodyRotation, true);
			if (!Submarine.RectContains(base.WorldRect, position, false))
			{
				return false;
			}
			if (this.StairDirection == Direction.Left)
			{
				return MathUtils.LineToPointDistanceSquared(new Vector2((float)base.WorldRect.X, (float)base.WorldRect.Y), new Vector2((float)base.WorldRect.Right, (float)(base.WorldRect.Y - base.WorldRect.Height)), transformedMousePos2) < 1600f;
			}
			return MathUtils.LineToPointDistanceSquared(new Vector2((float)base.WorldRect.X, (float)(base.WorldRect.Y - this.rect.Height)), new Vector2((float)base.WorldRect.Right, (float)base.WorldRect.Y), transformedMousePos2) < 1600f;
		}

		// Token: 0x06000A11 RID: 2577 RVA: 0x000621E4 File Offset: 0x000603E4
		public override void ShallowRemove()
		{
			base.ShallowRemove();
			if (Structure.WallList.Contains(this))
			{
				Structure.WallList.Remove(this);
			}
			if (this.Bodies != null)
			{
				foreach (Body b in this.Bodies)
				{
					GameMain.World.Remove(b);
				}
				this.Bodies.Clear();
			}
			if (this.Sections != null)
			{
				foreach (WallSection s in this.Sections)
				{
					if (s.gap != null)
					{
						s.gap.Remove();
						s.gap = null;
					}
				}
			}
		}

		// Token: 0x06000A12 RID: 2578 RVA: 0x000622AC File Offset: 0x000604AC
		public override void Remove()
		{
			base.Remove();
			if (Structure.WallList.Contains(this))
			{
				Structure.WallList.Remove(this);
			}
			if (this.Bodies != null)
			{
				foreach (Body b in this.Bodies)
				{
					GameMain.World.Remove(b);
				}
				this.Bodies.Clear();
			}
			if (this.Sections != null)
			{
				foreach (WallSection s in this.Sections)
				{
					if (s.gap != null)
					{
						s.gap.Remove();
						s.gap = null;
					}
				}
			}
		}

		// Token: 0x06000A13 RID: 2579 RVA: 0x00062374 File Offset: 0x00060574
		private bool OnWallCollision(Fixture f1, Fixture f2, Contact contact)
		{
			if (this.Prefab.Platform)
			{
				Limb limb = f2.Body.UserData as Limb;
				if (limb != null && limb.character.AnimController.IgnorePlatforms)
				{
					return false;
				}
			}
			if (f2.Body.UserData is Limb)
			{
				Character character = ((Limb)f2.Body.UserData).character;
				if (character.DisableImpactDamageTimer <= 0f)
				{
					float mass = ((Limb)f2.Body.UserData).Mass;
				}
				return true;
			}
			return true;
		}

		// Token: 0x06000A14 RID: 2580 RVA: 0x0006240A File Offset: 0x0006060A
		public WallSection GetSection(int sectionIndex)
		{
			if (sectionIndex < 0 || sectionIndex >= this.Sections.Length)
			{
				return null;
			}
			return this.Sections[sectionIndex];
		}

		// Token: 0x06000A15 RID: 2581 RVA: 0x00062425 File Offset: 0x00060625
		public bool SectionBodyDisabled(int sectionIndex)
		{
			return sectionIndex >= 0 && sectionIndex < this.Sections.Length && this.Sections[sectionIndex].damage >= this.MaxHealth;
		}

		// Token: 0x06000A16 RID: 2582 RVA: 0x00062450 File Offset: 0x00060650
		public bool AllSectionBodiesDisabled()
		{
			for (int i = 0; i < this.Sections.Length; i++)
			{
				if (this.Sections[i].damage < this.MaxHealth)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000A17 RID: 2583 RVA: 0x00062488 File Offset: 0x00060688
		public bool SectionIsLeaking(int sectionIndex)
		{
			return sectionIndex >= 0 && sectionIndex < this.Sections.Length && this.Sections[sectionIndex].damage >= this.MaxHealth * 0.1f;
		}

		// Token: 0x06000A18 RID: 2584 RVA: 0x000624BC File Offset: 0x000606BC
		public bool SectionIsLeakingFromOutside(int sectionIndex)
		{
			if (sectionIndex < 0 || sectionIndex >= this.Sections.Length)
			{
				return false;
			}
			if (this.SectionIsLeaking(sectionIndex))
			{
				Gap gap = this.Sections[sectionIndex].gap;
				return gap != null && !gap.IsRoomToRoom;
			}
			return false;
		}

		// Token: 0x06000A19 RID: 2585 RVA: 0x00062501 File Offset: 0x00060701
		public int SectionLength(int sectionIndex)
		{
			if (sectionIndex < 0 || sectionIndex >= this.Sections.Length)
			{
				return 0;
			}
			if (!this.IsHorizontal)
			{
				return this.Sections[sectionIndex].rect.Height;
			}
			return this.Sections[sectionIndex].rect.Width;
		}

		// Token: 0x06000A1A RID: 2586 RVA: 0x00062544 File Offset: 0x00060744
		public override bool AddUpgrade(Upgrade upgrade, bool createNetworkEvent = false)
		{
			if (!upgrade.Prefab.IsWallUpgrade)
			{
				return false;
			}
			Upgrade existingUpgrade = base.GetUpgrade(upgrade.Identifier);
			if (existingUpgrade != null)
			{
				existingUpgrade.Level += upgrade.Level;
				existingUpgrade.ApplyUpgrade();
				upgrade.Dispose();
			}
			else
			{
				this.Upgrades.Add(upgrade);
				upgrade.ApplyUpgrade();
			}
			this.UpdateSections();
			return true;
		}

		// Token: 0x06000A1B RID: 2587 RVA: 0x000625AC File Offset: 0x000607AC
		public void AddDamage(int sectionIndex, float damage, Character attacker = null, bool emitParticles = true, bool createWallDamageProjectiles = false)
		{
			if (!this.HasBody || this.Prefab.Platform || this.Indestructible)
			{
				return;
			}
			if (sectionIndex < 0 || sectionIndex > this.Sections.Length - 1)
			{
				return;
			}
			WallSection section = this.Sections[sectionIndex];
			float prevDamage = section.damage;
			if (GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer)
			{
				this.SetDamage(sectionIndex, section.damage + damage, attacker, true, true, true, createWallDamageProjectiles);
			}
		}

		// Token: 0x06000A1C RID: 2588 RVA: 0x00062624 File Offset: 0x00060824
		public int FindSectionIndex(Vector2 displayPos, bool world = false, bool clamp = false)
		{
			if (this.Sections.None(null))
			{
				return -1;
			}
			if (world && base.Submarine != null)
			{
				displayPos -= base.Submarine.Position;
			}
			if (this.IsHorizontal)
			{
				if (this.Sections[0].rect.Width < 96)
				{
					displayPos += this.DirectionUnit * (float)(96 - this.Sections[0].rect.Width);
				}
			}
			else if (this.Sections[0].rect.Height < 96)
			{
				displayPos += this.DirectionUnit * (float)(96 - this.Sections[0].rect.Height);
			}
			Vector2 leftmostPos = this.Position - this.DirectionUnit * (float)(this.IsHorizontal ? this.Rect.Width : this.Rect.Height) * 0.5f;
			int index = (int)Math.Floor((double)(Vector2.Dot(this.DirectionUnit, displayPos - leftmostPos) / 96f));
			if (clamp)
			{
				index = MathHelper.Clamp(index, 0, this.Sections.Length - 1);
			}
			else if (index < 0 || index > this.Sections.Length - 1)
			{
				return -1;
			}
			return index;
		}

		// Token: 0x06000A1D RID: 2589 RVA: 0x00062774 File Offset: 0x00060974
		public float SectionDamage(int sectionIndex)
		{
			if (sectionIndex < 0 || sectionIndex >= this.Sections.Length)
			{
				return 0f;
			}
			return this.Sections[sectionIndex].damage;
		}

		// Token: 0x170002BC RID: 700
		// (get) Token: 0x06000A1E RID: 2590 RVA: 0x00062798 File Offset: 0x00060998
		protected Vector2 DirectionUnit
		{
			get
			{
				float rotation = this.IsHorizontal ? (-this.BodyRotation) : (-1.5707964f - this.BodyRotation);
				if (this.IsHorizontal && base.FlippedX)
				{
					rotation += 3.1415927f;
				}
				if (!this.IsHorizontal && base.FlippedY)
				{
					rotation += 3.1415927f;
				}
				return MathUtils.RotatedUnitXRadians(rotation);
			}
		}

		// Token: 0x06000A1F RID: 2591 RVA: 0x000627FC File Offset: 0x000609FC
		public Vector2 SectionPosition(int sectionIndex, bool world = false)
		{
			if (sectionIndex < 0 || sectionIndex >= this.Sections.Length)
			{
				return Vector2.Zero;
			}
			if (MathUtils.NearlyEqual(this.BodyRotation, 0f, 0.0001f))
			{
				Vector2 sectionPos = new Vector2((float)this.Sections[sectionIndex].rect.X + (float)this.Sections[sectionIndex].rect.Width / 2f, (float)this.Sections[sectionIndex].rect.Y - (float)this.Sections[sectionIndex].rect.Height / 2f);
				if (world && base.Submarine != null)
				{
					sectionPos += base.Submarine.Position;
				}
				return sectionPos;
			}
			Rectangle sectionRect = this.Sections[sectionIndex].rect;
			float diffFromCenter;
			if (this.IsHorizontal)
			{
				diffFromCenter = (float)(sectionRect.Center.X - this.rect.Center.X) / (float)this.rect.Width * this.BodyWidth;
			}
			else
			{
				diffFromCenter = (float)(sectionRect.Y - sectionRect.Height / 2 - (this.rect.Y - this.rect.Height / 2)) / (float)this.rect.Height * this.BodyHeight;
				diffFromCenter = -diffFromCenter;
			}
			Vector2 sectionPos2 = this.Position + this.DirectionUnit * diffFromCenter;
			if (world && base.Submarine != null)
			{
				sectionPos2 += base.Submarine.Position;
			}
			return sectionPos2;
		}

		// Token: 0x06000A20 RID: 2592 RVA: 0x0006297C File Offset: 0x00060B7C
		public AttackResult AddDamage(Character attacker, Vector2 worldPosition, Attack attack, Vector2 impulseDirection, float deltaTime, bool playSound = false)
		{
			if (base.Submarine != null && base.Submarine.GodMode)
			{
				return new AttackResult(0f, null);
			}
			if (!this.HasBody || this.Prefab.Platform || this.Indestructible)
			{
				return new AttackResult(0f, null);
			}
			Vector2 transformedPos = worldPosition;
			if (base.Submarine != null)
			{
				transformedPos -= base.Submarine.Position;
			}
			if (!MathUtils.NearlyEqual(this.BodyRotation, 0f, 0.0001f))
			{
				Vector2 center = this.Rect.Location.ToVector2() + this.Rect.Size.ToVector2().FlipY() * 0.5f;
				float rotation = this.BodyRotation;
				if (this.IsHorizontal && base.FlippedX)
				{
					rotation += 3.1415927f;
				}
				if (!this.IsHorizontal && base.FlippedY)
				{
					rotation += 3.1415927f;
				}
				transformedPos = MathUtils.RotatePointAroundTarget(transformedPos, center, rotation, true);
			}
			float damageAmount = 0f;
			for (int i = 0; i < this.SectionCount; i++)
			{
				Rectangle sectionRect = this.Sections[i].rect;
				sectionRect.Y -= this.Sections[i].rect.Height;
				if (MathUtils.CircleIntersectsRectangle(transformedPos, attack.DamageRange, sectionRect))
				{
					damageAmount = attack.GetStructureDamage(deltaTime);
					this.AddDamage(i, damageAmount, attacker, true, attack.CreateWallDamageProjectiles);
				}
			}
			if (base.Submarine != null && damageAmount > 0f && attacker != null)
			{
				AbilityAttackerSubmarine abilityAttackerSubmarine = new AbilityAttackerSubmarine(attacker, base.Submarine);
				foreach (Character character in Character.CharacterList)
				{
					character.CheckTalents(AbilityEffectType.AfterSubmarineAttacked, abilityAttackerSubmarine);
				}
			}
			return new AttackResult(damageAmount, null);
		}

		// Token: 0x06000A21 RID: 2593 RVA: 0x00062B78 File Offset: 0x00060D78
		public void SetDamage(int sectionIndex, float damage, Character attacker = null, bool createNetworkEvent = true, bool isNetworkEvent = true, bool createExplosionEffect = true, bool createWallDamageProjectiles = false)
		{
			if ((base.Submarine != null && base.Submarine.GodMode) || (this.Indestructible && !isNetworkEvent))
			{
				return;
			}
			if (!this.HasBody)
			{
				return;
			}
			if (!MathUtils.IsValid(damage))
			{
				return;
			}
			damage = MathHelper.Clamp(damage, 0f, this.MaxHealth - this.Prefab.MinHealth);
			if (this.Sections[sectionIndex].NoPhysicsBody)
			{
				return;
			}
			if (GameMain.Server != null && createNetworkEvent && damage != this.Sections[sectionIndex].damage)
			{
				GameMain.Server.CreateEntityEvent(this, null);
			}
			bool noGaps = true;
			for (int i = 0; i < this.Sections.Length; i++)
			{
				if (i != sectionIndex && this.SectionIsLeaking(i))
				{
					noGaps = false;
					break;
				}
			}
			if (damage < this.MaxHealth * 0.1f)
			{
				if (this.Sections[sectionIndex].gap != null)
				{
					if (noGaps && attacker != null)
					{
						GameServer.Log((this.Sections[sectionIndex].gap.IsRoomToRoom ? "Inner" : "Outer") + " wall repaired by " + GameServer.CharacterLogName(attacker), ServerLog.MessageType.ItemInteraction);
					}
					DebugConsole.Log(string.Concat(new string[]
					{
						"Removing gap (ID ",
						this.Sections[sectionIndex].gap.ID.ToString(),
						", section: ",
						sectionIndex.ToString(),
						") from wall ",
						this.ID.ToString()
					}));
					this.Sections[sectionIndex].gap.Open = 0f;
					this.Sections[sectionIndex].gap.Remove();
					this.Sections[sectionIndex].gap = null;
				}
			}
			else
			{
				Screen selected = Screen.Selected;
				if (selected == null || !selected.IsEditor)
				{
					Gap gap2 = this.Sections[sectionIndex].gap;
					float prevGapOpenState = (gap2 != null) ? gap2.Open : 0f;
					if (this.Sections[sectionIndex].gap == null)
					{
						Rectangle gapRect = this.Sections[sectionIndex].rect;
						float diffFromCenter;
						if (this.IsHorizontal)
						{
							diffFromCenter = (float)(gapRect.Center.X - this.rect.Center.X) / (float)this.rect.Width * this.BodyWidth;
							if (this.BodyWidth > 0f)
							{
								gapRect.Width = (int)(this.BodyWidth * ((float)gapRect.Width / (float)this.rect.Width));
							}
							if (this.BodyHeight > 0f)
							{
								gapRect.Y = gapRect.Y - gapRect.Height / 2 + (int)(this.BodyHeight / 2f + this.BodyOffset.Y * this.scale);
								gapRect.Height = (int)this.BodyHeight;
							}
							if (base.FlippedX)
							{
								diffFromCenter = -diffFromCenter;
							}
						}
						else
						{
							diffFromCenter = (float)(gapRect.Y - gapRect.Height / 2 - (this.rect.Y - this.rect.Height / 2)) / (float)this.rect.Height * this.BodyHeight;
							if (this.BodyWidth > 0f)
							{
								gapRect.X = gapRect.Center.X + (int)(-this.BodyWidth / 2f + this.BodyOffset.X * this.scale);
								gapRect.Width = (int)this.BodyWidth;
							}
							if (this.BodyHeight > 0f)
							{
								gapRect.Height = (int)(this.BodyHeight * ((float)gapRect.Height / (float)this.rect.Height));
							}
							if (base.FlippedY)
							{
								diffFromCenter = -diffFromCenter;
							}
						}
						if (Math.Abs(this.BodyRotation) > 0.01f)
						{
							Vector2 structureCenter = this.Position;
							Vector2 gapPos = structureCenter + new Vector2((float)Math.Cos((double)(this.IsHorizontal ? (-(double)this.BodyRotation) : (1.5707964f - this.BodyRotation))), (float)Math.Sin((double)(this.IsHorizontal ? (-(double)this.BodyRotation) : (1.5707964f - this.BodyRotation)))) * diffFromCenter + this.BodyOffset * this.scale;
							gapRect = new Rectangle((int)(gapPos.X - (float)(gapRect.Width / 2)), (int)(gapPos.Y + (float)(gapRect.Height / 2)), gapRect.Width, gapRect.Height);
						}
						gapRect.X -= 10;
						gapRect.Y += 10;
						gapRect.Width += 20;
						gapRect.Height += 20;
						bool rotatedEnoughToChangeOrientation = MathUtils.WrapAngleTwoPi(this.RotationRad - 0.7853982f) % 3.1415927f < 1.5707964f;
						if (rotatedEnoughToChangeOrientation)
						{
							Point center = gapRect.Location + gapRect.Size.FlipY() / new Point(2);
							Point topLeft = gapRect.Location;
							Point diff = topLeft - center;
							diff = diff.FlipY().YX().FlipY();
							Point newTopLeft = diff + center;
							gapRect = new Rectangle(newTopLeft, gapRect.Size.YX());
						}
						bool horizontalGap = rotatedEnoughToChangeOrientation ? this.IsHorizontal : (!this.IsHorizontal);
						bool diagonalGap = false;
						if (!MathUtils.NearlyEqual(this.BodyRotation, 0f, 0.0001f))
						{
							float sectorizedRotation = MathUtils.WrapAngleTwoPi(this.BodyRotation) % 1.5707964f;
							diagonalGap = (sectorizedRotation > 0.5235988f && sectorizedRotation < 1.0471976f);
							if (diagonalGap)
							{
								horizontalGap = ((float)(gapRect.Y - gapRect.Height / 2) < this.Position.Y);
								if (base.FlippedY)
								{
									horizontalGap = !horizontalGap;
								}
							}
						}
						this.Sections[sectionIndex].gap = new Gap(gapRect, horizontalGap, base.Submarine, diagonalGap, 0);
						this.Sections[sectionIndex].gap.FreeID();
						this.Sections[sectionIndex].gap.ShouldBeSaved = false;
						this.Sections[sectionIndex].gap.ConnectedWall = this;
						DebugConsole.Log(string.Concat(new string[]
						{
							"Created gap (ID ",
							this.Sections[sectionIndex].gap.ID.ToString(),
							", section: ",
							sectionIndex.ToString(),
							") on wall ",
							this.ID.ToString()
						}));
						if (noGaps && attacker != null)
						{
							GameServer.Log((this.Sections[sectionIndex].gap.IsRoomToRoom ? "Inner" : "Outer") + " wall breached by " + GameServer.CharacterLogName(attacker), ServerLog.MessageType.ItemInteraction);
						}
					}
					Gap gap = this.Sections[sectionIndex].gap;
					float damageRatio = (this.MaxHealth <= 0f) ? 0f : (damage / this.MaxHealth);
					float gapOpen = 0f;
					if (damageRatio > 0.7f)
					{
						gapOpen = MathHelper.Lerp(0.35f, 0.75f, MathUtils.InverseLerp(0.7f, 1f, damageRatio));
					}
					else if (damageRatio > 0.1f)
					{
						gapOpen = MathHelper.Lerp(0f, 0.35f, MathUtils.InverseLerp(0.1f, 0.7f, damageRatio));
					}
					gap.Open = gapOpen;
					if (gapOpen - prevGapOpenState > 0.25f && createExplosionEffect && !gap.IsRoomToRoom)
					{
						Structure.CreateWallDamageExplosion(gap, attacker, createWallDamageProjectiles);
					}
				}
			}
			float damageDiff = damage - this.Sections[sectionIndex].damage;
			bool hadHole = this.SectionBodyDisabled(sectionIndex);
			this.Sections[sectionIndex].damage = MathHelper.Clamp(damage, 0f, this.MaxHealth);
			this.HasDamage = this.Sections.Any((WallSection s) => s.damage > 0f);
			if (damageDiff != 0f)
			{
				Structure.OnHealthChangedHandler onHealthChanged = this.OnHealthChanged;
				if (onHealthChanged != null)
				{
					onHealthChanged(attacker, damageDiff);
				}
				if (attacker != null)
				{
					HumanAIController.StructureDamaged(this, damageDiff, attacker);
					this.OnHealthChangedProjSpecific(attacker, damageDiff);
					if ((GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient) && damageDiff < 0f)
					{
						CharacterInfo info = attacker.Info;
						if (info != null)
						{
							info.ApplySkillGain(Barotrauma.Tags.MechanicalSkill, -damageDiff * SkillSettings.Current.SkillIncreasePerRepairedStructureDamage, false, 2f, false);
						}
					}
				}
			}
			bool hasHole = this.SectionBodyDisabled(sectionIndex);
			if (hadHole == hasHole)
			{
				return;
			}
			this.UpdateSections();
		}

		// Token: 0x06000A22 RID: 2594 RVA: 0x000633E4 File Offset: 0x000615E4
		private static void CreateWallDamageExplosion(Gap gap, Character attacker, bool createProjectiles)
		{
			float explosionStrength = gap.Open;
			Hull linkedHull = gap.linkedTo.FirstOrDefault<MapEntity>() as Hull;
			if (linkedHull != null)
			{
				foreach (Gap otherGap in linkedHull.ConnectedGaps)
				{
					if (otherGap != gap && !otherGap.IsRoomToRoom && otherGap.Open >= 0.25f)
					{
						explosionStrength -= Math.Max(0f, 500f - Vector2.Distance(otherGap.WorldPosition, gap.WorldPosition)) / 500f;
						if (explosionStrength <= 0f)
						{
							return;
						}
					}
				}
			}
			if (Structure.explosionOnBroken == null)
			{
				Structure.explosionOnBroken = new Explosion(500f, 5f, 0f, 0f, 0f, 0f, 0f);
				AfflictionPrefab lacerations;
				if (AfflictionPrefab.Prefabs.TryGet("lacerations".ToIdentifier(), out lacerations))
				{
					Structure.explosionOnBroken.Attack.Afflictions.Add(lacerations.Instantiate(5f, null), null);
				}
				else
				{
					Structure.explosionOnBroken.Attack.Afflictions.Add(AfflictionPrefab.InternalDamage.Instantiate(5f, null), null);
				}
				Structure.explosionOnBroken.CameraShake = 5f;
				Structure.explosionOnBroken.IgnoreCover = false;
				Structure.explosionOnBroken.OnlyInside = true;
				Structure.explosionOnBroken.DistanceFalloff = false;
				Structure.explosionOnBroken.PlayDamageSounds = true;
				Structure.explosionOnBroken.DisableParticles();
			}
			Structure.explosionOnBroken.CameraShake = 25f;
			Explosion explosion = Structure.explosionOnBroken;
			Structure connectedWall = gap.ConnectedWall;
			explosion.IgnoredCover = ((connectedWall != null) ? connectedWall.ToEnumerable<Structure>() : null);
			Structure.explosionOnBroken.Attack.Range = (Structure.explosionOnBroken.CameraShakeRange = 500f * gap.Open);
			Structure.explosionOnBroken.Attack.DamageMultiplier = explosionStrength;
			Structure.explosionOnBroken.Attack.Stun = MathHelper.Clamp(explosionStrength, 0.5f, 1f);
			Structure.explosionOnBroken.IgnoredCharacters.Clear();
			if (((attacker != null) ? attacker.AIController : null) is EnemyAIController)
			{
				Structure.explosionOnBroken.IgnoredCharacters.Add(attacker);
			}
			Explosion explosion2 = Structure.explosionOnBroken;
			if (explosion2 != null)
			{
				explosion2.Explode(gap.WorldPosition, null, attacker);
			}
			ItemPrefab projectilePrefab;
			if (createProjectiles && ItemPrefab.Prefabs.TryGet("walldamageprojectile", out projectilePrefab) && linkedHull != null)
			{
				float angle = gap.IsHorizontal ? ((linkedHull.WorldPosition.X < gap.WorldPosition.X) ? 3.1415927f : 0f) : ((linkedHull.WorldPosition.Y < gap.WorldPosition.Y) ? -1.5707964f : 1.5707964f);
				Entity.Spawner.AddItemToSpawnQueue(projectilePrefab, gap.WorldPosition, null, null, delegate(Item item)
				{
					item.body.SetTransformIgnoreContacts(item.body.SimPosition, angle, true);
					Projectile projectile = item.GetComponent<Projectile>();
					if (projectile != null)
					{
						projectile.Use(null, 0f);
					}
				});
			}
		}

		// Token: 0x06000A23 RID: 2595 RVA: 0x000636FC File Offset: 0x000618FC
		private void OnHealthChangedProjSpecific(Character attacker, float damageAmount)
		{
			GameMain.Server.KarmaManager.OnStructureHealthChanged(this, attacker, damageAmount);
		}

		// Token: 0x06000A24 RID: 2596 RVA: 0x00063710 File Offset: 0x00061910
		public void SetCollisionCategory(Category collisionCategory)
		{
			if (this.Bodies == null)
			{
				return;
			}
			foreach (Body body in this.Bodies)
			{
				body.CollisionCategories = collisionCategory;
			}
		}

		// Token: 0x06000A25 RID: 2597 RVA: 0x0006376C File Offset: 0x0006196C
		private void UpdateSections()
		{
			if (this.Bodies == null)
			{
				return;
			}
			foreach (Body b in this.Bodies)
			{
				GameMain.World.Remove(b);
			}
			this.Bodies.Clear();
			this.bodyDimensions.Clear();
			bool hasHoles = false;
			List<WallSection> mergedSections = new List<WallSection>();
			for (int i = 0; i < this.Sections.Length; i++)
			{
				if (this.SectionBodyDisabled(i))
				{
					hasHoles = true;
					if (mergedSections.Any<WallSection>())
					{
						Rectangle mergedRect = this.GenerateMergedRect(mergedSections);
						mergedSections.Clear();
						this.CreateRectBody(mergedRect, true);
					}
				}
				else
				{
					mergedSections.Add(this.Sections[i]);
				}
			}
			if (mergedSections.Count > 0)
			{
				Rectangle mergedRect2 = this.GenerateMergedRect(mergedSections);
				this.CreateRectBody(mergedRect2, true);
			}
			if (hasHoles || !this.Bodies.Any<Body>())
			{
				Body sensorBody = this.CreateRectBody(this.rect, false);
				sensorBody.CollisionCategories = Category.Cat9;
			}
			foreach (WallSection section in this.Sections)
			{
				bool intersectsWithBody = false;
				foreach (Body body in this.Bodies)
				{
					Rectangle bodyRect = new Rectangle(ConvertUnits.ToDisplayUnits(body.Position - this.bodyDimensions[body] / 2f).ToPoint(), ConvertUnits.ToDisplayUnits(this.bodyDimensions[body]).ToPoint());
					Rectangle sectionRect = section.rect;
					sectionRect.Y -= section.rect.Height;
					if (bodyRect.Intersects(sectionRect))
					{
						intersectsWithBody = true;
						break;
					}
				}
				section.NoPhysicsBody = !intersectsWithBody;
			}
		}

		// Token: 0x06000A26 RID: 2598 RVA: 0x00063984 File Offset: 0x00061B84
		private Body CreateRectBody(Rectangle rect, bool createConvexHull)
		{
			float diffFromCenter;
			if (this.IsHorizontal)
			{
				diffFromCenter = (float)(rect.Center.X - this.rect.Center.X) / (float)this.rect.Width * this.BodyWidth;
				if (this.BodyWidth > 0f)
				{
					rect.Width = Math.Max((int)Math.Round((double)(this.BodyWidth * ((float)rect.Width / (float)this.rect.Width))), 1);
				}
				if (this.BodyHeight > 0f)
				{
					rect.Height = (int)this.BodyHeight;
				}
				if (base.FlippedX)
				{
					diffFromCenter = -diffFromCenter;
				}
			}
			else
			{
				diffFromCenter = (float)(rect.Y - rect.Height / 2 - (this.rect.Y - this.rect.Height / 2)) / (float)this.rect.Height * this.BodyHeight;
				if (this.BodyWidth > 0f)
				{
					rect.Width = (int)this.BodyWidth;
				}
				if (this.BodyHeight > 0f)
				{
					rect.Height = Math.Max((int)Math.Round((double)(this.BodyHeight * ((float)rect.Height / (float)this.rect.Height))), 1);
				}
				if (base.FlippedY)
				{
					diffFromCenter = -diffFromCenter;
				}
			}
			Vector2 bodyOffset = ConvertUnits.ToSimUnits(this.BodyOffset) * this.scale;
			Body newBody = GameMain.World.CreateRectangle(ConvertUnits.ToSimUnits(rect.Width), ConvertUnits.ToSimUnits(rect.Height), 1.5f, default(Vector2), 0f, BodyType.Static, Category.Cat1, Category.All, false);
			newBody.Friction = 0.5f;
			newBody.OnCollision += this.OnWallCollision;
			newBody.CollisionCategories = (this.Prefab.Platform ? Category.Cat3 : Category.Cat1);
			newBody.UserData = this;
			Vector2 structureCenter = ConvertUnits.ToSimUnits(this.Position);
			if (!MathUtils.NearlyEqual(this.BodyRotation, 0f, 0.0001f))
			{
				Vector2 pos = structureCenter + bodyOffset + new Vector2((float)Math.Cos((double)(this.IsHorizontal ? (-(double)this.BodyRotation) : (1.5707964f - this.BodyRotation))), (float)Math.Sin((double)(this.IsHorizontal ? (-(double)this.BodyRotation) : (1.5707964f - this.BodyRotation)))) * ConvertUnits.ToSimUnits(diffFromCenter);
				newBody.SetTransformIgnoreContacts(ref pos, -this.BodyRotation);
			}
			else
			{
				Vector2 pos2 = structureCenter + (this.IsHorizontal ? Vector2.UnitX : Vector2.UnitY) * ConvertUnits.ToSimUnits(diffFromCenter) + bodyOffset;
				newBody.SetTransformIgnoreContacts(ref pos2, newBody.Rotation);
			}
			this.Bodies.Add(newBody);
			this.bodyDimensions.Add(newBody, new Vector2(ConvertUnits.ToSimUnits(rect.Width), ConvertUnits.ToSimUnits(rect.Height)));
			return newBody;
		}

		// Token: 0x06000A27 RID: 2599 RVA: 0x00063C74 File Offset: 0x00061E74
		public override void FlipX(bool relativeToSub, bool force = false)
		{
			base.FlipX(relativeToSub, false);
			if (this.StairDirection != Direction.None)
			{
				this.StairDirection = ((this.StairDirection == Direction.Left) ? Direction.Right : Direction.Left);
				this.Bodies.ForEach(delegate(Body b)
				{
					GameMain.World.Remove(b);
				});
				this.Bodies.Clear();
				this.bodyDimensions.Clear();
				this.CreateStairBodies();
			}
			if (this.Prefab.Body)
			{
				this.CreateSections();
				this.UpdateSections();
			}
		}

		// Token: 0x06000A28 RID: 2600 RVA: 0x00063D04 File Offset: 0x00061F04
		public override void FlipY(bool relativeToSub, bool force = false)
		{
			base.FlipY(relativeToSub, false);
			if (this.StairDirection != Direction.None)
			{
				this.StairDirection = ((this.StairDirection == Direction.Left) ? Direction.Right : Direction.Left);
				this.Bodies.ForEach(delegate(Body b)
				{
					GameMain.World.Remove(b);
				});
				this.Bodies.Clear();
				this.bodyDimensions.Clear();
				this.CreateStairBodies();
			}
			if (this.Prefab.Body)
			{
				this.CreateSections();
				this.UpdateSections();
			}
		}

		// Token: 0x06000A29 RID: 2601 RVA: 0x00063D94 File Offset: 0x00061F94
		public static Structure Load(ContentXElement element, Submarine submarine, IdRemap idRemap)
		{
			string name = element.GetAttribute("name").Value;
			Identifier identifier = element.GetAttributeIdentifier("identifier", "");
			StructurePrefab prefab = Structure.FindPrefab(name, identifier);
			if (prefab == null)
			{
				DebugConsole.ThrowError(string.Concat(new string[]
				{
					"Error loading structure - structure prefab \"",
					name,
					"\" (identifier \"",
					identifier.ToString(),
					"\") not found."
				}), null, null, false, false);
				return null;
			}
			string key = "rect";
			Rectangle empty = Rectangle.Empty;
			Rectangle rect = element.GetAttributeRect(key, empty);
			Structure s = new Structure(rect, prefab, submarine, idRemap.GetOffsetId(element), element)
			{
				Submarine = submarine
			};
			bool flippedX = element.GetAttributeBool("FlippedX", false);
			bool flippedY = element.GetAttributeBool("FlippedY", false);
			if (((submarine != null) ? submarine.Info.GameVersion : null) != null)
			{
				SerializableProperty.UpgradeGameVersion(s, s.Prefab.ConfigElement, submarine.Info.GameVersion);
				if (submarine.Info.GameVersion < new Version(0, 19, 10))
				{
					GameSession gameSession = GameMain.GameSession;
					if (((gameSession != null) ? gameSession.LevelData : null) != null)
					{
						s.CrushDepth = Math.Max(s.CrushDepth, (float)GameMain.GameSession.LevelData.InitialDepth * Physics.DisplayToRealWorldRatio + 500f);
					}
				}
			}
			bool hasDamage = false;
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "section"))
				{
					if (a == "upgrade")
					{
						Identifier upgradeIdentifier = subElement.GetAttributeIdentifier("identifier", Identifier.Empty);
						UpgradePrefab upgradePrefab = UpgradePrefab.Find(upgradeIdentifier);
						int level = subElement.GetAttributeInt("level", 1);
						if (upgradePrefab != null)
						{
							s.AddUpgrade(new Upgrade(s, upgradePrefab, level, subElement), false);
						}
						else
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(49, 2);
							defaultInterpolatedStringHandler.AppendLiteral("An upgrade with identifier \"");
							defaultInterpolatedStringHandler.AppendFormatted<Identifier>(upgradeIdentifier);
							defaultInterpolatedStringHandler.AppendLiteral("\" on ");
							defaultInterpolatedStringHandler.AppendFormatted(s.Name);
							defaultInterpolatedStringHandler.AppendLiteral(" was not found. ");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear() + "It's effect will not be applied and won't be saved after the round ends.", null, null, false, false);
						}
					}
				}
				else
				{
					int index = subElement.GetAttributeInt("i", -1);
					if (index != -1)
					{
						if (index < 0 || index >= s.SectionCount)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(95, 3);
							defaultInterpolatedStringHandler2.AppendLiteral("Error while loading structure \"");
							defaultInterpolatedStringHandler2.AppendFormatted(s.Name);
							defaultInterpolatedStringHandler2.AppendLiteral("\". Section damage index out of bounds. Index: ");
							defaultInterpolatedStringHandler2.AppendFormatted<int>(index);
							defaultInterpolatedStringHandler2.AppendLiteral(", section count: ");
							defaultInterpolatedStringHandler2.AppendFormatted<int>(s.SectionCount);
							defaultInterpolatedStringHandler2.AppendLiteral(".");
							string errorMsg = defaultInterpolatedStringHandler2.ToStringAndClear();
							DebugConsole.ThrowError(errorMsg, null, null, false, false);
							GameAnalyticsManager.AddErrorEventOnce("Structure.Load:SectionIndexOutOfBounds", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
						}
						else
						{
							float damage = subElement.GetAttributeFloat("damage", 0f);
							s.Sections[index].damage = damage;
							hasDamage |= (damage > 0f);
						}
					}
				}
			}
			if (flippedX)
			{
				s.FlipX(false, false);
			}
			if (flippedY)
			{
				s.FlipY(false, false);
			}
			if (element.GetAttribute("UseDropShadow") == null)
			{
				s.UseDropShadow = s.HasBody;
			}
			if (element.GetAttribute("NoAITarget") == null)
			{
				s.NoAITarget = prefab.NoAITarget;
			}
			if (hasDamage)
			{
				s.UpdateSections();
			}
			return s;
		}

		// Token: 0x06000A2A RID: 2602 RVA: 0x00064174 File Offset: 0x00062374
		public static StructurePrefab FindPrefab(string name, Identifier identifier)
		{
			StructurePrefab prefab = null;
			StructurePrefab structurePrefab;
			if (identifier.IsEmpty)
			{
				prefab = (MapEntityPrefab.Find(name, "", true) as StructurePrefab);
				if (prefab == null)
				{
					prefab = (MapEntityPrefab.Find(name, null, true) as StructurePrefab);
				}
				if (prefab == null)
				{
					prefab = (MapEntityPrefab.Find(null, name, true) as StructurePrefab);
				}
			}
			else if (StructurePrefab.Prefabs.TryGet(identifier, out structurePrefab))
			{
				prefab = structurePrefab;
			}
			return prefab;
		}

		// Token: 0x06000A2B RID: 2603 RVA: 0x000641D4 File Offset: 0x000623D4
		public override XElement Save(XElement parentElement)
		{
			XElement element = new XElement("Structure");
			int width = base.ResizeHorizontal ? this.rect.Width : this.defaultRect.Width;
			int height = base.ResizeVertical ? this.rect.Height : this.defaultRect.Height;
			element.Add(new object[]
			{
				new XAttribute("name", this.Prefab.Name),
				new XAttribute("identifier", this.Prefab.Identifier),
				new XAttribute("ID", this.ID),
				new XAttribute("rect", string.Concat(new string[]
				{
					((int)((float)this.rect.X - base.Submarine.HiddenSubPosition.X)).ToString(),
					",",
					((int)((float)this.rect.Y - base.Submarine.HiddenSubPosition.Y)).ToString(),
					",",
					width.ToString(),
					",",
					height.ToString()
				}))
			});
			if (base.FlippedX)
			{
				element.Add(new XAttribute("flippedx", true));
			}
			if (base.FlippedY)
			{
				element.Add(new XAttribute("flippedy", true));
			}
			for (int i = 0; i < this.Sections.Length; i++)
			{
				if (this.Sections[i].damage != 0f)
				{
					XElement sectionElement = new XElement("section", new object[]
					{
						new XAttribute("i", i),
						new XAttribute("damage", this.Sections[i].damage)
					});
					element.Add(sectionElement);
				}
			}
			SerializableProperty.SerializeProperties(this, element, false, false);
			if (this.CastShadow == this.Prefab.CastShadow)
			{
				XAttribute attribute = element.GetAttribute("CastShadow", StringComparison.OrdinalIgnoreCase);
				if (attribute != null)
				{
					attribute.Remove();
				}
			}
			foreach (Upgrade upgrade in this.Upgrades)
			{
				upgrade.Save(element);
			}
			parentElement.Add(element);
			return element;
		}

		// Token: 0x06000A2C RID: 2604 RVA: 0x0006448C File Offset: 0x0006268C
		public override void OnMapLoaded()
		{
			for (int i = 0; i < this.Sections.Length; i++)
			{
				this.SetDamage(i, this.Sections[i].damage, null, false, true, false, false);
			}
		}

		// Token: 0x06000A2D RID: 2605 RVA: 0x000644C8 File Offset: 0x000626C8
		public virtual void Reset()
		{
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, this.Prefab.ConfigElement);
			this.MaxHealth = this.Prefab.Health;
			this.Sprite.ReloadXML();
			base.SpriteDepth = this.Sprite.Depth;
			this.NoAITarget = this.Prefab.NoAITarget;
		}

		// Token: 0x06000A2E RID: 2606 RVA: 0x00064530 File Offset: 0x00062730
		public override void Update(float deltaTime, Camera cam)
		{
			if (this.aiTarget != null)
			{
				this.aiTarget.SightRange = ((base.Submarine == null) ? this.aiTarget.MinSightRange : MathHelper.Lerp(this.aiTarget.MinSightRange, this.aiTarget.MaxSightRange, base.Submarine.Velocity.Length() / 10f));
			}
		}

		// Token: 0x0400046B RID: 1131
		public const int WallSectionSize = 96;

		// Token: 0x0400046C RID: 1132
		public static List<Structure> WallList = new List<Structure>();

		// Token: 0x0400046D RID: 1133
		private const float LeakThreshold = 0.1f;

		// Token: 0x0400046E RID: 1134
		private const float BigGapThreshold = 0.7f;

		// Token: 0x0400046F RID: 1135
		public const float SmallGapOpenness = 0.35f;

		// Token: 0x04000470 RID: 1136
		public const float LargeGapOpenness = 0.75f;

		// Token: 0x04000471 RID: 1137
		private readonly Dictionary<Body, Vector2> bodyDimensions = new Dictionary<Body, Vector2>();

		// Token: 0x04000472 RID: 1138
		private static Explosion explosionOnBroken;

		// Token: 0x04000473 RID: 1139
		public Structure.OnHealthChangedHandler OnHealthChanged;

		// Token: 0x0400047B RID: 1147
		private float? maxHealth;

		// Token: 0x0400047C RID: 1148
		private float crushDepth;

		// Token: 0x0400047F RID: 1151
		protected Color spriteColor;

		// Token: 0x04000482 RID: 1154
		private float scale = 1f;

		// Token: 0x04000483 RID: 1155
		protected Vector2 textureScale = Vector2.One;

		// Token: 0x04000485 RID: 1157
		protected Vector2 textureOffset = Vector2.Zero;

		// Token: 0x04000486 RID: 1158
		private Rectangle defaultRect;

		// Token: 0x02000722 RID: 1826
		// (Invoke) Token: 0x060050DC RID: 20700
		public delegate void OnHealthChangedHandler(Character attacker, float damage);
	}
}
