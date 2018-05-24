/// FAKE Build script

#r "packages/build/FAKE/tools/FakeLib.dll"
open Fake
open Fake.AssemblyInfoFile
open Fake.Git
open Fake.ReleaseNotesHelper
open Fake.Testing.XUnit2
open System.IO

// Version info
let projectName = "Simple.OData.Client"
let authors = ["Vagif Abilov"]
let copyright = "Copyright © 2002-18 Vagif Abilov"

let release = LoadReleaseNotes "RELEASE_NOTES.md"

// Properties
let buildDir = "./Solutions/build"
let toolsDir = getBuildParamOrDefault "tools" "packages/build"
let solutionFile = "./Solutions/Simple.OData.Client.Win.sln"

let xunitPath = toolsDir @@ "/xunit.runner.console/tools/net452/xunit.console.exe"

// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir;]
)

Target "PackageRestore" (fun _ ->
    !! solutionFile
    |> MSBuildRelease "" "Restore"
    |> Log "AppBuild-Output: "
)

Target "SetVersion" (fun _ ->
    let commitHash = 
        try 
            Information.getCurrentHash()
        with
            | ex -> printfn "Exception! (%s)" (ex.Message); ""
    let infoVersion = String.concat " " [release.AssemblyVersion; commitHash]
    CreateCSharpAssemblyInfo "./CommonAssemblyVersion.cs"
        [
         Attribute.Version release.AssemblyVersion
         Attribute.FileVersion release.AssemblyVersion
         Attribute.InformationalVersion infoVersion]
)

Target "Build" (fun _ ->
    !! solutionFile
    |> MSBuild "" "Build"
        [
            "Configuration", "Release"
            "Platform", "Any CPU"
            "Authors", authors |> String.concat ", "
            "PackageVersion", release.AssemblyVersion
            "PackageReleaseNotes", release.Notes |> toLines
            "IncludeSymbols", "true"
        ]
    |> Log "AppBuild-Output: "
)

Target "Test" (fun _ ->
    Directory.GetFiles(buildDir, "*.Tests.Core.dll", SearchOption.AllDirectories)
    // Filter out the NET Core versions as this runner can't execute them
    |> Array.filter (fun x -> x.Contains("netcoreapp") = false)
    |> xUnit2 (fun p -> {p with ToolPath = xunitPath
                                Silent = true
                                Parallel = ParallelMode.Assemblies})
)

Target "Release" (fun _ ->
    let tag = String.concat "" ["v"; release.AssemblyVersion] 
    Branches.tag "" tag
    Branches.pushTag "" "origin" tag
)

Target "Default" DoNothing

// Dependencies
"Clean"
    ==> "SetVersion"
    ==> "PackageRestore"
    ==> "Build"
    ==> "Test"
    ==> "Default"
    ==> "Release"

RunTargetOrDefault "Default"