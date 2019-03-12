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

type BatchControlRecord(rowInput) =
    inherit NachaRecord(rowInput, "8")
    
    static member Create() = createRow {
            return! BatchControlRecord
        }

    override this.Setup () = setupMetaFor this {
        columns     1    this.RecordTypeCode            Format.leftPadString
        columns     3    this.ServiceClassCode          Format.leftPadString
        columns     6    this.Entry_AddendaCount        Format.zerodInt
        columns    10    this.EntryHash                 NachaFormat.hash
        columns    12    this.TotalDebitEntryAmount     Format.reqMoney
        columns    12    this.TotalCreditEntryAmount    Format.reqMoney
        columns    10    this.CompanyIdentification     Format.rightPadString
        columns    19    this.MAC                       Format.leftPadString
        placeholder 6
        columns     8    this.OriginatingDFIIdentification    Format.leftPadString
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
            and set value = this.SetColumn<int64> value            
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
    member this.OriginatingDFIIdentification
            with get () = this.GetColumn ()
            and set value = this.SetColumn<string> value
    member this.BatchNumber
            with get () = this.GetColumn ()
            and set value = this.SetColumn<int> value             
            
            
            
            