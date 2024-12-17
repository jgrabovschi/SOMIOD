using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Routing;
using MySql.Data.MySqlClient;
using SOMIOD.Models;


namespace SOMIOD.Controllers
{
    public class ApplicationController : ApiController
    {
       
        // Retrieve the connection string from the configuration file
        string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        private bool checkUniqueName(string name, MySqlConnection conn)
        {
            List<string> tableNames = new List<string> { "Applications", "Containers", "Records", "Notifications" };

            foreach (string tableName in tableNames)
            {
                // Table name can't be parameterized, so it's directly interpolated safely.
                string query = $"SELECT name FROM `{tableName}` WHERE name = @Name";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", name);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return false; // Name exists in the current table
                        }
                    }
                }
            }

            return true; // Name is unique across all tables
        }


        //Fazer aqui rota dinamica dependendo do que queremos ver ou seja
        //ao fazer o somiod-locate ele ira retornar a lista de nomes do todos os objecto que queremos ver
        [Route("api/somiod")]
        public IHttpActionResult Get(HttpRequestMessage requestHeader)
        {
            List<Application> applications = new List<Application>();


            // create a connection to the mysql database
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    if (requestHeader.Headers.Contains("somiod-locate")
                        && requestHeader.Headers.GetValues("somiod-locate").Contains("application"))
                    {
                        var names = new List<String>();
                        using (MySqlCommand cmd = new MySqlCommand("Select name from Applications", conn))
                        {
                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {

                                while (reader.Read())
                                {
                                    names.Add(reader["name"].ToString());
                                }
                            }
                            return Ok(names);

                        }
                    }
                    else if(requestHeader.Headers.Contains("somiod-locate")
                        && requestHeader.Headers.GetValues("somiod-locate").Contains("containers"))
                    {
                        var names = new List<String>();
                        using (MySqlCommand cmd = new MySqlCommand("Select name from Containers",conn))
                        {
                            using(MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    names.Add(reader["name"].ToString());
                                }
                            }
                        }
                    }
                    else
                    {
                        using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM Applications", conn))
                        {
                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                        var application = new Application
                                    {
                                        Id = Int32.Parse(reader["id"].ToString()),
                                        Name = reader["name"].ToString(),
                                        CreationDateTime = DateTime.TryParse(reader["creation_datetime"].ToString(), out DateTime parsedDate)
                                            ? parsedDate
                                            : DateTime.MinValue
                                    };

                                    applications.Add(application);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return InternalServerError();
            }



            return Ok(applications);
        }

        //Aqui irá retornar as propriedades dos obejctos dentro da applicação

        // GET: api/somioid/name
        [Route("api/somiod/{name}")]
        public IHttpActionResult Get(String name)
        {
            if (name == null)
            {
                return BadRequest();
            }
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT * FROM Applications WHERE name = @Name", conn))
                {
                    cmd.Parameters.AddWithValue("@Name", name);
                    try
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var application = new Application
                                {
                                    Id = Int32.Parse(reader["id"].ToString()),
                                    Name = reader["name"].ToString(),
                                    CreationDateTime = DateTime.TryParse(reader["creation_datetime"].ToString(), out DateTime parsedDate)
                                        ? parsedDate
                                        : DateTime.MinValue
                                };
                                return Ok(application);
                            }
                            return NotFound();
                        }
                    }
                    catch (Exception)
                    {
                        return InternalServerError();
                    }
                   
                }
            }
        }

        // POST: api/somiod/
        [Route("api/somiod")]
        public IHttpActionResult Post([FromBody]Application application)
        {
            if (application == null)
            {
                return BadRequest();
            }

            // create a connection to the mysql database
            using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                    }
                    catch (Exception)
                    {
                        return InternalServerError();
                    }

                    // check if the name is unique
                    if (!checkUniqueName(application.Name, conn))
                    {
                        return BadRequest();
                    }
                    using (MySqlCommand cmd = new MySqlCommand("INSERT INTO Applications (name, creation_datetime) VALUES (@Name, @CreationDateTime)", conn))
                    {
                        try
                        {
                            cmd.Parameters.AddWithValue("@Name", application.Name);
                            cmd.Parameters.AddWithValue("@CreationDateTime", DateTime.Now);
                        }
                        catch (Exception)
                        {
                            return BadRequest();
                        }

                        try
                        {
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception)
                        {
                            return InternalServerError();
                        }
                    }
                }

            return Ok();
        }

        // PUT: api/somiod/Application/5
        [Route("api/somiod/{name}")]
        public IHttpActionResult Put(string name, [FromBody]Application application)
        {
            if (application == null)
            {
                return BadRequest();
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                if (!checkUniqueName(application.Name, conn))
                {
                    return BadRequest();
                }
                using (MySqlCommand cmd = new MySqlCommand("UPDATE Applications SET name = @NameNew WHERE name = @NameOld", conn))
                {
                    cmd.Parameters.AddWithValue("@NameOld", name);
                    cmd.Parameters.AddWithValue("@NameNew", application.Name);
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception)
                    {
                        return InternalServerError();
                    }
                    return Ok();
                }
            }
        }


        [Route("api/somiod/{name}")]
        public IHttpActionResult Delete(string name)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand("DELETE FROM Applications WHERE name = @Name", conn))
                {
                    cmd.Parameters.AddWithValue("@Name", name);
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception)
                    {
                        return InternalServerError();
                    }
                    return Ok();
                }
            }
        }
    }
}
