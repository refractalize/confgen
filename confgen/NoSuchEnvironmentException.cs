namespace Confgen
{
    internal class NoSuchEnvironmentException : ConfgenException {
        public NoSuchEnvironmentException(string message) : base (message) {}
    }
}