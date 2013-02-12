using System.Collections.Generic;
using Moq;
using NUnit.Framework;

namespace Confgen.UnitTests {
    [TestFixture]
    public class TextReplacerTest {
        [Test]
        public void ShouldReplaceVariablesWithValues() {
            var replacer = new TextReplacer();
            var variables = new Mock<ITextVariables>();
            variables.Setup(v => v["host"]).Returns("example.com");
            Assert.That(replacer.ReplaceVariables("http://{host}/blah/blah", variables.Object), Is.EqualTo("http://example.com/blah/blah"));
        }

        [Test]
        public void ShouldReturnOriginalTextIfNoVariables() {
            var replacer = new TextReplacer();
            var variables = new Mock<ITextVariables>();
            variables.Setup(v => v["host"]).Returns("example.com");
            Assert.That(replacer.ReplaceVariables("http://example.com/blah/blah", variables.Object), Is.EqualTo("http://example.com/blah/blah"));
        }

        [Test]
        public void ShouldEscapeDoubleBraces() {
            var replacer = new TextReplacer();
            var variables = new Mock<ITextVariables>();
            variables.Setup(v => v["host"]).Returns("example.com");
            Assert.That(replacer.ReplaceVariables("http://{{host}}/blah/blah", variables.Object), Is.EqualTo("http://{host}/blah/blah"));
            Assert.That(replacer.ReplaceVariables("http://{{host}", variables.Object), Is.EqualTo("http://{host}"));
            Assert.That(replacer.ReplaceVariables("http://{", variables.Object), Is.EqualTo("http://{"));
            Assert.That(replacer.ReplaceVariables("http://}}", variables.Object), Is.EqualTo("http://}"));
            Assert.That(replacer.ReplaceVariables("http://{{", variables.Object), Is.EqualTo("http://{"));
            Assert.That(replacer.ReplaceVariables("http://{{{host}}}/blah/blah", variables.Object), Is.EqualTo("http://{example.com}/blah/blah"));
            Assert.That(replacer.ReplaceVariables("{{http://host/blah/blah", variables.Object), Is.EqualTo("{http://host/blah/blah"));
        }

        [Test]
        public void ShouldThrowExceptionIfVariableNotComplete() {
            var replacer = new TextReplacer();
            var variables = new Mock<ITextVariables>();
            Assert.That(() => replacer.ReplaceVariables("http://{host", variables.Object), Throws.InstanceOf(typeof (ParseException)));
        }
    }
}