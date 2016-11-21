#tool "nuget:?package=GitVersion.CommandLine"
#tool "nuget:?package=gitlink"

var sln = new FilePath("SGTabbedPager.sln");
var project = new FilePath("SGTabbedPager/SGTabbedPager.csproj");
var projectMvx = new FilePath("SGTabbedPagerMvx/SGTabbedPagerMvx.csproj");
var binDir = new DirectoryPath("SGTabbedPager/bin/Release");
var binDirMvx = new DirectoryPath("SGTabbedPagerMvx/bin/Release");
var nuspec = new FilePath("SGTabbedPager.nuspec");
var nuspecMvx = new FilePath("SGTabbedPagerMvx.nuspec");
var outputDir = new DirectoryPath("artifacts");
var target = Argument("target", "Default");

var isRunningOnAppVeyor = AppVeyor.IsRunningOnAppVeyor;
var isPullRequest = AppVeyor.Environment.PullRequest.IsPullRequest;

Task("Clean").Does(() =>
{
    CleanDirectories("./**/bin");
    CleanDirectories("./**/obj");
	CleanDirectories(outputDir.FullPath);
});

GitVersion versionInfo = null;
Task("Version").Does(() => {
	GitVersion(new GitVersionSettings {
		UpdateAssemblyInfo = true,
		OutputType = GitVersionOutput.BuildServer
	});

	versionInfo = GitVersion(new GitVersionSettings{ OutputType = GitVersionOutput.Json });
	Information("VI:\t{0}", versionInfo.FullSemVer);
});

Task("Restore").Does(() => {
	NuGetRestore(sln);
});

Task("Build")
	.IsDependentOn("Clean")
	.IsDependentOn("Version")
	.IsDependentOn("Restore")
	.Does(() =>  {
	
	DotNetBuild(sln, 
		settings => settings.SetConfiguration("Release")
          .WithProperty("Platform", "iPhone")
	);
});

Task("GitLink")
	.WithCriteria(() => IsRunningOnWindows()) //pdbstr.exe and costura are not xplat currently
	.Does(() => {
	GitLink(sln.GetDirectory(), new GitLinkSettings {
		ArgumentCustomization = args => args.Append("-ignore sample,samplemvx.core,samplemvx.ios"),
		RepositoryUrl = "https://github.com/Cheesebaron/SGTabbedPager"
	});
});

Task("Package")
	.IsDependentOn("Build")
	.IsDependentOn("GitLink")
	.Does(() => {

	EnsureDirectoryExists(outputDir);

	var dllDir = binDir + "/SGTabbedPager.*";

	Information("Dll Dir: {0}", dllDir);

	var nugetContent = new List<NuSpecContent>();
	foreach(var dll in GetFiles(dllDir)){
	 	Information("File: {0}", dll.ToString());
		nugetContent.Add(new NuSpecContent {
			Target = "lib/Xamarin.iOS10",
			Source = dll.ToString()
		});
	}

	Information("File Count {0}", nugetContent.Count);

	NuGetPack(nuspec, new NuGetPackSettings {
		Authors = new [] { "Tomasz Cielecki" },
		Owners = new [] { "Tomasz Cielecki" },
		IconUrl = new Uri("http://i.imgur.com/V3983YY.png"),
		ProjectUrl = new Uri("https://github.com/Cheesebaron/SGTabbedPager"),
		LicenseUrl = new Uri("https://github.com/Cheesebaron/SGTabbedPager/blob/master/LICENSE"),
		Copyright = "Copyright (c) Tomasz Cielecki",
		RequireLicenseAcceptance = false,
		Tags = new [] {"monotouch", "ui", "pager", "xamarin", "ios"},
		Version = versionInfo.NuGetVersion,
		Symbols = false,
		NoPackageAnalysis = true,
		OutputDirectory = outputDir,
		Verbosity = NuGetVerbosity.Detailed,
		Files = nugetContent,
		BasePath = "/."
	});
});

Task("PackageMvx")
	.IsDependentOn("Build")
	.IsDependentOn("GitLink")
	.Does(() => {

	EnsureDirectoryExists(outputDir);

	var dllDir = binDirMvx + "/SGTabbedPagerMvx.*";

	Information("Dll Dir: {0}", dllDir);

	var nugetContent = new List<NuSpecContent>();
	foreach(var dll in GetFiles(dllDir)){
	 	Information("File: {0}", dll.ToString());
		nugetContent.Add(new NuSpecContent {
			Target = "lib/Xamarin.iOS10",
			Source = dll.ToString()
		});
	}

	Information("File Count {0}", nugetContent.Count);

	NuGetPack(nuspecMvx, new NuGetPackSettings {
		Authors = new [] { "Tomasz Cielecki" },
		Owners = new [] { "Tomasz Cielecki" },
		IconUrl = new Uri("http://i.imgur.com/V3983YY.png"),
		ProjectUrl = new Uri("https://github.com/Cheesebaron/SGTabbedPager"),
		LicenseUrl = new Uri("https://github.com/Cheesebaron/SGTabbedPager/blob/master/LICENSE"),
		Copyright = "Copyright (c) Tomasz Cielecki",
		RequireLicenseAcceptance = false,
		Tags = new [] {"monotouch", "mvvmcross", "ui", "pager", "xamarin", "ios"},
		Version = versionInfo.NuGetVersion,
		Symbols = false,
		NoPackageAnalysis = true,
		OutputDirectory = outputDir,
		Verbosity = NuGetVerbosity.Detailed,
		Files = nugetContent,
		BasePath = "/."
	});
});

Task("UploadAppVeyorArtifact")
	.IsDependentOn("Package")
	.IsDependentOn("PackageMvx")
	.WithCriteria(() => !isPullRequest)
	.WithCriteria(() => isRunningOnAppVeyor)
	.Does(() => {

	Information("Artifacts Dir: {0}", outputDir.FullPath);

	foreach(var file in GetFiles(outputDir.FullPath + "/*")) {
		Information("Uploading {0}", file.FullPath);
		AppVeyor.UploadArtifact(file.FullPath);
	}
});

Task("Default").IsDependentOn("UploadAppVeyorArtifact").Does(() => {});

RunTarget(target);
