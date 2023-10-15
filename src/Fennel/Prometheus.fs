namespace Fennel


module Prometheus =
    open FParsec
    open PLang

    let private lineParser =
        let line = ws0 >>. (comment <|> metric)

        ws0 >>. (line <|> emptyLine)

    /// Parse a single Prometheus line
    let parseLine line =
        let r = run lineParser line

        match r with
        | Success(x, _, _) -> x |> Result.Ok
        | Failure(errorMsg, _, _) -> errorMsg |> Result.Error

    let private split (sep: char) (s: string) = s.Split(sep)

    /// Parse a block of Prometheus metric logs into Lines
    let parseText (text: string) =
        text |> split '\n' |> Array.map parseLine

    /// Returns a Prometheus comment line
    let comment (text: string) = sprintf "# %s" text

    /// Returns a Prometheus TYPE hint line
    let typeHint metricName metricType =
        sprintf "# TYPE %s %s" metricName (MetricType.asString metricType)

    /// Returns a Prometheus HELP line
    let help metricName text = sprintf "# HELP %s %s" metricName text

    /// Returns a Prometheus metric line with just a metric name and value
    let metricSimple metricName value =
        let m =
            Metric
                { Name = MetricName metricName
                  Labels = []
                  Value = FloatValue value
                  Timestamp = None }

        Line.asString m

    /// Returns a Prometheus metric line
    let metric metricName value (labels: (string * string) list) dateTime =
        let m =
            Metric
                { Name = MetricName metricName
                  Labels = labels
                  Value = FloatValue value
                  Timestamp = Some(dateTime |> Timestamp) }

        Line.asString m

    /// Returns a Prometheus metric line
    let metricSansTime metricName value (labels: (string * string) list) =
        let m =
            Metric
                { Name = MetricName metricName
                  Labels = labels
                  Value = FloatValue value
                  Timestamp = None }

        Line.asString m
