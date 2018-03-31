namespace rec FSharp.Data.FlatFileMeta

open FSharp.Control
open System.Runtime.CompilerServices
open System.IO
open System.Collections.Generic
open FSharp.Interop.Compose.Linq

module FlatRowSetup =   
    [<Extension;Sealed;AbstractClass>] 
    type Cache<'T when 'T :> FlatRow> ()=
        static member val MetaData: ParsedMeta option = Option.None with get,set

    let syncParseLines (parser:string AsyncSeq -> #FlatRow MaybeRow Async) = 
            AsyncSeq.ofSeq >> parser >> Async.RunSynchronously
            
    let asyncParseFile (parser:string AsyncSeq -> #FlatRow MaybeRow Async) (stream:Stream) =
        let seq = asyncSeq{
                        use streamReader = new StreamReader(stream)
                        let mutable completed = false
                        while not (completed) do 
                            let! line = streamReader.ReadLineAsync() |> Async.AwaitTask
                            let found = line |> Option.ofObj
                            match found with
                                | Some(line) ->
                                    yield line
                                | None -> completed <- true
                  }
        async {
            return! seq |> parser
        }
        
    let syncParseFile parser stream = 
         asyncParseFile parser stream |> Async.RunSynchronously

    let matchRecord (constructor:string -> #FlatRow) lineNumber value  =
        let result = value |> constructor       
        if result.IsMatch() && not <| result.IsNew() then
            result.ParsedLineNumber <- Some(lineNumber)
            Some(result)
        else
            None
           
    let multiMatch (matchers:(int -> string -> #FlatRow option) list) lineNo value =
        matchers
            |> List.map (fun f -> f lineNo value)
            |> List.tryFind (fun x->x.IsSome)
            |> Option.flatten

    let setup<'T when 'T :> FlatRow>  (_:'T) (v: DefinedMeta Lazy) : ParsedMeta = 
        match Cache<'T>.MetaData with
            | Some(md) -> md
            | None ->
                let meta = v.Force()
                let sumLength = meta.columns |> List.sumBy (fun x->x.Length)
                if sumLength <> meta.length then
                    raise <| InvalidDataException(sprintf "Data columns sum to %i which is not the expected value %i" sumLength meta.length)
                try
                    let result = meta.length,
                                 meta.columns 
                                    |> Seq.filter (fun x->not x.PlaceHolder) 
                                    |> Seq.map (fun x->x.Key)
                                    |> Enumerable.toList
                                    :> IList<_>,
                                 meta.columns 
                                     |> Seq.scan (fun state i -> i.Length + state) 0
                                     |> Seq.zip meta.columns
                                     |> Seq.filter (fun (c, _) ->  not c.PlaceHolder)
                                     |> Seq.map (fun (c, i) -> c.Key, (i,c))
                                     |> Map.ofSeq
                                     :> IDictionary<_,_>
                    Cache<'T>.MetaData <- Some(result)
                    result      
                with
                    | ex -> raise <| InvalidDataException("Columns must have unique names", ex)
                    