using HangulSharp.Internal;

namespace HangulSharp;

public static partial class Hangul
{
    #region 종류 판별

    /// <summary><paramref name="c"/>가 <paramref name="type"/>에 포함된 종류의 한글이면 <see langword="true"/>.</summary>
    /// <example>
    /// <code>
    /// Hangul.IsTypeOf('가', HangulType.Syllable); // true
    /// Hangul.IsTypeOf('ㄱ', HangulType.Syllable | HangulType.CompatibilityJamo); // true
    /// Hangul.IsTypeOf('ㄱ', HangulType.ConjoiningJamo); // false
    /// Hangul.IsTypeOf('ㆍ', HangulType.Modern); // false
    /// Hangul.IsTypeOf('ㆍ', HangulType.Archaic); // true
    /// Hangul.IsTypeOf('ㆍ', HangulType.CompatibilityJamo | HangulType.ArchaicCompatibilityJamo); // true
    /// </code>
    /// </example>
    public static bool IsTypeOf(char c, HangulType type) => (GetTypeBit(c) & type) != 0;

    // c가 속한 HangulType 비트 하나를 반환한다. 한글이 아니면 None.
    private static HangulType GetTypeBit(char c) => c switch
    {
        >= JamoTable.SyllableFirst and <= JamoTable.SyllableLast => HangulType.Syllable,

        >= JamoTable.CompatFirst and <= JamoTable.CompatModernLast => HangulType.CompatibilityJamo,
        >= JamoTable.CompatArchaicFirst and <= JamoTable.CompatLast => HangulType.ArchaicCompatibilityJamo,

        >= JamoTable.ConjoiningChoseongFirst and <= JamoTable.ConjoiningChoseongLast
            or >= JamoTable.ConjoiningJungseongFirst and <= JamoTable.ConjoiningJungseongLast
            or >= JamoTable.ConjoiningJongseongFirst and <= JamoTable.ConjoiningJongseongLast => HangulType.ConjoiningJamo,

        >= JamoTable.ArchaicChoseongFirst and <= JamoTable.ArchaicChoseongLast
            or >= JamoTable.ArchaicJungseongFirst and <= JamoTable.ArchaicJungseongLast
            or >= JamoTable.ArchaicJongseongFirst and <= JamoTable.ArchaicJongseongLast
            or >= JamoTable.ExtAChoseongFirst and <= JamoTable.ExtAChoseongLast
            or >= JamoTable.ExtBJungseongFirst and <= JamoTable.ExtBJungseongLast
            or >= JamoTable.ExtBJongseongFirst and <= JamoTable.ExtBJongseongLast => HangulType.ArchaicConjoiningJamo,

        _ => HangulType.None,
    };

    /// <summary>
    /// 문자열의 모든 문자가 <paramref name="type"/>에 포함된 종류의 한글이면 <see langword="true"/>.
    /// 빈 문자열은 <see langword="false"/>.
    /// </summary>
    public static bool IsTypeOf(ReadOnlySpan<char> text, HangulType type)
    {
        if (text.IsEmpty)
        {
            return false;
        }

        foreach (char c in text)
        {
            if (!IsTypeOf(c, type))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc cref="IsTypeOf(ReadOnlySpan{char}, HangulType)"/>
    /// <exception cref="ArgumentNullException"><paramref name="text"/>가 <see langword="null"/>일 때</exception>
    public static bool IsTypeOf(string text, HangulType type)
    {
        ArgumentNullException.ThrowIfNull(text);
        return IsTypeOf(text.AsSpan(), type);
    }

    #endregion

    /// <summary>현대 한글(완성형 음절 또는 호환, 조합형 자모)이면 <see langword="true"/>. (<see cref="HangulType.Modern"/>)</summary>
    public static bool IsHangul(char c) => IsTypeOf(c, HangulType.Modern);

    /// <summary>문자열의 모든 문자가 현대 한글이면 <see langword="true"/>. 빈 문자열은 <see langword="false"/>.</summary>
    public static bool IsHangul(ReadOnlySpan<char> text) => IsTypeOf(text, HangulType.Modern);

    /// <inheritdoc cref="IsHangul(ReadOnlySpan{char})"/>
    /// <exception cref="ArgumentNullException"><paramref name="text"/>가 <see langword="null"/>일 때</exception>
    public static bool IsHangul(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return IsHangul(text.AsSpan());
    }

    /// <summary>완성형 한글 음절(U+AC00 '가' ~ U+D7A3 '힣')이면 <see langword="true"/>. (<see cref="HangulType.Syllable"/>)</summary>
    public static bool IsSyllable(char c) => c is >= JamoTable.SyllableFirst and <= JamoTable.SyllableLast;

    /// <summary>현대 한글의 호환 자모 또는 조합형 자모이면 <see langword="true"/>. (<see cref="HangulType.Jamo"/>)</summary>
    public static bool IsJamo(char c) => IsTypeOf(c, HangulType.Jamo);

    #region 자모 세부 판별

    /// <summary>자음 자모이면 <see langword="true"/>. 호환, 조합형, 현대, 옛한글 모두 포함. (ㄱ, ㄳ, ᄀ, ᆨ, ㅿ...)</summary>
    public static bool IsConsonant(char c) => GetJamoKind(c) == JamoKind.Consonant;

    /// <summary>모음 자모이면 <see langword="true"/>. 호환, 조합형, 현대, 옛한글 모두 포함. (ㅏ, ㅘ, ᅡ, ㆍ...)</summary>
    public static bool IsVowel(char c) => GetJamoKind(c) == JamoKind.Vowel;

    /// <summary>
    /// 초성으로 쓸 수 있는 자모이면 <see langword="true"/>.
    /// 호환 자모는 현대 초성 19자(ㄳ 같은 겹받침 제외), 조합형 자모는 초성 문자(옛한글 포함)만 해당합니다.
    /// </summary>
    public static bool IsChoseong(char c) => JamoTable.Choseong.IndexOf(c) >= 0 || IsConjoiningChoseong(c);

    /// <summary>
    /// 중성으로 쓸 수 있는 자모이면 <see langword="true"/>.
    /// 호환 자모는 현대 중성 21자, 조합형 자모는 중성 문자(옛한글 포함)만 해당합니다.
    /// </summary>
    public static bool IsJungseong(char c) => JamoTable.Jungseong.IndexOf(c) >= 0 || IsConjoiningJungseong(c);

    /// <summary>
    /// 종성(받침)으로 쓸 수 있는 자모이면 <see langword="true"/>.
    /// 호환 자모는 현대 종성 27자(ㄸ, ㅃ, ㅉ 제외), 조합형 자모는 종성 문자(옛한글 포함)만 해당합니다.
    /// </summary>
    // [0]은 "받침 없음"('\0')이므로 인덱스 0은 제외한다.
    public static bool IsJongseong(char c) => JamoTable.Jongseong.IndexOf(c) > 0 || IsConjoiningJongseong(c);

    /// <summary>
    /// 자모의 음운상 종류(자음/모음)를 반환합니다. 호환, 조합형, 현대, 옛한글 모두 판별합니다.
    /// 자모가 아니면(완성형 음절, 채움 문자 포함) <see cref="JamoKind.None"/>.
    /// </summary>
    public static JamoKind GetJamoKind(char c)
    {
        if (c is >= JamoTable.CompatFirst and <= JamoTable.CompatConsonantLast
            or >= JamoTable.CompatArchaicFirst and <= JamoTable.CompatArchaicConsonantLast)
        {
            return JamoKind.Consonant;
        }

        if (c is >= JamoTable.CompatVowelFirst and <= JamoTable.CompatModernLast
            or >= JamoTable.CompatArchaicVowelFirst and <= JamoTable.CompatLast)
        {
            return JamoKind.Vowel;
        }

        if (IsConjoiningChoseong(c) || IsConjoiningJongseong(c))
        {
            return JamoKind.Consonant;
        }

        if (IsConjoiningJungseong(c))
        {
            return JamoKind.Vowel;
        }

        return JamoKind.None;
    }

    #endregion

    #region 조합형 자모 위치

    private static bool IsConjoiningChoseong(char c) =>
        c is >= JamoTable.ConjoiningChoseongFirst and <= JamoTable.ConjoiningChoseongLast
            or >= JamoTable.ArchaicChoseongFirst and <= JamoTable.ArchaicChoseongLast
            or >= JamoTable.ExtAChoseongFirst and <= JamoTable.ExtAChoseongLast;

    private static bool IsConjoiningJungseong(char c) =>
        c is >= JamoTable.ConjoiningJungseongFirst and <= JamoTable.ConjoiningJungseongLast
            or >= JamoTable.ArchaicJungseongFirst and <= JamoTable.ArchaicJungseongLast
            or >= JamoTable.ExtBJungseongFirst and <= JamoTable.ExtBJungseongLast;

    private static bool IsConjoiningJongseong(char c) =>
        c is >= JamoTable.ConjoiningJongseongFirst and <= JamoTable.ConjoiningJongseongLast
            or >= JamoTable.ArchaicJongseongFirst and <= JamoTable.ArchaicJongseongLast
            or >= JamoTable.ExtBJongseongFirst and <= JamoTable.ExtBJongseongLast;

    #endregion
}
