using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace SOMIOD.Models
{
    [XmlRoot("Record")]
    public class Record
    {
        public string content { get; set; }
        public DateTime creation_datetime { get; set; }
    }
}