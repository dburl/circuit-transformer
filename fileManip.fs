module fileManip

//used Modules
open System
open System.Collections.Generic
open System.Linq
open System.Text
open System.IO
open System.Reflection
open System.Runtime.CompilerServices
open System.Runtime.InteropServices


//current Directory Path
let currDir= System.IO.Directory.GetCurrentDirectory()

// Original Circuit 
let origP="orig/"
let verilFolder=origP+"cir/" 
//---Triple-Modular Redundancy(TMR)
let tmrP="TMR/"

//---Time Redundancy
let ttrP="TTR/"
//Time Double Detection
let tddP="TDD/"
//Time Double Recovery - DTR
let tdrP="DTR/"
//* Dynamic transformations
    //Dynamic Triple Time Redandancy
let t3dynP="DyTR3/"
    //Dynamic Double Time  Redandancy
let t2dynP="DyTR2/"

//

// Location of Xilinx Synthesis Files


//FUNC DEFINITIONS
// File of convertion to simple .v wo voter
let outNameF filename folderPath=
    let fileNameSplit=String.split [|'/';'.'|] filename // split the name to the folder directory&cir file
    let fileOutName= List.head (List.tail (List.rev (Array.toList fileNameSplit)))// name of the file
    let filePath= folderPath+fileOutName+".v" // adding the file extension
    filePath

let genCirFiles transformType filename cirFiles =
    let topModule = List.last cirFiles // top level of circuit hierarchy 
    let cirModule= List.head cirFiles // bottom level of circuit hierarchy
    File.WriteAllLines((outNameF filename (transformType+"bottom/")),Array.ofList cirModule)
    File.WriteAllLines((outNameF filename (transformType+"top/")),Array.ofList topModule)   

//several Folders Up
let getParents (hops:int) =
    let rec parRecF num curPath =
        if num>0 then
            begin
                let newPath=System.IO.Directory.GetParent(curPath)
                parRecF (num-1) newPath.FullName
            end
        else
            curPath
    let curDir=System.IO.Directory.GetCurrentDirectory()
    parRecF hops curDir

// 
let synthFolder= 
    let symFolder= getParents 3 // go up four folders in the folder tree
    let xilinxP= symFolder+"/"+"synthRes/" //defines the synthesis folder
    xilinxP
    
//Copying the all necessary files from F# generation folter to "xilinx" synthesis folder
let cpSynth transType filename=
    let typeFolder= synthFolder+transType+"/"
    let ifExist= Directory.Exists(typeFolder)
    if (not ifExist) then 
        ignore (Directory.CreateDirectory(typeFolder))
    let fileNameSplit=String.split [|'/';'.'|] filename
    let cirFile= (List.nth (List.rev (Array.toList fileNameSplit)) 1)
    let cirFolder= typeFolder + cirFile+"/"
    let ifCirFoldExist= Directory.Exists(cirFolder)
    if not ifCirFoldExist then 
        ignore (Directory.CreateDirectory(cirFolder))
    // we get all supporting substitutional files' paths of the transformation method
    let copyFrom =currDir+"/"+transType
    let suppFull=Directory.GetFileSystemEntries(copyFrom) |> Array.toList //all supporting file in the Transformation Type folder
    let suppSplitL= (List.map (fun file-> String.split [|'/';'.'|] file) suppFull)
    let onlyFilesB= suppSplitL|> List.mapi 
                                    (fun num arr -> if ((Array.first (Array.rev arr)) = "v" ) then (true,num) else (false, num) ) 
    let onlyFileNum = (onlyFilesB |> List.filter (fun (b,num)-> b))|> List.map (fun (b, num)-> num)
    let filePath= List.map (fun num-> List.nth suppFull num) onlyFileNum // filtered pathes only to supporting files

    let filePathSplit= filePath |> List.map (fun file-> String.split [|'/'|] file) 
    let suppFileNameL= filePathSplit|> List.map (fun partA->  Array.first (Array.rev partA)) //just a name of the file to copy

    ignore (List.map2 (fun file name-> File.Copy(file,(cirFolder+name),true)) filePath suppFileNameL)//copying
    //get the circuit and top files and copy them
    let cirFileP= currDir+"/"+transType+"bottom/"+cirFile+".v"
    let topFileP= currDir+"/"+transType+"top/"+cirFile+".v"
    File.Copy(cirFileP,cirFolder+"cirLev.v",true)//copying
    File.Copy(topFileP,cirFolder+"topLev.v",true)//copying
    
// replacing in string \ to / and back
let slashRepl (str:string)=
    String.replace "\\" "/" str
let slashReplDown (str:string)=
    String.replace "/" "\\" str







    


