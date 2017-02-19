module tmrTop
//lib
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
open cir2tmr

 //Choose the circuit from ITC
let filename="itc/b09_opt_r.bench"
    //Build the cir struction (Dictionary)
let cirD= cirBuild filename// |> outsNotDFF
    
// Convert cir to Verilog program
let verilogL= cir2verilog cirD//convertion to Verilog
// write Verilog to file
//File.WriteAllLines((outNameF filename verilFolder),verilogL )

//Voter Insertion in TMR
let allMemL= cirD.ToArray()|> Array.filter(fun el-> el.Value.opType=DFF)|>
                            Array.map (fun el->el.Key) |> List.ofArray
       
let voterL= //allMemL
            //["STATO_REG_1_";
            //"STATO_REG_0_"]
                ["D_OUT_REG_7_";
                "D_OUT_REG_6_";
                "D_OUT_REG_5_";
                "D_OUT_REG_4_";
                "D_OUT_REG_3_";
                "D_OUT_REG_2_";
                "D_OUT_REG_1_";
                "D_OUT_REG_0_";
                "OLD_REG_7_";
                "OLD_REG_6_";
                "OLD_REG_5_";
                "OLD_REG_4_";
                "OLD_REG_3_";
                "OLD_REG_2_";
                "OLD_REG_1_";
                "OLD_REG_0_"; 
                "STATO_REG_1_";
                "STATO_REG_0_"]
//
let tmrFiles= cir2tmrF cirD voterL
// Write TMR modules to files
//File.WriteAllLines((outNameF filename tmrFolder),List.head tmrFiles )
//File.WriteAllLines((outNameF filename redModFolder),List.last tmrFiles)