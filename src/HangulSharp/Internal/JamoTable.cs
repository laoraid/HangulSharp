namespace HangulSharp.Internal;

/// <summary>유니코드 범위, 자모 순서표, 겹자모 매핑 등 공용 상수</summary>
internal static class JamoTable
{
    // 완성형 음절: 0xAC00 + (초성 * 21 + 중성) * 28 + 종성
    public const char SyllableFirst = '가'; // U+AC00
    public const char SyllableLast = '힣'; // U+D7A3
    public const int ChoseongCount = 19;
    public const int JungseongCount = 21;
    public const int JongseongCount = 28; // 받침 없음 포함

    // 호환 자모 — 현대 (U+3164 채움 문자는 제외)
    public const char CompatFirst = 'ㄱ'; // U+3131 (자음 시작)
    public const char CompatConsonantLast = 'ㅎ'; // U+314E
    public const char CompatVowelFirst = 'ㅏ'; // U+314F
    public const char CompatModernLast = 'ㅣ'; // U+3163 (모음 끝)

    // 호환 자모 — 옛한글
    public const char CompatArchaicFirst = 'ㅥ'; // U+3165 (자음 시작)
    public const char CompatArchaicConsonantLast = 'ㆆ'; // U+3186
    public const char CompatArchaicVowelFirst = 'ㆇ'; // U+3187
    public const char CompatLast = 'ㆎ'; // U+318E (모음 끝)

    // 조합형 자모 — 현대 (순서가 완성형 인덱스와 같음)
    public const char ConjoiningChoseongFirst = 'ᄀ'; // U+1100
    public const char ConjoiningChoseongLast = 'ᄒ'; // U+1112
    public const char ConjoiningJungseongFirst = 'ᅡ'; // U+1161
    public const char ConjoiningJungseongLast = 'ᅵ'; // U+1175
    public const char ConjoiningJongseongFirst = 'ᆨ'; // U+11A8
    public const char ConjoiningJongseongLast = 'ᇂ'; // U+11C2 (받침 없음 제외 27개)

    // 조합형 자모 — 옛한글 (U+115F 초성 채움, U+1160 중성 채움은 제외)
    public const char ArchaicChoseongFirst = 'ᄓ'; // U+1113
    public const char ArchaicChoseongLast = 'ᅞ'; // U+115E
    public const char ArchaicJungseongFirst = 'ᅶ'; // U+1176
    public const char ArchaicJungseongLast = 'ᆧ'; // U+11A7
    public const char ArchaicJongseongFirst = 'ᇃ'; // U+11C3
    public const char ArchaicJongseongLast = 'ᇿ'; // U+11FF
    public const char ExtAChoseongFirst = 'ꥠ'; // U+A960 (한글 자모 확장 A)
    public const char ExtAChoseongLast = 'ꥼ'; // U+A97C
    public const char ExtBJungseongFirst = 'ힰ'; // U+D7B0 (한글 자모 확장 B)
    public const char ExtBJungseongLast = 'ퟆ'; // U+D7C6
    public const char ExtBJongseongFirst = 'ퟋ'; // U+D7CB
    public const char ExtBJongseongLast = 'ퟻ'; // U+D7FB

    // 완성형 인덱스 순서의 호환 자모
    public const string Choseong = "ㄱㄲㄴㄷㄸㄹㅁㅂㅃㅅㅆㅇㅈㅉㅊㅋㅌㅍㅎ";
    public const string Jungseong = "ㅏㅐㅑㅒㅓㅔㅕㅖㅗㅘㅙㅚㅛㅜㅝㅞㅟㅠㅡㅢㅣ";
    public const string Jongseong = "\0ㄱㄲㄳㄴㄵㄶㄷㄹㄺㄻㄼㄽㄾㄿㅀㅁㅂㅄㅅㅆㅇㅈㅊㅋㅌㅍㅎ"; // [0] = 받침 없음

    // TODO: 겹받침, 겹모음 매핑 (ㄳ ↔ ㄱㅅ, ㅘ ↔ ㅗㅏ ...)
}
