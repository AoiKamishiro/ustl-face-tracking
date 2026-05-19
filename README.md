# U-Stella FaceTracking

English | [日本語](README.ja.md)

U-Stella FaceTracking is a Unity editor extension that helps set up face tracking for VRChat avatars.
It provides an Inspector workflow for choosing tracking devices, selecting the facial features you want to use, and assigning blend shapes on your avatar's face mesh.

## Installation

Add the U-Stella VPM repository to VCC or ALCOM.

```text
https://ustl-vpm.kamishiro.online/index.json
```

If VCC or ALCOM can be launched from your browser, you can add the repository with this URL.

```text
vcc://vpm/addRepo?url=https%3A%2F%2Fustl-vpm.kamishiro.online%2Findex.json
```

After adding the repository, add the `U-Stella FaceTracking` package to your Unity project.

## Requirements

- Unity 2022.3
- A VRChat avatar project
- Modular Avatar
- A face mesh with blend shapes for face tracking

Required packages are resolved through VPM.

## Basic Usage

1. Open the target avatar prefab or scene avatar in Unity.
2. Select the avatar root, or a child object where you want to keep the settings.
3. Add the `U-Stella/U-Stella FaceTracking` component from the Inspector.
4. Assign a `SkinnedMeshRenderer` with face-tracking blend shapes to `Face Mesh Renderer`.
5. Configure tracking devices, feature settings, and blend shape assignments.

## Documentation

- [Documentation Home](https://ustl-face-tracking.kamishiro.online/)
- [Usage Guide](https://ustl-face-tracking.kamishiro.online/usage/)
- [Adding Hardware Profiles](https://ustl-face-tracking.kamishiro.online/add-hardware-profile/)

## License

[Apache License 2.0](LICENSE)
