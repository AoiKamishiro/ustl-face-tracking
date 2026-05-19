---
id: usage
title: Usage Guide
---

## Install the package

Add the U-Stella VPM repository to VCC or ALCOM.

```text
https://ustl-vpm.kamishiro.online/index.json
```

Then add `U-Stella FaceTracking` to your Unity project from the package list.

Required packages are resolved through VPM.

## Add the component

1. Open the target avatar prefab or scene avatar in Unity.
2. Select the avatar root, or a child object where you want to keep the settings.
3. Add the `U-Stella/U-Stella FaceTracking` component from the Inspector.
4. Assign a `SkinnedMeshRenderer` with face-tracking blend shapes to `Face Mesh Renderer`.

## Select tracking devices

Use `Tracking Devices` to select the face-tracking devices you want the avatar to support.
You can select more than one device.

The support status shows how each facial expression is handled by the selected devices.

| Status | Meaning |
| --- | --- |
| Fully Supported | The device can output that expression directly. |
| Converted | The expression can be produced through conversion, emulation, merged left/right values, or module-side processing. |
| Unknown | Public information is not enough to confirm support. |
| Unsupported | Support has not been confirmed. |

## Configure features

In `Feature Settings`, choose how each facial feature should be used.
This includes eyes, eyelids, brows, mouth, tongue, and related face-tracking features.

| Item | Description |
| --- | --- |
| Feature | The facial feature being configured. |
| Device | Shows whether the selected devices support the chosen output format. |
| Output Format | Selects how the feature should be represented, such as separate left/right values or a shared value. |
| Sync Mode | Selects how the feature should be synchronized. |

Use the sync cost display at the bottom of the Inspector to check the current parameter usage.

## Assign blend shapes

Use `Blend Shape Assignments` to map each face-tracking expression to a blend shape on your face mesh.

1. Set `Face Mesh Renderer`.
2. Select the matching blend shape for each expression.
3. Adjust `Maximum Value` when needed. Values are in the `0` to `100` range.
4. Expressions that are not used by the current feature settings are disabled.

If a blend shape has the same name as an expression, it may be filled automatically when the face mesh is assigned.
If a missing blend shape name remains in the settings, it is shown as an invalid value in the Inspector.
