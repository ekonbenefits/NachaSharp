namespace NachaSharp

open FSharp.Data.FlatFileMeta
open FSharp.Data.FlatFileMeta.FlatRowSetup


type BatchControlRecord(rowInput) =
    inherit NachaRecord(rowInput, "8")
    
    static member Create() = createRow {
            return! BatchControlRecord
        }

    override this.Setup () = 
        setup this <|
                lazy ({ 
                         columns =[
                                        MetaColumn.Make( 1, this.RecordTypeCode, Format.leftPadString)
                                        MetaColumn.Make( 3, this.ServiceClassCode, Format.leftPadString)
                                        MetaColumn.Make( 6, this.Entry_AddendaCount, Format.zerodInt)
                                        MetaColumn.Make(10, this.EntryHash, Format.zerodInt)
                                        MetaColumn.Make(12, this.TotalDebitEntryAmount, Format.reqMoney)
                                        MetaColumn.Make(12, this.TotalCreditEntryAmount, Format.reqMoney)
                                        MetaColumn.Make(10, this.CompanyIdentification, Format.rightPadString)
                                        MetaColumn.Make(19, this.MAC, Format.leftPadString)
                                        MetaColumn.PlaceHolder(6)
                                        MetaColumn.Make( 8, this.OriginatingDfiIdentification, Format.leftPadString)
                                        MetaColumn.Make( 7, this.BatchNumber, Format.zerodInt)

                                  ]
                         length = 94
                     })
                     
    member this.ServiceClassCode
            with get () = this.GetColumn ()
            and set value = this.SetColumn<string> value
    member this.Entry_AddendaCount
            with get () = this.GetColumn ()
            and set value = this.SetColumn<int> value
    member this.EntryHash
            with get () = this.GetColumn ()
            and set value = this.SetColumn<int> value            
    member this.TotalDebitEntryAmount
            with get () = this.GetColumn ()
            and set value = this.SetColumn<decimal> value
    member this.TotalCreditEntryAmount
            with get () = this.GetColumn ()
            and set value = this.SetColumn<decimal> value
    member this.CompanyIdentification
            with get () = this.GetColumn ()
            and set value = this.SetColumn<string> value           
    member this.MAC
            with get () = this.GetColumn ()
            and set value = this.SetColumn<string> value
    member this.OriginatingDfiIdentification
            with get () = this.GetColumn ()
            and set value = this.SetColumn<string> value
    member this.BatchNumber
            with get () = this.GetColumn ()
            and set value = this.SetColumn<int> value             
            
            
            
            