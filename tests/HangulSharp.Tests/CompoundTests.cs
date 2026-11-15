namespace HangulSharp.Tests;

public class CompoundTests
{
    public static TheoryData<char, char, char> CompoundCases => new()
    {
        { 'ㄳ', 'ㄱ', 'ㅅ' },
        { 'ㄵ', 'ㄴ', 'ㅈ' },
        { 'ㄶ', 'ㄴ', 'ㅎ' },
        { 'ㄺ', 'ㄹ', 'ㄱ' },
        { 'ㄻ', 'ㄹ', 'ㅁ' },
        { 'ㄼ', 'ㄹ', 'ㅂ' },
        { 'ㄽ', 'ㄹ', 'ㅅ' },
        { 'ㄾ', 'ㄹ', 'ㅌ' },
        { 'ㄿ', 'ㄹ', 'ㅍ' },
        { 'ㅀ', 'ㄹ', 'ㅎ' },
        { 'ㅄ', 'ㅂ', 'ㅅ' },
        { 'ㅘ', 'ㅗ', 'ㅏ' },
        { 'ㅙ', 'ㅗ', 'ㅐ' },
        { 'ㅚ', 'ㅗ', 'ㅣ' },
        { 'ㅝ', 'ㅜ', 'ㅓ' },
        { 'ㅞ', 'ㅜ', 'ㅔ' },
        { 'ㅟ', 'ㅜ', 'ㅣ' },
        { 'ㅢ', 'ㅡ', 'ㅣ' },
    };

    // 쌍자음, 키 하나인 모음, 조합형 겹받침, 한글이 아닌 문자
    public static TheoryData<char> NonCompoundCases => new()
    {
        'ㄱ', 'ㄲ', 'ㄸ', 'ㅃ', 'ㅆ', 'ㅉ', 'ㅏ', 'ㅐ', 'ㅔ', 'ㅒ', 'ㅖ', '가', 'ᆪ', 'A', '\0',
    };

    [Theory(DisplayName = "TrySplitCompound: 겹받침, 복합모음을 두 자모로 나눔")]
    [MemberData(nameof(CompoundCases))]
    public void TrySplitCompound_Compound_ReturnsParts(char compound, char first, char second)
    {
        Assert.True(Hangul.TrySplitCompound(compound, out char? actualFirst, out char? actualSecond));
        Assert.Equal(first, actualFirst);
        Assert.Equal(second, actualSecond);
    }

    [Theory(DisplayName = "TrySplitCompound: 겹자모가 아니면 false, 결과는 null")]
    [MemberData(nameof(NonCompoundCases))]
    public void TrySplitCompound_NonCompound_ReturnsFalse(char jamo)
    {
        Assert.False(Hangul.TrySplitCompound(jamo, out char? first, out char? second));
        Assert.Null(first);
        Assert.Null(second);
    }

    [Theory(DisplayName = "TryMergeCompound: 두 자모를 겹받침, 복합모음으로 합침")]
    [MemberData(nameof(CompoundCases))]
    public void TryMergeCompound_ValidPair_ReturnsCompound(char compound, char first, char second)
    {
        Assert.True(Hangul.TryMergeCompound(first, second, out char? merged));
        Assert.Equal(compound, merged);
    }

    [Theory(DisplayName = "TryMergeCompound: 합칠 수 없는 쌍이면 false, 결과는 null")]
    [InlineData('ㄱ', 'ㄱ')] // 쌍자음은 겹받침이 아님
    [InlineData('ㅅ', 'ㄱ')] // 순서가 반대
    [InlineData('ㅏ', 'ㅣ')] // ㅐ는 복합모음으로 다루지 않음
    [InlineData('ㄱ', 'ㅏ')]
    public void TryMergeCompound_InvalidPair_ReturnsFalse(char first, char second)
    {
        Assert.False(Hangul.TryMergeCompound(first, second, out char? merged));
        Assert.Null(merged);
    }

    public static TheoryData<char> CompoundJamoCases => new()
    {
        'ㄳ', 'ㄵ', 'ㄶ', 'ㄺ', 'ㄻ', 'ㄼ', 'ㄽ', 'ㄾ', 'ㄿ', 'ㅀ', 'ㅄ', 'ㅘ', 'ㅙ', 'ㅚ', 'ㅝ', 'ㅞ', 'ㅟ', 'ㅢ',
    };

    [Theory(DisplayName = "IsCompound: 겹받침, 복합모음이면 true")]
    [MemberData(nameof(CompoundJamoCases))]
    public void IsCompound_Compound_ReturnsTrue(char compound)
    {
        Assert.True(Hangul.IsCompound(compound));
    }

    [Theory(DisplayName = "IsCompound: 겹자모가 아니면 false")]
    [MemberData(nameof(NonCompoundCases))]
    public void IsCompound_NonCompound_ReturnsFalse(char jamo)
    {
        Assert.False(Hangul.IsCompound(jamo));
    }
}
