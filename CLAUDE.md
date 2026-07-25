# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

EWovaLinkBand is a Unity XR package (`com.ewova.wristband`) that renders a VR wristband UI for Meta Quest / Android XR devices. It lets users launch EWova-platform actions (navigate, screenshot, share, etc.) from a wrist-mounted radial menu inside any XR app that installs this package.

The UPM package is distributed via git URL:
```
https://github.com/EWova/Wristband.git?path=Assets/EWova.Wristband
```

## Development Environment

Open in Unity Editor (URP, XR project). Build target is Android (Meta Quest via `com.unity.xr.meta-openxr` + OpenXR).

**Switch API environment:** `EWova → Editor → DeploymentMode` menu in Unity. Dev hits `https://wristbands.ewova.dev/api/v1`; Production hits `https://wristbands.ewova.com/api/v1`.

**Package versioning:** Edit `Assets/EWova.Wristband/package.json`. On save, `PackageJsonPostprocessor` triggers `PkgVerGen` which auto-regenerates `Assets/EWova.Wristband/Runtimes/Scripts/PackageInfo.cs`. Never edit `PackageInfo.cs` manually — it is regenerated on every `package.json` import. Version format: `YYYY.MM.Build`.

## Architecture

### Core Components (`Assets/EWova.Wristband/Runtimes/Scripts/`)

| File | Role |
|---|---|
| `Wristband.cs` | Root MonoBehaviour. Manages menu open/close animation (EaseInExpo/EaseOutExpo), 5-second idle auto-close, pointer-enter hover suppression, feature-flag visibility, and runtime language switching. |
| `WristbandUI.cs` | Ticks the clock display (HH:MM) at 0.5s intervals with a blinking separator. |
| `Setup.cs` | Companion component — reads a comma-separated `Flag` string on `Start()` and calls `Wristband.LoadFlag()` to activate/deactivate button groups. |
| `CircleButtonElement.cs` | Reusable UI primitive: a circular button with a radial fill indicating async load progress. Disabled until `IsDone = true`. |
| `CircleGroupLayout.cs` | Custom `LayoutGroup` that arranges children in an evenly-spaced circle by radius/startAngle. |
| `Follower.cs` | `LateUpdate` smooth-damps the wristband transform toward a `Pivot` (the wrist anchor) and optionally faces a `LookAt` target. |
| `Logger.cs` | Thin wrapper around `EWova.Logger` with prefix `[EWova]Wristband`. |

### Button System (`Elements/`)

All action buttons extend `BaseBTN` (abstract):
- `Load(LoadProcess)` — async setup; call `loadProcess.SetComplete()` or `SetFailed()` when done
- `ProcessClick()` — async click handler

Available concrete buttons: `GoEWovaBTN`, `ScreenshotBTN`, `ShareBTN`, `ViewLearningProfileBTN`, `GoEWovaWebsiteBTN`, `QuitAppBTN`.

### Feature Flags

Feature visibility is driven by string flags. The default `Setup.Flag` value is:
```
GO_TO_EWOVA,CAPTURE_TO_EWOVA,SHARE_TO_EWOVA,VIEW_LEARNING_PROFILE,EXPLORE_EWOVA_WEBSITE,QUIT_APP
```
Each `FeatureGroup` in `Wristband` maps a flag string to a `CircleButtonElement`. `Wristband.LoadFlag(string)` enables only groups whose flag appears (case-insensitive) in the provided string.

### API Client (`ApiClient/`)

`WApiClient` extends `AuthApiClient` (from `com.ewova.core`). It is a partial class split across:
- `WApiClient.cs` — URL selection, constructor, package registration
- `WApiClient.Method.cs` — typed async methods (`UploadScreenshotAsync`, `ShareActivityAsync`); wraps `ApiException` in `ApiWristbandException`

DTOs live in `ApiModels.cs` with the following shape:
- `UploadScreenshotRequest` / `UploadScreenshotResponse` — image bytes → CDN URL + ID
- `ShareActivityRequest` — imageUrl + description

### Localization

Strings are stored in TSV format at `Assets/EWova.Wristband/Runtimes/Resources/Localization/Wristband.tsv` with columns `Key | en | zh-Hant | zh | ja | ko | vi`.

**Adding a string:** Add a row to the TSV. `TSVImporter` (a custom `ScriptedImporter`) handles `.tsv` as `TextAsset`.

`DefaultTextProvider.LoadFromFile("Localization/Wristband")` parses the TSV at runtime via `Resources.Load`. Falls back to `en` if a key is missing for the active language; returns `[Key]` if absent in all languages.

`Localizer.DoLocalizeUpdate(ITextProvider)` walks all `ILocalizeUpdater` children and pushes new strings. Language can be set at runtime via `Wristband.LocalizationLang`.

### Key Dependencies (from `Packages/manifest.json`)

| Package | Source |
|---|---|
| `com.ewova.core` | `https://github.com/EWova/UnityPackageCore.git?path=Assets/EWova.Core#Dev` |
| `com.cysharp.unitask` | `https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask` |
| XR stack | `com.unity.xr.hands`, `com.unity.xr.interaction.toolkit`, `com.unity.xr.meta-openxr`, `com.unity.xr.openxr` |
| Rendering | URP (`com.unity.render-pipelines.universal`) |

## Adding a New Button

1. Create a class extending `BaseBTN` in `Elements/`.
2. Override `LabelKey`, `DescriptionKey`, `Load(LoadProcess)`, and `ProcessClick()`.
3. Add a `FeatureGroup` entry in the `Wristband` prefab with a new flag string.
4. Add localization keys to `Wristband.tsv`.
