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

open FSharp.Data.FlatFileMeta
open FSharp.Interop.Compose.Linq

[<AbstractClass>]
type EntryDetail(batchSEC, rowInput) =
    inherit NachaRecord(rowInput, "6")

    abstract EntrySEC:string with get

    override this.IsIdentified() =
        base.IsIdentified() && batchSEC = this.EntrySEC
        
    override this.PostSetup() =
        base.PostSetup()
        if this.IsNew() then
            this.AddendaRecordedIndicator <-0
        
    override this.CalculateImpl () =
             base.CalculateImpl()
             
             //because only support adding entry 5 this works.
             this.Addenda
                |> Enumerable.ofType<EntryAddenda05>
                |> Seq.iteri (fun i a -> a.AddendaSeqNum <- i + 1)
             
             this.AddendaRecordedIndicator <- this.Addenda |> Seq.length
             
             ()
    
    member this.Addenda 
        with get () = this.GetChildList<EntryAddenda>(1)
    
    member this.AddendaRecordedIndicator
        with get () = this.GetColumn<int> ()
        and set value = this.SetColumn<int> value
        
    member this.TransactionCode
        with get () = this.GetColumn<TranCode> ()
        and set value = this.SetColumn<TranCode> value
        
    member this.CheckDigit
            with get () = this.GetColumn<int> ()
            and set value = this.SetColumn<int> value
            
    member this.DfiAccountNumber
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
                columns     17      this.DfiAccountNumber           NachaFormat.alpha
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
                columns 17 this.DfiAccountNumber        NachaFormat.alpha
                columns 10 this.Amount                  Format.reqMoney
                columns 15 this.IdentificationNumber    NachaFormat.alpha
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


type EntryCTX(batchSEC, rowInput) =
    inherit EntryDetail(batchSEC, rowInput)
    static let entrySEC = "CTX"
    static member Construct(r) = EntryCTX(entrySEC, r)
    override __.EntrySEC with get () = entrySEC
    
    static member Create() = createRow {
         return! EntryCTX.Construct
    }
    
    override this.CalculateImpl() =
        base.CalculateImpl()
        this.NumberOfAddendaRecords <- this.Addenda.Count
        ()
    
    override this.Setup () = setupMetaFor this {
                columns  1 this.RecordTypeCode          NachaFormat.alpha
                columns  2 this.TransactionCode         NachaFormat.tranCode
                columns  8 this.ReceivingDfiIdentification Format.leftPadString
                columns  1 this.CheckDigit              NachaFormat.numeric
                columns 17 this.DfiAccountNumber        NachaFormat.alpha
                columns 10 this.Amount                  Format.reqMoney
                columns 15 this.IdentificationNumber    NachaFormat.alpha
                columns  4 this.NumberOfAddendaRecords  NachaFormat.numeric
                columns 16 this.ReceivingCompanyNameOrNum    NachaFormat.alpha
                placeholder 2
                columns  2 this.DiscretionaryData       NachaFormat.alpha
                columns  1 this.AddendaRecordedIndicator NachaFormat.numeric
                columns 15 this.TraceNumber             NachaFormat.alpha
                
                checkLength 94
        }

    member this.NumberOfAddendaRecords
            with get () = this.GetColumn ()
            and set value = this.SetColumn<int> value 

    member this.IdentificationNumber
            with get () = this.GetColumn ()
            and set value = this.SetColumn<string> value 
    member this.ReceivingCompanyNameOrNum
            with get () = this.GetColumn ()
            and set value = this.SetColumn<string> value
    member this.DiscretionaryData
            with get () = this.GetColumn ()
            and set value = this.SetColumn<string> value
                     
type EntryPPD(batchSEC, rowInput) =
    inherit EntryDetail(batchSEC, rowInput)
    
    //setup SEC type for entry
    static let entrySEC = "PPD"
    static member Construct(r) = EntryPPD(entrySEC, r)
    override __.EntrySEC with get () = entrySEC
    
    static member Create() = createRow {
            return! EntryPPD.Construct
    }
    
    override this.Setup () = setupMetaFor this {
    
                 columns  1 this.RecordTypeCode         NachaFormat.alpha
                 columns  2 this.TransactionCode        NachaFormat.tranCode
                 columns  8 this.ReceivingDfiIdentification Format.leftPadString
                 columns  1 this.CheckDigit             NachaFormat.numeric
                 columns 17 this.DfiAccountNumber       NachaFormat.alpha
                 columns 10 this.Amount                 Format.reqMoney
                 columns 15 this.IndividualIdentificationNumber NachaFormat.alpha
                 columns 22 this.IndividualName         NachaFormat.alpha
                 columns  2 this.DiscretionaryData      NachaFormat.alpha
                 columns  1 this.AddendaRecordedIndicator NachaFormat.numeric
                 columns 15 this.TraceNumber            NachaFormat.alpha
                 
                 checkLength 94
        }

    member this.IndividualIdentificationNumber
            with get () = this.GetColumn ()
            and set value = this.SetColumn<string> value 
    member this.IndividualName
            with get () = this.GetColumn ()
            and set value = this.SetColumn<string> value
    member this.DiscretionaryData
            with get () = this.GetColumn ()
            and set value = this.SetColumn<string> value
                   