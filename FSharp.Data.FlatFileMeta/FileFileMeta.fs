namespace FSharp.Data.FlatFileMeta

open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open FSharp.Control
open System.Runtime.CompilerServices
open System.Collections.Generic
open System.IO
open System
open FSharp.Interop.Compose.Linq


module internal Helper =
    let inline optionOfString str =
                    if str |> String.IsNullOrEmpty then 
                        None
                    else
                        Some(str)


type ColumnIdentifier(key: string, length:int) =
    member __.Key = key
    member __.Length = length
    
type Column<'T>(key: string, length:int, getValue: string -> 'T, setValue: int -> 'T -> string) =
    inherit ColumnIdentifier(key, length)
    member __.GetValue = getValue
    member __.SetValue = setValue


type MetaColumn =
    static member Make<'T>(length, [<ReflectedDefinition>] value:Expr<'T> , (getValue: string -> 'T, setValue)) =
        
        let key = 
            match value with
            | PropertyGet(_, propOrValInfo, _) -> propOrValInfo.Name
            | ________________________________ -> invalidArg "value" "Must be a property get"
        Column(key, length, getValue, setValue)

type ParsedMeta = int * string IList * IDictionary<string, int * ColumnIdentifier>

type DefinedMeta = { columns: ColumnIdentifier list; length :int }


[<AbstractClass>]
type FlatRecord(rowData:string) =
    let rowInput = Helper.optionOfString rowData
    let mutable rawData: string array = Array.empty
    let mutable columnKeys: IList<string> = upcast List()
    let mutable columnMap: IDictionary<string, int * ColumnIdentifier> = upcast Map.empty 
    let mutable columnLength: int = 0
    
    let children = Dictionary<string,obj>()
    
    static member Create<'T when 'T :> FlatRecord>
                    (constructor:string option -> 'T,
                     init: 'T -> unit) =
                     let result = None |> constructor
                     result |> init
                     result

    member __.IsNew() = rowInput.IsNone

    abstract Setup: unit -> ParsedMeta
    
    member this.IsMatch() = this.DoesLengthMatch() && this.IsIdentified ()
    
    abstract IsIdentified: unit -> bool
    
    member this.DoesLengthMatch () = this.Row |> Array.length = columnLength

    member private this.LazySetup() =
        if columnMap.Count = 0 then
            let totalLength, orderedKeys, mapMeta = this.Setup()
      
            columnLength <- totalLength
            columnKeys <- orderedKeys
            rawData <- match rowInput with
                        | Some (row) -> row |> Array.ofSeq |> Array.map string
                        | None -> Array.init totalLength (fun _ -> " ")
            columnMap <- mapMeta
    

       
    member __.ChildKeys() =
        children.Keys
        
    member __.ChildData(key:string):obj=
        children.[key]
                
                
    member this.Keys() =
        this.LazySetup()
        columnKeys   
    member private this.Row =
        this.LazySetup()
        rawData
    
    member private this.ColumnMap =
        this.LazySetup()
        columnMap
    
    member this.Data(key:string):obj=
        this.GetColumn(key) |> box
        
    member this.ToRawString() =
        this.Row |> String.concat ""
    
    member this.RawData(key:string)=
        let start, columnIdent = this.ColumnMap.[key]
        this.Row.[start..columnIdent.Length] |> String.concat ""             
            
    member this.MetaData(key:string) =
        let start, columnIdent = this.ColumnMap.[key]
        struct (start, columnIdent.Length)
            
    
    member __.GetChild<'T>(defaultValue: 'T Lazy, [<CallerMemberName>] ?memberName: string) : 'T = 
            let key = 
                memberName
                   |> Option.defaultWith (invalidArg "memberName" "Compiler should automatically fill this value")
            match children.TryGetValue(key) with
                | true,v -> downcast v
                | ______ -> let d = defaultValue.Force()
                            children.Add(key, d)
                            d
            
    member __.SetChild<'T>(value:'T, [<CallerMemberName>] ?memberName: string) : unit = 
                let key = 
                    memberName
                       |> Option.defaultWith (invalidArg "memberName" "Compiler should automatically fill this value")
                children.Add(key, value)
            
            
    member this.GetColumn([<CallerMemberName>] ?memberName: string) : 'T =
        let start, columnIdent =
            match memberName with
                | Some(k) -> this.ColumnMap.[k]
                | None -> invalidArg "memberName" "Compiler should automatically fill this value"
        let data = this.Row.[start..columnIdent.Length] |> String.concat ""
        let columnDef:Column<'T> = downcast columnIdent
        data |> columnDef.GetValue 
            
    member this.SetColumn<'T>(value:'T, [<CallerMemberName>] ?memberName: string) =
        let start, columnIdent =
            match memberName with
                 | Some(k) -> this.ColumnMap.[k]
                 | None -> invalidArg "memberName" "Compiler should automatically fill this value"
        let columnDef:Column<'T> = downcast columnIdent
        let stringVal = value |> columnDef.SetValue columnIdent.Length
        this.Row.[start..columnIdent.Length] <- stringVal.ToCharArray() |> Array.map string
 
 
type MaybeRecord<'T when 'T :> FlatRecord> =
    SomeRecord of 'T | NoRecord
    
module MaybeRecord =

      [<CompiledName("IsSomeRecord")>]      
      let isSomeRecord =
           function 
                | SomeRecord _ -> true
                | NoRecord -> false

      [<CompiledName("IsNoRecord")>]      
      let isNoRecord =
           function 
                | SomeRecord _ -> false
                | NoRecord -> true

      [<CompiledName("ToOption")>]        
      let toOption =
            function 
                | SomeRecord x -> Some(x)
                | NoRecord -> None
        
      [<CompiledName("OfOption")>]        
      let ofOption =
            function 
                | Some x -> SomeRecord(x)
                | None -> NoRecord            
    
              
module MetaDataHelper =   
    [<Extension;Sealed;AbstractClass>] 
    type Cache<'T when 'T :> FlatRecord> ()=
        static member val MetaData: ParsedMeta option = Option.None with get,set

    let syncParseLines (parser:string AsyncSeq -> #FlatRecord MaybeRecord Async) = 
            AsyncSeq.ofSeq >> parser >> Async.RunSynchronously
            
    let asyncParseFile (parser:string AsyncSeq -> #FlatRecord MaybeRecord Async) (stream:Stream) =
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

    let matchRecord(constructor:string -> #FlatRecord) value  =
        let result = value |> constructor       
        if result.IsMatch() && not <| result.IsNew() then
            Some(result)
        else
            None
            
    let isMissingRecordButHasString record data  =
        record |> MaybeRecord.isNoRecord && data |> Option.isSome
           
    let multiMatch (matchers:(string -> #FlatRecord option) list) value =
        matchers
            |> List.map (fun f -> f value)
            |> List.tryFind (fun x->x.IsSome)
            |> Option.flatten

    let setup<'T when 'T :> FlatRecord>  (_:'T) (v: DefinedMeta Lazy) : ParsedMeta = 
        match Cache<'T>.MetaData with
            | Some(md) -> md
            | None ->
                let meta = v.Force()
                let sumLength = meta.columns |> List.sumBy (fun x->x.Length)
                if sumLength <> meta.length then
                    raise <| InvalidDataException(sprintf "Data columns sum to %i which is not the expected value %i" sumLength meta.length)
                try
                    let result = meta.length,
                                 meta.columns |> Seq.map (fun x->x.Key) |> Enumerable.toList :> IList<_>,
                                 meta.columns 
                                     |> Seq.scan (fun state i -> i.Length + state) 0
                                     |> Seq.zip meta.columns
                                     |> Seq.map (fun (c, i) -> c.Key, (i,c))
                                     |> Map.ofSeq
                                     :> IDictionary<_,_>
                    Cache<'T>.MetaData <- Some(result)
                    result      
                with
                    | ex -> raise <| InvalidDataException("Columns must have unique names", ex)
                    