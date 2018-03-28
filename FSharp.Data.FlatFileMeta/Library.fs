namespace FSharp.Data.FlatFileMeta
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open System.Runtime.CompilerServices
open System.Collections.Generic
open System
open System.IO
open FSharp.Interop.Compose.System

module Format =

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
        
    let zerodInt = (Int.getReq, Int.setZerod)
    let rightPadString = (Str.getRightTrim, Str.setRightPad)
    let leftPadString = (Str.getLeftTrim, Str.setLeftPad)
    let reqYYMMDD = (DateAndTime.getYYMMDD, DateAndTime.setYYMMDD)
    let optHHMM = (DateAndTime.getOptHHMM, DateAndTime.setOptHHMM)

type ColumnIdentifier(key: string, length:int) =
    member this.Key = key
    member this.Length = length
    
type Column<'T>(key: string, length:int, getValue: string -> 'T, setValue: int -> 'T -> string) =
    inherit ColumnIdentifier(key, length)
    member this.GetValue = getValue
    member this.SetValue = setValue


type MetaColumn =
    static member Make<'T>([<ReflectedDefinition>] value:Expr<'T> , length, (getValue: string -> 'T, setValue)) =
        
        let key = 
            match value with
            | PropertyGet(_, propOrValInfo, _) -> propOrValInfo.Name
            | ________________________________ -> invalidArg "value" "Must be a property get"
        Column(key, length, getValue, setValue)

type ParsedMeta = int * string list * Map<string, int * ColumnIdentifier>

type DefinedMeta = { columns: ColumnIdentifier list; length :int }

[<AbstractClass>]
type BaseFlatRecord(rowInput:string option) =
        
    let mutable rawData: string array = Array.empty
    let mutable columnKeys: string list = List.empty
    let mutable columnMap: Map<string, int * ColumnIdentifier> = Map.empty
    let mutable columnLength: int = 0
    
    abstract Setup: unit -> ParsedMeta
    abstract IsValid: unit -> bool
    default this.IsValid () = this.Row |> Array.length = columnLength

    member private this.LazySetup() =
        if columnMap |> Map.isEmpty then
            let totalLength, orderedKeys, mapMeta = this.Setup()
      
            columnLength <- totalLength
            columnKeys <- orderedKeys
            rawData <- match rowInput with
                        | Some (row) -> row |> Array.ofSeq |> Array.map string
                        | None -> Array.init totalLength (fun _ -> " ")
            columnMap <- columnMap
    
    member this.Keys =
        this.LazySetup()
        columnKeys
            
    member private this.Row =
        this.LazySetup()
        rawData
    
    member private this.ColumnMap =
        this.LazySetup()
        columnMap
    
    member this.Data(key:string):obj=
        this.GetColumn(key) |> box
    
    member this.RawData(key:string)=
        let start, columnIdent = this.ColumnMap |> Map.find key
        this.Row.[start..columnIdent.Length] |> String.concat ""             
            
    member this.GetColumn([<CallerMemberName>] ?memberName: string) : 'T =
        let start, columnIdent =
            match memberName with
                | Some(k) -> this.ColumnMap |> Map.find k 
                | None -> invalidArg "memberName" "Compiler should automatically fill this value"
        let data = this.Row.[start..columnIdent.Length] |> String.concat ""
        let columnDef:Column<'T> = downcast columnIdent
        data |> columnDef.GetValue 
            
    member this.SetColumn<'T>(value:'T, [<CallerMemberName>] ?memberName: string) =
        let start, columnIdent =
            match memberName with
                 | Some(key) -> this.ColumnMap |> Map.find key 
                 | None -> invalidArg "memberName" "Compiler should automatically fill this value"
        let columnDef:Column<'T> = downcast columnIdent
        let stringVal = value |> columnDef.SetValue columnIdent.Length
        this.Row.[start..columnIdent.Length] <- stringVal.ToCharArray() |> Array.map string
        
module MetaDataHelper =
    let cache = Dictionary<_, _>()

    let setup<'T>  (record:'T) (v: DefinedMeta Lazy) : ParsedMeta = 
        let k = typeof<'T>;
        if cache.ContainsKey(k) then
            cache.[k]
        else
            let meta = v.Force()
            let sumLength = meta.columns |> List.sumBy (fun x->x.Length)
            if sumLength <> meta.length then
                raise <| InvalidDataException(sprintf "Data columns sum to %i which is not the expected value %i" sumLength meta.length)
            try
                let result = meta.length,
                             meta.columns |> List.map (fun x->x.Key),
                             meta.columns 
                                 |> Seq.scan (fun state i -> i.Length + state) 0
                                 |> Seq.zip meta.columns
                                 |> Seq.map (fun (c, i) -> c.Key, (i,c))
                                 |> Map.ofSeq
                cache.[k] <- result
                result      
            with
                | ex -> raise <| InvalidDataException("Columns must have unique names", ex)