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


        private MongoClient Authenticate()
        {
            MongoCredential credential = MongoCredential.CreateCredential("admin", "somiod_user", ",$0m1O3");
            MongoClientSettings settings = MongoClientSettings.FromConnectionString(databaseURL);

            settings.Credential = credential;

            MongoClient mongoClient = null;

            try
            {
                mongoClient = new MongoClient(settings);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


            return mongoClient;

        }


        // GET: api/Application
        public IHttpActionResult Get()
        {
            var mongoClient = Authenticate();
            if (mongoClient == null)
            {
                // Assuming the password was right, the only reason for this to fail is if the connection to the database fails
                return StatusCode(HttpStatusCode.InternalServerError);
            }

            Console.Write("connected");
            var database = mongoClient.GetDatabase("somiod");
            var collection = database.GetCollection<Application>("applications");
            var applications = collection.Find(_ => true).ToList();


            return Ok(applications);
        }

        // GET: api/somioid/Application/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/somioid/
        public IHttpActionResult Post([FromBody]Application application)
        {
            var mongoClient = Authenticate();
            if (mongoClient == null)
            {
                // Assuming the password was right, the only reason for this to fail is if the connection to the database fails
                return StatusCode(HttpStatusCode.InternalServerError);
            }
            Console.Write("connected");
            var database = mongoClient.GetDatabase("somiod");
            var collection = database.GetCollection<Application>("applications");
            var applicationToInsert = new Application
            {
                id = application.id,
                name = application.name,
                creation_datetime = DateTime.Now
            };

            try
            {
                collection.InsertOne(applicationToInsert);
            }
            catch (Exception)
            {
                return StatusCode(HttpStatusCode.BadRequest);
            }

            return Ok();
        }

        // PUT: api/somiod/Application/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/somiod/Application/5
        public void Delete(int id)
        {
        }
    }
}
