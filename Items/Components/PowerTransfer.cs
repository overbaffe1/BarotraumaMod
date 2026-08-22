using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Barotrauma.Particles;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005CE RID: 1486
	internal class PowerTransfer : Powered
	{
		// Token: 0x170017C4 RID: 6084
		// (get) Token: 0x06005E62 RID: 24162 RVA: 0x003133D4 File Offset: 0x003115D4
		public override bool RecreateGUIOnResolutionChange
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06005E63 RID: 24163 RVA: 0x003133D8 File Offset: 0x003115D8
		protected override void CreateGUI()
		{
			if (base.GuiFrame == null)
			{
				return;
			}
			this.guiContent = new GUIFrame(new RectTransform(base.GuiFrame.Rect.Size - GUIStyle.ItemFrameMargin, base.GuiFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false)
			{
				AbsoluteOffset = GUIStyle.ItemFrameOffset
			}, null, null)
			{
				CanBeFocused = false
			};
			this.CreateDefaultPowerUI(this.guiContent);
		}

		// Token: 0x06005E64 RID: 24164 RVA: 0x0031345C File Offset: 0x0031165C
		protected void CreateDefaultPowerUI(GUIComponent parent)
		{
			GUILayoutGroup lightsArea = new GUILayoutGroup(new RectTransform(new Vector2(0.4f, 1f), parent.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			this.powerIndicator = GUI.CreateIndicatorLight(new RectTransform(new Vector2(1f, 0.33f), lightsArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "IndicatorLightGreen", TextManager.Get("PowerTransferPowered"), null, null);
			this.highVoltageIndicator = GUI.CreateIndicatorLight(new RectTransform(new Vector2(1f, 0.33f), lightsArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "IndicatorLightRed", TextManager.Get("PowerTransferHighVoltage"), TextManager.Get("PowerTransferTipOvervoltage"), null);
			this.lowVoltageIndicator = GUI.CreateIndicatorLight(new RectTransform(new Vector2(1f, 0.33f), lightsArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "IndicatorLightRed", TextManager.Get("PowerTransferLowVoltage"), TextManager.Get("PowerTransferTipLowvoltage"), null);
			GUITextBlock.AutoScaleAndNormalize(new GUITextBlock[]
			{
				this.powerIndicator.TextBlock,
				this.highVoltageIndicator.TextBlock,
				this.lowVoltageIndicator.TextBlock
			});
			GUIFrame textContainer = new GUIFrame(new RectTransform(new Vector2(0.58f, 1f), parent.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), null, null);
			GUITextBlock unitLabel;
			this.powerDisplay = GUI.CreateDigitalDisplay(new RectTransform(new Vector2(1f, 0.5f), textContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), out this.powerLabel, out unitLabel, TextManager.Get("PowerTransferPowerLabel"), TextManager.Get("kilowatt"), TextManager.Get("PowerTransferTipPower"), null);
			this.powerDisplay.TextGetter = delegate()
			{
				float currPower = (this.powerLoad < 0f) ? (-this.powerLoad) : 0f;
				if (!(this is RelayComponent) && this.PowerConnections != null && this.PowerConnections.Count > 0 && this.PowerConnections[0].Grid != null)
				{
					currPower = this.PowerConnections[0].Grid.Power;
				}
				return MathUtils.RoundToInt(currPower).ToString();
			};
			GUITextBlock unitLabel2;
			this.loadDisplay = GUI.CreateDigitalDisplay(new RectTransform(new Vector2(1f, 0.5f), textContainer.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal), out this.loadLabel, out unitLabel2, TextManager.Get("PowerTransferLoadLabel"), TextManager.Get("kilowatt"), TextManager.Get("PowerTransferTipLoad"), null);
			this.loadDisplay.TextGetter = delegate()
			{
				float load = this.PowerLoad;
				RelayComponent relay = this as RelayComponent;
				if (relay != null)
				{
					load = relay.DisplayLoad;
				}
				else if (load < 0f)
				{
					load = 0f;
				}
				return MathUtils.RoundToInt(load).ToString();
			};
			GUITextBlock.AutoScaleAndNormalize(new GUITextBlock[]
			{
				this.powerLabel,
				this.loadLabel
			});
			GUITextBlock.AutoScaleAndNormalize(true, true, new GUITextBlock[]
			{
				this.powerDisplay,
				this.loadDisplay
			});
			GUITextBlock.AutoScaleAndNormalize(new GUITextBlock[]
			{
				unitLabel,
				unitLabel2
			});
		}

		// Token: 0x06005E65 RID: 24165 RVA: 0x0031379C File Offset: 0x0031199C
		public override void UpdateHUDComponentSpecific(Character character, float deltaTime, Camera cam)
		{
			if (base.GuiFrame == null)
			{
				return;
			}
			float voltage = (this.PowerConnections.Count > 0 && this.PowerConnections[0].Grid != null) ? this.PowerConnections[0].Grid.Voltage : 0f;
			this.powerIndicator.Selected = (this.IsActive && voltage > 0f);
			this.highVoltageIndicator.Selected = (Timing.TotalTime % 0.5 < 0.25 && this.powerIndicator.Selected && voltage > 1.2f);
			this.lowVoltageIndicator.Selected = (Timing.TotalTime % 0.5 < 0.25 && this.powerIndicator.Selected && voltage < 0.8f);
			if (this.prevLanguage != GameSettings.CurrentConfig.Language)
			{
				GUITextBlock.AutoScaleAndNormalize(new GUITextBlock[]
				{
					this.powerIndicator.TextBlock,
					this.highVoltageIndicator.TextBlock,
					this.lowVoltageIndicator.TextBlock
				});
				GUITextBlock.AutoScaleAndNormalize(new GUITextBlock[]
				{
					this.powerLabel,
					this.loadLabel
				});
				this.prevLanguage = GameSettings.CurrentConfig.Language;
			}
		}

		// Token: 0x170017C5 RID: 6085
		// (get) Token: 0x06005E66 RID: 24166 RVA: 0x00313902 File Offset: 0x00311B02
		// (set) Token: 0x06005E67 RID: 24167 RVA: 0x0031390A File Offset: 0x00311B0A
		public List<Connection> PowerConnections { get; private set; }

		// Token: 0x170017C6 RID: 6086
		// (get) Token: 0x06005E68 RID: 24168 RVA: 0x00313914 File Offset: 0x00311B14
		// (set) Token: 0x06005E69 RID: 24169 RVA: 0x00313966 File Offset: 0x00311B66
		public float PowerLoad
		{
			get
			{
				if (this is RelayComponent || this.PowerConnections.Count == 0 || this.PowerConnections[0].Grid == null)
				{
					return this.powerLoad;
				}
				return this.PowerConnections[0].Grid.Load;
			}
			set
			{
				this.powerLoad = value;
			}
		}

		// Token: 0x170017C7 RID: 6087
		// (get) Token: 0x06005E6A RID: 24170 RVA: 0x0031396F File Offset: 0x00311B6F
		// (set) Token: 0x06005E6B RID: 24171 RVA: 0x00313977 File Offset: 0x00311B77
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Can the item be damaged if too much power is supplied to the power grid.", "", false)]
		public bool CanBeOverloaded { get; set; }

		// Token: 0x170017C8 RID: 6088
		// (get) Token: 0x06005E6C RID: 24172 RVA: 0x00313980 File Offset: 0x00311B80
		// (set) Token: 0x06005E6D RID: 24173 RVA: 0x00313988 File Offset: 0x00311B88
		[Editable(MinValueFloat = 1f)]
		[Serialize(2f, IsPropertySaveable.Yes, "How much power has to be supplied to the grid relative to the load before item starts taking damage. E.g. a value of 2 means that the grid has to be receiving twice as much power as the devices in the grid are consuming.", "", false)]
		public float OverloadVoltage { get; set; }

		// Token: 0x170017C9 RID: 6089
		// (get) Token: 0x06005E6E RID: 24174 RVA: 0x00313991 File Offset: 0x00311B91
		// (set) Token: 0x06005E6F RID: 24175 RVA: 0x00313999 File Offset: 0x00311B99
		[Serialize(0.15f, IsPropertySaveable.Yes, "The probability for a fire to start when the item breaks.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f)]
		public float FireProbability { get; set; }

		// Token: 0x170017CA RID: 6090
		// (get) Token: 0x06005E70 RID: 24176 RVA: 0x003139A2 File Offset: 0x00311BA2
		// (set) Token: 0x06005E71 RID: 24177 RVA: 0x003139AA File Offset: 0x00311BAA
		[Serialize(false, IsPropertySaveable.No, "Is the item currently overloaded. Intended to be used by StatusEffect conditionals (setting the value from XML is not recommended).", "", false)]
		public bool Overload { get; set; }

		// Token: 0x170017CB RID: 6091
		// (get) Token: 0x06005E72 RID: 24178 RVA: 0x003139B3 File Offset: 0x00311BB3
		// (set) Token: 0x06005E73 RID: 24179 RVA: 0x003139BB File Offset: 0x00311BBB
		public float ExtraLoad
		{
			get
			{
				return this.extraLoad;
			}
			set
			{
				this.extraLoad = value;
				this.extraLoadSetTime = (float)Timing.TotalTime;
			}
		}

		// Token: 0x170017CC RID: 6092
		// (get) Token: 0x06005E74 RID: 24180 RVA: 0x003139D0 File Offset: 0x00311BD0
		// (set) Token: 0x06005E75 RID: 24181 RVA: 0x003139D8 File Offset: 0x00311BD8
		public bool CanTransfer
		{
			get
			{
				return this.canTransfer;
			}
			set
			{
				if (this.canTransfer == value)
				{
					return;
				}
				this.canTransfer = value;
				this.SetAllConnectionsDirty();
			}
		}

		// Token: 0x170017CD RID: 6093
		// (get) Token: 0x06005E76 RID: 24182 RVA: 0x003139F1 File Offset: 0x00311BF1
		// (set) Token: 0x06005E77 RID: 24183 RVA: 0x003139F9 File Offset: 0x00311BF9
		public override bool IsActive
		{
			get
			{
				return base.IsActive;
			}
			set
			{
				if (base.IsActive == value)
				{
					return;
				}
				base.IsActive = value;
				this.powerLoad = 0f;
				this.currPowerConsumption = 0f;
				this.SetAllConnectionsDirty();
				if (!base.IsActive)
				{
					this.RefreshConnections();
				}
			}
		}

		// Token: 0x06005E78 RID: 24184 RVA: 0x00313A38 File Offset: 0x00311C38
		public PowerTransfer(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
			this.canTransfer = true;
			this.InitProjectSpecific(element);
		}

		// Token: 0x06005E79 RID: 24185 RVA: 0x00313A88 File Offset: 0x00311C88
		private void InitProjectSpecific(XElement element)
		{
			if (base.GuiFrame == null)
			{
				return;
			}
			this.CreateGUI();
			this.prevLanguage = GameSettings.CurrentConfig.Language;
		}

		// Token: 0x06005E7A RID: 24186 RVA: 0x00313AAC File Offset: 0x00311CAC
		public override void UpdateBroken(float deltaTime, Camera cam)
		{
			base.UpdateBroken(deltaTime, cam);
			this.Overload = false;
			if (!this.isBroken)
			{
				this.powerLoad = 0f;
				this.currPowerConsumption = 0f;
				this.SetAllConnectionsDirty();
				PowerTransfer.recipientsToRefresh.Clear();
				foreach (HashSet<Connection> recipientList in this.connectedRecipients.Values)
				{
					foreach (Connection c in recipientList)
					{
						if (c.Item != this.item)
						{
							PowerTransfer recipientPowerTransfer = c.Item.GetComponent<PowerTransfer>();
							if (recipientPowerTransfer != null)
							{
								PowerTransfer.recipientsToRefresh.Add(recipientPowerTransfer);
							}
						}
					}
				}
				foreach (PowerTransfer recipientPowerTransfer2 in PowerTransfer.recipientsToRefresh)
				{
					recipientPowerTransfer2.SetAllConnectionsDirty();
					recipientPowerTransfer2.RefreshConnections();
				}
				this.RefreshConnections();
				this.isBroken = true;
			}
		}

		// Token: 0x06005E7B RID: 24187 RVA: 0x00313BF8 File Offset: 0x00311DF8
		public override void Update(float deltaTime, Camera cam)
		{
			this.RefreshConnections();
			this.UpdateExtraLoad(deltaTime);
			if (!this.CanTransfer)
			{
				return;
			}
			if (this.isBroken)
			{
				this.SetAllConnectionsDirty();
				this.isBroken = false;
			}
			base.ApplyStatusEffects(ActionType.OnActive, deltaTime, null, null, null, null, null, 1f);
			this.SendSignals();
			this.UpdateOvervoltage(deltaTime);
		}

		// Token: 0x06005E7C RID: 24188 RVA: 0x00313C58 File Offset: 0x00311E58
		protected virtual void UpdateExtraLoad(float deltaTime)
		{
			if (Timing.TotalTime <= (double)this.extraLoadSetTime + 1.0)
			{
				return;
			}
			if (this.extraLoad > 0f)
			{
				this.extraLoad = Math.Max(this.extraLoad - 1000f * deltaTime, 0f);
				return;
			}
			this.extraLoad = Math.Min(this.extraLoad + 1000f * deltaTime, 0f);
		}

		// Token: 0x06005E7D RID: 24189 RVA: 0x00313CC8 File Offset: 0x00311EC8
		protected virtual void SendSignals()
		{
			float powerReadingOut = 0f;
			float loadReadingOut = this.ExtraLoad;
			if (this.powerLoad < 0f)
			{
				powerReadingOut = -this.powerLoad;
				loadReadingOut = 0f;
			}
			if (base.powerOut != null && base.powerOut.Grid != null)
			{
				powerReadingOut = base.powerOut.Grid.Power;
				loadReadingOut = base.powerOut.Grid.Load;
			}
			if (this.prevSentPowerValue != (int)powerReadingOut || this.powerSignal == null)
			{
				this.prevSentPowerValue = (int)Math.Round((double)powerReadingOut);
				this.powerSignal = this.prevSentPowerValue.ToString();
			}
			if (this.prevSentLoadValue != (int)loadReadingOut || this.loadSignal == null)
			{
				this.prevSentLoadValue = (int)Math.Round((double)loadReadingOut);
				this.loadSignal = this.prevSentLoadValue.ToString();
			}
			this.item.SendSignal(this.powerSignal, "power_value_out");
			this.item.SendSignal(this.loadSignal, "load_value_out");
		}

		// Token: 0x06005E7E RID: 24190 RVA: 0x00313DC4 File Offset: 0x00311FC4
		protected virtual void UpdateOvervoltage(float deltaTime)
		{
			if (!this.item.Repairables.Any<Repairable>() || !this.CanBeOverloaded)
			{
				return;
			}
			float maxOverVoltage = Math.Max(this.OverloadVoltage, 1f);
			bool overload;
			if (base.Voltage > maxOverVoltage)
			{
				GameSession gameSession = GameMain.GameSession;
				overload = (gameSession == null || gameSession.RoundDuration >= 5f);
			}
			else
			{
				overload = false;
			}
			this.Overload = overload;
			if (this.Overload)
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				if (networkMember == null || !networkMember.IsClient)
				{
					if (this.overloadCooldownTimer > 0f)
					{
						this.overloadCooldownTimer -= deltaTime;
						return;
					}
					float prevCondition = this.item.Condition;
					if (Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < 0.01f)
					{
						float conditionFactor = MathHelper.Lerp(5f, 1f, this.item.Condition / this.item.MaxCondition);
						this.item.Condition -= deltaTime * Rand.Range(10f, 500f, Rand.RandSync.Unsynced) * conditionFactor;
					}
					if (this.item.Condition > 0f || prevCondition <= 0f)
					{
						return;
					}
					this.overloadCooldownTimer = 5f;
					string soundTag = "zap";
					Vector2 worldPosition = this.item.WorldPosition;
					Hull currentHull = this.item.CurrentHull;
					SoundPlayer.PlaySound(soundTag, worldPosition, null, null, currentHull);
					Vector2 baseVel = Rand.Vector(300f, Rand.RandSync.Unsynced);
					for (int i = 0; i < 10; i++)
					{
						Particle particle = GameMain.ParticleManager.CreateParticle("spark", this.item.WorldPosition, baseVel + Rand.Vector(100f, Rand.RandSync.Unsynced), 0f, this.item.CurrentHull, 0f, null);
						if (particle != null)
						{
							particle.Size *= Rand.Range(0.5f, 1f, Rand.RandSync.Unsynced);
						}
					}
					GameSession gameSession2 = GameMain.GameSession;
					float currentIntensity = (((gameSession2 != null) ? gameSession2.EventManager : null) != null) ? GameMain.GameSession.EventManager.CurrentIntensity : 0.5f;
					if (this.FireProbability > 0f && Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < MathHelper.Lerp(this.FireProbability, this.FireProbability * 0.1f, currentIntensity))
					{
						new FireSource(this.item.WorldPosition, null, null, false);
					}
					return;
				}
			}
		}

		// Token: 0x06005E7F RID: 24191 RVA: 0x00314033 File Offset: 0x00312233
		public override float GetConnectionPowerOut(Connection conn, float power, PowerRange minMaxPower, float load)
		{
			if (conn != base.powerOut)
			{
				return 0f;
			}
			return MathHelper.Max(-(base.PowerConsumption + this.ExtraLoad), 0f);
		}

		// Token: 0x06005E80 RID: 24192 RVA: 0x0031405C File Offset: 0x0031225C
		public override bool Pick(Character picker)
		{
			return picker != null;
		}

		// Token: 0x06005E81 RID: 24193 RVA: 0x00314064 File Offset: 0x00312264
		protected void RefreshConnections()
		{
			List<Connection> connections = this.item.Connections;
			foreach (Connection c in connections)
			{
				if (!this.connectionDirty.ContainsKey(c))
				{
					this.connectionDirty[c] = true;
				}
				else if (!this.connectionDirty[c])
				{
					continue;
				}
				HashSet<Connection> tempConnected;
				if (!this.connectedRecipients.ContainsKey(c))
				{
					tempConnected = new HashSet<Connection>();
					this.connectedRecipients.Add(c, tempConnected);
				}
				else
				{
					tempConnected = this.connectedRecipients[c];
					tempConnected.Clear();
					foreach (Connection recipient in tempConnected)
					{
						PowerTransfer pt = recipient.Item.GetComponent<PowerTransfer>();
						if (pt != null)
						{
							pt.connectionDirty[recipient] = true;
						}
					}
				}
				tempConnected.Add(c);
				if (this.item.Condition > 0f)
				{
					this.GetConnected(c, tempConnected);
					foreach (Connection recipient2 in tempConnected)
					{
						if (recipient2 != c)
						{
							PowerTransfer recipientPowerTransfer = recipient2.Item.GetComponent<PowerTransfer>();
							if (recipientPowerTransfer != null)
							{
								if (!recipientPowerTransfer.connectedRecipients.ContainsKey(recipient2))
								{
									recipientPowerTransfer.connectedRecipients.Add(recipient2, new HashSet<Connection>());
								}
								else
								{
									recipientPowerTransfer.connectedRecipients[recipient2].Clear();
								}
								foreach (Connection connection in tempConnected)
								{
									recipientPowerTransfer.connectedRecipients[recipient2].Add(connection);
								}
								recipientPowerTransfer.connectionDirty[recipient2] = false;
							}
						}
					}
				}
				this.connectionDirty[c] = false;
			}
		}

		// Token: 0x06005E82 RID: 24194 RVA: 0x003142CC File Offset: 0x003124CC
		private void GetConnected(Connection c, HashSet<Connection> connected)
		{
			List<Connection> recipients = c.Recipients;
			foreach (Connection recipient in recipients)
			{
				if (recipient != null && !connected.Contains(recipient))
				{
					Item it = recipient.Item;
					if (it != null && it.Condition > 0f)
					{
						connected.Add(recipient);
						PowerTransfer powerTransfer = it.GetComponent<PowerTransfer>();
						if (powerTransfer != null && powerTransfer.CanTransfer && powerTransfer.IsActive)
						{
							this.GetConnected(recipient, connected);
						}
					}
				}
			}
		}

		// Token: 0x06005E83 RID: 24195 RVA: 0x0031436C File Offset: 0x0031256C
		public void SetAllConnectionsDirty()
		{
			if (this.item.Connections == null)
			{
				return;
			}
			foreach (Connection c2 in this.item.Connections)
			{
				this.connectionDirty[c2] = true;
				if (c2.IsPower)
				{
					Powered.ChangedConnections.Add(c2);
					HashSet<Connection> recipients;
					if (this.connectedRecipients.TryGetValue(c2, out recipients))
					{
						(from c in recipients
						where c.IsPower
						select c).ForEach(delegate(Connection c)
						{
							Powered.ChangedConnections.Add(c);
						});
					}
				}
			}
		}

		// Token: 0x06005E84 RID: 24196 RVA: 0x0031444C File Offset: 0x0031264C
		public void SetConnectionDirty(Connection connection)
		{
			List<Connection> connections = this.item.Connections;
			if (connections == null || !connections.Contains(connection))
			{
				return;
			}
			this.connectionDirty[connection] = true;
			if (connection.IsPower)
			{
				Powered.ChangedConnections.Add(connection);
				HashSet<Connection> recipients;
				if (this.connectedRecipients.TryGetValue(connection, out recipients))
				{
					(from c in recipients
					where c.IsPower
					select c).ForEach(delegate(Connection c)
					{
						Powered.ChangedConnections.Add(c);
					});
				}
			}
		}

		// Token: 0x06005E85 RID: 24197 RVA: 0x003144EC File Offset: 0x003126EC
		public override void OnItemLoaded()
		{
			base.OnItemLoaded();
			List<Connection> connections = base.Item.Connections;
			List<Connection> powerConnections;
			if (connections != null)
			{
				powerConnections = connections.FindAll((Connection c) => c.IsPower);
			}
			else
			{
				powerConnections = new List<Connection>();
			}
			this.PowerConnections = powerConnections;
			if (connections == null)
			{
				this.IsActive = false;
				return;
			}
			foreach (Connection c2 in connections)
			{
				if (c2.Name.Length > 5 && c2.Name.Substring(0, 6) == "signal")
				{
					this.signalConnections.Add(c2);
				}
			}
			if (!(this is RelayComponent) && !(this is PowerDistributor))
			{
				if (this.PowerConnections.Any((Connection p) => !p.IsOutput))
				{
					if (this.PowerConnections.Any((Connection p) => p.IsOutput))
					{
						DebugConsole.ThrowError("Error in item \"" + base.Name + "\" - PowerTransfer components should not have separate power inputs and outputs, but transfer power between wires connected to the same power connection. If you want power to pass from input to output, change the component to a RelayComponent.", null, null, false, false);
					}
				}
			}
			this.SetAllConnectionsDirty();
		}

		// Token: 0x06005E86 RID: 24198 RVA: 0x00314648 File Offset: 0x00312848
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			if (this.item.Condition <= 0f || connection.IsPower)
			{
				return;
			}
			if (!this.connectedRecipients.ContainsKey(connection))
			{
				return;
			}
			if (!this.signalConnections.Contains(connection))
			{
				return;
			}
			foreach (Connection recipient in this.connectedRecipients[connection])
			{
				if (recipient.Item != this.item && recipient.Item != signal.source)
				{
					Item source = signal.source;
					if (source != null)
					{
						source.LastSentSignalRecipients.Add(recipient);
					}
					foreach (ItemComponent ic in recipient.Item.Components)
					{
						PowerTransfer powerTransfer = ic as PowerTransfer;
						if (powerTransfer == null || powerTransfer is RelayComponent || powerTransfer is PowerDistributor)
						{
							ic.ReceiveSignal(signal, recipient);
						}
					}
					if (recipient.Effects != null && signal.value != "0" && !string.IsNullOrEmpty(signal.value))
					{
						foreach (StatusEffect effect in recipient.Effects)
						{
							recipient.Item.ApplyStatusEffect(effect, ActionType.OnUse, 1f, null, null, null, false, true, null);
						}
					}
				}
			}
		}

		// Token: 0x06005E87 RID: 24199 RVA: 0x00314824 File Offset: 0x00312A24
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			Dictionary<Connection, HashSet<Connection>> dictionary = this.connectedRecipients;
			if (dictionary != null)
			{
				dictionary.Clear();
			}
			Dictionary<Connection, bool> dictionary2 = this.connectionDirty;
			if (dictionary2 != null)
			{
				dictionary2.Clear();
			}
			PowerTransfer.recipientsToRefresh.Clear();
		}

		// Token: 0x040030BB RID: 12475
		protected GUIComponent guiContent;

		// Token: 0x040030BC RID: 12476
		private GUITickBox powerIndicator;

		// Token: 0x040030BD RID: 12477
		private GUITickBox highVoltageIndicator;

		// Token: 0x040030BE RID: 12478
		private GUITickBox lowVoltageIndicator;

		// Token: 0x040030BF RID: 12479
		private GUITextBlock powerLabel;

		// Token: 0x040030C0 RID: 12480
		private GUITextBlock loadLabel;

		// Token: 0x040030C1 RID: 12481
		protected GUITextBlock powerDisplay;

		// Token: 0x040030C2 RID: 12482
		protected GUITextBlock loadDisplay;

		// Token: 0x040030C3 RID: 12483
		protected LanguageIdentifier prevLanguage;

		// Token: 0x040030C5 RID: 12485
		private readonly HashSet<Connection> signalConnections = new HashSet<Connection>();

		// Token: 0x040030C6 RID: 12486
		private readonly Dictionary<Connection, bool> connectionDirty = new Dictionary<Connection, bool>();

		// Token: 0x040030C7 RID: 12487
		private readonly Dictionary<Connection, HashSet<Connection>> connectedRecipients = new Dictionary<Connection, HashSet<Connection>>();

		// Token: 0x040030C8 RID: 12488
		private float overloadCooldownTimer;

		// Token: 0x040030C9 RID: 12489
		private const float OverloadCooldown = 5f;

		// Token: 0x040030CA RID: 12490
		protected float powerLoad;

		// Token: 0x040030CB RID: 12491
		protected bool isBroken;

		// Token: 0x040030D0 RID: 12496
		private float extraLoad;

		// Token: 0x040030D1 RID: 12497
		private float extraLoadSetTime;

		// Token: 0x040030D2 RID: 12498
		private bool canTransfer;

		// Token: 0x040030D3 RID: 12499
		private static readonly HashSet<PowerTransfer> recipientsToRefresh = new HashSet<PowerTransfer>();

		// Token: 0x040030D4 RID: 12500
		private int prevSentPowerValue;

		// Token: 0x040030D5 RID: 12501
		private string powerSignal;

		// Token: 0x040030D6 RID: 12502
		private int prevSentLoadValue;

		// Token: 0x040030D7 RID: 12503
		private string loadSignal;
	}
}
