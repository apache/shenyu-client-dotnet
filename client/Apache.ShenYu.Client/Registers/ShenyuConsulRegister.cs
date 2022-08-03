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
using System.Text;
using System.Threading.Tasks;
using Apache.ShenYu.Client.Models.DTO;
using Apache.ShenYu.Client.Options;
using Consul;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Apache.ShenYu.Client.Registers
{
    public class ShenyuConsulRegister : ShenyuAbstractRegister
    {
        private readonly ILogger<ShenyuConsulRegister> _logger;
        private ShenyuOptions _shenyuOptions;
        private ConsulClient _client;

        public ShenyuConsulRegister(ILogger<ShenyuConsulRegister> logger)
        {
            _logger = logger;
        }

        public override Task Init(ShenyuOptions shenyuOptions)
        {
            this._shenyuOptions = shenyuOptions;
            ConsulClientConfiguration config = new ConsulClientConfiguration();
            config.Address = new Uri(this._shenyuOptions.Register.ServerList);
            this._client = new ConsulClient(config);

            return Task.CompletedTask;
        }

        public override async Task PersistInterface(MetaDataRegisterDTO metadata)
        {
            var metadataStr = JsonConvert.SerializeObject(metadata, Formatting.None,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            string contextPath = BuildContextNodePath(metadata.contextPath, metadata.appName);
            string parentPath = $"/shenyu/register/metadata/{metadata.rpcType}/{contextPath}";
            string nodeName = BuildMetadataNodeName(metadata);
            string nodePath = $"{parentPath}/{nodeName}".Substring(1);
            var putPair = new KVPair(nodePath) { Value = Encoding.UTF8.GetBytes(metadataStr) };
            await this._client.KV.Put(putPair);
        }

        public override Task PersistURI(URIRegisterDTO registerDTO)
        {
            var uriRegString = JsonConvert.SerializeObject(registerDTO, Formatting.None,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var dic = new Dictionary<string, string> { { "uri", uriRegString } };
            this._client.Agent.ServiceRegister(new AgentServiceRegistration
            {
                ID = this._shenyuOptions.Register.Props["Id"],
                Name = this._shenyuOptions.Register.Props["Name"],
                Tags = this._shenyuOptions.Register.Props["Tags"].Split(','),
                Port = Int32.Parse(this._shenyuOptions.Register.Props["Port"]),
                Address = "localhost",
                EnableTagOverride = Boolean.Parse(this._shenyuOptions.Register.Props["EnableTagOverride"]),
                Meta = dic,
            });

            return Task.CompletedTask;
        }

        public override Task Close()
        {
            return Task.CompletedTask;
        }
    }
}
