using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Core;
using System.Configuration;
//using StarBee_Printing_System.App_Start;

namespace StarBee_Printing_System
{
    public class s
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("First Name")]
        public string FirstName { get; set; }

        [BsonElement("Last Name")]
        public string LastName { get; set; }

        [BsonElement("Address")]
        public string CustomerAddress { get; set; }

        [BsonElement("Email Address")]
        public string Email { get; set; }

        [BsonElement("Phone Number")]
        public string PhoneNumber { get; set; }

        [BsonElement("Business Name")]
        public string BusinessName { get; set; }

        [BsonElement("Status")]
        public string Status { get; set; }

        [BsonElement("Password")]
        public string Password { get; set; }

        [BsonElement("Salt")]
        public string Salt { get; set; }

        [BsonElement("created_at")]
        public DateTime Created_at { get; set; }

        [BsonElement("updated_at")]
        public DateTime Updated_at { get; set; }


    }
}