using System;

namespace Barotrauma
{
	// Token: 0x02000175 RID: 373
	public enum StatTypes
	{
		// Token: 0x04000DBA RID: 3514
		None,
		// Token: 0x04000DBB RID: 3515
		ElectricalSkillBonus,
		// Token: 0x04000DBC RID: 3516
		HelmSkillBonus,
		// Token: 0x04000DBD RID: 3517
		MechanicalSkillBonus,
		// Token: 0x04000DBE RID: 3518
		MedicalSkillBonus,
		// Token: 0x04000DBF RID: 3519
		WeaponsSkillBonus,
		// Token: 0x04000DC0 RID: 3520
		HelmSkillOverride,
		// Token: 0x04000DC1 RID: 3521
		MedicalSkillOverride,
		// Token: 0x04000DC2 RID: 3522
		WeaponsSkillOverride,
		// Token: 0x04000DC3 RID: 3523
		ElectricalSkillOverride,
		// Token: 0x04000DC4 RID: 3524
		MechanicalSkillOverride,
		// Token: 0x04000DC5 RID: 3525
		MaximumHealthMultiplier,
		// Token: 0x04000DC6 RID: 3526
		MovementSpeed,
		// Token: 0x04000DC7 RID: 3527
		WalkingSpeed,
		// Token: 0x04000DC8 RID: 3528
		SwimmingSpeed,
		// Token: 0x04000DC9 RID: 3529
		PropulsionSpeed,
		// Token: 0x04000DCA RID: 3530
		BuffDurationMultiplier,
		// Token: 0x04000DCB RID: 3531
		DebuffDurationMultiplier,
		// Token: 0x04000DCC RID: 3532
		MedicalItemEffectivenessMultiplier,
		// Token: 0x04000DCD RID: 3533
		FlowResistance,
		// Token: 0x04000DCE RID: 3534
		AttackMultiplier,
		// Token: 0x04000DCF RID: 3535
		TeamAttackMultiplier,
		// Token: 0x04000DD0 RID: 3536
		RangedAttackSpeed,
		// Token: 0x04000DD1 RID: 3537
		RangedAttackMultiplier,
		// Token: 0x04000DD2 RID: 3538
		TurretAttackSpeed,
		// Token: 0x04000DD3 RID: 3539
		TurretPowerCostReduction,
		// Token: 0x04000DD4 RID: 3540
		TurretChargeSpeed,
		// Token: 0x04000DD5 RID: 3541
		MeleeAttackSpeed,
		// Token: 0x04000DD6 RID: 3542
		MeleeAttackMultiplier,
		// Token: 0x04000DD7 RID: 3543
		RangedSpreadReduction,
		// Token: 0x04000DD8 RID: 3544
		RepairSpeed,
		// Token: 0x04000DD9 RID: 3545
		MechanicalRepairSpeed,
		// Token: 0x04000DDA RID: 3546
		ElectricalRepairSpeed,
		// Token: 0x04000DDB RID: 3547
		DeconstructorSpeedMultiplier,
		// Token: 0x04000DDC RID: 3548
		RepairToolStructureRepairMultiplier,
		// Token: 0x04000DDD RID: 3549
		RepairToolStructureDamageMultiplier,
		// Token: 0x04000DDE RID: 3550
		RepairToolDeattachTimeMultiplier,
		// Token: 0x04000DDF RID: 3551
		MaxRepairConditionMultiplierMechanical,
		// Token: 0x04000DE0 RID: 3552
		MaxRepairConditionMultiplierElectrical,
		// Token: 0x04000DE1 RID: 3553
		IncreaseFabricationQuality,
		// Token: 0x04000DE2 RID: 3554
		GeneticMaterialRefineBonus,
		// Token: 0x04000DE3 RID: 3555
		GeneticMaterialTaintedProbabilityReductionOnCombine,
		// Token: 0x04000DE4 RID: 3556
		SkillGainSpeed,
		// Token: 0x04000DE5 RID: 3557
		ExtraLevelGain,
		// Token: 0x04000DE6 RID: 3558
		HelmSkillGainSpeed,
		// Token: 0x04000DE7 RID: 3559
		WeaponsSkillGainSpeed,
		// Token: 0x04000DE8 RID: 3560
		MedicalSkillGainSpeed,
		// Token: 0x04000DE9 RID: 3561
		ElectricalSkillGainSpeed,
		// Token: 0x04000DEA RID: 3562
		MechanicalSkillGainSpeed,
		// Token: 0x04000DEB RID: 3563
		MedicalItemApplyingMultiplier,
		// Token: 0x04000DEC RID: 3564
		BuffItemApplyingMultiplier,
		// Token: 0x04000DED RID: 3565
		PoisonMultiplier,
		// Token: 0x04000DEE RID: 3566
		TinkeringDuration,
		// Token: 0x04000DEF RID: 3567
		TinkeringStrength,
		// Token: 0x04000DF0 RID: 3568
		TinkeringDamage,
		// Token: 0x04000DF1 RID: 3569
		ReputationGainMultiplier,
		// Token: 0x04000DF2 RID: 3570
		ReputationLossMultiplier,
		// Token: 0x04000DF3 RID: 3571
		MissionMoneyGainMultiplier,
		// Token: 0x04000DF4 RID: 3572
		ExperienceGainMultiplier,
		// Token: 0x04000DF5 RID: 3573
		MissionExperienceGainMultiplier,
		// Token: 0x04000DF6 RID: 3574
		ExtraMissionCount,
		// Token: 0x04000DF7 RID: 3575
		ExtraSpecialSalesCount,
		// Token: 0x04000DF8 RID: 3576
		StoreSellMultiplier,
		// Token: 0x04000DF9 RID: 3577
		StoreBuyMultiplierAffiliated,
		// Token: 0x04000DFA RID: 3578
		StoreBuyMultiplier,
		// Token: 0x04000DFB RID: 3579
		ShipyardBuyMultiplierAffiliated,
		// Token: 0x04000DFC RID: 3580
		ShipyardBuyMultiplier,
		// Token: 0x04000DFD RID: 3581
		MaxAttachableCount,
		// Token: 0x04000DFE RID: 3582
		ExplosionRadiusMultiplier,
		// Token: 0x04000DFF RID: 3583
		ExplosionDamageMultiplier,
		// Token: 0x04000E00 RID: 3584
		FabricationSpeed,
		// Token: 0x04000E01 RID: 3585
		BallastFloraDamageMultiplier,
		// Token: 0x04000E02 RID: 3586
		HoldBreathMultiplier,
		// Token: 0x04000E03 RID: 3587
		Apprenticeship,
		// Token: 0x04000E04 RID: 3588
		CPRBoost,
		// Token: 0x04000E05 RID: 3589
		LockedTalents,
		// Token: 0x04000E06 RID: 3590
		HireCostMultiplier,
		// Token: 0x04000E07 RID: 3591
		InventoryExtraStackSize,
		// Token: 0x04000E08 RID: 3592
		SoundRangeMultiplier,
		// Token: 0x04000E09 RID: 3593
		SightRangeMultiplier,
		// Token: 0x04000E0A RID: 3594
		DualWieldingPenaltyReduction,
		// Token: 0x04000E0B RID: 3595
		NaturalMeleeAttackMultiplier,
		// Token: 0x04000E0C RID: 3596
		NaturalRangedAttackMultiplier
	}
}
