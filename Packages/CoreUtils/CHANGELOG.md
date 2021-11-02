# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.19] - 2021-11-03

### Fixed
- GameVariables trying to display an unsupported type in the Inspector will now only error once


## [1.0.18] - 2021-11-01

### Fixed
- Catch certain situations where AssetImportTracker wouldn't properly notify of changes
- Update GuidDatabase (used by Asset Usages Window) to be faster processing lots of imports at once

## [1.0.17] - 2021-10-21

### Added
- CoreUtils Settings to Edit > Project Settings > CoreUtils
- Support for disabling Asset Bucket Watcher in CoreUtils Settings
- Support for disabling Asset Usages Watcher in CoreUtils Settings
- Notifications in various locations when functionality is disabled

## [1.0.16] - 2021-10-12

### Added
- Support for Excluded Paths in Asset Dragnet tool
- OnVariableFloatEvents component for triggering UnityEvents based on a GameVarialbeFloat change
- Explicit 'PlayOnAwake' option for TriggerSound component

### Changed
- Cleaned up Assest Dragnet config UI names

### Fixed
- Removed unused field compile warnings


## [1.0.15] - 2021-9-10

### Fixed
- Fixed overly verbose 'Cancel' logging in Delay system

## [1.0.14] - 2021-8-31

### Fixed
- Fixed broken namespace when creating C# Editor from a script with parens on a separate line

## [1.0.13] - 2021-8-31

### Updated
- Removed case sensitivity from StateMachine

## [1.0.12] - 2021-8-22

### Updated
- Expanded `SaveLoadVariables` to support external access

## [1.0.11] - 2021-8-16

### Added
- `SceneBucket` - Auto-collects scenes in a folder and adds them to the BuildSettings
- `SaveLoadVariables` - Subscribes to variables in a `GameVariableBucket` and saves their values to the PlayerPrefs when changed and loads them on start.
- `OptionStackBool` - Collects a list of changes to a boolean, implementing the last one added.
- `DropDownBinding` - Binds a `GameVariable` to a drop-down.
- `DistributeChildrenInGrid` - Auto-distributes child transforms in a horizontal grid.
- `OnGameVariableSetEnabled` - Set the `enabled` property on a component based on a `GameVariableBool` state.
- `UnityEventInt` - Additional concrete `UnityEvent<T>` type.

### Updated
- Improved GameVariable and AssetBucket `AutoFill` options

## [1.0.10] - 2021-6-28

### Updated
- Grouped CoreUtil menu items together

## [1.0.9] - 2021-4-14

### Updated
- Updated project to Unity 2019.4.23f1
- Updated Promise library to latest
- Separated Promise library to it's own folder


## [1.0.8] - 2021-3-9

### Updated
- Updated project to Unity 2019.4.20f1

### Fixed
- Improved `AssetBucket` refresh speed
- Improved `AssetUsages` scanning speed when objects have lots of references
- Added error checks to `IsEqual<T>` extention
- Added error checks to `UnityUtils.DestroyObject()`


## [1.0.7] - 2021-2-3

### Fixed
- Incorrect `StateMachine` initialization
- Handling of `PauseOnEnable` out-of-order enable/disables

### Changed
- Added min/max events to `GameVariableFloatRange`
- Added optional `TriggerEvent` to `TriggerSound` component


## [1.0.6] - 2021-2-3

### Added
- Added `[AutoFillFromScene]` property attribute to auto-fill an object reference from anywhere in the scene.


## [1.0.5] - 2021-1-20

### Added
- Added `GameVariableCamera` and `GameVariableTransform` GameVariable types
- Added `GameEventInt` GameEvent type
- Added a `StateEvents` component to `StateMachine` to trigger UnityEvents when states change

### Changed
- Added an optional string format to `ValueTextBinding` component

### Fixed
- Fixed `Asset Context > Create > Create C# Editor Script` menu's template to use `CoreUtils` namespace
- Disabled 'Test' button in `TriggerSound`'s editor to avoid editing the `AudioSource` when not playing


## [1.0.4] - 2020-12-30

### Updated
- Updated project to Unity 2020.2.1f1
- Moved Installation notes to root README file
- Ran CodeCleanup on all CoreUtils


## [1.0.3] - 2020-12-30

### Updated

- Moved README to Unity Documentation folder

### Fixed

- Documentation Typos


## [1.0.2] - 2020-12-30

### Added
- Added README and CHANGELOG

### Fixed
- Renamed LICENSE to work with Package Manager


## [1.0.1] - 2020-12-30

### Added
- Version tags

### Removed
- Git LFS


## [1.0.0] - 2020-12-29
### Added
- Unity Project
- CoreUtils package


[Unreleased]: https://github.com/fantabulous-tech/coreutils/compare/v1.0.19...HEAD
[1.0.19]: https://github.com/fantabulous-tech/coreutils/compare/v1.0.18...v1.0.19
[1.0.18]: https://github.com/fantabulous-tech/coreutils/compare/v1.0.17...v1.0.18
[1.0.17]: https://github.com/fantabulous-tech/coreutils/compare/v1.0.16...v1.0.17
[1.0.16]: https://github.com/fantabulous-tech/coreutils/compare/v1.0.15...v1.0.16
[1.0.15]: https://github.com/fantabulous-tech/coreutils/compare/v1.0.14...v1.0.15
[1.0.14]: https://github.com/fantabulous-tech/coreutils/compare/v1.0.13...v1.0.14
[1.0.13]: https://github.com/fantabulous-tech/coreutils/compare/v1.0.12...v1.0.13
[1.0.12]: https://github.com/fantabulous-tech/coreutils/compare/v1.0.11...v1.0.12
[1.0.11]: https://github.com/fantabulous-tech/coreutils/compare/v1.0.10...v1.0.11
[1.0.10]: https://github.com/fantabulous-tech/coreutils/compare/v1.0.9...v1.0.10
[1.0.9]: https://github.com/fantabulous-tech/coreutils/compare/v1.0.8...v1.0.9
[1.0.8]: https://github.com/fantabulous-tech/coreutils/compare/v1.0.7...v1.0.8
[1.0.7]: https://github.com/fantabulous-tech/coreutils/compare/v1.0.6...v1.0.7
[1.0.6]: https://github.com/fantabulous-tech/coreutils/compare/v1.0.5...v1.0.6
[1.0.5]: https://github.com/fantabulous-tech/coreutils/compare/v1.0.4...v1.0.5
[1.0.4]: https://github.com/fantabulous-tech/coreutils/compare/v1.0.3...v1.0.4
[1.0.3]: https://github.com/fantabulous-tech/coreutils/compare/v1.0.2...v1.0.3
[1.0.2]: https://github.com/fantabulous-tech/coreutils/compare/v1.0.1...v1.0.2
[1.0.1]: https://github.com/fantabulous-tech/coreutils/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/fantabulous-tech/coreutils/releases/tag/v1.0.0