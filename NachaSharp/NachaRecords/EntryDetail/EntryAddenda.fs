namespace NachaSharp

open System
open System.Collections.Generic
open FSharp.Data.FlatFileMeta
open FSharp.Data.FlatFileMeta.MetaDataHelper

[<AbstractClass>]
type EntryAddenda(rowInput) =
    inherit NachaRecord(rowInput, "7")

type EntryAddendaWildCard(rowInput) =
    inherit EntryAddenda(rowInput)
    override this.Setup () = 
        setup this <|
                lazy ({ 
                         columns =[
                                  ]
                         length = 94
                     })