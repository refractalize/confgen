using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Xml.Linq;

namespace confgen {
    public class Console {
        public static string MasterConfigExtension = ".master.config";
        public static XName BuildConfigForAttribute = ConfigurationGenerator.Namespace + "environments";
        public const string DefaultEnvironment = "dev";

        public void BuildConfigFilesFromMaster(string masterConfigFilename) {
            BuildConfigFilesFromMaster(masterConfigFilename, new XmlLoaderSaver());
        }

        internal void BuildConfigFilesFromMaster(string masterConfigFilename, IXmlLoaderSaver xmlLoaderSaver)
        {
            XDocument configDocument = xmlLoaderSaver.Load(masterConfigFilename);

            string baseConfigFilename = GetBaseConfigFilename(masterConfigFilename);

            var configuration = configDocument.Root;
            var generator = new ConfigurationGenerator(configuration);

            foreach (string environment in generator.Environments) {
                string outputFilename = GetEnvironmentOutputFilename(baseConfigFilename, environment);

                XDocument environmentConfig = new XDocument(generator.ConfigForEnvironment(environment));

                xmlLoaderSaver.Save(environmentConfig, outputFilename);
            }
        }

        public void BuildConfigFilesFromMaster(string masterConfigFilename, string outputConfigFilename, string env)
        {
            BuildConfigFilesFromMaster(masterConfigFilename, outputConfigFilename, env, new XmlLoaderSaver());
        }

        internal void BuildConfigFilesFromMaster(string masterConfigFilename, string outputConfigFilename, string env, IXmlLoaderSaver xmlLoaderSaver)
        {
            XDocument configDocument = xmlLoaderSaver.Load(masterConfigFilename);
            var confgen = new ConfigurationGenerator(configDocument.Root);
            var environmentConfig = new XDocument(confgen.ConfigForEnvironment(env));
            xmlLoaderSaver.Save(environmentConfig, outputConfigFilename);
        }

        internal static List<string> GetEnvironments(XElement element) {
            var environmentsAttribute = element.Attribute(BuildConfigForAttribute);

            if (environmentsAttribute != null) {
                return ConfigurationGenerator.ParseEnvironments(environmentsAttribute);
            } else {
                throw new ConfgenException("expected 'environments' attribute with comma delimited list of environments to generate config files for");
            }
        }

        private static string GetBaseConfigFilename(string masterConfigFilename) {
            if (masterConfigFilename.EndsWith(MasterConfigExtension)) {
                return masterConfigFilename.Substring(0, masterConfigFilename.Length - MasterConfigExtension.Length) + ".config";
            } else {
                throw new ConfgenException(String.Format("expected master config file `{0}' to have {1} extension", masterConfigFilename, MasterConfigExtension));
            }
        }

        private static string GetEnvironmentOutputFilename(string baseConfigFilename, string environment) {
            if (environment == DefaultEnvironment) {
                return baseConfigFilename;
            } else
            {
                return Path.Combine(Path.GetDirectoryName(baseConfigFilename),
                                    Path.GetFileNameWithoutExtension(baseConfigFilename) +
                                    "." + environment +
                                    Path.GetExtension(baseConfigFilename));
            }
        }
    }
}