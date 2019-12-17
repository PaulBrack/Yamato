using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace SwaMe.Test
{
    [TestClass]
   public class IQRCalculatorTests
    {
        private static List<double> doubleList2Items;
        private static List<double> doubleList5Items;
        private static List<int> intList2Items;
        private static List<int> intList5Items;

        [TestInitialize]
        public void Initialize()
        {
            doubleList2Items = new List<double>() { 2.0,3.0};
            doubleList5Items = new List<double>() { 2.0,3.0,3.0,4.0,4.0 };
            intList2Items = new List<int>() { 2,3};
            intList5Items = new List<int>() {2,3,4,5,6 };
        }

        [TestMethod]
        public void IQRcorrectIfDoublesListContains5Items() 
        {
            var correctIQR = 1;
            Assert.AreEqual(correctIQR, InterQuartileRangeCalculator.CalcIQR(doubleList5Items));
        }
        [TestMethod]
        public void ExceptionTrownIfDoubleListLessThan5Items()
        {
            Assert.ThrowsException<ArgumentException>(() => InterQuartileRangeCalculator.CalcIQR(doubleList2Items), "2 is too few list items to calculate IQR");
        }

        [TestMethod]
        public void IQRcorrectIfIntListContains5Items()
        {
            var correctIQR = 2;
            Assert.AreEqual(correctIQR, InterQuartileRangeCalculator.CalcIQR(intList5Items));
        }
        [TestMethod]
        public void ExceptionTrownIfIntListLessThan5Items()
        {
            Assert.ThrowsException<ArgumentException>(() => InterQuartileRangeCalculator.CalcIQR(intList2Items), "2 is too few list items to calculate IQR");
        }
    }
}
