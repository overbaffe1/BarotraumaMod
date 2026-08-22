using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Barotrauma.IO;
using Barotrauma.Items.Components;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x02000043 RID: 67
	internal class KarmaManager : ISerializableEntity
	{
		// Token: 0x06000AD7 RID: 2775 RVA: 0x0006A588 File Offset: 0x00068788
		public void UpdateClients(IEnumerable<Client> clients, float deltaTime)
		{
			if (!GameMain.Server.GameStarted)
			{
				return;
			}
			this.bannedClients.Clear();
			foreach (Client client in clients)
			{
				this.UpdateClient(client, deltaTime);
				if (this.perSecondUpdate < DateTime.Now)
				{
					KarmaManager.ClientMemory clientMemory = this.GetClientMemory(client);
					clientMemory.StructureDamagePerSecond = clientMemory.StructureDamageAccumulator;
					clientMemory.StructureDamageAccumulator = 0f;
					clientMemory.StunsInPastMinute.RemoveAll((KarmaManager.ClientMemory.TimeAmount s) => s.Time + 60.0 < Timing.TotalTime);
					if (!clientMemory.StunsInPastMinute.Any<KarmaManager.ClientMemory.TimeAmount>())
					{
						clientMemory.StunKarmaDecreaseMultiplier = 1f;
					}
					List<Character> toRemove = (from pair in clientMemory.LastAttackTime
					where pair.Value < Timing.TotalTime - (double)this.AllowedRetaliationTime
					select pair.Key).ToList<Character>();
					foreach (Character lastAttacker in toRemove)
					{
						clientMemory.LastAttackTime.Remove(lastAttacker);
					}
				}
			}
			if (this.perSecondUpdate < DateTime.Now)
			{
				foreach (Client client2 in clients)
				{
					this.SendKarmaNotifications(client2, "");
				}
				this.perSecondUpdate = DateTime.Now + new TimeSpan(0, 0, 1);
			}
			foreach (Client bannedClient in this.bannedClients)
			{
				bannedClient.KarmaKickCount++;
				if (bannedClient.KarmaKickCount <= this.KicksBeforeBan)
				{
					GameServer server = GameMain.Server;
					Client client3 = bannedClient;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(27, 1);
					defaultInterpolatedStringHandler.AppendLiteral("KarmaKicked~[banthreshold]=");
					defaultInterpolatedStringHandler.AppendFormatted<int>((int)this.KickBanThreshold);
					server.KickClient(client3, defaultInterpolatedStringHandler.ToStringAndClear(), true);
				}
				else
				{
					GameServer server2 = GameMain.Server;
					Client client4 = bannedClient;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(27, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("KarmaBanned~[banthreshold]=");
					defaultInterpolatedStringHandler2.AppendFormatted<int>((int)this.KickBanThreshold);
					server2.BanClient(client4, defaultInterpolatedStringHandler2.ToStringAndClear(), new TimeSpan?(TimeSpan.FromSeconds((double)GameMain.Server.ServerSettings.AutoBanTime)));
				}
			}
		}

		// Token: 0x06000AD8 RID: 2776 RVA: 0x0006A878 File Offset: 0x00068A78
		private void SendKarmaNotifications(Client client, string debugKarmaChangeReason = "")
		{
			KarmaManager.ClientMemory clientMemory = this.GetClientMemory(client);
			float karmaChange = client.Karma - clientMemory.PreviousNotifiedKarma;
			if (Math.Abs(karmaChange) > 1f && this.TestMode)
			{
				string text;
				if (karmaChange >= 0f)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(28, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Your karma has increased to ");
					defaultInterpolatedStringHandler.AppendFormatted<float>(client.Karma);
					text = defaultInterpolatedStringHandler.ToStringAndClear();
				}
				else
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(28, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("Your karma has decreased to ");
					defaultInterpolatedStringHandler2.AppendFormatted<float>(client.Karma);
					text = defaultInterpolatedStringHandler2.ToStringAndClear();
				}
				string msg = text;
				if (!string.IsNullOrEmpty(debugKarmaChangeReason))
				{
					msg = msg + ". Reason: " + debugKarmaChangeReason;
				}
				GameMain.Server.SendDirectChatMessage(msg, client, ChatMessageType.Server);
				clientMemory.PreviousNotifiedKarma = client.Karma;
				clientMemory.PreviousKarmaNotificationTime = Timing.TotalTime;
				return;
			}
			if (Timing.TotalTime >= clientMemory.PreviousKarmaNotificationTime + 5.0 && clientMemory.PreviousNotifiedKarma >= this.KickBanThreshold + this.KarmaNotificationInterval && client.Karma < this.KickBanThreshold + this.KarmaNotificationInterval)
			{
				GameMain.Server.SendDirectChatMessage(TextManager.Get("KarmaBanWarning").Value, client, ChatMessageType.Server);
				GameServer.Log(NetworkMember.ClientLogName(client, null) + " has been warned for having dangerously low karma.", ServerLog.MessageType.Karma);
				clientMemory.PreviousNotifiedKarma = client.Karma;
				clientMemory.PreviousKarmaNotificationTime = Timing.TotalTime;
			}
		}

		// Token: 0x06000AD9 RID: 2777 RVA: 0x0006A9DC File Offset: 0x00068BDC
		private void UpdateClient(Client client, float deltaTime)
		{
			if (client.Character != null && !client.Character.Removed && !client.Character.IsDead)
			{
				if (client.Karma > this.KarmaDecayThreshold)
				{
					client.Karma -= this.KarmaDecay * deltaTime;
				}
				else if (client.Karma < this.KarmaIncreaseThreshold)
				{
					client.Karma += this.KarmaIncrease * deltaTime;
				}
				float herpesStrength = 0f;
				if (client.Karma < this.HerpesThreshold * 0.5f)
				{
					herpesStrength = 100f;
				}
				else if (client.Karma < this.HerpesThreshold * 0.75f)
				{
					herpesStrength = 60f;
				}
				else if (client.Karma < this.HerpesThreshold)
				{
					herpesStrength = 30f;
				}
				AfflictionSpaceHerpes existingAffliction = client.Character.CharacterHealth.GetAffliction<AfflictionSpaceHerpes>(AfflictionPrefab.SpaceHerpesType, true);
				if (existingAffliction == null && herpesStrength > 0f)
				{
					client.Character.CharacterHealth.ApplyAffliction(null, new Affliction(this.herpesAffliction, herpesStrength), true, false, true);
					GameServer.Log(NetworkMember.ClientLogName(client, null) + " has contracted space herpes due to low karma.", ServerLog.MessageType.Karma);
					NetworkMember networkMember = GameMain.NetworkMember;
					ushort lastClientListUpdateID = networkMember.LastClientListUpdateID;
					networkMember.LastClientListUpdateID = lastClientListUpdateID + 1;
				}
				else if (existingAffliction != null)
				{
					existingAffliction.Strength = herpesStrength;
					if (herpesStrength <= 0f)
					{
						client.Character.CharacterHealth.ReduceAfflictionOnAllLimbs(AfflictionPrefab.InvertControlsType, 100f, null, null);
					}
				}
				KarmaManager.ClientMemory clientMemory = this.GetClientMemory(client);
				Identifier? identifier;
				if (clientMemory.WireDisconnectTime.Count > this.AllowedWireDisconnectionsPerMinute)
				{
					clientMemory.WireDisconnectTime.RemoveRange(0, clientMemory.WireDisconnectTime.Count - this.AllowedWireDisconnectionsPerMinute);
					if (clientMemory.WireDisconnectTime.All((Pair<Wire, float> w) => Timing.TotalTime - (double)w.Second < 60.0))
					{
						float karmaDecrease = -this.WireDisconnectionKarmaDecrease;
						CharacterInfo info = client.Character.Info;
						Identifier? identifier2;
						if (info == null)
						{
							identifier = null;
							identifier2 = identifier;
						}
						else
						{
							identifier2 = new Identifier?(info.Job.Prefab.Identifier);
						}
						identifier = identifier2;
						if (identifier == "engineer")
						{
							karmaDecrease *= 0.5f;
						}
						this.AdjustKarma(client.Character, karmaDecrease, "Disconnected excessive number of wires");
					}
				}
				Character character = client.Character;
				Identifier? identifier3;
				if (character == null)
				{
					identifier = null;
					identifier3 = identifier;
				}
				else
				{
					CharacterInfo info2 = character.Info;
					if (info2 == null)
					{
						identifier = null;
						identifier3 = identifier;
					}
					else
					{
						identifier3 = new Identifier?(info2.Job.Prefab.Identifier);
					}
				}
				identifier = identifier3;
				if (identifier == "captain" && client.Character.SelectedItem != null && client.Character.SelectedItem.GetComponent<Steering>() != null)
				{
					this.AdjustKarma(client.Character, this.SteerSubKarmaIncrease * deltaTime, "Steering the sub");
				}
			}
			if (client.Karma < this.KickBanThreshold && client.Connection != GameMain.Server.OwnerConnection)
			{
				if (this.TestMode)
				{
					client.Karma = 50f;
					GameMain.Server.SendDirectChatMessage("BANNED! (not really because karma test mode is enabled)", client, ChatMessageType.Server);
					return;
				}
				this.bannedClients.Add(client);
			}
		}

		// Token: 0x06000ADA RID: 2778 RVA: 0x0006ACFC File Offset: 0x00068EFC
		public void OnRoundEnded()
		{
			if (this.ResetKarmaBetweenRounds)
			{
				this.clientMemories.Clear();
				foreach (Client client in GameMain.Server.ConnectedClients)
				{
					client.Karma = Math.Max(50f, client.Karma);
				}
			}
		}

		// Token: 0x06000ADB RID: 2779 RVA: 0x0006AD70 File Offset: 0x00068F70
		public void OnClientDisconnected(Client client)
		{
			this.clientMemories.Remove(client);
		}

		// Token: 0x06000ADC RID: 2780 RVA: 0x0006AD80 File Offset: 0x00068F80
		public void OnBallastFloraDamaged(Character character, float damage)
		{
			if (character == null)
			{
				return;
			}
			float karmaChange = damage * this.BallastFloraKarmaIncrease;
			this.AdjustKarma(character, karmaChange, "Damaged ballast flora");
		}

		// Token: 0x06000ADD RID: 2781 RVA: 0x0006ADA8 File Offset: 0x00068FA8
		public void OnItemTakenFromPlayer(CharacterInventory inventory, Client yoinker, Item item)
		{
			Client targetClient = GameMain.Server.ConnectedClients.Find((Client c) => c.Character == inventory.Owner);
			Character thiefCharacter = (yoinker != null) ? yoinker.Character : null;
			Character targetCharacter = inventory.Owner as Character;
			if (yoinker == null || item == null || thiefCharacter == null || targetCharacter == null || thiefCharacter == targetCharacter)
			{
				return;
			}
			if (thiefCharacter.TeamID != targetCharacter.TeamID)
			{
				return;
			}
			if (targetClient == null && (!this.DangerousItemStealBots || targetCharacter.AIController == null))
			{
				return;
			}
			if (targetCharacter.IsDead || targetCharacter.Removed || (targetCharacter.Stun <= 0f && !targetCharacter.IsUnconscious && !targetCharacter.LockHands))
			{
				return;
			}
			if (GameMain.Server.TraitorManager != null && (GameMain.Server.TraitorManager.IsTraitor(targetCharacter) || GameMain.Server.TraitorManager.IsTraitor(thiefCharacter)))
			{
				return;
			}
			Item foundItem = null;
			if (KarmaManager.<OnItemTakenFromPlayer>g__IsValid|11_1(item))
			{
				foundItem = item;
			}
			else
			{
				foreach (Item containedItem in item.ContainedItems)
				{
					if (KarmaManager.<OnItemTakenFromPlayer>g__IsValid|11_1(containedItem))
					{
						foundItem = containedItem;
						break;
					}
				}
			}
			if (foundItem == null)
			{
				return;
			}
			bool isIdCard = foundItem.GetComponent<IdCard>() != null;
			if (isIdCard)
			{
				string name = string.Empty;
				foreach (string tag in foundItem.Tags.Split(',', StringSplitOptions.None))
				{
					string[] split = tag.Split(':', StringSplitOptions.None);
					string key = (split.Length != 0) ? split[0] : string.Empty;
					string value = (split.Length > 1) ? split[1] : string.Empty;
					if (key == "name")
					{
						name = value;
					}
				}
				if (name == null || name == thiefCharacter.Name)
				{
					return;
				}
			}
			if (MathUtils.NearlyEqual(this.DangerousItemStealKarmaDecrease, 0f, 0.0001f))
			{
				return;
			}
			float upper = this.DangerousItemStealKarmaDecrease + 10f;
			float lower = this.DangerousItemStealKarmaDecrease - 10f;
			if (lower < 0f)
			{
				upper += Math.Abs(lower);
				lower = 0f;
			}
			float targetKarma = (targetClient != null) ? targetClient.Karma : 50f;
			float karmaDifference = Math.Clamp((targetKarma - yoinker.Karma) / 50f, -1f, 1f);
			float karmaDecrease = lower + (karmaDifference - -1f) * (upper - lower) / 2f;
			CharacterInfo characterInfo = yoinker.CharacterInfo;
			JobPrefab jobPrefab;
			if (characterInfo == null)
			{
				jobPrefab = null;
			}
			else
			{
				Job job = characterInfo.Job;
				jobPrefab = ((job != null) ? job.Prefab : null);
			}
			JobPrefab clientJob = jobPrefab;
			if (clientJob != null && clientJob.Identifier == "securityofficer" && KarmaManager.<OnItemTakenFromPlayer>g__IsWeapon|11_2(foundItem))
			{
				karmaDecrease *= 0.5f;
			}
			this.AdjustKarma(thiefCharacter, -karmaDecrease, "Stolen dangerous item");
		}

		// Token: 0x06000ADE RID: 2782 RVA: 0x0006B084 File Offset: 0x00069284
		public void OnCharacterHealthChanged(Character target, Character attacker, float damage, float stun, IEnumerable<Affliction> appliedAfflictions = null)
		{
			if (target == null || attacker == null)
			{
				return;
			}
			if (target == attacker)
			{
				return;
			}
			if (target.IsDead || target.Removed)
			{
				return;
			}
			bool isEnemy = target.AIController is EnemyAIController || target.TeamID != attacker.TeamID;
			if (GameMain.Server.TraitorManager != null)
			{
				if (GameMain.Server.TraitorManager.IsTraitor(target))
				{
					isEnemy = true;
				}
				if (GameMain.Server.TraitorManager.IsTraitor(attacker))
				{
					isEnemy = true;
				}
			}
			if (KarmaManager.<OnCharacterHealthChanged>g__IsHusk|12_0(attacker) != KarmaManager.<OnCharacterHealthChanged>g__IsHusk|12_0(target))
			{
				isEnemy = true;
			}
			if (appliedAfflictions != null)
			{
				foreach (Affliction affliction in appliedAfflictions)
				{
					if (!MathUtils.NearlyEqual(affliction.Prefab.KarmaChangeOnApplied, 0f, 0.0001f))
					{
						damage -= affliction.Prefab.KarmaChangeOnApplied * affliction.Strength;
					}
				}
			}
			Client targetClient = GameMain.Server.ConnectedClients.Find((Client c) => c.Character == target);
			if (damage > 0f && targetClient != null)
			{
				KarmaManager.ClientMemory targetMemory = this.GetClientMemory(targetClient);
				targetMemory.LastAttackTime[attacker] = Timing.TotalTime;
			}
			Client attackerClient = GameMain.Server.ConnectedClients.Find((Client c) => c.Character == attacker);
			KarmaManager.ClientMemory attackerMemory = this.GetClientMemory(attackerClient);
			if (attackerMemory != null && attackerMemory.LastAttackTime.ContainsKey(target) && attackerMemory.LastAttackTime[target] > Timing.TotalTime - (double)this.AllowedRetaliationTime)
			{
				damage = Math.Min(damage, 0f);
				stun = 0f;
			}
			if (target.HasEquippedItem("clownmask".ToIdentifier(), true, null) && target.HasEquippedItem("clowncostume".ToIdentifier(), true, null))
			{
				damage *= 0.5f;
				stun *= 0.5f;
			}
			if (damage > 0f && target.IsKeyDown(InputType.Aim))
			{
				if (target.HeldItems.Any((Item it) => it.GetComponent<MeleeWeapon>() != null || it.GetComponent<RangedWeapon>() != null))
				{
					damage *= 0.5f;
					stun *= 0.5f;
				}
			}
			if (damage > 0f && targetClient != null)
			{
				damage *= MathUtils.InverseLerp(0f, 50f, targetClient.Karma);
			}
			if (isEnemy)
			{
				if (damage > 0f)
				{
					float karmaIncrease = damage * this.DamageEnemyKarmaIncrease;
					Character attacker2 = attacker;
					Identifier? identifier;
					Identifier? identifier2;
					if (attacker2 == null)
					{
						identifier = null;
						identifier2 = identifier;
					}
					else
					{
						CharacterInfo info = attacker2.Info;
						if (info == null)
						{
							identifier = null;
							identifier2 = identifier;
						}
						else
						{
							identifier2 = new Identifier?(info.Job.Prefab.Identifier);
						}
					}
					identifier = identifier2;
					if (identifier == "securityofficer")
					{
						karmaIncrease *= 2f;
					}
					this.AdjustKarma(attacker, karmaIncrease, "Damaged enemy");
					return;
				}
			}
			else
			{
				if (stun > 0f && attackerMemory != null)
				{
					attackerMemory.StunsInPastMinute.Add(new KarmaManager.ClientMemory.TimeAmount
					{
						Time = Timing.TotalTime,
						Amount = stun
					});
					if (attackerMemory.StunsInPastMinute.Count > 1)
					{
						float avgStunsInflicted = attackerMemory.StunsInPastMinute[0].Amount / (float)(attackerMemory.StunsInPastMinute[1].Time - attackerMemory.StunsInPastMinute[0].Time);
						for (int i = 1; i < attackerMemory.StunsInPastMinute.Count; i++)
						{
							avgStunsInflicted += attackerMemory.StunsInPastMinute[i].Amount / (float)(attackerMemory.StunsInPastMinute[i].Time - attackerMemory.StunsInPastMinute[i - 1].Time);
						}
						if (avgStunsInflicted > this.StunFriendlyKarmaDecreaseThreshold || attackerMemory.StunKarmaDecreaseMultiplier > 1f)
						{
							this.AdjustKarma(attacker, -this.StunFriendlyKarmaDecrease * attackerMemory.StunKarmaDecreaseMultiplier, "Stunned friendly");
							attackerMemory.StunKarmaDecreaseMultiplier *= 2f;
						}
					}
				}
				if (damage > 0f)
				{
					this.AdjustKarma(attacker, -damage * this.DamageFriendlyKarmaDecrease, "Damaged friendly");
					return;
				}
				float karmaIncrease2 = -damage * this.HealFriendlyKarmaIncrease;
				Character attacker3 = attacker;
				Identifier? identifier;
				Identifier? identifier3;
				if (attacker3 == null)
				{
					identifier = null;
					identifier3 = identifier;
				}
				else
				{
					CharacterInfo info2 = attacker3.Info;
					if (info2 == null)
					{
						identifier = null;
						identifier3 = identifier;
					}
					else
					{
						identifier3 = new Identifier?(info2.Job.Prefab.Identifier);
					}
				}
				identifier = identifier3;
				if (identifier == "medicaldoctor")
				{
					karmaIncrease2 *= 2f;
				}
				this.AdjustKarma(attacker, karmaIncrease2, "Healed friendly");
			}
		}

		// Token: 0x06000ADF RID: 2783 RVA: 0x0006B5C4 File Offset: 0x000697C4
		public void OnStructureHealthChanged(Structure structure, Character attacker, float damageAmount)
		{
			if (attacker == null)
			{
				return;
			}
			if (structure.Submarine == null || structure.Submarine.TeamID != attacker.TeamID)
			{
				return;
			}
			if (damageAmount <= 0f)
			{
				float karmaIncrease = -damageAmount * this.StructureRepairKarmaIncrease;
				CharacterInfo info = attacker.Info;
				Identifier? identifier;
				Identifier? identifier2;
				if (info == null)
				{
					identifier = null;
					identifier2 = identifier;
				}
				else
				{
					identifier2 = new Identifier?(info.Job.Prefab.Identifier);
				}
				identifier = identifier2;
				if (identifier == "mechanic")
				{
					karmaIncrease *= 2f;
				}
				this.AdjustKarma(attacker, karmaIncrease, "Repaired structures");
				return;
			}
			if (this.StructureDamageKarmaDecrease <= 0f)
			{
				return;
			}
			if (GameMain.Server.TraitorManager != null && GameMain.Server.TraitorManager.IsTraitor(attacker))
			{
				return;
			}
			Client client = GameMain.Server.ConnectedClients.Find((Client c) => c.Character == attacker);
			if (client != null)
			{
				KarmaManager.ClientMemory clientMemory = this.GetClientMemory(client);
				if (clientMemory.StructureDamagePerSecond + damageAmount >= this.MaxStructureDamageKarmaDecreasePerSecond / this.StructureDamageKarmaDecrease)
				{
					damageAmount -= clientMemory.StructureDamagePerSecond + damageAmount - this.MaxStructureDamageKarmaDecreasePerSecond / this.StructureDamageKarmaDecrease;
					if (damageAmount <= 0f)
					{
						return;
					}
				}
				clientMemory.StructureDamageAccumulator += damageAmount;
			}
			this.AdjustKarma(attacker, -damageAmount * this.StructureDamageKarmaDecrease, "Damaged structures");
		}

		// Token: 0x06000AE0 RID: 2784 RVA: 0x0006B734 File Offset: 0x00069934
		public void OnItemRepaired(Character character, Repairable repairable, float repairAmount)
		{
			float karmaIncrease = repairAmount * this.ItemRepairKarmaIncrease;
			if (repairable.HasRequiredSkills(character))
			{
				karmaIncrease *= 2f;
			}
			this.AdjustKarma(character, karmaIncrease, "Repaired item");
		}

		// Token: 0x06000AE1 RID: 2785 RVA: 0x0006B768 File Offset: 0x00069968
		public void OnReactorOverHeating(Item reactor, Character character, float deltaTime)
		{
			if (((reactor != null) ? reactor.Submarine : null) == null || character == null)
			{
				return;
			}
			if (reactor.Submarine.TeamID == CharacterTeamType.FriendlyNPC || reactor.Submarine.TeamID == character.TeamID)
			{
				this.AdjustKarma(character, -this.ReactorOverheatKarmaDecrease * deltaTime, "Caused reactor to overheat");
			}
		}

		// Token: 0x06000AE2 RID: 2786 RVA: 0x0006B7C0 File Offset: 0x000699C0
		public void OnReactorMeltdown(Item reactor, Character character)
		{
			if (((reactor != null) ? reactor.Submarine : null) == null || character == null)
			{
				return;
			}
			if (reactor.Submarine.TeamID == CharacterTeamType.FriendlyNPC || reactor.Submarine.TeamID == character.TeamID)
			{
				this.AdjustKarma(character, -this.ReactorMeltdownKarmaDecrease, "Caused a reactor meltdown");
			}
		}

		// Token: 0x06000AE3 RID: 2787 RVA: 0x0006B813 File Offset: 0x00069A13
		public void OnExtinguishingFire(Character character, float deltaTime)
		{
			this.AdjustKarma(character, this.ExtinguishFireKarmaIncrease * deltaTime, "Extinguished a fire");
		}

		// Token: 0x06000AE4 RID: 2788 RVA: 0x0006B82C File Offset: 0x00069A2C
		public void OnWireDisconnected(Character character, Wire wire)
		{
			if (character == null || wire == null)
			{
				return;
			}
			Client client = GameMain.Server.ConnectedClients.Find((Client c) => c.Character == character);
			if (client == null)
			{
				return;
			}
			if (!this.clientMemories.ContainsKey(client))
			{
				this.clientMemories[client] = new KarmaManager.ClientMemory();
			}
			this.clientMemories[client].WireDisconnectTime.RemoveAll((Pair<Wire, float> w) => w.First == wire);
			this.clientMemories[client].WireDisconnectTime.Add(new Pair<Wire, float>(wire, (float)Timing.TotalTime));
		}

		// Token: 0x06000AE5 RID: 2789 RVA: 0x0006B8E7 File Offset: 0x00069AE7
		private KarmaManager.ClientMemory GetClientMemory(Client client)
		{
			if (client == null)
			{
				return null;
			}
			if (!this.clientMemories.ContainsKey(client))
			{
				this.clientMemories[client] = new KarmaManager.ClientMemory
				{
					PreviousNotifiedKarma = client.Karma
				};
			}
			return this.clientMemories[client];
		}

		// Token: 0x06000AE6 RID: 2790 RVA: 0x0006B925 File Offset: 0x00069B25
		public void OnSpamFilterTriggered(Client client)
		{
			if (client != null)
			{
				client.Karma -= this.SpamFilterKarmaDecrease;
				this.SendKarmaNotifications(client, "Triggered the spam filter");
			}
		}

		// Token: 0x06000AE7 RID: 2791 RVA: 0x0006B94C File Offset: 0x00069B4C
		public void OnItemContained(Item containedItem, Item container, Character character)
		{
			if (containedItem == null || container == null || character == null || character.IsTraitor)
			{
				return;
			}
			if (container.Prefab.Identifier == Tags.WeldingFuel && containedItem.HasTag(Tags.OxygenSource))
			{
				Client client = GameMain.Server.ConnectedClients.Find((Client c) => c.Character == character);
				if (client == null)
				{
					return;
				}
				float amount = -this.DangerousItemContainKarmaDecrease;
				KarmaManager.ClientMemory memory = this.GetClientMemory(client);
				if (this.IsDangerousItemContainKarmaDecreaseIncremental)
				{
					amount *= (float)memory.DangerousItemsContained;
				}
				amount = Math.Max(amount, -this.MaxDangerousItemContainKarmaDecrease);
				this.AdjustKarma(character, amount, "Put an oxygen tank inside a welding tool");
				this.clientMemories[client].DangerousItemsContained = memory.DangerousItemsContained + 1;
			}
		}

		// Token: 0x06000AE8 RID: 2792 RVA: 0x0006BA24 File Offset: 0x00069C24
		private void AdjustKarma(Character target, float amount, string debugKarmaChangeReason = "")
		{
			if (target == null)
			{
				return;
			}
			Client client = GameMain.Server.ConnectedClients.Find((Client c) => c.Character == target);
			if (client == null)
			{
				return;
			}
			if (target.HasEquippedItem("clownmask".ToIdentifier(), true, null) && target.HasEquippedItem("clowncostume".ToIdentifier(), true, null))
			{
				amount *= 0.5f;
			}
			client.Karma += amount;
			if (amount < 0f)
			{
				Character character = client.Character;
				float? herpesStrength = (character != null) ? new float?(character.CharacterHealth.GetAfflictionStrengthByType(AfflictionPrefab.SpaceHerpesType, true)) : null;
				KarmaManager.ClientMemory clientMemory = this.GetClientMemory(client);
				clientMemory.KarmaDecreasesInPastMinute.RemoveAll((KarmaManager.ClientMemory.TimeAmount ta) => ta.Time + 60.0 < Timing.TotalTime);
				float aggregate = (from ta in clientMemory.KarmaDecreasesInPastMinute
				select ta.Amount).DefaultIfEmpty<float>().Aggregate((float a, float b) => a + b);
				clientMemory.KarmaDecreasesInPastMinute.Add(new KarmaManager.ClientMemory.TimeAmount
				{
					Time = Timing.TotalTime,
					Amount = -amount
				});
				if (herpesStrength != null)
				{
					float? num = herpesStrength;
					float num2 = 0f;
					if ((num.GetValueOrDefault() <= num2 & num != null) && aggregate - amount > 25f && aggregate <= 25f)
					{
						GameServer.Log(NetworkMember.ClientLogName(client, null) + " has lost more than 25 karma in the past minute.", ServerLog.MessageType.Karma);
					}
				}
			}
			if (this.TestMode)
			{
				this.SendKarmaNotifications(client, debugKarmaChangeReason);
			}
		}

		// Token: 0x170002E5 RID: 741
		// (get) Token: 0x06000AE9 RID: 2793 RVA: 0x0006BC14 File Offset: 0x00069E14
		public string Name
		{
			get
			{
				return "KarmaManager";
			}
		}

		// Token: 0x170002E6 RID: 742
		// (get) Token: 0x06000AEA RID: 2794 RVA: 0x0006BC1B File Offset: 0x00069E1B
		// (set) Token: 0x06000AEB RID: 2795 RVA: 0x0006BC23 File Offset: 0x00069E23
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

		// Token: 0x170002E7 RID: 743
		// (get) Token: 0x06000AEC RID: 2796 RVA: 0x0006BC2C File Offset: 0x00069E2C
		// (set) Token: 0x06000AED RID: 2797 RVA: 0x0006BC34 File Offset: 0x00069E34
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool ResetKarmaBetweenRounds { get; set; }

		// Token: 0x170002E8 RID: 744
		// (get) Token: 0x06000AEE RID: 2798 RVA: 0x0006BC3D File Offset: 0x00069E3D
		// (set) Token: 0x06000AEF RID: 2799 RVA: 0x0006BC45 File Offset: 0x00069E45
		[Serialize(0.1f, IsPropertySaveable.Yes, "", "", false)]
		public float KarmaDecay { get; set; }

		// Token: 0x170002E9 RID: 745
		// (get) Token: 0x06000AF0 RID: 2800 RVA: 0x0006BC4E File Offset: 0x00069E4E
		// (set) Token: 0x06000AF1 RID: 2801 RVA: 0x0006BC56 File Offset: 0x00069E56
		[Serialize(50f, IsPropertySaveable.Yes, "", "", false)]
		public float KarmaDecayThreshold { get; set; }

		// Token: 0x170002EA RID: 746
		// (get) Token: 0x06000AF2 RID: 2802 RVA: 0x0006BC5F File Offset: 0x00069E5F
		// (set) Token: 0x06000AF3 RID: 2803 RVA: 0x0006BC67 File Offset: 0x00069E67
		[Serialize(0.15f, IsPropertySaveable.Yes, "", "", false)]
		public float KarmaIncrease { get; set; }

		// Token: 0x170002EB RID: 747
		// (get) Token: 0x06000AF4 RID: 2804 RVA: 0x0006BC70 File Offset: 0x00069E70
		// (set) Token: 0x06000AF5 RID: 2805 RVA: 0x0006BC78 File Offset: 0x00069E78
		[Serialize(50f, IsPropertySaveable.Yes, "", "", false)]
		public float KarmaIncreaseThreshold { get; set; }

		// Token: 0x170002EC RID: 748
		// (get) Token: 0x06000AF6 RID: 2806 RVA: 0x0006BC81 File Offset: 0x00069E81
		// (set) Token: 0x06000AF7 RID: 2807 RVA: 0x0006BC89 File Offset: 0x00069E89
		[Serialize(0.05f, IsPropertySaveable.Yes, "", "", false)]
		public float StructureRepairKarmaIncrease { get; set; }

		// Token: 0x170002ED RID: 749
		// (get) Token: 0x06000AF8 RID: 2808 RVA: 0x0006BC92 File Offset: 0x00069E92
		// (set) Token: 0x06000AF9 RID: 2809 RVA: 0x0006BC9A File Offset: 0x00069E9A
		[Serialize(0.1f, IsPropertySaveable.Yes, "", "", false)]
		public float StructureDamageKarmaDecrease { get; set; }

		// Token: 0x170002EE RID: 750
		// (get) Token: 0x06000AFA RID: 2810 RVA: 0x0006BCA3 File Offset: 0x00069EA3
		// (set) Token: 0x06000AFB RID: 2811 RVA: 0x0006BCAB File Offset: 0x00069EAB
		[Serialize(15f, IsPropertySaveable.Yes, "", "", false)]
		public float MaxStructureDamageKarmaDecreasePerSecond { get; set; }

		// Token: 0x170002EF RID: 751
		// (get) Token: 0x06000AFC RID: 2812 RVA: 0x0006BCB4 File Offset: 0x00069EB4
		// (set) Token: 0x06000AFD RID: 2813 RVA: 0x0006BCBC File Offset: 0x00069EBC
		[Serialize(0.03f, IsPropertySaveable.Yes, "", "", false)]
		public float ItemRepairKarmaIncrease { get; set; }

		// Token: 0x170002F0 RID: 752
		// (get) Token: 0x06000AFE RID: 2814 RVA: 0x0006BCC5 File Offset: 0x00069EC5
		// (set) Token: 0x06000AFF RID: 2815 RVA: 0x0006BCCD File Offset: 0x00069ECD
		[Serialize(0.5f, IsPropertySaveable.Yes, "", "", false)]
		public float ReactorOverheatKarmaDecrease { get; set; }

		// Token: 0x170002F1 RID: 753
		// (get) Token: 0x06000B00 RID: 2816 RVA: 0x0006BCD6 File Offset: 0x00069ED6
		// (set) Token: 0x06000B01 RID: 2817 RVA: 0x0006BCDE File Offset: 0x00069EDE
		[Serialize(30f, IsPropertySaveable.Yes, "", "", false)]
		public float ReactorMeltdownKarmaDecrease { get; set; }

		// Token: 0x170002F2 RID: 754
		// (get) Token: 0x06000B02 RID: 2818 RVA: 0x0006BCE7 File Offset: 0x00069EE7
		// (set) Token: 0x06000B03 RID: 2819 RVA: 0x0006BCEF File Offset: 0x00069EEF
		[Serialize(0.1f, IsPropertySaveable.Yes, "", "", false)]
		public float DamageEnemyKarmaIncrease { get; set; }

		// Token: 0x170002F3 RID: 755
		// (get) Token: 0x06000B04 RID: 2820 RVA: 0x0006BCF8 File Offset: 0x00069EF8
		// (set) Token: 0x06000B05 RID: 2821 RVA: 0x0006BD00 File Offset: 0x00069F00
		[Serialize(0.2f, IsPropertySaveable.Yes, "", "", false)]
		public float HealFriendlyKarmaIncrease { get; set; }

		// Token: 0x170002F4 RID: 756
		// (get) Token: 0x06000B06 RID: 2822 RVA: 0x0006BD09 File Offset: 0x00069F09
		// (set) Token: 0x06000B07 RID: 2823 RVA: 0x0006BD11 File Offset: 0x00069F11
		[Serialize(0.25f, IsPropertySaveable.Yes, "", "", false)]
		public float DamageFriendlyKarmaDecrease { get; set; }

		// Token: 0x170002F5 RID: 757
		// (get) Token: 0x06000B08 RID: 2824 RVA: 0x0006BD1A File Offset: 0x00069F1A
		// (set) Token: 0x06000B09 RID: 2825 RVA: 0x0006BD22 File Offset: 0x00069F22
		[Serialize(0.25f, IsPropertySaveable.Yes, "", "", false)]
		public float StunFriendlyKarmaDecrease { get; set; }

		// Token: 0x170002F6 RID: 758
		// (get) Token: 0x06000B0A RID: 2826 RVA: 0x0006BD2B File Offset: 0x00069F2B
		// (set) Token: 0x06000B0B RID: 2827 RVA: 0x0006BD33 File Offset: 0x00069F33
		[Serialize(0.3f, IsPropertySaveable.Yes, "", "", false)]
		public float StunFriendlyKarmaDecreaseThreshold { get; set; }

		// Token: 0x170002F7 RID: 759
		// (get) Token: 0x06000B0C RID: 2828 RVA: 0x0006BD3C File Offset: 0x00069F3C
		// (set) Token: 0x06000B0D RID: 2829 RVA: 0x0006BD44 File Offset: 0x00069F44
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		public float ExtinguishFireKarmaIncrease { get; set; }

		// Token: 0x170002F8 RID: 760
		// (get) Token: 0x06000B0E RID: 2830 RVA: 0x0006BD4D File Offset: 0x00069F4D
		// (set) Token: 0x06000B0F RID: 2831 RVA: 0x0006BD55 File Offset: 0x00069F55
		[Serialize(15f, IsPropertySaveable.Yes, "", "", false)]
		public float DangerousItemStealKarmaDecrease { get; set; }

		// Token: 0x170002F9 RID: 761
		// (get) Token: 0x06000B10 RID: 2832 RVA: 0x0006BD5E File Offset: 0x00069F5E
		// (set) Token: 0x06000B11 RID: 2833 RVA: 0x0006BD66 File Offset: 0x00069F66
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool DangerousItemStealBots { get; set; }

		// Token: 0x170002FA RID: 762
		// (get) Token: 0x06000B12 RID: 2834 RVA: 0x0006BD6F File Offset: 0x00069F6F
		// (set) Token: 0x06000B13 RID: 2835 RVA: 0x0006BD77 File Offset: 0x00069F77
		[Serialize(0.05f, IsPropertySaveable.Yes, "", "", false)]
		public float BallastFloraKarmaIncrease { get; set; }

		// Token: 0x170002FB RID: 763
		// (get) Token: 0x06000B14 RID: 2836 RVA: 0x0006BD80 File Offset: 0x00069F80
		// (set) Token: 0x06000B15 RID: 2837 RVA: 0x0006BD88 File Offset: 0x00069F88
		[Serialize(5, IsPropertySaveable.Yes, "", "", false)]
		public int AllowedWireDisconnectionsPerMinute
		{
			get
			{
				return this.allowedWireDisconnectionsPerMinute;
			}
			set
			{
				this.allowedWireDisconnectionsPerMinute = Math.Max(0, value);
			}
		}

		// Token: 0x170002FC RID: 764
		// (get) Token: 0x06000B16 RID: 2838 RVA: 0x0006BD97 File Offset: 0x00069F97
		// (set) Token: 0x06000B17 RID: 2839 RVA: 0x0006BD9F File Offset: 0x00069F9F
		[Serialize(6f, IsPropertySaveable.Yes, "", "", false)]
		public float WireDisconnectionKarmaDecrease { get; set; }

		// Token: 0x170002FD RID: 765
		// (get) Token: 0x06000B18 RID: 2840 RVA: 0x0006BDA8 File Offset: 0x00069FA8
		// (set) Token: 0x06000B19 RID: 2841 RVA: 0x0006BDB0 File Offset: 0x00069FB0
		[Serialize(0.15f, IsPropertySaveable.Yes, "", "", false)]
		public float SteerSubKarmaIncrease { get; set; }

		// Token: 0x170002FE RID: 766
		// (get) Token: 0x06000B1A RID: 2842 RVA: 0x0006BDB9 File Offset: 0x00069FB9
		// (set) Token: 0x06000B1B RID: 2843 RVA: 0x0006BDC1 File Offset: 0x00069FC1
		[Serialize(15f, IsPropertySaveable.Yes, "", "", false)]
		public float SpamFilterKarmaDecrease { get; set; }

		// Token: 0x170002FF RID: 767
		// (get) Token: 0x06000B1C RID: 2844 RVA: 0x0006BDCA File Offset: 0x00069FCA
		// (set) Token: 0x06000B1D RID: 2845 RVA: 0x0006BDD2 File Offset: 0x00069FD2
		[Serialize(40f, IsPropertySaveable.Yes, "", "", false)]
		public float HerpesThreshold { get; set; }

		// Token: 0x17000300 RID: 768
		// (get) Token: 0x06000B1E RID: 2846 RVA: 0x0006BDDB File Offset: 0x00069FDB
		// (set) Token: 0x06000B1F RID: 2847 RVA: 0x0006BDE3 File Offset: 0x00069FE3
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		public float KickBanThreshold { get; set; }

		// Token: 0x17000301 RID: 769
		// (get) Token: 0x06000B20 RID: 2848 RVA: 0x0006BDEC File Offset: 0x00069FEC
		// (set) Token: 0x06000B21 RID: 2849 RVA: 0x0006BDF4 File Offset: 0x00069FF4
		[Serialize(0, IsPropertySaveable.Yes, "", "", false)]
		public int KicksBeforeBan { get; set; }

		// Token: 0x17000302 RID: 770
		// (get) Token: 0x06000B22 RID: 2850 RVA: 0x0006BDFD File Offset: 0x00069FFD
		// (set) Token: 0x06000B23 RID: 2851 RVA: 0x0006BE05 File Offset: 0x0006A005
		[Serialize(10f, IsPropertySaveable.Yes, "", "", false)]
		public float KarmaNotificationInterval { get; set; }

		// Token: 0x17000303 RID: 771
		// (get) Token: 0x06000B24 RID: 2852 RVA: 0x0006BE0E File Offset: 0x0006A00E
		// (set) Token: 0x06000B25 RID: 2853 RVA: 0x0006BE16 File Offset: 0x0006A016
		[Serialize(120f, IsPropertySaveable.Yes, "", "", false)]
		public float AllowedRetaliationTime { get; set; }

		// Token: 0x17000304 RID: 772
		// (get) Token: 0x06000B26 RID: 2854 RVA: 0x0006BE1F File Offset: 0x0006A01F
		// (set) Token: 0x06000B27 RID: 2855 RVA: 0x0006BE27 File Offset: 0x0006A027
		[Serialize(5f, IsPropertySaveable.Yes, "", "", false)]
		public float DangerousItemContainKarmaDecrease { get; set; }

		// Token: 0x17000305 RID: 773
		// (get) Token: 0x06000B28 RID: 2856 RVA: 0x0006BE30 File Offset: 0x0006A030
		// (set) Token: 0x06000B29 RID: 2857 RVA: 0x0006BE38 File Offset: 0x0006A038
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool IsDangerousItemContainKarmaDecreaseIncremental { get; set; }

		// Token: 0x17000306 RID: 774
		// (get) Token: 0x06000B2A RID: 2858 RVA: 0x0006BE41 File Offset: 0x0006A041
		// (set) Token: 0x06000B2B RID: 2859 RVA: 0x0006BE49 File Offset: 0x0006A049
		[Serialize(30f, IsPropertySaveable.Yes, "", "", false)]
		public float MaxDangerousItemContainKarmaDecrease { get; set; }

		// Token: 0x06000B2C RID: 2860 RVA: 0x0006BE54 File Offset: 0x0006A054
		public KarmaManager()
		{
			XDocument doc = null;
			int maxLoadRetries = 4;
			for (int i = 0; i <= maxLoadRetries; i++)
			{
				try
				{
					doc = XMLExtensions.TryLoadXml(KarmaManager.ConfigFile);
					break;
				}
				catch (IOException)
				{
					if (i == maxLoadRetries)
					{
						break;
					}
					DebugConsole.NewMessage("Opening karma settings file \"" + KarmaManager.ConfigFile + "\" failed, retrying in 250 ms...", null, false);
					Thread.Sleep(250);
				}
			}
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, (doc != null) ? doc.Root : null);
			if (((doc != null) ? doc.Root : null) != null)
			{
				this.Presets["custom"] = doc.Root;
				foreach (XElement subElement in doc.Root.Elements())
				{
					string presetName = subElement.GetAttributeString("name", "");
					this.Presets[presetName.ToLowerInvariant()] = subElement;
				}
				NetworkMember networkMember = GameMain.NetworkMember;
				string text;
				if (networkMember == null)
				{
					text = null;
				}
				else
				{
					ServerSettings serverSettings = networkMember.ServerSettings;
					text = ((serverSettings != null) ? serverSettings.KarmaPreset : null);
				}
				this.SelectPreset(text ?? "default");
			}
			this.herpesAffliction = AfflictionPrefab.List.FirstOrDefault((AfflictionPrefab ap) => ap.Identifier == "spaceherpes");
		}

		// Token: 0x06000B2D RID: 2861 RVA: 0x0006BFF0 File Offset: 0x0006A1F0
		public void SelectPreset(string presetName)
		{
			if (string.IsNullOrEmpty(presetName))
			{
				return;
			}
			presetName = presetName.ToLowerInvariant();
			if (this.Presets.ContainsKey(presetName))
			{
				SerializableProperty.DeserializeProperties(this, this.Presets[presetName]);
				return;
			}
			if (this.Presets.ContainsKey("custom"))
			{
				SerializableProperty.DeserializeProperties(this, this.Presets["custom"]);
			}
		}

		// Token: 0x06000B2E RID: 2862 RVA: 0x0006C059 File Offset: 0x0006A259
		public void SaveCustomPreset()
		{
			if (this.Presets.ContainsKey("custom"))
			{
				SerializableProperty.SerializeProperties(this, this.Presets["custom"], true, false);
			}
		}

		// Token: 0x06000B2F RID: 2863 RVA: 0x0006C088 File Offset: 0x0006A288
		public void Save()
		{
			XDocument doc = new XDocument(new object[]
			{
				new XElement(this.Name)
			});
			foreach (KeyValuePair<string, XElement> preset in this.Presets)
			{
				doc.Root.Add(preset.Value);
			}
			XmlWriterSettings settings = new XmlWriterSettings
			{
				Indent = true,
				NewLineOnAttributes = true
			};
			int maxLoadRetries = 4;
			for (int i = 0; i <= maxLoadRetries; i++)
			{
				try
				{
					using (XmlWriter writer = XmlWriter.Create(KarmaManager.ConfigFile, settings))
					{
						doc.SaveSafe(writer);
					}
					break;
				}
				catch (IOException)
				{
					if (i == maxLoadRetries)
					{
						throw;
					}
					DebugConsole.NewMessage("Saving karma settings file file \"" + KarmaManager.ConfigFile + "\" failed, retrying in 250 ms...", null, false);
					Thread.Sleep(250);
				}
			}
		}

		// Token: 0x06000B30 RID: 2864 RVA: 0x0006C1A4 File Offset: 0x0006A3A4
		// Note: this type is marked as 'beforefieldinit'.
		static KarmaManager()
		{
			ReadOnlySpan<char> str = "Data";
			char directorySeparatorChar = Path.DirectorySeparatorChar;
			KarmaManager.ConfigFile = str + new ReadOnlySpan<char>(ref directorySeparatorChar) + "karmasettings.xml";
		}

		// Token: 0x06000B32 RID: 2866 RVA: 0x0006C1F4 File Offset: 0x0006A3F4
		[CompilerGenerated]
		internal static bool <OnItemTakenFromPlayer>g__IsValid|11_1(Item item)
		{
			return item.GetComponent<IdCard>() != null || KarmaManager.<OnItemTakenFromPlayer>g__IsWeapon|11_2(item);
		}

		// Token: 0x06000B33 RID: 2867 RVA: 0x0006C206 File Offset: 0x0006A406
		[CompilerGenerated]
		internal static bool <OnItemTakenFromPlayer>g__IsWeapon|11_2(Item item)
		{
			return item.Components.Max((ItemComponent c) => c.CombatPriority) > 10f || item.HasTag(Tags.Weapon);
		}

		// Token: 0x06000B34 RID: 2868 RVA: 0x0006C246 File Offset: 0x0006A446
		[CompilerGenerated]
		internal static bool <OnCharacterHealthChanged>g__IsHusk|12_0(Character c)
		{
			return c.IsHusk || c.IsHuskInfected;
		}

		// Token: 0x040004B3 RID: 1203
		public bool TestMode;

		// Token: 0x040004B4 RID: 1204
		private readonly Dictionary<Client, KarmaManager.ClientMemory> clientMemories = new Dictionary<Client, KarmaManager.ClientMemory>();

		// Token: 0x040004B5 RID: 1205
		private readonly List<Client> bannedClients = new List<Client>();

		// Token: 0x040004B6 RID: 1206
		private DateTime perSecondUpdate;

		// Token: 0x040004B7 RID: 1207
		public static readonly string ConfigFile;

		// Token: 0x040004CD RID: 1229
		private int allowedWireDisconnectionsPerMinute;

		// Token: 0x040004D9 RID: 1241
		private readonly AfflictionPrefab herpesAffliction;

		// Token: 0x040004DA RID: 1242
		public Dictionary<string, XElement> Presets = new Dictionary<string, XElement>();

		// Token: 0x02000739 RID: 1849
		private class ClientMemory
		{
			// Token: 0x17001424 RID: 5156
			// (get) Token: 0x06005138 RID: 20792 RVA: 0x001E9016 File Offset: 0x001E7216
			// (set) Token: 0x06005139 RID: 20793 RVA: 0x001E9029 File Offset: 0x001E7229
			public float StructureDamagePerSecond
			{
				get
				{
					return Math.Max(this.StructureDamageAccumulator, this.structureDamagePerSecond);
				}
				set
				{
					this.structureDamagePerSecond = value;
				}
			}

			// Token: 0x17001425 RID: 5157
			// (get) Token: 0x0600513A RID: 20794 RVA: 0x001E9032 File Offset: 0x001E7232
			// (set) Token: 0x0600513B RID: 20795 RVA: 0x001E903A File Offset: 0x001E723A
			public Dictionary<Character, double> LastAttackTime { get; private set; } = new Dictionary<Character, double>();

			// Token: 0x17001426 RID: 5158
			// (get) Token: 0x0600513C RID: 20796 RVA: 0x001E9043 File Offset: 0x001E7243
			// (set) Token: 0x0600513D RID: 20797 RVA: 0x001E904B File Offset: 0x001E724B
			public int DangerousItemsContained { get; set; }

			// Token: 0x04002C48 RID: 11336
			public List<Pair<Wire, float>> WireDisconnectTime = new List<Pair<Wire, float>>();

			// Token: 0x04002C49 RID: 11337
			public List<KarmaManager.ClientMemory.TimeAmount> KarmaDecreasesInPastMinute = new List<KarmaManager.ClientMemory.TimeAmount>();

			// Token: 0x04002C4A RID: 11338
			public float PreviousNotifiedKarma;

			// Token: 0x04002C4B RID: 11339
			public double PreviousKarmaNotificationTime;

			// Token: 0x04002C4C RID: 11340
			public float StructureDamageAccumulator;

			// Token: 0x04002C4D RID: 11341
			private float structureDamagePerSecond;

			// Token: 0x04002C4E RID: 11342
			public List<KarmaManager.ClientMemory.TimeAmount> StunsInPastMinute = new List<KarmaManager.ClientMemory.TimeAmount>();

			// Token: 0x04002C4F RID: 11343
			public float StunKarmaDecreaseMultiplier;

			// Token: 0x02000E7B RID: 3707
			public struct TimeAmount
			{
				// Token: 0x04004278 RID: 17016
				public double Time;

				// Token: 0x04004279 RID: 17017
				public float Amount;
			}
		}
	}
}
