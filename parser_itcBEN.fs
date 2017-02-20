module parser_itcBEN
(* Parsing of the .banch benchmark from Italy ITC'99*)
//Used modeules and NS
open System.IO
open System.Text.RegularExpressions



//DEFINITIONS
(*read_file takes the name of the file to read it and returns the string list of the files lines*)
let read_file filename = List.ofArray (File.ReadAllLines (filename)) 
(*Record gate_inf- record type which fully describes each gate used in the circuit*)
type gate_inf=
    {name: string;
    op: string;
    ins: string list;
    out: string;}

// Single line Parsing 
let recog_L line=
    //let line="OUTPUT(OVERFLW_REG)";; //[B] delete later
    let rexp_Iitc= "INPUT\((.*)\).*" 
    let rexp_Oitc= "OUTPUT\((.*)\).*"
    let rexp_CIRitc= "(.*)\s\=\s(.*)\((.*)\).*"
    let rexTL= [(rexp_Iitc,0);(rexp_Oitc,1);(rexp_CIRitc,2)] 

    let line_type= List.map (fun (regExp, num) -> (Regex.Match (line,regExp) ,num)) rexTL 
    let corr_regext= line_type |> List.filter (fun (mat,num)-> mat.Success) 

    let extr_ins (matEl: Match)=
        let fst_comp=   matEl.Groups.Item(1).ToString()
        let snd_comp=   matEl.Groups.Item(2).ToString()
        let thrd_comp=  matEl.Groups.Item(3).ToString()
        let inps_dirt=  Regex.Split(thrd_comp.ToString(), "[,]+") 
        let inps=       Array.map (fun str-> String.replace " " "" str) (inps_dirt)
        let ret= List.append [fst_comp;snd_comp] (List.ofArray inps)
        ret

    let final=
        if ((List.length corr_regext)>0) then
                if corr_regext.Length>0 then 
                    let apply_rexp = corr_regext.Head
                    let matEl= apply_rexp|> fun (a,b)->a
                    match corr_regext.Head with 
                    |(x,0) -> (0,[x.Groups.Item(1).ToString()] )(*INPUT wire*)
                    |(x,1) -> (1,[x.Groups.Item(1).ToString()] )(*OUTPUT wire*)
                    |(x,2) -> (2, extr_ins matEl) (*de-composed equation*)
                    |(_,_) -> (-1,["ERROR in matching of ITC'99"])
                else (3,[""])
        else (-2,[""])
    final
// Whole file parsing : line by line
let benITC_gRECL filename= 
    let lines=read_file filename
    let pair_parced= List.map (fun line-> recog_L line) lines 
    let inpsPL = List.filter(fun pair-> (fst pair)=0) pair_parced 
    let inpL= List.concat (List.map(fun pair-> snd pair) inpsPL )
    let outsPL= List.filter (fun pair-> (fst pair)=1) pair_parced 
    let outL= List.concat (List.map(fun pair-> snd pair) outsPL )
    let cirPL=List.filter(fun pair-> (fst pair)=2) pair_parced 
    let cirL=(List.map(fun pair-> snd pair) cirPL )
    let lstTOgateinf cir=
        {name=(List.nth  cir 0); 
        op=List.nth cir 1;
        ins=(List.tail (List.tail cir));
        out=(List.nth  cir 0);}
    let gate_recL= List.map (lstTOgateinf) cirL
    (gate_recL, (inpL,outL));;



(* *********************** SUPPORTING - used for debugging *)
(* let filename= "b01_opt_r.bench";; *)
(* let line="# 2 outputs";; *)
(* let line="# 41 gates (1 and, 29 nand, 2 or, 8 not)\n";; *)
(* let line="INPUT(SI_30_)\n";; *)
(* let line="OUTPUT(OVERFLW_REG)" *)
(* 
let line="U72 = AND(nRESET_G, STATO_REG_1_, U76, STATO_REG_0_)";;
   
let rexp_CIRitc= "\\(.*\\) = \\(.*\\)(\\(.*\\)).*" 

let rexp_CIRitc= "(?<left>[\d]+)\=(?<right>[\d]+)"

let rexp_CIRitc= "(\w)\s\=\s(\w)\(.*\)"

let line="U72 = ";;
let rexp_CIRitc= "(.*)\s\=\s(.*)\((.*)\).*"
let A=Regex.Match (line,rexp_CIRitc)
let A=Regex.Match (line,rexp_Iitc)

A.Groups.Item(2).ToString()

let line="nRESET_G, STATO_REG_1_, U76, STATO_REG_0_";;
*)