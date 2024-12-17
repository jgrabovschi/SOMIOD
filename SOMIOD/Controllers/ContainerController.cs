using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MongoDB.Driver;
using MySql.Data.MySqlClient;
using SOMIOD.Models;


namespace SOMIOD.Controllers
{
    
    public class ContainerController : ApiController
    {
        const string databaseURL = "mongodb://mongodb.cloud-ss.pt:27017";

        //GET -h "somiod-locate: XXXX" api/somioid/Application/
        [HttpGet]
        [Route("api/somiod/{app}/{container}")]
        public IHttpActionResult get(string app, string container)
        {
            try
            {
                // Connect to MongoDB
                var client = new MongoClient(databaseURL);
                var database = client.GetDatabase("somiod");

                // Step 1: Query the Applications Collection to Get the Application ID
                var applicationsCollection = database.GetCollection<Application>("applications");
                var appFilter = Builders<Application>.Filter.Eq(a => a.Name, app);
                var application = applicationsCollection.Find(appFilter).FirstOrDefault();

                if (application == null)
                {
                    return NotFound(); // Application not found
                }

                // Step 2: Query the Containers Collection Using the Retrieved Application ID
                var containersCollection = database.GetCollection<Container>("containers");
                var containerFilter = Builders<Container>.Filter.Eq(c => c.parent, application.Id) &
                                      Builders<Container>.Filter.Eq(c => c.name, container);

                var result = containersCollection.Find(containerFilter).FirstOrDefault();

                if (result == null)
                {
                    return NotFound(); // Container not found
                }

                // Return the matching container
                return Ok(result);

            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
            
        }


        // POST: api/somioid/Application/
        public IHttpActionResult Post([FromBody] Container container)
        {
            var client = new MongoClient(databaseURL);
            Console.Write("connected");
            var database = client.GetDatabase("somiod");
            var collection = database.GetCollection<Container>("containers");
            var containerToInsert = new Container
            {
                id = container.id,
                name = container.name,
                creation_datetime = DateTime.Now,
                parent = container.parent,
            };
            collection.InsertOne(containerToInsert);

            return Ok();
        }

        // PUT: api/somiod/Application/Container/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/somiod/Application/Container/5
        public void Delete(int id)
        {
        }




    }
}