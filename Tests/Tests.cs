using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MATQuestion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using static MATQuestion.Six;

namespace Tests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void ReturnItems_Input0_ReturnsEmptyArray()
        {
            Assert.IsNull(ReturnItems(0, new RandomProvider(0, 0, 0, 0)).SingleOrDefault());
        }

        [TestMethod]
        public void ReturnItems_Input10_Returns10Items()
        {
            Assert.AreEqual(10, ReturnItems(10, new RandomProvider(0, 0, 0, 0)).Count(),
                "The number of items returned should be equal to the length of " +
                "the integer inputted.");
        }

        [TestMethod]
        public void ReturnItems_Input1_ReturnsItem()
        {
            Assert.IsInstanceOfType(ReturnItems(1, new RandomProvider(0, 0, 0, 0)).Single(), typeof(Item),
                "IEnumerable returned should be of type 'Item'");
        }

        [TestMethod]
        public void CheckIsSafe_InputArrayOf1_ReturnsTrue()
        {
            Item[] array = {new Item(0, 0)};
            Assert.IsTrue(CheckIsSafe(array), "An array of one returns true");
        }

        [TestMethod]
        public void CheckIsSafe_InputUnSafeArray_ReturnsFalse()
        {
            Item[] array =
            {
                new Item(2, 0), new Item(0, 1)
            };
            Assert.IsFalse(CheckIsSafe(array), "An unsafe array should return false");
        }

        [TestMethod]
        public void CheckIsSafeIndex_InputUnSafeArrayButSafeIndex_ReturnsFalse()
        {
            Item[] array =
            {
                new Item(0, 0), new Item(1, 1), new Item(0, 0)
            };
            Assert.IsTrue(CheckIsSafeIndexed(array, 1), "An unsafe array at a safe index should return true");
        }

        [TestMethod]
        public void CheckIsSafeIndex_InputUnSafeArrayAndUnSafeIndex_ReturnsFalse()
        {
            Item[] array =
            {
                new Item(1, 0), new Item(0, 0), new Item(0, 0), new Item(0, 0)
            };
            Assert.IsFalse(CheckIsSafeIndexed(array, 1), "An unsafe array at a unsafe index should return false");
        }

        [TestMethod]
        public void MakeSafe_InputArrayOf0_ReturnsArray()
        {
            var stopWatchProviderStub = new Mock<IStopWatchProvider>();

            Item[] array = { };
            TimeSpan _;
            Assert.IsNotNull(MakeSafe(array, out _, stopWatchProviderStub.Object),
                "An input array of zero is already ordered!");
        }

        [TestMethod]
        public void MakeSafe_UnsafeInputArray_ReturnsSafeArray()
        {
            var stopWatchProviderStub = new Mock<IStopWatchProvider>();

            Item[] unSafeArray = {new Item(1, 0), new Item(0, 0)};
            Item[] safeArray = {new Item(0, 0), new Item(1, 0)};
            TimeSpan _;
            Assert.AreEqual(safeArray[0].Weight, MakeSafe(unSafeArray, out _, stopWatchProviderStub.Object)[0].Weight,
                "An the ordering should be working");
        }

        [TestMethod]
        public void ReturnItems_GivenNumbers_ReturnsCorrectArray()
        {
            var randomProviderStub = new Mock<IRandomNextProvider>();
            randomProviderStub.Setup(x => x.NextStr()).Returns(0);
            randomProviderStub.Setup(x => x.NextWgt()).Returns(0);
            Assert.AreEqual(0, ReturnItems(1, randomProviderStub.Object).Single().Weight, "The array should be " +
                                                                                          "getting produced properly.");
            Assert.AreEqual(0, ReturnItems(1, randomProviderStub.Object).Single().Strength, "The array should be " +
                                                                                            "getting produced properly.");
        }

        [TestMethod]
        public void Factorial_0_Returns1()
        {
            Assert.AreEqual(1, 0.Factorial(), "Factorial of 0 is 1.");
        }

        [TestMethod]
        public void Factorial_1_Returns1()
        {
            Assert.AreEqual(1, 1.Factorial(), "Factorial of 0 is 1.");
        }

        [TestMethod]
        public void Factorial_5_Returns120()
        {
            Assert.AreEqual(120, 5.Factorial(), "Factorial of 5 is 120.");
        }

        [TestMethod]
        public void CalculateTimings_InputFunc_ReturnsTupleOfLongs()
        {
            var stopWatchProviderStub = new Mock<IStopWatchProvider>();
            stopWatchProviderStub.Setup(x => x.Elapsed).Returns(new TimeSpan(0));
            var list = new List<Item> {new Item(1, 0), new Item(0, 1)};
            Func<IEnumerable<Item>, IOrderedEnumerable<Item>> function = a => a.OrderBy(y => y.Weight);
            var result = CalculateTimings(function, list, stopWatchProviderStub.Object);
            Assert.IsInstanceOfType(result, typeof(Tuple<TimeSpan, TimeSpan>), "We should get as the result.");
        }

        [TestMethod]
        public void MakeSafe_InputImpossibleArray_ThrowsOverflowException()
        {
            var stopWatchProviderStub = new Mock<IStopWatchProvider>();
            // Producing an unsafe array
            Item[] list = {new Item(1, 1), new Item(2, 0)};
            Assert.ThrowsException<OverflowException>(() => MakeSafe(list, out _, stopWatchProviderStub.Object));
        }

        [TestMethod]
        public void StopWatchProvider_Constructor_ReturnsStopWatch()
        {
            var stopWatch = new StopWatchProvider();
            Assert.IsInstanceOfType(stopWatch, typeof(Stopwatch));
        }

        [TestMethod]
        public void Calculations_Input0_ReturnsEmptyArray()
        {
            var (randomStub, stopWatchStub) = CalculationsStub(0, 0);

            var calculations = Calculations(0, randomStub, stopWatchStub);
            Assert.AreEqual(0, calculations.Count());
        }

        [TestMethod]
        public void Calculations_Input0_ReturnsArrayOfTuplesOfDoubles()
        {
            var (randomStub, stopWatchStub) = CalculationsStub(0, 0);
            var calculations = Calculations(0, randomStub, stopWatchStub);
            Assert.IsInstanceOfType(calculations, typeof(IEnumerable<Tuple<double, double>>));
        }

        [TestMethod]
        public void Calculations_Input1_ReturnsArrayOfTuplesOfDoubles()
        {
            var (randomStub, stopWatchStub) = CalculationsStub(1, 0);
            var calculations = Calculations(1, randomStub, stopWatchStub);
            Assert.AreEqual(0d, calculations.Single().Item1);
        }

        [TestMethod]
        public void Calculations_InputUnsafeArray_ReturnsEmptyArray()
        {
            var (randomStub, stopWatchStub) = CalculationsStub(0, 1);
            var calculations = Calculations(1, randomStub, stopWatchStub);
            Assert.IsFalse(calculations.Any());
        }


        public static Tuple<IRandomNextProvider, IStopWatchProvider> CalculationsStub(int str, int wgt)
        {
            var randomProviderStub = new Mock<IRandomNextProvider>();
            var stopWatchProviderStub = new Mock<IStopWatchProvider>();
            stopWatchProviderStub.Setup(x => x.Elapsed).Returns(new TimeSpan(0));
            randomProviderStub.Setup(x => x.NextStr()).Returns(str);
            randomProviderStub.Setup(x => x.NextWgt()).Returns(wgt);


            return new Tuple<IRandomNextProvider, IStopWatchProvider>(randomProviderStub.Object,
                stopWatchProviderStub.Object);
        }
    }
}