module cir2tmrS1
//used Modules
open System
open System.Collections.Generic
open System.Linq
open System.Text
open System.IO

open structBEN
open designGen


// Renaming function- retunrs inputs from voters
let v2cRename str=
        "frmV_"+str
// Renaming funation for outputs- now outputs can have a unique name
let outRename str=
        "out_"+str


// function to voter insertion to the circuit cirD after memory cells voterL
let cir2tmrS1F (cirD: cirDicT) voterL =    
    let cirOrigA=cirD.ToArray() //from dictionary to array
    
    let inpA= cirOrigA|> Array.filter (fun el->el.Value.sType=INP) //orig ins struct A.
    let outA= cirOrigA|> Array.filter (fun el->el.Value.sType=OUT)  // orig outs struct A.
    
    let inpStrA= inpA |> Array.map (fun el-> el.Key) // original ins
    let outStrA= outA |> Array.map (fun el-> el.Key) //original outs

    //CIRCUIT FILE GENERATION
    let inpStrL=  (List.append ["clk"] (Array.toList inpStrA))|>
                                    List.map  (fun inpName->  "\t input "+inpName) //inputs declaration
    let outStrL=  (Array.toList outStrA) |> 
                                    List.map  (fun outName->"\t output "+outName) //outputs declaration
    
    let casualInterf= List.append inpStrL outStrL  //inps+ outs
              
    // IOs for voting
    let voterA= List.toArray voterL
    //~
    let toVotStrL=  voterL |> List.map  (fun memName->  "\t output "+ memName) //outs declaration - inputs for voters
    let frmVotStrL= voterL |> List.map  (fun memName->  "\t input "+ v2cRename memName) //inputs declaration - taken outs of voters
            
    let interfVotL= List.append frmVotStrL toVotStrL  //inps+ outs
          
    let fullInterface = List.append casualInterf interfVotL |> commaFunc
    //Forming the interface of the circuit design
    let modEmp= []
    let modTop= modEmp |> List.append ["module Design("] 
    let interf= List.append modTop fullInterface //adding IO: casual+ coter interface
    let redModFin= List.append interf [");"] //final module interface -Part 1.1

    // MAIN part after interface
    let cirModD= new cirDicT() // new dictionary of the transformed circuit
   
    // if a component of rhs of the dictionary entry is in voterL then it's renamed to the voter output
    // in this ways all signal taken originally from memory cells will go through the voters and only voters outs are taken
    let votSubstit componL=
        List.map (fun compon -> if (List.exists (fun vot-> vot=compon) voterL) then v2cRename compon else compon ) componL
    //so all original circuit passes by through the aforementioned renaming procdure of the components of rhs
    cirOrigA|> Array.iter (fun cEl-> cirModD.Add(cEl.Key,
                                        {sType=cEl.Value.sType;
                                        opType=cEl.Value.opType;
                                        inps=votSubstit cEl.Value.inps})) //Substitution of protected mem with vot out in RHS
    
    let cirA=cirModD.ToArray() //from dictionary of the transformed circuit to array of the transfromed circuit

    let memCellA= cirA|> Array.filter (fun el-> el.Value.opType=DFF) //memory cells of the transformed circuit - 1 module
    let memCellStrL = (Array.toList memCellA) |> List.map (fun mem -> "\t reg "+ mem.Key+" ;" ) //mem cells declaration

    let wiresA  = cirA|> Array.filter (fun el-> ( ((el.Value.sType=MIDLL)&&(el.Value.opType<>DFF))||
                                                  (el.Value.opType=WIRE))
                                                )
    let wireWoOutsA = wiresA |> Array.filter (fun wire-> wire.Value.sType<>OUT) //wires but not outputs
    let wiresStrL = (Array.toList wireWoOutsA) |> List.map (fun wirName-> "\t wire "+wirName.Key+" ;") //wires declaration
    
    //we also have to assign renamed outputs(lhs) to some expression(rhs) [added]

    let add2lhs= outStrA//names of the renamed outputs - currently not defined 
        // the rhs expressions has to be taken from the transfromed circuit, since some components of rhs has been replaced
        //by the voters outputs (instead of direct memory output wires)
    let findNewRhs name= // function that takes the 1st input of the structure- needed for renamed outputs. since each renOut=voterOut||memOut depending on voter insertion
        let pair=  cirA|>Array.find (fun el-> el.Key=name)
        List.head pair.Value.inps
    let add2rhs= Array.map (fun name -> findNewRhs name)  add2lhs // rhs is transformed (renamed rhs)- now mem cells are substituted with voters outs
    //let add2rhs= outA |> Array.map (fun struc-> List.head struc.Value.inps)//the original output name- before the renaming

    let wirRhsA = wiresA |> Array.map (fun wirEl-> rhs2veril wirEl.Value) //rhs of eq. for signal wires assignments
    let wireNameA= wiresA |>Array.map (fun el-> el.Key) // lhs- name of the signal wires

    let wNameA= Array.append wireNameA add2lhs //[added]
    let wRhsA=Array.append wirRhsA add2rhs //[added]

    let assignL =  (Array.map2 (fun lhs rhs  -> ("\t assign "+lhs+" = "+rhs+";")) wNameA wRhsA) |>  //[added]
                    Array.toList  //combinatorial assigning
   //process Definition
    let procEmp=[] //empty process string lst - to be filled 

    let procTop= ["\t always @(posedge clk)";"\t begin"] |> List.append procEmp //shapeau
    let memAssigL= (Array.toList memCellA) |> List.map (fun el-> "\t\t\t"+el.Key+" <= "+(rhs2veril el.Value)+" ;") //mem cells definitions

    let procAll= List.append (memAssigL|> List.append procTop) ["\t end";"endmodule"] // process the whole- mem writing

    let fileRedModL = List.fold List.append [] ( [redModFin; //circuit interface
                                                    memCellStrL; // mem cells declarations
                                                    wiresStrL; // wires declarations
                                                    assignL;//
                                                    procAll// proccess insertion with mem definitions
                                                    ])
    //TOP TMR 
    //module with triplicated redundant modules and voter instances

    let tmrInterd= modEmp |> List.append ["module topTMR("]  // has oriignal interface naming
    let inpClkL=["\t input clk"]
    let inpStrL'= (Array.toList inpStrA) |> List.map (fun name-> "\t input[2:0] "+name)
    let inpStrL= List.append inpClkL inpStrL'
    let outStrL= (Array.toList outStrA) |> List.map (fun name-> "\t output[2:0] "+name)
    let ioStrL= List.append inpStrL outStrL // all input/output interdace spec

    let modTop= List.append tmrInterd  (commaFunc ioStrL)//["\t output fail"])//adding normal inputs and output
    let modFin= List.append modTop [");"] //final TOP TMR module interface
   
    //generate wires for voters
    let memOutWirL= List.map (fun vot-> "\t wire[2:0]"+vot+";") voterL // inputs for voters and outs of the circuit
    let votOutNameL= List.map (fun vot->v2cRename vot) voterL // outs of voters and inputs for circuit back
    let votOutWirL= List.map (fun vot-> "\t wire[2:0]"+v2cRename vot+";") voterL // the corresponding str,see 1 line above
        //snake-TMR voters
    let detectWirL = [" \t wire ["+ (voterL.Count()-1).ToString() + ":0] detectWire;"]
    let detectStrL = List.mapi ( fun count smth -> "detectWire ["+ (count).ToString() + "]") voterL 

    let detectRegL =  [" \t wire failWire ; \n
                        \t assign failWire = &detectWire;\n
                        \t reg failReg; \n
                        \t always @ (posedge clk) \n
                        \t\t begin\n
                        \t\t\t failReg <= failWire ; \n
                        \t\t end \n"]

    //generate input sequence
    let oldSeqInA= inpStrA //clk is skipped at the beginning-> add later
    let oldSeqA= Array.append oldSeqInA outStrA
    let newSeqA = Array.append  (List.toArray votOutNameL) (List.toArray voterL) 
    let seqA=Array.append oldSeqA newSeqA // interface to the redundant block: inps,outs; votOuts(cirInps), votInps(cirOuts)

    let addModNum num sigVecName= //funstion that add index num to the signal vector strA
        Array.map (fun str-> str+"["+num+"]") sigVecName
    
    //func instantiate redundant blocks
    let instansRedModStr numInt interfSeqStr=
        let num= numInt.ToString() // number to str
        let indexedSignals= addModNum num interfSeqStr
        let enumInterfL=(Array.append [|"clk"|] indexedSignals) |> Array.toList
        let inrfStr=  List.fold (fun was add-> was+", "+add) (List.head enumInterfL) ( List.tail enumInterfL) 
        inrfStr
    let strStartF num= "\t Design redModule"+num.ToString()+ "(" // func shapeau of redund block instantiation
    let tmrStrL= List.map (fun num->strStartF num+instansRedModStr num seqA+" );") [0..2] //instantiation of 3 redundant modules with wires for voting in air

    //instantiate voters
    
    //let votStrL=List.map3 (fun inp out name-> "\t voterS1D "+"vot_"+name+"("+ +", failReg,"+out+","+inp+");") voterL votOutNameL voterL
        
    let votStrL_1=List.map2 (fun inp detec -> "\t voterS1D "+"vot_"+inp+"("+ detec) voterL detectStrL
    let votStrL_2=List.map2 (fun inp out -> ", failReg,"+out+","+inp+");") voterL votOutNameL
    let votStrL= List.map2  (fun a b -> a+b) votStrL_1 votStrL_2

    let fileTopL= List.fold List.append [] [modFin;votOutWirL;memOutWirL; detectWirL; detectRegL; tmrStrL;votStrL;["endmodule"]]
    //
    [fileRedModL; fileTopL]


