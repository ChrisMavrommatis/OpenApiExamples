# Changelog

All notable changes to OpenApiExamples are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and OpenApiExamples follows
[semantic versioning](https://semver.org/spec/v2.0.0.html).

**A release reads its notes from this file.** The section for the version being released is published as the
release body, so write these entries for the people using the package, not for the people writing it.

**Version headings carry no icon** - the release workflow parses them.

## [Unreleased]

## [1.2.0] - 2026-08-31

### 🔀 Changed

- **Examples now inherit your app's JSON settings.** `AddOpenApiExamples()` takes a copy of the
  `JsonSerializerOptions` your app already serializes with, so a naming policy, a `JsonStringEnumConverter`
  or an ignore rule applies to your examples and your schemas at once instead of being configured twice.
  Setting `options.JsonSerializerOptions` yourself still wins, and replaces the inherited copy rather than
  adding to it.
- **If your models are PascalCase and you never configured this, your examples change to camelCase**, which
  is what your schemas already said. To keep the old output, pass
  `options.JsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = null }`.

## [1.1.0] - 2026-08-30

### ✨ Added

- **`ResponseExamples<T>()` now accepts a string status code.** The overload existed on both endpoints and
  route groups but was declared with no access modifier, so it defaulted to private and only the `int` form
  was ever reachable. `ResponseExample`, the singular form, always exposed both. The pair now matches.
  Additive only, so nothing that compiled against 1.0.2 changes.

### 🔀 Changed

- Source Link is enabled, so a debugger steps into the library source, and symbols are published as a
  `.snupkg` beside the package.
- The package description and tags were rewritten for search on nuget.org, and `Copyright` was added.

## [1.0.2] - 2026-07-28

### 🛠️ Fixed

- **JSON examples are no longer double encoded.** Examples were serialized to a string and then written into
  the document as a JSON string value, so every example shipped escaped. Swagger UI, Scalar and generated
  SDKs rendered the escaped blob verbatim, and spec linters flagged every example as invalid. Examples are
  now written as real JSON objects, arrays and values. XML examples are unchanged, because XML inside a JSON
  example genuinely is a string.
- **`AddExamplesFormatter<T>()` now rejects duplicates.** Its guard compared the service type rather than the
  implementation type, so registering the same formatter twice silently succeeded. It now throws
  `InvalidOperationException`, as its error message always said it would.

### 🔀 Changed

- `Microsoft.OpenApi` pinned to `2.11.0`, clearing the high severity advisory GHSA-v5pm-xwqc-g5wc, a stack
  overflow on circular `$ref`s, which arrived transitively as `2.0.0`.
- `Microsoft.AspNetCore.OpenApi` raised to `10.0.10`.

Every generated document changes shape in this release, so regenerate any specs you commit or publish.

## [1.0.1] - 2025-11-26

### 🛠️ Fixed

- Multiple examples.

## [1.0.0] - 2025-11-26

### 🔀 Changed

- Updated to .NET 10. **Breaking**, because ASP.NET Core's OpenAPI support was rewritten and this package
  moved onto it.

## [0.0.3] - 2025-08-26

### 🔀 Changed

- Updated NuGet packages.

## [0.0.2] - 2025-05-18

### 🛠️ Fixed

- Static object creation.

## [0.0.1] - 2025-05-16

Initial release.

---

This file was written on 2026-08-30, after the fact. `0.0.1` to `1.0.2` are reconstructed from their GitHub
release bodies, which is all the record there is: the entries before `1.0.2` are one line each because that
is what those releases said.
