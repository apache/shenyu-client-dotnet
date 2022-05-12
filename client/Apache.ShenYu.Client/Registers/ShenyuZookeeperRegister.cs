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
using System.Threading;
using System.Threading.Tasks;
using Apache.ShenYu.Client.Models.DTO;
using Apache.ShenYu.Client.Options;
using Apache.ShenYu.Client.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using org.apache.zookeeper;

namespace Apache.ShenYu.Client.Registers
{
    public class ShenyuZookeeperRegister : IShenyuRegister
    {
        private readonly ILogger<ShenyuZookeeperRegister> _logger;
        private string _serverList;
        private ShenyuOptions _shenyuOptions;
        private int _sessionTimeout;
        private ZooKeeper _zkClient;
        private Dictionary<string, string> _nodeDataMap = new Dictionary<string, string>();

        public ShenyuZookeeperRegister(ILogger<ShenyuZookeeperRegister> logger)
        {
            _logger = logger;
        }

        public Task Init(ShenyuOptions shenyuOptions)
        {
            this._shenyuOptions = shenyuOptions;
            this._serverList = shenyuOptions.Register.ServerList;
            this._sessionTimeout =
                Convert.ToInt32(
                    this._shenyuOptions.Register.Props.GetValueOrDefault(Constants.RegisterConstants.SessionTimeout,
                        "3000"));
            this._zkClient = CreateClient(this._serverList, this._sessionTimeout, new StatWatcher(this));
            return Task.CompletedTask;
        }

        public async Task PersistInterface(MetaDataRegisterDTO metadata)
        {
            // build metadata path
            string rpcType = metadata.rpcType;
            string contextPath = BuildContextNodePath(metadata.contextPath, metadata.appName);

            // create parent node
            string parentPath = $"/shenyu/register/metadata/{metadata.rpcType}/{contextPath}";
            await _zkClient.CreateWithParentAsync(parentPath, null, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);

            // create or set metadata node
            string nodeName = BuildMetadataNodeName(metadata);
            string nodePath = $"{parentPath}/{nodeName}";

            var existStat = await _zkClient.existsAsync(nodePath);
            // create node
            var metadataStr = JsonConvert.SerializeObject(metadata, Formatting.None, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            if (existStat == null)
            {
                await _zkClient.createAsync(nodePath, Encoding.UTF8.GetBytes(metadataStr), ZooDefs.Ids.OPEN_ACL_UNSAFE,
                    CreateMode.PERSISTENT);
            }
            else
            {
                // update node
                await _zkClient.setDataAsync(nodePath, Encoding.UTF8.GetBytes(metadataStr));
            }
        }

        public async Task PersistURI(URIRegisterDTO registerDTO)
        {
            // build uri path
            string contextPath = BuildContextNodePath(registerDTO.contextPath, registerDTO.appName);
            string parentPath =
                $"/shenyu/register/uri/{registerDTO.rpcType}/{contextPath}";
            await _zkClient.CreateWithParentAsync(parentPath, null, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);

            string nodePath = $"{parentPath}/{registerDTO.host}:{registerDTO.port}";

            // create ephemeral node
            var existStat = await _zkClient.existsAsync(nodePath, null);
            if (existStat == null)
            {
                var uriRegString = JsonConvert.SerializeObject(registerDTO, Formatting.None, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                _nodeDataMap[nodePath] = uriRegString;
                await _zkClient.createAsync(nodePath, Encoding.UTF8.GetBytes(uriRegString), ZooDefs.Ids.OPEN_ACL_UNSAFE,
                    CreateMode.EPHEMERAL);
            }
        }

        public async Task Close()
        {
            await this._zkClient.closeAsync();
        }

        private ZooKeeper CreateClient(string connString, int sessionTimeout, Watcher watcher, string chroot = null)
        {
            var zk = new ZooKeeper(connString, sessionTimeout, watcher);

            // waiting for connection finished
            Thread.Sleep(1000);
            while (zk.getState() == ZooKeeper.States.CONNECTING)
            {
                Thread.Sleep(1000);
            }

            var state = zk.getState();
            if (state != ZooKeeper.States.CONNECTED && state != ZooKeeper.States.CONNECTEDREADONLY)
            {
                throw new Exception($"failed to connect to zookeeper endpoint: {connString}");
            }

            return zk;
        }

        private string BuildContextNodePath(string contextPath, string appName)
        {
            return string.IsNullOrEmpty(contextPath)
                ? appName
                : (contextPath.StartsWith("/")
                    ? contextPath.Substring(1)
                    : contextPath);
        }

        private string BuildMetadataNodeName(MetaDataRegisterDTO metadata)
        {
            string nodeName;
            string rpcType = metadata.rpcType;

            if (Constants.RegisterRpcType.Http.Equals(rpcType) || Constants.RegisterRpcType.SpringCloud.Equals(rpcType))
            {
                nodeName = string.Join(Constants.SelectorJoinRule, metadata.contextPath,
                    metadata.ruleName.Replace(Constants.PathSeparator, Constants.SelectorJoinRule));
            }
            else
            {
                nodeName = string.Join(".", metadata.serviceName, metadata.methodName);
            }

            return nodeName.StartsWith(Constants.PathSeparator) ? nodeName.Substring(1) : nodeName;
        }

        class StatWatcher : Watcher
        {
            private readonly ShenyuZookeeperRegister _register;

            public StatWatcher(ShenyuZookeeperRegister register)
            {
                _register = register;
            }

            public override async Task process(WatchedEvent @event)
            {
                var state = @event.getState();

                if (Event.KeeperState.SyncConnected.Equals(state))
                {
                    foreach (var node in this._register._nodeDataMap)
                    {
                        var existStat = await this._register._zkClient.existsAsync(node.Key);
                        if (existStat != null)
                        {
                            await this._register._zkClient.CreateWithParentAsync(node.Key,
                                Encoding.UTF8.GetBytes(node.Value),
                                ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.EPHEMERAL);
                            this._register._logger.LogInformation("zookeeper client register success: {}", node.Value);
                        }
                    }
                }
            }
        }
    }
}