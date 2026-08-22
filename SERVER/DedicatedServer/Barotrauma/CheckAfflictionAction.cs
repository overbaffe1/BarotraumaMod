using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000185 RID: 389
	[NullableContext(1)]
	[Nullable(0)]
	internal class CheckAfflictionAction : BinaryOptionAction
	{
		// Token: 0x17000882 RID: 2178
		// (get) Token: 0x06001DD2 RID: 7634 RVA: 0x000D33B3 File Offset: 0x000D15B3
		// (set) Token: 0x06001DD3 RID: 7635 RVA: 0x000D33BB File Offset: 0x000D15BB
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the affliction.", "", false)]
		public Identifier Identifier { get; set; } = Identifier.Empty;

		// Token: 0x17000883 RID: 2179
		// (get) Token: 0x06001DD4 RID: 7636 RVA: 0x000D33C4 File Offset: 0x000D15C4
		// (set) Token: 0x06001DD5 RID: 7637 RVA: 0x000D33CC File Offset: 0x000D15CC
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character to check.", "", false)]
		public Identifier TargetTag { get; set; } = Identifier.Empty;

		// Token: 0x17000884 RID: 2180
		// (get) Token: 0x06001DD6 RID: 7638 RVA: 0x000D33D5 File Offset: 0x000D15D5
		// (set) Token: 0x06001DD7 RID: 7639 RVA: 0x000D33DD File Offset: 0x000D15DD
		[Serialize("", IsPropertySaveable.Yes, "Tag referring to the character who caused the affliction. Can be used to require the affliction to be caused by a specific character.", "", false)]
		public Identifier SourceCharacter { get; set; } = Identifier.Empty;

		// Token: 0x17000885 RID: 2181
		// (get) Token: 0x06001DD8 RID: 7640 RVA: 0x000D33E6 File Offset: 0x000D15E6
		// (set) Token: 0x06001DD9 RID: 7641 RVA: 0x000D33EE File Offset: 0x000D15EE
		[Serialize(LimbType.None, IsPropertySaveable.Yes, "Only check afflictions on the specified limb type.", "", false)]
		public LimbType TargetLimb { get; set; }

		// Token: 0x17000886 RID: 2182
		// (get) Token: 0x06001DDA RID: 7642 RVA: 0x000D33F7 File Offset: 0x000D15F7
		// (set) Token: 0x06001DDB RID: 7643 RVA: 0x000D33FF File Offset: 0x000D15FF
		[Serialize(true, IsPropertySaveable.Yes, "When set to false, limb-specific afflictions are ignored when not checking a specific limb.", "", false)]
		public bool AllowLimbAfflictions { get; set; }

		// Token: 0x17000887 RID: 2183
		// (get) Token: 0x06001DDC RID: 7644 RVA: 0x000D3408 File Offset: 0x000D1608
		// (set) Token: 0x06001DDD RID: 7645 RVA: 0x000D3410 File Offset: 0x000D1610
		[Serialize(0f, IsPropertySaveable.Yes, "Minimum strength of the affliction.", "", false)]
		public float MinStrength { get; set; }

		// Token: 0x06001DDE RID: 7646 RVA: 0x000D3419 File Offset: 0x000D1619
		public CheckAfflictionAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06001DDF RID: 7647 RVA: 0x000D3444 File Offset: 0x000D1644
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

		// Token: 0x06001DE0 RID: 7648 RVA: 0x000D3574 File Offset: 0x000D1774
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
