using System;
using System.Collections.Generic;

namespace Nextension
{
    public static class SortUtil
    {
        public static void quickSort<T>(Span<T> span)
        {
            if (span.Length <= 1) return;
            quickSort(span, 0, span.Length - 1);
        }

        public static void quickSort<T>(Span<T> span, Comparison<T> comparison)
        {
            if (span.Length <= 1) return;
            quickSort(span, 0, span.Length - 1, comparison);
        }

        public static void quickSort<T>(Span<T> span, int left, int right)
        {
            if (left >= right) return;

            T pivot = span[(left + right) / 2];
            int i = left;
            int j = right;

            while (i <= j)
            {
                while (Comparer<T>.Default.Compare(span[i], pivot) < 0) i++;
                while (Comparer<T>.Default.Compare(span[j], pivot) > 0) j--;

                if (i <= j)
                {
                    (span[j], span[i]) = (span[i], span[j]);
                    i++;
                    j--;
                }
            }

            if (left < j) quickSort(span, left, j);
            if (i < right) quickSort(span, i, right);
        }

        public static void quickSort<T>(Span<T> span, int left, int right, Comparison<T> comparison)
        {
            if (left >= right) return;

            T pivot = span[(left + right) / 2];
            int i = left;
            int j = right;

            while (i <= j)
            {
                while (comparison(span[i], pivot) < 0) i++;
                while (comparison(span[j], pivot) > 0) j--;

                if (i <= j)
                {
                    (span[j], span[i]) = (span[i], span[j]);
                    i++;
                    j--;
                }
            }

            if (left < j) quickSort(span, left, j, comparison);
            if (i < right) quickSort(span, i, right, comparison);
        }
    }
}
