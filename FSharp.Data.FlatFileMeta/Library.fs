namespace FSharp.Data.FlatFileMeta
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open System.Runtime.CompilerServices


type ColumnIdentifier(key: string, start:int, length:int) =
    member this.Key = key
    member this.Start = start
    member this.Length = length
    
type Column<'T>(key: string, start:int, length:int, getValue: string -> 'T, setValue: int -> 'T -> string) =
    inherit ColumnIdentifier(key, start, length)
    member this.GetValue = getValue
    member this.SetValue = setValue



type BaseFlatRecord(?rowInput) =
        
        
    let ColumnMaps = Map.empty<string, ColumnIdentifier>
    let row = [|""|]
    
    member this.GetColumn([<CallerMemberName>] ?memberName: string) : 'T =
            let columnIdent = match memberName with
                                | Some(key) -> ColumnMaps |> Map.find key 
                                | None -> invalidArg "memberName" "Compiler should automatically fill this value"
            let data = row.[columnIdent.Start..columnIdent.Length] |> String.concat ""
            let columnDef:Column<'T> = downcast columnIdent
            data |> columnDef.GetValue 
            
    member this.SetColumn(value:'T,[<CallerMemberName>] ?memberName: string) : unit =
            let columnIdent = match memberName with
                                | Some(key) -> ColumnMaps |> Map.find key 
                                | None -> invalidArg "memberName" "Compiler should automatically fill this value"
            let columnDef:Column<'T> = downcast columnIdent
            let stringVal = value |> columnDef.SetValue columnIdent.Length
            row.[columnIdent.Start..columnIdent.Length] <- stringVal.ToCharArray() |> Array.map string
            
        
    member this.MakeColumn([<ReflectedDefinition>] value:Expr<'T>, start, length, getValue: string -> 'T, setValue) =
        
        let key = 
            match value with
            | PropertyGet(_, propOrValInfo, _) -> propOrValInfo.Name
            | ________________________________ -> invalidArg "value" "Must be a property get"
        Column(key, start,length, getValue, setValue)