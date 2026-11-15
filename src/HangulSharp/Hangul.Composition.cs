using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using HangulSharp.Internal;

namespace HangulSharp;

public static partial class Hangul
{
    #region 음절 분해/조합

    /// <summary>음절을 초성, 중성, 종성으로 나눕니다. 겹받침, 겹모음은 하나의 자모로 둡니다. ('값' → ㄱ, ㅏ, ㅄ)</summary>
    /// <param name="syllable">나눌 완성형 음절</param>
    /// <param name="result">나눈 결과. 완성형 음절이 아니면 <see langword="null"/>.</param>
    /// <param name="conjoining">
    /// <see langword="true"/>면 조합형 자모(U+1100대)로, <see langword="false"/>면 호환 자모(U+3131대)로 나눕니다.
    /// </param>
    /// <returns>완성형 음절이면 <see langword="true"/>.</returns>
    /// <example>
    /// <code>
    /// Hangul.TryDecompose('값', out HangulSyllable? s); // true, s = (ㄱ, ㅏ, ㅄ) 호환 자모 U+3131, U+314F, U+3144
    /// Hangul.TryDecompose('값', out s, conjoining: true); // true, s = (ᄀ, ᅡ, ᆹ) 조합형 자모 U+1100, U+1161, U+11B9
    /// Hangul.TryDecompose('A', out s); // false, s = null
    /// </code>
    /// </example>
    public static bool TryDecompose(char syllable, [NotNullWhen(true)] out HangulSyllable? result, bool conjoining = false)
    {
        if (!IsSyllable(syllable))
        {
            result = null;
            return false;
        }

        int offset = syllable - JamoTable.SyllableFirst;

        int jongseongIndex = offset % JamoTable.JongseongCount;
        int jungseongIndex = offset / JamoTable.JongseongCount % JamoTable.JungseongCount;
        int choseongIndex = offset / (JamoTable.JongseongCount * JamoTable.JungseongCount);

        if (conjoining)
        {
            result = new HangulSyllable(
                (char)(JamoTable.ConjoiningChoseongFirst + choseongIndex),
                (char)(JamoTable.ConjoiningJungseongFirst + jungseongIndex),
                jongseongIndex == 0 ? null : (char)(JamoTable.ConjoiningJongseongFirst + jongseongIndex - 1)
            );
        }
        else
        {
            result = new HangulSyllable(
                JamoTable.Choseong[choseongIndex],
                JamoTable.Jungseong[jungseongIndex],
                jongseongIndex == 0 ? null : JamoTable.Jongseong[jongseongIndex]
            );
        }

        return true;
    }

    /// <summary>초성, 중성, 종성으로 음절을 만듭니다. (ㄱ, ㅏ, ㅄ → '값')</summary>
    /// <remarks>
    /// 각 자모는 현대 호환 자모(U+3131대) 또는 현대 조합형 자모(U+1100대)를 받습니다.
    /// 조합형 자모는 해당 위치의 문자여야 합니다. (예: 초성 자리에는 U+1100대 초성)
    /// </remarks>
    /// <param name="choseong">초성</param>
    /// <param name="jungseong">중성</param>
    /// <param name="jongseong">받침. 없으면 <see langword="null"/>.</param>
    /// <exception cref="ArgumentException">조합할 수 없는 자모일 때</exception>
    /// <example>
    /// <code>
    /// Hangul.Combine('ㄱ', 'ㅏ', 'ㅄ'); // '값'
    /// Hangul.Combine('ㄱ', 'ㅏ'); // '가'
    /// Hangul.Combine('ᄀ', 'ᅡ', 'ᆹ'); // '값' (조합형 자모)
    /// Hangul.Combine('ㄳ', 'ㅏ'); // ArgumentException (ㄳ은 초성이 될 수 없음)
    /// </code>
    /// </example>
    public static char Combine(char choseong, char jungseong, char? jongseong = null)
    {
        int choseongIndex = GetChoseongIndex(choseong);
        if (choseongIndex < 0)
        {
            throw new ArgumentException($"U+{(int)choseong:X4} is not a valid choseong (initial consonant).", nameof(choseong));
        }

        int jungseongIndex = GetJungseongIndex(jungseong);
        if (jungseongIndex < 0)
        {
            throw new ArgumentException($"U+{(int)jungseong:X4} is not a valid jungseong (medial vowel).", nameof(jungseong));
        }

        int jongseongIndex = jongseong is { } j ? GetJongseongIndex(j) : 0;
        if (jongseongIndex < 0)
        {
            throw new ArgumentException($"U+{(int)jongseong!.Value:X4} is not a valid jongseong (final consonant).", nameof(jongseong));
        }

        return ComposeSyllable(choseongIndex, jungseongIndex, jongseongIndex);
    }

    /// <summary>초성, 중성, 종성으로 음절을 만듭니다. 받는 자모는 <see cref="Combine(char, char, char?)"/>와 같습니다.</summary>
    /// <param name="choseong">초성</param>
    /// <param name="jungseong">중성</param>
    /// <param name="jongseong">받침. 없으면 <see langword="null"/>.</param>
    /// <param name="result">만든 음절. 실패하면 <see langword="null"/>.</param>
    /// <returns>조합할 수 있으면 <see langword="true"/>.</returns>
    /// <example>
    /// <code>
    /// Hangul.TryCombine('ㄱ', 'ㅏ', 'ㅄ', out char? c); // true, c = '값'
    /// Hangul.TryCombine('ㄱ', 'ㅏ', 'ㄸ', out c); // false, c = null (ㄸ은 받침이 될 수 없음)
    /// </code>
    /// </example>
    public static bool TryCombine(char choseong, char jungseong, char? jongseong, [NotNullWhen(true)] out char? result)
    {
        int choseongIndex = GetChoseongIndex(choseong);
        int jungseongIndex = GetJungseongIndex(jungseong);
        int jongseongIndex = jongseong is { } j ? GetJongseongIndex(j) : 0;

        if (choseongIndex < 0 || jungseongIndex < 0 || jongseongIndex < 0)
        {
            result = null;
            return false;
        }

        result = ComposeSyllable(choseongIndex, jungseongIndex, jongseongIndex);
        return true;
    }

    /// <summary>받침 없이 초성, 중성으로 음절을 만듭니다.</summary>
    /// <param name="choseong">초성</param>
    /// <param name="jungseong">중성</param>
    /// <param name="result">만든 음절. 실패하면 <see langword="null"/>.</param>
    /// <returns>조합할 수 있으면 <see langword="true"/>.</returns>
    public static bool TryCombine(char choseong, char jungseong, [NotNullWhen(true)] out char? result) => TryCombine(choseong, jungseong, null, out result);

    private static char ComposeSyllable(int choseongIndex, int jungseongIndex, int jongseongIndex) =>
        (char)(JamoTable.SyllableFirst
            + (choseongIndex * JamoTable.JungseongCount + jungseongIndex) * JamoTable.JongseongCount
            + jongseongIndex);

    // 완성형 인덱스. 쓸 수 없는 자모이면 -1.
    private static int GetChoseongIndex(char c) => c is >= JamoTable.ConjoiningChoseongFirst and <= JamoTable.ConjoiningChoseongLast
        ? c - JamoTable.ConjoiningChoseongFirst
        : JamoTable.Choseong.IndexOf(c);

    private static int GetJungseongIndex(char c) => c is >= JamoTable.ConjoiningJungseongFirst and <= JamoTable.ConjoiningJungseongLast
        ? c - JamoTable.ConjoiningJungseongFirst
        : JamoTable.Jungseong.IndexOf(c);

    // 1 ~ 27. Jongseong[0]은 받침 없음('\0')이라 0은 실패로 본다.
    private static int GetJongseongIndex(char c)
    {
        if (c is >= JamoTable.ConjoiningJongseongFirst and <= JamoTable.ConjoiningJongseongLast)
        {
            return c - JamoTable.ConjoiningJongseongFirst + 1;
        }

        int index = JamoTable.Jongseong.IndexOf(c);
        return index > 0 ? index : -1;
    }

    #endregion

    #region 문자열 분해

    /// <summary>
    /// 문자열을 호환 자모 단위로 끝까지 풉니다. 겹받침, 복합모음도 나눕니다. ("값" → "ㄱㅏㅂㅅ")
    /// </summary>
    /// <remarks>
    /// 완성형 음절과 호환 자모의 겹받침, 복합모음만 나눕니다.
    /// 그 밖의 문자(조합형 자모, 옛한글, 한글이 아닌 문자)는 그대로 둡니다. 나눌 것이 없으면 원본을 그대로 반환합니다.
    /// </remarks>
    /// <example>
    /// <code>
    /// Hangul.Disassemble("값"); // "ㄱㅏㅂㅅ"
    /// Hangul.Disassemble("과"); // "ㄱㅗㅏ"
    /// Hangul.Disassemble("까"); // "ㄲㅏ" (쌍자음은 나누지 않음)
    /// Hangul.Disassemble("한글 ABC"); // "ㅎㅏㄴㄱㅡㄹ ABC"
    /// </code>
    /// </example>
    /// <exception cref="ArgumentNullException"><paramref name="text"/>가 <see langword="null"/>일 때</exception>
    /// <exception cref="OutOfMemoryException">분해 결과가 너무 길어 문자열을 만들 수 없을 때</exception>
    public static string Disassemble(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (!NeedsDisassembly(text.AsSpan()))
        {
            return text;
        }

        // 짧은 입력은 길이를 세지 않고 최대 길이(5배)로 버퍼를 잡는다.
        // 긴 입력은 5배 버퍼가 실제 결과보다 훨씬 커질 수 있으므로 정확한 길이를 센다.
        int bufferLength = text.Length <= ExactLengthThreshold
            ? GetMaxDisassembledLength(text.Length)
            : GetDisassembledLength(text.AsSpan());

        char[]? rented = null;
        Span<char> buffer = bufferLength <= StackallocThreshold
            ? stackalloc char[bufferLength]
            : (rented = ArrayPool<char>.Shared.Rent(bufferLength));

        try
        {
            TryDisassemble(text.AsSpan(), buffer, out int written);
            return buffer[..written].ToString();
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<char>.Shared.Return(rented);
            }
        }
    }

    /// <summary>할당 없이 <paramref name="destination"/>에 분해 결과를 씁니다. 분해 규칙은 <see cref="Disassemble(string)"/>와 같습니다.</summary>
    /// <param name="text">분해할 문자열</param>
    /// <param name="destination">결과를 쓸 버퍼. 크기는 <see cref="GetMaxDisassembledLength(int)"/>로 계산합니다.</param>
    /// <param name="charsWritten">쓴 문자 수. 실패하면 0.</param>
    /// <returns>버퍼가 충분하면 <see langword="true"/>.</returns>
    public static bool TryDisassemble(ReadOnlySpan<char> text, Span<char> destination, out int charsWritten)
    {
        int written = 0;

        foreach (char c in text)
        {
            if (TryDecompose(c, out HangulSyllable? syllable))
            {
                HangulSyllable s = syllable.Value;
                if (!TryWriteJamo(s.Choseong, destination, ref written)
                    || !TryWriteJamo(s.Jungseong, destination, ref written)
                    || (s.Jongseong is { } jongseong && !TryWriteJamo(jongseong, destination, ref written)))
                {
                    charsWritten = 0;
                    return false;
                }
            }
            else if (!TryWriteJamo(c, destination, ref written))
            {
                charsWritten = 0;
                return false;
            }
        }

        charsWritten = written;
        return true;
    }

    // 겹받침, 복합모음이면 두 자모로 나눠 쓰고, 아니면 그대로 쓴다.
    private static bool TryWriteJamo(char jamo, Span<char> destination, ref int written)
    {
        if (TrySplitCompound(jamo, out char? first, out char? second))
        {
            if (written + 2 > destination.Length)
            {
                return false;
            }

            destination[written++] = first.Value;
            destination[written++] = second.Value;
            return true;
        }

        if (written + 1 > destination.Length)
        {
            return false;
        }

        destination[written++] = jamo;
        return true;
    }

    private static bool NeedsDisassembly(ReadOnlySpan<char> text)
    {
        foreach (char c in text)
        {
            if (IsSyllable(c) || IsCompound(c))
            {
                return true;
            }
        }

        return false;
    }

    // 분해 결과의 정확한 길이. TryDisassemble과 같은 규칙으로 센다.
    private static int GetDisassembledLength(ReadOnlySpan<char> text)
    {
        long length = 0;

        foreach (char c in text)
        {
            if (TryDecompose(c, out HangulSyllable? syllable))
            {
                HangulSyllable s = syllable.Value;
                length += GetJamoLength(s.Choseong) + GetJamoLength(s.Jungseong);
                if (s.Jongseong is { } jongseong)
                {
                    length += GetJamoLength(jongseong);
                }
            }
            else
            {
                length += GetJamoLength(c);
            }
        }

        if (length > int.MaxValue)
        {
            throw new OutOfMemoryException("The disassembled text exceeds the maximum string length.");
        }

        return (int)length;
    }

    private static int GetJamoLength(char jamo) => IsCompound(jamo) ? 2 : 1;

    /// <inheritdoc cref="TryDisassemble(ReadOnlySpan{char}, Span{char}, out int)"/>
    /// <exception cref="ArgumentNullException"><paramref name="text"/>가 <see langword="null"/>일 때</exception>
    public static bool TryDisassemble(string text, Span<char> destination, out int charsWritten)
    {
        ArgumentNullException.ThrowIfNull(text);
        return TryDisassemble(text.AsSpan(), destination, out charsWritten);
    }

    /// <summary><see cref="TryDisassemble(ReadOnlySpan{char}, Span{char}, out int)"/>에 필요한 최대 버퍼 길이.</summary>
    /// <remarks>글자 하나는 최대 5자모로 풀립니다. (예: 'ㄱ' + 'ㅘ' + 'ㄳ' → ㄱㅗㅏㄱㅅ)</remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="textLength"/>가 음수이거나, 결과가 <see cref="int.MaxValue"/>를 넘을 만큼 클 때 (<c>int.MaxValue / 5</c> 초과)
    /// </exception>
    public static int GetMaxDisassembledLength(int textLength)
    {
        if (textLength is < 0 or > MaxDisassemblyTextLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(textLength),
                textLength,
                $"The text length must be between 0 and {MaxDisassemblyTextLength}.");
        }

        return textLength * MaxJamoPerChar;
    }

    private const int MaxJamoPerChar = 5;
    private const int MaxDisassemblyTextLength = int.MaxValue / MaxJamoPerChar;
    private const int ExactLengthThreshold = 1 << 20;
    private const int StackallocThreshold = 256;

    #endregion

    #region 문자열 조합

    /// <summary>
    /// 자모 나열을 두벌식 규칙으로 음절로 조립합니다. 겹받침, 복합모음 결합과 연음(도깨비불)을 처리합니다. ("ㄱㅏㅂㅅㅣ" → "갑시")
    /// </summary>
    /// <remarks>
    /// 현대 호환 자모(U+3131 ~ U+3163)만 조립합니다. 그 밖의 문자는 조립을 끊고 그대로 둡니다.
    /// 음절이 되지 못한 자모(모음만, 자음만)는 자모 그대로 남습니다. 조립할 것이 없으면 원본을 그대로 반환합니다.
    /// 두벌식 입력기처럼 모음 없는 자음끼리도 겹자음으로 합치고, 모음이 오면 뒤 자모를 다음 글자로 넘깁니다.
    /// </remarks>
    /// <example>
    /// <code>
    /// Hangul.Assemble("ㄱㅏㅂㅅ"); // "값"
    /// Hangul.Assemble("ㄱㅏㅂㅅㅣ"); // "갑시"
    /// Hangul.Assemble("ㄱㅗㅏ"); // "과"
    /// Hangul.Assemble("ㅋㅋ"); // "ㅋㅋ"
    /// Hangul.Assemble("ㄱㅅ"); // "ㄳ"
    /// Hangul.Assemble("ㄱㅅㅏ"); // "ㄱ사"
    /// </code>
    /// </example>
    /// <exception cref="ArgumentNullException"><paramref name="jamos"/>가 <see langword="null"/>일 때</exception>
    public static string Assemble(string jamos)
    {
        ArgumentNullException.ThrowIfNull(jamos);

        if (!NeedsAssembly(jamos.AsSpan()))
        {
            return jamos;
        }

        char[]? rented = null;
        Span<char> buffer = jamos.Length <= StackallocThreshold
            ? stackalloc char[jamos.Length]
            : (rented = ArrayPool<char>.Shared.Rent(jamos.Length));

        try
        {
            TryAssemble(jamos.AsSpan(), buffer, out int written);
            return buffer[..written].ToString();
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<char>.Shared.Return(rented);
            }
        }
    }

    /// <summary>할당 없이 <paramref name="destination"/>에 조립 결과를 씁니다. 조립 규칙은 <see cref="Assemble(string)"/>와 같습니다.</summary>
    /// <param name="jamos">조립할 자모 나열</param>
    /// <param name="destination">결과를 쓸 버퍼. <paramref name="jamos"/>와 같은 길이면 항상 충분합니다.</param>
    /// <param name="charsWritten">쓴 문자 수. 실패하면 0.</param>
    /// <returns>버퍼가 충분하면 <see langword="true"/>.</returns>
    public static bool TryAssemble(ReadOnlySpan<char> jamos, Span<char> destination, out int charsWritten)
    {
        var assembler = new JamoAssembler();
        Span<char> committed = stackalloc char[JamoAssembler.MaxCommitted];
        int written = 0;

        foreach (char c in jamos)
        {
            int count;
            if (JamoAssembler.CanPush(c))
            {
                count = assembler.Push(c, committed);
            }
            else
            {
                count = assembler.Flush(committed);
                committed[count++] = c;
            }

            if (!TryCopy(committed[..count], destination, ref written))
            {
                charsWritten = 0;
                return false;
            }
        }

        if (!TryCopy(committed[..assembler.Flush(committed)], destination, ref written))
        {
            charsWritten = 0;
            return false;
        }

        charsWritten = written;
        return true;
    }

    /// <inheritdoc cref="TryAssemble(ReadOnlySpan{char}, Span{char}, out int)"/>
    /// <exception cref="ArgumentNullException"><paramref name="jamos"/>가 <see langword="null"/>일 때</exception>
    public static bool TryAssemble(string jamos, Span<char> destination, out int charsWritten)
    {
        ArgumentNullException.ThrowIfNull(jamos);
        return TryAssemble(jamos.AsSpan(), destination, out charsWritten);
    }

    private static bool TryCopy(ReadOnlySpan<char> source, Span<char> destination, ref int written)
    {
        if (written + source.Length > destination.Length)
        {
            return false;
        }

        source.CopyTo(destination[written..]);
        written += source.Length;
        return true;
    }

    // 현대 호환 자모가 두 개 이상 있어야 조립될 여지가 있다.
    private static bool NeedsAssembly(ReadOnlySpan<char> text)
    {
        int jamoCount = 0;
        foreach (char c in text)
        {
            if (JamoAssembler.CanPush(c) && ++jamoCount >= 2)
            {
                return true;
            }
        }

        return false;
    }

    #endregion
}
