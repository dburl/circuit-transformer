module genTcl

//used Modules
open System
open System.Collections.Generic
open System.Linq
open System.Text
open System.IO
open System.Reflection
open System.Runtime.CompilerServices
open System.Runtime.InteropServices

open System.Diagnostics
//My modules
open fileManip 
open readReport

// string constants
let prjNewF  (path:string)= // new project creation
    let toAdd="project -new `"+path+"genPrj.prj \n"
    toAdd
let prjSaveF  (path:string)= // project saving
    let toAdd="project -save "+path+"genPrj.prj \n"
    toAdd
let veril2PrjF  (fnam:string)= // adding HDL files to project
    let toAdd="add_file -verilog "+fnam+" \n"
    toAdd
//OPTIONS SETTING
let fsmCompF     (on:bool) =  
    let toAdd="set_option -symbolic_fsm_compiler "+(if on then "1" else "0")+" \n"
    toAdd
let resSharF     (on:bool) = 
    let toAdd="set_option -resource_sharing "+(if on then "1" else "0")+" \n"
    toAdd
let pipelinF     (on:bool) = 
    let toAdd="set_option -pipe "+(if on then "1" else "0")+" \n"
    toAdd
let setTechnF  (name:string)= 
    let toAdd="set_option -technology "+name+" \n"
    toAdd
    
let runSynthF  = // Run the Project
    let toAdd="project -run synthesis"+" \n"
    toAdd


// FUNCTION DEFINITION
//create tcl script to create and sunth a project
let genTclF transType filename=
    let typeFolder= synthFolder+transType
    let fileNameSplit=String.split [|'/';'.'|] filename
    let cirFile= (List.nth (List.rev (Array.toList fileNameSplit)) 1)
    let prjFolder= typeFolder + cirFile+"/"
    let prjFoldStr= slashRepl prjFolder
    let allVerilFiles= IO.Directory.GetFiles(prjFolder)|> Array.toList
    let cirFileP=prjFolder+"run.tcl"
    let stmWr=File.CreateText(cirFileP) //file creation
    // project instant and property setting
    let optimIf = false
    stmWr.WriteLine( prjNewF prjFoldStr) 
    stmWr.WriteLine( prjSaveF prjFoldStr)
    stmWr.WriteLine( fsmCompF optimIf)
    stmWr.WriteLine( resSharF optimIf)
    stmWr.WriteLine( pipelinF optimIf)
    stmWr.WriteLine( setTechnF "ProASIC3") //"Virtex6")            
    // HDL files inclusion 2 prj
    let verilFileNL= allVerilFiles |> List.filter (fun nameStr -> String.endsWith ".v" nameStr) |> List.map (fun l-> slashRepl l)
    let topFile= verilFileNL|> List.filter (fun path-> String.endsWith "topLev.v" path) |> List.head // only one topfile can exist
    let restFiles= verilFileNL|> List.filter (fun path-> path<>topFile)
    let namesL=  List.append  restFiles [topFile]

    List.iter (fun name ->  stmWr.WriteLine( veril2PrjF name ))namesL
    //save onse gain
    stmWr.WriteLine( prjSaveF prjFoldStr)
    // run synth line
    stmWr.WriteLine( runSynthF)
    stmWr.Flush()
    stmWr.Close()// close file
    prjFoldStr // path of the project

// Run synch cmd command
exception SynExeErr of string 
let exeCmd (tclP:string)=
    try
        let proc= new Diagnostics.Process()                 
      //property set
        proc.StartInfo.WindowStyle <- ProcessWindowStyle.Hidden
        let workDir= (slashReplDown tclP)
        proc.StartInfo.WorkingDirectory <- workDir// working directory
        let toRunStr="go.bat" // 
        proc.StartInfo.FileName <- toRunStr  // script to run
            
        let st= proc.Start()//start
        proc.WaitForExit()// wait till the end
    with
        _ ->   raise <| SynExeErr ("[ERR]: synopsys synthesis process"+ tclP)

// generate 1-line .bat file to start tcl script
let genBatCall transType filename=
    let typeFolder= synthFolder+transType
    let fileNameSplit=String.split [|'/';'.'|] filename
    let cirFile= (List.nth (List.rev (Array.toList fileNameSplit)) 1)
    let prjFolder= typeFolder + cirFile+"/"
    let prjFoldStr= slashRepl prjFolder
    let allVerilFiles= IO.Directory.GetFiles(prjFolder)|> Array.toList
    let cirFileP=prjFolder+"go.bat"
    let stmWr=File.CreateText(cirFileP) //file creation
    stmWr.WriteLine "C:\\Synopsys\\fpga_E2010091\\bin\\synplify.exe -batch run.tcl"
    stmWr.Flush()
    stmWr.Close()// close file


//Writing first to F# folder and copying from it to Synopsys syntheis sub-folder  + syntheses and .report extraction
let wrCirDesign transformType filename cirFiles=
    genCirFiles transformType filename cirFiles //do the tansformation in EAA project
    cpSynth transformType filename  // cp the circuit to synthesis folder
    genBatCall transformType filename// .bat call to run tcl through synplify
    let tclP=genTclF transformType filename // tcl file generation - returns path to it
    exeCmd (tclP)//"run.tcl") // runs the tcl file through .bat (not the best implem.but working)
    readSRR transformType filename // genarate .report file for easy seek and read


