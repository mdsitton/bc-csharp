using System;

namespace Org.BouncyCastle.Utilities.Test
{
    [Serializable]
    public class TestFailedException
        : Exception
    {
        private ITestResult _result;

        public TestFailedException(
            ITestResult result)
        {
            _result = result;
        }

        public ITestResult GetResult()
        {
            return _result;
        }
    }
}
