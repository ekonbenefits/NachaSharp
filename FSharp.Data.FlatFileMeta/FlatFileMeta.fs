namespace rec FSharp.Data.FlatFileMeta

open System.Collections.Generic
open System.IO
open System
open System.ComponentModel
open System.Runtime.CompilerServices
open FSharp.Interop.Compose.Linq

[<AutoOpen>]
module CreateRowExtension =
    type CreateRowBuilder() =
        member __.Bind(m:string->#FlatRow, f) = 
                null |> m |> f
        member __.Return(x) = x
        
        member __.ReturnFrom(x:string->#FlatRow) = null |> x
    let createRow = new CreateRowBuilder()      

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


type ProcessedMeta = int * string IList * IDictionary<string, int * ColumnIdentifier>

type DefinedMeta = { columns: ColumnIdentifier list; length :int }


type internal ChildList<'T when 'T :> FlatRow>(parent:FlatRow) =
    inherit System.Collections.ObjectModel.Collection<'T>()
    override this.InsertItem(index, item) =
        item.Parent <- SomeRow(parent)
        base.InsertItem(index, item)
        this.NotifyChanged(item)
    override this.SetItem(index, item) =
        item.Parent <- SomeRow(parent)
        base.SetItem(index, item)
        this.NotifyChanged(item)
    member __.NotifyChanged(item) =
        if item.HelperGetAllowMutation () then
            maybeRow {
                let! parent = item.Parent
                parent.Changed()
            } |> ignore


[<RequireQualifiedAccess>]
module MaybeRow =

      [<CompiledName("Cast")>]  
      let cast (m:MaybeRow<FlatRow>) : MaybeRow<#FlatRow> =
           match m with
              | SomeRow(r) -> SomeRow(downcast r)
              | NoRow -> NoRow
       
      [<CompiledName("Cast")>]  
      let generalize (m:MaybeRow<#FlatRow>) : MaybeRow<FlatRow> =
         match m with
            | SomeRow(r) -> SomeRow(upcast r)
            | NoRow -> NoRow
              
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
          

                     
 type Hierarchy = 
       Child of (FlatRow MaybeRow)
       | Children of (System.Collections.IEnumerable)
       | Other of (obj)
    
 type RankedHierarchy = Ranked of int * Hierarchy
      
[<AbstractClass>]
type FlatRow(rowData:string) =
    let rowInput = Helper.optionOfStringEmpty rowData
    let mutable rawData: string array = Array.empty
    let mutable columnKeys: IList<string> = upcast List()
    let mutable columnMap: IDictionary<string, int * ColumnIdentifier> = upcast Map.empty 
    let mutable columnLength: int = 0
    
    let children = Dictionary<string,RankedHierarchy>()
    
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

    abstract Setup: unit -> ProcessedMeta
    
    abstract PostSetup: unit -> unit
    
    abstract Calculate: unit -> unit
    
    default this.Calculate () =
        if not <| this.HelperGetAllowMutation ()  then
            invalidOp "AllowMutation is not set on root"
            
        children.Keys
            |> Seq.map this.ChildData
            |> Seq.iter (function | Child(mf) -> maybeRow {
                                                                let! f = mf
                                                                f.Calculate()
                                                            } |> ignore
                                  | Children (l) -> 
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
        children 
            |> Seq.sortBy (fun kp -> match kp.Value with |Ranked(r,_)->r) 
            |> Seq.map (fun kp -> kp.Key)
        
    member __.ChildData(key:string):Hierarchy=
        children.[key] |> (function | Ranked(_,h) -> h)
     
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
     
    member private __.HelperGetChild<'T when 'T :> FlatRow> (rank:int) (key:string) : MaybeRow<'T> =
                match children.TryGetValue(key) with
                    | true,Ranked(_,Child(mv)) -> mv |> MaybeRow.cast
                    | true,_ -> invalidOp "Different type for this name already added"
                    | ______ -> let d = Child(NoRow)
                                children.[key] <- Ranked(rank, d)
                                NoRow
                                
    member private __.HelperGetOther (rank:int) (defaultValue:'T Lazy) (key:string) =
                    match children.TryGetValue(key) with
                        | true,Ranked(_,Other(v)) -> downcast v
                        | true,_ -> invalidOp "Different type for this name already added"
                        | ______ -> let d = defaultValue.Force()
                                    children.[key] <- Ranked(rank, Other(rank, box d))
                                    d
                                
    member private this.HelperGetChildren<'T when 'T :> FlatRow> (rank:int) (key:string) : 'T IList  =
              match children.TryGetValue(key) with
                  | true,Ranked(_,Children(v)) -> downcast v
                  | true,_ -> invalidOp "Different type for this name already added"
                  | ______ -> let d: 'T IList = upcast ChildList<'T>(this)
                              let h = Children(d)
                              children.[key] <- Ranked(rank,h)
                              d                      
                                
    member this.GetChild(rank:int, [<CallerMemberName>] ?memberName: string) : #FlatRow MaybeRow= 
            let key =  memberName
                       |> Option.defaultWith Helper.raiseMissingCompilerMemberName
            this.HelperGetChild rank key
            
    member this.GetChildList(rank:int,[<CallerMemberName>] ?memberName: string) : #FlatRow IList = 
                let key =  memberName
                           |> Option.defaultWith Helper.raiseMissingCompilerMemberName
                this.HelperGetChildren rank key   
                                   
    member this.SetChild(rank:int, value:#FlatRow MaybeRow, [<CallerMemberName>] ?memberName: string) : unit = 
                let key = 
                    memberName
                       |> Option.defaultWith Helper.raiseMissingCompilerMemberName
                children.[key] <- Ranked(rank,Child(value |> MaybeRow.generalize))
                if this.HelperGetAllowMutation () then
                    this.Changed()
         
    member this.SetOther(rank:int, value:obj, [<CallerMemberName>] ?memberName: string) : unit = 
               let key = 
                   memberName
                      |> Option.defaultWith Helper.raiseMissingCompilerMemberName
               children.[key] <- Ranked(rank,Other(value))
               if this.HelperGetAllowMutation () then
                   this.Changed()
            
    member this.GetColumn([<CallerMemberName>] ?memberName: string) : 'T =
        let start, columnIdent =
            match memberName with
                | Some(k) -> 
                        try 
                            this.ColumnMap.[k]
                        with exn -> raise <| KeyNotFoundException(sprintf "Schema missing %s in %A" k this, exn)
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
                 | Some(k) ->
                    try 
                        this.ColumnMap.[k]
                    with exn -> raise <| KeyNotFoundException(sprintf "Schema missing %s in %A" k this, exn)
                 | None -> Helper.raiseMissingCompilerMemberName()
        if not <| this.HelperGetAllowMutation ()  then
            invalidOp "AllowMutation is not set on root"
                 
        let columnDef:Column<'T> = downcast columnIdent
        let stringVal = value |> columnDef.SetValue columnIdent.Length
        let newSlice =stringVal.ToCharArray() |> Array.map string
        let endSlice = start - 1 + columnIdent.Length
        this.Row.[start..endSlice] <- newSlice
        this.Changed()
 
 
       
