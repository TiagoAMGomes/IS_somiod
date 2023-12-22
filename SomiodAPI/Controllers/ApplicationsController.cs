using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Xml.Linq;
using SomiodAPI.Models;

namespace SomiodAPI.Controllers
{
    public class ApplicationsController : ApiController
    {
		string connStr = Properties.Settings.Default.ConnStr;

		// GET: api/somiod
		[Route("api/somiod")]
		public IHttpActionResult Get()
        {
			List<Application> applications = new List<Application>();
			string queryString = "SELECT * FROM applications";

			try
			{
				using (SqlConnection connection = new SqlConnection(connStr))
				{
					SqlCommand command = new SqlCommand(queryString, connection);

					try
					{
						command.Connection.Open();
						SqlDataReader reader = command.ExecuteReader();

						while (reader.Read())
						{
							Application application = new Application
							{
								Id = (int)reader["id"],
								Name = (string)reader["name"],
								Creation_dt = (DateTime)reader["creation_dt"]
							};

							applications.Add(application);
						}

						reader.Close();

						if (applications == null)
						{
							return NotFound();
						}
						XElement xmlData;

						if (Request.Headers.TryGetValues("somiod-discover", out var headerValues) && headerValues.FirstOrDefault().ToLower().Equals("application"))
						{
							xmlData = new XElement("applications",
							from app in applications
								select new XElement("name", app.Name)
							);
						}
						else
						{
							xmlData = new XElement("applications",
							from app in applications
								select new XElement("application",
									new XElement("id", app.Id),
									new XElement("name", app.Name),
									new XElement("creation_dt", app.Creation_dt.ToString("yyyy-MM-dd HH:mm:ss"))
								)
							);
						}

						return Ok(xmlData);
					}
					catch (SqlException)
					{
						throw;
					}
				}
			}
			catch (Exception)
			{
				throw;
			}
		}

		// GET: api/somiod/{appName}
		[Route("api/somiod/{appName}")]
		public IHttpActionResult Get(string appName)
		{
			string queryString = $"SELECT * FROM applications WHERE name = '{appName}'";

			try
			{
				using (SqlConnection connection = new SqlConnection(connStr))
				{
					SqlCommand command = new SqlCommand(queryString, connection);

					try
					{
						command.Connection.Open();
						SqlDataReader reader = command.ExecuteReader();

						if (reader.Read())
						{
							Application application = new Application
							{
								Id = (int)reader["id"],
								Name = (string)reader["name"],
								Creation_dt = (DateTime)reader["creation_dt"]
							};

							reader.Close();

							XElement xmlData = new XElement("applications",
								new XElement("application",
									new XElement("id", application.Id),
									new XElement("name", application.Name),
									new XElement("creation_dt", application.Creation_dt.ToString("yyyy-MM-dd HH:mm:ss"))
								)
							);

							return Ok(xmlData);
						}
						else
						{
							reader.Close();
							return NotFound();
						}
					}
					catch (SqlException)
					{
						throw;
					}
				}
			}
			catch (Exception)
			{
				throw;
			}
		}

		// POST: api/Application
		public void Post([FromBody]string value)
        {
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
