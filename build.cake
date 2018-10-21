#tool nuget:?package=GitVersion.CommandLine
#tool nuget:?package=vswhere

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
var releaseNotes = ParseReleaseNotes("./releasenotes.md").Notes.ToArray();

Task("Clean").Does(() =>
{
    CleanDirectories("./**/bin");
    CleanDirectories("./**/obj");
	CleanDirectories(outputDir.FullPath);
});

FilePath msBuildPath;
Task("ResolveBuildTools")
	.Does(() => 
{
	var vsLatest = VSWhereLatest();
	msBuildPath = (vsLatest == null)
		? null
		: vsLatest.CombineWithFilePath("./MSBuild/15.0/Bin/MSBuild.exe");
});

GitVersion versionInfo = null;
Task("Version").Does(() => {
	var branchName = AppVeyor.Environment.Repository.Branch;

	GitVersion(new GitVersionSettings {
		UpdateAssemblyInfo = true,
		OutputType = GitVersionOutput.BuildServer,
		Branch = branchName
	});

	versionInfo = GitVersion(new GitVersionSettings { 
		OutputType = GitVersionOutput.Json,
		Branch = branchName
	});

	Information("VI:\t{0}", versionInfo.FullSemVer);
});

Task("Restore").Does(() => {
	NuGetRestore(sln);
});

Task("Build")
	.IsDependentOn("Clean")
	.IsDependentOn("Version")
	.IsDependentOn("Restore")
	.IsDependentOn("ResolveBuildTools")
	.Does(() =>  {

	var settings = new MSBuildSettings 
	{
		Configuration = "Release",
		ToolPath = msBuildPath
	};

	settings.Properties.Add("Platform", new [] {"iPhone"});

	MSBuild(sln, settings);
});

Task("Package")
	.IsDependentOn("Build")
	.Does(() => {

	EnsureDirectoryExists(outputDir);

	var dllDir = binDir + "/SGTabbedPager.*";

	Information("Dll Dir: {0}", dllDir);

	var nugetContent = new List<NuSpecContent>();
	foreach(var dll in GetFiles(dllDir)){
		var dllString = dll.ToString();
		if (dllString.EndsWith(".mdb"))
			continue;

	 	Information("File: {0}", dll.ToString());
		nugetContent.Add(new NuSpecContent {
			Target = "lib/Xamarin.iOS10",
			Source = dllString
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
		ReleaseNotes = releaseNotes,
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
	.Does(() => {

	EnsureDirectoryExists(outputDir);

	var dllDir = binDirMvx + "/SGTabbedPagerMvx.*";

	Information("Dll Dir: {0}", dllDir);

	var nugetContent = new List<NuSpecContent>();
	foreach(var dll in GetFiles(dllDir)){
		var dllString = dll.ToString();
		if (dllString.EndsWith(".mdb"))
			continue;

	 	Information("File: {0}", dll.ToString());
		nugetContent.Add(new NuSpecContent {
			Target = "lib/Xamarin.iOS10",
			Source = dllString
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
		ReleaseNotes = releaseNotes,
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

Task("Default")
	.IsDependentOn("UploadAppVeyorArtifact");

RunTarget(target);
