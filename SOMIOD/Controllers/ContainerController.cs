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

    public class ContainerController : ApiController
    {
        const string databaseURL = "mongodb://mongodb.cloud-ss.pt:27017";

        //GET -h "somiod-locate: XXXX" api/somioid/Application/
        public IHttpActionResult get()
        {
            var client = new MongoClient(databaseURL);            
            Console.Write("connected");
            var database = client.GetDatabase("somiod");
            var collection = database.GetCollection<Container>("containers");
            var containers = collection.Find(_ => true).ToList();


            return Ok(containers);
        }


        // GET: api/somioid/Application/Container/5
        public string Get(int id)
        {
            return "value";
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