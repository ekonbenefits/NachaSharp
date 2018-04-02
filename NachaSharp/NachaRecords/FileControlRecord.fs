namespace NachaSharp

open FSharp.Data.FlatFileMeta

type FileControlRecord(rowInput) =
    inherit NachaRecord(rowInput, "9")
    
    static member Create() = createRow {
            return! FileControlRecord
        }
        
    override this.Setup () = setupMetaFor this {
            columns     1    this.RecordTypeCode            NachaFormat.alpha
            columns     6    this.BatchCount                NachaFormat.numeric
            columns     6    this.BlockCount                NachaFormat.numeric
            columns     8    this.Entry_AddendaCount        NachaFormat.numeric
            columns    10    this.EntryHash                 NachaFormat.numeric
            columns    12    this.TotalDebitEntryAmount     Format.reqMoney
            columns    12    this.TotalCreditEntryAmount    Format.reqMoney
            placeholder 39
            
            checkLength 94
        }
           

    member this.BatchCount 
        with get () = this.GetColumn()
        and set value = this.SetColumn<int> value

    member this.BlockCount
        with get () = this.GetColumn()
        and set value = this.SetColumn<int> value

    member this.Entry_AddendaCount
        with get () = this.GetColumn()
        and set value = this.SetColumn<int> value

    member this.EntryHash
        with get () = this.GetColumn()
        and set value = this.SetColumn<int> value
       
    member this.TotalDebitEntryAmount
        with get () = this.GetColumn()
        and set value = this.SetColumn<decimal> value

    member this.TotalCreditEntryAmount
        with get () = this.GetColumn()
        and set value = this.SetColumn<decimal> value
