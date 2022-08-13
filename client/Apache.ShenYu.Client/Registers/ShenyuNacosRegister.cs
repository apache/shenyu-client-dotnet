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
using System.Threading.Tasks;
using Apache.ShenYu.Client.Models.DTO;
using Apache.ShenYu.Client.Options;
using Apache.ShenYu.Client.Utils;
using Microsoft.Extensions.Logging;
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
        private static string _nacosNameSpace = "nacosNameSpace";
        private static string  _uriMetadata = "uriMetadata";
        private static string _nacosDefaultGroup = "DEFAULT_GROUP";
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

        public override async Task Init(ShenyuOptions shenyuOptions)
        {
            this._shenyuOptions = shenyuOptions;
            NacosSdkOptions options = JsonConvert.DeserializeObject<NacosSdkOptions>(JsonConvert.SerializeObject(shenyuOptions.Register.Props));
            // special handler this 
            options.ServerAddresses = this._shenyuOptions.Register.ServerList?.Split(',').ToList();//cluster split with ,                                                                                                
            this._shenyuOptions.Register.Props.TryGetValue(_nacosNameSpace, out string nacosNameSpace);
            options.Namespace = string.IsNullOrEmpty(nacosNameSpace) ? options.Namespace : nacosNameSpace;
            var op = Microsoft.Extensions.Options.Options.Create(options);

            this._namingService = new NacosNamingService(this._loggerFactory, op, this._httpClientFactory);
            this._configService = new NacosConfigService(this._loggerFactory, op);

            await Task.CompletedTask;
        }

        public override async Task PersistInterface(MetaDataRegisterDTO metadata)
        {
            /** orignal code ,now use below
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
            **/

            string contextPath = ContextPathUtils.BuildRealNode(metadata.contextPath, metadata.appName);
            await RegisterConfigAsync(contextPath, metadata);
        }

        public override async Task PersistURI(URIRegisterDTO registerDTO)
        {
            /** orignal code ,now use below
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
            await this._namingService.RegisterInstance(serviceName, instance).ConfigureAwait(false);
            **/

            string contextPath = ContextPathUtils.BuildRealNode(registerDTO.contextPath, registerDTO.appName);
            await RegisterServiceAsync(contextPath,registerDTO);
        }

        public override async Task Close()
        {
            await this._configService.ShutDown();
            await this._namingService.ShutDown();
        }

        private async Task RegisterServiceAsync(string contextPath,URIRegisterDTO registerDTO)
        {
            string serviceName = RegisterPathConstants.BuildServiceInstancePath(registerDTO.rpcType);
            var uriRegString = JsonConvert.SerializeObject(registerDTO, Formatting.None,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            Instance instance = new Instance();
            instance.Ephemeral = true;
            instance.Ip = registerDTO.host;
            instance.Port = registerDTO.port;
            instance.Metadata = new Dictionary<string, string>();
            instance.Metadata.Add(Constants.CONTEXT_PATH,contextPath);
            instance.Metadata.Add(_uriMetadata, uriRegString);
            try
            {
                await this._namingService.RegisterInstance(serviceName, instance)
                                         .ConfigureAwait(false);
            }catch(Exception ex)
            {
                _logger.LogError(ex, "nacos register serviceInatance fail,please check");
                throw new Exception("nacos register serviceInstance fail,please check", ex);
            }
            _logger.LogInformation($"nacos register serviceInstance uri success:{serviceName}");
        }

        private async Task RegisterConfigAsync(string contextPath, MetaDataRegisterDTO metadata)
        {
            var metadataStr = JsonConvert.SerializeObject(metadata, Formatting.None,
                              new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            string configName = RegisterPathConstants.BuildServiceConfigPath(metadata.rpcType, contextPath);
            lock (this.metadataSet)
            {
                this.metadataSet.Add(metadataStr);
            }

            var set = JsonConvert.SerializeObject(this.metadataSet, Formatting.None,
                     new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            try
            {
                var publishResult = await this._configService.PublishConfig(configName, _nacosDefaultGroup, set);
                if (publishResult)
                {
                    _logger.LogInformation($"nacos register metadata success: {metadata.ruleName}");
                }
                else
                {
                    throw new Exception("nacos register metadata fail,please check");
                }
            }catch(Exception ex)
            {
                _logger.LogError(ex, "nacos register metadata fail,please check");
                throw new Exception("nacos register metadata fail,please check", ex);
            }
        }
    }
}
