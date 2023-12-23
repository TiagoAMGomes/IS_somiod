using System;
using System.Collections.Generic;
using System.Data;
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
						return InternalServerError();
					}
				}
			}
			catch (Exception)
			{
				return InternalServerError();
			}
		}

		// GET: api/somiod/{appName}
		[Route("api/somiod/{appName}")]
		public IHttpActionResult Get(string appName)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(connStr))
				{
					string queryStr = $"SELECT * FROM applications WHERE name = '{appName}'";
					SqlCommand command = new SqlCommand(queryStr, connection);

					try
					{
						command.Connection.Open();
						SqlDataReader reader = command.ExecuteReader();

						if (!reader.Read())
						{
							command.Connection.Close();
							reader.Close();
							return NotFound();
						}

						command.Connection.Close();
						reader.Close();

						if (Request.Headers.TryGetValues("somiod-discover", out var headerValues) && headerValues.FirstOrDefault().ToLower().Equals("container"))
						{
							return GetContainerData(connection, appName);
						}
						else
						{
							return GetApplicationData(connection, appName);
						}
					}
					catch (Exception)
					{
						return InternalServerError();
					}
				}
			}
			catch (Exception)
			{
				return InternalServerError();
			}
		}

		private IHttpActionResult GetContainerData(SqlConnection connection, string appName)
		{
			List<Container> containers = new List<Container>();

			string queryString = $"SELECT containers.name FROM containers " +
								 $"JOIN applications ON containers.parent = applications.id " +
								 $"WHERE applications.name = '{appName}'";

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
			catch (Exception)
			{
				return InternalServerError();
			}
		}

		private IHttpActionResult GetApplicationData(SqlConnection connection, string appName)
		{
			string queryString = $"SELECT * FROM applications WHERE name = '{appName}'";
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
			catch (Exception)
			{
				return InternalServerError();
			}
		}

		// POST: api/somiod
		[Route("api/somiod")]
		public IHttpActionResult Post([FromBody] XElement xmlInput)
		{
			try
			{
				string appName = xmlInput.Element("name")?.Value;

				if (string.IsNullOrWhiteSpace(appName))
				{
					return BadRequest("Invalid XML format. 'name' element is required.");
				}

				if (!IsNameUnique(appName))
				{
					appName = GenerateUniqueName(appName);
				}

				using (SqlConnection connection = new SqlConnection(connStr))
				{
					string insertQueryString = $"INSERT INTO applications (name) VALUES ('{appName}')";
					string selectQueryString = $"SELECT * FROM applications WHERE name = '{appName}'";

					SqlCommand insertCommand = new SqlCommand(insertQueryString, connection);
					SqlCommand selectCommand = new SqlCommand(selectQueryString, connection);

					try
					{
						insertCommand.Connection.Open();
						int rowsAffected = insertCommand.ExecuteNonQuery();

						if (rowsAffected > 0)
						{
							SqlDataReader reader = selectCommand.ExecuteReader();

							if (reader.Read())
							{
								Application application = new Application
								{
									Id = (int)reader["id"],
									Name = (string)reader["name"],
									Creation_dt = (DateTime)reader["creation_dt"]
								};

								reader.Close();

								XElement xmlData = new XElement("application",
									new XElement("id", application.Id),
									new XElement("name", application.Name),
									new XElement("creation_dt", application.Creation_dt.ToString("yyyy-MM-dd HH:mm:ss"))
								);

								return Ok(xmlData);
							}
						}

						return InternalServerError(new Exception("Failed to create application or retrieve application details."));
					}
					catch (Exception)
					{
						return InternalServerError();
					}
				}
			}
			catch (Exception)
			{
				return InternalServerError();
			}
		}

		// PUT: api/somiod/{appName}
		[Route("api/somiod/{appName}")]
		public IHttpActionResult Put(string appName, [FromBody] XElement xmlInput)
		{
			try
			{
				string newAppName = xmlInput.Element("name")?.Value;

				if (string.IsNullOrWhiteSpace(newAppName))
				{
					return BadRequest("Invalid XML format. 'name' element is required.");
				}

				if (!IsNameUnique(newAppName))
				{
					newAppName = GenerateUniqueName(newAppName);
				}

				using (SqlConnection connection = new SqlConnection(connStr))
				{
					string updateQueryString = $"UPDATE applications SET name = '{newAppName}' WHERE name = '{appName}'";
					string selectQueryString = $"SELECT * FROM applications WHERE name = '{newAppName}'";

					SqlCommand updateCommand = new SqlCommand(updateQueryString, connection);
					SqlCommand selectCommand = new SqlCommand(selectQueryString, connection);

					try
					{
						updateCommand.Connection.Open();
						int rowsAffected = updateCommand.ExecuteNonQuery();
						updateCommand.Connection.Close();

						if (rowsAffected > 0)
						{
							selectCommand.Connection.Open();
							SqlDataReader reader = selectCommand.ExecuteReader();

							if (reader.Read())
							{
								Application application = new Application
								{
									Id = (int)reader["id"],
									Name = (string)reader["name"],
									Creation_dt = (DateTime)reader["creation_dt"]
								};

								reader.Close();

								XElement xmlData = new XElement("application",
									new XElement("id", application.Id),
									new XElement("name", application.Name),
									new XElement("creation_dt", application.Creation_dt.ToString("yyyy-MM-dd HH:mm:ss"))
								);

								return Ok(xmlData);
							}
						}

						return NotFound();
					}
					catch (Exception)
					{
						return InternalServerError();
					}
				}
			}
			catch (Exception)
			{
				return InternalServerError();
			}
		}

		// DELETE: api/somiod/{appName}
		[Route("api/somiod/{appName}")]
		public IHttpActionResult Delete(string appName)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(connStr))
				{
					string selectQueryString = $"SELECT * FROM applications WHERE name = '{appName}'";
					string deleteQueryString = $"DELETE FROM applications WHERE name = '{appName}'";

					SqlCommand selectCommand = new SqlCommand(selectQueryString, connection);
					SqlCommand deleteCommand = new SqlCommand(deleteQueryString, connection);

					try
					{
						selectCommand.Connection.Open();
						SqlDataReader reader = selectCommand.ExecuteReader();

						if (reader.Read())
						{
							Application deletedApp = new Application
							{
								Id = (int)reader["id"],
								Name = (string)reader["name"],
								Creation_dt = (DateTime)reader["creation_dt"]
							};

							reader.Close();

							selectCommand.Connection.Close();
							deleteCommand.Connection.Open();
							int rowsAffected = deleteCommand.ExecuteNonQuery();

							if (rowsAffected > 0)
							{
								XElement xmlData = new XElement("application",
									new XElement("id", deletedApp.Id),
									new XElement("name", deletedApp.Name),
									new XElement("creation_dt", deletedApp.Creation_dt.ToString("yyyy-MM-dd HH:mm:ss"))
								);
								return Ok(xmlData);
							}
						}

						return NotFound();
					}
					catch (Exception)
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


		// -----> HELPER FUNCTIONS <-----

		private bool IsNameUnique(string appName)
		{
			using (SqlConnection connection = new SqlConnection(connStr))
			{
				string queryString = $"SELECT COUNT(*) FROM applications WHERE name = '{appName}'";
				SqlCommand command = new SqlCommand(queryString, connection);

				try
				{
					command.Connection.Open();
					int count = (int)command.ExecuteScalar();
					return count == 0;
				}
				catch (Exception)
				{
					throw;
				}
			}
		}

		private string GenerateUniqueName(string baseName)
		{
			return $"{baseName}_{DateTime.Now.Ticks}";
		}
	}
}
