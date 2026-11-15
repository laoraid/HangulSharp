namespace HangulSharp;

/// <summary>한글 음절의 초성, 중성, 종성.</summary>
/// <remarks>
/// 자모는 호환 자모(U+3131대)이거나 조합형 자모(U+1100대)입니다.
/// 어느 쪽인지는 <see cref="Hangul.TryDecompose(char, out HangulSyllable?, bool)"/>의 <c>conjoining</c> 인자로 정해집니다.
/// </remarks>
/// <param name="Choseong">초성 (ㄱ ~ ㅎ, 쌍자음 포함)</param>
/// <param name="Jungseong">중성 (겹모음은 하나의 자모, 예: ㅘ)</param>
/// <param name="Jongseong">종성 (겹받침은 하나의 자모, 예: ㅄ). 없으면 <see langword="null"/>.</param>
public readonly record struct HangulSyllable(char Choseong, char Jungseong, char? Jongseong = null)
{
    /// <summary>받침이 있으면 <see langword="true"/>.</summary>
    public bool HasJongseong => Jongseong.HasValue;

    /// <summary>다시 음절 한 글자로 조합합니다.</summary>
    public char ToChar() => Hangul.Combine(Choseong, Jungseong, Jongseong);
}
