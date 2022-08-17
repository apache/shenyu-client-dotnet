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

using System.Threading.Tasks;
using Apache.ShenYu.Client.Models.DTO;
using Apache.ShenYu.Client.Options;
using dotnet_etcd;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Apache.ShenYu.Client.Registers
{
    public class ShenyuEtcdRegister : ShenyuAbstractRegister
    {
        private readonly ILogger<ShenyuEtcdRegister> _logger;
        private ShenyuOptions _shenyuOptions;
        private EtcdClient _client;

        public ShenyuEtcdRegister(ILogger<ShenyuEtcdRegister> logger)
        {
            this._logger = logger;
        }

        public override Task Init(ShenyuOptions shenyuOptions)
        {
            this._shenyuOptions = shenyuOptions;
            this._client = new EtcdClient(shenyuOptions.Register.ServerList);

            return Task.CompletedTask;
        }

        public override async Task PersistInterface(MetaDataRegisterDTO metadata)
        {
            // create key
            string contextPath = BuildContextNodePath(metadata.contextPath, metadata.appName);
            string parentPath = $"/shenyu/register/metadata/{metadata.rpcType}/{contextPath}";
            // create or set metadata node
            string nodeName = BuildMetadataNodeName(metadata);
            string nodePath = $"{parentPath}/{nodeName}";

            // create value
            var metadataStr = JsonConvert.SerializeObject(metadata, Formatting.None,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            await this._client.PutAsync(nodePath, metadataStr);
            this._logger.LogInformation("succeeded to register metadata, key: {}, value: {}", nodePath, metadataStr);
        }

        public override async Task PersistURI(URIRegisterDTO registerDTO)
        {
            // build uri path
            string contextPath = BuildContextNodePath(registerDTO.contextPath, registerDTO.appName);
            string parentPath =
                $"/shenyu/register/uri/{registerDTO.rpcType}/{contextPath}";
            string nodePath = $"{parentPath}/{registerDTO.host}:{registerDTO.port}";
            var uriRegString = JsonConvert.SerializeObject(registerDTO, Formatting.None,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            await this._client.PutAsync(nodePath, uriRegString);
            this._logger.LogInformation("succeeded to register uri, key: {}, value: {}", nodePath, uriRegString);
        }

        public override Task Close()
        {
            this._client.Dispose();
            return Task.CompletedTask;
        }
    }
}
