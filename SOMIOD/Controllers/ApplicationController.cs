using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MongoDB.Driver;
using SOMIOD.Models;


namespace SOMIOD.Controllers
{
    public class ApplicationController : ApiController
    {
        const string databaseURL = "mongodb://mongodb.cloud-ss.pt:27017";
        // GET: api/Application
        public IHttpActionResult Get()
        {
            var client = new MongoClient(databaseURL);
            Console.Write("connected");
            var database = client.GetDatabase("somiod");
            var collection = database.GetCollection<Application>("applications");
            var applications = collection.Find(_ => true).ToList();


            return Ok(applications);
        }

        // GET: api/Application/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Application
        public IHttpActionResult Post([FromBody]Application application)
        {
            var client = new MongoClient(databaseURL);
            Console.Write("connected");
            var database = client.GetDatabase("somiod");
            var collection = database.GetCollection<Application>("applications");
            var applicationToInsert = new Application
            {
                id = application.id,
                name = application.name,
                creation_datetime = DateTime.Now
            };
            collection.InsertOne(applicationToInsert);

            return Ok();
        }

        // PUT: api/Application/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Application/5
        public void Delete(int id)
        {
        }
    }
}
