﻿using RotationSolver.Updaters;

namespace RotationSolver.UI;

internal class CooldownWindow() : CtrlWindow(nameof(CooldownWindow))
{
    public override void Draw()
    {
        if (DataCenter.CurrentRotation == null) return;

        var config = Service.Config;
        var width = config.CooldownWindowIconSize;
        const float IconSpacingFactor = 6f / 82f;
        var style = ImGui.GetStyle();
        var columnWidth = ImGui.GetColumnWidth();
        var count = Math.Max(1, (int)MathF.Floor(columnWidth / (width * (1 + IconSpacingFactor) + style.ItemSpacing.X)));

        var allGroupedActions = RotationUpdater.AllGroupedActions;
        if (allGroupedActions == null) return;

        foreach (var pair in allGroupedActions)
        {
            // Manual filtering
            var showItems = new List<IAction>();
            foreach (var a in pair)
            {
                if (!a.IsInCooldown) continue;
                if (a is IBaseAction b1 && b1.Info.IsLimitBreak) continue;
                if (!config.ShowGcdCooldown && a is IBaseAction b2 && b2.Info.IsGeneralGCD) continue;
                if (!config.ShowItemsCooldown && a is IBaseItem) continue;
                showItems.Add(a);
            }

            // Manual sorting by SortKey
            showItems.Sort((x, y) => x.SortKey.CompareTo(y.SortKey));

            if (showItems.Count == 0) continue;

            ImGui.Text(pair.Key);

            ImGui.Columns(count, null, false);
            uint itemIndex = 0;
            foreach (var item in showItems)
            {
                try
                {
                    ControlWindow.DrawIAction(item, width, 1f);
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    Console.WriteLine($"Error drawing action: {ex.Message}");
                }
                itemIndex++;
                if (itemIndex % count != 0)
                {
                    ImGui.NextColumn();
                }
            }
            ImGui.Columns(1);
            ImGui.NewLine();
        }
    }
}