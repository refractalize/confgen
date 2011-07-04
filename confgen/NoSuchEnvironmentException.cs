namespace confgen
{
    public class NoSuchEnvironmentException : ConfgenException {
        public NoSuchEnvironmentException(string message) : base (message) {}
    }
}