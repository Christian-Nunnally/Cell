﻿namespace Cell.Data
{
    public class SortedListInserter<T>(Func<int, int?> getSortValue)
    {
        private readonly Func<int, int?> _getSortValue = getSortValue ?? throw new ArgumentNullException(nameof(getSortValue));
        public void InsertSorted(List<T> list, T newItem, int newItemCompareValue)
        {
            int index = BinarySearchIndex(list, newItemCompareValue);
            list.Insert(index, newItem);
        }

        private int BinarySearchIndex(List<T> list, int newItemCompareValue)
        {
            int low = 0;
            int high = list.Count - 1;

            IComparable newItemValue = newItemCompareValue;

            while (low <= high)
            {
                int mid = low + (high - low) / 2;
                var midValue = _getSortValue(mid);
                midValue ??= 0;

                int comparison = newItemValue.CompareTo(midValue);

                if (comparison == 0) return mid;
                else if (comparison < 0) high = mid - 1;
                else low = mid + 1;
            }

            return low;
        }
    }
}
