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
using System.Threading;
using System.Threading.Tasks;
using org.apache.zookeeper;
using org.apache.zookeeper.data;

#if !NET40

using TaskEx = System.Threading.Tasks.Task;

#endif

namespace Apache.ShenYu.Client.Utils
{
    /// <summary>
    /// zookeeper client
    /// </summary>
    public class ZookeeperClient : Watcher, IZookeeperClient
    {
        private ZooKeeper _zookeeperClient;
        private ZkOptions _options;

        private ConnectionStateChangeHandler _connectionStateChangeHandler;

        private Event.KeeperState _currentState;
        private readonly AutoResetEvent _stateChangedCondition = new AutoResetEvent(false);

        private readonly object _zkEventLock = new object();

        private bool _isDispose;

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
            _options = options;
            _zookeeperClient = CreateZooKeeper();
        }

        #region Public Method

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

                if (DateTime.Now - operationStartTime > _options.OperatingSpanTimeout)
                {
                    throw new TimeoutException(
                        $"Operation cannot be retried because of retry timeout ({_options.OperatingSpanTimeout.TotalMilliseconds} milli seconds)");
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
            return await RetryUntilConnected(async () =>
            {
                var data = await _zookeeperClient.getDataAsync(path, false);
                return data?.Data;
            });
        }

        /// <summary>
        /// node exists
        /// </summary>
        /// <param name="path"></param>
        /// <returns>if have return true，then return false。</returns>
        public async Task<bool> ExistsAsync(string path)
        {
            path = GetZooKeeperPath(path);
            return await RetryUntilConnected(async () =>
            {
                var data = await _zookeeperClient.existsAsync(path, false);
                var exists = data != null;
                return exists;
            });
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
            return await RetryUntilConnected(async () =>
            {
                path = await _zookeeperClient.createAsync(path, data, acls, createMode);
                return path;
            });
        }

        public async Task<string> CreateOrUpdateAsync(string path, byte[] data, List<ACL> acls, CreateMode createMode)
        {
            path = GetZooKeeperPath(path);
            return await RetryUntilConnected(async () =>
            {
                var existsResult = await _zookeeperClient.existsAsync(path, false) != null;
                if (existsResult)
                {
                    await _zookeeperClient.setDataAsync(path, data);
                }
                else
                {
                    path = await _zookeeperClient.createAsync(path, data, acls, createMode);
                }
                return path;
            });
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
            return await RetryUntilConnected(async () =>
            {
                var paths = path.Trim('/').Split('/');
                var cur = "";
                foreach (var item in paths)
                {
                    if (string.IsNullOrEmpty(item))
                    {
                        continue;
                    }
                    cur += $"/{item}";
                    var existStat = await _zookeeperClient.existsAsync(cur, null);
                    if (existStat != null)
                    {
                        continue;
                    }

                    if (cur.Equals(path))
                    {
                        await _zookeeperClient.createAsync(cur, data, acls, createMode);
                    }
                    else
                    {
                        await _zookeeperClient.createAsync(cur, null, acls, createMode);
                    }
                }
                return await Task.FromResult(true);
            });
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
            return await RetryUntilConnected(async () =>
            {
                var stat = await _zookeeperClient.setDataAsync(path, data, version);
                return stat;
            });
        }

        /// <summary>
        /// delete node
        /// </summary>
        /// <param name="path"></param>
        /// <param name="version"></param>
        public async Task DeleteAsync(string path, int version = -1)
        {
            path = GetZooKeeperPath(path);
            await RetryUntilConnected(async () =>
            {
                await _zookeeperClient.deleteAsync(path, version);
                return 0;
            });
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

        #endregion Public Method

        #region Overrides of Watcher

        /// <summary>Processes the specified event.</summary>
        /// <param name="watchedEvent">The event.</param>
        /// <returns></returns>
        public override async Task process(WatchedEvent watchedEvent)
        {
            if (_isDispose)
                return;

            await OnConnectionStateChange(watchedEvent);
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
                TaskEx.Run(async () => { await _zookeeperClient.closeAsync().ConfigureAwait(false); }).ConfigureAwait(false)
                    .GetAwaiter().GetResult();
            }
        }

        #endregion Implementation of IDisposable

        #region Private Method

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

            _stateChangedCondition.Set();
            if (_connectionStateChangeHandler == null)
                return;
            await _connectionStateChangeHandler(this, new ConnectionStateChangeArgs
            {
                State = state
            });
        }

        private ZooKeeper CreateZooKeeper()
        {
            //log write to file switch
            ZooKeeper.LogToFile = _options.LogToFile;
            return new ZooKeeper(_options.ConnectionString, (int)_options.SessionSpanTimeout.TotalMilliseconds, this,
                _options.SessionId, _options.SessionPasswdBytes, _options.ReadOnly);
        }

        private async Task ReConnect()
        {
            if (!Monitor.TryEnter(_zkEventLock, _options.ConnectionSpanTimeout))
                return;
            try
            {
                if (_zookeeperClient != null)
                {
                    try
                    {
                        await _zookeeperClient.closeAsync();
                    }
                    catch
                    {
                        // ignored
                    }
                }

                _zookeeperClient = CreateZooKeeper();
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
            var basePath = _options.BaseRoutePath ?? "/";

            if (!basePath.StartsWith("/"))
                basePath = basePath.Insert(0, "/");

            basePath = basePath.TrimEnd('/');

            if (!path.StartsWith("/"))
                path = path.Insert(0, "/");

            path = $"{basePath}{path.TrimEnd('/')}";
            return string.IsNullOrEmpty(path) ? "/" : path;
        }

        /// <summary>
        /// wait util zk connect success，timeout is in options
        /// </summary>
        private void WaitForRetry()
        {
            WaitForKeeperState(Watcher.Event.KeeperState.SyncConnected, _options.OperatingSpanTimeout);
        }

        #endregion Private Method
    }
}
