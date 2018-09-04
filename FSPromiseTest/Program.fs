open System
open FSPromise.PTools
open FSPromise.Promises

[<EntryPoint>]
let main _argv =
    let fileUrl = "https://en.wikipedia.org/wiki/F_Sharp_(programming_language)#/media/File:Fsharp,_Logomark,_October_2014.svg"
    let p = fetchData fileUrl
            |> timeout 1000
            |> catch (fun e -> printfn "%s" e.Message)
            |> thenDo (fun res -> printfn "%A" res)
    p.Wait () |> ignore

    Console.ReadKey true |> ignore
    0 // return an integer exit code
