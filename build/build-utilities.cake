// Miscellaneous build utilities.
public static class BuildUtilities {

    // Informs the build system of the build number that is being used.
    public static void SetBuildSystemBuildNumber(BuildSystem buildSystem, BuildState buildState) {
        // Tell TeamCity the build number if required.
        if (buildSystem.IsRunningOnTeamCity) {
            buildSystem.TeamCity.SetBuildNumber(buildState.BuildNumber);
            buildSystem.TeamCity.SetParameter("system.AssemblyVersion", buildState.AssemblyVersion);
            buildSystem.TeamCity.SetParameter("system.AssemblyFileVersion", buildState.AssemblyFileVersion);
            buildSystem.TeamCity.SetParameter("system.InformationalVersion", buildState.InformationalVersion);
            buildSystem.TeamCity.SetParameter("system.PackageVersion", buildState.PackageVersion);
        }
    }


    // Writes a log message.
    public static void WriteLogMessage(BuildSystem buildSystem, string message) {
        if (buildSystem.IsRunningOnTeamCity) {
            buildSystem.TeamCity.WriteProgressMessage(message);
        }
        else {
            Console.WriteLine(message);
        }
    }


    // Writes a task started message.
    public static void WriteTaskStartMessage(BuildSystem buildSystem, string description) {
        if (buildSystem.IsRunningOnTeamCity) {
            buildSystem.TeamCity.WriteStartBuildBlock(description);
        }
    }


    // Writes a task completed message.
    public static void WriteTaskEndMessage(BuildSystem buildSystem, string description) {
        if (buildSystem.IsRunningOnTeamCity) {
            buildSystem.TeamCity.WriteEndBuildBlock(description);
        }
    }


    // Writes the specified build state to the log.
    public static void WriteBuildStateToLog(BuildSystem buildSystem, BuildState state) {
        WriteLogMessage(buildSystem, $"Solution Name: {state.SolutionName}");
        WriteLogMessage(buildSystem, $"Build Number: {state.BuildNumber}");
        WriteLogMessage(buildSystem, $"Target: {state.Target}");
        WriteLogMessage(buildSystem, $"Configuration: {state.Configuration}");
        WriteLogMessage(buildSystem, $"Clean: {state.RunCleanTarget}");
        WriteLogMessage(buildSystem, $"Continous Integration Build: {state.ContinuousIntegrationBuild}");
        WriteLogMessage(buildSystem, $"Sign Output: {state.CanSignOutput}");
        WriteLogMessage(buildSystem, $"Informational Version: {state.InformationalVersion}");
        WriteLogMessage(buildSystem, $"Assembly Version: {state.AssemblyVersion}");
        WriteLogMessage(buildSystem, $"Assembly File Version: {state.AssemblyFileVersion}");
        WriteLogMessage(buildSystem, $"Package Version: {state.PackageVersion}");
    }


    // Adds MSBuild properties from the build state.
    public static void ApplyMSBuildProperties(DotNetCoreMSBuildSettings settings, BuildState state) {
        // Specify if this is a CI build. 
        if (state.ContinuousIntegrationBuild) {
            settings.Properties["ContinuousIntegrationBuild"] = new List<string> { "True" };
        }

        // Specify if we are signing DLLs and NuGet packages.
        if (state.CanSignOutput) {
            settings.Properties["SignOutput"] = new List<string> { "True" };
        }

        // Set version numbers.
        settings.Properties["AssemblyVersion"] = new List<string> { state.AssemblyVersion };
        settings.Properties["FileVersion"] = new List<string> { state.AssemblyFileVersion };
        settings.Properties["Version"] = new List<string> { state.PackageVersion };
        settings.Properties["InformationalVersion"] = new List<string> { state.InformationalVersion };
    }


    // Imports test results into the build system.
    public static void ImportTestResults(BuildSystem buildSystem, string testProvider, FilePath resultsFile) {
        if (resultsFile == null) {
            return;
        }

        if (buildSystem.IsRunningOnTeamCity) {
            buildSystem.TeamCity.ImportData(testProvider, resultsFile);
        }
    }

}
