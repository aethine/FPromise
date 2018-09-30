namespace FSPromise

module Promiser =
    open Promises
    

    type Promiser<'T>() =
        let mutable hasFinished : Result<'T, exn> option = None

        member _this.Then fn =
            promise (fun resolve reject ->
                let rec check () =
                    match hasFinished with
                    | None -> check () 
                    | Some result ->
                        fn result
                        match result with
                        | Ok ok -> resolve ok
                        | Error e -> reject e
                check ()
            )
        member _this.Resolve v =
            hasFinished <- Some (Ok v)
        member _this.Reject e =
            hasFinished <- Some (Error e)