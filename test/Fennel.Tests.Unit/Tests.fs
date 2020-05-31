// https://github.com/prometheus/docs/blob/master/content/docs/instrumenting/exposition_formats.md
module Tests

open System
open Fennel
open Xunit
open Fennel
open Swensen.Unquote

[<Fact>]
let ``A TYPE line: # TYPE http_requests_total counter`` () =
    let input = "     #  TYPE    http_requests_total counter"
    let expected = (MetricName "http_requests_total", MetricType.Counter)|> Line.Type |> Result<Line,string>.Ok
    let actual = input |> Prometheus.parseLine
    test <@ expected = actual @>
    
[<Fact>]
let ``A HELP line: # HELP http_requests_total The total number of HTTP requests.`` () =
    let input = " #  HELP   http_requests_total    The total number of HTTP requests."
    let expected = (MetricName "http_requests_total", DocString "The total number of HTTP requests.") |> Line.Help |> Result<Line,string>.Ok
    let actual = input |> Prometheus.parseLine
    test <@ expected = actual @>
    
[<Fact>]
let ``A Comment line: # A comment:`` () =
    let input = "     #  A comment:"
    let expected = "A comment:" |> Line.Comment |> Result<Line,string>.Ok
    let actual = input |> Prometheus.parseLine
    test <@ expected = actual @>
    
[<Fact>]
let ``A METRIC line: http_requests_total{method="post",code="200"} 1027 1395066363000`` () =
    let input = """http_requests_total{method="post",code="200"} 1027 1395066363000"""
    let expected =
            {   Name = MetricName "http_requests_total"
                Labels = [("method","post");("code","200")]
                Value = FloatValue 1027.
                Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(1395066363000L) |> Timestamp |> Some }
            |> Line.Metric |> Result<Line,string>.Ok
    let actual = input |> Prometheus.parseLine
    test <@ expected = actual @>
    
[<Fact>]
let ``A METRIC line: metric_without_timestamp_and_labels 12.47`` () =
    let input = """metric_without_timestamp_and_labels 12.47"""
    let expected =
            {   Name = MetricName "metric_without_timestamp_and_labels"
                Labels = []
                Value = FloatValue 12.47
                Timestamp = None }
            |> Line.Metric |> Result<Line,string>.Ok
    let actual = input |> Prometheus.parseLine
    test <@ expected = actual @>
    
[<Fact>]
let ``A METRIC line: something_weird{problem="division by zero"} +Inf -3982045`` () =
    let input = """something_weird{problem="division by zero"} -Inf -3982045"""
    let expected =
            {   Name = MetricName "something_weird"
                Labels = [("problem", "division by zero")]
                Value = FloatValue -infinity
                Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(-3982045L) |> Timestamp |> Some }
            |> Line.Metric |> Result<Line,string>.Ok
    let actual = input |> Prometheus.parseLine
    test <@ expected = actual @>
    
//[<Fact>]
//let scratch() = Prometheus.test()
