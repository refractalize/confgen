using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Confgen {
    internal class TextReplacer {
        public string ReplaceVariables(string text, ITextVariables variables) {
            var output = new StringWriter();
            new ParserReplacer(text.ToCharArray(), variables, output).ParseText(0);
            return output.ToString();
        }

        class ParserReplacer {
            private char[] text;
            private ITextVariables variables;
            private TextWriter output;

            public ParserReplacer(char[] text, ITextVariables variables, TextWriter output) {
                this.text = text;
                this.variables = variables;
                this.output = output;
            }

            public void ParseText(int startIndex) {
                int index = 0;

                index = startIndex;

                while (true) {
                    if (index >= text.Length) {
                        break;
                    }

                    if (NextTwoCharactersAre(index, '{', '{')) {
                        WriteSubstring(startIndex, index);
                        output.Write('{');
                        index += 2;
                        startIndex = index;
                    } else if (NextTwoCharactersAre(index, '}', '}')) {
                        WriteSubstring(startIndex, index);
                        output.Write('}');
                        index += 2;
                        startIndex = index;
                    } else if (text[index] == '{') {
                        WriteSubstring(startIndex, index);
                        index = ReplaceVariable(index + 1);
                        startIndex = index;
                    } else {
                        index++;
                    }
                }

                WriteSubstring(startIndex, index);
            }

            private bool NextTwoCharactersAre(int index, char first, char second) {
                if (text[index] == first) {
                    index++;

                    if (index >= text.Length) {
                        return false;
                    }

                    if (text[index] == second) {
                        return true;
                    }
                }

                return false;
            }

            private void WriteSubstring(int fromIndex, int toIndex) {
                output.Write(text, fromIndex, toIndex - fromIndex);
            }

            private int ReplaceVariable(int startIndex) {
                int index = startIndex;

                while (true) {
                    if (index >= text.Length) {
                        if (index > startIndex) {
                            throw new ParseException(String.Format("{0}: expected closing bracket for variable", index));
                        } else {
                            output.Write('{');
                            return index;
                        }
                    } else if (text[index] == '}') {
                        break;
                    } else {
                        index++;
                    }
                }

                string variableName = GetSubstring(startIndex, index);
                output.Write(variables[variableName]);

                index++;

                return index;
            }

            private string GetSubstring(int fromIndex, int toIndex) {
                return new string(text, fromIndex, toIndex - fromIndex);
            }
        }
    }

    internal class ParseException : ConfgenException {
        public ParseException(string message) : base(message) {
        }
    }
}