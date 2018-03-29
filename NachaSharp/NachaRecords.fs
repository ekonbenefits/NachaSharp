namespace NachaSharp

open System
open System.Collections.Generic
open FSharp.Data.FlatFileMeta
open FSharp.Data.FlatFileMeta.MetaDataHelper

[<AbstractClass>]
type NachaRecord(rowInput, recordTypeCode) =
    inherit FlatRecord(rowInput)
    override this.IsIdentified() =
        let blockFiller = "9"
        let charBlockFiller = blockFiller |> Seq.head
        this.RecordTypeCode = recordTypeCode
            && (this.RecordTypeCode <> blockFiller
                || this.ToRawString() |> Seq.exists (fun x -> x <> charBlockFiller))
                
    member this.RecordTypeCode
        with get () = this.GetColumn ()
        and set value = this.SetColumn<string> value

[<AbstractClass>]
type EntryAddenda(rowInput, recordTypeCode) =
    inherit NachaRecord(rowInput, recordTypeCode)

[<AbstractClass>]
type EntryDetail(entrySEC, batchSEC, rowInput) =
    inherit NachaRecord(rowInput, "6")
    override __.IsIdentified() =
        base.IsIdentified() && batchSEC = entrySEC
    
    member this.Addenda 
        with get () = this.GetChild<EntryAddenda IList>(lazy upcast List())
    
    member this.AddendaRecordedIndicator
        with get () = this.GetColumn<int> ()
        and set value = this.SetColumn<int> value

type EntryWildCard(batchSEC, rowInput) =
    inherit EntryDetail(batchSEC, batchSEC, rowInput)
    override this.Setup () = 
        setup this <|
                lazy ({ 
                         columns =[
                                  ]
                         length = 94
                     })

type EntryCCD(batchSEC, rowInput) =
    inherit EntryDetail("CCD", batchSEC, rowInput)
    override this.Setup () = 
        setup this <|
                lazy ({ 
                         columns =[
                                  ]
                         length = 94
                     })
                     
type EntryPPD(batchSEC, rowInput) =
    inherit EntryDetail("PPD", batchSEC, rowInput)
    override this.Setup () = 
        setup this <|
                lazy ({ 
                         columns =[
                                  ]
                         length = 94
                     })
    
type BatchControlRecord(rowInput) =
    inherit NachaRecord(rowInput, "8")
    override this.Setup () = 
        setup this <|
                lazy ({ 
                         columns =[
                                  ]
                         length = 94
                     })

        
type BatchHeaderRecord(rowInput) =
    inherit NachaRecord(rowInput, "5")
    override this.Setup () = 
        setup this <|
                lazy ({ 
                         columns =[
                                    MetaColumn.Make(this.RecordTypeCode, 1, Format.leftPadString)
                                    MetaColumn.Make(this.ServiceClassCode, 3, Format.leftPadString)
                                    MetaColumn.Make(this.CompanyName, 16, Format.rightPadString)
                                    MetaColumn.Make(this.CompanyDiscretionaryData, 20, Format.rightPadString)
                                    MetaColumn.Make(this.CompanyIdentification, 10, Format.leftPadString)
                                    MetaColumn.Make(this.StandardEntryClass, 3, Format.leftPadString)
                                  ]
                         length = 94
                     })

    member this.Entries 
        with get () = this.GetChild<EntryDetail IList>(lazy upcast List())
    member this.BatchControl 
        with get () = this.GetChild<BatchControlRecord MaybeRecord>(lazy NoRecord)
        and set value = this.SetChild<BatchControlRecord MaybeRecord>(value)
        
        
    member this.ServiceClassCode
        with get () = this.GetColumn()
        and set value = this.SetColumn<string> value

    member this.CompanyName
        with get () = this.GetColumn()
        and set value = this.SetColumn<string> value

    member this.CompanyDiscretionaryData
        with get () = this.GetColumn()
        and set value = this.SetColumn<string> value
        
    member this.CompanyIdentification
        with get () = this.GetColumn()
        and set value = this.SetColumn<string> value
        
    member this.StandardEntryClass
        with get () = this.GetColumn()
        and set value = this.SetColumn<string> value

type FileControlRecord(rowInput) =
    inherit NachaRecord(rowInput, "9")

    override this.Setup () = 
        setup this <|
                lazy ({ 
                         columns =[
                                  ]
                         length = 94
                     })
       
type FileHeaderRecord(rowInput) =
    inherit NachaRecord(rowInput, "1")
    
    member this.Batches 
        with get () = this.GetChild<BatchHeaderRecord IList>(lazy upcast List())
    member this.FileControl 
        with get () = this.GetChild<FileControlRecord MaybeRecord>(lazy NoRecord)
        and set value = this.SetChild<FileControlRecord MaybeRecord>(value)
        
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