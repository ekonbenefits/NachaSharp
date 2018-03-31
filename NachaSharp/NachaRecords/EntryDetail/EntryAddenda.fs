namespace NachaSharp

open FSharp.Data.FlatFileMeta

[<AbstractClass>]
type EntryAddenda(rowInput) =
    inherit NachaRecord(rowInput, "7")

type EntryAddendaWildCard(rowInput) =
    inherit EntryAddenda(rowInput)
    override this.Setup () = 
        FlatRowProvider.setup this <|
                lazy ({ 
                         columns =[
                                    MetaColumn.Make(1, this.RecordTypeCode, Format.leftPadString)
                                    MetaColumn.PlaceHolder(93)
                                  ]
                         length = 94
                     })