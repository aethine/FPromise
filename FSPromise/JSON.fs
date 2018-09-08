namespace FSPromise

open Promises
open Tools

//Whitespace is \t, \n, \r, space
module Json =
    open System

    let whitespace = ['\t'; '\n'; '\r'; ' ']

    type JsonObject = Map<string, JsonValue>
    and JsonValue =
        | JString of string
        | JNumber of float
        | JObject of JsonObject
        | JArray of JsonValue array
        | JTrue
        | JFalse
        | JNull
    

    let (|Prefix|_|) (p:string) (s:string) =
        if s.StartsWith p then Some (s.Substring p.Length)
        else None
    let (|In|_|) (lis: 'a list) (el: 'a) =
        if List.contains el lis then Some el
        else None

    let charsOf (str: string) = List.ofArray (str.ToCharArray ())
    let ofChars (chrs: char list) = List.fold (fun acc i -> acc + string i) "" chrs
    let join fn joiner coll =
        let result = Seq.fold (fun acc i -> acc + (fn i) + joiner) "" coll
        if joiner.Length > 0 then 
            result.Remove (result.Length - joiner.Length)
        else result
    let joinMap fn joiner (map: Map<'key, 'value>) =
        join fn joiner (seq {
                for kvp in map do
                    let k = kvp.Key
                    let v = kvp.Value
                    yield (k, v)
            })

    let scan until col =
        let rec loop current toGo =
            match toGo with
            | head::tail -> 
                match head with
                | _ when until head -> (List.rev current, tail)
                | cont -> loop (cont::current) tail
            | [] -> failwith "Scanned to the end"
        loop [] col
    let scanGroup opn cls col =
        let rec loop current toGo level =
            match level with
            | _ when level = 0 -> (List.rev current, toGo)
            | lvl ->
                match toGo with
                | head::tail -> 
                    match head with
                    | _ when opn head -> loop current tail (lvl + 1)
                    | _ when cls head -> loop current tail (lvl - 1)
                    | c -> loop (c::current) tail lvl
                | [] -> failwith "Scanned to the end"
        loop [] col 1

    let readString (str: string) =
        let rec loop escapeCharacter chrs =
            match chrs with
            | head::tail ->
                match head with
                | '\\' -> loop true tail
                | escape when escapeCharacter ->
                    match escape with
                    | '"' -> '"'::loop false tail
                    | '\\' -> '\\'::loop false tail
                    | '/' -> '/'::loop false tail
                    | 'b' -> '\b'::loop false tail
                    | 'f' -> '\f'::loop false tail
                    | 'n' -> '\n'::loop false tail
                    | 'r' -> '\r'::loop false tail
                    | 't' -> '\t'::loop false tail
                    | 'u' -> new NotImplementedException () |> raise
                    | error -> sprintf "Unrecognized character '%c'" error |> failwith
                | '"' -> loop false tail
                | char -> char::loop false tail
            | [] -> [] //End of loop
        charsOf str
        |> loop false 
        |> ofChars

    let writeString (str: string) =
        let rec loop chrs : string =
            match chrs with
            | head::tail ->
                match head with
                | '"' -> "\\" + loop tail
                | '\\' -> "\\\\" + loop tail
                | '/' -> "\\/" + loop tail
                | '\b' -> "\\b" + loop tail
                | '\f' -> "\\f" + loop tail
                | '\n' -> "\\n" + loop tail
                | '\r' -> "\\r" + loop tail
                | '\t' -> "\\t" + loop tail
                | c -> (string c) + loop tail
            | [] -> "" //End of loop
        sprintf "\"%s\"" (charsOf str |> loop)

    let readNumber str = Double.Parse str
    let writeNumber num =
        sprintf "%g" num

    let rec writeObject (obj: JsonObject) =
        obj
        |> joinMap (fun (k, v) -> sprintf "%s : %s" k (writeValue v)) ", \n"
        |> sprintf "{\n%s\n}"
    and writeValue value =
        match value with
        | JString s -> writeString s
        | JNumber n -> writeNumber n
        | JObject o -> writeObject o
        | JArray a -> writeArray a
        | JTrue -> "true"
        | JFalse -> "false"
        | JNull -> "null"
    and writeArray (arr: JsonValue array) =
        arr 
        |> join writeValue ", "
        |> sprintf "[%s]"
    
    let parseObjElements (str: string) =
        //When level = 0, expecting : or , or end
        //When level > 0, inside string or other object
        let loop n v level current toGo =
            match toGo with
            | head::tail -> 

            | [] -> 
                if level = 0 then Ok current
                else Error "Reached end of object while parsing"
        loop "" "" 0 [] (charsOf str)

    let rec readObject (str: string) =
        match str with
        | Prefix "{" rest -> 
            let sub = scanGroup (fun c -> c = '{') (fun c -> c = '}') (charsOf rest)
            let obj = fst sub |> ofChars
            
        | err -> Error "Json object must start with opening {"
    
    
