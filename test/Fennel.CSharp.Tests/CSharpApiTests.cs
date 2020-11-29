using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Fennel.CSharp.Tests
{
    public class ApiTests
    {
        [Fact]
        public void ParseLine_For_Help()
        {
            var input = " #  HELP   http_requests_total    The total number of HTTP requests.";
            var result = Prometheus.ParseLine(input);
            Assert.True(result.IsHelp);
            var help = result as Help;
            Assert.Equal("http_requests_total", help.MetricName);
            Assert.Equal("The total number of HTTP requests.", help.Text);
        }
        
        [Fact]
        public void ParseLine_For_Comment()
        {
            var input = "# A comment";
            var result = Prometheus.ParseLine(input);
            Assert.True(result.IsComment);
            var comment = result as Comment;
            Assert.Equal("A comment", comment.Text);
        }
        
        [Fact]
        public void ParseLine_For_Type()
        {
            var input = "     #  TYPE    http_requests_total counter";
            var result = Prometheus.ParseLine(input);
            Assert.True(result.IsMetricType);
            var metricType = result as MetricType;
            Assert.Equal("http_requests_total", metricType.MetricName);
            Assert.Equal(MetricTypeEnum.Counter, metricType.MetricType);
        }
        
        [Fact]
        public void ParseLine_For_Metric()
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
        public void ParseLine_For_Blank()
        {
            var input = "     ";
            var result = Prometheus.ParseLine(input);
            Assert.True(result.IsBlank);
        }
        
        
        [Fact]
        public void ParseText()
        {
            var input =
@"# HELP http_requests_total The total number of HTTP requests.
# TYPE http_requests_total counter
http_requests_total{method=""post"",code=""200""} 1027 1395066363000
http_requests_total{method=""post"",code=""400""}    3 1395066363000

# Escaping in label values:
msdos_file_access_time_seconds{path=""C:\\DIR\\FILE.TXT"",error=""Cannot find file:\n \""FILE.TXT\"" ""} 1.458255915e9

# Minimalistic line:
metric_without_timestamp_and_labels 12.47

# A weird metric from before the epoch:
something_weird{problem=""division by zero""} +Inf -3982045";
            var result = Prometheus.ParseText(input);
            Assert.Equal(13, result.Count());
        }
        
        [Fact]
        public void CommentLine()
        {
            var actual = Prometheus.Comment("A comment");
            var expected = "# A comment";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void HelpLine()
        {
            var actual = Prometheus.Help("http_requests_total", "The total number of HTTP requests.");
            var expected = "# HELP http_requests_total The total number of HTTP requests.";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void MetricSimpleLine()
        {
            var actual = Prometheus.Metric("metric_without_timestamp_and_labels", 12.47);
            var expected = "metric_without_timestamp_and_labels 12.47";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void MetricLine()
        {
            var labels = new Dictionary<string, string>{ {"method", "post"}, {"code", "200"} };
            var actual = Prometheus.Metric("http_requests_total", 1027, labels, DateTimeOffset.FromUnixTimeMilliseconds(1395066363000));
            var expected = "http_requests_total{method=\"post\",code=\"200\"} 1027 1395066363000";
            Assert.Equal(expected, actual);
        }

        
    }
}