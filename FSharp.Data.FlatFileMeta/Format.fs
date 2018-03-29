namespace FSharp.Data.FlatFileMeta
open System
open FSharp.Interop.Compose.System

module Format =

    type FormatPairs<'T> = (string -> 'T) * (int -> 'T -> string)

    module Str =
        let fillToLengthWith char length =  Array.init length (fun _ -> char) |> String
        let fillToLength = fillToLengthWith ' '
        
    
        let getRightTrim = String.trimEnd [|' '|]
        let setRightPad length = String.Full.padRight length ' '
        let getLeftTrim = String.trimStart [|' '|]
        let setLeftPad length = String.Full.padLeft length ' '
        
    module Int =
        let getReq (value:string) = value |> int
        let setZerod length (value:int)= value |> string |> String.Full.padLeft length '0'

        
    module DateAndTime =
        open System.Globalization
        let parseReq format value = DateTime.ParseExact(value, format, CultureInfo.InvariantCulture)
        let toStringReq format (value:DateTime) = value.ToString(format, CultureInfo.InvariantCulture)     
        let parseOpt (format:string) (value:string) = 
                    match DateTime.TryParseExact(value, 
                                                 format,
                                                 CultureInfo.InvariantCulture,
                                                 DateTimeStyles.NoCurrentDateDefault
                                                ) with
                        | true, d -> Some(d)
                        | _______ -> None
        
        let toStringOpt format (length:int) (value:DateTime option)=
           match value with
               | Some(d) -> d |> toStringReq format
               | None -> length |> Str.fillToLength 
        
        let getYYMMDD = parseReq "yyMMdd"
        let setYYMMDD (_:int) = toStringReq "yyMMdd"      
        
        let getOptHHMM = parseOpt "HHmm"
        let setOptHHMM = toStringOpt "HHmm"
        
        
    let zerodInt:FormatPairs<_>  = (Int.getReq, Int.setZerod)
    let rightPadString:FormatPairs<_> = (Str.getRightTrim, Str.setRightPad)
    let leftPadString:FormatPairs<_>  = (Str.getLeftTrim, Str.setLeftPad)
    let reqYYMMDD:FormatPairs<_>  = (DateAndTime.getYYMMDD, DateAndTime.setYYMMDD)
    let optHHMM:FormatPairs<_>  = (DateAndTime.getOptHHMM, DateAndTime.setOptHHMM)