using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200013A RID: 314
	internal class DecorativeSprite : ISerializableEntity
	{
		// Token: 0x17000A6D RID: 2669
		// (get) Token: 0x060028BE RID: 10430 RVA: 0x001C4952 File Offset: 0x001C2B52
		public string Name
		{
			get
			{
				return "Decorative Sprite";
			}
		}

		// Token: 0x17000A6E RID: 2670
		// (get) Token: 0x060028BF RID: 10431 RVA: 0x001C4959 File Offset: 0x001C2B59
		// (set) Token: 0x060028C0 RID: 10432 RVA: 0x001C4961 File Offset: 0x001C2B61
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; set; }

		// Token: 0x17000A6F RID: 2671
		// (get) Token: 0x060028C1 RID: 10433 RVA: 0x001C496A File Offset: 0x001C2B6A
		// (set) Token: 0x060028C2 RID: 10434 RVA: 0x001C4972 File Offset: 0x001C2B72
		public Sprite Sprite { get; private set; }

		// Token: 0x17000A70 RID: 2672
		// (get) Token: 0x060028C3 RID: 10435 RVA: 0x001C497B File Offset: 0x001C2B7B
		// (set) Token: 0x060028C4 RID: 10436 RVA: 0x001C4983 File Offset: 0x001C2B83
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public float BlinkFrequency { get; private set; }

		// Token: 0x17000A71 RID: 2673
		// (get) Token: 0x060028C5 RID: 10437 RVA: 0x001C498C File Offset: 0x001C2B8C
		// (set) Token: 0x060028C6 RID: 10438 RVA: 0x001C4994 File Offset: 0x001C2B94
		[Serialize("0,0", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public Vector2 Offset { get; private set; }

		// Token: 0x17000A72 RID: 2674
		// (get) Token: 0x060028C7 RID: 10439 RVA: 0x001C499D File Offset: 0x001C2B9D
		// (set) Token: 0x060028C8 RID: 10440 RVA: 0x001C49A5 File Offset: 0x001C2BA5
		[Serialize("0,0", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public Vector2 RandomOffset { get; private set; }

		// Token: 0x17000A73 RID: 2675
		// (get) Token: 0x060028C9 RID: 10441 RVA: 0x001C49AE File Offset: 0x001C2BAE
		// (set) Token: 0x060028CA RID: 10442 RVA: 0x001C49B6 File Offset: 0x001C2BB6
		[Serialize(DecorativeSprite.AnimationType.None, IsPropertySaveable.No, "", "", false)]
		[Editable]
		public DecorativeSprite.AnimationType OffsetAnim { get; private set; }

		// Token: 0x17000A74 RID: 2676
		// (get) Token: 0x060028CB RID: 10443 RVA: 0x001C49BF File Offset: 0x001C2BBF
		// (set) Token: 0x060028CC RID: 10444 RVA: 0x001C49C7 File Offset: 0x001C2BC7
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public float OffsetAnimSpeed { get; private set; }

		// Token: 0x17000A75 RID: 2677
		// (get) Token: 0x060028CD RID: 10445 RVA: 0x001C49D0 File Offset: 0x001C2BD0
		// (set) Token: 0x060028CE RID: 10446 RVA: 0x001C49D8 File Offset: 0x001C2BD8
		[Serialize(DecorativeSprite.AnimationType.None, IsPropertySaveable.No, "", "", false)]
		[Editable]
		public DecorativeSprite.AnimationType ScaleAnim { get; private set; }

		// Token: 0x17000A76 RID: 2678
		// (get) Token: 0x060028CF RID: 10447 RVA: 0x001C49E1 File Offset: 0x001C2BE1
		// (set) Token: 0x060028D0 RID: 10448 RVA: 0x001C49E9 File Offset: 0x001C2BE9
		[Serialize("0,0", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public Vector2 ScaleAnimAmount { get; private set; }

		// Token: 0x17000A77 RID: 2679
		// (get) Token: 0x060028D1 RID: 10449 RVA: 0x001C49F2 File Offset: 0x001C2BF2
		// (set) Token: 0x060028D2 RID: 10450 RVA: 0x001C49FA File Offset: 0x001C2BFA
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public float ScaleAnimSpeed { get; private set; }

		// Token: 0x17000A78 RID: 2680
		// (get) Token: 0x060028D3 RID: 10451 RVA: 0x001C4A03 File Offset: 0x001C2C03
		// (set) Token: 0x060028D4 RID: 10452 RVA: 0x001C4A10 File Offset: 0x001C2C10
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public float RotationSpeed
		{
			get
			{
				return MathHelper.ToDegrees(this.rotationSpeedRadians);
			}
			private set
			{
				this.rotationSpeedRadians = MathHelper.ToRadians(value);
				this.absRotationSpeedRadians = Math.Abs(this.rotationSpeedRadians);
			}
		}

		// Token: 0x17000A79 RID: 2681
		// (get) Token: 0x060028D5 RID: 10453 RVA: 0x001C4A2F File Offset: 0x001C2C2F
		// (set) Token: 0x060028D6 RID: 10454 RVA: 0x001C4A3C File Offset: 0x001C2C3C
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public float Rotation
		{
			get
			{
				return MathHelper.ToDegrees(this.rotationRadians);
			}
			private set
			{
				this.rotationRadians = MathHelper.ToRadians(value);
			}
		}

		// Token: 0x17000A7A RID: 2682
		// (get) Token: 0x060028D7 RID: 10455 RVA: 0x001C4A4A File Offset: 0x001C2C4A
		// (set) Token: 0x060028D8 RID: 10456 RVA: 0x001C4A71 File Offset: 0x001C2C71
		[Serialize("0,0", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public Vector2 RandomRotation
		{
			get
			{
				return new Vector2(MathHelper.ToDegrees(this.randomRotationRadians.X), MathHelper.ToDegrees(this.randomRotationRadians.Y));
			}
			private set
			{
				this.randomRotationRadians = new Vector2(MathHelper.ToRadians(value.X), MathHelper.ToRadians(value.Y));
			}
		}

		// Token: 0x17000A7B RID: 2683
		// (get) Token: 0x060028D9 RID: 10457 RVA: 0x001C4A94 File Offset: 0x001C2C94
		// (set) Token: 0x060028DA RID: 10458 RVA: 0x001C4A9C File Offset: 0x001C2C9C
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public float Scale
		{
			get
			{
				return this.scale;
			}
			private set
			{
				this.scale = MathHelper.Clamp(value, 0f, 10f);
			}
		}

		// Token: 0x17000A7C RID: 2684
		// (get) Token: 0x060028DB RID: 10459 RVA: 0x001C4AB4 File Offset: 0x001C2CB4
		// (set) Token: 0x060028DC RID: 10460 RVA: 0x001C4ABC File Offset: 0x001C2CBC
		[Serialize("0,0", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public Vector2 RandomScale { get; private set; }

		// Token: 0x17000A7D RID: 2685
		// (get) Token: 0x060028DD RID: 10461 RVA: 0x001C4AC5 File Offset: 0x001C2CC5
		// (set) Token: 0x060028DE RID: 10462 RVA: 0x001C4ACD File Offset: 0x001C2CCD
		[Serialize(DecorativeSprite.AnimationType.None, IsPropertySaveable.No, "", "", false)]
		[Editable]
		public DecorativeSprite.AnimationType RotationAnim { get; private set; }

		// Token: 0x17000A7E RID: 2686
		// (get) Token: 0x060028DF RID: 10463 RVA: 0x001C4AD6 File Offset: 0x001C2CD6
		// (set) Token: 0x060028E0 RID: 10464 RVA: 0x001C4ADE File Offset: 0x001C2CDE
		[Serialize(0, IsPropertySaveable.No, "If > 0, only one sprite of the same group is used (chosen randomly)", "", false)]
		[Editable(ReadOnly = true)]
		public int RandomGroupID { get; private set; }

		// Token: 0x17000A7F RID: 2687
		// (get) Token: 0x060028E1 RID: 10465 RVA: 0x001C4AE7 File Offset: 0x001C2CE7
		// (set) Token: 0x060028E2 RID: 10466 RVA: 0x001C4AEF File Offset: 0x001C2CEF
		[Serialize("1.0,1.0,1.0,1.0", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public Color Color { get; set; }

		// Token: 0x17000A80 RID: 2688
		// (get) Token: 0x060028E3 RID: 10467 RVA: 0x001C4AF8 File Offset: 0x001C2CF8
		// (set) Token: 0x060028E4 RID: 10468 RVA: 0x001C4B00 File Offset: 0x001C2D00
		internal List<PropertyConditional> IsActiveConditionals { get; private set; } = new List<PropertyConditional>();

		// Token: 0x17000A81 RID: 2689
		// (get) Token: 0x060028E5 RID: 10469 RVA: 0x001C4B09 File Offset: 0x001C2D09
		// (set) Token: 0x060028E6 RID: 10470 RVA: 0x001C4B11 File Offset: 0x001C2D11
		internal List<PropertyConditional> AnimationConditionals { get; private set; } = new List<PropertyConditional>();

		// Token: 0x060028E7 RID: 10471 RVA: 0x001C4B1C File Offset: 0x001C2D1C
		public DecorativeSprite(ContentXElement element, string path = "", string file = "", bool lazyLoad = false)
		{
			this.Sprite = new Sprite(element, path, file, lazyLoad, 1f);
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				List<PropertyConditional> conditionalList;
				if (!(a == "conditional") && !(a == "isactiveconditional"))
				{
					if (!(a == "animationconditional"))
					{
						continue;
					}
					conditionalList = this.AnimationConditionals;
				}
				else
				{
					conditionalList = this.IsActiveConditionals;
				}
				conditionalList.AddRange(PropertyConditional.FromXElement(subElement, null));
			}
		}

		// Token: 0x060028E8 RID: 10472 RVA: 0x001C4BFC File Offset: 0x001C2DFC
		public Vector2 GetOffset(ref float offsetState, Vector2 randomOffsetMultiplier, float rotation = 0f)
		{
			Vector2 offset = this.Offset;
			if (this.OffsetAnimSpeed > 0f)
			{
				switch (this.OffsetAnim)
				{
				case DecorativeSprite.AnimationType.Sine:
					offsetState %= 6.2831855f / this.OffsetAnimSpeed;
					offset *= MathF.Sin(offsetState * this.OffsetAnimSpeed);
					break;
				case DecorativeSprite.AnimationType.Noise:
					offset *= DecorativeSprite.GetNoiseVector(ref offsetState, this.OffsetAnimSpeed);
					break;
				case DecorativeSprite.AnimationType.Circle:
					offsetState %= 6.2831855f / this.OffsetAnimSpeed;
					offset *= new Vector2(MathF.Cos(offsetState * this.OffsetAnimSpeed), MathF.Sin(offsetState * this.OffsetAnimSpeed));
					break;
				}
			}
			offset += new Vector2(this.RandomOffset.X * randomOffsetMultiplier.X, this.RandomOffset.Y * randomOffsetMultiplier.Y);
			if (Math.Abs(rotation) > 0.01f)
			{
				Matrix transform = Matrix.CreateRotationZ(rotation);
				offset = Vector2.Transform(offset, transform);
			}
			return offset;
		}

		// Token: 0x060028E9 RID: 10473 RVA: 0x001C4D00 File Offset: 0x001C2F00
		public float GetRotation(ref float rotationState, float randomRotationFactor)
		{
			DecorativeSprite.AnimationType rotationAnim = this.RotationAnim;
			if (rotationAnim == DecorativeSprite.AnimationType.Sine)
			{
				rotationState %= 6.2831855f / this.absRotationSpeedRadians;
				return this.rotationRadians * MathF.Sin(rotationState * this.rotationSpeedRadians) + MathHelper.Lerp(this.randomRotationRadians.X, this.randomRotationRadians.Y, randomRotationFactor);
			}
			if (rotationAnim != DecorativeSprite.AnimationType.Noise)
			{
				return this.rotationRadians + rotationState * this.rotationSpeedRadians + MathHelper.Lerp(this.randomRotationRadians.X, this.randomRotationRadians.Y, randomRotationFactor);
			}
			rotationState %= 1f / this.absRotationSpeedRadians;
			return this.rotationRadians * (PerlinNoise.GetPerlin(rotationState * this.absRotationSpeedRadians, rotationState * this.absRotationSpeedRadians) - 0.5f) + MathHelper.Lerp(this.randomRotationRadians.X, this.randomRotationRadians.Y, randomRotationFactor);
		}

		// Token: 0x060028EA RID: 10474 RVA: 0x001C4DE8 File Offset: 0x001C2FE8
		public Vector2 GetScale(ref float scaleState, float randomScaleModifier)
		{
			Vector2 currentScale = Vector2.One * ((this.RandomScale == Vector2.Zero) ? this.scale : MathHelper.Lerp(this.RandomScale.X, this.RandomScale.Y, randomScaleModifier));
			if (this.ScaleAnimSpeed > 0f)
			{
				DecorativeSprite.AnimationType scaleAnim = this.ScaleAnim;
				if (scaleAnim != DecorativeSprite.AnimationType.Sine)
				{
					if (scaleAnim == DecorativeSprite.AnimationType.Noise)
					{
						currentScale *= Vector2.One + this.ScaleAnimAmount * DecorativeSprite.GetNoiseVector(ref scaleState, this.ScaleAnimSpeed);
					}
				}
				else
				{
					scaleState %= 6.2831855f / this.ScaleAnimSpeed;
					currentScale *= Vector2.One + this.ScaleAnimAmount * MathF.Sin(scaleState * this.ScaleAnimSpeed);
				}
			}
			return currentScale;
		}

		// Token: 0x060028EB RID: 10475 RVA: 0x001C4EBC File Offset: 0x001C30BC
		private static Vector2 GetNoiseVector(ref float state, float speed)
		{
			float modifiedSpeed = speed * 0.1f;
			state %= 1f / modifiedSpeed;
			float t = state * modifiedSpeed;
			Vector2 noiseValue = new Vector2(PerlinNoise.GetPerlin(t, t), PerlinNoise.GetPerlin(t + 0.5f, t + 0.5f));
			return noiseValue - new Vector2(0.5f, 0.5f);
		}

		// Token: 0x060028EC RID: 10476 RVA: 0x001C4F18 File Offset: 0x001C3118
		public static void UpdateSpriteStates(ImmutableDictionary<int, ImmutableArray<DecorativeSprite>> spriteGroups, Dictionary<DecorativeSprite, DecorativeSprite.State> animStates, int entityID, float deltaTime, Func<PropertyConditional, bool> checkConditional)
		{
			foreach (int spriteGroup in spriteGroups.Keys)
			{
				for (int i = 0; i < spriteGroups[spriteGroup].Length; i++)
				{
					DecorativeSprite decorativeSprite = spriteGroups[spriteGroup][i];
					if (decorativeSprite != null)
					{
						if (spriteGroup > 0)
						{
							int activeSpriteIndex = entityID % spriteGroups[spriteGroup].Length;
							if (i != activeSpriteIndex)
							{
								animStates[decorativeSprite].IsActive = false;
								goto IL_189;
							}
						}
						DecorativeSprite.State spriteState = animStates[decorativeSprite];
						spriteState.IsActive = true;
						foreach (PropertyConditional conditional in decorativeSprite.IsActiveConditionals)
						{
							if (!checkConditional(conditional))
							{
								spriteState.IsActive = false;
								break;
							}
						}
						if (spriteState.IsActive)
						{
							if (decorativeSprite.BlinkFrequency > 0f)
							{
								decorativeSprite.blinkTimer += deltaTime * decorativeSprite.BlinkFrequency;
								decorativeSprite.blinkTimer %= 1f;
								if (decorativeSprite.blinkTimer > 0.5f)
								{
									spriteState.IsActive = false;
									goto IL_189;
								}
							}
							bool animate = true;
							foreach (PropertyConditional conditional2 in decorativeSprite.AnimationConditionals)
							{
								if (!checkConditional(conditional2))
								{
									animate = false;
									break;
								}
							}
							if (animate)
							{
								spriteState.ScaleState += deltaTime;
								spriteState.OffsetState += deltaTime;
								spriteState.RotationState += deltaTime;
							}
						}
					}
					IL_189:;
				}
			}
		}

		// Token: 0x060028ED RID: 10477 RVA: 0x001C512C File Offset: 0x001C332C
		public void Remove()
		{
			Sprite sprite = this.Sprite;
			if (sprite != null)
			{
				sprite.Remove();
			}
			this.Sprite = null;
		}

		// Token: 0x040014E0 RID: 5344
		private float blinkTimer;

		// Token: 0x040014E8 RID: 5352
		private float rotationSpeedRadians;

		// Token: 0x040014E9 RID: 5353
		private float absRotationSpeedRadians;

		// Token: 0x040014EA RID: 5354
		private float rotationRadians;

		// Token: 0x040014EB RID: 5355
		private Vector2 randomRotationRadians;

		// Token: 0x040014EC RID: 5356
		private float scale;

		// Token: 0x02000D7F RID: 3455
		public class State
		{
			// Token: 0x04004F7E RID: 20350
			public float RotationState;

			// Token: 0x04004F7F RID: 20351
			public float OffsetState;

			// Token: 0x04004F80 RID: 20352
			public float ScaleState;

			// Token: 0x04004F81 RID: 20353
			public Vector2 RandomOffsetMultiplier = new Vector2(Rand.Range(-1f, 1f, Rand.RandSync.Unsynced), Rand.Range(-1f, 1f, Rand.RandSync.Unsynced));

			// Token: 0x04004F82 RID: 20354
			public float RandomRotationFactor = Rand.Range(0f, 1f, Rand.RandSync.Unsynced);

			// Token: 0x04004F83 RID: 20355
			public float RandomScaleFactor = Rand.Range(0f, 1f, Rand.RandSync.Unsynced);

			// Token: 0x04004F84 RID: 20356
			public bool IsActive = true;
		}

		// Token: 0x02000D80 RID: 3456
		public enum AnimationType
		{
			// Token: 0x04004F86 RID: 20358
			None,
			// Token: 0x04004F87 RID: 20359
			Sine,
			// Token: 0x04004F88 RID: 20360
			Noise,
			// Token: 0x04004F89 RID: 20361
			Circle
		}
	}
}
