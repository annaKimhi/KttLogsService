using System;
using System.Collections;
using System.Configuration;

namespace KTT.Config
{
    public class SyncConfig : ConfigurationSection
    {
        private static ConfigurationPropertyCollection _Properties;
        static ConfigurationProperty _ATT_IPProperty;
        static ConfigurationProperty _ATT_PortProperty;
        static ConfigurationProperty _KTT_URIProperty;


        static SyncConfig()
        {
            _ATT_IPProperty = new ConfigurationProperty("ATT_IP", typeof(string), "192.168.1.7", null, null, ConfigurationPropertyOptions.IsRequired, "last time of synchonization");
            _ATT_PortProperty = new ConfigurationProperty("ATT_Port", typeof(string), "4370", null, null, ConfigurationPropertyOptions.IsRequired, "last time of synchonization");
            _KTT_URIProperty = new ConfigurationProperty("KTT_URI", typeof(string), "http://www.korentec.co.il/kttdebug", null, null, ConfigurationPropertyOptions.IsRequired, "last time of synchonization");
            
            // Initialize the Property collection.
            _Properties = new ConfigurationPropertyCollection();
            _Properties.Add(_ATT_IPProperty);
            _Properties.Add(_ATT_PortProperty);
            _Properties.Add(_KTT_URIProperty);
        }

        // Return the initialized property bag
        // for the configuration element.
        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _Properties;
            }
        }

        // Clear the property.
        public void ClearCollection()
        {
            Properties.Clear();
        }

        // Remove an element from the property collection.
        public void RemoveCollectionElement(string elName)
        {
            Properties.Remove(elName);
        }

        // Get the property collection enumerator.
        public IEnumerator GetCollectionEnumerator()
        {
            return (Properties.GetEnumerator());
        }


        public string ATT_IP
        {
            get { return (string)this["ATT_IP"]; }
            set { this["ATT_IP"] = value; }
        }

        
        public int ATT_Port
        {
            get { return int.Parse((string)this["ATT_Port"]); }
            set { this["ATT_Port"] = value.ToString(); }
        }

        
        public Uri KTT_URI
        {
            get { return new Uri((string)this["KTT_URI"]); }
            set { this["KTT_URI"] = value.ToString(); }
        }
    }
}
