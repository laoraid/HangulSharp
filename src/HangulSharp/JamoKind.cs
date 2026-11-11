namespace HangulSharp;

/// <summary>자모의 음운상 종류 (자음/모음).</summary>
public enum JamoKind
{
    /// <summary>자모가 아님. (완성형 음절, 한글이 아닌 문자, 채움 문자 포함)</summary>
    None = 0,

    /// <summary>자음 자모. (ㄱ, ㄳ, ᄀ, ᇫ ...)</summary>
    Consonant,

    /// <summary>모음 자모. (ㅏ, ㅘ, ᅡ, ㆍ ...)</summary>
    Vowel,
}

/// <summary>음절 안에서 자모의 위치. 조합형 자모 변환에 씁니다.</summary>
public enum JamoPosition
{
    /// <summary>초성.</summary>
    Choseong,

    /// <summary>중성.</summary>
    Jungseong,

    /// <summary>종성.</summary>
    Jongseong,
}
