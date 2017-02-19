module structBEN
//used Modules
open parser_itcBEN
open System.Collections.Generic
//Type of Signals
type sigT =
    |INP //input
    |OUT //output
    |MIDLL//no input, no output-> intermediate signal
//Type of the operation
type opT=
    |OR
    |AND
    |NAND
    |NOR
    |NOT
    |DFF
    |WIRE
    |BORDER//means that it's either input or output

exception UnknGatOp // Exception of unknown gate operation
//Func. string to type of the operated according to the enumerated type
let str2opT str=
    try
        match str with
        |"OR"-> OR
        |"AND"->AND
        |"NAND"->NAND
        |"NOR"->NOR
        |"NOT"->NOT
        |"DFFX"->DFF
        |"DFF"->DFF
        |"BORDER"->BORDER
        |_-> raise UnknGatOp
    with 
        |UnknGatOp-> printfn "[ERR]::UnknGatOp:str2opT" ; BORDER//"File contains unknown gate operation::"+str

//Entry for the dictionary
type sigDisc=
    {sType:sigT;
    opType:opT;
    inps:string list;}
//Dictionary for full circuit description
type cirDicT= Dictionary<string, sigDisc>

exception CIRmanip
//Function to read the netlist and create internal dictionaey representation
let cirBuild filename=
    let (gate_recL, (inpL,outL))=benITC_gRECL filename
    let cirDic= new cirDicT()
    List.iteri (fun num inpNam-> cirDic.Add(inpNam, {sType=INP ; opType=BORDER; inps=[];})) inpL
    List.iter (fun gateR-> cirDic.Add(gateR.name,{sType=MIDLL;opType=str2opT gateR.op;inps= gateR.ins;})) gate_recL
    //Cleaning of duplicates between interDic and cirDic
    let outRename num str=
        "out"+num.ToString()+"_"+str
    //let outNumPL= List.mapi (fun num out-> (outRename num out,num) ) outL List.iter
    //add to cir renamed outs
    List.iteri (fun num out -> cirDic.Add(outRename num out, {sType=OUT ;opType=BORDER;inps=[out];})) outL
    cirDic

