using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MATQuestion
{
    public class StopWatchProvider : Stopwatch, IStopWatchProvider
    {
        public new IStopWatchProvider StartNew()
        {
            var watch = new StopWatchProvider();
            watch.Start();
            return watch;
        }
    }

    public class RandomProvider : Random, IRandomNextProvider
    {
        public RandomProvider(int lowerStr, int upperStr, int lowerWgt, int upperWgt)
        {
            UpperStr = upperStr;
            LowerStr = lowerStr;
            UpperWgt = upperWgt;
            LowerWgt = lowerWgt;
        }

        private int UpperStr { get; }
        private int LowerStr { get; }
        private int UpperWgt { get; }
        private int LowerWgt { get; }

        public int NextWgt()
        {
            return Next(LowerWgt, UpperWgt);
        }

        public int NextStr()
        {
            return Next(LowerStr, UpperStr);
        }
    }

    public static class Six
    {
        public static void Main()
        {
            Console.WriteLine("Enter the number of items in our shopping bag");
            var input = 0;
            try
            {
                input = int.Parse(Console.ReadLine());
            }
            catch (FormatException)
            {
                Main();
                Environment.Exit(0);
            }

            var x = new RandomProvider(1, 5, 1, 3);
            var items = ReturnItems(input, x);
            Item[] safeArray = null;
            var elapsedMsNormal = new TimeSpan();
            var enumerable1 = items as Item[] ?? items.ToArray();
            try
            {
                safeArray = MakeSafe(enumerable1.ToArray(), out elapsedMsNormal, new StopWatchProvider());
            }
            catch (OverflowException)
            {
                Console.WriteLine("It is impossible to make the randomly generated shopping bag safe! :(");
                Environment.Exit(0);
            }

            Console.WriteLine("The resulting shopping bag is:");
            foreach (var item in safeArray) Console.WriteLine("Weight: {0}, Strength: {1}", item.Weight, item.Strength);

            Console.WriteLine("... which took {0} milliseconds to calculate.", elapsedMsNormal.Ticks / 1000d);

            var (orderByStrengthTotal, orderByStrength) = CalculateTimings(e => e.OrderBy(y => y.Strength), enumerable1,
                new StopWatchProvider());
            Console.WriteLine(
                "However, if we order the shopping bag by strength first and then try to it safe, it takes {0} milliseconds({1} excluding the sorting time), " +
                "which is {2} times longer",
                orderByStrengthTotal.Ticks / 1000d, orderByStrength.Ticks / 1000d,
                orderByStrengthTotal.Ticks / 1000d / (elapsedMsNormal.Ticks / 1000d));


            var (orderByWeightTotal, orderByWeight) =
                CalculateTimings(e => e.OrderBy(y => y.Weight), enumerable1, new StopWatchProvider());
            var divided = orderByWeightTotal.Ticks * 100d /
                          (elapsedMsNormal.Ticks * 100d);
            Console.WriteLine(
                "However, if we order the shopping bag by weight first and then try to it safe, it takes {0} milliseconds({1} excluding the sorting time), " +
                "which is {2} times longer",
                orderByWeightTotal.Ticks / 1000d, orderByWeight.Ticks / 1000d, divided);

            var calculations = Calculations(10000, new RandomProvider(1, 4, 1, 4), new StopWatchProvider());
            var enumerable = calculations as Tuple<double, double>[] ?? calculations.ToArray();

            var strengthAverage = enumerable.Average(a => a.Item1);
            var weightAverage = enumerable.Average(a => a.Item2);
            Console.WriteLine(
                "On average, sorting by strength first takes {0} times longer and sorting by weight first takes {1} times longer",
                strengthAverage, weightAverage);
            //var orderedByWeightThenByStrength = items.OrderBy(y => y.Weight).ThenBy(y => y.Strength);
            Environment.Exit(0);
        }

        public static IEnumerable<Tuple<double, double>> Calculations(int count, IRandomNextProvider randomNextProvider,
            IStopWatchProvider stopWatchProvider)
        {
            var enumerable = new List<Tuple<double, double>>();
            var rand = new Random();
            for (var i = 0; i < count; i++)
                try
                {
                    var items = ReturnItems(rand.Next(1, 5), randomNextProvider).ToArray();
                    MakeSafe(items.ToArray(), out var elapsedMsNormal, stopWatchProvider);

                    var resultStrength = CalculateTimings(e => e.OrderBy(y => y.Strength), items,
                        stopWatchProvider);

                    var resultStrengthCalc = resultStrength.Item1.Ticks / 1000d /
                                             ((elapsedMsNormal.Ticks == 0 ? 1 : elapsedMsNormal.Ticks) / 1000d);

                    var resultWeight =
                        CalculateTimings(e => e.OrderBy(y => y.Weight), items, stopWatchProvider);

                    var resultWeightCalc = resultWeight.Item1.Ticks / 1000d /
                                           ((elapsedMsNormal.Ticks == 0 ? 1 : elapsedMsNormal.Ticks) / 1000d);

                    enumerable.Add(new Tuple<double, double>(resultStrengthCalc, resultWeightCalc));
                }
                catch (OverflowException)
                {
                }

            return enumerable;
        }

        public static Tuple<TimeSpan, TimeSpan> CalculateTimings(Func<IEnumerable<Item>,
            IOrderedEnumerable<Item>> calculation, IEnumerable<Item> arg, IStopWatchProvider stopWatchProvider)
        {
            var stopWatch = stopWatchProvider.StartNew();
            var output = calculation.Invoke(arg);
            MakeSafe(output.ToArray(), out var withoutCalcMs, stopWatchProvider);
            var totalMs = stopWatch.Elapsed;
            return new Tuple<TimeSpan, TimeSpan>(totalMs, withoutCalcMs);
        }

        public static Item[] MakeSafe(Item[] items, out TimeSpan elapsedMs, IStopWatchProvider stopWatchProvider)
        {
            var watch = stopWatchProvider.StartNew();
            if (CheckIsSafe(items))
            {
                watch.Stop();
                elapsedMs = watch.Elapsed;
                return items;
            }

            var factorial = items.Length.Factorial();
            var noOfTries = 0;
            var sorted = false;

            while (!sorted)
            {
                for (var i = 0; i < items.Length; i++)
                    if (!CheckIsSafeIndexed(items, i))
                    {
                        var temp = items[i - 1];
                        items[i - 1] = items[i];
                        items[i] = temp;
                    }

                if (CheckIsSafe(items)) sorted = true;

                if (noOfTries == factorial) throw new OverflowException();

                noOfTries++;
            }

            watch.Stop();
            elapsedMs = watch.Elapsed;
            return items;
        }

        public static IEnumerable<Item> ReturnItems(int noOfItems, IRandomNextProvider randomNextProvider)
        {
            for (var q = 0; q < noOfItems; q++)
                yield return new Item(randomNextProvider.NextWgt(), randomNextProvider.NextStr());
        }

        public static int Factorial(this int input)
        {
            if (input == 0) return 1;

            var result = input;
            for (var i = 1; i < input; i++) result *= i;

            return result;
        }

        public static bool CheckIsSafeIndexed(Item[] arrayItems, int index)
        {
            var totalWeight = 0;
            for (var j = 0; j < index; j++) totalWeight += arrayItems[j].Weight;

            return arrayItems[index].Strength >= totalWeight;
        }

        public static bool CheckIsSafe(Item[] arrayItems)
        {
            for (var i = 0; i < arrayItems.Count(); i++)
            {
                var totalWeight = 0;
                for (var j = 0; j < i; j++) totalWeight += arrayItems[j].Weight;

                if (arrayItems[i].Strength < totalWeight) return false;
            }

            return true;
        }
    }

    public interface IStopWatchProvider
    {
        TimeSpan Elapsed { get; }
        IStopWatchProvider StartNew();
        void Stop();
    }

    public interface IRandomNextProvider
    {
        int NextWgt();
        int NextStr();
    }

    public class Item
    {
        public Item(int weight, int strength)
        {
            Weight = weight;
            Strength = strength;
        }

        public int Weight { get; }
        public int Strength { get; }
    }
}