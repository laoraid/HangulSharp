namespace HangulSharp.Internal;

internal struct JamoAssembler
{
    public const int MaxCommitted = 2;

    // null은 비어 있음. 모음 없이 자음만 있을 때 _choseong에 겹자음(ㄳ 등)이 들어갈 수 있다.
    private char? _choseong;
    private char? _jungseong;
    private char? _jongseong;

    /// <summary><see cref="Push(char, Span{char})"/>에 넣을 수 있는 자모(현대 호환 자모)이면 <see langword="true"/>.</summary>
    public static bool CanPush(char c) => c is >= JamoTable.CompatFirst and <= JamoTable.CompatModernLast;

    /// <summary>
    /// 자모 하나를 입력합니다. 완성되어 밀려난 글자를 <paramref name="committed"/>에 쓰고 그 개수(0 ~ 1)를 반환합니다.
    /// </summary>
    /// <param name="jamo">현대 호환 자모. <see cref="CanPush(char)"/>로 확인합니다.</param>
    /// <param name="committed">길이가 <see cref="MaxCommitted"/> 이상인 버퍼</param>
    public int Push(char jamo, Span<char> committed)
    {
        return jamo >= JamoTable.CompatVowelFirst
            ? PushVowel(jamo, committed)
            : PushConsonant(jamo, committed);
    }

    /// <summary>조합 중인 글자를 마무리해서 <paramref name="committed"/>에 쓰고 그 개수(0 ~ 1)를 반환합니다.</summary>
    public int Flush(Span<char> committed)
    {
        int count = 0;
        if (_choseong is not null || _jungseong is not null)
        {
            committed[count++] = Compose();
        }

        Clear();
        return count;
    }

    private int PushConsonant(char consonant, Span<char> committed)
    {
        if (_jungseong is null)
        {
            // 자음만 있음: 겹자음으로 합칠 수 있으면 합친다.
            if (_choseong is { } choseong && Hangul.TryMergeCompound(choseong, consonant, out char? mergedConsonant))
            {
                _choseong = mergedConsonant;
                return 0;
            }
        }
        else if (_choseong is not null)
        {
            // 초성+중성: 받침 자리에 넣거나 기존 받침과 겹받침으로 합친다.
            if (_jongseong is not { } jongseong)
            {
                if (Hangul.IsJongseong(consonant))
                {
                    _jongseong = consonant;
                    return 0;
                }
            }
            else if (Hangul.TryMergeCompound(jongseong, consonant, out char? mergedJongseong))
            {
                _jongseong = mergedJongseong;
                return 0;
            }
        }

        // 지금 글자를 내보내고 이 자음으로 새 글자를 시작한다.
        int count = Flush(committed);
        _choseong = consonant;
        return count;
    }

    private int PushVowel(char vowel, Span<char> committed)
    {
        if (_jungseong is not { } jungseong)
        {
            // 빈 상태이거나 자음만 있음. 자음이 겹자음이면 앞 자모는 내보내고 뒤 자모를 초성으로 쓴다.
            if (_choseong is { } choseong && Hangul.TrySplitCompound(choseong, out char? first, out char? second))
            {
                committed[0] = first.Value;
                _choseong = second;
                _jungseong = vowel;
                return 1;
            }

            _jungseong = vowel;
            return 0;
        }

        if (_jongseong is not { } jongseong)
        {
            if (Hangul.TryMergeCompound(jungseong, vowel, out char? merged))
            {
                _jungseong = merged;
                return 0;
            }

            int count = Flush(committed);
            _jungseong = vowel;
            return count;
        }

        // 도깨비불: 마지막 받침을 다음 글자의 초성으로 넘긴다. 겹받침이면 뒤 자모만 넘긴다.
        char next;
        if (Hangul.TrySplitCompound(jongseong, out char? remain, out char? moved))
        {
            _jongseong = remain;
            next = moved.Value;
        }
        else
        {
            next = jongseong;
            _jongseong = null;
        }

        int flushed = Flush(committed);
        _choseong = next;
        _jungseong = vowel;
        return flushed;
    }

    // 초성+중성이 있으면 음절, 아니면 남은 자모 하나 (자음만 있으면 겹자음일 수 있음)
    private readonly char Compose()
    {
        if (_choseong is { } choseong && _jungseong is { } jungseong)
        {
            return Hangul.Combine(choseong, jungseong, _jongseong);
        }

        return _choseong ?? _jungseong!.Value;
    }

    private void Clear()
    {
        _choseong = null;
        _jungseong = null;
        _jongseong = null;
    }
}
