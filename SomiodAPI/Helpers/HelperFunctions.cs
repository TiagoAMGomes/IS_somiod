using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace SomiodAPI.Helpers
{
	public static class HelperFunctions
	{
		public static bool IsAppNameUnique(string appName, string connStr)
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
					return false;
				}
			}
		}

		public static bool IsContainerNameUnique(string appName, string containerName, string connStr)
		{
			using (SqlConnection connection = new SqlConnection(connStr))
			{
				string queryString = $"SELECT COUNT(*) FROM containers WHERE name = '{containerName}' AND parent = (SELECT id FROM applications WHERE name = '{appName}')";

				SqlCommand command = new SqlCommand(queryString, connection);

				try
				{
					connection.Open();
					int count = (int)command.ExecuteScalar();
					return count == 0;
				}
				catch (Exception)
				{
					return false;
				}
			}
		}

		public static bool IsContentUniqueInContainer(string appName, string containerName, string content, SqlConnection connection)
		{
			string checkQueryString = $"SELECT COUNT(*) FROM data WHERE content = '{content}' " +
				$"AND parent = (SELECT id FROM containers WHERE name = '{containerName}' " +
				$"AND parent = (SELECT id FROM applications WHERE name = '{appName}'))";

			SqlCommand checkCommand = new SqlCommand(checkQueryString, connection);

			int count = (int)checkCommand.ExecuteScalar();

			return count == 0;
		}

		public static string GenerateUniqueName(string baseName)
		{
			return $"{baseName}_{DateTime.Now.Ticks}";
		}
	}
}