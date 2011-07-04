using System;
using System.IO;
using System.Linq;

namespace confgen {
    public class Help {
        private readonly TextWriter output;

        public Help(TextWriter output) {
            this.output = output;
        }

        public void PrintHelp() {
            var helpStream = GetType().Assembly.GetManifestResourceStream(GetType(), "help.txt");

            if (helpStream == null) {
                throw new Exception("help not found in assembly!");
            }

            using (var stream = new StreamReader(helpStream)) {
                var helptext = stream.ReadToEnd();
                output.Write(helptext);
            }
        }

        public bool AreHelpArguments(string[] args) {
            return args.Any(a => a == "/?" || a.ToLower() == "/help");
        }
    }
}