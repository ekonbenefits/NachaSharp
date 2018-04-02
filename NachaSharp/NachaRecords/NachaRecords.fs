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

[<AbstractClass>]
type NachaRecord(rowInput, recordTypeCode) =
    inherit FlatRow(rowInput)
    
    override this.PostSetup() =
        if this.IsNew() then
           this.RecordTypeCode <- recordTypeCode
            
    override this.IsIdentified() =
        let blockFiller = "9"
        let charBlockFiller = blockFiller |> Seq.head
        let matchesType = this.RecordTypeCode = recordTypeCode
        let notPossibleToBeFiller = this.RecordTypeCode <> blockFiller
        let verifyIsNotFiller = lazy (this.ToRawString() |> Seq.exists (fun x -> x <> charBlockFiller))
        
        matchesType
            && (notPossibleToBeFiller || verifyIsNotFiller.Force())
                
    member this.RecordTypeCode
        with get () = this.GetColumn ()
        and set value = this.SetColumn<string> value