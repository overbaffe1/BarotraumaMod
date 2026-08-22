using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma.PerkBehaviors
{
	// Token: 0x020003AE RID: 942
	[NullableContext(1)]
	[Nullable(0)]
	internal abstract class PerkBase : ISerializableEntity
	{
		// Token: 0x170011F8 RID: 4600
		// (get) Token: 0x060045C1 RID: 17857 RVA: 0x00269898 File Offset: 0x00267A98
		public string Name { get; }

		// Token: 0x170011F9 RID: 4601
		// (get) Token: 0x060045C2 RID: 17858 RVA: 0x002698A0 File Offset: 0x00267AA0
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; }

		// Token: 0x170011FA RID: 4602
		// (get) Token: 0x060045C3 RID: 17859 RVA: 0x002698A8 File Offset: 0x00267AA8
		public virtual PerkSimulation Simulation
		{
			get
			{
				return PerkSimulation.ServerOnly;
			}
		}

		// Token: 0x060045C4 RID: 17860 RVA: 0x002698AB File Offset: 0x00267AAB
		protected PerkBase(ContentXElement element, DisembarkPerkPrefab prefab)
		{
			this.Name = element.Name.ToString();
			this.Prefab = prefab;
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
		}

		// Token: 0x060045C5 RID: 17861 RVA: 0x002698DD File Offset: 0x00267ADD
		public virtual bool CanApply(SubmarineInfo submarine)
		{
			return true;
		}

		// Token: 0x060045C6 RID: 17862 RVA: 0x002698E0 File Offset: 0x00267AE0
		public bool CanApplyWithoutSubmarine()
		{
			return false;
		}

		// Token: 0x060045C7 RID: 17863
		public abstract void ApplyOnRoundStart(IReadOnlyCollection<Character> teamCharacters, [Nullable(2)] Submarine teamSubmarine);

		// Token: 0x060045C8 RID: 17864 RVA: 0x002698E4 File Offset: 0x00267AE4
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

		// Token: 0x04002434 RID: 9268
		public readonly DisembarkPerkPrefab Prefab;
	}
}
