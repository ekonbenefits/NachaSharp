namespace NachaSharp

open FSharp.Data.FlatFileMeta
open System
open System.Runtime.InteropServices.ComTypes


module File =
    let Parse (lines: string seq) =
        ()
        
type FileHeaderRecord(rowInput) =
    inherit BaseFlatRecord(rowInput)
    
    let recordTypeCode = "1"
    
    override this.IsIdentified() =
            this.RecordTypeCode = recordTypeCode 
            
    override this.Setup () = 
                lazy ({ 
                         columns =[
                                    MetaColumn.Make(this.RecordTypeCode, 1, Format.costantString recordTypeCode)
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
                |> MetaDataHelper.setup this
        
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
        
    
