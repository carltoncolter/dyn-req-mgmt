using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PluginsTests.Common
{
    [TestClass]
    public class Common_AssemblyTests
    {
        #region Success Tests
        [TestMethod]
        public void Assembly_IsSigned()
        {
            #region arrange - given
            var asm = typeof(Plugins.Common.PluginBase).Assembly;
            var asmName = asm.GetName();
            #endregion

            #region act - when
            //always
            #endregion

            #region assert - then
            byte[] key = asmName.GetPublicKey();
            Assert.IsTrue(key.Length > 0, "Dynamics Plugins must be signed.");
            #endregion
        }
        #endregion
    }
}
