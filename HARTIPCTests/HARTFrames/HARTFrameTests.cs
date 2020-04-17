using Microsoft.VisualStudio.TestTools.UnitTesting;
using HARTIPC;
using System;
using System.Collections.Generic;
using System.Text;

namespace HARTIPC.Tests
{
    [TestClass()]
    public class HARTFrameTests
    {
        [TestMethod()]
        [DataRow(new byte[] { 0x06 }, (byte)0x00, new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 })]
        public void HARTFrameCTORTest(byte[] address, byte command, byte[] payload)
        {
            List<IHARTFrame> frames = new List<IHARTFrame>
            {
                new HARTFrame(address, command),
                new HARTFrameWithPayload(address, command, payload),
                new HARTFrameACK(address, command, payload)
            };
            foreach (IHARTFrame frame in frames)
                Assert.IsInstanceOfType(frame, typeof(HARTFrame));
        }
        [TestMethod()]
        [DataRow(new byte[] { 0x06 }, (byte)0x00, new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 })]
        public void HARTFrameSERIALIZETest(byte[] address, byte command, byte[] payload)
        {
            List<IHARTFrame> frames = new List<IHARTFrame>
            {
                new HARTFrame(address, command),
                new HARTFrameWithPayload(address, command, payload),
                new HARTFrameACK(address, command, payload)
            };
            foreach (IHARTFrame frame in frames)
            {
                CollectionAssert.Equals(frame.GetAddress(), address);
                Assert.AreEqual(frame.Command, command);
            }
        }
        [TestMethod()]
        [DataRow(new byte[] { 0x02, 0x80, 0x00, 0x00, 0x82 })]
        [DataRow(new byte[] { 0x82, 0xa6, 0x4e, 0x0b, 0x6f, 0xe4, 0x14, 0x00, 0xfe })]
        [DataRow(new byte[] { 0x06, 0x80, 0x00, 0x18, 0x00, 0x50, 0xfe, 0x26, 0x4e, 0x05, 0x07, 0x05, 0x02, 0x0e,
            0x0c, 0x0b, 0x6f, 0xe4, 0x05, 0x04, 0x00, 0x02, 0x00, 0x00, 0x26, 0x00, 0x26, 0x84, 0x58 })]
        [DataRow(new byte[] { 0x86, 0xa6, 0x4e, 0x0b, 0x6f, 0xe4, 0x14, 0x22, 0x00, 0x50, 0x77, 0x69, 0x68, 0x61,
            0x72, 0x74, 0x67, 0x77, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x89 })]
        [DataRow(new byte[] { 0x06, 0x80, 0x00, 0x18, 0x00, 0x50, 0xfe, 0x26, 0x4e, 0x05, 0x07, 0x05, 0x02, 0x0e,
            0x0c, 0x0b, 0x6f, 0xe4, 0x05, 0x04, 0x00, 0x02, 0x00, 0x00, 0x26, 0x00, 0x26, 0x84, 0x58, 0x00, 0x01 })]
        public void HARTFrameDECODERTest(byte[] binary)
        {
            HARTDecoder decoder = new HARTDecoder();
            var actual = decoder.Decode(ref binary);
            
            Assert.IsInstanceOfType(actual, typeof(IHARTFrame));
        }
    }
}