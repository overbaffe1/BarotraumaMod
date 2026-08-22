using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000279 RID: 633
	[NullableContext(1)]
	[Nullable(0)]
	internal class CheckAfflictionAction : BinaryOptionAction
	{
		// Token: 0x17000ED6 RID: 3798
		// (get) Token: 0x0600389E RID: 14494 RVA: 0x00218967 File Offset: 0x00216B67
		// (set) Token: 0x0600389F RID: 14495 RVA: 0x0021896F File Offset: 0x00216B6F
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the affliction.", "", false)]
		public Identifier Identifier { get; set; } = Identifier.Empty;

		// Token: 0x17000ED7 RID: 3799
		// (get) Token: 0x060038A0 RID: 14496 RVA: 0x00218978 File Offset: 0x00216B78
		// (set) Token: 0x060038A1 RID: 14497 RVA: 0x00218980 File Offset: 0x00216B80
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character to check.", "", false)]
		public Identifier TargetTag { get; set; } = Identifier.Empty;

		// Token: 0x17000ED8 RID: 3800
		// (get) Token: 0x060038A2 RID: 14498 RVA: 0x00218989 File Offset: 0x00216B89
		// (set) Token: 0x060038A3 RID: 14499 RVA: 0x00218991 File Offset: 0x00216B91
		[Serialize("", IsPropertySaveable.Yes, "Tag referring to the character who caused the affliction. Can be used to require the affliction to be caused by a specific character.", "", false)]
		public Identifier SourceCharacter { get; set; } = Identifier.Empty;

		// Token: 0x17000ED9 RID: 3801
		// (get) Token: 0x060038A4 RID: 14500 RVA: 0x0021899A File Offset: 0x00216B9A
		// (set) Token: 0x060038A5 RID: 14501 RVA: 0x002189A2 File Offset: 0x00216BA2
		[Serialize(LimbType.None, IsPropertySaveable.Yes, "Only check afflictions on the specified limb type.", "", false)]
		public LimbType TargetLimb { get; set; }

		// Token: 0x17000EDA RID: 3802
		// (get) Token: 0x060038A6 RID: 14502 RVA: 0x002189AB File Offset: 0x00216BAB
		// (set) Token: 0x060038A7 RID: 14503 RVA: 0x002189B3 File Offset: 0x00216BB3
		[Serialize(true, IsPropertySaveable.Yes, "When set to false, limb-specific afflictions are ignored when not checking a specific limb.", "", false)]
		public bool AllowLimbAfflictions { get; set; }

		// Token: 0x17000EDB RID: 3803
		// (get) Token: 0x060038A8 RID: 14504 RVA: 0x002189BC File Offset: 0x00216BBC
		// (set) Token: 0x060038A9 RID: 14505 RVA: 0x002189C4 File Offset: 0x00216BC4
		[Serialize(0f, IsPropertySaveable.Yes, "Minimum strength of the affliction.", "", false)]
		public float MinStrength { get; set; }

		// Token: 0x060038AA RID: 14506 RVA: 0x002189CD File Offset: 0x00216BCD
		public CheckAfflictionAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x060038AB RID: 14507 RVA: 0x002189F8 File Offset: 0x00216BF8
		protected override bool? DetermineSuccess()
		{
			if (this.Identifier.IsEmpty || this.TargetTag.IsEmpty)
			{
				return new bool?(false);
			}
			List<Character> targets = this.ParentEvent.GetTargets(this.TargetTag).OfType<Character>().ToList<Character>();
			using (List<Character>.Enumerator enumerator = targets.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Character target = enumerator.Current;
					if (target.CharacterHealth != null)
					{
						if (this.TargetLimb == LimbType.None && target.CharacterHealth.GetAfflictionStrengthByIdentifier(this.Identifier, this.AllowLimbAfflictions) >= this.MinStrength)
						{
							return new bool?(true);
						}
						IEnumerable<Affliction> afflictions = target.CharacterHealth.GetAllAfflictions().Where(delegate(Affliction affliction)
						{
							if (affliction.Prefab.LimbSpecific)
							{
								Limb afflictionLimb = target.CharacterHealth.GetAfflictionLimb(affliction);
								LimbType? limbType = (afflictionLimb != null) ? new LimbType?(afflictionLimb.type) : null;
								if (limbType != null)
								{
									LimbType? limbType2 = limbType;
									LimbType targetLimb = this.TargetLimb;
									if (limbType2.GetValueOrDefault() == targetLimb & limbType2 != null)
									{
										goto IL_65;
									}
								}
								return false;
							}
							IL_65:
							return (this.SourceCharacter.IsEmpty || this.ParentEvent.GetTargets(this.SourceCharacter).Contains(affliction.Source)) && affliction.Strength >= this.MinStrength;
						});
						if (afflictions.Any(delegate(Affliction a)
						{
							Identifier identifier = a.Identifier;
							Identifier identifier2 = this.Identifier;
							return identifier == identifier2;
						}))
						{
							return new bool?(true);
						}
					}
				}
			}
			return new bool?(false);
		}

		// Token: 0x060038AC RID: 14508 RVA: 0x00218B28 File Offset: 0x00216D28
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(69, 6);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(base.HasBeenDetermined(), false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("CheckAfflictionAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (TargetTag: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.TargetTag.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendLiteral("AfflictionIdentifier: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Identifier.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendLiteral("TargetLimb: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.TargetLimb.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendLiteral("Succeeded: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.succeeded.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}
	}
}
