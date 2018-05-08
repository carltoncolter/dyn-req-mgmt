using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugins.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PluginTests.Common
{
    using Microsoft.Xrm.Sdk;

    [TestClass]
    public class Common_ErrorCodeTests
    {
        private static readonly string INVALIDMESSAGE = "An unknown error occurred.";

        [TestMethod]
        public void ExceptionGenerationWithNumber()
        {
            var ex = ErrorCode.Exception(9999, "full test");
            Assert.AreEqual("Testing... full test", ex.Message);
            Assert.AreEqual(9999, ex.ErrorCode);
        }

        [TestMethod]
        public void ExceptionGenerationNullTarget1615WithEnum()
        {
            var ex = ErrorCode.Exception(ErrorCodes.NullTarget);
            Assert.AreNotEqual(INVALIDMESSAGE, ex.Message);
            Assert.AreEqual("Target was null.", ex.Message);
            Assert.AreEqual(1615, ex.ErrorCode);
        }

        [TestMethod]
        public void ExceptionGenerationWithNumberAndStatus()
        {
            var ex = ErrorCode.Exception(OperationStatus.Failed, 9999, "full test");
            Assert.AreEqual("Testing... full test", ex.Message);
            Assert.AreEqual(9999, ex.ErrorCode);
            Assert.AreEqual(OperationStatus.Failed, ex.Status);
        }

        [TestMethod]
        public void ExceptionGenerationWithEnumAndStatus()
        {
            var ex = ErrorCode.Exception(OperationStatus.Failed, ErrorCodes.TestError, "full test");
            Assert.AreEqual("Testing... full test", ex.Message);
            Assert.AreEqual(9999, ex.ErrorCode);
            Assert.AreEqual(OperationStatus.Failed, ex.Status);
        }

        [TestMethod]
        public void TestForErrorCodeMessages()
        {
            
            foreach (int code in Enum.GetValues(typeof(ErrorCodes)))
            {
                var message = ErrorCode.GetErrorCodeString(code);
                Assert.AreNotEqual(
                    INVALIDMESSAGE,
                    message,
                    $"Code {code} has an undefined or invalid message in GetErrorCodeString");
            }

            Assert.AreEqual(INVALIDMESSAGE, ErrorCode.GetErrorCodeString(-1), "Error Code -1 should be undefined.");
        }
    }
}
