[<AutoOpen>]
module TypeExtensions

open System
open System.Collections.Generic
open System.Text.RegularExpressions

let tee f x =
    f x
    x

let nullableToOption (n : System.Nullable<_>) =
    if n.HasValue
    then Some n.Value
    else None

let (|Regex|_|) pattern options input =
   let m = Regex.Match(input, pattern, options)
   if m.Success then Some(List.tail [ for g in m.Groups -> g.Value ])
   else None

module String =
   let caseInsensitiveContains (haystack : string) (needle : string) =
       not (String.IsNullOrEmpty(haystack))
        && haystack.IndexOf(needle, StringComparison.InvariantCultureIgnoreCase) >= 0

   let caseInsensitiveEquals (a : string) (b : string) =
       String.Equals(a, b, StringComparison.InvariantCultureIgnoreCase)

module Result =
    let retn x = Ok x
    let map = Result.map
    let mapError = Result.mapError
    let bind = Result.bind

    /// Predicate that returns true on success
    let isOk =
        function
        | Ok _ -> true
        | Error _ -> false

    /// Predicate that returns true on failure
    let isError xR =
        xR |> isOk |> not

    let either fOk fError = function
        | Ok x -> fOk x
        | Error err -> fError err

    let sideEffect (onOk:_ -> unit) (onError:_ -> unit) result =
        match result with
        | Ok v as s -> onOk v; s
        | Error err as e -> onError err; e

    let get = function
        | Ok x -> x
        | Error e -> failwithf "Result Error: [%A]" (e.ToString())

    let apply f xResult =
        match f, xResult with
        | Ok f, Ok x -> Ok (f x)
        | Error errs, _ -> Error errs
        | _, Error errs -> Error errs

    /// Convert an Option into a Result. If none, use the passed-in errorValue
    let ofOption errorValue opt =
        match opt with
        | Some v -> Ok v
        | None -> Error errorValue

    let ofChoice = function
        | Choice1Of2 x -> Ok x
        | Choice2Of2 err -> Error err

    let combine = function
        | Ok x -> x
        | Error x -> x

    /// Lift a two parameter function to use Result parameters
    let lift2 f x1 x2 =
        let (<!>) = map
        let (<*>) = apply
        f <!> x1 <*> x2

    /// Apply a monadic function with two parameters
    let bind2 f x1 x2 = lift2 f x1 x2 |> bind id

    /// Convert a Result into an Option
    let toOption xR =
        match xR with
        | Ok v -> Some v
        | Error _ -> None

    /// combine a list of results, monadically
    let sequence aListOfResults =
        let (<*>) = apply // monadic
        let (<!>) = map
        let cons head tail = head::tail
        let consR headR tailR = cons <!> headR <*> tailR
        let initialValue = Ok [] // empty list inside Result

        // loop through the list, prepending each element
        // to the initial value
        List.foldBack consR aListOfResults initialValue

    type ResultsBuilder () =
        member __.Bind(a, f) = Result.bind f a
        member __.Return(a) = Ok a
        member __.ReturnFrom(a) = a
        member __.Zero() = __.Return ()

let result = Result.ResultsBuilder()

let (>>=) r f = Result.bind f r
let (|>>) r m = Result.map m r

module List =
    let removeLastItem xs =
        // removing from the end of a cons chain is a bit of pain
        (List.rev >> List.tail >> List.rev) xs

module Async =
    let retn x = async.Return x

    let bind f xAsync = async.Bind (xAsync, f)

    let map f xAsync = async {
        let! x = xAsync
        return f x }

    let apply fA xAsync = async {
        let! fChild = Async.StartChild fA // run in parallel
        let! x = xAsync
        // wait for the result of the first one
        let! f = fChild
        return f x
    }

    let catch (xAsync : Async<'a>) =
        xAsync
        |> Async.Catch
        |> map Result.ofChoice

module Seq =
    let toDictionary xs =
        xs |> dict |> (fun d -> System.Collections.Generic.Dictionary(d))

module Dictionary =
    let tryGetValue (d : IDictionary<_,'v>) key : 'v option =
        match d.TryGetValue key with
            | true, value -> Some value
            | _ -> None

    let tryGetValueCaseInsensitive (d : IDictionary<string,'v>) key : 'v option =
        d
        |> Seq.tryFind(fun x -> String.caseInsensitiveEquals x.Key key)
        |> Option.map (fun x -> x.Value)
        
module Environment =
    let tryGetEnvironmentVariable name =
        match Environment.GetEnvironmentVariable name with
        | x when String.IsNullOrEmpty x -> Error ("cannot find environment variable " + x)
        | x -> Ok x