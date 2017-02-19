module cir2verilog
//used Modules
open System
open System.Collections.Generic
open System.Linq
open System.Text
open System.IO

open structBEN
open designGen

let cir2verilog (cirD: cirDicT) =
    let cirA=cirD.ToArray() //from dictionary to array
    
    let inpA= cirA|> Array.filter (fun el->el.Value.sType=INP)  //orig ins struct A.
    let outA= cirA|> Array.filter (fun el->el.Value.sType=OUT) // orig outs struct A.
    
    let inpStrA= inpA |> Array.map (fun el-> el.Key) // original ins
    let outStrA= outA |> Array.map (fun el-> el.Key)  //original outs
    //CIRCUIT FILE GENERATION
    let inpStrL=  (List.append ["clk"] (Array.toList inpStrA))|>
                                    List.map  (fun inpName->  "\t input "+inpName) //inputs declaration
    let outStrL=  (Array.toList outStrA) |> 
                                    List.map  (fun outName->"\t output "+outName) //outputs declaration

    let casualInterf= List.append inpStrL outStrL  //inps+ outs
              
    let fullInterface =  commaFunc casualInterf
     //Forming the interface of the circuit design
    let modEmp= []
    let modTop= modEmp |> List.append ["module Design("] 
    let interf= List.append modTop fullInterface //adding IO: casual+ coter interface
    let modFin= List.append interf [");"] //final module interface -Part 1.1
           
    // MAIN part after interface
    let memCellA= cirA|> Array.filter (fun el-> el.Value.opType=DFF)//memory cells
    let memCellStrL = (Array.toList memCellA) |> List.map (fun mem -> "\t reg "+ mem.Key+" ;" ) //mem cells declaration

    let wiresA  = cirA|> Array.filter (fun el-> ((el.Value.sType=MIDLL)&&(el.Value.opType<>DFF))||
                                                (el.Value.opType=WIRE))
    let wiresStrL = (Array.toList wiresA) |> List.map (fun wirName-> "\t wire "+wirName.Key+" ;") //wires declaration
    
    //we also have to assign renamed outputs - but in the original circuit we assign them to old outs
    let add2lhs= outStrA// lhs - renamed outs
    let add2rhs= outA |> Array.map (fun str-> List.head str.Value.inps)// rhs- original/not-tenamed outs

    let wirRhsA = wiresA |> Array.map (fun wirEl-> rhs2veril wirEl.Value)
    let wireNameA= wiresA |>Array.map (fun el-> el.Key)

    let wNameA= Array.append wireNameA add2lhs //
    let wRhsA=Array.append wirRhsA add2rhs //

    //so we create assign list for outs and for wires definitions
    let assignL =  (Array.map2 (fun lhs rhs  -> ("\t assign "+lhs+" = "+rhs+";")) wNameA wRhsA) |> Array.toList  //combinatorial assigning

   //process Definition
    let procEmp=[]

    let procTop= ["\t always @(posedge clk)";"\t begin"] |> List.append procEmp
    let memAssigL= (Array.toList memCellA) |> List.map (fun el-> "\t\t\t"+el.Key+" <= "+(rhs2veril el.Value)+" ;")

    let procAll= List.append (memAssigL|> List.append procTop) ["\t end";"endmodule"] // process the whole- mem writing

    let cirAllL = List.fold List.append [] ( [modFin; memCellStrL; wiresStrL; assignL;procAll])
    
    //TOP TMR module with triplicated redundant modules and voter instances
    let tmrInterd= modEmp |> List.append ["module topTMR("]  // has oriignal interface naming
    let inpClkL=["\t input clk"]
    let inpStrL'= (Array.toList inpStrA) |> List.map (fun name-> "\t input "+name)
    let inpStrL= List.append inpClkL inpStrL'
    let outStrL= (Array.toList outStrA) |> List.map (fun name-> "\t output "+name)
    let ioStrL= List.append inpStrL outStrL // all input/output interdace spec

    let modTop= List.append tmrInterd  (commaFunc ioStrL)//["\t output fail"])//adding normal inputs and output
    let modFin= List.append modTop [");"] //final TOP TMR module interface

    //generate wires
    //generate input sequence
    let oldSeqInA= inpStrA //clk is skipped at the beginning-> add later
    let oldSeqA= Array.append oldSeqInA outStrA
   
    let addModNum num strA= // can create only single-redundancy
        Array.map (fun str-> str) strA//+"["+num+"]") strA
    
    //instantiate redundant blocks
    let instansRedModStr numInt interfSeqStr=
        let num= numInt.ToString()
        let enumInterf=addModNum num (Array.append [|"clk"|] interfSeqStr) |> Array.toList
        let inrfStr=  List.fold (fun was add-> was+", "+add) (List.head enumInterf) ( List.tail enumInterf) 
        inrfStr
    let strStartF num= "\t Design redModule"+num.ToString()+ "("
    let tmrStrL= List.map (fun num->strStartF num+instansRedModStr num oldSeqA+" );") [0..0] //instantiation of 3 redundant modules with wires for voting in air  

    let fileTopL= List.fold List.append [] [modFin;tmrStrL;["endmodule"]]
    //
    [cirAllL; fileTopL]