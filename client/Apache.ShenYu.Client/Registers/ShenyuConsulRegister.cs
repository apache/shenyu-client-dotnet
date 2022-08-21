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
using Apache.ShenYu.Client.Utils;
using Consul;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static Apache.ShenYu.Client.Utils.Constants;

namespace Apache.ShenYu.Client.Registers
{
    public class ShenyuConsulRegister : ShenyuAbstractRegister
    {
        private readonly ILogger<ShenyuConsulRegister> _logger;
        private ShenyuOptions _shenyuOptions;
        private ConsulClient _client;
        private AgentServiceRegistration _service;
        private static char SEPARATOR = '-';

        public ShenyuConsulRegister(ILogger<ShenyuConsulRegister> logger)
        {
            _logger = logger;
        }

        public override Task Init(ShenyuOptions shenyuOptions)
        {
            if (string.IsNullOrEmpty(shenyuOptions.Register.ServerList))
            {
                throw new System.ArgumentException("serverList can not be null.");
            }
            this._shenyuOptions = shenyuOptions;
            ConsulClientConfiguration config = new ConsulClientConfiguration();
            config.Address = new Uri(this._shenyuOptions.Register.ServerList);
            config.Token = shenyuOptions.Register.Props.GetValueOrDefault(RegisterConstants.Token, "");
            this._client = new ConsulClient(config);
            _service = GetAgentService();
            return Task.CompletedTask;
        }

        public override async Task PersistInterface(MetaDataRegisterDTO metadata)
        {
            string contextPath = ContextPathUtils.BuildRealNode(metadata.contextPath, metadata.appName);
            string metadataNodeName = BuildMetadataNodeName(metadata);
            string metaDataPath = RegisterPathConstants.BuildMetaDataParentPath(metadata.rpcType, contextPath);
            string realNode = RegisterPathConstants.BuildRealNode(metaDataPath, metadataNodeName);
            realNode = UriUtils.RemovePrefix(realNode);//remove prefix /
            var metadataStr = JsonConvert.SerializeObject(metadata, Formatting.None,
                               new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var putPair = new KVPair(realNode) { Value = Encoding.UTF8.GetBytes(metadataStr) };
            await this._client.KV.Put(putPair);
            _logger.LogInformation($"{metadata.rpcType} Consul client register success: {metadataStr}");
        }

        public override Task PersistURI(URIRegisterDTO registerDTO)
        {
            var uriRegString = JsonConvert.SerializeObject(registerDTO, Formatting.None,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var dic = new Dictionary<string, string> { { Constants.UriType, uriRegString } };
            _service.Meta = dic;
            this._client.Agent.ServiceRegister(_service);
            return Task.CompletedTask;
        }

        public override async Task Close()
        {
            await this._client.Agent.ServiceDeregister(this._shenyuOptions.Register.Props[Constants.RegisterConstants.Id]);
        }

        private AgentServiceRegistration GetAgentService()
        {
            var props = this._shenyuOptions.Register.Props;
            //check data
            string appName = NormalizeForDns(props[Constants.RegisterConstants.Name]);
            string instanceId = NormalizeForDns(props[Constants.RegisterConstants.Id]);
            string portStr = props[Constants.RegisterConstants.Port];
            if (string.IsNullOrEmpty(portStr))
            {
                throw new System.ArgumentException("Port can not be null.");
            }
            string tagsStr = props[Constants.RegisterConstants.Tags];
            string[] tags = null ;
            if (!string.IsNullOrEmpty(tagsStr))
            {
                tags = tagsStr.Split(',');
            }
            string address = props.GetValueOrDefault(Constants.RegisterConstants.HostName,"localhost");
            _service = new AgentServiceRegistration
            {
                ID = instanceId,
                Name = appName,
                Tags = tags,
                Port = Int32.Parse(portStr),
                Address = address,
                EnableTagOverride = Boolean.Parse(props.GetValueOrDefault(Constants.RegisterConstants.EnableTagOverride, "false"))
            };
            return _service;
        }

        private string NormalizeForDns(string s)
        {
            if (string.ReferenceEquals(s, null) || !char.IsLetter(s[0]) || !char.IsLetterOrDigit(s[s.Length - 1]))
            {
                throw new System.ArgumentException("Consul service ids must not be empty, must start "
                    + "with a letter, end with a letter or digit, "
                    + "and have as interior characters only letters, "
                    + "digits, and hyphen: " + s);
            }

            StringBuilder normalized = new StringBuilder();
            char? prev = null;
            foreach (char curr in s.ToCharArray())
            {
                char? toAppend = null;
                if (char.IsLetterOrDigit(curr))
                {
                    toAppend = curr;
                }
                else if (prev == null || !(prev == SEPARATOR))
                {
                    toAppend = SEPARATOR;
                }
                if (toAppend != null)
                {
                    normalized.Append(toAppend);
                    prev = toAppend;
                }
            }

            return normalized.ToString();
        }
    }
}
