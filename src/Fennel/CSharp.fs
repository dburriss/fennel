namespace Fennel.CSharp

open System
open Fennel

module P = Prometheus

type ILine =
    abstract member IsHelp : bool
    abstract member IsComment : bool
    abstract member IsMetricType : bool
    abstract member IsMetric : bool
    abstract member IsBlank : bool

type Help(metricName : MetricName, docString : DocString) =
    member this.MetricName = MetricName.asString metricName
    member this.Text = DocString.asString docString
    interface ILine with
        member this.IsHelp = true
        member this.IsComment = false
        member this.IsMetricType = false
        member this.IsMetric = false
        member this.IsBlank = false
        
type Comment(text : string) =   
    member this.Text = text
    interface ILine with
        member this.IsHelp = false
        member this.IsComment = true
        member this.IsMetricType = false
        member this.IsMetric = false
        member this.IsBlank = false
        
type MetricTypeEnum = | Counter = 1 | Gauge = 2 | Histogram = 3 | Summary = 4 | Untyped = 0
          
type MetricType(metricName : MetricName, metricType : Fennel.MetricType) =
    member this.MetricName = MetricName.asString metricName
    member this.MetricType = metricType |> function
        | Counter -> MetricTypeEnum.Counter
        | Gauge -> MetricTypeEnum.Gauge
        | Histogram -> MetricTypeEnum.Histogram
        | Summary -> MetricTypeEnum.Summary
        | Untyped -> MetricTypeEnum.Untyped
    interface ILine with
        member this.IsHelp = false
        member this.IsComment = false
        member this.IsMetricType = true
        member this.IsMetric = false
        member this.IsBlank = false
        
        
type Blank() =   
    interface ILine with
        member this.IsHelp = false
        member this.IsComment = false
        member this.IsMetricType = false
        member this.IsMetric = false
        member this.IsBlank = true
        
type Metric(metricName : MetricName, value : MetricValue, timestamp : Timestamp option, ?labels : Label list) =
    member this.MetricName = MetricName.asString metricName
    member this.MetricValue = 
        match (MetricValue.asValue value) with
        | Some x -> Nullable<float>(x)
        | None -> Nullable<float>()
    member this.Labels = dict (defaultArg labels [])
    member this.Timestamp = 
        match timestamp with
        | Some (Timestamp x) -> Nullable<DateTimeOffset>(x)
        | None -> Nullable<DateTimeOffset>()
           
    interface ILine with
        member this.IsHelp = false
        member this.IsComment = false
        member this.IsMetricType = false
        member this.IsMetric = true
        member this.IsBlank = false
        
type Prometheus =
    static member private ToILine(line) =
        match line with
        | Help (name, doc) -> Help(name, doc) :> ILine
        | Comment txt -> Comment txt :> ILine
        | Type (name, t) -> MetricType(name, t) :> ILine
        | Metric m -> Metric(m.Name, m.Value, m.Timestamp, m.Labels) :> ILine
        | Blank -> Blank() :> ILine
        
    static member ParseLine(line) =
        match P.parseLine line with
        | Ok x -> Prometheus.ToILine(x)
        | Error err -> failwith err
        
    static member ParseText(text) =
        let rs = P.parseText text
        
        match (rs |> Result.seq) with
        | Ok x -> x
        | Error err -> failwith err