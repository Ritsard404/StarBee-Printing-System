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
    public class CustomerModel
    {
        private MongoClient mongoClient;
        private IMongoCollection<Customer> customerCollection;

        public CustomerModel()
        {
            mongoClient = new MongoClient(ConfigurationManager.AppSettings["mongoDBHost"]);
            var db = mongoClient.GetDatabase(ConfigurationManager.AppSettings["monngoDBName"]);
            customerCollection = db.GetCollection<Customer>("customer");
        }

        public List<Customer> findAll()
        {
            var sortedCustomers = customerCollection.Find(Builders<Customer>.Filter.Ne(c => c.Status, "Inactive"))
                                                     .Sort(Builders<Customer>.Sort.Ascending(customer => customer.Status))
                                                     .ToList();

            return sortedCustomers;
        }
        
        public Customer CustomerAccount(string email)
        {
            var filter = Builders<Customer>.Filter.Eq("Status", "Active") & Builders<Customer>.Filter.Eq("Email", email);
            var document = customerCollection.Find(filter).FirstOrDefault();
            return document;
        }


        public Customer find(string id)
        {
            var customerId = new ObjectId(id);
            return customerCollection.AsQueryable<Customer>().SingleOrDefault(a => a.Id == customerId);
        }

        public void create(Customer customer)
        {
            customerCollection.InsertOne(customer);

        }

        public void resetPassword(string email, string newPass, string salt)
        {
            customerCollection.UpdateOne(
                Builders<Customer>.Filter.Eq("Email", email) & Builders<Customer>.Filter.Eq("Status", "Active"),
                Builders<Customer>.Update.Set("Password", newPass).Set("Salt", salt).Set("updated_at", DateTime.Now)
                );

        }
        public void DeleteAccount(string email)
        {
            customerCollection.UpdateOne(
                Builders<Customer>.Filter.Eq("Email", email) & Builders<Customer>.Filter.Eq("Status", "Active"),
                Builders<Customer>.Update.Set("Status", "Inactive").Set("updated_at", DateTime.Now)
                );

        }

        public void updateProfile(string email, string fName, string lName, string pNum, string bName, string address)
        {
            customerCollection.UpdateOne(
                Builders<Customer>.Filter.Eq("Email", email),
                Builders<Customer>.Update.Set("First Name", fName)
                .Set("Last Name", lName)
                .Set("Phone Number", pNum)
                .Set("Business Name", bName)
                .Set("Address", address)
                .Set("updated_at", DateTime.Now)
                );
        }

        public void suspendCustomer(string id)
        {

            customerCollection.UpdateOne(
                Builders<Customer>.Filter.Eq(c => c.Id, ObjectId.Parse(id)),
                Builders<Customer>.Update
                .Set(c => c.Status, "Suspend")
                .Set(c => c.Updated_at, DateTime.Now)
                );
        }
        public void unsuspendCustomer(string id)
        {

            customerCollection.UpdateOne(
                Builders<Customer>.Filter.Eq(c => c.Id, ObjectId.Parse(id)),
                Builders<Customer>.Update
                .Set(c => c.Status, "Active")
                .Set(c => c.Updated_at, DateTime.Now)
                );
        }
        public void deleteCustomer(string id)
        {

            customerCollection.UpdateOne(
                Builders<Customer>.Filter.Eq(c => c.Id, ObjectId.Parse(id)),
                Builders<Customer>.Update
                .Set(c => c.Status, "Inactive")
                .Set(c => c.Updated_at, DateTime.Now)
                );
        }
        public void delete(string id)
        {
            customerCollection.DeleteOne(Builders<Customer>.Filter.Eq("_id", ObjectId.Parse(id)));

        }
    }
}