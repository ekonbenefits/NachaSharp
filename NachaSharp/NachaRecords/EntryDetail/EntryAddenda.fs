namespace NachaSharp

open FSharp.Data.FlatFileMeta

[<AbstractClass>]
type EntryAddenda(rowInput) =
    inherit NachaRecord(rowInput, "7")

type EntryAddendaWildCard(rowInput) =
    inherit EntryAddenda(rowInput)
    override this.Setup () = setupMetaFor this {
            columns      1 this.RecordTypeCode Format.leftPadString
            placeholder 93
            checkLength 94
        }
     