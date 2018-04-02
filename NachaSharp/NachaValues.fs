namespace NachaSharp

open FSharp.Data.FlatFileMeta
open FSharp.Interop.Compose.System

[<AutoOpen>]
module NachaValues =

    type AccountType = Checking=2 
                       | Savings=3 
                       | GeneralLedger=4 
                       | Loan=5 
                       | AccountingRecord =8
                       | Unknown = -1
    
    type TranCode = Credit of string
                    | Debit of  string 
                    | Unknown of string 

[<RequireQualifiedAccess>]
module NachaFormat =
    module Str = 
        let setUpper length = String.toUpper >> Format.Str.setRightPad length 
        
        
    module Codes = 
        let getTranCode (x:string) =
            let success, acc = System.Enum.TryParse(x.[0..0])
            let account = if success then acc else AccountType.Unknown
            match x.[1] with
                | '1' | '2' | '3' | '4' -> Credit(x)
                | '6' | '7' | '8' | '9' -> Debit(x)
                | _ -> Unknown(x)
        let setTransCode length x =
            match x with
                | Credit(s)-> s
                | Debit(s) -> s
                | _ -> invalidOp "can only set credit or debit"
                |> Format.Valid.checkFinal length
     
    let numeric = Format.zerodInt
    let alpha = Format.rightPadString
    let alphaUpper = (Format.Str.getRightTrim, Str.setUpper)
    let tranCode = (Codes.getTranCode, Codes.setTransCode)