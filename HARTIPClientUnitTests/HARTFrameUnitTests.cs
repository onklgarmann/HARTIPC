using Microsoft.VisualStudio.TestTools.UnitTesting;
using HARTIPC;


namespace HARTIPClientUnitTests
{
    [TestClass]
    public class HARTFrameUnitTest
    {
        [TestMethod]
        public void ConstructorTest()
        {
            HARTFrame frame = new HARTFrame(new byte[] { 0x00 }, 0x00);
            Assert.AreEqual(frame.ToArray(), 10);

        }
    }
}
