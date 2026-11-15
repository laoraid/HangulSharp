namespace HangulSharp.Tests;

public class HangulSyllableTests
{
    [Fact(DisplayName = "ToChar: 모든 음절에서 TryDecompose 결과를 원래 음절로 되돌림 (호환, 조합형 모두)")]
    public void ToChar_DecomposedAllSyllables_ReturnsOriginal()
    {
        for (char c = '가'; c <= '힣'; c++)
        {
            Assert.True(Hangul.TryDecompose(c, out HangulSyllable? compat));
            Assert.Equal(c, compat.Value.ToChar());

            Assert.True(Hangul.TryDecompose(c, out HangulSyllable? conjoining, conjoining: true));
            Assert.Equal(c, conjoining.Value.ToChar());
        }
    }

    [Theory(DisplayName = "HasJongseong: 받침이 있으면 true")]
    [InlineData('가', false)]
    [InlineData('각', true)]
    [InlineData('값', true)]
    public void HasJongseong_DecomposedSyllable_ReturnsWhetherFinalExists(char syllable, bool expected)
    {
        Assert.True(Hangul.TryDecompose(syllable, out HangulSyllable? result));
        Assert.Equal(expected, result.Value.HasJongseong);
    }
}
