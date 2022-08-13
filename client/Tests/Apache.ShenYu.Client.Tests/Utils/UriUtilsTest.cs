using System;
using Apache.ShenYu.Client.Utils;
using Xunit;

namespace Apache.ShenYu.Client.Tests.Utils
{
    public class UriUtilsTest
    {
        [Fact]
        public void Test()
        {
            string url = "http://127.0.0.1:8848/nacos/index.html#/serviceDetail?name=shenyu.register.service.http&groupName=DEFAULT_GROUP";
            Uri uri = new Uri(url);
           var path= UriUtils.GetPathWithParams(uri);
            Assert.True(path == url);
        }
    }
}
