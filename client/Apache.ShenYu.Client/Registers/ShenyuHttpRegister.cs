/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Apache.ShenYu.Client.Models.DTO;
using Apache.ShenYu.Client.Options;
using Apache.ShenYu.Client.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Apache.ShenYu.Client.Registers
{
    public class ShenyuHttpRegister : IShenyuRegister
    {
        private List<string> ServerList { get; set; }

        private Dictionary<string, string> AccessTokens { get; set; }

        private HttpClient _client;
        private URIRegisterDTO _uriRegisterDto;
        private ShenyuOptions _shenyuOptions;
        private readonly ILogger<ShenyuHttpRegister> _logger;
        private string _userName;
        private string _password;

        public ShenyuHttpRegister(ILogger<ShenyuHttpRegister> logger)
        {
            this._logger = logger;
        }

        public async Task Init(ShenyuOptions shenyuOptions)
        {
            this._shenyuOptions = shenyuOptions;
            this._shenyuOptions.Register.Props.TryGetValue(Constants.RegisterConstants.UserName, out this._userName);
            this._shenyuOptions.Register.Props.TryGetValue(Constants.RegisterConstants.Password, out this._password);
            this.ServerList = this._shenyuOptions.Register.ServerList?.Split(',').ToList();
            this.AccessTokens = new Dictionary<string, string>();
            this._client = new HttpClient();
            await this.SetAccessTokens();
        }

        public async Task PersistInterface(MetaDataRegisterDTO metadata)
        {
            await this.DoRegister(metadata, Constants.MetaPath, Constants.MetaType);
        }

        public async Task PersistURI(URIRegisterDTO registerDTO)
        {
            await this.DoRegister(registerDTO, Constants.UriPath, Constants.UriType);
            this._uriRegisterDto = registerDTO;
        }

        public Task Close()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get access token
        ///
        /// </summary>
        /// <returns></returns>
        private async Task SetAccessTokens()
        {
            List<Task> tasks = new List<Task>();
            foreach (var server in this.ServerList)
            {
                tasks.Add(this.SetAccessToken(server));
            }

            await Task.WhenAll(tasks);
            _logger.LogInformation("request access token successfully!");
        }

        private async Task SetAccessToken(string server)
        {
            var builder = new UriBuilder(server)
            {
                Path = Constants.LoginPath,
                Query = $"userName={this._userName}&password={this._password}"
            };
            var resp = await this._client.GetStringAsync(builder.ToString());
            var jObject = JObject.Parse(resp);
            var token = jObject["data"]?["token"].ToString();

            this.AccessTokens.Add(server, token);
        }

        private async Task DoRegister<T>(T t, string path, string type)
        {
            foreach (var server in this.ServerList)
            {
                string accessToken;
                if (!this.AccessTokens.TryGetValue(server, out accessToken))
                {
                    await this.SetAccessToken(server);
                    if (!this.AccessTokens.TryGetValue(server, out accessToken))
                    {
                        throw new NullReferenceException("access token is null.");
                    }
                }

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri($"{server}{path}"));
                requestMessage.Headers.Add(Constants.XAccessToken, accessToken);
                var content = JsonConvert.SerializeObject(t);
                requestMessage.Content = new StringContent(
                    content,
                    Encoding.UTF8, "application/json");

                var resp = await this._client.SendAsync(requestMessage);
                if (resp.IsSuccessStatusCode)
                {
                    this._logger.LogInformation("succeeded to register type: {}, content: {}", type, content);
                }
                else
                {
                    this._logger.LogWarning("failed to register type: {}, content: {}", type, content, resp);
                }
            }
        }
    }
}