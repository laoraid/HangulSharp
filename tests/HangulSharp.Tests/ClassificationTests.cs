namespace HangulSharp.Tests;

public class ClassificationTests
{
    #region IsTypeOf

    private static readonly HangulType[] SingleTypes =
    [
        HangulType.Syllable,
        HangulType.CompatibilityJamo,
        HangulType.ConjoiningJamo,
        HangulType.ArchaicCompatibilityJamo,
        HangulType.ArchaicConjoiningJamo,
    ];

    [Theory(DisplayName = "IsTypeOf: 한글 문자는 자기가 속한 단일 종류에서만 true")]
    [InlineData('가', HangulType.Syllable)] // U+AC00
    [InlineData('힣', HangulType.Syllable)] // U+D7A3
    [InlineData('ㄱ', HangulType.CompatibilityJamo)] // U+3131
    [InlineData('ㅣ', HangulType.CompatibilityJamo)] // U+3163
    [InlineData('ᄀ', HangulType.ConjoiningJamo)] // U+1100
    [InlineData('ᄒ', HangulType.ConjoiningJamo)] // U+1112
    [InlineData('ᅡ', HangulType.ConjoiningJamo)] // U+1161
    [InlineData('ᅵ', HangulType.ConjoiningJamo)] // U+1175
    [InlineData('ᆨ', HangulType.ConjoiningJamo)] // U+11A8
    [InlineData('ᇂ', HangulType.ConjoiningJamo)] // U+11C2
    [InlineData('ㅥ', HangulType.ArchaicCompatibilityJamo)] // U+3165 (호환 옛한글 시작)
    [InlineData('ㆍ', HangulType.ArchaicCompatibilityJamo)] // U+318D
    [InlineData('ㆎ', HangulType.ArchaicCompatibilityJamo)] // U+318E (호환 옛한글 끝)
    [InlineData('ᄓ', HangulType.ArchaicConjoiningJamo)] // U+1113 옛 초성 시작
    [InlineData('ᅞ', HangulType.ArchaicConjoiningJamo)] // U+115E 옛 초성 끝
    [InlineData('ᅶ', HangulType.ArchaicConjoiningJamo)] // U+1176 옛 중성 시작
    [InlineData('ᆧ', HangulType.ArchaicConjoiningJamo)] // U+11A7 옛 중성 끝
    [InlineData('ᇃ', HangulType.ArchaicConjoiningJamo)] // U+11C3 옛 종성 시작
    [InlineData('ᇿ', HangulType.ArchaicConjoiningJamo)] // U+11FF 옛 종성 끝
    [InlineData('ꥠ', HangulType.ArchaicConjoiningJamo)] // U+A960 확장 A 시작
    [InlineData('ꥼ', HangulType.ArchaicConjoiningJamo)] // U+A97C 확장 A 끝
    [InlineData('ힰ', HangulType.ArchaicConjoiningJamo)] // U+D7B0 확장 B 중성 시작
    [InlineData('ퟆ', HangulType.ArchaicConjoiningJamo)] // U+D7C6 확장 B 중성 끝
    [InlineData('ퟋ', HangulType.ArchaicConjoiningJamo)] // U+D7CB 확장 B 종성 시작
    [InlineData('ퟻ', HangulType.ArchaicConjoiningJamo)] // U+D7FB 확장 B 종성 끝
    public void IsTypeOf_HangulChar_TrueOnlyForOwnSingleType(char c, HangulType expected)
    {
        foreach (HangulType type in SingleTypes)
        {
            Assert.Equal(type == expected, Hangul.IsTypeOf(c, type));
        }
    }

    [Theory(DisplayName = "IsTypeOf: 조합 값은 포함한 단일 종류를 합친 범위와 같음")]
    [InlineData('가', true, false, true, true)] // 음절
    [InlineData('ㄱ', true, false, true, true)] // 현대 호환 자모
    [InlineData('ᄀ', true, false, true, true)] // 현대 조합형 자모
    [InlineData('ㆍ', false, true, false, true)] // 옛한글 호환 자모
    [InlineData('ᇫ', false, true, false, true)] // 옛한글 조합형 자모
    public void IsTypeOf_CompositeType_MatchesUnionOfSingleTypes(char c, bool jamoOrSyllable, bool archaic, bool modern, bool all)
    {
        Assert.Equal(jamoOrSyllable, Hangul.IsTypeOf(c, HangulType.Jamo | HangulType.Syllable));
        Assert.Equal(archaic, Hangul.IsTypeOf(c, HangulType.Archaic));
        Assert.Equal(modern, Hangul.IsTypeOf(c, HangulType.Modern));
        Assert.Equal(all, Hangul.IsTypeOf(c, HangulType.All));
    }

    [Theory(DisplayName = "IsTypeOf: 한글이 아닌 문자와 채움 문자는 All이어도 false")]
    [InlineData('ㅤ')] // 호환 채움
    [InlineData('ᅟ')] // 초성 채움
    [InlineData('ᅠ')] // 중성 채움
    [InlineData('㄰')] // 호환 자모 직전 (미할당)
    [InlineData('㆏')] // 호환 자모 직후 (미할당)
    [InlineData('꯿')] // 완성형 직전
    [InlineData('힤')] // 완성형 직후 (미할당)
    [InlineData('꥽')] // 확장 A 직후 (미할당)
    [InlineData('퟇')] // 확장 B 중성 직후 (미할당)
    [InlineData('A')]
    [InlineData('\0')]
    public void IsTypeOf_NonHangulOrFiller_ReturnsFalse(char c)
    {
        Assert.False(Hangul.IsTypeOf(c, HangulType.All));
    }

    [Fact(DisplayName = "IsTypeOf: None이면 항상 false")]
    public void IsTypeOf_NoneType_ReturnsFalse()
    {
        Assert.False(Hangul.IsTypeOf('가', HangulType.None));
        Assert.False(Hangul.IsTypeOf('ㄱ', HangulType.None));
    }

    [Theory(DisplayName = "IsTypeOf: 문자열은 모든 문자가 해당할 때만 true, 빈 문자열은 false")]
    [InlineData("한글", HangulType.Syllable, true)]
    [InlineData("한ㄱ", HangulType.Syllable, false)]
    [InlineData("한ㄱ", HangulType.Syllable | HangulType.CompatibilityJamo, true)]
    [InlineData("한 글", HangulType.All, false)]
    [InlineData("", HangulType.All, false)]
    public void IsTypeOf_String_ReturnsWhetherAllCharsMatch(string text, HangulType type, bool expected)
    {
        Assert.Equal(expected, Hangul.IsTypeOf(text, type));
        Assert.Equal(expected, Hangul.IsTypeOf(text.AsSpan(), type));
    }

    [Fact(DisplayName = "IsTypeOf: 문자열이 null이면 ArgumentNullException")]
    public void IsTypeOf_NullString_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("text", () => Hangul.IsTypeOf((string)null!, HangulType.All));
    }

    #endregion

    #region 바로가기

    [Theory(DisplayName = "IsHangul: 현대 한글 문자만 true")]
    [InlineData('가', true)] // 가
    [InlineData('ㄱ', true)] // ㄱ
    [InlineData('ᄀ', true)] // ᄀ
    [InlineData('ㆍ', false)] // ㆍ (옛한글)
    [InlineData('A', false)]
    public void IsHangul_Char_TrueOnlyForModernHangul(char c, bool expected)
    {
        Assert.Equal(expected, Hangul.IsHangul(c));
    }

    [Theory(DisplayName = "IsHangul: 문자열은 모든 문자가 현대 한글일 때만 true")]
    [InlineData("안녕하세요", true)]
    [InlineData("안녕!", false)]
    [InlineData("", false)]
    public void IsHangul_String_ReturnsWhetherAllCharsAreModernHangul(string text, bool expected)
    {
        Assert.Equal(expected, Hangul.IsHangul(text));
        Assert.Equal(expected, Hangul.IsHangul(text.AsSpan()));
    }

    [Fact(DisplayName = "IsHangul: 문자열이 null이면 ArgumentNullException")]
    public void IsHangul_NullString_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("text", () => Hangul.IsHangul((string)null!));
    }

    [Theory(DisplayName = "IsSyllable: 가~힣 범위만 true")]
    [InlineData('가', true)] // 가
    [InlineData('힣', true)] // 힣
    [InlineData('꯿', false)]
    [InlineData('힤', false)]
    [InlineData('ㄱ', false)] // ㄱ
    public void IsSyllable_Char_TrueOnlyInSyllableRange(char c, bool expected)
    {
        Assert.Equal(expected, Hangul.IsSyllable(c));
    }

    [Theory(DisplayName = "IsJamo: 현대 호환, 조합형 자모만 true")]
    [InlineData('ㄱ', true)] // ㄱ
    [InlineData('ᄀ', true)] // ᄀ
    [InlineData('가', false)] // 가
    [InlineData('ㆍ', false)] // ㆍ (옛한글)
    public void IsJamo_Char_TrueOnlyForModernJamo(char c, bool expected)
    {
        Assert.Equal(expected, Hangul.IsJamo(c));
    }

    #endregion

    #region 자모 세부 판별

    public static TheoryData<char, JamoKind> JamoKindCases => new()
    {
        { 'ㄱ', JamoKind.Consonant }, // ㄱ
        { 'ㄳ', JamoKind.Consonant }, // ㄳ
        { 'ㅎ', JamoKind.Consonant }, // ㅎ
        { 'ㅏ', JamoKind.Vowel }, // ㅏ
        { 'ㅣ', JamoKind.Vowel }, // ㅣ
        { 'ㅥ', JamoKind.Consonant }, // ㅥ
        { 'ㆆ', JamoKind.Consonant }, // ㆆ
        { 'ㆇ', JamoKind.Vowel }, // ㆇ
        { 'ㆍ', JamoKind.Vowel }, // ㆍ
        { 'ᄀ', JamoKind.Consonant }, // ᄀ
        { 'ᄓ', JamoKind.Consonant }, // 옛 초성
        { 'ᅡ', JamoKind.Vowel }, // ᅡ
        { 'ᆧ', JamoKind.Vowel }, // 옛 중성
        { 'ᆨ', JamoKind.Consonant }, // ᆨ
        { 'ᇫ', JamoKind.Consonant }, // ᇫ
        { 'ꥠ', JamoKind.Consonant }, // 확장 A 초성
        { 'ힰ', JamoKind.Vowel }, // 확장 B 중성
        { 'ퟋ', JamoKind.Consonant }, // 확장 B 종성
        { '가', JamoKind.None }, // 가
        { 'ㅤ', JamoKind.None }, // 호환 채움
        { 'ᅟ', JamoKind.None }, // 초성 채움
        { 'ᅠ', JamoKind.None }, // 중성 채움
        { 'A', JamoKind.None },
        { '\0', JamoKind.None },
    };

    [Theory(DisplayName = "GetJamoKind: 호환, 조합형, 옛한글 자모의 자음/모음을 구분하고 자모가 아니면 None")]
    [MemberData(nameof(JamoKindCases))]
    public void GetJamoKind_Char_ReturnsPhoneticKind(char c, JamoKind expected)
    {
        Assert.Equal(expected, Hangul.GetJamoKind(c));
    }

    [Theory(DisplayName = "IsConsonant: 자음 자모만 true")]
    [MemberData(nameof(JamoKindCases))]
    public void IsConsonant_Char_TrueOnlyForConsonant(char c, JamoKind kind)
    {
        Assert.Equal(kind == JamoKind.Consonant, Hangul.IsConsonant(c));
    }

    [Theory(DisplayName = "IsVowel: 모음 자모만 true")]
    [MemberData(nameof(JamoKindCases))]
    public void IsVowel_Char_TrueOnlyForVowel(char c, JamoKind kind)
    {
        Assert.Equal(kind == JamoKind.Vowel, Hangul.IsVowel(c));
    }

    // (문자, 초성 가능, 중성 가능, 종성 가능)
    // 호환 자모는 그 자리에 올 수 있는가, 조합형 자모는 문자에 정해진 위치로 판별한다.
    public static TheoryData<char, bool, bool, bool> PositionCases => new()
    {
        { 'ㄱ', true, false, true }, // ㄱ
        { 'ㄳ', false, false, true }, // ㄳ (겹받침)
        { 'ㄸ', true, false, false }, // ㄸ
        { 'ㅃ', true, false, false }, // ㅃ
        { 'ㅉ', true, false, false }, // ㅉ
        { 'ㅏ', false, true, false }, // ㅏ
        { 'ㅘ', false, true, false }, // ㅘ
        { 'ㅥ', false, false, false }, // ㅥ (옛한글 호환은 위치 미정)
        { 'ᄀ', true, false, false }, // ᄀ
        { 'ᅡ', false, true, false }, // ᅡ
        { 'ᆨ', false, false, true }, // ᆨ
        { 'ᄓ', true, false, false }, // 옛 초성
        { 'ᆧ', false, true, false }, // 옛 중성
        { 'ᇫ', false, false, true }, // ᇫ
        { 'ꥠ', true, false, false }, // 확장 A 초성
        { 'ힰ', false, true, false }, // 확장 B 중성
        { 'ퟋ', false, false, true }, // 확장 B 종성
        { '가', false, false, false }, // 가
        { 'ᅟ', false, false, false }, // 초성 채움
        { '\0', false, false, false }, // 받침 없음 표시값
    };

    [Theory(DisplayName = "IsChoseong: 호환 자모는 초성 자리에 올 수 있을 때, 조합형 자모는 초성 문자일 때 true")]
    [MemberData(nameof(PositionCases))]
    public void IsChoseong_Char_ReturnsWhetherChoseongAllowed(char c, bool choseong, bool jungseong, bool jongseong)
    {
        _ = (jungseong, jongseong);
        Assert.Equal(choseong, Hangul.IsChoseong(c));
    }

    [Theory(DisplayName = "IsJungseong: 호환 자모는 중성 자리에 올 수 있을 때, 조합형 자모는 중성 문자일 때 true")]
    [MemberData(nameof(PositionCases))]
    public void IsJungseong_Char_ReturnsWhetherJungseongAllowed(char c, bool choseong, bool jungseong, bool jongseong)
    {
        _ = (choseong, jongseong);
        Assert.Equal(jungseong, Hangul.IsJungseong(c));
    }

    [Theory(DisplayName = "IsJongseong: 호환 자모는 받침 자리에 올 수 있을 때, 조합형 자모는 종성 문자일 때 true")]
    [MemberData(nameof(PositionCases))]
    public void IsJongseong_Char_ReturnsWhetherJongseongAllowed(char c, bool choseong, bool jungseong, bool jongseong)
    {
        _ = (choseong, jungseong);
        Assert.Equal(jongseong, Hangul.IsJongseong(c));
    }

    #endregion
}
