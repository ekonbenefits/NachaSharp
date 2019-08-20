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
open FSharp.Interop.Compose.System

[<AutoOpen>]
module NachaValues =
    
    type TranAccountType = Checking=2 
                           | Savings=3 
                           | GeneralLedger=4 
                           | Loan=5 
                           | AccountingRecord =8
                           | Unknown = -1
    
    type TranActionType = Credit
                           | Debit
                           | Unknown
    
    
    type TranCode() =
         inherit DataCode<TranCode>()
    
         
         static member CheckingCredit =
            TranCode.GetCode()
         
         static member GeneralLedgerCredit =
                 TranCode.GetCode()
         
         static member SavingsCredit =
             TranCode.GetCode()
          
         static member CheckingDebit =
             TranCode.GetCode()
             
         static member GeneralLedgerDebit =
             TranCode.GetCode()
         
         static member SavingsDebit =
             TranCode.GetCode()
    
    
         member this.ActionType = 
                match this.Code.[0],this.Code.[1] with
                        | '8',_ -> Credit
                        | _,'1' | _,'2' | _,'3' | _,'4' -> Credit
                        | _,'6' | _,'7' | _,'8' | _,'9' -> Debit
                        | _ -> Unknown
         
         member this.AccountType = 
                let success, acc = System.Enum.TryParse(this.Code.[0..0])
                if success then acc else TranAccountType.Unknown
         
         override __.Setup () = DataCodeExtension.dataCodeMeta {
                code TranCode.CheckingCredit      "22"
                code TranCode.SavingsCredit       "32"
                code TranCode.GeneralLedgerCredit "42"
                code TranCode.CheckingDebit       "27"
                code TranCode.SavingsDebit        "37"
                code TranCode.GeneralLedgerDebit  "47"
          }




[<RequireQualifiedAccess>]
module NachaFormat =
    module Str = 
        let setUpper length = String.toUpper >> Format.Str.setRightPad length 
    
    module Int =
        let setRightMost (length, _) (value:int) = 
            let str = value |> string |> String.Full.padLeft length '0'
            let start = if str.Length > length then
                            str.Length - length
                        else
                            0
            str.[start..(str.Length - 1)]
            
    
     
    let tranCode = Format.reqDataCode<TranCode>
    let numeric = Format.zerodInt
    let hash = Format.zerodInt64
    let alpha = Format.rightPadString
    let alphaUpper:Format.FormatPairs<_> = (Format.Str.getRightTrim, Str.setUpper)