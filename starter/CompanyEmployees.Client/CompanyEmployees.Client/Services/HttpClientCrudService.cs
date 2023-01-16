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
			_options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true }; //setup case insensitive deseralization option for JsonSerializer
		}

		public async Task GetCompanies()
		{
            var response = await _httpClient.GetAsync("companies");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var companies = JsonSerializer.Deserialize<List<CompanyDto>>(content, _options);
        }
		public async Task Execute()
		{
            //await GetCompanies();
            //await GetCompaniesWithXmlHeader();
            //await CreateCompanyWithHttpRequestMessage();
            await UpdateCompanyWithHttpRequestMessage();
        }

		public async Task GetCompaniesWithXmlHeader()
		{
            var request = new HttpRequestMessage(HttpMethod.Get, "companies");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var doc = XDocument.Parse(content);
            foreach (var element in doc.Descendants())
            {
                element.Attributes().Where(a => a.IsNamespaceDeclaration).Remove();
                element.Name = element.Name.LocalName;
            }
            var serializer = new XmlSerializer(typeof(List<CompanyDto>));
            var companies = (List<CompanyDto>)serializer.Deserialize(new StringReader(doc.ToString()));
        }

        private async Task CreateCompanyWithHttpRequestMessage()
        {
            var companyForCreation = new CompanyForCreationDto
            {
                Name = "MX3",
                Address = "Drummond Street, Carlton 3053",
                Country = "Australia"
            };

            var company = JsonSerializer.Serialize(companyForCreation);

            var request = new HttpRequestMessage(HttpMethod.Post, "companies");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(company, Encoding.UTF8);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var createdCompany = JsonSerializer.Deserialize<CompanyDto>(content, _options);
        }

        private async Task UpdateCompanyWithHttpRequestMessage()
        {
            var companyForUpdate = new CompanyForUpdateDto
            {
                Name = "MX3",
                Address = "43/255 Drummond Street, Carlton 3053",
                Country = "Australia"
            };

            var company = JsonSerializer.Serialize(companyForUpdate);
            var uri = Path.Combine("companies", "DD261A1A-1DA5-4023-9F8B-08DAF768D2FD");
            var request = new HttpRequestMessage(HttpMethod.Put, uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(company, Encoding.UTF8);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var createdCompany = JsonSerializer.Deserialize<CompanyDto>(content, _options);
        }
    }
}
