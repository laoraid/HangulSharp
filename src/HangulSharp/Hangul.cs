namespace HangulSharp;

/// <summary>
/// 한글 자모 판별, 분해, 조합, 변환 기능의 진입점입니다.
/// </summary>
public static partial class Hangul
{
    // ── 오버로드 규칙 ──────────────────────────────────────
    // 1. 실제 로직은 ReadOnlySpan<char> 버전에만 둔다. (할당 없음, null 없음)
    // 2. Span을 받는 public API에는 string 오버로드를 함께 둔다.
    //    .NET Framework + C# 7.3 사용자는 string → ReadOnlySpan<char> 암시적 변환이 없기 때문.
    // 3. string 버전은 null 검사(ArgumentNullException) 후 text.AsSpan()으로 넘기기만 한다.
    // 4. 결과가 string인 API는 string 버전이 버퍼 준비(stackalloc / ArrayPool)와
    //    문자열 생성, "변경 없으면 원본 반환" 같은 string 전용 최적화를 맡는다.
}
