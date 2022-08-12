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
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;

namespace Apache.ShenYu.IntegrationTests
{
    public class HttpIntegrationTest : IDisposable
    {
        private HttpClient _client;

        public HttpIntegrationTest()
        {
            this._client = new HttpClient();
            this._client.BaseAddress = new Uri("http://localhost:9195");
        }

        public void Dispose()
        {
            this._client?.Dispose();
        }

        [Fact]
        public async Task HttpTest()
        {
            var resp = await this._client.GetAsync("/dotnet/template/hello");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            string responseBody = await resp.Content.ReadAsStringAsync();
            var body = JsonConvert.DeserializeObject<List<object>>(responseBody);
            Assert.Equal(5, body.Count);
        }
    }
}
