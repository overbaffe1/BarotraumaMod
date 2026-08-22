using System;
using System.Linq;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x020001F7 RID: 503
	internal readonly struct DeconstructItem
	{
		// Token: 0x060023C6 RID: 9158 RVA: 0x000EED38 File Offset: 0x000ECF38
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

		// Token: 0x060023C7 RID: 9159 RVA: 0x000EEEA4 File Offset: 0x000ED0A4
		public bool IsValidDeconstructor(Item deconstructor)
		{
			return this.RequiredDeconstructor.Length == 0 || this.RequiredDeconstructor.Any((Identifier r) => deconstructor.HasTag(r) || deconstructor.Prefab.Identifier == r);
		}

		// Token: 0x04001189 RID: 4489
		public readonly Identifier ItemIdentifier;

		// Token: 0x0400118A RID: 4490
		public readonly int Amount;

		// Token: 0x0400118B RID: 4491
		public readonly float MinCondition;

		// Token: 0x0400118C RID: 4492
		public readonly float MaxCondition;

		// Token: 0x0400118D RID: 4493
		public readonly float OutConditionMin;

		// Token: 0x0400118E RID: 4494
		public readonly float OutConditionMax;

		// Token: 0x0400118F RID: 4495
		public readonly bool CopyCondition;

		// Token: 0x04001190 RID: 4496
		public readonly Identifier[] RequiredDeconstructor;

		// Token: 0x04001191 RID: 4497
		public readonly Identifier[] RequiredOtherItem;

		// Token: 0x04001192 RID: 4498
		public readonly string ActivateButtonText;

		// Token: 0x04001193 RID: 4499
		public readonly string InfoText;

		// Token: 0x04001194 RID: 4500
		public readonly string InfoTextOnOtherItemMissing;

		// Token: 0x04001195 RID: 4501
		public readonly float Commonness;
	}
}
