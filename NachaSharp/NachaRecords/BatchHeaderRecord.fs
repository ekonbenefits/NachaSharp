namespace NachaSharp

open System
open FSharp.Data.FlatFileMeta

type BatchHeaderRecord(rowInput) =
    inherit NachaRecord(rowInput, "5")
    
     static member Create(
                            serviceClassCode:string,
                            companyName:string,
                            companyIdentification:string,
                            secCode:string,
                            companyEntryDesc:string,
                            effectiveEntryDate: DateTime,
                            originatorStatusCode:string,
                            originatingDfiIdent:string,
                            batchNum:int,
                            ?companyDescretionaryData:string,
                            ?companyDescriptiveDate:DateTime
                         ) =
        createRow {
            let! bh = BatchHeaderRecord
            
            bh.ServiceClassCode <- serviceClassCode
            bh.CompanyName <- companyName
            bh.CompanyIdentification <- companyIdentification
            bh.StandardEntryClass <- secCode
            bh.CompanyEntryDescription <- companyEntryDesc
            bh.EffectiveEntryDate <- effectiveEntryDate
            bh.OriginatorStatusCode <- originatorStatusCode
            bh.OriginatingDfiIndentifications <- originatingDfiIdent
            bh.BatchNumber <- batchNum
            bh.CompanyDiscretionaryData <- defaultArg companyDescretionaryData ""
            bh.CompanyDescriptiveDate <- companyDescriptiveDate |> Option.toNullable
            
            bh.BatchControl <- SomeRow <| BatchControlRecord.Create()
            return bh
        }
            
    override this.Setup () = setupMetaFor this {
            columns     1    this.RecordTypeCode              NachaFormat.alpha
            columns     3    this.ServiceClassCode            NachaFormat.alpha
            columns    16    this.CompanyName                 NachaFormat.alpha
            columns    20    this.CompanyDiscretionaryData    NachaFormat.alpha
            columns    10    this.CompanyIdentification       NachaFormat.alpha
            columns     3    this.StandardEntryClass          NachaFormat.alphaUpper
            columns    10    this.CompanyEntryDescription     NachaFormat.alpha
            columns     6    this.CompanyDescriptiveDate      Format.optYYMMDD
            columns     6    this.EffectiveEntryDate          Format.reqYYMMDD
            columns     3    this.SettlementDate              Format.optJulian
            columns     1    this.OriginatorStatusCode        NachaFormat.alpha
            columns     8    this.OriginatingDfiIndentifications    Format.leftPadString
            columns     7    this.BatchNumber                 NachaFormat.numeric
    
            checkLength 94
        }

    member this.Entries 
        with get () = this.GetChildList<EntryDetail>()
        
    member this.BatchControl 
        with get () = this.GetChild<BatchControlRecord>(lazy NoRow)
        and set value = this.SetChild<BatchControlRecord>(value)
        
    override this.Calculate () =
                 base.Calculate()
                 maybeRow {
                    let! bc = this.BatchControl
                    bc.Entry_AddendaCount <- 
                        (this.Entries |> Seq.length)
                        + (this.Entries |> Seq.collect (fun x->x.Addenda) |> Seq.length)
                    
                    bc.TotalCreditEntryAmount <- 
                        this.Entries 
                            |> Seq.filter(fun x-> match x.TransactionCode with Credit(_) -> true | _-> false)
                            |> Seq.sumBy (fun x-> x.Amount)
                 
                    bc.TotalDebitEntryAmount <- 
                                        this.Entries 
                                            |> Seq.filter(fun x-> match x.TransactionCode with Debit(_) -> true | _-> false)
                                            |> Seq.sumBy (fun x-> x.Amount)
                                            
                 } |> ignore
                 ()
    
        
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
        
    member this.CompanyEntryDescription
        with get () = this.GetColumn()
        and set value = this.SetColumn<string> value       
         
    member this.CompanyDescriptiveDate
        with get () = this.GetColumn()
        and set value = this.SetColumn<DateTime Nullable> value        
  
    member this.EffectiveEntryDate
        with get () = this.GetColumn()
        and set value = this.SetColumn<DateTime> value            
          
    member this.SettlementDate
        with get () = this.GetColumn()
        and set value = this.SetColumn<DateTime Nullable> value          
          
    member this.OriginatorStatusCode
        with get () = this.GetColumn()
        and set value = this.SetColumn<string> value        

    member this.OriginatingDfiIndentifications
        with get () = this.GetColumn()
        and set value = this.SetColumn<string> value
        
    member this.BatchNumber
        with get () = this.GetColumn()
        and set value = this.SetColumn<int> value 
        
        
