using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x0200001F RID: 31
	internal class AITarget
	{
		// Token: 0x06000177 RID: 375 RVA: 0x00008AAC File Offset: 0x00006CAC
		public void Draw(SpriteBatch spriteBatch)
		{
			if (!AITarget.ShowAITargets)
			{
				return;
			}
			Vector2 pos = new Vector2(this.WorldPosition.X, -this.WorldPosition.Y);
			float thickness = 1f / Screen.Selected.Cam.Zoom;
			float offset = MathUtils.VectorToAngle(new Vector2(this.sectorDir.X, -this.sectorDir.Y)) - this.sectorRad / 2f;
			if (this.soundRange > 0f)
			{
				Color color;
				if (this.Entity is Character)
				{
					color = Color.Yellow;
				}
				else if (this.Entity is Item)
				{
					color = Color.Orange;
				}
				else
				{
					color = Color.OrangeRed;
				}
				if (this.sectorRad < 6.2831855f)
				{
					spriteBatch.DrawSector(pos, this.SoundRange, this.sectorRad, 100, color, offset, thickness);
				}
				else
				{
					spriteBatch.DrawCircle(pos, this.SoundRange, 100, color, thickness);
				}
				spriteBatch.DrawCircle(pos, 3f, 8, color, 2f / Screen.Selected.Cam.Zoom);
				GUI.DrawLine(spriteBatch, pos, pos + Vector2.UnitY * this.SoundRange, color, 0f, (float)((int)(1f / Screen.Selected.Cam.Zoom) + 1));
			}
			if (this.sightRange > 0f)
			{
				Color color2;
				if (this.Entity is Character)
				{
					color2 = Color.CornflowerBlue;
				}
				else
				{
					Item i = this.Entity as Item;
					if (i == null)
					{
						return;
					}
					if (i.Submarine != null && i.Container != null)
					{
						return;
					}
					color2 = Color.CadetBlue;
				}
				if (this.sectorRad < 6.2831855f)
				{
					spriteBatch.DrawSector(pos, this.SightRange, this.sectorRad, 100, color2, offset, thickness);
				}
				else
				{
					spriteBatch.DrawCircle(pos, this.SightRange, 100, color2, thickness);
				}
				spriteBatch.DrawCircle(pos, 6f, 8, color2, 2f / Screen.Selected.Cam.Zoom);
				GUI.DrawLine(spriteBatch, pos, pos + Vector2.UnitY * this.SightRange, color2, 0f, (float)((int)(1f / Screen.Selected.Cam.Zoom) + 1));
			}
		}

		// Token: 0x1700003B RID: 59
		// (get) Token: 0x06000178 RID: 376 RVA: 0x00008CEA File Offset: 0x00006EEA
		public Entity Entity
		{
			get
			{
				if (this.entity != null && this.entity.Removed)
				{
					return null;
				}
				return this.entity;
			}
		}

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x06000179 RID: 377 RVA: 0x00008D09 File Offset: 0x00006F09
		// (set) Token: 0x0600017A RID: 378 RVA: 0x00008D11 File Offset: 0x00006F11
		public float FadeOutTime { get; private set; } = 2f;

		// Token: 0x1700003D RID: 61
		// (get) Token: 0x0600017B RID: 379 RVA: 0x00008D1A File Offset: 0x00006F1A
		// (set) Token: 0x0600017C RID: 380 RVA: 0x00008D22 File Offset: 0x00006F22
		public bool Static { get; private set; }

		// Token: 0x1700003E RID: 62
		// (get) Token: 0x0600017D RID: 381 RVA: 0x00008D2B File Offset: 0x00006F2B
		// (set) Token: 0x0600017E RID: 382 RVA: 0x00008D33 File Offset: 0x00006F33
		public bool StaticSound { get; private set; }

		// Token: 0x1700003F RID: 63
		// (get) Token: 0x0600017F RID: 383 RVA: 0x00008D3C File Offset: 0x00006F3C
		// (set) Token: 0x06000180 RID: 384 RVA: 0x00008D44 File Offset: 0x00006F44
		public bool StaticSight { get; private set; }

		// Token: 0x17000040 RID: 64
		// (get) Token: 0x06000181 RID: 385 RVA: 0x00008D4D File Offset: 0x00006F4D
		// (set) Token: 0x06000182 RID: 386 RVA: 0x00008D58 File Offset: 0x00006F58
		public float SoundRange
		{
			get
			{
				return this.soundRange;
			}
			set
			{
				if (float.IsNaN(value))
				{
					DebugConsole.ThrowError("Attempted to set the SoundRange of an AITarget to NaN.\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
					return;
				}
				this.soundRange = MathHelper.Clamp(value, this.MinSoundRange, this.MaxSoundRange);
				if (this.soundRange > 0f && !this.Static && this.FadeOutTime > 0f)
				{
					this.NeedsUpdate = true;
				}
			}
		}

		// Token: 0x17000041 RID: 65
		// (get) Token: 0x06000183 RID: 387 RVA: 0x00008DCC File Offset: 0x00006FCC
		// (set) Token: 0x06000184 RID: 388 RVA: 0x00008DD4 File Offset: 0x00006FD4
		public float SoundRangeOnSonarMultiplier { get; private set; } = 1f;

		// Token: 0x17000042 RID: 66
		// (get) Token: 0x06000185 RID: 389 RVA: 0x00008DDD File Offset: 0x00006FDD
		// (set) Token: 0x06000186 RID: 390 RVA: 0x00008DE8 File Offset: 0x00006FE8
		public float SightRange
		{
			get
			{
				return this.sightRange;
			}
			set
			{
				if (float.IsNaN(value))
				{
					DebugConsole.ThrowError("Attempted to set the SightRange of an AITarget to NaN.\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
					return;
				}
				this.sightRange = MathHelper.Clamp(value, this.MinSightRange, this.MaxSightRange);
				if (this.sightRange > 0f && !this.Static && this.FadeOutTime > 0f)
				{
					this.NeedsUpdate = true;
				}
			}
		}

		// Token: 0x17000043 RID: 67
		// (get) Token: 0x06000187 RID: 391 RVA: 0x00008E5C File Offset: 0x0000705C
		// (set) Token: 0x06000188 RID: 392 RVA: 0x00008E69 File Offset: 0x00007069
		public float SectorDegrees
		{
			get
			{
				return MathHelper.ToDegrees(this.sectorRad);
			}
			set
			{
				this.sectorRad = MathHelper.ToRadians(value);
			}
		}

		// Token: 0x17000044 RID: 68
		// (get) Token: 0x06000189 RID: 393 RVA: 0x00008E77 File Offset: 0x00007077
		// (set) Token: 0x0600018A RID: 394 RVA: 0x00008E80 File Offset: 0x00007080
		public Vector2 SectorDir
		{
			get
			{
				return this.sectorDir;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					string str = "Invalid AITarget sector direction (";
					Vector2 vector = value;
					string errorMsg = str + vector.ToString() + ")\n" + Environment.StackTrace.CleanupStackTrace();
					DebugConsole.ThrowError(errorMsg, null, null, false, false);
					string str2 = "AITarget.SectorDir:";
					Entity entity = this.entity;
					GameAnalyticsManager.AddErrorEventOnce(str2 + ((entity != null) ? entity.ToString() : null), GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
					return;
				}
				this.sectorDir = value;
			}
		}

		// Token: 0x17000045 RID: 69
		// (get) Token: 0x0600018B RID: 395 RVA: 0x00008EF3 File Offset: 0x000070F3
		// (set) Token: 0x0600018C RID: 396 RVA: 0x00008EFB File Offset: 0x000070FB
		public float SonarDisruption { get; set; }

		// Token: 0x17000046 RID: 70
		// (get) Token: 0x0600018D RID: 397 RVA: 0x00008F04 File Offset: 0x00007104
		// (set) Token: 0x0600018E RID: 398 RVA: 0x00008F2F File Offset: 0x0000712F
		public bool InDetectable
		{
			get
			{
				return this.inDetectable || (this.SoundRange <= 0f && this.SightRange <= 0f);
			}
			set
			{
				this.inDetectable = value;
				if (this.inDetectable)
				{
					this.InDetectableSetTime = Timing.TotalTime;
					this.NeedsUpdate = true;
				}
			}
		}

		// Token: 0x17000047 RID: 71
		// (get) Token: 0x0600018F RID: 399 RVA: 0x00008F52 File Offset: 0x00007152
		// (set) Token: 0x06000190 RID: 400 RVA: 0x00008F5A File Offset: 0x0000715A
		public bool NeedsUpdate { get; private set; } = true;

		// Token: 0x17000048 RID: 72
		// (get) Token: 0x06000191 RID: 401 RVA: 0x00008F63 File Offset: 0x00007163
		// (set) Token: 0x06000192 RID: 402 RVA: 0x00008F6B File Offset: 0x0000716B
		public AITarget.TargetType Type { get; private set; }

		// Token: 0x17000049 RID: 73
		// (get) Token: 0x06000193 RID: 403 RVA: 0x00008F74 File Offset: 0x00007174
		public Vector2 WorldPosition
		{
			get
			{
				if (this.entity == null || this.entity.Removed)
				{
					GameAnalyticsManager.AddErrorEventOnce("AITarget.WorldPosition:EntityRemoved", GameAnalyticsManager.ErrorSeverity.Error, "Attempted to access a removed AITarget\n" + Environment.StackTrace.CleanupStackTrace());
					return Vector2.Zero;
				}
				return this.entity.WorldPosition;
			}
		}

		// Token: 0x1700004A RID: 74
		// (get) Token: 0x06000194 RID: 404 RVA: 0x00008FC8 File Offset: 0x000071C8
		public Vector2 SimPosition
		{
			get
			{
				if (this.entity == null || this.entity.Removed)
				{
					GameAnalyticsManager.AddErrorEventOnce("AITarget.WorldPosition:EntityRemoved", GameAnalyticsManager.ErrorSeverity.Error, "Attempted to access a removed AITarget\n" + Environment.StackTrace.CleanupStackTrace());
					return Vector2.Zero;
				}
				return this.entity.SimPosition;
			}
		}

		// Token: 0x06000195 RID: 405 RVA: 0x0000901A File Offset: 0x0000721A
		public bool ShouldBeIgnored()
		{
			return this.InDetectable || this.Entity == null || Level.IsPositionAboveLevel(this.WorldPosition);
		}

		// Token: 0x06000196 RID: 406 RVA: 0x00009040 File Offset: 0x00007240
		public AITarget(Entity e, XElement element) : this(e)
		{
			this.SightRange = element.GetAttributeFloat("sightrange", 0f);
			this.SoundRange = element.GetAttributeFloat("soundrange", 0f);
			this.MinSightRange = element.GetAttributeFloat("minsightrange", 0f);
			this.MinSoundRange = element.GetAttributeFloat("minsoundrange", 0f);
			this.MaxSightRange = element.GetAttributeFloat("maxsightrange", this.SightRange);
			this.MaxSoundRange = element.GetAttributeFloat("maxsoundrange", this.SoundRange);
			this.SoundRangeOnSonarMultiplier = element.GetAttributeFloat("SoundRangeOnSonarMultiplier", 1f);
			this.FadeOutTime = element.GetAttributeFloat("fadeouttime", this.FadeOutTime);
			this.Static = element.GetAttributeBool("static", this.Static);
			this.StaticSight = element.GetAttributeBool("staticsight", this.StaticSight);
			this.StaticSound = element.GetAttributeBool("staticsound", this.StaticSound);
			if (this.Static)
			{
				this.StaticSound = true;
				this.StaticSight = true;
			}
			this.SonarDisruption = element.GetAttributeFloat("sonardisruption", 0f);
			string label = element.GetAttributeString("sonarlabel", "");
			this.SonarLabel = TextManager.Get(label).Fallback(label, true);
			this.SonarIconIdentifier = element.GetAttributeIdentifier("sonaricon", Identifier.Empty);
			this.Type = element.GetAttributeEnum("type", AITarget.TargetType.Any);
			this.Reset();
		}

		// Token: 0x06000197 RID: 407 RVA: 0x000091D0 File Offset: 0x000073D0
		public AITarget(Entity e)
		{
			this.entity = e;
			AITarget.List.Add(this);
		}

		// Token: 0x06000198 RID: 408 RVA: 0x00009234 File Offset: 0x00007434
		public void Update(float deltaTime)
		{
			this.InDetectable = false;
			if (!this.Static && this.FadeOutTime > 0f)
			{
				if (!this.StaticSight && this.sightRange > 0f)
				{
					this.DecreaseSightRange(deltaTime, 1f);
				}
				if (!this.StaticSound && this.soundRange > 0f)
				{
					this.DecreaseSoundRange(deltaTime, 1f);
				}
				if (this.sightRange <= 0f && this.soundRange <= 0f)
				{
					this.NeedsUpdate = false;
					return;
				}
			}
			else
			{
				this.NeedsUpdate = false;
			}
		}

		// Token: 0x06000199 RID: 409 RVA: 0x000092C8 File Offset: 0x000074C8
		public void IncreaseSoundRange(float deltaTime, float speed = 1f)
		{
			this.SoundRange += speed * deltaTime * (this.MaxSoundRange / this.FadeOutTime);
		}

		// Token: 0x0600019A RID: 410 RVA: 0x000092E8 File Offset: 0x000074E8
		public void IncreaseSightRange(float deltaTime, float speed = 1f)
		{
			this.SightRange += speed * deltaTime * (this.MaxSightRange / this.FadeOutTime);
		}

		// Token: 0x0600019B RID: 411 RVA: 0x00009308 File Offset: 0x00007508
		public void DecreaseSoundRange(float deltaTime, float speed = 1f)
		{
			this.SoundRange -= speed * deltaTime * (this.MaxSoundRange / this.FadeOutTime);
		}

		// Token: 0x0600019C RID: 412 RVA: 0x00009328 File Offset: 0x00007528
		public void DecreaseSightRange(float deltaTime, float speed = 1f)
		{
			this.SightRange -= speed * deltaTime * (this.MaxSightRange / this.FadeOutTime);
		}

		// Token: 0x0600019D RID: 413 RVA: 0x00009348 File Offset: 0x00007548
		public bool HasSector()
		{
			return this.sectorRad < 6.2831855f;
		}

		// Token: 0x0600019E RID: 414 RVA: 0x00009358 File Offset: 0x00007558
		public bool IsWithinSector(Vector2 worldPosition)
		{
			if (!this.HasSector())
			{
				return true;
			}
			Vector2 diff = worldPosition - this.WorldPosition;
			return Math.Abs(MathUtils.GetShortestAngle(MathUtils.VectorToAngle(diff), MathUtils.VectorToAngle(this.sectorDir))) <= this.sectorRad * 0.5f;
		}

		// Token: 0x0600019F RID: 415 RVA: 0x000093A8 File Offset: 0x000075A8
		public void Remove()
		{
			AITarget.List.Remove(this);
			this.entity = null;
		}

		// Token: 0x060001A0 RID: 416 RVA: 0x000093C0 File Offset: 0x000075C0
		public void Reset()
		{
			if (this.Static)
			{
				this.SightRange = this.MaxSightRange;
				this.SoundRange = this.MaxSoundRange;
				return;
			}
			this.SightRange = (this.StaticSight ? this.MaxSightRange : this.MinSightRange);
			this.SoundRange = (this.StaticSound ? this.MaxSoundRange : this.MinSoundRange);
		}

		// Token: 0x04000111 RID: 273
		public static bool ShowAITargets;

		// Token: 0x04000112 RID: 274
		public static List<AITarget> List = new List<AITarget>();

		// Token: 0x04000113 RID: 275
		private Entity entity;

		// Token: 0x04000114 RID: 276
		private float soundRange;

		// Token: 0x04000115 RID: 277
		private float sightRange;

		// Token: 0x0400011B RID: 283
		private float sectorRad = 6.2831855f;

		// Token: 0x0400011C RID: 284
		private Vector2 sectorDir;

		// Token: 0x0400011E RID: 286
		public LocalizedString SonarLabel;

		// Token: 0x0400011F RID: 287
		public Identifier SonarIconIdentifier;

		// Token: 0x04000120 RID: 288
		private bool inDetectable;

		// Token: 0x04000121 RID: 289
		public double InDetectableSetTime;

		// Token: 0x04000122 RID: 290
		public float MinSoundRange;

		// Token: 0x04000123 RID: 291
		public float MinSightRange;

		// Token: 0x04000124 RID: 292
		public float MaxSoundRange = 100000f;

		// Token: 0x04000125 RID: 293
		public float MaxSightRange = 100000f;

		// Token: 0x0200063F RID: 1599
		public enum TargetType
		{
			// Token: 0x0400363F RID: 13887
			Any,
			// Token: 0x04003640 RID: 13888
			HumanOnly,
			// Token: 0x04003641 RID: 13889
			EnemyOnly
		}
	}
}
