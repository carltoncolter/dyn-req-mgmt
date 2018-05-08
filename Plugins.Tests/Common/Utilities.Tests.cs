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
    public class Common_UtilitiesTests
    {
        [TestMethod]
        public void CompressGuid_DecompressGuid()
        {
            var g = Guid.NewGuid();
            var str = Plugins.Common.Utilities.CompressGuid(g);
            var o = Plugins.Common.Utilities.DecompressGuid(str);
            Assert.AreEqual(o, g, "Guids do not match)");
        }

        [TestMethod]
        public void DecompressGuid_EmptySourceReturnsNull()
        {
            var o = Plugins.Common.Utilities.DecompressGuid(string.Empty);
            Assert.AreEqual(Guid.Empty, o, "An Empty Source should return an Empty Result.");
        }

        [TestMethod]
        public void Bug182_CompressGuidWithPrecedingZeros_DecompressGuid()
        {
            var g = Guid.Parse("{00ba83ad-a333-41b8-9bd8-709429b83ac0}");
            var str = Plugins.Common.Utilities.CompressGuid(g);
            var o = Plugins.Common.Utilities.DecompressGuid(str);
            Assert.AreEqual(o, g, "Guids do not match)");
        }

        [TestMethod]
        public void ToEntityTest()
        {
            // arrange
            var er = new EntityReference("account", Guid.NewGuid());

            // act
            var entity = er.ToEntity();

            // assert
            Assert.AreEqual(er, entity.ToEntityReference(), "Entity References Do Not Match");
        }
    }
}
