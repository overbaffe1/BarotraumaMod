using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x020001C6 RID: 454
	internal class AfflictionHusk : Affliction
	{
		// Token: 0x17000D0F RID: 3343
		// (get) Token: 0x060031D4 RID: 12756 RVA: 0x00205B74 File Offset: 0x00203D74
		// (set) Token: 0x060031D5 RID: 12757 RVA: 0x00205B7C File Offset: 0x00203D7C
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public override float Strength
		{
			get
			{
				return this._strength;
			}
			set
			{
				float previousValue = this._strength;
				float threshold = (this._strength > this.ActiveThreshold) ? (this.ActiveThreshold + 1f) : (this.DormantThreshold - 1f);
				float max = Math.Max(threshold, previousValue);
				this._strength = Math.Clamp(value, 0f, max);
				GameSession gameSession = GameMain.GameSession;
				this.stun = (gameSession == null || gameSession.IsRunning);
				if (previousValue > 0f && value <= 0f)
				{
					this.DeactivateHusk();
					this.highestStrength = 0f;
				}
				this.activeEffectDirty = true;
			}
		}

		// Token: 0x17000D10 RID: 3344
		// (get) Token: 0x060031D6 RID: 12758 RVA: 0x00205C12 File Offset: 0x00203E12
		// (set) Token: 0x060031D7 RID: 12759 RVA: 0x00205C1A File Offset: 0x00203E1A
		public AfflictionHusk.InfectionState State
		{
			get
			{
				return this.state;
			}
			private set
			{
				if (this.state == value)
				{
					return;
				}
				this.state = value;
			}
		}

		// Token: 0x17000D11 RID: 3345
		// (get) Token: 0x060031D8 RID: 12760 RVA: 0x00205C2D File Offset: 0x00203E2D
		private float DormantThreshold
		{
			get
			{
				return this.HuskPrefab.DormantThreshold;
			}
		}

		// Token: 0x17000D12 RID: 3346
		// (get) Token: 0x060031D9 RID: 12761 RVA: 0x00205C3A File Offset: 0x00203E3A
		private float ActiveThreshold
		{
			get
			{
				return this.HuskPrefab.ActiveThreshold;
			}
		}

		// Token: 0x17000D13 RID: 3347
		// (get) Token: 0x060031DA RID: 12762 RVA: 0x00205C47 File Offset: 0x00203E47
		private float TransitionThreshold
		{
			get
			{
				return this.HuskPrefab.TransitionThreshold;
			}
		}

		// Token: 0x17000D14 RID: 3348
		// (get) Token: 0x060031DB RID: 12763 RVA: 0x00205C54 File Offset: 0x00203E54
		private float TransformThresholdOnDeath
		{
			get
			{
				return this.HuskPrefab.TransformThresholdOnDeath;
			}
		}

		// Token: 0x060031DC RID: 12764 RVA: 0x00205C61 File Offset: 0x00203E61
		public AfflictionHusk(AfflictionPrefab prefab, float strength) : base(prefab, strength)
		{
			this.HuskPrefab = (prefab as AfflictionPrefabHusk);
			if (this.HuskPrefab == null)
			{
				DebugConsole.ThrowError("Error in husk affliction definition: the prefab is of wrong type!", null, prefab.ContentPackage, false, false);
			}
		}

		// Token: 0x060031DD RID: 12765 RVA: 0x00205C94 File Offset: 0x00203E94
		public override void Update(CharacterHealth characterHealth, Limb targetLimb, float deltaTime)
		{
			if (this.HuskPrefab == null)
			{
				return;
			}
			base.Update(characterHealth, targetLimb, deltaTime);
			this.highestStrength = Math.Max(this._strength, this.highestStrength);
			this.character = characterHealth.Character;
			if (this.character == null)
			{
				return;
			}
			this.UpdateMessages();
			if (!this.subscribedToDeathEvent)
			{
				Character character = this.character;
				character.OnDeath = (Character.OnDeathHandler)Delegate.Combine(character.OnDeath, new Character.OnDeathHandler(this.CharacterDead));
				this.subscribedToDeathEvent = true;
			}
			if (this.Strength < this.DormantThreshold)
			{
				this.DeactivateHusk();
				if (this.Strength > Math.Min(1f, this.DormantThreshold))
				{
					this.State = AfflictionHusk.InfectionState.Dormant;
					return;
				}
			}
			else
			{
				if (this.Strength < this.ActiveThreshold)
				{
					this.DeactivateHusk();
					AfflictionPrefabHusk afflictionPrefabHusk = this.Prefab as AfflictionPrefabHusk;
					if (afflictionPrefabHusk != null && afflictionPrefabHusk.CauseSpeechImpediment)
					{
						this.character.SpeechImpediment = 30f;
					}
					this.State = AfflictionHusk.InfectionState.Transition;
					return;
				}
				if (this.Strength < this.TransitionThreshold)
				{
					if (this.State != AfflictionHusk.InfectionState.Active && this.stun)
					{
						this.character.SetStun(Rand.Range(2f, 3f, Rand.RandSync.Unsynced), false, false);
					}
					AfflictionPrefabHusk afflictionPrefabHusk = this.Prefab as AfflictionPrefabHusk;
					if (afflictionPrefabHusk != null && afflictionPrefabHusk.CauseSpeechImpediment)
					{
						this.character.SpeechImpediment = 100f;
					}
					this.State = AfflictionHusk.InfectionState.Active;
					this.ActivateHusk();
					return;
				}
				this.State = AfflictionHusk.InfectionState.Final;
				this.ActivateHusk();
				this.ApplyDamage(deltaTime);
				this.character.SetStun(5f, false, false);
			}
		}

		// Token: 0x060031DE RID: 12766 RVA: 0x00205E2C File Offset: 0x0020402C
		private void UpdateMessages()
		{
			AfflictionPrefabHusk afflictionPrefabHusk = this.Prefab as AfflictionPrefabHusk;
			if (afflictionPrefabHusk != null && !afflictionPrefabHusk.SendMessages)
			{
				return;
			}
			if (this.prevDisplayedMessage != null && this.prevDisplayedMessage.Value == this.State)
			{
				return;
			}
			if (this.highestStrength > this.Strength)
			{
				return;
			}
			GameSession gameSession = GameMain.GameSession;
			bool? flag;
			if (gameSession == null)
			{
				flag = null;
			}
			else
			{
				CampaignMode campaign = gameSession.Campaign;
				flag = ((campaign != null) ? new bool?(campaign.Settings.ShowHuskWarning) : null);
			}
			bool? flag2 = flag;
			bool showHuskWarning = flag2.GetValueOrDefault(true);
			switch (this.State)
			{
			case AfflictionHusk.InfectionState.Dormant:
				if (this.Strength < this.DormantThreshold * 0.5f)
				{
					return;
				}
				if (showHuskWarning)
				{
					if (this.character == Character.Controlled)
					{
						GUI.AddMessage(TextManager.Get("HuskDormant"), GUIStyle.Red, null, true, null);
					}
					else if (this.character.IsBot)
					{
						Character character = this.character;
						string value = TextManager.Get("dialoghuskdormant").Value;
						float delay = Rand.Range(0.5f, 5f, Rand.RandSync.Unsynced);
						Identifier identifier = "huskdormant".ToIdentifier();
						character.Speak(value, null, delay, identifier, 0f);
					}
				}
				break;
			case AfflictionHusk.InfectionState.Transition:
				afflictionPrefabHusk = (this.Prefab as AfflictionPrefabHusk);
				if (afflictionPrefabHusk != null && afflictionPrefabHusk.CauseSpeechImpediment)
				{
					if (this.character == Character.Controlled)
					{
						GUI.AddMessage(TextManager.Get("HuskCantSpeak"), GUIStyle.Red, null, true, null);
					}
					else if (this.character.IsBot)
					{
						Character character2 = this.character;
						string value2 = TextManager.Get("dialoghuskcantspeak").Value;
						float delay = Rand.Range(0.5f, 5f, Rand.RandSync.Unsynced);
						Identifier identifier = "huskcantspeak".ToIdentifier();
						character2.Speak(value2, null, delay, identifier, 0f);
					}
				}
				break;
			case AfflictionHusk.InfectionState.Active:
				if (this.character == Character.Controlled && this.character.Params.UseHuskAppendage)
				{
					string tag = "HuskActivate";
					string varName = "[Attack]";
					GameSettings.Config.KeyMapping keyMap = GameSettings.CurrentConfig.KeyMap;
					GUI.AddMessage(TextManager.GetWithVariable(tag, varName, keyMap.KeyBindText(InputType.Attack), FormatCapitals.No), GUIStyle.Red, null, true, null);
				}
				break;
			}
			this.prevDisplayedMessage = new AfflictionHusk.InfectionState?(this.State);
		}

		// Token: 0x060031DF RID: 12767 RVA: 0x002060B8 File Offset: 0x002042B8
		private void ApplyDamage(float deltaTime)
		{
			if (this.damageCooldownTimer > 0f)
			{
				this.damageCooldownTimer -= deltaTime;
				return;
			}
			this.damageCooldownTimer = 0.1f;
			IEnumerable<Limb> limbs = this.character.AnimController.Limbs;
			Func<Limb, bool> predicate;
			if ((predicate = AfflictionHusk.<>O.<0>__IsValidLimb) == null)
			{
				predicate = (AfflictionHusk.<>O.<0>__IsValidLimb = new Func<Limb, bool>(AfflictionHusk.<ApplyDamage>g__IsValidLimb|28_0));
			}
			int limbCount = limbs.Count(predicate);
			foreach (Limb limb in this.character.AnimController.Limbs)
			{
				if (AfflictionHusk.<ApplyDamage>g__IsValidLimb|28_0(limb))
				{
					float random = Rand.Value(Rand.RandSync.Unsynced);
					if (random != 0f)
					{
						float dmg = random / (float)limbCount * 2f;
						this.character.LastDamageSource = null;
						IEnumerable<Affliction> afflictions = AfflictionPrefab.InternalDamage.Instantiate(dmg, null).ToEnumerable<Affliction>();
						float force = dmg * limb.Mass * 5f;
						this.character.DamageLimb(limb.WorldPosition, limb, afflictions, 0f, false, Rand.Vector(force, Rand.RandSync.Unsynced), null, 1f, true, 0f, false, true, false);
					}
				}
			}
			this.character.CharacterHealth.RecalculateVitality();
		}

		// Token: 0x060031E0 RID: 12768 RVA: 0x002061E4 File Offset: 0x002043E4
		public void ActivateHusk()
		{
			if (this.huskAppendage == null && this.character.Params.UseHuskAppendage)
			{
				AfflictionPrefabHusk huskAffliction = this.Prefab as AfflictionPrefabHusk;
				this.huskAppendage = AfflictionHusk.AttachHuskAppendage(this.character, huskAffliction, AfflictionHusk.GetHuskedSpeciesName(this.character.Params, huskAffliction), null, null);
			}
			AfflictionPrefabHusk afflictionPrefabHusk = this.Prefab as AfflictionPrefabHusk;
			if (afflictionPrefabHusk != null && !afflictionPrefabHusk.NeedsAir)
			{
				this.character.NeedsAir = false;
			}
			afflictionPrefabHusk = (this.Prefab as AfflictionPrefabHusk);
			if (afflictionPrefabHusk != null && afflictionPrefabHusk.CauseSpeechImpediment)
			{
				this.character.SpeechImpediment = 100f;
			}
		}

		// Token: 0x060031E1 RID: 12769 RVA: 0x00206288 File Offset: 0x00204488
		private void DeactivateHusk()
		{
			Character character = this.character;
			if (((character != null) ? character.AnimController : null) == null || this.character.Removed)
			{
				return;
			}
			AfflictionPrefabHusk afflictionPrefabHusk = this.Prefab as AfflictionPrefabHusk;
			if (afflictionPrefabHusk != null && !afflictionPrefabHusk.NeedsAir && !this.character.CharacterHealth.GetAllAfflictions().Any(delegate(Affliction a)
			{
				if (a != this)
				{
					AfflictionPrefabHusk afflictionPrefabHusk2 = a.Prefab as AfflictionPrefabHusk;
					return afflictionPrefabHusk2 != null && !afflictionPrefabHusk2.NeedsAir;
				}
				return false;
			}))
			{
				this.character.NeedsAir = this.character.Params.MainElement.GetAttributeBool("needsair", false);
			}
			if (this.huskAppendage != null)
			{
				this.huskAppendage.ForEach(delegate(Limb l)
				{
					this.character.AnimController.RemoveLimb(l);
				});
				this.huskAppendage = null;
			}
		}

		// Token: 0x060031E2 RID: 12770 RVA: 0x00206340 File Offset: 0x00204540
		public void UnsubscribeFromDeathEvent()
		{
			if (this.character == null || !this.subscribedToDeathEvent)
			{
				return;
			}
			Character character = this.character;
			character.OnDeath = (Character.OnDeathHandler)Delegate.Remove(character.OnDeath, new Character.OnDeathHandler(this.CharacterDead));
			this.subscribedToDeathEvent = false;
		}

		// Token: 0x060031E3 RID: 12771 RVA: 0x0020638C File Offset: 0x0020458C
		private void CharacterDead(Character character, CauseOfDeath causeOfDeath)
		{
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			if (this.Strength < this.TransformThresholdOnDeath || character.Removed || character.CharacterHealth.GetAllAfflictions().Any(delegate(Affliction a)
			{
				AfflictionPrefab.Effect activeEffect = a.GetActiveEffect();
				return activeEffect != null && activeEffect.BlockTransformation.Contains(this.Prefab.Identifier);
			}))
			{
				this.UnsubscribeFromDeathEvent();
				return;
			}
			AnimController animController = character.AnimController;
			if (((animController != null) ? animController.LimbJoints : null) != null)
			{
				foreach (LimbJoint limbJoint in character.AnimController.LimbJoints)
				{
					if (limbJoint.IsSevered)
					{
						return;
					}
				}
			}
			CoroutineManager.StartCoroutine(this.CreateAIHusk(), "");
		}

		// Token: 0x060031E4 RID: 12772 RVA: 0x00206434 File Offset: 0x00204634
		private IEnumerable<CoroutineStatus> CreateAIHusk()
		{
			AfflictionHusk.<CreateAIHusk>d__33 <CreateAIHusk>d__ = new AfflictionHusk.<CreateAIHusk>d__33(-2);
			<CreateAIHusk>d__.<>4__this = this;
			return <CreateAIHusk>d__;
		}

		// Token: 0x060031E5 RID: 12773 RVA: 0x00206444 File Offset: 0x00204644
		public static List<Limb> AttachHuskAppendage(Character character, AfflictionPrefabHusk matchingAffliction, Identifier huskedSpeciesName, ContentXElement appendageDefinition = null, Ragdoll ragdoll = null)
		{
			List<Limb> appendageLimbs = new List<Limb>();
			CharacterPrefab huskPrefab = CharacterPrefab.FindBySpeciesName(huskedSpeciesName);
			ContentXElement contentXElement = (huskPrefab != null) ? huskPrefab.ConfigElement : null;
			ContentXElement contentXElement2 = null;
			if (contentXElement == contentXElement2)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(86, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to find the config file for the husk infected species with the species name '");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(huskedSpeciesName);
				defaultInterpolatedStringHandler.AppendLiteral("'!");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, matchingAffliction.ContentPackage, false, false);
				return appendageLimbs;
			}
			ContentXElement mainElement = huskPrefab.ConfigElement;
			ContentXElement element = appendageDefinition;
			contentXElement = null;
			if (element == contentXElement)
			{
				element = mainElement.GetChildElements("huskappendage").FirstOrDefault(delegate(ContentXElement e)
				{
					Identifier attributeIdentifier = e.GetAttributeIdentifier("affliction", Identifier.Empty);
					return attributeIdentifier == matchingAffliction.Identifier;
				});
			}
			contentXElement = null;
			if (element == contentXElement)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(94, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("Error in '");
				defaultInterpolatedStringHandler2.AppendFormatted<ContentPath>(huskPrefab.FilePath);
				defaultInterpolatedStringHandler2.AppendLiteral("': Failed to find a huskappendage that matches the affliction with an identifier '");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(matchingAffliction.Identifier);
				defaultInterpolatedStringHandler2.AppendLiteral("'!");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, matchingAffliction.ContentPackage, false, false);
				return appendageLimbs;
			}
			ContentPath pathToAppendage = element.GetAttributeContentPath("path") ?? ContentPath.Empty;
			XDocument doc = XMLExtensions.TryLoadXml(pathToAppendage);
			if (doc == null)
			{
				return appendageLimbs;
			}
			if (ragdoll == null)
			{
				ragdoll = character.AnimController;
			}
			if (ragdoll.Dir < 1f)
			{
				ragdoll.Flip();
			}
			ContentXElement root = doc.Root.FromPackage(pathToAppendage.ContentPackage);
			Dictionary<int, ContentXElement> limbElements = root.GetChildElements("limb").ToDictionary((ContentXElement e) => e.GetAttributeInt("id", -1), (ContentXElement e) => e);
			int? idOffset = null;
			Func<Limb, bool> <>9__3;
			Func<Limb, bool> <>9__4;
			Func<Limb, bool> <>9__5;
			foreach (ContentXElement jointElement in root.GetChildElements("joint"))
			{
				ContentXElement limbElement;
				if (limbElements.TryGetValue(jointElement.GetAttributeInt("limb2", -1), out limbElement))
				{
					RagdollParams.JointParams jointParams = new RagdollParams.JointParams(jointElement, ragdoll.RagdollParams);
					Limb attachLimb = null;
					if (matchingAffliction.AttachLimbId > -1)
					{
						IEnumerable<Limb> limbs = ragdoll.Limbs;
						Func<Limb, bool> predicate;
						if ((predicate = <>9__3) == null)
						{
							predicate = (<>9__3 = ((Limb l) => !l.IsSevered && l.Params.ID == matchingAffliction.AttachLimbId));
						}
						attachLimb = limbs.FirstOrDefault(predicate);
					}
					else if (matchingAffliction.AttachLimbName != null)
					{
						IEnumerable<Limb> limbs2 = ragdoll.Limbs;
						Func<Limb, bool> predicate2;
						if ((predicate2 = <>9__4) == null)
						{
							predicate2 = (<>9__4 = ((Limb l) => !l.IsSevered && l.Name == matchingAffliction.AttachLimbName));
						}
						attachLimb = limbs2.FirstOrDefault(predicate2);
					}
					else if (matchingAffliction.AttachLimbType != LimbType.None)
					{
						IEnumerable<Limb> limbs3 = ragdoll.Limbs;
						Func<Limb, bool> predicate3;
						if ((predicate3 = <>9__5) == null)
						{
							predicate3 = (<>9__5 = ((Limb l) => !l.IsSevered && l.type == matchingAffliction.AttachLimbType));
						}
						attachLimb = limbs3.FirstOrDefault(predicate3);
					}
					if (attachLimb == null)
					{
						attachLimb = ragdoll.Limbs.FirstOrDefault((Limb l) => !l.IsSevered && l.Params.ID == jointParams.Limb1);
					}
					if (attachLimb != null)
					{
						RagdollParams.LimbParams appendageLimbParams = new RagdollParams.LimbParams(limbElement, ragdoll.RagdollParams);
						int value = idOffset.GetValueOrDefault();
						if (idOffset == null)
						{
							value = ragdoll.Limbs.Length - appendageLimbParams.ID;
							idOffset = new int?(value);
						}
						jointParams.Limb1 = attachLimb.Params.ID;
						if (limbElements.ContainsKey(jointParams.Limb1))
						{
							jointParams.Limb1 += idOffset.Value;
						}
						if (limbElements.ContainsKey(jointParams.Limb2))
						{
							jointParams.Limb2 += idOffset.Value;
						}
						Limb huskAppendage = appendageLimbs.Find((Limb limb) => limb.Params.ID == appendageLimbParams.ID) ?? new Limb(ragdoll, character, appendageLimbParams);
						huskAppendage.body.Submarine = character.Submarine;
						huskAppendage.body.SetTransform(attachLimb.SimPosition, attachLimb.Rotation, true);
						ragdoll.AddLimb(huskAppendage);
						ragdoll.AddJoint(jointParams);
						appendageLimbs.Add(huskAppendage);
					}
				}
			}
			return appendageLimbs;
		}

		// Token: 0x060031E6 RID: 12774 RVA: 0x002068DC File Offset: 0x00204ADC
		public static Identifier GetHuskedSpeciesName(CharacterParams character, AfflictionPrefabHusk prefab)
		{
			Identifier huskedSpecies = character.HuskedSpecies;
			if (huskedSpecies.IsEmpty)
			{
				return new Identifier(character.SpeciesName.Value + prefab.HuskedSpeciesName.Value);
			}
			return huskedSpecies;
		}

		// Token: 0x060031E7 RID: 12775 RVA: 0x00206920 File Offset: 0x00204B20
		public static Identifier GetNonHuskedSpeciesName(CharacterParams character, AfflictionPrefabHusk prefab)
		{
			Identifier nonHuskedSpecies = character.NonHuskedSpecies;
			if (nonHuskedSpecies.IsEmpty)
			{
				return character.SpeciesName.Remove(prefab.HuskedSpeciesName);
			}
			return nonHuskedSpecies;
		}

		// Token: 0x060031E8 RID: 12776 RVA: 0x00206953 File Offset: 0x00204B53
		[CompilerGenerated]
		internal static bool <ApplyDamage>g__IsValidLimb|28_0(Limb limb)
		{
			return !limb.IgnoreCollisions && !limb.IsSevered && !limb.Hidden;
		}

		// Token: 0x040019EB RID: 6635
		private bool subscribedToDeathEvent;

		// Token: 0x040019EC RID: 6636
		private AfflictionHusk.InfectionState state;

		// Token: 0x040019ED RID: 6637
		private List<Limb> huskAppendage;

		// Token: 0x040019EE RID: 6638
		private Character character;

		// Token: 0x040019EF RID: 6639
		private bool stun;

		// Token: 0x040019F0 RID: 6640
		private float highestStrength;

		// Token: 0x040019F1 RID: 6641
		public readonly AfflictionPrefabHusk HuskPrefab;

		// Token: 0x040019F2 RID: 6642
		private AfflictionHusk.InfectionState? prevDisplayedMessage;

		// Token: 0x040019F3 RID: 6643
		private const float DamageCooldown = 0.1f;

		// Token: 0x040019F4 RID: 6644
		private float damageCooldownTimer;

		// Token: 0x02000EA6 RID: 3750
		public enum InfectionState
		{
			// Token: 0x040052CB RID: 21195
			Initial,
			// Token: 0x040052CC RID: 21196
			Dormant,
			// Token: 0x040052CD RID: 21197
			Transition,
			// Token: 0x040052CE RID: 21198
			Active,
			// Token: 0x040052CF RID: 21199
			Final
		}

		// Token: 0x02000EA7 RID: 3751
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x040052D0 RID: 21200
			public static Func<Limb, bool> <0>__IsValidLimb;
		}
	}
}
