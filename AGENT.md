# WisdomWarrior Editor Agent Guide

## Mission
Build a fast, modular 2D game engine editor using MonoGame + Avalonia, with iteration speed and runtime performance as top priorities.

## Product Context (Current Solution)
- Solution: `WisdomWarrior.Editor.sln`
- Editor projects:
  - `WisdomWarrior.Editor.Shell` (composition root / app startup)
  - `WisdomWarrior.Editor.Core` (shared editor services, trackers, helpers)
  - `WisdomWarrior.Editor.MonoGame` (viewport runtime, overlays, tools)
  - `WisdomWarrior.Editor.Inspector` (component/property editing UI)
  - `WisdomWarrior.Editor.SceneList` (scene hierarchy UI)
  - `WisdomWarrior.Editor.AssetBrowser` (filesystem/asset browsing UI)
  - `WisdomWarrior.Editor.FileSystem` (workspace, manifest, file operations, watchers)
  - `WisdomWarrior.Editor.Menus` (project creation/open flows)
- Referenced engine projects (sibling repo):
  - `../Engine/WisdomWarrior.Engine.Core`
  - `../Engine/WisdomWarrior.Engine.MonoGame`

## Non-Negotiable Architecture Boundary
- Editor code must never live in Engine code.
- The only approved editor-facing type in Engine is `HideInInspectorAttribute`.
- Engine must not reference Editor assemblies, namespaces, packages, or concepts.
- Dependency direction is one-way:
  - Allowed: `Editor -> Engine`
  - Forbidden: `Engine -> Editor`

## Rules For All Future Changes
- Keep features in the correct module instead of adding shortcuts in `Shell`.
- Preserve MVVM boundaries:
  - ViewModels own state/commands.
  - Views handle visual composition.
  - Services manage IO/runtime/workspace concerns.
- Respect DI composition in `WisdomWarrior.Editor.Shell/Configurations/Configurations.cs`.
- For inspector/runtime integration, prefer extending editor-side trackers/tools over changing engine internals.
- Avoid blocking UI thread for filesystem or long-running tasks.
- In MonoGame/editor runtime code, avoid unnecessary per-frame allocations.

## Engine Safety Checks (Mandatory When Touching Engine Or References)
- Verify no `using WisdomWarrior.Editor*` in Engine projects.
- Verify Engine `.csproj` files do not reference any Editor project/package.
- If metadata for editor UX is needed, default to editor-side reflection/adapters first.
- Any new engine attribute or API intended for editor use must stay generic and runtime-safe.

## Delivery Standards
- Make minimal, targeted changes that compile.
- Keep code simple and explicit; prefer small focused classes.
- Add/update tests when behavior changes (especially serialization, scene tracking, file operations).
- Call out tradeoffs and risks clearly when constraints conflict.

## Prioritization Heuristics
When multiple approaches are valid, prefer options that:
1. Improve edit-compile-test iteration speed.
2. Reduce coupling between modules.
3. Preserve runtime performance and determinism.
4. Keep engine clean from editor concerns.

## Suggested Next Improvements
- Add an automated architecture check that fails CI if Engine references Editor.
- Add a shared `Directory.Build.props` for common defaults and analyzer rules.
- Add tests around scene serialization/deserialization and tracker dirty-state behavior.
- Replace absolute local DLL hint paths (for ColorPicker) with package-managed references.
