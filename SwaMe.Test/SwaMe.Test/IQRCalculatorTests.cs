using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace SwaMe.Test
{
    [TestClass]
   public class IQRCalculatorTests
    {
        private static List<double> DoubleList2Items;
        private static List<double> DoubleList5Items;
        private static List<int> IntList2Items;
        private static List<int> IntList5Items;

        [TestInitialize]
        public void Initialize()
        {
            DoubleList2Items = new List<double>() { 2.0,3.0};
            DoubleList5Items = new List<double>() { 2.0,3.0,3.0,4.0,4.0 };
            IntList2Items = new List<int>() { 2,3};
            IntList5Items = new List<int>() {2,3,4,5,6 };
        }

        /// <summary>
        /// This class calculates the IQR from a List of double/ integers, if there are at least 5 elements in the sequence, else should throw an exception.
        /// </summary>

        ///<remarks>
        ///IQR correctly calculated from List<double> containing at least 5 items.
        ///</remarks>
        [TestMethod]
        public void IQRcorrectIfDoublesListContains5Items() 
        {
            var correctIQR = 1;
            Assert.AreEqual(correctIQR, InterQuartileRangeCalculator.CalcIQR(DoubleList5Items));
        }
        ///<remarks>
        ///Throws an exception if the number of items in the list is less than 5.
        ///</remarks>
        [TestMethod]
        public void ExceptionTrownIfDoubleListLessThan5Items()
        {
            Assert.ThrowsException<ArgumentException>(() => InterQuartileRangeCalculator.CalcIQR(DoubleList2Items), "2 is too few list items to calculate IQR");
        }
        ///<remarks>
        ///IQR correctly calculated from List<int> containing at least 5 items.
        ///</remarks>
        [TestMethod]
        public void IQRcorrectIfIntListContains5Items()
        {
            var correctIQR = 2;
            Assert.AreEqual(correctIQR, InterQuartileRangeCalculator.CalcIQR(IntList5Items));
        }
        ///<remarks>
        ///Throws an exception if the number of items in the list is less than 5.
        ///</remarks>
        [TestMethod]
        public void ExceptionTrownIfIntListLessThan5Items()
        {
            Assert.ThrowsException<ArgumentException>(() => InterQuartileRangeCalculator.CalcIQR(IntList2Items), "2 is too few list items to calculate IQR");
        }
    }
}
