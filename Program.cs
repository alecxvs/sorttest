using System;
using System.Collections.Generic;
using System.Linq;

namespace sorttest
{
    class Program
    {
        const int sizeArray = 1000;
        static void Main(string[] args)
        {
            Console.WriteLine("Sorting Algorithm Comparison");
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            List<int> sortedArray = new List<int>(sizeArray);
            List<int> reversedArray = new List<int>(sizeArray);
            List<int> randomArray = new List<int>(sizeArray);
            for (int i = 0; i < sizeArray; i++)
            {
                sortedArray.Add(i + 1);
                randomArray.Add(i + 1);
                reversedArray.Add(sizeArray - i);
            }
            Shuffle(randomArray);

            var sorters = new Dictionary<string, Sorter<int>> {
                {"Insertion Sort", new InsertionSort()},
                {"Merge Sort", new MergeSort()},
                {"Quick Sort", new QuickSort()},
                {"Heap Sort", new HeapSort()},
                {"Counting Sort", new CountingSort(k: sizeArray)},
                {"Radix Sort", new RadixSort()},
            };

            var datasets = new Dictionary<string, List<int>> {
                {"Sorted", sortedArray},
                {"Reversed", reversedArray},
                {"Random", randomArray}
            };

            foreach ((string sorterName, Sorter<int> sorter) in sorters)
            {
                foreach ((string dataName, List<int> dataset) in datasets)
                {
                    if (sorter.Sort(dataset).SequenceEqual(sortedArray))
                        Console.WriteLine($"[{sorterName} - {dataName}] {sorter.SortingStats()}");
                    else
                        Console.WriteLine($"{sorterName} failed to sort list {dataName} correctly ({sorter.SortingStats()}");
                }
            }
        }

        static void Shuffle(List<int> input)
        {
            int n = input.Count;
            Random rng = new Random();
            while (n > 1)
            {
                int k = rng.Next(n--);
                int temp = input[n];
                input[n] = input[k];
                input[k] = temp;
            }
        }
    }

    abstract class Sorter<T>
    {
        public int Comparisons { get; protected set; } = 0;
        public int Iterations { get; protected set; } = 0;
        public TimeSpan Duration { get; protected set; }
        abstract public IList<T> Sort(IList<T> input);
        public String SortingStats()
        {
            return $"Statistics for last sort: Duration {Duration} / {Iterations} Iterations / {Comparisons} Comparisons";
        }

        protected void swap(Span<int> span, int a, int b)
        {
            var tmp = span[a];
            span[a] = span[b];
            span[b] = tmp;
        }
    }

    class InsertionSort : Sorter<int>
    {
        override public IList<int> Sort(IList<int> input)
        {
            var startTime = DateTime.Now;
            this.Comparisons = 0;
            this.Iterations = 0;

            var output = new List<int>(input);
            var length = output.Count;

            for (int i = 1; i < length; i++)
            {
                var current = output[i];
                var lookback = 0;
                this.Iterations++;
                this.Comparisons++;
                while (++lookback <= i && output[i - lookback] > current)
                {
                    this.Iterations++;
                    this.Comparisons++;
                    output[(i - lookback) + 1] = output[i - lookback];
                }
                output[i - (lookback - 1)] = current;
            }

            Duration = DateTime.Now.Subtract(startTime);
            return output;
        }
    }

    class QuickSort : Sorter<int>
    {
        override public IList<int> Sort(IList<int> input)
        {
            var startTime = DateTime.Now;
            this.Comparisons = 0;
            this.Iterations = 0;

            var output = input.ToArray();
            quickSort(output.AsSpan());

            Duration = DateTime.Now.Subtract(startTime);
            return output;
        }

        private int findMedian(Span<int> span)
        {
            var rng = new Random();
            var third = span.Length / 3;
            var leftIdx = rng.Next(0, third + 1);
            var midIdx = rng.Next((span.Length / 2) - (third / 2), (span.Length / 2) + (third / 2) + 1);
            var rightIdx = rng.Next(span.Length - third, span.Length);

            var isorter = new InsertionSort();
            var three = new int[] { span[leftIdx], span[midIdx], span[rightIdx] };
            isorter.Sort(three);
            if (three[1] == span[leftIdx]) return leftIdx;
            else if (three[1] == span[midIdx]) return midIdx;
            else return rightIdx;
        }

        private void quickSort(Span<int> span)
        {
            if (span.Length <= 1)
            {
                this.Iterations++;
                return;
            }
            else if (span.Length == 2)
            {
                this.Iterations++;
                this.Comparisons++;
                if (span[0] > span[1])
                    swap(span, 0, 1);
                return;
            }
            var sLength = span.Length - 1;
            var pivot = findMedian(span);
            var pivotValue = span[pivot];
            span[pivot] = span[span.Length - 1];
            span[span.Length - 1] = pivotValue;

            // Console.Write($"[Quick Sort - Pivot {pivotValue} @ {half}] In: {String.Join(",",span.ToArray())}");

            var midpoint = 0;

            int i = 0;
            for (; i < sLength && span[i] <= pivotValue; i++)
            {
                this.Iterations++;
                this.Comparisons++;
                midpoint++;
            }
            for (; i < sLength; i++)
            {
                this.Iterations++;
                this.Comparisons++;
                if (span[i] <= pivotValue)
                {
                    swap(span, midpoint++, i);
                }
            }
            // Console.WriteLine($"Out: {String.Join(",",span.ToArray())}");
            swap(span, midpoint, span.Length - 1);
            var left = span.Slice(0, midpoint);
            quickSort(left);
            var right = span.Slice(midpoint + 1, span.Length - (midpoint + 1));
            quickSort(right);
        }
    }

    class HeapSort : Sorter<int>
    {
        override public IList<int> Sort(IList<int> input)
        {
            var startTime = DateTime.Now;
            this.Comparisons = 0;
            this.Iterations = 0;

            var output = input.ToArray();
            var outputSpan = output.AsSpan();
            var n = output.Length;

            // Convert array to min-heap
            for (int i = n / 2 - 1; i >= 0; i--)
                heapify(outputSpan, n, i);

            // Convert max-heap to sorted array
            for (int i = n - 1; i >= 0; i--)
            {
                swap(outputSpan, 0, i);
                heapify(outputSpan, i, 0);
            }

            Duration = DateTime.Now.Subtract(startTime);
            return output;
        }

        // Based on https://www.geeksforgeeks.org/heap-sort/
        private void heapify(Span<int> span, int n, int root)
        {
            Iterations++;
            int largest = root; // Initialize largest as root
            int left_child = (2 * root) + 1;
            int right_child = (2 * root) + 2;

            Comparisons++;
            if (left_child < n)
            {
                Comparisons++;
                if (span[left_child] > span[largest])
                    largest = left_child;
            }

            if (right_child < n)
            {
                Comparisons++;
                if (span[right_child] > span[largest])
                    largest = right_child;
            }

            if (largest != root)
            {
                swap(span, root, largest);
                heapify(span, n, largest);
            }
        }
    }

    class MergeSort : Sorter<int>
    {
        override public IList<int> Sort(IList<int> input)
        {
            var startTime = DateTime.Now;
            this.Comparisons = 0;
            this.Iterations = 0;

            var output = input.ToArray();
            mergeSort(output.AsSpan());

            Duration = DateTime.Now.Subtract(startTime);
            return output;
        }

        private void mergeSort(Span<int> span)
        {
            if (span.Length <= 1)
            {
                this.Iterations++;
                return;
            }
            var half = span.Length / 2;

            var left = span.Slice(0, half);
            mergeSort(left);
            var right = span.Slice(half, span.Length - half);
            mergeSort(right);


            var left_a = left.ToArray();
            var right_a = right.ToArray();

            var left_i = 0;
            var right_i = 0;
            while (left_i < left_a.Length && right_i < right_a.Length)
            {
                this.Iterations++;
                this.Comparisons++;
                if (left_a[left_i] <= right_a[right_i])
                    span[left_i + right_i] = left_a[left_i++];
                else
                    span[left_i + right_i] = right_a[right_i++];
            }

            for (; left_i < left_a.Length; left_i++)
            {
                span[left_i + right_i] = left_a[left_i];
                this.Iterations++;
            }

            for (; right_i < right_a.Length; right_i++)
            {
                span[left_i + right_i] = right_a[right_i];
                this.Iterations++;
            }
        }
    }
    
    class CountingSort : Sorter<int>
    {
        private readonly int k;

        public CountingSort() {
            this.k = 0;
        }

        public CountingSort(int k)
        {
            this.k = k;
        }

        override public IList<int> Sort(IList<int> input)
        {
            return Sort(input, this.k);
        }

        public IList<int> Sort(IList<int> input, int k = 0)
        {
            var startTime = DateTime.Now;
            this.Comparisons = 0;
            this.Iterations = 0;

            if (k == 0 && input.Count > 0)
            {
                Iterations += input.Count;
                Comparisons += input.Count;
                foreach (int num in input)
                    if (num > k) k = num;
            }

            var output = input.ToArray();
            countingSort(input, output.AsSpan(), k);

            Duration = DateTime.Now.Subtract(startTime);
            return output;
        }

        private void countingSort(IList<int> input, Span<int> output, int k)
        {
            var counts = new int[k + 1];

            foreach (var num in input)
            {
                counts[num]++;
                Iterations++;
            }

            for (int i = 1; i <= k; i++)
            {
                counts[i] += counts[i - 1];
                Iterations++;
            }

            for (int i = input.Count - 1; i >= 0; i--)
            {
                output[-1 + counts[input[i]]--] = input[i];
                Iterations++;
            }
        }
    }

    class RadixSort : Sorter<int>
    {
        private readonly int Base = 10;
        private int MaxLevel = 1;
        override public IList<int> Sort(IList<int> start)
        {
            var startTime = DateTime.Now;
            this.Comparisons = 0;
            this.Iterations = 0;

            int[] input;
            var output = start.ToArray();
            for (int level = 1; level <= this.MaxLevel; level *= this.Base)
            {
                input = output;
                output = new int[input.Length];
                radixSort(input, output.AsSpan(), level);
            }

            Duration = DateTime.Now.Subtract(startTime);
            return output;
        }

        private void radixSort(IList<int> input, Span<int> output, int level)
        {
            var counts = new int[this.Base];

            var upperLevel = level * this.Base;

            foreach (var num in input)
            {
                if (level == 1) 
                    while (num >= MaxLevel)
                        this.MaxLevel*=this.Base;
                
                counts[(num/level)%this.Base]++;
                Iterations++;
            }

            for (int i = 1; i < this.Base; i++)
            {
                counts[i] += counts[i - 1];
                Iterations++;
            }

            for (int i = input.Count - 1; i >= 0; i--)
            {
                output[-1 + counts[(input[i]/level)%this.Base]--] = input[i];
                Iterations++;
            }
        }
    }
}
