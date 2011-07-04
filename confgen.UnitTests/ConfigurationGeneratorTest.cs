using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NUnit.Framework;

namespace confgen.UnitTests
{
    [TestFixture]
    public class ConfigurationGeneratorTest
    {
        [Test]
        public void ShouldGenerateIdenticalXml() {
            XElement config = new XElement("stuff",
                new XAttribute(Console.BuildConfigForAttribute, "dev, systest1, systest2"),
                new XElement("dome", new XAttribute("name", "7")),
                new XElement("hill", new XElement ("speed", "some text")));

            XElement expectedConfig = new XElement("stuff",
                new XElement("dome", new XAttribute("name", "7")),
                new XElement("hill", new XElement ("speed", "some text")));

            var confgen = new ConfigurationGenerator(config);
            XElement generated = confgen.ConfigForEnvironment("someenv");

            Assert.AreEqual(expectedConfig.ToString(), generated.ToString());
        }

        [Test]
        public void ShouldSkipElementsNotForCurrentEnvironment()
        {
            XElement config = new XElement("stuff",
                new XAttribute(Console.BuildConfigForAttribute, "live,systest"),
                new XElement("dome", new XAttribute("name", "7")),
                new XElement("hill", new XElement("speed", new XAttribute(ConfigurationGenerator.ForEnvironmentsAttribute, "systest"), "some text")));

            var confgen = new ConfigurationGenerator(config);
            XElement generated = confgen.ConfigForEnvironment("live");

            XElement expected = new XElement("stuff",
                new XElement("dome", new XAttribute("name", "7")),
                new XElement("hill"));
            Assert.AreEqual(expected.ToString(), generated.ToString());
        }

        [Test]
        public void ShouldIncludeElementsForCurrentEnvironment()
        {
            XElement config = new XElement("stuff",
                new XAttribute(Console.BuildConfigForAttribute, "live,systest"),
                new XElement("dome", new XAttribute("name", "7")),
                new XElement("hill", new XElement("speed", new XAttribute(ConfigurationGenerator.ForEnvironmentsAttribute, "systest"), "some text")));

            var confgen = new ConfigurationGenerator(config);
            XElement generated = confgen.ConfigForEnvironment("systest");

            XElement expected = new XElement("stuff",
                new XElement("dome", new XAttribute("name", "7")),
                new XElement("hill", new XElement("speed", "some text")));
            Assert.AreEqual(expected.ToString(), generated.ToString());
        }

        [Test]
        public void ShouldFailIfElementForUnspecifiedEnvironment()
        {
            XElement config = new XElement("stuff",
                new XAttribute(Console.BuildConfigForAttribute, "live,nosystest"),
                new XElement("dome", new XAttribute("name", "7")),
                new XElement("hill", new XElement("speed", new XAttribute(ConfigurationGenerator.ForEnvironmentsAttribute, "systest"), "some text")));

            try {
                var confgen = new ConfigurationGenerator(config);
                confgen.ConfigForEnvironment("live");
                Assert.Fail("expected configuration exception");
            } catch (NoSuchEnvironmentException) {
            }
        }

        [Test]
        public void ShouldExtractEnvironmentsForGeneration() {
            XElement config = new XElement("stuff",
                new XAttribute(Console.BuildConfigForAttribute, "dev, systest1, systest2"),
                new XElement("dome", new XAttribute("name", "7")),
                new XElement("hill", new XElement("speed", "some text")));

            var confgen = new ConfigurationGenerator(config);
            var environments = confgen.Environments;

            Assert.IsTrue(environments.Contains("dev"));
            Assert.IsTrue(environments.Contains("systest1"));
            Assert.IsTrue(environments.Contains("systest2"));
        }

        [Test]
        public void ShouldReplaceVariablesIfReplaceIsTrue()
        {
            AssertOutputForInput(
                @"<stuff xmlns:conf=""http://schemas.bbc.com/development/confgen"" conf:environments=""dev"">
	                <conf:var name=""host"">example.com</conf:var>
	                <morestuff1 conf:with-vars=""true"">http://{host}/blah/blah</morestuff1>
	                <morestuff2 conf:with-vars=""false"">http://{host}/blah/blah</morestuff2>
	                <morestuff3>http://{host}/blah/blah</morestuff3>
	                <morestuff4 conf:with-vars=""true"" attr=""{host}"">
	                    <yob attr=""{host}"" conf:with-vars=""false"">
                            one{host}two
                        </yob>
	                    <gab attr=""three{host}four"">
                            one{host}two
                        </gab>
                    </morestuff4>
                </stuff>",

                "dev",

                @"<stuff>
	                <morestuff1>http://example.com/blah/blah</morestuff1>
	                <morestuff2>http://{host}/blah/blah</morestuff2>
	                <morestuff3>http://{host}/blah/blah</morestuff3>
	                <morestuff4 attr=""example.com"">
	                    <yob attr=""{host}"">
                            one{host}two
                        </yob>
	                    <gab attr=""threeexample.comfour"">
                            oneexample.comtwo
                        </gab>
                    </morestuff4>
                </stuff>"
            );
        }

        [Test]
        public void VariablesShouldAllowVariablesToo()
        {
            AssertOutputForInput(
                @"<stuff xmlns:conf=""http://schemas.bbc.com/development/confgen"" conf:environments=""dev"">
	                <conf:var name=""env"">test{notvar}</conf:var>
	                <conf:var name=""host"" conf:with-vars=""true"">{env}.example.com</conf:var>
	                <stuff conf:with-vars=""true"">http://{host}/blah/blah</stuff>
                </stuff>",

                "dev",

                @"<stuff>
	                <stuff>http://test{notvar}.example.com/blah/blah</stuff>
                </stuff>"
            );
        }

        private void AssertOutputForInput(string input, string env, string output)
        {
            XElement config = XElement.Parse(input);

            var confgen = new ConfigurationGenerator(config);
            var generated = confgen.ConfigForEnvironment(env);

            XElement expected = XElement.Parse(output);

            Assert.That(generated.ToString(), Is.EqualTo(expected.ToString()));
        }

        [Test]
        public void VariablesShouldBeSetOnlyForEnvironment() {
            var xml =
@"<stuff xmlns:conf=""http://schemas.bbc.com/development/confgen"" conf:with-vars=""true"" conf:environments=""dev,live"">
	<conf:var name=""host"" conf:for=""dev"">localhost</conf:var>
	<conf:var name=""host"" conf:for=""live"">example.com</conf:var>
	<host value=""{host}""/>
</stuff>";

            XElement config = XElement.Parse(xml);

            var confgen = new ConfigurationGenerator(config);
            var devGenerated = confgen.ConfigForEnvironment("dev");
            var liveGenerated = confgen.ConfigForEnvironment("live");

            var devXml =
@"<stuff>
	<host value=""localhost""/>
</stuff>";
            var liveXml =
@"<stuff>
	<host value=""example.com""/>
</stuff>";

            Assert.That(devGenerated.ToString(), Is.EqualTo(XElement.Parse(devXml).ToString()));
            Assert.That(liveGenerated.ToString(), Is.EqualTo(XElement.Parse(liveXml).ToString()));
        }

    }
}
