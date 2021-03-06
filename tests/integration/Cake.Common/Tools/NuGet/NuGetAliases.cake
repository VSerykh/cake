#tool "nuget:https://api.nuget.org/v3/index.json?package=nuget.commandline&version=4.9.2"
#load "./../../../utilities/xunit.cake"
#load "./../../../utilities/paths.cake"

FilePath NuGetAliasesConfigFile = null;
Task("Cake.Common.Tools.NuGet.NuGetAliases.Setup")
    .Does(() =>
{
    var sourcePath = Paths.Resources.Combine("./Cake.Common/Tools/NuGet");
    var targetPath = Paths.Temp.Combine("./Cake.Common/Tools/NuGet");
    EnsureDirectoryExist(targetPath.Combine("../").Collapse());
    CopyDirectory(sourcePath, targetPath);
    NuGetAliasesConfigFile = targetPath.CombineWithFilePath("NuGet.config");
});

Task("Cake.Common.Tools.NuGet.NuGetAliases.NuGetAddSource")
    .IsDependentOn("Cake.Common.Tools.NuGet.NuGetAliases.Setup")
    .Does(() =>
{
    // Given
    var name = "TestSource";
    var source = "https://contoso.com/packages/";
    var settings = new NuGetSourcesSettings {
            ConfigFile = NuGetAliasesConfigFile
        };

    // When
    NuGetAddSource(
        name,
        source,
        settings
        );
});

Task("Cake.Common.Tools.NuGet.NuGetAliases.NuGetHasSource")
    .IsDependentOn("Cake.Common.Tools.NuGet.NuGetAliases.NuGetAddSource")
    .Does(() =>
{
    // Given
    var source = "https://contoso.com/packages/";
    var settings = new NuGetSourcesSettings {
            ConfigFile = NuGetAliasesConfigFile
        };

    // When
    var result = NuGetHasSource(
        source,
        settings
        );

    // Then
    Assert.True(result);
});

Task("Cake.Common.Tools.NuGet.NuGetAliases.NuGetHasSource.ArgumentCustomization")
    .IsDependentOn("Cake.Common.Tools.NuGet.NuGetAliases.NuGetAddSource")
    .Does(() =>
{
    // Given
    var source = "https://contoso.com/packages/";
    var settings = new NuGetSourcesSettings {
            ArgumentCustomization = args => args.Append("-ConfigFile").AppendQuoted(NuGetAliasesConfigFile.FullPath)
        };

    // When
    var result = NuGetHasSource(
        source,
        settings
        );

    // Then
    Assert.True(result);
});

Task("Cake.Common.Tools.NuGet.NuGetAliases.NuGetRemoveSource")
    .IsDependentOn("Cake.Common.Tools.NuGet.NuGetAliases.NuGetHasSource")
    .IsDependentOn("Cake.Common.Tools.NuGet.NuGetAliases.NuGetHasSource.ArgumentCustomization")
    .Does(() =>
{
    // Given
    var name = "TestSource";
    var source = "https://contoso.com/packages/";
    var settings = new NuGetSourcesSettings {
            ConfigFile = NuGetAliasesConfigFile
        };

    // When
    NuGetRemoveSource(
        name,
        source,
        settings
        );

    var result = NuGetHasSource(
        source,
        settings
        );

    // Then
    Assert.False(result);

});

var nugetAliasesTask = Task("Cake.Common.Tools.NuGet.NuGetAliases");
if (!Context.Environment.Runtime.IsCoreClr || !Context.IsRunningOnUnix())
{
    nugetAliasesTask
        .IsDependentOn("Cake.Common.Tools.NuGet.NuGetAliases.NuGetAddSource")
        .IsDependentOn("Cake.Common.Tools.NuGet.NuGetAliases.NuGetHasSource")
        .IsDependentOn("Cake.Common.Tools.NuGet.NuGetAliases.NuGetRemoveSource");
}