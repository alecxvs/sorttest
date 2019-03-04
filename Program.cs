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
                {"Merge Sort", new MergeSort()}
            };

            var datasets = new Dictionary<string, List<int>> {
                {"Sorted", sortedArray},
                {"Reversed", reversedArray},
                {"Random", randomArray}
            };

            foreach((string sorterName, Sorter<int> sorter) in sorters) {
                foreach((string dataName, List<int> dataset) in datasets) {
                    if (sorter.Sort(dataset).SequenceEqual(sortedArray))
                        Console.WriteLine($"[{sorterName} - {dataName}] {sorter.SortingStats()}");
                    else
                        Console.WriteLine($"{sorterName} failed to sort list {dataName} correctly ({sorter.SortingStats()}");
                }
            }
        }

        static void Shuffle(List<int> input) {
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

    struct SorterTestCase
    {
        String SorterName;
        Sorter<int> Sorter;

    }

    interface ISorter<T> {
        IList<T> Sort(IList<T> input);
    }

    abstract class Sorter<T> {
        public int Comparisons { get; protected set; } = 0;
        public int Iterations { get; protected set; } = 0;
        public TimeSpan Duration { get; protected set; }
        abstract public IList<T> Sort(IList<T> input);
        public String SortingStats() {
            return $"Statistics for last sort: Duration {Duration} / {Iterations} Iterations / {Comparisons} Comparisons";
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
                while (++lookback <= i && output[i-lookback] > current) {
                    this.Iterations++;
                    this.Comparisons++;
                    output[(i-lookback)+1] = output[i-lookback];
                }
                output[i-(lookback-1)] = current;
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
            mergeSort(output.AsSpan());

            Duration = DateTime.Now.Subtract(startTime);
            return output;
        }

        private void mergeSort(Span<int> span) {
            if (span.Length <= 1) {
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
            while (left_i < left_a.Length && right_i < right_a.Length) {
                this.Iterations++;
                this.Comparisons++;
                if (left_a[left_i] <= right_a[right_i])
                    span[left_i + right_i] = left_a[left_i++];
                else
                    span[left_i + right_i] = right_a[right_i++];
            }

            for (;left_i < left_a.Length; left_i++) {
                span[left_i + right_i] = left_a[left_i];
                this.Iterations++;
            }
            
            for (;right_i < right_a.Length; right_i++) {
                span[left_i + right_i] = right_a[right_i];
                this.Iterations++;
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

        private void mergeSort(Span<int> span) {
            if (span.Length <= 1) {
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
            while (left_i < left_a.Length && right_i < right_a.Length) {
                this.Iterations++;
                this.Comparisons++;
                if (left_a[left_i] <= right_a[right_i])
                    span[left_i + right_i] = left_a[left_i++];
                else
                    span[left_i + right_i] = right_a[right_i++];
            }

            for (;left_i < left_a.Length; left_i++) {
                span[left_i + right_i] = left_a[left_i];
                this.Iterations++;
            }
            
            for (;right_i < right_a.Length; right_i++) {
                span[left_i + right_i] = right_a[right_i];
                this.Iterations++;
            }
        }
    }
}
