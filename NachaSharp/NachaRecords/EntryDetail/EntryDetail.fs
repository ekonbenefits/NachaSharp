namespace NachaSharp

open FSharp.Data.FlatFileMeta

[<AbstractClass>]
type EntryDetail(batchSEC, rowInput) =
    inherit NachaRecord(rowInput, "6")

    abstract EntrySEC:string with get

    override this.IsIdentified() =
        base.IsIdentified() && batchSEC = this.EntrySEC
    
    member this.Addenda 
        with get () = this.GetChildList<EntryAddenda>()
    
    member this.AddendaRecordedIndicator
        with get () = this.GetColumn<int> ()
        and set value = this.SetColumn<int> value
        
    member this.TransactionCode
        with get () = this.GetColumn<TranCode> ()
        and set value = this.SetColumn<TranCode> value
        
    member this.CheckDigit
            with get () = this.GetColumn<int> ()
            and set value = this.SetColumn<int> value
            
    member this.DfiAccountNUmber
            with get () = this.GetColumn<string> ()
            and set value = this.SetColumn<string> value 
            
    member this.Amount
            with get () = this.GetColumn<decimal> ()
            and set value = this.SetColumn<decimal> value
        
    member this.ReceivingDfiIdentification
                with get () = this.GetColumn ()
                and set value = this.SetColumn<string> value 
                
    member this.TraceNumber
                with get () = this.GetColumn ()
                and set value = this.SetColumn<string> value  

type EntryWildCard(batchSEC, rowInput) =
    inherit EntryDetail(batchSEC, rowInput)
    override __.EntrySEC with get () = batchSEC

    override this.Setup () = setupMetaFor this {
                columns      1      this.RecordTypeCode             NachaFormat.alpha
                columns      2      this.TransactionCode            NachaFormat.tranCode
                columns      8      this.ReceivingDfiIdentification Format.leftPadString
                columns      1      this.CheckDigit                 NachaFormat.numeric
                columns     17      this.DfiAccountNUmber           Format.leftPadString
                columns     10      this.Amount                     Format.reqMoney
                placeholder 15
                placeholder 22
                placeholder  2
                columns      1      this.AddendaRecordedIndicator   NachaFormat.numeric
                columns     15      this.TraceNumber                NachaFormat.alpha
                
                checkLength 94
        }

type EntryCCD(batchSEC, rowInput) =
    inherit EntryDetail(batchSEC, rowInput)
    static let entrySEC = "CCD"
    static member Construct(r) = EntryCCD(entrySEC, r)
    override __.EntrySEC with get () = entrySEC
    
    static member Create() = createRow {
         return! EntryCCD.Construct
    }
    
    override this.Setup () = setupMetaFor this {
                columns  1 this.RecordTypeCode          NachaFormat.alpha
                columns  2 this.TransactionCode         NachaFormat.tranCode
                columns  8 this.ReceivingDfiIdentification Format.leftPadString
                columns  1 this.CheckDigit              NachaFormat.numeric
                columns 17 this.DfiAccountNUmber        Format.leftPadString
                columns 10 this.Amount                  Format.reqMoney
                columns 15 this.IdentificationNumber    Format.leftPadString
                columns 22 this.ReceivingCompanyName    NachaFormat.alpha
                columns  2 this.DiscretionaryData       NachaFormat.alpha
                columns  1 this.AddendaRecordedIndicator NachaFormat.numeric
                columns 15 this.TraceNumber             NachaFormat.alpha
                
                checkLength 94
        }



    member this.IdentificationNumber
            with get () = this.GetColumn ()
            and set value = this.SetColumn<string> value 
    member this.ReceivingCompanyName
            with get () = this.GetColumn ()
            and set value = this.SetColumn<string> value
    member this.DiscretionaryData
            with get () = this.GetColumn ()
            and set value = this.SetColumn<string> value

                     
type EntryPPD(batchSEC, rowInput) =
    inherit EntryDetail(batchSEC, rowInput)
    
    //setup SEC type for entry
    static let entrySEC = "PPD"
    static member Construct(r) = EntryCCD(entrySEC, r)
    override __.EntrySEC with get () = entrySEC
    
    static member Create() = createRow {
            return! EntryPPD.Construct
    }
    
    override this.Setup () = setupMetaFor this {
    
                 columns  1 this.RecordTypeCode         NachaFormat.alpha
                 columns  2 this.TransactionCode        NachaFormat.tranCode
                 columns  8 this.ReceivingDfiIdentification Format.leftPadString
                 columns  1 this.CheckDigit             NachaFormat.numeric
                 columns 17 this.DfiAccountNUmber       Format.leftPadString
                 columns 10 this.Amount                 Format.reqMoney
                 columns 15 this.IndividualIdentificationNumber Format.leftPadString
                 columns 22 this.IndividualName         NachaFormat.alpha
                 columns  2 this.DiscretionaryData      NachaFormat.alpha
                 columns  1 this.AddendaRecordedIndicator NachaFormat.numeric
                 columns 15 this.TraceNumber            NachaFormat.alpha
                 
                 checkLength 94
        }
  
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