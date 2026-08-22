using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000277 RID: 631
	internal class AfflictionAction : EventAction
	{
		// Token: 0x17000ED1 RID: 3793
		// (get) Token: 0x06003885 RID: 14469 RVA: 0x002183E6 File Offset: 0x002165E6
		// (set) Token: 0x06003886 RID: 14470 RVA: 0x002183EE File Offset: 0x002165EE
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the affliction.", "", false)]
		public Identifier Affliction { get; set; }

		// Token: 0x17000ED2 RID: 3794
		// (get) Token: 0x06003887 RID: 14471 RVA: 0x002183F7 File Offset: 0x002165F7
		// (set) Token: 0x06003888 RID: 14472 RVA: 0x002183FF File Offset: 0x002165FF
		[Serialize(0f, IsPropertySaveable.Yes, "Strength of the affliction.", "", false)]
		public float Strength { get; set; }

		// Token: 0x17000ED3 RID: 3795
		// (get) Token: 0x06003889 RID: 14473 RVA: 0x00218408 File Offset: 0x00216608
		// (set) Token: 0x0600388A RID: 14474 RVA: 0x00218410 File Offset: 0x00216610
		[Serialize(LimbType.None, IsPropertySaveable.Yes, "Type of the limb(s) to apply the affliction on. Only valid if the affliction is limb-specific.", "", false)]
		public LimbType LimbType { get; set; }

		// Token: 0x17000ED4 RID: 3796
		// (get) Token: 0x0600388B RID: 14475 RVA: 0x00218419 File Offset: 0x00216619
		// (set) Token: 0x0600388C RID: 14476 RVA: 0x00218421 File Offset: 0x00216621
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character to apply the affliction on.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x17000ED5 RID: 3797
		// (get) Token: 0x0600388D RID: 14477 RVA: 0x0021842A File Offset: 0x0021662A
		// (set) Token: 0x0600388E RID: 14478 RVA: 0x00218432 File Offset: 0x00216632
		[Serialize(false, IsPropertySaveable.Yes, "Should the strength be multiplied by the maximum vitality of the target?", "", false)]
		public bool MultiplyByMaxVitality { get; set; }

		// Token: 0x0600388F RID: 14479 RVA: 0x0021843C File Offset: 0x0021663C
		public AfflictionAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			if (this.Affliction.IsEmpty)
			{
				DebugConsole.ThrowError("Error in AfflictionAction: affliction not defined (use the attribute \"Affliction\").", null, element.ContentPackage, false, false);
			}
		}

		// Token: 0x06003890 RID: 14480 RVA: 0x00218474 File Offset: 0x00216674
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06003891 RID: 14481 RVA: 0x0021847C File Offset: 0x0021667C
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06003892 RID: 14482 RVA: 0x00218488 File Offset: 0x00216688
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			AfflictionPrefab afflictionPrefab;
			if (AfflictionPrefab.Prefabs.TryGet(this.Affliction, out afflictionPrefab))
			{
				IEnumerable<Entity> targets = this.ParentEvent.GetTargets(this.TargetTag);
				foreach (Entity target in targets)
				{
					if (target != null)
					{
						Character character = target as Character;
						if (character != null)
						{
							float strength = this.Strength;
							if (this.MultiplyByMaxVitality)
							{
								strength *= character.MaxVitality;
							}
							if (this.LimbType != LimbType.None)
							{
								Limb limb = character.AnimController.GetLimb(this.LimbType, true, false, false);
								if (strength > 0f)
								{
									character.CharacterHealth.ApplyAffliction(limb, afflictionPrefab.Instantiate(strength, null), true, true, true);
								}
								else if (strength < 0f)
								{
									character.CharacterHealth.ReduceAfflictionOnLimb(limb, this.Affliction, -strength, null, null);
								}
							}
							else if (strength > 0f)
							{
								character.CharacterHealth.ApplyAffliction(null, afflictionPrefab.Instantiate(strength, null), true, true, true);
							}
							else if (strength < 0f)
							{
								character.CharacterHealth.ReduceAfflictionOnAllLimbs(this.Affliction, -strength, null, null);
							}
						}
					}
				}
			}
			this.isFinished = true;
		}

		// Token: 0x06003893 RID: 14483 RVA: 0x00218604 File Offset: 0x00216804
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(56, 6);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.isFinished, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("AfflictionAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (TargetTag: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.TargetTag.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendLiteral("Affliction: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Affliction.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", Strength: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Strength.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendLiteral("LimbType: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.LimbType.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04001D56 RID: 7510
		private bool isFinished;
	}
}
