// Single Voter Module
module voterF(out,eq, a, b, c); // voter
	output  out;
	output  eq;
	input a, b, c;
	//wire eq/*synthesis syn_keep = 1*/;
	assign eq=(a== b);
	assign out = eq? a :c;
endmodule