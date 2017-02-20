// Authour: Burlyaev (burlyaev.dmitry@gmail.com)
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
open cir2tmr
open cir2TTR
open cir2TDDet
open cir2TDR
// Tcl & Scripting
open genTcl
open readReport
open wrSynthRep
open allCirSynth


open System.Runtime.InteropServices
open  System.Reflection
open  System.Runtime.CompilerServices
open  System.Runtime.InteropServices

open System.Diagnostics // cmd exe

[<EntryPoint>]
let main argv = 
    let benchmarksNumber=4;
    itcSynth benchmarksNumber [["~~FTMR"]] // transform, save, synthesize
                    // "~~FTMR"- voters everywhere (except primary outs)
                    // Romment this transformation/synthesis part out- to directly visualize the last synthesis results

    synth2Excel synthFolder// export to Excel

  
    printfn "Program Finished"
    printfn "%A" argv
    0 // return an integer exit code
    