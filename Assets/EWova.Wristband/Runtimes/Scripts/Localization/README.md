# 新增語言（第三方注入用法）

不需要修改這個 package 任何檔案。整個流程只有兩步：準備一份補充 TSV，然後合併 + 切換。

## 1. 準備補充 TSV

跟 `Wristband.tsv` 一樣的 TSV 格式，第一欄是 `Key`，之後每欄是一個語系代碼（自己取，不受任何限制）。
只需要放你要新增/覆蓋的 key，不用整份翻譯都複製一遍。

```
Key	th
Confirm	ยืนยัน
Cancel	ยกเลิก
```

存成 `.txt` / `TextAsset`（Resources、StreamingAssets 或直接內嵌字串都行），值裡用 `\n` 代表換行。

## 2. 執行期注入 + 切換語言

```csharp
// wristband: 場景裡的 Wristband 元件參考

string extraTsv = myThaiTsvTextAsset.text;

wristband.LocalizeTextProvider.MergeTsv(extraTsv); // 合併進現有翻譯表，不會清掉原本的資料
wristband.SetLanguageByCode("th");                 // 切到 "th"，立即刷新畫面上所有文字
```

- 呼叫時機：任何 `Wristband` 已經 `Awake`（`LocalizeTextProvider` 已建立）之後都可以，例如你自己的啟動流程裡。
- `MergeTsv` 可以呼叫多次疊加多份語言包/多語言。
- 之後要切回內建語言，照舊呼叫 `wristband.LocalizeTextProvider.CurrentSetting = LocalizationLang.en;` 或改 `wristband.LocalizationLang` 欄位即可，兩套機制互不影響。
- 查無翻譯的 key 會自動 fallback 到 `en`，都沒有就顯示 `[Key]`，方便抓漏字。

就這樣，全程不用碰 `LocalizationLang` enum、不用改 package 原始碼。
