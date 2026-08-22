using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001C8 RID: 456
	internal class AfflictionPrefabHusk : AfflictionPrefab
	{
		// Token: 0x060031F1 RID: 12785 RVA: 0x00206B8C File Offset: 0x00204D8C
		public AfflictionPrefabHusk(ContentXElement element, AfflictionsFile file, Type type = null) : base(element, file, type)
		{
			this.HuskedSpeciesName = element.GetAttributeIdentifier("huskedspeciesname", Identifier.Empty);
			if (this.HuskedSpeciesName.IsEmpty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(61, 2);
				defaultInterpolatedStringHandler.AppendLiteral("No 'huskedspeciesname' defined for the husk affliction (");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral(") in ");
				defaultInterpolatedStringHandler.AppendFormatted<ContentXElement>(element);
				DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(Color.Orange), false);
				this.HuskedSpeciesName = "husk".ToIdentifier();
			}
			this.HuskedSpeciesName = this.HuskedSpeciesName.Remove("[speciesname]").ToIdentifier<Identifier>();
			if (base.TargetSpecies.Length == 0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(51, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("No 'targets' defined for the husk affliction (");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.Identifier);
				defaultInterpolatedStringHandler2.AppendLiteral(") in ");
				defaultInterpolatedStringHandler2.AppendFormatted<ContentXElement>(element);
				DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), new Color?(Color.Orange), false);
				base.TargetSpecies = new Identifier[]
				{
					CharacterPrefab.HumanSpeciesName
				};
			}
			ContentXElement attachElement = element.GetChildElement("attachlimb");
			ContentXElement contentXElement = null;
			if (attachElement != contentXElement)
			{
				this.AttachLimbId = attachElement.GetAttributeInt("id", -1);
				this.AttachLimbName = attachElement.GetAttributeString("name", null);
				ContentXElement contentXElement2 = attachElement;
				string key = "type";
				LimbType limbType = LimbType.None;
				this.AttachLimbType = contentXElement2.GetAttributeEnum<LimbType>(key, limbType);
			}
			else
			{
				this.AttachLimbId = -1;
				this.AttachLimbName = null;
				this.AttachLimbType = LimbType.None;
			}
			this.TransferBuffs = element.GetAttributeBool("transferbuffs", true);
			this.SendMessages = element.GetAttributeBool("sendmessages", true);
			this.CauseSpeechImpediment = element.GetAttributeBool("causespeechimpediment", true);
			this.NeedsAir = element.GetAttributeBool("needsair", false);
			this.ControlHusk = element.GetAttributeBool("controlhusk", false);
			this.DormantThreshold = element.GetAttributeFloat("dormantthreshold", this.MaxStrength * 0.5f);
			this.ActiveThreshold = element.GetAttributeFloat("activethreshold", this.MaxStrength * 0.75f);
			this.TransitionThreshold = element.GetAttributeFloat("transitionthreshold", this.MaxStrength);
			if (this.DormantThreshold > this.ActiveThreshold)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(36, 5);
				defaultInterpolatedStringHandler3.AppendLiteral("Error in \"");
				defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(this.Identifier);
				defaultInterpolatedStringHandler3.AppendLiteral("\": ");
				defaultInterpolatedStringHandler3.AppendFormatted("DormantThreshold");
				defaultInterpolatedStringHandler3.AppendLiteral(" is greater than ");
				defaultInterpolatedStringHandler3.AppendFormatted("ActiveThreshold");
				defaultInterpolatedStringHandler3.AppendLiteral(" (");
				defaultInterpolatedStringHandler3.AppendFormatted<float>(this.DormantThreshold);
				defaultInterpolatedStringHandler3.AppendLiteral(" > ");
				defaultInterpolatedStringHandler3.AppendFormatted<float>(this.ActiveThreshold);
				defaultInterpolatedStringHandler3.AppendLiteral(")");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), null, element.ContentPackage, false, false);
			}
			if (this.ActiveThreshold > this.TransitionThreshold)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(36, 5);
				defaultInterpolatedStringHandler4.AppendLiteral("Error in \"");
				defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(this.Identifier);
				defaultInterpolatedStringHandler4.AppendLiteral("\": ");
				defaultInterpolatedStringHandler4.AppendFormatted("ActiveThreshold");
				defaultInterpolatedStringHandler4.AppendLiteral(" is greater than ");
				defaultInterpolatedStringHandler4.AppendFormatted("TransitionThreshold");
				defaultInterpolatedStringHandler4.AppendLiteral(" (");
				defaultInterpolatedStringHandler4.AppendFormatted<float>(this.ActiveThreshold);
				defaultInterpolatedStringHandler4.AppendLiteral(" > ");
				defaultInterpolatedStringHandler4.AppendFormatted<float>(this.TransitionThreshold);
				defaultInterpolatedStringHandler4.AppendLiteral(")");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler4.ToStringAndClear(), null, element.ContentPackage, false, false);
			}
			this.TransformThresholdOnDeath = element.GetAttributeFloat("transformthresholdondeath", this.ActiveThreshold);
		}

		// Token: 0x04001A00 RID: 6656
		public readonly int AttachLimbId;

		// Token: 0x04001A01 RID: 6657
		public readonly string AttachLimbName;

		// Token: 0x04001A02 RID: 6658
		public readonly LimbType AttachLimbType;

		// Token: 0x04001A03 RID: 6659
		public readonly float DormantThreshold;

		// Token: 0x04001A04 RID: 6660
		public readonly float ActiveThreshold;

		// Token: 0x04001A05 RID: 6661
		public readonly float TransitionThreshold;

		// Token: 0x04001A06 RID: 6662
		public readonly float TransformThresholdOnDeath;

		// Token: 0x04001A07 RID: 6663
		public readonly Identifier HuskedSpeciesName;

		// Token: 0x04001A08 RID: 6664
		public readonly bool TransferBuffs;

		// Token: 0x04001A09 RID: 6665
		public readonly bool SendMessages;

		// Token: 0x04001A0A RID: 6666
		public readonly bool CauseSpeechImpediment;

		// Token: 0x04001A0B RID: 6667
		public readonly bool NeedsAir;

		// Token: 0x04001A0C RID: 6668
		public readonly bool ControlHusk;
	}
}
