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

using System.Threading.Tasks;
using Apache.ShenYu.Client.Utils;
using Google.Protobuf;
using Xunit;

namespace Apache.ShenYu.Client.Tests.Utils
{
    public class EtcdUtilsTest
    {
        [Fact]
        public async Task EtcdClientTest()
        {
            //var client = new EtcdClientUtils(new EtcdOptions()
            //{
            //    Address = "http://127.0.0.1:2379",
            //    TTL = 100
            //});
            //var tt = client.GetVal("/shenyu/register/metadata/http/etcddotnet/etcddotnet--etcddotnet-weather");
            //client.Put("foo/bar2", "testbar2");
            //var val01 = client.GetVal("foo/bar2");

            //var rep = client.PutEphemeral("foo/bar3", "bar33");
            //var val02 = client.GetVal("foo/bar3");
            //var val03 = client.Get(new Etcdserverpb.RangeRequest()
            //{
            //    Key = ByteString.CopyFromUtf8("foo/bar3")
            //});
            await Task.CompletedTask;
        }
    }
}
