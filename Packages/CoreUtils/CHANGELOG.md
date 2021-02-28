# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

<!-- ## [Unreleased]
-  -->

## [1.0.7] - 2020-2-3

### Fixed
- Incorrect `StateMachine` initialization
- Handling of `PauseOnEnable` out-of-order enable/disables

### Changed
- Added min/max events to `GameVariableFloatRange`
- Added optional `TriggerEvent` to `TriggerSound` component

## [1.0.6] - 2020-2-3

### Added
- Added `[AutoFillFromScene]` property attribute to auto-fill an object reference from anywhere in the scene.


## [1.0.5] - 2020-1-20

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


[Unreleased]: https://github.com/fantabulous-tech/coreutils/compare/v1.0.7...HEAD
[1.0.7]: https://github.com/fantabulous-tech/coreutils/compare/v1.0.6...v1.0.7
[1.0.6]: https://github.com/fantabulous-tech/coreutils/compare/v1.0.5...v1.0.6
[1.0.5]: https://github.com/fantabulous-tech/coreutils/compare/v1.0.4...v1.0.5
[1.0.4]: https://github.com/fantabulous-tech/coreutils/compare/v1.0.3...v1.0.4
[1.0.3]: https://github.com/fantabulous-tech/coreutils/compare/v1.0.2...v1.0.3
[1.0.2]: https://github.com/fantabulous-tech/coreutils/compare/v1.0.1...v1.0.2
[1.0.1]: https://github.com/fantabulous-tech/coreutils/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/fantabulous-tech/coreutils/releases/tag/v1.0.0