using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace MATQuestion
{
    public class StopWatchProvider : Stopwatch, IStopWatchProvider
    {
        public StopWatchProvider()
        {
            
        }
    }
    public class RandomProvider : Random, IRandomNextProvider
    {
        private int UpperStr { get; set; }
        private int LowerStr { get; set; }
        private int UpperWgt { get; set; }
        private int LowerWgt { get; set; }

        public RandomProvider(int lowerStr, int upperStr, int lowerWgt, int upperWgt)
        {
            UpperStr = upperStr;
            LowerStr = lowerStr;
            UpperWgt = upperWgt;
            LowerWgt = lowerWgt;
        }

        public int NextWgt()
        {
            return this.Next(LowerWgt, UpperWgt);
        }

        public int NextStr()
        {
            return this.Next(LowerStr, UpperStr);
        }
    }

    public static class Six
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Enter the number of items in our shopping bag");
            int input = 0;
            try
            {
                input = int.Parse(Console.ReadLine());
            }
            catch (FormatException)
            {
                Main(new string[0]);
                Environment.Exit(0);
            }

            var x = new RandomProvider(1, 5, 1, 3);
            var items = ReturnItems(input, x);
            Item[] safeArray = null;
            TimeSpan elapsedMsNormal = new TimeSpan();
            try
            {
                safeArray = MakeSafe(items.ToArray(), out elapsedMsNormal, new StopWatchProvider());
            }
            catch (OverflowException)
            {
                Console.WriteLine("It is impossible to make the randomly generated shopping bag safe! :(");
                Environment.Exit(0);
            }

            Console.WriteLine("The resulting shopping bag is:");
            foreach (var item in safeArray)
            {
                Console.WriteLine("Weight: {0}, Strength: {1}", item.Weight, item.Strength);
            }

            Console.WriteLine("... which took {0} milliseconds to calculate.", (elapsedMsNormal.Ticks / 1000d));

            var orderByStrength = CalculateTimings(e => e.OrderBy(y => y.Strength), items, new StopWatchProvider());
            Console.WriteLine(
                "However, if we order the shopping bag by strength first and then try to it safe, it takes {0} milliseconds({1} excluding the sorting time), " +
                "which is {2} times longer",
                orderByStrength.Item1.Ticks/1000, orderByStrength.Item2.Ticks/1000d, (orderByStrength.Item1.Ticks/1000d)/(elapsedMsNormal.Ticks/ 1000d));

            
            var orderByWeight = CalculateTimings(e => e.OrderBy(y => y.Weight), items, new StopWatchProvider());
            double divided = (orderByWeight.Item1.Ticks * 100d) /
                           (elapsedMsNormal.Ticks * 100d);
            Console.WriteLine(
                "However, if we order the shopping bag by weight first and then try to it safe, it takes {0} milliseconds({1} excluding the sorting time), " +
                "which is {2} times longer",
                orderByWeight.Item1.Ticks/1000d, orderByWeight.Item2.Ticks/1000d, divided);

            var calcs = Calculations(10000);
            double strengthAverage = (calcs.Average(a => a.Item1));
            
            double weightAverage = (calcs.Average(a => a.Item2));
            Console.WriteLine("On average, sorting by strength first takes {0} times longer and sorting by weight first takes {1} times longer", 
                strengthAverage, weightAverage);
            //var orderedByWeightThenByStrength = items.OrderBy(y => y.Weight).ThenBy(y => y.Strength);
            Environment.Exit(0);
        }

        private static IEnumerable<Tuple<double,double>> Calculations(int count)
        {
            var enumerable = new List<Tuple<double, double>>();
            TimeSpan elapsedMsNormal;
            Random rand = new Random();
            for (int i = 0; i < count; i++)
            {
                try
                {
                    var itemstest = ReturnItems(rand.Next(1, 5), new RandomProvider(1, 15, 1, 4));
                    MakeSafe(itemstest.ToArray(), out elapsedMsNormal, new StopWatchProvider());
                    var resultStrength = CalculateTimings(e => e.OrderBy(y => y.Strength), itemstest,
                        new StopWatchProvider());
                    var resultStrengthCalc = (resultStrength.Item1.Ticks / 1000d) / (elapsedMsNormal.Ticks / 1000d);
                    var resultWeight =
                        CalculateTimings(e => e.OrderBy(y => y.Weight), itemstest, new StopWatchProvider());
                    var resultWeightCalc = (resultWeight.Item1.Ticks / 1000d) / (elapsedMsNormal.Ticks / 1000d);
                    enumerable.Add(new Tuple<double, double>(resultStrengthCalc, resultWeightCalc));
                }
                catch
                {
                }
            }

            return enumerable;
        }
        public static Tuple<TimeSpan, TimeSpan> CalculateTimings(Func<IEnumerable<Item>,
            IOrderedEnumerable<Item>> calculation, IEnumerable<Item> arg, IStopWatchProvider stopWatchProvider)
        {
            var stopWatch = stopWatchProvider;
            stopWatch.Start();
            var output = calculation.Invoke(arg);
            MakeSafe(output.ToArray(), out var withoutCalcMs, stopWatchProvider);
            var totalMs = stopWatch.Elapsed;
            return new Tuple<TimeSpan, TimeSpan>(totalMs, withoutCalcMs);
        }

        public static Item[] MakeSafe(Item[] items, out TimeSpan elapsedMs, IStopWatchProvider stopWatchProvider)
        {
            var watch = stopWatchProvider;
            watch.Start();
            if (CheckIsSafe(items))
            {
                watch.Stop();
                elapsedMs = watch.Elapsed;
                return items;
            }

            int noOfTries = 0;
            bool sorted = false;

            while (!sorted)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    if (!CheckIsSafeIndexed(items, i))
                    {
                        var temp = items[i - 1];
                        items[i - 1] = items[i];
                        items[i] = temp;
                    }
                }

                if (CheckIsSafe(items))
                {
                    sorted = true;
                }

                if (noOfTries == items.Length.Factorial())
                {
                    throw new OverflowException();
                }

                noOfTries++;
            }

            watch.Stop();
            elapsedMs = watch.Elapsed;
            return items;
        }

        public static IEnumerable<Item> ReturnItems(int NoOfItems, IRandomNextProvider randomNextProvider)
        {
            for (int q = 0; q < NoOfItems; q++)
            {
                yield return new Item(randomNextProvider.NextWgt(), randomNextProvider.NextStr());
            }
        }

        public static int Factorial(this int input)
        {
            if (input == 0)
            {
                return 1;
            }

            int result = input;
            for (int i = 1; i < input; i++)
            {
                result *= i;
            }

            return result;
        }

        public static bool CheckIsSafeIndexed(Item[] arrayItems, int Index)
        {
            int totalWeight = 0;
            for (int j = 0; j < Index; j++)
            {
                totalWeight += arrayItems[j].Weight;
            }

            if (arrayItems[Index].Strength < totalWeight)
            {
                return false;
            }

            return true;
        }

        public static bool CheckIsSafe(Item[] arrayItems)
        {
            for (int i = 0; i < arrayItems.Count(); i++)
            {
                int totalWeight = 0;
                for (int j = 0; j < i; j++)
                {
                    totalWeight += arrayItems[j].Weight;
                }

                if (arrayItems[i].Strength < totalWeight)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public interface IStopWatchProvider
    {
        TimeSpan Elapsed { get; }
        void Start();
        void Stop();
    }

    public interface IRandomNextProvider
    {
        int NextWgt();
        int NextStr();
    }

    public class Item
    {
        public int Weight { get; }
        public int Strength { get; }

        public Item(int weight, int strength)
        {
            Weight = weight;
            Strength = strength;
        }
    }
}