using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace MATQuestion
{
    public static class Six
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Enter the number of items in our shopping bag");
            int input = 0;
            try
            {
                input = int.Parse(Console.ReadLine());
            }
            catch(FormatException)
            {
                Main(new string[0]);
            }

            var items = ReturnItems(input);
            Item[] safeArray = null;
            try
            {
                safeArray = MakeSafe(items.ToArray());
            }
            catch(OverflowException)
            {
                Console.WriteLine("It is impossible to make the randomly generated shopping bag safe! :(");
                Environment.Exit(0);
            }
            
            foreach (var item in safeArray)
            {
                Console.WriteLine("Weight: {0}, Strength: {1}",item.Weight, item.Strength);
            }
            Environment.Exit(0);
        }

        public static Item[] MakeSafe(Item[] items)
        {
            if (CheckIsSafe(items))
            {
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
                        var temp = items[i-1];
                        items[i - 1] = items[i];
                        items[i] = temp;
                    }
                }

                if (CheckIsSafe(items))
                {
                    sorted = true;
                }

                if (noOfTries == 100)
                {
                    throw new OverflowException();
                }

                noOfTries++;

                
            }

            return items;
        }

        public static IEnumerable<Item> ReturnItems(int NoOfItems)
        {
            var rand = new Random();
            for (int q = 0; q < NoOfItems; q++)
            {
                yield return new Item(rand.Next(1,5), rand.Next(1,15));
            }
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

    public class Item
    {
        public int Weight { get; set; }
        public int Strength { get; set; }

        public Item(int weight, int strength)
        {
            Weight = weight;
            Strength = strength;
        }
    }
    
}