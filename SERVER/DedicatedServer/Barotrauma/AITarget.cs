using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000055 RID: 85
	internal class AITarget
	{
		// Token: 0x17000348 RID: 840
		// (get) Token: 0x06000C6A RID: 3178 RVA: 0x0007530B File Offset: 0x0007350B
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

		// Token: 0x17000349 RID: 841
		// (get) Token: 0x06000C6B RID: 3179 RVA: 0x0007532A File Offset: 0x0007352A
		// (set) Token: 0x06000C6C RID: 3180 RVA: 0x00075332 File Offset: 0x00073532
		public float FadeOutTime { get; private set; } = 2f;

		// Token: 0x1700034A RID: 842
		// (get) Token: 0x06000C6D RID: 3181 RVA: 0x0007533B File Offset: 0x0007353B
		// (set) Token: 0x06000C6E RID: 3182 RVA: 0x00075343 File Offset: 0x00073543
		public bool Static { get; private set; }

		// Token: 0x1700034B RID: 843
		// (get) Token: 0x06000C6F RID: 3183 RVA: 0x0007534C File Offset: 0x0007354C
		// (set) Token: 0x06000C70 RID: 3184 RVA: 0x00075354 File Offset: 0x00073554
		public bool StaticSound { get; private set; }

		// Token: 0x1700034C RID: 844
		// (get) Token: 0x06000C71 RID: 3185 RVA: 0x0007535D File Offset: 0x0007355D
		// (set) Token: 0x06000C72 RID: 3186 RVA: 0x00075365 File Offset: 0x00073565
		public bool StaticSight { get; private set; }

		// Token: 0x1700034D RID: 845
		// (get) Token: 0x06000C73 RID: 3187 RVA: 0x0007536E File Offset: 0x0007356E
		// (set) Token: 0x06000C74 RID: 3188 RVA: 0x00075378 File Offset: 0x00073578
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

		// Token: 0x1700034E RID: 846
		// (get) Token: 0x06000C75 RID: 3189 RVA: 0x000753EC File Offset: 0x000735EC
		// (set) Token: 0x06000C76 RID: 3190 RVA: 0x000753F4 File Offset: 0x000735F4
		public float SoundRangeOnSonarMultiplier { get; private set; } = 1f;

		// Token: 0x1700034F RID: 847
		// (get) Token: 0x06000C77 RID: 3191 RVA: 0x000753FD File Offset: 0x000735FD
		// (set) Token: 0x06000C78 RID: 3192 RVA: 0x00075408 File Offset: 0x00073608
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

		// Token: 0x17000350 RID: 848
		// (get) Token: 0x06000C79 RID: 3193 RVA: 0x0007547C File Offset: 0x0007367C
		// (set) Token: 0x06000C7A RID: 3194 RVA: 0x00075489 File Offset: 0x00073689
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

		// Token: 0x17000351 RID: 849
		// (get) Token: 0x06000C7B RID: 3195 RVA: 0x00075497 File Offset: 0x00073697
		// (set) Token: 0x06000C7C RID: 3196 RVA: 0x000754A0 File Offset: 0x000736A0
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

		// Token: 0x17000352 RID: 850
		// (get) Token: 0x06000C7D RID: 3197 RVA: 0x00075513 File Offset: 0x00073713
		// (set) Token: 0x06000C7E RID: 3198 RVA: 0x0007551B File Offset: 0x0007371B
		public float SonarDisruption { get; set; }

		// Token: 0x17000353 RID: 851
		// (get) Token: 0x06000C7F RID: 3199 RVA: 0x00075524 File Offset: 0x00073724
		// (set) Token: 0x06000C80 RID: 3200 RVA: 0x0007554F File Offset: 0x0007374F
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

		// Token: 0x17000354 RID: 852
		// (get) Token: 0x06000C81 RID: 3201 RVA: 0x00075572 File Offset: 0x00073772
		// (set) Token: 0x06000C82 RID: 3202 RVA: 0x0007557A File Offset: 0x0007377A
		public bool NeedsUpdate { get; private set; } = true;

		// Token: 0x17000355 RID: 853
		// (get) Token: 0x06000C83 RID: 3203 RVA: 0x00075583 File Offset: 0x00073783
		// (set) Token: 0x06000C84 RID: 3204 RVA: 0x0007558B File Offset: 0x0007378B
		public AITarget.TargetType Type { get; private set; }

		// Token: 0x17000356 RID: 854
		// (get) Token: 0x06000C85 RID: 3205 RVA: 0x00075594 File Offset: 0x00073794
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

		// Token: 0x17000357 RID: 855
		// (get) Token: 0x06000C86 RID: 3206 RVA: 0x000755E8 File Offset: 0x000737E8
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

		// Token: 0x06000C87 RID: 3207 RVA: 0x0007563A File Offset: 0x0007383A
		public bool ShouldBeIgnored()
		{
			return this.InDetectable || this.Entity == null || Level.IsPositionAboveLevel(this.WorldPosition);
		}

		// Token: 0x06000C88 RID: 3208 RVA: 0x00075660 File Offset: 0x00073860
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

		// Token: 0x06000C89 RID: 3209 RVA: 0x000757F0 File Offset: 0x000739F0
		public AITarget(Entity e)
		{
			this.entity = e;
			AITarget.List.Add(this);
		}

		// Token: 0x06000C8A RID: 3210 RVA: 0x00075854 File Offset: 0x00073A54
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

		// Token: 0x06000C8B RID: 3211 RVA: 0x000758E8 File Offset: 0x00073AE8
		public void IncreaseSoundRange(float deltaTime, float speed = 1f)
		{
			this.SoundRange += speed * deltaTime * (this.MaxSoundRange / this.FadeOutTime);
		}

		// Token: 0x06000C8C RID: 3212 RVA: 0x00075908 File Offset: 0x00073B08
		public void IncreaseSightRange(float deltaTime, float speed = 1f)
		{
			this.SightRange += speed * deltaTime * (this.MaxSightRange / this.FadeOutTime);
		}

		// Token: 0x06000C8D RID: 3213 RVA: 0x00075928 File Offset: 0x00073B28
		public void DecreaseSoundRange(float deltaTime, float speed = 1f)
		{
			this.SoundRange -= speed * deltaTime * (this.MaxSoundRange / this.FadeOutTime);
		}

		// Token: 0x06000C8E RID: 3214 RVA: 0x00075948 File Offset: 0x00073B48
		public void DecreaseSightRange(float deltaTime, float speed = 1f)
		{
			this.SightRange -= speed * deltaTime * (this.MaxSightRange / this.FadeOutTime);
		}

		// Token: 0x06000C8F RID: 3215 RVA: 0x00075968 File Offset: 0x00073B68
		public bool HasSector()
		{
			return this.sectorRad < 6.2831855f;
		}

		// Token: 0x06000C90 RID: 3216 RVA: 0x00075978 File Offset: 0x00073B78
		public bool IsWithinSector(Vector2 worldPosition)
		{
			if (!this.HasSector())
			{
				return true;
			}
			Vector2 diff = worldPosition - this.WorldPosition;
			return Math.Abs(MathUtils.GetShortestAngle(MathUtils.VectorToAngle(diff), MathUtils.VectorToAngle(this.sectorDir))) <= this.sectorRad * 0.5f;
		}

		// Token: 0x06000C91 RID: 3217 RVA: 0x000759C8 File Offset: 0x00073BC8
		public void Remove()
		{
			AITarget.List.Remove(this);
			this.entity = null;
		}

		// Token: 0x06000C92 RID: 3218 RVA: 0x000759E0 File Offset: 0x00073BE0
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

		// Token: 0x0400055A RID: 1370
		public static List<AITarget> List = new List<AITarget>();

		// Token: 0x0400055B RID: 1371
		private Entity entity;

		// Token: 0x0400055C RID: 1372
		private float soundRange;

		// Token: 0x0400055D RID: 1373
		private float sightRange;

		// Token: 0x04000563 RID: 1379
		private float sectorRad = 6.2831855f;

		// Token: 0x04000564 RID: 1380
		private Vector2 sectorDir;

		// Token: 0x04000566 RID: 1382
		public LocalizedString SonarLabel;

		// Token: 0x04000567 RID: 1383
		public Identifier SonarIconIdentifier;

		// Token: 0x04000568 RID: 1384
		private bool inDetectable;

		// Token: 0x04000569 RID: 1385
		public double InDetectableSetTime;

		// Token: 0x0400056A RID: 1386
		public float MinSoundRange;

		// Token: 0x0400056B RID: 1387
		public float MinSightRange;

		// Token: 0x0400056C RID: 1388
		public float MaxSoundRange = 100000f;

		// Token: 0x0400056D RID: 1389
		public float MaxSightRange = 100000f;

		// Token: 0x0200076D RID: 1901
		public enum TargetType
		{
			// Token: 0x04002CF6 RID: 11510
			Any,
			// Token: 0x04002CF7 RID: 11511
			HumanOnly,
			// Token: 0x04002CF8 RID: 11512
			EnemyOnly
		}
	}
}
