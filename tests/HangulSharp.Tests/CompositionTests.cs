using System.Text;

namespace HangulSharp.Tests;

public class CompositionTests
{
    #region TryDecompose

    [Theory(DisplayName = "TryDecompose: 기본값은 호환 자모로 나눔")]
    [InlineData('가', 'ㄱ', 'ㅏ', null)]
    [InlineData('값', 'ㄱ', 'ㅏ', 'ㅄ')]
    [InlineData('뷁', 'ㅂ', 'ㅞ', 'ㄺ')]
    [InlineData('힣', 'ㅎ', 'ㅣ', 'ㅎ')]
    public void TryDecompose_DefaultMode_ReturnsCompatibilityJamo(char syllable, char choseong, char jungseong, char? jongseong)
    {
        Assert.True(Hangul.TryDecompose(syllable, out HangulSyllable? result));
        Assert.Equal(new HangulSyllable(choseong, jungseong, jongseong), result);
    }

    [Theory(DisplayName = "TryDecompose: conjoining이 true면 조합형 자모로 나눔")]
    [InlineData('가', 'ᄀ', 'ᅡ', null)] // ᄀ ᅡ
    [InlineData('값', 'ᄀ', 'ᅡ', 'ᆹ')] // ᄀ ᅡ ᆹ
    [InlineData('뷁', 'ᄇ', 'ᅰ', 'ᆰ')] // ᄇ ᅰ ᆰ
    [InlineData('힣', 'ᄒ', 'ᅵ', 'ᇂ')] // ᄒ ᅵ ᇂ
    public void TryDecompose_ConjoiningMode_ReturnsConjoiningJamo(char syllable, char choseong, char jungseong, char? jongseong)
    {
        Assert.True(Hangul.TryDecompose(syllable, out HangulSyllable? result, conjoining: true));
        Assert.Equal(new HangulSyllable(choseong, jungseong, jongseong), result);
    }

    [Fact(DisplayName = "TryDecompose: 조합형 분해 결과가 모든 음절에서 NFD와 같음")]
    public void TryDecompose_ConjoiningModeForAllSyllables_MatchesNfd()
    {
        for (char c = '가'; c <= '힣'; c++)
        {
            Assert.True(Hangul.TryDecompose(c, out HangulSyllable? result, conjoining: true));

            string expected = c.ToString().Normalize(NormalizationForm.FormD);
            string actual = $"{result.Value.Choseong}{result.Value.Jungseong}{result.Value.Jongseong}";
            Assert.Equal(expected, actual);
        }
    }

    [Theory(DisplayName = "TryDecompose: 완성형 음절이 아니면 false, 결과는 null")]
    [InlineData('ㄱ')]
    [InlineData('ᄀ')] // ᄀ
    [InlineData('A')]
    [InlineData('힤')] // 힣 다음 (미할당)
    public void TryDecompose_NonSyllable_ReturnsFalse(char c)
    {
        Assert.False(Hangul.TryDecompose(c, out HangulSyllable? result));
        Assert.Null(result);
        Assert.False(Hangul.TryDecompose(c, out result, conjoining: true));
        Assert.Null(result);
    }

    #endregion

    #region Combine, TryCombine

    [Theory(DisplayName = "Combine: 호환 자모로 음절을 만듦")]
    [InlineData('ㄱ', 'ㅏ', null, '가')]
    [InlineData('ㄱ', 'ㅏ', 'ㅄ', '값')]
    [InlineData('ㅂ', 'ㅞ', 'ㄺ', '뷁')]
    [InlineData('ㅎ', 'ㅣ', 'ㅎ', '힣')]
    public void Combine_CompatibilityJamo_ReturnsSyllable(char choseong, char jungseong, char? jongseong, char expected)
    {
        Assert.Equal(expected, Hangul.Combine(choseong, jungseong, jongseong));
    }

    [Theory(DisplayName = "Combine: 조합형 자모로 음절을 만듦")]
    [InlineData('ᄀ', 'ᅡ', null, '가')] // ᄀ ᅡ
    [InlineData('ᄀ', 'ᅡ', 'ᆹ', '값')] // ᄀ ᅡ ᆹ
    [InlineData('ᄒ', 'ᅵ', 'ᇂ', '힣')] // ᄒ ᅵ ᇂ
    public void Combine_ConjoiningJamo_ReturnsSyllable(char choseong, char jungseong, char? jongseong, char expected)
    {
        Assert.Equal(expected, Hangul.Combine(choseong, jungseong, jongseong));
    }

    [Theory(DisplayName = "Combine: 해당 위치에 쓸 수 없는 자모면 ArgumentException")]
    [InlineData('ㄳ', 'ㅏ', null, "choseong")] // 겹받침은 초성 불가
    [InlineData('ㅏ', 'ㅏ', null, "choseong")]
    [InlineData('ㄱ', 'ㄱ', null, "jungseong")]
    [InlineData('ㄱ', 'ㅏ', 'ㄸ', "jongseong")] // ㄸ은 받침 불가
    [InlineData('ㄱ', 'ㅏ', '\0', "jongseong")] // 받침 없음은 null로 표현
    [InlineData('ᆨ', 'ㅏ', null, "choseong")] // 조합형 종성을 초성 자리에
    [InlineData('ㄱ', 'ㅏ', 'ᄀ', "jongseong")] // 조합형 초성을 종성 자리에
    public void Combine_InvalidJamo_ThrowsArgumentException(char choseong, char jungseong, char? jongseong, string paramName)
    {
        Assert.Throws<ArgumentException>(paramName, () => Hangul.Combine(choseong, jungseong, jongseong));
    }

    [Fact(DisplayName = "Combine: 모든 음절에서 TryDecompose 결과를 되돌림 (호환, 조합형 모두)")]
    public void Combine_DecomposedAllSyllables_ReturnsOriginal()
    {
        for (char c = '가'; c <= '힣'; c++)
        {
            Assert.True(Hangul.TryDecompose(c, out HangulSyllable? compat));
            Assert.Equal(c, Hangul.Combine(compat.Value.Choseong, compat.Value.Jungseong, compat.Value.Jongseong));

            Assert.True(Hangul.TryDecompose(c, out HangulSyllable? conjoining, conjoining: true));
            Assert.Equal(c, Hangul.Combine(conjoining.Value.Choseong, conjoining.Value.Jungseong, conjoining.Value.Jongseong));
        }
    }

    [Theory(DisplayName = "TryCombine: 조합할 수 있으면 true와 음절, 없으면 false와 null")]
    [InlineData('ㄱ', 'ㅏ', 'ㅄ', true, '값')]
    [InlineData('ㄱ', 'ㅏ', null, true, '가')]
    [InlineData('ㄳ', 'ㅏ', null, false, null)]
    [InlineData('ㄱ', 'ㅏ', 'ㄸ', false, null)]
    public void TryCombine_Jamo_ReturnsWhetherCombinable(char choseong, char jungseong, char? jongseong, bool expected, char? expectedResult)
    {
        Assert.Equal(expected, Hangul.TryCombine(choseong, jungseong, jongseong, out char? result));
        Assert.Equal(expectedResult, result);
    }

    [Fact(DisplayName = "TryCombine: 받침 없는 오버로드는 받침 null과 같음")]
    public void TryCombine_WithoutJongseong_SameAsNullJongseong()
    {
        Assert.True(Hangul.TryCombine('ㄱ', 'ㅏ', out char? result));
        Assert.Equal('가', result);
    }

    #endregion

    #region Disassemble, TryDisassemble

    [Theory(DisplayName = "Disassemble: 음절과 겹자모를 호환 자모로 끝까지 풂")]
    [InlineData("값", "ㄱㅏㅂㅅ")]
    [InlineData("과", "ㄱㅗㅏ")]
    [InlineData("꽭", "ㄲㅗㅐㄹㄱ")] // 쌍자음과 ㅐ는 나누지 않음
    [InlineData("의자", "ㅇㅡㅣㅈㅏ")]
    [InlineData("ㄳㅘ", "ㄱㅅㅗㅏ")] // 낱자 겹자모도 나눔
    [InlineData("한글 ABC!", "ㅎㅏㄴㄱㅡㄹ ABC!")]
    public void Disassemble_Text_SplitsToCompatibilityJamo(string text, string expected)
    {
        Assert.Equal(expected, Hangul.Disassemble(text));
    }

    [Theory(DisplayName = "Disassemble: 나눌 것이 없으면 원본 인스턴스를 그대로 반환")]
    [InlineData("")]
    [InlineData("ABC")]
    [InlineData("ㄱㅏ")]
    [InlineData("가")] // 조합형 자모는 그대로
    public void Disassemble_NothingToSplit_ReturnsSameInstance(string text)
    {
        Assert.Same(text, Hangul.Disassemble(text));
    }

    [Fact(DisplayName = "Disassemble: 문자열이 null이면 ArgumentNullException")]
    public void Disassemble_NullString_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("text", () => Hangul.Disassemble(null!));
    }

    [Fact(DisplayName = "Disassemble: 스택 버퍼 한도를 넘는 긴 문자열도 처리")]
    public void Disassemble_LongText_SplitsAll()
    {
        string text = new('꽭', 1000);
        string expected = string.Concat(Enumerable.Repeat("ㄲㅗㅐㄹㄱ", 1000));
        Assert.Equal(expected, Hangul.Disassemble(text));
    }

    [Fact(DisplayName = "Disassemble: 정확한 길이를 세는 기준(2^20자)을 넘는 입력도 처리")]
    public void Disassemble_TextOverExactLengthThreshold_SplitsAll()
    {
        // Hangul.Composition.cs의 ExactLengthThreshold(1 << 20)를 넘도록 만든다.
        const int repeat = (1 << 20) / 5 + 1;
        string text = string.Concat(Enumerable.Repeat("가ABC꽭", repeat));
        string expected = string.Concat(Enumerable.Repeat("ㄱㅏABCㄲㅗㅐㄹㄱ", repeat));

        Assert.True(text.Length > 1 << 20);
        Assert.Equal(expected, Hangul.Disassemble(text));
    }

    [Theory(DisplayName = "TryDisassemble: 버퍼가 부족하면 false, charsWritten은 0")]
    [InlineData("값", 3)]
    [InlineData("꽭", 4)]
    [InlineData("가나", 3)]
    public void TryDisassemble_SmallBuffer_ReturnsFalse(string text, int bufferLength)
    {
        Span<char> buffer = stackalloc char[bufferLength];
        Assert.False(Hangul.TryDisassemble(text, buffer, out int written));
        Assert.Equal(0, written);
    }

    [Fact(DisplayName = "TryDisassemble: GetMaxDisassembledLength 크기의 버퍼면 항상 성공")]
    public void TryDisassemble_MaxLengthBuffer_Succeeds()
    {
        const string text = "꽭뷁ㄳ";
        Span<char> buffer = stackalloc char[Hangul.GetMaxDisassembledLength(text.Length)];
        Assert.True(Hangul.TryDisassemble(text, buffer, out int written));
        Assert.Equal("ㄲㅗㅐㄹㄱㅂㅜㅔㄹㄱㄱㅅ", buffer.Slice(0, written).ToString());
    }

    [Fact(DisplayName = "TryDisassemble: 문자열이 null이면 ArgumentNullException")]
    public void TryDisassemble_NullString_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("text", () => Hangul.TryDisassemble((string)null!, Span<char>.Empty, out _));
    }

    [Theory(DisplayName = "GetMaxDisassembledLength: 길이의 5배를 반환")]
    [InlineData(0, 0)]
    [InlineData(1, 5)]
    [InlineData(int.MaxValue / 5, int.MaxValue / 5 * 5)] // 허용 최대값
    public void GetMaxDisassembledLength_ValidLength_ReturnsFiveTimes(int textLength, int expected)
    {
        Assert.Equal(expected, Hangul.GetMaxDisassembledLength(textLength));
    }

    [Theory(DisplayName = "GetMaxDisassembledLength: 음수이거나 결과가 int 범위를 넘으면 ArgumentOutOfRangeException")]
    [InlineData(-1)]
    [InlineData(int.MaxValue / 5 + 1)]
    [InlineData(int.MaxValue)]
    public void GetMaxDisassembledLength_OutOfRange_ThrowsArgumentOutOfRangeException(int textLength)
    {
        Assert.Throws<ArgumentOutOfRangeException>("textLength", () => Hangul.GetMaxDisassembledLength(textLength));
    }

    #endregion

    #region Assemble, TryAssemble

    [Theory(DisplayName = "Assemble: 두벌식 규칙으로 음절을 조립")]
    [InlineData("ㄱㅏ", "가")]
    [InlineData("ㄱㅏㅂㅅ", "값")] // 겹받침
    [InlineData("ㄱㅗㅏ", "과")] // 복합모음
    [InlineData("ㄱㅏㅂㅅㅣ", "갑시")] // 도깨비불: 겹받침의 뒤 자모만 넘어감
    [InlineData("ㄱㅏㄱㅏ", "가가")] // 도깨비불: 홑받침이 통째로 넘어감
    [InlineData("ㄱㅏㄸㅏ", "가따")] // ㄸ은 받침이 될 수 없어 새 글자
    [InlineData("ㄱㅏㄱㄱ", "각ㄱ")] // ㄱㄱ은 겹받침이 아님
    [InlineData("ㅇㅡㅣ", "의")]
    [InlineData("ㅋㅋ", "ㅋㅋ")] // 모음 없는 자음은 그대로
    [InlineData("ㅏㅏ", "ㅏㅏ")] // 초성 없는 모음은 그대로
    [InlineData("ㅗㅏ", "ㅘ")] // 초성 없는 복합모음
    [InlineData("ㄳ", "ㄳ")] // 초성이 될 수 없는 자음
    [InlineData("ㄱㅏㄳ", "갃")] // 겹받침 낱자를 받침으로
    [InlineData("ㄱㅏㄳㅏ", "각사")] // 겹받침 낱자 받침도 도깨비불로 뒤 자모만 넘어감
    [InlineData("ㄱㅏㄲㅏ", "가까")] // 쌍자음 받침은 통째로 넘어감
    [InlineData("ㄱㅏㅆㅏ", "가싸")]
    [InlineData("ㄱㅏㄹㄱㅅ", "갉ㅅ")] // 겹받침 뒤 자음은 새 글자
    [InlineData("ㅏㄱㅏ", "ㅏ가")] // 모음 단독 다음 음절
    [InlineData("ㄱㅘㅏ", "과ㅏ")] // 복합모음 다음 모음
    [InlineData("ㄱㅅ", "ㄳ")] // 모음 없는 자음끼리 겹자음으로 합침
    [InlineData("ㄹㄱㅁ", "ㄺㅁ")]
    [InlineData("ㄱㄱ", "ㄱㄱ")] // 겹자음이 아니면 합치지 않음
    [InlineData("ㄱㅅㅏ", "ㄱ사")] // 겹자음 뒤 모음이면 뒤 자모가 초성으로
    [InlineData("ㄳㅏ", "ㄱ사")] // 겹자음 낱자도 같음
    [InlineData("ㄱㅏ ㄴㅏ", "가 나")] // 다른 문자에서 조립이 끊김
    [InlineData("ㄱ1ㅏ", "ㄱ1ㅏ")]
    [InlineData("ㄱㆍㅏ", "ㄱㆍㅏ")] // 옛한글 호환 자모(ㆍ)는 조립하지 않음
    [InlineData("ㄱㅤㅏ", "ㄱㅤㅏ")] // 채움 문자
    [InlineData("ㄱᅡ", "ㄱᅡ")] // 조합형 자모(ᅡ)
    public void Assemble_Jamo_ReturnsAssembledText(string jamos, string expected)
    {
        Assert.Equal(expected, Hangul.Assemble(jamos));
    }

    [Theory(DisplayName = "Assemble: 조립할 것이 없으면 원본 인스턴스를 그대로 반환")]
    [InlineData("")]
    [InlineData("ㄱ")]
    [InlineData("가나다")]
    [InlineData("ABC")]
    public void Assemble_NothingToAssemble_ReturnsSameInstance(string jamos)
    {
        Assert.Same(jamos, Hangul.Assemble(jamos));
    }

    [Fact(DisplayName = "Assemble: 문자열이 null이면 ArgumentNullException")]
    public void Assemble_NullString_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("jamos", () => Hangul.Assemble(null!));
    }

    [Fact(DisplayName = "Assemble: 모든 음절에서 Disassemble 결과를 되돌림")]
    public void Assemble_DisassembledAllSyllables_ReturnsOriginal()
    {
        for (char c = '가'; c <= '힣'; c++)
        {
            string text = c.ToString();
            Assert.Equal(text, Hangul.Assemble(Hangul.Disassemble(text)));
        }
    }

    [Fact(DisplayName = "Assemble: 무작위 음절 문자열에서 Disassemble 결과를 되돌림")]
    public void Assemble_DisassembledRandomText_ReturnsOriginal()
    {
        var random = new Random(20261112);
        char[] chars = new char[8];

        for (int i = 0; i < 5000; i++)
        {
            for (int j = 0; j < chars.Length; j++)
            {
                chars[j] = (char)random.Next('가', '힣' + 1);
            }

            string text = new(chars);
            Assert.Equal(text, Hangul.Assemble(Hangul.Disassemble(text)));
        }
    }

    [Fact(DisplayName = "Assemble: 겹자음 낱자는 Disassemble 후 다시 조립하면 되돌아옴")]
    public void Assemble_DisassembledCompoundConsonants_ReturnsOriginal()
    {
        foreach (char compound in "ㄳㄵㄶㄺㄻㄼㄽㄾㄿㅀㅄㅘㅙㅚㅝㅞㅟㅢ")
        {
            string text = compound.ToString();
            Assert.Equal(text, Hangul.Assemble(Hangul.Disassemble(text)));
        }
    }

    [Fact(DisplayName = "TryAssemble: 자모와 다른 문자를 섞은 무작위 입력에서 예외 없이 입력 길이 버퍼 안에 씀")]
    public void TryAssemble_RandomMixedInput_FitsInputLengthBuffer()
    {
        const string pool = "ㄱㄲㄳㄴㄵㄶㄷㄸㄹㄺㅀㅁㅂㅃㅄㅅㅆㅇㅈㅉㅊㅋㅌㅍㅎㅏㅐㅑㅗㅘㅙㅚㅜㅝㅞㅟㅡㅢㅣ가A 1ㆍㅤᄀ";
        var random = new Random(20261113);

        for (int i = 0; i < 20000; i++)
        {
            char[] chars = new char[random.Next(0, 13)];
            for (int j = 0; j < chars.Length; j++)
            {
                chars[j] = pool[random.Next(pool.Length)];
            }

            char[] buffer = new char[chars.Length];
            Assert.True(Hangul.TryAssemble(chars, buffer, out int written));
            Assert.InRange(written, 0, chars.Length);
        }
    }

    [Fact(DisplayName = "TryAssemble: 입력과 같은 길이의 버퍼면 성공")]
    public void TryAssemble_InputLengthBuffer_Succeeds()
    {
        const string jamos = "ㄱㅏㅂㅅㅣ";
        Span<char> buffer = stackalloc char[jamos.Length];
        Assert.True(Hangul.TryAssemble(jamos, buffer, out int written));
        Assert.Equal("갑시", buffer.Slice(0, written).ToString());
    }

    [Fact(DisplayName = "TryAssemble: 버퍼가 부족하면 false, charsWritten은 0")]
    public void TryAssemble_SmallBuffer_ReturnsFalse()
    {
        Span<char> buffer = stackalloc char[1];
        Assert.False(Hangul.TryAssemble("ㄱㅏㅂㅅㅣ", buffer, out int written));
        Assert.Equal(0, written);
    }

    [Fact(DisplayName = "TryAssemble: 마지막 글자를 마무리할 때 버퍼가 부족해도 false, charsWritten은 0")]
    public void TryAssemble_BufferFullAtFinalFlush_ReturnsFalse()
    {
        // "가"는 도깨비불 때 들어가고, 마지막 "나"를 마무리할 때 버퍼가 부족해진다.
        Span<char> buffer = stackalloc char[1];
        Assert.False(Hangul.TryAssemble("ㄱㅏㄴㅏ", buffer, out int written));
        Assert.Equal(0, written);
    }

    [Fact(DisplayName = "TryAssemble: 문자열이 null이면 ArgumentNullException")]
    public void TryAssemble_NullString_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("jamos", () => Hangul.TryAssemble((string)null!, Span<char>.Empty, out _));
    }

    #endregion
}
