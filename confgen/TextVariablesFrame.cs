using System;
using System.Collections.Generic;

namespace Confgen {
    internal class TextVariablesFrame : ITextVariables {
        private readonly ITextVariables outerFrame;
        private Dictionary<string, string> variables;

        public class NoTextVariables : ITextVariables {
            public string this[string name] {
                get {
                    throw new NoSuchVariableException(String.Format("no such variable: `{0}'", name));
                }
            }
        }

        public TextVariablesFrame() : this(new NoTextVariables()) {
        }

        public TextVariablesFrame(ITextVariables outerFrame) {
            variables = new Dictionary<string, string>();
            this.outerFrame = outerFrame;
        }

        public string this[string name] {
            get {
                string value;
                if (variables.TryGetValue(name, out value)) {
                    return value;
                } else {
                    return outerFrame[name];
                }
            }
            set {
                variables[name] = value;
            }
        }
    }
}