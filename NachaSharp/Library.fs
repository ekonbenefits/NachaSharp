namespace NachaSharp

open System
open FSharp.Data.FlatFileMeta
open FSharp.Data.FlatFileMeta.MetaDataHelper



[<AbstractClass>]
type EntryAddenda(rowInput) =
    inherit FlatRecord(rowInput)

type EntryDetail(rowInput) =
    inherit FlatRecord(rowInput)
    member val Trailer: EntryAddenda option = Option.None with get,set
    
type BatchControlRecord(rowInput) =
    inherit FlatRecord(rowInput) 
        
type BatchHeaderRecord(rowInput) =
    inherit FlatRecord(rowInput)
    member val Children: EntryDetail list = List.empty with get,set
    member val Trailer: BatchControlRecord option = Option.None with get,set
    
type FileControlRecord(rowInput) =
    inherit FlatRecord(rowInput)
       
type FileHeaderRecord(rowInput) =
    inherit FlatRecord(rowInput)

    let recordTypeCode = "1"
    
    member val Children: BatchHeaderRecord list = List.empty with get,set
    member val Trailer: FileControlRecord option = Option.None with get,set
        
    override this.IsIdentified() =
            this.RecordTypeCode = recordTypeCode 
            
    override this.Setup () = 
        setup this <|
                lazy ({ 
                         columns =[
                                    MetaColumn.Make(this.RecordTypeCode, 1, Format.constantString recordTypeCode)
                                    MetaColumn.Make(this.PriorityCode, 2, Format.zerodInt)
                                    MetaColumn.Make(this.IntermediateDestination, 10, Format.leftPadString)
                                    MetaColumn.Make(this.IntermediateOrigin, 10, Format.leftPadString)
                                    MetaColumn.Make(this.FileCreationDate, 6, Format.reqYYMMDD)
                                    MetaColumn.Make(this.FileCreationTime, 4, Format.optHHMM)
                                    MetaColumn.Make(this.FileIDModifier, 1, Format.rightPadString)
                                    MetaColumn.Make(this.RecordSize, 3, Format.zerodInt)
                                    MetaColumn.Make(this.BlockingFactor, 2, Format.zerodInt)
                                    MetaColumn.Make(this.FormatCode, 1, Format.leftPadString)
                                    MetaColumn.Make(this.IntermediateDestinationName,23, Format.rightPadString)
                                    MetaColumn.Make(this.IntermediateOriginName,23, Format.rightPadString)
                                    MetaColumn.Make(this.ReferenceCode, 8, Format.rightPadString)
                                  ]
                         length = 94
                     })
        
    member this.RecordTypeCode
        with get () = this.GetColumn ()
        and set value = this.SetColumn<string> value
        
    member this.PriorityCode
        with get () = this.GetColumn()
        and set value = this.SetColumn<int> value
        
    member this.IntermediateDestination
        with get () = this.GetColumn()
        and set value = this.SetColumn<string> value
            
    member this.IntermediateOrigin
        with get () = this.GetColumn()
        and set value = this.SetColumn<string> value
              
    member this.FileCreationDate
        with get () = this.GetColumn()
        and set value = this.SetColumn<DateTime> value
             
    member this.FileCreationTime
        with get () = this.GetColumn()
        and set value = this.SetColumn<DateTime option> value
        
    member this.FileIDModifier
        with get () = this.GetColumn()
        and set value = this.SetColumn<string> value
        
    member this.RecordSize
        with get () = this.GetColumn()
        and set value = this.SetColumn<int> value
        
    member this.BlockingFactor
        with get () = this.GetColumn()
        and set value = this.SetColumn<int> value
        
    member this.FormatCode
        with get () = this.GetColumn()
        and set value = this.SetColumn<string> value
        
    member this.IntermediateDestinationName
        with get () = this.GetColumn()
        and set value = this.SetColumn<string> value
    
    member this.IntermediateOriginName
        with get () = this.GetColumn()
        and set value = this.SetColumn<string> value

    member this.ReferenceCode
        with get () = this.GetColumn()
        and set value = this.SetColumn<string> value
        
module File =

    let (|FileHeaderMatch|_|)=
        matchRecord(FileHeaderRecord) 
    let (|FileControlMatch|_|)=
        matchRecord(FileControlRecord) 
    let (|BatchHeaderMatch|_|) =
        matchRecord(BatchHeaderRecord) 
    let (|BatchControlMatch|_|) =
        matchRecord(BatchControlRecord) 
    let (|EntryDetailMatch|_|) =
        matchRecord(EntryDetail) 

    let Parse (lines: string seq) =
        let mutable head: FileHeaderRecord option = None
        let enumerator = lines.GetEnumerator();
        while head.IsNone && enumerator.MoveNext() do
            match enumerator.Current with
                | FileHeaderMatch (fh) -> 
                    head <- Some(fh)
                    while fh.Trailer.IsNone && enumerator.MoveNext() do
                        match enumerator.Current with
                            | FileControlMatch t ->
                                fh.Trailer <- Some(t)
                            | BatchHeaderMatch bh ->
                                fh.Children <- fh.Children @ [bh]
                                while bh.Trailer.IsNone && enumerator.MoveNext() do
                                    match enumerator.Current with
                                        | BatchControlMatch  bt -> bh.Trailer <- Some(bt)
                                        | EntryDetailMatch ed ->
                                            bh.Children <- bh.Children @ [ed]
                                        | _ -> ()
                            | _ -> ()
                | _ -> ()
        head
