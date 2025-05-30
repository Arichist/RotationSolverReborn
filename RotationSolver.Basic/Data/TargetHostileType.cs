﻿namespace RotationSolver.Basic.Data;

/// <summary>
/// Hostile target.
/// </summary>
public enum TargetHostileType : byte
{
    /// <summary>
    /// All targets that are in range for any abilities (Tanks/Autoduty).
    /// </summary>
    [Description("All targets that are in range for any abilities (Tanks/Autoduty)")]
    AllTargetsCanAttack,

    /// <summary>
    /// Previously engaged targets (Non-Tanks).
    /// </summary>
    [Description("Previously engaged targets (Non-Tanks)")]
    TargetsHaveTarget,

    /// <summary>
    /// All targets when solo in duty, or previously engaged.
    /// </summary>
    [Description("All targets when solo in duty, or previously engaged.")]
    AllTargetsWhenSoloInDuty,

    /// <summary>
    /// All targets when solo, or previously engaged.
    /// </summary>
    [Description("All targets when solo, or previously engaged.")]
    AllTargetsWhenSolo,

    //[Description("Only attack targets in your parties enemy list")]
    //TargetIsInEnemiesList,

    //[Description("All targets when solo, or only attack targets in your parties enemy list")]
    //AllTargetsWhenSoloTargetIsInEnemiesList,

    //[Description("All targets when solo in duty, or only attack targets in your parties enemy list")]
    //AllTargetsWhenSoloInDutyTargetIsInEnemiesList,
}