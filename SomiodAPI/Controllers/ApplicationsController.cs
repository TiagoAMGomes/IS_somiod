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
using SomiodAPI.Helpers;
using Application = SomiodAPI.Models.Application;

namespace SomiodAPI.Controllers
{
	[RoutePrefix("api/somiod")]
	public class ApplicationsController : ApiController
    {
		string connStr = Properties.Settings.Default.ConnStr;

		// --------------------> APPLICATIONS <--------------------

		// GET: api/somiod
		[Route("")]
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
						connection.Open();
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
		[Route("{appName}")]
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
						connection.Open();
						SqlDataReader reader = command.ExecuteReader();

						if (!reader.Read())
						{
							reader.Close();
							return NotFound();
						}

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

					XElement xmlData = new XElement("application",
						new XElement("id", application.Id),
						new XElement("name", application.Name),
						new XElement("creation_dt", application.Creation_dt.ToString("yyyy-MM-dd HH:mm:ss"))
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
		[Route("")]
		public IHttpActionResult Post([FromBody] XElement xmlInput)
		{
			try
			{
				string appName = xmlInput.Element("name")?.Value;

				if (string.IsNullOrWhiteSpace(appName))
				{
					return BadRequest("Invalid XML format. 'name' element is required.");
				}

				if (!HelperFunctions.IsAppNameUnique(appName, connStr))
				{
					appName = HelperFunctions.GenerateUniqueName(appName);
				}

				using (SqlConnection connection = new SqlConnection(connStr))
				{
					string insertQueryString = $"INSERT INTO applications (name) VALUES ('{appName}')";
					string selectQueryString = $"SELECT * FROM applications WHERE name = '{appName}'";

					SqlCommand insertCommand = new SqlCommand(insertQueryString, connection);
					SqlCommand selectCommand = new SqlCommand(selectQueryString, connection);

					try
					{
						connection.Open();
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
		[Route("{appName}")]
		public IHttpActionResult Put(string appName, [FromBody] XElement xmlInput)
		{
			try
			{
				string newAppName = xmlInput.Element("name")?.Value;

				if (string.IsNullOrWhiteSpace(newAppName))
				{
					return BadRequest("Invalid XML format. 'name' element is required.");
				}

				if (!HelperFunctions.IsAppNameUnique(newAppName, connStr))
				{
					newAppName = HelperFunctions.GenerateUniqueName(newAppName);
				}

				using (SqlConnection connection = new SqlConnection(connStr))
				{
					string updateQueryString = $"UPDATE applications SET name = '{newAppName}' WHERE name = '{appName}'";
					string selectQueryString = $"SELECT * FROM applications WHERE name = '{newAppName}'";

					SqlCommand updateCommand = new SqlCommand(updateQueryString, connection);
					SqlCommand selectCommand = new SqlCommand(selectQueryString, connection);

					try
					{
						connection.Open();
						int rowsAffected = updateCommand.ExecuteNonQuery();

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
		[Route("{appName}")]
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
						connection.Open();
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
						return InternalServerError();
					}
				}
			}
			catch (Exception)
			{
				return InternalServerError();
			}
		}


		// --------------------> CONTAINERS <--------------------

		// GET: api/somiod/{appName}/{containerName}
		[Route("{appName}/{containerName}")]
		public IHttpActionResult Get(string appName, string containerName)
		{
			try
			{
				if (HelperFunctions.IsAppNameUnique(appName, connStr))
				{
					return NotFound();
				}

				if (HelperFunctions.IsContainerNameUnique(appName, containerName, connStr))
				{
					return NotFound();
				}

				using (SqlConnection connection = new SqlConnection(connStr))
				{
					if (Request.Headers.TryGetValues("somiod-discover", out var headerValues))
					{
						if (headerValues.FirstOrDefault().ToLower().Equals("data"))
						{
							return GetDataData(connection, appName, containerName);
						}
						else if (headerValues.FirstOrDefault().ToLower().Equals("subscription"))
						{
							return GetSubscriptionData(connection, appName, containerName);
						}
					}
					
					return GetContainer(connection, appName, containerName);
				}
			}
			catch (Exception)
			{
				return InternalServerError();
			}
		}

		private IHttpActionResult GetDataData(SqlConnection connection, string appName, string containerName)
		{
			List<Data> datas = new List<Data>();

			string selectQueryString = $"SELECT data.content FROM data " +
				$"JOIN containers ON data.parent = containers.id " +
				$"JOIN applications ON containers.parent = applications.id " +
				$"WHERE containers.name = '{containerName}' " +
				$"AND applications.name = '{appName}'";

			SqlCommand selectCommand = new SqlCommand(selectQueryString, connection);

			try
			{
				connection.Open();
				SqlDataReader reader = selectCommand.ExecuteReader();

				while (reader.Read())
				{
					Data data = new Data
					{
						Content = (string)reader["content"]
					};

					datas.Add(data);
				}

				reader.Close();

				XElement xmlData = new XElement("data",
					from data in datas
					select new XElement("content", data.Content)
				);

				return Ok(xmlData);
			}
			catch (Exception)
			{
				return InternalServerError();
			}
		}

		private IHttpActionResult GetSubscriptionData(SqlConnection connection, string appName, string containerName)
		{
			List<Subscription> subscriptions = new List<Subscription>();

			string selectQueryString = $"SELECT subscriptions.name FROM subscriptions " +
				$"JOIN containers ON subscriptions.parent = containers.id " +
				$"JOIN applications ON containers.parent = applications.id " +
				$"WHERE containers.name = '{containerName}' " +
				$"AND applications.name = '{appName}'";

			SqlCommand selectCommand = new SqlCommand(selectQueryString, connection);

			try
			{
				connection.Open();
				SqlDataReader reader = selectCommand.ExecuteReader();

				while (reader.Read())
				{
					Subscription subscription = new Subscription
					{
						Name = (string)reader["name"]
					};

					subscriptions.Add(subscription);
				}

				reader.Close();

				XElement xmlData = new XElement("subscriptions",
					from sub in subscriptions
					select new XElement("name", sub.Name)
				);

				return Ok(xmlData);
			}
			catch (Exception)
			{
				return InternalServerError();
			}
		}

		private IHttpActionResult GetContainer(SqlConnection connection, string appName, string containerName)
		{
			string selectQueryString = $"SELECT * FROM containers WHERE name = '{containerName}' AND parent = (SELECT id FROM applications WHERE name = '{appName}')";

			SqlCommand selectCommand = new SqlCommand(selectQueryString, connection);

			try
			{
				connection.Open();
				SqlDataReader reader = selectCommand.ExecuteReader();

				if (reader.Read())
				{
					Container container = new Container
					{
						Id = (int)reader["id"],
						Name = (string)reader["name"],
						Creation_dt = (DateTime)reader["creation_dt"],
						Parent = (int)reader["parent"]
					};

					reader.Close();

					XElement xmlData = new XElement("container",
						new XElement("id", container.Id),
						new XElement("name", container.Name),
						new XElement("creation_dt", container.Creation_dt.ToString("yyyy-MM-dd HH:mm:ss")),
						new XElement("parent", container.Parent)
					);

					return Ok(xmlData);
				}
				else
				{
					return NotFound();
				}
			}
			catch (Exception)
			{
				return InternalServerError();
			}
		}

		// POST: api/somiod/{appName}
		[Route("{appName}")]
		public IHttpActionResult Post(string appName, [FromBody] XElement xmlInput)
		{
			try
			{
				if (HelperFunctions.IsAppNameUnique(appName, connStr))
				{
					return NotFound();
				}

				string containerName = xmlInput.Element("name")?.Value;

				if (string.IsNullOrWhiteSpace(containerName))
				{
					return BadRequest("Invalid XML format. 'name' element is required for the container.");
				}

				if (!HelperFunctions.IsContainerNameUnique(appName, containerName, connStr))
				{
					containerName = HelperFunctions.GenerateUniqueName(containerName);
				}

				using (SqlConnection connection = new SqlConnection(connStr))
				{
					string insertQueryString = $"INSERT INTO containers (name, parent) VALUES ('{containerName}', (SELECT id FROM applications WHERE name = '{appName}'))";
					string selectQueryString = $"SELECT * FROM containers WHERE name = '{containerName}' AND parent = (SELECT id FROM applications WHERE name = '{appName}')";

					SqlCommand insertCommand = new SqlCommand(insertQueryString, connection);
					SqlCommand selectCommand = new SqlCommand(selectQueryString, connection);

					try
					{
						connection.Open();
						int rowsAffected = insertCommand.ExecuteNonQuery();

						if (rowsAffected > 0)
						{
							SqlDataReader reader = selectCommand.ExecuteReader();

							if (reader.Read())
							{
								Container container = new Container
								{
									Id = (int)reader["id"],
									Name = (string)reader["name"],
									Creation_dt = (DateTime)reader["creation_dt"],
									Parent = (int)reader["parent"]
								};

								reader.Close();

								XElement xmlData = new XElement("container",
									new XElement("id", container.Id),
									new XElement("name", container.Name),
									new XElement("creation_dt", container.Creation_dt.ToString("yyyy-MM-dd HH:mm:ss")),
									new XElement("parent", container.Parent)
								);

								return Ok(xmlData);
							}
						}

						return InternalServerError(new Exception("Failed to create container or retrieve container details."));
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

		// PUT: api/somiod/{appName}/{containerName}
		[Route("{appName}/{containerName}")]
		public IHttpActionResult Put(string appName, string containerName, [FromBody] XElement xmlInput)
		{
			try
			{
				if (HelperFunctions.IsAppNameUnique(appName, connStr))
				{
					return NotFound();
				}

				if (HelperFunctions.IsContainerNameUnique(appName, containerName, connStr))
				{
					return NotFound();
				}

				string newContainerName = xmlInput.Element("name")?.Value;

				if (string.IsNullOrWhiteSpace(newContainerName))
				{
					return BadRequest("Invalid XML format. 'name' element is required.");
				}

				if (!HelperFunctions.IsContainerNameUnique(appName, newContainerName, connStr))
				{
					newContainerName = HelperFunctions.GenerateUniqueName(newContainerName);
				}

				using (SqlConnection connection = new SqlConnection(connStr))
				{
					string updateQueryString = $"UPDATE containers SET name = '{newContainerName}' WHERE name = '{containerName}' AND parent = (SELECT id FROM applications WHERE name = '{appName}')";
					string selectQueryString = $"SELECT * FROM containers WHERE name = '{newContainerName}' AND parent = (SELECT id FROM applications WHERE name = '{appName}')";

					SqlCommand updateCommand = new SqlCommand(updateQueryString, connection);
					SqlCommand selectCommand = new SqlCommand(selectQueryString, connection);

					try
					{
						connection.Open();
						int rowsAffected = updateCommand.ExecuteNonQuery();

						if (rowsAffected > 0)
						{
							SqlDataReader reader = selectCommand.ExecuteReader();

							if (reader.Read())
							{
								Container updatedContainer = new Container
								{
									Id = (int)reader["id"],
									Name = (string)reader["name"],
									Creation_dt = (DateTime)reader["creation_dt"],
									Parent = (int)reader["parent"]
								};

								reader.Close();

								XElement xmlData = new XElement("container",
									new XElement("id", updatedContainer.Id),
									new XElement("name", updatedContainer.Name),
									new XElement("creation_dt", updatedContainer.Creation_dt.ToString("yyyy-MM-dd HH:mm:ss")),
									new XElement("parent", updatedContainer.Parent)
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

		// DELETE: api/somiod/{appName}/{containerName}
		[Route("{appName}/{containerName}")]
		public IHttpActionResult Delete(string appName, string containerName)
		{
			try
			{
				if (HelperFunctions.IsAppNameUnique(appName, connStr))
				{
					return NotFound();
				}

				if (HelperFunctions.IsContainerNameUnique(appName, containerName, connStr))
				{
					return NotFound();
				}

				using (SqlConnection connection = new SqlConnection(connStr))
				{
					string selectQueryString = $"SELECT * FROM containers WHERE name = '{containerName}' AND parent = (SELECT id FROM applications WHERE name = '{appName}')";

					SqlCommand selectCommand = new SqlCommand(selectQueryString, connection);

					try
					{
						connection.Open();

						SqlDataReader reader = selectCommand.ExecuteReader();

						if (reader.Read())
						{
							Container deletedContainer = new Container
							{
								Id = (int)reader["id"],
								Name = (string)reader["name"],
								Creation_dt = (DateTime)reader["creation_dt"],
								Parent = (int)reader["parent"]
							};

							reader.Close();

							string deleteQueryString = $"DELETE FROM containers WHERE name = '{containerName}' AND parent = (SELECT id FROM applications WHERE name = '{appName}')";

							SqlCommand deleteCommand = new SqlCommand(deleteQueryString, connection);

							int rowsAffected = deleteCommand.ExecuteNonQuery();

							if (rowsAffected > 0)
							{
								XElement xmlData = new XElement("container",
									new XElement("id", deletedContainer.Id),
									new XElement("name", deletedContainer.Name),
									new XElement("creation_dt", deletedContainer.Creation_dt.ToString("yyyy-MM-dd HH:mm:ss")),
									new XElement("parent", deletedContainer.Parent)
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
	}
}
