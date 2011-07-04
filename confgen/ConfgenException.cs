using System;

namespace confgen
{
    public class ConfgenException : Exception {
        public ConfgenException(string message) : base (message) {}
    }
}