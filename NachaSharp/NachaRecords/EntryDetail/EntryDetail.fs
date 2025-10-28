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

/// Module for validating ABA routing numbers and check digits
module RoutingNumberValidation =
    
    /// Calculates the ABA routing number check digit using the modulo 10 algorithm
    /// Formula: 3 * (d1 + d4 + d7) + 7 * (d2 + d5 + d8) + (d3 + d6 + d9) mod 10 = 0
    let calculateCheckDigit (routingNumber: string) : int option =
        if routingNumber.Length <> 8 then
            None
        else
            try
                let digits = routingNumber |> Seq.map (fun c -> int c - int '0') |> Seq.toArray
                
                // Verify all characters are digits
                if digits |> Array.exists (fun d -> d < 0 || d > 9) then
                    None
                else
                    let sum = 3 * (digits[0] + digits[3] + digits[6]) +
                              7 * (digits[1] + digits[4] + digits[7]) +
                              (digits[2] + digits[5])
                    
                    let checkDigit = (10 - (sum % 10)) % 10
                    Some checkDigit
            with
            | _ -> None
    
    /// Validates a complete 9-digit ABA routing number (8 digits + check digit)
    let validateRoutingNumber (fullRoutingNumber: string) : bool =
        if fullRoutingNumber.Length <> 9 then
            false
        else
            let routingBase = fullRoutingNumber.Substring(0, 8)
            let providedCheckDigit = 
                try
                    int fullRoutingNumber[8] - int '0'
                with
                | _ -> -1
            
            match calculateCheckDigit routingBase with
            | Some calculatedCheckDigit -> calculatedCheckDigit = providedCheckDigit
            | None -> false

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
             
             // field 'Addenda Record Indicator' should be either 0 or 1, regardless of amount of addenda
             this.AddendaRecordedIndicator <- if this.Addenda |> Seq.isEmpty then 0 else 1
             
             // Calculate and validate routing number check digit
             let routingNumber = this.ReceivingDfiIdentification
             if not (System.String.IsNullOrWhiteSpace(routingNumber)) && routingNumber.Length = 8 then
                 match RoutingNumberValidation.calculateCheckDigit routingNumber with
                 | Some calculatedCheckDigit ->
                     this.CheckDigit <- calculatedCheckDigit
                 | None ->
                     // Invalid routing number format - validation will catch this elsewhere
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

type EntryWEB(batchSEC, rowInput) =
    inherit EntryDetail(batchSEC, rowInput)
    
    //setup SEC type for entry
    static let entrySEC = "WEB"
    static member Construct(r) = EntryWEB(entrySEC, r)
    override __.EntrySEC with get () = entrySEC
    
    static member Create() = createRow {
            return! EntryWEB.Construct
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
                 columns  2 this.PaymentTypeCode        NachaFormat.alpha
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
    member this.PaymentTypeCode
            with get () = this.GetColumn ()
            and set value = this.SetColumn<string> value
                   