using Newtonsoft.Json.Linq;
using System;
using System.IO;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;

namespace TNCServicePlatformAccessDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var secretFileJObject = JObject.Parse(File.ReadAllText("secret.json"));
            var apiName = secretFileJObject["WebAppAuth"]["AppName"].ToString();
            var apiUrl = secretFileJObject["WebAppAuth"]["AppUrl"].ToString();
            var clientId = secretFileJObject["WebAppAuth"]["ClientId"].ToString();
            var clientSecret = secretFileJObject["WebAppAuth"]["ClientSecret"].ToString();
            var aadAuthority = @"https://login.microsoftonline.com/fb280588-57d8-4416-9821-e337832bfa02";
            var resourceId = @"https://tncservices.azurewebsites.net";
            
            var authContext = new AuthenticationContext(aadAuthority);
            var clientCredential = new ClientCredential(clientId, clientSecret);

            //AuthenticationContext AuthenticationContext = new AuthenticationContext("https://login.microsoftonline.com/fb280588-57d8-4416-9821-e337832bfa02", 
            //                                                        false, TokenCache.DefaultShared); // Authority
            
            //var authenticationRequestHeader = AuthenticationContext.AcquireTokenAsync(apiUrl, clientCredential).Result.CreateAuthorizationHeader();

            //var httpWebRequest = (HttpWebRequest)WebRequest.Create(apiUrl + "/api/Values/_echo/WebSiteKey");
            //httpWebRequest.Headers.Add("Authorization", authenticationRequestHeader);
            //var response = httpWebRequest.GetResponseAsync().Result;

            var authenticationContext = new AuthenticationContext(aadAuthority, null);
            var tokenResult = authenticationContext.AcquireTokenAsync(resourceId, clientCredential).Result;

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.AccessToken);
                //httpWebRequest.Headers.Add("Authorization", await authToken);

                var getResponse = httpClient.GetAsync(apiUrl + "/api/Values/_echo/WebSiteKey").Result;
                Console.WriteLine("here");
                //.PostAsync(todoListBaseAddress + "/api/todolist", content);
            }

            Console.WriteLine("Hello World!");
        }
        
    }
}
