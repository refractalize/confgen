using System.Xml.Linq;

namespace Confgen {
    class XmlLoaderSaver : IXmlLoaderSaver {
        public XDocument Load(string filename) {
            return XDocument.Load(filename);
        }

        public void Save(XDocument doc, string filename) {
            doc.Save(filename);
        }
    }
}