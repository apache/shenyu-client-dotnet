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
using System.Threading.Tasks;
using Apache.ShenYu.Client.Models.DTO;
using Apache.ShenYu.Client.Options;
using Apache.ShenYu.Client.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Apache.ShenYu.Client.Registers
{
    public class ShenyuEtcdRegister : ShenyuAbstractRegister
    {
        private readonly ILogger<ShenyuEtcdRegister> _logger;
        private EtcdClientUtils _etcdClient;
        private ShenyuOptions _shenyuOptions;

        public ShenyuEtcdRegister(ILogger<ShenyuEtcdRegister> logger)
        {
            this._logger = logger;
        }

        public override async Task Init(ShenyuOptions shenyuOptions)
        {
            if (string.IsNullOrEmpty(shenyuOptions.Register.ServerList))
            {
                throw new System.ArgumentException("serverList can not be null.");
            }
            this._shenyuOptions = shenyuOptions;
            var props = shenyuOptions.Register.Props;
            long timeout = Convert.ToInt64(props.GetValueOrDefault(Constants.RegisterConstants.EtcdTimeout, "3000"));
            long ttl = Convert.ToInt64(props.GetValueOrDefault(Constants.RegisterConstants.EtcdTTL, "5"));
            props.TryGetValue(Constants.RegisterConstants.UserName, out string userName);
            props.TryGetValue(Constants.RegisterConstants.Password, out string password);
            _etcdClient = new EtcdClientUtils(new EtcdOptions()
            {
                Address = shenyuOptions.Register.ServerList,
                UserName = userName,
                Password = password,
                TTL = ttl,
                Timeout = timeout,
            });
            await Task.CompletedTask;
        }

        public override async Task PersistInterface(MetaDataRegisterDTO metadata)
        {
            string contextPath = ContextPathUtils.BuildRealNode(metadata.contextPath, metadata.appName);
            await RegisterMetadataAsync(contextPath, metadata);
        }

        public override async Task PersistURI(URIRegisterDTO registerDTO)
        {
            string contextPath = ContextPathUtils.BuildRealNode(registerDTO.contextPath, registerDTO.appName);
            await RegisterURIAsync(contextPath, registerDTO);
        }

        public override async Task Close()
        {
            this._etcdClient.Close();
            await Task.CompletedTask;
        }

        private async Task RegisterMetadataAsync(string contextPath, MetaDataRegisterDTO metadata)
        {
            String metadataNodeName = BuildMetadataNodeName(metadata);
            String metaDataPath = RegisterPathConstants.BuildMetaDataParentPath(metadata.rpcType, contextPath);
            String realNode = RegisterPathConstants.BuildRealNode(metaDataPath, metadataNodeName);
            var metadataStr = JsonConvert.SerializeObject(metadata, Formatting.None,
                               new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            await this._etcdClient.PutEphemeralAsync(realNode, metadataStr);
            _logger.LogInformation("register metadata success: {}", realNode);
        }

        private async Task RegisterURIAsync(string contextPath, URIRegisterDTO registerDTO)
        {
            string uriNodeName = BuildURINodeName(registerDTO);
            string uriPath = RegisterPathConstants.BuildURIParentPath(registerDTO.rpcType, contextPath);
            string realNode = RegisterPathConstants.BuildRealNode(uriPath, uriNodeName);
            string nodeData = JsonConvert.SerializeObject(registerDTO, Formatting.None, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            await this._etcdClient.PutEphemeralAsync(realNode, nodeData);
            _logger.LogInformation("register uri data success: {}", realNode);
        }
    }
}
