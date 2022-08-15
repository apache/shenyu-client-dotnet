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
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Apache.ShenYu.Client.Attributes;
using Apache.ShenYu.Client.Models.DTO;
using Apache.ShenYu.Client.Options;
using Apache.ShenYu.Client.Registers;
using Apache.ShenYu.Client.Utils;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apache.ShenYu.AspNetCore.Services
{
    public class ShenyuStartupService : IHostedService
    {
        private readonly ILogger<ShenyuStartupService> _logger;
        private readonly IServer _server;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IShenyuRegister _shenyuRegister;
        private readonly IOptions<ShenyuOptions> _shenyuOptions;
        private readonly ApplicationPartManager _partManager;

        public ShenyuStartupService(
            ILogger<ShenyuStartupService> logger,
            IServer server,
            IHostApplicationLifetime hostApplicationLifetime,
            IShenyuRegister shenyuRegister,
            IOptions<ShenyuOptions> shenyuOptions,
            ApplicationPartManager partManager
        )
        {
            this._logger = logger;
            this._server = server;
            this._hostApplicationLifetime = hostApplicationLifetime;
            this._shenyuRegister = shenyuRegister;
            this._shenyuOptions = shenyuOptions;
            this._partManager = partManager;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this._hostApplicationLifetime.ApplicationStarted.Register(async () =>
            {
                await this._shenyuRegister.Init(this._shenyuOptions.Value);

                var addresses = this.GetAddresses();
                var ipAddresses = GetIpAddresses(addresses);
                
                var rpcTypes = this._shenyuOptions.Value.Client.ClientType
                    .Split(',')
                    .Select(type => type.Trim())
                    .ToList();
                foreach (var address in ipAddresses)
                {
                    var uri = new Uri(address);
                    if (!rpcTypes.Contains(uri.Scheme))
                    {
                        continue;
                    }

                    // if isFull equals to true, set the whole application as proxy
                    if (this._shenyuOptions.Value.Client.IsFull)
                    {
                        await this._shenyuRegister.PersistInterface(this.BuildMetadataDto(null, address));
                    }
                    else
                    {
                        var controllerFeature = new ControllerFeature();
                        this._partManager.PopulateFeature(controllerFeature);

                        var controllers = controllerFeature.Controllers;
                        // traverse all actions
                        foreach (var controller in controllers)
                        {
                            var shenyuClientAttr = controller.GetCustomAttribute<ShenyuClientAttribute>();
                            if (shenyuClientAttr != null && shenyuClientAttr.Path.Contains("**"))
                            {
                                // proxy the whole controller
                                await this._shenyuRegister.PersistInterface(this.BuildMetadataDto(shenyuClientAttr.Path,
                                    address));
                                continue;
                            }

                            var routeAttr = controller.GetCustomAttribute<RouteAttribute>();
                            if (routeAttr == null)
                            {
                                continue;
                            }

                            var basePath = this.GetPath(shenyuClientAttr, routeAttr, controller.Name);

                            var methods = controller.GetMethods();
                            foreach (var method in methods)
                            {
                                // only support action with one HttpMethodAttribute for now
                                var methodRouteAttr = method.GetCustomAttribute<HttpMethodAttribute>();
                                if (methodRouteAttr == null)
                                {
                                    continue;
                                }

                                var methodShenyuAttr = method.GetCustomAttribute<ShenyuClientAttribute>();
                                var path = this.GetPath(methodShenyuAttr, methodRouteAttr);

                                var wholePath = path == null ? basePath : Path.Combine("/", basePath, path.TrimStart('/'));
                                //fix if wholePath like "path01\\path02"
                                if(wholePath != null && wholePath.Contains("\\")){
                                    wholePath = wholePath.Replace("\\", "/");
                                }

                                await this._shenyuRegister.PersistInterface(this.BuildMetadataDto(wholePath, address));
                            }
                        }
                    }

                    await this._shenyuRegister.PersistURI(this.BuildUriRegisterDto(address));
                }
            });

            return Task.CompletedTask;
        }

        private ICollection<string> GetIpAddresses(ICollection<string> addresses)
        {
            var ipAddresses = new HashSet<string>();
            var ip = IpUtils.GetLocalIPv4(NetworkInterfaceType.Ethernet);
            foreach (var address in addresses)
            {
                var uriBuilder = new UriBuilder(address);
                uriBuilder.Host = ip;
                var ipAddr = uriBuilder.Uri.ToString();
                ipAddresses.Add(ipAddr);
            }
            return ipAddresses;
        }

        private ICollection<string> GetAddresses()
        {
            return this._server.Features.Get<IServerAddressesFeature>().Addresses;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private MetaDataRegisterDTO BuildMetadataDto(string path, string address)
        {
            var uri = new Uri(address);
            return new MetaDataRegisterDTO
            {
                appName = this._shenyuOptions.Value.Client.AppName,
                contextPath = this._shenyuOptions.Value.Client.ContextPath,
                path = this._shenyuOptions.Value.Client.ContextPath + (string.IsNullOrEmpty(path) ? "/**" : path),
                rpcType = uri.Scheme,
                ruleName = this._shenyuOptions.Value.Client.ContextPath + (string.IsNullOrEmpty(path) ? "/**" : path),
                enabled = true,
            };
        }

        private URIRegisterDTO BuildUriRegisterDto(string address)
        {
            var uri = new Uri(address);
            return new URIRegisterDTO
            {
                protocol = $"{uri.Scheme}://",
                appName = this._shenyuOptions.Value.Client.AppName,
                contextPath = this._shenyuOptions.Value.Client.ContextPath,
                rpcType = this._shenyuOptions.Value.Client.ClientType,
                host = uri.Host,
                port = uri.Port,
            };
        }

        private string GetPath(ShenyuClientAttribute shenyuClientAttribute, IRouteTemplateProvider routeAttribute,
            string controllerName = null)
        {
            string routePath = routeAttribute.Template == null ? "" : routeAttribute.Template;
            if (routePath.Equals("[controller]") && controllerName != null)
            {
                routePath = controllerName
                    .Substring(0, controllerName.IndexOf("Controller", StringComparison.Ordinal))
                    .ToLower();
            }

            return shenyuClientAttribute != null ? shenyuClientAttribute.Path : routePath;
        }
    }
}
