using System.IO;
using NUnit.Framework;

namespace confgen.UnitTests {
    [TestFixture]
    public class HelpTest {
        [Test]
        public void ShouldReturnTrueForHelpArguments() {
            var printer = new Help(null);
            AssertIsHelpArgument(printer, "/?");
            AssertIsHelpArgument(printer, "/help");
            AssertIsHelpArgument(printer, "/help", "another");
            AssertIsHelpArgument(printer, "/?", "another");

            AssertIsNotHelpArgument(printer, "/nothelp");
            AssertIsNotHelpArgument(printer, "somefile.xml.master");
            AssertIsNotHelpArgument(printer, "somefile.xml");
        }

        private void AssertIsNotHelpArgument(Help printer, params string [] args) {
            AssertIsHelpArgument(printer, false, args);
        }

        private void AssertIsHelpArgument(Help printer, params string [] args) {
            AssertIsHelpArgument(printer, true, args);
        }

        private void AssertIsHelpArgument(Help printer, bool isHelp, params string [] args) {
            Assert.That(printer.AreHelpArguments(args), Is.EqualTo(isHelp));
        }

        [Test]
        public void ShouldLoadHelpAndPrintToTextWriter() {
            var output = new StringWriter();
            var printer = new Help(output);

            printer.PrintHelp();

            Assert.That(output.ToString().Contains("confgen"));
        }
    }
}