namespace Fennel

open System
open System.Text
//==============================
// TYPES
//==============================
type MetricName = MetricName of string
type DocString = DocString of string

type MetricValue =
    | FloatValue of float
    | Nan // TODO: 22/11/2020 dburriss@xebia.com | what about +Inf and -Inf ?

type MetricType =
    | Counter
    | Gauge
    | Histogram
    | Summary
    | Untyped

type Timestamp = Timestamp of DateTimeOffset
type Label = string * string

type MetricLine =
    { Name: MetricName
      Labels: Label list
      Value: MetricValue
      Timestamp: Timestamp option }

type Line =
    | Help of (MetricName * DocString)
    | Comment of string
    | Type of (MetricName * MetricType)
    | Metric of MetricLine
    | Blank

module MetricName =
    let asString (MetricName name) = name

module DocString =
    let asString (DocString doc) = doc

module MetricValue =
    let asString =
        function
        | FloatValue v -> v.ToString()
        | Nan -> "NaN"

    let asValue =
        function
        | FloatValue v -> Some v
        | Nan -> None

module MetricType =
    let asString =
        function
        | Counter -> "counter"
        | Gauge -> "gauge"
        | Histogram -> "histogram"
        | Summary -> "summary"
        | Untyped -> "untyped"

module Timestamp =
    let date (Timestamp dt) = dt

    let asString ts =
        (date ts).ToUnixTimeMilliseconds().ToString()

module Label =
    let empty: Label list = List.empty

    let asString (labels: Label list) =
        match labels with
        | [] -> ""
        | [ (pname, pvalue) ] -> sprintf "{%s=\"%s\"}" pname pvalue
        | (pname, pvalue) :: labels ->
            let sb = StringBuilder(sprintf "{%s=\"%s\"" pname pvalue)

            let appendLabel (pname, pvalue) =
                // TODO: 22/11/2020 dburriss@xebia.com | Escape pvalue?
                let s = sprintf ",%s=\"%s\"" pname pvalue
                sb.Append(s) |> ignore

            labels |> List.iter appendLabel
            sb.Append("}") |> ignore
            sb.ToString()


module Line =
    let help metricName description = Help(metricName, description)
    let comment text = Comment text

    let metric name value (labels: (string * string) list) (timestamp: Timestamp option) =
        Metric
            { Name = name
              Labels = labels
              Value = value
              Timestamp = timestamp }

    let blank = Blank

    let asString line =
        match line with
        | Help(name, txt) -> sprintf "# HELP %s %s" (MetricName.asString name) (DocString.asString txt)
        | Comment txt -> sprintf "# %s" txt
        | Type(name, t) -> sprintf "# TYPE %s %s" (MetricName.asString name) (MetricType.asString t)
        | Blank -> Environment.NewLine
        | Metric m -> // value is Nan?
            match m with
            | { Name = MetricName name
                Labels = []
                Value = v
                Timestamp = None } -> sprintf "%s %s" name (MetricValue.asString v)
            | { Name = MetricName name
                Labels = []
                Value = v
                Timestamp = Some ts } -> sprintf "%s %s %s" name (MetricValue.asString v) (Timestamp.asString ts)
            | { Name = MetricName name
                Labels = labels
                Value = v
                Timestamp = None } -> sprintf "%s%s %s" name (Label.asString labels) (MetricValue.asString v)
            | { Name = MetricName name
                Labels = labels
                Value = v
                Timestamp = Some ts } ->
                sprintf "%s%s %s %s" name (Label.asString labels) (MetricValue.asString v) (Timestamp.asString ts)
