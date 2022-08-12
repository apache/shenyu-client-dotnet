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

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Apache.ShenYu.Client.Models.DTO;
using Apache.ShenYu.Client.Options;
using Apache.ShenYu.Client.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nacos.V2;
using Nacos.V2.Config;
using Nacos.V2.Naming;
using Nacos.V2.Naming.Dtos;
using Newtonsoft.Json;

namespace Apache.ShenYu.Client.Registers
{
    public class ShenyuNacosRegister : ShenyuAbstractRegister
    {
        private readonly ILogger<ShenyuNacosRegister> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private NacosNamingService _namingService;
        private NacosConfigService _configService;
        private ShenyuOptions _shenyuOptions;
        private HashSet<string> metadataSet = new HashSet<string>();

        public ShenyuNacosRegister(ILoggerFactory loggerFactory, ILogger<ShenyuNacosRegister> logger)
        {
            this._logger = logger;
            this._loggerFactory = loggerFactory;
            this._httpClientFactory = new DefaultHttpClientFactory();
        }

        public override Task Init(ShenyuOptions shenyuOptions)
        {
            this._shenyuOptions = shenyuOptions;
            NacosSdkOptions options = new NacosSdkOptions();
            options.Namespace = this._shenyuOptions.Register.Props["Namespace"];
            options.UserName = this._shenyuOptions.Register.Props["UserName"];
            options.Password = this._shenyuOptions.Register.Props["Password"];
            options.ServerAddresses = new List<string>() { { this._shenyuOptions.Register.ServerList } };
            var op = Microsoft.Extensions.Options.Options.Create(options);

            this._namingService =
                new NacosNamingService(this._loggerFactory, op, this._httpClientFactory);
            this._configService = new NacosConfigService(this._loggerFactory, op);

            return Task.CompletedTask;
        }

        public override async Task PersistInterface(MetaDataRegisterDTO metadata)
        {
            var metadataStr = JsonConvert.SerializeObject(metadata, Formatting.None,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            string configPath = $"shenyu.register.service.{metadata.rpcType}.{metadata.contextPath.Substring(1)}";
            lock (this.metadataSet)
            {
                this.metadataSet.Add(metadataStr);
            }

            var set = JsonConvert.SerializeObject(this.metadataSet, Formatting.None,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            await this._configService.PublishConfig(configPath, "DEFAULT_GROUP", set);
        }

        public override async Task PersistURI(URIRegisterDTO registerDTO)
        {
            string serviceName = $"shenyu.register.service.{registerDTO.rpcType}";
            var uriRegString = JsonConvert.SerializeObject(registerDTO, Formatting.None,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            Instance instance = new Instance();
            instance.Ephemeral = true;
            instance.Ip = registerDTO.host;
            instance.Port = registerDTO.port;
            instance.Metadata = new Dictionary<string, string>();
            instance.Metadata.Add("contextPath", registerDTO.contextPath.Substring(1));
            instance.Metadata.Add("uriMetadata", uriRegString);
            await this._namingService.RegisterInstance(serviceName, instance);
        }

        public override async Task Close()
        {
            await this._configService.ShutDown();
            await this._namingService.ShutDown();
        }
    }
}
