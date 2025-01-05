﻿namespace RotationSolver.Basic.Rotations.Basic;

partial class DancerRotation
{
    /// <summary>
    /// 
    /// </summary>
    public override MedicineType MedicineType => MedicineType.Dexterity;

    /// <summary>
    /// 
    /// </summary>
    public static bool IsDancing => JobGauge.IsDancing;

    /// <summary>
    /// 
    /// </summary>
    public static byte Esprit => JobGauge.Esprit;

    /// <summary>
    /// 
    /// </summary>
    public static byte Feathers => JobGauge.Feathers;

    /// <summary>
    /// 
    /// </summary>
    public static byte CompletedSteps => JobGauge.CompletedSteps;

    /// <inheritdoc/>
    public override void DisplayStatus()
    {
        ImGui.Text("IsDancing: " + IsDancing.ToString());
        ImGui.Text("Esprit: " + Esprit.ToString());
        ImGui.Text("Feathers: " + Feathers.ToString());
        ImGui.Text("CompletedSteps: " + CompletedSteps.ToString());
    }

    static partial void ModifyCascadePvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.SilkenSymmetry];
    }

    static partial void ModifyCuringWaltzPvE(ref ActionSetting setting)
    {
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyShieldSambaPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = StatusHelper.RangePhysicalDefense;
        setting.StatusFromSelf = false;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyImprovisationPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.Improvisation, StatusID.Improvisation_2695, StatusID.RisingRhythm];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyImprovisedFinishPvE(ref ActionSetting setting)
    {
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyFountainPvE(ref ActionSetting setting)
    {
        setting.ComboIds = [ActionID.CascadePvE];
        setting.StatusProvide = [StatusID.SilkenFlow];
    }

    static partial void ModifyWindmillPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.SilkenSymmetry];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyReverseCascadePvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.SilkenSymmetry, StatusID.FlourishingSymmetry];
    }

    static partial void ModifyFountainfallPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.SilkenFlow, StatusID.FlourishingFlow];
    }

    static partial void ModifyFanDancePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Feathers > 0;
        setting.StatusProvide = [StatusID.ThreefoldFanDance];
    }

    static partial void ModifyBladeshowerPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.SilkenFlow];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyRisingWindmillPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.SilkenSymmetry, StatusID.FlourishingSymmetry];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyBloodshowerPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.SilkenFlow, StatusID.FlourishingFlow];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyFanDanceIiPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Feathers > 0;
        setting.StatusProvide = [StatusID.ThreefoldFanDance];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 2,
        };
    }

    static partial void ModifyFanDanceIiiPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.ThreefoldFanDance];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyFanDanceIvPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.FourfoldFanDance];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifySaberDancePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Esprit >= 50;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyStarfallDancePvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.FlourishingStarfall];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyTillanaPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Esprit <= 50;
        setting.StatusNeed = [StatusID.FlourishingFinish];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyEnAvantPvE(ref ActionSetting setting)
    {
        setting.IsFriendly = true;
    }

    static partial void ModifyEnAvantPvP(ref ActionSetting setting)
    {
        setting.SpecialType = SpecialActionType.MovingForward;
    }

    static partial void ModifyClosedPositionPvE(ref ActionSetting setting)
    {
        setting.IsFriendly = true;
        setting.TargetType = TargetType.DancePartner;
        setting.ActionCheck = () => !IsDancing && !AllianceMembers.Any(b => b.HasStatus(true, StatusID.ClosedPosition_2026));
    }

    static partial void ModifyDevilmentPvE(ref ActionSetting setting)
    {
        setting.CreateConfig = () => new ActionConfig()
        {
            TimeToKill = 10,
        };
    }

    static partial void ModifyFlourishPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.StandardFinish];
        setting.StatusProvide = [StatusID.ThreefoldFanDance, StatusID.FourfoldFanDance, StatusID.FinishingMoveReady];
        setting.ActionCheck = () => InCombat;
    }

    static partial void ModifyTechnicalStepPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.StandardFinish];
        setting.UnlockedByQuestID = 68790;
    }

    static partial void ModifyDoubleTechnicalFinishPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.StandardStep, StatusID.TechnicalStep, StatusID.DanceOfTheDawnReady];
        setting.CreateConfig = () => new ActionConfig()
        {
            TimeToKill = 20,
            AoeCount = 1,
        };
    }

    static partial void ModifyDoubleStandardFinishPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.StandardStep];
        setting.StatusProvide = [StatusID.LastDanceReady];
        setting.ActionCheck = () => IsDancing && CompletedSteps == 2 && Service.GetAdjustedActionId(ActionID.StandardStepPvE) == ActionID.DoubleStandardFinishPvE;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyQuadrupleTechnicalFinishPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.TechnicalStep];
        setting.StatusProvide = [StatusID.DanceOfTheDawnReady];
        setting.ActionCheck = () => IsDancing && CompletedSteps == 4 && Service.GetAdjustedActionId(ActionID.TechnicalStepPvE) == ActionID.QuadrupleTechnicalFinishPvE;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyEmboitePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => (ActionID)JobGauge.NextStep == ActionID.EmboitePvE;
    }

    static partial void ModifyEntrechatPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => (ActionID)JobGauge.NextStep == ActionID.EntrechatPvE;
    }

    static partial void ModifyJetePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => (ActionID)JobGauge.NextStep == ActionID.JetePvE;
    }

    static partial void ModifyPirouettePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => (ActionID)JobGauge.NextStep == ActionID.PirouettePvE;
    }

    static partial void ModifyLastDancePvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.LastDanceReady];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
        //setting.ActionCheck = () => !IsDancing
    }

    static partial void ModifyFinishingMovePvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.FinishingMoveReady];
        setting.StatusProvide = [StatusID.LastDanceReady];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
        //setting.ActionCheck = () => !IsDancing
    }

    static partial void ModifyDanceOfTheDawnPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Esprit >= 50;
        setting.StatusNeed = [StatusID.DanceOfTheDawnReady];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
        //setting.ActionCheck = () => !IsDancing
    }

    #region Step
    /// <summary>
    /// Finish the dance.
    /// </summary>
    /// <param name="act"></param>
    /// <param name="finishNow">Finish the dance as soon as possible</param>
    /// <returns></returns>
    protected bool DanceFinishGCD(out IAction? act, bool finishNow = false)
    {
        if (Player.HasStatus(true, StatusID.StandardStep) && CompletedSteps == 2)
        {
            if (DoubleStandardFinishPvE.CanUse(out act, skipAoeCheck: true))
            {
                return true;
            }
            if (Player.WillStatusEnd(1, true, StatusID.StandardStep, StatusID.StandardFinish) || finishNow)
            {
                act = StandardStepPvE;
                return true;
            }
            return false;
        }

        if (Player.HasStatus(true, StatusID.TechnicalStep) && CompletedSteps == 4)
        {
            if (QuadrupleTechnicalFinishPvE.CanUse(out act, skipAoeCheck: true))
            {
                return true;
            }
            if (Player.WillStatusEnd(1, true, StatusID.TechnicalStep) || finishNow)
            {
                act = TechnicalStepPvE;
                return true;
            }
            return false;
        }

        act = null;
        return false;
    }

    /// <summary>
    /// Do the dancing steps.
    /// </summary>
    /// <param name="act"></param>
    /// <returns></returns>
    protected bool ExecuteStepGCD(out IAction? act)
    {
        if (!IsDancing)
        {
            act = null;
            return false;
        }

        if (EmboitePvE.CanUse(out act)) return true;
        if (EntrechatPvE.CanUse(out act)) return true;
        if (JetePvE.CanUse(out act)) return true;
        if (PirouettePvE.CanUse(out act)) return true;
        return false;
    }
    #endregion
}
