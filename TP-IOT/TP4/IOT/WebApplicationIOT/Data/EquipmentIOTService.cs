using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace WebApplicationIOT.Data
{
    public class EquipmentIOTService
    {

        HttpClient Client { get; set; }

        public EquipmentIOTService(HttpClient client)
        {
            this.Client = client;
        }

        public async Task<List<EquipmentIOT>> GetAllEquipmentAsync()
        {

            var httpRequestMessage = new HttpRequestMessage()
            {
                Method = new HttpMethod("GET"),
                RequestUri = new Uri("http://webapiiot/EquipmentIOT/all"),
            };

            var response = await Client.SendAsync(httpRequestMessage);
           
            if(response.StatusCode == HttpStatusCode.OK)
            {
                return await response.Content.ReadFromJsonAsync<List<EquipmentIOT>>();
            }

            return null;
        }

    }
}
