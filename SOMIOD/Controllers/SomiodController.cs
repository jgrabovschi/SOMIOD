using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using MySql.Data.MySqlClient;
using SOMIOD.Models;

namespace SOMIOD.Controllers
{
    public class SomiodController : ApiController
    {
        // Retrieve the connection string from the configuration file
        protected string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        protected bool checkUniqueName(string name, MySqlConnection conn)
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




        /**
         * GETS
        */


        [Route("api/somiod")]
        public IHttpActionResult GetApplications(HttpRequestMessage requestHeader)
        {
            List<Application> applications = new List<Application>();


            // create a connection to the mysql database
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    if (requestHeader.Headers.Contains("somiod-locate"))
                    {
                        if (requestHeader.Headers.GetValues("somiod-locate").Contains("application"))
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
                        else if (requestHeader.Headers.GetValues("somiod-locate").Contains("container"))
                        {
                            var names = new List<String>();
                            using (MySqlCommand cmd = new MySqlCommand("Select name from Containers", conn))
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
                        else if (requestHeader.Headers.GetValues("somiod-locate").Contains("record"))
                        {
                            var names = new List<String>();
                            using (MySqlCommand cmd = new MySqlCommand("Select name from Records", conn))
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
                        else if (requestHeader.Headers.GetValues("somiod-locate").Contains("notification"))
                        {
                            var names = new List<String>();
                            using (MySqlCommand cmd = new MySqlCommand("Select name from Notifications", conn))
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
                        else
                        {
                            return BadRequest();
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



        [HttpGet]
        [Route("api/somiod/{application}")]
        public IHttpActionResult GetApplication(String application, HttpRequestMessage requestHeader)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                if (requestHeader.Headers.Contains("somiod-locate"))
                {
                    var app_id = 0;

                    using (var cmd = new MySqlCommand("SELECT id FROM Applications WHERE name = @Name", conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", application);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                app_id = Int32.Parse(reader["id"].ToString());
                            }
                            else
                            {
                                return NotFound();

                            }
                        }
                    }

                    if (requestHeader.Headers.GetValues("somiod-locate").Contains("container"))
                    {
                        var names = new List<String>();
                        using (var cmd = new MySqlCommand("Select name from Containers where parent in (select id from Applications where name = @Name)", conn))
                        {
                            cmd.Parameters.AddWithValue("@Name", application);
                            using (var reader = cmd.ExecuteReader())
                            {

                                while (reader.Read())
                                {
                                    names.Add(reader["name"].ToString());
                                }
                            }
                            return Ok(names);
                        }
                    }
                    else if (requestHeader.Headers.GetValues("somiod-locate").Contains("record"))
                    {
                        var records = new List<String>();
                        using (var cmd = new MySqlCommand("Select name from Records where parent in (select id from Containers where parent in (select id from Applications where name = @Name))", conn))
                        {
                            cmd.Parameters.AddWithValue("@Name", application);
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    records.Add(reader["name"].ToString());
                                }
                            }
                            return Ok(records);
                        }
                    }
                    else if (requestHeader.Headers.GetValues("somiod-locate").Contains("notification"))
                    {
                        var notifications = new List<String>();
                        using (var cmd = new MySqlCommand("Select name from Notifications where parent in (select id from Containers where parent in (select id from Applications where name = @Name))", conn))
                        {
                            cmd.Parameters.AddWithValue("@Name", application);
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    notifications.Add(reader["name"].ToString());
                                }
                            }
                            return Ok(notifications);
                        }
                    }
                    else
                    {
                        return BadRequest();
                    }
                }

                using (var cmd = new MySqlCommand("SELECT * FROM Applications WHERE name = @Name", conn))
                {
                    cmd.Parameters.AddWithValue("@Name", application);
                    try
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var app = new Application
                                {
                                    Id = Int32.Parse(reader["id"].ToString()),
                                    Name = reader["name"].ToString(),
                                    CreationDateTime = DateTime.TryParse(reader["creation_datetime"].ToString(), out DateTime parsedDate)
                                        ? parsedDate
                                        : DateTime.MinValue
                                };
                                return Ok(app);
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



        [HttpGet]
        [Route("api/somiod/{application}/{container}")]
        public IHttpActionResult GetContainer(String application, String container, HttpRequestMessage request)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                if (request.Headers.Contains("somiod-locate"))
                {
                    if (request.Headers.GetValues("somiod-locate").Contains("record"))
                    {
                        var names = new List<String>();

                        using (var cmd = new MySqlCommand("SELECT name FROM Records WHERE parent in (Select id from Containers Where name = @Container)", conn))
                        {
                            cmd.Parameters.AddWithValue("@Container", container);
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    names.Add(reader.GetString(0));
                                }
                                return Ok(names);
                            }
                        }

                    }
                    else if (request.Headers.GetValues("somiod-locate").Contains("notification"))
                    {
                        var names = new List<String>();
                        using (var cmd = new MySqlCommand("SELECT name FROM Notifications WHERE parent in (Select id from Containers Where name = @Container)", conn))
                        {
                            cmd.Parameters.AddWithValue("@Container", container);
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    names.Add(reader.GetString(0));
                                }
                                return Ok(names);
                            }
                        }
                    }
                    else
                    {
                        return BadRequest();
                    }
                }

                var app_id = 0;

                using (var cmd = new MySqlCommand("SELECT id FROM Applications WHERE name = @Application", conn))
                {
                    cmd.Parameters.AddWithValue("@Application", application);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            app_id = Int32.Parse(reader["id"].ToString());
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                }

                using (var cmd2 = new MySqlCommand("SELECT * FROM Containers WHERE name = @Container AND parent = @App_id", conn))
                {
                    cmd2.Parameters.AddWithValue("@Container", container);
                    cmd2.Parameters.AddWithValue("@App_id", app_id);
                    using (var reader2 = cmd2.ExecuteReader())
                    {
                        if (reader2.Read())
                        {
                            Container cont = new Container
                            {
                                Id = Int32.Parse(reader2["id"].ToString()),
                                Name = reader2["name"].ToString(),
                                CreationDateTime = DateTime.TryParse(reader2["creation_datetime"].ToString(), out DateTime parsedDate)
                                         ? parsedDate
                                         : DateTime.MinValue,
                                Parent = Int32.Parse(reader2["parent"].ToString())
                            };
                            return Ok(cont);
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                }
            }
        }




        /**
         * POSTS
        */


        [HttpPost]
        [Route("api/somiod")]
        public IHttpActionResult PostApplication([FromBody] Application application)
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


        [HttpPost]
        [Route("api/somiod/{application}")]
        public IHttpActionResult PostContainer(String application, [FromBody] Container container)
        {
            if (container == null)
            {
                return BadRequest();
            }

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                if (!checkUniqueName(container.Name, conn))
                {
                    return BadRequest();
                }
                var app_id = 0;
                using (var cmd = new MySqlCommand("SELECT id FROM Applications WHERE name = @Application", conn))
                {
                    cmd.Parameters.AddWithValue("@Application", application);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            app_id = Int32.Parse(reader["id"].ToString());
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                }
                using (var cmd2 = new MySqlCommand("INSERT INTO Containers (name, creation_datetime, parent) VALUES (@Name, @CreationDateTime, @Parent)", conn))
                {
                    cmd2.Parameters.AddWithValue("@Name", container.Name);
                    cmd2.Parameters.AddWithValue("@CreationDateTime", DateTime.Now);
                    cmd2.Parameters.AddWithValue("@Parent", app_id);
                    cmd2.ExecuteNonQuery();
                    return Ok();
                }
            }
        }


        /**
         * PUTS
        */

        [HttpPut]
        [Route("api/somiod/{name}")]
        public IHttpActionResult PutApplication(string name, [FromBody] Application application)
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

        [HttpPut]
        [Route("api/somiod/{application}/{name}")]
        public IHttpActionResult PutContainer(string application, string name, [FromBody] Container container)
        {
            if (container == null)
            {
                return BadRequest();
            }
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                if (!checkUniqueName(container.Name, conn))
                {
                    return BadRequest();
                }
                var app_id = 0;
                using (MySqlCommand cmd = new MySqlCommand("SELECT id FROM Applications WHERE name = @Application", conn))
                {
                    cmd.Parameters.AddWithValue("@Application", application);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            app_id = Int32.Parse(reader["id"].ToString());
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                }
                using (MySqlCommand cmd = new MySqlCommand("UPDATE Containers SET name = @NameNew WHERE name = @NameOld AND parent = @Parent", conn))
                {
                    cmd.Parameters.AddWithValue("@NameOld", name);
                    cmd.Parameters.AddWithValue("@NameNew", container.Name);
                    cmd.Parameters.AddWithValue("@Parent", app_id);
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

        /**
         * DELETES
        */

        [HttpDelete]
        [Route("api/somiod/{name}")]
        public IHttpActionResult DeleteApplication(string name)
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

        [HttpDelete]
        [Route("api/somiod/{application}/{name}")]
        public IHttpActionResult DeleteContainer(string application, string name)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var app_id = 0;
                using (MySqlCommand cmd = new MySqlCommand("SELECT id FROM Applications WHERE name = @Application", conn))
                {
                    cmd.Parameters.AddWithValue("@Application", application);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            app_id = Int32.Parse(reader["id"].ToString());
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                }
                using (MySqlCommand cmd = new MySqlCommand("DELETE FROM Containers WHERE name = @Name AND parent = @Parent", conn))
                {
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Parent", app_id);
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