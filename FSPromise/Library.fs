namespace FSPromise

open System
open System.Threading.Tasks

type Promise<'T> (fn: ('T -> unit) -> (exn -> unit) -> unit) =

    let mutable result: Result<'T, exn> option = None
    let task = (fun () -> 
                    (fun e ->
                        match result with
                        | Some _ -> ()
                        | None -> result <- Some (Error e)
                    ) |> fn (fun t ->
                                match result with
                                | Some _ -> ()
                                | None -> result <- Some (Ok t)
                            )
               ) |> Task.Run
    
    member _this.Result with get () = result
    member _this.Wait () = 
        task.Wait ()
        result.Value

    interface IDisposable with 
        member _this.Dispose () = task.Dispose ()

    member this.Then (onResolve: 'T -> unit) = 
        new Promise<'T>(fun resolve reject -> 
                        this.Wait () |> ignore
                        match result.Value with
                        | Ok ok -> 
                            onResolve ok
                            resolve ok
                        | Error error -> reject error)
    member this.Catch (onReject: exn -> unit) =
        new Promise<'T>(fun resolve reject -> 
                        this.Wait () |> ignore
                        match result.Value with
                        | Ok ok -> resolve ok
                        | Error error ->
                            onReject error
                            reject error)
    member this.Finally (onFinally: unit -> unit) =
        new Promise<'T>(fun resolve reject -> 
                        this.Wait () |> ignore
                        match result.Value with
                        | Ok ok -> resolve ok
                        | Error error -> reject error
                        onFinally ())

    member this.Then (onResolve: 'T -> 'a) =
        new Promise<'a>(fun resolve reject -> 
                        this.Wait () |> ignore
                        match result.Value with
                        | Ok ok -> resolve (onResolve ok)
                        | Error error -> reject error)
    member this.Then (onResolve: 'T -> unit, onReject: exn -> unit) =
        (this.Then onResolve).Catch onReject
    member this.Then (onResolve: 'T -> 'a, onReject: exn -> unit) =
        (this.Then onResolve).Catch onReject


module Promises =
    open System.Threading

    let promise (fn: ('a -> unit) -> (exn -> unit) -> unit) =
        new Promise<'a>(fn)
    let promiseCatch (fn: unit -> 'a) =
        promise (fun resolve reject ->
            try resolve (fn ())
            with e -> reject e
        )
    let promiseTask (tsk: Task<'a>) =
        promise (fun resolve reject ->
                        tsk.Wait ()

                        match tsk.Exception with
                        | null -> resolve tsk.Result
                        | e -> reject e
                   ) 
    let await (promise: Promise<_>) = 
        match promise.Wait () with
        | Ok r -> r
        | Error j -> raise j

    let waitAll promises = List.map (fun (p: Promise<_>) -> p.Wait ()) promises
    let all (promises: Promise<_> list) =
        promise (fun resolve reject -> 
            let mutable list = []
            for p in promises do
                match p.Wait () with
                | Ok r -> list <- r::list
                | Error j -> reject j
            resolve (List.rev list)
        )
    let race promises = 
        (fun resolve reject ->
            promises
            |> List.map (fun (p: Promise<_>) -> 
                p.Then(fun t -> resolve t).Catch(fun e -> reject e)
                ) |> ignore
        ) |> promise
    let timeout (ms: int) (prom: Promise<_>) =
        promise (fun resolve reject -> 
            promise (fun resolve' _reject' -> 
                Thread.Sleep ms
                reject (exn (sprintf "Promise did not complete within %i milliseconds" ms))
                resolve' ()
            ) |> ignore
            match prom.Wait () with
            | Ok r -> resolve r
            | Error j -> reject j
        )

    let thenDo (f: 'a -> unit) (p: Promise<'a>) = p.Then f
    let thenRet (f: 'a -> 'b) (p: Promise<'a>) = p.Then f
    let catch f (p: Promise<_>) = p.Catch f

    let (!!>) = thenDo
    let (!>>) = thenRet