using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;

namespace textmining.demo.virastar.console
{
    public class VirastarConfig
    {
        public VirastarConfig()
        {
            CharConfiguration = new CharConfig();
            WordConfiguration = new WordConfig();
            WritingRuleConfiguration = new WritingRule();
            IgnoreProcessConfiguration = new IgnoreProcessConfig();
            SpellConfiguration = new SpellConfig();
        }

        public VirastarConfig(VirastarConfig cloneObj)
        {
            CharConfiguration = cloneObj.CharConfiguration.Clone();
            WordConfiguration = cloneObj.WordConfiguration.Clone();
            WritingRuleConfiguration = cloneObj.WritingRuleConfiguration.Clone();
            IgnoreProcessConfiguration = cloneObj.IgnoreProcessConfiguration.Clone();
            SpellConfiguration = cloneObj.SpellConfiguration.Clone();
        }

        public VirastarConfig(CharConfig charConfiguration, 
            WordConfig wordConfiguration,
            WritingRule writingRuleConfiguration, 
            IgnoreProcessConfig ignoreProcessConfiguration, 
            SpellConfig spellConfiguration)
        {
            CharConfiguration = charConfiguration ?? new CharConfig();
            WordConfiguration = wordConfiguration ?? new WordConfig();
            WritingRuleConfiguration = writingRuleConfiguration ?? new WritingRule();
            IgnoreProcessConfiguration = ignoreProcessConfiguration ?? new IgnoreProcessConfig();
            SpellConfiguration = spellConfiguration ?? new SpellConfig();
        }
        
        public void SaveToFile(string filePath)
        {
            string content = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filePath, content);
        }

        public static VirastarConfig LoadFromFile(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                try
                {
                    string content = File.ReadAllText(filePath);
                    return JsonConvert.DeserializeObject<VirastarConfig>(content);
                }
                catch 
                {
                    // ignored
                }
            }
            return new VirastarConfig();
        }

        /// <summary>
        /// تنظیمات مربوط به جایگزین نمودن واژه‌ها
        /// </summary>
        /// <value>The word configuration.</value>
        public WordConfig WordConfiguration { get; private set; }

        /// <summary>
        /// تنظیمات مربوط به اصلاح نویسه‌ها
        /// </summary>
        /// <value>The character configuration.</value>
        public CharConfig CharConfiguration { get; private set; }

        /// <summary>
        /// تنظیمات مربوط به آیین نگارش (دستور خط) فارسی
        /// </summary>
        /// <value>The writing rule configuration.</value>
        public WritingRule WritingRuleConfiguration { get; private set; }

        /// <summary>
        /// تنظیمات مربوط به نادید گرفتن بخش‌هایی از متن ورودی
        /// </summary>
        /// <value>The ignore process configuration.</value>
        public IgnoreProcessConfig IgnoreProcessConfiguration { get; private set; }

        /// <summary>
        /// تنظیمات مربوط به غلط‌یاب فارسی
        /// </summary>
        /// <value>The spell configuration.</value>
        public SpellConfig SpellConfiguration { get; private set; }


        /// <summary>
        /// تنظیمات مربوط به اصلاح نویسه‌ها
        /// </summary>
        public class CharConfig
        {
            public CharConfig()
            {
                IgnoreCharList = new HashSet<char>();
                LetterNormalization = true;
                DigitNormalization = true;
                PunctuationNormalization = true;
                SpaceNormalization = true;
                ErabNormalization = true;
                RemoveExtraSpace = false;
                RemoveExtraHalfSpace = true;
                ConvertHeHamzeToHeYe = true;
                ConvertHeYeToHeHamze = false;
            }

            /// <summary>
            /// لیست نویسه‌های استثنا که نادید گرفته می‌شوند
            /// </summary>
            /// <value>The ignore character list.</value>
            [DefaultValue(new char[0])]
            public HashSet<char> IgnoreCharList { get; set; }

            /// <summary> استانداردسازی حروف </summary>
            /// <remarks>
            /// اصلاح حروف الفبا (حروف الفبای عربی و سایر یونیکدهای غیراستاندارد) با معادل آنها در صفحه کلید استاندارد فارسی
            /// </remarks> 
            /// <value><c>true</c> if [letter normalization]; otherwise, <c>false</c>. (Default = true)</value>
            [DefaultValue(true)]
            public bool LetterNormalization { get; set; }

            /// <summary> تبدیل ارقام عربی به فارسی </summary>
            /// <remarks>
            /// تبدیل ارقام به شکل استاندارد فارسی مانند تبدیل ٤ به 4  یا  ٦ به 6
            /// </remarks> 
            /// <value><c>true</c> if [digit normalization]; otherwise, <c>false</c>. (Default = true)</value>
            [DefaultValue(true)]
            public bool DigitNormalization { get; set; }

            /// <summary> استانداردسازی یونیکدهای علائم </summary>
            /// <remarks>
            /// تبدیل علائم به شکل استاندارد مثلاً ﹦ به = تبدیل می‌شود
            /// </remarks> 
            /// <value>Default = true</value>
            [DefaultValue(true)]
            public bool PunctuationNormalization { get; set; }

            /// <summary> استانداردسازی یونیکدهای مختلف شبه‌فاصله (نیم‌فاصله) و فاصله </summary>
            /// <remarks>
            /// در برنامه‌های مختلف نویسه‌های فاصله و شبه‌فاصله مختلفی وجود دارند، این گزینه همه آنها را به معادل استانداردشان تبدیل می‌کند
            /// </remarks> 
            /// <value>Default = true</value>
            [DefaultValue(true)]
            public bool SpaceNormalization { get; set; }

            /// <summary> استانداردسازی یونیکدهای مختلف حرکت‌گذاری (اعراب، تنوین، ساکن، تشدید و ...) </summary>
            /// <remarks>
            /// در قلم‌های مختلف نویسه‌های حرکتی غیراستانداردی وجود دارد که با انتخاب این گزینه همه آنها به معادل استانداردشان تبدیل می‌شود
            /// </remarks> 
            /// <value>Default = true</value>
            [DefaultValue(true)]
            public bool ErabNormalization { get; set; }

            /// <summary> حذف فاصله‌های اضافی </summary>
            /// <remarks> حذف چند فاصله پشت‌سرهم </remarks> 
            /// <value>Default = false</value>
            [DefaultValue(true)]
            public bool RemoveExtraSpace { get; set; }

            /// <summary> حذف نیم‌فاصله‌های اضافی </summary>
            /// <remarks>
            /// حذف چند نیم‌فاصله پشت‌سرهم یا حذف نیم‌فاصله اضافی بعد از حروف غیر چسبان از قبیل: ا،د،ر،...
            /// </remarks> 
            /// <value>Default = true</value>
            [DefaultValue(true)]
            public bool RemoveExtraHalfSpace { get; set; }

            /// <summary> تبدیل ـۀ به ـه‌ی </summary>
            /// <value>Default = true</value>
            [DefaultValue(false)]
            public bool ConvertHeHamzeToHeYe { get; set; }

            /// <summary> تبدیل ـه‌ی به ـۀ </summary>
            /// <value>Default = false</value>
            [DefaultValue(true)]
            public bool ConvertHeYeToHeHamze { get; set; }

            /// <summary>
            /// تهیه یک کپی از تنظیمات
            /// </summary>
            /// <returns>CharConfig.</returns>
            public CharConfig Clone()
            {
                return new CharConfig
                {
                    IgnoreCharList = new HashSet<char>(IgnoreCharList),
                    LetterNormalization = LetterNormalization,
                    DigitNormalization = DigitNormalization,
                    PunctuationNormalization = PunctuationNormalization,
                    SpaceNormalization = SpaceNormalization,
                    ErabNormalization = ErabNormalization,
                    RemoveExtraSpace = RemoveExtraSpace,
                    RemoveExtraHalfSpace = RemoveExtraHalfSpace,
                    ConvertHeHamzeToHeYe = ConvertHeHamzeToHeYe,
                    ConvertHeYeToHeHamze = ConvertHeYeToHeHamze
                };
            }
        }

        /// <summary>
        /// تنظیمات مربوط به جایگزین نمودن واژه‌ها
        /// </summary>
        public class WordConfig
        {
            public WordConfig()
            {
                StickingCompoundWords = true;
                SplitMultiJoinedWords = true;
                SplitVaBeJoinedWords = true;
                AlternativeWordSuggestion = true;
                HighSensitiveRefinement = false;
            }

            /// <summary> پیشنهاد پیوسته‌نویسی واژه‌های چندبخشی </summary>
            /// <remarks>
            /// مانند «شگفت‌انگیزی» به جای «شگفت انگیزی» پیشنهاد شود
            /// </remarks> 
            /// <value>Default = true</value>
            [DefaultValue(true)]
            public bool StickingCompoundWords { get; set; }

            /// <summary> جداسازی واژه‌های به هم چسبیده </summary>
            /// <remarks>
            /// مانند «گروهمتنکاویفارسییار» به صورت «گروه متن کاوی فارسی یار» اصلاح شود
            /// </remarks> 
            /// <value>Default = false</value>
            [DefaultValue(true)]
            public bool SplitMultiJoinedWords { get; set; }

            /// <summary> جداسازی «و» «ب» چسبیده به ابتدا و انتهای کلمات </summary>
            /// <remarks>
            /// مانند «بشرح» به صورت «به شرح» اصلاح شود
            /// </remarks> 
            /// <value>Default = false</value>
            [DefaultValue(true)]
            public bool SplitVaBeJoinedWords { get; set; }

            /// <summary> حساسیت بالا برای اصلاح نیم‌فاصله‌ها و پیوسته‌نویسی </summary>
            /// <remarks>مثلاً برای کلمه «هیچ کس» شکل پیوسته آن «هیچ‌کس» پیشنهاد شود </remarks>
            /// <value><c>true</c> if [refine high sensitive]; otherwise, <c>false</c>.</value>
            [DefaultValue(false)]
            public bool HighSensitiveRefinement { get; set; }

            /// <summary> پیشنهاد جایگزینی با واژه‌های مصوب فرهنگستان </summary>
            /// <remarks>
            /// مانند «هستم» به جای «می‌باشم» پیشنهاد شود
            /// </remarks> 
            /// <value>Default = true</value>
            [DefaultValue(true)]
            public bool AlternativeWordSuggestion { get; set; }

            /// <summary>
            /// تهیه یک کپی از تنظیمات
            /// </summary>
            /// <returns>WordConfig.</returns>
            public WordConfig Clone()
            {
                return new WordConfig
                {
                    StickingCompoundWords = StickingCompoundWords,
                    SplitMultiJoinedWords = SplitMultiJoinedWords,
                    SplitVaBeJoinedWords = SplitVaBeJoinedWords,
                    HighSensitiveRefinement = HighSensitiveRefinement,
                    AlternativeWordSuggestion = AlternativeWordSuggestion,
                };
            }
        }

        /// <summary>
        /// تنظیمات مربوط به آیین نگارش (دستور خط) فارسی
        /// </summary>
        public class WritingRule
        {
            public WritingRule()
            {
                PrefixCorrection = true;
                SuffixCorrection = true;
                MorphologicalAnalysis = true;
                TanvinCorrection = true;
                HamzeCorrection = true;
                SpaceBetweenPuncCorrection = true;
            }

            /// <summary> اصلاح پیشوند واژه‌ها </summary>
            /// <remarks>
            /// مانند «میشود» یا «می شود» به صورت «می‌شود» اصلاح شود
            /// </remarks> 
            /// <value>Default = true</value>
            [DefaultValue(true)]
            public bool PrefixCorrection { get; set; }

            /// <summary> اصلاح پسوند واژه‌ها </summary>
            /// <remarks>
            /// مانند «انسانها» یا «انسان ها» به صورت «انسان‌ها» اصلاح شود
            /// </remarks> 
            /// <value>Default = true</value>
            [DefaultValue(true)]
            public bool SuffixCorrection { get; set; }

            /// <summary>
            /// جداسازی پسوندها بوسیله تحلیل واژگانی
            /// </summary>
            /// <remarks>
            /// شناسایی پسوندهای جدا بوسیله ریشه‌یابی مثل «خانهای» که به صورت «خانه‌ای» و «خان‌های» اصلاح می‌شود
            /// </remarks>
            /// <value>Default = true</value>
            [DefaultValue(true)]
            public bool MorphologicalAnalysis { get; set; }

            /// <summary> افزودن/اصلاح نگارش تنوین </summary>
            /// <remarks>
            /// مانند «مثلا» به صورت «مثلاً» اصلاح شود
            /// </remarks> 
            /// <value>Default = true</value>
            [DefaultValue(true)]
            public bool TanvinCorrection { get; set; }

            /// <summary> افزودن/اصلاح نگارش همزه </summary>
            /// <remarks>
            /// مانند «مومن» به صورت «مؤمن» اصلاح شود
            /// </remarks> 
            /// <value>Default = true</value>
            [DefaultValue(true)]
            public bool HamzeCorrection { get; set; }

            /*/// <summary> جداسازی/اصلاح فاصله‌گذاری «ب» متصل به ابتدای واژه </summary>
            /// <remarks>
            /// مانند «بعنوان» به صورت «به عنوان» اصلاح شود
            /// </remarks> 
            /// <value>Default = false</value>
            [ConfigItem(Text = "جداسازی/اصلاح فاصله‌گذاری «ب» متصل به ابتدای واژه", Desc = "مانند «بعنوان» به صورت «به عنوان» اصلاح شود", Default = false)]
            public bool StickingBeCorrection { get; set; }*/

            /// <summary> اصلاح فاصله‌گذاری علائم نگارشی </summary>
            /// <remarks>
            /// مانند اصلاح فاصله بعد از نقطه یا ویرگول مثلاً «رفت .سپس» به صورت «رفت. سپس» اصلاح شود
            /// </remarks> 
            /// <value>Default = true</value>
            [DefaultValue(true)]
            public bool SpaceBetweenPuncCorrection { get; set; }

            /// <summary>
            /// تهیه یک کپی از تنظیمات
            /// </summary>
            /// <returns>WritingRule.</returns>
            public WritingRule Clone()
            {
                return new WritingRule
                {
                    PrefixCorrection = PrefixCorrection,
                    SuffixCorrection = SuffixCorrection,
                    MorphologicalAnalysis = MorphologicalAnalysis,
                    TanvinCorrection = TanvinCorrection,
                    HamzeCorrection = HamzeCorrection,
                    //StickingBeCorrection = StickingBeCorrection,
                    SpaceBetweenPuncCorrection = SpaceBetweenPuncCorrection
                };
            }
        }

        /// <summary>
        /// تنظیمات مربوط به غلط‌یاب فارسی
        /// </summary>
        public class SpellConfig
        {
            /// <summary> بررسی اشتباهات تایپی یا املایی واژه‌ها از نظر لغوی </summary>
            public SpellConfig()
            {
                LexicalSpellCheckSuggestionCount = 3;
                RealWordAlternativeSuggestionCount = 2;
                LexicalSpellCheckerDistanceThreshold = 3.0;
                LexicalSpellCheckHighSensitive = false;
                ContextSpellCheckHighSensitive = false;
            }

            /// <summary>
            /// تعداد کلمات کاندید که برای هر واژه غلط پیشنهاد می‌شود
            /// </summary>
            /// <remarks>
            /// به ازای واژه‌هایی که در فهرست واژگان قرار ندارند، چند کلمه نزدیک به آنها از نظر ویرایشی (لغوی) پیشنهاد می‌شود
            /// </remarks> 
            /// <value>The candidate count (Default = 3).</value>
            [DefaultValue(3)]
            public int LexicalSpellCheckSuggestionCount { get; set; }

            /// <summary>
            /// حداکثر فاصله ویرایشی برای کلمات کاندید پیشنهادی برای هر واژه غلط است
            /// </summary>
            /// <remarks>
            /// هر چه فاصله کمتر باشد تعداد پیشنهادات غلط‌یابی کمتر می‌شود
            /// </remarks> 
            /// <value>The candidate distance threshold (Default = 4.0).</value>
            [DefaultValue(4.0)]
            public double LexicalSpellCheckerDistanceThreshold { get; set; }

            /// <summary>
            /// غلط‌یابی لغوی با حساسیت بالا انجام شود
            /// </summary>
            /// <remarks>انجام تحلیل‌های بیشتری (مانند ریشه‌یابی) در فرایند غلط‌یابی انجام می‌شود که باعث تولید پیشنهادات بیشتر می‌شود</remarks>
            [DefaultValue(false)]
            public bool LexicalSpellCheckHighSensitive { get; set; }

            /// <summary>
            /// تعداد پیشنهاد واژه‌های جایگزین برای هر واژه واقعی (درست)
            /// </summary>
            /// <remarks>
            /// بررسی اشتباهات املائی از نوع کلمه واقعی با توجه به موقعیت کلمه در متن برای مثال: «سلم علیکم» تبدیل می‌شود به «سلام علیکم» در حالیکه کلمه «سلم» نیز صحیح است.
            /// </remarks>
            /// <value>The real word alternative suggestion count (Default = 2).</value>
            [DefaultValue(2)]
            public int RealWordAlternativeSuggestionCount { get; set; }

            /// <summary>
            /// غلط‌یابی معنایی با حساسیت بالا انجام شود
            /// </summary>
            /// <remarks>انجام تحلیل‌های بیشتری (از قبیل درنظر گرفتن جایگشت‌های مختلف) در فرایند غلط‌یابی معنایی انجام می‌شود</remarks>
            [DefaultValue(false)]
            public bool ContextSpellCheckHighSensitive { get; set; }

            /// <summary>
            /// تهیه یک کپی از تنظیمات
            /// </summary>
            /// <returns>WritingRule.</returns>
            public SpellConfig Clone()
            {
                return new SpellConfig
                {
                    LexicalSpellCheckSuggestionCount = LexicalSpellCheckSuggestionCount,
                    LexicalSpellCheckerDistanceThreshold = LexicalSpellCheckerDistanceThreshold,
                    LexicalSpellCheckHighSensitive = LexicalSpellCheckHighSensitive,
                    RealWordAlternativeSuggestionCount = RealWordAlternativeSuggestionCount,
                    ContextSpellCheckHighSensitive = ContextSpellCheckHighSensitive
                };
            }
        }

        /// <summary>
        /// تنظیمات مربوط به نادید گرفتن بخش‌هایی از متن ورودی 
        /// </summary>
        public class IgnoreProcessConfig
        {
            public IgnoreProcessConfig()
            {
                IgnoreQuotes = false;
                IgnoreWordsWithErab = true;
                //CustomDictionaryFolderPath = Util.UserLexiconPath;
            }

            /// <summary>
            /// عدم پردازش عبارات داخل گیومه («»)
            /// </summary>
            /// <value><c>true</c> if [ignore quotes]; otherwise, <c>false</c>. (Default = false)</value>
            [DefaultValue(false)]
            public bool IgnoreQuotes { get; set; }

            /// <summary>
            /// عدم پردازش کلمات دارای اعراب
            /// </summary>
            /// <remarks> کلمات دارای اعراب معمولاً عربی هستند مانند: «أَصابَتْهُمْ» و غیره </remarks>
            /// <value><c>true</c> if [ignore words with erab]; otherwise, <c>false</c>.</value>
            [DefaultValue(true)]
            public bool IgnoreWordsWithErab { get; set; }

            /// <summary>
            /// مسیر (فولدر) شامل لیست کلمات صحیح مثل اسامی خاص
            /// </summary>
            /// <remarks> لیست کلمات صحیح که ویراستار به اشتباه سعی در اصلاح آنها دارد </remarks>
            /// <value>The custom dictionary folder path.</value>
            [DefaultValue("مسیر فایل متنی شامل لیست لغات استثنا مثل اسامی خاص")]
            public string CustomDictionaryFolderPath { get; set; }


            /// <summary>
            /// تهیه یک کپی از تنظیمات
            /// </summary>
            /// <returns>IgnoreProcessConfig.</returns>
            public IgnoreProcessConfig Clone()
            {
                return new IgnoreProcessConfig
                {
                    IgnoreQuotes = IgnoreQuotes,
                    IgnoreWordsWithErab = IgnoreWordsWithErab,
                    CustomDictionaryFolderPath = CustomDictionaryFolderPath
                };
            }
        }
    }
}