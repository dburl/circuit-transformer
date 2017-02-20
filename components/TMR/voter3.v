//Triple Voter Modle- manuel
 module voter3(out, i)/* synthesis syn_sharing = "off" */; // voter
 	output [2:0]out/*synthesis syn_keep = 1*/;
	input  [2:0]i/*synthesis syn_keep = 1*/;
		
	voter1 vA (out[0],i[0],i[1],i[2]) /*synthesis syn_preserve = 1*/;
	voter1 vB (out[1],i[1],i[2],i[0]) /*synthesis syn_preserve = 1*/;
	voter1 vC (out[2],i[2],i[0],i[1]) /*synthesis syn_preserve = 1*/;

endmodule