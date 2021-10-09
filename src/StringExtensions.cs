////////////////////////////////////////////////////////////////////////
//
// This file is part of gmic-sharp, a .NET wrapper for G'MIC.
//
// Copyright (c) 2020, 2021 Nicholas Hayes
//
// This file is licensed under the MIT License.
// See LICENSE.txt for complete licensing and attribution information.
//
////////////////////////////////////////////////////////////////////////

using System;

namespace GmicSharp
{
    internal static class StringExtensions
    {
        internal static bool Contains(this string item, string value, StringComparison comparisonType)
        {
            if (item is null)
            {
                ExceptionUtil.ThrowArgumentNullException(nameof(item));
            }

            return item.IndexOf(value, comparisonType) >= 0;
        }

        internal static bool IsEmptyOrWhiteSpace(this string value)
        {
            if (value is null)
            {
                ExceptionUtil.ThrowArgumentNullException(nameof(value));
            }

            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                {
                    return false;
                }
            }

            return true;
        }

    }
}
