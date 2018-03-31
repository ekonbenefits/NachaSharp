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
            
    override this.Setup () = 
        FlatRowProvider.setup this <|
                lazy ({ 
                         columns =[
                                    MetaColumn.Make( 1, this.RecordTypeCode,     Format.leftPadString)
                                    MetaColumn.Make( 3, this.ServiceClassCode,   Format.leftPadString)
                                    MetaColumn.Make(16, this.CompanyName,        Format.rightPadString)
                                    MetaColumn.Make(20, this.CompanyDiscretionaryData, Format.rightPadString)
                                    MetaColumn.Make(10, this.CompanyIdentification, Format.leftPadString)
                                    MetaColumn.Make( 3, this.StandardEntryClass, Format.leftPadString)
                                    MetaColumn.Make(10, this.CompanyEntryDescription, Format.rightPadString)
                                    MetaColumn.Make( 6, this.CompanyDescriptiveDate, Format.optYYMMDD)
                                    MetaColumn.Make( 6, this.EffectiveEntryDate, Format.reqYYMMDD)
                                    MetaColumn.Make( 3, this.SettlementDate, Format.optJulian)
                                    MetaColumn.Make( 1, this.OriginatorStatusCode, Format.leftPadString)
                                    MetaColumn.Make( 8, this.OriginatingDfiIndentifications, Format.leftPadString)
                                    MetaColumn.Make( 7, this.BatchNumber, Format.zerodInt)
                                  ]
                         length = 94
                     })

    member this.Entries 
        with get () = this.GetChildList<EntryDetail>()
        
    member this.BatchControl 
        with get () = this.GetChild<BatchControlRecord>(lazy NoRow)
        and set value = this.SetChild<BatchControlRecord>(value)
        
    override this.Calculate () =
                 base.Calculate()
                 match this.BatchControl with
                    | SomeRow(bc) -> 
                        let c = this.Entries |> Seq.sumBy (fun x->x.Addenda.Count + 1)
                        bc.Entry_AddendaCount <- c
                    | _ -> ()
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
        
        
