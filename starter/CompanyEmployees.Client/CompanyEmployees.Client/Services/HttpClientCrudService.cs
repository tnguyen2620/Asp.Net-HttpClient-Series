using Entities.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace CompanyEmployees.Client.Services
{
	public class HttpClientCrudService : IHttpClientServiceImplementation
	{
		private static readonly HttpClient _httpClient = new HttpClient();
		private readonly JsonSerializerOptions _options;

		public HttpClientCrudService() //constructor
		{
			_httpClient.BaseAddress = new Uri("https://localhost:5001/api/");
			_httpClient.Timeout = new TimeSpan(0, 0, 30);
			//set up the accepted media type by the httpClient
			_httpClient.DefaultRequestHeaders.Clear();
			_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json",0.9));
			_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
			_options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true }; //setup case insensitive deseralization option for JsonSerializer
		}

		public async Task GetCompanies()
		{
			var response = await _httpClient.GetAsync("companies");
			response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync(); //read the content of the response as a string
            var companies = new List<CompanyDto>();

			if(response.Content.Headers.ContentType.MediaType == "application/json")
			{
                companies = JsonSerializer.Deserialize<List<CompanyDto>>(content, _options);
            }
			else if (response.Content.Headers.ContentType.MediaType == "text/xml")
			{
				var doc = XDocument.Parse(content); //parse the content into the XDocument type
				foreach (var element in doc.Descendants())
				{//remove the declaration
					element.Attributes().Where(a => a.IsNamespaceDeclaration).Remove();
					//use a localname for the name property
					element.Name = element.Name.LocalName;
				}
				//create a new xmlserializer and deserialize our XDocument
				var serializer = new XmlSerializer(typeof(List<CompanyDto>));
				companies = (List<CompanyDto>)serializer.Deserialize(new StringReader(doc.ToString()));
			}
		
		}
		public async Task Execute()
		{
			await GetCompanies();
		}
	}
}
