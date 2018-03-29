namespace NachaSharp

open System
open System.Collections.Generic
open FSharp.Data.FlatFileMeta
open FSharp.Data.FlatFileMeta.MetaDataHelper

[<AbstractClass>]
type NachaRecord(rowInput, recordTypeCode) =
    inherit FlatRecord(rowInput)
    override this.IsIdentified() =
            this.RecordTypeCode = recordTypeCode
    member this.RecordTypeCode
        with get () = this.GetColumn ()
        and set value = this.SetColumn<string> value

[<AbstractClass>]
type EntryAddenda(rowInput, recordTypeCode) =
    inherit NachaRecord(rowInput, recordTypeCode)

[<AbstractClass>]
type EntryDetail(rowInput,recordTypeCode ) =
    inherit NachaRecord(rowInput, recordTypeCode)
    member val Addenda: EntryAddenda IList = upcast List() with get,set
    
    member this.AddendaRecordedIndicator
        with get () = this.GetColumn<int> ()
        and set value = this.SetColumn<int> value

type EntryExample1(rowInput) =
    inherit EntryDetail(rowInput, "X")
    override this.Setup () = 
        setup this <|
                lazy ({ 
                         columns =[
                                  ]
                         length = 94
                     })
                     
type EntryExample2(rowInput) =
    inherit EntryDetail(rowInput, "X")
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

    member val Entries: EntryDetail IList = upcast List() with get,set
    member val BatchControl: BatchControlRecord MaybeRecord = NoRecord with get,set



    
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
    
    member val Batches: BatchHeaderRecord IList = upcast List() with get,set
    member val FileControl: FileControlRecord MaybeRecord = NoRecord with get,set
            
    override this.Setup () = 
        setup this <|
                lazy ({ 
                         columns =[
                                    MetaColumn.Make(this.RecordTypeCode, 1, Format.leftPadString)
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