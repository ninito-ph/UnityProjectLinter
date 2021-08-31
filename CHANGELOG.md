# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.5] - 2021-08-31
### Fixed
- Fixed bug where _Variant prefab rule would not be checked properly

## [1.0.4] - 2021-08-31
### Added
- Added progress bar when renaming assets
- Added pagination to asset renamer window
- Safety check if trying to rename asset to illegal filename
### Fixed
- Massively improved performance in asset renamer window through pagination.

## [1.0.3] - 2021-08-30
### Fixed
- Fixed null reference exception in asset renamer window.

## [1.0.2] - 2021-08-30
### Fixed
- Improved performance in asset renamer window.

## [1.0.1] - 2021-08-30
### Fixed
- Improved UX when asset cannot be renamed.
- Fixed null reference exception errors when a null custom naming rule was added.
- Fixed all assets being ignored when an empty path was added to the ignored paths.
- Fixed null reference exception errors when a null asset was ignored.