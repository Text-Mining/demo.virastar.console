using System;
using System.Collections.Generic;
using System.Linq;

namespace textmining.demo.virastar.console
{
    /// <summary>
    /// این کلاس برای نگهداری اطلاعات اضافی برای رفتار کاربر در قبال هر توکن خروجی ویراستار است
    /// </summary>
    public class TokenInfoEdit
    {
        public TokenInfoEdit(TokenInfo tokenInfo)
        {
            Token = tokenInfo;
            ApplyChangeIndex = -1;
            RemoveByOtherTokens = false;
        }

        /// <summary>
        /// توکن فعلی
        /// </summary>
        public TokenInfo Token { get; }


        /// <summary>
        /// اندیس (انتخاب شده توسط کاربر) در لیست اصلاحات پیشنهادی ویراستار 
        /// </summary>
        /// <remarks>
        /// تصمیم‌گیری انجام شده توسط کاربر برای اعمال یا نادیده گرفتن تغییرات پیشنهادی
        /// </remarks>
        /// <value><c>-1</c> if [ignore to applying changes]; otherwise, <c>index of selected change recommendation (int)</c>.</value>
        public int ApplyChangeIndex { get; set; }

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
        
        /// <summary>
        /// اعمال تغییرات مورد نظر کاربر بر روی همه لیست توکن‌های خروجی
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public static List<TokenInfoEdit> ApplyTokensListChangesOnEachOther(List<TokenInfoEdit> tokens)
        {
            for (int j = 0; j < tokens.Count; j++)
                tokens = ApplyTokensListChangesOnEachOther(tokens, j);
            return tokens;
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
            catch //(Exception exp)
            {
                //log.FatalFormat(exp.Message);
            }

            return tokens;
        }
        
        /// <summary>
        /// بازگشت به عقب در تصمیمات کاربر برای اصلاح یک توکن
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="tokenIndex"></param>
        /// <returns></returns>
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