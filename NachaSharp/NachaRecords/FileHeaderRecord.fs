namespace NachaSharp

open System
open System.Collections.Generic
open FSharp.Data.FlatFileMeta

type FileHeaderRecord(rowInput) =
    inherit NachaRecord(rowInput, "1")
     
    static member Create() = createRow {
            let! fh = FileHeaderRecord
            fh.FileControl <- SomeRow <| FileControlRecord.Create()
            return fh
        }
    
    member this.Batches 
        with get () = this.GetChildList<BatchHeaderRecord>()
    member this.FileControl 
        with get () = this.GetChild<FileControlRecord>(lazy NoRow)
        and set value = this.SetChild<FileControlRecord>(value)
        
    override this.Calculate () =
         base.Calculate()
         maybeRow {
            let! fc = this.FileControl
            fc.BatchCount <- this.Batches.Count
         } |> ignore
        
    override this.Setup () = 
        FlatRowProvider.setup this <|
            lazy ({ 
                     columns =[
                                MetaColumn.Make( 1, this.RecordTypeCode,     Format.leftPadString)
                                MetaColumn.Make( 2, this.PriorityCode,       Format.zerodInt)
                                MetaColumn.Make(10, this.IntermediateDestination, Format.leftPadString)
                                MetaColumn.Make(10, this.IntermediateOrigin, Format.leftPadString)
                                MetaColumn.Make( 6, this.FileCreationDate,   Format.reqYYMMDD)
                                MetaColumn.Make( 4, this.FileCreationTime,   Format.optHHMM)
                                MetaColumn.Make( 1, this.FileIDModifier,     Format.rightPadString)
                                MetaColumn.Make( 3, this.RecordSize,         Format.zerodInt)
                                MetaColumn.Make( 2, this.BlockingFactor,     Format.zerodInt)
                                MetaColumn.Make( 1, this.FormatCode,         Format.leftPadString)
                                MetaColumn.Make(23, this.IntermediateDestinationName, Format.rightPadString)
                                MetaColumn.Make(23, this.IntermediateOriginName, Format.rightPadString)
                                MetaColumn.Make( 8, this.ReferenceCode,      Format.rightPadString)
                              ]
                     length = 94
                 })
        
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
        and set value = this.SetColumn<DateTime Nullable> value
        
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