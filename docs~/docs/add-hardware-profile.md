---
id: add-hardware-profile
title: Adding Hardware Profiles
---

This page is for contributors who want to add a new face-tracking device to the support table.
When adding hardware, always keep a source URL that explains the support status or the reason for the judgment.

## Files to update

| Purpose | Path |
| --- | --- |
| Hardware ID | `Runtime/Data/FaceTrackingHardwareProfile.cs` |
| Support profile JSON | `Editor/Data/HardwareSupport/Profiles/*.json` |
| Expression list | `Runtime/Data/Expressions/UnifiedExpression.cs` |
| Tests | `Tests/Editor/HardwareSupportDataTests.cs` |

## 1. Add the hardware ID

Add a new value to `FaceTrackingHardwareProfile`.
The values are bit flags, so use the next `1 << N` after the current largest value.

```csharp
NewHardware = 1 << 18,
```

`None` is reserved for the unselected state. Do not use it for a hardware definition.

## 2. Add the support profile JSON

Add a JSON file under `Editor/Data/HardwareSupport/Profiles/`.
The `profile` value must exactly match the enum name in `FaceTrackingHardwareProfile`.

```json
{
  "profile": "NewHardware",
  "displayName": "New Hardware",
  "displayOrder": 18,
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
| `profile` | Required | The enum name in `FaceTrackingHardwareProfile`. |
| `displayName` | Required | The hardware name shown in the Inspector. |
| `displayOrder` | Required | Sort order in the Inspector. Use a value that does not conflict with existing profiles. |
| `sources` | Required | Source material for the support status. Add at least one source. |
| `full` | Optional | `UnifiedExpression` values that the hardware can output directly. |
| `converted` | Optional | `UnifiedExpression` values supported through conversion, emulation, merged left/right values, or module-side processing. |
| `unknown` | Optional | `UnifiedExpression` values that cannot be confirmed from public information. |

Expressions that are not listed in `full`, `converted`, or `unknown` are treated as unsupported.

## 3. Check expression names

Only values from `UnifiedExpression` can be used in the JSON file.
Invalid names or `None` cause a load error.

If the device documentation uses its own expression names, map them to VRCFaceTracking Unified Expressions before adding them.
You do not need to list the same expression more than once in a single hardware profile.

## 4. Update tests

After adding hardware, update the expected list in `HardwareSupportDataTests.Profiles_LoadsJsonInDisplayOrder`.
Place the new profile according to `displayOrder`.

Then run Unity EditMode tests and confirm that:

- The JSON profile loads correctly.
- `profile` matches the enum value.
- `displayOrder` values do not conflict.
- All `UnifiedExpression` names in JSON exist.

## Support status criteria

| Status | Criteria |
| --- | --- |
| `full` | The hardware or VRCFT module outputs the expression value directly. |
| `converted` | The value is produced through estimation, shared left/right values, synthesized values, or module-side conversion. |
| `unknown` | The available information is not enough to determine support. |
| Unsupported | The expression is not supported, or support cannot be confirmed. |

When unsure, use `unknown` and explain the reason in `sources.note`.
