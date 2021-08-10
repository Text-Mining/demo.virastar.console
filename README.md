<div dir="rtl">

# نحوه استفاده از سرویس ویراستار فارسی‌یار

دموی نحوه استفاده از سرویس ویراستار فارسی‌یار و تحلیل خروجی سرویس

## توضیح درباره ورودی و خروجی ویراستار

**ورودی** سرویس ویراستار متن و تنظیمات ویراستاری (برای فعالسازی ماژول‌های مختلف ویراستاری و استانداردسازی متن در کلاس [VirastarConfig](../master/textmining.demo.virastar.console/VirastarConfig.cs)) است.

**خروجی** نیز لیستی از نمونه‌های کلاس TokenInfo است که اطلاعات توکن و بهمراه لیست پیشنهادات اصلاحی ویراستار (فیلد EditList) است.

بهترین (محتمل‌ترین) پیشنهاد در آخر لیست EditList است و به ترتیب به سمت ابتدای لیست گزینه‌های پیشنهادی ضعیفتر قرار می‌گیرند.

هر عنصر از لیست اصلاحات پیشنهادی (نمونه از کلاس EditItem است) که دارای 4 فیلد ذیل است:

</div>

```C#
/// <summary>
/// این کلاس برای نگهداری یک مورد اصلاح پیشنهاد شده توسط ویراستار فارسی‌یار استفاده می‌شود
/// </summary>
public class EditItem
{
	/// <summary>
	/// نوع اصلاح
	/// </summary>
	public EditType EditType { get; set; }

	/// <summary>
	/// عبارت پیشنهادی برای جایگزین شدن
	/// </summary>
	public string SuggestedText { get; set; }

	/// <summary>
	/// توضیح درباره تغییر پیشنهادی
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	/// حذف چند عدد از توکن‌های قبل یا بعد
	/// </summary>
	public int RemoveTokensCount { get; set; }
}
```

<div dir="rtl">

**فیلد RemoveTokensCount** تعداد توکن بعدی (در صورتی که عدد مثبت باشد) یا قبلی (در صورتی که عدد منفی باشد) که در صورت انتخاب این اصلاح باید حذف شوند را نشان می‌دهد.

چون ممکن است انجام اصلاح پیشنهاد شده برای یک توکن منجر به حذف چند توکن بعدی شود (مانند ادغام عبارات مرکب) مثلاً: «می شود» (که دارای 3 توکن «می»، « » و «شود» است) تبدیل به «می‌شود» (که دارای یک توکن «می‌شود» است) می‌شود.

**فیلد EditType** (نوع اصلاح پیشنهاد شده توسط ویراستار) می‌تواند یکی از 4 مقدار ذیل باشد:

</div>

```C#
/// <summary>
/// نوع اصلاح پیشنهادی ویراستار
/// </summary>
public enum EditType
{
	/// <summary>
	/// اصلاح فاصله و نیم‌فاصله‌ها
	/// </summary>
	SpaceCorrection,
	/// <summary>
	/// اصلاح نویسه‌ها (جایگزین کردن کاراکترهای فارسی استاندارد)ا
	/// </summary>
	CharReplace,
	/// <summary>
	/// اصلاح غلط‌های املائی
	/// </summary>
	SpellCorrection,
	/// <summary>
	/// پیشنهاد شکل بهتر واژه
	/// </summary>
	WordSuggestion
}
```

<div dir="rtl">

## توضیح بخش‌های مختلف پروژه

توضیح فایل‌های اصلی پروژه:
- **[فایل program.cs](../master/textmining.demo.virastar.console/Program.cs)** : بخش اصلی کد برای فراخوانی سرویس و نمایش خروجی (تبدیل خروجی ویراستار به متن) است.
- **[فایل VirastarConfig.cs](../master/textmining.demo.virastar.console/VirastarConfig.cs)** : پارامتر ورودی (تنظیمات ویراستار) سرویس است.
- **[فایل TokenInfo.cs](../master/textmining.demo.virastar.console/TokenInfo.cs)** : خروجی سرویس لیستی از نمونه‌های این نوع هستند.
- **[فایل TokenInfoEdit.cs](../master/textmining.demo.virastar.console/TokenInfoEdit.cs)** : برای عملیات و ذخیره ویژگی‌های اضافه، از جمله نگهداری پیشنهاد انتخاب شده توسط کاربر و تاثیر اعمال این پیشنهاد بر سایر کلمات (توکن‌ها) اطراف است.


> توضیحات بیشتر در بین سورس کد داده شده است.

---

## مرحله اول: فراخوانی سرویس ویراستاری فارسی‌یار

</div>

```C#
        public static string BaseAddress = "https://api.text-mining.ir/api/";
        
        ‌private static string GetJWTToken(string apiKey)
        {
            string jwtToken = string.Empty;
            HttpClient client = new HttpClient();
            var response = client.GetAsync($"{BaseAddress}Token/GetToken?apikey={apiKey}").Result;
            if (response.IsSuccessStatusCode)
            {
                string res = response.Content.ReadAsStringAsync().Result;
                jwtToken = (string)JObject.Parse(res)["token"];
            }
            return jwtToken;
        }


	using (HttpClient client = new HttpClient {Timeout = TimeSpan.FromSeconds(300)})
            {
                /**********************************************
                 ************ Step 1: get token ***************
                 **********************************************/
                string bearerToken = GetJWTToken(ApiKey);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

                /************************************************
                 *** Step 2: set your virastar configurations ***
                 ************************************************/
                var config = new VirastarConfig()
                {
                    SpellConfiguration =
                    {
                        LexicalSpellCheckerDistanceThreshold = 3.0,
                        ContextSpellCheckHighSensitive = false
                    },
                    WordConfiguration = { HighSensitiveRefinement = false }
                    // ...
                };

                /****************************************************
                 ** Step 3: convert virastar config to json string **
                 ****************************************************/
                string jsonStr = JsonConvert.SerializeObject(new
                {
                    Text = "حتما آن ها مومن را احترام مے ڪنند. یگ پنجره ی بزگ وسبز باز میشود . !حضور تان را کرامی می داشتم",
                    returnOnlyChangedTokens = false,
                    config.SpellConfiguration,
                    config.WordConfiguration,
                    config.CharConfiguration,
                    config.WritingRuleConfiguration,
                    config.IgnoreProcessConfiguration,
                });

                /**********************************************
                 ********* Step 4: call virastar api **********
                 **********************************************/
                var context = new StringContent(jsonStr, Encoding.UTF8, "application/json");
                var response = client.PostAsync(BaseAddress + "Virastar/ScanText", context).Result;
                Console.WriteLine($"Finished with status:{response.StatusCode}");
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine(response.ReasonPhrase);
                }
                else
                {
                    /**********************************************
                     *** Step 5: read api response and using it ***
                     **********************************************/
                    string resp = response.Content.ReadAsStringAsync().Result;

                      //  ...  ادامه کد برای تحلیل و استفاده از خروجی ویراستار

                } // else response.IsSuccessStatusCode
            } // using
```

<div dir="rtl">

## مرحله دوم: تحلیل و استفاده از خروجی ویراستار

**روش اول: تحلیل خروجی بوسیله پارس ابجکت جی.سان**

</div>

```C#
                    // Method 1: using json object
                    JArray jsonArrayObj = JArray.Parse(resp);
                    var result1 = new StringBuilder();
                    foreach (JObject tokenInfo in jsonArrayObj.Children<JObject>())
                    {
                        result1.Append($"{tokenInfo["OriginalText"]}");
                        if (tokenInfo["IsChanged"]?.Value<bool>() ?? false)
                        {
                            JArray edits = (JArray) tokenInfo["EditList"];
                            if (edits?.Count > 1)
                            {
                                result1.Append($"{{{edits[0]["SuggestedText"]}({edits[0]["Description"]})");
                                for (int i = 1; i < edits.Count; i++)
                                {
                                    result1.Append($"  -  {edits[i]["SuggestedText"]}({edits[i]["Description"]})");
                                }

                                result1.Append('}'); //.Remove(result.Length - 3, 3)
                            }
                            else if (edits?.Count == 1)
                                result1.Append($"{{{tokenInfo["NewText"]}({edits[0]["Description"]})}}");
                        }
                    }
                    Console.WriteLine(Environment.NewLine + result1);

```

<div dir="rtl">

**روش دوم: تحلیل شیء‌گرای خروجی**

</div>

```C#

                    // Method 2: deserialize json string to TokenInfo object
                    var tokens = JsonConvert.DeserializeObject<List<TokenInfo>>(resp);
                    var result2 = new StringBuilder();
                    if(tokens != null)
                        foreach (TokenInfo token in tokens)
                        {
                            result2.Append(token.OriginalText);
                            if (token.IsChanged)
                            {
                                if (token.EditList.Count > 1)
                                {
                                    result2.Append($"{{{token.EditList[0].SuggestedText}({token.EditList[0].Description})");
                                    for (int i = 1; i < token.EditList.Count; i++)
                                    {
                                        result2.Append(
                                            $"  -  {token.EditList[i].SuggestedText}({token.EditList[i].Description})");
                                    }

                                    result2.Append('}');
                                }
                                else if (token.EditList?.Count == 1)
                                    result2.Append($"{{{token.NewText}({token.EditList[0].Description})}}");
                            }
                        }
                    Console.WriteLine(Environment.NewLine + result2);

```

<div dir="rtl">

**روش سوم: اعمال محتمل‌ترین پیشنهاد (آخرین نمونه از لیست اصلاحات پیشنهادی)**

</div>

```C#

                    // Method 3: auto-apply first suggestion of virastar
                    //var tokens = JsonConvert.DeserializeObject<List<TokenInfo>>(resp);
                    if (tokens != null)
                    {
                        var tokenEdits = tokens.Select(t => new TokenInfoEdit(t)
                            {ApplyChangeIndex = t.IsChanged ? t.EditList.Count - 1 : -1}).ToList();

                        var result3 = new StringBuilder();
                        for (int j = 0; j < tokenEdits.Count; j++)
                        {
                            tokenEdits = ApplyTokensListChangesOnEachOther(tokenEdits, j);
                            result3.Append(tokenEdits[j].GetAppliedChange);
                        }

                        Console.WriteLine(Environment.NewLine + result3);
                    }



        /// <summary>
        /// اعمال تغییرات مورد نظر کاربر بر روی یک عنصر از لیست توکن‌های خروجی
        /// </summary>
        /// <remarks>
        /// ممکن است انجام اصلاح پیشنهاد شده برای یک توکن منجر به حذف چند توکن بعدی شود (مانند ادغام عبارات مرکب) مثلاً: می شود (3 توکن «می»، « » و «شود») => می‌شود (1 توکن «می‌شود»)  ا
        /// </remarks>
        /// <param name="tokens"></param>
        /// <param name="tokenIndex"></param>
        /// <returns></returns>
        public static List<TokenInfoEdit> ApplyTokensListChangesOnEachOther(List<TokenInfoEdit> tokens, int tokenIndex)
        {
            try
            {
                if (!tokens[tokenIndex].RemoveByOtherTokens && tokens[tokenIndex].ApplyChangeIndex >= 0 &&
                    tokens[tokenIndex].Token.EditList != null && tokens[tokenIndex].Token.EditList.Count > 0)
                {
                    int removeCnt = tokens[tokenIndex].Token.EditList[tokens[tokenIndex].ApplyChangeIndex].RemoveTokensCount;
                    if (removeCnt != 0 && tokenIndex + removeCnt < tokens.Count && tokenIndex + removeCnt >= 0)
                    {
                        for (int j = 1; j <= Math.Abs(removeCnt); j++)
                        {
                            int removeIndex = tokenIndex + Math.Sign(removeCnt) * j;
                            tokens[removeIndex].RemoveByOtherTokens = true;
                            //tokens.RemoveAt(removeIndex);
                        }
                    }
                }
            }
            catch //(Exception ex)
            {
                //log.FatalFormat(ex.Message);
            }

            return tokens;
        }

```

<div dir="rtl">

---

> [سرویس تحلیل متن (متن کاوی) فارسی‌یار](https://text-mining.ir "مجموعه ابزارهای پردازش متن برای زبان فارسی")

</div>