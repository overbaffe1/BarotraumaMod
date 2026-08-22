using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;

namespace Barotrauma.Abilities
{
	// Token: 0x02000329 RID: 809
	internal abstract class CharacterAbility
	{
		// Token: 0x17000E25 RID: 3621
		// (get) Token: 0x06003246 RID: 12870 RVA: 0x0015493C File Offset: 0x00152B3C
		public CharacterAbilityGroup CharacterAbilityGroup { get; }

		// Token: 0x17000E26 RID: 3622
		// (get) Token: 0x06003247 RID: 12871 RVA: 0x00154944 File Offset: 0x00152B44
		public CharacterTalent CharacterTalent { get; }

		// Token: 0x17000E27 RID: 3623
		// (get) Token: 0x06003248 RID: 12872 RVA: 0x0015494C File Offset: 0x00152B4C
		public Character Character { get; }

		// Token: 0x17000E28 RID: 3624
		// (get) Token: 0x06003249 RID: 12873 RVA: 0x00154954 File Offset: 0x00152B54
		public bool RequiresAlive { get; }

		// Token: 0x17000E29 RID: 3625
		// (get) Token: 0x0600324A RID: 12874 RVA: 0x0015495C File Offset: 0x00152B5C
		public virtual bool AllowClientSimulation
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000E2A RID: 3626
		// (get) Token: 0x0600324B RID: 12875 RVA: 0x0015495F File Offset: 0x00152B5F
		public virtual bool AppliesEffectOnIntervalUpdate
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000E2B RID: 3627
		// (get) Token: 0x0600324C RID: 12876 RVA: 0x00154964 File Offset: 0x00152B64
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

		// Token: 0x0600324D RID: 12877 RVA: 0x0015498C File Offset: 0x00152B8C
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

		// Token: 0x0600324E RID: 12878 RVA: 0x001549E4 File Offset: 0x00152BE4
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

		// Token: 0x0600324F RID: 12879 RVA: 0x00154A24 File Offset: 0x00152C24
		public virtual void InitializeAbility(bool addingFirstTime)
		{
		}

		// Token: 0x06003250 RID: 12880 RVA: 0x00154A26 File Offset: 0x00152C26
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

		// Token: 0x06003251 RID: 12881 RVA: 0x00154A44 File Offset: 0x00152C44
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

		// Token: 0x06003252 RID: 12882 RVA: 0x00154ABB File Offset: 0x00152CBB
		public void ApplyAbilityEffect(AbilityObject abilityObject)
		{
			if (abilityObject == null)
			{
				this.ApplyEffect();
				return;
			}
			this.ApplyEffect(abilityObject);
		}

		// Token: 0x06003253 RID: 12883 RVA: 0x00154AD0 File Offset: 0x00152CD0
		protected virtual void ApplyEffect()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(92, 2);
			defaultInterpolatedStringHandler.AppendLiteral("Ability ");
			defaultInterpolatedStringHandler.AppendFormatted<CharacterAbility>(this);
			defaultInterpolatedStringHandler.AppendLiteral(" used improperly! This ability does not have a definition for ApplyEffect in talent ");
			defaultInterpolatedStringHandler.AppendFormatted(this.CharacterTalent.DebugIdentifier);
			DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), this.CharacterTalent.Prefab.ContentPackage);
		}

		// Token: 0x06003254 RID: 12884 RVA: 0x00154B38 File Offset: 0x00152D38
		protected virtual void ApplyEffect(AbilityObject abilityObject)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(91, 2);
			defaultInterpolatedStringHandler.AppendLiteral("Ability ");
			defaultInterpolatedStringHandler.AppendFormatted<CharacterAbility>(this);
			defaultInterpolatedStringHandler.AppendLiteral(" used improperly! This ability does not take a parameter for ApplyEffect in talent ");
			defaultInterpolatedStringHandler.AppendFormatted(this.CharacterTalent.DebugIdentifier);
			DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), this.CharacterTalent.Prefab.ContentPackage);
		}

		// Token: 0x06003255 RID: 12885 RVA: 0x00154BA0 File Offset: 0x00152DA0
		protected void LogAbilityObjectMismatch()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(97, 2);
			defaultInterpolatedStringHandler.AppendLiteral("Incompatible ability! Ability ");
			defaultInterpolatedStringHandler.AppendFormatted<CharacterAbility>(this);
			defaultInterpolatedStringHandler.AppendLiteral(" is incompatitible with this type of ability effect type in talent ");
			defaultInterpolatedStringHandler.AppendFormatted(this.CharacterTalent.DebugIdentifier);
			DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.CharacterTalent.Prefab.ContentPackage, false, false);
		}

		// Token: 0x06003256 RID: 12886 RVA: 0x00154C08 File Offset: 0x00152E08
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

		// Token: 0x040018C1 RID: 6337
		private const float DefaultEffectTime = 1f;
	}
}
