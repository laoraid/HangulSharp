using System.Diagnostics.CodeAnalysis;
using HangulSharp.Internal;

namespace HangulSharp;

public static partial class Hangul
{
    /// <summary>
    /// 겹받침, 복합모음(호환 자모)을 두 자모로 나눕니다. 쌍자음(ㄲ, ㄸ ...)과 ㅐ, ㅔ, ㅒ, ㅖ는 나누지 않습니다. (ㄳ → ㄱ, ㅅ)
    /// </summary>
    /// <param name="jamo">나눌 자모</param>
    /// <param name="first">앞 자모. 실패하면 <see langword="null"/>.</param>
    /// <param name="second">뒤 자모. 실패하면 <see langword="null"/>.</param>
    /// <returns>겹받침 또는 복합모음이면 <see langword="true"/>.</returns>
    /// <example>
    /// <code>
    /// Hangul.TrySplitCompound('ㄳ', out char? first, out char? second); // true, (ㄱ, ㅅ)
    /// Hangul.TrySplitCompound('ㅘ', out first, out second); // true, (ㅗ, ㅏ)
    /// Hangul.TrySplitCompound('ㄲ', out first, out second); // false, (null, null)
    /// </code>
    /// </example>
    public static bool TrySplitCompound(char jamo, [NotNullWhen(true)] out char? first, [NotNullWhen(true)] out char? second)
    {
        int index = JamoTable.CompoundJamo.IndexOf(jamo);
        if (index < 0)
        {
            first = null;
            second = null;
            return false;
        }

        first = JamoTable.CompoundFirst[index];
        second = JamoTable.CompoundSecond[index];
        return true;
    }

    /// <summary>두 자모(호환 자모)를 겹받침 또는 복합모음으로 합칩니다. (ㄱ, ㅅ → ㄳ)</summary>
    /// <param name="first">앞 자모</param>
    /// <param name="second">뒤 자모</param>
    /// <param name="merged">합친 자모. 실패하면 <see langword="null"/>.</param>
    /// <returns>합칠 수 있으면 <see langword="true"/>.</returns>
    /// <example>
    /// <code>
    /// Hangul.TryMergeCompound('ㄱ', 'ㅅ', out char? merged); // true, merged = 'ㄳ'
    /// Hangul.TryMergeCompound('ㅗ', 'ㅏ', out merged); // true, merged = 'ㅘ'
    /// Hangul.TryMergeCompound('ㄱ', 'ㄱ', out merged); // false, merged = null (쌍자음은 겹받침이 아님)
    /// </code>
    /// </example>
    public static bool TryMergeCompound(char first, char second, [NotNullWhen(true)] out char? merged)
    {
        for (int i = 0; i < JamoTable.CompoundJamo.Length; i++)
        {
            if (JamoTable.CompoundFirst[i] == first && JamoTable.CompoundSecond[i] == second)
            {
                merged = JamoTable.CompoundJamo[i];
                return true;
            }
        }

        merged = null;
        return false;
    }

    /// <summary>겹받침 또는 복합모음(호환 자모)이면 <see langword="true"/>.</summary>
    public static bool IsCompound(char jamo) => JamoTable.CompoundJamo.IndexOf(jamo) >= 0;
}
