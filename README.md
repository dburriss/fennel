# Fennel

*Fennel* - **A flowering plant in the carrot family**

Fennel is a simple library that speaks Promethean. If can translate prometheus log strings to typed metrics, as well as create metrics in well formatted Prometheus lines.

## Why Fennel?

> But the noble son of Iapetus outwitted him and stole the far-seen gleam of unwearying fire in a hollow fennel stalk - Hesiod in the Theogony

The myth has it that Prometheus stole fire from Zeus by smuggling an ember in a hollow fennel stalk.

## Nuget

todo

## Usage

### Translating text to Types

Assuming we have the following Prometheus logs:

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

```f#
let lines = Prometheus.parseText input
```

### Metric to string

```f#
let metricLine = Prometheus.metric "http_requests_total" 1027. [("method","post");("code","200")] DateTimeOffset.UtcNow
```