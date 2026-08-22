using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.MapCreatures.Behavior;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200003F RID: 63
	internal class Hull : MapEntity, ISerializableEntity, IServerSerializable, INetSerializable, IClientSerializable
	{
		// Token: 0x0600095B RID: 2395 RVA: 0x0005CBDF File Offset: 0x0005ADDF
		public override bool IsMouseOn(Vector2 position)
		{
			return false;
		}

		// Token: 0x0600095C RID: 2396 RVA: 0x0005CBE2 File Offset: 0x0005ADE2
		public void ForceStatusUpdate()
		{
			this.statusUpdateTimer = NetConfig.SparseHullUpdateInterval;
		}

		// Token: 0x0600095D RID: 2397 RVA: 0x0005CBF0 File Offset: 0x0005ADF0
		public void CreateStatusEvent()
		{
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember == null)
			{
				return;
			}
			networkMember.CreateEntityEvent(this, default(Hull.StatusEventData));
		}

		// Token: 0x0600095E RID: 2398 RVA: 0x0005CC1C File Offset: 0x0005AE1C
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			Hull.IEventData eventData = extraData as Hull.IEventData;
			if (eventData == null)
			{
				throw new Exception("Malformed hull event: expected Hull.IEventData");
			}
			msg.WriteRangedInteger((int)eventData.EventType, 0, 3);
			if (eventData is Hull.StatusEventData)
			{
				Hull.StatusEventData statusEventData = (Hull.StatusEventData)eventData;
				msg.WriteRangedSingle(MathHelper.Clamp(this.OxygenPercentage, 0f, 100f), 0f, 100f, 8);
				this.SharedStatusWrite(msg);
				return;
			}
			if (eventData is Hull.BackgroundSectionsEventData)
			{
				Hull.BackgroundSectionsEventData backgroundSectionsEventData = (Hull.BackgroundSectionsEventData)eventData;
				this.SharedBackgroundSectionsWrite(msg, backgroundSectionsEventData);
				return;
			}
			Hull.BallastFloraEventData ballastFloraEventData;
			if (eventData is Hull.DecalEventData)
			{
				Hull.DecalEventData decalEventData = (Hull.DecalEventData)eventData;
				msg.WriteRangedInteger(this.decals.Count, 0, 10);
				using (List<Decal>.Enumerator enumerator = this.decals.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Decal decal = enumerator.Current;
						msg.WriteUInt32(decal.Prefab.UintIdentifier);
						msg.WriteByte((byte)decal.SpriteIndex);
						float normalizedXPos = MathHelper.Clamp(MathUtils.InverseLerp(0f, (float)this.rect.Width, decal.CenterPosition.X), 0f, 1f);
						float normalizedYPos = MathHelper.Clamp(MathUtils.InverseLerp((float)(-(float)this.rect.Height), 0f, decal.CenterPosition.Y), 0f, 1f);
						msg.WriteRangedSingle(normalizedXPos, 0f, 1f, 8);
						msg.WriteRangedSingle(normalizedYPos, 0f, 1f, 8);
						msg.WriteRangedSingle(decal.Scale, 0f, 2f, 12);
						msg.WriteRangedSingle(decal.BaseAlpha, 0f, 1f, 8);
					}
					return;
				}
			}
			else
			{
				if (!(eventData is Hull.BallastFloraEventData))
				{
					throw new Exception("Malformed hull event: did not expect " + eventData.GetType().Name);
				}
				ballastFloraEventData = (Hull.BallastFloraEventData)eventData;
			}
			ballastFloraEventData.Behavior.ServerWrite(msg, ballastFloraEventData.SubEventData);
		}

		// Token: 0x0600095F RID: 2399 RVA: 0x0005CE38 File Offset: 0x0005B038
		public void ServerEventRead(IReadMessage msg, Client c)
		{
			Hull.EventType eventType = (Hull.EventType)msg.ReadRangedInteger(0, 3);
			switch (eventType)
			{
			case Hull.EventType.Status:
			{
				float newWaterVolume;
				Hull.NetworkFireSource[] newFireSources;
				this.SharedStatusRead(msg, out newWaterVolume, out newFireSources);
				if (c.HasPermission(ClientPermissions.ConsoleCommands))
				{
					if (c.PermittedConsoleCommands.Any((DebugConsole.Command command) => command.Names.Contains("fire".ToIdentifier()) || command.Names.Contains("editfire".ToIdentifier())))
					{
						this.WaterVolume = newWaterVolume;
						if (newFireSources.Length != this.FireSources.Count)
						{
							this.ForceStatusUpdate();
						}
						for (int i = 0; i < newFireSources.Length; i++)
						{
							Vector2 pos = newFireSources[i].Position;
							float size = newFireSources[i].Size;
							FireSource newFire = (i < this.FireSources.Count) ? this.FireSources[i] : new FireSource((base.Submarine == null) ? pos : (pos + base.Submarine.Position), null, null, true);
							newFire.Position = pos;
							newFire.Size = new Vector2(size, newFire.Size.Y);
							if (!this.FireSources.Contains(newFire))
							{
								newFire.Remove();
							}
						}
						for (int j = this.FireSources.Count - 1; j >= newFireSources.Length; j--)
						{
							this.FireSources[j].Remove();
							if (j < this.FireSources.Count)
							{
								this.FireSources.RemoveAt(j);
							}
						}
						return;
					}
				}
				return;
			}
			case Hull.EventType.Decal:
			{
				byte decalIndex = msg.ReadByte();
				float decalAlpha = msg.ReadRangedSingle(0f, 1f, 8);
				if (decalIndex < 0 || (int)decalIndex >= this.decals.Count)
				{
					return;
				}
				if (c.Character != null && c.Character.AllowInput)
				{
					if (c.Character.HeldItems.Any((Item it) => it.GetComponent<Sprayer>() != null))
					{
						this.decals[(int)decalIndex].BaseAlpha = decalAlpha;
					}
				}
				this.decalUpdatePending = true;
				return;
			}
			case Hull.EventType.BackgroundSections:
			{
				bool addPendingSectorUpdate = false;
				int sectorToUpdate;
				this.SharedBackgroundSectionRead(msg, delegate(Hull.BackgroundSectionNetworkUpdate bsnu)
				{
					int k = bsnu.SectionIndex;
					Color color = bsnu.Color;
					float colorStrength = bsnu.ColorStrength;
					Character character = c.Character;
					if (character == null || !character.AllowInput)
					{
						return;
					}
					Sprayer sprayer = (from it in c.Character.HeldItems
					select it.GetComponent<Sprayer>()).FirstOrDefault((Sprayer component) => component != null);
					if (sprayer == null)
					{
						return;
					}
					ItemContainer liquidContainer = sprayer.LiquidContainer;
					Item item;
					if (liquidContainer == null)
					{
						item = null;
					}
					else
					{
						ItemInventory inventory = liquidContainer.Inventory;
						item = ((inventory != null) ? inventory.FirstOrDefault() : null);
					}
					Item liquidItem = item;
					if (liquidItem == null)
					{
						return;
					}
					Color paintColor;
					if (!sprayer.LiquidColors.TryGetValue(liquidItem.Prefab.Identifier, out paintColor))
					{
						return;
					}
					bool isCleaning = paintColor.A == 0;
					Vector2 backgroundSectionPos = this.GetBackgroundSectionWorldPos(this.BackgroundSections[k]);
					if (Vector2.Distance(backgroundSectionPos, sprayer.Item.WorldPosition) > sprayer.Range * 1.1f)
					{
						return;
					}
					addPendingSectorUpdate = true;
					if (isCleaning)
					{
						if (colorStrength >= this.BackgroundSections[k].ColorStrength)
						{
							return;
						}
					}
					else
					{
						Vector3 colorChange = color.ToVector3() - this.BackgroundSections[k].Color.ToVector3();
						Vector3 expectedColorChange = paintColor.ToVector3() - this.BackgroundSections[k].Color.ToVector3();
						if (Math.Sign(colorChange.X) != Math.Sign(expectedColorChange.X) || Math.Sign(colorChange.Y) != Math.Sign(expectedColorChange.Y) || Math.Sign(colorChange.Z) != Math.Sign(expectedColorChange.Z))
						{
							return;
						}
						this.BackgroundSections[k].SetColor(color);
					}
					this.BackgroundSections[k].SetColorStrength(colorStrength);
				}, out sectorToUpdate);
				if (addPendingSectorUpdate)
				{
					this.RefreshAveragePaintedColor();
					this.pendingSectorUpdates.Add(sectorToUpdate);
					return;
				}
				return;
			}
			default:
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(61, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Malformed incoming hull event: ");
				defaultInterpolatedStringHandler.AppendFormatted<Hull.EventType>(eventType);
				defaultInterpolatedStringHandler.AppendLiteral(" is not a supported event type");
				throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			}
		}

		// Token: 0x17000272 RID: 626
		// (get) Token: 0x06000960 RID: 2400 RVA: 0x0005D0FB File Offset: 0x0005B2FB
		public Dictionary<Identifier, SerializableProperty> SerializableProperties
		{
			get
			{
				return this.properties;
			}
		}

		// Token: 0x17000273 RID: 627
		// (get) Token: 0x06000961 RID: 2401 RVA: 0x0005D103 File Offset: 0x0005B303
		public override string Name
		{
			get
			{
				return "Hull";
			}
		}

		// Token: 0x17000274 RID: 628
		// (get) Token: 0x06000962 RID: 2402 RVA: 0x0005D10A File Offset: 0x0005B30A
		// (set) Token: 0x06000963 RID: 2403 RVA: 0x0005D112 File Offset: 0x0005B312
		public LocalizedString DisplayName { get; private set; }

		// Token: 0x17000275 RID: 629
		// (get) Token: 0x06000964 RID: 2404 RVA: 0x0005D11B File Offset: 0x0005B31B
		public IEnumerable<Identifier> OutpostModuleTags
		{
			get
			{
				return this.moduleTags;
			}
		}

		// Token: 0x17000276 RID: 630
		// (get) Token: 0x06000965 RID: 2405 RVA: 0x0005D123 File Offset: 0x0005B323
		// (set) Token: 0x06000966 RID: 2406 RVA: 0x0005D12C File Offset: 0x0005B32C
		[Editable]
		[Serialize("", IsPropertySaveable.Yes, "", "RoomName.", false)]
		public string RoomName
		{
			get
			{
				return this.roomName;
			}
			set
			{
				if (this.roomName == value)
				{
					return;
				}
				this.roomName = value;
				this.DisplayName = TextManager.Get(this.roomName).Fallback(this.roomName, true);
				if (!this.IsWetRoom && this.ForceAsWetRoom)
				{
					this.IsWetRoom = true;
				}
			}
		}

		// Token: 0x17000277 RID: 631
		// (get) Token: 0x06000967 RID: 2407 RVA: 0x0005D188 File Offset: 0x0005B388
		// (set) Token: 0x06000968 RID: 2408 RVA: 0x0005D190 File Offset: 0x0005B390
		[Editable]
		[Serialize("0,0,0,0", IsPropertySaveable.Yes, "", "", false)]
		public Color AmbientLight
		{
			get
			{
				return this.ambientLight;
			}
			set
			{
				this.ambientLight = value;
			}
		}

		// Token: 0x17000278 RID: 632
		// (get) Token: 0x06000969 RID: 2409 RVA: 0x0005D199 File Offset: 0x0005B399
		// (set) Token: 0x0600096A RID: 2410 RVA: 0x0005D1A4 File Offset: 0x0005B3A4
		public override Rectangle Rect
		{
			get
			{
				return base.Rect;
			}
			set
			{
				float prevOxygenPercentage = this.OxygenPercentage;
				if (value.Width != this.rect.Width)
				{
					int arraySize = (int)Math.Ceiling((double)((float)value.Width / 32f + 1f));
					this.waveY = new float[arraySize];
					this.waveVel = new float[arraySize];
					this.leftDelta = new float[arraySize];
					this.rightDelta = new float[arraySize];
				}
				base.Rect = value;
				if (base.Submarine == null || !base.Submarine.Loading)
				{
					Item.UpdateHulls();
					Gap.UpdateHulls();
				}
				this.OxygenPercentage = prevOxygenPercentage;
				this.surface = (float)(this.rect.Y - this.rect.Height) + this.WaterVolume / (float)this.rect.Width;
				this.Pressure = this.surface;
				this.CreateBackgroundSections();
			}
		}

		// Token: 0x17000279 RID: 633
		// (get) Token: 0x0600096B RID: 2411 RVA: 0x0005D287 File Offset: 0x0005B487
		public override bool Linkable
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700027A RID: 634
		// (get) Token: 0x0600096C RID: 2412 RVA: 0x0005D28A File Offset: 0x0005B48A
		// (set) Token: 0x0600096D RID: 2413 RVA: 0x0005D292 File Offset: 0x0005B492
		public float LethalPressure
		{
			get
			{
				return this.lethalPressure;
			}
			set
			{
				this.lethalPressure = MathHelper.Clamp(value, 0f, 100f);
			}
		}

		// Token: 0x1700027B RID: 635
		// (get) Token: 0x0600096E RID: 2414 RVA: 0x0005D2AA File Offset: 0x0005B4AA
		public Vector2 Size
		{
			get
			{
				return new Vector2((float)this.rect.Width, (float)this.rect.Height);
			}
		}

		// Token: 0x1700027C RID: 636
		// (get) Token: 0x0600096F RID: 2415 RVA: 0x0005D2C9 File Offset: 0x0005B4C9
		// (set) Token: 0x06000970 RID: 2416 RVA: 0x0005D2D1 File Offset: 0x0005B4D1
		public float CeilingHeight { get; private set; }

		// Token: 0x1700027D RID: 637
		// (get) Token: 0x06000971 RID: 2417 RVA: 0x0005D2DA File Offset: 0x0005B4DA
		public float Surface
		{
			get
			{
				return this.surface;
			}
		}

		// Token: 0x1700027E RID: 638
		// (get) Token: 0x06000972 RID: 2418 RVA: 0x0005D2E2 File Offset: 0x0005B4E2
		public float WorldSurface
		{
			get
			{
				if (base.Submarine != null)
				{
					return this.surface + base.Submarine.Position.Y;
				}
				return this.surface;
			}
		}

		// Token: 0x1700027F RID: 639
		// (get) Token: 0x06000973 RID: 2419 RVA: 0x0005D30A File Offset: 0x0005B50A
		// (set) Token: 0x06000974 RID: 2420 RVA: 0x0005D314 File Offset: 0x0005B514
		public float WaterVolume
		{
			get
			{
				return this.waterVolume;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					return;
				}
				this.waterVolume = MathHelper.Clamp(value, 0f, this.Volume * 1.05f);
				if (this.waterVolume <= this.Volume)
				{
					this.Pressure = (float)(this.rect.Y - this.rect.Height) + this.waterVolume / (float)this.rect.Width;
				}
				if (this.waterVolume > 0f)
				{
					this.update = true;
				}
			}
		}

		// Token: 0x17000280 RID: 640
		// (get) Token: 0x06000975 RID: 2421 RVA: 0x0005D39B File Offset: 0x0005B59B
		// (set) Token: 0x06000976 RID: 2422 RVA: 0x0005D3A3 File Offset: 0x0005B5A3
		[Serialize(100000f, IsPropertySaveable.Yes, "", "", false)]
		public float Oxygen
		{
			get
			{
				return this.oxygen;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					return;
				}
				this.oxygen = MathHelper.Clamp(value, 0f, this.Volume);
			}
		}

		// Token: 0x17000281 RID: 641
		// (get) Token: 0x06000977 RID: 2423 RVA: 0x0005D3C5 File Offset: 0x0005B5C5
		// (set) Token: 0x06000978 RID: 2424 RVA: 0x0005D3CD File Offset: 0x0005B5CD
		public bool IsAirlock { get; private set; }

		// Token: 0x17000282 RID: 642
		// (get) Token: 0x06000979 RID: 2425 RVA: 0x0005D3D8 File Offset: 0x0005B5D8
		private bool ForceAsWetRoom
		{
			get
			{
				return this.roomName != null && (this.roomName.Contains("ballast", StringComparison.OrdinalIgnoreCase) || this.roomName.Contains("bilge", StringComparison.OrdinalIgnoreCase) || this.roomName.Contains("airlock", StringComparison.OrdinalIgnoreCase) || this.roomName.Contains("dockingport", StringComparison.OrdinalIgnoreCase));
			}
		}

		// Token: 0x17000283 RID: 643
		// (get) Token: 0x0600097A RID: 2426 RVA: 0x0005D43B File Offset: 0x0005B63B
		// (set) Token: 0x0600097B RID: 2427 RVA: 0x0005D443 File Offset: 0x0005B643
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "It's normal for this hull to be filled with water. If the room name contains 'ballast', 'bilge', or 'airlock', you can't disable this setting.", "", false)]
		public bool IsWetRoom
		{
			get
			{
				return this.isWetRoom;
			}
			set
			{
				this.isWetRoom = value;
				if (this.ForceAsWetRoom)
				{
					this.isWetRoom = true;
				}
			}
		}

		// Token: 0x17000284 RID: 644
		// (get) Token: 0x0600097C RID: 2428 RVA: 0x0005D45B File Offset: 0x0005B65B
		// (set) Token: 0x0600097D RID: 2429 RVA: 0x0005D46D File Offset: 0x0005B66D
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "Bots avoid staying here, but they are still allowed to access the room when needed and go through it. Forced true for wet rooms.", "", false)]
		public bool AvoidStaying
		{
			get
			{
				return this.avoidStaying || this.IsWetRoom;
			}
			set
			{
				this.avoidStaying = value;
				if (this.IsWetRoom)
				{
					this.avoidStaying = true;
				}
			}
		}

		// Token: 0x17000285 RID: 645
		// (get) Token: 0x0600097E RID: 2430 RVA: 0x0005D485 File Offset: 0x0005B685
		public float WaterPercentage
		{
			get
			{
				return MathUtils.Percentage(this.WaterVolume, this.Volume);
			}
		}

		// Token: 0x17000286 RID: 646
		// (get) Token: 0x0600097F RID: 2431 RVA: 0x0005D498 File Offset: 0x0005B698
		// (set) Token: 0x06000980 RID: 2432 RVA: 0x0005D4C0 File Offset: 0x0005B6C0
		public float OxygenPercentage
		{
			get
			{
				if (this.Volume > 0f)
				{
					return this.oxygen / this.Volume * 100f;
				}
				return 100f;
			}
			set
			{
				this.Oxygen = value / 100f * this.Volume;
			}
		}

		// Token: 0x17000287 RID: 647
		// (get) Token: 0x06000981 RID: 2433 RVA: 0x0005D4D6 File Offset: 0x0005B6D6
		public float Volume
		{
			get
			{
				return (float)(this.rect.Width * this.rect.Height);
			}
		}

		// Token: 0x17000288 RID: 648
		// (get) Token: 0x06000982 RID: 2434 RVA: 0x0005D4F0 File Offset: 0x0005B6F0
		// (set) Token: 0x06000983 RID: 2435 RVA: 0x0005D4F8 File Offset: 0x0005B6F8
		public float Pressure
		{
			get
			{
				return this.pressure;
			}
			set
			{
				this.pressure = value;
			}
		}

		// Token: 0x17000289 RID: 649
		// (get) Token: 0x06000984 RID: 2436 RVA: 0x0005D501 File Offset: 0x0005B701
		public float[] WaveY
		{
			get
			{
				return this.waveY;
			}
		}

		// Token: 0x1700028A RID: 650
		// (get) Token: 0x06000985 RID: 2437 RVA: 0x0005D509 File Offset: 0x0005B709
		public float[] WaveVel
		{
			get
			{
				return this.waveVel;
			}
		}

		// Token: 0x1700028B RID: 651
		// (get) Token: 0x06000986 RID: 2438 RVA: 0x0005D511 File Offset: 0x0005B711
		// (set) Token: 0x06000987 RID: 2439 RVA: 0x0005D519 File Offset: 0x0005B719
		public List<BackgroundSection> BackgroundSections { get; private set; }

		// Token: 0x1700028C RID: 652
		// (get) Token: 0x06000988 RID: 2440 RVA: 0x0005D522 File Offset: 0x0005B722
		public bool SupportsPaintedColors
		{
			get
			{
				return this.BackgroundSections != null;
			}
		}

		// Token: 0x1700028D RID: 653
		// (get) Token: 0x06000989 RID: 2441 RVA: 0x0005D52D File Offset: 0x0005B72D
		// (set) Token: 0x0600098A RID: 2442 RVA: 0x0005D535 File Offset: 0x0005B735
		public Color AveragePaintedColor { get; private set; }

		// Token: 0x1700028E RID: 654
		// (get) Token: 0x0600098B RID: 2443 RVA: 0x0005D53E File Offset: 0x0005B73E
		public bool IsRed
		{
			get
			{
				return ColorExtensions.IsRedDominant(this.AveragePaintedColor, 2f, 100);
			}
		}

		// Token: 0x1700028F RID: 655
		// (get) Token: 0x0600098C RID: 2444 RVA: 0x0005D552 File Offset: 0x0005B752
		public bool IsGreen
		{
			get
			{
				return ColorExtensions.IsGreenDominant(this.AveragePaintedColor, 2f, 100);
			}
		}

		// Token: 0x17000290 RID: 656
		// (get) Token: 0x0600098D RID: 2445 RVA: 0x0005D566 File Offset: 0x0005B766
		public bool IsBlue
		{
			get
			{
				return ColorExtensions.IsBlueDominant(this.AveragePaintedColor, 2f, 100);
			}
		}

		// Token: 0x17000291 RID: 657
		// (get) Token: 0x0600098E RID: 2446 RVA: 0x0005D57A File Offset: 0x0005B77A
		// (set) Token: 0x0600098F RID: 2447 RVA: 0x0005D582 File Offset: 0x0005B782
		public List<FireSource> FireSources { get; private set; }

		// Token: 0x17000292 RID: 658
		// (get) Token: 0x06000990 RID: 2448 RVA: 0x0005D58B File Offset: 0x0005B78B
		// (set) Token: 0x06000991 RID: 2449 RVA: 0x0005D593 File Offset: 0x0005B793
		public List<DummyFireSource> FakeFireSources { get; private set; }

		// Token: 0x17000293 RID: 659
		// (get) Token: 0x06000992 RID: 2450 RVA: 0x0005D59C File Offset: 0x0005B79C
		public int FireCount
		{
			get
			{
				List<FireSource> fireSources = this.FireSources;
				if (fireSources == null)
				{
					return 0;
				}
				return fireSources.Count;
			}
		}

		// Token: 0x17000294 RID: 660
		// (get) Token: 0x06000993 RID: 2451 RVA: 0x0005D5AF File Offset: 0x0005B7AF
		// (set) Token: 0x06000994 RID: 2452 RVA: 0x0005D5B7 File Offset: 0x0005B7B7
		public BallastFloraBehavior BallastFlora { get; set; }

		// Token: 0x06000995 RID: 2453 RVA: 0x0005D5C0 File Offset: 0x0005B7C0
		public Hull(Rectangle rectangle) : this(rectangle, Submarine.MainSub, 0)
		{
		}

		// Token: 0x06000996 RID: 2454 RVA: 0x0005D5D0 File Offset: 0x0005B7D0
		public Hull(Rectangle rectangle, Submarine submarine, ushort id = 0) : base(CoreEntityPrefab.HullPrefab, submarine, id)
		{
			this.rect = rectangle;
			if (this.BackgroundSections == null)
			{
				this.CreateBackgroundSections();
			}
			this.OxygenPercentage = 100f;
			this.FireSources = new List<FireSource>();
			this.FakeFireSources = new List<DummyFireSource>();
			this.properties = SerializableProperty.GetProperties(this);
			int arraySize = (int)Math.Ceiling((double)((float)rectangle.Width / 32f + 1f));
			this.waveY = new float[arraySize];
			this.waveVel = new float[arraySize];
			this.leftDelta = new float[arraySize];
			this.rightDelta = new float[arraySize];
			this.surface = (float)(this.rect.Y - this.rect.Height);
			if (((submarine != null) ? submarine.Info : null) != null && !submarine.Info.IsWreck)
			{
				this.aiTarget = new AITarget(this)
				{
					MinSightRange = 1000f,
					MaxSightRange = 5000f,
					SoundRange = 0f
				};
			}
			Hull.HullList.Add(this);
			if (submarine == null || !submarine.Loading)
			{
				Item.UpdateHulls();
				Gap.UpdateHulls();
			}
			this.CreateBackgroundSections();
			this.WaterVolume = 0f;
			base.InsertToList();
			DebugConsole.Log("Created hull (" + this.ID.ToString() + ")");
		}

		// Token: 0x06000997 RID: 2455 RVA: 0x0005D774 File Offset: 0x0005B974
		public static Rectangle GetBorders()
		{
			if (!Hull.HullList.Any<Hull>())
			{
				return Rectangle.Empty;
			}
			Rectangle rect = Hull.HullList[0].rect;
			foreach (Hull hull in Hull.HullList)
			{
				if (hull.Rect.X < rect.X)
				{
					rect.Width += rect.X - hull.rect.X;
					rect.X = hull.rect.X;
				}
				if (hull.rect.Right > rect.Right)
				{
					rect.Width = hull.rect.Right - rect.X;
				}
				if (hull.rect.Y > rect.Y)
				{
					rect.Height += hull.rect.Y - rect.Y;
					rect.Y = hull.rect.Y;
				}
				if (hull.rect.Y - hull.rect.Height < rect.Y - rect.Height)
				{
					rect.Height = rect.Y - (hull.rect.Y - hull.rect.Height);
				}
			}
			return rect;
		}

		// Token: 0x06000998 RID: 2456 RVA: 0x0005D8F4 File Offset: 0x0005BAF4
		public override MapEntity Clone()
		{
			Hull clone = new Hull(this.rect, base.Submarine, 0);
			foreach (KeyValuePair<Identifier, SerializableProperty> property in this.SerializableProperties)
			{
				if (property.Value.Attributes.OfType<Serialize>().Any<Serialize>())
				{
					clone.SerializableProperties[property.Key].TrySetValue(clone, property.Value.GetValue(this));
				}
			}
			return clone;
		}

		// Token: 0x06000999 RID: 2457 RVA: 0x0005D994 File Offset: 0x0005BB94
		public static EntityGrid GenerateEntityGrid(Rectangle worldRect)
		{
			EntityGrid newGrid = new EntityGrid(worldRect, 200f);
			Hull.EntityGrids.Add(newGrid);
			return newGrid;
		}

		// Token: 0x0600099A RID: 2458 RVA: 0x0005D9BC File Offset: 0x0005BBBC
		public static EntityGrid GenerateEntityGrid(Submarine submarine)
		{
			EntityGrid newGrid = new EntityGrid(submarine, 200f);
			Hull.EntityGrids.Add(newGrid);
			foreach (Hull hull in Hull.HullList)
			{
				if (hull.Submarine == submarine && !hull.IdFreed)
				{
					newGrid.InsertEntity(hull);
				}
			}
			return newGrid;
		}

		// Token: 0x0600099B RID: 2459 RVA: 0x0005DA38 File Offset: 0x0005BC38
		public void SetModuleTags(IEnumerable<Identifier> tags)
		{
			this.moduleTags.Clear();
			foreach (Identifier tag in tags)
			{
				this.moduleTags.Add(tag);
			}
		}

		// Token: 0x0600099C RID: 2460 RVA: 0x0005DA94 File Offset: 0x0005BC94
		public override void OnMapLoaded()
		{
			this.CeilingHeight = (float)this.Rect.Height;
			Body lowerPickedBody = Submarine.PickBody(this.SimPosition, this.SimPosition - new Vector2(0f, ConvertUnits.ToSimUnits((float)this.rect.Height / 2f + 0.1f)), null, new Category?(Category.Cat1), true, null, false);
			if (lowerPickedBody != null)
			{
				Vector2 lowerPickedPos = Submarine.LastPickedPosition;
				if (Submarine.PickBody(this.SimPosition, this.SimPosition + new Vector2(0f, ConvertUnits.ToSimUnits((float)this.rect.Height / 2f + 0.1f)), null, new Category?(Category.Cat1), true, null, false) != null)
				{
					Vector2 upperPickedPos = Submarine.LastPickedPosition;
					this.CeilingHeight = ConvertUnits.ToDisplayUnits(upperPickedPos.Y - lowerPickedPos.Y);
				}
			}
			this.Pressure = (float)(this.rect.Y - this.rect.Height) + this.waterVolume / (float)this.rect.Width;
			this.DetermineIsAirlock();
			BallastFloraBehavior ballastFlora = this.BallastFlora;
			if (ballastFlora == null)
			{
				return;
			}
			ballastFlora.OnMapLoaded();
		}

		// Token: 0x0600099D RID: 2461 RVA: 0x0005DBB4 File Offset: 0x0005BDB4
		public void AddToGrid(Submarine submarine)
		{
			foreach (EntityGrid grid in Hull.EntityGrids)
			{
				if (grid.Submarine == submarine)
				{
					this.rect.Location = this.rect.Location - MathUtils.ToPoint(submarine.HiddenSubPosition);
					grid.InsertEntity(this);
					this.rect.Location = this.rect.Location + MathUtils.ToPoint(submarine.HiddenSubPosition);
					break;
				}
			}
		}

		// Token: 0x0600099E RID: 2462 RVA: 0x0005DC54 File Offset: 0x0005BE54
		public int GetWaveIndex(Vector2 position)
		{
			return this.GetWaveIndex(position.X);
		}

		// Token: 0x0600099F RID: 2463 RVA: 0x0005DC64 File Offset: 0x0005BE64
		public int GetWaveIndex(float xPos)
		{
			int index = (int)(xPos - (float)this.rect.X) / 32;
			return MathHelper.Clamp(index, 0, this.waveY.Length - 1);
		}

		// Token: 0x060009A0 RID: 2464 RVA: 0x0005DC98 File Offset: 0x0005BE98
		public override void Move(Vector2 amount, bool ignoreContacts = true)
		{
			if (!MathUtils.IsValid(amount))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(49, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Attempted to move a hull by an invalid amount (");
				defaultInterpolatedStringHandler.AppendFormatted<Vector2>(amount);
				defaultInterpolatedStringHandler.AppendLiteral(")\n");
				defaultInterpolatedStringHandler.AppendFormatted(Environment.StackTrace.CleanupStackTrace());
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return;
			}
			this.rect.X = this.rect.X + (int)amount.X;
			this.rect.Y = this.rect.Y + (int)amount.Y;
			if (base.Submarine == null || !base.Submarine.Loading)
			{
				Item.UpdateHulls();
				Gap.UpdateHulls();
			}
			this.surface = (float)(this.rect.Y - this.rect.Height) + this.WaterVolume / (float)this.rect.Width;
			this.Pressure = this.surface;
		}

		// Token: 0x060009A1 RID: 2465 RVA: 0x0005DD84 File Offset: 0x0005BF84
		public override void ShallowRemove()
		{
			base.Remove();
			Hull.HullList.Remove(this);
			if (base.Submarine == null || (!base.Submarine.Loading && !Submarine.Unloading))
			{
				Item.UpdateHulls();
				Gap.UpdateHulls();
			}
			List<FireSource> fireSourcesToRemove = new List<FireSource>(this.FireSources);
			fireSourcesToRemove.AddRange(this.FakeFireSources);
			foreach (FireSource fireSource in fireSourcesToRemove)
			{
				fireSource.Remove();
			}
			this.FireSources.Clear();
			this.FakeFireSources.Clear();
			if (Hull.EntityGrids != null)
			{
				foreach (EntityGrid entityGrid in Hull.EntityGrids)
				{
					entityGrid.RemoveEntity(this);
				}
			}
		}

		// Token: 0x060009A2 RID: 2466 RVA: 0x0005DE84 File Offset: 0x0005C084
		public override void Remove()
		{
			base.Remove();
			Hull.HullList.Remove(this);
			BallastFloraBehavior ballastFlora = this.BallastFlora;
			if (ballastFlora != null)
			{
				ballastFlora.Remove();
			}
			if (base.Submarine != null && !base.Submarine.Loading && !Submarine.Unloading)
			{
				Item.UpdateHulls();
				Gap.UpdateHulls();
			}
			List<BackgroundSection> backgroundSections = this.BackgroundSections;
			if (backgroundSections != null)
			{
				backgroundSections.Clear();
			}
			List<FireSource> fireSourcesToRemove = new List<FireSource>(this.FireSources);
			foreach (FireSource fireSource in fireSourcesToRemove)
			{
				fireSource.Remove();
			}
			this.FireSources.Clear();
			if (Hull.EntityGrids != null)
			{
				foreach (EntityGrid entityGrid in Hull.EntityGrids)
				{
					entityGrid.RemoveEntity(this);
				}
			}
		}

		// Token: 0x060009A3 RID: 2467 RVA: 0x0005DF8C File Offset: 0x0005C18C
		public void AddFireSource(FireSource fireSource)
		{
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient && base.IdFreed)
			{
				return;
			}
			DummyFireSource dummyFire = fireSource as DummyFireSource;
			if (dummyFire != null)
			{
				this.FakeFireSources.Add(dummyFire);
				return;
			}
			if (this.FireSources.Count >= 16)
			{
				return;
			}
			this.FireSources.Add(fireSource);
		}

		// Token: 0x060009A4 RID: 2468 RVA: 0x0005DFE8 File Offset: 0x0005C1E8
		public Decal AddDecal(uint decalId, Vector2 worldPosition, float scale, bool isNetworkEvent, int? spriteIndex = null)
		{
			if (!isNetworkEvent && GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return null;
			}
			DecalPrefab decal = DecalManager.Prefabs.Find((DecalPrefab p) => p.UintIdentifier == decalId);
			if (decal == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(56, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Could not find a decal prefab with the UInt identifier ");
				defaultInterpolatedStringHandler.AppendFormatted<uint>(decalId);
				defaultInterpolatedStringHandler.AppendLiteral("!");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return null;
			}
			return this.AddDecal(decal.Name, worldPosition, scale, isNetworkEvent, spriteIndex);
		}

		// Token: 0x060009A5 RID: 2469 RVA: 0x0005E088 File Offset: 0x0005C288
		public Decal AddDecal(string decalName, Vector2 worldPosition, float scale, bool isNetworkEvent, int? spriteIndex = null)
		{
			if (!isNetworkEvent && GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return null;
			}
			if (this.decals.Count >= 10)
			{
				return null;
			}
			Decal decal = DecalManager.CreateDecal(decalName, scale, worldPosition, this, spriteIndex);
			if (decal != null)
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				if (networkMember != null && networkMember.IsServer)
				{
					GameMain.NetworkMember.CreateEntityEvent(this, default(Hull.DecalEventData));
				}
				this.decals.Add(decal);
			}
			return decal;
		}

		// Token: 0x060009A6 RID: 2470 RVA: 0x0005E108 File Offset: 0x0005C308
		private void SharedStatusWrite(IWriteMessage msg)
		{
			msg.WriteSingle(this.waterVolume);
			msg.WriteRangedInteger(Math.Min(this.FireSources.Count, 16), 0, 16);
			for (int i = 0; i < Math.Min(this.FireSources.Count, 16); i++)
			{
				FireSource fireSource = this.FireSources[i];
				Vector2 normalizedPos = new Vector2((fireSource.Position.X - (float)this.rect.X) / (float)this.rect.Width, (fireSource.Position.Y - (float)(this.rect.Y - this.rect.Height)) / (float)this.rect.Height);
				msg.WriteRangedSingle(MathHelper.Clamp(normalizedPos.X, 0f, 1f), 0f, 1f, 8);
				msg.WriteRangedSingle(MathHelper.Clamp(normalizedPos.Y, 0f, 1f), 0f, 1f, 8);
				msg.WriteRangedSingle(MathHelper.Clamp(fireSource.Size.X / (float)this.rect.Width, 0f, 1f), 0f, 1f, 8);
			}
		}

		// Token: 0x060009A7 RID: 2471 RVA: 0x0005E250 File Offset: 0x0005C450
		private void SharedBackgroundSectionsWrite(IWriteMessage msg, in Hull.BackgroundSectionsEventData backgroundSectionsEventData)
		{
			int sectorToUpdate = backgroundSectionsEventData.SectorStartIndex;
			int start = sectorToUpdate * 16;
			int end = Math.Min((sectorToUpdate + 1) * 16, this.BackgroundSections.Count - 1);
			msg.WriteRangedInteger(sectorToUpdate, 0, this.BackgroundSections.Count - 1);
			for (int i = start; i <= end; i++)
			{
				msg.WriteRangedSingle(this.BackgroundSections[i].ColorStrength, 0f, 1f, 8);
				msg.WriteUInt32(this.BackgroundSections[i].Color.PackedValue);
			}
		}

		// Token: 0x060009A8 RID: 2472 RVA: 0x0005E2E8 File Offset: 0x0005C4E8
		private void SharedStatusRead(IReadMessage msg, out float newWaterVolume, out Hull.NetworkFireSource[] newFireSources)
		{
			newWaterVolume = msg.ReadSingle();
			int fireSourceCount = msg.ReadRangedInteger(0, 16);
			newFireSources = new Hull.NetworkFireSource[fireSourceCount];
			for (int i = 0; i < fireSourceCount; i++)
			{
				float x = MathHelper.Clamp(msg.ReadRangedSingle(0f, 1f, 8), 0.05f, 0.95f);
				float y = MathHelper.Clamp(msg.ReadRangedSingle(0f, 1f, 8), 0.05f, 0.95f);
				float size = msg.ReadRangedSingle(0f, 1f, 8);
				newFireSources[i] = new Hull.NetworkFireSource(this, new Vector2(x, y), size);
			}
		}

		// Token: 0x060009A9 RID: 2473 RVA: 0x0005E388 File Offset: 0x0005C588
		private void SharedBackgroundSectionRead(IReadMessage msg, Action<Hull.BackgroundSectionNetworkUpdate> action, out int sectionToUpdate)
		{
			sectionToUpdate = msg.ReadRangedInteger(0, this.BackgroundSections.Count - 1);
			int start = sectionToUpdate * 16;
			int end = Math.Min((sectionToUpdate + 1) * 16, this.BackgroundSections.Count - 1);
			for (int i = start; i <= end; i++)
			{
				float colorStrength = msg.ReadRangedSingle(0f, 1f, 8);
				Color color = new Color(msg.ReadUInt32());
				action(new Hull.BackgroundSectionNetworkUpdate(i, color, colorStrength));
			}
		}

		// Token: 0x060009AA RID: 2474 RVA: 0x0005E408 File Offset: 0x0005C608
		public override void Update(float deltaTime, Camera cam)
		{
			BallastFloraBehavior ballastFlora = this.BallastFlora;
			if (ballastFlora != null)
			{
				ballastFlora.Update(deltaTime);
			}
			this.UpdateProjSpecific(deltaTime, cam);
			this.Oxygen -= 0.3f * deltaTime;
			if (this.FakeFireSources.Count > 0)
			{
				Character controlled = Character.Controlled;
				float? num;
				if (controlled == null)
				{
					num = null;
				}
				else
				{
					CharacterHealth characterHealth = controlled.CharacterHealth;
					if (characterHealth == null)
					{
						num = null;
					}
					else
					{
						Affliction affliction = characterHealth.GetAffliction("psychosis", true);
						num = ((affliction != null) ? new float?(affliction.Strength) : null);
					}
				}
				float? num2 = num;
				if (num2.GetValueOrDefault() <= 0f)
				{
					for (int i = this.FakeFireSources.Count - 1; i >= 0; i--)
					{
						if (this.FakeFireSources[i].CausedByPsychosis)
						{
							this.FakeFireSources[i].Remove();
						}
					}
				}
				FireSource.UpdateAll(this.FakeFireSources, deltaTime);
			}
			FireSource.UpdateAll(this.FireSources, deltaTime);
			foreach (Decal decal in this.decals)
			{
				decal.Update(deltaTime);
			}
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember == null || !networkMember.IsClient)
			{
				for (int j = this.decals.Count - 1; j >= 0; j--)
				{
					Decal decal2 = this.decals[j];
					if (decal2.FadeTimer >= decal2.LifeTime || decal2.BaseAlpha <= 0.001f)
					{
						this.decals.RemoveAt(j);
						this.decalUpdatePending = true;
					}
				}
			}
			if (this.aiTarget != null)
			{
				this.aiTarget.SightRange = ((base.Submarine == null) ? this.aiTarget.MinSightRange : MathHelper.Lerp(this.aiTarget.MinSightRange, this.aiTarget.MaxSightRange, base.Submarine.Velocity.Length() / 10f));
				this.aiTarget.SoundRange -= deltaTime * 1000f;
			}
			if (!this.update)
			{
				this.lethalPressure = 0f;
				return;
			}
			float waterDepth = this.WaterVolume / (float)this.rect.Width;
			if (waterDepth < 1f)
			{
				waterDepth = 0f;
			}
			this.surface = Math.Max(MathHelper.Lerp(this.surface, (float)(this.rect.Y - this.rect.Height) + waterDepth, deltaTime * 10f), (float)(this.rect.Y - this.rect.Height));
			for (int k = 0; k < this.waveY.Length; k++)
			{
				this.waveY[k] = this.waveY[k] + this.waveVel[k];
				if (this.surface + this.waveY[k] > (float)this.rect.Y)
				{
					float excess = this.surface + this.waveY[k] - (float)this.rect.Y;
					this.waveY[k] -= excess;
					this.waveVel[k] = this.waveVel[k] * -0.5f;
				}
				else if (this.surface + this.waveY[k] < (float)(this.rect.Y - this.rect.Height))
				{
					float excess2 = this.surface + this.waveY[k] - (float)(this.rect.Y - this.rect.Height);
					this.waveY[k] -= excess2;
					this.waveVel[k] = this.waveVel[k] * -0.5f;
				}
				float a = -Hull.WaveStiffness * this.waveY[k] - this.waveVel[k] * Hull.WaveDampening;
				this.waveVel[k] = this.waveVel[k] + a;
			}
			for (int l = 0; l < 2; l++)
			{
				for (int m = 1; m < this.waveY.Length - 1; m++)
				{
					this.leftDelta[m] = Hull.WaveSpread * (this.waveY[m] - this.waveY[m - 1]);
					this.waveVel[m - 1] += this.leftDelta[m];
					this.rightDelta[m] = Hull.WaveSpread * (this.waveY[m] - this.waveY[m + 1]);
					this.waveVel[m + 1] += this.rightDelta[m];
				}
			}
			foreach (Gap gap in this.ConnectedGaps)
			{
				if (this == gap.linkedTo.FirstOrDefault<MapEntity>() as Hull && gap.IsRoomToRoom && gap.IsHorizontal && gap.Open > 0f && this.surface <= (float)gap.Rect.Y && this.surface >= (float)(gap.Rect.Y - gap.Rect.Height))
				{
					Hull hull2 = (this == gap.linkedTo[0]) ? ((Hull)gap.linkedTo[1]) : ((Hull)gap.linkedTo[0]);
					float otherSurfaceY = hull2.surface;
					if (otherSurfaceY <= (float)gap.Rect.Y && otherSurfaceY >= (float)(gap.Rect.Y - gap.Rect.Height))
					{
						float surfaceDiff = (this.surface - otherSurfaceY) * gap.Open;
						for (int n = 0; n < 2; n++)
						{
							this.rightDelta[this.waveY.Length - 1] = Hull.WaveSpread * (hull2.waveY[0] - this.waveY[this.waveY.Length - 1] - surfaceDiff) * 0.5f;
							this.waveVel[this.waveY.Length - 1] += this.rightDelta[this.waveY.Length - 1];
							this.waveY[this.waveY.Length - 1] += this.rightDelta[this.waveY.Length - 1];
							hull2.leftDelta[0] = Hull.WaveSpread * (this.waveY[this.waveY.Length - 1] - hull2.waveY[0] + surfaceDiff) * 0.5f;
							hull2.waveVel[0] += hull2.leftDelta[0];
							hull2.waveY[0] += hull2.leftDelta[0];
						}
						if (surfaceDiff < 32f)
						{
							hull2.waveY[0] = surfaceDiff * 0.5f;
							this.waveY[this.waveY.Length - 1] = -surfaceDiff * 0.5f;
						}
					}
				}
			}
			for (int j2 = 0; j2 < 2; j2++)
			{
				for (int i2 = 1; i2 < this.waveY.Length - 1; i2++)
				{
					this.waveY[i2 - 1] += this.leftDelta[i2];
					this.waveY[i2 + 1] += this.rightDelta[i2];
				}
			}
			if (this.waterVolume < this.Volume)
			{
				float waterVolumeFactor = Math.Max((100f - this.WaterPercentage) / 10f, 1f);
				this.LethalPressure -= 10f * waterVolumeFactor * deltaTime;
				if (this.WaterVolume <= 0f)
				{
					for (int i3 = 1; i3 < this.waveY.Length - 1; i3++)
					{
						if (this.waveY[i3] > 0.1f)
						{
							return;
						}
					}
					this.update = false;
				}
			}
		}

		// Token: 0x060009AB RID: 2475 RVA: 0x0005EC4C File Offset: 0x0005CE4C
		private void UpdateProjSpecific(float deltaTime, Camera cam)
		{
			if (base.IdFreed)
			{
				return;
			}
			float hullUpdateDistanceSqr = NetConfig.HullUpdateDistance * NetConfig.HullUpdateDistance;
			if (!GameMain.Server.ConnectedClients.Any((Client c) => (c.Character != null && Vector2.DistanceSquared(c.Character.WorldPosition, this.WorldPosition) < hullUpdateDistanceSqr) || (c.SpectatePos != null && Vector2.DistanceSquared(c.SpectatePos.Value, this.WorldPosition) < hullUpdateDistanceSqr)))
			{
				return;
			}
			this.statusUpdateTimer += deltaTime;
			this.decalUpdateTimer += deltaTime;
			this.backgroundSectionUpdateTimer += deltaTime;
			if (((Math.Abs(this.lastSentVolume - this.waterVolume) > this.Volume * 0.1f || Math.Abs(this.lastSentOxygen - this.OxygenPercentage) > 5f || this.lastSentFireCount != this.FireSources.Count) && this.statusUpdateTimer > NetConfig.HullUpdateInterval) || this.statusUpdateTimer > NetConfig.SparseHullUpdateInterval)
			{
				GameMain.NetworkMember.CreateEntityEvent(this, default(Hull.StatusEventData));
				this.lastSentVolume = this.waterVolume;
				this.lastSentOxygen = this.OxygenPercentage;
				this.lastSentFireCount = this.FireSources.Count;
				this.statusUpdateTimer = 0f;
			}
			if (this.decalUpdatePending && this.decalUpdateTimer > NetConfig.HullUpdateInterval)
			{
				GameMain.NetworkMember.CreateEntityEvent(this, default(Hull.DecalEventData));
				this.decalUpdateTimer = 0f;
				this.decalUpdatePending = false;
			}
			if (this.pendingSectorUpdates.Count > 0 && this.backgroundSectionUpdateTimer > NetConfig.HullUpdateInterval)
			{
				foreach (int pendingSectorUpdate in this.pendingSectorUpdates)
				{
					GameMain.NetworkMember.CreateEntityEvent(this, new Hull.BackgroundSectionsEventData(pendingSectorUpdate));
				}
				this.backgroundSectionUpdateTimer = 0f;
				this.pendingSectorUpdates.Clear();
			}
		}

		// Token: 0x060009AC RID: 2476 RVA: 0x0005EE4C File Offset: 0x0005D04C
		public void ApplyFlowForces(float deltaTime, Item item)
		{
			if (item.body.Mass <= 0f)
			{
				return;
			}
			foreach (Gap gap2 in from gap in this.ConnectedGaps
			where gap.Open > 0f
			select gap)
			{
				float distance = MathHelper.Max(Vector2.DistanceSquared(item.Position, gap2.Position) / 1000f, 1f);
				Vector2 force = gap2.LerpedFlowForce / distance * deltaTime;
				if (force.LengthSquared() > 0.01f)
				{
					item.body.ApplyForce(force, 64f);
				}
			}
		}

		// Token: 0x060009AD RID: 2477 RVA: 0x0005EF20 File Offset: 0x0005D120
		public void Extinguish(float deltaTime, float amount, Vector2 position, bool extinguishRealFires = true, bool extinguishFakeFires = true)
		{
			if (extinguishRealFires)
			{
				for (int i = this.FireSources.Count - 1; i >= 0; i--)
				{
					this.FireSources[i].Extinguish(deltaTime, amount, position);
				}
			}
			if (extinguishFakeFires)
			{
				for (int j = this.FakeFireSources.Count - 1; j >= 0; j--)
				{
					this.FakeFireSources[j].Extinguish(deltaTime, amount, position);
				}
			}
		}

		// Token: 0x060009AE RID: 2478 RVA: 0x0005EF90 File Offset: 0x0005D190
		public void RemoveFire(FireSource fire)
		{
			this.FireSources.Remove(fire);
			DummyFireSource dummyFire = fire as DummyFireSource;
			if (dummyFire != null)
			{
				this.FakeFireSources.Remove(dummyFire);
			}
		}

		// Token: 0x060009AF RID: 2479 RVA: 0x0005EFC4 File Offset: 0x0005D1C4
		public IEnumerable<Hull> GetConnectedHulls(bool includingThis, int? searchDepth = null, bool ignoreClosedGaps = false)
		{
			this.adjacentHulls.Clear();
			int startStep = 0;
			int value = searchDepth.GetValueOrDefault();
			if (searchDepth == null)
			{
				value = 100;
				searchDepth = new int?(value);
			}
			this.GetAdjacentHulls(this.adjacentHulls, ref startStep, searchDepth.Value, ignoreClosedGaps);
			if (!includingThis)
			{
				this.adjacentHulls.Remove(this);
			}
			return this.adjacentHulls;
		}

		// Token: 0x060009B0 RID: 2480 RVA: 0x0005F028 File Offset: 0x0005D228
		private void GetAdjacentHulls(HashSet<Hull> connectedHulls, ref int step, int searchDepth, bool ignoreClosedGaps = false)
		{
			connectedHulls.Add(this);
			if (step > searchDepth)
			{
				return;
			}
			foreach (Gap g in this.ConnectedGaps)
			{
				if (!ignoreClosedGaps || g.Open > 0f)
				{
					int i = 0;
					while (i < 2 && i < g.linkedTo.Count)
					{
						Hull hull = g.linkedTo[i] as Hull;
						if (hull != null && !connectedHulls.Contains(hull))
						{
							step++;
							hull.GetAdjacentHulls(connectedHulls, ref step, searchDepth, ignoreClosedGaps);
						}
						i++;
					}
				}
			}
		}

		// Token: 0x060009B1 RID: 2481 RVA: 0x0005F0DC File Offset: 0x0005D2DC
		public float GetApproximateDistance(Vector2 startPos, Vector2 endPos, Hull targetHull, float maxDistance, float distanceMultiplierPerClosedDoor = 0f, float minimumGapOpenness = 0.5f)
		{
			Hull.cachedDistances.Clear();
			Hull.priorityQueue.Clear();
			Hull.cachedDistances[this] = 0f;
			Hull.priorityQueue.Enqueue(new ValueTuple<Hull, Vector2>(this, startPos), 0f);
			ValueTuple<Hull, Vector2> current;
			float currentDist;
			while (Hull.priorityQueue.TryDequeue(out current, out currentDist))
			{
				Hull currentHull = current.Item1;
				Vector2 currentPos = current.Item2;
				if (currentDist > maxDistance)
				{
					return float.MaxValue;
				}
				if (currentHull == targetHull)
				{
					return currentDist + Vector2.Distance(currentPos, endPos);
				}
				using (List<Gap>.Enumerator enumerator = currentHull.ConnectedGaps.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Gap g = enumerator.Current;
						float distanceMultiplier = 1f;
						if (g.ConnectedDoor != null && !g.ConnectedDoor.IsBroken)
						{
							if (((g.ConnectedDoor.IsClosed && g.ConnectedDoor.PredictedState == null) || (g.ConnectedDoor.PredictedState != null && !g.ConnectedDoor.PredictedState.Value)) && g.ConnectedDoor.OpenState < 0.1f)
							{
								if (distanceMultiplierPerClosedDoor <= 0f)
								{
									continue;
								}
								distanceMultiplier *= distanceMultiplierPerClosedDoor;
							}
						}
						else if (g.Open < minimumGapOpenness)
						{
							continue;
						}
						int i = 0;
						while (i < 2 && i < g.linkedTo.Count)
						{
							Hull nextHull = g.linkedTo[i] as Hull;
							if (nextHull != null && nextHull != currentHull)
							{
								float newDist = currentDist + Vector2.Distance(currentPos, g.Position) * distanceMultiplier;
								float oldDist;
								if (!Hull.cachedDistances.TryGetValue(nextHull, out oldDist) || newDist < oldDist)
								{
									Hull.cachedDistances[nextHull] = newDist;
									Hull.priorityQueue.Enqueue(new ValueTuple<Hull, Vector2>(nextHull, g.Position), newDist);
								}
							}
							i++;
						}
					}
					continue;
				}
				break;
			}
			return float.MaxValue;
		}

		// Token: 0x060009B2 RID: 2482 RVA: 0x0005F2F8 File Offset: 0x0005D4F8
		public static Hull FindHull(Vector2 position, Hull guess = null, bool useWorldCoordinates = true, bool inclusive = true)
		{
			if (Hull.EntityGrids == null)
			{
				return null;
			}
			if (guess != null && Submarine.RectContains(useWorldCoordinates ? guess.WorldRect : guess.rect, position, inclusive))
			{
				return guess;
			}
			foreach (EntityGrid entityGrid in Hull.EntityGrids)
			{
				if (entityGrid.Submarine != null && !entityGrid.Submarine.Loading)
				{
					Rectangle borders = entityGrid.Submarine.Borders;
					if (useWorldCoordinates)
					{
						Vector2 worldPos = entityGrid.Submarine.WorldPosition;
						borders.Location += new Point((int)worldPos.X, (int)worldPos.Y);
					}
					else
					{
						borders.Location += new Point((int)entityGrid.Submarine.HiddenSubPosition.X, (int)entityGrid.Submarine.HiddenSubPosition.Y);
					}
					if (position.X < (float)borders.X - 128f || position.X > (float)borders.Right + 128f || position.Y > (float)borders.Y + 128f || position.Y < (float)(borders.Y - borders.Height) - 128f)
					{
						continue;
					}
				}
				Vector2 transformedPosition = position;
				if (useWorldCoordinates && entityGrid.Submarine != null)
				{
					transformedPosition -= entityGrid.Submarine.Position;
				}
				List<MapEntity> entities = entityGrid.GetEntities(transformedPosition);
				if (entities != null)
				{
					foreach (MapEntity mapEntity in entities)
					{
						Hull hull = (Hull)mapEntity;
						if (Submarine.RectContains(hull.rect, transformedPosition, inclusive))
						{
							return hull;
						}
					}
				}
			}
			return null;
		}

		// Token: 0x060009B3 RID: 2483 RVA: 0x0005F510 File Offset: 0x0005D710
		public static Hull FindHullUnoptimized(Vector2 position, Hull guess = null, bool useWorldCoordinates = true, bool inclusive = true)
		{
			if (guess != null && Hull.HullList.Contains(guess) && Submarine.RectContains(useWorldCoordinates ? guess.WorldRect : guess.rect, position, inclusive))
			{
				return guess;
			}
			foreach (Hull hull in Hull.HullList)
			{
				if (Submarine.RectContains(useWorldCoordinates ? hull.WorldRect : hull.rect, position, inclusive))
				{
					return hull;
				}
			}
			return null;
		}

		// Token: 0x060009B4 RID: 2484 RVA: 0x0005F5A8 File Offset: 0x0005D7A8
		public void GetLinkedHulls(List<Hull> linkedHulls, bool includeHiddenHulls = false)
		{
			foreach (MapEntity linkedEntity in this.linkedTo)
			{
				Hull linkedHull = linkedEntity as Hull;
				if (linkedHull != null && !linkedHulls.Contains(linkedHull) && (includeHiddenHulls || !linkedHull.IsHidden))
				{
					linkedHulls.Add(linkedHull);
					linkedHull.GetLinkedHulls(linkedHulls, includeHiddenHulls);
				}
			}
		}

		// Token: 0x060009B5 RID: 2485 RVA: 0x0005F624 File Offset: 0x0005D824
		public static void DetectItemVisibility(Character c = null)
		{
			if (c == null)
			{
				using (List<Item>.Enumerator enumerator = Item.ItemList.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Item it = enumerator.Current;
						it.Visible = true;
					}
					return;
				}
			}
			Hull h = c.CurrentHull;
			Hull.HullList.ForEach(delegate(Hull j)
			{
				j.Visible = false;
			});
			List<Hull> visibleHulls;
			if (h == null || c.Submarine == null)
			{
				visibleHulls = Hull.HullList.FindAll((Hull j) => j.CanSeeOther(null, false));
			}
			else
			{
				visibleHulls = Hull.HullList.FindAll((Hull j) => h.CanSeeOther(j, true));
			}
			visibleHulls.ForEach(delegate(Hull j)
			{
				j.Visible = true;
			});
			foreach (Item it2 in Item.ItemList)
			{
				if (it2.CurrentHull == null || visibleHulls.Contains(it2.CurrentHull))
				{
					it2.Visible = true;
				}
				else
				{
					it2.Visible = false;
				}
			}
		}

		// Token: 0x060009B6 RID: 2486 RVA: 0x0005F798 File Offset: 0x0005D998
		private bool CanSeeOther(Hull other, bool allowIndirect = true)
		{
			if (other == this)
			{
				return true;
			}
			if (other != null && other.Submarine == base.Submarine)
			{
				using (List<Gap>.Enumerator enumerator = this.ConnectedGaps.GetEnumerator())
				{
					Func<Hull, bool> <>9__1;
					Func<Hull, bool> <>9__2;
					while (enumerator.MoveNext())
					{
						Gap g = enumerator.Current;
						if (g.ConnectedWall == null || !g.ConnectedWall.CastShadow)
						{
							List<Hull> otherHulls = Hull.HullList.FindAll((Hull h) => h.ConnectedGaps.Contains(g) && h != this);
							IEnumerable<Hull> source = otherHulls;
							Func<Hull, bool> predicate;
							if ((predicate = <>9__1) == null)
							{
								predicate = (<>9__1 = ((Hull h) => h == other));
							}
							bool retVal = source.Any(predicate);
							if (!retVal && allowIndirect)
							{
								IEnumerable<Hull> source2 = otherHulls;
								Func<Hull, bool> predicate2;
								if ((predicate2 = <>9__2) == null)
								{
									predicate2 = (<>9__2 = ((Hull h) => h.CanSeeOther(other, false)));
								}
								retVal = source2.Any(predicate2);
							}
							if (retVal)
							{
								return true;
							}
						}
					}
					return false;
				}
			}
			using (List<Gap>.Enumerator enumerator2 = this.ConnectedGaps.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					Gap g = enumerator2.Current;
					if (g.ConnectedDoor != null && !Hull.HullList.Any((Hull h) => h.ConnectedGaps.Contains(g) && h != this))
					{
						return true;
					}
				}
			}
			List<MapEntity> structures = MapEntity.MapEntityList.FindAll((MapEntity me) => me is Structure && me.Rect.Intersects(this.Rect));
			return structures.Any((MapEntity st) => !(st as Structure).CastShadow);
		}

		// Token: 0x060009B7 RID: 2487 RVA: 0x0005F9C0 File Offset: 0x0005DBC0
		public string CreateRoomName()
		{
			List<string> roomItems = new List<string>();
			foreach (Item item in Item.ItemList)
			{
				if (item.CurrentHull == this)
				{
					if (item.GetComponent<Reactor>() != null)
					{
						roomItems.Add("reactor");
					}
					if (item.GetComponent<Engine>() != null)
					{
						roomItems.Add("engine");
					}
					if (item.GetComponent<Steering>() != null)
					{
						roomItems.Add("steering");
					}
					if (item.GetComponent<Sonar>() != null)
					{
						roomItems.Add("sonar");
					}
					if (item.HasTag(Tags.Ballast))
					{
						roomItems.Add("ballast");
					}
				}
			}
			if (roomItems.Contains("reactor"))
			{
				return "RoomName.ReactorRoom";
			}
			if (roomItems.Contains("engine"))
			{
				return "RoomName.EngineRoom";
			}
			if (roomItems.Contains("steering") && roomItems.Contains("sonar"))
			{
				return "RoomName.CommandRoom";
			}
			if (roomItems.Contains("ballast"))
			{
				return "RoomName.Ballast";
			}
			Submarine submarine = base.Submarine;
			IEnumerable<Identifier> enumerable;
			if (submarine == null)
			{
				enumerable = null;
			}
			else
			{
				SubmarineInfo info = submarine.Info;
				if (info == null)
				{
					enumerable = null;
				}
				else
				{
					OutpostModuleInfo outpostModuleInfo = info.OutpostModuleInfo;
					enumerable = ((outpostModuleInfo != null) ? outpostModuleInfo.ModuleFlags : null);
				}
			}
			IEnumerable<Identifier> moduleFlags = enumerable ?? this.moduleTags;
			if (moduleFlags != null && moduleFlags.Any<Identifier>() && (base.Submarine.Info.Type == SubmarineType.OutpostModule || base.Submarine.Info.Type == SubmarineType.Outpost))
			{
				if (moduleFlags.Contains(Tags.Airlock))
				{
					if (this.ConnectedGaps.Any((Gap g) => !g.IsRoomToRoom && g.ConnectedDoor != null))
					{
						return "RoomName.Airlock";
					}
				}
			}
			else if (this.ConnectedGaps.Any((Gap g) => !g.IsRoomToRoom && g.ConnectedDoor != null))
			{
				return "RoomName.Airlock";
			}
			Rectangle subRect = base.Submarine.Borders;
			Alignment roomPos;
			if ((float)(this.rect.Y - this.rect.Height / 2) > (float)subRect.Y + (float)subRect.Height * 0.66f)
			{
				roomPos = Alignment.Top;
			}
			else if ((float)(this.rect.Y - this.rect.Height / 2) > (float)subRect.Y + (float)subRect.Height * 0.33f)
			{
				roomPos = Alignment.CenterY;
			}
			else
			{
				roomPos = Alignment.Bottom;
			}
			if ((float)this.rect.Center.X < (float)subRect.X + (float)subRect.Width * 0.33f)
			{
				roomPos |= Alignment.Left;
			}
			else if ((float)this.rect.Center.X < (float)subRect.X + (float)subRect.Width * 0.66f)
			{
				roomPos |= Alignment.CenterX;
			}
			else
			{
				roomPos |= Alignment.Right;
			}
			return "RoomName.Sub" + roomPos.ToString();
		}

		// Token: 0x060009B8 RID: 2488 RVA: 0x0005FCAC File Offset: 0x0005DEAC
		private void DetermineIsAirlock()
		{
			if (this.RoomName != null && this.RoomName.Contains("airlock", StringComparison.OrdinalIgnoreCase))
			{
				this.IsAirlock = true;
				return;
			}
			Identifier airlockTag = "airlock".ToIdentifier();
			foreach (Item item in Item.ItemList)
			{
				if (item.CurrentHull != this && item.HasTag(airlockTag))
				{
					this.IsAirlock = true;
					return;
				}
			}
			this.IsAirlock = false;
		}

		// Token: 0x060009B9 RID: 2489 RVA: 0x0005FD48 File Offset: 0x0005DF48
		public bool LeadsOutside(Character character)
		{
			foreach (Gap gap in this.ConnectedGaps)
			{
				if (gap.ConnectedDoor != null && !gap.IsRoomToRoom && (gap.ConnectedDoor.CanBeTraversed || (character != null && gap.ConnectedDoor.HasAccess(character))))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060009BA RID: 2490 RVA: 0x0005FDCC File Offset: 0x0005DFCC
		private void CreateBackgroundSections()
		{
			int sectionWidth;
			int sectionHeight = sectionWidth = 16;
			this.xBackgroundMax = this.rect.Width / sectionWidth;
			this.yBackgroundMax = this.rect.Height / sectionHeight;
			this.BackgroundSections = new List<BackgroundSection>(this.xBackgroundMax * this.yBackgroundMax);
			int sections = this.xBackgroundMax * this.yBackgroundMax;
			float xSectors = (float)this.xBackgroundMax / 4f;
			for (int y = 0; y < this.yBackgroundMax; y++)
			{
				for (int x = 0; x < this.xBackgroundMax; x++)
				{
					ushort index = (ushort)this.BackgroundSections.Count;
					int sector = (int)Math.Floor((double)((float)index / 4f - xSectors * (float)y)) + y / 4 * (int)Math.Ceiling((double)xSectors);
					this.BackgroundSections.Add(new BackgroundSection(new Rectangle(x * sectionWidth, y * -sectionHeight, sectionWidth, sectionHeight), index, (ushort)y));
				}
			}
		}

		// Token: 0x060009BB RID: 2491 RVA: 0x0005FEBC File Offset: 0x0005E0BC
		public static Hull GetCleanTarget(Vector2 worldPosition)
		{
			foreach (Hull hull in Hull.HullList)
			{
				Rectangle worldRect = hull.WorldRect;
				if (worldPosition.X >= (float)worldRect.X && worldPosition.X <= (float)worldRect.Right && worldPosition.Y <= (float)worldRect.Y && worldPosition.Y >= (float)(worldRect.Y - worldRect.Height))
				{
					return hull;
				}
			}
			return null;
		}

		// Token: 0x060009BC RID: 2492 RVA: 0x0005FF5C File Offset: 0x0005E15C
		public BackgroundSection GetBackgroundSection(Vector2 worldPosition)
		{
			if (!this.SupportsPaintedColors)
			{
				return null;
			}
			Vector2 subOffset = (base.Submarine == null) ? Vector2.Zero : base.Submarine.Position;
			Vector2 relativePosition = new Vector2(worldPosition.X - subOffset.X - (float)this.rect.X, worldPosition.Y - subOffset.Y - (float)this.rect.Y);
			int xIndex = (int)Math.Floor((double)(relativePosition.X / 16f));
			if (xIndex < 0 || xIndex >= this.xBackgroundMax)
			{
				return null;
			}
			int yIndex = (int)Math.Floor((double)(-(double)relativePosition.Y / 16f));
			if (yIndex < 0 || yIndex >= this.yBackgroundMax)
			{
				return null;
			}
			return this.BackgroundSections[xIndex + yIndex * this.xBackgroundMax];
		}

		// Token: 0x060009BD RID: 2493 RVA: 0x00060028 File Offset: 0x0005E228
		public Vector2 GetBackgroundSectionWorldPos(BackgroundSection backgroundSection)
		{
			Vector2 subOffset = (base.Submarine == null) ? Vector2.Zero : base.Submarine.Position;
			return this.Rect.Location.ToVector2() + subOffset + new Vector2((float)backgroundSection.Rect.X, (float)backgroundSection.Rect.Y);
		}

		// Token: 0x060009BE RID: 2494 RVA: 0x0006008E File Offset: 0x0005E28E
		public IEnumerable<BackgroundSection> GetBackgroundSectionsViaContaining(Rectangle rectArea)
		{
			Hull.<GetBackgroundSectionsViaContaining>d__203 <GetBackgroundSectionsViaContaining>d__ = new Hull.<GetBackgroundSectionsViaContaining>d__203(-2);
			<GetBackgroundSectionsViaContaining>d__.<>4__this = this;
			<GetBackgroundSectionsViaContaining>d__.<>3__rectArea = rectArea;
			return <GetBackgroundSectionsViaContaining>d__;
		}

		// Token: 0x060009BF RID: 2495 RVA: 0x000600A5 File Offset: 0x0005E2A5
		public bool DoesSectionMatch(int index, int row)
		{
			return index >= 0 && row >= 0 && this.BackgroundSections.Count > index && this.BackgroundSections[index] != null && (int)this.BackgroundSections[index].RowIndex == row;
		}

		// Token: 0x060009C0 RID: 2496 RVA: 0x000600E4 File Offset: 0x0005E2E4
		public void IncreaseSectionColorOrStrength(BackgroundSection section, Color? color, float? strength, bool requiresUpdate, bool isCleaning)
		{
			bool sectionUpdated = isCleaning;
			if (color != null)
			{
				if (section.Color != color.Value && strength != null)
				{
					float changeSpeed = strength.Value / Math.Max(section.ColorStrength * section.ColorStrength, 0.001f) * 0.1f;
					if (section.LerpColor(color.Value, changeSpeed))
					{
						sectionUpdated = true;
					}
				}
				else if (section.SetColor(color.Value))
				{
					sectionUpdated = true;
				}
			}
			if (strength != null)
			{
				float previous = section.SetColorStrength(Math.Max(0f, Math.Min(0.7f, section.ColorStrength + strength.Value)));
				if (previous != -1f)
				{
					sectionUpdated = true;
				}
				this.RefreshAveragePaintedColor();
			}
			if (sectionUpdated && GameMain.NetworkMember != null && requiresUpdate)
			{
				this.networkUpdatePending = true;
				this.pendingSectorUpdates.Add((int)Math.Floor((double)((float)section.Index / 16f)));
			}
		}

		// Token: 0x060009C1 RID: 2497 RVA: 0x000601E4 File Offset: 0x0005E3E4
		private void RefreshAveragePaintedColor()
		{
			Vector4 avgColor = Vector4.Zero;
			foreach (BackgroundSection anySection in this.BackgroundSections)
			{
				avgColor += anySection.Color.ToVector4();
			}
			avgColor /= (float)this.BackgroundSections.Count;
			this.AveragePaintedColor = new Color(avgColor);
		}

		// Token: 0x060009C2 RID: 2498 RVA: 0x0006026C File Offset: 0x0005E46C
		public void SetSectionColorOrStrength(BackgroundSection section, Color? color, float? strength)
		{
			if (color != null)
			{
				section.SetColor(color.Value);
			}
			if (strength != null)
			{
				float previous = section.SetColorStrength(Math.Max(0f, Math.Min(0.7f, section.ColorStrength + strength.Value)));
			}
		}

		// Token: 0x060009C3 RID: 2499 RVA: 0x000602CC File Offset: 0x0005E4CC
		public void CleanSection(BackgroundSection section, float cleanVal, bool updateRequired)
		{
			bool decalsCleaned = false;
			foreach (Decal decal in this.decals)
			{
				if (decal.BaseAlpha > 0.001f && decal.AffectsSection(section))
				{
					decal.Clean(cleanVal);
					decalsCleaned = true;
					this.decalUpdatePending = true;
				}
			}
			if (section.ColorStrength == 0f && !decalsCleaned)
			{
				return;
			}
			this.IncreaseSectionColorOrStrength(section, null, new float?(cleanVal), updateRequired, true);
		}

		// Token: 0x060009C4 RID: 2500 RVA: 0x0006036C File Offset: 0x0005E56C
		public static Hull Load(ContentXElement element, Submarine submarine, IdRemap idRemap)
		{
			Rectangle rect;
			if (element.GetAttribute("rect") != null)
			{
				string key = "rect";
				Rectangle rectangle = Rectangle.Empty;
				rect = element.GetAttributeRect(key, rectangle);
			}
			else
			{
				rect = new Rectangle(int.Parse(element.GetAttribute("x").Value), int.Parse(element.GetAttribute("y").Value), int.Parse(element.GetAttribute("width").Value), int.Parse(element.GetAttribute("height").Value));
			}
			Hull hull = new Hull(rect, submarine, idRemap.GetOffsetId(element))
			{
				WaterVolume = element.GetAttributeFloat("water", 0f)
			};
			hull.linkedToID = new List<ushort>();
			hull.ParseLinks(element, idRemap);
			string originalAmbientLight = element.GetAttributeString("originalambientlight", null);
			if (!string.IsNullOrWhiteSpace(originalAmbientLight))
			{
				hull.OriginalAmbientLight = new Color?(XMLExtensions.ParseColor(originalAmbientLight, false));
			}
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "decal"))
				{
					if (a == "ballastflorabehavior")
					{
						Identifier identifier = subElement.GetAttributeIdentifier("identifier", Identifier.Empty);
						BallastFloraPrefab prefab = BallastFloraPrefab.Find(identifier);
						if (prefab != null)
						{
							hull.BallastFlora = new BallastFloraBehavior(hull, prefab, Vector2.Zero, false);
							hull.BallastFlora.LoadSave(subElement, idRemap);
						}
					}
				}
				else
				{
					string id = subElement.GetAttributeString("id", "");
					ContentXElement contentXElement = subElement;
					string key2 = "pos";
					Vector2 zero = Vector2.Zero;
					Vector2 pos = contentXElement.GetAttributeVector2(key2, zero);
					float scale = subElement.GetAttributeFloat("scale", 1f);
					float timer = subElement.GetAttributeFloat("timer", 1f);
					float baseAlpha = subElement.GetAttributeFloat("alpha", 1f);
					Hull hull2 = hull;
					string decalName = id;
					Vector2 value = pos;
					Rectangle rectangle = hull.WorldRect;
					Decal decal = hull2.AddDecal(decalName, value + rectangle.Location.ToVector2(), scale, true, null);
					if (decal != null)
					{
						decal.FadeTimer = timer;
						decal.BaseAlpha = baseAlpha;
					}
				}
			}
			string backgroundSectionStr = element.GetAttributeString("backgroundsections", "");
			if (!string.IsNullOrEmpty(backgroundSectionStr))
			{
				string[] backgroundSectionStrSplit = backgroundSectionStr.Split(';', StringSplitOptions.None);
				foreach (string str in backgroundSectionStrSplit)
				{
					string[] backgroundSectionData = str.Split(':', StringSplitOptions.None);
					if (backgroundSectionData.Length == 3)
					{
						Color color = XMLExtensions.ParseColor(backgroundSectionData[1], true);
						int index;
						float strength;
						if (int.TryParse(backgroundSectionData[0], out index) && float.TryParse(backgroundSectionData[2], NumberStyles.Any, CultureInfo.InvariantCulture, out strength))
						{
							hull.SetSectionColorOrStrength(hull.BackgroundSections[index], new Color?(color), new float?(strength));
						}
					}
				}
			}
			hull.RefreshAveragePaintedColor();
			SerializableProperty.DeserializeProperties(hull, element);
			if (element.GetAttribute("oxygen") == null)
			{
				hull.Oxygen = hull.Volume;
			}
			return hull;
		}

		// Token: 0x060009C5 RID: 2501 RVA: 0x000606B8 File Offset: 0x0005E8B8
		public override XElement Save(XElement parentElement)
		{
			if (base.Submarine == null)
			{
				string errorMsg = "Error - tried to save a hull that's not a part of any submarine.\n" + Environment.StackTrace.CleanupStackTrace();
				DebugConsole.ThrowError(errorMsg, null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("Hull.Save:WorldHull", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return null;
			}
			XElement element = new XElement("Hull");
			element.Add(new object[]
			{
				new XAttribute("ID", this.ID),
				new XAttribute("rect", string.Concat(new string[]
				{
					((int)((float)this.rect.X - base.Submarine.HiddenSubPosition.X)).ToString(),
					",",
					((int)((float)this.rect.Y - base.Submarine.HiddenSubPosition.Y)).ToString(),
					",",
					this.rect.Width.ToString(),
					",",
					this.rect.Height.ToString()
				})),
				new XAttribute("water", this.waterVolume)
			});
			if (this.linkedTo != null && this.linkedTo.Count > 0)
			{
				List<MapEntity> saveableLinked = (from l in this.linkedTo
				where l.ShouldBeSaved && l.Removed == base.Removed
				select l).ToList<MapEntity>();
				element.Add(new XAttribute("linked", string.Join(",", from l in saveableLinked
				select l.ID.ToString())));
			}
			if (this.OriginalAmbientLight != null)
			{
				element.Add(new XAttribute("originalambientlight", XMLExtensions.ColorToString(this.OriginalAmbientLight.Value)));
			}
			if (this.BackgroundSections != null && this.BackgroundSections.Count > 0)
			{
				element.Add(new XAttribute("backgroundsections", string.Join<string>(';', from b in this.BackgroundSections
				where b.ColorStrength > 0.01f
				select string.Concat(new string[]
				{
					b.Index.ToString(),
					":",
					XMLExtensions.ColorToString(b.Color),
					":",
					b.ColorStrength.ToString("G", CultureInfo.InvariantCulture)
				}))));
			}
			foreach (Decal decal in this.decals)
			{
				element.Add(new XElement("decal", new object[]
				{
					new XAttribute("id", decal.Prefab.Identifier),
					new XAttribute("pos", XMLExtensions.Vector2ToString(decal.NonClampedPosition)),
					new XAttribute("scale", decal.Scale.ToString("G", CultureInfo.InvariantCulture)),
					new XAttribute("timer", decal.FadeTimer.ToString("G", CultureInfo.InvariantCulture)),
					new XAttribute("alpha", decal.BaseAlpha.ToString("G", CultureInfo.InvariantCulture))
				}));
			}
			BallastFloraBehavior ballastFlora = this.BallastFlora;
			if (ballastFlora != null)
			{
				ballastFlora.Save(element);
			}
			SerializableProperty.SerializeProperties(this, element, false, false);
			parentElement.Add(element);
			return element;
		}

		// Token: 0x060009C6 RID: 2502 RVA: 0x00060A6C File Offset: 0x0005EC6C
		public override string ToString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(5, 3);
			defaultInterpolatedStringHandler.AppendFormatted(base.ToString());
			defaultInterpolatedStringHandler.AppendLiteral(" (");
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.DisplayName ?? "unnamed");
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			Submarine submarine = base.Submarine;
			string text;
			if (submarine == null)
			{
				text = null;
			}
			else
			{
				SubmarineInfo info = submarine.Info;
				text = ((info != null) ? info.Name : null);
			}
			defaultInterpolatedStringHandler.AppendFormatted(text ?? "no sub");
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04000428 RID: 1064
		private float lastSentVolume;

		// Token: 0x04000429 RID: 1065
		private float lastSentOxygen;

		// Token: 0x0400042A RID: 1066
		private int lastSentFireCount;

		// Token: 0x0400042B RID: 1067
		private float statusUpdateTimer;

		// Token: 0x0400042C RID: 1068
		private float decalUpdateTimer;

		// Token: 0x0400042D RID: 1069
		private float backgroundSectionUpdateTimer;

		// Token: 0x0400042E RID: 1070
		private bool decalUpdatePending;

		// Token: 0x0400042F RID: 1071
		public static readonly List<Hull> HullList = new List<Hull>();

		// Token: 0x04000430 RID: 1072
		public static readonly List<EntityGrid> EntityGrids = new List<EntityGrid>();

		// Token: 0x04000431 RID: 1073
		public static bool ShowHulls = true;

		// Token: 0x04000432 RID: 1074
		public static bool EditWater;

		// Token: 0x04000433 RID: 1075
		public static bool EditFire;

		// Token: 0x04000434 RID: 1076
		public const float OxygenDistributionSpeed = 30000f;

		// Token: 0x04000435 RID: 1077
		public const float OxygenDeteriorationSpeed = 0.3f;

		// Token: 0x04000436 RID: 1078
		public const float OxygenConsumptionSpeed = 700f;

		// Token: 0x04000437 RID: 1079
		private const float DecalAlphaRemoveThreshold = 0.001f;

		// Token: 0x04000438 RID: 1080
		public const int WaveWidth = 32;

		// Token: 0x04000439 RID: 1081
		public static float WaveStiffness = 0.01f;

		// Token: 0x0400043A RID: 1082
		public static float WaveSpread = 0.02f;

		// Token: 0x0400043B RID: 1083
		public static float WaveDampening = 0.02f;

		// Token: 0x0400043C RID: 1084
		public const float MaxCompress = 1.05f;

		// Token: 0x0400043D RID: 1085
		public const int BackgroundSectionSize = 16;

		// Token: 0x0400043E RID: 1086
		public const int BackgroundSectionsPerNetworkEvent = 16;

		// Token: 0x0400043F RID: 1087
		public readonly Dictionary<Identifier, SerializableProperty> properties;

		// Token: 0x04000440 RID: 1088
		public const float PressureBuildUpSpeed = 15f;

		// Token: 0x04000441 RID: 1089
		public const float PressureDropSpeed = 10f;

		// Token: 0x04000442 RID: 1090
		private float lethalPressure;

		// Token: 0x04000443 RID: 1091
		private float surface;

		// Token: 0x04000444 RID: 1092
		private float waterVolume;

		// Token: 0x04000445 RID: 1093
		private float pressure;

		// Token: 0x04000446 RID: 1094
		private float oxygen;

		// Token: 0x04000447 RID: 1095
		private bool update;

		// Token: 0x04000448 RID: 1096
		public bool Visible = true;

		// Token: 0x04000449 RID: 1097
		private float[] waveY;

		// Token: 0x0400044A RID: 1098
		private float[] waveVel;

		// Token: 0x0400044B RID: 1099
		private float[] leftDelta;

		// Token: 0x0400044C RID: 1100
		private float[] rightDelta;

		// Token: 0x0400044D RID: 1101
		public const int MaxDecalsPerHull = 10;

		// Token: 0x0400044E RID: 1102
		private readonly List<Decal> decals = new List<Decal>();

		// Token: 0x0400044F RID: 1103
		public readonly List<Gap> ConnectedGaps = new List<Gap>();

		// Token: 0x04000451 RID: 1105
		private readonly HashSet<Identifier> moduleTags = new HashSet<Identifier>();

		// Token: 0x04000452 RID: 1106
		private string roomName;

		// Token: 0x04000453 RID: 1107
		public Color? OriginalAmbientLight;

		// Token: 0x04000454 RID: 1108
		private Color ambientLight;

		// Token: 0x04000457 RID: 1111
		private bool isWetRoom;

		// Token: 0x04000458 RID: 1112
		private bool avoidStaying;

		// Token: 0x0400045A RID: 1114
		private readonly HashSet<int> pendingSectorUpdates = new HashSet<int>();

		// Token: 0x0400045B RID: 1115
		public int xBackgroundMax;

		// Token: 0x0400045C RID: 1116
		public int yBackgroundMax;

		// Token: 0x0400045D RID: 1117
		private const int SectionWidth = 4;

		// Token: 0x0400045E RID: 1118
		private const int SectionHeight = 4;

		// Token: 0x0400045F RID: 1119
		private const float minColorStrength = 0f;

		// Token: 0x04000460 RID: 1120
		private const float maxColorStrength = 0.7f;

		// Token: 0x04000461 RID: 1121
		private bool networkUpdatePending;

		// Token: 0x04000462 RID: 1122
		private float networkUpdateTimer;

		// Token: 0x04000464 RID: 1124
		public const int MaxFireSources = 16;

		// Token: 0x04000468 RID: 1128
		private readonly HashSet<Hull> adjacentHulls = new HashSet<Hull>();

		// Token: 0x04000469 RID: 1129
		private static readonly Dictionary<Hull, float> cachedDistances = new Dictionary<Hull, float>();

		// Token: 0x0400046A RID: 1130
		[TupleElementNames(new string[]
		{
			"hull",
			"pos"
		})]
		private static readonly PriorityQueue<ValueTuple<Hull, Vector2>, float> priorityQueue = new PriorityQueue<ValueTuple<Hull, Vector2>, float>();

		// Token: 0x02000711 RID: 1809
		public readonly struct NetworkFireSource
		{
			// Token: 0x060050A9 RID: 20649 RVA: 0x001E78C0 File Offset: 0x001E5AC0
			public NetworkFireSource(Hull hull, Vector2 normalizedPosition, float normalizedSize)
			{
				this.Position = hull.Rect.Location.ToVector2() + new Vector2(0f, (float)(-(float)hull.Rect.Height)) + normalizedPosition * hull.Rect.Size.ToVector2();
				this.Size = normalizedSize * (float)hull.Rect.Width;
			}

			// Token: 0x04002BA9 RID: 11177
			public readonly Vector2 Position;

			// Token: 0x04002BAA RID: 11178
			public readonly float Size;
		}

		// Token: 0x02000712 RID: 1810
		private readonly struct BackgroundSectionNetworkUpdate
		{
			// Token: 0x060050AA RID: 20650 RVA: 0x001E793A File Offset: 0x001E5B3A
			public BackgroundSectionNetworkUpdate(int sectionIndex, Color color, float colorStrength)
			{
				this.SectionIndex = sectionIndex;
				this.Color = color;
				this.ColorStrength = colorStrength;
			}

			// Token: 0x04002BAB RID: 11179
			public readonly int SectionIndex;

			// Token: 0x04002BAC RID: 11180
			public readonly Color Color;

			// Token: 0x04002BAD RID: 11181
			public readonly float ColorStrength;
		}

		// Token: 0x02000713 RID: 1811
		[Flags]
		public enum EventType
		{
			// Token: 0x04002BAF RID: 11183
			Status = 0,
			// Token: 0x04002BB0 RID: 11184
			Decal = 1,
			// Token: 0x04002BB1 RID: 11185
			BackgroundSections = 2,
			// Token: 0x04002BB2 RID: 11186
			BallastFlora = 3,
			// Token: 0x04002BB3 RID: 11187
			MinValue = 0,
			// Token: 0x04002BB4 RID: 11188
			MaxValue = 3
		}

		// Token: 0x02000714 RID: 1812
		public interface IEventData : NetEntityEvent.IData
		{
			// Token: 0x1700141A RID: 5146
			// (get) Token: 0x060050AB RID: 20651
			Hull.EventType EventType { get; }
		}

		// Token: 0x02000715 RID: 1813
		private readonly struct StatusEventData : Hull.IEventData, NetEntityEvent.IData
		{
			// Token: 0x1700141B RID: 5147
			// (get) Token: 0x060050AC RID: 20652 RVA: 0x001E7951 File Offset: 0x001E5B51
			public Hull.EventType EventType
			{
				get
				{
					return Hull.EventType.Status;
				}
			}
		}

		// Token: 0x02000716 RID: 1814
		private readonly struct DecalEventData : Hull.IEventData, NetEntityEvent.IData
		{
			// Token: 0x1700141C RID: 5148
			// (get) Token: 0x060050AD RID: 20653 RVA: 0x001E7954 File Offset: 0x001E5B54
			public Hull.EventType EventType
			{
				get
				{
					return Hull.EventType.Decal;
				}
			}

			// Token: 0x060050AE RID: 20654 RVA: 0x001E7957 File Offset: 0x001E5B57
			public DecalEventData(Decal decal)
			{
				this.Decal = decal;
			}

			// Token: 0x04002BB5 RID: 11189
			public readonly Decal Decal;
		}

		// Token: 0x02000717 RID: 1815
		private readonly struct BackgroundSectionsEventData : Hull.IEventData, NetEntityEvent.IData
		{
			// Token: 0x1700141D RID: 5149
			// (get) Token: 0x060050AF RID: 20655 RVA: 0x001E7960 File Offset: 0x001E5B60
			public Hull.EventType EventType
			{
				get
				{
					return Hull.EventType.BackgroundSections;
				}
			}

			// Token: 0x060050B0 RID: 20656 RVA: 0x001E7963 File Offset: 0x001E5B63
			public BackgroundSectionsEventData(int sectorStartIndex)
			{
				this.SectorStartIndex = sectorStartIndex;
			}

			// Token: 0x04002BB6 RID: 11190
			public readonly int SectorStartIndex;
		}

		// Token: 0x02000718 RID: 1816
		public readonly struct BallastFloraEventData : Hull.IEventData, NetEntityEvent.IData
		{
			// Token: 0x1700141E RID: 5150
			// (get) Token: 0x060050B1 RID: 20657 RVA: 0x001E796C File Offset: 0x001E5B6C
			public Hull.EventType EventType
			{
				get
				{
					return Hull.EventType.BallastFlora;
				}
			}

			// Token: 0x060050B2 RID: 20658 RVA: 0x001E796F File Offset: 0x001E5B6F
			public BallastFloraEventData(BallastFloraBehavior behavior, BallastFloraBehavior.IEventData subEventData)
			{
				this.Behavior = behavior;
				this.SubEventData = subEventData;
			}

			// Token: 0x04002BB7 RID: 11191
			public readonly BallastFloraBehavior Behavior;

			// Token: 0x04002BB8 RID: 11192
			public readonly BallastFloraBehavior.IEventData SubEventData;
		}
	}
}
