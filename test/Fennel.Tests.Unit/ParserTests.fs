// https://github.com/prometheus/docs/blob/master/content/docs/instrumenting/exposition_formats.md
module ParserTests

open System
open Fennel
open Xunit
open Swensen.Unquote

[<Fact>]
let ``A TYPE line: # TYPE http_requests_total counter`` () =
    let input = "     #  TYPE    http_requests_total counter"

    let expected =
        (MetricName "http_requests_total", MetricType.Counter)
        |> Line.Type
        |> Result<Line, string>.Ok

    let actual = input |> Prometheus.parseLine
    test <@ expected = actual @>

[<Fact>]
let ``A HELP line: # HELP http_requests_total The total number of HTTP requests.`` () =
    let input = " #  HELP   http_requests_total    The total number of HTTP requests."

    let expected =
        (MetricName "http_requests_total", DocString "The total number of HTTP requests.")
        |> Line.Help
        |> Result<Line, string>.Ok

    let actual = input |> Prometheus.parseLine
    test <@ expected = actual @>

[<Fact>]
let ``A Comment line: # A comment:`` () =
    let input = "     #  A comment:"
    let expected = "A comment:" |> Line.Comment |> Result<Line, string>.Ok
    let actual = input |> Prometheus.parseLine
    test <@ expected = actual @>

[<Fact>]
let ``A METRIC line: http_requests_total{method="post",code="200"} 1027 1395066363000`` () =
    let input = """http_requests_total{method="post",code="200"} 1027 1395066363000"""

    let expected =
        { Name = MetricName "http_requests_total"
          Labels = [ ("method", "post"); ("code", "200") ]
          Value = FloatValue 1027.
          Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(1395066363000L) |> Timestamp |> Some }
        |> Line.Metric
        |> Result<Line, string>.Ok

    let actual = input |> Prometheus.parseLine
    test <@ expected = actual @>

[<Fact>]
let ``A METRIC line: metric_without_timestamp_and_labels 12.47`` () =
    let input = """metric_without_timestamp_and_labels 12.47"""

    let expected =
        { Name = MetricName "metric_without_timestamp_and_labels"
          Labels = []
          Value = FloatValue 12.47
          Timestamp = None }
        |> Line.Metric
        |> Result<Line, string>.Ok

    let actual = input |> Prometheus.parseLine
    test <@ expected = actual @>

[<Fact>]
let ``A METRIC line: something_weird{problem="division by zero"} +Inf -3982045`` () =
    let input = """something_weird{problem="division by zero"} -Inf -3982045"""

    let expected =
        { Name = MetricName "something_weird"
          Labels = [ ("problem", "division by zero") ]
          Value = FloatValue -infinity
          Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(-3982045L) |> Timestamp |> Some }
        |> Line.Metric
        |> Result<Line, string>.Ok

    let actual = input |> Prometheus.parseLine
    test <@ expected = actual @>

[<Fact>]
let ``Read entire batch succeeds and expected number of lines parsed`` () =
    let input =
        """
# HELP http_requests_total The total number of HTTP requests.
# TYPE http_requests_total counter
http_requests_total{method="post",code="200"} 1027 1395066363000
http_requests_total{method="post",code="400"}    3 1395066363000

# Escaping in label values:
msdos_file_access_time_seconds{path="C:\\DIR\\FILE.TXT",error="Cannot find file:\n\"FILE.TXT\""} 1.458255915e9

# Minimalistic line:
metric_without_timestamp_and_labels 12.47

# A weird metric from before the epoch:
something_weird{problem="division by zero"} +Inf -3982045

# A histogram, which has a pretty complex representation in the text format:
# HELP http_request_duration_seconds A histogram of the request duration.
# TYPE http_request_duration_seconds histogram
http_request_duration_seconds_bucket{le="0.05"} 24054
http_request_duration_seconds_bucket{le="0.1"} 33444
http_request_duration_seconds_bucket{le="0.2"} 100392
http_request_duration_seconds_bucket{le="0.5"} 129389
http_request_duration_seconds_bucket{le="1"} 133988
http_request_duration_seconds_bucket{le="+Inf"} 144320
http_request_duration_seconds_sum 53423
http_request_duration_seconds_count 144320

# Finally a summary, which has a complex representation, too:
# HELP rpc_duration_seconds A summary of the RPC duration in seconds.
# TYPE rpc_duration_seconds summary
rpc_duration_seconds{quantile="0.01"} 3102
rpc_duration_seconds{quantile="0.05"} 3272
rpc_duration_seconds{quantile="0.5"} 4773
rpc_duration_seconds{quantile="0.9"} 9001
rpc_duration_seconds{quantile="0.99"} 76656
rpc_duration_seconds_sum 1.7560473e+07
rpc_duration_seconds_count 2693
"""

    let lines = Prometheus.parseText input |> Array.toList

    let isOk (x: Result<Line, string>) =
        match x with
        | Ok _ -> true
        | Error _ -> false

    test <@ (lines |> List.forall isOk) @>

    let result = lines |> Result.list

    let countRemaining lines filter =
        match lines with
        | Error _ -> 0
        | Ok lst -> lst |> List.filter filter |> List.length

    let count = countRemaining result

    let blankLines =
        function
        | Blank _ -> true
        | _ -> false

    let commentLines =
        function
        | Comment _ -> true
        | _ -> false

    let helpLines =
        function
        | Help _ -> true
        | _ -> false

    let typeLines =
        function
        | Type _ -> true
        | _ -> false

    let metricLines =
        function
        | Metric _ -> true
        | _ -> false

    test <@ (count commentLines) = 5 @>
    test <@ (count helpLines) = 3 @>
    test <@ (count typeLines) = 3 @>
    test <@ (count metricLines) = 20 @>
    test <@ (count blankLines) = 7 @> // beginning and end have return carriage 5 + 2
