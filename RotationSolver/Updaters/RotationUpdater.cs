﻿using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using Lumina.Excel.Sheets;
using RotationSolver.Basic.Rotations.Duties;
using RotationSolver.Data;
using RotationSolver.Helpers;


namespace RotationSolver.Updaters;

internal static class RotationUpdater
{
    internal record CustomRotationGroup(Job JobId, Job[] ClassJobIds, Type[] Rotations);
    internal static SortedList<JobRole, CustomRotationGroup[]> CustomRotationsDict { get; private set; } = [];

    internal static CustomRotationGroup[] CustomRotations { get; set; } = [];
    internal static SortedList<uint, Type[]> DutyRotations { get; set; } = [];

    public static IAction[] CurrentRotationActions { get; private set; } = [];

    private static DateTime LastRunTime;

    static bool _isLoading = false;

    public static Task ResetToDefaults()
    {
        try
        {
            var relayFolder = Svc.PluginInterface.ConfigDirectory.FullName + "\\Rotations";
            var files = Directory.GetFiles(relayFolder);
            foreach (var file in files)
            {
                Svc.Log.Information($"Deleting {file}");
                File.Delete(file);
            }
        }
        catch (Exception ex)
        {
            Svc.Log.Error(ex, "Failed to delete the rotation files");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves custom rotations from local and/or downloads
    /// them from remote server based on DownloadOption
    /// </summary>
    /// <param name="option"></param>
    /// <returns></returns>
    public static async Task GetAllCustomRotationsAsync(DownloadOption option)
    {
        if (_isLoading) return;
        _isLoading = true;

        try
        {
            var relayFolder = Svc.PluginInterface.ConfigDirectory.FullName + "\\Rotations";
            Directory.CreateDirectory(relayFolder);

            if (option.HasFlag(DownloadOption.Local))
            {
                LoadRotationsFromLocal(relayFolder);
            }

            if (option.HasFlag(DownloadOption.Download) && Service.Config.DownloadCustomRotations)
                await DownloadRotationsAsync(relayFolder, option.HasFlag(DownloadOption.MustDownload));

            if (option.HasFlag(DownloadOption.ShowList))
            {
                var assemblies = CustomRotationsDict
                    .SelectMany(d => d.Value)
                    .SelectMany(g => g.Rotations)
                    .Select(r => r.Assembly.FullName ?? string.Empty)
                    .Distinct()
                    .ToList();

                PrintLoadedAssemblies(assemblies);
            }
        }
        catch (Exception ex)
        {
#pragma warning disable 0436
            WarningHelper.AddSystemWarning($"Failed to load rotations because: {ex.Message}");
            Svc.Log.Error(ex, "Failed to get custom rotations");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private static Assembly? LoadDefaultRotationsFromLocal()
    {
        var directory = Svc.PluginInterface.AssemblyLocation.Directory;
        if (directory == null || !directory.Exists)
        {
            Svc.Log.Error("Failed to find main assembly directory");
            return null;
        }
        var assemblyPath = Path.Combine(directory.ToString(),
        #if DEBUG
            "net9.0-windows\\RebornRotations.dll"
        #else
            "RebornRotations.dll"
        #endif
        );
        return LoadOne(assemblyPath);
    }

    /// <summary>
    /// This method loads custom rotation groups from local directories and assemblies, creates a sorted list of
    /// author hashes, and creates a sorted list of custom rotations grouped by job role.
    /// </summary>
    /// <param name="relayFolder"></param>
    private static void LoadRotationsFromLocal(string relayFolder)
    {
        var directories = Service.Config.RotationLibs
            .Append(relayFolder)
            .Where(Directory.Exists);

        var assemblies = new List<Assembly>();

        if (Service.Config.LoadDefaultRotations)
        {
            var defaultAssembly = LoadDefaultRotationsFromLocal();
            if (defaultAssembly == null)
            {
                Svc.Log.Error("Failed to load default rotations from local directory");
                return;
            }
            assemblies.Add(defaultAssembly);
        }

        foreach (var dir in directories)
        {
            if (Directory.Exists(dir))
            {
                foreach (var dll in Directory.GetFiles(dir, "*.dll"))
                {
                    if (dll.Contains("RebornRotations.dll"))
                        continue;
                    var assembly = LoadOne(dll);

                    if (assembly != null && !assemblies.Any(a => a.FullName == assembly.FullName))
                    {
                        assemblies.Add(assembly);
                    }
                }
            }
        }

        DutyRotations = LoadDutyRotationGroup(assemblies);
        CustomRotations = LoadCustomRotationGroup(assemblies);
        var customRotationsGroupedByJobRole = new Dictionary<JobRole, List<CustomRotationGroup>>();
        foreach (var customRotationGroup in CustomRotations)
        {
            var job = customRotationGroup.Rotations[0].GetType().GetCustomAttribute<JobsAttribute>()?.Jobs[0] ?? Job.ADV;

            var jobRole = Svc.Data.GetExcelSheet<ClassJob>()!.GetRow((uint)job)!.GetJobRole();
            if (!customRotationsGroupedByJobRole.TryGetValue(jobRole, out var value))
            {
                value = [];
                customRotationsGroupedByJobRole[jobRole] = value;
            }

            value.Add(customRotationGroup);
        }

        CustomRotationsDict = [];
        foreach (var jobRole in customRotationsGroupedByJobRole.Keys)
        {
            var customRotationGroups = customRotationsGroupedByJobRole[jobRole];
            var sortedCustomRotationGroups = customRotationGroups.OrderBy(crg => crg.JobId).ToArray();
            CustomRotationsDict[jobRole] = sortedCustomRotationGroups;
        }
    }

    private static SortedList<uint, Type[]> LoadDutyRotationGroup(List<Assembly> assemblies)
    {
        var rotationList = new List<Type>();
        foreach (var assembly in assemblies)
        {
            foreach (var type in TryGetTypes(assembly))
            {
                if (type.IsAssignableTo(typeof(DutyRotation))
                    && !type.IsAbstract && type.GetConstructor([]) != null)
                {
                    rotationList.Add(type);
                }
            }
        }

        var result = new Dictionary<uint, List<Type>>();
        foreach (var type in rotationList)
        {
            var territories = type.GetCustomAttribute<DutyTerritoryAttribute>()?.TerritoryIds ?? [];

            foreach (var id in territories)
            {
                if (result.TryGetValue(id, out var list))
                {
                    list.Add(type);
                }
                else
                {
                    result[id] = [type];
                }
            }
        }

        return new(result.ToDictionary(i => i.Key, i => i.Value.ToArray()));
    }

    private static CustomRotationGroup[] LoadCustomRotationGroup(List<Assembly> assemblies)
    {
        var rotationList = new List<Type>();

        foreach (var assembly in assemblies)
        {
            foreach (var type in TryGetTypes(assembly))
            {
                var apiAttribute = type.GetCustomAttribute<ApiAttribute>();
                var info = assembly.GetInfo();
                var authorName = info.Author ?? "Unknown Author";

                if (type.GetInterfaces().Contains(typeof(ICustomRotation))
                    && !type.IsAbstract && !type.IsInterface && type.GetConstructor(Type.EmptyTypes) != null)
                {
                    if (apiAttribute?.ApiVersion == Service.ApiVersion)
                    {
                        rotationList.Add(type);
                    }
                    else
                    {
                        var warning = $"Failed to load rotation {type.Assembly.GetName().Name} by {authorName} due to API update";
                        WarningHelper.AddSystemWarning(warning);
                    }
                }
            }
        }

        var rotationGroups = new Dictionary<Job, List<Type>>();
        foreach (var rotation in rotationList)
        {
            var attr = rotation.GetCustomAttribute<JobsAttribute>();
            if (attr == null) continue;

            var jobId = attr.Jobs[0];
            if (!rotationGroups.TryGetValue(jobId, out var value))
            {
                value = new List<Type>();
                rotationGroups.Add(jobId, value);
            }

            value.Add(rotation);
        }

        var result = new List<CustomRotationGroup>();
        foreach (var kvp in rotationGroups)
        {
            var jobId = kvp.Key;
            var rotations = kvp.Value.ToArray();

            result.Add(new CustomRotationGroup(jobId, rotations[0].GetCustomAttribute<JobsAttribute>()!.Jobs,
                rotations));
        }

        return result.ToArray();
    }


    /// <summary>
    /// Downloads rotation files from a remote server and saves them to a local folder.
    /// The download list is obtained from a JSON file on the remote server.
    /// If mustDownload is set to true, it will always download the files, otherwise it will only download if the file doesn't exist locally. 
    /// </summary>
    /// <param name="relayFolder"></param>
    /// <param name="mustDownload"></param>
    /// <returns></returns>
    private static async Task DownloadRotationsAsync(string relayFolder, bool mustDownload)
    {
        // Code to download rotations from remote server
        bool hasDownload = false;

        using (var client = new HttpClient())
        {
            foreach (var url in Service.Config.RotationLibs)
            {
                hasDownload |= await DownloadOneUrlAsync(url, relayFolder, client, mustDownload);
                var pdbUrl = Path.ChangeExtension(url, ".pdb");
                await DownloadOneUrlAsync(pdbUrl, relayFolder, client, mustDownload);
            }
        }
        if (hasDownload) LoadRotationsFromLocal(relayFolder);
    }

    private static string Convert(string value)
    {
        var split = value.Split('|');
        if (split == null || split.Length < 2) return value;
        var username = split[0];
        var repo = split[1];
        var file = split.Last();
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(repo) || string.IsNullOrEmpty(file)) return value;
        return $"https://GitHub.com/{username}/{repo}/releases/latest/download/{file}.dll";
    }

    private static async Task<bool> DownloadOneUrlAsync(string url, string relayFolder, HttpClient client, bool mustDownload)
    {
        try
        {
            var valid = Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uriResult)
                 && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (!valid) return false;
        }
        catch
        {
            return false;
        }
        try
        {
            var fileName = url.Split('/').LastOrDefault();
            if (string.IsNullOrEmpty(fileName)) return false;
            var filePath = Path.Combine(relayFolder, fileName);

            // Check if the file needs to be downloaded
            HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            var fileInfo = new FileInfo(filePath);
            var header = response.Content.Headers;
            bool shouldDownload = mustDownload || !File.Exists(filePath) ||
                                  !header.LastModified.HasValue ||
                                  header.LastModified.Value.UtcDateTime >= fileInfo.LastWriteTimeUtc ||
                                  fileInfo.Length != header.ContentLength;

            if (!shouldDownload)
            {
                return false; // No need to download
            }

            // If reaching here, either the local file doesn't exist, or it's outdated. Proceed to download.
            if (File.Exists(filePath))
            {
                File.Delete(filePath); // Delete the old local file
            }

            using (var stream = new FileStream(filePath, FileMode.CreateNew))
            {
                await response.Content.CopyToAsync(stream);
            }

            Svc.Log.Information($"Successfully downloaded {filePath}");
            return true;
        }
        catch (Exception ex)
        {
            WarningHelper.AddSystemWarning($"Failed to download from {url} Please check VPN");
            Svc.Log.Error(ex, $"Failed to download from {url}");
        }
        return false;
    }


    private static void PrintLoadedAssemblies(IEnumerable<string>? assemblies)
    {
        if (assemblies == null) return;

        foreach (var assembly in assemblies)
        {
            Svc.Chat.Print("Loaded: " + assembly);
        }
    }

    private static Assembly? LoadOne(string filePath)
    {
        try
        {
            return RotationHelper.LoadCustomRotationAssembly(filePath);
        }
        catch (Exception ex)
        {
            WarningHelper.AddSystemWarning("Failed to load " + filePath);
            Svc.Log.Warning(ex, "Failed to load " + filePath);
        }
        return null;
    }

    // This method watches for changes in local rotation files by checking the
    // last modified time of the files in the directories specified in the configuration.
    // If there are new changes, it triggers a reload of the custom rotation.
    // This method uses Parallel.ForEach to improve performance.
    // It also has a check to ensure it's not running too frequently, to avoid hurting the FPS of the game.
    public static void LocalRotationWatcher()
    {
        if (DateTime.Now < LastRunTime.AddSeconds(2))
        {
            return;
        }

        var dirs = Service.Config.RotationLibs;

        foreach (var dir in dirs)
        {
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
            {
                continue;
            }

            var dlls = Directory.GetFiles(dir, "*.dll");

            // There may be many files in these directories,
            // so we opt to use Parallel.ForEach for performance.
            Parallel.ForEach(dlls, async dll =>
            {
                var loadedAssembly = new LoadedAssembly(
                    dll,
                    File.GetLastWriteTimeUtc(dll).ToString());

                int index = RotationHelper.LoadedCustomRotations.FindIndex(item => item.LastModified == loadedAssembly.LastModified);

                if (index == -1)
                {
                    await GetAllCustomRotationsAsync(DownloadOption.Local);
                }
            });
        }

        LastRunTime = DateTime.Now;
    }

    public static Type[] TryGetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (Exception ex)
        {
            Svc.Log.Warning(ex, $"Failed to load the types from {assembly.FullName}");
            return [];
        }
    }

    public static IEnumerable<IGrouping<string, IAction>>? AllGroupedActions
        => GroupActions([
            .. DataCenter.CurrentRotation?.AllActions ?? [],
            .. DataCenter.CurrentDutyRotation?.AllActions ?? []]);

    public static IEnumerable<IGrouping<string, IAction>>? GroupActions(IEnumerable<IAction> actions)
       => actions?.GroupBy(a =>
       {
           if (a is IBaseAction act)
           {
               if (!act.Info.IsOnSlot) return string.Empty;

               string result;

               if (act.Action.ActionCategory.RowId is 10 or 11)
               {
                   return "System Action";
               }
               else if (act.Action.IsRoleAction)
               {
                   return "Role Action";
               }
               else if (act.Info.IsLimitBreak)
               {
                   return "Limit Break";
               }
               else if (act.Info.IsDutyAction)
               {
                   return "Duty Action";
               }

               if (act.Info.IsRealGCD)
               {
                   result = "GCD";
               }
               else
               {
                   result = "oGCD";
               }

               if (act.Setting.IsFriendly)
               {
                   result += "-Friendly";
               }
               else
               {
                   result += "-Attack";
               }
               return result;
           }
           else if (a is IBaseItem)
           {
               return "Item";
           }
           return string.Empty;

       }).Where(g => !string.IsNullOrEmpty(g.Key)).OrderBy(g => g.Key);

    public static void UpdateRotation()
    {
        UpdateCustomRotation();
        UpdateDutyRotation();
    }

    private static void UpdateDutyRotation()
    {
        if (!DutyRotations.TryGetValue(Svc.ClientState.TerritoryType, out var rotations)) return;

        Service.Config.DutyRotationChoice.TryGetValue(Svc.ClientState.TerritoryType, out var value);
        var name = value ?? string.Empty;
        var type = GetChosenType(rotations, name);
        if (type != DataCenter.CurrentDutyRotation?.GetType())
        {
            DataCenter.CurrentDutyRotation?.Dispose();
            DataCenter.CurrentDutyRotation = GetRotation(type);
        }

        static DutyRotation? GetRotation(Type? t)
        {
            if (t == null) return null;
            try
            {
                return (DutyRotation?)Activator.CreateInstance(t);
            }
            catch (Exception ex)
            {
                WarningHelper.AddSystemWarning($"Failed to create the rotation: {t.Name}");
                Svc.Log.Error(ex, $"Failed to create the rotation: {t.Name}");
                return null;
            }
        }
    }

    private static void UpdateCustomRotation()
    {
        var nowJob = (Job)Player.Object.ClassJob.RowId;
        foreach (var group in CustomRotations)
        {
            if (!group.ClassJobIds.Contains(nowJob)) continue;

            var rotation = GetChosenRotation(group);

            if (rotation != DataCenter.CurrentRotation?.GetType())
            {
                var instance = GetRotation(rotation);
                if (instance == null)
                {
#if DEBUG
                    Svc.Log.Error($"Failed to create instance for rotation: {rotation?.Name}");
#endif
                    continue;
                }

                instance.OnTerritoryChanged();
                DataCenter.CurrentRotation = instance;
            }

            CurrentRotationActions = DataCenter.CurrentRotation?.AllActions ?? Array.Empty<IAction>();
            return;
        }

        CustomRotation.MoveTarget = null;
        DataCenter.CurrentRotation = null;
        CurrentRotationActions = Array.Empty<IAction>();

        static ICustomRotation? GetRotation(Type? t)
        {
            if (t == null) return null;
            try
            {
                return (ICustomRotation?)Activator.CreateInstance(t);
            }
            catch (Exception)
            {
#if DEBUG
                Svc.Log.Error($"Failed to create the rotation: {t.Name}");
#endif
                return null;
            }
        }

        static Type? GetChosenRotation(CustomRotationGroup group)
        {
            var isPvP = DataCenter.IsPvP;

            var rotations = group.Rotations
                .Where(r =>
                {
                    var rot = r.GetCustomAttribute<RotationAttribute>();
                    if (rot == null) return false;

                    var type = rot.Type;

                    return isPvP ? type.HasFlag(CombatType.PvP) : type.HasFlag(CombatType.PvE);
                });

            var name = isPvP ? Service.Config.PvPRotationChoice : Service.Config.RotationChoice;
            return GetChosenType(rotations, name);
        }
    }

    private static Type? GetChosenType(IEnumerable<Type> types, string name)
    {
        var rotation = types.FirstOrDefault(r => r.FullName == name);

        rotation ??= types.FirstOrDefault(r => r.Assembly.FullName!.Contains("DefaultRotations", StringComparison.OrdinalIgnoreCase));

        rotation ??= types.FirstOrDefault();

        return rotation;
    }
}
