using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Consumer
{
    public struct Product
    {
        public string id;
        public string type;
        public string name;
    }

    public class ProductClient
    {
        #nullable enable
        public async Task<System.Collections.Generic.List<Product>> GetProducts(string baseUrl, HttpClient? httpClient = null)
        {
            using var client = httpClient == null ? new HttpClient() : httpClient;

            var response = await client.GetAsync(baseUrl + "/products");
            response.EnsureSuccessStatusCode();

            var resp = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<Product>>(resp);
        }
        
        public async Task<Product?> GetProduct(string baseUrl, string productId, HttpClient? httpClient = null)
        {
            using var client = httpClient == null ? new HttpClient() : httpClient;

            var response = await client.GetAsync(baseUrl + $"/products/{productId}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            
            var resp = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Product>(resp);
        }
    }
}