namespace NachaSharp

open System.Collections.Generic
open FSharp.Data.FlatFileMeta
open FSharp.Data.FlatFileMeta.MetaDataHelper

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
                                    MetaColumn.Make(1, this.RecordTypeCode, Format.leftPadString)
                                    MetaColumn.PlaceHolder(78)
                                    MetaColumn.Make(1, this.AddendaRecordedIndicator, Format.zerodInt)
                                    MetaColumn.PlaceHolder(14)
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
    