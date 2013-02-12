using System;

namespace Confgen
{
    internal class ConfgenException : Exception {
        public ConfgenException(string message) : base (message) {}
    }
}