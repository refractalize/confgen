using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Confgen
{
    internal class ConfigurationGenerator
    {
        private readonly XElement _configurationXml;
        public static XNamespace Namespace = XNamespace.Get("http://schemas.refractalize.org/confgen");
        public static XName ForEnvironmentsAttribute = Namespace + "for";
        public static XName VarAttribute = Namespace + "var";
        public static XName WithVarsAttribute = Namespace + "with-vars";

        public ConfigurationGenerator(XElement configurationXml)
        {
            _configurationXml = configurationXml;
        }

        public ConfigurationGenerator()
        {
        }

        public IEnumerable<string> Environments
        {
            get { return GetEnvironments(_configurationXml); }
        }

        public XElement BuildConfigForEnvironment(XElement config, string environment, IDictionary<string, string> variables, HashSet<string> allEnvironments) {
            return BuildConfigForEnvironment(config, environment, allEnvironments, false, new TextVariablesFrame(variables));
        }

        private XElement BuildConfigForEnvironment(XElement config, string environment, HashSet<string> allEnvironments, bool replace, ITextVariables outerVariables) {
            replace = ReplaceForThisConfig(config, replace);

            var output = new XElement(config.Name);
            var variables = outerVariables.InnerFrame;

            CopyAttributes(config, variables, replace, output);
            CopyElements(config, environment, allEnvironments, replace, variables, output);

            return output;
        }

        private bool ReplaceForThisConfig(XElement config, bool replace) {
            XAttribute withVarsAttribute = config.Attribute(WithVarsAttribute);

            if (withVarsAttribute != null) {
                replace = withVarsAttribute.Value.ToLower() == "true";
            }
            return replace;
        }

        private void CopyElements(XElement config, string environment, HashSet<string> allEnvironments, bool replace, ITextVariables variables, XElement output) {
            foreach (XNode node in config.Nodes()) {
                XNode nodeToAdd = GenerateNode(node, environment, allEnvironments, variables, replace);

                if (nodeToAdd != null) {
                    output.Add(nodeToAdd);
                }
            }
        }

        private void CopyAttributes(XElement config, ITextVariables variables, bool replace, XElement output) {
            foreach (XAttribute attr in config.Attributes()) {
                if (!IsSpecialAttribute(attr)) {
                    string value = GenerateText(attr.Value, replace, variables);
                    output.Add(new XAttribute(attr.Name, value));
                }
            }
        }

        private static string GenerateText(string text, bool replace, ITextVariables variables) {
            if (replace) {
                var replacer = new TextReplacer();
                return replacer.ReplaceVariables(text, variables);
            } else {
                return text;
            }
        }

        private XNode GenerateNode(XNode node, string environment, HashSet<string> allEnvironments, ITextVariables variables, bool replace) {
            var element = node as XElement;
            if (element != null) {
                if (IsElementForEnvironment(element, environment, allEnvironments)) {
                    if (IsVarElement(element)) {
                        SetVariable(element, replace, variables);
                        return null;
                    } else {
                        return BuildConfigForEnvironment(element, environment, allEnvironments, replace, variables);
                    }
                } else {
                    return null;
                }
            }

            var text = node as XText;
            if (text != null) {
                if (replace) {
                    var replacer = new TextReplacer();
                    return new XText(replacer.ReplaceVariables(text.Value, variables));
                } else {
                    return node;
                }
            }

            return node;
        }

        private void SetVariable(XElement element, bool replace, ITextVariables variables)
        {
            var name = element.Attributes().First(a => a.Name.LocalName == "name");
            if (name != null)
            {
                var withVars = ReplaceForThisConfig(element, replace);
                var value = GenerateText(element.Value, withVars, variables);
                variables[name.Value] = value;
            }
        }

        private bool IsVarElement(XElement element) {
            return element.Name == VarAttribute;
        }

        private static bool IsElementForEnvironment(XElement element, string environment, HashSet<string> allEnvironments) {
            var environments = GetConfigEnvironments(element);

            if (environments != null) {
                CheckEnvironmentsAreInAllEnvironments(environments, allEnvironments);
                return environments.Contains(environment);
            } else {
                return true;
            }
        }

        private static void CheckEnvironmentsAreInAllEnvironments(List<string> environments, HashSet<string> allEnvironments) {
            foreach (string environment in environments) {
                if (!allEnvironments.Contains(environment)) {
                    throw new NoSuchEnvironmentException(String.Format("environment `{0}' not found in top level environments attribute, is this a typo?", environment));
                }
            }
        }

        private static bool IsSpecialAttribute(XAttribute attr) {
            return attr.Name.Namespace == Namespace
                   || attr.IsNamespaceDeclaration && attr.Value == Namespace.NamespaceName;
        }

        static List<string> GetConfigEnvironments(XElement element) {
            var forEnvironmentAttribute = element.Attribute(ForEnvironmentsAttribute);

            if (forEnvironmentAttribute != null) {
                return ParseEnvironments(forEnvironmentAttribute);
            } else {
                return null;
            }
        }

        public static List<string> ParseEnvironments(XAttribute environmentsAttribute) {
            return environmentsAttribute.Value.Split(',').Select(s => s.Trim()).ToList();
        }

        internal static List<string> GetEnvironments(XElement element) {
            var environmentsAttribute = element.Attribute(Console.BuildConfigForAttribute);

            if (environmentsAttribute != null) {
                return ParseEnvironments(environmentsAttribute);
            } else {
                throw new ConfgenException("expected 'environments' attribute with comma delimited list of environments to generate config files for");
            }
        }

        public XElement ConfigForEnvironment(string environment, IDictionary<string, string> variables = null)
        {
            return BuildConfigForEnvironment(_configurationXml, environment, variables, new HashSet<string>(GetEnvironments(_configurationXml)));
        }
    }
}
