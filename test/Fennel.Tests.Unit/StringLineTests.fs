module StringLineTests

open System
open Fennel
open Xunit
open Swensen.Unquote


[<Fact>]
let ``A TYPE line: # TYPE http_requests_total counter`` () =
    let expected = "# TYPE http_requests_total counter"
    let actual = Prometheus.typeHint "http_requests_total" MetricType.Counter
    test <@ expected = actual @>

[<Fact>]
let ``A HELP line: # HELP http_requests_total The total number of HTTP requests.`` () =
    let expected = "# HELP http_requests_total The total number of HTTP requests."
    let actual = Prometheus.help "http_requests_total" "The total number of HTTP requests."
    test <@ expected = actual @>
    

[<Fact>]
let ``A Comment line: # A comment:`` () =
    let expected = "# A comment" 
    let actual = Prometheus.comment "A comment"
    test <@ expected = actual @>
    
[<Fact>]
let ``A METRIC line: metric_without_timestamp_and_labels 12.47`` () =
    let expected = """metric_without_timestamp_and_labels 12.47"""
    let actual = Prometheus.metricSimple "metric_without_timestamp_and_labels" 12.47
    test <@ expected = actual @>    
    
[<Fact>]
let ``A METRIC line: http_requests_total 1027 1395066363000`` () =
    let expected = """http_requests_total 1027 1395066363000"""
    
    let name = "http_requests_total"
    let labels = []
    let value = 1027.
    let dt = DateTimeOffset.FromUnixTimeMilliseconds(1395066363000L)
    let actual = Prometheus.metric name value labels dt 
    test <@ expected = actual @>
    
[<Fact>]
let ``A METRIC line: http_requests_total{method="post",code="200"} 1027 1395066363000`` () =
    let expected = """http_requests_total{method="post",code="200"} 1027 1395066363000"""
    
    let name = "http_requests_total"
    let labels = [("method","post");("code","200")]
    let value = 1027.
    let dt = DateTimeOffset.FromUnixTimeMilliseconds(1395066363000L)
    let actual = Prometheus.metric name value labels dt
    test <@ expected = actual @>
    
[<Fact>]
let ``A METRIC line: http_requests_total{method="post",code="200"} 1027`` () =
    let expected = """http_requests_total{method="post",code="200"} 1027"""
    
    let name = "http_requests_total"
    let labels = [("method","post");("code","200")]
    let value = 1027.
    let actual = Prometheus.metricSansTime name value labels
    test <@ expected = actual @>    