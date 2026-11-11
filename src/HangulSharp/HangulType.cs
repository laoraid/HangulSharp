namespace HangulSharp;

/// <summary>
/// 한글 문자의 유니코드상 종류. 비트 OR로 조합해 <see cref="Hangul.IsTypeOf(char, HangulType)"/>에 넘깁니다.
/// </summary>
/// <remarks>
/// 비트 하나가 유니코드 범위 하나를 나타내므로, 여러 값을 OR로 합치면 범위를 합친 것과 같습니다.
/// 완성형 음절에는 옛한글이 없으므로 옛한글 값은 자모에만 있습니다.
/// 채움 문자(U+3164, U+115F, U+1160)는 어떤 조합으로도 한글로 보지 않습니다.
/// </remarks>
[Flags]
public enum HangulType
{
    /// <summary>아무 문자도 받아들이지 않음.</summary>
    None = 0,

    /// <summary>완성형 음절 (U+AC00 가 ~ U+D7A3 힣).</summary>
    Syllable = 1 << 0,

    /// <summary>현대 호환 자모 (U+3131 ㄱ ~ U+3163 ㅣ).</summary>
    CompatibilityJamo = 1 << 1,

    /// <summary>현대 조합형(첫가끝) 자모 (초성 U+1100~U+1112, 중성 U+1161~U+1175, 종성 U+11A8~U+11C2).</summary>
    ConjoiningJamo = 1 << 2,

    /// <summary>옛한글 호환 자모 (U+3165 ㅥ ~ U+318E ㆎ).</summary>
    ArchaicCompatibilityJamo = 1 << 3,

    /// <summary>
    /// 옛한글 조합형 자모. U+1100~U+11FF 중 현대 자모와 채움 문자를 뺀 나머지,
    /// 확장 A(U+A960~U+A97C), 확장 B(U+D7B0~U+D7C6, U+D7CB~U+D7FB).
    /// </summary>
    ArchaicConjoiningJamo = 1 << 4,

    /// <summary>현대 자모 (호환 + 조합형)</summary>
    Jamo = CompatibilityJamo | ConjoiningJamo,

    /// <summary>옛한글 자모 (호환 + 조합형).</summary>
    Archaic = ArchaicCompatibilityJamo | ArchaicConjoiningJamo,

    /// <summary>현대 한글 (음절 + 자모)</summary>
    Modern = Syllable | Jamo,

    /// <summary>모든 한글 (음절 + 현대 자모 + 옛한글 자모).</summary>
    All = Modern | Archaic,
}
