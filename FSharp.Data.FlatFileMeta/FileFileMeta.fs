namespace rec FSharp.Data.FlatFileMeta

open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open FSharp.Control
open System.Runtime.CompilerServices
open System.Collections.Generic
open System.IO
open System
open FSharp.Interop.Compose.Linq


type MaybeRow<'T when 'T :> FlatRow> =
    SomeRow of 'T | NoRow
   
[<AutoOpen>]
module MaybeRowExtension =
    type MaybeRowBuilder() =
        member __.Bind(m, f) = 
            Option.bind f m

        member __.Bind(m, f) = 
            match m with
                | SomeRow(r) -> f r
                | NoRow -> None

        member __.Return(x) = Some x

        member __.ReturnFrom(x) = x

        member __.Yield(x) = Some x

        member __.YieldFrom(x) = x
        
        member this.Zero() = this.Return ()
   
        member __.Delay(f) = f

        member __.Run(f) = f()

        member this.While(guard, body) =
            if not (guard()) 
            then this.Zero() 
            else this.Bind( body(), fun () -> 
                this.While(guard, body))  

        member this.TryWith(body, handler) =
            try this.ReturnFrom(body())
            with e -> handler e

        member this.TryFinally(body, compensation) =
            try this.ReturnFrom(body())
            finally compensation() 

        member this.Using(disposable:#System.IDisposable, body) =
            let body' = fun () -> body disposable
            this.TryFinally(body', fun () -> 
                match disposable with 
                    | null -> () 
                    | disp -> disp.Dispose())

        member this.For(sequence:seq<_>, body) =
            this.Using(sequence.GetEnumerator(),fun enum -> 
                this.While(enum.MoveNext, 
                    this.Delay(fun () -> body enum.Current)))

    let maybeRow = new MaybeRowBuilder()       


type ColumnIdentifier(key: string, length:int, placeHolder:bool) =
    member __.Key = key
    member __.Length = length
    member __.PlaceHolder = placeHolder
    
type Column<'T>(key: string, length:int, getValue: string -> 'T, setValue: int -> 'T -> string) =
    inherit ColumnIdentifier(key, length, false)
    member __.GetValue = getValue
    member __.SetValue = setValue


type MetaColumn =
    static member Make<'T>(length, [<ReflectedDefinition>] value:Expr<'T> , (getValue: string -> 'T, setValue)) =
        
        let key = 
            match value with
            | PropertyGet(_, propOrValInfo, _) -> propOrValInfo.Name
            | ________________________________ -> invalidArg "value" "Must be a property get"
        Column(key, length, getValue, setValue)

    static member PlaceHolder(length) =
        ColumnIdentifier("", length, true)

type ParsedMeta = int * string IList * IDictionary<string, int * ColumnIdentifier>

type DefinedMeta = { columns: ColumnIdentifier list; length :int }


type internal ChildList<'T when 'T :> FlatRow>(parent:FlatRow) =
    inherit System.Collections.ObjectModel.Collection<'T>()
    override this.InsertItem(index, item) =
        item.Parent <- SomeRow(parent)
        base.InsertItem(index, item)
        if item.HelperGetAllowMutation () then
            item.Parent |> MaybeRow.toOption |> Option.iter (fun x -> x.Changed())
    override this.SetItem(index, item) =
        item.Parent <- SomeRow(parent)
        base.SetItem(index, item)
        if item.HelperGetAllowMutation () then
            item.Parent |> MaybeRow.toOption |> Option.iter (fun x -> x.Changed())


[<RequireQualifiedAccess>]
module MaybeRow =

      [<CompiledName("IsSomeRow")>]      
      let isSomeRecord =
           function 
                | SomeRow _ -> true
                | NoRow -> false

      [<CompiledName("IsNoRow")>]      
      let isNoRecord =
           function 
                | SomeRow _ -> false
                | NoRow -> true


      [<CompiledName("ToOption")>]        
      let toOption<'T when 'T :> FlatRow> (maybeRec: MaybeRow<'T>) : Option<'T> =
            match maybeRec with 
                | SomeRow x -> Some(x)
                | NoRow -> None
        
      [<CompiledName("OfOption")>]        
      let ofOption (opt:#FlatRow option) =
            match opt with  
                | Some x -> SomeRow(x)
                | None -> NoRow       
                       
[<AbstractClass>]
type FlatRow(rowData:string) =
    

    let rowInput = Helper.optionOfStringEmpty rowData
    let mutable rawData: string array = Array.empty
    let mutable columnKeys: IList<string> = upcast List()
    let mutable columnMap: IDictionary<string, int * ColumnIdentifier> = upcast Map.empty 
    let mutable columnLength: int = 0
    
    let children = Dictionary<string,obj>()
    
    member val Parent: FlatRow MaybeRow = NoRow with get,set
    
    member this.Root:FlatRow MaybeRow = 
            let rec findRoot (f:FlatRow MaybeRow) =
                match f with 
                    | NoRow -> f
                    | SomeRow(p) -> findRoot p.Parent
            findRoot this.Parent
    
    member val ParsedLineNumber: int option = None with get,set
    
    member val AllowMutation: bool = rowInput.IsNone with get,set
                     
    member __.IsNew() = rowInput.IsNone

    abstract Setup: unit -> ParsedMeta
    
    abstract PostSetup: unit -> unit
    
    abstract Calculate: unit -> unit
    
    default this.Calculate () =
        if not <| this.HelperGetAllowMutation ()  then
            invalidOp "AllowMutation is not set on root"
            
        children.Values
            |> Seq.iter (function | :? FlatRow as f -> f.Calculate()
                                  | :? System.Collections.IEnumerable as l -> 
                                        l |> Enumerable.ofType<FlatRow>
                                          |> Seq.iter (fun i->i.Calculate())
                                  | _ ->())
                                      
    member this.Changed() =
            this.Root |> function | SomeRow(r) -> r.Changed() 
                                  | NoRow -> this.Calculate()
    
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
            this.PostSetup()

       
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
        let endSlice = start - 1 + columnIdent.Length
        this.Row.[start..endSlice] |> String.concat ""             
            
    member this.MetaData(key:string) =
        let start, columnIdent = this.ColumnMap.[key]
        struct (start, columnIdent.Length)
          
    member internal this.HelperGetAllowMutation () =
        maybeRow { let! root = this.Root
                   return root.AllowMutation
                 } |> Option.defaultValue this.AllowMutation
     
    member private __.HelperGetChild (defaultValue:'T Lazy) (key:string) =
                match children.TryGetValue(key) with
                    | true,v -> downcast v
                    | ______ -> let d = defaultValue.Force()
                                children.Add(key, d)
                                d
    member this.GetChild(defaultValue:#FlatRow MaybeRow Lazy, [<CallerMemberName>] ?memberName: string) : #FlatRow MaybeRow= 
            let key =  memberName
                       |> Option.defaultWith Helper.raiseMissingCompilerMemberName
            this.HelperGetChild defaultValue key
            
    member this.GetChildList([<CallerMemberName>] ?memberName: string) : #FlatRow IList = 
                let key =  memberName
                           |> Option.defaultWith Helper.raiseMissingCompilerMemberName
                this.HelperGetChild(lazy upcast ChildList(this)) key   
                                   
    member this.SetChild(value:#FlatRow MaybeRow, [<CallerMemberName>] ?memberName: string) : unit = 
                let key = 
                    memberName
                       |> Option.defaultWith Helper.raiseMissingCompilerMemberName
                children.Add(key, value)
                if this.HelperGetAllowMutation () then
                    this.Changed()
            
            
    member this.GetColumn([<CallerMemberName>] ?memberName: string) : 'T =
        let start, columnIdent =
            match memberName with
                | Some(k) -> this.ColumnMap.[k]
                | None -> Helper.raiseMissingCompilerMemberName()
        let endSlice = start - 1 + columnIdent.Length 
        let slice = this.Row.[start..endSlice]
        let data = slice |> String.concat ""
        let columnDef:Column<'T> = downcast columnIdent
        try 
            data |> columnDef.GetValue 
        with
            exn -> 
                    match this.ParsedLineNumber with
                        | Some(ln) -> 
                            raise <| InvalidDataException(sprintf "Unexpected value '%s' for '%s' at record %i %s" 
                                                                data columnIdent.Key ln (this.GetType().Name), exn)
                        | None ->
                            raise <| InvalidDataException(sprintf "Unexpected value '%s' for '%s'" data columnIdent.Key , exn)
            
    member this.SetColumn<'T>(value:'T, [<CallerMemberName>] ?memberName: string) =
        let start, columnIdent =
            match memberName with
                 | Some(k) -> this.ColumnMap.[k]
                 | None -> Helper.raiseMissingCompilerMemberName()
        if not <| this.HelperGetAllowMutation ()  then
            invalidOp "AllowMutation is not set on root"
                 
        let columnDef:Column<'T> = downcast columnIdent
        let stringVal = value |> columnDef.SetValue columnIdent.Length
        let newSlice =stringVal.ToCharArray() |> Array.map string
        let endSlice = start - 1 + columnIdent.Length
        this.Row.[start..endSlice] <- newSlice
        this.Changed()
 
 
       

module MetaDataHelper =   
    [<Extension;Sealed;AbstractClass>] 
    type Cache<'T when 'T :> FlatRow> ()=
        static member val MetaData: ParsedMeta option = Option.None with get,set


    let createRecord<'T when 'T :> FlatRow> (constructor:string -> 'T) (init:'T -> unit) =
                            let record = null |> constructor
                            record |> init
                            record

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
            
    let isMissingRecordButHasString record data  =
        record |> MaybeRow.isNoRecord && data |> Option.isSome
           
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
                    