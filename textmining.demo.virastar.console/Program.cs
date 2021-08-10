using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace textmining.demo.virastar.console
{
    class Program
    {
        public static string BaseAddress = "https://api.text-mining.ir/api/";  // "https://localhost:44385/api/";
        public static string ApiFilePath = Path.Combine(Directory.GetParent(Directory.GetParent(Path.GetDirectoryName(
            System.Reflection.Assembly.GetExecutingAssembly().Location)).FullName).FullName, "api_key.txt");
        public static string ApiKey =File.ReadAllText(ApiFilePath) ; // YOUR_API_KEY

        private static string GetJWTToken(string apiKey)
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

        static void Main()
        {
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
                } // else response.IsSuccessStatusCode
            } // using

            Console.WriteLine("Please, Press any key to exit.");
            Console.ReadKey();
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
    }
}