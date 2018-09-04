namespace FSPromise

open Promises

module PTools =
    open System.Net
    open System.IO
    open System.Threading
    open System.Diagnostics
    

    let fetch (uri: string) = 
        promise (fun resolve reject -> 
            try 
                let request = WebRequest.Create uri
                let response = request.GetResponse ()
                use stream = response.GetResponseStream ()
                use reader = new StreamReader(stream)
                resolve (reader.ReadToEnd ())
            with exn -> reject exn
        )
    let fetchData (uri: string) =
        promise (fun resolve reject ->
            try
                use client = new WebClient ()
                resolve (client.DownloadData uri)
            with e -> reject e
        )
    let fetchTo (uriFrom: string) (uriTo: string) =
        promise (fun resolve reject ->
            try
                use client = new WebClient ()
                client.DownloadFile(uriFrom, uriTo)
                resolve ()
            with e -> reject e
        )
    let delay fn (ms: int) =
        promise (fun resolve _reject ->
            Thread.Sleep ms
            resolve (fn ())
        )
    let time fn =
        promise (fun resolve _reject ->
            let watch = new Stopwatch()
            watch.Start ()
            let result = fn ()
            watch.Stop ()
            resolve (result, watch.Elapsed)
        )
