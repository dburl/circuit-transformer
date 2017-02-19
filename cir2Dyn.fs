module cir2Dyn

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

// function to insert Memory Blocks instead of original Memory cells
let cir2dynF (cirD: cirDicT) voterL =    
    let cirOrigA=cirD.ToArray() //from dictionary to array
    
    let inpA= cirOrigA|> Array.filter (fun el->el.Value.sType=INP)  //orig ins struct A.
    let outA= cirOrigA|> Array.filter (fun el->el.Value.sType=OUT) // orig outs struct A.
    
    let inpStrA= inpA |> Array.map (fun el-> el.Key) // original ins names A.
    let outStrA= outA |> Array.map (fun el-> el.Key) //original outs names A.

    
    //CIRCUIT FILE GENERATION

    let inpStrL=  (List.append ["clk"] (Array.toList inpStrA))|>
                                    List.map  (fun inpName->  "\t input "+inpName) //inputs declaration
    let outStrL=  (Array.toList outStrA) |> 
                                    List.map  (fun outName->"\t output "+outName) //outputs declaration
    let oldIOLwoCtr= List.append inpStrL outStrL  //inps+ outs
    let oldIOL= List.append  (List.map (fun str->str+",") oldIOLwoCtr) ["\t input modeS,"; //inps+outs+ heart-beat control
                                                                      //  "\t input fetchA,"
                                                                        "\t output fail"]
    // head part of the file
    let modEmp= []                                          //empty module
    let modTop= modEmp |> List.append ["module Design("]    // module delaration add
    let modInp= List.append modTop oldIOL            //adding normal inputs and output
    let redModFin=  List.append modInp [");"]   //final module interface -Part 1.1

    // MAIN part after interface

    //function that replaces the FF with FF-block (with and w/o voter depending on the voterL)
    let reg2RegBlock (number:int) regName= 
        let ifVot= List.exists (fun vot-> vot=regName) voterL
        let elCell= cirD.[regName]
        let dffRHS= elCell.inps
        let singleInp= List.head dffRHS
        let result=
                match ifVot with 
                |true -> ("ff2DB "+(reg2rBlockRenF regName)+"(clk, "+singleInp.ToString()+", "+ regName + ", modeS,  ErrDet["+number.ToString()+"]);" )//if FF-block with compare --[TODO]compare optim.
                |false ->("ff2DB "+(reg2rBlockRenF regName)+"(clk, "+singleInp.ToString()+", "+ regName + ", modeS,  ErrDet["+number.ToString()+"]);") //iff FF-block w/o voter--[TODO]compare optim.
        result
 
    let memCellOrigA= cirOrigA|> Array.filter (fun el-> el.Value.opType=DFF) //original memory cell names
    let memBlockStrL = (Array.toList memCellOrigA) |> List.mapi (fun num mem -> "\t"+reg2RegBlock num mem.Key) //mem cells  are replaced by mem Blocks
    let dffNum= List.length memBlockStrL
    let topRange= dffNum-1
    
    // it's necessary to declare  the FF-block IO-lines as wires plus wires for the ERR_detection sugnals from all FF-blocks
    let wireErr= "\t wire ["+topRange.ToString()+":0] ErrDet;"
    let wiresMemStrL = (Array.toList memCellOrigA) |> List.map (fun mem-> "\t wire "+mem.Key+" ;") // wires declarations for memory blocks IO

    //other wires
    let wiresA  = cirOrigA|> Array.filter (fun el-> ((el.Value.sType=MIDLL)&&(el.Value.opType<>DFF))|| (el.Value.opType=WIRE))
    let wiresStrL = (Array.toList wiresA) |> List.map (fun wirName-> "\t wire "+wirName.Key+" ;") //just wires declaration
    // signal assignments
    let add2lhs= outStrA//[added]
    let add2rhs= outA |> Array.map (fun str-> List.head str.Value.inps)//[added]

    let wirRhsA = wiresA |> Array.map (fun wirEl-> rhs2veril wirEl.Value)//RHS of the assignments for signals
    let wireNameA= wiresA |>Array.map (fun el-> el.Key) // LHS of the assignments for signals

    let wNameA= Array.append wireNameA add2lhs //[added]
    let wRhsA=Array.append wirRhsA add2rhs //[added]

    let assignSignStrL =  (Array.map2 (fun lhs rhs  -> ("\t assign "+lhs+" = "+rhs+";")) wNameA wRhsA) |> 
                                                                    Array.toList  //combinatorial assigning for signals
    let errFlStr="\t assign fail = |ErrDet;"

   //process Definition- full strings list
    let cirFileL = List.fold List.append [] ( [redModFin; [wireErr]; wiresMemStrL;wiresStrL; assignSignStrL; [errFlStr];memBlockStrL;["endmodule"]])
    
    // TOP FILE GENERATION

    //header of TTR module with ctrBlock and circuit block instantiations
    let tmrInterd= modEmp |> List.append ["module topDTR("]  // has oriignal interface naming
    let topOutStrL=  (Array.toList outStrA) |> 
                                    List.map  (fun outName->"\t output "+outName) //outputs declaration
    let topInterfL =List.append inpStrL topOutStrL 
    let modTop= List.append tmrInterd  (commaFunc topInterfL)//["\t output fail"])//adding normal inputs and output
    let modFin= List.append modTop [", input userMode, output userFail);"] //final TOP TMR module interface

    //generate wires
    //let rollBackL= ["\t wire fetchA;"]           // heart-beat wire from Beat-Block to circuit
    let saveL=  ["\t wire modeS;"]      // control of the recovery procedure
    //let  readBuffL=  ["\t wire readBuff;"]           //input buffers
    //let substBuffL=  ["\t wire subst;"]           //output buffers

    let failL=  ["\t wire fail;"]           //just indicateor that error detected in Mem Blocks
    let numOutFail= (Array.length outStrA) - 1
    let ctrErrOutBuffL=["\t wire ["+numOutFail.ToString()+":0] failOuts;"]           //just indicateor that error detected at output buffers //"\t wire ["+topRange.ToString()+":0] ErrDet;"
    let ctrErrGLOBL=["\t wire failGL;"]           //just indicateor that error detected somewhere: memBlocks||outBuffs                        

    let assFGlob= ["\t assign failGL=(|failOuts)||fail;"]

    //generate input sequence
    let ioRen oldName= oldName; //"_N_"+oldName; // renaming function for input/output interface wires - when there is not IO buffers I don't rename

    let newInpA= inpStrA |> Array.map (fun name-> ioRen name)//clk is skipped  \\ new inp naming 
    let newOutA= outStrA |> Array.map (fun name-> ioRen name)// new out naming

    let seqRenA= Array.append newInpA newOutA // renamed inputs and outputs

    //let intIntBsA=  (Array.mapi2 (fun num oldName newName->"\t inpBlock iB"+num.ToString()+"(clk, "+oldName+", "+newName+", readBuff);") inpStrA newInpA) |> Array.toList
    //let outIntBsA=  (Array.mapi2 (fun num oldName newName->"\t outBlock oB"+num.ToString()+
     //                               "(clk, "+newName+", "+oldName+", save, rollBack, subst, failOuts["+num.ToString()+"]);") outStrA newOutA) |> Array.toList
   
    //instantiate redundant blocks - function
    let instansRedModStr interfSeqStr=
        let clkInterf= (Array.append [|"clk"|] interfSeqStr) |> Array.toList // interface with clk
        let interf = List.append  clkInterf ["modeS"; "fail"]
        let inrfStr=  List.fold (fun was add-> was+", "+add) (List.head interf) ( List.tail interf) 
        inrfStr

    let strStartF num= "\t Design redModule"+num.ToString()+ "(" // "Design redModule0( "
    let ttmrStrL= [strStartF 0+instansRedModStr seqRenA+" );"] //interface of Design block

    //instantiate beat-clock control block
    let ctrBlockStrL= ["\t ctr2DynT heartBeat(clk, RESET_G, 
                                                failGL,modeS,
                                                    userMode,userFail);"] // Heard-control block
    
    let fileTopL= List.fold List.append [] [modFin; // shapeau
                                            //rollBackL; //rollback
                                            saveL;
                                            //readBuffL;
                                            //substBuffL; 

                                            failL;
                                            ctrErrOutBuffL;
                                            ctrErrGLOBL;
                                            assFGlob;
                                            
                                            
                                           // intIntBsA;outIntBsA; // IO blocks
                                            ttmrStrL;ctrBlockStrL; // design and HeartBeat
                                            ["endmodule"]]
    [cirFileL; fileTopL]

