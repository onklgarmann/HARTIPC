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
            Assert.Fail();
        }

        [TestMethod()]
        [DataRow(new byte[] { 0x86, 0xA6, 0x4E, 0x0B, 0x6F, 0xE4, 0x4D, 0x0B, 0x00, 0x00, 0x05, 0x82, 0x91, 0x0E, 0x5D, 0x6B, 0x68, 0x01, 0x00, 0xEF })]
        [DataRow(new byte[] { 0x86, 0xA6, 0x4E, 0x0B, 0x6F, 0xE4, 0x4D, 0x0B, 0x00, 0x00, 0x05, 0x82, 0x91, 0x0E, 0x5D, 0x6B, 0x68, 0x01, 0x00, 0xEF })]
        [DataRow(new byte[] { 0x06, 0x4F, 0x0B, 0x6F, 0xE4, 0x4D, 0x0B, 0x00, 0x00, 0x05, 0x82, 0x91, 0x0E, 0x5D, 0x6B, 0x68, 0x01, 0x00, 0xEF })]
        public void HARTFrameBinaryTest(byte[] input)
        {
            var frame = new HARTFrame(input);

            Assert.IsInstanceOfType(frame, typeof(HARTFrame));
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