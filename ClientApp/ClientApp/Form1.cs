using System;
using System.Drawing;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientApp
{
	public partial class Form1 : Form
	{
		private const string baseUrl = "http://localhost:50255/api/somiod";

		public Form1()
		{
			InitializeComponent();
			InitializeMotorAppStatus();
		}

		private async void InitializeMotorAppStatus()
		{
			string urlOn = $"{baseUrl}/app01/cont01/data/on";
			string urlOff = $"{baseUrl}/app01/cont01/data/off";

			HttpStatusCode status = await GetMotorAppStatus(urlOn);

			if (status == HttpStatusCode.OK)
			{
				labelStatus.Text = "ON";
				labelStatus.ForeColor = Color.Green;
			}
			else
			{
				status = await GetMotorAppStatus(urlOff);

				if (status == HttpStatusCode.OK)
				{
					labelStatus.Text = "OFF";
					labelStatus.ForeColor = Color.Red;
				}
				else
				{
					labelStatus.Text = "NOT SET";
					labelStatus.ForeColor = Color.Gray;
				}
			}
		}

		private async Task<HttpStatusCode> GetMotorAppStatus(string url)
		{
			using (HttpClient httpClient = new HttpClient())
			{
				try
				{
					HttpResponseMessage response = await httpClient.GetAsync(url);

					return response.StatusCode;
				}
				catch (Exception ex)
				{
					MessageBox.Show("An error occurred: " + ex.Message);
					return HttpStatusCode.InternalServerError;
				}
			}
		}

		private async void btnCreateApp_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(tbAppName.Text))
			{
				MessageBox.Show("Request invalid. The 'application name' is required.");
			}

			string getUrl = $"{baseUrl}/{tbAppName.Text}";
			string postUrl = baseUrl;
			string postXmlContent = $"<application><name>{tbAppName.Text}</name></application>";

			HttpStatusCode status = await SendGetRequest(getUrl);

			if (status == HttpStatusCode.OK)
			{
				MessageBox.Show($"The application '{tbAppName.Text}' already exists.");

				labelAppStatus.Text = $"'{tbAppName.Text}' created already";
				labelAppStatus.ForeColor = Color.Green;
			}
			else
			{
				await SendPostRequest(postUrl, postXmlContent);

				MessageBox.Show("The application was created successfully.");

				labelAppStatus.Text = $"'{tbAppName.Text}' created";
				labelAppStatus.ForeColor = Color.Green;
			}
		}

		private async void btnOn_Click(object sender, EventArgs e)
		{
			string getUrl = $"{baseUrl}/app01/cont01/data/on";
			string deleteUrl = $"{baseUrl}/app01/cont01/data/off";
			string postUrl = $"{baseUrl}/app01/cont01";
			string postXmlContent = "<data><content>on</content></data>";

			HttpStatusCode status = await SendGetRequest(getUrl);

			if (status == HttpStatusCode.OK)
			{
				MessageBox.Show("The state is already 'on'.");
			}
			else
			{
				await SendDeleteRequest(deleteUrl);

				await SendPostRequest(postUrl, postXmlContent);

				labelStatus.Text = "ON";
				labelStatus.ForeColor = Color.Green;

				MessageBox.Show("Request successful. The state is now 'on'.");
			}
		}

		private async void btnOff_Click(object sender, EventArgs e)
		{
			string getUrl = $"{baseUrl}/app01/cont01/data/off";
			string deleteUrl = $"{baseUrl}/app01/cont01/data/on";
			string postUrl = $"{baseUrl}/app01/cont01";
			string postXmlContent = "<data><content>off</content></data>";

			HttpStatusCode status = await SendGetRequest(getUrl);

			if (status == HttpStatusCode.OK)
			{
				MessageBox.Show("The state is already 'off'.");
			}
			else
			{
				await SendDeleteRequest(deleteUrl);

				await SendPostRequest(postUrl, postXmlContent);

				labelStatus.Text = "OFF";
				labelStatus.ForeColor = Color.Red;

				MessageBox.Show("Request successful. The state is now 'off'.");
			}
		}

		private async Task<HttpStatusCode> SendGetRequest(string url)
		{
			using (HttpClient httpClient = new HttpClient())
			{
				try
				{
					HttpResponseMessage response = await httpClient.GetAsync(url);

					return response.StatusCode;
				}
				catch (Exception ex)
				{
					MessageBox.Show("An error occurred: " + ex.Message);
					return HttpStatusCode.InternalServerError;
				}
			}
		}

		private async Task SendPostRequest(string url, string xmlContent)
		{
			using (HttpClient httpClient = new HttpClient())
			{
				HttpContent content = new StringContent(xmlContent, Encoding.UTF8, "application/xml");

				try
				{
					HttpResponseMessage response = await httpClient.PostAsync(url, content);

					if (response.IsSuccessStatusCode)
					{
						string responseData = await response.Content.ReadAsStringAsync();
					}
					else
					{
						MessageBox.Show("POST request failed. Status Code: " + response.StatusCode);
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show("An error occurred: " + ex.Message);
				}
			}
		}

		private async Task SendDeleteRequest(string url)
		{
			using (HttpClient httpClient = new HttpClient())
			{
				try
				{
					await httpClient.DeleteAsync(url);
				}
				catch (Exception ex)
				{
					MessageBox.Show("An error occurred: " + ex.Message);
				}
			}
		}
	}
}