using System.Collections.Generic;
using System.Xml.Linq;

namespace Confgen
{
    public class Generator
    {
        public static void GenerateConfig(string masterFilename, string outputFilename, string env)
        {
            new Console().BuildConfigFilesFromMaster(masterFilename, outputFilename, env);
        }

        public static XDocument GenerateConfig(XDocument masterXml, string env, IDictionary<string, string> variables = null)
        {
            return new Console().GenerateConfig(masterXml, env, variables);
        }
    }
}