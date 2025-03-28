using Lumina.Excel.Sheets;
using System.Text;
using System.Text.RegularExpressions;

namespace RotationSolver.GameData;
/// <summary>
/// Utility class for various helper methods.
/// </summary>
internal static partial class Util
{
    /// <summary>
    /// Determines if the job category represents a single job for combat.
    /// </summary>
    /// <param name="jobCategory">The job category to check.</param>
    /// <returns>True if the job category represents a single job for combat; otherwise, false.</returns>
    public static bool IsSingleJobForCombat(this ClassJobCategory jobCategory)
    {
        if (jobCategory.RowId == 68) return true; // ACN SMN SCH 
        var str = jobCategory.Name.ExtractText().Replace(" ", "");
        if (!str.All(char.IsUpper)) return false;
        if (str.Length is not 3 and not 6) return false;
        return true;
    }

    /// <summary>
    /// Indents each line of the string with four spaces.
    /// </summary>
    /// <param name="str">The string to indent.</param>
    /// <returns>The indented string.</returns>
    public static string Table(this string str) => "    " + str.Replace("\n", "\n    ");

    /// <summary>
    /// Inserts spaces before each uppercase letter in the string.
    /// </summary>
    /// <param name="str">The string to modify.</param>
    /// <returns>The modified string with spaces.</returns>
    public static string Space(this string str)
    {
        var result = new StringBuilder();
        bool lower = false;

        foreach (var c in str)
        {
            var isLower = char.IsLower(c);
            if (lower && !isLower)
            {
                result.Append(' ');
            }
            lower = isLower;
            result.Append(c);
        }

        return result.ToString();
    }

    /// <summary>
    /// Removes non-ASCII characters from the string.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <returns>The string containing only ASCII characters.</returns>
    public static string OnlyAscii(this string input) => new(input.Where(char.IsAscii).ToArray());

    /// <summary>
    /// Converts the string to PascalCase.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <returns>The PascalCase string.</returns>
    public static string ToPascalCase(this string input)
    {
        var pascalCase = InvalidCharsRgx().Replace(WhiteSpace().Replace(input, "_"), string.Empty)
            .Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(w => StartWithLowerCaseChar().Replace(w, m => m.Value.ToUpper()))
            .Select(w => FirstCharFollowedByUpperCasesOnly().Replace(w, m => m.Value.ToLower()))
            .Select(w => LowerCaseNextToNumber().Replace(w, m => m.Value.ToUpper()))
            .Select(w => UpperCaseInside().Replace(w, m => m.Value.ToLower()));

        var result = string.Concat(pascalCase);

        if (result.Length > 0 && char.IsNumber(result[0]))
        {
            result = "_" + result;
        }
        return result;
    }

    [GeneratedRegex("[^_a-zA-Z0-9]")]
    private static partial Regex InvalidCharsRgx();
    [GeneratedRegex("(?<=\\s)")]
    private static partial Regex WhiteSpace();
    [GeneratedRegex("^[a-z]")]
    private static partial Regex StartWithLowerCaseChar();
    [GeneratedRegex("(?<=[A-Z])[A-Z0-9]+$")]
    private static partial Regex FirstCharFollowedByUpperCasesOnly();
    [GeneratedRegex("(?<=[0-9])[a-z]")]
    private static partial Regex LowerCaseNextToNumber();
    [GeneratedRegex("(?<=[A-Z])[A-Z]+?((?=[A-Z][a-z])|(?=[0-9]))")]
    private static partial Regex UpperCaseInside();

    /// <summary>
    /// Generates a property with an array of names.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="propertyType">The type of the property.</param>
    /// <param name="modifier">The modifier for the property.</param>
    /// <param name="items">The items to include in the array.</param>
    /// <returns>The generated property code.</returns>
    public static string ArrayNames(string propertyName, string propertyType, string modifier, params string[] items)
    {
        var thisItems = $"""
            [
                {string.Join(", ", items)},
                {(modifier.Contains("override") ? $"..base.{propertyName}," : string.Empty)}
            ]
            """;
        return $$"""
        private {{propertyType}}[] _{{propertyName}} = null;

        /// <inheritdoc/>
        {{modifier}} {{propertyType}}[] {{propertyName}} => _{{propertyName}} ??= {{thisItems}};
        """;
    }

    /// <summary>
    /// Generates code for an action.
    /// </summary>
    /// <param name="item">The action item.</param>
    /// <param name="actionName">The name of the action.</param>
    /// <param name="actionDescName">The description name of the action.</param>
    /// <param name="desc">The description of the action.</param>
    /// <param name="isDuty">Indicates if the action is a duty action.</param>
    /// <returns>The generated action code.</returns>
    public static string ToCode(this Lumina.Excel.Sheets.Action item,
        string actionName, string actionDescName, string desc, bool isDuty)
    {
        if (isDuty)
        {
            actionDescName += " Duty Action";
        }

        return $$"""
        private readonly Lazy<IBaseAction> _{{actionName}}Creator = new(() => 
        {
            IBaseAction action = new BaseAction((ActionID){{item.RowId}}, {{isDuty.ToString().ToLower()}});
            CustomRotation.LoadActionSetting(ref action);

            var setting = action.Setting;
            Modify{{actionName}}(ref setting);
            action.Setting = setting;

            return action;
        });

        /// <summary>
        /// Modify {{actionDescName}}
        /// </summary>
        static partial void Modify{{actionName}}(ref ActionSetting setting);

        /// <summary>
        /// {{actionDescName}}
        /// {{desc}}
        /// </summary>
        {{(isDuty ? $"[ID({item.RowId})]" : string.Empty)}}
        {{(item.ActionCategory.Value.RowId is 15 ? "private" : "public")}} IBaseAction {{actionName}} => _{{actionName}}Creator.Value;
        """;    
    }

    /// <summary>
    /// Gets the description name for an action.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <returns>The description name of the action.</returns>
    public static string GetDescName(this Lumina.Excel.Sheets.Action action)
    {
        var jobs = action.ClassJobCategory.Value.Name.ExtractText();
        jobs = string.IsNullOrEmpty(jobs) ? string.Empty : $" ({jobs})";

        var cate = action.IsPvP ? " <i>PvP</i>" : " <i>PvE</i>";

        return $"<see href=\"https://garlandtools.org/db/#action/{action.RowId}\"><strong>{action.Name.ExtractText()}</strong></see>{cate}{jobs} [{action.RowId}] [{action.ActionCategory.Value.Name.ExtractText() ?? string.Empty}]";
    }
}