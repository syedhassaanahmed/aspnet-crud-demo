#tool "nuget:?package=GitVersion.CommandLine"

var target = Argument("target", "Default");
var outputDir = "./artifacts/";
var artifactName = "artifact.zip";
var solutionPath = "./AspNetCore.CrudDemo.sln";
var projectPath = "./AspNetCore.CrudDemo";
var projectJsonPath = projectPath + "/project.json";
var buildConfig = "Release";

Task("Clean")
	.Does(() => 
	{
		if (DirectoryExists(outputDir))
			DeleteDirectory(outputDir, recursive:true);

		CreateDirectory(outputDir);
	});

Task("Restore")
	.Does(() => DotNetCoreRestore());

Task("Version")
	.Does(() => 
	{
		GitVersion(new GitVersionSettings
		{
			UpdateAssemblyInfo = true,
			OutputType = GitVersionOutput.BuildServer
		});

		var versionInfo = GitVersion(new GitVersionSettings{ OutputType = GitVersionOutput.Json });

		var updatedProjectJson = System.IO.File.ReadAllText(projectJsonPath)
			.Replace("1.0.0-*", versionInfo.NuGetVersion);
		System.IO.File.WriteAllText(projectJsonPath, updatedProjectJson);
	});

Task("Build")
	.IsDependentOn("Clean")
	.IsDependentOn("Version")
	.IsDependentOn("Restore")
	.Does(() => 
	{
		DotNetBuild(solutionPath, settings => settings
			.SetConfiguration(buildConfig)
        	.SetVerbosity(Verbosity.Minimal)
        	.WithTarget("Build"));
	});

Task("Test")
	.IsDependentOn("Build")
	.Does(() => 
	{
		DotNetCoreTest("./AspNetCore.CrudDemo.Controllers.Tests");

		// Because DocumentDB emulator is not yet supported on CI
		if (BuildSystem.IsLocalBuild)
			DotNetCoreTest("./AspNetCore.CrudDemo.Services.Tests");
	});

Task("Publish")
	.IsDependentOn("Test")
	.Does(() => 
	{
		var settings = new DotNetCorePublishSettings
		{
			Configuration = buildConfig,
			OutputDirectory = outputDir
		};
					
		DotNetCorePublish(projectPath, settings);
		Zip(outputDir, artifactName);

		if (BuildSystem.IsRunningOnAppVeyor)
		{
			var files = GetFiles(artifactName);
			foreach(var file in files)
				AppVeyor.UploadArtifact(file.FullPath);
		}
	});

Task("Default")
	.IsDependentOn("Publish");

RunTarget(target);