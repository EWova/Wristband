# Localization 速查

## 介面

```csharp
namespace EWova.Localization
{
    public interface ITextProvider
    {
        string GetLocalizedString(string key);
    }
}
```

任何實作這個介面的類別都能提供字串。`EWova.Localization.TextProvider`（靜態類別）有一個 `Providers` 清單可以掛多個 `ITextProvider`，
但**目前 Wristband 實際的查詢路徑（`Localizer.DoLocalizeUpdate` / `BaseBTN.GetLocalizedString`）都是直接呼叫 `Wristband.LocalizeTextProvider`
這個 `DefaultTextProvider` 實例，不會經過 `TextProvider.Providers`**，所以掛進那個清單目前不會影響 Wristband 顯示的文字。
真正對第三方有用的擴充點是下面的 `DefaultTextProvider.MergeTsv` / `SetLanguageByCode`。

## 現有實作：DefaultTextProvider

檔案：`Assets/EWova.Wristband/Runtimes/Scripts/DefaultTextProvider.cs`

- 語言列表：`LocalizationLang` enum（`auto, en, zh_Hant, zh, ja, ko, vi`），這是內建、掛在 Inspector 上的清單。
- 但內部查表已改成用**字串語系代碼**（`en`、`zh-Hant`、`th`...）當 key，enum 只是內建語言的一個方便的殼，
  所以查表不再限制只能用 enum 裡列出的語言。
- 資料來源：`Resources/Localization/Wristband.tsv`，欄位為 `Key | en | zh-Hant | zh | ja | ko | vi`。
- `LoadFromFile("Localization/Wristband")` 讀檔並解析成 `Dictionary<string, Dictionary<string,string>>`（語系代碼 → key → 翻譯）。
- 查詢流程：`GetLocalizedString(key)` → 用目前語系代碼（`CurrentLanguageCode`）查表 → 查無則 fallback 到 `en` → 都沒有回傳 `[key]`。
- `CurrentSetting = LocalizationLang.auto` 時會呼叫 `GetSystemLanguage()`，依 `CultureInfo.CurrentUICulture` 猜語言。
- 語系代碼真的變了才會觸發 `OnLanguageChanged` 事件（`Localizer.DoLocalizeUpdate` 及各 `BaseBTN.OnLanguageChanged` 靠這個刷新文字）。

## 新增一種語言 — 第三方不侵入 package 的作法（推薦）

不需要改 package 任何原始碼、不需要在 `LocalizationLang` enum 加值。作法是自備一份補充 TSV，
merge 進已載入的 `DefaultTextProvider`，再用字串代碼切換過去：

```csharp
// myApp 自己的 TSV，欄位例如: Key    th
string extraTsv = myThaiTsvTextAsset.text;

wristband.LocalizeTextProvider.MergeTsv(extraTsv); // 合併，不會清掉既有翻譯
wristband.SetLanguageByCode("th");                 // 切換語言 + 觸發畫面刷新
```

- `DefaultTextProvider.MergeTsv(tsvContent)`：把任何 header（不限於內建 enum 認得的欄位）解析進現有的翻譯表，
  同 key/語系會被覆蓋、其餘保留。可以呼叫多次疊加多份語言包。
- `Wristband.SetLanguageByCode(code)` / `DefaultTextProvider.SetLanguageByCode(code)`：切到任意字串語系代碼，
  等同於原本設定 `LocalizationLang` enum 的效果（會觸發 `OnLanguageChanged` 並刷新 UI）。
- 之後只要 UI 元件的 Key 有對應到你補的翻譯，`BaseBTN`／`ILocalizeUpdater` 都會自動顯示新語言的文字。

## 新增一種語言 — 需要改 package 原始碼的作法（只有維護者會用到）

如果是要把某語言正式收進這個 package 本身（讓它出現在 `LocalizationLang` 的 Inspector 下拉選單、有 `auto` 系統語言自動偵測）：

1. **TSV 加欄位**：`Wristband.tsv` 每一列補上該語系欄，值用 `\n` 表示換行（讀檔時會轉回真正換行）。
2. **`LocalizationLang` enum 加一個值**，例如：
   ```csharp
   public enum LocalizationLang
   {
       auto, en, zh_Hant, zh, ja, ko, vi, th
   }
   ```
3. **`GetCode` 加對應的 TSV 欄位代碼**（要跟 TSV header 文字一致，比對是 `OrdinalIgnoreCase`）：
   ```csharp
   private static string GetCode(LocalizationLang lang) => lang switch
   {
       ...
       LocalizationLang.th => "th",
       _ => null
   };
   ```
4. **（可選）`GetSystemLanguage` 加自動偵測規則**：
   ```csharp
   var output = currentCulture.TwoLetterISOLanguageName switch
   {
       ...
       "th" => LocalizationLang.th,
       _ => LocalizationLang.en
   };
   ```
5. 其餘（`LoadFromTsv` 欄位掃描、fallback 邏輯）已改成掃描任意欄位，不用改。

## 完全自訂的語言來源（不用 TSV / DefaultTextProvider）

如果連 TSV 格式都不想用（例如翻譯來自遠端 API），可以自己組出一份 `Dictionary<string,string>` 再轉成 TSV 字串丟給 `MergeTsv`，
或是更直接：把 `Wristband.LocalizeTextProvider` 換成你自己實作的 `ITextProvider`（例如加一個 `Wristband.SetTextProvider(...)`，
目前 package 沒有現成的 setter，需要修改 `Wristband.cs` 才能整個替換掉）。單純**新增/覆蓋語言**用上面的 `MergeTsv` 就夠了，不需要走到這一步。

## 執行期切換語言

- 掛在 `Wristband` prefab 上的欄位：`Wristband.LocalizationLang`（enum），改這個值即會在下一個 `Update()` 觸發：
  `LocalizeTextProvider.CurrentSetting = LocalizationLang;` → `Localizer.DoLocalizeUpdate(...)`。
- 想切到 enum 沒有的自訂語系：呼叫 `Wristband.SetLanguageByCode("th")`（立即生效，不用等 `Update()`）。
- `Localizer.DoLocalizeUpdate` 會找所有子物件上的 `ILocalizeUpdater`（例如 `LocalizeStringEvent`），
  把新字串塞進去；`BaseBTN` 另外自己訂閱 `OnLanguageChanged` 更新按鈕 Label。
