using System.Xml.Linq;

namespace confgen {
    public interface IXmlLoaderSaver {
        XDocument Load(string filename);
        void Save(XDocument doc, string filename);
    }
}