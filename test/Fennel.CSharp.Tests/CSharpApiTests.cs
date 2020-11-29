using System;
using System.Diagnostics;
using Xunit;

namespace Fennel.CSharp.Tests
{
    public class ApiTests
    {
        [Fact]
        public void Test_ParseLine_For_Help()
        {
            var input = " #  HELP   http_requests_total    The total number of HTTP requests.";
            var result = Prometheus.ParseLine(input);
            Assert.True(result.IsHelp);
            var help = result as Help;
            Assert.Equal("http_requests_total", help.MetricName);
            Assert.Equal("The total number of HTTP requests.", help.Text);
        }
        
        [Fact]
        public void Test_ParseLine_For_Comment()
        {
            var input = "# A comment";
            var result = Prometheus.ParseLine(input);
            Assert.True(result.IsComment);
            var comment = result as Comment;
            Assert.Equal("A comment", comment.Text);
        }
        
        [Fact]
        public void Test_ParseLine_For_Type()
        {
            var input = "     #  TYPE    http_requests_total counter";
            var result = Prometheus.ParseLine(input);
            Assert.True(result.IsMetricType);
            var metricType = result as MetricType;
            Assert.Equal("http_requests_total", metricType.MetricName);
            Assert.Equal(MetricTypeEnum.Counter, metricType.MetricType);
        }
        
        [Fact]
        public void Test_ParseLine_For_Metric()
        {
            var input = "http_requests_total{method=\"post\",code=\"200\"} 1027 1395066363000";
            var result = Prometheus.ParseLine(input);
            Assert.True(result.IsMetric);
            var metric = result as Metric;
            Assert.Equal("http_requests_total", metric.MetricName);
            Assert.NotNull(metric.MetricValue);
            Assert.Equal(1027.0, metric.MetricValue.Value);
            Assert.Contains("method", metric.Labels);
            Assert.Contains("code", metric.Labels);
            Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(1395066363000), metric.Timestamp);
        }
        
        [Fact]
        public void Test_ParseLine_For_Blank()
        {
            var input = "     ";
            var result = Prometheus.ParseLine(input);
            Assert.True(result.IsBlank);
        }
        
        
    }
}