using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            var client = new EtcdClientUtils(new EtcdOptions()
            {
                 Address = "http://127.0.0.1:2379",
                 TTL = 100
            });
           var tt = client.GetVal("/shenyu/register/metadata/http/etcddotnet/etcddotnet--etcddotnet-weather");
            client.Put("foo/bar2", "testbar2");
            var val01 = client.GetVal("foo/bar2");

            var rep = client.PutEphemeral("foo/bar3", "bar33");
            var val02 = client.GetVal("foo/bar3");
            var val03 = client.Get(new Etcdserverpb.RangeRequest()
            {
                Key = ByteString.CopyFromUtf8("foo/bar3")
            });

        }
    }
}
