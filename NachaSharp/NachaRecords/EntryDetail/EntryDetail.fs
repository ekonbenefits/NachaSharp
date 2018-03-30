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
        with get () = this.GetChildList<EntryAddenda>()
    
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
                                    MetaColumn.PlaceHolder(77)
                                    MetaColumn.Make(1, this.AddendaRecordedIndicator, Format.zerodInt)
                                    MetaColumn.PlaceHolder(15)
                                  ]
                         length = 94
                     })

type EntryCCD(batchSEC, rowInput) =
    inherit EntryDetail("CCD", batchSEC, rowInput)
    
     static member Create() =
                MetaDataHelper.createRecord (fun x->EntryCCD("CCD", x)) ignore
    
    override this.Setup () = 
        setup this <|
                lazy ({ 
                         columns =[
                                     MetaColumn.Make( 1, this.RecordTypeCode, Format.leftPadString)
                                     MetaColumn.Make( 2, this.TransactionCode, Format.leftPadString)
                                     MetaColumn.Make( 8, this.ReceivingDfiIdentification, Format.leftPadString)
                                     MetaColumn.Make( 1, this.CheckDigit, Format.zerodInt)
                                     MetaColumn.Make(17, this.DfiAccountNUmber, Format.leftPadString)
                                     MetaColumn.Make(10, this.Amount, Format.reqMoney)
                                     MetaColumn.Make(15, this.IdentificationNumber, Format.leftPadString)
                                     MetaColumn.Make(22, this.ReceivingCompanyName, Format.leftPadString)
                                     MetaColumn.Make( 2, this.DiscretionaryData, Format.rightPadString)
                                     MetaColumn.Make( 1, this.AddendaRecordedIndicator, Format.zerodInt)
                                     MetaColumn.Make(15, this.TraceNumber, Format.leftPadString)
                                  ]
                         length = 94
                     })
    member this.TransactionCode
            with get () = this.GetColumn ()
            and set value = this.SetColumn<string> value
    member this.ReceivingDfiIdentification
            with get () = this.GetColumn ()
            and set value = this.SetColumn<string> value 
    member this.CheckDigit
            with get () = this.GetColumn ()
            and set value = this.SetColumn<int> value
    member this.DfiAccountNUmber
            with get () = this.GetColumn ()
            and set value = this.SetColumn<string> value 
    member this.Amount
            with get () = this.GetColumn ()
            and set value = this.SetColumn<decimal> value
    member this.IdentificationNumber
            with get () = this.GetColumn ()
            and set value = this.SetColumn<string> value 
    member this.ReceivingCompanyName
            with get () = this.GetColumn ()
            and set value = this.SetColumn<string> value
    member this.DiscretionaryData
            with get () = this.GetColumn ()
            and set value = this.SetColumn<string> value
    member this.TraceNumber
            with get () = this.GetColumn ()
            and set value = this.SetColumn<string> value  
                     
type EntryPPD(batchSEC, rowInput) =
    inherit EntryDetail("PPD", batchSEC, rowInput)
    override this.Setup () = 
        setup this <|
                lazy ({ 
                         columns =[ 
                                        MetaColumn.Make( 1, this.RecordTypeCode, Format.leftPadString)
                                        MetaColumn.Make( 2, this.TransactionCode, Format.leftPadString)
                                        MetaColumn.Make( 8, this.ReceivingDfiIdentification, Format.leftPadString)
                                        MetaColumn.Make( 1, this.CheckDigit, Format.zerodInt)
                                        MetaColumn.Make(17, this.DfiAccountNUmber, Format.leftPadString)
                                        MetaColumn.Make(10, this.Amount, Format.reqMoney)
                                        MetaColumn.Make(15, this.IndividualIdentificationNumber, Format.leftPadString)
                                        MetaColumn.Make(22, this.IndividualName, Format.leftPadString)
                                        MetaColumn.Make( 2, this.DiscretionaryData, Format.rightPadString)
                                        MetaColumn.Make( 1, this.AddendaRecordedIndicator, Format.zerodInt)
                                        MetaColumn.Make(15, this.TraceNumber, Format.leftPadString)
                              ]
                         length = 94
                     })
    member this.TransactionCode
            with get () = this.GetColumn ()
            and set value = this.SetColumn<string> value
    member this.ReceivingDfiIdentification
            with get () = this.GetColumn ()
            and set value = this.SetColumn<string> value 
    member this.CheckDigit
            with get () = this.GetColumn ()
            and set value = this.SetColumn<int> value
    member this.DfiAccountNUmber
            with get () = this.GetColumn ()
            and set value = this.SetColumn<string> value 
    member this.Amount
            with get () = this.GetColumn ()
            and set value = this.SetColumn<decimal> value
    member this.IndividualIdentificationNumber
            with get () = this.GetColumn ()
            and set value = this.SetColumn<string> value 
    member this.IndividualName
            with get () = this.GetColumn ()
            and set value = this.SetColumn<string> value
    member this.DiscretionaryData
            with get () = this.GetColumn ()
            and set value = this.SetColumn<string> value
    member this.TraceNumber
            with get () = this.GetColumn ()
            and set value = this.SetColumn<string> value                      