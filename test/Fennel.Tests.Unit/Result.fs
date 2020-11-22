module Result

let mapError = Result.mapError
let bind = Result.bind

// Like `map` but with a unit-returning function
let iter (f : _ -> unit) result =
    Result.map f result |> ignore

/// Apply a Result<fn> to a Result<x> monadically
let apply fR xR =
    match fR, xR with
    | Ok f, Ok x -> Ok (f x)
    | Error err1, Ok _ -> Error err1
    | Ok _, Error err2 -> Error err2
    | Error err1, Error _ -> Error err1
    
let sequence aListOfResults =
    let (<*>) = apply // monadic
    let (<!>) = Result.map
    let cons head tail = head::tail
    let consR headR tailR = cons <!> headR <*> tailR
    let initialValue = Ok [] // empty list inside Result

    // loop through the list, prepending each element
    // to the initial value
    List.foldBack consR aListOfResults initialValue