using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x020000E6 RID: 230
	[NullableContext(1)]
	[Nullable(0)]
	internal class Radiation : ISerializableEntity
	{
		// Token: 0x1700087F RID: 2175
		// (get) Token: 0x06001FBE RID: 8126 RVA: 0x0013FCBE File Offset: 0x0013DEBE
		private int maxFrames
		{
			get
			{
				SpriteSheet spriteSheet = this.radiationEdgeAnimSheet;
				return ((spriteSheet != null) ? spriteSheet.FrameCount : 0) + 1;
			}
		}

		// Token: 0x06001FBF RID: 8127 RVA: 0x0013FCD4 File Offset: 0x0013DED4
		public void Draw(SpriteBatch spriteBatch, Rectangle container, float zoom)
		{
			if (!this.Enabled)
			{
				return;
			}
			UISprite radiationMainSprite = GUIStyle.Radiation;
			float num;
			float num2;
			(this.Map.DrawOffset * zoom).Deconstruct(out num, out num2);
			float offsetX = num;
			float offsetY = num2;
			container.Center.ToVector2().Deconstruct(out num2, out num);
			float centerX = num2;
			float centerY = num;
			(new Vector2((float)container.Width / 2f, (float)container.Height / 2f) * zoom).Deconstruct(out num, out num2);
			float halfSizeX = num;
			float halfSizeY = num2;
			float viewBottom = centerY + (float)this.Map.Height * zoom;
			Vector2 topLeft = new Vector2(centerX + offsetX - halfSizeX, centerY + offsetY - halfSizeY);
			Vector2 size = new Vector2((this.Amount - this.increasedAmount) * zoom + halfSizeX, viewBottom - topLeft.Y);
			if (size.X < 0f)
			{
				return;
			}
			Vector2 spriteScale = new Vector2(zoom);
			if (radiationMainSprite != null)
			{
				Sprite sprite2 = radiationMainSprite.Sprite;
				Vector2 position = topLeft;
				Vector2 targetSize = size;
				float rotation = 0f;
				Color? color = new Color?(this.Params.RadiationAreaColor);
				Vector2? startOffset = new Vector2?(Vector2.Zero);
				Vector2? textureScale = new Vector2?(spriteScale);
				sprite2.DrawTiled(spriteBatch, position, targetSize, rotation, null, color, startOffset, textureScale, null);
			}
			Vector2 topRight = topLeft + Vector2.UnitX * size.X;
			int index = 0;
			if (this.radiationEdgeAnimSheet != null)
			{
				for (float i = 0f; i <= size.Y; i += (float)this.radiationEdgeAnimSheet.FrameSize.Y / 2f * zoom)
				{
					bool isEven = ++index % 2 == 0;
					Vector2 origin = new Vector2(0.5f, 0f) * (float)this.radiationEdgeAnimSheet.FrameSize.X;
					int sprite = (int)MathF.Floor(isEven ? Radiation.spriteIndex : ((float)this.maxFrames - Radiation.spriteIndex));
					this.radiationEdgeAnimSheet.Draw(spriteBatch, sprite, topRight + new Vector2(0f, i), this.Params.RadiationBorderTint, origin, 0f, spriteScale, SpriteEffects.None, null);
				}
			}
			this.radiationMultiplier = null;
			if (container.Contains(PlayerInput.MousePosition))
			{
				float rightEdge = topLeft.X + size.X;
				float distanceFromRight = rightEdge - PlayerInput.MousePosition.X;
				if (distanceFromRight >= 0f)
				{
					this.radiationMultiplier = new int?(Math.Min(4, (int)(distanceFromRight / (this.Params.RadiationEffectMultipliedPerPixelDistance * zoom)) + 1));
				}
			}
		}

		// Token: 0x06001FC0 RID: 8128 RVA: 0x0013FF90 File Offset: 0x0013E190
		public void DrawFront(SpriteBatch spriteBatch)
		{
			int? num = this.radiationMultiplier;
			if (num != null)
			{
				LocalizedString tooltip = TextManager.GetWithVariable("RadiationTooltip", "[jovianmultiplier]", num.GetValueOrDefault().ToString(), FormatCapitals.No);
				GUIComponent.DrawToolTip(spriteBatch, tooltip, PlayerInput.MousePosition + new Vector2(18f * GUI.Scale), null, null);
			}
		}

		// Token: 0x06001FC1 RID: 8129 RVA: 0x0014000C File Offset: 0x0013E20C
		public void MapUpdate(float deltaTime)
		{
			float spriteStep = this.Params.BorderAnimationSpeed * deltaTime;
			Radiation.spriteIndex = (Radiation.spriteIndex + spriteStep) % (float)this.maxFrames;
			if (this.increasedAmount > 0f)
			{
				this.increasedAmount -= this.lastIncrease / this.Params.AnimationSpeed * deltaTime;
			}
		}

		// Token: 0x17000880 RID: 2176
		// (get) Token: 0x06001FC2 RID: 8130 RVA: 0x00140069 File Offset: 0x0013E269
		public string Name
		{
			get
			{
				return "Radiation";
			}
		}

		// Token: 0x17000881 RID: 2177
		// (get) Token: 0x06001FC3 RID: 8131 RVA: 0x00140070 File Offset: 0x0013E270
		// (set) Token: 0x06001FC4 RID: 8132 RVA: 0x00140078 File Offset: 0x0013E278
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float Amount { get; set; }

		// Token: 0x17000882 RID: 2178
		// (get) Token: 0x06001FC5 RID: 8133 RVA: 0x00140081 File Offset: 0x0013E281
		// (set) Token: 0x06001FC6 RID: 8134 RVA: 0x00140089 File Offset: 0x0013E289
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool Enabled { get; set; }

		// Token: 0x17000883 RID: 2179
		// (get) Token: 0x06001FC7 RID: 8135 RVA: 0x00140092 File Offset: 0x0013E292
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; }

		// Token: 0x06001FC8 RID: 8136 RVA: 0x0014009C File Offset: 0x0013E29C
		public Radiation(Map map, RadiationParams radiationParams, [Nullable(2)] XElement element = null)
		{
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			this.Map = map;
			this.Params = radiationParams;
			this.radiationTimer = this.Params.RadiationDamageDelay;
			if (element == null)
			{
				this.Amount = this.Params.StartingRadiation;
			}
		}

		// Token: 0x06001FC9 RID: 8137 RVA: 0x00140100 File Offset: 0x0013E300
		public void OnStep(float steps = 1f)
		{
			if (!this.Enabled)
			{
				return;
			}
			if (steps <= 0f)
			{
				return;
			}
			float increaseAmount = this.Params.RadiationStep * steps;
			if (this.Params.MaxRadiation > 0f && this.Params.MaxRadiation < this.Amount + increaseAmount)
			{
				increaseAmount = this.Params.MaxRadiation - this.Amount;
			}
			this.IncreaseRadiation(increaseAmount);
			int amountOfOutposts = this.Map.Locations.Count((Location location) => location.Type.HasOutpost && !location.IsCriticallyRadiated());
			using (IEnumerator<Location> enumerator = (from l in this.Map.Locations
			where this.DepthInRadiation(l) > 0f
			select l).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Location location = enumerator.Current;
					if (location.IsGateBetweenBiomes)
					{
						location.Connections.ForEach(delegate(LocationConnection c)
						{
							c.Locked = false;
						});
					}
					else
					{
						if (amountOfOutposts <= this.Params.MinimumOutpostAmount)
						{
							break;
						}
						Location currLocation = this.Map.CurrentLocation;
						if (currLocation == null || (currLocation != location && !currLocation.Connections.Any((LocationConnection lc) => lc.OtherLocation(currLocation) == location)))
						{
							bool wasCritical = location.IsCriticallyRadiated();
							Location location2 = location;
							int turnsInRadiation = location2.TurnsInRadiation;
							location2.TurnsInRadiation = turnsInRadiation + 1;
							if (location.Type.HasOutpost && !wasCritical && location.IsCriticallyRadiated())
							{
								location.ClearMissions();
								amountOfOutposts--;
							}
						}
					}
				}
			}
		}

		// Token: 0x06001FCA RID: 8138 RVA: 0x001402FC File Offset: 0x0013E4FC
		public void IncreaseRadiation(float amount)
		{
			this.Amount += amount;
			this.lastIncrease = amount;
			this.increasedAmount = amount;
		}

		// Token: 0x06001FCB RID: 8139 RVA: 0x00140328 File Offset: 0x0013E528
		public void UpdateRadiation(float deltaTime)
		{
			GameSession gameSession = GameMain.GameSession;
			if (gameSession == null || !gameSession.IsCurrentLocationRadiated())
			{
				return;
			}
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember != null && networkMember.IsClient)
			{
				return;
			}
			if (this.radiationTimer > 0f)
			{
				this.radiationTimer -= deltaTime;
				return;
			}
			this.radiationTimer = this.Params.RadiationDamageDelay;
			foreach (Character character in Character.CharacterList)
			{
				if (!character.IsDead && !character.Removed)
				{
					CharacterHealth health = character.CharacterHealth;
					if (health != null)
					{
						float depthInRadiation = this.DepthInRadiation(character);
						if (depthInRadiation > 0f)
						{
							AfflictionPrefab afflictionPrefab = AfflictionPrefab.JovianRadiation ?? AfflictionPrefab.RadiationSickness;
							float currentAfflictionStrength = character.CharacterHealth.GetAfflictionStrengthByIdentifier(afflictionPrefab.Identifier, true);
							float radiationDamageAmount = this.Params.RadiationDamageAmount;
							AfflictionPrefab.Effect effect = afflictionPrefab.Effects.FirstOrDefault<AfflictionPrefab.Effect>();
							float addedStrength = (radiationDamageAmount - ((effect != null) ? new float?(effect.StrengthChange) : null)).GetValueOrDefault();
							addedStrength *= this.Params.RadiationDamageDelay;
							int multiplier = (int)Math.Ceiling((double)(depthInRadiation / this.Params.RadiationEffectMultipliedPerPixelDistance));
							float growthPotentialInBracket = (float)(multiplier * 25) - currentAfflictionStrength;
							if (growthPotentialInBracket > 0f)
							{
								addedStrength = Math.Min(addedStrength, growthPotentialInBracket);
								CharacterHealth characterHealth = character.CharacterHealth;
								AnimController animController = character.AnimController;
								characterHealth.ApplyAffliction((animController != null) ? animController.MainLimb : null, afflictionPrefab.Instantiate(addedStrength, null), true, false, true);
							}
						}
					}
				}
			}
		}

		// Token: 0x06001FCC RID: 8140 RVA: 0x0014050C File Offset: 0x0013E70C
		public float DepthInRadiation(Location location)
		{
			return this.DepthInRadiation(location.MapPosition);
		}

		// Token: 0x06001FCD RID: 8141 RVA: 0x0014051A File Offset: 0x0013E71A
		private float DepthInRadiation(Vector2 pos)
		{
			return this.Amount - pos.X;
		}

		// Token: 0x06001FCE RID: 8142 RVA: 0x0014052C File Offset: 0x0013E72C
		public float DepthInRadiation(Entity entity)
		{
			if (!this.Enabled)
			{
				return 0f;
			}
			Level level = Level.Loaded;
			if (level != null && level.Type == LevelData.LevelType.LocationConnection)
			{
				Location startLocation = level.StartLocation;
				if (startLocation != null)
				{
					Location endLocation = level.EndLocation;
					if (endLocation != null)
					{
						float distanceNormalized = MathHelper.Clamp((entity.WorldPosition.X - level.StartPosition.X) / (level.EndPosition.X - level.StartPosition.X), 0f, 1f);
						float num;
						float num2;
						startLocation.MapPosition.Deconstruct(out num, out num2);
						float startX = num;
						float startY = num2;
						endLocation.MapPosition.Deconstruct(out num2, out num);
						float endX = num2;
						float endY = num;
						Vector2 mapPos = new Vector2(startX, startY) + new Vector2(endX - startX, endY - startY) * distanceNormalized;
						return this.DepthInRadiation(mapPos);
					}
				}
			}
			return 0f;
		}

		// Token: 0x06001FCF RID: 8143 RVA: 0x00140624 File Offset: 0x0013E824
		public XElement Save()
		{
			XElement element = new XElement("Radiation");
			SerializableProperty.SerializeProperties(this, element, true, false);
			return element;
		}

		// Token: 0x0400104A RID: 4170
		private int? radiationMultiplier;

		// Token: 0x0400104B RID: 4171
		private static float spriteIndex;

		// Token: 0x0400104C RID: 4172
		[Nullable(2)]
		private readonly SpriteSheet radiationEdgeAnimSheet = GUIStyle.RadiationAnimSpriteSheet;

		// Token: 0x0400104D RID: 4173
		private bool isHoveringOver;

		// Token: 0x04001051 RID: 4177
		public readonly Map Map;

		// Token: 0x04001052 RID: 4178
		public readonly RadiationParams Params;

		// Token: 0x04001053 RID: 4179
		private float radiationTimer;

		// Token: 0x04001054 RID: 4180
		private float increasedAmount;

		// Token: 0x04001055 RID: 4181
		private float lastIncrease;
	}
}
