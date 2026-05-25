---
id: add-hardware-profile
title: Adding Hardware Profiles
---

This page is for contributors who want to add a new face-tracking device to the support table.
When adding hardware, add a hardware profile JSON and always keep a source URL that explains the support status or the reason for the judgment.

## Files to update

| Purpose | Path |
| --- | --- |
| Support profile JSON | `Editor/Data/HardwareSupport/Profiles/*.json` |
| Expression list, only when adding a new expression key | `Runtime/Data/Expressions/UnifiedExpression.cs` |
| Generated hardware enum | `Runtime/Data/HardwareSupport/SupportedHardwares.generated.cs` |
| Generated support definition dictionary | `Editor/Data/HardwareSupport/HardwareSupportProfileDefinition.generated.cs` |

## 1. Add the support profile JSON

Add a JSON file under `Editor/Data/HardwareSupport/Profiles/`.
The `profile` value is a stable unique key for the hardware profile.
The `id` value is used as both the Inspector order and the bit index for saved selections (`1 << id`), so do not reuse an existing value.

```json
{
  "profile": "NewHardware",
  "displayName": "New Hardware",
  "id": 18,
  "sources": [
    {
      "title": "New Hardware tracking specification",
      "url": "https://example.com/spec",
      "note": "Source used to map device output to UnifiedExpression."
    }
  ],
  "full": [
    "EyeLookOutRight",
    "EyeLookInRight",
    "EyeLookUpRight",
    "EyeLookDownRight"
  ],
  "converted": [
    "EyeClosedRight"
  ],
  "unknown": [
    "TongueOut"
  ]
}
```

| Field | Required | Description |
| --- | --- | --- |
| `profile` | Required | Stable unique key for this hardware profile. |
| `displayName` | Required | The hardware name shown in the Inspector. |
| `id` | Required | Bit index and Inspector order. Use 0 through 30 and do not conflict with existing profiles. |
| `sources` | Required | Source material for the support status. Add at least one source. |
| `full` | Optional | `UnifiedExpression` values that the hardware can output directly. |
| `converted` | Optional | `UnifiedExpression` values supported through conversion, emulation, merged left/right values, or module-side processing. |
| `unknown` | Optional | `UnifiedExpression` values that cannot be confirmed from public information. |

Expressions that are not listed in `full`, `converted`, or `unknown` are treated as unsupported.

Changing an existing `id` changes the saved bit flag for that hardware profile. Treat existing IDs as persistent.

## 2. Check expression names

Only values from `UnifiedExpression` can be used in the JSON file.
Invalid names or `None` cause a load error.

If the device documentation uses its own expression names, map them to VRCFaceTracking Unified Expressions before adding them.
You do not need to list the same expression more than once in a single hardware profile.

## 3. Generate support code

After changing JSON, run `Tools > U-Stella > Face Tracking > Generate Hardware Support Data` in Unity.
This updates `SupportedHardwares.generated.cs` and `HardwareSupportProfileDefinition.generated.cs`.
Do not edit generated files manually.

## 4. Run tests

Run Unity EditMode tests and confirm that:

- The JSON profile loads correctly.
- `profile` values do not conflict.
- `id` values do not conflict.
- All `UnifiedExpression` names in JSON exist.
- The generated enum and support definition dictionary match the JSON.

## Support status criteria

| Status | Criteria |
| --- | --- |
| `full` | The hardware or VRCFT module outputs the expression value directly. |
| `converted` | The value is produced through estimation, shared left/right values, synthesized values, or module-side conversion. |
| `unknown` | The available information is not enough to determine support. |
| Unsupported | The expression is not supported, or support cannot be confirmed. |

When unsure, use `unknown` and explain the reason in `sources.note`.
