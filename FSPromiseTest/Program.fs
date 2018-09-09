open System
open FSPromise.Promises
open FSPromise.Tools
open FSPromise.Json

[<EntryPoint>]
let main _argv =
    
    ////Fetch example
    //let fileUrl = "https://en.wikipedia.org/wiki/F_Sharp_(programming_language)"
    //let p = fetch fileUrl
    //        |> timeout 1000
    //        |> catch (fun e -> printfn "%s" e.Message)
    //        |> thenDo (fun res -> printfn "%A" res)
    //p.Wait () |> ignore

    //Json examples
    let writeValPromise = writeObject |>> promiseDo
    
    
    use written = JsonObject [
                                ("A", JNull)
                                ("B", JNumber 100.0)
                                ("C", JString "Hewwo?? \\\"")
                                ("D", JObject (JsonObject [("DA", JNull); ("DB", JNull)]))
                             ] 
                  |> writeValPromise
    match written.Wait () with
    | Ok value -> printfn "%s" value
    | Error e -> printfn "ERROR: %s" (string e)

    Console.ReadKey true |> ignore
    0 // return an integer exit code
