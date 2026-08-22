using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;

namespace Barotrauma.Abilities
{
	// Token: 0x020003EF RID: 1007
	internal abstract class CharacterAbility
	{
		// Token: 0x17001215 RID: 4629
		// (get) Token: 0x06004680 RID: 18048 RVA: 0x0026C7BC File Offset: 0x0026A9BC
		public CharacterAbilityGroup CharacterAbilityGroup { get; }

		// Token: 0x17001216 RID: 4630
		// (get) Token: 0x06004681 RID: 18049 RVA: 0x0026C7C4 File Offset: 0x0026A9C4
		public CharacterTalent CharacterTalent { get; }

		// Token: 0x17001217 RID: 4631
		// (get) Token: 0x06004682 RID: 18050 RVA: 0x0026C7CC File Offset: 0x0026A9CC
		public Character Character { get; }

		// Token: 0x17001218 RID: 4632
		// (get) Token: 0x06004683 RID: 18051 RVA: 0x0026C7D4 File Offset: 0x0026A9D4
		public bool RequiresAlive { get; }

		// Token: 0x17001219 RID: 4633
		// (get) Token: 0x06004684 RID: 18052 RVA: 0x0026C7DC File Offset: 0x0026A9DC
		public virtual bool AllowClientSimulation
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700121A RID: 4634
		// (get) Token: 0x06004685 RID: 18053 RVA: 0x0026C7DF File Offset: 0x0026A9DF
		public virtual bool AppliesEffectOnIntervalUpdate
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700121B RID: 4635
		// (get) Token: 0x06004686 RID: 18054 RVA: 0x0026C7E4 File Offset: 0x0026A9E4
		protected float EffectDeltaTime
		{
			get
			{
				CharacterAbilityGroupInterval abilityGroupInterval = this.CharacterAbilityGroup as CharacterAbilityGroupInterval;
				if (abilityGroupInterval == null)
				{
					return 1f;
				}
				return abilityGroupInterval.TimeSinceLastUpdate;
			}
		}

		// Token: 0x06004687 RID: 18055 RVA: 0x0026C80C File Offset: 0x0026AA0C
		public CharacterAbility(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement)
		{
			if (characterAbilityGroup == null)
			{
				throw new ArgumentNullException("characterAbilityGroup");
			}
			this.CharacterAbilityGroup = characterAbilityGroup;
			this.CharacterTalent = characterAbilityGroup.CharacterTalent;
			this.Character = this.CharacterTalent.Character;
			this.RequiresAlive = abilityElement.GetAttributeBool("requiresalive", true);
		}

		// Token: 0x06004688 RID: 18056 RVA: 0x0026C864 File Offset: 0x0026AA64
		public bool IsViable()
		{
			if (!this.AllowClientSimulation)
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				if (networkMember != null && networkMember.IsClient)
				{
					return false;
				}
			}
			return !this.RequiresAlive || !this.Character.IsDead;
		}

		// Token: 0x06004689 RID: 18057 RVA: 0x0026C8A4 File Offset: 0x0026AAA4
		public virtual void InitializeAbility(bool addingFirstTime)
		{
		}

		// Token: 0x0600468A RID: 18058 RVA: 0x0026C8A6 File Offset: 0x0026AAA6
		public virtual void UpdateCharacterAbility(bool conditionsMatched, float timeSinceLastUpdate)
		{
			if (this.AppliesEffectOnIntervalUpdate)
			{
				if (conditionsMatched)
				{
					this.ApplyEffect();
					return;
				}
			}
			else
			{
				this.VerifyState(conditionsMatched, timeSinceLastUpdate);
			}
		}

		// Token: 0x0600468B RID: 18059 RVA: 0x0026C8C4 File Offset: 0x0026AAC4
		protected virtual void VerifyState(bool conditionsMatched, float timeSinceLastUpdate)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(130, 2);
			defaultInterpolatedStringHandler.AppendLiteral("Error in talent ");
			defaultInterpolatedStringHandler.AppendFormatted(this.CharacterTalent.DebugIdentifier);
			defaultInterpolatedStringHandler.AppendLiteral(": Ability ");
			defaultInterpolatedStringHandler.AppendFormatted<CharacterAbility>(this);
			defaultInterpolatedStringHandler.AppendLiteral(" does not have an implementation for VerifyState! This ability does not work in interval ability groups.");
			DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.CharacterTalent.Prefab.ContentPackage, false, false);
		}

		// Token: 0x0600468C RID: 18060 RVA: 0x0026C93B File Offset: 0x0026AB3B
		public void ApplyAbilityEffect(AbilityObject abilityObject)
		{
			if (abilityObject == null)
			{
				this.ApplyEffect();
				return;
			}
			this.ApplyEffect(abilityObject);
		}

		// Token: 0x0600468D RID: 18061 RVA: 0x0026C950 File Offset: 0x0026AB50
		protected virtual void ApplyEffect()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(92, 2);
			defaultInterpolatedStringHandler.AppendLiteral("Ability ");
			defaultInterpolatedStringHandler.AppendFormatted<CharacterAbility>(this);
			defaultInterpolatedStringHandler.AppendLiteral(" used improperly! This ability does not have a definition for ApplyEffect in talent ");
			defaultInterpolatedStringHandler.AppendFormatted(this.CharacterTalent.DebugIdentifier);
			DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), this.CharacterTalent.Prefab.ContentPackage);
		}

		// Token: 0x0600468E RID: 18062 RVA: 0x0026C9B8 File Offset: 0x0026ABB8
		protected virtual void ApplyEffect(AbilityObject abilityObject)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(91, 2);
			defaultInterpolatedStringHandler.AppendLiteral("Ability ");
			defaultInterpolatedStringHandler.AppendFormatted<CharacterAbility>(this);
			defaultInterpolatedStringHandler.AppendLiteral(" used improperly! This ability does not take a parameter for ApplyEffect in talent ");
			defaultInterpolatedStringHandler.AppendFormatted(this.CharacterTalent.DebugIdentifier);
			DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), this.CharacterTalent.Prefab.ContentPackage);
		}

		// Token: 0x0600468F RID: 18063 RVA: 0x0026CA20 File Offset: 0x0026AC20
		protected void LogAbilityObjectMismatch()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(97, 2);
			defaultInterpolatedStringHandler.AppendLiteral("Incompatible ability! Ability ");
			defaultInterpolatedStringHandler.AppendFormatted<CharacterAbility>(this);
			defaultInterpolatedStringHandler.AppendLiteral(" is incompatitible with this type of ability effect type in talent ");
			defaultInterpolatedStringHandler.AppendFormatted(this.CharacterTalent.DebugIdentifier);
			DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.CharacterTalent.Prefab.ContentPackage, false, false);
		}

		// Token: 0x06004690 RID: 18064 RVA: 0x0026CA88 File Offset: 0x0026AC88
		public static CharacterAbility Load(ContentXElement abilityElement, CharacterAbilityGroup characterAbilityGroup, bool errorMessages = true)
		{
			string type = abilityElement.Name.ToString().ToLowerInvariant();
			Type abilityType;
			try
			{
				abilityType = ReflectionUtils.GetTypeWithBackwardsCompatibility(ToolBox.BarotraumaAssembly, "Barotrauma.Abilities", type, false, true);
				if (abilityType == null)
				{
					if (errorMessages)
					{
						DebugConsole.ThrowError(string.Concat(new string[]
						{
							"Could not find the CharacterAbility \"",
							type,
							"\" (",
							characterAbilityGroup.CharacterTalent.DebugIdentifier,
							")"
						}), null, abilityElement.ContentPackage, false, false);
					}
					return null;
				}
			}
			catch (Exception e)
			{
				if (errorMessages)
				{
					DebugConsole.ThrowError(string.Concat(new string[]
					{
						"Could not find the CharacterAbility \"",
						type,
						"\" (",
						characterAbilityGroup.CharacterTalent.DebugIdentifier,
						")"
					}), e, abilityElement.ContentPackage, false, false);
				}
				return null;
			}
			object[] args = new object[]
			{
				characterAbilityGroup,
				abilityElement
			};
			CharacterAbility characterAbility;
			try
			{
				characterAbility = (CharacterAbility)Activator.CreateInstance(abilityType, args);
			}
			catch (TargetInvocationException e2)
			{
				string str = "Error while creating an instance of a CharacterAbility of the type ";
				Type type2 = abilityType;
				DebugConsole.ThrowError(str + ((type2 != null) ? type2.ToString() : null) + ".", e2.InnerException, abilityElement.ContentPackage, false, false);
				return null;
			}
			return characterAbility;
		}

		// Token: 0x04002482 RID: 9346
		private const float DefaultEffectTime = 1f;
	}
}
