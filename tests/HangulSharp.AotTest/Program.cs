using HangulSharp;

int failures = 0;

Check("IsTypeOf", Hangul.IsTypeOf('ㆍ', HangulType.Archaic), true); // ㆍ
Check("IsHangul", Hangul.IsHangul("한글"), true);
Check("GetJamoKind", Hangul.GetJamoKind('ᆨ'), JamoKind.Consonant); // ᆨ
Check("TryDecompose", Hangul.TryDecompose('값', out HangulSyllable? syllable) ? syllable : null, new HangulSyllable('ㄱ', 'ㅏ', 'ㅄ'));
Check("Combine", Hangul.Combine('ㅎ', 'ㅏ', 'ㄴ'), '한');
Check("Disassemble", Hangul.Disassemble("값과"), "ㄱㅏㅂㅅㄱㅗㅏ");
Check("Assemble", Hangul.Assemble("ㄱㅏㅂㅅㅣ"), "갑시");
Check("TrySplitCompound", Hangul.TrySplitCompound('ㅘ', out char? first, out char? second) ? $"{first}{second}" : null, "ㅗㅏ");

Console.WriteLine(failures == 0 ? "All AOT checks passed." : $"{failures} AOT check(s) failed.");
return failures == 0 ? 0 : 1;

void Check<T>(string name, T actual, T expected)
{
    if (EqualityComparer<T>.Default.Equals(actual, expected))
    {
        return;
    }

    failures++;
    Console.WriteLine($"FAIL {name}: expected {expected}, actual {actual}");
}
