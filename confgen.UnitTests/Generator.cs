using System.Collections.Generic;
using System.Xml.Linq;
using NUnit.Framework;

namespace Confgen.UnitTests {
    [TestFixture]
    public class GeneratorTest {
        [Test]
        public void ShouldGenerateXmlWithVariables() {
            var xml = new XDocument(new XElement("config", new XAttribute(Console.BuildConfigForAttribute, "test,dev"),
                new XElement("base-url", new XAttribute(ConfigurationGenerator.ForEnvironmentsAttribute, "test"),
                    new XAttribute(ConfigurationGenerator.WithVarsAttribute, "true"),
                    "{base-url}"),
                new XElement("base-url", new XAttribute(ConfigurationGenerator.ForEnvironmentsAttribute, "dev"), "dev.com")));

            var output = Generator.GenerateConfig(xml, "test", new Dictionary<string, string> {{"base-url", "test.com"}});

            var expectedOutput = new XDocument(new XElement("config",
                new XElement("base-url", "test.com")));

            System.Console.WriteLine(output);

            Assert.That(output.ToString(), Is.EqualTo(expectedOutput.ToString()));
        }
    }
}