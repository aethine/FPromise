open System
open FSPromise.Promises
open FSPromise.Tools
open System.Net.NetworkInformation
open System.Threading

[<EntryPoint>]
let main _argv =
    let delayedPromise = (promise (fun resolve _reject ->
                               Thread.Sleep 1000
                               resolve "Hello, world!"
                             )
                        |> thenDo (fun res -> printfn "%s" res) //like .Then()
                     )

    Console.ReadKey true |> ignore
    0 // return an integer exit code
