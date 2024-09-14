using Bullseye;
using Bullseye.Internal;
using SimpleExec;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static build.Program;
using static SimpleExec.Command;
using Host = Bullseye.Host;

namespace build
{
    public static class BuildDefinition
    {
        public static async Task<GitVersion> ResolveVersion(Options options, PreviousReleases releases)
        {
            GitVersion version;
            if (ForcedVersion != null)
            {
                version = new GitVersion(ForcedVersion);
            }
            else
            {
                string versionJson;
                try
                {
                    var result = await ReadAsync("dotnet", $"gitversion /nofetch{(options.Verbose ? " /diag" : "")}", BasePath);
                    versionJson = result.StandardOutput;
                }
                catch (ExitCodeException)
                {
                    Run("dotnet", "gitversion /nofetch /diag", BasePath);
                    throw;
                }

                WriteVerbose(versionJson);

                if (options.Verbose)
                {
                    // Remove extra output from versionJson
                    var lines = versionJson
                        .Split('\n')
                        .Select(l => l.TrimEnd('\r'))
                        .SkipWhile(l => !l.StartsWith('{'))
                        .TakeWhile(l => !l.StartsWith('}'))
                        .Append("}");

                    versionJson = string.Join('\n', lines);
                }

                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };
                jsonOptions.Converters.Add(new AutoNumberToStringConverter());

                version = JsonSerializer.Deserialize<GitVersion>(versionJson, jsonOptions)!;

                // Workaround to prevent issues with some consumers of the NuGet API that build
                // links manually instead of following the links that come in the response.
                // https://github.com/aaubry/Framework/issues/703
                version.PreReleaseLabel = version.PreReleaseLabel?.ToLowerInvariant();
            }

            if (version.CommitsSinceVersionSource > 0 && version.Equals(releases.Latest))
            {
                ++version.Patch;
                WriteWarning("Patch was incremented because the version was not incremented since last release.");
            }

            if (version.IsPreRelease && NoPrerelease)
            {
                WriteWarning($"Forcing pre-release version '{version.PreReleaseLabel}' to be considered stable");
                version.PreReleaseLabel = null;
            }

            WriteImportant($"Current version is {version.NuGetVersion}");

            return version;
        }

        public static Task SetBuildVersion(Options options, GitVersion version)
        {
            switch (options.Host)
            {
                case Host.AppVeyor:
                    var buildNumber = Environment.GetEnvironmentVariable("APPVEYOR_BUILD_NUMBER");
                    Run("appveyor", $"UpdateBuild -Version {version.NuGetVersion}.{buildNumber}");
                    break;
            }

            return Task.CompletedTask;
        }

        public static Task<MetadataSet> SetMetadata(GitVersion version)
        {
            var templatePath = Path.Combine(BasePath, "Framework", "Properties", "AssemblyInfo.template");
            WriteVerbose($"Using template {templatePath}");

            var template = File.ReadAllText(templatePath);
            var assemblyInfo = template
                .Replace("<%assemblyVersion%>", $"{version.Major}.0.0.0")
                .Replace("<%assemblyFileVersion%>", $"{version.MajorMinorPatch}.0")
                .Replace("<%assemblyInformationalVersion%>", version.NuGetVersion);

            var asssemblyInfoPath = Path.Combine(BasePath, "Framework", "Properties", "AssemblyInfo.cs");
            WriteVerbose($"Writing metadata to {asssemblyInfoPath}");
            File.WriteAllText(asssemblyInfoPath, assemblyInfo);

            return Task.FromResult(new MetadataSet());
        }

        public static Task<SuccessfulBuild> Build(Options options, MetadataSet _)
        {
            var verbosity = options.Verbose ? "detailed" : "minimal";
            Run("dotnet", $"build Framework.sln --configuration Release --verbosity {verbosity} -p:ContinuousIntegrationBuild=true", BasePath);

            return Task.FromResult(new SuccessfulBuild());
        }

        public static Task<SuccessfulUnitTests> UnitTest(Options options, SuccessfulBuild _)
        {
            var verbosity = options.Verbose ? "detailed" : "minimal";
            Run("dotnet", $"test Framework.Test.csproj --no-build --configuration Release --verbosity {verbosity}", Path.Combine(BasePath, "Framework.Test"));

            return Task.FromResult(new SuccessfulUnitTests());
        }

        public static Task<List<NuGetPackage>> Pack(Options options, GitVersion version, SuccessfulUnitTests _)
        {
            var result = new List<NuGetPackage>();
            var verbosity = options.Verbose ? "detailed" : "minimal";
            var buildDir = Path.Combine(BasePath, "Framework");
            Run("dotnet", $"pack --no-build -p:PackageVersion={version.NuGetVersion} -p:ContinuousIntegrationBuild=true --output bin", buildDir);
            var packagePath = Path.Combine(buildDir, "bin", $"Framework.{version.NuGetVersion}.nupkg");
            result.Add(new NuGetPackage(packagePath, "Framework"));

            if (PushSerializer)
            {
                buildDir = Path.Combine(BasePath, "Framework.Analyzers.StaticGenerator");
                Run("dotnet", $"pack --no-build -p:PackageVersion={version.NuGetVersion} -p:ContinuousIntegrationBuild=true --output bin", buildDir);
                packagePath = Path.Combine(buildDir, "bin", $"Framework.Analyzers.StaticGenerator.{version.NuGetVersion}.nupkg");
                result.Add(new NuGetPackage(packagePath, "Framework.Analyzers.StaticGenerator"));
            }

            return Task.FromResult(result);
        }

        public static Task Publish(Options options, GitVersion version, List<NuGetPackage> packages)
        {
            var apiKey = Environment.GetEnvironmentVariable("NUGET_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("NuGet API key is missing. Please set the NUGET_API_KEY environment variable.");
            }

            var isSandbox = options.Host switch
            {
                Host.AppVeyor => Environment.GetEnvironmentVariable("APPVEYOR_REPO_NAME") != "aaubry/Framework",
                _ => false,
            };

            if (isSandbox)
            {
                WriteWarning("Skipped NuGet package publication because this is a sandbox environment");
            }
            else
            {
                foreach (var package in packages)
                {
                    Console.WriteLine($"nuget push {package.Path} -ApiKey *** -Source https://api.nuget.org/v3/index.json");
                    Run("nuget", $"push {package.Path} -ApiKey {apiKey} -Source https://api.nuget.org/v3/index.json", noEcho: true);

                    if (version.IsPreRelease && UnlistPackage)
                    {
                        Console.WriteLine($"nuget delete {package.Name} {version.NuGetVersion} -NonInteractive -ApiKey *** -Source https://api.nuget.org/v3/index.json");
                        Run("nuget", $"delete {package.Name} {version.NuGetVersion} -NonInteractive -ApiKey {apiKey} -Source https://api.nuget.org/v3/index.json", noEcho: true);
                    }
                }
            }

            return Task.CompletedTask;
        }
        public static async Task<ScaffoldedRelease> ScaffoldReleaseNotes(GitVersion version, PreviousReleases releases)
        {
            if (version.IsPreRelease)
            {
                throw new InvalidOperationException("Cannot release a pre-release version.");
            }

            var previousVersion = releases.Versions.First();

            // Get the git log to scaffold the release notes
            string? currentHash = null;
            var commits = (await ReadLines("git", $"rev-list v{previousVersion}..HEAD --first-parent --reverse --pretty=tformat:%B"))
                .Select(l =>
                {
                    var match = Regex.Match(l, "^commit (?<hash>[a-f0-9]+)$");
                    if (match.Success)
                    {
                        currentHash = match.Groups["hash"].Value;
                    }
                    return new
                    {
                        message = l,
                        commit = currentHash
                    };
                })
                .GroupBy(l => l.commit, (k, list) => new
                {
                    commit = k,
                    message = list
                        .Skip(1)
                        .Select(l => Regex.Replace(l.message, @"\+semver:\s*\w+", "").Trim())
                        .Where(l => !string.IsNullOrEmpty(l))
                        .ToList()
                });

            var log = commits
                .Select(c => c.message.Select((l, i) => $"{(i == 0 ? '-' : ' ')} {l}"))
                .Select(c => string.Join("  \n", c));

            var releaseNotes = string.Join("\n\n", log);

            WriteVerbose(releaseNotes);

            return new ScaffoldedRelease(releaseNotes);
        }

        public static async Task CreateGithubRelease(GitVersion version, ScaffoldedRelease release)
        {
            var releaseResponse = await GitHubClient.Value.PostAsJsonAsync($"/repos/{GitHubRepository}/releases", new
            {
                tag_name = $"v{version.NuGetVersion}",
                target_commitish = version.Sha,
                name = $"Release {version.NuGetVersion}",
                body = release.ReleaseNotes,
                draft = true,
                prerelease = version.IsPreRelease,
            });

            releaseResponse.EnsureSuccessStatusCode();

            var releaseInfo = await releaseResponse.Content.ReadAsAsync<GitHubApiModels.Release>();
            WriteImportant($"Release draft created:\n{releaseInfo.html_url}");
        }

        public static async Task LinkPullRequestsToReleases(GitVersion version)
        {
            if (version.IsPreRelease)
            {
                WriteWarning("Not linking to pre-releases.");
                return;
            }

            var releaseResponse = await GitHubClient.Value.GetAsync($"/repos/{GitHubRepository}/releases/tags/v{version.NuGetVersion}");

            var release = await releaseResponse.EnsureSuccessStatusCode().Content.ReadAsAsync<GitHubApiModels.Release>();

            var linkedIssues = Regex.Matches(release.body ?? string.Empty, @"#(\d+)").Select(m => m.Groups[1].Value);
            WriteVerbose($"Found the following issues / pull requests: {string.Join(",", linkedIssues)}");

            foreach (var issueNumber in linkedIssues)
            {
                var prResponse = await GitHubClient.Value.GetAsync($"/repos/{GitHubRepository}/pulls/{issueNumber}");
                var isIssue = prResponse.StatusCode == HttpStatusCode.NotFound;

                var commentResponse = await GitHubClient.Value.PostAsJsonAsync($"/repos/{GitHubRepository}/issues/{issueNumber}/comments", new
                {
                    body = isIssue
                        ? $"A fix for this issue has been released in [version {version.NuGetVersion}](https://github.com/{GitHubRepository}/releases/tag/v{version.NuGetVersion})."
                        : $"This feature has been released in [version {version.NuGetVersion}](https://github.com/{GitHubRepository}/releases/tag/v{version.NuGetVersion})."
                });

                commentResponse.EnsureSuccessStatusCode();
            }
        }

        public static async Task<PreviousReleases> DiscoverPreviousReleases()
        {
            // Find previous release
            var releases = (await ReadLines("git", "tag --list --merged origin/main --format=\"%(refname:short)\" v*"))
                .Select(tag => Regex.Match(tag.TrimEnd('\r'), @"^v(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)$"))
                .Where(m => m.Success)
                .Select(match => new Version(
                    int.Parse(match.Groups["major"].Value, CultureInfo.InvariantCulture),
                    int.Parse(match.Groups["minor"].Value, CultureInfo.InvariantCulture),
                    int.Parse(match.Groups["patch"].Value, CultureInfo.InvariantCulture)
                ))
                .OrderByDescending(v => v)
                .ToList();

            var previousReleases = new PreviousReleases(releases);
            WriteInformation($"The previous release was {previousReleases.Latest}");
            WriteVerbose("Releases:\n - " + string.Join("\n - ", releases));

            return previousReleases;
        }

        public static Task Document(Options options)
        {
            var samplesProjectDir = Path.Combine(BasePath, "Framework.Samples");
            var samplesOutputDir = Path.Combine(BasePath, "..", "Framework.wiki");

            var verbosity = options.Verbose ? "detailed" : "minimal";
            Run("dotnet", $"test Framework.Samples.csproj --no-build --configuration Release --verbosity {verbosity} --logger \"trx;LogFileName=TestResults.trx\"", samplesProjectDir);

            var report = XDocument.Load(Path.Combine(samplesProjectDir, "TestResults", "TestResults.trx"));

            const string ns = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";

            var testDefinitions = report.Root!
                .Element(XName.Get("TestDefinitions", ns))!
                .Elements(XName.Get("UnitTest", ns))
                .Select(e =>
                {
                    var testMethod = e.Element(XName.Get("TestMethod", ns))!;

                    var sampleClassName = testMethod.Attribute("className")!.Value;
                    var sampleMethodName = testMethod.Attribute("name")!.Value;

                    var testMethodAssembly = Assembly.LoadFrom(testMethod.Attribute("codeBase")!.Value);

                    var sampleClass = testMethodAssembly
                        .GetType(sampleClassName, true)!;

                    var sampleMethod = sampleClass
                        .GetMethod(sampleMethodName, BindingFlags.Instance | BindingFlags.Public)
                        ?? throw new InvalidOperationException($"Method {sampleClassName}.{sampleMethodName} not found");

                    var sampleAttr = sampleMethod
                        .GetCustomAttributes()
                        .Single(a => a.GetType().Name == "SampleAttribute");

                    var description = UnIndent((string)sampleAttr.GetType().GetProperty("Description")!.GetValue(sampleAttr, null)!);

                    return new
                    {
                        Id = e.Attribute("id")!.Value,
                        Name = e.Attribute("name")!.Value,
                        Description = description,
                        Code = File.ReadAllText(Path.Combine(samplesProjectDir, $"{sampleClass.Name}.cs")),
                        FileName = $"Samples.{sampleClass.Name}.md",
                    };
                });

            var testResults = report.Root
                .Element(XName.Get("Results", ns))!
                .Elements(XName.Get("UnitTestResult", ns))
                .Select(e => new
                {
                    TestId = e.Attribute("testId")!.Value,
                    Output = e
                        .Element(XName.Get("Output", ns))
                        ?.Element(XName.Get("StdOut", ns))
                        ?.Value
                });

            var samples = testDefinitions
                .GroupJoin(
                    testResults,
                    t => t.Id,
                    r => r.TestId,
                    (t, r) => new
                    {
                        t.Name,
                        t.Description,
                        t.Code,
                        t.FileName,
                        r.Single().Output, // For now we only know how to handle a single test result
                    }
                );

            var sampleList = new StringBuilder();

            foreach (var sample in samples)
            {
                WriteInformation($"Generating sample documentation page for {sample.Name}");

                File.WriteAllText(Path.Combine(samplesOutputDir, sample.FileName), @$"
# {sample.Name}

{sample.Description}

## Code

```C#
{sample.Code}
```

## Output

```
{sample.Output}
```
");

                sampleList
                    .AppendLine($"* *[{sample.Name}]({Path.GetFileNameWithoutExtension(sample.FileName)})*  ")
                    .AppendLine($"  {sample.Description.Replace("\n", "\n  ")}\n");
            }

            File.WriteAllText(Path.Combine(samplesOutputDir, "Samples.md"), $@"
# Samples

{sampleList}

* [Building Custom Formatters for .Net Core (Yaml Formatters)](http://www.fiyazhasan.me/building-custom-formatters-for-net-core-yaml-formatters/) by @FiyazBinHasan
");
            return Task.CompletedTask;
        }

        private static string GitHubRepository
        {
            get
            {
                var repository = Program.Host switch
                {
                    Host.AppVeyor => Environment.GetEnvironmentVariable("APPVEYOR_REPO_NAME"),
                    _ => Environment.GetEnvironmentVariable("GITHUB_REPOSITORY")
                };
                if (repository == null)
                {
                    WriteWarning($"Could not determine the current repository for host {Program.Host}. Falling-back to sandbox!");
                    repository = "aaubry/Framework.Sandbox";
                }
                return repository;
            }
        }

        private static readonly Lazy<HttpClient> GitHubClient = new Lazy<HttpClient>(() =>
        {
            var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN") ?? throw new InvalidOperationException("Please set the GITHUB_TOKEN environment variable.");

            var gitHubClient = new HttpClient(new LoggerHttpHandler())
            {
                BaseAddress = new Uri("https://api.github.com"),
            };

            gitHubClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            gitHubClient.DefaultRequestHeaders.Add("User-Agent", GitHubRepository.Split('/')[0]);
            gitHubClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return gitHubClient;
        });
    }

    public class GitVersion : IEquatable<Version>
    {
        public GitVersion()
        {
        }

        public GitVersion(string version)
        {
            var partsMatch = Regex.Match(version, @"^(?<major>\d+).(?<minor>\d+).(?<patch>\d+)(?:-(?<label>.*))?");
            if (!partsMatch.Success)
            {
                throw new ArgumentException($"Invalid version name '{version}'.", nameof(version));
            }

            Major = int.Parse(partsMatch.Groups["major"].Value, CultureInfo.InvariantCulture);
            Minor = int.Parse(partsMatch.Groups["minor"].Value, CultureInfo.InvariantCulture);
            Patch = int.Parse(partsMatch.Groups["patch"].Value, CultureInfo.InvariantCulture);
            PreReleaseLabel = partsMatch.Groups["label"].Value;
            CommitsSinceVersionSourcePadded = "0000";
            Sha = "invalid commit name";
        }

        public int Major { get; set; }
        public int Minor { get; set; }
        public int Patch { get; set; }
        public string? PreReleaseLabel { get; set; }
        public string? CommitsSinceVersionSourcePadded { get; set; }
        public string? Sha { get; set; }

        public string NuGetVersion
        {
            get
            {
                return IsPreRelease
                    ? $"{MajorMinorPatch}-{PreReleaseLabel}-{CommitsSinceVersionSourcePadded}"
                    : MajorMinorPatch;
            }
        }

        public string MajorMinorPatch => $"{Major}.{Minor}.{Patch}";

        public int CommitsSinceVersionSource { get; set; }

        public bool IsPreRelease => !string.IsNullOrEmpty(PreReleaseLabel);

        public bool Equals(Version? other)
        {
            return other is object
                && Major == other.Major
                && Minor == other.Minor
                && Patch == other.Build;
        }
    }

    public struct MetadataSet { }

    public struct SuccessfulBuild { }
    public struct SuccessfulUnitTests { }

    public class ScaffoldedRelease
    {
        public ScaffoldedRelease(string releaseNotes)
        {
            ReleaseNotes = releaseNotes;
        }

        public string ReleaseNotes { get; set; }
    }

    public class PreviousReleases
    {
        public PreviousReleases(IEnumerable<Version> versions)
        {
            Versions = versions;
        }

        public IEnumerable<Version> Versions { get; }

        public Version Latest => Versions.First();
    }

    public class NuGetPackage
    {
        public NuGetPackage(string path, string name)
        {
            Path = path;
            Name = name;
        }

        public string Path { get; }
        public string Name { get; }
    }

    internal class LoggerHttpHandler : HttpClientHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string requestText = string.Empty;
            var allHeaders = request.Headers.AsEnumerable();

            if (request.Content is object)
            {
                requestText = await request.Content.ReadAsStringAsync();
                allHeaders = allHeaders.Concat(request.Content.Headers);
            }

            var requestHeaders = allHeaders.Select(h => $"\n{h.Key}: {string.Join(", ", h.Value)}");

            WriteVerbose($"> {request.Method} {request.RequestUri}{string.Concat(requestHeaders)}\n\n{requestText}\n".Replace("\n", "\n> "));

            var response = await base.SendAsync(request, cancellationToken);

            var responseText = await response.Content.ReadAsStringAsync();
            var responseHeaders = response.Headers.Concat(response.Content.Headers)
                .Select(h => $"\n{h.Key}: {string.Join(", ", h.Value)}");

            WriteVerbose($"< {(int)response.StatusCode} {response.ReasonPhrase}{string.Concat(responseHeaders)}\n\n{responseText}\n".Replace("\n", "\n< "));

            return response;
        }
    }
}