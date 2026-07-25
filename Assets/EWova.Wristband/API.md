# EWova Wristband API Specification

## Changelog since last handoff

No endpoint, request, or response schema changed — `/me/wristband/features`, `/me/screenshots`, `/me/shares` and all field names/types are identical to what you already have. Two behavioral things changed client-side that affect how you should reason about the `features` response:

1. **`CAPTURE_TO_EWOVA`, `SHARE_TO_EWOVA`, `VIEW_LEARNING_PROFILE` now require the Learning Portfolio SDK.** The client will hide these three keys regardless of what you send if the host app doesn't have `com.ewova.learningportfoliosdk` installed at `>= 2026.6.0`, and will further hide/disable them live if the user is logged out of Learning Portfolio — independent of your `visible`/`enabled` values. Treat your flags for these three as "does this account have entitlement," not "is this currently showable" — the client owns the live on/off toggle now. `GO_TO_EWOVA`, `EXPLORE_EWOVA_WEBSITE`, `QUIT_APP` have no such dependency and are unaffected.
2. **Fallback-on-failure corrected.** This doc previously said "on failure, client hides all buttons" — that was never accurate to the shipped behavior and is now fixed below: on a failed fetch the client shows `GO_TO_EWOVA`, `EXPLORE_EWOVA_WEBSITE`, `QUIT_APP` (visible + enabled), not an empty menu. No backend action needed, just don't rely on the old wording if you read it from an earlier copy of this file.

Nothing here requires a backend code change — it's informational so your flag semantics for the three LP-gated keys match what the client actually does.

## Context
This API is consumed by a Unity XR wristband SDK (`com.ewova.wristband`).
The client authenticates via Bearer token. All requests carry `Authorization` and `X-Unity-Sdk` headers automatically.

- Dev base URL: `https://wristbands.ewova.dev/api/v1`
- Prod base URL: `https://wristbands.ewova.com/api/v1`

---

## Endpoints

### 1. GET /me/wristband/features
Returns which wristband buttons are visible/enabled for the current user, based on their role.
Client calls this once on startup. On failure (any non-2xx), client falls back to a fixed default: `GO_TO_EWOVA`, `EXPLORE_EWOVA_WEBSITE`, `QUIT_APP` visible+enabled, everything else hidden.

**Request:** no body

**Response 200**
```json
{
  "features": [
    {
      "key": "GO_TO_EWOVA",
      "visible": true,
      "enabled": true,
      "disabledReason": null
    },
    {
      "key": "CAPTURE_TO_EWOVA",
      "visible": true,
      "enabled": true,
      "disabledReason": null
    },
    {
      "key": "SHARE_TO_EWOVA",
      "visible": true,
      "enabled": true,
      "disabledReason": null
    },
    {
      "key": "VIEW_LEARNING_PROFILE",
      "visible": true,
      "enabled": false,
      "disabledReason": "FEATURE_NOT_AVAILABLE"
    },
    {
      "key": "EXPLORE_EWOVA_WEBSITE",
      "visible": true,
      "enabled": true,
      "disabledReason": null
    },
    {
      "key": "QUIT_APP",
      "visible": true,
      "enabled": true,
      "disabledReason": null
    }
  ]
}
```

**FeatureState schema**
```
key             string   required  — one of the feature keys listed below
visible         bool     required  — false = button hidden entirely
enabled         bool     required  — false = button shown but grayed out, not clickable
disabledReason  string   optional  — localization key shown to user when enabled=false
```

**Valid `key` values**
```
GO_TO_EWOVA           deep-links to EWova app
CAPTURE_TO_EWOVA      captures and uploads a screenshot          — requires com.ewova.learningportfoliosdk >= 2026.6.0 + user logged into Learning Portfolio (client-enforced, see Changelog)
SHARE_TO_EWOVA        shares an activity post                    — same Learning Portfolio requirement as above
VIEW_LEARNING_PROFILE opens learning profile                     — same Learning Portfolio requirement as above
EXPLORE_EWOVA_WEBSITE opens EWova website
QUIT_APP              quits the current XR app
```

**Valid `disabledReason` values**
```
FEATURE_NOT_AVAILABLE
FEATURE_PERMISSION_DENIED
```

---

### 2. POST /me/screenshots
Receives a screenshot from the XR device, stores it, and returns a CDN URL.
`imageData` is a Base64 string — the client sends `byte[]` which Newtonsoft.Json auto-encodes to Base64.

**Request**
```json
{
  "imageData": "<base64-encoded JPEG string>"
}
```
```
imageData  string (Base64)  required  — JPEG image, quality 85, encoded as Base64
```

**Response 200**
```json
{
  "imageUrl": "https://cdn.ewova.com/screenshots/abc123.jpg",
  "imageId": "img_abc123"
}
```
```
imageUrl  string  — publicly accessible CDN URL; passed to POST /me/shares
imageId   string  — database identifier for future reference (delete, re-share, etc.)
```

---

### 3. POST /me/shares
Creates a share post using a previously uploaded screenshot.

**Request**
```json
{
  "imageUrl": "https://cdn.ewova.com/screenshots/abc123.jpg",
  "description": ""
}
```
```
imageUrl     string  required  — value from POST /me/screenshots → imageUrl
description  string  optional  — may be empty string
```

**Response 200:** empty body
