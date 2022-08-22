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

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apache.ShenYu.Client.Utils;
using org.apache.zookeeper;
using Xunit;

namespace Apache.ShenYu.Client.Tests.Utils
{
    public class ZookeeperUtilsTest
    {
        [Fact]
        public async Task ZkClientTest()
        {
            //ZkOptions zkConfig = new ZkOptions("127.0.0.1:2181");
            //zkConfig
            //        .SetOperatingTimeout(50000)
            //        .SetSessionTimeout(100000000)
            //        .SetConnectionTimeout(10000000);
            //var zkClient = new ZookeeperClient(zkConfig);
            //await zkClient.CreateWithParentAsync("/test/ch", Encoding.UTF8.GetBytes("hello"), ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
            //await zkClient.SetDataAsync("/test/ch", Encoding.UTF8.GetBytes("hello"));
            //var test = await zkClient.GetDataAsync("/test/ch");
            //var strtest = Encoding.UTF8.GetString(test.ToArray());

            ////   await zkClient.CreateWithParentAsync("/shenyu/register/metadata/http/zk2dotnet/zk2dotnet--zk2dotnet-template", Encoding.UTF8.GetBytes("hello"), ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
            ////  await zkClient.SetDataAsync("/shenyu/register/metadata/http/zk2dotnet/zk2dotnet--zk2dotnet-template", Encoding.UTF8.GetBytes("youcan"));
            ////  var result =  await zkClient.GetDataAsync("/shenyu/register/metadata/http/zk2dotnet/zk2dotnet--zk2dotnet-template2");
            ////var str = Encoding.UTF8.GetString(result.ToArray());
            await Task.CompletedTask;
        }
    }
}
