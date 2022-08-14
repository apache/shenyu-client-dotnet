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
using dotnet_etcd;
using Etcdserverpb;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Apache.ShenYu.Client.Utils
{
    /// <summary>
    /// etcd client.
    /// </summary>
    public class EtcdClientUtils
    {
        private readonly EtcdClient _client;
        private readonly string _authToken;
        private readonly bool _authNeed;
        private readonly Grpc.Core.Metadata _metadata;
        private long _globalLeaseId;
        private ILogger _logger = NullLogger<EtcdClient>.Instance;

        public EtcdClientUtils(EtcdOptions options)
        {
            //cluster:like "https://localhost:23790,https://localhost:23791,https://localhost:23792"
            this._client = new EtcdClient(options.Address);
            //auth
            if (!string.IsNullOrEmpty(options.UserName) && !string.IsNullOrEmpty(options.Password))
            {
                var authRes = this._client.Authenticate(new Etcdserverpb.AuthenticateRequest()
                {
                    Name = options.UserName,
                    Password = options.Password,
                });
                _authToken = authRes.Token;
                _authNeed = true;
                _metadata = _authNeed ? new Grpc.Core.Metadata() { new Grpc.Core.Metadata.Entry("token", _authToken) } : null;
            }
            InitLease(options);
        }

        /// <summary>
        ///  rent
        /// </summary>
        private void InitLease(EtcdOptions options)
        {
            try
            {
                // create rent id to bind
                var response = this._client.LeaseGrant(new Etcdserverpb.LeaseGrantRequest()
                {
                    TTL = options.TTL
                });
                this._globalLeaseId = response.ID;
                var tokenSource = new CancellationTokenSource();

                this._client.LeaseKeepAlive(new LeaseKeepAliveRequest()
                {
                    ID = _globalLeaseId
                }, (x) =>
                {
                    Console.WriteLine(x.ID);
                }, tokenSource.Token, _metadata);
            }
            catch (Exception ex)
            {
                _logger.LogError("Init Lease error", ex);
            }
        }

        public void Close()
        {
            this._client.Dispose();
        }

        public RangeResponse Get(RangeRequest request)
        {
            var response = _client.Get(request, _metadata);
            return response;
        }

        public string GetVal(string key)
        {
            return _client.GetVal(key, _metadata);
        }

        public async Task<string> GetValAsync(string key)
        {
            return await _client.GetValAsync(key, _metadata);
        }

        public IDictionary<string, string> GetRangeVals(string prefixKey)
        {
            return _client.GetRangeVal(prefixKey, _metadata);
        }

        public async Task<IDictionary<string, string>> GetRangeValsAsync(string prefixKey)
        {
            return await _client.GetRangeValAsync(prefixKey, _metadata);
        }

        public long Delete(string key)
        {
            var response = _client.Delete(key, _metadata);
            return response.Deleted;
        }

        public async Task<long> DeleteAsync(string key)
        {
            var response = await _client.DeleteAsync(key, _metadata);
            return response.Deleted;
        }

        public long DeleteRange(string perfixKey)
        {
            var response = _client.DeleteRange(perfixKey, _metadata);
            return response.Deleted;
        }

        public async Task<long> DeleteRangeAsync(string perfixKey)
        {
            var response = await _client.DeleteRangeAsync(perfixKey, _metadata);
            return response.Deleted;
        }

        public Etcdserverpb.PutResponse Put(string key, string value)
        {
            return _client.Put(key, value, _metadata);
        }

        public async Task<Etcdserverpb.PutResponse> PutAsync(string key, string value)
        {
            return await _client.PutAsync(key, value, _metadata);
        }

        public PutResponse PutEphemeral(string key, string value)
        {
            try
            {
                PutRequest request = new PutRequest()
                {
                    Key = ByteString.CopyFromUtf8(key),
                    Value = ByteString.CopyFromUtf8(value),
                    Lease = this._globalLeaseId
                };
                var response = _client.Put(request, _metadata);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError("putEphemeral(key:{},value:{}) error.", key, value, ex);
            }
            return null;
        }

        public async Task<PutResponse> PutEphemeralAsync(string key, string value)
        {
            try
            {
                PutRequest request = new PutRequest()
                {
                    Key = ByteString.CopyFromUtf8(key),
                    Value = ByteString.CopyFromUtf8(value),
                    Lease = this._globalLeaseId
                };
                var response = await _client.PutAsync(request, _metadata);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError("putEphemeral(key:{},value:{}) error.", key, value, ex);
            }
            return null;
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
