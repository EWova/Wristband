# EWova Wristband API Specification

## Context
This API is consumed by a Unity XR wristband SDK (`com.ewova.wristband`).
The client authenticates via Bearer token. All requests carry `Authorization` and `X-Unity-Sdk` headers automatically.

- Dev base URL: `https://wristbands.ewova.dev/api/v1`
- Prod base URL: `https://wristbands.ewova.com/api/v1`

---

## Endpoints

### 1. GET /me/wristband/features
Returns which wristband buttons are visible/enabled for the current user, based on their role.
Client calls this once on startup. On failure (any non-2xx), client hides all buttons.

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
CAPTURE_TO_EWOVA      captures and uploads a screenshot
SHARE_TO_EWOVA        shares an activity post
VIEW_LEARNING_PROFILE opens learning profile (only active when client has com.ewova.learningportfoliosdk)
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
