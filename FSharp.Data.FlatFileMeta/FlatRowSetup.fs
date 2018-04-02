namespace rec FSharp.Data.FlatFileMeta

open FSharp.Control
open System.Runtime.CompilerServices
open System.IO
open System.Collections.Generic
open FSharp.Interop.Compose.Linq
open System
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns

[<RequireQualifiedAccess>]
module FlatRowProvider =   
    open System.Collections.Concurrent

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

    let internal cache = ConcurrentDictionary<Type,ProcessedMeta>()

    let setup (row:#FlatRow) (v: DefinedMeta Lazy) : ProcessedMeta =
        let k = row.GetType()
        if cache.ContainsKey(k) then
            cache.[k]
        else
                let meta = v.Force()
                let sumLength = meta.columns |> List.sumBy (fun x->x.Length)
                if sumLength <> meta.length then
                    raise <| InvalidDataException(sprintf "Data columns sum to %i which is not the expected value %i of %A" sumLength meta.length row)
                
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
                let _,keys,_ = result
                if keys |> Seq.distinct |> Seq.length <> keys.Count then
                    raise <| InvalidDataException(sprintf "duplicate column names defined in %A" row)
                cache.[k] <- result
                result      
            



[<AutoOpen>]
module setupExtensions =

    
    type SetupMetaBuilder(fr:FlatRow) = 
        member __.Yield(x) = {columns = [];length =0}
        
        member __.Delay(x) = lazy (x())
        
        member __.Run(x) =
            FlatRowProvider.setup fr x
                
        [<CustomOperation("checkLength")>] 
        member __.CheckLength (meta, x) = {meta with length = x }
    
        /// Defines width of data to ignore
        [<CustomOperation("placeholder")>] 
        member __.Placeholder (meta, length) =
           { meta with columns = meta.columns @ [ColumnIdentifier("", length, true)]}

        /// Defines width of property and how to format.
        [<CustomOperation("columns")>] 
        member __.Columns (meta : DefinedMeta, length, [<ReflectedDefinition>] value:Expr<'T> , (getValue: string -> 'T, setValue)) = 
           let key = 
                match value with
                | PropertyGet(_, propOrValInfo, _) -> propOrValInfo.Name
                | ________________________________ -> invalidArg "value" "Must be a property get"
            
           { meta with columns = meta.columns @ [Column(key, length, getValue, setValue)]}
           
    let setupMetaFor (fr) = SetupMetaBuilder(fr)