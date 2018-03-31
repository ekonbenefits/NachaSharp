namespace NachaSharp

open FSharp.Data.FlatFileMeta
open FSharp.Data.FlatFileMeta.FlatRowSetup

[<AbstractClass>]
type EntryAddenda(rowInput) =
    inherit NachaRecord(rowInput, "7")

type EntryAddendaWildCard(rowInput) =
    inherit EntryAddenda(rowInput)
    override this.Setup () = 
        setup this <|
                lazy ({ 
                         columns =[
                                    MetaColumn.Make(1, this.RecordTypeCode, Format.leftPadString)
                                    MetaColumn.PlaceHolder(93)
                                  ]
                         length = 94
                     })