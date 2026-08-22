using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Lights;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005A7 RID: 1447
	internal class Door : Pickable, IDrawableComponent, IServerSerializable, INetSerializable
	{
		// Token: 0x170015C8 RID: 5576
		// (get) Token: 0x06005793 RID: 22419 RVA: 0x002D5029 File Offset: 0x002D3229
		// (set) Token: 0x06005794 RID: 22420 RVA: 0x002D5031 File Offset: 0x002D3231
		[Serialize("1,1", IsPropertySaveable.No, "The scale of the shadow-casting area of the door (relative to the actual size of the door).", "", false)]
		public Vector2 ShadowScale { get; set; }

		// Token: 0x170015C9 RID: 5577
		// (get) Token: 0x06005795 RID: 22421 RVA: 0x002D503A File Offset: 0x002D323A
		public Vector2 DrawSize
		{
			get
			{
				return Vector2.Zero;
			}
		}

		// Token: 0x06005796 RID: 22422 RVA: 0x002D5044 File Offset: 0x002D3244
		private Vector2[] GetConvexHullCorners(Rectangle rect)
		{
			Point shadowSize = rect.Size.Multiply(this.ShadowScale);
			Vector2 center = new Vector2((float)rect.Center.X, (float)(rect.Y - rect.Height / 2));
			Vector2[] corners = new Vector2[]
			{
				center + new Vector2((float)(-(float)shadowSize.X), (float)(-(float)shadowSize.Y)) / 2f,
				center + new Vector2((float)(-(float)shadowSize.X), (float)shadowSize.Y) / 2f,
				center + new Vector2((float)shadowSize.X, (float)shadowSize.Y) / 2f,
				center + new Vector2((float)shadowSize.X, (float)(-(float)shadowSize.Y)) / 2f
			};
			if (this.IsHorizontal)
			{
				if (this.item.FlippedX)
				{
					Vector2 itemCenter = new Vector2((float)this.item.Rect.Center.X, (float)(this.item.Rect.Y - this.item.Rect.Height / 2));
					for (int i = 0; i < corners.Length; i++)
					{
						corners[i].X = itemCenter.X * 2f - corners[i].X;
					}
					Array.Reverse<Vector2>(corners);
				}
			}
			else if (this.item.FlippedY)
			{
				Vector2 itemCenter2 = new Vector2((float)this.item.Rect.Center.X, (float)(this.item.Rect.Y - this.item.Rect.Height / 2));
				for (int j = 0; j < corners.Length; j++)
				{
					corners[j].Y = itemCenter2.Y * 2f - corners[j].Y;
				}
				Array.Reverse<Vector2>(corners);
			}
			return corners;
		}

		// Token: 0x06005797 RID: 22423 RVA: 0x002D5278 File Offset: 0x002D3478
		private void UpdateConvexHulls()
		{
			if (this.item.Removed)
			{
				return;
			}
			if (this.doorSprite == null)
			{
				return;
			}
			this.doorRect = new Rectangle(this.item.Rect.Center.X - (int)(this.doorSprite.size.X / 2f * this.item.Scale), this.item.Rect.Y - this.item.Rect.Height / 2 + (int)(this.doorSprite.size.Y / 2f * this.item.Scale), (int)(this.doorSprite.size.X * this.item.Scale), (int)(this.doorSprite.size.Y * this.item.Scale));
			Rectangle rect = this.doorRect;
			if (this.IsConvexHullHorizontal)
			{
				rect.Width = (int)((float)rect.Width * (1f - this.openState));
			}
			else
			{
				rect.Height = (int)((float)rect.Height * (1f - this.openState));
			}
			if (this.Window.Height > 0 && this.Window.Width > 0)
			{
				if (this.IsConvexHullHorizontal)
				{
					rect.Width = (int)((float)this.Window.X * this.item.Scale);
					rect.X -= (int)((float)this.doorRect.Width * this.openState);
					rect.Width = Math.Max(rect.Width - (this.doorRect.X - rect.X), 0);
					rect.X = Math.Max(this.doorRect.X, rect.X);
					if (this.convexHull2 != null)
					{
						Rectangle rect2 = this.doorRect;
						rect2.X += (int)((float)this.Window.Right * this.item.Scale);
						rect2.X -= (int)((float)this.doorRect.Width * this.openState);
						rect2.X = Math.Max(this.doorRect.X, rect2.X);
						rect2.Width = this.doorRect.Right - (int)((float)this.doorRect.Width * this.openState) - rect2.X;
						if (rect2.Width == 0)
						{
							this.convexHull2.Enabled = false;
						}
						else
						{
							this.convexHull2.Enabled = true;
							this.SetVertices(this.convexHull2, rect2);
						}
					}
				}
				else
				{
					rect.Height = -(int)((float)this.Window.Y * this.item.Scale);
					rect.Y += (int)((float)this.doorRect.Height * this.openState);
					rect.Height = Math.Max(rect.Height - (rect.Y - this.doorRect.Y), 0);
					rect.Y = Math.Min(this.doorRect.Y, rect.Y);
					if (this.convexHull2 != null)
					{
						Rectangle rect3 = this.doorRect;
						rect3.Y += (int)((float)this.Window.Y * this.item.Scale - (float)this.Window.Height * this.item.Scale);
						rect3.Y += (int)((float)this.doorRect.Height * this.openState);
						rect3.Y = Math.Min(this.doorRect.Y, rect3.Y);
						rect3.Height = rect3.Y - (this.doorRect.Y - (int)((float)this.doorRect.Height * (1f - this.openState)));
						if (rect3.Height == 0)
						{
							this.convexHull2.Enabled = false;
						}
						else
						{
							this.convexHull2.Enabled = true;
							this.SetVertices(this.convexHull2, rect3);
						}
					}
				}
			}
			if (this.convexHull == null)
			{
				return;
			}
			if (rect.Height == 0 || rect.Width == 0)
			{
				this.convexHull.Enabled = false;
				return;
			}
			this.convexHull.Enabled = true;
			this.SetVertices(this.convexHull, rect);
		}

		// Token: 0x06005798 RID: 22424 RVA: 0x002D56E8 File Offset: 0x002D38E8
		private void SetVertices(ConvexHull convexHull, Rectangle rect)
		{
			Vector2[] verts = this.GetConvexHullCorners(rect);
			Vector2 center = (verts[0] + verts[2]) / 2f;
			Vector2[] points = verts;
			Vector2[] losPoints;
			if (!this.IsConvexHullHorizontal)
			{
				Vector2[] array = new Vector2[2];
				array[0] = new Vector2(center.X, verts[0].Y);
				losPoints = array;
				array[1] = new Vector2(center.X, verts[2].Y);
			}
			else
			{
				Vector2[] array2 = new Vector2[2];
				array2[0] = new Vector2(verts[0].X, center.Y);
				losPoints = array2;
				array2[1] = new Vector2(verts[2].X, center.Y);
			}
			convexHull.SetVertices(points, losPoints, true, null);
			convexHull.MaxMergeLosVerticesDist = new float?(35f);
		}

		// Token: 0x06005799 RID: 22425 RVA: 0x002D57CC File Offset: 0x002D39CC
		public void Draw(SpriteBatch spriteBatch, bool editing, float itemDepth = -1f, Color? overrideColor = null)
		{
			Color color = overrideColor ?? this.item.GetSpriteColor(null, true);
			if (this.brokenSprite == null)
			{
				color = color.Multiply(this.item.Condition / this.item.MaxCondition, false);
				color.A = byte.MaxValue;
			}
			if (this.stuck > 0f && this.weldedSprite != null)
			{
				Vector2 weldSpritePos = new Vector2((float)this.item.Rect.Center.X, (float)this.item.Rect.Y - (float)this.item.Rect.Height / 2f) + this.shakePos;
				if (this.item.Submarine != null)
				{
					weldSpritePos += this.item.Submarine.DrawPosition;
				}
				weldSpritePos.Y = -weldSpritePos.Y;
				this.weldedSprite.Draw(spriteBatch, weldSpritePos, overrideColor ?? (this.item.SpriteColor * (this.stuck / 100f)), 0f, this.item.Scale, SpriteEffects.None, null);
			}
			if (this.openState >= 1f)
			{
				return;
			}
			Vector2 pos;
			if (this.IsHorizontal)
			{
				pos = new Vector2((float)this.item.Rect.X, (float)(this.item.Rect.Y - this.item.Rect.Height / 2));
				if (this.item.FlippedX)
				{
					pos.X += (float)((int)(this.doorSprite.size.X * this.item.Scale * this.openState));
				}
			}
			else
			{
				pos = new Vector2((float)this.item.Rect.Center.X, (float)this.item.Rect.Y);
				if (this.item.FlippedY)
				{
					pos.Y -= (float)((int)(this.doorSprite.size.Y * this.item.Scale * this.openState));
				}
			}
			pos += this.shakePos;
			if (this.item.Submarine != null)
			{
				pos += this.item.Submarine.DrawPosition;
			}
			pos.Y = -pos.Y;
			if (this.brokenSprite == null || !this.IsBroken)
			{
				Sprite sprite = this.doorSprite;
				if (((sprite != null) ? sprite.Texture : null) != null)
				{
					spriteBatch.Draw(this.doorSprite.Texture, pos, new Rectangle?(Door.<Draw>g__getSourceRect|15_1(this.doorSprite, this.openState, this.IsHorizontal)), color, 0f, this.doorSprite.Origin, this.item.Scale, this.item.SpriteEffects, this.doorSprite.Depth);
				}
			}
			float num;
			if (!this.item.Repairables.Any<Repairable>())
			{
				num = this.item.MaxCondition;
			}
			else
			{
				num = this.item.Repairables.Min((Repairable r) => r.RepairThreshold) / 100f * this.item.MaxCondition;
			}
			float maxCondition = num;
			float healthRatio = this.item.Health / maxCondition;
			Sprite sprite2 = this.brokenSprite;
			if (((sprite2 != null) ? sprite2.Texture : null) != null && healthRatio < 1f)
			{
				Vector2 scale = this.scaleBrokenSprite ? new Vector2(1f - healthRatio) : Vector2.One;
				if (this.IsHorizontal)
				{
					scale.X = 1f;
				}
				else
				{
					scale.Y = 1f;
				}
				float alpha = this.fadeBrokenSprite ? (1f - healthRatio) : 1f;
				spriteBatch.Draw(this.brokenSprite.Texture, pos, new Rectangle?(Door.<Draw>g__getSourceRect|15_1(this.brokenSprite, this.openState, this.IsHorizontal)), color * alpha, 0f, this.brokenSprite.Origin, scale * this.item.Scale, this.item.SpriteEffects, this.brokenSprite.Depth);
			}
		}

		// Token: 0x0600579A RID: 22426 RVA: 0x002D5C58 File Offset: 0x002D3E58
		public override void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			base.ClientEventRead(msg, sendingTime);
			bool open = msg.ReadBoolean();
			bool broken = msg.ReadBoolean();
			bool forcedOpen = msg.ReadBoolean();
			bool isStuck = msg.ReadBoolean();
			bool isJammed = msg.ReadBoolean();
			this.SetState(open, true, false, forcedOpen);
			this.stuck = msg.ReadRangedSingle(0f, 100f, 8);
			ushort lastUserID = msg.ReadUInt16();
			Character user = (lastUserID == 0) ? null : (Entity.FindEntityByID(lastUserID) as Character);
			if (user != this.lastUser)
			{
				this.lastUser = user;
				this.toggleCooldownTimer = this.ToggleCoolDown;
			}
			this.isStuck = isStuck;
			this.isJammed = isJammed;
			if (isStuck)
			{
				this.OpenState = 0f;
			}
			this.IsBroken = broken;
			this.PredictedState = null;
		}

		// Token: 0x170015CA RID: 5578
		// (get) Token: 0x0600579B RID: 22427 RVA: 0x002D5D21 File Offset: 0x002D3F21
		public static IReadOnlyCollection<Door> DoorList
		{
			get
			{
				return Door.doorList;
			}
		}

		// Token: 0x170015CB RID: 5579
		// (get) Token: 0x0600579C RID: 22428 RVA: 0x002D5D28 File Offset: 0x002D3F28
		// (set) Token: 0x0600579D RID: 22429 RVA: 0x002D5D30 File Offset: 0x002D3F30
		public bool IsJammed
		{
			get
			{
				return this.isJammed;
			}
			set
			{
				if (this.isJammed == value)
				{
					return;
				}
				this.isJammed = value;
			}
		}

		// Token: 0x170015CC RID: 5580
		// (get) Token: 0x0600579E RID: 22430 RVA: 0x002D5D43 File Offset: 0x002D3F43
		// (set) Token: 0x0600579F RID: 22431 RVA: 0x002D5D4B File Offset: 0x002D3F4B
		[Serialize(false, IsPropertySaveable.Yes, "", "", true)]
		public bool IsStuck
		{
			get
			{
				return this.isStuck;
			}
			private set
			{
				if (this.isStuck == value)
				{
					return;
				}
				this.isStuck = value;
			}
		}

		// Token: 0x170015CD RID: 5581
		// (get) Token: 0x060057A0 RID: 22432 RVA: 0x002D5D5E File Offset: 0x002D3F5E
		// (set) Token: 0x060057A1 RID: 22433 RVA: 0x002D5D66 File Offset: 0x002D3F66
		public bool IgnoreSignals { get; private set; }

		// Token: 0x170015CE RID: 5582
		// (get) Token: 0x060057A2 RID: 22434 RVA: 0x002D5D6F File Offset: 0x002D3F6F
		public bool CanBeTraversed
		{
			get
			{
				return !this.Impassable && (this.IsBroken || this.IsOpen);
			}
		}

		// Token: 0x170015CF RID: 5583
		// (get) Token: 0x060057A3 RID: 22435 RVA: 0x002D5D8B File Offset: 0x002D3F8B
		// (set) Token: 0x060057A4 RID: 22436 RVA: 0x002D5D93 File Offset: 0x002D3F93
		public bool IsBroken
		{
			get
			{
				return this.isBroken;
			}
			set
			{
				if (this.isBroken == value)
				{
					return;
				}
				this.isBroken = value;
				if (this.isBroken)
				{
					this.DisableBody();
					return;
				}
				this.EnableBody();
			}
		}

		// Token: 0x170015D0 RID: 5584
		// (get) Token: 0x060057A5 RID: 22437 RVA: 0x002D5DBB File Offset: 0x002D3FBB
		// (set) Token: 0x060057A6 RID: 22438 RVA: 0x002D5DC3 File Offset: 0x002D3FC3
		public PhysicsBody Body { get; private set; }

		// Token: 0x170015D1 RID: 5585
		// (get) Token: 0x060057A7 RID: 22439 RVA: 0x002D5DCC File Offset: 0x002D3FCC
		private float RepairThreshold
		{
			get
			{
				if (this.item.GetComponent<Repairable>() != null)
				{
					return this.item.MaxCondition;
				}
				return 0f;
			}
		}

		// Token: 0x170015D2 RID: 5586
		// (get) Token: 0x060057A8 RID: 22440 RVA: 0x002D5DEC File Offset: 0x002D3FEC
		// (set) Token: 0x060057A9 RID: 22441 RVA: 0x002D5DF4 File Offset: 0x002D3FF4
		[Serialize(0f, IsPropertySaveable.Yes, "How badly stuck the door is (in percentages). If the percentage reaches 100, the door needs to be cut open to make it usable again.", "", false)]
		public float Stuck
		{
			get
			{
				return this.stuck;
			}
			set
			{
				if (this.isOpen || this.isBroken || !this.CanBeWelded)
				{
					return;
				}
				this.stuck = MathHelper.Clamp(value, 0f, 100f);
				if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
				{
					return;
				}
				if (this.stuck <= 0f)
				{
					this.IsStuck = false;
				}
				if (this.stuck >= 99f)
				{
					this.IsStuck = true;
				}
			}
		}

		// Token: 0x170015D3 RID: 5587
		// (get) Token: 0x060057AA RID: 22442 RVA: 0x002D5E6C File Offset: 0x002D406C
		// (set) Token: 0x060057AB RID: 22443 RVA: 0x002D5E74 File Offset: 0x002D4074
		[Serialize(3f, IsPropertySaveable.Yes, "How quickly the door opens.", "", false)]
		[Editable]
		public float OpeningSpeed { get; private set; }

		// Token: 0x170015D4 RID: 5588
		// (get) Token: 0x060057AC RID: 22444 RVA: 0x002D5E7D File Offset: 0x002D407D
		// (set) Token: 0x060057AD RID: 22445 RVA: 0x002D5E85 File Offset: 0x002D4085
		[Serialize(3f, IsPropertySaveable.Yes, "How quickly the door closes.", "", false)]
		[Editable]
		public float ClosingSpeed { get; private set; }

		// Token: 0x170015D5 RID: 5589
		// (get) Token: 0x060057AE RID: 22446 RVA: 0x002D5E8E File Offset: 0x002D408E
		// (set) Token: 0x060057AF RID: 22447 RVA: 0x002D5E96 File Offset: 0x002D4096
		[Serialize(1f, IsPropertySaveable.Yes, "The door cannot be opened/closed during this time after it has been opened/closed by another character.", "", false)]
		[Editable]
		public float ToggleCoolDown { get; private set; }

		// Token: 0x170015D6 RID: 5590
		// (get) Token: 0x060057B0 RID: 22448 RVA: 0x002D5E9F File Offset: 0x002D409F
		// (set) Token: 0x060057B1 RID: 22449 RVA: 0x002D5EA7 File Offset: 0x002D40A7
		public bool? PredictedState { get; private set; }

		// Token: 0x170015D7 RID: 5591
		// (get) Token: 0x060057B2 RID: 22450 RVA: 0x002D5EB0 File Offset: 0x002D40B0
		public Gap LinkedGap
		{
			get
			{
				if (this.linkedGap == null)
				{
					this.GetLinkedGap();
				}
				return this.linkedGap;
			}
		}

		// Token: 0x060057B3 RID: 22451 RVA: 0x002D5EC8 File Offset: 0x002D40C8
		private void GetLinkedGap()
		{
			this.linkedGap = (this.item.linkedTo.FirstOrDefault((MapEntity e) => e is Gap) as Gap);
			if (this.linkedGap == null)
			{
				Rectangle rect = this.item.Rect;
				this.linkedGap = new Gap(rect, !this.IsHorizontal, base.Item.Submarine, false, 0)
				{
					Submarine = this.item.Submarine
				};
				this.item.linkedTo.Add(this.linkedGap);
			}
			this.RefreshLinkedGap();
		}

		// Token: 0x170015D8 RID: 5592
		// (get) Token: 0x060057B4 RID: 22452 RVA: 0x002D5F72 File Offset: 0x002D4172
		// (set) Token: 0x060057B5 RID: 22453 RVA: 0x002D5F7A File Offset: 0x002D417A
		public bool IsHorizontal { get; private set; }

		// Token: 0x170015D9 RID: 5593
		// (get) Token: 0x060057B6 RID: 22454 RVA: 0x002D5F83 File Offset: 0x002D4183
		public bool IsConvexHullHorizontal
		{
			get
			{
				if (!this.autoOrientGap || this.linkedGap == null)
				{
					return this.IsHorizontal;
				}
				return !this.linkedGap.IsHorizontal;
			}
		}

		// Token: 0x170015DA RID: 5594
		// (get) Token: 0x060057B7 RID: 22455 RVA: 0x002D5FAA File Offset: 0x002D41AA
		// (set) Token: 0x060057B8 RID: 22456 RVA: 0x002D5FB2 File Offset: 0x002D41B2
		[Serialize("0.0,0.0,0.0,0.0", IsPropertySaveable.No, "Position and size of the window on the door. The upper left corner is 0,0. Set the width and height to 0 if you don't want the door to have a window.", "", false)]
		public Rectangle Window { get; set; }

		// Token: 0x170015DB RID: 5595
		// (get) Token: 0x060057B9 RID: 22457 RVA: 0x002D5FBB File Offset: 0x002D41BB
		// (set) Token: 0x060057BA RID: 22458 RVA: 0x002D5FC3 File Offset: 0x002D41C3
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "Is the door currently open.", "", false)]
		public bool IsOpen
		{
			get
			{
				return this.isOpen;
			}
			set
			{
				this.isOpen = value;
				this.OpenState = (this.isOpen ? 1f : 0f);
			}
		}

		// Token: 0x170015DC RID: 5596
		// (get) Token: 0x060057BB RID: 22459 RVA: 0x002D5FE6 File Offset: 0x002D41E6
		// (set) Token: 0x060057BC RID: 22460 RVA: 0x002D5FEE File Offset: 0x002D41EE
		public bool ShouldBeOpen
		{
			get
			{
				return this.isOpen;
			}
			set
			{
				if (this.isOpen != value)
				{
					this.ToggleState(ActionType.OnUse, null);
				}
			}
		}

		// Token: 0x170015DD RID: 5597
		// (get) Token: 0x060057BD RID: 22461 RVA: 0x002D6001 File Offset: 0x002D4201
		public bool IsClosed
		{
			get
			{
				return !this.IsOpen;
			}
		}

		// Token: 0x170015DE RID: 5598
		// (get) Token: 0x060057BE RID: 22462 RVA: 0x002D600C File Offset: 0x002D420C
		public bool IsFullyOpen
		{
			get
			{
				return this.IsOpen && this.OpenState >= 1f;
			}
		}

		// Token: 0x170015DF RID: 5599
		// (get) Token: 0x060057BF RID: 22463 RVA: 0x002D6028 File Offset: 0x002D4228
		public bool IsFullyClosed
		{
			get
			{
				return this.IsClosed && this.OpenState <= 0f;
			}
		}

		// Token: 0x170015E0 RID: 5600
		// (get) Token: 0x060057C0 RID: 22464 RVA: 0x002D6044 File Offset: 0x002D4244
		public bool HasWindow
		{
			get
			{
				return this.Window != Rectangle.Empty;
			}
		}

		// Token: 0x170015E1 RID: 5601
		// (get) Token: 0x060057C1 RID: 22465 RVA: 0x002D6056 File Offset: 0x002D4256
		// (set) Token: 0x060057C2 RID: 22466 RVA: 0x002D605E File Offset: 0x002D425E
		[Serialize(false, IsPropertySaveable.No, "If the door has integrated buttons, it can be opened by interacting with it directly (instead of using buttons wired to it).", "", false)]
		public bool HasIntegratedButtons { get; private set; }

		// Token: 0x170015E2 RID: 5602
		// (get) Token: 0x060057C3 RID: 22467 RVA: 0x002D6067 File Offset: 0x002D4267
		// (set) Token: 0x060057C4 RID: 22468 RVA: 0x002D606F File Offset: 0x002D426F
		[ConditionallyEditable(ConditionallyEditable.ConditionType.HasIntegratedButtons, true)]
		[Serialize(true, IsPropertySaveable.No, "If the door has integrated buttons, should clicking on it perform the default action of opening the door? Can be used in conjunction with the \"activate_out\" output to pass a signal to a circuit without toggling the door when someone tries to open/close the door.", "", false)]
		public bool ToggleWhenClicked { get; private set; }

		// Token: 0x170015E3 RID: 5603
		// (get) Token: 0x060057C5 RID: 22469 RVA: 0x002D6078 File Offset: 0x002D4278
		// (set) Token: 0x060057C6 RID: 22470 RVA: 0x002D6080 File Offset: 0x002D4280
		public float OpenState
		{
			get
			{
				return this.openState;
			}
			set
			{
				this.lastOpenState = this.openState;
				this.openState = MathHelper.Clamp(value, 0f, 1f);
				float size = (float)(this.IsHorizontal ? this.item.Rect.Width : this.item.Rect.Height);
				if (Math.Abs(this.lastConvexHullState - this.openState) * size > 5f || (this.openState <= 0f && this.lastConvexHullState > 0f) || (this.openState >= 1f && this.lastConvexHullState < 1f))
				{
					this.UpdateConvexHulls();
					this.lastConvexHullState = this.openState;
				}
			}
		}

		// Token: 0x170015E4 RID: 5604
		// (get) Token: 0x060057C7 RID: 22471 RVA: 0x002D613C File Offset: 0x002D433C
		// (set) Token: 0x060057C8 RID: 22472 RVA: 0x002D6144 File Offset: 0x002D4344
		[Serialize(false, IsPropertySaveable.No, "Characters and items cannot pass through impassable doors. Useful for things such as ducts that should only let water and air through.", "", false)]
		public bool Impassable { get; set; }

		// Token: 0x170015E5 RID: 5605
		// (get) Token: 0x060057C9 RID: 22473 RVA: 0x002D614D File Offset: 0x002D434D
		// (set) Token: 0x060057CA RID: 22474 RVA: 0x002D6155 File Offset: 0x002D4355
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "", "", true)]
		public bool UseBetweenOutpostModules { get; private set; }

		// Token: 0x170015E6 RID: 5606
		// (get) Token: 0x060057CB RID: 22475 RVA: 0x002D615E File Offset: 0x002D435E
		// (set) Token: 0x060057CC RID: 22476 RVA: 0x002D6166 File Offset: 0x002D4366
		[Editable]
		[Serialize(false, IsPropertySaveable.No, "If true, bots won't try to close this door behind them.", "", true)]
		public bool BotsShouldKeepOpen { get; private set; }

		// Token: 0x060057CD RID: 22477 RVA: 0x002D6170 File Offset: 0x002D4370
		public Door(Item item, ContentXElement element) : base(item, element)
		{
			this.IsHorizontal = element.GetAttributeBool("horizontal", false);
			this.canBePicked = element.GetAttributeBool("canbepicked", false);
			this.autoOrientGap = element.GetAttributeBool("autoorientgap", false);
			this.allowedSlots.Clear();
			foreach (ContentXElement subElement in element.Elements())
			{
				string textureDir = base.GetTextureDirectory(subElement);
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "sprite"))
				{
					if (!(a == "weldedsprite"))
					{
						if (a == "brokensprite")
						{
							this.brokenSprite = new Sprite(subElement, textureDir, "", false, 1f);
							this.scaleBrokenSprite = subElement.GetAttributeBool("scale", false);
							this.fadeBrokenSprite = subElement.GetAttributeBool("fade", false);
						}
					}
					else
					{
						this.weldedSprite = new Sprite(subElement, textureDir, "", false, 1f);
					}
				}
				else
				{
					this.doorSprite = new Sprite(subElement, textureDir, "", false, 1f);
				}
			}
			this.IsActive = true;
			Door.doorList.Add(this);
		}

		// Token: 0x060057CE RID: 22478 RVA: 0x002D62FC File Offset: 0x002D44FC
		public override void OnItemLoaded()
		{
			this.doorRect = new Rectangle(this.item.Rect.Center.X - (int)(this.doorSprite.size.X / 2f * this.item.Scale), this.item.Rect.Y - this.item.Rect.Height / 2 + (int)(this.doorSprite.size.Y / 2f * this.item.Scale), (int)(this.doorSprite.size.X * this.item.Scale), (int)(this.doorSprite.size.Y * this.item.Scale));
			this.Body = new PhysicsBody(ConvertUnits.ToSimUnits(Math.Max(this.doorRect.Width, 1)), ConvertUnits.ToSimUnits(Math.Max(this.doorRect.Height, 1)), 0f, 1.5f, BodyType.Static, Category.Cat1, Category.Cat2 | Category.Cat5 | Category.Cat6 | Category.Cat7, false)
			{
				UserData = this.item,
				Friction = 0.5f
			};
			this.Body.SetTransformIgnoreContacts(ConvertUnits.ToSimUnits(new Vector2((float)this.doorRect.Center.X, (float)(this.doorRect.Y - this.doorRect.Height / 2))), 0f, true);
			if (this.isBroken)
			{
				this.DisableBody();
			}
		}

		// Token: 0x060057CF RID: 22479 RVA: 0x002D6484 File Offset: 0x002D4684
		public override void Move(Vector2 amount, bool ignoreContacts = false)
		{
			if (ignoreContacts)
			{
				PhysicsBody body = this.Body;
				if (body != null)
				{
					body.SetTransformIgnoreContacts(this.Body.SimPosition + ConvertUnits.ToSimUnits(amount), 0f, true);
				}
			}
			else
			{
				PhysicsBody body2 = this.Body;
				if (body2 != null)
				{
					body2.SetTransform(this.Body.SimPosition + ConvertUnits.ToSimUnits(amount), 0f, true);
				}
			}
			this.UpdateConvexHulls();
		}

		// Token: 0x060057D0 RID: 22480 RVA: 0x002D64F8 File Offset: 0x002D46F8
		public override bool HasRequiredItems(Character character, bool addMessage, LocalizedString msg = null)
		{
			if (this.IsBroken)
			{
				return false;
			}
			if (this.isOpen)
			{
				base.Msg = (this.HasAccess(character) ? "ItemMsgClose" : "ItemMsgForceCloseCrowbar");
			}
			else
			{
				base.Msg = (this.HasAccess(character) ? "ItemMsgOpen" : "ItemMsgForceOpenCrowbar");
			}
			this.ParseMsg();
			if (addMessage && msg == null)
			{
				msg = (this.HasIntegratedButtons ? this.accessDeniedTxt : this.cannotOpenText).Value;
			}
			return base.HasRequiredItems(character, addMessage, msg);
		}

		// Token: 0x060057D1 RID: 22481 RVA: 0x002D6588 File Offset: 0x002D4788
		public override bool Pick(Character picker)
		{
			return (this.item.Condition < this.RepairThreshold && this.item.GetComponent<Repairable>().HasRequiredItems(picker, false, null)) || (!this.RequiredItems.None(null) && (!this.HasAccess(picker) || !this.HasRequiredItems(picker, false, null)) && base.Pick(picker));
		}

		// Token: 0x060057D2 RID: 22482 RVA: 0x002D65EC File Offset: 0x002D47EC
		public override bool OnPicked(Character picker)
		{
			if (this.item.Condition < this.RepairThreshold && this.item.GetComponent<Repairable>().HasRequiredItems(picker, false, null))
			{
				return true;
			}
			if (!this.HasAccess(picker))
			{
				this.ToggleState(ActionType.OnPicked, picker);
				base.ApplyStatusEffects(ActionType.OnPicked, 1f, picker, null, null, null, null, 1f);
			}
			return false;
		}

		// Token: 0x060057D3 RID: 22483 RVA: 0x002D6654 File Offset: 0x002D4854
		private void ToggleState(ActionType actionType, Character user)
		{
			if (this.toggleCooldownTimer > 0f && user != this.lastUser)
			{
				this.OnFailedToOpen();
				return;
			}
			if (this.ToggleWhenClicked)
			{
				this.toggleCooldownTimer = this.ToggleCoolDown;
			}
			if (this.IsStuck || this.IsJammed)
			{
				if (this.IsStuck)
				{
					HintManager.OnTryOpenStuckDoor(user);
				}
				this.toggleCooldownTimer = 1f;
				this.OnFailedToOpen();
				return;
			}
			this.item.SendSignal("1", "activate_out");
			this.lastUser = user;
			if (this.ToggleWhenClicked)
			{
				this.SetState((this.PredictedState == null) ? (!this.isOpen) : (!this.PredictedState.Value), false, true, actionType == ActionType.OnPicked);
			}
		}

		// Token: 0x060057D4 RID: 22484 RVA: 0x002D6720 File Offset: 0x002D4920
		public override bool Select(Character character)
		{
			if (this.isBroken)
			{
				return true;
			}
			bool hasRequiredItems = this.HasRequiredItems(character, false, null);
			if (this.HasAccess(character))
			{
				float originalPickingTime = base.PickingTime;
				base.PickingTime = 0f;
				this.ToggleState(ActionType.OnUse, character);
				base.PickingTime = originalPickingTime;
				base.StopPicking(this.picker);
				return true;
			}
			if (hasRequiredItems && character != null && character == Character.Controlled)
			{
				GUI.AddMessage(this.accessDeniedTxt, GUIStyle.Red, null, true, null);
			}
			return false;
		}

		// Token: 0x060057D5 RID: 22485 RVA: 0x002D67A8 File Offset: 0x002D49A8
		public bool IsPositionOnWindow(Vector2 position, float maxPerpendicularDistance = 10f)
		{
			if (this.IsHorizontal)
			{
				return position.X >= (float)(this.item.Rect.X + this.Window.X) && position.X <= (float)(this.item.Rect.X + this.Window.X + this.Window.Width) && position.Y >= (float)this.item.Rect.Y - maxPerpendicularDistance && position.Y <= (float)(this.item.Rect.Y - this.item.Rect.Height) - maxPerpendicularDistance;
			}
			return position.Y >= (float)(this.item.Rect.Y + this.Window.Y) && position.Y <= (float)(this.item.Rect.Y + this.Window.Y + this.Window.Height) && position.X >= (float)this.item.Rect.X - maxPerpendicularDistance && position.X <= (float)this.item.Rect.Right + maxPerpendicularDistance;
		}

		// Token: 0x060057D6 RID: 22486 RVA: 0x002D68F8 File Offset: 0x002D4AF8
		public override void Update(float deltaTime, Camera cam)
		{
			this.UpdateProjSpecific(deltaTime);
			this.toggleCooldownTimer -= deltaTime;
			this.damageSoundCooldown -= deltaTime;
			if (this.isBroken)
			{
				this.lastBrokenTime = Timing.TotalTime;
				if (this.item.ConditionPercentage * Math.Max(this.item.MaxRepairConditionMultiplier, 1f) > 50f && (GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer))
				{
					this.IsBroken = false;
				}
				return;
			}
			bool isClosing = false;
			if ((!this.IsStuck && !this.IsJammed) || !this.isOpen)
			{
				if (this.PredictedState == null)
				{
					this.OpenState += deltaTime * (this.isOpen ? this.OpeningSpeed : (-this.ClosingSpeed));
					float num = this.openState;
					isClosing = (num > 0f && num < 1f && !this.isOpen);
				}
				else
				{
					this.OpenState += deltaTime * (this.PredictedState.Value ? this.OpeningSpeed : (-this.ClosingSpeed));
					float num = this.openState;
					isClosing = (num > 0f && num < 1f && !this.PredictedState.Value);
					this.resetPredictionTimer -= deltaTime;
					if (this.resetPredictionTimer <= 0f)
					{
						this.PredictedState = null;
					}
				}
				this.LinkedGap.Open = (this.isBroken ? 1f : this.openState);
			}
			if (isClosing)
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				float pushCharactersAwayThreshold = (networkMember != null && networkMember.IsServer) ? 0.1f : 0.9f;
				if (this.OpenState < pushCharactersAwayThreshold)
				{
					this.PushCharactersAway();
				}
				if (this.CheckSubmarinesInDoorWay())
				{
					this.PredictedState = null;
					this.isOpen = true;
				}
			}
			else
			{
				bool wasEnabled = this.Body.Enabled;
				this.Body.Enabled = (this.Impassable || this.openState < 1f);
				if (this.OutsideSubmarineFixture != null)
				{
					this.OutsideSubmarineFixture.CollidesWith = (this.Body.Enabled ? (Category.Cat1 | Category.Cat2 | Category.Cat5 | Category.Cat7 | Category.Cat8) : Category.None);
				}
				if (wasEnabled && !this.Body.Enabled && this.IsHorizontal)
				{
					foreach (Character c in Character.CharacterList)
					{
						if (c.WorldPosition.Y >= this.item.WorldPosition.Y && c.WorldPosition.X >= (float)this.item.WorldRect.X && c.WorldPosition.X <= (float)this.item.WorldRect.Right)
						{
							AnimController animController = c.AnimController;
							if (animController != null)
							{
								animController.ForceRefreshFloorY();
							}
						}
					}
				}
			}
			this.item.SendSignal(this.isOpen ? "1" : "0", "state_out");
		}

		// Token: 0x060057D7 RID: 22487 RVA: 0x002D6C4C File Offset: 0x002D4E4C
		private void UpdateProjSpecific(float deltaTime)
		{
			if (this.shakeTimer > 0f)
			{
				this.shakeTimer -= deltaTime;
				Vector2 noisePos = new Vector2((float)PerlinNoise.CalculatePerlin((double)(this.shakeTimer * 10f), (double)(this.shakeTimer * 10f), 0.0) - 0.5f, (float)PerlinNoise.CalculatePerlin((double)(this.shakeTimer * 10f), (double)(this.shakeTimer * 10f), 0.5) - 0.5f);
				this.shakePos = noisePos * this.shake * 2f;
				this.shake = Math.Min(this.shake, this.shakeTimer * 10f);
			}
			else
			{
				this.shakePos = Vector2.Zero;
			}
			Character character = Character.Controlled;
			if (character != null && character.FocusedItem == this.item && (this.IsFullyOpen || this.IsFullyClosed) && MathF.Abs(this.openState - this.lastOpenState) > 0f)
			{
				CharacterHUD.RecreateHudTexts = true;
			}
		}

		// Token: 0x060057D8 RID: 22488 RVA: 0x002D6D68 File Offset: 0x002D4F68
		public override void UpdateBroken(float deltaTime, Camera cam)
		{
			base.UpdateBroken(deltaTime, cam);
			if (GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer)
			{
				this.IsBroken = true;
			}
		}

		// Token: 0x060057D9 RID: 22489 RVA: 0x002D6D8C File Offset: 0x002D4F8C
		private void EnableBody()
		{
			if (!this.Impassable)
			{
				this.Body.FarseerBody.SetIsSensor(false);
				ContactEdge ce = this.Body.FarseerBody.ContactList;
				while (ce != null && ce.Contact != null)
				{
					ce.Contact.Enabled = false;
					ce = ce.Next;
				}
				this.PushCharactersAway();
			}
			if (this.OutsideSubmarineFixture != null && this.Body.Enabled)
			{
				this.OutsideSubmarineFixture.CollidesWith = (Category.Cat1 | Category.Cat2 | Category.Cat5 | Category.Cat7 | Category.Cat8);
			}
			this.UpdateConvexHulls();
			this.isBroken = false;
		}

		// Token: 0x060057DA RID: 22490 RVA: 0x002D6E1C File Offset: 0x002D501C
		private void DisableBody()
		{
			if (!this.Impassable)
			{
				this.Body.FarseerBody.SetIsSensor(true);
				ContactEdge ce = this.Body.FarseerBody.ContactList;
				while (ce != null && ce.Contact != null)
				{
					ce.Contact.Enabled = false;
					ce = ce.Next;
				}
			}
			if (this.OutsideSubmarineFixture != null)
			{
				this.OutsideSubmarineFixture.CollidesWith = Category.None;
			}
			if (this.linkedGap != null)
			{
				this.linkedGap.Open = 1f;
			}
			this.IsOpen = false;
			if (this.convexHull != null)
			{
				this.convexHull.Enabled = false;
			}
			if (this.convexHull2 != null)
			{
				this.convexHull2.Enabled = false;
			}
		}

		// Token: 0x060057DB RID: 22491 RVA: 0x002D6ED0 File Offset: 0x002D50D0
		public void RefreshLinkedGap()
		{
			this.LinkedGap.Layer = this.item.Layer;
			this.LinkedGap.ConnectedDoor = this;
			if (this.autoOrientGap)
			{
				this.LinkedGap.AutoOrient();
			}
			this.LinkedGap.Open = (this.isBroken ? 1f : this.openState);
			this.LinkedGap.PassAmbientLight = (this.Window != Rectangle.Empty);
		}

		// Token: 0x060057DC RID: 22492 RVA: 0x002D6F50 File Offset: 0x002D5150
		public override void OnMapLoaded()
		{
			this.RefreshLinkedGap();
			this.convexHull = new ConvexHull(this.doorRect, this.IsConvexHullHorizontal, this.item);
			if (this.Window != Rectangle.Empty)
			{
				this.convexHull2 = new ConvexHull(this.doorRect, this.IsConvexHullHorizontal, this.item);
			}
			this.UpdateConvexHulls();
		}

		// Token: 0x060057DD RID: 22493 RVA: 0x002D6FB5 File Offset: 0x002D51B5
		public override void OnScaleChanged()
		{
			this.UpdateConvexHulls();
			if (this.linkedGap != null)
			{
				this.RefreshLinkedGap();
				this.linkedGap.Rect = this.item.Rect;
			}
		}

		// Token: 0x060057DE RID: 22494 RVA: 0x002D6FE4 File Offset: 0x002D51E4
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			if (this.Body != null)
			{
				this.Body.Remove();
				this.Body = null;
			}
			foreach (Gap gap in Gap.GapList)
			{
				if (gap.ConnectedDoor == this)
				{
					gap.ConnectedDoor = null;
				}
			}
			if (this.OutsideSubmarineFixture != null)
			{
				this.OutsideSubmarineFixture.Body.Remove(this.OutsideSubmarineFixture);
				this.OutsideSubmarineFixture = null;
			}
			if (!Submarine.Unloading)
			{
				Gap gap2 = this.linkedGap;
				if (gap2 != null)
				{
					gap2.Remove();
				}
			}
			Sprite sprite = this.doorSprite;
			if (sprite != null)
			{
				sprite.Remove();
			}
			Sprite sprite2 = this.weldedSprite;
			if (sprite2 != null)
			{
				sprite2.Remove();
			}
			ConvexHull convexHull = this.convexHull;
			if (convexHull != null)
			{
				convexHull.Remove();
			}
			ConvexHull convexHull2 = this.convexHull2;
			if (convexHull2 != null)
			{
				convexHull2.Remove();
			}
			Door.doorList.Remove(this);
		}

		// Token: 0x060057DF RID: 22495 RVA: 0x002D70EC File Offset: 0x002D52EC
		private bool CheckSubmarinesInDoorWay()
		{
			if (this.linkedGap != null && this.linkedGap.IsRoomToRoom)
			{
				return false;
			}
			Rectangle doorRect = this.item.WorldRect;
			if (this.IsHorizontal)
			{
				doorRect.Width = (int)((float)this.item.Rect.Width * (1f - this.openState));
			}
			else
			{
				doorRect.Height = (int)((float)this.item.Rect.Height * (1f - this.openState));
			}
			foreach (Submarine sub in Submarine.Loaded)
			{
				if (sub != this.item.Submarine && !sub.DockedTo.Contains(this.item.Submarine))
				{
					Rectangle worldBorders = sub.Borders;
					worldBorders.Location += sub.WorldPosition.ToPoint();
					if (Submarine.RectsOverlap(worldBorders, doorRect, true))
					{
						foreach (Hull hull in sub.GetHulls(false))
						{
							if (Submarine.RectsOverlap(hull.WorldRect, doorRect, true))
							{
								return true;
							}
						}
					}
				}
			}
			return false;
		}

		// Token: 0x060057E0 RID: 22496 RVA: 0x002D7268 File Offset: 0x002D5468
		private void PushCharactersAway()
		{
			if (!MathUtils.IsValid(this.item.SimPosition))
			{
				if (!this.itemPosErrorShown)
				{
					DebugConsole.ThrowError("Failed to push a character out of a doorway - position of the door is not valid (" + this.item.SimPosition.ToString() + ")", null, null, false, false);
					GameAnalyticsManager.AddErrorEventOnce("PushCharactersAway:DoorPosInvalid", GameAnalyticsManager.ErrorSeverity.Error, "Failed to push a character out of a doorway - position of the door is not valid (" + this.item.SimPosition.ToString() + ").");
					this.itemPosErrorShown = true;
				}
				return;
			}
			Vector2 simPos = ConvertUnits.ToSimUnits(new Vector2((float)this.item.Rect.X, (float)this.item.Rect.Y));
			Vector2 currSize = this.IsHorizontal ? new Vector2((float)this.item.Rect.Width * (1f - this.openState), this.doorSprite.size.Y * this.item.Scale) : new Vector2(this.doorSprite.size.X * this.item.Scale, (float)this.item.Rect.Height * (1f - this.openState));
			Vector2 simSize = ConvertUnits.ToSimUnits(currSize);
			foreach (Character c in Character.CharacterList)
			{
				if (c.Enabled)
				{
					Item selectedItem = c.SelectedItem;
					Controller controller = (selectedItem != null) ? selectedItem.GetComponent<Controller>() : null;
					if (controller == null || !controller.IsAttachedUser(c))
					{
						if (!MathUtils.IsValid(c.SimPosition))
						{
							if (!this.characterPosErrorShown.Contains(c))
							{
								if (GameSettings.CurrentConfig.VerboseLogging)
								{
									DebugConsole.ThrowError(string.Concat(new string[]
									{
										"Failed to push a character out of a doorway - position of the character \"",
										c.Name,
										"\" is not valid (",
										c.SimPosition.ToString(),
										")"
									}), null, null, false, false);
								}
								GameAnalyticsManager.AddErrorEventOnce("PushCharactersAway:CharacterPosInvalid", GameAnalyticsManager.ErrorSeverity.Error, string.Concat(new string[]
								{
									"Failed to push a character out of a doorway - position of the character \"",
									c.SpeciesName.ToString(),
									"\" is not valid (",
									c.SimPosition.ToString(),
									"). Removed: ",
									c.Removed.ToString(),
									" Remoteplayer: ",
									c.IsRemotePlayer.ToString()
								}));
								this.characterPosErrorShown.Add(c);
							}
						}
						else
						{
							int dir = this.IsHorizontal ? Math.Sign(c.SimPosition.Y - this.item.SimPosition.Y) : Math.Sign(c.SimPosition.X - this.item.SimPosition.X);
							foreach (Limb limb in c.AnimController.Limbs)
							{
								if (!limb.IsSevered && this.PushBodyOutOfDoorway(c, limb.body, dir, simPos, simSize) && this.damageSoundCooldown <= 0f)
								{
									SoundPlayer.PlayDamageSound("LimbBlunt", 1f, limb.body);
									this.damageSoundCooldown = 0.5f;
								}
							}
							this.PushBodyOutOfDoorway(c, c.AnimController.Collider, dir, simPos, simSize);
						}
					}
				}
			}
		}

		// Token: 0x060057E1 RID: 22497 RVA: 0x002D7638 File Offset: 0x002D5838
		private bool PushBodyOutOfDoorway(Character c, PhysicsBody body, int dir, Vector2 doorRectSimPos, Vector2 doorRectSimSize)
		{
			if (!MathUtils.IsValid(body.SimPosition))
			{
				DebugConsole.ThrowError(string.Concat(new string[]
				{
					"Failed to push a limb out of a doorway - position of the body (character \"",
					c.Name,
					"\") is not valid (",
					body.SimPosition.ToString(),
					")"
				}), null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("PushCharactersAway:LimbPosInvalid", GameAnalyticsManager.ErrorSeverity.Error, string.Concat(new string[]
				{
					"Failed to push a character out of a doorway - position of the character \"",
					c.SpeciesName.ToString(),
					"\" is not valid (",
					body.SimPosition.ToString(),
					"). Removed: ",
					c.Removed.ToString(),
					" Remoteplayer: ",
					c.IsRemotePlayer.ToString()
				}));
				return false;
			}
			float diff;
			if (this.IsHorizontal)
			{
				if (body.SimPosition.X < doorRectSimPos.X || body.SimPosition.X > doorRectSimPos.X + doorRectSimSize.X)
				{
					return false;
				}
				diff = body.SimPosition.Y - this.item.SimPosition.Y;
			}
			else
			{
				if (body.SimPosition.Y > doorRectSimPos.Y || body.SimPosition.Y < doorRectSimPos.Y - doorRectSimSize.Y)
				{
					return false;
				}
				diff = body.SimPosition.X - this.item.SimPosition.X;
			}
			if (Math.Sign(diff) != dir)
			{
				if (this.IsHorizontal)
				{
					body.SetTransformIgnoreContacts(new Vector2(body.SimPosition.X, this.item.SimPosition.Y + (float)dir * doorRectSimSize.Y * 2f), body.Rotation, true);
				}
				else
				{
					body.SetTransformIgnoreContacts(new Vector2(this.item.SimPosition.X + (float)dir * doorRectSimSize.X * 1.2f, body.SimPosition.Y), body.Rotation, true);
				}
			}
			if (this.IsHorizontal)
			{
				if (Math.Abs(body.SimPosition.Y - this.item.SimPosition.Y) > doorRectSimSize.Y * 0.5f)
				{
					return false;
				}
				body.ApplyLinearImpulse(new Vector2(this.isOpen ? 0f : 1f, (float)dir * 2f), 64f);
			}
			else
			{
				if (Math.Abs(body.SimPosition.X - this.item.SimPosition.X) > doorRectSimSize.X * 0.5f)
				{
					return false;
				}
				body.ApplyLinearImpulse(new Vector2((float)dir * 2f, this.isOpen ? 0f : -1f), 64f);
			}
			if (this.lastBrokenTime < Timing.TotalTime - 1.0)
			{
				c.SetStun(0.2f, false, false);
			}
			return true;
		}

		// Token: 0x060057E2 RID: 22498 RVA: 0x002D7950 File Offset: 0x002D5B50
		private void OnFailedToOpen()
		{
			if (this.shakeTimer <= 0f)
			{
				base.PlaySound(ActionType.OnFailure, null);
				this.shake = 5f;
				this.shakeTimer = 1f;
			}
		}

		// Token: 0x060057E3 RID: 22499 RVA: 0x002D7980 File Offset: 0x002D5B80
		public override bool HasAccess(Character character)
		{
			if (!this.item.IsInteractable(character))
			{
				return false;
			}
			if (!base.HasAccess(character))
			{
				return false;
			}
			if (this.HasIntegratedButtons)
			{
				return true;
			}
			List<Controller> buttons = base.Item.GetConnectedComponents<Controller>(true, true, null);
			return buttons.None(null) || buttons.Any((Controller b) => b.HasAccess(character));
		}

		// Token: 0x060057E4 RID: 22500 RVA: 0x002D79F8 File Offset: 0x002D5BF8
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			if (this.IsStuck || this.IsJammed || this.IgnoreSignals)
			{
				return;
			}
			bool wasOpen = (this.PredictedState == null) ? this.isOpen : this.PredictedState.Value;
			if (!(connection.Name == "toggle"))
			{
				if (connection.Name == "set_state")
				{
					bool signalOpen = signal.value != "0";
					if (this.IsStuck && signalOpen != wasOpen)
					{
						this.toggleCooldownTimer = 1f;
						this.OnFailedToOpen();
						return;
					}
					this.SetState(signalOpen, false, true, false);
				}
				return;
			}
			if (signal.value == "0")
			{
				return;
			}
			if (this.toggleCooldownTimer > 0f && signal.sender != this.lastUser)
			{
				this.OnFailedToOpen();
				return;
			}
			if (this.IsStuck)
			{
				this.toggleCooldownTimer = 1f;
				this.OnFailedToOpen();
				return;
			}
			this.toggleCooldownTimer = this.ToggleCoolDown;
			this.lastUser = signal.sender;
			this.SetState(!wasOpen, false, true, false);
		}

		// Token: 0x060057E5 RID: 22501 RVA: 0x002D7B17 File Offset: 0x002D5D17
		public void TrySetState(bool open, bool isNetworkMessage, bool sendNetworkMessage = false)
		{
			this.SetState(open, isNetworkMessage, sendNetworkMessage, false);
		}

		// Token: 0x060057E6 RID: 22502 RVA: 0x002D7B24 File Offset: 0x002D5D24
		private void SetState(bool open, bool isNetworkMessage, bool sendNetworkMessage, bool forcedOpen)
		{
			Door.<>c__DisplayClass162_0 CS$<>8__locals1;
			CS$<>8__locals1.forcedOpen = forcedOpen;
			CS$<>8__locals1.open = open;
			CS$<>8__locals1.<>4__this = this;
			if ((this.IsStuck && !isNetworkMessage) || (this.PredictedState == null && this.isOpen == CS$<>8__locals1.open) || (this.PredictedState != null && this.isOpen == this.PredictedState.Value && this.isOpen == CS$<>8__locals1.open))
			{
				return;
			}
			if (GameMain.Client != null && !isNetworkMessage)
			{
				bool open2 = CS$<>8__locals1.open;
				bool? predictedState = this.PredictedState;
				bool stateChanged = !(open2 == predictedState.GetValueOrDefault() & predictedState != null);
				this.PredictedState = new bool?(CS$<>8__locals1.open);
				this.resetPredictionTimer = 1f;
				if (stateChanged && !this.IsBroken)
				{
					this.<SetState>g__PlayInteractionSound|162_0(ref CS$<>8__locals1);
					return;
				}
			}
			else
			{
				bool stateChanged2 = CS$<>8__locals1.open != this.isOpen;
				this.isOpen = CS$<>8__locals1.open;
				if (isNetworkMessage)
				{
					bool open3 = CS$<>8__locals1.open;
					bool? predictedState = this.PredictedState;
					if (open3 == predictedState.GetValueOrDefault() & predictedState != null)
					{
						goto IL_15A;
					}
				}
				base.StopPicking(null);
				if (!this.IsBroken)
				{
					this.<SetState>g__PlayInteractionSound|162_0(ref CS$<>8__locals1);
				}
				if (this.isOpen)
				{
					this.stuck = MathHelper.Clamp(this.stuck - 30f, 0f, 100f);
				}
				IL_15A:
				if (stateChanged2)
				{
					ActionType actionType = CS$<>8__locals1.open ? ActionType.OnOpen : ActionType.OnClose;
					this.item.ApplyStatusEffects(actionType, 1f, null, null, null, false, null);
				}
			}
		}

		// Token: 0x060057E8 RID: 22504 RVA: 0x002D7CCC File Offset: 0x002D5ECC
		[CompilerGenerated]
		internal static Rectangle <Draw>g__getSourceRect|15_1(Sprite sprite, float openState, bool horizontal)
		{
			if (horizontal)
			{
				return new Rectangle((int)((float)sprite.SourceRect.X + sprite.size.X * openState), sprite.SourceRect.Y, (int)(sprite.size.X * (1f - openState)), (int)sprite.size.Y);
			}
			return new Rectangle(sprite.SourceRect.X, (int)((float)sprite.SourceRect.Y + sprite.size.Y * openState), (int)sprite.size.X, (int)(sprite.size.Y * (1f - openState)));
		}

		// Token: 0x060057E9 RID: 22505 RVA: 0x002D7D74 File Offset: 0x002D5F74
		[CompilerGenerated]
		private void <SetState>g__PlayInteractionSound|162_0(ref Door.<>c__DisplayClass162_0 A_1)
		{
			ActionType actionType = ActionType.OnUse;
			if (A_1.forcedOpen)
			{
				actionType = ActionType.OnPicked;
			}
			else if (A_1.open && base.HasSoundsOfType[19])
			{
				actionType = ActionType.OnOpen;
			}
			else if (!A_1.open && base.HasSoundsOfType[20])
			{
				actionType = ActionType.OnClose;
			}
			base.PlaySound(actionType, null);
		}

		// Token: 0x04002CB4 RID: 11444
		private ConvexHull convexHull;

		// Token: 0x04002CB5 RID: 11445
		private ConvexHull convexHull2;

		// Token: 0x04002CB6 RID: 11446
		private float shake;

		// Token: 0x04002CB7 RID: 11447
		private float shakeTimer;

		// Token: 0x04002CB8 RID: 11448
		private Vector2 shakePos;

		// Token: 0x04002CB9 RID: 11449
		private float lastConvexHullState;

		// Token: 0x04002CBB RID: 11451
		private static readonly HashSet<Door> doorList = new HashSet<Door>();

		// Token: 0x04002CBC RID: 11452
		private Gap linkedGap;

		// Token: 0x04002CBD RID: 11453
		private bool isOpen;

		// Token: 0x04002CBE RID: 11454
		private float openState;

		// Token: 0x04002CBF RID: 11455
		private float lastOpenState;

		// Token: 0x04002CC0 RID: 11456
		private readonly Sprite doorSprite;

		// Token: 0x04002CC1 RID: 11457
		private readonly Sprite weldedSprite;

		// Token: 0x04002CC2 RID: 11458
		private readonly Sprite brokenSprite;

		// Token: 0x04002CC3 RID: 11459
		private readonly bool scaleBrokenSprite;

		// Token: 0x04002CC4 RID: 11460
		private readonly bool fadeBrokenSprite;

		// Token: 0x04002CC5 RID: 11461
		private readonly bool autoOrientGap;

		// Token: 0x04002CC6 RID: 11462
		private bool isJammed;

		// Token: 0x04002CC7 RID: 11463
		private bool isStuck;

		// Token: 0x04002CC9 RID: 11465
		private const float StuckReductionOnOpen = 30f;

		// Token: 0x04002CCA RID: 11466
		private float resetPredictionTimer;

		// Token: 0x04002CCB RID: 11467
		private float toggleCooldownTimer;

		// Token: 0x04002CCC RID: 11468
		private Character lastUser;

		// Token: 0x04002CCD RID: 11469
		private float damageSoundCooldown;

		// Token: 0x04002CCE RID: 11470
		private double lastBrokenTime;

		// Token: 0x04002CCF RID: 11471
		private Rectangle doorRect;

		// Token: 0x04002CD0 RID: 11472
		private bool isBroken;

		// Token: 0x04002CD2 RID: 11474
		public Fixture OutsideSubmarineFixture;

		// Token: 0x04002CD3 RID: 11475
		public bool CanBeWelded = true;

		// Token: 0x04002CD4 RID: 11476
		private float stuck;

		// Token: 0x04002CE0 RID: 11488
		private readonly LocalizedString accessDeniedTxt = TextManager.Get("AccessDenied");

		// Token: 0x04002CE1 RID: 11489
		private readonly LocalizedString cannotOpenText = TextManager.Get("DoorMsgCannotOpen");

		// Token: 0x04002CE2 RID: 11490
		private bool itemPosErrorShown;

		// Token: 0x04002CE3 RID: 11491
		private readonly HashSet<Character> characterPosErrorShown = new HashSet<Character>();
	}
}
