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
