﻿using ECommons.DalamudServices;
using ECommons.GameHelpers;
using RotationSolver.Extensions;
using RotationSolver.Helpers;
using RotationSolver.Updaters;

namespace RotationSolver.Commands
{
    public static partial class RSCommands
    {
        public static string _stateString = "Off", _specialString = string.Empty;

        internal static string EntryString => _stateString + (DataCenter.SpecialTimeLeft < 0 ? string.Empty : $" - {_specialString}: {DataCenter.SpecialTimeLeft:F2}s");

        private static void UpdateToast()
        {
            if (!Service.Config.ShowInfoOnToast) return;

            Svc.Toasts.ShowQuest(" " + EntryString, new Dalamud.Game.Gui.Toast.QuestToastOptions()
            {
                IconId = 101,
            });
        }

        public static unsafe void DoStateCommandType(StateCommandType stateType, int index = -1) => DoOneCommandType((type, role) => type.ToStateString(role), role =>
        {
            if (DataCenter.State)
            {
                if (DataCenter.IsManual && stateType == StateCommandType.Manual
                    && Service.Config.ToggleManual)
                {
                    stateType = StateCommandType.Off;
                }
                else if (stateType == StateCommandType.Auto)
                {
                    if (Service.Config.ToggleAuto)
                    {
                        stateType = StateCommandType.Off;
                    }
                    else
                    {
                        if (index == -1)
                        {
                            // Increment the TargetingIndex to cycle through the TargetingTypes
                            index = Service.Config.TargetingIndex + 1;
                        }
                        // Ensure the index wraps around if it exceeds the number of TargetingTypes
                        index %= Service.Config.TargetingTypes.Count;
                        // Update the TargetingIndex in the configuration
                        Service.Config.TargetingIndex = index;
                    }
                }
            }

            switch (stateType)
            {
                case StateCommandType.Off:
                    DataCenter.State = false;
                    DataCenter.IsManual = false;
                    ActionUpdater.NextAction = ActionUpdater.NextGCDAction = null;
                    break;

                case StateCommandType.Auto:
                    DataCenter.IsManual = false;
                    DataCenter.State = true;
                    ActionUpdater.AutoCancelTime = DateTime.MinValue;
                    break;

                case StateCommandType.Manual:
                    DataCenter.IsManual = true;
                    DataCenter.State = true;
                    ActionUpdater.AutoCancelTime = DateTime.MinValue;
                    break;
            }

            _stateString = stateType.ToStateString(role);
            UpdateToast();
            return stateType;
        });

        private static void DoSpecialCommandType(SpecialCommandType specialType, bool sayout = true) => DoOneCommandType((type, role) => type.ToSpecialString(role), role =>
        {
            _specialString = specialType.ToSpecialString(role);
            DataCenter.SpecialType = specialType;
            if (sayout) UpdateToast();
            return specialType;
        });

        private static void DoOneCommandType<T>(Func<T, JobRole, string> sayout, Func<JobRole, T> doingSomething)
            where T : struct, Enum
        {
            //Get job role.
            var role = Player.Object?.ClassJob.Value.GetJobRole() ?? JobRole.None;

            if (role == JobRole.None) return;

            T type = doingSomething(role);

            //Saying out.
            if (Service.Config.SayOutStateChanged) SpeechHelper.Speak(sayout(type, role));
        }
    }
}
