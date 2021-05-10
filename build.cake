const string DefaultSolutionName = "./IntelligentPlant.BackgroundTasks.sln";

///////////////////////////////////////////////////////////////////////////////////////////////////
// COMMAND LINE ARGUMENTS:
//
// --project=<PROJECT OR SOLUTION>
//   The MSBuild project or solution to build. 
//     Default: see DefaultSolutionName constant above.
//
// --target=<TARGET>
//   Specifies the Cake target to run. 
//     Default: Test
//     Possible Values: Clean, Restore, Build, Test, Pack
//
// --configuration=<CONFIGURATION>
//   Specifies the MSBuild configuration to use. 
//     Default: Debug
//
// --clean
//   Specifies if this is a rebuild rather than an incremental build. All artifact, bin, and test 
//   output folders will be cleaned prior to running the specified target.
//
// --ci
//   Forces continuous integration build mode. Not required if the build is being run by a 
//   supported continuous integration build system.
//
// --sign-output
//   Tells MSBuild that signing is required by setting the 'SignOutput' property to 'True'. The 
//   signing implementation must be supplied by MSBuild.
//
// --build-counter=<COUNTER>
//   The build counter. This is used when generating version numbers for the build.
//
// --build-metadata=<METADATA>
//   Additional build metadata that will be included in the information version number generated 
//   for compiled assemblies.
//
// --verbose
//   Enables verbose messages.
// 
///////////////////////////////////////////////////////////////////////////////////////////////////

#addin nuget:?package=Cake.Git&version=1.0.0
#addin nuget:?package=Cake.Json&version=6.0.0
#addin nuget:?package=Newtonsoft.Json&version=12.0.3

#load "build/build-state.cake"
#load "build/build-utilities.cake"

// Get the target that was specified.
var target = Argument("target", "Test");


///////////////////////////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////////////////////////


// Constructs the build state object.
Setup<BuildState>(context => {
    try {
        BuildUtilities.WriteTaskStartMessage(BuildSystem, "Setup");
        var state = new BuildState() {
            SolutionName = Argument("project", DefaultSolutionName),
            Target = target,
            Configuration = Argument("configuration", "Debug"),
            ContinuousIntegrationBuild = HasArgument("ci") || !BuildSystem.IsLocalBuild,
            Clean = HasArgument("clean"),
            SignOutput = HasArgument("sign-output"),
            Verbose = HasArgument("verbose")
        };

        // Get raw version numbers from JSON.

        var versionJson = ParseJsonFromFile("./build/version.json");

        var majorVersion = versionJson.Value<int>("Major");
        var minorVersion = versionJson.Value<int>("Minor");
        var patchVersion = versionJson.Value<int>("Patch");
        var versionSuffix = versionJson.Value<string>("PreRelease");

        // Compute version numbers.

        var buildCounter = Argument("build-counter", 0);
        var buildMetadata = Argument("build-metadata", state.ContinuousIntegrationBuild ? "" : "unofficial");
        if (!string.IsNullOrEmpty(buildMetadata)) {
            var buildMetadataValidator = new System.Text.RegularExpressions.Regex(@"^[0-9A-Aa-z-]+(\.[0-9A-Aa-z-]+)*$");
            if (!buildMetadataValidator.Match(buildMetadata).Success) {
                throw new Exception($"Build metadata '{buildMetadata}' is invalid. Metadata must consist of dot-delimited groups of ASCII alphanumerics and hyphens (i.e. [0-9A-Za-z-]). See https://semver.org/#spec-item-10 for details.");
            }
        }
        var branch = GitBranchCurrent(DirectoryPath.FromString(".")).FriendlyName;

        state.AssemblyVersion = $"{majorVersion}.{minorVersion}.0.0";
        state.AssemblyFileVersion = $"{majorVersion}.{minorVersion}.{patchVersion}.{buildCounter}";

        state.PackageVersion = string.IsNullOrWhiteSpace(versionSuffix) 
            ? $"{majorVersion}.{minorVersion}.{patchVersion}"
            : $"{majorVersion}.{minorVersion}.{patchVersion}-{versionSuffix}.{buildCounter}";

        state.BuildNumber = string.IsNullOrWhiteSpace(versionSuffix)
            ? $"{majorVersion}.{minorVersion}.{patchVersion}.{buildCounter}+{branch}"
            : $"{majorVersion}.{minorVersion}.{patchVersion}-{versionSuffix}.{buildCounter}+{branch}";

        state.InformationalVersion = string.IsNullOrWhiteSpace(buildMetadata)
            ? state.BuildNumber
            : $"{state.BuildNumber}#{buildMetadata}";

        if (!string.Equals(state.Target, "Clean", StringComparison.OrdinalIgnoreCase)) {
            BuildUtilities.SetBuildSystemBuildNumber(BuildSystem, state);
            BuildUtilities.WriteBuildStateToLog(BuildSystem, state);
        }

        return state;
    }
    finally {
        BuildUtilities.WriteTaskEndMessage(BuildSystem, "Setup");
    }
});


// Pre-task action.
TaskSetup(context => {
    BuildUtilities.WriteTaskStartMessage(BuildSystem, context.Task.Name);
});


// Post task action.
TaskTeardown(context => {
    BuildUtilities.WriteTaskEndMessage(BuildSystem, context.Task.Name);
});


///////////////////////////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////////////////////////


// Cleans up artifact and bin folders.
Task("Clean")
    .WithCriteria<BuildState>((c, state) => state.RunCleanTarget)
    .Does<BuildState>(state => {
        foreach (var pattern in new [] { $"./src/**/bin/{state.Configuration}", "./artifacts/**", "./**/TestResults/**" }) {
            BuildUtilities.WriteLogMessage(BuildSystem, $"Cleaning directories: {pattern}");
            CleanDirectories(pattern);
        }
    });


// Restores NuGet packages.
Task("Restore")
    .Does<BuildState>(state => {
        DotNetCoreRestore(state.SolutionName);
    });


// Builds the solution.
Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .Does<BuildState>(state => {
        var buildSettings = new DotNetCoreBuildSettings {
            Configuration = state.Configuration,
            NoRestore = true,
            MSBuildSettings = new DotNetCoreMSBuildSettings()
        };

        buildSettings.MSBuildSettings.Targets.Add(state.Clean ? "Rebuild" : "Build");
        BuildUtilities.ApplyMSBuildProperties(buildSettings.MSBuildSettings, state);
        DotNetCoreBuild(state.SolutionName, buildSettings);
    });


// Runs unit tests.
Task("Test")
    .IsDependentOn("Build")
    .Does<BuildState>(state => {
        var testSettings = new DotNetCoreTestSettings {
            Configuration = state.Configuration,
            NoBuild = true
        };

        var testResultsPrefix = state.ContinuousIntegrationBuild
            ? Guid.NewGuid().ToString()
            : null;

        if (testResultsPrefix != null) {
            // We're using a build system; write the test results to a file so that they can be 
            // imported into the build system.
            testSettings.Loggers = new List<string> {
                $"trx;LogFilePrefix={testResultsPrefix}"
            };
        }

        DotNetCoreTest(state.SolutionName, testSettings);

        if (testResultsPrefix != null) {
            foreach (var testResultsFile in GetFiles($"./**/TestResults/{testResultsPrefix}*.trx")) {
                BuildUtilities.ImportTestResults(BuildSystem, "mstest", testResultsFile);
            }
        }
    });


// Builds NuGet packages.
Task("Pack")
    .IsDependentOn("Test")
    .Does<BuildState>(state => {
        var buildSettings = new DotNetCorePackSettings {
            Configuration = state.Configuration,
            NoRestore = true,
            NoBuild = true,
            MSBuildSettings = new DotNetCoreMSBuildSettings()
        };

        BuildUtilities.ApplyMSBuildProperties(buildSettings.MSBuildSettings, state);
        DotNetCorePack(state.SolutionName, buildSettings);
    });


///////////////////////////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////////////////////////


RunTarget(target);