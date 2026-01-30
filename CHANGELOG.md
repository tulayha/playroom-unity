# Changelog

## [2.0.0](https://github.com/tulayha/playroom-unity/compare/v1.5.4...v2.0.0) (2026-01-30)


### ⚠ BREAKING CHANGES

* **package:** SDK installation changed from importing into Assets/ (with a Unity project import flow) to installing as a UPM package under Packages/ via Package Manager/Git URL. The previous npm build step for generating .jslib binaries is no longer required because binaries are included.

### Features

* **2d-platformer:** add player entities and dynamic score UI + sync shoot ([fd22b14](https://github.com/tulayha/playroom-unity/commit/fd22b148bc6941d4dc68f27fbe1eb87064a34456))
* **editor:** add menu item to add mock-mode prefab to active scene ([aac9840](https://github.com/tulayha/playroom-unity/commit/aac984030952483698200f4c24a84e5bc0419557))
* **editor:** add menu item to apply recommended project settings ([aac9840](https://github.com/tulayha/playroom-unity/commit/aac984030952483698200f4c24a84e5bc0419557))
* **editor:** add menu item to import WebGL template ([aac9840](https://github.com/tulayha/playroom-unity/commit/aac984030952483698200f4c24a84e5bc0419557))


### Bug Fixes

* onQuit not updating internal player list ([0a73652](https://github.com/tulayha/playroom-unity/commit/0a73652cd4bb9c7ae47e0cd21bc8bcd0a2e375a2))
* onQuit not updating internal player list ([4b4fe24](https://github.com/tulayha/playroom-unity/commit/4b4fe245372218a44f15cbdbf3836ad41bbb0a0e))
* update references after package migration ([aac9840](https://github.com/tulayha/playroom-unity/commit/aac984030952483698200f4c24a84e5bc0419557))
* WaitForState callback issue due to static override ([ac40548](https://github.com/tulayha/playroom-unity/commit/ac405485aa232544f4ed791bcc5b9e4cf4ddbb85))
* WaitForState callback issue due to static override ([71fdb14](https://github.com/tulayha/playroom-unity/commit/71fdb146995b2eff9e692b3d9e80c0fcdcd43e28))


### Refactors

* **package:** move SDK from Assets/ import to UPM package in Packages/ ([aac9840](https://github.com/tulayha/playroom-unity/commit/aac984030952483698200f4c24a84e5bc0419557))
* **platformer:** simplify player sync and controller flow ([ed23010](https://github.com/tulayha/playroom-unity/commit/ed2301037ac11ee163ac9bb3285209428b9bf907))
