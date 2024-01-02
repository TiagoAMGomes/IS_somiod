using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Xml.Linq;
using SomiodAPI.Helpers;
using SomiodAPI.Models;

namespace SomiodAPI.Controllers
{
	[RoutePrefix("api/somiod")]
	public class SubController : ApiController
    {
		string connStr = Properties.Settings.Default.ConnStr;

		// GET: api/somiod/{appName}/{containerName}/sub
		[Route("{appName}/{containerName}/sub")]
		public IHttpActionResult Get(string appName, string containerName)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(connStr))
				{
					connection.Open();

					string selectQueryString = $"SELECT subscriptions.* FROM subscriptions " +
						$"INNER JOIN containers ON subscriptions.parent = containers.id " +
						$"INNER JOIN applications ON containers.parent = applications.id " +
						$"WHERE applications.name = '{appName}' " +
						$"AND containers.name = '{containerName}'";

					SqlCommand selectCommand = new SqlCommand(selectQueryString, connection);

					List<Subscription> subscriptions = new List<Subscription>();

					try
					{
						SqlDataReader reader = selectCommand.ExecuteReader();

						while (reader.Read())
						{
							Subscription subscription = new Subscription
							{
								Id = (int)reader["id"],
								Name = (string)reader["name"],
								Creation_dt = (DateTime)reader["creation_dt"],
								Event = (string)reader["event"],
								Endpoint = (string)reader["endpoint"]
							};

							subscriptions.Add(subscription);
						}

						reader.Close();
					}
					catch (Exception)
					{
						return InternalServerError();
					}

					XElement xmlData = new XElement("subscriptions",
						subscriptions.Select(s => new XElement("subscription",
							new XElement("id", s.Id),
							new XElement("name", s.Name),
							new XElement("creation_dt", s.Creation_dt.ToString("yyyy-MM-dd HH:mm:ss")),
							new XElement("event", s.Event),
							new XElement("endpoint", s.Endpoint)
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

		// GET: api/somiod/{appName}/{containerName}/sub/{subName}
		[Route("{appName}/{containerName}/sub/{subName}")]
		public IHttpActionResult Get(string appName, string containerName, string subName)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(connStr))
				{
					connection.Open();

					string selectQueryString = $"SELECT subscriptions.* FROM subscriptions " +
						$"INNER JOIN containers ON subscriptions.parent = containers.id " +
						$"INNER JOIN applications ON containers.parent = applications.id " +
						$"WHERE applications.name = '{appName}' " +
						$"AND containers.name = '{containerName}' " +
						$"AND subscriptions.name = '{subName}'";

					SqlCommand selectCommand = new SqlCommand(selectQueryString, connection);

					try
					{
						SqlDataReader reader = selectCommand.ExecuteReader();

						if (reader.Read())
						{
							Subscription subscription = new Subscription
							{
								Id = (int)reader["id"],
								Name = (string)reader["name"],
								Creation_dt = (DateTime)reader["creation_dt"],
								Event = (string)reader["event"],
								Endpoint = (string)reader["endpoint"]
							};

							reader.Close();

							XElement xmlData = new XElement("data",
								new XElement("id", subscription.Id),
								new XElement("name", subscription.Name),
								new XElement("creation_dt", subscription.Creation_dt.ToString("yyyy-MM-dd HH:mm:ss")),
								new XElement("event", subscription.Event),
								new XElement("endpoint", subscription.Endpoint)
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

		// DELETE: api/somiod/{appName}/{containerName}/sub/{subName}
		[Route("{appName}/{containerName}/sub/{subName}")]
		public IHttpActionResult Delete(string appName, string containerName, string subName)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(connStr))
				{
					connection.Open();

					string selectQueryString = $"SELECT * FROM subscriptions WHERE name = '{subName}' " +
						$"AND parent = (SELECT id FROM containers WHERE name = '{containerName}' " +
						$"AND parent = (SELECT id FROM applications WHERE name = '{appName}'))";

					string deleteQueryString = $"DELETE FROM subscriptions WHERE name = '{subName}' " +
						$"AND parent = (SELECT id FROM containers WHERE name = '{containerName}' " +
						$"AND parent = (SELECT id FROM applications WHERE name = '{appName}'))";

					SqlCommand selectCommand = new SqlCommand(selectQueryString, connection);
					SqlCommand deleteCommand = new SqlCommand(deleteQueryString, connection);

					try
					{
						SqlDataReader reader = selectCommand.ExecuteReader();

						if (reader.Read())
						{
							Subscription deletedSubscription = new Subscription
							{
								Id = (int)reader["id"],
								Name = (string)reader["name"],
								Creation_dt = (DateTime)reader["creation_dt"],
								Event = (string)reader["event"],
								Endpoint = (string)reader["endpoint"]
							};

							reader.Close();

							int rowsAffected = deleteCommand.ExecuteNonQuery();

							if (rowsAffected > 0)
							{
								XElement xmlData = new XElement("deletedSubscription",
									new XElement("id", deletedSubscription.Id),
									new XElement("name", deletedSubscription.Name),
									new XElement("creation_dt", deletedSubscription.Creation_dt.ToString("yyyy-MM-dd HH:mm:ss")),
									new XElement("event", deletedSubscription.Event),
									new XElement("endpoint", deletedSubscription.Endpoint)
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
