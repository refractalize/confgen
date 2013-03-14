using System;
using System.Collections.Generic;

namespace Confgen {
    internal class TextVariablesFrame : ITextVariables {
        private readonly ITextVariables outerFrame;
        private IDictionary<string, string> variables;

        public class NoTextVariables : ITextVariables {
            public string this[string name] {
                get {
                    throw new NoSuchVariableException(String.Format("no such variable: `{0}'", name));
                }
                set {
                    throw new NotImplementedException();
                }
            }

            public ITextVariables InnerFrame {
                get { throw new NotImplementedException(); }
            }
        }

        public TextVariablesFrame() : this(new Dictionary<string, string>(), new NoTextVariables()) {
        }

        public TextVariablesFrame(ITextVariables outerFrame) : this(new Dictionary<string, string>(), outerFrame) {
        }

        public TextVariablesFrame(IDictionary<string, string> variables)
            : this(variables ?? new Dictionary<string, string>(), new NoTextVariables())
        {
        }

        public ITextVariables InnerFrame {
            get { return new TextVariablesFrame(this); }
        }

        public TextVariablesFrame(IDictionary<string, string> variables, ITextVariables outerFrame) {
            this.variables = variables;
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