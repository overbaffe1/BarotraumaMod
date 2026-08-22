using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x020000C5 RID: 197
	internal class AfflictionHusk : Affliction
	{
		// Token: 0x17000669 RID: 1641
		// (get) Token: 0x06001614 RID: 5652 RVA: 0x000BAD64 File Offset: 0x000B8F64
		// (set) Token: 0x06001615 RID: 5653 RVA: 0x000BAD6C File Offset: 0x000B8F6C
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

		// Token: 0x1700066A RID: 1642
		// (get) Token: 0x06001616 RID: 5654 RVA: 0x000BAE02 File Offset: 0x000B9002
		// (set) Token: 0x06001617 RID: 5655 RVA: 0x000BAE0A File Offset: 0x000B900A
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

		// Token: 0x1700066B RID: 1643
		// (get) Token: 0x06001618 RID: 5656 RVA: 0x000BAE1D File Offset: 0x000B901D
		private float DormantThreshold
		{
			get
			{
				return this.HuskPrefab.DormantThreshold;
			}
		}

		// Token: 0x1700066C RID: 1644
		// (get) Token: 0x06001619 RID: 5657 RVA: 0x000BAE2A File Offset: 0x000B902A
		private float ActiveThreshold
		{
			get
			{
				return this.HuskPrefab.ActiveThreshold;
			}
		}

		// Token: 0x1700066D RID: 1645
		// (get) Token: 0x0600161A RID: 5658 RVA: 0x000BAE37 File Offset: 0x000B9037
		private float TransitionThreshold
		{
			get
			{
				return this.HuskPrefab.TransitionThreshold;
			}
		}

		// Token: 0x1700066E RID: 1646
		// (get) Token: 0x0600161B RID: 5659 RVA: 0x000BAE44 File Offset: 0x000B9044
		private float TransformThresholdOnDeath
		{
			get
			{
				return this.HuskPrefab.TransformThresholdOnDeath;
			}
		}

		// Token: 0x0600161C RID: 5660 RVA: 0x000BAE51 File Offset: 0x000B9051
		public AfflictionHusk(AfflictionPrefab prefab, float strength) : base(prefab, strength)
		{
			this.HuskPrefab = (prefab as AfflictionPrefabHusk);
			if (this.HuskPrefab == null)
			{
				DebugConsole.ThrowError("Error in husk affliction definition: the prefab is of wrong type!", null, prefab.ContentPackage, false, false);
			}
		}

		// Token: 0x0600161D RID: 5661 RVA: 0x000BAE84 File Offset: 0x000B9084
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

		// Token: 0x0600161E RID: 5662 RVA: 0x000BB01C File Offset: 0x000B921C
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
				if (showHuskWarning && this.character != Character.Controlled && this.character.IsBot)
				{
					Character character = this.character;
					string value = TextManager.Get("dialoghuskdormant").Value;
					float delay = Rand.Range(0.5f, 5f, Rand.RandSync.Unsynced);
					Identifier identifier = "huskdormant".ToIdentifier();
					character.Speak(value, null, delay, identifier, 0f);
				}
				break;
			case AfflictionHusk.InfectionState.Transition:
				afflictionPrefabHusk = (this.Prefab as AfflictionPrefabHusk);
				if (afflictionPrefabHusk != null && afflictionPrefabHusk.CauseSpeechImpediment && this.character != Character.Controlled && this.character.IsBot)
				{
					Character character2 = this.character;
					string value2 = TextManager.Get("dialoghuskcantspeak").Value;
					float delay = Rand.Range(0.5f, 5f, Rand.RandSync.Unsynced);
					Identifier identifier = "huskcantspeak".ToIdentifier();
					character2.Speak(value2, null, delay, identifier, 0f);
				}
				break;
			}
			this.prevDisplayedMessage = new AfflictionHusk.InfectionState?(this.State);
		}

		// Token: 0x0600161F RID: 5663 RVA: 0x000BB1EC File Offset: 0x000B93EC
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

		// Token: 0x06001620 RID: 5664 RVA: 0x000BB318 File Offset: 0x000B9518
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

		// Token: 0x06001621 RID: 5665 RVA: 0x000BB3BC File Offset: 0x000B95BC
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

		// Token: 0x06001622 RID: 5666 RVA: 0x000BB474 File Offset: 0x000B9674
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

		// Token: 0x06001623 RID: 5667 RVA: 0x000BB4C0 File Offset: 0x000B96C0
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

		// Token: 0x06001624 RID: 5668 RVA: 0x000BB568 File Offset: 0x000B9768
		private IEnumerable<CoroutineStatus> CreateAIHusk()
		{
			AfflictionHusk.<CreateAIHusk>d__33 <CreateAIHusk>d__ = new AfflictionHusk.<CreateAIHusk>d__33(-2);
			<CreateAIHusk>d__.<>4__this = this;
			return <CreateAIHusk>d__;
		}

		// Token: 0x06001625 RID: 5669 RVA: 0x000BB578 File Offset: 0x000B9778
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

		// Token: 0x06001626 RID: 5670 RVA: 0x000BBA10 File Offset: 0x000B9C10
		public static Identifier GetHuskedSpeciesName(CharacterParams character, AfflictionPrefabHusk prefab)
		{
			Identifier huskedSpecies = character.HuskedSpecies;
			if (huskedSpecies.IsEmpty)
			{
				return new Identifier(character.SpeciesName.Value + prefab.HuskedSpeciesName.Value);
			}
			return huskedSpecies;
		}

		// Token: 0x06001627 RID: 5671 RVA: 0x000BBA54 File Offset: 0x000B9C54
		public static Identifier GetNonHuskedSpeciesName(CharacterParams character, AfflictionPrefabHusk prefab)
		{
			Identifier nonHuskedSpecies = character.NonHuskedSpecies;
			if (nonHuskedSpecies.IsEmpty)
			{
				return character.SpeciesName.Remove(prefab.HuskedSpeciesName);
			}
			return nonHuskedSpecies;
		}

		// Token: 0x06001628 RID: 5672 RVA: 0x000BBA87 File Offset: 0x000B9C87
		[CompilerGenerated]
		internal static bool <ApplyDamage>g__IsValidLimb|28_0(Limb limb)
		{
			return !limb.IgnoreCollisions && !limb.IsSevered && !limb.Hidden;
		}

		// Token: 0x04000A80 RID: 2688
		private bool subscribedToDeathEvent;

		// Token: 0x04000A81 RID: 2689
		private AfflictionHusk.InfectionState state;

		// Token: 0x04000A82 RID: 2690
		private List<Limb> huskAppendage;

		// Token: 0x04000A83 RID: 2691
		private Character character;

		// Token: 0x04000A84 RID: 2692
		private bool stun;

		// Token: 0x04000A85 RID: 2693
		private float highestStrength;

		// Token: 0x04000A86 RID: 2694
		public readonly AfflictionPrefabHusk HuskPrefab;

		// Token: 0x04000A87 RID: 2695
		private AfflictionHusk.InfectionState? prevDisplayedMessage;

		// Token: 0x04000A88 RID: 2696
		private const float DamageCooldown = 0.1f;

		// Token: 0x04000A89 RID: 2697
		private float damageCooldownTimer;

		// Token: 0x02000871 RID: 2161
		public enum InfectionState
		{
			// Token: 0x04002FB0 RID: 12208
			Initial,
			// Token: 0x04002FB1 RID: 12209
			Dormant,
			// Token: 0x04002FB2 RID: 12210
			Transition,
			// Token: 0x04002FB3 RID: 12211
			Active,
			// Token: 0x04002FB4 RID: 12212
			Final
		}

		// Token: 0x02000872 RID: 2162
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04002FB5 RID: 12213
			public static Func<Limb, bool> <0>__IsValidLimb;
		}
	}
}
