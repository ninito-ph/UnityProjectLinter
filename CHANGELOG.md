# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
## [2.1.0] - Q1 2022
### Added
- Basic API for CI/CD support
- Test Runner test for checking if all assets are named correctly if you want to take advantage of your already-existing test CI/CD pipeline
- RulesetOverride ScriptableObject to allow you to override linting rules in specific folders
- Improved asset renamer tool
- Basic rule exception support

## [2.0.0] - 2022-01-27
### Added
- Logger selection dropdown interface
- CSV Asset Violation Logger
- Default rules as individual, customizable ScriptableObjects (SimplePrefixNamingRule, RegexNamingRule, VariantSuffixNamingRule and ReplaceSectionNamingRule)
- Support for rule priority, to decide which rule will be considered if two rules apply to an asset
- Support for Infix rule context, to check that an asset's name, not just its prefix/suffix, follows appropriate conventions
### Changed
- Made UnityProjectLinter settings an actual SettingsProvider that can be found in Project Settings
- Improved custom logger support; now you should be able to simply create a class that implements IRuleViolationLogger and it will automatically show up in the logger selection dropdown
- Moved scripts to new Ninito.UnityProjectLinter.Editor asmdef
- Moved default/sample rules to Ninito.UnityProjectLinter.Editor.Samples asmdef
- Refactored the whole project's codebase for greater extensibility and readability
### Removed
- Default hardcoded rules, except for ignoring script assets (Default rules have been replaced by more modular and extensible ScriptableObjects in the Samples folder).

## [1.0.7] - 2021-09-08
### Fixed
- Fixed rare bug in which asset renamer window get NullReferenceException from trying to retrieve asset type from broken/corrupted assets.

## [1.0.6] - 2021-09-04
### Fixed
- Fixed bug where Assets folder path was written incorrectly, resulting in inability to obtain project assets.
- Fixed bug where GUI focus would remain on a field after changing pages in the Asset Renamer Window, resulting in an incorrect display of information.

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