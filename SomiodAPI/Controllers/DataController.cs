using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using SomiodAPI.Models;
using SomiodAPI.Helpers;

namespace SomiodAPI.Controllers
{
	[RoutePrefix("api/somiod")]
	public class DataController : ApiController
    {
		string connStr = Properties.Settings.Default.ConnStr;

		// GET: api/somiod/{appName}/{containerName}/data
		[Route("{appName}/{containerName}/data")]
		public IHttpActionResult Get(string appName, string containerName)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(connStr))
				{
					connection.Open();

					string selectQueryString = $"SELECT data.* FROM data " +
						$"INNER JOIN containers ON data.parent = containers.id " +
						$"INNER JOIN applications ON containers.parent = applications.id " +
						$"WHERE applications.name = '{appName}' " +
						$"AND containers.name = '{containerName}'";

					SqlCommand selectCommand = new SqlCommand(selectQueryString, connection);

					List<Data> dataList = new List<Data>();

					try
					{
						SqlDataReader reader = selectCommand.ExecuteReader();

						while (reader.Read())
						{
							Data dataItem = new Data
							{
								Id = (int)reader["id"],
								Content = (string)reader["content"],
								Creation_dt = (DateTime)reader["creation_dt"]
							};

							dataList.Add(dataItem);
						}

						reader.Close();
					}
					catch (Exception)
					{
						return InternalServerError();
					}

					XElement xmlData = new XElement("dataList",
						dataList.Select(d => new XElement("data",
							new XElement("id", d.Id),
							new XElement("content", d.Content),
							new XElement("creation_dt", d.Creation_dt.ToString("yyyy-MM-dd HH:mm:ss"))
						))
					);

					return Ok(xmlData);
				}
			}
			catch (Exception)
			{
				return InternalServerError();
			}
		}

		// GET: api/somiod/{appName}/{containerName}/data/{dataContent}
		[Route("{appName}/{containerName}/data/{dataContent}")]
		public IHttpActionResult Get(string appName, string containerName, string dataContent)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(connStr))
				{
					connection.Open();

					string selectQueryString = $"SELECT data.* FROM data " +
						$"INNER JOIN containers ON data.parent = containers.id " +
						$"INNER JOIN applications ON containers.parent = applications.id " +
						$"WHERE applications.name = '{appName}' " +
						$"AND containers.name = '{containerName}' " +
						$"AND data.content = '{dataContent}'";

					SqlCommand selectCommand = new SqlCommand(selectQueryString, connection);

					try
					{
						SqlDataReader reader = selectCommand.ExecuteReader();

						if (reader.Read())
						{
							Data dataItem = new Data
							{
								Id = (int)reader["id"],
								Content = (string)reader["content"],
								Creation_dt = (DateTime)reader["creation_dt"]
							};

							reader.Close();

							XElement xmlData = new XElement("data",
								new XElement("id", dataItem.Id),
								new XElement("content", dataItem.Content),
								new XElement("creation_dt", dataItem.Creation_dt.ToString("yyyy-MM-dd HH:mm:ss"))
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
			}
			catch (Exception)
			{
				return InternalServerError();
			}
		}

		// POST: api/somiod/{appName}/{containerName}/data
		[Route("{appName}/{containerName}/data")]
		public IHttpActionResult Post(string appName, string containerName, [FromBody] XElement xmlInput)
		{
			try
			{
				string content = xmlInput.Element("content")?.Value;

				if (string.IsNullOrWhiteSpace(content))
				{
					return BadRequest("Invalid XML format. 'content' element is required.");
				}

				using (SqlConnection connection = new SqlConnection(connStr))
				{
					connection.Open();

					if (!HelperFunctions.IsContentUniqueInContainer(appName, containerName, content, connection))
					{
						content = HelperFunctions.GenerateUniqueName(content);
					}

					string insertQueryString = $"INSERT INTO data (content, parent) " +
						$"VALUES ('{content}', (SELECT id FROM containers WHERE name = '{containerName}' " +
						$"AND parent = (SELECT id FROM applications WHERE name = '{appName}')))";

					string selectQueryString = $"SELECT * FROM data WHERE content = '{content}' " +
						$"AND parent = (SELECT id FROM containers WHERE name = '{containerName}' " +
						$"AND parent = (SELECT id FROM applications WHERE name = '{appName}'))";

					SqlCommand insertCommand = new SqlCommand(insertQueryString, connection);
					SqlCommand selectCommand = new SqlCommand(selectQueryString, connection);

					try
					{
						int rowsAffected = insertCommand.ExecuteNonQuery();

						if (rowsAffected > 0)
						{
							SqlDataReader reader = selectCommand.ExecuteReader();

							if (reader.Read())
							{
								Data dataItem = new Data
								{
									Id = (int)reader["id"],
									Content = (string)reader["content"],
									Creation_dt = (DateTime)reader["creation_dt"]
								};

								reader.Close();

								XElement xmlData = new XElement("data",
									new XElement("id", dataItem.Id),
									new XElement("content", dataItem.Content),
									new XElement("creation_dt", dataItem.Creation_dt.ToString("yyyy-MM-dd HH:mm:ss"))
								);

								return Ok(xmlData);
							}
						}

						return InternalServerError(new Exception("Failed to create data or retrieve data details."));
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

		// DELETE: api/somiod/{appName}/{containerName}/data/{dataContent}
		[Route("{appName}/{containerName}/data/{dataContent}")]
		public IHttpActionResult Delete(string appName, string containerName, string dataContent)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(connStr))
				{
					connection.Open();

					string selectQueryString = $"SELECT * FROM data WHERE content = '{dataContent}' " +
						$"AND parent = (SELECT id FROM containers WHERE name = '{containerName}' " +
						$"AND parent = (SELECT id FROM applications WHERE name = '{appName}'))";

					string deleteQueryString = $"DELETE FROM data WHERE content = '{dataContent}' " +
						$"AND parent = (SELECT id FROM containers WHERE name = '{containerName}' " +
						$"AND parent = (SELECT id FROM applications WHERE name = '{appName}'))";

					SqlCommand selectCommand = new SqlCommand(selectQueryString, connection);
					SqlCommand deleteCommand = new SqlCommand(deleteQueryString, connection);

					try
					{
						SqlDataReader reader = selectCommand.ExecuteReader();

						if (reader.Read())
						{
							Data deletedData = new Data
							{
								Id = (int)reader["id"],
								Content = (string)reader["content"],
								Creation_dt = (DateTime)reader["creation_dt"]
							};

							reader.Close();

							int rowsAffected = deleteCommand.ExecuteNonQuery();

							if (rowsAffected > 0)
							{
								XElement xmlData = new XElement("data",
									new XElement("id", deletedData.Id),
									new XElement("content", deletedData.Content),
									new XElement("creation_dt", deletedData.Creation_dt.ToString("yyyy-MM-dd HH:mm:ss"))
								);

								return Ok(xmlData);
							}
						}

						reader.Close();
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
