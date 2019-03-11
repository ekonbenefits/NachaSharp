(*
   Copyright 2018 EkonBenefits

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*)

namespace NachaSharp

open System
open System.Runtime.InteropServices
open FSharp.Data.FlatFileMeta
open FSharp.Interop.Compose.Linq

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
                            originatingDFIIdent:string,
                            batchNum:int,
                            [<Optional;DefaultParameterValue("")>] companyDescretionaryData:string,
                            [<Optional;DefaultParameterValue(Nullable<DateTime>())>] 
                            companyDescriptiveDate:DateTime Nullable
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
            bh.OriginatingDFIIdentification <- originatingDFIIdent
            bh.BatchNumber <- batchNum
            bh.CompanyDiscretionaryData <- companyDescretionaryData
            bh.CompanyDescriptiveDate <- companyDescriptiveDate
            
            let! bc = BatchControlRecord
            bh.BatchControl <- SomeRow <| bc
            
            bc.ServiceClassCode <- bh.ServiceClassCode
            bc.CompanyIdentification <- bh.CompanyIdentification
            bc.OriginatingDFIIdentification <- bh.OriginatingDFIIdentification
            
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
            columns     8    this.OriginatingDFIIdentification    Format.leftPadString
            columns     7    this.BatchNumber                 NachaFormat.numeric
    
            checkLength 94
        }

    member this.Entries 
        with get () = this.GetChildList<EntryDetail>(1)
        
    member this.CreateEmptyEntry():EntryDetail = createRow {
             let castIt = (fun x -> x :> EntryDetail)
             let f:(string->EntryDetail) = match (this.StandardEntryClass) with
                                             | "CCD"-> EntryCCD.Construct >> castIt
                                             | "PPD" -> EntryPPD.Construct >> castIt
                                             | "CTX" -> EntryCTX.Construct >> castIt
                                             |  x -> (fun row -> EntryWildCard(x, row)) >> castIt
                                            
             return! f
        }
        
    member this.BatchControl 
        with get () = this.GetChild<BatchControlRecord>(2)
        and set value = this.SetChild<BatchControlRecord>(2,value)
        
    override this.CalculateImpl () =
                 base.CalculateImpl()
              
                 maybeRow {
                    let! bc = this.BatchControl
                    bc.Entry_AddendaCount <- 
                        (this.Entries |> Seq.length)
                        + (this.Entries |> Seq.collect (fun x->x.Addenda) |> Seq.length)
                    
                    bc.TotalCreditEntryAmount <- 
                        this.Entries 
                            |> Seq.filter(fun x-> x.TransactionCode.ActionType = Credit)
                            |> Seq.sumBy (fun x-> x.Amount)
                 
                    bc.TotalDebitEntryAmount <- 
                                        this.Entries 
                                            |> Seq.filter(fun x-> x.TransactionCode.ActionType = Debit)
                                            |> Seq.sumBy (fun x-> x.Amount)
                    bc.EntryHash <- 
                        this.Entries
                            |> Seq.map (fun x-> x.ReceivingDfiIdentification 
                                                    |> Int32.TryParse
                                                    |> function | (true, res)-> res | _ -> 0
                                        )
                            |> Seq.sum  
                                
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

    member this.OriginatingDFIIdentification
        with get () = this.GetColumn()
        and set value = this.SetColumn<string> value
        
    member this.BatchNumber
        with get () = this.GetColumn()
        and set value = this.SetColumn<int> value 
        
        
