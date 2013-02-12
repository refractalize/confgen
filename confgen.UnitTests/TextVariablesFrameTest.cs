using NUnit.Framework;

namespace Confgen.UnitTests {
    [TestFixture]
    public class TextVariablesFrameTest {
        [Test]
        public void ShouldGetVariableInFirstFrame() {
            var frame = new TextVariablesFrame();
            frame["variableName"] = "variableValue";

            Assert.That(frame["variableName"], Is.EqualTo("variableValue"));
        }

        [Test]
        public void ShouldGetVariableInSecondFrame() {
            var outerFrame = new TextVariablesFrame();
            var frame = new TextVariablesFrame(outerFrame);

            outerFrame["variableName"] = "variableValue";

            Assert.That(frame["variableName"], Is.EqualTo("variableValue"));
        }

        [Test]
        public void ShouldThrowIfVariableNotFound() {
            var frame = new TextVariablesFrame();
            frame["variableName"] = "variableValue";

            Assert.That(() => frame["not_a_variable"], Throws.InstanceOf(typeof(NoSuchVariableException)));
        }
    }
}