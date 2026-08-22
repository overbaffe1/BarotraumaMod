using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma.PerkBehaviors
{
	// Token: 0x020002E8 RID: 744
	[NullableContext(1)]
	[Nullable(0)]
	internal abstract class PerkBase : ISerializableEntity
	{
		// Token: 0x17000E08 RID: 3592
		// (get) Token: 0x06003187 RID: 12679 RVA: 0x00151A04 File Offset: 0x0014FC04
		public string Name { get; }

		// Token: 0x17000E09 RID: 3593
		// (get) Token: 0x06003188 RID: 12680 RVA: 0x00151A0C File Offset: 0x0014FC0C
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; }

		// Token: 0x17000E0A RID: 3594
		// (get) Token: 0x06003189 RID: 12681 RVA: 0x00151A14 File Offset: 0x0014FC14
		public virtual PerkSimulation Simulation
		{
			get
			{
				return PerkSimulation.ServerOnly;
			}
		}

		// Token: 0x0600318A RID: 12682 RVA: 0x00151A17 File Offset: 0x0014FC17
		protected PerkBase(ContentXElement element, DisembarkPerkPrefab prefab)
		{
			this.Name = element.Name.ToString();
			this.Prefab = prefab;
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
		}

		// Token: 0x0600318B RID: 12683 RVA: 0x00151A49 File Offset: 0x0014FC49
		public virtual bool CanApply(SubmarineInfo submarine)
		{
			return true;
		}

		// Token: 0x0600318C RID: 12684 RVA: 0x00151A4C File Offset: 0x0014FC4C
		public bool CanApplyWithoutSubmarine()
		{
			return false;
		}

		// Token: 0x0600318D RID: 12685
		public abstract void ApplyOnRoundStart(IReadOnlyCollection<Character> teamCharacters, [Nullable(2)] Submarine teamSubmarine);

		// Token: 0x0600318E RID: 12686 RVA: 0x00151A50 File Offset: 0x0014FC50
		public static bool TryLoadFromXml(ContentXElement element, DisembarkPerkPrefab prefab, [Nullable(2)] [NotNullWhen(true)] out PerkBase perk)
		{
			Type type = ReflectionUtils.GetTypeWithBackwardsCompatibility(ToolBox.BarotraumaAssembly, "Barotrauma.PerkBehaviors", element.Name.ToString(), false, true);
			if (type == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(46, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Could not find a perk behavior of the type \"");
				defaultInterpolatedStringHandler.AppendFormatted<XName>(element.Name);
				defaultInterpolatedStringHandler.AppendLiteral("\".");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
				perk = null;
				return false;
			}
			bool result;
			try
			{
				object instance = Activator.CreateInstance(type, new object[]
				{
					element,
					prefab
				});
				PerkBase perkInstance = instance as PerkBase;
				if (perkInstance == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(45, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("Could not cast the instance of type \"");
					defaultInterpolatedStringHandler2.AppendFormatted<Type>(type);
					defaultInterpolatedStringHandler2.AppendLiteral("\" to a ");
					defaultInterpolatedStringHandler2.AppendFormatted("PerkBase");
					defaultInterpolatedStringHandler2.AppendLiteral(".");
					throw new InvalidCastException(defaultInterpolatedStringHandler2.ToStringAndClear());
				}
				perk = perkInstance;
				result = true;
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError((e.InnerException != null) ? e.InnerException.ToString() : e.ToString(), null, element.ContentPackage, false, false);
				perk = null;
				result = false;
			}
			return result;
		}

		// Token: 0x04001873 RID: 6259
		public readonly DisembarkPerkPrefab Prefab;
	}
}
