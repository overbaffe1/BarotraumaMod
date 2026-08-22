using System;
using System.Linq;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x020002E1 RID: 737
	internal readonly struct DeconstructItem
	{
		// Token: 0x06003D6C RID: 15724 RVA: 0x0022E49C File Offset: 0x0022C69C
		public DeconstructItem(XElement element, Identifier parentDebugName)
		{
			this.ItemIdentifier = element.GetAttributeIdentifier("identifier", "");
			this.Amount = element.GetAttributeInt("amount", 1);
			this.MinCondition = element.GetAttributeFloat("mincondition", -0.1f);
			this.MaxCondition = element.GetAttributeFloat("maxcondition", 1f);
			this.OutConditionMin = element.GetAttributeFloat("outconditionmin", element.GetAttributeFloat("outcondition", 1f));
			this.OutConditionMax = element.GetAttributeFloat("outconditionmax", element.GetAttributeFloat("outcondition", 1f));
			this.CopyCondition = element.GetAttributeBool("copycondition", false);
			this.Commonness = element.GetAttributeFloat("commonness", 1f);
			Identifier[] defaultRequiredDeconstructor = new Identifier[]
			{
				"deconstructor".ToIdentifier()
			};
			string name = "requireddeconstructor";
			XElement parent = element.Parent;
			this.RequiredDeconstructor = element.GetAttributeIdentifierArray(name, ((parent != null) ? parent.GetAttributeIdentifierArray("requireddeconstructor", null, true) : null) ?? defaultRequiredDeconstructor, true);
			this.RequiredOtherItem = element.GetAttributeIdentifierArray("requiredotheritem", Array.Empty<Identifier>(), true);
			this.ActivateButtonText = element.GetAttributeString("activatebuttontext", string.Empty);
			this.InfoText = element.GetAttributeString("infotext", string.Empty);
			this.InfoTextOnOtherItemMissing = element.GetAttributeString("infotextonotheritemmissing", string.Empty);
		}

		// Token: 0x06003D6D RID: 15725 RVA: 0x0022E608 File Offset: 0x0022C808
		public bool IsValidDeconstructor(Item deconstructor)
		{
			return this.RequiredDeconstructor.Length == 0 || this.RequiredDeconstructor.Any((Identifier r) => deconstructor.HasTag(r) || deconstructor.Prefab.Identifier == r);
		}

		// Token: 0x04002012 RID: 8210
		public readonly Identifier ItemIdentifier;

		// Token: 0x04002013 RID: 8211
		public readonly int Amount;

		// Token: 0x04002014 RID: 8212
		public readonly float MinCondition;

		// Token: 0x04002015 RID: 8213
		public readonly float MaxCondition;

		// Token: 0x04002016 RID: 8214
		public readonly float OutConditionMin;

		// Token: 0x04002017 RID: 8215
		public readonly float OutConditionMax;

		// Token: 0x04002018 RID: 8216
		public readonly bool CopyCondition;

		// Token: 0x04002019 RID: 8217
		public readonly Identifier[] RequiredDeconstructor;

		// Token: 0x0400201A RID: 8218
		public readonly Identifier[] RequiredOtherItem;

		// Token: 0x0400201B RID: 8219
		public readonly string ActivateButtonText;

		// Token: 0x0400201C RID: 8220
		public readonly string InfoText;

		// Token: 0x0400201D RID: 8221
		public readonly string InfoTextOnOtherItemMissing;

		// Token: 0x0400201E RID: 8222
		public readonly float Commonness;
	}
}
