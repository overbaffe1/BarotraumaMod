using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000582 RID: 1410
	public class ConfigInfo : IConfigInfo, IConfigDisplayInfo, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IEquatable<ConfigInfo>
	{
		// Token: 0x17001564 RID: 5476
		// (get) Token: 0x06005694 RID: 22164 RVA: 0x002D27A1 File Offset: 0x002D09A1
		[Nullable(1)]
		[CompilerGenerated]
		protected virtual Type EqualityContract
		{
			[NullableContext(1)]
			[CompilerGenerated]
			get
			{
				return typeof(ConfigInfo);
			}
		}

		// Token: 0x17001565 RID: 5477
		// (get) Token: 0x06005695 RID: 22165 RVA: 0x002D27AD File Offset: 0x002D09AD
		// (set) Token: 0x06005696 RID: 22166 RVA: 0x002D27B5 File Offset: 0x002D09B5
		public string InternalName { get; set; }

		// Token: 0x17001566 RID: 5478
		// (get) Token: 0x06005697 RID: 22167 RVA: 0x002D27BE File Offset: 0x002D09BE
		// (set) Token: 0x06005698 RID: 22168 RVA: 0x002D27C6 File Offset: 0x002D09C6
		public ContentPackage OwnerPackage { get; set; }

		// Token: 0x17001567 RID: 5479
		// (get) Token: 0x06005699 RID: 22169 RVA: 0x002D27CF File Offset: 0x002D09CF
		// (set) Token: 0x0600569A RID: 22170 RVA: 0x002D27D7 File Offset: 0x002D09D7
		public string DataType { get; set; }

		// Token: 0x17001568 RID: 5480
		// (get) Token: 0x0600569B RID: 22171 RVA: 0x002D27E0 File Offset: 0x002D09E0
		// (set) Token: 0x0600569C RID: 22172 RVA: 0x002D27E8 File Offset: 0x002D09E8
		public XElement Element { get; set; }

		// Token: 0x17001569 RID: 5481
		// (get) Token: 0x0600569D RID: 22173 RVA: 0x002D27F1 File Offset: 0x002D09F1
		// (set) Token: 0x0600569E RID: 22174 RVA: 0x002D27F9 File Offset: 0x002D09F9
		public RunState EditableStates { get; set; }

		// Token: 0x1700156A RID: 5482
		// (get) Token: 0x0600569F RID: 22175 RVA: 0x002D2802 File Offset: 0x002D0A02
		// (set) Token: 0x060056A0 RID: 22176 RVA: 0x002D280A File Offset: 0x002D0A0A
		public NetSync NetSync { get; set; }

		// Token: 0x1700156B RID: 5483
		// (get) Token: 0x060056A1 RID: 22177 RVA: 0x002D2813 File Offset: 0x002D0A13
		// (set) Token: 0x060056A2 RID: 22178 RVA: 0x002D281B File Offset: 0x002D0A1B
		public string DisplayName { get; set; }

		// Token: 0x1700156C RID: 5484
		// (get) Token: 0x060056A3 RID: 22179 RVA: 0x002D2824 File Offset: 0x002D0A24
		// (set) Token: 0x060056A4 RID: 22180 RVA: 0x002D282C File Offset: 0x002D0A2C
		public string Description { get; set; }

		// Token: 0x1700156D RID: 5485
		// (get) Token: 0x060056A5 RID: 22181 RVA: 0x002D2835 File Offset: 0x002D0A35
		// (set) Token: 0x060056A6 RID: 22182 RVA: 0x002D283D File Offset: 0x002D0A3D
		public string DisplayCategory { get; set; }

		// Token: 0x1700156E RID: 5486
		// (get) Token: 0x060056A7 RID: 22183 RVA: 0x002D2846 File Offset: 0x002D0A46
		// (set) Token: 0x060056A8 RID: 22184 RVA: 0x002D284E File Offset: 0x002D0A4E
		public bool ShowInMenus { get; set; }

		// Token: 0x1700156F RID: 5487
		// (get) Token: 0x060056A9 RID: 22185 RVA: 0x002D2857 File Offset: 0x002D0A57
		// (set) Token: 0x060056AA RID: 22186 RVA: 0x002D285F File Offset: 0x002D0A5F
		public string Tooltip { get; set; }

		// Token: 0x17001570 RID: 5488
		// (get) Token: 0x060056AB RID: 22187 RVA: 0x002D2868 File Offset: 0x002D0A68
		// (set) Token: 0x060056AC RID: 22188 RVA: 0x002D2870 File Offset: 0x002D0A70
		public ContentPath ImageIconPath { get; set; }

		// Token: 0x060056AD RID: 22189 RVA: 0x002D287C File Offset: 0x002D0A7C
		[NullableContext(1)]
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("ConfigInfo");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x060056AE RID: 22190 RVA: 0x002D28C8 File Offset: 0x002D0AC8
		[NullableContext(1)]
		[CompilerGenerated]
		protected virtual bool PrintMembers(StringBuilder builder)
		{
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("InternalName = ");
			builder.Append(this.InternalName);
			builder.Append(", OwnerPackage = ");
			builder.Append(this.OwnerPackage);
			builder.Append(", DataType = ");
			builder.Append(this.DataType);
			builder.Append(", Element = ");
			builder.Append(this.Element);
			builder.Append(", EditableStates = ");
			builder.Append(this.EditableStates.ToString());
			builder.Append(", NetSync = ");
			builder.Append(this.NetSync.ToString());
			builder.Append(", DisplayName = ");
			builder.Append(this.DisplayName);
			builder.Append(", Description = ");
			builder.Append(this.Description);
			builder.Append(", DisplayCategory = ");
			builder.Append(this.DisplayCategory);
			builder.Append(", ShowInMenus = ");
			builder.Append(this.ShowInMenus.ToString());
			builder.Append(", Tooltip = ");
			builder.Append(this.Tooltip);
			builder.Append(", ImageIconPath = ");
			builder.Append(this.ImageIconPath);
			return true;
		}

		// Token: 0x060056AF RID: 22191 RVA: 0x002D2A31 File Offset: 0x002D0C31
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator !=(ConfigInfo left, ConfigInfo right)
		{
			return !(left == right);
		}

		// Token: 0x060056B0 RID: 22192 RVA: 0x002D2A3D File Offset: 0x002D0C3D
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator ==(ConfigInfo left, ConfigInfo right)
		{
			return left == right || (left != null && left.Equals(right));
		}

		// Token: 0x060056B1 RID: 22193 RVA: 0x002D2A54 File Offset: 0x002D0C54
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return (((((((((((EqualityComparer<Type>.Default.GetHashCode(this.EqualityContract) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<InternalName>k__BackingField)) * -1521134295 + EqualityComparer<ContentPackage>.Default.GetHashCode(this.<OwnerPackage>k__BackingField)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<DataType>k__BackingField)) * -1521134295 + EqualityComparer<XElement>.Default.GetHashCode(this.<Element>k__BackingField)) * -1521134295 + EqualityComparer<RunState>.Default.GetHashCode(this.<EditableStates>k__BackingField)) * -1521134295 + EqualityComparer<NetSync>.Default.GetHashCode(this.<NetSync>k__BackingField)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<DisplayName>k__BackingField)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<Description>k__BackingField)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<DisplayCategory>k__BackingField)) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<ShowInMenus>k__BackingField)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<Tooltip>k__BackingField)) * -1521134295 + EqualityComparer<ContentPath>.Default.GetHashCode(this.<ImageIconPath>k__BackingField);
		}

		// Token: 0x060056B2 RID: 22194 RVA: 0x002D2B85 File Offset: 0x002D0D85
		[NullableContext(2)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return this.Equals(obj as ConfigInfo);
		}

		// Token: 0x060056B3 RID: 22195 RVA: 0x002D2B94 File Offset: 0x002D0D94
		[NullableContext(2)]
		[CompilerGenerated]
		public virtual bool Equals(ConfigInfo other)
		{
			return this == other || (other != null && this.EqualityContract == other.EqualityContract && EqualityComparer<string>.Default.Equals(this.<InternalName>k__BackingField, other.<InternalName>k__BackingField) && EqualityComparer<ContentPackage>.Default.Equals(this.<OwnerPackage>k__BackingField, other.<OwnerPackage>k__BackingField) && EqualityComparer<string>.Default.Equals(this.<DataType>k__BackingField, other.<DataType>k__BackingField) && EqualityComparer<XElement>.Default.Equals(this.<Element>k__BackingField, other.<Element>k__BackingField) && EqualityComparer<RunState>.Default.Equals(this.<EditableStates>k__BackingField, other.<EditableStates>k__BackingField) && EqualityComparer<NetSync>.Default.Equals(this.<NetSync>k__BackingField, other.<NetSync>k__BackingField) && EqualityComparer<string>.Default.Equals(this.<DisplayName>k__BackingField, other.<DisplayName>k__BackingField) && EqualityComparer<string>.Default.Equals(this.<Description>k__BackingField, other.<Description>k__BackingField) && EqualityComparer<string>.Default.Equals(this.<DisplayCategory>k__BackingField, other.<DisplayCategory>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<ShowInMenus>k__BackingField, other.<ShowInMenus>k__BackingField) && EqualityComparer<string>.Default.Equals(this.<Tooltip>k__BackingField, other.<Tooltip>k__BackingField) && EqualityComparer<ContentPath>.Default.Equals(this.<ImageIconPath>k__BackingField, other.<ImageIconPath>k__BackingField));
		}

		// Token: 0x060056B5 RID: 22197 RVA: 0x002D2D00 File Offset: 0x002D0F00
		[CompilerGenerated]
		protected ConfigInfo([Nullable(1)] ConfigInfo original)
		{
			this.InternalName = original.<InternalName>k__BackingField;
			this.OwnerPackage = original.<OwnerPackage>k__BackingField;
			this.DataType = original.<DataType>k__BackingField;
			this.Element = original.<Element>k__BackingField;
			this.EditableStates = original.<EditableStates>k__BackingField;
			this.NetSync = original.<NetSync>k__BackingField;
			this.DisplayName = original.<DisplayName>k__BackingField;
			this.Description = original.<Description>k__BackingField;
			this.DisplayCategory = original.<DisplayCategory>k__BackingField;
			this.ShowInMenus = original.<ShowInMenus>k__BackingField;
			this.Tooltip = original.<Tooltip>k__BackingField;
			this.ImageIconPath = original.<ImageIconPath>k__BackingField;
		}

		// Token: 0x060056B6 RID: 22198 RVA: 0x002D2DA3 File Offset: 0x002D0FA3
		public ConfigInfo()
		{
		}
	}
}
