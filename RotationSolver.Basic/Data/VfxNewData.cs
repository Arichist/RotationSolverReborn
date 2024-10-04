﻿using ECommons.DalamudServices;

namespace RotationSolver.Basic.Data;

/// <summary>
/// Represents new VFX data.
/// </summary>
public readonly record struct VfxNewData
{
    /// <summary>
    /// Gets the object ID.
    /// </summary>
    public readonly ulong ObjectId;

    /// <summary>
    /// Gets the path.
    /// </summary>
    public readonly string Path;

    /// <summary>
    /// Gets the time when the VFX data was created.
    /// </summary>
    public readonly DateTime Time;

    /// <summary>
    /// Initializes a new instance of the <see cref="VfxNewData"/> struct.
    /// </summary>
    /// <param name="objectId">The object ID.</param>
    /// <param name="path">The path.</param>
    public VfxNewData(ulong objectId, string path)
    {
        ObjectId = objectId;
        Path = path;
        Time = DateTime.Now;
    }

    /// <summary>
    /// Gets the duration since the VFX data was created.
    /// </summary>
    public TimeSpan TimeDuration => DateTime.Now - Time;

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => $"Object Effect: {Svc.Objects.SearchById(ObjectId)?.Name ?? "Object"}: {Path}, {TimeDuration.TotalSeconds}";
}