# EWova.Wristband

VR 腕帶 UI 套件，提供 XR 場景中的腕部選單，讓使用者可快速執行 EWova 平台相關操作（跳轉 App、截圖、分享、查看學習歷程等）。

---

## 安裝

在 Unity Package Manager 加入 git URL：

```
https://github.com/EWova/Wristband.git?path=Assets/EWova.Wristband
```

**相依套件（需一併安裝）：**

| 套件 | git URL |
|---|---|
| `com.ewova.core` | `https://github.com/EWova/UnityPackageCore.git?path=Assets/EWova.Core#Dev` |
| `com.cysharp.unitask` | `https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask` |

---

## 目錄結構

```
Assets/EWova.Wristband/
├── package.json                        # 套件版本（格式：YYYY.MM.Build）
├── Editor/
│   ├── EWova.Wristband.Editor.asmdef
│   ├── TSVImporter.cs                  # 讓 .tsv 可被 AssetDatabase 匯入為 TextAsset
│   ├── PkgVerGen.cs                    # 從 package.json 自動產生 PackageInfo.cs
│   └── PackageJsonPostprocessor.cs     # 偵測 package.json 變動，觸發 PkgVerGen
└── Runtimes/
    ├── Animations/                     # 選單開關動畫（Animator.controller）
    ├── Font/                           # NotoSansTC（中文字型）
    ├── Models/                         # Wristband.fbx（腕帶 3D 模型）
    ├── Prefabs/
    │   ├── EWova Wristband.prefab      # 完整腕帶 Prefab（主要使用這個）
    │   ├── WristbandPivot.prefab       # 手腕吸附錨點
    │   └── Base/Element.prefab         # 單顆圓形按鈕基礎 Prefab
    ├── Resources/
    │   └── Localization/Wristband.tsv  # 多語系字串表（Key + 6 語言）
    ├── Sprites/                        # 按鈕圖示、背景
    └── Scripts/
        ├── EWova.Wristband.asmdef
        ├── Wristband.cs                # 主元件
        ├── WristbandUI.cs              # 時鐘顯示
        ├── Setup.cs                    # Feature Flag 初始化
        ├── CircleButtonElement.cs      # 圓形按鈕 UI 元件
        ├── CircleGroupLayout.cs        # 圓形排列 LayoutGroup
        ├── Follower.cs                 # 跟隨手腕錨點
        ├── UIButtonScaleEffect.cs      # Hover/Press 縮放效果
        ├── Logger.cs                   # 套件內部 Logger
        ├── PackageInfo.cs              # ⚠ 自動產生，勿手動修改
        ├── DefaultTextProvider.cs      # TSV 多語系解析器
        ├── ApiClient/
        │   ├── WApiClient.cs           # API Client（宣告 + URL）
        │   ├── WApiClient.Method.cs    # API 方法（截圖上傳、分享）
        │   └── ApiModels.cs            # DTO
        ├── Elements/
        │   ├── BaseBTN.cs              # 按鈕抽象基底
        │   ├── GoEWovaBTN.cs           # 跳轉 EWova App
        │   ├── ScreenshotBTN.cs        # 截圖
        │   ├── ShareBTN.cs             # 分享到 EWova
        │   ├── ViewLearningProfileBTN.cs # 查看學習歷程
        │   ├── GoEWovaWebsiteBTN.cs    # 開啟 EWova 官網
        │   └── QuitAppBTN.cs           # 離開 App
        └── Localization/
            ├── ITextProvider.cs
            ├── ILocalizeUpdater.cs
            ├── Localizer.cs
            ├── LocalizeStringEvent.cs
            └── TextProvider.cs
```

---

## 核心元件說明

### `Wristband.cs`

選單的主控元件。

- **Feature Flag**：透過 `LoadFlag(string flags)` 傳入逗號分隔的旗標字串，元件依此顯示或隱藏對應按鈕群組（`FeatureGroup[]`）。
- **開關動畫**：手動控制 Animator 播放進度（`EaseInExpo` / `EaseOutExpo`），並在閒置 5 秒後自動關閉選單。
- **Hover 抑制**：實作 `IPointerEnterHandler` / `IPointerExitHandler`，滑鼠懸停時暫停閒置計時。
- **語言切換**：`Update()` 偵測 `LocalizationLang` 欄位變動，即時重新套用多語系。

### `Setup.cs`

搭配 `Wristband` 使用的輔助元件（`RequireComponent`）。在 `Start()` 讀取 Inspector 上設定的 `Flag` 字串，呼叫 `Wristband.LoadFlag()`。

預設旗標：
```
GO_TO_EWOVA, CAPTURE_TO_EWOVA, SHARE_TO_EWOVA,
VIEW_LEARNING_PROFILE, EXPLORE_EWOVA_WEBSITE, QUIT_APP
```

### `CircleButtonElement.cs`

單顆圓形按鈕的 UI 元件，包含：

- `Progress`（0–1）：以環狀填充圖顯示載入進度。
- `IsDone`：`true` 後按鈕才可點擊。
- `OnClick`：點擊事件（Action）。

### `Follower.cs`

在 `LateUpdate` 以 `SmoothDamp` 跟隨 `Pivot`（手腕錨點），並可選擇性地朝向 `LookAt` 目標旋轉。

---

## 按鈕系統

所有按鈕繼承 `BaseBTN`，需實作兩個非同步方法：

```csharp
protected abstract UniTask Load(LoadProcess loadProcess);
protected abstract UniTask ProcessClick();
```

`Load()` 在 `Start()` 時呼叫，完成後呼叫 `loadProcess.SetComplete()`（失敗時呼叫 `loadProcess.SetFailed()`）；`ProcessClick()` 在使用者點擊時執行。

**新增按鈕步驟：**

1. 在 `Elements/` 建立繼承 `BaseBTN` 的新類別。
2. 覆寫 `LabelKey`、`DescriptionKey`，並實作 `Load` / `ProcessClick`。
3. 在 `Wristband` Prefab 的 `FeatureGroup[]` 新增對應的旗標與元件參照。
4. 在 `Wristband.tsv` 補充多語系字串。

---

## API Client

`WApiClient` 繼承 `com.ewova.core` 的 `AuthApiClient`，自動附帶 EWova 身份驗證。

**環境切換：** `EWova → Editor → DeploymentMode`

| 環境 | URL |
|---|---|
| Development | `https://wristbands.ewova.dev/api/v1` |
| Production | `https://wristbands.ewova.com/api/v1` |

**現有 API 方法：**

```csharp
// 截圖上傳
UniTask<ApiModels.UploadScreenshotResponse> UploadScreenshotAsync(UploadScreenshotRequest, ct)

// 活動分享
UniTask ShareActivityAsync(ShareActivityRequest, ct)
```

API 錯誤統一包裝為 `ApiWristbandException`，帶有 `WApiAction` 列舉識別動作類型。

---

## 多語系

字串表位於 `Runtimes/Resources/Localization/Wristband.tsv`（Tab 分隔）：

```
Key     en      zh-Hant     zh      ja      ko      vi
GoToEWova  Go to EWova  穿梭到 EWova  ...
```

**支援語言：** `en`、`zh-Hant`、`zh`、`ja`、`ko`、`vi`

語言解析邏輯（`DefaultTextProvider`）：

1. `LocalizationLang.auto` → 依 `CultureInfo.CurrentUICulture` 自動偵測系統語言。
2. 找不到指定語言的 key → 退回 `en`。
3. `en` 也找不到 → 回傳 `[Key]`。

在 Unity 場景中，將 `LocalizeStringEvent` 掛在 UI 物件上，設定 `Key`，並將 `OnUpdate` 事件綁定到 `TextMeshProUGUI.SetText`，即可接收語系更新。

---

## Editor 工具

### `PkgVerGen`

修改 `package.json` 後，`PackageJsonPostprocessor` 自動觸發此工具，將 `name` 與 `version` 寫入 `PackageInfo.cs`。**請勿手動編輯 `PackageInfo.cs`**。

版本格式：`YYYY.MM.Build`（例：`2026.6.1`）

### `TSVImporter`

自訂 `ScriptedImporter`，讓 `.tsv` 檔在 AssetDatabase 中被視為 `TextAsset`，可被 `Resources.Load` 載入。

---

## Assembly 說明

| Assembly | 用途 |
|---|---|
| `EWova.Wristband` | Runtime，namespace `EWova.Wristband` 與 `EWova.Localization` |
| `EWova.Wristband.Editor` | Editor 工具（`PkgVerGen`、`TSVImporter`） |

`InternalsVisibleTo("EWova.LearningPortfolio.Editor")` — 內部成員對 LearningPortfolio 編輯器可見。
