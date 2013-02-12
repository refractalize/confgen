namespace Confgen
{
    internal class NoSuchVariableException : ConfgenException {
        public NoSuchVariableException(string message) : base(message) {
        }
    }
}