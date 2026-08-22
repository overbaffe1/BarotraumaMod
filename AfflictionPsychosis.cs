using System;
using System.Collections.Generic;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200002B RID: 43
	internal class AfflictionPsychosis : Affliction
	{
		// Token: 0x170001F7 RID: 503
		// (get) Token: 0x060006FE RID: 1790 RVA: 0x0003D687 File Offset: 0x0003B887
		public AfflictionPsychosis.FloodType CurrentFloodType
		{
			get
			{
				return this.currentFloodType;
			}
		}

		// Token: 0x060006FF RID: 1791 RVA: 0x0003D690 File Offset: 0x0003B890
		private void UpdateSounds(Character character, float deltaTime)
		{
			if (this.soundTimer < MathHelper.Lerp(240f, 60f, this.Strength / 100f))
			{
				this.soundTimer += deltaTime;
				return;
			}
			float impactStrength = MathHelper.Lerp(0.1f, 1f, this.Strength / 100f);
			SoundPlayer.PlayDamageSound("StructureBlunt", Rand.Range(10f, 1000f, Rand.RandSync.Unsynced), character.WorldPosition + Rand.Vector(500f, Rand.RandSync.Unsynced), 2000f, null, 1f);
			GameMain.GameScreen.Cam.Shake = impactStrength * 10f;
			GameMain.GameScreen.Cam.AngularVelocity = Rand.Range(-impactStrength, impactStrength, Rand.RandSync.Unsynced);
			this.soundTimer = 0f;
		}

		// Token: 0x06000700 RID: 1792 RVA: 0x0003D760 File Offset: 0x0003B960
		private void UpdateFloods(float deltaTime)
		{
			if (this.currentFloodDuration > 0f)
			{
				this.currentFloodDuration -= deltaTime;
				switch (this.currentFloodType)
				{
				case AfflictionPsychosis.FloodType.Minor:
					this.currentFloodState += deltaTime;
					using (List<Hull>.Enumerator enumerator = Hull.HullList.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Hull hull = enumerator.Current;
							for (int i = hull.FakeFireSources.Count - 1; i >= 0; i--)
							{
								hull.FakeFireSources[i].Extinguish(deltaTime, 50f);
							}
							hull.DrawSurface = (float)(hull.Rect.Y - hull.Rect.Height) + MathHelper.Lerp(0f, 15f, this.currentFloodState / 10f);
						}
						return;
					}
					break;
				case AfflictionPsychosis.FloodType.Major:
					break;
				case AfflictionPsychosis.FloodType.HideFlooding:
					goto IL_18E;
				default:
					return;
				}
				this.currentFloodState += deltaTime;
				using (List<Hull>.Enumerator enumerator2 = Hull.HullList.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						Hull hull2 = enumerator2.Current;
						for (int j = hull2.FakeFireSources.Count - 1; j >= 0; j--)
						{
							hull2.FakeFireSources[j].Extinguish(deltaTime, 200f);
						}
						hull2.DrawSurface = (float)hull2.Rect.Y - MathHelper.Lerp((float)hull2.Rect.Height, 0f, this.currentFloodState / 10f);
					}
					return;
				}
				IL_18E:
				foreach (Hull hull3 in Hull.HullList)
				{
					hull3.DrawSurface = (float)(hull3.Rect.Y - hull3.Rect.Height);
				}
				return;
			}
			if (this.createFloodTimer < MathHelper.Lerp(240f, 60f, this.Strength / 100f))
			{
				this.currentFloodType = AfflictionPsychosis.FloodType.None;
				this.createFloodTimer += deltaTime;
				return;
			}
			if (Rand.Range(0f, 100f, Rand.RandSync.Unsynced) < this.Strength)
			{
				if (Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < 0.5f)
				{
					this.currentFloodType = AfflictionPsychosis.FloodType.HideFlooding;
					this.currentFloodType = AfflictionPsychosis.FloodType.Minor;
				}
				else
				{
					this.currentFloodType = AfflictionPsychosis.FloodType.Minor;
				}
				this.currentFloodDuration = Rand.Range(20f, 100f, Rand.RandSync.Unsynced);
			}
			this.createFloodTimer = 0f;
		}

		// Token: 0x06000701 RID: 1793 RVA: 0x0003DA18 File Offset: 0x0003BC18
		private void UpdateFires(Character character, float deltaTime)
		{
			this.createFireSourceTimer += deltaTime;
			this.fakeFireSources.RemoveAll((DummyFireSource fs) => fs.Removed);
			if (this.fakeFireSources.Count < 10 && character.Submarine != null && this.createFireSourceTimer > MathHelper.Lerp(240f, 30f, this.Strength / 100f))
			{
				Hull fireHull = Hull.HullList.GetRandomUnsynced((Hull h) => h.Submarine == character.Submarine);
				if (fireHull != null)
				{
					DummyFireSource fakeFire = new DummyFireSource(Vector2.One * 500f, new Vector2((float)Rand.Range(fireHull.WorldRect.X, fireHull.WorldRect.Right, Rand.RandSync.Unsynced), fireHull.WorldPosition.Y + 1f), fireHull, true)
					{
						CausedByPsychosis = true,
						DamagesItems = false,
						DamagesCharacters = false
					};
					this.fakeFireSources.Add(fakeFire);
					this.createFireSourceTimer = 0f;
				}
			}
		}

		// Token: 0x06000702 RID: 1794 RVA: 0x0003DB48 File Offset: 0x0003BD48
		private void UpdateInvisibleCharacters(float deltaTime)
		{
			this.invisibleCharacterTimer -= deltaTime;
			if (this.invisibleCharacterTimer > 0f)
			{
				return;
			}
			foreach (Character c in Character.CharacterList)
			{
				if (!c.IsDead && c != Character.Controlled && c.WorldPosition.X >= (float)GameMain.GameScreen.Cam.WorldView.X && c.WorldPosition.X <= (float)GameMain.GameScreen.Cam.WorldView.Right && c.WorldPosition.Y >= (float)(GameMain.GameScreen.Cam.WorldView.Y - GameMain.GameScreen.Cam.WorldView.Height) && c.WorldPosition.Y <= (float)GameMain.GameScreen.Cam.WorldView.Y && Rand.Range(0f, 500f, Rand.RandSync.Unsynced) < this.Strength)
				{
					c.InvisibleTimer = 60f;
				}
			}
			this.invisibleCharacterTimer = this.invisibleCharacterInterval;
		}

		// Token: 0x06000703 RID: 1795 RVA: 0x0003DCA0 File Offset: 0x0003BEA0
		private void UpdateFakeBroken(float deltaTime)
		{
			this.fakeBrokenTimer -= deltaTime;
			if (this.fakeBrokenTimer > 0f)
			{
				return;
			}
			foreach (Item item in Item.RepairableItems)
			{
				Repairable repairable = item.GetComponent<Repairable>();
				if (repairable != null && this.ShouldFakeBrokenItem(item))
				{
					repairable.FakeBrokenTimer = 60f;
				}
			}
			this.fakeBrokenTimer = this.fakeBrokenInterval;
		}

		// Token: 0x06000704 RID: 1796 RVA: 0x0003DD2C File Offset: 0x0003BF2C
		private bool ShouldFakeBrokenItem(Item item)
		{
			return Rand.Range(0f, 1000f, Rand.RandSync.Unsynced) < this.Strength;
		}

		// Token: 0x06000705 RID: 1797 RVA: 0x0003DD46 File Offset: 0x0003BF46
		public AfflictionPsychosis(AfflictionPrefab prefab, float strength) : base(prefab, strength)
		{
		}

		// Token: 0x06000706 RID: 1798 RVA: 0x0003DD71 File Offset: 0x0003BF71
		public override void Update(CharacterHealth characterHealth, Limb targetLimb, float deltaTime)
		{
			base.Update(characterHealth, targetLimb, deltaTime);
			this.UpdateProjSpecific(characterHealth, targetLimb, deltaTime);
		}

		// Token: 0x06000707 RID: 1799 RVA: 0x0003DD85 File Offset: 0x0003BF85
		private void UpdateProjSpecific(CharacterHealth characterHealth, Limb targetLimb, float deltaTime)
		{
			if (Character.Controlled != characterHealth.Character)
			{
				return;
			}
			this.UpdateFloods(deltaTime);
			this.UpdateSounds(characterHealth.Character, deltaTime);
			this.UpdateFires(characterHealth.Character, deltaTime);
			this.UpdateInvisibleCharacters(deltaTime);
			this.UpdateFakeBroken(deltaTime);
		}

		// Token: 0x04000395 RID: 917
		private const int MaxFakeFireSources = 10;

		// Token: 0x04000396 RID: 918
		private const float MinFakeFireSourceInterval = 30f;

		// Token: 0x04000397 RID: 919
		private const float MaxFakeFireSourceInterval = 240f;

		// Token: 0x04000398 RID: 920
		private float createFireSourceTimer;

		// Token: 0x04000399 RID: 921
		private readonly List<DummyFireSource> fakeFireSources = new List<DummyFireSource>();

		// Token: 0x0400039A RID: 922
		private const float MinSoundInterval = 60f;

		// Token: 0x0400039B RID: 923
		private const float MaxSoundInterval = 240f;

		// Token: 0x0400039C RID: 924
		private AfflictionPsychosis.FloodType currentFloodType;

		// Token: 0x0400039D RID: 925
		private float soundTimer;

		// Token: 0x0400039E RID: 926
		private const float MinFloodInterval = 60f;

		// Token: 0x0400039F RID: 927
		private const float MaxFloodInterval = 240f;

		// Token: 0x040003A0 RID: 928
		private float createFloodTimer;

		// Token: 0x040003A1 RID: 929
		private float currentFloodState;

		// Token: 0x040003A2 RID: 930
		private float currentFloodDuration;

		// Token: 0x040003A3 RID: 931
		private float fakeBrokenInterval = 30f;

		// Token: 0x040003A4 RID: 932
		private float fakeBrokenTimer;

		// Token: 0x040003A5 RID: 933
		private float invisibleCharacterInterval = 30f;

		// Token: 0x040003A6 RID: 934
		private float invisibleCharacterTimer;

		// Token: 0x020006DF RID: 1759
		public enum FloodType
		{
			// Token: 0x0400382B RID: 14379
			None,
			// Token: 0x0400382C RID: 14380
			Minor,
			// Token: 0x0400382D RID: 14381
			Major,
			// Token: 0x0400382E RID: 14382
			HideFlooding
		}
	}
}
