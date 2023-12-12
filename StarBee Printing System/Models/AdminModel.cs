using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StarBee_Printing_System.Entities;
using MongoDB;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Configuration;


namespace StarBee_Printing_System.Models
{
    public class AdminModel
    {
        private MongoClient mongoClient;
        private IMongoCollection<AdminField> adminCollection;

        public AdminModel()
        {
            mongoClient = new MongoClient(ConfigurationManager.AppSettings["mongoDBHost"]);
            var db = mongoClient.GetDatabase(ConfigurationManager.AppSettings["monngoDBName"]);
            adminCollection = db.GetCollection<AdminField>("admin");
        }

        public List<AdminField> findAll()
        {
            var sortedAdmin = adminCollection.Find(Builders<AdminField>.Filter.Ne(c => c.Status, "Inactive"))
                                                     .Sort(Builders<AdminField>.Sort.Ascending(admin => admin.Status))
                                                     .ToList();

            return sortedAdmin;
        }

        public AdminField AdminAccount(string email)
        {
            var filter = Builders<AdminField>.Filter.Eq(admin => admin.Status, "Active") & Builders<AdminField>.Filter.Eq(admin => admin.Email, email);
            var document = adminCollection.Find(filter).FirstOrDefault();
            return document;
        }
        public void create(AdminField admin)
        {
            adminCollection.InsertOne(admin);

        }

        public void resetPassword(string email, string newPass, string salt)
        {
            adminCollection.UpdateOne(
                Builders<AdminField>.Filter.Eq("Email", email) & Builders<AdminField>.Filter.Eq("Status", "Active"),
                Builders<AdminField>.Update.Set("Password", newPass).Set("Salt", salt).Set("updated_at", DateTime.Now)
                );

        }
        public void updateProfile(string email, string fName, string lName, string pNum)
        {
            adminCollection.UpdateOne(
                Builders<AdminField>.Filter.Eq("Email", email) & Builders<AdminField>.Filter.Eq("Status", "Active"),
                Builders<AdminField>.Update.Set("First Name", fName)
                .Set("Last Name", lName)
                .Set("Phone Number", pNum)
                .Set("updated_at", DateTime.Now)
                );
        }
        public void DeleteAccount(string email)
        {
            adminCollection.UpdateOne(
                Builders<AdminField>.Filter.Eq("Email", email) & Builders<AdminField>.Filter.Eq("Status", "Active"),
                Builders<AdminField>.Update
                .Set("Status", "Inactive")
                .Set("updated_at", DateTime.Now)
                );
        }

        public void suspendAdmin(string id)
        {

            adminCollection.UpdateOne(
                Builders<AdminField>.Filter.Eq(c => c.Id, ObjectId.Parse(id)),
                Builders<AdminField>.Update
                .Set(c => c.Status, "Suspend")
                .Set(c => c.Updated_at, DateTime.Now)
                );
        }
        public void unsuspendAdmin(string id)
        {

            adminCollection.UpdateOne(
                Builders<AdminField>.Filter.Eq(c => c.Id, ObjectId.Parse(id)),
                Builders<AdminField>.Update
                .Set(c => c.Status, "Active")
                .Set(c => c.Updated_at, DateTime.Now)
                );
        }
        public void deleteAdmin(string id)
        {

            adminCollection.UpdateOne(
                Builders<AdminField>.Filter.Eq(c => c.Id, ObjectId.Parse(id)),
                Builders<AdminField>.Update
                .Set(c => c.Status, "Inactive")
                .Set(c => c.Updated_at, DateTime.Now)
                );
        }
    }

}