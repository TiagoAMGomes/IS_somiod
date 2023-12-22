using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Xml.Linq;
using SomiodAPI.Models;
using Application = SomiodAPI.Models.Application;

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
			if (Request.Headers.TryGetValues("somiod-discover", out var headerValues) && headerValues.FirstOrDefault().ToLower().Equals("containers"))
			{
				List<Container> containers = new List<Container>();

				string queryString = $"SELECT containers.name FROM containers " +
					$"JOIN applications ON containers.parent = applications.id " +
					$"WHERE applications.name = '{appName}'";

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
								Container container = new Container
								{
									Name = (string)reader["name"]
								};

								containers.Add(container);
							}

							reader.Close();

							XElement xmlData = new XElement("containers",
								from cont in containers
								select new XElement("name", cont.Name)
							);

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
			else
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
		}

		// POST: api/somiod
		public void Post([FromBody]string value)
        {
        }

		// PUT: api/somiod/{appName}
		public void Put(int id, [FromBody]string value)
        {
        }

		// DELETE: api/somiod/{appName}
		public void Delete(int id)
        {
        }
    }
}
