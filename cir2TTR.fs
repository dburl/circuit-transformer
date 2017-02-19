module cir2TTR

//used Modules
open System
open System.Collections.Generic
open System.Linq
open System.Text
open System.IO
//mine
open structBEN
open designGen

// Renaming function: from FF name to the name of FF-block
let reg2rBlockRenF str=
        "ffBl_"+str

// function to voter insertion to the circuit cirD after memory cells voterL
let cir2ttrF (cirD: cirDicT) voterL =    
    let cirOrigA=cirD.ToArray() //from dictionary to array
    
    let inpA= cirOrigA|> Array.filter (fun el->el.Value.sType=INP)  //orig ins struct A.
    let outA= cirOrigA|> Array.filter (fun el->el.Value.sType=OUT) // orig outs stract A.
    
    let inpStrA= inpA |> Array.map (fun el-> el.Key) // original ins names A.
    let outStrA= outA |> Array.map (fun el-> el.Key) //original outs names A.

    
    //CIRCUIT FILE GENERATION

    let inpStrL=  (List.append ["clk"] (Array.toList inpStrA))|>
                                    List.map  (fun inpName->  "\t input "+inpName) //inputs declaration
    let outStrL=  (Array.toList outStrA) |> 
                                    List.map  (fun outName->"\t output "+outName) //outputs declaration
    let oldIOLwoCtr= List.append inpStrL outStrL  //inps+ outs
    let oldIOL= List.append  (List.map (fun str->str+",") oldIOLwoCtr) ["\t input[1:0] ctr"]//inps+outs+ heart-beat control

    // head part of the file
    let modEmp= []                                          //empty module
    let modTop= modEmp |> List.append ["module Design("]    // module delaration add
    let modInp= List.append modTop oldIOL            //adding normal inputs and output
    let redModFin=  List.append modInp [");"]   //final module interface -Part 1.1

    // MAIN part after interface

    //function that replaces the FF with FF-block (with and w/o voter depending on the voterL)
    let reg2RegBlock regName= 
        let ifVot= List.exists (fun vot-> vot=regName) voterL
        let elCell= cirD.[regName]
        let dffRHS= elCell.inps
        let singleInp= List.head dffRHS
        let result=
                match ifVot with 
                |true -> ("ff3TV "+(reg2rBlockRenF regName)+"(clk, "+singleInp.ToString()+", "+ regName + ", ctr);" )//if FF-block with voter
                |false -> ("ff3T "+(reg2rBlockRenF regName)+"(clk, "+singleInp.ToString()+", "+ regName + ", ctr);") //iff FF-block w/o voter
        result
   

    let memCellOrigA= cirOrigA|> Array.filter (fun el-> el.Value.opType=DFF) //original memory cell names
    let memBlockStrL = (Array.toList memCellOrigA) |> List.map (fun mem -> "\t"+reg2RegBlock mem.Key) //mem cells  are replaced by mem Blocks
    
    // it's necessary to declare  the FF-block IO-lines as wires
    let wiresMemStrL = (Array.toList memCellOrigA) |> List.map (fun wirName-> "\t wire "+wirName.Key+" ;") // wires declarations for memory blocks IO
    //other wires
    let wiresA  = cirOrigA|> Array.filter (fun el-> ((el.Value.sType=MIDLL)&&(el.Value.opType<>DFF))|| (el.Value.opType=WIRE))
    let wiresStrL = (Array.toList wiresA) |> List.map (fun wirName-> "\t wire "+wirName.Key+" ;") //just wires declaration
    // signla assignments
    let add2lhs= outStrA//[added]
    let add2rhs= outA |> Array.map (fun str-> List.head str.Value.inps)//[added]

    let wirRhsA = wiresA |> Array.map (fun wirEl-> rhs2veril wirEl.Value)//RHS of the assignments for signals
    let wireNameA= wiresA |>Array.map (fun el-> el.Key) // LHS of the assignments for signals
    
    let wNameA= Array.append wireNameA add2lhs //[added]
    let wRhsA=Array.append wirRhsA add2rhs //[added]

    let assignSignStrL =  (Array.map2 (fun lhs rhs  -> ("\t assign "+lhs+" = "+rhs+";")) wNameA wRhsA) |> 
                                                                    Array.toList  //combinatorial assigning for signals
   //process Definition- full strings list
    let cirFileL = List.fold List.append [] ( [redModFin;  wiresMemStrL;wiresStrL; assignSignStrL; memBlockStrL;["endmodule"]])
    
    // TOP FILE GENERATION

    //header of TTR module with ctrBlock and circuit block instantiations
    let tmrInterd= modEmp |> List.append ["module topTTMR("]
    let commas= List.dropLast oldIOLwoCtr |> List.map (fun str-> str+",")
    let modTop= List.append tmrInterd  (List.append commas [List.last oldIOLwoCtr])//adding normal inputs and output
    let modFin= List.append modTop [");"] //final TOP TMR module interface

    //generate wires
    let ctrInterfL= ["\t wire[1:0] ctr;"] // heart-beat wire from Beat-Block to circuit
    //generate input sequence

    let oldSeqInA= inpStrA //clk is skipped at the beginning-> add later
    let seqA= Array.append oldSeqInA outStrA
    
    //instantiate redundant blocks - function
    let instansRedModStr interfSeqStr=
        let clkInterf= (Array.append [|"clk"|] interfSeqStr) |> Array.toList // interface with clk
        let interf = List.append  clkInterf ["ctr"]
        let inrfStr=  List.fold (fun was add-> was+", "+add) (List.head interf) ( List.tail interf) 
        inrfStr

    let strStartF num= "\t Design redModule"+num.ToString()+ "(" // "Design redModule0( "
    let ttmrStrL= [strStartF 0+instansRedModStr seqA+" );"] //interface of Design block

    //instantiate beat-clock control block
    let ctrBlockStrL= ["\t ctr3Tmr ctrBlock(clk, RESET_G, ctr);"]   

    let fileTopL= List.fold List.append [] [modFin;ctrInterfL;ctrBlockStrL;ttmrStrL;["endmodule"]]
    //
    [cirFileL; fileTopL]



