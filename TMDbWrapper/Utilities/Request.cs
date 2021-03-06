﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TmdbWrapper.Search;


namespace TmdbWrapper.Utilities
{
    internal class Request<T> where T : ITmdbObject, new()
    {
        private const string BASE_URL = @"http://api.themoviedb.org/3/";   

        public string ApiName { get; private set; }
        private IDictionary<string, string> Parameters = new Dictionary<string, string>();

        public Request(string apiName)
        {
            if (string.IsNullOrWhiteSpace(TheMovieDb.ApiKey))
            {
                throw new ArgumentNullException("The library was not initialized correctly. Please specify an API_KEY.");
            }
            ApiName = apiName;
            AddParameter("api_key", TheMovieDb.ApiKey);
        }

        public void AddParameter(string key, string value)
        {
            Parameters.Add(key, value);
        }

        public void AddParameter(string key, object value)
        {
            AddParameter(key, value.ToString());
        }

        public string RequestUrl
        {
            get 
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(ApiName);
                sb.Append("?");
                var args = from n in Parameters
                           select n.Key + "=" + n.Value;
                sb.Append(string.Join("&", args));
                return sb.ToString();
            }
        }

        public async Task<T> ProcesRequestAsync()
        {
            var json = await TheMovieDb.Requester.Get(BASE_URL + RequestUrl);
            //HttpClient client = new HttpClient();
            //client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            //string response = await client.GetStringAsync(BASE_URL + RequestUrl);
            JObject jsonObject = JObject.Parse(json);

            T result = new T();
            result.ProcessJson(jsonObject);

            return result;
        }

        public async Task<SearchResult<T>> ProcessSearchRequestAsync()
        {
            if (Parameters["page"] == "0")
            {
                Parameters["page"] = "1";
                SearchResult<T> result = await GetSearchResponseAsync();
                for (int i = 2; i <= result.TotalPages; i++)
                {
                    Parameters["page"] = i.ToString();
                    SearchResult<T> subResult = await GetSearchResponseAsync();
                    result.Results.AddRange(subResult.Results);
                }
                result.TotalPages = 1;
                return result;
            }
            else
            {
                return await GetSearchResponseAsync();
            }
        
        }

        private async Task<SearchResult<T>> GetSearchResponseAsync()
        {
            var json = await TheMovieDb.Requester.Get(BASE_URL + RequestUrl);
            JObject jsonObject = JObject.Parse(json);

            SearchResult<T> result = new SearchResult<T>();
            result.Page = jsonObject.GetValue("page").ToObject<int>();
            result.TotalPages = jsonObject.GetValue("total_pages").ToObject<int>(); 
            result.TotalResults = jsonObject.GetValue("total_results").ToObject<int>(); 

            var jsonObjects = jsonObject["results"];
            result.Results = new List<T>();
            foreach (var jsonObj in jsonObjects)
            {
                var obj = new T();
                obj.ProcessJson((JObject) jsonObj);
                result.Results.Add(obj);
            }
            return result;
        }

        public async Task<IReadOnlyList<T>> ProcesRequestListAsync(string valueName)
        {
            var json = await TheMovieDb.Requester.Get(BASE_URL + RequestUrl);
            JObject jsonObject = JObject.Parse(json);
            IReadOnlyList<T> result = jsonObject.ProcessArray<T>("valueName");
            return result;
        }
    }
}
