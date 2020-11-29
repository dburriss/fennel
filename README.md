# Fennel

*Fennel* - **A flowering plant in the carrot family**

Fennel is a simple library that speaks Promethean. If can translate prometheus log strings to typed metrics, as well as create metrics in well formatted Prometheus lines.

## Why Fennel?

> But the noble son of Iapetus outwitted him and stole the far-seen gleam of unwearying fire in a hollow fennel stalk - Hesiod in the Theogony

The myth has it that Prometheus stole fire from Zeus by smuggling an ember in a hollow fennel stalk.

If you are wondering more about why I created this than about the name, it is simple. I was looking for something that just allowed me to create Prometheus log lines as strings. 
I also wanted to be able to parse Prometheus logs. Circa 2019 when I looked for this all I found were libraries that tied very deeply into ASP.NET Core.
I had been looking for an excuse to play with a parsing library like FParsec... and here we are.

## Nuget

Available on [Nuget.org](https://www.nuget.org/packages/Fennel/)

### Nuget Package Manger

`Install-Package Fennel`

### .NET CLI

`dotnet add package Fennel`

### PackageReference

`<PackageReference Include="Fennel" />`

### Paket

`paket add Fennel`

## Usage

### Translating text to Types

Assuming we have the following Prometheus logs as `input`:

```text
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
```

You can break the result down into individually parsed lines.

#### F#

```f#
open Fennel

let lines = Prometheus.parseText input
```

Each line has multiple possibilities:

```f#
match line with
| Help (name, doc) -> printfn "Help line %A" (name, doc)
| Comment txt -> printfn "Comment line %s" txt
| Type (name, t) -> printfn "Type line %A" (name, t)
| Metric m -> printfn "Metric line %A" m
| Blank -> printfn "Blank line"
```

#### C#

```c#
using Fennel.CSharp;

var lines = Prometheus.ParseText(input);
```

Or you can parse an individual line:

```c#
var input = "http_requests_total{method=\"post\",code=\"200\"} 1027 1395066363000";
var result = Prometheus.ParseLine(input);
if(result.IsMetric)
{
    var metric = result as Metric;
}
```

### Metric to string

You can also create correctly formatted Prometheus strings for `comment`, `help`, and `metric` lines.

#### F#

```f#
open Fennel

let prometheusString = Prometheus.metric "http_requests_total" 1027. [("method","post");("code","200")] DateTimeOffset.UtcNow
```

#### C#

```c#
using Fennel.CSharp;

var labels = new Dictionary<string, string>{ {"method", "post"}, {"code", "200"} };
var prometheusString = Prometheus.Metric("http_requests_total", 1027, labels, DateTimeOffset.UtcNow);
```