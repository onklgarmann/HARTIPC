using Microsoft.VisualStudio.TestTools.UnitTesting;
using HARTIPC;
using System;
using System.Collections.Generic;
using System.Text;

namespace HARTIPC.Tests
{
    [TestClass]
    public class HARTFrameTests
    {
        [TestMethod]
        public void HARTFrameTest()
        {
            var frame = new HARTFrame(new byte[] { 0x00 }, (byte)0x00);
            Assert.IsInstanceOfType(frame, typeof(HARTFrame));
        }

        [TestMethod()]
        public void HARTFrameBinaryTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ToArrayTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetAddressTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetPayloadTest()
        {
            Assert.Fail();
        }
    }
}