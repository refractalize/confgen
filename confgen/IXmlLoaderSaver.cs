using System.Xml.Linq;

namespace Confgen {
    internal interface IXmlLoaderSaver {
        XDocument Load(string filename);
        void Save(XDocument doc, string filename);
    }
}