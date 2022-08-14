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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Apache.ShenYu.Client.Utils.Internal;
using org.apache.zookeeper;
using org.apache.zookeeper.data;
#if !NET40
using TaskEx = System.Threading.Tasks.Task;

#endif

namespace Apache.ShenYu.Client.Utils
{
    /// <summary>
    /// ZooKeeper client
    /// </summary>
    public class ZookeeperClient : Watcher, IZookeeperClient
    {
        #region Field

        private readonly ConcurrentDictionary<string, NodeEntry> _nodeEntries =
            new ConcurrentDictionary<string, NodeEntry>();

        private ConnectionStateChangeHandler _connectionStateChangeHandler;

        private Event.KeeperState _currentState;
        private readonly AutoResetEvent _stateChangedCondition = new AutoResetEvent(false);

        private readonly object _zkEventLock = new object();

        private bool _isDispose;

        #endregion Field

        #region Constructor

        /// <summary>
        /// create client
        /// </summary>
        /// <param name="connectionString"></param>
        public ZookeeperClient(string connectionString)
            : this(new ZkOptions(connectionString))
        {
        }

      /// <summary>
      /// create client
      /// </summary>
      /// <param name="options"></param>
        public ZookeeperClient(ZkOptions options)
        {
            Options = options;
            ZooKeeper = CreateZooKeeper();
        }

        #endregion Constructor

        #region Public Method

        /// <summary>
        /// ZooKeeperConnect object
        /// </summary>
        public ZooKeeper ZooKeeper { get; private set; }

        /// <summary>
        /// options
        /// </summary>
        public ZkOptions Options { get; }

        /// <summary>
        /// wait zk connect to give states
        /// </summary>
        /// <param name="states"></param>
        /// <param name="timeout"></param>
        /// <returns>success:true,fail:false</returns>
        public bool WaitForKeeperState(Event.KeeperState states, TimeSpan timeout)
        {
            var stillWaiting = true;
            while (_currentState != states)
            {
                if (!stillWaiting)
                {
                    return false;
                }

                stillWaiting = _stateChangedCondition.WaitOne(timeout);
            }

            return true;
        }

        /// <summary>
        /// retry util zk connected
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callable">execute zk action</param>
        /// <returns></returns>
        public async Task<T> RetryUntilConnected<T>(Func<Task<T>> callable)
        {
            var operationStartTime = DateTime.Now;
            while (true)
            {
                try
                {
                    return await callable();
                }
                catch (KeeperException.ConnectionLossException)
                {
#if NET40
                    await TaskEx.Yield();
#else
                    await Task.Yield();
#endif
                    this.WaitForRetry();
                }
                catch (KeeperException.SessionExpiredException)
                {
#if NET40
                    await TaskEx.Yield();
#else
                    await Task.Yield();
#endif
                    this.WaitForRetry();
                }

                if (DateTime.Now - operationStartTime > Options.OperatingSpanTimeout)
                {
                    throw new TimeoutException(
                        $"Operation cannot be retried because of retry timeout ({Options.OperatingSpanTimeout.TotalMilliseconds} milli seconds)");
                }
            }
        }

        /// <summary>
        /// get give node data
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<IEnumerable<byte>> GetDataAsync(string path)
        {
            path = GetZooKeeperPath(path);
            var nodeEntry = GetOrAddNodeEntry(path);
            return await RetryUntilConnected(async () => await nodeEntry.GetDataAsync());
        }

        /// <summary>
        /// get childnodes
        /// </summary>
        /// <param name="path"></param>
        /// <returns>childnodesList</returns>
        public async Task<IEnumerable<string>> GetChildrenAsync(string path)
        {
            path = GetZooKeeperPath(path);
            var nodeEntry = GetOrAddNodeEntry(path);
            return await RetryUntilConnected(async () => await nodeEntry.GetChildrenAsync());
        }

        /// <summary>
        /// node exists
        /// </summary>
        /// <param name="path"></param>
        /// <returns>if have return true，then return false。</returns>
        public async Task<bool> ExistsAsync(string path)
        {
            path = GetZooKeeperPath(path);
            var nodeEntry = GetOrAddNodeEntry(path);
            return await RetryUntilConnected(async () => await nodeEntry.ExistsAsync());
        }

        /// <summary>
        /// create node
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <param name="acls"></param>
        /// <param name="createMode"></param>
        /// <returns></returns>
        /// <remarks>
        /// 
        /// </remarks>
        public async Task<string> CreateAsync(string path, byte[] data, List<ACL> acls, CreateMode createMode)
        {
            path = GetZooKeeperPath(path);
            var nodeEntry = GetOrAddNodeEntry(path);
            return await RetryUntilConnected(async () => await nodeEntry.CreateAsync(data, acls, createMode));
        }

        public async Task<string> CreateOrUpdateAsync(string path, byte[] data, List<ACL> acls, CreateMode createMode)
        {
            path = GetZooKeeperPath(path);
            var nodeEntry = GetOrAddNodeEntry(path);
            return await RetryUntilConnected(async () => await nodeEntry.CreateOrUpdateAsync(data, acls, createMode));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <param name="acls"></param>
        /// <param name="createMode"></param>
        /// <returns></returns>
        public async Task<bool> CreateWithParentAsync(string path, byte[] data, List<ACL> acls, CreateMode createMode)
        {
            path = GetZooKeeperPath(path);
            var nodeEntry = GetOrAddNodeEntry(path);
            return await RetryUntilConnected(async () => await nodeEntry.CreateWithParentAsync(data, acls, createMode));
        }

        /// <summary>
        /// set node data
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <param name="version"></param>
        /// <returns>node stat</returns>
        public async Task<Stat> SetDataAsync(string path, byte[] data, int version = -1)
        {
            path = GetZooKeeperPath(path);

            var nodeEntry = GetOrAddNodeEntry(path);
            return await RetryUntilConnected(async () => await nodeEntry.SetDataAsync(data, version));
        }

        /// <summary>
        /// delete node
        /// </summary>
        /// <param name="path"></param>
        /// <param name="version"></param>
        public async Task DeleteAsync(string path, int version = -1)
        {
            path = GetZooKeeperPath(path);
            var nodeEntry = GetOrAddNodeEntry(path);
            await RetryUntilConnected(async () =>
            {
                await nodeEntry.DeleteAsync(version);
                return 0;
            });
        }

        /// <summary>
        /// subscribe node data change
        /// </summary>
        /// <param name="path"></param>
        /// <param name="listener"></param>
        public async Task SubscribeDataChangeAsync(string path, NodeDataChangeHandler listener)
        {
            path = GetZooKeeperPath(path);
            var node = GetOrAddNodeEntry(path);
            await node.SubscribeDataChangeAsync(listener);
        }

        /// <summary>
        /// unsubscribe node data change
        /// </summary>
        /// <param name="path"></param>
        /// <param name="listener"></param>
        public void UnSubscribeDataChange(string path, NodeDataChangeHandler listener)
        {
            path = GetZooKeeperPath(path);
            var node = GetOrAddNodeEntry(path);
            node.UnSubscribeDataChange(listener);
        }

        /// <summary>
        /// subscribe connect stat change
        /// </summary>
        /// <param name="listener"></param>
        public void SubscribeStatusChange(ConnectionStateChangeHandler listener)
        {
            _connectionStateChangeHandler += listener;
        }

        /// <summary>
        /// unsubscribe connect stat change
        /// </summary>
        /// <param name="listener"></param>
        public void UnSubscribeStatusChange(ConnectionStateChangeHandler listener)
        {
            _connectionStateChangeHandler -= listener;
        }

        /// <summary>
        /// subscribe childnode change
        /// </summary>
        /// <param name="path"></param>
        /// <param name="listener"></param>
        public async Task<IEnumerable<string>> SubscribeChildrenChange(string path, NodeChildrenChangeHandler listener)
        {
            path = GetZooKeeperPath(path);
            var node = GetOrAddNodeEntry(path);
            return await node.SubscribeChildrenChange(listener);
        }

        /// <summary>
        /// Unsubscribe childnode change
        /// </summary>
        /// <param name="path"></param>
        /// <param name="listener"></param>
        public void UnSubscribeChildrenChange(string path, NodeChildrenChangeHandler listener)
        {
            path = GetZooKeeperPath(path);
            var node = GetOrAddNodeEntry(path);
            node.UnSubscribeChildrenChange(listener);
        }

        #endregion Public Method

        #region Overrides of Watcher

        /// <summary>Processes the specified event.</summary>
        /// <param name="watchedEvent">The event.</param>
        /// <returns></returns>
        public override async Task process(WatchedEvent watchedEvent)
        {
            if (_isDispose)
                return;

            var path = watchedEvent.getPath();
            if (path == null)
            {
                await OnConnectionStateChange(watchedEvent);
            }
            else
            {
                NodeEntry nodeEntry;
                if (!_nodeEntries.TryGetValue(path, out nodeEntry))
                    return;
                await nodeEntry.OnChange(watchedEvent, false);
            }
        }

        #endregion Overrides of Watcher

        #region Implementation of IDisposable

        /// <summary>execute dispose or reset</summary>
        public void Dispose()
        {
            if (_isDispose)
                return;
            _isDispose = true;

            lock (_zkEventLock)
            {
                TaskEx.Run(async () => { await ZooKeeper.closeAsync().ConfigureAwait(false); }).ConfigureAwait(false)
                    .GetAwaiter().GetResult();
            }
        }

        #endregion Implementation of IDisposable

        #region Private Method

        private bool _isFirstConnectioned = true;

        private async Task OnConnectionStateChange(WatchedEvent watchedEvent)
        {
            if (_isDispose)
                return;

            var state = watchedEvent.getState();
            SetCurrentState(state);

            if (state == Event.KeeperState.Expired)
            {
                await ReConnect();
            }
            else if (state == Event.KeeperState.SyncConnected)
            {
                if (_isFirstConnectioned)
                {
                    _isFirstConnectioned = false;
                }
                else
                {
                    foreach (var nodeEntry in _nodeEntries)
                    {
                        await nodeEntry.Value.OnChange(watchedEvent, true);
                    }
                }
            }

            _stateChangedCondition.Set();
            if (_connectionStateChangeHandler == null)
                return;
            await _connectionStateChangeHandler(this, new ConnectionStateChangeArgs
            {
                State = state
            });
        }

        private NodeEntry GetOrAddNodeEntry(string path)
        {
            return _nodeEntries.GetOrAdd(path, k => new NodeEntry(path, this));
        }

        private ZooKeeper CreateZooKeeper()
        {
            //log write to file switch
            ZooKeeper.LogToFile = Options.LogToFile;
            return new ZooKeeper(Options.ConnectionString, (int)Options.SessionSpanTimeout.TotalMilliseconds, this,
                Options.SessionId, Options.SessionPasswdBytes, Options.ReadOnly);
        }

        private async Task ReConnect()
        {
            if (!Monitor.TryEnter(_zkEventLock, Options.ConnectionTimeout))
                return;
            try
            {
                if (ZooKeeper != null)
                {
                    try
                    {
                        await ZooKeeper.closeAsync();
                    }
                    catch
                    {
                        // ignored
                    }
                }

                ZooKeeper = CreateZooKeeper();
            }
            finally
            {
                Monitor.Exit(_zkEventLock);
            }
        }

        private void SetCurrentState(Event.KeeperState state)
        {
            lock (this)
            {
                _currentState = state;
            }
        }

        private string GetZooKeeperPath(string path)
        {
            var basePath = Options.BaseRoutePath ?? "/";

            if (!basePath.StartsWith("/"))
                basePath = basePath.Insert(0, "/");

            basePath = basePath.TrimEnd('/');

            if (!path.StartsWith("/"))
                path = path.Insert(0, "/");

            path = $"{basePath}{path.TrimEnd('/')}";
            return string.IsNullOrEmpty(path) ? "/" : path;
        }

        #endregion Private Method
    }
}
