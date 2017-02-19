module graphBuild
//Used Modules
open QuickGraph
open structBEN

// Extract the name list for verices from cirDescription
let cirD2vL (cirD: cirDicT)=
    let mutable cEnum=cirD.GetEnumerator()
    let rec addV oldVerL=
        let keyPair = cEnum.Current
        let newVerL= List.append oldVerL [keyPair.Key]
        let stopCond= cEnum.MoveNext()
        if stopCond then 
            addV newVerL 
        else
        newVerL
    cEnum.Dispose()
    let empL= List<string>.Empty
    addV empL
//Introduce edges to the graph
 let create_edges (cirD: cirDicT) (graph:BidirectionalGraph<string, Edge<string>>)=
	  let signal_takers dest=
        let sourD= cirD.[=]
        let inpL= sourD.inps
        let sourL= List.map (fun inpName-> graph.) inpL

	    if List.exists (fun v -> rec_s.out=v) rec_t.ins then 
	      let s= List.find (fun v-> G.V.label v=rec_s.name) v_list in
	      let t= List.find (fun v-> G.V.label v=rec_t.name) v_list in
	      graph.add_edge_e graph (G.E.create s 1 t)
            in
	let signal_source rec_s=
	  List.iter (signal_takers rec_s graph) gate_rec_list 
	in
      List.iter signal_source gate_rec_list
    in

//DEFINITIONS
let build_graph filename =
    let cirD=cirBuild filename
    let graph= new BidirectionalGraph<string, Edge<string>>()
    //Vertex creation
    let vNameL=  cirD2vL cirD 
    List.iter (fun vName -> ignore(graph.AddVertex(vName))) vNameL
    //Edges introduction

    
	  in
	let signal_source rec_s=
	  List.iter (signal_takers rec_s graph) gate_rec_list 
	in
      List.iter signal_source gate_rec_list
    in
  List.iter (G.add_vertex graph) v_list;
  create_edges gate_rec_list graph;
  graph;;


//let export_graph graph filename_graph=
//   let file = open_out_bin filename_graph in
//   Dot.output_graph file graph;;