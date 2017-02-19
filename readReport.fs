module readReport

//used Modules
open System
open System.Collections.Generic
open System.Linq
open System.Text
open System.IO
open System.Reflection
open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open System.Text.RegularExpressions
open Microsoft.Office.Interop.Excel // to export to Excel
open System.Diagnostics
//My modules
open fileManip 
open fileManip
open parser_itcBEN

// Str Constants -- Xilinx
let hwStr= "Total  LUTs:"
let frqStr= "Required time"
//Str Contants
let hwStrActel="Core Cells"
let frqStrActel= "Total path delay"

exception SynthRepErr of string 
//func read SRR report and writes .report file
let readSRR transType filename=
    //transformation name extraction
    let transSplit=String.split [|'/'|] transType
    let transName=  transSplit |> (fun arr-> if (Array.length arr =2) then arr.[0] else arr.[1])
    // understanding pathes
    let typeFolder= synthFolder+transType
    let fileNameSplit=String.split [|'/';'.'|] filename
    let cirFile= (List.nth (List.rev (Array.toList fileNameSplit)) 1)
    let prjFolder= typeFolder + cirFile+"/"
    let prjFoldStr= slashRepl prjFolder
    let synthFolder = prjFoldStr + "rev_1/"
    let allFilesL= Directory.GetFiles synthFolder |> Array.toList
    let reportFile= allFilesL|> List.filter (fun fName-> String.endsWith "topLev.srr" fName)
    if List.isEmpty reportFile then raise <| 
                                    SynthRepErr ("[ERR]: synth.report:"+prjFoldStr)
    //file read
    let strL=read_file (List.head reportFile)
    //function to find string inclusion in other string
    let checkInclF (insideStr:string) (where:string)=
        let regExp = "(.*)"+insideStr+"(.*)"
        let matchRes=Regex.Match (where,regExp)
        matchRes.Success
    // hw res extraction - Xilinx
//    let hwResStr=List.find (fun str-> checkInclF hwStr str) strL
//    let frqResStr=List.find (fun str-> checkInclF frqStr str) strL
//
//    let hwRes=String.split [|':';'('|] hwResStr |> (fun lst-> Array.get lst 1)
//    let frqRes=String.split [|':'|] frqResStr |> (fun lst-> Array.get lst 1)

    // hw res extraction - Actel
    let hwResStrActel=List.find (fun str-> checkInclF hwStrActel str) strL
    let frqResStrActel=List.find (fun str-> checkInclF frqStrActel str) strL
    let hwRes=String.split [|':';'o'|] hwResStrActel |> (fun lst-> Array.get lst 2)
    let frqResDelay=String.split [|'f';'i'|] frqResStrActel |> (fun lst-> Array.get lst 3)
    let frqRes= (float frqResDelay)|> (fun x-> (1000.0/x)) |> (fun x-> x.ToString())
     
    //Write result to file ; sequence: circ, trans, HW, freq
    let outFile= prjFoldStr+"result.report"
    File.WriteAllLines(outFile,Array.ofList [cirFile;transName;hwRes;frqRes])

// find all synthesis report in the directroy and all subdirectories and returns the list of their addresses
let collectSynthRep (folderP:string)=
    let rec recSearch (dirL:string list) (accFileL:string list)=
        let filesHereL= dirL |> List.map (fun dir-> Directory.GetFiles(dir) |> Array.toList) |> List.concat
        let reportHereL =filesHereL|> List.filter (fun str-> String.endsWith "result.report" str)
        let newAccFL= List.append accFileL reportHereL
        let dirHereL = (dirL |> List.map (fun dir-> Directory.GetDirectories (dir)|> Array.toList) ) |> List.concat
        if (List.length dirHereL)>0 then 
            recSearch dirHereL newAccFL
        else
            newAccFL
    recSearch [folderP] []

// Extracts from the list of synth reports the inf as a record ro further give to Excel exporter    
type synth_T= // type of the report export
    {cirName: string; //circuti name
     transT: string; // type of FT transformation
     hwUse: float; // hw utilization [MHz]
     frq: float; //max frequency
    }

exception SynthRepRead of string
// function that parses the synthesis report file .report and converts it to the describing record synth_T
let synRepParse (path:string)=
    let strL= read_file path
    try
    {cirName= List.nth strL 0;
     transT = List.nth strL 1;
     hwUse  = float (List.nth strL 2);
     frq  = float (List.nth strL 3);
     }
    with 
    _ -> raise <| SynthRepRead ("[ERR]:synthesis report reading to record::"+path)




