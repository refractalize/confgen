using System.Xml.Linq;
using Moq;
using NUnit.Framework;

namespace confgen.UnitTests
{
    [TestFixture]
    public class ConsoleTest
    {
        [Test]
        public void ShouldLoadMasterConfigAndSaveOneForEachEnvironment() {
            XElement config = new XElement("stuff",
                                           new XAttribute(Console.BuildConfigForAttribute, "dev, systest1, systest2"),
                                           new XElement("dome", new XAttribute("name", "7")),
                                           new XElement("hill", new XElement("speed", new XAttribute(ConfigurationGenerator.ForEnvironmentsAttribute, "systest1"), "some text")),
                                           new XElement("development", new XAttribute(ConfigurationGenerator.ForEnvironmentsAttribute, Console.DefaultEnvironment), 4));

            XElement expectedDev = new XElement("stuff",
                                                new XElement("dome", new XAttribute("name", "7")),
                                                new XElement("hill"),
                                                new XElement("development", 4));
            XElement expectedSystest1 = new XElement("stuff",
                                                     new XElement("dome", new XAttribute("name", "7")),
                                                     new XElement("hill", new XElement("speed", "some text")));
            XElement expectedSystest2 = new XElement("stuff",
                                                     new XElement("dome", new XAttribute("name", "7")),
                                                     new XElement("hill"));

            Mock<IXmlLoaderSaver> diskAccessMock = new Mock<IXmlLoaderSaver>();
            string masterFileName = "app.master.config";
            diskAccessMock.Setup(ls => ls.Load(masterFileName)).Returns(new XDocument(config));

            XDocument devDoc = null, systest1Doc = null, systest2Doc = null;

            diskAccessMock.Setup(ls => ls.Save(It.IsAny<XDocument>(), "app.config")).Callback<XDocument, string>((doc, fn) => devDoc = doc);
            diskAccessMock.Setup(ls => ls.Save(It.IsAny<XDocument>(), "app.systest1.config")).Callback<XDocument, string>((doc, fn) => systest1Doc = doc);
            diskAccessMock.Setup(ls => ls.Save(It.IsAny<XDocument>(), "app.systest2.config")).Callback<XDocument, string>((doc, fn) => systest2Doc = doc);

            var console = new Console();
            console.BuildConfigFilesFromMaster(masterFileName, diskAccessMock.Object);

            Assert.AreEqual(expectedDev.ToString(), devDoc.ToString());
            Assert.AreEqual(expectedSystest1.ToString(), systest1Doc.ToString());
            Assert.AreEqual(expectedSystest2.ToString(), systest2Doc.ToString());
        }

        [Test]
        public void ShouldCreateConfigFileForNamedEnv() {
            var masterConfig = new XElement("stuff",
                                            new XAttribute(Console.BuildConfigForAttribute, "dev, systest, uat"),
                                            new XElement("dome", new XAttribute("name", "7")),
                                            new XElement("hill", new XElement("speed", new XAttribute(ConfigurationGenerator.ForEnvironmentsAttribute, "systest"), "some text")),
                                            new XElement("development", new XAttribute(ConfigurationGenerator.ForEnvironmentsAttribute, Console.DefaultEnvironment), 4));

            var config = new XElement("stuff",
                                      new XElement("dome", new XAttribute("name", "7")),
                                      new XElement("hill", new XElement("speed", "some text")));

            var diskAccessMock = new Mock<IXmlLoaderSaver>();
            const string masterFileName = "app.master.config";
            diskAccessMock.Setup(ls => ls.Load(masterFileName)).Returns(new XDocument(masterConfig));

            XDocument exportedConfig = null;

            diskAccessMock.Setup(ls => ls.Save(It.IsAny<XDocument>(), "the.config")).Callback<XDocument, string>((doc, fn) => exportedConfig = doc);

            var console = new Console();
            console.BuildConfigFilesFromMaster(masterFileName, "the.config", "systest", diskAccessMock.Object);

            Assert.That(exportedConfig, Is.Not.Null);
            Assert.AreEqual(config.ToString(), exportedConfig.ToString());
        }
    }
}