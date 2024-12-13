using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Web;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Xml.Serialization;

namespace SOMIOD.Models
{
    public abstract class Entity
    {

        public int Id { get; set; } 

 
        public string Name { get; set; }

        public DateTime CreationDateTime { get; set; }
    }
}