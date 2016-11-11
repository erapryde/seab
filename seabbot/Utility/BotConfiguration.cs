using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace SeabBot.Utility
{
    [Serializable]
    [XmlRoot]
    public class MessageCollection
    {
        [XmlArray("Messages")]
        [XmlArrayItem("Message", typeof(Message))]
        public Message[] Message { get; set;  }
    }

    [Serializable]
    public class Message
    {
        [XmlAttribute("id")]
        public string ID { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}