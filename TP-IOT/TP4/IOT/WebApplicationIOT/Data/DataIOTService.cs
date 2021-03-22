using Models;
using Models.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace WebApplicationIOT.Data
{
    public class DataIOTService
    {

        HttpClient Client { get; set; }

        public DataIOTService(HttpClient client)
        {
            this.Client = client;
        }

        public async Task<List<DataIOT>> GetAllDataEquipmentAsync(string id)
        {

            var httpRequestMessage = new HttpRequestMessage()
            {
                Method = new HttpMethod("GET"),
                RequestUri = new Uri("http://webapiiot/DataIOT/equipment/"+id),
            };

            var response = await Client.SendAsync(httpRequestMessage);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return await response.Content.ReadFromJsonAsync<List<DataIOT>>();
            }

            return null;
        }

        public async Task<List<DataIOT>> GetAllDataEquipmentSearchAsync(string id, long timestamp)
        {

            SearchDataPeriodView search = new()
            {
                EquipmentID = id,
                TimestampAfter = timestamp
            };

            var httpRequestMessage = new HttpRequestMessage()
            {
                Method = new HttpMethod("POST"),
                RequestUri = new Uri("http://webapiiot/DataIOT/search/"),
                Content = JsonContent.Create(search)
            };

            var response = await Client.SendAsync(httpRequestMessage);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return await response.Content.ReadFromJsonAsync<List<DataIOT>>();
            }

            return null;
        }
    }
}
