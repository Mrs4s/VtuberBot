using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class TestMain
    {
        [TestMethod]
        public void ChartBlocksTest()
        {
            var wc = new WordCloud.WordCloud(1920,1080);
        }
    }
}
