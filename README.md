# FSPromise
Promise-CS port for F#.


# Here's a quick guide:

To create a promise from scratch, you only need this code:

```f#
open FSPromise
open System.Threading

let delayedPromise = new Promise<_>(fun resolve _reject ->
                                     Thread.Sleep 1000
                                     resolve "Hello, world!"
                                   )
```

The promise will start executing immediately. The promise's result will now be `"Hello, world!"`, so we can then do something like this:

```c#
let delayedPromiseFinal = delayedPromise.Then(fun res -> printfn "%s" res)
```

In F#, the `unit` type can fit into type parameters, so a "Typeless" and "Typed" promise distinction is not needed.

There are alternatives to the C#-like `.Then`, `.Catch`, and even `new Promise()` in F#, in the `FSPromise.Promises` module:

```f#
open FSPromise.Promises
let delayedPromise = (
           promise (fun resolve _reject ->
               Thread.Sleep 1000
               resolve "Hello, world!"
               )
               |> thenDo (fun res -> printfn "%s" res) //like .Then()
           )
```

Finally, if you want to handle errors within your promise:

```f#
open System.Net.NetworkInformation //For Ping and PingReply
open FSPromise.Promises

let pingPromise = (
        promise (fun resolve reject -> 
            try
                let pinger = new Ping()
                let reply = pinger.Send "https://fsharp.org/"
                resolve reply
            with e -> reject e //Reject using the exn type like this
        )
        |> thenDo (fun reply -> printfn "Success! (Roundtrip time: %i ms)" reply.RoundtripTime)
        |> catch (fun e -> printfn "Error! %s" (string e))
        |> doFinally (fun () -> printfn "Done!")
    )
```

- In `doThen`, `reply` will be the same value as the one that was used to call `resolve()`.
- In `catch`, `e` will be the exception caught by the `catch` block, and used to call `reject()`.
- `doFinally` will be called if either `resolve()` or `reject()` was called.

Of course, `FSPromise.Tools` provides tools so that you don't have to create your own promises:

```c#
open FSPromise.Promises
open FSPromise.Tools 

let fetchPromise = fetch "https://github.com"
    |> !!> (fun text -> printfn "Contents of GitHub's main page: \n%s" text)
    |> catch (fun e -> printfn "An exception occurred! \n%s" e)
```
`!!>` is the same as `doThen`.
