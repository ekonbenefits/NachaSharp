namespace NachaSharp

open System
open FSharp.Data.FlatFileMeta
open FSharp.Data.FlatFileMeta.MetaDataHelper

[<AbstractClass>]
type NachaRecord(rowInput, recordTypeCode) =
    inherit FlatRecord(rowInput)
    member __.IdentifyingRecordTypeCode = recordTypeCode
    override this.IsIdentified() =
            this.RecordTypeCode = this.IdentifyingRecordTypeCode
    member this.RecordTypeCode
        with get () = this.GetColumn ()
        and set value = this.SetColumn<string> value
  
type EntryAddenda(rowInput) =
    inherit NachaRecord(rowInput, "X")
    override this.Setup () = 
        setup this <|
                lazy ({ 
                         columns =[
                                  ]
                         length = 94
                     })

type EntryDetail(rowInput) =
    inherit NachaRecord(rowInput, "X")
    member val Addenda: EntryAddenda option = Option.None with get,set
    override this.Setup () = 
        setup this <|
                lazy ({ 
                         columns =[
                                  ]
                         length = 94
                     })
    
type BatchControlRecord(rowInput) =
    inherit NachaRecord(rowInput, "X")
    override this.Setup () = 
        setup this <|
                lazy ({ 
                         columns =[
                                  ]
                         length = 94
                     })

        
type BatchHeaderRecord(rowInput) =
    inherit NachaRecord(rowInput, "X")
    override this.Setup () = 
        setup this <|
                lazy ({ 
                         columns =[
                                  ]
                         length = 94
                     })

    member val Children: EntryDetail list = List.empty with get,set
    member val Trailer: BatchControlRecord option = Option.None with get,set



    
type FileControlRecord(rowInput) =
    inherit NachaRecord(rowInput, "X")

    override this.Setup () = 
        setup this <|
                lazy ({ 
                         columns =[
                                  ]
                         length = 94
                     })
       
type FileHeaderRecord(rowInput) =
    inherit NachaRecord(rowInput, "1")
    
    member val Children: BatchHeaderRecord list = List.empty with get,set
    member val Trailer: FileControlRecord option = Option.None with get,set
            
    override this.Setup () = 
        setup this <|
                lazy ({ 
                         columns =[
                                    MetaColumn.Make(this.RecordTypeCode, 1, Format.constantString this.IdentifyingRecordTypeCode)
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