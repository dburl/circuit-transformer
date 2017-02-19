module allCirSynth

open System
open System.Collections.Generic
open System.Linq
open System.Text
open System.IO
//My libs
open parser_itcBEN
open structBEN
open designGen
open cir2verilog
open fileManip
// Cir Trans
    //hw-red
open cir2tmr
open cir2tmrS1
open cir2tmrS2
    //time-red
open cir2TTR
open cir2TDDet
open cir2TDR
    //dynam
open cir3Dyn
open cir2Dyn
// Tcl & Scripting
open genTcl
open readReport
open wrSynthRep



let cirSynth (voterComm:string list) (cirP: string)=
    let filename=cirP                           // E.g.:"itc/b01_opt_r.bench"
    let cirD= cirBuild filename //|> outsNotDFF   //Build the cir struction (Dictionary)
    let allMemL=   // all mem cells
        cirD.ToArray()|> Array.filter(fun el-> el.Value.opType=DFF)|>
                         Array.map (fun el->el.Key) |> List.ofArray
    //Voter Insertion in TMR 
    let voterL= 
            match voterComm with
            |["~FTMR"]-> allMemL
            |_->voterComm 
   //Original Circuit
    let origFiles= cir2verilog cirD//convertion to Verilog

   //--- TRANSMORMATIONS  - doing them
       //HW-redundancy
    let tmrFiles= cir2tmrF cirD voterL // TMR transformation
    
        // Time-redundancy
    let ttrFiles= cir2ttrF cirD voterL // Triple Time Redundancy (TTR, see thesis) transformation
    
    let tdrFiles= cir2tdrF cirD voterL //  Double Time  Redundancy (DTR, see thesis) transformation [
        //Dynamic transformations
    let t3dynFiles= cir3dynF cirD voterL // Dynamic Triple Time Redundacny (T3Dyn) transformation 
    let t2dynFiles= cir2dynF cirD voterL // Dynamic Double Time Redundancy (T2Dyn) transformation 
                                      
   //--- WRITE transfromed circuits to Files
    wrCirDesign origP filename origFiles // write Original modules to files
        //TMR transformations
    wrCirDesign tmrP filename tmrFiles  // Write TMR modules to files
       // Time transformations
    wrCirDesign ttrP filename ttrFiles // Write TTR modules to files
    
    wrCirDesign tdrP filename tdrFiles //Write DTR modules to files
        
        //Dynamic transformations
    wrCirDesign t3dynP filename t3dynFiles //Write DyTR3 modules to files
    wrCirDesign t2dynP filename t2dynFiles //Write DyTR2 modules to files
  

// for all memory cells in ITC'99 benchmark
let itcSynth (vLL:string list list) =
    let itcFolder= (getParents 3)+"/itc/" // absolut address of the folder with original non-transfromaed circuits
    let allCirP= Directory.GetFiles(itcFolder) |>Array.toList // path to all found circuit benchmarks
    let allNum= List.length allCirP // number of found original circuits
    let givenVLL= // checks if we have to intro voters after each mem.cell in ALL found circuits (check for argument ~~FTMR) 
        match vLL with 
        |[["~~FTMR"]]-> List.map (fun _->["~FTMR"]) [1..allNum] // if yes, then we say say for each circuit separately to intro all voters (~FTMR)
        |_-> vLL // otherwise we introduce according to the user specification in list of list argument [[]]
    let emptyVN=allNum-(List.length givenVLL) // we just make sure that we don't introduce any voters to the circuits if user didn't specify it
    let voterLL= List.append givenVLL (List.map(fun _->[]) [1..emptyVN]) // see line above
    List.iter2 (fun path voters-> cirSynth voters path) allCirP voterLL // for all circuits and correponding list of voters to intro, we call cirSynch (defined above)
    
