# Migrating the Web Enricher to Multi-Version Targeting

This document records the migration of `CluedIn.Enricher.Web` from a single CluedIn version to a
single branch that produces packages for multiple CluedIn versions.

## Overview

The shared `crawler.build.jobs.yml` pipeline template builds each CluedIn version on an isolated
agent and auto-detects the .NET target framework from the resolved `CluedIn.Core` package. The
framework must therefore not be hard-coded in `azure-pipelines.yml`.

| CluedIn version | .NET target framework | Test package suffix |
|---|---|---|
| 4.7.0 | net6.0 | `.470` |
| 4.8.0 | net6.0 | `.480` |
| 5.0.0-beta.* | net10.0 | `.500` |

The 5.0 target uses the current beta channel. The pipeline's `probeCluedInVersion` parameter can
perform a non-publishing build and test for a version outside this matrix.

## Pipeline

`azure-pipelines.yml` uses `crawler.build.jobs.yml` rather than the older `crawler.build.yml`
steps template. The jobs template supplies the SDK and agent setup for each matrix entry.

```yaml
jobs:
  - template: crawler.build.jobs.yml@templates
    parameters:
      pool:
        vmImage: 'ubuntu-22.04'
      probeCluedInVersion: ${{ parameters.probeCluedInVersion }}
      multiVersionCluedInTargets:
        - cluedInVersion: '4.7.0'
        - cluedInVersion: '4.8.0'
        - cluedInVersion: '5.0.0-beta.*'
```

The previous integration environment setup and teardown script parameters were removed: the
referenced `build/integration-test.ps1` file does not exist, and the integration test uses the
in-process `BaseExternalSearchTest` harness.

## Build and package configuration

`Directory.Build.props` accepts `CluedInMultiVersionTargetFramework`, while retaining `net10.0`
as the local-development default. It pins C# 13 so that the net6.0 legs can compile the project's
newer language features.

`Packages.props` lets the pipeline supply `_CluedIn`, removes its prerelease suffix for version
comparisons, and derives these conditional-compilation symbols:

- `CLUEDIN_V47`
- `CLUEDIN_V48`
- `CLUEDIN_V50`

The CluedIn 4.x and 5.x test-support packages are published under distinct package IDs, not one
package ID with multiple versions. The migration therefore computes a dotless, three-part package
suffix and references `CluedIn.Testing.Base.470`, `.480`, or `.500` as appropriate.

`Nuget.config` was renamed to `NuGet.Config` so package-source discovery is not dependent on the
case-insensitive Windows filesystem when the Ubuntu CI jobs run.

## Test projects

Test package references were moved from `test/Directory.Build.props` into the integration project.
This prevents xUnit v2 and v3 from both entering an older-target compile reference set.

- CluedIn 5.0 / net10.0 uses xUnit v3.
- CluedIn 4.7 and 4.8 / net6.0 use xUnit v2.

`Microsoft.NET.Test.Sdk` and `xunit.runner.visualstudio` are also selected conditionally for the
matching test framework generation.

## API compatibility audit

The migration crosses a RestSharp major-version boundary:

- CluedIn 4.7/4.8 uses RestSharp 106, with `Method.GET`, `IRestResponse`, and legacy
  `RestClient` configuration.
- CluedIn 5.0 uses RestSharp 114, with `Method.Get`, `RestResponse`, and `RestClientOptions`.

The Web enricher has a broader RestSharp surface than GoogleMaps because it serializes HTTP
responses and passes reconstructed responses to `OrganizationWebsiteParser`. The compatibility
branches therefore cover:

- `RestClientOptions` versus the legacy `RestClient(Uri)` initializer;
- `Method.Get` versus `Method.GET`;
- `RestResponse` versus `IRestResponse`; and
- scalar versus collection response content-encoding values, as well as legacy cookie handling.

The provider DTOs under `Models` remain version-neutral. They contain only primitive data and JSON
attributes, so they do not need `CLUEDIN_V50` conditions.

## Semantic version

The repository's next semantic version is reset to `1.0`. The CluedIn version is represented by
the package suffix instead. `GitVersion.yml` ignores commits before `2026-06-18T00:00:00`, which
prevents the existing 4.x tags from dominating the new version line.

## Checklist

- [x] `azure-pipelines.yml` — switched to `crawler.build.jobs.yml` with
  `multiVersionCluedInTargets` for 4.7.0, 4.8.0, and 5.0.0-beta.*; added the non-publishing
  `probeCluedInVersion` parameter.
- [x] `Directory.Build.props` — honors `CluedInMultiVersionTargetFramework` with a net10.0 local
  fallback and pins C# 13 for the net6.0 legs.
- [x] `Packages.props` — preserves a pipeline-supplied `_CluedIn`, derives version symbols, and
  selects the version-suffixed `CluedIn.Testing.Base` package ID and matching test tooling.
- [x] `test/Directory.Build.props` — removed unconditional test package references so xUnit v2 and
  v3 cannot be included together.
- [x] Integration test project — selects xUnit v2 for CluedIn 4.x and xUnit v3 for CluedIn 5.0;
  references `CluedIn.Testing.Base.$(_CluedInPackageSuffix)`.
- [x] `NuGet.Config` — renamed from `Nuget.config` for case-sensitive Ubuntu CI environments.
- [x] Source — added `CLUEDIN_V50` compatibility branches for the RestSharp 106↔114 differences
  in Web request construction, verification responses, and serialized HTTP responses.
- [x] `GitVersion.yml` — reset `next-version` to 1.0 and added `ignore.commits-before`.
- [x] Source and provider projects build locally for the default CluedIn 5.0 / net10.0 target.
- [x] Source and integration tests build and run for the 4.7.0 / net6.0 target.
- [x] Source and integration tests build and run for the 4.8.0 / net6.0 target.
- [x] Integration tests build and run for the 5.0.0-beta.* / net10.0 target.
- [ ] Azure pipeline — all multi-version build/test, integration-test, and publish jobs pass.

### Local verification results

On 2026-09-10, all three target legs restored successfully from the configured feeds and built
successfully. The integration-test assembly completed for every leg with zero failures; its only
test (`WebTests.Test`) is explicitly skipped by the repository.

---

## Addendum — PackageId fix and version baseline moved to 100.0.0

Status: **Done**

This repo was migrated to multi-version targeting independently of the wider org-wide migration
effort, and picked up two gaps other repos had already found and fixed by the time this addendum
landed:

### PackageId derived in-props, not via pipeline override

`ExternalSearch.Providers.Web.Provider` has a `ProjectReference` to `ExternalSearch.Providers.Web`.
This repo never had a `PackageId` property of its own - it relied entirely on the pipeline's raw
`-p:PackageId=` override. That override doesn't reach a `ProjectReference`'s own package identity
when NuGet computes that dependency for the nuspec (confirmed via a real repro across multiple
repos: `CluedIn-io/Azure-Extensions#631`, `CluedIn-io/CluedIn.Enricher.CVR#50`,
`CluedIn-io/CluedIn.Crawling.MasterDataServices#80`), so `ExternalSearch.Providers.Web.Provider`'s
nuspec would end up depending on a plain, unsuffixed `CluedIn.ExternalSearch.Providers.Web` instead
of the correct `.470`/`.480`/`.500` suffixed name.

Separately, `AzurePipelines.Templates#30` removed the `-p:PackageId=` pipeline override entirely
(it never worked for `ProjectReference`s anyway), so without a fix every package from this repo
would pack with a completely bare, unsuffixed `PackageId` - not just a broken internal dependency.

Fixed by deriving `PackageId` from `_CluedInPackageSuffix` directly in `Packages.props` (where that
variable is already computed), gated on `CluedInMultiVersionTargetFramework` so local development
keeps the plain `$(AssemblyName)`. Verified with a real local `dotnet pack` repro: packing
`ExternalSearch.Providers.Web.Provider` now produces a nuspec dependency on
`CluedIn.ExternalSearch.Providers.Web.500`, not the bare unsuffixed name.

### Version baseline moved from 1.0.0 to 100.0.0

By the time this repo's gaps were found, the rest of the migration effort had already moved from
resetting the version to `1.0.0` to starting at `100.0.0` instead. Reason: repos that were
previously at 4.x/5.x under the old single-version-targeting scheme would appear to "go backwards"
if their next version showed as `1.0.0` - `100.0.0` is unambiguously higher than any prior
single-version release number this repo ever had.

```yaml
next-version: 100.0
```

No `ignore.commits-before` trick is needed: `next-version` only needs help overriding an existing
tag when the configured value is *lower* than that tag (the original `1.0` reset needed it against
the `4.6.2` tag), and `100.0` is already higher than every pre-existing tag here. The
`ignore.commits-before` line was removed (kept `ignore.sha: []`).

### Historical release notes restored

The original migration deleted this repo's per-CluedIn-version release notes
(`docs/0.1.0-release-notes.md` through `docs/5.0.0-release-notes.md`, 16 files). That deletion no
longer makes sense now that the version baseline isn't being reset to a lower number that needs
"hiding" the old numbering - restored all 16 files from the commit immediately before their
deletion, alongside the new `docs/100.0.0-release-notes.md`.

- [x] `Packages.props` - `PackageId` derived from `_CluedInPackageSuffix`
- [x] `GitVersion.yml` - `next-version: 100.0`; `ignore.commits-before` removed (no longer needed)
- [x] `docs/1.0.0-release-notes.md` renamed to `docs/100.0.0-release-notes.md`
- [x] 16 historical per-version release notes files restored
