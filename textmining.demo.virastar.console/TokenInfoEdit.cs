using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace textmining.demo.virastar.console
{
    public enum ActionType { None, IgnoreManual, IgnoreAll, ApplyManual, ApplyAuto, ApplyAll, AddToDic, AddNewSuggestion };

    public class TokenInfoEdit
    {
        public static readonly Dictionary<string, List<string>> LocalSuggestionList =
            new Dictionary<string, List<string>>(File.Exists(VirastarConfig.UserSuggestionPath)
                ? File.ReadAllLines(VirastarConfig.UserSuggestionPath)
                    .Select(line => line.Split('\t'))
                    .Where(item => item.Length == 2 && !string.IsNullOrEmpty(item[0]) && !string.IsNullOrEmpty(item[1]))
                    .GroupBy(w => w[0])
                    .Select(grp=>new{grp.Key, Val=grp.Select(ww=>ww[1]).ToList()})
                    .ToDictionary(grp => grp.Key, grp => grp.Val)
                : new Dictionary<string, List<string>>());

        public void AddToSuggestionList(string key, string suggestion)
        {
            if (!Token.EditList.Any(ed => ed.SuggestedText.Equals(suggestion)))
            {
                var ed = new EditItem(EditType.WordSuggestion, suggestion, "پیشنهاد کاربر");

                if (!LocalSuggestionList.ContainsKey(key))
                {
                    LocalSuggestionList.Add(key, new List<string> { suggestion });
                    Token.EditList.Add(ed);
                    ApplyChangeIndex = Token.EditList.Count - 1;
                    File.AppendAllText(VirastarConfig.UserSuggestionPath, $"{key}\t{suggestion}{Environment.NewLine}");
                }
                else if(!LocalSuggestionList[key].Contains(suggestion))
                {
                    LocalSuggestionList[key].Add(suggestion);
                    Token.EditList.Add(ed);
                    ApplyChangeIndex = Token.EditList.Count - 1;
                    File.AppendAllText(VirastarConfig.UserSuggestionPath, $"{key}\t{suggestion}{Environment.NewLine}");
                }
            }
        }

        public TokenInfoEdit(TokenInfo tokenInfo)
        {
            Token = tokenInfo;
            if (LocalSuggestionList.ContainsKey(tokenInfo.RefineText))
            {
                for (int i = 0; i < LocalSuggestionList[tokenInfo.RefineText].Count; i++)
                    Token.EditList.Insert(i, new EditItem(
                        EditType.WordSuggestion, LocalSuggestionList[tokenInfo.RefineText][i], "پیشنهاد کاربر"));
            }
            ApplyChangeIndex = -1;
            RemoveByOtherTokens = false;
        }

        public TokenInfo Token { get; }


        /// <summary>
        /// تصمیم‌گیری انجام شده توسط کاربر برای اعمال یا نادیده گرفتن تغییرات پیشنهادی
        /// </summary>
        /// <value><c>-1</c> if [ignore to applying changes]; otherwise, <c>index of selected change recommendation (int)</c>.</value>
        //[JsonIgnore]
        public int ApplyChangeIndex { get; set; }

        /// <summary>
        /// نوع عکس العمل و انتخاب کاربر در برابر پیشنهادات اصلاح ارائه شده
        /// </summary>
        /// <value>The user interaction.</value>
        public ActionType UserReaction { get; set; } = ActionType.None;

        /// <summary>
        /// نشان میدهد که اعمال تغییرات توکن دیگر (قبل یا بعد) باعث حذف این توکن می‌شود
        /// </summary>
        /// <value><c>true</c> if [remove by applying changes]; otherwise, <c>false</c>.</value>
        public bool RemoveByOtherTokens { get; set; }
        
        /// <summary>
        /// تغییر پیشنهادی انتخاب شده توسط کاربر برای اعمال روی متن اصلی
        /// </summary>
        /// <value>The get applied change string.</value>
        public string GetAppliedChange => RemoveByOtherTokens
            ? string.Empty
            : (Token.EditList == null || Token.EditList.Count == 0 || ApplyChangeIndex < 0
                ? Token.OriginalText
                : Token.EditList[Math.Min(ApplyChangeIndex, Token.EditList.Count - 1)].SuggestedText);

        public override string ToString() => Token.ToString();

        public string TokenJsonStr() => JsonConvert.SerializeObject(Token);

        public bool IsEqualRecommendation(TokenInfoEdit otherToken)
        {
            if (otherToken != null && 
                Token.OriginalText.Equals(otherToken.Token.OriginalText) &&
                Token.EditList.Count == otherToken.Token.EditList.Count)
            {
                for (int i = 0; i < Token.EditList.Count; i++)
                    if (!Token.EditList[i].SuggestedText.Equals(otherToken.Token.EditList[i].SuggestedText))
                        return false;

                return true;
            }

            return false;
        }

        public static void AddOriginalTokenToLexicon(string exceptionWord /*, VirastarConfig vsConfig*/)
        {
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(VirastarConfig.UserLexiconPath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(VirastarConfig.UserLexiconPath));

                File.AppendAllText(VirastarConfig.UserLexiconPath, exceptionWord.Trim() + Environment.NewLine);
            }
            catch// (Exception ex)
            {
                // ignored
            }
        }

        public static List<TokenInfoEdit> ApplyTokensListChangesOnEachOther(List<TokenInfoEdit> tokens)
        {
            for (int j = 0; j < tokens.Count; j++)
                tokens = ApplyTokensListChangesOnEachOther(tokens, j);
            return tokens;
        }

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
            catch //(Exception exp)
            {
                //log.FatalFormat(exp.Message);
            }

            return tokens;
        }
        
        public static List<TokenInfoEdit> RollBackEffectOfTokensChangesOnEachOther(List<TokenInfoEdit> tokens, int tokenIndex)
        {
            var token = tokens[tokenIndex];
            if (token != null &&
                token.ApplyChangeIndex >= 0 &&
                token.Token.EditList != null &&
                token.Token.EditList.Count > 0)
            {
                // اگر اعمال این تگ باعث حذف توکن های مجاور شده، باید تاثیر آن برگردد
                int removeCnt = token.Token
                    .EditList[Math.Min(token.ApplyChangeIndex, token.Token.EditList.Count - 1)]
                    .RemoveTokensCount;
                if (removeCnt != 0 && tokenIndex + removeCnt < tokens.Count() && tokenIndex + removeCnt >= 0)
                {
                    for (int j = 1; j <= Math.Abs(removeCnt); j++)
                    {
                        int removeIndex = tokenIndex + Math.Sign(removeCnt) * j;
                        tokens[removeIndex].RemoveByOtherTokens = false;
                    }
                }

                token.ApplyChangeIndex = -1;
            }

            return tokens;
        }
    }
}