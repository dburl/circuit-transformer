module designGen

//used Modules
open structBEN
//Exceptionts
exception TooManyArgs of string

let orStrF strL=
        List.fold (fun a b -> "("+a+"||"+b+")")("("+(List.head strL)+")") (List.tail strL)

let andStrF strL=
    List.fold (fun a b -> "("+a+"&&"+b+")")("("+(List.head strL)+")") (List.tail strL)

let wireStrF strL=
    try
        if ((List.length strL)>1) then raise (TooManyArgs("wireStrF"))
        (List.head strL) |> (fun arg-> arg)
    with 
        |TooManyArgs(str) -> printfn "[ERR] %s" str;""

let notStrF strL=
    try
        if ((List.length strL)>1) then raise (TooManyArgs("notStrF"))
        (List.head strL) |> (fun arg-> "!"+ arg)
    with 
        |TooManyArgs(str) -> printfn "[ERR] %s" str;""

let nandStrF strL=
        notStrF [ andStrF strL] 

let norStrF strL=
    notStrF [orStrF strL]

let rhs2veril cirEl=
    try
        match cirEl.opType with
        | OR -> orStrF cirEl.inps
        | AND-> andStrF cirEl.inps
        | NOT-> notStrF cirEl.inps
        | NOR-> norStrF cirEl.inps
        | NAND-> nandStrF cirEl.inps
        | DFF-> cirEl.inps |> (fun lst-> List.head lst) 
        | WIRE-> wireStrF cirEl.inps
        | _ -> raise UnknGatOp
    with
        |UnknGatOp -> printfn "[ERR]::UnknGatOp:rhs2veril";""

//Functions that puts commas in after each str in lst except the last elemnt
let commaFunc (strL: string list)= 
    let withCommans= strL.[0..strL.Length-2] |> List.map (fun str-> str+",")
    let noComma=[List.last strL]
    List.append withCommans noComma
    