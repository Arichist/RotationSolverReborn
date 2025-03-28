using Lumina.Excel.Sheets;

namespace RotationSolver.GameData.Getters;

/// <summary>
/// Class responsible for getting trait rotations.
/// </summary>
internal class TraitRotationGetter : ExcelRowGetter<Trait>
{
    private readonly ClassJob _job;

    /// <summary>
    /// Initializes a new instance of the <see cref="TraitRotationGetter"/> class.
    /// </summary>
    /// <param name="gameData">The game data.</param>
    /// <param name="job">The class job.</param>
    public TraitRotationGetter(Lumina.GameData gameData, ClassJob job)
    : base(gameData)
    {
        if (job.RowId == 0)
        {
            throw new ArgumentNullException(nameof(job));
        }
        _job = job;
    }

    /// <summary>
    /// Gets the list of added names.
    /// </summary>
    public List<string> AddedNames { get; } = new List<string>();

    /// <summary>
    /// Called before creating the list of items.
    /// </summary>
    protected override void BeforeCreating()
    {
        AddedNames.Clear();
        base.BeforeCreating();
    }

    /// <summary>
    /// Determines whether the specified item should be added to the list.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <returns>True if the item should be added; otherwise, false.</returns>
    protected override bool AddToList(Trait item)
    {
        if (item.ClassJob.RowId == 0) return false;
        var name = item.Name.ExtractText();
        if (string.IsNullOrEmpty(name)) return false;
        if (!name.All(char.IsAscii)) return false;
        if (item.Icon == 0) return false;

        var category = item.ClassJob.Value;
        if (category.RowId == 0) return false;
        var jobName = _job.Abbreviation.ExtractText();
        return category.Abbreviation == jobName;
    }

    /// <summary>
    /// Converts the specified item to code.
    /// </summary>
    /// <param name="item">The item to convert.</param>
    /// <returns>The code representation of the item.</returns>
    protected override string ToCode(Trait item)
    {
        var name = item.Name.ExtractText().ToPascalCase() + "Trait";

        if (AddedNames.Contains(name))
        {
            name += "_" + item.RowId.ToString();
        }
        else
        {
            AddedNames.Add(name);
        }

        return $$"""
        /// <summary>
        /// {{GetDescName(item)}}
        /// {{GetDesc(item)}}
        /// </summary>
        public static IBaseTrait {{name}} { get; } = new BaseTrait({{item.RowId}});
        """;
    }

    /// <summary>
    /// Gets the description name for the specified item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The description name.</returns>
    private static string GetDescName(Trait item)
    {
        var jobs = item.ClassJobCategory.Value.Name.ExtractText();
        jobs = string.IsNullOrEmpty(jobs) ? string.Empty : $" ({jobs})";

        return $"<see href=\"https://garlandtools.org/db/#action/{50000 + item.RowId}\"><strong>{item.Name.ExtractText()}</strong></see>{jobs} [{item.RowId}]";
    }

    /// <summary>
    /// Gets the description for the specified item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The description.</returns>
    private string GetDesc(Trait item)
    {
        var desc = _gameData.GetExcelSheet<TraitTransient>()?.GetRow(item.RowId).Description.ExtractText() ?? string.Empty;

        return $"<para>{desc.Replace("\n", "</para>\n/// <para>")}</para>";
    }
}