namespace Fennel
open System

  
/// Defines the parsers for the Prometheus grammar     
module PLang =
    open FParsec
    // https://www.quanttec.com/fparsec/tutorial.html#fs-value-restriction
    type UserState = unit
    type Parser<'t> = Parser<'t, UserState>
    
    //==============================
    // General helper parsers
    //==============================
    let ws0 : Parser<_> = spaces
    let private ws1 : Parser<_> = skipMany1 spaces
    let private stringLiteral : Parser<_> =
        let normalChar = satisfy (fun c -> c <> '\\' && c <> '"')
        let unescape c = match c with
                         | 'n' -> '\n'
                         | 'r' -> '\r'
                         | 't' -> '\t'
                         | c   -> c
        let escapedChar = pstring "\\" >>. (anyOf "\\nrt\"" |>> unescape)
        (between (pstring "\"") (pstring "\"")
                (manyChars (normalChar <|> escapedChar))) <?> "String literal"
        
    let ascii_alpha_numeric = (asciiLetter <|> digit)
    let private rest_of_line =
        let skipNewLine = false
        ws0>>.restOfLine skipNewLine
    let text : Parser<_> = many1Chars (satisfy (isAnyOf ". "))
    let private betweenQuotes p = between (pchar '"') (pchar '"') p
    let private word = many1Satisfy (fun c -> (c = ' ' || c = '\n') |> not)
    //==============================
    // Prometheus helper parsers
    //==============================
    // Common parsers
    let private underscoreOrColon = satisfy (fun c -> c = '_' || c = ':')
    let private pname = manyChars2 (asciiLetter <|> underscoreOrColon) (ascii_alpha_numeric <|> underscoreOrColon)
    // Comments parsers
    let private ``#`` = skipChar '#'
    let private comment_prefix = ws0 >>.``#`` //not sure if must be a space after #
    let private ``TYPE`` = skipString "TYPE".>> ws0
    let private ``HELP`` = skipString "HELP".>> ws0
    let private doc_string = rest_of_line |>> DocString
    
    // Metrics parsers
    
    let private metric_name = pname.>> ws0 |>> MetricName
    let private metric_type =
        word
        |>> function
            | "counter" -> Counter
            | "gauge" -> Gauge
            | "histogram" -> Histogram
            | "summary" -> Summary
            | "" -> Untyped
    
    
    let private metric_value =
        (pfloat |>> FloatValue)
        <|> (skipString "Nan">>. preturn Nan)
    let private metric_timestamp =
        let withTS = pint64 |>> (fun x -> x |> DateTimeOffset.FromUnixTimeMilliseconds |> Timestamp |> Some)
        let noTS = preturn None
        (withTS <|> noTS) <?> "Timestamp"
    let private label_value = stringLiteral//(betweenQuotes text) <?> "Label value" //TODO: may need better parser for value
    let private a_label_pair =
        ((ws0 >>.pname.>> ws0).>>(skipChar '=').>>.(ws0 >>.label_value)) |>> Label <?> "Label name/value pair" 
    let private label_pair_list = (sepBy a_label_pair (pchar ',')) <?> "Label pairs"
    let private metric_labels =
        let withLabels = (between (pchar '{') (pchar '}') label_pair_list)
        let noLabels = (preturn Label.empty) 
        (withLabels <|> noLabels) <?> "Labels"
    
    //==============================
    // Main parsers for each line 
    //==============================
    // Helper functions
    let private mapToMetric n l v t =
          { Name = n
            Labels = l
            Value = v
            Timestamp = t
          } |> Line.Metric
    // Conversion Parsers
    let private typeLine = (``TYPE``>>.metric_name.>>.metric_type) |>> Line.Type
    let private helpLine = (``HELP``>>.metric_name.>>.doc_string) |>> Line.Help
    let private commentLine = rest_of_line |>> Line.Comment
    
    /// Parser for a blank line
    let emptyLine = ws0 >>.preturn Line.Blank
    
    /// Parser for a comment: TYPE, HELP, or plain comment
    let comment = comment_prefix >>.ws0 >>.(typeLine <|> helpLine <|> commentLine)
    /// Parser for a metric line
    let metric =
        let nameParser = (metric_name.>> ws0)
        let labelsParser = metric_labels
        let valueParser = (ws0>>.metric_value)
        let timestampParser = (ws0>>.metric_timestamp)
        pipe4 nameParser labelsParser valueParser timestampParser mapToMetric
    