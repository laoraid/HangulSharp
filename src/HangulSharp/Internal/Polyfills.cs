// .NET Framework / netstandard2.0 에 없는 BCL API를 C# 14 static extension member로 채웁니다.
// net8.0 빌드에서는 컴파일되지 않고 런타임 내장 API가 그대로 쓰입니다.
#if !NET

using System.Runtime.CompilerServices;

// extension이 using 없이 보이도록 루트 네임스페이스에 둡니다.
namespace HangulSharp;

internal static class Polyfills
{
    extension(ArgumentNullException)
    {
        public static void ThrowIfNull(
            object? argument,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}

#endif
