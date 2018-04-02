namespace NachaSharp

open FSharp.Data.FlatFileMeta

type BatchControlRecord(rowInput) =
    inherit NachaRecord(rowInput, "8")
    
    static member Create() = createRow {
            return! BatchControlRecord
        }

    override this.Setup () = setupMetaFor this {
        columns     1    this.RecordTypeCode            Format.leftPadString
        columns     3    this.ServiceClassCode          Format.leftPadString
        columns     6    this.Entry_AddendaCount        Format.zerodInt
        columns    10    this.EntryHash                 Format.zerodInt
        columns    12    this.TotalDebitEntryAmount     Format.reqMoney
        columns    12    this.TotalCreditEntryAmount    Format.reqMoney
        columns    10    this.CompanyIdentification     Format.rightPadString
        columns    19    this.MAC                       Format.leftPadString
        placeholder 6
        columns     8    this.OriginatingDfiIdentification    Format.leftPadString
        columns     7    this.BatchNumber               Format.zerodInt
        
        checkLength 94
    }
                     
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
            
            
            
            