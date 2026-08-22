using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200024A RID: 586
	[NullableContext(1)]
	[Nullable(0)]
	internal class Radiation : ISerializableEntity
	{
		// Token: 0x17000C51 RID: 3153
		// (get) Token: 0x060029CB RID: 10699 RVA: 0x00112A8E File Offset: 0x00110C8E
		public string Name
		{
			get
			{
				return "Radiation";
			}
		}

		// Token: 0x17000C52 RID: 3154
		// (get) Token: 0x060029CC RID: 10700 RVA: 0x00112A95 File Offset: 0x00110C95
		// (set) Token: 0x060029CD RID: 10701 RVA: 0x00112A9D File Offset: 0x00110C9D
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float Amount { get; set; }

		// Token: 0x17000C53 RID: 3155
		// (get) Token: 0x060029CE RID: 10702 RVA: 0x00112AA6 File Offset: 0x00110CA6
		// (set) Token: 0x060029CF RID: 10703 RVA: 0x00112AAE File Offset: 0x00110CAE
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool Enabled { get; set; }

		// Token: 0x17000C54 RID: 3156
		// (get) Token: 0x060029D0 RID: 10704 RVA: 0x00112AB7 File Offset: 0x00110CB7
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; }

		// Token: 0x060029D1 RID: 10705 RVA: 0x00112AC0 File Offset: 0x00110CC0
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

		// Token: 0x060029D2 RID: 10706 RVA: 0x00112B14 File Offset: 0x00110D14
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

		// Token: 0x060029D3 RID: 10707 RVA: 0x00112D10 File Offset: 0x00110F10
		public void IncreaseRadiation(float amount)
		{
			this.Amount += amount;
			this.lastIncrease = amount;
			this.increasedAmount = amount;
		}

		// Token: 0x060029D4 RID: 10708 RVA: 0x00112D3C File Offset: 0x00110F3C
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

		// Token: 0x060029D5 RID: 10709 RVA: 0x00112F20 File Offset: 0x00111120
		public float DepthInRadiation(Location location)
		{
			return this.DepthInRadiation(location.MapPosition);
		}

		// Token: 0x060029D6 RID: 10710 RVA: 0x00112F2E File Offset: 0x0011112E
		private float DepthInRadiation(Vector2 pos)
		{
			return this.Amount - pos.X;
		}

		// Token: 0x060029D7 RID: 10711 RVA: 0x00112F40 File Offset: 0x00111140
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

		// Token: 0x060029D8 RID: 10712 RVA: 0x00113038 File Offset: 0x00111238
		public XElement Save()
		{
			XElement element = new XElement("Radiation");
			SerializableProperty.SerializeProperties(this, element, true, false);
			return element;
		}

		// Token: 0x04001496 RID: 5270
		public readonly Map Map;

		// Token: 0x04001497 RID: 5271
		public readonly RadiationParams Params;

		// Token: 0x04001498 RID: 5272
		private float radiationTimer;

		// Token: 0x04001499 RID: 5273
		private float increasedAmount;

		// Token: 0x0400149A RID: 5274
		private float lastIncrease;
	}
}
