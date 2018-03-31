namespace NachaSharp

open System
open FSharp.Data.FlatFileMeta

type FileHeaderRecord(rowInput) =
    inherit NachaRecord(rowInput, "1")
     
    static member Create(
                         immediateDest:string,
                         immediateOrigin:string,
                         fileIDModifier:string,
                         ?immediateDestName: string,
                         ?immediateOriginName: string,
                         ?referenceCode:string
                         ) = 
        createRow {
            let! fh = FileHeaderRecord
            
            fh.ImmediateDestination <- immediateDest
            fh.ImmediateOrigin <- immediateOrigin
            fh.FileIDModifier <- fileIDModifier
            fh.ImmediateDestinationName <- defaultArg immediateDestName ""
            fh.ImmediateOriginName <- defaultArg immediateOriginName ""
            fh.ReferenceCode <- defaultArg referenceCode ""
            
            fh.FileControl <- SomeRow <| FileControlRecord.Create()
            return fh
        }
        
    override this.PostSetup() =
        base.PostSetup()
        if this.IsNew() then
           this.PriorityCode <- 1
           let now = DateTime.Now
           this.FileCreationDate <- now
           this.FileCreationTime <- Nullable(now)
           this.RecordSize <- 94
           this.BlockingFactor <- 10
           this.FormatCode <- "1"
           
           
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
                                MetaColumn.Make(10, this.ImmediateDestination, Format.leftPadString)
                                MetaColumn.Make(10, this.ImmediateOrigin, Format.leftPadString)
                                MetaColumn.Make( 6, this.FileCreationDate,   Format.reqYYMMDD)
                                MetaColumn.Make( 4, this.FileCreationTime,   Format.optHHMM)
                                MetaColumn.Make( 1, this.FileIDModifier,     Format.rightPadString)
                                MetaColumn.Make( 3, this.RecordSize,         Format.zerodInt)
                                MetaColumn.Make( 2, this.BlockingFactor,     Format.zerodInt)
                                MetaColumn.Make( 1, this.FormatCode,         Format.leftPadString)
                                MetaColumn.Make(23, this.ImmediateDestinationName, Format.rightPadString)
                                MetaColumn.Make(23, this.ImmediateOriginName, Format.rightPadString)
                                MetaColumn.Make( 8, this.ReferenceCode,      Format.rightPadString)
                              ]
                     length = 94
                 })
        
    member this.PriorityCode
        with get () = this.GetColumn()
        and set value = this.SetColumn<int> value
        
    member this.ImmediateDestination
        with get () = this.GetColumn()
        and set value = this.SetColumn<string> value
            
    member this.ImmediateOrigin
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
        
    member this.ImmediateDestinationName
        with get () = this.GetColumn()
        and set value = this.SetColumn<string> value
    
    member this.ImmediateOriginName
        with get () = this.GetColumn()
        and set value = this.SetColumn<string> value

    member this.ReferenceCode
        with get () = this.GetColumn()
        and set value = this.SetColumn<string> value