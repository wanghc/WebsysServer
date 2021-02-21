using System.Collections.Generic;
using System.Xml;
namespace WebsysServer
{
    public class MimeSettings : System.Configuration.IConfigurationSectionHandler
    {
        public Dictionary<string,string> mimeMappings = new Dictionary<string,string>();
        public object Create(object parent, object configContext, XmlNode section)
        {
            MimeSettings mimeSettings = new MimeSettings();
            if (section == null) return mimeSettings;
            foreach (XmlNode node in section.ChildNodes)
            {
                if (node.ChildNodes.Count > 1)
                {
                    mimeSettings.mimeMappings.Add(node.ChildNodes[0].InnerText, node.ChildNodes[1].InnerText);
                }
                // extension mime-type
            }
            return mimeSettings;
            throw new System.NotImplementedException();
        }
    }
}