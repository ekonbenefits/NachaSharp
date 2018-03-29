namespace FSharp.Data.FlatFileMeta

open System

module internal Helper =
    let inline optionOfStringEmpty str =
                    if str |> String.IsNullOrEmpty then 
                        None
                    else
                        Some(str)
    
    let inline optionOfStringWhitespace str =
                    if str |> String.IsNullOrWhiteSpace then 
                        None
                    else
                        Some(str)
