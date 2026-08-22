using System;

namespace Barotrauma
{
	// Token: 0x02000269 RID: 617
	public enum StatTypes
	{
		// Token: 0x04001CB1 RID: 7345
		None,
		// Token: 0x04001CB2 RID: 7346
		ElectricalSkillBonus,
		// Token: 0x04001CB3 RID: 7347
		HelmSkillBonus,
		// Token: 0x04001CB4 RID: 7348
		MechanicalSkillBonus,
		// Token: 0x04001CB5 RID: 7349
		MedicalSkillBonus,
		// Token: 0x04001CB6 RID: 7350
		WeaponsSkillBonus,
		// Token: 0x04001CB7 RID: 7351
		HelmSkillOverride,
		// Token: 0x04001CB8 RID: 7352
		MedicalSkillOverride,
		// Token: 0x04001CB9 RID: 7353
		WeaponsSkillOverride,
		// Token: 0x04001CBA RID: 7354
		ElectricalSkillOverride,
		// Token: 0x04001CBB RID: 7355
		MechanicalSkillOverride,
		// Token: 0x04001CBC RID: 7356
		MaximumHealthMultiplier,
		// Token: 0x04001CBD RID: 7357
		MovementSpeed,
		// Token: 0x04001CBE RID: 7358
		WalkingSpeed,
		// Token: 0x04001CBF RID: 7359
		SwimmingSpeed,
		// Token: 0x04001CC0 RID: 7360
		PropulsionSpeed,
		// Token: 0x04001CC1 RID: 7361
		BuffDurationMultiplier,
		// Token: 0x04001CC2 RID: 7362
		DebuffDurationMultiplier,
		// Token: 0x04001CC3 RID: 7363
		MedicalItemEffectivenessMultiplier,
		// Token: 0x04001CC4 RID: 7364
		FlowResistance,
		// Token: 0x04001CC5 RID: 7365
		AttackMultiplier,
		// Token: 0x04001CC6 RID: 7366
		TeamAttackMultiplier,
		// Token: 0x04001CC7 RID: 7367
		RangedAttackSpeed,
		// Token: 0x04001CC8 RID: 7368
		RangedAttackMultiplier,
		// Token: 0x04001CC9 RID: 7369
		TurretAttackSpeed,
		// Token: 0x04001CCA RID: 7370
		TurretPowerCostReduction,
		// Token: 0x04001CCB RID: 7371
		TurretChargeSpeed,
		// Token: 0x04001CCC RID: 7372
		MeleeAttackSpeed,
		// Token: 0x04001CCD RID: 7373
		MeleeAttackMultiplier,
		// Token: 0x04001CCE RID: 7374
		RangedSpreadReduction,
		// Token: 0x04001CCF RID: 7375
		RepairSpeed,
		// Token: 0x04001CD0 RID: 7376
		MechanicalRepairSpeed,
		// Token: 0x04001CD1 RID: 7377
		ElectricalRepairSpeed,
		// Token: 0x04001CD2 RID: 7378
		DeconstructorSpeedMultiplier,
		// Token: 0x04001CD3 RID: 7379
		RepairToolStructureRepairMultiplier,
		// Token: 0x04001CD4 RID: 7380
		RepairToolStructureDamageMultiplier,
		// Token: 0x04001CD5 RID: 7381
		RepairToolDeattachTimeMultiplier,
		// Token: 0x04001CD6 RID: 7382
		MaxRepairConditionMultiplierMechanical,
		// Token: 0x04001CD7 RID: 7383
		MaxRepairConditionMultiplierElectrical,
		// Token: 0x04001CD8 RID: 7384
		IncreaseFabricationQuality,
		// Token: 0x04001CD9 RID: 7385
		GeneticMaterialRefineBonus,
		// Token: 0x04001CDA RID: 7386
		GeneticMaterialTaintedProbabilityReductionOnCombine,
		// Token: 0x04001CDB RID: 7387
		SkillGainSpeed,
		// Token: 0x04001CDC RID: 7388
		ExtraLevelGain,
		// Token: 0x04001CDD RID: 7389
		HelmSkillGainSpeed,
		// Token: 0x04001CDE RID: 7390
		WeaponsSkillGainSpeed,
		// Token: 0x04001CDF RID: 7391
		MedicalSkillGainSpeed,
		// Token: 0x04001CE0 RID: 7392
		ElectricalSkillGainSpeed,
		// Token: 0x04001CE1 RID: 7393
		MechanicalSkillGainSpeed,
		// Token: 0x04001CE2 RID: 7394
		MedicalItemApplyingMultiplier,
		// Token: 0x04001CE3 RID: 7395
		BuffItemApplyingMultiplier,
		// Token: 0x04001CE4 RID: 7396
		PoisonMultiplier,
		// Token: 0x04001CE5 RID: 7397
		TinkeringDuration,
		// Token: 0x04001CE6 RID: 7398
		TinkeringStrength,
		// Token: 0x04001CE7 RID: 7399
		TinkeringDamage,
		// Token: 0x04001CE8 RID: 7400
		ReputationGainMultiplier,
		// Token: 0x04001CE9 RID: 7401
		ReputationLossMultiplier,
		// Token: 0x04001CEA RID: 7402
		MissionMoneyGainMultiplier,
		// Token: 0x04001CEB RID: 7403
		ExperienceGainMultiplier,
		// Token: 0x04001CEC RID: 7404
		MissionExperienceGainMultiplier,
		// Token: 0x04001CED RID: 7405
		ExtraMissionCount,
		// Token: 0x04001CEE RID: 7406
		ExtraSpecialSalesCount,
		// Token: 0x04001CEF RID: 7407
		StoreSellMultiplier,
		// Token: 0x04001CF0 RID: 7408
		StoreBuyMultiplierAffiliated,
		// Token: 0x04001CF1 RID: 7409
		StoreBuyMultiplier,
		// Token: 0x04001CF2 RID: 7410
		ShipyardBuyMultiplierAffiliated,
		// Token: 0x04001CF3 RID: 7411
		ShipyardBuyMultiplier,
		// Token: 0x04001CF4 RID: 7412
		MaxAttachableCount,
		// Token: 0x04001CF5 RID: 7413
		ExplosionRadiusMultiplier,
		// Token: 0x04001CF6 RID: 7414
		ExplosionDamageMultiplier,
		// Token: 0x04001CF7 RID: 7415
		FabricationSpeed,
		// Token: 0x04001CF8 RID: 7416
		BallastFloraDamageMultiplier,
		// Token: 0x04001CF9 RID: 7417
		HoldBreathMultiplier,
		// Token: 0x04001CFA RID: 7418
		Apprenticeship,
		// Token: 0x04001CFB RID: 7419
		CPRBoost,
		// Token: 0x04001CFC RID: 7420
		LockedTalents,
		// Token: 0x04001CFD RID: 7421
		HireCostMultiplier,
		// Token: 0x04001CFE RID: 7422
		InventoryExtraStackSize,
		// Token: 0x04001CFF RID: 7423
		SoundRangeMultiplier,
		// Token: 0x04001D00 RID: 7424
		SightRangeMultiplier,
		// Token: 0x04001D01 RID: 7425
		DualWieldingPenaltyReduction,
		// Token: 0x04001D02 RID: 7426
		NaturalMeleeAttackMultiplier,
		// Token: 0x04001D03 RID: 7427
		NaturalRangedAttackMultiplier
	}
}
