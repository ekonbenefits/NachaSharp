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
