#tool "nuget:?package=GitVersion.CommandLine"

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var framework = Argument("framework", "netcoreapp1.1");

var outputDir = "./artifacts/";
var artifactName = "artifact.zip";
var projectPath = "./AspNetCore.CrudDemo";
var projectJsonPath = projectPath + "/project.json";

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
		var projects = GetFiles("./**/*.xproj");

		var settings = new DotNetCoreBuildSettings
		{
			Framework = framework,
			Configuration = configuration,
		};

		foreach(var project in projects)
		{
			DotNetCoreBuild(project.GetDirectory().FullPath, settings);
		}
	});

Task("Test")
	.IsDependentOn("Build")
	.Does(() => 
	{
		var settings = new DotNetCoreTestSettings
		{
			Framework = framework,
			Configuration = configuration,
		};

		DotNetCoreTest("./AspNetCore.CrudDemo.Controllers.Tests", settings);

		// Because DocumentDB emulator is not yet supported on CI
		if (BuildSystem.IsLocalBuild)
			DotNetCoreTest("./AspNetCore.CrudDemo.Services.Tests", settings);
	});

Task("Publish")
	.IsDependentOn("Test")
	.Does(() => 
	{
		var settings = new DotNetCorePublishSettings
		{
			Configuration = configuration,
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